using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Carbon.Extensions;
using Carbon.Generator;
using Carbon.Pooling;
using Carbon.Test;
using HarmonyLib;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;

namespace Carbon.Base;

public class BaseHookable : Integrations.ITestable
{
	public class HookCachePool : Dictionary<uint, CachedHookInstance>
	{
		public void Reset()
		{
			foreach (CachedHook item in base.Values.SelectMany((CachedHookInstance value) => value.Hooks))
			{
				item.Reset();
			}
		}

		public void EnableDebugging(bool wants)
		{
			foreach (CachedHook item in base.Values.SelectMany((CachedHookInstance value) => value.Hooks))
			{
				item.EnableDebugging(wants);
			}
		}
	}

	public class CachedHookInstance
	{
		public CachedHook PrimaryHook;

		public List<CachedHook> Hooks;

		public bool IsValid()
		{
			if (Hooks != null)
			{
				return Hooks.Count > 0;
			}
			return false;
		}

		public void RefreshPrimary()
		{
			PrimaryHook = Hooks.OrderByDescending((CachedHook x) => x.Parameters.Length).FirstOrDefault();
		}
	}

	public class CachedHook
	{
		public string Name;

		public uint Id;

		public BaseHookable Hookable;

		public string HookableName;

		public MethodInfo Method;

		public Type[] Parameters;

		public ParameterInfo[] InfoParameters;

		public bool IsByRef;

		public bool IsAsync;

		public bool IsDebugged;

		public int Exceptions;

		public int LagSpikes;

		public int TimesFired;

		public TimeSpan HookTime;

		public double MemoryUsage;

		public void EnableDebugging(bool wants)
		{
			IsDebugged = wants;
		}

		public void Reset()
		{
			Exceptions = 0;
			LagSpikes = 0;
			TimesFired = 0;
			HookTime = default(TimeSpan);
			MemoryUsage = 0.0;
		}

		public void OnFired(BaseHookable hookable, TimeSpan hookTime, double memoryUsed)
		{
			hookable.TotalHookTime += hookTime;
			hookable.TotalMemoryUsed += memoryUsed;
			hookable.TotalHookFires++;
			HookTime += hookTime;
			MemoryUsage += memoryUsed;
			Interlocked.Increment(ref TimesFired);
			if (IsDebugged)
			{
				Logger.Log(string.Format(" {0}[{1}] fired on {2} {3} [{4:n0}|{5:0}ms|{6}]", new object[7]
				{
					Name,
					Id,
					HookableName,
					Hookable.ToPrettyString(),
					TimesFired,
					HookTime.TotalMilliseconds,
					MemoryUsage.Format(ByteEx.ByteTypes.Auto, shortName: true, "0.0", "{0}{1}").ToLower()
				}));
			}
		}

		public void OnLagSpike(BaseHookable hookable)
		{
			hookable.TotalHookLagSpikes++;
			Interlocked.Increment(ref LagSpikes);
		}

		public void OnException()
		{
			Interlocked.Increment(ref Exceptions);
		}

		public static CachedHook Make(string hookName, uint hookId, BaseHookable hookable, MethodInfo method)
		{
			ParameterInfo[] parameters = method.GetParameters();
			CachedHook cachedHook = new CachedHook();
			cachedHook.Name = hookName;
			cachedHook.Id = hookId;
			cachedHook.Hookable = hookable;
			cachedHook.HookableName = ((hookable is BaseModule) ? "module" : "plugin");
			cachedHook.Method = method;
			cachedHook.IsByRef = parameters.Any((ParameterInfo x) => x.ParameterType.IsByRef);
			cachedHook.IsAsync = method.ReturnType?.GetMethod("GetAwaiter") != null || method.GetCustomAttribute<AsyncStateMachineAttribute>() != null;
			cachedHook.Parameters = parameters.Select((ParameterInfo x) => x.ParameterType).ToArray();
			cachedHook.InfoParameters = parameters;
			return cachedHook;
		}
	}

	public List<uint> Hooks;

	public List<HookMethodAttribute> HookMethods;

	public List<PluginReferenceAttribute> PluginReferences;

	public HookCachePool HookPool = new HookCachePool();

	public HashSet<uint> IgnoredHooks = new HashSet<uint>();

	[JsonProperty]
	public TimeSpan TotalHookTime;

	[JsonProperty]
	public int TotalHookFires;

	[JsonProperty]
	public double TotalMemoryUsed;

	[JsonProperty]
	public int TotalHookLagSpikes;

	[JsonProperty]
	public int TotalHookExceptions;

	internal Stopwatch _trackStopwatch = new Stopwatch();

	internal int _currentGcCount;

	internal TimeSince? _initializationTime;

	private Dictionary<AutoPatchAttribute.Orders, Harmony> _harmonyInstanceCache = new Dictionary<AutoPatchAttribute.Orders, Harmony>();

	public virtual bool ManualSubscriptions => false;

	[JsonProperty]
	public string Name { get; set; }

	[JsonProperty]
	public virtual VersionNumber Version { get; set; }

	[JsonProperty]
	public double Uptime
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return TimeSince.op_Implicit(_initializationTime.GetValueOrDefault());
		}
	}

	public bool HasBuiltHookCache { get; internal set; }

	public bool HasInitialized { get; internal set; }

	public Type HookableType { get; internal set; }

	public bool InternalCallHookOverriden { get; internal set; } = true;

	public int TestCount { get; internal set; }

	public TimeSpan CurrentHookTime { get; internal set; }

	public static long CurrentMemory => GC.GetTotalMemory(forceFullCollection: false);

	public static int CurrentGcCount => GC.CollectionCount(0);

	public bool HasGCCollected => _currentGcCount != CurrentGcCount;

	protected string HarmonyId => GetHarmonyId();

	protected Harmony HarmonyInstance => GetHarmonyInstance(AutoPatchAttribute.Orders.AfterPluginInit, createIfNotExists: true);

	public virtual void TrackInit()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!_initializationTime.HasValue)
		{
			_initializationTime = TimeSince.op_Implicit(0f);
		}
	}

	public virtual void TrackStart()
	{
		if (Community.IsServerInitialized && !_trackStopwatch.IsRunning)
		{
			_trackStopwatch.Start();
			_currentGcCount = CurrentGcCount;
		}
	}

	public virtual void TrackEnd()
	{
		if (Community.IsServerInitialized && _trackStopwatch.IsRunning)
		{
			CurrentHookTime = _trackStopwatch.Elapsed;
			_trackStopwatch.Reset();
		}
	}

	protected void OnException(uint hook)
	{
		Interlocked.Increment(ref TotalHookExceptions);
		List<CachedHook> hooks = HookPool[hook].Hooks;
		foreach (CachedHook item in hooks)
		{
			item.OnException();
		}
	}

	protected string GetHarmonyId(AutoPatchAttribute.Orders order = AutoPatchAttribute.Orders.AfterPluginInit)
	{
		return $"com.carbon.{Name}.{order}";
	}

	protected Harmony GetHarmonyInstance(AutoPatchAttribute.Orders order, bool createIfNotExists = false)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0031: Expected O, but got Unknown
		if (_harmonyInstanceCache.TryGetValue(order, out var value))
		{
			return value;
		}
		if (!createIfNotExists)
		{
			return null;
		}
		Dictionary<AutoPatchAttribute.Orders, Harmony> harmonyInstanceCache = _harmonyInstanceCache;
		Harmony val = new Harmony(GetHarmonyId(order));
		Harmony result = val;
		harmonyInstanceCache[order] = val;
		return result;
	}

	protected int RemoveHarmonyInstance(AutoPatchAttribute.Orders order, string category = null)
	{
		if (_harmonyInstanceCache.TryGetValue(order, out var value))
		{
			int? num = value.GetPatchedMethods()?.Count();
			if (num > 0)
			{
				if (!string.IsNullOrEmpty(category))
				{
					value.UnpatchCategory(category);
				}
				else
				{
					value.UnpatchAll(GetHarmonyId(order));
				}
			}
			_harmonyInstanceCache.Remove(order);
			return num.GetValueOrDefault();
		}
		return 0;
	}

	public bool ApplyOrderedPatches(AutoPatchAttribute.Orders order, string category = null)
	{
		Harmony harmonyInstance = GetHarmonyInstance(order);
		if (harmonyInstance != null)
		{
			return true;
		}
		harmonyInstance = GetHarmonyInstance(order, createIfNotExists: true);
		Type[] nestedTypes = HookableType.GetNestedTypes(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		Type[] array = nestedTypes;
		foreach (Type type in array)
		{
			AutoPatchAttribute customAttribute = type.GetCustomAttribute<AutoPatchAttribute>(inherit: false);
			if (customAttribute == null || customAttribute.Order != order)
			{
				continue;
			}
			HarmonyPatchCategory customAttribute2 = ((MemberInfo)type).GetCustomAttribute<HarmonyPatchCategory>();
			if (customAttribute2 != null && ((HarmonyAttribute)customAttribute2).info != null && ((HarmonyAttribute)customAttribute2).info.category != category)
			{
				continue;
			}
			try
			{
				PatchClassProcessor obj = harmonyInstance.CreateClassProcessor(type);
				List<MethodInfo> list = ((obj != null) ? obj.Patch() : null);
				if (list == null || list.Count == 0)
				{
					if (!customAttribute.Silent)
					{
						Logger.Warn($"[{Name}] AutoPatch attribute found on '{type.Name}' but no HarmonyPatch methods found. Skipping.. [{order}]");
					}
					continue;
				}
				if (!customAttribute.Silent)
				{
					foreach (MethodInfo item in list)
					{
						Logger.Log(string.Format("[{0}] Automatically Harmony patched '{1}' ({2}) method [{3}].", new object[4] { Name, item.Name, type.Name, order }));
					}
				}
				if (string.IsNullOrEmpty(customAttribute.PatchSuccessCallback))
				{
					continue;
				}
				MethodInfo method = HookableType.GetMethod(customAttribute.PatchSuccessCallback);
				if (method != null)
				{
					try
					{
						method.Invoke(this, Array.Empty<object>());
					}
					catch (Exception ex)
					{
						Logger.Error(string.Format("[{0}] Failed to execute successful automatic Harmony patch callback '{1}': {2} [{3}]", new object[4] { Name, type.Name, customAttribute.PatchSuccessCallback, order }), ex);
					}
				}
			}
			catch (Exception ex2)
			{
				if (!string.IsNullOrEmpty(customAttribute.PatchFailureCallback))
				{
					MethodInfo method2 = HookableType.GetMethod(customAttribute.PatchFailureCallback);
					if (method2 != null)
					{
						try
						{
							method2.Invoke(this, Array.Empty<object>());
						}
						catch (Exception ex3)
						{
							Logger.Error(string.Format("[{0}] Failed to execute successful automatic Harmony patch callback '{1}': {2} [{3}]", new object[4] { Name, type.Name, customAttribute.PatchSuccessCallback, order }), ex3);
						}
					}
				}
				Logger.Error($"[{Name}] Failed to automatically Harmony patch '{type.Name}' [{order}]", ex2);
				if (customAttribute.IsRequired)
				{
					return false;
				}
			}
		}
		return true;
	}

	public void UnapplyOrderedPatches(AutoPatchAttribute.Orders order, bool silent = true, string category = null)
	{
		try
		{
			int num = RemoveHarmonyInstance(order, category);
			if (!silent && num > 0)
			{
				Logger.Log(string.Format("[{0}] Automatically Harmony unpatched {1:n0} {2} [{3}].", new object[4]
				{
					Name,
					num,
					num.Plural("method", "methods"),
					order
				}));
			}
		}
		catch (Exception ex)
		{
			Logger.Error($"[{Name}] Failed auto unpatching {GetHarmonyId(order)} [{order}]", ex);
		}
	}

	public bool ApplyDelayedPatches(string category = null)
	{
		return ApplyOrderedPatches(AutoPatchAttribute.Orders.Delayed, category);
	}

	public void UnapplyDelayedPatches(string category = null, bool silent = true)
	{
		UnapplyOrderedPatches(AutoPatchAttribute.Orders.Delayed, silent, category);
	}

	public virtual void CollectTests(int channel = 1)
	{
		TestCount = 0;
		Integrations.TestBank testBank = Integrations.Get(HookableType.Name, HookableType, this, channel);
		if (testBank != null)
		{
			TestCount++;
			Integrations.EnqueueBed(testBank);
		}
		Type[] nestedTypes = HookableType.GetNestedTypes(BindingFlags.Public);
		foreach (Type type in nestedTypes)
		{
			Integrations.TestBank testBank2 = Integrations.Get(type.Name, type, null, channel);
			if (testBank2 != null)
			{
				TestCount++;
				Integrations.EnqueueBed(testBank2);
			}
		}
	}

	public virtual async ValueTask OnAsyncServerShutdown()
	{
		await Task.CompletedTask;
	}

	public void BuildHookCache(BindingFlags flag)
	{
		if (HasBuiltHookCache)
		{
			return;
		}
		bool flag2 = Hooks.Count != 0;
		HookPool.Clear();
		MethodInfo[] methods = HookableType.GetMethods(flag);
		MethodInfo[] array = methods;
		foreach (MethodInfo methodInfo in array)
		{
			if (!Carbon.Generator.InternalCallHook.HasRefLikeSignature(methodInfo))
			{
				uint orAdd = HookStringPool.GetOrAdd(methodInfo.Name);
				if (!flag2 && Community.Runtime.HookManager.IsHook(methodInfo.Name) && !Hooks.Contains(orAdd))
				{
					Hooks.Add(orAdd);
				}
				if (!HookPool.TryGetValue(orAdd, out var value))
				{
					value = new CachedHookInstance();
					value.Hooks = new List<CachedHook>(5);
					HookPool.Add(orAdd, value);
				}
				value.Hooks.Add(CachedHook.Make(methodInfo.Name, orAdd, this, methodInfo));
				value.RefreshPrimary();
				InternalHooks.Handle(methodInfo.Name, subscribed: true);
			}
		}
		MethodInfo[] methods2 = HookableType.GetMethods(flag | BindingFlags.Public);
		MethodInfo[] array2 = methods2;
		foreach (MethodInfo method in array2)
		{
			if (Carbon.Generator.InternalCallHook.HasRefLikeSignature(method))
			{
				continue;
			}
			HookMethodAttribute customAttribute = method.GetCustomAttribute<HookMethodAttribute>();
			if (customAttribute != null)
			{
				uint orAdd2 = HookStringPool.GetOrAdd(string.IsNullOrEmpty(customAttribute.Name) ? method.Name : customAttribute.Name);
				if (!HookPool.TryGetValue(orAdd2, out var value2))
				{
					value2 = new CachedHookInstance();
					value2.Hooks = new List<CachedHook>(5);
					HookPool.Add(orAdd2, value2);
				}
				if (!value2.Hooks.Any((CachedHook x) => x.Method == method))
				{
					value2.Hooks.Add(CachedHook.Make(method.Name, orAdd2, this, method));
					value2.RefreshPrimary();
				}
			}
		}
		HasBuiltHookCache = true;
		InternalCallHook(0u, null);
	}

	public virtual object InternalCallHook(uint hook, object[] args)
	{
		InternalCallHookOverriden = false;
		return null;
	}

	public void Subscribe(string hook)
	{
		if (IgnoredHooks != null)
		{
			uint orAdd = HookStringPool.GetOrAdd(hook);
			if (IgnoredHooks.Contains(orAdd))
			{
				Community.Runtime.HookManager.Subscribe(hook, Name);
				IgnoredHooks.Remove(orAdd);
			}
		}
	}

	public void Unsubscribe(string hook)
	{
		if (IgnoredHooks != null)
		{
			uint orAdd = HookStringPool.GetOrAdd(hook);
			if (!IgnoredHooks.Contains(orAdd))
			{
				Community.Runtime.HookManager.Unsubscribe(hook, Name);
				IgnoredHooks.Add(orAdd);
			}
		}
	}

	public bool IsHookIgnored(uint hook)
	{
		if (IgnoredHooks != null)
		{
			return IgnoredHooks.Contains(hook);
		}
		return false;
	}

	public void SubscribeAll(Func<string, bool> condition = null)
	{
		foreach (string item in from hook in Hooks
			select HookStringPool.GetOrAdd(hook) into name
			where condition == null || condition(name)
			select name)
		{
			Subscribe(item);
		}
	}

	public void UnsubscribeAll(Func<string, bool> condition = null)
	{
		foreach (string item in from hook in Hooks
			select HookStringPool.GetOrAdd(hook) into name
			where condition == null || condition(name)
			select name)
		{
			Unsubscribe(item);
		}
	}

	public T To<T>()
	{
		if (this is T)
		{
			return (T)(object)((this is T) ? this : null);
		}
		return default(T);
	}

	public override string ToString()
	{
		return GetType().FullName;
	}

	public virtual string ToPrettyString()
	{
		return $"{Name} v{Version}";
	}
}

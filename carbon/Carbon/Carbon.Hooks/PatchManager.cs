using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using API.Abstracts;
using API.Events;
using API.Hooks;
using Carbon.Core;
using Carbon.Pooling;
using Oxide.Plugins;
using UnityEngine;

namespace Carbon.Hooks;

public sealed class PatchManager : CarbonBehaviour, IPatchManager, IDisposable
{
	private readonly int PatchLimitPerCycle = 25;

	private Stopwatch sw;

	private bool _doReload;

	private Queue<string> _workQueue;

	private List<Subscription> _subscribers;

	private readonly Dictionary<string, string> _checksums = new Dictionary<string, string>();

	private static bool FullFramePatch;

	private static bool InitialOnEnable;

	private static readonly string[] Files = new string[3] { "Carbon.Hooks.Base.dll", "Carbon.Hooks.Community.dll", "Carbon.Hooks.Oxide.dll" };

	private bool _disposing;

	internal List<HookEx> _patches { get; set; }

	internal List<HookEx> _staticHooks { get; set; }

	internal List<HookEx> _dynamicHooks { get; set; }

	internal List<HookEx> _metadataHooks { get; set; }

	internal List<HookEx> _installed { get; set; }

	private IEnumerable<HookEx> LoadedHooks => from x in _patches.Concat(_dynamicHooks).Concat(_staticHooks)
		where x.IsLoaded
		select x;

	private IEnumerable<HookEx> Hooks => _patches.Concat(_dynamicHooks).Concat(_staticHooks).Concat(_metadataHooks);

	public IEnumerable<IHook> LoadedPatches => _patches;

	public IEnumerable<IHook> InstalledPatches => _patches.Where((HookEx x) => x.IsInstalled);

	public IEnumerable<IHook> LoadedStaticHooks => _staticHooks;

	public IEnumerable<IHook> InstalledStaticHooks => _staticHooks.Where((HookEx x) => x.IsInstalled);

	public IEnumerable<IHook> LoadedDynamicHooks => _dynamicHooks;

	public IEnumerable<IHook> InstalledDynamicHooks => _dynamicHooks.Where((HookEx x) => x.IsInstalled);

	public void Enqueue(string identifier)
	{
		if (!_workQueue.Contains(identifier))
		{
			_workQueue.Enqueue(identifier);
		}
	}

	private void Awake()
	{
		if (!Community.Runtime.Config.Logging.ReducedLogging)
		{
			Logger.Log($"Initializing {this}..");
		}
		sw = new Stopwatch();
		_dynamicHooks = new List<HookEx>();
		_installed = new List<HookEx>();
		_patches = new List<HookEx>();
		_staticHooks = new List<HookEx>();
		_metadataHooks = new List<HookEx>();
		_subscribers = new List<Subscription>();
		_workQueue = new Queue<string>();
		((Behaviour)this).enabled = false;
		if (Community.Runtime.Config.SelfUpdating.HookUpdates)
		{
			if (!Community.Runtime.Config.Logging.ReducedLogging)
			{
				Logger.Log("Updating hooks...");
			}
			Updater.DoUpdate(delegate(bool result)
			{
				if (!result)
				{
					Logger.Error("Unable to update the hooks at this time, please try again later");
				}
				((Behaviour)this).enabled = true;
			});
		}
		else
		{
			Logger.Log("Hook updates disabled, loading from disk...");
			((FacepunchBehaviour)this).Invoke((Action)delegate
			{
				((Behaviour)this).enabled = true;
			}, 0.1f);
		}
	}

	private void OnEnable()
	{
		_patches.Clear();
		_staticHooks.Clear();
		_dynamicHooks.Clear();
		string[] files = Files;
		foreach (string filePath in files)
		{
			LoadHooksFromFile(filePath);
		}
		if (_patches.Count > 0)
		{
			for (int j = 0; j < _patches.Count; j++)
			{
				HookEx hookEx = _patches[j];
				if (!hookEx.IsInstalled && !hookEx.HasDependencies())
				{
					Subscribe(hookEx.Identifier, "Carbon.Patch");
				}
			}
		}
		if (_staticHooks.Count > 0)
		{
			for (int k = 0; k < _staticHooks.Count; k++)
			{
				HookEx hookEx2 = _staticHooks[k];
				if (!hookEx2.IsInstalled)
				{
					Subscribe(hookEx2.Identifier, "Carbon.Static");
				}
			}
			FullFramePatch = true;
			Update();
		}
		if (!InitialOnEnable)
		{
			InitialOnEnable = true;
			((FacepunchBehaviour)this).Invoke((Action)delegate
			{
				Community.Runtime.Events.Trigger(CarbonEvent.HooksInstalled, EventArgs.Empty);
			}, 1f);
		}
	}

	private void OnDisable()
	{
		if (!Community.Runtime.Config.Logging.ReducedLogging)
		{
			Logger.Log("Stopping hook processor...");
		}
		for (int i = 0; i < _installed.Count; i++)
		{
			HookEx hookEx = _installed[i];
			if (!hookEx.RemovePatch())
			{
				Logger.Warn(" Failed uninstalling patch: " + hookEx.HookFullName + "[" + hookEx.Identifier + "]");
			}
			else
			{
				_installed.RemoveAt(i);
				i--;
			}
		}
		if (_doReload)
		{
			if (!Community.Runtime.Config.Logging.ReducedLogging)
			{
				Logger.Log("Reloading hook processor...");
			}
			_doReload = false;
			((Behaviour)this).enabled = true;
		}
	}

	public void Fetch()
	{
		Community.Runtime.Events.Trigger(CarbonEvent.HookFetchStart, EventArgs.Empty);
		foreach (ModLoader.Package package in ModLoader.Packages)
		{
			foreach (RustPlugin plugin in package.Plugins)
			{
				foreach (uint hook in plugin.Hooks)
				{
					Unsubscribe(HookStringPool.GetOrAdd(hook), plugin.FileName);
				}
			}
		}
		try
		{
			OnDisable();
		}
		catch (Exception ex)
		{
			Logger.Error("Reinstall failed: OnDisable failed", ex);
			return;
		}
		Logger.Warn(" Re-downloading hooks...");
		Updater.DoUpdate(delegate(bool result)
		{
			if (!result)
			{
				Logger.Error("Unable to update the hooks at this time, please try again later");
			}
			else
			{
				OnEnable();
				foreach (ModLoader.Package package2 in ModLoader.Packages)
				{
					foreach (RustPlugin plugin2 in package2.Plugins)
					{
						foreach (uint hook2 in plugin2.Hooks)
						{
							string orAdd = HookStringPool.GetOrAdd(hook2);
							if (!plugin2.IsHookIgnored(hook2))
							{
								Subscribe(orAdd, plugin2.FileName);
							}
						}
					}
				}
				Community.Runtime.Events.Trigger(CarbonEvent.HookFetchEnd, EventArgs.Empty);
			}
		});
	}

	private void OnDestroy()
	{
		if (!Community.Runtime.Config.Logging.ReducedLogging)
		{
			Logger.Log("Destroying hook processor...");
		}
		Dispose();
	}

	private void Update()
	{
		if (_workQueue.Count == 0)
		{
			return;
		}
		int num = (FullFramePatch ? int.MaxValue : PatchLimitPerCycle);
		while (_workQueue.Count > 0 && num-- > 0)
		{
			string text = _workQueue.Dequeue();
			try
			{
				HookEx hookById = GetHookById(text);
				bool flag = HookHasSubscribers(hookById.Identifier);
				bool isInstalled = hookById.IsInstalled;
				string text2 = null;
				if (!isInstalled)
				{
					if (hookById.Status == HookState.Failure)
					{
						Logger.Warn($"A hook request for '{hookById}' received:");
						Logger.Warn(" - The current status is FAILURE: " + hookById.LastError);
						Logger.Warn(" - Check for possible errors on the log file");
					}
					text2 = GetMethodMSILHash(hookById.GetTargetMethodInfo());
				}
				if (flag)
				{
					if (!isInstalled)
					{
						if (!hookById.ApplyPatch())
						{
							return;
						}
						_installed.Add(hookById);
					}
				}
				else
				{
					if (!isInstalled)
					{
						continue;
					}
					if (!hookById.RemovePatch())
					{
						return;
					}
					_installed.Remove(hookById);
					if (!hookById.HasDependencies())
					{
						continue;
					}
					for (int i = 0; i < hookById.Dependencies.Length; i++)
					{
						IEnumerable<HookEx> hookByName = GetHookByName(hookById.Dependencies[i]);
						foreach (HookEx item in hookByName)
						{
							if (item != null && item.IsInstalled)
							{
								item.RemovePatch();
							}
						}
					}
					continue;
				}
			}
			catch (ApplicationException ex)
			{
				Logger.Error(ex.Message);
			}
			catch (Exception ex2)
			{
				Logger.Error("HookManager.Update() failed at '" + text + "'", ex2);
			}
		}
		if (FullFramePatch)
		{
			FullFramePatch = false;
			Community.Runtime.Events.Trigger(CarbonEvent.HooksPatchedFullFrame, EventArgs.Empty);
		}
	}

	private void LoadHooksFromFile(string filePath)
	{
		try
		{
			Assembly assembly = Community.Runtime.AssemblyEx.Hooks.Load(filePath, "HookManager.LoadHooksFromFile");
			if (assembly == null)
			{
				Logger.Error("Error while loading hooks from '" + filePath + "'.");
				Logger.Error("Either the file is corrupt or has an unsupported format/version.");
				return;
			}
			Type @base = assembly.GetType("API.Hooks.Patch") ?? typeof(Patch);
			Type attr = assembly.GetType("API.Hooks.HookAttribute.Patch") ?? typeof(HookAttribute.Patch);
			IEnumerable<TypeInfo> types = assembly.DefinedTypes.Where((TypeInfo typeInfo) => @base.IsAssignableFrom(typeInfo) && Attribute.IsDefined(typeInfo, attr));
			TaskStatus taskStatus = LoadHooks(types);
			if (taskStatus.Total == 0)
			{
				return;
			}
			string fileName = Path.GetFileName(filePath);
			if (!Community.Runtime.Config.Logging.ReducedLogging)
			{
				Logger.Log(string.Format("- Loaded {0} hooks ({1}/{2}/{3}) from file '{4}' in {5}ms", new object[6] { taskStatus.Total, taskStatus.Patch, taskStatus.Static, taskStatus.Dynamic, fileName, sw.ElapsedMilliseconds }));
			}
			Type type = assembly.GetType("Carbon.Hooks._Meta");
			if ((object)type != null)
			{
				string text = (string)type.GetField("Checksum").GetValue(null);
				string text2 = BitConverter.ToUInt32(new MD5CryptoServiceProvider().ComputeHash(File.ReadAllBytes(typeof(ServerMgr).Assembly.Location)), 0).ToString();
				if (!text.Equals(text2) && !Community.Runtime.Config.Logging.ReducedLogging)
				{
					Logger.Warn("Checksum validation failed for Rust has failed (" + text2 + " =/ " + text + ") - [" + fileName + "]");
				}
			}
		}
		catch (Exception)
		{
			Logger.Error("- Error while loading hooks from file '" + Path.GetFileName(filePath) + "'");
		}
	}

	public void LoadHooksFromType(Type type)
	{
		Type @base = typeof(Patch);
		Type attr = typeof(HookAttribute.Patch);
		IEnumerable<TypeInfo> types = from x in type.GetNestedTypes()
			where @base.IsAssignableFrom(x) && Attribute.IsDefined(x, attr)
			select x.GetTypeInfo();
		TaskStatus taskStatus = LoadHooks(types);
		if (taskStatus.Total != 0 && !Community.Runtime.Config.Logging.ReducedLogging)
		{
			Logger.Log(string.Format("- Loaded {0} hooks ({1}/{2}/{3})", new object[4] { taskStatus.Total, taskStatus.Patch, taskStatus.Static, taskStatus.Dynamic }) + $" from type '{type}' in {sw.ElapsedMilliseconds}ms");
		}
	}

	private TaskStatus LoadHooks(IEnumerable<TypeInfo> types)
	{
		TaskStatus result = new TaskStatus
		{
			Hooks = new List<HookEx>()
		};
		sw.Restart();
		foreach (TypeInfo type in types)
		{
			try
			{
				HookEx hookEx = new HookEx(type) ?? throw new Exception("Hook is null, this is a bug");
				if (hookEx.Options.HasFlag(HookFlags.MetadataOnly))
				{
					hookEx.SetStatus(HookState.Inactive);
					result.Metadata++;
					_metadataHooks.Add(hookEx);
					HookStringPool.GetOrAdd(hookEx.HookName);
					continue;
				}
				if (IsHookLoaded(hookEx))
				{
					AssemblyName name = type.Assembly.GetName();
					Logger.Warn($" Attempted to install duplicate hook '{hookEx}' (from {name.Name})");
					continue;
				}
				if (hookEx.IsPatch)
				{
					result.Patch++;
					_patches.Add(hookEx);
					if (!hookEx.HasDependencies())
					{
						Subscribe(hookEx.Identifier, "Carbon.Patch");
					}
				}
				else if (hookEx.IsStaticHook)
				{
					result.Static++;
					_staticHooks.Add(hookEx);
					Subscribe(hookEx.Identifier, "Carbon.Static");
				}
				else
				{
					result.Dynamic++;
					_dynamicHooks.Add(hookEx);
				}
				HookStringPool.GetOrAdd(hookEx.HookName);
				result.Hooks.Add(hookEx);
			}
			catch (Exception ex)
			{
				Logger.Error("Error while parsing '" + type.Name + "'", ex);
			}
		}
		sw.Stop();
		return result;
	}

	private IEnumerable<HookEx> GetHookDependencyTree(HookEx hook)
	{
		string[] dependencies = hook.Dependencies;
		foreach (string name in dependencies)
		{
			foreach (HookEx hookEx in GetHookByFullName(name))
			{
				foreach (HookEx item in GetHookDependencyTree(hookEx))
				{
					yield return item;
				}
				yield return hookEx;
			}
		}
	}

	private IEnumerable<HookEx> GetHookDependantTree(HookEx hook)
	{
		foreach (HookEx hookEx in _patches.Where((HookEx x) => Enumerable.Contains(x.Dependencies, hook.HookFullName)))
		{
			foreach (HookEx item in GetHookDependantTree(hookEx))
			{
				yield return item;
			}
			yield return hookEx;
		}
	}

	private IEnumerable<HookEx> GetHookByName(string name)
	{
		return LoadedHooks.Where((HookEx x) => x.HookName.Equals(name)) ?? null;
	}

	private IEnumerable<HookEx> GetHookByFullName(string name)
	{
		return LoadedHooks.Where((HookEx x) => x.HookFullName.Equals(name)) ?? null;
	}

	private HookEx GetHookById(string identifier)
	{
		return LoadedHooks.FirstOrDefault((HookEx x) => x.Identifier.Equals(identifier)) ?? null;
	}

	private IEnumerable<HookEx> GetHookByNameAll(string name)
	{
		return Hooks.Where((HookEx x) => x.HookName.Equals(name)) ?? null;
	}

	private IEnumerable<HookEx> GetHookByFullNameAll(string name)
	{
		return Hooks.Where((HookEx x) => x.HookFullName.Equals(name)) ?? null;
	}

	private HookEx GetHookByIdAll(string identifier)
	{
		return Hooks.FirstOrDefault((HookEx x) => x.Identifier.Equals(identifier)) ?? null;
	}

	internal bool IsHookLoaded(HookEx hook)
	{
		return LoadedHooks.Any((HookEx x) => x.HookFullName.Equals(hook.HookFullName) && x.TargetType == hook.TargetType && x.TargetMethod == hook.TargetMethod && (x.TargetMethodArgs?.SequenceEqual(hook.TargetMethodArgs) ?? true));
	}

	public bool IsHook(string hookName)
	{
		return GetHookByNameAll(hookName).Any();
	}

	public bool IsHookLoaded(string hookName)
	{
		return GetHookByName(hookName).Any(IsHookLoaded);
	}

	public void ForceUpdateHooks()
	{
		FullFramePatch = true;
		Update();
	}

	private bool HookIsSubscribedBy(string hookName, string subscriber)
	{
		return _subscribers?.Where((Subscription x) => x.Identifier.Equals(hookName)).Any((Subscription x) => x.Subscriber == subscriber) ?? false;
	}

	private bool HookHasSubscribers(string hookName)
	{
		return _subscribers?.Any((Subscription x) => x.Identifier.Equals(hookName)) ?? false;
	}

	public int GetHookSubscriberCount(string hookName)
	{
		return _subscribers.Count((Subscription x) => x.Identifier.Equals(hookName));
	}

	private void AddSubscriber(string hookName, string subscriber)
	{
		_subscribers.Add(new Subscription
		{
			Identifier = hookName,
			Subscriber = subscriber
		});
	}

	private void RemoveSubscriber(string hookName, string subscriber)
	{
		_subscribers.RemoveAll((Subscription x) => x.Identifier.Equals(hookName) && x.Subscriber.Equals(subscriber));
	}

	public void Subscribe(string hookName, string requester)
	{
		try
		{
			HookEx hookById = GetHookById(hookName);
			if (hookById != null && !HookIsSubscribedBy(hookById.Identifier, requester))
			{
				Subscribe(hookById, requester);
				return;
			}
			IEnumerable<HookEx> hookByName = GetHookByName(hookName);
			if (!hookByName.Any())
			{
				return;
			}
			foreach (HookEx item in hookByName.Where((HookEx hook) => !HookIsSubscribedBy(hook.Identifier, requester)))
			{
				Subscribe(item, requester);
			}
			hookByName = null;
		}
		catch (Exception ex)
		{
			Logger.Error("Error while subscribing hook '" + hookName + "'", ex);
		}
	}

	private void Subscribe(HookEx hook, string requester)
	{
		try
		{
			foreach (HookEx item in GetHookDependencyTree(hook))
			{
				AddSubscriber(item.Identifier, requester);
				Enqueue(item.Identifier);
			}
			AddSubscriber(hook.Identifier, requester);
			Enqueue(hook.Identifier);
			foreach (HookEx item2 in GetHookDependantTree(hook))
			{
				AddSubscriber(item2.Identifier, requester);
				Enqueue(item2.Identifier);
			}
		}
		catch (Exception ex)
		{
			Logger.Error($"Error while subscribing hook '{hook}'", ex);
		}
	}

	public IEnumerable<string> GetHookSubscribers(string hookName)
	{
		return from x in _subscribers
			where x.Identifier.Equals(hookName)
			select x.Subscriber;
	}

	public void Unsubscribe(string hookName, string requester)
	{
		try
		{
			HookEx hookById = GetHookById(hookName);
			if (hookById != null && !HookIsSubscribedBy(hookById.Identifier, requester))
			{
				Unsubscribe(hookById, requester);
				return;
			}
			IEnumerable<HookEx> hookByName = GetHookByName(hookName);
			if (!hookByName.Any())
			{
				return;
			}
			foreach (HookEx item in hookByName.Where((HookEx hook) => HookIsSubscribedBy(hook.Identifier, requester)).Reverse())
			{
				Unsubscribe(item, requester);
			}
		}
		catch (Exception ex)
		{
			Logger.Error("Error while unsubscribing hook '" + hookName + "'", ex);
		}
	}

	private void Unsubscribe(HookEx hook, string requester)
	{
		try
		{
			foreach (HookEx item in GetHookDependantTree(hook))
			{
				RemoveSubscriber(item.Identifier, requester);
				Enqueue(item.Identifier);
			}
			RemoveSubscriber(hook.Identifier, requester);
			Enqueue(hook.Identifier);
			foreach (HookEx item2 in GetHookDependencyTree(hook))
			{
				RemoveSubscriber(item2.Identifier, requester);
				Enqueue(item2.Identifier);
			}
		}
		catch (Exception ex)
		{
			Logger.Error($"Error while unsubscribing hook '{hook}'", ex);
		}
	}

	public void UnsubscribeAll(string hookName)
	{
		IEnumerable<string> hookSubscribers = GetHookSubscribers(hookName);
		foreach (string item in hookSubscribers)
		{
			Unsubscribe(hookName, item);
		}
	}

	public bool AnySubscribers(string hookName)
	{
		return _subscribers.Any((Subscription x) => x.Identifier.Equals(hookName));
	}

	public static string GetMethodMSILHash(MethodInfo method)
	{
		return SHA1(method?.GetMethodBody()?.GetILAsByteArray());
	}

	public static string SHA1(byte[] raw)
	{
		if (raw == null || raw.Length == 0)
		{
			return null;
		}
		using SHA1Managed sHA1Managed = new SHA1Managed();
		byte[] source = sHA1Managed.ComputeHash(raw);
		return string.Concat(source.Select((byte b) => b.ToString("x2"))).ToLower();
	}

	internal void Dispose(bool disposing)
	{
		if (_disposing)
		{
			return;
		}
		if (disposing)
		{
			foreach (HookEx dynamicHook in _dynamicHooks)
			{
				dynamicHook.Dispose();
			}
			_dynamicHooks = null;
			foreach (HookEx item in _installed)
			{
				item.Dispose();
			}
			_installed = null;
			foreach (HookEx patch in _patches)
			{
				patch.Dispose();
			}
			_patches = null;
			foreach (HookEx staticHook in _staticHooks)
			{
				staticHook.Dispose();
			}
			_staticHooks = null;
			_workQueue = null;
			_subscribers = null;
		}
		_disposing = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}

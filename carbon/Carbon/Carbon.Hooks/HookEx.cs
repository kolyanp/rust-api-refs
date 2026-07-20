using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using API.Hooks;
using Carbon.Extensions;
using HarmonyLib;

namespace Carbon.Hooks;

public class HookEx : IDisposable, IHook
{
	private HookRuntime _runtime;

	private readonly TypeInfo _patchMethod;

	private bool hasDisposed;

	public string HookName { get; }

	public string HookFullName { get; }

	public HookFlags Options { get; set; }

	public Type TargetType { get; }

	public MethodType MethodType { get; }

	public string TargetMethod { get; }

	public List<MethodBase> TargetMethods { get; }

	public Type[] TargetMethodArgs { get; }

	public string Identifier { get; }

	public string ShortIdentifier
	{
		get
		{
			string identifier = Identifier;
			return identifier.Substring(identifier.Length - 6);
		}
	}

	public string[] Dependencies { get; }

	public bool IsPatch => Options.HasFlag(HookFlags.Patch);

	public bool IsStaticHook => Options.HasFlag(HookFlags.Static);

	public bool IsDynamicHook
	{
		get
		{
			if (!Options.HasFlag(HookFlags.Static))
			{
				return !Options.HasFlag(HookFlags.Patch);
			}
			return false;
		}
	}

	public bool IsHidden => Options.HasFlag(HookFlags.Hidden);

	public bool IsChecksumIgnored => Options.HasFlag(HookFlags.IgnoreChecksum);

	public bool IsLoaded
	{
		get
		{
			HookState status = _runtime.Status;
			if ((uint)status <= 3u)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsInstalled
	{
		get
		{
			HookState status = _runtime.Status;
			if (status == HookState.Warning || status == HookState.Success)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsFailed => _runtime.Status == HookState.Failure;

	public HookState Status
	{
		get
		{
			return _runtime.Status;
		}
		set
		{
			_runtime.Status = value;
		}
	}

	public string PatchMethodName => _patchMethod.Name;

	public string LastError => _runtime.LastError;

	public override string ToString()
	{
		return HookName + "[" + ShortIdentifier + "]";
	}

	public HookEx(TypeInfo type)
	{
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Expected O, but got Unknown
		try
		{
			Harmony.DEBUG = false;
			if (type == null || !Attribute.IsDefined(type, typeof(HookAttribute.Patch), inherit: false))
			{
				throw new Exception("Type is null or metadata not defined");
			}
			HookAttribute.Patch patch = type.GetCustomAttribute<HookAttribute.Patch>() ?? null;
			if (patch == null)
			{
				throw new Exception("Metadata information is invalid or was not found");
			}
			Dependencies = Array.Empty<string>();
			HookFullName = patch.FullName;
			HookName = patch.Name;
			TargetMethod = patch.Method;
			TargetMethodArgs = ((patch.MethodArgs == null) ? Array.Empty<Type>() : patch.MethodArgs.Select(AccessToolsEx.TypeByName).ToArray());
			TargetMethods = new List<MethodBase>();
			TargetType = (string.IsNullOrEmpty(patch.Target) ? null : AccessToolsEx.TypeByName(patch.Target));
			MethodType = patch.MethodType;
			Identifier = type.GetCustomAttribute<HookAttribute.Identifier>()?.Value ?? $"{Guid.NewGuid():N}";
			Options = type.GetCustomAttribute<HookAttribute.Options>()?.Value ?? HookFlags.None;
			if (Attribute.IsDefined(type, typeof(HookAttribute.Dependencies), inherit: false))
			{
				Dependencies = type.GetCustomAttribute<HookAttribute.Dependencies>()?.Value ?? null;
			}
			if (Options.HasFlag(HookFlags.MetadataOnly))
			{
				SetStatus(HookState.Inactive);
				return;
			}
			_patchMethod = type;
			_runtime.Status = HookState.Inactive;
			_runtime.HarmonyHandler = new Harmony(Identifier);
			_runtime.Prefix = AccessTools.Method((Type)type, "Prefix", (Type[])null, (Type[])null) ?? null;
			_runtime.Postfix = AccessTools.Method((Type)type, "Postfix", (Type[])null, (Type[])null) ?? null;
			_runtime.Transpiler = AccessTools.Method((Type)type, "Transpiler", (Type[])null, (Type[])null) ?? null;
			if (TargetType.IsGenericType)
			{
				Type targetType = TargetType;
				IEnumerable<Type> constraints = AccessToolsEx.GetConstraints(targetType);
				foreach (Type item2 in AccessToolsEx.MatchConstrains(constraints))
				{
					Type type2 = targetType.MakeGenericType(item2);
					MethodInfo methodInfo = AccessTools.Method(type2, TargetMethod, (Type[])null, (Type[])null) ?? null;
					if (methodInfo != null)
					{
						TargetMethods.Add(methodInfo);
					}
				}
				if (TargetMethods.Count == 0)
				{
					throw new Exception($"Signature for '{TargetType}.{TargetMethod}' not found");
				}
			}
			else
			{
				MethodBase item = GetTargetMethodInfo() ?? throw new Exception($"Signature for '{TargetType}.{TargetMethod}' not found");
				TargetMethods.Add(item);
			}
		}
		catch (Exception ex)
		{
			Logger.Error("Error while parsing '" + type.Name + "'", ex);
			SetStatus(HookState.Failure, ex.Message);
		}
		finally
		{
			Harmony.DEBUG = true;
		}
	}

	public bool ApplyPatch()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		if (IsInstalled)
		{
			return true;
		}
		HarmonyMethod val = null;
		HarmonyMethod val2 = null;
		HarmonyMethod val3 = null;
		try
		{
			if (_runtime.Prefix != null)
			{
				val = new HarmonyMethod(_runtime.Prefix, 700, (string[])null, (string[])null, (bool?)null);
			}
			if (_runtime.Postfix != null)
			{
				val2 = new HarmonyMethod(_runtime.Postfix, 700, (string[])null, (string[])null, (bool?)null);
			}
			if (_runtime.Transpiler != null)
			{
				val3 = new HarmonyMethod(_runtime.Transpiler, 700, (string[])null, (string[])null, (bool?)null);
			}
			if (val == null && val2 == null && val3 == null)
			{
				throw new Exception("(prefix, postfix, transpiler not found");
			}
			if (TargetMethod == null || TargetMethod.Length == 0)
			{
				throw new Exception("target method not found");
			}
		}
		catch (Exception ex)
		{
			Logger.Error($"Error while patching hook '{this}'", ex);
			_runtime.Status = HookState.Failure;
			_runtime.LastError = ex.Message;
			return false;
		}
		try
		{
			foreach (MethodBase targetMethod in TargetMethods)
			{
				MethodInfo methodInfo = (_runtime.HarmonyHandler.Patch(targetMethod, val, val2, val3, (HarmonyMethod)null) ?? null) ?? throw new Exception($"HarmonyLib failed to patch '{targetMethod}'");
				_runtime.Status = HookState.Success;
			}
		}
		catch (Exception ex2)
		{
			Logger.Error($"Error while patching hook '{this}'", ex2.InnerException ?? ex2);
			_runtime.Status = HookState.Failure;
			_runtime.LastError = ex2.Message;
			return false;
		}
		return true;
	}

	public bool RemovePatch()
	{
		try
		{
			if (!IsInstalled)
			{
				return true;
			}
			_runtime.HarmonyHandler.UnpatchAll(Identifier);
			_runtime.Status = HookState.Inactive;
			return true;
		}
		catch (Exception ex)
		{
			_runtime.LastError = ex.Message;
			return false;
		}
	}

	public MethodInfo GetTargetMethodInfo()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		MethodType methodType = MethodType;
		if ((int)methodType != 1)
		{
			if ((int)methodType == 2)
			{
				return AccessTools.PropertySetter(TargetType, TargetMethod);
			}
			return AccessTools.Method(TargetType, TargetMethod, TargetMethodArgs, (Type[])null) ?? null;
		}
		return AccessTools.PropertyGetter(TargetType, TargetMethod);
	}

	public void SetStatus(HookState Status, string error = null)
	{
		_runtime.Status = Status;
		_runtime.LastError = error;
	}

	public bool HasDependencies()
	{
		string[] dependencies = Dependencies;
		if (dependencies != null)
		{
			return dependencies.Length > 0;
		}
		return false;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!hasDisposed)
		{
			if (disposing)
			{
				RemovePatch();
			}
			_runtime.HarmonyHandler = null;
			hasDisposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}

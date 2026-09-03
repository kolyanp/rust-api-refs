using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Carbon.Base;
using Carbon.Base.Interfaces;
using Carbon.Core;
using Carbon.Extensions;
using Carbon.Pooling;
using Facepunch;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Oxide.Core;
using Oxide.Plugins;

namespace Carbon;

public static class HookCaller
{
	private static char[] _underscoreChar = new char[1] { '_' };

	private static char[] _dotChar = new char[1] { '.' };

	private static string[] _operatorsStrings = new string[2] { "&&", "||" };

	private static string _ifDirective = "#if";

	private static string _elifDirective = "#elif";

	public static HookCallerCommon Caller { get; set; }

	public static IEnumerable<BaseHookable.CachedHook> GetAllFor(uint hook)
	{
		foreach (BaseHookable.CachedHook item in from package in ModLoader.Packages
			from plugin in package.Plugins
			from cache in plugin.HookPool
			where cache.Key == hook
			from cacheHook in cache.Value.Hooks
			select cacheHook)
		{
			yield return item;
		}
		foreach (BaseHookable.CachedHook item2 in from module in Community.Runtime.ModuleProcessor.Modules
			from cache in module.HookPool
			where cache.Key == hook
			from cacheHook in cache.Value.Hooks
			select cacheHook)
		{
			yield return item2;
		}
	}

	public static TimeSpan GetTotalTime(uint hook)
	{
		TimeSpan result = default(TimeSpan);
		foreach (BaseHookable.CachedHook item in GetAllFor(hook))
		{
			result += item.HookTime;
		}
		return result;
	}

	public static int GetTotalFires(uint hook)
	{
		int num = 0;
		foreach (BaseHookable.CachedHook item in GetAllFor(hook))
		{
			num += item.TimesFired;
		}
		return num;
	}

	public static double GetTotalMemory(uint hook)
	{
		double num = 0.0;
		foreach (BaseHookable.CachedHook item in GetAllFor(hook))
		{
			num += item.MemoryUsage;
		}
		return num;
	}

	public static double GetTotalLagSpikes(uint hook)
	{
		double num = 0.0;
		foreach (BaseHookable.CachedHook item in GetAllFor(hook))
		{
			num += (double)item.LagSpikes;
		}
		return num;
	}

	public static int GetTotalExceptions(uint hook)
	{
		int num = 0;
		foreach (BaseHookable.CachedHook item in GetAllFor(hook))
		{
			num += item.Exceptions;
		}
		return num;
	}

	private static object CallStaticHook(uint hookId, BindingFlags flag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, object[] args = null)
	{
		if (Community.Runtime == null || Community.Runtime.ModuleProcessor == null)
		{
			return null;
		}
		object result = null;
		List<HookCallerCommon.Conflict> list = null;
		List<BaseHookable> modules = Community.Runtime.ModuleProcessor.Modules;
		for (int i = 0; i < modules.Count; i++)
		{
			BaseHookable baseHookable = modules[i];
			try
			{
				if (baseHookable is IModule module && !module.IsEnabled())
				{
					continue;
				}
				object obj = Caller.CallHook(baseHookable, hookId, flag, args);
				if (obj != null)
				{
					if (list == null)
					{
						list = Pool.Get<List<HookCallerCommon.Conflict>>();
					}
					result = obj;
					ResultOverride(list, baseHookable, hookId, result);
				}
			}
			catch (Exception ex)
			{
				Exception ex2 = ex.InnerException ?? ex;
				string orAdd = HookStringPool.GetOrAdd(hookId);
				Logger.Error($"Failed to call hook '{orAdd}' on module '{baseHookable.Name} v{baseHookable.Version}'", ex2);
			}
		}
		for (int j = 0; j < ModLoader.Packages.Count; j++)
		{
			ModLoader.Package package = ModLoader.Packages[j];
			for (int k = 0; k < package.Plugins.Count; k++)
			{
				RustPlugin rustPlugin = package.Plugins[k];
				try
				{
					object obj2 = Caller.CallHook(rustPlugin, hookId, flag, args);
					if (obj2 != null)
					{
						if (list == null)
						{
							list = Pool.Get<List<HookCallerCommon.Conflict>>();
						}
						result = obj2;
						ResultOverride(list, rustPlugin, hookId, result);
					}
				}
				catch (Exception ex3)
				{
					Exception ex4 = ex3.InnerException ?? ex3;
					string orAdd2 = HookStringPool.GetOrAdd(hookId);
					Logger.Error($"Failed to call hook '{orAdd2}' on plugin '{rustPlugin.Name} v{rustPlugin.Version}'", ex4);
				}
			}
		}
		ConflictCheck(list, ref result, hookId);
		if (list != null)
		{
			Pool.FreeUnmanaged<HookCallerCommon.Conflict>(ref list);
		}
		return result;
	}

	private static object CallStaticDeprecatedHook(uint oldHookId, uint newHookId, DateTime expireDate, BindingFlags flag = BindingFlags.Static | BindingFlags.NonPublic, object[] args = null)
	{
		if (expireDate < DateTime.Now)
		{
			return null;
		}
		DateTime now = DateTime.Now;
		if (!Caller._lastDeprecatedWarningAt.TryGetValue(oldHookId, out var value) || (now - value).TotalSeconds > 3600.0)
		{
			Caller._lastDeprecatedWarningAt[oldHookId] = now;
			Logger.Warn(string.Format("A plugin is using deprecated hook '{0}[{1}]', which will stop working on {2:D}. Please ask the author to update to '{3}[{4}]'", new object[5]
			{
				HookStringPool.GetOrAdd(oldHookId),
				oldHookId,
				expireDate,
				HookStringPool.GetOrAdd(newHookId),
				newHookId
			}));
		}
		return CallStaticHook(oldHookId, flag, args);
	}

	public static void ResultOverride(List<HookCallerCommon.Conflict> conflicts, BaseHookable hookable, uint hookId, object result)
	{
		if (result != null)
		{
			conflicts.Add(HookCallerCommon.Conflict.Make(hookable, hookId, result));
		}
	}

	public static void ConflictCheck(List<HookCallerCommon.Conflict> conflicts, ref object result, uint hookId)
	{
		if (conflicts == null || conflicts.Count <= 1)
		{
			return;
		}
		object obj = (result = conflicts[0].Result);
		bool flag = false;
		for (int i = 0; i < conflicts.Count; i++)
		{
			HookCallerCommon.Conflict conflict = conflicts[i];
			if (obj != null && (conflict.Result == null || !conflict.Result.Equals(obj)))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			Logger.Warn(string.Format(" Hook conflict while calling '{0}[{1}]': {2}", HookStringPool.GetOrAdd(hookId), hookId, conflicts.Select((HookCallerCommon.Conflict x) => $"{x.Hookable.Name} {x.Hookable.Version} [{x.Result}]").ToString(", ", " and ")));
			result = conflicts[conflicts.Count - 1].Result;
		}
	}

	public static object CallHook(BaseHookable plugin, uint hookId)
	{
		object[] array = Caller.AllocateBuffer(0);
		object result = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static T CallHook<T>(BaseHookable plugin, uint hookId)
	{
		object[] array = Caller.AllocateBuffer(0);
		object obj = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static T CallDeprecatedHook<T>(BaseHookable plugin, uint oldHookId, uint newHookId, DateTime expireDate)
	{
		object[] array = Caller.AllocateBuffer(0);
		object obj = Caller.CallDeprecatedHook(plugin, oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static object CallHook(BaseHookable plugin, uint hookId, object arg1)
	{
		object[] array = Caller.AllocateBuffer(1);
		array[0] = arg1;
		object result = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static T CallHook<T>(BaseHookable plugin, uint hookId, object arg1)
	{
		object[] array = Caller.AllocateBuffer(1);
		array[0] = arg1;
		object obj = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static T CallDeprecatedHook<T>(BaseHookable plugin, uint oldHookId, uint newHookId, DateTime expireDate, object arg1)
	{
		object[] array = Caller.AllocateBuffer(1);
		array[0] = arg1;
		object obj = Caller.CallDeprecatedHook(plugin, oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static object CallHook(BaseHookable plugin, uint hookId, object arg1, object arg2)
	{
		object[] array = Caller.AllocateBuffer(2);
		array[0] = arg1;
		array[1] = arg2;
		object result = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static T CallHook<T>(BaseHookable plugin, uint hookId, object arg1, object arg2)
	{
		object[] array = Caller.AllocateBuffer(2);
		array[0] = arg1;
		array[1] = arg2;
		object obj = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static T CallDeprecatedHook<T>(BaseHookable plugin, uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2)
	{
		object[] array = Caller.AllocateBuffer(2);
		array[0] = arg1;
		array[1] = arg2;
		object obj = Caller.CallDeprecatedHook(plugin, oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static object CallHook(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3)
	{
		object[] array = Caller.AllocateBuffer(3);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		object result = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static T CallHook<T>(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3)
	{
		object[] array = Caller.AllocateBuffer(3);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		object obj = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static T CallDeprecatedHook<T>(BaseHookable plugin, uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3)
	{
		object[] array = Caller.AllocateBuffer(3);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		object obj = Caller.CallDeprecatedHook(plugin, oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static object CallHook(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4)
	{
		object[] array = Caller.AllocateBuffer(4);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		object result = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static T CallHook<T>(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4)
	{
		object[] array = Caller.AllocateBuffer(4);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		object obj = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static T CallDeprecatedHook<T>(BaseHookable plugin, uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4)
	{
		object[] array = Caller.AllocateBuffer(4);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		object obj = Caller.CallDeprecatedHook(plugin, oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static object CallHook(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5)
	{
		object[] array = Caller.AllocateBuffer(5);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		object result = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static T CallHook<T>(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5)
	{
		object[] array = Caller.AllocateBuffer(5);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		object obj = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static T CallDeprecatedHook<T>(BaseHookable plugin, uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5)
	{
		object[] array = Caller.AllocateBuffer(5);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		object obj = Caller.CallDeprecatedHook(plugin, oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static object CallHook(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
	{
		object[] array = Caller.AllocateBuffer(6);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		object result = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static T CallHook<T>(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
	{
		object[] array = Caller.AllocateBuffer(6);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		object obj = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static T CallDeprecatedHook<T>(BaseHookable plugin, uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
	{
		object[] array = Caller.AllocateBuffer(6);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		object obj = Caller.CallDeprecatedHook(plugin, oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static object CallHook(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
	{
		object[] array = Caller.AllocateBuffer(7);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		object result = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static T CallHook<T>(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
	{
		object[] array = Caller.AllocateBuffer(7);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		object obj = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static T CallDeprecatedHook<T>(BaseHookable plugin, uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
	{
		object[] array = Caller.AllocateBuffer(7);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		object obj = Caller.CallDeprecatedHook(plugin, oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static object CallHook(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
	{
		object[] array = Caller.AllocateBuffer(8);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		object result = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static T CallHook<T>(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
	{
		object[] array = Caller.AllocateBuffer(8);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		object obj = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static T CallDeprecatedHook<T>(BaseHookable plugin, uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
	{
		object[] array = Caller.AllocateBuffer(8);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		object obj = Caller.CallDeprecatedHook(plugin, oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static object CallHook(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
	{
		object[] array = Caller.AllocateBuffer(9);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		object result = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static T CallHook<T>(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
	{
		object[] array = Caller.AllocateBuffer(9);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		object obj = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static T CallDeprecatedHook<T>(BaseHookable plugin, uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
	{
		object[] array = Caller.AllocateBuffer(9);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		object obj = Caller.CallDeprecatedHook(plugin, oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static object CallHook(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10)
	{
		object[] array = Caller.AllocateBuffer(10);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		object result = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static T CallHook<T>(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10)
	{
		object[] array = Caller.AllocateBuffer(10);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		object obj = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static T CallDeprecatedHook<T>(BaseHookable plugin, uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10)
	{
		object[] array = Caller.AllocateBuffer(10);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		object obj = Caller.CallDeprecatedHook(plugin, oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static object CallHook(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11)
	{
		object[] array = Caller.AllocateBuffer(11);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		object result = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static T CallHook<T>(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11)
	{
		object[] array = Caller.AllocateBuffer(11);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		object obj = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static T CallDeprecatedHook<T>(BaseHookable plugin, uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11)
	{
		object[] array = Caller.AllocateBuffer(11);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		object obj = Caller.CallDeprecatedHook(plugin, oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static object CallHook(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12)
	{
		object[] array = Caller.AllocateBuffer(12);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		object result = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static T CallHook<T>(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12)
	{
		object[] array = Caller.AllocateBuffer(12);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		object obj = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static T CallDeprecatedHook<T>(BaseHookable plugin, uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12)
	{
		object[] array = Caller.AllocateBuffer(12);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		object obj = Caller.CallDeprecatedHook(plugin, oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static object CallHook(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13)
	{
		object[] array = Caller.AllocateBuffer(13);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		array[12] = arg13;
		object result = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static T CallHook<T>(BaseHookable plugin, uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13)
	{
		object[] array = Caller.AllocateBuffer(13);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		array[12] = arg13;
		object obj = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static T CallDeprecatedHook<T>(BaseHookable plugin, uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13)
	{
		object[] array = Caller.AllocateBuffer(13);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		array[12] = arg13;
		object obj = Caller.CallDeprecatedHook(plugin, oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static object CallHook(BaseHookable plugin, uint hookId, object[] args)
	{
		return Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, args);
	}

	public static T CallHook<T>(BaseHookable plugin, uint hookId, object[] args)
	{
		object obj = Caller.CallHook(plugin, hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, args);
		if (obj != null)
		{
			return (T)TypeEx.ConvertType<T>(obj);
		}
		return default(T);
	}

	public static object CallStaticHook(uint hookId)
	{
		object[] array = Caller.AllocateBuffer(0);
		object result = CallStaticHook(hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticDeprecatedHook(uint oldHookId, uint newHookId, DateTime expireDate)
	{
		object[] array = Caller.AllocateBuffer(0);
		object result = CallStaticDeprecatedHook(oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticHook(uint hookId, object arg1)
	{
		object[] array = Caller.AllocateBuffer(1);
		array[0] = arg1;
		object result = CallStaticHook(hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticDeprecatedHook(uint oldHookId, uint newHookId, DateTime expireDate, object arg1)
	{
		object[] array = Caller.AllocateBuffer(1);
		array[0] = arg1;
		object result = CallStaticDeprecatedHook(oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticHook(uint hookId, object arg1, object arg2)
	{
		object[] array = Caller.AllocateBuffer(2);
		array[0] = arg1;
		array[1] = arg2;
		object result = CallStaticHook(hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticDeprecatedHook(uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2)
	{
		object[] array = Caller.AllocateBuffer(2);
		array[0] = arg1;
		array[1] = arg2;
		object result = CallStaticDeprecatedHook(oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticHook(uint hookId, object arg1, object arg2, object arg3)
	{
		object[] array = Caller.AllocateBuffer(3);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		object result = CallStaticHook(hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticDeprecatedHook(uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3)
	{
		object[] array = Caller.AllocateBuffer(3);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		object result = CallStaticDeprecatedHook(oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticHook(uint hookId, object arg1, object arg2, object arg3, object arg4)
	{
		object[] array = Caller.AllocateBuffer(4);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		object result = CallStaticHook(hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticDeprecatedHook(uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4)
	{
		object[] array = Caller.AllocateBuffer(4);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		object result = CallStaticDeprecatedHook(oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticHook(uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5)
	{
		object[] array = Caller.AllocateBuffer(5);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		object result = CallStaticHook(hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticDeprecatedHook(uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5)
	{
		object[] array = Caller.AllocateBuffer(5);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		object result = CallStaticDeprecatedHook(oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticHook(uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
	{
		object[] array = Caller.AllocateBuffer(6);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		object result = CallStaticHook(hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticDeprecatedHook(uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
	{
		object[] array = Caller.AllocateBuffer(6);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		object result = CallStaticDeprecatedHook(oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticHook(uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
	{
		object[] array = Caller.AllocateBuffer(7);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		object result = CallStaticHook(hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticDeprecatedHook(uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
	{
		object[] array = Caller.AllocateBuffer(7);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		object result = CallStaticDeprecatedHook(oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticHook(uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
	{
		object[] array = Caller.AllocateBuffer(8);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		object result = CallStaticHook(hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticDeprecatedHook(uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
	{
		object[] array = Caller.AllocateBuffer(8);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		object result = CallStaticDeprecatedHook(oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticHook(uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
	{
		object[] array = Caller.AllocateBuffer(9);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		object result = CallStaticHook(hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticDeprecatedHook(uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
	{
		object[] array = Caller.AllocateBuffer(9);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		object result = CallStaticDeprecatedHook(oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticHook(uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10)
	{
		object[] array = Caller.AllocateBuffer(10);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		object result = CallStaticHook(hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticDeprecatedHook(uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10)
	{
		object[] array = Caller.AllocateBuffer(10);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		object result = CallStaticDeprecatedHook(oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticHook(uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11)
	{
		object[] array = Caller.AllocateBuffer(11);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		object result = CallStaticHook(hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticDeprecatedHook(uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11)
	{
		object[] array = Caller.AllocateBuffer(11);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		object result = CallStaticDeprecatedHook(oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticHook(uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12)
	{
		object[] array = Caller.AllocateBuffer(12);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		object result = CallStaticHook(hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticDeprecatedHook(uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12)
	{
		object[] array = Caller.AllocateBuffer(12);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		object result = CallStaticDeprecatedHook(oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticHook(uint hookId, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13)
	{
		object[] array = Caller.AllocateBuffer(13);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		array[12] = arg13;
		object result = CallStaticHook(hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticDeprecatedHook(uint oldHookId, uint newHookId, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13)
	{
		object[] array = Caller.AllocateBuffer(13);
		array[0] = arg1;
		array[1] = arg2;
		array[2] = arg3;
		array[3] = arg4;
		array[4] = arg5;
		array[5] = arg6;
		array[6] = arg7;
		array[7] = arg8;
		array[8] = arg9;
		array[9] = arg10;
		array[10] = arg11;
		array[11] = arg12;
		array[12] = arg13;
		object result = CallStaticDeprecatedHook(oldHookId, newHookId, expireDate, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, array);
		Caller.ReturnBuffer(array);
		return result;
	}

	public static object CallStaticHook(uint hookId, object[] args)
	{
		return CallStaticHook(hookId, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, args);
	}

	public static object CallStaticDeprecatedHook(uint oldHookId, uint newHookId, DateTime expireDate, object[] args)
	{
		return CallStaticDeprecatedHook(oldHookId, newHookId, expireDate, args);
	}

	public static void HandleVersionConditionals(CompilationUnitSyntax input, List<string> conditionals)
	{
		PooledList<string> val = Pool.Get<PooledList<string>>();
		try
		{
			GetDirectives((List<string>)(object)val);
			for (int i = 0; i < ((List<string>)(object)val).Count; i++)
			{
				string text = ((List<string>)(object)val)[i];
				string text2 = text.Replace(_ifDirective, string.Empty).Replace(_elifDirective, string.Empty).Trim();
				string[] array = text2.Split(_operatorsStrings, StringSplitOptions.RemoveEmptyEntries);
				for (int j = 0; j < array.Length; j++)
				{
					string text3 = array[j].Trim();
					string[] array2 = text3.Split(_underscoreChar);
					if (array2.Length < 3)
					{
						continue;
					}
					string text4 = ((array2.Length != 0) ? array2[0] : null);
					string text5 = ((array2.Length > 1) ? array2[1] : null);
					string value = ((array2.Length > 2) ? array2[2] : null);
					string value2 = ((array2.Length > 3) ? array2[3] : null);
					string value3 = ((array2.Length > 4) ? array2[4] : null);
					VersionNumber versionNumber = new VersionNumber(value.ToInt(), value2.ToInt(), value3.ToInt());
					if (!(text4 == "RUST"))
					{
						if (text4 == "CARBON")
						{
							string[] array3 = Community.Runtime.Analytics.Protocol.Split(_dotChar);
							VersionNumber versionNumber2 = new VersionNumber(array3[0].ToInt(), array3[1].ToInt(), array3[2].ToInt());
							if ((text5.Equals("ABV") && versionNumber2 > versionNumber) || (text5.Equals("BLW") && versionNumber2 < versionNumber) || (text5.Equals("IS") && versionNumber2 == versionNumber))
							{
								conditionals.Add(text3);
							}
						}
					}
					else
					{
						VersionNumber versionNumber3 = new VersionNumber(2633, 288, 1);
						if ((text5.Equals("ABV") && versionNumber3 > versionNumber) || (text5.Equals("BLW") && versionNumber3 < versionNumber) || (text5.Equals("IS") && versionNumber3 == versionNumber))
						{
							conditionals.Add(text3);
						}
					}
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		void GetDirectives(List<string> output)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			foreach (SyntaxNodeOrToken item in ((SyntaxNode)input).DescendantNodesAndTokensAndSelf((Func<SyntaxNode, bool>)null, false))
			{
				SyntaxNodeOrToken current = item;
				if (((SyntaxNodeOrToken)(ref current)).ContainsDirectives)
				{
					SyntaxNode val2 = ((SyntaxNodeOrToken)(ref current)).AsNode();
					if (val2 != null && (CSharpExtensions.IsKind(val2, (SyntaxKind)8548) || CSharpExtensions.IsKind(val2, (SyntaxKind)8549)))
					{
						DirectiveTriviaSyntax firstDirective = CSharpExtensions.GetFirstDirective(val2, (Func<DirectiveTriviaSyntax, bool>)null);
						if (firstDirective != null)
						{
							output.Add(((object)((SyntaxNode)firstDirective).GetText((Encoding)null, (SourceHashAlgorithm)1)).ToString());
						}
					}
					else
					{
						SyntaxToken val3 = ((SyntaxNodeOrToken)(ref current)).AsToken();
						SyntaxTriviaList leadingTrivia = ((SyntaxToken)(ref val3)).LeadingTrivia;
						for (int k = 0; k < ((SyntaxTriviaList)(ref leadingTrivia)).Count; k++)
						{
							SyntaxTrivia val4 = ((SyntaxTriviaList)(ref leadingTrivia))[k];
							if (((SyntaxTrivia)(ref val4)).IsDirective && (CSharpExtensions.IsKind(val4, (SyntaxKind)8548) || CSharpExtensions.IsKind(val4, (SyntaxKind)8549)))
							{
								output.Add(((object)((SyntaxTrivia)(ref val4)).GetStructure().GetText((Encoding)null, (SourceHashAlgorithm)1)).ToString());
							}
						}
					}
				}
			}
		}
	}
}

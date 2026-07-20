using System;
using System.Collections.Generic;
using System.Reflection;
using Carbon.Base;
using Carbon.Components;
using Carbon.Pooling;
using Facepunch;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Carbon.Hooks;

public class HookCallerInternal : HookCallerCommon
{
	public override object[] AllocateBuffer(int count)
	{
		if (_argumentBuffer.TryGetValue(count, out var value))
		{
			return value.Rent();
		}
		value = (_argumentBuffer[count] = new HookArgPool(count));
		return value.Rent();
	}

	public override object[] RescaleBuffer(object[] oldBuffer, int newScale, BaseHookable.CachedHook hook)
	{
		if (oldBuffer.Length == newScale)
		{
			return oldBuffer;
		}
		object[] array = AllocateBuffer(newScale);
		for (int i = 0; i < newScale; i++)
		{
			object obj = array[i];
			obj = ((i > oldBuffer.Length - 1) ? (array[i] = null) : (array[i] = oldBuffer[i]));
			if (i <= hook.InfoParameters.Length - 1 && obj == null)
			{
				ParameterInfo parameterInfo = hook.InfoParameters[i];
				if (parameterInfo.HasDefaultValue)
				{
					array[i] = parameterInfo.DefaultValue;
				}
			}
		}
		return array;
	}

	public override void ProcessDefaults(object[] buffer, BaseHookable.CachedHook hook)
	{
		int num = buffer.Length;
		for (int i = 0; i < num; i++)
		{
			if (i <= hook.InfoParameters.Length - 1 && buffer[i] == null)
			{
				ParameterInfo parameterInfo = hook.InfoParameters[i];
				if (parameterInfo != null && parameterInfo.HasDefaultValue)
				{
					buffer[i] = parameterInfo.DefaultValue;
				}
				else
				{
					buffer[i] = null;
				}
			}
		}
	}

	public override void ReturnBuffer(object[] buffer)
	{
		if (!_argumentBuffer.TryGetValue(buffer.Length, out var value))
		{
			value = (_argumentBuffer[buffer.Length] = new HookArgPool(buffer.Length));
		}
		value.Return(buffer);
	}

	public override object CallHook<T>(T hookable, uint hookId, BindingFlags flags, object[] args)
	{
		if (hookable.IsHookIgnored(hookId))
		{
			return null;
		}
		if (!hookable.HasBuiltHookCache)
		{
			hookable.BuildHookCache(flags);
		}
		BaseHookable.CachedHookInstance value = null;
		if (hookable.HookPool != null && !hookable.HookPool.TryGetValue(hookId, out value))
		{
			return null;
		}
		object result = null;
		List<Conflict> conflicts = null;
		bool hasRescaledBuffer = false;
		if (hookable.InternalCallHookOverriden)
		{
			BaseHookable.CachedHook cachedHook = null;
			if (value != null && value.IsValid())
			{
				cachedHook = value.PrimaryHook;
			}
			hookable.TrackStart();
			bool trackHookMemory = Community.Runtime.Config.Debugging.TrackHookMemory;
			long num = (trackHookMemory ? BaseHookable.CurrentMemory : 0);
			if (cachedHook != null && args != null)
			{
				int num2 = cachedHook.Parameters.Length;
				if (num2 != args.Length)
				{
					args = HookCaller.Caller.RescaleBuffer(args, num2, cachedHook);
					hasRescaledBuffer = true;
				}
				else
				{
					HookCaller.Caller.ProcessDefaults(args, cachedHook);
				}
			}
			if (cachedHook != null && cachedHook.IsAsync)
			{
				hookable.InternalCallHook(hookId, args);
				hookable.TrackEnd();
			}
			else
			{
				object obj = hookable.InternalCallHook(hookId, args);
				hookable.TrackEnd();
				if (obj != null)
				{
					if (conflicts == null)
					{
						conflicts = Pool.Get<List<Conflict>>();
					}
					HookCaller.ResultOverride(conflicts, hookable, hookId, result = obj);
				}
			}
			TimeSpan currentHookTime = hookable.CurrentHookTime;
			long num3 = (trackHookMemory ? BaseHookable.CurrentMemory : 0);
			float num4 = (trackHookMemory ? Mathf.Abs((float)(num3 - num)) : 0f);
			cachedHook?.OnFired(hookable, currentHookTime, num4);
			double totalMilliseconds = currentHookTime.TotalMilliseconds;
			if (totalMilliseconds > 100.0 && hookable is Plugin { IsCorePlugin: false } plugin)
			{
				string orAdd = HookStringPool.GetOrAdd(hookId);
				Logger.Warn(string.Format(" {0} hook '{1}' took longer than 100ms [{2:0}ms]{3}", new object[4]
				{
					hookable.ToPrettyString(),
					orAdd,
					totalMilliseconds,
					hookable.HasGCCollected ? " [GC]" : string.Empty
				}));
				bool flag = totalMilliseconds >= (double)Community.Runtime.Config.Debugging.HookLagSpikeThreshold;
				if (flag)
				{
					cachedHook?.OnLagSpike(hookable);
				}
				Analytics.plugin_time_warn(orAdd, plugin, totalMilliseconds, num4, flag, cachedHook, hookable);
			}
			HookCaller.ConflictCheck(conflicts, ref result, hookId);
			FrameDispose(hasRescaledBuffer, args, ref conflicts);
		}
		else
		{
			if (value != null && value.IsValid())
			{
				for (int i = 0; i < value.Hooks.Count; i++)
				{
					BaseHookable.CachedHook cachedHook2 = value.Hooks[i];
					try
					{
						if (cachedHook2.IsAsync)
						{
							DoCall(hookable, hookId, cachedHook2, args, ref hasRescaledBuffer);
							continue;
						}
						object obj2 = DoCall(hookable, hookId, cachedHook2, args, ref hasRescaledBuffer);
						if (obj2 != null)
						{
							if (conflicts == null)
							{
								conflicts = Pool.Get<List<Conflict>>();
							}
							HookCaller.ResultOverride(conflicts, hookable, hookId, result = obj2);
							break;
						}
					}
					catch (Exception ex)
					{
						Exception ex2 = ex.InnerException ?? ex;
						string orAdd2 = HookStringPool.GetOrAdd(hookId);
						Logger.Error($"Failed to call hook '{orAdd2}' on plugin '{hookable.Name} v{hookable.Version}'", ex2);
					}
				}
			}
			HookCaller.ConflictCheck(conflicts, ref result, hookId);
			FrameDispose(hasRescaledBuffer: false, args, ref conflicts);
		}
		return result;
		static object DoCall(T val, uint name, BaseHookable.CachedHook hook, object[] array, ref bool reference)
		{
			if (array != null)
			{
				int num5 = hook.Parameters.Length;
				if (num5 != array.Length)
				{
					array = HookCaller.Caller.RescaleBuffer(array, num5, hook);
					reference = true;
				}
				else
				{
					HookCaller.Caller.ProcessDefaults(array, hook);
				}
			}
			if (array != null && !SequenceEqual(hook.Parameters, array))
			{
				return null;
			}
			object result2 = null;
			val.TrackStart();
			double totalMemoryUsed = val.TotalMemoryUsed;
			try
			{
				result2 = hook.Method.Invoke(val, array);
			}
			catch (Exception ex3)
			{
				Exception ex4 = ex3.InnerException ?? ex3;
				string orAdd3 = HookStringPool.GetOrAdd(name);
				Logger.Error($"Failed to call hook '{orAdd3}' on plugin '{val.Name} v{val.Version}'", ex4);
			}
			val.TrackEnd();
			TimeSpan currentHookTime2 = val.CurrentHookTime;
			double totalMemoryUsed2 = val.TotalMemoryUsed;
			double num6 = totalMemoryUsed2 - totalMemoryUsed;
			hook?.OnFired(val, currentHookTime2, num6);
			double totalMilliseconds2 = currentHookTime2.TotalMilliseconds;
			if (totalMilliseconds2 > 100.0 && val is Plugin { IsCorePlugin: false } plugin2)
			{
				string orAdd4 = HookStringPool.GetOrAdd(name);
				Logger.Warn(string.Format(" {0} hook '{1}' took longer than 100ms [{2:0}ms]{3}", new object[4]
				{
					val.ToPrettyString(),
					orAdd4,
					totalMilliseconds2,
					val.HasGCCollected ? " [GC]" : string.Empty
				}));
				bool flag2 = totalMilliseconds2 >= (double)Community.Runtime.Config.Debugging.HookLagSpikeThreshold;
				if (flag2)
				{
					hook.OnLagSpike(val);
				}
				Analytics.plugin_time_warn(orAdd4, plugin2, totalMilliseconds2, num6, flag2, hook, val);
			}
			if (reference)
			{
				HookCaller.Caller.ReturnBuffer(array);
			}
			return result2;
		}
		static void FrameDispose(bool flag2, object[] buffer, ref List<Conflict> reference)
		{
			if (flag2)
			{
				HookCaller.Caller.ReturnBuffer(buffer);
			}
			if (reference != null)
			{
				Pool.FreeUnmanaged<Conflict>(ref reference);
			}
		}
	}

	public override object CallDeprecatedHook<T>(T plugin, uint oldHook, uint newHook, DateTime expireDate, BindingFlags flags, object[] args)
	{
		if (expireDate < DateTime.Now)
		{
			return null;
		}
		DateTime now = DateTime.Now;
		if (!_lastDeprecatedWarningAt.TryGetValue(oldHook, out var value) || (now - value).TotalSeconds > 3600.0)
		{
			_lastDeprecatedWarningAt[oldHook] = now;
			Logger.Warn(string.Format("'{0} v{1}' is using deprecated hook '{2}', which will stop working on {3}. Please ask the author to update to '{4}'", new object[5]
			{
				plugin.Name,
				plugin.Version,
				oldHook,
				expireDate.ToString("D"),
				newHook
			}));
		}
		return CallHook(plugin, newHook, flags, args);
	}

	internal static bool SequenceEqual(Type[] source, object[] target)
	{
		for (int i = 0; i < source.Length; i++)
		{
			Type type = source[i];
			Type type2 = target[i]?.GetType();
			if (type2 != null && !type.IsByRef && !type2.IsByRef && type != type2 && !type.IsAssignableFrom(type2))
			{
				return false;
			}
		}
		return true;
	}
}

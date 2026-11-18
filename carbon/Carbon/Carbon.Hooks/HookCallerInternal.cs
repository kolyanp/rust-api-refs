using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Carbon.Base;
using Carbon.Components;
using Carbon.Pooling;
using Facepunch;
using Oxide.Core.Plugins;

namespace Carbon.Hooks;

public class HookCallerInternal : HookCallerCommon
{
	private static readonly long _timestampsPerTick = Stopwatch.Frequency / 10000000;

	private static bool IsHookTimeTracked(BaseHookable hookable)
	{
		if (Community.IsServerInitialized)
		{
			return !(hookable is Plugin plugin) || !plugin.IsCorePlugin;
		}
		return false;
	}

	private static TimeSpan GetHookTime(BaseHookable hookable, long startTimestamp, int startGcCount)
	{
		TimeSpan result = (hookable.CurrentHookTime = TimeSpan.FromTicks((Stopwatch.GetTimestamp() - startTimestamp) / _timestampsPerTick));
		hookable._currentGcCount = startGcCount;
		return result;
	}

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
		if (hookable.HookPool == null || !hookable.HookPool.TryGetValue(hookId, out value))
		{
			return null;
		}
		object result = null;
		List<Conflict> conflicts = null;
		if (hookable.InternalCallHookOverriden)
		{
			bool hasRescaledBuffer = false;
			BaseHookable.CachedHook cachedHook = null;
			if (value != null && value.IsValid())
			{
				cachedHook = value.PrimaryHook;
			}
			bool flag = IsHookTimeTracked(hookable);
			long startTimestamp = (flag ? Stopwatch.GetTimestamp() : 0);
			int num = (flag ? BaseHookable.CurrentGcCount : 0);
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
			HookMemory.Scope scope = HookMemory.Begin();
			long num3 = 0L;
			TimeSpan hookTime = TimeSpan.Zero;
			bool flag2 = false;
			try
			{
				object obj = hookable.InternalCallHook(hookId, args);
				if (flag)
				{
					hookTime = GetHookTime(hookable, startTimestamp, num);
					flag2 = hookTime.TotalMilliseconds > 100.0 && num != BaseHookable.CurrentGcCount;
				}
				if (obj != null && (cachedHook == null || !cachedHook.IsAsync))
				{
					if (conflicts == null)
					{
						conflicts = Pool.Get<List<Conflict>>();
					}
					HookCaller.ResultOverride(conflicts, hookable, hookId, result = obj);
				}
			}
			finally
			{
				num3 = HookMemory.End(in scope);
			}
			double totalMilliseconds = hookTime.TotalMilliseconds;
			cachedHook?.OnFired(hookable, hookTime, num3);
			if (totalMilliseconds > 100.0 && hookable is Plugin { IsCorePlugin: false } plugin)
			{
				string orAdd = HookStringPool.GetOrAdd(hookId);
				Logger.Warn(string.Format(" {0} hook '{1}' took longer than 100ms [{2:0}ms]{3}", new object[4]
				{
					hookable.ToPrettyString(),
					orAdd,
					totalMilliseconds,
					flag2 ? " [GC]" : string.Empty
				}));
				bool flag3 = totalMilliseconds >= (double)Community.Runtime.Config.Debugging.HookLagSpikeThreshold;
				if (flag3)
				{
					cachedHook?.OnLagSpike(hookable);
				}
				Analytics.plugin_time_warn(orAdd, plugin, totalMilliseconds, num3, flag3, cachedHook, flag2);
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
							DoCall(hookable, hookId, cachedHook2, args);
							continue;
						}
						object obj2 = DoCall(hookable, hookId, cachedHook2, args);
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
		static object DoCall(T val, uint name, BaseHookable.CachedHook hook, object[] array)
		{
			bool flag4 = false;
			if (array != null)
			{
				int num4 = hook.Parameters.Length;
				if (num4 != array.Length)
				{
					array = HookCaller.Caller.RescaleBuffer(array, num4, hook);
					flag4 = true;
				}
				else
				{
					HookCaller.Caller.ProcessDefaults(array, hook);
				}
			}
			if (array != null && !SequenceEqual(hook.Parameters, array))
			{
				if (flag4)
				{
					HookCaller.Caller.ReturnBuffer(array);
				}
				return null;
			}
			object result2 = null;
			bool flag5 = IsHookTimeTracked(val);
			long startTimestamp2 = (flag5 ? Stopwatch.GetTimestamp() : 0);
			int num5 = (flag5 ? BaseHookable.CurrentGcCount : 0);
			HookMemory.Scope scope2 = HookMemory.Begin();
			long num6 = 0L;
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
			finally
			{
				num6 = HookMemory.End(in scope2);
			}
			TimeSpan hookTime2 = (flag5 ? GetHookTime(val, startTimestamp2, num5) : TimeSpan.Zero);
			double totalMilliseconds2 = hookTime2.TotalMilliseconds;
			bool flag6 = totalMilliseconds2 > 100.0 && num5 != BaseHookable.CurrentGcCount;
			hook?.OnFired(val, hookTime2, num6);
			if (totalMilliseconds2 > 100.0 && val is Plugin { IsCorePlugin: false } plugin2)
			{
				string orAdd4 = HookStringPool.GetOrAdd(name);
				Logger.Warn(string.Format(" {0} hook '{1}' took longer than 100ms [{2:0}ms]{3}", new object[4]
				{
					val.ToPrettyString(),
					orAdd4,
					totalMilliseconds2,
					flag6 ? " [GC]" : string.Empty
				}));
				bool flag7 = totalMilliseconds2 >= (double)Community.Runtime.Config.Debugging.HookLagSpikeThreshold;
				if (flag7)
				{
					hook.OnLagSpike(val);
				}
				Analytics.plugin_time_warn(orAdd4, plugin2, totalMilliseconds2, num6, flag7, hook, flag6);
			}
			if (flag4)
			{
				HookCaller.Caller.ReturnBuffer(array);
			}
			return result2;
		}
		static void FrameDispose(bool flag4, object[] buffer, ref List<Conflict> reference)
		{
			if (flag4)
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

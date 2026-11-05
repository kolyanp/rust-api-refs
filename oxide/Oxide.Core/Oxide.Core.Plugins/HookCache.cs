using System.Collections;
using System.Collections.Generic;

namespace Oxide.Core.Plugins;

public class HookCache
{
	private static readonly object NullKey = new object();

	private readonly Hashtable _cache = new Hashtable();

	private volatile List<HookMethod> _methods;

	public List<HookMethod> GetHookMethod(string hookName, object[] args, out HookCache cache)
	{
		return GetOrAdd(hookName).GetHookMethod(args, 0, out cache);
	}

	public List<HookMethod> GetHookMethod(object[] args, int index, out HookCache cache)
	{
		HookCache hookCache = this;
		if (args != null)
		{
			for (int i = index; i < args.Length; i++)
			{
				object obj = args[i];
				hookCache = hookCache.GetOrAdd((obj == null) ? NullKey : obj.GetType());
			}
		}
		cache = hookCache;
		return hookCache._methods;
	}

	public void SetupMethods(List<HookMethod> methods)
	{
		_methods = methods;
	}

	private HookCache GetOrAdd(object key)
	{
		if (_cache[key] is HookCache result)
		{
			return result;
		}
		lock (_cache)
		{
			HookCache hookCache = _cache[key] as HookCache;
			if (hookCache == null)
			{
				hookCache = new HookCache();
				_cache[key] = hookCache;
			}
			return hookCache;
		}
	}
}

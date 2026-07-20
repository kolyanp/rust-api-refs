using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MySqlConnector.Authentication;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
public static class AuthenticationPlugins
{
	private static readonly object s_lock = new object();

	private static readonly Dictionary<string, IAuthenticationPlugin> s_plugins = new Dictionary<string, IAuthenticationPlugin>();

	public static void Register(IAuthenticationPlugin plugin)
	{
		if (plugin == null)
		{
			throw new ArgumentNullException("plugin");
		}
		if (string.IsNullOrEmpty(plugin.Name))
		{
			throw new ArgumentException("Invalid plugin name.", "plugin");
		}
		lock (s_lock)
		{
			s_plugins.Add(plugin.Name, plugin);
		}
	}

	internal static bool TryGetPlugin(string name, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)][_003Ce940fe46_002D60b5_002D4fb7_002D817f_002D6effabbc4d82_003ENotNullWhen(true)] out IAuthenticationPlugin plugin)
	{
		lock (s_lock)
		{
			return s_plugins.TryGetValue(name, out plugin);
		}
	}
}

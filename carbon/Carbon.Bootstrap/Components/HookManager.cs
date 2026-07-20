using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using API.Hooks;
using Utility;

namespace Components;

internal sealed class HookManager : AddonManager
{
	private readonly string[] _directories = new string[1] { Context.CarbonHooks };

	[MethodImpl(MethodImplOptions.NoInlining)]
	public override Assembly Load(string file, string requester = null)
	{
		if (requester == null)
		{
			MethodBase method = new StackFrame(1).GetMethod();
			requester = $"{method.DeclaringType}.{method.Name}";
		}
		try
		{
			string extension = Path.GetExtension(file);
			if (extension == ".dll")
			{
				Assembly assembly = _loader.Load(file, requester, _directories)?.Assembly ?? throw new ReflectionTypeLoadException(null, null, null);
				if (!base.AssemblyManager.IsType<Patch>(assembly, out var _))
				{
					throw new Exception("Unsupported assembly type");
				}
				return assembly;
			}
			throw new Exception("File extension not supported");
		}
		catch (ReflectionTypeLoadException)
		{
			Logger.Error("Error while loading hooks from '" + file + "'.");
			Logger.Error("Either the file is corrupt or has an unsupported version.");
			return null;
		}
		catch (Exception)
		{
			Logger.Error("Failed loading hook '" + file + "'");
			return null;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public override void Unload(string file, string requester)
	{
	}
}

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using API.Assembly;
using API.Events;
using Carbon;
using Facepunch;
using Utility;

namespace Components;

internal sealed class ComponentManager : AddonManager
{
	private readonly string[] _directories = new string[1] { Context.CarbonManaged };

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
				if (base.AssemblyManager.IsType<ICarbonComponent>(assembly, out var output))
				{
					if (output != null)
					{
						foreach (Type item in output)
						{
							try
							{
								if (!(Activator.CreateInstance(item) is ICarbonComponent carbonComponent))
								{
									throw new NullReferenceException();
								}
								carbonComponent.Awake(EventArgs.Empty);
								carbonComponent.OnLoaded(EventArgs.Empty);
								CarbonEventArgs e = Pool.Get<CarbonEventArgs>();
								e.Init(file);
								Bootstrap.Events.Trigger(CarbonEvent.ComponentLoaded, e);
								Pool.Free<CarbonEventArgs>(ref e);
								base._loaded.Add(new Item
								{
									Addon = carbonComponent,
									File = file
								});
							}
							catch (Exception ex)
							{
								Utility.Logger.Error($"Failed to instantiate component from type '{item}'", ex);
							}
						}
					}
					return assembly;
				}
				throw new Exception("Unsupported assembly type");
			}
			throw new Exception("File extension not supported");
		}
		catch (ReflectionTypeLoadException)
		{
			Utility.Logger.Error("Error while loading component from '" + file + "'.");
			Utility.Logger.Error("Either the file is corrupt or has an unsupported version.");
			return null;
		}
		catch (Exception)
		{
			Utility.Logger.Error("Failed loading component '" + file + "'");
			return null;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public override void Unload(string file, string requester)
	{
	}
}

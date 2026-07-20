using System;
using System.Collections.Generic;
using System.Linq;
using API.Events;
using Carbon.Base;
using Carbon.Base.Interfaces;
using Carbon.Contracts;
using Facepunch;

namespace Carbon.Managers;

public class ModuleProcessor : BaseProcessor, IModuleProcessor, IDisposable
{
	internal List<BaseHookable> _modules;

	public override string Name => "Module Processor";

	public override bool EnableWatcher => false;

	List<BaseHookable> IModuleProcessor.Modules { get; } = new List<BaseHookable>(200);

	public void Init()
	{
		_modules = ((IModuleProcessor)this).Modules;
		Community.Runtime.Events.Subscribe(CarbonEvent.ModuleLoaded, delegate(EventArgs e)
		{
			if (e is ModuleEventArgs e2)
			{
				Type[] array = (e2.Data as IReadOnlyList<Type>).ToArray();
				Build(e2.Payload.ToString(), array);
				Array.Clear(array, 0, array.Length);
				array = null;
			}
		});
		Community.Runtime.Events.Subscribe(CarbonEvent.ModuleUnloaded, delegate(EventArgs e)
		{
			if (e is ModuleEventArgs e2)
			{
				List<BaseHookable> list = Pool.Get<List<BaseHookable>>();
				list.AddRange(_modules);
				foreach (BaseHookable item in list)
				{
					if (item is BaseModule { Context: { } context } baseModule && e2.Payload is string value && context.Equals(value))
					{
						baseModule.Shutdown();
					}
				}
				Pool.FreeUnmanaged<BaseHookable>(ref list);
			}
		});
		Build(typeof(Community).Assembly.GetExportedTypes());
		foreach (KeyValuePair<Type, string> item2 in Community.Runtime.AssemblyEx.Modules.Shared)
		{
			Build(item2.Value, item2.Key);
		}
	}

	public void OnServerInit()
	{
		foreach (BaseHookable module3 in _modules)
		{
			if (module3 is IModule module && module.IsEnabled())
			{
				try
				{
					module.OnServerInit(initial: true);
				}
				catch (Exception ex)
				{
					Logger.Error("[ModuleProcessor] Failed OnServerInit for '" + module3.Name + "'", ex);
				}
			}
		}
		foreach (BaseHookable module4 in _modules)
		{
			if (module4 is IModule module2 && module2.IsEnabled())
			{
				try
				{
					module2.OnPostServerInit(initial: true);
				}
				catch (Exception ex2)
				{
					Logger.Error("[ModuleProcessor] Failed OnPostServerInit for '" + module4.Name + "'", ex2);
				}
			}
		}
	}

	public void OnServerSave()
	{
		foreach (BaseHookable module2 in _modules)
		{
			if (module2 is IModule module && module.IsEnabled())
			{
				try
				{
					module.OnServerSaved();
				}
				catch (Exception ex)
				{
					Logger.Error("[ModuleProcessor] Failed OnServerSave for '" + module2.Name + "'", ex);
				}
			}
		}
	}

	public void Setup(BaseHookable hookable)
	{
		_modules.Add(hookable);
	}

	public void Build(params Type[] types)
	{
		Build(null, types);
	}

	public void Build(string context, params Type[] types)
	{
		List<BaseModule> list = Pool.Get<List<BaseModule>>();
		foreach (Type type in types)
		{
			if (!type.IsAbstract && !(type.BaseType == null) && type.IsSubclassOf(typeof(BaseModule)))
			{
				BaseModule baseModule = Activator.CreateInstance(type) as BaseModule;
				baseModule.Context = context;
				list.Add(baseModule);
			}
		}
		foreach (BaseModule item in list)
		{
			if (item is IModule)
			{
				Setup(item);
			}
		}
		foreach (BaseModule item2 in list)
		{
			if (item2 is IModule module)
			{
				try
				{
					module.Init();
				}
				catch (Exception ex)
				{
					Logger.Error("Failed module Init for " + module?.GetType().FullName, ex);
				}
			}
		}
		foreach (BaseModule item3 in list)
		{
			if (item3 is IModule module2)
			{
				try
				{
					module2.Load();
				}
				catch (Exception ex2)
				{
					Logger.Error("Failed module Load for " + module2?.GetType().FullName, ex2);
				}
			}
		}
		foreach (BaseModule item4 in list)
		{
			if (item4 is IModule module3 && module3.IsEnabled())
			{
				try
				{
					module3.InitEnd();
				}
				catch (Exception ex3)
				{
					Logger.Error("Failed module InitEnd for " + module3?.GetType().FullName, ex3);
				}
			}
		}
		foreach (BaseModule item5 in list)
		{
			if (item5 is IModule module4 && module4.IsEnabled())
			{
				try
				{
					module4.OnEnableStatus();
				}
				catch (Exception ex4)
				{
					Logger.Error("Failed module OnEnableStatus for " + module4?.GetType().FullName, ex4);
				}
			}
		}
		if (Community.IsServerInitialized)
		{
			foreach (BaseModule item6 in list)
			{
				if (item6 is IModule module5 && module5.IsEnabled())
				{
					try
					{
						module5.OnServerInit(initial: true);
					}
					catch (Exception ex5)
					{
						Logger.Error("Failed module OnEnableStatus for " + module5?.GetType().FullName, ex5);
					}
				}
			}
			foreach (BaseModule item7 in list)
			{
				if (item7 is IModule module6 && module6.IsEnabled())
				{
					try
					{
						module6.OnPostServerInit(initial: true);
					}
					catch (Exception ex6)
					{
						Logger.Error("Failed module OnEnableStatus for " + module6?.GetType().FullName, ex6);
					}
				}
			}
		}
		Pool.FreeUnmanaged<BaseModule>(ref list);
	}

	public void Uninstall(IModule module)
	{
		_modules.RemoveAll((BaseHookable x) => x == module);
	}

	public void Save()
	{
		foreach (BaseHookable module2 in _modules)
		{
			IModule module = module2.To<IModule>();
			module.Save();
		}
	}

	public void Load()
	{
		foreach (BaseHookable module2 in _modules)
		{
			IModule module = module2.To<IModule>();
			try
			{
				module.Load();
			}
			catch (Exception ex)
			{
				Logger.Error("Failed module Load for " + module?.GetType().FullName, ex);
			}
		}
	}

	public override void Dispose()
	{
		foreach (BaseHookable module2 in _modules)
		{
			IModule module = module2.To<IModule>();
			module.Dispose();
		}
		_modules.Clear();
		base.Dispose();
	}
}

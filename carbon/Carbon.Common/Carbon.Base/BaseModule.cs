using System;
using System.Globalization;
using System.Linq;
using Oxide.Core.Libraries;
using UnityEngine;

namespace Carbon.Base;

public abstract class BaseModule : BaseHookable
{
	public string Context { get; set; }

	public virtual bool EnabledByDefault => false;

	public virtual bool ForceModded => false;

	public virtual bool ForceEnabled => false;

	public virtual bool ForceDisabled => false;

	public virtual bool ManualCommands => false;

	public virtual bool ConfigVersionChecks => true;

	public abstract void OnServerInit(bool initial);

	public abstract void OnPostServerInit(bool initial);

	public abstract void OnServerSaved();

	public abstract void Load();

	public abstract void Save();

	public abstract void OnUnload();

	public abstract void Reload();

	public abstract bool IsEnabled();

	public abstract void SetEnabled(bool enable);

	public abstract void Shutdown();

	public abstract void SetPermissions(Permission perms);

	public static T GetModule<T>()
	{
		foreach (BaseHookable module in Community.Runtime.ModuleProcessor.Modules)
		{
			if (module.GetType() == typeof(T) && module is T)
			{
				return (T)(object)((module is T) ? module : null);
			}
		}
		return default(T);
	}

	public static BaseModule FindModule(string name)
	{
		return Community.Runtime.ModuleProcessor.Modules.FirstOrDefault((BaseHookable x) => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) || StringEx.Contains(x.Name, name, CompareOptions.OrdinalIgnoreCase)) as BaseModule;
	}
}

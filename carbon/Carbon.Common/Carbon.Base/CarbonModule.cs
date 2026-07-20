using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Carbon.Base.Interfaces;
using Carbon.Core;
using Carbon.Extensions;
using Carbon.Pooling;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;

namespace Carbon.Base;

public abstract class CarbonModule<C, D> : BaseModule, IModule, IDisposable
{
	public class Configuration : IModuleConfig
	{
		public bool Enabled { get; set; }

		public C Config { get; set; }

		public string Version { get; set; }

		public string GetVersion()
		{
			if (Config == null)
			{
				return null;
			}
			Type type = Config.GetType();
			string text = (from x in (from x in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
					select x.PropertyType.FullName + x.Name).Concat(from x in type.GetFields(BindingFlags.Instance | BindingFlags.Public)
					select x.FieldType.FullName + x.Name)
				select (x)).ToString(string.Empty);
			return StringPool.Add(text).ToString();
		}

		public bool HasConfigStructureChanged()
		{
			string version = GetVersion();
			bool flag = version == Version;
			if (!flag)
			{
				Version = version;
			}
			return !flag;
		}
	}

	private bool _isEnabledCached;

	public Permission Permissions;

	public Configuration ModuleConfiguration { get; set; }

	public DynamicConfigFile Config { get; private set; }

	public DynamicConfigFile Data { get; private set; }

	public Lang Lang { get; private set; }

	public virtual Type Type => null;

	public D DataInstance { get; private set; }

	public C ConfigInstance { get; private set; }

	public new virtual string Name => "Not set";

	protected void Puts(object message)
	{
		Logger.Log($"[{Name}] {message}");
	}

	protected void PutsError(object message, Exception ex = null)
	{
		Logger.Error($"[{Name}] {message}", ex);
	}

	protected void PutsWarn(object message)
	{
		Logger.Warn($"[{Name}] {message}");
	}

	public virtual void Dispose()
	{
		Config = null;
		ModuleConfiguration = null;
		RefreshEnabledCache();
	}

	public virtual void Init()
	{
		if (Hooks == null)
		{
			Hooks = new List<uint>();
		}
		if (base.Name == null)
		{
			string text = (base.Name = Name);
		}
		if ((object)base.HookableType == null)
		{
			Type type = (base.HookableType = Type);
		}
		if (!ForceDisabled)
		{
			SetPermissions(Interface.Oxide.Permission);
			TrackInit();
		}
	}

	public virtual bool InitEnd()
	{
		if (ForceDisabled || base.HasInitialized)
		{
			return false;
		}
		Community.Runtime.HookManager.LoadHooksFromType(Type);
		BuildHookCache(BindingFlags.Instance | BindingFlags.NonPublic);
		MethodInfo[] methods = Type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			if (Community.Runtime.HookManager.IsHook(methodInfo.Name))
			{
				Community.Runtime.HookManager.Subscribe(methodInfo.Name, Name);
				uint orAdd = HookStringPool.GetOrAdd(methodInfo.Name);
				if (!Hooks.Contains(orAdd))
				{
					Hooks.Add(orAdd);
				}
			}
		}
		Dictionary<string, Dictionary<string, string>> defaultPhrases = GetDefaultPhrases();
		if (defaultPhrases != null)
		{
			foreach (KeyValuePair<string, Dictionary<string, string>> item in defaultPhrases)
			{
				Lang.RegisterMessages(item.Value, this, item.Key);
			}
		}
		if (!Community.Runtime.Config.Logging.ReducedLogging)
		{
			Puts("Initialized.");
		}
		base.HasInitialized = true;
		return true;
	}

	public override void Load()
	{
		if (ForceDisabled)
		{
			return;
		}
		bool flag = false;
		if (Config == null)
		{
			DynamicConfigFile dynamicConfigFile = (Config = new DynamicConfigFile(GetConfigPath()));
		}
		if (Data == null)
		{
			DynamicConfigFile dynamicConfigFile = (Data = new DynamicConfigFile(GetDataPath()));
		}
		if (Lang == null)
		{
			Lang lang = (Lang = new Lang(this));
		}
		bool newConfig = !Config.Exists();
		bool newData = !Data.Exists();
		if (!Config.Exists())
		{
			ModuleConfiguration = new Configuration
			{
				Config = Activator.CreateInstance<C>()
			};
			if (EnabledByDefault)
			{
				ModuleConfiguration.Enabled = true;
			}
			flag = true;
		}
		else
		{
			try
			{
				ModuleConfiguration = Config.ReadObject<Configuration>();
				if (ConfigVersionChecks && ModuleConfiguration.HasConfigStructureChanged())
				{
					flag = true;
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Failed loading config. JSON file is corrupted and/or invalid.", ex);
			}
		}
		ConfigInstance = ModuleConfiguration.Config;
		if (ForceEnabled)
		{
			ModuleConfiguration.Enabled = true;
		}
		RefreshEnabledCache();
		if (typeof(D) != typeof(EmptyModuleData))
		{
			if (!Data.Exists())
			{
				DataInstance = Activator.CreateInstance<D>();
				flag = true;
			}
			else
			{
				try
				{
					DataInstance = Data.ReadObject<D>();
				}
				catch (Exception ex2)
				{
					Logger.Error("Failed loading data. JSON file is corrupted and/or invalid.", ex2);
				}
			}
		}
		if (PreLoadShouldSave(newConfig, newData))
		{
			flag = true;
		}
		if (flag)
		{
			Save();
		}
	}

	public override void Save()
	{
		if (!ForceDisabled)
		{
			if (ModuleConfiguration == null)
			{
				ModuleConfiguration = new Configuration
				{
					Config = Activator.CreateInstance<C>()
				};
				ConfigInstance = ModuleConfiguration.Config;
			}
			if (DataInstance == null && typeof(D) != typeof(EmptyModuleData))
			{
				DataInstance = Activator.CreateInstance<D>();
			}
			if (ForceEnabled)
			{
				ModuleConfiguration.Enabled = true;
			}
			RefreshEnabledCache();
			Config.WriteObject(ModuleConfiguration);
			if (DataInstance != null)
			{
				Data?.WriteObject(DataInstance);
			}
		}
	}

	public override void Reload()
	{
		if (ForceDisabled)
		{
			return;
		}
		try
		{
			SetEnabled(enable: false);
		}
		catch (Exception ex)
		{
			Logger.Error("Failed module Disable for " + Name + " [Reload Request]", ex);
		}
		try
		{
			Load();
		}
		catch (Exception ex2)
		{
			Logger.Error("Failed module Load for " + Name + " [Reload Request]", ex2);
		}
		try
		{
			if (IsEnabled())
			{
				SetEnabled(enable: true);
			}
		}
		catch (Exception ex3)
		{
			Logger.Error("Failed module Enable for " + Name + " [Reload Request]", ex3);
		}
	}

	public virtual bool PreLoadShouldSave(bool newConfig, bool newData)
	{
		return false;
	}

	public virtual string GetConfigPath()
	{
		return Path.Combine(Defines.GetModulesFolder(), Name, "config.json");
	}

	public virtual string GetDataPath()
	{
		return Path.Combine(Defines.GetModulesFolder(), Name, "data.json");
	}

	public override void SetEnabled(bool enable)
	{
		if (ForceDisabled)
		{
			return;
		}
		if (ModuleConfiguration != null)
		{
			ModuleConfiguration.Enabled = enable;
			RefreshEnabledCache();
			OnEnableStatus();
		}
		if (!enable || !Community.IsServerInitialized)
		{
			return;
		}
		try
		{
			OnServerInit(initial: false);
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed OnServerInit on '{Name} v{Version}'", ex);
		}
		try
		{
			OnPostServerInit(initial: false);
		}
		catch (Exception ex2)
		{
			Logger.Error($"Failed OnPostServerInit on '{Name} v{Version}'", ex2);
		}
	}

	public override bool IsEnabled()
	{
		return _isEnabledCached;
	}

	private void RefreshEnabledCache()
	{
		_isEnabledCached = !ForceDisabled && (ModuleConfiguration?.Enabled ?? false);
	}

	public virtual void OnDisabled(bool initialized)
	{
		if (!ForceDisabled)
		{
			OnUnload();
		}
	}

	public virtual void OnEnabled(bool initialized)
	{
		if (ForceDisabled)
		{
			return;
		}
		if (!ManualCommands)
		{
			ModLoader.ProcessCommands(Type, this, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}
		if (!ManualSubscriptions)
		{
			SubscribeAll();
			if (!Community.Runtime.Config.Logging.ReducedLogging && Hooks.Count > 0)
			{
				Puts(string.Format("Subscribed to {0:n0} {1}.", Hooks.Count, Hooks.Count.Plural("hook", "hooks")));
			}
		}
		ApplyOrderedPatches(AutoPatchAttribute.Orders.AfterPluginInit);
	}

	public void OnEnableStatus()
	{
		if (ForceDisabled)
		{
			return;
		}
		try
		{
			if (ModuleConfiguration != null)
			{
				if (ModuleConfiguration.Enabled)
				{
					OnEnabled(Community.IsServerInitialized);
				}
				else
				{
					OnDisabled(Community.IsServerInitialized);
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Error("Failed " + (ModuleConfiguration.Enabled ? "Enable" : "Disable") + " initialization.", ex);
		}
	}

	public override void OnServerSaved()
	{
	}

	public override void OnServerInit(bool initial)
	{
	}

	public override void OnPostServerInit(bool initial)
	{
	}

	public override void OnUnload()
	{
		ModLoader.RemoveCommands(this);
		UnsubscribeAll();
		Permissions?.UnregisterPermissions(this);
		if (!Community.Runtime.Config.Logging.ReducedLogging && Hooks.Count > 0)
		{
			Puts(string.Format("Unsubscribed from {0:n0} {1}.", Hooks.Count, Hooks.Count.Plural("hook", "hooks")));
		}
		UnapplyOrderedPatches(AutoPatchAttribute.Orders.Delayed, silent: false);
		UnapplyOrderedPatches(AutoPatchAttribute.Orders.AfterOnServerInitialized, silent: false);
		UnapplyOrderedPatches(AutoPatchAttribute.Orders.AfterPluginLoad, silent: false);
		UnapplyOrderedPatches(AutoPatchAttribute.Orders.AfterPluginInit, silent: false);
	}

	public override void Shutdown()
	{
		if (IsEnabled())
		{
			OnUnload();
		}
		Community.Runtime.ModuleProcessor.Uninstall(this);
	}

	public override void SetPermissions(Permission perms)
	{
		Permissions = perms;
	}

	public virtual Dictionary<string, Dictionary<string, string>> GetDefaultPhrases()
	{
		return null;
	}

	public virtual string GetPhrase(string key)
	{
		return Lang.GetMessage(key, this);
	}

	public virtual string GetPhrase(string key, string playerId)
	{
		return Lang.GetMessage(key, this, playerId);
	}

	public virtual string GetPhrase(string key, ulong playerId)
	{
		return Lang.GetMessage(key, this, (playerId == 0L) ? string.Empty : playerId.ToString());
	}

	public void NextFrame(Action callback)
	{
		Community.Runtime.Core.NextFrame(callback);
	}
}

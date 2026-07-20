using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using API.Abstracts;
using API.Analytics;
using API.Commands;
using API.Events;
using API.Hooks;
using Carbon.Base;
using Carbon.Base.Interfaces;
using Carbon.Components;
using Carbon.Contracts;
using Carbon.Extensions;
using Carbon.Modules;
using Carbon.OxideRefs;
using Carbon.Plugins;
using Carbon.Pooling;
using Carbon.Profiler;
using Carbon.Test;
using ConVar;
using Facepunch;
using HarmonyLib;
using Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using Oxide.Game.Rust.Libraries;
using Oxide.Game.Rust.Libraries.Covalence;
using Oxide.Plugins;
using Rust;
using Rust.Ai.Gen2;
using UnityEngine;

namespace Carbon.Core;

public class CorePlugin : CarbonPlugin
{
	[AutoPatch(Silent = true)]
	[HarmonyPatch]
	public class GivePatch
	{
		public static List<BasePlayer> giverPlayers = new List<BasePlayer>();

		public static IEnumerable<MethodBase> TargetMethods()
		{
			Type[] parameters = new Type[1] { typeof(Arg) };
			yield return AccessTools.Method(typeof(Inventory), "give", parameters, (Type[])null);
			yield return AccessTools.Method(typeof(Inventory), "giveall", parameters, (Type[])null);
			yield return AccessTools.Method(typeof(Inventory), "givearm", parameters, (Type[])null);
			yield return AccessTools.Method(typeof(Inventory), "giveBp", parameters, (Type[])null);
			yield return AccessTools.Method(typeof(Inventory), "giveid", parameters, (Type[])null);
			yield return AccessTools.Method(typeof(Inventory), "giveto", parameters, (Type[])null);
		}

		public static void Prefix(Arg arg)
		{
			if (Community.Runtime.Core.NoGiveNoticesCache)
			{
				BasePlayer val = ArgEx.Player(arg);
				if ((Object)(object)val != (Object)null && !giverPlayers.Contains(val))
				{
					giverPlayers.Add(val);
				}
			}
		}
	}

	[AutoPatch(Silent = true)]
	[HarmonyPatch(typeof(Item), "SetItemOwnership", new Type[]
	{
		typeof(BasePlayer),
		typeof(Phrase)
	})]
	public class OwnershipPatch
	{
		public static bool Prefix(BasePlayer player, Phrase reason)
		{
			if (GivePatch.giverPlayers.Contains(player))
			{
				GivePatch.giverPlayers.Remove(player);
				return false;
			}
			return true;
		}
	}

	public class ArgPool
	{
		public static readonly int DefaultCapacity = 10;

		private readonly int length;

		private readonly Stack<string[]> pool;

		private readonly object syncRoot = new object();

		private int rentedExtra;

		private int rented;

		private int returned;

		public int RentedExtra => rentedExtra;

		public int Rented => rented;

		public int Returned => returned;

		public int Length => length;

		public int Count => pool.Count;

		public ArgPool(int length)
		{
			this.length = length;
			rented = 0;
			returned = 0;
			rentedExtra = 0;
			pool = new Stack<string[]>(DefaultCapacity);
			for (int i = 0; i < DefaultCapacity; i++)
			{
				pool.Push(new string[length]);
			}
		}

		public string[] Rent()
		{
			lock (syncRoot)
			{
				if (pool.Count > 0)
				{
					rented++;
					return pool.Pop();
				}
				rentedExtra++;
				return new string[length];
			}
		}

		public void Return(string[] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = null;
			}
			lock (syncRoot)
			{
				returned++;
				pool.Push(array);
			}
		}
	}

	[AutoPatch(Silent = true)]
	[HarmonyPatch(typeof(SaveRestore), "ShiftSaveBackups", new Type[] { typeof(string) })]
	public class Save
	{
		public static void Prefix(string fileName)
		{
			StoredModifiers.Save();
		}
	}

	[AutoPatch(Silent = true)]
	[HarmonyPatch(typeof(BaseEntity), "Save")]
	public class BaseEntity_Save
	{
		public static void Postfix(BaseEntity __instance, SaveInfo info)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			if (info.forDisk && __instance.networkEntityScale)
			{
				info.msg.baseEntity.scale = ((Component)__instance).transform.localScale;
			}
		}
	}

	[AutoPatch(Silent = true)]
	[HarmonyPatch(typeof(BaseEntity), "Load")]
	public class BaseEntity_Load
	{
		public static void Prefix(BaseEntity __instance, LoadInfo info)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			if (info.fromDisk && info.msg.baseEntity != null && !(info.msg.baseEntity.scale == default(Vector3)))
			{
				((Component)__instance).transform.localScale = info.msg.baseEntity.scale;
				__instance.networkEntityScale = true;
			}
		}
	}

	public struct ProcessableFile
	{
		public enum Types
		{
			Script,
			CSZIP,
			CSZIP_Dev
		}

		public string Id;

		public string Path;

		public Types Type;

		public IBaseProcessor GetProcessor()
		{
			return Type switch
			{
				Types.Script => Community.Runtime.ScriptProcessor, 
				Types.CSZIP => Community.Runtime.ZipScriptProcessor, 
				_ => null, 
			};
		}
	}

	[CarbonAutoModdedVar("recycletickmultiplier", "Recycle Tick (*)", "Configures the recycling ticks multiplier base speed relative.", false, false)]
	[AuthLevel(2)]
	public float RecycleTickMultiplier = -1f;

	[CarbonAutoModdedVar("safezonerecycletickmultiplier", "Recycle Tick (Safezone) (*)", "Configures the SafeZone recycling ticks multiplier base speed relative.", false, false)]
	[AuthLevel(2)]
	public float SafezoneRecycleTickMultiplier = -1f;

	[CarbonAutoModdedVar("researchdurationmultiplier", "Researching Duration (*)", "The duration multiplier of blueprint researching finalization time.", false, false)]
	[AuthLevel(2)]
	public float ResearchDurationMultiplier = -1f;

	[CarbonAutoModdedVar("vendingbuydurationmultiplier", "Vending Buy Duration (*)", "The duration multiplier of transaction delay when buying from vending machines.", false, false)]
	[AuthLevel(2)]
	public float VendingMachineBuyDurationMultiplier = -1f;

	[CarbonAutoModdedVar("craftingspeedmultiplier_nowb", "Crafting Speed - No WB (*)", "The time multiplier of crafting items without a workbench.", false, false)]
	[AuthLevel(2)]
	public float CraftingSpeedMultiplierNoWB = -1f;

	[CarbonAutoModdedVar("craftingspeedmultiplier_wb1", "Crafting Speed - WB 1 (*)", "The time multiplier of crafting items at workbench level 1.", false, false)]
	[AuthLevel(2)]
	public float CraftingSpeedMultiplierWB1 = -1f;

	[CarbonAutoModdedVar("craftingspeedmultiplier_wb2", "Crafting Speed - WB 2 (*)", "The time multiplier of crafting items at workbench level 2.", false, false)]
	[AuthLevel(2)]
	public float CraftingSpeedMultiplierWB2 = -1f;

	[CarbonAutoModdedVar("craftingspeedmultiplier_wb3", "Crafting Speed - WB 3 (*)", "The time multiplier of crafting items at workbench level 3.", false, false)]
	[AuthLevel(2)]
	public float CraftingSpeedMultiplierWB3 = -1f;

	[CarbonAutoModdedVar("mixingspeedmultiplier", "Mixing Speed (*)", "The speed multiplier of mixing table crafts.", false, false)]
	[AuthLevel(2)]
	public float MixingSpeedMultiplier = -1f;

	[CarbonAutoModdedVar("exacavatorresourcetickratemultiplier", "Excavator Resource Rate (*)", "Excavator resource tick multiplier rate.", false, false)]
	[AuthLevel(2)]
	public float ExcavatorResourceTickRateMultiplier = -1f;

	[CarbonAutoModdedVar("excavatortimeforfullresourcesmultiplier", "Excavator Full Resources Time (*)", "Excavator time multiplier for processing full resources.", false, false)]
	[AuthLevel(2)]
	public float ExcavatorTimeForFullResourcesMultiplier = -1f;

	[CarbonAutoModdedVar("excavatorbeltspeedmaxmultiplier", "Excavator Belt Max. Speed (*)", "Excavator belt maximum speed multiplier.", false, false)]
	[AuthLevel(2)]
	public float ExcavatorBeltSpeedMaxMultiplier = -1f;

	public List<string> OvenBlacklistCache = new List<string>();

	private string _ovenBlacklist = "furnace,bbq.static,furnace.large";

	[CarbonAutoModdedVar("ovenspeedmultiplier", "Oven Speed (*)", "The burning speed multiplier of ovens.", false, false)]
	[AuthLevel(2)]
	public float OvenSpeedMultiplier = -1f;

	[CarbonAutoModdedVar("ovenblacklistspeedmultiplier", "Oven Blacklist Speed Duration (*)", "The burning speed multiplier of blacklisted ovens.", false, false)]
	[AuthLevel(2)]
	public float OvenBlacklistSpeedMultiplier = -1f;

	public bool NoTechTreeUnlockCache;

	public bool NoGiveNoticesCache;

	public bool NoAdminChatColorCache;

	public bool NoDevChatColorCache;

	private string _customMapName = "-1";

	[CarbonAutoVar("defaultserverchatname", "Server Chat Name", "Default server chat name.", false, false)]
	[AuthLevel(2)]
	public string DefaultServerChatName = "-1";

	[CarbonAutoVar("defaultserverchatcolor", "Server Chat Color", "Default server chat message name color.", false, false)]
	[AuthLevel(2)]
	public string DefaultServerChatColor = "-1";

	[CarbonAutoVar("defaultserverchatid", "Server Icon ID", "Default server chat icon SteamID.", false, false)]
	[AuthLevel(2)]
	public long DefaultServerChatId = -1L;

	public static MonoProfiler.Sample ProfileSample = MonoProfiler.Sample.Create();

	private static Dictionary<int, ArgPool> _argumentBuffer = new Dictionary<int, ArgPool>(ArgPool.DefaultCapacity);

	internal static bool _isPlayerTakingDamage = false;

	internal static readonly string[] _emptyStringArray = new string[0];

	private static readonly DateTime Eoy = new DateTime(2026, 12, 31);

	internal static readonly string OnEntitySaved = "OnEntitySaved";

	internal const string _blankZero = "0";

	internal const string _blankUnnamed = "Unnamed";

	internal static StackTraceLogType _defaultLogTrace;

	internal static StackTraceLogType _defaultWarningTrace;

	internal static StackTraceLogType _defaultErrorTrace;

	internal static StackTraceLogType _defaultAssertTrace;

	internal static StackTraceLogType _defaultExceptionTrace;

	[CommandVar("isforcemodded", "Is the server forcefully set to modded due to options affecting significant gameplay changes in Carbon Auto?")]
	[AuthLevel(2)]
	public bool IsForceModded
	{
		get
		{
			return API.Abstracts.CarbonAuto.Singleton.IsForceModded();
		}
		set
		{
		}
	}

	public float RuntimeResearchDurationMultiplier
	{
		get
		{
			if (ResearchDurationMultiplier != -1f)
			{
				return ResearchDurationMultiplier;
			}
			return 1f;
		}
	}

	[CarbonAutoVar("ovenblacklist", "Oven Blacklist", "Blacklisted oven entity prefabs.", false, false)]
	[AuthLevel(2)]
	public string OvenBlacklist
	{
		get
		{
			return _ovenBlacklist;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				OvenBlacklistCache.Clear();
				return;
			}
			if (_ovenBlacklist != value || OvenBlacklistCache.Count == 0)
			{
				OvenBlacklistCache.Clear();
				OvenBlacklistCache.AddRange(value.SplitEnumerable(','));
			}
			_ovenBlacklist = value;
		}
	}

	[CarbonAutoVar("notechtreeunlock", "No TechTree Unlocks", "Players will no longer be able to progress on any tech trees.", false, false)]
	[AuthLevel(2)]
	public string NoTechTreeUnlock
	{
		get
		{
			if (!NoTechTreeUnlockCache)
			{
				return "-1";
			}
			return "1";
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				NoTechTreeUnlockCache = false;
			}
			else
			{
				NoTechTreeUnlockCache = value.ToBool();
			}
		}
	}

	[CarbonAutoVar("nogivenotices", "No 'Give' Notices", "Will prohibit 'gave' messages to be printed to chat when admins give items.", false, false)]
	[AuthLevel(2)]
	public string NoGiveNotices
	{
		get
		{
			if (!NoGiveNoticesCache)
			{
				return "-1";
			}
			return "1";
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				NoGiveNoticesCache = false;
			}
			else
			{
				NoGiveNoticesCache = value.ToBool();
			}
		}
	}

	[CarbonAutoVar("noadminchatcolor", "No Admin Chat Coloring", "Will make admins' nicknames look the same as the player (no green, from #af5 to #5af).", false, false)]
	[AuthLevel(2)]
	public string NoAdminChatColor
	{
		get
		{
			if (!NoAdminChatColorCache)
			{
				return "-1";
			}
			return "1";
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				NoAdminChatColorCache = false;
			}
			else
			{
				NoAdminChatColorCache = value.ToBool();
			}
		}
	}

	[CarbonAutoVar("nodevchatcolor", "No Developer Chat Coloring", "Will make facepunch developers' nicknames look the same as the player (no orange, from #fa5 to #5af).", false, false)]
	[AuthLevel(2)]
	public string NoDevChatColor
	{
		get
		{
			if (!NoDevChatColorCache)
			{
				return "-1";
			}
			return "1";
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				NoDevChatColorCache = false;
			}
			else
			{
				NoDevChatColorCache = value.ToBool();
			}
		}
	}

	[CarbonAutoVar("custommapname", "Custom Map Name", "The map name displayed in the Rust server browser. Shouldn't be longer than 64 characters.", false, false)]
	[AuthLevel(2)]
	public string CustomMapName
	{
		get
		{
			return _customMapName;
		}
		set
		{
			_customMapName = StringEx.Truncate(value, 64);
		}
	}

	[CommandVar("developermode", "Enables developer mode which grants a few features that are designed and used by the developers.")]
	[AuthLevel(2)]
	private bool DeveloperMode
	{
		get
		{
			return Community.Runtime.Config.DeveloperMode;
		}
		set
		{
			Community.Runtime.Config.DeveloperMode = value;
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("modding", "Mark this server as modded or not.")]
	[AuthLevel(2)]
	private bool Modding
	{
		get
		{
			return Community.Runtime.Config.IsModded;
		}
		set
		{
			Community.Runtime.Config.IsModded = value;
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("scriptwatchers", "When disabled, you must load/unload plugins manually with `c.load` or `c.unload`.")]
	[AuthLevel(2)]
	private bool ScriptWatchers
	{
		get
		{
			return Community.Runtime.Config.Watchers.ScriptWatchers;
		}
		set
		{
			Community.Runtime.Config.Watchers.ScriptWatchers = value;
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("scriptwatchersoption", "Indicates wether the script watcher (whenever enabled) listens to the 'carbon/plugins' folder only, or its subfolders. (0 = Top-only directories, 1 = All directories)")]
	[AuthLevel(2)]
	private int ScriptWatchersOption
	{
		get
		{
			return (int)Community.Runtime.Config.Watchers.ScriptWatcherOption;
		}
		set
		{
			Community.Runtime.Config.Watchers.ScriptWatcherOption = (SearchOption)value;
			Community.Runtime.ScriptProcessor.IncludeSubdirectories = value == 1;
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("debug", "The level of debug logging for Carbon. Helpful for very detailed logs in case things break. (Set it to -1 to disable debug logging.)")]
	[AuthLevel(2)]
	private int CarbonDebug
	{
		get
		{
			return Community.Runtime.Config.Logging.LogVerbosity;
		}
		set
		{
			Community.Runtime.Config.Logging.LogVerbosity = value;
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("logfiletype", "The mode for writing the log to file. (0=disabled, 1=saves updates every 5 seconds, 2=saves immediately)")]
	[AuthLevel(2)]
	private int LogFileType
	{
		get
		{
			return Community.Runtime.Config.Logging.LogFileMode;
		}
		set
		{
			Community.Runtime.Config.Logging.LogFileMode = Mathf.Clamp(value, 0, 2);
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("language", "Server language used by the Language API.")]
	[AuthLevel(2)]
	private string Language
	{
		get
		{
			return Community.Runtime.Config.Language;
		}
		set
		{
			Community.Runtime.Config.Language = value;
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("unloadonfailure", "Unload already loaded plugins when recompilation attempt fails. (Disabled by default)")]
	[AuthLevel(2)]
	private bool UnloadOnFailure
	{
		get
		{
			return Community.Runtime.Config.Compiler.UnloadOnFailure;
		}
		set
		{
			Community.Runtime.Config.Compiler.UnloadOnFailure = value;
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("bypassadmincooldowns", "Bypasses the command cooldowns for admin-authed players.")]
	[AuthLevel(2)]
	private bool BypassAdminCooldowns
	{
		get
		{
			return Community.Runtime.Config.Permissions.BypassAdminCooldowns;
		}
		set
		{
			Community.Runtime.Config.Permissions.BypassAdminCooldowns = value;
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("logsplitsize", "The size for each log (in megabytes) required for it to be split into separate chunks.")]
	[AuthLevel(2)]
	private double LogSplitSize
	{
		get
		{
			return Community.Runtime.Config.Logging.LogSplitSize;
		}
		set
		{
			Community.Runtime.Config.Logging.LogSplitSize = value;
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("scriptprocessrate", "The speed of detecting local file changes for items in the carbon/plugins directory.")]
	[AuthLevel(2)]
	private float ScriptProcessingRate
	{
		get
		{
			return Community.Runtime.Config.Processors.ScriptProcessingRate;
		}
		set
		{
			Community.Runtime.Config.Processors.ScriptProcessingRate = value;
			Community.Runtime.ScriptProcessor.RefreshRate();
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("zipscriptprocessrate", "The speed of detecting local file changes for zipscript items in the carbon/plugins directory.")]
	[AuthLevel(2)]
	private float ZipScriptProcessingRate
	{
		get
		{
			return Community.Runtime.Config.Processors.ZipScriptProcessingRate;
		}
		set
		{
			Community.Runtime.Config.Processors.ZipScriptProcessingRate = value;
			Community.Runtime.ZipScriptProcessor.RefreshRate();
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("consoleinfo", "Show the Windows-only Carbon information at the bottom of the console.")]
	[AuthLevel(2)]
	private bool ConsoleInfo
	{
		get
		{
			return Community.Runtime.Config.Misc.ShowConsoleInfo;
		}
		set
		{
			Community.Runtime.Config.Misc.ShowConsoleInfo = value;
			if (value)
			{
				Community.Runtime.RefreshConsoleInfo();
			}
			else if ((Object)(object)SingletonComponent<ServerConsole>.Instance != (Object)null && SingletonComponent<ServerConsole>.Instance.input != null)
			{
				SingletonComponent<ServerConsole>.Instance.input.statusText = new string[3];
			}
		}
	}

	[CommandVar("trackhookmemory", "Tracks the memory usage of hooks.")]
	[AuthLevel(2)]
	private bool TrackHookMemory
	{
		get
		{
			return Community.Runtime.Config.Debugging.TrackHookMemory;
		}
		set
		{
			Community.Runtime.Config.Debugging.TrackHookMemory = value;
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("hooklsthreshold", "The threshold value used by the hook caller to determine what minimum time is considered as a server lag spike. Defaults to 1000ms.")]
	[AuthLevel(2)]
	private int HookLagSpikeThreshold
	{
		get
		{
			return Community.Runtime.Config.Debugging.HookLagSpikeThreshold;
		}
		set
		{
			Community.Runtime.Config.Debugging.HookLagSpikeThreshold = value.Clamp(100, 10000);
		}
	}

	[CommandVar("lang", "Current server language for Carbon and plugins loaded.")]
	[AuthLevel(2)]
	private string Lang
	{
		get
		{
			return lang.GetServerLanguage();
		}
		set
		{
			lang.SetServerLanguage(value);
		}
	}

	[CommandVar("default_player_group", "The default group for any player with the regular authority level they get assigned to.")]
	[AuthLevel(2)]
	private string DefaultPlayerGroup
	{
		get
		{
			return Community.Runtime.Config.Permissions.PlayerDefaultGroup;
		}
		set
		{
			Community.Runtime.Config.Permissions.PlayerDefaultGroup = value;
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("default_admin_group", "The default group players with auth-level 2 get assigned to.")]
	[AuthLevel(2)]
	private string DefaultAdminGroup
	{
		get
		{
			return Community.Runtime.Config.Permissions.AdminDefaultGroup;
		}
		set
		{
			Community.Runtime.Config.Permissions.AdminDefaultGroup = value;
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("default_mod_group", "The default group players with auth-level 1 get assigned to.")]
	[AuthLevel(2)]
	private string DefaultModeratorGroup
	{
		get
		{
			return Community.Runtime.Config.Permissions.ModeratorDefaultGroup;
		}
		set
		{
			Community.Runtime.Config.Permissions.ModeratorDefaultGroup = value;
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("autogrant_player_group", "Carbon should automatically grant (newer) players the default player group to them.")]
	[AuthLevel(2)]
	private bool AutoGrantPlayerGroup
	{
		get
		{
			return Community.Runtime.Config.Permissions.AutoGrantPlayerGroup;
		}
		set
		{
			Community.Runtime.Config.Permissions.AutoGrantPlayerGroup = value;
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("autogrant_admin_group", "Carbon should automatically grant (auth level 2) players the default admin group to them.")]
	[AuthLevel(2)]
	private bool AutoGrantAdminGroup
	{
		get
		{
			return Community.Runtime.Config.Permissions.AutoGrantAdminGroup;
		}
		set
		{
			Community.Runtime.Config.Permissions.AutoGrantAdminGroup = value;
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("autogrant_mod_group", "Carbon should automatically grant (auth level 1) players the default moderator group to them.")]
	[AuthLevel(2)]
	private bool AutoGrantModeratorGroup
	{
		get
		{
			return Community.Runtime.Config.Permissions.AutoGrantModeratorGroup;
		}
		set
		{
			Community.Runtime.Config.Permissions.AutoGrantModeratorGroup = value;
			Community.Runtime.SaveConfig();
		}
	}

	[CommandVar("profilestatus", "Mono profiling status.")]
	[AuthLevel(2)]
	private bool IsProfiling
	{
		get
		{
			return MonoProfiler.IsRecording;
		}
		set
		{
		}
	}

	[CommandVar("profiler.recwarns", "It should or should not print a reminding warning every 5 minutes when profiling for an un-set amount of time.")]
	[AuthLevel(2)]
	private bool RecordingWarnings
	{
		get
		{
			return Community.Runtime.Config.Profiler.RecordingWarnings;
		}
		set
		{
			Community.Runtime.Config.Profiler.RecordingWarnings = value;
		}
	}

	[CommandVar("webpanel.connected", "Is the WebControlPanel server connected")]
	[AuthLevel(2)]
	private bool IsWebControlPanelServerConnected => WebControlPanel.server?.IsConnected() ?? false;

	public static List<ProcessableFile> ProcessableFiles { get; } = new List<ProcessableFile>();

	[Conditional("!MINIMAL")]
	internal object IRecyclerThinkSpeed(Recycler recycler)
	{
		if (recycler.IsSafezoneRecycler())
		{
			if (SafezoneRecycleTickMultiplier != -1f)
			{
				return SafezoneRecycleTickMultiplier;
			}
			return null;
		}
		if (RecycleTickMultiplier != -1f)
		{
			return RecycleTickMultiplier;
		}
		return null;
	}

	[Conditional("!MINIMAL")]
	internal object ICraftDurationMultiplier(ItemBlueprint bp, float workbenchLevel, bool isInTutorial)
	{
		if (isInTutorial)
		{
			return null;
		}
		float num = workbenchLevel - (float)bp.workbenchLevelRequired;
		float num2 = num;
		if (num2 != 0f)
		{
			if (num2 != 1f)
			{
				if (num2 != 2f)
				{
					if (num2 == 3f && CraftingSpeedMultiplierWB3 != -1f)
					{
						return CraftingSpeedMultiplierWB3;
					}
				}
				else if (CraftingSpeedMultiplierWB2 != -1f)
				{
					return CraftingSpeedMultiplierWB2;
				}
			}
			else if (CraftingSpeedMultiplierWB1 != -1f)
			{
				return CraftingSpeedMultiplierWB1;
			}
		}
		else if (CraftingSpeedMultiplierNoWB != -1f)
		{
			return CraftingSpeedMultiplierNoWB;
		}
		return null;
	}

	[Conditional("!MINIMAL")]
	internal object IMixingSpeedMultiplier(MixingTable table, float originalValue)
	{
		if (MixingSpeedMultiplier == -1f || (Object)(object)table.currentRecipe == (Object)null)
		{
			return null;
		}
		if (originalValue == table.currentRecipe.MixingDuration * (float)table.currentQuantity)
		{
			return MixingSpeedMultiplier;
		}
		return null;
	}

	[Conditional("!MINIMAL")]
	internal object IVendingBuyDuration()
	{
		if (VendingMachineBuyDurationMultiplier != -1f)
		{
			return VendingMachineBuyDurationMultiplier;
		}
		return null;
	}

	[Conditional("!MINIMAL")]
	internal void IOnExcavatorInit(ExcavatorArm arm)
	{
		if (ExcavatorResourceTickRateMultiplier != -1f)
		{
			arm.resourceProductionTickRate *= ExcavatorResourceTickRateMultiplier;
		}
		if (ExcavatorTimeForFullResourcesMultiplier != -1f)
		{
			arm.timeForFullResources *= ExcavatorTimeForFullResourcesMultiplier;
		}
		if (ExcavatorBeltSpeedMaxMultiplier != -1f)
		{
			arm.beltSpeedMax *= ExcavatorBeltSpeedMaxMultiplier;
		}
	}

	[Conditional("!MINIMAL")]
	internal object IOvenSmeltSpeedMultiplier(BaseOven oven)
	{
		bool flag = false;
		if (OvenBlacklistCache != null)
		{
			for (int i = 0; i < OvenBlacklistCache.Count; i++)
			{
				string text = OvenBlacklistCache[i];
				if (StringEx.Contains(((BaseNetworkable)oven).ShortPrefabName, text, CompareOptions.IgnoreCase))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			if (OvenBlacklistSpeedMultiplier != -1f)
			{
				return OvenBlacklistSpeedMultiplier;
			}
			return null;
		}
		if (OvenSpeedMultiplier != -1f)
		{
			return OvenSpeedMultiplier;
		}
		return null;
	}

	[Conditional("!MINIMAL")]
	private void IResearchDuration()
	{
	}

	[Conditional("!MINIMAL")]
	private object CanUnlockTechTreeNode()
	{
		if (NoTechTreeUnlockCache)
		{
			return false;
		}
		return null;
	}

	[ConsoleCommand("find", "Searches through Carbon-processed console commands.")]
	[AuthLevel(2)]
	private void Find(Arg arg)
	{
		StringTable stringTable = new StringTable("command", "value", "help");
		try
		{
			string value = ((arg.Args != null && arg.Args.Length != 0) ? arg.GetString(0, "") : null);
			foreach (API.Commands.Command item in Community.Runtime.CommandManager.ClientConsole)
			{
				if (item.HasFlag(CommandFlags.Hidden) || (!string.IsNullOrEmpty(value) && !item.Name.Contains(value)))
				{
					continue;
				}
				string text = " ";
				string text2 = string.Empty;
				if (item.Token != null)
				{
					if (item.Token is FieldInfo fieldInfo)
					{
						text = fieldInfo.GetValue(item.Reference)?.ToString();
					}
					else if (item.Token is PropertyInfo propertyInfo)
					{
						text = propertyInfo.GetValue(item.Reference)?.ToString();
					}
				}
				if (item.HasFlag(CommandFlags.Protected))
				{
					text = new string('*', text.Length);
				}
				if (item.Token != null)
				{
					object token = item.Token;
					if (!(token is FieldInfo element))
					{
						if (token is PropertyInfo element2)
						{
							CarbonAutoVar customAttribute = element2.GetCustomAttribute<CarbonAutoVar>();
							if (customAttribute != null && customAttribute.ForceModded)
							{
								goto IL_0170;
							}
						}
					}
					else
					{
						CarbonAutoVar customAttribute2 = element.GetCustomAttribute<CarbonAutoVar>();
						if (customAttribute2 != null && customAttribute2.ForceModded)
						{
							goto IL_0170;
						}
					}
				}
				goto IL_017e;
				IL_0170:
				text2 += " Marks the server to be modded.";
				goto IL_017e;
				IL_017e:
				stringTable.AddRow(" " + item.Name, text, item.Help + text2);
			}
			arg.ReplyWith(stringTable.Write(StringTable.FormatTypes.None));
		}
		finally
		{
			((IDisposable)stringTable/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ConsoleCommand("findchat", "Searches through Carbon-processed chat commands.")]
	[AuthLevel(2)]
	private void FindChat(Arg arg)
	{
		StringTable stringTable = new StringTable("command", "help");
		try
		{
			string value = ((arg.Args != null && arg.Args.Length != 0) ? arg.GetString(0, "") : null);
			foreach (API.Commands.Command item in Community.Runtime.CommandManager.Chat)
			{
				if (!item.HasFlag(CommandFlags.Hidden) && (string.IsNullOrEmpty(value) || item.Name.Contains(value)))
				{
					stringTable.AddRow(" " + item.Name, item.Help);
				}
			}
			arg.ReplyWith(stringTable.Write(StringTable.FormatTypes.None));
		}
		finally
		{
			((IDisposable)stringTable/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ConsoleCommand("moddedvars", "Prints a table/list of all modified Rust ConVars.")]
	[AuthLevel(2)]
	private void ModdedRustConVars(Arg arg)
	{
		StringTable stringTable = new StringTable("variable", "value", "original_value");
		try
		{
			string value = ((arg.Args != null && arg.Args.Length != 0) ? arg.GetString(0, "") : null);
			foreach (KeyValuePair<string, ConVarSnapshots.Snapshot> snapshot in ConVarSnapshots.Snapshots)
			{
				if (string.IsNullOrEmpty(value) || snapshot.Key.Contains(value))
				{
					string text = snapshot.Value.Field.Key.GetValue(null)?.ToString();
					string text2 = snapshot.Value.Value?.ToString();
					if (text2 != text && !string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(text))
					{
						stringTable.AddRow(" " + snapshot.Key, text, text2);
					}
				}
			}
			arg.ReplyWith(stringTable.Write(StringTable.FormatTypes.None));
		}
		finally
		{
			((IDisposable)stringTable/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ConsoleCommand("addconditional", "Adds a new conditional compilation symbol to the compiler.")]
	[AuthLevel(2)]
	private void AddConditional(Arg arg)
	{
		string text = arg.GetString(0, "");
		if (!Community.Runtime.Config.Compiler.ConditionalCompilationSymbols.Contains(text))
		{
			Community.Runtime.Config.Compiler.ConditionalCompilationSymbols.Add(text);
			Community.Runtime.SaveConfig();
			arg.ReplyWith("Added conditional '" + text + "'.");
		}
		else
		{
			arg.ReplyWith("Conditional '" + text + "' already exists.");
		}
		for (int i = 0; i < ModLoader.Packages.Count; i++)
		{
			ModLoader.Package package = ModLoader.Packages[i];
			PooledList<RustPlugin> val = Pool.Get<PooledList<RustPlugin>>();
			try
			{
				((List<RustPlugin>)(object)val).AddRange((IEnumerable<RustPlugin>)package.Plugins);
				for (int j = 0; j < ((List<RustPlugin>)(object)val).Count; j++)
				{
					RustPlugin rustPlugin = ((List<RustPlugin>)(object)val)[j];
					if (rustPlugin.HasConditionals)
					{
						rustPlugin.ProcessorProcess.Dispose();
						rustPlugin.ProcessorProcess.Execute(rustPlugin.Processor);
						package.RemovePlugin(rustPlugin);
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	[ConsoleCommand("remconditional", "Removes an existent conditional compilation symbol from the compiler.")]
	[AuthLevel(2)]
	private void RemoveConditional(Arg arg)
	{
		string text = arg.GetString(0, "");
		if (Community.Runtime.Config.Compiler.ConditionalCompilationSymbols.Contains(text))
		{
			Community.Runtime.Config.Compiler.ConditionalCompilationSymbols.Remove(text);
			Community.Runtime.SaveConfig();
			arg.ReplyWith("Removed conditional '" + text + "'.");
		}
		else
		{
			arg.ReplyWith("Conditional '" + text + "' does not exist.");
		}
		for (int i = 0; i < ModLoader.Packages.Count; i++)
		{
			ModLoader.Package package = ModLoader.Packages[i];
			PooledList<RustPlugin> val = Pool.Get<PooledList<RustPlugin>>();
			try
			{
				((List<RustPlugin>)(object)val).AddRange((IEnumerable<RustPlugin>)package.Plugins);
				for (int j = 0; j < ((List<RustPlugin>)(object)val).Count; j++)
				{
					RustPlugin rustPlugin = ((List<RustPlugin>)(object)val)[j];
					if (rustPlugin.HasConditionals)
					{
						rustPlugin.ProcessorProcess.Dispose();
						rustPlugin.ProcessorProcess.Execute(rustPlugin.Processor);
						package.RemovePlugin(rustPlugin);
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	[ConsoleCommand("conditionals", "Prints a list of all conditional compilation symbols used by the compiler.")]
	[AuthLevel(2)]
	private void Conditionals(Arg arg)
	{
		arg.ReplyWith(string.Format("Conditionals ({0:n0}): {1}", Community.Runtime.Config.Compiler.ConditionalCompilationSymbols.Count, Community.Runtime.Config.Compiler.ConditionalCompilationSymbols.ToString(", ", " and ")));
	}

	[Conditional("!MINIMAL")]
	[ConsoleCommand("editconfig", "When ran by an admin client, the Carbon Admin module will open up a config editor.")]
	[AuthLevel(2)]
	private void EditConfig(Arg arg)
	{
		BasePlayer player = ArgEx.Player(arg);
		if (player == null)
		{
			arg.ReplyWith("Only admin clients can run this command");
			return;
		}
		string file = arg.GetString(0, "");
		if (!OsEx.File.Exists(file))
		{
			arg.ReplyWith("File '" + file + "' does not exist");
			return;
		}
		AdminModule.Singleton.SetTab(player, AdminModule.ConfigEditor.Make(OsEx.File.ReadText(file), delegate
		{
			AdminModule.Singleton.SetTab(player, "carbon");
			AdminModule.Singleton.Close(player);
		}, delegate(AdminModule.PlayerSession _, JObject jobj)
		{
			OsEx.File.Create(file, ((JToken)jobj).ToString((Formatting)1, Array.Empty<JsonConverter>()));
			AdminModule.Singleton.SetTab(player, "carbon");
			AdminModule.Singleton.Close(player);
		}, null, fullscreen: true));
	}

	[ConsoleCommand("loadconfig", "Loads Carbon config from file.")]
	[AuthLevel(2)]
	private void CarbonLoadConfig(Arg arg)
	{
		if (Community.Runtime != null)
		{
			Community.Runtime.LoadConfig();
			arg.ReplyWith("Loaded Carbon config.");
		}
	}

	[ConsoleCommand("saveconfig", "Saves Carbon config to file.")]
	[AuthLevel(2)]
	private void CarbonSaveConfig(Arg arg)
	{
		if (Community.Runtime != null)
		{
			Community.Runtime.SaveConfig();
			arg.ReplyWith("Saved Carbon config.");
		}
	}

	[ConsoleCommand("assignalias", "Assigns a new command alias. (Eg. c.assignalias myalias c.reload)")]
	[AuthLevel(2)]
	private void AssignAlias(Arg arg)
	{
		string alias = arg.GetString(0, "");
		string text = arg.GetString(1, "");
		if (string.IsNullOrEmpty(alias))
		{
			arg.ReplyWith("Alias cannot be null");
			return;
		}
		if (string.IsNullOrEmpty(text))
		{
			arg.ReplyWith("Alias command cannot be null");
			return;
		}
		if (alias.Equals(text, StringComparison.OrdinalIgnoreCase))
		{
			arg.ReplyWith("Don't be silly");
			return;
		}
		string text2 = (Index.All.Any((Command x) => x.FullName.Equals(alias, StringComparison.OrdinalIgnoreCase)) ? " (BEWARE! The alias you used is the name of an existent Rust command. Unassign this alias to make it accessible.)" : null);
		string value;
		if (!Community.Runtime.Config.IsValidAlias(alias, out var reason))
		{
			arg.ReplyWith("Invalid alias detected. Using '" + reason + "' is prohibited.");
		}
		else if (Community.Runtime.Config.Aliases.TryGetValue(alias, out value))
		{
			arg.ReplyWith("Overriding alias '" + alias + "' -> " + text + ":\n Old: " + value + text2);
			Community.Runtime.Config.Aliases[alias] = text;
			Community.Runtime.SaveConfig();
		}
		else
		{
			Community.Runtime.Config.Aliases[alias] = text;
			arg.ReplyWith("Assigned alias '" + alias + "' -> " + text + text2);
			Community.Runtime.SaveConfig();
		}
	}

	[ConsoleCommand("unassignalias", "Unassigns a command alias. (Eg. c.unassignalias myalias)")]
	[AuthLevel(2)]
	private void UnassignAlias(Arg arg)
	{
		string text = arg.GetString(0, "");
		if (string.IsNullOrEmpty(text))
		{
			arg.ReplyWith("Alias cannot be null");
			return;
		}
		if (!Community.Runtime.Config.Aliases.ContainsKey(text))
		{
			arg.ReplyWith("Alias '" + text + "' does not exist");
			return;
		}
		Community.Runtime.Config.Aliases.Remove(text);
		arg.ReplyWith("Unassigned alias '" + text + "'");
		Community.Runtime.SaveConfig();
	}

	[ConsoleCommand("aliases", "Prints the full list of aliases and respective redirected commands.")]
	[AuthLevel(2)]
	private void Aliases(Arg arg)
	{
		arg.ReplyWith(string.Format("Found {0:n0} {1}:\n{2}", Community.Runtime.Config.Aliases.Count, Community.Runtime.Config.Aliases.Count.Plural("alias", "aliases"), Community.Runtime.Config.Aliases.Select((KeyValuePair<string, string> x) => " " + x.Key + " -> " + x.Value).ToString("\n")));
	}

	[ConsoleCommand("changeversion", "It changes the current Carbon version you're running. Next reboot will swap to the overriden version. Run `c.changeversion` for syntax.")]
	[AuthLevel(2)]
	private void ChangeVersion(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			arg.ReplyWith("Version override change syntax:\neg. c.changeversion rustbeta_staging Debug\neg. c.changeversion production Minimal\neg. c.changeversion reset\nNOTE: When you've set the version override, self updating will enable itself automatically as it's required for the version change process.");
			return;
		}
		string file = Path.Combine(Defines.GetTempFolder(), "versionoverride.txt");
		if (arg.GetString(0, "").Equals("reset", StringComparison.OrdinalIgnoreCase))
		{
			OsEx.File.Delete(file);
			arg.ReplyWith("Reset version change. Next server reboot won't change your current Carbon version.");
			return;
		}
		string text = arg.GetString(0, "edge").Replace("_build", string.Empty);
		string text2 = arg.GetString(1, "Debug");
		string text3 = "Windows";
		string text4 = "zip";
		string content = "https://github.com/CarbonCommunity/Carbon/releases/download/" + text + "_build/Carbon." + text3 + "." + text2 + "." + text4;
		OsEx.File.Create(file, content);
		arg.ReplyWith("Overriding Carbon version to " + text + " (" + text2 + "). Next server reboot will swap to the overriden version.");
		if (!Community.Runtime.Config.SelfUpdating.Enabled)
		{
			Community.Runtime.Config.SelfUpdating.Enabled = true;
			Community.Runtime.SaveConfig();
		}
	}

	[ConsoleCommand("wipeui", "Clears the entire CUI containers and their elements from the caller's client.")]
	[AuthLevel(2)]
	private void WipeUI(Arg arg)
	{
		BasePlayer val = ArgEx.Player(arg);
		if (val != null)
		{
			arg.ReplyWith($"Cleared {CuiHelper.DestroyActivePanelList(val):n0} CUI panels");
		}
		else
		{
			arg.ReplyWith("This command can only be called from a client.");
		}
	}

	[ConsoleCommand("resethooks", "Clears all progress on all of the current hooks (hook time, fires, memory usage, exceptions and lag spikes).")]
	[AuthLevel(2)]
	private void ResetHooks(Arg arg)
	{
		foreach (RustPlugin item in ModLoader.Packages.SelectMany((ModLoader.Package package) => package.Plugins))
		{
			item.HookPool.Reset();
		}
		foreach (BaseHookable module in Community.Runtime.ModuleProcessor.Modules)
		{
			module.HookPool.Reset();
		}
		arg.ReplyWith("All plugin and module hook cache has been reset.");
	}

	[ConsoleCommand("printhookpool", "Print currently allocated hook argument pool memory")]
	[AuthLevel(2)]
	private void PrintHookPool(Arg arg)
	{
		StringTable stringTable = new StringTable("arg_count", "rented", "rented_extra", "returned", "stack_count", "max_size");
		try
		{
			foreach (KeyValuePair<int, HookCallerCommon.HookArgPool> item in HookCaller.Caller._argumentBuffer)
			{
				HookCallerCommon.HookArgPool value = item.Value;
				stringTable.AddRow(item.Key, value.Rented, value.RentedExtra, value.Returned, value.Count, HookCallerCommon.HookArgPool.BufferSize);
			}
			arg.ReplyWith(stringTable.Write(StringTable.FormatTypes.None));
		}
		finally
		{
			((IDisposable)stringTable/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ConsoleCommand("devdump", "Creates a zip package in the temporary directory of the Carbon folder with useful information (output log & profile snapshot). Syntax: c.devdump [logfile] [duration]")]
	[AuthLevel(2)]
	private void DevDumpSnapshot(Arg arg)
	{
		string file = Path.Combine(Defines.GetTempFolder(), "devdump-" + RandomEx.GetRandomString(5) + ".zip");
		DevDump dump = Pool.Get<DevDump>();
		bool includeServerLog = arg.GetBool(0, true);
		float duration = arg.GetFloat(arg.HasArgs(2) ? 1 : 0, 5f).Clamp(1f, 20f);
		dump.Init(includeServerLog);
		if (duration > 0f)
		{
			dump.Export(duration, file, delegate
			{
				Logger.Log($"Exported developer dump at '{file}' with a {duration} seconds profile recording.");
				Pool.Free<DevDump>(ref dump);
			});
		}
		else
		{
			dump.Export(file);
			Pool.Free<DevDump>(ref dump);
			arg.ReplyWith("Exported developer dump at '" + file + "'");
		}
	}

	[ConsoleCommand("shutdown", "Completely unloads Carbon from the game, rendering it fully vanilla. WARNING: This is for testing purposes only.")]
	[AuthLevel(2)]
	private void Shutdown(Arg arg)
	{
		Community.Runtime.Uninitialize();
	}

	[ConsoleCommand("help", "Returns a brief introduction to Carbon.")]
	[AuthLevel(2)]
	private void Help(Arg arg)
	{
		arg.ReplyWith("To get started, run the `c.find c.` to list all Carbon commands.\nTo list all currently loaded plugins, execute `c.plugins`.\nFor more information, please visit https://carbonmod.gg or join the Discord server at https://discord.gg/carbonmod\nYou're currently running " + Community.Runtime.Analytics.Version + ".");
	}

	private void HarmonyMods(Arg arg)
	{
		StringTable stringTable = new StringTable("name", "hooks", "methods");
		try
		{
			foreach (HarmonyMod loadedMod in HarmonyLoader.loadedMods)
			{
				object harmonyObject = loadedMod.Harmony.harmonyObject;
				Harmony val = (Harmony)((harmonyObject is Harmony) ? harmonyObject : null);
				stringTable.AddRow(loadedMod.Name, loadedMod.Hooks.Count.ToString("n0"), val.GetPatchedMethods().Count().ToString("n0"));
			}
			arg.ReplyWith(stringTable.ToStringMinimal());
		}
		finally
		{
			((IDisposable)stringTable/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private void SayAs(Arg arg)
	{
		if (!arg.HasArgs(4))
		{
			arg.ReplyWith("Syntax: sayas \"<username>\" \"<steamid>\" \"<color>\" \"<message>\"");
			return;
		}
		string text = arg.GetString(0, "SERVER");
		ulong uLong = arg.GetULong(1, 0uL);
		string text2 = arg.GetString(2, "#eee");
		string text3 = arg.GetString(3, "");
		Chat.Broadcast(text3, text, text2, uLong);
	}

	[ConsoleCommand("version", "Version information of the Carbon build and Rust.")]
	private void VersionCall(Arg arg)
	{
		IAnalyticsManager analytics = Community.Runtime.Analytics;
		if (arg.IsServerside)
		{
			arg.ReplyWith("Carbon" + string.Format(" {0}/{1}/{2} [{3}] [{4}] on Rust {5}/{6} ({7}) {8}", new object[9]
			{
				analytics.Version,
				analytics.Platform,
				analytics.Protocol,
				Build.Git.Branch,
				Build.Git.Tag,
				BuildInfo.Current.Build.Number,
				Protocol.printable,
				BuildInfo.Current.BuildDate,
				BuildInfo.Current.Scm.ChangeId
			}));
		}
		else
		{
			arg.ReplyWith("Carbon" + string.Format(" <color=#d14419>{0}/{1}/{2}</color> [{3}] [{4}] on Rust <color=#d14419>{5}/{6}</color> ({7}) {8}.", new object[9]
			{
				analytics.Version,
				analytics.Platform,
				analytics.Protocol,
				Build.Git.Branch,
				Build.Git.Tag,
				BuildInfo.Current.Build.Number,
				Protocol.printable,
				BuildInfo.Current.BuildDate,
				BuildInfo.Current.Scm.ChangeId
			}));
		}
	}

	[ConsoleCommand("build", "Information about the currently running Carbon build.")]
	[AuthLevel(2)]
	private void BuildCall(Arg arg)
	{
		arg.ReplyWith(Community.Runtime.Analytics.InformationalVersion);
	}

	[ConsoleCommand("protocol", "Protocol information used by the hook system of the Carbon build.")]
	[AuthLevel(2)]
	private void Protocol(Arg arg)
	{
		arg.ReplyWith(Community.Runtime.Analytics.Protocol);
	}

	[ConsoleCommand("commit", "Information about the Git commit of this build.")]
	[AuthLevel(2)]
	private void Commit(Arg arg)
	{
		StringBuilder stringBuilder = Pool.Get<StringBuilder>();
		stringBuilder.AppendLine("  Branch:  " + Build.Git.Branch);
		stringBuilder.AppendLine("  Author:  " + Build.Git.Author);
		stringBuilder.AppendLine(" Comment:  " + Build.Git.Comment);
		stringBuilder.AppendLine("    Date:  " + Build.Git.Date);
		stringBuilder.AppendLine("     Tag:  " + Build.Git.Tag);
		stringBuilder.AppendLine("    Hash:  " + Build.Git.HashShort + " (" + Build.Git.HashLong + ")");
		stringBuilder.AppendLine("     Url:  " + Build.Git.Url);
		stringBuilder.AppendLine($"   Debug:  {Build.IsDebug}");
		arg.ReplyWith(stringBuilder.ToString());
		Pool.FreeUnmanaged(ref stringBuilder);
	}

	[ConsoleCommand("whymodded", "Prints an intricate list of all the reasons why the server is set to modded and solutions to fix it.")]
	[AuthLevel(2)]
	private void WhyModded(Arg arg)
	{
		StringTable stringTable = new StringTable("reason", "type", "quick fix");
		try
		{
			if (Community.Runtime.Config.IsModded)
			{
				stringTable.AddRow("IsModded option is true", "Config", "c.modding 0");
			}
			foreach (BaseHookable module in Community.Runtime.ModuleProcessor.Modules)
			{
				if (module is BaseModule { ForceModded: not false } baseModule && baseModule.IsEnabled())
				{
					stringTable.AddRow(baseModule.Name + " is enabled", "Module", "c.setmodule \"" + baseModule.Name + "\" 0");
				}
			}
			foreach (KeyValuePair<string, Carbon.Components.CarbonAuto.AutoVar> item in Carbon.Components.CarbonAuto.AutoCache.Where((KeyValuePair<string, Carbon.Components.CarbonAuto.AutoVar> auto) => auto.Value.Variable.ForceModded && auto.Value.IsChanged()))
			{
				stringTable.AddRow("'" + item.Value.Variable.DisplayName + "' is changed", "Carbon Auto", item.Key + " -1");
			}
			arg.ReplyWith(stringTable.ToStringMinimal() + "\nTo apply all the changes necessary to be listed under the Community section, run 'c.gocommunity'.");
		}
		finally
		{
			((IDisposable)stringTable/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ConsoleCommand("gocommunity", "Executes a variety of changes necessary to set the server viable for the Community section. Run 'c.whymodded' to see what will be changed.")]
	[AuthLevel(2)]
	private void GoCommunity(Arg arg)
	{
		int num = 0;
		if (Community.Runtime.Config.IsModded)
		{
			Community.Runtime.Config.IsModded = false;
			num++;
		}
		foreach (BaseHookable module in Community.Runtime.ModuleProcessor.Modules)
		{
			if (module is BaseModule { ForceModded: not false } baseModule && baseModule.IsEnabled())
			{
				baseModule.SetEnabled(enable: false);
				num++;
			}
		}
		foreach (KeyValuePair<string, Carbon.Components.CarbonAuto.AutoVar> item in Carbon.Components.CarbonAuto.AutoCache.Where((KeyValuePair<string, Carbon.Components.CarbonAuto.AutoVar> auto) => auto.Value.Variable.ForceModded && auto.Value.IsChanged()))
		{
			item.Value.SetValue(-1);
			num++;
		}
		arg.ReplyWith(string.Format("Applied {0:n0} {1} to ensure that the server is no longer modded and fit for the Community tab.", num, num.Plural("change", "changes")));
	}

	[ConsoleCommand("extensions", "Prints a list of all currently loaded extensions.")]
	[AuthLevel(2)]
	private void Extensions(Arg arg)
	{
		StringTable stringTable = new StringTable("#", "extension", "type");
		try
		{
			int num = 1;
			foreach (KeyValuePair<Type, KeyValuePair<string, byte[]>> item in Community.Runtime.AssemblyEx.Extensions.Loaded)
			{
				stringTable.AddRow($"{num:n0}", Path.GetFileNameWithoutExtension(item.Value.Key), item.Key.FullName);
				num++;
			}
			arg.ReplyWith(stringTable.Write(StringTable.FormatTypes.None));
		}
		finally
		{
			((IDisposable)stringTable/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ConsoleCommand("hooks", "Prints total information for all currently active and patched hooks in the server. (syntax: c.hooks [loaded] [-p|-s|-d])")]
	[AuthLevel(2)]
	private void HooksCall(Arg args)
	{
		StringTable stringTable = new StringTable("#", "hook", "id", "type", "status", "time", "fires", "memory", "lag", "exceptions", "subs", "method");
		try
		{
			int num = 1;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			string text = args.GetString(0, (string)null);
			string text2 = args.GetString(1, (string)null);
			IEnumerable<IHook> installedPatches;
			switch (text)
			{
			case "loaded":
			{
				IEnumerable<IHook> loadedPatches;
				switch (text2)
				{
				case "-p":
					loadedPatches = Community.Runtime.HookManager.LoadedPatches;
					break;
				case "-s":
					loadedPatches = Community.Runtime.HookManager.LoadedStaticHooks;
					break;
				case "-d":
					loadedPatches = Community.Runtime.HookManager.LoadedDynamicHooks;
					break;
				default:
					loadedPatches = Community.Runtime.HookManager.LoadedPatches;
					loadedPatches = loadedPatches.Concat(Community.Runtime.HookManager.LoadedStaticHooks);
					loadedPatches = loadedPatches.Concat(Community.Runtime.HookManager.LoadedDynamicHooks);
					break;
				}
				loadedPatches = ((!(text2 == "-u")) ? loadedPatches.OrderBy((IHook x) => x.HookFullName) : loadedPatches.OrderByDescending((IHook x) => HookCaller.GetTotalTime(HookStringPool.GetOrAdd(x.HookName))));
				foreach (IHook item in loadedPatches)
				{
					if (item.Status == HookState.Failure)
					{
						num4++;
					}
					if (item.Status == HookState.Success)
					{
						num2++;
					}
					if (item.Status == HookState.Warning)
					{
						num3++;
					}
					uint orAdd = HookStringPool.GetOrAdd(item.HookName);
					double totalMilliseconds = HookCaller.GetTotalTime(orAdd).TotalMilliseconds;
					int totalFires = HookCaller.GetTotalFires(orAdd);
					double totalMemory = HookCaller.GetTotalMemory(orAdd);
					double totalLagSpikes = HookCaller.GetTotalLagSpikes(orAdd);
					int totalExceptions = HookCaller.GetTotalExceptions(orAdd);
					object[] obj = new object[12]
					{
						$"{num++:n0}",
						item.IsHidden ? (item.HookFullName + " (*)") : item.HookFullName,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null
					};
					string identifier = item.Identifier;
					obj[2] = identifier.Substring(identifier.Length - 6);
					obj[3] = (item.IsStaticHook ? "Static" : (item.IsPatch ? "Patch" : "Dynamic"));
					obj[4] = item.Status.ToString();
					obj[5] = ((totalMilliseconds == 0.0) ? string.Empty : $"{totalMilliseconds:0}ms");
					obj[6] = ((totalFires == 0) ? string.Empty : $"{totalFires}");
					obj[7] = ((totalMemory == 0.0) ? string.Empty : (totalMemory.Format().ToLower() ?? ""));
					obj[8] = ((totalLagSpikes == 0.0) ? string.Empty : $"{totalLagSpikes}");
					obj[9] = ((totalExceptions == 0) ? string.Empty : $"{totalExceptions}");
					obj[10] = (item.IsStaticHook ? "N/A" : $"{Community.Runtime.HookManager.GetHookSubscriberCount(item.Identifier),3}");
					obj[11] = item.TargetType?.Name + "." + item.TargetMethod;
					stringTable.AddRow(obj);
				}
				args.ReplyWith(string.Format("total:{0} success:{1} warning:{2} failed:{3}", new object[4] { num, num2, num3, num4 }) + Environment.NewLine + Environment.NewLine + stringTable.ToStringMinimal());
				return;
			}
			case "-p":
				installedPatches = Community.Runtime.HookManager.InstalledPatches;
				break;
			case "-s":
				installedPatches = Community.Runtime.HookManager.InstalledStaticHooks;
				break;
			case "-d":
				installedPatches = Community.Runtime.HookManager.InstalledDynamicHooks;
				break;
			default:
				installedPatches = Community.Runtime.HookManager.InstalledPatches;
				installedPatches = installedPatches.Concat(Community.Runtime.HookManager.InstalledStaticHooks);
				installedPatches = installedPatches.Concat(Community.Runtime.HookManager.InstalledDynamicHooks);
				break;
			}
			installedPatches = ((!(text == "-u") && !(text2 == "-u")) ? installedPatches.OrderBy((IHook x) => x.HookFullName) : installedPatches.OrderByDescending((IHook x) => HookCaller.GetTotalTime(HookStringPool.GetOrAdd(x.HookName))));
			foreach (IHook item2 in installedPatches)
			{
				if (item2.Status == HookState.Failure)
				{
					num4++;
				}
				if (item2.Status == HookState.Success)
				{
					num2++;
				}
				if (item2.Status == HookState.Warning)
				{
					num3++;
				}
				uint orAdd2 = HookStringPool.GetOrAdd(item2.HookName);
				double totalMilliseconds2 = HookCaller.GetTotalTime(orAdd2).TotalMilliseconds;
				int totalFires2 = HookCaller.GetTotalFires(orAdd2);
				double totalMemory2 = HookCaller.GetTotalMemory(orAdd2);
				double totalLagSpikes2 = HookCaller.GetTotalLagSpikes(orAdd2);
				int totalExceptions2 = HookCaller.GetTotalExceptions(orAdd2);
				object[] obj2 = new object[12]
				{
					$"{num++:n0}",
					item2.IsHidden ? (item2.HookFullName + " (*)") : item2.HookFullName,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				};
				string identifier2 = item2.Identifier;
				obj2[2] = identifier2.Substring(identifier2.Length - 6);
				obj2[3] = (item2.IsStaticHook ? "Static" : (item2.IsPatch ? "Patch" : "Dynamic"));
				obj2[4] = item2.Status.ToString();
				obj2[5] = ((totalMilliseconds2 == 0.0) ? string.Empty : $"{totalMilliseconds2:0}ms");
				obj2[6] = ((totalFires2 == 0) ? string.Empty : $"{totalFires2:n0}");
				obj2[7] = ((totalMemory2 == 0.0) ? string.Empty : (totalMemory2.Format().ToLower() ?? ""));
				obj2[8] = ((totalLagSpikes2 == 0.0) ? string.Empty : $"{totalLagSpikes2:n0}");
				obj2[9] = ((totalExceptions2 == 0) ? string.Empty : $"{totalExceptions2:n0}");
				obj2[10] = (item2.IsStaticHook ? "N/A" : $"{Community.Runtime.HookManager.GetHookSubscriberCount(item2.Identifier),3}");
				obj2[11] = item2.TargetType?.Name + "." + item2.TargetMethod;
				stringTable.AddRow(obj2);
			}
			args.ReplyWith(string.Format("total:{0} success:{1} warning:{2} failed:{3}", new object[4]
			{
				num - 1,
				num2,
				num3,
				num4
			}) + Environment.NewLine + Environment.NewLine + stringTable.ToStringMinimal());
		}
		finally
		{
			((IDisposable)stringTable/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ConsoleCommand("hookinfo", "Prints advanced information about a specific hook (takes [uint|string]). From hooks, hook times, hook memory usage to plugin and modules using it and other things.")]
	[AuthLevel(2)]
	private void HookInfo(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			Logger.Warn("You must provide the name of a hook to print plugin advanced information.");
			return;
		}
		string text = arg.GetString(0, "");
		uint result;
		bool flag = uint.TryParse(text, out result);
		string arg2 = (flag ? HookStringPool.GetOrAdd(text.ToUint()) : text);
		uint hookId = (flag ? text.ToUint() : HookStringPool.GetOrAdd(text));
		StringBuilder stringBuilder = Pool.Get<StringBuilder>();
		stringBuilder.AppendLine($"Information for {arg2}[{hookId}]");
		Dictionary<BaseHookable, CachedHookInstance> dictionary = Pool.Get<Dictionary<BaseHookable, CachedHookInstance>>();
		foreach (ModLoader.Package package in ModLoader.Packages)
		{
			foreach (RustPlugin plugin in package.Plugins)
			{
				foreach (KeyValuePair<uint, CachedHookInstance> item in plugin.HookPool.Where((KeyValuePair<uint, CachedHookInstance> hookCache) => hookCache.Key == hookId))
				{
					dictionary.Add(plugin, item.Value);
				}
			}
		}
		using StringTable stringTable = new StringTable(string.Empty, $"plugins ({dictionary.Count:n0})", "time", "fires", "memory", "lag", "exceptions", "async / hooks");
		foreach (KeyValuePair<BaseHookable, CachedHookInstance> item2 in dictionary)
		{
			CachedHook primaryHook = item2.Value.PrimaryHook;
			stringTable.AddRow(string.Empty, item2.Key.Name ?? "", (primaryHook.HookTime.TotalMilliseconds == 0.0) ? string.Empty : $"{primaryHook.HookTime.TotalMilliseconds:0}ms", (primaryHook.TimesFired == 0) ? string.Empty : $"{primaryHook.TimesFired:n0}", (primaryHook.MemoryUsage == 0.0) ? string.Empty : primaryHook.MemoryUsage.Format(ByteEx.ByteTypes.Auto, shortName: true, "0.0", "{0}{1}").ToLower(), (primaryHook.LagSpikes == 0) ? string.Empty : $"{primaryHook.LagSpikes:n0}", (primaryHook.Exceptions == 0) ? string.Empty : $"{primaryHook.Exceptions:n0}", $"{item2.Value.Hooks.Count((CachedHook x) => x.IsAsync):n0} / {item2.Value.Hooks.Count:n0}");
		}
		Dictionary<BaseHookable, CachedHookInstance> dictionary2 = Pool.Get<Dictionary<BaseHookable, CachedHookInstance>>();
		foreach (BaseHookable module in Community.Runtime.ModuleProcessor.Modules)
		{
			using IEnumerator<KeyValuePair<uint, CachedHookInstance>> enumerator6 = module.HookPool.Where((KeyValuePair<uint, CachedHookInstance> hookCache) => hookCache.Key == hookId).GetEnumerator();
			if (enumerator6.MoveNext())
			{
				KeyValuePair<uint, CachedHookInstance> current5 = enumerator6.Current;
				dictionary2.Add(module, current5.Value);
			}
		}
		stringTable.AddRow(string.Empty, $"Modules ({dictionary2.Count:n0})", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		foreach (KeyValuePair<BaseHookable, CachedHookInstance> item3 in dictionary2)
		{
			CachedHook cachedHook = item3.Value.Hooks.FirstOrDefault();
			stringTable.AddRow(string.Empty, item3.Key.Name ?? "", (cachedHook.HookTime.TotalMilliseconds == 0.0) ? string.Empty : $"{cachedHook.HookTime.TotalMilliseconds:0}ms", (cachedHook.TimesFired == 0) ? string.Empty : $"{cachedHook.TimesFired:n0}", (cachedHook.MemoryUsage == 0.0) ? string.Empty : cachedHook.MemoryUsage.Format(ByteEx.ByteTypes.Auto, shortName: true, "0.0", "{0}{1}").ToLower(), (cachedHook.LagSpikes == 0) ? string.Empty : $"{cachedHook.LagSpikes:n0}", (cachedHook.Exceptions == 0) ? string.Empty : $"{cachedHook.Exceptions:n0}", $"{item3.Value.Hooks.Count((CachedHook x) => x.IsAsync):n0} / {item3.Value.Hooks.Count:n0}");
		}
		double num = dictionary.Sum((KeyValuePair<BaseHookable, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook y) => y.HookTime.TotalMilliseconds)) + dictionary2.Sum((KeyValuePair<BaseHookable, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook y) => y.HookTime.TotalMilliseconds));
		int num2 = dictionary.Sum((KeyValuePair<BaseHookable, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook y) => y.TimesFired)) + dictionary2.Sum((KeyValuePair<BaseHookable, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook y) => y.TimesFired));
		double num3 = dictionary.Sum((KeyValuePair<BaseHookable, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook y) => y.MemoryUsage)) + dictionary2.Sum((KeyValuePair<BaseHookable, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook y) => y.MemoryUsage));
		int num4 = dictionary.Sum((KeyValuePair<BaseHookable, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook y) => y.LagSpikes)) + dictionary2.Sum((KeyValuePair<BaseHookable, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook y) => y.LagSpikes));
		int num5 = dictionary.Sum((KeyValuePair<BaseHookable, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook y) => y.Exceptions)) + dictionary2.Sum((KeyValuePair<BaseHookable, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook y) => y.Exceptions));
		stringTable.AddRow(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		stringTable.AddRow(string.Empty, "Total", (num == 0.0) ? string.Empty : $"{num:0}ms", (num2 == 0) ? string.Empty : $"{num2:n0}", (num3 == 0.0) ? string.Empty : (num3.Format().ToLower() ?? ""), (num4 == 0) ? string.Empty : $"{num4:n0}", (num5 == 0) ? string.Empty : $"{num5:n0}", string.Empty);
		stringBuilder.AppendLine(stringTable.ToStringMinimal().TrimEnd());
		arg.ReplyWith(stringBuilder.ToString());
		Pool.FreeUnmanaged(ref stringBuilder);
		Pool.FreeUnmanaged<BaseHookable, CachedHookInstance>(ref dictionary);
		Pool.FreeUnmanaged<BaseHookable, CachedHookInstance>(ref dictionary2);
	}

	[ConsoleCommand("wipemarkers", "Removes all markers of the calling player or argument filter.")]
	[AuthLevel(2)]
	private void ClearMarkers(Arg arg)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer val = ArgEx.Player(arg);
		if (arg.HasArgs(1))
		{
			val = BasePlayer.FindAwakeOrSleeping(arg.GetString(0, ""));
		}
		if ((Object)(object)val == (Object)null)
		{
			arg.ReplyWith("Couldn't find that player.");
			return;
		}
		arg.ReplyWith(arg.IsServerside ? ("Removed " + val.displayName + "'s map notes.") : "Removed all map notes.");
		val.Server_ClearMapMarkers(default(RPCMessage));
		val.SendMarkersToClient();
		((BaseNetworkable)val).SendNetworkUpdate((NetworkQueue)0);
	}

	[ConsoleCommand("moduleinfo", "Prints advanced information about a currently loaded module. From hooks, hook times, hook memory usage and other things.")]
	[AuthLevel(2)]
	private void ModuleInfo(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			Logger.Warn("You must provide the name of a module to print module advanced information.");
			return;
		}
		string name = arg.GetString(0, "");
		string text = arg.GetString(1, "");
		bool flag = arg.GetString(2, "").Equals("-asc");
		BaseModule baseModule = BaseModule.FindModule(name);
		if (baseModule == null)
		{
			arg.ReplyWith("Couldn't find that module.");
			return;
		}
		using StringTable stringTable = new StringTable(string.Empty, "id", "hook", "time", "fires", "memory", "lag", "exceptions", "subscribed", "async / hooks");
		foreach (List<CachedHook> item in text switch
		{
			"-t" => (flag ? baseModule.HookPool.OrderBy((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.HookTime.TotalMilliseconds)) : baseModule.HookPool.OrderByDescending((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.HookTime.TotalMilliseconds))).Select((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks), 
			"-m" => (flag ? baseModule.HookPool.OrderBy((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.MemoryUsage)) : baseModule.HookPool.OrderByDescending((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.MemoryUsage))).Select((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks), 
			"-f" => (flag ? baseModule.HookPool.OrderBy((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.TimesFired)) : baseModule.HookPool.OrderByDescending((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.TimesFired))).Select((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks), 
			"-ls" => (flag ? baseModule.HookPool.OrderBy((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.LagSpikes)) : baseModule.HookPool.OrderByDescending((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.LagSpikes))).Select((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks), 
			"-ex" => (flag ? baseModule.HookPool.OrderBy((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.Exceptions)) : baseModule.HookPool.OrderByDescending((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.Exceptions))).Select((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks), 
			_ => baseModule.HookPool.Select((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks), 
		})
		{
			if (item.Count == 0)
			{
				continue;
			}
			CachedHook cachedHook = item[0];
			string name2 = cachedHook.Method.Name;
			uint orAdd = HookStringPool.GetOrAdd(name2);
			if (baseModule.Hooks.Contains(orAdd))
			{
				double num = item.Sum((CachedHook x) => x.HookTime.TotalMilliseconds);
				double num2 = item.Sum((CachedHook x) => x.MemoryUsage);
				int count = item.Count;
				int num3 = item.Count((CachedHook x) => x.IsAsync);
				int num4 = item.Sum((CachedHook x) => x.TimesFired);
				int num5 = item.Sum((CachedHook x) => x.LagSpikes);
				int num6 = item.Sum((CachedHook x) => x.Exceptions);
				stringTable.AddRow(string.Empty, orAdd, name2 ?? "", (num == 0.0) ? string.Empty : $"{num:0}ms", (num4 == 0) ? string.Empty : $"{num4:n0}", (num2 == 0.0) ? string.Empty : (num2.Format().ToLower() ?? ""), (num5 == 0) ? string.Empty : $"{num5:n0}", (num6 == 0) ? string.Empty : $"{num6:n0}", (!baseModule.IgnoredHooks.Contains(orAdd)) ? "*" : string.Empty, $"{num3:n0} / {count:n0}");
			}
		}
		StringBuilder stringBuilder = Pool.Get<StringBuilder>();
		stringBuilder.AppendLine(string.Format("Additional information for {0} v{1}{2}", baseModule.Name, baseModule.Version, baseModule.ForceEnabled ? " [force enabled]" : string.Empty));
		stringBuilder.AppendLine($"  Enabled:                {baseModule.IsEnabled()}");
		stringBuilder.AppendLine($"  Enabled (default):      {baseModule.EnabledByDefault}");
		stringBuilder.AppendLine("  Context:                " + baseModule.Context);
		stringBuilder.AppendLine("  Uptime:                 " + TimeEx.Format(baseModule.Uptime).ToLower());
		stringBuilder.AppendLine($"  Total Hook Time:        {baseModule.TotalHookTime.TotalMilliseconds:0}ms");
		stringBuilder.AppendLine("  Total Memory Used:      " + baseModule.TotalMemoryUsed.Format().ToLower());
		stringBuilder.AppendLine($"  Internal Hook Override: {baseModule.InternalCallHookOverriden}");
		stringBuilder.AppendLine("Hooks:");
		stringBuilder.AppendLine(stringTable.ToStringMinimal());
		arg.ReplyWith(stringBuilder.ToString());
		Pool.FreeUnmanaged(ref stringBuilder);
	}

	[ConsoleCommand("setmodule", "Enables or disables Carbon modules. Visit root/carbon/modules and use the config file names as IDs.")]
	[AuthLevel(2)]
	private void SetModule(Arg arg)
	{
		if (!arg.HasArgs(2))
		{
			return;
		}
		string name = arg.GetString(0, "");
		BaseModule baseModule = BaseModule.FindModule(name);
		if (baseModule == null)
		{
			arg.ReplyWith("Couldn't find that module. Try 'c.modules' to print them all.");
			return;
		}
		if (baseModule.ForceEnabled)
		{
			arg.ReplyWith("That module is forcefully enabled, you may not change its status.");
			return;
		}
		if (baseModule.ForceDisabled)
		{
			arg.ReplyWith("That module is forcefully disabled, you may not change its status.");
			return;
		}
		bool flag = baseModule.IsEnabled();
		bool flag2 = arg.GetBool(1, false);
		if (flag != flag2)
		{
			baseModule.SetEnabled(flag2);
			baseModule.Save();
			arg.ReplyWith(baseModule.Name + " marked " + (baseModule.IsEnabled() ? "enabled" : "disabled") + ".");
		}
		else
		{
			arg.ReplyWith(baseModule.Name + " is already " + (baseModule.IsEnabled() ? "enabled" : "disabled") + ".");
		}
	}

	[ConsoleCommand("savemodule", "Saves Carbon module config & data file.")]
	[AuthLevel(2)]
	private void SaveModule(Arg arg)
	{
		if (arg.HasArgs(1))
		{
			string name = arg.GetString(0, "");
			BaseModule baseModule = BaseModule.FindModule(name);
			if (baseModule == null)
			{
				arg.ReplyWith("Couldn't find that module.");
				return;
			}
			baseModule.Save();
			arg.ReplyWith("Saved '" + baseModule.Name + "' module config & data file.");
		}
	}

	[ConsoleCommand("loadmodule", "Loads Carbon module config & data file.")]
	[AuthLevel(2)]
	private void LoadModule(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			return;
		}
		string name = arg.GetString(0, "");
		if (!(BaseModule.FindModule(name) is IModule module))
		{
			arg.ReplyWith("Couldn't find that module.");
			return;
		}
		try
		{
			module.Load();
			arg.ReplyWith("Reloaded '" + module.Name + "' module config & data.");
		}
		catch (Exception ex)
		{
			Logger.Error("Failed module Load for " + module.Name + " [Reload Request]", ex);
		}
	}

	[ConsoleCommand("modules", "Prints a list of all available modules. Eg. c.modules [-abc|--json|-t|-m|-f] [-asc]")]
	[AuthLevel(2)]
	private void Modules(Arg arg)
	{
		string text = arg.GetString(0, "");
		bool flag = arg.GetString(0, "").Equals("-asc") || arg.GetString(1, "").Equals("-asc");
		using StringTable stringTable = new StringTable("name", "enabled", "version", "time", "fires", "memory", "lag", "uptime");
		IEnumerable<BaseHookable> enumerable = text switch
		{
			"-abc" => Community.Runtime.ModuleProcessor.Modules.OrderBy((BaseHookable x) => x.Name), 
			"-t" => flag ? Community.Runtime.ModuleProcessor.Modules.OrderBy((BaseHookable x) => x.TotalHookTime) : Community.Runtime.ModuleProcessor.Modules.OrderByDescending((BaseHookable x) => x.TotalHookTime), 
			"-m" => flag ? Community.Runtime.ModuleProcessor.Modules.OrderBy((BaseHookable x) => x.TotalMemoryUsed) : Community.Runtime.ModuleProcessor.Modules.OrderByDescending((BaseHookable x) => x.TotalMemoryUsed), 
			"-f" => flag ? Community.Runtime.ModuleProcessor.Modules.OrderBy((BaseHookable x) => x.TotalHookFires) : Community.Runtime.ModuleProcessor.Modules.OrderByDescending((BaseHookable x) => x.TotalHookFires), 
			"-ls" => flag ? Community.Runtime.ModuleProcessor.Modules.OrderBy((BaseHookable x) => x.TotalHookLagSpikes) : Community.Runtime.ModuleProcessor.Modules.OrderByDescending((BaseHookable x) => x.TotalHookLagSpikes), 
			_ => flag ? Community.Runtime.ModuleProcessor.Modules.AsEnumerable().Reverse() : Community.Runtime.ModuleProcessor.Modules.AsEnumerable(), 
		};
		stringTable.AddRow("Native", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		foreach (BaseHookable item in enumerable)
		{
			if (item is BaseModule baseModule && string.IsNullOrEmpty(baseModule.Context))
			{
				stringTable.AddRow(" " + item.Name, baseModule.IsEnabled(), baseModule.Version, (baseModule.TotalHookTime.TotalMilliseconds == 0.0) ? string.Empty : $"{baseModule.TotalHookTime.TotalMilliseconds:0}ms", (baseModule.TotalHookFires == 0) ? string.Empty : $"{baseModule.TotalHookFires:n0}", (baseModule.TotalMemoryUsed == 0.0) ? string.Empty : (baseModule.TotalMemoryUsed.Format(ByteEx.ByteTypes.Auto, shortName: true, "0.0", "{0}{1}").ToLower() ?? ""), (baseModule.TotalHookLagSpikes == 0) ? string.Empty : $"{baseModule.TotalHookLagSpikes:n0}", TimeEx.Format(baseModule.Uptime) ?? "");
			}
		}
		foreach (KeyValuePair<Type, KeyValuePair<string, byte[]>> item2 in Community.Runtime.AssemblyEx.Modules.Loaded)
		{
			stringTable.AddRow(Path.GetFileNameWithoutExtension(item2.Value.Key), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			foreach (BaseHookable item3 in enumerable)
			{
				if (item3 is BaseModule baseModule2 && !string.IsNullOrEmpty(baseModule2.Context) && baseModule2.Context.Equals(item2.Value.Key, StringComparison.InvariantCultureIgnoreCase))
				{
					stringTable.AddRow(" " + item3.Name, baseModule2.IsEnabled(), baseModule2.Version, (baseModule2.TotalHookTime.TotalMilliseconds == 0.0) ? string.Empty : $"{baseModule2.TotalHookTime.TotalMilliseconds:0}ms", (baseModule2.TotalHookFires == 0) ? string.Empty : $"{baseModule2.TotalHookFires:n0}", (baseModule2.TotalMemoryUsed == 0.0) ? string.Empty : (baseModule2.TotalMemoryUsed.Format(ByteEx.ByteTypes.Auto, shortName: true, "0.0", "{0}{1}").ToLower() ?? ""), (baseModule2.TotalHookLagSpikes == 0) ? string.Empty : $"{baseModule2.TotalHookLagSpikes:n0}", TimeEx.Format(baseModule2.Uptime) ?? "");
				}
			}
		}
		arg.ReplyWith(stringTable.Write(StringTable.FormatTypes.None));
	}

	[ConsoleCommand("reloadmodule", "Reloads a currently loaded module assembly entirely.")]
	[AuthLevel(2)]
	private void ReloadModule(Arg arg)
	{
		if (arg.HasArgs(1))
		{
			BaseModule baseModule = BaseModule.FindModule(arg.GetString(0, ""));
			if (baseModule == null)
			{
				arg.ReplyWith("Couldn't find that module.");
				return;
			}
			baseModule.Reload();
			arg.ReplyWith("Reloaded '" + baseModule.Name + "' module.");
		}
	}

	[ConsoleCommand("openplugin", "Locally opens the `cs` file of a loaded plugin.")]
	[AuthLevel(2)]
	private void OpenPlugin(Arg arg)
	{
		RustPlugin rustPlugin = ModLoader.FindPlugin(arg.GetString(0, ""));
		if (rustPlugin == null)
		{
			arg.ReplyWith("Couldn't find plugin.");
			return;
		}
		Application.OpenURL(rustPlugin.FilePath);
		arg.ReplyWith("Opened '" + rustPlugin.ToPrettyString() + "' plugin file");
	}

	[ConsoleCommand("openroot", "Locally opens the root folder of Carbon.")]
	[AuthLevel(2)]
	private void OpenRoot(Arg arg)
	{
		string rootFolder = Defines.GetRootFolder();
		Application.OpenURL(rootFolder);
		arg.ReplyWith("Opened '" + rootFolder + "'");
	}

	[ConsoleCommand("openconfigs", "Locally opens the configs folder of Carbon.")]
	[AuthLevel(2)]
	private void OpenConfigs(Arg arg)
	{
		string configsFolder = Defines.GetConfigsFolder();
		Application.OpenURL(configsFolder);
		arg.ReplyWith("Opened '" + configsFolder + "'");
	}

	[ConsoleCommand("openmodules", "Locally opens the modules folder of Carbon.")]
	[AuthLevel(2)]
	private void OpenModules(Arg arg)
	{
		string modulesFolder = Defines.GetModulesFolder();
		Application.OpenURL(modulesFolder);
		arg.ReplyWith("Opened '" + modulesFolder + "'");
	}

	[ConsoleCommand("opendata", "Locally opens the data folder of Carbon.")]
	[AuthLevel(2)]
	private void OpenData(Arg arg)
	{
		string dataFolder = Defines.GetDataFolder();
		Application.OpenURL(dataFolder);
		arg.ReplyWith("Opened '" + dataFolder + "'");
	}

	[ConsoleCommand("openplugins", "Locally opens the plugins folder of Carbon.")]
	[AuthLevel(2)]
	private void OpenPlugins(Arg arg)
	{
		string scriptsFolder = Defines.GetScriptsFolder();
		Application.OpenURL(scriptsFolder);
		arg.ReplyWith("Opened '" + scriptsFolder + "'");
	}

	[ConsoleCommand("openextensions", "Locally opens the extensions folder of Carbon.")]
	[AuthLevel(2)]
	private void OpenExtensions(Arg arg)
	{
		string extensionsFolder = Defines.GetExtensionsFolder();
		Application.OpenURL(extensionsFolder);
		arg.ReplyWith("Opened '" + extensionsFolder + "'");
	}

	[ConsoleCommand("openlogs", "Locally opens the logs folder of Carbon.")]
	[AuthLevel(2)]
	private void OpenLogs(Arg arg)
	{
		string logsFolder = Defines.GetLogsFolder();
		Application.OpenURL(logsFolder);
		arg.ReplyWith("Opened '" + logsFolder + "'");
	}

	[ConsoleCommand("openlang", "Locally opens the language folder of Carbon.")]
	[AuthLevel(2)]
	private void OpenLang(Arg arg)
	{
		string langFolder = Defines.GetLangFolder();
		Application.OpenURL(langFolder);
		arg.ReplyWith("Opened '" + langFolder + "'");
	}

	[ConsoleCommand("delete", "Locally deletes a file or directory relative to the server root. Syntax: c.deleteext \"path/to\"")]
	[AuthLevel(2)]
	private void Delete(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			arg.ReplyWith("No arguments provided!\nSyntax: c.deleteext \"path/to\"\nSyntax: c.deleteext \"path/to/file.txt\"");
			return;
		}
		string text = Path.Combine(Defines.GetRootFolder(), arg.GetString(0, ""));
		if (OsEx.File.Exists(text) || OsEx.Folder.Exists(text))
		{
			OsEx.File.Delete(text);
			OsEx.Folder.Delete(text);
			arg.ReplyWith("Deleted '" + text + "'");
		}
		else
		{
			arg.ReplyWith("Couldn't delete '" + text + "' as it doesn't exist.");
		}
	}

	[ConsoleCommand("deleteext", "Locally deletes all files with a specified extension relative to the server root. Syntax: c.deleteext \"path/to\" \"cs\"")]
	[AuthLevel(2)]
	private void DeleteExt(Arg arg)
	{
		if (!arg.HasArgs(2))
		{
			arg.ReplyWith("Not enough arguments provided!\nSyntax: c.deleteext \"path/to\" \"cs\"");
			return;
		}
		string folder = Path.Combine(Defines.GetRootFolder(), arg.GetString(0, ""));
		string extension = arg.GetString(1, "");
		string[] filesWithExtension = OsEx.Folder.GetFilesWithExtension(folder, extension);
		OsEx.Folder.DeleteFilesWithExtension(folder, extension);
		for (int i = 0; i < filesWithExtension.Length; i++)
		{
			arg.ReplyWith("Deleted '" + filesWithExtension[i] + "'");
		}
	}

	[ConsoleCommand("createplugin", "It creates a new plugin in the plugins folder. Syntax: c.createplugin \"PluginName\" \"Author\" \"Description\"")]
	[AuthLevel(2)]
	private void CreatePlugin(Arg arg)
	{
		string text = arg.GetString(0, "NewPlugin");
		string text2 = text.Replace(" ", string.Empty);
		string file = Path.Combine(Defines.GetScriptsFolder(), text2 + ".cs");
		if (OsEx.File.Exists(file))
		{
			arg.ReplyWith("A plugin with the same name already exists.");
			return;
		}
		string text3 = arg.GetString(1, Environment.UserName);
		string text4 = arg.GetString(2, "New cool plugin that does things!");
		OsEx.File.Create(file, "namespace Carbon.Plugins;\n\n[Info(\"" + text + "\", \"" + text3 + "\", \"1.0\")]\n[Description(\"" + text4 + "\")]\npublic partial class " + text2 + " : CarbonPlugin\n{\n\tprivate void OnServerInitialized()\n\t{\n\t\tPuts(\"New plugin is here!\");\n\t}\n}");
	}

	[ConsoleCommand("grant", "Grant one or more permissions to users or groups. Do 'c.grant' for syntax info.")]
	[AuthLevel(2)]
	private void Grant(Arg arg)
	{
		if (!arg.HasArgs(3))
		{
			PrintWarn(arg);
			return;
		}
		string text = arg.GetString(0, "");
		string text2 = arg.GetString(1, "");
		string text3 = arg.GetString(2, "");
		KeyValuePair<string, UserData> keyValuePair = permission.FindUser(text2);
		if (!permission.PermissionExists(text3))
		{
			arg.ReplyWith("Couldn't grant permission - permission does not exist.");
			return;
		}
		bool flag = text3.Equals(Permission.StarStr);
		if (!(text == "user"))
		{
			if (text == "group")
			{
				if (!permission.GroupExists(text2))
				{
					arg.ReplyWith("Couldn't grant group permission - group not found, use full name.");
				}
				else if (permission.GroupHasPermission(text2, text3) && !flag)
				{
					arg.ReplyWith("Already has that permission assigned.");
				}
				else if (flag)
				{
					string[] groupPermissions = permission.GetGroupPermissions(text2);
					if (permission.GrantGroupPermission(text2, text3, null))
					{
						IEnumerable<string> enumerable = permission.GetGroupPermissions(text2).Except(groupPermissions);
						int num = enumerable.Count();
						arg.ReplyWith(string.Format("Granted group '{0}' {1:n0} {2}: {3}", new object[4]
						{
							text2,
							num,
							num.Plural("permission", "permissions"),
							enumerable.ToString(", ")
						}));
					}
					else
					{
						arg.ReplyWith("Couldn't grant group permissions - most likely because they're all already granted.");
					}
				}
				else if (permission.GrantGroupPermission(text2, text3, null))
				{
					arg.ReplyWith("Granted group '" + text2 + "' permission '" + text3 + "'");
				}
				else
				{
					arg.ReplyWith("Couldn't grant group permission.");
				}
			}
			else
			{
				PrintWarn(arg);
			}
		}
		else if (string.IsNullOrEmpty(keyValuePair.Key))
		{
			arg.ReplyWith("Couldn't grant user permission - user not found, use full name or steam ID.");
		}
		else if (permission.UserHasPermission(keyValuePair.Key, text3) && !flag)
		{
			arg.ReplyWith("Already has that permission assigned.");
		}
		else if (flag)
		{
			string[] userPermissions = permission.GetUserPermissions(keyValuePair.Key);
			if (permission.GrantUserPermission(keyValuePair.Key, text3, null))
			{
				IEnumerable<string> enumerable2 = permission.GetUserPermissions(keyValuePair.Key).Except(userPermissions);
				int num2 = enumerable2.Count();
				arg.ReplyWith(string.Format("Granted user '{0}' {1:n0} {2}: {3}", new object[4]
				{
					keyValuePair.Value.LastSeenNickname,
					num2,
					num2.Plural("permission", "permissions"),
					enumerable2.ToString(", ")
				}));
			}
			else
			{
				arg.ReplyWith("Couldn't grant user permissions - most likely because they're all already granted.");
			}
		}
		else if (permission.GrantUserPermission(keyValuePair.Key, text3, null))
		{
			arg.ReplyWith("Granted user '" + keyValuePair.Value.LastSeenNickname + "' permission '" + text3 + "'");
		}
		else
		{
			arg.ReplyWith("Couldn't grant user permission.");
		}
		static void PrintWarn(Arg val)
		{
			val.ReplyWith("Syntax: c.grant <user|group> <name|id> <perm>\nSyntax: c.grant <user|group> <name|id> *");
		}
	}

	[ConsoleCommand("revoke", "Revoke one or more permissions from users or groups. Do 'c.revoke' for syntax info.")]
	[AuthLevel(2)]
	private void Revoke(Arg arg)
	{
		if (!arg.HasArgs(3))
		{
			PrintWarn(arg);
			return;
		}
		string text = arg.GetString(0, "");
		string text2 = arg.GetString(1, "");
		string text3 = arg.GetString(2, "");
		KeyValuePair<string, UserData> keyValuePair = permission.FindUser(text2);
		bool flag = text3.Equals(Permission.StarStr);
		if (!(text == "user"))
		{
			if (text == "group")
			{
				if (!permission.GroupExists(text2))
				{
					arg.ReplyWith("Couldn't revoke group permission - group not found, use full name.");
				}
				else if (!permission.GroupHasPermission(text2, text3) && !flag)
				{
					arg.ReplyWith("Group does not have that permission assigned.");
				}
				else if (flag)
				{
					string[] groupPermissions = permission.GetGroupPermissions(text2);
					if (permission.RevokeGroupPermission(text2, text3))
					{
						IEnumerable<string> enumerable = groupPermissions.Except(permission.GetGroupPermissions(text2));
						int num = enumerable.Count();
						arg.ReplyWith(string.Format("Revoked group '{0}' {1:n0} {2}: {3}", new object[4]
						{
							text2,
							num,
							num.Plural("permission", "permissions"),
							enumerable.ToString(", ")
						}));
					}
					else
					{
						arg.ReplyWith("Couldn't revoke group permissions - most likely because none of them are granted.");
					}
				}
				else if (permission.RevokeGroupPermission(text2, text3))
				{
					arg.ReplyWith("Revoked group '" + text2 + "' permission '" + text3 + "'");
				}
				else
				{
					arg.ReplyWith("Couldn't revoke group permission.");
				}
			}
			else
			{
				PrintWarn(arg);
			}
		}
		else if (string.IsNullOrEmpty(keyValuePair.Key))
		{
			arg.ReplyWith("Couldn't revoke user permission - user not found, use full name or steam ID.");
		}
		else if (!permission.UserHasPermission(keyValuePair.Key, text3) && !flag)
		{
			arg.ReplyWith("User does not have that permission assigned.");
		}
		else if (flag)
		{
			string[] userPermissions = permission.GetUserPermissions(keyValuePair.Key);
			if (permission.RevokeUserPermission(keyValuePair.Key, text3))
			{
				IEnumerable<string> enumerable2 = userPermissions.Except(permission.GetUserPermissions(keyValuePair.Key));
				int num2 = enumerable2.Count();
				arg.ReplyWith(string.Format("Revoked user '{0}' {1:n0} {2}: {3}", new object[4]
				{
					keyValuePair.Value.LastSeenNickname,
					num2,
					num2.Plural("permission", "permissions"),
					enumerable2.ToString(", ")
				}));
			}
			else
			{
				arg.ReplyWith("Couldn't revoke user permissions - most likely because none of them are granted.");
			}
		}
		else if (permission.RevokeUserPermission(keyValuePair.Key, text3))
		{
			arg.ReplyWith("Revoked user '" + keyValuePair.Value?.LastSeenNickname + "' permission '" + text3 + "'");
		}
		else
		{
			arg.ReplyWith("Couldn't revoke user permission.");
		}
		static void PrintWarn(Arg val)
		{
			val.ReplyWith("Syntax: c.revoke <user|group> <name|id> <perm>\nSyntax: c.revoke <user|group> <name|id> *");
		}
	}

	[ConsoleCommand("show", "Displays information about a specific player or group (incl. permissions, groups and user list). Do 'c.show' for syntax info.")]
	[AuthLevel(2)]
	private void Show(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			PrintWarn(arg);
			return;
		}
		switch (arg.GetString(0, ""))
		{
		case "user":
		{
			if (!arg.HasArgs(2))
			{
				PrintWarn(arg);
				break;
			}
			string id = arg.GetString(1, "");
			KeyValuePair<string, UserData> keyValuePair2 = permission.FindUser(id);
			if (keyValuePair2.Value == null)
			{
				arg.ReplyWith("Couldn't find that user.");
				break;
			}
			string[] userPermissions = permission.GetUserPermissions(keyValuePair2.Key);
			arg.ReplyWith(string.Format("User {0}[{1}] found in {2:n0} groups:\n  {3}\n", new object[4]
			{
				keyValuePair2.Value.LastSeenNickname,
				keyValuePair2.Key,
				keyValuePair2.Value.Groups.Count,
				keyValuePair2.Value.Groups.Select((string x) => x).ToString(", ", " and ")
			}) + string.Format("and has {0:n0} permissions:\n  {1}", userPermissions.Count(), userPermissions.ToString(", ")));
			break;
		}
		case "group":
		{
			if (!arg.HasArgs(2))
			{
				PrintWarn(arg);
				break;
			}
			string text4 = arg.GetString(1, "");
			if (!permission.GroupExists(text4))
			{
				arg.ReplyWith("Couldn't find that group.");
				break;
			}
			string[] usersInGroup = permission.GetUsersInGroup(text4);
			string[] groupPermissions = permission.GetGroupPermissions(text4);
			arg.ReplyWith(string.Format("Group {0} has {1:n0} users:\n  {2}\n", text4, usersInGroup.Length, usersInGroup.Select((string x) => x).ToString(", ")) + string.Format("and has {0:n0} permissions:\n  {1}", groupPermissions.Length, groupPermissions.Select((string x) => x).ToString(", ")));
			break;
		}
		case "perm":
		{
			if (!arg.HasArgs(2))
			{
				PrintWarn(arg);
				break;
			}
			string text3 = arg.GetString(1, "");
			if (!permission.PermissionExists(text3))
			{
				arg.ReplyWith("Couldn't find that permission.");
				break;
			}
			string[] permissionUsers = permission.GetPermissionUsers(text3);
			string[] permissionGroups = permission.GetPermissionGroups(text3);
			arg.ReplyWith(string.Format("Permission {0} is granted to {1:n0} users:\n  {2}\n", text3, permissionUsers.Length, permissionUsers.Select((string x) => x).ToString(", ")) + string.Format("and {0:n0} groups:\n  {1}", permissionGroups.Length, permissionGroups.Select((string x) => x).ToString(", ")));
			break;
		}
		case "groups":
		{
			string[] groups = permission.GetGroups();
			if (groups.Count() == 0)
			{
				arg.ReplyWith("Couldn't find any group.");
			}
			else
			{
				arg.ReplyWith("Groups:\n " + groups.ToString(", "));
			}
			break;
		}
		case "perms":
		{
			string[] permissions = permission.GetPermissions();
			if (!permissions.Any())
			{
				arg.ReplyWith("Couldn't find any permission.");
			}
			arg.ReplyWith("Permissions:\n " + permissions.ToString(", "));
			break;
		}
		case "orphans":
		{
			string text = (arg.HasArgs(2) ? arg.GetString(1, "").ToLower() : Permission.StarStr);
			string text2 = (arg.HasArgs(3) ? arg.GetString(2, "") : null);
			bool flag = text.Equals(Permission.StarStr);
			bool flag2 = flag || text == "user";
			bool flag3 = flag || text == "group";
			if (!flag2 && !flag3)
			{
				PrintWarn(arg);
				break;
			}
			if (text2 != null && flag)
			{
				arg.ReplyWith("To target a specific user or group, specify the scope: c.show orphans <user|group> <name|id>");
				break;
			}
			List<string> list = Pool.Get<List<string>>();
			if (flag2)
			{
				if (text2 != null)
				{
					KeyValuePair<string, UserData> keyValuePair = permission.FindUser(text2);
					if (keyValuePair.Value == null)
					{
						arg.ReplyWith("Couldn't find that user.");
						Pool.FreeUnmanaged<string>(ref list);
						break;
					}
					AppendOrphans("user", keyValuePair.Key, keyValuePair.Value.Perms, list);
				}
				else
				{
					foreach (KeyValuePair<string, UserData> userdatum in permission.userdata)
					{
						AppendOrphans("user", userdatum.Key, userdatum.Value.Perms, list);
					}
				}
			}
			if (flag3)
			{
				if (text2 != null)
				{
					if (!permission.GroupExists(text2))
					{
						arg.ReplyWith("Couldn't find that group.");
						Pool.FreeUnmanaged<string>(ref list);
						break;
					}
					AppendOrphans("group", text2, permission.GetGroupData(text2)?.Perms, list);
				}
				else
				{
					foreach (KeyValuePair<string, GroupData> groupdatum in permission.groupdata)
					{
						AppendOrphans("group", groupdatum.Key, groupdatum.Value.Perms, list);
					}
				}
			}
			if (list.Count == 0)
			{
				arg.ReplyWith("No orphan permissions found.");
			}
			else
			{
				arg.ReplyWith(string.Format("Orphan permissions ({0:n0}):\n{1}", list.Count, string.Join("\n", list)));
			}
			Pool.FreeUnmanaged<string>(ref list);
			break;
		}
		default:
			PrintWarn(arg);
			break;
		}
		void AppendOrphans(string kind, string text5, HashSet<string> perms, List<string> output)
		{
			if (perms == null || perms.Count == 0)
			{
				return;
			}
			foreach (string perm in perms)
			{
				if (!permission.PermissionExists(perm))
				{
					output.Add("  " + kind + " " + text5 + " -> " + perm);
				}
			}
		}
		static void PrintWarn(Arg val)
		{
			val.ReplyWith("Syntax: c.show <groups|perms>\nSyntax: c.show <group|user|perm> <name|id>\nSyntax: c.show orphans [<user|group>] [<name|id>]");
		}
	}

	[ConsoleCommand("cleanup", "Cleans up grants whose owning plugin is no longer registered. Do 'c.cleanup' for syntax info.")]
	[AuthLevel(2)]
	private void Cleanup(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			PrintWarn(arg);
			return;
		}
		string text = arg.GetString(0, "");
		if (text == "orphans")
		{
			string text2 = (arg.HasArgs(2) ? arg.GetString(1, "").ToLower() : Permission.StarStr);
			string text3 = (arg.HasArgs(3) ? arg.GetString(2, "") : null);
			bool flag = text2.Equals(Permission.StarStr);
			bool flag2 = flag || text2 == "user";
			bool flag3 = flag || text2 == "group";
			if (!flag2 && !flag3)
			{
				PrintWarn(arg);
				return;
			}
			if (text3 != null && flag)
			{
				arg.ReplyWith("To target a specific user or group, specify the scope: c.cleanup orphans <user|group> <name|id>");
				return;
			}
			int num = 0;
			int num2 = 0;
			List<string> buffer = Pool.Get<List<string>>();
			if (flag2)
			{
				if (text3 != null)
				{
					KeyValuePair<string, UserData> keyValuePair = permission.FindUser(text3);
					if (keyValuePair.Value == null)
					{
						arg.ReplyWith("Couldn't find that user.");
						Pool.FreeUnmanaged<string>(ref buffer);
						return;
					}
					num += RevokeUserOrphans(keyValuePair.Key, keyValuePair.Value.Perms, buffer);
				}
				else
				{
					foreach (KeyValuePair<string, UserData> userdatum in permission.userdata)
					{
						num += RevokeUserOrphans(userdatum.Key, userdatum.Value.Perms, buffer);
					}
				}
			}
			if (flag3)
			{
				if (text3 != null)
				{
					if (!permission.GroupExists(text3))
					{
						arg.ReplyWith("Couldn't find that group.");
						Pool.FreeUnmanaged<string>(ref buffer);
						return;
					}
					num2 += RevokeGroupOrphans(text3, permission.GetGroupData(text3)?.Perms, buffer);
				}
				else
				{
					foreach (KeyValuePair<string, GroupData> groupdatum in permission.groupdata)
					{
						num2 += RevokeGroupOrphans(groupdatum.Key, groupdatum.Value.Perms, buffer);
					}
				}
			}
			Pool.FreeUnmanaged<string>(ref buffer);
			int num3 = num + num2;
			if (num3 == 0)
			{
				arg.ReplyWith("No orphan permissions found.");
				return;
			}
			arg.ReplyWith(string.Format("Revoked {0:n0} orphan {1} ({2:n0} user, {3:n0} group).", new object[4]
			{
				num3,
				num3.Plural("permission", "permissions"),
				num,
				num2
			}));
		}
		else
		{
			PrintWarn(arg);
		}
		static void PrintWarn(Arg val)
		{
			val.ReplyWith("Syntax: c.cleanup orphans [<user|group>] [<name|id>]");
		}
		int RevokeGroupOrphans(string name, HashSet<string> perms, List<string> list)
		{
			if (perms == null || perms.Count == 0)
			{
				return 0;
			}
			list.Clear();
			list.AddRange(perms);
			int num4 = 0;
			for (int i = 0; i < list.Count; i++)
			{
				string text4 = list[i];
				if (!permission.PermissionExists(text4) && permission.RevokeGroupPermission(name, text4))
				{
					num4++;
				}
			}
			return num4;
		}
		int RevokeUserOrphans(string id, HashSet<string> perms, List<string> list)
		{
			if (perms == null || perms.Count == 0)
			{
				return 0;
			}
			list.Clear();
			list.AddRange(perms);
			int num4 = 0;
			for (int i = 0; i < list.Count; i++)
			{
				string text4 = list[i];
				if (!permission.PermissionExists(text4) && permission.RevokeUserPermission(id, text4))
				{
					num4++;
				}
			}
			return num4;
		}
	}

	[ConsoleCommand("usergroup", "Adds or removes a player from a group. Do 'c.usergroup' for syntax info.")]
	[AuthLevel(2)]
	private void UserGroup(Arg arg)
	{
		string text = arg.GetString(0, "");
		string empty = string.Empty;
		string group = string.Empty;
		KeyValuePair<string, UserData> keyValuePair = default(KeyValuePair<string, UserData>);
		if (text == "add" || text == "remove")
		{
			if (!arg.HasArgs(3))
			{
				PrintWarn(arg);
				return;
			}
			empty = arg.GetString(1, "");
			group = arg.GetString(2, "");
			if (!permission.GroupExists(group))
			{
				arg.ReplyWith("Group '" + group + "' could not be found.");
				return;
			}
			keyValuePair = permission.FindUser(empty);
			if (keyValuePair.Value == null)
			{
				arg.ReplyWith("Couldn't find that player.");
				return;
			}
		}
		else
		{
			if (!arg.HasArgs(2))
			{
				PrintWarn(arg);
				return;
			}
			group = arg.GetString(1, "");
			if (!permission.GroupExists(group))
			{
				arg.ReplyWith("Group '" + group + "' could not be found.");
				return;
			}
		}
		switch (text)
		{
		case "add":
			if (permission.UserHasGroup(keyValuePair.Key, group))
			{
				arg.ReplyWith(keyValuePair.Value.LastSeenNickname + "[" + keyValuePair.Key + "] is already in '" + group + "' group.");
			}
			else
			{
				permission.AddUserGroup(keyValuePair.Key, group);
				arg.ReplyWith("Added " + keyValuePair.Value.LastSeenNickname + "[" + keyValuePair.Key + "] to '" + group + "' group.");
			}
			break;
		case "remove":
			if (!permission.UserHasGroup(keyValuePair.Key, group))
			{
				arg.ReplyWith(keyValuePair.Value.LastSeenNickname + "[" + keyValuePair.Key + "] isn't in '" + group + "' group.");
			}
			else
			{
				permission.RemoveUserGroup(keyValuePair.Key, group);
				arg.ReplyWith("Removed " + keyValuePair.Value.LastSeenNickname + "[" + keyValuePair.Key + "] from '" + group + "' group.");
			}
			break;
		case "addall":
		{
			group = group.ToLower();
			int num2 = permission.userdata.Count((KeyValuePair<string, UserData> userDataValue) => permission.GetUserData(userDataValue.Key).Groups.Add(group));
			arg.ReplyWith($"Added {num2:n0} users to '{group}' group.");
			break;
		}
		case "removeall":
		{
			group = group.ToLower();
			int num = permission.userdata.Count((KeyValuePair<string, UserData> userDataValue) => permission.GetUserData(userDataValue.Key).Groups.Remove(group));
			arg.ReplyWith($"Removed {num:n0} users from '{group}' group.");
			break;
		}
		default:
			PrintWarn(arg);
			break;
		}
		static void PrintWarn(Arg val)
		{
			val.ReplyWith("Syntax: c.usergroup <add|remove> <player> <group>\nSyntax: c.usergroup <addall|removeall> <group>");
		}
	}

	[ConsoleCommand("group", "Adds or removes a group. Do 'c.group' for syntax info.")]
	[AuthLevel(2)]
	private void Group(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			PrintWarn(arg);
			return;
		}
		switch (arg.GetString(0, ""))
		{
		case "add":
		{
			if (!arg.HasArgs(2))
			{
				PrintWarn(arg);
				break;
			}
			string text3 = arg.GetString(1, "");
			if (permission.GroupExists(text3))
			{
				arg.ReplyWith("Group '" + text3 + "' already exists. To set any values for this group, use 'c.group set'.");
			}
			else if (permission.CreateGroup(text3, arg.HasArgs(3) ? arg.GetString(2, "") : text3, arg.HasArgs(4) ? arg.GetInt(3, 0) : 0))
			{
				arg.ReplyWith("Created '" + text3 + "' group.");
			}
			break;
		}
		case "set":
		{
			if (!arg.HasArgs(4))
			{
				PrintWarn(arg);
				break;
			}
			string text5 = arg.GetString(1, "");
			if (!permission.GroupExists(text5))
			{
				arg.ReplyWith("Group '" + text5 + "' does not exists.");
				break;
			}
			string text6 = arg.GetString(2, "");
			string text7 = arg.GetString(3, "");
			if (!(text6 == "title"))
			{
				if (text6 == "rank")
				{
					permission.SetGroupRank(text5, text7.ToInt());
				}
			}
			else
			{
				permission.SetGroupTitle(text5, text7);
			}
			arg.ReplyWith("Set '" + text5 + "' group.");
			break;
		}
		case "remove":
		{
			if (!arg.HasArgs(2))
			{
				PrintWarn(arg);
				break;
			}
			string text4 = arg.GetString(1, "");
			if (permission.RemoveGroup(text4))
			{
				arg.ReplyWith("Removed '" + text4 + "' group.");
			}
			else
			{
				arg.ReplyWith("Couldn't remove '" + text4 + "' group.");
			}
			break;
		}
		case "parent":
		{
			if (!arg.HasArgs(3))
			{
				PrintWarn(arg);
				break;
			}
			string text = arg.GetString(1, "");
			string text2 = arg.GetString(2, "");
			if (permission.SetGroupParent(text, text2))
			{
				arg.ReplyWith("Changed '" + text + "' group's parent to '" + text2 + "'.");
			}
			else
			{
				arg.ReplyWith("Couldn't change '" + text + "' group's parent to '" + text2 + "'.");
			}
			break;
		}
		default:
			PrintWarn(arg);
			break;
		}
		static void PrintWarn(Arg val)
		{
			val.ReplyWith("Syntax: c.group add <group> [<displayName>] [<rank>]\nSyntax: c.group remove <group>\nSyntax: c.group set <group> <title|rank> <value>\nSyntax: c.group parent <group> [<parent>]");
		}
	}

	[ConsoleCommand("migrate_perms_sql", "This will migrate all groups and users to a locally stored SQLite database from your Protobuf/Storeless database.")]
	[AuthLevel(2)]
	private void MigrateToSql(Arg arg)
	{
		if (Community.Runtime.Config.Permissions.PermissionSerialization == Permission.SerializationMode.SQL)
		{
			arg.ReplyWith("Permission serialization must be anything but SQL");
			return;
		}
		string sQLPermissionsDatabase = Switches.GetSQLPermissionsDatabase(Path.Combine(Server.filesStorageFolder, "carbon.perms.db"));
		if (File.Exists(sQLPermissionsDatabase))
		{
			File.Move(sQLPermissionsDatabase, Switches.GetSQLPermissionsDatabase(Path.Combine(Server.filesStorageFolder, "carbon.perms.db.old")));
		}
		PermissionSql permissionSql = new PermissionSql();
		permissionSql.MigrateFromProto(Community.Runtime.Core.permission);
		Community.Runtime.Core.permission.Dispose();
		foreach (ModLoader.Package package in ModLoader.Packages)
		{
			foreach (RustPlugin plugin in package.Plugins)
			{
				plugin.permission = permissionSql;
			}
		}
		foreach (BaseHookable module in Community.Runtime.ModuleProcessor.Modules)
		{
			if (module is BaseModule baseModule)
			{
				baseModule.SetPermissions(permissionSql);
			}
		}
		Interface.Oxide.Permission = permissionSql;
		Community.Runtime.Core.permission = permissionSql;
		Community.Runtime.Config.Permissions.PermissionSerialization = Permission.SerializationMode.SQL;
		Community.Runtime.SaveConfig();
		Analytics.perms_migration(Community.Runtime.Config.Permissions.PermissionSerialization, permissionSql.groupdata.Count, permissionSql.userdata.Count);
	}

	[ConsoleCommand("migrate_perms_proto", "This will migrate all groups and users to a locally stored Protobuf database from your SQL database.")]
	[AuthLevel(2)]
	private void MigrateToProto(Arg arg)
	{
		if (Community.Runtime.Config.Permissions.PermissionSerialization == Permission.SerializationMode.Protobuf || !(Community.Runtime.Core.permission is PermissionSql permissionSql))
		{
			arg.ReplyWith("Permission serialization must be anything but Protobuf");
			return;
		}
		Permission permission = new Permission();
		permissionSql.MigrateToProto(permission);
		permissionSql.Dispose();
		foreach (ModLoader.Package package in ModLoader.Packages)
		{
			foreach (RustPlugin plugin in package.Plugins)
			{
				plugin.permission = permission;
			}
		}
		foreach (BaseHookable module in Community.Runtime.ModuleProcessor.Modules)
		{
			if (module is BaseModule baseModule)
			{
				baseModule.SetPermissions(permission);
			}
		}
		Interface.Oxide.Permission = permission;
		Community.Runtime.Core.permission = permission;
		Community.Runtime.Config.Permissions.PermissionSerialization = Permission.SerializationMode.Protobuf;
		Community.Runtime.SaveConfig();
		Analytics.perms_migration(Community.Runtime.Config.Permissions.PermissionSerialization, permission.groupdata.Count, permission.userdata.Count);
		permission.SaveData();
	}

	[ConsoleCommand("plugins", "Prints the list of mods and their loaded plugins. Eg. c.plugins [-j|--j|-json|-abc|--json|-t|-m|-f|-ls] [-asc]")]
	[AuthLevel(2)]
	private void Plugins(Arg arg)
	{
		if (!arg.IsPlayerCalledOrAdmin())
		{
			return;
		}
		string text = arg.GetString(0, "");
		bool flag = arg.GetString(0, "").Equals("-asc") || arg.GetString(1, "").Equals("-asc");
		IEnumerable<string> enumerable = Community.Runtime.ScriptProcessor.IgnoreList.Concat(Community.Runtime.ZipScriptProcessor.IgnoreList);
		switch (text)
		{
		case "-j":
		case "--j":
		case "-json":
		case "--json":
			arg.ReplyWith((object)new
			{
				Plugins = ModLoader.Packages,
				Unloaded = enumerable,
				Failed = ModLoader.FailedCompilations.Values.Where((ModLoader.CompilationResult x) => x.HasFailed())
			});
			return;
		}
		StringTable stringTable = new StringTable("#", "package", "author", "version", "hook time", "hook fires", "hook memory", "hook lag", "hook exceptions", "compile time", "uptime");
		try
		{
			int num = 1;
			foreach (ModLoader.Package package in ModLoader.Packages)
			{
				stringTable.AddRow($"{num:n0}", package.Name + ((package.Plugins.Count >= 1) ? $" ({package.Plugins.Count:n0})" : string.Empty), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
				foreach (RustPlugin item in text switch
				{
					"-abc" => package.Plugins.OrderBy((RustPlugin x) => x.Name), 
					"-t" => flag ? package.Plugins.OrderBy((RustPlugin x) => x.TotalHookTime) : package.Plugins.OrderByDescending((RustPlugin x) => x.TotalHookTime), 
					"-m" => flag ? package.Plugins.OrderBy((RustPlugin x) => x.TotalMemoryUsed) : package.Plugins.OrderByDescending((RustPlugin x) => x.TotalMemoryUsed), 
					"-f" => flag ? package.Plugins.OrderBy((RustPlugin x) => x.TotalHookFires) : package.Plugins.OrderByDescending((RustPlugin x) => x.TotalHookFires), 
					"-ls" => flag ? package.Plugins.OrderBy((RustPlugin x) => x.TotalHookLagSpikes) : package.Plugins.OrderByDescending((RustPlugin x) => x.TotalHookLagSpikes), 
					_ => flag ? package.Plugins.AsEnumerable().Reverse() : package.Plugins.AsEnumerable(), 
				})
				{
					stringTable.AddRow(string.Empty, item.Title, item.Author, $"v{item.Version}", (item.TotalHookTime.TotalMilliseconds == 0.0) ? string.Empty : $"{item.TotalHookTime.TotalMilliseconds:0}ms", (item.TotalHookFires == 0) ? string.Empty : $"{item.TotalHookFires:n0}", (item.TotalMemoryUsed == 0.0) ? string.Empty : (item.TotalMemoryUsed.Format(ByteEx.ByteTypes.Auto, shortName: true, "0.0", "{0}{1}").ToLower() ?? ""), (item.TotalHookLagSpikes == 0) ? string.Empty : $"{item.TotalHookLagSpikes:n0}", (item.TotalHookExceptions == 0) ? string.Empty : $"{item.TotalHookExceptions:n0}", item.IsPrecompiled ? string.Empty : $"{item.CompileTime.TotalMilliseconds:0}ms [{item.InternalCallHookGenTime.TotalMilliseconds:0}ms]", TimeEx.Format(item.Uptime) ?? "");
				}
				num++;
			}
			using StringTable stringTable2 = new StringTable("*", $"unloaded plugins ({enumerable.Count():n0})");
			foreach (string item2 in enumerable)
			{
				stringTable2.AddRow(string.Empty, Path.GetFileName(item2));
			}
			using StringTable table = new StringTable("*", $"failed plugins ({ModLoader.FailedCompilations.Count((KeyValuePair<string, ModLoader.CompilationResult> x) => x.Value.HasFailed()):n0})", "line", "stacktrace");
			foreach (ModLoader.CompilationResult value in ModLoader.FailedCompilations.Values)
			{
				if (!value.HasFailed())
				{
					continue;
				}
				ModLoader.Trace trace = value.Errors[0];
				SplitMessageUp(initial: true, table, value, trace, 0);
				foreach (ModLoader.Trace item3 in value.Errors.Skip(1))
				{
					SplitMessageUp(initial: true, table, value, item3, 0);
				}
			}
			arg.ReplyWith(stringTable.Write(StringTable.FormatTypes.None) + "\n" + stringTable2.Write(StringTable.FormatTypes.None) + "\n" + table.Write(StringTable.FormatTypes.None));
		}
		finally
		{
			((IDisposable)stringTable/*cast due to constrained. prefix*/).Dispose();
		}
		static void SplitMessageUp(bool initial, StringTable table2, ModLoader.CompilationResult compilation, ModLoader.Trace trace2, int skip)
		{
			bool flag2 = trace2.Message.Length - skip > 150;
			table2.AddRow(string.Empty, initial ? Path.GetFileName(compilation.File) : string.Empty, (flag2 || initial) ? $"{trace2.Line}:{trace2.Column}" : string.Empty, trace2.Message.Substring(skip, 150.Clamp(0, trace2.Message.Length - skip)) + (flag2 ? "..." : string.Empty));
			if (flag2)
			{
				SplitMessageUp(initial: false, table2, compilation, trace2, skip + 150);
			}
		}
	}

	[ConsoleCommand("reload", "Reloads all or specific plugins. E.g 'c.reload * <except[]>' to reload everything, 'c.reload PluginA [PluginB..]' to reload multiple..")]
	[AuthLevel(2)]
	private void Reload(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			return;
		}
		ProcessableFilesLookup();
		string fullString = arg.GetFullString();
		if (fullString == "*")
		{
			PooledList<RustPlugin> val = Pool.Get<PooledList<RustPlugin>>();
			try
			{
				ModLoader.Packages.GetAllHookables((List<RustPlugin>)(object)val, ignoreCore: true);
				foreach (RustPlugin item in (List<RustPlugin>)(object)val)
				{
					Puts($"Processing {item}");
					ProcessInput(item.Name, arg);
				}
				ModLoader.OnPluginProcessFinished();
				return;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		if (fullString.Contains(' '))
		{
			for (int i = 0; i < arg.Args.Length; i++)
			{
				ProcessInput(arg.GetString(i, ""), arg);
			}
		}
		else
		{
			ProcessInput(fullString, arg);
		}
		static void ProcessInput(string name, Arg arg2)
		{
			ProcessableFile pluginFile = GetPluginFile(name);
			RustPlugin plugin = ModLoader.FindPlugin(name);
			IBaseProcessor processor = pluginFile.GetProcessor();
			if (!string.IsNullOrEmpty(pluginFile.Path))
			{
				if (ProcessProcessor(pluginFile, processor))
				{
					return;
				}
				if (plugin != null && !plugin.IsPrecompiled)
				{
					if (plugin.HasInitialized)
					{
						if (Community.Runtime.Config.Watchers.ScriptWatchers)
						{
							Assemblies.RuntimeAssembly runtimeAssembly = Assemblies.Plugins.Get(plugin.Name);
							if (runtimeAssembly == null || Community.Runtime.MonoProfilerConfig.IsWhitelisted(MonoProfilerConfig.ProfileTypes.Plugin, plugin.Name) == runtimeAssembly.IsProfiledAssembly)
							{
								List<uint> hooks = Pool.Get<List<uint>>();
								List<HookMethodAttribute> hookMethods = Pool.Get<List<HookMethodAttribute>>();
								List<PluginReferenceAttribute> pluginReferences = Pool.Get<List<PluginReferenceAttribute>>();
								List<Plugin> requires = Pool.Get<List<Plugin>>();
								IBaseProcessor.IProcess process = plugin.ProcessorProcess;
								hooks.AddRange(plugin.Hooks);
								hookMethods.AddRange(plugin.HookMethods);
								pluginReferences.AddRange(plugin.PluginReferences);
								requires.AddRange(plugin.Requires);
								ModLoader.UninitializePlugin(plugin);
								ModLoader.InitializePlugin(plugin.GetType(), out var plugin2, plugin.Package, delegate(RustPlugin p)
								{
									p.IsCorePlugin = plugin.IsCorePlugin;
									p.HasConditionals = plugin.HasConditionals;
									p.IsExtension = plugin.IsExtension;
									p.Hooks = hooks.ToList();
									p.HookMethods = hookMethods.ToList();
									p.PluginReferences = pluginReferences.ToList();
									p.Requires = requires.ToArray();
									p.SetProcessor(plugin.Processor, process);
									p.CompileTime = plugin.CompileTime;
									p.InternalCallHookGenTime = plugin.InternalCallHookGenTime;
									p.InternalCallHookSource = plugin.InternalCallHookSource;
									p.FilePath = plugin.FilePath;
									p.FileName = plugin.FileName;
								});
								Pool.FreeUnmanaged<uint>(ref hooks);
								Pool.FreeUnmanaged<HookMethodAttribute>(ref hookMethods);
								Pool.FreeUnmanaged<PluginReferenceAttribute>(ref pluginReferences);
								Pool.FreeUnmanaged<Plugin>(ref requires);
								CarbonEventArgs e = Pool.Get<CarbonEventArgs>();
								e.Init(plugin2);
								Community.Runtime.Events.Trigger(CarbonEvent.PluginPreload, e);
								Pool.Free<CarbonEventArgs>(ref e);
								Plugin.InternalApplyAllPluginReferences();
								if (Community.AllProcessorsFinalized)
								{
									ModLoader.OnPluginProcessFinished();
								}
								HookCaller.CallStaticHook(3051933177u, plugin2);
								return;
							}
						}
						plugin.ProcessorProcess.MarkDirty();
					}
					return;
				}
			}
			if (plugin == null)
			{
				Community.Runtime.Core.LoadPlugin(arg2);
			}
			else if (plugin.IsPrecompiled)
			{
				Logger.Warn("Plugin " + name + " is a precompiled plugin which can only be reloaded programmatically.");
			}
		}
		static bool ProcessProcessor(ProcessableFile file, IBaseProcessor processor)
		{
			if (processor.IgnoreList.Contains(file.Path) || ModLoader.GetCompilationResult(file.Path).HasFailed())
			{
				processor.ClearIgnore(file.Path);
				if (processor.InstanceBuffer.TryGetValue(file.Id, out var value))
				{
					value.Clear();
				}
				processor.Prepare(file.Id, file.Path);
				return true;
			}
			return false;
		}
	}

	[ConsoleCommand("load", "Loads all mods and/or plugins. E.g 'c.load * <except[]>' to load everything, 'c.load PluginA [PluginB..]' to load multiple.")]
	[AuthLevel(2)]
	internal void LoadPlugin(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			Logger.Warn("You must provide the name of a plugin or use * to load all plugins.");
			return;
		}
		ProcessableFilesLookup();
		string fullString = arg.GetFullString();
		if (fullString == "*")
		{
			string except = arg.GetFullString(1);
			Community.Runtime.ScriptProcessor.IgnoreList.RemoveAll((string x) => !except.Any() || except.Any((char y) => x.Contains(y.ToString())));
			Community.Runtime.ZipScriptProcessor.IgnoreList.RemoveAll((string x) => !except.Any() || except.Any((char y) => x.Contains(y.ToString())));
			{
				foreach (ProcessableFile processableFile in ProcessableFiles)
				{
					IBaseProcessor processor = processableFile.GetProcessor();
					if (!except.Any(processableFile.Path.Contains) && !processor.InstanceBuffer.ContainsKey(processableFile.Id) && !processor.Exists(processableFile.Path))
					{
						processor.Prepare(processableFile.Id, processableFile.Path);
					}
				}
				return;
			}
		}
		if (fullString.Contains(' '))
		{
			for (int num = 0; num < arg.Args.Length; num++)
			{
				ProcessInput(arg.GetString(num, ""));
			}
		}
		else
		{
			ProcessInput(fullString);
		}
		static void ProcessInput(string name)
		{
			ProcessableFile pluginFile = GetPluginFile(name);
			if (!string.IsNullOrEmpty(pluginFile.Path))
			{
				IBaseProcessor processor2 = pluginFile.GetProcessor();
				processor2.ClearIgnore(pluginFile.Path);
				processor2.Prepare(pluginFile.Id, pluginFile.Path);
				Logger.Warn("Requested '" + pluginFile.Id + "' for compilation");
			}
			else
			{
				Logger.Warn("Plugin " + name + " was not found or was typed incorrectly.");
			}
		}
	}

	[ConsoleCommand("unload", "Unloads all mods and/or plugins. E.g 'c.unload * <except[]>' to unload everything, 'c.unload PluginA [PluginB..]' to unload multiple. They'll be marked as 'ignored'.")]
	[AuthLevel(2)]
	private unsafe void UnloadPlugin(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			Logger.Warn("You must provide the name of a plugin or use * to unload all plugins.");
			return;
		}
		ProcessableFilesLookup();
		string fullString = arg.GetFullString();
		if (fullString == "*")
		{
			IEnumerable<string> enumerable = from x in arg.Args.Skip(1)
				select ((object)(*(StringView*)(&x))/*cast due to constrained. prefix*/).ToString();
			Community.Runtime.ScriptProcessor.Clear(enumerable);
			Community.Runtime.ZipScriptProcessor.Clear(enumerable);
			PooledList<RustPlugin> val = Pool.Get<PooledList<RustPlugin>>();
			try
			{
				ModLoader.Packages.GetAllHookables((List<RustPlugin>)(object)val, ignoreCore: true);
				for (int num = 0; num < ((List<RustPlugin>)(object)val).Count; num++)
				{
					RustPlugin rustPlugin = ((List<RustPlugin>)(object)val)[num];
					if (!enumerable.Contains(rustPlugin.Name))
					{
						Plugin[] requires = rustPlugin.Requires;
						if (requires == null || requires.Length <= 0)
						{
							ModLoader.UninitializePlugin(rustPlugin);
							rustPlugin.Processor.Ignore(rustPlugin.Name);
						}
					}
				}
				return;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		if (fullString.Contains(' '))
		{
			for (int num2 = 0; num2 < arg.Args.Length; num2++)
			{
				ProcessInput(arg.GetString(num2, ""), arg);
			}
		}
		else
		{
			ProcessInput(fullString, arg);
		}
		static void ProcessInput(string name, Arg val2)
		{
			ProcessableFile pluginFile = GetPluginFile(name);
			if (!string.IsNullOrEmpty(pluginFile.Path))
			{
				pluginFile.GetProcessor().Ignore(pluginFile.Path);
			}
			RustPlugin rustPlugin2 = ModLoader.FindPlugin(name);
			if (rustPlugin2 != null)
			{
				ModLoader.UninitializePlugin(rustPlugin2);
			}
			else
			{
				val2.ReplyWith("Couldn't find a plugin with that name: " + name);
			}
		}
	}

	[ConsoleCommand("plugininfo", "Prints advanced information about a currently loaded plugin. From hooks, hook times, hook memory usage and other things.")]
	[AuthLevel(2)]
	private void PluginInfo(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			Logger.Warn("You must provide the name of a plugin to print plugin advanced information.");
			return;
		}
		string name = arg.GetString(0, "").ToLower();
		string text = arg.GetString(1, "");
		bool flag = arg.GetString(2, "").Equals("-asc");
		RustPlugin rustPlugin = ModLoader.Packages.FindPlugin(name);
		int num = 1;
		if (rustPlugin == null)
		{
			arg.ReplyWith("Couldn't find that plugin.");
			return;
		}
		using StringTable stringTable = new StringTable(string.Empty, "id", "hook", "time", "fires", "memory", "lag", "exceptions", "subscribed", "async / hooks");
		foreach (List<CachedHook> item in text switch
		{
			"-t" => (flag ? rustPlugin.HookPool.OrderBy((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.HookTime.TotalMilliseconds)) : rustPlugin.HookPool.OrderByDescending((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.HookTime.TotalMilliseconds))).Select((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks), 
			"-m" => (flag ? rustPlugin.HookPool.OrderBy((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.MemoryUsage)) : rustPlugin.HookPool.OrderByDescending((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.MemoryUsage))).Select((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks), 
			"-f" => (flag ? rustPlugin.HookPool.OrderBy((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.TimesFired)) : rustPlugin.HookPool.OrderByDescending((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.TimesFired))).Select((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks), 
			"-ls" => (flag ? rustPlugin.HookPool.OrderBy((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.LagSpikes)) : rustPlugin.HookPool.OrderByDescending((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.LagSpikes))).Select((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks), 
			"-ex" => (flag ? rustPlugin.HookPool.OrderBy((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.Exceptions)) : rustPlugin.HookPool.OrderByDescending((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks.Sum((CachedHook cachedHook2) => cachedHook2.Exceptions))).Select((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks), 
			_ => rustPlugin.HookPool.Select((KeyValuePair<uint, CachedHookInstance> x) => x.Value.Hooks), 
		})
		{
			if (item.Count == 0)
			{
				continue;
			}
			CachedHook cachedHook = item[0];
			string name2 = cachedHook.Method.Name;
			uint orAdd = HookStringPool.GetOrAdd(name2);
			if (rustPlugin.Hooks.Contains(orAdd))
			{
				double num2 = item.Sum((CachedHook x) => x.HookTime.TotalMilliseconds);
				double num3 = item.Sum((CachedHook x) => x.MemoryUsage);
				int count = item.Count;
				int num4 = item.Count((CachedHook x) => x.IsAsync);
				int num5 = item.Sum((CachedHook x) => x.TimesFired);
				int num6 = item.Sum((CachedHook x) => x.LagSpikes);
				int num7 = item.Sum((CachedHook x) => x.Exceptions);
				stringTable.AddRow(string.Empty, orAdd, name2 ?? "", (num2 == 0.0) ? string.Empty : $"{num2:0}ms", (num5 == 0) ? string.Empty : $"{num5:n0}", (num3 == 0.0) ? string.Empty : (num3.Format().ToLower() ?? ""), (num6 == 0) ? string.Empty : $"{num6:n0}", (num7 == 0) ? string.Empty : $"{num7:n0}", (!rustPlugin.IgnoredHooks.Contains(orAdd)) ? "*" : string.Empty, $"{num4:n0} / {count:n0}");
				num++;
			}
		}
		StringBuilder stringBuilder = Pool.Get<StringBuilder>();
		stringBuilder.AppendLine(string.Format("{0} v{1} by {2}{3}", new object[4]
		{
			rustPlugin.Name,
			rustPlugin.Version,
			rustPlugin.Author,
			rustPlugin.IsCorePlugin ? " [core]" : string.Empty
		}));
		stringBuilder.AppendLine("  Path:                   " + rustPlugin.FilePath);
		stringBuilder.AppendLine(string.Format("  Compile Time:           {0:0}ms{1}{2}", rustPlugin.CompileTime.TotalMilliseconds, rustPlugin.IsPrecompiled ? " [precompiled]" : string.Empty, rustPlugin.IsExtension ? " [ext]" : string.Empty));
		stringBuilder.AppendLine(string.Format("  Int.CallHook Gen Time:  {0:0}ms{1}{2}", rustPlugin.InternalCallHookGenTime.TotalMilliseconds, rustPlugin.IsPrecompiled ? " [precompiled]" : string.Empty, rustPlugin.IsExtension ? " [ext]" : string.Empty));
		stringBuilder.AppendLine("  Uptime:                 " + TimeEx.Format(rustPlugin.Uptime).ToLower());
		stringBuilder.AppendLine($"  Total Hook Time:        {rustPlugin.TotalHookTime.TotalMilliseconds:0}ms");
		stringBuilder.AppendLine("  Total Memory Used:      " + rustPlugin.TotalMemoryUsed.Format().ToLower());
		stringBuilder.AppendLine($"  Internal Hook Override: {rustPlugin.InternalCallHookOverriden}");
		stringBuilder.AppendLine($"  Has Conditionals:       {rustPlugin.HasConditionals}");
		stringBuilder.AppendLine(string.Format("  Mod Package:            {0} ({1}){2}", rustPlugin.Package.Name, rustPlugin.Package.PluginCount, rustPlugin.Package.IsCoreMod ? " [core]" : string.Empty));
		stringBuilder.AppendLine("  Processor:              " + ((rustPlugin.Processor == null) ? "[standalone]" : (rustPlugin.Processor.Name + " [" + rustPlugin.Processor.Extension + "]")));
		if (rustPlugin is CarbonPlugin carbonPlugin)
		{
			stringBuilder.AppendLine($"  Carbon CUI:             {carbonPlugin.CuiHandler.Pooled:n0} pooled, {carbonPlugin.CuiHandler.Used:n0} used");
		}
		string[] permissions = rustPlugin.permission.GetPermissions(rustPlugin);
		stringBuilder.AppendLine("  Permissions:            " + ((permissions.Length != 0) ? permissions.ToString("\n                          ") : "N/A"));
		stringBuilder.AppendLine(string.Empty);
		if (num != 1)
		{
			stringBuilder.AppendLine(stringTable.ToStringMinimal());
		}
		arg.ReplyWith(stringBuilder.ToString());
		Pool.FreeUnmanaged(ref stringBuilder);
	}

	[ConsoleCommand("plugincmds", "Prints a full list of chat and console commands for a specific plugin.")]
	[AuthLevel(2)]
	private void PluginCmds(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			Logger.Warn("You must provide the name of a plugin to print plugin command information.");
			return;
		}
		string name = arg.GetString(0, "").ToLower();
		RustPlugin plugin = ModLoader.Packages.SelectMany((ModLoader.Package x) => x.Plugins).FirstOrDefault((RustPlugin x) => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) || StringEx.Contains(x.Name, name, CompareOptions.OrdinalIgnoreCase));
		if (plugin == null)
		{
			arg.ReplyWith("Couldn't find that plugin.");
			return;
		}
		StringBuilder stringBuilder = Pool.Get<StringBuilder>();
		int num = 1;
		StringTable stringTable = new StringTable("chat commands");
		try
		{
			foreach (API.Commands.Command item in Community.Runtime.CommandManager.Chat.Where((API.Commands.Command x) => x.Reference == plugin).Distinct())
			{
				if (!item.HasFlag(CommandFlags.Protected) && !item.HasFlag(CommandFlags.Hidden))
				{
					stringTable.AddRow(item.Name);
					num++;
				}
			}
			stringBuilder.AppendLine(stringTable.ToStringMinimal());
		}
		finally
		{
			((IDisposable)stringTable/*cast due to constrained. prefix*/).Dispose();
		}
		StringTable stringTable2 = new StringTable("console commands");
		try
		{
			num = 1;
			foreach (API.Commands.Command item2 in Community.Runtime.CommandManager.ClientConsole.Where((API.Commands.Command x) => x.Reference == plugin))
			{
				if (!item2.HasFlag(CommandFlags.Protected) && !item2.HasFlag(CommandFlags.Hidden))
				{
					stringTable2.AddRow(item2.Name);
					num++;
				}
			}
			stringBuilder.AppendLine(stringTable2.ToStringMinimal());
		}
		finally
		{
			((IDisposable)stringTable2/*cast due to constrained. prefix*/).Dispose();
		}
		arg.ReplyWith(stringBuilder.ToString());
		Pool.FreeUnmanaged(ref stringBuilder);
	}

	[ConsoleCommand("reloadconfig", "Reloads a plugin's config file. This might have unexpected results, use cautiously.")]
	[AuthLevel(2)]
	private void ReloadConfig(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			Logger.Warn("You must provide the name of a plugin or use * to reload all plugin configs.");
			return;
		}
		ProcessableFilesLookup();
		string text = arg.GetString(0, "");
		if (text == "*")
		{
			foreach (ModLoader.Package package in ModLoader.Packages)
			{
				foreach (RustPlugin plugin in package.Plugins)
				{
					plugin.ILoadConfig();
					plugin.Load();
					plugin.Puts("Reloaded plugin's config.");
				}
			}
			return;
		}
		bool flag = false;
		foreach (ModLoader.Package package2 in ModLoader.Packages)
		{
			List<RustPlugin> list = Pool.Get<List<RustPlugin>>();
			list.AddRange(package2.Plugins);
			foreach (RustPlugin item in list)
			{
				if (item.Name.Equals(text, StringComparison.InvariantCultureIgnoreCase))
				{
					item.ILoadConfig();
					item.Load();
					item.Puts("Reloaded plugin's config.");
					flag = true;
				}
			}
			Pool.FreeUnmanaged<RustPlugin>(ref list);
		}
		if (!flag)
		{
			Logger.Warn("Plugin " + text + " was not found or was typed incorrectly.");
		}
	}

	[ConsoleCommand("uninstallplugin", "Unloads and uninstalls (moves the file to the backup folder) the plugin with the name.")]
	[AuthLevel(2)]
	private void UninstallPlugin(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			Logger.Warn("You must provide the name of a plugin to uninstall it.");
			return;
		}
		ProcessableFilesLookup();
		string name = arg.GetString(0, "");
		ProcessableFile pluginFile = GetPluginFile(name);
		bool flag = false;
		bool flag2 = false;
		foreach (ModLoader.Package package in ModLoader.Packages)
		{
			PooledList<RustPlugin> val = Pool.Get<PooledList<RustPlugin>>();
			try
			{
				((List<RustPlugin>)(object)val).AddRange((IEnumerable<RustPlugin>)package.Plugins);
				foreach (RustPlugin item in ((IEnumerable<RustPlugin>)val).Where((RustPlugin plugin) => plugin.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
				{
					flag = true;
					if (item.IsPrecompiled)
					{
						flag2 = true;
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		if (!flag)
		{
			if (string.IsNullOrEmpty(pluginFile.Path))
			{
				Logger.Warn("Plugin " + name + " was not found or was typed incorrectly.");
			}
			else
			{
				Logger.Warn("Plugin " + name + " was not loaded but was marked as ignored.");
			}
		}
		else if (flag2)
		{
			Logger.Warn("Plugin " + pluginFile.Id + " is a precompiled plugin which can only be unloaded/uninstalled programmatically.");
		}
		else
		{
			OsEx.File.Move(pluginFile.Path, Path.Combine(Defines.GetScriptBackupFolder(), Path.GetFileName(pluginFile.Path)));
		}
	}

	[ConsoleCommand("installplugin", "Looks up the backups directory and moves the plugin back in the plugins folder installing it with the name.")]
	[AuthLevel(2)]
	private void InstallPlugin(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			Logger.Warn("You must provide the name of a plugin to uninstall it.");
			return;
		}
		ProcessableFilesLookup();
		string text = arg.GetString(0, "");
		string text2 = Path.Combine(Defines.GetScriptBackupFolder(), text + ".cs");
		if (!OsEx.File.Exists(text2))
		{
			Logger.Warn("Plugin " + text + " was not found or was typed incorrectly.");
		}
		else
		{
			OsEx.File.Move(text2, Path.Combine(Defines.GetScriptsFolder(), Path.GetFileName(text2)));
		}
	}

	[ConsoleCommand("profile", "Toggles recording status of the Carbon native Mono-profiling. Syntax: c.profile [duration] [-cm] [-am] [-t] [-c] [-gc]")]
	[AuthLevel(2)]
	private void Profile(Arg arg)
	{
		if (!MonoProfiler.Enabled)
		{
			arg.ReplyWith("Mono profiler is disabled. Enable it in the 'carbon/config.profiler.json' config file. Must restart the server for changes to apply.");
			return;
		}
		float duration = arg.GetFloat(0, 0f);
		MonoProfiler.ProfilerArgs flags = MonoProfiler.ProfilerArgs.None;
		if (arg.HasArg("-cm", false))
		{
			flags |= MonoProfiler.ProfilerArgs.CallMemory;
		}
		if (arg.HasArg("-am", false))
		{
			flags |= MonoProfiler.ProfilerArgs.AdvancedMemory;
		}
		if (arg.HasArg("-t", false))
		{
			flags |= MonoProfiler.ProfilerArgs.Timings;
		}
		if (arg.HasArg("-c", false))
		{
			flags |= MonoProfiler.ProfilerArgs.Calls;
		}
		if (arg.HasArg("-gc", false))
		{
			flags |= MonoProfiler.ProfilerArgs.GCEvents;
		}
		if (arg.HasArg("-sw", false))
		{
			flags |= MonoProfiler.ProfilerArgs.StackWalkAllocations;
		}
		if (flags == MonoProfiler.ProfilerArgs.None)
		{
			flags = MonoProfiler.ProfilerArgs.CallMemory | MonoProfiler.ProfilerArgs.AdvancedMemory | MonoProfiler.ProfilerArgs.Timings | MonoProfiler.ProfilerArgs.Calls | MonoProfiler.ProfilerArgs.GCEvents;
		}
		if (MonoProfiler.IsRecording)
		{
			Analytics.profiler_ended(flags, MonoProfiler.CurrentDurationTime.TotalSeconds, timed: false);
			MonoProfiler.ToggleProfiling(flags);
			ProfileSample.Resample();
			MonoProfiler.Clear();
			return;
		}
		if (duration <= 0f)
		{
			MonoProfiler.ToggleProfiling(flags);
			Analytics.profiler_started(flags, timed: false);
			return;
		}
		MonoProfiler.ToggleProfilingTimed(duration, flags, delegate
		{
			Analytics.profiler_ended(flags, duration, timed: true);
			ProfileSample.Resample();
			MonoProfiler.Clear();
		});
		Analytics.profiler_started(flags, timed: true);
	}

	[ConsoleCommand("profileabort", "Aborts recording of the Carbon native Mono-profiling if it was recording.")]
	[AuthLevel(2)]
	private void ProfileAbort(Arg arg)
	{
		if (!MonoProfiler.IsRecording)
		{
			arg.ReplyWith("No profiling process active");
			return;
		}
		MonoProfiler.ToggleProfiling(MonoProfiler.ProfilerArgs.Abort);
		ProfileSample.Clear();
	}

	[ConsoleCommand("profiler.print", "If any parsed data available, it'll print basic and advanced information. (-c=CSV, -j=JSON, -t=Table, -p=ProtoBuf [default])")]
	[AuthLevel(2)]
	private void ProfilerPrint(Arg arg)
	{
		if (MonoProfiler.IsRecording)
		{
			arg.ReplyWith("Profiler is actively recording");
			return;
		}
		switch (arg.GetString(0, ""))
		{
		case "-c":
			arg.ReplyWith(WriteFileString("csv", ProfileSample.ToCSV()));
			break;
		case "-j":
			arg.ReplyWith(WriteFileString("json", ProfileSample.ToJson(indented: true)));
			break;
		case "-t":
			arg.ReplyWith(WriteFileString("txt", ProfileSample.ToTable()));
			break;
		default:
			arg.ReplyWith(WriteFileBytes("cprf", ProfileSample.ToProto()));
			break;
		}
		static string WriteFileBytes(string extension, byte[] data)
		{
			DateTime now = DateTime.Now;
			string text = Path.Combine(Defines.GetProfilesFolder(), string.Format("profile-{0}_{1}_{2}_{3}{4}{5}.{6}", new object[7] { now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, extension }));
			OsEx.File.Create(text, data);
			return "Exported profile output at '" + text + "'";
		}
		static string WriteFileString(string extension, string data)
		{
			DateTime now = DateTime.Now;
			string text = Path.Combine(Defines.GetProfilesFolder(), string.Format("profile-{0}_{1}_{2}_{3}{4}{5}.{6}", new object[7] { now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, extension }));
			OsEx.File.Create(text, data);
			return "Exported profile output at '" + text + "'";
		}
	}

	[ConsoleCommand("profiler.tracks", "All tracking lists present in the config which are used by the Mono profiler for tracking.")]
	[AuthLevel(2)]
	private void ProfilerTracked(Arg arg)
	{
		arg.ReplyWith($"Tracked Assemblies ({Community.Runtime.MonoProfilerConfig.Assemblies.Count:n0}):\n" + Community.Runtime.MonoProfilerConfig.Assemblies.Select((string x) => "- " + x).ToString("\n") + "\n" + $"Tracked Plugins ({Community.Runtime.MonoProfilerConfig.Plugins.Count:n0}):\n" + Community.Runtime.MonoProfilerConfig.Plugins.Select((string x) => "- " + x).ToString("\n") + "\n" + $"Tracked Modules ({Community.Runtime.MonoProfilerConfig.Modules.Count:n0}):\n" + Community.Runtime.MonoProfilerConfig.Modules.Select((string x) => "- " + x).ToString("\n") + "\n" + $"Tracked Extensions ({Community.Runtime.MonoProfilerConfig.Extensions.Count:n0}):\n" + Community.Runtime.MonoProfilerConfig.Extensions.Select((string x) => "- " + x).ToString("\n") + "\nUse wildcard (*) to include all.");
	}

	[ConsoleCommand("profiler.track", "Adds an object to be tracked. Reloading the plugin will start tracking. Restarting required for assemblies, modules and extensions.")]
	[AuthLevel(2)]
	private void ProfilerTrackPlugin(Arg arg)
	{
		if (!arg.HasArgs(2))
		{
			InvalidReturn(arg);
			return;
		}
		string text = arg.GetString(0, "");
		string text2 = arg.GetString(1, "");
		MonoProfilerConfig.ProfileTypes profileTypes = MonoProfilerConfig.ProfileTypes.Assembly;
		bool flag = text switch
		{
			"assembly" => Community.Runtime.MonoProfilerConfig.AppendProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Assembly, text2), 
			"plugin" => Community.Runtime.MonoProfilerConfig.AppendProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Plugin, text2), 
			"module" => Community.Runtime.MonoProfilerConfig.AppendProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Module, text2), 
			"ext" => Community.Runtime.MonoProfilerConfig.AppendProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Extension, text2), 
			_ => InvalidReturn(arg), 
		};
		arg.ReplyWith(flag ? $" Added {profileTypes} object '{text2}' to tracking" : $" Couldn't add {profileTypes} object '{text2}' for tracking");
		if (flag)
		{
			Community.Runtime.SaveMonoProfilerConfig();
		}
		static bool InvalidReturn(Arg val)
		{
			val.ReplyWith("Syntax: c.profiler.track (assembly|plugin|module|ext) value");
			return false;
		}
	}

	[ConsoleCommand("profiler.untrack", "Removes a plugin from being tracked. Reloading the plugin will remove it from being tracked. Restarting required for assemblies, modules and extensions.")]
	[AuthLevel(2)]
	private void ProfilerRemovePlugin(Arg arg)
	{
		if (!arg.HasArgs(2))
		{
			InvalidReturn(arg);
			return;
		}
		string text = arg.GetString(0, "");
		string text2 = arg.GetString(1, "");
		MonoProfilerConfig.ProfileTypes profileTypes = MonoProfilerConfig.ProfileTypes.Assembly;
		bool flag = text switch
		{
			"assembly" => Community.Runtime.MonoProfilerConfig.RemoveProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Assembly, text2), 
			"plugin" => Community.Runtime.MonoProfilerConfig.RemoveProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Plugin, text2), 
			"module" => Community.Runtime.MonoProfilerConfig.RemoveProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Module, text2), 
			"ext" => Community.Runtime.MonoProfilerConfig.RemoveProfile(profileTypes = MonoProfilerConfig.ProfileTypes.Extension, text2), 
			_ => InvalidReturn(arg), 
		};
		arg.ReplyWith(flag ? $" Removed {profileTypes} object '{text2}' from tracking" : $" Couldn't remove {profileTypes} object '{text2}' for tracking");
		if (flag)
		{
			Community.Runtime.SaveMonoProfilerConfig();
		}
		static bool InvalidReturn(Arg val)
		{
			val.ReplyWith("Syntax: c.profiler.untrack (assembly|plugin|module|ext) value");
			return false;
		}
	}

	[ConsoleCommand("skin", "Allowing you to get/change the skin ID of a deployed entity you're looking at.")]
	[AuthLevel(1)]
	private void Skin(Arg arg)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer val = ArgEx.Player(arg);
		RaycastHit val3 = default(RaycastHit);
		BaseEntity val2 = (Physics.Raycast(val.eyes.HeadRay(), ref val3, 10f, -1, (QueryTriggerInteraction)1) ? RaycastHitEx.GetEntity(val3) : null);
		if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)val2))
		{
			arg.ReplyWith($"{val2} skin: {val2.skinID}");
			val2.skinID = arg.GetULong(0, val2.skinID);
			((BaseNetworkable)val2).SendNetworkUpdate((NetworkQueue)0);
		}
		else
		{
			arg.ReplyWith("Couldn't find entity");
		}
	}

	[ConsoleCommand("test_plugin", "Executes a collection of tests found inside of the plugin, designed to ensure plugin logic integrity. Eg. c.test_plugin <plugin_name> [<channel|1>] [<delay|0.1>]")]
	[AuthLevel(2)]
	private void test_plugin(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			arg.ReplyWith("Syntax: c.test_plugin <plugin_name> [<channel|1>] [<delay|0.1>]");
			return;
		}
		string name = arg.GetString(0, "");
		int channel = arg.GetInt(1, 1);
		float delay = arg.GetFloat(2, 0.1f);
		Integrations.Clear(channel);
		RustPlugin rustPlugin = ModLoader.Packages.FindPlugin(name);
		if (rustPlugin == null)
		{
			arg.ReplyWith("Couldn't find that plugin");
			return;
		}
		rustPlugin.CollectTests();
		rustPlugin.NextFrame(delegate
		{
			Integrations.Run(delay, channel);
		});
		arg.ReplyWith(string.Format("Collected {0:n0} {1} for '{2}' and now running..", rustPlugin.TestCount, rustPlugin.TestCount.Plural("test", "tests"), rustPlugin));
	}

	[ConsoleCommand("test_collect", "Collects all available tests from all plugins and enabled modules currently loaded. Eg. c.test_collect [<channel|1>]")]
	[AuthLevel(2)]
	private void test_collect(Arg arg)
	{
		int channel = arg.GetInt(0, 1);
		PooledList<RustPlugin> val = Pool.Get<PooledList<RustPlugin>>();
		try
		{
			ModLoader.Packages.GetAllHookables((List<RustPlugin>)(object)val, ignoreCore: true);
			int num = 0;
			int num2 = 0;
			foreach (BaseHookable module in Community.Runtime.ModuleProcessor.Modules)
			{
				module.CollectTests();
				num += module.TestCount;
			}
			foreach (RustPlugin item in (List<RustPlugin>)(object)val)
			{
				item.CollectTests(channel);
				num2 += item.TestCount;
			}
			arg.ReplyWith(string.Format("Collected {0:n0} module and {1:n0} plugin {2}. Run c.test_beds to display all or c.test_run to execute.", num, num2, (num + num2).Plural("test", "tests")));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ConsoleCommand("test_beds", "Prints all currently queued up tests ready to be executed.")]
	[AuthLevel(2)]
	private void test_beds(Arg arg)
	{
		StringTable stringTable = new StringTable(string.Empty, "context", "tests", "channel");
		try
		{
			foreach (Integrations.TestBank item in Integrations.Banks.Values.SelectMany((Queue<Integrations.TestBank> bank) => bank))
			{
				stringTable.AddRow(string.Empty, item.Context, $"{item.Count:n0}", item.Channel.ToString());
			}
			arg.ReplyWith(stringTable.ToStringMinimal());
		}
		finally
		{
			((IDisposable)stringTable/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ConsoleCommand("test_run", "Executes all Test Beds that are currently queued up. Eg. c.test_run <channel|-1> [<delay|0.1>]")]
	[AuthLevel(2)]
	private void test_run(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			arg.ReplyWith("Syntax: c.test_run <channel|-1> [<delay|0.1>]");
			return;
		}
		int channel = arg.GetInt(0, -1);
		float num = arg.GetFloat(1, 0.1f);
		if (num < 0f)
		{
			arg.ReplyWith("Delay must be above or equal to zero.");
		}
		else
		{
			Integrations.Run(num, channel);
		}
	}

	[ConsoleCommand("test_clear", "Clears all Test Beds that are currently queued up. Eg. c.test_clear [<channel|-1>]")]
	[AuthLevel(2)]
	private void test_clear(Arg arg)
	{
		Integrations.Clear(arg.GetInt(0, -1));
	}

	[AuthLevel(2)]
	[ConsoleCommand("vault", "Prints a whole list of all vault factory and item keys without any protected values")]
	private void GetVault(Arg arg)
	{
		StringTable stringTable = new StringTable("factory", "items", "encrypted", "value");
		try
		{
			List<Vault.Factory> factories = Vault.GetFactories();
			foreach (Vault.Factory item3 in factories)
			{
				Vault.Item item = ((item3.Count > 0) ? item3[0] : null);
				stringTable.AddRow(" " + Vault.Pool.Get(item3.id), (item == null) ? string.Empty : Vault.Pool.Get(item.id), item?.encrypted, (item == null || item.encrypted) ? string.Empty : item.Cache);
				for (int i = 1; i < item3.Count; i++)
				{
					Vault.Item item2 = item3[i];
					stringTable.AddRow(string.Empty, Vault.Pool.Get(item2.id), item2.encrypted, item2.encrypted ? string.Empty : item2.Cache);
				}
			}
			arg.ReplyWith(stringTable.ToStringMinimal());
		}
		finally
		{
			((IDisposable)stringTable/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[AuthLevel(2)]
	[ConsoleCommand("vault_add", "Adds a new element to the vault")]
	private void VaultAdd(Arg arg)
	{
		if (!arg.HasArgs(2))
		{
			arg.ReplyWith("Syntax: c.vault_add <key> <value> [encrypted|true] [factory|global]");
			return;
		}
		string text = arg.GetString(0, "");
		string value = arg.GetString(1, "");
		bool flag = arg.GetBool(2, true);
		string text2 = arg.GetString(3, Vault.Global);
		bool valueOrDefault = Vault.GetFactory(Vault.Pool.Get(text2))?.HasItem(Vault.Pool.Get(text)) == true;
		arg.ReplyWith((!Vault.Add(text2, text, value, flag)) ? "Couldn't add a new vault factory item in Carbon.Vault, probably because invalid parameters" : (valueOrDefault ? ("Updated vault factory " + (flag ? "encrypted" : "unencrypted") + " item '" + text + "' for factory '" + text2 + "'") : ("Added new vault factory " + (flag ? "encrypted" : "unencrypted") + " item '" + text + "' for factory '" + text2 + "'")));
	}

	[AuthLevel(2)]
	[ConsoleCommand("vault_remove", "Removes an element from the vault")]
	private void VaultRemove(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			arg.ReplyWith("Syntax: c.vault_remove <key> [factory|global]");
			return;
		}
		string text = arg.GetString(0, "");
		string text2 = arg.GetString(1, Vault.Global);
		arg.ReplyWith(Vault.Remove(text2, text) ? ("Removed vault factory item '" + text + "' for factory '" + text2 + "'") : "Couldn't remove a vault factory item from Carbon.Vault, probably because it doesn't");
	}

	[ConsoleCommand("webpanel.loadcfg", "Loads the Carbon WebControlPanel configuration (refreshes authorization accounts)")]
	[AuthLevel(2)]
	private void LoadWebControlPanelConfig(Arg arg)
	{
		WebControlPanel.LoadConfig();
	}

	[ConsoleCommand("webpanel.savecfg", "Saves the Carbon WebControlPanel configuration")]
	[AuthLevel(2)]
	private void SaveWebControlPanelConfig(Arg arg)
	{
		WebControlPanel.SaveConfig();
	}

	[ConsoleCommand("webpanel.setport", "Update the WebControlPanel server port")]
	[AuthLevel(2)]
	private void SetWebControlPanelPort(Arg arg)
	{
		WebControlPanel.config.BridgeServer.Port = arg.GetInt(0, WebControlPanel.config.BridgeServer.Port);
		WebControlPanel.SaveConfig();
		WebControlPanel.RestartServer();
	}

	[ConsoleCommand("webpanel.setenabled", "Should the WebControlPanel server be started/stopped")]
	[AuthLevel(2)]
	private void TryToggleWebControlPanelServer(Arg arg)
	{
		WebControlPanel.config.Enabled = arg.GetBool(0, false);
		WebControlPanel.SaveConfig();
		WebControlPanel.RestartServer();
	}

	[ConsoleCommand("webpanel.clients", "Print all WebControlPanel clients")]
	[AuthLevel(2)]
	private void GetWebControlPanelClients(Arg arg)
	{
		if (!IsWebControlPanelServerConnected)
		{
			arg.ReplyWith("The WebControlPanel server isn't connected");
			return;
		}
		using StringTable stringTable = new StringTable("id", "address", "account");
		for (int i = 0; i < WebControlPanel.server.ConnectionsList.Count; i++)
		{
			BridgeConnection bridgeConnection = WebControlPanel.server.Connections[i];
			stringTable.AddRow($"{bridgeConnection.Id}", $"{bridgeConnection.Socket.ConnectionInfo.ClientIpAddress}:{bridgeConnection.Socket.ConnectionInfo.ClientPort}", (!(bridgeConnection.Reference is WebControlPanel.Account account)) ? "N/A" : account.Name);
		}
		arg.ReplyWith(stringTable.ToStringMinimal());
	}

	public static string[] AllocateBuffer(int count)
	{
		if (_argumentBuffer.TryGetValue(count, out var value))
		{
			return value.Rent();
		}
		value = (_argumentBuffer[count] = new ArgPool(count));
		return value.Rent();
	}

	public static void ReturnBuffer(string[] buffer)
	{
		if (buffer != null && _argumentBuffer.TryGetValue(buffer.Length, out var value))
		{
			value.Return(buffer);
		}
	}

	public static object IOnPlayerCommand(BasePlayer player, string message, API.Commands.Command.Prefix prefix)
	{
		if (Community.Runtime == null)
		{
			return Cache.True;
		}
		try
		{
			if (!ConsoleArgEx.TryParseCommand(message.AsSpan().Slice(prefix.Value.Length), out var command, out var args))
			{
				return Cache.False;
			}
			string[] array = AllocateBuffer(args.Length);
			for (int i = 0; i < args.Length; i++)
			{
				array[i] = args[i]?.ToString();
			}
			if (HookCaller.CallStaticHook(2198880635u, player, command, array) != null)
			{
				ReturnBuffer(array);
				return Cache.False;
			}
			if (HookCaller.CallStaticHook(2198880635u, player.AsIPlayer(), command, array) != null)
			{
				ReturnBuffer(array);
				return Cache.False;
			}
			if (HookCaller.CallStaticHook(2915735597u, player, command, array) != null)
			{
				ReturnBuffer(array);
				return Cache.False;
			}
			ReturnBuffer(array);
			if (Community.Runtime.CommandManager.Contains(Community.Runtime.CommandManager.Chat, command, out var outCommand))
			{
				PlayerArgs playerArgs = Pool.Get<PlayerArgs>();
				playerArgs.Type = outCommand.Type;
				playerArgs.Arguments = args;
				playerArgs.Player = player;
				playerArgs.PrintOutput = true;
				Community.Runtime.CommandManager.Execute(outCommand, playerArgs);
				Pool.Free<PlayerArgs>(ref playerArgs);
				return Cache.False;
			}
			if (player.Connection.authLevel < prefix.SuggestionAuthLevel)
			{
				goto IL_0289;
			}
			IEnumerable<Suggestions.SuggestionResult> source = Suggestions.Lookup(command, Community.Runtime.CommandManager.Chat.Select((API.Commands.Command x) => x.Name), 3, 5);
			if (!source.Any())
			{
				goto IL_0289;
			}
			CorePlugin core = Community.Runtime.Core;
			string message2 = core.lang.GetMessage("unknown_chat_cmd_2", core, player.UserIDString);
			string message3 = core.lang.GetMessage("unknown_chat_cmd_separator_1", core, player.UserIDString);
			string message4 = core.lang.GetMessage("unknown_chat_cmd_separator_2", core, player.UserIDString);
			string text = string.Format(message2, message, source.Select((Suggestions.SuggestionResult x) => prefix.Value + x.Result).ToString(message3, message4));
			player.SendConsoleCommand("chat.add", new object[3]
			{
				2,
				Community.Runtime.Core.DefaultServerChatId,
				text
			});
			goto end_IL_001b;
			IL_0289:
			CorePlugin core2 = Community.Runtime.Core;
			string message5 = core2.lang.GetMessage("unknown_chat_cmd_1", core2, player.UserIDString);
			string text2 = string.Format(message5, message);
			player.SendConsoleCommand("chat.add", new object[3]
			{
				2,
				Community.Runtime.Core.DefaultServerChatId,
				text2
			});
			end_IL_001b:;
		}
		catch (Exception ex)
		{
			Logger.Error("Failed IOnPlayerCommand.", ex);
		}
		return Cache.False;
	}

	internal static object IOnServerCommand(Arg arg)
	{
		if (arg != null && arg.cmd != null && (Object)(object)ArgEx.Player(arg) != (Object)null && arg.cmd.FullName == "chat.say")
		{
			return null;
		}
		if (HookCaller.CallStaticHook(2535152661u, arg) == null)
		{
			return null;
		}
		return Cache.True;
	}

	public static object IOnPlayerChat(ulong playerId, string playerName, ref string message, ChatChannel channel, BasePlayer basePlayer)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(message) || message.Equals("text"))
		{
			return Cache.True;
		}
		message = StringEx.EscapeRichText(message, false);
		if ((Object)(object)basePlayer == (Object)null || !basePlayer.IsConnected)
		{
			return HookCaller.CallStaticHook(4068177051u, playerId, playerName, message, channel);
		}
		object obj = HookCaller.CallStaticHook(2032160890u, basePlayer, message, channel);
		object obj2 = HookCaller.CallStaticHook(2894159933u, basePlayer.AsIPlayer(), message);
		return obj ?? obj2;
	}

	internal static object IOnRconInitialize()
	{
		if (Community.Runtime.Config.Rcon)
		{
			return null;
		}
		return Cache.False;
	}

	internal static object IOnRunCommandLine()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<string, string> @switch in CommandLine.GetSwitches())
		{
			string text = @switch.Value;
			if (text == "")
			{
				text = "1";
			}
			string text2 = @switch.Key.Substring(1);
			Option unrestricted = Option.Unrestricted;
			((Option)(ref unrestricted)).PrintOutput = false;
			ConsoleSystem.Run(unrestricted, text2, new object[1] { text });
		}
		return Cache.False;
	}

	internal static object IOnPlayerConnected(BasePlayer player)
	{
		CorePlugin corePlugin = RustPlugin.Singleton<CorePlugin>();
		player.SendEntitySnapshot((BaseNetworkable)(object)CommunityEntity.ServerInstance);
		corePlugin.permission.RefreshUser(player);
		HookCaller.CallStaticHook(2848347654u, player);
		HookCaller.CallStaticHook(1253832323u, player.AsIPlayer());
		CustomVitalManager.SendVitals(player);
		return null;
	}

	internal static object IOnUserApprove(Connection connection)
	{
		string username = connection.username;
		string arg = connection.userid.ToString();
		string arg2 = Regex.Replace(connection.ipaddress, Player.ipPattern, string.Empty);
		object obj = HookCaller.CallStaticHook(3081308902u, connection);
		object obj2 = HookCaller.CallStaticHook(1045800646u, username, arg, arg2);
		object obj3 = ((obj == null) ? obj2 : obj);
		if (obj3 is string || (obj3 is bool && !(bool)obj3))
		{
			ConnectionAuth.Reject(connection, (obj3 is string) ? obj3.ToString() : "Connection was rejected", (string)null);
			return Cache.True;
		}
		if (HookCaller.CallStaticHook(2666432541u, connection) != null)
		{
			return HookCaller.CallStaticHook(1330253375u, username, arg, arg2);
		}
		return null;
	}

	internal unsafe static object IOnPlayerBanned(Connection connection, AuthResponse status)
	{
		HookCaller.CallStaticHook(140408349u, connection, ((object)(*(AuthResponse*)(&status))/*cast due to constrained. prefix*/).ToString());
		return null;
	}

	private void OnPlayerDisconnected(BasePlayer player, string reason)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		HookCaller.CallStaticHook(649612044u, player?.AsIPlayer(), reason);
		if (!player.IsAdmin || player.IsOnGround())
		{
			return;
		}
		Vector3 position = ((Component)player).transform.position;
		RaycastHit val = default(RaycastHit);
		if (!Physics.Raycast(position, Vector3.down, ref val, float.MaxValue, -1, (QueryTriggerInteraction)1))
		{
			return;
		}
		position.y = ((RaycastHit)(ref val)).point.y;
		if (!(Vector3.Distance(((Component)player).transform.position, position) > 3.5f))
		{
			return;
		}
		player.SetServerFall(false);
		player.Teleport(position);
		player.estimatedVelocity = Vector3.zero;
		NextFrame(delegate
		{
			if ((Object)(object)player != (Object)null)
			{
				player.SetServerFall(true);
			}
		});
		Logger.Warn($"Moved admin player {((BaseNetworkable)player).net.connection} on the object underneath so it doesn't die from fall damage.");
	}

	private void OnPlayerKicked(BasePlayer basePlayer, string reason)
	{
		HookCaller.CallStaticHook(3928650942u, basePlayer.AsIPlayer(), reason);
	}

	private object OnPlayerRespawn(BasePlayer basePlayer)
	{
		return HookCaller.CallStaticHook(3398288406u, basePlayer.AsIPlayer());
	}

	private void OnPlayerRespawned(BasePlayer basePlayer)
	{
		HookCaller.CallStaticHook(960522643u, basePlayer.AsIPlayer());
	}

	private void OnClientAuth(Connection connection)
	{
		connection.username = Regex.Replace(connection.username, "<[^>]*>", string.Empty);
	}

	internal object IOnServerInitialized(bool inited)
	{
		if (!Community.IsServerInitialized)
		{
			Community.IsServerInitialized = true;
			Analytics.on_server_initialized();
		}
		Community.Runtime.MarkServerInitialized(wants: true);
		Community.Runtime.Events.Trigger(CarbonEvent.OnServerInitialized, EventArgs.Empty);
		return null;
	}

	internal static object IOnServerInitialized()
	{
		return Community.Runtime.Core.IOnServerInitialized(inited: true);
	}

	internal static object IOnServerShutdown()
	{
		Logger.Log("Saving plugin configuration and data..");
		List<BaseHookable> list = Pool.Get<List<BaseHookable>>();
		list.AddRange(Community.Runtime.ModuleProcessor.Modules);
		foreach (BaseHookable item in list)
		{
			if (item is BaseModule baseModule)
			{
				try
				{
					baseModule.Shutdown();
				}
				catch (Exception ex)
				{
					Logger.Error($"Failed shutting down module '{baseModule.Name} v{baseModule.Version}'", ex);
				}
			}
		}
		Pool.FreeUnmanaged<BaseHookable>(ref list);
		HookCaller.CallStaticHook(2414711472u);
		HookCaller.CallStaticHook(2396958305u);
		Logger.Log("Shutting down Carbon..");
		Interface.Oxide.OnShutdown();
		WebControlPanel.Shutdown();
		PooledList<RustPlugin> val = Pool.Get<PooledList<RustPlugin>>();
		try
		{
			ModLoader.Packages.GetAllHookables((List<RustPlugin>)(object)val);
			foreach (RustPlugin item2 in (List<RustPlugin>)(object)val)
			{
				Plugin[] requires = item2.Requires;
				if (requires == null || requires.Length <= 0)
				{
					ModLoader.UninitializePlugin(item2);
				}
			}
			return null;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	internal static object IOnEntitySaved(BaseNetworkable baseNetworkable, SaveInfo saveInfo)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (!Community.IsServerInitialized || saveInfo.forConnection == null || InternalHooks.OnEntitySaved == 0)
		{
			return null;
		}
		HookCaller.CallStaticHook(825712380u, baseNetworkable, saveInfo);
		return null;
	}

	internal static object IOnLoseCondition(Item item, float amount)
	{
		object[] array = HookCaller.Caller.AllocateBuffer(2);
		array[0] = item;
		array[1] = amount;
		HookCaller.CallStaticHook(2025192851u, array);
		amount = (float)array[1];
		HookCaller.Caller.ReturnBuffer(array);
		float condition = item.condition;
		item.condition -= amount;
		if (item.condition <= 0f && item.condition < condition)
		{
			item.OnBroken();
		}
		return Cache.True;
	}

	internal static object IOnNpcTarget(SenseComponent sense, BaseEntity target)
	{
		if (!Object.op_Implicit((Object)(object)sense) || !Object.op_Implicit((Object)(object)target))
		{
			return null;
		}
		BaseEntity baseEntity = ((EntityComponent<BaseEntity>)(object)sense).baseEntity;
		if ((Object)(object)baseEntity == (Object)null)
		{
			return null;
		}
		if (HookCaller.CallStaticHook(1066895325u, baseEntity, target) != null)
		{
			return Cache.False;
		}
		return null;
	}

	internal static object IOnBasePlayerAttacked(BasePlayer basePlayer, HitInfo hitInfo)
	{
		if (!Community.IsServerInitialized || _isPlayerTakingDamage || (Object)(object)basePlayer == (Object)null || hitInfo == null || ((BaseCombatEntity)basePlayer).IsDead() || basePlayer is NPCPlayer)
		{
			return null;
		}
		if (HookCaller.CallStaticHook(952055589u, basePlayer, hitInfo) != null)
		{
			return Cache.True;
		}
		_isPlayerTakingDamage = true;
		try
		{
			((BaseEntity)basePlayer).OnAttacked(hitInfo);
		}
		finally
		{
			_isPlayerTakingDamage = false;
		}
		return Cache.True;
	}

	internal static object IOnBasePlayerHurt(BasePlayer basePlayer, HitInfo hitInfo)
	{
		if (!_isPlayerTakingDamage)
		{
			return HookCaller.CallStaticHook(952055589u, basePlayer, hitInfo);
		}
		return null;
	}

	internal static object IOnBaseCombatEntityHurt(BaseCombatEntity entity, HitInfo hitInfo)
	{
		if (!(entity is BasePlayer))
		{
			return HookCaller.CallStaticHook(952055589u, entity, hitInfo);
		}
		return null;
	}

	internal static object ICanPickupEntity(BasePlayer basePlayer, DoorCloser entity)
	{
		if (HookCaller.CallStaticHook(861710679u, basePlayer, entity) is bool flag)
		{
			return flag;
		}
		return null;
	}

	private void OnPlayerSetInfo(Connection connection, string key, string val)
	{
		if (key == "global.language")
		{
			lang.SetLanguage(val, connection.userid.ToString());
			MonoBehaviour player = connection.player;
			BasePlayer val2 = (BasePlayer)(object)((player is BasePlayer) ? player : null);
			if (val2 != null)
			{
				HookCaller.CallStaticHook(1945313578u, val2, val);
				HookCaller.CallStaticHook(1945313578u, val2.AsIPlayer(), val);
			}
		}
	}

	private void OnPlayerChat(BasePlayer player, string message, ChatChannel channel)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		WebControlPanel.OnPlayerChat(player, message, channel);
	}

	private void OnPluginLoaded(RustPlugin plugin)
	{
		WebControlPanel.SendPluginsToAllConnections();
	}

	private void OnPluginUnloaded(RustPlugin plugin)
	{
		WebControlPanel.SendPluginsToAllConnections();
	}

	private void OnServerUserSet(ulong steamId, UserGroup group, string playerName, string reason, long expiry)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		if (Community.IsServerInitialized && (int)group == 3)
		{
			string arg = steamId.ToString();
			RustPlayer rustPlayer = BasePlayer.FindByID(steamId)?.AsIPlayer();
			HookCaller.CallStaticHook(140408349u, playerName, steamId, (rustPlayer == null) ? "0" : rustPlayer.Address, reason, expiry);
			HookCaller.CallStaticHook(3042565959u, playerName, arg, (rustPlayer == null) ? "0" : rustPlayer.Address, reason, expiry);
		}
	}

	private void OnServerUserRemove(ulong steamId)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		if (Community.IsServerInitialized && ServerUsers.users.ContainsKey(steamId) && (int)ServerUsers.users[steamId].group == 3)
		{
			string arg = steamId.ToString();
			RustPlayer rustPlayer = BasePlayer.FindByID(steamId)?.AsIPlayer();
			HookCaller.CallStaticHook(1455743240u, (rustPlayer == null || string.IsNullOrEmpty(rustPlayer.Name)) ? "Unnamed" : rustPlayer.Name, steamId, (rustPlayer == null || string.IsNullOrEmpty(rustPlayer.Address)) ? "0" : rustPlayer.Address);
			HookCaller.CallStaticHook(339730350u, (rustPlayer == null || string.IsNullOrEmpty(rustPlayer.Name)) ? "Unnamed" : rustPlayer.Name, arg, (rustPlayer == null || string.IsNullOrEmpty(rustPlayer.Address)) ? "0" : rustPlayer.Address);
		}
	}

	private void OnSaveLoad()
	{
		StoredModifiers.Load();
	}

	private static object IOnCupboardAuthorize(ulong userID, BasePlayer player, BuildingPrivlidge privlidge)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		if (userID == EncryptedValue<ulong>.op_Implicit(player.userID))
		{
			if (HookCaller.CallStaticHook(1460091328u, privlidge, player) != null)
			{
				return true;
			}
		}
		else if (HookCaller.CallStaticHook(2217887722u, privlidge, userID, player) != null)
		{
			return true;
		}
		return null;
	}

	public static void ProcessableFilesLookup()
	{
		ProcessableFiles.Clear();
		Config config = Community.Runtime.Config;
		string[] filesWithExtension = OsEx.Folder.GetFilesWithExtension(Defines.GetScriptsFolder(), "cs", config.Watchers.ScriptWatcherOption);
		foreach (string path in filesWithExtension)
		{
			if (!IsBlacklisted(path))
			{
				ProcessableFile item = new ProcessableFile
				{
					Id = Path.GetFileNameWithoutExtension(path),
					Path = path,
					Type = ProcessableFile.Types.Script
				};
				ProcessableFiles.Add(item);
			}
		}
		string[] filesWithExtension2 = OsEx.Folder.GetFilesWithExtension(Defines.GetScriptsFolder(), "cszip", config.Watchers.ScriptWatcherOption);
		foreach (string path2 in filesWithExtension2)
		{
			if (!IsBlacklisted(path2))
			{
				ProcessableFile item2 = new ProcessableFile
				{
					Id = Path.GetFileNameWithoutExtension(path2),
					Path = path2,
					Type = ProcessableFile.Types.CSZIP
				};
				ProcessableFiles.Add(item2);
			}
		}
		static bool IsBlacklisted(string path3)
		{
			if (!Community.Runtime.ScriptProcessor.IsBlacklisted(path3))
			{
				return Community.Runtime.ZipScriptProcessor.IsBlacklisted(path3);
			}
			return true;
		}
	}

	public static ProcessableFile GetPluginFile(string shortName)
	{
		ProcessableFilesLookup();
		foreach (ProcessableFile processableFile in ProcessableFiles)
		{
			if (processableFile.Id.Equals(shortName, StringComparison.InvariantCultureIgnoreCase))
			{
				return processableFile;
			}
		}
		return default(ProcessableFile);
	}

	public override bool IInit()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		_defaultLogTrace = Application.GetStackTraceLogType((LogType)3);
		_defaultWarningTrace = Application.GetStackTraceLogType((LogType)2);
		_defaultErrorTrace = Application.GetStackTraceLogType((LogType)0);
		_defaultAssertTrace = Application.GetStackTraceLogType((LogType)1);
		_defaultExceptionTrace = Application.GetStackTraceLogType((LogType)4);
		ApplyStacktrace();
		base.HookableType = GetType();
		Hooks = new List<uint>();
		MethodInfo[] methods = base.HookableType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			if (Community.Runtime.HookManager.IsHook(methodInfo.Name))
			{
				Community.Runtime.HookManager.Subscribe(methodInfo.Name, base.Name);
				uint orAdd = HookStringPool.GetOrAdd(methodInfo.Name);
				if (!Hooks.Contains(orAdd))
				{
					Hooks.Add(orAdd);
				}
			}
		}
		if (!base.IInit())
		{
			return false;
		}
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				permission.RefreshUser(current);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		timer.Every(5f, delegate
		{
			if (Community.Runtime != null && Logger.CoreLog != null && Logger.CoreLog.HasInit && Logger.CoreLog.PendingCount != 0 && Community.Runtime.Config.Logging.LogFileMode == 1)
			{
				Logger.CoreLog.Flush();
			}
		});
		cmd.AddConsoleCommand("help", this, "Help", "HELP!", null, null, null, 2);
		cmd.AddConsoleCommand("harmony.mods", this, "HarmonyMods", "Prints a full list of all active HarmonyMods processed by Rust.", null, null, null, 2);
		cmd.AddConsoleCommand("sayas", this, "SayAs", "Sends a message in chat. It's basically `global.say` but customizable.", null, null, null, 2);
		return true;
	}

	private void OnServerInitialized()
	{
		Community.Runtime.ModuleProcessor.OnServerInit();
		CommandLine.ExecuteCommands("+carbon.onserverinit", "OnServerInitialized");
		string file = Path.Combine(Server.GetServerFolder("cfg"), "server.cfg");
		string[] array = (OsEx.File.Exists(file) ? OsEx.File.ReadTextLines(file) : null);
		if (array != null)
		{
			CommandLine.ExecuteCommands("+carbon.onserverinit", "cfg/server.cfg", array);
			Array.Clear(array, 0, array.Length);
		}
		foreach (BasePlayer allPlayer in BasePlayer.allPlayerList)
		{
			try
			{
				if (!((BaseEntity)allPlayer).IsNpc)
				{
					allPlayer.AsIPlayer();
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Failed getting IPlayer object for " + allPlayer.displayName + "[" + allPlayer.UserIDString + "]", ex);
			}
		}
		WebControlPanel.ServerInit();
	}

	private void OnServerSave()
	{
		Interface.Oxide.Permission.SaveData();
		Community.Runtime.ModuleProcessor.OnServerSave();
		Community.Runtime.Events.Trigger(CarbonEvent.OnServerSave, EventArgs.Empty);
		API.Abstracts.CarbonAuto.Singleton?.Save();
	}

	public static void ApplyStacktrace()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Application.SetStackTraceLogType((LogType)3, _defaultLogTrace);
		Application.SetStackTraceLogType((LogType)2, _defaultWarningTrace);
		Application.SetStackTraceLogType((LogType)0, _defaultErrorTrace);
		Application.SetStackTraceLogType((LogType)1, _defaultAssertTrace);
		Application.SetStackTraceLogType((LogType)4, _defaultExceptionTrace);
	}

	protected override void LoadDefaultMessages()
	{
		lang.RegisterMessages(Localisation.Phrases, this);
	}

	public override object InternalCallHook(uint hook, object[] args)
	{
		//IL_1dea: Unknown result type (might be due to invalid IL or missing references)
		//IL_137c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dbe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cb6: Unknown result type (might be due to invalid IL or missing references)
		//IL_12f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_11a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b14: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f76: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e2c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a4e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a90: Unknown result type (might be due to invalid IL or missing references)
		//IL_107b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2522: Unknown result type (might be due to invalid IL or missing references)
		//IL_13be: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ef2: Unknown result type (might be due to invalid IL or missing references)
		//IL_19ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_095c: Unknown result type (might be due to invalid IL or missing references)
		//IL_133a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d3a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1103: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ad2: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1eb0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fb8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1988: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f34: Unknown result type (might be due to invalid IL or missing references)
		//IL_099e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f4a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b98: Unknown result type (might be due to invalid IL or missing references)
		//IL_1904: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c74: Unknown result type (might be due to invalid IL or missing references)
		//IL_091a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ec6: Unknown result type (might be due to invalid IL or missing references)
		//IL_203c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1400: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bf0: Unknown result type (might be due to invalid IL or missing references)
		//IL_22d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_12b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_2186: Unknown result type (might be due to invalid IL or missing references)
		//IL_241a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bda: Unknown result type (might be due to invalid IL or missing references)
		//IL_1da8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_15cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0847: Unknown result type (might be due to invalid IL or missing references)
		//IL_228e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0889: Unknown result type (might be due to invalid IL or missing references)
		//IL_245c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e84: Unknown result type (might be due to invalid IL or missing references)
		//IL_11f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c32: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ca0: Unknown result type (might be due to invalid IL or missing references)
		//IL_24e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_2144: Unknown result type (might be due to invalid IL or missing references)
		//IL_2312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a64: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa6: Unknown result type (might be due to invalid IL or missing references)
		//IL_14e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_249e: Unknown result type (might be due to invalid IL or missing references)
		//IL_207e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cf8: Unknown result type (might be due to invalid IL or missing references)
		//IL_2396: Unknown result type (might be due to invalid IL or missing references)
		//IL_1274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0805: Unknown result type (might be due to invalid IL or missing references)
		//IL_224c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2102: Unknown result type (might be due to invalid IL or missing references)
		//IL_1144: Unknown result type (might be due to invalid IL or missing references)
		//IL_21c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1946: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a22: Unknown result type (might be due to invalid IL or missing references)
		//IL_2561: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d7c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1650: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ce2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bae: Unknown result type (might be due to invalid IL or missing references)
		//IL_1442: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b56: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ae8: Unknown result type (might be due to invalid IL or missing references)
		//IL_20c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_2354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b6c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c5e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d66: Unknown result type (might be due to invalid IL or missing references)
		//IL_155b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d24: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ffd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f08: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c1c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e00: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ffa: Unknown result type (might be due to invalid IL or missing references)
		//IL_160e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e6e: Unknown result type (might be due to invalid IL or missing references)
		//IL_23d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_220a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e42: Unknown result type (might be due to invalid IL or missing references)
		//IL_1820: Unknown result type (might be due to invalid IL or missing references)
		//IL_1825: Unknown result type (might be due to invalid IL or missing references)
		//IL_149d: Unknown result type (might be due to invalid IL or missing references)
		//IL_14a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_14b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_18cb: Unknown result type (might be due to invalid IL or missing references)
		int? num = args?.Length;
		object obj = ((num > 0) ? args[0] : null);
		object obj2 = ((num > 1) ? args[1] : null);
		object obj3 = ((num > 2) ? args[2] : null);
		object obj4 = ((num > 3) ? args[3] : null);
		object obj5 = ((num > 4) ? args[4] : null);
		try
		{
			switch (hook)
			{
			case 3617829410u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag57 = flag;
				Arg arg40 = ((!flag57) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag57)
				{
					AddConditional(arg40);
					return null;
				}
				break;
			}
			case 1461696666u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag25 = flag;
				Arg arg18 = ((!flag25) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag25)
				{
					Aliases(arg18);
					return null;
				}
				break;
			}
			case 1310794640u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag68 = flag;
				Arg arg49 = ((!flag68) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag68)
				{
					AssignAlias(arg49);
					return null;
				}
				break;
			}
			case 1569187096u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag54 = flag;
				Arg arg37 = ((!flag54) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag54)
				{
					BuildCall(arg37);
					return null;
				}
				break;
			}
			case 307092880u:
				return CanUnlockTechTreeNode();
			case 2522567266u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag104 = flag;
				Arg arg74 = ((!flag104) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag104)
				{
					CarbonLoadConfig(arg74);
					return null;
				}
				break;
			}
			case 8097725u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag7 = flag;
				Arg arg6 = ((!flag7) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag7)
				{
					CarbonSaveConfig(arg6);
					return null;
				}
				break;
			}
			case 3026698837u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag43 = flag;
				Arg arg30 = ((!flag43) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag43)
				{
					ChangeVersion(arg30);
					return null;
				}
				break;
			}
			case 3716440972u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag67 = flag;
				Arg arg48 = ((!flag67) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag67)
				{
					Cleanup(arg48);
					return null;
				}
				break;
			}
			case 2486811342u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag34 = flag;
				Arg arg26 = ((!flag34) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag34)
				{
					ClearMarkers(arg26);
					return null;
				}
				break;
			}
			case 212981081u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag85 = flag;
				Arg arg61 = ((!flag85) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag85)
				{
					Commit(arg61);
					return null;
				}
				break;
			}
			case 121761328u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag66 = flag;
				Arg arg47 = ((!flag66) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag66)
				{
					Conditionals(arg47);
					return null;
				}
				break;
			}
			case 1813333766u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag86 = flag;
				Arg arg62 = ((!flag86) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag86)
				{
					CreatePlugin(arg62);
					return null;
				}
				break;
			}
			case 2563024626u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag18 = flag;
				Arg arg17 = ((!flag18) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag18)
				{
					Delete(arg17);
					return null;
				}
				break;
			}
			case 1566035725u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag116 = flag;
				Arg arg83 = ((!flag116) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag116)
				{
					DeleteExt(arg83);
					return null;
				}
				break;
			}
			case 2179227208u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag69 = flag;
				Arg arg50 = ((!flag69) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag69)
				{
					DevDumpSnapshot(arg50);
					return null;
				}
				break;
			}
			case 2215751651u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag112 = flag;
				Arg arg80 = ((!flag112) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag112)
				{
					EditConfig(arg80);
					return null;
				}
				break;
			}
			case 1012871006u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag32 = flag;
				Arg arg24 = ((!flag32) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag32)
				{
					Extensions(arg24);
					return null;
				}
				break;
			}
			case 2557278796u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag9 = flag;
				Arg arg8 = ((!flag9) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag9)
				{
					Find(arg8);
					return null;
				}
				break;
			}
			case 2822243214u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag49 = flag;
				Arg arg35 = ((!flag49) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag49)
				{
					FindChat(arg35);
					return null;
				}
				break;
			}
			case 3993433097u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag79 = flag;
				Arg arg55 = ((!flag79) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag79)
				{
					GetVault(arg55);
					return null;
				}
				break;
			}
			case 4128227484u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag33 = flag;
				Arg arg25 = ((!flag33) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag33)
				{
					GetWebControlPanelClients(arg25);
					return null;
				}
				break;
			}
			case 2362460257u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag73 = flag;
				Arg arg52 = ((!flag73) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag73)
				{
					GoCommunity(arg52);
					return null;
				}
				break;
			}
			case 3167076070u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag10 = flag;
				Arg arg9 = ((!flag10) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag10)
				{
					Grant(arg9);
					return null;
				}
				break;
			}
			case 879858435u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag89 = flag;
				Arg arg64 = ((!flag89) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag89)
				{
					Group(arg64);
					return null;
				}
				break;
			}
			case 126019937u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag98 = flag;
				Arg arg71 = ((!flag98) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag98)
				{
					HarmonyMods(arg71);
					return null;
				}
				break;
			}
			case 1224025706u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag12 = flag;
				Arg arg11 = ((!flag12) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag12)
				{
					Help(arg11);
					return null;
				}
				break;
			}
			case 2465598932u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag90 = flag;
				Arg arg65 = ((!flag90) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag90)
				{
					HookInfo(arg65);
					return null;
				}
				break;
			}
			case 1110553926u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag62 = flag;
				Arg args2 = ((!flag62) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag62)
				{
					HooksCall(args2);
					return null;
				}
				break;
			}
			case 2966092386u:
			{
				bool flag = ((obj is ItemBlueprint || obj == null) ? true : false);
				bool flag50 = flag;
				ItemBlueprint bp = ((!flag50) ? ((ItemBlueprint)null) : ((ItemBlueprint)(obj ?? null)));
				flag = ((obj2 is float || obj2 == null) ? true : false);
				bool flag51 = flag;
				float workbenchLevel = (flag51 ? ((float)(obj2 ?? ((object)0f))) : 0f);
				flag = ((obj3 is bool || obj3 == null) ? true : false);
				bool flag52 = flag;
				bool isInTutorial = flag52 && (bool)(obj3 ?? ((object)false));
				if (flag50 && flag51 && flag52)
				{
					return ICraftDurationMultiplier(bp, workbenchLevel, isInTutorial);
				}
				break;
			}
			case 1211592203u:
			{
				bool flag = ((obj is MixingTable || obj == null) ? true : false);
				bool flag95 = flag;
				MixingTable table = ((!flag95) ? ((MixingTable)null) : ((MixingTable)(obj ?? null)));
				flag = ((obj2 is float || obj2 == null) ? true : false);
				bool flag96 = flag;
				float originalValue = (flag96 ? ((float)(obj2 ?? ((object)0f))) : 0f);
				if (flag95 && flag96)
				{
					return IMixingSpeedMultiplier(table, originalValue);
				}
				break;
			}
			case 1112160822u:
			{
				bool flag = ((obj is ExcavatorArm || obj == null) ? true : false);
				bool flag44 = flag;
				ExcavatorArm arm = ((!flag44) ? ((ExcavatorArm)null) : ((ExcavatorArm)(obj ?? null)));
				if (flag44)
				{
					IOnExcavatorInit(arm);
					return null;
				}
				break;
			}
			case 4155259925u:
			{
				bool flag = ((obj is bool || obj == null) ? true : false);
				bool flag114 = flag;
				bool inited = flag114 && (bool)(obj ?? ((object)false));
				if (flag114)
				{
					return IOnServerInitialized(inited);
				}
				break;
			}
			case 3923985155u:
			{
				bool flag = ((obj is BaseOven || obj == null) ? true : false);
				bool flag88 = flag;
				BaseOven oven = ((!flag88) ? ((BaseOven)null) : ((BaseOven)(obj ?? null)));
				if (flag88)
				{
					return IOvenSmeltSpeedMultiplier(oven);
				}
				break;
			}
			case 3134346010u:
			{
				bool flag = ((obj is Recycler || obj == null) ? true : false);
				bool flag61 = flag;
				Recycler recycler = ((!flag61) ? ((Recycler)null) : ((Recycler)(obj ?? null)));
				if (flag61)
				{
					return IRecyclerThinkSpeed(recycler);
				}
				break;
			}
			case 1870500310u:
				IResearchDuration();
				return null;
			case 4192766051u:
				return IVendingBuyDuration();
			case 2370971930u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag110 = flag;
				Arg arg78 = ((!flag110) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag110)
				{
					InstallPlugin(arg78);
					return null;
				}
				break;
			}
			case 313256762u:
				LoadDefaultMessages();
				return null;
			case 1175002629u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag111 = flag;
				Arg arg79 = ((!flag111) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag111)
				{
					LoadModule(arg79);
					return null;
				}
				break;
			}
			case 2699051938u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag81 = flag;
				Arg arg57 = ((!flag81) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag81)
				{
					LoadPlugin(arg57);
					return null;
				}
				break;
			}
			case 3768797615u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag48 = flag;
				Arg arg34 = ((!flag48) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag48)
				{
					LoadWebControlPanelConfig(arg34);
					return null;
				}
				break;
			}
			case 2009737156u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag5 = flag;
				Arg arg4 = ((!flag5) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag5)
				{
					MigrateToProto(arg4);
					return null;
				}
				break;
			}
			case 2539568543u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag102 = flag;
				Arg arg73 = ((!flag102) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag102)
				{
					MigrateToSql(arg73);
					return null;
				}
				break;
			}
			case 4209386718u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag101 = flag;
				Arg arg72 = ((!flag101) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag101)
				{
					ModdedRustConVars(arg72);
					return null;
				}
				break;
			}
			case 3694325137u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag63 = flag;
				Arg arg44 = ((!flag63) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag63)
				{
					ModuleInfo(arg44);
					return null;
				}
				break;
			}
			case 346822591u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag14 = flag;
				Arg arg13 = ((!flag14) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag14)
				{
					Modules(arg13);
					return null;
				}
				break;
			}
			case 2263673102u:
			{
				bool flag = ((obj is Connection || obj == null) ? true : false);
				bool flag105 = flag;
				Connection connection2 = ((!flag105) ? ((Connection)null) : ((Connection)(obj ?? null)));
				if (flag105)
				{
					OnClientAuth(connection2);
					return null;
				}
				break;
			}
			case 2032160890u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag39 = flag;
				BasePlayer player = ((!flag39) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag40 = flag;
				string message = (flag40 ? ((string)(obj2 ?? null)) : null);
				flag = ((obj3 is ChatChannel || obj3 == null) ? true : false);
				bool flag41 = flag;
				ChatChannel channel = (ChatChannel)(flag41 ? ((int)(ChatChannel)(obj3 ?? ((object)(ChatChannel)0))) : 0);
				if (flag39 && flag40 && flag41)
				{
					OnPlayerChat(player, message, channel);
					return null;
				}
				break;
			}
			case 72085565u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag99 = flag;
				BasePlayer player2 = ((!flag99) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag100 = flag;
				string reason3 = (flag100 ? ((string)(obj2 ?? null)) : null);
				if (flag99 && flag100)
				{
					OnPlayerDisconnected(player2, reason3);
					return null;
				}
				break;
			}
			case 1321158727u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag70 = flag;
				BasePlayer basePlayer3 = ((!flag70) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag71 = flag;
				string reason2 = (flag71 ? ((string)(obj2 ?? null)) : null);
				if (flag70 && flag71)
				{
					OnPlayerKicked(basePlayer3, reason2);
					return null;
				}
				break;
			}
			case 1546340674u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag38 = flag;
				BasePlayer basePlayer2 = ((!flag38) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				if (flag38)
				{
					return OnPlayerRespawn(basePlayer2);
				}
				break;
			}
			case 458523914u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag24 = flag;
				BasePlayer basePlayer = ((!flag24) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				if (flag24)
				{
					OnPlayerRespawned(basePlayer);
					return null;
				}
				break;
			}
			case 2283023029u:
			{
				bool flag = ((obj is Connection || obj == null) ? true : false);
				bool flag75 = flag;
				Connection connection = ((!flag75) ? ((Connection)null) : ((Connection)(obj ?? null)));
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag76 = flag;
				string key = (flag76 ? ((string)(obj2 ?? null)) : null);
				flag = ((obj3 is string || obj3 == null) ? true : false);
				bool flag77 = flag;
				string val2 = (flag77 ? ((string)(obj3 ?? null)) : null);
				if (flag75 && flag76 && flag77)
				{
					OnPlayerSetInfo(connection, key, val2);
					return null;
				}
				break;
			}
			case 3051933177u:
			{
				bool flag = ((obj is RustPlugin || obj == null) ? true : false);
				bool flag42 = flag;
				RustPlugin plugin2 = (flag42 ? ((RustPlugin)(obj ?? null)) : null);
				if (flag42)
				{
					OnPluginLoaded(plugin2);
					return null;
				}
				break;
			}
			case 1250294368u:
			{
				bool flag = ((obj is RustPlugin || obj == null) ? true : false);
				bool flag28 = flag;
				RustPlugin plugin = (flag28 ? ((RustPlugin)(obj ?? null)) : null);
				if (flag28)
				{
					OnPluginUnloaded(plugin);
					return null;
				}
				break;
			}
			case 106238856u:
				OnSaveLoad();
				return null;
			case 352240293u:
				OnServerInitialized();
				return null;
			case 2396958305u:
				OnServerSave();
				return null;
			case 2043356880u:
			{
				bool flag = ((obj is ulong || obj == null) ? true : false);
				bool flag106 = flag;
				ulong steamId2 = (flag106 ? ((ulong)(obj ?? ((object)0uL))) : 0);
				if (flag106)
				{
					OnServerUserRemove(steamId2);
					return null;
				}
				break;
			}
			case 931424179u:
			{
				bool flag = ((obj is ulong || obj == null) ? true : false);
				bool flag19 = flag;
				ulong steamId = (flag19 ? ((ulong)(obj ?? ((object)0uL))) : 0);
				flag = ((obj2 is UserGroup || obj2 == null) ? true : false);
				bool flag20 = flag;
				UserGroup val = (UserGroup)(flag20 ? ((int)(UserGroup)(obj2 ?? ((object)(UserGroup)0))) : 0);
				flag = ((obj3 is string || obj3 == null) ? true : false);
				bool flag21 = flag;
				string playerName = (flag21 ? ((string)(obj3 ?? null)) : null);
				flag = ((obj4 is string || obj4 == null) ? true : false);
				bool flag22 = flag;
				string reason = (flag22 ? ((string)(obj4 ?? null)) : null);
				flag = ((obj5 is long || obj5 == null) ? true : false);
				bool flag23 = flag;
				long expiry = (flag23 ? ((long)(obj5 ?? ((object)0L))) : 0);
				if (flag19 && flag20 && flag21 && flag22 && flag23)
				{
					OnServerUserSet(steamId, val, playerName, reason, expiry);
					return null;
				}
				break;
			}
			case 2601596680u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag3 = flag;
				Arg arg2 = ((!flag3) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag3)
				{
					OpenConfigs(arg2);
					return null;
				}
				break;
			}
			case 2529893848u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag108 = flag;
				Arg arg76 = ((!flag108) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag108)
				{
					OpenData(arg76);
					return null;
				}
				break;
			}
			case 3168678498u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag82 = flag;
				Arg arg58 = ((!flag82) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag82)
				{
					OpenExtensions(arg58);
					return null;
				}
				break;
			}
			case 113708137u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag59 = flag;
				Arg arg42 = ((!flag59) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag59)
				{
					OpenLang(arg42);
					return null;
				}
				break;
			}
			case 2745846876u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag53 = flag;
				Arg arg36 = ((!flag53) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag53)
				{
					OpenLogs(arg36);
					return null;
				}
				break;
			}
			case 1539167114u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag17 = flag;
				Arg arg16 = ((!flag17) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag17)
				{
					OpenModules(arg16);
					return null;
				}
				break;
			}
			case 1290689173u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag4 = flag;
				Arg arg3 = ((!flag4) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag4)
				{
					OpenPlugin(arg3);
					return null;
				}
				break;
			}
			case 3801308235u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag115 = flag;
				Arg arg82 = ((!flag115) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag115)
				{
					OpenPlugins(arg82);
					return null;
				}
				break;
			}
			case 2180218250u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag93 = flag;
				Arg arg68 = ((!flag93) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag93)
				{
					OpenRoot(arg68);
					return null;
				}
				break;
			}
			case 1853185048u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag74 = flag;
				Arg arg53 = ((!flag74) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag74)
				{
					PluginCmds(arg53);
					return null;
				}
				break;
			}
			case 2730263207u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag56 = flag;
				Arg arg39 = ((!flag56) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag56)
				{
					PluginInfo(arg39);
					return null;
				}
				break;
			}
			case 1778989243u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag36 = flag;
				Arg arg28 = ((!flag36) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag36)
				{
					Plugins(arg28);
					return null;
				}
				break;
			}
			case 958120911u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag29 = flag;
				Arg arg21 = ((!flag29) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag29)
				{
					PrintHookPool(arg21);
					return null;
				}
				break;
			}
			case 1503455692u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag11 = flag;
				Arg arg10 = ((!flag11) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag11)
				{
					Profile(arg10);
					return null;
				}
				break;
			}
			case 869177234u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag109 = flag;
				Arg arg77 = ((!flag109) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag109)
				{
					ProfileAbort(arg77);
					return null;
				}
				break;
			}
			case 2235018487u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag92 = flag;
				Arg arg67 = ((!flag92) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag92)
				{
					ProfilerPrint(arg67);
					return null;
				}
				break;
			}
			case 1282370872u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag78 = flag;
				Arg arg54 = ((!flag78) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag78)
				{
					ProfilerRemovePlugin(arg54);
					return null;
				}
				break;
			}
			case 1400659095u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag64 = flag;
				Arg arg45 = ((!flag64) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag64)
				{
					ProfilerTrackPlugin(arg45);
					return null;
				}
				break;
			}
			case 1603732279u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag46 = flag;
				Arg arg32 = ((!flag46) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag46)
				{
					ProfilerTracked(arg32);
					return null;
				}
				break;
			}
			case 4118252168u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag31 = flag;
				Arg arg23 = ((!flag31) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag31)
				{
					Protocol(arg23);
					return null;
				}
				break;
			}
			case 1669471309u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag8 = flag;
				Arg arg7 = ((!flag8) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag8)
				{
					Reload(arg7);
					return null;
				}
				break;
			}
			case 407700441u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag113 = flag;
				Arg arg81 = ((!flag113) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag113)
				{
					ReloadConfig(arg81);
					return null;
				}
				break;
			}
			case 3607291287u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag97 = flag;
				Arg arg70 = ((!flag97) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag97)
				{
					ReloadModule(arg70);
					return null;
				}
				break;
			}
			case 165571558u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag87 = flag;
				Arg arg63 = ((!flag87) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag87)
				{
					RemoveConditional(arg63);
					return null;
				}
				break;
			}
			case 3053121748u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag83 = flag;
				Arg arg59 = ((!flag83) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag83)
				{
					ResetHooks(arg59);
					return null;
				}
				break;
			}
			case 1983852833u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag55 = flag;
				Arg arg38 = ((!flag55) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag55)
				{
					Revoke(arg38);
					return null;
				}
				break;
			}
			case 3408782460u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag45 = flag;
				Arg arg31 = ((!flag45) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag45)
				{
					SaveModule(arg31);
					return null;
				}
				break;
			}
			case 523784464u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag30 = flag;
				Arg arg22 = ((!flag30) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag30)
				{
					SaveWebControlPanelConfig(arg22);
					return null;
				}
				break;
			}
			case 2303807553u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag16 = flag;
				Arg arg15 = ((!flag16) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag16)
				{
					SayAs(arg15);
					return null;
				}
				break;
			}
			case 4206019811u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag117 = flag;
				Arg module = ((!flag117) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag117)
				{
					SetModule(module);
					return null;
				}
				break;
			}
			case 1624709752u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag103 = flag;
				Arg webControlPanelPort = ((!flag103) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag103)
				{
					SetWebControlPanelPort(webControlPanelPort);
					return null;
				}
				break;
			}
			case 3296300873u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag91 = flag;
				Arg arg66 = ((!flag91) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag91)
				{
					Show(arg66);
					return null;
				}
				break;
			}
			case 414928410u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag80 = flag;
				Arg arg56 = ((!flag80) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag80)
				{
					Shutdown(arg56);
					return null;
				}
				break;
			}
			case 1867912083u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag65 = flag;
				Arg arg46 = ((!flag65) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag65)
				{
					Skin(arg46);
					return null;
				}
				break;
			}
			case 2977018099u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag58 = flag;
				Arg arg41 = ((!flag58) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag58)
				{
					TryToggleWebControlPanelServer(arg41);
					return null;
				}
				break;
			}
			case 275530773u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag37 = flag;
				Arg arg29 = ((!flag37) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag37)
				{
					UnassignAlias(arg29);
					return null;
				}
				break;
			}
			case 3547710285u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag27 = flag;
				Arg arg20 = ((!flag27) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag27)
				{
					UninstallPlugin(arg20);
					return null;
				}
				break;
			}
			case 1342457948u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag15 = flag;
				Arg arg14 = ((!flag15) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag15)
				{
					UnloadPlugin(arg14);
					return null;
				}
				break;
			}
			case 2108743044u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag6 = flag;
				Arg arg5 = ((!flag6) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag6)
				{
					UserGroup(arg5);
					return null;
				}
				break;
			}
			case 297788813u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag107 = flag;
				Arg arg75 = ((!flag107) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag107)
				{
					VaultAdd(arg75);
					return null;
				}
				break;
			}
			case 1579191406u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag94 = flag;
				Arg arg69 = ((!flag94) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag94)
				{
					VaultRemove(arg69);
					return null;
				}
				break;
			}
			case 3912190147u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag84 = flag;
				Arg arg60 = ((!flag84) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag84)
				{
					VersionCall(arg60);
					return null;
				}
				break;
			}
			case 340806103u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag72 = flag;
				Arg arg51 = ((!flag72) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag72)
				{
					WhyModded(arg51);
					return null;
				}
				break;
			}
			case 1847800369u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag60 = flag;
				Arg arg43 = ((!flag60) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag60)
				{
					WipeUI(arg43);
					return null;
				}
				break;
			}
			case 1250811332u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag47 = flag;
				Arg arg33 = ((!flag47) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag47)
				{
					test_beds(arg33);
					return null;
				}
				break;
			}
			case 4236641972u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag35 = flag;
				Arg arg27 = ((!flag35) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag35)
				{
					test_clear(arg27);
					return null;
				}
				break;
			}
			case 473670306u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag26 = flag;
				Arg arg19 = ((!flag26) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag26)
				{
					test_collect(arg19);
					return null;
				}
				break;
			}
			case 683585953u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag13 = flag;
				Arg arg12 = ((!flag13) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag13)
				{
					test_plugin(arg12);
					return null;
				}
				break;
			}
			case 2426236348u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag2 = flag;
				Arg arg = ((!flag2) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag2)
				{
					test_run(arg);
					return null;
				}
				break;
			}
			}
		}
		catch (Exception ex)
		{
			Logger.Error(string.Format("Failed to call internal hook '{0}' on plugin '{1} v{2}' [{3}]", new object[4]
			{
				HookStringPool.GetOrAdd(hook),
				base.Name,
				base.Version,
				hook
			}), ex);
			OnException(hook);
		}
		return null;
	}
}

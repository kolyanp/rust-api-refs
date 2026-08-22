using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Carbon.Base;
using Carbon.Components;
using Carbon.Extensions;
using Carbon.Pooling;
using ConVar;
using Cronos;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Plugins;
using UnityEngine;

namespace Carbon.Modules;

public class AutoWipeModule : CarbonModule<AutoWipeConfig, AutoWipeData>
{
	public class Wipe
	{
		public string WipeName;

		public string[] Commands;

		public string MapBrowserName;

		public string MapUrl;

		public int MapSize;

		public int ServerSeed;

		public string Cron;

		public bool Temp;

		[JsonProperty("Type (0=fullwipe 1=mapwipe)")]
		public WipeTypes Type;

		public void CopyTo(Wipe other)
		{
			other.WipeName = WipeName;
			other.Commands = Commands.ToArray();
			other.MapBrowserName = MapBrowserName;
			other.MapUrl = MapUrl;
			other.MapSize = MapSize;
			other.ServerSeed = ServerSeed;
			other.Cron = Cron;
			other.Temp = Temp;
			other.Type = Type;
		}

		public unsafe void InitWorld(List<WipeMap> maps, long lastWipe)
		{
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			Community.Runtime.Core.CustomMapName = (string.IsNullOrEmpty(MapBrowserName) ? "-1" : MapBrowserName);
			if (MapUrl == "POOL")
			{
				int index = Random.Range(0, maps.Count);
				WipeMap wipeMap = maps[index];
				MapUrl = wipeMap.Url;
				if (wipeMap.Temp)
				{
					maps.RemoveAt(index);
				}
			}
			Singleton.RefreshHostName();
			World.Url = (Server.levelurl = MapUrl);
			if (MapSize != 0)
			{
				World.InitSize(Server.worldsize = MapSize);
			}
			if (ServerSeed == 0)
			{
				ServerSeed = Random.Range(1, int.MaxValue);
			}
			World.InitSeed(Server.seed = ServerSeed);
			StringTable val = default(StringTable);
			((StringTable)(ref val))._002Ector(new string[4] { "wipe_name", "seed", "size", "url" });
			try
			{
				((StringTable)(ref val)).AddRow(new object[4] { WipeName, ServerSeed, MapSize, MapUrl });
				Logger.Warn((object)((StringTable)(ref val)).Write((FormatTypes)0));
			}
			finally
			{
				((IDisposable)(*(StringTable*)(&val))/*cast due to constrained. prefix*/).Dispose();
			}
		}

		public override bool Equals(object other)
		{
			if (other is Wipe wipe)
			{
				return GetHashCode() == wipe.GetHashCode();
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (WipeName, MapBrowserName, MapUrl, MapSize, ServerSeed, Type, Temp, Cron, Commands).GetHashCode();
		}

		public bool ShouldWipe()
		{
			if (string.IsNullOrEmpty(Cron))
			{
				return false;
			}
			DateTime utcNow = DateTime.UtcNow;
			CronExpression val = CronExpression.Parse(Cron);
			DateTime? nextOccurrence = val.GetNextOccurrence(utcNow.AddMinutes(-1.0), TimeZoneInfo.Utc, false);
			if (!nextOccurrence.HasValue)
			{
				return false;
			}
			DateTime dateTime = RoundDownTo10Minutes(utcNow);
			DateTime dateTime2 = RoundDownTo10Minutes(nextOccurrence.Value);
			return dateTime == dateTime2;
			static DateTime RoundDownTo10Minutes(DateTime dt)
			{
				int minute = dt.Minute - dt.Minute % 10;
				return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, minute, 0, dt.Kind);
			}
		}
	}

	public struct WipeConfig
	{
		public string[] PostWipeCommands;

		public string[] PostWipeDeletes;
	}

	public struct WipeMap
	{
		public string Url;

		public bool Temp;
	}

	public enum WipeTypes
	{
		FullWipe,
		MapWipe
	}

	public static AutoWipeModule Singleton;

	private readonly char[] splitter = new char[1] { '|' };

	private readonly float wipeCooldown = 3600f;

	private readonly float wipeTick = 30f;

	private Timer wipeTimer;

	public override string Name => "AutoWipe";

	public override VersionNumber Version
	{
		get
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			return new VersionNumber(2, 0, 0);
		}
	}

	public override Type Type => typeof(AutoWipeModule);

	public override bool EnabledByDefault => false;

	public bool InCooldown()
	{
		return (DateTime.UtcNow - new DateTime(base.DataInstance.LastWipeTime)).TotalSeconds <= (double)wipeCooldown;
	}

	public override void Init()
	{
		base.Init();
		Singleton = this;
	}

	public override void Load()
	{
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		base.Load();
		if (!((BaseModule)this).IsEnabled() || Community.IsServerInitialized)
		{
			return;
		}
		if (InCooldown())
		{
			((CarbonModule<AutoWipeConfig, AutoWipeData>)Singleton).PutsWarn((object)"Initialized world config [WIPE_COOLDOWN]");
			base.DataInstance.Wipe?.InitWorld(base.ConfigInstance.Maps, base.DataInstance.LastWipeTime);
			return;
		}
		if (base.DataInstance.NextWipe == null)
		{
			base.DataInstance.NextWipe = GetUpcomingAvailableWipeImpl();
		}
		if (!string.IsNullOrEmpty(base.ConfigInstance.WipeChatCommand))
		{
			UpdateWipeChatCommand(null, base.ConfigInstance.WipeChatCommand);
		}
		Wipe wipe = base.DataInstance.Wipe;
		Wipe wipe2 = base.DataInstance.NextWipe ?? wipe;
		if (wipe2 != null && !wipe2.Equals(wipe))
		{
			WipeConfig wipeConfig = base.ConfigInstance.GetWipeConfig(wipe2);
			base.DataInstance.LastWipeTime = DateTime.UtcNow.Ticks;
			base.DataInstance.NextWipe = null;
			Server.autoUploadMap = false;
			if (wipe2.Temp)
			{
				base.ConfigInstance.AvailableWipes.Remove(wipe2);
				base.PutsWarn((object)"Removed map from list");
			}
			AutoWipeData dataInstance = base.DataInstance;
			if (dataInstance.Wipe == null)
			{
				dataInstance.Wipe = new Wipe();
			}
			wipe2.CopyTo(base.DataInstance.Wipe);
			base.PutsWarn((object)"New wipe detected!");
			base.DataInstance.Wipe?.InitWorld(base.ConfigInstance.Maps, base.DataInstance.LastWipeTime);
			if (wipeConfig.PostWipeCommands != null)
			{
				for (int i = 0; i < wipeConfig.PostWipeCommands.Length; i++)
				{
					string text = wipeConfig.PostWipeCommands[i];
					if (!string.IsNullOrEmpty(text))
					{
						Option server = Option.Server;
						ConsoleSystem.Run(((Option)(ref server)).Quiet(), text, Array.Empty<object>());
					}
				}
			}
			if (wipeConfig.PostWipeDeletes != null)
			{
				for (int j = 0; j < wipeConfig.PostWipeDeletes.Length; j++)
				{
					string text2 = wipeConfig.PostWipeDeletes[j];
					if (string.IsNullOrEmpty(text2))
					{
						continue;
					}
					if (text2.Contains("*"))
					{
						string text3 = Path.GetDirectoryName(text2);
						string fileName = Path.GetFileName(text2);
						if (string.IsNullOrEmpty(text3))
						{
							text3 = ".";
						}
						if (!Directory.Exists(text3))
						{
							continue;
						}
						try
						{
							string[] files = Directory.GetFiles(text3, fileName);
							foreach (string text4 in files)
							{
								File.Delete(text4);
								base.PutsWarn((object)("Deleting scheduled file '" + text4 + "'"));
							}
						}
						catch (Exception ex)
						{
							base.PutsError((object)("Error deleting files matching pattern '" + text2 + "'"), ex);
						}
					}
					else if (File.Exists(text2))
					{
						File.Delete(text2);
						base.PutsWarn((object)("Deleting scheduled file '" + text2 + "'"));
					}
					else if (Folder.Exists(text2))
					{
						Folder.Delete(text2);
						base.PutsWarn((object)("Deleting scheduled directory '" + text2 + "'"));
					}
				}
			}
			((BaseModule)this).Save();
		}
		else
		{
			((CarbonModule<AutoWipeConfig, AutoWipeData>)Singleton).PutsWarn((object)"Initialized world config");
			base.DataInstance.Wipe?.InitWorld(base.ConfigInstance.Maps, base.DataInstance.LastWipeTime);
		}
	}

	public bool UpdateWipeChatCommand(string old, string current)
	{
		if (old == current)
		{
			return false;
		}
		bool result = false;
		if (!string.IsNullOrEmpty(old))
		{
			((Plugin)Community.Runtime.Core).cmd.RemoveChatCommand(old, (BaseHookable)(object)this);
			result = true;
		}
		if (!string.IsNullOrEmpty(current))
		{
			((Plugin)Community.Runtime.Core).cmd.AddChatCommand(current, (BaseHookable)(object)this, "WipeChat", (string)null, (object)null, (string[])null, (string[])null, -1, 0, false, false, false, false);
			result = true;
		}
		return result;
	}

	private void WipeChat(BasePlayer player, string cmd, string[] args)
	{
		(Wipe, DateTime?) upcomingWipeImpl = GetUpcomingWipeImpl();
		if (upcomingWipeImpl.Item1 == null)
		{
			player.ChatMessage("No available wipe found");
			return;
		}
		double totalSeconds = (upcomingWipeImpl.Item2.GetValueOrDefault() - DateTime.UtcNow).TotalSeconds;
		player.ChatMessage("Next wipe happens in <color=orange>" + TimeEx.Format<double>(totalSeconds, false, false).ToLower() + "</color>");
	}

	public override void OnServerInit(bool initial)
	{
		base.OnServerInit(initial);
		base.OnEnableStatus();
	}

	public override bool PreLoadShouldSave(bool newConfig, bool newData)
	{
		bool result = false;
		if (base.ConfigInstance.Maps == null)
		{
			base.ConfigInstance.Maps = new List<WipeMap>();
			result = true;
		}
		return result;
	}

	public override void OnEnabled(bool initialized)
	{
		base.OnEnabled(initialized);
		if (initialized)
		{
			if (wipeTimer != null)
			{
				wipeTimer.Destroy();
			}
			wipeTimer = ((RustPlugin)Community.Runtime.Core).timer.Every(wipeTick, (Action)WipeTickImpl);
		}
	}

	public override void OnDisabled(bool initialized)
	{
		base.OnDisabled(initialized);
		if (initialized && wipeTimer != null)
		{
			wipeTimer.Destroy();
			wipeTimer = null;
		}
	}

	public override void OnUnload()
	{
		if (wipeTimer != null)
		{
			wipeTimer.Destroy();
			wipeTimer = null;
		}
		base.OnUnload();
	}

	private void RefreshHostName()
	{
		if (base.DataInstance != null)
		{
			DateTime time = new DateTime(base.DataInstance.LastWipeTime);
			if (!string.IsNullOrEmpty(Server.hostname) && HasReplacements(Server.hostname))
			{
				Server.hostname = ProcessString(Server.hostname, time);
				base.PutsWarn((object)"Updated server hostname replacements");
			}
			if (!string.IsNullOrEmpty(Server.description) && HasReplacements(Server.description))
			{
				Server.description = ProcessString(Server.description, time);
				base.PutsWarn((object)"Updated server description replacements");
			}
		}
		static bool HasReplacements(string source)
		{
			if (!source.Contains("[WIPE_DAY]") && !source.Contains("[WIPE_MONTH]") && !source.Contains("[WIPE_YEAR]") && !source.Contains("[WIPE_HOUR]"))
			{
				return source.Contains("[WIPE_MINUTE]");
			}
			return true;
		}
		static string ProcessString(string source, DateTime dateTime)
		{
			return source.Replace("[WIPE_DAY]", $"{dateTime.Day}").Replace("[WIPE_MONTH]", $"{dateTime.Month}").Replace("[WIPE_YEAR]", $"{dateTime.Year}")
				.Replace("[WIPE_HOUR]", $"{dateTime.Hour}")
				.Replace("[WIPE_MINUTE]", $"{dateTime.Minute}");
		}
	}

	private void OnServerInformationUpdated()
	{
		RefreshHostName();
	}

	private void WipeTickImpl()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (!((BaseModule)this).IsEnabled() || InCooldown())
		{
			return;
		}
		base.DataInstance.NextWipe = GetUpcomingAvailableWipeImpl();
		if (base.DataInstance.NextWipe == null)
		{
			return;
		}
		if (base.DataInstance.NextWipe.Commands != null)
		{
			for (int i = 0; i < base.DataInstance.NextWipe.Commands.Length; i++)
			{
				string text = base.DataInstance.NextWipe.Commands[i];
				if (!string.IsNullOrEmpty(text))
				{
					Option server = Option.Server;
					ConsoleSystem.Run(((Option)(ref server)).Quiet(), text, Array.Empty<object>());
				}
			}
		}
		wipeTimer.Destroy();
		((BaseModule)this).Save();
	}

	private Wipe GetUpcomingAvailableWipeImpl()
	{
		for (int i = 0; i < base.ConfigInstance.AvailableWipes.Count; i++)
		{
			Wipe wipe = base.ConfigInstance.AvailableWipes[i];
			if (wipe.ShouldWipe())
			{
				return wipe;
			}
		}
		return null;
	}

	private (Wipe wipe, DateTime? next) GetUpcomingWipeImpl()
	{
		DateTime now = DateTime.UtcNow;
		return (from job in base.ConfigInstance.AvailableWipes
			select (job: job, CronExpression.Parse(job.Cron).GetNextOccurrence(now, TimeZoneInfo.Utc, false)) into x
			where x.Item2.HasValue
			orderby x.Item2
			select x).FirstOrDefault();
	}

	[ConsoleCommand("autowipe.wipes", "Prints all available wipes present in the Wipes config property.")]
	[AuthLevel(2)]
	private unsafe void print_wipes(Arg arg)
	{
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		StringTable val = default(StringTable);
		((StringTable)(ref val))._002Ector(new string[9] { "#", "wipe name", "mapurl", "mapsize", "serverseed", "type", "temp", "nextwipe", "wipecommands" });
		try
		{
			for (int i = 0; i < base.ConfigInstance.AvailableWipes.Count; i++)
			{
				Wipe wipe = base.ConfigInstance.AvailableWipes[i];
				object[] obj = new object[9]
				{
					i + 1,
					wipe.WipeName,
					wipe.MapUrl,
					wipe.MapSize,
					(wipe.ServerSeed == 0) ? "random" : ((object)wipe.ServerSeed),
					wipe.Type,
					wipe.Temp ? "yes" : "no",
					wipe.Cron,
					null
				};
				string[] commands = wipe.Commands;
				obj[8] = ((commands != null) ? StringArrayEx.ToString((IEnumerable<string>)commands, "->", (string)null) : null);
				((StringTable)(ref val)).AddRow(obj);
			}
			arg.ReplyWith(((StringTable)(ref val)).ToStringMinimal());
		}
		finally
		{
			((IDisposable)(*(StringTable*)(&val))/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ConsoleCommand("autowipe.delete", "Deletes an existent wipe present in the Wipes config property.")]
	[AuthLevel(2)]
	private void delete_wipe(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			arg.ReplyWith("Provide an index from 'autowipe.wipes'");
			return;
		}
		int num = arg.GetInt(0, 0) - 1;
		if (num < 0 || num >= base.ConfigInstance.AvailableWipes.Count)
		{
			arg.ReplyWith("Went above or below indexes available. Use numbers from 'autowipe.wipes`'");
			return;
		}
		base.ConfigInstance.AvailableWipes.RemoveAt(num);
		((BaseModule)this).Save();
		arg.ReplyWith("Removed wipe");
	}

	[ConsoleCommand("autowipe.add", "Adds a new wipe to the list.")]
	[AuthLevel(2)]
	private void add_wipe(Arg arg)
	{
		if (!arg.HasArgs(8))
		{
			arg.ReplyWith("You've got missing arguments. Please make sure to follow the following syntax:\neg. autowipe.add \"<WipeName>\" \"<MapBrowserName>\" \"<MapUrl>\" \"<MapSize>\" \"<ServerSeed|0=random>\" \"<Type|0=fullwipe 1=mapwipe>\" \"<Temp|True/False>\" \"<Cron>\" \"<Commands>\"");
			return;
		}
		base.ConfigInstance.AvailableWipes.Add(new Wipe
		{
			WipeName = arg.GetString(0, ""),
			MapBrowserName = arg.GetString(1, ""),
			MapUrl = arg.GetString(2, ""),
			MapSize = arg.GetInt(3, 0),
			ServerSeed = arg.GetInt(4, 0),
			Type = (WipeTypes)arg.GetInt(5, 0),
			Temp = arg.GetBool(6, false),
			Cron = arg.GetString(7, ""),
			Commands = arg.GetString(8, "").Split(splitter, StringSplitOptions.RemoveEmptyEntries)
		});
		((BaseModule)this).Save();
		arg.ReplyWith("Added wipe");
	}

	[ConsoleCommand("autowipe.maps", "Prints all available map urls present in the MapPool config property.")]
	[AuthLevel(2)]
	private unsafe void print_maps(Arg arg)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		StringTable val = default(StringTable);
		((StringTable)(ref val))._002Ector(new string[3] { "", "map url", "temporary" });
		try
		{
			for (int i = 0; i < base.ConfigInstance.Maps.Count; i++)
			{
				WipeMap wipeMap = base.ConfigInstance.Maps[i];
				((StringTable)(ref val)).AddRow(new object[3]
				{
					i + 1,
					wipeMap.Url,
					wipeMap.Temp ? "temp" : "standard"
				});
			}
			arg.ReplyWith(((StringTable)(ref val)).ToStringMinimal());
		}
		finally
		{
			((IDisposable)(*(StringTable*)(&val))/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ConsoleCommand("autowipe.deletemap", "Deletes an existent map url present in the MapPool config property.")]
	[AuthLevel(2)]
	private void delete_map(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			arg.ReplyWith("Provide an index from 'autowipe.maps'");
			return;
		}
		int num = arg.GetInt(0, 0);
		if (num < 0 || num >= base.ConfigInstance.AvailableWipes.Count)
		{
			arg.ReplyWith("Went above or below indexes available. Use numbers from 'autowipe.maps`'");
			return;
		}
		base.ConfigInstance.AvailableWipes.RemoveAt(num);
		((BaseModule)this).Save();
		arg.ReplyWith("Removed map URL");
	}

	[ConsoleCommand("autowipe.addmap", "Adds a new map URLs to the list.")]
	[AuthLevel(2)]
	private void add_map(Arg arg)
	{
		if (!arg.HasArgs(1))
		{
			arg.ReplyWith("You've got missing arguments. Please make sure to follow the following syntax:\neg. autowipe.addmap \"<MapUrl>\" \"<Temp|True/False>\"");
			return;
		}
		string url = arg.GetString(0, "");
		bool temp = arg.GetBool(1, false);
		if (base.ConfigInstance.Maps.Any((WipeMap x) => x.Url.Equals(url, StringComparison.OrdinalIgnoreCase)))
		{
			arg.ReplyWith("Map url '" + url + "' already exists in the pool");
			return;
		}
		base.ConfigInstance.Maps.Add(new WipeMap
		{
			Url = url,
			Temp = temp
		});
		((BaseModule)this).Save();
		arg.ReplyWith("Added map url");
	}

	[ConsoleCommand("autowipe.wipechat", "Updates the wipe chat command.")]
	[AuthLevel(2)]
	private void wipe_chat(Arg arg)
	{
		string text = arg.GetString(0, "");
		bool flag = UpdateWipeChatCommand(base.ConfigInstance.WipeChatCommand, text);
		arg.ReplyWith(flag ? ("Updated Wipe chat command to '" + text + "'") : "Wipe chat command has not been changed.");
		if (flag)
		{
			((BaseModule)this).Save();
		}
	}

	public override object InternalCallHook(uint hook, object[] args)
	{
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		int? num = args?.Length;
		object obj = ((num > 0) ? args[0] : null);
		object obj2 = ((num > 1) ? args[1] : null);
		object obj3 = ((num > 2) ? args[2] : null);
		try
		{
			switch (hook)
			{
			case 1454470639u:
				return GetUpcomingAvailableWipeImpl();
			case 3261751851u:
				return GetUpcomingWipeImpl();
			case 4109979236u:
				OnServerInformationUpdated();
				return null;
			case 1766141157u:
				RefreshHostName();
				return null;
			case 252653938u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag5 = flag;
				BasePlayer player = ((!flag5) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag6 = flag;
				string cmd = (flag6 ? ((string)(obj2 ?? null)) : null);
				flag = ((obj3 is string[] || obj3 == null) ? true : false);
				bool flag7 = flag;
				string[] args2 = (flag7 ? ((string[])(obj3 ?? null)) : null);
				if (flag5 & flag6 & flag7)
				{
					WipeChat(player, cmd, args2);
					return null;
				}
				break;
			}
			case 2972648362u:
				WipeTickImpl();
				return null;
			case 769232612u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag8 = flag;
				Arg arg4 = ((!flag8) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag8)
				{
					add_map(arg4);
					return null;
				}
				break;
			}
			case 2453418844u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag11 = flag;
				Arg arg7 = ((!flag11) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag11)
				{
					add_wipe(arg7);
					return null;
				}
				break;
			}
			case 1791877600u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag3 = flag;
				Arg arg2 = ((!flag3) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag3)
				{
					delete_map(arg2);
					return null;
				}
				break;
			}
			case 2562168397u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag4 = flag;
				Arg arg3 = ((!flag4) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag4)
				{
					delete_wipe(arg3);
					return null;
				}
				break;
			}
			case 1689166025u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag9 = flag;
				Arg arg5 = ((!flag9) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag9)
				{
					print_maps(arg5);
					return null;
				}
				break;
			}
			case 2650953489u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag10 = flag;
				Arg arg6 = ((!flag10) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag10)
				{
					print_wipes(arg6);
					return null;
				}
				break;
			}
			case 4226930406u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag2 = flag;
				Arg arg = ((!flag2) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag2)
				{
					wipe_chat(arg);
					return null;
				}
				break;
			}
			}
		}
		catch (Exception ex)
		{
			Logger.Error((object)string.Format("Failed to call internal hook '{0}' on module '{1} v{2}' [{3}]", new object[4]
			{
				HookStringPool.GetOrAdd(hook),
				((CarbonModule<AutoWipeConfig, AutoWipeData>)this).Name,
				((BaseHookable)this).Version,
				hook
			}), ex);
			((BaseHookable)this).OnException(hook);
		}
		return null;
	}
}

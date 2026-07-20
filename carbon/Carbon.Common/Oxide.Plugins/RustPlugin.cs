using System;
using System.Collections.Generic;
using System.IO;
using Carbon;
using Carbon.Core;
using Carbon.Extensions;
using Cysharp.Text;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Libraries;
using UnityEngine;

namespace Oxide.Plugins;

public class RustPlugin : Plugin
{
	public Lang lang;

	public Server server;

	public Oxide.Core.Libraries.Plugins plugins;

	public Timers timer;

	public OxideMod mod;

	public WebRequests webrequest;

	public Rust rust;

	public Covalence covalence;

	private string _cachedLogFolder;

	private string _cachedDateStr;

	private int _cachedDay;

	private HashSet<string> _createdLogFolders;

	private bool loadedDefaultMessages;

	public bool IsPrecompiled { get; set; }

	public bool IsExtension { get; set; }

	public Player Player
	{
		get
		{
			return rust.Player;
		}
		private set
		{
		}
	}

	public Server Server
	{
		get
		{
			return rust.Server;
		}
		private set
		{
		}
	}

	public virtual void SetupMod(ModLoader.Package mod, string name, string author, VersionNumber version, string description)
	{
		Package = mod;
		Setup(name, author, version, description);
	}

	public virtual void Setup(string name, string author, VersionNumber version, string description)
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		base.Name = GetType().Name;
		base.Title = name.Replace(":", string.Empty);
		Version = version;
		base.Author = author;
		base.Description = description;
		permission = Interface.Oxide.Permission;
		cmd = new Command();
		server = new Server();
		base.Manager = new PluginManager();
		plugins = new Oxide.Core.Libraries.Plugins(base.Manager);
		timer = new Timers(this);
		lang = new Lang(this);
		mod = Interface.Oxide;
		rust = new Rust();
		webrequest = new WebRequests();
		persistence = new GameObject("Script_" + name).AddComponent<Persistence>();
		Object.DontDestroyOnLoad((Object)(object)((Component)persistence).gameObject);
		covalence = new Covalence();
		base.HookableType = GetType();
	}

	public override void Dispose()
	{
		permission.UnregisterPermissions(this);
		timer?.Clear();
		timer = null;
		_createdLogFolders = null;
		_cachedLogFolder = null;
		_cachedDateStr = null;
		if ((Object)(object)persistence != (Object)null)
		{
			GameObject gameObject = ((Component)persistence).gameObject;
			Object.DestroyImmediate((Object)(object)persistence);
			Object.Destroy((Object)(object)gameObject);
		}
		base.Dispose();
	}

	public static T Singleton<T>()
	{
		foreach (ModLoader.Package package in ModLoader.Packages)
		{
			foreach (RustPlugin plugin in package.Plugins)
			{
				if (plugin is T)
				{
					return (T)(object)((plugin is T) ? plugin : null);
				}
			}
		}
		return default(T);
	}

	public void Puts(object message)
	{
		Logger.Log($"[{base.Title}] {message}");
	}

	public void Puts(object message, params object[] args)
	{
		Logger.Log($"[{base.Title}] {((args == null || args.Length == 0) ? message : string.Format(message?.ToString() ?? string.Empty, args))}");
	}

	public void Puts(string message, params object[] args)
	{
		Puts((object)message, args);
	}

	public void Log(object message)
	{
		Logger.Log($"[{base.Title}] {message}");
	}

	public void Log(object message, params object[] args)
	{
		Logger.Log($"[{base.Title}] {((args == null || args.Length == 0) ? message : string.Format(message?.ToString() ?? string.Empty, args))}");
	}

	public void LogWarning(object message)
	{
		Logger.Warn($"[{base.Title}] {message}");
	}

	public void LogWarning(object message, params object[] args)
	{
		Logger.Warn($"[{base.Title}] {((args == null || args.Length == 0) ? message : string.Format(message?.ToString() ?? string.Empty, args))}");
	}

	public void LogError(object message, Exception ex)
	{
		Logger.Error($"[{base.Title}] {message}", ex);
	}

	public void LogError(object message, Exception ex, params object[] args)
	{
		Logger.Error($"[{base.Title}] {((args == null || args.Length == 0) ? message : string.Format(message?.ToString() ?? string.Empty, args))}", ex);
	}

	public void LogError(object message)
	{
		Logger.Error($"[{base.Title}] {message}");
	}

	public void LogError(object message, params object[] args)
	{
		Logger.Error($"[{base.Title}] {((args == null || args.Length == 0) ? message : string.Format(message?.ToString() ?? string.Empty, args))}");
	}

	public void PrintWarning(object format, params object[] args)
	{
		Logger.Warn($"[{base.Title}] {((args == null || args.Length == 0) ? format : string.Format(format?.ToString() ?? string.Empty, args))}");
	}

	public void PrintError(object format, params object[] args)
	{
		Logger.Error($"[{base.Title}] {((args == null || args.Length == 0) ? format : string.Format(format?.ToString() ?? string.Empty, args))}");
	}

	public void RaiseError(object message)
	{
		Logger.Error($"[{base.Title}] {message}");
	}

	protected void LogToFile(string filename, string text, Plugin plugin = null, bool timeStamp = true, bool anotherBool = false)
	{
		if (!string.IsNullOrEmpty(filename) && !string.IsNullOrEmpty(text))
		{
			DateTime now = DateTime.Now;
			if (_cachedDay != now.Day)
			{
				_cachedDay = now.Day;
				_cachedDateStr = now.ToString("yyyy-MM-dd");
			}
			string text2;
			string path;
			if (plugin == null)
			{
				string directoryName = Path.GetDirectoryName(filename);
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
				text2 = (string.IsNullOrEmpty(directoryName) ? Defines.GetLogsFolder() : Path.Combine(Defines.GetLogsFolder(), directoryName));
				path = (timeStamp ? ZString.Concat<string, string, string, string>(fileNameWithoutExtension, "-", _cachedDateStr, ".txt") : ZString.Concat<string, string>(fileNameWithoutExtension, ".txt"));
			}
			else
			{
				text2 = _cachedLogFolder ?? (_cachedLogFolder = Path.Combine(Defines.GetLogsFolder(), plugin.Name));
				path = (timeStamp ? ZString.Concat<string, string, string, string, string, string>(plugin.Name, "_", filename, "-", _cachedDateStr, ".txt").ToLower() : ZString.Concat<string, string, string, string>(plugin.Name, "_", filename, ".txt").ToLower());
			}
			if (_createdLogFolders == null)
			{
				_createdLogFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			}
			if (_createdLogFolders.Add(text2))
			{
				OsEx.Folder.Create(text2);
			}
			string file = Path.Combine(text2, Utility.CleanPath(path));
			string content = (timeStamp ? ZString.Concat<string, string, string, string, string, string, string>("[", _cachedDateStr, " ", now.ToString("HH:mm:ss"), "] ", text, Environment.NewLine) : ZString.Concat<string, string>(text, Environment.NewLine));
			OsEx.File.Append(file, content);
		}
	}

	public static T GetLibrary<T>(string name = null) where T : Library
	{
		return Interface.Oxide.GetLibrary<T>();
	}

	public void ILoadConfig()
	{
		try
		{
			LoadConfig();
		}
		catch (Exception ex)
		{
			LogError("Failed ILoadConfig", ex);
		}
	}

	public void ILoadDefaultMessages()
	{
		if (!loadedDefaultMessages)
		{
			CallHook("LoadDefaultMessages");
			loadedDefaultMessages = true;
		}
	}

	public override string ToPrettyString()
	{
		return $"{base.Title} v{Version} by {base.Author}";
	}

	protected void PrintWarning(object message)
	{
		LogWarning(message);
	}

	protected void PrintWarning(string format, params object[] args)
	{
		LogWarning(format, args);
	}

	protected void PrintToConsole(BasePlayer player, string format, params object[] args)
	{
		if (!((Object)(object)player == (Object)null) && ((BaseNetworkable)player).net != null)
		{
			player.SendConsoleCommand("echo " + ((args.Length != 0) ? string.Format(format, args) : format), Array.Empty<object>());
		}
	}

	protected void PrintToConsole(string format, params object[] args)
	{
		if (BasePlayer.activePlayerList.Count >= 1)
		{
			ConsoleNetwork.BroadcastToAllClients("echo " + ((args.Length != 0) ? string.Format(format, args) : format), Array.Empty<object>());
		}
	}

	protected void PrintToChat(BasePlayer player, string format, params object[] args)
	{
		if (!((Object)(object)player == (Object)null) && ((BaseNetworkable)player).net != null)
		{
			player.SendConsoleCommand("chat.add", new object[3]
			{
				2,
				Community.Runtime.Core.DefaultServerChatId,
				(args.Length != 0) ? string.Format(format, args) : format
			});
		}
	}

	protected void PrintToChat(string format, params object[] args)
	{
		if (BasePlayer.activePlayerList.Count != 0)
		{
			ConsoleNetwork.BroadcastToAllClients("chat.add", new object[3]
			{
				2,
				Community.Runtime.Core.DefaultServerChatId,
				(args.Length != 0) ? string.Format(format, args) : format
			});
		}
	}

	protected void PrintToChat(BasePlayer player, string format, long chatId, params object[] args)
	{
		if (!((Object)(object)player == (Object)null) && ((BaseNetworkable)player).net != null)
		{
			player.SendConsoleCommand("chat.add", new object[3]
			{
				2,
				chatId,
				(args.Length != 0) ? string.Format(format, args) : format
			});
		}
	}

	protected void PrintToChat(string format, long chatId, params object[] args)
	{
		if (BasePlayer.activePlayerList.Count > 0)
		{
			ConsoleNetwork.BroadcastToAllClients("chat.add", new object[3]
			{
				2,
				chatId,
				(args.Length != 0) ? string.Format(format, args) : format
			});
		}
	}

	protected void SendReply(Arg arg, string format, params object[] args)
	{
		if (arg != null || arg.Connection != null)
		{
			MonoBehaviour obj = arg.Connection?.player;
			BasePlayer val = (BasePlayer)(object)((obj is BasePlayer) ? obj : null);
			if ((Object)(object)val != (Object)null && ((BaseNetworkable)val).net != null)
			{
				val.SendConsoleCommand("echo " + ((args != null && args.Length != 0) ? string.Format(format, args) : format), Array.Empty<object>());
				return;
			}
		}
		Puts(format, args);
	}

	protected void SendReply(BasePlayer player, string format, params object[] args)
	{
		PrintToChat(player, format, args);
	}

	protected void SendWarning(Arg arg, string format, params object[] args)
	{
		MonoBehaviour obj = arg.Connection?.player;
		BasePlayer val = (BasePlayer)(object)((obj is BasePlayer) ? obj : null);
		if ((Object)(object)val != (Object)null && ((BaseNetworkable)val).net != null)
		{
			val.SendConsoleCommand("echo " + ((args != null && args.Length != 0) ? string.Format(format, args) : format), Array.Empty<object>());
		}
		else
		{
			PrintWarning(format, args);
		}
	}

	protected void SendError(Arg arg, string format, params object[] args)
	{
		MonoBehaviour obj = arg.Connection?.player;
		BasePlayer val = (BasePlayer)(object)((obj is BasePlayer) ? obj : null);
		if ((Object)(object)val != (Object)null && ((BaseNetworkable)val).net != null)
		{
			val.SendConsoleCommand("echo " + ((args != null && args.Length != 0) ? string.Format(format, args) : format), Array.Empty<object>());
		}
		else
		{
			PrintError(format, args);
		}
	}

	protected void ForcePlayerPosition(BasePlayer player, Vector3 destination)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		player.MovePosition(destination, true);
		if (!player.IsSpectating() || (double)Vector3.Distance(((Component)player).transform.position, destination) > 25.0)
		{
			((BaseEntity)player).ClientRPC(RpcTarget.Player("ForcePositionTo", player), destination);
		}
		else
		{
			((BaseNetworkable)player).SendNetworkUpdate((NetworkQueue)1);
		}
	}

	protected void AddCovalenceCommand(string command, string callback, params string[] perms)
	{
		cmd.AddCovalenceCommand(command, this, callback, null, null, perms);
		if (perms == null)
		{
			return;
		}
		foreach (string name in perms)
		{
			if (!permission.PermissionExists(name))
			{
				permission.RegisterPermission(name, this);
			}
		}
	}

	protected void AddCovalenceCommand(string[] commands, string callback, params string[] perms)
	{
		foreach (string command in commands)
		{
			cmd.AddCovalenceCommand(command, this, callback, null, null, perms);
		}
		if (perms == null)
		{
			return;
		}
		foreach (string name in perms)
		{
			if (!permission.PermissionExists(name))
			{
				permission.RegisterPermission(name, this);
			}
		}
	}

	protected void AddUniversalCommand(string command, string callback, params string[] perms)
	{
		cmd.AddCovalenceCommand(command, this, callback, null, null, perms);
		if (perms == null)
		{
			return;
		}
		foreach (string name in perms)
		{
			if (!permission.PermissionExists(name))
			{
				permission.RegisterPermission(name, this);
			}
		}
	}

	protected void AddUniversalCommand(string[] commands, string callback, params string[] perms)
	{
		foreach (string command in commands)
		{
			cmd.AddCovalenceCommand(command, this, callback, null, null, perms);
		}
		if (perms == null)
		{
			return;
		}
		foreach (string name in perms)
		{
			if (!permission.PermissionExists(name))
			{
				permission.RegisterPermission(name, this);
			}
		}
	}
}

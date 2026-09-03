using System;
using System.Collections.Generic;
using System.IO;
using API.Commands;
using API.Logger;
using Oxide.Core.Libraries;

namespace Carbon.Core;

[Serializable]
public class Config
{
	public class SelfUpdatingConfig
	{
		public bool Enabled = true;

		public bool HookUpdates = true;

		public string RedirectUri;
	}

	public class CompilerConfig
	{
		public bool EnableProxy = true;

		public bool CompileTestMode;

		public bool UnloadOnFailure;

		public List<string> ConditionalCompilationSymbols;
	}

	public class ProfilerConfig
	{
		public bool RecordingWarnings = true;
	}

	public class WatchersConfig
	{
		public bool ScriptWatchers = true;

		public bool ZipScriptWatchers = true;

		public SearchOption ScriptWatcherOption;
	}

	public class PermissionsConfig
	{
		public string PlayerDefaultGroup = "default";

		public string AdminDefaultGroup = "admin";

		public string ModeratorDefaultGroup = "moderator";

		public bool AutoGrantPlayerGroup = true;

		public bool AutoGrantAdminGroup = true;

		public bool AutoGrantModeratorGroup = true;

		public bool BypassAdminCooldowns;

		public Permission.SerializationMode PermissionSerialization;

		public bool SqlPermissionUserPreload = true;
	}

	public class ProcessorsConfig
	{
		public float ScriptProcessingRate = 0.2f;

		public float ZipScriptProcessingRate = 0.5f;
	}

	public class DebuggingConfig
	{
		public string ScriptDebuggingOrigin = string.Empty;

		public int HookLagSpikeThreshold = 1000;

		public bool TrackHookMemory = true;
	}

	public class LoggingConfig
	{
		public double LogSplitSize = 2.5;

		public Severity LogSeverity = Severity.Notice;

		public int LogFileMode = 2;

		public int LogVerbosity;

		public bool CommandSuggestions = true;

		public bool ReducedLogging = true;
	}

	public class AnalyticsConfig
	{
		public bool Enabled = true;
	}

	public class PublicizerConfig
	{
		public List<string> PublicizedAssemblies;

		public List<string> PublicizerMemberIgnores;
	}

	public class MiscConfig
	{
		public bool ShowConsoleInfo = true;
	}

	public bool DeveloperMode;

	public bool IsModded = true;

	public List<Command.Prefix> Prefixes = new List<Command.Prefix>();

	public Dictionary<string, string> Aliases;

	public bool Rcon = true;

	public string Language = "en";

	public string WebRequestIp;

	public WatchersConfig Watchers = new WatchersConfig();

	public PermissionsConfig Permissions = new PermissionsConfig();

	public AnalyticsConfig Analytics = new AnalyticsConfig();

	public SelfUpdatingConfig SelfUpdating = new SelfUpdatingConfig();

	public DebuggingConfig Debugging = new DebuggingConfig();

	public ProcessorsConfig Processors = new ProcessorsConfig();

	public PublicizerConfig Publicizer = new PublicizerConfig();

	public LoggingConfig Logging = new LoggingConfig();

	public ProfilerConfig Profiler = new ProfilerConfig();

	public CompilerConfig Compiler = new CompilerConfig();

	public MiscConfig Misc = new MiscConfig();

	internal readonly string[] _invalidAliases = new string[2] { "c.", "carbon." };

	public bool IsValidAlias(string input, out string reason)
	{
		reason = null;
		if (input.Contains(" "))
		{
			return false;
		}
		string[] invalidAliases = _invalidAliases;
		foreach (string text in invalidAliases)
		{
			if (input.StartsWith(text, StringComparison.OrdinalIgnoreCase))
			{
				reason = text;
				return false;
			}
		}
		return true;
	}
}

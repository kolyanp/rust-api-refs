using System.Linq;
using Newtonsoft.Json;

namespace Carbon.Modules;

public class AdminExtensionsConfig
{
	public class NameFilterSettings
	{
		public enum FilterModes
		{
			None,
			Kick,
			Rename
		}

		[JsonProperty("Mode (0=None 1=Kick 2=Rename)")]
		public FilterModes Mode;

		public string BypassPermission = "adminextensions.namefilter.bypass";

		public string CharacterWhitelist = "._- ";

		public string KickMessage = "Your name must only contain English alphanumeric characters.";

		public bool IsCharacterValid(char character)
		{
			if (!char.IsLetterOrDigit(character))
			{
				return CharacterWhitelist.Contains(character.ToString());
			}
			return true;
		}

		public bool IsValid(string displayName)
		{
			foreach (char character in displayName)
			{
				if (!IsCharacterValid(character))
				{
					return false;
				}
			}
			return true;
		}

		public bool TryRename(string displayName, out string correctedName)
		{
			correctedName = string.Join(string.Empty, displayName.Where((char x) => IsCharacterValid(x)));
			return displayName != correctedName;
		}
	}

	public class CommandSettings
	{
		public string Command;

		public string Permission;
	}

	public NameFilterSettings NameFilter = new NameFilterSettings();

	public CommandSettings Blind = new CommandSettings
	{
		Command = "blind",
		Permission = "adminextensions.blind"
	};

	public CommandSettings Empower = new CommandSettings
	{
		Command = "empower",
		Permission = "adminextensions.empower"
	};

	public CommandSettings PrivateMessage = new CommandSettings
	{
		Command = "cpm",
		Permission = "adminextensions.pm"
	};

	public CommandSettings Lock = new CommandSettings
	{
		Command = "lock",
		Permission = "adminextensions.lock"
	};

	public CommandSettings TeleportMarker = new CommandSettings
	{
		Command = "tpm",
		Permission = "adminextensions.tpm"
	};

	public CommandSettings Mute = new CommandSettings
	{
		Command = "mute",
		Permission = "adminextensions.mute"
	};

	public CommandSettings MuteList = new CommandSettings
	{
		Command = "mutelist",
		Permission = "adminextensions.mutelist"
	};

	public CommandSettings Ban = new CommandSettings
	{
		Command = "ban",
		Permission = "adminextensions.ban"
	};

	public CommandSettings Unban = new CommandSettings
	{
		Command = "unban",
		Permission = "adminextensions.unban"
	};

	public CommandSettings Kick = new CommandSettings
	{
		Command = "kick",
		Permission = "adminextensions.kick"
	};

	public CommandSettings ToggleCadmin = new CommandSettings
	{
		Command = "cadmin",
		Permission = "adminextensions.cadmin"
	};
}

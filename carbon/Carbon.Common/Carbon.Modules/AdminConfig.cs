using System.Collections.Generic;
using Newtonsoft.Json;

namespace Carbon.Modules;

public class AdminConfig
{
	public class ActionButton
	{
		public string Name;

		public string Command;

		public bool User;

		public bool IncludeUserId;

		public bool ConfirmDialog;
	}

	[JsonProperty("OpenCommands")]
	public string[] OpenCommands = new string[2] { "cp", "cpanel" };

	public int MinimumAuthLevel = 2;

	public int MaximumAuthLevel = 2;

	public bool SpectatingInfoOverlay = true;

	public bool SpectatingEndTeleportBack;

	public List<ActionButton> QuickActions = new List<ActionButton>();

	public bool HideConsole;

	public bool PlayPMSound = true;

	public string PMSound = "assets/prefabs/locks/keypad/effects/lock.code.unlock.prefab";
}

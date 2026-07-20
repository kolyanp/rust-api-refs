using Carbon;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Libraries.Covalence;
using Oxide.Plugins;

namespace Oxide.Core.Libraries.Covalence;

public class Covalence : Library, ICovalence
{
	public IPlayerManager Players { get; } = default(CovalencePlugin.PlayerManager);

	public IServer Server { get; } = new RustServer();

	public uint ClientAppId { get; } = 252490u;

	public string Game { get; } = "Rust";

	public string FormatText(string text)
	{
		return Formatter.ToUnity(text);
	}

	public void UnregisterCommand(string command, Plugin plugin)
	{
		Community.Runtime.Core.cmd.RemoveConsoleCommand(command, plugin);
	}
}

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using Oxide.Game.Rust.Libraries.Covalence;
using UnityEngine;

namespace Oxide.Plugins;

public class CovalencePlugin : RustPlugin
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct PlayerManager : IPlayerManager
	{
		internal static IEnumerable<IPlayer> _all;

		public IEnumerable<IPlayer> All => _all;

		public IEnumerable<IPlayer> Connected => ((IEnumerable<BasePlayer>)BasePlayer.activePlayerList).Select((BasePlayer x) => x.AsIPlayer());

		public static void RefreshDatabase(Dictionary<string, UserData> data)
		{
			_all = data.Select((KeyValuePair<string, UserData> x) => x.AsIPlayer());
		}

		public IPlayer FindPlayer(string partialNameOrId)
		{
			return All.FirstOrDefault((IPlayer x) => x.Id.Equals(partialNameOrId) || StringEx.Contains(x.Name, partialNameOrId, CompareOptions.OrdinalIgnoreCase));
		}

		public IPlayer FindPlayerById(string id)
		{
			return All.FirstOrDefault((IPlayer x) => x.Id.Equals(id));
		}

		public IPlayer FindPlayerByObj(object obj)
		{
			string text = obj?.ToString();
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			return FindPlayer(text);
		}

		public IEnumerable<IPlayer> FindPlayers(string partialNameOrId)
		{
			return All.Where((IPlayer x) => x.Id.Equals(partialNameOrId) || StringEx.Contains(x.Name, partialNameOrId, CompareOptions.OrdinalIgnoreCase));
		}
	}

	protected string game = "Rust";

	public PlayerManager players;

	public new RustServer server = new RustServer();
}

using System.Collections.Generic;

namespace Oxide.Core.Libraries.Covalence;

public interface IPlayerManager
{
	IEnumerable<IPlayer> All { get; }

	IEnumerable<IPlayer> Connected { get; }

	IPlayer FindPlayerById(string id);

	IPlayer FindPlayerByObj(object obj);

	IPlayer FindPlayer(string partialNameOrId);

	IEnumerable<IPlayer> FindPlayers(string partialNameOrId);
}

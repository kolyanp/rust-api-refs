using System;
using System.Globalization;
using System.Net;

namespace Oxide.Core.Libraries.Covalence;

public interface IServer
{
	string Name { get; set; }

	IPAddress Address { get; }

	IPAddress LocalAddress { get; }

	ushort Port { get; }

	string Version { get; }

	string Protocol { get; }

	CultureInfo Language { get; }

	SaveInfo SaveInfo { get; }

	int Players { get; }

	int MaxPlayers { get; set; }

	DateTime Time { get; set; }

	void Ban(string id, string reason, TimeSpan duration = default(TimeSpan));

	TimeSpan BanTimeRemaining(string id);

	bool IsBanned(string id);

	void Unban(string id);

	void Save();

	void Broadcast(string message, string prefix, params object[] args);

	void Broadcast(string message);

	void Command(string command, params object[] args);
}

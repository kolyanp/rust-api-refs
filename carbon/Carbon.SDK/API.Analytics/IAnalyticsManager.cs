using System.Collections.Generic;

namespace API.Analytics;

public interface IAnalyticsManager
{
	Dictionary<string, object> Segments { get; set; }

	bool Enabled { get; }

	string Branch { get; }

	string ClientID { get; }

	string InformationalVersion { get; }

	string Platform { get; }

	string Protocol { get; }

	string SessionID { get; }

	string SystemID { get; }

	string UserAgent { get; }

	string Version { get; }

	bool HasNewIdentifier { get; }

	void SessionStart();

	void LogEvent(string eventName);

	void LogEvents(string eventName);
}

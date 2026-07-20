using System.Runtime.CompilerServices;
using System.Threading;

namespace MySqlConnector.Core;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
internal interface ICancellableCommand
{
	int CommandId { get; }

	int CommandTimeout { get; }

	int? EffectiveCommandTimeout { get; set; }

	int CancelAttemptCount { get; set; }

	MySqlConnection Connection { get; }

	bool IsTimedOut { get; }

	CancellationTokenRegistration RegisterCancel(CancellationToken cancellationToken);

	void SetTimeout(int milliseconds);
}

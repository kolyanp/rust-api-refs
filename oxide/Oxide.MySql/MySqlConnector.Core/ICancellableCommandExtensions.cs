using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MySqlConnector.Core;

internal static class ICancellableCommandExtensions
{
	private static int s_id = 1;

	public static int GetNextId()
	{
		return Interlocked.Increment(ref s_id);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public static void ResetCommandTimeout(this ICancellableCommand command)
	{
		int? num = command.EffectiveCommandTimeout;
		if (num == int.MaxValue)
		{
			return;
		}
		ServerSession serverSession = command.Connection?.Session;
		if (serverSession != null)
		{
			if (!num.HasValue)
			{
				int commandTimeout = command.CommandTimeout;
				int cancellationTimeout = serverSession.CancellationTimeout;
				num = (command.EffectiveCommandTimeout = ((commandTimeout != 0 && cancellationTimeout != 0) ? new int?(Math.Min(commandTimeout, Math.Max(1, 2147483 - Math.Max(0, serverSession.CancellationTimeout))) * 1000) : new int?(int.MaxValue)));
			}
			if (num == int.MaxValue)
			{
				serverSession.SetTimeout(int.MaxValue);
			}
			else if (serverSession.CancellationTimeout > 0)
			{
				command.SetTimeout(num.Value);
				serverSession.SetTimeout(num.Value + serverSession.CancellationTimeout * 1000);
			}
			else
			{
				serverSession.SetTimeout(num.Value);
			}
		}
	}
}

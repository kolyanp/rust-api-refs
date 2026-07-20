using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector.Logging;
using MySqlConnector.Protocol;
using MySqlConnector.Protocol.Serialization;
using MySqlConnector.Utilities;

namespace MySqlConnector.Core;

internal static class CommandExecutor
{
	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	public static async ValueTask<MySqlDataReader> ExecuteReaderAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, CommandBehavior behavior, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		_ = 2;
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			IMySqlCommand command = commandListPosition.CommandAt(0);
			MySqlConnection connection = command.Connection;
			Log.CommandExecutorExecuteReader(command.Logger, connection.Session.Id, ioBehavior, commandListPosition.CommandCount);
			Dictionary<string, CachedProcedure> cachedProcedures = null;
			for (int commandIndex = 0; commandIndex < commandListPosition.CommandCount; commandIndex++)
			{
				IMySqlCommand mySqlCommand = commandListPosition.CommandAt(commandIndex);
				if (mySqlCommand.CommandType == CommandType.StoredProcedure)
				{
					if (cachedProcedures == null)
					{
						cachedProcedures = new Dictionary<string, CachedProcedure>();
					}
					string commandText = mySqlCommand.CommandText;
					if (!cachedProcedures.ContainsKey(commandText))
					{
						Dictionary<string, CachedProcedure> dictionary = cachedProcedures;
						string key = commandText;
						dictionary.Add(key, await connection.GetCachedProcedure(commandText, revalidateMissing: false, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
						command.CancellableCommand.ResetCommandTimeout();
					}
				}
			}
			ByteBufferWriter byteBufferWriter = new ByteBufferWriter();
			if (!payloadCreator.WriteQueryCommand(ref commandListPosition, cachedProcedures, byteBufferWriter, appendSemicolon: false))
			{
				throw new InvalidOperationException("ICommandPayloadCreator failed to write query payload");
			}
			cancellationToken.ThrowIfCancellationRequested();
			using PayloadData payload = byteBufferWriter.ToPayloadData();
			ServerSession session = connection.Session;
			session.StartQuerying(command.CancellableCommand);
			command.SetLastInsertedId(0L);
			try
			{
				await session.SendAsync(payload, ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
				await session.DataReader.InitAsync(commandListPosition, payloadCreator, cachedProcedures, command, behavior, activity, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return session.DataReader;
			}
			catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.QueryInterrupted && cancellationToken.IsCancellationRequested)
			{
				Log.QueryWasInterrupted(command.Logger, session.Id);
				throw new OperationCanceledException(ex.Message, ex, cancellationToken);
			}
			catch (Exception ex2) when (((Func<bool>)delegate
			{
				// Could not convert BlockContainer to single expression
				bool flag = payload.Span.Length > 4194304;
				if (flag)
				{
					flag = ((ex2 is SocketException || ex2 is IOException || ex2 is MySqlProtocolException) ? true : false);
				}
				return flag;
			}).Invoke())
			{
				int num = payload.Span.Length / 1000000;
				throw new MySqlException($"Error submitting {num}MB packet; ensure 'max_allowed_packet' is greater than {num}MB.", ex2);
			}
		}
		catch (Exception exception) when (activity?.IsAllDataRequested ?? false)
		{
			activity.SetException(exception);
			activity.Stop();
			throw;
		}
	}
}

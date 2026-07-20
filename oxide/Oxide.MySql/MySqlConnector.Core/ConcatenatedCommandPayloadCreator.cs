using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MySqlConnector.Logging;
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector.Core;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal sealed class ConcatenatedCommandPayloadCreator : ICommandPayloadCreator
{
	public static ICommandPayloadCreator Instance { get; } = new ConcatenatedCommandPayloadCreator();

	public bool WriteQueryCommand(ref CommandListPosition commandListPosition, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 1, 2 })] IDictionary<string, CachedProcedure> cachedProcedures, ByteBufferWriter writer, bool appendSemicolon)
	{
		if (commandListPosition.CommandIndex == commandListPosition.CommandCount)
		{
			return false;
		}
		writer.Write((byte)3);
		if (commandListPosition.CommandAt(commandListPosition.CommandIndex).Connection.Session.SupportsQueryAttributes)
		{
			writer.WriteLengthEncodedInteger(0uL);
			writer.Write((byte)1);
		}
		bool flag;
		do
		{
			IMySqlCommand mySqlCommand = commandListPosition.CommandAt(commandListPosition.CommandIndex);
			Log.PreparingCommandPayload(mySqlCommand.Logger, mySqlCommand.Connection.Session.Id, mySqlCommand.CommandText);
			flag = SingleCommandPayloadCreator.WriteQueryPayload(mySqlCommand, cachedProcedures, writer, commandListPosition.CommandIndex < commandListPosition.CommandCount - 1 || appendSemicolon, commandListPosition.CommandIndex == 0, commandListPosition.CommandIndex == commandListPosition.CommandCount - 1);
			commandListPosition.CommandIndex++;
		}
		while (commandListPosition.CommandIndex < commandListPosition.CommandCount && flag);
		return true;
	}
}

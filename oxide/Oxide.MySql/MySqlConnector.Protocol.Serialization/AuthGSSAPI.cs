using System;
using System.Net;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector.Core;
using MySqlConnector.Utilities;

namespace MySqlConnector.Protocol.Serialization;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal static class AuthGSSAPI
{
	private static string GetServicePrincipalName(byte[] switchRequest)
	{
		ByteArrayReader byteArrayReader = new ByteArrayReader(MemoryExtensions.AsSpan(switchRequest));
		return Utility.GetString(Encoding.UTF8, byteArrayReader.ReadNullOrEofTerminatedByteString());
	}

	public static async Task<PayloadData> AuthenticateAsync(ConnectionSettings cs, byte[] switchRequestPayloadData, ServerSession session, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		using NegotiateToMySqlConverterStream innerStream = new NegotiateToMySqlConverterStream(session, ioBehavior, cancellationToken);
		using NegotiateStream negotiateStream = new NegotiateStream(innerStream);
		string targetName = ((cs.ServerSPN.Length == 0) ? GetServicePrincipalName(switchRequestPayloadData) : cs.ServerSPN);
		if (ioBehavior != IOBehavior.Synchronous)
		{
			await negotiateStream.AuthenticateAsClientAsync(CredentialCache.DefaultNetworkCredentials, targetName).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			negotiateStream.AuthenticateAsClient(CredentialCache.DefaultNetworkCredentials, targetName);
		}
		if (cs.ServerSPN.Length != 0 && !negotiateStream.IsMutuallyAuthenticated)
		{
			throw new AuthenticationException("GSSAPI : Unable to verify server principal name using authentication type " + negotiateStream.RemoteIdentity?.AuthenticationType);
		}
		return innerStream.MySQLProtocolPayload ?? (await session.ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}
}

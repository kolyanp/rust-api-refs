using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using MySqlConnector.Utilities;

namespace MySqlConnector.Protocol.Serialization;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal static class AuthenticationUtility
{
	public static byte[] GetNullTerminatedPasswordBytes(string password)
	{
		byte[] array = new byte[Encoding.UTF8.GetByteCount(password) + 1];
		Utility.GetBytes(Encoding.UTF8, MemoryExtensions.AsSpan(password), array);
		return array;
	}

	public static byte[] CreateAuthenticationResponse([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)] ReadOnlySpan<byte> challenge, string password)
	{
		if (!string.IsNullOrEmpty(password))
		{
			return HashPassword(challenge, password);
		}
		return Array.Empty<byte>();
	}

	public static byte[] HashPassword([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)] ReadOnlySpan<byte> challenge, string password)
	{
		using SHA1 hashAlgorithm = SHA1.Create();
		Span<byte> span = stackalloc byte[40];
		challenge.CopyTo(span);
		int bytesWritten = Encoding.UTF8.GetByteCount(password);
		Span<byte> span2 = stackalloc byte[bytesWritten];
		Encoding.UTF8.GetBytes(MemoryExtensions.AsSpan(password), span2);
		Span<byte> span3 = stackalloc byte[20];
		hashAlgorithm.TryComputeHash(span2, span3, out bytesWritten);
		hashAlgorithm.TryComputeHash(span3, span.Slice(20, span.Length - 20), out bytesWritten);
		Span<byte> destination = stackalloc byte[20];
		hashAlgorithm.TryComputeHash(span, destination, out bytesWritten);
		for (int i = 0; i < span3.Length; i++)
		{
			span3[i] ^= destination[i];
		}
		return span3.ToArray();
	}

	public static byte[] CreateScrambleResponse([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)] ReadOnlySpan<byte> nonce, string password)
	{
		if (!string.IsNullOrEmpty(password))
		{
			return HashPasswordWithNonce(nonce, password);
		}
		return Array.Empty<byte>();
	}

	private static byte[] HashPasswordWithNonce([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)] ReadOnlySpan<byte> nonce, string password)
	{
		using SHA256 hashAlgorithm = SHA256.Create();
		int bytesWritten = Encoding.UTF8.GetByteCount(password);
		Span<byte> span = stackalloc byte[bytesWritten];
		Encoding.UTF8.GetBytes(MemoryExtensions.AsSpan(password), span);
		Span<byte> span2 = stackalloc byte[32];
		hashAlgorithm.TryComputeHash(span, span2, out bytesWritten);
		bytesWritten = 32 + nonce.Length;
		Span<byte> span3 = stackalloc byte[bytesWritten];
		hashAlgorithm.TryComputeHash(span2, span3, out bytesWritten);
		nonce.CopyTo(span3.Slice(32, span3.Length - 32));
		Span<byte> destination = stackalloc byte[32];
		hashAlgorithm.TryComputeHash(span3, destination, out bytesWritten);
		for (int i = 0; i < span2.Length; i++)
		{
			span2[i] ^= destination[i];
		}
		return span2.ToArray();
	}
}

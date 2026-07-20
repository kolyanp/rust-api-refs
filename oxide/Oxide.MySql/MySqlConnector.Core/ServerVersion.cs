using System;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;
using MySqlConnector.Utilities;

namespace MySqlConnector.Core;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal sealed class ServerVersion
{
	public string OriginalString { get; }

	public Version Version { get; }

	public bool IsMariaDb { get; }

	public static ServerVersion Empty { get; } = new ServerVersion();

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	public ServerVersion(ReadOnlySpan<byte> versionString)
	{
		OriginalString = Utility.GetString(Encoding.ASCII, versionString);
		if (versionString.StartsWith("5.5.5-"u8))
		{
			ref ReadOnlySpan<byte> reference = ref versionString;
			versionString = reference.Slice(6, reference.Length - 6);
			IsMariaDb = true;
		}
		else if (versionString.IndexOf("MariaDB"u8) != -1)
		{
			IsMariaDb = true;
		}
		int value = 0;
		int value2 = 0;
		if (Utf8Parser.TryParse(versionString, out int value3, out int bytesConsumed, '\0'))
		{
			ref ReadOnlySpan<byte> reference = ref versionString;
			int num = bytesConsumed;
			versionString = reference.Slice(num, reference.Length - num);
			if (versionString.Length >= 1 && versionString[0] == 46)
			{
				reference = ref versionString;
				versionString = reference.Slice(1, reference.Length - 1);
				if (Utf8Parser.TryParse(versionString, out value, out bytesConsumed, '\0'))
				{
					reference = ref versionString;
					num = bytesConsumed;
					versionString = reference.Slice(num, reference.Length - num);
					if (versionString.Length >= 1 && versionString[0] == 46)
					{
						reference = ref versionString;
						versionString = reference.Slice(1, reference.Length - 1);
						if (Utf8Parser.TryParse(versionString, out value2, out bytesConsumed, '\0'))
						{
							reference = ref versionString;
							num = bytesConsumed;
							versionString = reference.Slice(num, reference.Length - num);
						}
					}
				}
			}
		}
		Version = new Version(value3, value, value2);
	}

	private ServerVersion()
	{
		OriginalString = "";
		Version = new Version();
	}
}

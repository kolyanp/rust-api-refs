using System.Runtime.CompilerServices;
using System.Text;

namespace System.Net.Sockets;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal sealed class UnixDomainSocketEndPoint : EndPoint
{
	public string Filename { get; }

	public override AddressFamily AddressFamily => AddressFamily.Unix;

	public UnixDomainSocketEndPoint(string filename)
	{
		if (filename == null)
		{
			throw new ArgumentNullException("filename");
		}
		if (filename.Length == 0)
		{
			throw new ArgumentException("Cannot be empty.", "filename");
		}
		Filename = filename;
	}

	private UnixDomainSocketEndPoint()
	{
		Filename = "";
	}

	public override EndPoint Create(SocketAddress socketAddress)
	{
		if (socketAddress.Size == 2)
		{
			return new UnixDomainSocketEndPoint();
		}
		int num = socketAddress.Size - 2;
		byte[] array = new byte[num];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = socketAddress[i + 2];
			if (array[i] == 0)
			{
				num = i;
				break;
			}
		}
		return new UnixDomainSocketEndPoint(Encoding.UTF8.GetString(array, 0, num));
	}

	public override SocketAddress Serialize()
	{
		byte[] bytes = Encoding.UTF8.GetBytes(Filename);
		SocketAddress socketAddress = new SocketAddress(AddressFamily, 2 + bytes.Length + 1);
		for (int i = 0; i < bytes.Length; i++)
		{
			socketAddress[2 + i] = bytes[i];
		}
		socketAddress[2 + bytes.Length] = 0;
		return socketAddress;
	}

	public override string ToString()
	{
		return Filename;
	}

	public override int GetHashCode()
	{
		return Filename.GetHashCode();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	public override bool Equals(object obj)
	{
		if (obj is UnixDomainSocketEndPoint unixDomainSocketEndPoint)
		{
			return Filename == unixDomainSocketEndPoint.Filename;
		}
		return false;
	}
}

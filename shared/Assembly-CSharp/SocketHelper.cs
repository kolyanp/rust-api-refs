using System.Collections.Generic;
using Development.Attributes;

[ResetStaticFields]
public static class SocketHelper
{
	private static Dictionary<uint, uint> socketHashes = new Dictionary<uint, uint>();

	public static uint GetSocketHash(uint prefabID)
	{
		if (!socketHashes.TryGetValue(prefabID, out var value))
		{
			value = CRC.Compute32(0u, 1);
			Socket_Base[] array = PrefabAttribute.server.FindAll<Socket_Base>(prefabID);
			for (int i = 0; i < array.Length; i++)
			{
				value = array[i].GetLayoutHash(value);
			}
			socketHashes.Add(prefabID, value);
		}
		return value;
	}
}

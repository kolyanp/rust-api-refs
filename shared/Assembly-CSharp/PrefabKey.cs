using System;

public sealed class PrefabKey
{
	private EncryptedValue<ulong> salt = BitConverter.ToUInt64(Guid.NewGuid().ToByteArray(), 0);

	public ulong Get(uint prefabID)
	{
		ulong num = salt.Get();
		long num2 = (long)(prefabID ^ num) * -7046029254386353131L;
		long num3 = ((num2 ^ (num2 >>> 32)) + (long)num) * -4658895280553007687L;
		return (ulong)(num3 ^ (num3 >>> 29));
	}
}

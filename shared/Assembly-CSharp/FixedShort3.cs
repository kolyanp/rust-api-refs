using UnityEngine;

public struct FixedShort3(Vector3 vec)
{
	private const int FracBits = 10;

	private const float MaxFrac = 1024f;

	private const float RcpMaxFrac = 1f / 1024f;

	public short x = (short)(vec.x * 1024f);

	public short y = (short)(vec.y * 1024f);

	public short z = (short)(vec.z * 1024f);

	public static explicit operator Vector3(FixedShort3 vec)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3((float)vec.x * (1f / 1024f), (float)vec.y * (1f / 1024f), (float)vec.z * (1f / 1024f));
	}
}

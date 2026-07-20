using UnityEngine;

public struct FixedSByteNorm4(Vector4 vec)
{
	private const int FracBits = 7;

	private const float MaxFrac = 128f;

	private const float RcpMaxFrac = 1f / 128f;

	public sbyte x = (sbyte)(vec.x * 128f);

	public sbyte y = (sbyte)(vec.y * 128f);

	public sbyte z = (sbyte)(vec.z * 128f);

	public sbyte w = (sbyte)(vec.w * 128f);

	public static explicit operator Vector4(FixedSByteNorm4 vec)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		return new Vector4((float)vec.x * (1f / 128f), (float)vec.y * (1f / 128f), (float)vec.z * (1f / 128f), (float)vec.w * (1f / 128f));
	}
}

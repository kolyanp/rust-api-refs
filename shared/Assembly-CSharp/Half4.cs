using UnityEngine;

public struct Half4(Vector4 vec)
{
	public ushort x = Mathf.FloatToHalf(vec.x);

	public ushort y = Mathf.FloatToHalf(vec.y);

	public ushort z = Mathf.FloatToHalf(vec.z);

	public ushort w = Mathf.FloatToHalf(vec.w);

	public static explicit operator Vector4(Half4 vec)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector4(Mathf.HalfToFloat(vec.x), Mathf.HalfToFloat(vec.y), Mathf.HalfToFloat(vec.z), Mathf.HalfToFloat(vec.w));
	}
}

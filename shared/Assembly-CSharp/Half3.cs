using UnityEngine;

public struct Half3(Vector3 vec)
{
	public ushort x = Mathf.FloatToHalf(vec.x);

	public ushort y = Mathf.FloatToHalf(vec.y);

	public ushort z = Mathf.FloatToHalf(vec.z);

	public static explicit operator Vector3(Half3 vec)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(Mathf.HalfToFloat(vec.x), Mathf.HalfToFloat(vec.y), Mathf.HalfToFloat(vec.z));
	}
}

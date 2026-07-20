using Unity.Mathematics;
using UnityEngine;

namespace Rust.Water5;

public struct OceanDisplacementShort3
{
	private const float precision = 20f;

	private const float float2short = 32766f;

	private const float short2float = 3.051944E-05f;

	public short x;

	public short y;

	public short z;

	public static implicit operator Vector3(OceanDisplacementShort3 v)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3
		{
			x = 3.051944E-05f * (float)v.x * 20f,
			y = 3.051944E-05f * (float)v.y * 20f,
			z = 3.051944E-05f * (float)v.z * 20f
		};
	}

	public static implicit operator OceanDisplacementShort3(Vector3 v)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		return new OceanDisplacementShort3
		{
			x = (short)(v.x / 20f * 32766f + 0.5f),
			y = (short)(v.y / 20f * 32766f + 0.5f),
			z = (short)(v.z / 20f * 32766f + 0.5f)
		};
	}

	public static implicit operator OceanDisplacementShort3(float3 v)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		return new OceanDisplacementShort3
		{
			x = (short)(v.x / 20f * 32766f + 0.5f),
			y = (short)(v.y / 20f * 32766f + 0.5f),
			z = (short)(v.z / 20f * 32766f + 0.5f)
		};
	}

	public static implicit operator float3(OceanDisplacementShort3 v)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		return new float3
		{
			x = 3.051944E-05f * (float)v.x * 20f,
			y = 3.051944E-05f * (float)v.y * 20f,
			z = 3.051944E-05f * (float)v.z * 20f
		};
	}
}

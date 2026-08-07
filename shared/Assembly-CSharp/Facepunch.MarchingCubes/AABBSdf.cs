using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace Facepunch.MarchingCubes;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal readonly struct AABBSdf : Facepunch.MarchingCubes.ISdf
{
	public float Distance(in Shape s, float3 p)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return s.AABBDistance(p);
	}

	float Facepunch.MarchingCubes.ISdf.Distance(in Shape s, float3 p)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return Distance(in s, p);
	}
}

using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Facepunch.MarchingCubes;

internal readonly struct EdgeKey
{
	public readonly float3 vertex;

	public readonly int edgeId;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public EdgeKey(int3 cLo, int3 cHi, float sLo, float sHi, float iso, float3 vertexOffset, float scale, int edgeId)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		float num = math.saturate(math.unlerp(sLo, sHi, iso));
		vertex = (math.lerp(float3.op_Implicit(cLo), float3.op_Implicit(cHi), num) - vertexOffset) * scale;
		this.edgeId = edgeId;
	}

	[BurstDiscard]
	public override string ToString()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return $"{edgeId} | {vertex}";
	}
}

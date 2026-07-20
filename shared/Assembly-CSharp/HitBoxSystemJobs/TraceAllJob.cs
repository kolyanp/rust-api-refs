using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace HitBoxSystemJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct TraceAllJob : IJobFor
{
	public ReadOnly<HitboxSystem.HitboxShape.JobStruct> Shapes;

	[WriteOnly]
	public NativeArray<bool> DidHits;

	[WriteOnly]
	public NativeArray<RaycastHit> Hits;

	public Ray ray;

	public float maxDist;

	public float forgiveness;

	public void Execute(int index)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit hit;
		bool flag = Trace(Shapes[index], ray, out hit, forgiveness, maxDist);
		DidHits[index] = flag;
		Hits[index] = hit;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool Trace(in HitboxSystem.HitboxShape.JobStruct shape, Ray ray, out RaycastHit hit, float forgivness = 0f, float maxDistance = float.PositiveInfinity)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		((Ray)(ref ray)).origin = ((Matrix4x4)(ref shape.inverseTransform)).MultiplyPoint3x4(((Ray)(ref ray)).origin);
		((Ray)(ref ray)).direction = ((Matrix4x4)(ref shape.inverseTransform)).MultiplyVector(((Ray)(ref ray)).direction);
		if (shape.type == HitboxDefinition.Type.BOX)
		{
			AABB val = default(AABB);
			((AABB)(ref val))._002Ector(Vector3.zero, shape.size);
			if (!((AABB)(ref val)).Trace(ray, ref hit, forgivness, maxDistance))
			{
				return false;
			}
		}
		else
		{
			Capsule val2 = default(Capsule);
			((Capsule)(ref val2))._002Ector(Vector3.zero, shape.size.x, shape.size.y * 0.5f);
			if (!((Capsule)(ref val2)).Trace(ray, ref hit, forgivness, maxDistance))
			{
				return false;
			}
		}
		((RaycastHit)(ref hit)).point = ((Matrix4x4)(ref shape.transform)).MultiplyPoint3x4(((RaycastHit)(ref hit)).point);
		((RaycastHit)(ref hit)).normal = ((Matrix4x4)(ref shape.transform)).MultiplyVector(((RaycastHit)(ref hit)).normal);
		return true;
	}
}

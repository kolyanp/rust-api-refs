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
		bool flag = Trace(Shapes[index], ray, out var hit, forgiveness, maxDist);
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
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		ray.origin = shape.inverseTransform.MultiplyPoint3x4(ray.origin);
		ray.direction = shape.inverseTransform.MultiplyVector(ray.direction);
		if (shape.type == HitboxDefinition.Type.BOX)
		{
			AABB val = new AABB(Vector3.zero, shape.size);
			if (!val.Trace(ray, ref hit, forgivness, maxDistance))
			{
				return false;
			}
		}
		else
		{
			Capsule val2 = new Capsule(Vector3.zero, shape.size.x, shape.size.y * 0.5f);
			if (!val2.Trace(ray, ref hit, forgivness, maxDistance))
			{
				return false;
			}
		}
		hit.point = shape.transform.MultiplyPoint3x4(hit.point);
		hit.normal = shape.transform.MultiplyVector(hit.normal);
		return true;
	}
}

using System;
using System.Collections.Generic;
using Development.Attributes;
using Facepunch;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[ResetStaticFields]
public class EnvironmentManager : SingletonComponent<EnvironmentManager>
{
	private static ListHashSet<EnvironmentVolume> dynamicVolumes = new ListHashSet<EnvironmentVolume>();

	private static Collider[] check_colliderBuffer = (Collider[])(object)new Collider[32768];

	private void Update()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<EnvironmentVolume> enumerator = dynamicVolumes.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.UpdateVolumeTransformationAndBounds();
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public void RegisterDynamicVolume(EnvironmentVolume volume)
	{
		dynamicVolumes.Add(volume);
	}

	public void UnregisterDynamicVolume(EnvironmentVolume volume)
	{
		dynamicVolumes.Remove(volume);
	}

	public static EnvironmentType Get(OBB obb)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		EnvironmentType environmentType = (EnvironmentType)0;
		List<EnvironmentVolume> list = Pool.Get<List<EnvironmentVolume>>();
		GamePhysics.OverlapOBB<EnvironmentVolume>(obb, list, 262144, (QueryTriggerInteraction)2);
		for (int i = 0; i < list.Count; i++)
		{
			environmentType |= list[i].Type;
		}
		Pool.FreeUnmanaged<EnvironmentVolume>(ref list);
		return environmentType;
	}

	public static EnvironmentType Get(Vector3 pos, ref List<EnvironmentVolume> list, float radius = 0.01f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		EnvironmentType environmentType = (EnvironmentType)0;
		GamePhysics.OverlapSphere<EnvironmentVolume>(pos, radius, list, 262144, (QueryTriggerInteraction)2);
		for (int i = 0; i < list.Count; i++)
		{
			environmentType |= list[i].Type;
		}
		return environmentType;
	}

	public static EnvironmentType Get(Vector3 pos, float radius = 0.01f)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		List<EnvironmentVolume> list = Pool.Get<List<EnvironmentVolume>>();
		EnvironmentType result = Get(pos, ref list, radius);
		Pool.FreeUnmanaged<EnvironmentVolume>(ref list);
		return result;
	}

	public static bool Check(OBB obb, EnvironmentType type)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		int num = GamePhysics.HandleIgnoreCollision(obb.position, 262144);
		int num2 = Physics.OverlapBoxNonAlloc(obb.position, obb.extents, check_colliderBuffer, obb.rotation, num, (QueryTriggerInteraction)2);
		EnvironmentVolume environmentVolume = default(EnvironmentVolume);
		for (int i = 0; i < num2; i++)
		{
			if (((Component)check_colliderBuffer[i]).TryGetComponent<EnvironmentVolume>(ref environmentVolume) && (environmentVolume.Type & type) != 0)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Check(Vector3 pos, EnvironmentType type, float radius = 0.01f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		int num = GamePhysics.HandleIgnoreCollision(pos, 262144);
		int num2 = Physics.OverlapSphereNonAlloc(pos, radius, check_colliderBuffer, num, (QueryTriggerInteraction)2);
		EnvironmentVolume environmentVolume = default(EnvironmentVolume);
		for (int i = 0; i < num2; i++)
		{
			if (((Component)check_colliderBuffer[i]).TryGetComponent<EnvironmentVolume>(ref environmentVolume) && (environmentVolume.Type & type) != 0)
			{
				return true;
			}
		}
		return false;
	}

	public static void Get(ReadOnly<Vector3> positions, ReadOnly<float> radii, ReadOnly<int> layerMasks, NativeArray<EnvironmentType> results, int maxResPerCast, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1, GamePhysics.MasksToValidate validate = GamePhysics.MasksToValidate.All)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GamePhysics.OverlapSpheresEnvironment"))
		{
			NativeArray<ColliderHit> hits = new NativeArray<ColliderHit>(positions.Length * maxResPerCast, (Allocator)3, (NativeArrayOptions)0);
			JobHandle val = GamePhysics.OverlapSpheres(positions, radii, layerMasks, hits, maxResPerCast, triggerInteraction, validate);
			((JobHandle)(ref val)).Complete();
			using (TimeWarning.New("FindComponent"))
			{
				EnvironmentVolume environmentVolume = default(EnvironmentVolume);
				for (int i = 0; i < positions.Length; i++)
				{
					results[i] = (EnvironmentType)0;
					int num = i * maxResPerCast;
					for (int j = 0; j < maxResPerCast; j++)
					{
						ColliderHit val2 = hits[num + j];
						if (((ColliderHit)(ref val2)).instanceID == 0)
						{
							break;
						}
						if (((Component)((ColliderHit)(ref val2)).collider).TryGetComponent<EnvironmentVolume>(ref environmentVolume))
						{
							int num2 = i;
							results[num2] |= environmentVolume.Type;
						}
					}
				}
				hits.Dispose();
			}
		}
	}
}

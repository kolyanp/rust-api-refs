using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UtilityJobs;

public class TerrainCollision : TerrainExtension
{
	public TerrainIgnoreGrid TerrainIgnoreGrid;

	public ListDictionary<Collider, List<Collider>> ignoredColliders;

	public TerrainCollider terrainCollider;

	public const float IgnoreRadius = 0.01f;

	public override void Setup()
	{
		ignoredColliders = new ListDictionary<Collider, List<Collider>>();
		terrainCollider = terrainRenderer.gameObject.GetComponent<TerrainCollider>();
		TerrainIgnoreGrid = new TerrainIgnoreGrid();
	}

	private void OnDestroy()
	{
		TerrainIgnoreGrid.Dispose();
	}

	public void Clear()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)terrainCollider))
		{
			return;
		}
		Enumerator<Collider> enumerator = ignoredColliders.Keys.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Physics.IgnoreCollision(enumerator.Current, (Collider)(object)terrainCollider, false);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		ignoredColliders.Clear();
	}

	public void Reset(Collider collider)
	{
		if (Object.op_Implicit((Object)(object)terrainCollider) && Object.op_Implicit((Object)(object)collider))
		{
			Physics.IgnoreCollision(collider, (Collider)(object)terrainCollider, false);
			ignoredColliders.Remove(collider);
		}
	}

	public bool GetIgnore(Vector3 pos, float radius = 0.01f)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("TerrainCollision.GetIgnore"))
		{
			if (TerrainIgnoreGrid != null && !TerrainIgnoreGrid.Check(pos, radius))
			{
				return false;
			}
			return GamePhysics.CheckSphere<TerrainCollisionTrigger>(pos, radius, 262144, (QueryTriggerInteraction)2);
		}
	}

	public void GetIgnore(ReadOnly<Vector3> positions, ReadOnly<float> radii, NativeArray<bool> results)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("TerrainCollision.GetIgnore"))
		{
			FillJob<bool> fillJob = new FillJob<bool>
			{
				Values = results,
				Value = false
			};
			IJobExtensions.RunByRef<FillJob<bool>>(ref fillJob);
			NativeList<int> val = new NativeList<int>(positions.Length, AllocatorHandle.op_Implicit((Allocator)3));
			JobHandle val2 = ((TerrainIgnoreGrid == null) ? IJobExtensions.Schedule<GenerateAscSeqListJob>(new GenerateAscSeqListJob
			{
				Values = val,
				Start = 0,
				Step = 1,
				Count = positions.Length
			}, default(JobHandle)) : TerrainIgnoreGrid.Check(positions, radii, val));
			((JobHandle)(ref val2)).Complete();
			if (!val.IsEmpty)
			{
				NativeArray<Vector3> results2 = default(NativeArray<Vector3>);
				results2._002Ector(val.Length, (Allocator)3, (NativeArrayOptions)0);
				GatherJob<Vector3> gatherJob = new GatherJob<Vector3>
				{
					Results = results2,
					Source = positions,
					Indices = val.AsReadOnly()
				};
				IJobExtensions.RunByRef<GatherJob<Vector3>>(ref gatherJob);
				NativeArray<float> results3 = default(NativeArray<float>);
				results3._002Ector(val.Length, (Allocator)3, (NativeArrayOptions)0);
				GatherJob<float> gatherJob2 = new GatherJob<float>
				{
					Results = results3,
					Source = radii,
					Indices = val.AsReadOnly()
				};
				IJobExtensions.RunByRef<GatherJob<float>>(ref gatherJob2);
				NativeArray<int> values = default(NativeArray<int>);
				values._002Ector(val.Length, (Allocator)3, (NativeArrayOptions)0);
				FillJob<int> fillJob2 = new FillJob<int>
				{
					Values = values,
					Value = 262144
				};
				IJobExtensions.RunByRef<FillJob<int>>(ref fillJob2);
				new QueryParameters(262144, false, (QueryTriggerInteraction)2, false);
				GamePhysics.CheckSpheres<TerrainCollisionTrigger>(results2.AsReadOnly(), results3.AsReadOnly(), values.AsReadOnly(), results, GamePhysics.DefaultMaxResultsPerQuery, (QueryTriggerInteraction)2, GamePhysics.MasksToValidate.None);
				Span<bool> values2 = NativeArray<bool>.op_Implicit(ref results);
				ReadOnly<int> val3 = val.AsReadOnly();
				CollectionUtil.ScatterOutInplace(values2, ReadOnly<int>.op_Implicit(ref val3), defValue: false);
				values.Dispose();
				results3.Dispose();
				results2.Dispose();
			}
			val.Dispose();
		}
	}

	public bool GetIgnore(RaycastHit hit)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("TerrainCollision.GetIgnore"))
		{
			if (!(((RaycastHit)(ref hit)).collider is TerrainCollider))
			{
				return false;
			}
			if (!TerrainIgnoreGrid.Check(((RaycastHit)(ref hit)).point))
			{
				return false;
			}
			return ((RaycastHit)(ref hit)).collider is TerrainCollider && GetIgnore(((RaycastHit)(ref hit)).point);
		}
	}

	public bool GetIgnore(Collider collider)
	{
		if (!Object.op_Implicit((Object)(object)terrainCollider) || !Object.op_Implicit((Object)(object)collider))
		{
			return false;
		}
		return ignoredColliders.Contains(collider);
	}

	public void SetIgnore(Collider collider, Collider trigger, bool ignore = true)
	{
		if (!Object.op_Implicit((Object)(object)terrainCollider) || !Object.op_Implicit((Object)(object)collider))
		{
			return;
		}
		if (!GetIgnore(collider))
		{
			if (ignore)
			{
				List<Collider> list = new List<Collider> { trigger };
				Physics.IgnoreCollision(collider, (Collider)(object)terrainCollider, true);
				ignoredColliders.Add(collider, list);
			}
			return;
		}
		List<Collider> list2 = ignoredColliders[collider];
		if (ignore)
		{
			if (!list2.Contains(trigger))
			{
				list2.Add(trigger);
			}
		}
		else if (list2.Contains(trigger))
		{
			list2.Remove(trigger);
		}
	}

	protected void LateUpdate()
	{
		if (ignoredColliders == null)
		{
			return;
		}
		for (int i = 0; i < ignoredColliders.Count; i++)
		{
			KeyValuePair<Collider, List<Collider>> byIndex = ignoredColliders.GetByIndex(i);
			Collider key = byIndex.Key;
			List<Collider> value = byIndex.Value;
			if ((Object)(object)key == (Object)null)
			{
				ignoredColliders.RemoveAt(i--);
			}
			else if (value.Count == 0)
			{
				Physics.IgnoreCollision(key, (Collider)(object)terrainCollider, false);
				ignoredColliders.RemoveAt(i--);
			}
		}
	}
}

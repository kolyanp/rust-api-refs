using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UtilityJobs;

public class WaterCollision : MonoBehaviour
{
	private ListDictionary<Collider, List<Collider>> ignoredColliders;

	private HashSet<Collider> waterColliders;

	private WaterVisibilityGrid visibilityGrid;

	public const float IgnoreRadius = 0.01f;

	private NativeList<int> indicesToCheck;

	public WaterVisibilityGrid VisibilityGrid => visibilityGrid;

	public void Setup()
	{
		ignoredColliders = new ListDictionary<Collider, List<Collider>>();
		waterColliders = new HashSet<Collider>();
		if (visibilityGrid != null)
		{
			visibilityGrid.Dispose();
		}
		visibilityGrid = new WaterVisibilityGrid();
	}

	private void OnDestroy()
	{
		visibilityGrid?.Dispose();
		NativeListEx.SafeDispose(ref indicesToCheck);
	}

	public void Clear()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (waterColliders.Count == 0)
		{
			return;
		}
		HashSet<Collider>.Enumerator enumerator = waterColliders.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Enumerator<Collider> enumerator2 = ignoredColliders.Keys.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					Physics.IgnoreCollision(enumerator2.Current, enumerator.Current, false);
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
			}
		}
		ignoredColliders.Clear();
	}

	public void Reset(Collider collider)
	{
		if (waterColliders.Count != 0 && Object.op_Implicit((Object)(object)collider))
		{
			HashSet<Collider>.Enumerator enumerator = waterColliders.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Physics.IgnoreCollision(collider, enumerator.Current, false);
			}
			ignoredColliders.Remove(collider);
		}
	}

	public bool GetIgnore(Vector3 pos, float radius = 0.01f)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		WaterVisibilityGrid waterVisibilityGrid = visibilityGrid;
		if (waterVisibilityGrid != null && !waterVisibilityGrid.Check(pos, radius))
		{
			return false;
		}
		return GamePhysics.CheckSphere<WaterVisibilityTrigger>(pos, radius, 262144, (QueryTriggerInteraction)2);
	}

	private void PrepareIndiciesToCheckList(int length)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!indicesToCheck.IsCreated)
		{
			indicesToCheck = new NativeList<int>(length, AllocatorHandle.op_Implicit((Allocator)4));
			return;
		}
		if (length > indicesToCheck.Capacity)
		{
			indicesToCheck.Capacity = length;
		}
		indicesToCheck.Clear();
	}

	public void GetIgnore(ReadOnly<Vector3> positions, ReadOnly<float> radii, NativeArray<bool> results)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("WaterCollision.GetIgnore"))
		{
			FillJob<bool> fillJob = new FillJob<bool>
			{
				Values = results,
				Value = false
			};
			IJobExtensions.RunByRef<FillJob<bool>>(ref fillJob);
			PrepareIndiciesToCheckList(positions.Length);
			JobHandle val = ((visibilityGrid == null) ? IJobExtensions.Schedule<GenerateAscSeqListJob>(new GenerateAscSeqListJob
			{
				Values = indicesToCheck,
				Start = 0,
				Step = 1,
				Count = positions.Length
			}, default(JobHandle)) : visibilityGrid.Check(positions, radii, indicesToCheck));
			((JobHandle)(ref val)).Complete();
			if (!indicesToCheck.IsEmpty)
			{
				NativeArray<Vector3> results2 = default(NativeArray<Vector3>);
				results2._002Ector(indicesToCheck.Length, (Allocator)3, (NativeArrayOptions)0);
				GatherJob<Vector3> gatherJob = new GatherJob<Vector3>
				{
					Results = results2,
					Source = positions,
					Indices = indicesToCheck.AsReadOnly()
				};
				IJobExtensions.RunByRef<GatherJob<Vector3>>(ref gatherJob);
				NativeArray<float> results3 = default(NativeArray<float>);
				results3._002Ector(indicesToCheck.Length, (Allocator)3, (NativeArrayOptions)0);
				GatherJob<float> gatherJob2 = new GatherJob<float>
				{
					Results = results3,
					Source = radii,
					Indices = indicesToCheck.AsReadOnly()
				};
				IJobExtensions.RunByRef<GatherJob<float>>(ref gatherJob2);
				NativeArray<int> values = default(NativeArray<int>);
				values._002Ector(indicesToCheck.Length, (Allocator)3, (NativeArrayOptions)0);
				FillJob<int> fillJob2 = new FillJob<int>
				{
					Values = values,
					Value = 262144
				};
				IJobExtensions.RunByRef<FillJob<int>>(ref fillJob2);
				GamePhysics.CheckSpheres<WaterVisibilityTrigger>(results2.AsReadOnly(), results3.AsReadOnly(), values.AsReadOnly(), NativeArray<bool>.op_Implicit(ref results), GamePhysics.DefaultMaxResultsPerQuery, (QueryTriggerInteraction)2, GamePhysics.MasksToValidate.None);
				Span<bool> values2 = NativeArray<bool>.op_Implicit(ref results);
				ReadOnly<int> val2 = indicesToCheck.AsReadOnly();
				CollectionUtil.ScatterOutInplace(values2, ReadOnly<int>.op_Implicit(ref val2), defValue: false);
				values.Dispose();
				results3.Dispose();
				results2.Dispose();
			}
		}
	}

	public void GetIgnoreIndirect(ReadOnly<Vector3> pos, ReadOnly<float> radii, ReadOnly<int> indices, NativeArray<bool> results)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("WaterCollision.GetIgnoreIndirect"))
		{
			FillJob<bool> fillJob = new FillJob<bool>
			{
				Values = results,
				Value = false
			};
			IJobExtensions.RunByRef<FillJob<bool>>(ref fillJob);
			PrepareIndiciesToCheckList(indices.Length);
			JobHandle val = default(JobHandle);
			if (visibilityGrid != null)
			{
				val = visibilityGrid.CheckIndirect(pos, radii, indices, indicesToCheck);
			}
			else
			{
				indicesToCheck.CopyFrom(in indices);
			}
			((JobHandle)(ref val)).Complete();
			if (!indicesToCheck.IsEmpty)
			{
				NativeArray<Vector3> results2 = default(NativeArray<Vector3>);
				results2._002Ector(indicesToCheck.Length, (Allocator)3, (NativeArrayOptions)0);
				GatherJob<Vector3> gatherJob = new GatherJob<Vector3>
				{
					Results = results2,
					Source = pos,
					Indices = indicesToCheck.AsReadOnly()
				};
				IJobExtensions.RunByRef<GatherJob<Vector3>>(ref gatherJob);
				NativeArray<float> results3 = default(NativeArray<float>);
				results3._002Ector(indicesToCheck.Length, (Allocator)3, (NativeArrayOptions)0);
				GatherJob<float> gatherJob2 = new GatherJob<float>
				{
					Results = results3,
					Source = radii,
					Indices = indicesToCheck.AsReadOnly()
				};
				IJobExtensions.RunByRef<GatherJob<float>>(ref gatherJob2);
				NativeArray<int> values = default(NativeArray<int>);
				values._002Ector(indicesToCheck.Length, (Allocator)3, (NativeArrayOptions)0);
				FillJob<int> fillJob2 = new FillJob<int>
				{
					Values = values,
					Value = 262144
				};
				IJobExtensions.RunByRef<FillJob<int>>(ref fillJob2);
				NativeArray<bool> val2 = default(NativeArray<bool>);
				val2._002Ector(indicesToCheck.Length, (Allocator)3, (NativeArrayOptions)0);
				GamePhysics.CheckSpheres<WaterVisibilityTrigger>(results2.AsReadOnly(), results3.AsReadOnly(), values.AsReadOnly(), NativeArray<bool>.op_Implicit(ref val2), GamePhysics.DefaultMaxResultsPerQuery, (QueryTriggerInteraction)2, GamePhysics.MasksToValidate.None);
				ReadOnlySpan<bool> readOnlySpan = NativeArray<bool>.op_Implicit(ref val2);
				Span<bool> to = NativeArray<bool>.op_Implicit(ref results);
				ReadOnly<int> val3 = indicesToCheck.AsReadOnly();
				CollectionUtil.ScatterTo(readOnlySpan, to, ReadOnly<int>.op_Implicit(ref val3));
				val2.Dispose();
				values.Dispose();
				results3.Dispose();
				results2.Dispose();
			}
		}
	}

	public bool GetIgnore(Bounds bounds)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		WaterVisibilityGrid waterVisibilityGrid = visibilityGrid;
		if (waterVisibilityGrid != null && !waterVisibilityGrid.Check(bounds))
		{
			return false;
		}
		return GamePhysics.CheckBounds<WaterVisibilityTrigger>(bounds, 262144, (QueryTriggerInteraction)2);
	}

	public bool GetIgnore(Vector3 start, Vector3 end, float radius)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		WaterVisibilityGrid waterVisibilityGrid = visibilityGrid;
		if (waterVisibilityGrid != null && !waterVisibilityGrid.Check(start, end, radius))
		{
			return false;
		}
		return GamePhysics.CheckCapsule<WaterVisibilityTrigger>(start, end, radius, 262144, (QueryTriggerInteraction)2);
	}

	public void GetIgnoreIndirect(ReadOnly<Vector3> starts, ReadOnly<Vector3> ends, ReadOnly<float> radii, ReadOnly<int> indices, NativeArray<bool> results)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("WaterCollision.GetIgnoreIndirect"))
		{
			FillJob<bool> fillJob = new FillJob<bool>
			{
				Values = results,
				Value = false
			};
			IJobExtensions.RunByRef<FillJob<bool>>(ref fillJob);
			PrepareIndiciesToCheckList(indices.Length);
			JobHandle val = default(JobHandle);
			if (visibilityGrid != null)
			{
				val = visibilityGrid.CheckIndirect(starts, ends, radii, indices, indicesToCheck);
			}
			else
			{
				indicesToCheck.CopyFrom(in indices);
			}
			((JobHandle)(ref val)).Complete();
			if (!indicesToCheck.IsEmpty)
			{
				NativeArray<Vector3> results2 = default(NativeArray<Vector3>);
				results2._002Ector(indicesToCheck.Length, (Allocator)3, (NativeArrayOptions)0);
				GatherJob<Vector3> gatherJob = new GatherJob<Vector3>
				{
					Results = results2,
					Source = starts,
					Indices = indicesToCheck.AsReadOnly()
				};
				IJobExtensions.RunByRef<GatherJob<Vector3>>(ref gatherJob);
				NativeArray<Vector3> results3 = default(NativeArray<Vector3>);
				results3._002Ector(indicesToCheck.Length, (Allocator)3, (NativeArrayOptions)0);
				GatherJob<Vector3> gatherJob2 = new GatherJob<Vector3>
				{
					Results = results3,
					Source = ends,
					Indices = indicesToCheck.AsReadOnly()
				};
				IJobExtensions.RunByRef<GatherJob<Vector3>>(ref gatherJob2);
				NativeArray<float> results4 = default(NativeArray<float>);
				results4._002Ector(indicesToCheck.Length, (Allocator)3, (NativeArrayOptions)0);
				GatherJob<float> gatherJob3 = new GatherJob<float>
				{
					Results = results4,
					Source = radii,
					Indices = indicesToCheck.AsReadOnly()
				};
				IJobExtensions.RunByRef<GatherJob<float>>(ref gatherJob3);
				NativeArray<int> values = default(NativeArray<int>);
				values._002Ector(indicesToCheck.Length, (Allocator)3, (NativeArrayOptions)0);
				FillJob<int> fillJob2 = new FillJob<int>
				{
					Values = values,
					Value = 262144
				};
				IJobExtensions.RunByRef<FillJob<int>>(ref fillJob2);
				NativeArray<bool> val2 = default(NativeArray<bool>);
				val2._002Ector(indicesToCheck.Length, (Allocator)3, (NativeArrayOptions)0);
				GamePhysics.CheckCapsules<WaterVisibilityTrigger>(results2.AsReadOnly(), results3.AsReadOnly(), results4.AsReadOnly(), values.AsReadOnly(), NativeArray<bool>.op_Implicit(ref val2), GamePhysics.DefaultMaxResultsPerQuery, (QueryTriggerInteraction)2, GamePhysics.MasksToValidate.None, true);
				ReadOnlySpan<bool> readOnlySpan = NativeArray<bool>.op_Implicit(ref val2);
				Span<bool> to = NativeArray<bool>.op_Implicit(ref results);
				ReadOnly<int> val3 = indicesToCheck.AsReadOnly();
				CollectionUtil.ScatterTo(readOnlySpan, to, ReadOnly<int>.op_Implicit(ref val3));
				val2.Dispose();
				values.Dispose();
				results4.Dispose();
				results3.Dispose();
				results2.Dispose();
			}
		}
	}

	public bool GetIgnore(RaycastHit hit)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (waterColliders.Contains(((RaycastHit)(ref hit)).collider))
		{
			return GetIgnore(((RaycastHit)(ref hit)).point);
		}
		return false;
	}

	public bool GetIgnore(Collider collider)
	{
		if (waterColliders.Count == 0 || !Object.op_Implicit((Object)(object)collider))
		{
			return false;
		}
		return ignoredColliders.Contains(collider);
	}

	public void SetIgnore(Collider collider, Collider trigger, bool ignore = true)
	{
		if (waterColliders.Count == 0 || !Object.op_Implicit((Object)(object)collider))
		{
			return;
		}
		if (!GetIgnore(collider))
		{
			if (ignore)
			{
				List<Collider> list = new List<Collider> { trigger };
				HashSet<Collider>.Enumerator enumerator = waterColliders.GetEnumerator();
				while (enumerator.MoveNext())
				{
					Physics.IgnoreCollision(collider, enumerator.Current, true);
				}
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
				HashSet<Collider>.Enumerator enumerator = waterColliders.GetEnumerator();
				while (enumerator.MoveNext())
				{
					Physics.IgnoreCollision(key, enumerator.Current, false);
				}
				ignoredColliders.RemoveAt(i--);
			}
		}
	}
}

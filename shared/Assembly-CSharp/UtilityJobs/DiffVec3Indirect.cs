using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace UtilityJobs;

[BurstCompile]
public struct DiffVec3Indirect : IJob
{
	public NativeList<int> FoundDiff;

	public ReadOnly<Vector3> A;

	public ReadOnly<Vector3> B;

	public ReadOnly<int> Indices;

	public void Execute()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<int> enumerator = Indices.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				if (A[current] != B[current])
				{
					FoundDiff.AddNoResize(current);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}
}

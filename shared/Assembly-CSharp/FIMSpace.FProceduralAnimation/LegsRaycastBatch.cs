using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Profiling;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation;

internal static class LegsRaycastBatch
{
	private const int MinCommandsPerJob = 16;

	private static readonly ProfilerMarker castMarker = new ProfilerMarker("LegsRaycastBatch.Cast");

	private static readonly List<RaycastCommand> queuedRays = new List<RaycastCommand>();

	private static readonly List<LegsAnimator.Leg> queuedRayLegs = new List<LegsAnimator.Leg>();

	private static int queuedFrame = -1;

	public static void QueueLinecast(LegsAnimator.Leg leg, Vector3 origin, Vector3 rayEnd, int layerMask, QueryTriggerInteraction triggerInteraction)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		CastLastFrameRays();
		Vector3 val = rayEnd - origin;
		float magnitude = val.magnitude;
		QueryParameters val2 = new QueryParameters(layerMask, false, triggerInteraction, Physics.queriesHitBackfaces);
		queuedRays.Add(new RaycastCommand(origin, val / magnitude, val2, magnitude));
		queuedRayLegs.Add(leg);
	}

	public static void CastLastFrameRays()
	{
		int frameCount = Time.frameCount;
		if (queuedFrame != frameCount)
		{
			if (queuedFrame == frameCount - 1 && queuedRays.Count > 0)
			{
				Cast(queuedFrame);
			}
			queuedRays.Clear();
			queuedRayLegs.Clear();
			queuedFrame = frameCount;
		}
	}

	private static void Cast(int rayFrame)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		ProfilerMarker val = castMarker;
		AutoScope val2 = val.Auto();
		try
		{
			int count = queuedRays.Count;
			NativeArray<RaycastCommand> val3 = new NativeArray<RaycastCommand>(count, (Allocator)3, (NativeArrayOptions)0);
			NativeArray<RaycastHit> val4 = new NativeArray<RaycastHit>(count, (Allocator)3, (NativeArrayOptions)0);
			for (int i = 0; i < count; i++)
			{
				val3[i] = queuedRays[i];
			}
			if (Physics.autoSyncTransforms)
			{
				Physics.SyncTransforms();
			}
			NativeArray<RaycastCommand> val5 = val3;
			NativeArray<RaycastHit> val6 = val4;
			JobHandle val7 = default;
			val7 = RaycastCommand.ScheduleBatch(val5, val6, 16, val7);
			val7.Complete();
			for (int j = 0; j < count; j++)
			{
				queuedRayLegs[j].SetBatchedLinecastHit(rayFrame, val4[j]);
			}
			val3.Dispose();
			val4.Dispose();
		}
		finally
		{
			((IDisposable)val2/*cast due to constrained. prefix*/).Dispose();
		}
	}

	static LegsRaycastBatch()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
	}
}

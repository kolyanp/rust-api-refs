using System;
using Unity.Collections;
using UnityEngine;

public struct WaterVolumeBurstData : IEquatable<WaterVolumeBurstData>, IDisposable
{
	public OBB bounds;

	public NativeArray<Matrix4x4> cutOffPlaneMatrices;

	public NativeArray<Pose> cutOffPlanePoses;

	public bool naturalSource;

	public bool Equals(WaterVolumeBurstData other)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (((OBB)(ref bounds)).Equals(other.bounds) && cutOffPlaneMatrices.Equals(other.cutOffPlaneMatrices))
		{
			return naturalSource == other.naturalSource;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is WaterVolumeBurstData other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return HashCode.Combine<OBB, NativeArray<Matrix4x4>, bool>(bounds, cutOffPlaneMatrices, naturalSource);
	}

	public void Dispose()
	{
		cutOffPlaneMatrices.SafeDispose<Matrix4x4>();
		cutOffPlanePoses.SafeDispose<Pose>();
	}
}

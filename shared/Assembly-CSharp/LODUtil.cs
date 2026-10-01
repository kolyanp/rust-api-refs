using System;
using UnityEngine;

public static class LODUtil
{
	public const float DefaultDistance = 1000f;

	public const float FarCameraDistance = 30f;

	public const float DefaultFieldOfView = 75f;

	public static float GetDistance(Transform transform, LODDistanceMode mode = LODDistanceMode.XYZ)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return GetDistance(transform.position, mode);
	}

	public static float GetDistance(Vector3 meshPos, LODDistanceMode mode = LODDistanceMode.XYZ)
	{
		return 1000f;
	}

	public static float GetDistanceFrom(Vector3 cameraPos, Vector3 meshPos, LODDistanceMode mode = LODDistanceMode.XYZ)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return GetDistanceInternal(cameraPos, meshPos, mode);
	}

	private static float GetDistanceInternal(Vector3 cameraPos, Vector3 worldPos, LODDistanceMode mode)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		return mode switch
		{
			LODDistanceMode.XYZ => Vector3.Distance(cameraPos, worldPos), 
			LODDistanceMode.XZ => Vector3Ex.Distance2D(cameraPos, worldPos), 
			LODDistanceMode.Y => Mathf.Abs(cameraPos.y - worldPos.y), 
			_ => 1000f, 
		};
	}

	public static float VerifyDistance(float distance)
	{
		return Mathf.Min(500f, distance);
	}

	public static float RelativeHeightToDistance(float relativeHeight, float worldSize, float fieldOfView = 75f)
	{
		if (relativeHeight <= 0f)
		{
			return float.PositiveInfinity;
		}
		float num = Mathf.Tan(MathF.PI / 180f * fieldOfView * 0.5f);
		return worldSize * 0.5f / (relativeHeight * num);
	}

	public static float GetWorldSpaceSize(LODGroup group)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Vector3 lossyScale = ((Component)group).transform.lossyScale;
		float num = Mathf.Max(new float[3]
		{
			Mathf.Abs(lossyScale.x),
			Mathf.Abs(lossyScale.y),
			Mathf.Abs(lossyScale.z)
		});
		return group.size * num;
	}

	public static LODEnvironmentMode DetermineEnvironmentMode(Transform transform)
	{
		if (((Component)transform).CompareTag("OnlyVisibleUnderground") || ((Component)transform.root).CompareTag("OnlyVisibleUnderground"))
		{
			return LODEnvironmentMode.Underground;
		}
		return LODEnvironmentMode.Default;
	}
}

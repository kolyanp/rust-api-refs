using ProtoBuf;
using UnityEngine;

public struct CrashTargeting
{
	public Vector3 center;

	public float radius;

	public Vector3 finalCrashPos;

	public float finalCrashRadius;

	public bool isDescending;

	public float cooldownEndTime;

	public float descentEndTime;

	public static CrashTargeting FromProto(SatelliteControlComputer msg, float now)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		return new CrashTargeting
		{
			center = msg.targetingCenter,
			radius = msg.targetingRadius,
			finalCrashPos = msg.finalCrashPos,
			finalCrashRadius = msg.finalCrashRadius,
			isDescending = msg.isDescending,
			cooldownEndTime = ((msg.cooldownRemaining > 0f) ? (now + msg.cooldownRemaining) : 0f),
			descentEndTime = ((msg.descentRemaining > 0f) ? (now + msg.descentRemaining) : 0f)
		};
	}
}

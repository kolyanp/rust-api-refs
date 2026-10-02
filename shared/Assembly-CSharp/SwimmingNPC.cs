using System;
using ConVar;
using Rust.Ai.Gen2;
using UnityEngine;

public abstract class SwimmingNPC : BaseNPC2
{
	public class SwimWorkQueue : PersistentObjectWorkQueue<SwimmingNPC>
	{
		protected override void RunJob(SwimmingNPC swimmer)
		{
			if (((PersistentObjectWorkQueue<SwimmingNPC>)this).ShouldAdd(swimmer))
			{
				swimmer.ServerSwim();
			}
		}

		protected override bool ShouldAdd(SwimmingNPC swimmer)
		{
			if ((Object)(object)swimmer != (Object)null)
			{
				return !swimmer.IsDead();
			}
			return false;
		}
	}

	public const Flags Deactivated = Flags.Reserved12;

	public float minSpeed;

	public float maxSpeed;

	public float minTurnSpeed = 0.25f;

	public float maxTurnSpeed = 2f;

	public float obstacleDetectionRadius = 1f;

	[NonSerialized]
	public Vector3 destination;

	private float currentSpeed;

	public static SwimWorkQueue swimQueue = new SwimWorkQueue();

	private const float MaxSwimDelta = 0.25f;

	private TimeSince lastSwimTick;

	private float minFloorDist = 0.2f;

	private float minSurfaceDist = 0.2f;

	public const float MinSwimmableDepth = 0.2f;

	private Vector3 cachedObstacleNormal;

	protected float obstacleAvoidanceScale;

	private float obstacleDetectionRange = 5f;

	private float timeSinceLastObstacleCheck;

	public override bool MovesOnNavmesh => false;

	public static float swimFrameBudgetMs => AI.ocean_critters_movement_frametime;

	protected virtual float ForceSurfaceAmount => 0f;

	public bool IsDeactivated()
	{
		return HasFlag(Flags.Reserved12);
	}

	public override void ServerInit()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		((Component)this).transform.position = WaterClamp(((Component)this).transform.position);
		destination = ((Component)this).transform.position;
		lastSwimTick = TimeSince.op_Implicit(0f);
		((PersistentObjectWorkQueue<SwimmingNPC>)swimQueue).Add(this);
	}

	internal override void DoServerDestroy()
	{
		((PersistentObjectWorkQueue<SwimmingNPC>)swimQueue).Remove(this);
		base.DoServerDestroy();
	}

	private void ServerSwim()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!IsDeactivated())
		{
			float delta = Mathf.Min(TimeSince.op_Implicit(lastSwimTick), 0.25f);
			lastSwimTick = TimeSince.op_Implicit(0f);
			UpdateObstacleAvoidance(delta);
			UpdateDirection(delta);
			UpdateSpeed(delta);
			UpdatePosition(delta);
		}
	}

	public void SetDeactivatedFlag(bool toggle)
	{
		SetNetworkedFlag(Flags.Reserved12, toggle);
	}

	protected void SetNetworkedFlag(Flags flag, bool value)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(flag, value);
	}

	protected virtual float GetDesiredSpeed()
	{
		return minSpeed;
	}

	public virtual float GetTurnSpeed()
	{
		if (obstacleAvoidanceScale != 0f)
		{
			return Mathf.Lerp(minTurnSpeed, maxTurnSpeed, obstacleAvoidanceScale);
		}
		return minTurnSpeed;
	}

	private float GetCurrentSpeed()
	{
		return currentSpeed;
	}

	public static bool CanSwimAt(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return WaterLevel.GetOverallWaterDepth(point, waves: false, volumes: false) >= 0.2f;
	}

	public Vector3 WaterClamp(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		(float, float) waterAndTerrainSurface = WaterLevel.GetWaterAndTerrainSurface(point, waves: false, volumes: false);
		float item = waterAndTerrainSurface.Item1;
		float num = waterAndTerrainSurface.Item2 + minFloorDist;
		float num2 = item - minSurfaceDist;
		float forceSurfaceAmount = ForceSurfaceAmount;
		if (forceSurfaceAmount != 0f)
		{
			item = WaterLevel.GetWaterSurface(point, waves: false, volumes: false);
			num = (num2 = item + forceSurfaceAmount);
		}
		if (num > num2)
		{
			num = num2;
		}
		point.y = Mathf.Clamp(point.y, num, num2);
		return point;
	}

	private void UpdateObstacleAvoidance(float delta)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		timeSinceLastObstacleCheck += delta;
		if (timeSinceLastObstacleCheck < 0.5f)
		{
			return;
		}
		Vector3 forward = ((Component)this).transform.forward;
		Vector3 position = ((Component)this).transform.position;
		int num = 1503764737;
		RaycastHit val = default;
		if (Physics.SphereCast(position, obstacleDetectionRadius, forward, ref val, obstacleDetectionRange, num))
		{
			Vector3 point = val.point;
			Vector3 val2 = Vector3.zero;
			Vector3 val3 = Vector3.zero;
			RaycastHit val4 = default;
			if (Physics.SphereCast(position + Vector3.down * 0.25f + ((Component)this).transform.right * 0.25f, obstacleDetectionRadius, forward, ref val4, obstacleDetectionRange, num))
			{
				val2 = val4.point;
			}
			RaycastHit val5 = default;
			if (Physics.SphereCast(position + Vector3.down * 0.25f - ((Component)this).transform.right * 0.25f, obstacleDetectionRadius, forward, ref val5, obstacleDetectionRange, num))
			{
				val3 = val5.point;
			}
			if (val2 != Vector3.zero && val3 != Vector3.zero)
			{
				Plane val6 = new Plane(point, val2, val3);
				Vector3 normal = val6.normal;
				if (normal != Vector3.zero)
				{
					val.normal = normal;
				}
			}
			cachedObstacleNormal = val.normal;
			obstacleAvoidanceScale = 1f - Mathf.InverseLerp(2f, obstacleDetectionRange * 0.75f, val.distance);
		}
		else
		{
			obstacleAvoidanceScale = Mathf.MoveTowards(obstacleAvoidanceScale, 0f, timeSinceLastObstacleCheck * 2f);
		}
		timeSinceLastObstacleCheck = 0f;
	}

	private void UpdateDirection(float delta)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		_ = ((Component)this).transform.forward;
		Vector3 val = Vector3Ex.Direction(WaterClamp(destination), ((Component)this).transform.position);
		if (obstacleAvoidanceScale != 0f)
		{
			Vector3 val3;
			if (cachedObstacleNormal != Vector3.zero)
			{
				Vector3 val2 = QuaternionEx.LookRotationForcedUp(cachedObstacleNormal, Vector3.up) * Vector3.forward;
				val3 = ((!(Vector3.Dot(val2, ((Component)this).transform.right) > Vector3.Dot(val2, -((Component)this).transform.right))) ? (-((Component)this).transform.right) : ((Component)this).transform.right);
			}
			else
			{
				val3 = ((Component)this).transform.right;
			}
			val = val3 * obstacleAvoidanceScale;
			val.Normalize();
		}
		if (val != Vector3.zero)
		{
			Quaternion val4 = Quaternion.LookRotation(val, Vector3.up);
			((Component)this).transform.rotation = Quaternion.Lerp(((Component)this).transform.rotation, val4, delta * GetTurnSpeed());
		}
	}

	private void UpdatePosition(float delta)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = ((Component)this).transform.forward;
		Vector3 point = ((Component)this).transform.position + forward * GetCurrentSpeed() * delta;
		if (CanSwimAt(point))
		{
			((Component)this).transform.position = WaterClamp(point);
		}
	}

	private void UpdateSpeed(float delta)
	{
		currentSpeed = Mathf.Lerp(currentSpeed, GetDesiredSpeed(), delta * 4f);
	}
}

using System;
using UnityEngine;

public class JunkPileWater : JunkPile, IBudgetedFloatingEntity, IDestroyableOnPlayerBoatCollision
{
	public class JunkpileWaterWorkQueue : PersistentObjectWorkQueue<IBudgetedFloatingEntity>
	{
		protected override void RunJob(IBudgetedFloatingEntity entity)
		{
			if (((PersistentObjectWorkQueue<IBudgetedFloatingEntity>)this).ShouldAdd(entity))
			{
				entity.UpdateNearbyPlayers();
			}
		}

		protected override bool ShouldAdd(IBudgetedFloatingEntity entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.Entity.IsValid();
			}
			return false;
		}
	}

	public static JunkpileWaterWorkQueue junkpileWaterWorkQueue = new JunkpileWaterWorkQueue();

	[Help("How many milliseconds to budget for processing junk pile updates per frame")]
	[ServerVar]
	public static float framebudgetms = 0.05f;

	public Transform[] buoyancyPoints;

	public bool debugDraw;

	public float updateCullRange;

	public float VehicleCheckRadius;

	public Rigidbody Body;

	[Range(0f, 1f)]
	public float buoyancyAmplitude;

	[ServerVar]
	public static bool DestroyableByPlayerBoats = true;

	[ServerVar]
	public static float MinimumPlayerBoatMassToBeDestroyed = 2000f;

	[ServerVar]
	public static float MinimumPlayerBoatVelocityToBeDestroyed = 5f;

	private Action updateMovementFixedTick;

	private Quaternion baseRotation;

	private bool first;

	private bool hasPlayersNearby;

	private TimeUntil nextPlayerCheck;

	public BaseEntity Entity => this;

	public override void Spawn()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		position.y = WaterLevel.GetWaterSurface(((Component)this).transform.position, waves: false, volumes: false);
		((Component)this).transform.position = position;
		base.Spawn();
		Quaternion rotation = ((Component)this).transform.rotation;
		baseRotation = Quaternion.Euler(0f, ((Quaternion)(ref rotation)).eulerAngles.y, 0f);
		if (Physics.CheckSphere(((Component)this).transform.position, VehicleCheckRadius, 134217728))
		{
			Kill();
		}
		else
		{
			KillIfInMonument();
		}
	}

	private void KillIfInMonument()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TerrainMeta.TopologyMap != (Object)null && (TerrainMeta.TopologyMap.GetTopology(((Component)this).transform.position) & 0x400) != 0 && (Object)(object)TerrainMeta.Path != (Object)null)
		{
			Kill();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRandomized(EnqueueNearPlayersCheck, 0f, 0.75f, 0.25f);
	}

	public void EnqueueNearPlayersCheck()
	{
		((PersistentObjectWorkQueue<IBudgetedFloatingEntity>)junkpileWaterWorkQueue).Add((IBudgetedFloatingEntity)this);
	}

	public void UpdateMovementFixedTick()
	{
		if (!isSinking)
		{
			SimpleBuoyancyUpdate(buoyancyPoints, ((Component)this).transform, ref baseRotation, Body, ref first, debugDraw, 1f, buoyancyAmplitude);
		}
	}

	public static void SimpleBuoyancyUpdate(Transform[] buoyancyPoints, Transform forTransform, ref Quaternion baseRotation, Rigidbody body, ref bool first, bool debugDraw, float movementMultiplier = 1f, float buoyancyAmplitude = 1f)
	{
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		if (buoyancyPoints != null && buoyancyPoints.Length >= 3)
		{
			Vector3 position = forTransform.position;
			Vector3 localPosition = buoyancyPoints[0].localPosition;
			Vector3 localPosition2 = buoyancyPoints[1].localPosition;
			Vector3 localPosition3 = buoyancyPoints[2].localPosition;
			Vector3 val = localPosition + position;
			Vector3 val2 = localPosition2 + position;
			Vector3 val3 = localPosition3 + position;
			val.y = WaterLevel.GetWaterSurface(val, waves: true, volumes: false);
			val2.y = WaterLevel.GetWaterSurface(val2, waves: true, volumes: false);
			val3.y = WaterLevel.GetWaterSurface(val3, waves: true, volumes: false);
			Vector3 val4 = default(Vector3);
			((Vector3)(ref val4))._002Ector(position.x, val.y - localPosition.y, position.z);
			Vector3 val5 = val2 - val;
			Vector3 val6 = Vector3.Cross(val3 - val, val5);
			Quaternion val7 = Quaternion.LookRotation(new Vector3(val6.x, val6.z, val6.y));
			Vector3 eulerAngles = ((Quaternion)(ref val7)).eulerAngles;
			val7 = Quaternion.Euler(0f - eulerAngles.x, 0f, 0f - eulerAngles.y);
			if (first)
			{
				Quaternion rotation = forTransform.rotation;
				baseRotation = Quaternion.Euler(0f, ((Quaternion)(ref rotation)).eulerAngles.y, 0f);
				first = false;
			}
			Vector3 val8 = Vector3.Lerp(forTransform.position, val4, movementMultiplier);
			Quaternion val9 = Quaternion.Lerp(forTransform.rotation, val7 * baseRotation, movementMultiplier);
			if (!Mathf.Approximately(buoyancyAmplitude, 1f))
			{
				float waterSurface = WaterLevel.GetWaterSurface(val, waves: false, volumes: false);
				val8 = Vector3.Lerp(Vector3Ex.WithY(val4, waterSurface), val4, buoyancyAmplitude);
				val9 = Quaternion.Slerp(baseRotation, val9, buoyancyAmplitude);
			}
			if (!Object.op_Implicit((Object)(object)body))
			{
				forTransform.SetPositionAndRotation(val8, val9);
				return;
			}
			body.MovePosition(val8);
			body.MoveRotation(val9);
		}
		else
		{
			float waterSurface2 = WaterLevel.GetWaterSurface(forTransform.position, waves: true, volumes: false);
			if (!Object.op_Implicit((Object)(object)body))
			{
				forTransform.position = new Vector3(forTransform.position.x, waterSurface2, forTransform.position.z);
			}
			else
			{
				body.MovePosition(new Vector3(body.position.x, waterSurface2, body.position.z));
			}
		}
	}

	public void UpdateNearbyPlayers()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (TimeUntil.op_Implicit(nextPlayerCheck) > 0f)
		{
			return;
		}
		nextPlayerCheck = TimeUntil.op_Implicit(Random.Range(0.5f, 1f));
		hasPlayersNearby = BaseNetworkable.HasCloseConnections(((Component)this).transform.position, updateCullRange);
		ToggleNetworkPositionTick(hasPlayersNearby);
		if (updateMovementFixedTick == null)
		{
			updateMovementFixedTick = UpdateMovementFixedTick;
		}
		if (hasPlayersNearby)
		{
			if (!IsInvokingFixedTime(updateMovementFixedTick))
			{
				InvokeRepeatingFixedTime(updateMovementFixedTick);
			}
		}
		else
		{
			CancelInvokeFixedTime(updateMovementFixedTick);
		}
	}

	public bool ShouldBeDestroyedBy(PlayerBoat boat)
	{
		return ShouldJunkpileBeDestroyedBy(boat);
	}

	public virtual bool ShouldJunkpileBeDestroyedBy(PlayerBoat boat)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)boat == (Object)null)
		{
			return false;
		}
		if (!DestroyableByPlayerBoats)
		{
			return false;
		}
		if (boat.rigidBody.mass >= MinimumPlayerBoatMassToBeDestroyed)
		{
			Vector3 linearVelocity = boat.rigidBody.linearVelocity;
			return ((Vector3)(ref linearVelocity)).magnitude >= MinimumPlayerBoatVelocityToBeDestroyed;
		}
		return false;
	}

	public JunkPileWater()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		updateCullRange = 16f;
		VehicleCheckRadius = 5f;
		buoyancyAmplitude = 1f;
		baseRotation = Quaternion.identity;
		first = true;
		base._002Ector();
	}
}

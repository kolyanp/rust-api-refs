using UnityEngine;

public class TriggerBoatMagnet : TriggerBase, IServerComponent
{
	public BoatBuildingStation ParentStation;

	public float AngularVelocityLerpAmount = 10f;

	public float PositionLerpAmount = 10f;

	public float RotationLerpAmount = 10f;

	public SphereCollider SphereTrigger;

	[ServerVar(Help = "(Generated) When enabled, boat building station magnets are active and will magnetically attract compatible boat building blocks into position")]
	public static bool BoatMagnetsEnabled = true;

	private bool run;

	internal override GameObject InterestedInObject(GameObject obj)
	{
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if ((Object)(object)baseEntity == (Object)null || baseEntity.isClient)
		{
			return null;
		}
		if (baseEntity is PlayerBoat playerBoat)
		{
			return ((Component)playerBoat).gameObject;
		}
		PlayerBoat componentInParent = ((Component)baseEntity).GetComponentInParent<PlayerBoat>();
		if (!((Object)(object)componentInParent != (Object)null))
		{
			return null;
		}
		return ((Component)componentInParent).gameObject;
	}

	internal override void OnObjectAdded(GameObject obj, Collider col)
	{
		base.OnObjectAdded(obj, col);
		if (entityContents.Count == 1)
		{
			run = true;
		}
	}

	internal override void OnObjectRemoved(GameObject obj)
	{
		base.OnObjectRemoved(obj);
		if (entityContents.Count == 0)
		{
			run = false;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		run = false;
	}

	private void FixedUpdate()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		if (!run || !BoatMagnetsEnabled || ParentStation.IsOn() || ParentStation.IsBusy() || entityContents == null)
		{
			return;
		}
		PlayerBoat targetBoat = GetTargetBoat();
		Vector3 val = default(Vector3);
		Quaternion val2 = default(Quaternion);
		((Component)this).transform.GetPositionAndRotation(ref val, ref val2);
		if ((Object)(object)targetBoat != (Object)null && !targetBoat.Anchored && !targetBoat.rigidBody.isKinematic)
		{
			val.y = ((Component)targetBoat).transform.position.y;
			Vector3 val3 = default(Vector3);
			Quaternion val4 = default(Quaternion);
			((Component)targetBoat).transform.GetPositionAndRotation(ref val3, ref val4);
			float num = Mathf.InverseLerp(SphereTrigger.radius, 0f, Vector3.Distance(val3, val));
			if (targetBoat.EngineOn())
			{
				num = 0.05f;
			}
			targetBoat.rigidBody.angularVelocity = Vector3.Lerp(targetBoat.rigidBody.angularVelocity, Vector3.zero, Time.deltaTime * AngularVelocityLerpAmount * num);
			targetBoat.rigidBody.MovePosition(Vector3.Lerp(val3, val, Time.fixedDeltaTime * PositionLerpAmount * num));
			targetBoat.rigidBody.MoveRotation(Quaternion.Lerp(val4, val2, Time.fixedDeltaTime * RotationLerpAmount * num));
		}
	}

	private PlayerBoat GetTargetBoat()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		PlayerBoat result = null;
		float num = float.MaxValue;
		Vector3 val = default(Vector3);
		Quaternion val2 = default(Quaternion);
		((Component)this).transform.GetPositionAndRotation(ref val, ref val2);
		foreach (BaseEntity entityContent in entityContents)
		{
			if (!((Object)(object)entityContent == (Object)null) && entityContent is PlayerBoat { IsDying: false, IsDestructibleWreck: false } playerBoat)
			{
				float num2 = Vector3.Distance(((Component)playerBoat).transform.position, val);
				if (num2 < num)
				{
					num = num2;
					result = playerBoat;
				}
			}
		}
		return result;
	}
}

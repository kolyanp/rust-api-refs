using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_ThrowGrenade : FSMStateBase
{
	public GameObjectRef deployedGrenadePrefab;

	public float cooldown = 60f;

	public const float grenadeRadius = 0.2f;

	public const float explosionRadius = 6f;

	public const string thrownGrenadeKey = "ThrownGrenadeRecently";

	private const float duration = 1f;

	private const float overshoot = 0.01f;

	private NpcShootingComponent _shooting;

	private NpcBarkComponent _barkComponent;

	private float remainingDuration;

	private NpcShootingComponent Shooting => _shooting ?? (_shooting = ((Component)Owner).GetComponent<NpcShootingComponent>());

	private NpcBarkComponent BarkComponent => _barkComponent ?? (_barkComponent = ((Component)Owner).GetComponent<NpcBarkComponent>());

	public static bool FindPotentialLandingPoint(SenseComponent Senses, out Vector3 landingPoint, out Vector3 throwVelocity)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		landingPoint = Vector3.zero;
		throwVelocity = Vector3.zero;
		if (!Senses.FindTargetLKP(out var lkp))
		{
			return false;
		}
		Vector3 eyePosition = Senses.EyePosition;
		Vector3 val = Vector3Ex.NormalizeXZ(lkp - eyePosition);
		landingPoint = lkp + val * 1f;
		Vector3 val2 = Vector3.Cross(Vector3.up, val);
		Vector3 normalized = ((Vector3)(ref val2)).normalized;
		Vector3 val3 = val + Quaternion.AngleAxis(10f, normalized) * Vector3.up;
		float throwVelocity2 = ThrownWeapon.GetThrowVelocity(eyePosition, landingPoint, val3);
		if (float.IsNaN(throwVelocity2))
		{
			val3 = val + Quaternion.AngleAxis(20f, normalized) * Vector3.up;
			throwVelocity2 = ThrownWeapon.GetThrowVelocity(eyePosition, landingPoint, val3);
			if (float.IsNaN(throwVelocity2))
			{
				return false;
			}
		}
		throwVelocity = val3 * throwVelocity2;
		return true;
	}

	public static bool ValidateLandingPoint(BaseEntity querier, Vector3 origin, Vector3 destination, Vector3 initialVelocity, out RaycastHit hitInfo, int maxSegments = 5)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		hitInfo = default(RaycastHit);
		Vector3 val = Vector3Ex.WithY(initialVelocity, 0f);
		float magnitude = ((Vector3)(ref val)).magnitude;
		if (magnitude < 0.001f)
		{
			return false;
		}
		val = Vector3Ex.WithY(destination - origin, 0f);
		float num = ((Vector3)(ref val)).magnitude / magnitude + 0.01f;
		Vector3 val2 = origin;
		Vector3 val3 = initialVelocity;
		float num2 = Mathf.Abs(Physics.gravity.y);
		float num3 = num / (float)maxSegments;
		int num4 = Mathf.CeilToInt(num / num3);
		num3 = num / (float)num4;
		float num5 = 0f;
		Vector3 val4 = default(Vector3);
		for (int i = 0; i < num4; i++)
		{
			if (!(num5 < num))
			{
				break;
			}
			float num6 = Mathf.Min(num3, num - num5);
			((Vector3)(ref val4))._002Ector(0f, 0f - num2, 0f);
			Vector3 val5 = val2 + val3 * num6 + 0.5f * val4 * num6 * num6;
			Vector3 val6 = val3 + val4 * num6;
			Vector3 val7 = val5 - val2;
			float magnitude2 = ((Vector3)(ref val7)).magnitude;
			if (magnitude2 > 0.001f && GamePhysics.Trace(new Ray(val2, ((Vector3)(ref val7)).normalized), 0.2f, out hitInfo, magnitude2, 1218519297, (QueryTriggerInteraction)0, querier))
			{
				if (Vector3.Distance(((RaycastHit)(ref hitInfo)).point, origin) <= 6f)
				{
					return false;
				}
				if (Vector3.Distance(((RaycastHit)(ref hitInfo)).point, destination) > 6f)
				{
					return false;
				}
				return true;
			}
			val2 = val5;
			val3 = val6;
			num5 += num6;
		}
		return false;
	}

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (!payload.velocity.HasValue)
		{
			if (AI.logIssues)
			{
				Debug.LogError((object)$"State_ThrowGrenade entered without valid velocity payload for {Owner}");
			}
			return EFSMStateStatus.Failure;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(deployedGrenadePrefab.resourcePath, base.Senses.EyePosition, Quaternion.LookRotation(payload.velocity.Value));
		if ((Object)(object)baseEntity == (Object)null)
		{
			return EFSMStateStatus.Failure;
		}
		baseEntity.SetCreatorEntity(Owner);
		baseEntity.SetVelocity(payload.velocity.Value);
		baseEntity.Spawn();
		remainingDuration = 1f;
		Shooting.AllowShooting = false;
		base.Blackboard.Add("ThrownGrenadeRecently", cooldown);
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			base.Senses.GetPerceivedAllies((List<BaseEntity>)(object)val);
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				((Component)item).GetComponent<BlackboardComponent>().Add("ThrownGrenadeRecently", cooldown);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		return base.OnStateEnter(payload);
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		remainingDuration -= deltaTime;
		if (remainingDuration <= 0f)
		{
			return EFSMStateStatus.Success;
		}
		return base.OnStateUpdate(deltaTime);
	}

	public override void OnStateExit()
	{
		Shooting.AllowShooting = true;
		base.OnStateExit();
	}
}

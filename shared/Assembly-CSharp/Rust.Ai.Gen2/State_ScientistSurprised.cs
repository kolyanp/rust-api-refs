using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_ScientistSurprised : FSMStateBase
{
	public float angularSpeedOverride;

	public float timeBeforeTurning;

	public float minTimeBeforeShooting = 0.6f;

	public float shootingDuration = 0.4f;

	public float maxDuration = 1f;

	public ENPCVoicelineCategory voicelineCategory = ENPCVoicelineCategory.Surprise;

	private NpcShootingComponent _shooting;

	private NpcBarkComponent _barkComponent;

	private float elapsedTime;

	private float elapsedTimeShooting;

	private Quaternion startRotation;

	private bool wasSurprisedFromBehind;

	private float previousAngularSpeed;

	private NpcShootingComponent Shooting => _shooting ?? (_shooting = ((Component)Owner).GetComponent<NpcShootingComponent>());

	private NpcBarkComponent BarkComponent => _barkComponent ?? (_barkComponent = ((Component)Owner).GetComponent<NpcBarkComponent>());

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTargetLKP(out var lkp))
		{
			return EFSMStateStatus.Failure;
		}
		startRotation = ((Component)Owner).transform.rotation;
		base.Agent.overrideDirectionWS = Vector3Ex.NormalizeXZ(startRotation * Vector3.forward);
		base.Agent.Pause(this);
		Shooting.AllowShooting = false;
		Shooting.AllowBeingAccurate = false;
		BarkComponent.PlayVoicelineFromCategory(voicelineCategory);
		elapsedTime = 0f;
		elapsedTimeShooting = 0f;
		if (angularSpeedOverride > 0f)
		{
			previousAngularSpeed = base.Agent.angularSpeed;
			base.Agent.angularSpeed = angularSpeedOverride;
		}
		wasSurprisedFromBehind = Vector3.Angle(((Component)Owner).transform.forward, Vector3Ex.WithY(lkp - ((Component)Owner).transform.position, 0f)) > 60f;
		return base.OnStateEnter(payload);
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTargetLKP(out var lkp, applyHeightOffset: true, predict: false, ignoreCrouch: false))
		{
			return EFSMStateStatus.Failure;
		}
		EFSMStateStatus result = base.OnStateUpdate(deltaTime);
		elapsedTime += deltaTime;
		if (!wasSurprisedFromBehind || !(elapsedTime < timeBeforeTurning))
		{
			Vector3 val = lkp - base.Senses.EyePosition;
			base.Agent.overrideDirectionWS = val;
			Matrix4x4 worldToNavMeshSpace = Owner.WorldToNavMeshSpace;
			Vector3 val2 = ((Matrix4x4)(ref worldToNavMeshSpace)).MultiplyVector(val);
			base.Agent.Move(-Vector3Ex.NormalizeXZ(val2) * deltaTime * 1.7f);
		}
		if (elapsedTime >= minTimeBeforeShooting && Vector3.Angle(((Component)Owner).transform.forward, Vector3Ex.WithY(lkp - ((Component)Owner).transform.position, 0f)) <= 5f)
		{
			Shooting.AllowShooting = true;
		}
		if (Shooting.AllowShooting)
		{
			elapsedTimeShooting += deltaTime;
			if (elapsedTimeShooting >= shootingDuration)
			{
				return EFSMStateStatus.Success;
			}
		}
		if (elapsedTime > maxDuration)
		{
			return EFSMStateStatus.Success;
		}
		return result;
	}

	public override void OnStateExit()
	{
		if (angularSpeedOverride > 0f)
		{
			base.Agent.angularSpeed = previousAngularSpeed;
		}
		base.Agent.overrideDirectionWS = null;
		base.Agent.Unpause(this);
		Shooting.AllowShooting = true;
		Shooting.AllowBeingAccurate = true;
		base.OnStateExit();
	}
}

using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_DismountVehicle : FSMStateBase
{
	[Tooltip("Optional montage for climbing down. Without one the animal is placed on the dismount point, which is what the CH47 has always done.")]
	[SerializeField]
	public RootMotionData Animation;

	[Tooltip("The slice of the montage that carries the animal down to the ground. Leave zeroed to warp the whole clip.")]
	[SerializeField]
	public MountMontageWindow travelWindow = MountMontageWindow.Whole;

	private const float FacingTolerance = 5f;

	private const float MinimumTurnSpeed = 90f;

	private const float TurnSpeedMultiplier = 2f;

	private RootMotionPlayer.PlayServerState animState;

	private readonly RootMotionPlayer.Warp[] warps = new RootMotionPlayer.Warp[1];

	private Vector3 destination;

	private Quaternion destinationRotation;

	private bool hasDestination;

	private bool isTurning;

	public bool HasAnimation
	{
		get
		{
			if (Animation != null)
			{
				return (Object)(object)Animation.inPlaceAnimation != (Object)null;
			}
			return false;
		}
	}

	private BaseNPC2 Npc => Owner as BaseNPC2;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		animState = null;
		hasDestination = false;
		isTurning = false;
		NPCMountable nPCMountable = (((Object)(object)Npc != (Object)null) ? Npc.Mount : null);
		if ((Object)(object)nPCMountable == (Object)null)
		{
			return EFSMStateStatus.Failure;
		}
		if (!nPCMountable.TryGetDismountPosition(Npc, out destination, out destinationRotation))
		{
			return EFSMStateStatus.Failure;
		}
		hasDestination = true;
		if (!HasAnimation)
		{
			Npc.OnDismountFinalized(destination, destinationRotation);
			return EFSMStateStatus.Success;
		}
		isTurning = true;
		return base.OnStateEnter(payload);
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		if (isTurning)
		{
			if ((Object)(object)Npc.Mount == (Object)null || !Npc.Mount.IsHostRideable)
			{
				PutDown();
				return EFSMStateStatus.Success;
			}
			if (!TurnTowardsDestination(deltaTime))
			{
				return EFSMStateStatus.None;
			}
			isTurning = false;
			PlayMontage();
			return EFSMStateStatus.None;
		}
		if (animState != null && animState.isPlaying)
		{
			return EFSMStateStatus.None;
		}
		PutDown();
		return EFSMStateStatus.Success;
	}

	public override void OnStateExit()
	{
		AnimPlayer.StopServerAndReturnToPool(ref animState);
		PutDown();
		base.OnStateExit();
	}

	private bool TurnTowardsDestination(float deltaTime)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 direction = Vector3Ex.NormalizeXZ(destination - ((Component)Owner).transform.position);
		if (direction.sqrMagnitude < 0.001f)
		{
			return true;
		}
		Quaternion val = NPCMountMontage.FacingToTravel(Animation, direction);
		float num = Mathf.Max(Agent.angularSpeed, 90f) * 2f;
		Agent.Face(Quaternion.RotateTowards(((Component)Owner).transform.rotation, val, num * deltaTime));
		return Vector3.Angle(Vector3Ex.NormalizeXZ(((Component)Owner).transform.forward), Vector3Ex.NormalizeXZ(val * Vector3.forward)) < 5f;
	}

	private void PlayMontage()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)Owner).transform.position;
		if (!NPCMountMontage.HasYaw(Animation))
		{
			destinationRotation = Quaternion.LookRotation(Vector3Ex.NormalizeXZ(((Component)Owner).transform.forward));
		}
		Npc.DetachFromHost();
		animState = RootMotionPlayer.PlayServerState.TakeFromPool(Animation, ((Component)Owner).transform);
		animState.constrainToNavmesh = false;
		warps[0] = NPCMountMontage.BuildWarp(Animation, travelWindow, animState.initialRotation, position, destination, destinationRotation, animState.playbackSpeed);
		animState.warps = warps;
		AnimPlayer.PlayServer(animState);
	}

	private void PutDown()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (hasDestination && (Object)(object)Npc != (Object)null && Npc.IsMounted)
		{
			Npc.OnDismountFinalized(destination, destinationRotation);
		}
	}
}

using System;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_MountVehicle : State_MoveToTarget
{
	[SerializeField]
	[Tooltip("Optional montage for climbing aboard. Without one the animal is placed on the anchor when it arrives.")]
	public RootMotionData Animation;

	[SerializeField]
	[Tooltip("The slice of the montage that carries the animal up onto the anchor. Leave zeroed to warp the whole clip.")]
	public MountMontageWindow travelWindow = MountMontageWindow.Whole;

	private const float navmeshSampleRadius = 5f;

	private RootMotionPlayer.PlayServerState animState;

	private readonly RootMotionPlayer.Warp[] warps = new RootMotionPlayer.Warp[1];

	private bool isClimbing;

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
		isClimbing = false;
		animState = null;
		if ((Object)(object)Npc == (Object)null || (Object)(object)Npc.Mount == (Object)null)
		{
			return EFSMStateStatus.Failure;
		}
		return base.OnStateEnter(payload);
	}

	protected override bool GetMoveDestination(out NavVector3 destination)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		destination = default;
		NPCMountable nPCMountable = (((Object)(object)Npc != (Object)null) ? Npc.Mount : null);
		if ((Object)(object)nPCMountable == (Object)null)
		{
			return false;
		}
		Vector3 approachPosition = nPCMountable.GetApproachPosition(Npc.MountAnchorIndex);
		if (!Agent.SamplePosition(approachPosition, out var hitWS, 5f))
		{
			return false;
		}
		destination = Agent.WorldToNavSpace(hitWS.position);
		return true;
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		if ((Object)(object)Npc.Mount == (Object)null || !Npc.Mount.IsHostRideable)
		{
			return EFSMStateStatus.Failure;
		}
		if (!isClimbing)
		{
			return base.OnStateUpdate(deltaTime) switch
			{
				EFSMStateStatus.Failure => EFSMStateStatus.Failure, 
				EFSMStateStatus.Success => BeginClimb(), 
				_ => EFSMStateStatus.None, 
			};
		}
		if (animState == null || !animState.isPlaying)
		{
			return Finish();
		}
		return EFSMStateStatus.None;
	}

	private EFSMStateStatus BeginClimb()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		isClimbing = true;
		NPCMountable nPCMountable = (((Object)(object)Npc != (Object)null) ? Npc.Mount : null);
		Transform val = (((Object)(object)nPCMountable != (Object)null) ? nPCMountable.GetAnchorTransform(Npc.MountAnchorIndex) : null);
		if ((Object)(object)val == (Object)null)
		{
			return EFSMStateStatus.Failure;
		}
		if (!HasAnimation)
		{
			return Finish();
		}
		Agent.ResetPath();
		Agent.Pause(this);
		Agent.IsJumping = true;
		Vector3 position = ((Component)Owner).transform.position;
		Vector3 val2 = Vector3Ex.NormalizeXZ(val.position - position);
		if (val2.sqrMagnitude > 0.001f)
		{
			((Component)Owner).transform.rotation = Quaternion.LookRotation(val2);
		}
		animState = RootMotionPlayer.PlayServerState.TakeFromPool(Animation, ((Component)Owner).transform);
		animState.constrainToNavmesh = false;
		warps[0] = NPCMountMontage.BuildWarp(Animation, travelWindow, animState.initialRotation, position, val.position, val.rotation, animState.playbackSpeed);
		animState.warps = warps;
		AnimPlayer.PlayServer(animState);
		return EFSMStateStatus.None;
	}

	private EFSMStateStatus Finish()
	{
		NPCMountable nPCMountable = (((Object)(object)Npc != (Object)null) ? Npc.Mount : null);
		if ((Object)(object)nPCMountable == (Object)null)
		{
			return EFSMStateStatus.Failure;
		}
		Npc.OnMountFinalized(nPCMountable.GetBaseEntity(), Npc.MountAnchorIndex);
		return EFSMStateStatus.Success;
	}

	public override void OnStateExit()
	{
		bool flag = animState != null;
		AnimPlayer.StopServerAndReturnToPool(ref animState);
		if (flag)
		{
			Agent.IsJumping = false;
			Agent.Unpause(this);
		}
		if ((Object)(object)Npc != (Object)null && !Npc.IsMounted)
		{
			Npc.CancelMount();
		}
		base.OnStateExit();
	}
}

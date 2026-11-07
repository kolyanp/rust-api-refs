using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_TurnToTarget : State_PlayAnimationRM
{
	[SerializeField]
	public RootMotionData TurnLeft;

	[SerializeField]
	public RootMotionData TurnRight;

	[SerializeField]
	public RootMotionData TurnAbout;

	[SerializeField]
	[Tooltip("Past this many degrees off our facing, use the about turn rather than a side turn.")]
	private float aboutFaceAngle = 130f;

	public bool HasTurnAnimation
	{
		get
		{
			if (!(TurnLeft != null) && !(TurnRight != null))
			{
				return TurnAbout != null;
			}
			return true;
		}
	}

	protected virtual bool FindTurnTarget(out Vector3 position)
	{
		return Senses.FindTargetPosition(out position);
	}

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (!HasTurnAnimation || !FindTurnTarget(out var position))
		{
			return EFSMStateStatus.Failure;
		}
		Vector3 targetDirection = Vector3Ex.NormalizeXZ(position - ((Component)Owner).transform.position);
		Animation = TurnFor(((Component)Owner).transform.forward, targetDirection);
		return base.OnStateEnter(payload);
	}

	protected RootMotionData TurnFor(Vector3 forward, Vector3 targetDirection)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.SignedAngle(forward, targetDirection, Vector3.up);
		if (Mathf.Abs(num) > aboutFaceAngle)
		{
			RootMotionData rootMotionData = TurnAbout;
			if ((object)rootMotionData == null)
			{
				if (!(num > 0f))
				{
					return TurnLeft;
				}
				rootMotionData = TurnRight;
			}
			return rootMotionData;
		}
		if (num > 0f)
		{
			return TurnRight ?? TurnAbout ?? TurnLeft;
		}
		return TurnLeft ?? TurnAbout ?? TurnRight;
	}
}

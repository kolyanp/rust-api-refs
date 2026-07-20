using ConVar;
using UnityEngine;

namespace Rust.Ai.Gen2;

public abstract class State_PlayAnimationBase : FSMStateBase
{
	[SerializeField]
	public bool FaceTarget;

	protected RootMotionPlayer.PlayServerState animState;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (FaceTarget && base.Senses.FindTargetPosition(out var targetPosition))
		{
			Vector3 val = targetPosition - ((Component)Owner).transform.position;
			val.y = 0f;
			if (((Vector3)(ref val)).magnitude > 0.001f)
			{
				((Component)Owner).transform.rotation = Quaternion.LookRotation(val);
			}
		}
		return base.OnStateEnter(payload);
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		if (animState == null)
		{
			if (AI.logIssues)
			{
				Debug.LogError((object)$"[FSM] Animation state is null in state update {base.Name} from {Owner}");
			}
			return EFSMStateStatus.Failure;
		}
		if (!animState.isPlaying)
		{
			return EFSMStateStatus.Success;
		}
		return EFSMStateStatus.None;
	}

	public override void OnStateExit()
	{
		base.AnimPlayer.StopServerAndReturnToPool(ref animState);
	}
}

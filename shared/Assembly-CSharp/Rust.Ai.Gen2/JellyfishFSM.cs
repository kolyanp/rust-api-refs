using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class JellyfishFSM : SwimmingNPCFSM
{
	[Serializable]
	public class State_Drift : FSMStateBase
	{
		[Tooltip("How long the swarm hangs in the current before setting off on its next leg, in seconds.")]
		[SerializeField]
		private Vector2 driftDurationRange = new Vector2(4f, 12f);

		private TimeUntil driftEndTime;

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			SwimmingNPC swimmingNPC = default;
			if (!((Component)Owner).TryGetComponent<SwimmingNPC>(ref swimmingNPC))
			{
				return EFSMStateStatus.Failure;
			}
			swimmingNPC.destination = ((Component)swimmingNPC).transform.position;
			driftEndTime = TimeUntil.op_Implicit(Rng.Range(driftDurationRange.x, driftDurationRange.y));
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			if (TimeUntil.op_Implicit(driftEndTime) <= 0f)
			{
				return EFSMStateStatus.Success;
			}
			return EFSMStateStatus.None;
		}

		public State_Drift()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	public State_Drift drift = new State_Drift();

	public State_RoamWater roamWater = new State_RoamWater();

	public State_Deactivate deactivate = new State_Deactivate();

	protected override AliveLayout BuildAlive(State_Nothing onNavmesh)
	{
		State_Nothing state_Nothing = new State_Nothing
		{
			Name = "Can drift"
		};
		onNavmesh.AddChildren(deactivate.AddTickTransition(drift, ~new Trans_NoPlayerConnectionsInRange()), state_Nothing.AddTickTransition(deactivate, new Trans_NoPlayerConnectionsInRange()).AddChildren(drift.AddEndTransition(roamWater), roamWater.AddEndTransition(drift)));
		return new AliveLayout
		{
			entry = drift,
			idle = drift
		};
	}
}

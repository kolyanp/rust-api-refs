using UnityEngine;

namespace Rust.Ai.Gen2;

public class FrogFSM : CritterAnimalFSM
{
	public State_Idle stopIdle = new State_Idle();

	private RustNavMeshAgent _agent;

	public override FSMStateBase idle => stopIdle;

	private RustNavMeshAgent Agent => _agent ?? (_agent = ((Component)baseEntity).GetComponent<RustNavMeshAgent>());

	protected override void OnTicked(float deltaTime)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		base.OnTicked(deltaTime);
		if (baseEntity is Frog frog)
		{
			frog.SetNavTarget(Agent.destinationWS);
		}
	}
}

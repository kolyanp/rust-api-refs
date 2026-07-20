using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_ScientistDead : State_Dead
{
	public GameObjectRef DeathEffect;

	private NpcBarkComponent _barkComponent;

	private NpcBarkComponent BarkComponent => _barkComponent ?? (_barkComponent = ((Component)Owner).GetComponent<NpcBarkComponent>());

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (DeathEffect != null)
		{
			Effect.server.Run(DeathEffect.resourcePath, ((Component)Owner).transform.position, Vector3.up);
		}
		return base.OnStateEnter(payload);
	}
}

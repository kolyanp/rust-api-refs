using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_Circle : FSMStateBase
{
	[SerializeField]
	public float radius = 16f;

	[SerializeField]
	public RustNavMeshAgent.Speeds speed = RustNavMeshAgent.Speeds.Sprint;

	private bool clockWise = true;

	private float radiusOffset;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		if ((Object)(object)payload.entity != (Object)null)
		{
			base.Senses.TrySetTarget(payload.entity);
		}
		radiusOffset = Random.Range(-1f, 1f);
		clockWise = Random.value > 0.5f;
		return base.OnStateEnter(payload);
	}

	public override void OnStateExit()
	{
		base.Agent.ResetPath();
		base.OnStateExit();
	}

	protected virtual bool GetCircleOrigin(out Vector3 origin)
	{
		return base.Senses.FindTargetPosition(out origin);
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if (!GetCircleOrigin(out var origin))
		{
			return EFSMStateStatus.Failure;
		}
		float num = radius + radiusOffset;
		Quaternion val = Quaternion.LookRotation(((Component)Owner).transform.position - origin);
		float num2 = (((Quaternion)(ref val)).eulerAngles.y + 5f * (float)(clockWise ? 1 : (-1))) * (MathF.PI / 180f);
		Vector3 val2 = origin + new Vector3(Mathf.Sin(num2), 0f, Mathf.Cos(num2)) * num;
		val2.y = Mathf.Lerp(origin.y, ((Component)Owner).transform.position.y, Mathf.InverseLerp(0f, Vector3.Distance(origin, ((Component)Owner).transform.position), num));
		if (base.Agent.Raycast(val2, out var _))
		{
			return EFSMStateStatus.Failure;
		}
		if (!base.Agent.SetDestinationWithParams(base.Agent.WorldToNavSpace(val2), autoBraking: false, speed))
		{
			return EFSMStateStatus.Failure;
		}
		return base.OnStateUpdate(deltaTime);
	}
}

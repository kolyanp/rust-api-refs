using Facepunch;
using ProtoBuf;
using Rust.Ai.Gen2;
using UnityEngine;

public class NPCNetworking : EntityComponent<BaseEntity>
{
	public const BaseEntity.Flags FLAG_IS_SWIMMING = BaseEntity.Flags.Reserved1;

	public const BaseEntity.Flags FLAG_IS_JUMPING = BaseEntity.Flags.Reserved2;

	public const BaseEntity.Flags FLAG_IS_AIMING = BaseEntity.Flags.Reserved3;

	public const BaseEntity.Flags FLAG_IS_RELAXED = BaseEntity.Flags.Reserved4;

	public const BaseEntity.Flags FLAG_IS_CROUCHING = BaseEntity.Flags.Reserved5;

	public const BaseEntity.Flags FLAG_IS_ALERT = BaseEntity.Flags.Reserved6;

	private SenseComponent _senses;

	private RustNavMeshAgent _agent;

	public Vector3 LookDirection { get; private set; }

	public float DesiredSwimDepth { get; private set; }

	private SenseComponent Senses => _senses ?? (_senses = ((Component)base.baseEntity).GetComponent<SenseComponent>());

	private RustNavMeshAgent Agent => _agent ?? (_agent = ((Component)base.baseEntity).GetComponent<RustNavMeshAgent>());

	public override void InitShared()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (base.baseEntity.isServer)
		{
			LookDirection = ((Component)this).transform.forward;
		}
	}

	public void Tick()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		Vector3 lookDirection = LookDirection;
		float desiredSwimDepth = DesiredSwimDepth;
		bool flag = base.baseEntity.HasFlag(BaseEntity.Flags.Reserved6);
		Matrix4x4 eyeTransform = Senses.GetEyeTransform();
		LookDirection = ((Matrix4x4)(ref eyeTransform)).rotation * Vector3.forward;
		DesiredSwimDepth = Agent.desiredSwimDepth.Value;
		BaseEntity target;
		bool flag2 = Senses.FindTarget(out target);
		if (base.baseEntity.net != null && base.baseEntity.net.group != null && base.baseEntity.net.group.subscribers != null && base.baseEntity.net.group.subscribers.Count > 0 && (lookDirection != LookDirection || desiredSwimDepth != DesiredSwimDepth || flag != flag2))
		{
			base.baseEntity.SetFlagLocal(BaseEntity.Flags.Reserved6, flag2);
			base.baseEntity.SendNetworkUpdate();
		}
	}

	public override void SaveComponent(BaseNetworkable.SaveInfo info)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		base.SaveComponent(info);
		if (base.baseEntity.isServer && !info.forDisk)
		{
			info.msg.npcTargetState = Pool.Get<NPCTargetState>();
			info.msg.npcTargetState.lookDirection = LookDirection;
			info.msg.npcTargetState.desiredSwimDepth = DesiredSwimDepth;
		}
	}
}

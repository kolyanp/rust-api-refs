using System;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
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

	public bool networkLookDirection = true;

	private SenseComponent _senses;

	private RustNavMeshAgent _agent;

	private Action _sendLookDirection;

	public Vector3 LookDirection
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public float DesiredSwimDepth { get; private set; }

	private SenseComponent Senses => _senses ?? (_senses = ((Component)baseEntity).GetComponent<SenseComponent>());

	private RustNavMeshAgent Agent => _agent ?? (_agent = ((Component)baseEntity).GetComponent<RustNavMeshAgent>());

	private Action SendLookDirectionAction => SendLookDirection;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("NPCNetworking.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void InitShared()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (baseEntity.isServer)
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
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		Vector3 lookDirection = LookDirection;
		float desiredSwimDepth = DesiredSwimDepth;
		bool flag = baseEntity.HasFlag(BaseEntity.Flags.Reserved6);
		Matrix4x4 eyeTransform = Senses.GetEyeTransform();
		LookDirection = eyeTransform.rotation * Vector3.forward;
		DesiredSwimDepth = Agent.desiredSwimDepth.Value;
		bool flag2 = Senses.FindTarget(out var _);
		if (baseEntity.net != null && baseEntity.net.group != null && baseEntity.net.group.subscribers != null && baseEntity.net.group.subscribers.Count > 0)
		{
			if (desiredSwimDepth != DesiredSwimDepth || flag != flag2)
			{
				baseEntity.SetFlagLocal(BaseEntity.Flags.Reserved6, flag2);
				baseEntity.SendNetworkUpdate();
			}
			else if (networkLookDirection && lookDirection != LookDirection && !baseEntity.IsInvoking(SendLookDirectionAction))
			{
				baseEntity.Invoke(SendLookDirectionAction, Player.serverTickInterval);
			}
		}
	}

	private void SendLookDirection()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		baseEntity.InvalidateNetworkCache();
		baseEntity.ClientRPC(RpcTarget.NetworkGroup("CL_ReceiveLookDirection"), LookDirection);
	}

	public override void SaveComponent(BaseNetworkable.SaveInfo info)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		base.SaveComponent(info);
		if (baseEntity.isServer && !info.forDisk)
		{
			info.msg.npcTargetState = Pool.Get<NPCTargetState>();
			info.msg.npcTargetState.lookDirection = LookDirection;
			info.msg.npcTargetState.desiredSwimDepth = DesiredSwimDepth;
		}
	}
}

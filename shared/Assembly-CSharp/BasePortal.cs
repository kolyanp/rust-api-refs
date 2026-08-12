using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class BasePortal : BaseCombatEntity
{
	public bool isUsablePortal = true;

	private Vector3 destination_pos;

	private Quaternion destination_rot;

	public BasePortal targetPortal;

	public NetworkableId targetID;

	public Transform localEntryExitPos;

	public Transform relativeAnchor;

	public bool isMirrored = true;

	public GameObjectRef appearEffect;

	public GameObjectRef disappearEffect;

	public GameObjectRef transitionSoundEffect;

	public string useTagString = "";

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BasePortal.OnRpcMessage"))
		{
			if (rpc == 561762999 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_UsePortal"));
				}
				using (TimeWarning.New("RPC_UsePortal"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(561762999u, "RPC_UsePortal", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(561762999u, "RPC_UsePortal", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_UsePortal(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_UsePortal");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Save(SaveInfo info)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.ioEntity = Pool.Get<IOEntity>();
		info.msg.ioEntity.genericEntRef1 = targetID;
	}

	public override void Load(LoadInfo info)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			targetID = info.msg.ioEntity.genericEntRef1;
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
	}

	public void LinkPortal()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetPortal != (Object)null)
		{
			targetID = targetPortal.net.ID;
		}
		if ((Object)(object)targetPortal == (Object)null && ((NetworkableId)(ref targetID)).IsValid)
		{
			BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(targetID);
			if ((Object)(object)baseNetworkable != (Object)null)
			{
				targetPortal = ((Component)baseNetworkable).GetComponent<BasePortal>();
			}
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Debug.Log((object)"Post server load");
		LinkPortal();
	}

	public void SetDestination(Vector3 destPos, Quaternion destRot)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		destination_pos = destPos;
		destination_rot = destRot;
	}

	public Vector3 GetLocalEntryExitPosition()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)localEntryExitPos).transform.position;
	}

	public Quaternion GetLocalEntryExitRotation()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)localEntryExitPos).transform.rotation;
	}

	public BasePortal GetPortal()
	{
		LinkPortal();
		return targetPortal;
	}

	public virtual void UsePortal(BasePlayer player)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnPortalUse", player, this) != null)
		{
			return;
		}
		LinkPortal();
		if ((Object)(object)targetPortal != (Object)null)
		{
			player.PauseFlyHackDetection();
			player.PauseSpeedHackDetection();
			player.ApplyStallProtection(4f);
			Vector3 position = ((Component)player).transform.position;
			Vector3 val = targetPortal.GetLocalEntryExitPosition();
			Vector3 val2 = ((Component)this).transform.InverseTransformDirection(player.eyes.BodyForward());
			Vector3 val3 = val2;
			if (isMirrored)
			{
				Vector3 val4 = ((Component)this).transform.InverseTransformPoint(((Component)player).transform.position);
				val = ((Component)targetPortal.relativeAnchor).transform.TransformPoint(val4);
				val3 = ((Component)targetPortal.relativeAnchor).transform.TransformDirection(val2);
			}
			else
			{
				val3 = targetPortal.GetLocalEntryExitRotation() * Vector3.forward;
			}
			if (disappearEffect.isValid)
			{
				Effect.server.Run(disappearEffect.resourcePath, position, Vector3.up);
			}
			if (appearEffect.isValid)
			{
				Effect.server.Run(appearEffect.resourcePath, val, Vector3.up);
			}
			player.ClientRPC(RpcTarget.Player("StartLoading_Quick", player), arg1: true);
			player.SetParent(null, worldPositionStays: true);
			player.Teleport(val);
			player.ClientRPC(RpcTarget.Player("ForceViewAnglesTo", player), val3);
			if (transitionSoundEffect.isValid)
			{
				Effect.server.Run(transitionSoundEffect.resourcePath, ((Component)targetPortal.relativeAnchor).transform.position, Vector3.up);
			}
			player.UpdateNetworkGroup();
			player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, b: true);
			SendNetworkUpdateImmediate();
			Interface.CallHook("OnPortalUsed", player, this);
		}
		else
		{
			Debug.Log((object)"No portal...");
		}
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_UsePortal(RPCMessage msg)
	{
		if (IsActive())
		{
			BasePlayer player = msg.player;
			if (player.CanInteract())
			{
				UsePortal(player);
			}
		}
	}

	public bool IsActive()
	{
		return true;
	}
}

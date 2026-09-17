using System;
using ConVar;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Lift : AnimatedBuildingBlock
{
	public GameObjectRef triggerPrefab;

	public string triggerBone;

	public float resetDelay = 5f;

	private Collider cabinTrigger;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Lift.OnRpcMessage"))
		{
			if (rpc == 2657791441u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_UseLift"));
				}
				using (TimeWarning.New("RPC_UseLift"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2657791441u, "RPC_UseLift", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_UseLift(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_UseLift");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_UseLift(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && Interface.CallHook("OnLiftUse", this, rpc.player) == null && PlayerIsAtCabin(rpc.player))
		{
			MoveUp();
		}
	}

	private bool PlayerIsAtCabin(BasePlayer player)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)cabinTrigger == (Object)null && !ResolveCabinTrigger())
		{
			return false;
		}
		Bounds val = cabinTrigger.bounds;
		return ((Bounds)(ref val)).SqrDistance(player.eyes.position) <= 9f;
	}

	private bool ResolveCabinTrigger()
	{
		foreach (BaseEntity child in children)
		{
			if (child.prefabID != triggerPrefab.resourceID)
			{
				continue;
			}
			TriggerParent[] componentsInChildren = ((Component)child).GetComponentsInChildren<TriggerParent>();
			foreach (TriggerParent triggerParent in componentsInChildren)
			{
				if (!((Object)(object)GameObjectEx.ToBaseEntity(((Component)triggerParent).gameObject) != (Object)(object)child))
				{
					cabinTrigger = ((Component)triggerParent).GetComponent<Collider>();
					if ((Object)(object)cabinTrigger != (Object)null)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private void MoveUp()
	{
		if (!IsOpen() && !IsBusy())
		{
			SetFlagLocal(Flags.Open, b: true);
			SendNetworkUpdateImmediate();
		}
	}

	private void MoveDown()
	{
		if (IsOpen() && !IsBusy())
		{
			SetFlagLocal(Flags.Open, b: false);
			SendNetworkUpdateImmediate();
		}
	}

	protected override void OnAnimatorDisabled()
	{
		if (base.isServer && IsOpen())
		{
			Invoke(MoveDown, resetDelay);
		}
	}

	public override void Spawn()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		base.Spawn();
		bool flag = false;
		if (!Application.isLoadingSave && !flag)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(triggerPrefab.resourcePath, Vector3.zero, Quaternion.identity);
			baseEntity.Spawn();
			baseEntity.SetParent(this, triggerBone);
		}
	}
}

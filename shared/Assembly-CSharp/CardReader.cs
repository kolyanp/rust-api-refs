using System;
using ConVar;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class CardReader : IOEntity
{
	public float accessDuration = 10f;

	public int accessLevel;

	public GameObjectRef accessGrantedEffect;

	public GameObjectRef accessDeniedEffect;

	public GameObjectRef swipeEffect;

	public Transform audioPosition;

	public Flags AccessLevel1 = Flags.Reserved1;

	public Flags AccessLevel2 = Flags.Reserved2;

	public Flags AccessLevel3 = Flags.Reserved3;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CardReader.OnRpcMessage"))
		{
			if (rpc == 979061374 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ServerCardSwiped"));
				}
				using (TimeWarning.New("ServerCardSwiped"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(979061374u, "ServerCardSwiped", this, player, 3f))
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
							ServerCardSwiped(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in ServerCardSwiped");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ResetIOState()
	{
		base.ResetIOState();
		CancelInvoke(GrantCard);
		CancelInvoke(CancelAccess);
		CancelAccess();
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (!IsOn())
		{
			return 0;
		}
		return base.GetPassthroughAmount(outputSlot);
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
	}

	public void CancelAccess()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: false);
		}
		MarkDirty();
	}

	public void FailCard()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Effect.server.Run(accessDeniedEffect.resourcePath, audioPosition.position, Vector3.up);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(AccessLevel1, accessLevel == 1);
		flagsUpdateScope.Set(AccessLevel2, accessLevel == 2);
		flagsUpdateScope.Set(AccessLevel3, accessLevel == 3);
	}

	public void GrantCard()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: true);
		}
		MarkDirty();
		Effect.server.Run(accessGrantedEffect.resourcePath, audioPosition.position, Vector3.up);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void ServerCardSwiped(RPCMessage msg)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if (!IsPowered() || Vector3Ex.Distance2D(((Component)player).transform.position, ((Component)this).transform.position) > 1f || IsInvoking(GrantCard) || IsInvoking(FailCard) || HasFlag(Flags.On))
		{
			return;
		}
		NetworkableId uid = msg.read.EntityID();
		Keycard keycard = BaseNetworkable.serverEntities.Find(uid) as Keycard;
		Effect.server.Run(swipeEffect.resourcePath, audioPosition.position, Vector3.up, player.net.connection);
		if ((Object)(object)keycard == (Object)null || Interface.CallHook("OnCardSwipe", this, keycard, player) != null)
		{
			return;
		}
		Item item = keycard.GetItem();
		if (item == null || (Object)(object)item.parent.playerOwner != (Object)(object)player)
		{
			return;
		}
		if (keycard.accessLevel == accessLevel && item.conditionNormalized > 0f)
		{
			Facepunch.Rust.Analytics.Azure.OnKeycardSwiped(player, this);
			Invoke(GrantCard, 0.5f);
			item.LoseCondition(1f);
			if (accessLevel == 3 && player.serverClan != null)
			{
				player.AddClanScore((ClanScoreEventType)12);
			}
		}
		else
		{
			Invoke(FailCard, 0.5f);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.ioEntity.genericInt1 = accessLevel;
		info.msg.ioEntity.genericFloat1 = accessDuration;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			accessLevel = info.msg.ioEntity.genericInt1;
			accessDuration = info.msg.ioEntity.genericFloat1;
		}
	}
}

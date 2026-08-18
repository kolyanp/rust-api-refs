using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class MobilePhone : HeldEntity
{
	public PhoneController Controller;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MobilePhone.OnRpcMessage"))
		{
			if (rpc == 1529322558 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - AnswerPhone"));
				}
				using (TimeWarning.New("AnswerPhone"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(1529322558u, "AnswerPhone", this, player))
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
							AnswerPhone(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in AnswerPhone");
					}
				}
				return true;
			}
			if (rpc == 2754362156u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ClearCurrentUser"));
				}
				using (TimeWarning.New("ClearCurrentUser"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(2754362156u, "ClearCurrentUser", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ClearCurrentUser(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in ClearCurrentUser");
					}
				}
				return true;
			}
			if (rpc == 1095090232 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - InitiateCall"));
				}
				using (TimeWarning.New("InitiateCall"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(1095090232u, "InitiateCall", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							InitiateCall(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in InitiateCall");
					}
				}
				return true;
			}
			if (rpc == 2606442785u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_AddSavedNumber"));
				}
				using (TimeWarning.New("Server_AddSavedNumber"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2606442785u, "Server_AddSavedNumber", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(2606442785u, "Server_AddSavedNumber", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_AddSavedNumber(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in Server_AddSavedNumber");
					}
				}
				return true;
			}
			if (rpc == 1402406333 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_RemoveSavedNumber"));
				}
				using (TimeWarning.New("Server_RemoveSavedNumber"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1402406333u, "Server_RemoveSavedNumber", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1402406333u, "Server_RemoveSavedNumber", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg6 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_RemoveSavedNumber(msg6);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in Server_RemoveSavedNumber");
					}
				}
				return true;
			}
			if (rpc == 2704491961u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_RequestCurrentState"));
				}
				using (TimeWarning.New("Server_RequestCurrentState"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(2704491961u, "Server_RequestCurrentState", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg7 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_RequestCurrentState(msg7);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
						player.Kick("RPC Error in Server_RequestCurrentState");
					}
				}
				return true;
			}
			if (rpc == 942544266 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_RequestPhoneDirectory"));
				}
				using (TimeWarning.New("Server_RequestPhoneDirectory"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(942544266u, "Server_RequestPhoneDirectory", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(942544266u, "Server_RequestPhoneDirectory", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg8 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_RequestPhoneDirectory(msg8);
						}
					}
					catch (Exception ex7)
					{
						Debug.LogException(ex7);
						player.Kick("RPC Error in Server_RequestPhoneDirectory");
					}
				}
				return true;
			}
			if (rpc == 1240133378 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ServerDeleteVoicemail"));
				}
				using (TimeWarning.New("ServerDeleteVoicemail"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1240133378u, "ServerDeleteVoicemail", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1240133378u, "ServerDeleteVoicemail", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg9 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ServerDeleteVoicemail(msg9);
						}
					}
					catch (Exception ex8)
					{
						Debug.LogException(ex8);
						player.Kick("RPC Error in ServerDeleteVoicemail");
					}
				}
				return true;
			}
			if (rpc == 1221129498 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ServerHangUp"));
				}
				using (TimeWarning.New("ServerHangUp"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(1221129498u, "ServerHangUp", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg10 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ServerHangUp(msg10);
						}
					}
					catch (Exception ex9)
					{
						Debug.LogException(ex9);
						player.Kick("RPC Error in ServerHangUp");
					}
				}
				return true;
			}
			if (rpc == 239260010 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ServerPlayVoicemail"));
				}
				using (TimeWarning.New("ServerPlayVoicemail"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(239260010u, "ServerPlayVoicemail", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(239260010u, "ServerPlayVoicemail", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg11 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ServerPlayVoicemail(msg11);
						}
					}
					catch (Exception ex10)
					{
						Debug.LogException(ex10);
						player.Kick("RPC Error in ServerPlayVoicemail");
					}
				}
				return true;
			}
			if (rpc == 189198880 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ServerSendVoicemail"));
				}
				using (TimeWarning.New("ServerSendVoicemail"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(189198880u, "ServerSendVoicemail", this, player, 5uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg12 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ServerSendVoicemail(msg12);
						}
					}
					catch (Exception ex11)
					{
						Debug.LogException(ex11);
						player.Kick("RPC Error in ServerSendVoicemail");
					}
				}
				return true;
			}
			if (rpc == 2760189029u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ServerStopVoicemail"));
				}
				using (TimeWarning.New("ServerStopVoicemail"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2760189029u, "ServerStopVoicemail", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(2760189029u, "ServerStopVoicemail", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg13 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ServerStopVoicemail(msg13);
						}
					}
					catch (Exception ex12)
					{
						Debug.LogException(ex12);
						player.Kick("RPC Error in ServerStopVoicemail");
					}
				}
				return true;
			}
			if (rpc == 3900772076u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SetCurrentUser"));
				}
				using (TimeWarning.New("SetCurrentUser"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(3900772076u, "SetCurrentUser", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage currentUser = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SetCurrentUser(currentUser);
						}
					}
					catch (Exception ex13)
					{
						Debug.LogException(ex13);
						player.Kick("RPC Error in SetCurrentUser");
					}
				}
				return true;
			}
			if (rpc == 2760249627u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - UpdatePhoneName"));
				}
				using (TimeWarning.New("UpdatePhoneName"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2760249627u, "UpdatePhoneName", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(2760249627u, "UpdatePhoneName", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg14 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							UpdatePhoneName(msg14);
						}
					}
					catch (Exception ex14)
					{
						Debug.LogException(ex14);
						player.Kick("RPC Error in UpdatePhoneName");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Save(SaveInfo info)
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (info.msg.telephone == null)
		{
			info.msg.telephone = Pool.Get<Telephone>();
		}
		info.msg.telephone.phoneNumber = Controller.PhoneNumber;
		info.msg.telephone.phoneName = Controller.PhoneName;
		info.msg.telephone.lastNumber = Controller.lastDialedNumber;
		info.msg.telephone.savedNumbers = Controller.savedNumbers;
		if (!info.forDisk)
		{
			info.msg.telephone.usingPlayer = Controller.currentPlayerRef.uid;
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		Controller.ServerInit();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Controller.PostServerLoad();
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		Controller.DoServerDestroy();
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		Controller.OnParentChanged(newParent);
	}

	public override void OnHeldChanged()
	{
		base.OnHeldChanged();
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if ((Object)(object)ownerPlayer != (Object)null)
		{
			ownerPlayer.SetActiveTelephone(null);
		}
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	public void ClearCurrentUser(RPCMessage msg)
	{
		Controller.ClearCurrentUser(msg);
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	public void SetCurrentUser(RPCMessage msg)
	{
		Controller.SetCurrentUser(msg);
	}

	[RPC_Server.FromOwner]
	[RPC_Server]
	public void InitiateCall(RPCMessage msg)
	{
		Controller.InitiateCall(msg);
	}

	[RPC_Server.FromOwner]
	[RPC_Server]
	public void AnswerPhone(RPCMessage msg)
	{
		Controller.AnswerPhone(msg);
	}

	[RPC_Server.FromOwner]
	[RPC_Server]
	private void ServerHangUp(RPCMessage msg)
	{
		Controller.ServerHangUp(msg);
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		Controller.DestroyShared();
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.FromOwner]
	[RPC_Server]
	public void UpdatePhoneName(RPCMessage msg)
	{
		Controller.RPC_UpdatePhoneName(msg);
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.FromOwner]
	public void Server_RequestPhoneDirectory(RPCMessage msg)
	{
		Controller.Server_RequestPhoneDirectory(msg);
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.FromOwner]
	[RPC_Server]
	public void Server_AddSavedNumber(RPCMessage msg)
	{
		Controller.Server_AddSavedNumber(msg);
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.FromOwner]
	public void Server_RemoveSavedNumber(RPCMessage msg)
	{
		Controller.Server_RemoveSavedNumber(msg);
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	public void Server_RequestCurrentState(RPCMessage msg)
	{
		Controller.SetPhoneStateWithPlayer(Controller.serverState);
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	public void ServerSendVoicemail(RPCMessage msg)
	{
		Controller.ServerSendVoicemail(msg);
	}

	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	public void ServerPlayVoicemail(RPCMessage msg)
	{
		Controller.ServerPlayVoicemail(msg);
	}

	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	public void ServerStopVoicemail(RPCMessage msg)
	{
		Controller.ServerStopVoicemail(msg);
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	[RPC_Server.FromOwner]
	public void ServerDeleteVoicemail(RPCMessage msg)
	{
		Controller.ServerDeleteVoicemail(msg);
	}

	public void ToggleRinging(bool state)
	{
		MobileInventoryEntity associatedEntity = ItemModAssociatedEntity<MobileInventoryEntity>.GetAssociatedEntity(GetItem());
		if ((Object)(object)associatedEntity != (Object)null)
		{
			associatedEntity.ToggleRinging(state);
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg?.telephone != null)
		{
			Controller.PhoneNumber = info.msg.telephone.phoneNumber;
			Controller.PhoneName = info.msg.telephone.phoneName;
			Controller.lastDialedNumber = info.msg.telephone.lastNumber;
			Controller.currentPlayerRef.uid = info.msg.telephone.usingPlayer;
			PhoneDirectory savedNumbers = Controller.savedNumbers;
			if (savedNumbers != null)
			{
				savedNumbers.ResetToPool();
			}
			Controller.savedNumbers = info.msg.telephone.savedNumbers;
			if (Controller.savedNumbers != null)
			{
				Controller.savedNumbers.ShouldPool = false;
			}
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (base.isServer && (old & Flags.Busy) == Flags.Busy != ((next & Flags.Busy) == Flags.Busy))
		{
			if ((next & Flags.Busy) == Flags.Busy)
			{
				if (!IsInvoking(Controller.WatchForDisconnects))
				{
					InvokeRepeating(Controller.WatchForDisconnects, 0f, 0.1f);
				}
			}
			else
			{
				CancelInvoke(Controller.WatchForDisconnects);
			}
		}
		Controller.OnFlagsChanged(old, next);
	}
}

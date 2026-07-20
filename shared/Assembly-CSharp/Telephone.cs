using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

[DefaultExecutionOrder(-1300)]
public class Telephone : ContainerIOEntity, ICassettePlayer
{
	public enum CallState
	{
		Idle,
		Dialing,
		Ringing,
		InProcess
	}

	public enum DialFailReason
	{
		TimedOut,
		Engaged,
		WrongNumber,
		CallSelf,
		RemoteHangUp,
		NetworkBusy,
		TimeOutDuringCall,
		SelfHangUp
	}

	public const Flags Flag_ApartmentStaticPhone = Flags.Reserved11;

	public const int MaxPhoneNameLength = 30;

	public const int MaxSavedNumbers = 10;

	public Transform PhoneHotspot;

	public Transform AnsweringMachineHotspot;

	public GameObject HandsetRoot;

	public ItemDefinition[] ValidCassettes;

	public Transform ParentedHandsetTransform;

	public LineRenderer CableLineRenderer;

	public Transform CableStartPoint;

	public Transform CableEndPoint;

	public float LineDroopAmount = 0.25f;

	public PhoneController Controller;

	public AnimationSubSystem AnimationSubSystem;

	public uint AnsweringMessageId
	{
		get
		{
			if (!((Object)(object)cachedCassette != (Object)null))
			{
				return 0u;
			}
			return cachedCassette.AudioId;
		}
	}

	public Cassette cachedCassette { get; private set; }

	public BaseEntity ToBaseEntity => this;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Telephone.OnRpcMessage"))
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
						if (!RPC_Server.MaxDistance.Test(1529322558u, "AnswerPhone", this, player, 3f))
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
						if (!RPC_Server.MaxDistance.Test(2754362156u, "ClearCurrentUser", this, player, 9f))
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
						if (!RPC_Server.MaxDistance.Test(1095090232u, "InitiateCall", this, player, 3f))
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
						if (!RPC_Server.MaxDistance.Test(2606442785u, "Server_AddSavedNumber", this, player, 3f))
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
						if (!RPC_Server.MaxDistance.Test(1402406333u, "Server_RemoveSavedNumber", this, player, 3f))
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
						if (!RPC_Server.MaxDistance.Test(942544266u, "Server_RequestPhoneDirectory", this, player, 3f))
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
							Server_RequestPhoneDirectory(msg7);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
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
						if (!RPC_Server.IsVisible.Test(1240133378u, "ServerDeleteVoicemail", this, player, 3f))
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
							ServerDeleteVoicemail(msg8);
						}
					}
					catch (Exception ex7)
					{
						Debug.LogException(ex7);
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
							ServerHangUp(msg9);
						}
					}
					catch (Exception ex8)
					{
						Debug.LogException(ex8);
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
						if (!RPC_Server.IsVisible.Test(239260010u, "ServerPlayVoicemail", this, player, 3f))
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
							ServerPlayVoicemail(msg10);
						}
					}
					catch (Exception ex9)
					{
						Debug.LogException(ex9);
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
							RPCMessage msg11 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ServerSendVoicemail(msg11);
						}
					}
					catch (Exception ex10)
					{
						Debug.LogException(ex10);
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
						if (!RPC_Server.IsVisible.Test(2760189029u, "ServerStopVoicemail", this, player, 3f))
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
							ServerStopVoicemail(msg12);
						}
					}
					catch (Exception ex11)
					{
						Debug.LogException(ex11);
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
						if (!RPC_Server.MaxDistance.Test(3900772076u, "SetCurrentUser", this, player, 3f))
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
					catch (Exception ex12)
					{
						Debug.LogException(ex12);
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
						if (!RPC_Server.MaxDistance.Test(2760249627u, "UpdatePhoneName", this, player, 3f))
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
							UpdatePhoneName(msg13);
						}
					}
					catch (Exception ex13)
					{
						Debug.LogException(ex13);
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
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (info.msg.telephone == null)
		{
			info.msg.telephone = Pool.Get<Telephone>();
		}
		info.msg.telephone.phoneNumber = Controller.PhoneNumber;
		info.msg.telephone.phoneName = Controller.PhoneName;
		info.msg.telephone.lastNumber = Controller.lastDialedNumber;
		info.msg.telephone.savedNumbers = Controller.savedNumbers;
		if (Controller.savedVoicemail != null)
		{
			info.msg.telephone.voicemail = Pool.Get<List<VoicemailEntry>>();
			foreach (VoicemailEntry item in Controller.savedVoicemail)
			{
				info.msg.telephone.voicemail.Add(item);
			}
		}
		if (!info.forDisk)
		{
			info.msg.telephone.usingPlayer = Controller.currentPlayerRef.uid;
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		Controller.ServerInit();
		ItemContainer itemContainer = base.inventory;
		itemContainer.canAcceptItem = (Func<Item, int, bool>)Delegate.Combine(itemContainer.canAcceptItem, new Func<Item, int, bool>(CanAcceptItem));
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

	[RPC_Server.MaxDistance(9f)]
	[RPC_Server]
	public void ClearCurrentUser(RPCMessage msg)
	{
		Controller.ClearCurrentUser(msg);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void SetCurrentUser(RPCMessage msg)
	{
		Controller.SetCurrentUser(msg);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void InitiateCall(RPCMessage msg)
	{
		Controller.InitiateCall(msg);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void AnswerPhone(RPCMessage msg)
	{
		Controller.AnswerPhone(msg);
	}

	[RPC_Server]
	private void ServerHangUp(RPCMessage msg)
	{
		Controller.ServerHangUp(msg);
	}

	public void OnCassetteInserted(Cassette c)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		cachedCassette = c;
		ClientRPC(RpcTarget.NetworkGroup("ClientOnCassetteChanged"), c.net.ID);
	}

	public void OnCassetteRemoved(Cassette c)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		cachedCassette = null;
		Controller.DeleteAllVoicemail();
		((BaseEntity)this).ClientRPC(RpcTarget.NetworkGroup("ClientOnCassetteChanged"), default(NetworkableId));
	}

	private bool CanAcceptItem(Item item, int targetSlot)
	{
		ItemDefinition[] validCassettes = ValidCassettes;
		for (int i = 0; i < validCassettes.Length; i++)
		{
			if ((Object)(object)validCassettes[i] == (Object)(object)item.info)
			{
				return true;
			}
		}
		return false;
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.MaxDistance(3f)]
	public void UpdatePhoneName(RPCMessage msg)
	{
		Controller.RPC_UpdatePhoneName(msg);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	public void Server_RequestPhoneDirectory(RPCMessage msg)
	{
		Controller.Server_RequestPhoneDirectory(msg);
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void Server_AddSavedNumber(RPCMessage msg)
	{
		Controller.Server_AddSavedNumber(msg);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	public void Server_RemoveSavedNumber(RPCMessage msg)
	{
		Controller.Server_RemoveSavedNumber(msg);
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	public void ServerSendVoicemail(RPCMessage msg)
	{
		Controller.ServerSendVoicemail(msg);
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void ServerPlayVoicemail(RPCMessage msg)
	{
		Controller.ServerPlayVoicemail(msg);
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void ServerStopVoicemail(RPCMessage msg)
	{
		Controller.ServerStopVoicemail(msg);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	public void ServerDeleteVoicemail(RPCMessage msg)
	{
		Controller.ServerDeleteVoicemail(msg);
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (Controller.serverState == CallState.Ringing || Controller.serverState == CallState.InProcess)
		{
			return base.GetPassthroughAmount(outputSlot);
		}
		return 0;
	}

	public override void Load(LoadInfo info)
	{
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg?.telephone == null)
		{
			return;
		}
		Controller.PhoneNumber = info.msg.telephone.phoneNumber;
		Controller.PhoneName = info.msg.telephone.phoneName;
		Controller.lastDialedNumber = info.msg.telephone.lastNumber;
		Controller.savedVoicemail = Pool.Get<List<VoicemailEntry>>();
		foreach (VoicemailEntry item in info.msg.telephone.voicemail)
		{
			Controller.savedVoicemail.Add(item);
			item.ShouldPool = false;
		}
		if (!info.fromDisk)
		{
			Controller.currentPlayerRef.uid = info.msg.telephone.usingPlayer;
		}
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
		if (info.fromDisk)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Busy, b: false);
			}
		}
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if ((Object)(object)Controller.currentPlayer != (Object)null)
		{
			pickupErrorToFormat = (format: PickupErrors.ItemIsBeingUsed, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (base.isServer)
		{
			if (Controller.RequirePower && (next & Flags.Busy) == Flags.Busy && (next & Flags.Reserved8) != Flags.Reserved8)
			{
				Controller.ServerHangUp();
			}
			if ((old & Flags.Busy) == Flags.Busy != ((next & Flags.Busy) == Flags.Busy))
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
		}
		Controller.OnFlagsChanged(old, next);
	}
}

using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using GameMenu;
using Network;
using Oxide.Core;
using ProtoBuf;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class ApartmentDoor : Door
{
	public Transform KeyPosTransform;

	public static readonly Phrase Phrase_InsertKey = new Phrase("apartment_insert_key", "Insert Key");

	public static readonly Phrase Phrase_Unoccupied = new Phrase("apartment_unoccupied", "Unoccupied");

	public static readonly Phrase Phrase_BreakInAlreadyAuthed = new Phrase("apartment_breakin_already_authed", "You already have access to this room");

	public static readonly Phrase Phrase_BreakInUnoccupied = new Phrase("apartment_breakin_unoccupied", "This room is unoccupied");

	public static readonly Phrase Phrase_BreakInSuccess = new Phrase("apartment_breakin_success", "You broke into the room and have temporary access");

	public TextMeshPro RoomNumberLabel;

	public static readonly Flags Flag_BreakInActive = Flags.Reserved5;

	public SoundDefinition BreakInJingleSound;

	private Sound breakInJingleLoop;

	private Dictionary<ulong, TimeSince> breakInStarts = new Dictionary<ulong, TimeSince>();

	public NetworkableId ApartmentId { get; set; }

	public string RoomNumber { get; set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ApartmentDoor.OnRpcMessage"))
		{
			if (rpc == 4285583781u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - BreakIn"));
				}
				using (TimeWarning.New("BreakIn"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4285583781u, "BreakIn", this, player, 3f))
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
							BreakIn(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in BreakIn");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private void SetBreakInAnimation3rdPerson(BasePlayer player, bool state)
	{
		MasterKey masterKey = player.GetHeldEntity() as MasterKey;
		if ((Object)(object)masterKey != (Object)null)
		{
			using (FlagsUpdateScope flagsUpdateScope = masterKey.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags))
			{
				flagsUpdateScope.Set(MasterKey.Flag_BreakingIn, state);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void BreakIn(RPCMessage rpc)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		RPCProgressBarState rPCProgressBarState = (RPCProgressBarState)rpc.read.Int32();
		BasePlayer player = rpc.player;
		if (!((Object)(object)player == (Object)null) && player.CanInteract())
		{
			switch (rPCProgressBarState)
			{
			case RPCProgressBarState.Start:
				breakInStarts[player.userID] = TimeSince.op_Implicit(0f);
				StartBreakInSound();
				SetBreakInAnimation3rdPerson(player, state: true);
				break;
			case RPCProgressBarState.Cancel:
				breakInStarts.Remove(player.userID);
				StopBreakInSound();
				SetBreakInAnimation3rdPerson(player, state: false);
				break;
			case RPCProgressBarState.Complete:
				CompleteBreakIn(player);
				break;
			}
		}
	}

	private void StartBreakInSound()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags))
		{
			flagsUpdateScope.Set(Flag_BreakInActive, b: true);
		}
		CancelInvoke(BreakInCancelSafeguard);
		Invoke(BreakInCancelSafeguard, ApartmentCommands.breakinseconds + 5f);
	}

	private void StopBreakInSound()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flag_BreakInActive, b: false);
	}

	private void BreakInCancelSafeguard()
	{
		StopBreakInSound();
		foreach (ulong key in breakInStarts.Keys)
		{
			BasePlayer basePlayer = BasePlayer.FindByID(key);
			if (!((Object)(object)basePlayer == (Object)null))
			{
				SetBreakInAnimation3rdPerson(basePlayer, state: false);
			}
		}
	}

	private void CompleteBreakIn(BasePlayer player)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!breakInStarts.TryGetValue(player.userID, out var value))
		{
			return;
		}
		breakInStarts.Remove(player.userID);
		SetBreakInAnimation3rdPerson(player, state: false);
		if (TimeSince.op_Implicit(value) < ApartmentCommands.breakinseconds * 0.9f)
		{
			return;
		}
		ApartmentRoom apartmentRoom = BaseNetworkable.serverEntities.Find(ApartmentId) as ApartmentRoom;
		if (Interface.CallHook("OnApartmentRoomBreakInComplete", apartmentRoom, player, this) != null)
		{
			return;
		}
		if (!apartmentRoom.IsCurrentlyRented())
		{
			player.ShowToast(GameTip.Styles.Red_Normal, Phrase_BreakInUnoccupied, false);
			return;
		}
		if (apartmentRoom.IsAuthed(player.userID) || apartmentRoom.IsBreakInActive())
		{
			player.ShowToast(GameTip.Styles.Red_Normal, Phrase_BreakInAlreadyAuthed, false);
			return;
		}
		Item activeItem = player.GetActiveItem();
		if (activeItem != null && !((Object)(object)activeItem.info != (Object)(object)ItemManager.Items.MasterKey) && activeItem.amount >= 1)
		{
			activeItem.UseItem();
			apartmentRoom.StartBreakIn();
			player.ShowToast(GameTip.Styles.Blue_Long, Phrase_BreakInSuccess, false);
			StopBreakInSound();
			Facepunch.Rust.Analytics.Azure.OnApartmentBreakIn(player, apartmentRoom);
			Interface.CallHook("OnApartmentRoomBreakInCompleted", apartmentRoom, player, this);
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.apartmentDoor = Pool.Get<ApartmentDoor>();
		info.msg.apartmentDoor.roomNumber = RoomNumber;
		info.msg.apartmentDoor.apartmentId = ApartmentId;
	}

	public override void Load(LoadInfo info)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (base.isServer)
		{
			SetFlagLocal(Flag_BreakInActive, b: false);
		}
		if (info.msg.apartmentDoor != null)
		{
			RoomNumber = info.msg.apartmentDoor.roomNumber;
			ApartmentId = info.msg.apartmentDoor.apartmentId;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;

public class ApartmentVendor : NPCTalking
{
	public enum ApartmentConversationUpgrade
	{
		None,
		Small_Medium,
		Small_Large,
		Medium_Large,
		Last
	}

	public static Phrase Phrase_LargeApartment = new Phrase("apartment.penthouse", "Penthouse");

	public static Phrase Phrase_MediumApartment = new Phrase("apartment.standard", "Standard");

	public static Phrase Phrase_SmallApartment = new Phrase("apartment.basement", "Basement");

	public static Phrase Phrase_UnknownApartment = new Phrase("apartment.unknown", "Unknown");

	private EntityRef<ApartmentBuilding> __sync_BuildingRef;

	[Sync(Autosave = true)]
	public EntityRef<ApartmentBuilding> BuildingRef
	{
		[CompilerGenerated]
		get
		{
			return __sync_BuildingRef;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_BuildingRef, value))
			{
				__sync_BuildingRef = value;
				byte nameID = __GetWeaverID("BuildingRef");
				QueueSyncVar(nameID);
			}
		}
	}

	private static Phrase GetApartmentSizePhrase(ApartmentSize size)
	{
		return size switch
		{
			ApartmentSize.Small => Phrase_SmallApartment, 
			ApartmentSize.Medium => Phrase_MediumApartment, 
			ApartmentSize.Large => Phrase_LargeApartment, 
			_ => Phrase_UnknownApartment, 
		};
	}

	public override void AddFormatTextValues(BasePlayer player, ConversationData.AbstractSpeechNodeData speechNode, List<FormatTextValue> values)
	{
		base.AddFormatTextValues(player, speechNode, values);
		if (!speechNode.UsesFormatText)
		{
			return;
		}
		ApartmentBuilding apartmentBuilding = BuildingRef.Get(serverside: true);
		if ((Object)(object)apartmentBuilding == (Object)null)
		{
			Debug.LogWarning((object)string.Format("{0}.{1}: BuildingRef is null for vendor {2}", "ApartmentVendor", "AddFormatTextValues", this), (Object)(object)this);
			return;
		}
		int purchaseScrapCost = apartmentBuilding.GetPurchaseScrapCost(ApartmentSize.Small);
		int purchaseScrapCost2 = apartmentBuilding.GetPurchaseScrapCost(ApartmentSize.Medium);
		int purchaseScrapCost3 = apartmentBuilding.GetPurchaseScrapCost(ApartmentSize.Large);
		int upkeepCost = apartmentBuilding.GetUpkeepCost(ApartmentSize.Small);
		int upkeepCost2 = apartmentBuilding.GetUpkeepCost(ApartmentSize.Medium);
		int upkeepCost3 = apartmentBuilding.GetUpkeepCost(ApartmentSize.Large);
		AddFormatTextValue(values, speechNode, "small_cost", purchaseScrapCost);
		AddFormatTextValue(values, speechNode, "medium_cost", purchaseScrapCost2);
		AddFormatTextValue(values, speechNode, "large_cost", purchaseScrapCost3);
		AddFormatTextValue(values, speechNode, "small_upkeep", upkeepCost);
		AddFormatTextValue(values, speechNode, "medium_upkeep", upkeepCost2);
		AddFormatTextValue(values, speechNode, "large_upkeep", upkeepCost3);
		AddFormatTextValue(values, speechNode, "small_left", apartmentBuilding.GetRemainingRoomCount(ApartmentSize.Small));
		AddFormatTextValue(values, speechNode, "medium_left", apartmentBuilding.GetRemainingRoomCount(ApartmentSize.Medium));
		AddFormatTextValue(values, speechNode, "large_left", apartmentBuilding.GetRemainingRoomCount(ApartmentSize.Large));
		AddFormatTextValue(values, speechNode, "small_medium_upgrade", purchaseScrapCost2 - purchaseScrapCost);
		AddFormatTextValue(values, speechNode, "medium_large_upgrade", purchaseScrapCost3 - purchaseScrapCost2);
		AddFormatTextValue(values, speechNode, "small_large_upgrade", purchaseScrapCost3 - purchaseScrapCost);
		ApartmentRoom playerApartment = apartmentBuilding.GetPlayerApartment(player);
		if ((Object)(object)playerApartment != (Object)null)
		{
			AddFormatTextValue(values, speechNode, "my_room_number", playerApartment.RoomNumber);
			AddFormatTextValue(values, speechNode, "my_rent", playerApartment.MinimumRent);
			AddFormatTextValue(values, speechNode, "my_apartment_size", GetApartmentSizePhrase(playerApartment.Size).token, valueIsPhraseToken: true);
		}
	}

	public override void OnConversationAction(BasePlayer player, string action)
	{
		base.OnConversationAction(player, action);
		ApartmentBuilding apartmentBuilding = BuildingRef.Get(serverside: true);
		switch (action)
		{
		case "PurchaseSmall":
			TryPurchaseApartment(player, ApartmentSize.Small);
			break;
		case "PurchaseMedium":
			TryPurchaseApartment(player, ApartmentSize.Medium);
			break;
		case "PurchaseLarge":
			TryPurchaseApartment(player, ApartmentSize.Large);
			break;
		case "UpgradeMedium":
			apartmentBuilding.TryUpgradeRoom(player, ApartmentSize.Medium);
			break;
		case "UpgradeLarge":
			apartmentBuilding.TryUpgradeRoom(player, ApartmentSize.Large);
			break;
		case "Checkout":
			apartmentBuilding.TryCheckout(player);
			break;
		}
	}

	private void TryPurchaseApartment(BasePlayer player, ApartmentSize size)
	{
		BuildingRef.Get(serverside: true).TryPurchaseRoom(player, size);
	}

	public bool Conversation_CanUpgradeApartment(BasePlayer player, int size)
	{
		ApartmentBuilding apartmentBuilding = BuildingRef.Get(serverside: true);
		if ((Object)(object)apartmentBuilding == (Object)null)
		{
			Debug.LogError((object)$"ApartmentVendor.Conversation_CanUpgradeApartment: BuildingRef is null for vendor {this}", (Object)(object)this);
			return false;
		}
		switch ((ApartmentSize)size)
		{
		case ApartmentSize.Small:
			return apartmentBuilding.CanUpgradeRoom(player, ApartmentSize.Small);
		case ApartmentSize.Medium:
			return apartmentBuilding.CanUpgradeRoom(player, ApartmentSize.Medium);
		case ApartmentSize.Large:
			return apartmentBuilding.CanUpgradeRoom(player, ApartmentSize.Large);
		case ApartmentSize.Any:
		{
			ApartmentSize playerApartmentSize = apartmentBuilding.GetPlayerApartmentSize(player);
			return playerApartmentSize == ApartmentSize.Small || playerApartmentSize == ApartmentSize.Medium;
		}
		default:
			return false;
		}
	}

	public bool Conversation_CanPurchaseApartment(BasePlayer player, int size)
	{
		ApartmentBuilding apartmentBuilding = BuildingRef.Get(serverside: true);
		if ((Object)(object)apartmentBuilding == (Object)null)
		{
			Debug.LogError((object)$"ApartmentVendor.Conversation_CanPurchaseApartment: BuildingRef is null for vendor {this}", (Object)(object)this);
			return false;
		}
		return (ApartmentSize)size switch
		{
			ApartmentSize.Small => apartmentBuilding.CanBuyRoom(player, ApartmentSize.Small), 
			ApartmentSize.Medium => apartmentBuilding.CanBuyRoom(player, ApartmentSize.Medium), 
			ApartmentSize.Large => apartmentBuilding.CanBuyRoom(player, ApartmentSize.Large), 
			ApartmentSize.Any => apartmentBuilding.GetPlayerApartmentSize(player) == ApartmentSize.None, 
			_ => false, 
		};
	}

	public bool Conversation_CanCheckoutApartment(BasePlayer player)
	{
		if (!TryGetBuilding(out var building, "Conversation_CanCheckoutApartment"))
		{
			return false;
		}
		return building.GetPlayerApartmentSize(player) != ApartmentSize.None;
	}

	public bool Conversation_OwnsApartment(BasePlayer player, ApartmentSize size)
	{
		if (!TryGetBuilding(out var building, "Conversation_OwnsApartment"))
		{
			return false;
		}
		ApartmentSize playerApartmentSize = building.GetPlayerApartmentSize(player);
		if (size == ApartmentSize.Any)
		{
			return playerApartmentSize != ApartmentSize.None;
		}
		return playerApartmentSize == size;
	}

	public bool Conversation_CanAffordApartment(BasePlayer player, ApartmentSize size)
	{
		if (!TryGetBuilding(out var building, "Conversation_CanAffordApartment"))
		{
			return false;
		}
		return building.CanAffordRoom(player, size);
	}

	public bool Conversation_CanAffordUpgrade(BasePlayer player, ApartmentConversationUpgrade upgrade)
	{
		if (!TryGetBuilding(out var building, "Conversation_CanAffordUpgrade"))
		{
			return false;
		}
		return upgrade switch
		{
			ApartmentConversationUpgrade.Small_Medium => building.CanAffordUpgrade(player, ApartmentSize.Small, ApartmentSize.Medium), 
			ApartmentConversationUpgrade.Small_Large => building.CanAffordUpgrade(player, ApartmentSize.Small, ApartmentSize.Large), 
			ApartmentConversationUpgrade.Medium_Large => building.CanAffordUpgrade(player, ApartmentSize.Medium, ApartmentSize.Large), 
			_ => false, 
		};
	}

	public bool Conversation_HasAnyRoomAvailable()
	{
		if (!TryGetBuilding(out var building, "Conversation_HasAnyRoomAvailable"))
		{
			return false;
		}
		if (building.GetRemainingRoomCount(ApartmentSize.Small) <= 0 && building.GetRemainingRoomCount(ApartmentSize.Medium) <= 0)
		{
			return building.GetRemainingRoomCount(ApartmentSize.Large) > 0;
		}
		return true;
	}

	public bool TryGetBuilding(out ApartmentBuilding building, string logErrorPrefix = "")
	{
		if (!BuildingRef.TryGet(serverside: true, out building))
		{
			string arg = (string.IsNullOrWhiteSpace(logErrorPrefix) ? string.Empty : ("ApartmentVendor." + logErrorPrefix + ": "));
			Debug.LogError((object)$"{arg} BuildingRef is null for vendor {this}", (Object)(object)this);
			return false;
		}
		return true;
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (id == 1)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: BuildingRef for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_BuildingRef);
			return true;
		}
		return base.WriteSyncVar(id, writer);
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		if (id == 1)
		{
			try
			{
				_ = __sync_BuildingRef;
				EntityRef<ApartmentBuilding> _sync_BuildingRef = NetworkReadEx.EntityRef<ApartmentBuilding>(reader);
				__sync_BuildingRef = _sync_BuildingRef;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		}
		return base.OnSyncVar(id, reader, fromAutoSave);
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (propertyName == "BuildingRef")
		{
			return 1;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(1, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(1, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		WriteAutoSaveSyncVars(netWrite);
		(byte[] Buffer, int Length) buffer = netWrite.GetBuffer();
		byte[] item = buffer.Buffer;
		int item2 = buffer.Length;
		byte[] array = _autosaveBuffer;
		if (array == null || array.Length < item2)
		{
			byte[] array2 = BaseEntity._autosaveBufferPool.Rent(item2);
			while (array == null || array.Length < item2)
			{
				byte[] array3 = Interlocked.CompareExchange(ref _autosaveBuffer, array2, array);
				if (array3 == array)
				{
					if (array3 != null)
					{
						BaseEntity._autosaveBufferPool.Return(array3);
					}
					array = array2;
					break;
				}
				array = array3;
			}
			if (array != array2)
			{
				BaseEntity._autosaveBufferPool.Return(array2);
			}
		}
		Buffer.BlockCopy(item, 0, array, 0, item2);
		save.msg.baseEntity.syncVars = array;
		Pool.Free<NetWrite>(ref netWrite);
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		if (load.msg.baseEntity != null && load.msg.baseEntity.syncVars != null)
		{
			NetRead netRead = Pool.Get<NetRead>();
			netRead.Init(load.msg.baseEntity.syncVars.AsSpan());
			ReadAutoSaveSyncVars(netRead);
			Pool.Free<NetRead>(ref netRead);
		}
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_BuildingRef = default;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 1)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}

	static ApartmentVendor()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected Obj, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected Obj, but got Unknown
	}
}

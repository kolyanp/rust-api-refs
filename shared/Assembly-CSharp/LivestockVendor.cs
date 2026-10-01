using System;
using System.Collections.Generic;
using ConVar;
using Facepunch.Rust;
using Network;
using Rust.Ai.Gen2;
using UnityEngine;

public class LivestockVendor : NPCTalking
{
	public enum SaleKind : byte
	{
		Animal,
		Wool
	}

	public struct OfferItem
	{
		public ItemDefinition item;

		public int amount;
	}

	public struct Offer
	{
		public SaleKind kind;

		public NetworkableId subjectId;

		public string subjectName;

		public int lotAmount;

		public OfferItem[] items;

		public bool generous;

		public int index;

		public int count;

		public bool IsValid
		{
			get
			{
				if (items != null)
				{
					return items.Length != 0;
				}
				return false;
			}
		}

		public bool IsFinalOffer => index >= count - 1;
	}

	private class Negotiation
	{
		public SaleKind kind;

		public LivestockSaleTable table;

		public int lotAmount;

		public TimeSince lastActivity;

		public int rejections;

		public Offer[] offers;

		public bool Refused
		{
			get
			{
				if (offers != null)
				{
					return rejections >= offers.Length;
				}
				return false;
			}
		}
	}

	public struct AnimalListing
	{
		public NetworkableId animalId;

		public string animalName;
	}

	public const int MaxOfferItems = 3;

	private readonly Dictionary<NetworkableId, Negotiation> negotiations = new Dictionary<NetworkableId, Negotiation>();

	private readonly Dictionary<ulong, NetworkableId> selectedSubjects = new Dictionary<ulong, NetworkableId>();

	private readonly List<OfferItem> pickedItems = new List<OfferItem>();

	private readonly List<NetworkableId> expired = new List<NetworkableId>();

	public LivestockTrigger Trigger;

	[Tooltip("The items this vendor pays with, banded against how good the animal is.")]
	public LivestockSaleTable saleTable;

	[Tooltip("The items this vendor pays with for wool, banded against how full the lot is.")]
	public LivestockSaleTable woolSaleTable;

	[Tooltip("The wool this vendor buys. Held as a definition rather than a shortname so the item can still be renamed.")]
	public ItemDefinition woolItem;

	public GameObjectRef MaleLambPrefab;

	public GameObjectRef FemaleLambPrefab;

	public GameObjectRef MaleCalfPrefab;

	public GameObjectRef FemaleCalfPrefab;

	private readonly List<AnimalListing> listingBuffer = new List<AnimalListing>();

	private static int WoolLotSize => Mathf.Max(1, Livestock.vendorWoolLotSize);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("LivestockVendor.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool Server_IsAnnoyedAboutSelection(BasePlayer player)
	{
		if (TryGetSelected(player, out var negotiation, out var _))
		{
			return negotiation.Refused;
		}
		return false;
	}

	public bool Server_HasOfferRemaining(BasePlayer player)
	{
		if (TryGetSelected(player, out var negotiation, out var _))
		{
			return !negotiation.Refused;
		}
		return false;
	}

	public bool Server_HasSaleChanged(BasePlayer player)
	{
		if (TryGetSelected(player, out var negotiation, out var _) && !negotiation.Refused)
		{
			return IsShortOfLot(player, negotiation);
		}
		return false;
	}

	public int Server_WoolCarriedBy(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null || (Object)(object)player.inventory == (Object)null || (Object)(object)woolItem == (Object)null)
		{
			return 0;
		}
		return player.inventory.GetAmount(woolItem);
	}

	public bool Server_HasWoolToSell(BasePlayer player)
	{
		if ((Object)(object)woolSaleTable != (Object)null)
		{
			return Server_WoolCarriedBy(player) >= Livestock.vendorWoolMinimum;
		}
		return false;
	}

	public bool Server_SelectAnimal(BasePlayer player, NetworkableId animalId)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (!TryFindSellable(animalId, out var found) || !found.IsFollowing(player))
		{
			return false;
		}
		PruneNegotiations();
		if (!negotiations.TryGetValue(animalId, out var value))
		{
			value = new Negotiation
			{
				kind = SaleKind.Animal,
				table = saleTable
			};
			value.offers = RollOffers(value, animalId, NameOf(found), found.SaleQuality);
			negotiations.Add(animalId, value);
		}
		selectedSubjects[player.userID] = animalId;
		return PresentOffer(player, animalId, value);
	}

	public bool Server_SelectWool(BasePlayer player)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null || player.net == null || (Object)(object)woolItem == (Object)null)
		{
			return false;
		}
		NetworkableId iD = player.net.ID;
		PruneNegotiations();
		if (!negotiations.TryGetValue(iD, out var value))
		{
			int num = WoolLotFor(player);
			if (num < Livestock.vendorWoolMinimum)
			{
				selectedSubjects.Remove(player.userID);
				return false;
			}
			value = new Negotiation
			{
				kind = SaleKind.Wool,
				table = woolSaleTable,
				lotAmount = num
			};
			value.offers = RollOffers(value, iD, WoolLotName(num), WoolQuality(num));
			negotiations.Add(iD, value);
		}
		selectedSubjects[player.userID] = iD;
		return PresentOffer(player, iD, value);
	}

	public void Server_RejectOffer(BasePlayer player)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetSelected(player, out var negotiation, out var subjectId) && !negotiation.Refused)
		{
			negotiation.rejections++;
			negotiation.lastActivity = TimeSince.op_Implicit(0f);
			if (!negotiation.Refused)
			{
				SendOffer(player, subjectId, negotiation);
			}
		}
	}

	public bool Server_AcceptOffer(BasePlayer player)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetSelected(player, out var negotiation, out var subjectId) || negotiation.Refused)
		{
			return false;
		}
		Offer offer = negotiation.offers[negotiation.rejections];
		if (!offer.IsValid)
		{
			return false;
		}
		if (!TryTakeGoods(player, negotiation, subjectId))
		{
			return false;
		}
		OfferItem[] items = offer.items;
		for (int i = 0; i < items.Length; i++)
		{
			OfferItem offerItem = items[i];
			if (!((Object)(object)offerItem.item == (Object)null) && offerItem.amount > 0)
			{
				Item item = ItemManager.Create(offerItem.item, offerItem.amount, 0uL, isServerSide: true, 0uL);
				item.SetItemOwnership(player, ItemOwnershipPhrases.VendorSale);
				player.GiveItem(item);
			}
		}
		Analytics.Azure.OnLivestockSale(player, negotiation.kind, offer, negotiation.rejections);
		negotiations.Remove(subjectId);
		selectedSubjects.Remove(player.userID);
		return true;
	}

	public void Server_ClearSelection(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null)
		{
			selectedSubjects.Remove(player.userID);
		}
	}

	private bool TryTakeGoods(BasePlayer player, Negotiation negotiation, NetworkableId subjectId)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (negotiation.kind == SaleKind.Wool)
		{
			if ((Object)(object)woolItem == (Object)null || IsShortOfLot(player, negotiation))
			{
				return false;
			}
			player.inventory.UseAmount(woolItem, negotiation.lotAmount);
			return true;
		}
		if (!TryFindSellable(subjectId, out var found) || !found.IsFollowing(player))
		{
			return false;
		}
		found.Kill();
		return true;
	}

	private bool PresentOffer(BasePlayer player, NetworkableId subjectId, Negotiation negotiation)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (negotiation.Refused)
		{
			return true;
		}
		if (!IsShortOfLot(player, negotiation))
		{
			negotiation.lastActivity = TimeSince.op_Implicit(0f);
		}
		SendOffer(player, subjectId, negotiation);
		return true;
	}

	private bool IsShortOfLot(BasePlayer player, Negotiation negotiation)
	{
		if (negotiation.kind == SaleKind.Wool)
		{
			return Server_WoolCarriedBy(player) < negotiation.lotAmount;
		}
		return false;
	}

	private int WoolLotFor(BasePlayer player)
	{
		return Mathf.Min(Server_WoolCarriedBy(player), WoolLotSize);
	}

	private float WoolQuality(int lot)
	{
		return Mathf.Clamp01((float)lot / (float)WoolLotSize);
	}

	private string WoolLotName(int lot)
	{
		if (!((Object)(object)woolItem != (Object)null))
		{
			return string.Empty;
		}
		return $"{lot} {woolItem.shortname}";
	}

	private bool TryGetSelected(BasePlayer player, out Negotiation negotiation, out NetworkableId subjectId)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		negotiation = null;
		subjectId = default;
		if ((Object)(object)player == (Object)null || !selectedSubjects.TryGetValue(player.userID, out subjectId))
		{
			return false;
		}
		if (negotiations.TryGetValue(subjectId, out negotiation))
		{
			return negotiation.offers != null;
		}
		return false;
	}

	private void PruneNegotiations()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		expired.Clear();
		foreach (KeyValuePair<NetworkableId, Negotiation> negotiation in negotiations)
		{
			if (TimeSince.op_Implicit(negotiation.Value.lastActivity) > Livestock.vendorAnnoyedTime)
			{
				expired.Add(negotiation.Key);
			}
		}
		foreach (NetworkableId item in expired)
		{
			negotiations.Remove(item);
		}
		expired.Clear();
	}

	private Offer[] RollOffers(Negotiation negotiation, NetworkableId subjectId, string subjectName, float quality)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Max(1, Livestock.vendorOfferCount);
		Offer[] array = new Offer[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = RollOffer(negotiation, subjectId, subjectName, quality);
		}
		return array;
	}

	private Offer RollOffer(Negotiation negotiation, NetworkableId subjectId, string subjectName, float quality)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		bool flag = Random.value < Livestock.vendorGenerousChance;
		float amountToSpend = Mathf.Clamp01(flag ? (quality + Random.Range(Livestock.vendorGenerousMin, Livestock.vendorGenerousMax)) : (quality * (1f + Random.Range(0f - Livestock.vendorOfferVariance, Livestock.vendorOfferVariance))));
		return new Offer
		{
			kind = negotiation.kind,
			subjectId = subjectId,
			subjectName = subjectName,
			lotAmount = negotiation.lotAmount,
			items = PickItems(negotiation.table, amountToSpend),
			generous = flag
		};
	}

	private OfferItem[] PickItems(LivestockSaleTable table, float amountToSpend)
	{
		if ((Object)(object)table == (Object)null)
		{
			Debug.LogError((object)(((Object)this).name + " has no sale table for what it was asked to price, so it has nothing to offer"), (Object)(object)this);
			return Array.Empty<OfferItem>();
		}
		int picks = Mathf.Clamp(Livestock.vendorOfferItems, 0, 2);
		table.PickOffer(amountToSpend, picks, pickedItems);
		if (pickedItems.Count == 0)
		{
			Debug.LogWarning((object)$"{((Object)table).name} offers nothing for a budget of {amountToSpend:0.00}, so {((Object)this).name} has nothing to pay with", (Object)(object)this);
		}
		return pickedItems.ToArray();
	}

	private void SendOffer(BasePlayer player, NetworkableId subjectId, Negotiation negotiation)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)player == (Object)null) && player.net != null && player.net.connection != null)
		{
			Offer offer = negotiation.offers[negotiation.rejections];
			NetWrite netWrite = ClientRPCStart("Client_ReceiveAnimalOffer");
			netWrite.EntityID(subjectId);
			netWrite.UInt8((byte)negotiation.kind);
			netWrite.Int32(negotiation.lotAmount);
			netWrite.UInt8((byte)negotiation.rejections);
			netWrite.UInt8((byte)negotiation.offers.Length);
			netWrite.Bool(offer.generous);
			netWrite.UInt8((byte)offer.items.Length);
			OfferItem[] items = offer.items;
			for (int i = 0; i < items.Length; i++)
			{
				OfferItem offerItem = items[i];
				netWrite.Int32(((Object)(object)offerItem.item != (Object)null) ? offerItem.item.itemid : 0);
				netWrite.Int32(offerItem.amount);
			}
			ClientRPCSend(netWrite, new SendInfo(player.net.connection));
		}
	}

	public static bool IsSellable(LivestockAnimal animal)
	{
		if ((Object)(object)animal != (Object)null && !animal.IsDead() && animal.IsAdult())
		{
			return !animal.IsPregnant();
		}
		return false;
	}

	public static bool CanBeSoldBy(LivestockAnimal animal, BasePlayer player)
	{
		if (IsSellable(animal) && (Object)(object)player != (Object)null)
		{
			return animal.IsFollowing(player);
		}
		return false;
	}

	public bool TriggerHasAnimalsToSell(BasePlayer player)
	{
		if ((Object)(object)Trigger == (Object)null || Trigger.entityContents == null)
		{
			return false;
		}
		foreach (BaseEntity entityContent in Trigger.entityContents)
		{
			if (entityContent is LivestockAnimal animal && CanBeSoldBy(animal, player))
			{
				return true;
			}
		}
		return false;
	}

	public void GetAnimalListings(BasePlayer player, List<AnimalListing> buffer)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		buffer.Clear();
		if ((Object)(object)Trigger == (Object)null || Trigger.entityContents == null)
		{
			return;
		}
		foreach (BaseEntity entityContent in Trigger.entityContents)
		{
			if (!((Object)(object)entityContent == (Object)null) && entityContent is LivestockAnimal livestockAnimal && CanBeSoldBy(livestockAnimal, player))
			{
				buffer.Add(new AnimalListing
				{
					animalId = ((livestockAnimal.net != null) ? livestockAnimal.net.ID : default(NetworkableId)),
					animalName = NameOf(livestockAnimal)
				});
				if (buffer.Count >= 4)
				{
					break;
				}
			}
		}
	}

	public bool TryFindAnimal(NetworkableId animalId, out LivestockAnimal found)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		found = null;
		if (!animalId.IsValid || (Object)(object)Trigger == (Object)null || Trigger.entityContents == null)
		{
			return false;
		}
		foreach (BaseEntity entityContent in Trigger.entityContents)
		{
			if (!((Object)(object)entityContent == (Object)null) && entityContent.net != null && !(entityContent.net.ID != animalId))
			{
				found = entityContent as LivestockAnimal;
				return (Object)(object)found != (Object)null;
			}
		}
		return false;
	}

	public bool TryFindSellable(NetworkableId animalId, out LivestockAnimal found)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (TryFindAnimal(animalId, out found))
		{
			return IsSellable(found);
		}
		return false;
	}

	public static string NameOf(LivestockAnimal animal)
	{
		if ((Object)(object)animal == (Object)null)
		{
			return string.Empty;
		}
		if (!string.IsNullOrEmpty(animal.AnimalName))
		{
			return animal.AnimalName;
		}
		return animal.Categorize();
	}

	public int ReferenceValueFor(LivestockAnimal animal)
	{
		if ((Object)(object)animal == (Object)null || (Object)(object)saleTable == (Object)null)
		{
			return 0;
		}
		return saleTable.ReferenceValueFor(animal.SaleQuality);
	}

	public int SellForTest(LivestockAnimal animal)
	{
		if (!IsSellable(animal))
		{
			return 0;
		}
		int result = ReferenceValueFor(animal);
		animal.Kill();
		return result;
	}

	protected override void Server_OnSendingSpeechNode(BasePlayer player, ConversationData.AbstractSpeechNodeData speechNode)
	{
		base.Server_OnSendingSpeechNode(player, speechNode);
		if (speechNode is ConversationData.SellAnimalsSpeechNodeData)
		{
			Server_SendAnimalListings(player);
		}
	}

	private void Server_SendAnimalListings(BasePlayer player)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null || player.net == null || player.net.connection == null)
		{
			return;
		}
		GetAnimalListings(player, listingBuffer);
		NetWrite netWrite = ClientRPCStart("Client_ReceiveAnimalListings");
		netWrite.UInt8((byte)listingBuffer.Count);
		foreach (AnimalListing item in listingBuffer)
		{
			netWrite.EntityID(item.animalId);
			netWrite.String(item.animalName);
		}
		ClientRPCSend(netWrite, new SendInfo(player.net.connection));
		listingBuffer.Clear();
	}

	public override void OnConversationAction(BasePlayer player, string action)
	{
		base.OnConversationAction(player, action);
		switch (action)
		{
		case "selectwool":
			Server_SelectWool(player);
			break;
		case "acceptoffer":
			Server_AcceptOffer(player);
			break;
		case "rejectoffer":
			Server_RejectOffer(player);
			break;
		case "giveanimal male_calf":
			ProcessAnimalTransaction(MaleCalfPrefab, 300, player, isMale: true);
			break;
		case "giveanimal female_calf":
			ProcessAnimalTransaction(FemaleCalfPrefab, 300, player, isMale: false);
			break;
		case "giveanimal male_lamb":
			ProcessAnimalTransaction(MaleLambPrefab, 150, player, isMale: true);
			break;
		case "giveanimal female_lamb":
			ProcessAnimalTransaction(FemaleLambPrefab, 150, player, isMale: false);
			break;
		}
	}

	private void ProcessAnimalTransaction(GameObjectRef toCreate, int cost, BasePlayer targetPlayer, bool isMale)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (targetPlayer.inventory.GetAmount(ItemManager.Items.Scrap) >= cost)
		{
			LivestockAnimal livestockAnimal = gameManager.CreateEntity(toCreate.resourcePath, ((Component)targetPlayer).transform.position + -((Component)targetPlayer).transform.forward * 2f) as LivestockAnimal;
			if (!((Object)(object)livestockAnimal == (Object)null))
			{
				targetPlayer.inventory.UseAmount(ItemManager.Items.Scrap, cost);
				livestockAnimal.IsBeingPurchased = true;
				livestockAnimal.IsMale = isMale;
				livestockAnimal.Spawn();
				livestockAnimal.SetFamiliarity(targetPlayer.userID, Livestock.trustToBond, LivestockAnimal.FamiliarityReason.Purchased);
				livestockAnimal.TryLead(targetPlayer, wantsLead: true);
				livestockAnimal.SendNetworkUpdate();
			}
		}
	}
}

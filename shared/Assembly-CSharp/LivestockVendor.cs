using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using ProtoBuf;
using Rust.Ai.Gen2;
using UnityEngine;

public class LivestockVendor : NPCTalking
{
	public enum SaleKind : byte
	{
		Animal,
		Wool
	}

	public enum SaleRemark : byte
	{
		None,
		HiddenDefect,
		Inbred,
		Prize,
		LowPotential
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

		public float potential;

		public SaleRemark remark;

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

		public SaleRemark remark;

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

	private readonly Dictionary<ulong, Offer> presentedOffers = new Dictionary<ulong, Offer>();

	private readonly List<OfferItem> pickedItems = new List<OfferItem>();

	private readonly List<NetworkableId> expired = new List<NetworkableId>();

	public LivestockTrigger Trigger;

	[Tooltip("The items this vendor pays with, banded against how good the animal is.")]
	public LivestockSaleTable saleTable;

	[Tooltip("The items this vendor pays with for wool, banded against how full the lot is.")]
	public LivestockSaleTable woolSaleTable;

	public ItemDefinition woolItem;

	public SoundDefinition animalSoldSound;

	public SoundDefinition woolSoldSound;

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

	public bool Server_IsSelectionUnraised(BasePlayer player)
	{
		if (TryGetSelectedAnimal(player, out var animal) && animal.IsAdult())
		{
			return !WillPayFor(animal);
		}
		return false;
	}

	public bool Server_IsSelectionChild(BasePlayer player)
	{
		if (TryGetSelectedAnimal(player, out var animal))
		{
			return !animal.IsAdult();
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
		if ((Object)(object)player == (Object)null || player.inventory == null || (Object)(object)woolItem == (Object)null)
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
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (!TryFindAnimal(animalId, out var found) || !CanBeListedBy(found, player))
		{
			return false;
		}
		selectedSubjects[player.userID] = animalId;
		if (!IsSellable(found) || !WillPayFor(found))
		{
			return true;
		}
		PruneNegotiations();
		if (!negotiations.TryGetValue(animalId, out var value))
		{
			value = new Negotiation
			{
				kind = SaleKind.Animal,
				table = saleTable,
				remark = RemarkOn(found)
			};
			value.offers = RollOffers(value, animalId, NameOf(found), found.SaleQuality);
			negotiations.Add(animalId, value);
		}
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
				RememberOffer(player, subjectId, negotiation);
			}
		}
	}

	public bool Server_AcceptOffer(BasePlayer player)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
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
		ClientRPC(RpcTarget.Player("Client_OnSale", player), (byte)negotiation.kind);
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
		RememberOffer(player, subjectId, negotiation);
		return true;
	}

	public static SaleRemark RemarkOn(LivestockAnimal animal)
	{
		if ((Object)(object)animal == (Object)null)
		{
			return SaleRemark.None;
		}
		bool genesEnabled = Livestock.genesEnabled;
		if (genesEnabled && animal.IsInbred)
		{
			return SaleRemark.Inbred;
		}
		if (animal.Potential < Livestock.vendorLowPotential)
		{
			return SaleRemark.LowPotential;
		}
		if (genesEnabled && animal.HidesBadAllele)
		{
			return SaleRemark.HiddenDefect;
		}
		if (genesEnabled && animal.IsGodClone)
		{
			return SaleRemark.Prize;
		}
		return SaleRemark.None;
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

	private bool TryGetSelectedAnimal(BasePlayer player, out LivestockAnimal animal)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		animal = null;
		if ((Object)(object)player != (Object)null && selectedSubjects.TryGetValue(player.userID, out var value))
		{
			return TryFindAnimal(value, out animal);
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

	private void RememberOffer(BasePlayer player, NetworkableId subjectId, Negotiation negotiation)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null)
		{
			return;
		}
		Offer value = negotiation.offers[negotiation.rejections];
		value.index = negotiation.rejections;
		value.count = negotiation.offers.Length;
		value.remark = negotiation.remark;
		value.potential = 0f;
		if (negotiation.kind == SaleKind.Animal)
		{
			Offer value2;
			if (TryFindAnimal(subjectId, out var found))
			{
				value.potential = found.Potential;
				value.subjectName = found.GetNameOrLabelAsViewedBy(player);
			}
			else if (presentedOffers.TryGetValue(player.userID, out value2) && value2.subjectId == subjectId)
			{
				value.subjectName = value2.subjectName;
			}
		}
		presentedOffers[player.userID] = value;
	}

	private bool TryGetPresentedOffer(BasePlayer player, out Offer offer)
	{
		offer = default;
		if ((Object)(object)player != (Object)null && presentedOffers.TryGetValue(player.userID, out offer))
		{
			return offer.IsValid;
		}
		return false;
	}

	private void AddPresentedOffer(BasePlayer player, ConversationState state)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetPresentedOffer(player, out var offer))
		{
			AnimalOffer val = Pool.Get<AnimalOffer>();
			val.kind = (int)offer.kind;
			val.subjectId = offer.subjectId;
			val.subjectName = offer.subjectName;
			val.lotAmount = offer.lotAmount;
			val.lotItemId = ((offer.kind == SaleKind.Wool && (Object)(object)woolItem != (Object)null) ? woolItem.itemid : 0);
			val.index = offer.index;
			val.count = offer.count;
			val.generous = offer.generous;
			val.potential = offer.potential;
			val.remark = (int)offer.remark;
			val.items = Pool.Get<List<AnimalOfferItem>>();
			OfferItem[] items = offer.items;
			for (int i = 0; i < items.Length; i++)
			{
				OfferItem offerItem = items[i];
				AnimalOfferItem val2 = Pool.Get<AnimalOfferItem>();
				val2.itemId = (((Object)(object)offerItem.item != (Object)null) ? offerItem.item.itemid : 0);
				val2.amount = offerItem.amount;
				val.items.Add(val2);
			}
			state.animalOffer = val;
		}
	}

	public override void OnConversationEnded(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null)
		{
			presentedOffers.Remove(player.userID);
		}
		base.OnConversationEnded(player);
	}

	public static bool IsListable(LivestockAnimal animal)
	{
		if ((Object)(object)animal != (Object)null && !animal.IsDead())
		{
			return !animal.IsPregnant();
		}
		return false;
	}

	public static bool IsSellable(LivestockAnimal animal)
	{
		if (IsListable(animal))
		{
			return animal.IsAdult();
		}
		return false;
	}

	public static bool CanBeListedBy(LivestockAnimal animal, BasePlayer player)
	{
		if (IsListable(animal) && (Object)(object)player != (Object)null)
		{
			return animal.IsFollowing(player);
		}
		return false;
	}

	public static bool CanBeSoldBy(LivestockAnimal animal, BasePlayer player)
	{
		if (IsSellable(animal))
		{
			return CanBeListedBy(animal, player);
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
			if (entityContent is LivestockAnimal animal && CanBeListedBy(animal, player))
			{
				return true;
			}
		}
		return false;
	}

	public void GetAnimalListings(BasePlayer player, List<AnimalListing> buffer)
	{
		buffer.Clear();
		if (!((Object)(object)Trigger == (Object)null) && Trigger.entityContents != null)
		{
			AddListings(player, buffer, sellable: true);
			AddListings(player, buffer, sellable: false);
		}
	}

	private void AddListings(BasePlayer player, List<AnimalListing> buffer, bool sellable)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		foreach (BaseEntity entityContent in Trigger.entityContents)
		{
			if (buffer.Count >= 4)
			{
				break;
			}
			if (!((Object)(object)entityContent == (Object)null) && entityContent is LivestockAnimal livestockAnimal && CanBeListedBy(livestockAnimal, player) && IsSellable(livestockAnimal) == sellable)
			{
				buffer.Add(new AnimalListing
				{
					animalId = ((livestockAnimal.net != null) ? livestockAnimal.net.ID : default(NetworkableId)),
					animalName = livestockAnimal.GetNameOrLabelAsViewedBy(player)
				});
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
		return animal.NameOrLabel();
	}

	public override void AddFormatTextValues(BasePlayer player, ConversationData.AbstractSpeechNodeData speechNode, List<FormatTextValue> values)
	{
		base.AddFormatTextValues(player, speechNode, values);
		if (TryGetPresentedOffer(player, out var offer))
		{
			bool flag = offer.kind == SaleKind.Wool && (Object)(object)woolItem != (Object)null;
			string value = (flag ? woolItem.displayName.token : offer.subjectName);
			AddFormatTextValue(values, speechNode, "animal_name", value, flag);
			AddFormatTextValue(values, speechNode, "lot_name", value, flag);
			AddFormatTextValue(values, speechNode, "lot_amount", offer.lotAmount);
			AddFormatTextValue(values, speechNode, "offer_number", offer.index + 1);
			AddFormatTextValue(values, speechNode, "offers_left", offer.count - offer.index - 1);
		}
		AddFormatTextValue(values, speechNode, "minimum_wool_sale", Livestock.vendorWoolMinimum);
	}

	protected override bool NeedsFormatTextValue(ConversationData.AbstractSpeechNodeData speechNode, string key)
	{
		if (!(speechNode is ConversationData.AnimalSalePreviewSpeechNodeData))
		{
			return base.NeedsFormatTextValue(speechNode, key);
		}
		return true;
	}

	public static bool WillPayFor(LivestockAnimal animal)
	{
		if ((Object)(object)animal != (Object)null)
		{
			return animal.Potential > 0f;
		}
		return false;
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
		if (!IsSellable(animal) || !WillPayFor(animal))
		{
			return 0;
		}
		int result = ReferenceValueFor(animal);
		animal.Kill();
		return result;
	}

	protected override void AddSpeechNodeState(BasePlayer player, ConversationData.AbstractSpeechNodeData speechNode, ConversationState state)
	{
		base.AddSpeechNodeState(player, speechNode, state);
		if (speechNode is ConversationData.SellAnimalsSpeechNodeData)
		{
			AddAnimalListings(player, state);
		}
		if (speechNode is ConversationData.AnimalSalePreviewSpeechNodeData)
		{
			AddPresentedOffer(player, state);
		}
	}

	private void AddAnimalListings(BasePlayer player, ConversationState state)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		GetAnimalListings(player, listingBuffer);
		state.animalListings = Pool.Get<List<AnimalListing>>();
		foreach (AnimalListing item in listingBuffer)
		{
			AnimalListing val = Pool.Get<AnimalListing>();
			val.animalId = item.animalId;
			val.animalName = item.animalName;
			state.animalListings.Add(val);
		}
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
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (targetPlayer.inventory.GetAmount(ItemManager.Items.Scrap) >= cost && !LivestockAnimal.HasTooManyOnLeads(targetPlayer))
		{
			LivestockAnimal livestockAnimal = gameManager.CreateEntity(toCreate.resourcePath, ((Component)this).transform.position + ((Component)this).transform.forward * 2f) as LivestockAnimal;
			if (!((Object)(object)livestockAnimal == (Object)null))
			{
				targetPlayer.inventory.UseAmount(ItemManager.Items.Scrap, cost);
				livestockAnimal.IsBeingPurchased = true;
				livestockAnimal.IsMale = isMale;
				livestockAnimal.Spawn();
				livestockAnimal.SetOwner(targetPlayer.userID, LivestockAnimal.OwnerChangeReason.Purchased);
				livestockAnimal.SetFamiliarity(targetPlayer.userID, Livestock.trustToLead);
				livestockAnimal.TryLead(targetPlayer, wantsLead: true);
				livestockAnimal.SendNetworkUpdate();
			}
		}
	}
}

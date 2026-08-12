using System;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Safety;
using UnityEngine;
using UnityEngine.AI;

public class LootContainer : StorageContainer, ILootContainer
{
	public enum spawnType
	{
		GENERIC,
		PLAYER,
		TOWN,
		AIRDROP,
		CRASHSITE,
		ROADSIDE
	}

	[Serializable]
	public struct LootSpawnSlot
	{
		public LootSpawn definition;

		public int numberToSpawn;

		public float probability;

		public string onlyWithLoadoutNamed;

		public Era[] eras;
	}

	public bool destroyOnEmpty;

	public LootSpawn lootDefinition;

	public int maxDefinitionsToSpawn;

	public float minSecondsBetweenRefresh;

	public float maxSecondsBetweenRefresh;

	public bool initialLootSpawn;

	public float xpLootedScale;

	public float xpDestroyedScale;

	public bool BlockPlayerItemInput;

	public int scrapAmount;

	public string deathStat;

	public LootSpawnSlot[] LootSpawnSlots;

	public spawnType SpawnType;

	public ClanScoreEventType clanScoreEventForFirstLooter;

	[NonSerialized]
	public bool HasBeenLooted;

	[NonSerialized]
	public ulong FirstLooterId;

	private float timeAtLootCountdownStarted;

	private float currentLootCountdownLength;

	private bool isLootCountdownRunning;

	private bool isRestoringFromSave;

	private Action _actionSpawnLoot;

	private static ItemDefinition scrapDef;

	public bool shouldRefreshContents
	{
		get
		{
			if (minSecondsBetweenRefresh > 0f)
			{
				return maxSecondsBetweenRefresh > 0f;
			}
			return false;
		}
	}

	private Action actionSpawnLoot => _actionSpawnLoot ?? (_actionSpawnLoot = SpawnLoot);

	public override void ResetState()
	{
		FirstLooterId = 0uL;
		timeAtLootCountdownStarted = 0f;
		currentLootCountdownLength = 0f;
		isLootCountdownRunning = false;
		isRestoringFromSave = false;
		base.ResetState();
	}

	public static void FillLoot(ItemContainer inventory, LootSpawn lootDefinition, int maxDefinitionsToSpawn, LootSpawnSlot[] lootSpawnSlots)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (inventory == null)
		{
			return;
		}
		if (lootSpawnSlots != null && lootSpawnSlots.Length != 0)
		{
			for (int i = 0; i < lootSpawnSlots.Length; i++)
			{
				LootSpawnSlot lootSpawnSlot = lootSpawnSlots[i];
				if (lootSpawnSlot.eras != null && lootSpawnSlot.eras.Length != 0 && Array.IndexOf(lootSpawnSlot.eras, ConVar.Server.Era) == -1)
				{
					continue;
				}
				for (int j = 0; j < lootSpawnSlot.numberToSpawn; j++)
				{
					if (Random.Range(0f, 1f) <= lootSpawnSlot.probability)
					{
						lootSpawnSlot.definition.SpawnIntoContainer(inventory);
					}
				}
			}
		}
		else if ((Object)(object)lootDefinition != (Object)null)
		{
			for (int k = 0; k < maxDefinitionsToSpawn; k++)
			{
				lootDefinition.SpawnIntoContainer(inventory);
			}
		}
	}

	public override void PreServerLoad()
	{
		base.PreServerLoad();
		isRestoringFromSave = true;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (initialLootSpawn && !isRestoringFromSave)
		{
			SpawnLoot();
		}
		if (BlockPlayerItemInput && !Application.isLoadingSave && base.inventory != null)
		{
			base.inventory.SetFlag(ItemContainer.Flag.NoItemInput, b: true);
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved6, PlayerInventory.IsBirthday());
		}
		NavMeshObstacle val = default(NavMeshObstacle);
		if (Debugging.disableLootNavObstaclesInDeepSea && DeepSeaManager.IsInsideDeepSea((BaseNetworkable)this) && ((Component)this).TryGetComponent<NavMeshObstacle>(ref val))
		{
			val.carving = false;
		}
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		if (!added)
		{
			HasBeenLooted = true;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk && isLootCountdownRunning)
		{
			float num = info.cachedTime.Time - timeAtLootCountdownStarted;
			float countdownTimeRemaining = currentLootCountdownLength - num;
			info.msg.lootContainer = Pool.Get<LootContainer>();
			info.msg.lootContainer.countdownTimeRemaining = countdownTimeRemaining;
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (BlockPlayerItemInput && base.inventory != null)
		{
			base.inventory.SetFlag(ItemContainer.Flag.NoItemInput, b: true);
		}
	}

	internal override void DoServerDestroy()
	{
		CancelLootRefreshCountdown();
		base.DoServerDestroy();
	}

	public ItemContainer GetInventory()
	{
		return base.inventory;
	}

	public virtual void SpawnLoot()
	{
		if (base.IsDestroyed)
		{
			return;
		}
		if (base.inventory == null)
		{
			Debug.Log((object)"CONTACT DEVELOPERS! LootContainer::PopulateLoot has null inventory!!!");
			return;
		}
		base.inventory.Clear();
		ItemManager.DoRemoves();
		if (Interface.CallHook("OnLootSpawn", this) == null)
		{
			PopulateLoot();
			CancelLootRefreshCountdown();
			if (shouldRefreshContents)
			{
				StartLootRefreshCountdown();
			}
		}
	}

	private void StartLootRefreshCountdown(float? countdownLength = null)
	{
		CancelLootRefreshCountdown();
		timeAtLootCountdownStarted = Time.time;
		currentLootCountdownLength = ((!countdownLength.HasValue) ? Random.Range(minSecondsBetweenRefresh, maxSecondsBetweenRefresh) : countdownLength.Value);
		Invoke(actionSpawnLoot, currentLootCountdownLength);
		isLootCountdownRunning = true;
	}

	private void CancelLootRefreshCountdown()
	{
		if (IsInvoking(actionSpawnLoot))
		{
			CancelInvoke(actionSpawnLoot);
		}
		isLootCountdownRunning = false;
	}

	public float GetLootCountdownTimeRemaining()
	{
		if (!isLootCountdownRunning)
		{
			return -1f;
		}
		float num = Time.time - timeAtLootCountdownStarted;
		return Mathf.Max(0f, currentLootCountdownLength - num);
	}

	public int ScoreForRarity(Rarity rarity)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected I4, but got Unknown
		return (rarity - 1) switch
		{
			0 => 1, 
			1 => 2, 
			2 => 3, 
			3 => 4, 
			_ => 5000, 
		};
	}

	public virtual void PopulateLoot()
	{
		FillLoot(base.inventory, lootDefinition, maxDefinitionsToSpawn, LootSpawnSlots);
		if (SpawnType == spawnType.ROADSIDE || SpawnType == spawnType.TOWN)
		{
			foreach (Item item in base.inventory.itemList)
			{
				if (item.hasCondition)
				{
					item.condition = Random.Range(item.info.condition.foundCondition.fractionMin, item.info.condition.foundCondition.fractionMax) * item.info.condition.max;
				}
			}
		}
		GenerateScrap();
		HasBeenLooted = false;
		FirstLooterId = 0uL;
	}

	public void GenerateScrap()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (scrapAmount <= 0)
		{
			return;
		}
		if ((Object)(object)scrapDef == (Object)null)
		{
			scrapDef = ItemManager.FindItemDefinition("scrap");
		}
		int num = scrapAmount;
		if (num > 0)
		{
			Item item = ItemManager.Create(scrapDef, num, 0uL, isServerSide: true, 0uL);
			if (!item.MoveToContainer(base.inventory))
			{
				item.Drop(((Component)this).transform.position, GetInheritedDropVelocity());
			}
		}
	}

	public override void DropBonusItems(BaseEntity initiator, ItemContainer container)
	{
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		base.DropBonusItems(initiator, container);
		if ((Object)(object)initiator == (Object)null || container == null)
		{
			return;
		}
		BasePlayer basePlayer = initiator as BasePlayer;
		if ((Object)(object)basePlayer == (Object)null || scrapAmount <= 0 || !((Object)(object)scrapDef != (Object)null))
		{
			return;
		}
		float num = (((Object)(object)basePlayer.modifiers != (Object)null) ? (1f + basePlayer.modifiers.GetValue(Modifier.ModifierType.Scrap_Yield)) : 0f);
		if (!(num > 1f))
		{
			return;
		}
		float variableValue = basePlayer.modifiers.GetVariableValue(Modifier.ModifierType.Scrap_Yield, 0f);
		float num2 = Mathf.Max((float)scrapAmount * num - (float)scrapAmount, 0f);
		variableValue += num2;
		int num3 = 0;
		if (variableValue >= 1f)
		{
			num3 = (int)variableValue;
			variableValue -= (float)num3;
		}
		basePlayer.modifiers.SetVariableValue(Modifier.ModifierType.Scrap_Yield, variableValue);
		if (num3 > 0)
		{
			Item item = ItemManager.Create(scrapDef, num3, 0uL, isServerSide: true, 0uL);
			if (item != null && Interface.CallHook("OnBonusItemDrop", item, basePlayer, container) == null)
			{
				(item.Drop(GetDropPosition() + new Vector3(0f, 0.5f, 0f), GetInheritedDropVelocity()) as DroppedItem).DropReason = DroppedItem.DropReasonEnum.Loot;
				Interface.CallHook("OnBonusItemDropped", item, basePlayer, container);
			}
		}
	}

	public override bool OnStartBeingLooted(BasePlayer player)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (FirstLooterId == 0L && !player.isInvisible)
		{
			FirstLooterId = player.userID;
			Facepunch.Rust.Analytics.Azure.OnFirstLooted(this, player);
			if ((int)clanScoreEventForFirstLooter != -1)
			{
				player.AddClanScore(clanScoreEventForFirstLooter);
			}
			if (base.inventory != null && base.inventory.itemList != null)
			{
				foreach (Item item in base.inventory.itemList)
				{
					item?.SetItemOwnership(player, ItemOwnershipPhrases.LootedPhrase);
				}
			}
			if (Rust.GameInfo.HasAchievements)
			{
				string name = (player.IsInsideDeepSea() ? "STAT_LOOT_DEEPSEA" : "STAT_LOOT_MAINLAND");
				player?.stats.Add(name, 1);
			}
		}
		return base.OnStartBeingLooted(player);
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		base.PlayerStoppedLooting(player);
		if (destroyOnEmpty && (base.inventory == null || base.inventory.itemList == null || base.inventory.itemList.Count == 0))
		{
			Kill(DestroyMode.Gib);
		}
	}

	public void RemoveMe()
	{
		Kill(DestroyMode.Gib);
	}

	public override bool ShouldDropItemsIndividually()
	{
		return true;
	}

	public override void DropItems(BaseEntity initiator = null)
	{
		if (initiator is BasePlayer player)
		{
			foreach (Item item in base.inventory.itemList)
			{
				item?.SetItemOwnership(player, ItemOwnershipPhrases.LootedPhrase);
			}
		}
		base.DropItems(initiator);
	}

	public override void OnDied(HitInfo info)
	{
		if (info != null)
		{
			Facepunch.Rust.Analytics.Azure.OnLootContainerDestroyed(this, info.InitiatorPlayer, info.Weapon);
		}
		base.OnDied(info);
		if (info != null && (Object)(object)info.InitiatorPlayer != (Object)null && !string.IsNullOrEmpty(deathStat))
		{
			info.InitiatorPlayer.stats.Add(deathStat, 1, Stats.Life);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk && shouldRefreshContents)
		{
			if (info.msg.lootContainer != null)
			{
				StartLootRefreshCountdown(info.msg.lootContainer.countdownTimeRemaining);
			}
			else if (initialLootSpawn)
			{
				StartLootRefreshCountdown();
			}
		}
	}

	public override void OnAttacked(HitInfo info)
	{
		base.OnAttacked(info);
	}

	public override void InitShared()
	{
		base.InitShared();
	}

	public LootContainer()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		destroyOnEmpty = true;
		minSecondsBetweenRefresh = 3600f;
		maxSecondsBetweenRefresh = 7200f;
		initialLootSpawn = true;
		xpLootedScale = 1f;
		xpDestroyedScale = 1f;
		deathStat = "";
		clanScoreEventForFirstLooter = (ClanScoreEventType)(-1);
		base._002Ector();
	}
}

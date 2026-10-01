using System;
using Facepunch;
using Facepunch.Rust;
using ProtoBuf;
using Rust;
using UnityEngine;

public class RespawnableLootFridge : Fridge, ILootContainer
{
	public const Flags Flags_RequiresPowerToOpen = Flags.Reserved10;

	[Header("Respawnable Loot Fridge")]
	public LootContainer.LootSpawnSlot[] reducedLootSpawnSlots;

	public LootContainer.LootSpawnSlot[] fullLootSpawnSlots;

	[Tooltip("Minimum seconds between refreshes. Each refresh is a reduced spawn, apart from every Nth one (controlled by reducedRefreshesForFullRefresh) which is a full spawn instead.")]
	public float minSecondsBetweenRefresh = 900f;

	[Tooltip("Maximum seconds between refreshes.")]
	public float maxSecondsBetweenRefresh = 900f;

	[Min(1f)]
	[Tooltip("A full spawn replaces every Nth reduced refresh.")]
	public int refreshesForFullRefresh = 4;

	[Tooltip("Do a full loot spawn as soon as the fridge spawns (ignored when restoring from a save).")]
	public bool initialLootSpawn = true;

	public bool blockPlayerItemInput;

	public bool requiresPowerToOpen;

	[NonSerialized]
	public bool HasBeenLooted;

	[NonSerialized]
	public ulong FirstLooterId;

	private float timeAtLootCountdownStarted;

	private float currentLootCountdownLength;

	private bool isLootCountdownRunning;

	private bool isRestoringFromSave;

	private int refreshesUntilFullSpawn;

	private bool openedSinceFullSpawn;

	private Action _actionRefreshLoot;

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

	private Action actionRefreshLoot => OnLootRefreshElapsed;

	public int FullRefreshInterval => Mathf.Max(1, refreshesForFullRefresh);

	public bool NextRefreshIsFullSpawn => refreshesUntilFullSpawn <= 1;

	private bool HasReducedLoot
	{
		get
		{
			if (reducedLootSpawnSlots != null)
			{
				return reducedLootSpawnSlots.Length != 0;
			}
			return false;
		}
	}

	public override void ResetState()
	{
		FirstLooterId = 0uL;
		timeAtLootCountdownStarted = 0f;
		currentLootCountdownLength = 0f;
		isLootCountdownRunning = false;
		isRestoringFromSave = false;
		refreshesUntilFullSpawn = 0;
		openedSinceFullSpawn = false;
		base.ResetState();
	}

	void ILootContainer.SpawnLoot()
	{
		SpawnFullLoot();
	}

	void ILootContainer.PopulateLoot()
	{
		PopulateLoot(fullSpawn: true);
	}

	public override void PreServerLoad()
	{
		base.PreServerLoad();
		isRestoringFromSave = true;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		SetFlagLocal(Flags.Reserved10, requiresPowerToOpen);
		refreshesUntilFullSpawn = FullRefreshInterval;
		if (initialLootSpawn && !isRestoringFromSave)
		{
			SpawnFullLoot();
		}
		if (blockPlayerItemInput && !Application.isLoadingSave && inventory != null)
		{
			inventory.SetFlag(ItemContainer.Flag.NoItemInput, b: true);
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
			info.msg.lootContainer.refreshesUntilFullSpawn = refreshesUntilFullSpawn;
			info.msg.lootContainer.openedSinceFullSpawn = openedSinceFullSpawn;
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (blockPlayerItemInput && inventory != null)
		{
			inventory.SetFlag(ItemContainer.Flag.NoItemInput, b: true);
		}
	}

	internal override void DoServerDestroy()
	{
		CancelLootRefreshCountdown();
		base.DoServerDestroy();
	}

	public override void OnItemAddedOrRemoved(Item item, bool added, BasePlayer sourcePlayer)
	{
		base.OnItemAddedOrRemoved(item, added, sourcePlayer);
		if (!added)
		{
			HasBeenLooted = true;
		}
	}

	public ItemContainer GetInventory()
	{
		return inventory;
	}

	public void SpawnFullLoot()
	{
		RefreshLoot(fullSpawn: true);
	}

	public void SpawnReducedLoot()
	{
		RefreshLoot(fullSpawn: false);
	}

	private void OnLootRefreshElapsed()
	{
		isLootCountdownRunning = false;
		if (NextRefreshIsFullSpawn)
		{
			SpawnFullLoot();
		}
		else
		{
			SpawnReducedLoot();
		}
	}

	private void RefreshLoot(bool fullSpawn)
	{
		if (IsDestroyed || inventory == null)
		{
			return;
		}
		if (!fullSpawn)
		{
			if (!openedSinceFullSpawn)
			{
				AdvanceRefreshCycle(fullSpawn: false);
				return;
			}
			if (!HasReducedLoot)
			{
				AdvanceRefreshCycle(fullSpawn: false);
				return;
			}
		}
		inventory.Clear();
		ItemManager.DoRemoves();
		PopulateLoot(fullSpawn);
		FirstLooterId = 0uL;
		if (fullSpawn)
		{
			openedSinceFullSpawn = IsOpen();
		}
		AdvanceRefreshCycle(fullSpawn);
	}

	private void AdvanceRefreshCycle(bool fullSpawn)
	{
		refreshesUntilFullSpawn = (fullSpawn ? FullRefreshInterval : Mathf.Max(1, refreshesUntilFullSpawn - 1));
		CancelLootRefreshCountdown();
		if (shouldRefreshContents)
		{
			StartLootRefreshCountdown();
		}
	}

	private void StartLootRefreshCountdown(float? countdownLength = null)
	{
		CancelLootRefreshCountdown();
		timeAtLootCountdownStarted = Time.time;
		currentLootCountdownLength = ((!countdownLength.HasValue) ? Random.Range(minSecondsBetweenRefresh, maxSecondsBetweenRefresh) : countdownLength.Value);
		Invoke(actionRefreshLoot, currentLootCountdownLength);
		isLootCountdownRunning = true;
	}

	private void CancelLootRefreshCountdown()
	{
		if (IsInvoking(actionRefreshLoot))
		{
			CancelInvoke(actionRefreshLoot);
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

	public void PopulateLoot(bool fullSpawn)
	{
		if (inventory != null)
		{
			Func<BasePlayer, Item, int, bool> canAcceptItem = inventory.canAcceptItem;
			inventory.canAcceptItem = null;
			if (fullSpawn)
			{
				LootContainer.FillLoot(inventory, null, 0, fullLootSpawnSlots);
			}
			else
			{
				LootContainer.FillLoot(inventory, null, 0, reducedLootSpawnSlots);
			}
			inventory.canAcceptItem = canAcceptItem;
			HasBeenLooted = false;
		}
	}

	public override bool OnStartBeingLooted(BasePlayer player)
	{
		if (!player.isInvisible)
		{
			openedSinceFullSpawn = true;
		}
		if (FirstLooterId == 0L && !player.isInvisible)
		{
			FirstLooterId = player.userID;
			Analytics.Azure.OnFirstLooted(this, player);
			if (inventory != null && inventory.itemList != null)
			{
				foreach (Item item in inventory.itemList)
				{
					item?.SetItemOwnership(player, ItemOwnershipPhrases.LootedPhrase);
				}
			}
		}
		return base.OnStartBeingLooted(player);
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		if (!IsPowered() && HasFlag(Flags.Reserved10))
		{
			CloseAllLooters();
		}
	}

	private void CloseAllLooters()
	{
		int i = 0;
		for (int count = BasePlayer.activePlayerList.Count; i < count; i++)
		{
			BasePlayer basePlayer = BasePlayer.activePlayerList[i];
			if ((Object)(object)basePlayer != (Object)null && (Object)(object)basePlayer.inventory != (Object)null && (Object)(object)basePlayer.inventory.loot != (Object)null && (Object)(object)basePlayer.inventory.loot.entitySource == (Object)(object)this)
			{
				basePlayer.EndLooting();
			}
		}
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (HasFlag(Flags.Reserved10) && !IsPowered())
		{
			return false;
		}
		return base.CanBeLooted(player);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk && shouldRefreshContents)
		{
			if (info.msg.lootContainer != null)
			{
				refreshesUntilFullSpawn = info.msg.lootContainer.refreshesUntilFullSpawn;
				openedSinceFullSpawn = info.msg.lootContainer.openedSinceFullSpawn;
				StartLootRefreshCountdown(info.msg.lootContainer.countdownTimeRemaining);
			}
			else if (initialLootSpawn)
			{
				refreshesUntilFullSpawn = FullRefreshInterval;
				StartLootRefreshCountdown();
			}
		}
	}
}

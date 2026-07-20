using System;
using ConVar;
using Oxide.Core;
using UnityEngine;

public class LootFill : BaseMonoBehaviour, IServerComponent
{
	[Header("Loot")]
	public StorageContainer StorageContainer;

	public LootSpawn LootDefinition;

	public int MaxDefinitionsToSpawn;

	public LootContainer.LootSpawnSlot[] LootSpawnSlots;

	public void FillLoot()
	{
		Invoke(DelayFill, 5f);
	}

	private void DelayFill()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)StorageContainer == (Object)null || Interface.CallHook("OnLootSpawn", this) != null)
		{
			return;
		}
		if (LootSpawnSlots.Length != 0)
		{
			LootContainer.LootSpawnSlot[] lootSpawnSlots = LootSpawnSlots;
			for (int i = 0; i < lootSpawnSlots.Length; i++)
			{
				LootContainer.LootSpawnSlot lootSpawnSlot = lootSpawnSlots[i];
				if (lootSpawnSlot.eras != null && lootSpawnSlot.eras.Length != 0 && Array.IndexOf(lootSpawnSlot.eras, Server.Era) == -1)
				{
					continue;
				}
				for (int j = 0; j < lootSpawnSlot.numberToSpawn; j++)
				{
					if (Random.Range(0f, 1f) <= lootSpawnSlot.probability)
					{
						lootSpawnSlot.definition.SpawnIntoContainer(StorageContainer.inventory);
					}
				}
			}
		}
		else if ((Object)(object)LootDefinition != (Object)null)
		{
			for (int k = 0; k < MaxDefinitionsToSpawn; k++)
			{
				LootDefinition.SpawnIntoContainer(StorageContainer.inventory);
			}
		}
	}
}

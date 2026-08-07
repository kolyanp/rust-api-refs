using System;
using Rust;
using UnityEngine;

public class TrainCarUnloadableLoot : TrainCarUnloadable
{
	[Serializable]
	public class LootCrateSet
	{
		public GameObjectRef[] crates;
	}

	[SerializeField]
	private LootCrateSet[] lootLayouts;

	[SerializeField]
	private Transform[] lootPositions;

	public override void Spawn()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		base.Spawn();
		bool flag = false;
		if (Application.isLoadingSave || flag)
		{
			return;
		}
		int num = Random.Range(0, lootLayouts.Length);
		for (int i = 0; i < lootLayouts[num].crates.Length; i++)
		{
			GameObjectRef gameObjectRef = lootLayouts[num].crates[i];
			LootContainer lootContainer = GameManager.server.CreateEntity(gameObjectRef.resourcePath, lootPositions[i].localPosition, lootPositions[i].localRotation) as LootContainer;
			if ((Object)(object)lootContainer != (Object)null)
			{
				lootContainer.Spawn();
				lootContainer.SetParent(this);
				lootContainer.inventory.SetLocked(!IsEmpty());
				lootContainers.Add(new EntityRef<LootContainer>(lootContainer.net.ID));
			}
		}
	}
}

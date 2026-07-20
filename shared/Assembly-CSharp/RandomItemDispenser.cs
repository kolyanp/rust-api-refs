using System;
using Oxide.Core;
using UnityEngine;

public class RandomItemDispenser : PrefabAttribute, IServerComponent
{
	[Serializable]
	public struct RandomItemChance
	{
		public ItemDefinition Item;

		public int Amount;

		[Range(0f, 1f)]
		public float Chance;

		public bool IgnoreInTutorial;
	}

	public RandomItemChance[] Chances;

	public bool OnlyAwardOne = true;

	protected override Type GetIndexedType()
	{
		return typeof(RandomItemDispenser);
	}

	public void DistributeItems(BasePlayer forPlayer, Vector3 distributorPosition)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		RandomItemChance[] chances = Chances;
		for (int i = 0; i < chances.Length; i++)
		{
			RandomItemChance itemChance = chances[i];
			if (!((Object)(object)forPlayer != (Object)null) || !forPlayer.IsInTutorial || !itemChance.IgnoreInTutorial)
			{
				bool flag = TryAward(itemChance, forPlayer, distributorPosition);
				if (OnlyAwardOne && flag)
				{
					break;
				}
			}
		}
	}

	private bool TryAward(RandomItemChance itemChance, BasePlayer forPlayer, Vector3 distributorPosition)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnRandomItemAward", this, itemChance, forPlayer, distributorPosition) != null)
		{
			return false;
		}
		float num = Random.Range(0f, 1f);
		if (itemChance.Chance >= num)
		{
			Item item = ItemManager.Create(itemChance.Item, itemChance.Amount, 0uL, isServerSide: true, 0uL);
			if (item != null)
			{
				item.SetItemOwnership(forPlayer, ItemOwnershipPhrases.GatheredPhrase);
				if (Object.op_Implicit((Object)(object)forPlayer))
				{
					forPlayer.GiveItem(item, BaseEntity.GiveItemReason.ResourceHarvested);
				}
				else
				{
					item.Drop(distributorPosition + Vector3.up * 0.5f, Vector3.up);
				}
			}
			return true;
		}
		return false;
	}
}

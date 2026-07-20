using UnityEngine;

namespace Rust.Ai.Gen2;

public class Trans_TargetIsUndergeared : FSMTransitionBase
{
	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		using (TimeWarning.New("Trans_TargetIsUndergeared"))
		{
			if (!base.Senses.FindTarget(out var target))
			{
				return false;
			}
			if (!target.ToNonNpcPlayer(out var player))
			{
				return false;
			}
			foreach (Item item in player.inventory.containerBelt.itemList)
			{
				if (IsItemHighLevelWeapon(item))
				{
					return false;
				}
			}
			foreach (Item item2 in player.inventory.containerMain.itemList)
			{
				if (IsItemHighLevelWeapon(item2))
				{
					return false;
				}
			}
			return true;
		}
	}

	private bool IsItemHighLevelWeapon(Item item)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Invalid comparison between Unknown and I4
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Invalid comparison between Unknown and I4
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Invalid comparison between Unknown and I4
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Invalid comparison between Unknown and I4
		ItemMod[] itemMods = item.info.itemMods;
		for (int i = 0; i < itemMods.Length; i++)
		{
			if (BaseNetworkableEx.Is<ItemModEntity>((Object)(object)itemMods[i], out ItemModEntity castedUnityObject) && BaseNetworkableEx.Is<BaseProjectile>((Object)(object)castedUnityObject.entityPrefab.GetEntity(), out BaseProjectile castedUnityObject2) && ((castedUnityObject2.primaryMagazine.definition.ammoTypes & 1) == 1 || (castedUnityObject2.primaryMagazine.definition.ammoTypes & 4) == 4 || (castedUnityObject2.primaryMagazine.definition.ammoTypes & 2) == 2 || (castedUnityObject2.primaryMagazine.definition.ammoTypes & 0x20) == 32 || (castedUnityObject2.primaryMagazine.definition.ammoTypes & 0x1000) == 4096))
			{
				return true;
			}
		}
		return false;
	}
}

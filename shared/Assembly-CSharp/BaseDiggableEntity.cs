using System.Collections.Generic;
using System.Linq;
using Facepunch;
using Network;
using Oxide.Core;
using UnityEngine;

public class BaseDiggableEntity : BaseCombatEntity
{
	public bool RequiresShovel;

	public Vector3 DropOffset;

	public Vector3 DropVelocity;

	public int RequiredDigCount = 3;

	public bool DestroyOnDug = true;

	[Header("Loot")]
	public List<DiggableEntityLoot> LootLists = new List<DiggableEntityLoot>();

	public int digsRemaining;

	public override bool ValidateMeleeColliderAntihack => false;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseDiggableEntity.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		digsRemaining = RequiredDigCount;
		_maxHealth = RequiredDigCount;
	}

	public override void Hurt(HitInfo info)
	{
		if (!((Object)(object)info.InitiatorPlayer == (Object)null) && (!RequiresShovel || (!((Object)(object)info.Weapon == (Object)null) && info.Weapon is Shovel)) && info.damageTypes.IsMeleeType())
		{
			Dig(info.InitiatorPlayer);
		}
	}

	public virtual void Dig(BasePlayer player)
	{
		if (Interface.CallHook("OnPlayerDig", player, this) != null)
		{
			return;
		}
		if (digsRemaining == RequiredDigCount)
		{
			OnFirstDig(player);
		}
		ClientRPC(RpcTarget.NetworkGroup("RPC_OnDig"), RequiredDigCount - digsRemaining, RequiredDigCount);
		digsRemaining--;
		base.health = digsRemaining;
		SendNetworkUpdate();
		OnSingleDig(player);
		if (digsRemaining <= 0)
		{
			OnFullyDug(player);
			if (DestroyOnDug)
			{
				Kill();
			}
		}
	}

	public virtual void OnFirstDig(BasePlayer player)
	{
	}

	public virtual void OnSingleDig(BasePlayer player)
	{
	}

	public virtual void OnFullyDug(BasePlayer player)
	{
		SpawnLootListItem(player);
	}

	public BaseEntity SpawnLootListItem(BasePlayer player)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		DiggableEntityLoot.ItemEntry? item = GetItem(((Component)this).transform.position);
		if (!item.HasValue || !item.HasValue)
		{
			return null;
		}
		Item item2 = ItemManager.Create(item.Value.Item, Random.Range(item.Value.Min, item.Value.Max + 1), item.Value.Skin, isServerSide: true, 0uL);
		if (item.Value.Owner.HasValue)
		{
			ItemOwnershipShare value = item.Value.Owner.Value;
			item2.AddItemOwnership(value.username, value.reason, value.amount);
		}
		item2.AddItemOwnership(player, ItemOwnershipPhrases.MetalDetector);
		if (item.Value.Condition.HasValue)
		{
			item2.condition = item.Value.Condition.Value;
		}
		DroppedItem droppedItem = null;
		if (item2 != null)
		{
			if (item2.hasCondition && !item.Value.Condition.HasValue)
			{
				item2.condition = Random.Range(item2.info.condition.foundCondition.fractionMin, item2.info.condition.foundCondition.fractionMax) * item2.info.condition.max;
			}
			droppedItem = item2.Drop(((Component)this).transform.position + DropOffset, DropVelocity) as DroppedItem;
			if ((Object)(object)droppedItem != (Object)null)
			{
				droppedItem.NeverCombine = true;
				droppedItem.SetAngularVelocity(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 720f);
			}
			if (item.Value.UID.HasValue)
			{
				BuriedItems.Instance.UnregisterItem(item.Value.UID.Value);
			}
		}
		return droppedItem;
	}

	private DiggableEntityLoot.ItemEntry? GetItem(Vector3 digWorldPos)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (LootLists == null)
		{
			return null;
		}
		List<DiggableEntityLoot.ItemEntry> list = Pool.Get<List<DiggableEntityLoot.ItemEntry>>();
		list.Clear();
		foreach (DiggableEntityLoot lootList in LootLists)
		{
			if (lootList.VerifyLootListForWorldPosition(digWorldPos))
			{
				list.AddRange(lootList.GetItems());
			}
		}
		if (Object.op_Implicit((Object)(object)BuriedItems.Instance))
		{
			BuriedItems.Instance.AddItems(list, digWorldPos);
		}
		DiggableEntityLoot.ItemEntry? result = null;
		if (list.Count != 0)
		{
			int num = list.Sum((DiggableEntityLoot.ItemEntry x) => x.Weight);
			int num2 = Random.Range(0, num);
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				result = list[num3];
				if (result.HasValue)
				{
					num -= result.Value.Weight;
					if (num2 >= num)
					{
						break;
					}
				}
			}
		}
		Pool.FreeUnmanaged<DiggableEntityLoot.ItemEntry>(ref list);
		return result;
	}
}

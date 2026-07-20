using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Recycler : StorageContainer
{
	private static readonly int Param_On = Animator.StringToHash("on");

	public Animator Animator;

	[Tooltip("Depreciated")]
	public float recycleEfficiency = 0.6f;

	public float safezoneRecycleEfficiency = 0.4f;

	public float radtownRecycleEfficiency = 0.6f;

	public SoundDefinition grindingLoopDef;

	public SoundDefinition grindingLoopDef_Slow;

	public GameObjectRef startSound;

	public GameObjectRef stopSound;

	public const Flags SafeZone = Flags.Reserved9;

	public float scrapRemainder;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Recycler.OnRpcMessage"))
		{
			if (rpc == 4167839872u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SVSwitch"));
				}
				using (TimeWarning.New("SVSwitch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4167839872u, "SVSwitch", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SVSwitch(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SVSwitch");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	private bool CanBeRecycled(Item item)
	{
		object obj = Interface.CallHook("CanBeRecycled", item, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (item != null)
		{
			return (Object)(object)item.info.Blueprint != (Object)null;
		}
		return false;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		ItemContainer itemContainer = base.inventory;
		itemContainer.canAcceptItem = (Func<Item, int, bool>)Delegate.Combine(itemContainer.canAcceptItem, new Func<Item, int, bool>(RecyclerItemFilter));
		ItemContainer itemContainer2 = base.inventory;
		itemContainer2.onItemAddedRemoved = (Action<Item, bool>)Delegate.Combine(itemContainer2.onItemAddedRemoved, new Action<Item, bool>(OnItemAddedRemoved));
		UpdateInSafeZone();
	}

	private void OnItemAddedRemoved(Item item, bool added)
	{
		if (added && IsOn() && (Object)(object)base.LastLootedByPlayer != (Object)null)
		{
			item.CollectedForCrafting(base.LastLootedByPlayer);
		}
	}

	public bool RecyclerItemFilter(Item item, int targetSlot)
	{
		int num = Mathf.CeilToInt((float)base.inventory.capacity * 0.5f);
		if (targetSlot == -1)
		{
			bool flag = false;
			for (int i = 0; i < num; i++)
			{
				if (!base.inventory.SlotTaken(item, i))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		if (targetSlot < num)
		{
			return CanBeRecycled(item);
		}
		return true;
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void SVSwitch(RPCMessage msg)
	{
		bool flag = msg.read.Bit();
		if (flag == IsOn() || (Object)(object)msg.player == (Object)null || Interface.CallHook("OnRecyclerToggle", this, msg.player) != null || (!flag && onlyOneUser && (Object)(object)msg.player.inventory.loot.entitySource != (Object)(object)this) || (flag && !HasRecyclable()))
		{
			return;
		}
		if (flag)
		{
			foreach (Item item in base.inventory.itemList)
			{
				item.CollectedForCrafting(msg.player);
			}
			StartRecycling();
		}
		else
		{
			StopRecycling();
		}
	}

	public bool MoveItemToOutput(Item newItem)
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		int num = -1;
		for (int i = 6; i < 12; i++)
		{
			Item slot = base.inventory.GetSlot(i);
			if (slot == null)
			{
				num = i;
				break;
			}
			if (slot.CanStack(newItem))
			{
				if (slot.amount + newItem.amount <= slot.MaxStackable())
				{
					num = i;
					break;
				}
				int num2 = Mathf.Min(slot.MaxStackable() - slot.amount, newItem.amount);
				newItem.UseItem(num2);
				slot.amount += num2;
				slot.MarkDirty();
				newItem.MarkDirty();
			}
			if (newItem.amount <= 0)
			{
				return true;
			}
		}
		if (num != -1 && newItem.MoveToContainer(base.inventory, num))
		{
			return true;
		}
		newItem.Drop(((Component)this).transform.position + new Vector3(0f, 2f, 0f), GetInheritedDropVelocity() + ((Component)this).transform.forward * 2f);
		return false;
	}

	public bool HasRecyclable()
	{
		for (int i = 0; i < 6; i++)
		{
			Item slot = base.inventory.GetSlot(i);
			if (slot != null)
			{
				object obj = Interface.CallHook("CanRecycle", this, slot);
				if (obj is bool)
				{
					return (bool)obj;
				}
				if ((Object)(object)slot.info.Blueprint != (Object)null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void RecycleThink()
	{
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		float num = (IsSafezoneRecycler() ? safezoneRecycleEfficiency : radtownRecycleEfficiency);
		int num2 = 0;
		while (true)
		{
			if (num2 < 6)
			{
				Item slot = base.inventory.GetSlot(num2);
				if (!CanBeRecycled(slot))
				{
					num2++;
					continue;
				}
				if (Interface.CallHook("OnItemRecycle", slot, this) != null)
				{
					if (!HasRecyclable())
					{
						StopRecycling();
					}
					break;
				}
				if (slot.hasCondition)
				{
					num = Mathf.Clamp01(num * Mathf.Clamp(slot.conditionNormalized * slot.maxConditionNormalized, 0.1f, 1f));
				}
				int num3 = 1;
				if (slot.amount > 1)
				{
					num3 = Mathf.CeilToInt(Mathf.Min((float)slot.amount, (float)slot.MaxStackable() * 0.1f));
				}
				object obj = Interface.CallHook("OnItemRecycleAmount", slot, num3, this);
				if (obj is int)
				{
					num3 = (int)obj;
				}
				if (slot.info.Blueprint.scrapFromRecycle > 0)
				{
					float num4 = slot.info.Blueprint.scrapFromRecycle * num3;
					if (slot.MaxStackable() == 1 && slot.hasCondition)
					{
						num4 *= slot.conditionNormalized;
					}
					float num5 = num / 0.5f;
					num4 *= num5;
					int num6 = Mathf.FloorToInt(num4);
					float num7 = num4 - (float)num6;
					scrapRemainder += num7;
					if (scrapRemainder >= 1f)
					{
						int num8 = Mathf.FloorToInt(scrapRemainder);
						scrapRemainder -= num8;
						num6 += num8;
					}
					if (num6 >= 1)
					{
						Item item = ItemManager.CreateByName("scrap", num6, 0uL);
						if ((Object)(object)base.LastLootedByPlayer != (Object)null)
						{
							item.SetItemOwnership(base.LastLootedByPlayer, ItemOwnershipPhrases.Recycler);
						}
						Facepunch.Rust.Analytics.Azure.OnRecyclerItemProduced(item.info.shortname, item.amount, this, slot);
						MoveItemToOutput(item);
					}
				}
				if (!string.IsNullOrEmpty(slot.info.Blueprint.RecycleStat))
				{
					List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
					Vis.Entities(((Component)this).transform.position, 3f, list, 131072, (QueryTriggerInteraction)2);
					foreach (BasePlayer item3 in list)
					{
						if (item3.IsAlive() && !item3.IsSleeping() && (Object)(object)item3.inventory.loot.entitySource == (Object)(object)this)
						{
							item3.stats.Add(slot.info.Blueprint.RecycleStat, num3, (Stats)5);
							item3.stats.Save();
						}
					}
					Pool.FreeUnmanaged<BasePlayer>(ref list);
				}
				Facepunch.Rust.Analytics.Azure.OnItemRecycled(slot.info.shortname, num3, this);
				slot.UseItem(num3);
				foreach (ItemAmount ingredient in slot.info.Blueprint.GetIngredients())
				{
					if (ingredient.itemDef.shortname == "scrap")
					{
						continue;
					}
					float num9 = ingredient.amount / (float)slot.info.Blueprint.amountToCreate * num * (float)num3;
					int num10 = Mathf.FloorToInt(num9);
					float num11 = num9 - (float)num10;
					if (num11 > float.Epsilon && Random.Range(0f, 1f) <= num11)
					{
						num10++;
					}
					if (num10 <= 0)
					{
						continue;
					}
					int num12 = Mathf.CeilToInt((float)num10 / (float)ingredient.itemDef.stackable);
					for (int i = 0; i < num12; i++)
					{
						if (ingredient.itemDef.IsAllowed((EraRestriction)16))
						{
							int num13 = ((num10 > ingredient.itemDef.stackable) ? ingredient.itemDef.stackable : num10);
							Item item2 = ItemManager.Create(ingredient.itemDef, num13, 0uL, isServerSide: true, 0uL);
							if ((Object)(object)base.LastLootedByPlayer != (Object)null)
							{
								item2.SetItemOwnership(base.LastLootedByPlayer, ItemOwnershipPhrases.Recycler);
							}
							Facepunch.Rust.Analytics.Azure.OnRecyclerItemProduced(item2.info.shortname, item2.amount, this, slot);
							if (!MoveItemToOutput(item2))
							{
								flag = true;
							}
							num10 -= num13;
							if (num10 <= 0)
							{
								break;
							}
						}
					}
				}
			}
			if (flag || !HasRecyclable())
			{
				StopRecycling();
			}
			break;
		}
	}

	public float GetRecycleThinkDuration()
	{
		if (IsSafezoneRecycler())
		{
			return 8f;
		}
		return 5f;
	}

	public void StartRecycling()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (!IsOn())
		{
			InvokeRepeating(RecycleThink, GetRecycleThinkDuration(), GetRecycleThinkDuration());
			Effect.server.Run(startSound.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			SetFlagLocal(Flags.On, b: true);
			SendNetworkUpdateImmediate();
		}
	}

	public void StopRecycling()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		CancelInvoke(RecycleThink);
		if (IsOn())
		{
			Effect.server.Run(stopSound.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			SetFlagLocal(Flags.On, b: false);
			SendNetworkUpdateImmediate();
		}
	}

	public void UpdateInSafeZone()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(base.isServer);
		if ((Object)(object)activeGameMode != (Object)null && !activeGameMode.safeZone)
		{
			flagsUpdateScope.Set(Flags.Reserved9, b: false);
			return;
		}
		bool b = false;
		List<TriggerBase> list = Pool.Get<List<TriggerBase>>();
		GamePhysics.OverlapSphere<TriggerBase>(((Component)this).transform.position, 1f, list, 262144, (QueryTriggerInteraction)2);
		foreach (TriggerBase item in list)
		{
			if (!((Object)(object)item == (Object)null))
			{
				if (item is TriggerSafeZone)
				{
					b = true;
				}
				else if (item is TriggerSafeZoneOverride { IsCombatActive: not false })
				{
					b = false;
					break;
				}
			}
		}
		if (base.isServer)
		{
			flagsUpdateScope.Set(Flags.Reserved9, b);
		}
		Pool.FreeUnmanaged<TriggerBase>(ref list);
	}

	public bool IsSafezoneRecycler()
	{
		return HasFlag(Flags.Reserved9);
	}

	[UnityEvent]
	public void PlayAnim()
	{
	}

	[UnityEvent]
	public void StopAnim()
	{
	}

	private void ToggleAnim(bool toggle)
	{
	}
}

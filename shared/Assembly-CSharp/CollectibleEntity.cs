using System;
using ConVar;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class CollectibleEntity : BaseEntity, IPrefabPreProcess
{
	public static readonly Phrase EatTitle;

	public Phrase itemName;

	public ItemAmount[] itemList;

	public GameObjectRef pickupEffect;

	public float xpScale = 1f;

	public bool suppressGatherRateMultiplier;

	bool IPrefabPreProcess.CanRunDuringBundling => false;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CollectibleEntity.OnRpcMessage"))
		{
			if (rpc == 2778075470u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Pickup"));
				}
				using (TimeWarning.New("Pickup"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2778075470u, "Pickup", this, player, 3f))
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
							Pickup(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in Pickup");
					}
				}
				return true;
			}
			if (rpc == 3528769075u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - PickupEat"));
				}
				using (TimeWarning.New("PickupEat"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3528769075u, "PickupEat", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							PickupEat(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in PickupEat");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsFood(bool checkConsumeMod = false)
	{
		for (int i = 0; i < itemList.Length; i++)
		{
			if (itemList[i].itemDef.category == ItemCategory.Food && (!checkConsumeMod || (Object)(object)((Component)itemList[i].itemDef).GetComponent<ItemModConsume>() != (Object)null))
			{
				return true;
			}
		}
		return false;
	}

	public void DoPickup(BasePlayer reciever, bool eat = false)
	{
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		if (itemList == null || Interface.CallHook("OnCollectiblePickup", this, reciever, eat) != null)
		{
			return;
		}
		float num = (((Object)(object)reciever.modifiers != (Object)null) ? reciever.modifiers.GetValue(Modifier.ModifierType.Collectible_DoubleYield) : 0f);
		bool flag = num != 0f && Random.value < num;
		ItemAmount[] array = itemList;
		foreach (ItemAmount itemAmount in array)
		{
			if ((Object)(object)reciever != (Object)null && reciever.IsInTutorial && itemAmount.ignoreInTutorial)
			{
				continue;
			}
			int num2 = (flag ? ((int)itemAmount.amount * 2) : ((int)itemAmount.amount));
			float num3 = 1f;
			if (!suppressGatherRateMultiplier && BaseGameMode.GetActiveGameMode(serverside: true) is GameModeSoftcore)
			{
				num3 = Mathf.Max(0f, GameModeSoftcore.gather_rate);
			}
			int iAmount = ((num2 > 0) ? Mathf.Max(1, Mathf.RoundToInt((float)num2 * num3)) : num2);
			Item item = ItemManager.Create(itemAmount.itemDef, iAmount, 0uL, isServerSide: true, 0uL);
			if (item == null)
			{
				continue;
			}
			item.SetItemOwnership(reciever, ItemOwnershipPhrases.GatheredPhrase);
			if (eat && item.info.category == ItemCategory.Food && (Object)(object)reciever != (Object)null)
			{
				ItemModConsume component = ((Component)item.info).GetComponent<ItemModConsume>();
				if ((Object)(object)component != (Object)null)
				{
					component.DoAction(item, reciever);
					continue;
				}
			}
			if (Object.op_Implicit((Object)(object)reciever))
			{
				Facepunch.Rust.Analytics.Azure.OnGatherItem(item.info.shortname, item.amount, this, reciever);
				Interface.CallHook("OnCollectiblePickedup", this, reciever, item);
				reciever.GiveItem(item, GiveItemReason.ResourceHarvested, GiveItemOptions.BackpackOverflow);
			}
			else
			{
				item.Drop(((Component)this).transform.position + Vector3.up * 0.5f, Vector3.up);
			}
		}
		itemList = null;
		if (pickupEffect.isValid)
		{
			Effect.server.Run(pickupEffect.resourcePath, ((Component)this).transform.position, ((Component)this).transform.up);
		}
		RandomItemDispenser randomItemDispenser = PrefabAttribute.server.Find<RandomItemDispenser>(prefabID);
		if (randomItemDispenser != null)
		{
			randomItemDispenser.DistributeItems(reciever, ((Component)this).transform.position);
		}
		Kill();
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void Pickup(RPCMessage msg)
	{
		if (msg.player.CanInteract())
		{
			DoPickup(msg.player);
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void PickupEat(RPCMessage msg)
	{
		if (msg.player.CanInteract())
		{
			DoPickup(msg.player, eat: true);
		}
	}

	public bool HasItem(ItemDefinition def)
	{
		ItemAmount[] array = itemList;
		for (int i = 0; i < array.Length; i++)
		{
			if ((Object)(object)array[i].itemDef == (Object)(object)def)
			{
				return true;
			}
		}
		return false;
	}

	public override void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.PreProcess(preProcess, rootObj, name, serverside, clientside, bundling);
		if (serverside)
		{
			preProcess.RemoveComponent((Component)(object)((Component)this).GetComponent<Collider>());
		}
	}

	static CollectibleEntity()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		EatTitle = new Phrase("eat", "Eat");
	}
}

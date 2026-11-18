using System;
using System.Collections.Generic;
using Carbon.Base;
using Carbon.Pooling;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using UnityEngine;

namespace Carbon.Modules;

public class GatherManagerModule : CarbonModule<GatherManagerConfig, EmptyModuleData>
{
	public enum KindTypes
	{
		Pickup,
		Gather,
		Quarry,
		Excavator
	}

	private HashSet<NetworkableId> _processedEntities = new HashSet<NetworkableId>();

	public static GatherManagerModule Singleton { get; internal set; }

	public override string Name => "GatherManager";

	public override VersionNumber Version
	{
		get
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			return new VersionNumber(1, 0, 0);
		}
	}

	public override bool ForceModded => true;

	public override Type Type => typeof(GatherManagerModule);

	public override bool EnabledByDefault => false;

	public override void Init()
	{
		base.Init();
		Singleton = this;
	}

	private object OnCollectiblePickup(CollectibleEntity entity, BasePlayer reciever, bool eat)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Invalid comparison between Unknown and I4
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		NetworkableId entityId = ((BaseNetworkable)entity).net.ID;
		if (!_processedEntities.Add(entityId))
		{
			return false;
		}
		for (int i = 0; i < entity.itemList.Length; i++)
		{
			ItemAmount val = entity.itemList[i];
			Item val2 = ByDefinition(val.itemDef, (int)val.amount, 0uL, KindTypes.Pickup);
			if (val2 == null)
			{
				continue;
			}
			if (eat && (int)val2.info.category == 7 && (Object)(object)reciever != (Object)null)
			{
				ItemModConsume component = ((Component)val2.info).GetComponent<ItemModConsume>();
				if ((Object)(object)component != (Object)null)
				{
					((ItemMod)component).DoAction(val2, reciever);
					continue;
				}
			}
			if (Object.op_Implicit((Object)(object)reciever))
			{
				Azure.OnGatherItem(val2.info.shortname, val2.amount, (BaseEntity)(object)entity, reciever, (AttackEntity)null);
				((BaseEntity)reciever).GiveItem(val2, (GiveItemReason)1, (GiveItemOptions)0);
			}
			else
			{
				val2.Drop(((Component)entity).transform.position + Vector3.up * 0.5f, Vector3.up, default(Quaternion));
			}
		}
		if (((ResourceRef<GameObject>)(object)entity.pickupEffect).isValid)
		{
			server.Run(((ResourceRef<GameObject>)(object)entity.pickupEffect).resourcePath, ((Component)entity).transform.position, ((Component)entity).transform.up, (Connection)null, false, (List<Connection>)null, 0, (Type)0);
		}
		RandomItemDispenser val3 = PrefabAttribute.server.Find<RandomItemDispenser>(((BaseNetworkable)entity).prefabID);
		if ((PrefabAttribute)(object)val3 != (PrefabAttribute)null)
		{
			val3.DistributeItems(reciever, ((Component)entity).transform.position);
		}
		base.NextFrame((Action)(() =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			_processedEntities.Remove(entityId);
			if (!((Object)(object)entity == (Object)null) && !((BaseNetworkable)entity).IsDestroyed)
			{
				((BaseNetworkable)entity).Kill((DestroyMode)0, true);
			}
		}));
		return false;
	}

	private void OnExcavatorGather(ExcavatorArm arm, Item item)
	{
		item.amount = GetAmount(item.info, item.amount, KindTypes.Excavator);
	}

	private void OnQuarryGather(MiningQuarry quarry, Item item)
	{
		item.amount = GetAmount(item.info, item.amount, KindTypes.Quarry);
	}

	private void OnGrowableGathered(GrowableEntity entity, Item item, BasePlayer player)
	{
		item.amount = GetAmount(item.info, item.amount, KindTypes.Gather);
	}

	private void OnDispenserBonus(ResourceDispenser dispenser, BasePlayer player, Item item)
	{
		item.amount = GetAmount(item.info, item.amount, KindTypes.Gather);
	}

	private void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
	{
		item.amount = GetAmount(item.info, item.amount, KindTypes.Gather);
	}

	private void OnFishCatch(Item item)
	{
		item.amount = GetAmount(item.info, item.amount, KindTypes.Gather);
	}

	private Item ByID(int itemID, int amount, ulong skin, KindTypes kind)
	{
		return ByDefinition(ItemManager.FindItemDefinition(itemID), amount, skin, kind);
	}

	private Item ByDefinition(ItemDefinition itemDefinition, int amount, ulong skin, KindTypes kind)
	{
		return ItemManager.Create(itemDefinition, GetAmount(itemDefinition, amount, kind), skin, true, 0uL);
	}

	private int GetAmount(ItemDefinition itemDefinition, int amount, KindTypes kind)
	{
		Dictionary<string, float> dictionary = kind switch
		{
			KindTypes.Pickup => base.ConfigInstance.Pickup, 
			KindTypes.Gather => base.ConfigInstance.Gather, 
			KindTypes.Quarry => base.ConfigInstance.Quarry, 
			KindTypes.Excavator => base.ConfigInstance.Excavator, 
			_ => throw new Exception("Invalid GetAmount kind"), 
		};
		if (!dictionary.TryGetValue(itemDefinition.shortname, out var value) && !dictionary.TryGetValue("*", out value))
		{
			value = 1f;
		}
		return Mathf.CeilToInt((float)amount * value);
	}

	public override object InternalCallHook(uint hook, object[] args)
	{
		//IL_06dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0676: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0547: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_069b: Unknown result type (might be due to invalid IL or missing references)
		//IL_044c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_060e: Unknown result type (might be due to invalid IL or missing references)
		//IL_056c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0501: Unknown result type (might be due to invalid IL or missing references)
		//IL_0471: Unknown result type (might be due to invalid IL or missing references)
		//IL_0633: Unknown result type (might be due to invalid IL or missing references)
		int? num = args?.Length;
		object obj = ((num > 0) ? args[0] : null);
		object obj2 = ((num > 1) ? args[1] : null);
		object obj3 = ((num > 2) ? args[2] : null);
		object obj4 = ((num > 3) ? args[3] : null);
		try
		{
			switch (hook)
			{
			case 729326306u:
			{
				bool flag13 = obj is ItemDefinition || obj == null;
				ItemDefinition itemDefinition2 = ((!flag13) ? ((ItemDefinition)null) : ((ItemDefinition)(obj ?? null)));
				bool flag14 = obj2 is int || obj2 == null;
				int amount2 = (flag14 ? ((int)(obj2 ?? ((object)0))) : 0);
				bool flag15 = obj3 is ulong || obj3 == null;
				ulong skin = (flag15 ? ((ulong)(obj3 ?? ((object)0uL))) : 0);
				bool flag16 = obj4 is KindTypes || obj4 == null;
				KindTypes kind2 = (flag16 ? ((KindTypes)(obj4 ?? ((object)KindTypes.Pickup))) : KindTypes.Pickup);
				if (flag13 & flag14 & flag15 & flag16)
				{
					return ByDefinition(itemDefinition2, amount2, skin, kind2);
				}
				break;
			}
			case 3391923604u:
			{
				bool flag20 = obj is int || obj == null;
				int itemID = (flag20 ? ((int)(obj ?? ((object)0))) : 0);
				bool flag21 = obj2 is int || obj2 == null;
				int amount3 = (flag21 ? ((int)(obj2 ?? ((object)0))) : 0);
				bool flag22 = obj3 is ulong || obj3 == null;
				ulong skin2 = (flag22 ? ((ulong)(obj3 ?? ((object)0uL))) : 0);
				bool flag23 = obj4 is KindTypes || obj4 == null;
				KindTypes kind3 = (flag23 ? ((KindTypes)(obj4 ?? ((object)KindTypes.Pickup))) : KindTypes.Pickup);
				if (flag20 & flag21 & flag22 & flag23)
				{
					return ByID(itemID, amount3, skin2, kind3);
				}
				break;
			}
			case 2628990431u:
			{
				bool flag6 = obj is ItemDefinition || obj == null;
				ItemDefinition itemDefinition = ((!flag6) ? ((ItemDefinition)null) : ((ItemDefinition)(obj ?? null)));
				bool flag7 = obj2 is int || obj2 == null;
				int amount = (flag7 ? ((int)(obj2 ?? ((object)0))) : 0);
				bool flag8 = obj3 is KindTypes || obj3 == null;
				KindTypes kind = (flag8 ? ((KindTypes)(obj3 ?? ((object)KindTypes.Pickup))) : KindTypes.Pickup);
				if (flag6 & flag7 & flag8)
				{
					return GetAmount(itemDefinition, amount, kind);
				}
				break;
			}
			case 3290943891u:
			{
				bool flag26 = obj is CollectibleEntity || obj == null;
				CollectibleEntity entity3 = ((!flag26) ? ((CollectibleEntity)null) : ((CollectibleEntity)(obj ?? null)));
				bool flag27 = obj2 is BasePlayer || obj2 == null;
				BasePlayer reciever = ((!flag27) ? ((BasePlayer)null) : ((BasePlayer)(obj2 ?? null)));
				bool flag28 = obj3 is bool || obj3 == null;
				bool eat = flag28 && (bool)(obj3 ?? ((object)false));
				if (flag26 & flag27 & flag28)
				{
					return OnCollectiblePickup(entity3, reciever, eat);
				}
				break;
			}
			case 2399681302u:
			{
				bool flag10 = obj is ResourceDispenser || obj == null;
				ResourceDispenser dispenser = ((!flag10) ? ((ResourceDispenser)null) : ((ResourceDispenser)(obj ?? null)));
				bool flag11 = obj2 is BasePlayer || obj2 == null;
				BasePlayer player2 = ((!flag11) ? ((BasePlayer)null) : ((BasePlayer)(obj2 ?? null)));
				bool flag12 = obj3 is Item || obj3 == null;
				Item item4 = ((!flag12) ? ((Item)null) : ((Item)(obj3 ?? null)));
				if (flag10 & flag11 & flag12)
				{
					OnDispenserBonus(dispenser, player2, item4);
					return null;
				}
				break;
			}
			case 2949903609u:
			{
				bool flag17 = obj is ResourceDispenser || obj == null;
				ResourceDispenser dispenser2 = ((!flag17) ? ((ResourceDispenser)null) : ((ResourceDispenser)(obj ?? null)));
				bool flag18 = obj2 is BaseEntity || obj2 == null;
				BaseEntity entity2 = ((!flag18) ? ((BaseEntity)null) : ((BaseEntity)(obj2 ?? null)));
				bool flag19 = obj3 is Item || obj3 == null;
				Item item5 = ((!flag19) ? ((Item)null) : ((Item)(obj3 ?? null)));
				if (flag17 & flag18 & flag19)
				{
					OnDispenserGather(dispenser2, entity2, item5);
					return null;
				}
				break;
			}
			case 2447060701u:
			{
				bool flag24 = obj is ExcavatorArm || obj == null;
				ExcavatorArm arm = ((!flag24) ? ((ExcavatorArm)null) : ((ExcavatorArm)(obj ?? null)));
				bool flag25 = obj2 is Item || obj2 == null;
				Item item6 = ((!flag25) ? ((Item)null) : ((Item)(obj2 ?? null)));
				if (flag24 & flag25)
				{
					OnExcavatorGather(arm, item6);
					return null;
				}
				break;
			}
			case 1173046409u:
			{
				bool flag9 = obj is Item || obj == null;
				Item item3 = ((!flag9) ? ((Item)null) : ((Item)(obj ?? null)));
				if (flag9)
				{
					OnFishCatch(item3);
					return null;
				}
				break;
			}
			case 2863302180u:
			{
				bool flag3 = obj is GrowableEntity || obj == null;
				GrowableEntity entity = ((!flag3) ? ((GrowableEntity)null) : ((GrowableEntity)(obj ?? null)));
				bool flag4 = obj2 is Item || obj2 == null;
				Item item2 = ((!flag4) ? ((Item)null) : ((Item)(obj2 ?? null)));
				bool flag5 = obj3 is BasePlayer || obj3 == null;
				BasePlayer player = ((!flag5) ? ((BasePlayer)null) : ((BasePlayer)(obj3 ?? null)));
				if (flag3 & flag4 & flag5)
				{
					OnGrowableGathered(entity, item2, player);
					return null;
				}
				break;
			}
			case 2662773903u:
			{
				bool flag = obj is MiningQuarry || obj == null;
				MiningQuarry quarry = ((!flag) ? ((MiningQuarry)null) : ((MiningQuarry)(obj ?? null)));
				bool flag2 = obj2 is Item || obj2 == null;
				Item item = ((!flag2) ? ((Item)null) : ((Item)(obj2 ?? null)));
				if (flag & flag2)
				{
					OnQuarryGather(quarry, item);
					return null;
				}
				break;
			}
			}
		}
		catch (Exception ex)
		{
			Logger.Error((object)string.Format("Failed to call internal hook '{0}' on module '{1} v{2}' [{3}]", new object[4]
			{
				HookStringPool.GetOrAdd(hook),
				((CarbonModule<GatherManagerConfig, EmptyModuleData>)this).Name,
				((BaseHookable)this).Version,
				hook
			}), ex);
			((BaseHookable)this).OnException(hook);
		}
		return null;
	}
}

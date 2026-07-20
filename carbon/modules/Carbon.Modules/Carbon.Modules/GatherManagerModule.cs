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

	public override VersionNumber Version => new VersionNumber(1, 0, 0);

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
		base.NextFrame((Action)delegate
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			_processedEntities.Remove(entityId);
			if (!((Object)(object)entity == (Object)null) && !((BaseNetworkable)entity).IsDestroyed)
			{
				((BaseNetworkable)entity).Kill((DestroyMode)0, true);
			}
		});
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
		//IL_0787: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_071b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0679: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0637: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_054e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0746: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0579: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cf: Unknown result type (might be due to invalid IL or missing references)
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
				bool flag = ((obj is ItemDefinition || obj == null) ? true : false);
				bool flag24 = flag;
				ItemDefinition itemDefinition2 = ((!flag24) ? ((ItemDefinition)null) : ((ItemDefinition)(obj ?? null)));
				flag = ((obj2 is int || obj2 == null) ? true : false);
				bool flag25 = flag;
				int amount3 = (flag25 ? ((int)(obj2 ?? ((object)0))) : 0);
				flag = ((obj3 is ulong || obj3 == null) ? true : false);
				bool flag26 = flag;
				ulong skin2 = (flag26 ? ((ulong)(obj3 ?? ((object)0uL))) : 0);
				flag = ((obj4 is KindTypes || obj4 == null) ? true : false);
				bool flag27 = flag;
				KindTypes kind3 = (flag27 ? ((KindTypes)(obj4 ?? ((object)KindTypes.Pickup))) : KindTypes.Pickup);
				if (flag24 && flag25 && flag26 && flag27)
				{
					return ByDefinition(itemDefinition2, amount3, skin2, kind3);
				}
				break;
			}
			case 3391923604u:
			{
				bool flag = ((obj is int || obj == null) ? true : false);
				bool flag14 = flag;
				int itemID = (flag14 ? ((int)(obj ?? ((object)0))) : 0);
				flag = ((obj2 is int || obj2 == null) ? true : false);
				bool flag15 = flag;
				int amount2 = (flag15 ? ((int)(obj2 ?? ((object)0))) : 0);
				flag = ((obj3 is ulong || obj3 == null) ? true : false);
				bool flag16 = flag;
				ulong skin = (flag16 ? ((ulong)(obj3 ?? ((object)0uL))) : 0);
				flag = ((obj4 is KindTypes || obj4 == null) ? true : false);
				bool flag17 = flag;
				KindTypes kind2 = (flag17 ? ((KindTypes)(obj4 ?? ((object)KindTypes.Pickup))) : KindTypes.Pickup);
				if (flag14 && flag15 && flag16 && flag17)
				{
					return ByID(itemID, amount2, skin, kind2);
				}
				break;
			}
			case 2628990431u:
			{
				bool flag = ((obj is ItemDefinition || obj == null) ? true : false);
				bool flag11 = flag;
				ItemDefinition itemDefinition = ((!flag11) ? ((ItemDefinition)null) : ((ItemDefinition)(obj ?? null)));
				flag = ((obj2 is int || obj2 == null) ? true : false);
				bool flag12 = flag;
				int amount = (flag12 ? ((int)(obj2 ?? ((object)0))) : 0);
				flag = ((obj3 is KindTypes || obj3 == null) ? true : false);
				bool flag13 = flag;
				KindTypes kind = (flag13 ? ((KindTypes)(obj3 ?? ((object)KindTypes.Pickup))) : KindTypes.Pickup);
				if (flag11 && flag12 && flag13)
				{
					return GetAmount(itemDefinition, amount, kind);
				}
				break;
			}
			case 3290943891u:
			{
				bool flag = ((obj is CollectibleEntity || obj == null) ? true : false);
				bool flag18 = flag;
				CollectibleEntity entity3 = ((!flag18) ? ((CollectibleEntity)null) : ((CollectibleEntity)(obj ?? null)));
				flag = ((obj2 is BasePlayer || obj2 == null) ? true : false);
				bool flag19 = flag;
				BasePlayer reciever = ((!flag19) ? ((BasePlayer)null) : ((BasePlayer)(obj2 ?? null)));
				flag = ((obj3 is bool || obj3 == null) ? true : false);
				bool flag20 = flag;
				bool eat = flag20 && (bool)(obj3 ?? ((object)false));
				if (flag18 && flag19 && flag20)
				{
					return OnCollectiblePickup(entity3, reciever, eat);
				}
				break;
			}
			case 2399681302u:
			{
				bool flag = ((obj is ResourceDispenser || obj == null) ? true : false);
				bool flag21 = flag;
				ResourceDispenser dispenser2 = ((!flag21) ? ((ResourceDispenser)null) : ((ResourceDispenser)(obj ?? null)));
				flag = ((obj2 is BasePlayer || obj2 == null) ? true : false);
				bool flag22 = flag;
				BasePlayer player2 = ((!flag22) ? ((BasePlayer)null) : ((BasePlayer)(obj2 ?? null)));
				flag = ((obj3 is Item || obj3 == null) ? true : false);
				bool flag23 = flag;
				Item item5 = ((!flag23) ? ((Item)null) : ((Item)(obj3 ?? null)));
				if (flag21 && flag22 && flag23)
				{
					OnDispenserBonus(dispenser2, player2, item5);
					return null;
				}
				break;
			}
			case 2949903609u:
			{
				bool flag = ((obj is ResourceDispenser || obj == null) ? true : false);
				bool flag7 = flag;
				ResourceDispenser dispenser = ((!flag7) ? ((ResourceDispenser)null) : ((ResourceDispenser)(obj ?? null)));
				flag = ((obj2 is BaseEntity || obj2 == null) ? true : false);
				bool flag8 = flag;
				BaseEntity entity2 = ((!flag8) ? ((BaseEntity)null) : ((BaseEntity)(obj2 ?? null)));
				flag = ((obj3 is Item || obj3 == null) ? true : false);
				bool flag9 = flag;
				Item item3 = ((!flag9) ? ((Item)null) : ((Item)(obj3 ?? null)));
				if (flag7 && flag8 && flag9)
				{
					OnDispenserGather(dispenser, entity2, item3);
					return null;
				}
				break;
			}
			case 2447060701u:
			{
				bool flag = ((obj is ExcavatorArm || obj == null) ? true : false);
				bool flag28 = flag;
				ExcavatorArm arm = ((!flag28) ? ((ExcavatorArm)null) : ((ExcavatorArm)(obj ?? null)));
				flag = ((obj2 is Item || obj2 == null) ? true : false);
				bool flag29 = flag;
				Item item6 = ((!flag29) ? ((Item)null) : ((Item)(obj2 ?? null)));
				if (flag28 && flag29)
				{
					OnExcavatorGather(arm, item6);
					return null;
				}
				break;
			}
			case 1173046409u:
			{
				bool flag = ((obj is Item || obj == null) ? true : false);
				bool flag10 = flag;
				Item item4 = ((!flag10) ? ((Item)null) : ((Item)(obj ?? null)));
				if (flag10)
				{
					OnFishCatch(item4);
					return null;
				}
				break;
			}
			case 2863302180u:
			{
				bool flag = ((obj is GrowableEntity || obj == null) ? true : false);
				bool flag4 = flag;
				GrowableEntity entity = ((!flag4) ? ((GrowableEntity)null) : ((GrowableEntity)(obj ?? null)));
				flag = ((obj2 is Item || obj2 == null) ? true : false);
				bool flag5 = flag;
				Item item2 = ((!flag5) ? ((Item)null) : ((Item)(obj2 ?? null)));
				flag = ((obj3 is BasePlayer || obj3 == null) ? true : false);
				bool flag6 = flag;
				BasePlayer player = ((!flag6) ? ((BasePlayer)null) : ((BasePlayer)(obj3 ?? null)));
				if (flag4 && flag5 && flag6)
				{
					OnGrowableGathered(entity, item2, player);
					return null;
				}
				break;
			}
			case 2662773903u:
			{
				bool flag = ((obj is MiningQuarry || obj == null) ? true : false);
				bool flag2 = flag;
				MiningQuarry quarry = ((!flag2) ? ((MiningQuarry)null) : ((MiningQuarry)(obj ?? null)));
				flag = ((obj2 is Item || obj2 == null) ? true : false);
				bool flag3 = flag;
				Item item = ((!flag3) ? ((Item)null) : ((Item)(obj2 ?? null)));
				if (flag2 && flag3)
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

using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class SimpleBuildingBlock : StabilityEntity, ISimpleUpgradable, IReskinCallback
{
	public List<ItemDefinition> UpgradeItems;

	public Menu.Option UpgradeMenu;

	private GameObject currentModel;

	private SimpleBuildingBlockModelVariant[] variants;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SimpleBuildingBlock.OnRpcMessage"))
		{
			if (rpc == 2824056853u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - DoSimpleUpgrade"));
				}
				using (TimeWarning.New("DoSimpleUpgrade"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2824056853u, "DoSimpleUpgrade", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2824056853u, "DoSimpleUpgrade", this, player, 3f))
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
							DoSimpleUpgrade(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in DoSimpleUpgrade");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void InitShared()
	{
		base.InitShared();
		variants = PrefabAttribute.server.FindAll<SimpleBuildingBlockModelVariant>(prefabID);
	}

	public List<ItemDefinition> GetUpgradeItems()
	{
		return UpgradeItems;
	}

	public bool CanUpgrade(BasePlayer player, ItemDefinition upgradeItem)
	{
		return global::SimpleUpgrade.CanUpgrade(this, upgradeItem, player);
	}

	public void DoUpgrade(BasePlayer player, ItemDefinition upgradeItem)
	{
		global::SimpleUpgrade.DoUpgrade(this, player, upgradeItem);
	}

	public Menu.Option GetUpgradeMenuOption()
	{
		return UpgradeMenu;
	}

	public bool UpgradingEnabled()
	{
		if (UpgradeItems != null)
		{
			return UpgradeItems.Count > 0;
		}
		return false;
	}

	public bool CostIsItem()
	{
		return true;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	public void DoSimpleUpgrade(RPCMessage msg)
	{
		if (base.SecondsSinceAttacked < 30f)
		{
			msg.player.ShowToast(GameTip.Styles.Error, ConstructionErrors.CantUpgradeRecentlyDamaged, false, (30f - base.SecondsSinceAttacked).ToString("N0"));
			return;
		}
		int num = msg.read.Int32();
		if (num >= 0 && num < UpgradeItems.Count)
		{
			ItemDefinition upgradeItem = UpgradeItems[num];
			if (CanUpgrade(msg.player, upgradeItem))
			{
				DoUpgrade(msg.player, upgradeItem);
			}
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		PopulateVariants();
	}

	private void PopulateVariants()
	{
		if (base.isServer && variants.Any())
		{
			ulong value = net.ID.Value;
			SeedRandom.Wanghash(ref value);
			SeedRandom.Wanghash(ref value);
			SeedRandom.Wanghash(ref value);
			int num = (int)(value % (ulong)variants.Length);
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(variants[num].Flag, b: true);
		}
	}

	public void OnReskinned(BasePlayer byPlayer)
	{
		PopulateVariants();
	}

	public override bool CanBeRedirectSwapped(BasePlayer player)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			Vis.Entities(WorldSpaceBounds(), (List<BaseEntity>)(object)val, -2145386240, (QueryTriggerInteraction)2);
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				if (!((Object)(object)item == (Object)null) && !item.isClient && !((Object)(object)item == (Object)(object)this) && !(item is BuildingBlock) && !(item is SimpleBuildingBlock) && !(item is Door) && !(item is BaseOven) && !(item is Barricade))
				{
					if (!string.IsNullOrEmpty(ConstructionErrors.GetTranslatedNameFromEntity(item)))
					{
						SprayCan.LastReskinError = ConstructionErrors.BlockedBy;
						SprayCan.LastReskinErrorEntity = item;
					}
					else
					{
						SprayCan.LastReskinError = SprayCan.BlockedBySomething;
					}
					return false;
				}
			}
			return base.CanBeRedirectSwapped(player);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void SetVariant(int index)
	{
		int num = index % variants.Length;
		SimpleBuildingBlockModelVariant[] array = variants;
		foreach (SimpleBuildingBlockModelVariant simpleBuildingBlockModelVariant in array)
		{
			SetFlagLocal(simpleBuildingBlockModelVariant.Flag, b: false);
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(variants[num].Flag, b: true);
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
		if (!base.isServer)
		{
			return;
		}
		SimpleBuildingBlockModelVariant[] array = variants;
		foreach (SimpleBuildingBlockModelVariant simpleBuildingBlockModelVariant in array)
		{
			if (HasFlag(simpleBuildingBlockModelVariant.Flag))
			{
				using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
				flagsUpdateScope.Set(simpleBuildingBlockModelVariant.Flag, b: false);
			}
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (variants != null)
		{
			RefreshVariant();
		}
	}

	private void RefreshVariant()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (variants == null)
		{
			return;
		}
		base.gameManager.Retire(currentModel);
		SimpleBuildingBlockModelVariant[] array = variants;
		foreach (SimpleBuildingBlockModelVariant simpleBuildingBlockModelVariant in array)
		{
			if (HasFlag(simpleBuildingBlockModelVariant.Flag))
			{
				GameObject val = base.gameManager.CreatePrefab(simpleBuildingBlockModelVariant.prefab.resourcePath, ((Component)this).transform);
				if (Object.op_Implicit((Object)(object)val))
				{
					val.transform.localPosition = simpleBuildingBlockModelVariant.localPosition;
					val.transform.localRotation = simpleBuildingBlockModelVariant.localRotation;
				}
				currentModel = val;
			}
		}
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		base.gameManager.Retire(currentModel);
		currentModel = null;
	}
}

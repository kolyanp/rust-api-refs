using System;
using ConVar;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class InvisibleVendingMachine : NPCVendingMachine
{
	public GameObjectRef buyEffect;

	public NPCVendingOrderManifest vmoManifest;

	public bool canRefreshOrders;

	public EntityRef<NPCShopKeeper> cachedShopKeeper;

	public const Flags HasAttachedShopkeeper = Flags.Reserved11;

	private bool hasCurrentPlayerPurchasedSomething;

	private static ListHashSet<InvisibleVendingMachine> allMachines = new ListHashSet<InvisibleVendingMachine>();

	public TimeUntil nextOrderRefresh;

	protected override bool BlockOrderRefreshOnLoad => canRefreshOrders;

	public static InvisibleVendingMachine GetMachineAtPosition(float tolerance, Vector3 position)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<InvisibleVendingMachine> enumerator = allMachines.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				InvisibleVendingMachine current = enumerator.Current;
				if ((Object)(object)current != (Object)null && current.Distance(position) < tolerance)
				{
					return current;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return null;
	}

	public void KeeperLookAt(Vector3 pos)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		NPCShopKeeper nPCShopKeeper = cachedShopKeeper.Get(base.isServer);
		if (!((Object)(object)nPCShopKeeper == (Object)null))
		{
			nPCShopKeeper.SetAimDirection(Vector3Ex.Direction2D(pos, ((Component)nPCShopKeeper).transform.position));
		}
	}

	public override bool HasVendingSounds()
	{
		return false;
	}

	public override float GetBuyDuration()
	{
		return 0.5f;
	}

	public override void CompletePendingOrder()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		Effect.server.Run(buyEffect.resourcePath, ((Component)this).transform.position, Vector3.up);
		if (cachedShopKeeper.TryGet(base.isServer, out var entity))
		{
			entity.Server_StartGesture("victory", BasePlayer.GestureStartSource.ServerAction);
			if ((Object)(object)vend_Player != (Object)null)
			{
				entity.SetAimDirection(Vector3Ex.Direction2D(((Component)vend_Player).transform.position, ((Component)entity).transform.position));
			}
		}
		hasCurrentPlayerPurchasedSomething = true;
		NotifyShopkeeper(NPCShopKeeper.ShopkeeperEvent.SaleSuccess);
		base.CompletePendingOrder();
	}

	private void NotifyShopkeeper(NPCShopKeeper.ShopkeeperEvent eventType)
	{
		if ((Object)(object)cachedShopKeeper.Get(base.isServer) != (Object)null)
		{
			cachedShopKeeper.Get(base.isServer).NotifyEvent(eventType);
		}
	}

	public override bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		KeeperLookAt(((Component)player).transform.position);
		NotifyShopkeeper(NPCShopKeeper.ShopkeeperEvent.Talk);
		hasCurrentPlayerPurchasedSomething = false;
		return base.PlayerOpenLoot(player, panelToOpen, true);
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		base.PlayerStoppedLooting(player);
		NotifyShopkeeper((!hasCurrentPlayerPurchasedSomething) ? NPCShopKeeper.ShopkeeperEvent.EndConvoBoughtNothing : NPCShopKeeper.ShopkeeperEvent.EndConvoBoughtSomething);
	}

	public override void Save(SaveInfo info)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if ((Object)(object)vmoManifest != (Object)null && info.msg.vendingMachine != null)
		{
			info.msg.vendingMachine.vmoIndex = vmoManifest.GetIndex(vendingOrders);
		}
		info.msg.npcVendingMachine = Pool.Get<NPCVendingMachine>();
		info.msg.npcVendingMachine.attachedNpc = cachedShopKeeper.uid;
		info.msg.npcVendingMachine.nextRefresh = ((TimeUntil)(ref nextOrderRefresh)).LeftFrom(info.cachedTime.Time);
	}

	public override void ServerInit()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		if (vmoManifest.GetIndex(vendingOrders) == -1 && !(this is RentableShopVendingMachine))
		{
			Debug.LogError((object)"VENDING ORDERS NOT FOUND! Did you forget to add these orders to the VMOManifest?");
		}
		if (canRefreshOrders)
		{
			nextOrderRefresh = TimeUntil.op_Implicit(Server.waterWellNpcSalesRefreshFrequency * 60f * 60f);
			InvokeRepeating(CheckSellOrderRefresh, 30f, 30f);
		}
		allMachines.TryAdd(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		allMachines.Remove(this);
	}

	public void CheckSellOrderRefresh()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (TimeUntil.op_Implicit(nextOrderRefresh) < 0f)
		{
			nextOrderRefresh = TimeUntil.op_Implicit(Server.waterWellNpcSalesRefreshFrequency * 60f * 60f);
			InstallFromVendingOrders();
		}
	}

	public void SetAttachedNPC(NPCShopKeeper shopkeeper)
	{
		cachedShopKeeper.Set(shopkeeper);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved11, (Object)(object)shopkeeper != (Object)null);
		}
		SendNetworkUpdate();
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if ((Object)(object)cachedShopKeeper.Get(base.isServer) == (Object)null)
		{
			return false;
		}
		return base.CanBeLooted(player);
	}

	protected override bool CanShop(BasePlayer bp)
	{
		if (base.CanShop(bp))
		{
			return (Object)(object)cachedShopKeeper.Get(base.isServer) != (Object)null;
		}
		return false;
	}

	public override void Load(LoadInfo info)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.fromDisk && (Object)(object)vmoManifest != (Object)null && info.msg.vendingMachine != null)
		{
			NPCVendingOrder fromIndex = vmoManifest.GetFromIndex(info.msg.vendingMachine.vmoIndex);
			vendingOrders = fromIndex;
		}
		if (info.msg.npcVendingMachine != null)
		{
			cachedShopKeeper.uid = info.msg.npcVendingMachine.attachedNpc;
			if (base.isServer)
			{
				nextOrderRefresh = TimeUntil.op_Implicit(info.msg.npcVendingMachine.nextRefresh);
			}
		}
	}
}

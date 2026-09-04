using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class RentableShopVendingMachine : InvisibleVendingMachine, IPowergridEntity
{
	private RentableShop _cachedParent;

	[SerializeField]
	private ItemListScriptableObject BannedItemsList;

	private const Flags Refrigerated = Flags.Reserved12;

	private RentableShop ParentRentableShop
	{
		get
		{
			if ((Object)(object)_cachedParent == (Object)null)
			{
				_cachedParent = GetParentEntity() as RentableShop;
			}
			return _cachedParent;
		}
	}

	public override bool ShouldRecordStats => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RentableShopVendingMachine.OnRpcMessage"))
		{
			if (rpc == 892362950 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_AddSellOrderRelaxedDistance"));
				}
				using (TimeWarning.New("RPC_AddSellOrderRelaxedDistance"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(892362950u, "RPC_AddSellOrderRelaxedDistance", this, player, 12f))
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
							RPC_AddSellOrderRelaxedDistance(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_AddSellOrderRelaxedDistance");
					}
				}
				return true;
			}
			if (rpc == 169239598 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Broadcast"));
				}
				using (TimeWarning.New("RPC_Broadcast"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(169239598u, "RPC_Broadcast", this, player, 3f))
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
							RPC_Broadcast(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_Broadcast");
					}
				}
				return true;
			}
			if (rpc == 1017551721 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_BuyItem"));
				}
				using (TimeWarning.New("RPC_BuyItem"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1017551721u, "RPC_BuyItem", this, player, 9f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_BuyItem(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RPC_BuyItem");
					}
				}
				return true;
			}
			if (rpc == 3483523947u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_DeleteAllSellOrdersDistanceCheckOnly"));
				}
				using (TimeWarning.New("RPC_DeleteAllSellOrdersDistanceCheckOnly"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3483523947u, "RPC_DeleteAllSellOrdersDistanceCheckOnly", this, player, 9f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_DeleteAllSellOrdersDistanceCheckOnly(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in RPC_DeleteAllSellOrdersDistanceCheckOnly");
					}
				}
				return true;
			}
			if (rpc == 2204790576u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_DeleteSellOrderDistanceCheckOnly"));
				}
				using (TimeWarning.New("RPC_DeleteSellOrderDistanceCheckOnly"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2204790576u, "RPC_DeleteSellOrderDistanceCheckOnly", this, player, 9f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg6 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_DeleteSellOrderDistanceCheckOnly(msg6);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in RPC_DeleteSellOrderDistanceCheckOnly");
					}
				}
				return true;
			}
			if (rpc == 407432418 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_RequestLongTermData"));
				}
				using (TimeWarning.New("RPC_RequestLongTermData"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(407432418u, "RPC_RequestLongTermData", this, player, 9f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg7 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_RequestLongTermData(msg7);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
						player.Kick("RPC Error in RPC_RequestLongTermData");
					}
				}
				return true;
			}
			if (rpc == 1907075143 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_RequestPurchaseData"));
				}
				using (TimeWarning.New("RPC_RequestPurchaseData"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1907075143u, "RPC_RequestPurchaseData", this, player, 9f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg8 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_RequestPurchaseData(msg8);
						}
					}
					catch (Exception ex7)
					{
						Debug.LogException(ex7);
						player.Kick("RPC Error in RPC_RequestPurchaseData");
					}
				}
				return true;
			}
			if (rpc == 81697804 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_UpdateShopNameDistanceCheckOnly"));
				}
				using (TimeWarning.New("RPC_UpdateShopNameDistanceCheckOnly"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(81697804u, "RPC_UpdateShopNameDistanceCheckOnly", this, player, 9f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg9 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_UpdateShopNameDistanceCheckOnly(msg9);
						}
					}
					catch (Exception ex8)
					{
						Debug.LogException(ex8);
						player.Kick("RPC Error in RPC_UpdateShopNameDistanceCheckOnly");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override bool CanPlayerAdmin(BasePlayer player)
	{
		if ((Object)(object)ParentRentableShop != (Object)null)
		{
			return ParentRentableShop.IsOwner(player);
		}
		return false;
	}

	public override float GetSpoilMultiplier(Item arg)
	{
		if (!Powergrid.enabled)
		{
			return base.GetSpoilMultiplier(arg);
		}
		if (HasFlag(Flags.Reserved12))
		{
			return PoweredFoodSpoilageRateMultiplier;
		}
		return base.GetSpoilMultiplier(arg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (BannedItemsList != null)
		{
			base.inventory.SetBlacklist(BannedItemsList.Items);
		}
	}

	public override void RPC_Broadcast(RPCMessage msg)
	{
	}

	[RPC_Server.MaxDistance(12f)]
	[RPC_Server]
	public void RPC_AddSellOrderRelaxedDistance(RPCMessage msg)
	{
		RPC_AddSellOrder(msg);
	}

	[RPC_Server.MaxDistance(9f)]
	[RPC_Server]
	private void RPC_UpdateShopNameDistanceCheckOnly(RPCMessage msg)
	{
		base.RPC_UpdateShopName(msg);
	}

	[RPC_Server.MaxDistance(9f)]
	[RPC_Server]
	private void RPC_BuyItem(RPCMessage msg)
	{
		BuyItem(msg);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(9f)]
	private void RPC_RequestLongTermData(RPCMessage msg)
	{
		SV_RequestLongTermData(msg);
	}

	[RPC_Server.MaxDistance(9f)]
	[RPC_Server]
	private void RPC_RequestPurchaseData(RPCMessage msg)
	{
		SV_RequestPurchaseData(msg);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(9f)]
	private void RPC_DeleteSellOrderDistanceCheckOnly(RPCMessage msg)
	{
		RPC_DeleteSellOrder(msg);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(9f)]
	private void RPC_DeleteAllSellOrdersDistanceCheckOnly(RPCMessage msg)
	{
		RPC_DeleteAllSellOrders(msg);
	}

	public override void TakeCurrencyItem(Item takenCurrencyItem)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (!takenCurrencyItem.MoveToContainer(base.inventory))
		{
			takenCurrencyItem.Drop(base.inventory.dropPosition, Vector3.zero);
		}
	}

	public override bool CanOpenLootPanel(BasePlayer player, string panelName)
	{
		if ((Object)(object)player != (Object)null && GetParentEntity() is RentableShop rentableShop && rentableShop.IsIntruder(player.userID))
		{
			return true;
		}
		return base.CanOpenLootPanel(player, panelName);
	}

	public override PlayerInventory.CanMoveFromResponse CanMoveFrom(BasePlayer player, Item item)
	{
		if ((Object)(object)player != (Object)null && GetParentEntity() is RentableShop rentableShop && rentableShop.IsIntruder(player.userID))
		{
			return PlayerInventory.CanMoveFromResponse.Success();
		}
		return base.CanMoveFrom(player, item);
	}

	public bool Server_ShouldConnectToPowergrid()
	{
		return true;
	}

	public void Server_OnPowergridStageChanged(int newStage)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Reserved12, newStage > 0);
	}
}

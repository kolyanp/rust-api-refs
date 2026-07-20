using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class ApartmentTerminal : ComputerStation
{
	public enum ValueBucket
	{
		None,
		Low,
		Medium,
		High
	}

	private ApartmentBuilding cachedBuilding;

	private List<RentableShop> cachedBlockShops;

	[Header("Apartment Terminal")]
	public float MediumValueThreshold = 100f;

	public float HighValueThreshold = 500f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ApartmentTerminal.OnRpcMessage"))
		{
			if (rpc == 3958206997u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_RequestProperties"));
				}
				using (TimeWarning.New("SERVER_RequestProperties"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3958206997u, "SERVER_RequestProperties", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.FromMounted.Test(3958206997u, "SERVER_RequestProperties", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3958206997u, "SERVER_RequestProperties", this, player, 3f))
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
							SERVER_RequestProperties(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SERVER_RequestProperties");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private ApartmentBuilding ResolveBuilding()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)cachedBuilding != (Object)null && !cachedBuilding.IsDestroyed)
		{
			return cachedBuilding;
		}
		ApartmentBuilding apartmentBuilding = null;
		float num = ApartmentBuilding.MaxRadiusSearch;
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is ApartmentBuilding apartmentBuilding2)
				{
					float num2 = Vector3.Distance(((Component)apartmentBuilding2).transform.position, ((Component)this).transform.position);
					if (num2 <= num)
					{
						num = num2;
						apartmentBuilding = apartmentBuilding2;
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		cachedBuilding = apartmentBuilding;
		return cachedBuilding;
	}

	[RPC_Server.FromMounted]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(2uL)]
	public void SERVER_RequestProperties(RPCMessage msg)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if ((Object)(object)player == (Object)null)
		{
			return;
		}
		ApartmentBuilding apartmentBuilding = ResolveBuilding();
		if ((Object)(object)apartmentBuilding == (Object)null)
		{
			return;
		}
		ApartmentTerminalData val = Pool.Get<ApartmentTerminalData>();
		try
		{
			val.plots = Pool.Get<List<ApartmentPlotEntry>>();
			int num = 0;
			int num2 = 0;
			foreach (ApartmentRoom room in apartmentBuilding.Rooms)
			{
				if (!((Object)(object)room == (Object)null))
				{
					bool flag = room.IsCurrentlyRented();
					ApartmentPlotEntry val2 = Pool.Get<ApartmentPlotEntry>();
					val2.roomNumber = room.RoomNumber;
					val2.roomId = room.net.ID;
					val2.occupied = flag;
					val2.size = (int)room.Size;
					val2.storageSlots = room.GetTotalStorageCapacity();
					val2.roomCount = GetRoomCount(room.Size);
					val2.estimatedValue = (int)GetEstimatedValue(room);
					val.plots.Add(val2);
					if (flag)
					{
						num2++;
					}
					else
					{
						num++;
					}
				}
			}
			val.totalAvailable = num;
			val.totalOccupied = num2;
			PopulateShops(val, apartmentBuilding);
			ClientRPC(RpcTarget.Player("CLIENT_ReceiveProperties", player), val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void PopulateShops(ApartmentTerminalData data, ApartmentBuilding building)
	{
		data.shops = Pool.Get<List<ApartmentPlotEntry>>();
		int num = 0;
		int num2 = 0;
		foreach (RentableShop item in ResolveBlockShops(building))
		{
			if (!((Object)(object)item == (Object)null) && !item.IsDestroyed)
			{
				bool flag = item.IsOn();
				VendingMachine serverVendingMachine = item.GetServerVendingMachine();
				ApartmentPlotEntry val = Pool.Get<ApartmentPlotEntry>();
				val.occupied = flag;
				val.storageSlots = 30;
				val.estimatedValue = (int)(((Object)(object)serverVendingMachine != (Object)null) ? BucketFromSum(SumStorageValue(serverVendingMachine.inventory)) : ValueBucket.None);
				val.roomNumber = item.ShopNumberId.ToString();
				data.shops.Add(val);
				if (flag)
				{
					num2++;
				}
				else
				{
					num++;
				}
			}
		}
		data.totalShopsAvailable = num;
		data.totalShopsOccupied = num2;
	}

	private List<RentableShop> ResolveBlockShops(ApartmentBuilding building)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (cachedBlockShops != null)
		{
			return cachedBlockShops;
		}
		cachedBlockShops = new List<RentableShop>();
		Enumerator<RentableShop> enumerator = RentableShop.AllServerShops.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				RentableShop current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && !current.IsDestroyed && !(Vector3.Distance(((Component)current).transform.position, ((Component)building).transform.position) > ApartmentBuilding.MaxRadiusSearch))
				{
					cachedBlockShops.Add(current);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return cachedBlockShops;
	}

	private int GetRoomCount(ApartmentSize size)
	{
		return size switch
		{
			ApartmentSize.Small => 1, 
			ApartmentSize.Medium => 2, 
			ApartmentSize.Large => 3, 
			_ => 0, 
		};
	}

	private ValueBucket GetEstimatedValue(ApartmentRoom room)
	{
		float num = 0f;
		foreach (BaseEntity item in room.Furniture)
		{
			if (item is StorageContainer { IsDestroyed: false } storageContainer)
			{
				num += SumStorageValue(storageContainer.inventory);
			}
		}
		num += SumSleepingOwnerValue(room);
		if ((Object)(object)room.UpkeepTerminal != (Object)null && !room.UpkeepTerminal.IsDestroyed)
		{
			num += SumStorageValue(room.UpkeepTerminal.inventory);
		}
		return BucketFromSum(num);
	}

	private float SumStorageValue(ItemContainer inventory)
	{
		if (inventory == null)
		{
			return 0f;
		}
		float num = 0f;
		foreach (Item item in inventory.itemList)
		{
			num += GetApartmentTaxValue(item);
		}
		return num;
	}

	private float SumSleepingOwnerValue(ApartmentRoom room)
	{
		float num = 0f;
		List<Item> list = null;
		foreach (ulong owner in room.Owners)
		{
			BasePlayer basePlayer = BasePlayer.FindSleeping(owner);
			if ((Object)(object)basePlayer == (Object)null || basePlayer.IsDestroyed || (Object)(object)basePlayer.inventory == (Object)null || !room.IsInsideRoom(basePlayer))
			{
				continue;
			}
			if (list == null)
			{
				list = Pool.Get<List<Item>>();
			}
			basePlayer.inventory.GetAllItems(list);
			foreach (Item item in list)
			{
				num += GetApartmentTaxValue(item);
			}
		}
		if (list != null)
		{
			Pool.Free<Item>(ref list, false);
		}
		return num;
	}

	private static float GetApartmentTaxValue(Item item)
	{
		if (item.info.ApartmentTaxPerStack > 0f)
		{
			return item.info.ApartmentTaxPerStack * ((float)item.amount / (float)item.MaxStackable());
		}
		return 0f;
	}

	private ValueBucket BucketFromSum(float sum)
	{
		if (sum <= 0f)
		{
			return ValueBucket.None;
		}
		if (sum >= HighValueThreshold)
		{
			return ValueBucket.High;
		}
		if (sum >= MediumValueThreshold)
		{
			return ValueBucket.Medium;
		}
		return ValueBucket.Low;
	}
}

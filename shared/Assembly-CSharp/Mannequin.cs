using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class Mannequin : StorageContainer
{
	public static class MannequinFlags
	{
		public const Flags IsEquipping = Flags.Reserved1;
	}

	[Header("Mannequin Settings")]
	public GameObjectRef EquipSound;

	public GameObjectRef ChangePoseSound;

	public GameObject SupportRoot;

	public Transform MannequinSpawnPoint;

	public BaseCollision HitBoxCollision;

	public PhysicsMaterial OverrideHitBoxMaterial;

	[Range(0f, 1f)]
	public float LodRange0 = 0.3f;

	[Range(0f, 1f)]
	public float LodRange1 = 0.15f;

	[Range(0f, 1f)]
	public float LodRange2 = 0.08f;

	[Range(0f, 1f)]
	public float LodRange3 = 0.02f;

	private const int BACKPACK_SLOT_INDEX = 7;

	protected static string headPartPath = "assets/prefabs/clothes/skin/mannequin/head.male.mannequin.prefab";

	protected static string torsoPartPath = "assets/prefabs/clothes/skin/mannequin/torso.male.mannequin.prefab";

	protected static string handsPartPath = "assets/prefabs/clothes/skin/mannequin/hands.male.mannequin.prefab";

	protected static string legsPartPath = "assets/prefabs/clothes/skin/mannequin/legs.male.mannequin.prefab";

	public static HumanBodyBones[] ValidBoneArray;

	public MannequinPose[] AvailablePoses;

	private static Item[] clothingBuffer;

	private static Item[] lockerBuffer;

	private int __sync_PoseIndex;

	[Sync(Autosave = true)]
	public int PoseIndex
	{
		[CompilerGenerated]
		get
		{
			return __sync_PoseIndex;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_PoseIndex, value))
			{
				__sync_PoseIndex = value;
				byte nameID = __GetWeaverID("PoseIndex");
				QueueSyncVar(nameID);
			}
		}
	}

	public TimeSince LastPoseChange
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Mannequin.OnRpcMessage"))
		{
			if (rpc == 1116452643 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_ChangePose"));
				}
				using (TimeWarning.New("Server_ChangePose"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1116452643u, "Server_ChangePose", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1116452643u, "Server_ChangePose", this, player, 3f))
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
							Server_ChangePose(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in Server_ChangePose");
					}
				}
				return true;
			}
			if (rpc == 1422897100 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_RequestSwap"));
				}
				using (TimeWarning.New("Server_RequestSwap"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1422897100u, "Server_RequestSwap", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1422897100u, "Server_RequestSwap", this, player, 3f))
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
							Server_RequestSwap(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in Server_RequestSwap");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsEquipping()
	{
		return HasFlag(Flags.Reserved1);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
	}

	public override void Save(SaveInfo info)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.mannequin = Pool.Get<Mannequin>();
		info.msg.mannequin.clothingItems = Pool.Get<List<ClothingItem>>();
		foreach (Item item in inventory.itemList)
		{
			ClothingItem val = Pool.Get<ClothingItem>();
			val.itemId = item.info.itemid;
			val.skin = item.skin;
			val.uid = item.uid;
			val.reserved_int_0 = item.instanceData?.dataInt ?? 0;
			info.msg.mannequin.clothingItems.Add(val);
		}
	}

	protected override void OnInventoryDirty()
	{
		base.OnInventoryDirty();
		SendNetworkUpdate();
	}

	private bool IsBackpackSlot(int slot)
	{
		return (slot - 7) % 14 == 0;
	}

	public override bool ItemFilter(BasePlayer player, Item item, int targetSlot)
	{
		if (!base.ItemFilter(player, item, targetSlot))
		{
			return false;
		}
		bool flag = item.IsBackpack();
		bool flag2 = IsBackpackSlot(targetSlot);
		if (flag != flag2)
		{
			return false;
		}
		if ((Object)(object)item.info.ItemModWearable != (Object)null && item.info.ItemModWearable.blockFromMannequin)
		{
			return false;
		}
		return CanAcceptItem(player, item, targetSlot);
	}

	private bool CanAcceptItem(BasePlayer player, Item newItem, int slot)
	{
		ItemModWearable itemModWearable = default;
		if (!((Component)newItem.info).TryGetComponent<ItemModWearable>(ref itemModWearable))
		{
			return false;
		}
		ItemModWearable wearable = default;
		for (int i = 0; i < inventory.capacity; i++)
		{
			Item slot2 = inventory.GetSlot(i);
			if (slot2 != null && ((Component)slot2.info).TryGetComponent<ItemModWearable>(ref wearable) && !itemModWearable.CanExistWith(wearable) && slot != i)
			{
				return false;
			}
		}
		return true;
	}

	public void ClearEquipping()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b: false);
	}

	public override void OnInventoryFirstCreated(ItemContainer container)
	{
		base.OnInventoryFirstCreated(container);
		container.flags = ItemContainer.Flag.Clothing;
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void Server_ChangePose(RPCMessage msg)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)msg.player == (Object)null) && msg.player.CanBuild(cached: true) && Interface.CallHook("CanMannequinChangePose", this, msg.player) == null)
		{
			int num = PoseIndex + 1;
			if (num >= AvailablePoses.Length)
			{
				num = 0;
			}
			PoseIndex = num;
			if (ChangePoseSound.isValid)
			{
				Effect.server.Run(ChangePoseSound.resourcePath, ((Component)this).transform.position);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	private void Server_RequestSwap(RPCMessage msg)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (IsEquipping())
		{
			return;
		}
		BasePlayer player = msg.player;
		if (!CanBeLooted(player) || player.IsDead() || Interface.CallHook("CanMannequinSwap", this, player) != null)
		{
			return;
		}
		BaseLock baseLock = GetLock();
		if ((Object)(object)baseLock != (Object)null && !baseLock.OnTryToOpen(player))
		{
			player.ShowToast(GameTip.Styles.Error, PlayerInventoryErrors.ContainerLocked, false);
		}
		else if (SwapPlayerInventoryWithContainer(msg.player, inventory, GetDropPosition(), GetDropVelocity(), FilterItems))
		{
			if (EquipSound != null)
			{
				Effect.server.Run(EquipSound.resourcePath, player, StringPool.Get("spine3"), Vector3.zero, Vector3.zero);
			}
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved1, b: true);
			}
			Invoke(ClearEquipping, 1.5f);
		}
	}

	private bool FilterItems(Item item)
	{
		if ((Object)(object)item.info.ItemModWearable != (Object)null && item.info.ItemModWearable.blockFromMannequin)
		{
			return false;
		}
		return true;
	}

	public static bool SwapPlayerInventoryWithContainer(BasePlayer player, ItemContainer inventory, Vector3 dropPosition, Vector3 dropVelocity, Func<Item, bool> filterItems = null)
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		for (int i = 0; i < clothingBuffer.Length; i++)
		{
			Item slot = player.inventory.containerWear.GetSlot(i);
			if (slot != null && (filterItems == null || filterItems(slot)))
			{
				slot.RemoveFromContainer();
				clothingBuffer[i] = slot;
			}
		}
		for (int j = 0; j < lockerBuffer.Length; j++)
		{
			Item slot2 = inventory.GetSlot(j);
			if (slot2 != null && (filterItems == null || filterItems(slot2)))
			{
				slot2.RemoveFromContainer();
				lockerBuffer[j] = slot2;
			}
		}
		for (int k = 0; k < clothingBuffer.Length; k++)
		{
			Item item = lockerBuffer[k];
			Item item2 = clothingBuffer[k];
			if (item != null)
			{
				result = true;
				if (item.info.category != ItemCategory.Attire || !item.MoveToContainer(player.inventory.containerWear, k))
				{
					item.Drop(dropPosition, dropVelocity);
				}
			}
			if (item2 != null)
			{
				result = true;
				if (!item2.MoveToContainer(inventory, k) && !item2.MoveToContainer(player.inventory.containerWear, k) && !item2.MoveToContainer(player.inventory.containerMain))
				{
					item2.Drop(dropPosition, dropVelocity);
				}
			}
			clothingBuffer[k] = null;
			lockerBuffer[k] = null;
		}
		return result;
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: PoseIndex for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_PoseIndex);
			return true;
		}
		return base.WriteSyncVar(id, writer);
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		if (id == 0)
		{
			try
			{
				_ = __sync_PoseIndex;
				int _sync_PoseIndex = reader.Int32();
				__sync_PoseIndex = _sync_PoseIndex;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		}
		return base.OnSyncVar(id, reader, fromAutoSave);
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (propertyName == "PoseIndex")
		{
			return 0;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		WriteAutoSaveSyncVars(netWrite);
		(byte[] Buffer, int Length) buffer = netWrite.GetBuffer();
		byte[] item = buffer.Buffer;
		int item2 = buffer.Length;
		byte[] array = _autosaveBuffer;
		if (array == null || array.Length < item2)
		{
			byte[] array2 = BaseEntity._autosaveBufferPool.Rent(item2);
			while (array == null || array.Length < item2)
			{
				byte[] array3 = Interlocked.CompareExchange(ref _autosaveBuffer, array2, array);
				if (array3 == array)
				{
					if (array3 != null)
					{
						BaseEntity._autosaveBufferPool.Return(array3);
					}
					array = array2;
					break;
				}
				array = array3;
			}
			if (array != array2)
			{
				BaseEntity._autosaveBufferPool.Return(array2);
			}
		}
		Buffer.BlockCopy(item, 0, array, 0, item2);
		save.msg.baseEntity.syncVars = array;
		Pool.Free<NetWrite>(ref netWrite);
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		if (load.msg.baseEntity != null && load.msg.baseEntity.syncVars != null)
		{
			NetRead netRead = Pool.Get<NetRead>();
			netRead.Init(load.msg.baseEntity.syncVars.AsSpan());
			ReadAutoSaveSyncVars(netRead);
			Pool.Free<NetRead>(ref netRead);
		}
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_PoseIndex = 0;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 0)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}

	static Mannequin()
	{
		HumanBodyBones[] array = new HumanBodyBones[49];
		RuntimeHelpers.InitializeArray(array, __ldtoken(_003CPrivateImplementationDetails_003E.B75E8A426ABC97E5121195CEFBF6EB3596FF3326EF57E9E9A53B8886FD803B73));
		ValidBoneArray = array;
		clothingBuffer = new Item[8];
		lockerBuffer = new Item[8];
	}
}

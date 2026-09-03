using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class Signage : IOEntity, ILOD, ISignage, IUGCBrowserEntity, IEaselPaintable
{
	private const float TextureRequestTimeout = 15f;

	public GameObjectRef changeTextDialog;

	public MeshPaintableSource[] paintableSources;

	public int overrideMaxImageWidth;

	public int overrideMaxImageHeight;

	[NonSerialized]
	public uint[] textureIDs;

	[ReplicatedVar(Help = "If true admins can always update signs, regardless of lock, build auth, etc")]
	public static bool AdminsCanAlwaysUpdateSigns = true;

	[Header("Signage")]
	public ItemDefinition RequiredHeldEntity;

	private EaselDeployable _cachedParentEasel;

	public bool showIODisplayName;

	public const Flags BlockUnlocking = Flags.Reserved3;

	private List<ulong> editHistory = new List<ulong>();

	private NetworkableId __sync_EaselId;

	public Vector2i TextureSize
	{
		get
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (paintableSources == null || paintableSources.Length == 0)
			{
				return Vector2i.zero;
			}
			MeshPaintableSource meshPaintableSource = paintableSources[0];
			return new Vector2i(meshPaintableSource.texWidth, meshPaintableSource.texHeight);
		}
	}

	public int TextureCount
	{
		get
		{
			MeshPaintableSource[] array = paintableSources;
			if (array == null)
			{
				return 0;
			}
			return array.Length;
		}
	}

	[Sync(Autosave = true)]
	public NetworkableId EaselId
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return __sync_EaselId;
		}
		[CompilerGenerated]
		set
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (!IsSyncVarEqual<NetworkableId>(__sync_EaselId, value))
			{
				__sync_EaselId = value;
				byte nameID = __GetWeaverID("EaselId");
				QueueSyncVar(nameID);
			}
		}
	}

	public EaselDeployable parentEasel
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			NetworkableId easelId = EaselId;
			if (!((NetworkableId)(ref easelId)).IsValid)
			{
				return null;
			}
			if ((Object)(object)_cachedParentEasel == (Object)null)
			{
				_cachedParentEasel = new EntityRef<EaselDeployable>(EaselId).Get(base.isServer);
			}
			return _cachedParentEasel;
		}
	}

	public GameObject GameObject => ((Component)this).gameObject;

	public NetworkableId NetworkID
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return net.ID;
		}
	}

	public FileStorage.Type FileType => FileStorage.Type.png;

	public UGCType ContentType => UGCType.ImagePng;

	public List<ulong> EditingHistory => editHistory;

	public BaseNetworkable UgcEntity => this;

	public string ContentString => string.Empty;

	public uint[] GetContentCRCs => GetTextureCRCs();

	public override bool ShouldTransferAssociatedFiles => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Signage.OnRpcMessage"))
		{
			if (rpc == 1455609404 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - LockSign"));
				}
				using (TimeWarning.New("LockSign"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1455609404u, "LockSign", this, player, 3f))
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
							LockSign(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in LockSign");
					}
				}
				return true;
			}
			if (rpc == 4149904254u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - UnLockSign"));
				}
				using (TimeWarning.New("UnLockSign"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4149904254u, "UnLockSign", this, player, 3f))
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
							UnLockSign(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in UnLockSign");
					}
				}
				return true;
			}
			if (rpc == 1255380462 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - UpdateSign"));
				}
				using (TimeWarning.New("UpdateSign"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1255380462u, "UpdateSign", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1255380462u, "UpdateSign", this, player, 5f))
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
							UpdateSign(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in UpdateSign");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.PreProcess(preProcess, rootObj, name, serverside, clientside, bundling);
		if (clientside && paintableSources != null && paintableSources.Length > 1)
		{
			MeshPaintableSource meshPaintableSource = paintableSources[0];
			for (int i = 1; i < paintableSources.Length; i++)
			{
				MeshPaintableSource obj = paintableSources[i];
				obj.texWidth = meshPaintableSource.texWidth;
				obj.texHeight = meshPaintableSource.texHeight;
			}
		}
	}

	[RPC_Server.MaxDistance(5f)]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	public void UpdateSign(RPCMessage msg)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)msg.player == (Object)null || !CanUpdateSign(msg.player))
		{
			return;
		}
		int num = msg.read.Int32();
		if (num < 0 || num >= paintableSources.Length)
		{
			return;
		}
		byte[] array = msg.read.BytesWithSize();
		if (msg.read.Unread > 0 && msg.read.Bit() && !msg.player.IsAdmin)
		{
			Debug.LogWarning((object)$"{msg.player} tried to upload a sign from a file but they aren't admin, ignoring");
			return;
		}
		EnsureInitialized();
		if (array == null)
		{
			if (textureIDs[num] != 0)
			{
				FileStorage.server.RemoveExact(textureIDs[num], FileStorage.Type.png, net.ID, (uint)num);
			}
			textureIDs[num] = 0u;
		}
		else
		{
			int maxWidth = ((overrideMaxImageWidth > 0) ? overrideMaxImageWidth : 1024);
			int maxHeight = ((overrideMaxImageHeight > 0) ? overrideMaxImageHeight : 1024);
			if (!ImageProcessing.IsValidPNG(array, maxWidth, maxHeight))
			{
				return;
			}
			if (textureIDs[num] != 0)
			{
				FileStorage.server.RemoveExact(textureIDs[num], FileStorage.Type.png, net.ID, (uint)num);
			}
			textureIDs[num] = FileStorage.server.Store(array, FileStorage.Type.png, net.ID, (uint)num);
		}
		LogEdit(msg.player);
		SendNetworkUpdate();
		Interface.CallHook("OnSignUpdated", this, msg.player, num);
	}

	public void EnsureInitialized()
	{
		int num = Mathf.Max(paintableSources.Length, 1);
		if (textureIDs == null || textureIDs.Length != num)
		{
			Array.Resize(ref textureIDs, num);
		}
	}

	[Conditional("SIGN_DEBUG")]
	private static void SignDebugLog(string str)
	{
		Debug.Log((object)str);
	}

	public override bool ShowDisplayName()
	{
		return showIODisplayName;
	}

	public virtual bool CanUpdateSign(BasePlayer player)
	{
		object obj = Interface.CallHook("CanUpdateSign", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (AdminsCanAlwaysUpdateSigns && (player.IsAdmin || player.IsDeveloper))
		{
			return true;
		}
		if (!player.CanBuild())
		{
			return false;
		}
		if (IsLocked())
		{
			return (ulong)player.userID == base.OwnerID;
		}
		if (!HeldEntityCheck(player))
		{
			return false;
		}
		if (GetParentEntity() is RentableShop rentableShop)
		{
			if (IsOn())
			{
				return rentableShop.ShopOwnerId == player.OwnerID;
			}
			return false;
		}
		return true;
	}

	public bool CanUnlockSign(BasePlayer player)
	{
		if (!IsLocked())
		{
			return false;
		}
		if (HasFlag(Flags.Reserved3))
		{
			return false;
		}
		if (!HeldEntityCheck(player))
		{
			return false;
		}
		return CanUpdateSign(player);
	}

	public bool CanLockSign(BasePlayer player)
	{
		if (IsLocked())
		{
			return false;
		}
		if (!HeldEntityCheck(player))
		{
			return false;
		}
		return CanUpdateSign(player);
	}

	public override void Load(LoadInfo info)
	{
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		EnsureInitialized();
		bool flag = false;
		if (info.msg.sign != null)
		{
			uint num = textureIDs[0];
			if (info.msg.sign.imageIds != null && info.msg.sign.imageIds.Count > 0)
			{
				int num2 = Mathf.Min(info.msg.sign.imageIds.Count, textureIDs.Length);
				for (int i = 0; i < num2; i++)
				{
					uint num3 = info.msg.sign.imageIds[i];
					bool flag2 = num3 != textureIDs[i];
					flag |= flag2;
					textureIDs[i] = num3;
				}
			}
			else
			{
				flag = num != info.msg.sign.imageid;
				textureIDs[0] = info.msg.sign.imageid;
			}
		}
		if (!base.isServer)
		{
			return;
		}
		bool flag3 = false;
		for (int j = 0; j < paintableSources.Length; j++)
		{
			uint num4 = textureIDs[j];
			if (num4 != 0)
			{
				byte[] array = FileStorage.server.Get(num4, FileStorage.Type.png, net.ID, (uint)j);
				if (array == null)
				{
					Log($"Frame {j} (id={num4}) doesn't exist, clearing");
					textureIDs[j] = 0u;
				}
				flag3 = flag3 || array != null;
			}
		}
		if (!flag3)
		{
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Locked, b: false);
		}
		if (info.msg.sign == null)
		{
			return;
		}
		if (info.msg.sign.editHistory != null)
		{
			if (editHistory == null)
			{
				editHistory = Pool.Get<List<ulong>>();
			}
			editHistory.Clear();
			{
				foreach (ulong item in info.msg.sign.editHistory)
				{
					editHistory.Add(item);
				}
				return;
			}
		}
		if (editHistory != null)
		{
			Pool.FreeUnmanaged<ulong>(ref editHistory);
		}
	}

	private bool HeldEntityCheck(BasePlayer player)
	{
		if ((Object)(object)RequiredHeldEntity != (Object)null && (!Object.op_Implicit((Object)(object)player.GetHeldEntity()) || (Object)(object)player.GetHeldEntity().GetItem().info != (Object)(object)RequiredHeldEntity))
		{
			return false;
		}
		return true;
	}

	public uint[] GetTextureCRCs()
	{
		return textureIDs;
	}

	public override void PostInitShared()
	{
		base.PostInitShared();
		if (base.isServer)
		{
			_ = GetParentEntity() is RentableShop;
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void LockSign(RPCMessage msg)
	{
		if (msg.player.CanInteract() && CanUpdateSign(msg.player))
		{
			SetFlagLocal(Flags.Locked, b: true);
			SendNetworkUpdate();
			base.OwnerID = msg.player.userID;
			Interface.CallHook("OnSignLocked", this, msg.player);
		}
	}

	public void LockSign(BasePlayer forPlayer)
	{
		SetFlagLocal(Flags.Locked, b: true);
		base.OwnerID = (((Object)(object)forPlayer != (Object)null) ? forPlayer.userID : ((EncryptedValue<ulong>)0uL));
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void UnLockSign(RPCMessage msg)
	{
		if (msg.player.CanInteract() && CanUnlockSign(msg.player))
		{
			SetFlagLocal(Flags.Locked, b: false);
			SendNetworkUpdate();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		EnsureInitialized();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		List<uint> list = Pool.Get<List<uint>>();
		uint[] array = textureIDs;
		foreach (uint item in array)
		{
			list.Add(item);
		}
		info.msg.sign = Pool.Get<Sign>();
		info.msg.sign.imageid = 0u;
		info.msg.sign.imageIds = list;
		if (editHistory == null || editHistory.Count <= 0 || !info.forDisk)
		{
			return;
		}
		info.msg.sign.editHistory = Pool.Get<List<ulong>>();
		foreach (ulong item2 in editHistory)
		{
			info.msg.sign.editHistory.Add(item2);
		}
	}

	internal override void DoServerDestroy()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		base.DoServerDestroy();
		RemoveFromEasel();
		if (net != null)
		{
			FileStorage.server.RemoveAllByEntity(net.ID);
		}
		if (textureIDs != null)
		{
			Array.Clear(textureIDs, 0, textureIDs.Length);
		}
	}

	public override void OnPickedUpPreItemMove(Item createdItem, BasePlayer player)
	{
		base.OnPickedUpPreItemMove(createdItem, player);
		SaveSignageToItem(createdItem);
	}

	public void SaveSignageToItem(Item createdItem)
	{
		bool flag = false;
		uint[] array = textureIDs;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != 0)
			{
				flag = true;
				break;
			}
		}
		ItemModSign itemModSign = default(ItemModSign);
		if (flag && ((Component)createdItem.info).TryGetComponent<ItemModSign>(ref itemModSign))
		{
			itemModSign.OnSignPickedUp(this, this, createdItem);
		}
	}

	public void RemoveFromEasel()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)parentEasel != (Object)null)
		{
			parentEasel.RemovePainting(this);
		}
		EaselId = default(NetworkableId);
	}

	public void AddToEasel(BaseEntity parent)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)parent != (Object)null && parent is EaselDeployable easelDeployable)
		{
			EaselId = easelDeployable.net.ID;
			easelDeployable.SetPainting(this);
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		AddToEasel(parent);
		ItemModSign itemModSign = default(ItemModSign);
		if (((Component)fromItem.info).TryGetComponent<ItemModSign>(ref itemModSign))
		{
			SignContent associatedEntity = ItemModAssociatedEntity<SignContent>.GetAssociatedEntity(fromItem);
			if ((Object)(object)associatedEntity != (Object)null)
			{
				associatedEntity.CopyInfoToSign(this, this);
			}
		}
	}

	public override bool ShouldNetworkOwnerInfo()
	{
		return true;
	}

	public void SetTextureCRCs(uint[] crcs)
	{
		textureIDs = new uint[crcs.Length];
		crcs.CopyTo(textureIDs, 0);
		SendNetworkUpdate();
	}

	public void LogEdit(BasePlayer byPlayer)
	{
		if (!editHistory.Contains(byPlayer.userID))
		{
			editHistory.Insert(0, byPlayer.userID);
			int num = 0;
			while (editHistory.Count > 5 && num < 10)
			{
				editHistory.RemoveAt(5);
				num++;
			}
		}
	}

	public void ClearContent()
	{
		SetTextureCRCs(Array.Empty<uint>());
	}

	public override string Admin_Who()
	{
		if (editHistory == null || editHistory.Count == 0)
		{
			return base.Admin_Who();
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(base.Admin_Who());
		for (int i = 0; i < editHistory.Count; i++)
		{
			stringBuilder.AppendLine($"Edit {i}: {editHistory[i]}");
		}
		return stringBuilder.ToString();
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	public override string Categorize()
	{
		return "sign";
	}

	protected unsafe override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: EaselId for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite<NetworkableId>(writer, __sync_EaselId);
			return true;
		}
		return base.WriteSyncVar(id, writer);
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (id == 0)
		{
			try
			{
				_ = __sync_EaselId;
				NetworkableId _sync_EaselId = reader.EntityID();
				__sync_EaselId = _sync_EaselId;
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
		if (propertyName == "EaselId")
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
		var (src, num) = netWrite.GetBuffer();
		if (_autosaveBuffer == null)
		{
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		if (_autosaveBuffer.Length < num)
		{
			BaseEntity._autosaveBufferPool.Return(_autosaveBuffer);
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		Buffer.BlockCopy(src, 0, _autosaveBuffer, 0, num);
		save.msg.baseEntity.syncVars = _autosaveBuffer;
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
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		base.ResetSyncVars();
		__sync_EaselId = default(NetworkableId);
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 0)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}
}

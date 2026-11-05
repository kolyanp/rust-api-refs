using System;
using System.Collections.Generic;
using System.Diagnostics;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class CarvablePumpkin : BaseOven, ILOD, ISignage, IUGCBrowserEntity, IServerFileReceiver
{
	private const float TextureRequestTimeout = 15f;

	public GameObjectRef changeTextDialog;

	public MeshPaintableSource[] paintableSources;

	[NonSerialized]
	public uint[] textureIDs;

	private List<ulong> editHistory = new List<ulong>();

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

	public int TextureCount => paintableSources?.Length ?? 0;

	public FileStorage.Type FileType => FileStorage.Type.png;

	public NetworkableId NetworkID
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return net.ID;
		}
	}

	public UGCType ContentType => UGCType.ImagePng;

	public List<ulong> EditingHistory => editHistory;

	public uint[] GetContentCRCs => textureIDs;

	public override bool ShouldTransferAssociatedFiles => true;

	public BaseNetworkable UgcEntity => this;

	public string ContentString => string.Empty;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CarvablePumpkin.OnRpcMessage"))
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
		if (paintableSources != null && paintableSources.Length > 1)
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

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.MaxDistance(5f)]
	[RPC_Server]
	public void UpdateSign(RPCMessage msg)
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
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
			FileStorage.server.Remove(net.ID, (uint)num);
			textureIDs[num] = 0u;
		}
		else
		{
			if (!ImageProcessing.IsValidPNG(array, 1024, 1024))
			{
				return;
			}
			textureIDs[num] = FileStorage.server.Store(net.ID, (uint)num, FileStorage.Type.png, array);
			Facepunch.Rust.Analytics.Azure.OnUGCCreated(msg.player, this, "pumpkin", array.Length, num);
			UGCModeration.Send(msg.player, this, "pumpkin", textureIDs[num], FileStorage.Type.png, num);
		}
		LogEdit(msg.player);
		SendNetworkUpdate();
		Interface.CallHook("OnSignUpdated", this, msg.player);
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

	public uint[] GetTextureCRCs()
	{
		return textureIDs;
	}

	public virtual bool CanUpdateSign(BasePlayer player)
	{
		object obj = Interface.CallHook("CanUpdateSign", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (player.IsAdmin || player.IsDeveloper)
		{
			return true;
		}
		if (!player.CanBuild())
		{
			return false;
		}
		if (IsLocked())
		{
			return (ulong)player.userID == OwnerID;
		}
		return true;
	}

	public bool CanUnlockSign(BasePlayer player)
	{
		if (!IsLocked())
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
		return CanUpdateSign(player);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		EnsureInitialized();
		bool flag = false;
		if (info.msg.sign != null && info.msg.sign.imageIds != null)
		{
			int num = Mathf.Min(info.msg.sign.imageIds.Count, textureIDs.Length);
			for (int i = 0; i < num; i++)
			{
				uint num2 = info.msg.sign.imageIds[i];
				bool flag2 = num2 != textureIDs[i];
				flag |= flag2;
				textureIDs[i] = num2;
			}
		}
		if (!isServer)
		{
			return;
		}
		LoadTextureCrcs();
		if (Array.TrueForAll(textureIDs, (uint id) => id == 0))
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

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void LockSign(RPCMessage msg)
	{
		if (msg.player.CanInteract() && CanUpdateSign(msg.player))
		{
			SetFlagLocal(Flags.Locked, b: true);
			SendNetworkUpdate();
			OwnerID = msg.player.userID;
		}
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
		info.msg.sign = Pool.Get<Sign>();
		if (!info.forDisk)
		{
			info.msg.sign.imageIds = Pool.Get<List<uint>>();
			info.msg.sign.imageIds.AddRange(textureIDs);
		}
		if (editHistory.Count <= 0)
		{
			return;
		}
		info.msg.sign.editHistory = Pool.Get<List<ulong>>();
		foreach (ulong item in editHistory)
		{
			info.msg.sign.editHistory.Add(item);
		}
	}

	internal override void DoServerDestroy()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		base.DoServerDestroy();
		if (net != null)
		{
			FileStorage.server.RemoveAll(net.ID);
		}
		if (textureIDs != null)
		{
			Array.Clear(textureIDs, 0, textureIDs.Length);
		}
	}

	public override void OnPickedUpPreItemMove(Item createdItem, BasePlayer player)
	{
		base.OnPickedUpPreItemMove(createdItem, player);
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
		ItemModSign itemModSign = default;
		if (flag && ((Component)createdItem.info).TryGetComponent<ItemModSign>(ref itemModSign))
		{
			itemModSign.OnSignPickedUp(this, this, createdItem);
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		ItemModSign itemModSign = default;
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

	public void RefreshFileCrcs()
	{
		LoadTextureCrcs();
		SendNetworkUpdate();
	}

	private void LoadTextureCrcs()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		EnsureInitialized();
		Dictionary<ulong, uint> dictionary = Pool.Get<Dictionary<ulong, uint>>();
		FileStorage.server.GetCrcs(net.ID, dictionary);
		for (int i = 0; i < textureIDs.Length; i++)
		{
			dictionary.TryGetValue((ulong)i, out textureIDs[i]);
		}
		Pool.FreeUnmanaged<ulong, uint>(ref dictionary);
	}

	private void LogEdit(BasePlayer byPlayer)
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
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		FileStorage.server.RemoveAll(net.ID);
		RefreshFileCrcs();
	}

	public override string Categorize()
	{
		return "sign";
	}
}

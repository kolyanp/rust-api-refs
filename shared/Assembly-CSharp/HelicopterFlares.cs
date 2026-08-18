using System;
using System.Runtime.CompilerServices;
using ConVar;
using Network;
using UnityEngine;

public class HelicopterFlares : StorageContainer
{
	[Header("Helicopter Flares")]
	[SerializeField]
	private ItemDefinition flareItemDef;

	[SerializeField]
	private float timeBetweenFlares = 30f;

	[SerializeField]
	private float flareLaunchVel = 10f;

	[SerializeField]
	private GameObjectRef flareFireFX;

	[SerializeField]
	private GameObjectRef serverFlarePrefab;

	[SerializeField]
	private Transform leftFlareLaunchPos;

	[SerializeField]
	private Transform rightFlareLaunchPos;

	[HideInInspector]
	public ICanFireHelicopterFlares owner;

	private TimeSince timeSinceFlareFired;

	private bool __sync_HasFlares;

	[Sync]
	public bool HasFlares
	{
		[CompilerGenerated]
		get
		{
			return __sync_HasFlares;
		}
		[CompilerGenerated]
		private set
		{
			if (!IsSyncVarEqual(__sync_HasFlares, value))
			{
				__sync_HasFlares = value;
				byte nameID = __GetWeaverID("HasFlares");
				QueueSyncVar(nameID);
			}
		}
	}

	public bool CanFireFlare
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			if (TimeSince.op_Implicit(timeSinceFlareFired) >= timeBetweenFlares)
			{
				return HasFlareAmmo();
			}
			return false;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("HelicopterFlares.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool HasFlareAmmo()
	{
		if (base.isServer)
		{
			HasFlares = base.inventory.HasAny(flareItemDef);
			return HasFlares;
		}
		return false;
	}

	private void ResetFiringTimes()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		timeSinceFlareFired = TimeSince.op_Implicit(9999f);
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (owner.flareEntity.IsOn())
		{
			return false;
		}
		return base.CanBeLooted(player);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		HasFlares = HasFlareAmmo();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		HasFlares = HasFlareAmmo();
	}

	public bool TryFireFlare()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (!CanFireFlare)
		{
			return false;
		}
		if (owner == null)
		{
			return false;
		}
		if (!base.inventory.TryTakeOne(flareItemDef.itemid, out var item))
		{
			return false;
		}
		item.Remove();
		timeSinceFlareFired = TimeSince.op_Implicit(0f);
		LaunchFlare();
		ClientRPC(RpcTarget.NetworkGroup("RPCFlareFired"), HasFlareAmmo());
		return true;
	}

	public void LaunchFlare()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		Effect.server.Run(flareFireFX.resourcePath, owner.flareEntity, StringPool.Get("FlareLaunchPos"), Vector3.zero, Vector3.zero);
		GameManager.server.CreatePrefab(serverFlarePrefab.resourcePath, leftFlareLaunchPos.position, Quaternion.identity).GetComponent<HeliPilotFlare>().Init(-((Component)owner.flareEntity).transform.right * flareLaunchVel);
		GameManager.server.CreatePrefab(serverFlarePrefab.resourcePath, rightFlareLaunchPos.position, Quaternion.identity).GetComponent<HeliPilotFlare>().Init(((Component)owner.flareEntity).transform.right * flareLaunchVel);
	}

	public void RefillFlares()
	{
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition("flare");
		int amount = itemDefinition.stackable * 2;
		base.inventory.AddItem(itemDefinition, amount, 0uL, ItemContainer.LimitStack.All);
	}

	protected unsafe override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: HasFlares for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_HasFlares);
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
				_ = __sync_HasFlares;
				bool _sync_HasFlares = reader.Bool();
				__sync_HasFlares = _sync_HasFlares;
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
		if (propertyName == "HasFlares")
		{
			return 0;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_HasFlares = false;
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

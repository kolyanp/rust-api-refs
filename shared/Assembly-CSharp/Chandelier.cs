using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class Chandelier : IOEntity
{
	public float DefaultLength = 0.25f;

	public float MaxLength = 3f;

	public float MinLength = 0.25f;

	public float MoveIncrement = 0.25f;

	public Transform ChandelierBodyRoot;

	public Transform ChandelierCableRoot;

	public MeshRenderer CableRenderer;

	public MeshRenderer PedalCableRenderer;

	public GameObjectRef adjustHeightEffect;

	[SerializeField]
	private float pedalAnglePerIncrement;

	public Transform PedalTransform;

	public BoxCollider ReferenceBoxTrace;

	private float lastLength = -1f;

	private const float baseBoundsSizeY = 0.35f;

	public static Phrase BlockedByObjectPhrase = new Phrase("chandelier.blocked", "Cannot extend through solid objects");

	public static Phrase MaxLengthPhrase = new Phrase("chandelier.maxlengthreached", "Maximum length reached");

	private float __sync_ChandelierLength;

	[Sync(Autosave = true)]
	public float ChandelierLength
	{
		[CompilerGenerated]
		get
		{
			return __sync_ChandelierLength;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_ChandelierLength, value))
			{
				__sync_ChandelierLength = value;
				byte nameID = __GetWeaverID("ChandelierLength");
				QueueSyncVar(nameID);
			}
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Chandelier.OnRpcMessage"))
		{
			if (rpc == 3461669953u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_AdjustChandelierLength"));
				}
				using (TimeWarning.New("SERVER_AdjustChandelierLength"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3461669953u, "SERVER_AdjustChandelierLength", this, player, 5uL))
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
							SERVER_AdjustChandelierLength(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SERVER_AdjustChandelierLength");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override int ConsumptionAmount()
	{
		return 4;
	}

	public override float AntiHackPadding()
	{
		return ChandelierLength + 0.25f;
	}

	private void UpdateChandelierLength(float deltaTime)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		float chandelierLength = ChandelierLength;
		if (isServer)
		{
			if ((Object)(object)ChandelierBodyRoot != (Object)null)
			{
				float num = Mathx.Lerp(ChandelierBodyRoot.localPosition.y, 0f - ChandelierLength, 15f, deltaTime);
				ChandelierBodyRoot.localPosition = Vector3Ex.WithY(ChandelierBodyRoot.localPosition, num);
			}
		}
		else
		{
			SetBoundsYSize(0.35f + chandelierLength * 0.5f);
		}
	}

	private void ResetLength()
	{
		lastLength = -1f;
	}

	private void SetBoundsYSize(float y)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		Bounds val = bounds;
		val.center = new Vector3(bounds.center.x, 0f - y, bounds.center.z);
		val.extents = new Vector3(bounds.extents.x, y, bounds.extents.z);
		bounds = val;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		UpdateChandelierLength(1000f);
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		SetChandelierLength(0f);
	}

	private void StartServerTick()
	{
		CancelInvoke(ServerTick);
		InvokeRepeating(ServerTick, 0f, 0f);
		Invoke(() =>
		{
			CancelInvoke(ServerTick);
		}, 2.5f);
	}

	public void ServerTick()
	{
		if (!IsDestroyed)
		{
			UpdateChandelierLength(Time.deltaTime);
		}
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	private void SERVER_AdjustChandelierLength(RPCMessage msg)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if ((Object)(object)player == (Object)null || !player.CanBuild(cached: true) || player.Distance(ChandelierBodyRoot.position) > 3f)
		{
			return;
		}
		bool flag = msg.read.Bool();
		float num = MoveIncrement;
		if (flag)
		{
			num = 0f - num;
		}
		if (!flag && ChandelierLength >= MaxLength)
		{
			player.ShowToast(GameTip.Styles.Error, BlockedByObjectPhrase, false);
		}
		else
		{
			if (flag && ChandelierLength == MinLength)
			{
				return;
			}
			if ((Object)(object)ReferenceBoxTrace != (Object)null)
			{
				PooledList<RaycastHit> val = Pool.Get<PooledList<RaycastHit>>();
				try
				{
					Vector3 direction = (flag ? Vector3.up : (-Vector3.up));
					GamePhysics.OBBSweep(new OBB(((Component)ReferenceBoxTrace).transform, new Bounds(ReferenceBoxTrace.center, ReferenceBoxTrace.size)), direction, MoveIncrement, (List<RaycastHit>)(object)val, 1755513089, (QueryTriggerInteraction)1);
					foreach (RaycastHit item in (List<RaycastHit>)(object)val)
					{
						BaseEntity entity = RaycastHitEx.GetEntity(item);
						if (!((Object)(object)entity == (Object)(object)this))
						{
							if ((Object)(object)entity != (Object)null && entity.isServer)
							{
								player.ShowBlockedByEntityToast(entity, BlockedByObjectPhrase);
								return;
							}
							if ((Object)(object)entity == (Object)null)
							{
								player.ShowToast(GameTip.Styles.Error, BlockedByObjectPhrase, false);
								return;
							}
						}
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			float chandelierLength = Mathf.Clamp(ChandelierLength + num, MinLength, MaxLength);
			SetChandelierLength(chandelierLength);
		}
	}

	private void SetChandelierLength(float length)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (length != ChandelierLength)
		{
			ChandelierLength = length;
			lastLength = length;
			StartServerTick();
			if (adjustHeightEffect.isValid)
			{
				Effect.server.Run(adjustHeightEffect.resourcePath, ((Component)this).transform.position);
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (isServer && lastLength != ChandelierLength)
		{
			StartServerTick();
			lastLength = ChandelierLength;
		}
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
				Debug.Log((object)("SyncVar Writing: ChandelierLength for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_ChandelierLength);
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
				_ = __sync_ChandelierLength;
				float _sync_ChandelierLength = reader.Float();
				__sync_ChandelierLength = _sync_ChandelierLength;
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
		if (propertyName == "ChandelierLength")
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
		__sync_ChandelierLength = 0f;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 0)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}

	static Chandelier()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
	}
}

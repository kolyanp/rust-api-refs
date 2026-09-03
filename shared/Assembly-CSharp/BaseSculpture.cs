using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Facepunch.MarchingCubes;
using Network;
using ProtoBuf;
using Rust;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseSculpture : BaseCombatEntity, IUGCBrowserEntity, IServerFileReceiver, IMarchingCubesTarget, IDisposable
{
	private static readonly ListHashSet<BaseSculpture> ServerUpdateProcessQueue = new ListHashSet<BaseSculpture>();

	private bool sculptureDirty;

	private Action resetBlockExcludeLayersAction;

	[Header("BaseSculpture")]
	[SerializeField]
	private MeshFilter targetMesh;

	[SerializeField]
	private MeshCollider sharedMeshCollider;

	[SerializeField]
	private MeshLOD meshLOD;

	[SerializeField]
	private Renderer clientBlockRenderer;

	[SerializeField]
	private DamageType carvingDamageType;

	[SerializeField]
	private Vector3Int gridResolution;

	[SerializeField]
	private Vector3 gridOffset;

	[SerializeField]
	private float gridScale;

	[SerializeField]
	public SDFSet SDFSet;

	[SerializeField]
	private TriggerPlayerForce playerPushTrigger;

	[SerializeField]
	private CapsuleCollider playerPushCollider;

	[Header("Mesh Painting")]
	[SerializeField]
	private GameObjectRef meshPaintDialogueRef;

	[SerializeField]
	public GameObjectRef BasePlate;

	[SerializeField]
	private MeshPaintableSource[] meshPaintableSources;

	private Mesh generationMesh;

	private Mesh generationCollisionMesh;

	private Mesh[] generationLodMeshes;

	[ClientVar(Default = "false", Help = "(Generated) When enabled, logs mesh vertex and triangle count statistics when a sculpture mesh is applied or modified")]
	public static bool LogMeshStats = false;

	[ReplicatedVar(Default = "false", Help = "Use convex colliders for generated blocks on both the client and server - slower to generate but blocks holes, only effects future modifications")]
	public static bool UseConvexColliders = false;

	private uint __sync_crc;

	public uint[] GetContentCRCs => new uint[1] { crc };

	public UGCType ContentType => UGCType.Sculpt;

	public List<ulong> EditingHistory => new List<ulong> { base.OwnerID };

	public BaseNetworkable UgcEntity => this;

	public string ContentString => string.Empty;

	private static string SculpturePath => ConVar.Server.GetServerFolder("sculptures");

	public Vector3Int GridResolution
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return gridResolution;
		}
	}

	public float3 GridOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return float3.op_Implicit(gridOffset);
		}
	}

	public float GridScale => gridScale;

	public Renderer ClientBlockRenderer => clientBlockRenderer;

	public Bounds GridBounds
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			return new Bounds(-gridOffset * gridScale, Vector3Int.op_Implicit(gridResolution) * gridScale);
		}
	}

	[Sync(Autosave = true, RequireChange = false, InvalidateCache = true, Pack = false)]
	private uint crc
	{
		[CompilerGenerated]
		get
		{
			return __sync_crc;
		}
		[CompilerGenerated]
		set
		{
			__sync_crc = value;
			byte nameID = __GetWeaverID("crc");
			SV_SyncVarSend(nameID);
		}
	}

	Mesh IMarchingCubesTarget.TargetMesh => generationMesh;

	Mesh IMarchingCubesTarget.TargetMeshForCollision => generationCollisionMesh;

	MeshCollider IMarchingCubesTarget.TargetMeshCollider => sharedMeshCollider;

	SDFSet IMarchingCubesTarget.SDFSet => SDFSet;

	Vector3 IMarchingCubesTarget.VertexOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return gridOffset;
		}
	}

	float IMarchingCubesTarget.VertexScale => gridScale;

	bool IMarchingCubesTarget.WantsConvexCollider => UseConvexColliders;

	int IMarchingCubesTarget.LodMeshCount
	{
		get
		{
			Mesh[] array = generationLodMeshes;
			if (array == null)
			{
				return 0;
			}
			return array.Length;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseSculpture.OnRpcMessage"))
		{
			if (rpc == 4267718869u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SV_LockSculpture"));
				}
				using (TimeWarning.New("SV_LockSculpture"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4267718869u, "SV_LockSculpture", this, player, 3f))
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
							SV_LockSculpture(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SV_LockSculpture");
					}
				}
				return true;
			}
			if (rpc == 2509595789u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SV_SendSculptureUpdate"));
				}
				using (TimeWarning.New("SV_SendSculptureUpdate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2509595789u, "SV_SendSculptureUpdate", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2509595789u, "SV_SendSculptureUpdate", this, player, 3f))
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
							SV_SendSculptureUpdate(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SV_SendSculptureUpdate");
					}
				}
				return true;
			}
			if (rpc == 1358295833 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SV_UnlockSculpture"));
				}
				using (TimeWarning.New("SV_UnlockSculpture"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1358295833u, "SV_UnlockSculpture", this, player, 3f))
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
							SV_UnlockSculpture(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in SV_UnlockSculpture");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (crc == 0)
		{
			ResetSet(SDFSet);
			MarkServerSculptureDirty();
		}
	}

	public void LoadFromData(byte[] arr)
	{
		Sculpt val = SculptFormat.LoadDisposableSculptFromStorage(arr);
		try
		{
			SDFSet.Chunks[0].CopyFromByteArray(val.data);
			MarkServerSculptureDirty();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void SV_LockSculpture(RPCMessage msg)
	{
		if (!msg.player.CanInteract() || !CanUpdateSculpture(msg.player))
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Locked, b: true);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void SV_UnlockSculpture(RPCMessage msg)
	{
		if (!msg.player.CanInteract() || !CanUpdateSculpture(msg.player, ignoreLock: true))
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Locked, b: false);
	}

	private void MarkServerSculptureDirty()
	{
		if (!sculptureDirty)
		{
			sculptureDirty = true;
			ServerUpdateProcessQueue.Add(this);
		}
	}

	public static void ProcessSculptureUpdates()
	{
		if (ServerUpdateProcessQueue.Count == 0)
		{
			return;
		}
		using (TimeWarning.New("FileUpdates"))
		{
			for (int i = 0; i < ServerUpdateProcessQueue.Count; i++)
			{
				BaseSculpture baseSculpture = ServerUpdateProcessQueue[i];
				if (!((Object)(object)baseSculpture == (Object)null))
				{
					Debug.Assert(baseSculpture.isServer, "Added client sculpture to server process queue");
					baseSculpture.ServerSculptureUpdate();
				}
			}
		}
		ServerUpdateProcessQueue.Clear();
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void SV_SendSculptureUpdate(RPCMessage msg)
	{
		if (msg.read.Length > 2000000 || !CanUpdateSculpture(msg.player) || !msg.read.TemporaryBytesWithSize(out var buffer, out var size))
		{
			return;
		}
		Sculpt val = SculptFormat.LoadDisposableSculptFromStorage(new ArraySegment<byte>(buffer, 0, size));
		try
		{
			if (val?.data.Array != null && val.data.Count != 0)
			{
				SDFSet.Chunks[0].CopyFromByteArray(val.data);
				MarkServerSculptureDirty();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void ServerSculptureUpdate()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ServerSculptureUpdate"))
		{
			BufferStream val = Pool.Get<BufferStream>().Initialize();
			try
			{
				val.Clear();
				SculptFormat.SerializeToSculpt(this, val);
				EnqueueMarchingCubesUpdate();
				byte[] storageReadyBuffer = SculptFormat.GetStorageReadyBuffer(val);
				FileStorage.server.Remove(crc, FileStorage.Type.sculpt, net.ID);
				crc = FileStorage.server.Store(storageReadyBuffer, FileStorage.Type.sculpt, net.ID);
				AdjustCollidersForPlayerIntersection();
				sculptureDirty = false;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	private void AdjustCollidersForPlayerIntersection()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)playerPushCollider))
		{
			return;
		}
		using (TimeWarning.New("AdjustCollidersForPlayerIntersection"))
		{
			int maxYLayer = SDFSet.GetMaxYLayer();
			Vector3 localScale = ((Component)playerPushCollider).transform.localScale;
			float num = maxYLayer;
			Vector3Int val = GridResolution;
			localScale.y = num / (float)((Vector3Int)(ref val)).y;
			((Component)playerPushCollider).transform.localScale = localScale;
			MeshCollider obj = sharedMeshCollider;
			((Collider)obj).excludeLayers = LayerMask.op_Implicit(LayerMask.op_Implicit(((Collider)obj).excludeLayers) | 0x1000);
			playerPushTrigger.pushVelocity = 5f;
			if (resetBlockExcludeLayersAction == null)
			{
				resetBlockExcludeLayersAction = ResetBlockExcludeLayers;
			}
			Invoke(resetBlockExcludeLayersAction, 3f);
		}
	}

	private void ResetBlockExcludeLayers()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)sharedMeshCollider))
		{
			((Collider)sharedMeshCollider).excludeLayers = LayerMask.op_Implicit(0);
			playerPushTrigger.pushVelocity = 0.25f;
		}
	}

	public override void OnPickedUpPreItemMove(Item createdItem, BasePlayer player)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		base.OnPickedUpPreItemMove(createdItem, player);
		ItemModSculpture itemModSculpture = default(ItemModSculpture);
		if (crc != 0 && ((Component)createdItem.info).TryGetComponent<ItemModSculpture>(ref itemModSculpture))
		{
			itemModSculpture.OnSculpturePickUp(net.ID, crc, createdItem);
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		base.OnDeployed(parent, deployedBy, fromItem);
		if (!ComponentEx.HasComponent<ItemModSculpture>((Component)(object)fromItem.info))
		{
			return;
		}
		AssociatedSculptureStorage associatedEntity = ItemModAssociatedEntity<AssociatedSculptureStorage>.GetAssociatedEntity(fromItem);
		if ((Object)(object)associatedEntity != (Object)null)
		{
			crc = associatedEntity.Crc;
			FileStorage.server.ReassignEntityId(associatedEntity.net.ID, net.ID);
			byte[] array = FileStorage.server.Get(crc, FileStorage.Type.sculpt, net.ID);
			if (array == null)
			{
				Debug.LogError((object)"[SCULPT] Missing sculpt data on-disk - fill with default");
				ClearContent();
			}
			else
			{
				PopulateSculptureFromEncodedData(array);
				MarkServerSculptureDirty();
			}
		}
		else
		{
			ClearContent();
		}
	}

	public void ClearContent()
	{
		ResetSet(SDFSet);
		MarkServerSculptureDirty();
	}

	internal override void DoServerDestroy()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		base.DoServerDestroy();
		FileStorage.server.RemoveAllByEntity(net.ID);
	}

	[ServerVar(ServerAdmin = true)]
	public static void ListSavedSculptures(ConsoleSystem.Arg arg)
	{
		string sculpturePath = SculpturePath;
		TextTable val = Pool.Get<TextTable>();
		val.Clear();
		val.AddColumn("Sculptures");
		foreach (string item in Directory.EnumerateFiles(sculpturePath))
		{
			val.AddRow(new string[1] { Path.GetRelativePath(sculpturePath, item) });
		}
		arg.ReplyWith(((object)val).ToString());
		Pool.Free<TextTable>(ref val);
	}

	[ServerVar(ServerAdmin = true)]
	public static void SaveSculpture(ConsoleSystem.Arg arg)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		string text = Path.ChangeExtension(Path.Combine(SculpturePath, arg.GetString(0)), ".sculpt");
		if (!string.IsNullOrEmpty(text))
		{
			if (!Directory.Exists(SculpturePath))
			{
				Directory.CreateDirectory(SculpturePath);
			}
			BaseSculpture baseSculpture = GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, ArgEx.Player(arg).eyes.HeadRay(), 0f, float.PositiveInfinity, -5, (QueryTriggerInteraction)0) as BaseSculpture;
			if (!((Object)(object)baseSculpture == (Object)null))
			{
				baseSculpture.SaveToFile(text);
				arg.ReplyWith("[SCULPTING] Saved to " + text.Replace('\\', '/'));
			}
		}
	}

	private void SaveToFile(string path)
	{
		BufferStream val = Pool.Get<BufferStream>().Initialize();
		try
		{
			val.Clear();
			SculptFormat.SerializeToSculpt(this, val);
			byte[] storageReadyBuffer = SculptFormat.GetStorageReadyBuffer(val);
			File.WriteAllBytes(path, storageReadyBuffer);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(ClientAdmin = true)]
	public static void LoadSculpture(ConsoleSystem.Arg arg)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		string path = Path.ChangeExtension(Path.Combine(SculpturePath, arg.GetString(0)), ".sculpt");
		if (!File.Exists(path))
		{
			return;
		}
		PooledList<BaseSculpture> val = Pool.Get<PooledList<BaseSculpture>>();
		try
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			float num = arg.GetFloat(1, -1f);
			if (num < 0f)
			{
				Ray ray = basePlayer.eyes.HeadRay();
				if (GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, ray, 0f, float.PositiveInfinity, -5, (QueryTriggerInteraction)0) is BaseSculpture item)
				{
					((List<BaseSculpture>)(object)val).Add(item);
				}
			}
			else
			{
				Vis.Components<BaseSculpture>(((Component)basePlayer).transform.position, num, (List<BaseSculpture>)(object)val, -1, (QueryTriggerInteraction)2);
			}
			if (((List<BaseSculpture>)(object)val).Count == 0)
			{
				return;
			}
			byte[] arr = Array.Empty<byte>();
			using (FileStream fileStream = File.OpenRead(path))
			{
				using BinaryReader binaryReader = new BinaryReader(fileStream);
				if (fileStream.Length > 16000000)
				{
					return;
				}
				arr = binaryReader.ReadBytes((int)fileStream.Length);
			}
			HashSet<NetworkableId> hashSet = Pool.Get<HashSet<NetworkableId>>();
			int num2 = 0;
			foreach (BaseSculpture item2 in (List<BaseSculpture>)(object)val)
			{
				if (item2.isServer && hashSet.Add(item2.net.ID))
				{
					num2++;
					item2.LoadFromData(arr);
				}
			}
			arg.ReplyWith($"Sent data to {num2} sculptures");
			Pool.FreeUnmanaged<NetworkableId>(ref hashSet);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(ClientAdmin = true)]
	public static void ApplyRandomShapes(ConsoleSystem.Arg arg)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		PooledList<BaseSculpture> val = Pool.Get<PooledList<BaseSculpture>>();
		try
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			float num = arg.GetFloat(0, -1f);
			if (num < 0f)
			{
				Ray ray = basePlayer.eyes.HeadRay();
				if (GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, ray, 0f, float.PositiveInfinity, -5, (QueryTriggerInteraction)0) is BaseSculpture item)
				{
					((List<BaseSculpture>)(object)val).Add(item);
				}
			}
			else
			{
				Vis.Components<BaseSculpture>(((Component)basePlayer).transform.position, num, (List<BaseSculpture>)(object)val, -1, (QueryTriggerInteraction)2);
			}
			if (((List<BaseSculpture>)(object)val).Count == 0)
			{
				return;
			}
			HashSet<NetworkableId> hashSet = Pool.Get<HashSet<NetworkableId>>();
			int num2 = 0;
			foreach (BaseSculpture item2 in (List<BaseSculpture>)(object)val)
			{
				if (!item2.isClient && hashSet.Add(item2.net.ID))
				{
					num2++;
					item2.PopulateWithRandomShapes();
				}
			}
			arg.ReplyWith($"Applied random shapes to {num2} sculptures");
			Pool.FreeUnmanaged<NetworkableId>(ref hashSet);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void PopulateWithRandomShapes()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		Bounds chunkBoundsSetSpace = SDFSet.Chunks[0].ChunkBoundsSetSpace;
		float3 val = float3.op_Implicit(((Bounds)(ref chunkBoundsSetSpace)).center);
		float3 val2 = float3.op_Implicit(((Bounds)(ref chunkBoundsSetSpace)).extents);
		float3 val3 = float3.op_Implicit(((Bounds)(ref chunkBoundsSetSpace)).min);
		float num = math.cmin(val2);
		SDFSet.ClearAllMods();
		float num2 = ((Bounds)(ref chunkBoundsSetSpace)).size.y - 10f;
		SDFSet.AddAABBMod(new float3(val.x, val3.y + 10f + num2, val.z), new float3(val2.x * 2f, num2, val2.z * 2f), isAdditive: false);
		int num3 = Random.Range(6, 12);
		float3 blockSpacePos = default(float3);
		for (int i = 0; i < num3; i++)
		{
			((float3)(ref blockSpacePos))._002Ector(val.x + Random.Range(-0.7f, 0.7f) * val2.x, val3.y + Random.Range(2f, 10f + val2.y * 0.5f), val.z + Random.Range(-0.7f, 0.7f) * val2.z);
			float3 val4 = new float3(Random.Range(0.15f, 0.4f), Random.Range(0.15f, 0.4f), Random.Range(0.15f, 0.4f)) * num;
			quaternion rotation = quaternion.op_Implicit(Random.rotationUniform);
			float smoothing = SDFSet.SmoothingForRadius(Random.value, math.cmin(val4));
			switch (Random.Range(0, 7))
			{
			case 0:
				SDFSet.AddSphereMod(blockSpacePos, val4.x, isAdditive: true, smoothing);
				break;
			case 1:
				SDFSet.AddAABBMod(blockSpacePos, val4, isAdditive: true, smoothing);
				break;
			case 2:
				SDFSet.AddOBBMod(blockSpacePos, val4, rotation, isAdditive: true, smoothing);
				break;
			case 3:
				SDFSet.AddCylinderMod(blockSpacePos, val4, rotation, isAdditive: true, smoothing);
				break;
			case 4:
				SDFSet.AddCapsuleMod(blockSpacePos, val4, rotation, isAdditive: true, smoothing);
				break;
			case 5:
				SDFSet.AddConeMod(blockSpacePos, val4, rotation, isAdditive: true, smoothing);
				break;
			case 6:
				SDFSet.AddHexPrismMod(blockSpacePos, val4, rotation, isAdditive: true, smoothing);
				break;
			}
		}
		SDFSet.ScheduleRegenerateAllChunks();
		MarkServerSculptureDirty();
	}

	[ServerVar(ClientAdmin = true)]
	public static void PrintCrc(ConsoleSystem.Arg arg)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		PooledList<BaseSculpture> val = Pool.Get<PooledList<BaseSculpture>>();
		try
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			float num = arg.GetFloat(0, -1f);
			if (num < 0f)
			{
				Ray ray = basePlayer.eyes.HeadRay();
				if (GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, ray, 0f, float.PositiveInfinity, -5, (QueryTriggerInteraction)0) is BaseSculpture item)
				{
					((List<BaseSculpture>)(object)val).Add(item);
				}
			}
			else
			{
				Vis.Components<BaseSculpture>(((Component)basePlayer).transform.position, num, (List<BaseSculpture>)(object)val, -1, (QueryTriggerInteraction)2);
			}
			if (((List<BaseSculpture>)(object)val).Count == 0)
			{
				return;
			}
			TextTable val2 = Pool.Get<TextTable>();
			try
			{
				val2.AddColumns(new string[4] { "netID", "crc", "__sync_crc", "matches" });
				HashSet<NetworkableId> hashSet = Pool.Get<HashSet<NetworkableId>>();
				foreach (BaseSculpture item2 in (List<BaseSculpture>)(object)val)
				{
					if (!item2.isClient && hashSet.Add(item2.net.ID))
					{
						val2.AddRow(new string[4]
						{
							((object)System.Runtime.CompilerServices.Unsafe.As<NetworkableId, NetworkableId>(ref item2.net.ID)/*cast due to constrained. prefix*/).ToString(),
							item2.crc.ToString(),
							item2.__sync_crc.ToString(),
							(item2.crc == item2.__sync_crc).ToString()
						});
					}
				}
				arg.ReplyWith(((object)val2).ToString());
				Pool.FreeUnmanaged<NetworkableId>(ref hashSet);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	Mesh IMarchingCubesTarget.GetLodMesh(int level)
	{
		int num = level - 1;
		if (generationLodMeshes == null || num < 0 || num >= generationLodMeshes.Length)
		{
			return null;
		}
		return generationLodMeshes[num];
	}

	void IMarchingCubesTarget.OnRenderMeshesUpdated()
	{
	}

	public override void InitShared()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		base.InitShared();
		Bounds gridBounds = GridBounds;
		generationMesh = new Mesh
		{
			name = ((Object)this).name,
			bounds = gridBounds
		};
		generationCollisionMesh = new Mesh
		{
			name = ((Object)this).name + "_collision",
			bounds = gridBounds
		};
		if (base.isClient)
		{
			targetMesh.sharedMesh = generationMesh;
			InitLodMeshes();
		}
		sharedMeshCollider.sharedMesh = generationCollisionMesh;
		SDFSet.Init();
		SDFSet.AddChunk(int3.zero, new int3(((Vector3Int)(ref gridResolution)).x, ((Vector3Int)(ref gridResolution)).y, ((Vector3Int)(ref gridResolution)).z));
	}

	private void InitLodMeshes()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		if ((Object)(object)meshLOD == (Object)null || meshLOD.States == null || meshLOD.States.Length == 0)
		{
			return;
		}
		meshLOD.States[0].mesh = generationMesh;
		int num = Mathf.Min(meshLOD.States.Length - 2, 2);
		if (num > 0)
		{
			Bounds gridBounds = GridBounds;
			generationLodMeshes = (Mesh[])(object)new Mesh[num];
			for (int i = 0; i < num; i++)
			{
				generationLodMeshes[i] = new Mesh
				{
					name = $"{((Object)this).name}_lod{i + 1}",
					bounds = gridBounds
				};
				meshLOD.States[i + 1].mesh = generationLodMeshes[i];
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
	}

	public bool CanUpdateSculpture(BasePlayer player, bool ignoreLock = false)
	{
		if (!ignoreLock && IsLocked())
		{
			return false;
		}
		if (player.IsAdmin || player.IsDeveloper)
		{
			return true;
		}
		if (!player.CanBuild())
		{
			return false;
		}
		return true;
	}

	private void EnqueueMarchingCubesUpdate()
	{
		MarchingCubesManager.Instance.Enqueue(this);
	}

	private void PopulateSculptureFromEncodedData(byte[] encoded)
	{
		Sculpt val = SculptFormat.LoadDisposableSculptFromStorage(encoded);
		try
		{
			SDFSet.Chunks[0].CopyFromByteArray(val.data);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void ResetSet(SDFSet set)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		Debug.Assert(set.Chunks.Count == 1);
		set.ClearChunks();
		set.ClearAllMods();
		set.AddAABBMod(float3.op_Implicit(((Bounds)(ref set.Chunks[0].ChunkBoundsSetSpace)).center), float3.op_Implicit(((Bounds)(ref set.Chunks[0].ChunkBoundsSetSpace)).extents * 2f), isAdditive: true);
		set.ScheduleRegenerateAllChunks();
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		Dispose();
	}

	public void Dispose()
	{
		SDFSet.Dispose();
		if (Object.op_Implicit((Object)(object)generationMesh))
		{
			Object.Destroy((Object)(object)generationMesh);
		}
		if (Object.op_Implicit((Object)(object)generationCollisionMesh))
		{
			Object.Destroy((Object)(object)generationCollisionMesh);
		}
		if (generationLodMeshes == null)
		{
			return;
		}
		for (int i = 0; i < generationLodMeshes.Length; i++)
		{
			if (Object.op_Implicit((Object)(object)generationLodMeshes[i]))
			{
				Object.Destroy((Object)(object)generationLodMeshes[i]);
			}
		}
		generationLodMeshes = null;
	}

	private void OnSyncVar_crc(uint? oldValue, uint newValue)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BaseSculpture.OnSyncVar_crc"))
		{
			if (base.isServer && !oldValue.HasValue)
			{
				byte[] array = FileStorage.server.Get(newValue, FileStorage.Type.sculpt, net.ID);
				if (array == null || array.Length == 0)
				{
					Debug.LogWarning((object)$"[SCULPTING] ({net.ID}) Missing sculpt data on-disk for - fill with default");
					ResetSet(SDFSet);
				}
				else
				{
					PopulateSculptureFromEncodedData(array);
				}
				MarkServerSculptureDirty();
			}
		}
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
				Debug.Log((object)("SyncVar Writing: crc for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_crc);
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
				uint? oldValue = __sync_crc;
				uint newValue = (__sync_crc = reader.UInt32());
				if (fromAutoSave)
				{
					oldValue = null;
				}
				OnSyncVar_crc(oldValue, newValue);
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
		if (propertyName == "crc")
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
		base.ResetSyncVars();
		__sync_crc = 0u;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 0)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}

	public BaseSculpture()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		gridResolution = new Vector3Int(32, 32, 32);
		base._002Ector();
	}
}

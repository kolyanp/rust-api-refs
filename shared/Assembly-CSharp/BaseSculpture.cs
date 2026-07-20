using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.MarchingCubes;
using LZ4;
using Network;
using ProtoBuf;
using Rust;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseSculpture : IOEntity, IServerFileReceiver, IUGCBrowserEntity, ISplashable, IDisposable
{
	[Serializable]
	public struct ColorSetting
	{
		public GameObject toggleObj;

		public Phrase name;

		public Phrase desc;

		public Color color;

		[ColorUsage(false, true)]
		public Color materialColor;
	}

	[Header("BaseSculpture")]
	[SerializeField]
	private MeshFilter targetMesh;

	[SerializeField]
	private MeshCollider clientMeshCollider;

	[SerializeField]
	private Renderer clientBlockRenderer;

	[SerializeField]
	private DamageType carvingDamageType;

	[SerializeField]
	private Vector3Int gridResolution = new Vector3Int(32, 32, 32);

	[SerializeField]
	private Vector3 gridOffset;

	[SerializeField]
	private float gridScale;

	[SerializeField]
	private GameObjectRef blockImpactEffect;

	[SerializeField]
	private Collider blockerCollider;

	[SerializeField]
	[Header("HitGuide")]
	private GameObject hitGuide;

	[SerializeField]
	private GameObject carvingGuide;

	[SerializeField]
	private GameObject smoothingGuide;

	[SerializeField]
	private float guideLerpSpeed = 5f;

	[SerializeField]
	private Vector3 carveColorMultiplier;

	[SerializeField]
	private Vector3 smoothColorMultiplier;

	[Header("IO")]
	[SerializeField]
	private ColorSetting[] colorSettings;

	[SerializeField]
	private Renderer[] lightRenderers;

	[SerializeField]
	private Material noLightMaterial;

	[SerializeField]
	private Material lightMaterial;

	public const int CarveDepth = 3;

	private Point3DGrid _grid;

	private uint _crc = uint.MaxValue;

	private int _currentColorIndex;

	private bool _hasMovementBlocker;

	private Transform _movementBlockerTransform;

	private int _cachedMaxY;

	private int _carveRadius;

	private int _minCarveRadius;

	private int _maxCarveRadius;

	private static readonly byte[] _decompressArr = new byte[8192];

	[ClientVar(Default = "false", Help = "(Generated) When enabled, logs mesh vertex and triangle count statistics when a sculpture mesh is applied or modified")]
	public static bool LogMeshStats = false;

	private static readonly ListHashSet<BaseSculpture> ServerUpdateProcessQueue = new ListHashSet<BaseSculpture>();

	private bool _gridDirty;

	private Action _resetSplashedThisFrame;

	private bool _splashedThisFrame;

	private int CarveRadius
	{
		get
		{
			return _carveRadius;
		}
		set
		{
			_carveRadius = math.clamp(value, _minCarveRadius, _maxCarveRadius);
		}
	}

	public uint[] GetContentCRCs => new uint[1] { _crc };

	public UGCType ContentType => UGCType.Sculpt;

	public List<ulong> EditingHistory => new List<ulong> { base.OwnerID };

	public BaseNetworkable UgcEntity => this;

	public string ContentString => string.Empty;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0638: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BaseSculpture.OnRpcMessage"))
		{
			if (rpc == 3180266995u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SV_Add"));
				}
				using (TimeWarning.New("SV_Add"))
				{
					using (TimeWarning.New("Conditions"))
					{
						long position = msg.read.Position;
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						msg.read.Read<int>();
						msg.read.Position = position;
						if (!RPC_Server.MaxDistance.Test(3180266995u, "SV_Add", this, player, 3f))
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
							SV_Add(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SV_Add");
					}
				}
				return true;
			}
			if (rpc == 737203553 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SV_Carve"));
				}
				using (TimeWarning.New("SV_Carve"))
				{
					using (TimeWarning.New("Conditions"))
					{
						long position2 = msg.read.Position;
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						msg.read.Read<int>();
						msg.read.Position = position2;
						if (!RPC_Server.MaxDistance.Test(737203553u, "SV_Carve", this, player, 3f))
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
							SV_Carve(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SV_Carve");
					}
				}
				return true;
			}
			if (rpc == 3650562316u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SV_LoadFromData"));
				}
				using (TimeWarning.New("SV_LoadFromData"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3650562316u, "SV_LoadFromData", this, player, 1uL))
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
							SV_LoadFromData(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in SV_LoadFromData");
					}
				}
				return true;
			}
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
							RPCMessage msg5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SV_LockSculpture(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in SV_LockSculpture");
					}
				}
				return true;
			}
			if (rpc == 2374043062u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SV_SetColorIndex"));
				}
				using (TimeWarning.New("SV_SetColorIndex"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2374043062u, "SV_SetColorIndex", this, player, 3f))
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
							SV_SetColorIndex(msg6);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in SV_SetColorIndex");
					}
				}
				return true;
			}
			if (rpc == 2622097655u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SV_Smooth"));
				}
				using (TimeWarning.New("SV_Smooth"))
				{
					using (TimeWarning.New("Conditions"))
					{
						long position3 = msg.read.Position;
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						msg.read.Read<int>();
						msg.read.Position = position3;
						if (!RPC_Server.MaxDistance.Test(2622097655u, "SV_Smooth", this, player, 3f))
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
							SV_Smooth(msg7);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
						player.Kick("RPC Error in SV_Smooth");
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
							RPCMessage msg8 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SV_UnlockSculpture(msg8);
						}
					}
					catch (Exception ex7)
					{
						Debug.LogException(ex7);
						player.Kick("RPC Error in SV_UnlockSculpture");
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
		_grid = new Point3DGrid(((Vector3Int)(ref gridResolution)).x, ((Vector3Int)(ref gridResolution)).y, ((Vector3Int)(ref gridResolution)).z);
		_hasMovementBlocker = (Object)(object)blockerCollider != (Object)null;
		if (_hasMovementBlocker)
		{
			_movementBlockerTransform = ((Component)blockerCollider).transform;
			_cachedMaxY = ((Vector3Int)(ref gridResolution)).y - 1;
		}
	}

	public override void ServerInit()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		if (_crc == uint.MaxValue)
		{
			FillGrid(_grid);
		}
		else
		{
			byte[] array = FileStorage.server.Get(_crc, FileStorage.Type.sculpt, net.ID);
			if (array == null)
			{
				Debug.LogError((object)"Missing sculpt data on-disk - fill with default");
				FillGrid(_grid);
			}
			else
			{
				PopulateGridFromEncodedData(array);
			}
		}
		MarkServerGridUpdate();
		_resetSplashedThisFrame = ResetSplashedThisFrame;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Sprinkler.SplashableGrid.RegisterEntity(this);
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		Sprinkler.SplashableGrid.OnParentChanged(this, oldParent, newParent);
	}

	public bool CanUpdateSculpture(BasePlayer player)
	{
		if (IsLocked())
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

	public override void OnAttacked(HitInfo info)
	{
		if (info.damageTypes.Contains(carvingDamageType) && base.isServer)
		{
			info.DidHit = false;
			info.DoHitEffects = false;
		}
		else
		{
			base.OnAttacked(info);
		}
	}

	private void PopulateGridFromEncodedData(byte[] encoded)
	{
		int count = LZ4Codec.Decode(encoded, 0, encoded.Length, _decompressArr, 0, _decompressArr.Length, false);
		_grid.CopyFromByteArray(_decompressArr, count);
	}

	private static void FillGrid(Point3DGrid grid)
	{
		for (int i = 1; i < grid.Width - 1; i++)
		{
			for (int j = 1; j < grid.Height - 1; j++)
			{
				for (int k = 1; k < grid.Depth - 1; k++)
				{
					grid[i, j, k] = true;
				}
			}
		}
	}

	private int3 GetInBlockSpace(Vector3 worldSpace)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new int3(GetInBlockSpaceFloat(worldSpace));
	}

	private float3 GetInBlockSpaceFloat(Vector3 worldSpace)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)clientMeshCollider).transform.InverseTransformPoint(worldSpace);
		Vector3 val2 = new Vector3((float)((Vector3Int)(ref gridResolution)).x, (float)((Vector3Int)(ref gridResolution)).y, (float)((Vector3Int)(ref gridResolution)).z) * 0.5f;
		return float3.op_Implicit(val * (1f / gridScale) + (val2 + gridOffset));
	}

	private Vector3 GetInWorldSpace(int3 blockSpace)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = new Vector3((float)((Vector3Int)(ref gridResolution)).x, (float)((Vector3Int)(ref gridResolution)).y, (float)((Vector3Int)(ref gridResolution)).z) * 0.5f;
		Vector3 val2 = (float3.op_Implicit(float3.op_Implicit(blockSpace)) - (val + gridOffset)) * gridScale;
		return ((Component)clientMeshCollider).transform.TransformPoint(val2);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.baseSculpture = Pool.Get<BaseSculpture>();
		info.msg.baseSculpture.crc = _crc;
		info.msg.baseSculpture.colourSelection = _currentColorIndex;
	}

	public override void Load(LoadInfo info)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.baseSculpture == null)
		{
			return;
		}
		uint crc = _crc;
		_ = _currentColorIndex;
		_crc = info.msg.baseSculpture.crc;
		_currentColorIndex = info.msg.baseSculpture.colourSelection;
		if (base.isServer && info.fromDisk && crc != _crc)
		{
			byte[] array = FileStorage.server.Get(_crc, FileStorage.Type.sculpt, net.ID);
			if (array == null)
			{
				Debug.LogError((object)"Missing sculpt data on-disk - fill with default");
				FillGrid(_grid);
			}
			else
			{
				PopulateGridFromEncodedData(array);
				MarkServerGridUpdate();
			}
		}
	}

	private void UpdateMovementBlocker()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("UpdateMovementBlocker"))
		{
			if (_hasMovementBlocker)
			{
				int num = FindMaxY();
				if (num <= 0)
				{
					blockerCollider.enabled = false;
					return;
				}
				float num2 = (float)num / (float)((Vector3Int)(ref gridResolution)).y;
				_movementBlockerTransform.localScale = Vector3Ex.WithY(_movementBlockerTransform.localScale, num2);
			}
		}
		int FindMaxY()
		{
			for (int num3 = _cachedMaxY; num3 >= 0; num3--)
			{
				for (int i = 0; i < ((Vector3Int)(ref gridResolution)).x; i++)
				{
					for (int j = 0; j < ((Vector3Int)(ref gridResolution)).z; j++)
					{
						if (_grid[i, num3, j])
						{
							_cachedMaxY = num3;
							return num3;
						}
					}
				}
			}
			return -1;
		}
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		Dispose();
	}

	public void Dispose()
	{
		_grid.Dispose();
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void SV_SetColorIndex(RPCMessage msg)
	{
		if (CanUpdateSculpture(msg.player))
		{
			int num = msg.read.Int32();
			if (num >= 0 && num < colorSettings.Length)
			{
				_currentColorIndex = num;
				SendNetworkUpdate();
			}
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	public void SV_LoadFromData(RPCMessage msg)
	{
		if (msg.player.IsAdmin && msg.player.IsDeveloper)
		{
			ArraySegment<byte> arraySegment = msg.read.BytesSegmentWithSize();
			int count = LZ4Codec.Decode(arraySegment.Array, arraySegment.Offset, arraySegment.Count, _decompressArr, 0, _decompressArr.Length, false);
			_grid.CopyFromByteArray(_decompressArr, count);
			MarkServerGridUpdate();
		}
	}

	private bool TryGetHeldCarvingAttributeServer(BasePlayer player, out SculptingToolData attribute)
	{
		attribute = null;
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		HeldEntity heldEntity = player.GetHeldEntity();
		if ((Object)(object)heldEntity == (Object)null)
		{
			return false;
		}
		attribute = PrefabAttribute.server.Find<SculptingToolData>(heldEntity.prefabID);
		if (attribute == null)
		{
			return false;
		}
		return true;
	}

	[RPC_Server]
	[RPC_Server.InputValidation(new Type[]
	{
		typeof(Vector3),
		typeof(int)
	})]
	[RPC_Server.MaxDistance(3f)]
	public void SV_Add(RPCMessage msg)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (CanUpdateSculpture(msg.player))
		{
			Vector3 worldSpacePosition = msg.read.Vector3();
			if (TryGetHeldCarvingAttributeServer(msg.player, out var attribute) && attribute.AllowCarve)
			{
				int r = Mathf.Clamp(msg.read.Int32(), attribute.MinCarvingSize, attribute.MaxCarvingSize);
				AddSphere(worldSpacePosition, r);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.InputValidation(new Type[]
	{
		typeof(Vector3),
		typeof(int)
	})]
	[RPC_Server.MaxDistance(3f)]
	public void SV_Carve(RPCMessage msg)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (!CanUpdateSculpture(msg.player))
		{
			return;
		}
		Vector3 worldSpacePosition = msg.read.Vector3();
		if (TryGetHeldCarvingAttributeServer(msg.player, out var attribute) && attribute.AllowCarve)
		{
			int r = Mathf.Clamp(msg.read.Int32(), attribute.MinCarvingSize, attribute.MaxCarvingSize);
			switch (attribute.CarvingShape)
			{
			case SculptingToolData.CarvingShapeType.Cylinder:
				CarveCylinder(worldSpacePosition, msg.player.eyes.HeadForward(), r, 3);
				break;
			case SculptingToolData.CarvingShapeType.Sphere:
				CarveSphere(worldSpacePosition, r);
				break;
			case SculptingToolData.CarvingShapeType.Rectangle:
				break;
			}
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.InputValidation(new Type[]
	{
		typeof(Vector3),
		typeof(int)
	})]
	public void SV_Smooth(RPCMessage msg)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (!CanUpdateSculpture(msg.player))
		{
			return;
		}
		Vector3 worldSpacePosition = msg.read.Vector3();
		if (TryGetHeldCarvingAttributeServer(msg.player, out var attribute) && attribute.AllowSmooth)
		{
			int r = Mathf.Clamp(msg.read.Int32(), attribute.MinCarvingSize, attribute.MaxCarvingSize);
			switch (attribute.CarvingShape)
			{
			case SculptingToolData.CarvingShapeType.Cylinder:
				SmoothCylinder(worldSpacePosition, msg.player.eyes.HeadForward(), r, 3);
				break;
			case SculptingToolData.CarvingShapeType.Sphere:
				SmoothSphere(worldSpacePosition, r);
				break;
			case SculptingToolData.CarvingShapeType.Rectangle:
				break;
			}
		}
	}

	private void CarveCylinder(Vector3 worldSpacePosition, Vector3 worldSpaceView, int r, int depth)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)clientMeshCollider).transform.InverseTransformDirection(worldSpaceView);
		float3 inBlockSpaceFloat = GetInBlockSpaceFloat(worldSpacePosition);
		CarveAndBlurCylinderJob carveAndBlurCylinderJob = new CarveAndBlurCylinderJob
		{
			Grid = _grid,
			P0 = inBlockSpaceFloat,
			P1 = inBlockSpaceFloat + float3.op_Implicit(val) * (float)depth,
			R = r
		};
		IJobExtensions.RunByRef<CarveAndBlurCylinderJob>(ref carveAndBlurCylinderJob);
		MarkServerGridUpdate();
	}

	private void SmoothCylinder(Vector3 worldSpacePosition, Vector3 worldSpaceView, int r, int depth)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)clientMeshCollider).transform.InverseTransformDirection(worldSpaceView);
		float3 inBlockSpaceFloat = GetInBlockSpaceFloat(worldSpacePosition);
		BoxBlurCylinderJob boxBlurCylinderJob = new BoxBlurCylinderJob
		{
			Grid = _grid,
			P0 = inBlockSpaceFloat,
			P1 = inBlockSpaceFloat + float3.op_Implicit(val) * (float)depth,
			R = r
		};
		IJobExtensions.RunByRef<BoxBlurCylinderJob>(ref boxBlurCylinderJob);
		MarkServerGridUpdate();
	}

	private void AddSphere(Vector3 worldSpacePosition, int r)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		int3 inBlockSpace = GetInBlockSpace(worldSpacePosition);
		global::AddAndBlurSphereJob addAndBlurSphereJob = new global::AddAndBlurSphereJob
		{
			Grid = _grid,
			Origin = inBlockSpace,
			R = r
		};
		IJobExtensions.RunByRef<global::AddAndBlurSphereJob>(ref addAndBlurSphereJob);
		MarkServerGridUpdate();
	}

	private void CarveSphere(Vector3 worldSpacePosition, int r)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		int3 inBlockSpace = GetInBlockSpace(worldSpacePosition);
		global::CarveAndBlurSphereJob carveAndBlurSphereJob = new global::CarveAndBlurSphereJob
		{
			Grid = _grid,
			Origin = inBlockSpace,
			R = r
		};
		IJobExtensions.RunByRef<global::CarveAndBlurSphereJob>(ref carveAndBlurSphereJob);
		MarkServerGridUpdate();
	}

	private void SmoothSphere(Vector3 worldSpacePosition, int r)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		int3 inBlockSpace = GetInBlockSpace(worldSpacePosition);
		global::BoxBlurSphereJob boxBlurSphereJob = new global::BoxBlurSphereJob
		{
			Grid = _grid,
			Origin = inBlockSpace,
			R = r
		};
		IJobExtensions.RunByRef<global::BoxBlurSphereJob>(ref boxBlurSphereJob);
		MarkServerGridUpdate();
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		if (!IsLocked())
		{
			return (Object)(object)splashType != (Object)(object)WaterTypes.RadioactiveWaterItemDef;
		}
		return false;
	}

	private void ResetSplashedThisFrame()
	{
		_splashedThisFrame = false;
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (_splashedThisFrame)
		{
			return 0;
		}
		if (amount < 200)
		{
			return amount;
		}
		_splashedThisFrame = true;
		Invoke(_resetSplashedThisFrame, 0f);
		Debug.Log((object)"Splash");
		if ((Object)(object)splashType == (Object)(object)WaterTypes.WaterItemDef)
		{
			NativeBitArray other = default(NativeBitArray);
			((NativeBitArray)(ref other))._002Ector(_grid.Length, AllocatorHandle.op_Implicit((Allocator)3), (NativeArrayOptions)0);
			global::BoxBlur3DJob boxBlur3DJob = new global::BoxBlur3DJob
			{
				InputGrid = _grid,
				OutputGrid = other,
				Width = _grid.Width,
				WidthHeight = _grid.Width * _grid.Height
			};
			IJobForExtensions.RunByRef<global::BoxBlur3DJob>(ref boxBlur3DJob, _grid.Length);
			_grid.CopyFromNativeBitArray(ref other);
			((NativeBitArray)(ref other)).Dispose();
			MarkServerGridUpdate();
		}
		return 200;
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void SV_LockSculpture(RPCMessage msg)
	{
		if (!msg.player.CanInteract() || !CanUpdateSculpture(msg.player))
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Locked, b: true);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void SV_UnlockSculpture(RPCMessage msg)
	{
		if (!msg.player.CanInteract() || !CanUpdateSculpture(msg.player))
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Locked, b: false);
	}

	private void MarkServerGridUpdate()
	{
		if (!_gridDirty)
		{
			_gridDirty = true;
			ServerUpdateProcessQueue.Add(this);
		}
	}

	private JobHandle ScheduleRemoveIslandsFromGrid()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return IJobExtensions.Schedule<global::CleanFloatingIslandsJob>(new global::CleanFloatingIslandsJob
		{
			Sampler = _grid
		}, default(JobHandle));
	}

	public static void ProcessGridUpdates()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (ServerUpdateProcessQueue.Count == 0)
		{
			return;
		}
		using (TimeWarning.New("RemoveIslandsFromGrid"))
		{
			NativeArray<JobHandle> val = default(NativeArray<JobHandle>);
			val._002Ector(ServerUpdateProcessQueue.Count, (Allocator)2, (NativeArrayOptions)1);
			for (int i = 0; i < ServerUpdateProcessQueue.Count; i++)
			{
				BaseSculpture baseSculpture = ServerUpdateProcessQueue[i];
				if (!((Object)(object)baseSculpture == (Object)null))
				{
					val[i] = baseSculpture.ScheduleRemoveIslandsFromGrid();
				}
			}
			JobHandle.CompleteAll(val);
		}
		using (TimeWarning.New("FileUpdates"))
		{
			for (int j = 0; j < ServerUpdateProcessQueue.Count; j++)
			{
				BaseSculpture baseSculpture2 = ServerUpdateProcessQueue[j];
				if (!((Object)(object)baseSculpture2 == (Object)null))
				{
					baseSculpture2.ServerGridUpdate();
				}
			}
		}
		ServerUpdateProcessQueue.Clear();
	}

	private void ServerGridUpdate()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		byte[] arr = FileStorage.server.Get(_crc, FileStorage.Type.sculpt, net.ID);
		bool num = arr != null;
		if (!num)
		{
			arr = Array.Empty<byte>();
		}
		_grid.CopyToByteArray(ref arr);
		if (num)
		{
			FileStorage.server.Remove(_crc, FileStorage.Type.sculpt, net.ID);
		}
		arr = LZ4Codec.Encode(arr, 0, arr.Length);
		_crc = FileStorage.server.Store(arr, FileStorage.Type.sculpt, net.ID);
		InvalidateNetworkCache();
		ClientRPC(RpcTarget.NetworkGroup("CL_UpdateCrc"), _crc);
		UpdateMovementBlocker();
		_gridDirty = false;
	}

	public override void OnPickedUpPreItemMove(Item createdItem, BasePlayer player)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		base.OnPickedUpPreItemMove(createdItem, player);
		ItemModSculpture itemModSculpture = default(ItemModSculpture);
		if (_crc != uint.MaxValue && ((Component)createdItem.info).TryGetComponent<ItemModSculpture>(ref itemModSculpture))
		{
			itemModSculpture.OnSculpturePickUp(net.ID, _crc, createdItem);
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		base.OnDeployed(parent, deployedBy, fromItem);
		if (ComponentEx.HasComponent<ItemModSculpture>((Component)(object)fromItem.info))
		{
			AssociatedSculptureStorage associatedEntity = ItemModAssociatedEntity<AssociatedSculptureStorage>.GetAssociatedEntity(fromItem);
			if ((Object)(object)associatedEntity != (Object)null)
			{
				_crc = associatedEntity.Crc;
				FileStorage.server.ReassignEntityId(associatedEntity.net.ID, net.ID);
				byte[] array = FileStorage.server.Get(_crc, FileStorage.Type.sculpt, net.ID);
				if (array == null)
				{
					Debug.LogError((object)"Missing sculpt data on-disk - fill with default");
					FillGrid(_grid);
				}
				else
				{
					PopulateGridFromEncodedData(array);
					InvalidateNetworkCache();
					ClientRPC(RpcTarget.NetworkGroup("CL_UpdateCrc"), _crc);
				}
			}
		}
		Sprinkler.SplashableGrid.RegisterEntity(this);
	}

	public void ClearContent()
	{
		FillGrid(_grid);
		MarkServerGridUpdate();
	}

	internal override void DoServerDestroy()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		base.DoServerDestroy();
		FileStorage.server.RemoveAllByEntity(net.ID);
		Sprinkler.SplashableGrid.DeregisterEntity(this);
	}

	public override void ResetIOState()
	{
		base.ResetIOState();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, b: false);
	}

	public override void UpdateFromInput(int inputAmount, int inputSlot)
	{
		base.UpdateFromInput(inputAmount, inputSlot);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, IsPowered());
	}
}

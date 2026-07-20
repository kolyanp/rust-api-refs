using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer.Cameras;

public class CameraRenderer : IPooled
{
	[ServerVar(Help = "(Generated) When enabled, the companion server camera rendering system is active and processes camera render requests from the companion app")]
	public static bool enabled = true;

	[ServerVar(Help = "(Generated) Per-frame CPU budget in milliseconds for completing pending companion server camera renders")]
	public static float completionFrameBudgetMs = 5f;

	[ServerVar(Help = "(Generated) Maximum number of camera render tasks that can complete per frame for companion server cameras")]
	public static int maxRendersPerFrame = 25;

	[ServerVar(Help = "(Generated) Maximum number of raycasts per frame used for companion server camera depth sampling")]
	public static int maxRaysPerFrame = 100000;

	[ServerVar(Help = "(Generated) Width in pixels of the companion server camera render output; default 160")]
	public static int width = 160;

	[ServerVar(Help = "(Generated) Height in pixels of the companion server camera render output; default 90")]
	public static int height = 90;

	[ServerVar(Help = "(Generated) Vertical field of view in degrees for companion server camera renders; default 65")]
	public static float verticalFov = 65f;

	[ServerVar(Help = "(Generated) Near clipping plane distance for companion server camera renders; 0 = use default")]
	public static float nearPlane = 0f;

	[ServerVar(Help = "(Generated) Far clipping plane distance in metres for companion server camera renders; default 250")]
	public static float farPlane = 250f;

	[ServerVar(Help = "(Generated) Physics layer mask used for raycasting in companion server camera depth sampling; defaults to solid, water, and player movement layers")]
	public static int layerMask = 1218656529;

	[ServerVar(Help = "(Generated) Interval in seconds between successive companion server camera render dispatches; default 0.05s (20 Hz)")]
	public static float renderInterval = 0.05f;

	[ServerVar(Help = "(Generated) Number of raycast samples taken per companion server camera render pass for depth reconstruction")]
	public static int samplesPerRender = 3000;

	[ServerVar(Help = "(Generated) Maximum age in frames for a known collider entity entry in the companion server camera cache before it is evicted")]
	public static int entityMaxAge = 5;

	[ServerVar(Help = "(Generated) Maximum distance in metres from the companion server camera at which entity colliders are tracked for rendering")]
	public static int entityMaxDistance = 100;

	[ServerVar(Help = "(Generated) Maximum distance in metres at which player entities are included in companion server camera renders")]
	public static int playerMaxDistance = 30;

	[ServerVar(Help = "(Generated) Maximum distance in metres at which player name labels are included in companion server camera render output")]
	public static int playerNameMaxDistance = 10;

	[ServerVar(Help = "Enable developer-specific permissions for camera access (less restricted)")]
	public static bool developerPermissions = true;

	private static readonly Dictionary<NetworkableId, NetworkableId> _entityIdMap = new Dictionary<NetworkableId, NetworkableId>();

	private readonly Dictionary<int, (byte MaterialIndex, int Age)> _knownColliders = new Dictionary<int, (byte, int)>();

	private readonly Dictionary<int, BaseEntity> _colliderToEntity = new Dictionary<int, BaseEntity>();

	private double _lastRenderTimestamp;

	private float _fieldOfView;

	private int _sampleOffset;

	private int _nextSampleOffset;

	private int _sampleCount;

	private CameraRenderTask _task;

	private ulong? _cachedViewerSteamId;

	private BasePlayer _cachedViewer;

	public CameraRendererState state;

	public IRemoteControllable rc;

	public BaseEntity entity;

	public CameraRenderer()
	{
		Reset();
	}

	public void EnterPool()
	{
		Reset();
	}

	public void LeavePool()
	{
	}

	public void Reset()
	{
		_knownColliders.Clear();
		_colliderToEntity.Clear();
		_lastRenderTimestamp = 0.0;
		_fieldOfView = 0f;
		_sampleOffset = 0;
		_nextSampleOffset = 0;
		_sampleCount = 0;
		if (_task != null)
		{
			CameraRendererManager instance = SingletonComponent<CameraRendererManager>.Instance;
			if ((Object)(object)instance != (Object)null)
			{
				instance.ReturnTask(ref _task);
			}
		}
		_cachedViewerSteamId = null;
		_cachedViewer = null;
		state = CameraRendererState.Invalid;
		rc = null;
		entity = null;
	}

	public void Init(IRemoteControllable remoteControllable)
	{
		if (remoteControllable == null)
		{
			throw new ArgumentNullException("remoteControllable");
		}
		rc = remoteControllable;
		entity = remoteControllable.GetEnt();
		if ((Object)(object)entity == (Object)null || !entity.IsValid())
		{
			throw new ArgumentException("RemoteControllable's entity is null or invalid", "rc");
		}
		state = CameraRendererState.WaitingToRender;
	}

	public bool CanRender()
	{
		if (state != CameraRendererState.WaitingToRender)
		{
			return false;
		}
		if (TimeEx.realtimeSinceStartup - _lastRenderTimestamp < (double)renderInterval)
		{
			return false;
		}
		return true;
	}

	public void Render(int maxSampleCount)
	{
		CameraRendererManager instance = SingletonComponent<CameraRendererManager>.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			state = CameraRendererState.Invalid;
			return;
		}
		if (state != CameraRendererState.WaitingToRender)
		{
			throw new InvalidOperationException($"CameraRenderer cannot render in state {state}");
		}
		if (ObjectEx.IsUnityNull(rc) || !entity.IsValid())
		{
			state = CameraRendererState.Invalid;
			return;
		}
		Transform eyes = rc.GetEyes();
		if ((Object)(object)eyes == (Object)null)
		{
			state = CameraRendererState.Invalid;
			return;
		}
		if (_task != null)
		{
			Debug.LogError((object)"CameraRenderer: Trying to render but a task is already allocated?", (Object)(object)entity);
			instance.ReturnTask(ref _task);
		}
		_fieldOfView = verticalFov / Mathf.Clamp(rc.GetFovScale(), 1f, 8f);
		_sampleCount = Mathf.Clamp(samplesPerRender, 1, Mathf.Min(width * height, maxSampleCount));
		_task = instance.BorrowTask();
		_nextSampleOffset = _task.Start(width, height, _fieldOfView, nearPlane, farPlane, layerMask, eyes, _sampleCount, _sampleOffset, _knownColliders);
		state = CameraRendererState.Rendering;
	}

	public void CompleteRender()
	{
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0477: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		CameraRendererManager instance = SingletonComponent<CameraRendererManager>.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			state = CameraRendererState.Invalid;
			return;
		}
		if (state != CameraRendererState.Rendering)
		{
			throw new InvalidOperationException($"CameraRenderer cannot complete render in state {state}");
		}
		if (_task == null)
		{
			Debug.LogError((object)"CameraRenderer: Trying to complete render but no task is allocated?", (Object)(object)entity);
			state = CameraRendererState.Invalid;
		}
		else
		{
			if (((CustomYieldInstruction)_task).keepWaiting)
			{
				return;
			}
			if (ObjectEx.IsUnityNull(rc) || !entity.IsValid())
			{
				instance.ReturnTask(ref _task);
				state = CameraRendererState.Invalid;
				return;
			}
			Transform eyes = rc.GetEyes();
			if ((Object)(object)eyes == (Object)null)
			{
				instance.ReturnTask(ref _task);
				state = CameraRendererState.Invalid;
				return;
			}
			int num = _sampleCount * 4;
			byte[] array = Shared.ArrayPool.Rent(num);
			List<int> hitColliderIds = Pool.Get<List<int>>();
			List<int> foundColliderIds = Pool.Get<List<int>>();
			int count = _task.ExtractRayData(array, hitColliderIds, foundColliderIds);
			instance.ReturnTask(ref _task);
			UpdateCollidersMap(foundColliderIds);
			Pool.FreeUnmanaged<int>(ref hitColliderIds);
			Pool.FreeUnmanaged<int>(ref foundColliderIds);
			ulong num2 = rc.ControllingViewerId?.SteamId ?? 0;
			if (num2 == 0L)
			{
				_cachedViewerSteamId = null;
				_cachedViewer = null;
			}
			else if (num2 != _cachedViewerSteamId)
			{
				_cachedViewerSteamId = num2;
				_cachedViewer = BasePlayer.FindByID(num2) ?? BasePlayer.FindSleeping(num2);
			}
			float distance = (_cachedViewer.IsValid() ? Mathf.Clamp01(Vector3.Distance(((Component)_cachedViewer).transform.position, ((Component)entity).transform.position) / rc.MaxRange) : 0f);
			Vector3 position = eyes.position;
			Quaternion rotation = eyes.rotation;
			Matrix4x4 worldToLocalMatrix = eyes.worldToLocalMatrix;
			NetworkableId iD = entity.net.ID;
			_entityIdMap.Clear();
			AppBroadcast val = Pool.Get<AppBroadcast>();
			val.cameraRays = Pool.Get<AppCameraRays>();
			val.cameraRays.verticalFov = _fieldOfView;
			val.cameraRays.sampleOffset = _sampleOffset;
			val.cameraRays.rayData = new ArraySegment<byte>(array, 0, count);
			val.cameraRays.distance = distance;
			val.cameraRays.entities = Pool.Get<List<Entity>>();
			val.cameraRays.timeOfDay = (((Object)(object)TOD_Sky.Instance != (Object)null) ? TOD_Sky.Instance.LerpValue : 1f);
			val.cameraRays.cameraPosition = position;
			val.cameraRays.cameraRotation = ((Quaternion)(ref rotation)).eulerAngles * (MathF.PI / 180f);
			foreach (BaseEntity value in _colliderToEntity.Values)
			{
				if (!value.IsValid())
				{
					continue;
				}
				Vector3 position2 = ((Component)value).transform.position;
				float num3 = Vector3.Distance(position2, position);
				if (num3 > (float)entityMaxDistance)
				{
					continue;
				}
				string name = null;
				if (value is BasePlayer basePlayer)
				{
					if (num3 > (float)playerMaxDistance)
					{
						continue;
					}
					if (num3 <= (float)playerNameMaxDistance)
					{
						name = basePlayer.displayName;
					}
				}
				Entity val2 = Pool.Get<Entity>();
				val2.entityId = RandomizeEntityId(value.net.ID);
				val2.type = (EntityType)((value is TreeEntity) ? 1 : 2);
				val2.position = ((Matrix4x4)(ref worldToLocalMatrix)).MultiplyPoint3x4(position2);
				Quaternion val3 = Quaternion.Inverse(((Component)value).transform.rotation) * rotation;
				val2.rotation = ((Quaternion)(ref val3)).eulerAngles * (MathF.PI / 180f);
				val2.size = Vector3.Scale(((Bounds)(ref value.bounds)).size, ((Component)value).transform.localScale);
				val2.name = name;
				val.cameraRays.entities.Add(val2);
			}
			val.cameraRays.entities.Sort((Entity x, Entity y) => x.entityId.Value.CompareTo(y.entityId.Value));
			Server.Broadcast(new CameraTarget(iD), val);
			_sampleOffset = _nextSampleOffset;
			if (!Server.HasAnySubscribers(new CameraTarget(iD)))
			{
				state = CameraRendererState.Invalid;
				return;
			}
			_lastRenderTimestamp = TimeEx.realtimeSinceStartup;
			state = CameraRendererState.WaitingToRender;
		}
	}

	private void UpdateCollidersMap(List<int> foundColliderIds)
	{
		List<int> list = Pool.Get<List<int>>();
		foreach (int key in _knownColliders.Keys)
		{
			list.Add(key);
		}
		List<int> list2 = Pool.Get<List<int>>();
		foreach (int item2 in list)
		{
			if (_knownColliders.TryGetValue(item2, out (byte, int) value))
			{
				if (value.Item2 > entityMaxAge)
				{
					list2.Add(item2);
				}
				else
				{
					_knownColliders[item2] = (value.Item1, value.Item2 + 1);
				}
			}
		}
		Pool.FreeUnmanaged<int>(ref list);
		foreach (int item3 in list2)
		{
			_knownColliders.Remove(item3);
			_colliderToEntity.Remove(item3);
		}
		Pool.FreeUnmanaged<int>(ref list2);
		foreach (int foundColliderId in foundColliderIds)
		{
			if (_knownColliders.Count >= 512)
			{
				break;
			}
			Collider collider = CompanionServer.Cameras.CameraBurstUtil.GetCollider(foundColliderId);
			if ((Object)(object)collider == (Object)null)
			{
				continue;
			}
			byte item;
			if (collider is TerrainCollider)
			{
				item = 1;
			}
			else
			{
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(collider);
				item = GetMaterialIndex(collider.sharedMaterial, baseEntity);
				if (baseEntity is TreeEntity || baseEntity is BasePlayer)
				{
					_colliderToEntity[foundColliderId] = baseEntity;
				}
			}
			_knownColliders[foundColliderId] = (item, 0);
		}
	}

	private static NetworkableId RandomizeEntityId(NetworkableId realId)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (_entityIdMap.TryGetValue(realId, out var value))
		{
			return value;
		}
		NetworkableId val = default(NetworkableId);
		do
		{
			((NetworkableId)(ref val))._002Ector((ulong)Random.Range(0, 2500));
		}
		while (_entityIdMap.ContainsKey(val));
		_entityIdMap.Add(realId, val);
		return val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte GetMaterialIndex(PhysicsMaterial material, BaseEntity entity)
	{
		switch (AssetNameCache.GetName(material))
		{
		case "Water":
			return 2;
		case "Rock":
			return 3;
		case "Stones":
			return 4;
		case "Wood":
			return 5;
		case "Metal":
			return 6;
		default:
			if ((Object)(object)entity != (Object)null && entity is BasePlayer)
			{
				return 7;
			}
			return 0;
		}
	}
}

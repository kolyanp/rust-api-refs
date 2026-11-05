using System;
using System.Collections.Generic;
using Facepunch;
using Rust;
using Rust.Ai.Gen2;
using UnityEngine;

public class NPCMountable : EntityComponent<BaseEntity>
{
	[Serializable]
	public struct NPCMountAnchor
	{
		[Tooltip("Where the NPC sits. It is parented to the vehicle and zeroed against this transform.")]
		public Transform anchor;

		[Tooltip("Optional ground point on the world navmesh the NPC walks to before mounting. The dismount positions are offered too, and the clear one nearest the NPC wins.")]
		public Transform approach;

		[Tooltip("How close a parented NPC has to be to this anchor to count as sitting in it. Zero uses the default.")]
		public float radius;

		[Tooltip("Only read by LivestockMountable.")]
		public LivestockAnimal.LivestockSizeFlags acceptedSizes;

		[Tooltip("Indices of anchors sharing this one's space, so only one of them can be taken at once.")]
		public int[] overlaps;
	}

	[Header("NPC Mountable")]
	[Tooltip("An NPC takes the nearest free one.")]
	public NPCMountAnchor[] anchors;

	[Tooltip("Where NPCs are put down when they get off. Same name and meaning as BaseMountable's.")]
	public Transform[] dismountPositions;

	[Tooltip("Whether a timed mount refuses to start while the host is moving. Root motion accumulates in world space, so a moving deck slides out from under the montage.")]
	public bool requireStationary = true;

	[Tooltip("How far water can rise above an anchor before its rider is put off, so a sunk vehicle does not drown what it carries.")]
	public float maxAnchorWaterDepth = 0.5f;

	private const float DefaultAnchorRadius = 0.75f;

	private const float DismountPointRadius = 0.5f;

	private const float DismountNavmeshSampleRadius = 2f;

	private EntityRef<BaseNPC2>[] occupants;

	private Transform[] approaches;

	private Nullable[] dismountTowards;

	private const float RideableCheckInterval = 1f;

	private Action checkRideable;

	private const float ApproachStepHeight = 0.3f;

	public int AnchorCount
	{
		get
		{
			if (anchors == null)
			{
				return 0;
			}
			return anchors.Length;
		}
	}

	public bool IsHostStationary
	{
		get
		{
			if (Host is BaseVehicle baseVehicle)
			{
				return baseVehicle.IsStationary();
			}
			return true;
		}
	}

	private BaseEntity Host
	{
		get
		{
			if (!(baseEntity is BaseVehicleModule baseVehicleModule))
			{
				return baseEntity;
			}
			return baseVehicleModule.Vehicle;
		}
	}

	public bool IsHostRideable
	{
		get
		{
			if (!IsHostFlipped)
			{
				return !IsAnyAnchorSubmerged;
			}
			return false;
		}
	}

	private bool IsHostFlipped
	{
		get
		{
			if (baseEntity is BaseVehicle baseVehicle)
			{
				return baseVehicle.IsFlipped();
			}
			return false;
		}
	}

	private bool IsAnyAnchorSubmerged
	{
		get
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < AnchorCount; i++)
			{
				if (IsValidAnchor(i) && WaterLevel.GetWaterDepth(anchors[i].anchor.position, waves: true, volumes: true, baseEntity) > maxAnchorWaterDepth)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool AnyMounted
	{
		get
		{
			foreach (BaseEntity child in baseEntity.children)
			{
				if (child is BaseNPC2 { IsMounted: not false })
				{
					return true;
				}
			}
			return false;
		}
	}

	private Action CheckRideableAction => CheckRideable;

	private bool HasAnyOccupant
	{
		get
		{
			for (int i = 0; i < occupants.Length; i++)
			{
				if ((Object)(object)occupants[i].Get(serverside: true) != (Object)null)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool IsValidAnchor(int index)
	{
		if (index >= 0 && index < AnchorCount)
		{
			return (Object)(object)anchors[index].anchor != (Object)null;
		}
		return false;
	}

	public Transform GetAnchorTransform(int index)
	{
		if (!IsValidAnchor(index))
		{
			return null;
		}
		return anchors[index].anchor;
	}

	private float AnchorRadius(int index)
	{
		if (!(anchors[index].radius > 0f))
		{
			return 0.75f;
		}
		return anchors[index].radius;
	}

	public bool HasFreeAnchorFor(BaseNPC2 npc)
	{
		for (int i = 0; i < AnchorCount; i++)
		{
			if (IsAnchorFree(i) && CanSeat(npc, i))
			{
				return true;
			}
		}
		return false;
	}

	protected virtual bool CanSeat(BaseNPC2 npc, int index)
	{
		return true;
	}

	public bool IsAnchorFree(int index)
	{
		if (!IsValidAnchor(index) || IsAnchorTaken(index))
		{
			return false;
		}
		for (int i = 0; i < AnchorCount; i++)
		{
			if (Overlaps(index, i) && IsAnchorTaken(i))
			{
				return false;
			}
		}
		return true;
	}

	private bool Overlaps(int a, int b)
	{
		if (a != b)
		{
			if (!ListsOverlap(a, b))
			{
				return ListsOverlap(b, a);
			}
			return true;
		}
		return false;
	}

	private bool ListsOverlap(int a, int b)
	{
		if (anchors[a].overlaps != null)
		{
			return Array.IndexOf(anchors[a].overlaps, b) >= 0;
		}
		return false;
	}

	private bool IsAnchorTaken(int index)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (baseEntity.isServer)
		{
			return (Object)(object)occupants[index].Get(serverside: true) != (Object)null;
		}
		foreach (BaseEntity child in baseEntity.children)
		{
			if (child is BaseNPC2 { IsMounted: not false } baseNPC && GetNearestAnchor(((Component)baseNPC).transform.position) == index)
			{
				return true;
			}
		}
		return false;
	}

	private int GetNearestAnchor(Vector3 position)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		int result = -1;
		float num = float.MaxValue;
		for (int i = 0; i < AnchorCount; i++)
		{
			if (IsValidAnchor(i))
			{
				float num2 = Vector3.Distance(anchors[i].anchor.position, position);
				if (!(num2 > AnchorRadius(i)) && !(num2 >= num))
				{
					num = num2;
					result = i;
				}
			}
		}
		return result;
	}

	public override void InitShared()
	{
		base.InitShared();
		if (baseEntity.isServer)
		{
			occupants = new EntityRef<BaseNPC2>[AnchorCount];
			approaches = new Transform[AnchorCount];
			dismountTowards = new Nullable[AnchorCount];
		}
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		if (!baseEntity.isServer || Application.isQuitting || occupants == null)
		{
			return;
		}
		for (int i = 0; i < occupants.Length; i++)
		{
			BaseNPC2 baseNPC = occupants[i].Get(serverside: true);
			if ((Object)(object)baseNPC != (Object)null)
			{
				DismountImmediate(baseNPC);
			}
		}
	}

	private void Occupy(int index, BaseNPC2 npc)
	{
		occupants[index].Set(npc);
		if (!IsInvoking(CheckRideableAction))
		{
			InvokeRandomized(CheckRideableAction, 1f, 1f, 0.1f);
		}
	}

	private void CheckRideable()
	{
		if (!HasAnyOccupant)
		{
			CancelInvoke(CheckRideableAction);
		}
		else
		{
			if (IsHostRideable)
			{
				return;
			}
			for (int i = 0; i < occupants.Length; i++)
			{
				BaseNPC2 baseNPC = occupants[i].Get(serverside: true);
				if ((Object)(object)baseNPC != (Object)null && !baseNPC.IsDismounting)
				{
					DismountImmediate(baseNPC);
				}
			}
		}
	}

	public bool TryGetAnchorIndex(BaseNPC2 npc, out int index)
	{
		index = -1;
		if ((Object)(object)npc == (Object)null || occupants == null)
		{
			return false;
		}
		for (int i = 0; i < occupants.Length; i++)
		{
			if (!((Object)(object)occupants[i].Get(serverside: true) != (Object)(object)npc))
			{
				index = i;
				return true;
			}
		}
		return false;
	}

	public bool TryReserveAnchor(BaseNPC2 npc, out int index)
	{
		index = -1;
		if ((Object)(object)npc == (Object)null || occupants == null)
		{
			return false;
		}
		float num = float.MaxValue;
		Transform val = null;
		for (int i = 0; i < occupants.Length; i++)
		{
			if (IsAnchorFree(i) && CanSeat(npc, i) && TryPickApproach(npc, i, out var approach, out var distance) && !(distance >= num))
			{
				num = distance;
				index = i;
				val = approach;
			}
		}
		if (index < 0)
		{
			return false;
		}
		approaches[index] = val;
		Occupy(index, npc);
		return true;
	}

	public Vector3 GetApproachPosition(int index)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!IsValidAnchor(index))
		{
			return ((Component)baseEntity).transform.position;
		}
		if (!((Object)(object)approaches[index] != (Object)null))
		{
			return anchors[index].anchor.position;
		}
		return approaches[index].position;
	}

	private bool TryPickApproach(BaseNPC2 npc, int index, out Transform approach, out float distance)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		approach = null;
		distance = float.MaxValue;
		PooledList<Transform> val = Pool.Get<PooledList<Transform>>();
		try
		{
			if ((Object)(object)anchors[index].approach != (Object)null)
			{
				((List<Transform>)(object)val).Add(anchors[index].approach);
			}
			if (dismountPositions != null)
			{
				Transform[] array = dismountPositions;
				foreach (Transform val2 in array)
				{
					if ((Object)(object)val2 != (Object)null)
					{
						((List<Transform>)(object)val).Add(val2);
					}
				}
			}
			if (((List<Transform>)(object)val).Count == 0)
			{
				distance = Vector3.Distance(anchors[index].anchor.position, ((Component)npc).transform.position);
				return true;
			}
			foreach (Transform item in (List<Transform>)(object)val)
			{
				float num = Vector3.Distance(item.position, ((Component)npc).transform.position);
				if (!(num >= distance) && IsApproachClear(npc, item.position, anchors[index].anchor.position))
				{
					approach = item;
					distance = num;
				}
			}
			return (Object)(object)approach != (Object)null;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private bool IsApproachClear(BaseNPC2 npc, Vector3 approach, Vector3 anchor)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (!npc.TrySampleNavmesh(approach, 2f, out var onNavmesh))
		{
			return false;
		}
		Vector3 extents = npc.bounds.extents;
		float num = Mathf.Min(extents.x, extents.z);
		Vector3 val = Vector3.up * (num + 0.3f);
		Vector3 val2 = Vector3.up * Mathf.Max(extents.y * 2f - num, num + 0.3f);
		if (!IsBlocked(npc, onNavmesh + val, onNavmesh + val2, num) && !IsBlocked(npc, onNavmesh + val, anchor + val, num))
		{
			return !IsBlocked(npc, onNavmesh + val2, anchor + val2, num);
		}
		return false;
	}

	private bool IsBlocked(BaseNPC2 npc, Vector3 start, Vector3 end, float radius)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		PooledList<Collider> val = Pool.Get<PooledList<Collider>>();
		try
		{
			GamePhysics.OverlapCapsule(start, end, radius, (List<Collider>)(object)val, 1503731969, (QueryTriggerInteraction)1);
			foreach (Collider item in (List<Collider>)(object)val)
			{
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(item);
				if ((Object)(object)baseEntity == (Object)null)
				{
					return true;
				}
				if (baseEntity.isServer && (Object)(object)baseEntity != (Object)(object)npc && !IsPartOfHost(baseEntity))
				{
					return true;
				}
			}
			return false;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private bool IsPartOfHost(BaseEntity entity)
	{
		BaseEntity host = Host;
		BaseEntity baseEntity = entity;
		while ((Object)(object)baseEntity != (Object)null)
		{
			if ((Object)(object)baseEntity == (Object)(object)base.baseEntity || (Object)(object)baseEntity == (Object)(object)host)
			{
				return true;
			}
			if ((Object)(object)host != (Object)null && baseEntity is BaseVehicleModule baseVehicleModule && (Object)(object)baseVehicleModule.Vehicle == (Object)(object)host)
			{
				return true;
			}
			baseEntity = baseEntity.GetParentEntity();
		}
		return false;
	}

	public void ReleaseAnchor(BaseNPC2 npc)
	{
		if (TryGetAnchorIndex(npc, out var index))
		{
			occupants[index] = default;
			dismountTowards[index] = null;
		}
	}

	public void MountImmediate(BaseNPC2 npc, int index)
	{
		if (!((Object)(object)npc == (Object)null) && IsValidAnchor(index))
		{
			Occupy(index, npc);
			npc.OnMountFinalized(baseEntity, index);
			npc.ResumeMountedState();
		}
	}

	public bool RequestMount(BaseNPC2 npc)
	{
		bool noClearApproach;
		return RequestMount(npc, out noClearApproach);
	}

	public bool RequestMount(BaseNPC2 npc, out bool noClearApproach)
	{
		noClearApproach = false;
		if ((Object)(object)npc == (Object)null || npc.IsDead() || (Object)(object)npc.Mount != (Object)null)
		{
			return false;
		}
		if (requireStationary && !IsHostStationary)
		{
			return false;
		}
		if (!IsHostRideable)
		{
			return false;
		}
		if (!TryReserveAnchor(npc, out var index))
		{
			noClearApproach = HasFreeAnchorFor(npc);
			return false;
		}
		if (npc.StartMountTo(this, index))
		{
			return true;
		}
		occupants[index] = default;
		return false;
	}

	public bool RequestDismount(BaseNPC2 npc, Nullable towards = null)
	{
		if ((Object)(object)npc == (Object)null || !TryGetAnchorIndex(npc, out var index))
		{
			return false;
		}
		dismountTowards[index] = towards;
		if (npc.IsMounted)
		{
			if (!TryGetDismountPosition(npc, out var _, out var _))
			{
				return false;
			}
			if (npc.StartDismount())
			{
				return true;
			}
		}
		DismountImmediate(npc);
		return true;
	}

	public bool TryDismountOne(Nullable towards = null)
	{
		bool noDismountPoint;
		return TryDismountOne(towards, out noDismountPoint);
	}

	public bool TryDismountOne(Nullable towards, out bool noDismountPoint)
	{
		noDismountPoint = false;
		if (occupants == null)
		{
			return false;
		}
		for (int i = 0; i < occupants.Length; i++)
		{
			BaseNPC2 baseNPC = occupants[i].Get(serverside: true);
			if ((Object)(object)baseNPC == (Object)null)
			{
				occupants[i] = default;
				dismountTowards[i] = null;
			}
			else if (!baseNPC.IsDismounting)
			{
				if (RequestDismount(baseNPC, towards))
				{
					return true;
				}
				noDismountPoint = true;
			}
		}
		return false;
	}

	public void DismountImmediate(BaseNPC2 npc)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)npc == (Object)null))
		{
			if (!npc.IsMounted)
			{
				npc.CancelMount();
				return;
			}
			TryGetDismountPosition(npc, out var position, out var rotation);
			npc.OnDismountFinalized(position, rotation);
		}
	}

	public bool TryGetDismountPosition(BaseNPC2 npc, out Vector3 position, out Quaternion rotation)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		bool flag = false;
		position = default;
		rotation = default;
		if (dismountPositions != null)
		{
			Vector3 val = GetDismountTowards(npc);
			Transform[] array = dismountPositions;
			foreach (Transform val2 in array)
			{
				if (!((Object)(object)val2 == (Object)null) && !IsDismountPointTaken(val2, npc))
				{
					float num2 = Vector3.Distance(val2.position, val);
					if (!(num2 >= num) && npc.TrySampleNavmesh(val2.position, 2f, out var onNavmesh))
					{
						num = num2;
						position = onNavmesh;
						rotation = Upright(val2.rotation);
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			return true;
		}
		if (TryGetAnchorIndex(npc, out var index) && IsValidAnchor(index) && (Object)(object)anchors[index].approach != (Object)null && npc.TrySampleNavmesh(anchors[index].approach.position, 2f, out position))
		{
			rotation = Upright(anchors[index].approach.rotation);
			return true;
		}
		Transform transform = ((Component)baseEntity).transform;
		rotation = Upright(Quaternion.LookRotation(-transform.forward));
		position = transform.position - transform.forward * (baseEntity.bounds.extents.z + 1f);
		if (npc.TrySampleNavmesh(position, 2f, out var onNavmesh2))
		{
			position = onNavmesh2;
			return true;
		}
		return false;
	}

	private Vector3 GetDismountTowards(BaseNPC2 npc)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetAnchorIndex(npc, out var index) || !dismountTowards[index].HasValue)
		{
			return ((Component)npc).transform.position;
		}
		return dismountTowards[index].Value;
	}

	private static Quaternion Upright(Quaternion rotation)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.ProjectOnPlane(rotation * Vector3.forward, Vector3.up);
		if (val.sqrMagnitude < 0.001f)
		{
			val = Vector3.ProjectOnPlane(rotation * Vector3.up, Vector3.up);
		}
		if (!(val.sqrMagnitude < 0.001f))
		{
			return Quaternion.LookRotation(val);
		}
		return Quaternion.identity;
	}

	private bool IsDismountPointTaken(Transform point, BaseNPC2 asking)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		EntityRef<BaseNPC2>[] array = occupants;
		foreach (EntityRef<BaseNPC2> entityRef in array)
		{
			BaseNPC2 baseNPC = entityRef.Get(serverside: true);
			if (!((Object)(object)baseNPC == (Object)null) && !((Object)(object)baseNPC == (Object)(object)asking) && Vector3.Distance(((Component)baseNPC).transform.position, point.position) < 0.5f)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryReclaimAnchor(BaseNPC2 npc, out int index)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		index = -1;
		if ((Object)(object)npc == (Object)null || occupants == null)
		{
			return false;
		}
		float num = float.MaxValue;
		for (int i = 0; i < occupants.Length; i++)
		{
			if (IsAnchorFree(i) && CanSeat(npc, i))
			{
				float num2 = Vector3.Distance(anchors[i].anchor.position, ((Component)npc).transform.position);
				if (!(num2 > AnchorRadius(i)) && !(num2 >= num))
				{
					num = num2;
					index = i;
				}
			}
		}
		if (index < 0)
		{
			return false;
		}
		Occupy(index, npc);
		return true;
	}

	public bool HandOverAnchor(BaseNPC2 from, BaseNPC2 to)
	{
		if ((Object)(object)to == (Object)null || !TryGetAnchorIndex(from, out var index))
		{
			return false;
		}
		occupants[index].Set(to);
		return true;
	}
}

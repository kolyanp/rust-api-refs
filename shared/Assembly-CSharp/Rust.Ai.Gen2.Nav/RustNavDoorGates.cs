using System;
using System.Collections.Generic;
using System.Diagnostics;
using ConVar;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2.Nav;

public static class RustNavDoorGates
{
	private sealed class DoorEntry
	{
		public Vector3 gridPosition;

		public Matrix4x4 boundsMatrix;

		public Bounds writeBounds;

		public bool boundsCached;

		public bool hasWriteBounds;

		public List<Door> owners;

		public bool ownersValid;

		public List<ulong> apertureRefs;

		public bool apertureRefsValid;

		public BakeCache bakeHiRes;

		public BakeCache bakeLoRes;
	}

	private struct BakeCache
	{
		public NavMeshBuildVolume[] volumes;

		public Matrix4x4 matrix;

		public float agentRadius;

		public int leafCount;

		public int count;

		public bool cached;
	}

	public static class Profile
	{
		public static bool enabled;

		public static long gatherTicks;

		public static long writeTicks;

		public static int writes;

		public static int groupDoors;

		public static double GatherMs => (double)gatherTicks * 1000.0 / (double)Stopwatch.Frequency;

		public static double WriteMs => (double)writeTicks * 1000.0 / (double)Stopwatch.Frequency;

		public static void Reset()
		{
			gatherTicks = 0L;
			writeTicks = 0L;
			writes = 0;
			groupDoors = 0;
		}
	}

	public struct BakeVolumes(NavMeshBuildVolume[] volumes, int leafCount, int count)
	{
		private readonly NavMeshBuildVolume[] volumes = volumes;

		private readonly int leafCount = leafCount;

		private readonly int count = count;

		public int Count => count;

		public int CopyLeafVolumes(NavMeshBuildVolume[] dest, int destIndex)
		{
			for (int i = 0; i < leafCount; i++)
			{
				dest[destIndex + i] = volumes[i];
			}
			return leafCount;
		}

		public int CopyApertureVolume(NavMeshBuildVolume[] dest, int destIndex)
		{
			if (count <= leafCount)
			{
				return 0;
			}
			dest[destIndex] = volumes[leafCount];
			return 1;
		}
	}

	private static readonly Dictionary<Door, DoorEntry> allDoors = new Dictionary<Door, DoorEntry>();

	private static readonly SparseGrid<Door> doorGrid = new SparseGrid<Door>();

	public const float DoorReachMargin = 5f;

	private const int MaxOwnerPositions = 128;

	private static readonly Vector3[] ownerScratch = new Vector3[128];

	private const int MaxCachedPolyRefs = 64;

	private static readonly ulong[] refScratch = new ulong[64];

	private const int OwnersDeferred = -2;

	public static bool polyRefCacheEnabled = true;

	private static bool warnedOwnerOverflow;

	private const float DoorInteractPad = 1f;

	public static bool gatesEnabled = true;

	public static bool leafSlabsEnabled = true;

	public static float apertureHalfThickness = 0.5f;

	public static float apertureStepOver = 0.5f;

	public static bool ownershipEnabled = true;

	private static readonly HashSet<Door> pendingReassertSet = new HashSet<Door>();

	private static readonly List<Door> pendingReasserts = new List<Door>();

	private static readonly HashSet<Door> saveDeferred = new HashSet<Door>();

	public static bool HasGateDoors
	{
		get
		{
			if (gatesEnabled)
			{
				return allDoors.Count > 0;
			}
			return false;
		}
	}

	public static bool HasSaveDeferredDoors => saveDeferred.Count > 0;

	public static void InvalidateCaches()
	{
		foreach (KeyValuePair<Door, DoorEntry> allDoor in allDoors)
		{
			DoorEntry value = allDoor.Value;
			value.boundsCached = false;
			value.bakeHiRes.cached = false;
			value.bakeLoRes.cached = false;
			value.ownersValid = false;
			value.apertureRefsValid = false;
		}
	}

	public static void ForgetPolyRefs()
	{
		foreach (KeyValuePair<Door, DoorEntry> allDoor in allDoors)
		{
			allDoor.Value.apertureRefsValid = false;
		}
	}

	public static void Clear()
	{
		foreach (DoorEntry value in allDoors.Values)
		{
			List<Door> owners = value.owners;
			if (owners != null)
			{
				Pool.FreeUnmanaged<Door>(ref owners);
			}
		}
		allDoors.Clear();
		doorGrid.Clear();
		pendingReassertSet.Clear();
		pendingReasserts.Clear();
		saveDeferred.Clear();
		Door.ClearNavGateCache();
	}

	public static void Register(Door door)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!AI.useUnityNavmesh && gatesEnabled && !((Object)(object)door == (Object)null) && door.IsNavGate && !PlayerBoat.IsPartOfPlayerBoat(door))
		{
			Vector3 position = ((Component)door).transform.position;
			if (allDoors.TryAdd(door, new DoorEntry
			{
				gridPosition = position
			}))
			{
				doorGrid.Add(position, door);
				AddToNeighbourLists(door);
				Apply(door, topologyChanged: true);
			}
		}
	}

	public static void Unregister(Door door)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)door == (Object)null) && allDoors.Remove(door, out var value))
		{
			doorGrid.Remove(value.gridPosition, door);
			RemoveFromNeighbourLists(door, value);
			if (value.owners != null)
			{
				Pool.FreeUnmanaged<Door>(ref value.owners);
			}
			if (value.apertureRefs != null)
			{
				Pool.FreeUnmanaged<ulong>(ref value.apertureRefs);
			}
		}
	}

	public static void OnDoorToggled(Door door)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh || !gatesEnabled || (Object)(object)door == (Object)null || !allDoors.TryGetValue(door, out var value))
		{
			return;
		}
		if (PlayerBoat.IsPartOfPlayerBoat(door))
		{
			Unregister(door);
			return;
		}
		Vector3 position = ((Component)door).transform.position;
		Vector3 val = position - value.gridPosition;
		if (val.sqrMagnitude > 1f)
		{
			doorGrid.Remove(value.gridPosition, door);
			RemoveFromNeighbourLists(door, value);
			doorGrid.Add(position, door);
			value.gridPosition = position;
			value.ownersValid = false;
			value.apertureRefsValid = false;
			AddToNeighbourLists(door);
		}
		Apply(door);
	}

	public static void CollectGateDoorsReaching(Bounds worldBounds, HashSet<Door> results)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (!gatesEnabled || allDoors.Count == 0)
		{
			return;
		}
		worldBounds.Expand(10f);
		PooledList<Door> val = Pool.Get<PooledList<Door>>();
		try
		{
			doorGrid.GetInBounds(worldBounds, (List<Door>)(object)val);
			foreach (Door item in (List<Door>)(object)val)
			{
				if (!((Object)(object)item == (Object)null) && !item.IsDestroyed && DoorReachTouches(item, in worldBounds) && TryGetDoorWriteBounds(item, out var worldBounds2) && worldBounds2.Intersects(worldBounds) && !PlayerBoat.IsPartOfPlayerBoat(item))
				{
					results.Add(item);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static BakeVolumes GetBakeVolumes(Door door, bool hiRes, float agentRadius)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 localToWorld = ((Component)door).transform.localToWorldMatrix;
		if (!allDoors.TryGetValue(door, out var value))
		{
			NavMeshBuildVolume[] buffer = null;
			int count = BuildBakeVolumes(door, in localToWorld, agentRadius, ref buffer, out var leafCount);
			return new BakeVolumes(buffer, leafCount, count);
		}
		ref BakeCache reference = ref hiRes ? ref value.bakeHiRes : ref value.bakeLoRes;
		if (!reference.cached || reference.matrix != localToWorld || reference.agentRadius != agentRadius)
		{
			reference.count = BuildBakeVolumes(door, in localToWorld, agentRadius, ref reference.volumes, out reference.leafCount);
			reference.matrix = localToWorld;
			reference.agentRadius = agentRadius;
			reference.cached = true;
		}
		return new BakeVolumes(reference.volumes, reference.leafCount, reference.count);
	}

	private static int BuildBakeVolumes(Door door, in Matrix4x4 localToWorld, float agentRadius, ref NavMeshBuildVolume[] buffer, out int leafCount)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		leafCount = 0;
		PooledList<Bounds> val = Pool.Get<PooledList<Bounds>>();
		try
		{
			door.GetNavLeafRestSlabs((List<Bounds>)(object)val);
			bool flag = door.TryGetNavDoorwayBakeVolume(agentRadius, out var bakeLocal);
			int num = ((List<Bounds>)(object)val).Count + (flag ? 1 : 0);
			if (num == 0)
			{
				return 0;
			}
			if (buffer == null || buffer.Length < num)
			{
				buffer = new NavMeshBuildVolume[num];
			}
			for (int i = 0; i < ((List<Bounds>)(object)val).Count; i++)
			{
				BuildDoorVolume(((List<Bounds>)(object)val)[i], in localToWorld, 6, out buffer[i]);
			}
			leafCount = ((List<Bounds>)(object)val).Count;
			if (flag)
			{
				BuildDoorVolume(in bakeLocal, in localToWorld, 3, out buffer[leafCount]);
			}
			return num;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private unsafe static void BuildDoorVolume(in Bounds apertureLocal, in Matrix4x4 localToWorld, int area, out NavMeshBuildVolume volume)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		volume = default;
		Span<Vector2> span = stackalloc Vector2[8];
		float num = float.MaxValue;
		float num2 = float.MinValue;
		Vector3 extents = apertureLocal.extents;
		for (int i = 0; i < 8; i++)
		{
			Vector3 val = new Vector3(((i & 1) != 0) ? extents.x : (0f - extents.x), ((i & 2) != 0) ? extents.y : (0f - extents.y), ((i & 4) != 0) ? extents.z : (0f - extents.z));
			Vector3 val2 = localToWorld.MultiplyPoint3x4(apertureLocal.center + val);
			span[i] = new Vector2(val2.x, val2.z);
			num = Mathf.Min(num, val2.y);
			num2 = Mathf.Max(num2, val2.y);
		}
		for (int j = 1; j < 8; j++)
		{
			Vector2 val3 = span[j];
			int num3 = j - 1;
			while (num3 >= 0 && (span[num3].x > val3.x || (span[num3].x == val3.x && span[num3].y > val3.y)))
			{
				span[num3 + 1] = span[num3];
				num3--;
			}
			span[num3 + 1] = val3;
		}
		Span<Vector2> span2 = stackalloc Vector2[16];
		int num4 = 0;
		for (int k = 0; k < 8; k++)
		{
			while (num4 >= 2 && Cross(span2[num4 - 2], span2[num4 - 1], span[k]) <= 0f)
			{
				num4--;
			}
			span2[num4++] = span[k];
		}
		int num5 = num4 + 1;
		for (int num6 = 6; num6 >= 0; num6--)
		{
			while (num4 >= num5 && Cross(span2[num4 - 2], span2[num4 - 1], span[num6]) <= 0f)
			{
				num4--;
			}
			span2[num4++] = span[num6];
		}
		num4--;
		volume.nverts = Mathf.Min(num4, 8);
		for (int l = 0; l < volume.nverts; l++)
		{
			volume.verts[l * 3] = span2[l].x;
			volume.verts[l * 3 + 1] = 0f;
			volume.verts[l * 3 + 2] = span2[l].y;
		}
		volume.hmin = num;
		volume.hmax = num2;
		volume.area = area;
		static float Cross(Vector2 o, Vector2 a, Vector2 b)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
		}
	}

	public static void ReassertInBounds(Bounds worldBounds)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh || !gatesEnabled || allDoors.Count == 0)
		{
			return;
		}
		worldBounds.Expand(10f);
		PooledList<Door> val = Pool.Get<PooledList<Door>>();
		try
		{
			doorGrid.GetInBounds(worldBounds, (List<Door>)(object)val);
			PooledList<Door> val2 = Pool.Get<PooledList<Door>>();
			try
			{
				foreach (Door item in (List<Door>)(object)val)
				{
					if (!((Object)(object)item == (Object)null) && !item.IsDestroyed && DoorReachTouches(item, in worldBounds) && TryGetDoorWriteBounds(item, out var worldBounds2) && worldBounds2.Intersects(worldBounds))
					{
						((List<Door>)(object)val2).Add(item);
					}
				}
				ApplyDoors((List<Door>)(object)val2);
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

	public static void OnTileBuilt(RustNavmesh navmesh, int tx, int ty)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (!gatesEnabled || allDoors.Count == 0)
		{
			return;
		}
		Bounds region = navmesh.rcCalcTileBounds(new Vector2Int(tx, ty));
		region.Expand(10f);
		PooledList<Door> val = Pool.Get<PooledList<Door>>();
		try
		{
			doorGrid.GetInBounds(region, (List<Door>)(object)val);
			PooledList<Door> val2 = Pool.Get<PooledList<Door>>();
			try
			{
				foreach (Door item in (List<Door>)(object)val)
				{
					if (!((Object)(object)item == (Object)null) && !item.IsDestroyed && DoorReachTouches(item, in region) && TryGetDoorWriteBounds(item, out var worldBounds) && worldBounds.Intersects(region))
					{
						if (allDoors.TryGetValue(item, out var value))
						{
							value.apertureRefsValid = false;
						}
						bool flag = ApertureState(item) == DoorPolyState.Blocked;
						bool flag2 = !item.HasNavSwingLeaf || !item.IsOpen();
						if (!(flag & flag2) && pendingReassertSet.Add(item))
						{
							pendingReasserts.Add(item);
						}
					}
				}
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

	public static void FlushTileReasserts(bool complete = true)
	{
		if (pendingReasserts.Count == 0)
		{
			return;
		}
		int num = pendingReasserts.Count;
		if (!complete && RustNav.doorReassertsPerTick > 0)
		{
			num = Mathf.Min(num, RustNav.doorReassertsPerTick);
		}
		PooledList<Door> val = Pool.Get<PooledList<Door>>();
		try
		{
			for (int i = 0; i < num; i++)
			{
				Door item = pendingReasserts[i];
				((List<Door>)(object)val).Add(item);
				pendingReassertSet.Remove(item);
			}
			pendingReasserts.RemoveRange(0, num);
			ApplyDoors((List<Door>)(object)val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void FlushSaveDeferredDoors()
	{
		if (saveDeferred.Count == 0)
		{
			return;
		}
		PooledList<Door> val = Pool.Get<PooledList<Door>>();
		try
		{
			foreach (Door item in saveDeferred)
			{
				((List<Door>)(object)val).Add(item);
			}
			saveDeferred.Clear();
			ApplyDoors((List<Door>)(object)val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void ReassertAll()
	{
		if (AI.useUnityNavmesh || !gatesEnabled)
		{
			return;
		}
		pendingReasserts.Clear();
		pendingReassertSet.Clear();
		saveDeferred.Clear();
		PooledList<Door> val = Pool.Get<PooledList<Door>>();
		try
		{
			foreach (KeyValuePair<Door, DoorEntry> allDoor in allDoors)
			{
				Door key = allDoor.Key;
				if (!((Object)(object)key == (Object)null) && !key.IsDestroyed)
				{
					((List<Door>)(object)val).Add(key);
				}
			}
			ApplyDoors((List<Door>)(object)val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static DoorPolyState ApertureState(Door door)
	{
		if (door.HasFlag(BaseEntity.Flags.Open))
		{
			return DoorPolyState.Open;
		}
		if (!door.IsNpcOpenable)
		{
			return DoorPolyState.Blocked;
		}
		return DoorPolyState.NpcOpenable;
	}

	private static void Apply(Door door, bool topologyChanged = false)
	{
		if (Application.isLoadingSave)
		{
			return;
		}
		PooledList<Door> val = Pool.Get<PooledList<Door>>();
		try
		{
			((List<Door>)(object)val).Add(door);
			ApplyDoors((List<Door>)(object)val, topologyChanged);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static void ApplyDoors(List<Door> seeds, bool applyNeighbours = false)
	{
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		if (seeds.Count == 0)
		{
			return;
		}
		RustNavigation instance = RustNavigation.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		long num = (Profile.enabled ? Stopwatch.GetTimestamp() : 0);
		PooledList<Door> val = Pool.Get<PooledList<Door>>();
		try
		{
			PooledHashSet<Door> val2 = Pool.Get<PooledHashSet<Door>>();
			try
			{
				foreach (Door seed in seeds)
				{
					if (!((Object)(object)seed == (Object)null) && !seed.IsDestroyed && ((HashSet<Door>)(object)val2).Add(seed))
					{
						((List<Door>)(object)val).Add(seed);
					}
				}
				int count = ((List<Door>)(object)val).Count;
				if (count == 0)
				{
					return;
				}
				if (applyNeighbours)
				{
					for (int i = 0; i < count; i++)
					{
						List<Door> owners = GetOwners(((List<Door>)(object)val)[i]);
						if (owners == null)
						{
							continue;
						}
						for (int j = 0; j < owners.Count; j++)
						{
							Door item = owners[j];
							if (((HashSet<Door>)(object)val2).Add(item))
							{
								((List<Door>)(object)val).Add(item);
							}
						}
					}
				}
				if (Profile.enabled)
				{
					Profile.gatherTicks += Stopwatch.GetTimestamp() - num;
					Profile.groupDoors += ((List<Door>)(object)val).Count;
				}
				RustNavmesh defaultNavmesh = instance.DefaultNavmesh;
				if (defaultNavmesh != null && defaultNavmesh.IsValid())
				{
					if (defaultNavmesh.IsSaveInFlight)
					{
						for (int k = 0; k < ((List<Door>)(object)val).Count; k++)
						{
							saveDeferred.Add(((List<Door>)(object)val)[k]);
						}
					}
					else
					{
						ApplyGroupToNavmesh(defaultNavmesh, null, (List<Door>)(object)val);
					}
				}
				if (!IndependantNavmesh.AnyRegistered)
				{
					return;
				}
				PooledList<IndependantNavmesh> val3 = Pool.Get<PooledList<IndependantNavmesh>>();
				try
				{
					foreach (Door item2 in (List<Door>)(object)val)
					{
						IndependantNavmesh independantNavmesh = null;
						if ((Object)(object)item2 != (Object)null && !item2.IsDestroyed)
						{
							independantNavmesh = IndependantNavmesh.FindNavmeshAtPosition(((Component)item2).transform.position);
						}
						if ((Object)(object)independantNavmesh == (Object)null || independantNavmesh.Navmesh == null || !independantNavmesh.Navmesh.IsValid())
						{
							independantNavmesh = null;
						}
						((List<IndependantNavmesh>)(object)val3).Add(independantNavmesh);
					}
					PooledList<Door> val4 = Pool.Get<PooledList<Door>>();
					try
					{
						PooledHashSet<IndependantNavmesh> val5 = Pool.Get<PooledHashSet<IndependantNavmesh>>();
						try
						{
							for (int l = 0; l < ((List<Door>)(object)val).Count; l++)
							{
								IndependantNavmesh independantNavmesh2 = ((List<IndependantNavmesh>)(object)val3)[l];
								if ((Object)(object)independantNavmesh2 == (Object)null || !((HashSet<IndependantNavmesh>)(object)val5).Add(independantNavmesh2))
								{
									continue;
								}
								((List<Door>)(object)val4).Clear();
								for (int m = 0; m < ((List<Door>)(object)val).Count; m++)
								{
									if (((List<IndependantNavmesh>)(object)val3)[m] == independantNavmesh2)
									{
										((List<Door>)(object)val4).Add(((List<Door>)(object)val)[m]);
									}
								}
								ApplyGroupToNavmesh(independantNavmesh2.Navmesh, independantNavmesh2, (List<Door>)(object)val4);
							}
						}
						finally
						{
							((IDisposable)val5)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
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

	private static void ApplyGroupToNavmesh(RustNavmesh navmesh, IndependantNavmesh independent, List<Door> doors)
	{
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		long num = (Profile.enabled ? Stopwatch.GetTimestamp() : 0);
		PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
		try
		{
			PooledList<int> val2 = Pool.Get<PooledList<int>>();
			try
			{
				PooledList<int> val3 = Pool.Get<PooledList<int>>();
				try
				{
					for (int i = 0; i < doors.Count; i++)
					{
						((List<int>)(object)val3).Add(((List<Vector3>)(object)val).Count);
						((List<int>)(object)val2).Add(WillWriteFromRefCache(doors[i], independent) ? (-2) : GatherOwnerPositions(doors[i], independent, (List<Vector3>)(object)val));
					}
					if (Profile.enabled)
					{
						Profile.gatherTicks += Stopwatch.GetTimestamp() - num;
					}
					for (int j = 0; j < doors.Count; j++)
					{
						Door door = doors[j];
						if (!((Object)(object)door == (Object)null) && !door.IsDestroyed && door.TryGetNavLeafRegion(out var leafRegionLocal, out var localToWorld))
						{
							SetDoorObbFlags(navmesh, independent, in leafRegionLocal, in localToWorld, door, DoorPolyState.Open, 6, (List<Vector3>)(object)val, ((List<int>)(object)val3)[j], ((List<int>)(object)val2)[j]);
						}
					}
					for (DoorPolyState doorPolyState = DoorPolyState.Open; doorPolyState <= DoorPolyState.Blocked; doorPolyState++)
					{
						for (int k = 0; k < doors.Count; k++)
						{
							Door door2 = doors[k];
							if (!((Object)(object)door2 == (Object)null) && !door2.IsDestroyed && ApertureState(door2) == doorPolyState && door2.TryGetNavDoorwayVolume(out var apertureLocal, out var localToWorld2))
							{
								SetDoorObbFlags(navmesh, independent, in apertureLocal, in localToWorld2, door2, doorPolyState, 3, (List<Vector3>)(object)val, ((List<int>)(object)val3)[k], ((List<int>)(object)val2)[k]);
							}
						}
					}
					PooledList<Bounds> val4 = Pool.Get<PooledList<Bounds>>();
					try
					{
						for (int l = 0; l < doors.Count; l++)
						{
							Door door3 = doors[l];
							if ((Object)(object)door3 == (Object)null || door3.IsDestroyed || !door3.HasNavSwingLeaf || !door3.IsOpen())
							{
								continue;
							}
							Matrix4x4 localToWorld3 = ((Component)door3).transform.localToWorldMatrix;
							((List<Bounds>)(object)val4).Clear();
							door3.GetNavLeafActiveSlabs((List<Bounds>)(object)val4);
							foreach (Bounds item in (List<Bounds>)(object)val4)
							{
								SetDoorObbFlags(navmesh, independent, item, in localToWorld3, door3, DoorPolyState.Blocked, 6, (List<Vector3>)(object)val, ((List<int>)(object)val3)[l], ((List<int>)(object)val2)[l]);
							}
						}
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
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

	private static bool WillWriteFromRefCache(Door door, IndependantNavmesh independent)
	{
		if (!polyRefCacheEnabled || (Object)(object)independent != (Object)null)
		{
			return false;
		}
		if ((Object)(object)door == (Object)null || door.IsDestroyed || door.HasNavSwingLeaf)
		{
			return false;
		}
		if (allDoors.TryGetValue(door, out var value))
		{
			return value.apertureRefsValid;
		}
		return false;
	}

	private static int GatherOwnerPositions(Door door, IndependantNavmesh independent, List<Vector3> positions)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)door == (Object)null || door.IsDestroyed)
		{
			return -1;
		}
		if (!allDoors.ContainsKey(door))
		{
			return -1;
		}
		if (!ownershipEnabled)
		{
			return 0;
		}
		List<Door> owners = GetOwners(door);
		if (owners == null)
		{
			return -1;
		}
		int num = 0;
		for (int i = 0; i < owners.Count; i++)
		{
			Door door2 = owners[i];
			if ((Object)(object)door2 == (Object)null || door2.IsDestroyed)
			{
				continue;
			}
			Vector3 position = ((Component)door2).transform.position;
			if ((Object)(object)independent != (Object)null && (Object)(object)IndependantNavmesh.FindNavmeshAtPosition(position) != (Object)(object)independent)
			{
				continue;
			}
			if (num == 128)
			{
				if (!warnedOwnerOverflow)
				{
					warnedOwnerOverflow = true;
					Debug.LogWarning((object)$"RustNavDoorGates: door at {((Component)door).transform.position} has more than {128} owner candidates, ownership may misassign");
				}
				break;
			}
			positions.Add(((Object)(object)independent != (Object)null) ? independent.TransformPointFromWorldSpaceToNavSpace(position).Value : position);
			num++;
		}
		return num;
	}

	private static void SetDoorObbFlags(RustNavmesh navmesh, IndependantNavmesh independent, in Bounds localBounds, in Matrix4x4 localToWorld, Door door, DoorPolyState state, int area, List<Vector3> ownerPositions, int ownerStart, int ownerCount)
	{
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		if (ownerCount < 0 && ownerCount != -2)
		{
			return;
		}
		Transform transform = ((Component)door).transform;
		DoorEntry value = null;
		if (polyRefCacheEnabled && (Object)(object)independent == (Object)null && area == 3)
		{
			allDoors.TryGetValue(door, out value);
		}
		if (value != null && value.apertureRefsValid)
		{
			int count = value.apertureRefs.Count;
			if (count == 0)
			{
				return;
			}
			long num = (Profile.enabled ? Stopwatch.GetTimestamp() : 0);
			for (int i = 0; i < count; i++)
			{
				refScratch[i] = value.apertureRefs[i];
			}
			bool flag = navmesh.SetDoorPolyFlagsForRefs(refScratch, count, state, NavCenter(independent, localToWorld.MultiplyPoint3x4(localBounds.center)), DoorHalfExtents(in localBounds, transform), DoorAxisXZ(transform, independent));
			if (Profile.enabled)
			{
				Profile.writeTicks += Stopwatch.GetTimestamp() - num;
				Profile.writes++;
			}
			if (flag)
			{
				return;
			}
			value.apertureRefsValid = false;
		}
		if (ownerCount == -2)
		{
			PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
			try
			{
				ownerCount = GatherOwnerPositions(door, independent, (List<Vector3>)(object)val);
				if (ownerCount < 0)
				{
					return;
				}
				for (int j = 0; j < ownerCount; j++)
				{
					ownerScratch[j] = ((List<Vector3>)(object)val)[j];
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		else
		{
			for (int k = 0; k < ownerCount; k++)
			{
				ownerScratch[k] = ownerPositions[ownerStart + k];
			}
		}
		Vector3 worldCenter = localToWorld.MultiplyPoint3x4(localBounds.center);
		Vector3 val2 = transform.right;
		NavVector3 center = NavCenter(independent, worldCenter);
		if ((Object)(object)independent != (Object)null)
		{
			val2 = independent.TransformDirectionFromWorldSpaceToNavSpace(val2).Value;
		}
		Vector2 axisXZ = new Vector2(val2.x, val2.z);
		if (axisXZ.sqrMagnitude < 0.0001f)
		{
			axisXZ = Vector2.right;
		}
		axisXZ.Normalize();
		Vector3 lossyScale = transform.lossyScale;
		Vector3 halfExtents = new Vector3(localBounds.extents.x * Mathf.Abs(lossyScale.x), localBounds.extents.y * Mathf.Abs(lossyScale.y), localBounds.extents.z * Mathf.Abs(lossyScale.z)) + new Vector3(0.1f, 0.1f, 0.1f);
		ulong[] array = ((value != null) ? refScratch : null);
		long num2 = (Profile.enabled ? Stopwatch.GetTimestamp() : 0);
		navmesh.SetDoorPolyFlags(center, halfExtents, axisXZ, state, area, NavCenter(independent, transform.position), ownerScratch, ownerCount, array, (array != null) ? 64 : 0, out var outRefCount);
		if (Profile.enabled)
		{
			Profile.writeTicks += Stopwatch.GetTimestamp() - num2;
			Profile.writes++;
		}
		if (value != null && outRefCount >= 0 && outRefCount <= 64)
		{
			DoorEntry doorEntry = value;
			if (doorEntry.apertureRefs == null)
			{
				doorEntry.apertureRefs = Pool.Get<List<ulong>>();
			}
			value.apertureRefs.Clear();
			for (int l = 0; l < outRefCount; l++)
			{
				value.apertureRefs.Add(refScratch[l]);
			}
			value.apertureRefsValid = true;
		}
	}

	private static Vector3 DoorHalfExtents(in Bounds localBounds, Transform doorTransform)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Vector3 lossyScale = doorTransform.lossyScale;
		return new Vector3(localBounds.extents.x * Mathf.Abs(lossyScale.x), localBounds.extents.y * Mathf.Abs(lossyScale.y), localBounds.extents.z * Mathf.Abs(lossyScale.z)) + new Vector3(0.1f, 0.1f, 0.1f);
	}

	private static Vector2 DoorAxisXZ(Transform doorTransform, IndependantNavmesh independent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = doorTransform.right;
		if ((Object)(object)independent != (Object)null)
		{
			val = independent.TransformDirectionFromWorldSpaceToNavSpace(val).Value;
		}
		Vector2 result = new Vector2(val.x, val.z);
		if (result.sqrMagnitude < 0.0001f)
		{
			result = Vector2.right;
		}
		result.Normalize();
		return result;
	}

	private static NavVector3 NavCenter(IndependantNavmesh independent, Vector3 worldCenter)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)independent != (Object)null))
		{
			return new NavVector3(worldCenter);
		}
		return independent.TransformPointFromWorldSpaceToNavSpace(worldCenter);
	}

	private static List<Door> GetOwners(Door door)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if (!allDoors.TryGetValue(door, out var value))
		{
			return null;
		}
		if (value.ownersValid)
		{
			return value.owners;
		}
		DoorEntry doorEntry = value;
		if (doorEntry.owners == null)
		{
			doorEntry.owners = Pool.Get<List<Door>>();
		}
		value.owners.Clear();
		value.ownersValid = true;
		if (TryGetDoorWriteBounds(door, out var worldBounds))
		{
			Bounds bounds = worldBounds;
			bounds.Expand(12f);
			PooledList<Door> val = Pool.Get<PooledList<Door>>();
			try
			{
				doorGrid.GetInBounds(bounds, (List<Door>)(object)val);
				foreach (Door item in (List<Door>)(object)val)
				{
					if (!((Object)(object)item == (Object)(object)door) && !((Object)(object)item == (Object)null) && !item.IsDestroyed && DoorReachTouches(item, in worldBounds) && TryGetDoorWriteBounds(item, out var worldBounds2) && worldBounds.Intersects(worldBounds2))
					{
						value.owners.Add(item);
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		return value.owners;
	}

	private static void AddToNeighbourLists(Door door)
	{
		if (!ownershipEnabled)
		{
			return;
		}
		List<Door> owners = GetOwners(door);
		if (owners == null)
		{
			return;
		}
		for (int i = 0; i < owners.Count; i++)
		{
			Door door2 = owners[i];
			if (!((Object)(object)door2 == (Object)null) && allDoors.TryGetValue(door2, out var value) && value.ownersValid && !value.owners.Contains(door))
			{
				value.owners.Add(door);
			}
		}
	}

	private static void RemoveFromNeighbourLists(Door door, DoorEntry entry)
	{
		if (entry.owners == null)
		{
			return;
		}
		for (int i = 0; i < entry.owners.Count; i++)
		{
			Door door2 = entry.owners[i];
			if (!((Object)(object)door2 == (Object)null) && allDoors.TryGetValue(door2, out var value))
			{
				value.owners?.Remove(door);
			}
		}
	}

	private static bool DoorReachTouches(Door door, in Bounds region)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (!allDoors.TryGetValue(door, out var value))
		{
			return true;
		}
		Vector3 gridPosition = value.gridPosition;
		if (gridPosition.x >= region.min.x - 6f && gridPosition.x <= region.max.x + 6f && gridPosition.y >= region.min.y - 6f && gridPosition.y <= region.max.y + 6f && gridPosition.z >= region.min.z - 6f)
		{
			return gridPosition.z <= region.max.z + 6f;
		}
		return false;
	}

	private static bool TryGetDoorWriteBounds(Door door, out Bounds worldBounds)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (!allDoors.TryGetValue(door, out var value))
		{
			return BuildDoorWriteBounds(door, ((Component)door).transform.localToWorldMatrix, out worldBounds);
		}
		Matrix4x4 localToWorld = ((Component)door).transform.localToWorldMatrix;
		if (value.boundsCached && value.boundsMatrix == localToWorld)
		{
			worldBounds = value.writeBounds;
			return value.hasWriteBounds;
		}
		bool flag = BuildDoorWriteBounds(door, in localToWorld, out worldBounds);
		value.boundsCached = true;
		value.boundsMatrix = localToWorld;
		value.writeBounds = worldBounds;
		value.hasWriteBounds = flag;
		return flag;
	}

	private static bool BuildDoorWriteBounds(Door door, in Matrix4x4 localToWorld, out Bounds worldBounds)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!door.TryGetNavDoorwayVolume(out var apertureLocal, out var localToWorld2))
		{
			worldBounds = default;
			return false;
		}
		worldBounds = BoundsEx.Transform(apertureLocal, localToWorld);
		if (door.TryGetNavLeafRegion(out var leafRegionLocal, out localToWorld2))
		{
			worldBounds.Encapsulate(BoundsEx.Transform(leafRegionLocal, localToWorld));
		}
		worldBounds.Expand(2f);
		return true;
	}
}

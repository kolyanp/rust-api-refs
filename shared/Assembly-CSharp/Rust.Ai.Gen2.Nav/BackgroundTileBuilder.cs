using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ConVar;
using Facepunch;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2.Nav;

public class BackgroundTileBuilder : IDisposable
{
	private static class TileScratch
	{
		[ThreadStatic]
		private static RawBuffer<Vector3> _vertices;

		[ThreadStatic]
		private static RawBuffer<int> _triangles;

		[ThreadStatic]
		private static RawBuffer<int> _indices;

		[ThreadStatic]
		private static RawBuffer<byte> _triAreas;

		private static readonly List<IDisposable> _all = new List<IDisposable>();

		public static RawBuffer<Vector3> Vertices => _vertices ?? (_vertices = Track(new RawBuffer<Vector3>()));

		public static RawBuffer<int> Triangles => _triangles ?? (_triangles = Track(new RawBuffer<int>()));

		public static RawBuffer<int> Indices => _indices ?? (_indices = Track(new RawBuffer<int>()));

		public static RawBuffer<byte> TriAreas => _triAreas ?? (_triAreas = Track(new RawBuffer<byte>()));

		private static T Track<T>(T b) where T : IDisposable
		{
			lock (_all)
			{
				_all.Add(b);
				return b;
			}
		}

		public static void DisposeAll()
		{
			lock (_all)
			{
				foreach (IDisposable item in _all)
				{
					item.Dispose();
				}
				_all.Clear();
			}
			_vertices = null;
			_triangles = null;
			_indices = null;
			_triAreas = null;
		}
	}

	private sealed class TileCancellation
	{
		private volatile bool cancelled;

		public bool IsCancellationRequested => cancelled;

		public void Cancel()
		{
			cancelled = true;
		}
	}

	private struct TileCollectRequest(int tx, int ty, RustNavmesh navmesh)
	{
		public readonly int tx = tx;

		public readonly int ty = ty;

		public RustNavmesh navmesh = navmesh;

		public TileCancellation cancellation = new TileCancellation();
	}

	private struct TileBuildRequest(in TileCollectRequest collectRequest, List<ThreadSafeNavMeshBuildSource> sources, NavMeshBuildParams buildParams, NavMeshBuildVolume[] volumes, int volumeCount)
	{
		public readonly int tx = collectRequest.tx;

		public readonly int ty = collectRequest.ty;

		public RustNavmesh navmesh = collectRequest.navmesh;

		public NavMeshBuildParams buildParams = buildParams;

		public List<ThreadSafeNavMeshBuildSource> sources = sources;

		public NavMeshBuildVolume[] volumes = volumes;

		public int volumeCount = volumeCount;

		public TileCancellation cancellation = collectRequest.cancellation;
	}

	public enum TileBuildResultCode
	{
		Success,
		Cancelled,
		NoGeometry,
		UnknownError,
		ExtractGeometryError,
		SpanHeightError,
		CreateHeightFieldError,
		CreateCompactHeightFieldError,
		CreatePolymeshError,
		CreateDetailPolymeshError,
		CreateAndAddNavDataError,
		ValidationError
	}

	private struct TileBuildResult
	{
		public readonly int tx;

		public readonly int ty;

		public RustNavmesh navmesh;

		public IntPtr tileBytes;

		public readonly int dataSize;

		public TileBuildResultCode resultCode;

		public TileCancellation cancellation;

		public float debugSpanMinY;

		public float debugSpanMaxY;

		public TileBuildResult(in TileBuildRequest request, IntPtr tileBytes, int dataSize)
		{
			tx = request.tx;
			ty = request.ty;
			navmesh = request.navmesh;
			this.tileBytes = tileBytes;
			this.dataSize = dataSize;
			resultCode = TileBuildResultCode.Success;
			cancellation = request.cancellation;
			debugSpanMinY = 0f;
			debugSpanMaxY = 0f;
		}

		public TileBuildResult(in TileBuildRequest request, TileBuildResultCode resultCode)
		{
			tx = request.tx;
			ty = request.ty;
			navmesh = request.navmesh;
			tileBytes = IntPtr.Zero;
			dataSize = 0;
			this.resultCode = resultCode;
			cancellation = request.cancellation;
			debugSpanMinY = 0f;
			debugSpanMaxY = 0f;
		}
	}

	private struct ThreadSafeNavMeshBuildSource
	{
		public NavMeshBuildSourceShape shape;

		public int sourceObjectID;

		public Matrix4x4 transform;

		public Vector3 size;

		public int area;

		public bool forceUnwalkable;
	}

	private static readonly int[] boxTriangleIndices = new int[36]
	{
		7, 4, 3, 7, 6, 4, 4, 6, 5, 4,
		5, 0, 4, 5, 1, 4, 1, 0, 5, 6,
		2, 5, 2, 1, 6, 7, 3, 6, 3, 2,
		0, 1, 3, 0, 3, 7
	};

	public static (int tx, int ty, string path)? DumpGeometryRequest;

	public const int GeometryLayerMask = 1092714753;

	private Stopwatch stopwatch = new Stopwatch();

	private readonly Dictionary<(RustNavmesh navmesh, int tx, int ty), TileCancellation> tileCancellations = new Dictionary<(RustNavmesh, int, int), TileCancellation>();

	private readonly Queue<TileCollectRequest> collectMainThreadWorkQueue = new Queue<TileCollectRequest>();

	private readonly BlockingCollection<TileBuildRequest> backgroundWorkQueue = new BlockingCollection<TileBuildRequest>();

	private readonly ConcurrentBag<TileBuildResult> finalMainthreadWorkBag = new ConcurrentBag<TileBuildResult>();

	private Thread[] workers;

	private CancellationTokenSource globalInterrupt;

	private NavmeshSaveCatchUpStats gateCatchUp;

	private long gateLiftTimestamp;

	private readonly Queue<TileBuildResult> parkedResults = new Queue<TileBuildResult>();

	public NavmeshSaveCatchUpStats SaveGateCatchUp => gateCatchUp;

	public static void CreateBoxMesh(List<Vector3> vertices, List<int> triangles, Vector3 center, Vector3 size)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		vertices.Clear();
		triangles.Clear();
		vertices.Add(center + new Vector3(0f - size.x, 0f - size.y, 0f - size.z) * 0.5f);
		vertices.Add(center + new Vector3(size.x, 0f - size.y, 0f - size.z) * 0.5f);
		vertices.Add(center + new Vector3(size.x, 0f - size.y, size.z) * 0.5f);
		vertices.Add(center + new Vector3(0f - size.x, 0f - size.y, size.z) * 0.5f);
		vertices.Add(center + new Vector3(0f - size.x, size.y, 0f - size.z) * 0.5f);
		vertices.Add(center + new Vector3(size.x, size.y, 0f - size.z) * 0.5f);
		vertices.Add(center + new Vector3(size.x, size.y, size.z) * 0.5f);
		vertices.Add(center + new Vector3(0f - size.x, size.y, size.z) * 0.5f);
		for (int i = 0; i < boxTriangleIndices.Length; i++)
		{
			triangles.Add(boxTriangleIndices[i]);
		}
	}

	public unsafe static void ExtractTerrainGeometry(Vector3 topLeftCorner, int tileSize, RawBuffer<Vector3> vertices, RawBuffer<int> triangles)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		int num = tileSize + 1;
		int num2 = num * num;
		RawBuffer<int> indices = TileScratch.Indices;
		indices.Clear();
		int* ptr = indices.AppendUninitialized(num2);
		vertices.EnsureCapacity(vertices.Count + num2);
		triangles.EnsureCapacity(triangles.Count + tileSize * tileSize * 6);
		Vector3 val = new Vector3(topLeftCorner.x, 0f, topLeftCorner.z);
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		TerrainAlphaMap alphaMap = TerrainMeta.AlphaMap;
		float num3 = val.x + (float)tileSize * 0.5f;
		float num4 = val.z + (float)tileSize * 0.5f;
		bool deepSea = DeepSeaManager.IsInsideDeepSea(new Vector3(num3, 0f, num4));
		TerrainHeightMap.HeightSampler heightSampler = heightMap.CreateSampler(deepSea);
		TerrainAlphaMap.AlphaSampler alphaSampler = alphaMap.CreateSampler();
		int num5 = 0;
		for (int i = 0; i <= tileSize; i++)
		{
			float num6 = val.z + (float)i;
			heightSampler.BeginRow(num6);
			alphaSampler.BeginRow(num6);
			int num7 = 0;
			while (num7 <= tileSize)
			{
				float num8 = val.x + (float)num7;
				float num9 = heightSampler.SampleRow(num8);
				if (num9 < -1f)
				{
					ptr[num5] = -1;
				}
				else if (alphaSampler.SampleRow(num8) < 0.1f)
				{
					ptr[num5] = -1;
				}
				else
				{
					ptr[num5] = vertices.Count;
					vertices.Add(new Vector3(num8, num9, num6));
				}
				num7++;
				num5++;
			}
		}
		int num10 = 0;
		int num11 = 0;
		while (num11 < tileSize)
		{
			int num12 = 0;
			while (num12 < tileSize)
			{
				int num13 = ptr[num10];
				int num14 = ptr[num10 + tileSize + 1];
				int num15 = ptr[num10 + 1];
				int num16 = ptr[num10 + 1];
				int num17 = ptr[num10 + tileSize + 1];
				int num18 = ptr[num10 + tileSize + 2];
				if (num13 != -1 && num14 != -1 && num15 != -1)
				{
					triangles.Add(num13);
					triangles.Add(num14);
					triangles.Add(num15);
				}
				if (num16 != -1 && num17 != -1 && num18 != -1)
				{
					triangles.Add(num16);
					triangles.Add(num17);
					triangles.Add(num18);
				}
				num12++;
				num10++;
			}
			num11++;
			num10++;
		}
	}

	private unsafe static void DumpTileGeometry(string path, in NavMeshBuildParams buildParams, int tx, int ty, Vector3 hfMin, Vector3 hfMax, RawBuffer<Vector3> vertices, RawBuffer<int> triangles, RawBuffer<byte> triAreas)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		using FileStream output = new FileStream(path, FileMode.Create, FileAccess.Write);
		using BinaryWriter binaryWriter = new BinaryWriter(output);
		binaryWriter.Write(1380402511);
		binaryWriter.Write(3);
		NavMeshBuildParams navMeshBuildParams = buildParams;
		ReadOnlySpan<byte> buffer = new ReadOnlySpan<byte>(&navMeshBuildParams, sizeof(NavMeshBuildParams));
		binaryWriter.Write(buffer.Length);
		binaryWriter.Write(buffer);
		binaryWriter.Write(tx);
		binaryWriter.Write(ty);
		binaryWriter.Write(hfMin.x);
		binaryWriter.Write(hfMin.y);
		binaryWriter.Write(hfMin.z);
		binaryWriter.Write(hfMax.x);
		binaryWriter.Write(hfMax.y);
		binaryWriter.Write(hfMax.z);
		binaryWriter.Write(vertices.Count);
		binaryWriter.Write(new ReadOnlySpan<byte>((void*)vertices.Ptr, vertices.Count * 12));
		binaryWriter.Write(triangles.Count);
		binaryWriter.Write(new ReadOnlySpan<byte>((void*)triangles.Ptr, triangles.Count * 4));
		binaryWriter.Write(0);
		binaryWriter.Write(triAreas.Count);
		if (triAreas.Count > 0)
		{
			binaryWriter.Write(new ReadOnlySpan<byte>((void*)triAreas.Ptr, triAreas.Count));
		}
	}

	public BackgroundTileBuilder()
	{
		int num = Mathf.Clamp(RustNav.numThreads, 1, SystemInfo.processorCount - 1);
		globalInterrupt = new CancellationTokenSource();
		workers = new Thread[num];
		for (int i = 0; i < num; i++)
		{
			workers[i] = new Thread(WorkerLoopFromBackgroundThread)
			{
				IsBackground = true,
				Name = $"RustNavTileBuilder-{i}"
			};
			workers[i].Start();
		}
	}

	public void GetPendingTilesOnMainThread(List<(RustNavmesh navmesh, int tx, int ty)> pendingTiles)
	{
		foreach (KeyValuePair<(RustNavmesh, int, int), TileCancellation> tileCancellation in tileCancellations)
		{
			pendingTiles.Add(tileCancellation.Key);
		}
	}

	public void GetPendingTilesForNavmeshOnMainThread(RustNavmesh navmesh, List<(int tx, int ty)> pendingTiles)
	{
		foreach (KeyValuePair<(RustNavmesh, int, int), TileCancellation> tileCancellation in tileCancellations)
		{
			if (tileCancellation.Key.Item1 == navmesh)
			{
				pendingTiles.Add((tileCancellation.Key.Item2, tileCancellation.Key.Item3));
			}
		}
	}

	public void CancelPendingTilesForOnMainThread(RustNavmesh navmesh)
	{
		foreach (KeyValuePair<(RustNavmesh, int, int), TileCancellation> tileCancellation in tileCancellations)
		{
			if (tileCancellation.Key.Item1 == navmesh)
			{
				tileCancellation.Value.Cancel();
			}
		}
	}

	public void Dispose()
	{
		if (globalInterrupt == null)
		{
			return;
		}
		backgroundWorkQueue.CompleteAdding();
		try
		{
			globalInterrupt.Cancel();
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
		bool flag = true;
		Thread[] array = workers;
		foreach (Thread thread in array)
		{
			try
			{
				if (!thread.Join(1000))
				{
					flag = false;
				}
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
				flag = false;
			}
		}
		if (flag)
		{
			TileScratch.DisposeAll();
		}
		else
		{
			RustNavigation.LogError("RustNav: workers did not join in time, leaking tile scratch buffers deliberately");
		}
		CleanupRemainingWorkItemsOnMainThread();
		globalInterrupt.Dispose();
		globalInterrupt = null;
	}

	private TileBuildRequest DoInitialWorkOnMainThread(in TileCollectRequest collectRequest)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavigation.DoInitialWorkOnMainThread"))
		{
			long num = BakeStats.Timestamp();
			long num2 = num;
			Bounds tileBounds = collectRequest.navmesh.rcCalcTileBounds(new Vector2Int(collectRequest.tx, collectRequest.ty));
			tileBounds = collectRequest.navmesh.rcExpandTileBounds(tileBounds);
			BakeStats.AddStage(BakeStats.Stage.CollectBounds, BakeStats.Timestamp() - num2);
			int layerMask = 1629585665;
			int areaFromName = NavMesh.GetAreaFromName("Walkable");
			List<ThreadSafeNavMeshBuildSource> list = Pool.Get<List<ThreadSafeNavMeshBuildSource>>();
			PooledList<Collider> val = Pool.Get<PooledList<Collider>>();
			try
			{
				num2 = BakeStats.Timestamp();
				GamePhysics.OverlapBounds(tileBounds, (List<Collider>)(object)val, layerMask, (QueryTriggerInteraction)2);
				BakeStats.AddStage(BakeStats.Stage.CollectOverlap, BakeStats.Timestamp() - num2);
				bool flag = collectRequest.navmesh.ForceHiRes;
				PooledHashSet<Door> val2 = null;
				bool hasAny = RustNavmeshModifierVolume.HasAny;
				if (!flag && RustNavigation.HasTunnelRegions && (Object)(object)RustNavigation.Instance != (Object)null && RustNavigation.Instance.IsInTunnelRegion(tileBounds))
				{
					flag = true;
				}
				num2 = BakeStats.Timestamp();
				foreach (Collider item2 in (List<Collider>)(object)val)
				{
					try
					{
						BaseEntity baseEntity = GameObjectEx.ToBaseEntity(item2, allowDestroyed: true);
						if (((Object)(object)baseEntity != (Object)null && (baseEntity.isClient || baseEntity.IsDestroyed)) || PlayerBoat.IsPartOfPlayerBoat(baseEntity))
						{
							continue;
						}
						ThreadSafeNavMeshBuildSource item = new ThreadSafeNavMeshBuildSource
						{
							shape = (NavMeshBuildSourceShape)0,
							sourceObjectID = 0,
							transform = ((Component)item2).transform.localToWorldMatrix,
							size = Vector3.zero,
							area = areaFromName
						};
						if (!flag)
						{
							if (BaseNetworkableEx.Is<BuildingBlock>((Object)(object)baseEntity, out BuildingBlock _))
							{
								flag = true;
							}
							else if ((Object)(object)ConstructionErrors.GetPreventBuildingMonumentTag(item2) != (Object)null)
							{
								flag = true;
							}
						}
						if (BaseNetworkableEx.Is<TreeEntity>((Object)(object)baseEntity, out TreeEntity castedUnityObject2) && !castedUnityObject2.IncludeInNavmesh)
						{
							continue;
						}
						if (!BaseNetworkableEx.Is<Door>((Object)(object)baseEntity, out Door castedUnityObject3))
						{
							goto IL_01e9;
						}
						if (castedUnityObject3.IsNavGate)
						{
							if (val2 == null)
							{
								val2 = Pool.Get<PooledHashSet<Door>>();
							}
							((HashSet<Door>)(object)val2).Add(castedUnityObject3);
							if (!castedUnityObject3.IsNavGateMovingCollider(item2))
							{
								goto IL_01e9;
							}
						}
						else if (!castedUnityObject3.IsNpcOpenable)
						{
							goto IL_01e9;
						}
						goto end_IL_00e6;
						IL_01e9:
						if (item2.isTrigger || (0x20000000 & (1 << ((Component)item2).gameObject.layer)) != 0)
						{
							continue;
						}
						if (BaseNetworkableEx.Is<MeshCollider>((Object)(object)item2, out MeshCollider castedUnityObject4))
						{
							if ((Object)(object)castedUnityObject4.sharedMesh == (Object)null)
							{
								continue;
							}
							item.shape = (NavMeshBuildSourceShape)0;
							item.sourceObjectID = ((Object)castedUnityObject4.sharedMesh).GetInstanceID();
							MeshCache.Get(castedUnityObject4.sharedMesh);
							goto IL_03d5;
						}
						if (BaseNetworkableEx.Is<BoxCollider>((Object)(object)item2, out BoxCollider castedUnityObject5))
						{
							item.shape = (NavMeshBuildSourceShape)2;
							item.size = castedUnityObject5.size;
							item.transform = ((Component)item2).transform.localToWorldMatrix * Matrix4x4.Translate(castedUnityObject5.center);
							goto IL_03d5;
						}
						if (BaseNetworkableEx.Is<SphereCollider>((Object)(object)item2, out SphereCollider castedUnityObject6))
						{
							item.shape = (NavMeshBuildSourceShape)2;
							item.size = Vector3.one * castedUnityObject6.radius * 2f;
							item.transform = ((Component)item2).transform.localToWorldMatrix * Matrix4x4.Translate(castedUnityObject6.center);
							goto IL_03d5;
						}
						if (!BaseNetworkableEx.Is<CapsuleCollider>((Object)(object)item2, out CapsuleCollider castedUnityObject7))
						{
							continue;
						}
						item.shape = (NavMeshBuildSourceShape)2;
						float num3 = castedUnityObject7.radius * 2f;
						if (castedUnityObject7.direction == 0)
						{
							item.size = new Vector3(castedUnityObject7.height, num3, num3);
						}
						else if (castedUnityObject7.direction == 1)
						{
							item.size = new Vector3(num3, castedUnityObject7.height, num3);
						}
						else if (castedUnityObject7.direction == 2)
						{
							item.size = new Vector3(num3, num3, castedUnityObject7.height);
						}
						else
						{
							item.size = new Vector3(num3, castedUnityObject7.height, num3);
						}
						item.transform = ((Component)item2).transform.localToWorldMatrix * Matrix4x4.Translate(castedUnityObject7.center);
						goto IL_03d5;
						IL_03d5:
						item.forceUnwalkable = hasAny && (Object)(object)((Component)item2).GetComponentInParent<RustNavmeshModifierVolume>() != (Object)null;
						list.Add(item);
						end_IL_00e6:;
					}
					finally
					{
					}
				}
				BakeStats.AddStage(BakeStats.Stage.CollectColliders, BakeStats.Timestamp() - num2);
				if (val2 != null || RustNavDoorGates.HasGateDoors)
				{
					num2 = BakeStats.Timestamp();
					if (val2 == null)
					{
						val2 = Pool.Get<PooledHashSet<Door>>();
					}
					RustNavDoorGates.CollectGateDoorsReaching(tileBounds, (HashSet<Door>)(object)val2);
					if (((HashSet<Door>)(object)val2).Count == 0)
					{
						Pool.Free<PooledHashSet<Door>>(ref val2);
					}
					BakeStats.AddStage(BakeStats.Stage.CollectDoors, BakeStats.Timestamp() - num2);
					BakeStats.AddStage(BakeStats.Stage.CollectDoorsQuery, BakeStats.Timestamp() - num2);
				}
				NavMeshBuildParams buildParams = (flag ? collectRequest.navmesh.BuildParamsHiRes : collectRequest.navmesh.BuildParams);
				NavMeshBuildVolume[] array = null;
				int num4 = 0;
				if (val2 != null)
				{
					num2 = BakeStats.Timestamp();
					PooledList<RustNavDoorGates.BakeVolumes> val3 = Pool.Get<PooledList<RustNavDoorGates.BakeVolumes>>();
					try
					{
						int num5 = 0;
						foreach (Door item3 in (HashSet<Door>)(object)val2)
						{
							RustNavDoorGates.BakeVolumes bakeVolumes = RustNavDoorGates.GetBakeVolumes(item3);
							((List<RustNavDoorGates.BakeVolumes>)(object)val3).Add(bakeVolumes);
							num5 += bakeVolumes.Count;
						}
						if (num5 > 0)
						{
							array = ArrayPool<NavMeshBuildVolume>.Shared.Rent(num5);
							foreach (RustNavDoorGates.BakeVolumes item4 in (List<RustNavDoorGates.BakeVolumes>)(object)val3)
							{
								num4 += item4.CopyLeafVolumes(array, num4);
							}
							foreach (RustNavDoorGates.BakeVolumes item5 in (List<RustNavDoorGates.BakeVolumes>)(object)val3)
							{
								num4 += item5.CopyApertureVolume(array, num4);
							}
						}
						Pool.Free<PooledHashSet<Door>>(ref val2);
						BakeStats.AddStage(BakeStats.Stage.CollectDoors, BakeStats.Timestamp() - num2);
						BakeStats.AddStage(BakeStats.Stage.CollectDoorsVolumes, BakeStats.Timestamp() - num2);
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
				}
				BakeStats.OnTileCollected(flag);
				BakeStats.AddStage(BakeStats.Stage.CollectTotal, BakeStats.Timestamp() - num);
				return new TileBuildRequest(in collectRequest, list, buildParams, array, num4);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	private unsafe static void FillTriAreasUpTo(RawBuffer<byte> triAreas, int triangleIndexCount, byte area)
	{
		int num = triangleIndexCount / 3 - triAreas.Count;
		if (num > 0)
		{
			UnsafeUtility.MemSet((void*)triAreas.AppendUninitialized(num), area, (long)num);
		}
	}

	private static void FreeTileVolumes(ref TileBuildRequest buildRequest)
	{
		if (buildRequest.volumes != null)
		{
			ArrayPool<NavMeshBuildVolume>.Shared.Return(buildRequest.volumes);
			buildRequest.volumes = null;
			buildRequest.volumeCount = 0;
		}
	}

	private TileBuildResult DoWorkFromBackgroundThread(ref TileBuildRequest buildRequest, CancellationToken globalInterruptToken)
	{
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Invalid comparison between Unknown and I4
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Invalid comparison between Unknown and I4
		//IL_0515: Unknown result type (might be due to invalid IL or missing references)
		//IL_051a: Unknown result type (might be due to invalid IL or missing references)
		//IL_051f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0524: Unknown result type (might be due to invalid IL or missing references)
		//IL_053c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0548: Unknown result type (might be due to invalid IL or missing references)
		//IL_0554: Unknown result type (might be due to invalid IL or missing references)
		//IL_0560: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0613: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06da: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_075d: Unknown result type (might be due to invalid IL or missing references)
		//IL_075f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		if (globalInterruptToken.IsCancellationRequested || buildRequest.cancellation.IsCancellationRequested)
		{
			FreeTileVolumes(ref buildRequest);
			return new TileBuildResult(in buildRequest, TileBuildResultCode.Cancelled);
		}
		RawBuffer<Vector3> vertices = TileScratch.Vertices;
		RawBuffer<int> triangles = TileScratch.Triangles;
		RawBuffer<byte> triAreas = TileScratch.TriAreas;
		vertices.Clear();
		triangles.Clear();
		triAreas.Clear();
		bool flag = false;
		foreach (ThreadSafeNavMeshBuildSource source in buildRequest.sources)
		{
			if (source.forceUnwalkable)
			{
				flag = true;
				break;
			}
		}
		IntPtr intPtr = IntPtr.Zero;
		IntPtr intPtr2 = IntPtr.Zero;
		IntPtr intPtr3 = IntPtr.Zero;
		IntPtr intPtr4 = IntPtr.Zero;
		long num = BakeStats.Timestamp();
		long num2 = num;
		BakeStats.TileTiming timing = new BakeStats.TileTiming
		{
			hiRes = (buildRequest.buildParams.cellSize == buildRequest.navmesh.BuildParamsHiRes.cellSize && buildRequest.buildParams.tileSize == buildRequest.navmesh.BuildParamsHiRes.tileSize)
		};
		try
		{
			if ((Object)(object)TerrainMeta.HeightMap != (Object)null)
			{
				Bounds tileBounds = buildRequest.navmesh.rcCalcTileBounds(new Vector2Int(buildRequest.tx, buildRequest.ty));
				tileBounds = buildRequest.navmesh.rcExpandTileBounds(tileBounds);
				Vector3 topLeftCorner = Vector3Ex.WithY(tileBounds.center - tileBounds.extents, 0f);
				int tileSize = Mathf.CeilToInt(tileBounds.size.x);
				ExtractTerrainGeometry(topLeftCorner, tileSize, vertices, triangles);
			}
			timing.terrain = BakeStats.Timestamp() - num2;
			timing.terrainTris = triangles.Count / 3;
			if (globalInterruptToken.IsCancellationRequested || buildRequest.cancellation.IsCancellationRequested)
			{
				return new TileBuildResult(in buildRequest, TileBuildResultCode.Cancelled);
			}
			num2 = BakeStats.Timestamp();
			foreach (ThreadSafeNavMeshBuildSource source2 in buildRequest.sources)
			{
				if (globalInterruptToken.IsCancellationRequested || buildRequest.cancellation.IsCancellationRequested)
				{
					break;
				}
				if ((int)source2.shape == 1)
				{
					continue;
				}
				if (flag)
				{
					FillTriAreasUpTo(triAreas, triangles.Count, 0);
				}
				if ((int)source2.shape == 2)
				{
					PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
					try
					{
						PooledList<int> val2 = Pool.Get<PooledList<int>>();
						try
						{
							CreateBoxMesh((List<Vector3>)(object)val, (List<int>)(object)val2, Vector3.zero, source2.size);
							Matrix4x4 transform = source2.transform;
							for (int i = 0; i < ((List<Vector3>)(object)val).Count; i++)
							{
								((List<Vector3>)(object)val)[i] = transform.MultiplyPoint3x4(((List<Vector3>)(object)val)[i]);
							}
							int count = vertices.Count;
							triangles.EnsureCapacity(triangles.Count + ((List<int>)(object)val2).Count);
							foreach (int item2 in (List<int>)(object)val2)
							{
								triangles.Add(count + item2);
							}
							vertices.EnsureCapacity(vertices.Count + ((List<Vector3>)(object)val).Count);
							foreach (Vector3 item3 in (List<Vector3>)(object)val)
							{
								vertices.Add(item3);
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
				else if (source2.sourceObjectID != 0)
				{
					if (!MeshCache.TryGet(source2.sourceObjectID, out var data))
					{
						continue;
					}
					PooledList<Vector3> val3 = Pool.Get<PooledList<Vector3>>();
					try
					{
						PooledList<int> val4 = Pool.Get<PooledList<int>>();
						try
						{
							((List<Vector3>)(object)val3).AddRange((IEnumerable<Vector3>)data.vertices);
							((List<int>)(object)val4).AddRange((IEnumerable<int>)data.triangles);
							Matrix4x4 transform2 = source2.transform;
							for (int j = 0; j < ((List<Vector3>)(object)val3).Count; j++)
							{
								((List<Vector3>)(object)val3)[j] = transform2.MultiplyPoint3x4(((List<Vector3>)(object)val3)[j]);
							}
							int count2 = vertices.Count;
							triangles.EnsureCapacity(triangles.Count + ((List<int>)(object)val4).Count);
							foreach (int item4 in (List<int>)(object)val4)
							{
								triangles.Add(count2 + item4);
							}
							vertices.EnsureCapacity(vertices.Count + ((List<Vector3>)(object)val3).Count);
							foreach (Vector3 item5 in (List<Vector3>)(object)val3)
							{
								vertices.Add(item5);
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
				if (flag)
				{
					FillTriAreasUpTo(triAreas, triangles.Count, (byte)(source2.forceUnwalkable ? 62 : 0));
				}
			}
			if (flag)
			{
				FillTriAreasUpTo(triAreas, triangles.Count, 0);
			}
			timing.sources = BakeStats.Timestamp() - num2;
			timing.totalTris = triangles.Count / 3;
			timing.sourceCount = buildRequest.sources.Count;
			if (vertices.Count == 0 || triangles.Count == 0)
			{
				return new TileBuildResult(in buildRequest, TileBuildResultCode.NoGeometry);
			}
			num2 = BakeStats.Timestamp();
			Bounds val5 = buildRequest.navmesh.rcExpandTileBounds(buildRequest.navmesh.rcCalcTileBounds(new Vector2Int(buildRequest.tx, buildRequest.ty)));
			bool flag2 = RecastWrapper.ComputeTriangleYExtent(vertices.Ptr, triangles.Ptr, triangles.Count / 3, val5.min.x, val5.max.x, val5.min.z, val5.max.z, out var outMinY, out var outMaxY);
			timing.yExtent = BakeStats.Timestamp() - num2;
			if (!flag2)
			{
				return new TileBuildResult(in buildRequest, TileBuildResultCode.NoGeometry);
			}
			float cellHeight = buildRequest.buildParams.cellHeight;
			float num3 = cellHeight * 2f;
			outMinY -= num3;
			outMaxY += num3;
			Bounds currentNavmeshBounds = buildRequest.navmesh.CurrentNavmeshBounds;
			outMinY = Mathf.Max(outMinY, currentNavmeshBounds.min.y - num3);
			outMaxY = Mathf.Min(outMaxY, currentNavmeshBounds.max.y + num3);
			if (outMinY > outMaxY)
			{
				return new TileBuildResult(in buildRequest, TileBuildResultCode.NoGeometry);
			}
			float y = buildRequest.navmesh.CurrentNavmeshBounds.min.y;
			outMinY = y + Mathf.Floor((outMinY - y) / cellHeight) * cellHeight;
			if (Mathf.CeilToInt((outMaxY - outMinY) / cellHeight) > 8191)
			{
				TileBuildResult result = new TileBuildResult(in buildRequest, TileBuildResultCode.SpanHeightError);
				result.debugSpanMinY = outMinY;
				result.debugSpanMaxY = outMaxY;
				return result;
			}
			if (globalInterruptToken.IsCancellationRequested || buildRequest.cancellation.IsCancellationRequested)
			{
				return new TileBuildResult(in buildRequest, TileBuildResultCode.Cancelled);
			}
			num2 = BakeStats.Timestamp();
			Bounds val6 = buildRequest.navmesh.rcCalcTileBounds(new Vector2Int(buildRequest.tx, buildRequest.ty));
			Vector3 bmin = new Vector3(val6.min.x, outMinY, val6.min.z);
			Vector3 bmax = new Vector3(val6.max.x, outMaxY, val6.max.z);
			if (DumpGeometryRequest.HasValue && DumpGeometryRequest.Value.tx == buildRequest.tx && DumpGeometryRequest.Value.ty == buildRequest.ty)
			{
				string item = DumpGeometryRequest.Value.path;
				DumpGeometryRequest = null;
				DumpTileGeometry(item, in buildRequest.buildParams, buildRequest.tx, buildRequest.ty, bmin, bmax, vertices, triangles, triAreas);
			}
			RecastWrapper.SetLegacyBuild(RustNav.legacyBuild);
			intPtr = RecastWrapper.CreateHeightFieldRaw(in buildRequest.buildParams, vertices.Ptr, vertices.Count, triangles.Ptr, triangles.Count / 3, in bmin, in bmax, flag ? triAreas.Ptr : IntPtr.Zero);
			timing.heightField = BakeStats.Timestamp() - num2;
			if (intPtr == IntPtr.Zero)
			{
				return new TileBuildResult(in buildRequest, TileBuildResultCode.CreateHeightFieldError);
			}
			if (globalInterruptToken.IsCancellationRequested || buildRequest.cancellation.IsCancellationRequested)
			{
				return new TileBuildResult(in buildRequest, TileBuildResultCode.Cancelled);
			}
			num2 = BakeStats.Timestamp();
			intPtr2 = RecastWrapper.CreateCompactHeightField(in buildRequest.buildParams, intPtr, buildRequest.volumes, buildRequest.volumeCount);
			timing.compact = BakeStats.Timestamp() - num2;
			if (intPtr2 == IntPtr.Zero)
			{
				return new TileBuildResult(in buildRequest, TileBuildResultCode.CreateCompactHeightFieldError);
			}
			if (globalInterruptToken.IsCancellationRequested || buildRequest.cancellation.IsCancellationRequested)
			{
				return new TileBuildResult(in buildRequest, TileBuildResultCode.Cancelled);
			}
			num2 = BakeStats.Timestamp();
			intPtr3 = RecastWrapper.CreatePolymesh(in buildRequest.buildParams, intPtr2);
			timing.polymesh = BakeStats.Timestamp() - num2;
			if (intPtr3 == IntPtr.Zero)
			{
				return new TileBuildResult(in buildRequest, TileBuildResultCode.CreatePolymeshError);
			}
			if (globalInterruptToken.IsCancellationRequested || buildRequest.cancellation.IsCancellationRequested)
			{
				return new TileBuildResult(in buildRequest, TileBuildResultCode.Cancelled);
			}
			if (buildRequest.buildParams.buildDetailMesh)
			{
				num2 = BakeStats.Timestamp();
				intPtr4 = RecastWrapper.CreateDetailPolymesh(in buildRequest.buildParams, intPtr3, intPtr2, RustNav.detailSampleDistMult, RustNav.detailSampleMaxErrorMult);
				timing.detail = BakeStats.Timestamp() - num2;
				if (intPtr4 == IntPtr.Zero)
				{
					return new TileBuildResult(in buildRequest, TileBuildResultCode.CreateDetailPolymeshError);
				}
			}
			if (globalInterruptToken.IsCancellationRequested || buildRequest.cancellation.IsCancellationRequested)
			{
				return new TileBuildResult(in buildRequest, TileBuildResultCode.Cancelled);
			}
			num2 = BakeStats.Timestamp();
			IntPtr intPtr5 = RecastWrapper.CreateNavData(in buildRequest.buildParams, buildRequest.tx, buildRequest.ty, intPtr3, intPtr4, out var dataSize);
			timing.navData = BakeStats.Timestamp() - num2;
			if (intPtr5 == IntPtr.Zero)
			{
				return new TileBuildResult(in buildRequest, TileBuildResultCode.CreateAndAddNavDataError);
			}
			if (AI.checkTileValid && !RecastWrapper.ValidateTileData(intPtr5, dataSize))
			{
				RecastWrapper.FreeTileData(intPtr5);
				return new TileBuildResult(in buildRequest, TileBuildResultCode.ValidationError);
			}
			return new TileBuildResult(in buildRequest, intPtr5, dataSize);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				RecastWrapper.FreeHeightField(intPtr);
			}
			if (intPtr2 != IntPtr.Zero)
			{
				RecastWrapper.FreeCompactHeightField(intPtr2);
			}
			if (intPtr3 != IntPtr.Zero)
			{
				RecastWrapper.FreePolymesh(intPtr3);
			}
			if (intPtr4 != IntPtr.Zero)
			{
				RecastWrapper.FreeDetailPolymesh(intPtr4);
			}
			Pool.FreeUnmanaged<ThreadSafeNavMeshBuildSource>(ref buildRequest.sources);
			FreeTileVolumes(ref buildRequest);
			long num4 = BakeStats.Timestamp() - num;
			BakeStats.AddStage(BakeStats.Stage.WorkerTotal, num4);
			BakeStats.OnTileBuilt(buildRequest.tx, buildRequest.ty, in timing);
			if (RustNav.bakeStatsEnabled && buildRequest.navmesh != null)
			{
				Interlocked.Add(ref buildRequest.navmesh.workerBuildTicks, num4);
			}
		}
	}

	public void TickOnMainThread()
	{
		int num = 0;
		int resultBagDepth = finalMainthreadWorkBag.Count + parkedResults.Count;
		bool flag = IsDefaultNavmeshSaveGated();
		stopwatch.Restart();
		TileBuildResult buildResult;
		bool wasParked;
		while (!flag && TryTakeResult(out buildResult, out wasParked))
		{
			long num2 = BakeStats.Timestamp();
			AddSingleBuiltTileOnMainThread(ref buildResult, wasParked);
			BakeStats.AddStage(BakeStats.Stage.MainAddTile, BakeStats.Timestamp() - num2);
			num++;
			if (RustNav.addTileBudgetMs > 0f && stopwatch.Elapsed.TotalMilliseconds >= (double)RustNav.addTileBudgetMs)
			{
				break;
			}
		}
		if (!flag)
		{
			RustNavDoorGates.FlushTileReasserts(complete: false);
			RustNavDoorGates.FlushSaveDeferredDoors();
			ReportSaveGateCatchUpIfDrained();
		}
		stopwatch.Restart();
		int num3 = 0;
		TileCollectRequest result;
		while (collectMainThreadWorkQueue.TryDequeue(out result))
		{
			(RustNavmesh, int, int) key = (result.navmesh, result.tx, result.ty);
			bool flag2 = tileCancellations.TryGetValue(key, out var value) && value == result.cancellation;
			bool isCancellationRequested = result.cancellation.IsCancellationRequested;
			if (!flag2 | isCancellationRequested)
			{
				if (flag2)
				{
					tileCancellations.Remove(key);
				}
				continue;
			}
			TileBuildRequest item = DoInitialWorkOnMainThread(in result);
			backgroundWorkQueue.Add(item);
			num3++;
			if (stopwatch.Elapsed.TotalMilliseconds >= (double)RustNav.collectBudgetMs)
			{
				break;
			}
		}
		bool budgetLimited = num3 > 0 && collectMainThreadWorkQueue.Count > 0;
		BakeStats.OnMainThreadTick(collectMainThreadWorkQueue.Count, backgroundWorkQueue.Count, resultBagDepth, num3 > 0, budgetLimited);
	}

	private static bool IsDefaultNavmeshSaveGated()
	{
		RustNavigation instance = RustNavigation.Instance;
		if ((Object)(object)instance != (Object)null)
		{
			return instance.IsNavmeshSaveInFlight;
		}
		return false;
	}

	public void OnNavmeshSaveGateLifted()
	{
		TileBuildResult result;
		while (finalMainthreadWorkBag.TryTake(out result))
		{
			parkedResults.Enqueue(result);
		}
		int count = parkedResults.Count;
		gateCatchUp = new NavmeshSaveCatchUpStats
		{
			valid = true,
			parked = count,
			inProgress = (count > 0)
		};
		gateLiftTimestamp = Stopwatch.GetTimestamp();
	}

	private bool TryTakeResult(out TileBuildResult buildResult, out bool wasParked)
	{
		if (parkedResults.TryDequeue(out buildResult))
		{
			wasParked = true;
			return true;
		}
		wasParked = false;
		return finalMainthreadWorkBag.TryTake(out buildResult);
	}

	public void LandParkedResultsOnMainThread()
	{
		if (!IsDefaultNavmeshSaveGated())
		{
			TileBuildResult result;
			while (parkedResults.TryDequeue(out result))
			{
				AddSingleBuiltTileOnMainThread(ref result, countForCatchUp: true);
			}
			RustNavDoorGates.FlushSaveDeferredDoors();
			ReportSaveGateCatchUpIfDrained();
		}
	}

	private void ReportSaveGateCatchUpIfDrained()
	{
		if (gateCatchUp.inProgress && parkedResults.Count <= 0)
		{
			gateCatchUp.inProgress = false;
			gateCatchUp.drainMs = BakeStats.TicksToMs(Stopwatch.GetTimestamp() - gateLiftTimestamp);
			RustNavigation.Log(DescribeSaveGateCatchUp());
		}
	}

	public string DescribeSaveGateCatchUp()
	{
		if (!gateCatchUp.valid)
		{
			return "navmesh save catch up: nothing has been parked this session";
		}
		if (!gateCatchUp.inProgress)
		{
			return $"navmesh save catch up: {gateCatchUp.parked} parked, {gateCatchUp.applied} distinct tiles applied, " + $"{gateCatchUp.superseded} superseded, drain {gateCatchUp.drainMs:F0} ms";
		}
		return string.Format("navmesh save catch up in progress: {0} parked, {1} applied, {2} superseded, {3} still parked", new object[4] { gateCatchUp.parked, gateCatchUp.applied, gateCatchUp.superseded, parkedResults.Count });
	}

	private bool AddSingleBuiltTileOnMainThread(ref TileBuildResult buildResult, bool countForCatchUp)
	{
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		(RustNavmesh, int, int) key = (buildResult.navmesh, buildResult.tx, buildResult.ty);
		bool num = tileCancellations.TryGetValue(key, out var value) && value == buildResult.cancellation;
		bool isCancellationRequested = buildResult.cancellation.IsCancellationRequested;
		buildResult.cancellation = null;
		if (num)
		{
			tileCancellations.Remove(key);
		}
		if (!num | isCancellationRequested)
		{
			if (buildResult.tileBytes != IntPtr.Zero)
			{
				RecastWrapper.FreeTileData(buildResult.tileBytes);
				buildResult.tileBytes = IntPtr.Zero;
			}
			BakeStats.OnResult((int)buildResult.resultCode, superseded: true);
			if (countForCatchUp && gateCatchUp.inProgress)
			{
				gateCatchUp.superseded++;
			}
			return false;
		}
		BakeStats.OnResult((int)buildResult.resultCode, superseded: false);
		if (countForCatchUp && gateCatchUp.inProgress)
		{
			gateCatchUp.applied++;
		}
		if (buildResult.resultCode != TileBuildResultCode.Success)
		{
			if (buildResult.tileBytes != IntPtr.Zero)
			{
				RecastWrapper.FreeTileData(buildResult.tileBytes);
				buildResult.tileBytes = IntPtr.Zero;
			}
			if (buildResult.resultCode != TileBuildResultCode.CreatePolymeshError && buildResult.resultCode != TileBuildResultCode.NoGeometry)
			{
				if (buildResult.resultCode == TileBuildResultCode.SpanHeightError)
				{
					Bounds val = buildResult.navmesh.rcCalcTileBounds(new Vector2Int(buildResult.tx, buildResult.ty));
					Vector3 center = val.center;
					RustNavigation.LogError($"Failed to build navmesh tile {buildResult.tx},{buildResult.ty} at {center}, error code SpanHeightError: " + $"tile geometry spans y {buildResult.debugSpanMinY:F1} to {buildResult.debugSpanMaxY:F1} ({buildResult.debugSpanMaxY - buildResult.debugSpanMinY:F0}m), more than 8191 span cells");
				}
				else
				{
					RustNavigation.LogError($"Failed to build navmesh tile {buildResult.tx},{buildResult.ty}, error code {buildResult.resultCode}");
				}
			}
			buildResult.navmesh.FailTile(buildResult.tx, buildResult.ty);
			return false;
		}
		buildResult.navmesh.AddTile(buildResult.tx, buildResult.ty, buildResult.tileBytes, buildResult.dataSize);
		buildResult.tileBytes = IntPtr.Zero;
		return true;
	}

	private void CleanupRemainingWorkItemsOnMainThread()
	{
		TileBuildRequest item;
		while (backgroundWorkQueue.TryTake(out item))
		{
			Pool.FreeUnmanaged<ThreadSafeNavMeshBuildSource>(ref item.sources);
			FreeTileVolumes(ref item);
		}
		TileBuildResult result;
		while (finalMainthreadWorkBag.TryTake(out result))
		{
			if (result.tileBytes != IntPtr.Zero)
			{
				RecastWrapper.FreeTileData(result.tileBytes);
				result.tileBytes = IntPtr.Zero;
			}
		}
		TileBuildResult result2;
		while (parkedResults.TryDequeue(out result2))
		{
			if (result2.tileBytes != IntPtr.Zero)
			{
				RecastWrapper.FreeTileData(result2.tileBytes);
			}
		}
	}

	private void WorkerLoopFromBackgroundThread()
	{
		CancellationToken token = globalInterrupt.Token;
		while (true)
		{
			TileBuildRequest buildRequest;
			try
			{
				long waitStartTs = BakeStats.Timestamp();
				buildRequest = backgroundWorkQueue.Take(token);
				BakeStats.AddWorkerIdle(waitStartTs, BakeStats.Timestamp());
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (InvalidOperationException)
			{
				break;
			}
			catch (Exception ex3)
			{
				Debug.LogException(ex3);
				continue;
			}
			try
			{
				TileBuildResult item = DoWorkFromBackgroundThread(ref buildRequest, token);
				finalMainthreadWorkBag.Add(item);
			}
			catch (Exception ex4)
			{
				Debug.LogException(ex4);
				try
				{
					finalMainthreadWorkBag.Add(new TileBuildResult(in buildRequest, TileBuildResultCode.UnknownError));
				}
				catch (Exception ex5)
				{
					Debug.LogException(ex5);
				}
			}
		}
	}

	public bool EnqueueOnMainThread(RustNavmesh navmesh, int tx, int ty, bool synchronous = false)
	{
		using (TimeWarning.New("RustNav.BackgroundTileBuilders.Enqueue"))
		{
			(RustNavmesh, int, int) key = (navmesh, tx, ty);
			if (tileCancellations.TryGetValue(key, out var value))
			{
				value.Cancel();
			}
			if (navmesh.IsTileFarFromShore(tx, ty))
			{
				tileCancellations.Remove(key);
				Tile tile = navmesh.GetTile(tx, ty);
				if (tile != null && tile.hasData)
				{
					navmesh.JoinSaveForSynchronousMutation("Culling a navmesh tile that now lies out at sea");
				}
				navmesh.FailTile(tx, ty);
				return false;
			}
			TileCollectRequest collectRequest = new TileCollectRequest(tx, ty, navmesh);
			tileCancellations[key] = collectRequest.cancellation;
			BakeStats.OnTileQueued();
			if (synchronous)
			{
				navmesh.JoinSaveForSynchronousMutation("A synchronous navmesh tile rebuild");
				TileBuildRequest buildRequest = DoInitialWorkOnMainThread(in collectRequest);
				TileBuildResult buildResult = DoWorkFromBackgroundThread(ref buildRequest, CancellationToken.None);
				AddSingleBuiltTileOnMainThread(ref buildResult, countForCatchUp: false);
			}
			else
			{
				collectMainThreadWorkQueue.Enqueue(collectRequest);
			}
			return true;
		}
	}
}

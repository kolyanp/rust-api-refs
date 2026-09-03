using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ConVar;
using Facepunch;
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

		private static readonly List<IDisposable> _all = new List<IDisposable>();

		public static RawBuffer<Vector3> Vertices => _vertices ?? (_vertices = Track(new RawBuffer<Vector3>()));

		public static RawBuffer<int> Triangles => _triangles ?? (_triangles = Track(new RawBuffer<int>()));

		public static RawBuffer<int> Indices => _indices ?? (_indices = Track(new RawBuffer<int>()));

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

	private struct TileBuildRequest(in TileCollectRequest collectRequest, List<ThreadSafeNavMeshBuildSource> sources, NavMeshBuildParams buildParams)
	{
		public readonly int tx = collectRequest.tx;

		public readonly int ty = collectRequest.ty;

		public RustNavmesh navmesh = collectRequest.navmesh;

		public NavMeshBuildParams buildParams = buildParams;

		public List<ThreadSafeNavMeshBuildSource> sources = sources;

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

		public static ThreadSafeNavMeshBuildSource FromNavMeshBuildSource(NavMeshBuildSource source)
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			int num = 0;
			Object sourceObject = ((NavMeshBuildSource)(ref source)).sourceObject;
			Mesh val = (Mesh)(object)((sourceObject is Mesh) ? sourceObject : null);
			if (val != null)
			{
				using (TimeWarning.New("RustNav.ThreadSafeNavMeshBuildSource.MeshCacheGet"))
				{
					MeshCache.Get(val);
					num = ((Object)val).GetInstanceID();
				}
			}
			return new ThreadSafeNavMeshBuildSource
			{
				shape = ((NavMeshBuildSource)(ref source)).shape,
				sourceObjectID = num,
				transform = ((NavMeshBuildSource)(ref source)).transform,
				size = ((NavMeshBuildSource)(ref source)).size,
				area = ((NavMeshBuildSource)(ref source)).area
			};
		}
	}

	private static readonly int[] boxTriangleIndices = new int[36]
	{
		7, 4, 3, 7, 6, 4, 4, 6, 5, 4,
		5, 0, 4, 5, 1, 4, 1, 0, 5, 6,
		2, 5, 2, 1, 6, 7, 3, 6, 3, 2,
		0, 1, 3, 0, 3, 7
	};

	public static (int tx, int ty, string path)? DumpGeometryRequest;

	private Stopwatch stopwatch = new Stopwatch();

	private readonly Dictionary<(RustNavmesh navmesh, int tx, int ty), TileCancellation> tileCancellations = new Dictionary<(RustNavmesh, int, int), TileCancellation>();

	private readonly Queue<TileCollectRequest> collectMainThreadWorkQueue = new Queue<TileCollectRequest>();

	private readonly BlockingCollection<TileBuildRequest> backgroundWorkQueue = new BlockingCollection<TileBuildRequest>();

	private readonly ConcurrentBag<TileBuildResult> finalMainthreadWorkBag = new ConcurrentBag<TileBuildResult>();

	private Thread[] workers;

	private CancellationTokenSource globalInterrupt;

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
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(topLeftCorner.x, 0f, topLeftCorner.z);
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

	private unsafe static void DumpTileGeometry(string path, in NavMeshBuildParams buildParams, int tx, int ty, Vector3 hfMin, Vector3 hfMax, RawBuffer<Vector3> vertices, RawBuffer<int> triangles)
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
		binaryWriter.Write(1);
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
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavigation.DoInitialWorkOnMainThread"))
		{
			long num = BakeStats.Timestamp();
			long num2 = num;
			Bounds tileBounds = collectRequest.navmesh.rcCalcTileBounds(new Vector2Int(collectRequest.tx, collectRequest.ty));
			tileBounds = collectRequest.navmesh.rcExpandTileBounds(tileBounds);
			BakeStats.AddStage(BakeStats.Stage.CollectBounds, BakeStats.Timestamp() - num2);
			int layerMask = 1612808449;
			int areaFromName = NavMesh.GetAreaFromName("Walkable");
			List<ThreadSafeNavMeshBuildSource> list = Pool.Get<List<ThreadSafeNavMeshBuildSource>>();
			PooledList<Collider> val = Pool.Get<PooledList<Collider>>();
			try
			{
				num2 = BakeStats.Timestamp();
				GamePhysics.OverlapBounds(tileBounds, (List<Collider>)(object)val, layerMask, (QueryTriggerInteraction)2);
				BakeStats.AddStage(BakeStats.Stage.CollectOverlap, BakeStats.Timestamp() - num2);
				bool flag = collectRequest.navmesh.ForceHiRes;
				num2 = BakeStats.Timestamp();
				foreach (Collider item2 in (List<Collider>)(object)val)
				{
					try
					{
						BaseEntity baseEntity = GameObjectEx.ToBaseEntity(item2, allowDestroyed: true);
						if ((Object)(object)baseEntity != (Object)null && (baseEntity.isClient || baseEntity.IsDestroyed))
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
						if ((BaseNetworkableEx.Is<TreeEntity>((Object)(object)baseEntity, out TreeEntity castedUnityObject2) && !castedUnityObject2.IncludeInNavmesh) || (BaseNetworkableEx.Is<Door>((Object)(object)baseEntity, out Door castedUnityObject3) && castedUnityObject3.canNpcOpen) || item2.isTrigger || (0x20000000 & (1 << ((Component)item2).gameObject.layer)) != 0)
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
							goto IL_0367;
						}
						if (BaseNetworkableEx.Is<BoxCollider>((Object)(object)item2, out BoxCollider castedUnityObject5))
						{
							item.shape = (NavMeshBuildSourceShape)2;
							item.size = castedUnityObject5.size;
							item.transform = ((Component)item2).transform.localToWorldMatrix * Matrix4x4.Translate(castedUnityObject5.center);
							goto IL_0367;
						}
						if (BaseNetworkableEx.Is<SphereCollider>((Object)(object)item2, out SphereCollider castedUnityObject6))
						{
							item.shape = (NavMeshBuildSourceShape)2;
							item.size = Vector3.one * castedUnityObject6.radius * 2f;
							item.transform = ((Component)item2).transform.localToWorldMatrix * Matrix4x4.Translate(castedUnityObject6.center);
							goto IL_0367;
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
						goto IL_0367;
						IL_0367:
						list.Add(item);
					}
					finally
					{
					}
				}
				BakeStats.AddStage(BakeStats.Stage.CollectColliders, BakeStats.Timestamp() - num2);
				NavMeshBuildParams buildParams = (flag ? collectRequest.navmesh.BuildParamsHiRes : collectRequest.navmesh.BuildParams);
				BakeStats.OnTileCollected(flag);
				BakeStats.AddStage(BakeStats.Stage.CollectTotal, BakeStats.Timestamp() - num);
				return new TileBuildRequest(in collectRequest, list, buildParams);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	private TileBuildResult DoWorkFromBackgroundThread(ref TileBuildRequest buildRequest, CancellationToken globalInterruptToken)
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Invalid comparison between Unknown and I4
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Invalid comparison between Unknown and I4
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_0497: Unknown result type (might be due to invalid IL or missing references)
		//IL_04af: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0530: Unknown result type (might be due to invalid IL or missing references)
		//IL_0535: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0553: Unknown result type (might be due to invalid IL or missing references)
		//IL_0586: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_061c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0621: Unknown result type (might be due to invalid IL or missing references)
		//IL_0626: Unknown result type (might be due to invalid IL or missing references)
		//IL_062c: Unknown result type (might be due to invalid IL or missing references)
		//IL_063a: Unknown result type (might be due to invalid IL or missing references)
		//IL_064d: Unknown result type (might be due to invalid IL or missing references)
		//IL_065b: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		if (globalInterruptToken.IsCancellationRequested || buildRequest.cancellation.IsCancellationRequested)
		{
			return new TileBuildResult(in buildRequest, TileBuildResultCode.Cancelled);
		}
		RawBuffer<Vector3> vertices = TileScratch.Vertices;
		RawBuffer<int> triangles = TileScratch.Triangles;
		vertices.Clear();
		triangles.Clear();
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
				Vector3 topLeftCorner = Vector3Ex.WithY(((Bounds)(ref tileBounds)).center - ((Bounds)(ref tileBounds)).extents, 0f);
				int tileSize = Mathf.CeilToInt(((Bounds)(ref tileBounds)).size.x);
				ExtractTerrainGeometry(topLeftCorner, tileSize, vertices, triangles);
			}
			timing.terrain = BakeStats.Timestamp() - num2;
			timing.terrainTris = triangles.Count / 3;
			if (globalInterruptToken.IsCancellationRequested || buildRequest.cancellation.IsCancellationRequested)
			{
				return new TileBuildResult(in buildRequest, TileBuildResultCode.Cancelled);
			}
			num2 = BakeStats.Timestamp();
			foreach (ThreadSafeNavMeshBuildSource source in buildRequest.sources)
			{
				if (globalInterruptToken.IsCancellationRequested || buildRequest.cancellation.IsCancellationRequested)
				{
					break;
				}
				if ((int)source.shape == 1)
				{
					continue;
				}
				if ((int)source.shape == 2)
				{
					PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
					try
					{
						PooledList<int> val2 = Pool.Get<PooledList<int>>();
						try
						{
							CreateBoxMesh((List<Vector3>)(object)val, (List<int>)(object)val2, Vector3.zero, source.size);
							Matrix4x4 transform = source.transform;
							for (int i = 0; i < ((List<Vector3>)(object)val).Count; i++)
							{
								((List<Vector3>)(object)val)[i] = ((Matrix4x4)(ref transform)).MultiplyPoint3x4(((List<Vector3>)(object)val)[i]);
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
				else
				{
					if (source.sourceObjectID == 0 || !MeshCache.TryGet(source.sourceObjectID, out var data))
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
							Matrix4x4 transform2 = source.transform;
							for (int j = 0; j < ((List<Vector3>)(object)val3).Count; j++)
							{
								((List<Vector3>)(object)val3)[j] = ((Matrix4x4)(ref transform2)).MultiplyPoint3x4(((List<Vector3>)(object)val3)[j]);
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
			bool num3 = RecastWrapper.ComputeTriangleYExtent(vertices.Ptr, triangles.Ptr, triangles.Count / 3, ((Bounds)(ref val5)).min.x, ((Bounds)(ref val5)).max.x, ((Bounds)(ref val5)).min.z, ((Bounds)(ref val5)).max.z, out var outMinY, out var outMaxY);
			timing.yExtent = BakeStats.Timestamp() - num2;
			if (!num3)
			{
				return new TileBuildResult(in buildRequest, TileBuildResultCode.NoGeometry);
			}
			float cellHeight = buildRequest.buildParams.cellHeight;
			float num4 = cellHeight * 2f;
			outMinY -= num4;
			outMaxY += num4;
			Bounds currentNavmeshBounds = buildRequest.navmesh.CurrentNavmeshBounds;
			outMinY = Mathf.Max(outMinY, ((Bounds)(ref currentNavmeshBounds)).min.y - num4);
			outMaxY = Mathf.Min(outMaxY, ((Bounds)(ref currentNavmeshBounds)).max.y + num4);
			if (outMinY > outMaxY)
			{
				return new TileBuildResult(in buildRequest, TileBuildResultCode.NoGeometry);
			}
			float y = ((Bounds)(ref buildRequest.navmesh.CurrentNavmeshBounds)).min.y;
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
			Vector3 bmin = default(Vector3);
			((Vector3)(ref bmin))._002Ector(((Bounds)(ref val6)).min.x, outMinY, ((Bounds)(ref val6)).min.z);
			Vector3 bmax = default(Vector3);
			((Vector3)(ref bmax))._002Ector(((Bounds)(ref val6)).max.x, outMaxY, ((Bounds)(ref val6)).max.z);
			if (DumpGeometryRequest.HasValue && DumpGeometryRequest.Value.tx == buildRequest.tx && DumpGeometryRequest.Value.ty == buildRequest.ty)
			{
				string item = DumpGeometryRequest.Value.path;
				DumpGeometryRequest = null;
				DumpTileGeometry(item, in buildRequest.buildParams, buildRequest.tx, buildRequest.ty, bmin, bmax, vertices, triangles);
			}
			RecastWrapper.SetLegacyBuild(RustNav.legacyBuild);
			intPtr = RecastWrapper.CreateHeightFieldRaw(in buildRequest.buildParams, vertices.Ptr, vertices.Count, triangles.Ptr, triangles.Count / 3, in bmin, in bmax);
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
			intPtr2 = RecastWrapper.CreateCompactHeightField(in buildRequest.buildParams, intPtr);
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
			long num5 = BakeStats.Timestamp() - num;
			BakeStats.AddStage(BakeStats.Stage.WorkerTotal, num5);
			BakeStats.OnTileBuilt(buildRequest.tx, buildRequest.ty, in timing);
			if (RustNav.bakeStatsEnabled && buildRequest.navmesh != null)
			{
				Interlocked.Add(ref buildRequest.navmesh.workerBuildTicks, num5);
			}
		}
	}

	public void TickOnMainThread()
	{
		int num = 0;
		int count = finalMainthreadWorkBag.Count;
		TileBuildResult result;
		while (finalMainthreadWorkBag.TryTake(out result))
		{
			long num2 = BakeStats.Timestamp();
			AddSingleBuiltTileOnMainThread(ref result);
			BakeStats.AddStage(BakeStats.Stage.MainAddTile, BakeStats.Timestamp() - num2);
			num++;
		}
		stopwatch.Restart();
		int num3 = 0;
		TileCollectRequest result2;
		while (collectMainThreadWorkQueue.TryDequeue(out result2))
		{
			(RustNavmesh, int, int) key = (result2.navmesh, result2.tx, result2.ty);
			bool flag = tileCancellations.TryGetValue(key, out var value) && value == result2.cancellation;
			bool isCancellationRequested = result2.cancellation.IsCancellationRequested;
			if (!flag | isCancellationRequested)
			{
				if (flag)
				{
					tileCancellations.Remove(key);
				}
				continue;
			}
			TileBuildRequest item = DoInitialWorkOnMainThread(in result2);
			backgroundWorkQueue.Add(item);
			num3++;
			if (stopwatch.Elapsed.TotalMilliseconds >= (double)RustNav.collectBudgetMs)
			{
				break;
			}
		}
		bool budgetLimited = num3 > 0 && collectMainThreadWorkQueue.Count > 0;
		BakeStats.OnMainThreadTick(collectMainThreadWorkQueue.Count, backgroundWorkQueue.Count, count, num3 > 0, budgetLimited);
	}

	private bool AddSingleBuiltTileOnMainThread(ref TileBuildResult buildResult)
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
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
			return false;
		}
		BakeStats.OnResult((int)buildResult.resultCode, superseded: false);
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
					Vector3 center = ((Bounds)(ref val)).center;
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
				navmesh.FailTile(tx, ty);
				return false;
			}
			TileCollectRequest collectRequest = new TileCollectRequest(tx, ty, navmesh);
			tileCancellations[key] = collectRequest.cancellation;
			BakeStats.OnTileQueued();
			if (synchronous)
			{
				TileBuildRequest buildRequest = DoInitialWorkOnMainThread(in collectRequest);
				TileBuildResult buildResult = DoWorkFromBackgroundThread(ref buildRequest, CancellationToken.None);
				AddSingleBuiltTileOnMainThread(ref buildResult);
			}
			else
			{
				collectMainThreadWorkQueue.Enqueue(collectRequest);
			}
			return true;
		}
	}
}

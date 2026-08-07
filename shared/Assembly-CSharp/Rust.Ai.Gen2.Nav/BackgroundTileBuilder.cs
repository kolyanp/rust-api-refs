using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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

	private struct TileBuildResult
	{
		public enum ResultCode
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

		public readonly int tx;

		public readonly int ty;

		public RustNavmesh navmesh;

		public IntPtr tileBytes;

		public readonly int dataSize;

		public ResultCode resultCode;

		public TileCancellation cancellation;

		public TileBuildResult(in TileBuildRequest request, IntPtr tileBytes, int dataSize)
		{
			tx = request.tx;
			ty = request.ty;
			navmesh = request.navmesh;
			this.tileBytes = tileBytes;
			this.dataSize = dataSize;
			resultCode = ResultCode.Success;
			cancellation = request.cancellation;
		}

		public TileBuildResult(in TileBuildRequest request, ResultCode resultCode)
		{
			tx = request.tx;
			ty = request.ty;
			navmesh = request.navmesh;
			tileBytes = IntPtr.Zero;
			dataSize = 0;
			this.resultCode = resultCode;
			cancellation = request.cancellation;
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

	private readonly int MAX_BUILD_QUEUE;

	private const double TIME_BUDGET_MS = 1.0;

	private Stopwatch stopwatch = new Stopwatch();

	private readonly Dictionary<(RustNavmesh navmesh, int tx, int ty), TileCancellation> tileCancellations = new Dictionary<(RustNavmesh, int, int), TileCancellation>();

	private readonly Queue<TileCollectRequest> collectMainThreadWorkQueue = new Queue<TileCollectRequest>();

	private readonly BlockingCollection<TileBuildRequest> backgroundWorkQueue = new BlockingCollection<TileBuildRequest>();

	private readonly ConcurrentBag<TileBuildResult> finalMainthreadWorkBag = new ConcurrentBag<TileBuildResult>();

	private Thread[] workers;

	private CancellationTokenSource globalInterrupt;

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
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavigation.DoInitialWorkOnMainThread"))
		{
			Bounds tileBounds = collectRequest.navmesh.rcCalcTileBounds(new Vector2Int(collectRequest.tx, collectRequest.ty));
			tileBounds = collectRequest.navmesh.rcExpandTileBounds(tileBounds);
			int layerMask = 1612808449;
			int areaFromName = NavMesh.GetAreaFromName("Walkable");
			List<ThreadSafeNavMeshBuildSource> list = Pool.Get<List<ThreadSafeNavMeshBuildSource>>();
			PooledList<Collider> val = Pool.Get<PooledList<Collider>>();
			try
			{
				GamePhysics.OverlapBounds(tileBounds, (List<Collider>)(object)val, layerMask, (QueryTriggerInteraction)2);
				bool flag = false;
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
						if (BaseNetworkableEx.Is<BuildingBlock>((Object)(object)baseEntity, out BuildingBlock _))
						{
							flag = true;
						}
						else if ((Object)(object)ConstructionErrors.GetPreventBuildingMonumentTag(item2) != (Object)null)
						{
							flag = true;
						}
						if ((BaseNetworkableEx.Is<TreeEntity>((Object)(object)baseEntity, out TreeEntity castedUnityObject2) && !castedUnityObject2.IncludeInNavmesh) || item2.isTrigger)
						{
							continue;
						}
						if (BaseNetworkableEx.Is<MeshCollider>((Object)(object)item2, out MeshCollider castedUnityObject3))
						{
							item.shape = (NavMeshBuildSourceShape)0;
							item.sourceObjectID = ((Object)castedUnityObject3.sharedMesh).GetInstanceID();
							MeshCache.Get(castedUnityObject3.sharedMesh);
							goto IL_02dc;
						}
						if (BaseNetworkableEx.Is<BoxCollider>((Object)(object)item2, out BoxCollider castedUnityObject4))
						{
							item.shape = (NavMeshBuildSourceShape)2;
							item.size = castedUnityObject4.size;
							item.transform = ((Component)item2).transform.localToWorldMatrix * Matrix4x4.Translate(castedUnityObject4.center);
							goto IL_02dc;
						}
						if (BaseNetworkableEx.Is<SphereCollider>((Object)(object)item2, out SphereCollider castedUnityObject5))
						{
							item.shape = (NavMeshBuildSourceShape)2;
							item.size = Vector3.one * castedUnityObject5.radius * 2f;
							item.transform = ((Component)item2).transform.localToWorldMatrix * Matrix4x4.Translate(castedUnityObject5.center);
							goto IL_02dc;
						}
						if (!BaseNetworkableEx.Is<CapsuleCollider>((Object)(object)item2, out CapsuleCollider castedUnityObject6))
						{
							continue;
						}
						item.shape = (NavMeshBuildSourceShape)2;
						float num = castedUnityObject6.radius * 2f;
						if (castedUnityObject6.direction == 0)
						{
							item.size = new Vector3(castedUnityObject6.height, num, num);
						}
						else if (castedUnityObject6.direction == 1)
						{
							item.size = new Vector3(num, castedUnityObject6.height, num);
						}
						else if (castedUnityObject6.direction == 2)
						{
							item.size = new Vector3(num, num, castedUnityObject6.height);
						}
						else
						{
							item.size = new Vector3(num, castedUnityObject6.height, num);
						}
						item.transform = ((Component)item2).transform.localToWorldMatrix * Matrix4x4.Translate(castedUnityObject6.center);
						goto IL_02dc;
						IL_02dc:
						list.Add(item);
					}
					finally
					{
					}
				}
				NavMeshBuildParams buildParams = (flag ? collectRequest.navmesh.BuildParamsHiRes : collectRequest.navmesh.BuildParams);
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
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Invalid comparison between Unknown and I4
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Invalid comparison between Unknown and I4
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04da: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		if (globalInterruptToken.IsCancellationRequested || buildRequest.cancellation.IsCancellationRequested)
		{
			return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.Cancelled);
		}
		RawBuffer<Vector3> vertices = TileScratch.Vertices;
		RawBuffer<int> triangles = TileScratch.Triangles;
		vertices.Clear();
		triangles.Clear();
		IntPtr intPtr = IntPtr.Zero;
		IntPtr intPtr2 = IntPtr.Zero;
		IntPtr intPtr3 = IntPtr.Zero;
		IntPtr intPtr4 = IntPtr.Zero;
		try
		{
			if ((Object)(object)TerrainMeta.HeightMap != (Object)null)
			{
				Bounds tileBounds = buildRequest.navmesh.rcCalcTileBounds(new Vector2Int(buildRequest.tx, buildRequest.ty));
				tileBounds = buildRequest.navmesh.rcExpandTileBounds(tileBounds);
				ExtractTerrainGeometry(Vector3Ex.WithY(((Bounds)(ref tileBounds)).center - ((Bounds)(ref tileBounds)).extents, 0f), Mathf.CeilToInt(((Bounds)(ref tileBounds)).size.x), vertices, triangles);
			}
			if (globalInterruptToken.IsCancellationRequested || buildRequest.cancellation.IsCancellationRequested)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.Cancelled);
			}
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
							TileExtractor.CreateBoxMesh((List<Vector3>)(object)val, (List<int>)(object)val2, Vector3.zero, source.size);
							Matrix4x4 transform = source.transform;
							for (int i = 0; i < ((List<Vector3>)(object)val).Count; i++)
							{
								((List<Vector3>)(object)val)[i] = ((Matrix4x4)(ref transform)).MultiplyPoint3x4(((List<Vector3>)(object)val)[i]);
							}
							int count = vertices.Count;
							triangles.EnsureCapacity(triangles.Count + ((List<int>)(object)val2).Count);
							foreach (int item in (List<int>)(object)val2)
							{
								triangles.Add(count + item);
							}
							vertices.EnsureCapacity(vertices.Count + ((List<Vector3>)(object)val).Count);
							foreach (Vector3 item2 in (List<Vector3>)(object)val)
							{
								vertices.Add(item2);
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
							foreach (int item3 in (List<int>)(object)val4)
							{
								triangles.Add(count2 + item3);
							}
							vertices.EnsureCapacity(vertices.Count + ((List<Vector3>)(object)val3).Count);
							foreach (Vector3 item4 in (List<Vector3>)(object)val3)
							{
								vertices.Add(item4);
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
			if (vertices.Count == 0 || triangles.Count == 0)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.NoGeometry);
			}
			float num = float.MaxValue;
			float num2 = float.MinValue;
			for (int k = 0; k < vertices.Count; k++)
			{
				float y = vertices[k].y;
				if (y < num)
				{
					num = y;
				}
				if (y > num2)
				{
					num2 = y;
				}
			}
			if (!(num <= num2))
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.NoGeometry);
			}
			float cellHeight = buildRequest.buildParams.cellHeight;
			float num3 = cellHeight * 2f;
			num -= num3;
			num2 += num3;
			float y2 = ((Bounds)(ref buildRequest.navmesh.CurrentNavmeshBounds)).min.y;
			num = y2 + Mathf.Floor((num - y2) / cellHeight) * cellHeight;
			if (Mathf.CeilToInt((num2 - num) / cellHeight) > 8191)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.SpanHeightError);
			}
			if (globalInterruptToken.IsCancellationRequested || buildRequest.cancellation.IsCancellationRequested)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.Cancelled);
			}
			Bounds val5 = buildRequest.navmesh.rcCalcTileBounds(new Vector2Int(buildRequest.tx, buildRequest.ty));
			Vector3 bmin = default(Vector3);
			((Vector3)(ref bmin))._002Ector(((Bounds)(ref val5)).min.x, num, ((Bounds)(ref val5)).min.z);
			Vector3 bmax = default(Vector3);
			((Vector3)(ref bmax))._002Ector(((Bounds)(ref val5)).max.x, num2, ((Bounds)(ref val5)).max.z);
			intPtr = RecastWrapper.CreateHeightFieldRaw(in buildRequest.buildParams, vertices.Ptr, vertices.Count, triangles.Ptr, triangles.Count / 3, in bmin, in bmax);
			if (intPtr == IntPtr.Zero)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.CreateHeightFieldError);
			}
			if (globalInterruptToken.IsCancellationRequested || buildRequest.cancellation.IsCancellationRequested)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.Cancelled);
			}
			intPtr2 = RecastWrapper.CreateCompactHeightField(in buildRequest.buildParams, intPtr);
			if (intPtr2 == IntPtr.Zero)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.CreateCompactHeightFieldError);
			}
			if (globalInterruptToken.IsCancellationRequested || buildRequest.cancellation.IsCancellationRequested)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.Cancelled);
			}
			intPtr3 = RecastWrapper.CreatePolymesh(in buildRequest.buildParams, intPtr2);
			if (intPtr3 == IntPtr.Zero)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.CreatePolymeshError);
			}
			if (globalInterruptToken.IsCancellationRequested || buildRequest.cancellation.IsCancellationRequested)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.Cancelled);
			}
			if (buildRequest.buildParams.buildDetailMesh)
			{
				intPtr4 = RecastWrapper.CreateDetailPolymesh(in buildRequest.buildParams, intPtr3, intPtr2);
				if (intPtr4 == IntPtr.Zero)
				{
					return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.CreateDetailPolymeshError);
				}
			}
			if (globalInterruptToken.IsCancellationRequested || buildRequest.cancellation.IsCancellationRequested)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.Cancelled);
			}
			int dataSize;
			IntPtr intPtr5 = RecastWrapper.CreateNavData(in buildRequest.buildParams, buildRequest.tx, buildRequest.ty, intPtr3, intPtr4, out dataSize);
			if (intPtr5 == IntPtr.Zero)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.CreateAndAddNavDataError);
			}
			if (AI.checkTileValid && !RecastWrapper.ValidateTileData(intPtr5, dataSize))
			{
				RecastWrapper.FreeTileData(intPtr5);
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.ValidationError);
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
		}
	}

	public void TickOnMainThread()
	{
		int num = 0;
		TileBuildResult result;
		while (finalMainthreadWorkBag.TryTake(out result))
		{
			AddSingleBuiltTileOnMainThread(ref result);
			num++;
		}
		stopwatch.Restart();
		int num2 = 0;
		TileCollectRequest collectRequest;
		while ((MAX_BUILD_QUEUE <= 0 || backgroundWorkQueue.Count < MAX_BUILD_QUEUE) && collectMainThreadWorkQueue.TryDequeue(out collectRequest))
		{
			(RustNavmesh, int, int) key = (collectRequest.navmesh, collectRequest.tx, collectRequest.ty);
			TileCancellation value;
			bool flag = tileCancellations.TryGetValue(key, out value) && value == collectRequest.cancellation;
			bool isCancellationRequested = collectRequest.cancellation.IsCancellationRequested;
			if (!flag || isCancellationRequested)
			{
				if (flag)
				{
					tileCancellations.Remove(key);
				}
				continue;
			}
			TileBuildRequest item = DoInitialWorkOnMainThread(in collectRequest);
			backgroundWorkQueue.Add(item);
			num2++;
			if (!(stopwatch.Elapsed.TotalMilliseconds >= 1.0))
			{
				continue;
			}
			break;
		}
	}

	private bool AddSingleBuiltTileOnMainThread(ref TileBuildResult buildResult)
	{
		(RustNavmesh, int, int) key = (buildResult.navmesh, buildResult.tx, buildResult.ty);
		TileCancellation value;
		bool num = tileCancellations.TryGetValue(key, out value) && value == buildResult.cancellation;
		bool isCancellationRequested = buildResult.cancellation.IsCancellationRequested;
		buildResult.cancellation = null;
		if (num)
		{
			tileCancellations.Remove(key);
		}
		if (!num || isCancellationRequested || buildResult.resultCode != TileBuildResult.ResultCode.Success)
		{
			if (buildResult.tileBytes != IntPtr.Zero)
			{
				RecastWrapper.FreeTileData(buildResult.tileBytes);
				buildResult.tileBytes = IntPtr.Zero;
			}
			if (buildResult.resultCode != TileBuildResult.ResultCode.Success && buildResult.resultCode != TileBuildResult.ResultCode.CreatePolymeshError && buildResult.resultCode != TileBuildResult.ResultCode.NoGeometry && buildResult.resultCode != TileBuildResult.ResultCode.Cancelled)
			{
				RustNavigation.LogError($"Failed to build navmesh tile {buildResult.tx},{buildResult.ty}, error code {buildResult.resultCode}");
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
				buildRequest = backgroundWorkQueue.Take(token);
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
					finalMainthreadWorkBag.Add(new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.UnknownError));
				}
				catch (Exception ex5)
				{
					Debug.LogException(ex5);
				}
			}
		}
	}

	public void EnqueueOnMainThread(RustNavmesh navmesh, int tx, int ty, bool synchronous = false)
	{
		using (TimeWarning.New("RustNav.BackgroundTileBuilders.Enqueue"))
		{
			(RustNavmesh, int, int) key = (navmesh, tx, ty);
			if (tileCancellations.TryGetValue(key, out var value))
			{
				value.Cancel();
			}
			TileCollectRequest collectRequest = new TileCollectRequest(tx, ty, navmesh);
			tileCancellations[key] = collectRequest.cancellation;
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
		}
	}
}

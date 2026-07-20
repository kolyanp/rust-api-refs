using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ConVar;
using Facepunch;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2.Nav;

public class BackgroundTileBuilder : IDisposable
{
	private struct TileCollectRequest(int tx, int ty, RustNavmesh navmesh)
	{
		public readonly int tx = tx;

		public readonly int ty = ty;

		public RustNavmesh navmesh = navmesh;

		public CancellationTokenSource tokenSource = new CancellationTokenSource();

		public CancellationToken token = tokenSource.Token;
	}

	private struct TileBuildRequest(in TileCollectRequest collectRequest, List<ThreadSafeNavMeshBuildSource> sources)
	{
		public readonly int tx = collectRequest.tx;

		public readonly int ty = collectRequest.ty;

		public RustNavmesh navmesh = collectRequest.navmesh;

		public NavMeshBuildParams buildParams = navmesh.GetBuildParamsForTile(tx, ty);

		public List<ThreadSafeNavMeshBuildSource> sources = sources;

		public CancellationTokenSource tokenSource = collectRequest.tokenSource;

		public CancellationToken token = collectRequest.token;
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

		public CancellationTokenSource tokenSource;

		public TileBuildResult(in TileBuildRequest request, IntPtr tileBytes, int dataSize)
		{
			tx = request.tx;
			ty = request.ty;
			navmesh = request.navmesh;
			this.tileBytes = tileBytes;
			this.dataSize = dataSize;
			resultCode = ResultCode.Success;
			tokenSource = request.tokenSource;
		}

		public TileBuildResult(in TileBuildRequest request, ResultCode resultCode)
		{
			tx = request.tx;
			ty = request.ty;
			navmesh = request.navmesh;
			tileBytes = IntPtr.Zero;
			dataSize = 0;
			this.resultCode = resultCode;
			tokenSource = request.tokenSource;
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

	private readonly Dictionary<(RustNavmesh navmesh, int tx, int ty), CancellationTokenSource> tileCancellations = new Dictionary<(RustNavmesh, int, int), CancellationTokenSource>();

	private readonly Queue<TileCollectRequest> collectMainThreadWorkQueue = new Queue<TileCollectRequest>();

	private readonly BlockingCollection<TileBuildRequest> backgroundWorkQueue = new BlockingCollection<TileBuildRequest>();

	private readonly ConcurrentBag<TileBuildResult> finalMainthreadWorkBag = new ConcurrentBag<TileBuildResult>();

	private Thread[] workers;

	private CancellationTokenSource globalInterrupt;

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
		foreach (KeyValuePair<(RustNavmesh, int, int), CancellationTokenSource> tileCancellation in tileCancellations)
		{
			pendingTiles.Add(tileCancellation.Key);
		}
	}

	public void GetPendingTilesForNavmeshOnMainThread(RustNavmesh navmesh, List<(int tx, int ty)> pendingTiles)
	{
		foreach (KeyValuePair<(RustNavmesh, int, int), CancellationTokenSource> tileCancellation in tileCancellations)
		{
			if (tileCancellation.Key.Item1 == navmesh)
			{
				pendingTiles.Add((tileCancellation.Key.Item2, tileCancellation.Key.Item3));
			}
		}
	}

	public void CancelPendingTilesForOnMainThread(RustNavmesh navmesh)
	{
		foreach (KeyValuePair<(RustNavmesh, int, int), CancellationTokenSource> tileCancellation in tileCancellations)
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
		Thread[] array = workers;
		foreach (Thread thread in array)
		{
			try
			{
				thread.Join(1000);
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
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
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavigation.DoInitialWorkOnMainThread"))
		{
			Bounds tileBounds = collectRequest.navmesh.rcCalcTileBounds(new Vector2Int(collectRequest.tx, collectRequest.ty));
			tileBounds = collectRequest.navmesh.rcExpandTileBounds(tileBounds);
			int layerMask = 2195713;
			int areaFromName = NavMesh.GetAreaFromName("Walkable");
			List<ThreadSafeNavMeshBuildSource> list = Pool.Get<List<ThreadSafeNavMeshBuildSource>>();
			PooledList<Collider> val = Pool.Get<PooledList<Collider>>();
			try
			{
				GamePhysics.OverlapBounds(tileBounds, (List<Collider>)(object)val, layerMask, (QueryTriggerInteraction)1);
				foreach (Collider item2 in (List<Collider>)(object)val)
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
					BoxCollider castedUnityObject2;
					SphereCollider castedUnityObject3;
					if (BaseNetworkableEx.Is<MeshCollider>((Object)(object)item2, out MeshCollider castedUnityObject))
					{
						item.shape = (NavMeshBuildSourceShape)0;
						item.sourceObjectID = ((Object)castedUnityObject.sharedMesh).GetInstanceID();
						MeshCache.Get(castedUnityObject.sharedMesh);
					}
					else if (BaseNetworkableEx.Is<BoxCollider>((Object)(object)item2, out castedUnityObject2))
					{
						item.shape = (NavMeshBuildSourceShape)2;
						item.size = castedUnityObject2.size;
						item.transform = ((Component)item2).transform.localToWorldMatrix * Matrix4x4.Translate(castedUnityObject2.center);
					}
					else if (BaseNetworkableEx.Is<SphereCollider>((Object)(object)item2, out castedUnityObject3))
					{
						item.shape = (NavMeshBuildSourceShape)2;
						item.size = Vector3.one * castedUnityObject3.radius * 2f;
						item.transform = ((Component)item2).transform.localToWorldMatrix * Matrix4x4.Translate(castedUnityObject3.center);
					}
					else
					{
						if (!BaseNetworkableEx.Is<CapsuleCollider>((Object)(object)item2, out CapsuleCollider castedUnityObject4))
						{
							continue;
						}
						item.shape = (NavMeshBuildSourceShape)2;
						float num = castedUnityObject4.radius * 2f;
						if (castedUnityObject4.direction == 0)
						{
							item.size = new Vector3(castedUnityObject4.height, num, num);
						}
						else if (castedUnityObject4.direction == 1)
						{
							item.size = new Vector3(num, castedUnityObject4.height, num);
						}
						else if (castedUnityObject4.direction == 2)
						{
							item.size = new Vector3(num, num, castedUnityObject4.height);
						}
						else
						{
							item.size = new Vector3(num, castedUnityObject4.height, num);
						}
						item.transform = ((Component)item2).transform.localToWorldMatrix * Matrix4x4.Translate(castedUnityObject4.center);
					}
					list.Add(item);
				}
				return new TileBuildRequest(in collectRequest, list);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	private unsafe TileBuildResult DoWorkFromBackgroundThread(ref TileBuildRequest buildRequest, CancellationToken globalInterruptToken)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Invalid comparison between Unknown and I4
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Invalid comparison between Unknown and I4
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_0469: Unknown result type (might be due to invalid IL or missing references)
		//IL_046e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_0479: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		if (globalInterruptToken.IsCancellationRequested || buildRequest.token.IsCancellationRequested)
		{
			return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.Cancelled);
		}
		FPNativeList<Vector3> fPNativeList = Pool.Get<FPNativeList<Vector3>>();
		FPNativeList<int> fPNativeList2 = Pool.Get<FPNativeList<int>>();
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
				PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
				try
				{
					PooledList<int> val2 = Pool.Get<PooledList<int>>();
					try
					{
						TileExtractor.ExtractTerrainGeometry2(Vector3Ex.WithY(((Bounds)(ref tileBounds)).center - ((Bounds)(ref tileBounds)).extents, 0f), Mathf.CeilToInt(((Bounds)(ref tileBounds)).size.x), (List<Vector3>)(object)val, (List<int>)(object)val2);
						foreach (int item in (List<int>)(object)val2)
						{
							fPNativeList2.Add(fPNativeList.Count + item);
						}
						foreach (Vector3 item2 in (List<Vector3>)(object)val)
						{
							fPNativeList.Add(item2);
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
			if (globalInterruptToken.IsCancellationRequested || buildRequest.token.IsCancellationRequested)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.Cancelled);
			}
			foreach (ThreadSafeNavMeshBuildSource source in buildRequest.sources)
			{
				if (globalInterruptToken.IsCancellationRequested || buildRequest.token.IsCancellationRequested)
				{
					break;
				}
				if ((int)source.shape == 1)
				{
					continue;
				}
				if ((int)source.shape == 2)
				{
					PooledList<Vector3> val3 = Pool.Get<PooledList<Vector3>>();
					try
					{
						PooledList<int> val4 = Pool.Get<PooledList<int>>();
						try
						{
							TileExtractor.CreateBoxMesh((List<Vector3>)(object)val3, (List<int>)(object)val4, Vector3.zero, source.size);
							Matrix4x4 transform = source.transform;
							for (int i = 0; i < ((List<Vector3>)(object)val3).Count; i++)
							{
								((List<Vector3>)(object)val3)[i] = ((Matrix4x4)(ref transform)).MultiplyPoint3x4(((List<Vector3>)(object)val3)[i]);
							}
							foreach (int item3 in (List<int>)(object)val4)
							{
								fPNativeList2.Add(fPNativeList.Count + item3);
							}
							foreach (Vector3 item4 in (List<Vector3>)(object)val3)
							{
								fPNativeList.Add(item4);
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
				else
				{
					if (source.sourceObjectID == 0 || !MeshCache.TryGet(source.sourceObjectID, out var data))
					{
						continue;
					}
					PooledList<Vector3> val5 = Pool.Get<PooledList<Vector3>>();
					try
					{
						PooledList<int> val6 = Pool.Get<PooledList<int>>();
						try
						{
							((List<Vector3>)(object)val5).AddRange((IEnumerable<Vector3>)data.vertices);
							((List<int>)(object)val6).AddRange((IEnumerable<int>)data.triangles);
							Matrix4x4 transform2 = source.transform;
							for (int j = 0; j < ((List<Vector3>)(object)val5).Count; j++)
							{
								((List<Vector3>)(object)val5)[j] = ((Matrix4x4)(ref transform2)).MultiplyPoint3x4(((List<Vector3>)(object)val5)[j]);
							}
							foreach (int item5 in (List<int>)(object)val6)
							{
								fPNativeList2.Add(fPNativeList.Count + item5);
							}
							foreach (Vector3 item6 in (List<Vector3>)(object)val5)
							{
								fPNativeList.Add(item6);
							}
						}
						finally
						{
							((IDisposable)val6)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val5)?.Dispose();
					}
				}
			}
			if (fPNativeList.Count == 0 || fPNativeList2.Count == 0)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.NoGeometry);
			}
			if (globalInterruptToken.IsCancellationRequested || buildRequest.token.IsCancellationRequested)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.Cancelled);
			}
			Bounds val7 = buildRequest.navmesh.rcCalcTileBounds(new Vector2Int(buildRequest.tx, buildRequest.ty));
			intPtr = RecastWrapper.CreateHeightFieldRaw(in buildRequest.buildParams, (IntPtr)NativeArrayUnsafeUtility.GetUnsafePtr<Vector3>(fPNativeList.Array), fPNativeList.Count, (IntPtr)NativeArrayUnsafeUtility.GetUnsafePtr<int>(fPNativeList2.Array), fPNativeList2.Count / 3, ((Bounds)(ref val7)).min, ((Bounds)(ref val7)).max);
			if (intPtr == IntPtr.Zero)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.CreateHeightFieldError);
			}
			if (globalInterruptToken.IsCancellationRequested || buildRequest.token.IsCancellationRequested)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.Cancelled);
			}
			intPtr2 = RecastWrapper.CreateCompactHeightField(in buildRequest.buildParams, intPtr);
			if (intPtr2 == IntPtr.Zero)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.CreateCompactHeightFieldError);
			}
			if (globalInterruptToken.IsCancellationRequested || buildRequest.token.IsCancellationRequested)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.Cancelled);
			}
			intPtr3 = RecastWrapper.CreatePolymesh(in buildRequest.buildParams, intPtr2);
			if (intPtr3 == IntPtr.Zero)
			{
				return new TileBuildResult(in buildRequest, TileBuildResult.ResultCode.CreatePolymeshError);
			}
			if (globalInterruptToken.IsCancellationRequested || buildRequest.token.IsCancellationRequested)
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
			if (globalInterruptToken.IsCancellationRequested || buildRequest.token.IsCancellationRequested)
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
			Pool.Free<FPNativeList<Vector3>>(ref fPNativeList);
			Pool.Free<FPNativeList<int>>(ref fPNativeList2);
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
			CancellationTokenSource value;
			bool flag = tileCancellations.TryGetValue(key, out value) && value == collectRequest.tokenSource;
			bool isCancellationRequested = collectRequest.tokenSource.IsCancellationRequested;
			if (!flag || isCancellationRequested)
			{
				if (flag)
				{
					tileCancellations.Remove(key);
				}
				collectRequest.tokenSource.Dispose();
				collectRequest.tokenSource = null;
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
		CancellationTokenSource value;
		bool num = tileCancellations.TryGetValue(key, out value) && value == buildResult.tokenSource;
		bool isCancellationRequested = buildResult.tokenSource.IsCancellationRequested;
		buildResult.tokenSource.Dispose();
		buildResult.tokenSource = null;
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
			item.tokenSource.Dispose();
		}
		TileBuildResult result;
		while (finalMainthreadWorkBag.TryTake(out result))
		{
			if (result.tileBytes != IntPtr.Zero)
			{
				RecastWrapper.FreeTileData(result.tileBytes);
				result.tileBytes = IntPtr.Zero;
			}
			result.tokenSource.Dispose();
		}
	}

	private void WorkerLoopFromBackgroundThread()
	{
		CancellationToken token = globalInterrupt.Token;
		while (true)
		{
			try
			{
				TileBuildRequest buildRequest = backgroundWorkQueue.Take(token);
				TileBuildResult item = DoWorkFromBackgroundThread(ref buildRequest, token);
				finalMainthreadWorkBag.Add(item);
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
			tileCancellations[key] = collectRequest.tokenSource;
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

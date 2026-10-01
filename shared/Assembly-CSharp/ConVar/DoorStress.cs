using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Facepunch;
using ProtoBuf;
using Rust;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace ConVar;

[Factory("doorstress")]
public class DoorStress : ConsoleSystem
{
	public struct BaseSlot
	{
		public string name;

		public Vector3 origin;

		public float radius;

		public int entityStart;

		public int entities;

		public int gateDoors;

		public Vector3 footprint;

		public string note;
	}

	private struct ArmResult
	{
		public double bakeCollectTotal;

		public double bakeDoors;

		public double bakeDoorQuery;

		public double bakeDoorVolumes;

		public double bakeAddTile;

		public double bakeWorker;

		public long bakeTiles;

		public float bakeWall;

		public double toggleMedian;

		public double toggleMean;

		public double spawnFirst;

		public double spawnMean;

		public double killMean;

		public double spawnCold;

		public double saveMs;

		public double loadMs;

		public long saveBytes;

		public string polys;
	}

	private struct PlacementResult
	{
		public double first;

		public double mean;

		public double kill;

		public double cold;
	}

	private struct ProposalArm
	{
		public string name;

		public bool slabs;

		public float thickness;

		public bool ownership;

		public bool refCache;

		public double toggle;

		public double gatherUs;

		public double writeUs;

		public double register;

		public double unregister;

		public double reassert;

		public double bakeDoorUsPerTile;

		public int totalPolys;

		public int aperturePolys;

		public int leafPolys;

		public int unclaimed;

		public int shared;

		public float neighbours;

		public int doors;
	}

	private enum NoPolyCause
	{
		HasPolys,
		MarkingFailed,
		IslandPruned,
		DropBehind,
		SplitLevel,
		NoFloorAtAll
	}

	private struct FinalArm
	{
		public string name;

		public bool gates;

		public bool refCache;

		public double toggle;

		public double gatherUs;

		public double writeUs;

		public double register;

		public double unregister;

		public double reassert;

		public double bakeDoorUsPerTile;

		public double bakeMain;

		public double bakeWorker;

		public double saveMs;

		public double loadMs;

		public long saveBytes;

		public long bakeTiles;

		public int totalPolys;

		public int doors;
	}

	private const string CorpusName = "doorstress";

	private const string CorpusDir = "Assets/AutomatedTests/CopyPastes/";

	private static readonly string[] CorpusBases = new string[8] { "bigbaseRF", "standard1", "standard2", "standard3", "flat1", "circle", "base_4x4", "autoturretbase" };

	private const float BaseSpacing = 300f;

	private static readonly List<BaseSlot> corpusSlots = new List<BaseSlot>();

	private static readonly List<BaseEntity> corpusEntities = new List<BaseEntity>();

	private static Vector3 corpusOrigin;

	private static float corpusRadius;

	private static bool envBuilt;

	private static string requestedBase = "all";

	private static string lastEnvReport = "no corpus built yet";

	private static string lastBenchReport = "no bench run yet";

	private static string lastFeatureReport = "no feature bench run yet";

	private static string lastSweepReport = "no thickness sweep run yet";

	private static string lastProposalReport = "no proposal bench run yet";

	private static string lastAuditReport = "no audit run yet";

	private static string lastPruneReport = "no prune sweep run yet";

	private static string lastFinalReport = "no final bench run yet";

	private static bool EnsureBackend(Arg arg)
	{
		if (!RustNavigation.EnsureNewNavmesh())
		{
			arg.ReplyWith("Restart the editor without command line argument -useOldNavmesh");
			return false;
		}
		if (AI.useUnityNavmesh)
		{
			arg.ReplyWith("ai.useUnityNavmesh is True, there is no door gating to measure on that backend");
			return false;
		}
		RustNavigation instance = RustNavigation.Instance;
		if ((Object)(object)instance == (Object)null || !instance.IsDefaultNavmeshBuilt())
		{
			arg.ReplyWith("The default navmesh is not built yet");
			return false;
		}
		return true;
	}

	[ServerVar(EditorOnly = true, Help = "Paste the bigbaseRF corpus on the ground and bake it: doorstress.build_env <centerX centerZ>, defaults -140 140")]
	public static void build_env(Arg arg)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		if (!EnsureBackend(arg))
		{
			return;
		}
		if (envBuilt || corpusEntities.Count > 0)
		{
			arg.ReplyWith("a corpus is already pasted, run doorstress.clear first");
			return;
		}
		int num = CountGateDoors();
		if (num > 0)
		{
			arg.ReplyWith($"{num} nav gate doors already stand on this map, doorstress needs a clean map or the census stops describing the corpus");
			return;
		}
		requestedBase = arg.GetString(0, "all");
		int num2 = 1;
		if (requestedBase.Length > 0 && (char.IsDigit(requestedBase[0]) || requestedBase[0] == '-'))
		{
			requestedBase = "all";
			num2 = 0;
		}
		Vector3 val = new Vector3(arg.GetFloat(num2, -140f), 0f, arg.GetFloat(num2 + 1, 140f));
		RaycastHit val2 = default;
		if (!Physics.Raycast(val + Vector3.up * 500f, Vector3.down, ref val2, 2000f, LayerMask.GetMask(new string[3] { "World", "Terrain", "Default" })))
		{
			arg.ReplyWith($"no ground under {val}, pick another spot");
			return;
		}
		corpusOrigin = val2.point;
		((MonoBehaviour)Global.Runner).StartCoroutine(BuildEnvRoutine());
		arg.ReplyWith($"doorstress corpus pasting at {corpusOrigin}, poll doorstress.envreport");
	}

	[ServerVar(EditorOnly = true, Help = "Print the last corpus build report")]
	public static void envreport(Arg arg)
	{
		arg.ReplyWith(lastEnvReport);
	}

	private static IEnumerator BuildEnvRoutine()
	{
		Vector3 gridCenter = corpusOrigin;
		corpusSlots.Clear();
		int cols = Mathf.CeilToInt(Mathf.Sqrt((float)CorpusBases.Length));
		Bounds totalBounds = new Bounds(gridCenter, Vector3.zero);
		bool anyPasted = false;
		bool single = requestedBase != "all";
		RaycastHit val = default;
		for (int b = 0; b < CorpusBases.Length; b++)
		{
			string text = CorpusBases[b];
			if (single && text != requestedBase)
			{
				continue;
			}
			BaseSlot item = new BaseSlot
			{
				name = text
			};
			int num = b % cols;
			int num2 = b / cols;
			if (!Physics.Raycast((single ? new Vector3(gridCenter.x, 0f, gridCenter.z) : new Vector3(gridCenter.x + ((float)num - (float)(cols - 1) * 0.5f) * 300f, 0f, gridCenter.z + ((float)num2 - (float)(cols - 1) * 0.5f) * 300f)) + Vector3.up * 500f, Vector3.down, ref val, 2000f, LayerMask.GetMask(new string[3] { "World", "Terrain", "Default" })))
			{
				item.note = "no ground under the slot, skipped";
				corpusSlots.Add(item);
				continue;
			}
			item.origin = val.point;
			CopyPasteEntityInfo val2 = CopyPaste.LoadFileFromBundles("Assets/AutomatedTests/CopyPastes/" + text + ".data");
			if (val2 == null)
			{
				item.note = "could not load the .data, skipped";
				corpusSlots.Add(item);
				continue;
			}
			CopyPaste.PasteOptions options = new CopyPaste.PasteOptions
			{
				Deployables = true,
				Vehicles = true,
				SnapToTerrain = true,
				Origin = item.origin,
				PlayerRotation = Quaternion.identity
			};
			int num3 = (item.entityStart = corpusEntities.Count);
			bool flag = false;
			try
			{
				corpusEntities.AddRange(CopyPaste.PasteEntitiesInternal(val2, options, 0uL));
				flag = true;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			if (!flag)
			{
				item.note = "paste threw, see the exception above";
				corpusSlots.Add(item);
				continue;
			}
			item.entities = corpusEntities.Count - num3;
			anyPasted |= item.entities > 0;
			Bounds val3 = default;
			bool flag2 = false;
			for (int i = num3; i < corpusEntities.Count; i++)
			{
				BaseEntity baseEntity = corpusEntities[i];
				if (!((Object)(object)baseEntity == (Object)null) && !baseEntity.IsDestroyed)
				{
					if (flag2)
					{
						val3.Encapsulate(((Component)baseEntity).transform.position);
						continue;
					}
					val3 = new Bounds(((Component)baseEntity).transform.position, Vector3.zero);
					flag2 = true;
				}
			}
			if (item.entities > 0 && Mathf.Abs(val3.min.y - item.origin.y) > 5f)
			{
				item.note = $"snapped to y {val3.min.y:F1} against ground y {item.origin.y:F1}, NOT standing on the navmesh";
			}
			item.radius = Mathf.Max(val3.extents.x, val3.extents.z) + 8f;
			item.footprint = val3.size;
			item.origin = new Vector3(val3.center.x, item.origin.y, val3.center.z);
			corpusSlots.Add(item);
			if (item.entities > 0)
			{
				totalBounds.Encapsulate(val3);
			}
			yield return null;
		}
		if (!anyPasted)
		{
			Fail("no base pasted, see the notes above");
			ClearCorpus();
			yield break;
		}
		float deadline = Time.realtimeSinceStartup + 120f;
		while (!AllFullySpawned() && Time.realtimeSinceStartup < deadline)
		{
			yield return null;
		}
		((ObjectWorkQueue<BuildingBlock>)BuildingBlock.updateSkinQueueServer).RunQueue(60000.0);
		yield return CoroutineEx.waitForEndOfFrame;
		while (((ObjectWorkQueue)BuildingBlock.updateSkinQueueServer).QueueLength > 0)
		{
			yield return null;
		}
		Physics.SyncTransforms();
		corpusRadius = Mathf.Max(totalBounds.extents.x, totalBounds.extents.z) + 16f;
		corpusOrigin = new Vector3(totalBounds.center.x, gridCenter.y, totalBounds.center.z);
		RustNavigation.Instance.RebuildTilesInBounds(RebakeBounds(corpusOrigin, corpusRadius));
		yield return WaitForTileDrain();
		for (int j = 0; j < corpusSlots.Count; j++)
		{
			BaseSlot baseSlot = corpusSlots[j];
			PooledList<Door> val4 = Pool.Get<PooledList<Door>>();
			try
			{
				CollectSlotGateDoors(baseSlot, (List<Door>)(object)val4);
				baseSlot.gateDoors = ((List<Door>)(object)val4).Count;
				corpusSlots[j] = baseSlot;
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
		}
		int num4 = CountGateDoors();
		envBuilt = num4 > 0;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(string.Format("doorstress corpus at {0}, radius {1:F0}m: {2} entities, {3} nav gate doors over {4} bases{5}", new object[6]
		{
			corpusOrigin,
			corpusRadius,
			corpusEntities.Count,
			num4,
			CorpusBases.Length,
			envBuilt ? "" : " (FAILED, no gate doors registered)"
		}));
		foreach (BaseSlot corpusSlot in corpusSlots)
		{
			stringBuilder.Append(string.Format(" | {0}: {1} entities, {2} gate doors, footprint {3:F0}x{4:F0}m at ({5:F0},{6:F0}){7}", new object[8]
			{
				corpusSlot.name,
				corpusSlot.entities,
				corpusSlot.gateDoors,
				corpusSlot.footprint.x,
				corpusSlot.footprint.z,
				corpusSlot.origin.x,
				corpusSlot.origin.z,
				string.IsNullOrEmpty(corpusSlot.note) ? "" : (" [" + corpusSlot.note + "]")
			}));
		}
		lastEnvReport = stringBuilder.ToString();
		Debug.Log((object)("[DoorStress] " + lastEnvReport));
	}

	[ServerVar(EditorOnly = true, Help = "Kill the pasted corpus and rebake the ground it stood on")]
	public static void clear(Arg arg)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (corpusEntities.Count == 0)
		{
			arg.ReplyWith("no corpus pasted");
			return;
		}
		Bounds rebuildBounds = RebakeBounds(corpusOrigin, corpusRadius);
		int num = ClearCorpus();
		if ((Object)(object)RustNavigation.Instance != (Object)null && !AI.useUnityNavmesh)
		{
			RustNavigation.Instance.RebuildTilesInBounds(rebuildBounds);
		}
		lastEnvReport = "no corpus built yet";
		arg.ReplyWith($"killed {num} corpus entities, the ground is rebaking");
	}

	private static int ClearCorpus()
	{
		int num = 0;
		for (int i = 0; i < corpusEntities.Count; i++)
		{
			BaseEntity baseEntity = corpusEntities[i];
			if (!((Object)(object)baseEntity == (Object)null) && !baseEntity.IsDestroyed)
			{
				baseEntity.Kill();
				num++;
			}
		}
		corpusEntities.Clear();
		envBuilt = false;
		return num;
	}

	private static void Fail(string reason)
	{
		lastEnvReport = reason;
		Debug.LogError((object)("[DoorStress] " + reason));
	}

	private static bool AllFullySpawned()
	{
		for (int i = 0; i < corpusEntities.Count; i++)
		{
			BaseEntity baseEntity = corpusEntities[i];
			if ((Object)(object)baseEntity != (Object)null && !baseEntity.IsDestroyed && !baseEntity.IsFullySpawned())
			{
				return false;
			}
		}
		return true;
	}

	private static Bounds RebakeBounds(Vector3 origin, float radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return new Bounds(origin, new Vector3(radius * 2f, 120f, radius * 2f));
	}

	private static IEnumerator WaitForTileDrain()
	{
		for (int stableEmptyChecks = 0; stableEmptyChecks < 5; stableEmptyChecks = ((!RustNavigation.Instance.HasPendingTiles()) ? (stableEmptyChecks + 1) : 0))
		{
			yield return CoroutineEx.waitForEndOfFrame;
		}
		RustNavDoorGates.FlushTileReasserts();
	}

	[ServerVar(EditorOnly = true, Help = "Run the fixed door gating suite over the pasted corpus: doorstress.bench <burst reps, default 3> <leaf slabs 1 or 0, default 1>. Non destructive, every door is restored to the state it was in")]
	public static void bench(Arg arg)
	{
		if (EnsureBackend(arg))
		{
			if (!envBuilt)
			{
				arg.ReplyWith("run doorstress.build_env first");
				return;
			}
			int reps = Mathf.Clamp(arg.GetInt(0, 3), 1, 20);
			bool flag = arg.GetInt(1, 1) != 0;
			((MonoBehaviour)Global.Runner).StartCoroutine(MeasureRoutine(reps, flag));
			arg.ReplyWith("doorstress bench started over doorstress, leaf slabs " + (flag ? "on" : "off") + ", results via doorstress.benchresults once the rebake drains");
		}
	}

	[ServerVar(EditorOnly = true, Help = "Print the last benchmark results")]
	public static void benchresults(Arg arg)
	{
		arg.ReplyWith(lastBenchReport);
	}

	private static IEnumerator MeasureRoutine(int reps, bool leafSlabs)
	{
		RustNavDoorGates.leafSlabsEnabled = leafSlabs;
		RustNavDoorGates.InvalidateCaches();
		RustNavigation.Instance.RebuildTilesInBounds(RebakeBounds(corpusOrigin, corpusRadius));
		yield return WaitForTileDrain();
		RustNavDoorGates.ReassertAll();
		PooledList<Door> doors = Pool.Get<PooledList<Door>>();
		try
		{
			CollectGateDoors(corpusOrigin, corpusRadius, (List<Door>)(object)doors);
			if (((List<Door>)(object)doors).Count == 0)
			{
				lastBenchReport = $"[DoorStress] no nav gate doors within {corpusRadius:F0}m of {corpusOrigin}";
				Debug.Log((object)lastBenchReport);
				yield break;
			}
			string census = Census((List<Door>)(object)doors, corpusRadius);
			string polyCensus = PolyCensus();
			double controlFirst = TogglePass((List<Door>)(object)doors, gating: false, null, profile: false);
			TogglePass((List<Door>)(object)doors, gating: true, null, profile: false);
			PooledList<double> samples = Pool.Get<PooledList<double>>();
			try
			{
				RustNavDoorGates.Profile.Reset();
				TogglePass((List<Door>)(object)doors, gating: true, (List<double>)(object)samples, profile: true);
				double toggleGatherMs = RustNavDoorGates.Profile.GatherMs;
				double toggleWriteMs = RustNavDoorGates.Profile.WriteMs;
				int toggleWrites = RustNavDoorGates.Profile.writes;
				int toggleGroupDoors = RustNavDoorGates.Profile.groupDoors;
				double controlLast = TogglePass((List<Door>)(object)doors, gating: false, null, profile: false);
				((List<double>)(object)samples).Sort();
				double num = 0.0;
				for (int i = 0; i < ((List<double>)(object)samples).Count; i++)
				{
					num += ((List<double>)(object)samples)[i];
				}
				double toggleMedian = ((List<double>)(object)samples)[((List<double>)(object)samples).Count / 2];
				double toggleP90 = ((List<double>)(object)samples)[Mathf.Min(((List<double>)(object)samples).Count - 1, (int)((float)((List<double>)(object)samples).Count * 0.9f))];
				double toggleWorst = ((List<double>)(object)samples)[((List<double>)(object)samples).Count - 1];
				double toggleMean = num / (double)((List<double>)(object)samples).Count;
				yield return null;
				PooledList<double> reassertSamples = Pool.Get<PooledList<double>>();
				try
				{
					Stopwatch watch = new Stopwatch();
					RustNavDoorGates.Profile.Reset();
					RustNavDoorGates.Profile.enabled = true;
					for (int j = 0; j < reps; j++)
					{
						watch.Restart();
						RustNavDoorGates.ReassertAll();
						watch.Stop();
						((List<double>)(object)reassertSamples).Add(watch.Elapsed.TotalMilliseconds);
						yield return null;
					}
					RustNavDoorGates.Profile.enabled = false;
					((List<double>)(object)reassertSamples).Sort();
					double reassertMedian = ((List<double>)(object)reassertSamples)[((List<double>)(object)reassertSamples).Count / 2];
					double reassertGatherMs = RustNavDoorGates.Profile.GatherMs / (double)reps;
					double reassertWriteMs = RustNavDoorGates.Profile.WriteMs / (double)reps;
					int num2 = Mathf.Max(1, ((List<Door>)(object)doors).Count / 60);
					int cycles = 0;
					double num3 = 0.0;
					double num4 = 0.0;
					for (int k = 0; k < ((List<Door>)(object)doors).Count; k += num2)
					{
						Door door = ((List<Door>)(object)doors)[k];
						if (!((Object)(object)door == (Object)null) && !door.IsDestroyed)
						{
							watch.Restart();
							RustNavDoorGates.Unregister(door);
							watch.Stop();
							num3 += watch.Elapsed.TotalMilliseconds;
							watch.Restart();
							RustNavDoorGates.Register(door);
							watch.Stop();
							num4 += watch.Elapsed.TotalMilliseconds;
							cycles++;
						}
					}
					double unregisterMean = ((cycles > 0) ? (num3 / (double)cycles) : 0.0);
					double registerMean = ((cycles > 0) ? (num4 / (double)cycles) : 0.0);
					yield return null;
					bool bakeStatsWereEnabled = RustNav.bakeStatsEnabled;
					RustNav.bakeStatsEnabled = true;
					BakeStats.Reset();
					float rebakeStart = Time.realtimeSinceStartup;
					RustNavigation.Instance.RebuildTilesInBounds(RebakeBounds(corpusOrigin, corpusRadius));
					yield return WaitForTileDrain();
					double num5 = BakeStats.StageMs(BakeStats.Stage.CollectTotal) + BakeStats.StageMs(BakeStats.Stage.MainAddTile);
					double num6 = BakeStats.StageMs(BakeStats.Stage.WorkerTotal);
					double num7 = BakeStats.StageMs(BakeStats.Stage.CollectDoors);
					float num8 = Time.realtimeSinceStartup - rebakeStart;
					RustNav.bakeStatsEnabled = bakeStatsWereEnabled;
					double num9 = toggleMedian - controlLast;
					double num10 = unregisterMean + registerMean;
					double num11 = num9 + reassertMedian + num10 + num7;
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("===== doorstress bench, doorstress, leaf slabs " + (leafSlabs ? "ON" : "OFF") + " =====");
					stringBuilder.AppendLine("census        " + census);
					stringBuilder.AppendLine("navmesh       " + polyCensus);
					stringBuilder.AppendLine(string.Format("toggle        median {0:F3} ms, p90 {1:F3} ms, worst {2:F3} ms, mean {3:F3} ms over {4} doors", new object[5]
					{
						toggleMedian,
						toggleP90,
						toggleWorst,
						toggleMean,
						((List<double>)(object)samples).Count
					}));
					stringBuilder.AppendLine(string.Format("toggle split  gather {0:F2} ms total, native writes {1:F2} ms total over {2} writes, {3} doors applied", new object[4] { toggleGatherMs, toggleWriteMs, toggleWrites, toggleGroupDoors }));
					stringBuilder.AppendLine($"control       {controlFirst:F3} ms first pass, {controlLast:F3} ms last pass, gating off. The last pass is the honest one, the first pays warm up");
					stringBuilder.AppendLine(string.Format("reassert-all  median {0:F3} ms over {1} reps, gather {2:F2} ms, writes {3:F2} ms", new object[4] { reassertMedian, reps, reassertGatherMs, reassertWriteMs }));
					stringBuilder.AppendLine($"register      unregister {unregisterMean:F3} ms, register {registerMean:F3} ms, mean over {cycles} cycles");
					stringBuilder.AppendLine(string.Format("rebake        {0:F1} ms main thread ({1:F1} ms door volumes), {2:F1} ms worker, {3:F1}s wall clock", new object[4] { num5, num7, num6, num8 }));
					stringBuilder.AppendLine(string.Format("SUITE TOTAL   {0:F3} ms of door work = toggle gating {1:F3} + reassert-all {2:F3} + register cycle {3:F3} + bake door volumes {4:F1}", new object[5] { num11, num9, reassertMedian, num10, num7 }));
					stringBuilder.AppendLine(string.Format("table row     | leaf slabs {0} | {1:F3} ms | {2:F3} ms | {3:F3} ms | {4:F3} ms | {5:F2} ms | {6:F2} ms | {7:F3} ms | {8:F3} ms | {9:F1} ms | {10:F1} ms |", new object[11]
					{
						leafSlabs ? "on" : "off",
						toggleMedian,
						toggleP90,
						toggleWorst,
						toggleMean,
						toggleGatherMs,
						toggleWriteMs,
						reassertMedian,
						registerMean,
						num5,
						num7
					}));
					lastBenchReport = stringBuilder.ToString();
					Debug.Log((object)lastBenchReport);
				}
				finally
				{
					((IDisposable)reassertSamples)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)samples)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)doors)?.Dispose();
		}
	}

	private static double TogglePass(List<Door> doors, bool gating, List<double> samples, bool profile)
	{
		bool useUnityNavmesh = AI.useUnityNavmesh;
		Stopwatch stopwatch = new Stopwatch();
		double num = 0.0;
		int num2 = 0;
		try
		{
			if (!gating)
			{
				AI.useUnityNavmesh = true;
			}
			for (int i = 0; i < doors.Count; i++)
			{
				Door door = doors[i];
				if (!((Object)(object)door == (Object)null) && !door.IsDestroyed)
				{
					bool flag = door.IsOpen();
					RustNavDoorGates.Profile.enabled = profile;
					stopwatch.Restart();
					door.SetOpen(!flag);
					stopwatch.Stop();
					RustNavDoorGates.Profile.enabled = false;
					double totalMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
					num += totalMilliseconds;
					num2++;
					samples?.Add(totalMilliseconds);
					door.SetOpen(flag);
				}
			}
		}
		finally
		{
			AI.useUnityNavmesh = useUnityNavmesh;
			RustNavDoorGates.Profile.enabled = false;
		}
		if (num2 <= 0)
		{
			return 0.0;
		}
		return num / (double)num2;
	}

	private static int CountGateDoors()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Door door = enumerator.Current as Door;
				if ((Object)(object)door != (Object)null && !door.IsDestroyed && door.IsNavGate)
				{
					num++;
				}
			}
			return num;
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private static void CollectGateDoors(Vector3 origin, float radius, List<Door> results)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		float num = radius * radius;
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Door door = enumerator.Current as Door;
				if (!((Object)(object)door == (Object)null) && !door.IsDestroyed && door.IsNavGate)
				{
					Vector3 val = ((Component)door).transform.position - origin;
					if (val.sqrMagnitude <= num)
					{
						results.Add(door);
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private static string PolyCensus()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		RustNavmesh rustNavmesh = (((Object)(object)RustNavigation.Instance != (Object)null) ? RustNavigation.Instance.DefaultNavmesh : null);
		if (rustNavmesh == null || !rustNavmesh.IsValid())
		{
			return "no default navmesh";
		}
		Bounds val = RebakeBounds(corpusOrigin, corpusRadius);
		Vector2Int val2 = rustNavmesh.rcCalcTileCoordFromPos(val.min);
		Vector2Int val3 = rustNavmesh.rcCalcTileCoordFromPos(val.max);
		PooledList<Vector3> val4 = Pool.Get<PooledList<Vector3>>();
		try
		{
			PooledList<byte> val5 = Pool.Get<PooledList<byte>>();
			try
			{
				PooledList<ushort> val6 = Pool.Get<PooledList<ushort>>();
				try
				{
					int num = 0;
					int num2 = 0;
					int num3 = 0;
					int num4 = 0;
					int num5 = 0;
					for (int i = val2.x; i <= val3.x; i++)
					{
						for (int j = val2.y; j <= val3.y; j++)
						{
							((List<Vector3>)(object)val4).Clear();
							((List<byte>)(object)val5).Clear();
							((List<ushort>)(object)val6).Clear();
							if (!rustNavmesh.GetTilePolysWithStateInternal(i, j, (List<Vector3>)(object)val4, (List<byte>)(object)val5, (List<ushort>)(object)val6))
							{
								continue;
							}
							num5++;
							num += ((List<byte>)(object)val5).Count;
							for (int k = 0; k < ((List<byte>)(object)val5).Count; k++)
							{
								if (((List<byte>)(object)val5)[k] == 3 || ((List<byte>)(object)val5)[k] == 6)
								{
									if (((List<byte>)(object)val5)[k] == 3)
									{
										num2++;
									}
									else
									{
										num3++;
									}
									if ((((List<ushort>)(object)val6)[k] & 1) == 0)
									{
										num4++;
									}
								}
							}
						}
					}
					return string.Format("{0} polys over {1} tiles, {2} aperture polys, {3} leaf polys, {4} of them currently unwalkable", new object[5] { num, num5, num2, num3, num4 });
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
		finally
		{
			((IDisposable)val4)?.Dispose();
		}
	}

	private static string Census(List<Door> doors, float radius)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = 0;
		float num3 = float.MaxValue;
		float num4 = float.MinValue;
		for (int i = 0; i < doors.Count; i++)
		{
			Door door = doors[i];
			if (Mathf.Abs(Mathf.Repeat(((Component)door).transform.eulerAngles.y + 45f, 90f) - 45f) > 5f)
			{
				num++;
			}
			if (door.HasNavSwingLeaf && door.IsOpen())
			{
				num2++;
			}
			num3 = Mathf.Min(num3, ((Component)door).transform.position.y);
			num4 = Mathf.Max(num4, ((Component)door).transform.position.y);
		}
		return string.Format("{0} gate doors within {1:F0}m, {2} off axis, {3} open with a swing leaf, {4:F1}m vertical spread", new object[5]
		{
			doors.Count,
			radius,
			num,
			num2,
			num4 - num3
		});
	}

	[ServerVar(EditorOnly = true, Help = "Price the whole door gating feature per event class, gates on vs off: doorstress.featurebench <door placement samples, default 24>")]
	public static void featurebench(Arg arg)
	{
		if (EnsureBackend(arg))
		{
			if (!envBuilt)
			{
				arg.ReplyWith("run doorstress.build_env first");
				return;
			}
			int placements = Mathf.Clamp(arg.GetInt(0, 24), 1, 200);
			((MonoBehaviour)Global.Runner).StartCoroutine(FeatureRoutine(placements));
			arg.ReplyWith("doorstress feature bench started over doorstress, results via doorstress.featureresults");
		}
	}

	[ServerVar(EditorOnly = true, Help = "Print the last feature bench results")]
	public static void featureresults(Arg arg)
	{
		arg.ReplyWith(lastFeatureReport);
	}

	private static IEnumerator FeatureRoutine(int placements)
	{
		ArmResult on = default;
		ArmResult off = default;
		yield return RunArm(gates: true, placements, (ArmResult r) =>
		{
			on = r;
		});
		yield return RunArm(gates: false, placements, (ArmResult r) =>
		{
			off = r;
		});
		ArmResult onAgain = default;
		yield return RunArm(gates: true, placements, (ArmResult r) =>
		{
			onAgain = r;
		});
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("===== doorstress feature bench, doorstress, whole door gating on vs off =====");
		stringBuilder.AppendLine("navmesh on    " + on.polys);
		stringBuilder.AppendLine("navmesh off   " + off.polys);
		stringBuilder.AppendLine("| event | gates ON | gates OFF | ON again |");
		stringBuilder.AppendLine($"| tile bake, collect main total | {on.bakeCollectTotal:F1} ms | {off.bakeCollectTotal:F1} ms | {onAgain.bakeCollectTotal:F1} ms |");
		stringBuilder.AppendLine($"| tile bake, door volumes | {on.bakeDoors:F1} ms | {off.bakeDoors:F1} ms | {onAgain.bakeDoors:F1} ms |");
		stringBuilder.AppendLine($"|   of which registry query | {on.bakeDoorQuery:F1} ms | {off.bakeDoorQuery:F1} ms | {onAgain.bakeDoorQuery:F1} ms |");
		stringBuilder.AppendLine($"|   of which hull copy | {on.bakeDoorVolumes:F1} ms | {off.bakeDoorVolumes:F1} ms | {onAgain.bakeDoorVolumes:F1} ms |");
		stringBuilder.AppendLine($"| tile bake, add tile main | {on.bakeAddTile:F1} ms | {off.bakeAddTile:F1} ms | {onAgain.bakeAddTile:F1} ms |");
		stringBuilder.AppendLine($"| tile bake, worker total | {on.bakeWorker:F1} ms | {off.bakeWorker:F1} ms | {onAgain.bakeWorker:F1} ms |");
		stringBuilder.AppendLine($"| tiles collected | {on.bakeTiles} | {off.bakeTiles} | {onAgain.bakeTiles} |");
		stringBuilder.AppendLine("| per tile main thread door cost | " + PerTile(on.bakeDoors, on.bakeTiles) + " | " + PerTile(off.bakeDoors, off.bakeTiles) + " | " + PerTile(onAgain.bakeDoors, onAgain.bakeTiles) + " |");
		stringBuilder.AppendLine($"| door toggle, median | {on.toggleMedian:F3} ms | {off.toggleMedian:F3} ms | {onAgain.toggleMedian:F3} ms |");
		stringBuilder.AppendLine($"| door toggle, mean | {on.toggleMean:F3} ms | {off.toggleMean:F3} ms | {onAgain.toggleMean:F3} ms |");
		stringBuilder.AppendLine($"| door placement, first spawn | {on.spawnFirst:F3} ms | {off.spawnFirst:F3} ms | {onAgain.spawnFirst:F3} ms |");
		stringBuilder.AppendLine($"| door placement, mean spawn | {on.spawnMean:F3} ms | {off.spawnMean:F3} ms | {onAgain.spawnMean:F3} ms |");
		stringBuilder.AppendLine($"| door placement, cold prefab (first door of its kind) | {on.spawnCold:F3} ms | {off.spawnCold:F3} ms | {onAgain.spawnCold:F3} ms |");
		stringBuilder.AppendLine($"| door destroy, mean | {on.killMean:F3} ms | {off.killMean:F3} ms | {onAgain.killMean:F3} ms |");
		stringBuilder.AppendLine(string.Format("| navmesh save | {0:F1} ms, {1} KB | {2:F1} ms, {3} KB | {4:F1} ms, {5} KB |", new object[6]
		{
			on.saveMs,
			on.saveBytes / 1024,
			off.saveMs,
			off.saveBytes / 1024,
			onAgain.saveMs,
			onAgain.saveBytes / 1024
		}));
		stringBuilder.AppendLine($"| navmesh load | {on.loadMs:F1} ms | {off.loadMs:F1} ms | {onAgain.loadMs:F1} ms |");
		lastFeatureReport = stringBuilder.ToString();
		Debug.Log((object)lastFeatureReport);
	}

	private static string PerTile(double ms, long tiles)
	{
		if (tiles <= 0)
		{
			return "n/a";
		}
		return $"{ms * 1000.0 / (double)tiles:F1} us";
	}

	private static IEnumerator RunArm(bool gates, int placements, Action<ArmResult> report)
	{
		ArmResult result = default;
		RustNavDoorGates.gatesEnabled = gates;
		RustNavDoorGates.leafSlabsEnabled = true;
		RustNavDoorGates.InvalidateCaches();
		bool bakeStatsWereEnabled = RustNav.bakeStatsEnabled;
		RustNav.bakeStatsEnabled = true;
		BakeStats.Reset();
		float bakeStart = Time.realtimeSinceStartup;
		RustNavigation.Instance.RebuildTilesInBounds(RebakeBounds(corpusOrigin, corpusRadius));
		yield return WaitForTileDrain();
		result.bakeWall = Time.realtimeSinceStartup - bakeStart;
		result.bakeCollectTotal = BakeStats.StageMs(BakeStats.Stage.CollectTotal);
		result.bakeDoors = BakeStats.StageMs(BakeStats.Stage.CollectDoors);
		result.bakeDoorQuery = BakeStats.StageMs(BakeStats.Stage.CollectDoorsQuery);
		result.bakeDoorVolumes = BakeStats.StageMs(BakeStats.Stage.CollectDoorsVolumes);
		result.bakeAddTile = BakeStats.StageMs(BakeStats.Stage.MainAddTile);
		result.bakeWorker = BakeStats.StageMs(BakeStats.Stage.WorkerTotal);
		result.bakeTiles = BakeStats.StageCount(BakeStats.Stage.CollectTotal);
		RustNav.bakeStatsEnabled = bakeStatsWereEnabled;
		RustNavDoorGates.ReassertAll();
		result.polys = PolyCensus();
		PooledList<Door> val = Pool.Get<PooledList<Door>>();
		try
		{
			CollectAllDoors(corpusOrigin, corpusRadius, (List<Door>)(object)val);
			TogglePassRaw((List<Door>)(object)val, null);
			PooledList<double> val2 = Pool.Get<PooledList<double>>();
			try
			{
				TogglePassRaw((List<Door>)(object)val, (List<double>)(object)val2);
				((List<double>)(object)val2).Sort();
				double num = 0.0;
				for (int i = 0; i < ((List<double>)(object)val2).Count; i++)
				{
					num += ((List<double>)(object)val2)[i];
				}
				result.toggleMedian = ((((List<double>)(object)val2).Count > 0) ? ((List<double>)(object)val2)[((List<double>)(object)val2).Count / 2] : 0.0);
				result.toggleMean = ((((List<double>)(object)val2).Count > 0) ? (num / (double)((List<double>)(object)val2).Count) : 0.0);
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
		yield return null;
		yield return MeasurePlacement(placements, (PlacementResult r) =>
		{
			result.spawnFirst = r.first;
			result.spawnMean = r.mean;
			result.killMean = r.kill;
			result.spawnCold = r.cold;
		});
		yield return null;
		string text = Path.Combine(Application.temporaryCachePath, "doorstress.navmesh");
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Restart();
		bool flag = RustNavigation.Instance.Save(text);
		stopwatch.Stop();
		result.saveMs = (flag ? stopwatch.Elapsed.TotalMilliseconds : (-1.0));
		result.saveBytes = ((flag && File.Exists(text)) ? new FileInfo(text).Length : 0);
		if (flag)
		{
			stopwatch.Restart();
			RustNavigation.Instance.Load(text, synchronous: true);
			stopwatch.Stop();
			result.loadMs = stopwatch.Elapsed.TotalMilliseconds;
			yield return WaitForTileDrain();
		}
		else
		{
			result.loadMs = -1.0;
		}
		report(result);
	}

	private static IEnumerator MeasurePlacement(int samples, Action<PlacementResult> report)
	{
		PlacementResult result = default;
		PooledList<Door> doors = Pool.Get<PooledList<Door>>();
		try
		{
			CollectAllDoors(corpusOrigin, corpusRadius, (List<Door>)(object)doors);
			if (((List<Door>)(object)doors).Count == 0)
			{
				report(result);
				yield break;
			}
			Stopwatch watch = new Stopwatch();
			double spawnTotal = 0.0;
			double killTotal = 0.0;
			int counted = 0;
			int stride = Mathf.Max(1, ((List<Door>)(object)doors).Count / samples);
			for (int i = 0; i < ((List<Door>)(object)doors).Count; i += stride)
			{
				if (counted >= samples)
				{
					break;
				}
				Door door = ((List<Door>)(object)doors)[i];
				if ((Object)(object)door == (Object)null || door.IsDestroyed)
				{
					continue;
				}
				string prefabName = door.PrefabName;
				Vector3 position = ((Component)door).transform.position;
				Quaternion rotation = ((Component)door).transform.rotation;
				watch.Restart();
				BaseEntity baseEntity = GameManager.server.CreateEntity(prefabName, position, rotation);
				if ((Object)(object)baseEntity == (Object)null)
				{
					watch.Stop();
					continue;
				}
				baseEntity.Spawn();
				watch.Stop();
				double totalMilliseconds = watch.Elapsed.TotalMilliseconds;
				if (counted == 0)
				{
					result.first = totalMilliseconds;
				}
				spawnTotal += totalMilliseconds;
				watch.Restart();
				baseEntity.Kill();
				watch.Stop();
				killTotal += watch.Elapsed.TotalMilliseconds;
				counted++;
				yield return null;
			}
			result.mean = ((counted > 0) ? (spawnTotal / (double)counted) : 0.0);
			result.kill = ((counted > 0) ? (killTotal / (double)counted) : 0.0);
			Door door2 = ((List<Door>)(object)doors)[0];
			if ((Object)(object)door2 != (Object)null && !door2.IsDestroyed)
			{
				Door.ClearNavGateCache();
				watch.Restart();
				BaseEntity baseEntity2 = GameManager.server.CreateEntity(door2.PrefabName, ((Component)door2).transform.position, ((Component)door2).transform.rotation);
				if ((Object)(object)baseEntity2 != (Object)null)
				{
					baseEntity2.Spawn();
					watch.Stop();
					result.cold = watch.Elapsed.TotalMilliseconds;
					baseEntity2.Kill();
				}
				else
				{
					watch.Stop();
				}
			}
			report(result);
			yield return WaitForTileDrain();
		}
		finally
		{
			((IDisposable)doors)?.Dispose();
		}
	}

	private static void CollectAllDoors(Vector3 origin, float radius, List<Door> results)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		float num = radius * radius;
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Door door = enumerator.Current as Door;
				if (!((Object)(object)door == (Object)null) && !door.IsDestroyed)
				{
					Vector3 val = ((Component)door).transform.position - origin;
					if (val.sqrMagnitude <= num)
					{
						results.Add(door);
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private static void TogglePassRaw(List<Door> doors, List<double> samples)
	{
		Stopwatch stopwatch = new Stopwatch();
		for (int i = 0; i < doors.Count; i++)
		{
			Door door = doors[i];
			if (!((Object)(object)door == (Object)null) && !door.IsDestroyed)
			{
				bool flag = door.IsOpen();
				stopwatch.Restart();
				door.SetOpen(!flag);
				stopwatch.Stop();
				samples?.Add(stopwatch.Elapsed.TotalMilliseconds);
				door.SetOpen(flag);
			}
		}
	}

	[ServerVar(EditorOnly = true, Help = "Rebake the corpus at a range of aperture half thicknesses and census the door to door overlap at each: doorstress.thicknesssweep")]
	public static void thicknesssweep(Arg arg)
	{
		if (EnsureBackend(arg))
		{
			if (!envBuilt)
			{
				arg.ReplyWith("run doorstress.build_env first");
				return;
			}
			((MonoBehaviour)Global.Runner).StartCoroutine(SweepRoutine());
			arg.ReplyWith("doorstress thickness sweep started, results via doorstress.sweepresults");
		}
	}

	[ServerVar(EditorOnly = true, Help = "Print the last thickness sweep results")]
	public static void sweepresults(Arg arg)
	{
		arg.ReplyWith(lastSweepReport);
	}

	private static IEnumerator SweepRoutine()
	{
		float shipped = RustNavDoorGates.apertureHalfThickness;
		float[] array = new float[7] { 0.5f, 0.4f, 0.3f, 0.25f, 0.2f, 0.15f, 0.1f };
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("===== doorstress aperture thickness sweep, doorstress =====");
		sb.AppendLine("Each row is a full rebake at that half thickness. 'unclaimed' MUST stay at 0: a baked");
		sb.AppendLine("DOOR poly no runtime box covers is a door that fails open, which is a bug not a saving.");
		sb.AppendLine("| half thickness | cut across wall | total polys | aperture polys | unclaimed | owned by 1 | SHARED by 2+ | neighbours/door | toggle median | reassert-all | bake door us/tile |");
		float[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			float t = (RustNavDoorGates.apertureHalfThickness = array2[i]);
			PooledList<Door> val = Pool.Get<PooledList<Door>>();
			try
			{
				CollectGateDoors(corpusOrigin, corpusRadius, (List<Door>)(object)val);
				foreach (Door item in (List<Door>)(object)val)
				{
					item.RefreshNavAperture();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			RustNavDoorGates.InvalidateCaches();
			bool bakeStatsWereEnabled = RustNav.bakeStatsEnabled;
			RustNav.bakeStatsEnabled = true;
			BakeStats.Reset();
			RustNavigation.Instance.RebuildTilesInBounds(RebakeBounds(corpusOrigin, corpusRadius));
			yield return WaitForTileDrain();
			double ms = BakeStats.StageMs(BakeStats.Stage.CollectDoors);
			long tiles = BakeStats.StageCount(BakeStats.Stage.CollectTotal);
			RustNav.bakeStatsEnabled = bakeStatsWereEnabled;
			RustNavDoorGates.ReassertAll();
			sb.AppendLine(string.Format("| {0:F2} | {1:F2} m | {2} | {3} | {4} | {5} | {6} | {7:F1} | {8:F3} ms | {9:F3} ms | {10} |", new object[11]
			{
				t,
				t * 2f,
				SweepCensus(out var aperturePolys, out var unclaimed, out var owned, out var shared),
				aperturePolys,
				unclaimed,
				owned,
				shared,
				NeighboursPerDoor(),
				SweepToggle(),
				SweepReassert(),
				PerTile(ms, tiles)
			}));
			yield return null;
		}
		RustNavDoorGates.apertureHalfThickness = shipped;
		PooledList<Door> val2 = Pool.Get<PooledList<Door>>();
		try
		{
			CollectGateDoors(corpusOrigin, corpusRadius, (List<Door>)(object)val2);
			foreach (Door item2 in (List<Door>)(object)val2)
			{
				item2.RefreshNavAperture();
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		RustNavDoorGates.InvalidateCaches();
		RustNavigation.Instance.RebuildTilesInBounds(RebakeBounds(corpusOrigin, corpusRadius));
		yield return WaitForTileDrain();
		RustNavDoorGates.ReassertAll();
		sb.AppendLine($"restored to the shipped half thickness {shipped:F2} and rebaked");
		lastSweepReport = sb.ToString();
		Debug.Log((object)lastSweepReport);
	}

	private static string SweepCensus(out int aperturePolys, out int unclaimed, out int owned, out int shared)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		aperturePolys = 0;
		unclaimed = 0;
		owned = 0;
		shared = 0;
		RustNavmesh rustNavmesh = (((Object)(object)RustNavigation.Instance != (Object)null) ? RustNavigation.Instance.DefaultNavmesh : null);
		if (rustNavmesh == null || !rustNavmesh.IsValid())
		{
			return "no navmesh";
		}
		PooledList<Door> val = Pool.Get<PooledList<Door>>();
		try
		{
			CollectGateDoors(corpusOrigin, corpusRadius, (List<Door>)(object)val);
			PooledList<Vector3> val2 = Pool.Get<PooledList<Vector3>>();
			try
			{
				PooledList<Vector2> val3 = Pool.Get<PooledList<Vector2>>();
				try
				{
					PooledList<Vector3> val4 = Pool.Get<PooledList<Vector3>>();
					try
					{
						BuildApertureBoxes((List<Door>)(object)val, (List<Vector3>)(object)val2, (List<Vector2>)(object)val3, (List<Vector3>)(object)val4);
						Bounds val5 = RebakeBounds(corpusOrigin, corpusRadius);
						Vector2Int val6 = rustNavmesh.rcCalcTileCoordFromPos(val5.min);
						Vector2Int val7 = rustNavmesh.rcCalcTileCoordFromPos(val5.max);
						PooledList<Vector3> val8 = Pool.Get<PooledList<Vector3>>();
						try
						{
							PooledList<byte> val9 = Pool.Get<PooledList<byte>>();
							try
							{
								PooledList<ushort> val10 = Pool.Get<PooledList<ushort>>();
								try
								{
									int num = 0;
									for (int i = val6.x; i <= val7.x; i++)
									{
										for (int j = val6.y; j <= val7.y; j++)
										{
											((List<Vector3>)(object)val8).Clear();
											((List<byte>)(object)val9).Clear();
											((List<ushort>)(object)val10).Clear();
											if (!rustNavmesh.GetTilePolysWithStateInternal(i, j, (List<Vector3>)(object)val8, (List<byte>)(object)val9, (List<ushort>)(object)val10))
											{
												continue;
											}
											num += ((List<byte>)(object)val9).Count;
											for (int k = 0; k < ((List<byte>)(object)val9).Count; k++)
											{
												if (((List<byte>)(object)val9)[k] != 3)
												{
													continue;
												}
												aperturePolys++;
												Vector3 val11 = Vector3.zero;
												int num2 = 0;
												for (int l = 0; l < 6; l++)
												{
													Vector3 val12 = ((List<Vector3>)(object)val8)[k * 6 + l];
													if (!(val12 == Vector3.zero))
													{
														num2++;
														val11 += val12;
													}
												}
												if (num2 == 0)
												{
													continue;
												}
												Vector3 val13 = val11 / (float)num2;
												int num3 = 0;
												for (int m = 0; m < ((List<Vector3>)(object)val2).Count; m++)
												{
													if (((List<Vector3>)(object)val4)[m] == Vector3.zero)
													{
														continue;
													}
													Vector3 val14 = val13 - ((List<Vector3>)(object)val2)[m];
													if (!(Mathf.Abs(val14.y) > ((List<Vector3>)(object)val4)[m].y))
													{
														float num4 = val14.x * ((List<Vector2>)(object)val3)[m].x + val14.z * ((List<Vector2>)(object)val3)[m].y;
														float num5 = (0f - val14.x) * ((List<Vector2>)(object)val3)[m].y + val14.z * ((List<Vector2>)(object)val3)[m].x;
														if (Mathf.Abs(num4) <= ((List<Vector3>)(object)val4)[m].x && Mathf.Abs(num5) <= ((List<Vector3>)(object)val4)[m].z)
														{
															num3++;
														}
													}
												}
												switch (num3)
												{
												case 0:
													unclaimed++;
													break;
												case 1:
													owned++;
													break;
												default:
													shared++;
													break;
												}
											}
										}
									}
									return num.ToString();
								}
								finally
								{
									((IDisposable)val10)?.Dispose();
								}
							}
							finally
							{
								((IDisposable)val9)?.Dispose();
							}
						}
						finally
						{
							((IDisposable)val8)?.Dispose();
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

	private static void BuildApertureBoxes(List<Door> doors, List<Vector3> centers, List<Vector2> axes, List<Vector3> halfs)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		foreach (Door door in doors)
		{
			if (!door.TryGetNavDoorwayVolume(out var apertureLocal, out var localToWorld))
			{
				centers.Add(Vector3.zero);
				axes.Add(Vector2.right);
				halfs.Add(Vector3.zero);
				continue;
			}
			Vector3 right = ((Component)door).transform.right;
			Vector2 item = new Vector2(right.x, right.z);
			if (item.sqrMagnitude < 0.0001f)
			{
				item = Vector2.right;
			}
			item.Normalize();
			Vector3 lossyScale = ((Component)door).transform.lossyScale;
			centers.Add(localToWorld.MultiplyPoint3x4(apertureLocal.center));
			axes.Add(item);
			halfs.Add(new Vector3(apertureLocal.extents.x * Mathf.Abs(lossyScale.x), apertureLocal.extents.y * Mathf.Abs(lossyScale.y), apertureLocal.extents.z * Mathf.Abs(lossyScale.z)) + new Vector3(0.1f, 0.1f, 0.1f));
		}
	}

	private static float NeighboursPerDoor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		PooledList<Door> val = Pool.Get<PooledList<Door>>();
		try
		{
			CollectGateDoors(corpusOrigin, corpusRadius, (List<Door>)(object)val);
			if (((List<Door>)(object)val).Count == 0)
			{
				return 0f;
			}
			PooledList<Bounds> val2 = Pool.Get<PooledList<Bounds>>();
			try
			{
				foreach (Door item2 in (List<Door>)(object)val)
				{
					if (!item2.TryGetNavDoorwayVolume(out var apertureLocal, out var localToWorld))
					{
						((List<Bounds>)(object)val2).Add(new Bounds(new Vector3(1E+09f, 1E+09f, 1E+09f), Vector3.zero));
						continue;
					}
					Bounds item = BoundsEx.Transform(apertureLocal, localToWorld);
					if (item2.TryGetNavLeafRegion(out var leafRegionLocal, out var localToWorld2))
					{
						item.Encapsulate(BoundsEx.Transform(leafRegionLocal, localToWorld2));
					}
					item.Expand(2f);
					((List<Bounds>)(object)val2).Add(item);
				}
				long num = 0L;
				for (int i = 0; i < ((List<Bounds>)(object)val2).Count; i++)
				{
					for (int j = 0; j < ((List<Bounds>)(object)val2).Count; j++)
					{
						if (i != j)
						{
							Bounds val3 = ((List<Bounds>)(object)val2)[i];
							if (val3.Intersects(((List<Bounds>)(object)val2)[j]))
							{
								num++;
							}
						}
					}
				}
				return (float)num / (float)((List<Door>)(object)val).Count;
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

	private static double SweepToggle()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		PooledList<Door> val = Pool.Get<PooledList<Door>>();
		try
		{
			CollectGateDoors(corpusOrigin, corpusRadius, (List<Door>)(object)val);
			if (((List<Door>)(object)val).Count == 0)
			{
				return 0.0;
			}
			TogglePassRaw((List<Door>)(object)val, null);
			PooledList<double> val2 = Pool.Get<PooledList<double>>();
			try
			{
				TogglePassRaw((List<Door>)(object)val, (List<double>)(object)val2);
				((List<double>)(object)val2).Sort();
				return (((List<double>)(object)val2).Count > 0) ? ((List<double>)(object)val2)[((List<double>)(object)val2).Count / 2] : 0.0;
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

	private static double SweepReassert()
	{
		Stopwatch stopwatch = new Stopwatch();
		double num = double.MaxValue;
		for (int i = 0; i < 3; i++)
		{
			stopwatch.Restart();
			RustNavDoorGates.ReassertAll();
			stopwatch.Stop();
			num = Math.Min(num, stopwatch.Elapsed.TotalMilliseconds);
		}
		return num;
	}

	[ServerVar(EditorOnly = true, Help = "Price the no-slabs + thin-aperture + no-ownership proposal against shipped, step by step: doorstress.proposalbench <reps, default 3>")]
	public static void proposalbench(Arg arg)
	{
		if (EnsureBackend(arg))
		{
			if (!envBuilt)
			{
				arg.ReplyWith("run doorstress.build_env first");
				return;
			}
			int num = Mathf.Clamp(arg.GetInt(0, 3), 1, 10);
			((MonoBehaviour)Global.Runner).StartCoroutine(ProposalRoutine(num));
			arg.ReplyWith($"doorstress proposal bench started, {num} reps averaged, results via doorstress.proposalresults");
		}
	}

	[ServerVar(EditorOnly = true, Help = "Print the last proposal bench results")]
	public static void proposalresults(Arg arg)
	{
		arg.ReplyWith(lastProposalReport);
	}

	private static IEnumerator ProposalRoutine(int reps)
	{
		List<ProposalArm> arms = new List<ProposalArm>
		{
			new ProposalArm
			{
				name = "A shipped (slabs, 0.50, ownership, scan)",
				slabs = true,
				thickness = 0.5f,
				ownership = true,
				refCache = false
			},
			new ProposalArm
			{
				name = "B no slabs",
				slabs = false,
				thickness = 0.5f,
				ownership = true,
				refCache = false
			},
			new ProposalArm
			{
				name = "C no slabs + thin 0.10",
				slabs = false,
				thickness = 0.1f,
				ownership = true,
				refCache = false
			},
			new ProposalArm
			{
				name = "D no slabs + thin + NO ownership",
				slabs = false,
				thickness = 0.1f,
				ownership = false,
				refCache = false
			},
			new ProposalArm
			{
				name = "E shipped + POLY REF CACHE",
				slabs = true,
				thickness = 0.5f,
				ownership = true,
				refCache = true
			},
			new ProposalArm
			{
				name = "F no slabs + POLY REF CACHE",
				slabs = false,
				thickness = 0.5f,
				ownership = true,
				refCache = true
			},
			new ProposalArm
			{
				name = "A again (drift check)",
				slabs = true,
				thickness = 0.5f,
				ownership = true,
				refCache = false
			}
		};
		for (int i = 0; i < arms.Count; i++)
		{
			ProposalArm arm = arms[i];
			yield return RunProposalArm(reps, (ProposalArm r) =>
			{
				arm = r;
			}, arm);
			arms[i] = arm;
		}
		RustNavDoorGates.leafSlabsEnabled = true;
		RustNavDoorGates.apertureHalfThickness = 0.5f;
		RustNavDoorGates.ownershipEnabled = true;
		RustNavDoorGates.polyRefCacheEnabled = true;
		RefreshApertures();
		RustNavDoorGates.InvalidateCaches();
		RustNavigation.Instance.RebuildTilesInBounds(RebakeBounds(corpusOrigin, corpusRadius));
		yield return WaitForTileDrain();
		RustNavDoorGates.ReassertAll();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(string.Format("===== doorstress proposal bench, {0}, {1} reps averaged =====", "doorstress", reps));
		stringBuilder.AppendLine("PER DOOR / PER TILE. 'unclaimed' must be 0 or doors fail open. 'shared' > 0 with");
		stringBuilder.AppendLine("ownership off means two doors gate as one, which is a correctness break not a cost.");
		stringBuilder.AppendLine("| arm | toggle/door | gather/door | write/door | register/door | unregister/door | reassert/door | bake door us/tile | aperture polys/door | total polys | unclaimed | shared | neighbours/door |");
		foreach (ProposalArm item in arms)
		{
			stringBuilder.AppendLine(string.Format("| {0} | {1:F4} ms | {2:F2} us | {3:F2} us | {4:F4} ms | {5:F4} ms | {6:F2} us | {7:F1} us | {8:F2} | {9} | {10} | {11} | {12:F1} |", new object[13]
			{
				item.name,
				item.toggle,
				item.gatherUs,
				item.writeUs,
				item.register,
				item.unregister,
				(item.doors > 0) ? (item.reassert * 1000.0 / (double)item.doors) : 0.0,
				item.bakeDoorUsPerTile,
				(item.doors > 0) ? ((float)item.aperturePolys / (float)item.doors) : 0f,
				item.totalPolys,
				item.unclaimed,
				item.shared,
				item.neighbours
			}));
		}
		stringBuilder.AppendLine("reassert-all whole corpus, for reference: " + string.Join(", ", arms.ConvertAll((ProposalArm a) => $"{a.name.Substring(0, 1)}={a.reassert:F3} ms")));
		stringBuilder.AppendLine("restored to shipped settings and rebaked");
		lastProposalReport = stringBuilder.ToString();
		Debug.Log((object)lastProposalReport);
	}

	private static void RefreshApertures()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		PooledList<Door> val = Pool.Get<PooledList<Door>>();
		try
		{
			CollectGateDoors(corpusOrigin, corpusRadius, (List<Door>)(object)val);
			foreach (Door item in (List<Door>)(object)val)
			{
				item.RefreshNavAperture();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static IEnumerator RunProposalArm(int reps, Action<ProposalArm> report, ProposalArm arm)
	{
		RustNavDoorGates.leafSlabsEnabled = arm.slabs;
		RustNavDoorGates.apertureHalfThickness = arm.thickness;
		RustNavDoorGates.ownershipEnabled = arm.ownership;
		RustNavDoorGates.polyRefCacheEnabled = arm.refCache;
		RefreshApertures();
		RustNavDoorGates.InvalidateCaches();
		bool bakeStatsWereEnabled = RustNav.bakeStatsEnabled;
		RustNav.bakeStatsEnabled = true;
		BakeStats.Reset();
		RustNavigation.Instance.RebuildTilesInBounds(RebakeBounds(corpusOrigin, corpusRadius));
		yield return WaitForTileDrain();
		double num = BakeStats.StageMs(BakeStats.Stage.CollectDoors);
		long num2 = BakeStats.StageCount(BakeStats.Stage.CollectTotal);
		arm.bakeDoorUsPerTile = ((num2 > 0) ? (num * 1000.0 / (double)num2) : 0.0);
		RustNav.bakeStatsEnabled = bakeStatsWereEnabled;
		RustNavDoorGates.ReassertAll();
		arm.totalPolys = int.Parse(SweepCensus(out var aperturePolys, out var unclaimed, out var _, out var shared));
		arm.aperturePolys = aperturePolys;
		arm.unclaimed = unclaimed;
		arm.shared = shared;
		arm.neighbours = NeighboursPerDoor();
		arm.leafPolys = LeafPolyCount();
		PooledList<Door> doors = Pool.Get<PooledList<Door>>();
		try
		{
			CollectGateDoors(corpusOrigin, corpusRadius, (List<Door>)(object)doors);
			arm.doors = ((List<Door>)(object)doors).Count;
			double toggleSum = 0.0;
			double gatherSum = 0.0;
			double writeSum = 0.0;
			double reassertSum = 0.0;
			double regSum = 0.0;
			double unregSum = 0.0;
			for (int rep = 0; rep < reps; rep++)
			{
				TogglePassRaw((List<Door>)(object)doors, null);
				PooledList<double> val = Pool.Get<PooledList<double>>();
				try
				{
					RustNavDoorGates.Profile.Reset();
					RustNavDoorGates.Profile.enabled = true;
					TogglePassRaw((List<Door>)(object)doors, (List<double>)(object)val);
					RustNavDoorGates.Profile.enabled = false;
					((List<double>)(object)val).Sort();
					toggleSum += ((((List<double>)(object)val).Count > 0) ? ((List<double>)(object)val)[((List<double>)(object)val).Count / 2] : 0.0);
					gatherSum += ((((List<Door>)(object)doors).Count > 0) ? (RustNavDoorGates.Profile.GatherMs * 1000.0 / (double)((List<Door>)(object)doors).Count) : 0.0);
					writeSum += ((((List<Door>)(object)doors).Count > 0) ? (RustNavDoorGates.Profile.WriteMs * 1000.0 / (double)((List<Door>)(object)doors).Count) : 0.0);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Restart();
				RustNavDoorGates.ReassertAll();
				stopwatch.Stop();
				reassertSum += stopwatch.Elapsed.TotalMilliseconds;
				int num3 = Mathf.Max(1, ((List<Door>)(object)doors).Count / 60);
				int num4 = 0;
				double num5 = 0.0;
				double num6 = 0.0;
				for (int i = 0; i < ((List<Door>)(object)doors).Count; i += num3)
				{
					Door door = ((List<Door>)(object)doors)[i];
					if (!((Object)(object)door == (Object)null) && !door.IsDestroyed)
					{
						stopwatch.Restart();
						RustNavDoorGates.Unregister(door);
						stopwatch.Stop();
						num6 += stopwatch.Elapsed.TotalMilliseconds;
						stopwatch.Restart();
						RustNavDoorGates.Register(door);
						stopwatch.Stop();
						num5 += stopwatch.Elapsed.TotalMilliseconds;
						num4++;
					}
				}
				regSum += ((num4 > 0) ? (num5 / (double)num4) : 0.0);
				unregSum += ((num4 > 0) ? (num6 / (double)num4) : 0.0);
				yield return null;
			}
			arm.toggle = toggleSum / (double)reps;
			arm.gatherUs = gatherSum / (double)reps;
			arm.writeUs = writeSum / (double)reps;
			arm.reassert = reassertSum / (double)reps;
			arm.register = regSum / (double)reps;
			arm.unregister = unregSum / (double)reps;
			report(arm);
		}
		finally
		{
			((IDisposable)doors)?.Dispose();
		}
	}

	private static int LeafPolyCount()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		RustNavmesh rustNavmesh = (((Object)(object)RustNavigation.Instance != (Object)null) ? RustNavigation.Instance.DefaultNavmesh : null);
		if (rustNavmesh == null || !rustNavmesh.IsValid())
		{
			return 0;
		}
		Bounds val = RebakeBounds(corpusOrigin, corpusRadius);
		Vector2Int val2 = rustNavmesh.rcCalcTileCoordFromPos(val.min);
		Vector2Int val3 = rustNavmesh.rcCalcTileCoordFromPos(val.max);
		PooledList<Vector3> val4 = Pool.Get<PooledList<Vector3>>();
		try
		{
			PooledList<byte> val5 = Pool.Get<PooledList<byte>>();
			try
			{
				PooledList<ushort> val6 = Pool.Get<PooledList<ushort>>();
				try
				{
					int num = 0;
					for (int i = val2.x; i <= val3.x; i++)
					{
						for (int j = val2.y; j <= val3.y; j++)
						{
							((List<Vector3>)(object)val4).Clear();
							((List<byte>)(object)val5).Clear();
							((List<ushort>)(object)val6).Clear();
							if (!rustNavmesh.GetTilePolysWithStateInternal(i, j, (List<Vector3>)(object)val4, (List<byte>)(object)val5, (List<ushort>)(object)val6))
							{
								continue;
							}
							for (int k = 0; k < ((List<byte>)(object)val5).Count; k++)
							{
								if (((List<byte>)(object)val5)[k] == 6)
								{
									num++;
								}
							}
						}
					}
					return num;
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
		finally
		{
			((IDisposable)val4)?.Dispose();
		}
	}

	[ServerVar(EditorOnly = true, Help = "Audit every pasted base for door gating anomalies: doorstress.audit")]
	public static void audit(Arg arg)
	{
		if (EnsureBackend(arg))
		{
			if (!envBuilt)
			{
				arg.ReplyWith("run doorstress.build_env first");
				return;
			}
			((MonoBehaviour)Global.Runner).StartCoroutine(AuditRoutine());
			arg.ReplyWith("doorstress audit started, results via doorstress.auditresults");
		}
	}

	[ServerVar(EditorOnly = true, Help = "Print the last audit results")]
	public static void auditresults(Arg arg)
	{
		arg.ReplyWith(lastAuditReport);
	}

	private static IEnumerator AuditRoutine()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine($"===== doorstress audit, {CorpusBases.Length} bases =====");
		int totalDoors = 0;
		int totalNoPoly = 0;
		int totalMarking = 0;
		int totalPruned = 0;
		int totalDrop = 0;
		int totalNoFloor = 0;
		int totalShared = 0;
		int totalNotGate = 0;
		int totalSplit = 0;
		sb.AppendLine($"minRegionSizeMeters {RustNavigation.Instance.DefaultNavmesh.BuildParamsHiRes.minRegionSizeMeters} (hi res, what bases bake with)");
		sb.AppendLine("| base | gate doors | not gated | 0 door polys | MARKING FAILED | island pruned | drop behind | split level | no floor | shared polys | off axis |");
		foreach (BaseSlot corpusSlot in corpusSlots)
		{
			if (corpusSlot.entities == 0)
			{
				sb.AppendLine("| " + corpusSlot.name + " | - | - | - | - | - | - | - | - | - | (not pasted: " + corpusSlot.note + ") |");
				continue;
			}
			PooledList<Door> doors = Pool.Get<PooledList<Door>>();
			try
			{
				CollectSlotGateDoors(corpusSlot, (List<Door>)(object)doors);
				int num = CountSlotNonGateDoors(corpusSlot);
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				int num5 = 0;
				int num6 = 0;
				int num7 = 0;
				foreach (Door item in (List<Door>)(object)doors)
				{
					if (Mathf.Abs(Mathf.Repeat(((Component)item).transform.eulerAngles.y + 45f, 90f) - 45f) > 5f)
					{
						num6++;
					}
					switch (ClassifyDoor(item))
					{
					case NoPolyCause.MarkingFailed:
						num2++;
						break;
					case NoPolyCause.IslandPruned:
						num3++;
						break;
					case NoPolyCause.DropBehind:
						num4++;
						break;
					case NoPolyCause.SplitLevel:
						num7++;
						break;
					case NoPolyCause.NoFloorAtAll:
						num5++;
						break;
					}
				}
				int num8 = num2 + num3 + num4 + num5 + num7;
				int num9 = SharedPolysInSlot(corpusSlot);
				sb.AppendLine(string.Format("| {0} | {1} | {2} | {3} | **{4}** | {5} | {6} | {7} | {8} | {9} | {10} |", new object[11]
				{
					corpusSlot.name,
					((List<Door>)(object)doors).Count,
					num,
					num8,
					num2,
					num3,
					num4,
					num7,
					num5,
					num9,
					num6
				}));
				totalDoors += ((List<Door>)(object)doors).Count;
				totalNotGate += num;
				totalNoPoly += num8;
				totalMarking += num2;
				totalPruned += num3;
				totalDrop += num4;
				totalNoFloor += num5;
				totalSplit += num7;
				totalShared += num9;
				yield return null;
			}
			finally
			{
				((IDisposable)doors)?.Dispose();
			}
		}
		sb.AppendLine(string.Format("| TOTAL | {0} | {1} | {2} | **{3}** | {4} | {5} | {6} | {7} | {8} | |", new object[9] { totalDoors, totalNotGate, totalNoPoly, totalMarking, totalPruned, totalDrop, totalSplit, totalNoFloor, totalShared }));
		sb.AppendLine("MARKING FAILED is the only column that is a bug: floor and navmesh on BOTH sides of the");
		sb.AppendLine("doorway, yet no DOOR area poly was baked, so the door has nothing to flip and never blocks.");
		lastAuditReport = sb.ToString();
		Debug.Log((object)lastAuditReport);
	}

	private static NoPolyCause ClassifyDoor(Door door)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		RustNavmesh defaultNavmesh = RustNavigation.Instance.DefaultNavmesh;
		if (!door.TryGetNavDoorwayVolume(out var apertureLocal, out var localToWorld))
		{
			return NoPolyCause.HasPolys;
		}
		if (DoorPolyCount(door) > 0)
		{
			return NoPolyCause.HasPolys;
		}
		bool flag = apertureLocal.extents.x <= apertureLocal.extents.z;
		Vector3 val = localToWorld.MultiplyVector(flag ? Vector3.right : Vector3.forward);
		Vector3 normalized = val.normalized;
		Vector3 val2 = localToWorld.MultiplyPoint3x4(new Vector3(apertureLocal.center.x, apertureLocal.min.y + 0.6f, apertureLocal.center.z));
		int num = 0;
		int num2 = 0;
		float num3 = 0f;
		float num4 = 0f;
		RaycastHit val4 = default;
		for (int i = -1; i <= 1; i += 2)
		{
			Vector3 val3 = val2 + normalized * (1.1f * (float)i);
			if (Physics.Raycast(val3 + Vector3.up * 1.5f, Vector3.down, ref val4, 4f, LayerMask.GetMask(new string[4] { "World", "Terrain", "Default", "Construction" })))
			{
				num++;
				if (i < 0)
				{
					num3 = val4.point.y;
				}
				else
				{
					num4 = val4.point.y;
				}
			}
			if (defaultNavmesh.SamplePosition(new NavVector3(val3), out var _, new Vector3(1.5f, 1.5f, 1.5f)))
			{
				num2++;
			}
		}
		switch (num)
		{
		case 0:
			return NoPolyCause.NoFloorAtAll;
		case 1:
			return NoPolyCause.DropBehind;
		default:
			if (Mathf.Abs(num3 - num4) > RustNavigation.Instance.DefaultNavmesh.BuildParamsHiRes.agentMaxClimb)
			{
				return NoPolyCause.SplitLevel;
			}
			if (num2 != 2)
			{
				return NoPolyCause.IslandPruned;
			}
			return NoPolyCause.MarkingFailed;
		}
	}

	private static int DoorPolyCount(Door door)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		RustNavmesh defaultNavmesh = RustNavigation.Instance.DefaultNavmesh;
		if (defaultNavmesh == null || !defaultNavmesh.IsValid() || !door.TryGetNavDoorwayVolume(out var apertureLocal, out var localToWorld))
		{
			return 0;
		}
		Vector3 right = ((Component)door).transform.right;
		Vector2 val = new Vector2(right.x, right.z);
		if (val.sqrMagnitude < 0.0001f)
		{
			val = Vector2.right;
		}
		val.Normalize();
		Vector3 lossyScale = ((Component)door).transform.lossyScale;
		Vector3 val2 = new Vector3(apertureLocal.extents.x * Mathf.Abs(lossyScale.x), apertureLocal.extents.y * Mathf.Abs(lossyScale.y), apertureLocal.extents.z * Mathf.Abs(lossyScale.z)) + new Vector3(0.1f, 0.1f, 0.1f);
		Vector3 val3 = localToWorld.MultiplyPoint3x4(apertureLocal.center);
		PooledList<Vector3> val4 = Pool.Get<PooledList<Vector3>>();
		try
		{
			PooledList<byte> val5 = Pool.Get<PooledList<byte>>();
			try
			{
				PooledList<ushort> val6 = Pool.Get<PooledList<ushort>>();
				try
				{
					Vector2Int val7 = defaultNavmesh.rcCalcTileCoordFromPos(val3 - Vector3.one * 4f);
					Vector2Int val8 = defaultNavmesh.rcCalcTileCoordFromPos(val3 + Vector3.one * 4f);
					int num = 0;
					for (int i = val7.x; i <= val8.x; i++)
					{
						for (int j = val7.y; j <= val8.y; j++)
						{
							((List<Vector3>)(object)val4).Clear();
							((List<byte>)(object)val5).Clear();
							((List<ushort>)(object)val6).Clear();
							if (!defaultNavmesh.GetTilePolysWithStateInternal(i, j, (List<Vector3>)(object)val4, (List<byte>)(object)val5, (List<ushort>)(object)val6))
							{
								continue;
							}
							for (int k = 0; k < ((List<byte>)(object)val5).Count; k++)
							{
								if (((List<byte>)(object)val5)[k] != 3)
								{
									continue;
								}
								Vector3 val9 = Vector3.zero;
								int num2 = 0;
								for (int l = 0; l < 6; l++)
								{
									Vector3 val10 = ((List<Vector3>)(object)val4)[k * 6 + l];
									if (!(val10 == Vector3.zero))
									{
										num2++;
										val9 += val10;
									}
								}
								if (num2 == 0)
								{
									continue;
								}
								Vector3 val11 = val9 / (float)num2 - val3;
								if (!(Mathf.Abs(val11.y) > val2.y))
								{
									float num3 = val11.x * val.x + val11.z * val.y;
									float num4 = (0f - val11.x) * val.y + val11.z * val.x;
									if (Mathf.Abs(num3) <= val2.x && Mathf.Abs(num4) <= val2.z)
									{
										num++;
									}
								}
							}
						}
					}
					return num;
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
		finally
		{
			((IDisposable)val4)?.Dispose();
		}
	}

	private static int SharedPolysInSlot(BaseSlot slot)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		PooledList<Door> val = Pool.Get<PooledList<Door>>();
		try
		{
			CollectGateDoors(slot.origin, slot.radius, (List<Door>)(object)val);
			if (((List<Door>)(object)val).Count == 0)
			{
				return 0;
			}
			PooledList<Vector3> val2 = Pool.Get<PooledList<Vector3>>();
			try
			{
				PooledList<Vector2> val3 = Pool.Get<PooledList<Vector2>>();
				try
				{
					PooledList<Vector3> val4 = Pool.Get<PooledList<Vector3>>();
					try
					{
						BuildApertureBoxes((List<Door>)(object)val, (List<Vector3>)(object)val2, (List<Vector2>)(object)val3, (List<Vector3>)(object)val4);
						RustNavmesh defaultNavmesh = RustNavigation.Instance.DefaultNavmesh;
						Bounds val5 = RebakeBounds(slot.origin, slot.radius);
						Vector2Int val6 = defaultNavmesh.rcCalcTileCoordFromPos(val5.min);
						Vector2Int val7 = defaultNavmesh.rcCalcTileCoordFromPos(val5.max);
						PooledList<Vector3> val8 = Pool.Get<PooledList<Vector3>>();
						try
						{
							PooledList<byte> val9 = Pool.Get<PooledList<byte>>();
							try
							{
								PooledList<ushort> val10 = Pool.Get<PooledList<ushort>>();
								try
								{
									int num = 0;
									for (int i = val6.x; i <= val7.x; i++)
									{
										for (int j = val6.y; j <= val7.y; j++)
										{
											((List<Vector3>)(object)val8).Clear();
											((List<byte>)(object)val9).Clear();
											((List<ushort>)(object)val10).Clear();
											if (!defaultNavmesh.GetTilePolysWithStateInternal(i, j, (List<Vector3>)(object)val8, (List<byte>)(object)val9, (List<ushort>)(object)val10))
											{
												continue;
											}
											for (int k = 0; k < ((List<byte>)(object)val9).Count; k++)
											{
												if (((List<byte>)(object)val9)[k] != 3)
												{
													continue;
												}
												Vector3 val11 = Vector3.zero;
												int num2 = 0;
												for (int l = 0; l < 6; l++)
												{
													Vector3 val12 = ((List<Vector3>)(object)val8)[k * 6 + l];
													if (!(val12 == Vector3.zero))
													{
														num2++;
														val11 += val12;
													}
												}
												if (num2 == 0)
												{
													continue;
												}
												Vector3 val13 = val11 / (float)num2;
												int num3 = 0;
												for (int m = 0; m < ((List<Vector3>)(object)val2).Count; m++)
												{
													if (((List<Vector3>)(object)val4)[m] == Vector3.zero)
													{
														continue;
													}
													Vector3 val14 = val13 - ((List<Vector3>)(object)val2)[m];
													if (!(Mathf.Abs(val14.y) > ((List<Vector3>)(object)val4)[m].y))
													{
														float num4 = val14.x * ((List<Vector2>)(object)val3)[m].x + val14.z * ((List<Vector2>)(object)val3)[m].y;
														float num5 = (0f - val14.x) * ((List<Vector2>)(object)val3)[m].y + val14.z * ((List<Vector2>)(object)val3)[m].x;
														if (Mathf.Abs(num4) <= ((List<Vector3>)(object)val4)[m].x && Mathf.Abs(num5) <= ((List<Vector3>)(object)val4)[m].z)
														{
															num3++;
														}
													}
												}
												if (num3 >= 2)
												{
													num++;
												}
											}
										}
									}
									return num;
								}
								finally
								{
									((IDisposable)val10)?.Dispose();
								}
							}
							finally
							{
								((IDisposable)val9)?.Dispose();
							}
						}
						finally
						{
							((IDisposable)val8)?.Dispose();
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

	private static void CollectSlotGateDoors(BaseSlot slot, List<Door> results)
	{
		int num = Mathf.Min(slot.entityStart + slot.entities, corpusEntities.Count);
		for (int i = slot.entityStart; i < num; i++)
		{
			Door door = corpusEntities[i] as Door;
			if ((Object)(object)door != (Object)null && !door.IsDestroyed && door.IsNavGate)
			{
				results.Add(door);
			}
		}
	}

	private static int CountSlotNonGateDoors(BaseSlot slot)
	{
		int num = Mathf.Min(slot.entityStart + slot.entities, corpusEntities.Count);
		int num2 = 0;
		for (int i = slot.entityStart; i < num; i++)
		{
			Door door = corpusEntities[i] as Door;
			if ((Object)(object)door != (Object)null && !door.IsDestroyed && !door.IsNavGate)
			{
				num2++;
			}
		}
		return num2;
	}

	private static int CountNonGateDoors(Vector3 origin, float radius)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		float num = radius * radius;
		int num2 = 0;
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Door door = enumerator.Current as Door;
				if (!((Object)(object)door == (Object)null) && !door.IsDestroyed && !door.IsNavGate)
				{
					Vector3 val = ((Component)door).transform.position - origin;
					if (val.sqrMagnitude <= num)
					{
						num2++;
					}
				}
			}
			return num2;
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ServerVar(EditorOnly = true, Help = "Rebake the pasted base at a range of minRegionSizeMeters and census doors and polys at each: doorstress.prunesweep")]
	public static void prunesweep(Arg arg)
	{
		if (EnsureBackend(arg))
		{
			if (!envBuilt)
			{
				arg.ReplyWith("run doorstress.build_env <baseName> first");
				return;
			}
			((MonoBehaviour)Global.Runner).StartCoroutine(PruneSweepRoutine());
			arg.ReplyWith("doorstress prune sweep started, results via doorstress.pruneresults");
		}
	}

	[ServerVar(EditorOnly = true, Help = "Print the last prune sweep results")]
	public static void pruneresults(Arg arg)
	{
		arg.ReplyWith(lastPruneReport);
	}

	private static IEnumerator PruneSweepRoutine()
	{
		RustNavigation navigation = RustNavigation.Instance;
		float shippedLo = navigation.DefaultNavmesh.BuildParams.minRegionSizeMeters;
		float shippedHi = navigation.DefaultNavmesh.BuildParamsHiRes.minRegionSizeMeters;
		float[] array = new float[7] { 0f, 20f, 10f, 5.76f, 2f, 1f, 0f };
		array[0] = shippedHi;
		float[] array2 = array;
		StringBuilder sb = new StringBuilder();
		sb.AppendLine($"===== doorstress prune sweep, shipped hi res minRegionSizeMeters {shippedHi} =====");
		sb.AppendLine("Recast prunes any walkable region smaller than this. Lower keeps more small islands.");
		sb.AppendLine("| minRegion m2 | total polys | aperture polys | gate doors | 0 door polys | MARKING FAILED | island pruned | drop behind | split level | bake worker ms | bake main ms |");
		float[] array3 = array2;
		foreach (float v in array3)
		{
			NavMeshBuildParams buildParams = navigation.DefaultNavmesh.BuildParams;
			buildParams.minRegionSizeMeters = v;
			navigation.DefaultNavmesh.BuildParams = buildParams;
			NavMeshBuildParams buildParamsHiRes = navigation.DefaultNavmesh.BuildParamsHiRes;
			buildParamsHiRes.minRegionSizeMeters = v;
			navigation.DefaultNavmesh.BuildParamsHiRes = buildParamsHiRes;
			RustNavDoorGates.InvalidateCaches();
			bool bakeStatsWereEnabled = RustNav.bakeStatsEnabled;
			RustNav.bakeStatsEnabled = true;
			BakeStats.Reset();
			navigation.RebuildTilesInBounds(RebakeBounds(corpusOrigin, corpusRadius));
			yield return WaitForTileDrain();
			double num = BakeStats.StageMs(BakeStats.Stage.WorkerTotal);
			double num2 = BakeStats.StageMs(BakeStats.Stage.CollectTotal) + BakeStats.StageMs(BakeStats.Stage.MainAddTile);
			RustNav.bakeStatsEnabled = bakeStatsWereEnabled;
			RustNavDoorGates.ReassertAll();
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			int num8 = 0;
			foreach (BaseSlot corpusSlot in corpusSlots)
			{
				if (corpusSlot.entities == 0)
				{
					continue;
				}
				PooledList<Door> val = Pool.Get<PooledList<Door>>();
				try
				{
					CollectSlotGateDoors(corpusSlot, (List<Door>)(object)val);
					num8 += ((List<Door>)(object)val).Count;
					foreach (Door item in (List<Door>)(object)val)
					{
						switch (ClassifyDoor(item))
						{
						case NoPolyCause.MarkingFailed:
							num3++;
							break;
						case NoPolyCause.IslandPruned:
							num4++;
							break;
						case NoPolyCause.DropBehind:
							num5++;
							break;
						case NoPolyCause.SplitLevel:
							num6++;
							break;
						case NoPolyCause.NoFloorAtAll:
							num7++;
							break;
						}
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			string text = SweepCensus(out var aperturePolys, out var _, out var _, out var _);
			int num9 = num3 + num4 + num5 + num6 + num7;
			sb.AppendLine(string.Format("| {0} | {1} | {2} | {3} | {4} | **{5}** | {6} | {7} | {8} | {9:F0} ms | {10:F0} ms |", new object[11]
			{
				v, text, aperturePolys, num8, num9, num3, num4, num5, num6, num,
				num2
			}));
			yield return null;
		}
		NavMeshBuildParams buildParams2 = navigation.DefaultNavmesh.BuildParams;
		buildParams2.minRegionSizeMeters = shippedLo;
		navigation.DefaultNavmesh.BuildParams = buildParams2;
		NavMeshBuildParams buildParamsHiRes2 = navigation.DefaultNavmesh.BuildParamsHiRes;
		buildParamsHiRes2.minRegionSizeMeters = shippedHi;
		navigation.DefaultNavmesh.BuildParamsHiRes = buildParamsHiRes2;
		RustNavDoorGates.InvalidateCaches();
		navigation.RebuildTilesInBounds(RebakeBounds(corpusOrigin, corpusRadius));
		yield return WaitForTileDrain();
		RustNavDoorGates.ReassertAll();
		sb.AppendLine($"restored to the shipped {shippedHi} and rebaked");
		lastPruneReport = sb.ToString();
		Debug.Log((object)lastPruneReport);
	}

	[ServerVar(EditorOnly = true, Help = "Final door gating comparison over every pasted base: doorstress.finalbench <reps, default 3>")]
	public static void finalbench(Arg arg)
	{
		if (EnsureBackend(arg))
		{
			if (!envBuilt)
			{
				arg.ReplyWith("run doorstress.build_env all first");
				return;
			}
			int num = Mathf.Clamp(arg.GetInt(0, 3), 1, 10);
			((MonoBehaviour)Global.Runner).StartCoroutine(FinalRoutine(num));
			arg.ReplyWith($"doorstress final bench started over every pasted base, {num} reps averaged, results via doorstress.finalresults");
		}
	}

	[ServerVar(EditorOnly = true, Help = "Print the last final bench results")]
	public static void finalresults(Arg arg)
	{
		arg.ReplyWith(lastFinalReport);
	}

	private static IEnumerator FinalRoutine(int reps)
	{
		List<FinalArm> arms = new List<FinalArm>
		{
			new FinalArm
			{
				name = "A previously shipped (scan)",
				gates = true,
				refCache = false
			},
			new FinalArm
			{
				name = "B shipped + POLY REF CACHE",
				gates = true,
				refCache = true
			},
			new FinalArm
			{
				name = "C door gating OFF (legacy)",
				gates = false,
				refCache = false
			},
			new FinalArm
			{
				name = "A again (drift check)",
				gates = true,
				refCache = false
			}
		};
		for (int i = 0; i < arms.Count; i++)
		{
			FinalArm arm = arms[i];
			yield return RunFinalArm(reps, arm, (FinalArm r) =>
			{
				arm = r;
			});
			arms[i] = arm;
		}
		RustNavDoorGates.gatesEnabled = true;
		RustNavDoorGates.polyRefCacheEnabled = true;
		RustNavDoorGates.InvalidateCaches();
		RustNavigation.Instance.RebuildTilesInBounds(RebakeBounds(corpusOrigin, corpusRadius));
		yield return WaitForTileDrain();
		RustNavDoorGates.ReassertAll();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"===== doorstress FINAL bench, {corpusSlots.Count} bases, {reps} reps averaged =====");
		foreach (BaseSlot corpusSlot in corpusSlots)
		{
			stringBuilder.Append($"{corpusSlot.name}({corpusSlot.gateDoors}) ");
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("PER DOOR and PER TILE. Arm C is the legacy behaviour, closed doors rasterize solid.");
		stringBuilder.AppendLine("| arm | toggle/door | gather/door | write/door | register/door | unregister/door | reassert/door | reassert all | bake door us/tile | bake main | bake worker | polys | navmesh save | navmesh load |");
		foreach (FinalArm item in arms)
		{
			double num = ((item.doors > 0) ? (item.reassert * 1000.0 / (double)item.doors) : 0.0);
			stringBuilder.AppendLine(string.Format("| {0} | {1:F4} ms | {2:F2} us | {3:F2} us | {4:F4} ms | {5:F4} ms | {6:F2} us | {7:F2} ms | {8:F1} us | {9:F0} ms | {10:F0} ms | {11} | {12:F1} ms / {13} KB | {14:F1} ms |", new object[15]
			{
				item.name,
				item.toggle,
				item.gatherUs,
				item.writeUs,
				item.register,
				item.unregister,
				num,
				item.reassert,
				item.bakeDoorUsPerTile,
				item.bakeMain,
				item.bakeWorker,
				item.totalPolys,
				item.saveMs,
				item.saveBytes / 1024,
				item.loadMs
			}));
		}
		lastFinalReport = stringBuilder.ToString();
		Debug.Log((object)lastFinalReport);
	}

	private static IEnumerator RunFinalArm(int reps, FinalArm arm, Action<FinalArm> report)
	{
		RustNavDoorGates.gatesEnabled = arm.gates;
		RustNavDoorGates.polyRefCacheEnabled = arm.refCache;
		RustNavDoorGates.leafSlabsEnabled = true;
		RustNavDoorGates.apertureHalfThickness = 0.5f;
		RefreshApertures();
		RustNavDoorGates.InvalidateCaches();
		bool bakeStatsWereEnabled = RustNav.bakeStatsEnabled;
		RustNav.bakeStatsEnabled = true;
		BakeStats.Reset();
		RustNavigation.Instance.RebuildTilesInBounds(RebakeBounds(corpusOrigin, corpusRadius));
		yield return WaitForTileDrain();
		double num = BakeStats.StageMs(BakeStats.Stage.CollectDoors);
		arm.bakeTiles = BakeStats.StageCount(BakeStats.Stage.CollectTotal);
		arm.bakeDoorUsPerTile = ((arm.bakeTiles > 0) ? (num * 1000.0 / (double)arm.bakeTiles) : 0.0);
		arm.bakeMain = BakeStats.StageMs(BakeStats.Stage.CollectTotal) + BakeStats.StageMs(BakeStats.Stage.MainAddTile);
		arm.bakeWorker = BakeStats.StageMs(BakeStats.Stage.WorkerTotal);
		RustNav.bakeStatsEnabled = bakeStatsWereEnabled;
		RustNavDoorGates.ReassertAll();
		arm.totalPolys = int.Parse(SweepCensus(out var _, out var _, out var _, out var _));
		PooledList<Door> doors = Pool.Get<PooledList<Door>>();
		try
		{
			CollectAllDoors(corpusOrigin, corpusRadius, (List<Door>)(object)doors);
			arm.doors = ((List<Door>)(object)doors).Count;
			double toggleSum = 0.0;
			double gatherSum = 0.0;
			double writeSum = 0.0;
			double reassertSum = 0.0;
			double regSum = 0.0;
			double unregSum = 0.0;
			for (int rep = 0; rep < reps; rep++)
			{
				TogglePassRaw((List<Door>)(object)doors, null);
				PooledList<double> val = Pool.Get<PooledList<double>>();
				try
				{
					RustNavDoorGates.Profile.Reset();
					RustNavDoorGates.Profile.enabled = true;
					TogglePassRaw((List<Door>)(object)doors, (List<double>)(object)val);
					RustNavDoorGates.Profile.enabled = false;
					((List<double>)(object)val).Sort();
					toggleSum += ((((List<double>)(object)val).Count > 0) ? ((List<double>)(object)val)[((List<double>)(object)val).Count / 2] : 0.0);
					gatherSum += ((((List<Door>)(object)doors).Count > 0) ? (RustNavDoorGates.Profile.GatherMs * 1000.0 / (double)((List<Door>)(object)doors).Count) : 0.0);
					writeSum += ((((List<Door>)(object)doors).Count > 0) ? (RustNavDoorGates.Profile.WriteMs * 1000.0 / (double)((List<Door>)(object)doors).Count) : 0.0);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Restart();
				RustNavDoorGates.ReassertAll();
				stopwatch.Stop();
				reassertSum += stopwatch.Elapsed.TotalMilliseconds;
				int num2 = Mathf.Max(1, ((List<Door>)(object)doors).Count / 60);
				int num3 = 0;
				double num4 = 0.0;
				double num5 = 0.0;
				for (int i = 0; i < ((List<Door>)(object)doors).Count; i += num2)
				{
					Door door = ((List<Door>)(object)doors)[i];
					if (!((Object)(object)door == (Object)null) && !door.IsDestroyed)
					{
						stopwatch.Restart();
						RustNavDoorGates.Unregister(door);
						stopwatch.Stop();
						num5 += stopwatch.Elapsed.TotalMilliseconds;
						stopwatch.Restart();
						RustNavDoorGates.Register(door);
						stopwatch.Stop();
						num4 += stopwatch.Elapsed.TotalMilliseconds;
						num3++;
					}
				}
				regSum += ((num3 > 0) ? (num4 / (double)num3) : 0.0);
				unregSum += ((num3 > 0) ? (num5 / (double)num3) : 0.0);
				yield return null;
			}
			arm.toggle = toggleSum / (double)reps;
			arm.gatherUs = gatherSum / (double)reps;
			arm.writeUs = writeSum / (double)reps;
			arm.reassert = reassertSum / (double)reps;
			arm.register = regSum / (double)reps;
			arm.unregister = unregSum / (double)reps;
			string text = Path.Combine(Application.temporaryCachePath, "doorstress.navmesh");
			Stopwatch stopwatch2 = new Stopwatch();
			stopwatch2.Restart();
			bool flag = RustNavigation.Instance.Save(text);
			stopwatch2.Stop();
			arm.saveMs = (flag ? stopwatch2.Elapsed.TotalMilliseconds : (-1.0));
			arm.saveBytes = ((flag && File.Exists(text)) ? new FileInfo(text).Length : 0);
			if (flag)
			{
				stopwatch2.Restart();
				RustNavigation.Instance.Load(text, synchronous: true);
				stopwatch2.Stop();
				arm.loadMs = stopwatch2.Elapsed.TotalMilliseconds;
				yield return WaitForTileDrain();
			}
			else
			{
				arm.loadMs = -1.0;
			}
			report(arm);
		}
		finally
		{
			((IDisposable)doors)?.Dispose();
		}
	}
}

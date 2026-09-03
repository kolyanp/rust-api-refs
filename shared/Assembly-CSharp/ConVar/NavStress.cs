using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Rust.Ai.Gen2;
using Rust.Ai.Gen2.Nav;
using UnityEngine;
using UnityEngine.AI;

namespace ConVar;

[Factory("navstress")]
public class NavStress : ConsoleSystem
{
	private enum Mode
	{
		Idle,
		Wander,
		Chase
	}

	private class DummyState
	{
		public float nextActionTime;

		public Vector3 chaseTarget;

		public Vector3 chaseDir;
	}

	private struct RefSample
	{
		public float x;

		public float y;

		public float z;

		public float t;

		public bool swimming;
	}

	private class RefLeg
	{
		public string name;

		public readonly List<RefSample> samples = new List<RefSample>();

		public float duration;
	}

	private const string WolfPrefab = "assets/rust.ai/agents/wolf/wolf2.prefab";

	private static readonly List<GameObject> envObjects = new List<GameObject>();

	private static readonly List<BaseEntity> dummyEntities = new List<BaseEntity>();

	private static readonly List<RustNavMeshAgent> dummyAgents = new List<RustNavMeshAgent>();

	private static readonly List<DummyState> dummyStates = new List<DummyState>();

	private static readonly List<Vector3> groundPoints = new List<Vector3>();

	private static readonly List<Vector3> islandPoints = new List<Vector3>();

	private static Vector3 fieldCenter;

	private static float fieldHalfExtent;

	private static float groundY;

	private static bool envBuilt;

	private static Mode mode = Mode.Idle;

	private static float wanderInterval = 4f;

	private static float unreachableFraction = 0f;

	private static float chaseSpeed = 5.5f;

	private static float chaseInterval = 0.1f;

	private static float rebuildStormInterval = 0f;

	private static float nextRebuildTime = 0f;

	private static NavStressDriver driver;

	private static Random rng = new Random(12345);

	private static string lastEnvReport = "no env built yet";

	private static readonly string RefPathDir = "profile/refpaths";

	private static string lastRefReport = "no refpath run yet";

	private static string lastBenchReport = "no bench run yet";

	private static float NextFloat(float min, float max)
	{
		return min + (float)rng.NextDouble() * (max - min);
	}

	private static int NextIndex(int count)
	{
		return rng.Next(count);
	}

	private static bool EnsureReady(Arg arg, bool needEnv, bool needDummies)
	{
		if (AI.useUnityNavmesh)
		{
			if ((Object)(object)SingletonComponent<DynamicNavMesh>.Instance == (Object)null)
			{
				arg.ReplyWith("navstress needs a DynamicNavMesh in the scene on the unity backend");
				return false;
			}
		}
		else if ((Object)(object)RustNavigation.Instance == (Object)null || !RustNavigation.Instance.IsDefaultNavmeshBuilt())
		{
			arg.ReplyWith("navstress requires a built RustNav navmesh");
			return false;
		}
		if (needEnv && !envBuilt)
		{
			arg.ReplyWith("run navstress.build_env first");
			return false;
		}
		if (needDummies && dummyAgents.Count == 0)
		{
			arg.ReplyWith("run navstress.spawn first");
			return false;
		}
		return true;
	}

	private static void EnsureDriver()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)driver != (Object)null))
		{
			driver = new GameObject("NavStressDriver").AddComponent<NavStressDriver>();
		}
	}

	[ServerVar(Help = "Build the synthetic stress field: args centerX centerZ halfExtent (defaults -140 140 50)")]
	public static void build_env(Arg arg)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		if (!EnsureReady(arg, needEnv: false, needDummies: false))
		{
			return;
		}
		ClearEnvironment();
		fieldCenter = new Vector3(arg.GetFloat(0, -140f), 0f, arg.GetFloat(1, 140f));
		fieldHalfExtent = arg.GetFloat(2, 50f);
		RaycastHit val = default(RaycastHit);
		if (!Physics.Raycast(fieldCenter + Vector3.up * 500f, Vector3.down, ref val, 2000f, LayerMask.GetMask(new string[3] { "World", "Terrain", "Default" })))
		{
			arg.ReplyWith($"no ground under field center {fieldCenter}, pick another spot");
			return;
		}
		groundY = ((RaycastHit)(ref val)).point.y;
		fieldCenter.y = groundY;
		if (!RustNavMeshHelpers.SamplePosition(fieldCenter + Vector3.up, out var hitWS, 10f, -1) || Mathf.Abs(((NavMeshHit)(ref hitWS)).position.y - groundY) > 3f)
		{
			arg.ReplyWith($"no navmesh near ground at field center {fieldCenter} (ground y {groundY:F1})");
			return;
		}
		int layer = LayerMask.NameToLayer("World");
		State state = Random.state;
		Random.InitState(1337);
		float num = 12f;
		for (float num2 = 0f - fieldHalfExtent + num; num2 < fieldHalfExtent - num * 0.5f; num2 += num)
		{
			for (float num3 = 0f - fieldHalfExtent + num; num3 < fieldHalfExtent - num * 0.5f; num3 += num)
			{
				if (!(Random.value < 0.35f))
				{
					GameObject val2 = GameObject.CreatePrimitive((PrimitiveType)3);
					((Object)val2).name = "navstress_wall";
					val2.layer = layer;
					bool flag = Random.value < 0.5f;
					val2.transform.localScale = (flag ? new Vector3(8f, 3f, 0.35f) : new Vector3(0.35f, 3f, 8f));
					val2.transform.position = fieldCenter + new Vector3(num2 + Random.Range(-2f, 2f), 1.5f, num3 + Random.Range(-2f, 2f));
					envObjects.Add(val2);
				}
			}
		}
		for (int i = 0; i < 3; i++)
		{
			GameObject val3 = GameObject.CreatePrimitive((PrimitiveType)3);
			((Object)val3).name = "navstress_island";
			val3.layer = layer;
			val3.transform.localScale = new Vector3(10f, 0.5f, 10f);
			float num4 = (float)i * 120f * (MathF.PI / 180f);
			val3.transform.position = fieldCenter + new Vector3(Mathf.Cos(num4), 0f, Mathf.Sin(num4)) * (fieldHalfExtent * 0.5f) + Vector3.up * 12f;
			envObjects.Add(val3);
		}
		Random.state = state;
		Physics.SyncTransforms();
		if (AI.useUnityNavmesh)
		{
			EnsureDriver();
			((MonoBehaviour)driver).StartCoroutine(FinishUnityEnvBuild());
			arg.ReplyWith($"navstress env building at {fieldCenter} (unity backend, async) - poll navstress.envreport");
			return;
		}
		RebuildFieldTiles();
		GatherDestinationPools();
		envBuilt = groundPoints.Count > 50;
		lastEnvReport = string.Format("navstress env built at {0} halfExtent {1}: {2} objects, {3} ground points, {4} island points{5}", new object[6]
		{
			fieldCenter,
			fieldHalfExtent,
			envObjects.Count,
			groundPoints.Count,
			islandPoints.Count,
			envBuilt ? "" : " (FAILED, too few points)"
		});
		arg.ReplyWith(lastEnvReport);
	}

	[ServerVar(Help = "Print the last environment build report")]
	public static void envreport(Arg arg)
	{
		arg.ReplyWith(lastEnvReport);
	}

	private static IEnumerator FinishUnityEnvBuild()
	{
		yield return SingletonComponent<DynamicNavMesh>.Instance.UpdateNavMeshAndWait();
		GatherDestinationPools();
		envBuilt = groundPoints.Count > 50;
		lastEnvReport = string.Format("navstress env built at {0} halfExtent {1} (unity): {2} objects, {3} ground points, {4} island points{5}", new object[6]
		{
			fieldCenter,
			fieldHalfExtent,
			envObjects.Count,
			groundPoints.Count,
			islandPoints.Count,
			envBuilt ? "" : " (FAILED, too few points)"
		});
		Debug.Log((object)lastEnvReport);
	}

	private static void RebuildBoundsAnyBackend(Bounds bounds)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh)
		{
			SingletonComponent<DynamicNavMesh>.Instance.UpdateNavMeshAsync();
		}
		else
		{
			RustNavigation.Instance.RebuildTilesInBounds(bounds, synchronous: true);
		}
	}

	private static void RebuildFieldTiles()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh)
		{
			SingletonComponent<DynamicNavMesh>.Instance.UpdateNavMeshAsync();
			return;
		}
		Bounds rebuildBounds = default(Bounds);
		((Bounds)(ref rebuildBounds))._002Ector(fieldCenter, new Vector3(fieldHalfExtent * 2f + 40f, 80f, fieldHalfExtent * 2f + 40f));
		RustNavigation.Instance.RebuildTilesInBounds(rebuildBounds, synchronous: true);
	}

	private static void GatherDestinationPools()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		groundPoints.Clear();
		islandPoints.Clear();
		State state = Random.state;
		Random.InitState(4242);
		for (int i = 0; i < 2000; i++)
		{
			if (groundPoints.Count >= 400)
			{
				break;
			}
			if (RustNavMeshHelpers.SamplePosition(fieldCenter + new Vector3(Random.Range(0f - fieldHalfExtent, fieldHalfExtent), 0f, Random.Range(0f - fieldHalfExtent, fieldHalfExtent)), out var hitWS, 4f, -1) && !(Mathf.Abs(((NavMeshHit)(ref hitWS)).position.y - groundY) > 1.5f))
			{
				groundPoints.Add(((NavMeshHit)(ref hitWS)).position);
			}
		}
		for (int j = 0; j < envObjects.Count; j++)
		{
			GameObject val = envObjects[j];
			if ((Object)(object)val == (Object)null || ((Object)val).name != "navstress_island")
			{
				continue;
			}
			for (int k = 0; k < 20; k++)
			{
				if (islandPoints.Count >= 30)
				{
					break;
				}
				if (RustNavMeshHelpers.SamplePosition(val.transform.position + Vector3.up * 0.3f + new Vector3(Random.Range(-4f, 4f), 0f, Random.Range(-4f, 4f)), out var hitWS2, 3f, -1) && !(((NavMeshHit)(ref hitWS2)).position.y < groundY + 8f))
				{
					islandPoints.Add(((NavMeshHit)(ref hitWS2)).position);
				}
			}
		}
		Random.state = state;
	}

	[ServerVar(Help = "Spawn stripped wolves: args count mode(steering|normal) canSwim canOpenDoors")]
	public static void spawn(Arg arg)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		if (!EnsureReady(arg, needEnv: true, needDummies: false))
		{
			return;
		}
		ClearDummies();
		int num = arg.GetInt(0, 100);
		string text = arg.GetString(1, "steering");
		bool flag = arg.GetBool(2);
		bool flag2 = arg.GetBool(3);
		State state = Random.state;
		Random.InitState(777);
		FSMComponent fSMComponent = default(FSMComponent);
		RustNavMeshAgent rustNavMeshAgent = default(RustNavMeshAgent);
		for (int i = 0; i < num; i++)
		{
			Vector3 pos = groundPoints[Random.Range(0, groundPoints.Count)];
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/rust.ai/agents/wolf/wolf2.prefab", pos, Quaternion.identity);
			if ((Object)(object)baseEntity == (Object)null)
			{
				break;
			}
			baseEntity.Spawn();
			if (((Component)baseEntity).TryGetComponent<FSMComponent>(ref fSMComponent))
			{
				fSMComponent.SetFsmActive(newActive: false);
			}
			if (!((Component)baseEntity).TryGetComponent<RustNavMeshAgent>(ref rustNavMeshAgent))
			{
				baseEntity.Kill();
				continue;
			}
			ConfigureAgent(rustNavMeshAgent, text);
			rustNavMeshAgent.canSwim = flag;
			rustNavMeshAgent.canOpenDoors = flag2;
			dummyEntities.Add(baseEntity);
			dummyAgents.Add(rustNavMeshAgent);
			dummyStates.Add(new DummyState());
		}
		Random.state = state;
		mode = Mode.Idle;
		EnsureDriver();
		NavStressStats.enabled = true;
		arg.ReplyWith(string.Format("navstress spawned {0} dummies, mode {1}, swim {2}, doors {3}", new object[4] { dummyAgents.Count, text, flag, flag2 }));
	}

	private static void ConfigureAgent(RustNavMeshAgent agent, string modeName)
	{
		bool flag = modeName != "normal";
		agent.letUnityMoveAgentIfPossible = !flag;
		agent.canSteer = flag;
	}

	[ServerVar(Help = "Flip all dummies between steering and normal mode without respawning")]
	public static void setmode(Arg arg)
	{
		if (EnsureReady(arg, needEnv: true, needDummies: true))
		{
			string text = arg.GetString(0, "steering");
			for (int i = 0; i < dummyAgents.Count; i++)
			{
				ConfigureAgent(dummyAgents[i], text);
			}
			arg.ReplyWith($"navstress mode set to {text} on {dummyAgents.Count} dummies");
		}
	}

	[ServerVar(Help = "Random destinations forever: args intervalSeconds unreachableFraction")]
	public static void wander(Arg arg)
	{
		if (EnsureReady(arg, needEnv: true, needDummies: true))
		{
			wanderInterval = arg.GetFloat(0, 4f);
			unreachableFraction = Mathf.Clamp01(arg.GetFloat(1));
			mode = Mode.Wander;
			ResetDummyTimers();
			arg.ReplyWith($"navstress wandering every {wanderInterval}s, unreachable {unreachableFraction:P0}");
		}
	}

	[ServerVar(Help = "Chase a sliding virtual target: args targetSpeed setDestinationHz")]
	public static void chase(Arg arg)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (EnsureReady(arg, needEnv: true, needDummies: true))
		{
			chaseSpeed = arg.GetFloat(0, 5.5f);
			float num = Mathf.Max(0.5f, arg.GetFloat(1, 10f));
			chaseInterval = 1f / num;
			mode = Mode.Chase;
			ResetDummyTimers();
			for (int i = 0; i < dummyAgents.Count; i++)
			{
				DummyState dummyState = dummyStates[i];
				dummyState.chaseTarget = ((Component)dummyAgents[i]).transform.position;
				dummyState.chaseDir = RandomFlatDir();
			}
			arg.ReplyWith($"navstress chasing, target speed {chaseSpeed} m/s, SetDestination {num} Hz");
		}
	}

	[ServerVar(Help = "Stop giving destinations and reset paths")]
	public static void idle(Arg arg)
	{
		if (!EnsureReady(arg, needEnv: false, needDummies: true))
		{
			return;
		}
		mode = Mode.Idle;
		for (int i = 0; i < dummyAgents.Count; i++)
		{
			if ((Object)(object)dummyAgents[i] != (Object)null)
			{
				dummyAgents[i].ResetPath();
			}
		}
		arg.ReplyWith("navstress idle");
	}

	[ServerVar(Help = "Async tile rebuilds under the field: args rebuildsPerSecond (0 stops)")]
	public static void rebuildstorm(Arg arg)
	{
		if (EnsureReady(arg, needEnv: true, needDummies: false))
		{
			float num = arg.GetFloat(0, 1f);
			rebuildStormInterval = ((num > 0f) ? (1f / num) : 0f);
			nextRebuildTime = Time.time;
			arg.ReplyWith((num > 0f) ? $"navstress rebuild storm at {num}/s" : "navstress rebuild storm stopped");
		}
	}

	[ServerVar(Help = "Print and reset tick statistics")]
	public static void stats(Arg arg)
	{
		arg.ReplyWith(NavStressStats.Report(dummyAgents.Count, mode.ToString()));
		NavStressStats.ResetSamples();
	}

	[ServerVar(Help = "Kill every NPC with a RustNavMeshAgent that is not a navstress dummy")]
	public static void killforeign(Arg arg)
	{
		arg.ReplyWith($"killed {KillForeignAgents()} foreign agents");
	}

	private static int KillForeignAgents()
	{
		int num = 0;
		List<BaseEntity> list = new List<BaseEntity>();
		RustNavMeshAgent[] array = Object.FindObjectsByType<RustNavMeshAgent>((FindObjectsSortMode)0);
		for (int i = 0; i < array.Length; i++)
		{
			BaseEntity component = ((Component)array[i]).GetComponent<BaseEntity>();
			if (!((Object)(object)component == (Object)null) && component.isServer && !component.IsDestroyed && !dummyEntities.Contains(component))
			{
				list.Add(component);
			}
		}
		foreach (BaseEntity item in list)
		{
			item.Kill();
			num++;
		}
		return num;
	}

	private static void RepairField()
	{
		if (!AI.useUnityNavmesh)
		{
			Physics.SyncTransforms();
			RebuildFieldTiles();
			GatherDestinationPools();
		}
	}

	[ServerVar(Help = "Despawn dummies and remove the field")]
	public static void clear(Arg arg)
	{
		ClearDummies();
		bool num = envBuilt;
		ClearEnvironment();
		if (num && (AI.useUnityNavmesh || ((Object)(object)RustNavigation.Instance != (Object)null && RustNavigation.Instance.IsDefaultNavmeshBuilt())))
		{
			Physics.SyncTransforms();
			RebuildFieldTiles();
		}
		NavStressStats.enabled = false;
		arg.ReplyWith("navstress cleared");
	}

	private static void ClearDummies()
	{
		for (int i = 0; i < dummyEntities.Count; i++)
		{
			if ((Object)(object)dummyEntities[i] != (Object)null && !dummyEntities[i].IsDestroyed)
			{
				dummyEntities[i].Kill();
			}
		}
		dummyEntities.Clear();
		dummyAgents.Clear();
		dummyStates.Clear();
		mode = Mode.Idle;
	}

	private static void ClearEnvironment()
	{
		for (int i = 0; i < envObjects.Count; i++)
		{
			if ((Object)(object)envObjects[i] != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)envObjects[i]);
			}
		}
		envObjects.Clear();
		groundPoints.Clear();
		islandPoints.Clear();
		envBuilt = false;
		rebuildStormInterval = 0f;
	}

	private static void ResetDummyTimers()
	{
		rng = new Random(12345);
		for (int i = 0; i < dummyStates.Count; i++)
		{
			dummyStates[i].nextActionTime = Time.time + NextFloat(0f, 0.5f);
		}
	}

	private static Vector3 RandomFlatDir()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		float num = NextFloat(0f, MathF.PI * 2f);
		return new Vector3(Mathf.Cos(num), 0f, Mathf.Sin(num));
	}

	internal static void DriverUpdate()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		if (dummyAgents.Count == 0)
		{
			return;
		}
		float time = Time.time;
		if (rebuildStormInterval > 0f && !AI.useUnityNavmesh && time >= nextRebuildTime)
		{
			nextRebuildTime = time + rebuildStormInterval;
			Vector3 val = fieldCenter + new Vector3(NextFloat(0f - fieldHalfExtent, fieldHalfExtent), 0f, NextFloat(0f - fieldHalfExtent, fieldHalfExtent));
			RustNavigation.Instance.RebuildTilesInBounds(new Bounds(val, new Vector3(8f, 60f, 8f)));
		}
		if (mode == Mode.Idle)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		for (int i = 0; i < dummyAgents.Count; i++)
		{
			RustNavMeshAgent rustNavMeshAgent = dummyAgents[i];
			if ((Object)(object)rustNavMeshAgent == (Object)null)
			{
				continue;
			}
			DummyState dummyState = dummyStates[i];
			if (mode == Mode.Wander)
			{
				if (time < dummyState.nextActionTime)
				{
					continue;
				}
				dummyState.nextActionTime = time + wanderInterval * NextFloat(0.75f, 1.25f);
				List<Vector3> list = ((islandPoints.Count > 0 && NextFloat(0f, 1f) < unreachableFraction) ? islandPoints : groundPoints);
				if (list.Count != 0)
				{
					Vector3 targetPositionWS = list[NextIndex(list.Count)];
					NavStressStats.setDestinationCalls++;
					if (!rustNavMeshAgent.SetDestinationWithParams(targetPositionWS, autoBraking: true, RustNavMeshAgent.Speeds.Run))
					{
						NavStressStats.setDestinationFails++;
					}
				}
			}
			else
			{
				if (mode != Mode.Chase)
				{
					continue;
				}
				dummyState.chaseTarget += dummyState.chaseDir * (chaseSpeed * deltaTime);
				Vector3 val2 = dummyState.chaseTarget - fieldCenter;
				if (Mathf.Abs(val2.x) > fieldHalfExtent || Mathf.Abs(val2.z) > fieldHalfExtent)
				{
					dummyState.chaseDir = -dummyState.chaseDir;
					dummyState.chaseTarget += dummyState.chaseDir * (chaseSpeed * deltaTime * 2f);
				}
				if (time < dummyState.nextActionTime)
				{
					continue;
				}
				dummyState.nextActionTime = time + chaseInterval;
				if (RustNavMeshHelpers.SamplePosition(dummyState.chaseTarget, out var hitWS, 4f, -1))
				{
					NavStressStats.setDestinationCalls++;
					if (!rustNavMeshAgent.SetDestinationWithParams(((NavMeshHit)(ref hitWS)).position, autoBraking: true, RustNavMeshAgent.Speeds.Sprint))
					{
						NavStressStats.setDestinationFails++;
					}
				}
				else
				{
					dummyState.chaseDir = RandomFlatDir();
					dummyState.chaseTarget = ((Component)rustNavMeshAgent).transform.position;
				}
			}
		}
	}

	[ServerVar(Help = "Record the reference trajectory scenario: args name")]
	public static void refrecord(Arg arg)
	{
		if (EnsureReady(arg, needEnv: true, needDummies: false))
		{
			EnsureDriver();
			((MonoBehaviour)driver).StartCoroutine(RefPathRoutine(arg.GetString(0, "baseline"), record: true));
			arg.ReplyWith("refpath recording started, results via navstress.refresults");
		}
	}

	[ServerVar(Help = "Replay the scenario and compare against a recording: args name")]
	public static void refcompare(Arg arg)
	{
		if (EnsureReady(arg, needEnv: true, needDummies: false))
		{
			EnsureDriver();
			((MonoBehaviour)driver).StartCoroutine(RefPathRoutine(arg.GetString(0, "baseline"), record: false));
			arg.ReplyWith("refpath comparison started, results via navstress.refresults");
		}
	}

	[ServerVar(Help = "Print the last refpath result")]
	public static void refresults(Arg arg)
	{
		arg.ReplyWith(lastRefReport);
	}

	private static IEnumerator RefPathRoutine(string name, bool record)
	{
		ClearDummies();
		Vector3 start = SnapToNavmesh(fieldCenter + new Vector3(-40f, 0f, -40f));
		Vector3 mazeEnd = SnapToNavmesh(fieldCenter + new Vector3(38f, 0f, 41f));
		Vector3 secondEnd = SnapToNavmesh(fieldCenter + new Vector3(-35f, 0f, 30f));
		BaseEntity baseEntity = GameManager.server.CreateEntity("assets/rust.ai/agents/wolf/wolf2.prefab", start, Quaternion.identity);
		baseEntity.Spawn();
		FSMComponent fSMComponent = default(FSMComponent);
		if (((Component)baseEntity).TryGetComponent<FSMComponent>(ref fSMComponent))
		{
			fSMComponent.SetFsmActive(newActive: false);
		}
		RustNavMeshAgent agent = default(RustNavMeshAgent);
		((Component)baseEntity).TryGetComponent<RustNavMeshAgent>(ref agent);
		ConfigureAgent(agent, "steering");
		agent.canSwim = false;
		agent.canOpenDoors = false;
		dummyEntities.Add(baseEntity);
		dummyAgents.Add(agent);
		dummyStates.Add(new DummyState());
		mode = Mode.Idle;
		yield return CoroutineEx.waitForSeconds(1f);
		NavStressOcean ocean = new NavStressOcean();
		ocean.SetFlatOcean();
		Vector3 pierCenter = new Vector3(fieldCenter.x, -2f, fieldCenter.z + fieldHalfExtent + 60f);
		GameObject pier = GameObject.CreatePrimitive((PrimitiveType)3);
		((Object)pier).name = "navstress_pier";
		pier.layer = LayerMask.NameToLayer("World");
		pier.transform.localScale = new Vector3(50f, 1f, 8f);
		pier.transform.position = pierCenter;
		envObjects.Add(pier);
		Physics.SyncTransforms();
		RebuildBoundsAnyBackend(new Bounds(pierCenter, new Vector3(60f, 20f, 20f)));
		yield return CoroutineEx.waitForSeconds(1f);
		List<RefLeg> legs = new List<RefLeg>();
		try
		{
			agent.WarpToWorldPosition(start);
			yield return CoroutineEx.waitForSeconds(0.5f);
			yield return RecordLeg(legs, "maze", agent, mazeEnd, 60f);
			yield return RecordLeg(legs, "second", agent, secondEnd, 60f);
			yield return RecordChaseLeg(legs, agent, 12f);
			yield return RecordSwimLeg(legs, agent, pierCenter);
		}
		finally
		{
			if ((Object)(object)pier != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)pier);
				envObjects.Remove(pier);
				Physics.SyncTransforms();
				RebuildBoundsAnyBackend(new Bounds(pierCenter, new Vector3(60f, 20f, 20f)));
			}
			ocean.Restore();
		}
		if (record)
		{
			SaveLegs(name, legs);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("===== refpath recorded '" + name + "' =====");
			foreach (RefLeg item in legs)
			{
				stringBuilder.AppendLine(string.Format("{0,-8} {1,5} samples, {2:F1}s, length {3:F1}m", new object[4]
				{
					item.name,
					item.samples.Count,
					item.duration,
					LegLength(item.samples)
				}));
			}
			lastRefReport = stringBuilder.ToString();
		}
		else
		{
			lastRefReport = CompareLegs(name, legs);
		}
		Debug.Log((object)lastRefReport);
		ClearDummies();
	}

	private static Vector3 SnapToNavmesh(Vector3 posWS)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (RustNavMeshHelpers.SamplePosition(posWS, out var hitWS, 6f, -1))
		{
			return ((NavMeshHit)(ref hitWS)).position;
		}
		return posWS;
	}

	private static IEnumerator RecordLeg(List<RefLeg> legs, string legName, RustNavMeshAgent agent, Vector3 destination, float timeout)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		RefLeg leg = new RefLeg
		{
			name = legName
		};
		legs.Add(leg);
		if (!agent.SetDestinationWithParams(destination, autoBraking: true, RustNavMeshAgent.Speeds.Run))
		{
			leg.duration = -1f;
			yield break;
		}
		float startTime = Time.time;
		while (agent.hasPath && Time.time - startTime < timeout)
		{
			leg.samples.Add(SampleOf(agent, startTime));
			yield return null;
		}
		leg.duration = Time.time - startTime;
	}

	private static IEnumerator RecordChaseLeg(List<RefLeg> legs, RustNavMeshAgent agent, float seconds)
	{
		RefLeg leg = new RefLeg
		{
			name = "chase"
		};
		legs.Add(leg);
		Vector3 target = ((Component)agent).transform.position;
		Vector3 dir = new Vector3(0.8f, 0f, 0.6f);
		float startTime = Time.time;
		float nextSet = 0f;
		while (Time.time - startTime < seconds)
		{
			target += dir * (4.5f * Time.deltaTime);
			Vector3 val = target - fieldCenter;
			if (Mathf.Abs(val.x) > fieldHalfExtent || Mathf.Abs(val.z) > fieldHalfExtent)
			{
				dir = -dir;
			}
			if (Time.time >= nextSet)
			{
				nextSet = Time.time + 0.1f;
				if (RustNavMeshHelpers.SamplePosition(target, out var hitWS, 4f, -1))
				{
					agent.SetDestinationWithParams(((NavMeshHit)(ref hitWS)).position, autoBraking: true, RustNavMeshAgent.Speeds.Sprint);
				}
			}
			leg.samples.Add(SampleOf(agent, startTime));
			yield return null;
		}
		leg.duration = seconds;
		agent.ResetPath();
	}

	private static IEnumerator RecordSwimLeg(List<RefLeg> legs, RustNavMeshAgent agent, Vector3 pierCenter)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		RefLeg leg = new RefLeg
		{
			name = "swim"
		};
		legs.Add(leg);
		Vector3 newPositionWS = SnapToNavmesh(pierCenter + new Vector3(-22f, 1f, 0f));
		Vector3 pierEnd = SnapToNavmesh(pierCenter + new Vector3(22f, 1f, 0f));
		agent.WarpToWorldPosition(newPositionWS);
		yield return CoroutineEx.waitForSeconds(0.5f);
		agent.canSwim = true;
		yield return null;
		if (agent.SetDestinationWithParams(pierEnd, autoBraking: true, RustNavMeshAgent.Speeds.Run))
		{
			float startTime = Time.time;
			while (agent.hasPath && Time.time - startTime < 90f)
			{
				leg.samples.Add(SampleOf(agent, startTime));
				yield return null;
			}
			leg.duration = Time.time - startTime;
		}
		else
		{
			leg.duration = -1f;
		}
		agent.canSwim = false;
	}

	private static RefSample SampleOf(RustNavMeshAgent agent, float legStartTime)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)agent).transform.position;
		return new RefSample
		{
			x = position.x,
			y = position.y,
			z = position.z,
			t = Time.time - legStartTime,
			swimming = agent.IsSwimming
		};
	}

	private static void SaveLegs(string name, List<RefLeg> legs)
	{
		Directory.CreateDirectory(RefPathDir);
		foreach (RefLeg leg in legs)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"# duration {leg.duration:F3}");
			foreach (RefSample sample in leg.samples)
			{
				stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0:F3},{1:F3},{2:F3},{3:F3},{4}", new object[5]
				{
					sample.x,
					sample.y,
					sample.z,
					sample.t,
					sample.swimming ? 1 : 0
				}));
			}
			File.WriteAllText(RefPathDir + "/" + name + "_" + leg.name + ".csv", stringBuilder.ToString());
		}
	}

	private static bool LoadLeg(string name, string legName, List<RefSample> samples, out float duration)
	{
		duration = 0f;
		string path = RefPathDir + "/" + name + "_" + legName + ".csv";
		if (!File.Exists(path))
		{
			return false;
		}
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		string[] array = File.ReadAllLines(path);
		foreach (string text in array)
		{
			if (text.StartsWith("#"))
			{
				float.TryParse(text.Substring(11), NumberStyles.Float, invariantCulture, out duration);
				continue;
			}
			string[] array2 = text.Split(',');
			if (array2.Length >= 5)
			{
				samples.Add(new RefSample
				{
					x = float.Parse(array2[0], invariantCulture),
					y = float.Parse(array2[1], invariantCulture),
					z = float.Parse(array2[2], invariantCulture),
					t = float.Parse(array2[3], invariantCulture),
					swimming = (array2[4].Trim() == "1")
				});
			}
		}
		return samples.Count > 1;
	}

	private static float LegLength(List<RefSample> samples)
	{
		float num = 0f;
		for (int i = 1; i < samples.Count; i++)
		{
			RefSample refSample = samples[i - 1];
			RefSample refSample2 = samples[i];
			num += Mathf.Sqrt((refSample2.x - refSample.x) * (refSample2.x - refSample.x) + (refSample2.z - refSample.z) * (refSample2.z - refSample.z));
		}
		return num;
	}

	private static RefSample SampleAtArcLength(List<RefSample> samples, float arc)
	{
		float num = 0f;
		for (int i = 1; i < samples.Count; i++)
		{
			RefSample refSample = samples[i - 1];
			RefSample refSample2 = samples[i];
			float num2 = Mathf.Sqrt((refSample2.x - refSample.x) * (refSample2.x - refSample.x) + (refSample2.z - refSample.z) * (refSample2.z - refSample.z));
			if (num + num2 >= arc && num2 > 0f)
			{
				float num3 = (arc - num) / num2;
				return new RefSample
				{
					x = Mathf.Lerp(refSample.x, refSample2.x, num3),
					y = Mathf.Lerp(refSample.y, refSample2.y, num3),
					z = Mathf.Lerp(refSample.z, refSample2.z, num3),
					t = Mathf.Lerp(refSample.t, refSample2.t, num3),
					swimming = ((num3 < 0.5f) ? refSample.swimming : refSample2.swimming)
				};
			}
			num += num2;
		}
		return samples[samples.Count - 1];
	}

	private static string CompareLegs(string name, List<RefLeg> currentLegs)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("===== refpath compare vs '" + name + "' =====");
		foreach (RefLeg currentLeg in currentLegs)
		{
			List<RefSample> samples = new List<RefSample>();
			if (!LoadLeg(name, currentLeg.name, samples, out var duration))
			{
				stringBuilder.AppendLine($"{currentLeg.name,-8} NO REFERENCE RECORDING");
				continue;
			}
			if (currentLeg.samples.Count < 2 || currentLeg.duration < 0f)
			{
				stringBuilder.AppendLine($"{currentLeg.name,-8} LEG FAILED TO RUN");
				continue;
			}
			float num = LegLength(samples);
			float num2 = LegLength(currentLeg.samples);
			float num3 = Mathf.Min(num, num2);
			float num4 = 0f;
			float num5 = 0f;
			float num6 = 0f;
			int num7 = 0;
			for (int i = 0; i <= 200; i++)
			{
				float arc = num3 * (float)i / 200f;
				RefSample refSample = SampleAtArcLength(samples, arc);
				RefSample refSample2 = SampleAtArcLength(currentLeg.samples, arc);
				float num8 = Mathf.Sqrt((refSample.x - refSample2.x) * (refSample.x - refSample2.x) + (refSample.z - refSample2.z) * (refSample.z - refSample2.z));
				num4 += num8;
				if (num8 > num5)
				{
					num5 = num8;
				}
				num6 += Mathf.Abs(refSample.y - refSample2.y);
				if (refSample.swimming != refSample2.swimming)
				{
					num7++;
				}
			}
			stringBuilder.AppendLine(string.Format("{0,-8} meanXZ {1:F2}m maxXZ {2:F2}m meanY {3:F2}m, length {4:F1} -> {5:F1}m ({6:+0.0;-0.0}%), duration {7:F1} -> {8:F1}s, swim flag mismatches {9}/201", new object[10]
			{
				currentLeg.name,
				num4 / 201f,
				num5,
				num6 / 201f,
				num,
				num2,
				(num2 - num) / Mathf.Max(num, 0.01f) * 100f,
				duration,
				currentLeg.duration,
				num7
			}));
		}
		return stringBuilder.ToString();
	}

	[ServerVar(Help = "Run the full benchmark suite: args agentCount (default 200)")]
	public static void bench(Arg arg)
	{
		if (EnsureReady(arg, needEnv: true, needDummies: false))
		{
			int num = arg.GetInt(0, 200);
			EnsureDriver();
			((MonoBehaviour)driver).StartCoroutine(BenchRoutine(num));
			arg.ReplyWith($"navstress bench started with {num} agents, results via navstress.benchresults");
		}
	}

	[ServerVar(Help = "Print the last benchmark results")]
	public static void benchresults(Arg arg)
	{
		arg.ReplyWith(lastBenchReport);
	}

	private static void SpawnForBench(int count)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		ClearDummies();
		State state = Random.state;
		Random.InitState(777);
		FSMComponent fSMComponent = default(FSMComponent);
		RustNavMeshAgent rustNavMeshAgent = default(RustNavMeshAgent);
		for (int i = 0; i < count; i++)
		{
			Vector3 pos = groundPoints[Random.Range(0, groundPoints.Count)];
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/rust.ai/agents/wolf/wolf2.prefab", pos, Quaternion.identity);
			if ((Object)(object)baseEntity == (Object)null)
			{
				break;
			}
			baseEntity.Spawn();
			if (((Component)baseEntity).TryGetComponent<FSMComponent>(ref fSMComponent))
			{
				fSMComponent.SetFsmActive(newActive: false);
			}
			if (!((Component)baseEntity).TryGetComponent<RustNavMeshAgent>(ref rustNavMeshAgent))
			{
				baseEntity.Kill();
				continue;
			}
			ConfigureAgent(rustNavMeshAgent, "steering");
			rustNavMeshAgent.canSwim = false;
			rustNavMeshAgent.canOpenDoors = false;
			dummyEntities.Add(baseEntity);
			dummyAgents.Add(rustNavMeshAgent);
			dummyStates.Add(new DummyState());
		}
		Random.state = state;
		NavStressStats.enabled = true;
	}

	private static void SetAllIdle()
	{
		mode = Mode.Idle;
		for (int i = 0; i < dummyAgents.Count; i++)
		{
			if ((Object)(object)dummyAgents[i] != (Object)null)
			{
				dummyAgents[i].ResetPath();
			}
		}
	}

	private static void StartWander(float interval, float unreachable)
	{
		wanderInterval = interval;
		unreachableFraction = unreachable;
		mode = Mode.Wander;
		ResetDummyTimers();
	}

	private static void StartChase(float speed, float hz)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		chaseSpeed = speed;
		chaseInterval = 1f / hz;
		mode = Mode.Chase;
		ResetDummyTimers();
		for (int i = 0; i < dummyAgents.Count; i++)
		{
			DummyState dummyState = dummyStates[i];
			dummyState.chaseTarget = (((Object)(object)dummyAgents[i] != (Object)null) ? ((Component)dummyAgents[i]).transform.position : fieldCenter);
			dummyState.chaseDir = RandomFlatDir();
		}
	}

	private static IEnumerator BenchRoutine(int agentCount)
	{
		RepairField();
		KillForeignAgents();
		SpawnForBench(agentCount);
		yield return CoroutineEx.waitForSeconds(1f);
		List<string> names = new List<string>();
		List<double> means = new List<double>();
		List<string> details = new List<string>();
		for (int s = 0; s < 6; s++)
		{
			string name;
			switch (s)
			{
			case 0:
				name = "idle";
				SetAllIdle();
				break;
			case 1:
				name = "wander-steer";
				SetModeAll("steering");
				StartWander(4f, 0f);
				break;
			case 2:
				name = "wander-normal";
				SetModeAll("normal");
				StartWander(4f, 0f);
				break;
			case 3:
				name = "chase-steer";
				SetModeAll("steering");
				StartChase(5.5f, 10f);
				break;
			case 4:
				name = "wander-unreach25";
				SetModeAll("steering");
				StartWander(4f, 0.25f);
				break;
			default:
				name = "wander-storm";
				SetModeAll("steering");
				StartWander(4f, 0f);
				rebuildStormInterval = 0.5f;
				nextRebuildTime = Time.time;
				break;
			}
			yield return CoroutineEx.waitForSeconds(5f);
			NavStressStats.ResetSamples();
			yield return CoroutineEx.waitForSeconds(20f);
			names.Add(name);
			means.Add(NavStressStats.MeanMs() + NavStressStats.MeanDriverMs());
			details.Add(NavStressStats.Report(dummyAgents.Count, name));
			rebuildStormInterval = 0f;
		}
		SetAllIdle();
		double num = 0.0;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"===== navstress bench, {dummyAgents.Count} agents =====");
		for (int i = 0; i < names.Count; i++)
		{
			num += means[i];
			stringBuilder.AppendLine($"{names[i],-18} {means[i] * 1000.0,8:F1} us/frame");
		}
		stringBuilder.AppendLine($"SUITE TOTAL {num * 1000.0:F1} us/frame");
		stringBuilder.AppendLine("---- details ----");
		for (int j = 0; j < details.Count; j++)
		{
			stringBuilder.AppendLine(details[j]);
		}
		lastBenchReport = stringBuilder.ToString();
		Debug.Log((object)lastBenchReport);
	}

	private static void SetModeAll(string modeName)
	{
		for (int i = 0; i < dummyAgents.Count; i++)
		{
			if ((Object)(object)dummyAgents[i] != (Object)null)
			{
				ConfigureAgent(dummyAgents[i], modeName);
			}
		}
	}
}

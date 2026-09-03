using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ConVar;
using Rust;
using Rust.Ai;
using Rust.Ai.Gen2.Nav;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class DungeonNavmesh : FacepunchBehaviour, IServerComponent
{
	public int NavMeshAgentTypeIndex;

	[Tooltip("The default area associated with the NavMeshAgent index.")]
	public string DefaultAreaName = "HumanNPC";

	public float NavmeshResolutionModifier = 1.25f;

	[Tooltip("Bounds which are auto calculated from CellSize * CellCount")]
	public Bounds Bounds;

	public NavMeshData NavMeshData;

	public NavMeshDataInstance NavMeshDataInstance;

	public LayerMask LayerMask;

	public NavMeshCollectGeometry NavMeshCollectGeometry;

	public static List<DungeonNavmesh> Instances = new List<DungeonNavmesh>();

	[ServerVar]
	public static bool use_baked_terrain_mesh = true;

	private List<NavMeshBuildSource> sources;

	private AsyncOperation BuildingOperation;

	private bool HasBuildOperationStarted;

	private Stopwatch BuildTimer = new Stopwatch();

	private int defaultArea;

	private int agentTypeId;

	private IndependantNavmesh independantNavmesh;

	public bool IsBuilding
	{
		get
		{
			if (AI.useUnityNavmesh)
			{
				if (HasBuildOperationStarted)
				{
					return BuildingOperation != null;
				}
				return true;
			}
			if ((Object)(object)independantNavmesh != (Object)null)
			{
				return !independantNavmesh.IsBuilt();
			}
			return true;
		}
	}

	public static bool NavReady()
	{
		if (Instances == null || Instances.Count == 0)
		{
			return true;
		}
		foreach (DungeonNavmesh instance in Instances)
		{
			if (instance.IsBuilding)
			{
				return false;
			}
		}
		return true;
	}

	private void OnEnable()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		if (AI.useUnityNavmesh)
		{
			NavMeshBuildSettings settingsByIndex = NavMesh.GetSettingsByIndex(NavMeshAgentTypeIndex);
			agentTypeId = ((NavMeshBuildSettings)(ref settingsByIndex)).agentTypeID;
			NavMeshData = new NavMeshData(agentTypeId);
			sources = new List<NavMeshBuildSource>();
			defaultArea = NavMesh.GetAreaFromName(DefaultAreaName);
			InvokeRepeating(FinishBuildingNavmesh, 0f, 1f);
		}
		Instances.Add(this);
	}

	private void OnDisable()
	{
		if (!Application.isQuitting)
		{
			if (AI.useUnityNavmesh)
			{
				CancelInvoke(FinishBuildingNavmesh);
				((NavMeshDataInstance)(ref NavMeshDataInstance)).Remove();
			}
			Instances.Remove(this);
		}
	}

	[ContextMenu("Update Monument Nav Mesh")]
	public void UpdateNavMeshAsync()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		RustNavigation.EnsureUnityNavmesh();
		if (!HasBuildOperationStarted && !AiManager.nav_disable && AI.npc_enable)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			NavMeshTools.Log("Starting Dungeon Navmesh Build with " + sources.Count + " sources");
			NavMeshBuildSettings settingsByIndex = NavMesh.GetSettingsByIndex(NavMeshAgentTypeIndex);
			((NavMeshBuildSettings)(ref settingsByIndex)).overrideVoxelSize = true;
			((NavMeshBuildSettings)(ref settingsByIndex)).voxelSize = ((NavMeshBuildSettings)(ref settingsByIndex)).voxelSize * NavmeshResolutionModifier;
			BuildingOperation = NavMeshBuilder.UpdateNavMeshDataAsync(NavMeshData, settingsByIndex, sources, Bounds);
			BuildTimer.Reset();
			BuildTimer.Start();
			HasBuildOperationStarted = true;
			float num = Time.realtimeSinceStartup - realtimeSinceStartup;
			if (num > 0.1f)
			{
				NavMeshTools.LogWarning("Calling UpdateNavMesh took " + num);
			}
			NotifyInformationZonesOfCompletion();
		}
	}

	public void NotifyInformationZonesOfCompletion()
	{
		RustNavigation.EnsureUnityNavmesh();
		foreach (AIInformationZone zone in AIInformationZone.zones)
		{
			zone.NavmeshBuildingComplete();
		}
	}

	public void SourcesCollected()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		RustNavigation.EnsureUnityNavmesh();
		int count = sources.Count;
		NavMeshTools.Log("Source count Pre cull : " + sources.Count);
		Vector3 val = default(Vector3);
		for (int num = sources.Count - 1; num >= 0; num--)
		{
			NavMeshBuildSource item = sources[num];
			Matrix4x4 transform = ((NavMeshBuildSource)(ref item)).transform;
			((Vector3)(ref val))._002Ector(((Matrix4x4)(ref transform))[0, 3], ((Matrix4x4)(ref transform))[1, 3], ((Matrix4x4)(ref transform))[2, 3]);
			bool flag = false;
			foreach (AIInformationZone zone in AIInformationZone.zones)
			{
				if (Vector3Ex.Distance2D(zone.ClosestPointTo(val), val) <= 50f)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				sources.Remove(item);
			}
		}
		NavMeshTools.Log("Source count post cull : " + sources.Count + " total removed : " + (count - sources.Count));
	}

	public IEnumerator UpdateNavMeshAndWait(IEnumerable<GameObject> roots)
	{
		if (AiManager.nav_disable || !AI.npc_enable)
		{
			yield break;
		}
		if (AI.useUnityNavmesh)
		{
			if (HasBuildOperationStarted)
			{
				yield break;
			}
			HasBuildOperationStarted = false;
			((Bounds)(ref Bounds)).center = ((Component)this).transform.position;
			((Bounds)(ref Bounds)).size = new Vector3(1000000f, 100000f, 100000f);
			IEnumerator enumerator = NavMeshTools.CollectSourcesAsync(roots, ((LayerMask)(ref LayerMask)).value, NavMeshCollectGeometry, defaultArea, sources, AppendModifierVolumes, UpdateNavMeshAsync);
			if (AiManager.nav_wait)
			{
				yield return enumerator;
				int lastPct = 0;
				while (!HasBuildOperationStarted)
				{
					yield return CoroutineEx.waitForSecondsRealtime(0.25f);
				}
				while (BuildingOperation != null)
				{
					int num = (int)(BuildingOperation.progress * 100f);
					if (lastPct != num)
					{
						NavMeshTools.Log($"{num}%");
						lastPct = num;
					}
					yield return CoroutineEx.waitForSecondsRealtime(0.25f);
					FinishBuildingNavmesh();
				}
			}
			else
			{
				((MonoBehaviour)this).StartCoroutine(enumerator);
				NavMeshTools.Log("nav_wait is false, so we're not waiting for the navmesh to finish generating. This might cause your server to sputter while it's generating.");
			}
		}
		else
		{
			if (!((Component)this).TryGetComponent<IndependantNavmesh>(ref independantNavmesh))
			{
				independantNavmesh = ((Component)this).gameObject.AddComponent<IndependantNavmesh>();
			}
			independantNavmesh.size = ((Bounds)(ref Bounds)).size;
			RustNavigation.Instance.AddNavmesh(independantNavmesh);
		}
	}

	private void AppendModifierVolumes(List<NavMeshBuildSource> sources)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		RustNavigation.EnsureUnityNavmesh();
		Vector3 size = default(Vector3);
		foreach (NavMeshModifierVolume activeModifier in NavMeshModifierVolume.activeModifiers)
		{
			if ((LayerMask.op_Implicit(LayerMask) & (1 << ((Component)activeModifier).gameObject.layer)) != 0 && activeModifier.AffectsAgentType(agentTypeId))
			{
				Vector3 val = ((Component)activeModifier).transform.TransformPoint(activeModifier.center);
				if (((Bounds)(ref Bounds)).Contains(val))
				{
					Vector3 lossyScale = ((Component)activeModifier).transform.lossyScale;
					((Vector3)(ref size))._002Ector(activeModifier.size.x * Mathf.Abs(lossyScale.x), activeModifier.size.y * Mathf.Abs(lossyScale.y), activeModifier.size.z * Mathf.Abs(lossyScale.z));
					NavMeshBuildSource item = default(NavMeshBuildSource);
					((NavMeshBuildSource)(ref item)).shape = (NavMeshBuildSourceShape)5;
					((NavMeshBuildSource)(ref item)).transform = Matrix4x4.TRS(val, ((Component)activeModifier).transform.rotation, Vector3.one);
					((NavMeshBuildSource)(ref item)).size = size;
					((NavMeshBuildSource)(ref item)).area = activeModifier.area;
					sources.Add(item);
				}
			}
		}
	}

	public void FinishBuildingNavmesh()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		RustNavigation.EnsureUnityNavmesh();
		if (BuildingOperation != null && BuildingOperation.isDone)
		{
			if (!((NavMeshDataInstance)(ref NavMeshDataInstance)).valid)
			{
				NavMeshDataInstance = NavMesh.AddNavMeshData(NavMeshData);
			}
			NavMeshTools.Log($"Monument Navmesh Build took {BuildTimer.Elapsed.TotalSeconds:0.00} seconds");
			BuildingOperation = null;
		}
	}
}

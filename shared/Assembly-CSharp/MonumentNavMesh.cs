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

public class MonumentNavMesh : FacepunchBehaviour, IServerComponent
{
	public int NavMeshAgentTypeIndex;

	[Tooltip("The default area associated with the NavMeshAgent index.")]
	public string DefaultAreaName = "HumanNPC";

	[Tooltip("How many cells to use squared")]
	public int CellCount = 1;

	[Tooltip("The size of each cell for async object gathering")]
	public int CellSize = 80;

	public int Height = 100;

	public float NavmeshResolutionModifier = 0.5f;

	[Tooltip("Use the bounds specified in editor instead of generating it from cellsize * cellcount")]
	public bool overrideAutoBounds;

	[Tooltip("Bounds which are auto calculated from CellSize * CellCount")]
	public Bounds Bounds;

	public NavMeshData NavMeshData;

	public NavMeshDataInstance NavMeshDataInstance;

	public LayerMask LayerMask;

	public NavMeshCollectGeometry NavMeshCollectGeometry;

	public bool forceCollectTerrain;

	public bool shouldNotifyAIZones = true;

	public Transform CustomNavMeshRoot;

	public bool IgnoreTerrain;

	public bool offsetBoundsByCenterPoint;

	[ServerVar]
	public static bool use_baked_terrain_mesh = true;

	private List<NavMeshBuildSource> sources;

	private AsyncOperation BuildingOperation;

	private bool HasBuildOperationStarted;

	private Stopwatch BuildTimer = new Stopwatch();

	private int defaultArea;

	private int agentTypeId;

	private bool isOffMainLand;

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
			if (!isOffMainLand)
			{
				return !RustNavigation.Instance.IsDefaultNavmeshBuilt();
			}
			if ((Object)(object)independantNavmesh != (Object)null)
			{
				return !independantNavmesh.IsBuilt();
			}
			return true;
		}
	}

	public Bounds GetBounds()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (!overrideAutoBounds)
		{
			((Bounds)(ref Bounds)).size = new Vector3((float)(CellSize * CellCount), (float)Height, (float)(CellSize * CellCount));
		}
		Bounds result = default(Bounds);
		((Bounds)(ref result))._002Ector(((Bounds)(ref Bounds)).center, ((Bounds)(ref Bounds)).size);
		if (offsetBoundsByCenterPoint)
		{
			((Bounds)(ref result)).center = ((Component)this).transform.TransformPoint(((Bounds)(ref Bounds)).center);
		}
		else
		{
			((Bounds)(ref result)).center = ((Component)this).transform.position;
		}
		return result;
	}

	private void OnEnable()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
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
		else
		{
			isOffMainLand = TerrainMeta.OutOfBounds(((Component)this).transform.position);
		}
	}

	private void OnDisable()
	{
		if (AI.useUnityNavmesh && !Application.isQuitting)
		{
			CancelInvoke(FinishBuildingNavmesh);
			((NavMeshDataInstance)(ref NavMeshDataInstance)).Remove();
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
			NavMeshTools.Log("Starting Monument Navmesh Build with " + sources.Count + " sources");
			NavMeshBuildSettings settingsByIndex = NavMesh.GetSettingsByIndex(NavMeshAgentTypeIndex);
			((NavMeshBuildSettings)(ref settingsByIndex)).overrideVoxelSize = true;
			((NavMeshBuildSettings)(ref settingsByIndex)).voxelSize = ((NavMeshBuildSettings)(ref settingsByIndex)).voxelSize * NavmeshResolutionModifier;
			BuildingOperation = NavMeshBuilder.UpdateNavMeshDataAsync(NavMeshData, settingsByIndex, sources, GetBounds());
			BuildTimer.Reset();
			BuildTimer.Start();
			HasBuildOperationStarted = true;
			float num = Time.realtimeSinceStartup - realtimeSinceStartup;
			if (num > 0.1f)
			{
				NavMeshTools.LogWarning("Calling UpdateNavMesh took " + num);
			}
			if (shouldNotifyAIZones)
			{
				NotifyInformationZonesOfCompletion();
			}
		}
	}

	public IEnumerator UpdateNavMeshAndWait()
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
			IEnumerator enumerator = NavMeshTools.CollectSourcesAsync(GetBounds(), LayerMask.op_Implicit(LayerMask), NavMeshCollectGeometry, defaultArea, use_baked_terrain_mesh && !forceCollectTerrain && !IgnoreTerrain, CellSize, sources, AppendModifierVolumes, UpdateNavMeshAsync, CustomNavMeshRoot);
			if (AiManager.nav_wait)
			{
				yield return enumerator;
			}
			else
			{
				((MonoBehaviour)this).StartCoroutine(enumerator);
			}
			if (!AiManager.nav_wait)
			{
				NavMeshTools.Log("nav_wait is false, so we're not waiting for the navmesh to finish generating. This might cause your server to sputter while it's generating.");
				yield break;
			}
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
		else if (isOffMainLand)
		{
			if (!((Component)this).TryGetComponent<IndependantNavmesh>(ref independantNavmesh))
			{
				independantNavmesh = ((Component)this).gameObject.AddComponent<IndependantNavmesh>();
			}
			IndependantNavmesh obj = independantNavmesh;
			Bounds bounds = GetBounds();
			obj.size = ((Bounds)(ref bounds)).size;
			RustNavigation.Instance.AddNavmesh(independantNavmesh);
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

	private void AppendModifierVolumes(List<NavMeshBuildSource> sources)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		RustNavigation.EnsureUnityNavmesh();
		Vector3 size = default(Vector3);
		foreach (NavMeshModifierVolume activeModifier in NavMeshModifierVolume.activeModifiers)
		{
			if ((LayerMask.op_Implicit(LayerMask) & (1 << ((Component)activeModifier).gameObject.layer)) != 0 && activeModifier.AffectsAgentType(agentTypeId))
			{
				Vector3 val = ((Component)activeModifier).transform.TransformPoint(activeModifier.center);
				Bounds bounds = GetBounds();
				if (((Bounds)(ref bounds)).Contains(val))
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

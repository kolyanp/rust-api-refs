using System.Collections;
using ConVar;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Prefabs.Misc;

public class GhostShip : JunkPileWater, IDeepSeaSpawner
{
	public GameObjectRef hackableLockedCratePrefab;

	public Transform[] crateSpawnPoints;

	public GameObjectRef mapMarkerPrefab;

	public BoatGroupSpawner boatGroupSpawner;

	private SpawnGroup[] _spawnGroups = new SpawnGroup[0];

	private BaseEntity spawnedMapMarker;

	private NavMeshDataInstance navMeshInst;

	private Matrix4x4 navMeshTransf;

	public override Matrix4x4 WorldToNavMeshSpace
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (!((NavMeshDataInstance)(ref navMeshInst)).valid)
			{
				return base.WorldToNavMeshSpace;
			}
			return navMeshTransf * ((Component)this).transform.worldToLocalMatrix;
		}
	}

	public override Matrix4x4 NavMeshToWorldSpace
	{
		get
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (!((NavMeshDataInstance)(ref navMeshInst)).valid)
			{
				return base.WorldToNavMeshSpace;
			}
			return ((Component)this).transform.localToWorldMatrix * ((Matrix4x4)(ref navMeshTransf)).inverse;
		}
	}

	protected override void StartTimeout()
	{
	}

	public override void ServerInit()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		DeepSeaManager.ServerGhostShips.Add(this);
		if (mapMarkerPrefab.isValid)
		{
			spawnedMapMarker = base.gameManager.CreateEntity(mapMarkerPrefab.resourcePath, ((Component)this).transform.position, ((Component)this).transform.rotation);
			spawnedMapMarker.Spawn();
		}
		if (AI.useUnityNavmesh)
		{
			NavMeshSurface componentInChildren = ((Component)this).GetComponentInChildren<NavMeshSurface>();
			if (Object.op_Implicit((Object)(object)componentInChildren) && (Object)(object)componentInChildren.navMeshData != (Object)null)
			{
				Vector3 position = ((Component)this).transform.position;
				Quaternion rotation = ((Component)this).transform.rotation;
				navMeshInst = NavMesh.AddNavMeshData(componentInChildren.navMeshData, position, rotation);
				((NavMeshDataInstance)(ref navMeshInst)).owner = (Object)(object)this;
				navMeshTransf = Matrix4x4.TRS(position, rotation, Vector3.one);
				if ((Object)(object)SingletonComponent<DynamicNavMesh>.Instance != (Object)null)
				{
					SingletonComponent<DynamicNavMesh>.Instance.IgnoreRoots.Add(((Component)this).transform);
				}
			}
		}
		if (CollectionEx.IsEmpty(_spawnGroups))
		{
			GetAllSpawnGroups();
		}
	}

	internal override void DoServerDestroy()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		base.DoServerDestroy();
		DeepSeaManager.ServerGhostShips.Remove(this);
		if (AI.useUnityNavmesh)
		{
			if (((NavMeshDataInstance)(ref navMeshInst)).valid)
			{
				NavMesh.RemoveNavMeshData(navMeshInst);
			}
			if ((Object)(object)SingletonComponent<DynamicNavMesh>.Instance != (Object)null)
			{
				SingletonComponent<DynamicNavMesh>.Instance.IgnoreRoots.Remove(((Component)this).transform);
			}
		}
		if ((Object)(object)spawnedMapMarker != (Object)null)
		{
			spawnedMapMarker.Kill();
		}
		spawnedMapMarker = null;
	}

	public void SpawnHackableLockedCrate()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (crateSpawnPoints != null)
		{
			Transform random = ArrayEx.GetRandom(crateSpawnPoints);
			if (!((Object)(object)random == (Object)null))
			{
				BaseEntity baseEntity = GameManager.server.CreateEntity(hackableLockedCratePrefab.resourcePath, random.position, random.rotation);
				baseEntity.Spawn();
				baseEntity.SetParent(this, worldPositionStays: true);
			}
		}
	}

	public override void OnEntityMessage(BaseEntity from, string msg)
	{
		if (from is HackableLockedCrate && msg == "HackingStarted")
		{
			boatGroupSpawner.SpawnBoatGroup(BoatAI.AILoadMode.KillBoat);
		}
	}

	public override bool ShouldJunkpileBeDestroyedBy(PlayerBoat boat)
	{
		return false;
	}

	public IEnumerator TriggerSpawnGroups()
	{
		SpawnGroup[] spawnGroups = _spawnGroups;
		foreach (SpawnGroup spawnGroup in spawnGroups)
		{
			bool flag = DeepSeaManager.IsRespawnVariant(spawnGroup);
			DeepSeaManager.ApplyPopulationScale(spawnGroup, flag ? DeepSea.loot_respawn_scale : DeepSea.loot_scale);
			if (!flag && (!spawnGroup.HasSpawnedAny() || !spawnGroup.wantsInitialSpawn))
			{
				spawnGroup.Spawn();
				yield return (object)new WaitForSeconds(DeepSea.spawngroups_spawninterval);
			}
		}
	}

	public void GetAllSpawnGroups()
	{
		_spawnGroups = ((Component)this).GetComponentsInChildren<SpawnGroup>();
	}

	public override bool ShouldChildrenInheritNetworkGroup()
	{
		return false;
	}

	private void OnDrawGizmos()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (crateSpawnPoints == null)
		{
			return;
		}
		Transform[] array = crateSpawnPoints;
		foreach (Transform val in array)
		{
			if (!((Object)(object)val == (Object)null))
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(val.position, 0.1f);
			}
		}
	}
}

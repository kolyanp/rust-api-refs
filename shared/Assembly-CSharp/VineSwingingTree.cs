using System;
using System.Collections.Generic;
using Facepunch;
using ProtoBuf;
using Rust;
using UnityEngine;

public class VineSwingingTree : TreeEntity
{
	public GameObjectRef StumpPrefab;

	public MeshRenderer[] BranchRenderers;

	public GameObject[] BranchRoots;

	public MeshRenderer BranchHighlightRenderer;

	public float VineSpawnHeight = 15f;

	public float VineSpawnRadius = 5f;

	public VineLaunchPoint[] LaunchPoints;

	public Collider[] ClimbColliders;

	public Collider StumpCollider;

	public List<EntityRef<VineMountable>> SpawnedVines = new List<EntityRef<VineMountable>>();

	public VineMountable GetSpawnedVine(VineLaunchPoint point)
	{
		int index = point.Index();
		EnsureVineArrayLength(index);
		return SpawnedVines[index].Get(base.isServer);
	}

	private void EnsureVineArrayLength(int index)
	{
		if (SpawnedVines.Count <= index)
		{
			while (SpawnedVines.Count <= index)
			{
				SpawnedVines.Add(default(EntityRef<VineMountable>));
			}
		}
	}

	public void SetSpawnedVine(VineLaunchPoint point, VineMountable vine)
	{
		int index = point.Index();
		EnsureVineArrayLength(index);
		EntityRef<VineMountable> value = default(EntityRef<VineMountable>);
		value.Set(vine);
		SpawnedVines[index] = value;
	}

	public Vector3 GetVineSpawnPos(List<VineLaunchPoint> possibleDestinations)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		if (possibleDestinations.Count == 0)
		{
			return Vector3.zero;
		}
		Vector3 val = Vector3.zero;
		foreach (VineLaunchPoint possibleDestination in possibleDestinations)
		{
			val += ((Component)possibleDestination).transform.position;
		}
		val /= (float)possibleDestinations.Count;
		Vector3 val2 = ((Component)this).transform.position + ((Component)this).transform.up * VineSpawnHeight;
		val = Vector3Ex.WithY(val, val2.y);
		Vector3 val3 = val - val2;
		Vector3 normalized = ((Vector3)(ref val3)).normalized;
		return val2 + normalized * VineSpawnRadius;
	}

	public override void InitShared()
	{
		base.InitShared();
		GameObject[] branchRoots = BranchRoots;
		foreach (GameObject val in branchRoots)
		{
			if ((Object)(object)val != (Object)null)
			{
				val.SetActive(true);
			}
		}
		if ((Object)(object)StumpCollider != (Object)null)
		{
			StumpCollider.enabled = false;
		}
	}

	public void RefreshVineState()
	{
		if (Application.isLoading)
		{
			Invoke(RefreshVineState, 0.25f);
			return;
		}
		VineLaunchPoint[] launchPoints = LaunchPoints;
		for (int i = 0; i < launchPoints.Length; i++)
		{
			launchPoints[i].SpawnVineIfPossible(this);
		}
	}

	public override void PostMapEntitySpawn()
	{
		base.PostMapEntitySpawn();
		RefreshVineState();
	}

	public override void ServerInit()
	{
		base.ServerInit();
		VineLaunchPoint[] launchPoints = LaunchPoints;
		for (int i = 0; i < launchPoints.Length; i++)
		{
			launchPoints[i].ServerInit();
		}
		Invoke(RefreshVineState, 0.25f);
	}

	internal override void DoServerDestroy()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		base.DoServerDestroy();
		VineLaunchPoint[] launchPoints = LaunchPoints;
		foreach (VineLaunchPoint vineLaunchPoint in launchPoints)
		{
			if ((Object)(object)vineLaunchPoint != (Object)null)
			{
				vineLaunchPoint.DoServerDestroy();
			}
		}
		if (StumpPrefab.isValid)
		{
			VineSwingingTreeStump obj = base.gameManager.CreateEntity(StumpPrefab.resourcePath, ((Component)this).transform.position, ((Component)this).transform.rotation) as VineSwingingTreeStump;
			obj.InitializeTree(this);
			obj.Spawn();
		}
	}

	public void NotifyNearbyTreesSpawned()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		PooledList<VineSwingingTree> val = Pool.Get<PooledList<VineSwingingTree>>();
		try
		{
			Vis.Entities(((Component)this).transform.position, 64f, (List<VineSwingingTree>)(object)val, 1073741824, (QueryTriggerInteraction)2);
			foreach (VineSwingingTree item in (List<VineSwingingTree>)(object)val)
			{
				if (!item.isClient && !((Object)(object)item == (Object)(object)this))
				{
					item.RefreshVineState();
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.vineTree = Pool.Get<VineTree>();
		info.msg.vineTree.spawnedVines = Pool.Get<List<NetworkableId>>();
		foreach (EntityRef<VineMountable> spawnedVine in SpawnedVines)
		{
			info.msg.vineTree.spawnedVines.Add(spawnedVine.uid);
		}
	}

	protected override void OnFallServer()
	{
		base.OnFallServer();
		ToggleClimbColliders(state: false);
		GameObject[] branchRoots = BranchRoots;
		foreach (GameObject val in branchRoots)
		{
			if ((Object)(object)val != (Object)null)
			{
				val.SetActive(false);
			}
		}
		PooledList<Collider> val2 = Pool.Get<PooledList<Collider>>();
		try
		{
			((Component)this).GetComponentsInChildren<Collider>((List<Collider>)(object)val2);
			foreach (Collider item in (List<Collider>)(object)val2)
			{
				if (!item.isTrigger)
				{
					item.enabled = false;
				}
			}
			VineLaunchPoint[] launchPoints = LaunchPoints;
			for (int i = 0; i < launchPoints.Length; i++)
			{
				launchPoints[i].DoServerDestroy();
			}
			if ((Object)(object)StumpCollider != (Object)null)
			{
				StumpCollider.enabled = true;
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	public override void OnAttacked(HitInfo info)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		base.OnAttacked(info);
		if ((Object)(object)info.InitiatorPlayer == (Object)null || base.isClient)
		{
			return;
		}
		PooledList<VineMountable> val = Pool.Get<PooledList<VineMountable>>();
		try
		{
			VineMountable.pointGrid.Query(((Component)this).transform.position.x, ((Component)this).transform.position.z, 10f, (List<VineMountable>)(object)val);
			foreach (VineMountable item in (List<VineMountable>)(object)val)
			{
				if (!item.IsOn())
				{
					VineLaunchPoint vineLaunchPoint = item.currentLocation.Get(isServer: true);
					if ((Object)(object)vineLaunchPoint != (Object)null && (Object)(object)vineLaunchPoint.ParentTree == (Object)(object)this && item.AttackedByPlayer(info.InitiatorPlayer))
					{
						break;
					}
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.vineTree == null || info.msg.vineTree.spawnedVines == null)
		{
			return;
		}
		SpawnedVines.Clear();
		foreach (NetworkableId spawnedVine in info.msg.vineTree.spawnedVines)
		{
			SpawnedVines.Add(new EntityRef<VineMountable>(spawnedVine));
		}
	}

	private void ToggleClimbColliders(bool state)
	{
		Collider[] climbColliders = ClimbColliders;
		foreach (Collider val in climbColliders)
		{
			if ((Object)(object)val != (Object)null)
			{
				((Component)val).gameObject.SetActive(state);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class VineSwingingTreeStump : BaseEntity
{
	public GameObjectRef[] TreePrefabs;

	public float MaxTreeRespawnTime = 5f;

	public float MinTreeRespawnTime = 10f;

	public GameObject PreventBuildingVolume;

	private TimeUntil treeRespawnTime;

	private int treeToRespawn;

	public void InitializeTree(VineSwingingTree fromTree)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		treeRespawnTime = TimeUntil.op_Implicit(Random.Range(MinTreeRespawnTime, MaxTreeRespawnTime));
		Invoke(RespawnTreeInvoke, TimeUntil.op_Implicit(treeRespawnTime));
		treeToRespawn = 0;
		for (int i = 0; i < TreePrefabs.Length; i++)
		{
			if (TreePrefabs[i].resourceID == fromTree.prefabID)
			{
				treeToRespawn = i;
				break;
			}
		}
	}

	private void RespawnTreeInvoke()
	{
		RespawnTree();
	}

	public bool RespawnTree()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		GameObjectRef gameObjectRef = TreePrefabs[Mathf.Clamp(treeToRespawn, 0, TreePrefabs.Length)];
		if (gameObjectRef.isValid)
		{
			if (!IsTreeRespawnClear())
			{
				Invoke(RespawnTreeInvoke, 10f);
				return false;
			}
			VineSwingingTree obj = base.gameManager.CreateEntity(gameObjectRef.resourcePath, ((Component)this).transform.position, ((Component)this).transform.rotation) as VineSwingingTree;
			obj.Spawn();
			obj.NotifyNearbyTreesSpawned();
			Kill();
			return true;
		}
		return false;
	}

	private bool IsTreeRespawnClear()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		PooledList<Collider> val = Pool.Get<PooledList<Collider>>();
		try
		{
			PreventBuildingVolume.GetComponents<Collider>((List<Collider>)(object)val);
			Vector3 val4 = default(Vector3);
			foreach (Collider item in (List<Collider>)(object)val)
			{
				BoxCollider val2 = (BoxCollider)(object)((item is BoxCollider) ? item : null);
				if (val2 != null)
				{
					if (GamePhysics.CheckOBB(new OBB(PreventBuildingVolume.transform, new Bounds(val2.center, val2.size)), 131072, (QueryTriggerInteraction)0))
					{
						return false;
					}
					continue;
				}
				CapsuleCollider val3 = (CapsuleCollider)(object)((item is CapsuleCollider) ? item : null);
				if (val3 != null)
				{
					((Vector3)(ref val4))._002Ector(0f, val3.height * 0.5f, 0f);
					if (GamePhysics.CheckCapsule(((Component)item).transform.TransformPoint(val3.center + val4), ((Component)item).transform.TransformPoint(val3.center - val4), val3.radius, 131072, (QueryTriggerInteraction)0))
					{
						return false;
					}
				}
			}
			return true;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.treeRespawn = Pool.Get<TreeRespawn>();
			info.msg.treeRespawn.timeToRespawn = TimeUntil.op_Implicit(treeRespawnTime);
			info.msg.treeRespawn.treeIndex = treeToRespawn;
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.fromDisk && info.msg.treeRespawn != null)
		{
			treeRespawnTime = TimeUntil.op_Implicit(info.msg.treeRespawn.timeToRespawn);
			treeToRespawn = info.msg.treeRespawn.treeIndex;
			Invoke(RespawnTreeInvoke, TimeUntil.op_Implicit(treeRespawnTime));
		}
	}
}

using System;
using System.Collections.Generic;
using Facepunch;
using Oxide.Core;
using UnityEngine;

public class BoatGroupSpawner : BaseMonoBehaviour
{
	public float radius = 30f;

	public float protectionAreaRadius = 80f;

	public void SpawnBoatGroup(BoatAI.AILoadMode loadMode = BoatAI.AILoadMode.LoadAi)
	{
		PooledHashSet<RHIB> val = Pool.Get<PooledHashSet<RHIB>>();
		try
		{
			SpawnBoatGroup((HashSet<RHIB>)(object)val, loadMode);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void SpawnBoatGroup(HashSet<RHIB> list, BoatAI.AILoadMode loadMode = BoatAI.AILoadMode.LoadAi, bool spawnsPT = true, ScientistBoatOilrigManager manager = null)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(((Component)this).transform.position.x, ((Component)this).transform.position.z);
		if (!BoatAI.FindBoatSpawnPositionInRadius(val, radius, out var position))
		{
			return;
		}
		Vector2 val2 = val - position;
		Vector3 val3 = Vector2.op_Implicit(((Vector2)(ref val2)).normalized);
		bool flag = false;
		if ((Object)(object)PointEntity<DeepSeaManager>.ServerInstance != (Object)null)
		{
			flag = DeepSeaManager.IsInsideDeepSea(Vector2.op_Implicit(val));
		}
		val3.y = 0f;
		Quaternion val4 = Quaternion.LookRotation(val3);
		if (Interface.CallHook("OnBoatGroupSpawn", this, position, val4, list, flag, spawnsPT) != null)
		{
			return;
		}
		BoatAI.SpawnBoatGroup(position, val4, list, flag, spawnsPT);
		foreach (RHIB item in list)
		{
			BoatAI componentInChildren = ((Component)item).GetComponentInChildren<BoatAI>();
			componentInChildren.MoveTo(((Component)this).transform.position);
			componentInChildren.SetProtectionArea(((Component)this).transform.position, protectionAreaRadius);
			componentInChildren.LoadMode = loadMode;
			componentInChildren.SetOilRigManager(manager);
		}
		Interface.CallHook("OnBoatGroupSpawned", this, position, val4, list, flag, spawnsPT);
	}
}

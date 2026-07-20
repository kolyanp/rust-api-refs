using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class ClawMarkSpawner : EntityComponent<BaseEntity>, IServerComponent
{
	public GameObjectRef clawDecal;

	public float radius = 130f;

	public float height = 1.8f;

	public float minTreeRadius = 1.4f;

	[Range(0f, 1f)]
	public float ratioOfTreesMarked = 0.5f;

	private static bool showClawMarks;

	private List<ClawMark> clawMarks = new List<ClawMark>();

	[ServerVar]
	public static void ShowClawMarks(ConsoleSystem.Arg arg)
	{
		bool flag = arg.GetBool(0, !showClawMarks);
		if (flag == showClawMarks)
		{
			arg.ReplyWith("Claw marks are already " + (showClawMarks ? "visible" : "hidden") + ".");
			return;
		}
		showClawMarks = flag;
		BaseNPC2[] array;
		if (showClawMarks)
		{
			array = BaseEntity.Util.FindAll<BaseNPC2>();
			for (int i = 0; i < array.Length; i++)
			{
				ClawMarkSpawner component = ((Component)array[i]).GetComponent<ClawMarkSpawner>();
				if ((Object)(object)component != (Object)null)
				{
					component.SpawnClawMarks();
				}
			}
			arg.ReplyWith("Claw marks are now visible.");
			return;
		}
		array = BaseEntity.Util.FindAll<BaseNPC2>();
		for (int i = 0; i < array.Length; i++)
		{
			ClawMarkSpawner component2 = ((Component)array[i]).GetComponent<ClawMarkSpawner>();
			if ((Object)(object)component2 != (Object)null)
			{
				component2.ClearClawMarks();
			}
		}
		arg.ReplyWith("Claw marks are now hidden.");
	}

	public override void InitShared()
	{
		UpdateBaseEntity();
		if (showClawMarks)
		{
			SpawnClawMarks();
		}
	}

	public override void DestroyShared()
	{
		ClearClawMarks();
	}

	private void SpawnClawMarks()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		if (!base.baseEntity.isServer)
		{
			return;
		}
		if (AI.logIssues && clawMarks.Count > 0)
		{
			Debug.LogWarning((object)$"Claw marks already spawned for {base.baseEntity}.");
			return;
		}
		PooledList<TreeEntity> val = Pool.Get<PooledList<TreeEntity>>();
		try
		{
			BaseEntity.Query.Server.GetInSphere(((Component)base.baseEntity).transform.position, radius, (List<TreeEntity>)(object)val);
			clawMarks.Capacity = ((List<TreeEntity>)(object)val).Count;
			RaycastHit val3 = default(RaycastHit);
			foreach (TreeEntity item in (List<TreeEntity>)(object)val)
			{
				if (Random.value > ratioOfTreesMarked || (Object)(object)item.serverCollider == (Object)null)
				{
					continue;
				}
				float num = Mathf.Min(((Bounds)(ref item.bounds)).extents.x, ((Bounds)(ref item.bounds)).extents.z);
				if (num < minTreeRadius)
				{
					continue;
				}
				Vector3 val2 = ((Component)item).transform.position + Vector3.up * height;
				Vector3 forward = ((Component)item).transform.forward;
				if (!item.serverCollider.Raycast(new Ray(val2 - forward * num, forward), ref val3, num))
				{
					continue;
				}
				ClawMark clawMark = GameManager.server.CreateEntity(clawDecal.resourcePath, ((RaycastHit)(ref val3)).point, Quaternion.LookRotation(-((RaycastHit)(ref val3)).normal)) as ClawMark;
				if ((Object)(object)clawMark == (Object)null)
				{
					if (AI.logIssues)
					{
						Debug.LogWarning((object)$"Failed to create claw mark for {base.baseEntity}.");
					}
				}
				else
				{
					clawMarks.Add(clawMark);
					clawMark.SetParent(item, worldPositionStays: true);
					clawMark.Spawn();
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void ClearClawMarks()
	{
		if (!base.baseEntity.isServer)
		{
			return;
		}
		foreach (ClawMark clawMark in clawMarks)
		{
			clawMark.Kill();
		}
		clawMarks.Clear();
	}
}

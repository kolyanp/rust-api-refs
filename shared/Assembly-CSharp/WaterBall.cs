using System.Collections.Generic;
using Facepunch;
using Oxide.Core;
using UnityEngine;

public class WaterBall : BaseEntity
{
	public ItemDefinition liquidType;

	public int waterAmount;

	public GameObjectRef waterExplosion;

	public Collider waterCollider;

	public Rigidbody myRigidBody;

	public override void ServerInit()
	{
		base.ServerInit();
		Invoke(Extinguish, 10f);
	}

	public void Extinguish()
	{
		CancelInvoke(Extinguish);
		if (!base.IsDestroyed)
		{
			Kill();
		}
	}

	public void FixedUpdate()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer && (Object)(object)myRigidBody != (Object)null)
		{
			myRigidBody.AddForce(Physics.gravity, (ForceMode)5);
		}
	}

	public static bool DoSplash(Vector3 position, float radius, ItemDefinition liquidDef, int amount, bool funWater = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		object obj = Interface.CallHook("CanWaterBallSplash", liquidDef, position, radius, amount, funWater);
		if (obj is bool)
		{
			return (bool)obj;
		}
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		Vis.Entities(position, radius, list, 1220225811, (QueryTriggerInteraction)2);
		int num = 0;
		int num2 = amount;
		bool flag = false;
		while (amount > 0 && num < 3)
		{
			List<ISplashable> list2 = Pool.Get<List<ISplashable>>();
			foreach (BaseEntity item in list)
			{
				if (item.isClient || !(item is ISplashable splashable) || list2.Contains(splashable) || !splashable.WantsSplash(liquidDef, amount))
				{
					continue;
				}
				bool flag2 = true;
				bool flag3 = item is PlanterBox;
				bool flag4 = item is TimedExplosive;
				if (flag3 || flag4)
				{
					Vector3 val = Vector3.zero;
					if (flag3)
					{
						val = Vector3.up;
					}
					if (!GamePhysics.LineOfSight(((Component)item).transform.position + val, position, 136314880))
					{
						flag2 = false;
					}
					if (flag2)
					{
						flag = true;
					}
				}
				if (flag2)
				{
					list2.Add(splashable);
				}
			}
			if (list2.Count == 0)
			{
				break;
			}
			int num3 = Mathf.CeilToInt((float)(amount / list2.Count));
			foreach (ISplashable item2 in list2)
			{
				if (!flag || !(item2 is BasePlayer))
				{
					int num4 = 0;
					BasePlayer basePlayer = item2 as BasePlayer;
					num4 = ((!(basePlayer != null && funWater)) ? item2.DoSplash(liquidDef, Mathf.Min(amount, num3)) : basePlayer.DoSplashFunWater(liquidDef, Mathf.Min(amount, num3)));
					amount -= num4;
					if (amount <= 0)
					{
						break;
					}
				}
			}
			Pool.FreeUnmanaged<ISplashable>(ref list2);
			num++;
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
		return amount < num2;
	}

	private void OnCollisionEnter(Collision collision)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (!base.isClient && !myRigidBody.isKinematic)
		{
			float num = 2.5f;
			Vector3 position = ((Component)this).transform.position;
			float num2 = num * 0.75f;
			if (GamePhysics.Trace(new Ray(position, Vector3.up), 0.05f, out var hitInfo, num2, 1084293377, (QueryTriggerInteraction)0))
			{
				num2 = ((RaycastHit)(ref hitInfo)).distance;
			}
			DoSplash(position + new Vector3(0f, num2, 0f), num, liquidType, waterAmount);
			Effect.server.Run(waterExplosion.resourcePath, position, Vector3.up);
			myRigidBody.isKinematic = true;
			waterCollider.enabled = false;
			Invoke(Extinguish, 2f);
		}
	}
}

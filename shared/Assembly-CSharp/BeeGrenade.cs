using UnityEngine;

public class BeeGrenade : TimedExplosive
{
	public GameObjectRef beeSwarmPrefab;

	[Header("Spawning Settings")]
	public int beeSwarmAmount = 1;

	public float spawnRadius = 2f;

	private const int mask = -928830719;

	public override void Explode()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		if (beeSwarmPrefab.isValid && !WaterLevel.Test(((Component)this).transform.position, waves: true, volumes: true, this))
		{
			RaycastHit val4 = default(RaycastHit);
			RaycastHit val6 = default(RaycastHit);
			RaycastHit val7 = default(RaycastHit);
			for (int i = 0; i < Mathf.Max(1, beeSwarmAmount); i++)
			{
				Vector3 val = ((Component)this).transform.position;
				Vector3 val5;
				if (beeSwarmAmount > 1)
				{
					Vector2 val2 = Random.insideUnitCircle * spawnRadius;
					Vector3 val3 = ((Component)this).transform.position + new Vector3(val2.x, 0f, val2.y);
					if (Physics.Linecast(((Component)this).transform.position, val3, ref val4, -928830719))
					{
						Vector3 point = ((RaycastHit)(ref val4)).point;
						val5 = ((Component)this).transform.position - point;
						Vector3 normalized = ((Vector3)(ref val5)).normalized;
						val = point + normalized * 1.5f;
					}
					else
					{
						val5 = ((Component)this).transform.position - ((Component)this).transform.position;
						Vector3 normalized2 = ((Vector3)(ref val5)).normalized;
						val = val3;
						val += normalized2 * 0.5f;
					}
				}
				if (Physics.Raycast(new Ray(val + Vector3.up * 0.5f, Vector3.down), ref val6, 2f, -928830719))
				{
					val.y = ((RaycastHit)(ref val6)).point.y;
				}
				val += Vector3.up * 1.5f;
				if (Physics.Linecast(((Component)this).transform.position, val, ref val7, -928830719))
				{
					val = ((RaycastHit)(ref val7)).point;
				}
				if ((Object)(object)creatorPlayer != (Object)null)
				{
					val5 = ((Component)creatorPlayer).transform.position - ((Component)this).transform.position;
					Vector3 normalized3 = ((Vector3)(ref val5)).normalized;
					val += normalized3;
				}
				BaseEntity baseEntity = GameManager.server.CreateEntity(beeSwarmPrefab.resourcePath, val, Quaternion.identity);
				if ((Object)(object)creatorPlayer != (Object)null)
				{
					baseEntity.OwnerID = creatorPlayer.userID;
					baseEntity.creatorEntity = creatorPlayer;
				}
				baseEntity.Spawn();
			}
		}
		base.Explode();
	}

	public void DelayedDestroy()
	{
		Kill(DestroyMode.Gib);
	}
}

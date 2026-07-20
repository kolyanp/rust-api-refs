using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class CH47PathFinder : BasePathFinder
{
	public List<Vector3> visitedPatrolPoints = new List<Vector3>();

	public override Vector3 GetRandomPatrolPoint()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		MonumentInfo monumentInfo = null;
		if ((Object)(object)TerrainMeta.Path != (Object)null && TerrainMeta.Path.Monuments != null && TerrainMeta.Path.Monuments.Count > 0)
		{
			PooledList<MonumentInfo> val2 = Pool.Get<PooledList<MonumentInfo>>();
			try
			{
				foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
				{
					if (monument.Type == MonumentType.Cave || monument.Type == MonumentType.WaterWell || monument.Tier == MonumentTier.Tier0 || monument.IsSafeZone || (monument.Tier & MonumentTier.Tier0) > (MonumentTier)0)
					{
						continue;
					}
					bool flag = false;
					foreach (Vector3 visitedPatrolPoint in visitedPatrolPoints)
					{
						if (Vector3Ex.Distance2D(((Component)monument).transform.position, visitedPatrolPoint) < 100f)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						((List<MonumentInfo>)(object)val2).Add(monument);
					}
				}
				if (((List<MonumentInfo>)(object)val2).Count > 0)
				{
					int index = Random.Range(0, ((List<MonumentInfo>)(object)val2).Count);
					monumentInfo = ((List<MonumentInfo>)(object)val2)[index];
				}
				if ((Object)(object)monumentInfo == (Object)null)
				{
					visitedPatrolPoints.Clear();
					monumentInfo = GetRandomValidMonumentInfo();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		if ((Object)(object)monumentInfo != (Object)null)
		{
			visitedPatrolPoints.Add(((Component)monumentInfo).transform.position);
			val = ((Component)monumentInfo).transform.position;
		}
		else
		{
			float x = TerrainMeta.Size.x;
			float y = 30f;
			val = Vector3Ex.Range(-1f, 1f);
			val.y = 0f;
			((Vector3)(ref val)).Normalize();
			val *= x * Random.Range(0f, 0.75f);
			val.y = y;
		}
		float waterOrTerrainSurface = WaterLevel.GetWaterOrTerrainSurface(val, waves: false, volumes: false);
		float num = waterOrTerrainSurface;
		RaycastHit val3 = default(RaycastHit);
		if (Physics.SphereCast(val + new Vector3(0f, 200f, 0f), 20f, Vector3.down, ref val3, 300f, 1218511105))
		{
			num = Mathf.Max(((RaycastHit)(ref val3)).point.y, waterOrTerrainSurface);
		}
		val.y = num + 30f;
		return val;
	}

	private MonumentInfo GetRandomValidMonumentInfo()
	{
		int count = TerrainMeta.Path.Monuments.Count;
		int num = Random.Range(0, count);
		for (int i = 0; i < count; i++)
		{
			int num2 = i + num;
			if (num2 >= count)
			{
				num2 -= count;
			}
			MonumentInfo monumentInfo = TerrainMeta.Path.Monuments[num2];
			if (monumentInfo.Type != MonumentType.Cave && monumentInfo.Type != MonumentType.WaterWell && monumentInfo.Tier != MonumentTier.Tier0 && !monumentInfo.IsSafeZone)
			{
				return monumentInfo;
			}
		}
		return null;
	}
}

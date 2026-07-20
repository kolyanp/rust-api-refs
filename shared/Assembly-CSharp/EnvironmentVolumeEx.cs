using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public static class EnvironmentVolumeEx
{
	public static bool CheckEnvironmentVolumes(this Transform transform, Vector3 pos, Quaternion rot, Vector3 scale, EnvironmentType type)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		List<EnvironmentVolume> list = Pool.Get<List<EnvironmentVolume>>();
		((Component)transform).GetComponentsInChildren<EnvironmentVolume>(true, list);
		OBB obb = default(OBB);
		for (int i = 0; i < list.Count; i++)
		{
			EnvironmentVolume environmentVolume = list[i];
			((OBB)(ref obb))._002Ector(((Component)environmentVolume).transform, new Bounds(environmentVolume.Center, environmentVolume.Size));
			((OBB)(ref obb)).Transform(pos, scale, rot);
			if (EnvironmentManager.Check(obb, type))
			{
				Pool.FreeUnmanaged<EnvironmentVolume>(ref list);
				return true;
			}
		}
		Pool.FreeUnmanaged<EnvironmentVolume>(ref list);
		return false;
	}

	public static bool CheckEnvironmentVolumes(this Transform transform, EnvironmentType type)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return transform.CheckEnvironmentVolumes(transform.position, transform.rotation, transform.lossyScale, type);
	}

	public static bool CheckEnvironmentVolumesInsideTerrain(this Transform transform, Vector3 pos, Quaternion rot, Vector3 scale, EnvironmentType typeRequire, EnvironmentType typeIgnore, float padding = 0f)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TerrainMeta.HeightMap == (Object)null)
		{
			return true;
		}
		List<EnvironmentVolume> list = Pool.Get<List<EnvironmentVolume>>();
		((Component)transform).GetComponentsInChildren<EnvironmentVolume>(true, list);
		if (list.Count == 0)
		{
			Pool.FreeUnmanaged<EnvironmentVolume>(ref list);
			return true;
		}
		OBB val = default(OBB);
		for (int i = 0; i < list.Count; i++)
		{
			EnvironmentVolume environmentVolume = list[i];
			if ((environmentVolume.Type & typeRequire) == 0 || (environmentVolume.Type & typeIgnore) != 0)
			{
				continue;
			}
			((OBB)(ref val))._002Ector(((Component)environmentVolume).transform, new Bounds(environmentVolume.Center, environmentVolume.Size));
			((OBB)(ref val)).Transform(pos, scale, rot);
			Vector3 point = ((OBB)(ref val)).GetPoint(-1f, 0f, -1f);
			Vector3 point2 = ((OBB)(ref val)).GetPoint(1f, 0f, -1f);
			Vector3 point3 = ((OBB)(ref val)).GetPoint(-1f, 0f, 1f);
			Vector3 point4 = ((OBB)(ref val)).GetPoint(1f, 0f, 1f);
			Bounds val2 = ((OBB)(ref val)).ToBounds();
			float max = ((Bounds)(ref val2)).max.y + padding;
			bool fail = false;
			TerrainMeta.HeightMap.ForEachParallel(point, point2, point3, point4, delegate(int x, int z)
			{
				if (TerrainMeta.HeightMap.GetHeight(x, z) <= max)
				{
					fail = true;
				}
			});
			if (fail)
			{
				Pool.FreeUnmanaged<EnvironmentVolume>(ref list);
				return false;
			}
		}
		Pool.FreeUnmanaged<EnvironmentVolume>(ref list);
		return true;
	}

	public static bool CheckEnvironmentVolumesInsideTerrain(this Transform transform, EnvironmentType typeRequire, EnvironmentType typeIgnore)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return transform.CheckEnvironmentVolumesInsideTerrain(transform.position, transform.rotation, transform.lossyScale, typeRequire, typeIgnore);
	}

	public static bool CheckEnvironmentVolumesOutsideTerrain(this Transform transform, Vector3 pos, Quaternion rot, Vector3 scale, EnvironmentType typeRequire, EnvironmentType typeIgnore, float padding = 0f)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TerrainMeta.HeightMap == (Object)null)
		{
			return true;
		}
		List<EnvironmentVolume> list = Pool.Get<List<EnvironmentVolume>>();
		((Component)transform).GetComponentsInChildren<EnvironmentVolume>(true, list);
		if (list.Count == 0)
		{
			Pool.FreeUnmanaged<EnvironmentVolume>(ref list);
			return true;
		}
		OBB val = default(OBB);
		for (int i = 0; i < list.Count; i++)
		{
			EnvironmentVolume environmentVolume = list[i];
			if ((environmentVolume.Type & typeRequire) == 0 || (environmentVolume.Type & typeIgnore) != 0)
			{
				continue;
			}
			((OBB)(ref val))._002Ector(((Component)environmentVolume).transform, new Bounds(environmentVolume.Center, environmentVolume.Size));
			((OBB)(ref val)).Transform(pos, scale, rot);
			Vector3 point = ((OBB)(ref val)).GetPoint(-1f, 0f, -1f);
			Vector3 point2 = ((OBB)(ref val)).GetPoint(1f, 0f, -1f);
			Vector3 point3 = ((OBB)(ref val)).GetPoint(-1f, 0f, 1f);
			Vector3 point4 = ((OBB)(ref val)).GetPoint(1f, 0f, 1f);
			Bounds val2 = ((OBB)(ref val)).ToBounds();
			float min = ((Bounds)(ref val2)).min.y - padding;
			bool fail = false;
			TerrainMeta.HeightMap.ForEachParallel(point, point2, point3, point4, delegate(int x, int z)
			{
				if (TerrainMeta.HeightMap.GetHeight(x, z) >= min)
				{
					fail = true;
				}
			});
			if (fail)
			{
				Pool.FreeUnmanaged<EnvironmentVolume>(ref list);
				return false;
			}
		}
		Pool.FreeUnmanaged<EnvironmentVolume>(ref list);
		return true;
	}

	public static bool CheckEnvironmentVolumesOutsideTerrain(this Transform transform, EnvironmentType typeRequire, EnvironmentType typeIgnore)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return transform.CheckEnvironmentVolumesOutsideTerrain(transform.position, transform.rotation, transform.lossyScale, typeRequire, typeIgnore);
	}

	public static bool CheckEnvironmentVolumesAboveAltitude(this Transform transform, Vector3 pos, Quaternion rot, Vector3 scale, EnvironmentType typeRequire, EnvironmentType typeIgnore, float altitude = 0f)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		List<EnvironmentVolume> list = Pool.Get<List<EnvironmentVolume>>();
		((Component)transform).GetComponentsInChildren<EnvironmentVolume>(true, list);
		if (list.Count == 0)
		{
			Pool.FreeUnmanaged<EnvironmentVolume>(ref list);
			return true;
		}
		OBB val = default(OBB);
		for (int i = 0; i < list.Count; i++)
		{
			EnvironmentVolume environmentVolume = list[i];
			if ((environmentVolume.Type & typeRequire) != 0 && (environmentVolume.Type & typeIgnore) == 0)
			{
				((OBB)(ref val))._002Ector(((Component)environmentVolume).transform, new Bounds(environmentVolume.Center, environmentVolume.Size));
				((OBB)(ref val)).Transform(pos, scale, rot);
				Bounds val2 = ((OBB)(ref val)).ToBounds();
				if (((Bounds)(ref val2)).min.y <= altitude)
				{
					Pool.FreeUnmanaged<EnvironmentVolume>(ref list);
					return false;
				}
			}
		}
		Pool.FreeUnmanaged<EnvironmentVolume>(ref list);
		return true;
	}

	public static bool CheckEnvironmentVolumesAboveAltitude(this Transform transform, EnvironmentType typeRequire, EnvironmentType typeIgnore)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return transform.CheckEnvironmentVolumesAboveAltitude(transform.position, transform.rotation, transform.lossyScale, typeRequire, typeIgnore);
	}

	public static bool CheckEnvironmentVolumesBelowAltitude(this Transform transform, Vector3 pos, Quaternion rot, Vector3 scale, EnvironmentType typeRequire, EnvironmentType typeIgnore, float altitude = 0f)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		List<EnvironmentVolume> list = Pool.Get<List<EnvironmentVolume>>();
		((Component)transform).GetComponentsInChildren<EnvironmentVolume>(true, list);
		if (list.Count == 0)
		{
			Pool.FreeUnmanaged<EnvironmentVolume>(ref list);
			return true;
		}
		OBB val = default(OBB);
		for (int i = 0; i < list.Count; i++)
		{
			EnvironmentVolume environmentVolume = list[i];
			if ((environmentVolume.Type & typeRequire) != 0 && (environmentVolume.Type & typeIgnore) == 0)
			{
				((OBB)(ref val))._002Ector(((Component)environmentVolume).transform, new Bounds(environmentVolume.Center, environmentVolume.Size));
				((OBB)(ref val)).Transform(pos, scale, rot);
				Bounds val2 = ((OBB)(ref val)).ToBounds();
				if (((Bounds)(ref val2)).max.y >= altitude)
				{
					Pool.FreeUnmanaged<EnvironmentVolume>(ref list);
					return false;
				}
			}
		}
		Pool.FreeUnmanaged<EnvironmentVolume>(ref list);
		return true;
	}

	public static bool CheckEnvironmentVolumesBelowAltitude(this Transform transform, EnvironmentType typeRequire, EnvironmentType typeIgnore)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return transform.CheckEnvironmentVolumesBelowAltitude(transform.position, transform.rotation, transform.lossyScale, typeRequire, typeIgnore);
	}
}

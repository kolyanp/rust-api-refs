using System.Collections.Generic;
using UnityEngine;

public class SpawnDistribution
{
	internal SpawnHandler Handler;

	public float Density;

	public int Count;

	private WorldSpaceGrid<int> grid;

	private Dictionary<uint, int> dict = new Dictionary<uint, int>();

	private ByteQuadtree quadtree = new ByteQuadtree();

	private Vector3 origin;

	private Vector3 area;

	public SpawnDistribution(SpawnHandler handler, byte[] baseValues, Vector3 origin, Vector3 area)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		Handler = handler;
		quadtree.UpdateValues(baseValues);
		this.origin = origin;
		float num = 0f;
		for (int i = 0; i < baseValues.Length; i++)
		{
			num += (float)(int)baseValues[i];
		}
		Density = num / (float)(255 * baseValues.Length);
		Count = 0;
		this.area = new Vector3(area.x / (float)quadtree.Size, area.y, area.z / (float)quadtree.Size);
		grid = new WorldSpaceGrid<int>(area.x, 20f, (RoundingMode)0);
	}

	public bool Sample(out Vector3 spawnPos, out Quaternion spawnRot, bool alignToNormal = false, float dithering = 0f, float radius = 0f, SpawnFilter filter = null, float cutoff = 0f)
	{
		return Sample(out spawnPos, out spawnRot, SampleNode(), alignToNormal, dithering, radius, filter, cutoff);
	}

	public bool Sample(out Vector3 spawnPos, out Quaternion spawnRot, ByteQuadtree.Element node, bool alignToNormal = false, float dithering = 0f, float radius = 0f, SpawnFilter filter = null, float cutoff = 0f)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Handler == (Object)null || (Object)(object)TerrainMeta.HeightMap == (Object)null)
		{
			spawnPos = Vector3.zero;
			spawnRot = Quaternion.identity;
			return false;
		}
		LayerMask placementMask = Handler.PlacementMask;
		LayerMask placementCheckMask = Handler.PlacementCheckMask;
		float placementCheckHeight = Handler.PlacementCheckHeight;
		LayerMask radiusCheckMask = Handler.RadiusCheckMask;
		float radiusCheckDistance = Handler.RadiusCheckDistance;
		Vector3 val = default(Vector3);
		RaycastHit val2 = default(RaycastHit);
		for (int i = 0; i < 15; i++)
		{
			spawnPos = origin;
			spawnPos.x += node.Coords.x * area.x;
			spawnPos.z += node.Coords.y * area.z;
			spawnPos.x += Random.value * area.x;
			spawnPos.z += Random.value * area.z;
			spawnPos.x += Random.Range(0f - dithering, dithering);
			spawnPos.z += Random.Range(0f - dithering, dithering);
			((Vector3)(ref val))._002Ector(spawnPos.x, TerrainMeta.HeightMap.GetHeight(spawnPos), spawnPos.z);
			if (val.y <= spawnPos.y || (filter != null && filter.GetFactor(spawnPos) <= cutoff))
			{
				continue;
			}
			if (LayerMask.op_Implicit(placementCheckMask) != 0)
			{
				bool flag = false;
				if ((radius != 0f) ? Physics.SphereCast(val + Vector3.up * placementCheckHeight, radius, Vector3.down, ref val2, placementCheckHeight, LayerMask.op_Implicit(placementCheckMask)) : Physics.Raycast(val + Vector3.up * placementCheckHeight, Vector3.down, ref val2, placementCheckHeight, LayerMask.op_Implicit(placementCheckMask)))
				{
					if (((1 << ((Component)((RaycastHit)(ref val2)).transform).gameObject.layer) & LayerMask.op_Implicit(placementMask)) == 0)
					{
						continue;
					}
					val.y = ((RaycastHit)(ref val2)).point.y;
				}
			}
			if (LayerMask.op_Implicit(radiusCheckMask) == 0 || !Physics.CheckSphere(val, radiusCheckDistance, LayerMask.op_Implicit(radiusCheckMask)))
			{
				spawnPos.y = val.y;
				spawnRot = Quaternion.Euler(new Vector3(0f, Random.Range(0f, 360f), 0f));
				if (alignToNormal)
				{
					Vector3 normal = TerrainMeta.HeightMap.GetNormal(spawnPos);
					spawnRot = QuaternionEx.LookRotationForcedUp(spawnRot * Vector3.forward, normal);
				}
				return true;
			}
		}
		spawnPos = Vector3.zero;
		spawnRot = Quaternion.identity;
		return false;
	}

	public ByteQuadtree.Element SampleNode()
	{
		ByteQuadtree.Element result = quadtree.Root;
		while (!result.IsLeaf)
		{
			result = result.RandChild;
		}
		return result;
	}

	public void AddInstance(Spawnable spawnable)
	{
		UpdateCount(spawnable, 1);
	}

	public void RemoveInstance(Spawnable spawnable)
	{
		UpdateCount(spawnable, -1);
	}

	private void UpdateCount(Spawnable spawnable, int delta)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Count += delta;
		WorldSpaceGrid<int> val = grid;
		Vector3 spawnPosition = spawnable.SpawnPosition;
		val[spawnPosition] += delta;
		BaseEntity component = ((Component)spawnable).GetComponent<BaseEntity>();
		if (Object.op_Implicit((Object)(object)component))
		{
			if (dict.TryGetValue(component.prefabID, out var value))
			{
				dict[component.prefabID] = value + delta;
				return;
			}
			value = delta;
			dict.Add(component.prefabID, value);
		}
	}

	public int GetCount(uint prefabID)
	{
		dict.TryGetValue(prefabID, out var value);
		return value;
	}

	public int GetCount(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return grid[position];
	}

	public float GetGridCellArea()
	{
		return grid.CellArea;
	}
}

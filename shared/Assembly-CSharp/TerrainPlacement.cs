using System;
using UnityEngine;

public abstract class TerrainPlacement : PrefabAttribute
{
	[ReadOnly]
	public Vector3 size = Vector3.zero;

	[ReadOnly]
	public Vector3 extents = Vector3.zero;

	[ReadOnly]
	public Vector3 offset = Vector3.zero;

	public bool HeightMap = true;

	public bool AlphaMap = true;

	public bool WaterMap;

	[InspectorFlags]
	public Enum SplatMask;

	[InspectorFlags]
	public Enum BiomeMask;

	[InspectorFlags]
	public Enum TopologyMask;

	[HideInInspector]
	public Texture2DRef heightmap;

	[HideInInspector]
	public Texture2DRef splatmap0;

	[HideInInspector]
	public Texture2DRef splatmap1;

	[HideInInspector]
	public Texture2DRef alphamap;

	[HideInInspector]
	public Texture2DRef biomemap;

	[HideInInspector]
	public Texture2DRef topologymap;

	[HideInInspector]
	public Texture2DRef watermap;

	[HideInInspector]
	public Texture2DRef blendmap;

	public void Apply(Matrix4x4 localToWorld, Matrix4x4 worldToLocal)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (ShouldHeight())
		{
			ApplyHeight(localToWorld, worldToLocal);
		}
		if (ShouldSplat())
		{
			ApplySplat(localToWorld, worldToLocal);
		}
		if (ShouldAlpha())
		{
			ApplyAlpha(localToWorld, worldToLocal);
		}
		if (ShouldBiome())
		{
			ApplyBiome(localToWorld, worldToLocal);
		}
		if (ShouldTopology())
		{
			ApplyTopology(localToWorld, worldToLocal);
			GetTopologyCorners(localToWorld, out var v, out var v2, out var v3, out var v4);
			TerrainMeta.TopologyMap.RefreshHierarchy(v, v2, v3, v4);
		}
		if (ShouldWater())
		{
			ApplyWater(localToWorld, worldToLocal);
		}
	}

	protected bool ShouldHeight()
	{
		if (heightmap.isValid)
		{
			return HeightMap;
		}
		return false;
	}

	protected bool ShouldSplat(int id = -1)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		if (splatmap0.isValid && splatmap1.isValid)
		{
			return (SplatMask & id) > 0;
		}
		return false;
	}

	protected bool ShouldAlpha()
	{
		if (alphamap.isValid)
		{
			return AlphaMap;
		}
		return false;
	}

	protected bool ShouldBiome(int id = -1)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		if (biomemap.isValid)
		{
			return (BiomeMask & id) > 0;
		}
		return false;
	}

	protected bool ShouldTopology(int id = -1)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		if (topologymap.isValid)
		{
			return (TopologyMask & id) > 0;
		}
		return false;
	}

	protected bool ShouldWater()
	{
		if (watermap.isValid)
		{
			return WaterMap;
		}
		return false;
	}

	protected virtual void GetTopologyCorners(Matrix4x4 localToWorld, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		v1 = localToWorld.MultiplyPoint3x4(offset + new Vector3(0f - extents.x, 0f, 0f - extents.z));
		v2 = localToWorld.MultiplyPoint3x4(offset + new Vector3(extents.x, 0f, 0f - extents.z));
		v3 = localToWorld.MultiplyPoint3x4(offset + new Vector3(0f - extents.x, 0f, extents.z));
		v4 = localToWorld.MultiplyPoint3x4(offset + new Vector3(extents.x, 0f, extents.z));
	}

	protected abstract void ApplyHeight(Matrix4x4 localToWorld, Matrix4x4 worldToLocal);

	protected abstract void ApplySplat(Matrix4x4 localToWorld, Matrix4x4 worldToLocal);

	protected abstract void ApplyAlpha(Matrix4x4 localToWorld, Matrix4x4 worldToLocal);

	protected abstract void ApplyBiome(Matrix4x4 localToWorld, Matrix4x4 worldToLocal);

	protected abstract void ApplyTopology(Matrix4x4 localToWorld, Matrix4x4 worldToLocal);

	protected abstract void ApplyWater(Matrix4x4 localToWorld, Matrix4x4 worldToLocal);

	protected override Type GetIndexedType()
	{
		return typeof(TerrainPlacement);
	}

	protected TerrainPlacement()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
	}
}

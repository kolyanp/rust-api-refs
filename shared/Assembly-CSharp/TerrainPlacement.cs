using System;
using UnityEngine;

public abstract class TerrainPlacement : PrefabAttribute
{
	[ReadOnly]
	public Vector3 size;

	[ReadOnly]
	public Vector3 extents;

	[ReadOnly]
	public Vector3 offset;

	public bool HeightMap;

	public bool AlphaMap;

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
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
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
		size = Vector3.zero;
		extents = Vector3.zero;
		offset = Vector3.zero;
		HeightMap = true;
		AlphaMap = true;
		base._002Ector();
	}
}

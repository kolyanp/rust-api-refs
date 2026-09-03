using UnityEngine;

public class PlaceDecorWhiteNoise : ProceduralComponent
{
	public SpawnFilter Filter;

	public string ResourceFolder = string.Empty;

	public float ObjectDensity = 100f;

	public TerrainAnchorMode AnchorMode;

	[InspectorFlags]
	public SpawnFilterMode FilterMode = (SpawnFilterMode)(-1);

	public override void Process(uint seed)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		if (World.Networked)
		{
			World.Spawn("Decor", "assets/bundled/prefabs/autospawn/" + ResourceFolder + "/");
			return;
		}
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		Prefab[] array = Prefab.Load("assets/bundled/prefabs/autospawn/" + ResourceFolder);
		if (array == null || array.Length == 0)
		{
			return;
		}
		Vector3 position = TerrainMeta.Position;
		Vector3 size = TerrainMeta.Size;
		int num = Mathf.RoundToInt(ObjectDensity * size.x * size.z * 1E-06f);
		float x = position.x;
		float z = position.z;
		float num2 = position.x + size.x;
		float num3 = position.z + size.z;
		Vector3 pos = default(Vector3);
		for (int i = 0; i < num; i++)
		{
			float num4 = SeedRandom.Range(ref seed, x, num2);
			float num5 = SeedRandom.Range(ref seed, z, num3);
			float normX = TerrainMeta.NormalizeX(num4);
			float normZ = TerrainMeta.NormalizeZ(num5);
			float num6 = SeedRandom.Value(ref seed);
			Prefab random = ArrayEx.GetRandom(array, ref seed);
			if ((FilterMode & SpawnFilterMode.PivotPoint) != 0)
			{
				float factor = Filter.GetFactor(normX, normZ);
				if (factor * factor < num6)
				{
					continue;
				}
			}
			float height = heightMap.GetHeight(normX, normZ);
			((Vector3)(ref pos))._002Ector(num4, height, num5);
			Quaternion rot = random.Object.transform.localRotation;
			Vector3 scale = random.Object.transform.localScale;
			random.ApplyDecorComponents(ref pos, ref rot, ref scale);
			if (random.ApplyTerrainFilters(pos, rot, scale) && random.ApplyTerrainAnchors(ref pos, rot, scale, AnchorMode, ((FilterMode & SpawnFilterMode.TerrainAnchorPoints) != 0) ? Filter : null) && random.ApplyTerrainChecks(pos, rot, scale, ((FilterMode & SpawnFilterMode.TerrainCheckPoints) != 0) ? Filter : null) && random.ApplyWaterChecks(pos, rot, scale) && random.ApplyEnvironmentVolumeChecks(pos, rot, scale) && random.CheckTerrainFootprint(pos, rot, scale))
			{
				random.FillTerrainFootprint(pos, rot, scale);
				World.AddPrefab("Decor", random, pos, rot, scale);
			}
		}
	}
}

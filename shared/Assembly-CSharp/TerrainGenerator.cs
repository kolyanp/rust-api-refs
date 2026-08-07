using Oxide.Core;
using UnityEngine;

public class TerrainGenerator : SingletonComponent<TerrainGenerator>
{
	public TerrainConfig config;

	private const float HeightMapRes = 1f;

	private const float SplatMapRes = 0.5f;

	private const float BaseMapRes = 0.01f;

	public static int GetHeightMapRes()
	{
		return Mathf.Min(4096, Mathf.ClosestPowerOfTwo((int)((float)World.Size * 1f))) + 1;
	}

	public static int GetSplatMapRes()
	{
		return Mathf.Min(2048, Mathf.NextPowerOfTwo((int)((float)World.Size * 0.5f)));
	}

	public static int GetBaseMapRes()
	{
		return Mathf.Min(2048, Mathf.NextPowerOfTwo((int)((float)World.Size * 0.01f)));
	}

	public GameObject CreateTerrain()
	{
		return CreateTerrain(GetHeightMapRes(), GetSplatMapRes());
	}

	public GameObject CreateTerrain(int heightmapResolution, int alphamapResolution)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		Interface.CallHook("OnTerrainCreate", this);
		TerrainData val = new TerrainData();
		val.baseMapResolution = GetBaseMapRes();
		val.heightmapResolution = heightmapResolution;
		val.alphamapResolution = alphamapResolution;
		val.size = new Vector3((float)World.Size, 1000f, (float)World.Size);
		Terrain val2 = null;
		GeometryClipmapTerrain geometryClipmapTerrain = null;
		val2 = Terrain.CreateTerrainGameObject(val).GetComponent<Terrain>();
		((Component)val2).transform.position = ((Component)this).transform.position + new Vector3((float)(0L - (long)World.Size) * 0.5f, 0f, (float)(0L - (long)World.Size) * 0.5f);
		val2.drawInstanced = false;
		val2.castShadows = config.CastShadows;
		val2.materialType = (MaterialType)3;
		val2.materialTemplate = config.Material;
		((Component)val2).gameObject.tag = ((Component)this).gameObject.tag;
		((Component)val2).gameObject.layer = ((Component)this).gameObject.layer;
		((Collider)((Component)val2).gameObject.GetComponent<TerrainCollider>()).sharedMaterial = config.GenericMaterial;
		GameObject gameObject = ((Component)val2).gameObject;
		TerrainMeta terrainMeta = gameObject.AddComponent<TerrainMeta>();
		gameObject.AddComponent<TerrainPhysics>();
		gameObject.AddComponent<TerrainColors>();
		gameObject.AddComponent<TerrainCollision>();
		gameObject.AddComponent<TerrainBiomeMap>();
		gameObject.AddComponent<TerrainAlphaMap>();
		gameObject.AddComponent<TerrainHeightMap>();
		gameObject.AddComponent<TerrainSplatMap>();
		gameObject.AddComponent<TerrainTopologyMap>();
		gameObject.AddComponent<TerrainWaterMap>();
		gameObject.AddComponent<TerrainPlacementMap>();
		gameObject.AddComponent<TerrainPath>();
		gameObject.AddComponent<TerrainTexturing>();
		gameObject.AddComponent<TerrainWaterFlowMap>();
		gameObject.AddComponent<TerrainHoleRenderer>();
		if (Object.op_Implicit((Object)(object)val2))
		{
			terrainMeta.terrainRenderer.SetTerrain(val2);
		}
		else if (Object.op_Implicit((Object)(object)geometryClipmapTerrain))
		{
			terrainMeta.terrainRenderer.SetTerrain(geometryClipmapTerrain);
		}
		terrainMeta.terrainData = val;
		terrainMeta.config = config;
		Object.DestroyImmediate((Object)(object)((Component)this).gameObject);
		return gameObject;
	}
}

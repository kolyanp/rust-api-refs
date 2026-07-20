using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class TerrainRenderer
{
	public enum TerrainRendererBackend
	{
		Null,
		Unity,
		GeoClipmapping
	}

	public GameObject gameObject;

	public Terrain terrain;

	public GeometryClipmapTerrain geoClipTerrain;

	public TerrainRendererBackend rendererBackend;

	private bool isUnityTerrain => rendererBackend == TerrainRendererBackend.Unity;

	public bool HasTerrain
	{
		get
		{
			if (rendererBackend != TerrainRendererBackend.Unity || !Object.op_Implicit((Object)(object)terrain))
			{
				if (rendererBackend == TerrainRendererBackend.GeoClipmapping)
				{
					return Object.op_Implicit((Object)(object)geoClipTerrain);
				}
				return false;
			}
			return true;
		}
	}

	internal Material material
	{
		get
		{
			if (rendererBackend == TerrainRendererBackend.Unity && Object.op_Implicit((Object)(object)terrain))
			{
				return terrain.materialTemplate;
			}
			if (rendererBackend == TerrainRendererBackend.GeoClipmapping && Object.op_Implicit((Object)(object)geoClipTerrain))
			{
				return geoClipTerrain.terrainMaterial;
			}
			return null;
		}
	}

	public static implicit operator bool(TerrainRenderer a)
	{
		return a?.HasTerrain ?? false;
	}

	public void SetTerrain(GeometryClipmapTerrain t)
	{
		geoClipTerrain = t;
		gameObject = ((Component)t).gameObject;
		rendererBackend = TerrainRendererBackend.GeoClipmapping;
		((Behaviour)geoClipTerrain).enabled = true;
	}

	public void SetTerrain(Terrain t)
	{
		terrain = t;
		gameObject = ((Component)t).gameObject;
		rendererBackend = TerrainRendererBackend.Unity;
		((Behaviour)terrain).enabled = true;
	}

	public ReflectionProbeUsage GetReflectionProbeUsage()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		return (ReflectionProbeUsage)(rendererBackend switch
		{
			TerrainRendererBackend.Unity => terrain.reflectionProbeUsage, 
			TerrainRendererBackend.GeoClipmapping => geoClipTerrain.reflectionProbeUsage, 
			_ => 0, 
		});
	}

	public void SetReflectionProbeUsage(ReflectionProbeUsage value)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		switch (rendererBackend)
		{
		case TerrainRendererBackend.Unity:
			terrain.reflectionProbeUsage = value;
			break;
		case TerrainRendererBackend.GeoClipmapping:
			geoClipTerrain.reflectionProbeUsage = value;
			break;
		}
	}

	public void CheckForRenderer(GameObject go)
	{
		switch (rendererBackend)
		{
		case TerrainRendererBackend.Null:
			if (Object.op_Implicit((Object)(object)terrain) || go.TryGetComponent<Terrain>(ref terrain))
			{
				SetTerrain(terrain);
			}
			else if (Object.op_Implicit((Object)(object)geoClipTerrain) || go.TryGetComponent<GeometryClipmapTerrain>(ref geoClipTerrain))
			{
				SetTerrain(geoClipTerrain);
			}
			break;
		case TerrainRendererBackend.Unity:
			if (Object.op_Implicit((Object)(object)geoClipTerrain))
			{
				((Behaviour)geoClipTerrain).enabled = false;
			}
			if (!Object.op_Implicit((Object)(object)gameObject))
			{
				gameObject = go;
			}
			if (Object.op_Implicit((Object)(object)terrain) || go.TryGetComponent<Terrain>(ref terrain))
			{
				SetTerrain(terrain);
				break;
			}
			rendererBackend = TerrainRendererBackend.Null;
			gameObject = null;
			break;
		case TerrainRendererBackend.GeoClipmapping:
			if (Object.op_Implicit((Object)(object)terrain))
			{
				((Behaviour)terrain).enabled = false;
			}
			if (!Object.op_Implicit((Object)(object)gameObject))
			{
				gameObject = go;
			}
			if (Object.op_Implicit((Object)(object)geoClipTerrain) || go.TryGetComponent<GeometryClipmapTerrain>(ref geoClipTerrain))
			{
				SetTerrain(geoClipTerrain);
				break;
			}
			rendererBackend = TerrainRendererBackend.Null;
			gameObject = null;
			break;
		}
	}

	public void ValidateMaterial(TerrainConfig config)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		switch (rendererBackend)
		{
		case TerrainRendererBackend.Unity:
			if ((int)terrain.materialType == 0 && (Object)(object)terrain.materialTemplate != (Object)(object)config.Material)
			{
				terrain.materialType = (MaterialType)3;
				terrain.materialTemplate = config.Material;
			}
			break;
		case TerrainRendererBackend.GeoClipmapping:
			geoClipTerrain.terrainMaterial = config.GeoClipmapMaterial;
			break;
		}
	}

	public Vector3 GetPosition()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return gameObject.transform.position;
	}

	public void SwitchTerrain(TerrainRendererBackend rendererOption, TerrainData terrainData, TerrainConfig config, GameObject go)
	{
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		if (rendererOption == rendererBackend)
		{
			return;
		}
		switch (rendererOption)
		{
		case TerrainRendererBackend.Unity:
			gameObject = go;
			if (Object.op_Implicit((Object)(object)terrain) || go.TryGetComponent<Terrain>(ref terrain))
			{
				((Behaviour)terrain).enabled = true;
			}
			else
			{
				terrain = gameObject.AddComponent<Terrain>();
				terrain.drawInstanced = false;
				terrain.terrainData = terrainData;
				terrain.castShadows = config.CastShadows;
				terrain.materialType = (MaterialType)3;
				terrain.materialTemplate = config.Material;
			}
			if (Object.op_Implicit((Object)(object)geoClipTerrain))
			{
				((Behaviour)geoClipTerrain).enabled = false;
			}
			rendererBackend = rendererOption;
			break;
		case TerrainRendererBackend.GeoClipmapping:
			gameObject = go;
			if (Object.op_Implicit((Object)(object)geoClipTerrain) || go.TryGetComponent<GeometryClipmapTerrain>(ref geoClipTerrain))
			{
				((Behaviour)geoClipTerrain).enabled = true;
			}
			else
			{
				geoClipTerrain = gameObject.AddComponent<GeometryClipmapTerrain>();
				geoClipTerrain.terrainMaterial = config.GeoClipmapMaterial;
				geoClipTerrain.terrainShadows = (ShadowCastingMode)(config.CastShadows ? 1 : 0);
				geoClipTerrain.terrainData = terrainData;
				geoClipTerrain.terrainLayer = gameObject.layer;
				geoClipTerrain.terrainCompute = config.GeoClipCompute;
			}
			if (Object.op_Implicit((Object)(object)terrain))
			{
				((Behaviour)terrain).enabled = false;
			}
			rendererBackend = rendererOption;
			break;
		}
	}
}

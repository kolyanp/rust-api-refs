using UnityEngine;

public class TerrainPhysics : TerrainExtension
{
	private TerrainSplatMap splat;

	private PhysicsMaterial[] materials;

	public override void Setup()
	{
		splat = terrainRenderer.gameObject.GetComponent<TerrainSplatMap>();
		materials = config.GetPhysicMaterials();
	}

	public PhysicsMaterial GetMaterial(Vector3 worldPos)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)splat == (Object)null || materials.Length == 0)
		{
			return null;
		}
		return materials[splat.GetSplatMaxIndex(worldPos)];
	}
}

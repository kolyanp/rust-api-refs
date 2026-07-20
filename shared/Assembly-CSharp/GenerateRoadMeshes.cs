using UnityEngine;

public class GenerateRoadMeshes : ProceduralComponent
{
	public const float NormalSmoothing = 0f;

	public const bool SnapToTerrain = true;

	public Mesh RoadMesh;

	public Mesh[] RoadMeshes;

	public Material RoadMaterial;

	public Material RoadRingMaterial;

	public PhysicsMaterial RoadPhysicMaterial;

	public override bool RunOnCache => true;

	public override void Process(uint seed)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Expected O, but got Unknown
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		if (RoadMeshes == null || RoadMeshes.Length == 0)
		{
			RoadMeshes = (Mesh[])(object)new Mesh[1] { RoadMesh };
		}
		foreach (PathList road in TerrainMeta.Path.Roads)
		{
			if (road.Hierarchy >= 2)
			{
				continue;
			}
			GameObject val = new GameObject(road.Name);
			foreach (PathList.MeshObject item in road.CreateMesh(RoadMeshes, 0f, snapToTerrain: true, !road.Path.Circular, !road.Path.Circular))
			{
				GameObject val2 = new GameObject("Road Mesh");
				val2.transform.position = item.Position;
				val2.layer = 16;
				val2.tag = "IgnoreCollider";
				val2.transform.SetParent(val.transform, true);
				val2.SetActive(false);
				MeshCollider obj = val2.AddComponent<MeshCollider>();
				((Collider)obj).sharedMaterial = RoadPhysicMaterial;
				obj.sharedMesh = item.Meshes[0];
				TagComponentEx.SetCustomTag(val2, GameObjectTag.Road, apply: true);
				val2.AddComponent<AddToHeightMap>();
				val2.SetActive(true);
			}
		}
	}
}

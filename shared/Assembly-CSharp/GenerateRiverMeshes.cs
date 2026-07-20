using System.Collections.Generic;
using UnityEngine;

public class GenerateRiverMeshes : ProceduralComponent
{
	public const float NormalSmoothing = 0.1f;

	public const bool SnapToTerrain = false;

	public Mesh RiverMesh;

	public Mesh RiverInteriorMesh;

	public Mesh RiverInteriorFrontCapMesh;

	public Mesh RiverInteriorBackCapMesh;

	public Mesh[] RiverMeshes;

	public Material RiverMaterial;

	public PhysicsMaterial RiverPhysicMaterial;

	public override bool RunOnCache => true;

	public override void Process(uint seed)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		RiverMeshes = (Mesh[])(object)new Mesh[1] { RiverMesh };
		foreach (PathList river in TerrainMeta.Path.Rivers)
		{
			GameObject val = new GameObject(river.Name);
			List<PathList.MeshObject> list = river.CreateMesh(RiverMeshes, 0.1f, snapToTerrain: false, !river.Path.Circular, !river.Path.Circular, scaleWidthWithLength: true, topAligned: false, 4);
			for (int i = 0; i < list.Count; i++)
			{
				PathList.MeshObject meshObject = list[i];
				GameObject val2 = new GameObject("River Mesh");
				val2.transform.position = meshObject.Position;
				val2.tag = "River";
				val2.layer = 4;
				val2.transform.SetParent(val.transform, true);
				val2.SetActive(false);
				MeshCollider obj = val2.AddComponent<MeshCollider>();
				((Collider)obj).sharedMaterial = RiverPhysicMaterial;
				obj.sharedMesh = meshObject.Meshes[0];
				val2.AddComponent<RiverInfo>();
				WaterBody waterBody = val2.AddComponent<WaterBody>();
				waterBody.Type = WaterBodyType.River;
				waterBody.FishingType = WaterBody.FishingTag.River;
				val2.AddComponent<AddToWaterMap>();
				val2.SetActive(true);
			}
		}
	}
}

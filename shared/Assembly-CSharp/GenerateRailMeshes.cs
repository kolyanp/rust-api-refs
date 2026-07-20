using UnityEngine;

public class GenerateRailMeshes : ProceduralComponent
{
	public const float NormalSmoothing = 0f;

	public const bool SnapToTerrain = false;

	public Mesh RailMesh;

	public Mesh[] RailMeshes;

	public Material RailMaterial;

	public PhysicsMaterial RailPhysicMaterial;

	public override bool RunOnCache => true;

	public override void Process(uint seed)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		if (RailMeshes == null || RailMeshes.Length == 0)
		{
			RailMeshes = (Mesh[])(object)new Mesh[1] { RailMesh };
		}
		foreach (PathList rail in TerrainMeta.Path.Rails)
		{
			GameObject val = new GameObject(rail.Name);
			foreach (PathMeshTemplate item in rail.CreateMeshTemplates(RailMeshes, 0f, snapToTerrain: false, !rail.Path.Circular && !rail.Start, !rail.Path.Circular && !rail.End))
			{
				GameObject val2 = new GameObject("Rail Mesh");
				val2.transform.position = item.Position;
				val2.tag = "Railway";
				TagComponentEx.SetCustomTag(val2, GameObjectTag.AllowBarricadePlacement, apply: true);
				val2.layer = 16;
				val2.transform.SetParent(val.transform, true);
				val2.SetActive(false);
				MeshCollider obj = val2.AddComponent<MeshCollider>();
				((Collider)obj).sharedMaterial = RailPhysicMaterial;
				item.Generate();
				obj.sharedMesh = item.outputMeshes[0];
				val2.AddComponent<AddToHeightMap>();
				val2.SetActive(true);
			}
			AddTrackSpline(val, rail);
		}
	}

	private void AddTrackSpline(GameObject rootGO, PathList rail)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		TrainTrackSpline trainTrackSpline = rootGO.AddComponent<TrainTrackSpline>();
		trainTrackSpline.aboveGroundSpawn = rail.Hierarchy == 2;
		trainTrackSpline.hierarchy = rail.Hierarchy;
		if (trainTrackSpline.aboveGroundSpawn)
		{
			TrainTrackSpline.SidingSplines.Add(trainTrackSpline);
		}
		Vector3[] array = (Vector3[])(object)new Vector3[rail.Path.Points.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = rail.Path.Points[i];
			array[i].y += 0.41f;
		}
		Vector3[] array2 = (Vector3[])(object)new Vector3[rail.Path.Tangents.Length];
		for (int j = 0; j < array.Length; j++)
		{
			array2[j] = rail.Path.Tangents[j];
		}
		trainTrackSpline.SetAll(array, array2, 0.25f);
	}
}

using UnityEngine;

namespace VLB;

public static class GlobalMesh
{
	private static Mesh ms_Mesh;

	private static Bounds ms_MeshBounds;

	public static Mesh mesh
	{
		get
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)ms_Mesh == (Object)null)
			{
				ms_Mesh = MeshGenerator.GenerateConeZ_Radius(1f, 1f, 1f, Config.Instance.sharedMeshSides, Config.Instance.sharedMeshSegments, cap: true);
				((Object)ms_Mesh).hideFlags = Consts.ProceduralObjectsHideFlags;
				ms_MeshBounds = ms_Mesh.bounds;
			}
			return ms_Mesh;
		}
	}

	public static Bounds MeshBounds
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return ms_MeshBounds;
		}
	}

	public static void Destroy()
	{
		if ((Object)(object)ms_Mesh != (Object)null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)ms_Mesh);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)ms_Mesh);
			}
			ms_Mesh = null;
		}
	}
}

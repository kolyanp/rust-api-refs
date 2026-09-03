using UnityEngine;

public class WorldModel : MonoBehaviour
{
	public float mass = 1f;

	public int GetTriCount(int lod = 0)
	{
		LODGroup componentInChildren = ((Component)this).GetComponentInChildren<LODGroup>(true);
		if ((Object)(object)componentInChildren == (Object)null)
		{
			return 0;
		}
		LOD[] lODs = componentInChildren.GetLODs();
		if (lODs.Length == 0)
		{
			return 0;
		}
		int num = 0;
		Renderer[] renderers = lODs[lod].renderers;
		foreach (Renderer val in renderers)
		{
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			MeshRenderer val2 = (MeshRenderer)(object)((val is MeshRenderer) ? val : null);
			if (val2 != null)
			{
				MeshFilter component = ((Component)val2).GetComponent<MeshFilter>();
				if ((Object)(object)component != (Object)null && (Object)(object)component.sharedMesh != (Object)null)
				{
					num += GetMeshTriangleCount(component.sharedMesh);
				}
			}
			else
			{
				SkinnedMeshRenderer val3 = (SkinnedMeshRenderer)(object)((val is SkinnedMeshRenderer) ? val : null);
				if (val3 != null && (Object)(object)val3.sharedMesh != (Object)null)
				{
					num += GetMeshTriangleCount(val3.sharedMesh);
				}
			}
		}
		return num;
	}

	private static int GetMeshTriangleCount(Mesh mesh)
	{
		int num = 0;
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			num += (int)mesh.GetIndexCount(i) / 3;
		}
		return num;
	}
}

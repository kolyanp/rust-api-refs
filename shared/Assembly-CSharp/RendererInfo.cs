using UnityEngine;
using UnityEngine.Rendering;

public class RendererInfo : ComponentInfo<Renderer>
{
	public ShadowCastingMode shadows;

	public Material material;

	public Mesh mesh;

	public MeshFilter meshFilter;

	public override void Reset()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		component.shadowCastingMode = shadows;
		if (Object.op_Implicit((Object)(object)material))
		{
			component.sharedMaterial = material;
		}
		Renderer val = component;
		SkinnedMeshRenderer val2 = (SkinnedMeshRenderer)(object)((val is SkinnedMeshRenderer) ? val : null);
		if (val2 != null)
		{
			val2.sharedMesh = mesh;
		}
		else if (component is MeshRenderer)
		{
			meshFilter.sharedMesh = mesh;
		}
	}

	public override void Setup()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		shadows = component.shadowCastingMode;
		material = component.sharedMaterial;
		Renderer val = component;
		SkinnedMeshRenderer val2 = (SkinnedMeshRenderer)(object)((val is SkinnedMeshRenderer) ? val : null);
		if (val2 != null)
		{
			mesh = val2.sharedMesh;
		}
		else if (component is MeshRenderer)
		{
			meshFilter = ((Component)this).GetComponent<MeshFilter>();
			mesh = meshFilter.sharedMesh;
		}
	}
}

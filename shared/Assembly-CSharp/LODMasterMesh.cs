using UnityEngine;

public class LODMasterMesh : LODComponent
{
	public MeshRenderer ReplacementMesh;

	public float Distance = 100f;

	[FlattenIgnoreReference]
	public LODComponent[] ChildComponents;

	public bool Block;

	public Bounds MeshBounds;
}

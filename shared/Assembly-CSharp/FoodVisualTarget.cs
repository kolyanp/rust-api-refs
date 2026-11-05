using UnityEngine;

public class FoodVisualTarget : MonoBehaviour, IClientComponent
{
	public MeshFilter TopperFilter;

	public MeshRenderer TopperRenderer;

	public MeshLOD TopperLod;

	public bool ApplyTopperMaterial = true;

	public Renderer[] TintRenderers;

	public GameObject[] VisualRoots;
}

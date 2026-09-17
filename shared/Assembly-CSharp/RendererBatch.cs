using Rust.Workshop;
using UnityEngine;

public class RendererBatch : MonoBehaviour, IClientComponent, ICustomMaterialReplacer, IWorkshopPreview
{
	[HideInInspector]
	[SerializeField]
	public int MaxVertexCountOverride;

	[SerializeField]
	public bool AllowSubmeshes;
}

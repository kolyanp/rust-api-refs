using ConVar;
using Rust.Workshop;
using UnityEngine;

public class RendererBatch : MonoBehaviour, IClientComponent, ICustomMaterialReplacer, IWorkshopPreview
{
	[HideInInspector]
	[SerializeField]
	public int MaxVertexCountOverride;

	[SerializeField]
	public bool AllowSubmeshes;

	public int MaxVertexCount
	{
		get
		{
			if (MaxVertexCountOverride <= 0)
			{
				return Batching.renderer_vertices;
			}
			return MaxVertexCountOverride;
		}
	}
}

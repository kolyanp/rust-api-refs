using System;
using UnityEngine;

public class BoxColliderBake : MonoBehaviour, IEditorComponent
{
	[Tooltip("Model whose box meshes are baked into colliders on this object")]
	public GameObject Source;

	public BoxCollider[] Colliders = Array.Empty<BoxCollider>();

	public GameObject[] Children = Array.Empty<GameObject>();
}

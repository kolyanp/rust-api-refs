namespace UnityEngine;

[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class InstancedDebugDraw : SingletonComponent<InstancedDebugDraw>
{
	public Material overlayMaterial;

	public Material depthTestedMaterial;
}

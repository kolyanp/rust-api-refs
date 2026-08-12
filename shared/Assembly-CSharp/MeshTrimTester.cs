using UnityEngine;

[ExecuteInEditMode]
public class MeshTrimTester : MonoBehaviour
{
	public MeshTrimSettings Settings;

	public Mesh SourceMesh;

	public MeshFilter TargetMeshFilter;

	public int SubtractIndex;

	public MeshTrimTester()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Settings = MeshTrimSettings.Default;
		((MonoBehaviour)this)._002Ector();
	}
}

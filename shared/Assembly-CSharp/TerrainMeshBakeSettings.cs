using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Terrain Mesh Bake Settings", fileName = "TerrainMeshBakeSettings.asset")]
public class TerrainMeshBakeSettings : ScriptableObject
{
	public int chunkCount = 6;

	public int[] lodVertexCounts = new int[3] { 64, 32, 16 };

	public int colliderVertexCount = 384;

	[Range(0.01f, 1f)]
	public float colliderQuality = 0.25f;

	public AnimationCurve lodDistanceCurve = new AnimationCurve(new Keyframe[3]
	{
		new Keyframe(0f, 100f),
		new Keyframe(1f, 300f),
		new Keyframe(2f, 600f)
	});

	public string outputFolderName = "MeshLODs";

	public Material terrainMaterialTemplate;

	public Shader terrainShader;

	public TerrainMeshBakeSettings()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected Obj, but got Unknown
	}
}

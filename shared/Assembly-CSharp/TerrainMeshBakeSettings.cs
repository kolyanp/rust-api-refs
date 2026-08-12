using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Terrain Mesh Bake Settings", fileName = "TerrainMeshBakeSettings.asset")]
public class TerrainMeshBakeSettings : ScriptableObject
{
	public int chunkCount;

	public int[] lodVertexCounts;

	public int colliderVertexCount;

	[Range(0.01f, 1f)]
	public float colliderQuality;

	public AnimationCurve lodDistanceCurve;

	public string outputFolderName;

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
		//IL_0087: Expected O, but got Unknown
		chunkCount = 6;
		lodVertexCounts = new int[3] { 64, 32, 16 };
		colliderVertexCount = 384;
		colliderQuality = 0.25f;
		lodDistanceCurve = new AnimationCurve((Keyframe[])(object)new Keyframe[3]
		{
			new Keyframe(0f, 100f),
			new Keyframe(1f, 300f),
			new Keyframe(2f, 600f)
		});
		outputFolderName = "MeshLODs";
		((ScriptableObject)this)._002Ector();
	}
}

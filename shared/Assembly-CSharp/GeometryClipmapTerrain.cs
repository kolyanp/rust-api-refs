using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class GeometryClipmapTerrain : MonoBehaviour, IClientComponent
{
	protected enum TerrainTopology
	{
		TriangleFan,
		UnityQuads
	}

	public const int LOD_COUNT = 6;

	public const int LOD_MAX = 5;

	public const int MESH_COUNT = 3;

	[SerializeField]
	protected TerrainTopology terrainTopology;

	[SerializeField]
	public ComputeShader terrainCompute;

	[SerializeField]
	public Material terrainMaterial;

	[SerializeField]
	[Range(1f, 8f)]
	public int minVertLOD;

	[SerializeField]
	[Range(1f, 10f)]
	protected int lodGlobalScale;

	[SerializeField]
	public float renderDistance;

	[SerializeField]
	public LayerSelect terrainLayer;

	[SerializeField]
	public ShadowCastingMode terrainShadows;

	public ReflectionProbeUsage reflectionProbeUsage;

	public bool isDepthPrepassEnabled;

	public Mesh terrainCellMaster;

	public bool debugValidate;

	public bool allowEditorCamLOD;

	public float cellSize;

	[Range(0f, 4f)]
	[SerializeField]
	protected int colliderVertexReduction;

	[Range(0f, 4f)]
	[SerializeField]
	protected int vertexDensity;

	[Range(0f, 4f)]
	[SerializeField]
	protected int vertexDensityReduction;

	public bool debugCullingOn;

	public bool debugTestDeform;

	public bool debugTestApply;

	public float deformRadius;

	public float deformFade;

	public float deformDelta;

	public bool testColliderOneObject;

	public TerrainData terrainData;

	private const string OUTPUT_DEPTH_PREPASS_KEYWORD = "OUTPUT_DEPTH_PREPASS";

	[SerializeField]
	private int[] lodCellExtents;

	public GeometryClipmapTerrain()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		terrainTopology = TerrainTopology.UnityQuads;
		minVertLOD = 4;
		lodGlobalScale = 2;
		renderDistance = 2500f;
		terrainLayer = 23;
		terrainShadows = (ShadowCastingMode)1;
		isDepthPrepassEnabled = true;
		allowEditorCamLOD = true;
		debugCullingOn = true;
		deformRadius = 5f;
		deformFade = 1f;
		deformDelta = 1f;
		lodCellExtents = new int[6];
		((MonoBehaviour)this)._002Ector();
	}
}

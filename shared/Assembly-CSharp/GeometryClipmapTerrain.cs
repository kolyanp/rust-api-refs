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
	protected TerrainTopology terrainTopology = TerrainTopology.UnityQuads;

	[SerializeField]
	public ComputeShader terrainCompute;

	[SerializeField]
	public Material terrainMaterial;

	[SerializeField]
	[Range(1f, 8f)]
	public int minVertLOD = 4;

	[SerializeField]
	[Range(1f, 10f)]
	protected int lodGlobalScale = 2;

	[SerializeField]
	public float renderDistance = 2500f;

	[SerializeField]
	public LayerSelect terrainLayer = 23;

	[SerializeField]
	public ShadowCastingMode terrainShadows = (ShadowCastingMode)1;

	public ReflectionProbeUsage reflectionProbeUsage;

	public Mesh terrainCellMaster;

	public bool debugValidate;

	public bool allowEditorCamLOD = true;

	public float cellSize;

	[Range(0f, 4f)]
	[SerializeField]
	protected int colliderVertexReduction;

	[Range(0f, 4f)]
	[SerializeField]
	protected int vertexDensity;

	[SerializeField]
	[Range(0f, 4f)]
	protected int vertexDensityReduction;

	public bool debugCullingOn = true;

	public bool debugTestDeform;

	public bool debugTestApply;

	public float deformRadius = 5f;

	public float deformFade = 1f;

	public float deformDelta = 1f;

	public bool testColliderOneObject;

	public TerrainData terrainData;

	[SerializeField]
	private int[] lodCellExtents = new int[6];
}

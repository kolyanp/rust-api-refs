using UnityEngine;

public class TerrainCopyPaste : MonoBehaviour, IEditorComponent
{
	public Vector3 Size;

	public bool CopyHeightMap;

	public bool CopySplatMap;

	public bool CopyBiomeMap;

	public bool CopyAlphaMap;

	public bool CopyTopologyMap;

	public bool CopyWaterMap;

	[SerializeField]
	[HideInInspector]
	private bool _hasCopied;

	[SerializeField]
	[HideInInspector]
	private bool _isUndo;

	[SerializeField]
	[HideInInspector]
	private Vector3 _copySize;

	[HideInInspector]
	[SerializeField]
	private RectInt _heightMapRect;

	[HideInInspector]
	[SerializeField]
	private Color[] _heightMapData;

	[HideInInspector]
	[SerializeField]
	private RectInt _splat0Rect;

	[HideInInspector]
	[SerializeField]
	private Color[] _splat0Data;

	[HideInInspector]
	[SerializeField]
	private RectInt _splat1Rect;

	[HideInInspector]
	[SerializeField]
	private Color[] _splat1Data;

	[SerializeField]
	[HideInInspector]
	private RectInt _biomeRect;

	[SerializeField]
	[HideInInspector]
	private Color[] _biomeData;

	[HideInInspector]
	[SerializeField]
	private RectInt _alphaRect;

	[SerializeField]
	[HideInInspector]
	private Color[] _alphaData;

	[SerializeField]
	[HideInInspector]
	private RectInt _topologyRect;

	[SerializeField]
	[HideInInspector]
	private Color[] _topologyData;

	[SerializeField]
	[HideInInspector]
	private RectInt _waterRect;

	[SerializeField]
	[HideInInspector]
	private Color[] _waterData;

	public bool HasCopied => _hasCopied;

	public bool IsUndo => _isUndo;

	public TerrainCopyPaste()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Size = new Vector3(100f, 10f, 100f);
		CopyHeightMap = true;
		CopySplatMap = true;
		CopyBiomeMap = true;
		CopyAlphaMap = true;
		CopyTopologyMap = true;
		CopyWaterMap = true;
		((MonoBehaviour)this)._002Ector();
	}
}

using UnityEngine;

public class HLODBounds : MonoBehaviour, IEditorComponent
{
	[Tooltip("The bounds that this HLOD will cover. This should not overlap with any other HLODs")]
	public Bounds MeshBounds;

	[Tooltip("Assets created will use this prefix. Make sure multiple HLODS in a scene have different prefixes")]
	public string MeshPrefix;

	[Tooltip("The point from which to calculate the HLOD. Any RendererLODs that are visible at this distance will baked into the HLOD mesh")]
	public float CullDistance;

	[Tooltip("If set, the lod will take over at this distance instead of the CullDistance (eg. we make a model based on what this area looks like at 200m but we actually want it take over rendering at 300m)")]
	public float OverrideLodDistance;

	[Tooltip("Any renderers below this height will considered culled even if they are visible from a distance. Good for underground areas")]
	public float CullBelowHeight;

	[Tooltip("Optimises the mesh produced by removing non-visible and small faces. Can turn it off during dev but should be on for final builds")]
	public bool ApplyMeshTrimming;

	public MeshTrimSettings Settings;

	public LODComponent DebugComponent;

	public bool ShowTrimSettings;

	[Tooltip("Prints out information about what the baker is doing, helpful for diagnosing errors")]
	public bool DebugMode;

	public HLODBounds()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		MeshBounds = new Bounds(Vector3.zero, new Vector3(50f, 25f, 50f));
		MeshPrefix = "root";
		CullDistance = 100f;
		ApplyMeshTrimming = true;
		Settings = MeshTrimSettings.Default;
		((MonoBehaviour)this)._002Ector();
	}
}

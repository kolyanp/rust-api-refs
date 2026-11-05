using Development.Attributes;
using UnityEngine;

[ResetStaticFields]
public class LightLOD : MonoBehaviour, ILOD, IClientComponent
{
	public float DistanceBias;

	public bool ToggleLight;

	public bool ToggleShadows = true;

	[Tooltip("Never cast shadows further than this from the camera, 0 = no limit")]
	public float MaxShadowDistance;

	[SerializeField]
	private ShadowCacher shadowCacher = new ShadowCacher();

	protected void OnValidate()
	{
		LightEx.CheckConflict(((Component)this).gameObject);
	}
}

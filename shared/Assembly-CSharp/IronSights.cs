using UnityEngine;

public class IronSights : MonoBehaviour
{
	[Header("View Setup")]
	public IronsightAimPoint aimPoint;

	public float fieldOfViewOffset = -20f;

	public float zoomFactor = 1f;

	[Header("Animation")]
	public float introSpeed = 1f;

	public AnimationCurve introCurve = new AnimationCurve();

	public float outroSpeed = 1f;

	public AnimationCurve outroCurve = new AnimationCurve();

	[Tooltip("Force the ironsight rotation every frame, don't lerp to the rotation. Can be useful if the ADS is animated and this component is conflicting")]
	public bool disableLerps;

	[Header("Sounds")]
	public SoundDefinition upSound;

	public SoundDefinition downSound;

	[Header("Info")]
	public IronSightOverride ironsightsOverride;

	public bool processUltrawideOffset;

	public IronSights()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected Obj, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected Obj, but got Unknown
	}
}

using UnityEngine;

public class IronSights : MonoBehaviour
{
	[Header("View Setup")]
	public IronsightAimPoint aimPoint;

	public float fieldOfViewOffset;

	public float zoomFactor;

	[Header("Animation")]
	public float introSpeed;

	public AnimationCurve introCurve;

	public float outroSpeed;

	public AnimationCurve outroCurve;

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
		//IL_002c: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		fieldOfViewOffset = -20f;
		zoomFactor = 1f;
		introSpeed = 1f;
		introCurve = new AnimationCurve();
		outroSpeed = 1f;
		outroCurve = new AnimationCurve();
		((MonoBehaviour)this)._002Ector();
	}
}

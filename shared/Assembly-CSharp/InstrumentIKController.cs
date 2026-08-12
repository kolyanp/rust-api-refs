using UnityEngine;

public class InstrumentIKController : MonoBehaviour
{
	public Vector3 HitRotationVector;

	public Transform[] LeftHandIkTargets;

	public Transform[] LeftHandIKTargetHitRotations;

	public Transform[] RightHandIkTargets;

	public Transform[] RightHandIKTargetHitRotations;

	public Transform[] RightFootIkTargets;

	public Transform LeftFootIkTarget;

	public AnimationCurve HandHeightCurve;

	public float HandHeightMultiplier;

	public float HandMoveLerpSpeed;

	public bool DebugHitRotation;

	public AnimationCurve HandHitCurve;

	public float NoteHitTime;

	[Header("Look IK")]
	public float BodyLookWeight;

	public float HeadLookWeight;

	public float LookWeightLimit;

	public bool HoldHandsAtPlay;

	public InstrumentIKController()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		HitRotationVector = Vector3.forward;
		LeftHandIkTargets = (Transform[])(object)new Transform[0];
		LeftHandIKTargetHitRotations = (Transform[])(object)new Transform[0];
		RightHandIkTargets = (Transform[])(object)new Transform[0];
		RightHandIKTargetHitRotations = (Transform[])(object)new Transform[0];
		RightFootIkTargets = (Transform[])(object)new Transform[0];
		HandHeightCurve = AnimationCurve.Constant(0f, 1f, 0f);
		HandHeightMultiplier = 1f;
		HandMoveLerpSpeed = 50f;
		HandHitCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		NoteHitTime = 0.5f;
		((MonoBehaviour)this)._002Ector();
	}
}

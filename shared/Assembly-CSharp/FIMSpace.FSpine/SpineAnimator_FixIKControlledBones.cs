using UnityEngine;

namespace FIMSpace.FSpine;

[DefaultExecutionOrder(-12)]
[AddComponentMenu("FImpossible Creations/Spine Animator Utilities/Spine Animator IK Controlled Bone Fixer")]
public class SpineAnimator_FixIKControlledBones : MonoBehaviour
{
	public Transform SkeletonParentBone;

	[Tooltip("If bones are twisting with this option off you should turn it on (calibrating bone if it's animation don't use keyframes)")]
	public bool Calibrate = true;

	private Quaternion initLocalRot;

	private Vector3 initLocalPos;

	private Quaternion localRotation;

	private Vector3 localPosition;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		initLocalPos = ((Component)this).transform.localPosition;
		initLocalRot = ((Component)this).transform.localRotation;
	}

	public void Calibration()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (Calibrate)
		{
			((Component)this).transform.localPosition = initLocalPos;
			((Component)this).transform.localRotation = initLocalRot;
		}
	}

	public void UpdateOnAnimator()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled)
		{
			localRotation = FEngineering.QToLocal(SkeletonParentBone.rotation, ((Component)this).transform.rotation);
			localPosition = SkeletonParentBone.InverseTransformPoint(((Component)this).transform.position);
		}
	}

	public void UpdateAfterProcedural()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled)
		{
			((Component)this).transform.rotation = FEngineering.QToWorld(SkeletonParentBone.rotation, localRotation);
			((Component)this).transform.position = SkeletonParentBone.TransformPoint(localPosition);
		}
	}
}

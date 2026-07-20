using System;
using UnityEngine;

namespace FIMSpace.FTools;

[Serializable]
public class FIK_CCDProcessor : FIK_ProcessorBase
{
	[Serializable]
	public class CCDIKBone : FIK_IKBoneBase
	{
		[Range(0f, 180f)]
		public float AngleLimit = 45f;

		[Range(0f, 180f)]
		public float TwistAngleLimit = 5f;

		public Vector3 ForwardOrientation;

		public float FrameWorldLength = 1f;

		public Vector2 HingeLimits = Vector2.zero;

		public Quaternion PreviousHingeRotation;

		public float PreviousHingeAngle;

		public Vector3 LastIKLocPosition;

		public Quaternion LastIKLocRotation;

		public CCDIKBone IKParent { get; private set; }

		public CCDIKBone IKChild { get; private set; }

		public CCDIKBone(Transform t)
			: base(t)
		{
		}//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)


		public void Init(CCDIKBone child, CCDIKBone parent)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			LastIKLocPosition = base.transform.localPosition;
			IKParent = parent;
			if (child != null)
			{
				SetChild(child);
			}
			IKChild = child;
		}

		public override void SetChild(FIK_IKBoneBase child)
		{
			base.SetChild(child);
		}

		public void AngleLimiting()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			Quaternion val = Quaternion.Inverse(LastKeyLocalRotation) * base.transform.localRotation;
			Quaternion val2 = val;
			if (FEngineering.VIsZero(Vector2.op_Implicit(HingeLimits)))
			{
				if (AngleLimit < 180f)
				{
					val2 = LimitSpherical(val2);
				}
				if (TwistAngleLimit < 180f)
				{
					val2 = LimitZ(val2);
				}
			}
			else
			{
				val2 = LimitHinge(val2);
			}
			if (!FEngineering.QIsSame(val2, val))
			{
				base.transform.localRotation = LastKeyLocalRotation * val2;
			}
		}

		private Quaternion LimitSpherical(Quaternion rotation)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			if (FEngineering.QIsZero(rotation))
			{
				return rotation;
			}
			Vector3 val = rotation * ForwardOrientation;
			Quaternion val2 = Quaternion.RotateTowards(Quaternion.identity, Quaternion.FromToRotation(ForwardOrientation, val), AngleLimit);
			return Quaternion.FromToRotation(val, val2 * ForwardOrientation) * rotation;
		}

		private Quaternion LimitZ(Quaternion currentRotation)
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(ForwardOrientation.y, ForwardOrientation.z, ForwardOrientation.x);
			Vector3 val2 = currentRotation * ForwardOrientation;
			Vector3 val3 = val;
			Vector3.OrthoNormalize(ref val2, ref val3);
			val = currentRotation * val;
			Vector3.OrthoNormalize(ref val2, ref val);
			Quaternion val4 = Quaternion.FromToRotation(val, val3) * currentRotation;
			if (TwistAngleLimit <= 0f)
			{
				return val4;
			}
			return Quaternion.RotateTowards(val4, currentRotation, TwistAngleLimit);
		}

		private Quaternion LimitHinge(Quaternion rotation)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			Quaternion val = Quaternion.FromToRotation(rotation * ForwardOrientation, ForwardOrientation) * rotation * Quaternion.Inverse(PreviousHingeRotation);
			float num = Quaternion.Angle(Quaternion.identity, val);
			Vector3 val2 = default(Vector3);
			((Vector3)(ref val2))._002Ector(ForwardOrientation.z, ForwardOrientation.x, ForwardOrientation.y);
			Vector3 val3 = Vector3.Cross(val2, ForwardOrientation);
			if (Vector3.Dot(val * val2, val3) > 0f)
			{
				num = 0f - num;
			}
			PreviousHingeAngle = Mathf.Clamp(PreviousHingeAngle + num, HingeLimits.x, HingeLimits.y);
			PreviousHingeRotation = Quaternion.AngleAxis(PreviousHingeAngle, ForwardOrientation);
			return PreviousHingeRotation;
		}
	}

	public CCDIKBone[] IKBones;

	public bool ContinousSolving = true;

	[Range(0f, 1f)]
	public float SyncWithAnimator = 1f;

	[Range(1f, 12f)]
	public int ReactionQuality = 2;

	[Range(0f, 1f)]
	public float Smoothing;

	[Range(0f, 1.5f)]
	public float StretchToTarget;

	public AnimationCurve StretchCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public bool Use2D;

	public bool Invert;

	public CCDIKBone StartIKBone => IKBones[0];

	public CCDIKBone EndIKBone => IKBones[IKBones.Length - 1];

	public float ActiveLength { get; private set; }

	public FIK_CCDProcessor(Transform[] bonesChain)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		IKBones = new CCDIKBone[bonesChain.Length];
		FIK_IKBoneBase[] bones = new CCDIKBone[IKBones.Length];
		base.Bones = bones;
		for (int i = 0; i < bonesChain.Length; i++)
		{
			IKBones[i] = new CCDIKBone(bonesChain[i]);
			base.Bones[i] = IKBones[i];
		}
		IKTargetPosition = base.EndBone.transform.position;
		IKTargetRotation = base.EndBone.transform.rotation;
	}

	public override void Init(Transform root)
	{
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		if (base.Initialized)
		{
			return;
		}
		base.fullLength = 0f;
		for (int i = 0; i < base.Bones.Length; i++)
		{
			CCDIKBone cCDIKBone = IKBones[i];
			CCDIKBone child = null;
			CCDIKBone parent = null;
			if (i > 0)
			{
				parent = IKBones[i - 1];
			}
			if (i < base.Bones.Length - 1)
			{
				child = IKBones[i + 1];
			}
			if (i < base.Bones.Length - 1)
			{
				IKBones[i].Init(child, parent);
				base.fullLength += cCDIKBone.BoneLength;
				cCDIKBone.ForwardOrientation = Quaternion.Inverse(cCDIKBone.transform.rotation) * (IKBones[i + 1].transform.position - cCDIKBone.transform.position);
			}
			else
			{
				IKBones[i].Init(child, parent);
				cCDIKBone.ForwardOrientation = Quaternion.Inverse(cCDIKBone.transform.rotation) * (IKBones[IKBones.Length - 1].transform.position - IKBones[0].transform.position);
			}
		}
		base.Initialized = true;
	}

	public override void Update()
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_065a: Unknown result type (might be due to invalid IL or missing references)
		//IL_065f: Unknown result type (might be due to invalid IL or missing references)
		//IL_066b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0670: Unknown result type (might be due to invalid IL or missing references)
		//IL_0676: Unknown result type (might be due to invalid IL or missing references)
		//IL_067c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0681: Unknown result type (might be due to invalid IL or missing references)
		//IL_0686: Unknown result type (might be due to invalid IL or missing references)
		//IL_068b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0694: Unknown result type (might be due to invalid IL or missing references)
		//IL_0699: Unknown result type (might be due to invalid IL or missing references)
		//IL_069c: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0533: Unknown result type (might be due to invalid IL or missing references)
		//IL_0538: Unknown result type (might be due to invalid IL or missing references)
		//IL_053c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0541: Unknown result type (might be due to invalid IL or missing references)
		//IL_0549: Unknown result type (might be due to invalid IL or missing references)
		//IL_054e: Unknown result type (might be due to invalid IL or missing references)
		//IL_055b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0560: Unknown result type (might be due to invalid IL or missing references)
		//IL_0562: Unknown result type (might be due to invalid IL or missing references)
		//IL_0564: Unknown result type (might be due to invalid IL or missing references)
		//IL_0566: Unknown result type (might be due to invalid IL or missing references)
		//IL_056b: Unknown result type (might be due to invalid IL or missing references)
		//IL_056f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0574: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_0578: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0606: Unknown result type (might be due to invalid IL or missing references)
		//IL_060b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0610: Unknown result type (might be due to invalid IL or missing references)
		//IL_0615: Unknown result type (might be due to invalid IL or missing references)
		//IL_0621: Unknown result type (might be due to invalid IL or missing references)
		//IL_0623: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_0428: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_046a: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Initialized || IKWeight <= 0f)
		{
			return;
		}
		CCDIKBone cCDIKBone = IKBones[0];
		if (ContinousSolving)
		{
			while (cCDIKBone != null)
			{
				cCDIKBone.LastKeyLocalRotation = cCDIKBone.transform.localRotation;
				cCDIKBone.transform.localPosition = cCDIKBone.LastIKLocPosition;
				cCDIKBone.transform.localRotation = cCDIKBone.LastIKLocRotation;
				cCDIKBone = cCDIKBone.IKChild;
			}
		}
		else if (SyncWithAnimator > 0f)
		{
			while (cCDIKBone != null)
			{
				cCDIKBone.LastKeyLocalRotation = cCDIKBone.transform.localRotation;
				cCDIKBone = cCDIKBone.IKChild;
			}
		}
		if (ReactionQuality < 0)
		{
			ReactionQuality = 1;
		}
		Vector3 val = Vector3.zero;
		if (ReactionQuality > 1)
		{
			val = GetGoalPivotOffset();
		}
		for (int i = 0; i < ReactionQuality && (i < 1 || ((Vector3)(ref val)).sqrMagnitude != 0f || !(Smoothing > 0f) || !(GetVelocityDifference() < Smoothing * Smoothing)); i++)
		{
			LastLocalDirection = RefreshLocalDirection();
			Vector3 val2 = IKTargetPosition + val;
			cCDIKBone = IKBones[IKBones.Length - 2];
			if (!Use2D)
			{
				if (!Invert)
				{
					while (cCDIKBone != null)
					{
						float num = cCDIKBone.MotionWeight * IKWeight;
						if (num > 0f)
						{
							Quaternion val3 = Quaternion.FromToRotation(base.Bones[base.Bones.Length - 1].transform.position - cCDIKBone.transform.position, val2 - cCDIKBone.transform.position) * cCDIKBone.transform.rotation;
							if (num < 1f)
							{
								cCDIKBone.transform.rotation = Quaternion.Lerp(cCDIKBone.transform.rotation, val3, num);
							}
							else
							{
								cCDIKBone.transform.rotation = val3;
							}
						}
						cCDIKBone.AngleLimiting();
						cCDIKBone = cCDIKBone.IKParent;
					}
					continue;
				}
				while (cCDIKBone != null)
				{
					cCDIKBone.AngleLimiting();
					cCDIKBone = cCDIKBone.IKParent;
				}
				for (cCDIKBone = IKBones[0]; cCDIKBone != null; cCDIKBone = cCDIKBone.IKChild)
				{
					float num2 = cCDIKBone.MotionWeight * IKWeight;
					if (num2 > 0f)
					{
						Quaternion val4 = Quaternion.FromToRotation(base.Bones[base.Bones.Length - 1].transform.position - cCDIKBone.transform.position, val2 - cCDIKBone.transform.position) * cCDIKBone.transform.rotation;
						if (num2 < 1f)
						{
							cCDIKBone.transform.rotation = Quaternion.Lerp(cCDIKBone.transform.rotation, val4, num2);
						}
						else
						{
							cCDIKBone.transform.rotation = val4;
						}
					}
				}
				continue;
			}
			if (!Invert)
			{
				while (cCDIKBone != null)
				{
					float num3 = cCDIKBone.MotionWeight * IKWeight;
					if (num3 > 0f)
					{
						Vector3 val5 = base.Bones[base.Bones.Length - 1].transform.position - cCDIKBone.transform.position;
						Vector3 val6 = val2 - cCDIKBone.transform.position;
						cCDIKBone.transform.rotation = Quaternion.AngleAxis(Mathf.DeltaAngle(Mathf.Atan2(val5.x, val5.y) * 57.29578f, Mathf.Atan2(val6.x, val6.y) * 57.29578f) * num3, Vector3.back) * cCDIKBone.transform.rotation;
					}
					cCDIKBone.AngleLimiting();
					cCDIKBone = cCDIKBone.IKParent;
				}
				continue;
			}
			while (cCDIKBone != null)
			{
				cCDIKBone.AngleLimiting();
				cCDIKBone = cCDIKBone.IKParent;
			}
			for (cCDIKBone = IKBones[0]; cCDIKBone != null; cCDIKBone = cCDIKBone.IKChild)
			{
				float num4 = cCDIKBone.MotionWeight * IKWeight;
				if (num4 > 0f)
				{
					Vector3 val7 = base.Bones[base.Bones.Length - 1].transform.position - cCDIKBone.transform.position;
					Vector3 val8 = val2 - cCDIKBone.transform.position;
					cCDIKBone.transform.rotation = Quaternion.AngleAxis(Mathf.DeltaAngle(Mathf.Atan2(val7.x, val7.y) * 57.29578f, Mathf.Atan2(val8.x, val8.y) * 57.29578f) * num4, Vector3.back) * cCDIKBone.transform.rotation;
				}
			}
		}
		LastLocalDirection = RefreshLocalDirection();
		if (StretchToTarget > 0f)
		{
			Vector3 val9 = IKTargetPosition - EndIKBone.transform.position;
			float num5 = ((Vector3)(ref val9)).magnitude;
			ActiveLength = Mathf.Epsilon;
			cCDIKBone = IKBones[0];
			int num6 = 0;
			float num7 = Mathf.Max(1f, StretchToTarget);
			while (cCDIKBone.IKChild != null && !(num5 <= 0f))
			{
				Vector3 val10 = IKTargetPosition - cCDIKBone.transform.position;
				Vector3 normalized = ((Vector3)(ref val10)).normalized;
				Vector3 position = cCDIKBone.transform.position;
				Vector3 position2 = cCDIKBone.IKChild.transform.position;
				Vector3 val11 = position2 - position;
				Vector3 normalized2 = ((Vector3)(ref val11)).normalized;
				float num8 = Vector3.Dot(normalized2, normalized);
				if (num8 > 0f)
				{
					float num9 = cCDIKBone.BoneLength * num7 * num8;
					if (num9 > num5)
					{
						num9 = num5;
					}
					Vector3 val12 = position2 + normalized2 * num9;
					cCDIKBone.IKChild.transform.position = Vector3.Lerp(position2, val12, StretchToTarget);
					cCDIKBone.transform.rotation = cCDIKBone.transform.rotation * Quaternion.FromToRotation(position2 - position, cCDIKBone.Child.transform.position - cCDIKBone.transform.position);
					num5 -= Vector3.Distance(position2, val12);
				}
				cCDIKBone = cCDIKBone.IKChild;
				num6++;
			}
		}
		for (cCDIKBone = IKBones[0]; cCDIKBone != null; cCDIKBone = cCDIKBone.IKChild)
		{
			cCDIKBone.LastIKLocRotation = cCDIKBone.transform.localRotation;
			cCDIKBone.LastIKLocPosition = cCDIKBone.transform.localPosition;
			Quaternion val13 = cCDIKBone.LastIKLocRotation * Quaternion.Inverse(cCDIKBone.InitialLocalRotation);
			cCDIKBone.transform.localRotation = Quaternion.Lerp(cCDIKBone.LastIKLocRotation, val13 * cCDIKBone.LastKeyLocalRotation, SyncWithAnimator);
			if (IKWeight < 1f)
			{
				cCDIKBone.transform.localRotation = Quaternion.Lerp(cCDIKBone.LastKeyLocalRotation, cCDIKBone.transform.localRotation, IKWeight);
			}
		}
	}

	protected Vector3 GetGoalPivotOffset()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		if (!GoalPivotOffsetDetected())
		{
			return Vector3.zero;
		}
		Vector3 val = IKTargetPosition - IKBones[0].transform.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(normalized.y, normalized.z, normalized.x);
		if (IKBones[IKBones.Length - 2].AngleLimit < 180f || IKBones[IKBones.Length - 2].TwistAngleLimit < 180f)
		{
			val2 = IKBones[IKBones.Length - 2].transform.rotation * IKBones[IKBones.Length - 2].ForwardOrientation;
		}
		return Vector3.Cross(normalized, val2) * IKBones[IKBones.Length - 2].BoneLength * 0.5f;
	}

	private bool GoalPivotOffsetDetected()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Initialized)
		{
			return false;
		}
		Vector3 val = base.Bones[base.Bones.Length - 1].transform.position - base.Bones[0].transform.position;
		Vector3 val2 = IKTargetPosition - base.Bones[0].transform.position;
		float magnitude = ((Vector3)(ref val)).magnitude;
		float magnitude2 = ((Vector3)(ref val2)).magnitude;
		if (magnitude2 == 0f)
		{
			return false;
		}
		if (magnitude == 0f)
		{
			return false;
		}
		if (magnitude < magnitude2)
		{
			return false;
		}
		if (magnitude < base.fullLength - base.Bones[base.Bones.Length - 2].BoneLength * 0.1f)
		{
			return false;
		}
		if (magnitude2 > magnitude)
		{
			return false;
		}
		if (Vector3.Dot(val / magnitude, val2 / magnitude2) < 0.999f)
		{
			return false;
		}
		return true;
	}

	private Vector3 RefreshLocalDirection()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		LocalDirection = base.Bones[0].transform.InverseTransformDirection(base.Bones[base.Bones.Length - 1].transform.position - base.Bones[0].transform.position);
		return LocalDirection;
	}

	private float GetVelocityDifference()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.SqrMagnitude(LocalDirection - LastLocalDirection);
	}

	public void AutoLimitAngle(float angleLimit = 60f, float twistAngleLimit = 50f)
	{
		if (IKBones == null)
		{
			return;
		}
		float num = 1f / (float)IKBones.Length;
		if (Invert)
		{
			for (int i = 0; i < IKBones.Length; i++)
			{
				IKBones[i].AngleLimit = angleLimit * Mathf.Min(1f, (1f - (float)(i + 1) * num) * 3f);
				IKBones[i].TwistAngleLimit = twistAngleLimit * Mathf.Min(1f, (1f - (float)(i + 1) * num) * 4.5f);
			}
		}
		else
		{
			for (int j = 0; j < IKBones.Length; j++)
			{
				IKBones[j].AngleLimit = angleLimit * Mathf.Min(1f, (float)(j + 1) * num * 3f);
				IKBones[j].TwistAngleLimit = twistAngleLimit * Mathf.Min(1f, (float)(j + 1) * num * 4.5f);
			}
		}
	}

	public void AutoWeightBones(float baseValue = 1f)
	{
		float num = baseValue / ((float)base.Bones.Length * 1.3f);
		if (Invert)
		{
			for (int i = 0; i < base.Bones.Length; i++)
			{
				base.Bones[i].MotionWeight = 1f - (baseValue - num * (float)i);
			}
		}
		else
		{
			for (int j = 0; j < base.Bones.Length; j++)
			{
				base.Bones[j].MotionWeight = baseValue - num * (float)j;
			}
		}
	}

	public void AutoWeightBones(AnimationCurve weightCurve)
	{
		if (Invert)
		{
			for (int i = 0; i < base.Bones.Length; i++)
			{
				base.Bones[i].MotionWeight = Mathf.Clamp(1f - weightCurve.Evaluate((float)i / (float)base.Bones.Length), 0f, 1f);
			}
		}
		else
		{
			for (int j = 0; j < base.Bones.Length; j++)
			{
				base.Bones[j].MotionWeight = Mathf.Clamp(weightCurve.Evaluate((float)j / (float)base.Bones.Length), 0f, 1f);
			}
		}
	}
}

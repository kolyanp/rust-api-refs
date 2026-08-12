using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FIMSpace.FTools;

[Serializable]
public abstract class FIK_ProcessorBase
{
	[Range(0f, 1f)]
	public float IKWeight;

	public Vector3 IKTargetPosition;

	public Quaternion IKTargetRotation;

	public Vector3 LastLocalDirection;

	public Vector3 LocalDirection;

	[CompilerGenerated]
	private Quaternion _003CStartBoneRotationOffset_003Ek__BackingField;

	[NonSerialized]
	public bool CallPreCalibrate;

	public float fullLength { get; protected set; }

	public bool Initialized { get; protected set; }

	public FIK_IKBoneBase[] Bones { get; protected set; }

	public FIK_IKBoneBase StartBone => Bones[0];

	public FIK_IKBoneBase EndBone => Bones[Bones.Length - 1];

	public Quaternion StartBoneRotationOffset
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CStartBoneRotationOffset_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CStartBoneRotationOffset_003Ek__BackingField = value;
		}
	}

	public virtual void Init(Transform root)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		StartBoneRotationOffset = Quaternion.identity;
	}

	public virtual void PreCalibrate()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (CallPreCalibrate)
		{
			for (FIK_IKBoneBase fIK_IKBoneBase = Bones[0]; fIK_IKBoneBase != null; fIK_IKBoneBase = fIK_IKBoneBase.Child)
			{
				fIK_IKBoneBase.transform.localRotation = fIK_IKBoneBase.InitialLocalRotation;
			}
		}
	}

	public virtual void Update()
	{
	}

	public static float EaseInOutQuint(float start, float end, float value)
	{
		value /= 0.5f;
		end -= start;
		if (value < 1f)
		{
			return end * 0.5f * value * value * value * value * value + start;
		}
		value -= 2f;
		return end * 0.5f * (value * value * value * value * value + 2f) + start;
	}

	protected FIK_ProcessorBase()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		IKWeight = 1f;
		StartBoneRotationOffset = Quaternion.identity;
		CallPreCalibrate = true;
		base._002Ector();
	}
}

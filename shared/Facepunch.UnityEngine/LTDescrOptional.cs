using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LTDescrOptional
{
	[CompilerGenerated]
	private Vector3 _003Cpoint_003Ek__BackingField;

	[CompilerGenerated]
	private Vector3 _003Caxis_003Ek__BackingField;

	[CompilerGenerated]
	private Quaternion _003CorigRotation_003Ek__BackingField;

	public AnimationCurve animationCurve;

	public int initFrameCount;

	public Transform toTrans { get; set; }

	public Vector3 point
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003Cpoint_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003Cpoint_003Ek__BackingField = value;
		}
	}

	public Vector3 axis
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003Caxis_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003Caxis_003Ek__BackingField = value;
		}
	}

	public float lastVal { get; set; }

	public Quaternion origRotation
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CorigRotation_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CorigRotation_003Ek__BackingField = value;
		}
	}

	public LTBezierPath path { get; set; }

	public LTSpline spline { get; set; }

	public LTRect ltRect { get; set; }

	public Action<float> onUpdateFloat { get; set; }

	public Action<float, float> onUpdateFloatRatio { get; set; }

	public Action<float, object> onUpdateFloatObject { get; set; }

	public Action<Vector2> onUpdateVector2 { get; set; }

	public Action<Vector3> onUpdateVector3 { get; set; }

	public Action<Vector3, object> onUpdateVector3Object { get; set; }

	public Action<Color> onUpdateColor { get; set; }

	public Action<Color, object> onUpdateColorObject { get; set; }

	public Action onComplete { get; set; }

	public Action<object> onCompleteObject { get; set; }

	public object onCompleteParam { get; set; }

	public object onUpdateParam { get; set; }

	public Action onStart { get; set; }

	public void reset()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		animationCurve = null;
		onUpdateFloat = null;
		onUpdateFloatRatio = null;
		onUpdateVector2 = null;
		onUpdateVector3 = null;
		onUpdateFloatObject = null;
		onUpdateVector3Object = null;
		onUpdateColor = null;
		onComplete = null;
		onCompleteObject = null;
		onCompleteParam = null;
		onStart = null;
		point = Vector3.zero;
		initFrameCount = 0;
	}

	public void callOnUpdate(float val, float ratioPassed)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		if (onUpdateFloat != null)
		{
			onUpdateFloat(val);
		}
		if (onUpdateFloatRatio != null)
		{
			onUpdateFloatRatio(val, ratioPassed);
		}
		else if (onUpdateFloatObject != null)
		{
			if (onUpdateParam != null)
			{
				onUpdateFloatObject(val, onUpdateParam);
			}
		}
		else if (onUpdateVector3Object != null)
		{
			if (onUpdateParam != null)
			{
				onUpdateVector3Object(LTDescr.newVect, onUpdateParam);
			}
		}
		else if (onUpdateVector3 != null)
		{
			onUpdateVector3(LTDescr.newVect);
		}
		else if (onUpdateVector2 != null)
		{
			onUpdateVector2(new Vector2(LTDescr.newVect.x, LTDescr.newVect.y));
		}
	}
}

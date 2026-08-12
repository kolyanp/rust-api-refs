using System.Runtime.CompilerServices;
using UnityEngine;

namespace FIMSpace.GroundFitter;

[RequireComponent(typeof(FGroundFitter_Movement))]
public abstract class FGroundFitter_InputBase : MonoBehaviour
{
	protected FGroundFitter fitter;

	protected FGroundFitter_Movement controller;

	[CompilerGenerated]
	private Vector3 _003CMoveVector_003Ek__BackingField;

	public float RotationOffset { get; protected set; }

	public bool Sprint { get; protected set; }

	public Vector3 MoveVector
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CMoveVector_003Ek__BackingField;
		}
		[CompilerGenerated]
		protected set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CMoveVector_003Ek__BackingField = value;
		}
	}

	public virtual void Start()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		fitter = ((Component)this).GetComponent<FGroundFitter>();
		controller = ((Component)this).GetComponent<FGroundFitter_Movement>();
		RotationOffset = 0f;
		Sprint = false;
		MoveVector = Vector3.zero;
	}

	protected virtual void TriggerJump()
	{
		controller.Jump();
	}
}

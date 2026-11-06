using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_TargetInRange : FSMTransitionBase
{
	[SerializeField]
	public float Range = 4f;

	[SerializeField]
	public float TimeToPredict;

	[SerializeField]
	[Tooltip("Point on the NPC to measure from, in its local space, or zero for its origin. A long animal measures its attack range from out in front of itself rather than from its middle.")]
	public Vector3 Offset = Vector3.zero;

	[SerializeField]
	[Tooltip("How far above or below us the target may be, or zero for no bound. A range measured in three dimensions counts a storey of height as ordinary reach, so an attack that cannot swing upwards needs this as well as a range.")]
	public float MaxHeightDifference;

	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Trans_TargetInRange"))
		{
			if (!Senses.FindTarget(out var target))
			{
				return false;
			}
			Vector3 val = ((Component)target).transform.position;
			if (TimeToPredict > 0f && target.ToNonNpcPlayer(out var player))
			{
				Vector3 inferedVelocity = player.inferedVelocity;
				inferedVelocity = Vector3.ProjectOnPlane(inferedVelocity, ((Component)Owner).transform.right);
				val += inferedVelocity * TimeToPredict;
			}
			Vector3 val2 = ((Component)Owner).transform.position;
			if (Offset != Vector3.zero)
			{
				val2 += ((Component)Owner).transform.TransformDirection(Offset);
			}
			if (MaxHeightDifference > 0f && Mathf.Abs(val.y - val2.y) > MaxHeightDifference)
			{
				return false;
			}
			return Vector3.Distance(val, val2) <= Range;
		}
	}

	public override string GetName()
	{
		string text = ((MaxHeightDifference > 0f) ? $" +-{MaxHeightDifference}m" : "");
		return string.Format("{0} {1}{2}m{3}", new object[4]
		{
			base.GetName(),
			Inverted ? ">=" : "<",
			Range,
			text
		});
	}

	public Trans_TargetInRange()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
	}
}

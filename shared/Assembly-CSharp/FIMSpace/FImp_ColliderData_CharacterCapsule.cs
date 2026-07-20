using UnityEngine;

namespace FIMSpace;

public class FImp_ColliderData_CharacterCapsule : FImp_ColliderData_Base
{
	private Vector3 Top;

	private Vector3 Bottom;

	private Vector3 Direction;

	private float radius;

	private float scaleFactor;

	private float preRadius;

	public CharacterController Capsule { get; private set; }

	public FImp_ColliderData_CharacterCapsule(CharacterController collider)
	{
		Is2D = false;
		base.Transform = ((Component)collider).transform;
		base.Collider = (Collider)(object)collider;
		base.Transform = ((Component)collider).transform;
		Capsule = collider;
		base.ColliderType = EFColliderType.Capsule;
		CalculateCapsuleParameters(Capsule, ref Direction, ref radius, ref scaleFactor);
		RefreshColliderData();
	}

	public override void RefreshColliderData()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsStatic)
		{
			bool flag = false;
			if (!FEngineering.VIsSame(previousPosition, base.Transform.position))
			{
				flag = true;
			}
			else if (!FEngineering.QIsSame(base.Transform.rotation, previousRotation))
			{
				flag = true;
			}
			else if (preRadius != Capsule.radius || !FEngineering.VIsSame(previousScale, base.Transform.lossyScale))
			{
				CalculateCapsuleParameters(Capsule, ref Direction, ref radius, ref scaleFactor);
			}
			if (flag)
			{
				GetCapsuleHeadsPositions(Capsule, ref Top, ref Bottom, Direction, radius, scaleFactor);
			}
			base.RefreshColliderData();
			previousPosition = base.Transform.position;
			previousRotation = base.Transform.rotation;
			previousScale = base.Transform.lossyScale;
			preRadius = Capsule.radius;
		}
	}

	public override bool PushIfInside(ref Vector3 point, float pointRadius, Vector3 pointOffset)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return PushOutFromCapsuleCollider(pointRadius, ref point, Top, Bottom, radius, pointOffset);
	}

	public static bool PushOutFromCapsuleCollider(float segmentColliderRadius, ref Vector3 segmentPos, Vector3 capSphereCenter1, Vector3 capSphereCenter2, float capsuleRadius, Vector3 segmentOffset, bool is2D = false)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		float num = capsuleRadius + segmentColliderRadius;
		Vector3 val = capSphereCenter2 - capSphereCenter1;
		Vector3 val2 = segmentPos + segmentOffset - capSphereCenter1;
		float num2 = Vector3.Dot(val2, val);
		if (num2 <= 0f)
		{
			float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
			if (sqrMagnitude > 0f && sqrMagnitude < num * num)
			{
				segmentPos = capSphereCenter1 - segmentOffset + val2 * (num / Mathf.Sqrt(sqrMagnitude));
				return true;
			}
		}
		else
		{
			float sqrMagnitude2 = ((Vector3)(ref val)).sqrMagnitude;
			if (num2 >= sqrMagnitude2)
			{
				val2 = segmentPos + segmentOffset - capSphereCenter2;
				float sqrMagnitude3 = ((Vector3)(ref val2)).sqrMagnitude;
				if (sqrMagnitude3 > 0f && sqrMagnitude3 < num * num)
				{
					segmentPos = capSphereCenter2 - segmentOffset + val2 * (num / Mathf.Sqrt(sqrMagnitude3));
					return true;
				}
			}
			else if (sqrMagnitude2 > 0f)
			{
				val2 -= val * (num2 / sqrMagnitude2);
				float sqrMagnitude4 = ((Vector3)(ref val2)).sqrMagnitude;
				if (sqrMagnitude4 > 0f && sqrMagnitude4 < num * num)
				{
					float num3 = Mathf.Sqrt(sqrMagnitude4);
					segmentPos += val2 * ((num - num3) / num3);
					return true;
				}
			}
		}
		return false;
	}

	protected static void CalculateCapsuleParameters(CharacterController capsule, ref Vector3 direction, ref float trueRadius, ref float scalerFactor)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)capsule).transform;
		direction = Vector3.up;
		scalerFactor = transform.lossyScale.y;
		float num = ((transform.lossyScale.x > transform.lossyScale.z) ? transform.lossyScale.x : transform.lossyScale.z);
		trueRadius = capsule.radius * num;
	}

	protected static void GetCapsuleHeadsPositions(CharacterController capsule, ref Vector3 upper, ref Vector3 bottom, Vector3 direction, float radius, float scalerFactor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = direction * (capsule.height / 2f * scalerFactor - radius);
		upper = ((Component)capsule).transform.position + ((Component)capsule).transform.TransformDirection(val) + ((Component)capsule).transform.TransformVector(capsule.center);
		Vector3 val2 = -direction * (capsule.height / 2f * scalerFactor - radius);
		bottom = ((Component)capsule).transform.position + ((Component)capsule).transform.TransformDirection(val2) + ((Component)capsule).transform.TransformVector(capsule.center);
	}
}

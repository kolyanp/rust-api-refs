using System.Collections.Generic;
using UnityEngine;

public class Socket_Free : Socket_Base
{
	public Vector3 idealPlacementNormal = Vector3.up;

	public bool useTargetNormal = true;

	public bool snapToTargetProvidedRotations;

	public bool blendAimAngle = true;

	public override bool TestTarget(Construction.Target target)
	{
		return target.onTerrain;
	}

	public override Construction.Placement DoPlacement(Construction.Target target)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		Quaternion identity = Quaternion.identity;
		Vector3 val2;
		if (snapToTargetProvidedRotations && !target.isHoldingShift && (Object)(object)target.entity != (Object)null && (Object)(object)target.player != (Object)null && target.entity is IPlacementDirectionProvider placementDirectionProvider)
		{
			List<Vector3> snapForwardDirections = placementDirectionProvider.GetSnapForwardDirections();
			identity = GetTargetSnappedRotation(((Component)target.entity).transform, ((Component)target.player).transform.position, target.position, target.rotation.y, snapForwardDirections);
		}
		else if (useTargetNormal)
		{
			Vector3 normal = target.normal;
			Vector3 val = idealPlacementNormal;
			if (blendAimAngle || Mathf.Abs(target.normal.y) > 0.98f)
			{
				val2 = target.position - ((Ray)(ref target.ray)).origin;
				Vector3 normalized = ((Vector3)(ref val2)).normalized;
				float num = Mathf.Abs(Vector3.Dot(normalized, normal));
				val = Vector3.Lerp(normalized, idealPlacementNormal, num);
			}
			identity = Quaternion.LookRotation(normal, val) * Quaternion.Inverse(rotation) * Quaternion.Euler(target.rotation);
		}
		else
		{
			val2 = target.position - ((Ray)(ref target.ray)).origin;
			Vector3 normalized2 = ((Vector3)(ref val2)).normalized;
			normalized2.y = 0f;
			identity = Quaternion.LookRotation(normalized2, idealPlacementNormal) * Quaternion.Euler(target.rotation);
		}
		Vector3 val3 = target.position;
		val3 -= identity * position;
		Construction.Placement result = new Construction.Placement(target);
		result.rotation = identity;
		result.position = val3;
		return result;
	}

	private Quaternion GetTargetSnappedRotation(Transform targetTransform, Vector3 playerWorldPos, Vector3 placementWorldPos, float yOffsetDegrees, List<Vector3> cachedLocalDirs)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = placementWorldPos - playerWorldPos;
		val = Vector3.ProjectOnPlane(val, targetTransform.up);
		((Vector3)(ref val)).Normalize();
		Quaternion val2 = Quaternion.LookRotation(SnapToBestTargetLocalCardinal(val, targetTransform, cachedLocalDirs), targetTransform.up);
		return Quaternion.AngleAxis(yOffsetDegrees, targetTransform.up) * val2;
	}

	private Vector3 SnapToBestTargetLocalCardinal(Vector3 desiredWorldForward, Transform target, List<Vector3> cachedLocalDirs)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		float num = -1f;
		float num2 = 0f;
		Vector3 val = target.forward;
		for (int i = 0; i < cachedLocalDirs.Count; i++)
		{
			Vector3 val2 = Vector3.ProjectOnPlane(target.TransformDirection(cachedLocalDirs[i]), target.up);
			if (!(((Vector3)(ref val2)).sqrMagnitude < 0.0001f))
			{
				((Vector3)(ref val2)).Normalize();
				float num3 = Vector3.Dot(desiredWorldForward, val2);
				float num4 = Mathf.Abs(num3);
				if (num4 > num)
				{
					num = num4;
					num2 = num3;
					val = val2;
				}
			}
		}
		if (!(num2 >= 0f))
		{
			return -val;
		}
		return val;
	}
}

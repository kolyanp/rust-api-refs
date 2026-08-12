using System;
using UnityEngine;

public class SocketMod_UseTargetOrientation : SocketMod
{
	[Flags]
	public enum OrientationAxes
	{
		X = 1,
		Y = 2,
		Z = 4
	}

	public OrientationAxes inheritAxes;

	public GameObjectRef[] onlyOrientateToThese = Array.Empty<GameObjectRef>();

	public bool ignoreIfHoldingShift;

	private const float alignAxisThreshold = 0.0001f;

	public override bool DoCheck(ref Construction.Placement place)
	{
		return true;
	}

	public override void ModifyPlacement(ref Construction.Placement place)
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		if (inheritAxes == (OrientationAxes)0 || (ignoreIfHoldingShift && place.isHoldingShift) || (Object)(object)place.transform == (Object)null)
		{
			return;
		}
		if (onlyOrientateToThese.Length != 0)
		{
			bool flag = false;
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(place.transform);
			if ((Object)(object)baseEntity != (Object)null)
			{
				GameObjectRef[] array = onlyOrientateToThese;
				foreach (GameObjectRef gameObjectRef in array)
				{
					if (baseEntity.prefabID == gameObjectRef.resourceID)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return;
			}
		}
		Quaternion rotation = place.rotation;
		Quaternion rotation2 = place.transform.rotation;
		bool flag2 = (inheritAxes & OrientationAxes.X) != 0;
		bool flag3 = (inheritAxes & OrientationAxes.Y) != 0;
		bool flag4 = (inheritAxes & OrientationAxes.Z) != 0;
		switch ((flag2 ? 1 : 0) + (flag3 ? 1 : 0) + (flag4 ? 1 : 0))
		{
		case 1:
		{
			Vector3 val8 = (flag2 ? (rotation * Vector3.right) : (flag3 ? (rotation * Vector3.up) : (rotation * Vector3.forward)));
			Vector3 val9 = (flag2 ? (rotation2 * Vector3.right) : (flag3 ? (rotation2 * Vector3.up) : (rotation2 * Vector3.forward)));
			if (((Vector3)(ref val8)).sqrMagnitude > 0.0001f && ((Vector3)(ref val9)).sqrMagnitude > 0.0001f)
			{
				Quaternion val10 = Quaternion.FromToRotation(val8, val9);
				place.rotation = val10 * rotation;
			}
			break;
		}
		case 2:
		{
			Vector3 val3;
			if (flag2 & flag3)
			{
				Vector3 val = rotation2 * Vector3.right;
				Vector3 val2 = rotation2 * Vector3.up;
				val3 = Vector3.Cross(val, val2);
				Vector3 normalized = ((Vector3)(ref val3)).normalized;
				if (((Vector3)(ref normalized)).sqrMagnitude > 0.0001f)
				{
					place.rotation = Quaternion.LookRotation(normalized, val2);
				}
			}
			else if (flag3 & flag4)
			{
				Vector3 val4 = rotation2 * Vector3.up;
				Vector3 val5 = rotation2 * Vector3.forward;
				val3 = Vector3.Cross(val4, val5);
				Vector3 normalized2 = ((Vector3)(ref val3)).normalized;
				if (((Vector3)(ref normalized2)).sqrMagnitude > 0.0001f)
				{
					place.rotation = Quaternion.LookRotation(val5, val4);
				}
			}
			else if (flag2 & flag4)
			{
				Vector3 val6 = rotation2 * Vector3.right;
				Vector3 val7 = rotation2 * Vector3.forward;
				val3 = Vector3.Cross(val7, val6);
				Vector3 normalized3 = ((Vector3)(ref val3)).normalized;
				if (((Vector3)(ref normalized3)).sqrMagnitude > 0.0001f)
				{
					place.rotation = Quaternion.LookRotation(val7, normalized3);
				}
			}
			break;
		}
		case 3:
			place.rotation = rotation2;
			break;
		}
	}
}

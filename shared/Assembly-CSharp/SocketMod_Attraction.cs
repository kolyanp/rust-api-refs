using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_Attraction : SocketMod
{
	public float outerRadius = 1f;

	public float innerRadius = 0.1f;

	public string groupName = "wallbottom";

	public bool lockRotation;

	public bool bypassPlayerRotation;

	public bool ignoreRotationForRadiusCheck;

	public bool shiftDisableSnap = true;

	public bool shiftEnableSnap;

	public bool applyPostRotationSnapping;

	private static float[] PostSnapRotations = new float[4] { 0f, 90f, 180f, 270f };

	private void OnDrawGizmosSelected()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.matrix = ((Component)this).transform.localToWorldMatrix;
		Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
		Gizmos.DrawSphere(Vector3.zero, outerRadius);
		Gizmos.color = new Color(0f, 1f, 0f, 0.6f);
		Gizmos.DrawSphere(Vector3.zero, innerRadius);
	}

	public override bool DoCheck(ref Construction.Placement place)
	{
		return true;
	}

	public override void ModifyPlacement(ref Construction.Placement place)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		if ((shiftEnableSnap && !place.isHoldingShift) || (shiftDisableSnap && place.isHoldingShift))
		{
			return;
		}
		Vector3 val = place.position + place.rotation * worldPosition;
		Quaternion rotation = place.rotation;
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		Vis.Entities(val, outerRadius * 2f, list, -1, (QueryTriggerInteraction)2);
		Vector3 position = Vector3.zero;
		float num = float.MaxValue;
		Vector3 position2 = place.position;
		Quaternion rotation2 = Quaternion.identity;
		foreach (BaseEntity item in list)
		{
			if (item.isServer != isServer || place.ShouldIgnoreEntity(item))
			{
				continue;
			}
			AttractionPoint[] array = prefabAttribute.FindAll<AttractionPoint>(item.prefabID);
			if (array == null)
			{
				continue;
			}
			AttractionPoint[] array2 = array;
			foreach (AttractionPoint attractionPoint in array2)
			{
				if (attractionPoint.groupName != groupName)
				{
					continue;
				}
				Vector3 val2 = ((Component)item).transform.position + ((Component)item).transform.rotation * attractionPoint.worldPosition;
				Vector3 val3 = val2 - val;
				float magnitude = ((Vector3)(ref val3)).magnitude;
				if (ignoreRotationForRadiusCheck)
				{
					Vector3 val4 = ((Component)item).transform.TransformPoint(Vector3.LerpUnclamped(Vector3.zero, Vector3Ex.WithY(attractionPoint.worldPosition, 0f), 2f));
					float num2 = Vector3.Distance(val4, position2);
					if (num2 < num)
					{
						num = num2;
						position = val4;
						rotation2 = ((Component)item).transform.rotation;
					}
				}
				if (magnitude > outerRadius)
				{
					continue;
				}
				Quaternion val5 = ((lockRotation && bypassPlayerRotation) ? (((Component)item).transform.rotation * attractionPoint.localRotation) : QuaternionEx.LookRotationWithOffset(worldPosition, val2 - place.position, Vector3.up));
				float num3 = Mathf.InverseLerp(outerRadius, innerRadius, magnitude);
				if (lockRotation)
				{
					num3 = 1f;
				}
				if (lockRotation)
				{
					if (bypassPlayerRotation)
					{
						place.rotation = ((Component)item).transform.rotation * attractionPoint.localRotation;
					}
					else
					{
						Vector3 eulerAngles = ((Quaternion)(ref place.rotation)).eulerAngles;
						eulerAngles -= new Vector3(eulerAngles.x % 90f, eulerAngles.y % 90f, eulerAngles.z % 90f);
						place.rotation = Quaternion.Euler(eulerAngles + ((Component)item).transform.eulerAngles);
					}
				}
				else
				{
					place.rotation = Quaternion.Lerp(place.rotation, val5, num3);
				}
				val = place.position + place.rotation * worldPosition;
				val3 = val2 - val;
				place.position += val3 * num3;
			}
		}
		if (num < float.MaxValue && ignoreRotationForRadiusCheck)
		{
			place.position = position;
			place.rotation = rotation2;
		}
		if (applyPostRotationSnapping)
		{
			Quaternion rotation3 = place.rotation;
			float num4 = float.MaxValue;
			float[] postSnapRotations = PostSnapRotations;
			foreach (float num5 in postSnapRotations)
			{
				Quaternion val6 = place.rotation * Quaternion.Euler(0f, num5, 0f);
				float num6 = Quaternion.Angle(val6, rotation);
				if (num6 < num4)
				{
					num4 = num6;
					rotation3 = val6;
				}
			}
			if (num4 < float.MaxValue)
			{
				place.rotation = rotation3;
			}
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
	}
}

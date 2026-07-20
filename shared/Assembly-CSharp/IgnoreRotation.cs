using System;
using UnityEngine;

public class IgnoreRotation : MonoBehaviour
{
	[Serializable]
	public enum RotationType
	{
		None,
		X,
		Y,
		Z
	}

	public RotationType ignoreType;

	public Transform parent;

	private void LateUpdate()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		if (ignoreType != RotationType.None)
		{
			Quaternion localRotation;
			if (ignoreType == RotationType.X)
			{
				Transform transform = ((Component)this).transform;
				localRotation = parent.localRotation;
				float y = ((Quaternion)(ref localRotation)).eulerAngles.y;
				localRotation = parent.localRotation;
				transform.localRotation = Quaternion.Euler(0f, y, ((Quaternion)(ref localRotation)).eulerAngles.z);
			}
			else if (ignoreType == RotationType.Y)
			{
				Transform transform2 = ((Component)this).transform;
				localRotation = parent.localRotation;
				float x = ((Quaternion)(ref localRotation)).eulerAngles.x;
				localRotation = parent.localRotation;
				transform2.localRotation = Quaternion.Euler(x, 0f, ((Quaternion)(ref localRotation)).eulerAngles.z);
			}
			else if (ignoreType == RotationType.Z)
			{
				Transform transform3 = ((Component)this).transform;
				localRotation = parent.localRotation;
				float x2 = ((Quaternion)(ref localRotation)).eulerAngles.x;
				localRotation = parent.localRotation;
				transform3.localRotation = Quaternion.Euler(x2, ((Quaternion)(ref localRotation)).eulerAngles.y, 0f);
			}
		}
	}
}

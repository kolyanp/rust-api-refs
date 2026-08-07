using System.Collections.Generic;
using UnityEngine;

public class CH47DropZone : MonoBehaviour
{
	public TimeSince lastDropTime;

	public static List<CH47DropZone> dropZones = new List<CH47DropZone>();

	public void Awake()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (!dropZones.Contains(this))
		{
			dropZones.Add(this);
		}
		lastDropTime = TimeSince.op_Implicit(100000000f);
	}

	public static CH47DropZone GetClosest(Vector3 pos)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		float num = float.PositiveInfinity;
		CH47DropZone result = null;
		foreach (CH47DropZone dropZone in dropZones)
		{
			float num2 = Vector3Ex.Distance2D(pos, ((Component)dropZone).transform.position);
			if (num2 < num)
			{
				num = num2;
				result = dropZone;
			}
		}
		return result;
	}

	public void OnDestroy()
	{
		if (dropZones.Contains(this))
		{
			dropZones.Remove(this);
		}
	}

	public void Used()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		lastDropTime = TimeSince.op_Implicit(0f);
	}

	public void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(((Component)this).transform.position, 5f);
	}
}

using UnityEngine;

public class TickHistory
{
	private Deque<Vector3> points;

	private Deque<Vector3> parentPoints;

	public int Count => points.Count;

	public int ParentCount => parentPoints.Count;

	public Vector3 this[int index]
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return points[index];
		}
	}

	public TickHistory(int capacity)
	{
		points = new Deque<Vector3>(capacity);
		parentPoints = new Deque<Vector3>(capacity);
	}

	public Vector3 GetHistoryAtIndex(int index)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return points[index];
	}

	public Vector3 GetParentHistoryAtIndex(int index)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return parentPoints[index];
	}

	public void Reset()
	{
		points.Clear();
		parentPoints.Clear();
	}

	public void Reset(Vector3 point)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Reset();
		AddPoint(point);
	}

	public float Distance(BasePlayer player, Vector3 point)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		if (points.Count == 0)
		{
			return player.Distance(point);
		}
		Vector3 position = ((Component)player).transform.position;
		Quaternion rotation = ((Component)player).transform.rotation;
		Bounds bounds = player.bounds;
		Matrix4x4 tickHistoryMatrix = player.tickHistoryMatrix;
		float num = float.MaxValue;
		Line val3 = default(Line);
		OBB val5 = default(OBB);
		for (int i = 0; i < points.Count; i++)
		{
			Vector3 val = ((Matrix4x4)(ref tickHistoryMatrix)).MultiplyPoint3x4(points[i]);
			Vector3 val2 = ((i == points.Count - 1) ? position : ((Matrix4x4)(ref tickHistoryMatrix)).MultiplyPoint3x4(points[i + 1]));
			((Line)(ref val3))._002Ector(val, val2);
			Vector3 val4 = ((Line)(ref val3)).ClosestPoint(point);
			((OBB)(ref val5))._002Ector(val4, rotation, bounds);
			num = Mathf.Min(num, ((OBB)(ref val5)).Distance(point));
		}
		return num;
	}

	public float DistanceParented(BasePlayer player, Vector3 point)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		int count = points.Count;
		int count2 = parentPoints.Count;
		if (count == 0 || count2 == 0)
		{
			return player.Distance(point);
		}
		int num = Mathf.Min(count, count2);
		int num2 = count - num;
		int num3 = count2 - num;
		Quaternion rotation = ((Component)player).transform.rotation;
		Bounds bounds = player.bounds;
		Vector3 val = (((Object)(object)((Component)player).transform.parent != (Object)null) ? ((Component)player).transform.parent.position : ((Component)player).transform.position);
		Matrix4x4 tickHistoryMatrix = player.tickHistoryMatrix;
		float num4 = float.MaxValue;
		Line val8 = default(Line);
		OBB val10 = default(OBB);
		for (int i = 0; i < num; i++)
		{
			Vector3 val2 = points[num2 + i];
			Vector3 val3 = parentPoints[num3 + i];
			Vector3 val4 = ((Matrix4x4)(ref tickHistoryMatrix)).MultiplyPoint3x4(val2) + (val3 - val);
			Vector3 val7;
			if (i < num - 1)
			{
				Vector3 val5 = points[num2 + i + 1];
				Vector3 val6 = parentPoints[num3 + i + 1];
				val7 = ((Matrix4x4)(ref tickHistoryMatrix)).MultiplyPoint3x4(val5) + (val6 - val);
			}
			else
			{
				val7 = ((Component)player).transform.position;
			}
			((Line)(ref val8))._002Ector(val4, val7);
			Vector3 val9 = ((Line)(ref val8)).ClosestPoint(point);
			((OBB)(ref val10))._002Ector(val9, rotation, bounds);
			num4 = Mathf.Min(num4, ((OBB)(ref val10)).Distance(point));
		}
		return num4;
	}

	public void AddPoint(Vector3 point, int limit = -1)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		while (limit > 0 && points.Count >= limit)
		{
			points.PopFront();
		}
		points.PushBack(point);
	}

	public void AddParentPoint(Vector3 point, int limit = -1)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		while (limit > 0 && parentPoints.Count >= limit)
		{
			parentPoints.PopFront();
		}
		parentPoints.PushBack(point);
	}

	public void TransformEntries(Matrix4x4 matrix)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < points.Count; i++)
		{
			Vector3 val = points[i];
			val = ((Matrix4x4)(ref matrix)).MultiplyPoint3x4(val);
			points[i] = val;
		}
	}
}

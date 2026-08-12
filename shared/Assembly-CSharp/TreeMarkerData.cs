using System;
using UnityEngine;

public class TreeMarkerData : PrefabAttribute, IServerComponent
{
	[Serializable]
	public struct MarkerLocation
	{
		public Vector3 LocalPosition;

		public Vector3 LocalNormal;
	}

	[Serializable]
	public struct GenerationArc
	{
		public Vector3 CentrePoint;

		public float Radius;

		public Vector3 Rotation;

		public int OverrideCount;
	}

	public GenerationArc[] GenerationArcs;

	public MarkerLocation[] Markers;

	public Vector3 GenerationStartPoint;

	public float GenerationRadius;

	public float MaxY;

	public float MinY;

	public bool ProcessAngleChecks;

	protected override Type GetIndexedType()
	{
		return typeof(TreeMarkerData);
	}

	public Vector3 GetNearbyPoint(Vector3 fromWorldPos, TreeEntity hitEntity, ref int ignoreIndex, out Vector3 normal)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		int num = Markers.Length;
		if (ignoreIndex != -1 && ProcessAngleChecks)
		{
			ignoreIndex++;
			if (ignoreIndex >= num)
			{
				ignoreIndex = 0;
			}
			normal = Markers[ignoreIndex].LocalNormal;
			return Markers[ignoreIndex].LocalPosition;
		}
		int num2 = Random.Range(0, num);
		float num3 = float.MaxValue;
		int num4 = -1;
		for (int i = 0; i < num; i++)
		{
			if (ignoreIndex == num2)
			{
				continue;
			}
			MarkerLocation markerLocation = Markers[num2];
			if (!(markerLocation.LocalPosition.y < MinY))
			{
				Vector3 val = ((Component)hitEntity).transform.InverseTransformPoint(fromWorldPos);
				Vector3 val2 = markerLocation.LocalPosition;
				val2.y = Mathf.Lerp(val2.y, val.y, 0.5f);
				Vector3 val3 = val2 - val;
				float sqrMagnitude = ((Vector3)(ref val3)).sqrMagnitude;
				sqrMagnitude *= Random.Range(0.95f, 1.05f);
				if (sqrMagnitude < num3)
				{
					num3 = sqrMagnitude;
					num4 = num2;
				}
				num2++;
				if (num2 >= num)
				{
					num2 = 0;
				}
			}
		}
		if (num4 > -1)
		{
			normal = Markers[num4].LocalNormal;
			ignoreIndex = num4;
			return Markers[num4].LocalPosition;
		}
		normal = Markers[0].LocalNormal;
		return Markers[0].LocalPosition;
	}

	public bool TryGetClosestPoint(Vector3 hitWorldPos, TreeEntity hitEntity, out Vector3 closestPoint, out Vector3 normal)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		int num = Markers.Length;
		int num2 = Random.Range(0, num);
		float num3 = float.MaxValue;
		int num4 = -1;
		Vector3 val = ((Component)hitEntity).transform.InverseTransformPoint(hitWorldPos);
		for (int i = 0; i < num; i++)
		{
			MarkerLocation markerLocation = Markers[num2];
			if (!(markerLocation.LocalPosition.y < MinY))
			{
				Vector3 val2 = markerLocation.LocalPosition - val;
				float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
				if (sqrMagnitude < num3)
				{
					num3 = sqrMagnitude;
					num4 = num2;
				}
				num2++;
				if (num2 >= num)
				{
					num2 = 0;
				}
			}
		}
		if (num4 > -1)
		{
			normal = Markers[num4].LocalNormal;
			closestPoint = Markers[num4].LocalPosition;
			return true;
		}
		closestPoint = default(Vector3);
		normal = default(Vector3);
		return false;
	}

	public TreeMarkerData()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		GenerationStartPoint = Vector3.up * 2f;
		GenerationRadius = 2f;
		MaxY = 1.7f;
		MinY = 0.2f;
		base._002Ector();
	}
}

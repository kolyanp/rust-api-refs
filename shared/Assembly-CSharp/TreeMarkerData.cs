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

	public Vector3 GenerationStartPoint = Vector3.up * 2f;

	public float GenerationRadius = 2f;

	public float MaxY = 1.7f;

	public float MinY = 0.2f;

	public bool ProcessAngleChecks;

	public float HotspotPadding;

	private const float MinHeightAboveTerrain = 0.1f;

	protected override Type GetIndexedType()
	{
		return typeof(TreeMarkerData);
	}

	public static bool IsAboveTerrain(Vector3 worldPos)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)TerrainMeta.HeightMap == (Object)null))
		{
			return worldPos.y >= TerrainMeta.HeightMap.GetHeight(worldPos) + 0.1f;
		}
		return true;
	}

	private bool IsMarkerAboveTerrain(int index, Transform tree)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return IsAboveTerrain(tree.TransformPoint(Markers[index].LocalPosition));
	}

	private bool IsValidMarker(int index, Transform tree)
	{
		if (Markers[index].LocalPosition.y >= MinY)
		{
			return IsMarkerAboveTerrain(index, tree);
		}
		return false;
	}

	public bool TryGetNearbyPoint(Vector3 fromWorldPos, TreeEntity hitEntity, ref int ignoreIndex, out Vector3 point, out Vector3 normal)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		int num = Markers.Length;
		Transform transform = ((Component)hitEntity).transform;
		if (ignoreIndex != -1 && ProcessAngleChecks)
		{
			for (int i = 1; i <= num; i++)
			{
				int num2 = (ignoreIndex + i) % num;
				if (IsMarkerAboveTerrain(num2, transform))
				{
					ignoreIndex = num2;
					point = Markers[num2].LocalPosition;
					normal = Markers[num2].LocalNormal;
					return true;
				}
			}
			point = default;
			normal = default;
			return false;
		}
		int num3 = Random.Range(0, num);
		Vector3 val = transform.InverseTransformPoint(fromWorldPos);
		float num4 = float.MaxValue;
		int num5 = -1;
		for (int j = 0; j < num; j++)
		{
			int num6 = (num3 + j) % num;
			if (num6 != ignoreIndex && IsValidMarker(num6, transform))
			{
				Vector3 val2 = Markers[num6].LocalPosition;
				val2.y = Mathf.Lerp(val2.y, val.y, 0.5f);
				Vector3 val3 = val2 - val;
				float sqrMagnitude = val3.sqrMagnitude;
				sqrMagnitude *= Random.Range(0.95f, 1.05f);
				if (sqrMagnitude < num4)
				{
					num4 = sqrMagnitude;
					num5 = num6;
				}
			}
		}
		if (num5 == -1 && ignoreIndex != -1 && IsValidMarker(ignoreIndex, transform))
		{
			num5 = ignoreIndex;
		}
		if (num5 > -1)
		{
			ignoreIndex = num5;
			point = Markers[num5].LocalPosition;
			normal = Markers[num5].LocalNormal;
			return true;
		}
		point = default;
		normal = default;
		return false;
	}

	public bool TryGetClosestPoint(Vector3 hitWorldPos, TreeEntity hitEntity, out Vector3 closestPoint, out Vector3 normal)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		int num = Markers.Length;
		int num2 = Random.Range(0, num);
		Transform transform = ((Component)hitEntity).transform;
		float num3 = float.MaxValue;
		int num4 = -1;
		Vector3 val = transform.InverseTransformPoint(hitWorldPos);
		for (int i = 0; i < num; i++)
		{
			int num5 = (num2 + i) % num;
			if (IsValidMarker(num5, transform))
			{
				Vector3 val2 = Markers[num5].LocalPosition - val;
				float sqrMagnitude = val2.sqrMagnitude;
				if (sqrMagnitude < num3)
				{
					num3 = sqrMagnitude;
					num4 = num5;
				}
			}
		}
		if (num4 > -1)
		{
			normal = Markers[num4].LocalNormal;
			closestPoint = Markers[num4].LocalPosition;
			return true;
		}
		closestPoint = default;
		normal = default;
		return false;
	}

	public TreeMarkerData()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
	}
}

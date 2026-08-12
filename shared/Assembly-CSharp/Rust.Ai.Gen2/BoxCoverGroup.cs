using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class BoxCoverGroup : CoverGroup
{
	[SerializeField]
	private Vector3 size;

	private OBB obb;

	private static readonly (int x, int z)[] boxCorners = new(int, int)[4]
	{
		(1, 1),
		(1, -1),
		(-1, -1),
		(-1, 1)
	};

	public override void GenerateCovers(GameObject gameObject)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		covers.Clear();
		BaseEntity component = gameObject.GetComponent<BaseEntity>();
		MeshRenderer componentInChildren = gameObject.GetComponentInChildren<MeshRenderer>();
		if ((Object)(object)component != (Object)null)
		{
			Bounds bounds = component.bounds;
			obb = new OBB(gameObject.transform.position + ((Bounds)(ref bounds)).center, ((Bounds)(ref bounds)).size, gameObject.transform.rotation);
		}
		else if ((Object)(object)componentInChildren != (Object)null)
		{
			Vector3 position = gameObject.transform.position;
			Bounds localBounds = ((Renderer)componentInChildren).localBounds;
			Vector3 val = position + ((Bounds)(ref localBounds)).center * ((Component)componentInChildren).transform.lossyScale.x;
			localBounds = ((Renderer)componentInChildren).localBounds;
			obb = new OBB(val, ((Bounds)(ref localBounds)).size * ((Component)componentInChildren).transform.lossyScale.x, gameObject.transform.rotation);
		}
		else
		{
			obb = new OBB(gameObject.transform.position + size.y * 0.5f * Vector3.up, size, gameObject.transform.rotation);
		}
		isTall = obb.extents.y * 2f >= 1.8f;
		for (int i = 0; i < boxCorners.Length; i++)
		{
			(int, int) tuple = boxCorners[i];
			(int, int) tuple2 = boxCorners[(i + 1) % boxCorners.Length];
			Vector3 point = ((OBB)(ref obb)).GetPoint((float)tuple.Item1, -1f, (float)tuple.Item2);
			Vector3 point2 = ((OBB)(ref obb)).GetPoint((float)tuple2.Item1, -1f, (float)tuple2.Item2);
			Vector3 val2 = point2 - point;
			Vector3 normalized = ((Vector3)(ref val2)).normalized;
			point += normalized * 0.875f;
			point2 -= normalized * 0.875f;
			int num = Mathf.FloorToInt(Vector3.Distance(point, point2) / 1f);
			for (int j = 0; j < num; j++)
			{
				Cover.Peeks peeks = Cover.Peeks.None;
				if (!isTall)
				{
					peeks |= Cover.Peeks.Up;
				}
				else
				{
					if (j == 0)
					{
						peeks |= Cover.Peeks.Right;
					}
					if (j == num - 1)
					{
						peeks |= Cover.Peeks.Left;
					}
				}
				if (peeks != Cover.Peeks.None)
				{
					Vector3 val3 = Vector3.Lerp(point, point2, (float)(j / (num - 1)));
					Vector3 val4 = val3;
					val2 = Vector3.Cross(normalized, Vector3.up);
					val3 = val4 + ((Vector3)(ref val2)).normalized * 0.5f;
					Cover item = new Cover(val3, Mathf.Atan2(point2.x - point.x, point2.z - point.z) * 57.29578f + 90f, peeks);
					covers.Add(item);
				}
			}
		}
	}

	public override bool GetCovers(Transform transform, List<Cover> covers, Vector3 from)
	{
		covers.AddRange(base.covers);
		return base.covers.Count > 0;
	}

	public BoxCoverGroup()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		size = new Vector3(5f, 1.5f, 1f);
		base._002Ector();
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class PillarCoverGroup : CoverGroup
{
	[SerializeField]
	private float radius = 1f;

	[SerializeField]
	private float radiusOffset;

	private Vector3 position;

	private Quaternion rotation;

	public override void GenerateCovers(GameObject gameObject)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		position = gameObject.transform.position;
		rotation = gameObject.transform.rotation;
		BaseEntity component = gameObject.GetComponent<BaseEntity>();
		MeshRenderer componentInChildren = gameObject.GetComponentInChildren<MeshRenderer>();
		if ((Object)(object)component != (Object)null)
		{
			Bounds bounds = component.bounds;
			radius = Mathf.Max(((Bounds)(ref bounds)).extents.x, ((Bounds)(ref bounds)).extents.z) - radiusOffset;
			isTall = ((Bounds)(ref bounds)).size.y >= 1.8f;
		}
		else if ((Object)(object)componentInChildren != (Object)null)
		{
			Bounds localBounds = ((Renderer)componentInChildren).localBounds;
			Vector3 extents = ((Bounds)(ref localBounds)).extents;
			radius = ((Vector3)(ref extents)).magnitude * ((Component)componentInChildren).transform.lossyScale.x - radiusOffset;
			localBounds = ((Renderer)componentInChildren).localBounds;
			isTall = ((Bounds)(ref localBounds)).size.y * ((Component)componentInChildren).transform.lossyScale.y >= 1.8f;
		}
	}

	public override bool GetCovers(Transform transform, List<Cover> covers, Vector3 from)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = from - position;
		Cover.Peeks peeks = (isTall ? Cover.Peeks.Sides : Cover.Peeks.Up);
		if (rotation == Quaternion.identity)
		{
			float yaw = Mathf.Atan2(val.x, val.z) * 57.29578f;
			Vector3 val2 = position + -Vector3Ex.NormalizeXZ(val) * (radius + 0.5f);
			covers.Add(new Cover(val2, yaw, peeks));
		}
		else
		{
			Vector3 val3 = rotation * Vector3.forward;
			Vector3 val4 = rotation * Vector3.right;
			Vector3 val5 = default(Vector3);
			((Vector3)(ref val5))._002Ector(Vector3.Dot(val, val4), 0f, Vector3.Dot(val, val3));
			float yaw2 = Mathf.Atan2(val.x, val.z) * 57.29578f;
			Vector3 val6 = -((Vector3)(ref val5)).normalized * (radius + 0.5f);
			Vector3 val7 = position + val4 * val6.x + val3 * val6.z;
			covers.Add(new Cover(val7, yaw2, peeks));
		}
		return true;
	}
}

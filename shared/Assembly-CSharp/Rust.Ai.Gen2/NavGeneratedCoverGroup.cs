using System.Collections.Generic;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class NavGeneratedCoverGroup : CoverGroup
{
	[SerializeField]
	public List<Cover> cachedCovers = new List<Cover>();

	public override bool IsSlow => true;

	public override bool GetCovers(Transform transform, List<Cover> covers, Vector3 from)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (cachedCovers.Count == 0)
		{
			return false;
		}
		foreach (Cover cachedCover in cachedCovers)
		{
			Cover item = cachedCover;
			item.position = transform.TransformPoint(cachedCover.position);
			float yaw = cachedCover.yaw;
			Quaternion rotation = transform.rotation;
			item.yaw = yaw + ((Quaternion)(ref rotation)).eulerAngles.y;
			covers.Add(item);
		}
		return true;
	}
}

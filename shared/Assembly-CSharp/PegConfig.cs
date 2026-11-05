using System;
using UnityEngine;

[Serializable]
public class PegConfig
{
	public enum PegType
	{
		Small,
		Large
	}

	public PegType Type;

	public Vector3 VerticalMountLocalRotation;

	public Vector3 VerticalMountLocalOffset;

	public void Init(PegType t, Vector3 localRot, Vector3 localOffset)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Type = t;
		VerticalMountLocalRotation = localRot;
		VerticalMountLocalOffset = localOffset;
	}

	public void GetLocalPose(Transform item, WorldModelRackMountConfig config, out Vector3 position, out Quaternion rotation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		position = item.localPosition + item.localRotation * Vector3.Scale(item.localScale, VerticalMountLocalOffset);
		rotation = item.localRotation * Quaternion.Inverse(Quaternion.Euler(config.VerticalMountLocalRotation)) * Quaternion.Euler(VerticalMountLocalRotation);
	}
}

using System;
using UnityEngine;

public class EnvironmentVolumeCheck : PrefabAttribute
{
	[InspectorFlags]
	public EnvironmentType Type;

	public Vector3 Center;

	public Vector3 Size;

	protected void OnDrawGizmosSelected()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.matrix = ((Component)this).transform.localToWorldMatrix;
		Gizmos.color = new Color(0f, 0f, 0.5f, 1f);
		Gizmos.DrawWireCube(Center, Size);
	}

	public bool Check(OBB obb)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return !EnvironmentManager.Check(obb, Type);
	}

	protected override Type GetIndexedType()
	{
		return typeof(EnvironmentVolumeCheck);
	}

	public EnvironmentVolumeCheck()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Type = EnvironmentType.Underground | EnvironmentType.TrainTunnels;
		Center = Vector3.zero;
		Size = Vector3.one;
		base._002Ector();
	}
}

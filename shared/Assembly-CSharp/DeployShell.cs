using System;
using UnityEngine;

public class DeployShell : PrefabAttribute
{
	public Bounds bounds;

	public OBB WorldSpaceBounds(Transform transform)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return new OBB(transform.position, transform.lossyScale, transform.rotation, bounds);
	}

	public float LineOfSightPadding()
	{
		return 0.025f;
	}

	protected override Type GetIndexedType()
	{
		return typeof(DeployShell);
	}

	public DeployShell()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		bounds = new Bounds(Vector3.zero, Vector3.one);
		base._002Ector();
	}
}

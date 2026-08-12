using System;
using UnityEngine;

[Serializable]
public class PowerLineWireConnection
{
	public Vector3 inOffset;

	public Vector3 outOffset;

	public float radius;

	public Transform start;

	public Transform end;

	public PowerLineWireConnection()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		inOffset = Vector3.zero;
		outOffset = Vector3.zero;
		radius = 0.01f;
		base._002Ector();
	}
}

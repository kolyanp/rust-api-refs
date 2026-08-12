using System;
using UnityEngine;

[Serializable]
public class PowerLineWireConnectionDef
{
	public Vector3 inOffset;

	public Vector3 outOffset;

	public float radius;

	public bool hidden;

	public PowerLineWireConnectionDef()
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

	public PowerLineWireConnectionDef(PowerLineWireConnectionDef src)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		inOffset = Vector3.zero;
		outOffset = Vector3.zero;
		radius = 0.01f;
		base._002Ector();
		inOffset = src.inOffset;
		outOffset = src.outOffset;
		radius = src.radius;
	}
}

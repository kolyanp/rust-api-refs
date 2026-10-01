using System;
using UnityEngine;

public class BlockFaceDefinition : PrefabAttribute
{
	public BlockEdgeDefinition[] edges;

	public Vector3 NormalDirection;

	public bool SealsRoom = true;

	[NonSerialized]
	public Vector3 LocalCenter;

	[NonSerialized]
	public Vector3 LocalNormal;

	public Vector3[] DebugPoints;

	protected override Type GetIndexedType()
	{
		return typeof(BlockFaceDefinition);
	}

	public override void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		base.PreProcess(preProcess, rootObj, name, serverside, clientside, bundling);
		LocalCenter = rootObj.transform.InverseTransformPoint(((Component)this).transform.position);
		LocalNormal = NormalDirection.normalized;
	}
}

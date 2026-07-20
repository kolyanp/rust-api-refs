using System;
using UnityEngine;

public class VisualFoodSettings : PrefabAttribute, IClientComponent
{
	[Serializable]
	public class VisualFoodSetting
	{
		public GameObjectRef model;

		public GameObjectRef effectPrefab;

		public ItemDefinition[] items;

		public Transform[] parents;

		public Transform[] effects;

		[HideInInspector]
		public Vector3[] parentPositions;

		[HideInInspector]
		public Quaternion[] parentRotations;

		[HideInInspector]
		public Vector3[] parentScales;

		[HideInInspector]
		public Vector3[] effectParentPositions;

		[HideInInspector]
		public Quaternion[] effectParentRotations;

		[HideInInspector]
		public Vector3[] effectParentScales;

		public void ProcessSpawnPos(Transform rootTransform)
		{
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			if (parents != null && parents.Length != 0)
			{
				parentPositions = (Vector3[])(object)new Vector3[parents.Length];
				parentRotations = (Quaternion[])(object)new Quaternion[parents.Length];
				parentScales = (Vector3[])(object)new Vector3[parents.Length];
				for (int i = 0; i < parents.Length; i++)
				{
					if ((Object)(object)parents[i] != (Object)null)
					{
						parentPositions[i] = parents[i].position;
						parentRotations[i] = parents[i].rotation;
						parentScales[i] = parents[i].localScale;
					}
				}
			}
			if (effects == null || effects.Length == 0)
			{
				return;
			}
			effectParentPositions = (Vector3[])(object)new Vector3[effects.Length];
			effectParentRotations = (Quaternion[])(object)new Quaternion[effects.Length];
			effectParentScales = (Vector3[])(object)new Vector3[effects.Length];
			for (int j = 0; j < effects.Length; j++)
			{
				if ((Object)(object)effects[j] != (Object)null)
				{
					effectParentPositions[j] = effects[j].position;
					effectParentRotations[j] = effects[j].rotation;
					effectParentScales[j] = effects[j].localScale;
				}
			}
		}
	}

	public Transform strippedParent;

	public VisualFoodSetting[] settings;

	public GameObjectRef[] slotPanModels;

	public Transform[] slotPanParents;

	[HideInInspector]
	public Vector3[] slotPanPositions;

	[HideInInspector]
	public Quaternion[] slotPanRotations;

	[HideInInspector]
	public Vector3[] slotPanScales;

	protected override Type GetIndexedType()
	{
		return typeof(VisualFoodSettings);
	}
}

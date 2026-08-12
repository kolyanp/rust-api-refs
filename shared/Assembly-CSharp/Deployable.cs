using System;
using UnityEngine;

public class Deployable : PrefabAttribute
{
	public GameObjectRef guidePrefab;

	public Mesh guideMesh;

	public Vector3 guideMeshScale;

	public bool overrideRotation;

	public Vector3 guideMeshOrientation;

	public Vector3 guideMeshPositionOffset;

	[Tooltip("Moves the deploy guide towards the camera by 0.05 units, to avoid clipping. Sometimes you want that off to avoid that little offset in betweenthe deploy guide position and the actual position the deployable is going to spawn at.")]
	public bool moveGuideTowardsCamera;

	public bool guideLights;

	public bool wantsInstanceData;

	public bool copyInventoryFromItem;

	public bool setSocketParent;

	public bool toSlot;

	public BaseEntity.Slot slot;

	public GameObjectRef placeEffect;

	[Tooltip("Only required if the guideMesh is in a significantly different position or there are multiple meshes")]
	public Transform[] guideTargets;

	[NonSerialized]
	public Bounds bounds;

	protected override void AttributeSetup(GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		base.AttributeSetup(rootObj, name, serverside, clientside, bundling);
		bounds = rootObj.GetComponent<BaseEntity>().bounds;
	}

	protected override Type GetIndexedType()
	{
		return typeof(Deployable);
	}

	public bool IsGuideTarget(Transform t)
	{
		if (guideTargets != null)
		{
			Transform[] array = guideTargets;
			for (int i = 0; i < array.Length; i++)
			{
				if ((Object)(object)array[i] == (Object)(object)t)
				{
					return true;
				}
			}
		}
		return false;
	}

	public Deployable()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		guideMeshScale = Vector3.one;
		guideMeshOrientation = Vector3.zero;
		guideMeshPositionOffset = Vector3.zero;
		moveGuideTowardsCamera = true;
		guideLights = true;
		base._002Ector();
	}
}

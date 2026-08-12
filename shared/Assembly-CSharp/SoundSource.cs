using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundSource : MonoBehaviour, IClientComponentEx, ILOD
{
	[Serializable]
	public class OcclusionPoint
	{
		public Vector3 offset;

		public bool isOccluded;

		public OcclusionPoint()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			offset = Vector3.zero;
			base._002Ector();
		}
	}

	[Header("Occlusion")]
	public bool handleOcclusionChecks;

	public LayerMask occlusionLayerMask;

	public List<OcclusionPoint> occlusionPoints = new List<OcclusionPoint>();

	public bool isOccluded;

	public float occlusionAmount;

	public float lodDistance = 100f;

	public bool inRange;

	public virtual void PreClientComponentCull(IPrefabProcessor p)
	{
		p.RemoveComponent((Component)(object)this);
	}

	public bool IsSyncedToParent()
	{
		return false;
	}
}

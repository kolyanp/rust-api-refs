using System;
using UnityEngine;

public class TreadAnimator : MonoBehaviour, IClientComponent
{
	[Serializable]
	public struct TreadRenderer
	{
		public Renderer Renderer;

		public int leftMaterialIndex;

		public int rightMaterialIndex;
	}

	public Animator mainBodyAnimator;

	public Transform[] wheelBones;

	public Vector3[] vecShocksOffsetPosition;

	public Vector3[] wheelBoneOrigin;

	public float wheelBoneDistMax;

	public TreadRenderer[] treadRenderers;

	public TreadEffects treadEffects;

	public float traceThickness;

	public float heightFudge;

	public bool useWheelYOrigin;

	public Vector2 treadTextureDirection;

	public bool isMetallic;

	public float angularTreadConstant;

	public float treadConstant;

	public float wheelSpinConstant;

	public float traceLineMin;

	public float traceLineMax;

	public float maxShockDist;

	public TreadAnimator()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		wheelBoneDistMax = 0.26f;
		traceThickness = 0.25f;
		heightFudge = 0.13f;
		treadTextureDirection = new Vector2(1f, 0f);
		angularTreadConstant = 0.05f;
		treadConstant = 0.14f;
		wheelSpinConstant = 80f;
		traceLineMin = 0.55f;
		traceLineMax = 0.79f;
		maxShockDist = 0.26f;
		((MonoBehaviour)this)._002Ector();
	}
}

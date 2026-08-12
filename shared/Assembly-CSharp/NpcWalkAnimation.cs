using UnityEngine;

public class NpcWalkAnimation : MonoBehaviour, IClientComponent
{
	public Vector3 HipFudge;

	public BaseNpc Npc;

	public Animator Animator;

	public Transform HipBone;

	public Transform LookBone;

	public bool UpdateWalkSpeed;

	public bool UpdateFacingDirection;

	public bool UpdateGroundNormal;

	public Transform alignmentRoot;

	public bool LaggyAss;

	public bool LookAtTarget;

	public float MaxLaggyAssRotation;

	public float MaxWalkAnimSpeed;

	public bool UseDirectionBlending;

	public bool useTurnPosing;

	public float turnPoseScale;

	public float laggyAssLerpScale;

	public bool skeletonChainInverted;

	public NpcWalkAnimation()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		HipFudge = new Vector3(-90f, 0f, 90f);
		UpdateWalkSpeed = true;
		UpdateFacingDirection = true;
		UpdateGroundNormal = true;
		LaggyAss = true;
		MaxLaggyAssRotation = 70f;
		MaxWalkAnimSpeed = 25f;
		turnPoseScale = 0.5f;
		laggyAssLerpScale = 15f;
		((MonoBehaviour)this)._002Ector();
	}
}

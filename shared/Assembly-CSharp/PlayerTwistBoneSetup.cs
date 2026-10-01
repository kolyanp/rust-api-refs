using System;
using UnityEngine;

[Serializable]
public class PlayerTwistBoneSetup
{
	public Transform parentBone;

	public Transform childBone;

	public float weight;

	public Vector3 childRollAxis;

	public Quaternion childBindRotation;
}

using System;
using UnityEngine;

public class WearableHolsterOffset : MonoBehaviour
{
	[Serializable]
	public class offsetInfo
	{
		public enum OverrideBone
		{
			None,
			Spine4,
			Custom
		}

		public HeldEntity.HolsterInfo.HolsterSlot type;

		public Vector3 offset;

		public Vector3 rotationOffset;

		public int priority;

		public Vector3 rightBackpackSlotOffset;

		public Vector3 rightBackpackSlotRotationOffset;

		public OverrideBone overrideBone;

		[Tooltip("Bone added by this wearable's BoneRetargets, offset and rotationOffset then place the regular spine3 holster position relative to this bone")]
		public string customBoneName;
	}

	public offsetInfo[] Offsets;
}

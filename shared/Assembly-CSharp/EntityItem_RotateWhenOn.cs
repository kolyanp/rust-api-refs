using System;
using UnityEngine;

public class EntityItem_RotateWhenOn : EntityComponent<BaseEntity>
{
	[Serializable]
	public class State
	{
		public Vector3 position;

		public Vector3 rotation;

		public float initialDelay;

		public float timeToTake = 2f;

		public AnimationCurve animationCurve = new AnimationCurve(new Keyframe[2]
		{
			new Keyframe(0f, 0f),
			new Keyframe(1f, 1f)
		});

		public string effectOnStart = "";

		public string effectOnFinish = "";

		public SoundDefinition movementLoop;

		public float movementLoopFadeOutTime = 0.1f;

		public SoundDefinition startSound;

		public SoundDefinition stopSound;

		public State()
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Expected Obj, but got Unknown
		}
	}

	public State on;

	public State off;

	public bool usePosition;

	internal bool currentlyOn;

	internal bool stateInitialized;

	public BaseEntity.Flags targetFlag = BaseEntity.Flags.On;
}

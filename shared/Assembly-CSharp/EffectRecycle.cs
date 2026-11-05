using UnityEngine;
using UnityEngine.Serialization;

public class EffectRecycle : BaseMonoBehaviour, IClientComponent, IRagdollInhert, IEffectRecycle, IFlattenKeepsDescendants
{
	public enum PlayMode
	{
		Once,
		Looped
	}

	public enum ParentDestroyBehaviour
	{
		Detach,
		Destroy,
		DetachWaitDestroy
	}

	[FormerlySerializedAs("lifeTime")]
	public float detachTime;

	[FormerlySerializedAs("lifeTime")]
	public float recycleTime;

	[Tooltip("Will replay this effect in place instead of popping and retiring a pooled copy every time")]
	public bool replayInPlace;

	[Tooltip("Recycle this effect before the tail sound finishes, they'll keep playing on their own after this effect is recycled")]
	public bool recycleBeforeSoundsFinish;

	public PlayMode playMode;

	public ParentDestroyBehaviour onParentDestroyed;

	[Range(1f, 50f)]
	public int WarmedUpAmount = 1;
}

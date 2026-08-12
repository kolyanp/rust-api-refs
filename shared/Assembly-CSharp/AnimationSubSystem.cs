using System;
using UnityEngine;

public abstract class AnimationSubSystem : MonoBehaviour, IClientComponent
{
	public enum PriorityLevels
	{
		Low,
		Medium,
		High
	}

	public class SubSystemVariable : Attribute
	{
	}

	public const int TotalPriorityLevels = 3;

	[SubSystemVariable]
	[Tooltip("Used for debugging, should describe what this system does")]
	[SerializeField]
	private string SubSystemName = string.Empty;

	[Tooltip("Controls what part of the body this system modifies, if none full body mask will be used")]
	[SerializeField]
	[SubSystemVariable]
	protected AvatarMask Mask;

	[SerializeField]
	protected float FadeInTime = 0.25f;

	[SerializeField]
	protected float FadeOutTime = 0.25f;

	[SerializeField]
	protected bool DisableDuringGestures;

	[SerializeField]
	protected bool DisableSpineIK;

	[SubSystemVariable]
	[SerializeField]
	private PriorityLevels Priority = PriorityLevels.Medium;
}

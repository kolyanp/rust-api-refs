using UnityEngine;

public class HapticPulseEffect : MonoBehaviour, IEffect
{
	public enum Ownership : byte
	{
		NearLocalPlayer = 1,
		Always
	}

	public HapticSource source = HapticSource.Effects;

	public HapticPreset preset = HapticPreset.Thump;

	[Range(0f, 2f)]
	public float scale = 1f;

	public Ownership ownership = Ownership.NearLocalPlayer;

	[Header("Near Local Player")]
	public float maxDistance = 8f;

	public AnimationCurve falloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);
}

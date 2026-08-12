using UnityEngine;

public class SoundPlayer : BaseMonoBehaviour, IClientComponent
{
	public SoundDefinition soundDefinition;

	public bool playImmediately;

	public float minStartDelay;

	public float maxStartDelay;

	public bool debugRepeat;

	public bool pending;

	public Vector3 soundOffset;

	public SoundPlayer()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		playImmediately = true;
		soundOffset = Vector3.zero;
		base._002Ector();
	}
}

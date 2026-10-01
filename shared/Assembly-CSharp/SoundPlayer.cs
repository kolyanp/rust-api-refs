using UnityEngine;

public class SoundPlayer : BaseMonoBehaviour, IClientComponent
{
	public SoundDefinition soundDefinition;

	public bool playImmediately = true;

	public float minStartDelay;

	public float maxStartDelay;

	public bool debugRepeat;

	public bool pending;

	public Vector3 soundOffset = Vector3.zero;

	public SoundPlayer()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
	}
}

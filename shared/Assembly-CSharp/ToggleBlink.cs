using UnityEngine;
using UnityEngine.Events;

public class ToggleBlink : FacepunchBehaviour, IClientComponent, INotifyLOD
{
	public enum BlinkState
	{
		Off,
		On,
		Blinking
	}

	[SerializeField]
	private UnityEvent onEnabled;

	[SerializeField]
	private UnityEvent onDisabled;

	public BlinkState initialState;

	public float blinkDuration;

	public ToggleBlink()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		onEnabled = new UnityEvent();
		onDisabled = new UnityEvent();
		blinkDuration = 1f;
		base._002Ector();
	}
}

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
	private UnityEvent onEnabled = new UnityEvent();

	[SerializeField]
	private UnityEvent onDisabled = new UnityEvent();

	public BlinkState initialState;

	public float blinkDuration = 1f;

	public ToggleBlink()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
	}
}

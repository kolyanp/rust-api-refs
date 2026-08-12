using UnityEngine;
using UnityEngine.Events;

public class SkinViewerEvents : MonoBehaviour
{
	public UnityEvent OnEnteredFullScreenEvent;

	public UnityEvent OnExitFullScreenEvent;

	[UnityEvent]
	public void OnEnteredFullScreen()
	{
		UnityEvent onEnteredFullScreenEvent = OnEnteredFullScreenEvent;
		if (onEnteredFullScreenEvent != null)
		{
			onEnteredFullScreenEvent.Invoke();
		}
	}

	[UnityEvent]
	public void OnExitFullScreen()
	{
		UnityEvent onExitFullScreenEvent = OnExitFullScreenEvent;
		if (onExitFullScreenEvent != null)
		{
			onExitFullScreenEvent.Invoke();
		}
	}

	public SkinViewerEvents()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		OnEnteredFullScreenEvent = new UnityEvent();
		OnExitFullScreenEvent = new UnityEvent();
		((MonoBehaviour)this)._002Ector();
	}
}

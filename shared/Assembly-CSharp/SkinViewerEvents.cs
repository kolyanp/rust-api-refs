using UnityEngine;
using UnityEngine.Events;

public class SkinViewerEvents : MonoBehaviour
{
	public UnityEvent OnEnteredFullScreenEvent = new UnityEvent();

	public UnityEvent OnExitFullScreenEvent = new UnityEvent();

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
}

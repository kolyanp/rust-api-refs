using UnityEngine;

public class AnimationEventForward : MonoBehaviour
{
	public GameObject targetObject;

	[UnityEvent]
	public void Event(string type)
	{
		targetObject.SendMessage(type);
	}
}

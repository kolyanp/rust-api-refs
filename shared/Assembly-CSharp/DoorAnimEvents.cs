using UnityEngine;

public class DoorAnimEvents : MonoBehaviour
{
	public GameObjectRef openStart;

	public GameObjectRef openEnd;

	public GameObjectRef closeStart;

	public GameObjectRef closeEnd;

	public GameObject soundTarget;

	public bool checkAnimSpeed;

	private Animator _animator;

	public Animator animator
	{
		get
		{
			if (_animator == null)
			{
				_animator = ((Component)this).GetComponent<Animator>();
			}
			return _animator;
		}
	}

	[UnityEvent]
	public void DoorOpenStart()
	{
	}

	[UnityEvent]
	public void DoorOpenEnd()
	{
	}

	[UnityEvent]
	public void DoorCloseStart()
	{
	}

	[UnityEvent]
	public void DoorCloseEnd()
	{
	}
}

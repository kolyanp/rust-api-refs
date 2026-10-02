using UnityEngine;

public class DoorWithLock : Door
{
	[ItemSelector]
	[Header("Lock Settings")]
	public GameObjectRef lockObject;
}

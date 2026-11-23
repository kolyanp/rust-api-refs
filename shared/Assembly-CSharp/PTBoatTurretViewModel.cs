using UnityEngine;

public class PTBoatTurretViewModel : MountedViewModel
{
	[SerializeField]
	[Tooltip("The viewmodel bone carrying the sights, the gunner's eye rides it while the gun kicks so the sights stay lined up")]
	private string sightsBone;
}

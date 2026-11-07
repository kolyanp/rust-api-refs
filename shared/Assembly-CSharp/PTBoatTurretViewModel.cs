using UnityEngine;

public class PTBoatTurretViewModel : MountedViewModel
{
	[Tooltip("The viewmodel bone carrying the sights, the gunner's eye rides it while the gun kicks so the sights stay lined up")]
	[SerializeField]
	private string sightsBone;
}

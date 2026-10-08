using UnityEngine;

public class MountedWeaponViewmodel : BaseViewModel
{
	[SerializeField]
	[Header("Mounted Weapon Viewmodel")]
	private bool _invertForward;

	[SerializeField]
	private Vector3 _positionOffset;
}

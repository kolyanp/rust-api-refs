using UnityEngine;

public class RHIBVisuals : FacepunchBehaviour, INotifyLOD, IClientComponent
{
	[Header("RHIB")]
	[SerializeField]
	private RHIB _owner;

	[SerializeField]
	[Header("References")]
	private Transform _compass;

	[Header("References - Screens")]
	[SerializeField]
	private RHIBScreen[] _mapScreens;
}

using UnityEngine;

public class RHIBVisuals : FacepunchBehaviour, INotifyLOD, IClientComponent
{
	[SerializeField]
	[Header("RHIB")]
	private RHIB _owner;

	[Header("References")]
	[SerializeField]
	private Transform _compass;

	[Header("References - Screens")]
	[SerializeField]
	private RHIBScreen[] _mapScreens;
}

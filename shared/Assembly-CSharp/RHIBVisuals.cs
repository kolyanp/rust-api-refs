using UnityEngine;

public class RHIBVisuals : FacepunchBehaviour, INotifyLOD, IClientComponent
{
	[Header("RHIB")]
	[SerializeField]
	private RHIB _owner;

	[Header("References")]
	[SerializeField]
	private Transform _compass;

	[SerializeField]
	[Header("References - Screens")]
	private RHIBScreen[] _mapScreens;
}

using UnityEngine;

public class LookAtFarmableAnimal : MonoBehaviour
{
	public CanvasGroup Group;

	public ChickenCoopStatusWidget Status;

	public LivestockAnimalStatusWidget LivestockStatus;

	[Tooltip("The plate behind the chicken panel, sized to it. The livestock panel carries a plate per section instead, so this is turned off while that one is up.")]
	public RectTransform Backing;
}

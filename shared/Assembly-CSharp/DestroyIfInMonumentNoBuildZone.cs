using UnityEngine;

public class DestroyIfInMonumentNoBuildZone : MonoBehaviour
{
	protected void Start()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (ConstructionErrors.IsBuildBlockedByMonument(((Component)this).transform.position))
		{
			GameManager.Destroy(((Component)this).gameObject);
		}
	}
}

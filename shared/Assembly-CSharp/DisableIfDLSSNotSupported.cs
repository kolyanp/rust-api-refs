using UnityEngine;

public class DisableIfDLSSNotSupported : MonoBehaviour
{
	private void OnEnable()
	{
		((Component)this).gameObject.SetActive(false);
	}
}

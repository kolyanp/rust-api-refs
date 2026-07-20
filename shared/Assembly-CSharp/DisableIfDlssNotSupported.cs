using UnityEngine;

public class DisableIfDlssNotSupported : MonoBehaviour
{
	private void OnEnable()
	{
		((Component)this).gameObject.SetActive(false);
	}
}

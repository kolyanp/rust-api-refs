using UnityEngine;

public class DisableIfNvidiaReflexNotSupported : MonoBehaviour
{
	public GameObject reflexModeOption;

	public GameObject reflexLatencyMarkerOption;

	private void OnEnable()
	{
		reflexModeOption.SetActive(false);
		reflexLatencyMarkerOption.SetActive(false);
	}
}

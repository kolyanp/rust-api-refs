using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class StatusPanel : MonoBehaviour
{
	[SerializeField]
	[Header("On Status")]
	private RustText onStatusText;

	[SerializeField]
	private Image onStatusImage;

	[Header("Off Status")]
	[SerializeField]
	private RustText offStatusText;

	[SerializeField]
	private Image offStatusImage;

	public void SetStatus(bool status)
	{
		((Component)onStatusText).gameObject.SetActive(status);
		((Component)offStatusText).gameObject.SetActive(!status);
		((Component)onStatusImage).gameObject.SetActive(status);
		((Component)offStatusImage).gameObject.SetActive(!status);
	}
}

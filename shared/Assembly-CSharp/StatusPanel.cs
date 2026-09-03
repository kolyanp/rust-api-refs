using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class StatusPanel : MonoBehaviour
{
	[Header("On Status")]
	[SerializeField]
	private RustText onStatusText;

	[SerializeField]
	private Image onStatusImage;

	[SerializeField]
	[Header("Off Status")]
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

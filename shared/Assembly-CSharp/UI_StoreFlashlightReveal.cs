using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UI_StoreFlashlightReveal : MonoBehaviour
{
	[SerializeField]
	private MaskableGraphic targetGraphic;

	[SerializeField]
	private Material flashlightMaterial;

	[SerializeField]
	[Space]
	private string imageFolder;

	[SerializeField]
	private string lightSuffix;

	[SerializeField]
	private string imageExtension;

	[SerializeField]
	[Space]
	private float fadeSpeed;
}

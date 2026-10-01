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
	private float fadeSpeed;
}

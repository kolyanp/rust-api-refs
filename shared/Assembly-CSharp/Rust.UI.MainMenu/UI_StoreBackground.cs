using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_StoreBackground : BaseMonoBehaviour
{
	public RectTransform viewport;

	public RectTransform section;

	public Image backgroundImage;

	public DynamicResourceRef<Sprite> DynamicImage;

	public string ImageUrl;

	public string ImageBlurHash;

	public float fadeRange = 400f;

	public float fadeSpeed = 5f;
}

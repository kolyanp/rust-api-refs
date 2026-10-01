using System;
using Facepunch.Flexbox;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_StoreItemOverlayPage : UI_Window
{
	[Serializable]
	public struct PageElement
	{
		public Phrase Name;

		[ItemSelector]
		public ItemDefinition Item;

		public bool isVideo;

		public string videoURL;

		public DynamicResourceRef<Sprite> DynamicFullscreenSprite;

		public DynamicResourceRef<Sprite> DynamicGallerySprite;

		public string FullscreenSpriteUrl;

		public string GallerySpriteUrl;

		public string FullscreenSpriteBlurHash;

		public string GallerySpriteBlurHash;

		public DynamicResourceRef<Sprite> DynamicFullscreenLightSprite;

		public string FullscreenLightSpriteUrl;

		public string FullscreenLightSpriteBlurHash;

		[Min(0f)]
		public int VariantCount;

		public bool overrideItem;

		public Phrase ItemName;

		public Sprite ItemIcon;

		public bool UseSkinViewer;
	}

	[Serializable]
	public struct PageContent
	{
		public PageElement[] Elements;
	}

	[Space]
	[Header("Page Content")]
	[SerializeField]
	private CanvasGroup bodyCanvasGroup;

	[SerializeField]
	private FlexTransition crossFadeTransition;

	[SerializeField]
	private CoverVideo coverVideo;

	[SerializeField]
	private CoverImage coverImage;

	[SerializeField]
	private HttpImage httpImage;

	[SerializeField]
	private UI_BackgroundAspectRatioFitter coverBackground;

	[SerializeField]
	private Canvas backButtonCanvas;

	[SerializeField]
	private GameObject textContainerGroup;

	[SerializeField]
	private RustText titleText;

	[SerializeField]
	private GameObject itemGroup;

	[SerializeField]
	private RustText itemNameText;

	[SerializeField]
	private Image itemIconImage;

	[SerializeField]
	private GameObject variantGroup;

	[SerializeField]
	private RustText variantCoutText;

	[SerializeField]
	private UI_StoreFlashlightReveal flashlightReveal;

	[Header("Gallery")]
	public DynamicResourceRef<SpriteAtlas> DynamicSmallAtlas;

	[SerializeField]
	private Transform galleryParent;

	[SerializeField]
	private UI_StoreCarrouselButton carouselButtonPrefab;

	[SerializeField]
	private Canvas galleryCanvas;

	[SerializeField]
	private CanvasGroup arrowButtons;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private CanvasGroup leftArrow;

	[SerializeField]
	private CanvasGroup rightArrow;

	[SerializeField]
	private UI_StoreAddCartButton cartButton;

	[SerializeField]
	private GameObject ownedButton;

	[SerializeField]
	[Space]
	private bool autoCycleEnabled = true;

	[SerializeField]
	private float autoCycleInterval = 10f;

	[SerializeField]
	[Header("Skin Viewer")]
	private UI_SkinViewerControls skinViewerControls;

	[SerializeField]
	private CoverImage skinViewerImage;

	[Space]
	[SerializeField]
	private PageContent pageContent;

	public bool HasFlashlightReveal => (Object)(object)flashlightReveal != (Object)null;
}

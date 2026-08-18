using System;
using Facepunch.Flexbox;
using Facepunch.Models;
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

		public Sprite FullscreenSprite;

		public Sprite GallerySprite;

		[NonSerialized]
		public string FullscreenImageUrl;

		[NonSerialized]
		public string GalleryImageUrl;

		[Min(0f)]
		public int VariantCount;

		public bool overrideItem;

		public Phrase ItemName;

		public Sprite ItemIcon;

		public bool UseSkinViewer;

		public Phrase GetTitle()
		{
			if (Name != null && !string.IsNullOrEmpty(Name.english))
			{
				return Name;
			}
			if ((Object)(object)Item != (Object)null)
			{
				return Item.displayName;
			}
			return Phrase.op_Implicit(string.Empty);
		}

		public Phrase GetRedirectItemName()
		{
			if ((Object)(object)Item != (Object)null && (Object)(object)Item.isRedirectOf != (Object)null && !Item.isRedirectOf.hidden)
			{
				return Item.isRedirectOf.displayName;
			}
			if (overrideItem)
			{
				return ItemName;
			}
			return null;
		}

		public Sprite GetRedirectItemIcon()
		{
			if ((Object)(object)Item != (Object)null && (Object)(object)Item.isRedirectOf != (Object)null && !Item.isRedirectOf.hidden)
			{
				return Item.isRedirectOf.iconSprite;
			}
			if (overrideItem)
			{
				return ItemIcon;
			}
			return null;
		}

		public PageElement OverrideWith(ElementOverride overrideElement)
		{
			PageElement result = this;
			if (!string.IsNullOrEmpty(overrideElement.Name))
			{
				result.Name = Phrase.op_Implicit(overrideElement.Name);
			}
			if (!string.IsNullOrEmpty(overrideElement.ItemShortname))
			{
				result.Item = ItemManager.FindItemDefinition(overrideElement.ItemShortname);
			}
			if (!string.IsNullOrEmpty(overrideElement.ImageUrl))
			{
				result.FullscreenImageUrl = overrideElement.ImageUrl;
			}
			if (!string.IsNullOrEmpty(overrideElement.GalleryImageUrl))
			{
				result.GalleryImageUrl = overrideElement.GalleryImageUrl;
			}
			if (!string.IsNullOrEmpty(overrideElement.VideoUrl))
			{
				result.videoURL = overrideElement.VideoUrl;
				result.isVideo = true;
			}
			if (overrideElement.VariantCount > 0)
			{
				result.VariantCount = overrideElement.VariantCount;
			}
			result.UseSkinViewer = overrideElement.UseSkinViewer;
			return result;
		}
	}

	[Serializable]
	public struct PageContent
	{
		public PageElement[] Elements;
	}

	[SerializeField]
	[Header("Page Content")]
	[Space]
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

	[Header("Gallery")]
	public SpriteAtlas SmallAtlas;

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

	[Header("Skin Viewer")]
	[SerializeField]
	private UI_SkinViewerControls skinViewerControls;

	[SerializeField]
	private CoverImage skinViewerImage;

	[Space]
	[SerializeField]
	private PageContent pageContent;
}

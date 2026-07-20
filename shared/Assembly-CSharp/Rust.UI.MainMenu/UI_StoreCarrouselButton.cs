using System;
using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_StoreCarrouselButton : MonoBehaviour
{
	public RustButton button;

	[Space]
	public RustText titleText;

	public RustText subtitleText;

	public CoverImage coverImage;

	public GameObject videoIcon;

	[Space]
	public GameObject gaugeParent;

	public Image gaugeImage;

	public GameObject variantGroup;

	public RustText variantText;

	[Space]
	public GameObject skinViewerGroup;

	public HttpImage httpImage;

	public void UpdateGauge(float fillAmount)
	{
		if (fillAmount != 0f && !gaugeParent.activeInHierarchy)
		{
			gaugeParent.SetActive(true);
		}
		else if (fillAmount == 0f && gaugeParent.activeInHierarchy)
		{
			gaugeParent.SetActive(false);
		}
		gaugeImage.fillAmount = fillAmount;
	}

	public void Init(UI_StoreItemOverlayPage.PageElement element, UI_StoreItemOverlayPage page)
	{
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		if (!element.UseSkinViewer)
		{
			if (element.Name != null && !string.IsNullOrEmpty(element.Name.english))
			{
				titleText.SetPhrase(element.Name, Array.Empty<object>());
			}
			else if ((Object)(object)element.Item != (Object)null)
			{
				titleText.SetPhrase(element.Item.displayName, Array.Empty<object>());
			}
		}
		skinViewerGroup.SetActive(element.UseSkinViewer);
		videoIcon.SetActive(element.isVideo);
		string text = element.GalleryImageUrl ?? element.FullscreenImageUrl;
		if (!string.IsNullOrEmpty(text) && (Object)(object)httpImage != (Object)null)
		{
			((Behaviour)httpImage).enabled = true;
			httpImage.Load(text);
		}
		else
		{
			((Behaviour)httpImage).enabled = false;
			coverImage.texture = null;
			Sprite val = (((Object)(object)element.GallerySprite == (Object)null) ? element.FullscreenSprite : element.GallerySprite);
			Sprite val2 = null;
			if ((Object)(object)val != (Object)null && (Object)(object)page.SmallAtlas != (Object)null)
			{
				Rect rect = val.rect;
				if (((Rect)(ref rect)).width > 512f)
				{
					rect = val.rect;
					if (((Rect)(ref rect)).height > 512f)
					{
						val2 = page.SmallAtlas.GetSprite(((Object)val).name);
					}
				}
			}
			coverImage.sprite = (((Object)(object)val2 != (Object)null) ? val2 : val);
		}
		variantGroup.SetActive(element.VariantCount > 0);
		variantText.SetText(element.VariantCount.ToString());
	}

	public void Init(string imageURL)
	{
		if ((Object)(object)httpImage != (Object)null)
		{
			httpImage.Load(imageURL);
		}
	}
}

using System;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_StoreItemDLC : MonoBehaviour
{
	public int appID;

	[NonSerialized]
	public UI_StoreItemOverlayPage OverlayPagePrefab;

	public DynamicResourceRef<UI_StoreItemOverlayPage> DynamicOverlayPagePrefab;

	public DynamicResourceRef<Sprite> DynamicIcon;

	public string IconUrl;

	public string IconBlurHash;

	public UI_StoreAddCartButton cartButton;

	private IPlayerItemDefinition _item;

	private HttpImage _coverImage;
}

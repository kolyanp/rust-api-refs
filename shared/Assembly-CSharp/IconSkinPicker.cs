using System;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class IconSkinPicker : MonoBehaviour
{
	public GameObjectRef pickerIcon;

	public GameObject container;

	public Action skinChangedEvent;

	public Action<AccessoryItem> charmChangedEvent;

	public ScrollRect scroller;

	public SearchFilterInput searchFilter;

	[Header("Charms")]
	public CharmPicker charmPicker;

	public Image currentCharmIcon;

	public GameObject noCharmIcon;

	[Space]
	public RustButton skinsButton;

	public RustButton charmsButton;

	public static Phrase defaultSkin = new Phrase("skin.default", "Default");

	public static Phrase randomSkin = new Phrase("skin.random", "Random");

	static IconSkinPicker()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
	}
}

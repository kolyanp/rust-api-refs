using UnityEngine;
using UnityEngine.UI;

public class LootPanelMixingTable : LootPanel, IInventoryChanged
{
	public GameObject controlsOn;

	public GameObject controlsOff;

	public Button StartMixingButton;

	public InfoBar ProgressBar;

	public GameObjectRef recipeItemPrefab;

	public RectTransform recipeContentRect;

	public ScrollRect ScrollView;

	public static readonly Phrase MixingPhrase;

	public static readonly Phrase CookingPhrase;

	static LootPanelMixingTable()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		MixingPhrase = new Phrase("mixingtable.mixing", "Mixing... {0} seconds remaining");
		CookingPhrase = new Phrase("cookingworkbench.cooking", "Cooking... {0} seconds remaining");
	}
}

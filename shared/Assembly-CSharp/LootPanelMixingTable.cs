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

	public static readonly Phrase MixingPhrase = new Phrase("mixingtable.mixing", "Mixing... {0} seconds remaining");

	public static readonly Phrase CookingPhrase = new Phrase("cookingworkbench.cooking", "Cooking... {0} seconds remaining");

	static LootPanelMixingTable()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
	}
}

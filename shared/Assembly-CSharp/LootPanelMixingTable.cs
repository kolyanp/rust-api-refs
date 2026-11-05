using TMPro;
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

	public TMP_InputField SearchField;

	public GameObjectRef sectionHeaderPrefab;

	public GameObjectRef sourceTogglePrefab;

	public RectTransform SourceToggleContainer;

	public Sprite AllSourcesIcon;

	public Sprite CraftableIcon;

	public static readonly Phrase MixingPhrase = new Phrase("mixingtable.mixing", "Mixing... {0} seconds remaining");

	public static readonly Phrase CookingPhrase = new Phrase("cookingworkbench.cooking", "Cooking... {0} seconds remaining");

	public static readonly Phrase RecipeNamePhrase = new Phrase("recipe.name", "Recipe Name");

	public static readonly Phrase AllSourcesPhrase = new Phrase("mixingtable.allsources", "All Recipes");

	public static readonly Phrase CraftablePhrase = new Phrase("mixingtable.craftable", "Craftable");

	static LootPanelMixingTable()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected Obj, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected Obj, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected Obj, but got Unknown
	}
}

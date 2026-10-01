using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(menuName = "Rust/Tutorials/Full Screen Help Info")]
public class TutorialFullScreenHelpInfo : ScriptableObject
{
	public enum MenuCategory
	{
		Movement,
		Crafting,
		Combat,
		Building
	}

	public static Phrase MovementPhrase = new Phrase("help_cat_movement", "MOVEMENT");

	public static Phrase CraftingPhrase = new Phrase("help_cat_crafting", "CRAFTING");

	public static Phrase CombatPhrase = new Phrase("help_cat_combat", "COMBAT");

	public static Phrase BuildingPhrase = new Phrase("help_cat_building", "BUILDING");

	public static Dictionary<MenuCategory, Phrase> CategoryPhraseLookup = new Dictionary<MenuCategory, Phrase>
	{
		{
			MenuCategory.Movement,
			MovementPhrase
		},
		{
			MenuCategory.Crafting,
			CraftingPhrase
		},
		{
			MenuCategory.Combat,
			CombatPhrase
		},
		{
			MenuCategory.Building,
			BuildingPhrase
		}
	};

	public MenuCategory Category;

	public int Priority;

	public TokenisedPhrase TextToDisplay;

	public Sprite StaticImage;

	public VideoClip VideoClip;

	static TutorialFullScreenHelpInfo()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected Obj, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected Obj, but got Unknown
	}
}

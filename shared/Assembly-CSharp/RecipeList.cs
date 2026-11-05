using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Recipe List")]
public class RecipeList : BaseScriptableObject
{
	public Recipe[] Recipes;

	public RecipeList[] AdditionalRecipeLists;

	[Header("Shown when a table uses more than one list")]
	public Phrase SectionTitle;

	public Sprite SectionIcon;

	private bool initd;

	private List<Recipe> _recipes;

	public List<Recipe> AllRecipes
	{
		get
		{
			if (initd)
			{
				return _recipes;
			}
			initd = true;
			_recipes = new List<Recipe>();
			if (Recipes != null)
			{
				_recipes.AddRange(Recipes);
			}
			if (AdditionalRecipeLists != null)
			{
				RecipeList[] additionalRecipeLists = AdditionalRecipeLists;
				foreach (RecipeList recipeList in additionalRecipeLists)
				{
					_recipes.AddRange(recipeList.Recipes);
				}
			}
			return _recipes;
		}
	}
}

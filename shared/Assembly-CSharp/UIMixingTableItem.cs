using Rust.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VLB;

public class UIMixingTableItem : MonoBehaviour
{
	public Image ItemIcon;

	public Tooltip ItemTooltip;

	public RustText TextItemNameAndQuantity;

	public UIMixingTableItemIngredient[] Ingredients;

	public bool Available;

	public Recipe Recipe;

	public void Init(Recipe r, UnityAction<Recipe> onClicked)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		Recipe = r;
		if ((Object)(object)Recipe == (Object)null)
		{
			return;
		}
		((UnityEvent)Utils.GetOrAddComponent<Button>(((Component)this).gameObject).onClick).AddListener((UnityAction)delegate
		{
			onClicked.Invoke(Recipe);
		});
		ItemIcon.sprite = Recipe.DisplayIcon;
		TextItemNameAndQuantity.SetText($"{Recipe.ProducedItemCount} x {Recipe.DisplayName}", true, false);
		ItemTooltip.Text = Recipe.DisplayDescription;
		for (int num = 0; num < Ingredients.Length; num++)
		{
			if (num >= Recipe.Ingredients.Length)
			{
				Ingredients[num].InitBlank();
			}
			else
			{
				Ingredients[num].Init(Recipe.Ingredients[num], Recipe.ProducedItem);
			}
		}
	}

	public void CleanUp()
	{
		Button component = ((Component)this).gameObject.GetComponent<Button>();
		if ((Object)(object)component != (Object)null)
		{
			((UnityEventBase)component.onClick).RemoveAllListeners();
		}
	}

	public void SetAvailable(bool flag)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Available = flag;
		((Graphic)TextItemNameAndQuantity).color = (Color)(flag ? new Color(0.78f, 0.78f, 0.78f) : Color.grey);
	}
}

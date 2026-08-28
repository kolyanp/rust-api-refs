using UnityEngine;

public class ItemModMenuOption : ItemMod
{
	public enum DisabledTooltipOption
	{
		CANT_USE_WHILE_HANDS_OCCUPIED,
		DISABLED
	}

	public string commandName;

	public ItemMod actionTarget;

	public BaseEntity.Menu.Option option;

	[Tooltip("If true, this is the command that will run when an item is 'selected' on the toolbar")]
	public bool isPrimaryOption = true;

	[Header("Still shows when disabled")]
	[Space(10f)]
	public bool showDisabled = true;

	public DisabledTooltipOption disabledTooltipDescription;

	public override void ServerCommand(Item item, string command, BasePlayer player)
	{
		if (!(command != commandName) && actionTarget.CanDoAction(item, player))
		{
			actionTarget.DoAction(item, player);
		}
	}

	private void OnValidate()
	{
		if ((Object)(object)actionTarget == (Object)null)
		{
			Debug.LogWarning((object)"ItemModMenuOption: actionTarget is null!", (Object)(object)((Component)this).gameObject);
		}
		if (string.IsNullOrEmpty(commandName))
		{
			Debug.LogWarning((object)"ItemModMenuOption: commandName can't be empty!", (Object)(object)((Component)this).gameObject);
		}
		if ((Object)(object)option.icon == (Object)null)
		{
			Debug.LogWarning((object)("No icon set for ItemModMenuOption " + ((Object)((Component)this).gameObject).name), (Object)(object)((Component)this).gameObject);
		}
	}
}

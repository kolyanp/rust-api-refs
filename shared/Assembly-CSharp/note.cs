using Facepunch.Extend;
using UnityEngine;

[Factory("note")]
public class note : ConsoleSystem
{
	[ServerUserVar]
	public static void update(Arg arg)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		ItemId itemID = ArgEx.GetItemID(arg, 0);
		string text = arg.GetString(1);
		Item item = ArgEx.Player(arg).inventory.FindItemByUID(itemID);
		if (item != null)
		{
			item.text = StringExtensions.Truncate(text, 1024, (string)null);
			item.MarkDirty();
		}
	}
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class ItemButtonTools : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
	public ScrollRect MainScroll;

	public Image image;

	public Image backgroundImage;

	public ItemDefinition itemDef;

	[UnityEvent]
	public void GiveSelf(int amount)
	{
		DebugLog();
		ConsoleSystem.Run(ConsoleSystem.Option.Client, "inventory.giveid", itemDef.itemid, amount);
	}

	[UnityEvent]
	public void GiveArmed()
	{
		DebugLog();
		ConsoleSystem.Run(ConsoleSystem.Option.Client, "inventory.givearm", itemDef.itemid);
	}

	[UnityEvent]
	public void GiveStack()
	{
		DebugLog();
		ConsoleSystem.Run(ConsoleSystem.Option.Client, "inventory.giveid", itemDef.itemid, itemDef.stackable);
	}

	public void GiveBlueprint()
	{
	}

	private void DebugLog()
	{
		if (((ButtonControl)Keyboard.current[(Key)53]).isPressed)
		{
			Debug.Log((object)((Object)((Component)itemDef).gameObject).name, (Object)(object)((Component)itemDef).gameObject);
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		MainScroll.OnBeginDrag(eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		MainScroll.OnDrag(eventData);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		MainScroll.OnEndDrag(eventData);
	}

	public void OnScroll(PointerEventData data)
	{
		MainScroll.OnScroll(data);
	}
}

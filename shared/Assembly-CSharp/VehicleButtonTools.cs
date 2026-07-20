using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class VehicleButtonTools : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
	public ScrollRect MainScroll;

	public Image image;

	public Image backgroundImage;

	public ItemDefinition itemDef;

	[UnityEvent]
	public void Spawn()
	{
		DebugLog();
		string text = itemDef.shortname;
		ItemModEntityReference component = ((Component)itemDef).GetComponent<ItemModEntityReference>();
		if (Object.op_Implicit((Object)(object)component))
		{
			text = ((Object)component.entityPrefab.Get()).name;
		}
		ConsoleSystem.Run(ConsoleSystem.Option.Client, "spawn", text);
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

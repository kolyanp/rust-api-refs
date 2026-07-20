using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Oxide.Game.Rust.Cui;

public class ComponentConverter : JsonConverter
{
	public override bool CanWrite => false;

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JObject val = JObject.Load(reader);
		string text = ((object)val["type"]).ToString();
		Type type = null;
		switch (text)
		{
		case "UnityEngine.UI.Text":
			type = typeof(CuiTextComponent);
			break;
		case "UnityEngine.UI.Image":
			type = typeof(CuiImageComponent);
			break;
		case "UnityEngine.UI.RawImage":
			type = typeof(CuiRawImageComponent);
			break;
		case "UnityEngine.UI.Button":
			type = typeof(CuiButtonComponent);
			break;
		case "UnityEngine.UI.Outline":
			type = typeof(CuiOutlineComponent);
			break;
		case "UnityEngine.UI.InputField":
			type = typeof(CuiInputFieldComponent);
			break;
		case "Countdown":
			type = typeof(CuiCountdownComponent);
			break;
		case "NeedsCursor":
			type = typeof(CuiNeedsCursorComponent);
			break;
		case "NeedsKeyboard":
			type = typeof(CuiNeedsKeyboardComponent);
			break;
		case "RectTransform":
			type = typeof(CuiRectTransformComponent);
			break;
		case "UnityEngine.UI.ScrollView":
			type = typeof(CuiScrollViewComponent);
			break;
		case "UnityEngine.UI.HorizontalLayoutGroup":
			type = typeof(CuiHorizontalLayoutGroupComponent);
			break;
		case "UnityEngine.UI.VerticalLayoutGroup":
			type = typeof(CuiVerticalLayoutGroupComponent);
			break;
		case "UnityEngine.UI.GridLayoutGroup":
			type = typeof(CuiGridLayoutGroupComponent);
			break;
		case "UnityEngine.UI.ContentSizeFitter":
			type = typeof(CuiContentSizeFitterComponent);
			break;
		case "UnityEngine.UI.LayoutElement":
			type = typeof(CuiLayoutElementComponent);
			break;
		case "Draggable":
			type = typeof(CuiDraggableComponent);
			break;
		case "Slot":
			type = typeof(CuiSlotComponent);
			break;
		default:
			return null;
		}
		object obj = Activator.CreateInstance(type);
		serializer.Populate(((JToken)val).CreateReader(), obj);
		return obj;
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(ICuiComponent);
	}
}

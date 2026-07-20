using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using Network;
using Oxide.Game.Rust.Cui;
using UnityEngine;
using UnityEngine.UI;

namespace Carbon.Components;

public static class CUIStatics
{
	public static readonly uint AddUiString = StringPool.Get("AddUi");

	internal static string ProcessColor(string color)
	{
		if (color.StartsWith("#"))
		{
			return CUI.HexToRustColor(color);
		}
		return color;
	}

	public static CUI.Pair<string, CuiElement> UpdatePanel(this CUI cui, string id, string color, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, bool blur = false, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string destroyUi = null, bool activeSelf = true, float rotation = 0f)
	{
		return cui.CreatePanel(null, null, color, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, blur, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update: true, activeSelf, rotation);
	}

	public static CUI.Pair<string, CuiElement> UpdateText(this CUI cui, string id, string color, string text, int size, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, TextAnchor align = (TextAnchor)4, CUI.Handler.FontTypes font = CUI.Handler.FontTypes.RobotoCondensedRegular, VerticalWrapMode verticalOverflow = (VerticalWrapMode)1, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string destroyUi = null, bool activeSelf = true, float rotation = 0f)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return cui.CreateText(null, null, color, text, size, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, align, font, verticalOverflow, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update: true, activeSelf, rotation);
	}

	public static CUI.Pair<string, CuiElement, CuiElement> UpdateButton(this CUI cui, string id, string color, string textColor, string text, int size, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, string command = null, TextAnchor align = (TextAnchor)4, CUI.Handler.FontTypes font = CUI.Handler.FontTypes.RobotoCondensedRegular, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string destroyUi = null, bool activeSelf = true, float rotation = 0f)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		return cui.CreateButton(null, null, color, textColor, text, size, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, command, align, font, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update: true, activeSelf, rotation);
	}

	public static CUI.Pair<string, CuiElement, CuiElement> UpdateProtectedButton(this CUI cui, string id, string color, string textColor, string text, int size, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, string command = null, TextAnchor align = (TextAnchor)4, CUI.Handler.FontTypes font = CUI.Handler.FontTypes.RobotoCondensedRegular, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string destroyUi = null, bool activeSelf = true, float rotation = 0f)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		return cui.CreateProtectedButton(null, null, color, textColor, text, size, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, command, align, font, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update: true, activeSelf, rotation);
	}

	public static CUI.Pair<string, CuiElement> UpdateInputField(this CUI cui, string id, string color, string text, int size, int characterLimit, bool readOnly, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, string command = null, TextAnchor align = (TextAnchor)4, CUI.Handler.FontTypes font = CUI.Handler.FontTypes.RobotoCondensedRegular, bool autoFocus = false, bool hudMenuInput = false, LineType lineType = (LineType)0, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string destroyUi = null, bool activeSelf = true, float rotation = 0f)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		return cui.CreateInputField(null, null, color, text, size, characterLimit, readOnly, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, command, align, font, autoFocus, hudMenuInput, lineType, fadeIn, fadeOut, needsCursor, needsKeyboard, id, destroyUi, update: true, activeSelf, rotation);
	}

	public static CUI.Pair<string, CuiElement> UpdateProtectedInputField(this CUI cui, string id, string color, string text, int size, int characterLimit, bool readOnly, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, string command = null, TextAnchor align = (TextAnchor)4, CUI.Handler.FontTypes font = CUI.Handler.FontTypes.RobotoCondensedRegular, bool autoFocus = false, bool hudMenuInput = false, LineType lineType = (LineType)0, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string destroyUi = null, bool activeSelf = true, float rotation = 0f)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		return cui.CreateProtectedInputField(null, null, color, text, size, characterLimit, readOnly, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, command, align, font, autoFocus, hudMenuInput, lineType, fadeIn, fadeOut, needsCursor, needsKeyboard, id, destroyUi, update: true, activeSelf, rotation);
	}

	public static CUI.Pair<string, CuiElement> UpdateImage(this CUI cui, string id, uint png, string color, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string destroyUi = null, bool activeSelf = true, float rotation = 0f)
	{
		return cui.CreateImage(null, null, png, color, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update: true, activeSelf, rotation);
	}

	public static CUI.Pair<string, CuiElement> UpdateImage(this CUI cui, string id, string url, string color, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string destroyUi = null, bool activeSelf = true, float rotation = 0f)
	{
		return cui.CreateImage(null, null, url, color, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update: true, activeSelf, rotation);
	}

	public static CUI.Pair<string, CuiElement> UpdateSimpleImage(this CUI cui, string id, string png, string sprite, string color, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string destroyUi = null, bool activeSelf = true, float rotation = 0f, string slice = null)
	{
		return cui.CreateSimpleImage(null, null, png, sprite, color, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update: true, activeSelf, rotation, slice);
	}

	public static CUI.Pair<string, CuiElement> UpdateSprite(this CUI cui, string id, string sprite, string color, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string destroyUi = null, bool activeSelf = true, float rotation = 0f)
	{
		return cui.CreateSprite(null, null, sprite, color, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update: true, activeSelf, rotation);
	}

	public static CUI.Pair<string, CuiElement> UpdateItemImage(this CUI cui, string id, int itemID, ulong skinID, string color, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string destroyUi = null, bool activeSelf = true, float rotation = 0f)
	{
		return cui.CreateItemImage(null, null, itemID, skinID, color, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update: true, activeSelf, rotation);
	}

	public static CUI.Pair<string, CuiElement> UpdateQRCodeImage(this CUI cui, string id, string text, string brandUrl, string brandColor, string brandBgColor, int pixels, bool transparent, bool quietZones, string color, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string destroyUi = null, bool activeSelf = true)
	{
		return cui.CreateQRCodeImage(null, null, text, brandUrl, brandColor, brandBgColor, pixels, transparent, quietZones, color, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update: true, activeSelf);
	}

	public static CUI.Pair<string, CuiElement> UpdateClientImage(this CUI cui, string id, string url, string color, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string destroyUi = null, bool activeSelf = true, float rotation = 0f)
	{
		return cui.CreateClientImage(null, null, url, color, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update: true, activeSelf, rotation);
	}

	public static CUI.Pair<string, CuiElement> UpdateCountdown(this CUI cui, string id, int startTime, int endTime, int step, string command, float fadeIn = 0f, float fadeOut = 0f, string destroyUi = null, bool activeSelf = true)
	{
		return cui.CreateCountdown(null, null, startTime, endTime, step, command, fadeIn, fadeOut, id, destroyUi, update: true, activeSelf);
	}

	public static CUI.Pair<string, CuiElement> UpdateScrollView(this CUI cui, string id, bool vertical, bool horizontal, MovementType movementType, float elasticity, bool inertia, float decelerationRate, float scrollSensitivity, out CuiRectTransform contentTransformComponent, out CuiScrollbar horizontalScrollBar, out CuiScrollbar verticalScrollBar, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string destroyUi = null, bool activeSelf = true, float rotation = 0f, float pivotX = 0.5f, float pivotY = 0.5f, float scrollPosHorizontal = 0f, float scrollPosVertical = 0f)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return cui.CreateScrollView(null, null, vertical, horizontal, movementType, elasticity, inertia, decelerationRate, scrollSensitivity, out contentTransformComponent, out horizontalScrollBar, out verticalScrollBar, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, fadeIn, fadeOut, needsCursor, needsKeyboard, id, destroyUi, update: true, activeSelf, rotation, pivotX, pivotY, scrollPosHorizontal, scrollPosVertical);
	}

	public static CUI.Pair<string, CuiElement> Panel(this CUI.Handler cui, CuiElementContainer container, string parent, string color, string material, float xMin, float xMax, float yMin, float yMax, float OxMin, float OxMax, float OyMin, float OyMax, bool blur = false, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		if (id == null)
		{
			id = cui.AppendId();
		}
		CuiElement cuiElement = cui.TakeFromPool(id, parent, fadeOut, destroyUi, update, activeSelf);
		CuiImageComponent cuiImageComponent = cui.TakeFromPoolImage();
		cuiImageComponent.Color = ProcessColor(color);
		if (blur)
		{
			cuiImageComponent.Material = "assets/content/ui/uibackgroundblur.mat";
		}
		else if (material != null)
		{
			cuiImageComponent.Material = material;
		}
		cuiImageComponent.FadeIn = fadeIn;
		cuiElement.Components.Add(cuiImageComponent);
		if (!update || (update && (xMin != 0f || xMax != 1f || yMin != 0f || yMax != 1f)))
		{
			CuiRectTransformComponent cuiRectTransformComponent = cui.TakeFromPoolRect();
			cuiRectTransformComponent.AnchorMin = LUIBuilder.GetStringFloat(xMin, yMin);
			cuiRectTransformComponent.AnchorMax = LUIBuilder.GetStringFloat(xMax, yMax);
			cuiRectTransformComponent.OffsetMin = LUIBuilder.GetStringFloat(OxMin, OyMin);
			cuiRectTransformComponent.OffsetMax = LUIBuilder.GetStringFloat(OxMax, OyMax);
			cuiRectTransformComponent.Rotation = rotation;
			cuiElement.Components.Add(cuiRectTransformComponent);
		}
		if (needsCursor)
		{
			cuiElement.Components.Add(cui.TakeFromPoolNeedsCursor());
		}
		if (needsKeyboard)
		{
			cuiElement.Components.Add(cui.TakeFromPoolNeedsKeyboard());
		}
		if (outlineColor != null)
		{
			CuiOutlineComponent cuiOutlineComponent = cui.TakeFromPoolOutline();
			cuiOutlineComponent.Color = ProcessColor(outlineColor);
			cuiOutlineComponent.Distance = outlineDistance;
			cuiOutlineComponent.UseGraphicAlpha = outlineUseGraphicAlpha;
			cuiElement.Components.Add(cuiOutlineComponent);
		}
		if (!update)
		{
			container?.Add(cuiElement);
		}
		return new CUI.Pair<string, CuiElement>(id, cuiElement);
	}

	public static CUI.Pair<string, CuiElement> Text(this CUI.Handler cui, CuiElementContainer container, string parent, string color, string text, int size, float xMin, float xMax, float yMin, float yMax, float OxMin, float OxMax, float OyMin, float OyMax, TextAnchor align, CUI.Handler.FontTypes font, VerticalWrapMode verticalOverflow, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (id == null)
		{
			id = cui.AppendId();
		}
		CuiElement cuiElement = cui.TakeFromPool(id, parent, fadeOut, destroyUi, update, activeSelf);
		CuiTextComponent cuiTextComponent = cui.TakeFromPoolText();
		cuiTextComponent.Text = (string.IsNullOrEmpty(text) ? string.Empty : text);
		cuiTextComponent.FontSize = size;
		cuiTextComponent.Align = align;
		cuiTextComponent.Font = cui.GetFont(font);
		cuiTextComponent.Color = ProcessColor(color);
		cuiTextComponent.FadeIn = fadeIn;
		cuiTextComponent.VerticalOverflow = verticalOverflow;
		cuiElement.Components.Add(cuiTextComponent);
		if (!update || (update && (xMin != 0f || xMax != 1f || yMin != 0f || yMax != 1f)))
		{
			CuiRectTransformComponent cuiRectTransformComponent = cui.TakeFromPoolRect();
			cuiRectTransformComponent.AnchorMin = LUIBuilder.GetStringFloat(xMin, yMin);
			cuiRectTransformComponent.AnchorMax = LUIBuilder.GetStringFloat(xMax, yMax);
			cuiRectTransformComponent.OffsetMin = LUIBuilder.GetStringFloat(OxMin, OyMin);
			cuiRectTransformComponent.OffsetMax = LUIBuilder.GetStringFloat(OxMax, OyMax);
			cuiRectTransformComponent.Rotation = rotation;
			cuiElement.Components.Add(cuiRectTransformComponent);
		}
		if (needsCursor)
		{
			cuiElement.Components.Add(cui.TakeFromPoolNeedsCursor());
		}
		if (needsKeyboard)
		{
			cuiElement.Components.Add(cui.TakeFromPoolNeedsKeyboard());
		}
		if (outlineColor != null)
		{
			CuiOutlineComponent cuiOutlineComponent = cui.TakeFromPoolOutline();
			cuiOutlineComponent.Color = ProcessColor(outlineColor);
			cuiOutlineComponent.Distance = outlineDistance;
			cuiOutlineComponent.UseGraphicAlpha = outlineUseGraphicAlpha;
			cuiElement.Components.Add(cuiOutlineComponent);
		}
		if (!update)
		{
			container?.Add(cuiElement);
		}
		return new CUI.Pair<string, CuiElement>(id, cuiElement);
	}

	public static CUI.Pair<string, CuiElement, CuiElement> Button(this CUI.Handler cui, CuiElementContainer container, string parent, string color, string textColor, string text, int size, string material, float xMin, float xMax, float yMin, float yMax, float OxMin, float OxMax, float OyMin, float OyMax, string command, TextAnchor align, CUI.Handler.FontTypes font, bool @protected, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		if (id == null)
		{
			id = cui.AppendId();
		}
		CuiElement cuiElement = cui.TakeFromPool(id, parent, fadeOut, destroyUi, update, activeSelf);
		CuiButtonComponent cuiButtonComponent = cui.TakeFromPoolButton();
		cuiButtonComponent.FadeIn = fadeIn;
		cuiButtonComponent.Color = ProcessColor(color);
		cuiButtonComponent.Command = (@protected ? Community.Protect(command) : command);
		if (material != null)
		{
			cuiButtonComponent.Material = material;
		}
		cuiElement.Components.Add(cuiButtonComponent);
		if (!update || (update && (xMin != 0f || xMax != 1f || yMin != 0f || yMax != 1f)))
		{
			CuiRectTransformComponent cuiRectTransformComponent = cui.TakeFromPoolRect();
			cuiRectTransformComponent.AnchorMin = LUIBuilder.GetStringFloat(xMin, yMin);
			cuiRectTransformComponent.AnchorMax = LUIBuilder.GetStringFloat(xMax, yMax);
			cuiRectTransformComponent.OffsetMin = LUIBuilder.GetStringFloat(OxMin, OyMin);
			cuiRectTransformComponent.OffsetMax = LUIBuilder.GetStringFloat(OxMax, OyMax);
			cuiRectTransformComponent.Rotation = rotation;
			cuiElement.Components.Add(cuiRectTransformComponent);
		}
		if (needsCursor)
		{
			cuiElement.Components.Add(cui.TakeFromPoolNeedsCursor());
		}
		if (needsKeyboard)
		{
			cuiElement.Components.Add(cui.TakeFromPoolNeedsKeyboard());
		}
		if (!update)
		{
			container?.Add(cuiElement);
		}
		CuiElement cuiElement2 = null;
		if (!string.IsNullOrEmpty(text))
		{
			cuiElement2 = cui.TakeFromPool(cui.AppendId(), cuiElement.Name);
			CuiTextComponent cuiTextComponent = cui.TakeFromPoolText();
			cuiTextComponent.Text = text;
			cuiTextComponent.FontSize = size;
			cuiTextComponent.Align = align;
			cuiTextComponent.Color = ProcessColor(textColor);
			cuiTextComponent.Font = cui.GetFont(font);
			cuiElement2.Components.Add(cuiTextComponent);
			CuiRectTransformComponent cuiRectTransformComponent2 = cui.TakeFromPoolRect();
			cuiRectTransformComponent2.AnchorMin = "0.02 0";
			cuiRectTransformComponent2.AnchorMax = "0.98 1";
			cuiElement2.Components.Add(cuiRectTransformComponent2);
			if (!update)
			{
				container?.Add(cuiElement2);
			}
		}
		if (outlineColor != null)
		{
			CuiOutlineComponent cuiOutlineComponent = cui.TakeFromPoolOutline();
			cuiOutlineComponent.Color = ProcessColor(outlineColor);
			cuiOutlineComponent.Distance = outlineDistance;
			cuiOutlineComponent.UseGraphicAlpha = outlineUseGraphicAlpha;
			cuiElement.Components.Add(cuiOutlineComponent);
		}
		return new CUI.Pair<string, CuiElement, CuiElement>(id, cuiElement, cuiElement2);
	}

	public static CUI.Pair<string, CuiElement> InputField(this CUI.Handler cui, CuiElementContainer container, string parent, string color, string text, int size, int characterLimit, bool readOnly, float xMin, float xMax, float yMin, float yMax, float OxMin, float OxMax, float OyMin, float OyMax, string command, TextAnchor align, CUI.Handler.FontTypes font, bool @protected, bool autoFocus = false, bool hudMenuInput = false, LineType lineType = (LineType)0, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (id == null)
		{
			id = cui.AppendId();
		}
		CuiElement cuiElement = cui.TakeFromPool(id, parent, fadeOut, destroyUi, update, activeSelf);
		CuiInputFieldComponent cuiInputFieldComponent = cui.TakeFromPoolInputField();
		cuiInputFieldComponent.Color = ProcessColor(color);
		cuiInputFieldComponent.Text = (string.IsNullOrEmpty(text) ? string.Empty : text);
		cuiInputFieldComponent.FontSize = size;
		cuiInputFieldComponent.Font = cui.GetFont(font);
		cuiInputFieldComponent.Align = align;
		cuiInputFieldComponent.CharsLimit = characterLimit;
		cuiInputFieldComponent.ReadOnly = readOnly;
		cuiInputFieldComponent.Command = (@protected ? Community.Protect(command) : command);
		cuiInputFieldComponent.LineType = lineType;
		cuiInputFieldComponent.Autofocus = autoFocus;
		cuiInputFieldComponent.HudMenuInput = hudMenuInput;
		cuiElement.Components.Add(cuiInputFieldComponent);
		if (needsCursor)
		{
			cuiElement.Components.Add(cui.TakeFromPoolNeedsCursor());
		}
		if (needsKeyboard && !cuiInputFieldComponent.ReadOnly)
		{
			cuiElement.Components.Add(cui.TakeFromPoolNeedsKeyboard());
		}
		if (!update || (update && (xMin != 0f || xMax != 1f || yMin != 0f || yMax != 1f)))
		{
			CuiRectTransformComponent cuiRectTransformComponent = cui.TakeFromPoolRect();
			cuiRectTransformComponent.AnchorMin = LUIBuilder.GetStringFloat(xMin, yMin);
			cuiRectTransformComponent.AnchorMax = LUIBuilder.GetStringFloat(xMax, yMax);
			cuiRectTransformComponent.OffsetMin = LUIBuilder.GetStringFloat(OxMin, OyMin);
			cuiRectTransformComponent.OffsetMax = LUIBuilder.GetStringFloat(OxMax, OyMax);
			cuiRectTransformComponent.Rotation = rotation;
			cuiElement.Components.Add(cuiRectTransformComponent);
		}
		if (!update)
		{
			container?.Add(cuiElement);
		}
		return new CUI.Pair<string, CuiElement>(id, cuiElement);
	}

	public static CUI.Pair<string, CuiElement> Image(this CUI.Handler cui, CuiElementContainer container, string parent, string png, string url, string steamId, string color, string material, float xMin, float xMax, float yMin, float yMax, float OxMin, float OxMax, float OyMin, float OyMax, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		if (id == null)
		{
			id = cui.AppendId();
		}
		CuiElement cuiElement = cui.TakeFromPool(id, parent, fadeOut, destroyUi, update, activeSelf);
		CuiRawImageComponent cuiRawImageComponent = cui.TakeFromPoolRawImage();
		cuiRawImageComponent.Png = png;
		cuiRawImageComponent.Url = url;
		cuiRawImageComponent.SteamId = steamId;
		cuiRawImageComponent.FadeIn = fadeIn;
		cuiRawImageComponent.Color = ProcessColor(color);
		if (material != null)
		{
			cuiRawImageComponent.Material = material;
		}
		cuiElement.Components.Add(cuiRawImageComponent);
		if (!update || (update && (xMin != 0f || xMax != 1f || yMin != 0f || yMax != 1f)))
		{
			CuiRectTransformComponent cuiRectTransformComponent = cui.TakeFromPoolRect();
			cuiRectTransformComponent.AnchorMin = LUIBuilder.GetStringFloat(xMin, yMin);
			cuiRectTransformComponent.AnchorMax = LUIBuilder.GetStringFloat(xMax, yMax);
			cuiRectTransformComponent.OffsetMin = LUIBuilder.GetStringFloat(OxMin, OyMin);
			cuiRectTransformComponent.OffsetMax = LUIBuilder.GetStringFloat(OxMax, OyMax);
			cuiRectTransformComponent.Rotation = rotation;
			cuiElement.Components.Add(cuiRectTransformComponent);
		}
		if (needsCursor)
		{
			cuiElement.Components.Add(cui.TakeFromPoolNeedsCursor());
		}
		if (needsKeyboard)
		{
			cuiElement.Components.Add(cui.TakeFromPoolNeedsKeyboard());
		}
		if (outlineColor != null)
		{
			CuiOutlineComponent cuiOutlineComponent = cui.TakeFromPoolOutline();
			cuiOutlineComponent.Color = ProcessColor(outlineColor);
			cuiOutlineComponent.Distance = outlineDistance;
			cuiOutlineComponent.UseGraphicAlpha = outlineUseGraphicAlpha;
			cuiElement.Components.Add(cuiOutlineComponent);
		}
		if (!update)
		{
			container?.Add(cuiElement);
		}
		return new CUI.Pair<string, CuiElement>(id, cuiElement);
	}

	public static CUI.Pair<string, CuiElement> SimpleImage(this CUI.Handler cui, CuiElementContainer container, string parent, string png, string sprite, string color, string material, float xMin, float xMax, float yMin, float yMax, float OxMin, float OxMax, float OyMin, float OyMax, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f, string slice = null)
	{
		if (id == null)
		{
			id = cui.AppendId();
		}
		CuiElement cuiElement = cui.TakeFromPool(id, parent, fadeOut, destroyUi, update, activeSelf);
		CuiImageComponent cuiImageComponent = cui.TakeFromPoolImage();
		cuiImageComponent.Png = png;
		cuiImageComponent.Sprite = sprite;
		cuiImageComponent.FadeIn = fadeIn;
		cuiImageComponent.Color = ProcessColor(color);
		cuiImageComponent.Slice = slice;
		if (material != null)
		{
			cuiImageComponent.Material = material;
		}
		cuiElement.Components.Add(cuiImageComponent);
		if (!update || (update && (xMin != 0f || xMax != 1f || yMin != 0f || yMax != 1f)))
		{
			CuiRectTransformComponent cuiRectTransformComponent = cui.TakeFromPoolRect();
			cuiRectTransformComponent.AnchorMin = LUIBuilder.GetStringFloat(xMin, yMin);
			cuiRectTransformComponent.AnchorMax = LUIBuilder.GetStringFloat(xMax, yMax);
			cuiRectTransformComponent.OffsetMin = LUIBuilder.GetStringFloat(OxMin, OyMin);
			cuiRectTransformComponent.OffsetMax = LUIBuilder.GetStringFloat(OxMax, OyMax);
			cuiRectTransformComponent.Rotation = rotation;
			cuiElement.Components.Add(cuiRectTransformComponent);
		}
		if (needsCursor)
		{
			cuiElement.Components.Add(cui.TakeFromPoolNeedsCursor());
		}
		if (needsKeyboard)
		{
			cuiElement.Components.Add(cui.TakeFromPoolNeedsKeyboard());
		}
		if (outlineColor != null)
		{
			CuiOutlineComponent cuiOutlineComponent = cui.TakeFromPoolOutline();
			cuiOutlineComponent.Color = ProcessColor(outlineColor);
			cuiOutlineComponent.Distance = outlineDistance;
			cuiOutlineComponent.UseGraphicAlpha = outlineUseGraphicAlpha;
			cuiElement.Components.Add(cuiOutlineComponent);
		}
		if (!update)
		{
			container?.Add(cuiElement);
		}
		return new CUI.Pair<string, CuiElement>(id, cuiElement);
	}

	public static CUI.Pair<string, CuiElement> Sprite(this CUI.Handler cui, CuiElementContainer container, string parent, string sprite, string color, string material, float xMin, float xMax, float yMin, float yMax, float OxMin, float OxMax, float OyMin, float OyMax, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		if (id == null)
		{
			id = cui.AppendId();
		}
		CuiElement cuiElement = cui.TakeFromPool(id, parent, fadeOut, destroyUi, update, activeSelf);
		CuiRawImageComponent cuiRawImageComponent = cui.TakeFromPoolRawImage();
		cuiRawImageComponent.Sprite = sprite;
		cuiRawImageComponent.FadeIn = fadeIn;
		cuiRawImageComponent.Color = ProcessColor(color);
		if (material != null)
		{
			cuiRawImageComponent.Material = material;
		}
		cuiElement.Components.Add(cuiRawImageComponent);
		if (!update || (update && (xMin != 0f || xMax != 1f || yMin != 0f || yMax != 1f)))
		{
			CuiRectTransformComponent cuiRectTransformComponent = cui.TakeFromPoolRect();
			cuiRectTransformComponent.AnchorMin = LUIBuilder.GetStringFloat(xMin, yMin);
			cuiRectTransformComponent.AnchorMax = LUIBuilder.GetStringFloat(xMax, yMax);
			cuiRectTransformComponent.OffsetMin = LUIBuilder.GetStringFloat(OxMin, OyMin);
			cuiRectTransformComponent.OffsetMax = LUIBuilder.GetStringFloat(OxMax, OyMax);
			cuiRectTransformComponent.Rotation = rotation;
			cuiElement.Components.Add(cuiRectTransformComponent);
		}
		if (needsCursor)
		{
			cuiElement.Components.Add(cui.TakeFromPoolNeedsCursor());
		}
		if (needsKeyboard)
		{
			cuiElement.Components.Add(cui.TakeFromPoolNeedsKeyboard());
		}
		if (outlineColor != null)
		{
			CuiOutlineComponent cuiOutlineComponent = cui.TakeFromPoolOutline();
			cuiOutlineComponent.Color = ProcessColor(outlineColor);
			cuiOutlineComponent.Distance = outlineDistance;
			cuiOutlineComponent.UseGraphicAlpha = outlineUseGraphicAlpha;
			cuiElement.Components.Add(cuiOutlineComponent);
		}
		if (!update)
		{
			container?.Add(cuiElement);
		}
		return new CUI.Pair<string, CuiElement>(id, cuiElement);
	}

	public static CUI.Pair<string, CuiElement> ItemImage(this CUI.Handler cui, CuiElementContainer container, string parent, int itemID, ulong skinID, string color, string material, float xMin, float xMax, float yMin, float yMax, float OxMin, float OxMax, float OyMin, float OyMax, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		if (id == null)
		{
			id = cui.AppendId();
		}
		CuiElement cuiElement = cui.TakeFromPool(id, parent, fadeOut, destroyUi, update, activeSelf);
		CuiImageComponent cuiImageComponent = cui.TakeFromPoolImage();
		cuiImageComponent.ItemId = itemID;
		cuiImageComponent.SkinId = skinID;
		cuiImageComponent.FadeIn = fadeIn;
		cuiImageComponent.Color = ProcessColor(color);
		if (material != null)
		{
			cuiImageComponent.Material = material;
		}
		cuiElement.Components.Add(cuiImageComponent);
		if (!update || (update && (xMin != 0f || xMax != 1f || yMin != 0f || yMax != 1f)))
		{
			CuiRectTransformComponent cuiRectTransformComponent = cui.TakeFromPoolRect();
			cuiRectTransformComponent.AnchorMin = LUIBuilder.GetStringFloat(xMin, yMin);
			cuiRectTransformComponent.AnchorMax = LUIBuilder.GetStringFloat(xMax, yMax);
			cuiRectTransformComponent.OffsetMin = LUIBuilder.GetStringFloat(OxMin, OyMin);
			cuiRectTransformComponent.OffsetMax = LUIBuilder.GetStringFloat(OxMax, OyMax);
			cuiRectTransformComponent.Rotation = rotation;
			cuiElement.Components.Add(cuiRectTransformComponent);
		}
		if (needsCursor)
		{
			cuiElement.Components.Add(cui.TakeFromPoolNeedsCursor());
		}
		if (needsKeyboard)
		{
			cuiElement.Components.Add(cui.TakeFromPoolNeedsKeyboard());
		}
		if (outlineColor != null)
		{
			CuiOutlineComponent cuiOutlineComponent = cui.TakeFromPoolOutline();
			cuiOutlineComponent.Color = ProcessColor(outlineColor);
			cuiOutlineComponent.Distance = outlineDistance;
			cuiOutlineComponent.UseGraphicAlpha = outlineUseGraphicAlpha;
			cuiElement.Components.Add(cuiOutlineComponent);
		}
		if (!update)
		{
			container?.Add(cuiElement);
		}
		return new CUI.Pair<string, CuiElement>(id, cuiElement);
	}

	public static CUI.Pair<string, CuiElement> Countdown(this CUI.Handler cui, CuiElementContainer container, string parent, int startTime, int endTime, int step, string command, float fadeIn = 0f, float fadeOut = 0f, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true)
	{
		if (id == null)
		{
			id = cui.AppendId();
		}
		CuiElement cuiElement = cui.TakeFromPool(id, parent, fadeOut, destroyUi, update, activeSelf);
		CuiCountdownComponent cuiCountdownComponent = cui.TakeFromPoolCountdown();
		cuiCountdownComponent.StartTime = startTime;
		cuiCountdownComponent.EndTime = endTime;
		cuiCountdownComponent.Step = step;
		cuiCountdownComponent.Command = command;
		cuiCountdownComponent.FadeIn = fadeIn;
		cuiElement.Components.Add(cuiCountdownComponent);
		if (!update)
		{
			container?.Add(cuiElement);
		}
		return new CUI.Pair<string, CuiElement>(id, cuiElement);
	}

	public static CUI.Pair<string, CuiElement> ScrollView(this CUI.Handler cui, CuiElementContainer container, string parent, bool vertical, bool horizontal, MovementType movementType, float elasticity, bool inertia, float decelerationRate, float scrollSensitivity, out CuiRectTransform contentTransformComponent, out CuiScrollbar horizontalScrollBar, out CuiScrollbar verticalScrollBar, float xMin, float xMax, float yMin, float yMax, float OxMin, float OxMax, float OyMin, float OyMax, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f, float pivotX = 0.5f, float pivotY = 0.5f, float scrollPosHorizontal = 0f, float scrollPosVertical = 1f)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (id == null)
		{
			id = cui.AppendId();
		}
		CuiElement cuiElement = cui.TakeFromPool(id, parent, fadeOut, destroyUi, update, activeSelf);
		CuiScrollViewComponent cuiScrollViewComponent = cui.TakeFromPoolScrollView();
		cuiScrollViewComponent.Vertical = vertical;
		cuiScrollViewComponent.Horizontal = horizontal;
		cuiScrollViewComponent.MovementType = movementType;
		cuiScrollViewComponent.Elasticity = elasticity;
		cuiScrollViewComponent.Inertia = inertia;
		cuiScrollViewComponent.DecelerationRate = decelerationRate;
		cuiScrollViewComponent.ScrollSensitivity = scrollSensitivity;
		contentTransformComponent = cuiScrollViewComponent.ContentTransform;
		horizontalScrollBar = cuiScrollViewComponent.HorizontalScrollbar;
		verticalScrollBar = cuiScrollViewComponent.VerticalScrollbar;
		cuiScrollViewComponent.HorizontalNormalizedPosition = scrollPosHorizontal;
		cuiScrollViewComponent.VerticalNormalizedPosition = scrollPosVertical;
		cuiElement.Components.Add(cuiScrollViewComponent);
		if (!update || (update && (xMin != 0f || xMax != 1f || yMin != 0f || yMax != 1f)))
		{
			CuiRectTransformComponent cuiRectTransformComponent = cui.TakeFromPoolRect();
			cuiRectTransformComponent.AnchorMin = LUIBuilder.GetStringFloat(xMin, yMin);
			cuiRectTransformComponent.AnchorMax = LUIBuilder.GetStringFloat(xMax, yMax);
			cuiRectTransformComponent.OffsetMin = LUIBuilder.GetStringFloat(OxMin, OyMin);
			cuiRectTransformComponent.OffsetMax = LUIBuilder.GetStringFloat(OxMax, OyMax);
			cuiRectTransformComponent.Rotation = rotation;
			cuiRectTransformComponent.Pivot = LUIBuilder.GetStringFloat(pivotX, pivotY);
			cuiElement.Components.Add(cuiRectTransformComponent);
		}
		if (needsCursor)
		{
			cuiElement.Components.Add(cui.TakeFromPoolNeedsCursor());
		}
		if (needsKeyboard)
		{
			cuiElement.Components.Add(cui.TakeFromPoolNeedsKeyboard());
		}
		if (!update)
		{
			container?.Add(cuiElement);
		}
		return new CUI.Pair<string, CuiElement>(id, cuiElement);
	}

	public static void Send(this CuiElementContainer container, BasePlayer player)
	{
		CuiHelper.AddUi(player, container);
	}

	public static byte[] GetData(this CuiElementContainer container)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		NetWrite val = ((BaseNetwork)Net.sv).StartWrite();
		val.PacketID((Type)9);
		val.EntityID(((BaseNetworkable)CommunityEntity.ServerInstance).net.ID);
		val.UInt32(AddUiString);
		val.UInt64(0uL);
		val.String(container.ToJson(), false);
		byte[] array = new byte[((Stream)(object)val).Length];
		Array.Copy(val.stream._buffer, array, ((Stream)(object)val).Length);
		Pool.Free<NetWrite>(ref val);
		return array;
	}

	public static void SendData(byte[] data, BasePlayer player)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		NetWrite val = ((BaseNetwork)Net.sv).StartWrite();
		val.PacketID((Type)9);
		Array.Copy(data, 0L, val.stream._buffer, ((Stream)(object)val).Length, data.Length);
		BufferStream stream = val.stream;
		stream._length += data.Length;
		val.Send(new SendInfo(player.Connection));
	}

	public static void SendUpdate(this CUI.Pair<string, CuiElement> pair, BasePlayer player)
	{
		List<CuiElement> list = Pool.Get<List<CuiElement>>();
		list.Add(pair.Element);
		CuiHelper.AddUi(player, list);
		Pool.FreeUnmanaged<CuiElement>(ref list);
	}

	public static void Destroy(this CuiElementContainer container, BasePlayer player)
	{
		CuiHelper.DestroyUi(player, container.Name);
	}

	public static void Destroy(string name, BasePlayer player)
	{
		CuiHelper.DestroyUi(player, name);
	}
}

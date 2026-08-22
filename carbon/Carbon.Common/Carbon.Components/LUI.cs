using System;
using System.Collections.Generic;
using Carbon.Base;
using Carbon.Extensions;
using Carbon.Modules;
using Network;
using Oxide.Game.Rust.Cui;
using UnityEngine;
using UnityEngine.UI;

namespace Carbon.Components;

public class LUI : IDisposable
{
	public class LuiContainer
	{
		public string name;

		public string parent;

		public LuiComponentDictionary luiComponents;

		public string destroyUi;

		public float fadeOut;

		public bool update;

		public bool activeSelf = true;

		public LuiContainer SetDestroy(string name)
		{
			destroyUi = name;
			return this;
		}

		public LuiContainer SetFadeOut(float time)
		{
			fadeOut = time;
			return this;
		}

		public LuiContainer SetName(string newName)
		{
			name = newName;
			return this;
		}

		public LuiContainer SetActiveSelf(bool active)
		{
			activeSelf = active;
			return this;
		}

		public T UpdateComp<T>() where T : LuiCompBase
		{
			if (luiComponents.TryGetValue<T>(LuiPool.GetLuiCompType(typeof(T)), out var value))
			{
				return value;
			}
			value = LuiPool.GetLuiCompFromPool<T>(typeof(T));
			luiComponents.Add(value.type, value);
			return value;
		}

		public void SetEnabled<T>(bool enabled = true) where T : LuiCompBase
		{
			if (luiComponents.TryGetValue<T>(LuiPool.GetLuiCompType(typeof(T)), out var value))
			{
				value.enabled = enabled;
			}
			else
			{
				Logger.Warn($"[LUI] You're trying to switch state of component '{typeof(T)}' but it isn't present. Ignoring.");
			}
		}

		public void SetFadeIn<T>(float fadeIn) where T : LuiCompBase
		{
			if (luiComponents.TryGetValue<T>(LuiPool.GetLuiCompType(typeof(T)), out var value))
			{
				value.fadeIn = fadeIn;
			}
			else
			{
				Logger.Warn($"[LUI] You're trying to switch fadeIn of component '{typeof(T)}' but it isn't present. Ignoring.");
			}
		}

		public void SetBlocksRaycast<T>(bool blocksRaycast) where T : LuiCompBase
		{
			if (luiComponents.TryGetValue<T>(LuiPool.GetLuiCompType(typeof(T)), out var value))
			{
				value.blocksRaycast = blocksRaycast;
			}
			else
			{
				Logger.Warn($"[LUI] You're trying to switch blocksRaycast of component '{typeof(T)}' but it isn't present. Ignoring.");
			}
		}

		public void SetPlaceholderParentId<T>(string placeholderParentId) where T : LuiCompBase
		{
			if (luiComponents.TryGetValue<T>(LuiPool.GetLuiCompType(typeof(T)), out var value))
			{
				value.placeholderParentId = placeholderParentId;
			}
			else
			{
				Logger.Warn($"[LUI] You're trying to switch placeholderParentId of component '{typeof(T)}' but it isn't present. Ignoring.");
			}
		}

		public unsafe LuiContainer SetText(string input, int fontSize = 0, string color = null, TextAnchor alignment = (TextAnchor)4, bool update = false)
		{
			if (luiComponents.TryGetValue<LuiTextComp>(LuiCompType.Text, out var value))
			{
				value.text = input;
				if (fontSize > 0)
				{
					value.fontSize = fontSize;
				}
				if (color != null)
				{
					value.color = color;
				}
				if (!update)
				{
					value.align = ((object)(*(TextAnchor*)(&alignment))/*cast due to constrained. prefix*/).ToString();
				}
			}
			else
			{
				value = LuiPool.GetText();
				value.text = input;
				if (fontSize > 0)
				{
					value.fontSize = fontSize;
				}
				if (color != null)
				{
					value.color = color;
				}
				if (!update)
				{
					value.align = ((object)(*(TextAnchor*)(&alignment))/*cast due to constrained. prefix*/).ToString();
				}
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetTextColor(string color)
		{
			if (luiComponents.TryGetValue<LuiTextComp>(LuiCompType.Text, out var value))
			{
				value.color = color;
			}
			else
			{
				value = LuiPool.GetText();
				value.color = color;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetTextFont(CUI.Handler.FontTypes font)
		{
			if (luiComponents.TryGetValue<LuiTextComp>(LuiCompType.Text, out var value))
			{
				value.font = GetFont(font);
			}
			else
			{
				value = LuiPool.GetText();
				value.font = GetFont(font);
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public unsafe LuiContainer SetTextAlign(TextAnchor align)
		{
			if (luiComponents.TryGetValue<LuiTextComp>(LuiCompType.Text, out var value))
			{
				value.align = ((object)(*(TextAnchor*)(&align))/*cast due to constrained. prefix*/).ToString();
			}
			else
			{
				value = LuiPool.GetText();
				value.align = ((object)(*(TextAnchor*)(&align))/*cast due to constrained. prefix*/).ToString();
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public unsafe LuiContainer SetTextOverflow(VerticalWrapMode verticalOverflow)
		{
			if (luiComponents.TryGetValue<LuiTextComp>(LuiCompType.Text, out var value))
			{
				value.verticalOverflow = ((object)(*(VerticalWrapMode*)(&verticalOverflow))/*cast due to constrained. prefix*/).ToString();
			}
			else
			{
				value = LuiPool.GetText();
				value.verticalOverflow = ((object)(*(VerticalWrapMode*)(&verticalOverflow))/*cast due to constrained. prefix*/).ToString();
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetColor(string color)
		{
			if (luiComponents.TryGetValue<LuiImageComp>(LuiCompType.Image, out var value))
			{
				value.color = color;
			}
			else
			{
				value = LuiPool.GetImage();
				value.color = color;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetMaterial(string material)
		{
			if (luiComponents.TryGetValue<LuiImageComp>(LuiCompType.Image, out var value))
			{
				value.material = material;
			}
			else
			{
				value = LuiPool.GetImage();
				value.material = material;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public unsafe LuiContainer SetImageType(Type imageType)
		{
			if (luiComponents.TryGetValue<LuiImageComp>(LuiCompType.Image, out var value))
			{
				value.imageType = ((object)(*(Type*)(&imageType))/*cast due to constrained. prefix*/).ToString();
			}
			else
			{
				value = LuiPool.GetImage();
				value.imageType = ((object)(*(Type*)(&imageType))/*cast due to constrained. prefix*/).ToString();
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public unsafe LuiContainer SetSprite(string sprite = null, string color = null, Type imageType = (Type)0)
		{
			if (luiComponents.TryGetValue<LuiImageComp>(LuiCompType.Image, out var value))
			{
				if (sprite != null)
				{
					value.sprite = sprite;
					value.imageType = ((object)(*(Type*)(&imageType))/*cast due to constrained. prefix*/).ToString();
				}
				if (color != null)
				{
					value.color = color;
				}
			}
			else
			{
				value = LuiPool.GetImage();
				if (sprite != null)
				{
					value.sprite = sprite;
					value.imageType = ((object)(*(Type*)(&imageType))/*cast due to constrained. prefix*/).ToString();
				}
				if (color != null)
				{
					value.color = color;
				}
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetImage(string png = null, string color = null)
		{
			if (luiComponents.TryGetValue<LuiImageComp>(LuiCompType.Image, out var value))
			{
				if (png != null)
				{
					value.png = png;
				}
				if (color != null)
				{
					value.color = color;
				}
			}
			else
			{
				value = LuiPool.GetImage();
				if (png != null)
				{
					value.png = png;
				}
				if (color != null)
				{
					value.color = color;
				}
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetFillCenter(bool fill)
		{
			if (luiComponents.TryGetValue<LuiImageComp>(LuiCompType.Image, out var value))
			{
				value.fillCenter = fill;
			}
			else
			{
				value = LuiPool.GetImage();
				value.fillCenter = fill;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetImageSlice(string sliceValue)
		{
			if (luiComponents.TryGetValue<LuiImageComp>(LuiCompType.Image, out var value))
			{
				value.slice = sliceValue;
			}
			else
			{
				value = LuiPool.GetImage();
				value.slice = sliceValue;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetPpuMultiplier(float ppuMultiplier)
		{
			if (luiComponents.TryGetValue<LuiImageComp>(LuiCompType.Image, out var value))
			{
				value.ppuMultiplier = ppuMultiplier;
			}
			else
			{
				value = LuiPool.GetImage();
				value.ppuMultiplier = ppuMultiplier;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetItemIcon(int itemid, ulong skinid)
		{
			if (luiComponents.TryGetValue<LuiImageComp>(LuiCompType.Image, out var value))
			{
				value.itemid = itemid;
				value.skinid = skinid;
			}
			else
			{
				value = LuiPool.GetImage();
				value.itemid = itemid;
				value.skinid = skinid;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetUrlImage(string url = null, string color = null)
		{
			if (luiComponents.TryGetValue<LuiRawImageComp>(LuiCompType.RawImage, out var value))
			{
				if (url != null)
				{
					value.url = url;
				}
				if (color != null)
				{
					value.color = color;
				}
			}
			else
			{
				value = LuiPool.GetRawImage();
				if (url != null)
				{
					value.url = url;
				}
				if (color != null)
				{
					value.color = color;
				}
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetRawImage(string png = null, string color = null)
		{
			if (luiComponents.TryGetValue<LuiRawImageComp>(LuiCompType.RawImage, out var value))
			{
				if (png != null)
				{
					value.png = png;
				}
				if (color != null)
				{
					value.color = color;
				}
			}
			else
			{
				value = LuiPool.GetRawImage();
				if (png != null)
				{
					value.png = png;
				}
				if (color != null)
				{
					value.color = color;
				}
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetSteamIcon(string steamid, string color = null)
		{
			if (luiComponents.TryGetValue<LuiRawImageComp>(LuiCompType.RawImage, out var value))
			{
				value.steamid = steamid;
				if (!string.IsNullOrEmpty(color))
				{
					value.color = color;
				}
			}
			else
			{
				value = LuiPool.GetRawImage();
				value.steamid = steamid;
				if (!string.IsNullOrEmpty(color))
				{
					value.color = color;
				}
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetRawSprite(string sprite, string color = null)
		{
			if (luiComponents.TryGetValue<LuiRawImageComp>(LuiCompType.RawImage, out var value))
			{
				value.sprite = sprite;
				if (color != null)
				{
					value.color = color;
				}
			}
			else
			{
				value = LuiPool.GetRawImage();
				value.sprite = sprite;
				if (color != null)
				{
					value.color = color;
				}
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetRawMaterial(string material, string color = null)
		{
			if (luiComponents.TryGetValue<LuiRawImageComp>(LuiCompType.RawImage, out var value))
			{
				value.material = material;
				if (color != null)
				{
					value.color = color;
				}
			}
			else
			{
				value = LuiPool.GetRawImage();
				value.material = material;
				if (color != null)
				{
					value.color = color;
				}
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetButton(string command = null, string color = null)
		{
			if (luiComponents.TryGetValue<LuiButtonComp>(LuiCompType.Button, out var value))
			{
				if (command != null)
				{
					value.command = command;
				}
				if (color != null)
				{
					value.color = color;
				}
			}
			else
			{
				value = LuiPool.GetButton();
				if (command != null)
				{
					value.command = command;
				}
				if (color != null)
				{
					value.color = color;
				}
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetButtonColors(string color = null, string normalColor = null, string highlightedColor = null, string pressedColor = null, string selectedColor = null, string disabledColor = null, float colorMultiplier = -1f, float fadeDuration = -1f)
		{
			if (luiComponents.TryGetValue<LuiButtonComp>(LuiCompType.Button, out var value))
			{
				if (color != null)
				{
					value.color = color;
				}
				if (normalColor != null)
				{
					value.normalColor = normalColor;
				}
				if (highlightedColor != null)
				{
					value.highlightedColor = highlightedColor;
				}
				if (pressedColor != null)
				{
					value.pressedColor = pressedColor;
				}
				if (selectedColor != null)
				{
					value.selectedColor = selectedColor;
				}
				if (disabledColor != null)
				{
					value.disabledColor = disabledColor;
				}
				if (colorMultiplier != -1f)
				{
					value.colorMultiplier = colorMultiplier;
				}
				if (fadeDuration != -1f)
				{
					value.fadeDuration = fadeDuration;
				}
			}
			else
			{
				value = LuiPool.GetButton();
				if (color != null)
				{
					value.color = color;
				}
				if (normalColor != null)
				{
					value.normalColor = normalColor;
				}
				if (highlightedColor != null)
				{
					value.highlightedColor = highlightedColor;
				}
				if (pressedColor != null)
				{
					value.pressedColor = pressedColor;
				}
				if (selectedColor != null)
				{
					value.selectedColor = selectedColor;
				}
				if (disabledColor != null)
				{
					value.disabledColor = disabledColor;
				}
				if (colorMultiplier != -1f)
				{
					value.colorMultiplier = colorMultiplier;
				}
				if (fadeDuration != -1f)
				{
					value.fadeDuration = fadeDuration;
				}
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetButtonMaterial(string material)
		{
			if (luiComponents.TryGetValue<LuiButtonComp>(LuiCompType.Button, out var value))
			{
				value.material = material;
			}
			else
			{
				value = LuiPool.GetButton();
				value.material = material;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public unsafe LuiContainer SetButtonSprite(string sprite, Type imageType = (Type)0)
		{
			if (luiComponents.TryGetValue<LuiButtonComp>(LuiCompType.Button, out var value))
			{
				value.sprite = sprite;
				value.imageType = ((object)(*(Type*)(&imageType))/*cast due to constrained. prefix*/).ToString();
			}
			else
			{
				value = LuiPool.GetButton();
				value.sprite = sprite;
				value.imageType = ((object)(*(Type*)(&imageType))/*cast due to constrained. prefix*/).ToString();
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetButtonClose(string close)
		{
			if (luiComponents.TryGetValue<LuiButtonComp>(LuiCompType.Button, out var value))
			{
				value.close = close;
			}
			else
			{
				value = LuiPool.GetButton();
				value.close = close;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetButtonInteractable(bool interactable)
		{
			if (luiComponents.TryGetValue<LuiButtonComp>(LuiCompType.Button, out var value))
			{
				value.interactable = interactable;
			}
			else
			{
				value = LuiPool.GetButton();
				value.interactable = interactable;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetOutline(string color, Vector2 distance, bool useGraphicAlpha = false)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if (luiComponents.TryGetValue<LuiOutlineComp>(LuiCompType.Outline, out var value))
			{
				value.color = color;
				value.distance = distance;
				value.useGraphicAlpha = useGraphicAlpha;
			}
			else
			{
				value = LuiPool.GetOutline();
				value.color = color;
				value.distance = distance;
				value.useGraphicAlpha = useGraphicAlpha;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public unsafe LuiContainer SetInput(string color = null, string text = null, int fontSize = 0, string command = null, int charLimit = 0, CUI.Handler.FontTypes font = CUI.Handler.FontTypes.RobotoCondensedBold, TextAnchor alignment = (TextAnchor)4, bool update = false)
		{
			if (luiComponents.TryGetValue<LuiInputComp>(LuiCompType.InputField, out var value))
			{
				if (color != null)
				{
					value.color = color;
				}
				if (text != null)
				{
					value.text = text;
				}
				if (fontSize > 0)
				{
					value.fontSize = fontSize;
				}
				if (command != null)
				{
					value.command = command;
				}
				if (charLimit > 0)
				{
					value.characterLimit = charLimit;
				}
				if (!update)
				{
					value.align = ((object)(*(TextAnchor*)(&alignment))/*cast due to constrained. prefix*/).ToString();
					value.font = GetFont(font);
				}
			}
			else
			{
				value = LuiPool.GetInput();
				if (color != null)
				{
					value.color = color;
				}
				if (text != null)
				{
					value.text = text;
				}
				if (fontSize > 0)
				{
					value.fontSize = fontSize;
				}
				if (command != null)
				{
					value.command = command;
				}
				if (charLimit > 0)
				{
					value.characterLimit = charLimit;
				}
				if (!update)
				{
					value.align = ((object)(*(TextAnchor*)(&alignment))/*cast due to constrained. prefix*/).ToString();
					value.font = GetFont(font);
				}
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetInputReadOnly(bool readOnly)
		{
			if (luiComponents.TryGetValue<LuiInputComp>(LuiCompType.InputField, out var value))
			{
				value.readOnly = readOnly;
			}
			else
			{
				value = LuiPool.GetInput();
				value.readOnly = readOnly;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetInputPassword(bool password)
		{
			if (luiComponents.TryGetValue<LuiInputComp>(LuiCompType.InputField, out var value))
			{
				value.password = password;
			}
			else
			{
				value = LuiPool.GetInput();
				value.password = password;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetInputAutoFocus(bool autofocus)
		{
			if (luiComponents.TryGetValue<LuiInputComp>(LuiCompType.InputField, out var value))
			{
				value.autofocus = autofocus;
			}
			else
			{
				value = LuiPool.GetInput();
				value.autofocus = autofocus;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetInputKeyboard(bool needsKeyboard = false, bool hudMenuInput = false)
		{
			if (luiComponents.TryGetValue<LuiInputComp>(LuiCompType.InputField, out var value))
			{
				value.needsKeyboard = needsKeyboard;
				value.hudMenuInput = hudMenuInput;
			}
			else
			{
				value = LuiPool.GetInput();
				value.needsKeyboard = needsKeyboard;
				value.hudMenuInput = hudMenuInput;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public unsafe LuiContainer SetInputLineType(LineType lineType)
		{
			if (luiComponents.TryGetValue<LuiInputComp>(LuiCompType.InputField, out var value))
			{
				value.lineType = ((object)(*(LineType*)(&lineType))/*cast due to constrained. prefix*/).ToString();
			}
			else
			{
				value = LuiPool.GetInput();
				value.lineType = ((object)(*(LineType*)(&lineType))/*cast due to constrained. prefix*/).ToString();
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetInputPlaceholder(string placeholderId)
		{
			if (luiComponents.TryGetValue<LuiInputComp>(LuiCompType.InputField, out var value))
			{
				value.placeholderId = placeholderId;
			}
			else
			{
				value = LuiPool.GetInput();
				value.placeholderId = placeholderId;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetInputInteractable(bool interactable)
		{
			if (luiComponents.TryGetValue<LuiInputComp>(LuiCompType.InputField, out var value))
			{
				value.interactable = interactable;
			}
			else
			{
				value = LuiPool.GetInput();
				value.interactable = interactable;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer AddCursor()
		{
			if (!luiComponents.TryGetValue<LuiCursorComp>(LuiCompType.Button, out var value))
			{
				value = LuiPool.GetCursor();
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetAnchors(LuiPosition pos)
		{
			if (luiComponents.TryGetValue<LuiRectTransformComp>(LuiCompType.RectTransform, out var value))
			{
				value.anchor = pos;
			}
			else
			{
				value = LuiPool.GetRect();
				value.anchor = pos;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetOffset(LuiOffset off)
		{
			if (luiComponents.TryGetValue<LuiRectTransformComp>(LuiCompType.RectTransform, out var value))
			{
				value.offset = off;
			}
			else
			{
				value = LuiPool.GetRect();
				value.offset = off;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetRotation(float rotation)
		{
			if (luiComponents.TryGetValue<LuiRectTransformComp>(LuiCompType.RectTransform, out var value))
			{
				value.rotation = rotation;
			}
			else
			{
				value = LuiPool.GetRect();
				value.rotation = rotation;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetAnchorAndOffset(LuiPosition pos, LuiOffset off)
		{
			if (luiComponents.TryGetValue<LuiRectTransformComp>(LuiCompType.RectTransform, out var value))
			{
				value.anchor = pos;
				value.offset = off;
			}
			else
			{
				value = LuiPool.GetRect();
				value.anchor = pos;
				value.offset = off;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetRectParent(string setParent)
		{
			if (luiComponents.TryGetValue<LuiRectTransformComp>(LuiCompType.RectTransform, out var value))
			{
				value.setParent = setParent;
			}
			else
			{
				value = LuiPool.GetRect();
				value.setParent = setParent;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetRectIndex(int setTransformIndex)
		{
			if (luiComponents.TryGetValue<LuiRectTransformComp>(LuiCompType.RectTransform, out var value))
			{
				value.setTransformIndex = setTransformIndex;
			}
			else
			{
				value = LuiPool.GetRect();
				value.setTransformIndex = setTransformIndex;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetCountdown(float startTime, float endTime, float step = 1f, float interval = 1f, string command = null, string numberFormat = null)
		{
			if (luiComponents.TryGetValue<LuiCountdownComp>(LuiCompType.Countdown, out var value))
			{
				value.startTime = startTime;
				value.endTime = endTime;
				if (step != 1f)
				{
					value.step = step;
				}
				if (interval != 1f)
				{
					value.interval = interval;
				}
				if (command != null)
				{
					value.command = command;
				}
				if (numberFormat != null)
				{
					value.numberFormat = numberFormat;
				}
			}
			else
			{
				value = LuiPool.GetCountdown();
				value.startTime = startTime;
				value.endTime = endTime;
				if (step != 1f)
				{
					value.step = step;
				}
				if (interval != 1f)
				{
					value.interval = interval;
				}
				if (command != null)
				{
					value.command = command;
				}
				if (numberFormat != null)
				{
					value.numberFormat = numberFormat;
				}
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetCountdownDestroy(bool destroy)
		{
			if (luiComponents.TryGetValue<LuiCountdownComp>(LuiCompType.Countdown, out var value))
			{
				value.destroyIfDone = destroy;
			}
			else
			{
				value = LuiPool.GetCountdown();
				value.destroyIfDone = destroy;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetCountdownTimerFormat(TimerFormat format)
		{
			if (luiComponents.TryGetValue<LuiCountdownComp>(LuiCompType.Countdown, out var value))
			{
				value.timerFormat = format.ToString();
			}
			else
			{
				value = LuiPool.GetCountdown();
				value.timerFormat = format.ToString();
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetHorizontalLayoutSpacing(float spacing)
		{
			if (luiComponents.TryGetValue<LuiHorizontalLayoutGroupComp>(LuiCompType.HorizontalLayoutGroup, out var value))
			{
				value.spacing = spacing;
			}
			else
			{
				value = LuiPool.GetHorizontalLayoutGroup();
				value.spacing = spacing;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public unsafe LuiContainer SetHorizontalLayoutAlignment(TextAnchor anchor)
		{
			if (luiComponents.TryGetValue<LuiHorizontalLayoutGroupComp>(LuiCompType.HorizontalLayoutGroup, out var value))
			{
				value.childAlignment = ((object)(*(TextAnchor*)(&anchor))/*cast due to constrained. prefix*/).ToString();
			}
			else
			{
				value = LuiPool.GetHorizontalLayoutGroup();
				value.childAlignment = ((object)(*(TextAnchor*)(&anchor))/*cast due to constrained. prefix*/).ToString();
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetHorizontalLayoutForceExpand(bool width, bool height)
		{
			if (luiComponents.TryGetValue<LuiHorizontalLayoutGroupComp>(LuiCompType.HorizontalLayoutGroup, out var value))
			{
				value.childForceExpandWidth = width;
				value.childForceExpandHeight = height;
			}
			else
			{
				value = LuiPool.GetHorizontalLayoutGroup();
				value.childForceExpandWidth = width;
				value.childForceExpandHeight = height;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetHorizontalLayoutControl(bool width, bool height)
		{
			if (luiComponents.TryGetValue<LuiHorizontalLayoutGroupComp>(LuiCompType.HorizontalLayoutGroup, out var value))
			{
				value.childControlWidth = width;
				value.childControlHeight = height;
			}
			else
			{
				value = LuiPool.GetHorizontalLayoutGroup();
				value.childControlWidth = width;
				value.childControlHeight = height;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetHorizontalLayoutScale(bool width, bool height)
		{
			if (luiComponents.TryGetValue<LuiHorizontalLayoutGroupComp>(LuiCompType.HorizontalLayoutGroup, out var value))
			{
				value.childScaleWidth = width;
				value.childScaleHeight = height;
			}
			else
			{
				value = LuiPool.GetHorizontalLayoutGroup();
				value.childScaleWidth = width;
				value.childScaleHeight = height;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetHorizontalLayoutPadding(string padding)
		{
			if (luiComponents.TryGetValue<LuiHorizontalLayoutGroupComp>(LuiCompType.HorizontalLayoutGroup, out var value))
			{
				value.padding = padding;
			}
			else
			{
				value = LuiPool.GetHorizontalLayoutGroup();
				value.padding = padding;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetVerticalLayoutSpacing(float spacing)
		{
			if (luiComponents.TryGetValue<LuiVerticalLayoutGroupComp>(LuiCompType.VerticalLayoutGroup, out var value))
			{
				value.spacing = spacing;
			}
			else
			{
				value = LuiPool.GetVerticalLayoutGroup();
				value.spacing = spacing;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public unsafe LuiContainer SetVerticalLayoutAlignment(TextAnchor anchor)
		{
			if (luiComponents.TryGetValue<LuiVerticalLayoutGroupComp>(LuiCompType.VerticalLayoutGroup, out var value))
			{
				value.childAlignment = ((object)(*(TextAnchor*)(&anchor))/*cast due to constrained. prefix*/).ToString();
			}
			else
			{
				value = LuiPool.GetVerticalLayoutGroup();
				value.childAlignment = ((object)(*(TextAnchor*)(&anchor))/*cast due to constrained. prefix*/).ToString();
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetVerticalLayoutForceExpand(bool width, bool height)
		{
			if (luiComponents.TryGetValue<LuiVerticalLayoutGroupComp>(LuiCompType.VerticalLayoutGroup, out var value))
			{
				value.childForceExpandWidth = width;
				value.childForceExpandHeight = height;
			}
			else
			{
				value = LuiPool.GetVerticalLayoutGroup();
				value.childForceExpandWidth = width;
				value.childForceExpandHeight = height;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetVerticalLayoutControl(bool width, bool height)
		{
			if (luiComponents.TryGetValue<LuiVerticalLayoutGroupComp>(LuiCompType.VerticalLayoutGroup, out var value))
			{
				value.childControlWidth = width;
				value.childControlHeight = height;
			}
			else
			{
				value = LuiPool.GetVerticalLayoutGroup();
				value.childControlWidth = width;
				value.childControlHeight = height;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetVerticalLayoutScale(bool width, bool height)
		{
			if (luiComponents.TryGetValue<LuiVerticalLayoutGroupComp>(LuiCompType.VerticalLayoutGroup, out var value))
			{
				value.childScaleWidth = width;
				value.childScaleHeight = height;
			}
			else
			{
				value = LuiPool.GetVerticalLayoutGroup();
				value.childScaleWidth = width;
				value.childScaleHeight = height;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetVerticalLayoutPadding(string padding)
		{
			if (luiComponents.TryGetValue<LuiVerticalLayoutGroupComp>(LuiCompType.VerticalLayoutGroup, out var value))
			{
				value.padding = padding;
			}
			else
			{
				value = LuiPool.GetVerticalLayoutGroup();
				value.padding = padding;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetCellSize(Vector2 size)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (luiComponents.TryGetValue<LuiGridLayoutGroupComp>(LuiCompType.GridLayoutGroup, out var value))
			{
				value.cellSize = size;
			}
			else
			{
				value = LuiPool.GetGridLayoutGroup();
				value.cellSize = size;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetCellSpacing(Vector2 spacing)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (luiComponents.TryGetValue<LuiGridLayoutGroupComp>(LuiCompType.GridLayoutGroup, out var value))
			{
				value.spacing = spacing;
			}
			else
			{
				value = LuiPool.GetGridLayoutGroup();
				value.spacing = spacing;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public unsafe LuiContainer SetStartCorner(Corner corner)
		{
			if (luiComponents.TryGetValue<LuiGridLayoutGroupComp>(LuiCompType.GridLayoutGroup, out var value))
			{
				value.startCorner = ((object)(*(Corner*)(&corner))/*cast due to constrained. prefix*/).ToString();
			}
			else
			{
				value = LuiPool.GetGridLayoutGroup();
				value.startCorner = ((object)(*(Corner*)(&corner))/*cast due to constrained. prefix*/).ToString();
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public unsafe LuiContainer SetStartAxis(Axis axis)
		{
			if (luiComponents.TryGetValue<LuiGridLayoutGroupComp>(LuiCompType.GridLayoutGroup, out var value))
			{
				value.startAxis = ((object)(*(Axis*)(&axis))/*cast due to constrained. prefix*/).ToString();
			}
			else
			{
				value = LuiPool.GetGridLayoutGroup();
				value.startAxis = ((object)(*(Axis*)(&axis))/*cast due to constrained. prefix*/).ToString();
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public unsafe LuiContainer SetChildAlign(TextAnchor align)
		{
			if (luiComponents.TryGetValue<LuiGridLayoutGroupComp>(LuiCompType.GridLayoutGroup, out var value))
			{
				value.childAlignment = ((object)(*(TextAnchor*)(&align))/*cast due to constrained. prefix*/).ToString();
			}
			else
			{
				value = LuiPool.GetGridLayoutGroup();
				value.childAlignment = ((object)(*(TextAnchor*)(&align))/*cast due to constrained. prefix*/).ToString();
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public unsafe LuiContainer SetContraint(Constraint constraint)
		{
			if (luiComponents.TryGetValue<LuiGridLayoutGroupComp>(LuiCompType.GridLayoutGroup, out var value))
			{
				value.constraint = ((object)(*(Constraint*)(&constraint))/*cast due to constrained. prefix*/).ToString();
			}
			else
			{
				value = LuiPool.GetGridLayoutGroup();
				value.constraint = ((object)(*(Constraint*)(&constraint))/*cast due to constrained. prefix*/).ToString();
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetContraintCount(int count)
		{
			if (luiComponents.TryGetValue<LuiGridLayoutGroupComp>(LuiCompType.GridLayoutGroup, out var value))
			{
				value.constraintCount = count;
			}
			else
			{
				value = LuiPool.GetGridLayoutGroup();
				value.constraintCount = count;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetGridLayoutPadding(string padding)
		{
			if (luiComponents.TryGetValue<LuiGridLayoutGroupComp>(LuiCompType.GridLayoutGroup, out var value))
			{
				value.padding = padding;
			}
			else
			{
				value = LuiPool.GetGridLayoutGroup();
				value.padding = padding;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public unsafe LuiContainer SetFitMode(FitMode horizontalFit, FitMode verticalFit)
		{
			if (luiComponents.TryGetValue<LuiContentSizeFitterComp>(LuiCompType.ContentSizeFitter, out var value))
			{
				value.horizontalFit = ((object)(*(FitMode*)(&horizontalFit))/*cast due to constrained. prefix*/).ToString();
				value.verticalFit = ((object)(*(FitMode*)(&verticalFit))/*cast due to constrained. prefix*/).ToString();
			}
			else
			{
				value = LuiPool.GetContentSizeFitter();
				value.horizontalFit = ((object)(*(FitMode*)(&horizontalFit))/*cast due to constrained. prefix*/).ToString();
				value.verticalFit = ((object)(*(FitMode*)(&verticalFit))/*cast due to constrained. prefix*/).ToString();
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetPrefferedSize(float width = -1f, float height = -1f)
		{
			if (luiComponents.TryGetValue<LuiLayoutElementComp>(LuiCompType.LayoutElement, out var value))
			{
				if (width != -1f)
				{
					value.preferredWidth = width;
				}
				if (height != -1f)
				{
					value.preferredHeight = height;
				}
			}
			else
			{
				value = LuiPool.GetLayoutElement();
				if (width != -1f)
				{
					value.preferredWidth = width;
				}
				if (height != -1f)
				{
					value.preferredHeight = height;
				}
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetMinimalSize(float width = -1f, float height = -1f)
		{
			if (luiComponents.TryGetValue<LuiLayoutElementComp>(LuiCompType.LayoutElement, out var value))
			{
				if (width != -1f)
				{
					value.minWidth = width;
				}
				if (height != -1f)
				{
					value.minHeight = height;
				}
			}
			else
			{
				value = LuiPool.GetLayoutElement();
				if (width != -1f)
				{
					value.minWidth = width;
				}
				if (height != -1f)
				{
					value.minHeight = height;
				}
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetFlexible(float width = -1f, float height = -1f)
		{
			if (luiComponents.TryGetValue<LuiLayoutElementComp>(LuiCompType.LayoutElement, out var value))
			{
				if (width != -1f)
				{
					value.flexibleWidth = width;
				}
				if (height != -1f)
				{
					value.flexibleHeight = height;
				}
			}
			else
			{
				value = LuiPool.GetLayoutElement();
				if (width != -1f)
				{
					value.flexibleWidth = width;
				}
				if (height != -1f)
				{
					value.flexibleHeight = height;
				}
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetIgnoreLayout(bool ignore)
		{
			if (luiComponents.TryGetValue<LuiLayoutElementComp>(LuiCompType.LayoutElement, out var value))
			{
				value.ignoreLayout = ignore;
			}
			else
			{
				value = LuiPool.GetLayoutElement();
				value.ignoreLayout = ignore;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetDraggable(string filter = null, bool dropAnywhere = true, bool keepOnTop = false, bool limitToParent = false, float maxDistance = -1f, bool allowSwapping = false)
		{
			if (luiComponents.TryGetValue<LuiDraggableComp>(LuiCompType.Draggable, out var value))
			{
				if (filter != null)
				{
					value.filter = filter;
				}
				if (value.maxDistance != -1f)
				{
					value.maxDistance = maxDistance;
				}
				value.dropAnywhere = dropAnywhere;
				value.keepOnTop = keepOnTop;
				value.limitToParent = limitToParent;
				value.allowSwapping = allowSwapping;
			}
			else
			{
				value = LuiPool.GetDraggable();
				if (filter != null)
				{
					value.filter = filter;
				}
				if (value.maxDistance != -1f)
				{
					value.maxDistance = maxDistance;
				}
				value.dropAnywhere = dropAnywhere;
				value.keepOnTop = keepOnTop;
				value.limitToParent = limitToParent;
				value.allowSwapping = allowSwapping;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetDragAlpha(float alpha)
		{
			if (luiComponents.TryGetValue<LuiDraggableComp>(LuiCompType.Draggable, out var value))
			{
				value.dragAlpha = alpha;
			}
			else
			{
				value = LuiPool.GetDraggable();
				value.dragAlpha = alpha;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetParentLimitIndex(int index)
		{
			if (luiComponents.TryGetValue<LuiDraggableComp>(LuiCompType.Draggable, out var value))
			{
				value.parentLimitIndex = index;
			}
			else
			{
				value = LuiPool.GetDraggable();
				value.parentLimitIndex = index;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetDraggableParentPadding(Vector2 padding)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (luiComponents.TryGetValue<LuiDraggableComp>(LuiCompType.Draggable, out var value))
			{
				value.parentPadding = padding;
			}
			else
			{
				value = LuiPool.GetDraggable();
				value.parentPadding = padding;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetDraggableAnchorOffset(Vector2 offset)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (luiComponents.TryGetValue<LuiDraggableComp>(LuiCompType.Draggable, out var value))
			{
				value.anchorOffset = offset;
			}
			else
			{
				value = LuiPool.GetDraggable();
				value.anchorOffset = offset;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public unsafe LuiContainer SetDraggableRPC(DraggablePositionSendType posSendType)
		{
			if (luiComponents.TryGetValue<LuiDraggableComp>(LuiCompType.Draggable, out var value))
			{
				value.positionRPC = ((object)(*(DraggablePositionSendType*)(&posSendType))/*cast due to constrained. prefix*/).ToString();
			}
			else
			{
				value = LuiPool.GetDraggable();
				value.positionRPC = ((object)(*(DraggablePositionSendType*)(&posSendType))/*cast due to constrained. prefix*/).ToString();
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetSlot(string filter = null)
		{
			if (luiComponents.TryGetValue<LuiSlotComp>(LuiCompType.Slot, out var value))
			{
				if (filter != null)
				{
					value.filter = filter;
				}
			}
			else
			{
				value = LuiPool.GetSlot();
				if (filter != null)
				{
					value.filter = filter;
				}
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer AddKeyboard()
		{
			if (!luiComponents.TryGetValue<LuiKeyboardComp>(LuiCompType.Button, out var value))
			{
				value = LuiPool.GetKeyboard();
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public unsafe LuiContainer SetScrollView(bool vertical, bool horizontal, MovementType movementType = (MovementType)2, float elasticity = 0f, bool inertia = false, float decelerationRate = 0f, float scrollSensitivity = 0f, LuiScrollbar verticalScrollOptions = default(LuiScrollbar), LuiScrollbar horizontalScrollOptions = default(LuiScrollbar), bool update = false)
		{
			if (luiComponents.TryGetValue<LuiScrollComp>(LuiCompType.ScrollView, out var value))
			{
				if (!update)
				{
					value.vertical = vertical;
					value.horizontal = horizontal;
					value.movementType = ((object)(*(MovementType*)(&movementType))/*cast due to constrained. prefix*/).ToString();
					value.inertia = inertia;
				}
				if (elasticity != 0f)
				{
					value.elasticity = elasticity;
				}
				if (decelerationRate != 0f)
				{
					value.decelerationRate = decelerationRate;
				}
				if (scrollSensitivity != 0f)
				{
					value.scrollSensitivity = scrollSensitivity;
				}
				value.verticalScrollbar = verticalScrollOptions;
				value.horizontalScrollbar = horizontalScrollOptions;
			}
			else
			{
				value = LuiPool.GetScroll();
				if (!update)
				{
					value.vertical = vertical;
					value.horizontal = horizontal;
					value.movementType = ((object)(*(MovementType*)(&movementType))/*cast due to constrained. prefix*/).ToString();
					value.inertia = inertia;
				}
				if (elasticity != 0f)
				{
					value.elasticity = elasticity;
				}
				if (decelerationRate != 0f)
				{
					value.decelerationRate = decelerationRate;
				}
				if (scrollSensitivity != 0f)
				{
					value.scrollSensitivity = scrollSensitivity;
				}
				value.verticalScrollbar = verticalScrollOptions;
				value.horizontalScrollbar = horizontalScrollOptions;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetScrollContent(LuiPosition pos, LuiOffset offset)
		{
			if (luiComponents.TryGetValue<LuiScrollComp>(LuiCompType.ScrollView, out var value))
			{
				value.anchor = pos;
				value.offset = offset;
			}
			else
			{
				value = LuiPool.GetScroll();
				value.anchor = pos;
				value.offset = offset;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetScrollPivot(Vector2 pivot)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (luiComponents.TryGetValue<LuiScrollComp>(LuiCompType.ScrollView, out var value))
			{
				value.pivot = pivot;
			}
			else
			{
				value = LuiPool.GetScroll();
				value.pivot = pivot;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetScrollbarPosition(float horizontal = 0f, float vertical = 0f)
		{
			if (luiComponents.TryGetValue<LuiScrollComp>(LuiCompType.ScrollView, out var value))
			{
				if (horizontal != 0f)
				{
					value.horizontalNormalizedPosition = horizontal;
				}
				if (vertical != 0f)
				{
					value.verticalNormalizedPosition = vertical;
				}
			}
			else
			{
				value = LuiPool.GetScroll();
				if (horizontal != 0f)
				{
					value.horizontalNormalizedPosition = horizontal;
				}
				if (vertical != 0f)
				{
					value.verticalNormalizedPosition = vertical;
				}
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetCanvasGroup(float alpha = -1f, bool blocksRaycasts = true, bool interactable = true)
		{
			if (luiComponents.TryGetValue<LuiCanvasGroupComp>(LuiCompType.CanvasGroup, out var value))
			{
				if (alpha != -1f)
				{
					value.alpha = alpha;
				}
				value.blocksRaycasts = blocksRaycasts;
				value.interactable = interactable;
			}
			else
			{
				value = LuiPool.GetCanvasGroup();
				if (alpha != -1f)
				{
					value.alpha = alpha;
				}
				value.blocksRaycasts = blocksRaycasts;
				value.interactable = interactable;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetMask(bool showMaskGraphic = true)
		{
			if (luiComponents.TryGetValue<LuiMaskComp>(LuiCompType.Mask, out var value))
			{
				value.showMaskGraphic = showMaskGraphic;
			}
			else
			{
				value = LuiPool.GetMask();
				value.showMaskGraphic = showMaskGraphic;
				luiComponents.Add(value.type, value);
			}
			return this;
		}

		public LuiContainer SetTooltip(string text, TooltipType? tooltipType = null, string offset = null, bool useCentre = false, DelayType? delay = null, PositionMode? position = null)
		{
			if (luiComponents.TryGetValue<LuiTooltipComp>(LuiCompType.Tooltip, out var value))
			{
				value.text = text;
				value.tooltipType = tooltipType.ToString();
				value.offset = offset;
				value.useCentre = useCentre;
				value.delay = delay.ToString();
				value.position = position.ToString();
			}
			else
			{
				value = LuiPool.GetTooltip();
				value.text = text;
				value.tooltipType = tooltipType.ToString();
				value.offset = offset;
				value.useCentre = useCentre;
				value.delay = delay.ToString();
				value.position = position.ToString();
				luiComponents.Add(value.type, value);
			}
			return this;
		}
	}

	public readonly List<LuiContainer> elements = new List<LuiContainer>();

	private readonly CUI _parent;

	public bool generateNames = true;

	public string lastName = string.Empty;

	public static readonly Vector2 defaultPivot;

	public static readonly Vector2 defaultFade;

	public static readonly Vector2 defaultCellSize;

	private ImageDatabaseModule imgDb { get; }

	public LUI(CUI cui)
	{
		_parent = cui;
		imgDb = BaseModule.GetModule<ImageDatabaseModule>();
	}

	public LuiContainer CreateParent(CUI.ClientPanels parent, LuiPosition position, string name = "")
	{
		return CreateParent(_parent.GetClientPanel(parent), position, name);
	}

	public LuiContainer CreateParent(LuiContainer container, LuiPosition position, string name = "")
	{
		return CreateParent(container.name, position, name);
	}

	public LuiContainer CreateParent(string parent, LuiPosition position, string name = "")
	{
		LuiContainer container = LuiPool.GetContainer();
		container.parent = parent;
		if (name != string.Empty)
		{
			container.name = name;
		}
		else if (generateNames)
		{
			container.name = RandomEx.GetRandomString(4);
		}
		container.SetAnchors(position);
		elements.Add(container);
		return container;
	}

	public LuiContainer UpdatePosition(string name, LuiPosition pos)
	{
		LuiContainer container = LuiPool.GetContainer();
		container.name = name;
		container.update = true;
		container.SetAnchors(pos);
		elements.Add(container);
		return container;
	}

	public LuiContainer UpdatePosition(string name, LuiOffset off)
	{
		LuiContainer container = LuiPool.GetContainer();
		container.name = name;
		container.update = true;
		container.SetOffset(off);
		elements.Add(container);
		return container;
	}

	public LuiContainer UpdatePosition(string name, LuiPosition pos, LuiOffset off)
	{
		LuiContainer container = LuiPool.GetContainer();
		container.name = name;
		container.update = true;
		container.SetAnchorAndOffset(pos, off);
		elements.Add(container);
		return container;
	}

	public LuiContainer UpdateRotation(string name, float rotation)
	{
		LuiContainer container = LuiPool.GetContainer();
		container.name = name;
		container.update = true;
		container.SetRotation(rotation);
		elements.Add(container);
		return container;
	}

	public LuiContainer Update(string name)
	{
		LuiContainer container = LuiPool.GetContainer();
		container.name = name;
		container.update = true;
		elements.Add(container);
		return container;
	}

	public LuiContainer UpdateColor(string name, string color)
	{
		LuiContainer container = LuiPool.GetContainer();
		container.name = name;
		container.update = true;
		container.SetColor(color);
		elements.Add(container);
		return container;
	}

	public LuiContainer UpdateText(string name, string text, int fontSize = 0, string color = null)
	{
		LuiContainer container = LuiPool.GetContainer();
		container.name = name;
		container.update = true;
		container.SetText(text, fontSize, color, (TextAnchor)4, update: true);
		elements.Add(container);
		return container;
	}

	public LuiContainer UpdateButtonCommand(string name, string command, bool isProtected = true)
	{
		LuiContainer container = LuiPool.GetContainer();
		container.name = name;
		container.update = true;
		container.SetButton(isProtected ? Community.Protect(command) : command);
		elements.Add(container);
		return container;
	}

	public LuiContainer CreateEmptyContainer(LuiContainer container, string name = "", bool add = false)
	{
		return CreateEmptyContainer(container.name, name, add);
	}

	public LuiContainer CreateEmptyContainer(string parent, string name = "", bool add = false)
	{
		LuiContainer container = LuiPool.GetContainer();
		container.parent = parent;
		if (name != string.Empty)
		{
			container.name = name;
		}
		else if (generateNames)
		{
			container.name = (lastName = _parent.Manager.AppendId());
		}
		if (add)
		{
			elements.Add(container);
		}
		return container;
	}

	public LuiContainer CreatePanel(LuiContainer container, LuiPosition position, LuiOffset offset, string color, string name = "")
	{
		return CreatePanel(container.name, position, offset, color, name);
	}

	public LuiContainer CreatePanel(LuiContainer container, LuiOffset offset, string color, string name = "")
	{
		return CreatePanel(container.name, LuiPosition.None, offset, color, name);
	}

	public LuiContainer CreatePanel(string parent, LuiOffset offset, string color, string name = "")
	{
		return CreatePanel(parent, LuiPosition.None, offset, color, name);
	}

	public LuiContainer CreatePanel(string parent, LuiPosition position, LuiOffset offset, string color, string name = "")
	{
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetColor(color);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateText(LuiContainer container, LuiPosition position, LuiOffset offset, int fontSize, string color, string text, TextAnchor alignment = (TextAnchor)0, string name = "")
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		return CreateText(container.name, position, offset, fontSize, color, text, alignment, name);
	}

	public LuiContainer CreateText(LuiContainer container, LuiOffset offset, int fontSize, string color, string text, TextAnchor alignment = (TextAnchor)0, string name = "")
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return CreateText(container.name, LuiPosition.None, offset, fontSize, color, text, alignment, name);
	}

	public LuiContainer CreateText(string parent, LuiOffset offset, int fontSize, string color, string text, TextAnchor alignment = (TextAnchor)0, string name = "")
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return CreateText(parent, LuiPosition.None, offset, fontSize, color, text, alignment, name);
	}

	public LuiContainer CreateText(string parent, LuiPosition position, LuiOffset offset, int fontSize, string color, string text, TextAnchor alignment = (TextAnchor)4, string name = "")
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetText(text, fontSize, color, alignment);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateSprite(LuiContainer container, LuiPosition position, LuiOffset offset, string sprite, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		return CreateSprite(container.name, position, offset, sprite, color, name);
	}

	public LuiContainer CreateSprite(LuiContainer container, LuiOffset offset, string sprite, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		return CreateSprite(container.name, LuiPosition.None, offset, sprite, color, name);
	}

	public LuiContainer CreateSprite(string parent, LuiOffset offset, string sprite, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		return CreateSprite(parent, LuiPosition.None, offset, sprite, color, name);
	}

	public LuiContainer CreateSprite(string parent, LuiPosition position, LuiOffset offset, string sprite, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetSprite(sprite, color, (Type)0);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateImage(LuiContainer container, LuiPosition position, LuiOffset offset, string png, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		return CreateImage(container.name, position, offset, png, color, name);
	}

	public LuiContainer CreateImage(LuiContainer container, LuiOffset offset, string png, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		return CreateImage(container.name, LuiPosition.None, offset, png, color, name);
	}

	public LuiContainer CreateImage(string parent, LuiOffset offset, string png, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		return CreateImage(parent, LuiPosition.None, offset, png, color, name);
	}

	public LuiContainer CreateImage(string parent, LuiPosition position, LuiOffset offset, string png, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetImage(png, color);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateImageFromDb(LuiContainer container, LuiPosition position, LuiOffset offset, string dbName, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		return CreateImageFromDb(container.name, position, offset, dbName, color, name);
	}

	public LuiContainer CreateImageFromDb(LuiContainer container, LuiOffset offset, string dbName, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		return CreateImageFromDb(container.name, LuiPosition.None, offset, dbName, color, name);
	}

	public LuiContainer CreateImageFromDb(string parent, LuiOffset offset, string dbName, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		return CreateImageFromDb(parent, LuiPosition.None, offset, dbName, color, name);
	}

	public LuiContainer CreateImageFromDb(string parent, LuiPosition position, LuiOffset offset, string dbName, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		if (imgDb.HasImage(dbName))
		{
			luiContainer.SetAnchorAndOffset(position, offset);
			luiContainer.SetImage(imgDb.GetImageString(dbName), color);
			elements.Add(luiContainer);
			return luiContainer;
		}
		Logger.Warn("[LUI] You're trying to load an image from ImageDatabase '" + dbName + "' which doesn't exist. Ignoring.");
		return null;
	}

	public LuiContainer CreateRawImageFromDb(LuiContainer container, LuiPosition position, LuiOffset offset, string dbName, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		return CreateRawImageFromDb(container.name, position, offset, dbName, color, name);
	}

	public LuiContainer CreateRawImageFromDb(LuiContainer container, LuiOffset offset, string dbName, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		return CreateRawImageFromDb(container.name, LuiPosition.None, offset, dbName, color, name);
	}

	public LuiContainer CreateRawImageFromDb(string parent, LuiOffset offset, string dbName, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		return CreateRawImageFromDb(parent, LuiPosition.None, offset, dbName, color, name);
	}

	public LuiContainer CreateRawImageFromDb(string parent, LuiPosition position, LuiOffset offset, string dbName, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		if (imgDb.HasImage(dbName))
		{
			luiContainer.SetAnchorAndOffset(position, offset);
			luiContainer.SetRawImage(imgDb.GetImageString(dbName), color);
			elements.Add(luiContainer);
			return luiContainer;
		}
		Logger.Warn("[LUI] You're trying to load an image from ImageDatabase '" + dbName + "' which doesn't exist. Ignoring.");
		return null;
	}

	public LuiContainer CreateUrlImage(LuiContainer container, LuiPosition position, LuiOffset offset, string url, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		return CreateUrlImage(container.name, position, offset, url, color, name);
	}

	public LuiContainer CreateUrlImage(LuiContainer container, LuiOffset offset, string url, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		return CreateUrlImage(container.name, LuiPosition.None, offset, url, color, name);
	}

	public LuiContainer CreateUrlImage(string parent, LuiOffset offset, string url, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		return CreateUrlImage(parent, LuiPosition.None, offset, url, color, name);
	}

	public LuiContainer CreateUrlImage(string parent, LuiPosition position, LuiOffset offset, string url, string color = "1.0 1.0 1.0 1.0", string name = "")
	{
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetUrlImage(url, color);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateItemIcon(LuiContainer container, LuiPosition position, LuiOffset offset, string shortname, ulong skinId = 0uL, string color = "", string name = "")
	{
		return CreateItemIcon(container.name, position, offset, shortname, skinId, color, name);
	}

	public LuiContainer CreateItemIcon(LuiContainer container, LuiOffset offset, string shortname, ulong skinId = 0uL, string color = "", string name = "")
	{
		return CreateItemIcon(container.name, LuiPosition.None, offset, shortname, skinId, color, name);
	}

	public LuiContainer CreateItemIcon(string parent, LuiOffset offset, string shortname, ulong skinId = 0uL, string color = "", string name = "")
	{
		return CreateItemIcon(parent, LuiPosition.None, offset, shortname, skinId, color, name);
	}

	public LuiContainer CreateItemIcon(string parent, LuiPosition position, LuiOffset offset, string shortname, ulong skinId = 0uL, string color = "", string name = "")
	{
		ItemDefinition val = ItemManager.FindItemDefinition(shortname);
		if (Object.op_Implicit((Object)(object)val))
		{
			return CreateItemIcon(parent, position, offset, val.itemid, skinId, color, name);
		}
		Logger.Warn("[LUI] We couldn't find '" + shortname + "' as valid item shortname. Ignoring.");
		return null;
	}

	public LuiContainer CreateItemIcon(LuiContainer container, LuiPosition position, LuiOffset offset, int itemId, ulong skinId = 0uL, string color = "", string name = "")
	{
		return CreateItemIcon(container.name, position, offset, itemId, skinId, color, name);
	}

	public LuiContainer CreateItemIcon(LuiContainer container, LuiOffset offset, int itemId, ulong skinId = 0uL, string color = "", string name = "")
	{
		return CreateItemIcon(container.name, LuiPosition.None, offset, itemId, skinId, color, name);
	}

	public LuiContainer CreateItemIcon(string parent, LuiOffset offset, int itemId, ulong skinId = 0uL, string color = "", string name = "")
	{
		return CreateItemIcon(parent, LuiPosition.None, offset, itemId, skinId, color, name);
	}

	public LuiContainer CreateItemIcon(string parent, LuiPosition position, LuiOffset offset, int itemId, ulong skinId = 0uL, string color = "", string name = "")
	{
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetItemIcon(itemId, skinId);
		if (color != string.Empty)
		{
			luiContainer.SetColor(color);
		}
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateSteamAvatar(LuiContainer container, LuiPosition position, LuiOffset offset, string steamId, string color = null, string name = "")
	{
		return CreateSteamAvatar(container.name, position, offset, steamId, color, name);
	}

	public LuiContainer CreateSteamAvatar(LuiContainer container, LuiOffset offset, string steamId, string color = null, string name = "")
	{
		return CreateSteamAvatar(container.name, LuiPosition.None, offset, steamId, color, name);
	}

	public LuiContainer CreateSteamAvatar(string parent, LuiOffset offset, string steamId, string color = null, string name = "")
	{
		return CreateSteamAvatar(parent, LuiPosition.None, offset, steamId, color, name);
	}

	public LuiContainer CreateSteamAvatar(string parent, LuiPosition position, LuiOffset offset, string steamId, string color = null, string name = "")
	{
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetSteamIcon(steamId, color);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateButton(LuiContainer container, LuiPosition position, LuiOffset offset, string command, string color, bool isProtected = true, string name = "")
	{
		return CreateButton(container.name, position, offset, command, color, isProtected, name);
	}

	public LuiContainer CreateButton(LuiContainer container, LuiOffset offset, string command, string color, bool isProtected = true, string name = "")
	{
		return CreateButton(container.name, LuiPosition.None, offset, command, color, isProtected, name);
	}

	public LuiContainer CreateButton(string parent, LuiOffset offset, string command, string color, bool isProtected = true, string name = "")
	{
		return CreateButton(parent, LuiPosition.None, offset, command, color, isProtected, name);
	}

	public LuiContainer CreateButton(string parent, LuiPosition position, LuiOffset offset, string command, string color, bool isProtected = true, string name = "")
	{
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetButton(isProtected ? Community.Protect(command) : command, color);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateInput(LuiContainer container, LuiPosition position, LuiOffset offset, string color, string text, int fontSize, string command, int charLimit = 0, bool isProtected = true, CUI.Handler.FontTypes font = CUI.Handler.FontTypes.RobotoCondensedBold, TextAnchor alignment = (TextAnchor)0, string name = "")
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		return CreateInput(container.name, position, offset, color, text, fontSize, command, charLimit, isProtected, font, alignment, name);
	}

	public LuiContainer CreateInput(LuiContainer container, LuiOffset offset, string color, string text, int fontSize, string command, int charLimit = 0, bool isProtected = true, CUI.Handler.FontTypes font = CUI.Handler.FontTypes.RobotoCondensedBold, TextAnchor alignment = (TextAnchor)0, string name = "")
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		return CreateInput(container.name, LuiPosition.None, offset, color, text, fontSize, command, charLimit, isProtected, font, alignment, name);
	}

	public LuiContainer CreateInput(string parent, LuiOffset offset, string color, string text, int fontSize, string command, int charLimit = 0, bool isProtected = true, CUI.Handler.FontTypes font = CUI.Handler.FontTypes.RobotoCondensedBold, TextAnchor alignment = (TextAnchor)0, string name = "")
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return CreateInput(parent, LuiPosition.None, offset, color, text, fontSize, command, charLimit, isProtected, font, alignment, name);
	}

	public LuiContainer CreateInput(string parent, LuiPosition position, LuiOffset offset, string color, string text, int fontSize, string command, int charLimit = 0, bool isProtected = true, CUI.Handler.FontTypes font = CUI.Handler.FontTypes.RobotoCondensedBold, TextAnchor alignment = (TextAnchor)4, string name = "")
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetInput(color, text, fontSize, isProtected ? Community.Protect(command) : command, charLimit, font, alignment);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateCountdown(LuiContainer container, LuiPosition position, LuiOffset offset, int fontSize, string color, string text, TextAnchor alignment, float startTime, float endTime, float step = 1f, float interval = 1f, string command = null, bool isProtected = true, string name = "")
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		return CreateCountdown(container.name, position, offset, fontSize, color, text, alignment, startTime, endTime, step, interval, command, isProtected, name);
	}

	public LuiContainer CreateCountdown(LuiContainer container, LuiOffset offset, int fontSize, string color, string text, TextAnchor alignment, float startTime, float endTime, float step = 1f, float interval = 1f, string command = null, bool isProtected = true, string name = "")
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return CreateCountdown(container.name, LuiPosition.None, offset, fontSize, color, text, alignment, startTime, endTime, step, interval, command, isProtected, name);
	}

	public LuiContainer CreateCountdown(string parent, LuiOffset offset, int fontSize, string color, string text, TextAnchor alignment, float startTime, float endTime, float step = 1f, float interval = 1f, string command = null, bool isProtected = true, string name = "")
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return CreateCountdown(parent, LuiPosition.None, offset, fontSize, color, text, alignment, startTime, endTime, step, interval, command, isProtected, name);
	}

	public LuiContainer CreateCountdown(string parent, LuiPosition position, LuiOffset offset, int fontSize, string color, string text, TextAnchor alignment, float startTime, float endTime, float step = 1f, float interval = 1f, string command = null, bool isProtected = true, string name = "")
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetText(text, fontSize, color, alignment);
		luiContainer.SetCountdown(startTime, endTime, step, interval, isProtected ? Community.Protect(command) : command);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateHorizontalLayoutGroup(LuiContainer container, LuiPosition position, LuiOffset offset, float spacing = 0f, string name = "")
	{
		return CreateHorizontalLayoutGroup(container.name, position, offset, spacing, name);
	}

	public LuiContainer CreateHorizontalLayoutGroup(LuiContainer container, LuiOffset offset, float spacing = 0f, string name = "")
	{
		return CreateHorizontalLayoutGroup(container.name, LuiPosition.None, offset, spacing, name);
	}

	public LuiContainer CreateHorizontalLayoutGroup(string parent, LuiOffset offset, float spacing = 0f, string name = "")
	{
		return CreateHorizontalLayoutGroup(parent, LuiPosition.None, offset, spacing, name);
	}

	public LuiContainer CreateHorizontalLayoutGroup(string parent, LuiPosition position, LuiOffset offset, float spacing = 0f, string name = "")
	{
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetHorizontalLayoutSpacing(spacing);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateVerticalLayoutGroup(LuiContainer container, LuiPosition position, LuiOffset offset, float spacing = 0f, string name = "")
	{
		return CreateVerticalLayoutGroup(container.name, position, offset, spacing, name);
	}

	public LuiContainer CreateVerticalLayoutGroup(LuiContainer container, LuiOffset offset, float spacing = 0f, string name = "")
	{
		return CreateVerticalLayoutGroup(container.name, LuiPosition.None, offset, spacing, name);
	}

	public LuiContainer CreateVerticalLayoutGroup(string parent, LuiOffset offset, float spacing = 0f, string name = "")
	{
		return CreateVerticalLayoutGroup(parent, LuiPosition.None, offset, spacing, name);
	}

	public LuiContainer CreateVerticalLayoutGroup(string parent, LuiPosition position, LuiOffset offset, float spacing = 0f, string name = "")
	{
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetVerticalLayoutSpacing(spacing);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateGridLayoutGroup(LuiContainer container, LuiPosition position, LuiOffset offset, Vector2 cellSize, string name = "")
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return CreateGridLayoutGroup(container.name, position, offset, cellSize, name);
	}

	public LuiContainer CreateGridLayoutGroup(LuiContainer container, LuiOffset offset, Vector2 cellSize, string name = "")
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return CreateGridLayoutGroup(container.name, LuiPosition.None, offset, cellSize, name);
	}

	public LuiContainer CreateGridLayoutGroup(string parent, LuiOffset offset, Vector2 cellSize, string name = "")
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return CreateGridLayoutGroup(parent, LuiPosition.None, offset, cellSize, name);
	}

	public LuiContainer CreateGridLayoutGroup(string parent, LuiPosition position, LuiOffset offset, Vector2 cellSize, string name = "")
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetCellSize(cellSize);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateContentFitter(LuiContainer container, LuiPosition position, LuiOffset offset, FitMode horizontal, FitMode vertical, string name = "")
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return CreateContentFitter(container.name, position, offset, horizontal, vertical, name);
	}

	public LuiContainer CreateContentFitter(LuiContainer container, LuiOffset offset, FitMode horizontal, FitMode vertical, string name = "")
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return CreateContentFitter(container.name, LuiPosition.None, offset, horizontal, vertical, name);
	}

	public LuiContainer CreateContentFitter(string parent, LuiOffset offset, FitMode horizontal, FitMode vertical, string name = "")
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return CreateContentFitter(parent, LuiPosition.None, offset, horizontal, vertical, name);
	}

	public LuiContainer CreateContentFitter(string parent, LuiPosition position, LuiOffset offset, FitMode horizontal, FitMode vertical, string name = "")
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetFitMode(horizontal, vertical);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateLayoutElement(LuiContainer container, LuiPosition position, LuiOffset offset, float minWidth, float minHeight, string name = "")
	{
		return CreateLayoutElement(container.name, position, offset, minWidth, minHeight, name);
	}

	public LuiContainer CreateLayoutElement(LuiContainer container, LuiOffset offset, float minWidth, float minHeight, string name = "")
	{
		return CreateLayoutElement(container.name, LuiPosition.None, offset, minWidth, minHeight, name);
	}

	public LuiContainer CreateLayoutElement(string parent, LuiOffset offset, float minWidth, float minHeight, string name = "")
	{
		return CreateLayoutElement(parent, LuiPosition.None, offset, minWidth, minHeight, name);
	}

	public LuiContainer CreateLayoutElement(string parent, LuiPosition position, LuiOffset offset, float minWidth, float minHeight, string name = "")
	{
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetMinimalSize(minWidth, minHeight);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateDraggable(LuiContainer container, LuiPosition position, LuiOffset offset, string color, string filter = null, bool dropAnywhere = true, bool keepOnTop = false, bool limitToParent = false, float maxDistance = -1f, bool allowSwapping = false, string name = "")
	{
		return CreateDraggable(container.name, position, offset, color, filter, dropAnywhere, keepOnTop, limitToParent, maxDistance, allowSwapping, name);
	}

	public LuiContainer CreateDraggable(LuiContainer container, LuiOffset offset, string color, string filter = null, bool dropAnywhere = true, bool keepOnTop = false, bool limitToParent = false, float maxDistance = -1f, bool allowSwapping = false, string name = "")
	{
		return CreateDraggable(container.name, LuiPosition.None, offset, color, filter, dropAnywhere, keepOnTop, limitToParent, maxDistance, allowSwapping, name);
	}

	public LuiContainer CreateDraggable(string parent, LuiOffset offset, string color, string filter = null, bool dropAnywhere = true, bool keepOnTop = false, bool limitToParent = false, float maxDistance = -1f, bool allowSwapping = false, string name = "")
	{
		return CreateDraggable(parent, LuiPosition.None, offset, color, filter, dropAnywhere, keepOnTop, limitToParent, maxDistance, allowSwapping, name);
	}

	public LuiContainer CreateDraggable(string parent, LuiPosition position, LuiOffset offset, string color, string filter = null, bool dropAnywhere = true, bool keepOnTop = false, bool limitToParent = false, float maxDistance = -1f, bool allowSwapping = false, string name = "")
	{
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetColor(color);
		luiContainer.SetDraggable(filter, dropAnywhere, keepOnTop, limitToParent, maxDistance, allowSwapping);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateSlot(LuiContainer container, LuiPosition position, LuiOffset offset, string filter = null, string name = "")
	{
		return CreateSlot(container.name, position, offset, filter, name);
	}

	public LuiContainer CreateSlot(LuiContainer container, LuiOffset offset, string filter = null, string name = "")
	{
		return CreateSlot(container.name, LuiPosition.None, offset, filter, name);
	}

	public LuiContainer CreateSlot(string parent, LuiOffset offset, string filter = null, string name = "")
	{
		return CreateSlot(parent, LuiPosition.None, offset, filter, name);
	}

	public LuiContainer CreateSlot(string parent, LuiPosition position, LuiOffset offset, string filter = null, string name = "")
	{
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetSlot(filter);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateScrollView(LuiContainer container, LuiPosition position, LuiOffset offset, bool vertical, bool horizontal, MovementType movementType = (MovementType)2, float elasticity = 0f, bool inertia = false, float decelerationRate = 0f, float scrollSensitivity = 0f, LuiScrollbar verticalScrollOptions = default(LuiScrollbar), LuiScrollbar horizontalScrollOptions = default(LuiScrollbar), string name = "")
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return CreateScrollView(container.name, position, offset, vertical, horizontal, movementType, elasticity, inertia, decelerationRate, scrollSensitivity, verticalScrollOptions, horizontalScrollOptions, name);
	}

	public LuiContainer CreateScrollView(LuiContainer container, LuiOffset offset, bool vertical, bool horizontal, MovementType movementType = (MovementType)2, float elasticity = 0f, bool inertia = false, float decelerationRate = 0f, float scrollSensitivity = 0f, LuiScrollbar verticalScrollOptions = default(LuiScrollbar), LuiScrollbar horizontalScrollOptions = default(LuiScrollbar), string name = "")
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return CreateScrollView(container.name, LuiPosition.None, offset, vertical, horizontal, movementType, elasticity, inertia, decelerationRate, scrollSensitivity, verticalScrollOptions, horizontalScrollOptions, name);
	}

	public LuiContainer CreateScrollView(string parent, LuiOffset offset, bool vertical, bool horizontal, MovementType movementType = (MovementType)2, float elasticity = 0f, bool inertia = false, float decelerationRate = 0f, float scrollSensitivity = 0f, LuiScrollbar verticalScrollOptions = default(LuiScrollbar), LuiScrollbar horizontalScrollOptions = default(LuiScrollbar), string name = "")
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return CreateScrollView(parent, LuiPosition.None, offset, vertical, horizontal, movementType, elasticity, inertia, decelerationRate, scrollSensitivity, verticalScrollOptions, horizontalScrollOptions, name);
	}

	public LuiContainer CreateScrollView(string parent, LuiPosition position, LuiOffset offset, bool vertical, bool horizontal, MovementType movementType = (MovementType)2, float elasticity = 0f, bool inertia = false, float decelerationRate = 0f, float scrollSensitivity = 0f, LuiScrollbar verticalScrollOptions = default(LuiScrollbar), LuiScrollbar horizontalScrollOptions = default(LuiScrollbar), string name = "")
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetScrollView(vertical, horizontal, movementType, elasticity, inertia, decelerationRate, scrollSensitivity, verticalScrollOptions, horizontalScrollOptions);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateCanvasGroup(LuiContainer container, LuiPosition position, LuiOffset offset, float alpha = -1f, bool blocksRaycasts = true, bool interactable = true, string name = "")
	{
		return CreateCanvasGroup(container.name, position, offset, alpha, blocksRaycasts, interactable, name);
	}

	public LuiContainer CreateCanvasGroup(LuiContainer container, LuiOffset offset, float alpha = -1f, bool blocksRaycasts = true, bool interactable = true, string name = "")
	{
		return CreateCanvasGroup(container.name, LuiPosition.None, offset, alpha, blocksRaycasts, interactable, name);
	}

	public LuiContainer CreateCanvasGroup(string parent, LuiOffset offset, float alpha = -1f, bool blocksRaycasts = true, bool interactable = true, string name = "")
	{
		return CreateCanvasGroup(parent, LuiPosition.None, offset, alpha, blocksRaycasts, interactable, name);
	}

	public LuiContainer CreateCanvasGroup(string parent, LuiPosition position, LuiOffset offset, float alpha = -1f, bool blocksRaycasts = true, bool interactable = true, string name = "")
	{
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetCanvasGroup(alpha, blocksRaycasts, interactable);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateMask(LuiContainer container, LuiPosition position, LuiOffset offset, bool showMaskGraphic = true, string color = null, string name = "")
	{
		return CreateMask(container.name, position, offset, showMaskGraphic, color, name);
	}

	public LuiContainer CreateMask(LuiContainer container, LuiOffset offset, bool showMaskGraphic = true, string color = null, string name = "")
	{
		return CreateMask(container.name, LuiPosition.None, offset, showMaskGraphic, color, name);
	}

	public LuiContainer CreateMask(string parent, LuiOffset offset, bool showMaskGraphic = true, string color = null, string name = "")
	{
		return CreateMask(parent, LuiPosition.None, offset, showMaskGraphic, color, name);
	}

	public LuiContainer CreateMask(string parent, LuiPosition position, LuiOffset offset, bool showMaskGraphic = true, string color = null, string name = "")
	{
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		if (color != null)
		{
			luiContainer.SetColor(color);
		}
		luiContainer.SetMask(showMaskGraphic);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public LuiContainer CreateTooltip(LuiContainer container, LuiPosition position, LuiOffset offset, string text, TooltipType? tooltipType = null, string tooltipOffset = null, bool useCentre = false, DelayType? delay = null, PositionMode? positionMode = null, string name = "")
	{
		return CreateTooltip(container.name, position, offset, text, tooltipType, tooltipOffset, useCentre, delay, positionMode, name);
	}

	public LuiContainer CreateTooltip(LuiContainer container, LuiOffset offset, string text, TooltipType? tooltipType = null, string tooltipOffset = null, bool useCentre = false, DelayType? delay = null, PositionMode? positionMode = null, string name = "")
	{
		return CreateTooltip(container.name, LuiPosition.None, offset, text, tooltipType, tooltipOffset, useCentre, delay, positionMode, name);
	}

	public LuiContainer CreateTooltip(string parent, LuiOffset offset, string text, TooltipType? tooltipType = null, string tooltipOffset = null, bool useCentre = false, DelayType? delay = null, PositionMode? positionMode = null, string name = "")
	{
		return CreateTooltip(parent, LuiPosition.None, offset, text, tooltipType, tooltipOffset, useCentre, delay, positionMode, name);
	}

	public LuiContainer CreateTooltip(string parent, LuiPosition position, LuiOffset offset, string text, TooltipType? tooltipType = null, string tooltipOffset = null, bool useCentre = false, DelayType? delay = null, PositionMode? positionMode = null, string name = "")
	{
		LuiContainer luiContainer = CreateEmptyContainer(parent, name);
		luiContainer.SetAnchorAndOffset(position, offset);
		luiContainer.SetColor("0.0 0.0 0.0 0.0");
		luiContainer.SetTooltip(text, tooltipType, tooltipOffset, useCentre, delay, positionMode);
		elements.Add(luiContainer);
		return luiContainer;
	}

	public byte[] GetUiBytes()
	{
		using LuiBuilderInstance luiBuilderInstance = new LuiBuilderInstance(this);
		return luiBuilderInstance.GetMergedBytes();
	}

	public void SendUi(BasePlayer player)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Send(new SendInfo(player.Connection));
	}

	public void SendUiJson(BasePlayer player)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		SendJson(new SendInfo(player.Connection));
	}

	public void SendUiBytes(BasePlayer player, byte[] bytes)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		SendBytes(new SendInfo(player.Connection), bytes);
	}

	public string ToJson()
	{
		using LuiBuilderInstance luiBuilderInstance = new LuiBuilderInstance(this);
		return luiBuilderInstance.GetJsonString();
	}

	private void Send(SendInfo send)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		using LuiBuilderInstance luiBuilderInstance = new LuiBuilderInstance(this);
		NetWrite val = ((BaseNetwork)Net.sv).StartWrite();
		val.PacketID((Type)9);
		val.EntityID(((BaseNetworkable)CommunityEntity.ServerInstance).net.ID);
		val.UInt32(StringPool.Get("AddUI"));
		val.BytesWithSize((ReadOnlySpan<byte>)luiBuilderInstance.GetMergedBytes(), false);
		val.Send(send);
	}

	private void SendBytes(SendInfo send, byte[] bytes)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		NetWrite val = ((BaseNetwork)Net.sv).StartWrite();
		val.PacketID((Type)9);
		val.EntityID(((BaseNetworkable)CommunityEntity.ServerInstance).net.ID);
		val.UInt32(StringPool.Get("AddUI"));
		val.BytesWithSize((ReadOnlySpan<byte>)bytes, false);
		val.Send(send);
	}

	private void SendJson(SendInfo send)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		using LuiBuilderInstance luiBuilderInstance = new LuiBuilderInstance(this);
		NetWrite val = ((BaseNetwork)Net.sv).StartWrite();
		val.PacketID((Type)9);
		val.EntityID(((BaseNetworkable)CommunityEntity.ServerInstance).net.ID);
		val.UInt32(StringPool.Get("AddUI"));
		val.String(luiBuilderInstance.GetJsonString(), false);
		val.Send(send);
	}

	public void Dispose()
	{
		for (int i = 0; i < elements.Count; i++)
		{
			LuiContainer luiContainer = elements[i];
			foreach (LuiCompBase luiComponent in luiContainer.luiComponents)
			{
				LuiPool.ReturnComp(luiComponent);
			}
			LuiPool.ReturnContainer(luiContainer);
		}
		elements.Clear();
	}

	public static string GetFont(CUI.Handler.FontTypes type)
	{
		return type switch
		{
			CUI.Handler.FontTypes.RobotoCondensedBold => "robotocondensed-bold.ttf", 
			CUI.Handler.FontTypes.RobotoCondensedRegular => "robotocondensed-regular.ttf", 
			CUI.Handler.FontTypes.PermanentMarker => "permanentmarker.ttf", 
			CUI.Handler.FontTypes.DroidSansMono => "droidsansmono.ttf", 
			CUI.Handler.FontTypes.NotoSansArabicBold => "_nonenglish/notosanscjksc-bold.otf", 
			CUI.Handler.FontTypes.Poxel => "poxel.otf", 
			CUI.Handler.FontTypes.LCD => "lcd.ttf", 
			CUI.Handler.FontTypes.NoToEmoji => "_nonenglish/notoemoji-regular.ttf", 
			CUI.Handler.FontTypes.PressStart => "pressstart2p-regular.ttf", 
			_ => "robotocondensed-regular.ttf", 
		};
	}

	static LUI()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		defaultPivot = new Vector2(0.5f, 0.5f);
		defaultFade = new Vector2(0f, 1f);
		defaultCellSize = new Vector2(100f, 100f);
	}
}

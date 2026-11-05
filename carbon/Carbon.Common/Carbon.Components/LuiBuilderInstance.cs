using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Carbon.Components;

public struct LuiBuilderInstance : IDisposable
{
	private const int initialCapacity = 8192;

	internal char[] _charBuffer = ArrayPool<char>.Shared.Rent(8192);

	public int _charIndex = 0;

	internal int MaxUtf8ByteCount
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _charIndex * 3;
		}
	}

	private static string GetFieldName(LuiCompType type)
	{
		return type switch
		{
			LuiCompType.Text => "UnityEngine.UI.Text", 
			LuiCompType.Image => "UnityEngine.UI.Image", 
			LuiCompType.RawImage => "UnityEngine.UI.RawImage", 
			LuiCompType.Button => "UnityEngine.UI.Button", 
			LuiCompType.Outline => "UnityEngine.UI.Outline", 
			LuiCompType.InputField => "UnityEngine.UI.InputField", 
			LuiCompType.NeedsCursor => "NeedsCursor", 
			LuiCompType.RectTransform => "RectTransform", 
			LuiCompType.Countdown => "Countdown", 
			LuiCompType.HorizontalLayoutGroup => "UnityEngine.UI.HorizontalLayoutGroup", 
			LuiCompType.VerticalLayoutGroup => "UnityEngine.UI.VerticalLayoutGroup", 
			LuiCompType.GridLayoutGroup => "UnityEngine.UI.GridLayoutGroup", 
			LuiCompType.ContentSizeFitter => "UnityEngine.UI.ContentSizeFitter", 
			LuiCompType.LayoutElement => "UnityEngine.UI.LayoutElement", 
			LuiCompType.Draggable => "Draggable", 
			LuiCompType.Slot => "Slot", 
			LuiCompType.NeedsKeyboard => "NeedsKeyboard", 
			LuiCompType.ScrollView => "UnityEngine.UI.ScrollView", 
			LuiCompType.CanvasGroup => "UnityEngine.UI.CanvasGroup", 
			LuiCompType.Mask => "UnityEngine.UI.Mask", 
			LuiCompType.Tooltip => "Tooltip", 
			_ => "UnityEngine.UI.Image", 
		};
	}

	private static void WriteName(ref LuiBuilderInstance inst, LUI.LuiContainer element, bool comma)
	{
		if (element.nameId != 0)
		{
			if (comma)
			{
				inst.WriteFieldIdComma("name", element.nameIdentifier, element.nameId);
			}
			else
			{
				inst.WriteFieldId("name", element.nameIdentifier, element.nameId);
			}
		}
		else if (comma)
		{
			inst.WriteFieldComma("name", element._name);
		}
		else
		{
			inst.WriteField("name", element._name);
		}
	}

	private static void WriteParent(ref LuiBuilderInstance inst, LUI.LuiContainer element, bool comma)
	{
		if (element.parentId != 0)
		{
			if (comma)
			{
				inst.WriteFieldIdComma("parent", element.parentIdentifier, element.parentId);
			}
			else
			{
				inst.WriteFieldId("parent", element.parentIdentifier, element.parentId);
			}
		}
		else if (comma)
		{
			inst.WriteFieldComma("parent", element._parent);
		}
		else
		{
			inst.WriteField("parent", element._parent);
		}
	}

	public LuiBuilderInstance(LUI cui)
	{
		//IL_0bc8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bcd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0656: Unknown result type (might be due to invalid IL or missing references)
		//IL_065d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0663: Unknown result type (might be due to invalid IL or missing references)
		//IL_087c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0893: Unknown result type (might be due to invalid IL or missing references)
		//IL_0848: Unknown result type (might be due to invalid IL or missing references)
		//IL_085f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bf4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bfa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0be1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fd7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fdc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0674: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c0b: Unknown result type (might be due to invalid IL or missing references)
		//IL_100d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1024: Unknown result type (might be due to invalid IL or missing references)
		//IL_107f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1084: Unknown result type (might be due to invalid IL or missing references)
		//IL_1290: Unknown result type (might be due to invalid IL or missing references)
		//IL_1295: Unknown result type (might be due to invalid IL or missing references)
		//IL_105c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1043: Unknown result type (might be due to invalid IL or missing references)
		//IL_12ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_10b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_109f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ead: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eb4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ed7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ede: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ee4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ecb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ef5: Unknown result type (might be due to invalid IL or missing references)
		this.WriteStartArray();
		List<LUI.LuiContainer> elements = cui.elements;
		int count = elements.Count;
		for (int i = 0; i < count; i++)
		{
			LUI.LuiContainer luiContainer = elements[i];
			this.WriteStartObject();
			if (luiContainer.update)
			{
				WriteName(ref this, luiContainer, comma: false);
				if (luiContainer.HasParent)
				{
					WriteParent(ref this, luiContainer, comma: true);
				}
			}
			else
			{
				WriteParent(ref this, luiContainer, comma: false);
				if (luiContainer.HasName)
				{
					WriteName(ref this, luiContainer, comma: true);
				}
			}
			if (luiContainer.destroyUi != null)
			{
				this.WriteFieldComma("destroyUi", luiContainer.destroyUi);
			}
			if (luiContainer.fadeOut > 0f)
			{
				this.WriteFieldComma("fadeOut", luiContainer.fadeOut);
			}
			if (luiContainer.update)
			{
				this.WriteFieldComma("update", value: true);
			}
			if (luiContainer.activeSelfSet || !luiContainer.activeSelf)
			{
				this.WriteFieldComma("activeSelf", luiContainer.activeSelf);
			}
			if (luiContainer.luiComponents.Count > 0)
			{
				this.WriteComma();
				this.WriteStartArray("components");
				LuiComponentDictionary luiComponents = luiContainer.luiComponents;
				int count2 = luiComponents.Count;
				for (int j = 0; j < count2; j++)
				{
					LuiCompBase luiCompBase = luiComponents[j];
					this.WriteStartObject();
					this.WriteField("type", GetFieldName(luiCompBase.type));
					if (!luiCompBase.enabled)
					{
						this.WriteFieldComma("enabled", value: false);
					}
					if (luiCompBase.fadeIn > 0f)
					{
						this.WriteFieldComma("fadeIn", luiCompBase.fadeIn);
					}
					if (!luiCompBase.blocksRaycast)
					{
						this.WriteFieldComma("blocksRaycast", value: false);
					}
					if (luiCompBase.placeholderParentId != null)
					{
						this.WriteFieldComma("placeholderParentId", luiCompBase.placeholderParentId);
					}
					switch (luiCompBase.type)
					{
					case LuiCompType.Text:
					{
						LuiTextComp luiTextComp = luiCompBase as LuiTextComp;
						if (luiTextComp.text != null)
						{
							this.WriteFieldComma("text", luiTextComp.text);
						}
						if (luiTextComp.fontSize > 0)
						{
							this.WriteFieldComma("fontSize", luiTextComp.fontSize);
						}
						if (luiTextComp.font != null)
						{
							this.WriteFieldComma("font", luiTextComp.font);
						}
						if (luiTextComp.align != null)
						{
							this.WriteFieldComma("align", luiTextComp.align);
						}
						if (luiTextComp.color != null)
						{
							this.WriteFieldComma("color", luiTextComp.color);
						}
						if (luiTextComp.verticalOverflow != null)
						{
							this.WriteFieldComma("verticalOverflow", luiTextComp.verticalOverflow);
						}
						break;
					}
					case LuiCompType.Image:
					{
						LuiImageComp luiImageComp = luiCompBase as LuiImageComp;
						if (luiImageComp.sprite != null)
						{
							this.WriteFieldComma("sprite", luiImageComp.sprite);
						}
						if (luiImageComp.material != null)
						{
							this.WriteFieldComma("material", luiImageComp.material);
						}
						if (luiImageComp.color != null)
						{
							this.WriteFieldComma("color", luiImageComp.color);
						}
						if (luiImageComp.imageType != null)
						{
							this.WriteFieldComma("imagetype", luiImageComp.imageType);
						}
						if (luiImageComp.fillCenter)
						{
							this.WriteFieldComma("fillCenter", value: true);
						}
						if (luiImageComp.ppuMultiplier != -1f)
						{
							this.WriteFieldComma("ppuMultiplier", luiImageComp.ppuMultiplier);
						}
						if (luiImageComp.png != null)
						{
							this.WriteFieldComma("png", luiImageComp.png);
						}
						if (luiImageComp.slice != null)
						{
							this.WriteFieldComma("slice", luiImageComp.slice);
						}
						if (luiImageComp.itemid != 0)
						{
							this.WriteFieldComma("itemid", luiImageComp.itemid);
						}
						if (luiImageComp.skinid != 0L)
						{
							this.WriteFieldComma("skinid", luiImageComp.skinid);
						}
						break;
					}
					case LuiCompType.RawImage:
					{
						LuiRawImageComp luiRawImageComp = luiCompBase as LuiRawImageComp;
						if (luiRawImageComp.sprite != null)
						{
							this.WriteFieldComma("sprite", luiRawImageComp.sprite);
						}
						if (luiRawImageComp.color != null)
						{
							this.WriteFieldComma("color", luiRawImageComp.color);
						}
						if (luiRawImageComp.material != null)
						{
							this.WriteFieldComma("material", luiRawImageComp.material);
						}
						if (luiRawImageComp.url != null)
						{
							this.WriteFieldComma("url", luiRawImageComp.url);
						}
						if (luiRawImageComp.png != null)
						{
							this.WriteFieldComma("png", luiRawImageComp.png);
						}
						if (luiRawImageComp.steamid != null)
						{
							this.WriteFieldComma("steamid", luiRawImageComp.steamid);
						}
						break;
					}
					case LuiCompType.Button:
					{
						LuiButtonComp luiButtonComp = luiCompBase as LuiButtonComp;
						if (!luiButtonComp.interactable)
						{
							this.WriteFieldComma("interactable", value: false);
						}
						if (luiButtonComp.command != null)
						{
							this.WriteFieldComma("command", luiButtonComp.command);
						}
						if (luiButtonComp.close != null)
						{
							this.WriteFieldComma("close", luiButtonComp.close);
						}
						if (luiButtonComp.sprite != null)
						{
							this.WriteFieldComma("sprite", luiButtonComp.sprite);
						}
						if (luiButtonComp.material != null)
						{
							this.WriteFieldComma("material", luiButtonComp.material);
						}
						if (luiButtonComp.color != null)
						{
							this.WriteFieldComma("color", luiButtonComp.color);
						}
						if (luiButtonComp.imageType != null)
						{
							this.WriteFieldComma("imagetype", luiButtonComp.imageType);
						}
						if (luiButtonComp.normalColor != null)
						{
							this.WriteFieldComma("normalColor", luiButtonComp.normalColor);
						}
						if (luiButtonComp.highlightedColor != null)
						{
							this.WriteFieldComma("highlightedColor", luiButtonComp.highlightedColor);
						}
						if (luiButtonComp.pressedColor != null)
						{
							this.WriteFieldComma("pressedColor", luiButtonComp.pressedColor);
						}
						if (luiButtonComp.selectedColor != null)
						{
							this.WriteFieldComma("selectedColor", luiButtonComp.selectedColor);
						}
						if (luiButtonComp.disabledColor != null)
						{
							this.WriteFieldComma("disabledColor", luiButtonComp.disabledColor);
						}
						if (luiButtonComp.colorMultiplier != -1f)
						{
							this.WriteFieldComma("colorMultiplier", luiButtonComp.colorMultiplier);
						}
						if (luiButtonComp.fadeDuration != -1f)
						{
							this.WriteFieldComma("fadeDuration", luiButtonComp.fadeDuration);
						}
						break;
					}
					case LuiCompType.Outline:
					{
						LuiOutlineComp luiOutlineComp = luiCompBase as LuiOutlineComp;
						if (luiOutlineComp.color != null)
						{
							this.WriteFieldComma("color", luiOutlineComp.color);
						}
						if (luiOutlineComp.distance != default(Vector2))
						{
							this.WriteFieldComma("distance", luiOutlineComp.distance);
						}
						if (luiOutlineComp.useGraphicAlpha)
						{
							this.WriteFieldComma("useGraphicAlpha", value: true);
						}
						break;
					}
					case LuiCompType.InputField:
					{
						LuiInputComp luiInputComp = luiCompBase as LuiInputComp;
						if (luiInputComp.fontSize > 0)
						{
							this.WriteFieldComma("fontSize", luiInputComp.fontSize);
						}
						if (luiInputComp.font != null)
						{
							this.WriteFieldComma("font", luiInputComp.font);
						}
						if (luiInputComp.align != null)
						{
							this.WriteFieldComma("align", luiInputComp.align);
						}
						if (luiInputComp.color != null)
						{
							this.WriteFieldComma("color", luiInputComp.color);
						}
						if (luiInputComp.characterLimit > 0)
						{
							this.WriteFieldComma("characterLimit", luiInputComp.characterLimit);
						}
						if (luiInputComp.command != null)
						{
							this.WriteFieldComma("command", luiInputComp.command);
						}
						if (luiInputComp.lineType != null)
						{
							this.WriteFieldComma("lineType", luiInputComp.lineType);
						}
						if (luiInputComp.text != null)
						{
							this.WriteFieldComma("text", luiInputComp.text);
						}
						if (luiInputComp.readOnly)
						{
							this.WriteFieldComma("readOnly", value: true);
						}
						if (luiInputComp.placeholderId != null)
						{
							this.WriteFieldComma("placeholderId", luiInputComp.placeholderId);
						}
						if (luiInputComp.password)
						{
							this.WriteFieldComma("password", value: true);
						}
						if (luiInputComp.needsKeyboard)
						{
							this.WriteFieldComma("needsKeyboard", value: true);
						}
						if (luiInputComp.hudMenuInput)
						{
							this.WriteFieldComma("hudMenuInput", value: true);
						}
						if (luiInputComp.autofocus)
						{
							this.WriteFieldComma("autofocus", value: true);
						}
						if (!luiInputComp.interactable)
						{
							this.WriteFieldComma("interactable", value: false);
						}
						break;
					}
					case LuiCompType.RectTransform:
					{
						LuiRectTransformComp luiRectTransformComp = luiCompBase as LuiRectTransformComp;
						if (luiRectTransformComp.anchor != LuiPosition.Full)
						{
							this.WriteFieldComma("anchormin", luiRectTransformComp.anchor.anchorMin);
							this.WriteFieldComma("anchormax", luiRectTransformComp.anchor.anchorMax);
						}
						this.WriteComma();
						this.WriteField("offsetmin", luiRectTransformComp.offset.offsetMin);
						this.WriteFieldComma("offsetmax", luiRectTransformComp.offset.offsetMax);
						if (luiRectTransformComp.rotation != 0f)
						{
							this.WriteFieldComma("rotation", luiRectTransformComp.rotation);
						}
						if (luiRectTransformComp.setParent != null)
						{
							this.WriteFieldComma("setParent", luiRectTransformComp.setParent);
						}
						if (luiRectTransformComp.setTransformIndex != -1)
						{
							this.WriteFieldComma("setTransformIndex", luiRectTransformComp.setTransformIndex);
						}
						break;
					}
					case LuiCompType.Countdown:
					{
						LuiCountdownComp luiCountdownComp = luiCompBase as LuiCountdownComp;
						if (luiCountdownComp.endTime != -1f)
						{
							this.WriteFieldComma("endTime", luiCountdownComp.endTime);
						}
						if (luiCountdownComp.startTime != -1f)
						{
							this.WriteFieldComma("startTime", luiCountdownComp.startTime);
						}
						if (luiCountdownComp.step > 0f)
						{
							this.WriteFieldComma("step", luiCountdownComp.step);
						}
						if (luiCountdownComp.interval > 0f)
						{
							this.WriteFieldComma("interval", luiCountdownComp.interval);
						}
						if (luiCountdownComp.timerFormat != null)
						{
							this.WriteFieldComma("timerFormat", luiCountdownComp.timerFormat);
						}
						if (luiCountdownComp.numberFormat != null)
						{
							this.WriteFieldComma("numberFormat", luiCountdownComp.numberFormat);
						}
						if (!luiCountdownComp.destroyIfDone)
						{
							this.WriteFieldComma("destroyIfDone", value: false);
						}
						if (luiCountdownComp.command != null)
						{
							this.WriteFieldComma("command", luiCountdownComp.command);
						}
						break;
					}
					case LuiCompType.HorizontalLayoutGroup:
					{
						LuiHorizontalLayoutGroupComp luiHorizontalLayoutGroupComp = luiCompBase as LuiHorizontalLayoutGroupComp;
						if (luiHorizontalLayoutGroupComp.spacing != 0f)
						{
							this.WriteFieldComma("spacing", luiHorizontalLayoutGroupComp.spacing);
						}
						if (luiHorizontalLayoutGroupComp.childAlignment != null)
						{
							this.WriteFieldComma("childAlignment", luiHorizontalLayoutGroupComp.childAlignment);
						}
						if (!luiHorizontalLayoutGroupComp.childForceExpandWidth)
						{
							this.WriteFieldComma("childForceExpandWidth", value: false);
						}
						if (!luiHorizontalLayoutGroupComp.childForceExpandHeight)
						{
							this.WriteFieldComma("childForceExpandHeight", value: false);
						}
						if (luiHorizontalLayoutGroupComp.childControlWidth)
						{
							this.WriteFieldComma("childControlWidth", value: true);
						}
						if (luiHorizontalLayoutGroupComp.childControlHeight)
						{
							this.WriteFieldComma("childControlHeight", value: true);
						}
						if (luiHorizontalLayoutGroupComp.childScaleWidth)
						{
							this.WriteFieldComma("childScaleWidth", value: true);
						}
						if (luiHorizontalLayoutGroupComp.childScaleHeight)
						{
							this.WriteFieldComma("childScaleHeight", value: true);
						}
						if (luiHorizontalLayoutGroupComp.padding != null)
						{
							this.WriteFieldComma("padding", luiHorizontalLayoutGroupComp.padding);
						}
						break;
					}
					case LuiCompType.VerticalLayoutGroup:
					{
						LuiVerticalLayoutGroupComp luiVerticalLayoutGroupComp = luiCompBase as LuiVerticalLayoutGroupComp;
						if (luiVerticalLayoutGroupComp.spacing != 0f)
						{
							this.WriteFieldComma("spacing", luiVerticalLayoutGroupComp.spacing);
						}
						if (luiVerticalLayoutGroupComp.childAlignment != null)
						{
							this.WriteFieldComma("childAlignment", luiVerticalLayoutGroupComp.childAlignment);
						}
						if (!luiVerticalLayoutGroupComp.childForceExpandWidth)
						{
							this.WriteFieldComma("childForceExpandWidth", value: false);
						}
						if (!luiVerticalLayoutGroupComp.childForceExpandHeight)
						{
							this.WriteFieldComma("childForceExpandHeight", value: false);
						}
						if (luiVerticalLayoutGroupComp.childControlWidth)
						{
							this.WriteFieldComma("childControlWidth", value: true);
						}
						if (luiVerticalLayoutGroupComp.childControlHeight)
						{
							this.WriteFieldComma("childControlHeight", value: true);
						}
						if (luiVerticalLayoutGroupComp.childScaleWidth)
						{
							this.WriteFieldComma("childScaleWidth", value: true);
						}
						if (luiVerticalLayoutGroupComp.childScaleHeight)
						{
							this.WriteFieldComma("childScaleHeight", value: true);
						}
						if (luiVerticalLayoutGroupComp.padding != null)
						{
							this.WriteFieldComma("padding", luiVerticalLayoutGroupComp.padding);
						}
						break;
					}
					case LuiCompType.GridLayoutGroup:
					{
						LuiGridLayoutGroupComp luiGridLayoutGroupComp = luiCompBase as LuiGridLayoutGroupComp;
						if (luiGridLayoutGroupComp.cellSize != LUI.defaultCellSize)
						{
							this.WriteFieldComma("cellSize", luiGridLayoutGroupComp.cellSize);
						}
						if (luiGridLayoutGroupComp.spacing != default(Vector2))
						{
							this.WriteFieldComma("spacing", luiGridLayoutGroupComp.spacing);
						}
						if (luiGridLayoutGroupComp.startCorner != null)
						{
							this.WriteFieldComma("startCorner", luiGridLayoutGroupComp.startCorner);
						}
						if (luiGridLayoutGroupComp.startAxis != null)
						{
							this.WriteFieldComma("startAxis", luiGridLayoutGroupComp.startAxis);
						}
						if (luiGridLayoutGroupComp.childAlignment != null)
						{
							this.WriteFieldComma("childAlignment", luiGridLayoutGroupComp.childAlignment);
						}
						if (luiGridLayoutGroupComp.constraint != null)
						{
							this.WriteFieldComma("constraint", luiGridLayoutGroupComp.constraint);
						}
						if (luiGridLayoutGroupComp.constraintCount != 0)
						{
							this.WriteFieldComma("constraintCount", luiGridLayoutGroupComp.constraintCount);
						}
						if (luiGridLayoutGroupComp.padding != null)
						{
							this.WriteFieldComma("padding", luiGridLayoutGroupComp.padding);
						}
						break;
					}
					case LuiCompType.ContentSizeFitter:
					{
						LuiContentSizeFitterComp luiContentSizeFitterComp = luiCompBase as LuiContentSizeFitterComp;
						if (luiContentSizeFitterComp.horizontalFit != null)
						{
							this.WriteFieldComma("horizontalFit", luiContentSizeFitterComp.horizontalFit);
						}
						if (luiContentSizeFitterComp.verticalFit != null)
						{
							this.WriteFieldComma("verticalFit", luiContentSizeFitterComp.verticalFit);
						}
						break;
					}
					case LuiCompType.LayoutElement:
					{
						LuiLayoutElementComp luiLayoutElementComp = luiCompBase as LuiLayoutElementComp;
						if (luiLayoutElementComp.preferredWidth != -1f)
						{
							this.WriteFieldComma("preferredWidth", luiLayoutElementComp.preferredWidth);
						}
						if (luiLayoutElementComp.preferredHeight != -1f)
						{
							this.WriteFieldComma("preferredHeight", luiLayoutElementComp.preferredHeight);
						}
						if (luiLayoutElementComp.minWidth != 0f)
						{
							this.WriteFieldComma("minWidth", luiLayoutElementComp.minWidth);
						}
						if (luiLayoutElementComp.minHeight != 0f)
						{
							this.WriteFieldComma("minHeight", luiLayoutElementComp.minHeight);
						}
						if (luiLayoutElementComp.flexibleWidth != 0f)
						{
							this.WriteFieldComma("flexibleWidth", luiLayoutElementComp.flexibleWidth);
						}
						if (luiLayoutElementComp.flexibleHeight != 0f)
						{
							this.WriteFieldComma("flexibleHeight", luiLayoutElementComp.flexibleHeight);
						}
						if (luiLayoutElementComp.ignoreLayout)
						{
							this.WriteFieldComma("ignoreLayout", value: true);
						}
						break;
					}
					case LuiCompType.Draggable:
					{
						LuiDraggableComp luiDraggableComp = luiCompBase as LuiDraggableComp;
						if (luiDraggableComp.limitToParent)
						{
							this.WriteFieldComma("limitToParent", value: true);
						}
						if (luiDraggableComp.maxDistance > 0f)
						{
							this.WriteFieldComma("maxDistance", luiDraggableComp.maxDistance);
						}
						if (luiDraggableComp.allowSwapping)
						{
							this.WriteFieldComma("allowSwapping", value: true);
						}
						if (!luiDraggableComp.dropAnywhere)
						{
							this.WriteFieldComma("dropAnywhere", value: false);
						}
						if (luiDraggableComp.dragAlpha != -1f)
						{
							this.WriteFieldComma("dragAlpha", luiDraggableComp.dragAlpha);
						}
						if (luiDraggableComp.parentLimitIndex != -1)
						{
							this.WriteFieldComma("parentLimitIndex", luiDraggableComp.parentLimitIndex);
						}
						if (luiDraggableComp.filter != null)
						{
							this.WriteFieldComma("filter", luiDraggableComp.filter);
						}
						if (luiDraggableComp.parentPadding != default(Vector2))
						{
							this.WriteFieldComma("parentPadding", luiDraggableComp.parentPadding);
						}
						if (luiDraggableComp.anchorOffset != default(Vector2))
						{
							this.WriteFieldComma("anchorOffset", luiDraggableComp.anchorOffset);
						}
						if (luiDraggableComp.keepOnTop)
						{
							this.WriteFieldComma("keepOnTop", luiDraggableComp.keepOnTop);
						}
						if (luiDraggableComp.positionRPC != null)
						{
							this.WriteFieldComma("positionRPC", luiDraggableComp.positionRPC);
						}
						if (luiDraggableComp.moveToAnchor)
						{
							this.WriteFieldComma("moveToAnchor", luiDraggableComp.moveToAnchor);
						}
						if (luiDraggableComp.rebuildAnchor)
						{
							this.WriteFieldComma("rebuildAnchor", luiDraggableComp.rebuildAnchor);
						}
						break;
					}
					case LuiCompType.Slot:
					{
						LuiSlotComp luiSlotComp = luiCompBase as LuiSlotComp;
						if (luiSlotComp.filter != null)
						{
							this.WriteFieldComma("filter", luiSlotComp.filter);
						}
						break;
					}
					case LuiCompType.ScrollView:
					{
						LuiScrollComp luiScrollComp = luiCompBase as LuiScrollComp;
						bool flag = luiScrollComp.anchor != LuiPosition.Full;
						bool flag2 = luiScrollComp.offset != LuiOffset.None;
						if ((flag | flag2) || luiScrollComp.pivot != LUI.defaultPivot)
						{
							this.WriteComma();
							this.WriteStartObject("contentTransform");
							if (flag)
							{
								this.WriteField("anchormin", luiScrollComp.anchor.anchorMin);
								this.WriteFieldComma("anchormax", luiScrollComp.anchor.anchorMax);
							}
							if (flag2)
							{
								if (flag)
								{
									this.WriteFieldComma("offsetmin", luiScrollComp.offset.offsetMin);
								}
								else
								{
									this.WriteField("offsetmin", luiScrollComp.offset.offsetMin);
								}
								this.WriteFieldComma("offsetmax", luiScrollComp.offset.offsetMax);
							}
							if (luiScrollComp.pivot != LUI.defaultPivot)
							{
								if (flag | flag2)
								{
									this.WriteFieldComma("pivot", luiScrollComp.pivot);
								}
								else
								{
									this.WriteField("pivot", luiScrollComp.pivot);
								}
							}
							this.WriteEndObject();
						}
						if (luiScrollComp.horizontal)
						{
							this.WriteFieldComma("horizontal", value: true);
						}
						if (luiScrollComp.vertical)
						{
							this.WriteFieldComma("vertical", value: true);
						}
						if (luiScrollComp.movementType != null)
						{
							this.WriteFieldComma("movementType", luiScrollComp.movementType);
						}
						if (luiScrollComp.elasticity != -1f)
						{
							this.WriteFieldComma("elasticity", luiScrollComp.elasticity);
						}
						if (luiScrollComp.inertia)
						{
							this.WriteFieldComma("inertia", value: true);
						}
						if (luiScrollComp.decelerationRate != -1f)
						{
							this.WriteFieldComma("decelerationRate", luiScrollComp.decelerationRate);
						}
						if (luiScrollComp.scrollSensitivity != -1f)
						{
							this.WriteFieldComma("scrollSensitivity", luiScrollComp.scrollSensitivity);
						}
						if (luiScrollComp.horizontal && !luiScrollComp.horizontalScrollbar.disabled)
						{
							this.WriteComma();
							this.WriteStartObject("horizontalScrollbar");
							WriteScrollBar(luiScrollComp.horizontalScrollbar);
							this.WriteEndObject();
						}
						if (luiScrollComp.vertical && !luiScrollComp.verticalScrollbar.disabled)
						{
							this.WriteComma();
							this.WriteStartObject("verticalScrollbar");
							WriteScrollBar(luiScrollComp.verticalScrollbar);
							this.WriteEndObject();
						}
						if (luiScrollComp.horizontalNormalizedPosition != 0f)
						{
							this.WriteFieldComma("horizontalNormalizedPosition", luiScrollComp.horizontalNormalizedPosition);
						}
						if (luiScrollComp.verticalNormalizedPosition != 0f)
						{
							this.WriteFieldComma("verticalNormalizedPosition", luiScrollComp.verticalNormalizedPosition);
						}
						break;
					}
					case LuiCompType.CanvasGroup:
					{
						LuiCanvasGroupComp luiCanvasGroupComp = luiCompBase as LuiCanvasGroupComp;
						if (luiCanvasGroupComp.alpha != -1f)
						{
							this.WriteFieldComma("alpha", luiCanvasGroupComp.alpha);
						}
						if (!luiCanvasGroupComp.blocksRaycasts)
						{
							this.WriteFieldComma("blocksRaycasts", value: false);
						}
						if (!luiCanvasGroupComp.interactable)
						{
							this.WriteFieldComma("interactable", value: false);
						}
						if (luiCanvasGroupComp.fade != LUI.defaultFade)
						{
							this.WriteFieldComma("fade", luiCanvasGroupComp.fade);
						}
						break;
					}
					case LuiCompType.Mask:
					{
						LuiMaskComp luiMaskComp = luiCompBase as LuiMaskComp;
						if (!luiMaskComp.showMaskGraphic)
						{
							this.WriteFieldComma("showMaskGraphic", value: false);
						}
						break;
					}
					case LuiCompType.Tooltip:
					{
						LuiTooltipComp luiTooltipComp = luiCompBase as LuiTooltipComp;
						if (luiTooltipComp.tooltipType != null)
						{
							this.WriteFieldComma("tooltipType", luiTooltipComp.tooltipType);
						}
						if (luiTooltipComp.offset != null)
						{
							this.WriteFieldComma("offset", luiTooltipComp.offset);
						}
						if (luiTooltipComp.useCentre)
						{
							this.WriteFieldComma("useCentre", value: true);
						}
						if (luiTooltipComp.text != null)
						{
							this.WriteFieldComma("text", luiTooltipComp.text);
						}
						if (luiTooltipComp.delay != null)
						{
							this.WriteFieldComma("delay", luiTooltipComp.delay);
						}
						if (luiTooltipComp.position != null)
						{
							this.WriteFieldComma("position", luiTooltipComp.position);
						}
						break;
					}
					}
					this.WriteEndObject();
					if (j < count2 - 1)
					{
						this.WriteComma();
					}
				}
				this.WriteEndArray();
			}
			this.WriteEndObject();
			if (i < count - 1)
			{
				this.WriteComma();
			}
		}
		this.WriteEndArray();
	}

	private void WriteScrollBar(LuiScrollbar scroll)
	{
		this.WriteField("enabled", !scroll.disabled);
		if (scroll.invert)
		{
			this.WriteFieldComma("invert", value: true);
		}
		if (scroll.autoHide)
		{
			this.WriteFieldComma("autoHide", value: true);
		}
		if (scroll.handleSprite != null)
		{
			this.WriteFieldComma("handleSprite", scroll.handleSprite);
		}
		if (scroll.size != 0f)
		{
			this.WriteFieldComma("size", scroll.size);
		}
		if (scroll.handleColor != null)
		{
			this.WriteFieldComma("handleColor", scroll.handleColor);
		}
		if (scroll.highlightColor != null)
		{
			this.WriteFieldComma("highlightColor", scroll.highlightColor);
		}
		if (scroll.pressedColor != null)
		{
			this.WriteFieldComma("pressedColor", scroll.pressedColor);
		}
		if (scroll.trackSprite != null)
		{
			this.WriteFieldComma("trackSprite", scroll.trackSprite);
		}
		if (scroll.trackColor != null)
		{
			this.WriteFieldComma("trackColor", scroll.trackColor);
		}
	}

	internal unsafe int WriteUtf8To(Span<byte> destination)
	{
		if (_charIndex == 0)
		{
			return 0;
		}
		fixed (char* charBuffer = _charBuffer)
		{
			fixed (byte* bytes = destination)
			{
				return Encoding.UTF8.GetBytes(charBuffer, _charIndex, bytes, destination.Length);
			}
		}
	}

	public string GetJsonString()
	{
		return new string(_charBuffer, 0, _charIndex);
	}

	public byte[] GetMergedBytes()
	{
		byte[] array = new byte[Encoding.UTF8.GetByteCount(_charBuffer, 0, _charIndex)];
		Encoding.UTF8.GetBytes(_charBuffer, 0, _charIndex, array, 0);
		return array;
	}

	public void Dispose()
	{
		char[] charBuffer = _charBuffer;
		if (charBuffer != null)
		{
			_charBuffer = null;
			_charIndex = 0;
			ArrayPool<char>.Shared.Return(charBuffer);
		}
	}
}

using System;
using System.Buffers;
using System.Text;
using UnityEngine;

namespace Carbon.Components;

public struct LuiBuilderInstance : IDisposable
{
	private static int _segmentLimit = 100;

	private LUIBuilder.WriteArray[] _segments = ArrayPool<LUIBuilder.WriteArray>.Shared.Rent(_segmentLimit);

	private int _segmentCount = 0;

	private const int maxSegmentSize = 4096;

	public readonly char[] _charBuffer = new char[4096];

	public int _charIndex = 0;

	private string GetFieldName(LuiCompType type)
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
			_ => "UnityEngine.UI.Image", 
		};
	}

	public LuiBuilderInstance(LUI cui)
	{
		//IL_0e25: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0779: Unknown result type (might be due to invalid IL or missing references)
		//IL_0780: Unknown result type (might be due to invalid IL or missing references)
		//IL_0786: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a0d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e50: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e57: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e5d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e44: Unknown result type (might be due to invalid IL or missing references)
		//IL_1317: Unknown result type (might be due to invalid IL or missing references)
		//IL_131c: Unknown result type (might be due to invalid IL or missing references)
		//IL_079d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e74: Unknown result type (might be due to invalid IL or missing references)
		//IL_134d: Unknown result type (might be due to invalid IL or missing references)
		//IL_136a: Unknown result type (might be due to invalid IL or missing references)
		//IL_13b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_13bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_138f: Unknown result type (might be due to invalid IL or missing references)
		//IL_13ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_13de: Unknown result type (might be due to invalid IL or missing references)
		//IL_11ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_11b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_11b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_11dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_11e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_11e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_11d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1200: Unknown result type (might be due to invalid IL or missing references)
		this.WriteStartArray();
		int count = cui.elements.Count;
		int num = 0;
		foreach (LUI.LuiContainer element in cui.elements)
		{
			num++;
			this.WriteStartObject();
			if (element.update)
			{
				this.WriteField("name", element.name);
				if (!string.IsNullOrEmpty(element.parent))
				{
					this.WriteComma();
					this.WriteField("parent", element.parent);
				}
			}
			else
			{
				this.WriteField("parent", element.parent);
				if (!string.IsNullOrEmpty(element.name))
				{
					this.WriteComma();
					this.WriteField("name", element.name);
				}
			}
			if (element.destroyUi != null)
			{
				this.WriteComma();
				this.WriteField("destroyUi", element.destroyUi);
			}
			if (element.fadeOut > 0f)
			{
				this.WriteComma();
				this.WriteField("fadeOut", element.fadeOut);
			}
			if (element.update)
			{
				this.WriteComma();
				this.WriteField("update", value: true);
			}
			if (!element.activeSelf)
			{
				this.WriteComma();
				this.WriteField("activeSelf", value: false);
			}
			if (element.luiComponents.Count > 0)
			{
				this.WriteComma();
				this.WriteStartArray("components");
				int count2 = element.luiComponents.Count;
				int num2 = 0;
				foreach (LuiCompBase luiComponent in element.luiComponents)
				{
					this.WriteStartObject();
					this.WriteField("type", GetFieldName(luiComponent.type));
					if (!luiComponent.enabled)
					{
						this.WriteComma();
						this.WriteField("enabled", value: false);
					}
					if (luiComponent.fadeIn > 0f)
					{
						this.WriteComma();
						this.WriteField("fadeIn", luiComponent.fadeIn);
					}
					if (luiComponent.placeholderParentId != null)
					{
						this.WriteComma();
						this.WriteField("placeholderParentId", luiComponent.placeholderParentId);
					}
					switch (luiComponent.type)
					{
					case LuiCompType.Text:
					{
						LuiTextComp luiTextComp = luiComponent as LuiTextComp;
						num2++;
						if (luiTextComp.text != null)
						{
							this.WriteComma();
							this.WriteField("text", luiTextComp.text);
						}
						if (luiTextComp.fontSize > 0)
						{
							this.WriteComma();
							this.WriteField("fontSize", luiTextComp.fontSize);
						}
						if (luiTextComp.font != null)
						{
							this.WriteComma();
							this.WriteField("font", luiTextComp.font);
						}
						if (luiTextComp.align != null)
						{
							this.WriteComma();
							this.WriteField("align", luiTextComp.align);
						}
						if (luiTextComp.color != null)
						{
							this.WriteComma();
							this.WriteField("color", luiTextComp.color);
						}
						if (luiTextComp.verticalOverflow != null)
						{
							this.WriteComma();
							this.WriteField("verticalOverflow", luiTextComp.verticalOverflow);
						}
						break;
					}
					case LuiCompType.Image:
					{
						LuiImageComp luiImageComp = luiComponent as LuiImageComp;
						num2++;
						if (luiImageComp.sprite != null)
						{
							this.WriteComma();
							this.WriteField("sprite", luiImageComp.sprite);
						}
						if (luiImageComp.material != null)
						{
							this.WriteComma();
							this.WriteField("material", luiImageComp.material);
						}
						if (luiImageComp.color != null)
						{
							this.WriteComma();
							this.WriteField("color", luiImageComp.color);
						}
						if (luiImageComp.imageType != null)
						{
							this.WriteComma();
							this.WriteField("imagetype", luiImageComp.imageType);
						}
						if (luiImageComp.fillCenter)
						{
							this.WriteComma();
							this.WriteField("fillCenter", value: true);
						}
						if (luiImageComp.png != null)
						{
							this.WriteComma();
							this.WriteField("png", luiImageComp.png);
						}
						if (luiImageComp.slice != null)
						{
							this.WriteComma();
							this.WriteField("slice", luiImageComp.slice);
						}
						if (luiImageComp.itemid != 0)
						{
							this.WriteComma();
							this.WriteField("itemid", luiImageComp.itemid);
						}
						if (luiImageComp.skinid != 0L)
						{
							this.WriteComma();
							this.WriteField("skinid", luiImageComp.skinid);
						}
						break;
					}
					case LuiCompType.RawImage:
					{
						LuiRawImageComp luiRawImageComp = luiComponent as LuiRawImageComp;
						num2++;
						if (luiRawImageComp.sprite != null)
						{
							this.WriteComma();
							this.WriteField("sprite", luiRawImageComp.sprite);
						}
						if (luiRawImageComp.color != null)
						{
							this.WriteComma();
							this.WriteField("color", luiRawImageComp.color);
						}
						if (luiRawImageComp.material != null)
						{
							this.WriteComma();
							this.WriteField("material", luiRawImageComp.material);
						}
						if (luiRawImageComp.url != null)
						{
							this.WriteComma();
							this.WriteField("url", luiRawImageComp.url);
						}
						if (luiRawImageComp.png != null)
						{
							this.WriteComma();
							this.WriteField("png", luiRawImageComp.png);
						}
						if (luiRawImageComp.steamid != null)
						{
							this.WriteComma();
							this.WriteField("steamid", luiRawImageComp.steamid);
						}
						break;
					}
					case LuiCompType.Button:
					{
						LuiButtonComp luiButtonComp = luiComponent as LuiButtonComp;
						num2++;
						if (luiButtonComp.command != null)
						{
							this.WriteComma();
							this.WriteField("command", luiButtonComp.command);
						}
						if (luiButtonComp.close != null)
						{
							this.WriteComma();
							this.WriteField("close", luiButtonComp.close);
						}
						if (luiButtonComp.sprite != null)
						{
							this.WriteComma();
							this.WriteField("sprite", luiButtonComp.sprite);
						}
						if (luiButtonComp.material != null)
						{
							this.WriteComma();
							this.WriteField("material", luiButtonComp.material);
						}
						if (luiButtonComp.color != null)
						{
							this.WriteComma();
							this.WriteField("color", luiButtonComp.color);
						}
						if (luiButtonComp.imageType != null)
						{
							this.WriteComma();
							this.WriteField("imagetype", luiButtonComp.imageType);
						}
						if (luiButtonComp.normalColor != null)
						{
							this.WriteComma();
							this.WriteField("normalColor", luiButtonComp.normalColor);
						}
						if (luiButtonComp.highlightedColor != null)
						{
							this.WriteComma();
							this.WriteField("highlightedColor", luiButtonComp.highlightedColor);
						}
						if (luiButtonComp.pressedColor != null)
						{
							this.WriteComma();
							this.WriteField("pressedColor", luiButtonComp.pressedColor);
						}
						if (luiButtonComp.selectedColor != null)
						{
							this.WriteComma();
							this.WriteField("selectedColor", luiButtonComp.selectedColor);
						}
						if (luiButtonComp.disabledColor != null)
						{
							this.WriteComma();
							this.WriteField("disabledColor", luiButtonComp.disabledColor);
						}
						if (luiButtonComp.colorMultiplier != -1f)
						{
							this.WriteComma();
							this.WriteField("colorMultiplier", luiButtonComp.colorMultiplier);
						}
						if (luiButtonComp.fadeDuration != -1f)
						{
							this.WriteComma();
							this.WriteField("fadeDuration", luiButtonComp.fadeDuration);
						}
						break;
					}
					case LuiCompType.Outline:
					{
						LuiOutlineComp luiOutlineComp = luiComponent as LuiOutlineComp;
						num2++;
						if (luiOutlineComp.color != null)
						{
							this.WriteComma();
							this.WriteField("color", luiOutlineComp.color);
						}
						if (luiOutlineComp.distance != default(Vector2))
						{
							this.WriteComma();
							this.WriteField("distance", luiOutlineComp.distance);
						}
						if (luiOutlineComp.useGraphicAlpha)
						{
							this.WriteComma();
							this.WriteField("useGraphicAlpha", value: true);
						}
						break;
					}
					case LuiCompType.InputField:
					{
						LuiInputComp luiInputComp = luiComponent as LuiInputComp;
						num2++;
						if (luiInputComp.fontSize > 0)
						{
							this.WriteComma();
							this.WriteField("fontSize", luiInputComp.fontSize);
						}
						if (luiInputComp.font != null)
						{
							this.WriteComma();
							this.WriteField("font", luiInputComp.font);
						}
						if (luiInputComp.align != null)
						{
							this.WriteComma();
							this.WriteField("align", luiInputComp.align);
						}
						if (luiInputComp.color != null)
						{
							this.WriteComma();
							this.WriteField("color", luiInputComp.color);
						}
						if (luiInputComp.characterLimit > 0)
						{
							this.WriteComma();
							this.WriteField("characterLimit", luiInputComp.characterLimit);
						}
						if (luiInputComp.command != null)
						{
							this.WriteComma();
							this.WriteField("command", luiInputComp.command);
						}
						if (luiInputComp.lineType != null)
						{
							this.WriteComma();
							this.WriteField("lineType", luiInputComp.lineType);
						}
						if (luiInputComp.text != null)
						{
							this.WriteComma();
							this.WriteField("text", luiInputComp.text);
						}
						if (luiInputComp.readOnly)
						{
							this.WriteComma();
							this.WriteField("readOnly", value: true);
						}
						if (luiInputComp.placeholderId != null)
						{
							this.WriteComma();
							this.WriteField("placeholderId", luiInputComp.placeholderId);
						}
						if (luiInputComp.password)
						{
							this.WriteComma();
							this.WriteField("password", value: true);
						}
						if (luiInputComp.needsKeyboard)
						{
							this.WriteComma();
							this.WriteField("needsKeyboard", value: true);
						}
						if (luiInputComp.hudMenuInput)
						{
							this.WriteComma();
							this.WriteField("hudMenuInput", value: true);
						}
						if (luiInputComp.autofocus)
						{
							this.WriteComma();
							this.WriteField("autofocus", value: true);
						}
						break;
					}
					case LuiCompType.NeedsCursor:
						num2++;
						break;
					case LuiCompType.RectTransform:
					{
						LuiRectTransformComp luiRectTransformComp = luiComponent as LuiRectTransformComp;
						num2++;
						if (luiRectTransformComp.anchor != LuiPosition.Full)
						{
							this.WriteComma();
							this.WriteField("anchormin", luiRectTransformComp.anchor.anchorMin);
							this.WriteComma();
							this.WriteField("anchormax", luiRectTransformComp.anchor.anchorMax);
						}
						this.WriteComma();
						this.WriteField("offsetmin", luiRectTransformComp.offset.offsetMin);
						this.WriteComma();
						this.WriteField("offsetmax", luiRectTransformComp.offset.offsetMax);
						if (luiRectTransformComp.rotation != 0f)
						{
							this.WriteComma();
							this.WriteField("rotation", luiRectTransformComp.rotation);
						}
						if (luiRectTransformComp.setParent != null)
						{
							this.WriteComma();
							this.WriteField("setParent", luiRectTransformComp.setParent);
						}
						if (luiRectTransformComp.setTransformIndex != -1)
						{
							this.WriteComma();
							this.WriteField("setTransformIndex", luiRectTransformComp.setTransformIndex);
						}
						break;
					}
					case LuiCompType.Countdown:
					{
						LuiCountdownComp luiCountdownComp = luiComponent as LuiCountdownComp;
						num2++;
						if (luiCountdownComp.endTime != -1f)
						{
							this.WriteComma();
							this.WriteField("endTime", luiCountdownComp.endTime);
						}
						if (luiCountdownComp.startTime != -1f)
						{
							this.WriteComma();
							this.WriteField("startTime", luiCountdownComp.startTime);
						}
						if (luiCountdownComp.step > 0f)
						{
							this.WriteComma();
							this.WriteField("step", luiCountdownComp.step);
						}
						if (luiCountdownComp.interval > 0f)
						{
							this.WriteComma();
							this.WriteField("interval", luiCountdownComp.interval);
						}
						if (luiCountdownComp.timerFormat != null)
						{
							this.WriteComma();
							this.WriteField("timerFormat", luiCountdownComp.timerFormat);
						}
						if (luiCountdownComp.numberFormat != null)
						{
							this.WriteComma();
							this.WriteField("numberFormat", luiCountdownComp.numberFormat);
						}
						if (!luiCountdownComp.destroyIfDone)
						{
							this.WriteComma();
							this.WriteField("destroyIfDone", value: false);
						}
						if (luiCountdownComp.command != null)
						{
							this.WriteComma();
							this.WriteField("command", luiCountdownComp.command);
						}
						break;
					}
					case LuiCompType.HorizontalLayoutGroup:
					{
						LuiHorizontalLayoutGroupComp luiHorizontalLayoutGroupComp = luiComponent as LuiHorizontalLayoutGroupComp;
						num2++;
						if (luiHorizontalLayoutGroupComp.spacing != 0f)
						{
							this.WriteComma();
							this.WriteField("spacing", luiHorizontalLayoutGroupComp.spacing);
						}
						if (luiHorizontalLayoutGroupComp.childAlignment != null)
						{
							this.WriteComma();
							this.WriteField("childAlignment", luiHorizontalLayoutGroupComp.childAlignment);
						}
						if (!luiHorizontalLayoutGroupComp.childForceExpandWidth)
						{
							this.WriteComma();
							this.WriteField("childForceExpandWidth", value: false);
						}
						if (!luiHorizontalLayoutGroupComp.childForceExpandHeight)
						{
							this.WriteComma();
							this.WriteField("childForceExpandHeight", value: false);
						}
						if (luiHorizontalLayoutGroupComp.childControlWidth)
						{
							this.WriteComma();
							this.WriteField("childControlWidth", value: true);
						}
						if (luiHorizontalLayoutGroupComp.childControlHeight)
						{
							this.WriteComma();
							this.WriteField("childControlHeight", value: true);
						}
						if (luiHorizontalLayoutGroupComp.childScaleWidth)
						{
							this.WriteComma();
							this.WriteField("childScaleWidth", value: true);
						}
						if (luiHorizontalLayoutGroupComp.childScaleHeight)
						{
							this.WriteComma();
							this.WriteField("childScaleHeight", value: true);
						}
						if (luiHorizontalLayoutGroupComp.padding != null)
						{
							this.WriteComma();
							this.WriteField("padding", luiHorizontalLayoutGroupComp.padding);
						}
						break;
					}
					case LuiCompType.VerticalLayoutGroup:
					{
						LuiVerticalLayoutGroupComp luiVerticalLayoutGroupComp = luiComponent as LuiVerticalLayoutGroupComp;
						num2++;
						if (luiVerticalLayoutGroupComp.spacing != 0f)
						{
							this.WriteComma();
							this.WriteField("spacing", luiVerticalLayoutGroupComp.spacing);
						}
						if (luiVerticalLayoutGroupComp.childAlignment != null)
						{
							this.WriteComma();
							this.WriteField("childAlignment", luiVerticalLayoutGroupComp.childAlignment);
						}
						if (!luiVerticalLayoutGroupComp.childForceExpandWidth)
						{
							this.WriteComma();
							this.WriteField("childForceExpandWidth", value: false);
						}
						if (!luiVerticalLayoutGroupComp.childForceExpandHeight)
						{
							this.WriteComma();
							this.WriteField("childForceExpandHeight", value: false);
						}
						if (luiVerticalLayoutGroupComp.childControlWidth)
						{
							this.WriteComma();
							this.WriteField("childControlWidth", value: true);
						}
						if (luiVerticalLayoutGroupComp.childControlHeight)
						{
							this.WriteComma();
							this.WriteField("childControlHeight", value: true);
						}
						if (luiVerticalLayoutGroupComp.childScaleWidth)
						{
							this.WriteComma();
							this.WriteField("childScaleWidth", value: true);
						}
						if (luiVerticalLayoutGroupComp.childScaleHeight)
						{
							this.WriteComma();
							this.WriteField("childScaleHeight", value: true);
						}
						if (luiVerticalLayoutGroupComp.padding != null)
						{
							this.WriteComma();
							this.WriteField("padding", luiVerticalLayoutGroupComp.padding);
						}
						break;
					}
					case LuiCompType.GridLayoutGroup:
					{
						LuiGridLayoutGroupComp luiGridLayoutGroupComp = luiComponent as LuiGridLayoutGroupComp;
						num2++;
						if (luiGridLayoutGroupComp.cellSize != LUI.defaultCellSize)
						{
							this.WriteComma();
							this.WriteField("cellSize", luiGridLayoutGroupComp.cellSize);
						}
						if (luiGridLayoutGroupComp.spacing != default(Vector2))
						{
							this.WriteComma();
							this.WriteField("spacing", luiGridLayoutGroupComp.spacing);
						}
						if (luiGridLayoutGroupComp.startCorner != null)
						{
							this.WriteComma();
							this.WriteField("startCorner", luiGridLayoutGroupComp.startCorner);
						}
						if (luiGridLayoutGroupComp.startAxis != null)
						{
							this.WriteComma();
							this.WriteField("startAxis", luiGridLayoutGroupComp.startAxis);
						}
						if (luiGridLayoutGroupComp.childAlignment != null)
						{
							this.WriteComma();
							this.WriteField("childAlignment", luiGridLayoutGroupComp.childAlignment);
						}
						if (luiGridLayoutGroupComp.constraint != null)
						{
							this.WriteComma();
							this.WriteField("constraint", luiGridLayoutGroupComp.constraint);
						}
						if (luiGridLayoutGroupComp.constraintCount != 0)
						{
							this.WriteComma();
							this.WriteField("constraintCount", luiGridLayoutGroupComp.constraintCount);
						}
						if (luiGridLayoutGroupComp.padding != null)
						{
							this.WriteComma();
							this.WriteField("padding", luiGridLayoutGroupComp.padding);
						}
						break;
					}
					case LuiCompType.ContentSizeFitter:
					{
						LuiContentSizeFitterComp luiContentSizeFitterComp = luiComponent as LuiContentSizeFitterComp;
						num2++;
						if (luiContentSizeFitterComp.horizontalFit != null)
						{
							this.WriteComma();
							this.WriteField("horizontalFit", luiContentSizeFitterComp.horizontalFit);
						}
						if (luiContentSizeFitterComp.verticalFit != null)
						{
							this.WriteComma();
							this.WriteField("verticalFit", luiContentSizeFitterComp.verticalFit);
						}
						break;
					}
					case LuiCompType.LayoutElement:
					{
						LuiLayoutElementComp luiLayoutElementComp = luiComponent as LuiLayoutElementComp;
						num2++;
						if (luiLayoutElementComp.preferredWidth != -1f)
						{
							this.WriteComma();
							this.WriteField("preferredWidth", luiLayoutElementComp.preferredWidth);
						}
						if (luiLayoutElementComp.preferredHeight != -1f)
						{
							this.WriteComma();
							this.WriteField("preferredHeight", luiLayoutElementComp.preferredHeight);
						}
						if (luiLayoutElementComp.minWidth != 0f)
						{
							this.WriteComma();
							this.WriteField("minWidth", luiLayoutElementComp.minWidth);
						}
						if (luiLayoutElementComp.minHeight != 0f)
						{
							this.WriteComma();
							this.WriteField("minHeight", luiLayoutElementComp.minHeight);
						}
						if (luiLayoutElementComp.flexibleWidth != 0f)
						{
							this.WriteComma();
							this.WriteField("flexibleWidth", luiLayoutElementComp.flexibleWidth);
						}
						if (luiLayoutElementComp.flexibleHeight != 0f)
						{
							this.WriteComma();
							this.WriteField("flexibleHeight", luiLayoutElementComp.flexibleHeight);
						}
						if (luiLayoutElementComp.ignoreLayout)
						{
							this.WriteComma();
							this.WriteField("ignoreLayout", value: true);
						}
						break;
					}
					case LuiCompType.Draggable:
					{
						LuiDraggableComp luiDraggableComp = luiComponent as LuiDraggableComp;
						num2++;
						if (luiDraggableComp.limitToParent)
						{
							this.WriteComma();
							this.WriteField("limitToParent", value: true);
						}
						if (luiDraggableComp.maxDistance > 0f)
						{
							this.WriteComma();
							this.WriteField("maxDistance", luiDraggableComp.maxDistance);
						}
						if (luiDraggableComp.allowSwapping)
						{
							this.WriteComma();
							this.WriteField("allowSwapping", value: true);
						}
						if (!luiDraggableComp.dropAnywhere)
						{
							this.WriteComma();
							this.WriteField("dropAnywhere", value: false);
						}
						if (luiDraggableComp.dragAlpha != -1f)
						{
							this.WriteComma();
							this.WriteField("dragAlpha", luiDraggableComp.dragAlpha);
						}
						if (luiDraggableComp.parentLimitIndex != -1)
						{
							this.WriteComma();
							this.WriteField("parentLimitIndex", luiDraggableComp.parentLimitIndex);
						}
						if (luiDraggableComp.filter != null)
						{
							this.WriteComma();
							this.WriteField("filter", luiDraggableComp.filter);
						}
						if (luiDraggableComp.parentPadding != default(Vector2))
						{
							this.WriteComma();
							this.WriteField("parentPadding", luiDraggableComp.parentPadding);
						}
						if (luiDraggableComp.anchorOffset != default(Vector2))
						{
							this.WriteComma();
							this.WriteField("anchorOffset", luiDraggableComp.anchorOffset);
						}
						if (luiDraggableComp.keepOnTop)
						{
							this.WriteComma();
							this.WriteField("keepOnTop", luiDraggableComp.keepOnTop);
						}
						if (luiDraggableComp.positionRPC != null)
						{
							this.WriteComma();
							this.WriteField("positionRPC", luiDraggableComp.positionRPC);
						}
						if (luiDraggableComp.moveToAnchor)
						{
							this.WriteComma();
							this.WriteField("moveToAnchor", luiDraggableComp.moveToAnchor);
						}
						if (luiDraggableComp.rebuildAnchor)
						{
							this.WriteComma();
							this.WriteField("rebuildAnchor", luiDraggableComp.rebuildAnchor);
						}
						break;
					}
					case LuiCompType.Slot:
					{
						LuiSlotComp luiSlotComp = luiComponent as LuiSlotComp;
						num2++;
						if (luiSlotComp.filter != null)
						{
							this.WriteComma();
							this.WriteField("filter", luiSlotComp.filter);
						}
						break;
					}
					case LuiCompType.NeedsKeyboard:
						num2++;
						break;
					case LuiCompType.ScrollView:
					{
						LuiScrollComp luiScrollComp = luiComponent as LuiScrollComp;
						num2++;
						bool flag = luiScrollComp.anchor != LuiPosition.Full;
						bool flag2 = luiScrollComp.offset != LuiOffset.None;
						if (flag || flag2 || luiScrollComp.pivot != LUI.defaultPivot)
						{
							this.WriteComma();
							this.WriteStartObject("contentTransform");
							if (flag)
							{
								this.WriteField("anchormin", luiScrollComp.anchor.anchorMin);
								this.WriteComma();
								this.WriteField("anchormax", luiScrollComp.anchor.anchorMax);
							}
							if (flag2)
							{
								if (flag)
								{
									this.WriteComma();
								}
								this.WriteField("offsetmin", luiScrollComp.offset.offsetMin);
								this.WriteComma();
								this.WriteField("offsetmax", luiScrollComp.offset.offsetMax);
							}
							if (luiScrollComp.pivot != LUI.defaultPivot)
							{
								if (flag || flag2)
								{
									this.WriteComma();
								}
								this.WriteField("pivot", luiScrollComp.pivot);
							}
							this.WriteEndObject();
						}
						if (luiScrollComp.horizontal)
						{
							this.WriteComma();
							this.WriteField("horizontal", value: true);
						}
						if (luiScrollComp.vertical)
						{
							this.WriteComma();
							this.WriteField("vertical", value: true);
						}
						if (luiScrollComp.movementType != null)
						{
							this.WriteComma();
							this.WriteField("movementType", luiScrollComp.movementType);
						}
						if (luiScrollComp.elasticity != -1f)
						{
							this.WriteComma();
							this.WriteField("elasticity", luiScrollComp.elasticity);
						}
						if (luiScrollComp.inertia)
						{
							this.WriteComma();
							this.WriteField("inertia", value: true);
						}
						if (luiScrollComp.decelerationRate != -1f)
						{
							this.WriteComma();
							this.WriteField("decelerationRate", luiScrollComp.decelerationRate);
						}
						if (luiScrollComp.scrollSensitivity != -1f)
						{
							this.WriteComma();
							this.WriteField("scrollSensitivity", luiScrollComp.scrollSensitivity);
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
							this.WriteComma();
							this.WriteField("horizontalNormalizedPosition", luiScrollComp.horizontalNormalizedPosition);
						}
						if (luiScrollComp.verticalNormalizedPosition != 0f)
						{
							this.WriteComma();
							this.WriteField("verticalNormalizedPosition", luiScrollComp.verticalNormalizedPosition);
						}
						break;
					}
					}
					this.WriteEndObject();
					if (num2 < count2)
					{
						this.WriteComma();
					}
				}
				this.WriteEndArray();
			}
			this.WriteEndObject();
			if (num < count)
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
			this.WriteComma();
			this.WriteField("invert", value: true);
		}
		if (scroll.autoHide)
		{
			this.WriteComma();
			this.WriteField("autoHide", value: true);
		}
		if (scroll.handleSprite != null)
		{
			this.WriteComma();
			this.WriteField("handleSprite", scroll.handleSprite);
		}
		if (scroll.size != 0f)
		{
			this.WriteComma();
			this.WriteField("size", scroll.size);
		}
		if (scroll.handleColor != null)
		{
			this.WriteComma();
			this.WriteField("handleColor", scroll.handleColor);
		}
		if (scroll.highlightColor != null)
		{
			this.WriteComma();
			this.WriteField("highlightColor", scroll.highlightColor);
		}
		if (scroll.pressedColor != null)
		{
			this.WriteComma();
			this.WriteField("pressedColor", scroll.pressedColor);
		}
		if (scroll.trackSprite != null)
		{
			this.WriteComma();
			this.WriteField("trackSprite", scroll.trackSprite);
		}
		if (scroll.trackColor != null)
		{
			this.WriteComma();
			this.WriteField("trackColor", scroll.trackColor);
		}
	}

	public string GetJsonString()
	{
		byte[] mergedBytes = GetMergedBytes();
		return Encoding.UTF8.GetString(mergedBytes);
	}

	public byte[] GetMergedBytes()
	{
		Flush();
		int num = 0;
		for (int i = 0; i < _segmentCount; i++)
		{
			num += _segments[i].size;
		}
		byte[] array = new byte[num];
		int num2 = 0;
		for (int j = 0; j < _segmentCount; j++)
		{
			LUIBuilder.WriteArray writeArray = _segments[j];
			writeArray.values.AsSpan(0, writeArray.size).CopyTo(array.AsSpan(num2));
			num2 += writeArray.size;
		}
		return array;
	}

	public void Flush()
	{
		if (_charIndex != 0)
		{
			byte[] array = ArrayPool<byte>.Shared.Rent(4096);
			int bytes = Encoding.UTF8.GetBytes(_charBuffer, 0, _charIndex, array, 0);
			if (_segmentCount == _segmentLimit)
			{
				_segmentLimit *= 2;
				LUIBuilder.WriteArray[] array2 = ArrayPool<LUIBuilder.WriteArray>.Shared.Rent(_segmentLimit);
				Array.Copy(_segments, array2, _segmentCount);
				ArrayPool<LUIBuilder.WriteArray>.Shared.Return(_segments, clearArray: true);
				_segments = array2;
				_segmentCount = 0;
			}
			_segments[_segmentCount++] = new LUIBuilder.WriteArray(array, bytes);
			_charIndex = 0;
		}
	}

	public void Dispose()
	{
		_charIndex = 0;
		for (int i = 0; i < _segmentCount; i++)
		{
			ArrayPool<byte>.Shared.Return(_segments[i].values);
		}
		ArrayPool<LUIBuilder.WriteArray>.Shared.Return(_segments, clearArray: true);
	}
}

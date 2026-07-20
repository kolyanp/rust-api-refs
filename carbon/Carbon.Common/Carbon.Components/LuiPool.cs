using System;
using System.Collections.Generic;
using UnityEngine;

namespace Carbon.Components;

public static class LuiPool
{
	private static readonly Stack<LUI.LuiContainer> _containers = new Stack<LUI.LuiContainer>();

	private static readonly Stack<LuiCompBase> _texts = new Stack<LuiCompBase>();

	private static readonly Stack<LuiCompBase> _images = new Stack<LuiCompBase>();

	private static readonly Stack<LuiCompBase> _rawImages = new Stack<LuiCompBase>();

	private static readonly Stack<LuiCompBase> _buttons = new Stack<LuiCompBase>();

	private static readonly Stack<LuiCompBase> _outlines = new Stack<LuiCompBase>();

	private static readonly Stack<LuiCompBase> _inputs = new Stack<LuiCompBase>();

	private static readonly Stack<LuiCompBase> _cursors = new Stack<LuiCompBase>();

	private static readonly Stack<LuiCompBase> _rects = new Stack<LuiCompBase>();

	private static readonly Stack<LuiCompBase> _countdowns = new Stack<LuiCompBase>();

	private static readonly Stack<LuiCompBase> _horizontals = new Stack<LuiCompBase>();

	private static readonly Stack<LuiCompBase> _verticals = new Stack<LuiCompBase>();

	private static readonly Stack<LuiCompBase> _grids = new Stack<LuiCompBase>();

	private static readonly Stack<LuiCompBase> _fitters = new Stack<LuiCompBase>();

	private static readonly Stack<LuiCompBase> _layouts = new Stack<LuiCompBase>();

	private static readonly Stack<LuiCompBase> _draggables = new Stack<LuiCompBase>();

	private static readonly Stack<LuiCompBase> _slots = new Stack<LuiCompBase>();

	private static readonly Stack<LuiCompBase> _keyboards = new Stack<LuiCompBase>();

	private static readonly Stack<LuiCompBase> _scrolls = new Stack<LuiCompBase>();

	public static int poolElements => _containers.Count + _texts.Count + _images.Count + _rawImages.Count + _buttons.Count + _outlines.Count + _inputs.Count + _cursors.Count + _rects.Count + _countdowns.Count + _draggables.Count + _slots.Count + _keyboards.Count + _scrolls.Count;

	public static void ReturnComp<T>(T component) where T : LuiCompBase
	{
		if (component != null)
		{
			switch (component.type)
			{
			case LuiCompType.Text:
				_texts.Push(component);
				break;
			case LuiCompType.Image:
				_images.Push(component);
				break;
			case LuiCompType.RawImage:
				_rawImages.Push(component);
				break;
			case LuiCompType.Button:
				_buttons.Push(component);
				break;
			case LuiCompType.Outline:
				_outlines.Push(component);
				break;
			case LuiCompType.InputField:
				_inputs.Push(component);
				break;
			case LuiCompType.NeedsCursor:
				_cursors.Push(component);
				break;
			case LuiCompType.RectTransform:
				_rects.Push(component);
				break;
			case LuiCompType.Countdown:
				_countdowns.Push(component);
				break;
			case LuiCompType.Draggable:
				_draggables.Push(component);
				break;
			case LuiCompType.Slot:
				_slots.Push(component);
				break;
			case LuiCompType.NeedsKeyboard:
				_keyboards.Push(component);
				break;
			case LuiCompType.ScrollView:
				_scrolls.Push(component);
				break;
			case LuiCompType.HorizontalLayoutGroup:
			case LuiCompType.VerticalLayoutGroup:
			case LuiCompType.GridLayoutGroup:
			case LuiCompType.ContentSizeFitter:
			case LuiCompType.LayoutElement:
				break;
			}
		}
	}

	public static LuiTextComp GetText()
	{
		if (_texts.Count == 0)
		{
			return new LuiTextComp();
		}
		LuiTextComp luiTextComp = _texts.Pop() as LuiTextComp;
		luiTextComp.enabled = true;
		luiTextComp.text = null;
		luiTextComp.fontSize = 0;
		luiTextComp.font = null;
		luiTextComp.align = null;
		luiTextComp.color = null;
		luiTextComp.verticalOverflow = null;
		luiTextComp.fadeIn = 0f;
		luiTextComp.placeholderParentId = null;
		return luiTextComp;
	}

	public static LuiImageComp GetImage()
	{
		if (_images.Count == 0)
		{
			return new LuiImageComp();
		}
		LuiImageComp luiImageComp = _images.Pop() as LuiImageComp;
		luiImageComp.enabled = true;
		luiImageComp.sprite = null;
		luiImageComp.material = null;
		luiImageComp.color = null;
		luiImageComp.imageType = null;
		luiImageComp.fillCenter = false;
		luiImageComp.slice = null;
		luiImageComp.png = null;
		luiImageComp.itemid = 0;
		luiImageComp.skinid = 0uL;
		luiImageComp.fadeIn = 0f;
		luiImageComp.placeholderParentId = null;
		return luiImageComp;
	}

	public static LuiRawImageComp GetRawImage()
	{
		if (_rawImages.Count == 0)
		{
			return new LuiRawImageComp();
		}
		LuiRawImageComp luiRawImageComp = _rawImages.Pop() as LuiRawImageComp;
		luiRawImageComp.enabled = true;
		luiRawImageComp.sprite = null;
		luiRawImageComp.color = null;
		luiRawImageComp.material = null;
		luiRawImageComp.url = null;
		luiRawImageComp.png = null;
		luiRawImageComp.steamid = null;
		luiRawImageComp.fadeIn = 0f;
		luiRawImageComp.placeholderParentId = null;
		return luiRawImageComp;
	}

	public static LuiButtonComp GetButton()
	{
		if (_buttons.Count == 0)
		{
			return new LuiButtonComp();
		}
		LuiButtonComp luiButtonComp = _buttons.Pop() as LuiButtonComp;
		luiButtonComp.enabled = true;
		luiButtonComp.command = null;
		luiButtonComp.close = null;
		luiButtonComp.sprite = null;
		luiButtonComp.material = null;
		luiButtonComp.color = null;
		luiButtonComp.imageType = null;
		luiButtonComp.normalColor = null;
		luiButtonComp.highlightedColor = null;
		luiButtonComp.pressedColor = null;
		luiButtonComp.selectedColor = null;
		luiButtonComp.disabledColor = null;
		luiButtonComp.colorMultiplier = -1f;
		luiButtonComp.fadeDuration = -1f;
		luiButtonComp.fadeIn = 0f;
		luiButtonComp.placeholderParentId = null;
		return luiButtonComp;
	}

	public static LuiOutlineComp GetOutline()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (_outlines.Count == 0)
		{
			return new LuiOutlineComp();
		}
		LuiOutlineComp luiOutlineComp = _outlines.Pop() as LuiOutlineComp;
		luiOutlineComp.enabled = true;
		luiOutlineComp.color = null;
		luiOutlineComp.distance = default(Vector2);
		luiOutlineComp.useGraphicAlpha = false;
		return luiOutlineComp;
	}

	public static LuiInputComp GetInput()
	{
		if (_inputs.Count == 0)
		{
			return new LuiInputComp();
		}
		LuiInputComp luiInputComp = _inputs.Pop() as LuiInputComp;
		luiInputComp.enabled = true;
		luiInputComp.fontSize = 0;
		luiInputComp.font = null;
		luiInputComp.align = null;
		luiInputComp.color = null;
		luiInputComp.characterLimit = 0;
		luiInputComp.command = null;
		luiInputComp.text = null;
		luiInputComp.readOnly = false;
		luiInputComp.placeholderId = null;
		luiInputComp.lineType = null;
		luiInputComp.password = false;
		luiInputComp.needsKeyboard = false;
		luiInputComp.hudMenuInput = false;
		luiInputComp.autofocus = false;
		luiInputComp.fadeIn = 0f;
		luiInputComp.placeholderParentId = null;
		return luiInputComp;
	}

	public static LuiCursorComp GetCursor()
	{
		if (_cursors.Count == 0)
		{
			return new LuiCursorComp();
		}
		LuiCursorComp luiCursorComp = _cursors.Pop() as LuiCursorComp;
		luiCursorComp.enabled = true;
		return luiCursorComp;
	}

	public static LuiRectTransformComp GetRect()
	{
		if (_rects.Count == 0)
		{
			return new LuiRectTransformComp();
		}
		LuiRectTransformComp luiRectTransformComp = _rects.Pop() as LuiRectTransformComp;
		luiRectTransformComp.anchor = LuiPosition.Full;
		luiRectTransformComp.offset = LuiOffset.None;
		luiRectTransformComp.rotation = 0f;
		luiRectTransformComp.setParent = null;
		luiRectTransformComp.setTransformIndex = -1;
		return luiRectTransformComp;
	}

	public static LuiCountdownComp GetCountdown()
	{
		if (_countdowns.Count == 0)
		{
			return new LuiCountdownComp();
		}
		LuiCountdownComp luiCountdownComp = _countdowns.Pop() as LuiCountdownComp;
		luiCountdownComp.enabled = true;
		luiCountdownComp.endTime = -1f;
		luiCountdownComp.startTime = -1f;
		luiCountdownComp.step = 0f;
		luiCountdownComp.interval = 0f;
		luiCountdownComp.timerFormat = null;
		luiCountdownComp.numberFormat = null;
		luiCountdownComp.destroyIfDone = true;
		luiCountdownComp.command = null;
		return luiCountdownComp;
	}

	public static LuiHorizontalLayoutGroupComp GetHorizontalLayoutGroup()
	{
		if (_horizontals.Count == 0)
		{
			return new LuiHorizontalLayoutGroupComp();
		}
		LuiHorizontalLayoutGroupComp luiHorizontalLayoutGroupComp = _horizontals.Pop() as LuiHorizontalLayoutGroupComp;
		luiHorizontalLayoutGroupComp.spacing = 0f;
		luiHorizontalLayoutGroupComp.childAlignment = null;
		luiHorizontalLayoutGroupComp.childForceExpandWidth = true;
		luiHorizontalLayoutGroupComp.childForceExpandWidth = true;
		luiHorizontalLayoutGroupComp.childControlWidth = false;
		luiHorizontalLayoutGroupComp.childControlHeight = false;
		luiHorizontalLayoutGroupComp.childScaleWidth = false;
		luiHorizontalLayoutGroupComp.childScaleHeight = false;
		luiHorizontalLayoutGroupComp.padding = null;
		return luiHorizontalLayoutGroupComp;
	}

	public static LuiVerticalLayoutGroupComp GetVerticalLayoutGroup()
	{
		if (_verticals.Count == 0)
		{
			return new LuiVerticalLayoutGroupComp();
		}
		LuiVerticalLayoutGroupComp luiVerticalLayoutGroupComp = _verticals.Pop() as LuiVerticalLayoutGroupComp;
		luiVerticalLayoutGroupComp.spacing = 0f;
		luiVerticalLayoutGroupComp.childAlignment = null;
		luiVerticalLayoutGroupComp.childForceExpandWidth = true;
		luiVerticalLayoutGroupComp.childForceExpandWidth = true;
		luiVerticalLayoutGroupComp.childControlWidth = false;
		luiVerticalLayoutGroupComp.childControlHeight = false;
		luiVerticalLayoutGroupComp.childScaleWidth = false;
		luiVerticalLayoutGroupComp.childScaleHeight = false;
		luiVerticalLayoutGroupComp.padding = null;
		return luiVerticalLayoutGroupComp;
	}

	public static LuiGridLayoutGroupComp GetGridLayoutGroup()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (_grids.Count == 0)
		{
			return new LuiGridLayoutGroupComp();
		}
		LuiGridLayoutGroupComp luiGridLayoutGroupComp = _grids.Pop() as LuiGridLayoutGroupComp;
		luiGridLayoutGroupComp.cellSize = LUI.defaultCellSize;
		luiGridLayoutGroupComp.spacing = default(Vector2);
		luiGridLayoutGroupComp.startCorner = null;
		luiGridLayoutGroupComp.startAxis = null;
		luiGridLayoutGroupComp.childAlignment = null;
		luiGridLayoutGroupComp.constraint = null;
		luiGridLayoutGroupComp.constraintCount = 0;
		luiGridLayoutGroupComp.padding = null;
		return luiGridLayoutGroupComp;
	}

	public static LuiContentSizeFitterComp GetContentSizeFitter()
	{
		if (_fitters.Count == 0)
		{
			return new LuiContentSizeFitterComp();
		}
		LuiContentSizeFitterComp luiContentSizeFitterComp = _fitters.Pop() as LuiContentSizeFitterComp;
		luiContentSizeFitterComp.horizontalFit = null;
		luiContentSizeFitterComp.verticalFit = null;
		return luiContentSizeFitterComp;
	}

	public static LuiLayoutElementComp GetLayoutElement()
	{
		if (_layouts.Count == 0)
		{
			return new LuiLayoutElementComp();
		}
		LuiLayoutElementComp luiLayoutElementComp = _layouts.Pop() as LuiLayoutElementComp;
		luiLayoutElementComp.preferredWidth = -1f;
		luiLayoutElementComp.preferredHeight = -1f;
		luiLayoutElementComp.minWidth = 0f;
		luiLayoutElementComp.minHeight = 0f;
		luiLayoutElementComp.flexibleWidth = 0f;
		luiLayoutElementComp.flexibleHeight = 0f;
		luiLayoutElementComp.ignoreLayout = false;
		return luiLayoutElementComp;
	}

	public static LuiDraggableComp GetDraggable()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (_draggables.Count == 0)
		{
			return new LuiDraggableComp();
		}
		LuiDraggableComp luiDraggableComp = _draggables.Pop() as LuiDraggableComp;
		luiDraggableComp.enabled = true;
		luiDraggableComp.limitToParent = false;
		luiDraggableComp.maxDistance = 0f;
		luiDraggableComp.allowSwapping = false;
		luiDraggableComp.dropAnywhere = true;
		luiDraggableComp.dragAlpha = -1f;
		luiDraggableComp.parentLimitIndex = -1;
		luiDraggableComp.filter = null;
		luiDraggableComp.parentPadding = default(Vector2);
		luiDraggableComp.anchorOffset = default(Vector2);
		luiDraggableComp.keepOnTop = false;
		luiDraggableComp.positionRPC = null;
		luiDraggableComp.moveToAnchor = false;
		luiDraggableComp.rebuildAnchor = false;
		return luiDraggableComp;
	}

	public static LuiSlotComp GetSlot()
	{
		if (_slots.Count == 0)
		{
			return new LuiSlotComp();
		}
		LuiSlotComp luiSlotComp = _slots.Pop() as LuiSlotComp;
		luiSlotComp.enabled = true;
		luiSlotComp.filter = null;
		return luiSlotComp;
	}

	public static LuiKeyboardComp GetKeyboard()
	{
		if (_keyboards.Count == 0)
		{
			return new LuiKeyboardComp();
		}
		LuiKeyboardComp luiKeyboardComp = _keyboards.Pop() as LuiKeyboardComp;
		luiKeyboardComp.enabled = true;
		return luiKeyboardComp;
	}

	public static LuiScrollComp GetScroll()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (_scrolls.Count == 0)
		{
			return new LuiScrollComp();
		}
		LuiScrollComp luiScrollComp = _scrolls.Pop() as LuiScrollComp;
		luiScrollComp.enabled = true;
		luiScrollComp.anchor = LuiPosition.Full;
		luiScrollComp.offset = LuiOffset.None;
		luiScrollComp.pivot = LUI.defaultPivot;
		luiScrollComp.horizontal = false;
		luiScrollComp.vertical = false;
		luiScrollComp.movementType = null;
		luiScrollComp.elasticity = -1f;
		luiScrollComp.inertia = false;
		luiScrollComp.decelerationRate = -1f;
		luiScrollComp.scrollSensitivity = -1f;
		luiScrollComp.horizontalScrollbar = default(LuiScrollbar);
		luiScrollComp.verticalScrollbar = default(LuiScrollbar);
		luiScrollComp.horizontalNormalizedPosition = 0f;
		luiScrollComp.verticalNormalizedPosition = 0f;
		return luiScrollComp;
	}

	public static LuiCompType GetLuiCompType(Type type)
	{
		if ((object)type != null)
		{
			if (type == typeof(LuiTextComp))
			{
				return LuiCompType.Text;
			}
			if (type == typeof(LuiImageComp))
			{
				return LuiCompType.Image;
			}
			if (type == typeof(LuiRawImageComp))
			{
				return LuiCompType.RawImage;
			}
			if (type == typeof(LuiButtonComp))
			{
				return LuiCompType.Button;
			}
			if (type == typeof(LuiOutlineComp))
			{
				return LuiCompType.Outline;
			}
			if (type == typeof(LuiInputComp))
			{
				return LuiCompType.InputField;
			}
			if (type == typeof(LuiCursorComp))
			{
				return LuiCompType.NeedsCursor;
			}
			if (type == typeof(LuiRectTransformComp))
			{
				return LuiCompType.RectTransform;
			}
			if (type == typeof(LuiCountdownComp))
			{
				return LuiCompType.Countdown;
			}
			if (type == typeof(LuiHorizontalLayoutGroupComp))
			{
				return LuiCompType.HorizontalLayoutGroup;
			}
			if (type == typeof(LuiVerticalLayoutGroupComp))
			{
				return LuiCompType.VerticalLayoutGroup;
			}
			if (type == typeof(LuiGridLayoutGroupComp))
			{
				return LuiCompType.GridLayoutGroup;
			}
			if (type == typeof(LuiContentSizeFitterComp))
			{
				return LuiCompType.ContentSizeFitter;
			}
			if (type == typeof(LuiLayoutElementComp))
			{
				return LuiCompType.LayoutElement;
			}
			if (type == typeof(LuiDraggableComp))
			{
				return LuiCompType.Draggable;
			}
			if (type == typeof(LuiSlotComp))
			{
				return LuiCompType.Slot;
			}
			if (type == typeof(LuiKeyboardComp))
			{
				return LuiCompType.NeedsKeyboard;
			}
			if (type == typeof(LuiScrollComp))
			{
				return LuiCompType.ScrollView;
			}
		}
		return LuiCompType.Image;
	}

	public static T GetLuiCompFromPool<T>(Type type) where T : LuiCompBase
	{
		if ((object)type != null)
		{
			if (type == typeof(LuiTextComp))
			{
				return GetText() as T;
			}
			if (type == typeof(LuiImageComp))
			{
				return GetImage() as T;
			}
			if (type == typeof(LuiRawImageComp))
			{
				return GetRawImage() as T;
			}
			if (type == typeof(LuiButtonComp))
			{
				return GetButton() as T;
			}
			if (type == typeof(LuiOutlineComp))
			{
				return GetOutline() as T;
			}
			if (type == typeof(LuiInputComp))
			{
				return GetInput() as T;
			}
			if (type == typeof(LuiCursorComp))
			{
				return GetCursor() as T;
			}
			if (type == typeof(LuiRectTransformComp))
			{
				return GetRect() as T;
			}
			if (type == typeof(LuiCountdownComp))
			{
				return GetCountdown() as T;
			}
			if (type == typeof(LuiHorizontalLayoutGroupComp))
			{
				return GetHorizontalLayoutGroup() as T;
			}
			if (type == typeof(LuiVerticalLayoutGroupComp))
			{
				return GetVerticalLayoutGroup() as T;
			}
			if (type == typeof(LuiGridLayoutGroupComp))
			{
				return GetGridLayoutGroup() as T;
			}
			if (type == typeof(LuiContentSizeFitterComp))
			{
				return GetContentSizeFitter() as T;
			}
			if (type == typeof(LuiLayoutElementComp))
			{
				return GetLayoutElement() as T;
			}
			if (type == typeof(LuiDraggableComp))
			{
				return GetDraggable() as T;
			}
			if (type == typeof(LuiSlotComp))
			{
				return GetSlot() as T;
			}
			if (type == typeof(LuiKeyboardComp))
			{
				return GetKeyboard() as T;
			}
			if (type == typeof(LuiScrollComp))
			{
				return GetScroll() as T;
			}
		}
		return GetImage() as T;
	}

	public static LUI.LuiContainer GetContainer()
	{
		if (_containers.Count == 0)
		{
			LUI.LuiContainer luiContainer = new LUI.LuiContainer();
			luiContainer.name = null;
			luiContainer.parent = null;
			luiContainer.luiComponents = new LuiComponentDictionary();
			luiContainer.destroyUi = null;
			luiContainer.fadeOut = 0f;
			luiContainer.update = false;
			luiContainer.activeSelf = true;
			return luiContainer;
		}
		LUI.LuiContainer luiContainer2 = _containers.Pop();
		luiContainer2.name = null;
		luiContainer2.parent = null;
		luiContainer2.luiComponents.Clear();
		luiContainer2.destroyUi = null;
		luiContainer2.fadeOut = 0f;
		luiContainer2.update = false;
		return luiContainer2;
	}

	public static void ReturnContainer(LUI.LuiContainer container)
	{
		_containers.Push(container);
	}
}

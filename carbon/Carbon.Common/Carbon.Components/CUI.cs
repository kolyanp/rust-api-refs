using System;
using System.Collections.Generic;
using Carbon.Base;
using Carbon.Extensions;
using Carbon.Modules;
using Oxide.Game.Rust.Cui;
using UnityEngine;
using UnityEngine.UI;

namespace Carbon.Components;

public readonly struct CUI : IDisposable
{
	public enum ClientPanels
	{
		Overall,
		Overlay,
		OverlayNonScaled,
		Hud,
		HudMenu,
		Under,
		UnderNonScaled,
		Inventory,
		TechTree,
		Crafting,
		Contacts,
		Clans,
		Map
	}

	public struct Pair<T1, T2>(T1 id, T2 element)
	{
		public T1 Id = id;

		public T2 Element = element;

		public static implicit operator string(Pair<T1, T2> value)
		{
			return value.Id.ToString();
		}
	}

	public struct Pair<T1, T2, T3>(T1 id, T2 element1, T3 element2)
	{
		public T1 Id = id;

		public T2 Element1 = element1;

		public T3 Element2 = element2;

		public static implicit operator string(Pair<T1, T2, T3> value)
		{
			return value.Id.ToString();
		}
	}

	public class Handler
	{
		public enum FontTypes
		{
			RobotoCondensedBold,
			RobotoCondensedRegular,
			PermanentMarker,
			DroidSansMono,
			NotoSansArabicBold,
			Poxel,
			LCD,
			NoToEmoji,
			PressStart
		}

		public class UpdatePool : CuiElementContainer, IDisposable
		{
			private bool _disposed;

			public void Add(Pair<string, CuiElement> pair)
			{
				if (pair.Element != null)
				{
					if (!pair.Element.Update)
					{
						Logger.Warn("You're trying to update element '" + pair.Element.Name + "' (of parent '" + pair.Element.Parent + "') which doesn't allow updates. Ignoring.");
					}
					else
					{
						Add(pair.Element);
					}
				}
			}

			public void Add(Pair<string, CuiElement, CuiElement> pair)
			{
				if (pair.Element1 != null)
				{
					if (!pair.Element1.Update)
					{
						Logger.Warn("You're trying to update element '" + pair.Element1.Name + "' (of parent '" + pair.Element1.Parent + "') which doesn't allow updates. Ignoring.");
						return;
					}
					Add(pair.Element1);
				}
				if (pair.Element2 != null && pair.Element2.Update)
				{
					Add(pair.Element2);
				}
			}

			public void Send(BasePlayer player)
			{
				CUIStatics.Send(this, player);
			}

			public void Dispose()
			{
				if (!_disposed)
				{
					Clear();
					_disposed = true;
				}
			}
		}

		public class Cache
		{
			internal Dictionary<string, byte[]> _cuiData = new Dictionary<string, byte[]>();

			public bool TryStore(string id, CuiElementContainer container)
			{
				if (TryTake(id, out var _))
				{
					return false;
				}
				_cuiData.Add(id, container.GetData());
				return true;
			}

			public bool TryTake(string id, out byte[] data)
			{
				data = null;
				if (_cuiData.TryGetValue(id, out var value))
				{
					data = value;
					return true;
				}
				return false;
			}

			public bool TrySend(string id, BasePlayer player)
			{
				if (!TryTake(id, out var data))
				{
					return false;
				}
				CUIStatics.SendData(data, player);
				return true;
			}
		}

		public Cache CacheInstance = new Cache();

		private List<object> _queue = new List<object>();

		private List<CuiElementContainer> _containerPool = new List<CuiElementContainer>();

		private List<CuiElement> _elements = new List<CuiElement>();

		private List<ICuiComponent> _images = new List<ICuiComponent>();

		private List<ICuiComponent> _rawImages = new List<ICuiComponent>();

		private List<ICuiComponent> _texts = new List<ICuiComponent>();

		private List<ICuiComponent> _buttons = new List<ICuiComponent>();

		private List<ICuiComponent> _inputFields = new List<ICuiComponent>();

		private List<ICuiComponent> _rects = new List<ICuiComponent>();

		private List<ICuiComponent> _needsCursors = new List<ICuiComponent>();

		private List<ICuiComponent> _needsKeyboards = new List<ICuiComponent>();

		private List<ICuiComponent> _countdowns = new List<ICuiComponent>();

		private List<ICuiComponent> _outlines = new List<ICuiComponent>();

		private List<ICuiComponent> _scrollViews = new List<ICuiComponent>();

		private List<ICuiComponent> _scrollbars = new List<ICuiComponent>();

		private CuiImageComponent _defaultImage = new CuiImageComponent();

		private CuiRawImageComponent _defaultRawImage = new CuiRawImageComponent();

		private CuiRectTransformComponent DefaultRectTransformComponent = new CuiRectTransformComponent
		{
			OffsetMax = "0 0"
		};

		private CuiTextComponent _defaultText = new CuiTextComponent();

		private CuiButtonComponent _defaultButton = new CuiButtonComponent();

		private CuiInputFieldComponent _defaultInputField = new CuiInputFieldComponent();

		private CuiNeedsCursorComponent _defaultNeedsCursor = new CuiNeedsCursorComponent();

		private CuiNeedsKeyboardComponent _defaultNeedsKeyboard = new CuiNeedsKeyboardComponent();

		private CuiCountdownComponent _defaultCountdown = new CuiCountdownComponent();

		private CuiOutlineComponent _defaultOutline = new CuiOutlineComponent();

		private CuiScrollViewComponent _defaultScrollView = new CuiScrollViewComponent
		{
			HorizontalNormalizedPosition = 0f,
			VerticalNormalizedPosition = 1f
		};

		private CuiScrollbar _defaultScrollBar = new CuiScrollbar();

		internal string Identifier { get; set; } = RandomEx.GetRandomString(4);

		public int Pooled => _containerPool.Count + _elements.Count + _images.Count + _rawImages.Count + _texts.Count + _buttons.Count + _inputFields.Count + _rects.Count + _needsCursors.Count + _needsKeyboards.Count;

		public int Used => _queue.Count;

		private int _currentId { get; set; }

		public string AppendId()
		{
			_currentId++;
			return LUIBuilder.BuildElementId(Identifier, _currentId);
		}

		public void SendToPool<T>(T element) where T : ICuiComponent
		{
			if (element == null)
			{
				return;
			}
			if (!(element is CuiImageComponent))
			{
				if (!(element is CuiRawImageComponent))
				{
					if (!(element is CuiTextComponent))
					{
						if (!(element is CuiButtonComponent))
						{
							if (!(element is CuiRectTransformComponent))
							{
								if (!(element is CuiInputFieldComponent))
								{
									if (!(element is CuiNeedsCursorComponent))
									{
										if (!(element is CuiNeedsKeyboardComponent))
										{
											if (!(element is CuiCountdownComponent))
											{
												if (!(element is CuiOutlineComponent))
												{
													if (!(element is CuiScrollViewComponent cuiScrollViewComponent))
													{
														if (element is CuiScrollbar)
														{
															_scrollbars.Add(element);
														}
													}
													else
													{
														SendToPool(cuiScrollViewComponent.HorizontalScrollbar);
														SendToPool(cuiScrollViewComponent.VerticalScrollbar);
														_scrollViews.Add(element);
													}
												}
												else
												{
													_outlines.Add(element);
												}
											}
											else
											{
												_countdowns.Add(element);
											}
										}
										else
										{
											_needsKeyboards.Add(element);
										}
									}
									else
									{
										_needsCursors.Add(element);
									}
								}
								else
								{
									_inputFields.Add(element);
								}
							}
							else
							{
								_rects.Add(element);
							}
						}
						else
						{
							_buttons.Add(element);
						}
					}
					else
					{
						_texts.Add(element);
					}
				}
				else
				{
					_rawImages.Add(element);
				}
			}
			else
			{
				_images.Add(element);
			}
		}

		public void SendToPool()
		{
			foreach (object item3 in _queue)
			{
				if (item3 is CuiElement item)
				{
					_elements.Add(item);
				}
				else if (item3 is CuiElementContainer item2)
				{
					_containerPool.Add(item2);
				}
				else
				{
					SendToPool(item3 as ICuiComponent);
				}
			}
			_queue.Clear();
			Identifier = RandomEx.GetRandomString(4);
			_currentId = 0;
		}

		public CuiElement TakeFromPool(string name = null, string parent = "Hud", float fadeOut = 0f, string destroyUi = null, bool update = false, bool activeSelf = true)
		{
			CuiElement cuiElement = null;
			if (_elements.Count == 0)
			{
				cuiElement = new CuiElement();
			}
			else
			{
				cuiElement = _elements[0];
				_elements.RemoveAt(0);
			}
			cuiElement.Name = name;
			cuiElement.Update = update;
			cuiElement.Parent = parent;
			cuiElement.Components.Clear();
			cuiElement.DestroyUi = destroyUi;
			cuiElement.FadeOut = fadeOut;
			cuiElement.ActiveSelf = activeSelf;
			_queue.Add(cuiElement);
			return cuiElement;
		}

		public CuiElementContainer TakeFromPoolContainer()
		{
			CuiElementContainer cuiElementContainer = null;
			if (_containerPool.Count == 0)
			{
				cuiElementContainer = new CuiElementContainer();
			}
			else
			{
				cuiElementContainer = _containerPool[0];
				cuiElementContainer.Clear();
				_containerPool.RemoveAt(0);
			}
			_queue.Add(cuiElementContainer);
			return cuiElementContainer;
		}

		public CuiImageComponent TakeFromPoolImage()
		{
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			CuiImageComponent cuiImageComponent = null;
			if (_images.Count == 0)
			{
				cuiImageComponent = new CuiImageComponent();
			}
			else
			{
				cuiImageComponent = _images[0] as CuiImageComponent;
				cuiImageComponent.Sprite = _defaultImage.Sprite;
				cuiImageComponent.Material = _defaultImage.Material;
				cuiImageComponent.Color = _defaultImage.Color;
				cuiImageComponent.SkinId = _defaultImage.SkinId;
				cuiImageComponent.ImageType = _defaultImage.ImageType;
				cuiImageComponent.FillCenter = _defaultImage.FillCenter;
				cuiImageComponent.Png = _defaultImage.Png;
				cuiImageComponent.Slice = _defaultImage.Slice;
				cuiImageComponent.FadeIn = _defaultImage.FadeIn;
				cuiImageComponent.ItemId = _defaultImage.ItemId;
				cuiImageComponent.SkinId = _defaultImage.SkinId;
				cuiImageComponent.PlaceholderParentId = cuiImageComponent.PlaceholderParentId;
				cuiImageComponent.Enabled = _defaultImage.Enabled;
				_images.RemoveAt(0);
			}
			_queue.Add(cuiImageComponent);
			return cuiImageComponent;
		}

		public CuiRawImageComponent TakeFromPoolRawImage()
		{
			CuiRawImageComponent cuiRawImageComponent = null;
			if (_rawImages.Count == 0)
			{
				cuiRawImageComponent = new CuiRawImageComponent();
			}
			else
			{
				cuiRawImageComponent = _rawImages[0] as CuiRawImageComponent;
				cuiRawImageComponent.Sprite = _defaultRawImage.Sprite;
				cuiRawImageComponent.Color = _defaultRawImage.Color;
				cuiRawImageComponent.Material = _defaultRawImage.Material;
				cuiRawImageComponent.Url = _defaultRawImage.Url;
				cuiRawImageComponent.Png = _defaultRawImage.Png;
				cuiRawImageComponent.FadeIn = _defaultRawImage.FadeIn;
				cuiRawImageComponent.SteamId = _defaultRawImage.SteamId;
				cuiRawImageComponent.PlaceholderParentId = _defaultRawImage.PlaceholderParentId;
				cuiRawImageComponent.Enabled = _defaultRawImage.Enabled;
				_rawImages.RemoveAt(0);
			}
			_queue.Add(cuiRawImageComponent);
			return cuiRawImageComponent;
		}

		public CuiRectTransformComponent TakeFromPoolRect()
		{
			CuiRectTransformComponent cuiRectTransformComponent = null;
			if (_rects.Count == 0)
			{
				cuiRectTransformComponent = new CuiRectTransformComponent();
			}
			else
			{
				cuiRectTransformComponent = _rects[0] as CuiRectTransformComponent;
				cuiRectTransformComponent.AnchorMin = DefaultRectTransformComponent.AnchorMin;
				cuiRectTransformComponent.AnchorMax = DefaultRectTransformComponent.AnchorMax;
				cuiRectTransformComponent.OffsetMin = DefaultRectTransformComponent.OffsetMin;
				cuiRectTransformComponent.OffsetMax = DefaultRectTransformComponent.OffsetMax;
				cuiRectTransformComponent.Rotation = DefaultRectTransformComponent.Rotation;
				cuiRectTransformComponent.Pivot = DefaultRectTransformComponent.Pivot;
				cuiRectTransformComponent.SetParent = DefaultRectTransformComponent.SetParent;
				cuiRectTransformComponent.SetTransformIndex = DefaultRectTransformComponent.SetTransformIndex;
				_rects.RemoveAt(0);
			}
			_queue.Add(cuiRectTransformComponent);
			return cuiRectTransformComponent;
		}

		public CuiTextComponent TakeFromPoolText()
		{
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			CuiTextComponent cuiTextComponent = null;
			if (_texts.Count == 0)
			{
				cuiTextComponent = new CuiTextComponent();
			}
			else
			{
				cuiTextComponent = _texts[0] as CuiTextComponent;
				cuiTextComponent.Text = _defaultText.Text;
				cuiTextComponent.FontSize = _defaultText.FontSize;
				cuiTextComponent.Font = _defaultText.Font;
				cuiTextComponent.Align = _defaultText.Align;
				cuiTextComponent.Color = _defaultText.Color;
				cuiTextComponent.FadeIn = _defaultText.FadeIn;
				cuiTextComponent.VerticalOverflow = _defaultText.VerticalOverflow;
				cuiTextComponent.PlaceholderParentId = _defaultText.PlaceholderParentId;
				cuiTextComponent.Enabled = _defaultText.Enabled;
				_texts.RemoveAt(0);
			}
			_queue.Add(cuiTextComponent);
			return cuiTextComponent;
		}

		public CuiButtonComponent TakeFromPoolButton()
		{
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			CuiButtonComponent cuiButtonComponent = null;
			if (_buttons.Count == 0)
			{
				cuiButtonComponent = new CuiButtonComponent();
			}
			else
			{
				cuiButtonComponent = _buttons[0] as CuiButtonComponent;
				cuiButtonComponent.Command = _defaultButton.Command;
				cuiButtonComponent.Close = _defaultButton.Close;
				cuiButtonComponent.Sprite = _defaultButton.Sprite;
				cuiButtonComponent.Material = _defaultButton.Material;
				cuiButtonComponent.Color = _defaultButton.Color;
				cuiButtonComponent.ImageType = _defaultButton.ImageType;
				cuiButtonComponent.NormalColor = _defaultButton.NormalColor;
				cuiButtonComponent.HighlightedColor = _defaultButton.HighlightedColor;
				cuiButtonComponent.PressedColor = _defaultButton.PressedColor;
				cuiButtonComponent.SelectedColor = _defaultButton.SelectedColor;
				cuiButtonComponent.DisabledColor = _defaultButton.DisabledColor;
				cuiButtonComponent.ColorMultiplier = _defaultButton.ColorMultiplier;
				cuiButtonComponent.FadeDuration = _defaultButton.FadeDuration;
				cuiButtonComponent.FadeIn = _defaultButton.FadeIn;
				cuiButtonComponent.PlaceholderParentId = _defaultButton.PlaceholderParentId;
				cuiButtonComponent.Enabled = _defaultButton.Enabled;
				_buttons.RemoveAt(0);
			}
			_queue.Add(cuiButtonComponent);
			return cuiButtonComponent;
		}

		public CuiInputFieldComponent TakeFromPoolInputField()
		{
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			CuiInputFieldComponent cuiInputFieldComponent = null;
			if (_inputFields.Count == 0)
			{
				cuiInputFieldComponent = new CuiInputFieldComponent();
			}
			else
			{
				cuiInputFieldComponent = _inputFields[0] as CuiInputFieldComponent;
				cuiInputFieldComponent.Text = _defaultInputField.Text;
				cuiInputFieldComponent.FontSize = _defaultInputField.FontSize;
				cuiInputFieldComponent.Font = _defaultInputField.Font;
				cuiInputFieldComponent.Align = _defaultInputField.Align;
				cuiInputFieldComponent.Color = _defaultInputField.Color;
				cuiInputFieldComponent.CharsLimit = _defaultInputField.CharsLimit;
				cuiInputFieldComponent.Command = _defaultInputField.Command;
				cuiInputFieldComponent.IsPassword = _defaultInputField.IsPassword;
				cuiInputFieldComponent.ReadOnly = _defaultInputField.ReadOnly;
				cuiInputFieldComponent.PlaceholderId = _defaultInputField.PlaceholderId;
				cuiInputFieldComponent.NeedsKeyboard = _defaultInputField.NeedsKeyboard;
				cuiInputFieldComponent.LineType = _defaultInputField.LineType;
				cuiInputFieldComponent.Autofocus = _defaultInputField.Autofocus;
				cuiInputFieldComponent.HudMenuInput = _defaultInputField.HudMenuInput;
				cuiInputFieldComponent.FadeIn = _defaultInputField.FadeIn;
				cuiInputFieldComponent.PlaceholderParentId = _defaultInputField.PlaceholderParentId;
				cuiInputFieldComponent.Enabled = _defaultInputField.Enabled;
				_inputFields.RemoveAt(0);
			}
			_queue.Add(cuiInputFieldComponent);
			return cuiInputFieldComponent;
		}

		public CuiNeedsCursorComponent TakeFromPoolNeedsCursor()
		{
			CuiNeedsCursorComponent cuiNeedsCursorComponent = null;
			if (_needsCursors.Count == 0)
			{
				cuiNeedsCursorComponent = new CuiNeedsCursorComponent();
			}
			else
			{
				cuiNeedsCursorComponent = _needsCursors[0] as CuiNeedsCursorComponent;
				cuiNeedsCursorComponent.Enabled = _defaultNeedsCursor.Enabled;
				_needsCursors.RemoveAt(0);
			}
			_queue.Add(cuiNeedsCursorComponent);
			return cuiNeedsCursorComponent;
		}

		public CuiNeedsKeyboardComponent TakeFromPoolNeedsKeyboard()
		{
			CuiNeedsKeyboardComponent cuiNeedsKeyboardComponent = null;
			if (_needsKeyboards.Count == 0)
			{
				cuiNeedsKeyboardComponent = new CuiNeedsKeyboardComponent();
			}
			else
			{
				cuiNeedsKeyboardComponent = _needsKeyboards[0] as CuiNeedsKeyboardComponent;
				cuiNeedsKeyboardComponent.Enabled = _defaultNeedsKeyboard.Enabled;
				_needsKeyboards.RemoveAt(0);
			}
			_queue.Add(cuiNeedsKeyboardComponent);
			return cuiNeedsKeyboardComponent;
		}

		public CuiCountdownComponent TakeFromPoolCountdown()
		{
			CuiCountdownComponent cuiCountdownComponent = null;
			if (_countdowns.Count == 0)
			{
				cuiCountdownComponent = new CuiCountdownComponent();
			}
			else
			{
				cuiCountdownComponent = _countdowns[0] as CuiCountdownComponent;
				cuiCountdownComponent.EndTime = _defaultCountdown.EndTime;
				cuiCountdownComponent.StartTime = _defaultCountdown.StartTime;
				cuiCountdownComponent.Step = _defaultCountdown.Step;
				cuiCountdownComponent.Command = _defaultCountdown.Command;
				cuiCountdownComponent.FadeIn = _defaultCountdown.FadeIn;
				cuiCountdownComponent.Interval = _defaultCountdown.Interval;
				cuiCountdownComponent.TimerFormat = _defaultCountdown.TimerFormat;
				cuiCountdownComponent.NumberFormat = _defaultCountdown.NumberFormat;
				cuiCountdownComponent.DestroyIfDone = _defaultCountdown.DestroyIfDone;
				cuiCountdownComponent.Enabled = _defaultCountdown.Enabled;
				_countdowns.RemoveAt(0);
			}
			_queue.Add(cuiCountdownComponent);
			return cuiCountdownComponent;
		}

		public CuiOutlineComponent TakeFromPoolOutline()
		{
			CuiOutlineComponent cuiOutlineComponent = null;
			if (_outlines.Count == 0)
			{
				cuiOutlineComponent = new CuiOutlineComponent();
			}
			else
			{
				cuiOutlineComponent = _outlines[0] as CuiOutlineComponent;
				cuiOutlineComponent.Color = _defaultOutline.Color;
				cuiOutlineComponent.Distance = _defaultOutline.Distance;
				cuiOutlineComponent.UseGraphicAlpha = _defaultOutline.UseGraphicAlpha;
				cuiOutlineComponent.Enabled = _defaultOutline.Enabled;
				_outlines.RemoveAt(0);
			}
			_queue.Add(cuiOutlineComponent);
			return cuiOutlineComponent;
		}

		public CuiScrollViewComponent TakeFromPoolScrollView()
		{
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			CuiScrollViewComponent cuiScrollViewComponent = null;
			CuiScrollViewComponent cuiScrollViewComponent2;
			if (_scrollViews.Count == 0)
			{
				cuiScrollViewComponent = new CuiScrollViewComponent();
				cuiScrollViewComponent2 = cuiScrollViewComponent;
				if (cuiScrollViewComponent2.ContentTransform == null)
				{
					CuiRectTransform cuiRectTransform = (cuiScrollViewComponent2.ContentTransform = new CuiRectTransform());
				}
				cuiScrollViewComponent2 = cuiScrollViewComponent;
				if (cuiScrollViewComponent2.HorizontalScrollbar == null)
				{
					CuiScrollbar cuiScrollbar = (cuiScrollViewComponent2.HorizontalScrollbar = new CuiScrollbar());
				}
				cuiScrollViewComponent2 = cuiScrollViewComponent;
				if (cuiScrollViewComponent2.VerticalScrollbar == null)
				{
					CuiScrollbar cuiScrollbar = (cuiScrollViewComponent2.VerticalScrollbar = new CuiScrollbar());
				}
			}
			else
			{
				cuiScrollViewComponent = _scrollViews[0] as CuiScrollViewComponent;
				_scrollViews.RemoveAt(0);
			}
			cuiScrollViewComponent.Vertical = _defaultScrollView.Vertical;
			cuiScrollViewComponent.Horizontal = _defaultScrollView.Horizontal;
			cuiScrollViewComponent.MovementType = _defaultScrollView.MovementType;
			cuiScrollViewComponent.Elasticity = _defaultScrollView.Elasticity;
			cuiScrollViewComponent.Inertia = _defaultScrollView.Inertia;
			cuiScrollViewComponent.DecelerationRate = _defaultScrollView.DecelerationRate;
			cuiScrollViewComponent.ScrollSensitivity = _defaultScrollView.ScrollSensitivity;
			cuiScrollViewComponent2 = cuiScrollViewComponent;
			if (cuiScrollViewComponent2.ContentTransform == null)
			{
				CuiRectTransform cuiRectTransform = (cuiScrollViewComponent2.ContentTransform = new CuiRectTransform());
			}
			cuiScrollViewComponent.ContentTransform.AnchorMin = "0 0";
			cuiScrollViewComponent.ContentTransform.AnchorMax = "1 1";
			cuiScrollViewComponent.ContentTransform.OffsetMin = "0 0";
			cuiScrollViewComponent.ContentTransform.OffsetMax = "0 0";
			cuiScrollViewComponent2 = cuiScrollViewComponent;
			if (cuiScrollViewComponent2.HorizontalScrollbar == null)
			{
				CuiScrollbar cuiScrollbar = (cuiScrollViewComponent2.HorizontalScrollbar = new CuiScrollbar());
			}
			ResetScrollbar(cuiScrollViewComponent.HorizontalScrollbar);
			cuiScrollViewComponent2 = cuiScrollViewComponent;
			if (cuiScrollViewComponent2.VerticalScrollbar == null)
			{
				CuiScrollbar cuiScrollbar = (cuiScrollViewComponent2.VerticalScrollbar = new CuiScrollbar());
			}
			ResetScrollbar(cuiScrollViewComponent.VerticalScrollbar);
			cuiScrollViewComponent.HorizontalNormalizedPosition = _defaultScrollView.HorizontalNormalizedPosition;
			cuiScrollViewComponent.VerticalNormalizedPosition = _defaultScrollView.VerticalNormalizedPosition;
			cuiScrollViewComponent.Enabled = _defaultScrollView.Enabled;
			_queue.Add(cuiScrollViewComponent);
			return cuiScrollViewComponent;
		}

		public void ResetScrollbar(CuiScrollbar scrollbar)
		{
			scrollbar.Invert = _defaultScrollBar.Invert;
			scrollbar.AutoHide = _defaultScrollBar.AutoHide;
			scrollbar.HandleSprite = _defaultScrollBar.HandleSprite;
			scrollbar.Size = _defaultScrollBar.Size;
			scrollbar.HandleColor = _defaultScrollBar.HandleColor;
			scrollbar.HighlightColor = _defaultScrollBar.HighlightColor;
			scrollbar.PressedColor = _defaultScrollBar.PressedColor;
			scrollbar.TrackSprite = _defaultScrollBar.TrackSprite;
			scrollbar.TrackColor = _defaultScrollBar.TrackColor;
			scrollbar.Enabled = _defaultScrollBar.Enabled;
		}

		public string GetFont(FontTypes type)
		{
			return type switch
			{
				FontTypes.RobotoCondensedBold => "robotocondensed-bold.ttf", 
				FontTypes.RobotoCondensedRegular => "robotocondensed-regular.ttf", 
				FontTypes.PermanentMarker => "permanentmarker.ttf", 
				FontTypes.DroidSansMono => "droidsansmono.ttf", 
				FontTypes.NotoSansArabicBold => "_nonenglish/notosanscjksc-bold.otf", 
				FontTypes.Poxel => "poxel.otf", 
				FontTypes.LCD => "lcd.ttf", 
				FontTypes.NoToEmoji => "_nonenglish/notoemoji-regular.ttf", 
				FontTypes.PressStart => "pressstart2p-regular.ttf", 
				_ => "robotocondensed-regular.ttf", 
			};
		}

		public void Send(CuiElementContainer container, BasePlayer player)
		{
			container.Send(player);
		}

		public void SendUpdate(Pair<string, CuiElement> pair, BasePlayer player)
		{
			pair.SendUpdate(player);
		}

		public void Destroy(CuiElementContainer container, BasePlayer player)
		{
			container.Destroy(player);
		}

		public void Destroy(string name, BasePlayer player)
		{
			CUIStatics.Destroy(name, player);
		}
	}

	public Handler Manager { get; }

	public LUI v2 { get; }

	public ImageDatabaseModule ImageDatabase { get; }

	public Handler.Cache CacheInstance => Manager.CacheInstance;

	public string GetClientPanel(ClientPanels panel)
	{
		return panel switch
		{
			ClientPanels.Overall => "Overall", 
			ClientPanels.OverlayNonScaled => "OverlayNonScaled", 
			ClientPanels.Hud => "Hud", 
			ClientPanels.HudMenu => "Hud.Menu", 
			ClientPanels.Under => "Under", 
			ClientPanels.UnderNonScaled => "UnderNonScaled", 
			ClientPanels.Inventory => "Inventory", 
			ClientPanels.TechTree => "TechTree", 
			ClientPanels.Crafting => "Crafting", 
			ClientPanels.Contacts => "Contacts", 
			ClientPanels.Clans => "Clans", 
			ClientPanels.Map => "Map", 
			_ => "Overlay", 
		};
	}

	public CUI(Handler manager)
	{
		v2 = null;
		ImageDatabase = null;
		Manager = manager;
		v2 = new LUI(this);
		ImageDatabase = BaseModule.GetModule<ImageDatabaseModule>();
	}

	public Handler.UpdatePool UpdatePool()
	{
		return new Handler.UpdatePool();
	}

	public CuiElementContainer CreateContainer(string panel, string color = "0 0 0 0", float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, ClientPanels parent = ClientPanels.Overlay, string destroyUi = null, bool activeSelf = true, float rotation = 0f)
	{
		return CreateContainerParent(panel, color, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, fadeIn, fadeOut, needsCursor, needsKeyboard, GetClientPanel(parent), destroyUi, activeSelf, rotation);
	}

	public CuiElementContainer CreateContainerParent(string panel, string color = "0 0 0 0", float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string parentName = "Overlay", string destroyUi = null, bool activeSelf = true, float rotation = 0f)
	{
		CuiElementContainer cuiElementContainer = Manager.TakeFromPoolContainer();
		cuiElementContainer.Name = panel;
		CuiElement cuiElement = Manager.TakeFromPool(panel, parentName);
		cuiElement.FadeOut = fadeOut;
		cuiElement.DestroyUi = destroyUi;
		cuiElement.ActiveSelf = activeSelf;
		if (!string.IsNullOrEmpty(color) && color != "0 0 0 0")
		{
			CuiImageComponent cuiImageComponent = Manager.TakeFromPoolImage();
			cuiImageComponent.Color = color;
			cuiImageComponent.FadeIn = fadeIn;
			cuiElement.Components.Add(cuiImageComponent);
		}
		CuiRectTransformComponent cuiRectTransformComponent = Manager.TakeFromPoolRect();
		cuiRectTransformComponent.AnchorMin = LUIBuilder.GetStringFloat(xMin, yMin);
		cuiRectTransformComponent.AnchorMax = LUIBuilder.GetStringFloat(xMax, yMax);
		cuiRectTransformComponent.OffsetMin = LUIBuilder.GetStringFloat(OxMin, OyMin);
		cuiRectTransformComponent.OffsetMax = LUIBuilder.GetStringFloat(OxMax, OyMax);
		cuiRectTransformComponent.Rotation = rotation;
		cuiElement.Components.Add(cuiRectTransformComponent);
		if (needsCursor)
		{
			cuiElement.Components.Add(Manager.TakeFromPoolNeedsCursor());
		}
		if (needsKeyboard)
		{
			cuiElement.Components.Add(Manager.TakeFromPoolNeedsKeyboard());
		}
		cuiElementContainer.Add(cuiElement);
		cuiElementContainer.Add(Manager.TakeFromPool(Manager.AppendId(), parentName));
		return cuiElementContainer;
	}

	public Pair<string, CuiElement> CreatePanel(CuiElementContainer container, string parent, string color, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, bool blur = false, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		return Manager.Panel(container, parent, color, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, blur, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update, activeSelf, rotation);
	}

	public Pair<string, CuiElement> CreateText(CuiElementContainer container, string parent, string color, string text, int size, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, TextAnchor align = (TextAnchor)4, Handler.FontTypes font = Handler.FontTypes.RobotoCondensedRegular, VerticalWrapMode verticalOverflow = (VerticalWrapMode)1, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		return Manager.Text(container, parent, color, text, size, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, align, font, verticalOverflow, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update, activeSelf, rotation);
	}

	public Pair<string, CuiElement, CuiElement> CreateButton(CuiElementContainer container, string parent, string color, string textColor, string text, int size, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, string command = null, TextAnchor align = (TextAnchor)4, Handler.FontTypes font = Handler.FontTypes.RobotoCondensedRegular, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		return Manager.Button(container, parent, color, textColor, text, size, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, command, align, font, @protected: false, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update, activeSelf, rotation);
	}

	public Pair<string, CuiElement, CuiElement> CreateProtectedButton(CuiElementContainer container, string parent, string color, string textColor, string text, int size, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, string command = null, TextAnchor align = (TextAnchor)4, Handler.FontTypes font = Handler.FontTypes.RobotoCondensedRegular, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		return Manager.Button(container, parent, color, textColor, text, size, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, command, align, font, @protected: true, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update, activeSelf, rotation);
	}

	public Pair<string, CuiElement> CreateInputField(CuiElementContainer container, string parent, string color, string text, int size, int characterLimit, bool readOnly, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, string command = null, TextAnchor align = (TextAnchor)4, Handler.FontTypes font = Handler.FontTypes.RobotoCondensedRegular, bool autoFocus = false, bool hudMenuInput = false, LineType lineType = (LineType)0, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		return Manager.InputField(container, parent, color, text, size, characterLimit, readOnly, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, command, align, font, @protected: false, autoFocus, hudMenuInput, lineType, fadeIn, fadeOut, needsCursor, needsKeyboard, id, destroyUi, update, activeSelf, rotation);
	}

	public Pair<string, CuiElement> CreateProtectedInputField(CuiElementContainer container, string parent, string color, string text, int size, int characterLimit, bool readOnly, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, string command = null, TextAnchor align = (TextAnchor)4, Handler.FontTypes font = Handler.FontTypes.RobotoCondensedRegular, bool autoFocus = false, bool hudMenuInput = false, LineType lineType = (LineType)0, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		return Manager.InputField(container, parent, color, text, size, characterLimit, readOnly, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, command, align, font, @protected: true, autoFocus, hudMenuInput, lineType, fadeIn, fadeOut, needsCursor, needsKeyboard, id, destroyUi, update, activeSelf, rotation);
	}

	public Pair<string, CuiElement> CreateImage(CuiElementContainer container, string parent, uint png, string color, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		return Manager.Image(container, parent, png.ToString(), null, null, color, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update, activeSelf, rotation);
	}

	public Pair<string, CuiElement> CreateImage(CuiElementContainer container, string parent, string url, string color, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		if (!HasImage(url))
		{
			return Manager.Panel(container, parent, color, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, blur: false, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update, activeSelf, rotation);
		}
		return Manager.Image(container, parent, GetImage(url), null, null, color, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update, activeSelf, rotation);
	}

	public Pair<string, CuiElement> CreatePlayerImage(CuiElementContainer container, string parent, string steamId, string color, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		return Manager.Image(container, parent, null, null, steamId, color, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update, activeSelf, rotation);
	}

	public Pair<string, CuiElement> CreateSimpleImage(CuiElementContainer container, string parent, string png, string sprite, string color, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f, string slice = null)
	{
		return Manager.SimpleImage(container, parent, png, sprite, color, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update, activeSelf, rotation, slice);
	}

	public Pair<string, CuiElement> CreateSprite(CuiElementContainer container, string parent, string sprite, string color, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		return Manager.Sprite(container, parent, sprite, color, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update, activeSelf, rotation);
	}

	public Pair<string, CuiElement> CreateItemImage(CuiElementContainer container, string parent, int itemID, ulong skinID, string color, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		return Manager.ItemImage(container, parent, itemID, skinID, color, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update, activeSelf, rotation);
	}

	public Pair<string, CuiElement> CreateQRCodeImage(CuiElementContainer container, string parent, string text, string brandUrl, string brandColor, string brandBgColor, int pixels, bool transparent, bool quietZones, string color, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true)
	{
		try
		{
			uint qRCode = ImageDatabase.GetQRCode(text, pixels, transparent, quietZones, whiteMode: true);
			Pair<string, CuiElement> pair = CreateImage(container, parent, qRCode, color, null, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update, activeSelf);
			if (!string.IsNullOrEmpty(brandUrl))
			{
				Pair<string, CuiElement> pair2 = CreatePanel(container, pair, brandBgColor, null, 0.4f, 0.6f, 0.4f, 0.6f);
				CreateImage(container, pair2, brandUrl, brandColor, null, 0.15f, 0.85f, 0.15f, 0.85f);
			}
			return pair;
		}
		catch (Exception)
		{
			Pair<string, CuiElement> pair3 = CreatePanel(container, parent, Cache.CUI.WhiteColor, color, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, blur: false, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update, activeSelf);
			CreateText(container, pair3, "0 0 0 0.5", "GDI+ is not installed!", 10, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
			return pair3;
		}
	}

	public Pair<string, CuiElement> CreateClientImage(CuiElementContainer container, string parent, string url, string color, string material = null, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string outlineColor = null, string outlineDistance = null, bool outlineUseGraphicAlpha = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f)
	{
		return Manager.Image(container, parent, null, url, null, color, material, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, fadeIn, fadeOut, needsCursor, needsKeyboard, outlineColor, outlineDistance, outlineUseGraphicAlpha, id, destroyUi, update, activeSelf, rotation);
	}

	public Pair<string, CuiElement> CreateCountdown(CuiElementContainer container, string parent, int startTime, int endTime, int step, string command, float fadeIn = 0f, float fadeOut = 0f, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true)
	{
		return Manager.Countdown(container, parent, startTime, endTime, step, command, fadeIn, fadeOut, id, destroyUi, update, activeSelf);
	}

	public Pair<string, CuiElement> CreateScrollView(CuiElementContainer container, string parent, bool vertical, bool horizontal, MovementType movementType, float elasticity, bool inertia, float decelerationRate, float scrollSensitivity, out CuiRectTransform contentTransformComponent, out CuiScrollbar horizontalScrollBar, out CuiScrollbar verticalScrollBar, float xMin = 0f, float xMax = 1f, float yMin = 0f, float yMax = 1f, float OxMin = 0f, float OxMax = 0f, float OyMin = 0f, float OyMax = 0f, float fadeIn = 0f, float fadeOut = 0f, bool needsCursor = false, bool needsKeyboard = false, string id = null, string destroyUi = null, bool update = false, bool activeSelf = true, float rotation = 0f, float pivotX = 0.5f, float pivotY = 0.5f, float scrollPosHorizontal = 0f, float scrollPosVertical = 1f)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return Manager.ScrollView(container, parent, vertical, horizontal, movementType, elasticity, inertia, decelerationRate, scrollSensitivity, out contentTransformComponent, out horizontalScrollBar, out verticalScrollBar, xMin, xMax, yMin, yMax, OxMin, OxMax, OyMin, OyMax, fadeIn, fadeOut, needsCursor, needsKeyboard, id, destroyUi, update, activeSelf, rotation, pivotX, pivotY, scrollPosHorizontal, scrollPosVertical);
	}

	public static string HexToRustColor(string hexColor, float? alpha = null)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		Color val = default(Color);
		if (!ColorUtility.TryParseHtmlString(hexColor, ref val))
		{
			float[] obj = new float[4] { 1f, 1f, 1f, 0f };
			obj[3] = alpha ?? 1f;
			return LUIBuilder.GetStringFloat(obj);
		}
		return LUIBuilder.GetStringFloat(val.r, val.g, val.b, alpha ?? val.a);
	}

	public static string RustToHexColor(string rustColor, float? alpha = null, bool includeAlpha = true)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		string[] array = rustColor.Split(' ');
		Color val = default(Color);
		((Color)(ref val))._002Ector(array[0].ToFloat(), array[1].ToFloat(), array[2].ToFloat(), (!includeAlpha) ? 1f : (alpha ?? ((array.Length > 2) ? array[3].ToFloat() : 1f)));
		string text = (includeAlpha ? ColorUtility.ToHtmlStringRGBA(val) : ColorUtility.ToHtmlStringRGB(val));
		Array.Clear(array, 0, array.Length);
		return "#" + text;
	}

	public string GetImage(string keyOrUrl)
	{
		return ImageDatabase.GetImageString(keyOrUrl);
	}

	public bool HasImage(string url)
	{
		return ImageDatabase.HasImage(url);
	}

	public void QueueImages(IEnumerable<string> urls)
	{
		ImageDatabase.QueueBatch(@override: false, urls);
	}

	public void ClearImages(IEnumerable<string> urls)
	{
		foreach (string url in urls)
		{
			ImageDatabase.DeleteImage(url);
		}
	}

	public void Send(CuiElementContainer container, BasePlayer player)
	{
		Manager.Send(container, player);
	}

	public void Destroy(string name, BasePlayer player)
	{
		Manager.Destroy(name, player);
	}

	public void Dispose()
	{
		v2.Dispose();
		Manager.SendToPool();
	}
}

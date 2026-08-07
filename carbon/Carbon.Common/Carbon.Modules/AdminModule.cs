using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using API.Hooks;
using API.Logger;
using Carbon.Base;
using Carbon.Components;
using Carbon.Components.Graphics;
using Carbon.Core;
using Carbon.Extensions;
using Carbon.Pooling;
using ConVar;
using Facepunch;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using Oxide.Plugins;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Carbon.Modules;

public class AdminModule : CarbonModule<AdminConfig, AdminData>
{
	public class Notifications
	{
		public class NotificationQueue : Dictionary<ulong, List<Notification>>
		{
		}

		public struct Notification
		{
			public string Message;

			public float Duration;
		}

		public static NotificationQueue Queue = new NotificationQueue();

		public static List<Notification> GetOrCreateQueue(BasePlayer player)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			if (!Queue.TryGetValue(EncryptedValue<ulong>.op_Implicit(player.userID), out var value))
			{
				value = (Queue[EncryptedValue<ulong>.op_Implicit(player.userID)] = new List<Notification>());
			}
			return value;
		}

		public static void Redraw(BasePlayer player)
		{
			List<Notification> orCreateQueue = GetOrCreateQueue(player);
			using CUI cUI = new CUI(Singleton.Handler);
			CuiElementContainer container = cUI.CreateContainer("adminmodulenotifs", "0 0 0 0", 0.95f, 0.95f, 0.95f, 0.95f, 0f, 0f, 0f, 0f, 0f, 0f, needsCursor: false, needsKeyboard: false, CUI.ClientPanels.Overlay, "adminmodulenotifs");
			for (int i = 0; i < orCreateQueue.Count; i++)
			{
				Notification notification = orCreateQueue[i];
				CUI.Pair<string, CuiElement> pair = cUI.CreatePanel(container, "adminmodulenotifs", Cache.CUI.BlankColor, null, 0f, 1f, 0f, 1f, -2050f, 0f, -20 - 20 * i, -(20 * i));
				cUI.CreateText(container, pair, "1 1 1 1", notification.Message, 9, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)5, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				cUI.CreatePanel(container, pair, "1 0 0 0.2", null, 1f, 1f, 0f, 0.1f, -40f);
			}
			cUI.Send(container, player);
		}

		public static void Add(BasePlayer player, string message, float duration = 2f)
		{
			List<Notification> queue = GetOrCreateQueue(player);
			Community.Runtime.Core.timer.In(duration, delegate
			{
				queue.RemoveAt(queue.Count - 1);
				Redraw(player);
			});
			Notification item = new Notification
			{
				Message = message,
				Duration = duration
			};
			queue.Insert(0, item);
			Redraw(player);
		}
	}

	[HookAttribute.Patch("IModBackpack", "IModBackpack", typeof(ItemModBackpack), "CanAcceptItem", new Type[]
	{
		typeof(Item),
		typeof(Item),
		typeof(int)
	})]
	[HookAttribute.Options(HookFlags.Hidden)]
	public class Item_ModBackpack : Patch
	{
		public static bool Prefix(Item backpack, Item item, int slot, ref bool __result)
		{
			if (AcceptOnBackpack(backpack))
			{
				__result = true;
				return false;
			}
			return true;
		}
	}

	[HookAttribute.Patch("IValidDismountPosition", "IValidDismountPosition", typeof(BaseMountable), "ValidDismountPosition", new Type[]
	{
		typeof(BasePlayer),
		typeof(Vector3)
	})]
	[HookAttribute.Options(HookFlags.Hidden)]
	public class BaseMountable_ValidDismountPosition : Patch
	{
		public static bool Prefix(BasePlayer player, Vector3 disPos, BaseMountable __instance, ref bool __result)
		{
			ulong skinID = ((BaseEntity)__instance).skinID;
			if (skinID == 69696)
			{
				__result = true;
				return false;
			}
			return true;
		}
	}

	public class PlayerSession : IDisposable
	{
		public class Page : IPooled
		{
			public int CurrentPage { get; set; }

			public int TotalPages { get; set; }

			public void Check()
			{
				if (CurrentPage < 0)
				{
					CurrentPage = TotalPages;
				}
				else if (CurrentPage > TotalPages)
				{
					CurrentPage = 0;
				}
			}

			public void EnterPool()
			{
			}

			public void LeavePool()
			{
				CurrentPage = 0;
				TotalPages = 0;
			}
		}

		public BasePlayer Player;

		public bool IsInMenu;

		public Dictionary<int, Page> ColumnPages = new Dictionary<int, Page>();

		public Dictionary<string, object> LocalStorage = new Dictionary<string, object>();

		public Tab SelectedTab;

		public int TabSkip;

		public int LastPressedColumn;

		public int LastPressedRow;

		public Tab.Option Tooltip;

		public Tab.Option Input;

		public Tab.Option PreviousInput;

		internal Tab.OptionDropdown _selectedDropdown;

		internal Page _selectedDropdownPage = new Page();

		public static PlayerSession Blank { get; } = new PlayerSession(null);

		public PlayerSession(BasePlayer player)
		{
			Player = player;
		}

		public void SetPage(int column, int page)
		{
			if (ColumnPages.TryGetValue(column, out var value))
			{
				value.CurrentPage = page;
				value.Check();
			}
		}

		public T GetStorage<T>(Tab tab, string id, T @default = default(T))
		{
			try
			{
				string id2 = id;
				id = tab?.Id + "_" + id;
				if (LocalStorage.TryGetValue(id, out var value))
				{
					return (T)value;
				}
				return SetStorage(tab, id2, @default);
			}
			catch (Exception ex)
			{
				Logger.Warn("Failed GetStorage<" + typeof(T).Name + ">(" + tab?.Id + ", " + id + "): " + ex.Message);
			}
			return default(T);
		}

		public T SetStorage<T>(Tab tab, string id, T value)
		{
			id = tab?.Id + "_" + id;
			LocalStorage[id] = value;
			return value;
		}

		public T SetDefaultStorage<T>(Tab tab, string id, T value)
		{
			if ((Object)(object)Player == (Object)null)
			{
				return default(T);
			}
			return GetStorage(tab, id, value);
		}

		public void ClearStorage(Tab tab, string id)
		{
			id = tab?.Id + "_" + id;
			LocalStorage[id] = null;
		}

		public bool HasStorage(Tab tab, string id)
		{
			id = tab?.Id + "_" + id;
			return LocalStorage.ContainsKey(id);
		}

		public void Clear()
		{
			foreach (KeyValuePair<int, Page> columnPage in ColumnPages)
			{
				Page page = ColumnPages[columnPage.Key];
				Pool.Free<Page>(ref page);
			}
			ColumnPages.Clear();
			_selectedDropdown = null;
			_selectedDropdownPage.CurrentPage = 0;
		}

		public Page GetOrCreatePage(int column)
		{
			if (ColumnPages.TryGetValue(column, out var value))
			{
				return value;
			}
			return ColumnPages[column] = Pool.Get<Page>();
		}

		public void Dispose()
		{
			Clear();
		}
	}

	public class Tab : IDisposable
	{
		public class TabDialog
		{
			public string Title;

			public Action<PlayerSession> OnConfirm;

			public Action<PlayerSession> OnDecline;

			public TabDialog(string title, Action<PlayerSession> onConfirm, Action<PlayerSession> onDecline)
			{
				Title = title;
				OnConfirm = onConfirm;
				OnDecline = onDecline;
			}
		}

		public class OptionPool : List<Option>
		{
			public Option pinnedOption;

			public void ClearToPool()
			{
				ReturnToPool();
				Clear();
			}

			public void ReturnToPool()
			{
				if (pinnedOption != null)
				{
					Pool.Free<Option>(ref pinnedOption);
				}
				for (int i = 0; i < base.Count; i++)
				{
					Option option = base[i];
					Pool.Free<Option>(ref option);
				}
			}
		}

		public class Option : IPooled
		{
			public string Name;

			public string Tooltip;

			public bool Hidden;

			public bool CurrentlyHidden;

			public Option()
			{
			}

			public Option(string name, string tooltip = null, bool hidden = false)
			{
				Name = name;
				Tooltip = tooltip;
				CurrentlyHidden = (Hidden = hidden);
			}

			public void EnterPool()
			{
			}

			public void LeavePool()
			{
			}
		}

		public class OptionName : Option
		{
			public TextAnchor Align;

			public OptionName()
			{
			}

			public OptionName(string name, TextAnchor align, string tooltip = null, bool hidden = false)
				: base(name, tooltip, hidden)
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				Align = align;
			}
		}

		public class OptionText : Option
		{
			public int Size;

			public string Color;

			public TextAnchor Align;

			public CUI.Handler.FontTypes Font;

			public bool IsInput;

			public OptionText()
			{
			}

			public OptionText(string name, int size, string color, TextAnchor align, CUI.Handler.FontTypes font, bool isInput, string tooltip = null, bool hidden = false)
				: base(name, tooltip, hidden)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				Align = align;
				Size = size;
				Color = color;
				Font = font;
				IsInput = isInput;
			}
		}

		public class OptionInput : Option
		{
			public Func<PlayerSession, string> Placeholder;

			public int CharacterLimit;

			public bool ReadOnly;

			public Action<PlayerSession, object[]> Callback;

			public OptionInput()
			{
			}

			public OptionInput(string name, Func<PlayerSession, string> placeholder, int characterLimit, bool readOnly, Action<PlayerSession, object[]> args, string tooltip = null, bool hidden = false)
				: base(name, tooltip, hidden)
			{
				Placeholder = delegate(PlayerSession ap)
				{
					try
					{
						return placeholder?.Invoke(ap);
					}
					catch (Exception ex)
					{
						Logger.Error("Failed OptionInput.Placeholder callback (" + name + "): " + ex.Message);
						return string.Empty;
					}
				};
				Callback = delegate(PlayerSession ap, object[] arg)
				{
					try
					{
						args?.Invoke(ap, arg);
					}
					catch (Exception ex)
					{
						Logger.Error("Failed OptionInput.Callback callback (" + name + "): " + ex.Message);
					}
				};
				CharacterLimit = characterLimit;
				ReadOnly = readOnly;
			}
		}

		public class OptionButton : Option
		{
			public enum Types
			{
				None,
				Selected,
				Warned,
				Important
			}

			public Func<PlayerSession, Types> Type;

			public Action<PlayerSession> Callback;

			public TextAnchor Align = (TextAnchor)4;

			public OptionButton()
			{
			}//IL_0002: Unknown result type (might be due to invalid IL or missing references)


			public OptionButton(string name, TextAnchor align, Action<PlayerSession> callback, Func<PlayerSession, Types> type = null, string tooltip = null, bool hidden = false)
				: base(name, tooltip, hidden)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0034: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				Align = align;
				Callback = delegate(PlayerSession ap)
				{
					try
					{
						callback?.Invoke(ap);
					}
					catch (Exception ex)
					{
						Logger.Error("Failed OptionButton.Callback callback (" + name + "): " + ex.Message);
					}
				};
				Type = delegate(PlayerSession ap)
				{
					try
					{
						return type?.Invoke(ap) ?? Types.None;
					}
					catch (Exception ex)
					{
						Logger.Error("Failed OptionButton.Type callback (" + name + "): " + ex.Message);
						return Types.None;
					}
				};
			}

			public OptionButton(string name, Action<PlayerSession> callback, Func<PlayerSession, Types> type = null, string tooltip = null, bool hidden = false)
				: base(name, tooltip, hidden)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				Callback = callback;
				Type = type;
			}
		}

		public class OptionToggle : Option
		{
			public Func<PlayerSession, bool> IsOn;

			public Action<PlayerSession> Callback;

			public OptionToggle()
			{
			}

			public OptionToggle(string name, Action<PlayerSession> callback, Func<PlayerSession, bool> isOn = null, string tooltip = null, bool hidden = false)
				: base(name, tooltip, hidden)
			{
				Callback = delegate(PlayerSession ap)
				{
					try
					{
						callback?.Invoke(ap);
					}
					catch (Exception ex)
					{
						Logger.Error("Failed OptionToggle.Callback callback (" + name + "): " + ex.Message);
					}
				};
				IsOn = delegate(PlayerSession ap)
				{
					try
					{
						return isOn?.Invoke(ap) ?? false;
					}
					catch (Exception ex)
					{
						Logger.Error("Failed OptionToggle.IsOn callback (" + name + "): " + ex.Message);
						return false;
					}
				};
			}
		}

		public class OptionEnum : Option
		{
			public Func<PlayerSession, string> Text;

			public Action<PlayerSession, bool> Callback;

			public OptionEnum()
			{
			}

			public OptionEnum(string name, Action<PlayerSession, bool> callback, Func<PlayerSession, string> text, string tooltip = null, bool hidden = false)
				: base(name, tooltip, hidden)
			{
				Callback = delegate(PlayerSession ap, bool value)
				{
					try
					{
						callback?.Invoke(ap, value);
					}
					catch (Exception ex)
					{
						Logger.Error("Failed OptionEnum.Callback callback (" + name + "): " + ex.Message);
					}
				};
				Text = delegate(PlayerSession ap)
				{
					try
					{
						return text?.Invoke(ap);
					}
					catch (Exception ex)
					{
						Logger.Error("Failed OptionToggle.Callback callback (" + name + "): " + ex.Message);
						return string.Empty;
					}
				};
			}
		}

		public class OptionRange : Option
		{
			public float Min;

			public float Max = 1f;

			public Func<PlayerSession, float> Value;

			public Action<PlayerSession, float> Callback;

			public Func<PlayerSession, string> Text;

			public OptionRange()
			{
			}

			public OptionRange(string name, float min, float max, Func<PlayerSession, float> value, Action<PlayerSession, float> callback, Func<PlayerSession, string> text, string tooltip = null, bool hidden = false)
				: base(name, tooltip, hidden)
			{
				Min = min;
				Max = max;
				Callback = delegate(PlayerSession ap, float arg)
				{
					try
					{
						callback?.Invoke(ap, arg);
					}
					catch (Exception ex)
					{
						Logger.Error("Failed OptionRange.Callback callback (" + name + "): " + ex.Message);
					}
				};
				Value = delegate(PlayerSession ap)
				{
					try
					{
						return value?.Invoke(ap) ?? 0f;
					}
					catch (Exception ex)
					{
						Logger.Error("Failed OptionRange.Callback callback (" + name + "): " + ex.Message);
						return 0f;
					}
				};
				Text = delegate(PlayerSession ap)
				{
					try
					{
						return text?.Invoke(ap);
					}
					catch (Exception ex)
					{
						Logger.Error("Failed OptionRange.Callback callback (" + name + "): " + ex.Message);
						return string.Empty;
					}
				};
			}
		}

		public class OptionDropdown : Option
		{
			public Func<PlayerSession, int> Index;

			public Action<PlayerSession, int> Callback;

			public string[] Options;

			public string[] OptionsIcons;

			public OptionDropdown()
			{
			}

			public OptionDropdown(string name, Func<PlayerSession, int> index, Action<PlayerSession, int> callback, string[] options, string[] optionsIcons, string tooltip = null, bool hidden = false)
				: base(name, tooltip, hidden)
			{
				Index = delegate(PlayerSession ap)
				{
					try
					{
						return index?.Invoke(ap) ?? 0;
					}
					catch (Exception ex)
					{
						Logger.Error("Failed OptionRange.Callback callback (" + name + "): " + ex.Message);
						return 0;
					}
				};
				Callback = delegate(PlayerSession ap, int value)
				{
					try
					{
						callback?.Invoke(ap, value);
					}
					catch (Exception ex)
					{
						Logger.Error("Failed OptionRange.Callback callback (" + name + "): " + ex.Message);
					}
				};
				Options = options;
				OptionsIcons = optionsIcons;
			}
		}

		public class OptionInputButton : Option
		{
			public OptionInput Input;

			public OptionButton Button;

			public float ButtonPriority = 0.25f;

			public OptionInputButton()
			{
			}

			public OptionInputButton(string name, float buttonPriority, OptionInput input, OptionButton button, string tooltip = null, bool hidden = false)
				: base(name, tooltip, hidden)
			{
				ButtonPriority = buttonPriority;
				Input = input;
				Button = button;
			}
		}

		public class OptionButtonArray : Option
		{
			public OptionButton[] Buttons;

			public OptionButtonArray()
			{
			}

			public OptionButtonArray(string name, string tooltip = null, bool hidden = false, params OptionButton[] buttons)
				: base(name, tooltip, hidden)
			{
				Buttons = buttons;
			}
		}

		public class OptionColor : Option
		{
			public Func<string> Color;

			public Action<PlayerSession, string, string, float> Callback;

			public OptionColor()
			{
			}

			public OptionColor(string name, Func<string> color, Action<PlayerSession, string, string, float> callback, string tooltip = null, bool hidden = false)
				: base(name, tooltip, hidden)
			{
				Color = color;
				Callback = callback;
			}
		}

		public class OptionWidget : Option
		{
			public int Height = 1;

			public string WidgetPanel;

			public Action<PlayerSession, CUI, CuiElementContainer, string> Callback;

			public OptionWidget()
			{
			}

			public OptionWidget(string name, int height, Action<PlayerSession, CUI, CuiElementContainer, string> callback, string tooltip = null, bool hidden = false)
				: base(name, tooltip, hidden)
			{
				Height = height;
				Callback = callback;
			}
		}

		public class OptionSpace : Option
		{
			public OptionSpace()
			{
			}

			public OptionSpace(string name, string tooltip = null, bool hidden = false)
				: base(name, tooltip, hidden)
			{
			}
		}

		public class OptionChart : Option
		{
			public class ChartCacheDatabase : Dictionary<string, ChartCache>
			{
				public ChartCache GetOrProcessCache(string identifier, Chart chart, Action<ChartCache> onProcessed)
				{
					if (string.IsNullOrEmpty(identifier))
					{
						return default(ChartCache);
					}
					if (TryGetValue(identifier, out var chartCache) && chartCache.Status != ChartCache.StatusTypes.Failure)
					{
						onProcessed?.Invoke(chartCache);
						return chartCache;
					}
					chartCache.Dispose();
					chartCache = default(ChartCache);
					chartCache.ViewingPool = new List<ulong>();
					chartCache.Status = ChartCache.StatusTypes.Processing;
					chart.StartProcess(delegate(byte[] data, Exception exception)
					{
						if (exception != null)
						{
							chartCache.Status = ChartCache.StatusTypes.Failure;
							base[identifier] = chartCache;
							onProcessed?.Invoke(chartCache);
						}
						else
						{
							chartCache.Status = ChartCache.StatusTypes.Finalized;
							chartCache.Crc = FileStorage.server.GetCRC(data, (Type)0);
							chartCache.Data = data;
							base[identifier] = chartCache;
							onProcessed?.Invoke(chartCache);
						}
					});
					return base[identifier] = chartCache;
				}

				public void ClearPlayerViewer(ulong player)
				{
					using Enumerator enumerator = GetEnumerator();
					while (enumerator.MoveNext())
					{
						enumerator.Current.Value.ClearPlayerViewer(player);
					}
				}
			}

			public struct ChartCache
			{
				public enum StatusTypes
				{
					Finalized,
					Processing,
					Failure
				}

				public StatusTypes Status;

				public uint Crc;

				public byte[] Data;

				public List<ulong> ViewingPool;

				public void ClearPlayerViewer(ulong player)
				{
					ViewingPool.RemoveAll((ulong x) => x == player);
				}

				public bool HasPlayerReceivedData(ulong player)
				{
					bool flag = ViewingPool.Contains(player);
					if (!flag)
					{
						ViewingPool.Add(player);
					}
					return flag;
				}

				public void Dispose()
				{
					if (Data != null)
					{
						Array.Clear(Data, 0, Data.Length);
					}
					Data = null;
					Crc = 0u;
					Status = StatusTypes.Finalized;
					ViewingPool?.Clear();
					ViewingPool = null;
				}
			}

			public static ChartCacheDatabase Cache = new ChartCacheDatabase();

			public bool Responsive;

			public int NameSize = 20;

			public TextAnchor NameAlign;

			public const int Height = 8;

			public Chart.ChartSettings Settings;

			public Chart Chart;

			internal string _identifier { get; private set; }

			public string GetIdentifier(bool reset = false)
			{
				if (reset || string.IsNullOrEmpty(_identifier))
				{
					_identifier = GenerateIdentifier();
				}
				return _identifier;
			}

			public string GenerateIdentifier()
			{
				return string.Format("chart_{0}_{1}{2}", Chart.Name.Replace(" ", string.Empty), (from x in Chart.Layers
					where !x.Disabled
					select string.Format("{0}_{1}", x.Name.Replace(" ", string.Empty), x.LayerSettings.Shadows)).ToString("_"), Chart.Layers.Where((Chart.Layer x) => !x.Disabled).SumULong((Chart.Layer x) => (ulong)((long)x.Name.Length + (long)x.Data.SumULong((ulong y) => y) + ((!x.Disabled) ? 1 : 0))));
			}

			public bool IsEmpty()
			{
				return Chart.Layers.Length == 0;
			}

			public void Setup(string name, TextAnchor nameAlign, int nameSize, Chart.Layer[] layers, string[] verticalLabels, IEnumerable<string> horizontalLabels, Chart.ChartSettings settings, bool responsive)
			{
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				Name = name;
				NameAlign = nameAlign;
				NameSize = nameSize;
				Responsive = responsive;
				Chart = Chart.Create(name, 10750, 600, settings, new Chart.ChartRect
				{
					Width = 10600f,
					Height = 450f,
					X = 75f,
					Y = 100f
				}, layers, verticalLabels, horizontalLabels.ToArray(), Brushes.White, Color.Transparent);
				GetIdentifier();
			}
		}

		public string Id;

		public string Name;

		public string Access;

		public RustPlugin Plugin;

		public Action<Tab, CUI, CuiElementContainer, string, PlayerSession> Over;

		public Action<Tab, CUI, CuiElementContainer, string, PlayerSession> Under;

		public Action<Tab, CUI, CuiElementContainer, string, PlayerSession> Override;

		public Dictionary<int, OptionPool> Columns = new Dictionary<int, OptionPool>();

		public Action<PlayerSession, Tab> OnChange;

		public TabDialog Dialog;

		public bool IsFullscreen;

		public Tab(string id, string name, RustPlugin plugin, Action<PlayerSession, Tab> onChange = null, string access = null)
		{
			Id = id;
			Name = name;
			Plugin = plugin;
			OnChange = onChange;
			Access = access;
		}

		public void ClearColumn(int column, bool erase = false)
		{
			if (Columns.TryGetValue(column, out var value))
			{
				value.ClearToPool();
				if (erase)
				{
					Columns[column] = null;
					Columns.Remove(column);
				}
			}
		}

		public void ClearAfter(int index, bool erase = false)
		{
			int count = Columns.Count;
			for (int i = 0; i < count; i++)
			{
				if (i >= index)
				{
					ClearColumn(i, erase);
				}
			}
		}

		public Tab AddColumn(int column, bool clear = false)
		{
			if (!Columns.TryGetValue(column, out var value))
			{
				value = (Columns[column] = new OptionPool());
			}
			if (clear)
			{
				value.ClearToPool();
			}
			return this;
		}

		public Tab AddRow(int column, Option row, bool hidden = false)
		{
			bool flag = column < 0;
			column = Mathf.Abs(column) - (flag ? 1 : 0);
			row.CurrentlyHidden = (row.Hidden = hidden);
			if (!Columns.TryGetValue(column, out var value))
			{
				value = (Columns[column] = new OptionPool());
			}
			if (flag)
			{
				value.pinnedOption = row;
			}
			else
			{
				value.Add(row);
			}
			return this;
		}

		public Tab InsertRow(int column, int index, Option row, bool hidden = false)
		{
			row.CurrentlyHidden = (row.Hidden = hidden);
			if (!Columns.TryGetValue(column, out var value))
			{
				value = (Columns[column] = new OptionPool());
			}
			value.Insert(index, row);
			return this;
		}

		public Tab AddName(int column, string name, TextAnchor align = (TextAnchor)3, bool hidden = false)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			OptionName optionName = Pool.Get<OptionName>();
			optionName.Name = name;
			optionName.Align = align;
			return AddRow(column, optionName, hidden);
		}

		public Tab AddButton(int column, string name, Action<PlayerSession> callback, Func<PlayerSession, OptionButton.Types> type = null, TextAnchor align = (TextAnchor)4, bool hidden = false)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			OptionButton optionButton = Pool.Get<OptionButton>();
			optionButton.Name = name;
			optionButton.Align = align;
			optionButton.Callback = callback;
			optionButton.Type = type;
			return AddRow(column, optionButton, hidden);
		}

		public Tab AddToggle(int column, string name, Action<PlayerSession> callback, Func<PlayerSession, bool> isOn = null, string tooltip = null, bool hidden = false)
		{
			OptionToggle optionToggle = Pool.Get<OptionToggle>();
			optionToggle.Name = name;
			optionToggle.Callback = callback;
			optionToggle.IsOn = delegate(PlayerSession ap)
			{
				try
				{
					return isOn?.Invoke(ap) ?? false;
				}
				catch (Exception ex)
				{
					Logger.Error($"AddToggle[{column}][{name}] failed", ex);
				}
				return false;
			};
			optionToggle.Tooltip = tooltip;
			return AddRow(column, optionToggle, hidden);
		}

		public Tab AddText(int column, string name, int size, string color, TextAnchor align = (TextAnchor)4, CUI.Handler.FontTypes font = CUI.Handler.FontTypes.RobotoCondensedRegular, bool isInput = false, bool hidden = false)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			OptionText optionText = Pool.Get<OptionText>();
			optionText.Name = name;
			optionText.Size = size;
			optionText.Color = color;
			optionText.Align = align;
			optionText.Font = font;
			optionText.IsInput = isInput;
			return AddRow(column, optionText, hidden);
		}

		public Tab AddInput(int column, string name, Func<PlayerSession, string> placeholder, int characterLimit, bool readOnly, Action<PlayerSession, object[]> callback = null, string tooltip = null, bool hidden = false)
		{
			OptionInput optionInput = Pool.Get<OptionInput>();
			optionInput.Name = name;
			optionInput.Placeholder = placeholder;
			optionInput.CharacterLimit = characterLimit;
			optionInput.ReadOnly = readOnly;
			optionInput.Callback = callback;
			optionInput.Tooltip = tooltip;
			return AddRow(column, optionInput, hidden);
		}

		public Tab AddInput(int column, string name, Func<PlayerSession, string> placeholder, Action<PlayerSession, object[]> callback = null, string tooltip = null, bool hidden = false)
		{
			return AddInput(column, name, placeholder, 0, callback == null, callback, tooltip, hidden);
		}

		public Tab AddEnum(int column, string name, Action<PlayerSession, bool> callback, Func<PlayerSession, string> text, string tooltip = null, bool hidden = false)
		{
			OptionEnum optionEnum = Pool.Get<OptionEnum>();
			optionEnum.Name = name;
			optionEnum.Callback = callback;
			optionEnum.Text = text;
			optionEnum.Tooltip = tooltip;
			return AddRow(column, optionEnum, hidden);
		}

		public Tab AddDropdown(int column, string name, Func<PlayerSession, int> index, Action<PlayerSession, int> callback, string[] options, string[] optionsIcons = null, float optionsIconScale = 0f, string tooltip = null, bool hidden = false)
		{
			OptionDropdown optionDropdown = Pool.Get<OptionDropdown>();
			optionDropdown.Name = name;
			optionDropdown.Index = index;
			optionDropdown.Callback = callback;
			optionDropdown.Options = options;
			optionDropdown.OptionsIcons = optionsIcons;
			optionDropdown.Tooltip = tooltip;
			return AddRow(column, optionDropdown, hidden);
		}

		public Tab AddRange(int column, string name, float min, float max, Func<PlayerSession, float> value, Action<PlayerSession, float> callback, Func<PlayerSession, string> text = null, string tooltip = null, bool hidden = false)
		{
			OptionRange optionRange = Pool.Get<OptionRange>();
			optionRange.Name = name;
			optionRange.Min = min;
			optionRange.Max = max;
			optionRange.Value = value;
			optionRange.Callback = callback;
			optionRange.Text = text;
			optionRange.Tooltip = tooltip;
			return AddRow(column, optionRange, hidden);
		}

		public Tab AddButtonArray(int column, params OptionButton[] buttons)
		{
			OptionButtonArray optionButtonArray = Pool.Get<OptionButtonArray>();
			optionButtonArray.Name = string.Empty;
			optionButtonArray.Buttons = buttons;
			return AddRow(column, optionButtonArray);
		}

		public Tab AddInputButton(int column, string name, float buttonPriority, OptionInput input, OptionButton button, string tooltip = null, bool hidden = false)
		{
			OptionInputButton optionInputButton = Pool.Get<OptionInputButton>();
			optionInputButton.Name = name;
			optionInputButton.ButtonPriority = buttonPriority;
			optionInputButton.Input = input;
			optionInputButton.Button = button;
			optionInputButton.Tooltip = tooltip;
			return AddRow(column, optionInputButton, hidden);
		}

		public Tab AddColor(int column, string name, Func<string> color, Action<PlayerSession, string, string, float> callback, string tooltip = null, bool hidden = false)
		{
			OptionColor optionColor = Pool.Get<OptionColor>();
			optionColor.Name = name;
			optionColor.Color = color;
			optionColor.Callback = callback;
			optionColor.Tooltip = tooltip;
			return AddRow(column, optionColor, hidden);
		}

		public Tab AddWidget(int column, int height, Action<PlayerSession, CUI, CuiElementContainer, string> callback)
		{
			for (int i = 0; i < height; i++)
			{
				AddRow(column, Pool.Get<OptionSpace>());
			}
			OptionWidget optionWidget = Pool.Get<OptionWidget>();
			optionWidget.Name = string.Empty;
			optionWidget.Height = height;
			optionWidget.Callback = callback;
			return AddRow(column, optionWidget);
		}

		public Tab AddChart(int column, string name, TextAnchor nameAlign, int nameSize, Chart.Layer[] layers, string[] verticalLabels, IEnumerable<string> horizontalLabels, Chart.ChartSettings settings, bool responsive = true)
		{
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			int count = Columns[column].Count;
			for (int i = 0; i < 8; i++)
			{
				AddRow(column, Pool.Get<OptionSpace>());
			}
			if (!responsive)
			{
				for (int j = 1; j < Columns.Count; j++)
				{
					for (int k = 0; k < 9; k++)
					{
						InsertRow(j, count, Pool.Get<OptionSpace>());
					}
				}
			}
			OptionChart optionChart = Pool.Get<OptionChart>();
			optionChart.Setup(name, nameAlign, nameSize, layers, verticalLabels, horizontalLabels, settings, responsive);
			return AddRow(column, optionChart);
		}

		public Tab AddSpace(int column)
		{
			OptionSpace row = Pool.Get<OptionSpace>();
			return AddRow(column, row);
		}

		public void CreateDialog(string title, Action<PlayerSession> onConfirm, Action<PlayerSession> onDecline = null)
		{
			Dialog = new TabDialog(title, onConfirm, onDecline);
		}

		public void ResetHiddens()
		{
			foreach (KeyValuePair<int, OptionPool> column in Columns)
			{
				foreach (Option item in column.Value)
				{
					item.CurrentlyHidden = item.Hidden;
				}
			}
		}

		public virtual void Dispose()
		{
			foreach (KeyValuePair<int, OptionPool> column in Columns)
			{
				column.Value.ClearToPool();
			}
			Columns.Clear();
			Columns = null;
		}
	}

	public class CarbonTab
	{
		public static Tab Instance;

		internal static readonly string[] LogFileModes = new string[3] { "Disabled", "Save every 5 min.", "Save immediately" };

		internal static readonly string[] LogVerbosity = new string[7] { "Normal", "Level 1", "Level 2", "Level 3", "Level 4", "Level 5", "Level 6" };

		internal static readonly string[] SearchDirectories = new string[2] { "Primary", "All" };

		internal static readonly string[] TabTypes = new string[1] { "Quick Actions" };

		public static Config Config => Community.Runtime.Config;

		public static Tab Get()
		{
			Instance = new Tab("carbon", "Carbon", Community.Runtime.Core, delegate(PlayerSession ap, Tab t)
			{
				ap.SetStorage(t, "carbontabedit", value: false);
				Refresh(t, ap);
			}, "carbon.use");
			Instance.AddColumn(0);
			Instance.AddColumn(1);
			return Instance;
		}

		public static void Refresh(Tab tab, PlayerSession ap)
		{
			tab.ClearColumn(0);
			tab.ClearColumn(1);
			if (!Singleton.HasAccess(ap.Player, "carbon.use"))
			{
				return;
			}
			if (Singleton.HasAccess(ap.Player, "carbon.server_settings"))
			{
				tab.AddInput(0, Singleton.GetPhrase("hostname", ap.Player.UserIDString), (PlayerSession playerSession) => Server.hostname ?? "", delegate(PlayerSession playerSession, object[] args)
				{
					string str = ((args.Length != 0) ? args.Select((object x) => x as string).ToString(" ") : Server.hostname);
					tab.CreateDialog("Are you sure you want to update the host name?", delegate
					{
						Server.hostname = str;
					});
				});
				tab.AddInput(0, Singleton.GetPhrase("maxplayers", ap.Player.UserIDString), (PlayerSession playerSession) => $"{Server.maxplayers}", delegate(PlayerSession playerSession, object[] args)
				{
					int val = ((args.Length != 0) ? ((string)args[0]).ToInt() : Server.maxplayers);
					tab.CreateDialog("Are you sure you want to update the maximum players that can join the server?", delegate
					{
						Server.maxplayers = val;
					});
				});
				tab.AddInput(0, Singleton.GetPhrase("level", ap.Player.UserIDString), (PlayerSession playerSession) => Server.level ?? "");
			}
			if (Singleton.HasAccess(ap.Player, "carbon.server_info"))
			{
				tab.AddName(0, Singleton.GetPhrase("info", ap.Player.UserIDString), (TextAnchor)3);
				tab.AddInput(0, Singleton.GetPhrase("version", ap.Player.UserIDString), (PlayerSession playerSession) => Community.Runtime.Analytics.Version ?? "");
				tab.AddInput(0, Singleton.GetPhrase("version2", ap.Player.UserIDString), (PlayerSession playerSession) => Community.Runtime.Analytics.InformationalVersion ?? "");
				int loadedHooks = Community.Runtime.HookManager.LoadedDynamicHooks.Count((IHook x) => x.IsInstalled) + Community.Runtime.HookManager.LoadedStaticHooks.Count((IHook x) => x.IsInstalled);
				int totalHooks = Community.Runtime.HookManager.LoadedDynamicHooks.Count() + Community.Runtime.HookManager.LoadedStaticHooks.Count();
				tab.AddInput(0, Singleton.GetPhrase("hooks", ap.Player.UserIDString), (PlayerSession playerSession) => $"<b>{loadedHooks:n0}</b> / {totalHooks:n0} loaded");
				tab.AddInput(0, Singleton.GetPhrase("statichooks", ap.Player.UserIDString), (PlayerSession playerSession) => $"{Community.Runtime.HookManager.LoadedStaticHooks.Count():n0}");
				tab.AddInput(0, Singleton.GetPhrase("dynamichooks", ap.Player.UserIDString), (PlayerSession playerSession) => $"{Community.Runtime.HookManager.LoadedDynamicHooks.Count():n0}");
				tab.AddName(0, Singleton.GetPhrase("plugins", ap.Player.UserIDString), (TextAnchor)3);
				tab.AddInput(0, Singleton.GetPhrase("mods", ap.Player.UserIDString), (PlayerSession playerSession) => $"{Community.Runtime.Plugins.Plugins.Count:n0}");
				if (!Singleton.ConfigInstance.HideConsole && Singleton.HasAccess(ap.Player, "carbon.server_console"))
				{
					tab.AddName(0, Singleton.GetPhrase("console", ap.Player.UserIDString), (TextAnchor)3);
					foreach (string item in _logQueue)
					{
						tab.AddText(0, item, 8, "1 1 1 0.85", (TextAnchor)3, CUI.Handler.FontTypes.DroidSansMono, isInput: true);
					}
					tab.AddInputButton(0, Singleton.GetPhrase("execservercmd", ap.Player.UserIDString), 0.2f, new Tab.OptionInput(null, null, 0, readOnly: false, delegate(PlayerSession ap2, object[] args)
					{
						//IL_001d: Unknown result type (might be due to invalid IL or missing references)
						string text = ((args.Length != 0) ? ((string)args[0]) : string.Empty);
						if (!string.IsNullOrEmpty(text))
						{
							ConsoleSystem.Run(Option.Server, text, (object[])null);
							Refresh(tab, ap2);
						}
					}), new Tab.OptionButton("Refresh", delegate(PlayerSession ap2)
					{
						Refresh(tab, ap2);
					}));
				}
			}
			if (Singleton.HasAccess(ap.Player, "carbon.quickactions"))
			{
				tab.AddName(1, Singleton.GetPhrase("quickactions", ap.Player.UserIDString), (TextAnchor)3);
				bool editMode = Singleton.HasAccess(ap.Player, "carbon.quickactions.edit") && ap.GetStorage(tab, "carbontabedit", @default: false);
				foreach (AdminConfig.ActionButton action in Singleton.ConfigInstance.QuickActions)
				{
					tab.AddButton(1, editMode ? (action.Name + " (" + action.Command + ")" + (action.User ? " [user]" : string.Empty) + (action.IncludeUserId ? " [incl.user]" : string.Empty)) : action.Name, delegate(PlayerSession ap2)
					{
						if (editMode)
						{
							Singleton.ConfigInstance.QuickActions.RemoveAll((AdminConfig.ActionButton x) => x.Name == action.Name);
							Singleton.Save();
							Refresh(tab, ap2);
						}
						else if (action.ConfirmDialog)
						{
							tab.CreateDialog("Are you sure you want to execute?", delegate(PlayerSession ap3)
							{
								Execute(action, ap3);
							});
						}
						else
						{
							Execute(action, ap2);
						}
					}, (PlayerSession playerSession) => Tab.OptionButton.Types.Selected, (TextAnchor)4);
				}
				if (editMode)
				{
					tab.AddText(1, "Click on existent buttons above to delete. Separate commands with | if you want multiple commands per button.", 10, "1 1 1 0.5", (TextAnchor)4);
					tab.AddInput(1, Singleton.GetPhrase("quickactions_name", ap.Player.UserIDString), (PlayerSession playerSession) => playerSession.GetStorage(tab, "carbontabbtnname", string.Empty), delegate(PlayerSession playerSession, object[] args)
					{
						playerSession.SetStorage(tab, "carbontabbtnname", (args.Length != 0) ? ((string)args[0]) : string.Empty);
					}, Singleton.GetPhrase("quickactions_name_help", ap.Player.UserIDString));
					tab.AddInput(1, Singleton.GetPhrase("quickactions_command", ap.Player.UserIDString), (PlayerSession playerSession) => playerSession.GetStorage(tab, "carbontabbtncmd", string.Empty), delegate(PlayerSession playerSession, object[] args)
					{
						playerSession.SetStorage(tab, "carbontabbtncmd", (args.Length != 0) ? ((string)args[0]) : string.Empty);
					}, Singleton.GetPhrase("quickactions_command_help", ap.Player.UserIDString));
					tab.AddToggle(1, Singleton.GetPhrase("quickactions_user", ap.Player.UserIDString), delegate(PlayerSession playerSession)
					{
						playerSession.SetStorage(tab, "carbontabbtnuser", !playerSession.GetStorage(tab, "carbontabbtnuser", @default: false));
					}, (PlayerSession playerSession) => playerSession.GetStorage(tab, "carbontabbtnuser", @default: false), Singleton.GetPhrase("quickactions_user_help", ap.Player.UserIDString));
					tab.AddToggle(1, Singleton.GetPhrase("quickactions_incluserid", ap.Player.UserIDString), delegate(PlayerSession playerSession)
					{
						playerSession.SetStorage(tab, "carbontabbtnincludeuserid", !playerSession.GetStorage(tab, "carbontabbtnincludeuserid", @default: false));
					}, (PlayerSession playerSession) => playerSession.GetStorage(tab, "carbontabbtnincludeuserid", @default: false), Singleton.GetPhrase("quickactions_incluserid_help", ap.Player.UserIDString));
					tab.AddToggle(1, Singleton.GetPhrase("quickactions_confirmdialog", ap.Player.UserIDString), delegate(PlayerSession playerSession)
					{
						playerSession.SetStorage(tab, "carbontabbtnconfirmdialog", !playerSession.GetStorage(tab, "carbontabbtnconfirmdialog", @default: false));
					}, (PlayerSession playerSession) => playerSession.GetStorage(tab, "carbontabbtnconfirmdialog", @default: false), Singleton.GetPhrase("quickactions_confirmdialog_help", ap.Player.UserIDString));
					tab.AddButton(1, Singleton.GetPhrase("quickactions_add", ap.Player.UserIDString), delegate(PlayerSession playerSession)
					{
						string storage = playerSession.GetStorage(tab, "carbontabbtnname", string.Empty);
						string storage2 = playerSession.GetStorage(tab, "carbontabbtncmd", string.Empty);
						bool storage3 = playerSession.GetStorage(tab, "carbontabbtnuser", @default: false);
						bool storage4 = playerSession.GetStorage(tab, "carbontabbtnincludeuserid", @default: false);
						bool storage5 = playerSession.GetStorage(tab, "carbontabbtnconfirmdialog", @default: false);
						if (!string.IsNullOrEmpty(storage) && !string.IsNullOrEmpty(storage2))
						{
							Singleton.ConfigInstance.QuickActions.Add(new AdminConfig.ActionButton
							{
								Name = storage,
								Command = storage2,
								User = storage3,
								IncludeUserId = storage4,
								ConfirmDialog = storage5
							});
							Singleton.Save();
							playerSession.SetStorage(tab, "carbontabbtnname", string.Empty);
							playerSession.SetStorage(tab, "carbontabbtncmd", string.Empty);
							playerSession.SetStorage(tab, "carbontabbtnuser", value: false);
							playerSession.SetStorage(tab, "carbontabbtnincludeuserid", value: false);
							playerSession.SetStorage(tab, "carbontabbtnconfirmdialog", value: false);
							Refresh(tab, playerSession);
						}
					}, (PlayerSession playerSession) => Tab.OptionButton.Types.Selected, (TextAnchor)4);
				}
				if (Singleton.HasAccess(ap.Player, "carbon.quickactions.edit"))
				{
					tab.AddButton(1, Singleton.GetPhrase(editMode ? "quickactions_stopedit" : "quickactions_edit", ap.Player.UserIDString), delegate(PlayerSession playerSession)
					{
						playerSession.SetStorage(tab, "carbontabedit", !editMode);
						Refresh(tab, playerSession);
					}, (PlayerSession playerSession) => editMode ? Tab.OptionButton.Types.Important : Tab.OptionButton.Types.None, (TextAnchor)4);
				}
			}
			if (!Singleton.HasAccess(ap.Player, "carbon.server_config"))
			{
				return;
			}
			tab.AddName(1, Singleton.GetPhrase("general", ap.Player.UserIDString), (TextAnchor)3);
			tab.AddToggle(1, Singleton.GetPhrase("ismodded", ap.Player.UserIDString), delegate
			{
				Config.IsModded = !Config.IsModded;
				Community.Runtime.SaveConfig();
			}, (PlayerSession playerSession) => Config.IsModded, Singleton.GetPhrase("ismodded_help", ap.Player.UserIDString));
			tab.AddToggle(1, Singleton.GetPhrase("scriptwatchers", ap.Player.UserIDString), delegate
			{
				Config.Watchers.ScriptWatchers = !Config.Watchers.ScriptWatchers;
				Community.Runtime.SaveConfig();
			}, (PlayerSession playerSession) => Config.Watchers.ScriptWatchers, Singleton.GetPhrase("scriptwatchers_help", ap.Player.UserIDString));
			tab.AddDropdown(1, Singleton.GetPhrase("scriptwatchersoption", ap.Player.UserIDString), (PlayerSession playerSession) => (int)Config.Watchers.ScriptWatcherOption, delegate(PlayerSession playerSession, int num2)
			{
				Config.Watchers.ScriptWatcherOption = (SearchOption)num2;
				Community.Runtime.ScriptProcessor.IncludeSubdirectories = num2 == 1;
				Community.Runtime.SaveConfig();
			}, SearchDirectories, null, 0f, Singleton.GetPhrase("scriptwatchersoption_help", ap.Player.UserIDString));
			tab.AddToggle(1, Singleton.GetPhrase("zipscriptwatchers", ap.Player.UserIDString), delegate
			{
				Config.Watchers.ZipScriptWatchers = !Config.Watchers.ZipScriptWatchers;
				Community.Runtime.SaveConfig();
			}, (PlayerSession playerSession) => Config.Watchers.ZipScriptWatchers, Singleton.GetPhrase("zipscriptwatchers_help", ap.Player.UserIDString));
			tab.AddName(1, Singleton.GetPhrase("logging", ap.Player.UserIDString), (TextAnchor)3);
			tab.AddDropdown(1, Singleton.GetPhrase("logfilemode", ap.Player.UserIDString), (PlayerSession playerSession) => Config.Logging.LogFileMode, delegate(PlayerSession playerSession, int logFileMode)
			{
				Config.Logging.LogFileMode = logFileMode;
				Community.Runtime.SaveConfig();
			}, LogFileModes);
			tab.AddDropdown(1, Singleton.GetPhrase("logverbosity", ap.Player.UserIDString), (PlayerSession playerSession) => Config.Logging.LogVerbosity, delegate(PlayerSession playerSession, int logVerbosity)
			{
				Config.Logging.LogVerbosity = logVerbosity;
				Community.Runtime.SaveConfig();
			}, LogVerbosity);
			tab.AddDropdown(1, Singleton.GetPhrase("logseverity", ap.Player.UserIDString), (PlayerSession playerSession) => (int)Config.Logging.LogSeverity, delegate(PlayerSession playerSession, int logSeverity)
			{
				Config.Logging.LogSeverity = (Severity)logSeverity;
				Community.Runtime.SaveConfig();
			}, Enum.GetNames(typeof(Severity)));
			tab.AddName(1, Singleton.GetPhrase("misc", ap.Player.UserIDString), (TextAnchor)3);
			tab.AddInput(1, Singleton.GetPhrase("serverlang", ap.Player.UserIDString), (PlayerSession playerSession) => Config.Language, delegate(PlayerSession playerSession, object[] args)
			{
				Config.Language = ((args.Length == 0) ? Config.Language : args[0]?.ToString());
				Community.Runtime.SaveConfig();
			});
			tab.AddInput(1, Singleton.GetPhrase("webreqip", ap.Player.UserIDString), (PlayerSession playerSession) => Config.WebRequestIp, delegate(PlayerSession playerSession, object[] args)
			{
				if (args.Length != 0)
				{
					string text = args[0]?.ToString();
					if (string.IsNullOrEmpty(text) || (IPAddress.TryParse(text, out IPAddress _) && text.Contains(".")))
					{
						Config.WebRequestIp = text;
						Community.Runtime.SaveConfig();
					}
				}
			});
			tab.AddToggle(1, Singleton.GetPhrase("consoleinfo", ap.Player.UserIDString), delegate
			{
				Config.Misc.ShowConsoleInfo = !Config.Misc.ShowConsoleInfo;
				if (Config.Misc.ShowConsoleInfo)
				{
					Community.Runtime.RefreshConsoleInfo();
				}
				else if ((Object)(object)SingletonComponent<ServerConsole>.Instance != (Object)null && SingletonComponent<ServerConsole>.Instance.input != null)
				{
					SingletonComponent<ServerConsole>.Instance.input.statusText = new string[3];
				}
				Community.Runtime.SaveConfig();
			}, (PlayerSession playerSession) => Config.Misc.ShowConsoleInfo, Singleton.GetPhrase("consoleinfo_help", ap.Player.UserIDString));
			tab.AddName(1, Singleton.GetPhrase("permissions", ap.Player.UserIDString), (TextAnchor)3);
			tab.AddInput(1, Singleton.GetPhrase("playerdefgroup", ap.Player.UserIDString), (PlayerSession playerSession) => Config.Permissions.PlayerDefaultGroup, delegate(PlayerSession playerSession, object[] args)
			{
				Config.Permissions.PlayerDefaultGroup = ((args.Length == 0) ? Config.Permissions.PlayerDefaultGroup : args[0]?.ToString());
				if (string.IsNullOrEmpty(Config.Permissions.PlayerDefaultGroup))
				{
					Config.Permissions.PlayerDefaultGroup = "default";
				}
				Community.Runtime.SaveConfig();
			});
			tab.AddInput(1, Singleton.GetPhrase("admindefgroup", ap.Player.UserIDString), (PlayerSession playerSession) => Config.Permissions.AdminDefaultGroup, delegate(PlayerSession playerSession, object[] args)
			{
				Config.Permissions.AdminDefaultGroup = ((args.Length == 0) ? Config.Permissions.AdminDefaultGroup : args[0]?.ToString());
				if (string.IsNullOrEmpty(Config.Permissions.AdminDefaultGroup))
				{
					Config.Permissions.AdminDefaultGroup = "admin";
				}
				Community.Runtime.SaveConfig();
			});
			tab.AddInput(1, Singleton.GetPhrase("moderatordefgroup", ap.Player.UserIDString), (PlayerSession playerSession) => Config.Permissions.ModeratorDefaultGroup, delegate(PlayerSession playerSession, object[] args)
			{
				Config.Permissions.ModeratorDefaultGroup = ((args.Length == 0) ? Config.Permissions.ModeratorDefaultGroup : args[0]?.ToString());
				if (string.IsNullOrEmpty(Config.Permissions.ModeratorDefaultGroup))
				{
					Config.Permissions.ModeratorDefaultGroup = "moderator";
				}
				Community.Runtime.SaveConfig();
			});
			tab.AddName(1, Singleton.GetPhrase("conditionals", ap.Player.UserIDString), (TextAnchor)3);
			for (int num = 0; num < Config.Compiler.ConditionalCompilationSymbols.Count; num++)
			{
				int index = num;
				string symbol = Config.Compiler.ConditionalCompilationSymbols[num];
				tab.AddInputButton(1, string.Empty, 0.075f, new Tab.OptionInput(null, (PlayerSession playerSession) => symbol, 0, readOnly: false, delegate(PlayerSession ap2, object[] args)
				{
					Config.Compiler.ConditionalCompilationSymbols[index] = ((args.Length == 0) ? Config.Compiler.ConditionalCompilationSymbols[index] : args[0]?.ToString().ToUpper().Trim());
					Refresh(tab, ap2);
					Community.Runtime.SaveConfig();
				}), new Tab.OptionButton("X", delegate(PlayerSession ap2)
				{
					Config.Compiler.ConditionalCompilationSymbols.RemoveAt(index);
					Refresh(tab, ap2);
					Community.Runtime.SaveConfig();
				}, (PlayerSession playerSession) => Tab.OptionButton.Types.Important));
			}
			tab.AddInputButton(1, string.Empty, 0.075f, new Tab.OptionInput(null, (PlayerSession playerSession) => playerSession.GetStorage<string>(tab, "conditional"), 0, readOnly: false, delegate(PlayerSession playerSession, object[] args)
			{
				playerSession.SetStorage(tab, "conditional", (args.Length == 0) ? string.Empty : args[0]?.ToString().ToUpper().Trim());
			}), new Tab.OptionButton("+", delegate(PlayerSession playerSession)
			{
				string storage = playerSession.GetStorage<string>(tab, "conditional");
				if (!string.IsNullOrEmpty(storage))
				{
					Config.Compiler.ConditionalCompilationSymbols.Add(storage);
					playerSession.SetStorage(tab, "conditional", string.Empty);
					Refresh(tab, playerSession);
					Community.Runtime.SaveConfig();
				}
			}, (PlayerSession playerSession) => Tab.OptionButton.Types.Selected));
			tab.AddName(1, Singleton.GetPhrase("debugging", ap.Player.UserIDString), (TextAnchor)3);
			tab.AddInput(1, Singleton.GetPhrase("scriptdebugorigin", ap.Player.UserIDString), (PlayerSession playerSession) => Config.Debugging.ScriptDebuggingOrigin, delegate(PlayerSession playerSession, object[] args)
			{
				Config.Debugging.ScriptDebuggingOrigin = ((args.Length == 0) ? Config.Debugging.ScriptDebuggingOrigin : args[0]?.ToString());
				Community.Runtime.SaveConfig();
			}, Singleton.GetPhrase("scriptdebugorigin_help", ap.Player.UserIDString));
			static void Execute(AdminConfig.ActionButton actionButton, PlayerSession playerSession)
			{
				//IL_0059: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
				if (!actionButton.Command.Contains("|"))
				{
					if (actionButton.User)
					{
						playerSession.Player.SendConsoleCommand(actionButton.IncludeUserId ? (actionButton.Command + " " + playerSession.Player.UserIDString) : actionButton.Command, Array.Empty<object>());
					}
					else
					{
						ConsoleSystem.Run(Option.Server, actionButton.IncludeUserId ? (actionButton.Command + " " + playerSession.Player.UserIDString) : actionButton.Command, Array.Empty<object>());
					}
				}
				else
				{
					string[] array = actionButton.Command.Split('|');
					string[] array2 = array;
					foreach (string text in array2)
					{
						if (actionButton.User)
						{
							playerSession.Player.SendConsoleCommand(actionButton.IncludeUserId ? (text + " " + playerSession.Player.UserIDString) : text, Array.Empty<object>());
						}
						else
						{
							ConsoleSystem.Run(Option.Server, actionButton.IncludeUserId ? (text + " " + playerSession.Player.UserIDString) : text, Array.Empty<object>());
						}
					}
				}
			}
		}
	}

	public class ConfigEditor : Tab
	{
		internal Action<PlayerSession, JObject> OnSave;

		internal Action<PlayerSession, JObject> OnSaveAndReload;

		internal Action<PlayerSession, JObject> OnCancel;

		internal const string Spacing = " ";

		internal string[] Blacklist;

		private const string LegendSuffix = "Legend";

		internal Dictionary<string, int> LegendIndex;

		internal Dictionary<string, string[]> LegendOptions;

		internal JObject Entry { get; set; }

		public ConfigEditor(string id, string name, RustPlugin plugin, Action<PlayerSession, Tab> onChange = null)
			: base(id, name, plugin, onChange)
		{
			LegendIndex = Pool.Get<Dictionary<string, int>>();
			LegendOptions = Pool.Get<Dictionary<string, string[]>>();
		}

		public static ConfigEditor Make(string json, Action<PlayerSession, JObject> onCancel, Action<PlayerSession, JObject> onSave, Action<PlayerSession, JObject> onSaveAndReload, bool fullscreen = false, string[] blacklist = null)
		{
			ConfigEditor configEditor = new ConfigEditor("configeditor", "Config Editor", Community.Runtime.Core)
			{
				Entry = JObject.Parse(json),
				OnSave = onSave,
				OnSaveAndReload = onSaveAndReload,
				OnCancel = onCancel,
				Blacklist = blacklist,
				IsFullscreen = fullscreen
			};
			configEditor._draw();
			return configEditor;
		}

		internal void _draw()
		{
			AddColumn(0);
			List<OptionButton> list = Pool.Get<List<OptionButton>>();
			if (OnCancel != null)
			{
				list.Add(new OptionButton("Cancel", delegate(PlayerSession ap)
				{
					OnCancel?.Invoke(ap, Entry);
				}));
			}
			if (OnSave != null)
			{
				list.Add(new OptionButton("Save", delegate(PlayerSession ap)
				{
					OnSave?.Invoke(ap, Entry);
				}));
			}
			if (OnSaveAndReload != null)
			{
				list.Add(new OptionButton("Save & Reload", delegate(PlayerSession ap)
				{
					OnSaveAndReload?.Invoke(ap, Entry);
				}));
			}
			AddButtonArray(-1, list.ToArray());
			Pool.FreeUnmanaged<OptionButton>(ref list);
			createLegendMap(Entry);
			foreach (KeyValuePair<string, JToken> item in Entry)
			{
				if (item.Value is JObject)
				{
					string text = item.Key.Trim();
					if (LegendOptions.Count == 0 || !text.EndsWith("Legend") || !LegendOptions.ContainsKey(text))
					{
						AddName(0, item.Key ?? "", (TextAnchor)3);
					}
				}
				_recurseBuild(item.Key, item.Value, 0, 0);
			}
			void createLegendMap(JObject inner)
			{
				//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ee: Invalid comparison between Unknown and I4
				foreach (KeyValuePair<string, JToken> item2 in inner)
				{
					JToken value = item2.Value;
					JObject val = (JObject)(object)((value is JObject) ? value : null);
					if (val != null)
					{
						if (item2.Key.EndsWith("Legend"))
						{
							string key = item2.Key;
							int length = "Legend".Length;
							string text2 = key.Substring(0, key.Length - length).Trim();
							JContainer parent = ((JToken)val).Parent;
							object obj;
							if (parent == null)
							{
								obj = null;
							}
							else
							{
								JContainer parent2 = ((JToken)parent).Parent;
								obj = ((parent2 != null) ? ((JToken)parent2)[(object)text2] : null);
							}
							JToken val2 = (JToken)obj;
							if (val2 != null)
							{
								List<JProperty> list2 = Pool.Get<List<JProperty>>();
								list2.AddRange(val.Properties());
								LegendOptions[item2.Key] = list2.Select((JProperty p) => p.Name).ToArray();
								int num = 0;
								if ((int)val2.Type != 10)
								{
									string text3 = ((object)val2).ToString();
									foreach (JProperty item3 in list2)
									{
										if (((object)item3.Value).ToString() == text3)
										{
											break;
										}
										num++;
									}
								}
								if (num >= list2.Count)
								{
									num = 0;
								}
								LegendIndex[item2.Key] = num;
								Pool.FreeUnmanaged<JProperty>(ref list2);
								continue;
							}
						}
						createLegendMap(val);
					}
				}
			}
		}

		internal void _recurseBuild(string name, JToken token, int level, int column, bool removeButtons = false)
		{
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Expected I4, but got Unknown
			if (Blacklist != null && Enumerable.Contains(Blacklist, name))
			{
				return;
			}
			string trimName = name.Trim();
			if (LegendOptions.Count > 0 && LegendOptions.ContainsKey(trimName + "Legend"))
			{
				return;
			}
			JToken obj = token;
			JArray val = (JArray)(object)((obj is JArray) ? obj : null);
			if (val != null)
			{
				AddName(column, " ".SpacedString(level, trimEnd: false) + name, (TextAnchor)3);
				AddButton(column, "Edit", delegate(PlayerSession ap)
				{
					_drawArray(name, val, level, column, ap);
				}, null, (TextAnchor)4);
				return;
			}
			JToken obj2 = token;
			JProperty val2 = (JProperty)(object)((obj2 is JProperty) ? obj2 : null);
			JToken usableToken = ((val2 != null) ? val2.Value : token);
			JToken obj3 = usableToken;
			JTokenType? val3 = ((obj3 != null) ? new JTokenType?(obj3.Type) : ((JTokenType?)null));
			if (!val3.HasValue)
			{
				return;
			}
			JTokenType valueOrDefault = val3.GetValueOrDefault();
			switch (valueOrDefault - 1)
			{
			case 7:
			{
				string value2 = usableToken.ToObject<string>();
				string[] array2 = value2.Split(' ');
				if (value2.StartsWith("#") || (array2.Length >= 3 && array2.All((string x) => float.TryParse(x, out var _))))
				{
					AddColor(column, name, () => (!value2.StartsWith("#")) ? value2 : CUI.HexToRustColor(value2), delegate(PlayerSession ap, string hex, string rust, float alpha)
					{
						value2 = (value2.StartsWith("#") ? hex : rust);
						usableToken.Replace(usableToken = JToken.op_Implicit("#" + value2));
						Community.Runtime.Core.NextFrame(delegate
						{
							Singleton.SetTab(ap.Player, Make(((object)Entry).ToString(), OnCancel, OnSave, OnSaveAndReload), onChange: false);
						});
					}, "The color value of the '" + name.Trim() + "' property.");
				}
				else
				{
					AddInput(column, name, (PlayerSession ap) => usableToken.ToObject<string>(), delegate(PlayerSession ap, object[] args)
					{
						usableToken.Replace(usableToken = JToken.op_Implicit(args.Select((object x) => x as string).ToString(" ")));
					});
				}
				Array.Clear(array2, 0, array2.Length);
				array2 = null;
				break;
			}
			case 5:
				AddInput(column, name, delegate
				{
					JToken obj6 = usableToken;
					return (obj6 == null) ? null : obj6.ToObject<long>().ToString();
				}, delegate(PlayerSession ap, object[] args)
				{
					usableToken.Replace(usableToken = JToken.op_Implicit(args[0]?.ToString().ToLong(0L)));
				}, "The integer/long value of the '" + name.Trim() + "' property.");
				break;
			case 6:
				AddInput(column, name, delegate
				{
					JToken obj6 = usableToken;
					return (obj6 == null) ? null : obj6.ToObject<float>().ToString();
				}, delegate(PlayerSession ap, object[] args)
				{
					usableToken.Replace(usableToken = JToken.op_Implicit(args[0]?.ToString().ToFloat()));
				}, "The float value of the '" + name.Trim() + "' property.");
				break;
			case 8:
				AddToggle(column, name, delegate
				{
					usableToken.Replace(usableToken = JToken.op_Implicit(!usableToken.ToObject<bool>()));
				}, (PlayerSession ap) => usableToken.ToObject<bool>(), "The boolean value of the '" + name.Trim() + "' property.");
				break;
			case 1:
			{
				JToken obj5 = usableToken;
				JArray array3 = (JArray)(object)((obj5 is JArray) ? obj5 : null);
				AddName(column, " ".SpacedString(level, trimEnd: false) + name, (TextAnchor)3);
				AddButton(column, "Edit", delegate(PlayerSession ap)
				{
					_drawArray(name, array3, level, column, ap);
				}, null, (TextAnchor)4);
				break;
			}
			case 0:
			{
				if (LegendIndex.Count > 0 && LegendOptions.Count > 0 && LegendIndex.TryGetValue(trimName, out var _) && LegendOptions.TryGetValue(trimName, out var options))
				{
					JToken obj4 = usableToken;
					JObject tokenObject = (JObject)(object)((obj4 is JObject) ? obj4 : null);
					if (tokenObject != null)
					{
						string text = trimName;
						int length = "Legend".Length;
						string baseName = text.Substring(0, text.Length - length);
						AddDropdown(column, baseName, (PlayerSession ap) => Mathf.Clamp(LegendIndex[trimName], 0, options.Length - 1), delegate(PlayerSession ap, int i)
						{
							JContainer parent2 = ((JToken)tokenObject).Parent;
							object obj6;
							if (parent2 == null)
							{
								obj6 = null;
							}
							else
							{
								JContainer parent3 = ((JToken)parent2).Parent;
								obj6 = ((parent3 != null) ? ((JToken)parent3)[(object)baseName] : null);
							}
							JToken val4 = (JToken)obj6;
							if (val4 == null)
							{
								throw new InvalidOperationException($"Failed to find token for '{val4}', please validate configuration and try again.");
							}
							JToken value3 = tokenObject.Properties().ElementAt(i).Value;
							val4.Replace(value3);
							LegendIndex[trimName] = i;
						}, options, null, 0f, "The selected value of the '" + baseName + "' property.");
						break;
					}
				}
				int ulevel = level + 1;
				JContainer parent = token.Parent;
				JArray array = (JArray)(object)((parent is JArray) ? parent : null);
				if (array != null)
				{
					AddInputButton(column, null, 0.2f, new OptionInput(null, (PlayerSession ap) => $"{array.IndexOf(token)}", 0, readOnly: true, null), new OptionButton("Remove", (TextAnchor)4, delegate(PlayerSession ap)
					{
						array.Remove(token);
						ClearColumn(column);
						_drawArray(name, array, level, column, ap);
					}, (PlayerSession ap) => OptionButton.Types.Important));
				}
				DrawArray(name, token, ulevel);
				break;
			}
			case 2:
			case 3:
			case 4:
				break;
			}
			void DrawArray(string title, JToken tok, int num, bool editRefresh = false)
			{
				if (editRefresh)
				{
					ConfigEditor configEditor = this;
					int column2 = column;
					JContainer parent2 = tok.Parent;
					JContainer obj6 = ((parent2 is JProperty) ? parent2 : null);
					configEditor.AddName(column2, "Editing '" + ((obj6 != null) ? ((JProperty)obj6).Name : null) + "'", (TextAnchor)3);
				}
				foreach (JToken item in (IEnumerable<JToken>)tok)
				{
					if (item is JObject && !editRefresh)
					{
						ConfigEditor configEditor2 = this;
						int column3 = column;
						string text2 = " ".SpacedString(num, trimEnd: false);
						JContainer parent3 = item.Parent;
						JContainer obj7 = ((parent3 is JProperty) ? parent3 : null);
						configEditor2.AddName(column3, text2 + ((obj7 != null) ? ((JProperty)obj7).Name : null), (TextAnchor)3);
					}
					ConfigEditor configEditor3 = this;
					string text3 = " ".SpacedString(num + 1, trimEnd: false);
					JToken obj8 = ((item is JProperty) ? item : null);
					configEditor3._recurseBuild(text3 + ((obj8 != null) ? ((JProperty)obj8).Name : null), item, num + 1, column);
					if (removeButtons)
					{
						JProperty jproperty = (JProperty)(object)((item is JProperty) ? item : null);
						ConfigEditor configEditor4 = this;
						int column4 = column;
						JProperty obj9 = jproperty;
						configEditor4.AddButton(column4, "Remove '" + ((obj9 != null) ? obj9.Name.Trim() : null) + "'", delegate(PlayerSession ap2)
						{
							JToken obj10 = tok;
							((JObject)((obj10 is JObject) ? obj10 : null)).Remove(jproperty.Name);
							ConfigEditor configEditor5 = this;
							string name2 = name;
							JContainer parent4 = tok.Parent;
							configEditor5._drawArray(name2, (JArray)(object)((parent4 is JArray) ? parent4 : null), num, column, ap2);
						}, (PlayerSession ap2) => OptionButton.Types.Important, (TextAnchor)4);
					}
				}
			}
		}

		internal void _drawArray(string name, JArray array, int level, int column, PlayerSession ap)
		{
			int num = 0;
			int num2 = column + 1;
			ClearAfter(num2, erase: true);
			AddName(num2, "Editing '" + name.Trim() + "'", (TextAnchor)3);
			foreach (JToken element in array)
			{
				_recurseBuild(string.Format("{0}{1:n0}", " ".SpacedString(level, trimEnd: false), num), element, 0, num2, ((JContainer)array).Count == 1);
				AddButton(num2, "Remove", delegate
				{
					array.Remove(element);
					_drawArray(name, array, level, column, ap);
				}, (PlayerSession playerSession) => OptionButton.Types.Important, (TextAnchor)4);
				num++;
			}
			if (((JContainer)array).Count <= 1)
			{
				JToken? obj = ((IEnumerable<JToken>)array).FirstOrDefault();
				JObject sample = (JObject)(object)((obj is JObject) ? obj : null);
				string newPropertyName = ap.GetStorage(this, "jsonprop", "New Property");
				if (((JContainer)array).Count == 1)
				{
					AddButton(num2, "Duplicate", delegate
					{
						array.Add(((IEnumerable<JToken>)array).LastOrDefault());
						_drawArray(name, array, level, column, ap);
					}, (PlayerSession playerSession) => OptionButton.Types.Warned, (TextAnchor)4);
				}
				else if (((JContainer)array).Count == 0)
				{
					AddText(num2, " ".SpacedString(0, trimEnd: false) + "No entries", 10, "1 1 1 0.6", (TextAnchor)3);
				}
				AddInput(num2, "Property Name", (PlayerSession playerSession) => playerSession.GetStorage(this, "jsonprop", "New Property"), delegate(PlayerSession playerSession, object[] args)
				{
					playerSession.SetStorage(this, "jsonprop", newPropertyName = args.Select((object x) => x as string).ToString(" "));
				});
				AddButtonArray(num2, new OptionButton("Add Label", delegate(PlayerSession ap2)
				{
					if (sample == null)
					{
						array.Add((JToken)(object)(sample = JObject.Parse("{ }")));
					}
					if (!((IDictionary<string, JToken>)sample).ContainsKey(newPropertyName))
					{
						sample.Add(newPropertyName, JToken.op_Implicit(string.Empty));
						_drawArray(name, array, level, column, ap2);
					}
				}), new OptionButton("Add Toggle", delegate(PlayerSession ap2)
				{
					if (sample == null)
					{
						array.Add((JToken)(object)(sample = JObject.Parse("{ }")));
					}
					if (!((IDictionary<string, JToken>)sample).ContainsKey(newPropertyName))
					{
						sample.Add(newPropertyName, JToken.op_Implicit(false));
						_drawArray(name, array, level, column, ap2);
					}
				}), new OptionButton("Add Int", delegate(PlayerSession ap2)
				{
					if (sample == null)
					{
						array.Add((JToken)(object)(sample = JObject.Parse("{ }")));
					}
					if (!((IDictionary<string, JToken>)sample).ContainsKey(newPropertyName))
					{
						sample.Add(newPropertyName, JToken.op_Implicit(0));
						_drawArray(name, array, level, column, ap2);
					}
				}), new OptionButton("Add Float", delegate(PlayerSession ap2)
				{
					if (sample == null)
					{
						array.Add((JToken)(object)(sample = JObject.Parse("{ }")));
					}
					if (!((IDictionary<string, JToken>)sample).ContainsKey(newPropertyName))
					{
						sample.Add(newPropertyName, JToken.op_Implicit(0f));
						_drawArray(name, array, level, column, ap2);
					}
				}));
			}
			else
			{
				AddButton(num2, "Duplicate", delegate
				{
					array.Add(((IEnumerable<JToken>)array).LastOrDefault());
					_drawArray(name, array, level, column, ap);
				}, (PlayerSession playerSession) => OptionButton.Types.Selected, (TextAnchor)4);
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			Pool.FreeUnmanaged<string, int>(ref LegendIndex);
			Pool.FreeUnmanaged<string, string[]>(ref LegendOptions);
		}
	}

	public class ConfigurationTab : Tab
	{
		public enum ConfigTabs
		{
			ConVars,
			CarbonAuto,
			Items
		}

		private static ConfigurationTab _instance;

		private const float _applyChangesCooldown = 60f;

		private static TimeSince _applyChangesTimeSince = TimeSince.op_Implicit(60f);

		public static readonly string[] AuthLevels = new string[4] { "User", "Moderator", "Admin", "Developer" };

		public ConfigurationTab(string id, string name, RustPlugin plugin, Action<PlayerSession, Tab> onChange = null)
			: base(id, name, plugin, onChange)
		{
		}

		public static ConfigurationTab GetOrCache()
		{
			return _instance ?? (_instance = Make());
		}

		private static ConfigurationTab Make()
		{
			ConfigurationTab configurationTab = new ConfigurationTab("configuration", "Configuration", Community.Runtime.Core, delegate(PlayerSession session, Tab tab)
			{
				session.ClearStorage(null, "itemtabitem");
				Refresh(tab, session);
			})
			{
				IsFullscreen = true,
				Access = "config.use"
			};
			configurationTab.Over = delegate(Tab t, CUI cui, CuiElementContainer container, string panel, PlayerSession ap)
			{
				ItemDefinition storage = ap.GetStorage<ItemDefinition>(null, "itemtabitem");
				if ((Object)(object)storage != (Object)null)
				{
					CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, panel, "0.1 0.1 0.1 0.5", null, 0.5f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, blur: true);
					CUI.Pair<string, CuiElement, CuiElement> pair2 = cui.CreateProtectedButton(container, pair, "0.7 0.1 0.05 1", Cache.CUI.BlankColor, string.Empty, 0, null, 0.92f, 0.98f, 0.94f, 0.99f, 0f, 0f, 0f, 0f, "adminmodule.itemclear", (TextAnchor)4);
					cui.CreateImage(container, pair2, "close", "1 0.4 0.35 0.9", null, 0.2f, 0.8f, 0.2f, 0.8f);
					CUI.Pair<string, CuiElement> pair3 = cui.CreatePanel(container, pair, "0.1 0.1 0.1 0.5", null, 0.06f, 0.42f, 0.6f, 0.9f);
					cui.CreateItemImage(container, pair3, storage.itemid, 0uL, "1 1 1 1", null, 0.05f, 0.95f, 0.05f, 0.95f);
					cui.CreateText(container, pair, "0.8 0.2 0.15 1", ((object)Unsafe.As<ItemCategory, ItemCategory>(ref storage.category)/*cast due to constrained. prefix*/).ToString().ToUpper().SpacedString(1), 10, 0.45f, 1f, 0f, 0.88f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
					cui.CreateInputField(container, pair, "1 1 1 1", storage.displayName.english, 16, 0, readOnly: true, 0.45f, 1f, 0f, 0.8525f, 0f, 0f, 0f, 0f, null, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedBold, autoFocus: false, hudMenuInput: false, (LineType)0);
					cui.CreateText(container, pair, "1 1 1 0.4", "ID", 10, 0.45f, 1f, 0f, 0.81f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
					cui.CreateInputField(container, pair, "0.8 0.2 0.15 1", storage.itemid.ToString(), 10, 0, readOnly: true, 0.475f, 1f, 0f, 0.81f, 0f, 0f, 0f, 0f, null, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedBold, autoFocus: false, hudMenuInput: false, (LineType)0);
					cui.CreateText(container, pair, "1 1 1 0.4", "SN", 10, 0.63f, 1f, 0f, 0.81f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
					cui.CreateInputField(container, pair, "0.8 0.2 0.15 1", storage.shortname, 10, 0, readOnly: true, 0.667f, 1f, 0f, 0.81f, 0f, 0f, 0f, 0f, null, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedBold, autoFocus: false, hudMenuInput: false, (LineType)0);
					cui.CreateText(container, pair, "0.8 0.8 0.8 1", "DESCRIPTION", 11, 0.45f, 1f, 0f, 0.76f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
					cui.CreateInputField(container, pair, "0.8 0.8 0.8 0.6", storage.displayDescription.english, 11, 0, readOnly: true, 0.45f, 0.8f, 0f, 0.73f, 0f, 0f, 0f, 0f, null, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, autoFocus: false, hudMenuInput: false, (LineType)0);
					cui.CreatePanel(container, pair, "0.8 0.8 0.8 0.2", null, 0.06f, 0.94f, 0.54f, 0.55f);
					cui.CreateText(container, pair, "1 1 1 1", "CREATE", 16, 0.07f, 1f, 0f, 0.5f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
					cui.CreateText(container, pair, "1 1 1 0.5", "Generate an inventory item based on this definition.", 10, 0.07f, 1f, 0f, 0.46f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
					cui.CreateText(container, pair, "1 1 1 0.7", "CUSTOM NAME", 12, 0.07f, 1f, 0f, 0.4f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
					CUI.Pair<string, CuiElement> pair4 = cui.CreatePanel(container, pair, "0.1 0.1 0.1 0.95", null, 0.07f, 0.45f, 0.31f, 0.36f);
					cui.CreateProtectedInputField(container, pair4, "1 1 1 1", ap.GetStorage<string>(null, "itemscustomname"), 12, 0, readOnly: false, 0.05f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "adminmodule.itemsetting customname", (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, autoFocus: false, hudMenuInput: false, (LineType)0);
					int num = 0;
					int num2 = 0;
					cui.CreateText(container, pair, "1 1 1 0.7", "SKIN", 12, 0.07f, 1f, 0f, 0.4f, OxMax: num2 += 200, OxMin: num2, OyMin: 0f, OyMax: 0f, align: (TextAnchor)0, font: CUI.Handler.FontTypes.RobotoCondensedRegular, verticalOverflow: (VerticalWrapMode)1);
					string parent = pair;
					float oxMax = num2;
					CUI.Pair<string, CuiElement> pair5 = cui.CreatePanel(container, parent, "0.1 0.1 0.1 0.95", null, 0.07f, 0.45f, 0.31f, 0.36f, num2, oxMax);
					cui.CreateProtectedInputField(container, pair5, "1 1 1 1", ap.GetStorage(null, "itemsskin", 0uL).ToString(), 12, 0, readOnly: false, 0.05f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "adminmodule.itemsetting skin", (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, autoFocus: false, hudMenuInput: false, (LineType)0);
					cui.CreateText(container, pair, "1 1 1 0.7", "AMOUNT", 12, 0.07f, 1f, 0f, 0.4f, 0f, 0f, OyMax: num -= 60, OyMin: num, align: (TextAnchor)0, font: CUI.Handler.FontTypes.RobotoCondensedRegular, verticalOverflow: (VerticalWrapMode)1);
					string parent2 = pair;
					oxMax = num;
					CUI.Pair<string, CuiElement> pair6 = cui.CreatePanel(container, parent2, "0.1 0.1 0.1 0.95", null, 0.07f, 0.45f, 0.31f, 0.36f, 0f, 0f, num, oxMax);
					cui.CreateProtectedInputField(container, pair6, "1 1 1 1", ap.GetStorage(null, "itemsamount", 1).ToString(), 12, 0, readOnly: false, 0.05f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "adminmodule.itemsetting amount", (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, autoFocus: false, hudMenuInput: false, (LineType)0);
					string parent3 = pair;
					oxMax = num2;
					float oxMin = num2;
					float oxMax2 = oxMax;
					float oyMax = num;
					cui.CreateText(container, parent3, "1 1 1 0.7", "BLUEPRINT", 12, 0.07f, 1f, 0f, 0.4f, oxMin, oxMax2, num, oyMax, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
					string parent4 = pair;
					string empty = string.Empty;
					string blankColor = Cache.CUI.BlankColor;
					string text = empty;
					oyMax = num2;
					float oxMin2 = num2;
					float oxMax3 = oyMax;
					oxMax = num;
					CUI.Pair<string, CuiElement, CuiElement> pair7 = cui.CreateProtectedButton(container, parent4, "0.1 0.1 0.1 0.95", blankColor, text, 0, null, 0.07f, 0.125f, 0.31f, 0.36f, oxMin2, oxMax3, num, oxMax, "adminmodule.itemsetting blueprint", (TextAnchor)4);
					if (ap.GetStorage(null, "itemsblueprint", @default: false))
					{
						cui.CreateImage(container, pair7, "checkmark", "0.4 0.9 0.4 0.9", null, 0.15f, 0.85f, 0.15f, 0.85f);
					}
					cui.CreateText(container, pair, "1 1 1 0.7", "ITEM TEXT", 12, 0.07f, 1f, 0f, 0.4f, 0f, 0f, OyMax: num -= 60, OyMin: num, align: (TextAnchor)0, font: CUI.Handler.FontTypes.RobotoCondensedRegular, verticalOverflow: (VerticalWrapMode)1);
					string parent5 = pair;
					empty = string.Empty;
					string blankColor2 = Cache.CUI.BlankColor;
					string text2 = empty;
					oxMax = num;
					CUI.Pair<string, CuiElement, CuiElement> pair8 = cui.CreateProtectedButton(container, parent5, "0.1 0.1 0.1 0.95", blankColor2, text2, 0, null, 0.07f, 0.875f, 0.26f, 0.36f, 0f, 0f, num, oxMax, string.Empty, (TextAnchor)4);
					cui.CreateProtectedInputField(container, pair8, "1 1 1 1", ap.GetStorage(null, "itemstext", string.Empty), 12, 0, readOnly: false, 0.02f, 1f, 0f, 0.9f, 0f, 0f, 0f, 0f, "adminmodule.itemsetting text", (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, autoFocus: false, hudMenuInput: false, (LineType)2, 0f, 0f, needsCursor: false, needsKeyboard: true);
					cui.CreateProtectedButton(container, pair, "0.4 0.6 0.3 1", "0.8 1 0.7 1", "CREATE ITEM", 10, null, 0.07f, 0.25f, 0.1f, 0.15f, 290f, 290f, 200f, 200f, "adminmodule.itemcreate", (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedBold);
				}
			};
			return configurationTab;
			static void Refresh(Tab tab, PlayerSession session)
			{
				//IL_1902: Unknown result type (might be due to invalid IL or missing references)
				//IL_1907: Unknown result type (might be due to invalid IL or missing references)
				tab.ClearColumn(0);
				tab.AddButton(-1, "< Go Back", delegate
				{
					Singleton.SetTab(session.Player, "carbon");
				}, (PlayerSession ap) => OptionButton.Types.Selected, (TextAnchor)4);
				tab.AddName(0, "Configuration", (TextAnchor)3);
				tab.AddName(0, "Tabs", (TextAnchor)3);
				for (int num = 0; num < Singleton.Tabs.Count; num++)
				{
					Tab t = Singleton.Tabs[num];
					tab.AddToggle(0, t.Name, delegate
					{
						Singleton.DataInstance.MarkTabHidden(t.Id, !Singleton.DataInstance.IsTabHidden(t.Id));
					}, (PlayerSession ap) => !Singleton.DataInstance.IsTabHidden(t.Id));
				}
				tab.AddButton(0, "Apply Changes", delegate(PlayerSession ap)
				{
					//IL_0000: Unknown result type (might be due to invalid IL or missing references)
					//IL_0020: Unknown result type (might be due to invalid IL or missing references)
					//IL_0025: Unknown result type (might be due to invalid IL or missing references)
					if (TimeSince.op_Implicit(_applyChangesTimeSince) > 60f)
					{
						Singleton.GenerateTabs();
						_applyChangesTimeSince = TimeSince.op_Implicit(0f);
						Refresh(tab, session);
						Singleton.Draw(ap.Player);
						Singleton.Save();
					}
				}, (PlayerSession ap) => (TimeSince.op_Implicit(_applyChangesTimeSince) > 60f) ? OptionButton.Types.Selected : OptionButton.Types.None, (TextAnchor)4);
				tab.AddToggle(0, "Spectating Info Overlay", delegate
				{
					Singleton.ConfigInstance.SpectatingInfoOverlay = !Singleton.ConfigInstance.SpectatingInfoOverlay;
				}, (PlayerSession ap) => Singleton.ConfigInstance.SpectatingInfoOverlay);
				tab.AddToggle(0, "Spectating End Teleport Back", delegate
				{
					Singleton.ConfigInstance.SpectatingEndTeleportBack = !Singleton.ConfigInstance.SpectatingEndTeleportBack;
				}, (PlayerSession ap) => Singleton.ConfigInstance.SpectatingEndTeleportBack);
				tab.AddToggle(0, "Disable uMod (Plugins tab)", delegate
				{
					Singleton.DataInstance.DisableUMod = !Singleton.DataInstance.DisableUMod;
				}, (PlayerSession ap) => Singleton.DataInstance.DisableUMod);
				tab.AddToggle(0, "Hide Plugin Icons (Plugins tab)", delegate
				{
					Singleton.DataInstance.HidePluginIcons = !Singleton.DataInstance.HidePluginIcons;
				}, (PlayerSession ap) => Singleton.DataInstance.HidePluginIcons);
				tab.AddName(0, "Customization", (TextAnchor)3);
				tab.AddToggle(0, "Background Blur", delegate
				{
					Singleton.DataInstance.BackgroundBlur = !Singleton.DataInstance.BackgroundBlur;
				}, (PlayerSession ap) => Singleton.DataInstance.BackgroundBlur);
				tab.AddRange(0, "Background Opacity", 0f, 100f, (PlayerSession ap) => Singleton.DataInstance.BackgroundOpacity * 100f, delegate(PlayerSession ap, float value)
				{
					Singleton.DataInstance.BackgroundOpacity = value * 0.01f;
				}, (PlayerSession ap) => Singleton.DataInstance.BackgroundOpacity.ToString("0.0"));
				tab.AddInput(0, "Background Image", (PlayerSession ap) => Singleton.DataInstance.BackgroundImage, delegate(PlayerSession ap, object[] value)
				{
					Singleton.DataInstance.BackgroundImage = (string)value[0];
				});
				tab.AddRange(0, "Background Image Opacity", 0f, 100f, (PlayerSession ap) => Singleton.DataInstance.BackgroundImageOpacity * 100f, delegate(PlayerSession ap, float value)
				{
					Singleton.DataInstance.BackgroundImageOpacity = value * 0.01f;
				}, (PlayerSession ap) => Singleton.DataInstance.BackgroundImageOpacity.ToString("0.0"));
				tab.AddRange(0, "Background Column Opacity", 0f, 100f, (PlayerSession ap) => Singleton.DataInstance.BackgroundColumnOpacity * 100f, delegate(PlayerSession ap, float value)
				{
					Singleton.DataInstance.BackgroundColumnOpacity = value * 0.01f;
				}, (PlayerSession ap) => Singleton.DataInstance.BackgroundColumnOpacity.ToString("0.0"));
				tab.AddRange(0, "Title Underline Opacity", 0f, 100f, (PlayerSession ap) => Singleton.DataInstance.Colors.TitleUnderlineOpacity * 100f, delegate(PlayerSession ap, float value)
				{
					Singleton.DataInstance.Colors.TitleUnderlineOpacity = value * 0.01f;
					Singleton.Draw(ap.Player);
				}, (PlayerSession ap) => Singleton.DataInstance.Colors.TitleUnderlineOpacity.ToString("0.0"));
				tab.AddRange(0, "Option Width", 20f, 80f, (PlayerSession ap) => Singleton.DataInstance.Colors.OptionWidth * 100f, delegate(PlayerSession ap, float value)
				{
					Singleton.DataInstance.Colors.OptionWidth = value * 0.01f;
					Singleton.Draw(ap.Player);
				}, (PlayerSession ap) => Singleton.DataInstance.Colors.OptionWidth.ToString("0.0"));
				tab.AddColor(0, "Selected Tab Color", () => Singleton.DataInstance.Colors.SelectedTabColor, delegate(PlayerSession ap, string color1, string color2, float value)
				{
					Singleton.DataInstance.Colors.SelectedTabColor = CUI.HexToRustColor("#" + color1);
					Singleton.Draw(ap.Player);
				});
				tab.AddColor(0, "Editable Input Highlight", () => Singleton.DataInstance.Colors.EditableInputHighlight, delegate(PlayerSession ap, string color1, string color2, float value)
				{
					Singleton.DataInstance.Colors.EditableInputHighlight = CUI.HexToRustColor("#" + color1);
					Singleton.Draw(ap.Player);
				});
				tab.AddColor(0, "Name Text Color", () => Singleton.DataInstance.Colors.NameTextColor, delegate(PlayerSession ap, string color1, string color2, float value)
				{
					Singleton.DataInstance.Colors.NameTextColor = CUI.HexToRustColor("#" + color1, value);
					Singleton.Draw(ap.Player);
				});
				tab.AddColor(0, "Option Name Color", () => Singleton.DataInstance.Colors.OptionNameColor, delegate(PlayerSession ap, string color1, string color2, float value)
				{
					Singleton.DataInstance.Colors.OptionNameColor = CUI.HexToRustColor("#" + color1, value);
					Singleton.Draw(ap.Player);
				});
				tab.AddColor(0, "Button Selected Color", () => Singleton.DataInstance.Colors.ButtonSelectedColor, delegate(PlayerSession ap, string color1, string color2, float value)
				{
					Singleton.DataInstance.Colors.ButtonSelectedColor = CUI.HexToRustColor("#" + color1, value);
					Singleton.Draw(ap.Player);
				});
				tab.AddColor(0, "Button Warned Color", () => Singleton.DataInstance.Colors.ButtonWarnedColor, delegate(PlayerSession ap, string color1, string color2, float value)
				{
					Singleton.DataInstance.Colors.ButtonWarnedColor = CUI.HexToRustColor("#" + color1, value);
					Singleton.Draw(ap.Player);
				});
				tab.AddColor(0, "Button Important Color", () => Singleton.DataInstance.Colors.ButtonImportantColor, delegate(PlayerSession ap, string color1, string color2, float value)
				{
					Singleton.DataInstance.Colors.ButtonImportantColor = CUI.HexToRustColor("#" + color1, value);
					Singleton.Draw(ap.Player);
				});
				tab.AddColor(0, "Option Color (1st)", () => Singleton.DataInstance.Colors.OptionColor, delegate(PlayerSession ap, string color1, string color2, float value)
				{
					Singleton.DataInstance.Colors.OptionColor = CUI.HexToRustColor("#" + color1, value);
					Singleton.Draw(ap.Player);
				});
				tab.AddColor(0, "Option Color (2nd)", () => Singleton.DataInstance.Colors.OptionColor2, delegate(PlayerSession ap, string color1, string color2, float value)
				{
					Singleton.DataInstance.Colors.OptionColor2 = CUI.HexToRustColor("#" + color1, value);
					Singleton.Draw(ap.Player);
				});
				tab.ClearColumn(1);
				ConfigTabs configTab = session.GetStorage(tab, "configtab", ConfigTabs.ConVars);
				tab.AddButtonArray(-2, new OptionButton("ConVars", delegate(PlayerSession ap)
				{
					session.SetStorage(tab, "configtab", ConfigTabs.ConVars);
					Refresh(tab, ap);
				}, (PlayerSession ap) => (configTab == ConfigTabs.ConVars) ? OptionButton.Types.Selected : OptionButton.Types.None), new OptionButton("Carbon Auto", delegate(PlayerSession ap)
				{
					session.SetStorage(tab, "configtab", ConfigTabs.CarbonAuto);
					Refresh(tab, ap);
				}, (PlayerSession ap) => (configTab == ConfigTabs.CarbonAuto) ? OptionButton.Types.Selected : OptionButton.Types.None), new OptionButton("Items", delegate(PlayerSession ap)
				{
					session.SetStorage(tab, "configtab", ConfigTabs.Items);
					Refresh(tab, ap);
				}, (PlayerSession ap) => (configTab == ConfigTabs.Items) ? OptionButton.Types.Selected : OptionButton.Types.None));
				tab.AddName(1, configTab.ToString(), (TextAnchor)3);
				switch (configTab)
				{
				case ConfigTabs.ConVars:
				{
					string convarSearch = session.GetStorage(tab, "convarsearch", string.Empty);
					int num4 = ConVarSnapshots.Snapshots.Count((KeyValuePair<string, ConVarSnapshots.Snapshot> x) => string.IsNullOrEmpty(convarSearch) || x.Key.Contains(convarSearch));
					tab.AddText(1, "Changing the following values will not be stored anywhere. This page is simply for informational purposes.\nIf you want Rust to load up your changes, please add them in 'server/identity/cfg/server.cfg'.", 8, "1 1 1 0.5", (TextAnchor)4);
					if (string.IsNullOrEmpty(convarSearch))
					{
						tab.AddInput(1, $"Search ({num4:n0})", (PlayerSession ap) => convarSearch, 0, readOnly: false, delegate(PlayerSession ap, object[] args)
						{
							ap.SetStorage(tab, "convarsearch", args.Select((object x) => x as string).ToString(" "));
							Refresh(tab, ap);
						});
					}
					else
					{
						tab.AddInputButton(1, $"Search ({num4:n0})", 0.08f, new OptionInput(string.Empty, (PlayerSession ap) => convarSearch, 0, readOnly: false, delegate(PlayerSession ap, object[] args)
						{
							ap.SetStorage(tab, "convarsearch", args.Select((object x) => x as string).ToString(" "));
							Refresh(tab, ap);
						}), new OptionButton("X", delegate(PlayerSession ap)
						{
							ap.SetStorage(tab, "convarsearch", string.Empty);
							Refresh(tab, ap);
						}, (PlayerSession ap) => OptionButton.Types.Important));
					}
					Type[] exportedTypes = typeof(BasePlayer).Assembly.GetExportedTypes();
					Type[] array = exportedTypes;
					foreach (Type type in array)
					{
						Factory customAttribute = ((MemberInfo)type).GetCustomAttribute<Factory>();
						string factoryName = ((customAttribute == null) ? type.Name.ToLower() : customAttribute.Name);
						IEnumerable<FieldInfo> enumerable2 = from x in type.GetFields(BindingFlags.Static | BindingFlags.Public)
							where ((MemberInfo)x).GetCustomAttribute<ServerVar>() != null && (string.IsNullOrEmpty(convarSearch) || (factoryName + "." + x.Name).Contains(convarSearch))
							select x;
						if (enumerable2.Any())
						{
							tab.AddText(1, "<color=orange>></color> " + type.Name.ToCamelCase() + "— " + factoryName + ".*", 13, "1 1 1 0.4", (TextAnchor)6);
							foreach (FieldInfo field in enumerable2)
							{
								ServerVar customAttribute2 = ((MemberInfo)field).GetCustomAttribute<ServerVar>();
								string key = factoryName + "." + field.Name;
								ConVarSnapshots.Snapshot snapshot = ConVarSnapshots.Snapshots[key];
								if (field.FieldType == typeof(string))
								{
									tab.AddInput(1, field.Name, (PlayerSession ap) => field.GetValue(null)?.ToString(), 0, readOnly: false, delegate(PlayerSession ap, object[] args)
									{
										field.SetValue(null, args.Select((object x) => x as string).ToString(" "));
									}, ((ConsoleVar)customAttribute2).Help);
								}
								else if (field.FieldType == typeof(bool))
								{
									tab.AddToggle(1, $"{field.Name} (default: {snapshot.Value})", delegate
									{
										field.SetValue(null, !(bool)field.GetValue(null));
									}, (PlayerSession ap) => (bool)field.GetValue(null), ((ConsoleVar)customAttribute2).Help);
								}
								else if (field.FieldType == typeof(float))
								{
									tab.AddInputButton(1, field.Name, 0.2f, new OptionInput(string.Empty, (PlayerSession ap) => $"{field.GetValue(null)}", 0, readOnly: false, delegate(PlayerSession ap, object[] args)
									{
										field.SetValue(null, ((string)args[0]).ToFloat());
									}), new OptionButton($"<size=8>{snapshot.Value:n0}</size>", delegate
									{
										field.SetValue(null, snapshot.Value);
									}), ((ConsoleVar)customAttribute2).Help);
								}
								else if (field.FieldType == typeof(int))
								{
									tab.AddInputButton(1, field.Name, 0.2f, new OptionInput(string.Empty, (PlayerSession ap) => $"{field.GetValue(null)}", 0, readOnly: false, delegate(PlayerSession ap, object[] args)
									{
										field.SetValue(null, ((string)args[0]).ToInt());
									}), new OptionButton($"<size=8>{snapshot.Value:n0}</size>", delegate
									{
										field.SetValue(null, snapshot.Value);
									}), ((ConsoleVar)customAttribute2).Help);
								}
								else if (field.FieldType == typeof(long))
								{
									tab.AddInputButton(1, field.Name, 0.2f, new OptionInput(string.Empty, (PlayerSession ap) => $"{field.GetValue(null)}", 0, readOnly: false, delegate(PlayerSession ap, object[] args)
									{
										field.SetValue(null, ((string)args[0]).ToLong(0L));
									}), new OptionButton($"<size=8>{snapshot.Value:n0}</size>", delegate
									{
										field.SetValue(null, snapshot.Value);
									}), ((ConsoleVar)customAttribute2).Help);
								}
								else if (field.FieldType == typeof(ulong))
								{
									tab.AddInputButton(1, field.Name, 0.2f, new OptionInput(string.Empty, (PlayerSession ap) => $"{field.GetValue(null)}", 0, readOnly: false, delegate(PlayerSession ap, object[] args)
									{
										field.SetValue(null, ((string)args[0]).ToUlong(0uL));
									}), new OptionButton($"<size=8>{snapshot.Value:n0}</size>", delegate
									{
										field.SetValue(null, snapshot.Value);
									}), ((ConsoleVar)customAttribute2).Help);
								}
								else if (field.FieldType == typeof(uint))
								{
									tab.AddInputButton(1, field.Name, 0.2f, new OptionInput(string.Empty, (PlayerSession ap) => $"{field.GetValue(null)}", 0, readOnly: false, delegate(PlayerSession ap, object[] args)
									{
										field.SetValue(null, ((string)args[0]).ToUint());
									}), new OptionButton($"<size=8>{snapshot.Value:n0}</size>", delegate
									{
										field.SetValue(null, snapshot.Value);
									}), ((ConsoleVar)customAttribute2).Help);
								}
								else
								{
									tab.AddText(1, $"{field.Name} ({field.FieldType})", 10, "1 1 1 1", (TextAnchor)4);
								}
							}
						}
					}
					break;
				}
				case ConfigTabs.CarbonAuto:
				{
					string carbonAutoSearch = session.GetStorage(tab, "carbonautosearch", string.Empty);
					int num6 = CarbonAuto.AutoCache.Count((KeyValuePair<string, CarbonAuto.AutoVar> x) => string.IsNullOrEmpty(carbonAutoSearch) || x.Key.Contains(carbonAutoSearch));
					if (string.IsNullOrEmpty(carbonAutoSearch))
					{
						tab.AddInput(1, $"Search ({num6:n0})", (PlayerSession ap) => carbonAutoSearch, 0, readOnly: false, delegate(PlayerSession ap, object[] args)
						{
							ap.SetStorage(tab, "carbonautosearch", args.Select((object x) => x as string).ToString(" "));
							Refresh(tab, ap);
						});
					}
					else
					{
						tab.AddInputButton(1, $"Search ({num6:n0})", 0.08f, new OptionInput(string.Empty, (PlayerSession ap) => carbonAutoSearch, 0, readOnly: false, delegate(PlayerSession ap, object[] args)
						{
							ap.SetStorage(tab, "carbonautosearch", args.Select((object x) => x as string).ToString(" "));
							Refresh(tab, ap);
						}), new OptionButton("X", delegate(PlayerSession ap)
						{
							ap.SetStorage(tab, "carbonautosearch", string.Empty);
							Refresh(tab, ap);
						}, (PlayerSession ap) => OptionButton.Types.Important));
					}
					tab.AddWidget(1, 1, delegate(PlayerSession playerSession, CUI cui, CuiElementContainer container, string parent)
					{
						cui.CreateText(container, parent, "1 1 1 0.5", "All values with <b>(*)</b> indicate that they're a multiplier value \nrelative to Rust's native value the configuration is defined for.", 8, 0f, 0.48f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)5, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
						cui.CreateText(container, parent, "1 1 1 0.5", "<color=orange>Orange variables</color> indicate will enforce the server\nto modded once the value is not <b>-1</b>.", 8, 0.52f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
					});
					{
						foreach (KeyValuePair<string, CarbonAuto.AutoVar> cache in from x in CarbonAuto.AutoCache
							orderby x.Value.Variable.DisplayName
							where string.IsNullOrEmpty(carbonAutoSearch) || x.Key.Contains(carbonAutoSearch)
							select x)
						{
							Type varType = cache.Value.GetVarType();
							if (varType == typeof(string))
							{
								tab.AddInput(1, cache.Value.Variable.ForceModded ? ("<color=orange>" + cache.Value.Variable.DisplayName + "</color>") : cache.Value.Variable.DisplayName, (PlayerSession ap) => cache.Value.GetValue()?.ToString(), 0, readOnly: false, delegate(PlayerSession ap, object[] args)
								{
									cache.Value.SetValue(args.Select((object x) => x as string).ToString(" "));
								}, cache.Value.Variable.Help + " (" + cache.Key + ")");
							}
							else if (varType == typeof(bool))
							{
								tab.AddToggle(1, cache.Value.Variable.ForceModded ? ("<color=orange>" + cache.Value.Variable.DisplayName + "</color>") : cache.Value.Variable.DisplayName, delegate
								{
									cache.Value.SetValue(!(bool)cache.Value.GetValue());
								}, (PlayerSession ap) => (bool)cache.Value.GetValue(), cache.Value.Variable.Help + " (" + cache.Key + ")");
							}
							else if (varType == typeof(float))
							{
								tab.AddInputButton(1, cache.Value.Variable.ForceModded ? ("<color=orange>" + cache.Value.Variable.DisplayName + "</color>") : cache.Value.Variable.DisplayName, 0.2f, new OptionInput(string.Empty, (PlayerSession ap) => $"{cache.Value.GetValue()}", 0, readOnly: false, delegate(PlayerSession ap, object[] args)
								{
									cache.Value.SetValue(((string)args[0]).ToFloat());
								}), new OptionButton("<size=8>-1</size>", delegate
								{
									cache.Value.SetValue(-1);
								}), cache.Value.Variable.Help + " (" + cache.Key + ")");
							}
							else if (varType == typeof(int))
							{
								tab.AddInputButton(1, cache.Value.Variable.ForceModded ? ("<color=orange>" + cache.Value.Variable.DisplayName + "</color>") : cache.Value.Variable.DisplayName, 0.2f, new OptionInput(string.Empty, (PlayerSession ap) => $"{cache.Value.GetValue()}", 0, readOnly: false, delegate(PlayerSession ap, object[] args)
								{
									cache.Value.SetValue(((string)args[0]).ToInt());
								}), new OptionButton("<size=8>-1</size>", delegate
								{
									cache.Value.SetValue(-1);
								}), cache.Value.Variable.Help + " (" + cache.Key + ")");
							}
							else if (varType == typeof(long))
							{
								tab.AddInputButton(1, cache.Value.Variable.ForceModded ? ("<color=orange>" + cache.Value.Variable.DisplayName + "</color>") : cache.Value.Variable.DisplayName, 0.2f, new OptionInput(string.Empty, (PlayerSession ap) => $"{cache.Value.GetValue()}", 0, readOnly: false, delegate(PlayerSession ap, object[] args)
								{
									cache.Value.SetValue(((string)args[0]).ToLong(0L));
								}), new OptionButton("<size=8>-1</size>", delegate
								{
									cache.Value.SetValue(-1);
								}), cache.Value.Variable.Help + " (" + cache.Key + ")");
							}
							else
							{
								tab.AddText(1, string.Format("{0} ({1})", cache.Value.Variable.ForceModded ? ("<color=orange>" + cache.Value.Variable.DisplayName + "</color>") : cache.Value.Variable.DisplayName, varType), 10, "1 1 1 1", (TextAnchor)4);
							}
						}
						break;
					}
				}
				case ConfigTabs.Items:
				{
					string itemSearch = session.GetStorage(tab, "itemsearch", string.Empty);
					IEnumerable<ItemDefinition> source = ItemManager.itemList.Where((ItemDefinition x) => string.IsNullOrEmpty(itemSearch) || StringEx.Contains(x.displayName.english, itemSearch, CompareOptions.IgnoreCase) || StringEx.Contains(x.shortname, itemSearch, CompareOptions.IgnoreCase) || x.itemid.ToString().Contains(itemSearch));
					int num2 = source.Count();
					if (string.IsNullOrEmpty(itemSearch))
					{
						tab.AddInput(1, $"Search ({num2:n0})", (PlayerSession ap) => itemSearch, 0, readOnly: false, delegate(PlayerSession ap, object[] args)
						{
							ap.SetStorage(tab, "itemsearch", args.Select((object x) => x as string).ToString(" "));
							Refresh(tab, ap);
						});
					}
					else
					{
						tab.AddInputButton(1, $"Search ({num2:n0})", 0.08f, new OptionInput(string.Empty, (PlayerSession ap) => itemSearch, 0, readOnly: false, delegate(PlayerSession ap, object[] args)
						{
							ap.SetStorage(tab, "itemsearch", args.Select((object x) => x as string).ToString(" "));
							Refresh(tab, ap);
						}), new OptionButton("X", delegate(PlayerSession ap)
						{
							ap.SetStorage(tab, "itemsearch", string.Empty);
							Refresh(tab, ap);
						}, (PlayerSession ap) => OptionButton.Types.Important));
					}
					string[] names = Enum.GetNames(typeof(ItemCategory));
					foreach (string text in names)
					{
						ItemCategory parsedCategory = (ItemCategory)Enum.Parse(typeof(ItemCategory), text);
						IEnumerable<ItemDefinition> enumerable = source.Where((ItemDefinition x) => x.category == parsedCategory);
						if (enumerable.Any())
						{
							tab.AddName(1, "<color=orange>></color> " + text, (TextAnchor)3);
							foreach (ItemDefinition item in enumerable)
							{
								tab.AddButton(1, item.displayName.english + "  (" + item.shortname + ")", delegate(PlayerSession ap)
								{
									ap.SetStorage<ItemDefinition>(null, "itemtabitem", item);
									Singleton.Draw(ap.Player);
								}, null, (TextAnchor)4);
							}
						}
					}
					break;
				}
				}
			}
		}
	}

	public class EntitiesTab
	{
		internal static int EntityCount = 0;

		internal static RustPlugin Core = Community.Runtime.Core;

		internal static AdminModule Admin = BaseModule.GetModule<AdminModule>();

		internal static PlayerSession LastContainerLooter;

		internal static string[] BuildingGrades = new string[5] { "Twig", "Wood", "Stone", "Metal", "Top Tier" };

		internal const string MultiselectionReplacement = "-";

		public static Tab Get()
		{
			Tab tab = new Tab("entities", "Entities", Community.Runtime.Core, delegate(PlayerSession ap, Tab tab2)
			{
				tab2.ClearColumn(1);
				ResetSelection(tab2, ap);
				DrawEntities(tab2, ap);
			}, "entities.use");
			tab.AddColumn(0);
			tab.AddColumn(1);
			return tab;
		}

		internal static void SelectEntity(Tab tab, PlayerSession session, BaseEntity entity)
		{
			List<BaseEntity> list = null;
			list = (session.HasStorage(tab, "selectedentities") ? session.GetStorage<List<BaseEntity>>(tab, "selectedentities") : session.SetStorage(tab, "selectedentities", new List<BaseEntity>()));
			if (!session.GetStorage(tab, "multi", @default: false))
			{
				list.Clear();
			}
			if (!list.Contains(entity))
			{
				list.Add(entity);
			}
		}

		internal static void ResetSelection(Tab tab, PlayerSession session)
		{
			List<BaseEntity> list = null;
			if (!session.HasStorage(tab, "selectedentities"))
			{
				list = session.SetStorage(tab, "selectedentities", new List<BaseEntity>());
				return;
			}
			list = session.GetStorage<List<BaseEntity>>(tab, "selectedentities");
			list.Clear();
		}

		internal static void DrawEntities(Tab tab, PlayerSession ap3)
		{
			tab.ClearColumn(0);
			tab.AddName(0, "Entities", (TextAnchor)3);
			List<BaseEntity> selectedEntitites = ap3.GetStorage<List<BaseEntity>>(tab, "selectedentities");
			if (!ap3.HasStorage(tab, "selectedentities"))
			{
				selectedEntitites = ap3.SetStorage(tab, "selectedentities", new List<BaseEntity>());
			}
			tab.AddInputButton(0, "Search Entity", 0.3f, new Tab.OptionInput(null, (PlayerSession playerSession) => playerSession.GetStorage(tab, "filter", string.Empty), 0, readOnly: false, delegate(PlayerSession playerSession, object[] args)
			{
				playerSession.SetStorage(tab, "filter", args.Select((object x) => x as string).ToString(" "));
				DrawEntities(tab, playerSession);
			}), new Tab.OptionButton("Refresh", delegate(PlayerSession ap4)
			{
				DrawEntities(tab, ap4);
			}));
			bool isMulti = ap3.GetStorage(tab, "multi", @default: false);
			tab.AddToggle(0, "Multi-selection", delegate(PlayerSession playerSession)
			{
				isMulti = playerSession.SetStorage(tab, "multi", !isMulti);
				selectedEntitites.Clear();
				tab.ClearColumn(1);
				DrawEntities(tab, ap3);
			}, (PlayerSession playerSession) => isMulti);
			EntityCount = 0;
			string usedFilter = ap3.GetStorage(tab, "filter", string.Empty)?.ToLower()?.Trim();
			Func<BaseEntity, bool> validateFilter = ap3.GetStorage<Func<BaseEntity, bool>>(tab, "validatefilter");
			int size = (int)World.Size;
			int range = ap3.GetStorage(tab, "range", size);
			IEnumerable<BaseEntity> enumerable = ((IEnumerable)BaseNetworkable.serverEntities).OfType<BaseEntity>().Where(delegate(BaseEntity val2)
			{
				//IL_005d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0068: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)val2 == (Object)null || (Object)(object)((Component)val2).transform == (Object)null)
				{
					return false;
				}
				if (validateFilter != null && !validateFilter(val2))
				{
					return false;
				}
				if (range != -1 && (Object)(object)ap3.Player != (Object)null && Vector3.Distance(((Component)ap3.Player).transform.position, ((Component)val2).transform.position) > (float)range)
				{
					return false;
				}
				return StringEx.Contains(((Object)val2).name, usedFilter, CompareOptions.OrdinalIgnoreCase) || StringEx.Contains(((object)val2).GetType().Name, usedFilter, CompareOptions.OrdinalIgnoreCase) || val2.OwnerID.ToString().Equals(usedFilter, StringComparison.OrdinalIgnoreCase) || val2.skinID.ToString().Equals(usedFilter, StringComparison.OrdinalIgnoreCase);
			});
			EntityCount = ((!string.IsNullOrEmpty(usedFilter)) ? enumerable.Count() : 0);
			tab.AddRange(0, "Range", 0f, size, (PlayerSession playerSession) => range, delegate(PlayerSession playerSession, float value)
			{
				try
				{
					playerSession.SetStorage(tab, "range", (int)value);
					DrawEntities(tab, playerSession);
				}
				catch (Exception ex)
				{
					Logger.Error("Oof", ex);
				}
			}, (PlayerSession playerSession) => $"{range:0.0}m");
			tab.AddName(0, $"Entities  ({EntityCount:n0})", (TextAnchor)3);
			string filter = ap3.GetStorage(tab, "filter", string.Empty);
			tab.AddButtonArray(0, new Tab.OptionButton("Players", delegate(PlayerSession playerSession)
			{
				playerSession.SetStorage(tab, "filter", "BasePlayer");
				DrawEntities(tab, playerSession);
			}, (PlayerSession _) => (filter == "BasePlayer") ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None), new Tab.OptionButton("Containers", delegate(PlayerSession playerSession)
			{
				playerSession.SetStorage(tab, "filter", "StorageContainer");
				playerSession.ClearStorage(tab, "validatefilter");
				DrawEntities(tab, playerSession);
			}, (PlayerSession _) => (filter == "StorageContainer") ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None), new Tab.OptionButton("Deployables", delegate(PlayerSession playerSession)
			{
				playerSession.SetStorage(tab, "filter", "Deployable");
				playerSession.ClearStorage(tab, "validatefilter");
				DrawEntities(tab, playerSession);
			}, (PlayerSession _) => (filter == "Deployable") ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None), new Tab.OptionButton("Collectibles", delegate(PlayerSession playerSession)
			{
				playerSession.SetStorage(tab, "filter", "CollectibleEntity");
				playerSession.ClearStorage(tab, "validatefilter");
				DrawEntities(tab, playerSession);
			}, (PlayerSession _) => (filter == "CollectibleEntity") ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None), new Tab.OptionButton("NPCs", delegate(PlayerSession playerSession)
			{
				playerSession.SetStorage(tab, "filter", "NPCPlayer");
				playerSession.ClearStorage(tab, "validatefilter");
				DrawEntities(tab, playerSession);
			}, (PlayerSession _) => (filter == "NPCPlayer") ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None), new Tab.OptionButton("I/O", delegate(PlayerSession playerSession)
			{
				playerSession.SetStorage(tab, "filter", "IOEntity");
				playerSession.ClearStorage(tab, "validatefilter");
				DrawEntities(tab, playerSession);
			}, (PlayerSession _) => (filter == "IOEntity") ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None));
			string storage = ap3.GetStorage(tab, "filter", string.Empty);
			if (storage == "BasePlayer")
			{
				tab.AddButtonArray(0, new Tab.OptionButton("Online", delegate(PlayerSession playerSession)
				{
					playerSession.SetStorage(tab, "filter", "BasePlayer");
					playerSession.SetStorage<Func<BaseEntity, bool>>(tab, "validatefilter", delegate(BaseEntity val3)
					{
						BasePlayer val2 = (BasePlayer)(object)((val3 is BasePlayer) ? val3 : null);
						return val2 != null && val2.IsConnected;
					});
					DrawEntities(tab, playerSession);
				}), new Tab.OptionButton("Offline", delegate(PlayerSession playerSession)
				{
					playerSession.SetStorage<Func<BaseEntity, bool>>(tab, "validatefilter", delegate(BaseEntity val3)
					{
						BasePlayer val2 = (BasePlayer)(object)((val3 is BasePlayer) ? val3 : null);
						return val2 != null && !val2.IsConnected;
					});
					DrawEntities(tab, playerSession);
				}), new Tab.OptionButton("Dead", delegate(PlayerSession playerSession)
				{
					playerSession.SetStorage<Func<BaseEntity, bool>>(tab, "validatefilter", delegate(BaseEntity val3)
					{
						BasePlayer val2 = (BasePlayer)(object)((val3 is BasePlayer) ? val3 : null);
						return val2 != null && ((BaseCombatEntity)val2).IsDead();
					});
					DrawEntities(tab, playerSession);
				}));
			}
			if (!string.IsNullOrEmpty(usedFilter))
			{
				foreach (BaseEntity entity in enumerable)
				{
					BaseEntity obj = entity;
					BasePlayer val = (BasePlayer)(object)((obj is BasePlayer) ? obj : null);
					string text = ((val == null) ? ((object)entity).ToString() : val.displayName);
					string name = text;
					tab.AddButton(0, name, delegate(PlayerSession playerSession)
					{
						if (selectedEntitites.Contains(entity))
						{
							selectedEntitites.Remove(entity);
							tab.ClearColumn(1);
						}
						else
						{
							SelectEntity(tab, playerSession, entity);
						}
						DrawEntitySettings(tab, 1, playerSession);
					}, (PlayerSession playerSession) => selectedEntitites.Contains(entity) ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None, (TextAnchor)4);
				}
			}
			if (EntityCount == 0)
			{
				tab.AddText(0, "No entities found with that filter", 9, "1 1 1 0.2", (TextAnchor)4);
			}
		}

		internal static void DrawEntitySettings(Tab tab, int column = 1, PlayerSession ap3 = null)
		{
			//IL_07a5: Unknown result type (might be due to invalid IL or missing references)
			List<BaseEntity> selectedEntitites = ap3.GetStorage<List<BaseEntity>>(tab, "selectedentities");
			tab.ClearColumn(column);
			if (selectedEntitites.Count == 0)
			{
				return;
			}
			BaseEntity entity = selectedEntitites[0];
			bool multiSelection = selectedEntitites.Count > 1;
			bool flag = selectedEntitites.All((BaseEntity x) => (Object)(object)x != (Object)null && (Object)(object)entity != (Object)null && ((object)x).GetType() == ((object)entity).GetType());
			tab.AddName(column, "Hierarchy", (TextAnchor)3);
			if (column != 1)
			{
				tab.AddButton(column, "<", delegate(PlayerSession ap4)
				{
					DrawEntities(tab, ap4);
					DrawEntitySettings(tab, 1, ap4);
				}, (PlayerSession playerSession) => Tab.OptionButton.Types.Warned, (TextAnchor)4);
			}
			if ((Object)(object)entity != (Object)null && !((BaseNetworkable)entity).IsDestroyed)
			{
				BaseEntity obj = entity;
				BasePlayer player = (BasePlayer)(object)((obj is BasePlayer) ? obj : null);
				BasePlayer owner = BasePlayer.FindByID(entity.OwnerID);
				if ((Object)(object)player != (Object)(object)ap3?.Player && Singleton.HasAccess(ap3.Player, "entities.kill_entity"))
				{
					tab.AddButtonArray(column, new Tab.OptionButton("Kill", delegate
					{
						tab.CreateDialog("Are you sure about that?", delegate(PlayerSession ap4)
						{
							DoAll<BaseEntity>(delegate(BaseEntity e)
							{
								((BaseNetworkable)e).Kill((DestroyMode)0, true);
							});
							List<BaseEntity> list2 = ap3.GetStorage<List<BaseEntity>>(tab, "selectedentities");
							if (!ap3.HasStorage(tab, "selectedentities"))
							{
								list2 = ap3.SetStorage(tab, "selectedentities", new List<BaseEntity>());
							}
							list2.Clear();
							DrawEntities(tab, ap4);
							tab.ClearColumn(column);
						});
					}, (PlayerSession playerSession) => Tab.OptionButton.Types.Important), new Tab.OptionButton("Kill (Gibbed)", delegate
					{
						tab.CreateDialog("Are you sure about that?", delegate(PlayerSession ap4)
						{
							DoAll<BaseEntity>(delegate(BaseEntity e)
							{
								((BaseNetworkable)e).Kill((DestroyMode)1, true);
							});
							List<BaseEntity> list2 = ap3.GetStorage<List<BaseEntity>>(tab, "selectedentities");
							if (!ap3.HasStorage(tab, "selectedentities"))
							{
								list2 = ap3.SetStorage(tab, "selectedentities", new List<BaseEntity>());
							}
							list2.Clear();
							DrawEntities(tab, ap4);
							tab.ClearColumn(column);
						});
					}));
				}
				tab.AddInput(column, "Id", (PlayerSession playerSession) => (!multiSelection) ? $"{((BaseNetworkable)entity).net.ID} [<b>{((object)entity).GetType().FullName}</b>]" : "-");
				tab.AddInput(column, "Name", delegate
				{
					object obj8;
					if (!multiSelection)
					{
						obj8 = ((BaseNetworkable)entity).ShortPrefabName;
						if (obj8 == null)
						{
							return "";
						}
					}
					else
					{
						obj8 = "-";
					}
					return (string)obj8;
				});
				if (!multiSelection)
				{
					tab.AddInputButton(column, "Owner", 0.3f, new Tab.OptionInput(null, (PlayerSession playerSession) => $"{entity.OwnerID}", 0, !Singleton.HasAccess(ap3.Player, "entities.owner_change"), delegate(PlayerSession ap4, object[] args)
					{
						ulong id = ((string)args[0]).ToUlong(0uL);
						DoAll<BaseEntity>(delegate(BaseEntity e)
						{
							e.OwnerID = id;
						});
						DrawEntities(tab, ap4);
						DrawEntitySettings(tab, 1, ap4);
					}), new Tab.OptionButton("Select", delegate(PlayerSession playerSession)
					{
						if (!((Object)(object)owner == (Object)null))
						{
							SelectEntity(tab, playerSession, (BaseEntity)(object)owner);
							DrawEntities(tab, playerSession);
							DrawEntitySettings(tab, 1, playerSession);
						}
					}, (PlayerSession playerSession) => (!((Object)(object)owner == (Object)null)) ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None));
				}
				tab.AddInput(column, "Prefab", delegate
				{
					object obj8;
					if (!multiSelection)
					{
						obj8 = ((BaseNetworkable)entity).PrefabName;
						if (obj8 == null)
						{
							return "";
						}
					}
					else
					{
						obj8 = "-";
					}
					return (string)obj8;
				});
				tab.AddInput(column, "Flags", (PlayerSession playerSession) => (!multiSelection) ? (((int)entity.flags != 0) ? $"{entity.flags}" : "None") : "-");
				tab.AddInput(column, "Skin", (PlayerSession playerSession) => (!multiSelection) ? entity.skinID.ToString() : "-", delegate(PlayerSession session, object[] args)
				{
					entity.skinID = ((string)args[0]).ToUlong(0uL);
					((BaseNetworkable)entity).SendNetworkUpdate((NetworkQueue)0);
				});
				tab.AddButton(column, "Edit Flags", delegate(PlayerSession playerSession)
				{
					DrawEntitySettings(tab, 0, playerSession);
					DrawEntityFlags(tab, playerSession);
				}, null, (TextAnchor)4);
				tab.AddInput(column, "Position", (PlayerSession playerSession) => (!multiSelection) ? $"{((Component)entity).transform.position} [{MapHelper.PositionToString(((Component)entity).transform.position)}]" : "-");
				tab.AddInput(column, "Rotation", delegate
				{
					//IL_0013: Unknown result type (might be due to invalid IL or missing references)
					//IL_0018: Unknown result type (might be due to invalid IL or missing references)
					//IL_001b: Unknown result type (might be due to invalid IL or missing references)
					if (!multiSelection)
					{
						Quaternion serverRotation = entity.ServerRotation;
						return $"{((Quaternion)(ref serverRotation)).eulerAngles}";
					}
					return "-";
				});
				if (!flag)
				{
					return;
				}
				if (!multiSelection && Singleton.HasAccess(ap3.Player, "entities.tp_entity"))
				{
					tab.AddButtonArray(column, new Tab.OptionButton("TeleportTo", delegate(PlayerSession playerSession)
					{
						//IL_0011: Unknown result type (might be due to invalid IL or missing references)
						playerSession.Player.Teleport(((Component)entity).transform.position);
					}), new Tab.OptionButton("Teleport2Me", delegate
					{
						tab.CreateDialog("Are you sure about that?", delegate(PlayerSession playerSession2)
						{
							//IL_003c: Unknown result type (might be due to invalid IL or missing references)
							//IL_001b: Unknown result type (might be due to invalid IL or missing references)
							BaseEntity obj8 = entity;
							BasePlayer val5 = (BasePlayer)(object)((obj8 is BasePlayer) ? obj8 : null);
							if (val5 != null)
							{
								val5.Teleport(((Component)playerSession2.Player).transform.position);
							}
							else
							{
								((Component)entity).transform.position = ((Component)playerSession2.Player).transform.position;
								((BaseNetworkable)entity).SendNetworkUpdate_Position();
							}
						});
					}), new Tab.OptionButton("Teleport2OwnedItem", delegate(PlayerSession playerSession)
					{
						//IL_0006: Unknown result type (might be due to invalid IL or missing references)
						//IL_0037: Unknown result type (might be due to invalid IL or missing references)
						BaseEntity[] array = Util.FindTargetsOwnedBy(EncryptedValue<ulong>.op_Implicit(player.userID), string.Empty);
						if (array.Length != 0)
						{
							BaseEntity val5 = array[RandomEx.GetRandomInteger(0, array.Length)];
							playerSession.Player.Teleport(((Component)val5).transform.position);
						}
						else
						{
							Logger.Warn($" No entities owned by {player} could be found to teleport to.");
						}
					}));
				}
				BaseEntity obj2 = entity;
				StorageContainer storage = (StorageContainer)(object)((obj2 is StorageContainer) ? obj2 : null);
				if (storage != null && !multiSelection && Singleton.HasAccess(ap3.Player, "entities.loot_entity"))
				{
					tab.AddButton(column, "Loot Container", delegate(PlayerSession playerSession)
					{
						LastContainerLooter = playerSession;
						playerSession.SetStorage<BaseEntity>(tab, "lootedent", entity);
						Core.timer.In(0.2f, delegate
						{
							Admin.Close(playerSession.Player);
						});
						Core.timer.In(0.5f, delegate
						{
							//IL_0113: Unknown result type (might be due to invalid IL or missing references)
							SendEntityToPlayer(playerSession.Player, entity);
							playerSession.Player.inventory.loot.Clear();
							playerSession.Player.inventory.loot.PositionChecks = false;
							playerSession.Player.inventory.loot.entitySource = (BaseEntity)(object)storage;
							playerSession.Player.inventory.loot.itemSource = null;
							playerSession.Player.inventory.loot.AddContainer(storage.inventory);
							playerSession.Player.inventory.loot.MarkDirty();
							playerSession.Player.inventory.loot.SendImmediate();
							((BaseEntity)playerSession.Player).ClientRPC(RpcTarget.Player("RPC_OpenLootPanel", playerSession.Player), storage.panelName);
						});
					}, null, (TextAnchor)4);
					tab.AddText(1, "To loot a backpack, drag the backpack item over any hotbar slots while looting an entity", 10, "1 1 1 0.4", (TextAnchor)4);
				}
				if (entity is BasePlayer)
				{
					tab.AddInput(column, "Display Name", (PlayerSession playerSession) => (!multiSelection) ? player.displayName : "-");
					tab.AddInput(column, "Steam ID", (PlayerSession playerSession) => (!multiSelection) ? player.UserIDString : "-");
					if (Singleton.HasAccess(ap3.Player, "players.see_ips"))
					{
						tab.AddInput(column, "IP", delegate
						{
							object obj8;
							if (!multiSelection)
							{
								Networkable net = ((BaseNetworkable)player).net;
								if (net == null)
								{
									obj8 = null;
								}
								else
								{
									Connection connection = net.connection;
									obj8 = ((connection != null) ? connection.ipaddress : null);
								}
								if (obj8 == null)
								{
									return "";
								}
							}
							else
							{
								obj8 = "-";
							}
							return (string)obj8;
						}, null, null, hidden: true);
					}
					if (!multiSelection && (ap3.Player.IsAdmin || Singleton.Permissions.UserHasPermission(ap3?.Player.UserIDString, "carbon.cmod") || player.userID.IsSteamId()))
					{
						tab.AddButtonArray(1, new Tab.OptionButton("Kick", delegate(PlayerSession playerSession)
						{
							Singleton.Modal.Open(playerSession.Player, "Kick " + player.displayName, new Dictionary<string, ModalModule.Modal.Field> { ["reason"] = ModalModule.Modal.Field.Make("Reason", ModalModule.Modal.Field.FieldTypes.String, required: false, "Stop doing that.") }, delegate(BasePlayer p, ModalModule.Modal m)
							{
								player.Kick(m.Get<string>("reason"), true);
							});
						}), new Tab.OptionButton("Ban", delegate(PlayerSession playerSession)
						{
							Singleton.Modal.Open(playerSession.Player, "Ban " + player.displayName, new Dictionary<string, ModalModule.Modal.Field>
							{
								["reason"] = ModalModule.Modal.Field.Make("Reason", ModalModule.Modal.Field.FieldTypes.String, required: false, "Stop doing that."),
								["until"] = ModalModule.Modal.ButtonField.MakeButton("Until", "Select Date", delegate
								{
									Core.NextTick(delegate
									{
										Singleton.DatePicker.Draw(playerSession.Player, delegate(DateTime date)
										{
											playerSession.SetStorage(tab, "date", date);
										});
									});
								})
							}, delegate(BasePlayer p, ModalModule.Modal m)
							{
								DateTime storage2 = playerSession.GetStorage(tab, "date", DateTime.UtcNow.AddYears(100));
								DateTime utcNow = DateTime.UtcNow;
								storage2 = new DateTime(storage2.Year, storage2.Month, storage2.Day, utcNow.Hour, utcNow.Minute, utcNow.Second, DateTimeKind.Utc);
								if (utcNow <= storage2)
								{
									storage2 = DateTime.UtcNow.AddYears(100);
								}
								TimeSpan duration = utcNow - storage2;
								player.AsIPlayer().Ban(m.Get<string>("reason"), duration);
							});
						}), new Tab.OptionButton(player.IsSleeping() ? "End Sleep" : "Sleep", delegate(PlayerSession ap4)
						{
							if (player.IsSleeping())
							{
								player.EndSleeping();
							}
							else
							{
								player.StartSleeping();
							}
							DrawEntitySettings(tab, 1, ap4);
						}), new Tab.OptionButton("Hostility", delegate(PlayerSession playerSession)
						{
							Dictionary<string, ModalModule.Modal.Field> fields = new Dictionary<string, ModalModule.Modal.Field> { ["duration"] = ModalModule.Modal.Field.Make("Duration", ModalModule.Modal.Field.FieldTypes.Float, required: true, 60f) };
							Singleton.Modal.Open(playerSession.Player, "Player Hostile", fields, delegate(BasePlayer val5, ModalModule.Modal modal)
							{
								//IL_0063: Unknown result type (might be due to invalid IL or missing references)
								float num = modal.Get<float>("duration").Clamp(0f, float.MaxValue);
								player.State.unHostileTimestamp = TimeEx.currentTimestamp + (double)num;
								player.DirtyPlayerState();
								((BaseEntity)player).ClientRPC(RpcTarget.Player("SetHostileLength", player), num);
								fields.Clear();
								fields = null;
								SelectEntity(tab, ap3, (BaseEntity)(object)owner);
								Singleton.Draw(ap3.Player);
							}, delegate
							{
								fields.Clear();
								fields = null;
							});
						}));
					}
					else
					{
						tab.AddText(1, "You need 'carbon.cmod' permission to kick, ban, sleep or change player hostility.", 10, "1 1 1 0.4", (TextAnchor)4);
					}
					List<Tab.OptionButton> list = Pool.Get<List<Tab.OptionButton>>();
					if (Singleton.HasAccess(ap3.Player, "entities.loot_players"))
					{
						list.Add(new Tab.OptionButton("Loot", delegate(PlayerSession ap4)
						{
							if (!multiSelection)
							{
								OpenPlayerContainer(ap4, player, tab);
							}
						}));
						list.Add(new Tab.OptionButton("Strip", delegate
						{
							if (!multiSelection)
							{
								player.inventory.Strip();
							}
						}));
					}
					if (Singleton.HasAccess(ap3.Player, "entities.respawn_players"))
					{
						list.Add(new Tab.OptionButton("Respawn", delegate
						{
							tab.CreateDialog("Are you sure about that?", delegate
							{
								DoAll<BasePlayer>(delegate(BasePlayer e)
								{
									((BaseCombatEntity)e).Hurt(((BaseEntity)player).MaxHealth());
									e.Respawn();
									e.EndSleeping();
								});
							});
						}));
					}
					tab.AddButtonArray(column, list.ToArray());
					Pool.FreeUnmanaged<Tab.OptionButton>(ref list);
					tab.AddText(1, "To loot a backpack, drag the backpack item over any hotbar slots while looting a player", 10, "1 1 1 0.4", (TextAnchor)4);
					if (Singleton.HasAccess(ap3.Player, "players.inventory_management"))
					{
						tab.AddName(1, "Inventory Lock", (TextAnchor)3);
						tab.AddButtonArray(1, new Tab.OptionButton("Main", delegate
						{
							player.inventory.containerMain.SetLocked(!player.inventory.containerMain.IsLocked(), false);
						}, (PlayerSession playerSession) => player.inventory.containerMain.IsLocked() ? Tab.OptionButton.Types.Important : Tab.OptionButton.Types.None), new Tab.OptionButton("Belt", delegate
						{
							player.inventory.containerBelt.SetLocked(!player.inventory.containerBelt.IsLocked(), false);
						}, (PlayerSession playerSession) => player.inventory.containerBelt.IsLocked() ? Tab.OptionButton.Types.Important : Tab.OptionButton.Types.None), new Tab.OptionButton("Wear", delegate
						{
							player.inventory.containerWear.SetLocked(!player.inventory.containerWear.IsLocked(), false);
						}, (PlayerSession playerSession) => player.inventory.containerWear.IsLocked() ? Tab.OptionButton.Types.Important : Tab.OptionButton.Types.None));
					}
					if (!multiSelection && ap3 != null && Singleton.HasAccess(ap3.Player, "entities.blind_players"))
					{
						if (!PlayersTab.BlindedPlayers.Contains(player))
						{
							tab.AddButton(1, "Blind Player", delegate
							{
								tab.CreateDialog("Are you sure you want to blind the player?", delegate(PlayerSession playerSession2)
								{
									BlindPlayer(ap3.Player, player);
									SelectEntity(tab, playerSession2, entity);
									DrawEntitySettings(tab, column, ap3);
									if ((Object)(object)playerSession2.Player == (Object)(object)player)
									{
										Core.timer.In(1f, delegate
										{
											Singleton.Close(player);
										});
									}
								});
							}, null, (TextAnchor)4);
						}
						else
						{
							tab.AddButton(1, "Unblind Player", delegate(PlayerSession session)
							{
								UnblindPlayer(ap3.Player, player);
								SelectEntity(tab, session, entity);
								DrawEntitySettings(tab, column, ap3);
							}, (PlayerSession playerSession) => Tab.OptionButton.Types.Selected, (TextAnchor)4);
						}
					}
				}
				if (!multiSelection && ((EntityRef)(ref ((BaseNetworkable)entity).parentEntity)).IsValid(true))
				{
					tab.AddButton(column, $"Parent: {((EntityRef)(ref ((BaseNetworkable)entity).parentEntity)).Get(true)}", delegate(PlayerSession playerSession)
					{
						DrawEntities(tab, playerSession);
						SelectEntity(tab, playerSession, ((EntityRef)(ref ((BaseNetworkable)entity).parentEntity)).Get(true));
						DrawEntitySettings(tab, 1, playerSession);
					}, null, (TextAnchor)4);
				}
				if (!multiSelection && ((BaseNetworkable)entity).children.Count > 0)
				{
					tab.AddName(column, "Children", (TextAnchor)3);
					foreach (BaseEntity child in ((BaseNetworkable)entity).children)
					{
						tab.AddButton(column, $"{child}", delegate(PlayerSession playerSession)
						{
							SelectEntity(tab, playerSession, child);
							DrawEntities(tab, playerSession);
							DrawEntitySettings(tab, 1, playerSession);
						}, null, (TextAnchor)4);
					}
				}
				BaseEntity obj3 = entity;
				CCTV_RC val = (CCTV_RC)(object)((obj3 is CCTV_RC) ? obj3 : null);
				if (val == null)
				{
					BaseEntity obj4 = entity;
					CodeLock val2 = (CodeLock)(object)((obj4 is CodeLock) ? obj4 : null);
					if (val2 == null)
					{
						BaseEntity obj5 = entity;
						Minicopter val3 = (Minicopter)(object)((obj5 is Minicopter) ? obj5 : null);
						if (val3 == null)
						{
							BaseEntity obj6 = entity;
							BuildingBlock val4 = (BuildingBlock)(object)((obj6 is BuildingBlock) ? obj6 : null);
							if (val4 != null)
							{
								tab.AddName(column, "Building Block", (TextAnchor)3);
								tab.AddDropdown(column, "Grade", (PlayerSession playerSession) => (int)val4.grade, delegate(PlayerSession ap4, int index)
								{
									DoAll<BuildingBlock>(delegate(BuildingBlock e)
									{
										e.ChangeGrade((Enum)index, true, true);
										((BaseEntity)e).skinID = 0uL;
									});
									DrawEntitySettings(tab, column, ap4);
								}, BuildingGrades);
							}
						}
						else
						{
							tab.AddName(column, "Minicopter", (TextAnchor)3);
							if (!Object.op_Implicit((Object)(object)val3))
							{
								tab.AddButton(column, "Open Fuel", delegate(PlayerSession playerSession)
								{
									LastContainerLooter = playerSession;
									Core.timer.In(0.2f, delegate
									{
										Admin.Close(playerSession.Player);
									});
									Core.timer.In(0.5f, delegate
									{
										((PlayerHelicopter)val3).engineController.FuelSystem.LootFuel(playerSession.Player);
									});
								}, null, (TextAnchor)4);
							}
						}
					}
					else
					{
						tab.AddName(column, "Code Lock", (TextAnchor)3);
						tab.AddInput(column, "Code", (PlayerSession playerSession) => (!multiSelection) ? val2.code : "-", delegate(PlayerSession playerSession, object[] args)
						{
							string code = (string)args[0];
							string text = code;
							foreach (char c in text)
							{
								if (char.IsLetter(c))
								{
									return;
								}
							}
							DoAll<CodeLock>(delegate(CodeLock e)
							{
								e.code = StringEx.Truncate(code, 4);
							});
						});
					}
				}
				else
				{
					tab.AddName(column, "CCTV", (TextAnchor)3);
					tab.AddInput(column, "Identifier", (PlayerSession playerSession) => (!multiSelection) ? ((PoweredRemoteControlEntity)val).GetIdentifier() : "-", delegate(PlayerSession playerSession, object[] args)
					{
						((PoweredRemoteControlEntity)val).UpdateIdentifier((string)args[0], true);
					});
					if (!multiSelection)
					{
						tab.AddButton(column, "View CCTV", delegate(PlayerSession playerSession)
						{
							Core.timer.In(0.1f, delegate
							{
								Admin.Close(playerSession.Player);
								playerSession.SetStorage(tab, "wasviewingcam", value: true);
							});
							Core.timer.In(0.3f, delegate
							{
								//IL_0038: Unknown result type (might be due to invalid IL or missing references)
								//IL_003f: Unknown result type (might be due to invalid IL or missing references)
								//IL_0045: Unknown result type (might be due to invalid IL or missing references)
								Admin.Subscribe("OnEntityDismounted");
								Admin.Subscribe("CanDismountEntity");
								BaseEntity obj8 = GameManager.server.CreateEntity("assets/prefabs/deployable/computerstation/computerstation.deployed.prefab", ((Component)playerSession.Player).transform.position, default(Quaternion), true);
								ComputerStation val5 = (ComputerStation)(object)((obj8 is ComputerStation) ? obj8 : null);
								((BaseEntity)val5).skinID = 69696uL;
								val5.SendControlBookmarks(playerSession.Player);
								((BaseNetworkable)val5).Spawn();
								((BaseMountable)val5).checkPlayerLosOnMount = false;
								((BaseMountable)val5).legacyDismount = true;
								((BaseMountable)val5).MountPlayer(playerSession.Player);
								ViewCamera(playerSession.Player, val5, val);
							});
						}, null, (TextAnchor)4);
					}
				}
				BaseEntity obj7 = entity;
				BaseCombatEntity combat = (BaseCombatEntity)(object)((obj7 is BaseCombatEntity) ? obj7 : null);
				if (combat == null)
				{
					return;
				}
				tab.AddName(column, "Combat", (TextAnchor)3);
				tab.AddRange(column, "Health", 0f, ((BaseEntity)combat).MaxHealth(), (PlayerSession playerSession) => combat.health, delegate(PlayerSession playerSession, float value)
				{
					DoAll<BaseCombatEntity>(delegate(BaseCombatEntity e)
					{
						e.SetHealth(value);
					});
				}, (PlayerSession playerSession) => $"{combat.health:0}");
				if (!(entity is BasePlayer))
				{
					return;
				}
				tab.AddRange(column, "Thirst", 0f, ((BaseMetabolism<BasePlayer>)(object)player.metabolism).hydration.max, (PlayerSession _) => ((BaseMetabolism<BasePlayer>)(object)player.metabolism).hydration.value, delegate(PlayerSession _, float value)
				{
					DoAll<BasePlayer>(delegate(BasePlayer e)
					{
						((BaseMetabolism<BasePlayer>)(object)e.metabolism).hydration.SetValue(value);
					});
				}, (PlayerSession _) => $"{((BaseMetabolism<BasePlayer>)(object)player.metabolism).hydration.value:0}");
				tab.AddRange(column, "Hunger", 0f, ((BaseMetabolism<BasePlayer>)(object)player.metabolism).calories.max, (PlayerSession _) => ((BaseMetabolism<BasePlayer>)(object)player.metabolism).calories.value, delegate(PlayerSession _, float value)
				{
					DoAll<BasePlayer>(delegate(BasePlayer e)
					{
						((BaseMetabolism<BasePlayer>)(object)e.metabolism).calories.SetValue(value);
					});
				}, (PlayerSession _) => $"{((BaseMetabolism<BasePlayer>)(object)player.metabolism).calories.value:0}");
				tab.AddRange(column, "Radiation", 0f, player.metabolism.radiation_poison.max, (PlayerSession _) => player.metabolism.radiation_poison.value, delegate(PlayerSession _, float value)
				{
					DoAll<BasePlayer>(delegate(BasePlayer e)
					{
						e.metabolism.radiation_poison.SetValue(value);
					});
				}, (PlayerSession _) => $"{player.metabolism.radiation_poison.value:0}");
				tab.AddRange(column, "Bleeding", 0f, player.metabolism.bleeding.max, (PlayerSession _) => player.metabolism.bleeding.value, delegate(PlayerSession _, float value)
				{
					DoAll<BasePlayer>(delegate(BasePlayer e)
					{
						e.metabolism.bleeding.SetValue(value);
					});
				}, (PlayerSession _) => $"{player.metabolism.bleeding.value:0}");
				tab.AddRange(column, "Wetness", 0f, player.metabolism.wetness.max * 10f, (PlayerSession playerSession) => player.metabolism.wetness.value * 10f, delegate(PlayerSession _, float value)
				{
					player.metabolism.wetness.SetValue(value * 0.1f);
				}, (PlayerSession _) => $"{player.metabolism.wetness.value * 100f:0}%");
				tab.AddButton(column, "Empower Stats", delegate
				{
					EmpowerPlayerStats(ap3.Player, player);
				}, null, (TextAnchor)4);
			}
			else
			{
				tab.ClearColumn(1);
				DrawEntities(tab, ap3);
			}
			void DoAll<T>(Action<T> callback) where T : BaseEntity
			{
				foreach (BaseEntity item in selectedEntitites.Where((BaseEntity selectedEntity) => (Object)(object)selectedEntity != (Object)null))
				{
					callback?.Invoke((T)(object)item);
				}
			}
		}

		internal static void DrawEntityFlags(Tab tab, PlayerSession session, int column = 1)
		{
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			List<BaseEntity> selectedEntitites = session.GetStorage(tab, "selectedentities", new List<BaseEntity>());
			tab.ClearColumn(column);
			if (selectedEntitites.Count == 0)
			{
				return;
			}
			BaseEntity val = selectedEntitites[0];
			int num = 0;
			List<Tab.OptionButton> list = Pool.Get<List<Tab.OptionButton>>();
			tab.ClearColumn(column);
			tab.AddName(column, "Entity Flags", (TextAnchor)3);
			foreach (string item in from x in Enum.GetNames(typeof(Flags))
				orderby x
				select x)
			{
				Flags flagValue = (Flags)Enum.Parse(typeof(Flags), item);
				bool isDifferent = selectedEntitites.All((BaseEntity x) => x.HasFlag(flagValue));
				bool hasFlag = val.HasFlag(flagValue);
				list.Add(new Tab.OptionButton(item, delegate(PlayerSession ap)
				{
					DoAll<BaseEntity>(delegate(BaseEntity e)
					{
						//IL_0002: Unknown result type (might be due to invalid IL or missing references)
						e.SetFlag(flagValue, !hasFlag, false, true);
					});
					DrawEntitySettings(tab, 0, ap);
					DrawEntityFlags(tab, ap, column);
				}, (PlayerSession ap) => (!isDifferent) ? (hasFlag ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None) : Tab.OptionButton.Types.Warned));
				num++;
				if (num >= 5)
				{
					tab.AddButtonArray(column, list.ToArray());
					list.Clear();
					num = 0;
				}
			}
			Pool.FreeUnmanaged<Tab.OptionButton>(ref list);
			void DoAll<T>(Action<T> callback) where T : BaseEntity
			{
				foreach (BaseEntity item2 in selectedEntitites)
				{
					if (!((Object)(object)item2 == (Object)null))
					{
						callback?.Invoke((T)(object)item2);
					}
				}
			}
		}

		internal static void ViewCamera(BasePlayer player, ComputerStation station, CCTV_RC camera)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			((BaseNetworkable)player).net.SwitchSecondaryGroup(((BaseNetworkable)camera).net.group);
			((EntityRef)(ref station.currentlyControllingEnt)).uid = ((BaseNetworkable)camera).net.ID;
			station.currentPlayerID = EncryptedValue<ulong>.op_Implicit(player.userID);
			bool flag = ((PoweredRemoteControlEntity)camera).InitializeControl(new CameraViewerId(station.currentPlayerID, 0L));
			((BaseEntity)station).SetFlag((Flags)256, flag, false, false);
			((BaseNetworkable)station).SendNetworkUpdateImmediate();
			station.SendControlBookmarks(player);
		}

		internal static void SendEntityToPlayer(BasePlayer player, BaseEntity entity)
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			Connection connection = player.Connection;
			if (connection != null)
			{
				NetWrite val = ((BaseNetwork)Net.sv).StartWrite();
				if (val != null)
				{
					connection.validate.entityUpdates++;
					val.PacketID((Type)5);
					val.UInt32(connection.validate.entityUpdates);
					((BaseNetworkable)entity).ToStreamForNetwork((Stream)(object)val, new SaveInfo
					{
						forConnection = connection,
						forDisk = false
					});
					val.Send(new SendInfo(connection));
				}
			}
		}
	}

	public class EnvironmentTab
	{
		private static int LastWeatherPresetSelectedIndex;

		private static string[] Options;

		public static Tab Get()
		{
			Tab tab = null;
			return new Tab("env", "Environment", Community.Runtime.Core, delegate(PlayerSession ap, Tab tab2)
			{
				if (Options == null)
				{
					Options = SingletonComponent<Climate>.Instance?.WeatherPresets.Select((WeatherPreset x) => ((Object)x).name).ToArray();
				}
				Draw(tab2);
			}, "environment.use");
		}

		private static void Draw(Tab tab)
		{
			WeatherPreset[] presets = SingletonComponent<Climate>.Instance.WeatherPresets;
			WeatherPreset overrides = SingletonComponent<Climate>.Instance.WeatherOverrides;
			tab.AddColumn(0, clear: true);
			tab.AddColumn(1, clear: true);
			tab.AddName(0, "Time", (TextAnchor)3);
			tab.AddInputButton(0, "Date", 0.3f, new Tab.OptionInput(null, (PlayerSession ap) => TOD_Sky.Instance.Cycle.DateTime.ToString(), 0, readOnly: true, null), new Tab.OptionButton("Change", delegate(PlayerSession ap)
			{
				Singleton.DatePicker.Open(ap.Player, delegate(DateTime date)
				{
					float hour = TOD_Sky.Instance.Cycle.Hour;
					TOD_Sky.Instance.Cycle.DateTime = date;
					TOD_Sky.Instance.Cycle.Hour = hour;
					Draw(tab);
					Singleton.Draw(ap.Player);
				});
			}));
			tab.AddToggle(0, "Progress Time", delegate
			{
				TOD_Sky.Instance.Components.Time.ProgressTime = !TOD_Sky.Instance.Components.Time.ProgressTime;
			}, (PlayerSession ap) => TOD_Sky.Instance.Components.Time.ProgressTime);
			tab.AddRange(0, "Time", 0f, 24f, (PlayerSession ap) => TOD_Sky.Instance.Cycle.Hour, delegate(PlayerSession ap, float value)
			{
				TOD_Sky.Instance.Cycle.Hour = value;
			}, (PlayerSession ap) => $"{TOD_Sky.Instance.Cycle.Hour:0.0}");
			tab.AddName(0, "Ocean", (TextAnchor)3);
			tab.AddRange(0, "Scale", -100f, 500f, (PlayerSession ap) => overrides.OceanScale * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.OceanScale = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.OceanScale:0.0}");
			tab.AddRange(0, "Level", 0f, 500f, (PlayerSession ap) => WaterSystem.OceanLevel, delegate(PlayerSession ap, float value)
			{
				WaterSystem.OceanLevel = value;
				ServerMgr.SendReplicatedVars("env.");
			}, (PlayerSession ap) => $"{WaterSystem.OceanLevel:0.0}");
			tab.AddDropdown(1, "Weather Preset", (PlayerSession ap) => LastWeatherPresetSelectedIndex, delegate(PlayerSession ap, int index)
			{
				overrides.Set(presets[LastWeatherPresetSelectedIndex = index]);
				ServerMgr.SendReplicatedVars("weather.");
			}, Options);
			tab.AddName(1, "Environment", (TextAnchor)3);
			tab.AddRange(1, "Wind", -100f, 100f, (PlayerSession ap) => overrides.Wind * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Wind = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Wind:0.0}");
			tab.AddRange(1, "Rain", -100f, 100f, (PlayerSession ap) => overrides.Rain * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Rain = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Rain:0.0}");
			tab.AddRange(1, "Thunder", -100f, 100f, (PlayerSession ap) => overrides.Thunder * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Thunder = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Thunder:0.0}");
			tab.AddRange(1, "Rainbow", -100f, 100f, (PlayerSession ap) => overrides.Rainbow * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Rainbow = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Rainbow:0.0}");
			tab.AddName(1, "Atmosphere", (TextAnchor)3);
			tab.AddRange(1, "RayleighMultiplier", -100f, 500f, (PlayerSession ap) => overrides.Atmosphere.RayleighMultiplier * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Atmosphere.RayleighMultiplier = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Atmosphere.RayleighMultiplier:0.0}");
			tab.AddRange(1, "MieMultiplier", -100f, 500f, (PlayerSession ap) => overrides.Atmosphere.MieMultiplier * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Atmosphere.MieMultiplier = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Atmosphere.MieMultiplier:0.0}");
			tab.AddRange(1, "Brightness", -100f, 500f, (PlayerSession ap) => overrides.Atmosphere.Brightness * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Atmosphere.Brightness = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Atmosphere.Brightness:0.0}");
			tab.AddRange(1, "Contrast", -100f, 500f, (PlayerSession ap) => overrides.Atmosphere.Contrast * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Atmosphere.Contrast = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Atmosphere.Contrast:0.0}");
			tab.AddRange(1, "Directionality", -100f, 500f, (PlayerSession ap) => overrides.Atmosphere.Directionality * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Atmosphere.Directionality = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Atmosphere.Directionality:0.0}");
			tab.AddRange(1, "Fogginess", -100f, 500f, (PlayerSession ap) => overrides.Atmosphere.Fogginess * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Atmosphere.Fogginess = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Atmosphere.Fogginess:0.0}");
			tab.AddName(1, "Clouds", (TextAnchor)3);
			tab.AddRange(1, "Size", -100f, 500f, (PlayerSession ap) => overrides.Clouds.Size * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Clouds.Size = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Clouds.Size:0.0}");
			tab.AddRange(1, "Opacity", -100f, 500f, (PlayerSession ap) => overrides.Clouds.Opacity * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Clouds.Opacity = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Clouds.Opacity:0.0}");
			tab.AddRange(1, "Coverage", -100f, 500f, (PlayerSession ap) => overrides.Clouds.Coverage * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Clouds.Coverage = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Clouds.Coverage:0.0}");
			tab.AddRange(1, "Sharpness", -100f, 500f, (PlayerSession ap) => overrides.Clouds.Sharpness * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Clouds.Sharpness = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Clouds.Sharpness:0.0}");
			tab.AddRange(1, "Coloring", -100f, 500f, (PlayerSession ap) => overrides.Clouds.Coloring * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Clouds.Coloring = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Clouds.Coloring:0.0}");
			tab.AddRange(1, "Attenuation", -100f, 500f, (PlayerSession ap) => overrides.Clouds.Attenuation * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Clouds.Attenuation = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Clouds.Attenuation:0.0}");
			tab.AddRange(1, "Saturation", -100f, 500f, (PlayerSession ap) => overrides.Clouds.Saturation * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Clouds.Saturation = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Clouds.Saturation:0.0}");
			tab.AddRange(1, "Scattering", -100f, 500f, (PlayerSession ap) => overrides.Clouds.Scattering * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Clouds.Scattering = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Clouds.Scattering:0.0}");
			tab.AddRange(1, "Brightness", -100f, 500f, (PlayerSession ap) => overrides.Clouds.Brightness * 100f, delegate(PlayerSession ap, float value)
			{
				overrides.Clouds.Brightness = value * 0.01f;
				ServerMgr.SendReplicatedVars("weather.");
			}, (PlayerSession ap) => $"{overrides.Clouds.Brightness:0.0}");
		}
	}

	public class Greet
	{
		public static Tab Make()
		{
			Tab tab = new Tab("greet", "Greet", Community.Runtime.Core)
			{
				IsFullscreen = true
			};
			tab.Override = delegate(Tab _, CUI cui, CuiElementContainer container, string panel, PlayerSession _)
			{
				cui.CreateImage(container, panel, "carbonws", "1 1 1 0.7", null, 0.2f, 0.8f, 0.52f, 0.71f, 0f, 0f, -20f, -20f);
				cui.CreateText(container, panel, "1 1 1 0.5", "Welcome to <b>Carbon</b>!\n\n<size=12><color=grey>If you've seen this panel again, your existent settings have not been reset.\nFor more information, go to <color=orange>carbonmod.gg</color>.</color></size>", 18, 0f, 1f, 0f, 0.495f, 0f, 0f, -20f, -20f, (TextAnchor)1, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				cui.CreateProtectedButton(container, panel, "#7d8f32", "1 1 1 1", "Continue ▶", 9, null, 0.5f, 0.5f, 0.25f, 0.25f, -30f, 30f, -12.5f, 12.5f, "greet.continue", (TextAnchor)4);
			};
			return tab;
		}
	}

	public class LangEditor : Tab
	{
		internal BaseHookable TargetPlugin;

		internal Action<PlayerSession> OnCancel;

		internal const string Spacing = " ";

		public LangEditor(string id, string name, RustPlugin plugin, Action<PlayerSession, Tab> onChange = null)
			: base(id, name, plugin, onChange)
		{
		}

		public static LangEditor Make(Plugin plugin, Action<PlayerSession> onCancel)
		{
			LangEditor langEditor = new LangEditor("langeditor", "Lang Editor", Community.Runtime.Core)
			{
				TargetPlugin = plugin,
				OnCancel = onCancel
			};
			langEditor._draw();
			return langEditor;
		}

		internal void _draw()
		{
			AddColumn(0, clear: true);
			AddColumn(1, clear: true);
			AddButton(0, "Cancel", delegate(PlayerSession ap)
			{
				OnCancel?.Invoke(ap);
			}, (PlayerSession ap) => OptionButton.Types.Important, (TextAnchor)4);
			string[] directories = Directory.GetDirectories(Defines.GetLangFolder());
			foreach (string path in directories)
			{
				string[] files = Directory.GetFiles(path);
				if (files.Length == 0)
				{
					continue;
				}
				IEnumerable<string> source = files.Where((string x) => StringEx.Contains(x, TargetPlugin.Name, CompareOptions.OrdinalIgnoreCase));
				if (!source.Any())
				{
					continue;
				}
				string file = source.FirstOrDefault();
				AddButton(0, Path.GetFileName(path), delegate(PlayerSession ap)
				{
					Singleton.SetTab(ap.Player, ConfigEditor.Make(OsEx.File.ReadText(file), delegate(PlayerSession playerSession, JObject jobject)
					{
						Community.Runtime.Core.NextTick(delegate
						{
							Singleton.SetTab(playerSession.Player, "plugins", onChange: false);
						});
					}, delegate(PlayerSession playerSession, JObject jobject)
					{
						OsEx.File.Create(file, ((JToken)jobject).ToString((Formatting)1, Array.Empty<JsonConverter>()));
						Community.Runtime.Core.NextTick(delegate
						{
							Singleton.SetTab(playerSession.Player, "plugins", onChange: false);
						});
					}, delegate(PlayerSession playerSession, JObject jobject)
					{
						OsEx.File.Create(file, ((JToken)jobject).ToString((Formatting)1, Array.Empty<JsonConverter>()));
						if (TargetPlugin is RustPlugin rustPlugin)
						{
							rustPlugin.ProcessorProcess.MarkDirty();
						}
						Community.Runtime.Core.NextTick(delegate
						{
							Singleton.SetTab(playerSession.Player, "plugins", onChange: false);
						});
					}));
				}, (PlayerSession ap) => OptionButton.Types.Warned, (TextAnchor)4);
			}
		}
	}

	public class ModulesTab
	{
		public enum SortTypes
		{
			Loaded,
			Name,
			Enabled
		}

		private static string[] _sortTypeNames = Enum.GetNames(typeof(SortTypes));

		private static string[] _configBlacklist = new string[1] { "Version" };

		public static Tab Get()
		{
			Tab tab = null;
			return new Tab("modules", "Modules", Community.Runtime.Core, delegate(PlayerSession ap, Tab tab2)
			{
				Draw(tab2, ap);
			}, "modules.use");
		}

		private static void Draw(Tab tab, PlayerSession ap)
		{
			tab.AddColumn(0, clear: true);
			tab.AddColumn(1, clear: true);
			string searchInput = ap.GetStorage<string>(tab, "search")?.ToLower();
			tab.AddInput(0, "Search", (PlayerSession playerSession) => searchInput, delegate(PlayerSession playerSession, object[] args)
			{
				playerSession.SetStorage(tab, "search", args.Select((object x) => x as string).ToString(" "));
				Draw(tab, playerSession);
			});
			SortTypes sort = (SortTypes)ap.GetStorage(tab, "sorttype", 0);
			bool sortFlip = ap.GetStorage(tab, "sortflip", @default: false);
			tab.AddDropdown(0, "Sorting", (PlayerSession playerSession) => (int)sort, delegate(PlayerSession playerSession, int index)
			{
				if (sort != (SortTypes)index)
				{
					playerSession.SetStorage(tab, "sortflip", value: false);
					playerSession.SetStorage(tab, "sorttype", index);
				}
				else
				{
					playerSession.SetStorage(tab, "sortflip", !sortFlip);
				}
				Draw(tab, playerSession);
			}, _sortTypeNames);
			tab.AddName(0, "Core Modules", (TextAnchor)3);
			Generate(sort, sortFlip, tab, (BaseModule x) => x.ForceEnabled && (!ap.HasStorage(tab, "search") || string.IsNullOrEmpty(searchInput) || x.Name.ToLower().Contains(searchInput)));
			tab.AddName(0, "Other Modules", (TextAnchor)3);
			Generate(sort, sortFlip, tab, (BaseModule x) => !x.ForceEnabled && (!ap.HasStorage(tab, "search") || string.IsNullOrEmpty(searchInput) || x.Name.ToLower().Contains(searchInput)));
			static void Generate(SortTypes sortTypes, bool flag, Tab tab2, Func<BaseModule, bool> condition)
			{
				IEnumerable<BaseHookable> enumerable = sortTypes switch
				{
					SortTypes.Name => Community.Runtime.ModuleProcessor.Modules.OrderBy((BaseHookable x) => x.Name), 
					SortTypes.Enabled => Community.Runtime.ModuleProcessor.Modules.OrderByDescending((BaseHookable x) => x is BaseModule baseModule && baseModule.IsEnabled()), 
					_ => Community.Runtime.ModuleProcessor.Modules, 
				};
				if (flag)
				{
					enumerable = enumerable.Reverse();
				}
				foreach (BaseHookable item in enumerable)
				{
					BaseModule module = item as BaseModule;
					if (module != null && condition(module))
					{
						string moduleConfigFile = Path.Combine(Defines.GetModulesFolder(), module.Name, "config.json");
						bool exists = OsEx.File.Exists(moduleConfigFile);
						tab2.AddButtonArray(0, new Tab.OptionButton(item.Name, delegate(PlayerSession ap2)
						{
							Draw(tab2, ap2);
						}, (PlayerSession _) => Tab.OptionButton.Types.None), new Tab.OptionButton((module.ForceEnabled ? "Always Enabled" : (module.IsEnabled() ? "Enabled" : "Disabled")) ?? "", delegate(PlayerSession ap2)
						{
							if (!module.ForceEnabled)
							{
								module.SetEnabled(!module.IsEnabled());
								module.Save();
								Draw(tab2, ap2);
							}
						}, (PlayerSession playerSession) => (!module.ForceEnabled) ? (module.IsEnabled() ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None) : Tab.OptionButton.Types.Warned), new Tab.OptionButton("Edit Config", delegate(PlayerSession playerSession)
						{
							if (exists)
							{
								playerSession.SelectedTab = ConfigEditor.Make(OsEx.File.ReadText(moduleConfigFile), delegate(PlayerSession playerSession2, JObject _)
								{
									Singleton.SetTab(playerSession2.Player, "modules");
									Singleton.Draw(playerSession2.Player);
								}, delegate(PlayerSession playerSession2, JObject jobject)
								{
									bool flag2 = module.IsEnabled();
									OsEx.File.Create(moduleConfigFile, ((JToken)jobject).ToString((Formatting)1, Array.Empty<JsonConverter>()));
									module.SetEnabled(enable: false);
									module.OnUnload();
									module.Load();
									if (flag2)
									{
										module.SetEnabled(flag2);
									}
									Singleton.SetTab(playerSession2.Player, "modules");
									Singleton.Draw(playerSession2.Player);
								}, null, fullscreen: false, _configBlacklist);
							}
						}, (PlayerSession playerSession) => exists ? Tab.OptionButton.Types.Warned : Tab.OptionButton.Types.None));
					}
				}
			}
		}
	}

	public class PermissionsTab
	{
		public enum HookableTypes
		{
			Plugin,
			Module
		}

		public enum SortTypes
		{
			Loaded,
			Name,
			Version
		}

		internal static Permission permission;

		public static string[] SortTypeNames = Enum.GetNames(typeof(SortTypes));

		public static Tab Get()
		{
			permission = Community.Runtime.Core.permission;
			Tab tab = new Tab("permissions", "Permissions", Community.Runtime.Core, delegate(PlayerSession ap, Tab tab2)
			{
				ap.SetStorage(tab2, "toggleall", value: true);
				ap.SetStorage(tab2, "groupedit", value: false);
				tab2.ClearColumn(1);
				tab2.ClearColumn(2);
				tab2.ClearColumn(3);
				ap.Clear();
				ap.SetStorage(tab2, "pluginedit", value: false);
				ap.SetStorage(tab2, "option", 0);
				GeneratePlayers(tab2, permission, ap);
			}, "permissions.use");
			tab.AddName(0, "Options", (TextAnchor)3);
			tab.AddButton(0, "Players", delegate(PlayerSession ap)
			{
				ap.SetStorage(tab, "toggleall", value: true);
				ap.SetStorage(tab, "groupedit", value: false);
				tab.ClearColumn(1);
				tab.ClearColumn(2);
				tab.ClearColumn(3);
				ap.Clear();
				ap.SetStorage(tab, "pluginedit", value: false);
				ap.SetStorage(tab, "option", 0);
				GeneratePlayers(tab, permission, ap);
			}, (PlayerSession ap) => (ap.GetStorage(tab, "option", 0) == 0) ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None, (TextAnchor)4);
			GeneratePlayers(tab, permission, PlayerSession.Blank);
			tab.AddButton(0, "Groups", delegate(PlayerSession ap)
			{
				ap.SetStorage(tab, "toggleall", value: true);
				ap.SetStorage(tab, "pluginedit", value: false);
				ap.SetStorage(tab, "groupedit", value: false);
				tab.ClearColumn(1);
				tab.ClearColumn(2);
				tab.ClearColumn(3);
				ap.Clear();
				ap.ClearStorage(tab, "player");
				ap.ClearStorage(tab, "plugin");
				ap.SetStorage(tab, "option", 1);
				GenerateGroups(tab, permission, ap);
			}, (PlayerSession ap) => (ap.GetStorage(tab, "option", 0) == 1) ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None, (TextAnchor)4);
			tab.AddColumn(1);
			tab.AddColumn(2);
			tab.AddColumn(3);
			return tab;
		}

		public static void GeneratePlayers(Tab tab, Permission perms, PlayerSession ap)
		{
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			string filter = ap.GetStorage(tab, "playerfilter", string.Empty)?.Trim().ToLower();
			tab.ClearColumn(1);
			tab.AddName(1, "Players", (TextAnchor)3);
			tab.AddInput(1, "Search", (PlayerSession playerSession) => playerSession.GetStorage(tab, "playerfilter", string.Empty), delegate(PlayerSession playerSession, object[] args)
			{
				playerSession.SetStorage(tab, "playerfilter", args.Select((object x) => x as string).ToString(" "));
				GeneratePlayers(tab, perms, playerSession);
			});
			tab.AddButtonArray(1, new Tab.OptionButton("Add User", delegate(PlayerSession playerSession)
			{
				Singleton.Modal.Open(playerSession.Player, "Create New User", new Dictionary<string, ModalModule.Modal.Field>
				{
					["steamid"] = ModalModule.Modal.Field.Make("Steam ID", ModalModule.Modal.Field.FieldTypes.String, required: true, null, isReadOnly: false, (ModalModule.Modal.Field field) => field.Get<string>().IsSteamId() ? ((!permission.UserExists(field.Get<string>())) ? string.Empty : "User with the same Steam ID already exists.") : "Not a valid Steam ID."),
					["displayname"] = ModalModule.Modal.Field.Make("Display Name", ModalModule.Modal.Field.FieldTypes.String),
					["language"] = ModalModule.Modal.Field.Make("Language", ModalModule.Modal.Field.FieldTypes.String)
				}, delegate(BasePlayer pl, ModalModule.Modal mod)
				{
					UserData userData = permission.GetUserData(mod.Get<string>("steamid"), addIfNotExisting: true);
					userData.LastSeenNickname = mod.Get<string>("displayname");
					userData.Language = mod.Get<string>("language");
					GeneratePlayers(tab, perms, playerSession);
				});
			}, (PlayerSession playerSession) => Tab.OptionButton.Types.None));
			IEnumerable<BasePlayer> enumerable = BasePlayer.allPlayerList.Where(delegate(BasePlayer x)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				if (!x.userID.IsSteamId())
				{
					return false;
				}
				return string.IsNullOrEmpty(filter) || x.displayName.ToLower().Contains(filter) || x.UserIDString.Contains(filter);
			});
			foreach (BasePlayer player in enumerable)
			{
				tab.AddRow(1, new Tab.OptionButton($"{player.displayName} ({player.userID})", delegate
				{
					ap.SetStorage(tab, "player", player.UserIDString);
					ap.ClearStorage(tab, "plugin");
					tab.ClearColumn(3);
					GenerateHookables(tab, ap, perms, permission.FindUser(player.UserIDString), null, HookableTypes.Plugin);
				}, (PlayerSession _instance) => (ap.GetStorage<string>(tab, "player") == player.UserIDString) ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None));
			}
		}

		public static void GenerateHookables(Tab tab, PlayerSession ap, Permission permission, KeyValuePair<string, UserData> player, string selectedGroup, HookableTypes hookableType)
		{
			bool groupEdit = ap.GetStorage(tab, "groupedit", @default: false);
			bool pluginEdit = ap.GetStorage(tab, "pluginedit", @default: false);
			string filter = ap.GetStorage(tab, "pluginfilter", string.Empty)?.Trim().ToLower();
			tab.ClearColumn(2);
			if (string.IsNullOrEmpty(selectedGroup))
			{
				tab.AddName(2, player.Value.LastSeenNickname ?? "", (TextAnchor)7);
				tab.AddText(2, player.Key, 8, "1 1 1 0.6", (TextAnchor)1, CUI.Handler.FontTypes.RobotoCondensedRegular, isInput: true);
				BasePlayer existentPlayer = BasePlayer.FindAwakeOrSleeping(player.Key);
				tab.AddButtonArray(2, new Tab.OptionButton("Select Player", delegate
				{
					Singleton.SetTab(ap.Player, "players");
					Tab tab2 = Singleton.GetTab(ap.Player);
					ap.SetStorage(tab2, "playerfilterpl", player);
					PlayersTab.RefreshPlayers(tab2, ap);
					PlayersTab.ShowInfo(1, tab2, ap, existentPlayer);
				}, (PlayerSession playerSession) => Tab.OptionButton.Types.Warned), new Tab.OptionButton(groupEdit ? "▼ Plugins" : (((hookableType == HookableTypes.Plugin) ? "▼ Modules" : "▼ Groups") ?? ""), delegate
				{
					if (groupEdit)
					{
						ap.SetStorage(tab, "groupedit", !groupEdit);
						GenerateHookables(tab, ap, permission, player, null, HookableTypes.Plugin);
					}
					else if (hookableType == HookableTypes.Plugin)
					{
						GenerateHookables(tab, ap, permission, player, null, HookableTypes.Module);
					}
					else
					{
						ap.SetStorage(tab, "groupedit", !groupEdit);
						GenerateHookables(tab, ap, permission, player, null, hookableType);
					}
				}), new Tab.OptionButton("Edit User", delegate
				{
					Singleton.Modal.Open(ap.Player, "Edit User", new Dictionary<string, ModalModule.Modal.Field>
					{
						["steamid"] = ModalModule.Modal.Field.Make("Steam ID", ModalModule.Modal.Field.FieldTypes.String, required: false, player.Key, isReadOnly: true),
						["displayname"] = ModalModule.Modal.Field.Make("Display Name", ModalModule.Modal.Field.FieldTypes.String, required: true, player.Value.LastSeenNickname),
						["language"] = ModalModule.Modal.Field.Make("Language", ModalModule.Modal.Field.FieldTypes.String, required: true, player.Value.Language)
					}, delegate(BasePlayer pl, ModalModule.Modal mod)
					{
						UserData userData = permission.GetUserData(player.Key);
						userData.LastSeenNickname = mod.Get<string>("displayname");
						userData.Language = mod.Get<string>("language");
						GeneratePlayers(tab, permission, ap);
					});
				}));
			}
			else
			{
				tab.AddName(2, selectedGroup ?? "", (TextAnchor)3);
				tab.AddButtonArray(2, new Tab.OptionButton("Delete", delegate(PlayerSession ap2)
				{
					tab.CreateDialog("Are you sure you want to delete the '" + selectedGroup + "' group?", delegate
					{
						permission.RemoveGroup(selectedGroup);
						tab.ClearColumn(1);
						tab.ClearColumn(2);
						tab.ClearColumn(3);
						GenerateGroups(tab, permission, ap2);
					});
				}, (PlayerSession playerSession) => Tab.OptionButton.Types.Important), new Tab.OptionButton("Edit", delegate(PlayerSession playerSession)
				{
					List<string> list = Pool.Get<List<string>>();
					string[] groups2 = Community.Runtime.Core.permission.GetGroups();
					list.Add("None");
					list.AddRange(groups2);
					list.Remove(selectedGroup);
					string[] array = list.ToArray();
					Pool.FreeUnmanaged<string>(ref list);
					string groupParent = permission.GetGroupParent(selectedGroup);
					int num2 = Array.IndexOf(array, groupParent);
					Singleton.Modal.Open(playerSession.Player, "Editing '" + selectedGroup + "'", new Dictionary<string, ModalModule.Modal.Field>
					{
						["name"] = ModalModule.Modal.Field.Make("Name", ModalModule.Modal.Field.FieldTypes.String, required: true, selectedGroup, isReadOnly: true),
						["dname"] = ModalModule.Modal.Field.Make("Display Name", ModalModule.Modal.Field.FieldTypes.String, required: false, permission.GetGroupTitle(selectedGroup)),
						["rank"] = ModalModule.Modal.Field.Make("Rank", ModalModule.Modal.Field.FieldTypes.Integer, required: false, permission.GetGroupRank(selectedGroup)),
						["parent"] = ModalModule.Modal.EnumField.MakeEnum("Parent", array, required: false, (!string.IsNullOrEmpty(groupParent)) ? Array.IndexOf(array, groupParent) : 0, isReadOnly: false, (ModalModule.Modal.Field field) => (!(permission.GetGroupParent(array[field.Get<int>()]) == selectedGroup)) ? null : ("Circular parenting detected with '" + array[field.Get<int>()] + "'."))
					}, delegate(BasePlayer val, ModalModule.Modal modal)
					{
						int num3 = modal.Get<int>("parent");
						permission.SetGroupTitle(selectedGroup, modal.Get<string>("dname"));
						permission.SetGroupRank(selectedGroup, modal.Get<int>("rank"));
						if (num3 != 0)
						{
							permission.SetGroupParent(selectedGroup, array[num3]);
						}
						else
						{
							permission.SetGroupParent(selectedGroup, null);
						}
						tab.ClearColumn(1);
						tab.ClearColumn(2);
						tab.ClearColumn(3);
						GenerateGroups(tab, permission, playerSession);
						GenerateHookables(tab, playerSession, permission, permission.FindUser(playerSession.Player.UserIDString), selectedGroup, hookableType);
						Singleton.NextFrame(delegate
						{
							Singleton.Draw(playerSession.Player);
						});
					});
				}));
				tab.AddButtonArray(2, new Tab.OptionButton("Duplicate Group", delegate(PlayerSession playerSession)
				{
					List<string> list = Pool.Get<List<string>>();
					string[] groups2 = Community.Runtime.Core.permission.GetGroups();
					list.Add("None");
					list.AddRange(groups2);
					string[] array = list.ToArray();
					Pool.FreeUnmanaged<string>(ref list);
					Singleton.Modal.Open(playerSession.Player, "Duplicate Group", new Dictionary<string, ModalModule.Modal.Field>
					{
						["name"] = ModalModule.Modal.Field.Make("Name", ModalModule.Modal.Field.FieldTypes.String, required: true, null, isReadOnly: false, (ModalModule.Modal.Field field) => (!permission.GetGroups().Any((string x) => x == field.Get<string>())) ? null : "Group with that name already exists."),
						["dname"] = ModalModule.Modal.Field.Make("Display Name", ModalModule.Modal.Field.FieldTypes.String, required: false, string.Empty),
						["rank"] = ModalModule.Modal.Field.Make("Rank", ModalModule.Modal.Field.FieldTypes.Integer, required: false, 0),
						["parent"] = ModalModule.Modal.EnumField.MakeEnum("Parent", array, required: false, 0)
					}, delegate(BasePlayer p, ModalModule.Modal modal)
					{
						string text = modal.Get<string>("name");
						int num2 = modal.Get<int>("parent");
						permission.CreateGroup(text, modal.Get<string>("dname"), modal.Get<int>("rank"));
						if (num2 != 0)
						{
							permission.SetGroupParent(modal.Get<string>("name"), array[num2]);
						}
						string[] groupPermissions = permission.GetGroupPermissions(selectedGroup);
						string[] array2 = groupPermissions;
						foreach (string perm in array2)
						{
							permission.GrantGroupPermission(text, perm, null);
						}
						tab.ClearColumn(1);
						tab.ClearColumn(2);
						tab.ClearColumn(3);
						GenerateGroups(tab, permission, playerSession);
						Singleton.NextFrame(delegate
						{
							Singleton.Draw(playerSession.Player);
						});
					});
				}, (PlayerSession playerSession) => Tab.OptionButton.Types.None), new Tab.OptionButton(groupEdit ? "▼ Plugins" : (((hookableType == HookableTypes.Plugin) ? "▼ Modules" : "▼ Groups") ?? ""), delegate
				{
					if (pluginEdit)
					{
						ap.SetStorage(tab, "pluginedit", !pluginEdit);
						GenerateHookables(tab, ap, permission, player, selectedGroup, HookableTypes.Plugin);
					}
					else if (hookableType == HookableTypes.Plugin)
					{
						GenerateHookables(tab, ap, permission, player, selectedGroup, HookableTypes.Module);
					}
					else
					{
						ap.SetStorage(tab, "pluginedit", !pluginEdit);
						GenerateHookables(tab, ap, permission, player, selectedGroup, hookableType);
					}
				}, (PlayerSession playerSession) => Tab.OptionButton.Types.None));
			}
			if (groupEdit)
			{
				tab.ClearColumn(3);
				tab.AddName(2, "Groups", (TextAnchor)3);
				tab.AddInput(2, "Search", (PlayerSession playerSession) => playerSession.GetStorage(tab, "groupfilter", string.Empty), delegate(PlayerSession playerSession, object[] args)
				{
					playerSession.SetStorage(tab, "groupfilter", args.Select((object x) => x as string).ToString(" "));
					GenerateHookables(tab, playerSession, permission, player, selectedGroup, hookableType);
				});
				string storage = ap.GetStorage<string>(tab, "groupfilter");
				string[] groups = permission.GetGroups();
				foreach (string group in groups)
				{
					if (!string.IsNullOrEmpty(storage) && !group.Contains(storage))
					{
						continue;
					}
					tab.AddButton(2, group ?? "", delegate(PlayerSession ap2)
					{
						if (permission.UserHasGroup(player.Key, group))
						{
							permission.RemoveUserGroup(player.Key, group);
						}
						else
						{
							permission.AddUserGroup(player.Key, group);
						}
						GenerateHookables(tab, ap2, permission, player, selectedGroup, hookableType);
					}, (PlayerSession _instance) => permission.UserHasGroup(player.Key, group) ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None, (TextAnchor)4);
				}
				return;
			}
			if (!pluginEdit)
			{
				tab.AddName(2, (hookableType == HookableTypes.Module) ? "Modules" : "Plugins", (TextAnchor)3);
				tab.AddInput(2, "Search", (PlayerSession playerSession) => playerSession.GetStorage(tab, "pluginfilter", string.Empty), delegate(PlayerSession playerSession, object[] args)
				{
					playerSession.SetStorage(tab, "pluginfilter", args.Select((object x) => x as string).ToString(" "));
					GenerateHookables(tab, playerSession, permission, player, selectedGroup, hookableType);
				});
				SortTypes sort = (SortTypes)ap.GetStorage(tab, "sorttype", 0);
				bool sortFlip = ap.GetStorage(tab, "sortflip", @default: false);
				tab.AddDropdown(2, "Sorting", (PlayerSession playerSession) => (int)sort, delegate(PlayerSession session, int index)
				{
					if (sort != (SortTypes)index)
					{
						ap.SetStorage(tab, "sortflip", value: false);
						ap.SetStorage(tab, "sorttype", index);
					}
					else
					{
						ap.SetStorage(tab, "sortflip", !sortFlip);
					}
					GenerateHookables(tab, ap, permission, player, selectedGroup, hookableType);
				}, SortTypeNames);
				IEnumerable<BaseHookable> enumerable = ((hookableType == HookableTypes.Plugin) ? (from x in ModLoader.Packages.SelectMany((ModLoader.Package x) => x.Plugins)
					where x.permission.permset.TryGetValue(x, out var value) && ((!string.IsNullOrEmpty(filter)) ? value.Any((string y) => x.Name.Trim().ToLower().Contains(filter)) : (value.Count > 0))
					select x).Select((Func<RustPlugin, BaseHookable>)((RustPlugin x) => x)) : Community.Runtime.ModuleProcessor.Modules.Where((BaseHookable x) => permission.permset.TryGetValue(x, out var value) && ((!string.IsNullOrEmpty(filter)) ? permission.GetPermissions().Any((string y) => x.Name.Trim().ToLower().Contains(filter)) : (value.Count > 0))));
				switch (sort)
				{
				case SortTypes.Name:
					enumerable = enumerable.OrderBy((BaseHookable x) => x.Name);
					break;
				case SortTypes.Version:
					enumerable = enumerable.OrderBy((BaseHookable x) => x.Version.ToString());
					break;
				}
				if (sortFlip)
				{
					enumerable = enumerable.Reverse();
				}
				{
					foreach (BaseHookable plugin in enumerable)
					{
						tab.AddRow(2, new Tab.OptionButton($"{plugin.Name} ({plugin.Version})", delegate(PlayerSession instance3)
						{
							ap.SetStorage(tab, "toggleall", value: true);
							ap.SetStorage(tab, "plugin", plugin);
							ap.SetStorage(tab, "pluginr", instance3.LastPressedRow);
							ap.SetStorage(tab, "pluginc", instance3.LastPressedColumn);
							GeneratePermissions(tab, ap, permission, plugin, player, selectedGroup);
						}, (PlayerSession _instance) => (ap.GetStorage<BaseHookable>(tab, "plugin") == plugin) ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None));
					}
					return;
				}
			}
			tab.AddName(2, "Players", (TextAnchor)3);
			tab.AddInput(2, "Search", (PlayerSession playerSession) => playerSession.GetStorage(tab, "pluginfilter", string.Empty), delegate(PlayerSession playerSession, object[] args)
			{
				playerSession.SetStorage(tab, "pluginfilter", args.Select((object x) => x as string).ToString(" "));
				GenerateHookables(tab, playerSession, permission, player, selectedGroup, hookableType);
			});
			IEnumerable<KeyValuePair<string, UserData>> enumerable2 = permission.userdata.Where(delegate(KeyValuePair<string, UserData> x)
			{
				if (!x.Value.Groups.Contains(selectedGroup))
				{
					return false;
				}
				return string.IsNullOrEmpty(filter) || x.Value.LastSeenNickname.ToLower().StartsWith(filter) || x.Key.Contains(filter);
			});
			foreach (KeyValuePair<string, UserData> user in enumerable2)
			{
				tab.AddRow(2, new Tab.OptionButton(user.Value.LastSeenNickname + " (" + user.Key + ")", delegate
				{
					ap.SetStorage(tab, "toggleall", value: true);
					ap.SetStorage(tab, "groupedit", value: false);
					ap.SetStorage(tab, "pluginedit", value: false);
					tab.ClearColumn(1);
					tab.ClearColumn(2);
					tab.ClearColumn(3);
					ap.Clear();
					ap.SetStorage(tab, "option", 0);
					ap.SetStorage(tab, "player", user.Key);
					GeneratePlayers(tab, permission, ap);
					GenerateHookables(tab, ap, permission, user, null, hookableType);
				}, (PlayerSession _instance) => Tab.OptionButton.Types.Selected));
			}
		}

		public static void GeneratePermissions(Tab tab, PlayerSession ap, Permission perms, BaseHookable hookable, KeyValuePair<string, UserData> player, string selectedGroup)
		{
			bool grantAllStatus = ap.GetStorage(tab, "toggleall", @default: true);
			string text = ap.GetStorage(tab, "permfilter", string.Empty)?.Trim().ToLower();
			tab.ClearColumn(3);
			tab.AddName(3, "Permissions", (TextAnchor)3);
			tab.AddInput(3, "Search", (PlayerSession playerSession) => playerSession.GetStorage(tab, "permfilter", string.Empty), delegate(PlayerSession playerSession, object[] args)
			{
				playerSession.SetStorage(tab, "permfilter", args.Select((object x) => x as string).ToString(" "));
				GeneratePermissions(tab, playerSession, perms, hookable, player, selectedGroup);
			});
			tab.AddButton(3, grantAllStatus ? "Grant All" : "Revoke All", delegate(PlayerSession playerSession)
			{
				string[] permissions2 = perms.GetPermissions(hookable);
				foreach (string perm2 in permissions2)
				{
					if (string.IsNullOrEmpty(selectedGroup))
					{
						if (grantAllStatus)
						{
							if (!perms.UserHasPermission(player.Key, perm2))
							{
								perms.GrantUserPermission(player.Key, perm2, hookable);
							}
						}
						else if (perms.UserHasPermission(player.Key, perm2))
						{
							perms.RevokeUserPermission(player.Key, perm2);
						}
					}
					else if (grantAllStatus)
					{
						if (!perms.GroupHasPermission(selectedGroup, perm2))
						{
							perms.GrantGroupPermission(selectedGroup, perm2, hookable);
						}
					}
					else if (perms.GroupHasPermission(selectedGroup, perm2))
					{
						perms.RevokeGroupPermission(selectedGroup, perm2);
					}
				}
				playerSession.SetStorage(tab, "toggleall", !grantAllStatus);
				GeneratePermissions(tab, playerSession, permission, hookable, player, selectedGroup);
			}, (PlayerSession playerSession) => (!grantAllStatus) ? Tab.OptionButton.Types.Important : Tab.OptionButton.Types.Warned, (TextAnchor)4);
			string[] permissions = perms.GetPermissions(hookable);
			foreach (string perm in permissions)
			{
				if (!string.IsNullOrEmpty(text) && !StringEx.Contains(perm, text, CompareOptions.OrdinalIgnoreCase))
				{
					continue;
				}
				if (string.IsNullOrEmpty(selectedGroup))
				{
					bool isInherited = false;
					string text2 = "";
					string[] userGroups = perms.GetUserGroups(player.Key);
					foreach (string text3 in userGroups)
					{
						if (perms.GroupHasPermission(text3, perm))
						{
							isInherited = true;
							text2 = text2 + "<b>" + text3 + "</b>, ";
						}
					}
					tab.AddRow(3, new Tab.OptionButton(perm ?? "", delegate
					{
						if (perms.UserHasPermission(player.Key, perm))
						{
							perms.RevokeUserPermission(player.Key, perm);
						}
						else
						{
							perms.GrantUserPermission(player.Key, perm, hookable);
						}
					}, (PlayerSession _instance) => (!isInherited) ? (perms.UserHasPermission(player.Key, perm) ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None) : Tab.OptionButton.Types.Important));
					if (isInherited)
					{
						tab.AddText(3, "Inherited by the following groups: " + text2.TrimEnd(new char[2] { ',', ' ' }), 8, "1 1 1 0.6", (TextAnchor)0);
					}
					continue;
				}
				tab.AddRow(3, new Tab.OptionButton(perm ?? "", delegate
				{
					if (permission.GroupHasPermission(selectedGroup, perm))
					{
						permission.RevokeGroupPermission(selectedGroup, perm);
					}
					else
					{
						permission.GrantGroupPermission(selectedGroup, perm, hookable);
					}
				}, (PlayerSession _instance) => permission.GroupHasPermission(selectedGroup, perm) ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None));
			}
		}

		public static void GenerateGroups(Tab tab, Permission perms, PlayerSession ap)
		{
			tab.ClearColumn(1);
			tab.AddName(1, "Groups", (TextAnchor)3);
			tab.AddInput(1, "Search", (PlayerSession playerSession) => playerSession.GetStorage(tab, "groupfilter", string.Empty), delegate(PlayerSession playerSession, object[] args)
			{
				playerSession.SetStorage(tab, "groupfilter", args.Select((object x) => x as string).ToString(" "));
				GenerateGroups(tab, perms, playerSession);
			});
			string storage = ap.GetStorage<string>(tab, "groupfilter");
			tab.AddButton(1, "Add Group", delegate(PlayerSession playerSession)
			{
				List<string> list = Pool.Get<List<string>>();
				string[] groups = Community.Runtime.Core.permission.GetGroups();
				list.Add("None");
				list.AddRange(groups);
				string[] array = list.ToArray();
				Pool.FreeUnmanaged<string>(ref list);
				Singleton.Modal.Open(playerSession.Player, "Create Group", new Dictionary<string, ModalModule.Modal.Field>
				{
					["name"] = ModalModule.Modal.Field.Make("Name", ModalModule.Modal.Field.FieldTypes.String, required: true, null, isReadOnly: false, (ModalModule.Modal.Field field) => (!perms.GetGroups().Any((string x) => x == field.Get<string>())) ? null : "Group with that name already exists."),
					["dname"] = ModalModule.Modal.Field.Make("Display Name", ModalModule.Modal.Field.FieldTypes.String, required: false, string.Empty),
					["rank"] = ModalModule.Modal.Field.Make("Rank", ModalModule.Modal.Field.FieldTypes.Integer, required: false, 0),
					["parent"] = ModalModule.Modal.EnumField.MakeEnum("Parent", array, required: false, 0)
				}, delegate(BasePlayer player, ModalModule.Modal modal)
				{
					int num = modal.Get<int>("parent");
					perms.CreateGroup(modal.Get<string>("name"), modal.Get<string>("dname"), modal.Get<int>("rank"));
					if (num != 0)
					{
						perms.SetGroupParent(modal.Get<string>("name"), array[num]);
					}
					tab.ClearColumn(1);
					tab.ClearColumn(2);
					tab.ClearColumn(3);
					GenerateGroups(tab, perms, playerSession);
					Singleton.NextFrame(delegate
					{
						Singleton.Draw(playerSession.Player);
					});
				});
			}, (PlayerSession _instance) => Tab.OptionButton.Types.Warned, (TextAnchor)4);
			foreach (string group in from x in permission.GetGroups()
				orderby permission.GetGroupData(x).Rank
				select x)
			{
				if (string.IsNullOrEmpty(storage) || group.Contains(storage))
				{
					GroupData groupData = permission.GetGroupData(group);
					tab.AddButton(1, string.IsNullOrEmpty(groupData.Title) ? (group ?? "") : (groupData.Title + " (" + group + ")"), delegate
					{
						ap.SetStorage(tab, "group", group);
						ap.ClearStorage(tab, "plugin");
						tab.ClearColumn(2);
						tab.ClearColumn(3);
						GenerateHookables(tab, ap, permission, permission.FindUser(ap.Player.UserIDString), group, HookableTypes.Plugin);
					}, (PlayerSession _instance) => (ap.GetStorage(tab, "group", string.Empty) == group) ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None, (TextAnchor)4);
				}
			}
		}
	}

	public class PlayersTab
	{
		public static readonly List<BasePlayer> BlindedPlayers = new List<BasePlayer>();

		public static Tab Get()
		{
			Tab tab = new Tab("players", "Players", Community.Runtime.Core, delegate(PlayerSession instance, Tab tab2)
			{
				tab2.ClearColumn(1);
				RefreshPlayers(tab2, instance);
			}, "players.use");
			tab.AddColumn(0);
			tab.AddColumn(1);
			return tab;
		}

		public static void RefreshPlayers(Tab tab, PlayerSession ap)
		{
			tab.ClearColumn(0);
			tab.AddInput(0, "Search", (PlayerSession playerSession) => playerSession?.GetStorage<string>(tab, "playerfilter"), delegate(PlayerSession playerSession, object[] args)
			{
				playerSession.SetStorage(tab, "playerfilter", args.Select((object x) => x as string).ToString(" "));
				RefreshPlayers(tab, playerSession);
			});
			IOrderedEnumerable<BasePlayer> orderedEnumerable = from x in BasePlayer.allPlayerList.Distinct()
				where x.userID.IsSteamId() && x.IsConnected
				orderby x.Connection?.connectionTime
				select x;
			tab.AddName(0, $"Online ({orderedEnumerable.Count():n0})", (TextAnchor)3);
			foreach (BasePlayer item in orderedEnumerable)
			{
				AddPlayer(tab, ap, item);
			}
			if (orderedEnumerable.Count() == 0)
			{
				tab.AddText(0, "No online players found.", 10, "1 1 1 0.4", (TextAnchor)4);
			}
			IEnumerable<BasePlayer> enumerable = from x in BasePlayer.allPlayerList.Distinct()
				where x.userID.IsSteamId() && !x.IsConnected
				select x;
			tab.AddName(0, $"Offline ({enumerable.Count():n0})", (TextAnchor)3);
			foreach (BasePlayer item2 in enumerable)
			{
				AddPlayer(tab, ap, item2);
			}
			if (enumerable.Count() == 0)
			{
				tab.AddText(0, "No offline players found.", 10, "1 1 1 0.4", (TextAnchor)4);
			}
		}

		public static void AddPlayer(Tab tab, PlayerSession ap, BasePlayer player)
		{
			if (ap != null)
			{
				string storage = ap.GetStorage<string>(tab, "playerfilter");
				if (!string.IsNullOrEmpty(storage) && !player.displayName.ToLower().Contains(storage.ToLower()) && !player.UserIDString.Contains(storage))
				{
					return;
				}
			}
			tab.AddButton(0, player.displayName ?? "", delegate
			{
				ap.SetStorage<BasePlayer>(tab, "playerfilterpl", player);
				ShowInfo(1, tab, ap, player);
			}, (PlayerSession aap) => (aap != null && (Object)(object)aap.GetStorage<BasePlayer>(tab, "playerfilterpl") == (Object)(object)player) ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None, (TextAnchor)4);
		}

		public static void ShowInfo(int column, Tab tab, PlayerSession aap, BasePlayer player)
		{
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			tab.ClearColumn(column);
			if (column != 1)
			{
				tab.AddButton(column, "<", delegate(PlayerSession ap)
				{
					RefreshPlayers(tab, ap);
					ShowInfo(1, tab, ap, player);
				}, null, (TextAnchor)4);
			}
			tab.AddName(column, "Player Information", (TextAnchor)3);
			tab.AddInput(column, "Name", (PlayerSession _) => player.displayName, delegate(PlayerSession _, object[] args)
			{
				player.AsIPlayer().Rename(args.Select((object x) => x as string).ToString(" "));
			});
			tab.AddInput(column, "Steam ID", (PlayerSession _) => player.UserIDString);
			tab.AddInput(column, "Net ID", (PlayerSession _) => $"{((BaseNetworkable)player).net?.ID}");
			if (Singleton.HasAccess(aap.Player, "players.see_ips"))
			{
				tab.AddInput(column, "IP", delegate
				{
					Networkable net = ((BaseNetworkable)player).net;
					object obj2;
					if (net == null)
					{
						obj2 = null;
					}
					else
					{
						Connection connection = net.connection;
						obj2 = ((connection != null) ? connection.ipaddress : null);
					}
					if (obj2 == null)
					{
						obj2 = "";
					}
					return (string)obj2;
				}, null, null, hidden: true);
			}
			try
			{
				Vector3 position = ((Component)player).transform.position;
				tab.AddInput(column, "Position", (PlayerSession _) => $"{position} [{MapHelper.PositionToGrid(position)}]");
			}
			catch
			{
			}
			tab.AddButton(column, "Player Flags", delegate(PlayerSession ap)
			{
				ShowInfo(0, tab, ap, player);
				PlayerFlags(1, tab, player);
			}, null, (TextAnchor)4);
			if (Singleton.HasAccess(aap.Player, "permissions.use"))
			{
				tab.AddName(column, "Permissions", (TextAnchor)3);
				tab.AddButton(column, "View Permissions", delegate(PlayerSession ap)
				{
					Tab tab2 = Singleton.FindTab("permissions");
					Permission permission = Community.Runtime.Core.permission;
					Singleton.SetTab(ap.Player, "permissions");
					ap.SetStorage(tab, "player", player.UserIDString);
					PermissionsTab.GeneratePlayers(tab2, permission, ap);
					PermissionsTab.GenerateHookables(tab2, ap, permission, permission.FindUser(player.UserIDString), null, PermissionsTab.HookableTypes.Plugin);
				}, (PlayerSession _) => Tab.OptionButton.Types.Important, (TextAnchor)4);
			}
			if (aap.Player.IsAdmin || Singleton.Permissions.UserHasPermission(aap.Player.UserIDString, "carbon.cmod"))
			{
				tab.AddButtonArray(column, new Tab.OptionButton("Kick", delegate
				{
					Singleton.Modal.Open(aap.Player, "Kick " + player.displayName, new Dictionary<string, ModalModule.Modal.Field> { ["reason"] = ModalModule.Modal.Field.Make("Reason", ModalModule.Modal.Field.FieldTypes.String, required: false, "Stop doing that.") }, delegate(BasePlayer val, ModalModule.Modal m)
					{
						player.Kick(m.Get<string>("reason"), true);
					});
				}), new Tab.OptionButton("Ban", delegate(PlayerSession ap)
				{
					Singleton.Modal.Open(aap.Player, "Ban " + player.displayName, new Dictionary<string, ModalModule.Modal.Field>
					{
						["reason"] = ModalModule.Modal.Field.Make("Reason", ModalModule.Modal.Field.FieldTypes.String, required: false, "Stop doing that."),
						["until"] = ModalModule.Modal.ButtonField.MakeButton("Until", "Select Date", delegate
						{
							Core.NextTick(delegate
							{
								Singleton.DatePicker.Draw(ap.Player, delegate(DateTime date)
								{
									ap.SetStorage(tab, "date", date);
								});
							});
						})
					}, delegate(BasePlayer _, ModalModule.Modal m)
					{
						DateTime storage = ap.GetStorage(tab, "date", DateTime.UtcNow.AddYears(100));
						DateTime utcNow = DateTime.UtcNow;
						storage = new DateTime(storage.Year, storage.Month, storage.Day, utcNow.Hour, utcNow.Minute, utcNow.Second, DateTimeKind.Utc);
						TimeSpan duration = utcNow - storage;
						player.AsIPlayer().Ban(m.Get<string>("reason"), duration);
					});
				}), new Tab.OptionButton(player.IsSleeping() ? "End Sleep" : "Sleep", delegate(PlayerSession ap)
				{
					if (player.IsSleeping())
					{
						player.EndSleeping();
					}
					else
					{
						player.StartSleeping();
					}
					ShowInfo(column, tab, ap, player);
				}), new Tab.OptionButton("Hostility", delegate(PlayerSession ap)
				{
					Dictionary<string, ModalModule.Modal.Field> fields = new Dictionary<string, ModalModule.Modal.Field> { ["duration"] = ModalModule.Modal.Field.Make("Duration", ModalModule.Modal.Field.FieldTypes.Float, required: true, 60f) };
					Singleton.Modal.Open(ap.Player, "Player Hostile", fields, delegate(BasePlayer val, ModalModule.Modal modal)
					{
						//IL_0063: Unknown result type (might be due to invalid IL or missing references)
						float num = modal.Get<float>("duration").Clamp(0f, float.MaxValue);
						player.State.unHostileTimestamp = TimeEx.currentTimestamp + (double)num;
						player.DirtyPlayerState();
						((BaseEntity)player).ClientRPC(RpcTarget.Player("SetHostileLength", player), num);
						fields.Clear();
						fields = null;
						ShowInfo(column, tab, aap, player);
						Singleton.Draw(aap.Player);
					}, delegate
					{
						fields.Clear();
						fields = null;
					});
				}));
			}
			else
			{
				tab.AddText(column, "You need 'carbon.cmod' permission to kick, ban, sleep or change player hostility", 10, "1 1 1 0.4", (TextAnchor)4);
			}
			tab.AddName(column, "Actions", (TextAnchor)3);
			if (Singleton.HasAccess(aap.Player, "entities.tp_entity"))
			{
				tab.AddButtonArray(column, new Tab.OptionButton("TeleportTo", delegate(PlayerSession ap)
				{
					//IL_0011: Unknown result type (might be due to invalid IL or missing references)
					ap.Player.Teleport(((Component)player).transform.position);
				}), new Tab.OptionButton("Teleport2Me", delegate
				{
					tab.CreateDialog("Are you sure about that?", delegate(PlayerSession ap)
					{
						//IL_0011: Unknown result type (might be due to invalid IL or missing references)
						player.Teleport(((Component)ap.Player).transform.position);
					});
				}), new Tab.OptionButton("Teleport2OwnedItem", delegate(PlayerSession ap)
				{
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					//IL_0037: Unknown result type (might be due to invalid IL or missing references)
					BaseEntity[] array = Util.FindTargetsOwnedBy(EncryptedValue<ulong>.op_Implicit(player.userID), string.Empty);
					if (array.Length != 0)
					{
						BaseEntity val = array[RandomEx.GetRandomInteger(0, array.Length)];
						ap.Player.Teleport(((Component)val).transform.position);
					}
					else
					{
						Logger.Warn($" No entities owned by {player} could be found to teleport to.");
					}
				}));
			}
			if (Singleton.HasAccess(aap.Player, "entities.loot_players"))
			{
				tab.AddButtonArray(column, new Tab.OptionButton("Loot", delegate(PlayerSession ap)
				{
					OpenPlayerContainer(ap, player, tab);
				}), new Tab.OptionButton("Strip", delegate
				{
					player.inventory.Strip();
				}), new Tab.OptionButton("Respawn", delegate
				{
					tab.CreateDialog("Are you sure about that?", delegate
					{
						((BaseCombatEntity)player).Hurt(((BaseEntity)player).MaxHealth());
						player.Respawn();
						player.EndSleeping();
					});
				}));
				tab.AddText(column, "To loot a backpack, drag the backpack item over any hotbar slots while looting a player", 10, "1 1 1 0.4", (TextAnchor)4);
			}
			if (Singleton.HasAccess(aap.Player, "players.inventory_management"))
			{
				tab.AddName(column, "Inventory Lock", (TextAnchor)3);
				tab.AddButtonArray(column, new Tab.OptionButton("Main", delegate
				{
					LockPlayerContainer(aap.Player, player, player.inventory.containerMain, !player.inventory.containerMain.IsLocked());
				}, (PlayerSession _) => player.inventory.containerMain.IsLocked() ? Tab.OptionButton.Types.Important : Tab.OptionButton.Types.None), new Tab.OptionButton("Belt", delegate
				{
					LockPlayerContainer(aap.Player, player, player.inventory.containerBelt, !player.inventory.containerBelt.IsLocked());
				}, (PlayerSession _) => player.inventory.containerBelt.IsLocked() ? Tab.OptionButton.Types.Important : Tab.OptionButton.Types.None), new Tab.OptionButton("Wear", delegate
				{
					LockPlayerContainer(aap.Player, player, player.inventory.containerWear, !player.inventory.containerWear.IsLocked());
				}, (PlayerSession _) => player.inventory.containerWear.IsLocked() ? Tab.OptionButton.Types.Important : Tab.OptionButton.Types.None));
			}
			if (Singleton.HasTab("entities"))
			{
				tab.AddButton(column, "Select Entity", delegate(PlayerSession ap2)
				{
					Singleton.SetTab(ap2.Player, "entities");
					Tab tab2 = Singleton.GetTab(ap2.Player);
					EntitiesTab.SelectEntity(tab2, ap2, (BaseEntity)(object)player);
					EntitiesTab.DrawEntities(tab2, ap2);
					EntitiesTab.DrawEntitySettings(tab2, 1, ap2);
				}, null, (TextAnchor)4);
			}
			if (Singleton.HasAccess(aap.Player, "entities.blind_players"))
			{
				if (!BlindedPlayers.Contains(player))
				{
					tab.AddButton(column, "Blind Player", delegate
					{
						tab.CreateDialog("Are you sure you want to blind the player?", delegate(PlayerSession ap)
						{
							BlindPlayer(aap.Player, player);
							ShowInfo(column, tab, ap, player);
							if ((Object)(object)ap.Player == (Object)(object)player)
							{
								Core.timer.In(1f, delegate
								{
									Singleton.Close(player);
								});
							}
						});
					}, null, (TextAnchor)4);
				}
				else
				{
					tab.AddButton(column, "Unblind Player", delegate(PlayerSession ap)
					{
						UnblindPlayer(aap.Player, player);
						ShowInfo(column, tab, ap, player);
					}, (PlayerSession _) => Tab.OptionButton.Types.Selected, (TextAnchor)4);
				}
			}
			tab.AddName(column, "Stats", (TextAnchor)3);
			tab.AddName(column, "Combat", (TextAnchor)3);
			tab.AddRange(column, "Health", 0f, ((BaseEntity)player).MaxHealth(), (PlayerSession _) => ((BaseCombatEntity)player).health, delegate(PlayerSession _, float value)
			{
				((BaseCombatEntity)player).SetHealth(value);
			}, (PlayerSession _) => $"{((BaseCombatEntity)player).health:0}");
			tab.AddRange(column, "Thirst", 0f, ((BaseMetabolism<BasePlayer>)(object)player.metabolism).hydration.max, (PlayerSession _) => ((BaseMetabolism<BasePlayer>)(object)player.metabolism).hydration.value, delegate(PlayerSession _, float value)
			{
				((BaseMetabolism<BasePlayer>)(object)player.metabolism).hydration.SetValue(value);
			}, (PlayerSession _) => $"{((BaseMetabolism<BasePlayer>)(object)player.metabolism).hydration.value:0}");
			tab.AddRange(column, "Hunger", 0f, ((BaseMetabolism<BasePlayer>)(object)player.metabolism).calories.max, (PlayerSession _) => ((BaseMetabolism<BasePlayer>)(object)player.metabolism).calories.value, delegate(PlayerSession _, float value)
			{
				((BaseMetabolism<BasePlayer>)(object)player.metabolism).calories.SetValue(value);
			}, (PlayerSession _) => $"{((BaseMetabolism<BasePlayer>)(object)player.metabolism).calories.value:0}");
			tab.AddRange(column, "Radiation", 0f, player.metabolism.radiation_poison.max, (PlayerSession _) => player.metabolism.radiation_poison.value, delegate(PlayerSession _, float value)
			{
				player.metabolism.radiation_poison.SetValue(value);
			}, (PlayerSession _) => $"{player.metabolism.radiation_poison.value:0}");
			tab.AddRange(column, "Bleeding", 0f, player.metabolism.bleeding.max, (PlayerSession _) => player.metabolism.bleeding.value, delegate(PlayerSession _, float value)
			{
				player.metabolism.bleeding.SetValue(value);
			}, (PlayerSession _) => $"{player.metabolism.bleeding.value:0}");
			tab.AddRange(column, "Wetness", 0f, player.metabolism.wetness.max * 10f, (PlayerSession ap) => player.metabolism.wetness.value * 10f, delegate(PlayerSession _, float value)
			{
				player.metabolism.wetness.SetValue(value * 0.1f);
			}, (PlayerSession _) => $"{player.metabolism.wetness.value * 100f:0}%");
			tab.AddButton(column, "Empower Stats", delegate
			{
				EmpowerPlayerStats(aap.Player, player);
			}, null, (TextAnchor)4);
			if (!Singleton.HasAccess(aap.Player, "players.craft_queue"))
			{
				return;
			}
			tab.AddName(column, "Crafting", (TextAnchor)3);
			IEnumerable<ItemCraftTask> enumerable = player.inventory.crafting.queue.Where((ItemCraftTask x) => !x.cancelled);
			foreach (ItemCraftTask craft in enumerable)
			{
				tab.AddInputButton(column, $"{craft.blueprint.targetItem.displayName.english} (x{craft.amount}, {TimeEx.Format(craft.endTime - Time.realtimeSinceStartup)})", 0.1f, new Tab.OptionInput(null, (PlayerSession _) => "<size=8>" + craft.takenItems.Select((Item x) => $"{x.info.displayName.english} x {x.amount}").ToString(", ") + "</size>", 0, readOnly: true, null), new Tab.OptionButton("X", (TextAnchor)4, delegate(PlayerSession ap)
				{
					player.inventory.crafting.CancelTask(craft.taskUID);
					ShowInfo(column, tab, ap, player);
				}, (PlayerSession _) => Tab.OptionButton.Types.Important));
			}
			if (!enumerable.Any())
			{
				tab.AddText(column, "No crafts.", 8, "1 1 1 0.5", (TextAnchor)4);
			}
		}

		public static void PlayerFlags(int column, Tab tab, BasePlayer player)
		{
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			tab.ClearColumn(column);
			int num = 0;
			List<Tab.OptionButton> list = Pool.Get<List<Tab.OptionButton>>();
			tab.ClearColumn(column);
			tab.AddName(column, "Player Flags", (TextAnchor)3);
			foreach (string item in from x in Enum.GetNames(typeof(PlayerFlags))
				orderby x
				select x)
			{
				PlayerFlags flagValue = (PlayerFlags)Enum.Parse(typeof(PlayerFlags), item);
				bool hasFlag = player.HasPlayerFlag(flagValue);
				list.Add(new Tab.OptionButton(item, delegate(PlayerSession ap)
				{
					//IL_000c: Unknown result type (might be due to invalid IL or missing references)
					player.SetPlayerFlag(flagValue, !hasFlag);
					ShowInfo(0, tab, ap, player);
					PlayerFlags(column, tab, player);
				}, (PlayerSession ap) => hasFlag ? Tab.OptionButton.Types.Selected : Tab.OptionButton.Types.None));
				num++;
				if (num >= 5)
				{
					tab.AddButtonArray(column, list.ToArray());
					list.Clear();
					num = 0;
				}
			}
			Pool.FreeUnmanaged<Tab.OptionButton>(ref list);
		}
	}

	public static class PluginsTab
	{
		public enum VendorTypes
		{
			Installed,
			Codefling,
			uMod
		}

		public enum FilterTypes
		{
			None,
			Price,
			Author,
			Installed,
			OutOfDate,
			Favourites,
			Owned
		}

		[ProtoContract]
		[ProtoInclude(100, typeof(Codefling))]
		[ProtoInclude(101, typeof(uMod))]
		public abstract class Vendor
		{
			public IEnumerable<Plugin> PriceData;

			public IEnumerable<Plugin> AuthorData;

			public IEnumerable<Plugin> InstalledData;

			public IEnumerable<Plugin> OutOfDateData;

			public IEnumerable<Plugin> OwnedData;

			[ProtoMember(1)]
			public List<Plugin> FetchedPlugins = new List<Plugin>();

			[ProtoMember(2)]
			public long LastTick;

			public virtual string Type { get; }

			public virtual string Url { get; }

			public virtual string Logo { get; }

			public virtual float LogoRatio { get; }

			public virtual string Hero { get; }

			public virtual string Tagline { get; }

			public virtual bool CanRefresh { get; } = true;

			public virtual string BarInfo { get; }

			public virtual string ListEndpoint { get; }

			public virtual string DownloadEndpoint { get; }

			public virtual string PluginLookupEndpoint { get; }

			public abstract void Refresh();

			public abstract void FetchList(Action<Vendor> callback = null);

			public abstract void Download(string id, Action onTimeout = null);

			public abstract void Uninstall(string id);

			public abstract void CheckMetadata(string id, Action onMetadataRetrieved);

			public virtual void VersionCheck()
			{
				foreach (Plugin fetchedPlugin in FetchedPlugins)
				{
					if (fetchedPlugin.IsInstalled() && !fetchedPlugin.IsUpToDate())
					{
						HookCaller.CallStaticHook(3156772569u, fetchedPlugin.Name, new VersionNumber(fetchedPlugin.CurrentVersion()), new VersionNumber(fetchedPlugin.Version), fetchedPlugin.ExistentPlugin, Type);
					}
				}
			}

			public override string ToString()
			{
				return Type + " Vendor";
			}
		}

		public interface IVendorStored
		{
			bool Load();

			void Save();
		}

		public interface IVendorAuthenticated
		{
			string AuthCode { get; set; }

			string AuthRequestEndpoint { get; }

			string AuthRequestEndpointPreview { get; }

			string AuthValidationEndpoint { get; }

			string AuthUserInfoEndpoint { get; }

			string AuthOwnedPluginsEndpoint { get; }

			string AuthDownloadFileEndpoint { get; }

			KeyValuePair<HttpRequestHeader, string> AuthHeader { get; }

			float AuthValidationCheckRate { get; }

			Oxide.Plugins.Timer ValidationTimer { get; set; }

			LoggedInUser User { get; set; }

			bool IsLoggedIn { get; }

			void Validate(PlayerSession session, Action onCompletion);

			void RefreshUser(PlayerSession session);
		}

		[ProtoContract]
		public class LoggedInUser
		{
			public enum RequestResult
			{
				None,
				Processing,
				Complete
			}

			[ProtoMember(1)]
			public int Id;

			[ProtoMember(2)]
			public string Authority;

			[ProtoMember(3)]
			public string DisplayName;

			[ProtoMember(4)]
			public string AvatarUrl;

			[ProtoMember(5)]
			public string AccessTokenEncoded;

			[ProtoMember(6)]
			public string CoverUrl;

			[ProtoMember(500)]
			public bool PendingAccessToken;

			[ProtoMember(501)]
			public RequestResult PendingResult;

			[ProtoMember(502)]
			public bool IsAdmin;

			public string AccessToken;

			[ProtoMember(600)]
			public List<string> OwnedFiles { get; } = new List<string>();
		}

		public enum Status
		{
			Pending = 1,
			Approved = 0,
			Hidden = -1,
			Deleted = -2
		}

		[ProtoContract]
		public class Codefling : Vendor, IVendorStored, IVendorAuthenticated
		{
			private Dictionary<string, string> _headers = new Dictionary<string, string>();

			private static readonly string _backSlashes = "\\";

			public override string Type => "Codefling";

			public override string Url => "https://codefling.com";

			public override string Logo => "cflogo";

			public override float LogoRatio => 0f;

			public override string Hero => "cf_hero";

			public override string Tagline => "The largest marketplace for Rust community-driven content.";

			public override string BarInfo => $"{FetchedPlugins.Count((Plugin x) => !x.IsPaid()):n0} free, {FetchedPlugins.Count((Plugin x) => x.IsPaid()):n0} paid";

			public override string ListEndpoint => "https://codefling.com/db/?category=2,21";

			public override string DownloadEndpoint => "https://codefling.com/files/file/[ID]-a?do=download";

			[ProtoMember(50, IsRequired = false)]
			public LoggedInUser User { get; set; }

			public string AuthRequestEndpoint => "https://codefling.com/auth/?pin={0}";

			public string AuthRequestEndpointPreview => "codefling.com/auth";

			public string AuthValidationEndpoint => "https://codefling.com/auth/bearer?code={0}";

			public string AuthUserInfoEndpoint => "https://codefling.com/api/core/me";

			public string AuthOwnedPluginsEndpoint => "https://codefling.com/api/nexus/purchases?perPage=100000&itemType=file&itemApp=downloads";

			public string AuthDownloadFileEndpoint => "https://codefling.com/api/downloads/files/{0}/download";

			public KeyValuePair<HttpRequestHeader, string> AuthHeader => new KeyValuePair<HttpRequestHeader, string>(HttpRequestHeader.Authorization, "Bearer {0}");

			public float AuthValidationCheckRate => 5f;

			public Oxide.Plugins.Timer ValidationTimer { get; set; }

			public string AuthCode { get; set; }

			public bool IsLoggedIn => User != null;

			public override void Refresh()
			{
				if (FetchedPlugins == null)
				{
					return;
				}
				List<RustPlugin> list = Pool.Get<List<RustPlugin>>();
				Community.Runtime.Core.plugins.GetAllNonAlloc(list);
				foreach (Plugin fetchedPlugin in FetchedPlugins)
				{
					try
					{
						string fileName = Path.GetFileName(fetchedPlugin.File);
						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fetchedPlugin.File);
						fetchedPlugin.SetOwned(((IVendorAuthenticated)this).User != null && (((IVendorAuthenticated)this).User.IsAdmin || ((IVendorAuthenticated)this).User.OwnedFiles.Contains(fetchedPlugin.Id)));
						foreach (RustPlugin item in list)
						{
							if ((!string.IsNullOrEmpty(item.FileName) && (item.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase) || item.FileName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase))) || (!string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(fetchedPlugin.Name) && item.Name.Equals(fetchedPlugin.Name, StringComparison.OrdinalIgnoreCase)))
							{
								fetchedPlugin.SetExistentPlugin(item);
								break;
							}
						}
					}
					catch (Exception ex)
					{
						Logger.Warn(fetchedPlugin.File + " (" + ex.Message + ")\n" + ex.StackTrace);
					}
				}
				Pool.FreeUnmanaged<RustPlugin>(ref list);
				PriceData = from x in FetchedPlugins
					where x.Status == Status.Approved
					orderby x.OriginalPrice.ToFloat()
					select x;
				AuthorData = from x in FetchedPlugins
					where x.Status == Status.Approved
					orderby x.Author
					select x;
				InstalledData = FetchedPlugins.Where((Plugin x) => x.IsInstalled());
				OutOfDateData = from x in FetchedPlugins
					where x.Status == Status.Approved
					where x.IsInstalled() && !x.IsUpToDate()
					select x;
				OwnedData = FetchedPlugins.Where((Plugin x) => x.Owned);
			}

			public override void FetchList(Action<Vendor> callback = null)
			{
				Community.Runtime.Core.webrequest.Enqueue(ListEndpoint, null, delegate(int error, string data)
				{
					if (error != 200)
					{
						Logger.Log($"[{Type}] Failed fetching vendor. Error code {error}!");
					}
					else
					{
						FetchedPlugins.Clear();
						List<RustPlugin> list = Pool.Get<List<RustPlugin>>();
						Community.Runtime.Core.plugins.GetAllNonAlloc(list);
						ParseData(data, doSave: false, insert: false, FetchedPlugins, callback, this, list);
						Pool.FreeUnmanaged<RustPlugin>(ref list);
						VersionCheck();
					}
				}, Community.Runtime.Core);
				static void ParseData(string data, bool doSave, bool insert, List<Plugin> fetchedPlugins, Action<Vendor> action, Vendor vendor, List<RustPlugin> plugins)
				{
					try
					{
						JArray val = JArray.Parse(data);
						foreach (JToken item in val)
						{
							JToken val2 = item[(object)"prices"];
							Plugin plugin = new Plugin
							{
								Id = ((object)item[(object)"id"])?.ToString(),
								Name = ((object)item[(object)"title"])?.ToString(),
								Author = ((object)item[(object)"author"])?.ToString(),
								Description = ((object)item[(object)"description"])?.ToString().Replace(_backSlashes, string.Empty),
								Version = ((object)item[(object)"version"])?.ToString(),
								OriginalPrice = ((val2 == null || !val2.HasValues) ? "FREE" : ((object)val2[(object)"USD"])?.ToString()),
								Date = ((object)item[(object)"date"])?.ToString(),
								UpdateDate = ((object)item[(object)"updated"])?.ToString(),
								Changelog = ((object)item[(object)"changelog"])?.ToString().Replace(_backSlashes, string.Empty),
								File = ((object)item[(object)"fileName"])?.ToString(),
								Image = string.Format("https://codefling.com/cdn-cgi/image/width=1250,height=1250,quality=100,blur=25,fit=cover,format=jpeg/{0}", item[(object)"primaryScreenshot"]),
								ImageThumbnail = string.Format("https://codefling.com/cdn-cgi/image/width=246,height=246,quality=75,fit=cover,format=jpeg/{0}", item[(object)"primaryScreenshot"]),
								Tags = ((IEnumerable<JToken>)item[(object)"tags"])?.Select((JToken x) => ((object)x).ToString()),
								DownloadCount = (((object)item[(object)"downloads"])?.ToString().ToInt()).GetValueOrDefault(),
								CarbonCompatible = (((object)item[(object)"compatibility"])?.ToString().ToBool() == true),
								Rating = (((object)item[(object)"rating"])?.ToString().ToFloat() ?? 0f),
								Status = Status.Approved,
								HasLookup = true
							};
							DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(plugin.UpdateDate.ToLong(0L));
							DateTimeOffset dateTimeOffset2 = DateTimeOffset.FromUnixTimeSeconds(plugin.Date.ToLong(0L));
							plugin.UpdateDate = dateTimeOffset.UtcDateTime.ToString();
							plugin.Date = dateTimeOffset2.UtcDateTime.ToString();
							plugin.PreferredVendor = VendorTypes.Codefling;
							try
							{
								plugin.Description = plugin.Description.TrimStart('\t').Replace("\t", "\n").Split('\n')[0];
							}
							catch
							{
							}
							if (plugin.OriginalPrice == "{}")
							{
								plugin.OriginalPrice = "FREE";
							}
							try
							{
								plugin.ExistentPlugin = plugins.FirstOrDefault((RustPlugin x) => Path.GetFileNameWithoutExtension(x.FilePath) == Path.GetFileNameWithoutExtension(plugin.File));
							}
							catch
							{
							}
							if (insert)
							{
								fetchedPlugins.Insert(0, plugin);
							}
							else
							{
								fetchedPlugins.Add(plugin);
							}
						}
						if (doSave)
						{
							action?.Invoke(vendor);
							Logger.Log($"[{vendor} Tab] Fetched latest plugin information.");
							if (vendor is IVendorStored vendorStored)
							{
								vendorStored.Save();
							}
						}
					}
					catch (Exception ex)
					{
						Logger.Error(" Couldn't fetch Codefling API to get the plugins list. Most likely because it's down.", ex);
					}
				}
			}

			public override void Download(string id, Action onTimeout = null)
			{
				Plugin plugin = FetchedPlugins.FirstOrDefault((Plugin x) => x.Id.Equals(id, StringComparison.CurrentCultureIgnoreCase) || x.Name.Equals(id, StringComparison.CurrentCultureIgnoreCase) || Path.GetFileNameWithoutExtension(x.File).Equals(id, StringComparison.CurrentCultureIgnoreCase));
				if (plugin == null)
				{
					Logger.Error("Couldn't find '" + id + "' on " + Type);
				}
				plugin.IsBusy = true;
				plugin.DownloadCount++;
				CorePlugin core = Community.Runtime.Core;
				core.timer.In(2f, delegate
				{
					if (plugin.IsBusy)
					{
						plugin.IsBusy = false;
						onTimeout?.Invoke();
					}
				});
				if (IsLoggedIn)
				{
					string extension = Path.GetExtension(plugin.File);
					if (!(extension == ".zip"))
					{
						if (!(extension == ".cs"))
						{
							return;
						}
						core.webrequest.Enqueue(string.Format(AuthDownloadFileEndpoint, plugin.Id), null, delegate(int error, string source)
						{
							if (error != 200)
							{
								Logger.Error("Auth token for Codefling is expired! Please log in once again.");
								User = null;
							}
							else
							{
								JObject val = JObject.Parse(source);
								string url2 = ((object)val["files"][(object)0][(object)"url"]).ToString();
								string path2 = ((plugin.ExistentPlugin == null) ? Path.Combine(Defines.GetScriptsFolder(), plugin.File) : plugin.ExistentPlugin.FilePath);
								core.webrequest.Enqueue(url2, null, delegate(int _, string content)
								{
									plugin.IsBusy = false;
									Singleton.Puts("Downloaded " + plugin.Name);
									OsEx.File.Move(path2, Path.Combine(Defines.GetScriptsFolder(), "backups", plugin.File));
									OsEx.File.Create(path2, content);
								}, core, RequestMethod.GET, _headers);
							}
						}, core, RequestMethod.GET, _headers);
						return;
					}
					core.webrequest.Enqueue(string.Format(AuthDownloadFileEndpoint, plugin.Id), null, delegate(int error, string source)
					{
						if (error != 200)
						{
							Logger.Error("Auth token for Codefling is expired! Please log in once again.");
							User = null;
						}
						else
						{
							JObject val = JObject.Parse(source);
							string path2 = ((object)val["files"][(object)0][(object)"name"]).ToString();
							string url2 = ((object)val["files"][(object)0][(object)"url"]).ToString();
							string path3 = ((plugin.ExistentPlugin == null) ? Path.Combine(Defines.GetScriptsFolder(), path2) : plugin.ExistentPlugin.FilePath);
							core.webrequest.EnqueueData(url2, null, delegate(int _, byte[] array)
							{
								plugin.IsBusy = false;
								using MemoryStream stream = new MemoryStream(array);
								using ZipArchive zipArchive = new ZipArchive(stream);
								foreach (ZipArchiveEntry entry in zipArchive.Entries)
								{
									switch (Path.GetExtension(entry.Name))
									{
									case ".cs":
									{
										using (StreamReader streamReader = new StreamReader(entry.Open()))
										{
											string content = streamReader.ReadToEnd();
											OsEx.File.Move(Path.Combine(Defines.GetScriptsFolder(), entry.Name), Path.Combine(Defines.GetScriptsFolder(), "backups", entry.Name));
											OsEx.File.Create(Path.Combine(Defines.GetScriptsFolder(), entry.Name), content);
											Singleton.Puts(" Extracted plugin file " + entry.Name);
										}
										break;
									}
									case ".dll":
										StoreFile(entry, Path.Combine(Defines.GetLibFolder(), entry.Name ?? ""), "extension");
										break;
									case ".json":
									{
										string fullName = entry.FullName;
										if (fullName.Contains("data"))
										{
											int startIndex = fullName.IndexOf("data") + 5;
											string directoryName = Path.GetDirectoryName(fullName.Substring(startIndex));
											StoreFile(entry, Path.Combine(Defines.GetDataFolder(), directoryName, entry.Name), "data");
										}
										else
										{
											string text = fullName;
											if (text.Contains("config"))
											{
												int startIndex2 = text.IndexOf("config") + 5;
												string directoryName2 = Path.GetDirectoryName(text.Substring(startIndex2));
												StoreFile(entry, Path.Combine(Defines.GetConfigsFolder(), directoryName2, entry.Name), "config");
											}
										}
										break;
									}
									}
								}
								Singleton.Puts("Downloaded " + plugin.Name);
								OsEx.File.Create(path3, array);
							}, core, RequestMethod.GET, _headers);
						}
					}, core, RequestMethod.GET, _headers);
					return;
				}
				string path = ((plugin.ExistentPlugin == null) ? Path.Combine(Defines.GetScriptsFolder(), plugin.File) : plugin.ExistentPlugin.FilePath);
				string url = DownloadEndpoint.Replace("[ID]", id);
				core.webrequest.Enqueue(url, null, delegate(int error, string source)
				{
					if (error != 200)
					{
						Logger.Error(string.Format("[{0}] Failed downloading item '{1} by {2}'. Error code {3}!", new object[4] { Type, plugin.Name, plugin.Author, error }));
					}
					else
					{
						plugin.IsBusy = false;
						if (!source.StartsWith("<!DOCTYPE html>"))
						{
							Singleton.Puts("Downloaded " + plugin.Name);
							OsEx.File.Move(path, Path.Combine(Defines.GetScriptsFolder(), "backups", plugin.File));
							OsEx.File.Create(path, source);
						}
					}
				}, core, RequestMethod.GET, new Dictionary<string, string>
				{
					["user-agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36 Edg/110.0.1587.63",
					["accept"] = "ext/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7"
				});
				static void StoreFile(ZipArchiveEntry entry, string text, string context)
				{
					using MemoryStream memoryStream = new MemoryStream();
					using Stream stream = entry.Open();
					stream.CopyTo(memoryStream);
					byte[] contents = memoryStream.ToArray();
					OsEx.File.Move(text, Path.Combine(Defines.GetScriptsFolder(), "backups", Path.GetFileName(text)));
					OsEx.File.Create(text, contents);
					Singleton.Puts(" Extracted plugin " + context + " file '" + entry.Name + "'");
				}
			}

			public override void Uninstall(string id)
			{
				Plugin plugin = FetchedPlugins.FirstOrDefault((Plugin x) => x.Id.Equals(id, StringComparison.CurrentCultureIgnoreCase) || x.Name.Equals(id, StringComparison.CurrentCultureIgnoreCase) || Path.GetFileNameWithoutExtension(x.File).Equals(id, StringComparison.CurrentCultureIgnoreCase));
				ModLoader.UninitializePlugin(plugin.ExistentPlugin);
				OsEx.File.Move(plugin.ExistentPlugin.FilePath, Path.Combine(Defines.GetScriptsFolder(), "backups", plugin.ExistentPlugin.FileName));
				plugin.ExistentPlugin = null;
			}

			public override void CheckMetadata(string id, Action onMetadataRetrieved)
			{
			}

			public void Validate(PlayerSession session, Action onComplete)
			{
				CorePlugin core = Community.Runtime.Core;
				ValidationTimer = core.timer.Every(AuthValidationCheckRate, delegate
				{
					if (User == null || !session.IsInMenu)
					{
						ValidationTimer?.Destroy();
						ValidationTimer = null;
						User = null;
					}
					else
					{
						string url = string.Format(AuthValidationEndpoint, AuthCode);
						core.webrequest.Enqueue(url, null, delegate(int code, string result)
						{
							if (User == null)
							{
								LoggedInUser loggedInUser = (User = new LoggedInUser());
							}
							if (code == 401)
							{
								LoggedInUser.RequestResult pendingResult = User.PendingResult;
								User.PendingResult = LoggedInUser.RequestResult.Processing;
								if (pendingResult != User.PendingResult)
								{
									Singleton.Draw(session.Player);
								}
							}
							else
							{
								JObject val = JObject.Parse(result);
								User.AccessToken = ((object)val["accesstoken"]).ToString();
								User.AccessTokenEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(User.AccessToken));
								ValidationTimer.Destroy();
								ValidationTimer = null;
								_headers[AuthHeader.Key.ToString()] = string.Format(AuthHeader.Value, User.AccessToken);
								Analytics.codefling_login();
								User.PendingResult = LoggedInUser.RequestResult.Complete;
								onComplete?.Invoke();
							}
						}, null);
					}
				});
			}

			public void RefreshUser(PlayerSession session)
			{
				if (!IsLoggedIn)
				{
					return;
				}
				CorePlugin core = Community.Runtime.Core;
				KeyValuePair<HttpRequestHeader, string> authHeader = AuthHeader;
				Dictionary<string, string> headers = new Dictionary<string, string> { [authHeader.Key.ToString()] = string.Format(authHeader.Value, User.AccessToken) };
				core.webrequest.Enqueue(AuthUserInfoEndpoint, null, delegate(int code, string info)
				{
					if (code == 200)
					{
						JObject val = JObject.Parse(info);
						User.Authority = ((object)val["primaryGroup"][(object)"name"])?.ToString();
						User.AvatarUrl = ((object)val["photoUrl"])?.ToString();
						User.DisplayName = ((object)val["formattedName"])?.ToString();
						User.CoverUrl = ((object)val["coverPhotoUrl"])?.ToString();
						User.Id = ((object)val["id"]).ToString().ToInt();
						User.IsAdmin = User.Authority == "Administrator";
						core.webrequest.Enqueue(AuthOwnedPluginsEndpoint, null, delegate(int num, string data)
						{
							JObject val2 = JObject.Parse(data);
							User.OwnedFiles.Clear();
							foreach (JToken item in (IEnumerable<JToken>)val2["results"])
							{
								User.OwnedFiles.Add(((object)item[(object)"itemId"]).ToString());
							}
							Refresh();
							Save();
							Singleton.Draw(session.Player);
						}, core, RequestMethod.GET, headers);
					}
					else
					{
						User = null;
						Refresh();
						Save();
						Singleton.Draw(session.Player);
					}
				}, core, RequestMethod.GET, headers);
			}

			public bool Load()
			{
				try
				{
					string file = Path.Combine(Defines.GetDataFolder(), "vendordata_cf.db");
					if (!OsEx.File.Exists(file))
					{
						return false;
					}
					using MemoryStream memoryStream = new MemoryStream(OsEx.File.ReadBytes(file));
					Codefling codefling = Serializer.Deserialize<Codefling>((Stream)memoryStream);
					LastTick = codefling.LastTick;
					FetchedPlugins.Clear();
					FetchedPlugins.AddRange(codefling.FetchedPlugins);
					User = codefling.User;
					if (User != null && !string.IsNullOrEmpty(User.AccessTokenEncoded))
					{
						User.AccessToken = Encoding.UTF8.GetString(Convert.FromBase64String(User.AccessTokenEncoded));
						_headers[AuthHeader.Key.ToString()] = string.Format(AuthHeader.Value, User.AccessToken);
					}
					if ((DateTime.Now - new DateTime(codefling.LastTick)).TotalHours >= 24.0)
					{
						Singleton.Puts("Invalidated " + Type + " database. Fetching...");
						return false;
					}
					Singleton.Puts("Loaded " + Type + " plugin metadata cache from file.");
					Refresh();
				}
				catch
				{
					return false;
				}
				return true;
			}

			public void Save()
			{
				try
				{
					string text = Path.Combine(Defines.GetDataFolder(), "vendordata_cf.db");
					using MemoryStream memoryStream = new MemoryStream();
					LastTick = DateTime.Now.Ticks;
					Serializer.Serialize<Codefling>((Stream)memoryStream, this);
					OsEx.File.Create(text, memoryStream.ToArray());
					Singleton.Puts("Stored " + Type + " to file: " + text);
				}
				catch (Exception ex)
				{
					Logger.Error(Type + ".Save error", ex);
				}
			}
		}

		[ProtoContract]
		public class uMod : Vendor, IVendorStored
		{
			public WebRequests.WebRequest FetchingRequest;

			public WebRequests.WebRequest FetchingPageRequest;

			public Oxide.Plugins.Timer FetchingTimer;

			public override string Type => "uMod";

			public override string Url => "https://umod.org";

			public override string Logo => "umodlogo";

			public override float LogoRatio => 0.2f;

			public override string Hero => "umod_hero";

			public override string Tagline => "A large platform for free plugins curated by the Oxide team.";

			public override string BarInfo => $"{FetchedPlugins.Count:n0} free";

			public override string ListEndpoint => "https://umod.org/plugins/search.json?page=[ID]&sort=title&sortdir=asc&categories%5B0%5D=universal&categories%5B1%5D=rust";

			public override string DownloadEndpoint => "https://umod.org/plugins/[ID].cs";

			public override string PluginLookupEndpoint => "https://umod.org/plugins/[ID]/latest.json";

			public void Dispose()
			{
				FetchingRequest?.Dispose();
				FetchingPageRequest?.Dispose();
				FetchingTimer?.Destroy();
				FetchingRequest = null;
				FetchingPageRequest = null;
				FetchingTimer = null;
			}

			public override void Refresh()
			{
				if (FetchedPlugins == null)
				{
					return;
				}
				List<RustPlugin> list = Pool.Get<List<RustPlugin>>();
				Community.Runtime.Core.plugins.GetAllNonAlloc(list);
				foreach (Plugin fetchedPlugin in FetchedPlugins)
				{
					string fileName = Path.GetFileName(fetchedPlugin.File);
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fetchedPlugin.File);
					foreach (RustPlugin item in list)
					{
						if ((!string.IsNullOrEmpty(item.FileName) && (item.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase) || item.FileName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase))) || (!string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(fetchedPlugin.Name) && item.Name.Equals(fetchedPlugin.Name, StringComparison.OrdinalIgnoreCase)))
						{
							fetchedPlugin.SetExistentPlugin(item);
							break;
						}
					}
				}
				Pool.FreeUnmanaged<RustPlugin>(ref list);
				PriceData = FetchedPlugins.OrderBy((Plugin x) => x.OriginalPrice);
				AuthorData = FetchedPlugins.OrderBy((Plugin x) => x.Author);
				InstalledData = FetchedPlugins.Where((Plugin x) => x.IsInstalled());
				OutOfDateData = FetchedPlugins.Where((Plugin x) => x.IsInstalled() && !x.IsUpToDate());
				OwnedData = FetchedPlugins.Where((Plugin x) => x.Owned);
			}

			public override void FetchList(Action<Vendor> callback = null)
			{
				FetchedPlugins.Clear();
				Logger.Log("[" + Type + "] Caching plugin metadata for displaying plugins in the Admin module -> Plugins tab. This might take a while..");
				FetchingRequest = Community.Runtime.Core.webrequest.Enqueue(ListEndpoint.Replace("[ID]", "0"), null, delegate(int error, string data)
				{
					if (error != 200)
					{
						Logger.Error($"[{Type}] Failed fetching vendor. Error code {error}!");
					}
					else
					{
						JObject val = JObject.Parse(data);
						int? num = ((object)val["last_page"])?.ToString().ToInt();
						if (num == 0)
						{
							Logger.Warn("[" + Type + "] Endpoint seems to be down. Will retry gathering plugin metadata again later...");
							val = null;
						}
						else
						{
							FetchPage(0, num.GetValueOrDefault(), callback);
							val = null;
						}
					}
				}, Community.Runtime.Core);
			}

			public override void Download(string id, Action onTimeout = null)
			{
				Plugin plugin = FetchedPlugins.FirstOrDefault((Plugin x) => x.Id.Equals(id, StringComparison.CurrentCultureIgnoreCase) || x.Name.Equals(id, StringComparison.CurrentCultureIgnoreCase) || Path.GetFileNameWithoutExtension(x.File).Equals(id, StringComparison.CurrentCultureIgnoreCase));
				string path = ((plugin.ExistentPlugin == null) ? Path.Combine(Defines.GetScriptsFolder(), plugin.File) : plugin.ExistentPlugin.FilePath);
				string url = DownloadEndpoint.Replace("[ID]", plugin.Name);
				plugin.IsBusy = true;
				Community.Runtime.Core.timer.In(2f, delegate
				{
					if (plugin.IsBusy)
					{
						plugin.IsBusy = false;
						onTimeout?.Invoke();
					}
				});
				Community.Runtime.Core.webrequest.Enqueue(url, null, delegate(int error, string source)
				{
					if (error != 200)
					{
						Logger.Error(string.Format("[{0}] Failed downloading item '{1} by {2}'. Error code {3}!", new object[4] { Type, plugin.Name, plugin.Author, error }));
					}
					else
					{
						Singleton.Puts("Downloaded " + plugin.Name);
						OsEx.File.Move(path, Path.Combine(Defines.GetScriptsFolder(), "backups", plugin.File));
						OsEx.File.Create(path, source);
						plugin.IsBusy = false;
						plugin.DownloadCount++;
					}
				}, Community.Runtime.Core, RequestMethod.GET, new Dictionary<string, string>
				{
					["user-agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36 Edg/110.0.1587.63",
					["accept"] = "ext/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7"
				});
			}

			public override void Uninstall(string id)
			{
				Plugin plugin = FetchedPlugins.FirstOrDefault((Plugin x) => x.Id.Equals(id, StringComparison.CurrentCultureIgnoreCase) || x.Name.Equals(id, StringComparison.CurrentCultureIgnoreCase) || Path.GetFileNameWithoutExtension(x.File).Equals(id, StringComparison.CurrentCultureIgnoreCase));
				ModLoader.UninitializePlugin(plugin.ExistentPlugin);
				OsEx.File.Move(plugin.ExistentPlugin.FilePath, Path.Combine(Defines.GetScriptsFolder(), "backups", plugin.ExistentPlugin.FileName));
				plugin.ExistentPlugin = null;
			}

			public override void CheckMetadata(string id, Action onMetadataRetrieved)
			{
				Plugin plugin = FetchedPlugins.FirstOrDefault((Plugin x) => x.Id.Equals(id, StringComparison.CurrentCultureIgnoreCase) || x.Name.Equals(id, StringComparison.CurrentCultureIgnoreCase) || Path.GetFileNameWithoutExtension(x.File).Equals(id, StringComparison.CurrentCultureIgnoreCase));
				if (plugin.HasLookup)
				{
					return;
				}
				Community.Runtime.Core.webrequest.Enqueue(PluginLookupEndpoint.Replace("[ID]", plugin.Name.ToLower().Trim()), null, delegate(int error, string data)
				{
					if (error != 200)
					{
						Logger.Error(string.Format("[{0}] Failed fetching item metadata for '{1} by {2}'. Error code {3}!", new object[4] { Type, plugin.Name, plugin.Author, error }));
					}
					else
					{
						JObject val = JObject.Parse(data);
						string text = ((object)val["description_md"])?.ToString();
						plugin.Changelog = text.Replace("<div>", "").Replace("</div>", "").Replace("\\n", "")
							.Replace("<br />", "\n")
							.Replace("<pre>", "")
							.Replace("<p>", "")
							.Replace("</p>", "")
							.Replace("<span class=\"documentation\">", "")
							.Replace("</span>", "")
							.Replace("<code>", "<b>")
							.Replace("</code>", "</b>")
							.Replace("<ul>", "")
							.Replace("</ul>", "")
							.Replace("<li>", "")
							.Replace("</li>", "")
							.Replace("<em>", "")
							.Replace("</em>", "")
							.Replace("<h1>", "<b>")
							.Replace("</h1>", "</b>")
							.Replace("<h2>", "<b>")
							.Replace("</h2>", "</b>")
							.Replace("<h3>", "<b>")
							.Replace("</h3>", "</b>")
							.Replace("<h4>", "<b>")
							.Replace("</h4>", "</b>")
							.Replace("<strong>", "<b>")
							.Replace("</strong>", "</b>");
						if (!string.IsNullOrEmpty(plugin.Changelog) && !plugin.Changelog.EndsWith("."))
						{
							plugin.Changelog = plugin.Changelog.Trim() + ".";
						}
						plugin.HasLookup = true;
						onMetadataRetrieved?.Invoke();
					}
				}, Community.Runtime.Core);
			}

			public void FetchPage(int page, int maxPage, Action<Vendor> callback = null)
			{
				if (page > maxPage)
				{
					Save();
					callback?.Invoke(this);
					return;
				}
				FetchingPageRequest = Community.Runtime.Core.webrequest.Enqueue(ListEndpoint.Replace("[ID]", $"{page}"), null, delegate(int error, string data)
				{
					if (error != 200)
					{
						Logger.Error($"[{Type}] Failed fetching page for vendor. Error code {error}!");
					}
					else
					{
						JObject val = JObject.Parse(data);
						JToken val2 = val["data"];
						List<RustPlugin> list = Pool.Get<List<RustPlugin>>();
						Community.Runtime.Core.plugins.GetAllNonAlloc(list);
						foreach (JToken item in (IEnumerable<JToken>)val2)
						{
							string text = ((object)item[(object)"icon_url"])?.ToString();
							Plugin p = new Plugin
							{
								Id = ((object)item[(object)"url"])?.ToString(),
								Name = ((object)item[(object)"name"])?.ToString(),
								Author = ((object)item[(object)"author"])?.ToString(),
								Version = ((object)item[(object)"latest_release_version"])?.ToString(),
								Description = ((object)item[(object)"description"])?.ToString(),
								OriginalPrice = "FREE",
								File = ((object)item[(object)"name"])?.ToString() + ".cs",
								Image = text,
								ImageThumbnail = text,
								ImageSize = 0,
								DownloadCount = (((object)item[(object)"downloads"])?.ToString().ToInt()).GetValueOrDefault(),
								Date = ((object)item[(object)"published_at"])?.ToString(),
								UpdateDate = ((object)item[(object)"updated_at"])?.ToString(),
								Tags = ((object)item[(object)"tags_all"])?.ToString().Split(','),
								Rating = -1f
							};
							p.PreferredVendor = VendorTypes.uMod;
							if (!string.IsNullOrEmpty(p.Description) && !p.Description.EndsWith("."))
							{
								p.Description += ".";
							}
							if (string.IsNullOrEmpty(p.Author.Trim()))
							{
								p.Author = "Unmaintained";
							}
							if (p.OriginalPrice == "{}")
							{
								p.OriginalPrice = "FREE";
							}
							try
							{
								p.ExistentPlugin = list.FirstOrDefault((RustPlugin x) => Path.GetFileNameWithoutExtension(x.FilePath) == Path.GetFileNameWithoutExtension(p.File));
							}
							catch
							{
							}
							if (!FetchedPlugins.Any((Plugin x) => x.Name == p.Name))
							{
								FetchedPlugins.Add(p);
							}
						}
						Pool.FreeUnmanaged<RustPlugin>(ref list);
						if (page % (maxPage / 4) == 0 || page == maxPage - 1)
						{
							Logger.Log($"Caching plugin metadata page {page} out of {maxPage}");
						}
					}
				}, Community.Runtime.Core);
				FetchingTimer = Community.Runtime.Core.timer.In(5f, delegate
				{
					FetchPage(page + 1, maxPage, callback);
				});
			}

			public bool Load()
			{
				try
				{
					string file = Path.Combine(Defines.GetDataFolder(), "vendordata_umod.db");
					if (!OsEx.File.Exists(file))
					{
						return false;
					}
					using MemoryStream memoryStream = new MemoryStream(OsEx.File.ReadBytes(file));
					uMod uMod2 = Serializer.Deserialize<uMod>((Stream)memoryStream);
					LastTick = uMod2.LastTick;
					FetchedPlugins.Clear();
					FetchedPlugins.AddRange(uMod2.FetchedPlugins);
					if ((DateTime.Now - new DateTime(uMod2.LastTick)).TotalHours >= 24.0)
					{
						Singleton.Puts("Invalidated " + Type + " database. Fetching...");
						return false;
					}
					Singleton.Puts("Loaded " + Type + " plugin metadata cache from file.");
					Refresh();
				}
				catch
				{
					return false;
				}
				return true;
			}

			public void Save()
			{
				try
				{
					string file = Path.Combine(Defines.GetDataFolder(), "vendordata_umod.db");
					using MemoryStream memoryStream = new MemoryStream();
					LastTick = DateTime.Now.Ticks;
					Serializer.Serialize<uMod>((Stream)memoryStream, this);
					OsEx.File.Create(file, memoryStream.ToArray());
					Singleton.Puts("Stored " + Type + " plugin metadata cache to file.");
				}
				catch (Exception ex)
				{
					Singleton.PutsError(" Couldn't store uMod plugins list.", ex);
				}
			}
		}

		[ProtoContract]
		public class Installed : Vendor
		{
			private string[] _defaultTags = new string[2] { "carbon", "oxide" };

			public override string Type => "Installed";

			public override string Url => "none";

			public override string Logo => "carbonw";

			public override string Hero => "installed_hero";

			public override string Tagline => "All actively loaded plugins. Items with no metadata most likely don't exist on the public vendors.";

			public override float LogoRatio => 0.23f;

			public override string ListEndpoint => string.Empty;

			public override string DownloadEndpoint => string.Empty;

			public override string BarInfo => $"{FetchedPlugins.Count:n0} loaded";

			public override bool CanRefresh => false;

			public override void CheckMetadata(string id, Action callback)
			{
			}

			public override void Download(string id, Action onTimeout = null)
			{
			}

			public override void Uninstall(string id)
			{
				Plugin plugin = FetchedPlugins.FirstOrDefault((Plugin x) => x.Id.Equals(id, StringComparison.CurrentCultureIgnoreCase) || x.Name.Equals(id, StringComparison.CurrentCultureIgnoreCase) || Path.GetFileNameWithoutExtension(x.File).Equals(id, StringComparison.CurrentCultureIgnoreCase));
				ModLoader.UninitializePlugin(plugin.ExistentPlugin);
				OsEx.File.Move(plugin.ExistentPlugin.FilePath, Path.Combine(Defines.GetScriptsFolder(), "backups", plugin.ExistentPlugin.FileName));
				plugin.ExistentPlugin = null;
			}

			public override void FetchList(Action<Vendor> callback = null)
			{
			}

			public bool Load()
			{
				return true;
			}

			public override void Refresh()
			{
				PooledList<Plugin> val = Pool.Get<PooledList<Plugin>>();
				try
				{
					((List<Plugin>)(object)val).AddRange((IEnumerable<Plugin>)FetchedPlugins);
					foreach (Plugin item in (List<Plugin>)(object)val)
					{
						if (item.ExistentPlugin == null || !item.ExistentPlugin.HasInitialized)
						{
							FetchedPlugins.Remove(item);
						}
					}
					foreach (ModLoader.Package package in ModLoader.Packages)
					{
						foreach (RustPlugin plugin in package.Plugins)
						{
							if (!plugin.IsCorePlugin)
							{
								Plugin plugin2 = CodeflingInstance.FetchedPlugins.FirstOrDefault((Plugin x) => x.ExistentPlugin == plugin);
								Plugin plugin3 = uModInstance?.FetchedPlugins.FirstOrDefault((Plugin x) => x.ExistentPlugin == plugin);
								Plugin plugin4 = FetchedPlugins.FirstOrDefault((Plugin x) => x.ExistentPlugin == plugin);
								if (plugin4 == null)
								{
									plugin4 = new Plugin
									{
										Name = plugin.Name,
										Author = plugin.Author,
										Version = plugin.Version.ToString(),
										ExistentPlugin = plugin,
										Description = "This is an unlisted plugin.",
										Tags = _defaultTags,
										File = plugin.FileName,
										Id = plugin.Name,
										UpdateDate = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
										Rating = -1f
									};
									FetchedPlugins.Add(plugin4);
								}
								plugin4.TryMarkFoundOn(plugin3);
								plugin4.TryMarkFoundOn(plugin2);
								if (plugin4.PreferredVendor == VendorTypes.Installed && plugin4.AvailableOn != null && plugin4.AvailableOn.Count > 0)
								{
									Plugin plugin5 = plugin4.AvailableOn[0];
									plugin4.PreferredVendor = plugin5.PreferredVendor;
									plugin4.PreferredVendorPlugin = plugin5;
								}
							}
						}
					}
					PriceData = FetchedPlugins.OrderBy((Plugin x) => x.OriginalPrice);
					AuthorData = FetchedPlugins.OrderBy((Plugin x) => x.Author);
					InstalledData = FetchedPlugins.Where((Plugin x) => x.IsInstalled());
					OutOfDateData = FetchedPlugins.Where((Plugin x) => x.IsInstalled() && !x.IsUpToDate());
					OwnedData = FetchedPlugins.OrderBy((Plugin x) => x.Owned);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}

			public void Save()
			{
			}
		}

		[ProtoContract]
		public class ServerOwner
		{
			[ProtoMember(1)]
			public List<string> FavouritePlugins = new List<string>();

			public static ServerOwner Singleton { get; internal set; } = new ServerOwner();

			public static void Load()
			{
				try
				{
					string file = Path.Combine(Defines.GetDataFolder(), "vendordata_svowner.db");
					if (!OsEx.File.Exists(file))
					{
						Save();
						return;
					}
					using MemoryStream memoryStream = new MemoryStream(OsEx.File.ReadBytes(file));
					Singleton = Serializer.Deserialize<ServerOwner>((Stream)memoryStream);
					ServerOwner singleton = Singleton;
					if (singleton.FavouritePlugins == null)
					{
						singleton.FavouritePlugins = new List<string>();
					}
				}
				catch (Exception ex)
				{
					Logger.Error("ServerOwner.Load failed", ex);
					Singleton = new ServerOwner();
					Save();
				}
			}

			public static void Save()
			{
				try
				{
					string file = Path.Combine(Defines.GetDataFolder(), "vendordata_svowner.db");
					using MemoryStream memoryStream = new MemoryStream();
					Serializer.Serialize<ServerOwner>((Stream)memoryStream, Singleton);
					OsEx.File.Create(file, memoryStream.ToArray());
				}
				catch (Exception ex)
				{
					Logger.Error("ServerOwner.Save failed", ex);
				}
			}
		}

		[ProtoContract(/*Could not decode attribute arguments.*/)]
		public class Plugin
		{
			public string Id;

			public string Name;

			public string Author;

			public string Version;

			public string Description;

			public string Changelog;

			public string OriginalPrice;

			public string SalePrice;

			public string[] Dependencies;

			public string File;

			public string Image;

			public string ImageThumbnail;

			public int ImageSize;

			public IEnumerable<string> Tags;

			public int DownloadCount;

			public float Rating;

			public string Date;

			public string UpdateDate;

			public bool HasLookup;

			public Status Status;

			public bool CarbonCompatible;

			public bool Owned;

			public VendorTypes PreferredVendor;

			[ProtoIgnore]
			public List<Plugin> AvailableOn;

			[ProtoIgnore]
			public RustPlugin ExistentPlugin;

			internal Plugin PreferredVendorPlugin;

			internal bool IsBusy;

			[ProtoIgnore]
			public bool HasRating => Rating != -1f;

			[ProtoIgnore]
			public bool HasPrice => OriginalPrice != "Null";

			public Vendor GetPreferredVendor()
			{
				return GetVendor(PreferredVendor);
			}

			public void SetPreferredVendor(VendorTypes vendor)
			{
				PreferredVendor = vendor;
				PreferredVendorPlugin = GetVendor(vendor).FetchedPlugins.FirstOrDefault((Plugin x) => x.Name.Equals(Name));
			}

			public void TryMarkFoundOn(Plugin plugin)
			{
				if (plugin != null)
				{
					if (AvailableOn == null)
					{
						AvailableOn = new List<Plugin>();
					}
					if (!AvailableOn.Contains(plugin) && !AvailableOn.Contains(this))
					{
						AvailableOn.Add(plugin);
					}
				}
			}

			public bool HasInvalidImage()
			{
				return ImageSize >= 2504304;
			}

			public bool HasNoImage()
			{
				if (!string.IsNullOrEmpty(Image))
				{
					return Image.Equals("Null");
				}
				return true;
			}

			public bool IsInstalled()
			{
				if (ExistentPlugin != null)
				{
					return ExistentPlugin.IsLoaded;
				}
				return false;
			}

			public string CurrentVersion()
			{
				if (IsInstalled())
				{
					return ExistentPlugin.Version.ToString();
				}
				return "N/A";
			}

			public bool IsPaid()
			{
				if (!string.IsNullOrEmpty(OriginalPrice) && OriginalPrice != "FREE")
				{
					return OriginalPrice != "Null";
				}
				return false;
			}

			public bool IsUpToDate()
			{
				if (!IsInstalled())
				{
					return false;
				}
				return ExistentPlugin.Version.ToString() == Version;
			}

			public void SetOwned(bool wants)
			{
				Owned = wants;
			}

			public void SetExistentPlugin(RustPlugin plugin)
			{
				ExistentPlugin = plugin;
			}
		}

		public static bool DropdownShow;

		public static List<string> TagFilter = new List<string>();

		public static Tab TabInstance;

		public static Vendor CodeflingInstance;

		public static Vendor uModInstance;

		public static Vendor LocalInstance;

		public static string[] DropdownOptions { get; } = new string[7] { "A-Z", "Price", "Author", "Installed", "Pending Update", "Favourites", "Owned" };

		public static PlayerSession.Page PlaceboPage { get; } = new PlayerSession.Page();

		public static string[] PopularTags { get; } = new string[21]
		{
			"gui", "admin", "moderation", "chat", "building", "discord", "libraries", "loot", "pve", "event",
			"logging", "anti-cheat", "economics", "npc", "info", "limitations", "statistics", "monuments", "seasonal", "banan",
			"peanus"
		};

		public static Vendor GetVendor(VendorTypes vendor)
		{
			return vendor switch
			{
				VendorTypes.Codefling => CodeflingInstance, 
				VendorTypes.uMod => uModInstance, 
				VendorTypes.Installed => LocalInstance, 
				_ => null, 
			};
		}

		public static Tab Get()
		{
			OsEx.Folder.Create(Path.Combine(Defines.GetScriptsFolder(), "backups"));
			Tab tab = null;
			tab = new Tab("plugins", "Plugins", Community.Runtime.Core, delegate
			{
				tab.AddColumn(0, clear: true);
				tab.AddColumn(1, clear: true);
				tab.Override = delegate(Tab tab3, CUI cui, CuiElementContainer container, string panel, PlayerSession ap)
				{
					cui.CreatePanel(container, panel, Cache.CUI.BlackColor);
					float optionsOffset = 0f;
					Vendor vendor = GetVendor(ap.GetStorage(tab3, "vendor", VendorTypes.Installed));
					bool flag = vendor == LocalInstance;
					CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, panel, "#1f1f1d", null, 0f, 1f, 0.93f);
					string[] names = Enum.GetNames(typeof(VendorTypes));
					foreach (string text in names)
					{
						bool disabled = GetVendor((VendorTypes)Enum.Parse(typeof(VendorTypes), text)) == null;
						bool isSelected = vendor.GetType().Name == text;
						CreateTabButton(cui, container, pair, text, vendor?.BarInfo.ToUpper(), isSelected, ref optionsOffset, disabled);
					}
					string storage = ap.GetStorage(tab3, "search", string.Empty);
					int maxPages;
					List<Plugin> plugins = GetPlugins(vendor, tab3, ap, out maxPages, 75);
					PlayerSession.Page orCreatePage = ap.GetOrCreatePage(230);
					float num = plugins.Count();
					float num2 = Mathf.Ceil(num / 5f) - 2f;
					float num3 = (150f + num2 * 200f).Clamp(30f, 2.1474836E+09f);
					orCreatePage.TotalPages = maxPages;
					orCreatePage.Check();
					CuiRectTransform contentTransformComponent;
					CuiScrollbar horizontalScrollBar;
					CuiScrollbar verticalScrollBar;
					CUI.Pair<string, CuiElement> pair2 = cui.CreateScrollView(container, panel, vertical: true, horizontal: false, (MovementType)1, 0.1f, inertia: true, 0.1f, 150f, out contentTransformComponent, out horizontalScrollBar, out verticalScrollBar, 0f, 1f, 0f, 0.93f);
					contentTransformComponent.AnchorMin = "0 0";
					contentTransformComponent.AnchorMax = "1 1";
					contentTransformComponent.OffsetMin = $"0 -{num3}";
					contentTransformComponent.OffsetMax = "0 0";
					verticalScrollBar.Size = 4f;
					verticalScrollBar.AutoHide = false;
					verticalScrollBar.Invert = false;
					cui.CreateImage(container, pair2, vendor.Hero, Cache.CUI.WhiteColor, null, 0f, 1f, 1f, 1f, 0f, 0f, -500f);
					CUI.Pair<string, CuiElement> pair3 = cui.CreatePanel(container, pair2, "0 0.1 0.3 0.4", null, 0f, 1f, 1f, 1f, 0f, 0f, 0f - (num3 + 450f));
					if (!plugins.Any())
					{
						cui.CreateText(container, pair3, "0.4 0.4 0.4 0.5", "No plugins available", 10, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
					}
					cui.CreateText(container, pair3, "0.8 0.8 0.8 0.9", vendor.Type.ToUpper(), 32, 0.04f, 1f, 1f, 1f, 0f, 0f, -100f, -30f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
					cui.CreateText(container, pair3, "0.6 0.6 0.6 0.9", vendor.Tagline, 15, 0.04f, 1f, 1f, 1f, 0f, 0f, -100f, -70f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
					if (vendor.CanRefresh)
					{
						CUI.Pair<string, CuiElement, CuiElement> pair4 = cui.CreateProtectedButton(container, pair3, "0.2 0.3 0.5 0.9", Cache.CUI.BlankColor, string.Empty, 0, null, 0.26f, 0.295f, 1f, 1f, 0f, 0f, -130f, -100f, "pluginbrowser.refreshvendor", (TextAnchor)4);
						cui.CreateImage(container, pair4, "reload", "0.3 0.5 0.8 1", null, 0.2f, 0.8f, 0.2f, 0.8f);
						cui.CreateImage(container, pair4, "fade", Cache.CUI.WhiteColor);
					}
					if (vendor is IVendorAuthenticated vendorAuthenticated)
					{
						string keyImage = Singleton.ImageDatabase.GetKeyImage("default_profile");
						cui.CreateClientImage(container, pair3, (vendorAuthenticated != null && vendorAuthenticated.IsLoggedIn) ? vendorAuthenticated.User.AvatarUrl : keyImage, "1 1 1 0.7", null, 0.91f, 0.96f, 1f, 1f, 0f, 0f, -65f, -20f);
						cui.CreateText(container, pair3, "0.8 0.8 0.8 0.9", (vendorAuthenticated == null || !vendorAuthenticated.IsLoggedIn) ? "GUEST" : vendorAuthenticated.User?.DisplayName?.ToUpper(), 15, 0f, 0.9f, 1f, 1f, 0f, 0f, -100f, -30f, (TextAnchor)2, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
						cui.CreateText(container, pair3, "0.85 0.8 0.1 0.9", (vendorAuthenticated == null || !vendorAuthenticated.IsLoggedIn) ? "NOT AUTHENTICATED" : vendorAuthenticated.User?.Authority?.ToUpper(), 10, 0f, 0.9f, 1f, 1f, 0f, 0f, -100f, -50f, (TextAnchor)2, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
						if (vendorAuthenticated != null)
						{
							cui.CreateProtectedButton(container, pair3, (vendorAuthenticated != null && vendorAuthenticated.IsLoggedIn) ? "0.55 0.1 0.1 1" : "0.1 0.55 0.1 1", (vendorAuthenticated != null && vendorAuthenticated.IsLoggedIn) ? "1 0.5 0.5 1" : "0.5 1 0.5 1", (vendorAuthenticated != null && vendorAuthenticated.IsLoggedIn) ? "LOG OUT" : "LOG IN", 10, null, 0.85f, 0.9f, 1f, 1f, 0f, 0f, -90f, -70f, "pluginbrowser.login", (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedBold);
						}
					}
					float num4 = 50f;
					float num5 = 150f;
					int num6 = 0;
					float num7 = 1f;
					string keyImage2 = Singleton.ImageDatabase.GetKeyImage(vendor.Logo);
					string command;
					for (int j = 0; j < plugins.Count; j++)
					{
						num6++;
						Plugin plugin = plugins[j];
						Plugin plugin2 = plugin;
						if (flag && plugin.PreferredVendorPlugin != null)
						{
							plugin = plugin.PreferredVendorPlugin;
						}
						bool flag2 = Singleton.DataInstance.HidePluginIcons || plugin.HasNoImage() || plugin.HasInvalidImage();
						string parent = pair3;
						string blankColor = Cache.CUI.BlankColor;
						command = "pluginbrowser.selectplugin \"" + Path.GetFileNameWithoutExtension(plugin.File) + "\"";
						CUI.Pair<string, CuiElement, CuiElement> pair5 = cui.CreateProtectedButton(container, parent, "0.1 0.1 0.1 0.7", blankColor, null, 0, null, 0f, 0f, 1f, 1f, num4, num4 + 150f, 0f - (num5 + 190f), 0f - num5, command, (TextAnchor)4);
						CUI.Pair<string, CuiElement> pair6 = cui.CreateClientImage(container, pair5, plugin.ImageThumbnail, flag2 ? "0 0 0 0" : "1 1 1 0.95", null, 0f, 1f, 1f, 1f, 0f, 0f, -150f, 0f, num7);
						if (flag2)
						{
							cui.CreatePanel(container, pair6, "1 1 1 0.15");
							cui.CreateImage(container, pair6, keyImage2, "0.15 0.15 0.15 0.9", null, 0.2f, 0.8f, 0.2f + vendor.LogoRatio, 0.8f - vendor.LogoRatio);
						}
						cui.CreateImage(container, pair5, "fade", Cache.CUI.WhiteColor);
						if (plugin.IsInstalled())
						{
							bool flag3 = !plugin.IsUpToDate();
							CUI.Pair<string, CuiElement> pair7 = cui.CreatePanel(container, pair6, flag3 ? "0.9 0.5 0.1 0.6" : "0.5 0.9 0.1 0.6", null, 0f, 1f, 0.9f);
							cui.CreateText(container, pair7, flag3 ? "1 0.75 0.5 1" : "0.75 1 0.5 1", (flag3 ? "OUTDATED" : "INSTALLED").SpacedString(1), 8, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
						}
						if (vendor == LocalInstance && plugin2.AvailableOn != null && plugin2.AvailableOn.Count > 1)
						{
							cui.CreateProtectedButton(container, pair5, Cache.CUI.BlankColor, "0.75 1 0.5 0.8", plugin2.PreferredVendor.ToString().ToUpper().SpacedString(1), 8, null, 0f, 1f, 0.83f, 0.92f, 0f, 0f, 0f, 0f, "pluginbrowser.interact 12 \"" + Path.GetFileNameWithoutExtension(plugin.File) + "\"", (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedBold);
						}
						bool flag4 = ServerOwner.Singleton.FavouritePlugins.Contains(Path.GetFileNameWithoutExtension(plugin.File));
						CUI.Pair<string, CuiElement, CuiElement> pair8 = cui.CreateProtectedButton(container, pair5, Cache.CUI.BlankColor, Cache.CUI.BlankColor, string.Empty, 0, null, 0f, 0f, 1f, 1f, 0f, 30f, -30f, 0f, "pluginbrowser.interact 10 \"" + Path.GetFileNameWithoutExtension(plugin.File) + "\"", (TextAnchor)4);
						cui.CreateImage(container, pair8, "top_left", flag4 ? "0.7 0.2 0.2 0.6" : "0.1 0.1 0.1 0.2");
						cui.CreateImage(container, pair8, "star", flag4 ? "0.9 0.5 0.4" : "0.1 0.1 0.1 0.5", null, 0.075f, 0.5f, 0.5f, 0.9f);
						cui.CreateText(container, pair5, Cache.CUI.WhiteColor, plugin.Name.Truncate(17, "...", countElipsisLength: false), 12, 0.05f, 1f, 0f, 0.165f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1, num7);
						cui.CreateText(container, pair5, "0.8 0.8 0.8 0.6", "by " + plugin.Author, 8, 0.05f, 1f, 0f, 0.085f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1, num7);
						if (plugin.HasPrice)
						{
							cui.CreateText(container, pair5, plugin.IsPaid() ? "#e0e344" : "#44e3db", plugin.IsPaid() ? $"${plugin.OriginalPrice.ToFloat():0.00}" : "FREE", 10, 0f, 0.95f, 0f, 0.155f, 0f, 0f, 0f, 0f, (TextAnchor)2, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1, num7);
						}
						if (!Mathf.Approximately(plugin.Rating, -1f))
						{
							StringBuilder stringBuilder = Pool.Get<StringBuilder>();
							for (int k = 0; k < 5; k++)
							{
								stringBuilder.Append((plugin.Rating <= (float)k) ? "☆" : "★");
							}
							cui.CreateText(container, pair5, "0.8 0.8 0.8 0.6", stringBuilder.ToString(), 12, 0.565f, 0.96f, 0.02f, 0.08f, 0f, 0f, 0f, 0f, (TextAnchor)2, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
							Pool.FreeUnmanaged(ref stringBuilder);
						}
						if (plugin.Owned)
						{
							CUI.Pair<string, CuiElement> pair9 = cui.CreatePanel(container, pair6, "0 0.4 0.8 0.9", null, 0f, 1f, 0f, 0.1f);
							cui.CreateText(container, pair9, "0.5 0.75 1 1", "PURCHASED".SpacedString(1), 8, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
						}
						num4 += 160f;
						if (num6 % 5 == 0)
						{
							num5 += 200f;
							num4 = 50f;
						}
						num7 += 0.2f;
					}
					CUI.Pair<string, CuiElement> pair10 = cui.CreatePanel(container, pair3, "0.2 0.2 0.2 0.9", null, 0.04f, 0.25f, 1f, 1f, 0f, 0f, -130f, -100f);
					cui.CreateImage(container, pair10, "fade", Cache.CUI.WhiteColor);
					cui.CreateImage(container, pair10, "magnifying-glass", "0.8 0.8 0.8 0.6", null, 0.05f, 0.14f, 0.2f, 0.8f);
					cui.CreateProtectedInputField(container, pair10, "0.8 0.8 0.8 0.6", string.IsNullOrEmpty(storage) ? "Search..." : storage, 13, 50, readOnly: false, 0.17f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "pluginbrowser.search", (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, autoFocus: false, hudMenuInput: false, (LineType)0, 0f, 0f, needsCursor: false, needsKeyboard: true);
					CUI.Pair<string, CuiElement, CuiElement> pair11 = cui.CreateProtectedButton(container, pair10, Cache.CUI.BlankColor, Cache.CUI.BlankColor, string.Empty, 0, null, 0.9f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "pluginbrowser.search ", (TextAnchor)4);
					cui.CreateImage(container, pair11, "close", "0.4 0.4 0.4 0.8", null, 0.5f, 0.5f, 0.5f, 0.5f, -5f, 5f, -5f, 5f);
					int storage2 = (int)ap.GetStorage(tab3, "filter", FilterTypes.None);
					CUI.Pair<string, CuiElement, CuiElement> pair12 = cui.CreateProtectedButton(container, pair3, "0.2 0.2 0.2 0.9", Cache.CUI.BlankColor, string.Empty, 0, null, 0.7f, 0.9f, 1f, 1f, 0f, 0f, -130f, -100f, "pluginbrowser.changesetting filter_dd", (TextAnchor)4);
					cui.CreateImage(container, pair12, "fade", Cache.CUI.WhiteColor);
					cui.CreateImage(container, pair12, "sort", "0.8 0.8 0.8 0.6", null, 0.05f, 0.14f, 0.2f, 0.8f);
					cui.CreateText(container, pair12, "0.8 0.8 0.8 0.6", DropdownOptions[storage2], 13, 0.17f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
					if (DropdownShow)
					{
						float num8 = 0f;
						for (int l = 0; l < DropdownOptions.Length; l++)
						{
							float yMax = num8;
							float yMin = num8 - 0.85f;
							CUI.Pair<string, CuiElement, CuiElement> pair13 = cui.CreateProtectedButton(container, pair12, (storage2 == l) ? "0.4 0.4 0.4 0.8" : "0.2 0.2 0.2 0.9", Cache.CUI.BlankColor, string.Empty, 0, null, 0f, 1f, yMin, yMax, 0f, 0f, 0f, 0f, $"pluginbrowser.changesetting filter_dd true call {l}", (TextAnchor)4);
							cui.CreateText(container, pair13, "0.8 0.8 0.8 0.6", DropdownOptions[l], 13, 0.07f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
							num8 -= 0.85f;
						}
					}
					PaginationButtons(cui, container, pair3, orCreatePage, 0.5f, 0.5f, 1f, 1f, -100f, 100f, -130f, -100f);
					if (plugins.Any())
					{
						PaginationButtons(cui, container, pair3, orCreatePage);
					}
					CUI.Pair<string, CuiElement> pair14 = cui.CreatePanel(container, panel, Cache.CUI.BlankColor, null, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f, blur: false, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, "selectedpluginpnl");
					cui.CreatePanel(container, pair14, "0 0 0 0.6", null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, blur: true);
					cui.CreatePanel(container, pair14, "0 0 0 0.6");
					CuiRectTransform contentTransformComponent2;
					CuiScrollbar verticalScrollBar2;
					CUI.Pair<string, CuiElement> pair15 = cui.CreateScrollView(container, pair14, vertical: true, horizontal: false, (MovementType)1, 0.1f, inertia: true, 0.1f, 50f, out contentTransformComponent2, out horizontalScrollBar, out verticalScrollBar2);
					contentTransformComponent2.AnchorMin = "0 0";
					contentTransformComponent2.AnchorMax = "1 1";
					contentTransformComponent2.OffsetMin = "0 -250";
					contentTransformComponent2.OffsetMax = "0 0";
					verticalScrollBar2.Size = 7f;
					verticalScrollBar2.AutoHide = false;
					verticalScrollBar2.Invert = true;
					CUI.Pair<string, CuiElement> pair16 = cui.CreateClientImage(container, pair15, string.Empty, Cache.CUI.BlankColor, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, "selectedpluginicn");
					cui.CreateImage(container, pair16, "hero_fade", "0 0.1 0.2 1");
					cui.CreatePanel(container, pair15, "0 0.1 0.2 1", null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, -1000f);
					cui.CreatePanel(container, pair16, "0 0 0 0.2");
					cui.CreateText(container, pair15, Cache.CUI.WhiteColor, string.Empty, 0, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, "selectedpluginname");
					cui.CreateText(container, pair15, Cache.CUI.WhiteColor, string.Empty, 0, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, "selectedpluginprice");
					cui.CreateText(container, pair15, Cache.CUI.WhiteColor, string.Empty, 0, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, "selectedplugindesc");
					cui.CreateText(container, pair15, "0.8 0.8 0.8 0.6", string.Empty, 0, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, "selectedplugininfo");
					cui.CreatePanel(container, pair15, "0.7 0.7 0.7 0.3", null, 0.05f, 0.95f, 1f, 1f, 0f, 0f, -477f, -475f);
					cui.CreateProtectedButton(container, pair15, Cache.CUI.BlankColor, Cache.CUI.BlankColor, string.Empty, 0, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "pluginbrowser.deselectplugin", (TextAnchor)4);
					CUI.Pair<string, CuiElement> pair17 = cui.CreatePanel(container, pair15, "0.2 0.2 0.2 0.9", null, 0.05f, 0.95f, 1f, 1f, 0f, 0f, -700f, -500f);
					cui.CreateImage(container, pair17, "fade", Cache.CUI.WhiteColor);
					cui.CreateText(container, pair17, "1 1 1 0.8", "VERSION CHANGES", 13, 0.02f, 1f, 0f, 0.95f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
					cui.CreatePanel(container, pair17, "0.7 0.7 0.7 0.2", null, 0.78f, 0.78f, 0.05f, 0.95f, -2f);
					CUI.Pair<string, CuiElement> pair18 = cui.CreatePanel(container, pair17, Cache.CUI.BlankColor, null, 0.78f);
					cui.CreateText(container, pair18, "1 1 1 0.2", "RELEASE DATE", 12, 0f, 1f, 0f, 0.7f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
					cui.CreateText(container, pair18, "0.8 0.8 0.8 0.6", "14 June", 15, 0f, 1f, 0f, 0.8f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, "selectedpluginrdate");
					cui.CreatePanel(container, pair18, "0.7 0.7 0.7 0.2", null, 0.3f, 0.7f, 0.55f, 0.55f, 0f, 0f, -2f);
					cui.CreateText(container, pair18, "1 1 1 0.2", "RATING", 12, 0f, 1f, 0.65f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
					cui.CreateText(container, pair18, Cache.CUI.BlankColor, string.Empty, 0, 0.3f, 0.7f, 0.7f, 0.75f, 0f, 0f, 0f, 0f, (TextAnchor)1, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, "selectedpluginrating");
					CuiRectTransform contentTransformComponent3;
					CuiScrollbar verticalScrollBar3;
					CUI.Pair<string, CuiElement> pair19 = cui.CreateScrollView(container, pair17, vertical: true, horizontal: false, (MovementType)1, 0.1f, inertia: true, 0.1f, 50f, out contentTransformComponent3, out horizontalScrollBar, out verticalScrollBar3, 0.02f, 0.72f, 0.1f, 0.8f);
					contentTransformComponent3.AnchorMin = "0 0";
					contentTransformComponent3.AnchorMax = "1 1";
					contentTransformComponent3.OffsetMin = "0 -100";
					contentTransformComponent3.OffsetMax = "0 0";
					CuiScrollbar cuiScrollbar = verticalScrollBar3;
					command = (verticalScrollBar3.TrackColor = Cache.CUI.BlankColor);
					cuiScrollbar.HandleColor = command;
					verticalScrollBar3.Size = 0f;
					verticalScrollBar3.AutoHide = true;
					verticalScrollBar3.Invert = true;
					cui.CreateText(container, pair19, "1 1 1 0.7", "lorem ipsum", 13, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, "selectedpluginchlog");
					cui.CreateImage(container, pair14, "fade_flip", "0 0 0 1", null, 0f, 1f, 1f, 1f, 0f, 0f, -75f);
					CUI.Pair<string, CuiElement, CuiElement> pair20 = cui.CreateProtectedButton(container, pair14, "0.2 0.2 0.2 0.9", Cache.CUI.BlankColor, string.Empty, 0, null, 0.05f, 0.08f, 0.9f, 0.955f, 0f, 0f, 0f, 0f, "pluginbrowser.deselectplugin", (TextAnchor)4);
					cui.CreateImage(container, pair20, "close", "0.7 0.7 0.7 0.4", null, 0.3f, 0.7f, 0.3f, 0.7f);
					cui.CreateImage(container, pair20, "fade", Cache.CUI.WhiteColor);
					float offset = 0f;
					DrawButton(cui, container, pair14, "selectedplugin_b1", "selectedplugin_b1_icn", "selectedplugin_b1_txt", "selectedplugin_b1_fade", 100f, ref offset);
					DrawButton(cui, container, pair14, "selectedplugin_b2", "selectedplugin_b2_icn", "selectedplugin_b2_txt", "selectedplugin_b2_fade", 90f, ref offset);
					DrawButton(cui, container, pair14, "selectedplugin_b3", "selectedplugin_b3_icn", "selectedplugin_b3_txt", "selectedplugin_b3_fade", 80f, ref offset);
					DrawButton(cui, container, pair14, "selectedplugin_b4", "selectedplugin_b4_icn", "selectedplugin_b4_txt", "selectedplugin_b4_fade", 75f, ref offset);
					DrawButton(cui, container, pair14, "selectedplugin_b5", "selectedplugin_b5_icn", "selectedplugin_b5_txt", "selectedplugin_b5_fade", 80f, ref offset);
					if (vendor is IVendorAuthenticated { IsLoggedIn: not false, User: not null } vendorAuthenticated2 && vendorAuthenticated2.User.PendingAccessToken)
					{
						CUI.Pair<string, CuiElement> pair21 = cui.CreatePanel(container, panel, "0.15 0.15 0.15 0.35", null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, blur: true);
						cui.CreatePanel(container, pair21, "0 0 0 0.9");
						cui.CreateText(container, panel, "1 1 1 1", vendor.Type + " Auth", 25, 0.51f, 1f, 0f, 0.75f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
						cui.CreateText(container, panel, "1 1 1 0.5", "Securely log into your " + vendor.Type + " account through OAuth-based login!\n\nScan the QR code or go to the URL, log into " + vendor.Type + " and type in the provided authentication code below to complete the login process.", 15, 0.51f, 0.9f, 0f, 0.67f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
						cui.CreateText(container, panel, "1 1 1 1", "Authorization code:", 13, 0.51f, 1f, 0f, 0.35f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
						cui.CreateProtectedButton(container, panel, Cache.CUI.BlankColor, Cache.CUI.BlankColor, string.Empty, 0, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "pluginbrowser.closelogin", (TextAnchor)4);
						CUI.Pair<string, CuiElement> pair22 = cui.CreatePanel(container, panel, "1 1 1 1", null, 0.12f, 0.45f, 0.2f, 0.8f);
						string text2 = string.Format(vendorAuthenticated2.AuthRequestEndpoint, vendorAuthenticated2.AuthCode);
						cui.CreateQRCodeImage(container, pair22, text2, vendor.Logo, "0 0 0 1", "1 1 1 1", 15, transparent: true, quietZones: true, "0 0 0 1");
						CUI.Pair<string, CuiElement> pair23 = cui.CreatePanel(container, panel, "0.1 0.1 0.1 1", null, 0.5f, 0.8f, 0.21f, 0.31f);
						CUI.Pair<string, CuiElement> pair24 = cui.CreatePanel(container, pair22, "0.1 0.1 0.1 0.8", null, 0f, 1f, 0f, 0f, 0f, 0f, -20f);
						cui.CreateInputField(container, pair24, "1 1 1 1", vendorAuthenticated2.AuthRequestEndpointPreview, 9, 0, readOnly: true, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, null, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, autoFocus: false, hudMenuInput: false, (LineType)0);
						cui.CreateInputField(container, pair23, "1 1 1 1", vendorAuthenticated2.AuthCode, 30, 0, readOnly: true, 0.05f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, null, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedBold, autoFocus: false, hudMenuInput: false, (LineType)0);
						if (vendorAuthenticated2.User.PendingResult != LoggedInUser.RequestResult.None)
						{
							string text3 = string.Empty;
							string color = string.Empty;
							LoggedInUser.RequestResult pendingResult = vendorAuthenticated2.User.PendingResult;
							if (pendingResult == LoggedInUser.RequestResult.Complete)
							{
								text3 = "checkmark";
								color = "#81c740";
							}
							if (!string.IsNullOrEmpty(text3))
							{
								cui.CreatePanel(container, pair22, "0 0 0 0.4", null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, blur: true);
								cui.CreateImage(container, pair22, text3, color, null, 0.3f, 0.7f, 0.3f, 0.7f);
							}
						}
					}
					Pool.FreeUnmanaged<Plugin>(ref plugins);
				};
			}, "plugins.use");
			CodeflingInstance = new Codefling();
			if (CodeflingInstance is IVendorStored vendorStored && !vendorStored.Load())
			{
				CodeflingInstance.FetchList(delegate
				{
					CodeflingInstance.Refresh();
					CodeflingInstance.VersionCheck();
				});
			}
			InstallUModTab();
			LocalInstance = new Installed();
			LocalInstance.Refresh();
			ServerOwner.Load();
			return TabInstance = tab;
			static void CreateTabButton(CUI cui, CuiElementContainer container, string panel, string text, string subtext, bool isSelected, ref float optionsOffset, bool disabled)
			{
				CUI.Pair<string, CuiElement, CuiElement> pair = cui.CreateProtectedButton(container, panel, isSelected ? "#af3726" : CUI.HexToRustColor("#454239", 0.5f), Cache.CUI.BlankColor, string.Empty, 0, null, 0.05f, 0.05f, 0f, 1f, optionsOffset, optionsOffset + 100f, 0f, 0f, disabled ? string.Empty : ("pluginbrowser.changetab " + text), (TextAnchor)4);
				cui.CreateImage(container, pair, "fade", Cache.CUI.WhiteColor);
				cui.CreateText(container, pair, (!isSelected) ? "1 1 1 0.4" : "1 0.8 0.8 1", text.ToUpper(), 12, 0f, 1f, (isSelected || disabled) ? 0.3f : 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
				if (isSelected)
				{
					cui.CreateText(container, pair, "1 0.8 0.8 0.5", subtext, 10, 0f, 1f, 0f, 0.5f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				}
				if (disabled)
				{
					cui.CreateText(container, pair, "1 0.8 0.8 0.5", "ENABLE VIA COG", 10, 0f, 1f, 0f, 0.5f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				}
				optionsOffset += 102f;
			}
			static void DrawButton(CUI cui, CuiElementContainer container, string parent, string btn, string icn, string txt, string fade, float width, ref float offset)
			{
				CUI.Pair<string, CuiElement, CuiElement> pair = cui.CreateProtectedButton(container, parent, "0.5 0.5 0.5 0.5", Cache.CUI.BlankColor, string.Empty, 0, null, 0.09f, 0.09f, 0.9f, 0.955f, offset, offset + width, 0f, 0f, null, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, btn);
				cui.CreateImage(container, pair, "graph", Cache.CUI.BlankColor, null, 0f, 0f, 0.5f, 0.5f, 5f, 25f, -10f, 10f, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, icn);
				cui.CreateText(container, pair, Cache.CUI.BlankColor, string.Empty, 0, 0.2f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, txt);
				cui.CreateImage(container, pair, "fade", Cache.CUI.BlankColor, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, fade);
				offset += width + 5f;
			}
			static void PaginationButton(ref float pageOffset, CUI cui, CuiElementContainer container, string parent, string text, bool disabled = false, string command = null)
			{
				CUI.Pair<string, CuiElement, CuiElement> pair = cui.CreateProtectedButton(container, parent, "0.2 0.2 0.2 0.9", "0.7 0.7 0.7 0.4", text, 10, null, 0f, 0f, 0f, 1f, pageOffset, pageOffset + 30f, 0f, 0f, command, (TextAnchor)4);
				cui.CreateImage(container, pair, "fade", Cache.CUI.WhiteColor);
				pageOffset += 35f;
			}
			static void PaginationButtons(CUI cui, CuiElementContainer container, string parent, PlayerSession.Page page, float xMin = 0.5f, float xMax = 0.5f, float yMin = 0f, float yMax = 0f, float oXMin = -100f, float oXMax = 100f, float oYMin = 10f, float oYMax = 40f)
			{
				float pageOffset = 0f;
				CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, parent, Cache.CUI.BlankColor, null, xMin, xMax, yMin, yMax, -100f, 100f, oYMin, oYMax);
				PaginationButton(ref pageOffset, cui, container, pair, "<<", disabled: false, "pluginbrowser.page -2");
				PaginationButton(ref pageOffset, cui, container, pair, "<", disabled: false, "pluginbrowser.page -1");
				CUI.Pair<string, CuiElement> pair2 = cui.CreatePanel(container, pair, "0.2 0.2 0.2 0.5", null, 0f, 0f, 0f, 1f, pageOffset, pageOffset + 70f);
				cui.CreateImage(container, pair2, "fade", Cache.CUI.WhiteColor);
				cui.CreateText(container, pair2, "0.4 0.4 0.4 0.8", $"/ {page.TotalPages:n0}", 10, 0.5f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				cui.CreateProtectedInputField(container, pair2, Cache.CUI.WhiteColor, $"{page.CurrentPage + 1}", 10, 60, readOnly: false, 0f, 0.45f, 0f, 1f, 0f, 0f, 0f, 0f, "pluginbrowser.page", (TextAnchor)5, CUI.Handler.FontTypes.RobotoCondensedRegular, autoFocus: false, hudMenuInput: false, (LineType)0);
				pageOffset += 75f;
				PaginationButton(ref pageOffset, cui, container, pair, ">", disabled: false, "pluginbrowser.page 1");
				PaginationButton(ref pageOffset, cui, container, pair, ">>", disabled: false, "pluginbrowser.page -3");
			}
		}

		public static void InstallUModTab()
		{
			if (Singleton.DataInstance.DisableUMod)
			{
				if (uModInstance is uMod uMod2)
				{
					uMod2.Dispose();
				}
				uModInstance = null;
				return;
			}
			uModInstance = new uMod();
			if (uModInstance is IVendorStored vendorStored && !vendorStored.Load())
			{
				uModInstance.FetchList(delegate
				{
					uModInstance.Refresh();
					uModInstance.VersionCheck();
				});
			}
		}

		public static List<Plugin> GetPlugins(Vendor vendor, Tab tab, PlayerSession ap, int pluginCount)
		{
			int maxPages;
			return GetPlugins(vendor, tab, ap, out maxPages, pluginCount);
		}

		public static List<Plugin> GetPlugins(Vendor vendor, Tab tab, PlayerSession ap, out int maxPages, int pluginCount)
		{
			maxPages = 0;
			List<Plugin> list = Pool.Get<List<Plugin>>();
			List<Plugin> list2 = Pool.Get<List<Plugin>>();
			using (TimeMeasure.New("GetPluginsFromVendor"))
			{
				try
				{
					FilterTypes storage = ap.GetStorage(tab, "filter", FilterTypes.None);
					bool storage2 = ap.GetStorage(tab, "flipfilter", @default: false);
					IEnumerable<Plugin> enumerable2;
					switch (storage)
					{
					case FilterTypes.Price:
						enumerable2 = (storage2 ? vendor.PriceData.Reverse() : vendor.PriceData);
						break;
					case FilterTypes.Author:
						enumerable2 = (storage2 ? vendor.AuthorData.Reverse() : vendor.AuthorData);
						break;
					case FilterTypes.Installed:
						enumerable2 = (storage2 ? vendor.InstalledData.Reverse() : vendor.InstalledData);
						break;
					case FilterTypes.OutOfDate:
						enumerable2 = (storage2 ? vendor.OutOfDateData.Reverse() : vendor.OutOfDateData);
						break;
					case FilterTypes.Owned:
						enumerable2 = (storage2 ? vendor.OwnedData.Reverse() : vendor.OwnedData);
						break;
					default:
					{
						IEnumerable<Plugin> enumerable;
						if (!storage2)
						{
							IEnumerable<Plugin> fetchedPlugins = vendor.FetchedPlugins;
							enumerable = fetchedPlugins;
						}
						else
						{
							enumerable = vendor.FetchedPlugins.AsEnumerable().Reverse();
						}
						enumerable2 = enumerable;
						break;
					}
					}
					IEnumerable<Plugin> enumerable3 = enumerable2;
					string storage3 = ap.GetStorage<string>(tab, "search");
					if (!string.IsNullOrEmpty(storage3))
					{
						foreach (Plugin item in enumerable3)
						{
							if (item.Status != Status.Approved || (item.ExistentPlugin != null && item.ExistentPlugin.IsPrecompiled))
							{
								continue;
							}
							if (storage == FilterTypes.Favourites)
							{
								if (ServerOwner.Singleton.FavouritePlugins.Contains(item.Name))
								{
									list2.Add(item);
								}
							}
							else if (item.Id == storage3)
							{
								list2.Add(item);
							}
							else if (item.Name.ToLower().Trim().Contains(storage3.ToLower().Trim()))
							{
								list2.Add(item);
							}
							else if (item.Author.ToLower().Trim().Contains(storage3.ToLower().Trim()))
							{
								list2.Add(item);
							}
							else
							{
								if (TagFilter.Count <= 0 || item.Tags == null)
								{
									continue;
								}
								bool flag = false;
								foreach (string tag in item.Tags)
								{
									if (TagFilter.Contains(tag))
									{
										flag = true;
										break;
									}
								}
								if (flag)
								{
									list2.Add(item);
								}
							}
						}
					}
					else
					{
						foreach (Plugin item2 in enumerable3)
						{
							if (item2.Status != Status.Approved || (item2.ExistentPlugin != null && item2.ExistentPlugin.IsPrecompiled))
							{
								continue;
							}
							if (storage == FilterTypes.Favourites)
							{
								if (ServerOwner.Singleton.FavouritePlugins.Contains(item2.Name))
								{
									list2.Add(item2);
								}
							}
							else if (TagFilter.Count > 0 && item2.Tags != null)
							{
								bool flag2 = false;
								foreach (string tag2 in item2.Tags)
								{
									if (TagFilter.Contains(tag2))
									{
										flag2 = true;
										break;
									}
								}
								if (flag2)
								{
									list2.Add(item2);
								}
							}
							else
							{
								list2.Add(item2);
							}
						}
					}
					enumerable3 = null;
					maxPages = (list2.Count - 1) / pluginCount;
					int storage4 = ap.GetStorage(tab, "page", 0);
					if (storage4 > maxPages)
					{
						ap.SetStorage(tab, "page", maxPages);
					}
					int num = pluginCount * storage4;
					int num2 = (num + pluginCount).Clamp(0, list2.Count);
					if (num2 > 0)
					{
						for (int i = num; i < num2; i++)
						{
							try
							{
								list.Add(list2[i]);
							}
							catch
							{
								break;
							}
						}
					}
				}
				catch (Exception ex)
				{
					Pool.FreeUnmanaged<Plugin>(ref list);
					Logger.Error("Failed getting plugins.", ex);
				}
				Pool.FreeUnmanaged<Plugin>(ref list2);
				return list;
			}
		}

		public static void DownloadThumbnails(Vendor vendor, Tab tab, PlayerSession ap)
		{
			List<Plugin> plugins = GetPlugins(vendor, tab, ap, 15);
			List<string> list = Pool.Get<List<string>>();
			List<string> list2 = Pool.Get<List<string>>();
			foreach (Plugin item in plugins)
			{
				if (!Singleton.DataInstance.HidePluginIcons && !item.HasNoImage())
				{
					if (item.HasInvalidImage())
					{
						list2.Add(item.Image);
					}
					else
					{
						list.Add(item.Image);
					}
				}
			}
			bool flag = false;
			if (list.Count > 0)
			{
				Singleton.ImageDatabase.QueueBatch(flag, list);
			}
			if (list2.Count > 0)
			{
				Singleton.ImageDatabase.QueueBatch(flag, list2);
			}
			Pool.FreeUnmanaged<Plugin>(ref plugins);
			Pool.FreeUnmanaged<string>(ref list);
			Pool.FreeUnmanaged<string>(ref list2);
		}
	}

	public class ProfilerTab : Tab
	{
		public enum SubtabTypes
		{
			Calls,
			Memory
		}

		public static MonoProfiler.Sample sample;

		internal static ProfilerTab _instance;

		internal static Color intenseColor;

		internal static Color calmColor;

		internal static Color niceColor;

		internal static MonoProfiler.TimelineRecording recording = new MonoProfiler.TimelineRecording();

		public static readonly Color[] ChartColors = new Color[10]
		{
			Color.Tomato,
			Color.MediumVioletRed,
			Color.Violet,
			Color.SteelBlue,
			Color.SlateBlue,
			Color.Orange,
			Color.LightSeaGreen,
			Color.Red,
			Color.Chocolate,
			Color.DarkCyan
		};

		internal static string[] timelineChartOptions = new string[13]
		{
			"Assembly Calls", "Assembly Memory", "Assembly Time", "Assembly Exceptions", "Calls", "Call Time (Total)", "Call Time (Own)", "Call Memory (Total)", "Call Memory (Own)", "Call Exceptions (Total)",
			"Call Exceptions (Own)", "Memory Allocs", "Memory Allocs (Memory)"
		};

		internal static string[] sortAssemblyOptions = new string[5] { "Name", "Time", "Calls", "Memory", "Exceptions" };

		internal static string[] sortCallsOptions = new string[8] { "Method", "Calls", "Time (Total)", "Time (Own)", "Memory (Total)", "Memory (Own)", "Exceptions (Total)", "Exceptions (Own)" };

		internal static string[] sortMemoryOptions = new string[3] { "Type", "Allocations", "Memory" };

		public ProfilerTab(string id, string name, RustPlugin plugin, Action<PlayerSession, Tab> onChange = null)
			: base(id, name, plugin, onChange)
		{
			ColorUtility.TryParseHtmlString("#d13b38", ref intenseColor);
			ColorUtility.TryParseHtmlString("#3882d1", ref calmColor);
			ColorUtility.TryParseHtmlString("#60a848", ref niceColor);
		}

		public static ProfilerTab GetOrCache(PlayerSession session)
		{
			return _instance ?? (_instance = Make(session));
		}

		public static ProfilerTab Make(PlayerSession session)
		{
			if (sample.Assemblies == null)
			{
				sample = MonoProfiler.Sample.Create();
				sample.Clear();
			}
			ProfilerTab profiler = new ProfilerTab("profiler", "Profiler", Community.Runtime.Core);
			profiler.OnChange = delegate(PlayerSession ap, Tab _)
			{
				profiler.Draw(ap);
			};
			profiler.Over = delegate(Tab _, CUI cui, CuiElementContainer container, string parent, PlayerSession _)
			{
				string text = (MonoProfiler.Crashed ? "<b>Mono profiler has failed initializing properly</b>\nPlease ensure CarbonNative.dll is located in <b>carbon/native</b> or contact developers" : ((!MonoProfiler.Enabled) ? "<b>Mono profiler is disabled</b>\nEnable it in the config, then reboot the server" : null));
				if (!string.IsNullOrEmpty(text))
				{
					CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, parent, "0 0 0 0.5", null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, blur: true);
					cui.CreateText(container, pair, "1 1 1 0.5", text, 10, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				}
			};
			profiler.Draw(session);
			return profiler;
		}

		public static IEnumerable<MonoProfiler.AssemblyRecord> GetSortedAssemblies(int sort, string search)
		{
			if (sample.Assemblies == null)
			{
				return null;
			}
			return (sort switch
			{
				0 => sample.Assemblies.OrderBy((MonoProfiler.AssemblyRecord x) => x.assembly_name.GetDisplayName(sample.IsCleared)), 
				1 => sample.Assemblies.OrderByDescending((MonoProfiler.AssemblyRecord x) => x.total_time), 
				2 => sample.Assemblies.OrderByDescending((MonoProfiler.AssemblyRecord x) => x.calls), 
				3 => sample.Assemblies.OrderByDescending((MonoProfiler.AssemblyRecord x) => x.alloc), 
				4 => sample.Assemblies.OrderByDescending((MonoProfiler.AssemblyRecord x) => x.total_exceptions), 
				_ => null, 
			}).Where((MonoProfiler.AssemblyRecord x) => string.IsNullOrEmpty(search) || StringEx.Contains(x.assembly_name.GetDisplayName(sample.IsCleared), search, CompareOptions.OrdinalIgnoreCase));
		}

		public static IEnumerable<MonoProfiler.CallRecord> GetSortedCalls(string assembly, int sort, string search)
		{
			if (sample.Calls == null)
			{
				return null;
			}
			IEnumerable<MonoProfiler.CallRecord> enumerable = sample.Calls.Where((MonoProfiler.CallRecord x) => string.IsNullOrEmpty(assembly) || x.assembly_name.name == assembly);
			if (!enumerable.Any())
			{
				return enumerable;
			}
			return (sort switch
			{
				0 => enumerable.OrderBy((MonoProfiler.CallRecord x) => x.method_name), 
				1 => enumerable.OrderByDescending((MonoProfiler.CallRecord x) => x.calls), 
				2 => enumerable.OrderByDescending((MonoProfiler.CallRecord x) => x.total_time), 
				3 => enumerable.OrderByDescending((MonoProfiler.CallRecord x) => x.own_time), 
				4 => enumerable.OrderByDescending((MonoProfiler.CallRecord x) => x.total_alloc), 
				5 => enumerable.OrderByDescending((MonoProfiler.CallRecord x) => x.own_alloc), 
				6 => enumerable.OrderByDescending((MonoProfiler.CallRecord x) => x.total_exceptions), 
				7 => enumerable.OrderByDescending((MonoProfiler.CallRecord x) => x.own_exceptions), 
				_ => enumerable, 
			}).Where((MonoProfiler.CallRecord x) => string.IsNullOrEmpty(search) || StringEx.Contains(x.method_name, search, CompareOptions.OrdinalIgnoreCase));
		}

		public static IEnumerable<MonoProfiler.MemoryRecord> GetSortedMemory(int sort, string search)
		{
			if (sample.Memory == null)
			{
				return null;
			}
			IEnumerable<MonoProfiler.MemoryRecord> enumerable = sample.Memory.AsEnumerable();
			if (!enumerable.Any())
			{
				return enumerable;
			}
			return (sort switch
			{
				0 => enumerable.OrderBy((MonoProfiler.MemoryRecord x) => x.class_name), 
				1 => enumerable.OrderByDescending((MonoProfiler.MemoryRecord x) => x.allocations), 
				2 => enumerable.OrderByDescending((MonoProfiler.MemoryRecord x) => x.total_alloc_size), 
				_ => enumerable, 
			}).Where((MonoProfiler.MemoryRecord x) => string.IsNullOrEmpty(search) || StringEx.Contains(x.class_name, search, CompareOptions.OrdinalIgnoreCase));
		}

		internal void Draw(PlayerSession ap)
		{
			string storage = ap.GetStorage<string>(null, "profilerval");
			DrawSubtabs(ap, storage);
			DrawAssemblies(ap, storage);
		}

		private static void Stripe(Tab tab, int column, float value, float maxValue, Color intenseColor, Color calmColor, string title, string subtitle, string side, string command, bool selected = false)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			if (maxValue <= value)
			{
				maxValue = value;
			}
			tab.AddWidget(column, 0, delegate(PlayerSession ap, CUI cui, CuiElementContainer container, string parent)
			{
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				//IL_0027: Unknown result type (might be due to invalid IL or missing references)
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Unknown result type (might be due to invalid IL or missing references)
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_003d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0121: Unknown result type (might be due to invalid IL or missing references)
				//IL_0122: Unknown result type (might be due to invalid IL or missing references)
				float num = value.Scale(0f, maxValue, 0f, 1f);
				Color32 val = Color32.Lerp(Color32.op_Implicit(calmColor), Color32.op_Implicit(intenseColor), num);
				string parent2 = (string.IsNullOrEmpty(command) ? cui.CreatePanel(container, parent, "0.15 0.15 0.15 0.7", null, 0.01f, 0.99f).Id : cui.CreateProtectedButton(container, parent, "0.15 0.15 0.15 0.7", Cache.CUI.BlankColor, string.Empty, 0, null, 0.01f, 0.99f, 0f, 1f, 0f, 0f, 0f, 0f, command, (TextAnchor)4).Id);
				CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, parent2, "#" + ColorUtility.ToHtmlStringRGB(Color32.op_Implicit(val)), null, 0f, num);
				if (selected)
				{
					cui.CreatePanel(container, parent2, "#d13b38", null, 0f, 0.005f);
				}
				cui.CreateText(container, parent2, Cache.CUI.WhiteColor, title, 9, selected ? 0.02f : 0.01f, 1f, 0f, 0.9f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				cui.CreateText(container, parent2, "1 1 1 0.6", subtitle, 8, selected ? 0.02f : 0.01f, 1f, 0.05f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)6, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				cui.CreateText(container, parent2, "1 1 1 0.2", side, 8, 0f, 0.99f, 0.05f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)5, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				cui.CreateImage(container, pair, "fade", Cache.CUI.WhiteColor);
			});
		}

		public void DrawAssemblies(PlayerSession session, string assembly)
		{
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
			AddColumn(0, clear: true);
			bool timelineMode = session.GetStorage(null, "timeline", @default: false);
			if (timelineMode)
			{
				DrawTimeline(session);
				return;
			}
			string searchInput = session.GetStorage(this, "bsearch", string.Empty);
			int sortIndex = session.GetStorage(this, "bsort", 1);
			List<MonoProfiler.AssemblyRecord> list = Pool.Get<List<MonoProfiler.AssemblyRecord>>();
			float num = 0f;
			list.AddRange(GetSortedAssemblies(sortIndex, searchInput));
			if (list.Count > 0)
			{
				float num2;
				switch (sortIndex)
				{
				case 0:
				case 1:
					num2 = ((IEnumerable<MonoProfiler.AssemblyRecord>)list).Max((Func<MonoProfiler.AssemblyRecord, float>)((MonoProfiler.AssemblyRecord x) => x.total_time));
					break;
				case 2:
					num2 = ((IEnumerable<MonoProfiler.AssemblyRecord>)list).Max((Func<MonoProfiler.AssemblyRecord, float>)((MonoProfiler.AssemblyRecord x) => x.calls));
					break;
				case 3:
					num2 = ((IEnumerable<MonoProfiler.AssemblyRecord>)list).Max((Func<MonoProfiler.AssemblyRecord, float>)((MonoProfiler.AssemblyRecord x) => x.alloc));
					break;
				case 4:
					num2 = ((IEnumerable<MonoProfiler.AssemblyRecord>)list).Max((Func<MonoProfiler.AssemblyRecord, float>)((MonoProfiler.AssemblyRecord x) => x.total_exceptions));
					break;
				default:
					num2 = num;
					break;
				}
				num = num2;
			}
			AddWidget(-1, 0, delegate(PlayerSession ap, CUI cui, CuiElementContainer container, string panel)
			{
				int num4 = 1;
				cui.CreateProtectedButton(container, panel, "0.2 0.2 0.2 0.7", $"1 1 1 {((!timelineMode) ? 0.2 : 0.5)}", "TIMELINE\nMODE", 8, null, 0.83f, 0.925f, 0f, 1f, -46f * (float)num4, -46f * (float)num4, 0f, 0f, "adminmodule.timelinemode", (TextAnchor)4);
				num4++;
				cui.CreateProtectedButton(container, panel, "0.2 0.2 0.2 0.7", $"1 1 1 {(sample.IsCompared ? 0.2 : 0.5)}", "<size=6>" + ((!sample.IsCleared) ? "COMPARE" : "IMPORT") + "\n</size>PROTO", 8, null, 0.83f, 0.925f, 0f, 1f, -46f * (float)num4, -46f * (float)num4, 0f, 0f, "adminmodule.profilerimport", (TextAnchor)4);
				num4++;
				cui.CreateProtectedButton(container, panel, "0.2 0.2 0.2 0.7", $"1 1 1 {(sample.IsCleared ? 0.2 : 0.5)}", "<size=6>EXPORT\n</size>PROTO", 8, null, 0.83f, 0.925f, 0f, 1f, -46f * (float)num4, -46f * (float)num4, 0f, 0f, "adminmodule.profilerexport 3", (TextAnchor)4);
				num4++;
				cui.CreateProtectedButton(container, panel, "0.2 0.2 0.2 0.7", $"1 1 1 {(sample.IsCleared ? 0.2 : 0.5)}", "<size=6>EXPORT\n</size>CSV", 8, null, 0.83f, 0.925f, 0f, 1f, -46f * (float)num4, -46f * (float)num4, 0f, 0f, "adminmodule.profilerexport 2", (TextAnchor)4);
				num4++;
				cui.CreateProtectedButton(container, panel, "0.2 0.2 0.2 0.7", $"1 1 1 {(sample.IsCleared ? 0.2 : 0.5)}", "<size=6>EXPORT\n</size>JSON", 8, null, 0.83f, 0.925f, 0f, 1f, -46f * (float)num4, -46f * (float)num4, 0f, 0f, "adminmodule.profilerexport 1", (TextAnchor)4);
				num4++;
				cui.CreateProtectedButton(container, panel, "0.2 0.2 0.2 0.7", $"1 1 1 {(sample.IsCleared ? 0.2 : 0.5)}", "<size=6>EXPORT\n</size>TABLE", 8, null, 0.83f, 0.925f, 0f, 1f, -46f * (float)num4, -46f * (float)num4, 0f, 0f, "adminmodule.profilerexport 0", (TextAnchor)4);
				num4++;
				cui.CreateProtectedButton(container, panel, (!sample.IsCleared || MonoProfiler.IsRecording) ? "0.9 0.1 0.1 1" : "0.2 0.2 0.2 0.7", "1 1 1 0.5", MonoProfiler.IsRecording ? "ABORT" : "CLEAR", 8, null, 0.83f, 0.925f, 0f, 1f, 0f, 0f, 0f, 0f, "adminmodule.profilerclear", (TextAnchor)4);
				cui.CreateProtectedButton(container, panel, MonoProfiler.IsRecording ? "0.9 0.1 0.1 1" : "0.2 0.2 0.2 0.7", "1 1 1 0.5", "REC<size=6>\n[SHIFT]</size>", 8, null, 0.93f, 0.99f, 0f, 1f, 0f, 0f, 0f, 0f, "adminmodule.profilertoggle", (TextAnchor)4);
			});
			Stripe(this, 0, (float)list.Sum((MonoProfiler.AssemblyRecord x) => x.total_time_percentage), 100f, niceColor, niceColor, "All", $"{list.Sum((MonoProfiler.AssemblyRecord x) => (float)x.total_time_ms):n0}ms | {list.Sum((MonoProfiler.AssemblyRecord x) => (float)x.total_time_percentage):0.0}%", $"<size=7>{MonoProfiler.Sample.GetDifferenceString(sample.Comparison.Duration)}{TimeEx.Format(sample.Duration, shortName: false).ToLower()}\n{sample.Calls.Count:n0} calls</size>", "adminmodule.profilerselect -1", string.IsNullOrEmpty(assembly));
			AddDropdown(0, $"<b>ASSEMBLIES ({sample.Assemblies.Count:n0})</b>", (PlayerSession ap) => sortIndex, delegate(PlayerSession ap, int i)
			{
				ap.SetStorage(this, "bsort", i);
				DrawAssemblies(session, assembly);
			}, sortAssemblyOptions);
			AddInputButton(0, "Search", 0.075f, new OptionInput(null, (PlayerSession ap) => searchInput, 0, readOnly: false, delegate(PlayerSession ap, object[] args)
			{
				ap.SetStorage(this, "bsearch", args.Select((object x) => x as string).ToString(" "));
				DrawAssemblies(ap, assembly);
			}), new OptionButton("X", delegate(PlayerSession ap)
			{
				ap.SetStorage(this, "bsearch", string.Empty);
				DrawAssemblies(ap, assembly);
			}, (PlayerSession _) => (!string.IsNullOrEmpty(searchInput)) ? OptionButton.Types.Important : OptionButton.Types.None));
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				MonoProfiler.AssemblyRecord assemblyRecord = list[num3];
				float num2;
				switch (sortIndex)
				{
				case 0:
				case 1:
					num2 = assemblyRecord.total_time;
					break;
				case 2:
					num2 = assemblyRecord.calls;
					break;
				case 3:
					num2 = assemblyRecord.alloc;
					break;
				case 4:
					num2 = assemblyRecord.total_exceptions;
					break;
				default:
					num2 = 0f;
					break;
				}
				float value = num2;
				Stripe(this, 0, value, num, intenseColor, calmColor, assemblyRecord.assembly_name.GetDisplayName(assemblyRecord.comparison.isCompared), string.Format("{0}{1} ({2:0.0}%) | {3}{4} | {5}{6:n0} excep.", new object[7]
				{
					MonoProfiler.Sample.GetDifferenceString(assemblyRecord.comparison.total_time),
					assemblyRecord.GetTotalTime(),
					assemblyRecord.total_time_percentage,
					MonoProfiler.Sample.GetDifferenceString(assemblyRecord.comparison.alloc),
					assemblyRecord.alloc.Format().ToUpper(),
					MonoProfiler.Sample.GetDifferenceString(assemblyRecord.comparison.total_exceptions),
					assemblyRecord.total_exceptions
				}), $"{assemblyRecord.assembly_name.profileType}\n{MonoProfiler.Sample.GetDifferenceString(assemblyRecord.comparison.calls)}<b>{assemblyRecord.calls:n0}</b> calls", $"adminmodule.profilerselect {num3}", assemblyRecord.assembly_name.name == assembly);
			}
			if (list.Count == 0)
			{
				AddText(0, "No assemblies available", 8, "1 1 1 0.5", (TextAnchor)4);
			}
			Pool.FreeUnmanaged<MonoProfiler.AssemblyRecord>(ref list);
		}

		public void DrawSubtabs(PlayerSession session, string assembly)
		{
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_0790: Unknown result type (might be due to invalid IL or missing references)
			//IL_0795: Unknown result type (might be due to invalid IL or missing references)
			//IL_0381: Unknown result type (might be due to invalid IL or missing references)
			//IL_0386: Unknown result type (might be due to invalid IL or missing references)
			AddColumn(1, clear: true);
			if (session.GetStorage(null, "timeline", @default: false))
			{
				int timelineChartType = session.GetStorage(this, "timelinect", 0);
				AddSpace(1);
				AddDropdown(1, "Chart Options", (PlayerSession ap) => timelineChartType, delegate(PlayerSession ap, int i)
				{
					ap.SetStorage(this, "timelinect", i);
					DrawSubtabs(session, assembly);
					DrawAssemblies(session, assembly);
				}, timelineChartOptions);
				return;
			}
			SubtabTypes subtab = session.GetStorage(this, "subtab", SubtabTypes.Calls);
			AddButtonArray(-2, new OptionButton("Calls", delegate
			{
				session.SetStorage(this, "subtab", SubtabTypes.Calls);
				DrawSubtabs(session, assembly);
			}, (PlayerSession ap) => (subtab == SubtabTypes.Calls) ? OptionButton.Types.Selected : OptionButton.Types.None), new OptionButton("Memory", delegate
			{
				session.SetStorage(this, "subtab", SubtabTypes.Memory);
				DrawSubtabs(session, assembly);
			}, (PlayerSession ap) => (subtab == SubtabTypes.Memory) ? OptionButton.Types.Selected : OptionButton.Types.None));
			Stripe(this, 1, 100f, 100f, niceColor, niceColor, "GC", string.Format("{0}{1:n0} calls | {2}{3}", new object[4]
			{
				MonoProfiler.Sample.GetDifferenceString(sample.GC.comparison.calls_c),
				sample.GC.calls,
				MonoProfiler.Sample.GetDifferenceString(sample.GC.comparison.total_time_c),
				sample.GC.GetTotalTime()
			}), string.Empty, null, selected: true);
			switch (subtab)
			{
			case SubtabTypes.Memory:
			{
				string searchInput2 = session.GetStorage(this, "msearch", string.Empty);
				int sort2 = session.GetStorage(this, "msort", 1);
				IEnumerable<MonoProfiler.MemoryRecord> sortedMemory = GetSortedMemory(sort2, searchInput2);
				float num4 = 0f;
				if (sortedMemory.Any())
				{
					float num2;
					switch (sort2)
					{
					case 0:
					case 1:
						num2 = sortedMemory.Max((Func<MonoProfiler.MemoryRecord, float>)((MonoProfiler.MemoryRecord x) => x.allocations));
						break;
					case 2:
						num2 = sortedMemory.Max((Func<MonoProfiler.MemoryRecord, float>)((MonoProfiler.MemoryRecord x) => x.total_alloc_size));
						break;
					default:
						num2 = num4;
						break;
					}
					num4 = num2;
				}
				AddDropdown(1, $"<b>MEMORY ({sortedMemory.Count():n0})</b>", (PlayerSession ap) => sort2, delegate(PlayerSession ap, int i)
				{
					ap.SetStorage(this, "msort", i);
					DrawSubtabs(session, assembly);
				}, sortMemoryOptions);
				AddInputButton(1, "Search", 0.075f, new OptionInput(null, (PlayerSession ap) => searchInput2, 0, readOnly: false, delegate(PlayerSession ap, object[] args)
				{
					ap.SetStorage(this, "msearch", args.Select((object x) => x as string).ToString(" "));
					DrawSubtabs(ap, assembly);
				}), new OptionButton("X", delegate(PlayerSession ap)
				{
					ap.SetStorage(this, "msearch", string.Empty);
					DrawSubtabs(ap, assembly);
				}, (PlayerSession _) => (!string.IsNullOrEmpty(searchInput2)) ? OptionButton.Types.Important : OptionButton.Types.None));
				int num5 = 0;
				foreach (MonoProfiler.MemoryRecord item in sortedMemory)
				{
					float num2;
					switch (sort2)
					{
					case 0:
					case 1:
						num2 = item.allocations;
						break;
					case 2:
						num2 = item.total_alloc_size;
						break;
					default:
						num2 = 0f;
						break;
					}
					float value2 = num2;
					Stripe(this, 1, value2, num4, intenseColor, calmColor, item.class_name, string.Format("{0}{1:n0} allocated | {2}{3} total", new object[4]
					{
						MonoProfiler.Sample.GetDifferenceString(item.comparison.allocations),
						item.allocations,
						MonoProfiler.Sample.GetDifferenceString(item.comparison.total_alloc_size),
						item.total_alloc_size.Format().ToUpper()
					}), $"<b>{item.instance_size} B</b>", string.Empty);
					num5++;
				}
				if (!sortedMemory.Any())
				{
					AddText(1, "No memory records available", 8, "1 1 1 0.5", (TextAnchor)4);
				}
				break;
			}
			case SubtabTypes.Calls:
			{
				string searchInput = session.GetStorage(this, "asearch", string.Empty);
				int sort = session.GetStorage(this, "asort", 1);
				IEnumerable<MonoProfiler.CallRecord> sortedCalls = GetSortedCalls(assembly, sort, searchInput);
				float num = 0f;
				if (sortedCalls.Any())
				{
					float num2;
					switch (sort)
					{
					case 0:
					case 1:
						num2 = sortedCalls.Max((Func<MonoProfiler.CallRecord, float>)((MonoProfiler.CallRecord x) => x.calls));
						break;
					case 2:
						num2 = sortedCalls.Max((Func<MonoProfiler.CallRecord, float>)((MonoProfiler.CallRecord x) => x.total_time));
						break;
					case 3:
						num2 = sortedCalls.Max((Func<MonoProfiler.CallRecord, float>)((MonoProfiler.CallRecord x) => x.own_time));
						break;
					case 4:
						num2 = sortedCalls.Max((Func<MonoProfiler.CallRecord, float>)((MonoProfiler.CallRecord x) => x.total_alloc));
						break;
					case 5:
						num2 = sortedCalls.Max((Func<MonoProfiler.CallRecord, float>)((MonoProfiler.CallRecord x) => x.own_alloc));
						break;
					case 6:
						num2 = sortedCalls.Max((Func<MonoProfiler.CallRecord, float>)((MonoProfiler.CallRecord x) => x.total_exceptions));
						break;
					case 7:
						num2 = sortedCalls.Max((Func<MonoProfiler.CallRecord, float>)((MonoProfiler.CallRecord x) => x.own_exceptions));
						break;
					default:
						num2 = num;
						break;
					}
					num = num2;
				}
				AddDropdown(1, $"<b>CALLS ({sortedCalls.Count():n0})</b>", (PlayerSession ap) => sort, delegate(PlayerSession ap, int i)
				{
					ap.SetStorage(this, "asort", i);
					DrawSubtabs(session, assembly);
				}, sortCallsOptions);
				AddInputButton(1, "Search", 0.075f, new OptionInput(null, (PlayerSession ap) => searchInput, 0, readOnly: false, delegate(PlayerSession ap, object[] args)
				{
					ap.SetStorage(this, "asearch", args.Select((object x) => x as string).ToString(" "));
					DrawSubtabs(ap, assembly);
				}), new OptionButton("X", delegate(PlayerSession ap)
				{
					ap.SetStorage(this, "asearch", string.Empty);
					DrawSubtabs(ap, assembly);
				}, (PlayerSession _) => (!string.IsNullOrEmpty(searchInput)) ? OptionButton.Types.Important : OptionButton.Types.None));
				int num3 = 0;
				foreach (MonoProfiler.CallRecord item2 in sortedCalls)
				{
					float num2;
					switch (sort)
					{
					case 0:
					case 1:
						num2 = item2.calls;
						break;
					case 2:
						num2 = item2.total_time;
						break;
					case 3:
						num2 = item2.own_time;
						break;
					case 4:
						num2 = item2.total_alloc;
						break;
					case 5:
						num2 = item2.own_alloc;
						break;
					case 6:
						num2 = item2.total_exceptions;
						break;
					case 7:
						num2 = item2.own_exceptions;
						break;
					default:
						num2 = 0f;
						break;
					}
					float value = num2;
					Stripe(this, 1, value, num, intenseColor, calmColor, item2.method_name.Truncate(105, "..."), string.Format("{0}{1} total ({2:0.0}%) | {3}{4} own ({5:0.0}%) | {6}{7:n0} total / {8}{9:n0} own excep.", new object[10]
					{
						MonoProfiler.Sample.GetDifferenceString(item2.comparison.total_time),
						item2.GetTotalTime(),
						item2.total_time_percentage,
						MonoProfiler.Sample.GetDifferenceString(item2.comparison.own_time),
						item2.GetOwnTime(),
						item2.own_time_percentage,
						MonoProfiler.Sample.GetDifferenceString(item2.comparison.total_exceptions),
						item2.total_exceptions,
						MonoProfiler.Sample.GetDifferenceString(item2.comparison.own_exceptions),
						item2.own_exceptions
					}), string.Format("{0}<b>{1:n0}</b> {2}\n{3}{4} total | {5}{6} own", new object[7]
					{
						MonoProfiler.Sample.GetDifferenceString(item2.comparison.calls),
						item2.calls,
						item2.calls.Plural("call", "calls"),
						MonoProfiler.Sample.GetDifferenceString(item2.comparison.total_alloc),
						item2.total_alloc.Format().ToUpper(),
						MonoProfiler.Sample.GetDifferenceString(item2.comparison.own_alloc),
						item2.own_alloc.Format().ToUpper()
					}), (Community.Runtime.MonoProfilerConfig.SourceViewer && !sample.FromDisk) ? $"adminmodule.profilerselectcall {num3}" : string.Empty);
					num3++;
				}
				if (!sortedCalls.Any())
				{
					AddText(1, "No call records available", 8, "1 1 1 0.5", (TextAnchor)4);
				}
				break;
			}
			}
		}

		public void DrawTimeline(PlayerSession session)
		{
			//IL_07a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_07a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0945: Unknown result type (might be due to invalid IL or missing references)
			//IL_094a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c04: Unknown result type (might be due to invalid IL or missing references)
			int storage = session.GetStorage(this, "timelinect", 0);
			AddWidget(0, 0, delegate(PlayerSession ap, CUI cui, CuiElementContainer container, string panel)
			{
				int num5 = 1;
				cui.CreateProtectedButton(container, panel, "0.2 0.2 0.2 0.7", "1 1 1 0.5", "TIMELINE\nMODE", 8, null, 0.83f, 0.925f, 0f, 1f, -46f * (float)num5, -46f * (float)num5, 0f, 0f, "adminmodule.timelinemode", (TextAnchor)4);
				num5++;
				cui.CreateProtectedButton(container, panel, (!recording.IsDiscarded() || recording.IsRecording()) ? "0.9 0.1 0.1 1" : "0.2 0.2 0.2 0.7", "1 1 1 0.5", recording.IsRecording() ? "ABORT" : "CLEAR", 8, null, 0.83f, 0.925f, 0f, 1f, 0f, 0f, 0f, 0f, "adminmodule.timelineclear", (TextAnchor)4);
				cui.CreateProtectedButton(container, panel, recording.IsRecording() ? "0.9 0.1 0.1 1" : "0.2 0.2 0.2 0.7", "1 1 1 0.5", "REC", 8, null, 0.93f, 0.99f, 0f, 1f, 0f, 0f, 0f, 0f, "adminmodule.timelinetoggle", (TextAnchor)4);
			});
			Chart.ChartSettings settings = new Chart.ChartSettings
			{
				HorizontalLabels = true
			};
			Chart.Layer[] layers = null;
			string[] vLabels = null;
			string[] hLabels = null;
			switch (storage)
			{
			case 0:
				GenerateProfilerDataChart_Assembly(recording, (MonoProfiler.AssemblyRecord assembly) => assembly.calls, (ulong value) => value.ToString("n0"), 5, 7, out layers, out vLabels, out hLabels);
				break;
			case 1:
				GenerateProfilerDataChart_Assembly(recording, (MonoProfiler.AssemblyRecord assembly) => assembly.alloc, (ulong value) => value.Format().ToUpper(), 5, 7, out layers, out vLabels, out hLabels);
				break;
			case 2:
				GenerateProfilerDataChart_Assembly(recording, (MonoProfiler.AssemblyRecord assembly) => (ulong)assembly.total_time_ms, (ulong value) => $"{value:n0}ms", 5, 7, out layers, out vLabels, out hLabels);
				break;
			case 3:
				GenerateProfilerDataChart_Assembly(recording, (MonoProfiler.AssemblyRecord assembly) => assembly.total_exceptions, (ulong value) => value.ToString("n0"), 3, 5, out layers, out vLabels, out hLabels);
				break;
			case 4:
				GenerateProfilerDataChart_Call(recording, (MonoProfiler.CallRecord call) => call.calls, (MonoProfiler.AssemblyRecord assembly) => assembly.calls, (ulong value) => value.ToString("n0"), 5, 7, out layers, out vLabels, out hLabels);
				break;
			case 5:
				GenerateProfilerDataChart_Call(recording, (MonoProfiler.CallRecord call) => (ulong)call.total_time_ms, (MonoProfiler.AssemblyRecord assembly) => (ulong)assembly.total_time_ms, (ulong value) => $"{value:n0}ms", 5, 7, out layers, out vLabels, out hLabels);
				break;
			case 6:
				GenerateProfilerDataChart_Call(recording, (MonoProfiler.CallRecord call) => (ulong)call.own_time_ms, (MonoProfiler.AssemblyRecord assembly) => (ulong)assembly.total_time_ms, (ulong value) => $"{value:n0}ms", 5, 7, out layers, out vLabels, out hLabels);
				break;
			case 7:
				GenerateProfilerDataChart_Call(recording, (MonoProfiler.CallRecord call) => call.total_alloc, (MonoProfiler.AssemblyRecord assembly) => assembly.alloc, (ulong value) => value.Format().ToUpper(), 5, 7, out layers, out vLabels, out hLabels);
				break;
			case 8:
				GenerateProfilerDataChart_Call(recording, (MonoProfiler.CallRecord call) => call.own_alloc, (MonoProfiler.AssemblyRecord assembly) => assembly.alloc, (ulong value) => value.Format().ToUpper(), 5, 7, out layers, out vLabels, out hLabels);
				break;
			case 9:
				GenerateProfilerDataChart_Call(recording, (MonoProfiler.CallRecord call) => call.total_exceptions, (MonoProfiler.AssemblyRecord assembly) => assembly.total_exceptions, (ulong value) => value.ToString("n0"), 3, 5, out layers, out vLabels, out hLabels);
				break;
			case 10:
				GenerateProfilerDataChart_Call(recording, (MonoProfiler.CallRecord call) => call.own_exceptions, (MonoProfiler.AssemblyRecord assembly) => assembly.total_exceptions, (ulong value) => value.ToString("n0"), 3, 5, out layers, out vLabels, out hLabels);
				break;
			case 11:
				GenerateProfilerDataChart_Memory(recording, (MonoProfiler.MemoryRecord memory) => memory.total_alloc_size, (ulong value) => value.Format().ToUpper(), 6, 6, out layers, out vLabels, out hLabels);
				break;
			case 12:
				GenerateProfilerDataChart_Memory(recording, (MonoProfiler.MemoryRecord memory) => memory.total_alloc_size, (ulong value) => value.Format().ToUpper(), 6, 6, out layers, out vLabels, out hLabels);
				break;
			case 13:
				GenerateProfilerDataChart_Memory(recording, (MonoProfiler.MemoryRecord memory) => memory.allocations, (ulong value) => value.Format().ToUpper(), 6, 6, out layers, out vLabels, out hLabels);
				break;
			}
			AddChart(0, timelineChartOptions[storage], (TextAnchor)0, 18, layers, vLabels, hLabels, settings, responsive: false);
			AddName(0, "Recording Info", (TextAnchor)3);
			AddInput(0, "Status", (PlayerSession ap) => recording.Status.ToString());
			AddInput(0, "Duration", (PlayerSession ap) => $"{recording.Duration:0.0}s ({recording.Rate:0.0}s rate)");
			AddInput(0, "Flags", (PlayerSession ap) => recording.Args.ToString());
			AddName(0, $"Samples ({recording.Timeline.Count:n0})", (TextAnchor)3);
			int num = recording.Timeline.Sum((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Assemblies.Count);
			int num2 = recording.Timeline.Sum((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Calls.Count);
			int num3 = recording.Timeline.Sum((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Memory.Count);
			int num4 = 0;
			if (num > num4)
			{
				num4 = num;
			}
			if (num2 > num4)
			{
				num4 = num2;
			}
			if (num3 > num4)
			{
				num4 = num3;
			}
			Stripe(session.SelectedTab, 0, num, num4, intenseColor, niceColor, "Assemblies", $"{(recording.Timeline.Any() ? recording.Timeline.Max((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Assemblies.Max((MonoProfiler.AssemblyRecord y) => y.calls)) : 0):n0} calls | " + (recording.Timeline.Any() ? recording.Timeline.Max((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Assemblies.Max((MonoProfiler.AssemblyRecord y) => y.alloc)) : 0).Format().ToUpper() + " allocs. | " + $"{(recording.Timeline.Any() ? recording.Timeline.Max((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Assemblies.Max((MonoProfiler.AssemblyRecord y) => y.total_time_ms)) : 0.0):n0}ms time | " + $"{(recording.Timeline.Any() ? recording.Timeline.Max((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Assemblies.Max((MonoProfiler.AssemblyRecord y) => y.total_exceptions)) : 0):n0} excep.", num.ToString("n0"), null);
			Stripe(session.SelectedTab, 0, num2, num4, intenseColor, niceColor, "Calls", $"{(recording.Timeline.Any() ? recording.Timeline.Max((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Calls.Max((MonoProfiler.CallRecord y) => y.calls)) : 0):n0} calls | " + (recording.Timeline.Any() ? recording.Timeline.Max((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Calls.Max((MonoProfiler.CallRecord y) => y.total_alloc)) : 0).Format().ToUpper() + " total / " + (recording.Timeline.Any() ? recording.Timeline.Max((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Calls.Max((MonoProfiler.CallRecord y) => y.own_alloc)) : 0).Format().ToUpper() + " own allocs. | " + $"{(recording.Timeline.Any() ? recording.Timeline.Max((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Calls.Max((MonoProfiler.CallRecord y) => y.total_time_ms)) : 0.0):n0}ms total / " + $"{(recording.Timeline.Any() ? recording.Timeline.Max((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Calls.Max((MonoProfiler.CallRecord y) => y.own_time_ms)) : 0.0):n0}ms own time | " + $"{(recording.Timeline.Any() ? recording.Timeline.Max((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Calls.Max((MonoProfiler.CallRecord y) => y.total_exceptions)) : 0):n0} total / " + $"{(recording.Timeline.Any() ? recording.Timeline.Max((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Calls.Max((MonoProfiler.CallRecord y) => y.own_exceptions)) : 0):n0} own excep.", num2.ToString("n0"), null);
			Stripe(session.SelectedTab, 0, num3, num4, intenseColor, niceColor, "Memory", $"{(recording.Timeline.Any() ? recording.Timeline.Max((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Memory.Max((MonoProfiler.MemoryRecord y) => y.allocations)) : 0):n0} allocs. | " + (recording.Timeline.Any() ? recording.Timeline.Max((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Memory.Max((MonoProfiler.MemoryRecord y) => y.total_alloc_size)) : 0).Format().ToUpper() + " total alloc. | " + (recording.Timeline.Any() ? recording.Timeline.Max((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Memory.Max((MonoProfiler.MemoryRecord y) => y.instance_size)) : 0u).Format().ToUpper() + " inst. size", num3.ToString("n0"), null);
		}

		public static void GenerateProfilerDataChart_Assembly(MonoProfiler.TimelineRecording recording, Func<MonoProfiler.AssemblyRecord, ulong> value, Func<ulong, string> valueFormat, int valueCuts, int assemblyCount, out Chart.Layer[] layers, out string[] vLabels, out string[] hLabels)
		{
			List<Chart.Layer> list = Pool.Get<List<Chart.Layer>>();
			List<string> list2 = Pool.Get<List<string>>();
			List<string> list3 = Pool.Get<List<string>>();
			list3.AddRange(recording.Timeline.Select((KeyValuePair<DateTime, MonoProfiler.Sample> sample) => $"{sample.Key.Hour:00}:{sample.Key.Minute:00}:{sample.Key.Second:00}"));
			IEnumerable<MonoProfiler.AssemblyRecord> source = recording.Timeline.SelectMany((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Assemblies.OrderByDescending(value));
			ulong num = (source.Any() ? source.Max(value) : 0);
			if (source.Any())
			{
				for (int num2 = 0; num2 < valueCuts; num2++)
				{
					list2.Add(valueFormat(((ulong)num2).Scale(0uL, (ulong)valueCuts, 0uL, num)));
				}
			}
			list2.Add(valueFormat(num));
			int num3 = 0;
			foreach (MonoProfiler.AssemblyRecord assembly in source.Take(assemblyCount))
			{
				Color color = ChartColors[num3];
				list.Add(new Chart.Layer
				{
					Name = assembly.assembly_name.GetDisplayName(isCompared: false),
					Data = recording.Timeline.Select((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Assemblies.Where((MonoProfiler.AssemblyRecord assemblyRecord) => assemblyRecord.assembly_handle == assembly.assembly_handle).SumULong(value)).ToArray(),
					LayerSettings = new Chart.LayerSettings
					{
						Color = color,
						Shadows = 0
					}
				});
				num3++;
			}
			layers = list.ToArray();
			vLabels = list2.ToArray();
			hLabels = list3.ToArray();
			Pool.FreeUnmanaged<string>(ref list2);
			Pool.FreeUnmanaged<string>(ref list3);
			Pool.FreeUnmanaged<Chart.Layer>(ref list);
		}

		public static void GenerateProfilerDataChart_Call(MonoProfiler.TimelineRecording recording, Func<MonoProfiler.CallRecord, ulong> callValue, Func<MonoProfiler.AssemblyRecord, ulong> assemblyValue, Func<ulong, string> valueFormat, int valueCuts, int callCount, out Chart.Layer[] layers, out string[] vLabels, out string[] hLabels)
		{
			List<Chart.Layer> list = Pool.Get<List<Chart.Layer>>();
			List<string> list2 = Pool.Get<List<string>>();
			List<string> list3 = Pool.Get<List<string>>();
			list3.AddRange(recording.Timeline.Select((KeyValuePair<DateTime, MonoProfiler.Sample> sample) => $"{sample.Key.Hour:00}:{sample.Key.Minute:00}:{sample.Key.Second:00}"));
			IEnumerable<MonoProfiler.AssemblyRecord> source = recording.Timeline.SelectMany((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Assemblies.OrderByDescending(assemblyValue));
			ulong num = (source.Any() ? source.Max(assemblyValue) : 0);
			if (source.Any())
			{
				for (int num2 = 0; num2 < valueCuts; num2++)
				{
					list2.Add(valueFormat(((ulong)num2).Scale(0uL, (ulong)valueCuts, 0uL, num)));
				}
			}
			list2.Add(valueFormat(num));
			int num3 = 0;
			foreach (MonoProfiler.AssemblyRecord assembly in source.Take(callCount))
			{
				Color color = ChartColors[num3];
				MonoProfiler.AssemblyMap.TryGetValue(assembly.assembly_handle, out var value);
				list.Add(new Chart.Layer
				{
					Name = value.GetDisplayName(isCompared: false),
					Data = recording.Timeline.Select((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Calls.Where((MonoProfiler.CallRecord callRecord) => callRecord.assembly_handle == assembly.assembly_handle).SumULong(callValue)).ToArray(),
					LayerSettings = new Chart.LayerSettings
					{
						Color = color,
						Shadows = 0
					}
				});
				num3++;
			}
			layers = list.ToArray();
			vLabels = list2.ToArray();
			hLabels = list3.ToArray();
			Pool.FreeUnmanaged<string>(ref list2);
			Pool.FreeUnmanaged<string>(ref list3);
			Pool.FreeUnmanaged<Chart.Layer>(ref list);
		}

		public static void GenerateProfilerDataChart_Memory(MonoProfiler.TimelineRecording recording, Func<MonoProfiler.MemoryRecord, ulong> value, Func<ulong, string> valueFormat, int valueCuts, int memoryCount, out Chart.Layer[] layers, out string[] vLabels, out string[] hLabels)
		{
			List<Chart.Layer> list = Pool.Get<List<Chart.Layer>>();
			List<string> list2 = Pool.Get<List<string>>();
			List<string> list3 = Pool.Get<List<string>>();
			list3.AddRange(recording.Timeline.Select((KeyValuePair<DateTime, MonoProfiler.Sample> sample) => $"{sample.Key.Hour:00}:{sample.Key.Minute:00}:{sample.Key.Second:00}"));
			IEnumerable<MonoProfiler.MemoryRecord> source = recording.Timeline.SelectMany((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Memory.OrderByDescending(value));
			ulong num = (source.Any() ? source.Max(value) : 0);
			if (source.Any())
			{
				for (int num2 = 0; num2 < valueCuts; num2++)
				{
					list2.Add(valueFormat(((ulong)num2).Scale(0uL, (ulong)valueCuts, 0uL, num)));
				}
			}
			list2.Add(valueFormat(num));
			int num3 = 0;
			foreach (MonoProfiler.MemoryRecord assembly in source.Take(memoryCount))
			{
				Color color = ChartColors[num3];
				list.Add(new Chart.Layer
				{
					Name = assembly.class_name,
					Data = recording.Timeline.Select((KeyValuePair<DateTime, MonoProfiler.Sample> x) => x.Value.Memory.Where((MonoProfiler.MemoryRecord memoryRecord) => memoryRecord.assembly_handle == assembly.assembly_handle).SumULong(value)).ToArray(),
					LayerSettings = new Chart.LayerSettings
					{
						Color = color,
						Shadows = 0
					}
				});
				num3++;
			}
			layers = list.ToArray();
			vLabels = list2.ToArray();
			hLabels = list3.ToArray();
			Pool.FreeUnmanaged<string>(ref list2);
			Pool.FreeUnmanaged<string>(ref list3);
			Pool.FreeUnmanaged<Chart.Layer>(ref list);
		}
	}

	public class SourceViewerTab : Tab
	{
		public class SyntaxHighlighter
		{
			private readonly StyleResolver _resolver = new StyleResolver();

			public void AddPattern(SyntaxKind syntaxKind, string color)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				_resolver[syntaxKind] = "<color=" + color + ">";
			}

			public string Process(SyntaxTree syntaxTree)
			{
				SyntaxNode root = syntaxTree.GetRoot(default(CancellationToken));
				StringBuilder stringBuilder = Pool.Get<StringBuilder>();
				WriteNode(root, stringBuilder);
				string result = stringBuilder.ToString();
				Pool.FreeUnmanaged(ref stringBuilder);
				return result;
			}

			private void WriteNode(SyntaxNode node, StringBuilder builder)
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				//IL_0026: Unknown result type (might be due to invalid IL or missing references)
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Unknown result type (might be due to invalid IL or missing references)
				foreach (SyntaxToken item in node.DescendantTokens((Func<SyntaxNode, bool>)null, false))
				{
					SyntaxToken current = item;
					string text = ((SyntaxToken)(ref current)).ToFullString();
					SyntaxKind syntaxKind = CSharpExtensions.Kind(((SyntaxToken)(ref current)).Parent);
					string text2 = _resolver[syntaxKind];
					if (!string.IsNullOrEmpty(text2))
					{
						builder.Append(text2 + text + "</color>");
					}
					else
					{
						builder.Append(text ?? "");
					}
				}
			}
		}

		public class StyleResolver
		{
			private static readonly string[] _names = Enum.GetNames(typeof(SyntaxKind));

			private static readonly string[] _styles = new string[_names.Length];

			public unsafe string this[SyntaxKind syntaxKind]
			{
				get
				{
					return _styles[_names.IndexOf<string>(((object)(*(SyntaxKind*)(&syntaxKind))/*cast due to constrained. prefix*/).ToString())];
				}
				set
				{
					_styles[_names.IndexOf<string>(((object)(*(SyntaxKind*)(&syntaxKind))/*cast due to constrained. prefix*/).ToString())] = value;
				}
			}
		}

		public Action<PlayerSession> Close;

		public SourceViewerTab(string id, string name, RustPlugin plugin, Action<PlayerSession, Tab> onChange = null, string access = null)
			: base(id, name, plugin, onChange, access)
		{
		}

		public static SourceViewerTab Make(string fileName, string content, string context, int size = 8)
		{
			SourceViewerTab sourceViewerTab = new SourceViewerTab("sourceviewer", "Source Viewer", Community.Runtime.Core);
			sourceViewerTab.OnChange = (Action<PlayerSession, Tab>)Delegate.Combine(sourceViewerTab.OnChange, (Action<PlayerSession, Tab>)delegate(PlayerSession _, Tab tab1)
			{
				tab1.AddColumn(0, clear: true);
			});
			sourceViewerTab.Over = (Action<Tab, CUI, CuiElementContainer, string, PlayerSession>)Delegate.Combine(sourceViewerTab.Over, (Action<Tab, CUI, CuiElementContainer, string, PlayerSession>)delegate(Tab _, CUI cui, CuiElementContainer container, string panel, PlayerSession ap)
			{
				CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, panel, "0.1 0.1 0.1 0.8", null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, blur: true);
				string[] array = content.Split('\n');
				List<string> list = Pool.Get<List<string>>();
				string text = array.ToString("\n");
				for (int i = 0; i < array.Length; i++)
				{
					list.Add($"{i + 1}");
				}
				cui.CreateImage(container, pair, "fade", Cache.CUI.WhiteColor, null, 0f, 1f, 0.96f);
				cui.CreateText(container, pair, "0.8 0.8 0.8 1", fileName + " <color=orange>*</color>", 8, 0.036f, 1f, 0.2f, 0.9875f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				cui.CreateText(container, pair, "0.8 0.8 0.8 0.5", context, 8, 0.036f, 0.97f, 0.2f, 0.9875f, 0f, 0f, 0f, 0f, (TextAnchor)2, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				CUI.Pair<string, CuiElement, CuiElement> pair2 = cui.CreateProtectedButton(container, pair, "0.9 0.2 0.1 1", Cache.CUI.BlankColor, string.Empty, 0, null, 0.978f, 1f, 0.96f, 1f, 0f, 0f, 0f, 0f, "adminmodule.profilerpreviewclose", (TextAnchor)4);
				cui.CreateImage(container, pair2, "close", "1 1 1 0.8", null, 0.2f, 0.8f, 0.2f, 0.8f);
				CuiRectTransform contentTransformComponent;
				CuiScrollbar horizontalScrollBar;
				CuiScrollbar verticalScrollBar;
				CUI.Pair<string, CuiElement> pair3 = cui.CreateScrollView(container, pair, vertical: true, horizontal: true, (MovementType)2, 0.5f, inertia: true, 0.2f, 75f, out contentTransformComponent, out horizontalScrollBar, out verticalScrollBar, 0f, 1f, 0f, 0.96f);
				cui.CreatePanel(container, pair, "0.2 0.2 0.2 1", null, 0f, 1f, 1f, 1f, 0f, 0f, -20f, -19f);
				cui.CreatePanel(container, pair3, "0.2 0.2 0.2 1", null, 0f, 0f, 0f, 1f, 29f, 30f);
				cui.CreatePanel(container, pair, "0.2 0.2 0.2 1", null, 0f, 0f, 0.96f, 1f, 29f, 30f);
				int value = array.Max((string x) => x.Length);
				float num = 0f - 11.2f * (float)array.Length.Clamp(45, int.MaxValue);
				float num2 = 2.75f * (float)value.Clamp(547, int.MaxValue);
				contentTransformComponent.AnchorMin = "0 1";
				contentTransformComponent.AnchorMax = "0 1";
				contentTransformComponent.OffsetMin = $"0 {num}";
				contentTransformComponent.OffsetMax = $"{num2} 0";
				verticalScrollBar.Size = 2f;
				horizontalScrollBar.Size = 2f;
				horizontalScrollBar.AutoHide = false;
				horizontalScrollBar.Invert = true;
				cui.CreateText(container, pair3, "0.3 0.7 0.9 0.5", string.Join("\n", list), size, 0f, 0f, 1f, 1f, 0f, 20f, num, -7.5f, (TextAnchor)2, CUI.Handler.FontTypes.DroidSansMono, (VerticalWrapMode)1);
				cui.CreateText(container, pair3, "0.8 0.8 0.8 1", text.Replace("\r", "").Replace("\"", "'").Replace("\t", "<color=#454545>————</color>"), size, 0f, 0f, 1f, 1f, 40f, 40f + num2, num, -7.5f, (TextAnchor)0, CUI.Handler.FontTypes.DroidSansMono, (VerticalWrapMode)1);
				Pool.FreeUnmanaged<string>(ref list);
			});
			return sourceViewerTab;
		}

		public static SourceViewerTab MakeMethod(string assembly, string type, string method, int size = 8)
		{
			string content = SourceCodeBank.Parse(assembly).ParseMethod(type, method).Trim();
			return Make("<color=#878787>" + type + ".</color>" + method + "<color=#878787>.cs</color>", ProcessSyntaxHighlight(content), Path.GetFileNameWithoutExtension(assembly) + ".dll", size);
		}

		public unsafe static SourceViewerTab MakeMethod(MonoProfiler.CallRecord call, int size = 8)
		{
			string type;
			string method;
			string content = SourceCodeBank.Parse(call.assembly_name.name, call.assembly_handle).ParseMethod(call.method_handle, out type, out method).Trim();
			return Make("<color=#878787>" + type + ".</color>" + method, ProcessSyntaxHighlight(content), call.assembly_name.GetDisplayName(isCompared: true), size);
		}

		public static string ProcessSyntaxHighlight(string content)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(content, Encoding.UTF8, (SourceHashAlgorithm)1), new CSharpParseOptions((LanguageVersion)0, (DocumentationMode)1, (SourceCodeKind)0, (IEnumerable<string>)null).WithDocumentationMode((DocumentationMode)1).WithKind((SourceCodeKind)1).WithLanguageVersion((LanguageVersion)2147483646), "", default(CancellationToken));
			SyntaxHighlighter syntaxHighlighter = new SyntaxHighlighter();
			syntaxHighlighter.AddPattern((SyntaxKind)8843, "#0000FF");
			syntaxHighlighter.AddPattern((SyntaxKind)8819, "#caf553");
			syntaxHighlighter.AddPattern((SyntaxKind)8326, "#dcdcaa");
			syntaxHighlighter.AddPattern((SyntaxKind)8616, "#e0e0e0");
			syntaxHighlighter.AddPattern((SyntaxKind)8621, "#caf553");
			syntaxHighlighter.AddPattern((SyntaxKind)8207, "#b3b1b1");
			syntaxHighlighter.AddPattern((SyntaxKind)8208, "#b3b1b1");
			syntaxHighlighter.AddPattern((SyntaxKind)8343, "#caf553");
			syntaxHighlighter.AddPattern((SyntaxKind)8344, "#caf553");
			syntaxHighlighter.AddPattern((SyntaxKind)8847, "#b3b1b1");
			syntaxHighlighter.AddPattern((SyntaxKind)8413, "#caf553");
			syntaxHighlighter.AddPattern((SyntaxKind)8875, "#caf553");
			syntaxHighlighter.AddPattern((SyntaxKind)8508, "#e0e0e0");
			syntaxHighlighter.AddPattern((SyntaxKind)8347, "#caf553");
			syntaxHighlighter.AddPattern((SyntaxKind)8805, "#caf553");
			syntaxHighlighter.AddPattern((SyntaxKind)8842, "#caf553");
			syntaxHighlighter.AddPattern((SyntaxKind)8373, "#caf553");
			syntaxHighlighter.AddPattern((SyntaxKind)8318, "#caf553");
			syntaxHighlighter.AddPattern((SyntaxKind)8746, "#caf553");
			syntaxHighlighter.AddPattern((SyntaxKind)8511, "#d69d85");
			syntaxHighlighter.AddPattern((SyntaxKind)8795, "#b3b1b1");
			syntaxHighlighter.AddPattern((SyntaxKind)8752, "#caf553");
			syntaxHighlighter.AddPattern((SyntaxKind)8753, "#caf553");
			syntaxHighlighter.AddPattern((SyntaxKind)8812, "#caf553");
			syntaxHighlighter.AddPattern((SyntaxKind)8749, "#caf553");
			return syntaxHighlighter.Process(syntaxTree);
		}
	}

	public static readonly string Title = "<b>Admin Centre</b>";

	public static CorePlugin Core = Community.Runtime.Core;

	public ImageDatabaseModule ImageDatabase;

	public ColorPickerModule ColorPicker;

	public DatePickerModule DatePicker;

	public ModalModule Modal;

	public FileModule File;

	public readonly CUI.Handler Handler = new CUI.Handler();

	internal const int RangeCuts = 50;

	internal readonly string[] EmptyElement = new string[1] { string.Empty };

	internal List<Tab> Tabs = new List<Tab>();

	private const string PanelId = "carbonmodularui";

	private const string CursorPanelId = "carbonmodularuicur";

	private readonly string[] AdminPermissions = new string[30]
	{
		"greet", "config.use", "carbon.use", "carbon.quickactions", "carbon.quickactions.edit", "carbon.server_settings", "carbon.server_config", "carbon.server_info", "carbon.server_console", "entities.use",
		"entities.kill_entity", "entities.tp_entity", "entities.loot_entity", "entities.loot_players", "entities.respawn_players", "entities.blind_players", "entities.owner_change", "environment.use", "modules.use", "modules.config_edit",
		"permissions.use", "players.use", "players.inventory_management", "players.craft_queue", "players.see_ips", "plugins.use", "plugins.setup", "profiler.use", "profiler.startstop", "profiler.sourceviewer"
	};

	internal bool _logRegistration;

	private static readonly float Option_LeftOffset = 10f;

	private static readonly float Option_RightOffset = 0f - Option_LeftOffset;

	public const float MaximizedScale_XMin = 1.1f;

	public const float MaximizedScale_XMax = 1.1f;

	public const float MaximizedScale_YMin = 1.15f;

	public const float MaximizedScale_YMax = 1.15f;

	public const float OptionHeightOffset = 0.0035f;

	internal Dictionary<BasePlayer, PlayerSession> PlayerSessions = new Dictionary<BasePlayer, PlayerSession>();

	private readonly int[] _backpacks = new int[2] { -907422733, 2068884361 };

	public override string Name => "Admin";

	public override VersionNumber Version => new VersionNumber(1, 8, 0);

	public override Type Type => typeof(AdminModule);

	public override bool EnabledByDefault => true;

	internal static AdminModule Singleton { get; set; }

	internal static List<string> _logQueue { get; } = new List<string>();

	internal static Dictionary<LogType, string> _logColor { get; } = new Dictionary<LogType, string>
	{
		[(LogType)3] = "white",
		[(LogType)2] = "#dbbe2a",
		[(LogType)0] = "#db2a2a"
	};

	[Conditional("!MINIMAL")]
	[ProtectedCommand("carbonmodularui.changetab")]
	private void ChangeTab(Arg args)
	{
		BasePlayer player = ArgEx.Player(args);
		PlayerSession playerSession = GetPlayerSession(player);
		Tab selectedTab = playerSession.SelectedTab;
		string value = args.GetString(0, "");
		playerSession.Clear();
		IEnumerable<Tab> source = Tabs.Where((Tab x) => !base.DataInstance.IsTabHidden(x.Id));
		SetTab(player, source.FirstOrDefault((Tab x) => x.Id.Equals(value)));
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("carbonmodularui.callaction")]
	private void CallAction(Arg args)
	{
		BasePlayer player = ArgEx.Player(args);
		object[] array = Array.Empty<object>();
		if (args.Args != null && args.Args.Length - 2 > 0)
		{
			array = HookCaller.Caller.AllocateBuffer(args.Args.Length - 2);
			for (int i = 2; i < args.Args.Length; i++)
			{
				array[i - 2] = ((object)Unsafe.As<StringView, StringView>(ref args.Args[i])/*cast due to constrained. prefix*/).ToString();
			}
		}
		if (CallColumnRow(player, args.GetInt(0, 0), args.GetInt(1, 0), array))
		{
			Draw(player);
		}
		if (array.Length != 0)
		{
			HookCaller.Caller.ReturnBuffer(array);
		}
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("carbonmodularui.changecolumnpage")]
	private void ChangeColumnPage(Arg args)
	{
		BasePlayer player = ArgEx.Player(args);
		PlayerSession playerSession = GetPlayerSession(player);
		PlayerSession.Page orCreatePage = playerSession.GetOrCreatePage(args.GetInt(0, 0));
		switch (args.GetInt(1, 0))
		{
		case 0:
			orCreatePage.CurrentPage--;
			if (orCreatePage.CurrentPage < 0)
			{
				orCreatePage.CurrentPage = orCreatePage.TotalPages;
			}
			break;
		case 1:
			orCreatePage.CurrentPage++;
			if (orCreatePage.CurrentPage > orCreatePage.TotalPages)
			{
				orCreatePage.CurrentPage = 0;
			}
			break;
		case 2:
			orCreatePage.CurrentPage = 0;
			break;
		case 3:
			orCreatePage.CurrentPage = orCreatePage.TotalPages;
			break;
		case 4:
			orCreatePage.CurrentPage = (args.GetInt(2, 0) - 1).Clamp(0, orCreatePage.TotalPages);
			break;
		}
		Draw(player);
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("carbonmodularui.config")]
	private void ShowConfig(Arg args)
	{
		BasePlayer player = ArgEx.Player(args);
		Tab tab = GetTab(player);
		if (tab != null && tab.Id == "configuration")
		{
			SetTab(player, "carbon");
		}
		else
		{
			SetTab(player, ConfigurationTab.GetOrCache());
		}
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("carbonmodularui.profiler")]
	private void ShowProfiler(Arg args)
	{
		BasePlayer player = ArgEx.Player(args);
		if (GetTab(player).Id == "profiler")
		{
			SetTab(player, "carbon");
		}
		else
		{
			SetTab(player, ProfilerTab.GetOrCache(GetPlayerSession(player)));
		}
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("carbonmodularui.maximize")]
	private void Maximize(Arg args)
	{
		base.DataInstance.Maximize = !base.DataInstance.Maximize;
		Draw(ArgEx.Player(args));
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("carbonmodularui.close")]
	private void CloseUI(Arg args)
	{
		Close(ArgEx.Player(args));
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("carbonmodularui.dialogaction")]
	private void Dialog_Action(Arg args)
	{
		BasePlayer player = ArgEx.Player(args);
		PlayerSession playerSession = GetPlayerSession(player);
		Tab tab = GetTab(player);
		Tab.TabDialog tabDialog = tab?.Dialog;
		if (tab != null)
		{
			tab.Dialog = null;
		}
		string text = args.GetString(0, "");
		if (!(text == "confirm"))
		{
			if (text == "decline")
			{
				try
				{
					tabDialog?.OnDecline(playerSession);
				}
				catch
				{
				}
			}
		}
		else
		{
			try
			{
				tabDialog?.OnConfirm(playerSession);
			}
			catch
			{
			}
		}
		Draw(player);
	}

	public AdminModule()
	{
		Singleton = this;
	}

	public bool HandleEnableNeedsKeyboard(PlayerSession ap)
	{
		if (ap.SelectedTab != null)
		{
			return ap.SelectedTab.Dialog == null;
		}
		return true;
	}

	public bool HandleEnableNeedsKeyboard(BasePlayer player)
	{
		return HandleEnableNeedsKeyboard(GetPlayerSession(player));
	}

	public override void OnServerInit(bool initial)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		base.OnServerInit(initial);
		if (initial)
		{
			ImageDatabase = BaseModule.GetModule<ImageDatabaseModule>();
			ColorPicker = BaseModule.GetModule<ColorPickerModule>();
			DatePicker = BaseModule.GetModule<DatePickerModule>();
			Modal = BaseModule.GetModule<ModalModule>();
			File = BaseModule.GetModule<FileModule>();
			Unsubscribe("OnPluginLoaded");
			Unsubscribe("OnPluginUnloaded");
			Unsubscribe("OnEntityDismounted");
			Unsubscribe("CanDismountEntity");
			Unsubscribe("OnEntityVisibilityCheck");
			Unsubscribe("OnEntityDistanceCheck");
			Unsubscribe("CanAcceptItem");
			if (!_logRegistration)
			{
				Application.logMessageReceived += new LogCallback(OnLog);
				_logRegistration = true;
			}
			OnEnabled(initialized: true);
		}
	}

	public override void OnPostServerInit(bool initial)
	{
		base.OnPostServerInit(initial);
		GenerateTabs();
	}

	public override void OnEnabled(bool initialized)
	{
		base.OnEnabled(initialized);
		if (!initialized)
		{
			return;
		}
		for (int i = 0; i < base.ConfigInstance.OpenCommands.Length; i++)
		{
			string command = base.ConfigInstance.OpenCommands[i];
			Action<BasePlayer, string, string[]> callback = delegate(BasePlayer player, string cmd, string[] args)
			{
				if (CanAccess(player))
				{
					PlayerSession playerSession = GetPlayerSession(player);
					if (playerSession.IsInMenu)
					{
						Close(player);
					}
					else
					{
						if (playerSession.SelectedTab == null)
						{
							playerSession.SelectedTab = Tabs.FirstOrDefault((Tab x) => !base.DataInstance.IsTabHidden(x.Id) && HasAccess(player, x.Access));
							playerSession.Clear();
						}
						else if (base.DataInstance.IsTabHidden(playerSession.SelectedTab.Id) || !HasAccess(player, playerSession.SelectedTab.Access))
						{
							playerSession.SelectedTab = null;
						}
						Tab tab = GetTab(player);
						try
						{
							tab?.OnChange?.Invoke(playerSession, tab);
						}
						catch (Exception ex)
						{
							Logger.Error("Failed OnChange callback for tab '" + tab?.Name + "[" + tab?.Id + "], falling back to default tab", ex);
							playerSession.SelectedTab = Tabs.FirstOrDefault((Tab x) => HasAccess(player, x.Access));
							playerSession.Clear();
						}
						DrawCursorLocker(player);
						Draw(player);
					}
				}
			};
			Community.Runtime.Core.cmd.AddChatCommand(command, this, callback, null, null, null, null, -1, 0, isHidden: false, @protected: false, silent: true);
			Community.Runtime.Core.cmd.AddConsoleCommand(command, this, callback, null, null, null, null, -1, 0, isHidden: false, @protected: false, silent: true);
		}
		for (int num = 0; num < AdminPermissions.Length; num++)
		{
			Permissions.RegisterPermission("adminmodule." + AdminPermissions[num], this);
		}
		if (ImageDatabase == null)
		{
			ImageDatabase = BaseModule.GetModule<ImageDatabaseModule>();
		}
		ImageDatabase.Queue(@override: true, base.DataInstance.BackgroundImage);
	}

	public override void OnDisabled(bool initialized)
	{
		if (initialized)
		{
			Community.Runtime.Core.NextTick(delegate
			{
				for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
				{
					Close(BasePlayer.activePlayerList[i]);
				}
			});
		}
		base.OnDisabled(initialized);
	}

	public override void Shutdown()
	{
		Save();
		base.Shutdown();
	}

	public override void Load()
	{
		base.Load();
		base.ConfigInstance.MinimumAuthLevel = base.ConfigInstance.MinimumAuthLevel.Clamp(0, 3);
		base.ConfigInstance.MaximumAuthLevel = base.ConfigInstance.MaximumAuthLevel.Clamp(0, 3);
		if (Community.IsServerInitialized)
		{
			GenerateTabs();
		}
		if (base.ModuleConfiguration.HasConfigStructureChanged())
		{
			base.DataInstance.GreetDisplayed = false;
		}
	}

	public override void Save()
	{
		base.Save();
		PluginsTab.ServerOwner.Save();
	}

	public override Dictionary<string, Dictionary<string, string>> GetDefaultPhrases()
	{
		return new Dictionary<string, Dictionary<string, string>> { ["en"] = new Dictionary<string, string>
		{
			["hostname"] = "Host Name",
			["level"] = "Level",
			["info"] = "Info",
			["version"] = "Version",
			["version2"] = "Informational Version",
			["hooks"] = "Hooks",
			["statichooks"] = "Static Hooks",
			["dynamichooks"] = "Dynamic Hooks",
			["plugins"] = "Plugins",
			["mods"] = "Mods",
			["console"] = "Console",
			["execservercmd"] = "Execute Server Command",
			["config"] = "Config",
			["ismodded"] = "Is Modded",
			["ismodded_help"] = "When enabled, it marks the server as modded.",
			["general"] = "General",
			["watchers"] = "Watchers",
			["scriptwatchers"] = "Script Watchers",
			["scriptwatchers_help"] = "When disabled, you must load/unload plugins manually with 'c.load' or 'c.unload'.",
			["zipscriptwatchers"] = "ZIP Script Watchers",
			["zipscriptwatchers_help"] = "When disabled, you must load/unload plugins manually with 'c.load' or 'c.unload'.",
			["scriptwatchersoption"] = "Script Watchers Option",
			["scriptwatchersoption_help"] = "Indicates wether the script watcher (whenever enabled) listens to the 'carbon/plugins' folder only, or its subfolders.",
			["logging"] = "Logging",
			["logfilemode"] = "Log File Mode",
			["logverbosity"] = "Log Verbosity (Debug)",
			["logseverity"] = "Log Severity",
			["misc"] = "Miscellaneous",
			["serverlang"] = "Server Language",
			["webreqip"] = "WebRequest IP",
			["permmode"] = "Permission Mode",
			["nocontent"] = "There are no options available.\nSelect a sub-tab to populate this area (if available).",
			["consoleinfo"] = "Show Console Info",
			["consoleinfo_help"] = "Show the Windows-only Carbon information at the bottom of the console.",
			["playerdefgroup"] = "Player Group",
			["admindefgroup"] = "Admin Group",
			["moderatordefgroup"] = "Moderator Group",
			["permissions"] = "Permissions",
			["debugging"] = "Debugging",
			["scriptdebugorigin"] = "Script Debugging Origin",
			["scriptdebugorigin_help"] = "Whenever a debugger is attached on server boot, the compiler will replace the debugging origin of the plugin file.",
			["conditionals"] = "Conditionals",
			["quickactions"] = "Quick Actions",
			["quickactions_name"] = "Button Name",
			["quickactions_name_help"] = "The name of the button for the Quick Action.",
			["quickactions_command"] = "Button Command",
			["quickactions_command_help"] = "Command (separated with | for multiple) of the Quick Action button.",
			["quickactions_user"] = "User Mode",
			["quickactions_user_help"] = "When the command gets executed, it'll call it with user permissions.",
			["quickactions_incluserid"] = "Include User ID",
			["quickactions_incluserid_help"] = "When the command gets executed, append the player's Steam ID at the end of the command after a space.",
			["quickactions_confirmdialog"] = "Confirm Dialog",
			["quickactions_confirmdialog_help"] = "Show a dialog which asks you to confirm before executing sensitive command(s).",
			["quickactions_add"] = "Add",
			["quickactions_edit"] = "Edit",
			["quickactions_stopedit"] = "Stop Editing",
			["maxplayers"] = "Maximum Players"
		} };
	}

	[Conditional("!MINIMAL")]
	private void OnLog(string condition, string stackTrace, LogType type)
	{
		try
		{
			if (_logQueue.Count >= 6)
			{
				_logQueue.RemoveAt(0);
			}
			string[] array = condition.Split('\n');
			string value = array[0];
			Array.Clear(array, 0, array.Length);
			_logQueue.Add(StringEx.Truncate(value, 85));
		}
		catch
		{
		}
	}

	public bool HasAccess(BasePlayer player, string access)
	{
		if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)player) && player.IsConnected && player.Connection.authLevel >= base.ConfigInstance.MinimumAuthLevel && player.Connection.authLevel <= base.ConfigInstance.MaximumAuthLevel)
		{
			return true;
		}
		if (Permissions.UserHasPermission(player.UserIDString, "adminmodule." + access))
		{
			return true;
		}
		return false;
	}

	public void GenerateTabs()
	{
		UnregisterAllTabs();
		RegisterTab(CarbonTab.Get());
		RegisterTab(PlayersTab.Get());
		RegisterTab(EntitiesTab.Get());
		RegisterTab(PermissionsTab.Get());
		RegisterTab(ModulesTab.Get());
		RegisterTab(EnvironmentTab.Get());
		RegisterTab(PluginsTab.Get());
	}

	[Conditional("!MINIMAL")]
	private bool CanAccess(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		object obj = HookCaller.CallStaticHook(3266674522u, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		uint authLevel = player.Connection.authLevel;
		int minimumAuthLevel = base.ConfigInstance.MinimumAuthLevel;
		int maximumAuthLevel = base.ConfigInstance.MaximumAuthLevel;
		bool flag = authLevel >= minimumAuthLevel && authLevel <= maximumAuthLevel;
		if (!flag)
		{
			if (authLevel == 0)
			{
				player.ChatMessage("Your auth level is not high enough to use this feature.");
			}
			else if (authLevel > maximumAuthLevel)
			{
				player.ChatMessage($"Your auth level is above the maximum level required to use this feature. Please adjust the maximum level required in your config or give yourself auth level {maximumAuthLevel}.");
			}
			else if (authLevel < minimumAuthLevel && authLevel != 0)
			{
				player.ChatMessage($"Your auth level is not high enough to use this feature. Please adjust the minimum level required in your config or give yourself auth level {minimumAuthLevel}.");
			}
		}
		return flag;
	}

	[Conditional("!MINIMAL")]
	internal void TabButton(CUI cui, CuiElementContainer container, string parent, string text, string command, float width, float offset, bool highlight = false, bool disabled = false)
	{
		CUI.Pair<string, CuiElement, CuiElement> pair = cui.CreateProtectedButton(container, parent, highlight ? (base.DataInstance.Colors.SelectedTabColor + " 0.7") : "0.3 0.3 0.3 0.1", $"1 1 1 {(disabled ? 0.15 : 0.5)}", text, 11, null, offset, offset + width, 0f, 1f, 0f, 0f, 0f, 0f, disabled ? string.Empty : command, (TextAnchor)4);
		cui.CreateImage(container, pair, "fade", Cache.CUI.WhiteColor);
		if (highlight)
		{
			cui.CreatePanel(container, pair, "1 1 1 0.4", null, 0f, 1f, 0f, 0.03f, 0f, -0.5f);
		}
	}

	public void TabColumnPagination(CUI cui, CuiElementContainer container, string parent, int column, PlayerSession.Page page, float height, float offset)
	{
		CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, parent, Cache.CUI.BlankColor, null, 0.02f, 0.98f, offset, offset + height);
		cui.CreateText(container, pair, "1 1 1 0.5", $" / {page.TotalPages + 1:n0}", 9, 0.5f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
		cui.CreateProtectedInputField(container, pair, "1 1 1 1", $"{page.CurrentPage + 1}", 9, 0, readOnly: false, 0f, 0.495f, 0f, 1f, 0f, 0f, 0f, 0f, "carbonmodularui" + $".changecolumnpage {column} 4 ", (TextAnchor)5, CUI.Handler.FontTypes.RobotoCondensedRegular, autoFocus: false, hudMenuInput: false, (LineType)0);
		cui.CreateProtectedButton(container, pair, "0.3 0.3 0.3 0.1", (page.CurrentPage > 0) ? "1 1 1 0.5" : "0.5 0.5 0.5 0.5", "<<", 8, null, 0f, 0.1f, 0f, 1f, 0f, 0f, 0f, 0f, (page.CurrentPage > 0) ? ("carbonmodularui" + $".changecolumnpage {column} 2") : "", (TextAnchor)4);
		cui.CreateProtectedButton(container, pair, "0.3 0.3 0.3 0.1", "1 1 1 0.5", "<", 8, null, 0.1f, 0.2f, 0f, 1f, 0f, 0f, 0f, 0f, "carbonmodularui" + $".changecolumnpage {column} 0", (TextAnchor)4);
		cui.CreateProtectedButton(container, pair, "0.3 0.3 0.3 0.1", (page.CurrentPage < page.TotalPages) ? "1 1 1 0.5" : "0.5 0.5 0.5 0.5", ">>", 8, null, 0.9f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (page.CurrentPage < page.TotalPages) ? ("carbonmodularui" + $".changecolumnpage {column} 3") : "", (TextAnchor)4);
		cui.CreateProtectedButton(container, pair, "0.3 0.3 0.3 0.1", "1 1 1 0.5", ">", 8, null, 0.8f, 0.9f, 0f, 1f, 0f, 0f, 0f, 0f, "carbonmodularui" + $".changecolumnpage {column} 1", (TextAnchor)4);
	}

	public void TabPanelName(CUI cui, CuiElementContainer container, string parent, string text, float height, float offset, TextAnchor align)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		string nameTextColor = base.DataInstance.Colors.NameTextColor;
		string text2 = text?.ToUpper();
		float option_LeftOffset = Option_LeftOffset;
		float option_RightOffset = Option_RightOffset;
		CUI.Pair<string, CuiElement> pair = cui.CreateText(container, parent, nameTextColor, text2, 12, 0f, 1f, offset, offset + height, option_LeftOffset, option_RightOffset, 0f, 0f, align, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
		if (!string.IsNullOrEmpty(text))
		{
			cui.CreatePanel(container, pair, $"1 1 1 {base.DataInstance.Colors.TitleUnderlineOpacity}", null, 0f, 1f, 0f, 0.015f);
		}
	}

	public void TabPanelText(CUI cui, CuiElementContainer container, string parent, string text, int size, string color, float height, float offset, TextAnchor align, CUI.Handler.FontTypes font, bool isInput)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (isInput)
		{
			float option_LeftOffset = Option_LeftOffset;
			float option_RightOffset = Option_RightOffset;
			cui.CreateInputField(container, parent, color, text, size, 0, readOnly: true, 0f, 1f, offset, offset + height, option_LeftOffset, option_RightOffset, 0f, 0f, null, align, font, autoFocus: false, hudMenuInput: false, (LineType)0);
		}
		else
		{
			float option_RightOffset = Option_LeftOffset;
			float option_LeftOffset = Option_RightOffset;
			cui.CreateText(container, parent, color, text, size, 0f, 1f, offset, offset + height, option_RightOffset, option_LeftOffset, 0f, 0f, align, font, (VerticalWrapMode)1);
		}
	}

	public void TabPanelButton(CUI cui, CuiElementContainer container, string parent, string text, string command, float height, float offset, Tab.OptionButton.Types type = Tab.OptionButton.Types.None, TextAnchor align = (TextAnchor)4)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		object color = type switch
		{
			Tab.OptionButton.Types.Selected => base.DataInstance.Colors.ButtonSelectedColor, 
			Tab.OptionButton.Types.Warned => base.DataInstance.Colors.ButtonWarnedColor, 
			Tab.OptionButton.Types.Important => base.DataInstance.Colors.ButtonImportantColor, 
			_ => base.DataInstance.Colors.OptionColor, 
		};
		float option_LeftOffset = Option_LeftOffset;
		float option_RightOffset = Option_RightOffset;
		CUI.Pair<string, CuiElement, CuiElement> pair = cui.CreateProtectedButton(container, parent, (string)color, "1 1 1 0.5", text, 11, null, 0f, 1f, offset, offset + height, option_LeftOffset, option_RightOffset, 0f, 0f, command, align);
		cui.CreateImage(container, pair, "fade", Cache.CUI.WhiteColor);
	}

	public void TabPanelToggle(CUI cui, CuiElementContainer container, string parent, string text, string command, float height, float offset, bool isOn, Tab tab)
	{
		float xMax = (tab.IsFullscreen ? 0.93f : 0.94f);
		CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, parent, Cache.CUI.BlankColor, null, 0f, 1f, offset, offset + height);
		if (!string.IsNullOrEmpty(text))
		{
			cui.CreateText(container, pair, base.DataInstance.Colors.OptionNameColor, text + ":", 12, 0f, 1f, 0f, 1f, Option_LeftOffset, Option_RightOffset, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
			cui.CreatePanel(container, pair, "0.2 0.2 0.2 0.5", null, 0f, xMax, 0f, 0.015f);
		}
		CUI.Pair<string, CuiElement, CuiElement> pair2 = cui.CreateProtectedButton(container, pair, base.DataInstance.Colors.OptionColor, "1 1 1 0.5", string.Empty, 11, null, 0.975f, 0.975f, 0.5f, 0.5f, -25f, 0f, -12.5f, 12.5f, command, (TextAnchor)4);
		cui.CreateImage(container, pair2, "fade", Cache.CUI.WhiteColor);
		if (isOn)
		{
			cui.CreateImage(container, pair2, "checkmark", base.DataInstance.Colors.ButtonSelectedColor, null, 0.15f, 0.85f, 0.15f, 0.85f);
		}
	}

	public void TabPanelInput(CUI cui, CuiElementContainer container, string parent, string text, string placeholder, string command, int characterLimit, bool readOnly, float height, float offset, PlayerSession session, Tab.OptionButton.Types type = Tab.OptionButton.Types.None, Tab.Option option = null)
	{
		string color = type switch
		{
			Tab.OptionButton.Types.Selected => base.DataInstance.Colors.ButtonSelectedColor, 
			Tab.OptionButton.Types.Warned => base.DataInstance.Colors.ButtonWarnedColor, 
			Tab.OptionButton.Types.Important => base.DataInstance.Colors.ButtonImportantColor, 
			_ => base.DataInstance.Colors.OptionColor, 
		};
		CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, parent, Cache.CUI.BlankColor, null, 0f, 1f, offset, offset + height);
		if (!string.IsNullOrEmpty(text))
		{
			cui.CreateText(container, pair, base.DataInstance.Colors.OptionNameColor, text + ":", 12, 0f, 1f, 0f, 1f, Option_LeftOffset, Option_RightOffset, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
			cui.CreatePanel(container, pair, color, null, 0f, base.DataInstance.Colors.OptionWidth, 0f, 0.015f);
		}
		CUI.Pair<string, CuiElement> pair2 = cui.CreatePanel(container, pair, color, null, base.DataInstance.Colors.OptionWidth, 1f, 0f, 1f, 0f, Option_RightOffset);
		cui.CreateImage(container, pair2, "fade", Cache.CUI.WhiteColor);
		string parent2 = pair2;
		string color2 = $"1 1 1 {(readOnly ? 0.2f : 1f)}";
		string text2 = command;
		cui.CreateProtectedInputField(command: text2, font: CUI.Handler.FontTypes.RobotoCondensedRegular, needsKeyboard: session.Input == option, container: container, parent: parent2, color: color2, text: placeholder, size: 11, characterLimit: characterLimit, readOnly: readOnly, xMin: 0.03f, xMax: 1f, yMin: 0f, yMax: 1f, OxMin: 0f, OxMax: 0f, OyMin: 0f, OyMax: 0f, align: (TextAnchor)3, autoFocus: session.Input == option && session.Input != session.PreviousInput, hudMenuInput: false, lineType: (LineType)0);
		if (session.Input == option)
		{
			session.PreviousInput = session.Input;
		}
		if (!readOnly)
		{
			cui.CreatePanel(container, pair2, base.DataInstance.Colors.EditableInputHighlight + " 0.9", null, 0f, 1f, 0f, 0.05f, 0f, -0.5f);
		}
	}

	public void TabPanelEnum(CUI cui, CuiElementContainer container, string parent, string text, string value, string command, float height, float offset, Tab.OptionButton.Types type = Tab.OptionButton.Types.Selected)
	{
		string color = type switch
		{
			Tab.OptionButton.Types.Selected => base.DataInstance.Colors.ButtonSelectedColor, 
			Tab.OptionButton.Types.Warned => base.DataInstance.Colors.ButtonWarnedColor, 
			Tab.OptionButton.Types.Important => base.DataInstance.Colors.ButtonImportantColor, 
			_ => base.DataInstance.Colors.OptionColor, 
		};
		CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, parent, Cache.CUI.BlankColor, null, 0f, 1f, offset, offset + height);
		if (!string.IsNullOrEmpty(text))
		{
			cui.CreateText(container, pair, base.DataInstance.Colors.OptionNameColor, text + ":", 12, 0f, 1f, 0f, 1f, Option_LeftOffset, Option_RightOffset, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
			cui.CreatePanel(container, pair, "0.2 0.2 0.2 0.5", null, 0f, base.DataInstance.Colors.OptionWidth, 0f, 0.015f);
		}
		CUI.Pair<string, CuiElement> pair2 = cui.CreatePanel(container, pair, base.DataInstance.Colors.OptionColor, null, base.DataInstance.Colors.OptionWidth, 1f, 0f, 1f, 0f, Option_RightOffset);
		cui.CreateImage(container, pair2, "fade", Cache.CUI.WhiteColor);
		cui.CreateText(container, pair2, "1 1 1 0.7", value, 11, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
		CUI.Pair<string, CuiElement, CuiElement> pair3 = cui.CreateProtectedButton(container, pair2, color, "1 1 1 0.7", "<", 10, null, 0f, 0.15f, 0f, 1f, 0f, 0f, 0f, 0f, command + " true", (TextAnchor)4);
		cui.CreateImage(container, pair3, "fade", Cache.CUI.WhiteColor);
		CUI.Pair<string, CuiElement, CuiElement> pair4 = cui.CreateProtectedButton(container, pair2, color, "1 1 1 0.7", ">", 10, null, 0.85f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, command + " false", (TextAnchor)4);
		cui.CreateImage(container, pair4, "fade", Cache.CUI.WhiteColor);
	}

	public void TabPanelDropdown(CUI cui, PlayerSession.Page page, CuiElementContainer container, string parent, string text, string command, float height, float offset, int index, string[] options, string[] optionsIcons, bool display, Tab.OptionButton.Types type = Tab.OptionButton.Types.Selected)
	{
		string rustColor = type switch
		{
			Tab.OptionButton.Types.Selected => base.DataInstance.Colors.ButtonSelectedColor, 
			Tab.OptionButton.Types.Warned => base.DataInstance.Colors.ButtonWarnedColor, 
			Tab.OptionButton.Types.Important => base.DataInstance.Colors.ButtonImportantColor, 
			_ => base.DataInstance.Colors.OptionColor2, 
		};
		CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, parent, Cache.CUI.BlankColor, null, 0f, 1f, offset, offset + height);
		if (!string.IsNullOrEmpty(text))
		{
			cui.CreateText(container, pair, base.DataInstance.Colors.OptionNameColor, text + ":", 12, 0f, 1f, 0f, 1f, Option_LeftOffset, Option_RightOffset, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
			cui.CreatePanel(container, pair, base.DataInstance.Colors.OptionColor, null, 0f, base.DataInstance.Colors.OptionWidth, 0f, 0.015f);
		}
		CUI.Pair<string, CuiElement> pair2 = cui.CreatePanel(container, pair, base.DataInstance.Colors.OptionColor, null, base.DataInstance.Colors.OptionWidth, 1f, 0f, 1f, 0f, Option_RightOffset);
		string text2 = ((optionsIcons != null && index < optionsIcons.Length) ? optionsIcons[index] : null);
		CUI.Pair<string, CuiElement, CuiElement> pair3 = cui.CreateProtectedButton(container, pair2, base.DataInstance.Colors.OptionColor, Cache.CUI.BlankColor, string.Empty, 0, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, command + " false", (TextAnchor)3);
		cui.CreateImage(container, pair3, "fade", Cache.CUI.WhiteColor);
		cui.CreateText(container, pair3, "1 1 1 0.7", (index >= options.Length) ? "Out of bounds" : options[index], 10, string.IsNullOrEmpty(text2) ? 0.035f : 0.09f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
		cui.CreateText(container, pair3, "1 1 1 0.4", "▼", 8, 0f, 1f, 0f, 1f, 0f, Option_RightOffset, 0f, 0f, (TextAnchor)5, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
		if (!string.IsNullOrEmpty(text2))
		{
			cui.CreateImage(container, pair3, text2, "1 1 1 0.7", null, 0.015f, 0.072f, 0.2f, 0.8f);
		}
		if (!display)
		{
			return;
		}
		float num = -22f;
		IEnumerable<string> source = options.Skip(10 * page.CurrentPage).Take(10);
		int num2 = source.Count();
		int num3 = 15;
		page.TotalPages = (int)Math.Ceiling((double)options.Length / 10.0 - 1.0);
		page.Check();
		for (int i = 0; i < num2; i++)
		{
			int num4 = i + page.CurrentPage * 10;
			string text3 = options[num4];
			bool flag = num4 == index;
			string text4 = ((optionsIcons != null && num4 <= optionsIcons.Length - 1) ? optionsIcons[num4] : null);
			string parent2 = pair2;
			string color = (flag ? CUI.RustToHexColor(rustColor, 1f) : "0.1 0.1 0.1 1");
			string blankColor = Cache.CUI.BlankColor;
			string empty = string.Empty;
			float oyMin = num;
			float oyMax = num;
			CUI.Pair<string, CuiElement, CuiElement> pair4 = cui.CreateProtectedButton(container, parent2, color, blankColor, empty, 0, null, 0f, 1f, 0f, 1f, num3, 0f, oyMin, oyMax, $"{command} true call {num4}", (TextAnchor)3);
			cui.CreateImage(container, pair4, "fade", Cache.CUI.WhiteColor);
			cui.CreateText(container, pair4, flag ? "1 1 1 0.7" : "1 1 1 0.4", text3, 10, string.IsNullOrEmpty(text4) ? 0.035f : 0.085f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
			if (!string.IsNullOrEmpty(text4))
			{
				cui.CreateImage(container, pair4, text4, flag ? "1 1 1 0.7" : "1 1 1 0.4", null, 0.015f, 0.072f, 0.2f, 0.8f);
			}
			num -= 22f;
		}
		if (page.TotalPages > 0)
		{
			string parent3 = pair2;
			float oyMax = num;
			float oyMin = num - 2f;
			CUI.Pair<string, CuiElement> pair5 = cui.CreatePanel(container, parent3, "0.2 0.2 0.2 0.2", null, 0f, 1f, 0f, 1f, num3, 0f, oyMax, oyMin);
			CUI.Pair<string, CuiElement> pair6 = cui.CreatePanel(container, pair5, "0.3 0.3 0.3 0.3");
			cui.CreateText(container, pair6, "1 1 1 0.5", $"{page.CurrentPage + 1:n0} / {page.TotalPages + 1:n0}", 9, 0.5f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
			cui.CreateProtectedButton(container, pair6, (page.CurrentPage > 0) ? "0.8 0.7 0.2 0.7" : "0.3 0.3 0.3 0.1", "1 1 1 0.5", "<<", 8, null, 0f, 0.1f, 0f, 1f, 0f, 0f, 0f, 0f, command + " true --", (TextAnchor)4);
			cui.CreateProtectedButton(container, pair6, "0.4 0.7 0.2 0.7", "1 1 1 0.5", "<", 8, null, 0.1f, 0.2f, 0f, 1f, 0f, 0f, 0f, 0f, command + " true -1", (TextAnchor)4);
			cui.CreateProtectedButton(container, pair6, (page.CurrentPage < page.TotalPages) ? "0.8 0.7 0.2 0.7" : "0.3 0.3 0.3 0.1", "1 1 1 0.5", ">>", 8, null, 0.9f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, command + " true ++", (TextAnchor)4);
			cui.CreateProtectedButton(container, pair6, "0.4 0.7 0.2 0.7", "1 1 1 0.5", ">", 8, null, 0.8f, 0.9f, 0f, 1f, 0f, 0f, 0f, 0f, command + " true 1", (TextAnchor)4);
		}
	}

	public void TabPanelRange(CUI cui, CuiElementContainer container, string parent, string text, string command, string valueText, float min, float max, float value, float height, float offset, Tab.OptionButton.Types type = Tab.OptionButton.Types.None)
	{
		string color = type switch
		{
			Tab.OptionButton.Types.Selected => base.DataInstance.Colors.ButtonSelectedColor, 
			Tab.OptionButton.Types.Warned => base.DataInstance.Colors.ButtonWarnedColor, 
			Tab.OptionButton.Types.Important => base.DataInstance.Colors.ButtonImportantColor, 
			_ => base.DataInstance.Colors.OptionColor, 
		};
		CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, parent, Cache.CUI.BlankColor, null, 0f, 1f, offset, offset + height);
		if (!string.IsNullOrEmpty(text))
		{
			cui.CreateText(container, pair, base.DataInstance.Colors.OptionNameColor, text + ":", 12, 0f, 1f, 0f, 1f, Option_LeftOffset, Option_RightOffset, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
			cui.CreatePanel(container, pair, color, null, 0f, base.DataInstance.Colors.OptionWidth, 0f, 0.015f);
		}
		CUI.Pair<string, CuiElement> pair2 = cui.CreatePanel(container, pair, Cache.CUI.BlankColor, null, base.DataInstance.Colors.OptionWidth, 1f, 0f, 1f, 0f, Option_RightOffset);
		CUI.Pair<string, CuiElement> pair3 = cui.CreatePanel(container, pair2, color, null, 0f, 1f, 0.4f, 0.6f);
		cui.CreateImage(container, pair3, "fade", Cache.CUI.WhiteColor);
		float num = value.Scale(min, max, 0f, 1f);
		CUI.Pair<string, CuiElement> pair4 = cui.CreatePanel(container, pair3, CUI.HexToRustColor("#f54242", 0.8f), null, 0f, num);
		cui.CreateImage(container, pair4, "fade", Cache.CUI.WhiteColor);
		CUI.Pair<string, CuiElement> pair5 = cui.CreatePanel(container, pair3, CUI.HexToRustColor("#fc5d5d", 0.8f), null, num, num, 0f, 1f, -2.5f, 2.5f, -6f, 6f);
		cui.CreateImage(container, pair5, "fade", Cache.CUI.WhiteColor);
		if (num <= 0.15f)
		{
			cui.CreateText(container, pair2, "1 1 1 1", valueText, 8, num + 0.03f, 1f, 0f, 1f, 0f, 0f, -2.5f, 0f, (TextAnchor)6, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
		}
		else
		{
			cui.CreateText(container, pair2, "1 1 1 1", valueText, 8, 0f, num - 0.03f, 0f, 1f, 0f, 0f, -2.5f, 0f, (TextAnchor)8, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
		}
		float num2 = max.Clamp(min, 50f);
		float num3 = 1f / num2;
		float num4 = 0f;
		for (int i = 0; (float)i < num2; i++)
		{
			cui.CreateProtectedButton(container, pair2, Cache.CUI.BlankColor, Cache.CUI.BlankColor, string.Empty, 0, null, num4, num4 + num3, 0f, 1f, 0f, 0f, 0f, 0f, $"{command} {i}", (TextAnchor)4);
			num4 += num3;
		}
	}

	public void TabPanelButtonArray(CUI cui, CuiElementContainer container, string parent, string command, float height, float offset, PlayerSession session, params Tab.OptionButton[] buttons)
	{
		string blankColor = Cache.CUI.BlankColor;
		float option_LeftOffset = Option_LeftOffset;
		float option_RightOffset = Option_RightOffset;
		CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, parent, blankColor, null, 0f, 1f, offset, offset + height, option_LeftOffset, option_RightOffset);
		float num = 1f / (float)buttons.Length;
		float num2 = 0f;
		for (int i = 0; i < buttons.Length; i++)
		{
			Tab.OptionButton optionButton = buttons[i];
			string color = ((optionButton.Type != null) ? optionButton.Type(session) : Tab.OptionButton.Types.None) switch
			{
				Tab.OptionButton.Types.Selected => base.DataInstance.Colors.ButtonSelectedColor, 
				Tab.OptionButton.Types.Warned => base.DataInstance.Colors.ButtonWarnedColor, 
				Tab.OptionButton.Types.Important => base.DataInstance.Colors.ButtonImportantColor, 
				_ => base.DataInstance.Colors.OptionColor, 
			};
			CUI.Pair<string, CuiElement, CuiElement> pair2 = cui.CreateProtectedButton(container, pair, color, "1 1 1 0.5", optionButton.Name, 11, null, num2, num2 + num, 0f, 1f, 0f, 0f, 0f, 0f, $"{command} {i}", (TextAnchor)4);
			cui.CreateImage(container, pair2, "fade", Cache.CUI.WhiteColor);
			num2 += num;
		}
	}

	public void TabPanelInputButton(CUI cui, CuiElementContainer container, string parent, string text, string command, float buttonPriority, Tab.OptionInput input, Tab.OptionButton button, PlayerSession session, float height, float offset, Tab.Option option = null)
	{
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		string optionColor = base.DataInstance.Colors.OptionColor;
		string color = ((button.Type != null) ? button.Type(null) : Tab.OptionButton.Types.None) switch
		{
			Tab.OptionButton.Types.Selected => base.DataInstance.Colors.ButtonSelectedColor, 
			Tab.OptionButton.Types.Warned => base.DataInstance.Colors.ButtonWarnedColor, 
			Tab.OptionButton.Types.Important => base.DataInstance.Colors.ButtonImportantColor, 
			_ => base.DataInstance.Colors.OptionColor, 
		};
		CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, parent, Cache.CUI.BlankColor, null, 0f, 1f, offset, offset + height);
		if (!string.IsNullOrEmpty(text))
		{
			cui.CreateText(container, pair, base.DataInstance.Colors.OptionNameColor, text + ":", 12, 0f, 1f, 0f, 1f, Option_LeftOffset, Option_RightOffset, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
		}
		CUI.Pair<string, CuiElement> pair2 = cui.CreatePanel(container, pair, optionColor, null, base.DataInstance.Colors.OptionWidth, 1f, 0f, 1f, 0f, Option_RightOffset);
		cui.CreatePanel(container, pair, optionColor, null, 0f, base.DataInstance.Colors.OptionWidth, 0f, 0.015f);
		cui.CreateImage(container, pair2, "fade", Cache.CUI.WhiteColor, null, 0f, 1f - buttonPriority);
		string parent2 = pair2;
		string color2 = $"1 1 1 {(input.ReadOnly ? 0.2f : 1f)}";
		string text2 = input.Placeholder?.Invoke(session);
		float xMax = 1f - buttonPriority;
		string text3 = command + " input";
		cui.CreateProtectedInputField(characterLimit: input.CharacterLimit, readOnly: input.ReadOnly, command: text3, font: CUI.Handler.FontTypes.RobotoCondensedRegular, needsKeyboard: session.Input == option, container: container, parent: parent2, color: color2, text: text2, size: 11, xMin: 0.03f, xMax: xMax, yMin: 0f, yMax: 1f, OxMin: 0f, OxMax: 0f, OyMin: 0f, OyMax: 0f, align: (TextAnchor)3, autoFocus: session.Input == option && session.Input != session.PreviousInput, hudMenuInput: false, lineType: (LineType)0);
		if (session.Input == option)
		{
			session.PreviousInput = session.Input;
		}
		CUI.Pair<string, CuiElement, CuiElement> pair3 = cui.CreateProtectedButton(container, pair2, color, "1 1 1 0.5", button.Name, 11, null, 1f - buttonPriority, 1f, 0f, 1f, 0f, 0f, 0f, 0f, command + " button", button.Align);
		cui.CreateImage(container, pair3, "fade", Cache.CUI.WhiteColor);
		if (!input.ReadOnly)
		{
			cui.CreatePanel(container, pair2, base.DataInstance.Colors.EditableInputHighlight + " 0.9", null, 0f, 1f - buttonPriority, 0f, 0.05f, 0f, -0.5f);
		}
	}

	public void TabPanelColor(CUI cui, CuiElementContainer container, string parent, string text, string color, string command, float height, float offset)
	{
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.825f;
		CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, parent, Cache.CUI.BlankColor, null, 0f, 1f, offset, offset + height);
		if (!string.IsNullOrEmpty(text))
		{
			cui.CreateText(container, pair, base.DataInstance.Colors.OptionNameColor, text + ":", 12, 0f, 1f, 0f, 1f, Option_LeftOffset, Option_RightOffset, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
			cui.CreatePanel(container, pair, base.DataInstance.Colors.OptionColor, null, 0f, num, 0f, 0.015f);
		}
		string[] array = color.Split(' ');
		string text2 = ((array.Length > 1) ? ("#" + ColorUtility.ToHtmlStringRGB(new Color(array[0].ToFloat(), array[1].ToFloat(), array[2].ToFloat(), 1f))) : string.Empty);
		float option_RightOffset = Option_RightOffset;
		cui.CreateProtectedButton(container, parent, color, "1 1 1 1", text2, 10, null, num, 1f, offset, offset + height, 0f, option_RightOffset, 0f, 0f, command, (TextAnchor)4);
	}

	public void TabPanelWidget(CUI cui, CuiElementContainer container, string parent, PlayerSession session, Tab.OptionWidget widget, float height, float offset)
	{
		widget.WidgetPanel = cui.CreatePanel(container, parent, Cache.CUI.BlankColor, null, 0f, 1f, offset, offset + height);
		widget.Callback?.Invoke(session, cui, container, widget.WidgetPanel);
	}

	public void TabPanelChart(CUI cui, CuiElementContainer container, string parent, PlayerSession session, Tab.OptionChart chart, float height, float offset, float panelSpacing, string layerCommand, string layerShadowCommand, Tab tab, int columnIndex)
	{
		//IL_0861: Unknown result type (might be due to invalid IL or missing references)
		PlayerSession.Page currentPage = session.GetOrCreatePage(columnIndex);
		bool flag = !chart.Responsive && tab.Columns.All((KeyValuePair<int, Tab.OptionPool> x) => session.GetOrCreatePage(x.Key).CurrentPage == currentPage.CurrentPage);
		float num = (float)((chart.Responsive || !flag) ? 1 : tab.Columns.Count) + panelSpacing * (float)tab.Columns.Count;
		CUI.Pair<string, CuiElement> panel = cui.CreatePanel(container, parent, Cache.CUI.BlankColor, null, 0f, 1f * num, offset, offset + height);
		if (!chart.Responsive && !flag)
		{
			cui.CreatePanel(container, panel, "0.15 0.15 0.15 0.3", null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 25f, blur: true);
			cui.CreateText(container, panel, "1 1 1 0.5", "To view the chart,\nremain on the same page number as this.", 10, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
			return;
		}
		if (chart.IsEmpty())
		{
			cui.CreatePanel(container, panel, "0.15 0.15 0.15 0.3", null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 25f, blur: true);
			cui.CreateText(container, panel, "1 1 1 0.5", "No data available.", 10, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
			return;
		}
		if (!chart.Responsive)
		{
			cui.CreatePanel(container, panel, "0.15 0.15 0.15 0.3", null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 25f, blur: true);
		}
		CuiRectTransform contentTransformComponent;
		CuiScrollbar horizontalScrollBar;
		CuiScrollbar verticalScrollBar;
		CUI.Pair<string, CuiElement> pair = cui.CreateScrollView(container, panel, vertical: false, horizontal: true, (MovementType)2, 0.5f, inertia: true, 0.3f, 120f, out contentTransformComponent, out horizontalScrollBar, out verticalScrollBar);
		contentTransformComponent.AnchorMin = "0 0";
		contentTransformComponent.AnchorMax = "0 1";
		contentTransformComponent.OffsetMin = "0 0";
		contentTransformComponent.OffsetMax = "3500 0";
		horizontalScrollBar.TrackColor = "0 0 0 0";
		CuiScrollbar cuiScrollbar = horizontalScrollBar;
		string handleColor = (horizontalScrollBar.HighlightColor = "0.2 0.2 0.2 1");
		cuiScrollbar.HandleColor = handleColor;
		horizontalScrollBar.Size = 1f;
		horizontalScrollBar.Invert = true;
		CUI.Pair<string, CuiElement> pair2 = cui.CreatePanel(container, panel, Cache.CUI.BlankColor, null, 0f, 0.05f, 0.078f);
		string identifier = chart.GetIdentifier();
		int num2 = chart.Chart.verticalLabels.Length;
		if (num2 != 1)
		{
			int num3 = 0;
			for (int num4 = 0; num4 < chart.Chart.verticalLabels.Length; num4++)
			{
				string text2 = chart.Chart.verticalLabels[num4];
				int num5 = num3.Scale(0, num2 - 1, 0, 150);
				cui.CreateText(container, pair2, "1 1 1 0.9", text2, 7, 0f, 0.9f, 0f, 0f, 0f, 0f, num5, num5, (TextAnchor)5, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				num3++;
			}
		}
		int layerIndex = -1;
		float xOffset = 0f;
		float xOffsetWidth = 47.5f;
		int xMoving = 50;
		int spacing = -5;
		CUI.Pair<string, CuiElement> loadingOverlay = cui.CreatePanel(container, panel, "0 0 0 0.2", null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, blur: true, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, identifier + "_loading");
		CUI.Pair<string, CuiElement> loadingText = cui.CreateText(container, loadingOverlay, "1 1 1 0.5", "Please wait...", 10, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, identifier + "_loadingtxt");
		CUI.Pair<string, CuiElement> chartImage = cui.CreateImage(container, pair, 0u, Cache.CUI.WhiteColor, null, 0.01f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, identifier + "_chart");
		CreateLayerButton("All", Color.BlanchedAlmond, chart.Chart.Layers.All((Chart.Layer x) => x.Disabled), !chart.Chart.Layers.All((Chart.Layer x) => x.LayerSettings.Shadows == 0));
		for (int num6 = 0; num6 < chart.Chart.Layers.Length; num6++)
		{
			Chart.Layer layer = chart.Chart.Layers[num6];
			CreateLayerButton(layer.Name, layer.LayerSettings.Color, !layer.Disabled, layer.LayerSettings.Shadows > 0);
		}
		cui.CreateText(container, panel, Cache.CUI.WhiteColor, chart.Name, chart.NameSize, 0.025f, 0.95f, 1f, 1f, 0f, 0f, 10f, 17.5f, chart.NameAlign, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
		Community.Runtime.Core.NextFrame(delegate
		{
			Tab.OptionChart.Cache.GetOrProcessCache(identifier, chart.Chart, delegate(Tab.OptionChart.ChartCache chartCache)
			{
				//IL_003d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0063: Unknown result type (might be due to invalid IL or missing references)
				using CUI cui2 = new CUI(Handler);
				using CUI.Handler.UpdatePool updatePool = cui2.UpdatePool();
				switch (chartCache.Status)
				{
				case Tab.OptionChart.ChartCache.StatusTypes.Finalized:
					if (!chartCache.HasPlayerReceivedData(EncryptedValue<ulong>.op_Implicit(session.Player.userID)))
					{
						((BaseEntity)CommunityEntity.ServerInstance).ClientRPC(RpcTarget.Player("CL_ReceiveFilePng", session.Player), chartCache.Crc, (uint)chartCache.Data.Length, (ReadOnlySpan<byte>)chartCache.Data, 0u, (byte)0);
					}
					updatePool.Add(cui2.UpdatePanel(loadingOverlay, "0 0 0 0", null, 0f, 0f));
					updatePool.Add(cui2.UpdateText(loadingText, "0 0 0 0", string.Empty, 0, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1));
					updatePool.Add(cui2.UpdateImage(chartImage, chartCache.Crc, Cache.CUI.WhiteColor));
					updatePool.Send(session.Player);
					break;
				default:
					updatePool.Add(cui2.UpdateText(loadingText, "0.9 0.1 0.1 0.75", "Failed to load chart!", 10, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1));
					updatePool.Send(session.Player);
					break;
				}
			});
		});
		void CreateLayerButton(string text3, Color color, bool mainEnabled, bool secondEnabled)
		{
			int length = text3.Length;
			Color color2 = color;
			Color color3 = Color.FromArgb((int)((float)(int)color2.R * 1.5f).Clamp(0f, 255f), (int)((float)(int)color2.G * 1.5f).Clamp(0f, 255f), (int)((float)(int)color2.B * 1.5f).Clamp(0f, 255f));
			string textColor = $"{(float)(int)color3.R / 255f} {(float)(int)color3.G / 255f} {(float)(int)color3.B / 255f} 1";
			CUI.Pair<string, CuiElement, CuiElement> pair3 = cui.CreateProtectedButton(container, panel, string.Format("{0} {1} {2} {3}", new object[4]
			{
				(float)(int)color2.R / 255f,
				(float)(int)color2.G / 255f,
				(float)(int)color2.B / 255f,
				(!mainEnabled) ? 0.15 : 0.5
			}), textColor, "    " + text3, 8, null, 0.01f, 0f, 0.94f, 1f, (float)xMoving + xOffset, (float)xMoving + (xOffset += xOffsetWidth + (float)length * 3f), -15f, -15f, string.Format("{0} {1} {2} {3}", new object[4] { layerCommand, layerIndex, identifier, layerCommand }), (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, $"{identifier}_layerbtn_{layerIndex}");
			cui.CreateProtectedButton(container, pair3, string.Format("{0} {1} {2} {3}", new object[4]
			{
				(float)(int)color2.R / 255f,
				(float)(int)color2.G / 255f,
				(float)(int)color2.B / 255f,
				(!secondEnabled) ? 0.15 : 0.5
			}), textColor, "⦿", 8, null, 0f, 0f, 0f, 1f, 0f, 12.5f, 0f, 0f, string.Format("{0} {1} {2} {3}", new object[4] { layerShadowCommand, layerIndex, identifier, layerShadowCommand }), (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, $"{identifier}_layerbtn2_{layerIndex}");
			xOffset += spacing;
			layerIndex++;
		}
	}

	public void TabTooltip(CUI cui, CuiElementContainer container, string parent, Tab.Option tooltip, string command, PlayerSession admin, float height, float offset)
	{
		if (admin.Tooltip == tooltip)
		{
			CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, parent, "#1a6498", null, 0.05f, MathEx.Scale(admin.Tooltip.Tooltip.Length, 1f, 78f, 0.1f, 0.79f), offset, offset + height);
			cui.CreateText(container, pair, "#6bc0fc", admin.Tooltip.Tooltip, 10, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
		}
		if (!string.IsNullOrEmpty(tooltip.Tooltip))
		{
			cui.CreateProtectedButton(container, parent, Cache.CUI.BlankColor, Cache.CUI.BlankColor, string.Empty, 0, null, 0f, base.DataInstance.Colors.OptionWidth, offset, offset + height, 0f, 0f, 0f, 0f, command + " tooltip", (TextAnchor)4);
		}
	}

	public void Draw(BasePlayer player)
	{
		try
		{
			PlayerSession playerSession = GetPlayerSession(player);
			Tab tab = GetTab(player);
			playerSession.IsInMenu = true;
			if (CanAccess(player) && !base.DataInstance.GreetDisplayed && tab != null && tab.Id != "greet" && tab.Id != "configeditor" && HasAccess(player, "greet"))
			{
				tab = (playerSession.SelectedTab = Greet.Make());
			}
			if (playerSession.SelectedTab != null && ((!string.IsNullOrEmpty(playerSession.SelectedTab.Access) && !HasAccess(player, playerSession.SelectedTab.Access)) || base.DataInstance.IsTabHidden(playerSession.SelectedTab.Id)))
			{
				playerSession.SelectedTab = null;
			}
			using CUI cUI = new CUI(Handler);
			CuiElementContainer cuiElementContainer = cUI.CreateContainer("carbonmodularui", $"0 0 0 {base.DataInstance.BackgroundOpacity}", 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, needsCursor: true, needsKeyboard: false, CUI.ClientPanels.HudMenu, "carbonmodularui");
			cUI.CreatePanel(cuiElementContainer, "carbonmodularui", "0 0 0 0.6");
			cUI.CreatePanel(cuiElementContainer, "carbonmodularui", "0 0 0 0.5", null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, base.DataInstance.BackgroundBlur);
			cUI.CreateImage(cuiElementContainer, "carbonmodularui", "fade", Cache.CUI.WhiteColor);
			bool maximize = base.DataInstance.Maximize;
			CUI.Pair<string, CuiElement> pair = cUI.CreatePanel(cuiElementContainer, "carbonmodularui", "0 0 0 0.6", null, 0.5f, 0.5f, 0.5f, 0.5f, -475f * (maximize ? 1.1f : 1f), 475f * (maximize ? 1.1f : 1f), -300f * (maximize ? 1.15f : 1f), 300f * (maximize ? 1.15f : 1f), blur: false, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, "carbonmodularuicolor");
			cUI.CreateImage(cuiElementContainer, pair, base.DataInstance.BackgroundImage, "1 1 1 " + base.DataInstance.BackgroundImageOpacity, null, 0f, 1f, base.DataInstance.BackgroundImageYAnchor.x, base.DataInstance.BackgroundImageYAnchor.y);
			using (TimeMeasure.New(Name + ".Main"))
			{
				if (tab == null || !tab.IsFullscreen)
				{
					cUI.CreateText(cuiElementContainer, pair, "1 1 1 0.8", Title, 18, 0.0175f, 1f, 0.8f, 0.97f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
					try
					{
						CUI.Pair<string, CuiElement> pair2 = cUI.CreatePanel(cuiElementContainer, pair, "0 0 0 0.6", null, 0.01f, 0.99f, 0.875f, 0.92f);
						IEnumerable<Tab> enumerable = Tabs.Where((Tab x) => !base.DataInstance.IsTabHidden(x.Id));
						float num = 0f;
						int num2 = enumerable.Count();
						float num3 = ((num2 == 0) ? 0f : (1f / (float)num2));
						for (int num4 = playerSession.TabSkip; num4 < num2; num4++)
						{
							Tab tab2 = enumerable.ElementAt(playerSession.TabSkip + num4);
							if (!base.DataInstance.IsTabHidden(tab2.Id))
							{
								string text = (tab2.Plugin.IsCorePlugin ? string.Empty : ("<size=8>\nby " + tab2.Plugin?.Name + "</size>"));
								TabButton(cUI, cuiElementContainer, pair2, ((enumerable.IndexOf(playerSession.SelectedTab) == num4) ? ("<b>" + tab2.Name + "</b>") : tab2.Name) + text, "carbonmodularui.changetab " + tab2.Id, num3, num, enumerable.IndexOf(playerSession.SelectedTab) == num4, !HasAccess(player, tab2.Access));
								num += num3;
							}
						}
					}
					catch (Exception ex)
					{
						PutsError($"Draw({player}).Tabs", ex);
					}
				}
			}
			try
			{
				using (TimeMeasure.New(Name + ".Panels/Overrides"))
				{
					CUI.Pair<string, CuiElement> pair3 = cUI.CreatePanel(cuiElementContainer, pair, Cache.CUI.BlankColor, null, 0.01f, 0.99f, 0.02f, (tab != null && tab.IsFullscreen) ? 0.98f : 0.86f);
					cUI.CreateImage(cuiElementContainer, pair3, "fade", Cache.CUI.WhiteColor);
					if (tab != null)
					{
						tab.Under?.Invoke(tab, cUI, cuiElementContainer, pair3, playerSession);
						if (tab.Override == null)
						{
							float num5 = 0.005f;
							float num6 = ((tab.Columns.Count == 0) ? 0f : (1f / (float)tab.Columns.Count)) - num5;
							float num7 = (num6 + num5 * 2f) * (float)(tab.Columns.Count - 1);
							int count = tab.Columns.Count;
							while (count-- > 0)
							{
								Tab.OptionPool optionPool = tab.Columns[count];
								CUI.Pair<string, CuiElement> pair4 = cUI.CreatePanel(cuiElementContainer, pair3, "0 0 0 " + base.DataInstance.BackgroundColumnOpacity, null, num7, num7 + num6 - num5, 0f, 1f, 0f, 0f, 0f, 0f, blur: false, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, $"sub{count}");
								cUI.CreateImage(cuiElementContainer, pair4, "fade", "1 1 1 " + base.DataInstance.BackgroundColumnOpacity);
								PlayerSession.Page orCreatePage = playerSession.GetOrCreatePage(count);
								int num8 = 19 - ((optionPool.pinnedOption != null) ? 1 : 0);
								float num9 = 0.04f;
								IEnumerable<Tab.Option> source = optionPool.Skip(num8 * orCreatePage.CurrentPage).Take(num8);
								int num10 = source.Count();
								orCreatePage.TotalPages = (int)Math.Ceiling((double)optionPool.Count / (double)num8 - 1.0);
								orCreatePage.Check();
								float num11 = (num9 + 0.01f) * (float)(num8 - (num10 - ((orCreatePage.TotalPages <= 0) ? 1 : 0)));
								if (num10 == 0)
								{
									cUI.CreateText(cuiElementContainer, pair4, "1 1 1 0.35", GetPhrase("nocontent", player.UserIDString), 8, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
								}
								if (orCreatePage.TotalPages > 0)
								{
									num9 += 0.0035f;
									TabColumnPagination(cUI, cuiElementContainer, pair4, count, orCreatePage, num9, 0f);
									num9 -= 0.0035f;
									num11 += num9 + 0.01f;
								}
								int num12 = num10;
								while (num12-- > 0)
								{
									int num13 = num12 + orCreatePage.CurrentPage * num8;
									Tab.Option row = optionPool[num13];
									num9 += 0.0035f;
									DrawRow(cUI, cuiElementContainer, pair4, row, count, num13, num9, num11, playerSession, tab);
									num9 -= 0.0035f;
									num11 += num9 + 0.01f;
								}
								if (optionPool.pinnedOption != null)
								{
									DrawRow(cUI, cuiElementContainer, pair4, optionPool.pinnedOption, count, -1, num9, num11, playerSession, tab);
								}
								num7 -= num6 + num5 * 2f;
							}
						}
						else
						{
							tab.Override(tab, cUI, cuiElementContainer, pair3, playerSession);
						}
						tab.Over?.Invoke(tab, cUI, cuiElementContainer, pair3, playerSession);
						if (tab.Dialog != null)
						{
							CUI.Pair<string, CuiElement> pair5 = cUI.CreatePanel(cuiElementContainer, pair3, "0.15 0.15 0.15 0.2", null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, blur: true);
							cUI.CreatePanel(cuiElementContainer, pair5, "0 0 0 0.9");
							cUI.CreateText(cuiElementContainer, pair5, "1 1 1 1", tab.Dialog.Title, 20, 0f, 1f, 0.1f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
							cUI.CreateText(cuiElementContainer, pair5, "1 1 1 0.4", "Confirm action".ToUpper().SpacedString(3), 10, 0f, 1f, 0.2f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
							cUI.CreateProtectedButton(cuiElementContainer, pair5, "0.9 0.4 0.3 0.8", "1 1 1 0.7", "DECLINE".SpacedString(1), 10, null, 0.4f, 0.49f, 0.425f, 0.475f, 0f, 0f, 0f, 0f, "carbonmodularui.dialogaction decline", (TextAnchor)4);
							cUI.CreateProtectedButton(cuiElementContainer, pair5, "0.4 0.9 0.3 0.8", "1 1 1 0.7", "CONFIRM".SpacedString(1), 10, null, 0.51f, 0.6f, 0.425f, 0.475f, 0f, 0f, 0f, 0f, "carbonmodularui.dialogaction confirm", (TextAnchor)4);
						}
					}
					else
					{
						cUI.CreateText(cuiElementContainer, pair3, "1 1 1 0.4", "No tab selected.", 9, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
					}
				}
			}
			catch (Exception ex2)
			{
				PutsError($"Draw({player}).Panels", ex2);
			}
			using (TimeMeasure.New(Name + ".Exit"))
			{
				int num14 = ((tab == null || tab.IsFullscreen) ? 15 : 0);
				CUI.Pair<string, CuiElement, CuiElement> pair6 = cUI.CreateProtectedButton(cuiElementContainer, pair, "#d1cd56", Cache.CUI.BlankColor, string.Empty, 0, null, 0.9675f, 0.99f, 0.955f, 0.99f, -75f, -75f, num14, num14, "carbonmodularui.maximize", (TextAnchor)4);
				cUI.CreateImage(cuiElementContainer, pair6, base.DataInstance.Maximize ? "minimize" : "maximize", "#fffed4", null, 0.15f, 0.85f, 0.15f, 0.85f);
				cUI.CreateImage(cuiElementContainer, pair6, "fade", Cache.CUI.WhiteColor);
				bool flag = HasAccess(playerSession.Player, "profiler.use");
				CUI.Pair<string, CuiElement, CuiElement> pair7 = cUI.CreateProtectedButton(cuiElementContainer, pair, (!flag) ? "0.3 0.3 0.3 0.7" : "#6651c2", Cache.CUI.BlankColor, string.Empty, 0, null, 0.9675f, 0.99f, 0.955f, 0.99f, -50f, -50f, num14, num14, flag ? "carbonmodularui.profiler" : string.Empty, (TextAnchor)4);
				cUI.CreateImage(cuiElementContainer, pair7, "graph", "#af9ff5", null, 0.15f, 0.85f, 0.15f, 0.85f);
				cUI.CreateImage(cuiElementContainer, pair7, "fade", Cache.CUI.WhiteColor);
				if (playerSession.SelectedTab != null && playerSession.SelectedTab.Id == "profiler")
				{
					cUI.CreatePanel(cuiElementContainer, pair7, "1 0 0 1", null, 0f, 1f, 0f, 0.1f);
				}
				bool flag2 = HasAccess(playerSession.Player, "config.use");
				CUI.Pair<string, CuiElement, CuiElement> pair8 = cUI.CreateProtectedButton(cuiElementContainer, pair, flag2 ? "0.2 0.6 0.2 0.9" : "0.3 0.3 0.3 0.7", Cache.CUI.BlankColor, string.Empty, 0, null, 0.9675f, 0.99f, 0.955f, 0.99f, -25f, -25f, num14, num14, flag2 ? "carbonmodularui.config" : string.Empty, (TextAnchor)4);
				cUI.CreateImage(cuiElementContainer, pair8, "gear", "0.5 1 0.5 1", null, 0.15f, 0.85f, 0.15f, 0.85f);
				cUI.CreateImage(cuiElementContainer, pair8, "fade", Cache.CUI.WhiteColor);
				if (playerSession.SelectedTab != null && playerSession.SelectedTab.Id == "configuration")
				{
					cUI.CreatePanel(cuiElementContainer, pair8, "1 0 0 1", null, 0f, 1f, 0f, 0.1f);
				}
				CUI.Pair<string, CuiElement, CuiElement> pair9 = cUI.CreateProtectedButton(cuiElementContainer, pair, "0.6 0.2 0.2 0.9", Cache.CUI.BlankColor, string.Empty, 0, null, 0.9675f, 0.99f, 0.955f, 0.99f, 0f, 0f, num14, num14, "carbonmodularui.close", (TextAnchor)4);
				cUI.CreateImage(cuiElementContainer, pair9, "close", "1 0.5 0.5 1", null, 0.2f, 0.8f, 0.2f, 0.8f);
				cUI.CreateImage(cuiElementContainer, pair9, "fade", Cache.CUI.WhiteColor);
			}
			using (TimeMeasure.New(Name + ".Send"))
			{
				cUI.Send(cuiElementContainer, player);
			}
		}
		catch (Exception ex3)
		{
			PutsError("Draw(player) failed.", ex3);
		}
		Subscribe("OnPluginLoaded");
		Subscribe("OnPluginUnloaded");
		static void DrawRow(CUI cui, CuiElementContainer container, string panel, Tab.Option option, int i, int actualI, float rowHeight, float rowIndex, PlayerSession ap, Tab tab3)
		{
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			if (!(option is Tab.OptionName optionName))
			{
				if (!(option is Tab.OptionButton optionButton))
				{
					if (!(option is Tab.OptionText optionText))
					{
						if (!(option is Tab.OptionInput optionInput))
						{
							if (!(option is Tab.OptionEnum optionEnum))
							{
								if (!(option is Tab.OptionToggle optionToggle))
								{
									if (!(option is Tab.OptionDropdown optionDropdown))
									{
										if (!(option is Tab.OptionRange optionRange))
										{
											if (!(option is Tab.OptionButtonArray optionButtonArray))
											{
												if (!(option is Tab.OptionInputButton optionInputButton))
												{
													if (!(option is Tab.OptionColor optionColor))
													{
														if (!(option is Tab.OptionWidget optionWidget))
														{
															if (option is Tab.OptionChart chart)
															{
																Singleton.TabPanelChart(cui, container, panel, ap, chart, rowHeight * 9f, rowIndex, 0.01f, "carbonmodularui" + $".callaction {i} {actualI} layer", "carbonmodularui" + $".callaction {i} {actualI} layershadow", tab3, i);
															}
														}
														else
														{
															Singleton.TabPanelWidget(cui, container, panel, ap, optionWidget, rowHeight * (float)(optionWidget.Height + 1), rowIndex);
														}
													}
													else
													{
														Singleton.TabPanelColor(cui, container, panel, optionColor.Name, optionColor.Color?.Invoke() ?? "0.1 0.1 0.1 0.5", "carbonmodularui" + $".callaction {i} {actualI}", rowHeight, rowIndex);
														HandleReveal(Singleton.DataInstance.Colors.OptionWidth, option, cui, container, panel, rowIndex, rowHeight, i, actualI);
													}
												}
												else
												{
													Singleton.TabPanelInputButton(cui, container, panel, optionInputButton.Name, "carbonmodularui" + $".callaction {i} {actualI}", optionInputButton.ButtonPriority, optionInputButton.Input, optionInputButton.Button, ap, rowHeight, rowIndex, optionInputButton);
													HandleReveal(Singleton.DataInstance.Colors.OptionWidth, option, cui, container, panel, rowIndex, rowHeight, i, actualI);
													HandleInputHighlight(Singleton.DataInstance.Colors.OptionWidth, option, cui, container, panel, ap, rowIndex, rowHeight, i, actualI, 1f - optionInputButton.ButtonPriority, "input");
												}
											}
											else
											{
												Singleton.TabPanelButtonArray(cui, container, panel, "carbonmodularui" + $".callaction {i} {actualI}", rowHeight, rowIndex, ap, optionButtonArray.Buttons);
											}
										}
										else
										{
											Singleton.TabPanelRange(cui, container, panel, optionRange.Name, "carbonmodularui" + $".callaction {i} {actualI}", optionRange.Text?.Invoke(ap), optionRange.Min, optionRange.Max, (optionRange.Value == null) ? 0f : optionRange.Value(ap), rowHeight, rowIndex);
											HandleReveal(Singleton.DataInstance.Colors.OptionWidth, option, cui, container, panel, rowIndex, rowHeight, i, actualI);
										}
									}
									else
									{
										Singleton.TabPanelDropdown(cui, ap._selectedDropdownPage, container, panel, optionDropdown.Name, "carbonmodularui" + $".callaction {i} {actualI}", rowHeight, rowIndex, optionDropdown.Index(ap), optionDropdown.Options, optionDropdown.OptionsIcons, ap._selectedDropdown == optionDropdown);
										HandleReveal(Singleton.DataInstance.Colors.OptionWidth, option, cui, container, panel, rowIndex, rowHeight, i, actualI);
									}
								}
								else
								{
									Singleton.TabPanelToggle(cui, container, panel, optionToggle.Name, "carbonmodularui" + $".callaction {i} {actualI}", rowHeight, rowIndex, optionToggle.IsOn != null && optionToggle.IsOn(ap), tab3);
									HandleReveal(Singleton.DataInstance.Colors.OptionWidth, option, cui, container, panel, rowIndex, rowHeight, i, actualI);
								}
							}
							else
							{
								Singleton.TabPanelEnum(cui, container, panel, optionEnum.Name, optionEnum.Text?.Invoke(ap), "carbonmodularui" + $".callaction {i} {actualI}", rowHeight, rowIndex);
								HandleReveal(Singleton.DataInstance.Colors.OptionWidth, option, cui, container, panel, rowIndex, rowHeight, i, actualI);
							}
						}
						else
						{
							Singleton.TabPanelInput(cui, container, panel, optionInput.Name, optionInput.Placeholder?.Invoke(ap), "carbonmodularui" + $".callaction {i} {actualI}", optionInput.CharacterLimit, optionInput.ReadOnly, rowHeight, rowIndex, ap, Tab.OptionButton.Types.None, optionInput);
							HandleReveal(Singleton.DataInstance.Colors.OptionWidth, option, cui, container, panel, rowIndex, rowHeight, i, actualI);
							HandleInputHighlight(Singleton.DataInstance.Colors.OptionWidth, option, cui, container, panel, ap, rowIndex, rowHeight, i, actualI);
						}
					}
					else
					{
						Singleton.TabPanelText(cui, container, panel, optionText.Name, optionText.Size, optionText.Color, rowHeight, rowIndex, optionText.Align, optionText.Font, optionText.IsInput);
						HandleReveal(0f, option, cui, container, panel, rowIndex, rowHeight, i, actualI);
					}
				}
				else
				{
					Singleton.TabPanelButton(cui, container, panel, optionButton.Name, "carbonmodularui" + $".callaction {i} {actualI}", rowHeight, rowIndex, (optionButton.Type != null) ? optionButton.Type(ap) : Tab.OptionButton.Types.None, optionButton.Align);
					HandleReveal(0f, option, cui, container, panel, rowIndex, rowHeight, i, actualI);
				}
			}
			else
			{
				Singleton.TabPanelName(cui, container, panel, optionName.Name, rowHeight, rowIndex, optionName.Align);
				HandleReveal(0f, option, cui, container, panel, rowIndex, rowHeight, i, actualI);
			}
			Singleton.TabTooltip(cui, container, panel, option, "carbonmodularui" + $".callaction {i} {actualI}", ap, rowHeight, rowIndex);
		}
		static void HandleInputHighlight(float xMin, Tab.Option option, CUI cui, CuiElementContainer container, string panel, PlayerSession ap, float rowIndex, float rowHeight, int i, int actualI, float xMax = 0.985f, string command = null)
		{
			if (option != ap.Input)
			{
				cui.CreateProtectedButton(container, panel, Cache.CUI.BlankColor, Cache.CUI.BlankColor, string.Empty, 0, null, xMin, xMax, rowIndex, rowIndex + rowHeight, 0f, 0f, 0f, 0f, "carbonmodularui" + $".callaction {i} {actualI} {command}", (TextAnchor)4);
			}
		}
		static void HandleReveal(float xMin, Tab.Option option, CUI cui, CuiElementContainer container, string panel, float rowIndex, float rowHeight, int i, int actualI)
		{
			if (option.CurrentlyHidden)
			{
				float option_RightOffset = Option_RightOffset;
				CUI.Pair<string, CuiElement> pair10 = cui.CreatePanel(container, panel, "0 0 0 0.4", null, xMin, 1f, rowIndex, rowIndex + rowHeight, 0f, option_RightOffset, 0f, 0f, blur: true);
				cui.CreateImage(container, pair10, "fade", Cache.CUI.WhiteColor);
				cui.CreateProtectedButton(container, pair10, Cache.CUI.BlankColor, "1 1 1 0.5", "REVEAL".SpacedString(1), 8, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "carbonmodularui" + $".callaction {i} {actualI}", (TextAnchor)4);
			}
		}
	}

	public void DrawCursorLocker(BasePlayer player)
	{
		using CUI cUI = new CUI(Handler);
		CuiElementContainer container = cUI.CreateContainer("carbonmodularuicur", Cache.CUI.BlankColor, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.005f, 0f, needsCursor: true, needsKeyboard: false, CUI.ClientPanels.Overlay, "carbonmodularuicur");
		cUI.Send(container, player);
	}

	public void Close(BasePlayer player)
	{
		Handler.Destroy("carbonmodularui", player);
		Handler.Destroy("carbonmodularuicur", player);
		PlayerSession playerSession = GetPlayerSession(player);
		playerSession.IsInMenu = false;
		playerSession.SelectedTab?.ResetHiddens();
		bool flag = true;
		foreach (KeyValuePair<BasePlayer, PlayerSession> playerSession2 in PlayerSessions)
		{
			if (playerSession2.Value.IsInMenu)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			Unsubscribe("OnPluginLoaded");
			Unsubscribe("OnPluginUnloaded");
		}
	}

	public void RegisterTab(Tab tab, int? insert = null)
	{
		Tab tab2 = Tabs.FirstOrDefault((Tab x) => x.Id == tab.Id);
		if (tab2 != null)
		{
			int num = Tabs.IndexOf(tab2);
			Tabs.RemoveAt(num);
			tab2 = null;
			Tabs.Insert(insert ?? num, tab);
		}
		else if (insert.HasValue)
		{
			Tabs.Insert(insert.Value, tab);
		}
		else
		{
			Tabs.Add(tab);
		}
	}

	public void UnregisterTab(string id)
	{
		for (int i = 0; i < Tabs.Count; i++)
		{
			Tab tab = Tabs[i];
			if (tab.Id == id)
			{
				tab.Dispose();
				Tabs.RemoveAt(i);
				i--;
			}
		}
	}

	public void UnregisterAllTabs()
	{
		Tabs.Clear();
	}

	public void SetTab(BasePlayer player, string id, bool onChange = true)
	{
		PlayerSession playerSession = GetPlayerSession(player);
		Tab selectedTab = playerSession.SelectedTab;
		Tab tab = Tabs.FirstOrDefault((Tab x) => !base.DataInstance.IsTabHidden(x.Id) && HasAccess(player, x.Access) && x.Id == id);
		if (tab != null)
		{
			playerSession.Tooltip = null;
			if (onChange)
			{
				try
				{
					tab?.OnChange?.Invoke(playerSession, tab);
				}
				catch
				{
				}
			}
		}
		playerSession.SelectedTab = tab;
		if (playerSession.SelectedTab != selectedTab)
		{
			playerSession.Input = (playerSession.PreviousInput = null);
			playerSession.SelectedTab?.ResetHiddens();
			Draw(player);
		}
	}

	public void SetTab(BasePlayer player, Tab tab, bool onChange = true)
	{
		PlayerSession playerSession = GetPlayerSession(player);
		Tab selectedTab = playerSession.SelectedTab;
		tab = (string.IsNullOrEmpty(tab.Access) ? tab : (HasAccess(player, tab.Access) ? tab : Tabs.FirstOrDefault((Tab x) => !base.DataInstance.IsTabHidden(x.Id) && HasAccess(player, x.Access))));
		if (base.DataInstance.IsTabHidden(tab.Id))
		{
			tab = null;
		}
		if (tab != null)
		{
			playerSession.Tooltip = null;
			if (onChange)
			{
				try
				{
					tab?.OnChange?.Invoke(playerSession, tab);
				}
				catch
				{
				}
			}
		}
		playerSession.SelectedTab = tab;
		if (playerSession.SelectedTab != selectedTab)
		{
			playerSession.Input = (playerSession.PreviousInput = null);
			playerSession.SelectedTab?.ResetHiddens();
			Draw(player);
		}
	}

	public Tab GetTab(BasePlayer player)
	{
		if (Tabs.Count == 0)
		{
			return null;
		}
		PlayerSession playerSession = GetPlayerSession(player);
		if (playerSession.SelectedTab == null)
		{
			return null;
		}
		return playerSession.SelectedTab;
	}

	public Tab FindTab(string id)
	{
		for (int i = 0; i < Tabs.Count; i++)
		{
			Tab tab = Tabs[i];
			if (tab.Id == id)
			{
				return tab;
			}
		}
		return null;
	}

	public bool HasTab(string id)
	{
		return FindTab(id) != null;
	}

	public bool CallColumnRow(BasePlayer player, int column, int row, object[] args)
	{
		PlayerSession ap = GetPlayerSession(player);
		Tab tab = GetTab(player);
		ap.LastPressedColumn = column;
		ap.LastPressedRow = row;
		Tab.OptionPool optionPool = tab.Columns[column];
		Tab.Option option = ((row == -1) ? optionPool.pinnedOption : optionPool[row]);
		if (args.Length != 0 && (string)args[0] == "tooltip")
		{
			if (ap.Tooltip != option)
			{
				ap.Tooltip = option;
			}
			else
			{
				ap.Tooltip = null;
			}
			return true;
		}
		if (option.CurrentlyHidden)
		{
			option.CurrentlyHidden = false;
			return true;
		}
		if (!(option is Tab.OptionButton optionButton))
		{
			if (!(option is Tab.OptionInput optionInput))
			{
				if (!(option is Tab.OptionEnum optionEnum))
				{
					if (!(option is Tab.OptionToggle optionToggle))
					{
						if (!(option is Tab.OptionDropdown optionDropdown))
						{
							if (!(option is Tab.OptionRange optionRange))
							{
								if (!(option is Tab.OptionButtonArray optionButtonArray))
								{
									if (!(option is Tab.OptionInputButton optionInputButton))
									{
										Tab.OptionColor optionColor = option as Tab.OptionColor;
										if (optionColor == null)
										{
											if (option is Tab.OptionChart optionChart)
											{
												int num = ((string)args[1]).ToInt();
												object obj = args[0];
												switch (obj as string)
												{
												case "layer":
												{
													object oldIdentifier2 = args[2];
													string empty2 = string.Empty;
													using CUI cui2 = new CUI(Handler);
													using CUI.Handler.UpdatePool updatePool2 = cui2.UpdatePool();
													bool flag3 = false;
													Color blanchedAlmond2 = Color.BlanchedAlmond;
													string text2 = "    All";
													if (num == -1)
													{
														bool flag4 = optionChart.Chart.Layers.All((Chart.Layer x) => x.Disabled);
														for (int num3 = 0; num3 < optionChart.Chart.Layers.Length; num3++)
														{
															optionChart.Chart.Layers[num3].Disabled = !flag4;
														}
														empty2 = optionChart.GetIdentifier(reset: true);
														return true;
													}
													Chart.Layer layer3 = optionChart.Chart.Layers[num];
													layer3.ToggleDisabled();
													empty2 = optionChart.GetIdentifier(reset: true);
													flag3 = !layer3.Disabled;
													blanchedAlmond2 = layer3.LayerSettings.Color;
													text2 = "    " + layer3.Name;
													Color color2 = Color.FromArgb((int)((float)(int)blanchedAlmond2.R * 1.5f).Clamp(0f, 255f), (int)((float)(int)blanchedAlmond2.G * 1.5f).Clamp(0f, 255f), (int)((float)(int)blanchedAlmond2.B * 1.5f).Clamp(0f, 255f));
													string textColor2 = $"{(float)(int)color2.R / 255f} {(float)(int)color2.G / 255f} {(float)(int)color2.B / 255f} 1";
													string text3 = args.Select((object x) => x as string).Skip(3).ToString(" ");
													updatePool2.Add(cui2.UpdatePanel($"{oldIdentifier2}_loading", "0 0 0 0.2", null, 0.01f, 0.99f, 0.01f, 0.99f, 0f, 0f, 0f, 0f, blur: true));
													updatePool2.Add(cui2.UpdateText($"{oldIdentifier2}_loadingtxt", "1 1 1 0.5", "Please wait...", 10, 0.01f, 0.99f, 0.01f, 0.99f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1));
													updatePool2.Add(cui2.UpdateImage($"{oldIdentifier2}_chart", 0u, Cache.CUI.WhiteColor, null, 0.01f));
													updatePool2.Add(cui2.UpdateProtectedButton($"{oldIdentifier2}_layerbtn_{num}", string.Format("{0} {1} {2} {3}", new object[4]
													{
														(float)(int)blanchedAlmond2.R / 255f,
														(float)(int)blanchedAlmond2.G / 255f,
														(float)(int)blanchedAlmond2.B / 255f,
														(!flag3) ? 0.15 : 0.5
													}), textColor2, text2, 8, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, string.Format("{0} {1} {2} {3}", new object[4] { text3, num, oldIdentifier2, text3 }), (TextAnchor)4));
													updatePool2.Send(ap.Player);
													Tab.OptionChart.Cache.GetOrProcessCache(empty2, optionChart.Chart, delegate(Tab.OptionChart.ChartCache chartCache)
													{
														//IL_0047: Unknown result type (might be due to invalid IL or missing references)
														//IL_0072: Unknown result type (might be due to invalid IL or missing references)
														using CUI cui3 = new CUI(Handler);
														using CUI.Handler.UpdatePool updatePool3 = cui3.UpdatePool();
														switch (chartCache.Status)
														{
														case Tab.OptionChart.ChartCache.StatusTypes.Finalized:
															if (!chartCache.HasPlayerReceivedData(EncryptedValue<ulong>.op_Implicit(ap.Player.userID)))
															{
																((BaseEntity)CommunityEntity.ServerInstance).ClientRPC(RpcTarget.Player("CL_ReceiveFilePng", ap.Player), chartCache.Crc, (uint)chartCache.Data.Length, (ReadOnlySpan<byte>)chartCache.Data, 0u, (byte)0);
															}
															updatePool3.Add(cui3.UpdatePanel($"{oldIdentifier2}_loading", "0 0 0 0", null, 0f, 0f));
															updatePool3.Add(cui3.UpdateText($"{oldIdentifier2}_loadingtxt", "0 0 0 0", string.Empty, 0, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1));
															updatePool3.Add(cui3.UpdateImage($"{oldIdentifier2}_chart", chartCache.Crc, Cache.CUI.WhiteColor));
															updatePool3.Send(ap.Player);
															break;
														default:
															updatePool3.Add(cui3.UpdateText($"{oldIdentifier2}_loadingtxt", "0.9 0.1 0.1 0.75", "Failed to load chart!", 10, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1));
															updatePool3.Send(ap.Player);
															break;
														}
													});
													return false;
												}
												case "layershadow":
												{
													object oldIdentifier = args[2];
													string empty = string.Empty;
													using CUI cui = new CUI(Handler);
													using CUI.Handler.UpdatePool updatePool = cui.UpdatePool();
													bool flag = false;
													Color blanchedAlmond = Color.BlanchedAlmond;
													if (num == -1)
													{
														bool flag2 = optionChart.Chart.Layers.All((Chart.Layer x) => x.LayerSettings.Shadows == 0);
														for (int num2 = 0; num2 < optionChart.Chart.Layers.Length; num2++)
														{
															Chart.Layer layer = optionChart.Chart.Layers[num2];
															if (flag2)
															{
																layer.LayerSettings.Shadows = 1;
															}
															else
															{
																layer.LayerSettings.Shadows = 0;
															}
														}
														empty = optionChart.GetIdentifier(reset: true);
														return true;
													}
													Chart.Layer layer2 = optionChart.Chart.Layers[num];
													layer2.LayerSettings.Shadows = ((layer2.LayerSettings.Shadows != 1) ? 1 : 0);
													empty = optionChart.GetIdentifier(reset: true);
													blanchedAlmond = layer2.LayerSettings.Color;
													flag = layer2.LayerSettings.Shadows > 0;
													Color color = Color.FromArgb((int)((float)(int)blanchedAlmond.R * 1.5f).Clamp(0f, 255f), (int)((float)(int)blanchedAlmond.G * 1.5f).Clamp(0f, 255f), (int)((float)(int)blanchedAlmond.B * 1.5f).Clamp(0f, 255f));
													string textColor = $"{(float)(int)color.R / 255f} {(float)(int)color.G / 255f} {(float)(int)color.B / 255f} 1";
													string text = args.Select((object x) => x as string).Skip(3).ToString(" ");
													updatePool.Add(cui.UpdatePanel($"{oldIdentifier}_loading", "0 0 0 0.2", null, 0.01f, 0.99f, 0.01f, 0.99f, 0f, 0f, 0f, 0f, blur: true));
													updatePool.Add(cui.UpdateText($"{oldIdentifier}_loadingtxt", "1 1 1 0.5", "Please wait...", 10, 0.01f, 0.99f, 0.01f, 0.99f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1));
													updatePool.Add(cui.UpdateImage($"{oldIdentifier}_chart", 0u, Cache.CUI.WhiteColor, null, 0.01f));
													updatePool.Add(cui.UpdateProtectedButton($"{oldIdentifier}_layerbtn2_{num}", string.Format("{0} {1} {2} {3}", new object[4]
													{
														(float)(int)blanchedAlmond.R / 255f,
														(float)(int)blanchedAlmond.G / 255f,
														(float)(int)blanchedAlmond.B / 255f,
														(!flag) ? 0.15 : 0.5
													}), textColor, "⦿", 8, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, string.Format("{0} {1} {2} {3}", new object[4] { text, num, oldIdentifier, text }), (TextAnchor)4));
													updatePool.Send(ap.Player);
													Tab.OptionChart.Cache.GetOrProcessCache(empty, optionChart.Chart, delegate(Tab.OptionChart.ChartCache chartCache)
													{
														//IL_0047: Unknown result type (might be due to invalid IL or missing references)
														//IL_0072: Unknown result type (might be due to invalid IL or missing references)
														using CUI cui3 = new CUI(Handler);
														using CUI.Handler.UpdatePool updatePool3 = cui3.UpdatePool();
														switch (chartCache.Status)
														{
														case Tab.OptionChart.ChartCache.StatusTypes.Finalized:
															if (!chartCache.HasPlayerReceivedData(EncryptedValue<ulong>.op_Implicit(ap.Player.userID)))
															{
																((BaseEntity)CommunityEntity.ServerInstance).ClientRPC(RpcTarget.Player("CL_ReceiveFilePng", ap.Player), chartCache.Crc, (uint)chartCache.Data.Length, (ReadOnlySpan<byte>)chartCache.Data, 0u, (byte)0);
															}
															updatePool3.Add(cui3.UpdatePanel($"{oldIdentifier}_loading", "0 0 0 0", null, 0f, 0f));
															updatePool3.Add(cui3.UpdateText($"{oldIdentifier}_loadingtxt", "0 0 0 0", string.Empty, 0, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1));
															updatePool3.Add(cui3.UpdateImage($"{oldIdentifier}_chart", chartCache.Crc, Cache.CUI.WhiteColor));
															updatePool3.Send(ap.Player);
															break;
														default:
															updatePool3.Add(cui3.UpdateText($"{oldIdentifier}_loadingtxt", "0.9 0.1 0.1 0.75", "Failed to load chart!", 10, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1));
															updatePool3.Send(ap.Player);
															break;
														}
													});
													return false;
												}
												}
											}
										}
										else if (optionColor.Callback != null)
										{
											ColorPicker.Open(player, delegate(string rustColor, string hexColor, float alpha)
											{
												optionColor.Callback?.Invoke(ap, rustColor, hexColor, alpha);
											});
											return false;
										}
									}
									else
									{
										object obj2 = args[0];
										switch (obj2 as string)
										{
										case "input":
											if (ap.Input != optionInputButton)
											{
												ap.Input = optionInputButton;
												return true;
											}
											ap.Input = (ap.PreviousInput = null);
											if (!optionInputButton.Input.ReadOnly)
											{
												object[] array = Array.Empty<object>();
												if (args.Length - 1 > 0)
												{
													array = HookCaller.Caller.AllocateBuffer(args.Length - 1);
													for (int num4 = 1; num4 < args.Length; num4++)
													{
														array[num4 - 1] = args[num4];
													}
												}
												optionInputButton.Input.Callback?.Invoke(ap, array);
												if (array.Length != 0)
												{
													HookCaller.Caller.ReturnBuffer(array);
												}
											}
											return optionInputButton.Input.Callback != null;
										case "button":
											optionInputButton.Button.Callback?.Invoke(ap);
											return optionInputButton.Button.Callback != null;
										}
									}
									return false;
								}
								Action<PlayerSession> callback = optionButtonArray.Buttons[((string)args[0]).ToInt()].Callback;
								callback?.Invoke(ap);
								return callback != null;
							}
							optionRange.Callback?.Invoke(ap, ((string)args[0]).ToFloat().Scale(0f, optionRange.Max.Clamp(optionRange.Min, 50f) - 1f, optionRange.Min, optionRange.Max));
							return optionRange.Callback != null;
						}
						PlayerSession.Page selectedDropdownPage = ap._selectedDropdownPage;
						if (((string)args[0]).ToBool())
						{
							object obj3 = args[1];
							if (obj3 is string text4 && text4 == "call")
							{
								ap._selectedDropdown = null;
								optionDropdown.Callback?.Invoke(ap, ((string)args[2]).ToInt());
								selectedDropdownPage.CurrentPage = 0;
							}
							else
							{
								object obj4 = args[1];
								switch (obj4 as string)
								{
								case "--":
									selectedDropdownPage.CurrentPage = 0;
									break;
								case "++":
									selectedDropdownPage.CurrentPage = selectedDropdownPage.TotalPages;
									break;
								default:
									selectedDropdownPage.CurrentPage += ((string)args[1]).ToInt();
									break;
								}
								if (selectedDropdownPage.CurrentPage < 0)
								{
									selectedDropdownPage.CurrentPage = selectedDropdownPage.TotalPages;
								}
								else if (selectedDropdownPage.CurrentPage > selectedDropdownPage.TotalPages)
								{
									selectedDropdownPage.CurrentPage = 0;
								}
							}
							return true;
						}
						selectedDropdownPage.CurrentPage = 0;
						Tab.OptionDropdown selectedDropdown = ap._selectedDropdown;
						if (selectedDropdown == optionDropdown)
						{
							ap._selectedDropdown = null;
							return true;
						}
						ap._selectedDropdown = optionDropdown;
						return selectedDropdown != optionDropdown;
					}
					optionToggle.Callback?.Invoke(ap);
					return optionToggle.Callback != null;
				}
				optionEnum.Callback?.Invoke(ap, ((string)args[0]).ToBool());
				return optionEnum.Callback != null;
			}
			if (ap.Input != optionInput)
			{
				ap.Input = optionInput;
				return true;
			}
			if (!optionInput.ReadOnly)
			{
				optionInput.Callback?.Invoke(ap, args);
			}
			ap.Input = (ap.PreviousInput = null);
			return optionInput.Callback != null;
		}
		optionButton.Callback?.Invoke(ap);
		return optionButton.Callback != null;
	}

	[Conditional("!MINIMAL")]
	private void OnPluginLoaded(RustPlugin plugin)
	{
		PluginsTab.GetVendor(PluginsTab.VendorTypes.Codefling)?.Refresh();
		PluginsTab.GetVendor(PluginsTab.VendorTypes.uMod)?.Refresh();
		for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
		{
			BasePlayer player = BasePlayer.activePlayerList[i];
			PlayerSession playerSession = Singleton.GetPlayerSession(player);
			if (playerSession.IsInMenu && Singleton.GetTab(player).Id == "plugins")
			{
				Singleton.Draw(player);
			}
		}
	}

	[Conditional("!MINIMAL")]
	private void OnPluginUnloaded(RustPlugin plugin)
	{
		Community.Runtime.Core.NextTick(delegate
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					BasePlayer current = enumerator.Current;
					PlayerSession playerSession = Singleton.GetPlayerSession(current);
					if (playerSession.IsInMenu && Singleton.GetTab(current).Id == "pluginbrowser")
					{
						Singleton.Draw(current);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		});
	}

	public static void BlindPlayer(BasePlayer player, BasePlayer target)
	{
		using CUI cUI = new CUI(Singleton.Handler);
		LUI.LuiContainer container = cUI.v2.CreateParent(CUI.ClientPanels.Overlay, LuiPosition.Full, "blindingpanel").AddCursor().AddKeyboard()
			.SetDestroy("blindingpanel");
		cUI.v2.CreateImageFromDb(container, LuiPosition.Full, LuiOffset.None, "bsod", "0 0 0 1");
		PlayersTab.BlindedPlayers.Add(target);
		cUI.v2.SendUi(target);
		HookCaller.CallStaticHook(3658185574u, player, target);
	}

	public static void UnblindPlayer(BasePlayer player, BasePlayer target)
	{
		if (!PlayersTab.BlindedPlayers.Remove(target))
		{
			return;
		}
		using CUI cUI = new CUI(Singleton.Handler);
		cUI.Destroy("blindingpanel", target);
		HookCaller.CallStaticHook(3911772319u, player, target);
	}

	public static void EmpowerPlayerStats(BasePlayer player, BasePlayer target)
	{
		Debugging.RefillPlayerVitals(target, true);
		HookCaller.CallStaticHook(837777771u, player, target);
	}

	public static void LockPlayerContainer(BasePlayer player, BasePlayer target, ItemContainer container, bool wants)
	{
		container.SetLocked(wants, false);
		HookCaller.CallStaticHook(298795244u, player, target, container, wants);
	}

	public static void PrivateMessagePlayer(BasePlayer player, BasePlayer target, string message)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(message))
		{
			target.ChatMessage("[" + player.displayName + "]: " + message);
			if (Singleton.ConfigInstance.PlayPMSound)
			{
				server.Run(Singleton.ConfigInstance.PMSound, (BaseEntity)(object)target, 2u, Vector3.zero, new Vector3(0f, 2f, 0f), (Connection)null, false, (List<Connection>)null, 0, (Type)0);
			}
			HookCaller.CallStaticHook(468227819u, player, target, message);
		}
	}

	public static void MutePlayer(BasePlayer player, BasePlayer target, bool wants, string reason = "No reason given")
	{
		target.State.chatMuted = wants;
		target.SetPlayerFlag((PlayerFlags)4096, wants);
		target.DirtyPlayerState();
		target.ChatMessage("You have been " + (wants ? "muted" : "unmuted") + " by an admin. Reason: " + reason);
		HookCaller.CallStaticHook(2716457321u, player, target, wants, reason);
	}

	public static void BanPlayer(BasePlayer player, BasePlayer target, string reason = "No reason given", string duration = "")
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		TimeSpan timeSpan = TimeSpan.Zero;
		if (!string.IsNullOrEmpty(duration))
		{
			timeSpan = BanStringToSeconds(duration);
		}
		long num = -1L;
		if (timeSpan != TimeSpan.Zero)
		{
			num = new DateTimeOffset(DateTime.UtcNow.Add(timeSpan)).ToUnixTimeSeconds();
		}
		ServerUsers.Set(EncryptedValue<ulong>.op_Implicit(target.userID), (UserGroup)3, target.displayName, reason, num);
		ServerUsers.Save();
		HookCaller.CallStaticHook(338697635u, player, target, reason, timeSpan);
		KickPlayer(player, target, reason);
	}

	public static void UnbanPlayer(BasePlayer player, BasePlayer target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		ServerUsers.Remove(EncryptedValue<ulong>.op_Implicit(target.userID));
		ServerUsers.Save();
		HookCaller.CallStaticHook(1355418005u, player, target);
	}

	public static void KickPlayer(BasePlayer player, BasePlayer target, string reason = "No reason given")
	{
		target.Kick(reason, false);
		HookCaller.CallStaticHook(3987303153u, player, target, reason);
	}

	private static TimeSpan BanStringToSeconds(string duration)
	{
		TimeSpan result = TimeSpan.Zero;
		int result3;
		int result4;
		int result5;
		int result6;
		int result7;
		if (duration.EndsWith("Y") && int.TryParse(duration.TrimEnd('Y'), out var result2))
		{
			result = TimeSpan.FromDays((double)(result2 * 365));
		}
		else if (duration.EndsWith("M") && int.TryParse(duration.TrimEnd('M'), out result3))
		{
			result = TimeSpan.FromDays((double)(result3 * 30));
		}
		else if (duration.EndsWith("d") && int.TryParse(duration.TrimEnd('d'), out result4))
		{
			result = TimeSpan.FromDays((double)result4);
		}
		else if (duration.EndsWith("h") && int.TryParse(duration.TrimEnd('h'), out result5))
		{
			result = TimeSpan.FromHours((double)result5);
		}
		else if (duration.EndsWith("m") && int.TryParse(duration.TrimEnd('m'), out result6))
		{
			result = TimeSpan.FromMinutes((double)result6);
		}
		else if (duration.EndsWith("s") && int.TryParse(duration.TrimEnd('s'), out result7))
		{
			result = TimeSpan.FromSeconds((double)result7);
		}
		return result;
	}

	public static void OpenPlayerContainer(PlayerSession ap, BasePlayer player, Tab tab)
	{
		Singleton.Subscribe("OnEntityVisibilityCheck");
		Singleton.Subscribe("OnEntityDistanceCheck");
		Singleton.Subscribe("CanAcceptItem");
		EntitiesTab.LastContainerLooter = ap;
		ap.SetStorage<BasePlayer>(tab, "lootedent", player);
		EntitiesTab.SendEntityToPlayer(ap.Player, (BaseEntity)(object)player);
		Core.timer.In(0.2f, delegate
		{
			Singleton.Close(ap.Player);
		});
		Core.timer.In(0.5f, delegate
		{
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			EntitiesTab.SendEntityToPlayer(ap.Player, (BaseEntity)(object)player);
			ap.Player.inventory.loot.Clear();
			ap.Player.inventory.loot.PositionChecks = false;
			ap.Player.inventory.loot.entitySource = (BaseEntity)(object)RelationshipManager.ServerInstance;
			ap.Player.inventory.loot.itemSource = null;
			ap.Player.inventory.loot.AddContainer(player.inventory.containerMain);
			ap.Player.inventory.loot.AddContainer(player.inventory.containerWear);
			ap.Player.inventory.loot.AddContainer(player.inventory.containerBelt);
			ap.Player.inventory.loot.MarkDirty();
			ap.Player.inventory.loot.SendImmediate();
			((BaseEntity)ap.Player).ClientRPC(RpcTarget.Player("RPC_OpenLootPanel", ap.Player), "player_corpse");
		});
	}

	public static void OpenContainer(PlayerSession ap, ItemContainer container, Tab tab)
	{
		EntitiesTab.LastContainerLooter = null;
		ap.ClearStorage(tab, "lootedent");
		ap.Player.inventory.loot.Clear();
		Core.timer.In(0.5f, delegate
		{
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			EntitiesTab.LastContainerLooter = ap;
			ap.SetStorage<BasePlayer>(tab, "lootedent", ap.Player);
			ap.Player.inventory.loot.PositionChecks = false;
			ap.Player.inventory.loot.entitySource = (BaseEntity)(object)RelationshipManager.ServerInstance;
			ap.Player.inventory.loot.itemSource = null;
			ap.Player.inventory.loot.AddContainer(container);
			ap.Player.inventory.loot.MarkDirty();
			ap.Player.inventory.loot.SendImmediate();
			((BaseEntity)ap.Player).ClientRPC(RpcTarget.Player("RPC_OpenLootPanel", ap.Player), "generic");
		});
	}

	[Conditional("!MINIMAL")]
	private void OnEntityDismounted(BaseMountable entity, BasePlayer player)
	{
		PlayerSession playerSession = GetPlayerSession(player);
		Tab tab = GetTab(player);
		if (playerSession.GetStorage(tab, "wasviewingcam", @default: false))
		{
			((BaseNetworkable)entity).Kill((DestroyMode)0, true);
			Draw(player);
			Unsubscribe("OnEntityDismounted");
		}
	}

	[Conditional("!MINIMAL")]
	private void OnPlayerLootEnd(PlayerLoot loot)
	{
		if (EntitiesTab.LastContainerLooter != null && (Object)(object)((EntityComponent<BasePlayer>)(object)loot).baseEntity == (Object)(object)EntitiesTab.LastContainerLooter.Player)
		{
			Draw(EntitiesTab.LastContainerLooter.Player);
			EntitiesTab.LastContainerLooter = null;
			Unsubscribe("OnEntityVisibilityCheck");
			Unsubscribe("OnEntityDistanceCheck");
			Unsubscribe("CanAcceptItem");
		}
	}

	[Conditional("!MINIMAL")]
	private object OnEntityDistanceCheck(BaseEntity ent, BasePlayer player, uint id, string debugName, float maximumDistance)
	{
		PlayerSession playerSession = GetPlayerSession(player);
		Tab tab = GetTab(player);
		BaseEntity storage = playerSession.GetStorage<BaseEntity>(tab, "lootedent");
		if ((Object)(object)storage == (Object)null)
		{
			return null;
		}
		return true;
	}

	[Conditional("!MINIMAL")]
	private object OnEntityVisibilityCheck(BaseEntity ent, BasePlayer player, uint id, string debugName, float maximumDistance)
	{
		PlayerSession playerSession = GetPlayerSession(player);
		Tab tab = GetTab(player);
		BaseEntity storage = playerSession.GetStorage<BaseEntity>(tab, "lootedent");
		if ((Object)(object)storage == (Object)null)
		{
			return null;
		}
		return true;
	}

	[Conditional("!MINIMAL")]
	private void OnPlayerDisconnected(BasePlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Tab.OptionChart.Cache.ClearPlayerViewer(EncryptedValue<ulong>.op_Implicit(player.userID));
		if (PlayersTab.BlindedPlayers.Contains(player))
		{
			PlayersTab.BlindedPlayers.Remove(player);
		}
	}

	[Conditional("!MINIMAL")]
	private object CanAcceptItem(ItemContainer container, Item item, int targetPos)
	{
		BasePlayer playerOwner = container.playerOwner;
		if ((Object)(object)playerOwner == (Object)null || container != playerOwner.inventory.containerBelt || !Oxide.Core.ExtensionMethods.Contains(_backpacks, item.info.itemid))
		{
			return null;
		}
		if (!Singleton.HasAccess(playerOwner, "entities.loot_players"))
		{
			return null;
		}
		OpenContainer(GetPlayerSession(playerOwner), item.contents, null);
		return (object)(CanAcceptResult)1;
	}

	[Conditional("!MINIMAL")]
	private object IValidDismountPosition(BaseMountable mountable, BasePlayer player)
	{
		return null;
	}

	[Conditional("!MINIMAL")]
	private object IModBackpack(BaseMountable mountable, BasePlayer player)
	{
		return null;
	}

	public static bool AcceptOnBackpack(Item backpack)
	{
		if (EntitiesTab.LastContainerLooter != null)
		{
			BasePlayer player = EntitiesTab.LastContainerLooter.Player;
			return ((player == null) ? null : player.inventory?.loot?.containers[0]) == backpack.contents;
		}
		return false;
	}

	public PlayerSession GetPlayerSession(BasePlayer player)
	{
		if (PlayerSessions.TryGetValue(player, out var value))
		{
			return value;
		}
		value = new PlayerSession(player);
		PlayerSessions.Add(player, value);
		return value;
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("adminmodule.itemsetting")]
	private void ItemSetting(Arg arg)
	{
		if (arg.Args != null)
		{
			BasePlayer player = ArgEx.Player(arg);
			PlayerSession playerSession = GetPlayerSession(player);
			string text = arg.GetString(0, "");
			string fullString = arg.GetFullString(1);
			switch (text)
			{
			case "customname":
				playerSession.SetStorage(null, "itemscustomname", fullString);
				break;
			case "amount":
			{
				playerSession.SetStorage(null, "itemsamount", ((!int.TryParse(fullString, out var result)) ? 1 : result).Clamp(1, int.MaxValue));
				break;
			}
			case "skin":
				playerSession.SetStorage(null, "itemsskin", fullString.ToUlong(0uL));
				break;
			case "text":
				playerSession.SetStorage(null, "itemstext", fullString);
				break;
			case "blueprint":
				playerSession.SetStorage(null, "itemsblueprint", !playerSession.GetStorage(null, "itemsblueprint", @default: false));
				break;
			}
			Draw(player);
		}
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("adminmodule.itemcreate")]
	private void ItemCreate(Arg arg)
	{
		BasePlayer val = ArgEx.Player(arg);
		PlayerSession playerSession = GetPlayerSession(val);
		ItemDefinition storage = playerSession.GetStorage<ItemDefinition>(null, "itemtabitem");
		if (!((Object)(object)storage == (Object)null))
		{
			bool storage2 = playerSession.GetStorage(null, "itemsblueprint", @default: false);
			Item val2 = ItemManager.CreateByName(storage2 ? "blueprintbase" : storage.shortname, playerSession.GetStorage(null, "itemsamount", 1), playerSession.GetStorage(null, "itemsskin", 0uL));
			val2.name = playerSession.GetStorage(null, "itemscustomname", string.Empty);
			val2.skin = playerSession.GetStorage(null, "itemsskin", 0uL);
			val2.text = playerSession.GetStorage(null, "itemstext", string.Empty);
			if (storage2)
			{
				val2.blueprintTarget = storage.itemid;
			}
			((BaseEntity)val).GiveItem(val2, (GiveItemReason)0, (GiveItemOptions)0);
			Puts(string.Format(" {0} created {1}[{2}] x {3}{4}", new object[5]
			{
				val.Connection,
				storage.displayName,
				storage.shortname,
				val2.amount,
				storage2 ? "[bp]" : string.Empty
			}));
			playerSession.ClearStorage(null, "itemtabitem");
			Draw(playerSession.Player);
		}
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("adminmodule.itemclear")]
	private void ItemClear(Arg arg)
	{
		BasePlayer player = ArgEx.Player(arg);
		PlayerSession playerSession = GetPlayerSession(player);
		playerSession.ClearStorage(null, "itemtabitem");
		Draw(playerSession.Player);
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("greet.continue")]
	private void ChangePage(Arg arg)
	{
		PlayerSession ap = GetPlayerSession(ArgEx.Player(arg));
		Tab tab = GetTab(ap.Player);
		Analytics.admin_module_greet_continue();
		ap.SetStorage(tab, "page", 0);
		Singleton.DataInstance.GreetDisplayed = true;
		Singleton.GenerateTabs();
		Community.Runtime.Core.NextTick(delegate
		{
			Save();
			Singleton.SetTab(ap.Player, "carbon");
			Draw(ap.Player);
		});
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("pluginbrowser.changetab")]
	private void PluginBrowserChange(Arg args)
	{
		PlayerSession playerSession = Singleton.GetPlayerSession(ArgEx.Player(args));
		BasePlayer player = playerSession.Player;
		Tab tab = Singleton.GetTab(playerSession.Player);
		string text = args.GetString(0, "");
		if (text == "LIST")
		{
			playerSession.SetStorage(tab, "listview", value: true);
			tab.OnChange(playerSession, tab);
			Singleton.Draw(player);
			return;
		}
		PluginsTab.Vendor vendor = PluginsTab.GetVendor(playerSession.SetStorage(tab, "vendor", (PluginsTab.VendorTypes)Enum.Parse(typeof(PluginsTab.VendorTypes), text)));
		vendor.Refresh();
		PluginsTab.TagFilter.Clear();
		PluginsTab.DropdownShow = false;
		PluginsTab.DownloadThumbnails(vendor, tab, Singleton.GetPlayerSession(ArgEx.Player(args)));
		Singleton.Draw(ArgEx.Player(args));
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("pluginbrowser.interact")]
	private void PluginBrowserInteract(Arg args)
	{
		//IL_0613: Unknown result type (might be due to invalid IL or missing references)
		//IL_0645: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = ArgEx.Player(args);
		if (!Singleton.HasAccess(player, "plugins.setup"))
		{
			return;
		}
		PlayerSession ap = Singleton.GetPlayerSession(player);
		Tab tab = Singleton.GetTab(ap.Player);
		PluginsTab.VendorTypes storage = ap.GetStorage(tab, "vendor", PluginsTab.VendorTypes.Installed);
		PluginsTab.Vendor vendor = PluginsTab.GetVendor(storage);
		string pluginName = args.GetFullString(1).Replace("\"", string.Empty).Trim();
		PluginsTab.Plugin tabPlugin = ap.GetStorage<PluginsTab.Plugin>(tab, "plugin") ?? vendor.FetchedPlugins.FirstOrDefault((PluginsTab.Plugin x) => Path.GetFileNameWithoutExtension(x.File).Equals(pluginName));
		PluginsTab.Plugin mainTabPlugin = tabPlugin;
		if (tabPlugin.PreferredVendorPlugin != null)
		{
			tabPlugin = tabPlugin.PreferredVendorPlugin;
		}
		RustPlugin plugin = tabPlugin.ExistentPlugin;
		string[] arg = new string[args.Args.Length];
		for (int num = 0; num < args.Args.Length; num++)
		{
			arg[num] = ((object)Unsafe.As<StringView, StringView>(ref args.Args[num])/*cast due to constrained. prefix*/).ToString();
		}
		switch (arg[0])
		{
		case "0":
			tabPlugin.GetPreferredVendor().Download(pluginName, delegate
			{
				Singleton.Draw(ArgEx.Player(args));
			});
			Array.Clear(arg, 0, arg.Length);
			break;
		case "1":
			tab.CreateDialog("Are you sure you want to update '" + ap.GetStorage<PluginsTab.Plugin>(tab, "selectedplugin").Name + "'?", delegate
			{
				tabPlugin.GetPreferredVendor().Download(pluginName, delegate
				{
					Singleton.Draw(ArgEx.Player(args));
				});
				Array.Clear(arg, 0, arg.Length);
			});
			break;
		case "2":
			tab.CreateDialog("Are you sure you want to uninstall '" + ap.GetStorage<PluginsTab.Plugin>(tab, "selectedplugin").Name + "'?", delegate
			{
				Singleton.Puts("Uninstalling " + pluginName + " on " + vendor?.GetType().Name);
				tabPlugin.GetPreferredVendor().Uninstall(pluginName);
				Array.Clear(arg, 0, arg.Length);
			});
			break;
		case "3":
		{
			if (plugin == null)
			{
				plugin = vendor.FetchedPlugins.FirstOrDefault((PluginsTab.Plugin x) => x.Id.Equals(pluginName))?.ExistentPlugin;
			}
			string path = Path.Combine(Defines.GetConfigsFolder(), plugin.Config.Filename);
			if (OsEx.File.Exists(path))
			{
				Singleton.SetTab(ap.Player, ConfigEditor.Make(OsEx.File.ReadText(path), delegate(PlayerSession playerSession, JObject jobject)
				{
					Community.Runtime.Core.NextTick(delegate
					{
						Singleton.SetTab(playerSession.Player, "plugins", onChange: false);
					});
				}, delegate(PlayerSession playerSession, JObject jobject)
				{
					OsEx.File.Create(path, ((JToken)jobject).ToString((Formatting)1, Array.Empty<JsonConverter>()));
					Community.Runtime.Core.NextTick(delegate
					{
						Singleton.SetTab(playerSession.Player, "plugins", onChange: false);
					});
				}, delegate(PlayerSession playerSession, JObject jobject)
				{
					OsEx.File.Create(path, ((JToken)jobject).ToString((Formatting)1, Array.Empty<JsonConverter>()));
					plugin.ProcessorProcess.MarkDirty();
					Community.Runtime.Core.NextTick(delegate
					{
						Singleton.SetTab(playerSession.Player, "plugins", onChange: false);
					});
				}));
			}
			else
			{
				args.ReplyWith("Config file not found at '" + path + "'");
			}
			Array.Clear(arg, 0, arg.Length);
			break;
		}
		case "4":
			if (plugin == null)
			{
				plugin = vendor.FetchedPlugins.FirstOrDefault((PluginsTab.Plugin x) => x.Id.Equals(pluginName)).ExistentPlugin;
			}
			if (plugin != null)
			{
				plugin.ProcessorProcess.MarkDirty();
				Community.Runtime.Core.NextTick(delegate
				{
					Singleton.SetTab(ap.Player, "plugins", onChange: false);
				});
			}
			break;
		case "5":
			if (plugin == null)
			{
				plugin = vendor.FetchedPlugins.FirstOrDefault((PluginsTab.Plugin x) => x.Id.Equals(pluginName)).ExistentPlugin;
			}
			Singleton.SetTab(ap.Player, LangEditor.Make(plugin, delegate(PlayerSession playerSession)
			{
				Community.Runtime.Core.NextTick(delegate
				{
					Singleton.SetTab(playerSession.Player, "plugins", onChange: false);
				});
			}));
			break;
		case "10":
			if (PluginsTab.ServerOwner.Singleton.FavouritePlugins.Contains(pluginName))
			{
				PluginsTab.ServerOwner.Singleton.FavouritePlugins.Remove(pluginName);
				Logger.Log(" [" + vendor.Type + "] Unfavorited plugin '" + pluginName + "'");
			}
			else
			{
				PluginsTab.ServerOwner.Singleton.FavouritePlugins.Add(pluginName);
				Logger.Log(" [" + vendor.Type + "] Favorited plugin '" + pluginName + "'");
			}
			Array.Clear(arg, 0, arg.Length);
			break;
		case "11":
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(tabPlugin.File);
			bool flag = !string.IsNullOrEmpty(CorePlugin.GetPluginFile(fileNameWithoutExtension).Path);
			if (tabPlugin.IsInstalled())
			{
				ConsoleSystem.Run(Option.Server, "c.unload \"" + fileNameWithoutExtension + "\"", Array.Empty<object>());
				Singleton.Draw(player);
			}
			else if (flag)
			{
				ConsoleSystem.Run(Option.Server, "c.load \"" + fileNameWithoutExtension + "\"", Array.Empty<object>());
				Singleton.Draw(player);
			}
			break;
		}
		case "12":
			mainTabPlugin.SetPreferredVendor(mainTabPlugin.AvailableOn.FirstOrDefault((PluginsTab.Plugin x) => x.PreferredVendor != mainTabPlugin.PreferredVendor).PreferredVendor);
			Singleton.Draw(player);
			break;
		}
		Singleton.Draw(ArgEx.Player(args));
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("pluginbrowser.page")]
	private void PluginBrowserPage(Arg arg)
	{
		BasePlayer player = ArgEx.Player(arg);
		PlayerSession playerSession = Singleton.GetPlayerSession(player);
		PlayerSession.Page orCreatePage = playerSession.GetOrCreatePage(230);
		int num = arg.GetInt(0, 0);
		int currentPage = orCreatePage.CurrentPage;
		switch (num)
		{
		case 0:
			orCreatePage.CurrentPage = 0;
			break;
		case 1:
			orCreatePage.CurrentPage++;
			if (orCreatePage.CurrentPage > orCreatePage.TotalPages - 1)
			{
				orCreatePage.CurrentPage = 0;
			}
			break;
		case -1:
			orCreatePage.CurrentPage--;
			if (orCreatePage.CurrentPage < 0)
			{
				orCreatePage.CurrentPage = orCreatePage.TotalPages - 1;
			}
			break;
		case -2:
			orCreatePage.CurrentPage = 0;
			break;
		case -3:
			orCreatePage.CurrentPage = orCreatePage.TotalPages - 1;
			break;
		default:
			orCreatePage.CurrentPage = num - 1;
			break;
		}
		if (orCreatePage.CurrentPage <= 0)
		{
			orCreatePage.CurrentPage = 0;
		}
		else if (orCreatePage.CurrentPage > orCreatePage.TotalPages)
		{
			orCreatePage.CurrentPage = orCreatePage.TotalPages - 1;
		}
		playerSession.SetStorage(playerSession.SelectedTab, "page", orCreatePage.CurrentPage);
		PluginsTab.DropdownShow = false;
		if (currentPage != orCreatePage.CurrentPage)
		{
			Singleton.Draw(player);
		}
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("pluginbrowser.search")]
	private unsafe void PluginBrowserSearch(Arg args)
	{
		PlayerSession playerSession = Singleton.GetPlayerSession(ArgEx.Player(args));
		Tab tab = Singleton.GetTab(playerSession.Player);
		PluginsTab.Vendor vendor = PluginsTab.GetVendor(playerSession.GetStorage(tab, "vendor", PluginsTab.VendorTypes.Installed));
		vendor.Refresh();
		string text = playerSession.SetStorage(tab, "search", args.Args?.Select((StringView x) => ((object)(*(StringView*)(&x))/*cast due to constrained. prefix*/).ToString()).ToString(" "));
		playerSession.SetStorage(tab, "page", 0);
		if (text == "Search...")
		{
			playerSession.SetStorage(tab, "search", string.Empty);
		}
		PluginsTab.DownloadThumbnails(vendor, tab, Singleton.GetPlayerSession(ArgEx.Player(args)));
		Singleton.Draw(ArgEx.Player(args));
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("pluginbrowser.refreshvendor")]
	private void PluginBrowserRefreshVendor(Arg args)
	{
		PlayerSession playerSession = Singleton.GetPlayerSession(ArgEx.Player(args));
		Tab tab = Singleton.GetTab(playerSession.Player);
		PluginsTab.Vendor vendor = PluginsTab.GetVendor(playerSession.GetStorage(tab, "vendor", PluginsTab.VendorTypes.Installed));
		if (vendor is PluginsTab.Installed)
		{
			return;
		}
		tab.CreateDialog("Are you sure you want to fetch the " + vendor.Type + " plugin list?", delegate(PlayerSession ap)
		{
			string text = string.Empty;
			if (!(vendor is PluginsTab.Codefling))
			{
				if (vendor is PluginsTab.uMod)
				{
					text = "umod";
				}
			}
			else
			{
				text = "cf";
			}
			string file = Path.Combine(Defines.GetDataFolder(), "vendordata_" + text + ".db");
			OsEx.File.Delete(file);
			if (vendor is PluginsTab.IVendorStored vendorStored && !vendorStored.Load())
			{
				vendor.FetchList(delegate(PluginsTab.Vendor vendor2)
				{
					vendor2.Refresh();
				});
				vendor.Refresh();
			}
			if (vendor is PluginsTab.IVendorAuthenticated vendorAuthenticated)
			{
				vendorAuthenticated.RefreshUser(ap);
			}
			Singleton.Draw(ap.Player);
		});
		Singleton.Draw(playerSession.Player);
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("pluginbrowser.selectplugin")]
	private void PluginBrowserSelectPlugin(Arg arg)
	{
		PlayerSession playerSession = Singleton.GetPlayerSession(ArgEx.Player(arg));
		Tab selectedTab = playerSession.SelectedTab;
		PluginsTab.Vendor vendor = PluginsTab.GetVendor(playerSession.GetStorage(selectedTab, "vendor", PluginsTab.VendorTypes.Installed));
		string pluginName = arg.GetString(0, "").Replace("\"", string.Empty);
		PluginsTab.Plugin plugin = vendor.FetchedPlugins.FirstOrDefault((PluginsTab.Plugin x) => Path.GetFileNameWithoutExtension(x.File).Equals(pluginName, StringComparison.CurrentCultureIgnoreCase));
		try
		{
			if (plugin.PreferredVendorPlugin != null)
			{
				plugin = plugin.PreferredVendorPlugin;
			}
		}
		catch (Exception ex)
		{
			Logger.Error("Failed " + pluginName, ex);
			return;
		}
		playerSession.SetStorage(selectedTab, "selectedplugin", plugin);
		using CUI cui = new CUI(Singleton.Handler);
		using CUI.Handler.UpdatePool updatePool = cui.UpdatePool();
		if (!DateTime.TryParse(plugin.Date, out var result))
		{
			result = DateTime.Now;
		}
		updatePool.Add(cui.UpdatePanel("selectedpluginpnl", "0.1 0.1 0.1 0.5", null, 0.0001f, 1f, 0.0001f));
		updatePool.Add(cui.UpdateClientImage("selectedpluginicn", plugin.Image, "1 1 1 0.85", null, 0f, 1f, 1f, 1f, 0f, 0f, -1000f, 0f, 1f));
		updatePool.Add(cui.UpdateText("selectedpluginname", Cache.CUI.WhiteColor, plugin.Name, 35, 0.05f, 1f, 1f, 1f, 0f, 0f, -400f, 0f, (TextAnchor)6, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1, 1f));
		updatePool.Add(cui.UpdateText("selectedpluginprice", plugin.IsPaid() ? "#e0e344" : "#44e3db", plugin.IsPaid() ? $"${plugin.OriginalPrice.ToFloat():0.00}" : "FREE", 25, 0f, 0.925f, 1f, 1f, 0f, 0f, -400f, 0f, (TextAnchor)8, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1, 1f));
		updatePool.Add(cui.UpdateText("selectedpluginchlog", "0.8 0.8 0.8 0.6", plugin.Changelog, 13, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1, 1f));
		updatePool.Add(cui.UpdateText("selectedplugindesc", "0.8 0.8 0.8 0.6", plugin.Description, 12, 0.05f, 0.95f, 1f, 1f, 0f, 0f, -470f, -420f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1, 1f));
		updatePool.Add(cui.UpdateText("selectedpluginrdate", "0.8 0.8 0.8 0.6", $"{result.Day} {result:MMMM}, {result.Year:0000}", 15, 0f, 1f, 0f, 0.5f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1, 1f));
		updatePool.Add(cui.UpdateText("selectedplugininfo", "0.8 0.8 0.8 0.6", string.Format("by <b>{0}</b>  <b>•</b>  v{1}  <b>•</b>  Updated on {2}  <b>•</b>  {3:n0} downloads", new object[4] { plugin.Author, plugin.Version, plugin.UpdateDate, plugin.DownloadCount }), 12, 0.05f, 0.95f, 1f, 1f, 0f, 0f, -450f, -400f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1, 1f));
		StringBuilder stringBuilder = Pool.Get<StringBuilder>();
		for (int num = 0; num < 5; num++)
		{
			stringBuilder.Append((plugin.Rating <= (float)num) ? "☆" : "★");
		}
		updatePool.Add(cui.UpdateText("selectedpluginrating", "0.8 0.8 0.8 0.6", stringBuilder.ToString(), 18, 0.3f, 0.7f, 0.7f, 0.75f, 0f, 0f, 0f, 0f, (TextAnchor)1, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1));
		Pool.FreeUnmanaged(ref stringBuilder);
		if (plugin.IsInstalled())
		{
			updatePool.Add(cui.UpdateProtectedButton("selectedplugin_b1", "#b84242", Cache.CUI.BlankColor, string.Empty, 0, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "pluginbrowser.interact 2 \"" + Path.GetFileNameWithoutExtension(plugin.File) + "\"", (TextAnchor)6));
			updatePool.Add(cui.UpdateText("selectedplugin_b1_txt", "#f7a3a3", "UNINSTALL", 12, 0.2f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1));
			updatePool.Add(cui.UpdateImage("selectedplugin_b1_icn", "trashcan", "#f7a3a3"));
			updatePool.Add(cui.UpdateImage("selectedplugin_b1_fade", "fade", Cache.CUI.WhiteColor));
			updatePool.Add(cui.UpdateProtectedButton("selectedplugin_b2", "#b84242", Cache.CUI.BlankColor, string.Empty, 0, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "pluginbrowser.interact 11 \"" + Path.GetFileNameWithoutExtension(plugin.File) + "\"", (TextAnchor)6));
			updatePool.Add(cui.UpdateText("selectedplugin_b2_txt", "#f7a3a3", "UNLOAD", 12, 0.2f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1));
			updatePool.Add(cui.UpdateImage("selectedplugin_b2_icn", "installed", "#f7a3a3"));
			updatePool.Add(cui.UpdateImage("selectedplugin_b2_fade", "fade", Cache.CUI.WhiteColor));
			updatePool.Add(cui.UpdateProtectedButton("selectedplugin_b3", "0.2 0.2 0.2 0.8", Cache.CUI.BlankColor, string.Empty, 0, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "pluginbrowser.interact 3 \"" + Path.GetFileNameWithoutExtension(plugin.File) + "\"", (TextAnchor)6));
			updatePool.Add(cui.UpdateText("selectedplugin_b3_txt", "0.8 0.8 0.8 0.8", "CONFIG", 12, 0.2f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1));
			updatePool.Add(cui.UpdateImage("selectedplugin_b3_icn", "file", "0.8 0.8 0.8 0.8"));
			updatePool.Add(cui.UpdateImage("selectedplugin_b3_fade", "fade", Cache.CUI.WhiteColor));
			updatePool.Add(cui.UpdateProtectedButton("selectedplugin_b4", "0.2 0.2 0.2 0.8", Cache.CUI.BlankColor, string.Empty, 0, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "pluginbrowser.interact 5 \"" + Path.GetFileNameWithoutExtension(plugin.File) + "\"", (TextAnchor)6));
			updatePool.Add(cui.UpdateText("selectedplugin_b4_txt", "0.8 0.8 0.8 0.8", "LANG", 12, 0.2f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1));
			updatePool.Add(cui.UpdateImage("selectedplugin_b4_icn", "translate", "0.8 0.8 0.8 0.8"));
			updatePool.Add(cui.UpdateImage("selectedplugin_b4_fade", "fade", Cache.CUI.WhiteColor));
			bool flag = !plugin.IsUpToDate();
			if (plugin.IsPaid() && !plugin.Owned)
			{
				flag = false;
			}
			updatePool.Add(cui.UpdateProtectedButton("selectedplugin_b5", $"0.2 0.2 0.2 {(flag ? 0.8f : 0.2f)}", Cache.CUI.BlankColor, string.Empty, 0, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, flag ? ("pluginbrowser.interact 1 \"" + Path.GetFileNameWithoutExtension(plugin.File) + "\"") : string.Empty, (TextAnchor)6));
			updatePool.Add(cui.UpdateText("selectedplugin_b5_txt", "0.8 0.8 0.8 0.8", "UPDATE", 12, 0.2f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1));
			updatePool.Add(cui.UpdateImage("selectedplugin_b5_icn", "clouddl", "0.8 0.8 0.8 0.8"));
			updatePool.Add(cui.UpdateImage("selectedplugin_b5_fade", "fade", Cache.CUI.WhiteColor));
		}
		else
		{
			bool flag2 = (plugin.GetPreferredVendor() != PluginsTab.LocalInstance && !plugin.IsPaid()) || plugin.Owned || (plugin.AvailableOn != null && plugin.AvailableOn.Count > 1);
			updatePool.Add(cui.UpdateProtectedButton("selectedplugin_b1", (!flag2) ? CUI.HexToRustColor("#8db842", 0.4f) : "#8db842", Cache.CUI.BlankColor, string.Empty, 0, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, flag2 ? ("pluginbrowser.interact 0 \"" + Path.GetFileNameWithoutExtension(plugin.File) + "\"") : string.Empty, (TextAnchor)6));
			updatePool.Add(cui.UpdateText("selectedplugin_b1_txt", "#d9f7a3", flag2 ? "DOWNLOAD" : "CAN'T DOWNLOAD", 12, 0.2f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1));
			updatePool.Add(cui.UpdateImage("selectedplugin_b1_icn", "clouddl", "#d9f7a3"));
			updatePool.Add(cui.UpdateImage("selectedplugin_b1_fade", "fade", Cache.CUI.BlankColor));
			bool flag3 = !string.IsNullOrEmpty(CorePlugin.GetPluginFile(Path.GetFileNameWithoutExtension(plugin.File)).Path);
			updatePool.Add(cui.UpdateProtectedButton("selectedplugin_b2", (!flag3) ? CUI.HexToRustColor("#8db842", 0.4f) : "#8db842", Cache.CUI.BlankColor, string.Empty, 0, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "pluginbrowser.interact 11 \"" + Path.GetFileNameWithoutExtension(plugin.File) + "\"", (TextAnchor)6));
			updatePool.Add(cui.UpdateText("selectedplugin_b2_txt", "#d9f7a3", "LOAD", 12, 0.2f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1));
			updatePool.Add(cui.UpdateImage("selectedplugin_b2_icn", "installed", "#d9f7a3"));
			updatePool.Add(cui.UpdateImage("selectedplugin_b2_fade", "fade", Cache.CUI.WhiteColor));
			updatePool.Add(cui.UpdateProtectedButton("selectedplugin_b3", Cache.CUI.BlankColor, Cache.CUI.BlankColor, string.Empty, 0, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, null, (TextAnchor)6));
			updatePool.Add(cui.UpdateText("selectedplugin_b3_txt", Cache.CUI.BlankColor, string.Empty, 0, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1));
			updatePool.Add(cui.UpdateImage("selectedplugin_b3_icn", "trashcan", Cache.CUI.BlankColor));
			updatePool.Add(cui.UpdateImage("selectedplugin_b3_fade", "fade", Cache.CUI.BlankColor));
			updatePool.Add(cui.UpdateProtectedButton("selectedplugin_b4", Cache.CUI.BlankColor, Cache.CUI.BlankColor, string.Empty, 0, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, null, (TextAnchor)6));
			updatePool.Add(cui.UpdateText("selectedplugin_b4_txt", Cache.CUI.BlankColor, string.Empty, 0, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1));
			updatePool.Add(cui.UpdateImage("selectedplugin_b4_icn", "trashcan", Cache.CUI.BlankColor));
			updatePool.Add(cui.UpdateImage("selectedplugin_b4_fade", "fade", Cache.CUI.BlankColor));
			updatePool.Add(cui.UpdateProtectedButton("selectedplugin_b5", Cache.CUI.BlankColor, Cache.CUI.BlankColor, string.Empty, 0, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, null, (TextAnchor)6));
			updatePool.Add(cui.UpdateText("selectedplugin_b5_txt", Cache.CUI.BlankColor, string.Empty, 0, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1));
			updatePool.Add(cui.UpdateImage("selectedplugin_b5_icn", "trashcan", Cache.CUI.BlankColor));
			updatePool.Add(cui.UpdateImage("selectedplugin_b5_fade", "fade", Cache.CUI.BlankColor));
		}
		updatePool.Send(ArgEx.Player(arg));
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("pluginbrowser.deselectplugin")]
	private void PluginBrowserDeselectPlugin(Arg arg)
	{
		using CUI cui = new CUI(Singleton.Handler);
		using CUI.Handler.UpdatePool updatePool = cui.UpdatePool();
		updatePool.Add(cui.UpdatePanel("selectedpluginpnl", Cache.CUI.BlankColor, null, 0f, 0f, -1000f, -1000f));
		updatePool.Send(ArgEx.Player(arg));
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("pluginbrowser.changeselectedplugin")]
	private void PluginBrowserChangeSelected(Arg args)
	{
		PlayerSession playerSession = Singleton.GetPlayerSession(ArgEx.Player(args));
		Tab tab = Singleton.GetTab(playerSession.Player);
		PluginsTab.Vendor vendor = PluginsTab.GetVendor(playerSession.GetStorage(tab, "vendor", PluginsTab.VendorTypes.Installed));
		vendor.Refresh();
		List<PluginsTab.Plugin> plugins = PluginsTab.GetPlugins(vendor, tab, playerSession, 15);
		int num = plugins.IndexOf(playerSession.GetStorage<PluginsTab.Plugin>(tab, "selectedplugin")) + args.GetInt(0, 0);
		playerSession.SetStorage(tab, "selectedplugin", plugins[(num <= plugins.Count - 1) ? ((num < 0) ? (plugins.Count - 1) : num) : 0]);
		Pool.FreeUnmanaged<PluginsTab.Plugin>(ref plugins);
		PluginsTab.DownloadThumbnails(vendor, tab, Singleton.GetPlayerSession(ArgEx.Player(args)));
		Singleton.Draw(ArgEx.Player(args));
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("pluginbrowser.changesetting")]
	private void PluginBrowserChangeSetting(Arg args)
	{
		PlayerSession playerSession = Singleton.GetPlayerSession(ArgEx.Player(args));
		Tab tab = Singleton.GetTab(playerSession.Player);
		PluginsTab.Vendor vendor = PluginsTab.GetVendor(playerSession.GetStorage(tab, "vendor", PluginsTab.VendorTypes.Installed));
		string text = args.GetString(0, "");
		if (text == "filter_dd")
		{
			PluginsTab.DropdownShow = !PluginsTab.DropdownShow;
			if (args.HasArgs(4))
			{
				int num = args.GetInt(3, 0);
				PluginsTab.FilterTypes storage = playerSession.GetStorage(tab, "filter", PluginsTab.FilterTypes.None);
				bool storage2 = playerSession.GetStorage(tab, "flipfilter", @default: false);
				if (storage == (PluginsTab.FilterTypes)num)
				{
					playerSession.SetStorage(tab, "flipfilter", !storage2);
				}
				else
				{
					playerSession.SetStorage(tab, "flipfilter", value: false);
				}
				playerSession.SetStorage(tab, "page", 0);
				playerSession.SetStorage(tab, "filter", (PluginsTab.FilterTypes)num);
			}
		}
		PluginsTab.DownloadThumbnails(vendor, tab, Singleton.GetPlayerSession(ArgEx.Player(args)));
		vendor.Refresh();
		Singleton.Draw(ArgEx.Player(args));
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("pluginbrowser.login")]
	private void PluginBrowserLogin(Arg args)
	{
		PlayerSession ap = Singleton.GetPlayerSession(ArgEx.Player(args));
		BasePlayer player = ap.Player;
		Tab tab = Singleton.GetTab(ap.Player);
		PluginsTab.Vendor vendor = PluginsTab.GetVendor(ap.GetStorage(tab, "vendor", PluginsTab.VendorTypes.Installed));
		PluginsTab.IVendorAuthenticated auth = vendor as PluginsTab.IVendorAuthenticated;
		if (auth == null)
		{
			return;
		}
		if (auth.IsLoggedIn)
		{
			tab.CreateDialog("Are you sure you want to log out?", delegate
			{
				auth.User = null;
				vendor.Refresh();
				Singleton.Draw(player);
				if (vendor is PluginsTab.IVendorStored vendorStored)
				{
					vendorStored.Save();
				}
			});
		}
		else
		{
			auth.AuthCode = StringEx.Truncate(Guid.NewGuid().ToString(), 6).ToUpper();
			auth.User = new PluginsTab.LoggedInUser
			{
				PendingAccessToken = true
			};
			string currentCode = auth.AuthCode;
			CorePlugin core = Community.Runtime.Core;
			core.timer.In(5f, delegate
			{
				if (currentCode != auth.AuthCode || !ap.IsInMenu)
				{
					auth.User = null;
				}
				else
				{
					auth.Validate(ap, delegate
					{
						core.timer.In(2f, delegate
						{
							auth.User.PendingAccessToken = false;
							Singleton.Draw(player);
							auth.RefreshUser(ap);
						});
						Singleton.Draw(player);
					});
				}
			});
		}
		Singleton.Draw(ArgEx.Player(args));
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("pluginbrowser.closelogin")]
	private void PluginBrowserCloseLogin(Arg args)
	{
		PlayerSession playerSession = Singleton.GetPlayerSession(ArgEx.Player(args));
		Tab tab = Singleton.GetTab(playerSession.Player);
		PluginsTab.Vendor vendor = PluginsTab.GetVendor(playerSession.GetStorage(tab, "vendor", PluginsTab.VendorTypes.Installed));
		if (vendor is PluginsTab.IVendorAuthenticated vendorAuthenticated)
		{
			vendorAuthenticated.User = null;
			vendorAuthenticated.ValidationTimer?.Destroy();
			Singleton.Draw(ArgEx.Player(args));
		}
	}

	[Conditional("!MINIMAL")]
	[ConsoleCommand("adminmodule.downloadplugin", "Downloads a plugin from a vendor (if available). Syntax: adminmodule.downloadplugin <codefling|umod> <plugin>")]
	[AuthLevel(2)]
	private void DownloadPlugin(Arg args)
	{
		PluginsTab.Vendor vendor = PluginsTab.GetVendor((args.GetString(0, "") == "codefling") ? PluginsTab.VendorTypes.Codefling : PluginsTab.VendorTypes.uMod);
		if (vendor == null)
		{
			Singleton.PutsWarn("Couldn't find that vendor.");
			return;
		}
		PluginsTab.Plugin plugin = vendor.FetchedPlugins.FirstOrDefault((PluginsTab.Plugin x) => x.Name.Equals(args.GetString(1, ""), StringComparison.InvariantCultureIgnoreCase));
		if (plugin == null)
		{
			Singleton.PutsWarn("Cannot find that plugin.");
			return;
		}
		vendor.Download(plugin.Id, delegate
		{
			Singleton.PutsWarn("Couldn't download " + plugin.Name + ".");
		});
	}

	[Conditional("!MINIMAL")]
	[ConsoleCommand("adminmodule.updatevendor", "Downloads latest vendor information. Syntax: adminmodule.updatevendor <codefling|umod>")]
	[AuthLevel(2)]
	private void UpdateVendor(Arg arg)
	{
		PluginsTab.Vendor vendor = PluginsTab.GetVendor((arg.GetString(0, "") == "codefling") ? PluginsTab.VendorTypes.Codefling : PluginsTab.VendorTypes.uMod);
		if (vendor == null)
		{
			Singleton.PutsWarn("Couldn't find that vendor.");
			return;
		}
		string text = string.Empty;
		if (!(vendor is PluginsTab.Codefling))
		{
			if (vendor is PluginsTab.uMod)
			{
				text = "umod";
			}
		}
		else
		{
			text = "cf";
		}
		string file = Path.Combine(Defines.GetDataFolder(), "vendordata_" + text + ".db");
		OsEx.File.Delete(file);
		if (vendor is PluginsTab.IVendorStored vendorStored && !vendorStored.Load())
		{
			vendor.FetchList(delegate(PluginsTab.Vendor vendor2)
			{
				vendor2.Refresh();
			});
			vendor.Refresh();
		}
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("adminmodule.profilerselect")]
	private void ProfilerSelect(Arg arg)
	{
		PlayerSession playerSession = GetPlayerSession(ArgEx.Player(arg));
		MonoProfiler.AssemblyRecord assemblyRecord = ProfilerTab.GetSortedAssemblies(playerSession.GetStorage(playerSession.SelectedTab, "bsort", 1), playerSession.GetStorage(playerSession.SelectedTab, "bsearch", string.Empty)).FindAt(arg.GetInt(0, 0));
		playerSession.SetStorage(null, "profilerval", (assemblyRecord.assembly_name == null) ? string.Empty : assemblyRecord.assembly_name.name);
		playerSession.SelectedTab.OnChange(playerSession, playerSession.SelectedTab);
		Draw(playerSession.Player);
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("adminmodule.profilerselectcall")]
	private void ProfilerSelectCall(Arg arg)
	{
		BasePlayer player = ArgEx.Player(arg);
		if (HasAccess(player, "profiler.sourceviewer") && !ProfilerTab.sample.FromDisk)
		{
			int index = arg.GetInt(0, 0);
			PlayerSession playerSession = GetPlayerSession(player);
			string storage = playerSession.GetStorage<string>(null, "profilerval");
			MonoProfiler.CallRecord call = ProfilerTab.GetSortedCalls(storage, playerSession.GetStorage(playerSession.SelectedTab, "asort", 1), playerSession.GetStorage(playerSession.SelectedTab, "asearch", string.Empty)).FindAt(index);
			Tab currentTab = playerSession.SelectedTab;
			SourceViewerTab sourceViewerTab = SourceViewerTab.MakeMethod(call);
			sourceViewerTab.Close = delegate
			{
				SetTab(player, currentTab);
			};
			SetTab(player, sourceViewerTab);
		}
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("adminmodule.profilertoggle")]
	private void ProfilerToggle(Arg arg)
	{
		BasePlayer player = ArgEx.Player(arg);
		if (!HasAccess(player, "profiler.startstop"))
		{
			return;
		}
		PlayerSession ap = GetPlayerSession(player);
		if (!MonoProfiler.Enabled)
		{
			return;
		}
		if (!MonoProfiler.IsRecording && ap.Player.serverInput.IsDown((BUTTON)128))
		{
			Dictionary<string, ModalModule.Modal.Field> dictionary = Pool.Get<Dictionary<string, ModalModule.Modal.Field>>();
			dictionary["duration"] = ModalModule.Modal.Field.Make("Duration", ModalModule.Modal.Field.FieldTypes.Float, required: true, 3f, isReadOnly: false, (ModalModule.Modal.Field field) => (!(field.Value.ToString().ToFloat() <= 0f)) ? string.Empty : "Duration must be above zero.");
			dictionary["calls"] = ModalModule.Modal.Field.Make("Calls", ModalModule.Modal.Field.FieldTypes.Boolean, required: false, true);
			dictionary["advancedmemory"] = ModalModule.Modal.Field.Make("Advanced Memory", ModalModule.Modal.Field.FieldTypes.Boolean, required: false, true);
			dictionary["callmemory"] = ModalModule.Modal.Field.Make("Call Memory", ModalModule.Modal.Field.FieldTypes.Boolean, required: false, true);
			dictionary["swa"] = ModalModule.Modal.Field.Make("Stack Walk Allocations", ModalModule.Modal.Field.FieldTypes.Boolean, required: false, true);
			dictionary["gc"] = ModalModule.Modal.Field.Make("GC Events", ModalModule.Modal.Field.FieldTypes.Boolean, required: false, true);
			dictionary["timings"] = ModalModule.Modal.Field.Make("Timings (Performance Intensive)", ModalModule.Modal.Field.FieldTypes.Boolean, required: false, true);
			Modal.Open(player, "Profile Recording", dictionary, delegate
			{
				MonoProfiler.ProfilerArgs profilerArgs = MonoProfiler.ProfilerArgs.None;
				if (dictionary["advancedmemory"].Get<bool>())
				{
					profilerArgs |= MonoProfiler.ProfilerArgs.AdvancedMemory;
				}
				if (dictionary["callmemory"].Get<bool>())
				{
					profilerArgs |= MonoProfiler.ProfilerArgs.CallMemory;
				}
				if (dictionary["calls"].Get<bool>())
				{
					profilerArgs |= MonoProfiler.ProfilerArgs.Calls;
				}
				if (dictionary["timings"].Get<bool>())
				{
					profilerArgs |= MonoProfiler.ProfilerArgs.Timings;
				}
				if (dictionary["gc"].Get<bool>())
				{
					profilerArgs |= MonoProfiler.ProfilerArgs.GCEvents;
				}
				if (dictionary["swa"].Get<bool>())
				{
					profilerArgs |= MonoProfiler.ProfilerArgs.StackWalkAllocations;
				}
				float duration = dictionary["duration"].Get<float>();
				MonoProfiler.Clear();
				MonoProfiler.ToggleProfilingTimed(duration, profilerArgs, delegate
				{
					if (ap.IsInMenu && ap.SelectedTab != null && ap.SelectedTab.Id == "profiler")
					{
						ap.SelectedTab.OnChange(ap, ap.SelectedTab);
						Draw(ap.Player);
					}
					ProfilerTab.sample.Resample();
					Analytics.profiler_ended(profilerArgs, ProfilerTab.sample.Duration, timed: true);
				}, logging: false);
				Analytics.profiler_started(profilerArgs, timed: true);
				Pool.FreeUnmanaged<string, ModalModule.Modal.Field>(ref dictionary);
				ap.SelectedTab.OnChange(ap, ap.SelectedTab);
				Draw(player);
			}, delegate
			{
				Pool.FreeUnmanaged<string, ModalModule.Modal.Field>(ref dictionary);
				ap.SelectedTab.OnChange(ap, ap.SelectedTab);
				Draw(player);
			});
		}
		else
		{
			MonoProfiler.ToggleProfiling(MonoProfiler.ProfilerArgs.CallMemory | MonoProfiler.ProfilerArgs.AdvancedMemory | MonoProfiler.ProfilerArgs.Timings | MonoProfiler.ProfilerArgs.Calls | MonoProfiler.ProfilerArgs.GCEvents, logging: false);
			if (!MonoProfiler.IsRecording)
			{
				ProfilerTab.sample.Resample();
				MonoProfiler.Clear();
				Analytics.profiler_ended(MonoProfiler.ProfilerArgs.CallMemory | MonoProfiler.ProfilerArgs.AdvancedMemory | MonoProfiler.ProfilerArgs.Timings | MonoProfiler.ProfilerArgs.Calls | MonoProfiler.ProfilerArgs.GCEvents, ProfilerTab.sample.Duration, timed: false);
			}
			ap.SelectedTab.OnChange(ap, ap.SelectedTab);
			Draw(player);
		}
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("adminmodule.profilerexport")]
	private void ProfilerExport(Arg arg)
	{
		if (!ProfilerTab.sample.IsCleared)
		{
			PlayerSession playerSession = GetPlayerSession(ArgEx.Player(arg));
			switch (arg.GetInt(0, 0))
			{
			case 0:
				WriteFileString("txt", ProfilerTab.sample.ToTable(), playerSession.Player);
				break;
			case 1:
				WriteFileString("json", ProfilerTab.sample.ToJson(indented: true), playerSession.Player);
				break;
			case 2:
				WriteFileString("csv", ProfilerTab.sample.ToCSV(), playerSession.Player);
				break;
			case 3:
				WriteFileBytes("cprf", ProfilerTab.sample.ToProto(), playerSession.Player);
				break;
			}
		}
		static void WriteFileBytes(string extension, byte[] data, BasePlayer player)
		{
			DateTime now = DateTime.Now;
			string text = Path.Combine(Defines.GetProfilesFolder(), string.Format("profile-{0}_{1}_{2}_{3}{4}{5}.{6}", new object[7] { now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, extension }));
			OsEx.File.Create(text, data);
			Notifications.Add(player, "Exported profile output at '" + text + "'");
		}
		static void WriteFileString(string extension, string data, BasePlayer player)
		{
			DateTime now = DateTime.Now;
			string text = Path.Combine(Defines.GetProfilesFolder(), string.Format("profile-{0}_{1}_{2}_{3}{4}{5}.{6}", new object[7] { now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, extension }));
			OsEx.File.Create(text, data);
			Notifications.Add(player, "Exported profile output at '" + text + "'");
		}
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("adminmodule.profilerimport")]
	private void ProfilerImport(Arg arg)
	{
		if (ProfilerTab.sample.IsCompared)
		{
			return;
		}
		BasePlayer player = ArgEx.Player(arg);
		File.Open(player, "Profiles", Defines.GetProfilesFolder(), Defines.GetProfilesFolder(), "cprf", delegate(BasePlayer player2, FileModule.FileBrowser file)
		{
			byte[] data = OsEx.File.ReadBytes(file.SelectedFile);
			if (ProfilerTab.sample.IsCleared)
			{
				ProfilerTab.sample = MonoProfiler.Sample.Load(data);
			}
			else
			{
				MonoProfiler.Sample other = MonoProfiler.Sample.Load(data);
				ProfilerTab.sample = ProfilerTab.sample.Compare(other);
			}
			PlayerSession playerSession = Singleton.GetPlayerSession(player2);
			playerSession.SelectedTab.OnChange(playerSession, playerSession.SelectedTab);
			Singleton.Draw(player2);
		}, null, delegate(FileModule.FileBrowser.Item item)
		{
			if (item.IsDirectory)
			{
				return string.Empty;
			}
			int protocol;
			double duration;
			bool isCompared;
			return MonoProfiler.ValidateFile(item.Path, out protocol, out duration, out isCompared) ? string.Format("Duration: {0}s (protocol {1}){2}", TimeEx.FormatPlayer(duration).ToLower(), protocol, isCompared ? " [C]" : string.Empty) : $"Invalid protocol {protocol}";
		});
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("adminmodule.profilerclear")]
	private void ProfilerClear(Arg arg)
	{
		PlayerSession playerSession = GetPlayerSession(ArgEx.Player(arg));
		if (MonoProfiler.IsRecording)
		{
			MonoProfiler.ToggleProfiling(MonoProfiler.ProfilerArgs.Abort);
		}
		else
		{
			ProfilerTab.sample.Clear();
			MonoProfiler.Clear();
			playerSession.SetStorage(null, "profilerval", string.Empty);
		}
		playerSession.SelectedTab.OnChange(playerSession, playerSession.SelectedTab);
		Draw(playerSession.Player);
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("adminmodule.timelinemode")]
	private void TimelineMode(Arg arg)
	{
		PlayerSession playerSession = GetPlayerSession(ArgEx.Player(arg));
		playerSession.SetStorage(null, "timeline", !playerSession.GetStorage(null, "timeline", @default: false));
		playerSession.SelectedTab.OnChange(playerSession, playerSession.SelectedTab);
		Draw(playerSession.Player);
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("adminmodule.timelinetoggle")]
	private void TimelineToggle(Arg arg)
	{
		BasePlayer player = ArgEx.Player(arg);
		if (!HasAccess(player, "profiler.startstop"))
		{
			return;
		}
		PlayerSession ap = GetPlayerSession(player);
		if (!MonoProfiler.Enabled)
		{
			return;
		}
		if (!MonoProfiler.IsRecording)
		{
			Dictionary<string, ModalModule.Modal.Field> dictionary = Pool.Get<Dictionary<string, ModalModule.Modal.Field>>();
			dictionary["duration"] = ModalModule.Modal.Field.Make("Duration", ModalModule.Modal.Field.FieldTypes.Float, required: true, 3f, isReadOnly: false, (ModalModule.Modal.Field field) => (!(field.Get<float>() <= 0f)) ? ((!(field.Get<float>() > 100f)) ? string.Empty : ("You cannot record above " + TimeEx.Format(100, shortName: false).ToLower() + ".")) : "Duration must be above zero.");
			dictionary["rate"] = ModalModule.Modal.Field.Make("Rate", ModalModule.Modal.Field.FieldTypes.Float, required: true, 1f, isReadOnly: false, (ModalModule.Modal.Field field) => (!(field.Get<float>() < 1f)) ? ((!(field.Get<float>() > 10f)) ? string.Empty : "Rate must be under or equal to 10 seconds.") : "Rate must be above or equal to one second.");
			dictionary["calls"] = ModalModule.Modal.Field.Make("Calls", ModalModule.Modal.Field.FieldTypes.Boolean, required: false, true);
			dictionary["advancedmemory"] = ModalModule.Modal.Field.Make("Advanced Memory", ModalModule.Modal.Field.FieldTypes.Boolean, required: false, true);
			dictionary["callmemory"] = ModalModule.Modal.Field.Make("Call Memory", ModalModule.Modal.Field.FieldTypes.Boolean, required: false, true);
			dictionary["swa"] = ModalModule.Modal.Field.Make("Stack Walk Allocations", ModalModule.Modal.Field.FieldTypes.Boolean, required: false, true);
			dictionary["timings"] = ModalModule.Modal.Field.Make("Timings (Performance Intensive)", ModalModule.Modal.Field.FieldTypes.Boolean, required: false, true);
			Modal.Open(player, "Timeline Profiling", dictionary, delegate(BasePlayer player2, ModalModule.Modal _)
			{
				MonoProfiler.ProfilerArgs profilerArgs = MonoProfiler.ProfilerArgs.None;
				if (dictionary["advancedmemory"].Get<bool>())
				{
					profilerArgs |= MonoProfiler.ProfilerArgs.AdvancedMemory;
				}
				if (dictionary["callmemory"].Get<bool>())
				{
					profilerArgs |= MonoProfiler.ProfilerArgs.CallMemory;
				}
				if (dictionary["calls"].Get<bool>())
				{
					profilerArgs |= MonoProfiler.ProfilerArgs.Calls;
				}
				if (dictionary["timings"].Get<bool>())
				{
					profilerArgs |= MonoProfiler.ProfilerArgs.Timings;
				}
				if (dictionary["swa"].Get<bool>())
				{
					profilerArgs |= MonoProfiler.ProfilerArgs.StackWalkAllocations;
				}
				ProfilerTab.recording.Discard();
				ProfilerTab.recording.Start(dictionary["rate"].Get<float>(), dictionary["duration"].Get<float>(), profilerArgs, delegate(bool discarded)
				{
					if (discarded)
					{
						Notifications.Add(player2, "Timeline profiling has been discarded.");
					}
					if (ap.IsInMenu && ap.SelectedTab != null && ap.SelectedTab.Id == "profiler")
					{
						ap.SelectedTab.OnChange(ap, ap.SelectedTab);
						Draw(ap.Player);
					}
					Analytics.profiler_tl_ended(profilerArgs, ProfilerTab.recording.CurrentDuration, ProfilerTab.recording.Status);
				});
				Analytics.profiler_tl_started(profilerArgs);
				Pool.FreeUnmanaged<string, ModalModule.Modal.Field>(ref dictionary);
				ap.SelectedTab.OnChange(ap, ap.SelectedTab);
				Draw(player2);
			}, delegate
			{
				Pool.FreeUnmanaged<string, ModalModule.Modal.Field>(ref dictionary);
				ap.SelectedTab.OnChange(ap, ap.SelectedTab);
				Draw(player);
			});
		}
		else
		{
			ProfilerTab.recording?.Stop();
			ap.SelectedTab.OnChange(ap, ap.SelectedTab);
			Analytics.profiler_tl_ended(ProfilerTab.recording.Args, ProfilerTab.recording.CurrentDuration, ProfilerTab.recording.Status);
			Draw(player);
		}
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("adminmodule.timelineclear")]
	private void TimelineClear(Arg arg)
	{
		PlayerSession playerSession = GetPlayerSession(ArgEx.Player(arg));
		if (ProfilerTab.recording.IsRecording())
		{
			ProfilerTab.recording.Stop(discard: true);
			Analytics.profiler_tl_ended(ProfilerTab.recording.Args, ProfilerTab.recording.CurrentDuration, ProfilerTab.recording.Status);
		}
		else
		{
			ProfilerTab.recording.Discard();
		}
		playerSession.SelectedTab.OnChange(playerSession, playerSession.SelectedTab);
		Draw(playerSession.Player);
	}

	[Conditional("!MINIMAL")]
	[ProtectedCommand("adminmodule.profilerpreviewclose")]
	private void ProfilerPreviewClose(Arg arg)
	{
		PlayerSession playerSession = GetPlayerSession(ArgEx.Player(arg));
		if (playerSession.SelectedTab is SourceViewerTab sourceViewerTab)
		{
			sourceViewerTab.Close?.Invoke(playerSession);
		}
	}

	public override object InternalCallHook(uint hook, object[] args)
	{
		//IL_1061: Unknown result type (might be due to invalid IL or missing references)
		//IL_070c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e51: Unknown result type (might be due to invalid IL or missing references)
		//IL_0580: Unknown result type (might be due to invalid IL or missing references)
		//IL_0604: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1442: Unknown result type (might be due to invalid IL or missing references)
		//IL_1127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f59: Unknown result type (might be due to invalid IL or missing references)
		//IL_10a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1484: Unknown result type (might be due to invalid IL or missing references)
		//IL_077d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c41: Unknown result type (might be due to invalid IL or missing references)
		//IL_0872: Unknown result type (might be due to invalid IL or missing references)
		//IL_053a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0830: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e0f: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_11ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dcd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fdd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d49: Unknown result type (might be due to invalid IL or missing references)
		//IL_0688: Unknown result type (might be due to invalid IL or missing references)
		//IL_101f: Unknown result type (might be due to invalid IL or missing references)
		//IL_122f: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c83: Unknown result type (might be due to invalid IL or missing references)
		//IL_0646: Unknown result type (might be due to invalid IL or missing references)
		//IL_1169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ed5: Unknown result type (might be due to invalid IL or missing references)
		//IL_11ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a81: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f9b: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e93: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d8b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0968: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f17: Unknown result type (might be due to invalid IL or missing references)
		//IL_1505: Unknown result type (might be due to invalid IL or missing references)
		//IL_14c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_10e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0737: Unknown result type (might be due to invalid IL or missing references)
		//IL_0921: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0993: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bf5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bfa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c0e: Unknown result type (might be due to invalid IL or missing references)
		int? num = args?.Length;
		object obj = ((num > 0) ? args[0] : null);
		object obj2 = ((num > 1) ? args[1] : null);
		object obj3 = ((num > 2) ? args[2] : null);
		object obj4 = ((num > 3) ? args[3] : null);
		object obj5 = ((num > 4) ? args[4] : null);
		object obj6 = ((num > 5) ? args[5] : null);
		object obj7 = ((num > 6) ? args[6] : null);
		object obj8 = ((num > 7) ? args[7] : null);
		object obj9 = ((num > 8) ? args[8] : null);
		try
		{
			switch (hook)
			{
			case 3567215104u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag23 = flag;
				Arg args6 = ((!flag23) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag23)
				{
					CallAction(args6);
					return null;
				}
				break;
			}
			case 1360889797u:
			{
				bool flag = ((obj is ItemContainer || obj == null) ? true : false);
				bool flag19 = flag;
				ItemContainer container = ((!flag19) ? ((ItemContainer)null) : ((ItemContainer)(obj ?? null)));
				flag = ((obj2 is Item || obj2 == null) ? true : false);
				bool flag20 = flag;
				Item item = ((!flag20) ? ((Item)null) : ((Item)(obj2 ?? null)));
				flag = ((obj3 is int || obj3 == null) ? true : false);
				bool flag21 = flag;
				int targetPos = (flag21 ? ((int)(obj3 ?? ((object)0))) : 0);
				if (flag19 && flag20 && flag21)
				{
					return CanAcceptItem(container, item, targetPos);
				}
				break;
			}
			case 3983791359u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag72 = flag;
				BasePlayer player7 = ((!flag72) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				if (flag72)
				{
					return CanAccess(player7);
				}
				break;
			}
			case 2673788810u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag31 = flag;
				Arg args10 = ((!flag31) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag31)
				{
					ChangeColumnPage(args10);
					return null;
				}
				break;
			}
			case 2461250588u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag32 = flag;
				Arg arg12 = ((!flag32) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag32)
				{
					ChangePage(arg12);
					return null;
				}
				break;
			}
			case 2281510334u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag4 = flag;
				Arg args2 = ((!flag4) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag4)
				{
					ChangeTab(args2);
					return null;
				}
				break;
			}
			case 4151318565u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag56 = flag;
				Arg args17 = ((!flag56) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag56)
				{
					CloseUI(args17);
					return null;
				}
				break;
			}
			case 1294167637u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag42 = flag;
				Arg args12 = ((!flag42) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag42)
				{
					Dialog_Action(args12);
					return null;
				}
				break;
			}
			case 821139628u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag14 = flag;
				Arg args3 = ((!flag14) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag14)
				{
					DownloadPlugin(args3);
					return null;
				}
				break;
			}
			case 3944193281u:
			{
				bool flag = ((obj is BaseMountable || obj == null) ? true : false);
				bool flag25 = flag;
				BaseMountable mountable = ((!flag25) ? ((BaseMountable)null) : ((BaseMountable)(obj ?? null)));
				flag = ((obj2 is BasePlayer || obj2 == null) ? true : false);
				bool flag26 = flag;
				BasePlayer player = ((!flag26) ? ((BasePlayer)null) : ((BasePlayer)(obj2 ?? null)));
				if (flag25 && flag26)
				{
					return IModBackpack(mountable, player);
				}
				break;
			}
			case 513549662u:
			{
				bool flag = ((obj is BaseMountable || obj == null) ? true : false);
				bool flag57 = flag;
				BaseMountable mountable2 = ((!flag57) ? ((BaseMountable)null) : ((BaseMountable)(obj ?? null)));
				flag = ((obj2 is BasePlayer || obj2 == null) ? true : false);
				bool flag58 = flag;
				BasePlayer player5 = ((!flag58) ? ((BasePlayer)null) : ((BasePlayer)(obj2 ?? null)));
				if (flag57 && flag58)
				{
					return IValidDismountPosition(mountable2, player5);
				}
				break;
			}
			case 2737989634u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag6 = flag;
				Arg arg3 = ((!flag6) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag6)
				{
					ItemClear(arg3);
					return null;
				}
				break;
			}
			case 3688899430u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag60 = flag;
				Arg arg18 = ((!flag60) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag60)
				{
					ItemCreate(arg18);
					return null;
				}
				break;
			}
			case 4063786885u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag16 = flag;
				Arg arg9 = ((!flag16) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag16)
				{
					ItemSetting(arg9);
					return null;
				}
				break;
			}
			case 347761043u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag18 = flag;
				Arg args4 = ((!flag18) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag18)
				{
					Maximize(args4);
					return null;
				}
				break;
			}
			case 2026747374u:
			{
				bool flag = ((obj is BaseMountable || obj == null) ? true : false);
				bool flag53 = flag;
				BaseMountable entity = ((!flag53) ? ((BaseMountable)null) : ((BaseMountable)(obj ?? null)));
				flag = ((obj2 is BasePlayer || obj2 == null) ? true : false);
				bool flag54 = flag;
				BasePlayer player4 = ((!flag54) ? ((BasePlayer)null) : ((BasePlayer)(obj2 ?? null)));
				if (flag53 && flag54)
				{
					OnEntityDismounted(entity, player4);
					return null;
				}
				break;
			}
			case 1582967250u:
			{
				bool flag = ((obj is BaseEntity || obj == null) ? true : false);
				bool flag36 = flag;
				BaseEntity ent = ((!flag36) ? ((BaseEntity)null) : ((BaseEntity)(obj ?? null)));
				flag = ((obj2 is BasePlayer || obj2 == null) ? true : false);
				bool flag37 = flag;
				BasePlayer player2 = ((!flag37) ? ((BasePlayer)null) : ((BasePlayer)(obj2 ?? null)));
				flag = ((obj3 is uint || obj3 == null) ? true : false);
				bool flag38 = flag;
				uint id = (flag38 ? ((uint)(obj3 ?? ((object)0u))) : 0u);
				flag = ((obj4 is string || obj4 == null) ? true : false);
				bool flag39 = flag;
				string debugName = (flag39 ? ((string)(obj4 ?? null)) : null);
				flag = ((obj5 is float || obj5 == null) ? true : false);
				bool flag40 = flag;
				float maximumDistance = (flag40 ? ((float)(obj5 ?? ((object)0f))) : 0f);
				if (flag36 && flag37 && flag38 && flag39 && flag40)
				{
					return OnEntityDistanceCheck(ent, player2, id, debugName, maximumDistance);
				}
				break;
			}
			case 3141188509u:
			{
				bool flag = ((obj is BaseEntity || obj == null) ? true : false);
				bool flag45 = flag;
				BaseEntity ent2 = ((!flag45) ? ((BaseEntity)null) : ((BaseEntity)(obj ?? null)));
				flag = ((obj2 is BasePlayer || obj2 == null) ? true : false);
				bool flag46 = flag;
				BasePlayer player3 = ((!flag46) ? ((BasePlayer)null) : ((BasePlayer)(obj2 ?? null)));
				flag = ((obj3 is uint || obj3 == null) ? true : false);
				bool flag47 = flag;
				uint id2 = (flag47 ? ((uint)(obj3 ?? ((object)0u))) : 0u);
				flag = ((obj4 is string || obj4 == null) ? true : false);
				bool flag48 = flag;
				string debugName2 = (flag48 ? ((string)(obj4 ?? null)) : null);
				flag = ((obj5 is float || obj5 == null) ? true : false);
				bool flag49 = flag;
				float maximumDistance2 = (flag49 ? ((float)(obj5 ?? ((object)0f))) : 0f);
				if (flag45 && flag46 && flag47 && flag48 && flag49)
				{
					return OnEntityVisibilityCheck(ent2, player3, id2, debugName2, maximumDistance2);
				}
				break;
			}
			case 3088593565u:
			{
				bool flag = ((obj is string || obj == null) ? true : false);
				bool flag8 = flag;
				string condition = (flag8 ? ((string)(obj ?? null)) : null);
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag9 = flag;
				string stackTrace = (flag9 ? ((string)(obj2 ?? null)) : null);
				flag = ((obj3 is LogType || obj3 == null) ? true : false);
				bool flag10 = flag;
				LogType type = (LogType)(flag10 ? ((int)(LogType)(obj3 ?? ((object)(LogType)0))) : 0);
				if (flag8 && flag9 && flag10)
				{
					OnLog(condition, stackTrace, type);
					return null;
				}
				break;
			}
			case 72085565u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag70 = flag;
				BasePlayer player6 = ((!flag70) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				if (flag70)
				{
					OnPlayerDisconnected(player6);
					return null;
				}
				break;
			}
			case 78733418u:
			{
				bool flag = ((obj is PlayerLoot || obj == null) ? true : false);
				bool flag27 = flag;
				PlayerLoot loot = ((!flag27) ? ((PlayerLoot)null) : ((PlayerLoot)(obj ?? null)));
				if (flag27)
				{
					OnPlayerLootEnd(loot);
					return null;
				}
				break;
			}
			case 3051933177u:
			{
				bool flag = ((obj is RustPlugin || obj == null) ? true : false);
				bool flag35 = flag;
				RustPlugin plugin2 = (flag35 ? ((RustPlugin)(obj ?? null)) : null);
				if (flag35)
				{
					OnPluginLoaded(plugin2);
					return null;
				}
				break;
			}
			case 1250294368u:
			{
				bool flag = ((obj is RustPlugin || obj == null) ? true : false);
				bool flag3 = flag;
				RustPlugin plugin = (flag3 ? ((RustPlugin)(obj ?? null)) : null);
				if (flag3)
				{
					OnPluginUnloaded(plugin);
					return null;
				}
				break;
			}
			case 1575289668u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag52 = flag;
				Arg args15 = ((!flag52) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag52)
				{
					PluginBrowserChange(args15);
					return null;
				}
				break;
			}
			case 1844112064u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag24 = flag;
				Arg args7 = ((!flag24) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag24)
				{
					PluginBrowserChangeSelected(args7);
					return null;
				}
				break;
			}
			case 2086187233u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag71 = flag;
				Arg args18 = ((!flag71) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag71)
				{
					PluginBrowserChangeSetting(args18);
					return null;
				}
				break;
			}
			case 3307168968u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag28 = flag;
				Arg args8 = ((!flag28) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag28)
				{
					PluginBrowserCloseLogin(args8);
					return null;
				}
				break;
			}
			case 3216105140u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag12 = flag;
				Arg arg6 = ((!flag12) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag12)
				{
					PluginBrowserDeselectPlugin(arg6);
					return null;
				}
				break;
			}
			case 2137231192u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag51 = flag;
				Arg args14 = ((!flag51) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag51)
				{
					PluginBrowserInteract(args14);
					return null;
				}
				break;
			}
			case 3865171010u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag30 = flag;
				Arg args9 = ((!flag30) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag30)
				{
					PluginBrowserLogin(args9);
					return null;
				}
				break;
			}
			case 1494079807u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag7 = flag;
				Arg arg4 = ((!flag7) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag7)
				{
					PluginBrowserPage(arg4);
					return null;
				}
				break;
			}
			case 1286856038u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag55 = flag;
				Arg args16 = ((!flag55) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag55)
				{
					PluginBrowserRefreshVendor(args16);
					return null;
				}
				break;
			}
			case 3027434422u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag22 = flag;
				Arg args5 = ((!flag22) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag22)
				{
					PluginBrowserSearch(args5);
					return null;
				}
				break;
			}
			case 1756664287u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag5 = flag;
				Arg arg2 = ((!flag5) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag5)
				{
					PluginBrowserSelectPlugin(arg2);
					return null;
				}
				break;
			}
			case 1200368304u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag43 = flag;
				Arg arg15 = ((!flag43) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag43)
				{
					ProfilerClear(arg15);
					return null;
				}
				break;
			}
			case 4013736532u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag33 = flag;
				Arg arg13 = ((!flag33) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag33)
				{
					ProfilerExport(arg13);
					return null;
				}
				break;
			}
			case 1104199099u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag13 = flag;
				Arg arg7 = ((!flag13) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag13)
				{
					ProfilerImport(arg7);
					return null;
				}
				break;
			}
			case 939287569u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag59 = flag;
				Arg arg17 = ((!flag59) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag59)
				{
					ProfilerPreviewClose(arg17);
					return null;
				}
				break;
			}
			case 1496207780u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag34 = flag;
				Arg arg14 = ((!flag34) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag34)
				{
					ProfilerSelect(arg14);
					return null;
				}
				break;
			}
			case 3990053074u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag17 = flag;
				Arg arg10 = ((!flag17) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag17)
				{
					ProfilerSelectCall(arg10);
					return null;
				}
				break;
			}
			case 3314827294u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag11 = flag;
				Arg arg5 = ((!flag11) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag11)
				{
					ProfilerToggle(arg5);
					return null;
				}
				break;
			}
			case 2369215218u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag50 = flag;
				Arg args13 = ((!flag50) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag50)
				{
					ShowConfig(args13);
					return null;
				}
				break;
			}
			case 1081434670u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag41 = flag;
				Arg args11 = ((!flag41) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag41)
				{
					ShowProfiler(args11);
					return null;
				}
				break;
			}
			case 1462991777u:
			{
				bool flag = ((obj is CUI || obj == null) ? true : false);
				bool flag61 = flag;
				CUI cui = (flag61 ? ((CUI)(obj ?? ((object)default(CUI)))) : default(CUI));
				flag = ((obj2 is CuiElementContainer || obj2 == null) ? true : false);
				bool flag62 = flag;
				CuiElementContainer container2 = (flag62 ? ((CuiElementContainer)(obj2 ?? null)) : null);
				flag = ((obj3 is string || obj3 == null) ? true : false);
				bool flag63 = flag;
				string parent = (flag63 ? ((string)(obj3 ?? null)) : null);
				flag = ((obj4 is string || obj4 == null) ? true : false);
				bool flag64 = flag;
				string text = (flag64 ? ((string)(obj4 ?? null)) : null);
				flag = ((obj5 is string || obj5 == null) ? true : false);
				bool flag65 = flag;
				string command = (flag65 ? ((string)(obj5 ?? null)) : null);
				flag = ((obj6 is float || obj6 == null) ? true : false);
				bool flag66 = flag;
				float width = (flag66 ? ((float)(obj6 ?? ((object)0f))) : 0f);
				flag = ((obj7 is float || obj7 == null) ? true : false);
				bool flag67 = flag;
				float offset = (flag67 ? ((float)(obj7 ?? ((object)0f))) : 0f);
				if (flag61 && flag62 && flag63 && flag64 && flag65 && flag66 && flag67)
				{
					TabButton(cui, container2, parent, text, command, width, offset, obj8 is bool flag68 && flag68, obj9 is bool flag69 && flag69);
					return null;
				}
				break;
			}
			case 1709151936u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag44 = flag;
				Arg arg16 = ((!flag44) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag44)
				{
					TimelineClear(arg16);
					return null;
				}
				break;
			}
			case 1042898331u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag29 = flag;
				Arg arg11 = ((!flag29) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag29)
				{
					TimelineMode(arg11);
					return null;
				}
				break;
			}
			case 1088631640u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag15 = flag;
				Arg arg8 = ((!flag15) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag15)
				{
					TimelineToggle(arg8);
					return null;
				}
				break;
			}
			case 1322676003u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag2 = flag;
				Arg arg = ((!flag2) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag2)
				{
					UpdateVendor(arg);
					return null;
				}
				break;
			}
			}
		}
		catch (Exception ex)
		{
			Logger.Error(string.Format("Failed to call internal hook '{0}' on module '{1} v{2}' [{3}]", new object[4]
			{
				HookStringPool.GetOrAdd(hook),
				Name,
				Version,
				hook
			}), ex);
			OnException(hook);
		}
		return null;
	}
}

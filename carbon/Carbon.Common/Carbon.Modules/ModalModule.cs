using System;
using System.Collections.Generic;
using System.Linq;
using Carbon.Base;
using Carbon.Components;
using Carbon.Extensions;
using Carbon.Pooling;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
using UnityEngine;
using UnityEngine.UI;

namespace Carbon.Modules;

public class ModalModule : CarbonModule<EmptyModuleConfig, EmptyModuleData>
{
	public class Modal
	{
		public class Field : IDisposable
		{
			public enum FieldTypes
			{
				String,
				Integer,
				Float,
				ULong,
				Boolean,
				Enum,
				RustColor,
				HexColor,
				Button
			}

			public string DisplayName { get; set; }

			public object Value { get; set; }

			public FieldTypes Type { get; set; }

			public bool IsRequired { get; set; }

			public bool IsReadOnly { get; set; }

			public Func<Field, string> CustomIsInvalid { get; set; }

			public T Get<T>()
			{
				return (T)Value;
			}

			public bool IsInvalid()
			{
				if (!IsRequired || (Value != null && !string.IsNullOrEmpty(Value.ToString())))
				{
					return !string.IsNullOrEmpty(CustomIsInvalid?.Invoke(this));
				}
				return true;
			}

			public static Field Make(string displayName, FieldTypes type, bool required = false, object @default = null, bool isReadOnly = false, Func<Field, string> customIsInvalid = null)
			{
				return new Field
				{
					DisplayName = displayName,
					Type = type,
					IsRequired = required,
					Value = @default,
					IsReadOnly = isReadOnly,
					CustomIsInvalid = customIsInvalid
				};
			}

			public void Dispose()
			{
				DisplayName = null;
				Value = null;
			}
		}

		public class EnumField : Field
		{
			public string[] Options { get; set; }

			public static EnumField MakeEnum(string displayName, string[] options, bool required = false, object @default = null, bool isReadOnly = false, Func<Field, string> customIsInvalid = null)
			{
				return new EnumField
				{
					DisplayName = displayName,
					Type = FieldTypes.Enum,
					IsRequired = required,
					Value = @default,
					Options = options,
					IsReadOnly = isReadOnly,
					CustomIsInvalid = customIsInvalid
				};
			}
		}

		public class ButtonField : Field
		{
			public string ButtonName { get; set; }

			public Action<Modal> Callback { get; set; }

			public static ButtonField MakeButton(string displayName, string buttonName, Action<Modal> callback, bool isReadOnly = false)
			{
				return new ButtonField
				{
					DisplayName = displayName,
					ButtonName = buttonName,
					Type = FieldTypes.Button,
					IsRequired = false,
					Callback = callback,
					IsReadOnly = isReadOnly,
					CustomIsInvalid = null
				};
			}
		}

		public string Title;

		public Dictionary<string, Field> Fields;

		public Action OnCancel;

		public Action<BasePlayer, Modal> OnConfirm;

		public int Page;

		public string BackgroundColor = "0 0 0 0.99";

		public float PositionXMin = 0.3f;

		public float PositionXMax = 0.7f;

		public float PositionYMin = 0.275f;

		public float PositionYMax = 0.825f;

		internal CUI.Handler Handler;

		internal const string PanelId = "carbonmodalui";

		internal BasePlayer Player;

		internal Action<Modal, string, Field, object, object> OnFieldChanged;

		public int Pages
		{
			get
			{
				if (Fields.Count <= 4)
				{
					return 0;
				}
				return (Fields.Count - 1) / 4;
			}
		}

		public string[] InvalidMessages => Fields.Where((KeyValuePair<string, Field> x) => x.Value.Value != null && x.Value.CustomIsInvalid != null).Select(delegate(KeyValuePair<string, Field> x)
		{
			try
			{
				return "    <color=red>" + (x.Value?.CustomIsInvalid?.Invoke(x.Value))?.ToUpper()?.SpacedString(1) + "</color>";
			}
			catch
			{
			}
			return string.Empty;
		}).ToArray();

		public bool IsValid()
		{
			foreach (KeyValuePair<string, Field> field in Fields)
			{
				if (field.Value.IsInvalid())
				{
					return false;
				}
			}
			return true;
		}

		public void Draw(BasePlayer player)
		{
			AdminModule.PlayerSession playerSession = Instance.Admin.GetPlayerSession(player);
			using CUI cui = new CUI(Handler);
			CuiElementContainer container = cui.CreateContainer("carbonmodalui", BackgroundColor, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, needsCursor: true, needsKeyboard: false, CUI.ClientPanels.Overlay, "carbonmodalui");
			CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, "carbonmodalui", "0 0 0 0.6", null, PositionXMin, PositionXMax, PositionYMin, PositionYMax, 0f, 0f, 0f, 0f, blur: false, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, "carbonmodalui.color");
			CUI.Pair<string, CuiElement> pair2 = cui.CreatePanel(container, pair, "0 0 0 0.5", null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, blur: true, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, "carbonmodalui.main");
			_drawInternal(cui, container, pair2);
			playerSession.SetStorage(null, "modal", this);
			cui.Send(container, player);
		}

		internal void _drawInternal(CUI cui, CuiElementContainer container, string panel)
		{
			if (Fields.Count > 0)
			{
				string text = "<b><color=red>*</color></b>  " + "Assign all required field values.".ToUpper().SpacedString(1) + "\n    " + (IsValid() ? ("<b><color=green>" + "The modal is valid.".ToUpper().SpacedString(1) + "</color></b>") : ("<b><color=red>" + "The modal has invalid fields.".ToUpper().SpacedString(1) + "</color></b>")) + ((InvalidMessages != null) ? ("\n" + InvalidMessages.ToString("\n")) : "");
				cui.CreateText(container, panel, "1 1 1 1", text.Trim(), 9, 0.05f, 1f, 0.05f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)6, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
			}
			CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, panel, "0 0 0 0", null, 0.1f, 0.9f, 0.2f, 0.9f);
			cui.CreateText(container, pair, "1 1 1 0.7", Title, 20, 0.025f, 1f, 0f, 0.985f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
			float num = 0f;
			float num2 = 60f;
			CUI.Pair<string, CuiElement> pair2 = cui.CreatePanel(container, pair, "0 0 0 0", null, 0f, 1f, 0f, 1f, 0f, 0f, -35f, -35f);
			IEnumerable<KeyValuePair<string, Field>> enumerable = Fields.Skip(Page * 4).Take(4);
			bool flag = default(bool);
			foreach (KeyValuePair<string, Field> item in enumerable)
			{
				CUI.Pair<string, CuiElement> pair3 = cui.CreatePanel(container, pair2, "0 0 0 0.5", null, 0f, 1f, 0.8f, 1f, 0f, 0f, num, num);
				cui.CreateText(container, pair3, "1 1 1 1", "<b>" + item.Value.DisplayName.ToUpper().SpacedString(1) + "</b>" + (item.Value.IsRequired ? "  <b><color=red>*</color></b>" : ""), 11, 0.03f, 1f, 0f, 0.85f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				CUI.Pair<string, CuiElement> pair4 = cui.CreatePanel(container, pair3, "0.1 0.1 0.1 " + (item.Value.IsReadOnly ? "0.45" : "0.75"), null, 0f, 1f, 0f, 0.55f);
				string color = (item.Value.IsReadOnly ? "1 1 1 0.15" : "1 1 1 1");
				if (item.Value.IsInvalid())
				{
					cui.CreatePanel(container, pair4, CUI.HexToRustColor("#b8302e", 0.5f));
				}
				switch (item.Value.Type)
				{
				case Field.FieldTypes.String:
				case Field.FieldTypes.Integer:
				case Field.FieldTypes.Float:
				case Field.FieldTypes.ULong:
				{
					string text5 = item.Value.Value?.ToString();
					cui.CreateProtectedInputField(container, pair4, color, text5, 15, 256, readOnly: false, 0.025f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "modal.action " + item.Key, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, autoFocus: false, hudMenuInput: false, (LineType)0, 0f, 0f, needsCursor: false, needsKeyboard: true);
					break;
				}
				case Field.FieldTypes.Boolean:
				{
					cui.CreateProtectedButton(container, pair4, "0 0 0 0", "0 0 0 0", string.Empty, 0, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, $"modal.action {item.Key} {item.Value.Value}", (TextAnchor)4);
					CUI.Pair<string, CuiElement, CuiElement> pair5 = cui.CreateProtectedButton(container, pair4, "0.1 0.1 0.1 0.8", "0 0 0 0", string.Empty, 0, null, 0.025f, 0.085f, 0.1f, 0.9f, 0f, 0f, 0f, 0f, $"modal.action {item.Key} {item.Value.Value}", (TextAnchor)4);
					object value = item.Value.Value;
					int num3;
					if (value is bool)
					{
						flag = (bool)value;
						num3 = 1;
					}
					else
					{
						num3 = 0;
					}
					if (((uint)num3 & (flag ? 1u : 0u)) != 0)
					{
						cui.CreateImage(container, pair5, "checkmark", color, null, 0.2f, 0.8f, 0.2f, 0.8f);
					}
					break;
				}
				case Field.FieldTypes.RustColor:
				case Field.FieldTypes.HexColor:
				{
					string text2 = ((!string.IsNullOrEmpty($"{item.Value.Value}")) ? item.Value.Value.ToString() : ((item.Value.Type == Field.FieldTypes.RustColor) ? "1 1 1" : "#ffffff"));
					string text3 = ((item.Value.Type == Field.FieldTypes.RustColor) ? CUI.RustToHexColor(text2) : text2);
					string text4 = ((item.Value.Type == Field.FieldTypes.HexColor) ? CUI.HexToRustColor(text2) : text2);
					string[] array = text4.Split(' ');
					text4 = string.Format("R:{0:0}   G:{1:0}   B:{2:0}   A:{3:0}", new object[4]
					{
						array[0].ToFloat() * 255f,
						array[1].ToFloat() * 255f,
						array[2].ToFloat() * 255f,
						((array.Length == 4) ? array[3].ToFloat() : 1f) * 255f
					});
					Array.Clear(array, 0, array.Length);
					cui.CreateText(container, pair4, color, "<b>" + "HEX".SpacedString(1) + ":</b>  " + text3, 12, 0.7f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
					cui.CreateText(container, pair4, color, "<b>" + "RUST".SpacedString(1) + ":</b>  " + text4, 12, 0.115f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
					cui.CreateProtectedButton(container, pair4, text3, "0 0 0 0", string.Empty, 0, null, 0.025f, 0.085f, 0.1f, 0.9f, 0f, 0f, 0f, 0f, "modal.action " + item.Key, (TextAnchor)4);
					break;
				}
				case Field.FieldTypes.Enum:
				{
					EnumField enumField = item.Value as EnumField;
					cui.CreateText(container, pair4, color, enumField.Options[(enumField.Value != null) ? enumField.Value.ToString().ToInt() : 0], 12, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
					cui.CreateProtectedButton(container, pair4, "0.1 0.1 0.1 0.75", "1 1 1 0.7", "<", 10, null, 0f, 0.5f, 0f, 1f, 0f, 0f, 0f, 0f, "modal.action " + item.Key + " -", (TextAnchor)4);
					cui.CreateProtectedButton(container, pair4, "0.1 0.1 0.1 0.75", "1 1 1 0.7", ">", 10, null, 0.5f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "modal.action " + item.Key + " +", (TextAnchor)4);
					break;
				}
				case Field.FieldTypes.Button:
				{
					ButtonField buttonField = item.Value as ButtonField;
					cui.CreateProtectedButton(container, pair4, "0.1 0.1 0.1 0.85", "1 1 1 0.7", buttonField.ButtonName?.ToUpper().SpacedString(1), 10, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "modal.action " + item.Key, (TextAnchor)4);
					break;
				}
				}
				if (item.Value.IsReadOnly)
				{
					cui.CreatePanel(container, pair4, "0 0 0 0");
				}
				num -= num2;
			}
			CUI.Pair<string, CuiElement> pair6 = cui.CreatePanel(container, panel, "0 0 0 0", null, 0.075f, 0.925f, 0.025f, 0.1f);
			cui.CreateProtectedButton(container, pair6, "0.1 0.1 0.1 0.85", "1 1 1 0.7", "CANCEL".SpacedString(1), 10, null, 0.7f, 0.84f, 0f, 1f, 0f, 0f, 0f, 0f, "modal.cancel", (TextAnchor)4);
			cui.CreateProtectedButton(container, pair6, IsValid() ? CUI.HexToRustColor("#7ebf37", 0.6f) : "0.1 0.1 0.1 0.85", "1 1 1 0.7", "CONFIRM".SpacedString(1), 10, null, 0.85f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "modal.confirm", (TextAnchor)4);
			if (Pages > 0)
			{
				CUI.Pair<string, CuiElement> pair7 = cui.CreatePanel(container, panel, "0 0 0 0", null, 0.1f, 0.9f, 0.15f, 0.2f);
				cui.CreateText(container, pair7, "1 1 1 0.7", $"{Page + 1:n0} / {Pages + 1:n0}", 10, 0.82f, 0.92f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				cui.CreateProtectedButton(container, pair7, (Page == 0) ? "0.2 0.2 0.2 0.6" : CUI.HexToRustColor("#7ebf37", 0.6f), "1 1 1 0.7", "<", 10, null, 0.82f, 0.9f, 0f, 1f, -30f, -30f, 0f, 0f, "modal.page -", (TextAnchor)4);
				cui.CreateProtectedButton(container, pair7, (Page == Pages) ? "0.2 0.2 0.2 0.6" : CUI.HexToRustColor("#7ebf37", 0.6f), "1 1 1 0.7", ">", 10, null, 0.92f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "modal.page +", (TextAnchor)4);
			}
		}

		public T Get<T>(string key)
		{
			if (Fields.TryGetValue(key, out var value))
			{
				return (T)value.Value;
			}
			return default(T);
		}
	}

	public readonly CUI.Handler Handler = new CUI.Handler();

	public static ModalModule Instance { get; internal set; }

	public AdminModule Admin { get; internal set; }

	public ColorPickerModule ColorPicker { get; internal set; }

	public override string Name => "Modal";

	public override VersionNumber Version => new VersionNumber(1, 0, 0);

	public override Type Type => typeof(ModalModule);

	public override bool EnabledByDefault => true;

	public override bool ForceEnabled => true;

	public override bool InitEnd()
	{
		Instance = this;
		Admin = BaseModule.GetModule<AdminModule>();
		ColorPicker = BaseModule.GetModule<ColorPickerModule>();
		return base.InitEnd();
	}

	public Modal Open(BasePlayer player, string title, Dictionary<string, Modal.Field> fields, Action<BasePlayer, Modal> onConfirm = null, Action onCancel = null, Action<Modal, string, Modal.Field, object, object> onFieldChanged = null)
	{
		Modal modal = new Modal
		{
			Title = title,
			Fields = fields,
			OnCancel = onCancel,
			OnConfirm = onConfirm,
			Player = player,
			OnFieldChanged = onFieldChanged,
			Handler = new CUI.Handler()
		};
		NextFrame(delegate
		{
			modal.Draw(player);
		});
		return modal;
	}

	public void Close(BasePlayer player)
	{
		using CUI cUI = new CUI(Handler);
		cUI.Destroy("carbonmodalui", player);
	}

	[ProtectedCommand("modal.action")]
	private void ModalAction(Arg arg)
	{
		AdminModule.PlayerSession ap = Admin.GetPlayerSession(ArgEx.Player(arg));
		Modal modal = ap.GetStorage<Modal>(null, "modal");
		string fieldName = arg.GetString(0, "");
		Modal.Field field = modal.Fields[fieldName];
		object oldValue = field.Value;
		if (!field.IsReadOnly)
		{
			string value = arg.GetFullString(1);
			switch (field.Type)
			{
			case Modal.Field.FieldTypes.String:
				field.Value = value;
				modal.OnFieldChanged?.Invoke(modal, fieldName, field, oldValue, value);
				break;
			case Modal.Field.FieldTypes.Integer:
				field.Value = value.ToInt();
				modal.OnFieldChanged?.Invoke(modal, fieldName, field, oldValue, value);
				break;
			case Modal.Field.FieldTypes.ULong:
				field.Value = value.ToUlong(0uL);
				modal.OnFieldChanged?.Invoke(modal, fieldName, field, oldValue, value);
				break;
			case Modal.Field.FieldTypes.Float:
				field.Value = value.ToFloat();
				modal.OnFieldChanged?.Invoke(modal, fieldName, field, oldValue, value);
				break;
			case Modal.Field.FieldTypes.Boolean:
				field.Value = !value.ToBool();
				modal.OnFieldChanged?.Invoke(modal, fieldName, field, oldValue, value);
				break;
			case Modal.Field.FieldTypes.RustColor:
			case Modal.Field.FieldTypes.HexColor:
				Community.Runtime.Core.NextFrame(delegate
				{
					ColorPicker.Open(ap.Player, delegate(string hexColor, string rustColor, float alpha)
					{
						object value2 = field.Value;
						if (field.Type == Modal.Field.FieldTypes.RustColor)
						{
							field.Value = CUI.HexToRustColor("#" + hexColor, alpha);
						}
						else
						{
							field.Value = CUI.RustToHexColor(rustColor, alpha);
						}
						modal.OnFieldChanged?.Invoke(modal, fieldName, field, oldValue, value);
						modal.Draw(ap.Player);
					});
				});
				break;
			case Modal.Field.FieldTypes.Enum:
			{
				Modal.EnumField enumField = field as Modal.EnumField;
				int num = ((field.Value != null) ? field.Value.ToString().ToInt() : 0);
				string text = value;
				if (!(text == "+"))
				{
					if (text == "-")
					{
						num--;
						field.Value = num - 1;
					}
				}
				else
				{
					num++;
				}
				if (num > enumField.Options.Length - 1)
				{
					num = 0;
				}
				else if (num < 0)
				{
					num = enumField.Options.Length - 1;
				}
				field.Value = num;
				modal.OnFieldChanged?.Invoke(modal, fieldName, field, oldValue, field.Value);
				break;
			}
			case Modal.Field.FieldTypes.Button:
			{
				Modal.ButtonField buttonField = field as Modal.ButtonField;
				buttonField.Callback?.Invoke(modal);
				break;
			}
			}
		}
		modal.Draw(ap.Player);
	}

	[ProtectedCommand("modal.confirm")]
	private void ModalConfirm(Arg arg)
	{
		AdminModule.PlayerSession playerSession = Admin.GetPlayerSession(ArgEx.Player(arg));
		Modal storage = playerSession.GetStorage<Modal>(null, "modal");
		if (!storage.IsValid())
		{
			storage.Draw(playerSession.Player);
			return;
		}
		storage.OnConfirm?.Invoke(playerSession.Player, storage);
		Close(playerSession.Player);
	}

	[ProtectedCommand("modal.cancel")]
	private void ModalCancel(Arg arg)
	{
		AdminModule.PlayerSession playerSession = Admin.GetPlayerSession(ArgEx.Player(arg));
		playerSession.GetStorage<Modal>(null, "modal")?.OnCancel?.Invoke();
		Close(playerSession.Player);
	}

	[ProtectedCommand("modal.page")]
	private void ModalPage(Arg arg)
	{
		AdminModule.PlayerSession playerSession = Admin.GetPlayerSession(ArgEx.Player(arg));
		Modal storage = playerSession.GetStorage<Modal>(null, "modal");
		string text = arg.GetString(0, "");
		if (!(text == "+"))
		{
			if (text == "-")
			{
				storage.Page--;
			}
		}
		else
		{
			storage.Page++;
		}
		if (storage.Page > storage.Pages)
		{
			storage.Page = 0;
		}
		else if (storage.Page < 0)
		{
			storage.Page = storage.Pages;
		}
		storage.Draw(playerSession.Player);
	}

	public override object InternalCallHook(uint hook, object[] args)
	{
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		object obj = ((args?.Length > 0) ? args[0] : null);
		try
		{
			switch (hook)
			{
			case 104934377u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag4 = flag;
				Arg arg3 = ((!flag4) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag4)
				{
					ModalAction(arg3);
					return null;
				}
				break;
			}
			case 1752466352u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag3 = flag;
				Arg arg2 = ((!flag3) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag3)
				{
					ModalCancel(arg2);
					return null;
				}
				break;
			}
			case 3879514865u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag5 = flag;
				Arg arg4 = ((!flag5) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag5)
				{
					ModalConfirm(arg4);
					return null;
				}
				break;
			}
			case 4001517656u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag2 = flag;
				Arg arg = ((!flag2) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag2)
				{
					ModalPage(arg);
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

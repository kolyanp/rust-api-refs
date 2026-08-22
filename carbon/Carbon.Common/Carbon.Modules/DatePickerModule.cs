using System;
using Carbon.Base;
using Carbon.Components;
using Carbon.Extensions;
using Carbon.Pooling;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
using UnityEngine;
using UnityEngine.UI;

namespace Carbon.Modules;

public class DatePickerModule : CarbonModule<EmptyModuleConfig, EmptyModuleData>
{
	public readonly CUI.Handler Handler = new CUI.Handler();

	public const string OnDatePicked = "datepicker_ondatepicked";

	public const string PanelId = "carbonuidatepicker";

	public const string PanelCursorLockId = "carbonuidatepickercurlock";

	internal static int Year = DateTime.UtcNow.Year;

	internal static int Month = DateTime.UtcNow.Month;

	internal static int Day = DateTime.UtcNow.Day;

	internal static float AnimationLength = 0.005f;

	internal static float CurrentAnimation = 0f;

	internal static readonly string[] Months = new string[12]
	{
		"January", "February", "March", "April", "May", "June", "July", "August", "September", "October",
		"November", "December"
	};

	public AdminModule Admin { get; internal set; }

	public override string Name => "DatePicker";

	public override VersionNumber Version => new VersionNumber(1, 0, 0);

	public override Type Type => typeof(DatePickerModule);

	public override bool EnabledByDefault => true;

	public override bool ForceEnabled => true;

	public override bool InitEnd()
	{
		Admin = BaseModule.GetModule<AdminModule>();
		return base.InitEnd();
	}

	public void Open(BasePlayer player, Action<DateTime> onDatePicked)
	{
		AdminModule.PlayerSession playerSession = Admin.GetPlayerSession(player);
		if (!base.ModuleConfiguration.Enabled)
		{
			string empty = string.Empty;
			onDatePicked?.Invoke(default(DateTime));
		}
		else
		{
			DrawCursorLocker(player);
			Draw(player, onDatePicked);
		}
	}

	public void Close(BasePlayer player)
	{
		Handler.Destroy("carbonuidatepicker", player);
		Handler.Destroy("carbonuidatepickercurlock", player);
	}

	internal void Draw(BasePlayer player, Action<DateTime> onDatePicked)
	{
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null)
		{
			return;
		}
		AdminModule.PlayerSession playerSession = Admin.GetPlayerSession(player);
		playerSession.SetStorage(playerSession.SelectedTab, "datepicker_ondatepicked", onDatePicked);
		using CUI cUI = new CUI(Handler);
		CuiElementContainer container = cUI.CreateContainer("carbonuidatepicker", "0 0 0 0.75", 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, needsCursor: true, needsKeyboard: false, CUI.ClientPanels.Overlay, "carbonuidatepicker");
		CUI.Pair<string, CuiElement> pair = cUI.CreatePanel(container, "carbonuidatepicker", "0 0 0 0.6", null, 0.3f, 0.7f, 0.275f, 0.825f);
		CUI.Pair<string, CuiElement> pair2 = cUI.CreatePanel(container, pair, "0 0 0 0.5", null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, blur: true);
		cUI.CreateText(container, pair2, "1 1 1 0.8", "<b>Date Picker</b>", 18, 0f, 1f, 0.8f, 0.98f, 0f, 0f, 0f, 0f, (TextAnchor)1, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
		float num = 20f;
		float num2 = num * 0.77f;
		float num3 = num * 2f - 8f;
		Color blue = Color.blue;
		Color green = Color.green;
		Color red = Color.red;
		Color yellow = Color.yellow;
		CUI.Pair<string, CuiElement> pair3 = cUI.CreatePanel(container, pair2, "1 1 1 0.1", null, 0.15f, 0.85f, 0.86f, 0.92f);
		cUI.CreateProtectedInputField(container, pair3, "1 1 1 0.7", $"{Year}", 10, 4, readOnly: false, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "carbonuidatepicker.action yearchange ", (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, autoFocus: false, hudMenuInput: false, (LineType)0);
		cUI.CreateProtectedButton(container, pair3, "1 1 1 0.2", "1 1 1 0.7", "<", 8, null, 0f, 0.1f, 0f, 1f, 0f, 0f, 0f, 0f, "carbonuidatepicker.action yearappend -1", (TextAnchor)4);
		cUI.CreateProtectedButton(container, pair3, "1 1 1 0.2", "1 1 1 0.7", ">", 8, null, 0.9f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "carbonuidatepicker.action yearappend 1", (TextAnchor)4);
		CUI.Pair<string, CuiElement> pair4 = cUI.CreatePanel(container, pair2, "0 0 0 0", null, 0.15f, 0.85f, 0.71f, 0.85f);
		float num4 = 1f;
		float num5 = 0f;
		float num6 = 1f / (float)(Months.Length / 2);
		for (float num7 = 0f; num7 < (float)Months.Length; num7++)
		{
			cUI.CreateProtectedButton(container, pair4, $"1 1 1 {(((float)Month == num7 + 1f) ? 0.2 : 0.1)}", "1 1 1 0.7", Months[(int)num7], 8, null, num5, num5 + num6, num4 - 0.5f, num4, 0f, 0f, 0f, 0f, "carbonuidatepicker" + $".action monthchange {num7 + 1f}", (TextAnchor)4);
			num5 += num6;
			if (num7 == (float)((Months.Length - 1) / 2))
			{
				num5 = 0f;
				num4 = 0.5f;
			}
		}
		CUI.Pair<string, CuiElement> pair5 = cUI.CreatePanel(container, pair2, "0 0 0 0", null, 0.15f, 0.85f, 0.15f, 0.7f);
		int num8 = DateTime.DaysInMonth(Year, Month);
		float num9 = 0f;
		float num10 = 0.2f;
		float num11 = 1f / 7f;
		float num12 = 1f;
		for (float num13 = 0f; num13 < (float)num8; num13++)
		{
			cUI.CreateProtectedButton(container, pair5, $"1 1 1 {(((float)Day == num13 + 1f) ? 0.2 : 0.05)}", "1 1 1 0.7", $"{num13 + 1f}", 8, null, num9, num9 + num11, num12 - num10, num12, 0f, 0f, 0f, 0f, "carbonuidatepicker" + $".action daychange {num13 + 1f}", (TextAnchor)4);
			num9 += num11;
			if ((num13 + 1f) % 7f == 0f)
			{
				num9 = 0f;
				num12 -= num10;
			}
		}
		cUI.CreateProtectedButton(container, pair2, "0.2 0.6 0.2 0.5", "0.5 1 0.5 1", "NOW", 9, null, 0.9f, 0.95f, 0.95f, 0.99f, 0f, 0f, 0f, 0f, "carbonuidatepicker.action reset", (TextAnchor)4);
		cUI.CreateProtectedButton(container, pair2, "0.6 0.2 0.2 0.9", "1 0.5 0.5 1", "X", 8, null, 0.96f, 0.99f, 0.95f, 0.99f, 0f, 0f, 0f, 0f, "carbonuidatepicker.close", (TextAnchor)4, CUI.Handler.FontTypes.DroidSansMono);
		cUI.CreateProtectedButton(container, pair2, "0.3 1 0.3 0.2", "0.8 1 0.8 1", "CONFIRM".SpacedString(1), 8, null, 0.4f, 0.6f, 0.05f, 0.12f, 0f, 0f, 0f, 0f, "carbonuidatepicker.action confirm", (TextAnchor)4);
		cUI.Send(container, player);
		CurrentAnimation = 0f;
	}

	internal void DrawCursorLocker(BasePlayer player)
	{
		using CUI cUI = new CUI(Handler);
		CuiElementContainer container = cUI.CreateContainer("carbonuidatepickercurlock", "0 0 0 0", 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.005f, 0f, needsCursor: true);
		cUI.Send(container, player);
	}

	[ProtectedCommand("carbonuidatepicker.close")]
	private void CloseDatePickerUI(Arg args)
	{
		Close(ArgEx.Player(args));
	}

	[ProtectedCommand("carbonuidatepicker.action")]
	private void ActionDatePickerUI(Arg args)
	{
		BasePlayer player = ArgEx.Player(args);
		AdminModule.PlayerSession playerSession = Admin.GetPlayerSession(player);
		Action<DateTime> storage = playerSession.GetStorage<Action<DateTime>>(playerSession.SelectedTab, "datepicker_ondatepicked");
		switch (args.GetString(0, ""))
		{
		case "yearchange":
			Year = args.GetInt(1, DateTime.UtcNow.Year).Clamp(1977, 2100);
			break;
		case "monthchange":
			Month = args.GetInt(1, 1);
			break;
		case "daychange":
			Day = args.GetInt(1, 1);
			break;
		case "yearappend":
			Year += args.GetInt(1, 0);
			break;
		case "reset":
			Year = DateTime.UtcNow.Year;
			Month = DateTime.UtcNow.Month;
			Day = DateTime.UtcNow.Day;
			break;
		case "confirm":
			storage?.Invoke(new DateTime(Year, Month, Day));
			Close(player);
			return;
		}
		Draw(player, storage);
	}

	public override object InternalCallHook(uint hook, object[] args)
	{
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		int? num = args?.Length;
		object obj = ((num > 0) ? args[0] : null);
		object obj2 = ((num > 1) ? args[1] : null);
		try
		{
			switch (hook)
			{
			case 388755728u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag4 = flag;
				Arg args3 = ((!flag4) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag4)
				{
					ActionDatePickerUI(args3);
					return null;
				}
				break;
			}
			case 2637635139u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag3 = flag;
				Arg args2 = ((!flag3) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag3)
				{
					CloseDatePickerUI(args2);
					return null;
				}
				break;
			}
			case 3586261805u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag5 = flag;
				BasePlayer player2 = ((!flag5) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is Action<DateTime> || obj2 == null) ? true : false);
				bool flag6 = flag;
				Action<DateTime> onDatePicked = (flag6 ? ((Action<DateTime>)(obj2 ?? null)) : null);
				if (flag5 & flag6)
				{
					Draw(player2, onDatePicked);
					return null;
				}
				break;
			}
			case 3895172637u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag2 = flag;
				BasePlayer player = ((!flag2) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				if (flag2)
				{
					DrawCursorLocker(player);
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

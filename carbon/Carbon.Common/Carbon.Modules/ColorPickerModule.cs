using System;
using System.Linq;
using Carbon.Base;
using Carbon.Components;
using Carbon.Extensions;
using Carbon.Pooling;
using Facepunch;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
using UnityEngine;
using UnityEngine.UI;

namespace Carbon.Modules;

public class ColorPickerModule : CarbonModule<EmptyModuleConfig, EmptyModuleData>
{
	public readonly CUI.Handler Handler = new CUI.Handler();

	public const string Brightness = "colorpicker_brightness";

	public const string BrightnessIndicator = "colorpicker_brightnessindicator";

	public const string FirstOpen = "colorpicker_firstopen";

	public const string Alpha = "colorpicker_alpha";

	public const string OnColorPicked = "colorpicker_oncolorpicked";

	public const string PanelId = "carbonuicolorpicker";

	public const string PanelCursorLockId = "carbonuicolorpickercurlock";

	internal static float AnimationLength = 0.005f;

	internal static float CurrentAnimation = 0f;

	public AdminModule Admin { get; internal set; }

	public override string Name => "ColorPicker";

	public override VersionNumber Version => new VersionNumber(1, 0, 0);

	public override Type Type => typeof(ColorPickerModule);

	public override bool EnabledByDefault => true;

	public override bool ForceEnabled => true;

	public override bool InitEnd()
	{
		Admin = BaseModule.GetModule<AdminModule>();
		return base.InitEnd();
	}

	public void Open(BasePlayer player, Action<string, string, float> onColorPicked)
	{
		AdminModule.PlayerSession playerSession = Admin.GetPlayerSession(player);
		playerSession.SetStorage(playerSession.SelectedTab, "colorpicker_alpha", 1f);
		if (!base.ModuleConfiguration.Enabled)
		{
			string empty = string.Empty;
			onColorPicked?.Invoke(empty, empty, 1f);
		}
		else
		{
			DrawCursorLocker(player);
			Draw(player, onColorPicked);
			playerSession.SetStorage(playerSession.SelectedTab, "colorpicker_firstopen", value: true);
		}
	}

	public void Close(BasePlayer player)
	{
		AdminModule.PlayerSession playerSession = Admin.GetPlayerSession(player);
		Handler.Destroy("carbonuicolorpicker", player);
		Handler.Destroy("carbonuicolorpickercurlock", player);
		playerSession.SetStorage(playerSession.SelectedTab, "colorpicker_firstopen", value: false);
	}

	internal void Draw(BasePlayer player, Action<string, string, float> onColorPicked)
	{
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0527: Unknown result type (might be due to invalid IL or missing references)
		//IL_0529: Unknown result type (might be due to invalid IL or missing references)
		//IL_0543: Unknown result type (might be due to invalid IL or missing references)
		//IL_0548: Unknown result type (might be due to invalid IL or missing references)
		//IL_0556: Unknown result type (might be due to invalid IL or missing references)
		//IL_0558: Unknown result type (might be due to invalid IL or missing references)
		//IL_0575: Unknown result type (might be due to invalid IL or missing references)
		//IL_057a: Unknown result type (might be due to invalid IL or missing references)
		//IL_057c: Unknown result type (might be due to invalid IL or missing references)
		//IL_057e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0598: Unknown result type (might be due to invalid IL or missing references)
		//IL_059e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_061f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0624: Unknown result type (might be due to invalid IL or missing references)
		//IL_0641: Unknown result type (might be due to invalid IL or missing references)
		//IL_0646: Unknown result type (might be due to invalid IL or missing references)
		//IL_064f: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e6: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null)
		{
			return;
		}
		AdminModule.PlayerSession playerSession = Admin.GetPlayerSession(player);
		playerSession.SetStorage(playerSession.SelectedTab, "colorpicker_oncolorpicked", onColorPicked);
		float storage = playerSession.GetStorage(playerSession.SelectedTab, "colorpicker_alpha", 1f);
		float storage2 = playerSession.GetStorage(playerSession.SelectedTab, "colorpicker_brightness", 1f);
		bool storage3 = playerSession.GetStorage(playerSession.SelectedTab, "colorpicker_firstopen", @default: false);
		using CUI cui = new CUI(Handler);
		CuiElementContainer container = cui.CreateContainer("carbonuicolorpicker", "0 0 0 0.75", 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, needsCursor: true, needsKeyboard: false, CUI.ClientPanels.Overlay, "carbonuicolorpicker");
		CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, "carbonuicolorpicker", "0 0 0 0.6", null, 0.3f, 0.7f, 0.275f, 0.825f);
		CUI.Pair<string, CuiElement> pair2 = cui.CreatePanel(container, pair, "0 0 0 0.5", null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, blur: true);
		cui.CreateText(container, pair2, "1 1 1 0.8", "<b>Color Picker</b>", 18, 0f, 1f, 0.8f, 0.98f, 0f, 0f, 0f, 0f, (TextAnchor)1, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
		float num = 20f;
		float num2 = num * 0.77f;
		float oldMax = num * 2f - 8f;
		Color blue = Color.blue;
		Color green = Color.green;
		Color red = Color.red;
		Color yellow = Color.yellow;
		cui.CreateText(container, pair2, "1 1 1 0.3", "------------------------------------------------------------------------------------------------------------------------------------- BRIGHTNESS", 8, 0f, 0.775f, 0.01f, 0.98f, 0f, 0f, 0f, 0f, (TextAnchor)8, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
		cui.CreateText(container, pair2, "1 1 1 0.3", "SHADES ---------------", 8, 0.805f, 1f, 0.085f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)6, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
		cui.CreateText(container, pair2, "1 1 1 0.3", "------------------- ALPHA", 8, 0f, 0.14f, 0.085f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)8, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
		CUI.Pair<string, CuiElement> pair3 = cui.CreatePanel(container, pair2, "0.1 0.1 0.1 0.5", null, 0.805f, 0.94f, 0.085f, 0.15f, 0f, 0f, -30f, -30f);
		cui.CreateProtectedInputField(container, pair3, "1 1 1 1", "#", 10, 0, readOnly: false, 0.075f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "carbonuicolorpicker.pickhexcolor", (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, autoFocus: false, hudMenuInput: false, (LineType)0, 0f, 0f, needsCursor: false, needsKeyboard: true);
		CUI.Pair<string, CuiElement> pair4 = cui.CreatePanel(container, pair2, "0.1 0.1 0.1 0.5", null, 0.015f, 0.14f, 0.085f, 0.15f, 0f, 0f, -30f, -30f);
		cui.CreateProtectedInputField(container, pair4, "1 1 1 1", $"{storage}", 10, 0, readOnly: false, 0.075f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, "carbonuicolorpicker.pickalpha", (TextAnchor)5, CUI.Handler.FontTypes.RobotoCondensedRegular, autoFocus: false, hudMenuInput: false, (LineType)0, 0f, 0f, needsCursor: false, needsKeyboard: true);
		CUI.Pair<string, CuiElement> pair5 = cui.CreatePanel(container, pair2, "0 0 0 0", null, 0.175f, 0.8f, 0.1f, 0.9f, 0f, 0f, 0f, 0f, blur: false, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, "carbonuicolorpicker.picker");
		for (float num3 = 0f; num3 < num; num3 += 1f)
		{
			Color val = Color.Lerp(blue, green, num3.Scale(0f, num, 0f, 1f));
			for (float num4 = 0f; num4 < num; num4 += 1f)
			{
				Color val2 = Color.Lerp(red, yellow, (num4 + num3).Scale(0f, oldMax, 0f, 1f));
				Color color = Color.Lerp(val2, val, num4.Scale(0f, num, 0f, 1f)) * storage2;
				DrawColor(cui, container, playerSession, num, color, pair5, num2 * num4, 0f - num2 * num3, "color", (!storage3) ? CurrentAnimation : 0f);
				CurrentAnimation += AnimationLength;
			}
		}
		int num5 = 0;
		for (float num6 = 0f; num6 < num; num6 += 1f)
		{
			Color color2 = Color.Lerp(Color.black, Color.white, num6.Scale(0f, num, 0f, 1f));
			DrawColor(cui, container, playerSession, num, color2, pair5, num2 * num6, 0f - num2 * (num + 1f), "brightness", (!storage3) ? CurrentAnimation : 0f, num5);
			CurrentAnimation += AnimationLength;
			num5++;
		}
		for (float num7 = 0f; num7 < num; num7 += 1f)
		{
			Color color3 = Color.Lerp(Color.white, Color.black, num7.Scale(0f, num, 0f, 1f));
			DrawColor(cui, container, playerSession, num, color3, pair5, num2 * (num + 1f), 0f - num2 * num7, "color", (!storage3) ? CurrentAnimation : 0f);
			CurrentAnimation += AnimationLength;
		}
		cui.CreateProtectedButton(container, pair2, "0.6 0.2 0.2 0.9", "1 0.5 0.5 1", "X", 8, null, 0.96f, 0.99f, 0.95f, 0.99f, 0f, 0f, 0f, 0f, "carbonuicolorpicker.close", (TextAnchor)4, CUI.Handler.FontTypes.DroidSansMono);
		cui.Send(container, player);
		CurrentAnimation = 0f;
	}

	internal static void DrawColor(CUI cui, CuiElementContainer container, AdminModule.PlayerSession ap, float scale, Color color, string parent, float xOffset, float yOffset, string mode = "color", float fade = 0f, int index = -1)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f.Scale(0f, scale, 0f, 1f);
		CUI.Pair<string, CuiElement, CuiElement> pair = cui.CreateProtectedButton(color: $"{color.r} {color.g} {color.b} 1", text: string.Empty, yMin: num * (scale - 1f), xMax: 1f - num * (scale - 1f), OxMax: xOffset, OyMin: yOffset, OyMax: yOffset, fadeIn: fade, container: container, parent: parent, textColor: "0 0 0 0", size: 0, material: null, xMin: 0f, yMax: 1f, OxMin: xOffset, command: "carbonuicolorpicker" + string.Format(".pickcolor {0} {1} {2} {3} {4}", new object[5]
		{
			mode,
			ColorUtility.ToHtmlStringRGBA(color),
			color.r,
			color.g,
			color.b
		}), align: (TextAnchor)4);
		if (mode == "brightness" && index == ap.GetStorage(ap.SelectedTab, "colorpicker_brightnessindicator", 8))
		{
			cui.CreatePanel(container, pair, "0.75 0.75 0.2 0.8", null, 0f, 1f, 0.85f, 1f, 0f, 0f, 4f, 8.5f);
		}
	}

	internal void DrawCursorLocker(BasePlayer player)
	{
		using CUI cUI = new CUI(Handler);
		CuiElementContainer container = cUI.CreateContainer("carbonuicolorpickercurlock", "0 0 0 0", 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.005f, 0f, needsCursor: true);
		cUI.Send(container, player);
	}

	[ProtectedCommand("carbonuicolorpicker.close")]
	private void CloseColorPickerUI(Arg args)
	{
		Close(ArgEx.Player(args));
	}

	[ProtectedCommand("carbonuicolorpicker.pickcolor")]
	private unsafe void PickColorPickerUI(Arg arg)
	{
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		if (arg.Args != null)
		{
			BasePlayer player = ArgEx.Player(arg);
			AdminModule.PlayerSession playerSession = Admin.GetPlayerSession(player);
			string text = arg.GetString(0, "");
			string text2 = arg.GetString(1, "");
			float storage = playerSession.GetStorage(playerSession.SelectedTab, "colorpicker_alpha", 1f);
			string arg2 = string.Join(" ", from x in arg.Args.Skip(2)
				select ((object)(*(StringView*)(&x))/*cast due to constrained. prefix*/).ToString());
			Color val = default(Color);
			ColorUtility.TryParseHtmlString("#" + text2, ref val);
			float storage2 = playerSession.GetStorage(playerSession.SelectedTab, "colorpicker_brightness", 1f);
			int storage3 = playerSession.GetStorage(playerSession.SelectedTab, "colorpicker_brightnessindicator", 8);
			Action<string, string, float> storage4 = playerSession.GetStorage<Action<string, string, float>>(playerSession.SelectedTab, "colorpicker_oncolorpicked");
			if (text == "brightness")
			{
				playerSession.SetStorage(playerSession.SelectedTab, "colorpicker_brightness", val.r.Scale(0f, 1f, 0f, 2.5f));
				playerSession.SetStorage(playerSession.SelectedTab, "colorpicker_brightnessindicator", (int)val.r.Scale(0f, 1f, 0f, 20.5f));
				playerSession.SetStorage(playerSession.SelectedTab, "colorpicker_firstopen", value: true);
				Draw(player, storage4);
			}
			else
			{
				storage4?.Invoke(text2, arg2, storage);
				Close(ArgEx.Player(arg));
			}
		}
	}

	[ProtectedCommand("carbonuicolorpicker.pickhexcolor")]
	private void PickHexColorPickerUI(Arg arg)
	{
		BasePlayer player = ArgEx.Player(arg);
		AdminModule.PlayerSession playerSession = Admin.GetPlayerSession(player);
		string text = arg.GetString(0, "");
		float storage = playerSession.GetStorage(playerSession.SelectedTab, "colorpicker_alpha", 1f);
		if (arg.Args != null && arg.Args.Length != 0 && !string.IsNullOrEmpty(text) && !(text == "#"))
		{
			Action<string, string, float> storage2 = playerSession.GetStorage<Action<string, string, float>>(playerSession.SelectedTab, "colorpicker_oncolorpicked");
			if (!text.StartsWith("#"))
			{
				text = "#" + text;
			}
			string arg2 = CUI.HexToRustColor(text);
			storage2?.Invoke(text, arg2, storage);
			Close(ArgEx.Player(arg));
		}
	}

	[ProtectedCommand("carbonuicolorpicker.pickalpha")]
	private void PickAlphaColorPickerUI(Arg args)
	{
		BasePlayer player = ArgEx.Player(args);
		AdminModule.PlayerSession playerSession = Admin.GetPlayerSession(player);
		float value = args.GetFloat(0, 0f).Clamp(0f, 1f);
		playerSession.SetStorage(playerSession.SelectedTab, "colorpicker_alpha", value);
	}

	public override object InternalCallHook(uint hook, object[] args)
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		int? num = args?.Length;
		object obj = ((num > 0) ? args[0] : null);
		object obj2 = ((num > 1) ? args[1] : null);
		try
		{
			switch (hook)
			{
			case 3488114070u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag7 = flag;
				Arg args3 = ((!flag7) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag7)
				{
					CloseColorPickerUI(args3);
					return null;
				}
				break;
			}
			case 3586261805u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag3 = flag;
				BasePlayer player = ((!flag3) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is Action<string, string, float> || obj2 == null) ? true : false);
				bool flag4 = flag;
				Action<string, string, float> onColorPicked = (flag4 ? ((Action<string, string, float>)(obj2 ?? null)) : null);
				if (flag3 && flag4)
				{
					Draw(player, onColorPicked);
					return null;
				}
				break;
			}
			case 3895172637u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag5 = flag;
				BasePlayer player2 = ((!flag5) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				if (flag5)
				{
					DrawCursorLocker(player2);
					return null;
				}
				break;
			}
			case 877006401u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag6 = flag;
				Arg args2 = ((!flag6) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag6)
				{
					PickAlphaColorPickerUI(args2);
					return null;
				}
				break;
			}
			case 2210168706u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag8 = flag;
				Arg arg2 = ((!flag8) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag8)
				{
					PickColorPickerUI(arg2);
					return null;
				}
				break;
			}
			case 1750458046u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag2 = flag;
				Arg arg = ((!flag2) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag2)
				{
					PickHexColorPickerUI(arg);
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

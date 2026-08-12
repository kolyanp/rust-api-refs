using System;
using Rust.UI;
using UnityEngine;

public class UICameraOverlay : SingletonComponent<UICameraOverlay>
{
	public static readonly Phrase FocusOffText;

	public static readonly Phrase FocusAutoText;

	public static readonly Phrase FocusManualText;

	public static readonly Phrase FlashOn;

	public static readonly Phrase FlashOff;

	public Canvas Canvas;

	public CanvasGroup CanvasGroup;

	public RustText FocusModeLabel;

	public RustText FlashLabel;

	protected override void Awake()
	{
		base.Awake();
		Hide();
	}

	public void Show()
	{
		if ((Object)(object)Canvas != (Object)null)
		{
			((Behaviour)Canvas).enabled = true;
		}
		CanvasGroup.alpha = 1f;
	}

	public void Hide()
	{
		if ((Object)(object)Canvas != (Object)null)
		{
			((Behaviour)Canvas).enabled = false;
		}
		CanvasGroup.alpha = 0f;
	}

	public void SetFlash(bool flashEnabled)
	{
		FlashLabel.SetPhrase(flashEnabled ? FlashOn : FlashOff, Array.Empty<object>());
	}

	public void SetFocusMode(CameraFocusMode mode)
	{
		switch (mode)
		{
		case CameraFocusMode.Auto:
			FocusModeLabel.SetPhrase(FocusAutoText, Array.Empty<object>());
			break;
		case CameraFocusMode.Manual:
			FocusModeLabel.SetPhrase(FocusManualText, Array.Empty<object>());
			break;
		default:
			FocusModeLabel.SetPhrase(FocusOffText, Array.Empty<object>());
			break;
		}
	}

	static UICameraOverlay()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		FocusOffText = new Phrase("camera.infinite_focus", "Infinite Focus");
		FocusAutoText = new Phrase("camera.auto_focus", "Auto Focus");
		FocusManualText = new Phrase("camera.manual_focus", "Manual Focus");
		FlashOn = new Phrase("camera.flash_is_on", "Flash [ON]");
		FlashOff = new Phrase("camera.flash_is_off", "Flash [OFF]");
	}
}

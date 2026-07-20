using System;
using Rust.UI;
using UnityEngine;

public class UICameraOverlay : SingletonComponent<UICameraOverlay>
{
	public static readonly Phrase FocusOffText = new Phrase("camera.infinite_focus", "Infinite Focus");

	public static readonly Phrase FocusAutoText = new Phrase("camera.auto_focus", "Auto Focus");

	public static readonly Phrase FocusManualText = new Phrase("camera.manual_focus", "Manual Focus");

	public static readonly Phrase FlashOn = new Phrase("camera.flash_is_on", "Flash [ON]");

	public static readonly Phrase FlashOff = new Phrase("camera.flash_is_off", "Flash [OFF]");

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
}

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rust.UI.MainMenu;

public class UI_ServerBrowser_RefreshButton : RustButton
{
	[SerializeField]
	private UI_LoadingRotate loadingRotate;

	[SerializeField]
	private Image refreshOverview;

	[SerializeField]
	private RustText text;

	private Phrase _refreshPhrase = new Phrase("serverbrowser.refresh", "Refresh");

	private Phrase _cancelPhrase = new Phrase("serverbrowser.cancel", "Cancel");

	public void SetRefreshState(bool state)
	{
		if (!((Object)(object)loadingRotate == (Object)null))
		{
			if (state)
			{
				loadingRotate.ContinuouslyRotate(state: true);
				((RustButton)this).SetToggleVisualOn();
				text.SetPhrase(_cancelPhrase, Array.Empty<object>());
			}
			else
			{
				loadingRotate.Reset();
				loadingRotate.ContinuouslyRotate(state: false);
				((RustButton)this).SetToggleVisualOff();
				text.SetPhrase(_refreshPhrase, Array.Empty<object>());
			}
		}
	}
}

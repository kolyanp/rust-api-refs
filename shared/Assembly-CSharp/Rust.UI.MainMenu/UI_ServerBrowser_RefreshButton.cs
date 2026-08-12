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

	private Phrase _refreshPhrase;

	private Phrase _cancelPhrase;

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

	public UI_ServerBrowser_RefreshButton()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		_refreshPhrase = new Phrase("serverbrowser.refresh", "Refresh");
		_cancelPhrase = new Phrase("serverbrowser.cancel", "Cancel");
		((RustButton)this)._002Ector();
	}
}

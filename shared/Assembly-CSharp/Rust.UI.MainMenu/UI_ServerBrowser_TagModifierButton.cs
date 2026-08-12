using TMPro;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_ServerBrowser_TagModifierButton : RustButton
{
	[SerializeField]
	[Header("Tag Modifier Button")]
	private string _serverTag;

	[SerializeField]
	private RustText _countText;

	[SerializeField]
	private bool _secureTag;

	private string _tag;

	private LTDescr _countTween;

	public string ServerTag => _serverTag;

	public bool IsSecureTag => _secureTag;

	public string CompactTag
	{
		get
		{
			if (_tag == null)
			{
				_tag = ServerTagCompressor.ShortenTag(_serverTag);
			}
			return _tag;
		}
	}

	public void SetCount(int count)
	{
		if (!((Object)(object)_countText == (Object)null))
		{
			if (_countTween != null)
			{
				LeanTween.cancel(_countTween.id);
			}
			int.TryParse(((TMP_Text)_countText).text, out var result);
			_countTween = LeanTween.value(((Component)this).gameObject, (float)result, (float)count, 0.2f).setEaseOutQuad().setOnUpdate(delegate(float val)
			{
				int num = Mathf.RoundToInt(val);
				((TMP_Text)_countText).text = num.ToString();
			});
		}
	}
}

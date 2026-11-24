using Facepunch.Flexbox;
using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_Hero_InfoBox : FacepunchBehaviour
{
	[Header("References")]
	[SerializeField]
	protected RustText _title;

	[SerializeField]
	protected RustText _subtitle;

	[SerializeField]
	protected RustText _tag;

	[SerializeField]
	protected CoverImage _coverImage;

	[SerializeField]
	protected RustButton _button;

	[SerializeField]
	protected FlexTransition _hoverFlex;

	[SerializeField]
	protected Image _blackout;

	[Header("Settings")]
	[SerializeField]
	private Phrase _titlePhrase;

	[SerializeField]
	private Phrase _subtitlePhrase;

	[SerializeField]
	protected Texture _image;

	[SerializeField]
	protected bool _internalLink;

	[SerializeField]
	protected string _linkUrl;
}

using Facepunch.Flexbox;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_ServerEntry : FacepunchBehaviour
{
	[SerializeField]
	[Header("References")]
	private RustButton _button;

	[SerializeField]
	private RustText _serverNameText;

	[SerializeField]
	private RustText _modeText;

	[SerializeField]
	private RustText _playerCurrentText;

	[SerializeField]
	private RustText _playerMaxText;

	[SerializeField]
	private RustText _regionText;

	[SerializeField]
	private RustText _mapTypeText;

	[SerializeField]
	private ServerBrowserTagList _tagController;

	[SerializeField]
	private GameObject _favouritingParticlesPrefab;

	[SerializeField]
	private Tooltip _distanceTooltip;

	[SerializeField]
	[Header("References - Last Played")]
	private RustText _lastPlayedText;

	[SerializeField]
	private GameObject _lastPlayedObject;

	[SerializeField]
	[Header("References - Friends")]
	private RustText _friendsText;

	[SerializeField]
	private GameObject _friendsObject;

	[SerializeField]
	private Tooltip _friendsTooltip;

	[SerializeField]
	[Header("References - Queue")]
	private RustText _queueText;

	[SerializeField]
	private GameObject _queueObject;

	[SerializeField]
	[Header("References - Favourites")]
	private RustButton _favouritesButton;

	[SerializeField]
	private FlexTransition _favouritesTransition;

	[SerializeField]
	private RectTransform _favouritesSpawnPoint;

	[SerializeField]
	[Header("References - Styles")]
	private StyleAsset _evenStyle;

	[SerializeField]
	private StyleAsset _oddStyle;

	[SerializeField]
	private StyleAsset _favouriteStyle;

	[SerializeField]
	[Header("Other")]
	private bool _joinOnClick;
}

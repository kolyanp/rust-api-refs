using Facepunch.Flexbox;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_ServerEntry : FacepunchBehaviour
{
	[Header("References")]
	[SerializeField]
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

	[Header("References - Last Played")]
	[SerializeField]
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

	[Header("References - Queue")]
	[SerializeField]
	private RustText _queueText;

	[SerializeField]
	private GameObject _queueObject;

	[Header("References - Favourites")]
	[SerializeField]
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

	[Header("Other")]
	[SerializeField]
	private bool _joinOnClick;
}

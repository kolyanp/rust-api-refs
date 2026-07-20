using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_ConnectModal : UI_Window
{
	[SerializeField]
	[Header("References")]
	private RustText _title;

	[SerializeField]
	private RustText _description;

	[SerializeField]
	private HttpImage _headerImage;

	[SerializeField]
	private GameObject _headerImageLoading;

	[SerializeField]
	private ServerBrowserTagList _tagController;

	[SerializeField]
	private RustText _mapTypeText;

	[SerializeField]
	private RustButton _websiteButton;

	[SerializeField]
	private Tooltip _websiteTooltip;

	[SerializeField]
	private GameObject _descriptionLoading;

	[SerializeField]
	private GameObject _connectToServerButton;

	[SerializeField]
	private GameObject _needsPremiumButton;

	[SerializeField]
	private GameObject _mapButton;

	[SerializeField]
	private UI_ServerMap _map;

	[Header("References - System Config")]
	[SerializeField]
	private GameObject _requiredSystemConfigSection;

	[SerializeField]
	private UI_TagToggle _tpmCheck;

	[SerializeField]
	private UI_TagToggle _secureBootCheck;

	[SerializeField]
	private UI_TagToggle _kernelCodeIntegrityCheck;

	[SerializeField]
	private UI_TagToggle _iommuCheck;

	[Header("References - Friends")]
	[SerializeField]
	private RustText _friendsText;

	[SerializeField]
	private GameObject _friendsObject;

	[SerializeField]
	private Tooltip _friendsTooltip;

	[Header("Info Box References")]
	[SerializeField]
	private RustText _playerCount;

	[SerializeField]
	private RustText _pingText;

	[SerializeField]
	private RustText _regionText;

	[SerializeField]
	private GameObject _queuedPlayersObject;

	[SerializeField]
	private RustText _queuedPlayersCount;

	[SerializeField]
	private GameObject _lastPlayedObject;

	[SerializeField]
	private RustText _lastPlayedText;

	[SerializeField]
	private GameObject _wipedObject;

	[SerializeField]
	private RustText _wipedText;

	[SerializeField]
	private ScrollRect _scrollRect;

	[SerializeField]
	private RectMask2D _scrollMask;

	public static Phrase lastPlayedPhrase = new Phrase("connection.modal.lastplayed.ago", "{0} ago");

	public static Phrase serverAgePhrase = new Phrase("connection.modal.serverage.old", "{0} old");

	public static Phrase loadingError = new Phrase("connection.modal.error", "Error loading server");
}

using System.Collections.Generic;
using Facepunch.Flexbox;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_ServerBrowserPage : UI_Page
{
	[SerializeField]
	private UI_ServerBrowser_RefreshButton _refreshButton;

	[SerializeField]
	private UI_ConnectModal _connectModal;

	[SerializeField]
	private UI_Window _shockbyteWindow;

	[Header("Categories")]
	[SerializeField]
	private List<Rust.UI.MainMenu.ServerBrowserCategoryData> _categories;

	[SerializeField]
	[Header("Headers")]
	private List<Rust.UI.MainMenu.ServerBrowserHeader> _headers;

	[SerializeField]
	[Header("Filters")]
	private RustButton _showEmptyToggle;

	[SerializeField]
	private RustButton _showFullToggle;

	[SerializeField]
	private RustButton _prioritiseFavouritesToggle;

	[SerializeField]
	private RustButton _prioritisePremiumToggle;

	[SerializeField]
	private RustButton _prioritiseSecureToggle;

	[SerializeField]
	private RustButton _useCacheToggle;

	[SerializeField]
	private RustInput _searchInput;

	[Header("No Results")]
	[SerializeField]
	private UI_ServerBrowser_NoResults_Controller _noResultsController;

	[Header("Other")]
	[SerializeField]
	private FlexTransition _favouritesButtonAnimation;

	[SerializeField]
	private GameObject _loadingSpinner;
}

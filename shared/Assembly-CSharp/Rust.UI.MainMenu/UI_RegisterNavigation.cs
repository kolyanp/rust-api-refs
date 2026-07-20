using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_RegisterNavigation : MonoBehaviour
{
	public UI_MainMenuNavigationEntry NavigationEntry;

	public void Setup()
	{
		if (NavigationEntry != null)
		{
			SetupEntry();
		}
	}

	private void SetupEntry()
	{
		UI_Page page = default(UI_Page);
		if (!((Object)(object)NavigationEntry.Reference == (Object)null) && NavigationEntry.Reference.TryGetComponent<UI_Page>(ref page))
		{
			NavigationEntry.Page = page;
		}
	}
}

using Development.Attributes;
using Rust.UI.MainMenu;
using UnityEngine;

[ResetStaticFields]
public static class MainMenu
{
	private static Canvas _canvas;

	public static Canvas Canvas
	{
		get
		{
			if ((Object)(object)SingletonComponent<UI_MainMenuManager>.Instance == (Object)null)
			{
				return null;
			}
			if ((Object)(object)_canvas == (Object)null)
			{
				_canvas = ((Component)SingletonComponent<UI_MainMenuManager>.Instance).GetComponent<Canvas>();
			}
			return _canvas;
		}
	}

	public static bool IsOpen()
	{
		return UI_MainMenuManager.IsOpen;
	}

	public static bool IsLoaded()
	{
		return UI_MainMenuManager.IsLoaded;
	}
}

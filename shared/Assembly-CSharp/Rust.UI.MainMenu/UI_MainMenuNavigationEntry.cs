using System;
using UnityEngine;

namespace Rust.UI.MainMenu;

[Serializable]
public class UI_MainMenuNavigationEntry
{
	public string Path;

	public GameObject Reference;

	[NonSerialized]
	public UI_Page Page;

	public void Hide()
	{
		if (CheckReference() && (Object)(object)Page != (Object)null)
		{
			Page.Close();
		}
	}

	public void Show()
	{
		if (CheckReference() && (Object)(object)Page != (Object)null)
		{
			Page.Open();
		}
	}

	private bool CheckReference()
	{
		if ((Object)(object)Reference == (Object)null)
		{
			Debug.LogError((object)("Navigation Entry '" + Path + "' doesn't have a valid reference."));
			return false;
		}
		return true;
	}
}

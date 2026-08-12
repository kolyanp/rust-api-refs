using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_MenuNavigationGroup : UI_RustButtonGroup
{
	[Serializable]
	private class TabButton
	{
		public RustButton Button;

		public string Path;
	}

	[SerializeField]
	[Header("Navigation Groups (IGNORE BUTTON GROUPS - Just Add Here)")]
	private List<TabButton> _navigationGroups = new List<TabButton>();
}

using System.Collections.Generic;
using UnityEngine;

namespace Carbon.Modules;

public class AdminData
{
	public class DataColors
	{
		public string SelectedTabColor = "0.4 0.7 0.2";

		public string EditableInputHighlight = "0.259 0.529 0.961";

		public string NameTextColor = "1 1 1 0.7";

		public string ButtonSelectedColor = "0.4 0.7 0.2 0.75";

		public string ButtonWarnedColor = "0.8 0.7 0.2 0.75";

		public string ButtonImportantColor = "0.97 0.2 0.1 0.75";

		public string OptionColor = "0.2 0.2 0.2 0.75";

		public string OptionColor2 = "0.2 0.2 0.2 0.1";

		public string OptionNameColor = "1 1 1 0.7";

		public float TitleUnderlineOpacity = 0.9f;

		public float OptionWidth = 0.475f;
	}

	public bool GreetDisplayed;

	public bool HidePluginIcons;

	public bool DisableUMod = true;

	public bool Maximize;

	public bool BackgroundBlur = true;

	public float BackgroundOpacity = 0.75f;

	public float BackgroundImageOpacity = 0.75f;

	public string BackgroundImage = "https://cdn.carbonmod.gg/content/carbon-background.png";

	public Vector2 BackgroundImageYAnchor = new Vector2(0.15f, 1f);

	public float BackgroundColumnOpacity = 0.5f;

	public DataColors Colors = new DataColors();

	public Dictionary<string, bool> TabsHiddenStatus = new Dictionary<string, bool>();

	public bool IsTabHidden(string id)
	{
		bool value;
		return TabsHiddenStatus.TryGetValue(id, out value) && value;
	}

	public void MarkTabHidden(string id, bool wants)
	{
		TabsHiddenStatus[id] = wants;
	}
}

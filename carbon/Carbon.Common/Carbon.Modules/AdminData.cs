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

	public bool DisableUMod;

	public bool Maximize;

	public bool BackgroundBlur;

	public float BackgroundOpacity;

	public float BackgroundImageOpacity;

	public string BackgroundImage;

	public Vector2 BackgroundImageYAnchor;

	public float BackgroundColumnOpacity;

	public DataColors Colors;

	public Dictionary<string, bool> TabsHiddenStatus;

	public bool IsTabHidden(string id)
	{
		bool value;
		return TabsHiddenStatus.TryGetValue(id, out value) & value;
	}

	public void MarkTabHidden(string id, bool wants)
	{
		TabsHiddenStatus[id] = wants;
	}

	public AdminData()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		DisableUMod = true;
		BackgroundBlur = true;
		BackgroundOpacity = 0.75f;
		BackgroundImageOpacity = 0.75f;
		BackgroundImage = "https://cdn.carbonmod.gg/content/carbon-background.png";
		BackgroundImageYAnchor = new Vector2(0.15f, 1f);
		BackgroundColumnOpacity = 0.5f;
		Colors = new DataColors();
		TabsHiddenStatus = new Dictionary<string, bool>();
		base._002Ector();
	}
}

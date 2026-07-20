namespace Carbon.Components;

public class LuiButtonComp : LuiCompBase
{
	public string command;

	public string close;

	public string sprite;

	public string material;

	public string color;

	public string imageType;

	public string normalColor;

	public string highlightedColor;

	public string pressedColor;

	public string selectedColor;

	public string disabledColor;

	public float colorMultiplier = -1f;

	public float fadeDuration = -1f;

	public LuiButtonComp()
	{
		type = LuiCompType.Button;
	}
}

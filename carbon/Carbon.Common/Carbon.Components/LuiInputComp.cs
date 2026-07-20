namespace Carbon.Components;

public class LuiInputComp : LuiCompBase
{
	public int fontSize;

	public string font;

	public string align;

	public string color;

	public int characterLimit;

	public string command;

	public string lineType;

	public string text;

	public bool readOnly;

	public string placeholderId;

	public bool password;

	public bool needsKeyboard;

	public bool hudMenuInput;

	public bool autofocus;

	public LuiInputComp()
	{
		type = LuiCompType.InputField;
	}
}

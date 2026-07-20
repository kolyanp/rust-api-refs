namespace Carbon.Components;

public class LuiTextComp : LuiCompBase
{
	public string text;

	public int fontSize;

	public string font;

	public string align;

	public string color;

	public string verticalOverflow;

	public LuiTextComp()
	{
		type = LuiCompType.Text;
	}
}

namespace Carbon.Components;

public class LuiLayoutElementComp : LuiCompBase
{
	public float preferredWidth = -1f;

	public float preferredHeight = -1f;

	public float minWidth;

	public float minHeight;

	public float flexibleWidth;

	public float flexibleHeight;

	public bool ignoreLayout;

	public LuiLayoutElementComp()
	{
		type = LuiCompType.LayoutElement;
	}
}

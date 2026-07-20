namespace Carbon.Components;

public class LuiVerticalLayoutGroupComp : LuiCompBase
{
	public float spacing;

	public string childAlignment;

	public bool childForceExpandWidth = true;

	public bool childForceExpandHeight = true;

	public bool childControlWidth;

	public bool childControlHeight;

	public bool childScaleWidth;

	public bool childScaleHeight;

	public string padding;

	public LuiVerticalLayoutGroupComp()
	{
		type = LuiCompType.VerticalLayoutGroup;
	}
}

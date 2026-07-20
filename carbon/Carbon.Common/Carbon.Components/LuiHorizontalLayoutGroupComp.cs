namespace Carbon.Components;

public class LuiHorizontalLayoutGroupComp : LuiCompBase
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

	public LuiHorizontalLayoutGroupComp()
	{
		type = LuiCompType.HorizontalLayoutGroup;
	}
}

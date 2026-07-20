namespace Carbon.Components;

public class LuiRectTransformComp : LuiCompBase
{
	public LuiPosition anchor = LuiPosition.Full;

	public LuiOffset offset = LuiOffset.None;

	public float rotation;

	public string setParent;

	public int setTransformIndex = -1;

	public LuiRectTransformComp()
	{
		type = LuiCompType.RectTransform;
	}
}

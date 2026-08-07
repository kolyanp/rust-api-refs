namespace Carbon.Components;

public class LuiImageComp : LuiCompBase
{
	public string sprite;

	public string material;

	public string color;

	public string imageType;

	public bool fillCenter;

	public string png;

	public string slice;

	public int itemid;

	public ulong skinid;

	public float ppuMultiplier = -1f;

	public LuiImageComp()
	{
		type = LuiCompType.Image;
	}
}

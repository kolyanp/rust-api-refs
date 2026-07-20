namespace Carbon.Components;

public class LuiCountdownComp : LuiCompBase
{
	public float endTime = -1f;

	public float startTime = -1f;

	public float step;

	public float interval;

	public string timerFormat;

	public string numberFormat;

	public bool destroyIfDone = true;

	public string command;

	public LuiCountdownComp()
	{
		type = LuiCompType.Countdown;
	}
}

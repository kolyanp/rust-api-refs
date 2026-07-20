using Network;

public struct RealTimeSinceEx
{
	private double time;

	public static implicit operator double(RealTimeSinceEx ts)
	{
		return TimeEx.realtimeSinceStartup - ts.time;
	}

	public static implicit operator RealTimeSinceEx(double ts)
	{
		return new RealTimeSinceEx
		{
			time = TimeEx.realtimeSinceStartup - ts
		};
	}

	public override string ToString()
	{
		return (TimeEx.realtimeSinceStartup - time).ToString();
	}
}

using Facepunch;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer.Handlers;

public class Time : BasePlayerHandler<AppEmpty>
{
	public override void Execute()
	{
		TOD_Sky instance = TOD_Sky.Instance;
		TOD_Time time = instance.Components.Time;
		AppTime val = Pool.Get<AppTime>();
		val.dayLengthMinutes = time.DayLengthInMinutes;
		val.timeScale = (time.ProgressTime ? Time.timeScale : 0f);
		val.sunrise = instance.SunriseTime;
		val.sunset = instance.SunsetTime;
		val.time = instance.Cycle.Hour;
		AppResponse val2 = Pool.Get<AppResponse>();
		val2.time = val;
		Send(val2);
	}
}

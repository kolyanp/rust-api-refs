using System;
using ConVar;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class EnvSync : PointEntity
{
	private const float syncInterval = 5f;

	private const float syncIntervalInv = 0.2f;

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRepeating(UpdateNetwork, 5f, 5f);
		if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance) && !World.LoadedFromSave)
		{
			SingletonComponent<Climate>.Instance.InitRainGracePeriod();
		}
	}

	private void UpdateNetwork()
	{
		if (NexusServer.Started && NexusServer.LastReset.HasValue && (Object)(object)TOD_Sky.Instance != (Object)null)
		{
			TOD_Time time = TOD_Sky.Instance.Components.Time;
			DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(NexusServer.LastReset.Value);
			double totalMinutes = (DateTimeOffset.UtcNow - dateTimeOffset).TotalMinutes;
			double num = (double)(Nexus.timeOffset / 24f) + totalMinutes / (double)time.DayLengthInMinutes;
			if (time.UseTimeCurve)
			{
				double num2 = Math.Truncate(num);
				double num3 = (num - num2) * 24.0;
				float num4 = time.EvaulateTimeCurve((float)num3);
				num = num2 + (double)(num4 / 24f);
			}
			double value = num * 24.0;
			DateTime dateTime = dateTimeOffset.Date.AddHours(value);
			TOD_Sky.Instance.Cycle.DateTime = dateTime;
		}
		SendNetworkUpdate();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.environment = Pool.Get<Environment>();
		if (Object.op_Implicit((Object)(object)TOD_Sky.Instance))
		{
			info.msg.environment.dateTime = TOD_Sky.Instance.Cycle.DateTime.ToBinary();
		}
		info.msg.environment.engineTime = Time.realtimeSinceStartup;
		if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
		{
			info.msg.environment.rainGraceStartTicks = SingletonComponent<Climate>.Instance.RainGraceStartTicks;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.environment != null && Object.op_Implicit((Object)(object)TOD_Sky.Instance) && base.isServer)
		{
			TOD_Sky.Instance.Cycle.DateTime = DateTime.FromBinary(info.msg.environment.dateTime);
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.RainGraceStartTicks = info.msg.environment.rainGraceStartTicks;
				SingletonComponent<Climate>.Instance.RestoreRainGracePeriod();
			}
		}
	}
}

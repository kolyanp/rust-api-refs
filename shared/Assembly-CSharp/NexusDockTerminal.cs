using System;
using System.Collections.Generic;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Nexus.Models;
using ProtoBuf;
using Rust.UI;
using UnityEngine;

public class NexusDockTerminal : BaseEntity
{
	public static readonly Phrase ScheduleSoonPhrase;

	public static readonly Phrase ScheduleMinutesPhrase;

	public static readonly Phrase ScheduleUnknownPhrase;

	public float TravelTime = 90f;

	public RustText[] ScheduleLabels;

	private List<ScheduleEntry> _scheduleEntries;

	private static readonly HashSet<string> SeenFerries;

	public override void InitShared()
	{
		base.InitShared();
		if (base.isServer)
		{
			InvokeRandomized(UpdateFerrySchedule, 0f, 10f, 5f);
		}
	}

	public override void AdminKill()
	{
		if (!HasFlag(Flags.Debugging))
		{
			Debug.LogWarning((object)"Prevented killing NexusDock, set debugging flag to override");
		}
	}

	private void UpdateFerrySchedule()
	{
		if (_scheduleEntries == null)
		{
			_scheduleEntries = Pool.Get<List<ScheduleEntry>>();
		}
		foreach (ScheduleEntry scheduleEntry in _scheduleEntries)
		{
			scheduleEntry.Dispose();
		}
		_scheduleEntries.Clear();
		List<(string, float?)> list = Pool.Get<List<(string, float?)>>();
		CalculateFerryEstimates(list);
		foreach (var item in list)
		{
			NexusZoneDetails val = NexusServer.FindZone(item.Item1);
			if (val != null)
			{
				ScheduleEntry val2 = Pool.Get<ScheduleEntry>();
				val2.nextZoneId = val.Id;
				val2.estimate = (int)Mathf.Round(item.Item2 ?? (-1f));
				_scheduleEntries.Add(val2);
			}
		}
		SendNetworkUpdate();
	}

	private void CalculateFerryEstimates(List<(string NextZone, float? Estimate)> estimates)
	{
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Invalid comparison between Unknown and I4
		if (estimates == null)
		{
			throw new ArgumentNullException("estimates");
		}
		estimates.Clear();
		SeenFerries.Clear();
		NexusDock instance = SingletonComponent<NexusDock>.Instance;
		if ((Object)(object)instance == (Object)null || !NexusServer.Started || NexusServer.Zones == null)
		{
			return;
		}
		instance.CleanupQueuedFerries();
		float num = 0f;
		if ((Object)(object)instance.CurrentFerry != (Object)null && !instance.CurrentFerry.IsRetiring)
		{
			estimates.Add((instance.CurrentFerry.ExpectedNextZone, num));
			SeenFerries.Add(instance.CurrentFerry.OwnerZone);
		}
		NexusFerry[] queuedFerries = instance.QueuedFerries;
		foreach (NexusFerry nexusFerry in queuedFerries)
		{
			if (!((Object)(object)nexusFerry == (Object)null) && !nexusFerry.IsRetiring)
			{
				estimates.Add((nexusFerry.ExpectedNextZone, num));
				num += instance.WaitTime;
				SeenFerries.Add(nexusFerry.OwnerZone);
			}
		}
		string zoneKey = NexusServer.ZoneKey;
		foreach (NexusZoneDetails zone in NexusServer.Zones)
		{
			if (SeenFerries.Contains(zone.Key) || !((Dictionary<string, VariableData>)(object)zone.Variables).TryGetValue("ferry", out VariableData value) || (int)((VariableData)(ref value)).Type != 1 || string.IsNullOrWhiteSpace(((VariableData)(ref value)).Value) || !((VariableData)(ref value)).Value.Contains(zoneKey, StringComparison.InvariantCultureIgnoreCase) || !NexusUtil.TryParseFerrySchedule(zone.Key, ((VariableData)(ref value)).Value, out var entries) || List.FindIndex<string>((IReadOnlyList<string>)entries, zoneKey, (IEqualityComparer<string>)StringComparer.InvariantCultureIgnoreCase) < 0)
			{
				continue;
			}
			if (!NexusFerry.TryPredictNextZone(entries, zoneKey, zone.Key, out var nextZoneKey))
			{
				NexusFerry.TryGetNextScheduledZone(entries, zoneKey, out nextZoneKey);
			}
			if (!NexusServer.TryGetFerryStatus(zone.Key, out var currentZone, out var status))
			{
				estimates.Add((nextZoneKey, null));
				SeenFerries.Add(zone.Key);
				continue;
			}
			if (status.isRetiring)
			{
				SeenFerries.Add(zone.Key);
				continue;
			}
			IReadOnlyList<string> readOnlyList2;
			if (status.schedule == null || status.schedule.Count <= 0)
			{
				IReadOnlyList<string> readOnlyList = entries;
				readOnlyList2 = readOnlyList;
			}
			else
			{
				IReadOnlyList<string> readOnlyList = status.schedule;
				readOnlyList2 = readOnlyList;
			}
			IReadOnlyList<string> schedule = readOnlyList2;
			if (NexusFerry.TryPredictNextZone(schedule, zoneKey, status.ownerZone, out var nextZoneKey2))
			{
				nextZoneKey = nextZoneKey2;
			}
			float? item = EstimateTimeUntilArrival(schedule, status.ownerZone, currentZone, (NexusFerry.State)status.state, zoneKey, num, instance.WaitTime);
			estimates.Add((nextZoneKey, item));
			SeenFerries.Add(zone.Key);
		}
		SeenFerries.Clear();
		estimates.Sort(delegate((string NextZone, float? Estimate) a, (string NextZone, float? Estimate) b)
		{
			int num2 = StringComparer.InvariantCultureIgnoreCase.Compare(a.NextZone, b.NextZone);
			if (num2 != 0)
			{
				return num2;
			}
			if (!a.Estimate.HasValue && !b.Estimate.HasValue)
			{
				return 0;
			}
			if (!a.Estimate.HasValue)
			{
				return 1;
			}
			return (!b.Estimate.HasValue) ? (-1) : a.Estimate.Value.CompareTo(b.Estimate.Value);
		});
	}

	private float? EstimateTimeUntilArrival(IReadOnlyList<string> schedule, string ownerZone, string currentZoneKey, NexusFerry.State state, string thisZoneKey, float queueTime, float waitTime)
	{
		if (schedule == null || schedule.Count == 0)
		{
			return null;
		}
		if (string.Equals(currentZoneKey, thisZoneKey, StringComparison.InvariantCultureIgnoreCase))
		{
			if (state == NexusFerry.State.SailingIn)
			{
				return queueTime + TravelTime;
			}
			if (state <= NexusFerry.State.Waiting)
			{
				return queueTime;
			}
		}
		float timeUntilDeparture = 0f;
		if (state <= NexusFerry.State.Stopping)
		{
			timeUntilDeparture += TravelTime;
		}
		if (state <= NexusFerry.State.Waiting)
		{
			timeUntilDeparture += waitTime;
		}
		if (state <= NexusFerry.State.SailingOut)
		{
			timeUntilDeparture += TravelTime;
		}
		return WalkRoute(useRouting: true) ?? WalkRoute(useRouting: false);
		float? WalkRoute(bool useRouting)
		{
			float num = timeUntilDeparture;
			string fromZoneKey = currentZoneKey;
			for (int i = 0; i < schedule.Count; i++)
			{
				if (!(useRouting ? NexusFerry.TryPredictNextZone(schedule, fromZoneKey, ownerZone, out var nextZoneKey) : NexusFerry.TryGetNextScheduledZone(schedule, fromZoneKey, out nextZoneKey)))
				{
					return null;
				}
				if (string.Equals(nextZoneKey, thisZoneKey, StringComparison.InvariantCultureIgnoreCase))
				{
					return num + TravelTime;
				}
				num += TravelTime + waitTime + TravelTime;
				fromZoneKey = nextZoneKey;
			}
			return null;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.nexusDockTerminal = Pool.Get<NexusDockTerminal>();
		info.msg.nexusDockTerminal.schedule = Pool.Get<List<ScheduleEntry>>();
		if (_scheduleEntries == null)
		{
			return;
		}
		foreach (ScheduleEntry scheduleEntry in _scheduleEntries)
		{
			info.msg.nexusDockTerminal.schedule.Add(scheduleEntry.Copy());
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.nexusDockTerminal?.schedule == null)
		{
			return;
		}
		if (_scheduleEntries != null)
		{
			foreach (ScheduleEntry scheduleEntry in _scheduleEntries)
			{
				scheduleEntry.Dispose();
			}
			Pool.Free<ScheduleEntry>(ref _scheduleEntries, false);
		}
		_scheduleEntries = info.msg.nexusDockTerminal.schedule;
		info.msg.nexusDockTerminal.schedule = null;
	}

	static NexusDockTerminal()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		ScheduleSoonPhrase = new Phrase("nexus.dock.schedule.soon", "{0} - Now");
		ScheduleMinutesPhrase = new Phrase("nexus.dock.schedule.minutes", "{0} - {1} min");
		ScheduleUnknownPhrase = new Phrase("nexus.dock.schedule.unknown", "{0} - Unknown");
		SeenFerries = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
	}
}

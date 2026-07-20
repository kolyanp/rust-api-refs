using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ConVar;

[Factory("events")]
public class Events : ConsoleSystem
{
	public class PendingEventChange
	{
		public string Id;

		public float? MinHours;

		public float? MaxHours;

		public bool? Enabled;

		public bool Invalid;

		public PendingEventChange(string id)
		{
			Id = id;
		}
	}

	private const string BradleyEventId = "bradley";

	private static List<PendingEventChange> TargetChanges = new List<PendingEventChange>();

	private static PendingEventChange GetPendingChanges(string id, bool create)
	{
		PendingEventChange pendingEventChange = TargetChanges.FirstOrDefault((PendingEventChange c) => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
		if (pendingEventChange == null && create)
		{
			pendingEventChange = new PendingEventChange(id);
			TargetChanges.Add(pendingEventChange);
		}
		return pendingEventChange;
	}

	private static void ApplyAllChanges()
	{
		foreach (PendingEventChange targetChange in TargetChanges)
		{
			UpdateServerEvent(targetChange);
		}
		TargetChanges.RemoveAll((PendingEventChange c) => c.Invalid);
	}

	public static void UpdateServerEvent(PendingEventChange targetState)
	{
		if (targetState.Id == "bradley")
		{
			Bradley.enabled = targetState.Enabled ?? Bradley.enabled;
			Bradley.respawnDelayMinutes = targetState.MinHours ?? targetState.MaxHours ?? Bradley.respawnDelayMinutes;
		}
		else if (HasAnyEvents())
		{
			EventSchedule eventSchedule = FindEventById(targetState.Id);
			if ((Object)(object)eventSchedule == (Object)null)
			{
				Debug.LogWarning((object)("Unknown event '" + targetState.Id + "' when applying settings to server events"));
				targetState.Invalid = true;
			}
			else
			{
				UpdateScheduleFromConVars(eventSchedule);
			}
		}
	}

	public static void UpdateScheduleFromConVars(EventSchedule schedule)
	{
		PendingEventChange pendingChanges = GetPendingChanges(schedule.Key, create: false);
		if (pendingChanges != null)
		{
			((Behaviour)schedule).enabled = pendingChanges.Enabled ?? ((Behaviour)schedule).enabled;
			schedule.minimumHoursBetween = pendingChanges.MinHours ?? schedule.minimumHoursBetween;
			schedule.maxmumHoursBetween = pendingChanges.MaxHours ?? schedule.maxmumHoursBetween;
		}
	}

	[ServerVar(Help = "(Generated) Prints a table of all registered server events with their current enabled state, minimum delay, maximum delay, and last trigger time")]
	public static void print_server_events(Arg arg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (EventSchedule allEvent in EventSchedule.allEvents)
		{
			if (string.IsNullOrEmpty(allEvent.Key))
			{
				stringBuilder.AppendLine("WARNING: Missing event key for prefab '" + ((Object)((Component)allEvent).gameObject).name + "'");
				continue;
			}
			stringBuilder.AppendLine(allEvent.Key ?? "");
			stringBuilder.AppendLine($"  - enabled: {((Behaviour)allEvent).enabled}");
			stringBuilder.AppendLine($"  - min hours: {allEvent.minimumHoursBetween}");
			stringBuilder.AppendLine($"  - max hours: {allEvent.maxmumHoursBetween}");
			stringBuilder.AppendLine();
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "(Generated) Enables or disables a specific server event by name; disabled events will not trigger automatically until re-enabled")]
	public static void set_event_enabled(Arg arg)
	{
		if (!arg.HasArgs(2))
		{
			arg.ReplyWith("Usage: set_event_enabled <event_name> <true|false>");
			return;
		}
		string text = arg.GetString(0);
		bool flag = arg.GetBool(1);
		GetPendingChanges(text, create: true).Enabled = flag;
		ApplyAllChanges();
		arg.ReplyWith((flag ? "Enabled" : "Disabled") + " event '" + text + "'");
	}

	[ServerVar(Help = "(Generated) Sets the minimum delay in seconds between automatic triggers of the named server event")]
	public static void set_event_min_delay(Arg arg)
	{
		if (!arg.HasArgs(2))
		{
			arg.ReplyWith("Usage: set_event_max_delay <event_name> <float>");
			return;
		}
		string text = arg.GetString(0);
		float num = arg.GetFloat(1);
		GetPendingChanges(text, create: true).MinHours = num;
		ApplyAllChanges();
		arg.ReplyWith($"Set minimum delay for '{text}' to {num} in-game hours");
	}

	[ServerVar(Help = "(Generated) Sets the maximum delay in seconds between automatic triggers of the named server event")]
	public static void set_event_max_delay(Arg arg)
	{
		if (!arg.HasArgs(2))
		{
			arg.ReplyWith("Usage: set_event_max_delay <event_name> <float>");
			return;
		}
		string text = arg.GetString(0);
		float num = arg.GetFloat(1);
		GetPendingChanges(text, create: true).MaxHours = num;
		ApplyAllChanges();
		arg.ReplyWith($"Set maximum delay for '{text}' to {num} in-game hours");
	}

	private static bool HasAnyEvents()
	{
		return EventSchedule.allEvents.Count > 0;
	}

	private static EventSchedule FindEventById(string key)
	{
		return EventSchedule.allEvents.FirstOrDefault((EventSchedule e) => e.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
	}
}

using System;
using System.Collections.Generic;
using API.Abstracts;
using API.Events;
using Utility;

namespace Components;

internal sealed class EventManager : CarbonBehaviour, IEventManager
{
	private readonly Dictionary<CarbonEvent, Delegate> events = new Dictionary<CarbonEvent, Delegate>();

	public void Subscribe(CarbonEvent eventId, Action<EventArgs> callback)
	{
		if (!events.ContainsKey(eventId))
		{
			events[eventId] = callback;
		}
		else
		{
			events[eventId] = Delegate.Combine(events[eventId], callback);
		}
	}

	public void Trigger(CarbonEvent eventId, EventArgs args)
	{
		if (!events.ContainsKey(eventId))
		{
			return;
		}
		Action<EventArgs> action = events[eventId] as Action<EventArgs>;
		try
		{
			action?.Invoke(args);
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed executing {eventId}", ex);
		}
	}

	public void Unsubscribe(CarbonEvent eventId, Action<EventArgs> callback)
	{
		if (events.ContainsKey(eventId))
		{
			events[eventId] = Delegate.Remove(events[eventId], callback);
		}
	}

	public void Reset(CarbonEvent eventId)
	{
		if (events.ContainsKey(eventId))
		{
			events[eventId] = null;
			events.Remove(eventId);
		}
	}
}

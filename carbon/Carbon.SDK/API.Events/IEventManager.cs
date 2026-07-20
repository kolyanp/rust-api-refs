using System;

namespace API.Events;

public interface IEventManager
{
	void Subscribe(CarbonEvent eventId, Action<EventArgs> callback);

	void Trigger(CarbonEvent eventId, EventArgs args);

	void Unsubscribe(CarbonEvent eventId, Action<EventArgs> callback);

	void Reset(CarbonEvent eventId);
}

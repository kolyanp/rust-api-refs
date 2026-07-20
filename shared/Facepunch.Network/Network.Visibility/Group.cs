using System;
using System.Collections.Generic;
using Network.Relay;
using UnityEngine;

namespace Network.Visibility;

public class Group : IDisposable
{
	protected Manager manager;

	public uint ID;

	public Bounds bounds;

	public bool restricted;

	public ListHashSet<Networkable> networkables;

	public List<Connection> subscribers;

	public bool isGlobal => ID == 0;

	public Group(Manager m, uint id)
	{
		manager = m;
		ID = id;
	}

	public virtual void Dispose()
	{
		networkables = null;
		subscribers = null;
		manager = null;
		ID = 0u;
	}

	public void Join(Networkable nw)
	{
		if (networkables == null)
		{
			networkables = new ListHashSet<Networkable>();
		}
		if (networkables.Contains(nw))
		{
			Debug.LogWarning((object)"Insert: Network Group already contains networkable!");
		}
		else
		{
			networkables.Add(nw);
		}
	}

	public void Leave(Networkable nw)
	{
		if (networkables != null)
		{
			if (!networkables.Contains(nw))
			{
				Debug.LogWarning((object)"Leave: Network Group doesn't contain networkable!");
			}
			else
			{
				networkables.Remove(nw);
			}
		}
	}

	public void AddSubscriber(Connection cn)
	{
		if (subscribers == null)
		{
			subscribers = new List<Connection>();
		}
		subscribers.Add(cn);
	}

	public void RemoveSubscriber(Connection cn)
	{
		if (subscribers != null)
		{
			subscribers.Remove(cn);
		}
	}

	public bool HasSubscribers()
	{
		if (subscribers == null)
		{
			return false;
		}
		if (!RustRelay.HasActiveFakeConnection)
		{
			return subscribers.Count > 0;
		}
		if (subscribers.Count == 0)
		{
			return false;
		}
		if (!RustRelay.IsFakeConnection(subscribers[0]))
		{
			return true;
		}
		return subscribers.Count > 1;
	}

	public override string ToString()
	{
		return "NWGroup" + ID;
	}
}

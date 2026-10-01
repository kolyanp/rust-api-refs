using System.Collections.Generic;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class PersistentTimerSet
{
	public readonly FacepunchBehaviour Owner;

	private readonly List<PersistentTimer> timers = new List<PersistentTimer>();

	public PersistentTimerSet(FacepunchBehaviour owner)
	{
		Owner = owner;
	}

	public void Register(PersistentTimer timer)
	{
		if (Find(timer.Id) != null)
		{
			Debug.LogError((object)$"{((Object)Owner).name}: two persistent timers both claim id {timer.Id}.");
		}
		timers.Add(timer);
	}

	public void Save(List<PersistentTimer> into, float cachedTime)
	{
		foreach (PersistentTimer timer in timers)
		{
			if (timer.IsRunning)
			{
				PersistentTimer val = Pool.Get<PersistentTimer>();
				val.id = timer.Id;
				val.remaining = timer.RemainingFrom(cachedTime);
				into.Add(val);
			}
		}
	}

	public void Load(List<PersistentTimer> saved)
	{
		if (saved == null)
		{
			return;
		}
		foreach (PersistentTimer item in saved)
		{
			PersistentTimer persistentTimer = Find(item.id);
			if (persistentTimer != null)
			{
				if (item.remaining > 0f)
				{
					persistentTimer.Start(item.remaining);
				}
				else
				{
					persistentTimer.onElapsed?.Invoke();
				}
			}
		}
	}

	public void CopyTo(PersistentTimerSet other)
	{
		foreach (PersistentTimer timer in timers)
		{
			if (!timer.carryOnReplace)
			{
				continue;
			}
			PersistentTimer persistentTimer = other.Find(timer.Id);
			if (persistentTimer != null)
			{
				if (timer.IsRunning)
				{
					persistentTimer.Start(timer.Remaining);
				}
				else
				{
					persistentTimer.Stop();
				}
			}
		}
	}

	private PersistentTimer Find(int id)
	{
		foreach (PersistentTimer timer in timers)
		{
			if (timer.Id == id)
			{
				return timer;
			}
		}
		return null;
	}
}

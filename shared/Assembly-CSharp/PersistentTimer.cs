using System;
using UnityEngine;

public class PersistentTimer
{
	public Action onElapsed;

	public Action onStarted;

	public bool carryOnReplace = true;

	public readonly int Id;

	private readonly FacepunchBehaviour owner;

	private TimeUntil timeLeft;

	private bool running;

	public bool IsRunning => running;

	public float Remaining
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (!running)
			{
				return 0f;
			}
			return Mathf.Max(0f, TimeUntil.op_Implicit(timeLeft));
		}
	}

	public PersistentTimer(PersistentTimerSet set, int id)
	{
		Id = id;
		owner = set.Owner;
		set.Register(this);
	}

	public void Start(float duration)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		owner.CancelInvoke(Fire);
		running = true;
		timeLeft = TimeUntil.op_Implicit(Mathf.Max(0f, duration));
		owner.Invoke(Fire, TimeUntil.op_Implicit(timeLeft));
		onStarted?.Invoke();
	}

	public void Stop()
	{
		running = false;
		owner.CancelInvoke(Fire);
	}

	public float RemainingFrom(float cachedTime)
	{
		if (!running)
		{
			return 0f;
		}
		return Mathf.Max(0f, timeLeft.LeftFrom(cachedTime));
	}

	private void Fire()
	{
		running = false;
		onElapsed?.Invoke();
	}
}

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

	private float elapsedWhenEnded;

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

	public float Duration { get; private set; }

	public float Elapsed
	{
		get
		{
			if (!running)
			{
				return elapsedWhenEnded;
			}
			return Mathf.Max(0f, Duration - Remaining);
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
		Resume(duration, duration);
	}

	public void Reschedule(float remaining)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (running)
		{
			owner.CancelInvoke(Fire);
			timeLeft = TimeUntil.op_Implicit(Mathf.Max(0f, remaining));
			owner.Invoke(Fire, TimeUntil.op_Implicit(timeLeft));
		}
	}

	public void Resume(float remaining, float duration)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		owner.CancelInvoke(Fire);
		running = true;
		Duration = duration;
		elapsedWhenEnded = 0f;
		timeLeft = TimeUntil.op_Implicit(Mathf.Max(0f, remaining));
		owner.Invoke(Fire, TimeUntil.op_Implicit(timeLeft));
		onStarted?.Invoke();
	}

	public void Stop()
	{
		elapsedWhenEnded = Elapsed;
		running = false;
		owner.CancelInvoke(Fire);
	}

	public void RunWhile(bool shouldRun, float duration)
	{
		if (shouldRun && !running)
		{
			Start(duration);
		}
		else if (!shouldRun && running)
		{
			Stop();
		}
	}

	public void CopyTo(PersistentTimer other)
	{
		if (running)
		{
			other.Resume(Remaining, Duration);
		}
		else
		{
			other.Stop();
		}
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
		elapsedWhenEnded = Duration;
		running = false;
		onElapsed?.Invoke();
	}
}

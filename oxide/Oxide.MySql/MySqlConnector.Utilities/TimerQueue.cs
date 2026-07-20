using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MySqlConnector.Utilities;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal sealed class TimerQueue
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
	private readonly struct Data(uint id, int time, Action action)
	{
		public uint Id { get; } = id;

		public int Time { get; } = time;

		public Action Action { get; } = action;
	}

	private readonly object m_lock;

	private readonly Timer m_timer;

	private readonly List<Data> m_timeoutActions;

	private uint m_counter;

	private bool m_isTimerEnabled;

	private int m_nextTimerTick;

	public static TimerQueue Instance { get; } = new TimerQueue();

	public uint Add(int delay, Action action)
	{
		if (delay < 0)
		{
			throw new ArgumentOutOfRangeException("delay", $"delay must not be negative: {delay}");
		}
		int tickCount = Environment.TickCount;
		lock (m_lock)
		{
			uint num = ++m_counter;
			if (num == 0)
			{
				num = ++m_counter;
			}
			int num2 = m_timeoutActions.Count;
			while (num2 > 0 && delay < m_timeoutActions[num2 - 1].Time - tickCount)
			{
				num2--;
			}
			int time = tickCount + delay;
			m_timeoutActions.Insert(num2, new Data(num, time, action));
			if (!m_isTimerEnabled || (num2 == 0 && m_nextTimerTick - tickCount > delay))
			{
				UnsafeSetTimer(delay);
			}
			return num;
		}
	}

	public bool Remove(uint id)
	{
		lock (m_lock)
		{
			for (int i = 0; i < m_timeoutActions.Count; i++)
			{
				if (m_timeoutActions[i].Id == id)
				{
					m_timeoutActions.RemoveAt(i);
					return true;
				}
			}
		}
		return false;
	}

	private TimerQueue()
	{
		m_lock = new object();
		m_timer = new Timer(Callback, this, -1, -1);
		m_timeoutActions = new List<Data>();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	private void Callback(object obj)
	{
		List<Action> list = new List<Action>();
		lock (m_lock)
		{
			while (m_timeoutActions.Count > 0 && m_timeoutActions[0].Time - Environment.TickCount < 15)
			{
				list.Add(m_timeoutActions[0].Action);
				m_timeoutActions.RemoveAt(0);
			}
			if (m_timeoutActions.Count == 0)
			{
				UnsafeClearTimer();
			}
			else
			{
				int delay = Math.Max(250, m_timeoutActions[0].Time - Environment.TickCount);
				UnsafeSetTimer(delay);
			}
		}
		foreach (Action item in list)
		{
			item();
		}
	}

	private void UnsafeSetTimer(int delay)
	{
		m_nextTimerTick = Environment.TickCount + delay;
		m_isTimerEnabled = true;
		m_timer.Change(delay, -1);
	}

	private void UnsafeClearTimer()
	{
		m_nextTimerTick = 0;
		m_isTimerEnabled = false;
		m_timer.Change(-1, -1);
	}
}

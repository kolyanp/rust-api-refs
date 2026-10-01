using System;
using UnityEngine.Playables;

public readonly struct SafePlayable<T>(T raw) : IEquatable<SafePlayable<T>> where T : struct, IPlayable
{
	public readonly T Raw = raw;

	public static implicit operator SafePlayable<T>(T raw)
	{
		return new SafePlayable<T>(raw);
	}

	public bool IsValid()
	{
		return PlayableExtensions.IsValid<T>(Raw);
	}

	public bool Equals(SafePlayable<T> other)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		PlayableHandle handle = ((IPlayable)Raw/*cast due to constrained. prefix*/).GetHandle();
		return handle.Equals(((IPlayable)other.Raw/*cast due to constrained. prefix*/).GetHandle());
	}

	public override bool Equals(object obj)
	{
		if (obj is SafePlayable<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return ((object)((IPlayable)Raw/*cast due to constrained. prefix*/).GetHandle()/*cast due to constrained. prefix*/).GetHashCode();
	}

	public void Play()
	{
		if (IsValid())
		{
			PlayableExtensions.Play<T>(Raw);
		}
	}

	public void Pause()
	{
		if (IsValid())
		{
			PlayableExtensions.Pause<T>(Raw);
		}
	}

	public PlayState GetPlayState()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!IsValid())
		{
			return (PlayState)0;
		}
		return PlayableExtensions.GetPlayState<T>(Raw);
	}

	public void SetTime(double time)
	{
		if (IsValid())
		{
			PlayableExtensions.SetTime<T>(Raw, time);
		}
	}

	public double GetTime()
	{
		if (!IsValid())
		{
			return 0.0;
		}
		return PlayableExtensions.GetTime<T>(Raw);
	}

	public void SetSpeed(double speed)
	{
		if (IsValid())
		{
			PlayableExtensions.SetSpeed<T>(Raw, speed);
		}
	}

	public double GetSpeed()
	{
		if (!IsValid())
		{
			return 0.0;
		}
		return PlayableExtensions.GetSpeed<T>(Raw);
	}

	public void Destroy()
	{
		if (IsValid())
		{
			PlayableExtensions.Destroy<T>(Raw);
		}
	}

	public int GetInputCount()
	{
		if (!IsValid())
		{
			return 0;
		}
		return PlayableExtensions.GetInputCount<T>(Raw);
	}

	public void SetInputCount(int count)
	{
		if (IsValid())
		{
			PlayableExtensions.SetInputCount<T>(Raw, count);
		}
	}

	public bool HasInput(int port)
	{
		if (port >= 0)
		{
			return port < GetInputCount();
		}
		return false;
	}

	public float GetInputWeight(int port)
	{
		if (!HasInput(port))
		{
			return 0f;
		}
		return PlayableExtensions.GetInputWeight<T>(Raw, port);
	}

	public void SetInputWeight(int port, float weight)
	{
		if (HasInput(port))
		{
			PlayableExtensions.SetInputWeight<T>(Raw, port, weight);
		}
	}

	public void ConnectInput<TSource>(int port, SafePlayable<TSource> source, int sourcePort) where TSource : struct, IPlayable
	{
		this.ConnectInput<TSource>(port, source.Raw, sourcePort);
	}

	public void ConnectInput<TSource>(int port, TSource source, int sourcePort) where TSource : struct, IPlayable
	{
		if (HasInput(port) && PlayableExtensions.IsValid<TSource>(source))
		{
			PlayableExtensions.ConnectInput<T, TSource>(Raw, port, source, sourcePort);
		}
	}

	public void DisconnectInput(int port)
	{
		if (HasInput(port))
		{
			PlayableExtensions.DisconnectInput<T>(Raw, port);
		}
	}

	public int AddInput<TSource>(SafePlayable<TSource> source, int sourcePort, float weight) where TSource : struct, IPlayable
	{
		return this.AddInput<TSource>(source.Raw, sourcePort, weight);
	}

	public int AddInput<TSource>(TSource source, int sourcePort, float weight) where TSource : struct, IPlayable
	{
		if (!IsValid() || !PlayableExtensions.IsValid<TSource>(source))
		{
			return -1;
		}
		int inputCount = PlayableExtensions.GetInputCount<T>(Raw);
		PlayableExtensions.SetInputCount<T>(Raw, inputCount + 1);
		PlayableExtensions.ConnectInput<T, TSource>(Raw, inputCount, source, sourcePort);
		PlayableExtensions.SetInputWeight<T>(Raw, inputCount, weight);
		return inputCount;
	}
}

using UnityEngine;

public struct PlayerOneShotHandle
{
	public double StartTime { get; private set; }

	public float Length { get; private set; }

	public bool Valid { get; private set; }

	public static PlayerOneShotHandle InvalidHandle => new PlayerOneShotHandle
	{
		Valid = false
	};

	public readonly float GetNormalizedTime()
	{
		if (!Valid || Length <= Mathf.Epsilon)
		{
			return 0f;
		}
		return Mathf.Clamp01((float)((Time.timeAsDouble - StartTime) / (double)Length));
	}

	public static PlayerOneShotHandle Create(float length)
	{
		return new PlayerOneShotHandle
		{
			StartTime = Time.timeAsDouble,
			Length = length,
			Valid = true
		};
	}

	public static implicit operator bool(PlayerOneShotHandle handle)
	{
		return handle.Valid;
	}
}

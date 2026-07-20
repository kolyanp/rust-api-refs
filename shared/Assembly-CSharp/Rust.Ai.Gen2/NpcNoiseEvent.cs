using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

public readonly struct NpcNoiseEvent(int id, BaseEntity initiator, Vector3 position, Vector3 initiatorPosition, NpcNoiseIntensity intensity, double eventTime) : IEquatable<NpcNoiseEvent>
{
	public readonly int Id = id;

	public readonly BaseEntity Initiator = initiator;

	public readonly Vector3 NoisePosition = position;

	public readonly Vector3 GuessedInitiatorPosition = initiatorPosition;

	public readonly NpcNoiseIntensity Intensity = intensity;

	public readonly double EventTime = eventTime;

	public bool Equals(NpcNoiseEvent other)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (Id == other.Id)
		{
			double eventTime = EventTime;
			if (eventTime.Equals(other.EventTime) && (Object)(object)Initiator == (Object)(object)other.Initiator && Intensity == other.Intensity && ((Vector3)(ref NoisePosition)).Equals(other.NoisePosition))
			{
				return ((Vector3)(ref GuessedInitiatorPosition)).Equals(other.GuessedInitiatorPosition);
			}
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Id;
	}

	public override bool Equals(object obj)
	{
		if (obj is NpcNoiseEvent other)
		{
			return Equals(other);
		}
		return false;
	}

	public static bool operator ==(NpcNoiseEvent left, NpcNoiseEvent right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(NpcNoiseEvent left, NpcNoiseEvent right)
	{
		return !left.Equals(right);
	}
}

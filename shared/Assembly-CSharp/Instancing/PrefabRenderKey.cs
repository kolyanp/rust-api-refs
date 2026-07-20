using System;

namespace Instancing;

public struct PrefabRenderKey(uint prefabId, int grade, ulong skin) : IEquatable<PrefabRenderKey>
{
	public uint PrefabId = prefabId;

	public int Grade = grade;

	public ulong Skin = skin;

	public bool Equals(PrefabRenderKey other)
	{
		if (PrefabId == other.PrefabId && Grade == other.Grade)
		{
			return Skin == other.Skin;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is PrefabRenderKey other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)(((PrefabId * 397) ^ (uint)Grade) * 397) ^ Skin.GetHashCode();
	}
}

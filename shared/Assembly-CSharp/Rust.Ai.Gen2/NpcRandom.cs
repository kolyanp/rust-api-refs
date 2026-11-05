using System;
using ConVar;
using Unity.Mathematics;
using UnityEngine;

namespace Rust.Ai.Gen2;

public sealed class NpcRandom
{
	private Random random;

	private static uint spawnOrder;

	private static NpcRandom shared;

	public float Value => random.NextFloat();

	public Vector2 InsideUnitCircle
	{
		get
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			float num = random.NextFloat(0f, MathF.PI * 2f);
			float num2 = Mathf.Sqrt(random.NextFloat());
			return new Vector2(Mathf.Cos(num), Mathf.Sin(num)) * num2;
		}
	}

	public Vector3 InsideUnitSphere
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			return float3.op_Implicit(random.NextFloat3Direction()) * Mathf.Pow(random.NextFloat(), 1f / 3f);
		}
	}

	public Quaternion RotationUniform
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return quaternion.op_Implicit(random.NextQuaternionRotation());
		}
	}

	public NpcRandom(uint seed)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		random = new Random((seed == 0) ? 1u : seed);
	}

	public static NpcRandom ForSpawn()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (AI.debug_seed != 0)
		{
			uint seed = math.hash(new uint2((uint)AI.debug_seed, spawnOrder));
			spawnOrder++;
			return new NpcRandom(seed);
		}
		return new NpcRandom((uint)Random.Range(1, int.MaxValue));
	}

	public static void ResetSpawnOrder()
	{
		spawnOrder = 0u;
		shared = null;
	}

	public static NpcRandom Of(BaseEntity entity)
	{
		if (entity is BaseNPC2 baseNPC)
		{
			return baseNPC.Rng;
		}
		return shared ?? (shared = ForShared());
	}

	private static NpcRandom ForShared()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (AI.debug_seed != 0)
		{
			return new NpcRandom(math.hash(new uint3((uint)AI.debug_seed, 0u, 0u)));
		}
		return new NpcRandom((uint)Random.Range(1, int.MaxValue));
	}

	public float Range(float min, float max)
	{
		return random.NextFloat(min, max);
	}

	public int Range(int min, int max)
	{
		if (max > min)
		{
			return random.NextInt(min, max);
		}
		return min;
	}

	public uint NextSeed()
	{
		return random.NextUInt(1u, uint.MaxValue);
	}

	public T Pick<T>(T[] items)
	{
		return items[Range(0, items.Length)];
	}
}

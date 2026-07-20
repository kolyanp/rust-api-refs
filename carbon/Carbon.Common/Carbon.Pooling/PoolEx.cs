using System.Collections.Generic;
using System.Diagnostics;
using Facepunch;
using UnityEngine;

namespace Carbon.Pooling;

public class PoolEx
{
	public static Stopwatch GetStopwatch()
	{
		return Pool.Get<Stopwatch>();
	}

	public static void FreeStopwatch(ref Stopwatch value)
	{
		value.Reset();
		Pool.FreeUnsafe<Stopwatch>(ref value);
	}

	public static void FreeRaycastHitList(ref List<RaycastHit> hitList)
	{
		Pool.FreeUnmanaged<RaycastHit>(ref hitList);
	}
}

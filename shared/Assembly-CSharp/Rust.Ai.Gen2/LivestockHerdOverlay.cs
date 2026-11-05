using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class LivestockHerdOverlay : DebugDrawOverlay
{
	public static readonly LivestockHerdOverlay Instance = new LivestockHerdOverlay();

	private const float LabelLift = 2.5f;

	private const int MaxDrawnHerd = 256;

	public override string Name => "livestock.herds";

	public override int FirstColour => 6;

	public override void Build(BasePlayer viewer, DebugDrawOverlayFrame frame)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			PooledList<LivestockAnimal> val2 = Pool.Get<PooledList<LivestockAnimal>>();
			try
			{
				PooledHashSet<LivestockAnimal> val3 = Pool.Get<PooledHashSet<LivestockAnimal>>();
				try
				{
					BaseEntity.Query.Server.GetBrainsInSphere(((Component)viewer).transform.position, Livestock.drawRadius, (List<LivestockAnimal>)(object)val);
					foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
					{
						if (!item.CountsFreeRangeHerd || ((HashSet<LivestockAnimal>)(object)val3).Contains(item))
						{
							continue;
						}
						((List<LivestockAnimal>)(object)val2).Clear();
						item.GatherFreeRangeHerd(256, (List<LivestockAnimal>)(object)val2);
						foreach (LivestockAnimal item2 in (List<LivestockAnimal>)(object)val2)
						{
							((HashSet<LivestockAnimal>)(object)val3).Add(item2);
						}
						DrawHerd(frame, (List<LivestockAnimal>)(object)val2);
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static void DrawHerd(DebugDrawOverlayFrame frame, List<LivestockAnimal> herd)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		LivestockAnimal livestockAnimal = herd[0];
		Vector3 val = Vector3.zero;
		int num = 0;
		foreach (LivestockAnimal item in herd)
		{
			val += ((Component)item).transform.position;
			if (item.net.ID.Value < livestockAnimal.net.ID.Value)
			{
				livestockAnimal = item;
			}
			if (item.HasOwner)
			{
				num++;
			}
		}
		uint key = (uint)livestockAnimal.net.ID.Value;
		if (frame.BeginGroup(key))
		{
			Color32 colour = frame.ColourFor(key);
			DrawSpanningTree(frame, herd, colour);
			val /= (float)herd.Count;
			string arg = ((herd.Count >= 256) ? $"{herd.Count}+" : $"{herd.Count}");
			string arg2 = ((num < herd.Count) ? $" (with {herd.Count - num} non-owned)" : "");
			frame.AddLabel(val + Vector3.up * 2.5f, $"{arg}/{Livestock.maxFreeRangeHerd}{arg2}", colour);
		}
	}

	private static void DrawSpanningTree(DebugDrawOverlayFrame frame, List<LivestockAnimal> herd, Color32 colour)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
		try
		{
			PooledList<bool> val2 = Pool.Get<PooledList<bool>>();
			try
			{
				PooledList<float> val3 = Pool.Get<PooledList<float>>();
				try
				{
					PooledList<int> val4 = Pool.Get<PooledList<int>>();
					try
					{
						foreach (LivestockAnimal item in herd)
						{
							((List<Vector3>)(object)val).Add(item.CenterPoint());
						}
						Vector3 val5;
						for (int i = 0; i < herd.Count; i++)
						{
							((List<bool>)(object)val2).Add(i == 0);
							val5 = ((List<Vector3>)(object)val)[i] - ((List<Vector3>)(object)val)[0];
							((List<float>)(object)val3).Add(val5.sqrMagnitude);
							((List<int>)(object)val4).Add(0);
						}
						for (int j = 1; j < herd.Count; j++)
						{
							int num = -1;
							for (int k = 0; k < herd.Count; k++)
							{
								if (!((List<bool>)(object)val2)[k] && (num < 0 || ((List<float>)(object)val3)[k] < ((List<float>)(object)val3)[num]))
								{
									num = k;
								}
							}
							((List<bool>)(object)val2)[num] = true;
							frame.AddLine(((List<Vector3>)(object)val)[((List<int>)(object)val4)[num]], ((List<Vector3>)(object)val)[num], colour);
							for (int l = 0; l < herd.Count; l++)
							{
								if (!((List<bool>)(object)val2)[l])
								{
									val5 = ((List<Vector3>)(object)val)[l] - ((List<Vector3>)(object)val)[num];
									float sqrMagnitude = val5.sqrMagnitude;
									if (sqrMagnitude < ((List<float>)(object)val3)[l])
									{
										((List<float>)(object)val3)[l] = sqrMagnitude;
										((List<int>)(object)val4)[l] = num;
									}
								}
							}
						}
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}

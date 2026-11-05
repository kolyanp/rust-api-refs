using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class LivestockPenOverlay : DebugDrawOverlay
{
	public static readonly LivestockPenOverlay Instance = new LivestockPenOverlay();

	private const float BorderLift = 0.2f;

	private const float LabelLift = 4.5f;

	private const float MarkerSize = 0.1f;

	public override string Name => "livestock.pens";

	public override void Build(BasePlayer viewer, DebugDrawOverlayFrame frame)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			BaseEntity.Query.Server.GetBrainsInSphere(((Component)viewer).transform.position, Livestock.drawRadius, (List<LivestockAnimal>)(object)val);
			foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
			{
				LivestockFloor floor = item.Floor;
				if (floor != null && !floor.IsField && (Object)(object)item.NavAgent != (Object)null)
				{
					DrawPen(frame, floor, item);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static void DrawPen(DebugDrawOverlayFrame frame, LivestockFloor floor, LivestockAnimal standing)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		uint num = KeyOf(floor.Centre);
		if (!frame.BeginGroup(num * 2))
		{
			return;
		}
		Color32 colour = frame.ColourFor(num);
		Vector3 val = Vector3.up * 0.2f;
		PooledList<Vector3> val2 = Pool.Get<PooledList<Vector3>>();
		try
		{
			floor.GetBorder(standing.NavAgent, (List<Vector3>)(object)val2);
			for (int i = 0; i + 1 < ((List<Vector3>)(object)val2).Count; i += 2)
			{
				frame.AddLine(((List<Vector3>)(object)val2)[i] + val, ((List<Vector3>)(object)val2)[i + 1] + val, colour);
			}
			PooledList<LivestockAnimal> val3 = Pool.Get<PooledList<LivestockAnimal>>();
			try
			{
				PooledList<BaseCombatEntity> val4 = Pool.Get<PooledList<BaseCombatEntity>>();
				try
				{
					floor.GetContents((List<LivestockAnimal>)(object)val3, (List<BaseCombatEntity>)(object)val4);
					foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val3)
					{
						frame.AddBox(item, 0.1f, colour);
					}
					foreach (BaseCombatEntity item2 in (List<BaseCombatEntity>)(object)val4)
					{
						frame.AddBox(item2, 0.1f, colour);
					}
					frame.BeginGroup(num * 2 + 1);
					frame.AddLabel(floor.Centre + Vector3.up * 4.5f, Describe(floor), colour);
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

	private static string Describe(LivestockFloor floor)
	{
		floor.Refresh();
		float lethalFloorPerHead = Livestock.lethalFloorPerHead;
		float num = floor.Area / LivestockFloor.BreedingFloorPerHead;
		string text = ((lethalFloorPerHead > 0f) ? $"{floor.Area / lethalFloorPerHead:0.#}" : "never");
		float share = ((floor.Area > 0f) ? (floor.GrassArea / floor.Area) : 0f);
		return string.Format("max happy cows {0:0.#}, cows start dying at {1}, now {2:0.#}, grass {3}, sun {4}", new object[5]
		{
			num,
			text,
			floor.Heads,
			Percent(share),
			Percent(floor.SunShare)
		});
	}

	private static string Percent(float share)
	{
		return $"{Mathf.RoundToInt(share * 100f)}%";
	}

	private static uint KeyOf(Vector3 centre)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		return (uint)(((((((-2128831035 ^ Mathf.RoundToInt(centre.x)) * 16777619) ^ Mathf.RoundToInt(centre.y)) * 16777619) ^ Mathf.RoundToInt(centre.z)) * 16777619) & 0x7FFFFFFF);
	}
}

using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

public class OvergrazedArea : DepletedArea
{
	public struct GrazedCell
	{
		public int Count;

		public TimeSince LastGrazed;
	}

	public const string PrefabPath = "assets/prefabs/npc/ranch/overgrazedarea.prefab";

	public static Dictionary<Vector2Int, GrazedCell> GrazedCells = new Dictionary<Vector2Int, GrazedCell>();

	private const int PruneThreshold = 256;

	protected override float Radius => Livestock.overgrazedAreaRadius;

	protected override float DurationMinutes => Livestock.overgrazedAreaCooldownMinutes;

	protected override bool DebugEnabled => Livestock.debugOvergrazing;

	public override void ServerInit()
	{
		base.ServerInit();
		limitNetworking = true;
	}

	public static bool IsOvergrazed(Vector3 position)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (Livestock.overgrazingEnabled)
		{
			return (Object)(object)GetAreaAtPosition(position) != (Object)null;
		}
		return false;
	}

	private static OvergrazedArea GetAreaAtPosition(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return DepletedArea.GetAtPosition<OvergrazedArea>(position, Livestock.overgrazedAreaRadius);
	}

	public static void RegisterGraze(Vector3 position)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		if (Livestock.overgrazingEnabled && !((Object)(object)GetAreaAtPosition(position) != (Object)null))
		{
			Vector2Int cell = GetCell(position);
			float num = Livestock.overgrazedAreaCooldownMinutes * 60f;
			if (!GrazedCells.TryGetValue(cell, out var value))
			{
				value = default;
			}
			else if (TimeSince.op_Implicit(value.LastGrazed) >= num)
			{
				value.Count = 0;
			}
			else if (TimeSince.op_Implicit(value.LastGrazed) >= num * 0.5f)
			{
				value.Count = Mathf.FloorToInt((float)value.Count * 0.5f);
			}
			value.Count++;
			value.LastGrazed = TimeSince.op_Implicit(0f);
			GrazedCells[cell] = value;
			DebugOvergrazing($"REGISTER GRAZE | Cell {cell} grazed {value.Count}/{Livestock.grazesUntilOvergrazed} times");
			if (GrazedCells.Count >= 256)
			{
				PruneRecoveredCells(num);
			}
			if (value.Count >= Livestock.grazesUntilOvergrazed && (Object)(object)MarkOvergrazed(position) != (Object)null)
			{
				GrazedCells.Remove(cell);
			}
		}
	}

	public static OvergrazedArea MarkOvergrazed(Vector3 position)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		OvergrazedArea overgrazedArea = GameManager.server.CreateEntity("assets/prefabs/npc/ranch/overgrazedarea.prefab", position, Quaternion.identity) as OvergrazedArea;
		if ((Object)(object)overgrazedArea == (Object)null)
		{
			return null;
		}
		overgrazedArea.Spawn();
		DebugOvergrazing($"MARK OVERGRAZED | Grass at position {position} is now overgrazed", (Object)(object)overgrazedArea);
		return overgrazedArea;
	}

	private static Vector2Int GetCell(Vector3 position)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Max(Livestock.overgrazedAreaRadius, 1f);
		return new Vector2Int(Mathf.FloorToInt(position.x / num), Mathf.FloorToInt(position.z / num));
	}

	private static void PruneRecoveredCells(float cooldown)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		PooledList<Vector2Int> val = Pool.Get<PooledList<Vector2Int>>();
		try
		{
			foreach (KeyValuePair<Vector2Int, GrazedCell> grazedCell in GrazedCells)
			{
				if (TimeSince.op_Implicit(grazedCell.Value.LastGrazed) >= cooldown)
				{
					((List<Vector2Int>)(object)val).Add(grazedCell.Key);
				}
			}
			foreach (Vector2Int item in (List<Vector2Int>)(object)val)
			{
				GrazedCells.Remove(item);
			}
			DebugOvergrazing($"PRUNE | Dropped {((List<Vector2Int>)(object)val).Count} grown back cells, {GrazedCells.Count} left");
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static void DebugOvergrazing(string message, Object context = null)
	{
		if (Livestock.debugOvergrazing)
		{
			Debug.Log((object)message, context);
		}
	}
}

using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class TerritoryZoneController : PointEntity<TerritoryZoneController>
{
	public const int MaxFactions = 32;

	[NonSerialized]
	public float HexSize;

	[NonSerialized]
	public Vector2 GridOffset;

	[NonSerialized]
	public byte[] CellFactions;

	[NonSerialized]
	public uint[] FactionColors = DefaultColors();

	[NonSerialized]
	public string[] FactionNames = new string[32];

	[NonSerialized]
	public int StateVersion;

	private static uint[] DefaultColors()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		return new uint[32]
		{
			0u,
			Pack(new Color32((byte)231, (byte)76, (byte)60, (byte)178)),
			Pack(new Color32((byte)52, (byte)152, (byte)219, (byte)178)),
			Pack(new Color32((byte)46, (byte)204, (byte)113, (byte)178)),
			Pack(new Color32((byte)241, (byte)196, (byte)15, (byte)178)),
			Pack(new Color32((byte)155, (byte)89, (byte)182, (byte)178)),
			Pack(new Color32((byte)230, (byte)126, (byte)34, (byte)178)),
			Pack(new Color32((byte)26, (byte)188, (byte)156, (byte)178)),
			Pack(new Color32((byte)236, (byte)240, (byte)241, (byte)178)),
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u,
			0u
		};
	}

	public static uint Pack(Color32 c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return (uint)((c.r << 24) | (c.g << 16) | (c.b << 8) | c.a);
	}

	public static Color32 Unpack(uint v)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return new Color32((byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v);
	}

	public int GetFaction(int cell)
	{
		if (CellFactions == null || cell < 0 || cell >= CellFactions.Length)
		{
			return 0;
		}
		return CellFactions[cell];
	}

	public Color32 GetFactionColor(int faction)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (faction <= 0 || faction >= FactionColors.Length)
		{
			return default(Color32);
		}
		return Unpack(FactionColors[faction]);
	}

	public int FindFaction(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return -1;
		}
		for (int i = 1; i < FactionNames.Length; i++)
		{
			if (string.Equals(FactionNames[i], name, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}

	public override void Load(LoadInfo info)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.territoryZones == null)
		{
			return;
		}
		TerritoryZones territoryZones = info.msg.territoryZones;
		HexSize = territoryZones.hexSize;
		GridOffset = new Vector2(territoryZones.offsetX, territoryZones.offsetZ);
		if (territoryZones.cellFactions != null)
		{
			CellFactions = (byte[])territoryZones.cellFactions.Clone();
		}
		if (territoryZones.factionColors != null)
		{
			for (int i = 0; i < FactionColors.Length && i < territoryZones.factionColors.Count; i++)
			{
				FactionColors[i] = territoryZones.factionColors[i];
			}
		}
		if (territoryZones.factionNames != null)
		{
			for (int j = 0; j < FactionNames.Length && j < territoryZones.factionNames.Count; j++)
			{
				FactionNames[j] = territoryZones.factionNames[j];
			}
		}
		StateVersion++;
		if (base.isServer)
		{
			Territory.SyncFromController(this);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (CellFactions != null)
		{
			info.msg.territoryZones = Pool.Get<TerritoryZones>();
			info.msg.territoryZones.hexSize = HexSize;
			info.msg.territoryZones.offsetX = GridOffset.x;
			info.msg.territoryZones.offsetZ = GridOffset.y;
			info.msg.territoryZones.cellFactions = (byte[])CellFactions.Clone();
			info.msg.territoryZones.factionColors = Pool.Get<List<uint>>();
			info.msg.territoryZones.factionColors.AddRange(FactionColors);
			info.msg.territoryZones.factionNames = Pool.Get<List<string>>();
			string[] factionNames = FactionNames;
			foreach (string text in factionNames)
			{
				info.msg.territoryZones.factionNames.Add(text ?? string.Empty);
			}
		}
	}

	public void EnsureGrid(float hexSize)
	{
		if (CellFactions == null || !Mathf.Approximately(HexSize, hexSize))
		{
			HexSize = hexSize;
			CellFactions = new byte[HexGridLayout.CellCount(hexSize)];
			MarkChanged();
		}
	}

	public bool SetCell(int cell, int faction)
	{
		if (CellFactions == null || cell < 0 || cell >= CellFactions.Length || faction < 0 || faction >= 32)
		{
			return false;
		}
		if (CellFactions[cell] == faction)
		{
			return true;
		}
		CellFactions[cell] = (byte)faction;
		MarkChanged();
		return true;
	}

	public bool SetCellAt(Vector3 worldPos, int faction)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (CellFactions == null)
		{
			return false;
		}
		return SetCell(HexGridLayout.WorldToCell(worldPos, HexSize, GridOffset), faction);
	}

	public void SetGridOffset(Vector2 offset)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!(GridOffset == offset))
		{
			GridOffset = offset;
			MarkChanged();
		}
	}

	public int CreateFaction(string name, Color32 color)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(name))
		{
			return -1;
		}
		int num = FindFaction(name);
		if (num < 0)
		{
			for (int i = 1; i < FactionNames.Length; i++)
			{
				if (string.IsNullOrEmpty(FactionNames[i]))
				{
					num = i;
					break;
				}
			}
		}
		if (num < 0)
		{
			return -1;
		}
		FactionNames[num] = name;
		FactionColors[num] = Pack(color);
		MarkChanged();
		return num;
	}

	public void FillAll(int faction)
	{
		if (CellFactions != null && faction >= 0 && faction < 32)
		{
			for (int i = 0; i < CellFactions.Length; i++)
			{
				CellFactions[i] = (byte)faction;
			}
			MarkChanged();
		}
	}

	private void MarkChanged()
	{
		StateVersion++;
		InvalidateNetworkCache();
		SendNetworkUpdate();
	}

	public int CountRegions(int faction, out int largestRegion)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		largestRegion = 0;
		if (CellFactions == null)
		{
			return 0;
		}
		bool[] array = new bool[CellFactions.Length];
		Queue<int> queue = new Queue<int>();
		int num = 0;
		for (int i = 0; i < CellFactions.Length; i++)
		{
			if (CellFactions[i] != faction || array[i])
			{
				continue;
			}
			num++;
			int num2 = 0;
			queue.Enqueue(i);
			array[i] = true;
			while (queue.Count > 0)
			{
				int cell = queue.Dequeue();
				num2++;
				HexGridLayout.CellToAxial(cell, HexSize, out var q, out var r);
				for (int j = 0; j < 6; j++)
				{
					Vector2Int val = HexGridLayout.NeighbourDirs[j];
					int num3 = HexGridLayout.AxialToCell(q + ((Vector2Int)(ref val)).x, r + ((Vector2Int)(ref val)).y, HexSize);
					if (num3 >= 0 && num3 < CellFactions.Length && !array[num3] && CellFactions[num3] == faction)
					{
						array[num3] = true;
						queue.Enqueue(num3);
					}
				}
			}
			largestRegion = Mathf.Max(largestRegion, num2);
		}
		return num;
	}
}

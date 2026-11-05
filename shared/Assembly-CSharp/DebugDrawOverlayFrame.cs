using System;
using System.Collections.Generic;
using Facepunch;
using Network;
using UnityEngine;

public class DebugDrawOverlayFrame
{
	private struct Line
	{
		public Vector3 From;

		public Vector3 To;

		public Color32 Colour;
	}

	private struct Box
	{
		public NetworkableId Entity;

		public Vector3 Centre;

		public float Size;

		public Color32 Colour;
	}

	private struct Label
	{
		public Vector3 Position;

		public string Text;

		public Color32 Colour;
	}

	private struct Group
	{
		public uint Key;

		public int FirstLine;

		public int Lines;

		public int FirstBox;

		public int Boxes;

		public int FirstLabel;

		public int Labels;

		public ulong Hash;

		public Vector3 Anchor;

		public int Bytes;
	}

	private const ulong HashSeed = 14695981039346656037uL;

	private const ulong HashPrime = 1099511628211uL;

	private const int GroupBytes = 10;

	private const int LineBytes = 28;

	private const int BoxBytes = 28;

	private const int LabelBytes = 24;

	private static readonly Color32[] Palette = new Color32[12]
	{
		new Color32(byte.MaxValue, (byte)80, (byte)80, byte.MaxValue),
		new Color32((byte)80, (byte)220, byte.MaxValue, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)220, (byte)40, byte.MaxValue),
		new Color32((byte)120, byte.MaxValue, (byte)100, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)90, (byte)230, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)150, (byte)40, byte.MaxValue),
		new Color32((byte)150, (byte)120, byte.MaxValue, byte.MaxValue),
		new Color32((byte)40, byte.MaxValue, (byte)190, byte.MaxValue),
		new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
		new Color32((byte)200, byte.MaxValue, (byte)60, byte.MaxValue),
		new Color32(byte.MaxValue, (byte)140, (byte)170, byte.MaxValue),
		new Color32((byte)60, (byte)140, byte.MaxValue, byte.MaxValue)
	};

	public readonly DebugDrawOverlay Overlay;

	private readonly List<Line> lines = new List<Line>();

	private readonly List<Box> boxes = new List<Box>();

	private readonly List<Label> labels = new List<Label>();

	private readonly List<Group> groups = new List<Group>();

	private readonly HashSet<uint> keysThisBuild = new HashSet<uint>();

	private readonly Dictionary<uint, ulong> sent = new Dictionary<uint, ulong>();

	private readonly List<int> changed = new List<int>();

	private readonly List<uint> removed = new List<uint>();

	private readonly Dictionary<uint, int> colourSlots = new Dictionary<uint, int>();

	private readonly HashSet<uint> colourKeysThisBuild = new HashSet<uint>();

	public DebugDrawOverlayFrame(DebugDrawOverlay overlay)
	{
		Overlay = overlay;
	}

	public bool BeginGroup(uint key)
	{
		if (!keysThisBuild.Add(key))
		{
			return false;
		}
		groups.Add(new Group
		{
			Key = key,
			FirstLine = lines.Count,
			FirstBox = boxes.Count,
			FirstLabel = labels.Count,
			Hash = 14695981039346656037uL,
			Bytes = 10
		});
		return true;
	}

	public Color32 ColourFor(uint key)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		colourKeysThisBuild.Add(key);
		if (!colourSlots.TryGetValue(key, out var value))
		{
			value = FreeSlot();
			colourSlots[key] = value;
		}
		return Palette[(value + Overlay.FirstColour) % Palette.Length];
	}

	public void AddLine(Vector3 from, Vector3 to, Color32 colour)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (TryOpenGroup(out var group) && group.Lines < 65535)
		{
			lines.Add(new Line
			{
				From = from,
				To = to,
				Colour = colour
			});
			AnchorAt(ref group, from);
			group.Lines++;
			group.Bytes += 28;
			group.Hash = Mix(Mix(Mix(group.Hash, from), to), colour);
			groups[groups.Count - 1] = group;
		}
	}

	public void AddBox(BaseEntity entity, float size, Color32 colour)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		if (entity.IsValid() && TryOpenGroup(out var group) && group.Boxes < 65535)
		{
			Box item = new Box
			{
				Entity = entity.net.ID,
				Centre = entity.bounds.center,
				Size = size,
				Colour = colour
			};
			boxes.Add(item);
			AnchorAt(ref group, ((Component)entity).transform.position);
			group.Boxes++;
			group.Bytes += 28;
			group.Hash = Mix(Mix(Mix(Mix(group.Hash, (uint)item.Entity.Value), item.Centre), item.Size), colour);
			groups[groups.Count - 1] = group;
		}
	}

	public void AddLabel(Vector3 position, string text, Color32 colour)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(text) && TryOpenGroup(out var group) && group.Labels < 65535)
		{
			labels.Add(new Label
			{
				Position = position,
				Text = text,
				Colour = colour
			});
			AnchorAt(ref group, position);
			group.Labels++;
			group.Bytes += 24 + text.Length;
			group.Hash = Mix(Mix(Mix(group.Hash, position), text), colour);
			groups[groups.Count - 1] = group;
		}
	}

	public void Begin()
	{
		lines.Clear();
		boxes.Clear();
		labels.Clear();
		groups.Clear();
		keysThisBuild.Clear();
		colourKeysThisBuild.Clear();
		changed.Clear();
		removed.Clear();
	}

	public bool End(Vector3 viewer, ref int allowance)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < groups.Count; i++)
		{
			Group obj = groups[i];
			if (!sent.TryGetValue(obj.Key, out var value) || value != obj.Hash)
			{
				changed.Add(i);
			}
		}
		SortNearestFirst(changed, viewer);
		int j;
		for (j = 0; j < changed.Count; j++)
		{
			if (allowance <= 0)
			{
				break;
			}
			Group obj2 = groups[changed[j]];
			allowance -= obj2.Bytes;
			sent[obj2.Key] = obj2.Hash;
		}
		changed.RemoveRange(j, changed.Count - j);
		foreach (uint key in sent.Keys)
		{
			if (!keysThisBuild.Contains(key))
			{
				removed.Add(key);
			}
		}
		foreach (uint item in removed)
		{
			sent.Remove(item);
		}
		ReleaseUnusedColours();
		if (changed.Count <= 0)
		{
			return removed.Count > 0;
		}
		return true;
	}

	public void Write(NetWrite write)
	{
		write.String(Overlay.Name);
		int num = Mathf.Min(removed.Count, 65535);
		write.UInt16((ushort)num);
		for (int i = 0; i < num; i++)
		{
			write.UInt32(removed[i]);
		}
		int num2 = Mathf.Min(changed.Count, 65535);
		write.UInt16((ushort)num2);
		for (int j = 0; j < num2; j++)
		{
			WriteGroup(write, groups[changed[j]]);
		}
	}

	private void WriteGroup(NetWrite write, Group group)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		write.UInt32(group.Key);
		write.UInt16((ushort)group.Lines);
		for (int i = group.FirstLine; i < group.FirstLine + group.Lines; i++)
		{
			Line line = lines[i];
			write.Vector3(in line.From);
			line = lines[i];
			write.Vector3(in line.To);
			line = lines[i];
			write.Color32(in line.Colour);
		}
		write.UInt16((ushort)group.Boxes);
		for (int j = group.FirstBox; j < group.FirstBox + group.Boxes; j++)
		{
			write.EntityID(boxes[j].Entity);
			Box box = boxes[j];
			write.Vector3(in box.Centre);
			write.Float(boxes[j].Size);
			box = boxes[j];
			write.Color32(in box.Colour);
		}
		write.UInt16((ushort)group.Labels);
		for (int k = group.FirstLabel; k < group.FirstLabel + group.Labels; k++)
		{
			Label label = labels[k];
			write.Vector3(in label.Position);
			write.String(labels[k].Text);
			label = labels[k];
			write.Color32(in label.Colour);
		}
	}

	private static void AnchorAt(ref Group group, Vector3 position)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (group.Lines + group.Boxes + group.Labels == 0)
		{
			group.Anchor = position;
		}
	}

	private void SortNearestFirst(List<int> indices, Vector3 viewer)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 1; i < indices.Count; i++)
		{
			int num = indices[i];
			Vector3 val = groups[num].Anchor - viewer;
			float sqrMagnitude = val.sqrMagnitude;
			int num2;
			for (num2 = i - 1; num2 >= 0; num2--)
			{
				val = groups[indices[num2]].Anchor - viewer;
				if (!(val.sqrMagnitude > sqrMagnitude))
				{
					break;
				}
				indices[num2 + 1] = indices[num2];
			}
			indices[num2 + 1] = num;
		}
	}

	private bool TryOpenGroup(out Group group)
	{
		if (groups.Count == 0)
		{
			group = default;
			return false;
		}
		group = groups[groups.Count - 1];
		return true;
	}

	private int FreeSlot()
	{
		for (int i = 0; i < Palette.Length; i++)
		{
			if (!colourSlots.ContainsValue(i))
			{
				return i;
			}
		}
		return colourSlots.Count % Palette.Length;
	}

	private void ReleaseUnusedColours()
	{
		PooledList<uint> val = Pool.Get<PooledList<uint>>();
		try
		{
			foreach (uint key in colourSlots.Keys)
			{
				if (!colourKeysThisBuild.Contains(key))
				{
					((List<uint>)(object)val).Add(key);
				}
			}
			foreach (uint item in (List<uint>)(object)val)
			{
				colourSlots.Remove(item);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static ulong Mix(ulong hash, uint value)
	{
		return (hash ^ value) * 1099511628211L;
	}

	private static ulong Mix(ulong hash, float value)
	{
		return Mix(hash, (uint)BitConverter.SingleToInt32Bits(value));
	}

	private static ulong Mix(ulong hash, Vector3 value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		return Mix(Mix(Mix(hash, value.x), value.y), value.z);
	}

	private static ulong Mix(ulong hash, Color32 value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Mix(hash, (uint)value.ToInt32());
	}

	private static ulong Mix(ulong hash, string value)
	{
		for (int i = 0; i < value.Length; i++)
		{
			hash = Mix(hash, (uint)value[i]);
		}
		return Mix(hash, (uint)value.Length);
	}

	static DebugDrawOverlayFrame()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
	}
}

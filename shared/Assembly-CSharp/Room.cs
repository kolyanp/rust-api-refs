using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class Room : IPooled
{
	public int Id;

	public int DebugId;

	public ulong Anchor;

	public int NetworkGroupIndex;

	public bool IsOutside;

	public ListHashSet<BlockFace> Faces = new ListHashSet<BlockFace>();

	public List<Portal> Portals = new List<Portal>();

	public int VisibilityGroup;

	public bool EffectivelyOutside;

	public int Generation;

	public uint BuildingId;

	public Bounds Bounds;

	void IPooled.EnterPool()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Id = 0;
		DebugId = 0;
		Anchor = 0uL;
		NetworkGroupIndex = 0;
		IsOutside = false;
		VisibilityGroup = 0;
		EffectivelyOutside = false;
		BuildingId = 0u;
		Bounds = default;
		Faces.Clear();
		Portals.Clear();
		Generation++;
	}

	void IPooled.LeavePool()
	{
	}
}

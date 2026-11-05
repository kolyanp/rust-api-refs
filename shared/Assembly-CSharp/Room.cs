using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class Room : IPooled
{
	public int Id;

	public RoomSolver Solver;

	public int SolverIndex;

	public int DebugId;

	public ulong Anchor;

	public int NetworkGroupIndex;

	public ulong ClaimedAnchor;

	public bool IsOutside;

	public float Outwardness;

	public int OutwardFaces;

	public ListHashSet<BlockFace> Faces = new ListHashSet<BlockFace>();

	public List<Portal> Portals = new List<Portal>();

	public int VisibilityGroup;

	public bool EffectivelyOutside;

	public int Generation;

	public uint BuildingId;

	public Bounds Bounds;

	void IPooled.EnterPool()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		Id = 0;
		Solver = null;
		SolverIndex = 0;
		DebugId = 0;
		Anchor = 0uL;
		NetworkGroupIndex = 0;
		ClaimedAnchor = 0uL;
		IsOutside = false;
		Outwardness = 0f;
		OutwardFaces = 0;
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

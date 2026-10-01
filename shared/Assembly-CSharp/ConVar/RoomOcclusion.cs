using System;
using System.Collections.Generic;
using System.Text;
using Network.Visibility;
using UnityEngine;

namespace ConVar;

[Factory("room")]
public class RoomOcclusion : ConsoleSystem
{
	private struct NetworkGroupDrawCounts
	{
		public int Rooms;

		public int Boundaries;

		public int Portals;
	}

	private const float LiveDrawInterval = 1f;

	private const int MaxLiveFaces = 400;

	private const int MaxLiveNetworkGroups = 200;

	private static float nextLiveDraw;

	private static readonly Color SubscribedGroupColor = new Color(0.3f, 1f, 0.4f);

	private static readonly Color HeldGroupColor = new Color(1f, 0.8f, 0.2f);

	private static readonly Color UnsubscribedGroupColor = new Color(1f, 0.3f, 0.3f);

	private static readonly Color LinkColor = new Color(0.2f, 0.9f, 0.6f);

	private static readonly Color ThroughBlockLinkColor = new Color(0.2f, 0.55f, 1f);

	private const int MaxDebugRows = 64;

	private static readonly Color[] roomColors = new Color[8]
	{
		new Color(1f, 0.3f, 0.3f),
		new Color(0.3f, 1f, 0.4f),
		new Color(0.4f, 0.6f, 1f),
		new Color(1f, 0.9f, 0.2f),
		new Color(1f, 0.4f, 1f),
		new Color(0.3f, 1f, 1f),
		new Color(1f, 0.6f, 0.2f),
		new Color(0.7f, 0.5f, 1f)
	};

	private static readonly Color OutsideRoomColor = new Color(0.75f, 0.75f, 0.75f);

	private const float DefaultDrawRange = 40f;

	private static bool exportingPastes;

	[ServerVar(SavedInEditor = true, Help = "Server-side interior occlusion: 0 = off, 1 = shadow mode (compute + log, hide nothing), 2 = enforce")]
	public static int occlusion = 0;

	[ServerVar(SavedInEditor = true, Help = "Admins and developers bypass interior occlusion and see everything")]
	public static bool occlusion_admin_bypass = true;

	public const int DeployablesOff = 0;

	public const int DeployablesStatic = 1;

	public const int DeployablesWithIO = 2;

	[ServerVar(Help = "What interior occlusion hides inside a room: 0 = nothing (building blocks only), 1 = static entities (boxes, workbenches, beds), 2 = also IO entities (wires and pipes can be dragged to an entity that is not on your client)")]
	public static int occlusion_deployables = 1;

	[ServerVar(Help = "How far from a player interior occlusion looks for rooms to hide things in, in metres. Must comfortably exceed network range; rooms beyond it fail open.")]
	public static float occlusion_range = 250f;

	[ServerVar(Help = "Rooms within this many metres of a player are always sent, whether or not they can see into them. Leaving the radius starts the room.occlusion_linger countdown rather than dropping it at once. 0 = only ever send what is actually visible.")]
	public static float occlusion_nearby_range = 32f;

	[ServerVar(Help = "Kill switch for the whole synchronous pre-open pass: repartition and deliver the rooms behind a door the moment it opens, instead of leaving it to the normal budgeted queue. Only players within room.occlusion_door_immediate_range are flushed; further ones enqueue as usual. Off skips the repartition entirely, not just the flush.")]
	public static bool occlusion_door_immediate = true;

	[ServerVar(Help = "How close a player must be to a door for its rooms to be sent immediately when it opens, in metres. 0 = no limit, flush every viewer of the door.")]
	public static float occlusion_door_immediate_range = 30f;

	[ServerVar(Help = "How long a room must have been out of sight before it is dropped from a player's client, in seconds. Stops a door being opened and shut, or a walk past a doorway, from destroying and re-sending a whole room. 0 = drop immediately.")]
	public static float occlusion_linger = 5f;

	[ServerVar(Help = "Continuously draw the room partition around every player, refreshing as the base changes. Needs room.occlusion on (1 = shadow mode draws without hiding anything). Editor/dev tool - it redraws for everyone connected, so do not leave it on.")]
	public static bool draw_live = false;

	[ServerVar(Help = "Radius of the room.draw_live overlay, in metres")]
	public static float draw_live_range = 30f;

	[ServerVar(Help = "Draw the normal arrows and face outlines of the room.draw_live overlay (off = room-coloured outlines only, much less traffic)")]
	public static bool draw_live_normals = true;

	[ServerVar(Help = "Continuously draw room contents, boundary groups and portal subscription state around every player. Needs room.occlusion on. Editor/dev tool - do not leave it on.")]
	public static bool draw_networkgroups_live = false;

	public static void DrawLiveTick()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		if ((!draw_live && !draw_networkgroups_live) || Time.realtimeSinceStartup < nextLiveDraw)
		{
			return;
		}
		nextLiveDraw = Time.realtimeSinceStartup + 1f;
		RoomSolver solver = RoomOcclusionManager.Solver;
		if (solver == null)
		{
			return;
		}
		float rangeSqr = draw_live_range * draw_live_range;
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if ((Object)(object)current == (Object)null || !current.IsConnected)
				{
					continue;
				}
				if (draw_live)
				{
					Vector3 position = current.eyes.position;
					int num = 0;
					foreach (Room room in solver.Rooms)
					{
						Color color = RoomColor(room);
						string label = room.DebugId.ToString();
						Enumerator<BlockFace> enumerator3 = room.Faces.GetEnumerator();
						try
						{
							while (enumerator3.MoveNext())
							{
								BlockFace current3 = enumerator3.Current;
								if (InRange(current3.WorldCenter, position, rangeSqr))
								{
									if (num++ >= 400)
									{
										break;
									}
									current3.DDraw(current, color, 1.15f, draw_live_normals, label);
								}
							}
						}
						finally
						{
							((IDisposable)enumerator3/*cast due to constrained. prefix*/).Dispose();
						}
						if (num >= 400)
						{
							break;
						}
					}
				}
				if (draw_networkgroups_live)
				{
					DrawNetworkGroups(current, solver.Rooms, 1.15f, draw_live_range, 200);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private static NetworkGroupDrawCounts DrawNetworkGroups(BasePlayer player, IEnumerable<Room> rooms, float duration, float range, int maxDraws = int.MaxValue)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = player.eyes.position;
		float num = range * range;
		HashSet<Room> hashSet = new HashSet<Room>();
		NetworkGroupDrawCounts result = default;
		foreach (Room room in rooms)
		{
			hashSet.Add(room);
			if (result.Rooms + result.Boundaries + result.Portals < maxDraws && !(room.Bounds.SqrDistance(position) > num))
			{
				Group obj = RoomNetworkGroups.Resolve(room.NetworkGroupIndex);
				Color color = NetworkGroupColor(player, obj);
				string text = $"R{room.DebugId} RG{room.VisibilityGroup}\nNG {NetworkGroupLabel(room.NetworkGroupIndex, obj, player)}";
				UnityEngine.DDraw.Bounds(player, room.Bounds, color, duration);
				UnityEngine.DDraw.Text(player, room.Bounds.center, text, color, duration, distanceFade: true, zTest: true, 1.5f);
				result.Rooms++;
			}
		}
		HashSet<BuildingBlock> hashSet2 = new HashSet<BuildingBlock>();
		foreach (Room item2 in hashSet)
		{
			Enumerator<BlockFace> enumerator3 = item2.Faces.GetEnumerator();
			try
			{
				while (enumerator3.MoveNext())
				{
					if (enumerator3.Current.Entity is BuildingBlock item)
					{
						hashSet2.Add(item);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator3/*cast due to constrained. prefix*/).Dispose();
			}
		}
		foreach (BuildingBlock item3 in hashSet2)
		{
			if (result.Rooms + result.Boundaries + result.Portals >= maxDraws)
			{
				break;
			}
			if (!InRange(item3.CenterPoint(), position, num) || !TryGetBoundaryGroup(item3, out var index))
			{
				continue;
			}
			Group obj2 = RoomNetworkGroups.Resolve(index);
			Color color2 = NetworkGroupColor(player, obj2);
			string text2 = "B " + NetworkGroupLabel(index, obj2, player);
			bool flag = false;
			foreach (BlockFace face in item3.faces)
			{
				face.DDraw(player, color2, duration, drawNormal: false, flag ? null : text2);
				flag = true;
			}
			result.Boundaries++;
		}
		List<Portal> portals = RoomOcclusionManager.Portals;
		if (portals != null)
		{
			foreach (Portal item4 in portals)
			{
				if (result.Rooms + result.Boundaries + result.Portals >= maxDraws)
				{
					break;
				}
				if ((hashSet.Contains(item4.RoomA) || hashSet.Contains(item4.RoomB)) && (InRange(item4.FaceA.WorldCenter, position, num) || InRange(item4.FaceB.WorldCenter, position, num)))
				{
					bool flag2 = item4.IsOpen();
					Color color3 = (flag2 ? SubscribedGroupColor : UnsubscribedGroupColor);
					Vector3 val = item4.FaceA.WorldCenter + item4.FaceA.WorldNormal * 0.2f;
					Vector3 val2 = item4.FaceB.WorldCenter + item4.FaceB.WorldNormal * 0.2f;
					Vector3 pos = (val + val2) * 0.5f;
					UnityEngine.DDraw.Line(player, val, val2, color3, duration, distanceFade: true, zTest: false);
					UnityEngine.DDraw.Text(player, pos, string.Format("R{0} C{1} <-> R{2} C{3}\n", new object[4]
					{
						item4.RoomA.DebugId,
						item4.RoomA.NetworkGroupIndex,
						item4.RoomB.DebugId,
						item4.RoomB.NetworkGroupIndex
					}) + (flag2 ? $"OPEN: V{item4.RoomA.VisibilityGroup}" : "closed"), color3, duration, distanceFade: true, zTest: false, 1.25f);
					result.Portals++;
				}
			}
		}
		return result;
	}

	private static bool TryGetBoundaryGroup(BuildingBlock block, out int index)
	{
		index = 0;
		ulong num = 0uL;
		bool flag = false;
		foreach (BlockFace face in block.faces)
		{
			Room room = face.Room;
			if (room == null || room.EffectivelyOutside || room.Anchor == 0L)
			{
				return false;
			}
			if (num == 0L)
			{
				num = room.Anchor;
			}
			else if (num != room.Anchor)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return false;
		}
		index = RoomNetworkGroups.GroupForBlock(block.faces);
		return index != 0;
	}

	private static Color NetworkGroupColor(BasePlayer player, Group group)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (group == null)
		{
			return OutsideRoomColor;
		}
		if (player.net?.subscriber?.IsSubscribed(group) == true)
		{
			return SubscribedGroupColor;
		}
		if (player.net?.roomGroups?.Contains(group) == true)
		{
			return HeldGroupColor;
		}
		return UnsubscribedGroupColor;
	}

	private static string NetworkGroupLabel(int index, Group group, BasePlayer player)
	{
		if (group == null)
		{
			return "grid";
		}
		string arg;
		if (player.net?.subscriber?.IsSubscribed(group) == true)
		{
			arg = "true";
		}
		else
		{
			arg = ((player.net?.roomGroups?.Contains(group) == true) ? "pending" : "false");
		}
		return $"{index} {arg}";
	}

	private static void DrawFaceLink(BasePlayer player, BlockFace face, BlockFace otherFace, float duration)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = face.WorldCenter + face.WorldNormal * 0.22f;
		Vector3 val2 = otherFace.WorldCenter + otherFace.WorldNormal * 0.22f;
		bool flag = GamePhysics.LineOfSight(val, val2, 2097152);
		UnityEngine.DDraw.Arrow(player, val, val2, flag ? LinkColor : ThroughBlockLinkColor, duration, 0.06f, distanceFade: true, flag);
	}

	private static string SnapshotRooms()
	{
		if (RoomOcclusionManager.PartitionIsLive)
		{
			return "";
		}
		RoomOcclusionManager.RebuildForDebug();
		return $"\n(room.occlusion is {occlusion}: one-off snapshot, the partition is not being maintained. Set room.occlusion 1 for shadow mode.)";
	}

	private static Color RoomColor(Room room)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (!room.IsOutside)
		{
			return roomColors[room.DebugId % roomColors.Length];
		}
		return OutsideRoomColor;
	}

	private static string DrawLegend(int drawn, int skipped, float range)
	{
		string arg = ((skipped > 0) ? $", {skipped} beyond {range:0}m" : "");
		return $"\ndrew {drawn} faces{arg}. Arrows point INTO the room the face borders; outlines are depth tested, so you only see the faces on your side of a wall.";
	}

	private static bool InRange(Vector3 point, Vector3 eye, float rangeSqr)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = point - eye;
		return val.sqrMagnitude <= rangeSqr;
	}

	[ServerVar(Help = "Prints the room partition of the building you are looking at")]
	public static void printrooms(Arg arg)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		if (!TryTraceBlock(arg, out var block, out var error))
		{
			arg.ReplyWith(error);
			return;
		}
		string text = SnapshotRooms();
		TextTable val = new TextTable();
		val.AddColumns(new string[5] { "Room", "Outside", "Faces", "Group", "Eff. outside" });
		HashSet<Room> hashSet = new HashSet<Room>();
		CollectBuildingRooms(block, hashSet);
		foreach (Room item in hashSet)
		{
			val.AddRow(new string[5]
			{
				item.DebugId.ToString(),
				item.IsOutside ? "x" : "",
				item.Faces.Count.ToString(),
				item.VisibilityGroup.ToString(),
				item.EffectivelyOutside ? "x" : ""
			});
		}
		arg.ReplyWith(string.Format("'{0}' (building {1}): {2} rooms\n{3}{4}", new object[5] { block.ShortPrefabName, block.buildingID, hashSet.Count, val, text }));
	}

	[ServerVar(Help = "Draws the faces of the building you are looking at coloured by room. Args: [duration=10] [range=40]. Outside room is dim white; arrows point into the room each face borders.")]
	public static void draw_rooms(Arg arg)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!TryTraceBlock(arg, out var block, out var error))
		{
			arg.ReplyWith(error);
			return;
		}
		float duration = arg.GetFloat(0, 10f);
		float num = arg.GetFloat(1, 40f);
		string text = SnapshotRooms();
		HashSet<Room> hashSet = new HashSet<Room>();
		CollectBuildingRooms(block, hashSet);
		Vector3 position = basePlayer.eyes.position;
		float rangeSqr = num * num;
		int num2 = 0;
		int num3 = 0;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Room item in hashSet)
		{
			Color color = RoomColor(item);
			stringBuilder.AppendLine(string.Format("room {0}: {1} faces{2}{3}", new object[4]
			{
				item.DebugId,
				item.Faces.Count,
				item.IsOutside ? " (OUTSIDE, drawn dim white)" : "",
				(item.EffectivelyOutside && !item.IsOutside) ? " (effectively outside)" : ""
			}));
			Enumerator<BlockFace> enumerator2 = item.Faces.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					BlockFace current2 = enumerator2.Current;
					if (!InRange(current2.WorldCenter, position, rangeSqr))
					{
						num3++;
						continue;
					}
					current2.DDraw(basePlayer, color, duration, drawNormal: true, item.DebugId.ToString());
					num2++;
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
			}
		}
		arg.ReplyWith(string.Format("{0} rooms\n{1}{2}{3}", new object[4]
		{
			hashSet.Count,
			stringBuilder,
			DrawLegend(num2, num3, num),
			text
		}));
	}

	[ServerVar(Help = "Prints every portal of the building you are looking at (rooms joined, filler, open state)")]
	public static void printportals(Arg arg)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected Obj, but got Unknown
		if (!TryTraceBlock(arg, out var block, out var error))
		{
			arg.ReplyWith(error);
			return;
		}
		string arg2 = SnapshotRooms();
		HashSet<Room> hashSet = new HashSet<Room>();
		CollectBuildingRooms(block, hashSet);
		TextTable val = new TextTable();
		val.AddColumns(new string[4] { "Rooms", "Filler", "Blocks vision", "Open" });
		List<Portal> portals = RoomOcclusionManager.Portals;
		if (portals != null)
		{
			foreach (Portal item in portals)
			{
				if (hashSet.Contains(item.RoomA) || hashSet.Contains(item.RoomB))
				{
					val.AddRow(new string[4]
					{
						$"{item.RoomA.DebugId} <-> {item.RoomB.DebugId}",
						item.PermanentlyOpen ? "(low wall)" : (item.Filler?.ShortPrefabName ?? "(empty)"),
						item.FillerBlocksVision ? "x" : "",
						item.IsOpen() ? "x" : ""
					});
				}
			}
		}
		arg.ReplyWith($"{val}{arg2}");
	}

	[ServerVar(Help = "Draws the portals of the building you are looking at (green = open, red = closed)")]
	public static void draw_portals(Arg arg)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = ArgEx.Player(arg);
		if (!TryTraceBlock(arg, out var block, out var error))
		{
			arg.ReplyWith(error);
			return;
		}
		float duration = arg.GetFloat(0, 10f);
		string arg2 = SnapshotRooms();
		HashSet<Room> hashSet = new HashSet<Room>();
		CollectBuildingRooms(block, hashSet);
		int num = 0;
		List<Portal> portals = RoomOcclusionManager.Portals;
		if (portals != null)
		{
			foreach (Portal item in portals)
			{
				if (hashSet.Contains(item.RoomA) || hashSet.Contains(item.RoomB))
				{
					bool flag = item.IsOpen();
					Color color = (flag ? new Color(0.3f, 1f, 0.4f) : new Color(1f, 0.3f, 0.3f));
					item.FaceA.DDraw(player, color, duration, drawNormal: false);
					item.FaceB.DDraw(player, color, duration, drawNormal: false);
					Vector3 val = item.FaceA.WorldCenter + item.FaceA.WorldNormal * 0.2f;
					Vector3 val2 = item.FaceB.WorldCenter + item.FaceB.WorldNormal * 0.2f;
					Vector3 pos = (val + val2) * 0.5f;
					UnityEngine.DDraw.Line(player, val, val2, color, duration, distanceFade: true, zTest: false);
					UnityEngine.DDraw.Sphere(player, pos, 0.12f, color, duration, distanceFade: true, zTest: false);
					string arg3;
					if (item.PermanentlyOpen)
					{
						arg3 = "always open";
					}
					else
					{
						arg3 = (((Object)(object)item.Filler == (Object)null) ? "empty frame" : (item.Filler.ShortPrefabName + ": " + (flag ? "open" : "SHUT")));
					}
					UnityEngine.DDraw.Text(player, pos, $"{item.RoomA.DebugId}<->{item.RoomB.DebugId}\n{arg3}", color, duration, distanceFade: true, zTest: false, 1.5f);
					num++;
				}
			}
		}
		arg.ReplyWith($"{num} portals (green = see-through, red = blocked){arg2}");
	}

	[ServerVar(Help = "Prints the room a player is standing in and every room they can see into. Defaults to yourself.")]
	public static void printvisible(Arg arg)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected Obj, but got Unknown
		BasePlayer basePlayer = ArgEx.GetPlayer(arg, 0) ?? ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("No player - pass a name or steamid");
			return;
		}
		string text = SnapshotRooms();
		RoomSolver solver = RoomOcclusionManager.Solver;
		if (solver == null)
		{
			arg.ReplyWith("Room system is not running (the face graph could not be built - see the server log)");
			return;
		}
		Room roomForPlayer = RoomEntityBridge.GetRoomForPlayer(basePlayer);
		if (roomForPlayer == null)
		{
			arg.ReplyWith(basePlayer.displayName + " is out in the world - no room resolved under them, so they see everything that is effectively outside and nothing sealed" + text);
			return;
		}
		TextTable val = new TextTable();
		val.AddColumns(new string[4] { "Room", "Outside", "Faces", "Portals" });
		int num = 0;
		foreach (Room room in solver.Rooms)
		{
			if (RoomVisibility.CanSee(roomForPlayer, room))
			{
				num++;
				if (num <= 64)
				{
					val.AddRow(new string[4]
					{
						(room == roomForPlayer) ? $"{room.DebugId} (here)" : room.DebugId.ToString(),
						room.IsOutside ? "x" : "",
						room.Faces.Count.ToString(),
						room.Portals.Count.ToString()
					});
				}
			}
		}
		string text2 = ((num > 64) ? $" (showing first {64})" : "");
		arg.ReplyWith(string.Format("{0} in room {1} (group {2}{3}), sees {4} of {5} rooms{6}\n{7}{8}", new object[9]
		{
			basePlayer.displayName,
			roomForPlayer.DebugId,
			roomForPlayer.VisibilityGroup,
			roomForPlayer.EffectivelyOutside ? ", effectively outside" : "",
			num,
			solver.Rooms.Count,
			text2,
			val,
			text
		}));
	}

	[ServerVar(Help = "Draws the faces of every room you can see into from where you stand (green), and the rooms you cannot (red)")]
	public static void draw_visible(Arg arg)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Server console cannot draw");
			return;
		}
		string text = SnapshotRooms();
		RoomSolver solver = RoomOcclusionManager.Solver;
		if (solver == null)
		{
			arg.ReplyWith("Room system is not running (the face graph could not be built - see the server log)");
			return;
		}
		float duration = arg.GetFloat(0, 10f);
		float num = arg.GetFloat(1, 60f);
		Room roomForPlayer = RoomEntityBridge.GetRoomForPlayer(basePlayer);
		float rangeSqr = num * num;
		Vector3 position = basePlayer.eyes.position;
		Color val = new Color(0.3f, 1f, 0.4f);
		Color val2 = new Color(1f, 0.3f, 0.3f);
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		foreach (Room room in solver.Rooms)
		{
			bool flag = RoomVisibility.CanSeeFrom(roomForPlayer, room);
			if (!flag)
			{
				num4++;
			}
			Color color = (flag ? val : val2);
			Enumerator<BlockFace> enumerator2 = room.Faces.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					BlockFace current2 = enumerator2.Current;
					if (!InRange(current2.WorldCenter, position, rangeSqr))
					{
						num3++;
						continue;
					}
					current2.DDraw(basePlayer, color, duration, drawNormal: true, flag ? null : room.DebugId.ToString());
					num2++;
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
			}
		}
		arg.ReplyWith("standing in " + ((roomForPlayer == null) ? "no room (out in the world)" : ("room " + roomForPlayer.DebugId)) + ": " + $"{num4} of {solver.Rooms.Count} rooms are hidden from here (red, labelled with their room id; green = visible)" + DrawLegend(num2, num3, num) + text);
	}

	[ServerVar(Help = "Editor-only: exports the current room partition to RoomFixtures/<name>.json so it can be replayed offline in the edit-mode tests. Pass a name; defaults to 'capture'. To capture a base from a live server, copypaste it into an editor server first.")]
	public static void exportbase(Arg arg)
	{
		arg.ReplyWith("room.exportbase is editor only - copypaste the base into an editor server and export it there");
	}

	[ServerVar(Help = "Editor-only: pastes every .data file in a directory one at a time and exports each to RoomFixtures/<folder>/<paste>.json for the edit-mode fixture tests. Args: <directory> [folder=directory name]. Pastes that already have a fixture are skipped.")]
	public static void exportpastes(Arg arg)
	{
		arg.ReplyWith("room.exportpastes is editor only - copypaste the bases into an editor server and export them there");
	}

	[ServerVar(Help = "Prints which interior rooms a player can currently see into, i.e. which room network groups they hold. Defaults to yourself.")]
	public static void printhidden(Arg arg)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected Obj, but got Unknown
		BasePlayer basePlayer = ArgEx.GetPlayer(arg, 0) ?? ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("No player - pass a name or steamid");
			return;
		}
		ListHashSet<Group> val = basePlayer.net?.roomGroups;
		RoomSolver solver = RoomOcclusionManager.Solver;
		if (val == null || val.Count == 0 || solver == null)
		{
			arg.ReplyWith(basePlayer.displayName + ": holds no room groups - sees only what is on the grid" + string.Format(" (occlusion {0}{1})", occlusion, RoomOcclusionManager.FailedOpen ? ", solver failed open" : ""));
			return;
		}
		TextTable val2 = new TextTable();
		val2.AddColumns(new string[4] { "Room", "Vis group", "Net group", "Entities" });
		int num = 0;
		foreach (Room room in solver.Rooms)
		{
			Group obj = RoomNetworkGroups.Resolve(room.NetworkGroupIndex);
			if (obj != null && val.Contains(obj) && num++ < 64)
			{
				val2.AddRow(new string[4]
				{
					room.DebugId.ToString(),
					room.VisibilityGroup.ToString(),
					obj.ID.ToString(),
					(obj.networkables?.Count ?? 0).ToString()
				});
			}
		}
		string arg2 = ((num > 64) ? $" (showing first {64})" : "");
		arg.ReplyWith($"{basePlayer.displayName}: {val.Count} room groups held, {num} of them a room's contents" + $" (the rest are the walls between them){arg2}\n{val2}");
	}

	[ServerVar(Help = "Draws room contents and boundary network groups for the building you are looking at. Green = subscribed, yellow = held but awaiting subscription, red = not held, grey = positional grid. Args: [duration=10] [range=40].")]
	public static void draw_networkgroups(Arg arg)
	{
		BasePlayer player = ArgEx.Player(arg);
		if (!TryTraceBlock(arg, out var block, out var error))
		{
			arg.ReplyWith(error);
			return;
		}
		float duration = arg.GetFloat(0, 10f);
		float range = arg.GetFloat(1, 40f);
		string text = SnapshotRooms();
		HashSet<Room> rooms = new HashSet<Room>();
		CollectBuildingRooms(block, rooms);
		NetworkGroupDrawCounts networkGroupDrawCounts = DrawNetworkGroups(player, rooms, duration, range);
		arg.ReplyWith($"{networkGroupDrawCounts.Rooms} room contents, {networkGroupDrawCounts.Boundaries} boundary groups and {networkGroupDrawCounts.Portals} portals drawn" + "\nC = contents group, B = boundary group. Green = subscribed, yellow = held but not subscribed, red = not held, grey = positional grid." + text);
	}

	[ServerVar(Help = "Prints interior occlusion's shadow-mode report: how much of the world ended up effectively outside (fail open) and how much each player has hidden")]
	public static void occlusionstats(Arg arg)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected Obj, but got Unknown
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		string arg2 = SnapshotRooms();
		RoomSolver solver = RoomOcclusionManager.Solver;
		if (solver == null)
		{
			arg.ReplyWith("Room system is not running (the face graph could not be built - see the server log)");
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		foreach (Room room in solver.Rooms)
		{
			if (room.Faces.Count != 0)
			{
				num++;
				num3 += room.Faces.Count;
				if (room.EffectivelyOutside)
				{
					num2++;
					num4 += room.Faces.Count;
				}
			}
		}
		float num5 = ((num3 == 0) ? 0f : ((float)num4 / (float)num3));
		TextTable val = new TextTable();
		val.AddColumns(new string[4] { "Player", "Room", "Vis group", "Room groups held" });
		int num6 = 0;
		Enumerator<BasePlayer> enumerator2 = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator2.MoveNext())
			{
				BasePlayer current2 = enumerator2.Current;
				if (!((Object)(object)current2 == (Object)null) && current2.IsConnected)
				{
					num6++;
					if (num6 <= 64)
					{
						Room roomForPlayer = RoomEntityBridge.GetRoomForPlayer(current2);
						val.AddRow(new string[4]
						{
							current2.displayName,
							roomForPlayer?.DebugId.ToString() ?? "(world)",
							roomForPlayer?.VisibilityGroup.ToString() ?? "-",
							(current2.net?.roomGroups?.Count).GetValueOrDefault().ToString()
						});
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
		}
		arg.ReplyWith(string.Format("occlusion {0} ({1})", occlusion, (occlusion == 0) ? "off" : ((occlusion == 1) ? "shadow" : "enforce")) + $", solver failed open: {RoomOcclusionManager.FailedOpen}, delivery failed: {RoomOcclusionDelivery.Failed}\n" + string.Format("{0} rooms ({1} effectively outside), {2} faces ({3} effectively outside)\n", new object[4] { num, num2, num3, num4 }) + $"fail-open rate: {num5:P1}, geometry components with no outward-facing room: {RoomOcclusionManager.FailedOpenBuildings}\n" + $"network groups: {RoomNetworkGroups.ContentsGroupCount} contents + {RoomNetworkGroups.BoundaryGroupCount} boundary" + $" ({RoomNetworkGroups.AllocatedGroups} ever allocated, {RoomNetworkGroups.MultiRoomBlocks} cumulative 3+ room blocks)\n" + $"deployables occupying more than one room: {RoomNetworkGroups.SpanningDeployables} cumulative\n" + $"{RoomOcclusionDelivery.GroupedEntities} entities currently in a room group\n" + $"{num6} players tracked\n{val}{arg2}");
	}

	private static void CollectBuildingRooms(BuildingBlock startBlock, HashSet<Room> rooms)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		RoomSolver solver = RoomOcclusionManager.Solver;
		if (solver == null)
		{
			return;
		}
		uint buildingID = startBlock.buildingID;
		foreach (Room room in solver.Rooms)
		{
			if (room.Faces.Count == 0)
			{
				continue;
			}
			Enumerator<BlockFace> enumerator2 = room.Faces.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					if (enumerator2.Current.Entity is DecayEntity decayEntity && decayEntity.buildingID == buildingID)
					{
						rooms.Add(room);
						break;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private static BuildingBlock TraceBlock(Arg arg)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return null;
		}
		return GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, basePlayer.eyes.BodyRay(), 0f, 20f, 2097152, (QueryTriggerInteraction)1) as BuildingBlock;
	}

	private static string DescribeTraceFailure(Arg arg, BuildingBlock startBlock)
	{
		if ((Object)(object)ArgEx.Player(arg) == (Object)null)
		{
			return "Run this in-game: it works on the block you are looking at";
		}
		if ((Object)(object)startBlock == (Object)null)
		{
			return "Not looking at a building block (aim at a wall, floor or foundation within 20m)";
		}
		return "'" + startBlock.ShortPrefabName + "' has no authored faces - it is one of the blocks the room system ignores (stairs, ramps, roofs, external walls). Aim at a wall, floor or foundation instead.";
	}

	private static bool TryTraceBlock(Arg arg, out BuildingBlock block, out string error)
	{
		block = TraceBlock(arg);
		if ((Object)(object)block == (Object)null || block.faces == null)
		{
			error = DescribeTraceFailure(arg, block);
			return false;
		}
		error = null;
		return true;
	}

	[ServerVar(Help = "Prints the closest-face link of every edge on the building block you are looking at. Pass --ddraw to also draw the links.")]
	public static void printfaces(Arg arg)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected Obj, but got Unknown
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = ArgEx.Player(arg);
		BuildingBlock buildingBlock = TraceBlock(arg);
		if ((Object)(object)buildingBlock == (Object)null)
		{
			arg.ReplyWith("Not looking at a building block");
			return;
		}
		if (buildingBlock.faces == null)
		{
			arg.ReplyWith("'" + buildingBlock.ShortPrefabName + "' has no authored faces");
			return;
		}
		bool flag = arg.HasArg("--ddraw", remove: true);
		float duration = arg.GetFloat(0, 10f);
		TextTable val = new TextTable();
		val.AddColumns(new string[8] { "Entity", "Face", "Edge", "Entity 2", "Block 2", "Face 2", "Dist", "Room" });
		Dictionary<BaseEntity, int> entityIds = new Dictionary<BaseEntity, int>();
		foreach (BlockFace face in buildingBlock.faces)
		{
			int num = GetEntityId(face.Entity);
			foreach (EdgeLink edge in face.Edges)
			{
				BlockFace closestFace = edge.ClosestFace;
				if (closestFace == null)
				{
					val.AddRow(new string[8]
					{
						num.ToString(),
						face.FacePrefab.localName,
						edge.EdgePrefab.localName,
						"",
						"(none)",
						"",
						"",
						""
					});
					continue;
				}
				float num2 = Vector3.Distance(face.WorldCenter, closestFace.WorldCenter);
				val.AddRow(new string[8]
				{
					num.ToString(),
					face.FacePrefab.localName,
					edge.EdgePrefab.localName,
					GetEntityId(closestFace.Entity).ToString(),
					closestFace.Entity.ShortPrefabName,
					closestFace.FacePrefab.localName,
					string.Format("{0:0.00}{1}", num2, (num2 > 3.5f) ? " !" : ""),
					closestFace.Room?.DebugId.ToString() ?? "-"
				});
				if (flag)
				{
					DrawFaceLink(player, face, closestFace, duration);
				}
			}
		}
		if (flag)
		{
			foreach (KeyValuePair<BaseEntity, int> item in entityIds)
			{
				UnityEngine.DDraw.Text(player, item.Key.CenterPoint(), item.Value.ToString(), Color.white, duration, distanceFade: true, zTest: true);
			}
		}
		arg.ReplyWith($"{val}\nDist is between face centres: a link only ever reaches the next block, so anything marked ! is suspect. Links drawn green when both faces can see each other, blue when the link passes through a block.");
		int GetEntityId(BaseEntity entity)
		{
			if (!entityIds.TryGetValue(entity, out var value))
			{
				value = entityIds.Count + 1;
				entityIds.Add(entity, value);
			}
			return value;
		}
	}

	[ServerVar(Help = "Draws the face outlines and closest-face links of the building block you are looking at and everything connected to it (breadth-first walk). Args: [duration=10] [range=40].")]
	public static void draw_faces(Arg arg)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		BuildingBlock buildingBlock = TraceBlock(arg);
		if ((Object)(object)buildingBlock == (Object)null)
		{
			arg.ReplyWith("Not looking at a building block");
			return;
		}
		if (buildingBlock.faces == null)
		{
			arg.ReplyWith("'" + buildingBlock.ShortPrefabName + "' has no authored faces");
			return;
		}
		float duration = arg.GetFloat(0, 10f);
		float num = arg.GetFloat(1, 40f);
		Vector3 position = basePlayer.eyes.position;
		float rangeSqr = num * num;
		HashSet<BlockFace> hashSet = new HashSet<BlockFace>();
		Queue<BlockFace> queue = new Queue<BlockFace>();
		foreach (BlockFace face in buildingBlock.faces)
		{
			if (hashSet.Add(face))
			{
				queue.Enqueue(face);
			}
		}
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		Color white = Color.white;
		Color val = new Color(1f, 0.7f, 0.2f);
		BlockFace result;
		while (queue.TryDequeue(out result))
		{
			bool flag = InRange(result.WorldCenter, position, rangeSqr);
			if (flag)
			{
				bool flag2 = (Object)(object)result.Entity == (Object)(object)buildingBlock;
				Color color;
				if (flag2)
				{
					color = white;
				}
				else
				{
					color = ((result.Room != null) ? RoomColor(result.Room) : val);
				}
				result.DDraw(basePlayer, color, duration, drawNormal: true, flag2 ? result.FacePrefab.localName : null);
				num2++;
			}
			else
			{
				num3++;
			}
			foreach (EdgeLink edge in result.Edges)
			{
				if (edge.ClosestFace == null)
				{
					num5++;
					continue;
				}
				if (flag)
				{
					DrawFaceLink(basePlayer, result, edge.ClosestFace, duration);
					num4++;
				}
				if (hashSet.Add(edge.ClosestFace))
				{
					queue.Enqueue(edge.ClosestFace);
				}
			}
		}
		arg.ReplyWith(string.Format("'{0}' (white, faces labelled): {1} connected faces, {2} links drawn, {3} edges with no neighbour", new object[4] { buildingBlock.ShortPrefabName, hashSet.Count, num4, num5 }) + "\ncolour = room where one is known, amber where it is not" + DrawLegend(num2, num3, num));
	}

	static RoomOcclusion()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
	}
}

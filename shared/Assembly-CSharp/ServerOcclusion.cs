using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using ConVar;
using Facepunch;
using Network.Visibility;
using ServerOcclusionJobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class ServerOcclusion
{
	public class Group : ListHashSet<BaseNetworkable>, IPooled
	{
		void IPooled.EnterPool()
		{
			base.Clear();
		}

		void IPooled.LeavePool()
		{
		}
	}

	public readonly struct Grid(int x, int y, int z) : IEquatable<Grid>
	{
		public readonly int x = x;

		public readonly int y = y;

		public readonly int z = z;

		public const float Resolution = 16f;

		public const float HalfResolution = 8f;

		public static Grid FromIndex(int index)
		{
			int num = index / (ChunkCountX * ChunkCountY);
			index -= num * (ChunkCountX * ChunkCountY);
			int num2 = index / ChunkCountX;
			index -= num2 * ChunkCountX;
			return new Grid(index, num2, num);
		}

		public static int GetOffset(float axis)
		{
			return Mathf.RoundToInt(axis / 2f / 16f);
		}

		public Vector3 GetCenterPoint()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3((float)(x - GetOffset(TerrainMeta.Size.x)) * 16f, (float)(y - GetOffset(MaxY)) * 16f, (float)(z - GetOffset(TerrainMeta.Size.z)) * 16f);
		}

		public override string ToString()
		{
			return $"(x: {x}, y: {y}, z: {z})";
		}

		public bool Equals(Grid other)
		{
			if (x == other.x && y == other.y)
			{
				return z == other.z;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(x, y, z);
		}

		public bool IsBlocked()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			return GamePhysics.CheckBounds(new Bounds(GetCenterPoint(), new Vector3(16f, 16f, 16f)), 8388608, (QueryTriggerInteraction)0);
		}

		public int GetIndex()
		{
			return GetGridIndex(x, y, z);
		}
	}

	public readonly struct SubGrid : IEquatable<SubGrid>
	{
		public readonly int x;

		public readonly int y;

		public readonly int z;

		public const float Resolution = 2f;

		public const float HalfResolution = 1f;

		public SubGrid(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public SubGrid(int3 p)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			x = p.x;
			y = p.y;
			z = p.z;
		}

		public static int GetOffset(float axis)
		{
			return Mathf.RoundToInt(axis / 2f / 2f);
		}

		public Vector3 GetCenterPoint()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			return new Vector3((float)(x - GetOffset(TerrainMeta.Size.x)) * 2f, (float)(y - GetOffset(MaxY)) * 2f, (float)(z - GetOffset(TerrainMeta.Size.z)) * 2f);
		}

		public override string ToString()
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			return string.Format("(x: {0}, y: {1}, z: {2}) - {3}, {4}", new object[5]
			{
				x,
				y,
				z,
				GetCenterPoint(),
				IsBlocked()
			});
		}

		public bool Equals(SubGrid other)
		{
			if (x == other.x && y == other.y)
			{
				return z == other.z;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(x, y, z);
		}

		public bool IsBlocked()
		{
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			Vector3[] gridOffsets;
			if (OcclusionIncludeRocks)
			{
				bool flag = true;
				gridOffsets = GridOffsets;
				foreach (Vector3 val in gridOffsets)
				{
					if (!flag)
					{
						break;
					}
					Vector3 pos = GetCenterPoint() + val;
					flag &= AntiHack.IsInsideMesh(pos);
					if (flag)
					{
						GameObject gameObject = ((Component)((RaycastHit)(ref AntiHack.isInsideRayHit)).collider).gameObject;
						flag &= gameObject.HasCustomTag(GameObjectTag.AllowBarricadePlacement);
					}
				}
				if (flag)
				{
					return true;
				}
			}
			gridOffsets = GridOffsets;
			foreach (Vector3 val2 in gridOffsets)
			{
				if (AntiHack.TestInsideTerrain(GetCenterPoint() + val2))
				{
					return true;
				}
			}
			return false;
		}

		public int GetIndex()
		{
			return GetSubGridIndex(x, y, z);
		}

		public int GetDistance(SubGrid other)
		{
			return Mathf.Abs(x - other.x) + Mathf.Abs(y - other.y) + Mathf.Abs(z - other.z);
		}
	}

	public const int CacheVersion = 3;

	public static int MaxY;

	public static int ChunkCountX;

	public static int ChunkCountY;

	public static int ChunkCountZ;

	public static int SubChunkCountX;

	public static int SubChunkCountY;

	public static int SubChunkCountZ;

	public static float AxisX;

	public static float AxisY;

	public static float AxisZ;

	public static LimitDictionary<(int, int), bool> OcclusionCache;

	public static NativeArray<NativeBitArray> OcclusionSubGridBlocked;

	public static NativeReference<bool> ReturnHolder;

	public const int OcclusionChunkSize = 16;

	public const int OcclusionChunkResolution = 8;

	public static Dictionary<Network.Visibility.Group, Group> Occludees;

	public static readonly Vector3[] GridOffsets;

	public static readonly (int, int, int)[] neighbours;

	public static bool OcclusionEnabled { get; set; }

	public static bool OcclusionIncludeRocks { get; set; }

	public static float OcclusionPollRate => 2f;

	public static int MinOcclusionDistance => 25;

	public static string SubGridFilePath
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		get
		{
			return string.Format("{0}/{1}_occlusion_{2}.dat", Server.rootFolder, World.MapFileName.Replace(".map", ""), 3);
		}
	}

	public static int GetGridIndex(int x, int y, int z)
	{
		return z * ChunkCountX * ChunkCountY + y * ChunkCountX + x;
	}

	public static int GetSubGridIndex(int x, int y, int z)
	{
		return z * SubChunkCountX * SubChunkCountY + y * SubChunkCountX + x;
	}

	public static int GetGrid(float position, float axis)
	{
		return Mathf.RoundToInt(position / 16f + axis / 16f);
	}

	public static Grid GetGrid(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		int grid = GetGrid(position.x, AxisX);
		int grid2 = GetGrid(position.y, AxisY);
		int grid3 = GetGrid(position.z, AxisZ);
		if (IsValidGrid(grid, grid2, grid3))
		{
			return new Grid(grid, grid2, grid3);
		}
		return default(Grid);
	}

	public static int GetSubGrid(float position, float axis)
	{
		return Mathf.RoundToInt(position / 2f + axis / 2f);
	}

	public static SubGrid GetSubGrid(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		int subGrid = GetSubGrid(position.x, AxisX);
		int subGrid2 = GetSubGrid(position.y, AxisY);
		int subGrid3 = GetSubGrid(position.z, AxisZ);
		if (IsValidSubGrid(subGrid, subGrid2, subGrid3))
		{
			return new SubGrid(subGrid, subGrid2, subGrid3);
		}
		return default(SubGrid);
	}

	public static bool IsBlocked(int x, int y, int z)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		int x2 = Math.DivRem(x, 8, out var result);
		int y2 = Math.DivRem(y, 8, out var result2);
		int z2 = Math.DivRem(z, 8, out var result3);
		int gridIndex = GetGridIndex(x2, y2, z2);
		NativeBitArray val = (NativeBitArray)(IsValidGrid(x2, y2, z2) ? OcclusionSubGridBlocked[gridIndex] : default(NativeBitArray));
		int num = result3 * 8 * 8 + result2 * 8 + result;
		if (((NativeBitArray)(ref val)).IsCreated)
		{
			return ((NativeBitArray)(ref val)).IsSet(num);
		}
		return false;
	}

	public static bool IsBlocked(SubGrid sub)
	{
		return IsBlocked(sub.x, sub.y, sub.z);
	}

	public static bool IsValidGrid(int x, int y, int z)
	{
		if (x < 0 || y < 0 || z < 0)
		{
			return false;
		}
		if (x >= ChunkCountX || y >= ChunkCountY || z >= ChunkCountZ)
		{
			return false;
		}
		return true;
	}

	public static bool IsValidSubGrid(int x, int y, int z)
	{
		if (x < 0 || y < 0 || z < 0)
		{
			return false;
		}
		if (x >= SubChunkCountX || y >= SubChunkCountY || z >= SubChunkCountZ)
		{
			return false;
		}
		return true;
	}

	public static void CalculatePathBetweenGrids(SubGrid grid1, SubGrid grid2, out bool pathBlocked)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		pathBlocked = false;
		NativeReference<bool> returnHolder = ReturnHolder;
		CalculatePathBetweenGridsJob calculatePathBetweenGridsJob = new CalculatePathBetweenGridsJob
		{
			From = grid1,
			To = grid2,
			PathBlocked = returnHolder,
			Grid = new GridDefinition
			{
				OcclusionSubGridBlocked = OcclusionSubGridBlocked.AsReadOnly(),
				ChunkCount = new int3(ChunkCountX, ChunkCountY, ChunkCountZ),
				SubChunkCount = new int3(SubChunkCountX, SubChunkCountY, SubChunkCountZ)
			},
			BlockedGridThreshold = ConVar.AntiHack.server_occlusion_blocked_grid_threshold,
			NeighbourThreshold = ConVar.AntiHack.server_occlusion_neighbour_threshold,
			UseNeighbourThresholds = ConVar.AntiHack.server_occlusion_use_neighbour_thresholds
		};
		IJobExtensions.RunByRef<CalculatePathBetweenGridsJob>(ref calculatePathBetweenGridsJob);
		pathBlocked = returnHolder.Value;
	}

	public static JobHandle CalculatePathsBetweenGridsJob(ReadOnly<(SubGrid from, SubGrid to)> paths, NativeArray<bool> pathsBlocked)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		CalculatePathsBetweenGridsJob calculatePathsBetweenGridsJob = new CalculatePathsBetweenGridsJob
		{
			Paths = paths,
			PathsBlocked = pathsBlocked,
			Grid = new GridDefinition
			{
				OcclusionSubGridBlocked = OcclusionSubGridBlocked.AsReadOnly(),
				ChunkCount = new int3(ChunkCountX, ChunkCountY, ChunkCountZ),
				SubChunkCount = new int3(SubChunkCountX, SubChunkCountY, SubChunkCountZ)
			},
			BlockedGridThreshold = ConVar.AntiHack.server_occlusion_blocked_grid_threshold,
			NeighbourThreshold = ConVar.AntiHack.server_occlusion_neighbour_threshold,
			UseNeighbourThresholds = ConVar.AntiHack.server_occlusion_use_neighbour_thresholds
		};
		return IJobParallelForBatchExtensions.ScheduleBatchByRef<CalculatePathsBetweenGridsJob>(ref calculatePathsBetweenGridsJob, paths.Length, 64, default(JobHandle));
	}

	public static void SetupGrid()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 size = TerrainMeta.Size;
		ChunkCountX = Mathf.Max(Mathf.CeilToInt(size.x / 16f), 1);
		ChunkCountY = Mathf.Max(Mathf.CeilToInt((float)MaxY / 16f), 1);
		ChunkCountZ = Mathf.Max(Mathf.CeilToInt(size.z / 16f), 1);
		SubChunkCountX = Mathf.Max(Mathf.CeilToInt(size.x / 2f), 1);
		SubChunkCountY = Mathf.Max(Mathf.CeilToInt((float)MaxY / 2f), 1);
		SubChunkCountZ = Mathf.Max(Mathf.CeilToInt(size.z / 2f), 1);
		AxisX = TerrainMeta.Size.x / 2f;
		AxisY = MaxY / 2;
		AxisZ = TerrainMeta.Size.z / 2f;
		NativeReferenceEx.SafeDispose(ref ReturnHolder);
		ReturnHolder = new NativeReference<bool>(AllocatorHandle.op_Implicit((Allocator)4), (NativeArrayOptions)1);
		bool server_occlusion_save_grid = ConVar.AntiHack.server_occlusion_save_grid;
		if (!server_occlusion_save_grid || !ReadGridFromFile(SubGridFilePath))
		{
			GenerateOcclusionGrid();
			if (server_occlusion_save_grid)
			{
				WriteGridToFile(ChunkCountX * ChunkCountY * ChunkCountZ, OcclusionSubGridBlocked);
			}
		}
		foreach (BasePlayer allPlayer in BasePlayer.allPlayerList)
		{
			if (OcclusionEnabled && allPlayer.SupportsServerOcclusion())
			{
				allPlayer.SubGrid = GetSubGrid(allPlayer.GetOcclusionOffset());
			}
		}
	}

	public static void Dispose()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (OcclusionSubGridBlocked.IsCreated)
		{
			for (int i = 0; i < OcclusionSubGridBlocked.Length; i++)
			{
				NativeBitArray val = OcclusionSubGridBlocked[i];
				if (((NativeBitArray)(ref val)).IsCreated)
				{
					((NativeBitArray)(ref val)).Dispose();
				}
			}
			OcclusionSubGridBlocked.Dispose();
		}
		if (ReturnHolder.IsCreated)
		{
			ReturnHolder.Dispose();
		}
	}

	private static void WriteGridToFile(int length, NativeArray<NativeBitArray> data)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			using BinaryWriter binaryWriter = new BinaryWriter(File.Open(SubGridFilePath, FileMode.Create));
			binaryWriter.Write(length);
			binaryWriter.Write(OcclusionIncludeRocks);
			Enumerator<NativeBitArray> enumerator = data.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					NativeBitArray current = enumerator.Current;
					if (!((NativeBitArray)(ref current)).IsCreated)
					{
						binaryWriter.Write(0);
						continue;
					}
					binaryWriter.Write(((NativeBitArray)(ref current)).Length);
					byte[] array = new byte[(((NativeBitArray)(ref current)).Length + 7) / 8];
					((NativeBitArray)(ref current)).AsNativeArray<byte>().CopyTo(array);
					binaryWriter.Write(array);
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex.Message);
		}
	}

	public static bool ReadGridFromFile(string path)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!File.Exists(path))
			{
				return false;
			}
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			using (BinaryReader binaryReader = new BinaryReader(File.Open(path, FileMode.Open)))
			{
				int num = binaryReader.ReadInt32();
				if (binaryReader.ReadBoolean() != OcclusionIncludeRocks)
				{
					Debug.LogWarning((object)"Grid file and occlusion parameters don't match, rebuilding grid");
					binaryReader.Close();
					File.Delete(path);
					return false;
				}
				OcclusionSubGridBlocked = new NativeArray<NativeBitArray>(num, (Allocator)4, (NativeArrayOptions)1);
				for (int i = 0; i < num; i++)
				{
					int num2 = binaryReader.ReadInt32();
					if (num2 != 0)
					{
						byte[] array = binaryReader.ReadBytes((num2 + 7) / 8);
						OcclusionSubGridBlocked[i] = new NativeBitArray(num2, AllocatorHandle.op_Implicit((Allocator)4), (NativeArrayOptions)1);
						NativeBitArray val = OcclusionSubGridBlocked[i];
						((NativeBitArray)(ref val)).AsNativeArray<byte>().CopyFrom(array);
					}
				}
				Debug.Log((object)$"Loaded {num} occlusion sub-chunks from file - took {stopwatch.Elapsed.TotalMilliseconds / 1000.0} seconds");
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex.Message);
			return false;
		}
	}

	[ServerVar(Help = "Tests occlusion visibility between two positions")]
	public static string serverocclusiondebug(ConsoleSystem.Arg arg)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = arg.GetVector3(0) + PlayerEyes.EyeOffset;
		Vector3 val2 = arg.GetVector3(1) + PlayerEyes.EyeOffset;
		SubGrid subGrid = GetSubGrid(val);
		SubGrid subGrid2 = GetSubGrid(val2);
		if (subGrid.Equals(default(SubGrid)) || subGrid2.Equals(default(SubGrid)))
		{
			return "Path not blocked due to one of positions being outside of grid";
		}
		NativeList<(int3, Color)> cells = default(NativeList<(int3, Color)>);
		cells._002Ector(AllocatorHandle.op_Implicit((Allocator)2));
		bool flag = DebugPath(val, val2, cells);
		cells.Dispose();
		return $"Grid 1: {subGrid}, Grid 2: {subGrid2}\nPath blocked: {flag}";
	}

	[ServerVar(Help = "(Generated) Validates that all server occlusion network groups are correctly mapped to their occlusion data; reports any inconsistencies found")]
	public static void OcclusionValidateGroups(ConsoleSystem.Arg arg)
	{
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		if (!OcclusionEnabled)
		{
			arg.ReplyWith("ServerOcclusion disabled");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (var (arg2, obj3) in Occludees)
		{
			if (stringBuilder.Length > 1024)
			{
				break;
			}
			if (CollectionEx.IsEmpty((ICollection<BaseNetworkable>)obj3))
			{
				stringBuilder.AppendLine($"Occlusion group for {arg2} is empty - it should've been cleaned up!");
				continue;
			}
			Enumerator<BaseNetworkable> enumerator2 = ((ListHashSet<BaseNetworkable>)obj3).GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					BaseNetworkable current = enumerator2.Current;
					if (stringBuilder.Length > 1024)
					{
						break;
					}
					if ((Object)(object)current == (Object)null)
					{
						stringBuilder.AppendLine($"Occlusion group for {arg2} has a null networkable!");
						continue;
					}
					if (!current.SupportsServerOcclusion())
					{
						stringBuilder.AppendLine($"Occlusion group for {arg2} has a {current} that doesn't support server occlusion!");
						continue;
					}
					Enumerator<BaseNetworkable> enumerator3 = current.OcclusionGroupRefs.GetEnumerator();
					try
					{
						while (enumerator3.MoveNext())
						{
							BaseNetworkable current2 = enumerator3.Current;
							if (stringBuilder.Length > 1024)
							{
								break;
							}
							if ((Object)(object)current2 == (Object)null)
							{
								stringBuilder.AppendLine($"Occlusion group for {current} had a null referrer!");
							}
							else if (!((ListHashSet<BaseNetworkable>)current2.OcclusionGroup).Contains(current))
							{
								stringBuilder.AppendLine($"Occlusion group for referrer-{current2} of {current} was desynced!");
							}
						}
					}
					finally
					{
						((IDisposable)enumerator3/*cast due to constrained. prefix*/).Dispose();
					}
					Group occlusionGroup = current.OcclusionGroup;
					if (current.net.connection == null)
					{
						if (((ListHashSet<BaseNetworkable>)occlusionGroup).Count != 1 || !((ListHashSet<BaseNetworkable>)occlusionGroup).Contains(current))
						{
							stringBuilder.AppendLine($"Occlusion group for sleeper-{current} has other participants!");
						}
						continue;
					}
					bool flag = false;
					enumerator3 = ((ListHashSet<BaseNetworkable>)occlusionGroup).GetEnumerator();
					try
					{
						while (enumerator3.MoveNext())
						{
							BaseNetworkable current3 = enumerator3.Current;
							if (stringBuilder.Length > 1024)
							{
								break;
							}
							if ((Object)(object)current3 == (Object)null)
							{
								stringBuilder.AppendLine($"Occlusion group for {current} has a null!");
							}
							else if ((Object)(object)current3 == (Object)(object)current)
							{
								flag = true;
							}
							else if (!current.net.subscriber.IsSubscribed(current3.net.group))
							{
								stringBuilder.AppendLine($"Occlusion group for {current} has a stale participant!");
							}
						}
					}
					finally
					{
						((IDisposable)enumerator3/*cast due to constrained. prefix*/).Dispose();
					}
					if (!flag)
					{
						stringBuilder.AppendLine($"Occlusion group for {current} doesn't have an owner!");
					}
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
			}
		}
		Enumerator<BasePlayer> enumerator4 = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator4.MoveNext())
			{
				BasePlayer current4 = enumerator4.Current;
				if (stringBuilder.Length > 1024)
				{
					break;
				}
				if (!((Object)(object)current4 == (Object)null))
				{
					bool flag2 = current4.SupportsServerOcclusion();
					bool flag3 = current4.OcclusionGroup != null;
					if (flag2 != flag3)
					{
						stringBuilder.AppendLine($"Active {current4} SupportsServerOcclusion:{flag2} but hasLocalGroup: {flag3}");
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator4/*cast due to constrained. prefix*/).Dispose();
		}
		enumerator4 = BasePlayer.sleepingPlayerList.GetEnumerator();
		try
		{
			while (enumerator4.MoveNext())
			{
				BasePlayer current5 = enumerator4.Current;
				if (stringBuilder.Length > 1024)
				{
					break;
				}
				if (!((Object)(object)current5 == (Object)null))
				{
					bool flag4 = current5.SupportsServerOcclusion();
					bool flag5 = current5.OcclusionGroup != null;
					if (flag4 != flag5)
					{
						stringBuilder.AppendLine($"Sleeper {current5} SupportsServerOcclusion:{flag4} but hasLocalGroup: {flag5}");
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator4/*cast due to constrained. prefix*/).Dispose();
		}
		if (stringBuilder.Length > 0)
		{
			arg.ReplyWith(stringBuilder.ToString());
		}
		else
		{
			arg.ReplyWith($"All {Occludees.Count} server occlusion groups are valid");
		}
	}

	public static bool DebugPath(Vector3 p1, Vector3 p2, NativeList<(int3, Color)> cells)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		SubGrid subGrid = GetSubGrid(p1);
		SubGrid subGrid2 = GetSubGrid(p2);
		GridDefinition gridDef = new GridDefinition
		{
			OcclusionSubGridBlocked = OcclusionSubGridBlocked.AsReadOnly(),
			ChunkCount = new int3(ChunkCountX, ChunkCountY, ChunkCountZ),
			SubChunkCount = new int3(SubChunkCountX, SubChunkCountY, SubChunkCountZ)
		};
		int3 val = new int3(subGrid.x, subGrid.y, subGrid.z);
		int3 to = default(int3);
		((int3)(ref to))._002Ector(subGrid2.x, subGrid2.y, subGrid2.z);
		return Algorithm.Gather(blockedGridThreshold: ConVar.AntiHack.server_occlusion_blocked_grid_threshold, neighbourThreshold: ConVar.AntiHack.server_occlusion_neighbour_threshold, useNeighbourThresholds: ConVar.AntiHack.server_occlusion_use_neighbour_thresholds, from: val, to: to, gridDef: in gridDef, cells: cells);
	}

	public static bool GetCachedVisibility(SubGrid from, SubGrid to, out bool isVisible)
	{
		int num = from.GetIndex();
		int num2 = to.GetIndex();
		if (num > num2)
		{
			int num3 = num2;
			int num4 = num;
			num = num3;
			num2 = num4;
		}
		return ((Dictionary<(int, int), bool>)(object)OcclusionCache).TryGetValue((num, num2), out isVisible);
	}

	public static void CacheVisibility(SubGrid from, SubGrid to, bool isVisible)
	{
		int num = from.GetIndex();
		int num2 = to.GetIndex();
		if (num > num2)
		{
			int num3 = num2;
			int num4 = num;
			num = num3;
			num2 = num4;
		}
		OcclusionCache.TryAdd((num, num2), isVisible);
	}

	private static void GenerateOcclusionGrid()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		int num = ChunkCountX * ChunkCountY * ChunkCountZ;
		OcclusionSubGridBlocked = new NativeArray<NativeBitArray>(num, (Allocator)4, (NativeArrayOptions)1);
		Debug.Log((object)$"Preparing Occlusion Grid ({SubChunkCountX}, {SubChunkCountY}, {SubChunkCountZ})");
		NativeList<int> cellsToCheck = default(NativeList<int>);
		cellsToCheck._002Ector(1024, AllocatorHandle.op_Implicit((Allocator)3));
		GenerateOcclusionBroadPhase(cellsToCheck, num);
		int num2 = (cellsToCheck.Length + 32000 - 1) / 32000;
		NativeList<SubGrid> subGridCells = default(NativeList<SubGrid>);
		subGridCells._002Ector(16384000, AllocatorHandle.op_Implicit((Allocator)3));
		Debug.Log((object)$"Processing {num2} batches({cellsToCheck.Length} broadphase cells total)...");
		for (int i = 0; i < num2; i++)
		{
			_ = stopwatch.Elapsed.TotalSeconds;
			subGridCells.Clear();
			int num3 = i * 32000;
			int num4 = Math.Min(num3 + 32000, cellsToCheck.Length);
			for (int j = num3; j < num4; j++)
			{
				Grid grid = Grid.FromIndex(cellsToCheck[j]);
				int num5 = grid.x * 8;
				int num6 = grid.y * 8;
				int num7 = grid.z * 8;
				for (int k = 0; k < 8; k++)
				{
					for (int l = 0; l < 8; l++)
					{
						for (int m = 0; m < 8; m++)
						{
							SubGrid subGrid = new SubGrid(m + num5, l + num6, k + num7);
							subGridCells.AddNoResize(subGrid);
						}
					}
				}
			}
			GenerateOcclusionNarrowPhase(subGridCells);
		}
		subGridCells.Dispose();
		cellsToCheck.Dispose();
		Debug.Log((object)$"Initialized {SubChunkCountX * SubChunkCountY * SubChunkCountZ} occlusion sub-chunks - took {stopwatch.Elapsed.TotalSeconds}s");
	}

	private static void GenerateOcclusionBroadPhase(NativeList<int> cellsToCheck, int chunkTotalCount)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		NativeArray<Vector3> val = default(NativeArray<Vector3>);
		val._002Ector(chunkTotalCount, (Allocator)3, (NativeArrayOptions)0);
		NativeArray<Vector3> val2 = default(NativeArray<Vector3>);
		val2._002Ector(chunkTotalCount, (Allocator)3, (NativeArrayOptions)0);
		NativeArray<int> val3 = default(NativeArray<int>);
		val3._002Ector(chunkTotalCount, (Allocator)3, (NativeArrayOptions)0);
		Vector3 val4 = default(Vector3);
		((Vector3)(ref val4))._002Ector(8f, 8f, 8f);
		for (int i = 0; i < chunkTotalCount; i++)
		{
			val[i] = Grid.FromIndex(i).GetCenterPoint();
			val2[i] = val4;
			val3[i] = 8388608;
		}
		NativeArray<bool> results = default(NativeArray<bool>);
		results._002Ector(chunkTotalCount, (Allocator)3, (NativeArrayOptions)0);
		GamePhysics.CheckBounds(val.AsReadOnly(), val2.AsReadOnly(), val3.AsReadOnly(), results, (QueryTriggerInteraction)1, GamePhysics.MasksToValidate.Terrain);
		val3.Dispose();
		val2.Dispose();
		val.Dispose();
		for (int j = 0; j < results.Length; j++)
		{
			Grid grid = Grid.FromIndex(j);
			bool num = results[j];
			bool flag = false;
			if (grid.y < ChunkCountY - 1)
			{
				int index = new Grid(grid.x, grid.y + 1, grid.z).GetIndex();
				flag = results[index];
			}
			NativeBitArray val5 = OcclusionSubGridBlocked[j];
			if (((NativeBitArray)(ref val5)).IsCreated)
			{
				val5 = OcclusionSubGridBlocked[j];
				((NativeBitArray)(ref val5)).Dispose();
			}
			if (num | flag)
			{
				OcclusionSubGridBlocked[j] = new NativeBitArray(512, AllocatorHandle.op_Implicit((Allocator)4), (NativeArrayOptions)1);
				cellsToCheck.Add(ref j);
			}
		}
		results.Dispose();
	}

	private static void GenerateOcclusionNarrowPhase(NativeList<SubGrid> subGridCells)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		int num = GridOffsets.Length;
		NativeArray<Vector3> posi = default(NativeArray<Vector3>);
		posi._002Ector(subGridCells.Length * num, (Allocator)3, (NativeArrayOptions)0);
		NativeArray<Vector3> val = default(NativeArray<Vector3>);
		val._002Ector(GridOffsets, (Allocator)3);
		NativeBitArray val3;
		if (!subGridCells.IsEmpty)
		{
			CalculateSubGridSamplePointsJob obj = new CalculateSubGridSamplePointsJob
			{
				Posi = posi,
				SubGridCells = subGridCells.AsReadOnly(),
				GridOffsets = val.AsReadOnly(),
				CellOffset = new Vector3((float)SubGrid.GetOffset(TerrainMeta.Size.x), (float)SubGrid.GetOffset(MaxY), (float)SubGrid.GetOffset(TerrainMeta.Size.z))
			};
			int batchSize = GamePhysics.GetBatchSize(subGridCells.Length);
			int length = subGridCells.Length;
			JobHandle val2 = default(JobHandle);
			val2 = IJobForExtensions.ScheduleParallel<CalculateSubGridSamplePointsJob>(obj, length, batchSize, val2);
			((JobHandle)(ref val2)).Complete();
			NativeArray<bool> results = default(NativeArray<bool>);
			results._002Ector(posi.Length, (Allocator)3, (NativeArrayOptions)0);
			AntiHack.TestInsideTerrain(posi.AsReadOnly(), results);
			int num2 = 0;
			for (int i = 0; i < subGridCells.Length; i++)
			{
				SubGrid subGrid = subGridCells[i];
				bool flag = true;
				for (int j = 0; j < num; j++)
				{
					int num3 = i * num + j;
					flag &= results[num3];
				}
				if (flag)
				{
					int x = Math.DivRem(subGrid.x, 8, out var result);
					int y = Math.DivRem(subGrid.y, 8, out var result2);
					int z = Math.DivRem(subGrid.z, 8, out var result3);
					int gridIndex = GetGridIndex(x, y, z);
					int num4 = result3 * 8 * 8 + result2 * 8 + result;
					val3 = OcclusionSubGridBlocked[gridIndex];
					((NativeBitArray)(ref val3)).Set(num4, true);
				}
				else
				{
					int num5 = num2++;
					subGridCells[num5] = subGrid;
					for (int k = 0; k < num; k++)
					{
						posi[num5 * num + k] = posi[i * num + k];
					}
				}
			}
			subGridCells.ResizeUninitialized(num2);
			results.Dispose();
		}
		if (OcclusionIncludeRocks && !subGridCells.IsEmpty)
		{
			NativeArray<Vector3> subArray = posi.GetSubArray(0, subGridCells.Length * num);
			NativeArray<RaycastHit> hits = default(NativeArray<RaycastHit>);
			hits._002Ector(subArray.Length, (Allocator)3, (NativeArrayOptions)0);
			AntiHack.AreInsideMesh(subArray.AsReadOnly(), hits);
			Span<int> span = stackalloc int[num];
			int num6 = 0;
			for (int l = 0; l < subGridCells.Length; l++)
			{
				bool flag2 = true;
				for (int m = 0; m < num; m++)
				{
					RaycastHit val4 = hits[l * num + m];
					int colliderInstanceID = ((RaycastHit)(ref val4)).colliderInstanceID;
					flag2 &= colliderInstanceID != 0;
					if (!flag2)
					{
						break;
					}
					bool flag3 = false;
					for (int n = 0; n < num6; n++)
					{
						if (span[n] == colliderInstanceID)
						{
							flag3 = true;
							break;
						}
					}
					if (!flag3)
					{
						GameObject gameObject = ((Component)((RaycastHit)(ref val4)).collider).gameObject;
						flag2 &= gameObject.HasCustomTag(GameObjectTag.AllowBarricadePlacement);
						if (!flag2)
						{
							break;
						}
						span[num6++] = colliderInstanceID;
					}
				}
				num6 = 0;
				if (flag2)
				{
					SubGrid subGrid2 = subGridCells[l];
					int x2 = Math.DivRem(subGrid2.x, 8, out var result4);
					int y2 = Math.DivRem(subGrid2.y, 8, out var result5);
					int z2 = Math.DivRem(subGrid2.z, 8, out var result6);
					int gridIndex2 = GetGridIndex(x2, y2, z2);
					int num7 = result6 * 8 * 8 + result5 * 8 + result4;
					val3 = OcclusionSubGridBlocked[gridIndex2];
					((NativeBitArray)(ref val3)).Set(num7, true);
				}
			}
			hits.Dispose();
		}
		val.Dispose();
		posi.Dispose();
	}

	static ServerOcclusion()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		MaxY = 200;
		OcclusionCache = new LimitDictionary<(int, int), bool>(32768);
		OcclusionEnabled = true;
		OcclusionIncludeRocks = true;
		Occludees = new Dictionary<Network.Visibility.Group, Group>();
		GridOffsets = (Vector3[])(object)new Vector3[2]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, 1f, 0f)
		};
		neighbours = new(int, int, int)[6]
		{
			(1, 0, 0),
			(-1, 0, 0),
			(0, 1, 0),
			(0, -1, 0),
			(0, 0, 1),
			(0, 0, -1)
		};
	}
}

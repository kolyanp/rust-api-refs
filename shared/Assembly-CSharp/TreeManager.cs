using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;
using UnityEngine.Assertions;

public class TreeManager : BaseEntity
{
	private struct ToProcess
	{
		public struct Telemetry
		{
			public TimeSpan InitialTime;

			public TimeSpan IterativeTime;

			public int FramesToComplete;

			public void Report(BasePlayer player)
			{
				TimeSpan timeSpan = InitialTime + IterativeTime;
				TimeSpan timeSpan2 = new TimeSpan((long)(PlayerBudgetMS * 10000f * (float)FramesToComplete));
				TimeSpan timeSpan3 = timeSpan - timeSpan2;
				TimeSpan timeSpan4 = timeSpan / FramesToComplete;
				RustLog.Log(RustLog.EntryType.Network, 1, ((Component)player).gameObject, "TreeManager: Initial: {0}ms, Iterative: {1}ms, Total: {2}ms({3}ms/frame), Overspent: {4}ms", InitialTime.TotalMilliseconds, IterativeTime.TotalMilliseconds, timeSpan.TotalMilliseconds, timeSpan4.TotalMilliseconds, timeSpan3.TotalMilliseconds);
			}
		}

		public BasePlayer Player;

		public BitArray SentCells;

		public int Left;

		public int Range;

		public int OldCellIndex;

		public int LastProcessedIndex;

		public Telemetry Stats;
	}

	public struct TreeCell
	{
		public TreeList TreeList;

		public MemoryStream SerializedCell;

		public bool IsDirty;
	}

	public static ListHashSet<BaseEntity> entities = new ListHashSet<BaseEntity>();

	public static TreeManager server;

	[ServerVar(Help = "(Generated) When enabled, tree data is streamed to players based on proximity rather than sending all trees at connect; reduces initial bandwidth")]
	public static bool EnableTreeStreaming = true;

	[ServerVar(Help = "(Generated) Per-frame CPU budget in milliseconds allocated to sending tree streaming data per player")]
	public static float PlayerBudgetMS = 0.01f;

	[ServerVar(Help = "(Generated) Total per-frame CPU budget in milliseconds for the tree streaming update system")]
	public static float UpdateBudgetMS = 1f;

	private const string CellSizeHelp = "Define cell size(in m) of a grid for trees  - only has effect on world load and must be > 1. This affects how much data we send per tree cell(bigger the cell - more trees we have to send). The smaller the cell, the more cells we have to process and the more memory we need per player to track what's left to send(gridSize ^ 2 / 8 bytes). We readjust CellSize to ensure gridSize never exceeds 512.";

	[ServerVar(Help = "Define cell size(in m) of a grid for trees  - only has effect on world load and must be > 1. This affects how much data we send per tree cell(bigger the cell - more trees we have to send). The smaller the cell, the more cells we have to process and the more memory we need per player to track what's left to send(gridSize ^ 2 / 8 bytes). We readjust CellSize to ensure gridSize never exceeds 512.")]
	public static int CellSize = 100;

	private const string UseLazySerializationHelp = "Instead of reserializing grid cell on every tree add/removal(which can cost 0.25ms on 4.5k world), defer it to the streaming update. This reduces amount of times we need to serialize the tree list, but causes the player queue to take longer to process, as that's where evaluation happens.";

	[ServerVar(Help = "Instead of reserializing grid cell on every tree add/removal(which can cost 0.25ms on 4.5k world), defer it to the streaming update. This reduces amount of times we need to serialize the tree list, but causes the player queue to take longer to process, as that's where evaluation happens.")]
	public static bool UseLazySerialization = true;

	private List<ToProcess> playersToProcess = new List<ToProcess>(100);

	private int gridSize = 64;

	[NonSerialized]
	public List<TreeCell> treesGrid;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("TreeManager.OnRpcMessage"))
		{
			if (rpc == 1907121457 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_RequestTrees"));
				}
				using (TimeWarning.New("SERVER_RequestTrees"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1907121457u, "SERVER_RequestTrees", this, player, 0uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SERVER_RequestTrees(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SERVER_RequestTrees");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public static Vector3 ProtoHalf3ToVec3(Half3 half3)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3
		{
			x = Mathf.HalfToFloat((ushort)half3.x),
			y = Mathf.HalfToFloat((ushort)half3.y),
			z = Mathf.HalfToFloat((ushort)half3.z)
		};
	}

	public static Half3 Vec3ToProtoHalf3(Vector3 vec3)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		return new Half3
		{
			x = Mathf.FloatToHalf(vec3.x),
			y = Mathf.FloatToHalf(vec3.y),
			z = Mathf.FloatToHalf(vec3.z)
		};
	}

	public int GetTreeCount()
	{
		if ((Object)(object)server == (Object)(object)this)
		{
			return entities.Count;
		}
		return -1;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		server = this;
		InitTreeGrid();
	}

	private void InitTreeGrid()
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		if (CellSize <= 0)
		{
			CellSize = 100;
		}
		gridSize = (int)Mathf.Ceil((float)World.Size / (float)CellSize);
		gridSize = Math.Clamp(gridSize, 1, 512);
		if (gridSize == 512)
		{
			CellSize = (int)Mathf.Ceil((float)World.Size / (float)gridSize);
		}
		RustLog.Log(RustLog.EntryType.Network, 1, null, "TreeManager: using {0}x{0} grid with cell size {1}", gridSize, CellSize);
		treesGrid = new List<TreeCell>(gridSize * gridSize);
		for (int i = 0; i < gridSize * gridSize; i++)
		{
			TreeCell item = default(TreeCell);
			item.TreeList = new TreeList();
			item.TreeList.trees = new List<Tree>();
			item.SerializedCell = new MemoryStream();
			treesGrid.Add(item);
		}
		Enumerator<BaseEntity> enumerator = entities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BaseEntity current = enumerator.Current;
				Vector2i val = ToCellIndices(current.ServerWorldPosition);
				Tree val2 = Pool.Get<Tree>();
				ExtractTreeNetworkData(current, val2);
				treesGrid[val.y * gridSize + val.x].TreeList.trees.Add(val2);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		foreach (TreeCell item2 in treesGrid)
		{
			ProtoStreamExtensions.WriteToStream((IProto)(object)item2.TreeList, (Stream)item2.SerializedCell, false, 2097152);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		InitTreeGrid();
	}

	public void SendPendingTrees()
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		playersToProcess.RemoveAll((ToProcess toProcess) => (Object)(object)toProcess.Player == (Object)null);
		if (CollectionEx.IsEmpty(playersToProcess))
		{
			return;
		}
		playersToProcess.Sort((ToProcess left, ToProcess right) => right.Left - left.Left);
		Stopwatch stopwatch = Pool.Get<Stopwatch>();
		Stopwatch stopwatch2 = Pool.Get<Stopwatch>();
		stopwatch.Start();
		for (int num = 0; num < playersToProcess.Count; num++)
		{
			if (stopwatch.Elapsed.TotalMilliseconds > (double)UpdateBudgetMS)
			{
				break;
			}
			using (TimeWarning.New("Player"))
			{
				stopwatch2.Restart();
				ToProcess record = playersToProcess[num];
				Vector2i val = ToCellIndices(record.Player.ServerWorldPosition);
				if (record.OldCellIndex != val.y * gridSize + val.x)
				{
					record.LastProcessedIndex = -1;
					record.Range = 1;
					record.OldCellIndex = val.y * gridSize + val.x;
				}
				int num2 = record.Range;
				while (stopwatch2.Elapsed.TotalMilliseconds < (double)PlayerBudgetMS && record.Left > 0)
				{
					int num3 = Math.Max(val.x - num2 / 2, 0);
					int num4 = Math.Max(val.y - num2 / 2, 0);
					int num5 = Math.Min(num3 + num2, gridSize - 1);
					int num6 = Math.Min(num4 + num2, gridSize - 1);
					for (int num7 = num3; num7 <= num5; num7++)
					{
						if (SendToPlayer(num4 * gridSize + num7, ref record) && stopwatch2.Elapsed.TotalMilliseconds >= (double)PlayerBudgetMS)
						{
							stopwatch2.Stop();
							break;
						}
					}
					if (stopwatch2.Elapsed.TotalMilliseconds >= (double)PlayerBudgetMS)
					{
						stopwatch2.Stop();
						break;
					}
					if (num6 - num4 > 1)
					{
						for (int num8 = num4 + 1; num8 <= num6 - 1; num8++)
						{
							if (SendToPlayer(num8 * gridSize + num3, ref record) && stopwatch2.Elapsed.TotalMilliseconds >= (double)PlayerBudgetMS)
							{
								stopwatch2.Stop();
								break;
							}
							if (num5 != num3 && SendToPlayer(num8 * gridSize + num5, ref record) && stopwatch2.Elapsed.TotalMilliseconds >= (double)PlayerBudgetMS)
							{
								stopwatch2.Stop();
								break;
							}
						}
					}
					if (stopwatch2.Elapsed.TotalMilliseconds >= (double)PlayerBudgetMS)
					{
						stopwatch2.Stop();
						break;
					}
					if (num6 != num4)
					{
						for (int num9 = num3; num9 <= num5; num9++)
						{
							if (SendToPlayer(num6 * gridSize + num9, ref record) && stopwatch2.Elapsed.TotalMilliseconds >= (double)PlayerBudgetMS)
							{
								stopwatch2.Stop();
								break;
							}
						}
					}
					if (stopwatch2.IsRunning)
					{
						num2++;
						record.LastProcessedIndex = -1;
					}
				}
				record.Range = num2;
				stopwatch2.Stop();
				ToProcess.Telemetry stats = record.Stats;
				stats.IterativeTime += stopwatch2.Elapsed;
				stats.FramesToComplete++;
				record.Stats = stats;
				playersToProcess[num] = record;
			}
		}
		Pool.FreeUnmanaged(ref stopwatch2);
		Pool.FreeUnmanaged(ref stopwatch);
		playersToProcess.RemoveAll(delegate(ToProcess toProcess)
		{
			if (toProcess.Left == 0)
			{
				toProcess.Stats.Report(toProcess.Player);
				return true;
			}
			return false;
		});
		static bool SendToPlayer(int index, ref ToProcess reference)
		{
			if (reference.LastProcessedIndex >= index || reference.SentCells[index])
			{
				return false;
			}
			reference.LastProcessedIndex = index;
			reference.SentCells[index] = true;
			reference.Left--;
			Debug.Assert(reference.Left >= 0);
			TreeCell value = server.treesGrid[index];
			if (CollectionEx.IsEmpty(value.TreeList.trees))
			{
				return false;
			}
			if (UseLazySerialization && value.IsDirty)
			{
				using (TimeWarning.New("LazySerialize"))
				{
					value.SerializedCell.SetLength(0L);
					ProtoStreamExtensions.WriteToStream((IProto)(object)value.TreeList, (Stream)value.SerializedCell, false, 2097152);
					value.IsDirty = false;
					server.treesGrid[index] = value;
				}
			}
			using (TimeWarning.New("RPC"))
			{
				server.ClientRPC(RpcTarget.Player("CLIENT_ReceiveTrees", reference.Player), value.SerializedCell);
				return true;
			}
		}
	}

	public static void StartTreesBatch(BasePlayer player)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		Stopwatch stopwatch = Stopwatch.StartNew();
		int num = server.gridSize * server.gridSize;
		BitArray bitArray = new BitArray(num);
		Vector2i val = ToCellIndices(player.ServerWorldPosition);
		int num2 = Math.Max(val.x - 1, 0);
		int num3 = Math.Max(val.y - 1, 0);
		int num4 = Math.Min(num2 + 3, server.gridSize - 1);
		int num5 = Math.Min(num3 + 3, server.gridSize - 1);
		for (int i = num3; i <= num5; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				int index = i * server.gridSize + j;
				TreeCell value = server.treesGrid[index];
				if (!CollectionEx.IsEmpty(value.TreeList.trees))
				{
					if (UseLazySerialization && value.IsDirty)
					{
						using (TimeWarning.New("LazySerialize"))
						{
							value.SerializedCell.SetLength(0L);
							ProtoStreamExtensions.WriteToStream((IProto)(object)value.TreeList, (Stream)value.SerializedCell, false, 2097152);
							value.IsDirty = false;
							server.treesGrid[index] = value;
						}
					}
					server.ClientRPC(RpcTarget.Player("CLIENT_ReceiveTrees", player), value.SerializedCell);
				}
				bitArray[index] = true;
				num--;
			}
		}
		stopwatch.Stop();
		ToProcess item = new ToProcess
		{
			Player = player,
			SentCells = bitArray,
			Left = num,
			Range = 4,
			OldCellIndex = val.y * server.gridSize + val.x,
			LastProcessedIndex = -1,
			Stats = new ToProcess.Telemetry
			{
				InitialTime = stopwatch.Elapsed,
				FramesToComplete = 1
			}
		};
		server.playersToProcess.Add(item);
	}

	private static Vector2i ToCellIndices(Vector3 worldPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)World.Size / 2f;
		Vector2 val = Vector3Ex.XZ2D(worldPos) + new Vector2(num, num);
		val.x = Mathf.Clamp(val.x, 0f, (float)(World.Size - 1));
		val.y = Mathf.Clamp(val.y, 0f, (float)(World.Size - 1));
		return new Vector2i((int)(val.x / (float)CellSize), (int)(val.y / (float)CellSize));
	}

	public static void OnTreeDestroyed(BaseEntity billboardEntity)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		entities.Remove(billboardEntity);
		if (Application.isLoading || Application.isQuitting)
		{
			return;
		}
		using (TimeWarning.New("TreeManager.OnTreeDestroyed"))
		{
			Vector2i val = ToCellIndices(billboardEntity.ServerWorldPosition);
			int index = val.y * server.gridSize + val.x;
			TreeCell value = server.treesGrid[index];
			List<Tree> trees = value.TreeList.trees;
			for (int i = 0; i < trees.Count; i++)
			{
				if (trees[i].netId == billboardEntity.net.ID)
				{
					trees[i].Dispose();
					trees.RemoveAt(i);
					if (UseLazySerialization)
					{
						value.IsDirty = true;
						server.treesGrid[index] = value;
					}
					else
					{
						value.SerializedCell.SetLength(0L);
						ProtoStreamExtensions.WriteToStream((IProto)(object)value.TreeList, (Stream)value.SerializedCell, false, 2097152);
					}
					break;
				}
			}
			server.ClientRPC(RpcTarget.NetworkGroup("CLIENT_TreeDestroyed"), billboardEntity.net.ID);
		}
	}

	public static void OnTreeSpawned(BaseEntity billboardEntity)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (billboardEntity.net.group != null && billboardEntity.net.group.restricted)
		{
			return;
		}
		entities.Add(billboardEntity);
		if (Application.isLoading || Application.isQuitting)
		{
			return;
		}
		using (TimeWarning.New("TreeManager.OnTreeSpawned"))
		{
			Vector2i val = ToCellIndices(billboardEntity.ServerWorldPosition);
			int index = val.y * server.gridSize + val.x;
			Tree val2 = Pool.Get<Tree>();
			ExtractTreeNetworkData(billboardEntity, val2);
			TreeCell value = server.treesGrid[index];
			value.TreeList.trees.Add(val2);
			if (UseLazySerialization)
			{
				value.IsDirty = true;
				server.treesGrid[index] = value;
			}
			else
			{
				value.SerializedCell.SetLength(0L);
				ProtoStreamExtensions.WriteToStream((IProto)(object)value.TreeList, (Stream)value.SerializedCell, false, 2097152);
			}
			List<Connection> subscribers = server.net.group.subscribers;
			if (subscribers == null || CollectionEx.IsEmpty(subscribers))
			{
				return;
			}
			List<Connection> list = Pool.Get<List<Connection>>();
			foreach (Connection item in subscribers)
			{
				bool flag = true;
				for (int i = 0; i < server.playersToProcess.Count; i++)
				{
					ToProcess toProcess = server.playersToProcess[i];
					if (toProcess.Player.Connection == item && !toProcess.SentCells[index])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					list.Add(item);
				}
			}
			if (!CollectionEx.IsEmpty(list))
			{
				Tree val3 = Pool.Get<Tree>();
				try
				{
					ExtractTreeNetworkData(billboardEntity, val3);
					server.ClientRPC(RpcTarget.Players("CLIENT_TreeSpawned", list), val3);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			Pool.FreeUnmanaged<Connection>(ref list);
		}
	}

	private static void ExtractTreeNetworkData(BaseEntity billboardEntity, Tree tree)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		tree.netId = billboardEntity.net.ID;
		tree.prefabId = billboardEntity.prefabID;
		tree.position = Vec3ToProtoHalf3(((Component)billboardEntity).transform.position);
		tree.scale = ((Component)billboardEntity).transform.lossyScale.y;
	}

	public static void SendSnapshot(BasePlayer player)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		BufferList<BaseEntity> values = entities.Values;
		TreeList val = null;
		for (int i = 0; i < values.Count; i++)
		{
			BaseEntity billboardEntity = values[i];
			Tree val2 = Pool.Get<Tree>();
			ExtractTreeNetworkData(billboardEntity, val2);
			if (val == null)
			{
				val = Pool.Get<TreeList>();
				val.trees = Pool.Get<List<Tree>>();
			}
			val.trees.Add(val2);
			if (val.trees.Count >= ConVar.Server.maxpacketsize_globaltrees)
			{
				server.ClientRPC(RpcTarget.Player("CLIENT_ReceiveTrees", player), val);
				val.Dispose();
				val = null;
			}
		}
		if (val != null)
		{
			server.ClientRPC(RpcTarget.Player("CLIENT_ReceiveTrees", player), val);
			val.Dispose();
			val = null;
		}
		stopwatch.Stop();
		RustLog.Log(RustLog.EntryType.Network, 1, ((Component)player).gameObject, "Took {0}ms to send {1} global trees to {2}", stopwatch.Elapsed.TotalMilliseconds, values.Count, player);
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(0uL)]
	private void SERVER_RequestTrees(RPCMessage msg)
	{
		if (EnableTreeStreaming)
		{
			StartTreesBatch(msg.player);
		}
		else
		{
			SendSnapshot(msg.player);
		}
	}
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Facepunch;
using Facepunch.Extend;
using Network;
using Network.Relay;
using UnityEngine;

namespace ConVar;

[Factory("pool")]
public class Pool : ConsoleSystem
{
	[ServerVar(Help = "(Generated) When enabled, object pools are pre-allocated at startup to avoid first-use latency; increases startup time but reduces runtime GC stutter")]
	[ClientVar(ClientAdmin = true)]
	public static int mode = 2;

	[ClientVar(Help = "(Generated) When enabled, object pools are pre-allocated at startup to avoid first-use latency; increases startup time but reduces runtime GC stutter")]
	[ServerVar(Help = "(Generated) When enabled, object pools are pre-allocated at startup to avoid first-use latency; increases startup time but reduces runtime GC stutter")]
	public static bool prewarm = true;

	[ClientVar(Help = "(Generated) When enabled, this system is globally active; disable to deactivate the system for the current session")]
	[ServerVar(Help = "(Generated) When enabled, this system is globally active; disable to deactivate the system for the current session")]
	public static bool enabled = true;

	[ClientVar(Help = "(Generated) When enabled, logs additional diagnostic information about pool hits, misses, and spills to the console")]
	[ServerVar(Help = "(Generated) When enabled, logs additional diagnostic information about pool hits, misses, and spills to the console")]
	public static bool debug = false;

	[ServerVar(Help = "Whether to use original pool implementation (slower, but tested). Default is false")]
	[ClientVar(Help = "Whether to use original pool implementation (slower, but tested). Default is false")]
	public static bool UseMutexPool
	{
		get
		{
			return Pool.UseMutexPool;
		}
		set
		{
			Pool.UseMutexPool = value;
		}
	}

	[ServerVar(Help = "(Generated) Prints a table of all object pool entries showing type, capacity, active count, peak usage, hit/miss counts, and spill counts; supports --json")]
	[ClientVar(Help = "(Generated) Prints a table of all object pool entries showing type, capacity, active count, peak usage, hit/miss counts, and spill counts; supports --json")]
	public static void print_memory(Arg arg)
	{
		if (Pool.Directory.Count == 0)
		{
			arg.ReplyWith("Memory pool is empty.");
			return;
		}
		bool flag = arg.HasArg("--raw", remove: true);
		bool flag2 = arg.HasArg("--json", remove: true);
		string text = arg.GetString(0, null);
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.ShouldPadColumns = !flag2;
			val.AddColumn("type");
			val.AddColumn("capacity");
			val.AddColumn("pooled");
			val.AddColumn("active");
			val.AddColumn("max");
			val.AddColumn("hits");
			val.AddColumn("misses");
			val.AddColumn("spills");
			foreach (KeyValuePair<Type, IPoolCollection> item in Pool.Directory.OrderByDescending((KeyValuePair<Type, IPoolCollection> x) => x.Value.ItemsCreated))
			{
				Type key = item.Key;
				IPoolCollection value = item.Value;
				if (text == null || key.ToString().Contains(text))
				{
					val.AddRow(new string[8]
					{
						key.ToString().Replace("System.Collections.Generic.", ""),
						flag ? value.ItemsCapacity.ToString() : NumberExtensions.FormatNumberShort(value.ItemsCapacity),
						flag ? value.ItemsInStack.ToString() : NumberExtensions.FormatNumberShort(value.ItemsInStack),
						flag ? value.ItemsInUse.ToString() : NumberExtensions.FormatNumberShort(value.ItemsInUse),
						flag ? value.MaxItemsInUse.ToString() : NumberExtensions.FormatNumberShort(value.MaxItemsInUse),
						flag ? value.ItemsTaken.ToString() : NumberExtensions.FormatNumberShort(value.ItemsTaken),
						flag ? value.ItemsCreated.ToString() : NumberExtensions.FormatNumberShort(value.ItemsCreated),
						flag ? value.ItemsSpilled.ToString() : NumberExtensions.FormatNumberShort(value.ItemsSpilled)
					});
				}
			}
			arg.ReplyWith(flag2 ? val.ToJson(true) : ((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "(Generated) Resets the peak-usage high-water-mark counter for all pools, allowing fresh measurement of maximum pool demand")]
	[ClientVar(Help = "(Generated) Resets the peak-usage high-water-mark counter for all pools, allowing fresh measurement of maximum pool demand")]
	public static void reset_max_pool_counter(Arg arg)
	{
		if (Pool.Directory.Count == 0)
		{
			arg.ReplyWith("Memory pool is empty.");
			return;
		}
		foreach (IPoolCollection value in Pool.Directory.Values)
		{
			value.ResetMaxUsageCounter();
		}
		arg.ReplyWith("Reset max item counter of pool");
	}

	[ServerVar(Help = "(Generated) Prints a usage report for the BaseNetwork and ProtocolParser array pools, showing bucket sizes, capacities, and hit/miss stats")]
	[ClientVar(Help = "(Generated) Prints a usage report for the BaseNetwork and ProtocolParser array pools, showing bucket sizes, capacities, and hit/miss stats")]
	public static void print_arraypool(Arg arg)
	{
		bool flag = arg.HasArg("--json");
		string text = (flag ? "[" : string.Empty);
		string table = PrintArrayPool<byte>(BaseNetwork.ArrayPool, flag);
		text += FormatTable("BaseNetwork.ArrayPool", table, flag);
		text += (flag ? "," : "\n");
		string table2 = PrintArrayPool<byte>(Shared.ArrayPool, flag);
		text += FormatTable("ProtocolParser.ArrayPool", table2, flag);
		text += (flag ? "," : "\n");
		string table3 = PrintArrayPool<byte>(RustRelay.PacketArrayPool, flag);
		text += FormatTable("RustRelay.PacketArrayPool", table3, flag);
		if (flag)
		{
			text += "]";
		}
		arg.ReplyWith(text);
		static string FormatTable(string name, string text2, bool toJson)
		{
			if (!toJson)
			{
				return name + "\n" + text2;
			}
			return "{\"name\":\"" + name + "\",\"content\":" + text2 + "}";
		}
		unsafe static string PrintArrayPool<T>(ArrayPool<T> pool, bool toJson) where T : unmanaged
		{
			ConcurrentQueue<T[]>[] buffer = pool.GetBuffer();
			TextTable val = Pool.Get<TextTable>();
			try
			{
				val.ShouldPadColumns = !toJson;
				val.ResizeColumns(5);
				val.AddColumn("index");
				val.AddColumn("size");
				val.AddColumn("bytes");
				val.AddColumn("count");
				val.AddColumn("memory");
				val.ResizeRows(buffer.Length);
				int num = 1;
				num = sizeof(T);
				for (int i = 0; i < buffer.Length; i++)
				{
					int num2 = pool.IndexToSize(i);
					int num3 = num2 * num;
					int count = buffer[i].Count;
					int num4 = num3 * count;
					val.AddValue(i);
					val.AddValue(num2);
					val.AddValue(NumberExtensions.FormatBytes<int>(num2, false));
					val.AddValue(count);
					val.AddValue(NumberExtensions.FormatBytes<int>(num4, false));
				}
				return toJson ? val.ToJson(false) : ((object)val).ToString();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	[ServerVar(Help = "(Generated) Prints a table of all prefab pool entries showing prefab name, miss count, current count, target capacity, and push/pop counts; supports --json")]
	[ClientVar(Help = "(Generated) Prints a table of all prefab pool entries showing prefab name, miss count, current count, target capacity, and push/pop counts; supports --json")]
	public static void print_prefabs(Arg arg)
	{
		PrefabPoolCollection pool = GameManager.server.pool;
		if (pool.storage.Count == 0)
		{
			arg.ReplyWith("Prefab pool is empty.");
			return;
		}
		string text = arg.GetString(0, string.Empty);
		bool flag = arg.HasArg("--json");
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.ShouldPadColumns = !flag;
			val.AddColumn("id");
			val.AddColumn("name");
			val.AddColumn("missed");
			val.AddColumn("count");
			val.AddColumn("target");
			val.AddColumn("added");
			val.AddColumn("removed");
			foreach (PrefabPool item in pool.storage.Values.OrderByDescending((PrefabPool x) => x.Missed))
			{
				string text2 = StringPool.Get(item.PrefabName).ToString();
				string prefabName = item.PrefabName;
				string text3 = item.Count.ToString();
				if (string.IsNullOrEmpty(text) || StringEx.Contains(prefabName, text, CompareOptions.IgnoreCase))
				{
					val.AddRow(new string[7]
					{
						text2,
						Path.GetFileNameWithoutExtension(prefabName),
						text3,
						item.TargetCapacity.ToString(),
						item.Missed.ToString(),
						item.Pushed.ToString(),
						item.Popped.ToString()
					});
				}
			}
			arg.ReplyWith(flag ? val.ToJson(true) : ((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "(Generated) Prints a table of all asset pool entries showing asset type, current pooled count, and pool capacity")]
	[ClientVar(Help = "(Generated) Prints a table of all asset pool entries showing asset type, current pooled count, and pool capacity")]
	public static void print_assets(Arg arg)
	{
		if (AssetPool.storage.Count == 0)
		{
			arg.ReplyWith("Asset pool is empty.");
			return;
		}
		string text = arg.GetString(0, string.Empty);
		bool flag = arg.HasArg("--json");
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.ShouldPadColumns = !flag;
			val.AddColumn("type");
			val.AddColumn("allocated");
			val.AddColumn("available");
			foreach (KeyValuePair<Type, Pool> item in AssetPool.storage)
			{
				string text2 = item.Key.ToString();
				string text3 = item.Value.allocated.ToString();
				string text4 = item.Value.available.ToString();
				if (string.IsNullOrEmpty(text) || StringEx.Contains(text2, text, CompareOptions.IgnoreCase))
				{
					val.AddRow(new string[3] { text2, text3, text4 });
				}
			}
			arg.ReplyWith(flag ? val.ToJson(true) : ((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ClientVar(Help = "(Generated) Clears all entries from the object memory pool matching the optional name filter; freed pooled objects are garbage collected")]
	[ServerVar(Help = "(Generated) Clears all entries from the object memory pool matching the optional name filter; freed pooled objects are garbage collected")]
	public static void clear_memory(Arg arg)
	{
		Pool.Clear(arg.GetString(0, string.Empty));
	}

	[ServerVar(Help = "(Generated) Clears all cached prefab instances from the prefab pool matching the optional filter, across client, server, and generic pools")]
	[ClientVar(Help = "(Generated) Clears all cached prefab instances from the prefab pool matching the optional filter, across client, server, and generic pools")]
	public static void clear_prefabs(Arg arg)
	{
		string filter = arg.GetString(0, string.Empty);
		GameManager.server.pool.Clear(filter);
	}

	[ServerVar(Help = "(Generated) Clears all cached entries from the asset pool matching the optional name filter")]
	[ClientVar(Help = "(Generated) Clears all cached entries from the asset pool matching the optional name filter")]
	public static void clear_assets(Arg arg)
	{
		AssetPool.Clear(arg.GetString(0, string.Empty));
	}

	[ServerVar(Help = "(Generated) Exports the current prefab pool contents to a prefabs.csv file listing pool ID, prefab short name, and instance count")]
	[ClientVar(Help = "(Generated) Exports the current prefab pool contents to a prefabs.csv file listing pool ID, prefab short name, and instance count")]
	public static void export_prefabs(Arg arg)
	{
		PrefabPoolCollection pool = GameManager.server.pool;
		if (pool.storage.Count == 0)
		{
			arg.ReplyWith("Prefab pool is empty.");
			return;
		}
		string text = arg.GetString(0, string.Empty);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<uint, PrefabPool> item in pool.storage)
		{
			string arg2 = item.Key.ToString();
			string text2 = StringPool.Get(item.Key);
			string arg3 = item.Value.Count.ToString();
			if (string.IsNullOrEmpty(text) || StringEx.Contains(text2, text, CompareOptions.IgnoreCase))
			{
				stringBuilder.AppendLine($"{arg2},{Path.GetFileNameWithoutExtension(text2)},{arg3}");
			}
		}
		File.WriteAllText("prefabs.csv", stringBuilder.ToString());
	}

	[ServerVar(Help = "(Generated) Pre-warms the prefab pool by instantiating and pooling prefabs matching the optional filter up to the given count override")]
	[ClientVar(Help = "(Generated) Pre-warms the prefab pool by instantiating and pooling prefabs matching the optional filter up to the given count override")]
	public static void fill_prefabs(Arg arg)
	{
		string filter = arg.GetString(0, string.Empty);
		int countOverride = arg.GetInt(1);
		PrefabPoolWarmup.Run(filter, countOverride);
	}
}

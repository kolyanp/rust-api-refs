using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;

namespace Network;

public static class PacketProfiler
{
	public static class AnalyticsKeys
	{
		public static string[] MessageType;

		static AnalyticsKeys()
		{
			MessageType = new string[29];
			for (int i = 0; i < 29; i++)
			{
				MessageType[i] = ((Message.Type)i/*cast due to constrained. prefix*/).ToString();
			}
		}
	}

	[Serializable]
	public class PacketProfilerSnapshot
	{
		public int[] inboundCount;

		public int[] inboundBytes;

		public int[] outboundCount;

		public int[] outboundSum;

		public int[] outboundMsgBytes;

		public int[] outboundBytes;

		public List<PacketProfilingData> recentPackets;
	}

	public static class Import
	{
		public static PacketProfilerSnapshot FromFile(string path)
		{
			try
			{
				if (!File.Exists(path))
				{
					Debug.LogError((object)("[PacketProfiler] Snapshot file not found: " + path));
					return null;
				}
				return JsonConvert.DeserializeObject<PacketProfilerSnapshot>(File.ReadAllText(path));
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("[PacketProfiler] Failed to load snapshot: " + ex.Message));
				return null;
			}
		}

		public static void ApplySnapshot(PacketProfilerSnapshot snapshot)
		{
			if (snapshot == null)
			{
				return;
			}
			Array.Copy(snapshot.inboundCount, inboundCount, snapshot.inboundCount.Length);
			Array.Copy(snapshot.inboundBytes, inboundBytes, snapshot.inboundBytes.Length);
			Array.Copy(snapshot.outboundCount, outboundCount, snapshot.outboundCount.Length);
			Array.Copy(snapshot.outboundSum, outboundSum, snapshot.outboundSum.Length);
			Array.Copy(snapshot.outboundMsgBytes, outboundMsgBytes, snapshot.outboundMsgBytes.Length);
			Array.Copy(snapshot.outboundBytes, outboundBytes, snapshot.outboundBytes.Length);
			if (recentPackets == null)
			{
				return;
			}
			recentPackets.Clear();
			foreach (PacketProfilingData recentPacket in snapshot.recentPackets)
			{
				recentPackets.Add(recentPacket);
			}
		}
	}

	public static class Export
	{
		public static PacketProfilerSnapshot CreateSnapshot()
		{
			return new PacketProfilerSnapshot
			{
				inboundCount = inboundCount.ToArray(),
				inboundBytes = inboundBytes.ToArray(),
				outboundCount = outboundCount.ToArray(),
				outboundSum = outboundSum.ToArray(),
				outboundMsgBytes = outboundMsgBytes.ToArray(),
				outboundBytes = outboundBytes.ToArray(),
				recentPackets = recentPackets.ToList()
			};
		}

		public static void ToFile(PacketProfilerSnapshot snapshot)
		{
			if (snapshot == null)
			{
				return;
			}
			try
			{
				string text = Path.Combine(Path.GetDirectoryName(Application.dataPath), "networkprofiles");
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				string contents = JsonConvert.SerializeObject((object)snapshot);
				string text2 = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
				string text3 = Path.Combine(text, "networkprofile_" + text2 + ".json");
				File.WriteAllText(text3, contents);
				Debug.Log((object)("[PacketProfiler] Exported profile to: " + text3));
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("[PacketProfiler] Failed to export profile to file: " + ex.Message));
			}
		}
	}

	public static bool enabled = false;

	public static bool detailedProfiling = false;

	public static int[] inboundCount = new int[29];

	public static int[] inboundBytes = new int[29];

	public static int[] outboundCount = new int[29];

	public static int[] outboundMsgBytes = new int[29];

	public static int[] outboundSum = new int[29];

	public static int[] outboundBytes = new int[29];

	public static ConcurrentBag<PacketProfilingData> recentPackets = new ConcurrentBag<PacketProfilingData>();

	public static bool shouldCaptureDetailedProfiling
	{
		get
		{
			if (enabled)
			{
				return detailedProfiling;
			}
			return false;
		}
	}

	public static void LogDetailedInbound(Message.Type type, NetworkableId entityId, string entityName, int length, byte[] data, int timestamp, bool server, string info = "")
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		LogDetailed(server ? PacketProfilingData.PacketRealm.Server : PacketProfilingData.PacketRealm.Client, PacketProfilingData.PacketProfilingDirection.Inbound, type, entityId, entityName, length, data, timestamp, info);
	}

	public static void LogDetailedOutbound(Message.Type type, NetworkableId entityId, string entityName, int length, byte[] data, int timestamp, bool server, string info = "")
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		LogDetailed(server ? PacketProfilingData.PacketRealm.Server : PacketProfilingData.PacketRealm.Client, PacketProfilingData.PacketProfilingDirection.Outbound, type, entityId, entityName, length, data, timestamp, info);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void LogDetailed(PacketProfilingData.PacketRealm realm, PacketProfilingData.PacketProfilingDirection direction, Message.Type type, NetworkableId entityId, string entityName, int length, byte[] data, int timestamp, string info)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (enabled && (int)type < 29)
		{
			recentPackets.Add(new PacketProfilingData
			{
				OriginRealm = realm,
				Direction = direction,
				PacketType = type,
				AssociatedEntityName = entityName,
				AssociatedEntityId = entityId,
				PacketByteLength = length,
				PacketData = data,
				Info = info,
				Timestamp = timestamp
			});
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LogSimpleInbound(Message.Type type, int length)
	{
		if (enabled && (int)type < 29)
		{
			inboundCount[(uint)type]++;
			inboundBytes[(uint)type] += length;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LogSimpleOutbound(int type, int connectionCount, int length, int totalLength)
	{
		if (enabled && type < 29)
		{
			Interlocked.Increment(ref outboundCount[type]);
			Interlocked.Add(ref outboundSum[type], connectionCount);
			Interlocked.Add(ref outboundMsgBytes[type], length);
			Interlocked.Add(ref outboundBytes[type], totalLength);
		}
	}

	public static void Reset()
	{
		for (int i = 0; i < 29; i++)
		{
			inboundCount[i] = 0;
			inboundBytes[i] = 0;
			outboundCount[i] = 0;
			outboundSum[i] = 0;
			outboundMsgBytes[i] = 0;
			outboundBytes[i] = 0;
		}
		recentPackets.Clear();
	}
}

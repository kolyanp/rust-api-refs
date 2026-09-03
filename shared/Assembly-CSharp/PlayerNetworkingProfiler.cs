using System;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch.Network.Raknet;
using Facepunch.Rust;
using Network;

public static class PlayerNetworkingProfiler
{
	public static int level = 0;

	public static TimeSpan MinFlushInterval = TimeSpan.FromSeconds(1.0);

	public static int ConnectionsPerFrame = 30;

	private static int currentIndex;

	private static DateTime flushCooldown;

	public static void Serialize(AnalyticsTable table, int frameIndex, DateTime timestamp)
	{
		if (level == 0)
		{
			return;
		}
		if (currentIndex >= Net.sv.connections.Count)
		{
			if (flushCooldown > DateTime.UtcNow)
			{
				return;
			}
			flushCooldown = DateTime.UtcNow + MinFlushInterval;
			currentIndex = 0;
		}
		Network.Server sv = Net.sv;
		Server val = (Server)(object)((sv is Server) ? sv : null);
		if (val != null)
		{
			SerializeRaknet(table, frameIndex, timestamp, val);
		}
	}

	private static void SerializeRaknet(AnalyticsTable table, int frameIndex, DateTime timestamp, Server server)
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		int num = Math.Min(((Network.Server)(object)server).connections.Count, currentIndex + ConnectionsPerFrame);
		RaknetStats val = default(RaknetStats);
		while (currentIndex < num)
		{
			Connection connection = ((Network.Server)(object)server).connections[currentIndex];
			if (server.TryGetConnectionStats(connection, ref val))
			{
				string value = connection.IPAddressWithoutPort();
				string value2 = connection.Port();
				int latestPing = server.GetLatestPing(connection);
				EventRecord eventRecord = EventRecord.CSV().AddField("", timestamp).AddField("", frameIndex)
					.AddField("", ConVar.Server.server_id)
					.AddField("", connection.guid)
					.AddField("", connection.userid)
					.AddField("", value)
					.AddField("", value2)
					.AddField("", latestPing)
					.AddField("", val.connectionStartTime)
					.AddField("", val.isLimitedByCongestionControl)
					.AddField("", val.isLimitedByOutgoingBandwidthLimit)
					.AddField("", val.BPSLimitByCongestionControl)
					.AddField("", val.BPSLimitByOutgoingBandwidthLimit)
					.AddField("", val.messagesInResendBuffer)
					.AddField("", val.bytesInResendBuffer)
					.AddField("", val.packetlossLastSecond)
					.AddField("", val.packetlossTotal);
				for (int i = 0; i < 4; i++)
				{
					eventRecord.AddField("", (ulong)System.Runtime.CompilerServices.Unsafe.Add(ref val.bytesInSendBuffer.FixedElementField, i));
				}
				for (int j = 0; j < 4; j++)
				{
					eventRecord.AddField("", (ulong)System.Runtime.CompilerServices.Unsafe.Add(ref val.messageInSendBuffer.FixedElementField, j));
				}
				eventRecord.AddField("", val.runningTotal.FixedElementField);
				eventRecord.AddField("", System.Runtime.CompilerServices.Unsafe.Add(ref val.runningTotal.FixedElementField, 1));
				eventRecord.AddField("", System.Runtime.CompilerServices.Unsafe.Add(ref val.runningTotal.FixedElementField, 2));
				eventRecord.AddField("", System.Runtime.CompilerServices.Unsafe.Add(ref val.runningTotal.FixedElementField, 3));
				eventRecord.AddField("", System.Runtime.CompilerServices.Unsafe.Add(ref val.runningTotal.FixedElementField, 4));
				eventRecord.AddField("", System.Runtime.CompilerServices.Unsafe.Add(ref val.runningTotal.FixedElementField, 5));
				eventRecord.AddField("", System.Runtime.CompilerServices.Unsafe.Add(ref val.runningTotal.FixedElementField, 6));
				table.Append(eventRecord);
			}
			currentIndex++;
		}
	}
}

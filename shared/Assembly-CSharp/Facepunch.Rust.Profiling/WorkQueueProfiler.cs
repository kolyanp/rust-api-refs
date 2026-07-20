using System;
using ConVar;
using UnityEngine;

namespace Facepunch.Rust.Profiling;

public static class WorkQueueProfiler
{
	public static bool enabled;

	public static void Serialize(AnalyticsTable table, int frameIndex, DateTime timestamp)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		if (!enabled)
		{
			return;
		}
		try
		{
			WorkQueueTelemStats val = default(WorkQueueTelemStats);
			foreach (ObjectWorkQueue item in ObjectWorkQueue.All)
			{
				((WorkQueueTelemStats)(ref val)).Append(item.Stats);
				if (item.Stats.ProcessedCount != 0)
				{
					EventRecord eventRecord = EventRecord.CSV();
					eventRecord.AddField("frame_index", frameIndex).AddField("Timestamp", timestamp).AddField("QueueName", item.Name)
						.AddField("StartCount", item.Stats.QueueCount)
						.AddField("TimeTaken", item.Stats.ExecutionTime)
						.AddField("ProcessedCount", item.Stats.ProcessedCount)
						.AddField("server_id", Server.server_id)
						.AddField("BudgetTime", item.Stats.BudgetTime);
					table.Append(eventRecord);
				}
			}
			foreach (PersistentObjectWorkQueue item2 in PersistentObjectWorkQueue.All)
			{
				((WorkQueueTelemStats)(ref val)).Append(item2.Stats);
				if (item2.Stats.ProcessedCount != 0)
				{
					EventRecord eventRecord2 = EventRecord.CSV();
					eventRecord2.AddField("frame_index", frameIndex).AddField("Timestamp", timestamp).AddField("QueueName", item2.Name)
						.AddField("StartCount", item2.Stats.QueueCount)
						.AddField("TimeTaken", item2.Stats.ExecutionTime)
						.AddField("ProcessedCount", item2.Stats.ProcessedCount)
						.AddField("server_id", Server.server_id)
						.AddField("BudgetTime", item2.Stats.BudgetTime);
					table.Append(eventRecord2);
				}
			}
			EventRecord eventRecord3 = EventRecord.CSV();
			eventRecord3.AddField("frame_index", frameIndex).AddField("Timestamp", timestamp).AddField("QueueName", "Aggregate")
				.AddField("StartCount", val.QueueCount)
				.AddField("TimeTaken", val.ExecutionTime)
				.AddField("ProcessedCount", val.ProcessedCount)
				.AddField("server_id", Server.server_id)
				.AddField("BudgetTime", val.BudgetTime);
			table.Append(eventRecord3);
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Failed to serialize work queues: " + ex.Message));
		}
	}

	public static void Reset()
	{
	}
}

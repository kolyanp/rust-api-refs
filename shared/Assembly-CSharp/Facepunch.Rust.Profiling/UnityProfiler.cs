using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;

namespace Facepunch.Rust.Profiling;

public static class UnityProfiler
{
	private struct RecorderInfo
	{
		public string MethodName;

		public ProfilerRecorder Recorder;
	}

	private static List<RecorderInfo> ActiveRecorders = new List<RecorderInfo>();

	private static bool _enabled;

	public static bool enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			SetEnabled(value);
		}
	}

	private static void SetEnabled(bool state)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Invalid comparison between Unknown and I4
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		_enabled = state;
		Unload();
		if (!state)
		{
			return;
		}
		List<ProfilerRecorderHandle> list = new List<ProfilerRecorderHandle>();
		ProfilerRecorderHandle.GetAvailable(list);
		foreach (ProfilerRecorderDescription item2 in list.Select((ProfilerRecorderHandle x) =>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return ProfilerRecorderHandle.GetDescription(x);
		}).ToList())
		{
			ProfilerRecorderDescription current = item2;
			if ((int)current.UnitType == 1 && ProfilerCategory.op_Implicit(current.Category) == ProfilerCategory.op_Implicit(ProfilerCategory.Scripts))
			{
				ProfilerRecorder recorder = ProfilerRecorder.StartNew(current.Category, current.Name, 2, (ProfilerRecorderOptions)24);
				RecorderInfo item = new RecorderInfo
				{
					MethodName = current.Name,
					Recorder = recorder
				};
				ActiveRecorders.Add(item);
			}
		}
	}

	public static void Unload()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		foreach (RecorderInfo activeRecorder in ActiveRecorders)
		{
			ProfilerRecorder recorder = activeRecorder.Recorder;
			recorder.Dispose();
		}
		ActiveRecorders.Clear();
	}

	public static void Serialize(AnalyticsTable table, DateTime timestamp, int frameIndex)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (!enabled)
		{
			return;
		}
		try
		{
			foreach (RecorderInfo activeRecorder in ActiveRecorders)
			{
				ProfilerRecorder recorder = activeRecorder.Recorder;
				if (recorder.LastValue != 0L)
				{
					EventRecord eventRecord = EventRecord.CSV();
					EventRecord eventRecord2 = eventRecord.AddField("", frameIndex).AddField("", timestamp).AddField("", activeRecorder.MethodName);
					recorder = activeRecorder.Recorder;
					eventRecord2.AddField("", recorder.LastValue).AddField("", Server.server_id);
					table.Append(eventRecord);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Failed to serialize profiler data: " + ex.Message));
		}
	}
}

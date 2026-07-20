using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Facepunch;
using Facepunch.Utility;
using ProtoBuf;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

public class VDDraw : SingletonComponent<VDDraw>, IServerComponent
{
	private const int maxLogFiles = 10;

	private const string prefix = "VDDraw";

	private const string extension = "vddraw";

	private const string compressionExtension = "gz";

	private const float saveIntervalSeconds = 60f;

	private ConcurrentQueue<VDDrawEntry> logEntries = new ConcurrentQueue<VDDrawEntry>();

	private bool _isRecording;

	private string currentLogPath;

	private static readonly HashSet<string> disabledLabels = new HashSet<string> { "speed", "ground snap", "fsm tick", "targeting scores", "fire", "food", "aim", "shots" };

	public static bool isRecording
	{
		get
		{
			if (!((Object)(object)SingletonComponent<VDDraw>.Instance != (Object)null))
			{
				return false;
			}
			return SingletonComponent<VDDraw>.Instance._isRecording;
		}
	}

	private static string logDirectoryPath => Path.Combine(Application.persistentDataPath, "Logs");

	[ServerVar(Help = "(Generated) Toggles or explicitly sets VDDraw recording state; when enabled starts capturing DDraw commands for replay; when disabled stops recording")]
	public static void SetIsRecording(ConsoleSystem.Arg arg)
	{
		bool flag = (arg.HasArgs() ? arg.GetBool(0) : (!SingletonComponent<VDDraw>.Instance._isRecording));
		if (SingletonComponent<VDDraw>.Instance._isRecording == flag)
		{
			if (SingletonComponent<VDDraw>.Instance._isRecording)
			{
				arg.ReplyWith("Already recording");
			}
			else
			{
				arg.ReplyWith("Not recording");
			}
		}
		else if (flag)
		{
			SingletonComponent<VDDraw>.Instance.StartRecording();
			arg.ReplyWith("Recording started");
		}
		else
		{
			SingletonComponent<VDDraw>.Instance.StopRecording();
			arg.ReplyWith("Recording stopped");
		}
	}

	private void StartRecording()
	{
		if (!_isRecording)
		{
			_isRecording = true;
			if (!Directory.Exists(logDirectoryPath))
			{
				Directory.CreateDirectory(logDirectoryPath);
			}
			ManageLogFiles();
			currentLogPath = GetNewLogFilePath();
			Application.quitting += StopRecording;
		}
	}

	private void StopRecording()
	{
		if (_isRecording)
		{
			_isRecording = false;
			Stopwatch stopwatch = Stopwatch.StartNew();
			SaveLogs();
			stopwatch.Stop();
			Debug.Log((object)$"VDDraw: Saved logs in {stopwatch.ElapsedMilliseconds} ms");
			stopwatch.Reset();
			CompressCurrentLog();
			stopwatch.Stop();
			Debug.Log((object)$"VDDraw: Compressed logs in {stopwatch.ElapsedMilliseconds} ms");
			currentLogPath = null;
			Application.quitting -= StopRecording;
		}
	}

	private static bool ShouldRecord(string label)
	{
		if (SingletonComponent<VDDraw>.Instance._isRecording)
		{
			return !disabledLabels.Contains(label);
		}
		return false;
	}

	public static void Log(BaseEntity entity, bool display, string label, string message)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (entity.IsValid() && ShouldRecord(label))
		{
			VDDrawEntry val = Pool.Get<VDDrawEntry>();
			val.entityName = GetEntityReadableName(entity);
			val.label = label;
			val.frame = Time.frameCount;
			val.category = (Category)1;
			val.message = message;
			SingletonComponent<VDDraw>.Instance.logEntries.Enqueue(val);
		}
	}

	public static void Line(BaseEntity entity, bool display, string label, Vector3 start, Vector3 end, Color color, float duration = 0f)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (entity.IsValid() && ShouldRecord(label))
		{
			VDDrawEntry val = Pool.Get<VDDrawEntry>();
			val.entityName = GetEntityReadableName(entity);
			val.label = label;
			val.frame = Time.frameCount;
			val.category = (Category)0;
			val.start = start;
			val.end = end;
			val.color = color;
			SingletonComponent<VDDraw>.Instance.logEntries.Enqueue(val);
		}
	}

	public static void Text(BaseEntity entity, bool display, string label, string message, Vector3 position, Color color, float duration = 0f, float scaleMulti = 1f, bool alsoLog = false)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (entity.IsValid())
		{
			if (ShouldRecord(label))
			{
				VDDrawEntry val = Pool.Get<VDDrawEntry>();
				val.entityName = GetEntityReadableName(entity);
				val.label = label;
				val.frame = Time.frameCount;
				val.category = (Category)2;
				val.start = position;
				val.message = message;
				val.color = color;
				SingletonComponent<VDDraw>.Instance.logEntries.Enqueue(val);
			}
			if (alsoLog)
			{
				Log(entity, display, label, message);
			}
		}
	}

	public static void Sphere(BaseEntity entity, bool display, string label, Vector3 position, float size, Color color, float duration = 0f)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (entity.IsValid() && ShouldRecord(label))
		{
			VDDrawEntry val = Pool.Get<VDDrawEntry>();
			val.entityName = GetEntityReadableName(entity);
			val.label = label;
			val.frame = Time.frameCount;
			val.category = (Category)3;
			val.start = position;
			val.sizeX = size;
			val.color = color;
			SingletonComponent<VDDraw>.Instance.logEntries.Enqueue(val);
		}
	}

	public static void Box(BaseEntity entity, bool display, string label, Vector3 center, Quaternion rotation, Vector3 size, Color color, float duration = 0f)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (entity.IsValid() && ShouldRecord(label))
		{
			VDDrawEntry val = Pool.Get<VDDrawEntry>();
			val.entityName = GetEntityReadableName(entity);
			val.label = label;
			val.frame = Time.frameCount;
			val.category = (Category)4;
			val.start = center;
			val.end = rotation * Vector3.forward;
			val.sizeX = size.x;
			val.sizeY = size.y;
			val.sizeZ = size.z;
			val.color = color;
			SingletonComponent<VDDraw>.Instance.logEntries.Enqueue(val);
		}
	}

	private static string GetNewLogFilePath()
	{
		string text = DateTime.Now.ToString("yyyyMMdd_HHmmss");
		return Path.Combine(logDirectoryPath, "VDDraw_" + text + ".vddraw");
	}

	public static string GetLastLogFilePath()
	{
		List<FileInfo> list = (from f in new DirectoryInfo(logDirectoryPath).GetFiles("VDDraw_*")
			orderby f.CreationTime
			select f).ToList();
		if (list.Count == 0)
		{
			return null;
		}
		return list.Last().FullName;
	}

	private void CompressCurrentLog()
	{
		using (TimeWarning.New("VDDraw:CompressCurrentLog"))
		{
			string lastLogFilePath = GetLastLogFilePath();
			byte[] data = File.ReadAllBytes(lastLogFilePath);
			File.WriteAllBytes(bytes: Compression.Compress(data), path: lastLogFilePath + ".gz");
			File.Delete(lastLogFilePath);
		}
	}

	private void SaveLogs()
	{
		using (TimeWarning.New("VDDraw:SaveLogs"))
		{
			using FileStream fileStream = new FileStream(currentLogPath, FileMode.Append, FileAccess.Write, FileShare.None, 4096, useAsync: false);
			VDDrawEntry result;
			while (logEntries.TryDequeue(out result))
			{
				ProtoStreamExtensions.WriteToStream((IProto)(object)result, (Stream)fileStream, true, 2097152);
				result.Dispose();
			}
		}
	}

	private static void ManageLogFiles()
	{
		List<FileInfo> list = (from f in new DirectoryInfo(logDirectoryPath).GetFiles("VDDraw_*")
			orderby f.CreationTime
			select f).ToList();
		if (list.Count > 10)
		{
			int num = list.Count - 10;
			for (int num2 = 0; num2 < num; num2++)
			{
				Debug.Log((object)("Deleting log file: " + list[num2].FullName));
				list[num2].Delete();
			}
		}
	}

	public unsafe static string GetEntityReadableName(BaseEntity entity)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		string name = ((object)entity).GetType().Name;
		NetworkableId iD = entity.net.ID;
		return name + "_" + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString();
	}
}

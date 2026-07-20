using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Carbon.Profiler;
using Facepunch;
using Facepunch.Extend;
using Newtonsoft.Json;
using UnityEngine;

namespace Carbon.Components;

[SuppressUnmanagedCodeSecurity]
public static class MonoProfiler
{
	public enum ProfilerResultCode : byte
	{
		OK,
		InvalidArgs,
		Aborted,
		MainThreadOnly,
		NotInitialized,
		CorruptedState,
		UnknownError,
		Busy,
		NoOp
	}

	public class AssemblyNameEntry
	{
		public string name;

		public string displayName;

		public string displayNameNonIncrement;

		public MonoProfilerConfig.ProfileTypes profileType;

		public string GetDisplayName(bool isCompared)
		{
			return isCompared ? displayNameNonIncrement : displayName;
		}
	}

	public class AssemblyOutput : List<AssemblyRecord>
	{
		public bool AnyValidRecords => base.Count > 0;

		public AssemblyOutput Compare(AssemblyOutput other)
		{
			if (other == null)
			{
				return null;
			}
			AssemblyOutput assemblyOutput = new AssemblyOutput();
			assemblyOutput.AddRange(from record in this
				let otherRecord = other.FirstOrDefault((AssemblyRecord x) => x.assembly_name.displayNameNonIncrement == record.assembly_name.displayNameNonIncrement)
				select new AssemblyRecord
				{
					assembly_name = record.assembly_name,
					total_time = (AreRecordsValid(record, otherRecord) ? Sample.CompareValue(record.total_time, otherRecord.total_time) : 0),
					total_time_percentage = (AreRecordsValid(record, otherRecord) ? Sample.CompareValue(record.total_time_percentage, otherRecord.total_time_percentage) : 0.0),
					total_exceptions = (AreRecordsValid(record, otherRecord) ? Sample.CompareValue(record.total_exceptions, otherRecord.total_exceptions) : 0),
					calls = (AreRecordsValid(record, otherRecord) ? Sample.CompareValue(record.calls, otherRecord.calls) : 0),
					alloc = (AreRecordsValid(record, otherRecord) ? Sample.CompareValue(record.alloc, otherRecord.alloc) : 0),
					comparison = new AssemblyRecord.Comparison
					{
						isCompared = true,
						total_time = (AreRecordsValid(record, otherRecord) ? Sample.Compare(record.total_time, otherRecord.total_time) : Sample.Difference.None),
						total_exceptions = (AreRecordsValid(record, otherRecord) ? Sample.Compare(record.total_exceptions, otherRecord.total_exceptions) : Sample.Difference.None),
						calls = (AreRecordsValid(record, otherRecord) ? Sample.Compare(record.calls, otherRecord.calls) : Sample.Difference.None),
						alloc = (AreRecordsValid(record, otherRecord) ? Sample.Compare(record.alloc, otherRecord.alloc) : Sample.Difference.None)
					}
				});
			return assemblyOutput;
		}

		public static bool AreRecordsValid(AssemblyRecord recordA, AssemblyRecord recordB)
		{
			return recordA.IsValid && recordB.IsValid;
		}

		public string ToTable()
		{
			TextTable val = Pool.Get<TextTable>();
			val.Clear();
			val.AddColumns(new string[6] { "assembly", "total time", "(%)", "calls", "exceptions", "allocations" });
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					AssemblyRecord current = enumerator.Current;
					if (AssemblyMap.TryGetValue(current.assembly_handle, out var value))
					{
						val.AddRow(new string[6]
						{
							value.GetDisplayName(current.comparison.isCompared) ?? "",
							(current.total_time == 0L) ? string.Empty : current.GetTotalTime(),
							(current.total_time_percentage == 0.0) ? string.Empty : $"{current.total_time_percentage:0}%",
							(current.calls == 0L) ? string.Empty : current.calls.ToString(DecimalFormat),
							(current.total_exceptions == 0L) ? string.Empty : current.total_exceptions.ToString(DecimalFormat),
							NumberExtensions.FormatBytes<ulong>(current.alloc, true)
						});
					}
				}
			}
			string result = ((object)val).ToString();
			Pool.FreeUnsafe<TextTable>(ref val);
			return result;
		}

		public string ToCSV()
		{
			StringBuilder stringBuilder = Pool.Get<StringBuilder>();
			stringBuilder.AppendLine("assembly,total time,(%),calls,exceptions,allocations");
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					AssemblyRecord current = enumerator.Current;
					if (AssemblyMap.TryGetValue(current.assembly_handle, out var value))
					{
						stringBuilder.AppendLine(value.GetDisplayName(current.comparison.isCompared) + "," + current.GetTotalTime() + "," + $"{current.total_time_percentage:0}%," + current.calls.ToString(DecimalFormat) + "," + current.total_exceptions.ToString(DecimalFormat) + "," + NumberExtensions.FormatBytes<ulong>(current.alloc, true));
					}
				}
			}
			string result = stringBuilder.ToString();
			Pool.FreeUnmanaged(ref stringBuilder);
			return result;
		}

		public string ToJson(bool indented)
		{
			return JsonConvert.SerializeObject((object)this, (Formatting)(indented ? 1 : 0));
		}
	}

	public class CallOutput : List<CallRecord>
	{
		public bool Disabled;

		public bool AnyValidRecords => base.Count > 0;

		public CallOutput Compare(CallOutput other)
		{
			if (other == null)
			{
				return null;
			}
			CallOutput callOutput = new CallOutput();
			callOutput.AddRange(from record in this
				let otherRecord = other.FirstOrDefault((CallRecord x) => x.assembly_name.displayNameNonIncrement == record.assembly_name.displayNameNonIncrement && x.method_name == record.method_name)
				select new CallRecord
				{
					assembly_name = record.assembly_name,
					method_name = record.method_name,
					total_time = (AreRecordsValid(record, otherRecord) ? Sample.CompareValue(record.total_time, otherRecord.total_time) : 0),
					total_time_percentage = (AreRecordsValid(record, otherRecord) ? Sample.CompareValue(record.total_time_percentage, otherRecord.total_time_percentage) : 0.0),
					own_time = (AreRecordsValid(record, otherRecord) ? Sample.CompareValue(record.own_time, otherRecord.own_time) : 0),
					own_time_percentage = (AreRecordsValid(record, otherRecord) ? Sample.CompareValue(record.own_time_percentage, otherRecord.own_time_percentage) : 0.0),
					calls = (AreRecordsValid(record, otherRecord) ? Sample.CompareValue(record.calls, otherRecord.calls) : 0),
					total_alloc = (AreRecordsValid(record, otherRecord) ? Sample.CompareValue(record.total_alloc, otherRecord.total_alloc) : 0),
					own_alloc = (AreRecordsValid(record, otherRecord) ? Sample.CompareValue(record.own_alloc, otherRecord.own_alloc) : 0),
					total_exceptions = (AreRecordsValid(record, otherRecord) ? Sample.CompareValue(record.total_exceptions, otherRecord.total_exceptions) : 0),
					own_exceptions = (AreRecordsValid(record, otherRecord) ? Sample.CompareValue(record.own_exceptions, otherRecord.own_exceptions) : 0),
					comparison = new CallRecord.Comparison
					{
						isCompared = true,
						total_time = (AreRecordsValid(record, otherRecord) ? Sample.Compare(record.total_time, otherRecord.total_time) : Sample.Difference.None),
						own_time = (AreRecordsValid(record, otherRecord) ? Sample.Compare(record.own_time, otherRecord.own_time) : Sample.Difference.None),
						calls = (AreRecordsValid(record, otherRecord) ? Sample.Compare(record.calls, otherRecord.calls) : Sample.Difference.None),
						total_alloc = (AreRecordsValid(record, otherRecord) ? Sample.Compare(record.total_alloc, otherRecord.total_alloc) : Sample.Difference.None),
						own_alloc = (AreRecordsValid(record, otherRecord) ? Sample.Compare(record.own_alloc, otherRecord.own_alloc) : Sample.Difference.None),
						total_exceptions = (AreRecordsValid(record, otherRecord) ? Sample.Compare(record.total_exceptions, otherRecord.total_exceptions) : Sample.Difference.None),
						own_exceptions = (AreRecordsValid(record, otherRecord) ? Sample.Compare(record.own_exceptions, otherRecord.own_exceptions) : Sample.Difference.None)
					}
				});
			return callOutput;
		}

		public static bool AreRecordsValid(CallRecord recordA, CallRecord recordB)
		{
			return recordA.IsValid && recordB.IsValid;
		}

		public string ToTable()
		{
			TextTable val = Pool.Get<TextTable>();
			val.Clear();
			val.AddColumns(new string[11]
			{
				"assembly", "method", "total time", "(%)", "own time", "(%)", "calls", "total exceptions", "own exceptions", "total allocations",
				"own allocations"
			});
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					CallRecord current = enumerator.Current;
					if (AssemblyMap.TryGetValue(current.assembly_handle, out var value))
					{
						val.AddRow(new string[11]
						{
							value.GetDisplayName(current.comparison.isCompared) ?? "",
							current.method_name,
							(current.total_time == 0L) ? string.Empty : current.GetTotalTime(),
							(current.total_time_percentage == 0.0) ? string.Empty : $"{current.total_time_percentage:0}%",
							(current.own_time == 0L) ? string.Empty : current.GetOwnTime(),
							(current.own_time_percentage == 0.0) ? string.Empty : $"{current.own_time_percentage:0}%",
							(current.calls == 0L) ? string.Empty : current.calls.ToString(DecimalFormat),
							(current.total_exceptions == 0L) ? string.Empty : current.total_exceptions.ToString(DecimalFormat),
							(current.own_exceptions == 0L) ? string.Empty : current.own_exceptions.ToString(DecimalFormat),
							(current.total_alloc == 0L) ? string.Empty : NumberExtensions.FormatBytes<ulong>(current.total_alloc, true),
							(current.own_alloc == 0L) ? string.Empty : NumberExtensions.FormatBytes<ulong>(current.own_alloc, true)
						});
					}
				}
			}
			string result = ((object)val).ToString();
			Pool.FreeUnsafe<TextTable>(ref val);
			return result;
		}

		public string ToCSV()
		{
			StringBuilder stringBuilder = Pool.Get<StringBuilder>();
			stringBuilder.AppendLine("assembly,method,total time,(%),own time,(%),calls,total exceptions,own exceptions,total allocations,own allocations");
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					CallRecord current = enumerator.Current;
					if (AssemblyMap.TryGetValue(current.assembly_handle, out var value))
					{
						stringBuilder.AppendLine(value.GetDisplayName(current.comparison.isCompared) + "," + current.method_name + "," + current.GetTotalTime() + "," + $"{current.total_time_percentage:0}%," + current.GetOwnTime() + "," + $"{current.own_time_percentage:0}%," + $"{current.calls:n0}," + current.total_exceptions.ToString(DecimalFormat) + "," + current.own_exceptions.ToString(DecimalFormat) + "," + NumberExtensions.FormatBytes<ulong>(current.total_alloc, true) + "," + NumberExtensions.FormatBytes<ulong>(current.own_alloc, true));
					}
				}
			}
			string result = stringBuilder.ToString();
			Pool.FreeUnmanaged(ref stringBuilder);
			return result;
		}

		public string ToJson(bool indented)
		{
			return JsonConvert.SerializeObject((object)this, (Formatting)(indented ? 1 : 0));
		}
	}

	public class MemoryOutput : List<MemoryRecord>
	{
		public MemoryOutput Compare(MemoryOutput other)
		{
			if (other == null)
			{
				return null;
			}
			MemoryOutput memoryOutput = new MemoryOutput();
			memoryOutput.AddRange(from record in this
				let otherRecord = other.FirstOrDefault((MemoryRecord x) => x.assembly_name.displayNameNonIncrement == record.assembly_name.displayNameNonIncrement && x.class_name == record.class_name)
				select new MemoryRecord
				{
					assembly_name = record.assembly_name,
					class_name = record.class_name,
					class_token = record.class_token,
					allocations = (AreRecordsValid(record, otherRecord) ? Sample.CompareValue(record.allocations, otherRecord.allocations) : 0),
					total_alloc_size = (AreRecordsValid(record, otherRecord) ? Sample.CompareValue(record.total_alloc_size, otherRecord.total_alloc_size) : 0),
					instance_size = (AreRecordsValid(record, otherRecord) ? Sample.CompareValue(record.instance_size, otherRecord.instance_size) : 0u),
					comparison = new MemoryRecord.Comparison
					{
						isCompared = true,
						allocations = (AreRecordsValid(record, otherRecord) ? Sample.Compare(record.allocations, otherRecord.allocations) : Sample.Difference.None),
						total_alloc_size = (AreRecordsValid(record, otherRecord) ? Sample.Compare(record.total_alloc_size, otherRecord.total_alloc_size) : Sample.Difference.None)
					}
				});
			return memoryOutput;
		}

		public static bool AreRecordsValid(MemoryRecord recordA, MemoryRecord recordB)
		{
			return recordA.IsValid && recordB.IsValid;
		}

		public string ToTable()
		{
			TextTable val = Pool.Get<TextTable>();
			val.Clear();
			val.AddColumns(new string[5] { "assembly", "class", "allocations", "total allocation size", "instance size" });
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					MemoryRecord current = enumerator.Current;
					if (AssemblyMap.TryGetValue(current.assembly_handle, out var value))
					{
						val.AddRow(new string[5]
						{
							value.GetDisplayName(current.comparison.isCompared) ?? "",
							current.class_name,
							(current.allocations == 0L) ? string.Empty : current.allocations.ToString(DecimalFormat),
							(current.total_alloc_size == 0L) ? string.Empty : (NumberExtensions.FormatBytes<ulong>(current.total_alloc_size, true) ?? ""),
							(current.instance_size == 0) ? string.Empty : (current.instance_size.ToString(DecimalFormat) + "b")
						});
					}
				}
			}
			string result = ((object)val).ToString();
			Pool.FreeUnsafe<TextTable>(ref val);
			return result;
		}

		public string ToCSV()
		{
			StringBuilder stringBuilder = Pool.Get<StringBuilder>();
			stringBuilder.AppendLine("assembly,class,allocations,total allocation size,instance size");
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					MemoryRecord current = enumerator.Current;
					if (AssemblyMap.TryGetValue(current.assembly_handle, out var value))
					{
						stringBuilder.AppendLine(value.GetDisplayName(current.comparison.isCompared) + "," + current.class_name + "," + current.allocations.ToString(DecimalFormat) + "," + NumberExtensions.FormatBytes<ulong>(current.total_alloc_size, true) + "," + current.instance_size.ToString(DecimalFormat) + "b");
					}
				}
			}
			string result = stringBuilder.ToString();
			Pool.FreeUnmanaged(ref stringBuilder);
			return result;
		}

		public string ToJson(bool indented)
		{
			return JsonConvert.SerializeObject((object)this, (Formatting)(indented ? 1 : 0));
		}
	}

	public class RuntimeAssemblyBank : ConcurrentDictionary<string, int>
	{
		public string Increment(string value)
		{
			return string.IsNullOrEmpty(value) ? string.Empty : $"{value} ({AddOrUpdate(value, 1, (string _, int arg) => arg + 1)})";
		}
	}

	public struct GCRecord
	{
		public struct Comparison
		{
			public bool isCompared;

			public Sample.Difference calls_c;

			public Sample.Difference total_time_c;
		}

		public ulong calls;

		public ulong total_time;

		public Comparison comparison;

		private string total_time_ms_str;

		public double total_time_ms => (float)total_time * 0.001f;

		public GCRecord Compare(GCRecord other)
		{
			GCRecord result = default(GCRecord);
			result.calls = Sample.CompareValue(calls, other.calls);
			result.total_time = Sample.CompareValue(total_time, other.total_time);
			result.comparison.isCompared = true;
			result.comparison.calls_c = Sample.Compare(result.calls, other.calls);
			result.comparison.total_time_c = Sample.Compare(result.total_time, other.total_time);
			return result;
		}

		public string ToTable()
		{
			TextTable val = Pool.Get<TextTable>();
			val.Clear();
			val.AddColumns(new string[2] { "calls", "total time" });
			val.AddRow(new string[2]
			{
				$" {calls:n0}",
				GetTotalTime() ?? ""
			});
			string result = ((object)val).ToString();
			Pool.FreeUnsafe<TextTable>(ref val);
			return result;
		}

		public string ToCSV()
		{
			StringBuilder stringBuilder = Pool.Get<StringBuilder>();
			stringBuilder.AppendLine("Calls,Total Time");
			stringBuilder.AppendLine($"{calls}," + GetTotalTime());
			string result = stringBuilder.ToString();
			Pool.FreeUnmanaged(ref stringBuilder);
			return result;
		}

		public string ToJson(bool indented)
		{
			return JsonConvert.SerializeObject((object)this, (Formatting)(indented ? 1 : 0));
		}

		public string GetTotalTime()
		{
			return total_time_ms_str ?? (total_time_ms_str = ((total_time_ms < 10.0) ? $"{total_time:n0}μs" : $"{total_time_ms:n0}ms"));
		}
	}

	public struct AssemblyRecord
	{
		public struct Comparison
		{
			public bool isCompared;

			public Sample.Difference total_time;

			public Sample.Difference total_exceptions;

			public Sample.Difference calls;

			public Sample.Difference alloc;
		}

		[JsonIgnore]
		public ModuleHandle assembly_handle;

		public ulong total_time;

		public double total_time_percentage;

		public ulong total_exceptions;

		public ulong calls;

		public ulong alloc;

		public Comparison comparison;

		public AssemblyNameEntry assembly_name;

		private string total_time_ms_str;

		public double total_time_ms => (float)total_time * 0.001f;

		[JsonIgnore]
		public bool IsValid => assembly_name != null;

		public string GetTotalTime()
		{
			return total_time_ms_str ?? (total_time_ms_str = ((total_time_ms < 10.0) ? $"{total_time:n0}μs" : $"{total_time_ms:n0}ms"));
		}
	}

	public struct MemoryRecord
	{
		public struct Comparison
		{
			public bool isCompared;

			public Sample.Difference allocations;

			public Sample.Difference total_alloc_size;
		}

		[JsonIgnore]
		public ModuleHandle assembly_handle;

		[JsonIgnore]
		public IntPtr class_handle;

		public ulong allocations;

		public ulong total_alloc_size;

		public uint instance_size;

		public uint class_token;

		public AssemblyNameEntry assembly_name;

		public string class_name;

		public Comparison comparison;

		[JsonIgnore]
		public bool IsValid => assembly_name != null;
	}

	public struct CallRecord
	{
		public struct Comparison
		{
			public bool isCompared;

			public Sample.Difference total_time;

			public Sample.Difference own_time;

			public Sample.Difference calls;

			public Sample.Difference total_alloc;

			public Sample.Difference own_alloc;

			public Sample.Difference total_exceptions;

			public Sample.Difference own_exceptions;
		}

		[JsonIgnore]
		public ModuleHandle assembly_handle;

		[JsonIgnore]
		public unsafe MonoMethod* method_handle;

		public ulong total_time;

		public double total_time_percentage;

		public ulong own_time;

		public double own_time_percentage;

		public ulong calls;

		public ulong total_alloc;

		public ulong own_alloc;

		public ulong total_exceptions;

		public ulong own_exceptions;

		public AssemblyNameEntry assembly_name;

		public string method_name;

		public Comparison comparison;

		private string total_time_ms_str;

		private string own_time_ms_str;

		public double total_time_ms => (float)total_time * 0.001f;

		public double own_time_ms => (float)own_time * 0.001f;

		[JsonIgnore]
		public bool IsValid => assembly_name != null;

		public string GetTotalTime()
		{
			return total_time_ms_str ?? (total_time_ms_str = ((total_time_ms < 10.0) ? $"{total_time:n0}μs" : $"{total_time_ms:n0}ms"));
		}

		public string GetOwnTime()
		{
			return own_time_ms_str ?? (own_time_ms_str = ((own_time_ms < 10.0) ? $"{own_time:n0}μs" : $"{own_time_ms:n0}ms"));
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct MonoImageUnion
	{
		[FieldOffset(0)]
		public ModuleHandle handle;

		[FieldOffset(0)]
		public unsafe MonoImage* ptr;
	}

	public readonly struct MonoImage
	{
		public readonly int ref_count;

		public unsafe readonly void* storage;

		public unsafe readonly byte* raw_data;

		public readonly uint raw_data_len;

		public unsafe static ModuleHandle image_to_handle(MonoImage* image)
		{
			MonoImageUnion monoImageUnion = new MonoImageUnion
			{
				ptr = image
			};
			return monoImageUnion.handle;
		}

		public unsafe static MonoImage* handle_to_image(ModuleHandle handle)
		{
			MonoImageUnion monoImageUnion = new MonoImageUnion
			{
				handle = handle
			};
			return monoImageUnion.ptr;
		}
	}

	public readonly struct MonoMethod
	{
		public readonly ushort flags;

		public readonly ushort iflags;

		public readonly uint token;

		public unsafe readonly void* klass;

		public unsafe readonly void* signature;

		public unsafe readonly byte* name;
	}

	[Flags]
	public enum ProfilerArgs : ushort
	{
		None = 0,
		Abort = 1,
		CallMemory = 2,
		AdvancedMemory = 4,
		Timings = 8,
		Calls = 0x10,
		FastResume = 0x20,
		GCEvents = 0x40,
		StackWalkAllocations = 0x80
	}

	private struct ProfilerCallbacks
	{
		private unsafe delegate*<string*, byte*, int, void> string_marshal = &native_string_cb;

		private unsafe delegate*<byte[]*, byte*, ulong, void> bytes_marshal = &memcpy_array_cb;

		private unsafe delegate*<List<AssemblyRecord>*, ulong, IntPtr, delegate*<IntPtr, out AssemblyRecord, bool>, void> basic_iter = &native_iter;

		private unsafe delegate*<List<CallRecord>*, ulong, IntPtr, delegate*<IntPtr, out CallRecord, bool>, void> advanced_iter = &native_iter;

		private unsafe delegate*<List<MemoryRecord>*, ulong, IntPtr, delegate*<IntPtr, out MemoryRecord, bool>, void> memory_iter = &native_iter;

		public unsafe ProfilerCallbacks()
		{
		}
	}

	public enum LogSource : uint
	{
		Native,
		Profiler
	}

	public struct Sample
	{
		public struct SampleComparison
		{
			public Difference Duration;
		}

		public enum Difference
		{
			None,
			ValueHigher,
			ValueEqual,
			ValueLower
		}

		public double Duration;

		public bool IsCompared;

		public AssemblyOutput Assemblies;

		public CallOutput Calls;

		public MemoryOutput Memory;

		public GCRecord GC;

		public SampleComparison Comparison;

		[JsonIgnore]
		public bool FromDisk;

		public const string ValueHigherStr = "<color=#ff370a>↑</color>";

		public const string ValueLowerStr = "<color=#91ff0a>↓</color>";

		public const string ValueEqualStr = "<color=#fff30a>—</color>";

		[JsonIgnore]
		public bool IsCleared => Assemblies == null || !Assemblies.Any();

		public static Sample Create()
		{
			return new Sample
			{
				Duration = 0.0,
				Assemblies = new AssemblyOutput(),
				Calls = new CallOutput(),
				Memory = new MemoryOutput(),
				GC = default(GCRecord)
			};
		}

		public static Sample Load(byte[] data)
		{
			return DeserializeSample(data);
		}

		public Sample Compare(Sample other)
		{
			return new Sample
			{
				FromDisk = true,
				Duration = CompareValue(Duration, other.Duration),
				Comparison = 
				{
					Duration = Compare(Duration, other.Duration)
				},
				Assemblies = Assemblies.Compare(other.Assemblies),
				Calls = Calls.Compare(other.Calls),
				Memory = Memory.Compare(other.Memory),
				GC = GC.Compare(other.GC),
				IsCompared = true
			};
		}

		public void Resample()
		{
			Clear();
			FromDisk = false;
			IsCompared = false;
			Duration = DurationTime.TotalSeconds;
			Comparison = default(SampleComparison);
			Assemblies.AddRange(AssemblyRecords);
			Calls.AddRange(CallRecords);
			Memory.AddRange(MemoryRecords);
			GC = GCStats;
		}

		public void Clear()
		{
			IsCompared = false;
			Duration = 0.0;
			Comparison = default(SampleComparison);
			FromDisk = false;
			if (Assemblies == null)
			{
				Assemblies = new AssemblyOutput();
			}
			if (Calls == null)
			{
				Calls = new CallOutput();
			}
			if (Memory == null)
			{
				Memory = new MemoryOutput();
			}
			Assemblies.Clear();
			Calls.Clear();
			Memory.Clear();
			GC = default(GCRecord);
		}

		public string ToTable()
		{
			StringBuilder stringBuilder = Pool.Get<StringBuilder>();
			stringBuilder.AppendLine(Assemblies.ToTable());
			stringBuilder.AppendLine(Calls.ToTable());
			stringBuilder.AppendLine(Memory.ToTable());
			stringBuilder.AppendLine(GC.ToTable());
			string result = stringBuilder.ToString();
			Pool.FreeUnmanaged(ref stringBuilder);
			return result;
		}

		public string ToCSV()
		{
			StringBuilder stringBuilder = Pool.Get<StringBuilder>();
			stringBuilder.AppendLine(Assemblies.ToCSV());
			stringBuilder.AppendLine(Calls.ToCSV());
			stringBuilder.AppendLine(Memory.ToCSV());
			stringBuilder.AppendLine(GC.ToCSV());
			string result = stringBuilder.ToString();
			Pool.FreeUnmanaged(ref stringBuilder);
			return result;
		}

		public string ToJson(bool indented)
		{
			return JsonConvert.SerializeObject((object)this, (Formatting)(indented ? 1 : 0));
		}

		public byte[] ToProto()
		{
			return SerializeSample(this);
		}

		public static Difference Compare(ulong a, ulong b)
		{
			if (a == b)
			{
				return Difference.ValueEqual;
			}
			return (a > b) ? Difference.ValueHigher : Difference.ValueLower;
		}

		public static Difference Compare(uint a, uint b)
		{
			if (a == b)
			{
				return Difference.ValueEqual;
			}
			return (a > b) ? Difference.ValueHigher : Difference.ValueLower;
		}

		public static Difference Compare(double a, double b)
		{
			if (a == b)
			{
				return Difference.ValueEqual;
			}
			return (a > b) ? Difference.ValueHigher : Difference.ValueLower;
		}

		public static string GetDifferenceString(Difference difference)
		{
			if (1 == 0)
			{
			}
			string result = difference switch
			{
				Difference.ValueHigher => "<color=#ff370a>↑</color>", 
				Difference.ValueEqual => "<color=#fff30a>—</color>", 
				Difference.ValueLower => "<color=#91ff0a>↓</color>", 
				_ => string.Empty, 
			};
			if (1 == 0)
			{
			}
			return result;
		}

		public static ulong CompareValue(ulong a, ulong b)
		{
			return Max(a, b) - Min(a, b);
		}

		public static uint CompareValue(uint a, uint b)
		{
			return Max(a, b) - Min(a, b);
		}

		public static double CompareValue(double a, double b)
		{
			return Max(a, b) - Min(a, b);
		}

		private static ulong Max(ulong a, ulong b)
		{
			return (a > b) ? a : b;
		}

		private static ulong Min(ulong a, ulong b)
		{
			return (a < b) ? a : b;
		}

		private static uint Max(uint a, uint b)
		{
			return (a > b) ? a : b;
		}

		private static uint Min(uint a, uint b)
		{
			return (a < b) ? a : b;
		}

		private static double Max(double a, double b)
		{
			return (a > b) ? a : b;
		}

		private static double Min(double a, double b)
		{
			return (a < b) ? a : b;
		}
	}

	public class TimelineRecording
	{
		public enum StatusTypes
		{
			None,
			Running,
			Discarded,
			Completed
		}

		public StatusTypes Status;

		public float Rate;

		public float Duration;

		public Timeline Timeline = new Timeline();

		public ProfilerArgs Args = ProfilerArgs.CallMemory | ProfilerArgs.AdvancedMemory | ProfilerArgs.Timings | ProfilerArgs.Calls | ProfilerArgs.GCEvents;

		public Action<Sample> OnSample;

		public Action<bool> OnStopped;

		private DateTime _timeSinceStart;

		public double CurrentDuration => (DateTime.Now - _timeSinceStart).TotalSeconds;

		public bool IsRecording()
		{
			return Status == StatusTypes.Running;
		}

		public bool IsDiscarded()
		{
			return Status == StatusTypes.Discarded;
		}

		public bool IsClear()
		{
			return Timeline.Count == 0;
		}

		private Sample Record(AssemblyOutput assemblies, CallOutput calls, MemoryOutput memory, GCRecord gc)
		{
			Sample sample = Sample.Create();
			sample.Assemblies.AddRange(assemblies);
			sample.Calls.AddRange(calls);
			sample.Memory.AddRange(memory);
			sample.GC = gc;
			Record(sample);
			return sample;
		}

		private void Record(Sample snapshot)
		{
			Timeline.Add(DateTime.Now, snapshot);
		}

		private void Clear()
		{
			foreach (KeyValuePair<DateTime, Sample> item in Timeline)
			{
				item.Value.Clear();
			}
			Timeline.Clear();
			Duration = 0f;
			Rate = 0f;
		}

		public TimelineRecording Start(float rate, float duration, ProfilerArgs args, Action<bool> onStopped)
		{
			if (Status == StatusTypes.Running)
			{
				Debug.LogWarning((object)"Timeline is already recording.");
				return this;
			}
			Rate = rate;
			Duration = duration;
			Args = args | ProfilerArgs.FastResume;
			OnStopped = onStopped;
			if (MonoProfiler.IsRecording)
			{
				ToggleProfiling(ProfilerArgs.Abort);
			}
			_timeSinceStart = DateTime.Now;
			Status = StatusTypes.Running;
			Debug.LogWarning((object)"Started timeline recording..");
			Recurse(this);
			return this;
			static void Recurse(TimelineRecording recording)
			{
				ToggleProfilingTimed(recording.Rate, recording.Args, delegate
				{
					Sample obj = recording.Record(AssemblyRecords, CallRecords, MemoryRecords, GCStats);
					recording.OnSample?.Invoke(obj);
					if (recording.CurrentDuration >= (double)recording.Duration)
					{
						recording.Stop();
					}
					else
					{
						Recurse(recording);
					}
				}, logging: false);
			}
		}

		public void Stop(bool discard = false)
		{
			if (MonoProfiler.IsRecording)
			{
				Sample obj = Record(AssemblyRecords, CallRecords, MemoryRecords, GCStats);
				OnSample?.Invoke(obj);
			}
			Debug.LogWarning((object)("Ended timeline recording." + (discard ? " Discarded." : string.Empty)));
			if (discard)
			{
				Discard();
			}
			else
			{
				Status = StatusTypes.Completed;
			}
			OnStopped?.Invoke(discard);
		}

		public void Discard()
		{
			if (MonoProfiler.IsRecording)
			{
				ToggleProfiling(ProfilerArgs.Abort, logging: false);
			}
			Clear();
			Status = StatusTypes.Discarded;
		}

		public static TimelineRecording Create(float rate, float duration, ProfilerArgs args, Action<bool> onStopped)
		{
			return new TimelineRecording().Start(rate, duration, args, onStopped);
		}
	}

	public class Timeline : Dictionary<DateTime, Sample>
	{
	}

	public const string ProfileExtension = "cprf";

	public static readonly string DecimalFormat;

	public const ProfilerArgs AllFlags = ProfilerArgs.CallMemory | ProfilerArgs.AdvancedMemory | ProfilerArgs.Timings | ProfilerArgs.Calls | ProfilerArgs.GCEvents;

	public const ProfilerArgs AllNoTimingsFlags = ProfilerArgs.CallMemory | ProfilerArgs.AdvancedMemory | ProfilerArgs.Calls | ProfilerArgs.GCEvents;

	public static GCRecord GCStats;

	public static AssemblyOutput AssemblyRecords;

	public static CallOutput CallRecords;

	public static MemoryOutput MemoryRecords;

	public static RuntimeAssemblyBank AssemblyBank;

	public static ConcurrentDictionary<ModuleHandle, AssemblyNameEntry> AssemblyMap;

	public static Dictionary<IntPtr, string> ClassMap;

	public static Dictionary<IntPtr, string> MethodMap;

	public static TimeSpan DataProcessingTime;

	public static TimeSpan DurationTime;

	private static Stopwatch _dataProcessTimer;

	private static Stopwatch _durationTimer;

	private static Action _profileTimer;

	private static Action _profileWarningTimer;

	public const int NATIVE_PROTOCOL = 4;

	public const int MANAGED_PROTOCOL = 127;

	public static TimeSpan CurrentDurationTime => (_durationTimer?.Elapsed).GetValueOrDefault();

	public static bool Enabled { get; }

	public static bool IsRecording { get; private set; }

	public static bool Crashed { get; }

	public static bool IsCleared => AssemblyRecords.Count == 0 && CallRecords.Count == 0;

	unsafe static MonoProfiler()
	{
		DecimalFormat = "n0";
		AssemblyRecords = new AssemblyOutput();
		CallRecords = new CallOutput();
		MemoryRecords = new MemoryOutput();
		AssemblyBank = new RuntimeAssemblyBank();
		AssemblyMap = new ConcurrentDictionary<ModuleHandle, AssemblyNameEntry>();
		ClassMap = new Dictionary<IntPtr, string>();
		MethodMap = new Dictionary<IntPtr, string>();
		try
		{
			ulong num = carbon_get_protocol();
			if (num != 4)
			{
				Debug.LogError((object)$"Native protocol mismatch (native) {num} != (managed) {4}");
				Enabled = false;
				Crashed = true;
			}
			else
			{
				ProfilerCallbacks profilerCallbacks = new ProfilerCallbacks();
				profiler_register_callbacks(&profilerCallbacks);
				Enabled = profiler_is_enabled();
				carbon_init_logger((delegate*<int, int, byte*, int, LogSource, void>)(&native_logger));
			}
		}
		catch (Exception arg)
		{
			Crashed = true;
			Debug.LogError((object)$"NativeInitFailure {arg}");
		}
	}

	private unsafe static void native_logger(int level, int verbosity, byte* data, int length, LogSource source)
	{
		Debug.Log((object)$"[{source}] {Encoding.UTF8.GetString(data, length)}");
	}

	private unsafe static void native_string_cb(string* target, byte* ptr, int len)
	{
		Unsafe.Write(target, Encoding.UTF8.GetString(ptr, len));
	}

	private unsafe static void memcpy_array_cb<T>(T[]* target, T* src, ulong len)
	{
		T[] array = Unsafe.Write(target, new T[len]);
		ulong num = len * (uint)Unsafe.SizeOf<T>();
		fixed (T* destination = array)
		{
			Buffer.MemoryCopy(src, destination, num, num);
		}
	}

	private unsafe static void native_iter<T>(List<T>* data, ulong length, IntPtr iter, delegate*<IntPtr, out T, bool> cb) where T : struct
	{
		if (*data == null)
		{
			Unsafe.Write(data, new List<T>((int)length));
		}
		else if (length > (ulong)data->Capacity)
		{
			data->Capacity = (int)length;
		}
		T item = default(T);
		while (cb(iter, out item))
		{
			data->Add(item);
		}
	}

	public static void Clear()
	{
		AssemblyRecords.Clear();
		CallRecords.Clear();
		MemoryRecords.Clear();
		DurationTime = default(TimeSpan);
		GCStats = default(GCRecord);
	}

	public static void ToggleProfilingTimed(float duration, ProfilerArgs args = ProfilerArgs.CallMemory | ProfilerArgs.AdvancedMemory | ProfilerArgs.Timings | ProfilerArgs.Calls | ProfilerArgs.GCEvents, Action<ProfilerArgs> onTimerEnded = null, bool logging = true)
	{
		if (Crashed)
		{
			Debug.LogError((object)"CarbonNative did not properly initialize. Please report to the developers.");
			return;
		}
		if (_profileTimer != null)
		{
			HarmonyProfiler.Runner.CancelInvoke(_profileTimer);
			_profileTimer = null;
		}
		if (_profileWarningTimer != null)
		{
			HarmonyProfiler.Runner.CancelInvoke(_profileWarningTimer);
			_profileWarningTimer = null;
		}
		if (ToggleProfiling(args, logging) != true && logging)
		{
			PrintWarn();
		}
		if (duration >= 1f && IsRecording)
		{
			if (logging)
			{
				Debug.LogWarning((object)("[MonoProfiler] Profiling duration " + NumberExtensions.FormatSeconds((long)duration) + ".."));
			}
			HarmonyProfiler.Runner.Invoke(_profileTimer = delegate
			{
				if (IsRecording)
				{
					_ = ToggleProfiling(args, logging) == true;
					if (logging)
					{
						PrintWarn();
					}
					onTimerEnded?.Invoke(args);
					Clear();
				}
			}, duration);
		}
		else if (IsRecording && logging)
		{
			HarmonyProfiler.Runner.Invoke(_profileWarningTimer = delegate
			{
				Debug.LogWarning((object)$" Reminder: You've been profiling for {CurrentDurationTime.TotalSeconds}s");
			}, 300f);
		}
		static void PrintWarn()
		{
			TextTable val = Pool.Get<TextTable>();
			val.Clear();
			val.AddColumns(new string[4] { " duration", "processing", "assemblies", "calls" });
			val.AddRow(new string[4]
			{
				$" {DurationTime.TotalSeconds}s",
				$"{DataProcessingTime.TotalMilliseconds:0}ms",
				AssemblyRecords.Count.ToString(),
				CallRecords.Count.ToString()
			});
			Debug.LogWarning((object)((object)val).ToString());
			Pool.FreeUnsafe<TextTable>(ref val);
		}
	}

	public unsafe static bool? ToggleProfiling(ProfilerArgs args = ProfilerArgs.CallMemory | ProfilerArgs.AdvancedMemory | ProfilerArgs.Timings | ProfilerArgs.Calls | ProfilerArgs.GCEvents, bool logging = true)
	{
		if (!Enabled)
		{
			Debug.Log((object)"Profiler disabled");
			return null;
		}
		AssemblyRecords.Clear();
		CallRecords.Clear();
		MemoryRecords.Clear();
		List<AssemblyRecord> assemblyRecords = AssemblyRecords;
		List<CallRecord> callRecords = CallRecords;
		List<MemoryRecord> memoryRecords = MemoryRecords;
		GCRecord gCStats = default(GCRecord);
		if (IsRecording)
		{
			_dataProcessTimer = Pool.Get<Stopwatch>();
			_dataProcessTimer.Start();
		}
		bool flag = default(bool);
		ProfilerResultCode profilerResultCode = profiler_toggle(args, &flag, &gCStats, &assemblyRecords, &callRecords, &memoryRecords);
		if (profilerResultCode == ProfilerResultCode.Aborted)
		{
			if (logging)
			{
				Debug.LogWarning((object)"[MonoProfiler] Profiler aborted");
			}
			IsRecording = false;
			return false;
		}
		if (!flag)
		{
			DataProcessingTime = _dataProcessTimer?.Elapsed ?? TimeSpan.Zero;
			if (_dataProcessTimer != null)
			{
				Pool.FreeUnmanaged(ref _dataProcessTimer);
			}
		}
		if (profilerResultCode != ProfilerResultCode.OK)
		{
			Debug.LogError((object)$"[MonoProfiler] Failed to toggle profiler: {profilerResultCode}");
			return null;
		}
		if (assemblyRecords != null && assemblyRecords.Count > 0)
		{
			MapAssemblyRecords(assemblyRecords);
		}
		if (callRecords != null && callRecords.Count > 0)
		{
			MapCallRecords(callRecords);
		}
		if (memoryRecords != null && memoryRecords.Count > 0)
		{
			MapMemoryRecords(memoryRecords);
		}
		GCStats = gCStats;
		CallRecords.Disabled = callRecords.Count == 0;
		IsRecording = flag;
		if (flag)
		{
			if (logging)
			{
				Debug.LogWarning((object)"[MonoProfiler] Started recording..");
			}
			_durationTimer = Pool.Get<Stopwatch>();
			_durationTimer.Start();
		}
		else
		{
			if (logging)
			{
				Debug.LogWarning((object)"[MonoProfiler] Recording ended");
			}
			DurationTime = _durationTimer.Elapsed;
			Pool.FreeUnmanaged(ref _durationTimer);
		}
		return flag;
	}

	private unsafe static void MapAssemblyRecords(List<AssemblyRecord> records)
	{
		string text = default(string);
		for (int i = 0; i < records.Count; i++)
		{
			AssemblyRecord value = records[i];
			if (AssemblyMap.TryGetValue(value.assembly_handle, out var value2))
			{
				value.assembly_name = value2;
			}
			else
			{
				get_image_name(&text, value.assembly_handle);
				if (text == null)
				{
					throw new NullReferenceException();
				}
				value2 = new AssemblyNameEntry
				{
					name = text,
					displayName = text,
					displayNameNonIncrement = text,
					profileType = MonoProfilerConfig.ProfileTypes.Assembly
				};
				AssemblyMap[value.assembly_handle] = value2;
				value.assembly_name = value2;
			}
			records[i] = value;
		}
	}

	private unsafe static void MapMemoryRecords(List<MemoryRecord> records)
	{
		string text = default(string);
		for (int i = 0; i < records.Count; i++)
		{
			MemoryRecord value = records[i];
			if (ClassMap.TryGetValue(value.class_handle, out var value2))
			{
				value.class_name = value2;
			}
			else
			{
				get_class_name(&value2, value.class_handle);
				if (value2 == null)
				{
					throw new NullReferenceException();
				}
				ClassMap[value.class_handle] = value2;
				value.class_name = value2;
			}
			if (AssemblyMap.TryGetValue(value.assembly_handle, out var value3))
			{
				value.assembly_name = value3;
			}
			else
			{
				get_image_name(&text, value.assembly_handle);
				if (text == null)
				{
					throw new NullReferenceException();
				}
				value3 = new AssemblyNameEntry
				{
					name = text,
					displayName = text,
					displayNameNonIncrement = text,
					profileType = MonoProfilerConfig.ProfileTypes.Assembly
				};
				AssemblyMap[value.assembly_handle] = value3;
				value.assembly_name = value3;
			}
			records[i] = value;
		}
	}

	private unsafe static void MapCallRecords(List<CallRecord> records)
	{
		Dictionary<string, CallRecord> dictionary = Pool.Get<Dictionary<string, CallRecord>>();
		string text = default(string);
		for (int i = 0; i < records.Count; i++)
		{
			CallRecord value = records[i];
			if (MethodMap.TryGetValue((IntPtr)value.method_handle, out var value2))
			{
				value.method_name = value2;
			}
			else
			{
				get_method_name(&value2, value.method_handle);
				MethodMap[(IntPtr)value.method_handle] = value2 ?? throw new NullReferenceException();
				value.method_name = value2;
			}
			if (AssemblyMap.TryGetValue(value.assembly_handle, out var value3))
			{
				value.assembly_name = value3;
			}
			else
			{
				get_image_name(&text, value.assembly_handle);
				if (text == null)
				{
					throw new NullReferenceException();
				}
				value3 = new AssemblyNameEntry
				{
					name = text,
					displayName = text,
					displayNameNonIncrement = text,
					profileType = MonoProfilerConfig.ProfileTypes.Assembly
				};
				AssemblyMap[value.assembly_handle] = value3;
				value.assembly_name = value3;
			}
			if (dictionary.TryGetValue(value.method_name, out var value4))
			{
				value4.total_time += value.total_time;
				value4.total_time_percentage += value.total_time_percentage;
				value4.own_time += value.own_time;
				value4.own_time_percentage += value.own_time_percentage;
				value4.calls += value.calls;
				value4.total_alloc += value.total_alloc;
				value4.own_alloc += value.own_alloc;
				value4.total_exceptions += value.total_exceptions;
				value4.own_exceptions += value.own_exceptions;
				dictionary[value.method_name] = value4;
			}
			else
			{
				dictionary[value.method_name] = value;
			}
		}
		records.Clear();
		records.AddRange(dictionary.Values);
		Pool.FreeUnmanaged<string, CallRecord>(ref dictionary);
	}

	public static bool TryStartProfileFor(MonoProfilerConfig.ProfileTypes profileType, Assembly assembly, string value, bool incremental = false)
	{
		if (!MonoProfilerConfig.Instance.IsWhitelisted(profileType, value))
		{
			return false;
		}
		return ProfileAssembly(assembly, value, incremental, profileType);
	}

	public static bool ProfileAssembly(Assembly assembly, string assemblyName, bool incremental, MonoProfilerConfig.ProfileTypes profileType)
	{
		if (!Enabled)
		{
			return false;
		}
		string displayName = assemblyName;
		if (incremental)
		{
			displayName = AssemblyBank.Increment(assemblyName);
		}
		ModuleHandle moduleHandle = assembly.ManifestModule.ModuleHandle;
		AssemblyMap[moduleHandle] = new AssemblyNameEntry
		{
			name = assembly.GetName().Name,
			displayName = displayName,
			displayNameNonIncrement = assemblyName,
			profileType = profileType
		};
		register_profiler_assembly(moduleHandle);
		return true;
	}

	[DllImport("CarbonNative")]
	private unsafe static extern void profiler_register_callbacks(ProfilerCallbacks* callbacks);

	[DllImport("CarbonNative")]
	private static extern void register_profiler_assembly(ModuleHandle handle);

	[DllImport("CarbonNative")]
	private static extern bool profiler_is_enabled();

	[DllImport("CarbonNative")]
	private unsafe static extern void carbon_init_logger(delegate*<int, int, byte*, int, LogSource, void> logger);

	[DllImport("CarbonNative")]
	private static extern ulong carbon_get_protocol();

	[DllImport("CarbonNative")]
	private unsafe static extern void get_image_name(string* str, ModuleHandle handle);

	[DllImport("CarbonNative")]
	private unsafe static extern void get_class_name(string* str, IntPtr handle);

	[DllImport("CarbonNative")]
	private unsafe static extern void get_method_name(string* str, MonoMethod* handle);

	[DllImport("CarbonNative")]
	private unsafe static extern ProfilerResultCode profiler_toggle(ProfilerArgs args, bool* state, GCRecord* gc_out, List<AssemblyRecord>* basic_out, List<CallRecord>* advanced_out, List<MemoryRecord>* mem_out);

	public static bool ValidateFile(string file, out int protocol, out double duration, out bool isCompared)
	{
		try
		{
			if (!File.Exists(file))
			{
				protocol = 0;
				duration = 0.0;
				isCompared = false;
				return false;
			}
			using FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
			using GZipStream input = new GZipStream(stream, CompressionMode.Decompress);
			using BinaryReader binaryReader = new BinaryReader(input);
			protocol = binaryReader.ReadInt32();
			duration = binaryReader.ReadDouble();
			isCompared = binaryReader.ReadBoolean();
			return protocol == 127;
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Failed MonoProfiler file validation: " + file + " (" + ex.Message + ")\n" + ex.StackTrace));
		}
		protocol = 0;
		duration = 0.0;
		isCompared = false;
		return false;
	}

	public static byte[] SerializeSample(Sample sample)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (GZipStream output = new GZipStream(memoryStream, CompressionMode.Compress))
		{
			using BinaryWriter binaryWriter = new BinaryWriter(output);
			binaryWriter.Write(127);
			binaryWriter.Write(sample.Duration);
			binaryWriter.Write(sample.IsCompared);
			binaryWriter.Write((int)sample.Comparison.Duration);
			Dictionary<string, int> dictionary = Pool.Get<Dictionary<string, int>>();
			binaryWriter.Write(sample.Assemblies.Count);
			for (int i = 0; i < sample.Assemblies.Count; i++)
			{
				AssemblyRecord assemblyRecord = sample.Assemblies[i];
				binaryWriter.Write(assemblyRecord.total_time);
				binaryWriter.Write(assemblyRecord.total_time_percentage);
				binaryWriter.Write(assemblyRecord.total_exceptions);
				binaryWriter.Write(assemblyRecord.calls);
				binaryWriter.Write(assemblyRecord.alloc);
				binaryWriter.Write(assemblyRecord.assembly_name.name);
				binaryWriter.Write(assemblyRecord.assembly_name.displayName);
				binaryWriter.Write(assemblyRecord.assembly_name.displayNameNonIncrement);
				binaryWriter.Write((int)assemblyRecord.assembly_name.profileType);
				binaryWriter.Write(assemblyRecord.comparison.isCompared);
				binaryWriter.Write((int)assemblyRecord.comparison.total_time);
				binaryWriter.Write((int)assemblyRecord.comparison.total_exceptions);
				binaryWriter.Write((int)assemblyRecord.comparison.calls);
				binaryWriter.Write((int)assemblyRecord.comparison.alloc);
				dictionary.Add(assemblyRecord.assembly_name.name, i);
			}
			binaryWriter.Write(sample.Calls.Count);
			for (int j = 0; j < sample.Calls.Count; j++)
			{
				CallRecord callRecord = sample.Calls[j];
				binaryWriter.Write(callRecord.total_time);
				binaryWriter.Write(callRecord.total_time_percentage);
				binaryWriter.Write(callRecord.own_time);
				binaryWriter.Write(callRecord.own_time_percentage);
				binaryWriter.Write(callRecord.calls);
				binaryWriter.Write(callRecord.total_alloc);
				binaryWriter.Write(callRecord.own_alloc);
				binaryWriter.Write(callRecord.total_exceptions);
				binaryWriter.Write(callRecord.own_exceptions);
				binaryWriter.Write(callRecord.method_name);
				dictionary.TryGetValue(callRecord.assembly_name.name, out var value);
				binaryWriter.Write(value);
				binaryWriter.Write(callRecord.comparison.isCompared);
				binaryWriter.Write((int)callRecord.comparison.total_time);
				binaryWriter.Write((int)callRecord.comparison.own_time);
				binaryWriter.Write((int)callRecord.comparison.calls);
				binaryWriter.Write((int)callRecord.comparison.total_alloc);
				binaryWriter.Write((int)callRecord.comparison.own_alloc);
				binaryWriter.Write((int)callRecord.comparison.total_exceptions);
				binaryWriter.Write((int)callRecord.comparison.own_exceptions);
			}
			binaryWriter.Write(sample.Memory.Count);
			for (int k = 0; k < sample.Memory.Count; k++)
			{
				MemoryRecord memoryRecord = sample.Memory[k];
				binaryWriter.Write(memoryRecord.allocations);
				binaryWriter.Write(memoryRecord.total_alloc_size);
				binaryWriter.Write(memoryRecord.instance_size);
				binaryWriter.Write(memoryRecord.class_token);
				binaryWriter.Write(memoryRecord.class_name);
				dictionary.TryGetValue(memoryRecord.assembly_name.name, out var value2);
				binaryWriter.Write(value2);
				binaryWriter.Write(memoryRecord.comparison.isCompared);
				binaryWriter.Write((int)memoryRecord.comparison.allocations);
				binaryWriter.Write((int)memoryRecord.comparison.total_alloc_size);
			}
			binaryWriter.Write(sample.GC.calls);
			binaryWriter.Write(sample.GC.total_time);
			binaryWriter.Write(sample.GC.comparison.isCompared);
			binaryWriter.Write((int)sample.GC.comparison.calls_c);
			binaryWriter.Write((int)sample.GC.comparison.total_time_c);
			Pool.FreeUnmanaged<string, int>(ref dictionary);
		}
		return memoryStream.ToArray();
	}

	public static Sample DeserializeSample(byte[] buffer)
	{
		using MemoryStream stream = new MemoryStream(buffer);
		using GZipStream input = new GZipStream(stream, CompressionMode.Decompress);
		using BinaryReader binaryReader = new BinaryReader(input);
		Sample result = Sample.Create();
		uint num = binaryReader.ReadUInt32();
		if (num != 127)
		{
			throw new Exception($"Invalid protocol: {num} [expected {127}]");
		}
		result.Duration = binaryReader.ReadDouble();
		result.IsCompared = binaryReader.ReadBoolean();
		result.Comparison.Duration = (Sample.Difference)binaryReader.ReadInt32();
		Dictionary<int, AssemblyNameEntry> dictionary = Pool.Get<Dictionary<int, AssemblyNameEntry>>();
		int num2 = binaryReader.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			AssemblyRecord item = new AssemblyRecord
			{
				total_time = binaryReader.ReadUInt64(),
				total_time_percentage = binaryReader.ReadDouble(),
				total_exceptions = binaryReader.ReadUInt64(),
				calls = binaryReader.ReadUInt64(),
				alloc = binaryReader.ReadUInt64(),
				assembly_name = new AssemblyNameEntry
				{
					name = binaryReader.ReadString(),
					displayName = binaryReader.ReadString(),
					displayNameNonIncrement = binaryReader.ReadString(),
					profileType = (MonoProfilerConfig.ProfileTypes)binaryReader.ReadInt32()
				},
				comparison = 
				{
					isCompared = binaryReader.ReadBoolean(),
					total_time = (Sample.Difference)binaryReader.ReadInt32(),
					total_exceptions = (Sample.Difference)binaryReader.ReadInt32(),
					calls = (Sample.Difference)binaryReader.ReadInt32(),
					alloc = (Sample.Difference)binaryReader.ReadInt32()
				}
			};
			result.Assemblies.Add(item);
			dictionary.Add(i, item.assembly_name);
		}
		int num3 = binaryReader.ReadInt32();
		for (int j = 0; j < num3; j++)
		{
			CallRecord item2 = new CallRecord
			{
				total_time = binaryReader.ReadUInt64(),
				total_time_percentage = binaryReader.ReadDouble(),
				own_time = binaryReader.ReadUInt64(),
				own_time_percentage = binaryReader.ReadDouble(),
				calls = binaryReader.ReadUInt64(),
				total_alloc = binaryReader.ReadUInt64(),
				own_alloc = binaryReader.ReadUInt64(),
				total_exceptions = binaryReader.ReadUInt64(),
				own_exceptions = binaryReader.ReadUInt64(),
				method_name = binaryReader.ReadString()
			};
			if (dictionary.TryGetValue(binaryReader.ReadInt32(), out var value))
			{
				item2.assembly_name = value;
			}
			item2.comparison.isCompared = binaryReader.ReadBoolean();
			item2.comparison.total_time = (Sample.Difference)binaryReader.ReadInt32();
			item2.comparison.own_time = (Sample.Difference)binaryReader.ReadInt32();
			item2.comparison.calls = (Sample.Difference)binaryReader.ReadInt32();
			item2.comparison.total_alloc = (Sample.Difference)binaryReader.ReadInt32();
			item2.comparison.own_alloc = (Sample.Difference)binaryReader.ReadInt32();
			item2.comparison.total_exceptions = (Sample.Difference)binaryReader.ReadInt32();
			item2.comparison.own_exceptions = (Sample.Difference)binaryReader.ReadInt32();
			result.Calls.Add(item2);
		}
		int num4 = binaryReader.ReadInt32();
		for (int k = 0; k < num4; k++)
		{
			MemoryRecord item3 = new MemoryRecord
			{
				allocations = binaryReader.ReadUInt64(),
				total_alloc_size = binaryReader.ReadUInt64(),
				instance_size = binaryReader.ReadUInt32(),
				class_token = binaryReader.ReadUInt32(),
				class_name = binaryReader.ReadString()
			};
			if (dictionary.TryGetValue(binaryReader.ReadInt32(), out var value2))
			{
				item3.assembly_name = value2;
			}
			item3.comparison.isCompared = binaryReader.ReadBoolean();
			item3.comparison.allocations = (Sample.Difference)binaryReader.ReadInt32();
			item3.comparison.total_alloc_size = (Sample.Difference)binaryReader.ReadInt32();
			result.Memory.Add(item3);
		}
		result.GC.calls = binaryReader.ReadUInt64();
		result.GC.total_time = binaryReader.ReadUInt64();
		result.FromDisk = true;
		Pool.FreeUnmanaged<int, AssemblyNameEntry>(ref dictionary);
		return result;
	}
}

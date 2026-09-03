using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Network;

public class NetProfileSession
{
	public const double BucketInterval = 0.1;

	public const int TypeCount = 29;

	public const int RealmCount = 2;

	public const int FieldCount = 5;

	public const int FieldCountIn = 0;

	public const int FieldBytesIn = 1;

	public const int FieldCountOut = 2;

	public const int FieldMsgBytesOut = 3;

	public const int FieldWireBytesOut = 4;

	public const int BucketStride = 290;

	public const int BucketClearMargin = 64;

	public DateTime StartedUtc;

	public long StartTimestamp;

	public double InvFrequency;

	public int MainThreadId;

	public volatile bool Active;

	public double StoppedAt = double.NaN;

	public int RealmFlags;

	public int[] Buckets;

	public int BucketCapacity;

	public long ClearedThroughBucket;

	public long BucketAddsDropped;

	public NetProfileEvent[] Events;

	public long EventWriteIndex;

	public readonly object EventLock = new object();

	public long LastInboundEventIndex = -1L;

	public long EventsSkipped;

	public NetProfileInternTable Info = new NetProfileInternTable();

	public List<NetProfileConnection> Connections = new List<NetProfileConnection>();

	public List<NetProfileTransportSample> TransportSamples = new List<NetProfileTransportSample>();

	public List<double> FrameMarkers = new List<double>();

	public const int MaxFrameMarkers = 65536;

	public long[] Totals;

	public double NowSeconds => (double)(Stopwatch.GetTimestamp() - StartTimestamp) * InvFrequency;

	public double Duration
	{
		get
		{
			if (!double.IsNaN(StoppedAt))
			{
				return StoppedAt;
			}
			return NowSeconds;
		}
	}

	public long FirstValidEventIndex => Math.Max(0L, Volatile.Read(in EventWriteIndex) - Events.Length);

	public long FirstValidBucket => Math.Max(0L, Volatile.Read(in ClearedThroughBucket) - BucketCapacity + 1);

	public NetProfileSession(int bucketCapacity, int eventCapacity)
	{
		BucketCapacity = bucketCapacity;
		Buckets = new int[bucketCapacity * 290];
		Events = new NetProfileEvent[eventCapacity];
		Totals = new long[290];
		StartedUtc = DateTime.UtcNow;
		StartTimestamp = Stopwatch.GetTimestamp();
		InvFrequency = 1.0 / (double)Stopwatch.Frequency;
		MainThreadId = Environment.CurrentManagedThreadId;
		ClearedThroughBucket = Math.Min(64, bucketCapacity - 1);
	}

	public static int BucketOffset(long bucketIndex, int bucketCapacity, bool serverRealm, int type, int field)
	{
		return (int)(bucketIndex % bucketCapacity) * 290 + CellOffset(serverRealm, type, field);
	}

	public static int CellOffset(bool serverRealm, int type, int field)
	{
		return (serverRealm ? 1 : 0) * 29 * 5 + type * 5 + field;
	}

	public int CopyEvents(long fromIndex, NetProfileEvent[] dest, int destOffset = 0)
	{
		lock (EventLock)
		{
			long eventWriteIndex = EventWriteIndex;
			long num = Math.Max(fromIndex, Math.Max(0L, eventWriteIndex - Events.Length));
			int num2 = (int)Math.Min(eventWriteIndex - num, dest.Length - destOffset);
			for (int i = 0; i < num2; i++)
			{
				dest[destOffset + i] = Events[(num + i) % Events.Length];
			}
			return num2;
		}
	}
}

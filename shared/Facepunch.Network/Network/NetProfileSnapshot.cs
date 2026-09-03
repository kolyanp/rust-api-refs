using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace Network;

public class NetProfileSnapshot
{
	public const uint Magic = 1380864070u;

	public const int CurrentVersion = 1;

	public int Version = 1;

	public byte RealmFlags;

	public DateTime StartedUtc;

	public double Duration;

	public double BucketInterval;

	public double FirstBucketTime;

	public int TypeCount;

	public int FieldCount;

	public int RealmCount;

	public int BucketCount;

	public int BucketStride;

	public int[] Buckets = Array.Empty<int>();

	public NetProfileEvent[] Events = Array.Empty<NetProfileEvent>();

	public long EventsDroppedByRing;

	public long EventsSkipped;

	public long BucketAddsDropped;

	public long[] Totals = Array.Empty<long>();

	public Dictionary<uint, string> Names = new Dictionary<uint, string>();

	public string[] InfoStrings = Array.Empty<string>();

	public NetProfileConnection[] Connections = Array.Empty<NetProfileConnection>();

	public NetProfileTransportSample[] TransportSamples = Array.Empty<NetProfileTransportSample>();

	public double[] FrameMarkers = Array.Empty<double>();

	public static string DefaultDirectory => Path.Combine(Path.GetDirectoryName(Application.dataPath), "networkprofiles");

	public static string DefaultPath()
	{
		string text = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
		return Path.Combine(DefaultDirectory, "networkprofile_" + text + ".rnp");
	}

	public static NetProfileSnapshot FromSession(NetProfileSession session, Func<uint, string> nameResolver)
	{
		if (session == null)
		{
			return null;
		}
		NetProfileSnapshot netProfileSnapshot = new NetProfileSnapshot
		{
			RealmFlags = (byte)session.RealmFlags,
			StartedUtc = session.StartedUtc,
			Duration = session.Duration,
			BucketInterval = 0.1,
			TypeCount = 29,
			FieldCount = 5,
			RealmCount = 2,
			BucketStride = 290,
			EventsSkipped = session.EventsSkipped,
			BucketAddsDropped = session.BucketAddsDropped,
			InfoStrings = session.Info.ToArray(),
			Connections = session.Connections.ToArray(),
			TransportSamples = session.TransportSamples.ToArray(),
			FrameMarkers = session.FrameMarkers.ToArray()
		};
		long num = (long)(session.Duration / 0.1);
		long num2 = Math.Max(0L, Math.Min(session.FirstValidBucket, num));
		int num3 = (int)(num - num2 + 1);
		netProfileSnapshot.FirstBucketTime = (double)num2 * 0.1;
		netProfileSnapshot.BucketCount = num3;
		netProfileSnapshot.Buckets = new int[num3 * 290];
		for (int i = 0; i < num3; i++)
		{
			int sourceIndex = (int)((num2 + i) % session.BucketCapacity) * 290;
			Array.Copy(session.Buckets, sourceIndex, netProfileSnapshot.Buckets, i * 290, 290);
		}
		netProfileSnapshot.Totals = new long[session.Totals.Length];
		Array.Copy(session.Totals, netProfileSnapshot.Totals, session.Totals.Length);
		long num4 = (netProfileSnapshot.EventsDroppedByRing = session.FirstValidEventIndex);
		NetProfileEvent[] array = new NetProfileEvent[(int)Math.Min(session.EventWriteIndex - num4, session.Events.Length)];
		int num5 = session.CopyEvents(num4, array);
		if (num5 != array.Length)
		{
			Array.Resize(ref array, num5);
		}
		netProfileSnapshot.Events = array;
		if (nameResolver != null)
		{
			for (int j = 0; j < array.Length; j++)
			{
				ref NetProfileEvent reference = ref array[j];
				ResolveName(netProfileSnapshot.Names, reference.PrefabId, nameResolver);
				ResolveName(netProfileSnapshot.Names, reference.RpcId, nameResolver);
				if ((reference.Flags & NetProfileEventFlags.AuxIsStringId) != NetProfileEventFlags.None)
				{
					ResolveName(netProfileSnapshot.Names, reference.Aux, nameResolver);
				}
			}
		}
		return netProfileSnapshot;
	}

	private static void ResolveName(Dictionary<uint, string> names, uint id, Func<uint, string> nameResolver)
	{
		if (id != 0 && !names.ContainsKey(id))
		{
			string value;
			try
			{
				value = nameResolver(id);
			}
			catch (Exception)
			{
				value = null;
			}
			if (!string.IsNullOrEmpty(value))
			{
				names.Add(id, value);
			}
		}
	}

	public void Save(string path)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		using FileStream stream = File.Create(path);
		using DeflateStream output = new DeflateStream(stream, CompressionLevel.Fastest);
		using BinaryWriter binaryWriter = new BinaryWriter(output);
		binaryWriter.Write(1380864070u);
		binaryWriter.Write(1);
		binaryWriter.Write(RealmFlags);
		binaryWriter.Write(StartedUtc.Ticks);
		binaryWriter.Write(Duration);
		binaryWriter.Write(BucketInterval);
		binaryWriter.Write(FirstBucketTime);
		binaryWriter.Write(TypeCount);
		binaryWriter.Write(FieldCount);
		binaryWriter.Write(RealmCount);
		binaryWriter.Write(BucketCount);
		binaryWriter.Write(BucketStride);
		binaryWriter.Write(EventsDroppedByRing);
		binaryWriter.Write(EventsSkipped);
		binaryWriter.Write(BucketAddsDropped);
		binaryWriter.Write(Buckets.Length);
		for (int i = 0; i < Buckets.Length; i++)
		{
			binaryWriter.Write(Buckets[i]);
		}
		binaryWriter.Write(Events.Length);
		for (int j = 0; j < Events.Length; j++)
		{
			ref NetProfileEvent reference = ref Events[j];
			binaryWriter.Write(reference.Time);
			binaryWriter.Write(reference.EntityId);
			binaryWriter.Write(reference.ConnectionId);
			binaryWriter.Write(reference.PrefabId);
			binaryWriter.Write(reference.RpcId);
			binaryWriter.Write(reference.Aux);
			binaryWriter.Write(reference.Bytes);
			binaryWriter.Write(reference.Fanout);
			binaryWriter.Write(reference.Type);
			binaryWriter.Write((byte)reference.Flags);
		}
		binaryWriter.Write(Totals.Length);
		for (int k = 0; k < Totals.Length; k++)
		{
			binaryWriter.Write(Totals[k]);
		}
		binaryWriter.Write(Names.Count);
		foreach (KeyValuePair<uint, string> name in Names)
		{
			binaryWriter.Write(name.Key);
			binaryWriter.Write(name.Value);
		}
		binaryWriter.Write(InfoStrings.Length);
		for (int l = 0; l < InfoStrings.Length; l++)
		{
			binaryWriter.Write(InfoStrings[l] ?? string.Empty);
		}
		binaryWriter.Write(Connections.Length);
		for (int m = 0; m < Connections.Length; m++)
		{
			binaryWriter.Write(Connections[m].UserId);
			binaryWriter.Write(Connections[m].Guid);
			binaryWriter.Write(Connections[m].Username ?? string.Empty);
		}
		binaryWriter.Write(TransportSamples.Length);
		for (int n = 0; n < TransportSamples.Length; n++)
		{
			binaryWriter.Write(TransportSamples[n].Time);
			binaryWriter.Write(TransportSamples[n].Realm);
			binaryWriter.Write(TransportSamples[n].BytesIn);
			binaryWriter.Write(TransportSamples[n].BytesOut);
		}
		binaryWriter.Write(FrameMarkers.Length);
		for (int num = 0; num < FrameMarkers.Length; num++)
		{
			binaryWriter.Write(FrameMarkers[num]);
		}
	}

	public static NetProfileSnapshot Load(string path)
	{
		try
		{
			using FileStream stream = File.OpenRead(path);
			using DeflateStream input = new DeflateStream(stream, CompressionMode.Decompress);
			using BinaryReader binaryReader = new BinaryReader(input);
			if (binaryReader.ReadUInt32() != 1380864070)
			{
				Debug.LogError((object)("[NetProfileSnapshot] Not a network profile: " + path));
				return null;
			}
			int num = binaryReader.ReadInt32();
			if (num != 1)
			{
				Debug.LogError((object)$"[NetProfileSnapshot] Unsupported version {num} (expected {1}): {path}");
				return null;
			}
			NetProfileSnapshot netProfileSnapshot = new NetProfileSnapshot
			{
				Version = num,
				RealmFlags = binaryReader.ReadByte(),
				StartedUtc = new DateTime(binaryReader.ReadInt64(), DateTimeKind.Utc),
				Duration = binaryReader.ReadDouble(),
				BucketInterval = binaryReader.ReadDouble(),
				FirstBucketTime = binaryReader.ReadDouble(),
				TypeCount = binaryReader.ReadInt32(),
				FieldCount = binaryReader.ReadInt32(),
				RealmCount = binaryReader.ReadInt32(),
				BucketCount = binaryReader.ReadInt32(),
				BucketStride = binaryReader.ReadInt32(),
				EventsDroppedByRing = binaryReader.ReadInt64(),
				EventsSkipped = binaryReader.ReadInt64(),
				BucketAddsDropped = binaryReader.ReadInt64()
			};
			netProfileSnapshot.Buckets = new int[binaryReader.ReadInt32()];
			for (int i = 0; i < netProfileSnapshot.Buckets.Length; i++)
			{
				netProfileSnapshot.Buckets[i] = binaryReader.ReadInt32();
			}
			netProfileSnapshot.Events = new NetProfileEvent[binaryReader.ReadInt32()];
			for (int j = 0; j < netProfileSnapshot.Events.Length; j++)
			{
				ref NetProfileEvent reference = ref netProfileSnapshot.Events[j];
				reference.Time = binaryReader.ReadDouble();
				reference.EntityId = binaryReader.ReadUInt64();
				reference.ConnectionId = binaryReader.ReadUInt64();
				reference.PrefabId = binaryReader.ReadUInt32();
				reference.RpcId = binaryReader.ReadUInt32();
				reference.Aux = binaryReader.ReadUInt32();
				reference.Bytes = binaryReader.ReadInt32();
				reference.Fanout = binaryReader.ReadUInt16();
				reference.Type = binaryReader.ReadByte();
				reference.Flags = (NetProfileEventFlags)binaryReader.ReadByte();
			}
			netProfileSnapshot.Totals = new long[binaryReader.ReadInt32()];
			for (int k = 0; k < netProfileSnapshot.Totals.Length; k++)
			{
				netProfileSnapshot.Totals[k] = binaryReader.ReadInt64();
			}
			int num2 = binaryReader.ReadInt32();
			for (int l = 0; l < num2; l++)
			{
				uint key = binaryReader.ReadUInt32();
				netProfileSnapshot.Names[key] = binaryReader.ReadString();
			}
			netProfileSnapshot.InfoStrings = new string[binaryReader.ReadInt32()];
			for (int m = 0; m < netProfileSnapshot.InfoStrings.Length; m++)
			{
				netProfileSnapshot.InfoStrings[m] = binaryReader.ReadString();
			}
			netProfileSnapshot.Connections = new NetProfileConnection[binaryReader.ReadInt32()];
			for (int n = 0; n < netProfileSnapshot.Connections.Length; n++)
			{
				netProfileSnapshot.Connections[n].UserId = binaryReader.ReadUInt64();
				netProfileSnapshot.Connections[n].Guid = binaryReader.ReadUInt64();
				netProfileSnapshot.Connections[n].Username = binaryReader.ReadString();
			}
			netProfileSnapshot.TransportSamples = new NetProfileTransportSample[binaryReader.ReadInt32()];
			for (int num3 = 0; num3 < netProfileSnapshot.TransportSamples.Length; num3++)
			{
				netProfileSnapshot.TransportSamples[num3].Time = binaryReader.ReadDouble();
				netProfileSnapshot.TransportSamples[num3].Realm = binaryReader.ReadByte();
				netProfileSnapshot.TransportSamples[num3].BytesIn = binaryReader.ReadInt64();
				netProfileSnapshot.TransportSamples[num3].BytesOut = binaryReader.ReadInt64();
			}
			netProfileSnapshot.FrameMarkers = new double[binaryReader.ReadInt32()];
			for (int num4 = 0; num4 < netProfileSnapshot.FrameMarkers.Length; num4++)
			{
				netProfileSnapshot.FrameMarkers[num4] = binaryReader.ReadDouble();
			}
			return netProfileSnapshot;
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("[NetProfileSnapshot] Failed to load '" + path + "': " + ex.Message));
			return null;
		}
	}
}

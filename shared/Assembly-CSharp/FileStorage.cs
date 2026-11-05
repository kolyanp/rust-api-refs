using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.Sqlite;
using Ionic.Crc;
using ProtoBuf;
using UnityEngine.Assertions;

public class FileStorage : IDisposable
{
	private readonly struct Slot(NetworkableId entityId, ulong part) : IEquatable<Slot>
	{
		public readonly NetworkableId EntityId = entityId;

		public readonly ulong Part = part;

		public bool Equals(Slot other)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			if (EntityId == other.EntityId)
			{
				return Part == other.Part;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Slot other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return HashCode.Combine<NetworkableId, ulong>(EntityId, Part);
		}
	}

	private class CacheData
	{
		public uint crc;

		public byte[] data;

		public float? storedLocallyAt;

		public uint replacedCrc;
	}

	public enum Type
	{
		png,
		jpg,
		ogg,
		sculpt
	}

	private class FileDatabase : Database
	{
		public (uint crc, byte[] data)? QueryFile(NetworkableId entityId, ulong part, uint? crc)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			IntPtr stmHandle = Prepare(crc.HasValue ? "SELECT crc, data FROM files WHERE entid = ? AND part = ? AND crc = ?" : "SELECT crc, data FROM files WHERE entid = ? AND part = ?");
			Database.Bind(stmHandle, 1, entityId.Value);
			Database.Bind(stmHandle, 2, part);
			if (crc.HasValue)
			{
				Database.Bind(stmHandle, 3, crc.Value);
			}
			return ExecuteAndReadQueryResult<(uint, byte[])>(stmHandle, (Func<IntPtr, (uint, byte[])>)((IntPtr stmHandle2) => (Database.GetColumnValue<uint>(stmHandle2, 0), Database.GetColumnValue<byte[]>(stmHandle2, 1))));
		}

		public IEnumerable<(ulong part, uint crc)> QueryCrcs(NetworkableId entityId)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			IntPtr stmHandle = Prepare("SELECT part, crc FROM files WHERE entid = ?");
			Database.Bind(stmHandle, 1, entityId.Value);
			return ExecuteAndReadQueryResults(stmHandle, (IntPtr stmHandle2) => (Database.GetColumnValue<ulong>(stmHandle2, 0), Database.GetColumnValue<uint>(stmHandle2, 1)));
		}

		public IEnumerable<AssociatedFile> QueryAll(NetworkableId entityId)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			IntPtr stmHandle = Prepare("SELECT filetype, part, data FROM files WHERE entid = ?");
			Database.Bind(stmHandle, 1, entityId.Value);
			return ExecuteAndReadQueryResults(stmHandle, ReadAssociatedFileRow);
		}

		private static AssociatedFile ReadAssociatedFileRow(IntPtr stmHandle)
		{
			AssociatedFile val = Pool.Get<AssociatedFile>();
			val.type = Database.GetColumnValue<int>(stmHandle, 0);
			val.part = Database.GetColumnValue<ulong>(stmHandle, 1);
			val.data = Database.GetColumnValue<byte[]>(stmHandle, 2);
			return val;
		}
	}

	private FileDatabase db;

	private static readonly CRC32 crc32 = new CRC32();

	private readonly MruDictionary<Slot, CacheData> _cache = new MruDictionary<Slot, CacheData>(1000, (Action<Slot, CacheData>)null);

	public static FileStorage server = new FileStorage("sv.files." + 290, server: true);

	protected FileStorage(string name, bool server)
	{
		if (server)
		{
			db = new FileDatabase();
			db.Open(Server.filesStorageFolder + "/" + name + ".db");
			if (!db.TableExists("files"))
			{
				db.Execute("CREATE TABLE files ( entid INTEGER, part INTEGER, crc INTEGER, filetype INTEGER, data BLOB, updated INTEGER, PRIMARY KEY ( entid, part ) )");
			}
			db.Execute("CREATE INDEX IF NOT EXISTS crcindex ON files ( crc )");
		}
	}

	~FileStorage()
	{
		Dispose();
	}

	public void Dispose()
	{
		if (db != null)
		{
			db.Close();
			db = null;
		}
	}

	public static uint ComputeCrc(byte[] data)
	{
		using (TimeWarning.New("FileStorage.ComputeCrc"))
		{
			crc32.Reset();
			crc32.SlurpBlock(data, 0, data.Length);
			return (uint)crc32.Crc32Result;
		}
	}

	public uint Store(NetworkableId entityId, ulong part, Type type, byte[] data)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		uint num = ComputeCrc(data);
		Store(new Slot(entityId, part), num, type, data);
		return num;
	}

	public uint StoreByCrc(NetworkableId entityId, Type type, byte[] data)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		uint num = ComputeCrc(data);
		Store(new Slot(entityId, num), num, type, data);
		return num;
	}

	private void Store(Slot slot, uint crc, Type type, byte[] data)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("FileStorage.Store"))
		{
			db.Execute("INSERT OR REPLACE INTO files ( crc, data, entid, filetype, part ) VALUES ( ?, ?, ?, ?, ? )", crc, data, slot.EntityId.Value, (int)type, slot.Part);
			AddToCache(slot, crc, data);
		}
	}

	private void AddToCache(Slot slot, uint crc, byte[] data, float? storedLocallyAt = null, uint replacedCrc = 0u)
	{
		_cache.Remove(slot);
		_cache.Add(slot, new CacheData
		{
			crc = crc,
			data = data,
			storedLocallyAt = storedLocallyAt,
			replacedCrc = replacedCrc
		});
	}

	public byte[] Get(NetworkableId entityId, ulong part, uint crc)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (crc == 0)
		{
			return null;
		}
		return Find(new Slot(entityId, part), crc)?.data;
	}

	public byte[] Get(NetworkableId entityId, ulong part = 0uL)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Find(new Slot(entityId, part), null)?.data;
	}

	private CacheData Find(Slot slot, uint? crc)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("FileStorage.Get"))
		{
			CacheData cacheData = default;
			if (_cache.TryGetValue(slot, ref cacheData))
			{
				Assert.IsTrue(cacheData.data != null, "FileStorage cache contains a null file");
				return (!crc.HasValue || cacheData.crc == crc) ? cacheData : null;
			}
			(uint, byte[])? tuple = db?.QueryFile(slot.EntityId, slot.Part, crc);
			if (!tuple.HasValue)
			{
				return null;
			}
			CacheData cacheData2 = new CacheData
			{
				crc = tuple.Value.Item1,
				data = tuple.Value.Item2
			};
			_cache.Add(slot, cacheData2);
			return cacheData2;
		}
	}

	public void Remove(NetworkableId entityId, ulong part = 0uL)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("FileStorage.Remove"))
		{
			db?.Execute("DELETE FROM files WHERE entid = ? AND part = ?", entityId.Value, part);
			_cache.Remove(new Slot(entityId, part));
		}
	}

	public void RemoveParts(NetworkableId entityId, ulong firstPart, ulong partCount)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("FileStorage.RemoveParts"))
		{
			db?.Execute("DELETE FROM files WHERE entid = ? AND part >= ? AND part < ?", entityId.Value, firstPart, firstPart + partCount);
			for (ulong num = firstPart; num < firstPart + partCount; num++)
			{
				_cache.Remove(new Slot(entityId, num));
			}
		}
	}

	public void RemoveAll(NetworkableId entityId)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("FileStorage.RemoveAll"))
		{
			db?.Execute("DELETE FROM files WHERE entid = ?", entityId.Value);
			RemoveFromCache(entityId);
		}
	}

	private void RemoveFromCache(NetworkableId entityId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Slot[] array = (from x in ((IEnumerable<KeyValuePair<Slot, CacheData>>)_cache).Where((KeyValuePair<Slot, CacheData> x) =>
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				return x.Key.EntityId == entityId;
			})
			select x.Key).ToArray();
		foreach (Slot slot in array)
		{
			_cache.Remove(slot);
		}
	}

	public uint GetCrc(NetworkableId entityId, ulong part = 0uL)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("FileStorage.GetCrc"))
		{
			CacheData cacheData = default;
			if (_cache.TryGetValue(new Slot(entityId, part), ref cacheData))
			{
				return cacheData.crc;
			}
			return db.Query<uint, ulong, ulong>("SELECT crc FROM files WHERE entid = ? AND part = ?", entityId.Value, part);
		}
	}

	public void GetCrcs(NetworkableId entityId, Dictionary<ulong, uint> crcsByPart)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("FileStorage.GetCrcs"))
		{
			foreach (var item3 in db.QueryCrcs(entityId))
			{
				ulong item = item3.part;
				uint item2 = item3.crc;
				crcsByPart[item] = item2;
			}
		}
	}

	public void Reassign(NetworkableId fromEntityId, NetworkableId toEntityId)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("FileStorage.Reassign"))
		{
			db.Execute("UPDATE OR REPLACE files SET entid = ? WHERE entid = ?", toEntityId.Value, fromEntityId.Value);
			RemoveFromCache(fromEntityId);
			RemoveFromCache(toEntityId);
		}
	}

	public byte[] GetByCrc(uint crc, Type type)
	{
		using (TimeWarning.New("FileStorage.GetByCrc"))
		{
			return db.Query<byte[], uint, int>("SELECT data FROM files WHERE crc = ? AND filetype = ? LIMIT 1", crc, (int)type);
		}
	}

	public bool HasFiles(NetworkableId entityId)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return db.Query<int, ulong>("SELECT EXISTS ( SELECT 1 FROM files WHERE entid = ? )", entityId.Value) != 0;
	}

	public IEnumerable<AssociatedFile> QueryAllByEntity(NetworkableId entityId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return db.QueryAll(entityId);
	}

	static FileStorage()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected Obj, but got Unknown
	}
}

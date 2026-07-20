using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Carbon.Extensions;
using Facepunch;
using ProtoBuf;
using UnityEngine;

namespace Carbon.Components;

public sealed class StoredModifiers
{
	[ProtoContract(/*Could not decode attribute arguments.*/)]
	public class Data
	{
	}

	private static class TypeId<T> where T : Data
	{
		public static readonly uint Value = Vault.Pool.Get(typeof(T).Name);
	}

	public static Dictionary<ulong, Dictionary<uint, Data>> Entities = new Dictionary<ulong, Dictionary<uint, Data>>();

	public static string GetSavePath()
	{
		return World.SaveFolderName + "/" + Path.GetFileNameWithoutExtension(World.SaveFileName) + ".carbon.sav";
	}

	public static bool HasLocalSave()
	{
		return File.Exists(GetSavePath());
	}

	public static void TryUpdateData<T>(BaseNetworkable entity, Data data, SaveInfo info) where T : Data
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!info.forDisk || !BaseNetworkableEx.IsValid(entity) || Entities == null)
		{
			return;
		}
		ulong value = entity.net.ID.Value;
		uint value2 = TypeId<T>.Value;
		Dictionary<uint, Data> value3;
		if (data == null)
		{
			if (Entities.TryGetValue(value, out value3))
			{
				value3.Remove(value2);
				if (value3.Count == 0)
				{
					Entities.Remove(value);
				}
			}
		}
		else
		{
			if (!Entities.TryGetValue(value, out value3))
			{
				value3 = (Entities[value] = new Dictionary<uint, Data>());
			}
			value3[value2] = data;
		}
	}

	public static void TryGetData<T>(BaseNetworkable entity, ref T data, LoadInfo info) where T : Data
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (info.fromDisk && BaseNetworkableEx.IsValid(entity) && Entities != null)
		{
			ulong value = entity.net.ID.Value;
			uint value2 = TypeId<T>.Value;
			if (Entities.TryGetValue(value, out var value3) && value3.TryGetValue(value2, out var value4))
			{
				data = value4 as T;
			}
		}
	}

	public static void Init()
	{
		Type typeFromHandle = typeof(Data);
		foreach (Assembly item in AccessToolsEx.AllAssemblies())
		{
			try
			{
				Type[] exportedTypes = item.GetExportedTypes();
				foreach (Type type in exportedTypes)
				{
					try
					{
						if (type != typeFromHandle && typeFromHandle.IsAssignableFrom(type))
						{
							type.GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Invoke(null, null);
						}
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
		}
	}

	public static void Load()
	{
		if (!HasLocalSave())
		{
			return;
		}
		string savePath = GetSavePath();
		using FileStream fileStream = File.OpenRead(savePath);
		using (TimeMeasure.New("StoredModifiers.Load", 100, "Carbon modifier entity data"))
		{
			Entities = Serializer.Deserialize<Dictionary<ulong, Dictionary<uint, Data>>>((Stream)fileStream);
			Logger.Log(string.Format("Processed {0} {1} with Carbon modifier data", Entities.Count, Entities.Count.Plural("entity", "entities")));
		}
	}

	public static void Save()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (Entities.Count == 0 && !HasLocalSave())
		{
			return;
		}
		string savePath = GetSavePath();
		using (TimeMeasure.New("StoredModifiers.Save", 100, $"saved {Entities.Count:n0} entities"))
		{
			PooledList<ulong> val = Pool.Get<PooledList<ulong>>();
			try
			{
				foreach (KeyValuePair<ulong, Dictionary<uint, Data>> entity in Entities)
				{
					if (!Object.op_Implicit((Object)(object)BaseNetworkable.serverEntities.Find(new NetworkableId(entity.Key))))
					{
						((List<ulong>)(object)val).Add(entity.Key);
					}
				}
				for (int i = 0; i < ((List<ulong>)(object)val).Count; i++)
				{
					Entities.Remove(((List<ulong>)(object)val)[i]);
				}
				using FileStream fileStream = File.Create(savePath);
				Serializer.Serialize<Dictionary<ulong, Dictionary<uint, Data>>>((Stream)fileStream, Entities);
				Logger.Log(string.Format("Saved {0:n0} {1} with Carbon modifier data", Entities.Count, Entities.Count.Plural("ent", "ents")));
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}
}

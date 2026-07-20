using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Facepunch.Rust;

public class EventRecord : IPooled
{
	public static readonly long NSPerTick = 100L;

	public DateTime Timestamp;

	[NonSerialized]
	public bool IsServer;

	public List<EventRecordField> Data = new List<EventRecordField>();

	public int TimesCreated;

	public int TimesSubmitted;

	public static readonly Guid SessionId = Guid.NewGuid();

	public string EventType { get; private set; }

	public EventRecord AddField(byte value)
	{
		return AddField((long)value);
	}

	public EventRecord AddField(short value)
	{
		return AddField((long)value);
	}

	public EventRecord AddField(ushort value)
	{
		return AddField((long)value);
	}

	public EventRecord AddField(int value)
	{
		return AddField((long)value);
	}

	public EventRecord AddField(uint value)
	{
		return AddField((long)value);
	}

	public EventRecord AddField(ulong value)
	{
		return AddField((long)value);
	}

	public EventRecord AddField(float value)
	{
		return AddField((double)value);
	}

	[Obsolete("Char not supported, either cast to int or string")]
	public EventRecord AddField(char value)
	{
		throw new NotImplementedException();
	}

	public EventRecord AddField(long value)
	{
		Data.Add(new EventRecordField
		{
			Value = new EventRecordField.ValueUnion
			{
				Number = value
			},
			Type = FieldType.Number
		});
		return this;
	}

	public EventRecord AddField(double value)
	{
		Data.Add(new EventRecordField
		{
			Value = new EventRecordField.ValueUnion
			{
				Float = value
			},
			Type = FieldType.Float
		});
		return this;
	}

	public EventRecord AddField(string value)
	{
		Data.Add(new EventRecordField
		{
			String = value,
			Type = FieldType.String
		});
		return this;
	}

	public EventRecord AddField(bool value)
	{
		Data.Add(new EventRecordField
		{
			String = (value ? "true" : "false"),
			Type = FieldType.String
		});
		return this;
	}

	public EventRecord AddField(DateTime value)
	{
		Data.Add(new EventRecordField
		{
			Value = new EventRecordField.ValueUnion
			{
				DateTime = value
			},
			Type = FieldType.DateTime
		});
		return this;
	}

	public EventRecord AddField(TimeSpan value)
	{
		Data.Add(new EventRecordField
		{
			Value = new EventRecordField.ValueUnion
			{
				Number = value.Ticks * NSPerTick
			},
			Type = FieldType.Number
		});
		return this;
	}

	public EventRecord AddField(Guid value)
	{
		Data.Add(new EventRecordField
		{
			Value = new EventRecordField.ValueUnion
			{
				Guid = value
			},
			Type = FieldType.Guid
		});
		return this;
	}

	public EventRecord AddField(Vector3 vector)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Data.Add(new EventRecordField
		{
			Value = new EventRecordField.ValueUnion
			{
				Vector = vector
			},
			Type = FieldType.Vector
		});
		return this;
	}

	public EventRecord AddField(byte[] bytes)
	{
		return AddField(null, null, bytes);
	}

	public EventRecord AddField(MemoryStream bytes)
	{
		return AddField(null, null, bytes);
	}

	public EventRecord AddField(string key1, string key2, byte value)
	{
		return AddField(key1, key2, (long)value);
	}

	public EventRecord AddField(string key1, string key2, short value)
	{
		return AddField(key1, key2, (long)value);
	}

	public EventRecord AddField(string key1, string key2, ushort value)
	{
		return AddField(key1, key2, (long)value);
	}

	public EventRecord AddField(string key1, string key2, int value)
	{
		return AddField(key1, key2, (long)value);
	}

	public EventRecord AddField(string key1, string key2, uint value)
	{
		return AddField(key1, key2, (long)value);
	}

	public EventRecord AddField(string key1, string key2, ulong value)
	{
		return AddField(key1, key2, (long)value);
	}

	public EventRecord AddField(string key1, string key2, float value)
	{
		return AddField(key1, key2, (double)value);
	}

	[Obsolete("Char not supported, either cast to int or string")]
	public EventRecord AddField(string key1, string key2, char value)
	{
		throw new NotImplementedException();
	}

	public EventRecord AddField(string key1, string key2, long value)
	{
		Data.Add(new EventRecordField(key1, key2)
		{
			Value = new EventRecordField.ValueUnion
			{
				Number = value
			},
			Type = FieldType.Number
		});
		return this;
	}

	public EventRecord AddField(string key1, string key2, double value)
	{
		Data.Add(new EventRecordField(key1, key2)
		{
			Value = new EventRecordField.ValueUnion
			{
				Float = value
			},
			Type = FieldType.Float
		});
		return this;
	}

	public EventRecord AddField(string key1, string key2, string value)
	{
		Data.Add(new EventRecordField(key1, key2)
		{
			String = value,
			Type = FieldType.String
		});
		return this;
	}

	public EventRecord AddField(string key1, string key2, bool value)
	{
		Data.Add(new EventRecordField(key1, key2)
		{
			String = (value ? "true" : "false"),
			Type = FieldType.String
		});
		return this;
	}

	public EventRecord AddField(string key1, string key2, DateTime value)
	{
		Data.Add(new EventRecordField(key1, key2)
		{
			Value = new EventRecordField.ValueUnion
			{
				DateTime = value
			},
			Type = FieldType.DateTime
		});
		return this;
	}

	public EventRecord AddField(string key1, string key2, TimeSpan value)
	{
		Data.Add(new EventRecordField(key1, key2)
		{
			Value = new EventRecordField.ValueUnion
			{
				Number = value.Ticks * NSPerTick
			},
			Type = FieldType.Number
		});
		return this;
	}

	public EventRecord AddField(string key1, string key2, Guid value)
	{
		Data.Add(new EventRecordField(key1, key2)
		{
			Value = new EventRecordField.ValueUnion
			{
				Guid = value
			},
			Type = FieldType.Guid
		});
		return this;
	}

	public EventRecord AddField(string key1, string key2, Vector3 vector)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Data.Add(new EventRecordField(key1, key2)
		{
			Value = new EventRecordField.ValueUnion
			{
				Vector = vector
			},
			Type = FieldType.Vector
		});
		return this;
	}

	public EventRecord AddField(string key, byte[] bytes)
	{
		return AddField(key, null, bytes);
	}

	public EventRecord AddField(string key, MemoryStream bytes)
	{
		return AddField(key, null, bytes);
	}

	public void EnterPool()
	{
		Timestamp = default(DateTime);
		EventType = null;
		IsServer = false;
		foreach (EventRecordField datum in Data)
		{
			MemoryStream bytes = datum.Bytes;
			if (bytes != null)
			{
				Pool.FreeUnmanaged(ref bytes);
			}
		}
		Data.Clear();
	}

	public void LeavePool()
	{
	}

	public static EventRecord CSV()
	{
		EventRecord eventRecord = Pool.Get<EventRecord>();
		eventRecord.IsServer = true;
		eventRecord.TimesCreated++;
		return eventRecord;
	}

	public static EventRecord New(string type, bool isServer = true)
	{
		EventRecord eventRecord = Pool.Get<EventRecord>();
		eventRecord.EventType = type;
		eventRecord.AddField("type", type);
		eventRecord.AddField("guid", Guid.NewGuid());
		BuildInfo current = BuildInfo.Current;
		bool num = (current.Scm.Branch != null && current.Scm.Branch == "experimental/release") || current.Scm.Branch == "release";
		bool isEditor = Application.isEditor;
		string value = ((num && !isEditor) ? "release" : (isEditor ? "editor" : "staging"));
		eventRecord.AddField("environment", value);
		eventRecord.IsServer = isServer;
		if (isServer && SaveRestore.WipeId != null)
		{
			eventRecord.AddField("wipe_id", SaveRestore.WipeId);
		}
		eventRecord.AddField("frame_count", ServerMgr.FrameCount);
		eventRecord.Timestamp = DateTime.UtcNow;
		eventRecord.TimesCreated++;
		return eventRecord;
	}

	public EventRecord AddObject(string key, object data)
	{
		if (data == null)
		{
			return this;
		}
		Data.Add(new EventRecordField(key)
		{
			String = JsonConvert.SerializeObject(data),
			IsObject = true,
			Type = FieldType.String
		});
		return this;
	}

	public EventRecord SetTimestamp(DateTime timestamp)
	{
		Timestamp = timestamp;
		return this;
	}

	public EventRecord AddField(string key, DateTime time)
	{
		Data.Add(new EventRecordField(key)
		{
			Value = new EventRecordField.ValueUnion
			{
				DateTime = time
			},
			Type = FieldType.DateTime
		});
		return this;
	}

	public EventRecord AddField(string key, bool value)
	{
		Data.Add(new EventRecordField(key)
		{
			String = (value ? "true" : "false"),
			Type = FieldType.String
		});
		return this;
	}

	public EventRecord AddField(string key, string value)
	{
		Data.Add(new EventRecordField(key)
		{
			String = value,
			Type = FieldType.String
		});
		return this;
	}

	public EventRecord AddField(string key, StringView value)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Data.Add(new EventRecordField(key)
		{
			Chars = StringView.op_Implicit(value),
			Type = FieldType.Chars
		});
		return this;
	}

	public EventRecord AddField(string key, byte value)
	{
		return AddField(key, (int)value);
	}

	public EventRecord AddField(string key, sbyte value)
	{
		return AddField(key, (int)value);
	}

	public EventRecord AddField(string key, short value)
	{
		return AddField(key, (int)value);
	}

	public EventRecord AddField(string key, ushort value)
	{
		return AddField(key, (int)value);
	}

	public EventRecord AddField(string key, int value)
	{
		return AddField(key, (long)value);
	}

	public EventRecord AddField(string key, uint value)
	{
		return AddField(key, (long)value);
	}

	public EventRecord AddField(string key, ulong value)
	{
		return AddField(key, (long)value);
	}

	[Obsolete("Char not supported, either cast to int or string", true)]
	public EventRecord AddField(string key, char value)
	{
		throw new NotImplementedException();
	}

	public EventRecord AddField(string key, float value)
	{
		return AddField(key, (double)value);
	}

	public EventRecord AddField(string key, long value)
	{
		Data.Add(new EventRecordField(key)
		{
			Value = new EventRecordField.ValueUnion
			{
				Number = value
			},
			Type = FieldType.Number
		});
		return this;
	}

	public EventRecord AddField(string key, double value)
	{
		Data.Add(new EventRecordField(key)
		{
			Value = new EventRecordField.ValueUnion
			{
				Float = value
			},
			Type = FieldType.Float
		});
		return this;
	}

	public EventRecord AddField(string key, TimeSpan value)
	{
		Data.Add(new EventRecordField(key)
		{
			Value = new EventRecordField.ValueUnion
			{
				Number = value.Ticks * NSPerTick
			},
			Type = FieldType.Number
		});
		return this;
	}

	public EventRecord AddLegacyTimespan(string key, TimeSpan value)
	{
		Data.Add(new EventRecordField(key)
		{
			Value = new EventRecordField.ValueUnion
			{
				Float = value.TotalSeconds
			},
			Type = FieldType.Float
		});
		return this;
	}

	public EventRecord AddField(string key, Guid value)
	{
		Data.Add(new EventRecordField(key)
		{
			Value = new EventRecordField.ValueUnion
			{
				Guid = value
			},
			Type = FieldType.Guid
		});
		return this;
	}

	public EventRecord AddField(string key, Vector3 value)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Data.Add(new EventRecordField(key)
		{
			Value = new EventRecordField.ValueUnion
			{
				Vector = value
			},
			Type = FieldType.Vector
		});
		return this;
	}

	public EventRecord AddField(string key, string key2, byte[] bytes)
	{
		EventRecordField item = new EventRecordField(key, key2);
		item.Bytes = Pool.Get<MemoryStream>();
		item.Bytes.Write(bytes);
		item.Type = FieldType.Bytes;
		Data.Add(item);
		return this;
	}

	public EventRecord AddField(string key, string key2, MemoryStream bytes)
	{
		EventRecordField item = new EventRecordField(key, key2);
		item.Bytes = Pool.Get<MemoryStream>();
		item.Bytes.Write(bytes.GetBuffer(), (int)bytes.Position, (int)bytes.Length);
		item.Type = FieldType.Bytes;
		Data.Add(item);
		return this;
	}

	public EventRecord AddField(string key, BaseNetworkable entity)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)entity == (Object)null || entity.net == null)
		{
			return this;
		}
		if (entity is BasePlayer { IsNpc: false, IsBot: false } basePlayer)
		{
			string userWipeId = SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(basePlayer.userID);
			AddField(key, "_userid", userWipeId);
			AddField(key, "_steamid", basePlayer.UserIDString);
			AddField(key, "_modelstate", (basePlayer.modelStateTick ?? basePlayer.modelState).flags);
			AddField(key, "_tickViewAngles", basePlayer.tickViewAngles);
			AddField(key, "_mouse_delta", basePlayer.tickMouseDelta);
			AddField(key, "_heldentity", ((Object)(object)basePlayer.GetHeldEntity() != (Object)null) ? basePlayer.GetHeldEntity().ShortPrefabName : "");
			AddField(key, "_mounted", Object.op_Implicit((Object)(object)basePlayer.GetMounted()));
			AddField(key, "_parented", basePlayer.HasParent());
			if (basePlayer.IsAdmin || basePlayer.IsDeveloper)
			{
				AddField(key, "_admin", value: true);
			}
		}
		if (entity is BaseEntity { skinID: not 0uL } baseEntity)
		{
			AddField(key, "_skin", baseEntity.skinID);
		}
		if (entity is BaseProjectile baseProjectile)
		{
			Item item = baseProjectile.GetItem();
			if (item != null && (item.contents?.itemList?.Count).GetValueOrDefault() > 0)
			{
				List<string> list = Pool.Get<List<string>>();
				foreach (Item item2 in item.contents.itemList)
				{
					list.Add(item2.info.shortname);
				}
				AddObject(key + "_inventory", list);
				Pool.FreeUnmanaged<string>(ref list);
			}
		}
		if (entity is DroppedItem droppedItem && droppedItem.DroppedTime != default(DateTime) && droppedItem.DroppedTime >= DateTime.UnixEpoch)
		{
			string userWipeId2 = SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(droppedItem.DroppedBy);
			AddField("dropped_at", ((DateTimeOffset)droppedItem.DroppedTime).ToUnixTimeMilliseconds());
			AddField("dropped_by", userWipeId2);
			AddField("dropped_by_steam_id", droppedItem.DroppedBy);
		}
		if (entity is Door door)
		{
			AddField(key, "_building_id", door.buildingID);
		}
		if (entity is CodeLock codeLock && (Object)(object)codeLock.GetParentEntity() != (Object)null && codeLock.GetParentEntity() is DecayEntity entity2)
		{
			AddField("parent", (BaseNetworkable)entity2);
		}
		if (entity is BuildingBlock buildingBlock)
		{
			AddField(key, "_grade", (int)buildingBlock.grade);
			AddField(key, "_building_id", (int)buildingBlock.buildingID);
		}
		AddField(key, "_prefab", entity.ShortPrefabName);
		Transform transform = ((Component)entity).transform;
		AddField(key, "_pos", transform.position);
		Quaternion rotation = transform.rotation;
		AddField(key, "_rot", ((Quaternion)(ref rotation)).eulerAngles);
		AddField(key, "_id", entity.net.ID.Value);
		return this;
	}

	public EventRecord AddShortEntityField(string key, BaseEntity ent, bool includePos)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ent == (Object)null || ent.net == null)
		{
			return this;
		}
		AddField(key, "_id", ent.net.ID.Value);
		AddField(key, "_prefab_id", ent.prefabID);
		if (includePos)
		{
			Transform transform = ((Component)ent).transform;
			AddField(key, "_pos", transform.position);
			Quaternion rotation = transform.rotation;
			AddField(key, "_rot", ((Quaternion)(ref rotation)).eulerAngles);
		}
		return this;
	}

	public EventRecord AddField(string key, Item item)
	{
		if (item == null)
		{
			return this;
		}
		AddField(key, "_name", item.info.shortname);
		AddField(key, "_amount", item.amount);
		AddField(key, "_skin", item.skin);
		AddField(key, "_condition", item.conditionNormalized);
		return this;
	}

	public void MarkSubmitted()
	{
		TimesSubmitted++;
		if (TimesCreated != TimesSubmitted)
		{
			Debug.LogError((object)$"EventRecord pooling error: event has been submitted ({TimesSubmitted}) a different amount of times than it was created ({TimesCreated})");
		}
	}

	public void Submit()
	{
		if (IsServer)
		{
			AnalyticsManager.GameplayEventsTableServer.Append(this);
		}
	}

	public void SerializeAsCSV(ref Utf8ValueStringBuilder writer)
	{
		if (Data.Count == 0)
		{
			return;
		}
		bool flag = false;
		foreach (EventRecordField datum in Data)
		{
			if (flag)
			{
				((Utf8ValueStringBuilder)(ref writer)).Append(',');
			}
			else
			{
				flag = true;
			}
			((Utf8ValueStringBuilder)(ref writer)).Append('"');
			datum.Serialize(ref writer, AnalyticsDocumentMode.CSV);
			((Utf8ValueStringBuilder)(ref writer)).Append('"');
		}
	}

	public void SerializeAsJson(ref Utf8ValueStringBuilder writer, bool useDataObject = true)
	{
		((Utf8ValueStringBuilder)(ref writer)).Append("{\"Timestamp\":\"");
		((Utf8ValueStringBuilder)(ref writer)).Append(Timestamp, StandardFormats.DateTime_ISO);
		bool flag = false;
		if (useDataObject)
		{
			((Utf8ValueStringBuilder)(ref writer)).Append("\",\"Data\":{");
		}
		else
		{
			((Utf8ValueStringBuilder)(ref writer)).Append("\"");
			flag = true;
		}
		foreach (EventRecordField datum in Data)
		{
			if (flag)
			{
				((Utf8ValueStringBuilder)(ref writer)).Append(',');
			}
			else
			{
				flag = true;
			}
			((Utf8ValueStringBuilder)(ref writer)).Append("\"");
			((Utf8ValueStringBuilder)(ref writer)).Append(datum.Key1);
			if (datum.Key2 != null)
			{
				((Utf8ValueStringBuilder)(ref writer)).Append(datum.Key2);
			}
			((Utf8ValueStringBuilder)(ref writer)).Append("\":");
			if (!datum.IsObject)
			{
				((Utf8ValueStringBuilder)(ref writer)).Append('"');
			}
			datum.Serialize(ref writer, AnalyticsDocumentMode.JSON);
			if (!datum.IsObject)
			{
				((Utf8ValueStringBuilder)(ref writer)).Append("\"");
			}
		}
		if (useDataObject)
		{
			((Utf8ValueStringBuilder)(ref writer)).Append('}');
		}
		((Utf8ValueStringBuilder)(ref writer)).Append('}');
	}
}

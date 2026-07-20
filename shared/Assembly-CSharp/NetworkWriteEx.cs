using Network;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

public static class NetworkWriteEx
{
	public static void WriteObject<T>(this NetWrite write, T obj)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		if (typeof(T) == typeof(Vector3))
		{
			write.Vector3(GenericsUtil.Cast<T, Vector3>(obj));
			return;
		}
		if (typeof(T) == typeof(Vector4))
		{
			write.Vector4(GenericsUtil.Cast<T, Vector4>(obj));
			return;
		}
		if (typeof(T) == typeof(Ray))
		{
			write.Ray(GenericsUtil.Cast<T, Ray>(obj));
			return;
		}
		if (typeof(T) == typeof(float))
		{
			write.Float(GenericsUtil.Cast<T, float>(obj));
			return;
		}
		if (typeof(T) == typeof(short))
		{
			write.Int16(GenericsUtil.Cast<T, short>(obj));
			return;
		}
		if (typeof(T) == typeof(ushort))
		{
			write.UInt16(GenericsUtil.Cast<T, ushort>(obj));
			return;
		}
		if (typeof(T) == typeof(int))
		{
			write.Int32(GenericsUtil.Cast<T, int>(obj));
			return;
		}
		if (typeof(T) == typeof(uint))
		{
			write.UInt32(GenericsUtil.Cast<T, uint>(obj));
			return;
		}
		if (typeof(T) == typeof(byte[]))
		{
			write.Bytes(GenericsUtil.Cast<T, byte[]>(obj));
			return;
		}
		if (typeof(T) == typeof(long))
		{
			write.Int64(GenericsUtil.Cast<T, long>(obj));
			return;
		}
		if (typeof(T) == typeof(ulong))
		{
			write.UInt64(GenericsUtil.Cast<T, ulong>(obj));
			return;
		}
		if (typeof(T) == typeof(string))
		{
			write.String(GenericsUtil.Cast<T, string>(obj));
			return;
		}
		if (typeof(T) == typeof(sbyte))
		{
			write.Int8(GenericsUtil.Cast<T, sbyte>(obj));
			return;
		}
		if (typeof(T) == typeof(byte))
		{
			write.UInt8(GenericsUtil.Cast<T, byte>(obj));
			return;
		}
		if (typeof(T) == typeof(bool))
		{
			write.Bool(GenericsUtil.Cast<T, bool>(obj));
			return;
		}
		if (typeof(T) == typeof(Color))
		{
			write.Color(GenericsUtil.Cast<T, Color>(obj));
			return;
		}
		if (typeof(T) == typeof(Color32))
		{
			write.Color32(GenericsUtil.Cast<T, Color32>(obj));
			return;
		}
		if (typeof(T) == typeof(NetworkableId))
		{
			write.EntityID(GenericsUtil.Cast<T, NetworkableId>(obj));
			return;
		}
		if (typeof(T) == typeof(ItemContainerId))
		{
			write.ItemContainerID(GenericsUtil.Cast<T, ItemContainerId>(obj));
			return;
		}
		if (typeof(T) == typeof(ItemId))
		{
			write.ItemID(GenericsUtil.Cast<T, ItemId>(obj));
			return;
		}
		if (typeof(T) == typeof(BaseEntity))
		{
			BaseEntity baseEntity = GenericsUtil.Cast<T, BaseEntity>(obj);
			write.EntityID(((Object)(object)baseEntity != (Object)null && baseEntity.net != null) ? baseEntity.net.ID : NetworkableId.EmptyId);
			return;
		}
		if (typeof(T) == typeof(BasePlayer))
		{
			BasePlayer basePlayer = GenericsUtil.Cast<T, BasePlayer>(obj);
			ulong val = (((Object)(object)basePlayer != (Object)null && basePlayer.net != null) ? basePlayer.userID.Get() : ulong.MaxValue);
			write.UInt64(val);
			return;
		}
		if (typeof(T) == typeof(EntityRef))
		{
			write.EntityID(GenericsUtil.Cast<T, EntityRef>(obj).uid);
			return;
		}
		if (obj is IEntityRef entityRef)
		{
			write.EntityID(entityRef.uid);
			return;
		}
		IProto val2 = (IProto)((((object)obj) is IProto) ? ((object)obj) : null);
		if (val2 != null)
		{
			write.Proto<IProto>(val2);
			return;
		}
		T val3 = obj;
		Debug.LogError((object)("NetworkData.Write - no handler to write " + val3?.ToString() + " -> " + obj.GetType()));
	}

	public static void Player(this NetWrite write, BasePlayer player)
	{
		ulong val = (((Object)(object)player != (Object)null && player.net != null) ? player.userID.Get() : ulong.MaxValue);
		write.UInt64(val);
	}

	public static void Entity(this NetWrite write, BaseEntity ent)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		NetworkableId id = (((Object)(object)ent != (Object)null && ent.net != null) ? ent.net.ID : NetworkableId.EmptyId);
		write.EntityID(id);
	}

	public static void EntityRef(this NetWrite write, EntityRef entityRef)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		write.EntityID(entityRef.uid);
	}

	public static void EntityRef<T>(this NetWrite write, EntityRef<T> entityRef) where T : BaseEntity
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		write.EntityID(entityRef.uid);
	}
}

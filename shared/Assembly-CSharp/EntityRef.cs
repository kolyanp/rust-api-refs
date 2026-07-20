using System;
using UnityEngine;

public struct EntityRef : IEntityRef, IEquatable<EntityRef>
{
	internal BaseEntity ent_cached;

	internal NetworkableId id_cached;

	public NetworkableId uid
	{
		get
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			if (ent_cached.IsValid())
			{
				id_cached = ent_cached.net.ID;
			}
			return id_cached;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			id_cached = value;
			if (!((NetworkableId)(ref id_cached)).IsValid)
			{
				ent_cached = null;
			}
			else if (!ent_cached.IsValid() || !(ent_cached.net.ID == id_cached))
			{
				ent_cached = null;
			}
		}
	}

	public bool IsSet()
	{
		return ((NetworkableId)(ref id_cached)).IsValid;
	}

	public bool IsValid(bool serverside)
	{
		return Get(serverside).IsValid();
	}

	public void Set(BaseEntity ent)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		ent_cached = ent;
		id_cached = default(NetworkableId);
		if (ent_cached.IsValid())
		{
			id_cached = ent_cached.net.ID;
		}
	}

	public BaseEntity Get(bool serverside)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ent_cached == (Object)null && ((NetworkableId)(ref id_cached)).IsValid)
		{
			if (serverside)
			{
				ent_cached = BaseNetworkable.serverEntities.Find(id_cached) as BaseEntity;
			}
			else
			{
				Debug.LogWarning((object)"EntityRef: Looking for clientside entities on pure server!");
			}
		}
		if (!ent_cached.IsValid())
		{
			ent_cached = null;
		}
		return ent_cached;
	}

	public bool Equals(EntityRef other)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return uid == other.uid;
	}

	public override bool Equals(object obj)
	{
		if (obj is EntityRef other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((object)uid/*cast due to constrained. prefix*/).GetHashCode();
	}
}
public struct EntityRef<T>(NetworkableId uid) : IEntityRef, IEquatable<EntityRef<T>> where T : BaseEntity
{
	private EntityRef entityRef = new EntityRef
	{
		uid = uid
	};

	public bool IsSet => entityRef.IsSet();

	public NetworkableId uid
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return entityRef.uid;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			entityRef.uid = value;
		}
	}

	public bool IsValid(bool serverside)
	{
		return Get(serverside).IsValid();
	}

	public void Set(T entity)
	{
		entityRef.Set(entity);
	}

	public T Get(bool serverside)
	{
		BaseEntity baseEntity = entityRef.Get(serverside);
		if (baseEntity == null)
		{
			return null;
		}
		if (!(baseEntity is T result))
		{
			Set(null);
			return null;
		}
		return result;
	}

	public bool TryGet(bool serverside, out T entity)
	{
		entity = Get(serverside);
		return entity != null;
	}

	public bool Equals(EntityRef<T> other)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return uid == other.uid;
	}

	public override bool Equals(object obj)
	{
		if (obj is EntityRef<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((object)uid/*cast due to constrained. prefix*/).GetHashCode();
	}
}

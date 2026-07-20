using Network;

public static class NetworkReadEx
{
	public static BasePlayer Player(this NetRead read)
	{
		return BasePlayer.FindByID(read.UInt64());
	}

	public static BaseEntity Entity(this NetRead read)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		NetworkableId uid = read.EntityID();
		return BaseNetworkable.serverEntities.Find(uid) as BaseEntity;
	}

	public static EntityRef EntityRef(this NetRead read)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return new EntityRef
		{
			uid = read.EntityID()
		};
	}

	public static EntityRef<T> EntityRef<T>(this NetRead read) where T : BaseEntity
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return new EntityRef<T>(read.EntityID());
	}
}

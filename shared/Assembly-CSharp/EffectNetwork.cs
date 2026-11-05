using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using Network;
using Network.Visibility;
using ProtoBuf;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

public static class EffectNetwork
{
	public static void Send(Effect effect, EntityNetworkRange networkRange = EntityNetworkRange.Medium)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv == null || !Net.sv.IsConnected())
		{
			return;
		}
		using (TimeWarning.New("EffectNetwork.Send"))
		{
			if (!string.IsNullOrEmpty(effect.pooledString))
			{
				((EffectData)effect).pooledstringid = StringPool.Get(effect.pooledString);
			}
			if (((EffectData)effect).pooledstringid == 0)
			{
				Debug.Log((object)("String ID is 0 - unknown effect " + effect.pooledString));
				return;
			}
			List<Connection> list;
			if (effect.broadcast)
			{
				list = BaseNetworkable.GlobalNetworkGroup.subscribers;
			}
			else if (effect.targets != null)
			{
				list = effect.targets;
			}
			else
			{
				Group obj;
				if (((EffectData)effect).entity.IsValid)
				{
					BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(((EffectData)effect).entity) as BaseEntity;
					if (!baseEntity.IsValid())
					{
						return;
					}
					obj = baseEntity.net.group;
				}
				else
				{
					obj = Net.sv.visibility.GetGroup(effect.worldPos, networkRange);
				}
				if (obj == null)
				{
					return;
				}
				list = obj.subscribers;
			}
			if (list != null)
			{
				if ((Object)(object)effect.invisibleInstigator != (Object)null)
				{
					SendEffectFromInvisibleInstigator(effect, list, effect.invisibleInstigator);
				}
				else
				{
					SendEffectToRecipients(effect, list);
				}
			}
		}
	}

	private static void SendEffectToRecipients(Effect effect, List<Connection> recipients)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		netWrite.PacketID(Message.Type.Effect);
		ProtoStreamExtensions.WriteToStream((IProto)(object)effect, (Stream)netWrite, false, 2097152);
		NetProfileCapture.Annotate(netWrite, ((EffectData)effect).entity.Value, 0u, ((EffectData)effect).pooledstringid, auxIsStringId: true);
		netWrite.Send(new SendInfo(recipients));
	}

	private static bool IsAttachedToInvisiblePlayer(Effect effect)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return BaseNetworkable.serverEntities.Find(((EffectData)effect).entity)?.shouldFilterSubscribers ?? false;
	}

	private static void SendEffectFromInvisibleInstigator(Effect effect, List<Connection> recipients, BasePlayer instigator)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		PooledList<Connection> val = Pool.Get<PooledList<Connection>>();
		try
		{
			PooledList<Connection> val2 = Pool.Get<PooledList<Connection>>();
			try
			{
				foreach (Connection recipient in recipients)
				{
					if (recipient.player is BasePlayer player && instigator.ShouldNetworkToSkipOcclusion(player))
					{
						((List<Connection>)(object)val).Add(recipient);
					}
					else
					{
						((List<Connection>)(object)val2).Add(recipient);
					}
				}
				if (((List<Connection>)(object)val).Count > 0)
				{
					SendEffectToRecipients(effect, (List<Connection>)(object)val);
				}
				if (((List<Connection>)(object)val2).Count != 0 && !IsAttachedToInvisiblePlayer(effect))
				{
					ulong source = ((EffectData)effect).source;
					NetworkableId sourceEntity = ((EffectData)effect).sourceEntity;
					if (BaseNetworkable.serverEntities.Find(sourceEntity) is HeldEntity heldEntity)
					{
						((EffectData)effect).invisiblePlayerData = Pool.Get<InvisiblePlayerEffectData>();
						((EffectData)effect).invisiblePlayerData.heldEntityPrefabID = heldEntity.prefabID;
						((EffectData)effect).invisiblePlayerData.heldItemID = heldEntity.GetItem()?.info.itemid ?? 0;
					}
					((EffectData)effect).source = 0uL;
					((EffectData)effect).sourceEntity = default;
					SendEffectToRecipients(effect, (List<Connection>)(object)val2);
					((EffectData)effect).source = source;
					((EffectData)effect).sourceEntity = sourceEntity;
					if (((EffectData)effect).invisiblePlayerData != null)
					{
						((EffectData)effect).invisiblePlayerData.ResetToPool();
						((EffectData)effect).invisiblePlayerData = null;
					}
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void Send(Effect effect, Connection target)
	{
		((EffectData)effect).pooledstringid = StringPool.Get(effect.pooledString);
		if (((EffectData)effect).pooledstringid == 0)
		{
			Debug.LogWarning((object)("EffectNetwork.Send - unpooled effect name: " + effect.pooledString));
			return;
		}
		NetWrite netWrite = Net.sv.StartWrite();
		netWrite.PacketID(Message.Type.Effect);
		ProtoStreamExtensions.WriteToStream((IProto)(object)effect, (Stream)netWrite, false, 2097152);
		NetProfileCapture.Annotate(netWrite, ((EffectData)effect).entity.Value, 0u, ((EffectData)effect).pooledstringid, auxIsStringId: true);
		netWrite.Send(new SendInfo(target));
	}
}

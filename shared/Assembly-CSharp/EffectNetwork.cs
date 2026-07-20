using System.IO;
using Facepunch.Math;
using Network;
using Network.Visibility;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

public static class EffectNetwork
{
	public static void Send(Effect effect, EntityNetworkRange networkRange = EntityNetworkRange.Medium)
	{
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv == null || !Net.sv.IsConnected())
		{
			return;
		}
		using (TimeWarning.New("EffectNetwork.Send"))
		{
			Group obj = null;
			if (!string.IsNullOrEmpty(effect.pooledString))
			{
				((EffectData)effect).pooledstringid = StringPool.Get(effect.pooledString);
			}
			if (((EffectData)effect).pooledstringid == 0)
			{
				Debug.Log((object)("String ID is 0 - unknown effect " + effect.pooledString));
				return;
			}
			if (effect.broadcast)
			{
				NetWrite netWrite = Net.sv.StartWrite();
				netWrite.PacketID(Message.Type.Effect);
				ProtoStreamExtensions.WriteToStream((IProto)(object)effect, (Stream)netWrite, false, 2097152);
				netWrite.Send(new SendInfo(BaseNetworkable.GlobalNetworkGroup.subscribers));
			}
			else if (effect.targets != null)
			{
				NetWrite netWrite2 = Net.sv.StartWrite();
				netWrite2.PacketID(Message.Type.Effect);
				ProtoStreamExtensions.WriteToStream((IProto)(object)effect, (Stream)netWrite2, false, 2097152);
				netWrite2.Send(new SendInfo(effect.targets));
			}
			else
			{
				if (((NetworkableId)(ref ((EffectData)effect).entity)).IsValid)
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
				NetWrite netWrite3 = Net.sv.StartWrite();
				netWrite3.PacketID(Message.Type.Effect);
				ProtoStreamExtensions.WriteToStream((IProto)(object)effect, (Stream)netWrite3, false, 2097152);
				netWrite3.Send(new SendInfo(obj.subscribers));
			}
			if (PacketProfiler.shouldCaptureDetailedProfiling)
			{
				BaseEntity baseEntity2 = BaseNetworkable.serverEntities.Find(((EffectData)effect).entity) as BaseEntity;
				PacketProfiler.LogDetailedOutbound(Message.Type.Effect, ((EffectData)effect).entity, ((Object)(object)baseEntity2 != (Object)null) ? baseEntity2.PrefabName : null, -1, null, Epoch.Current, server: true);
			}
		}
	}

	public static void Send(Effect effect, Connection target)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		((EffectData)effect).pooledstringid = StringPool.Get(effect.pooledString);
		if (((EffectData)effect).pooledstringid == 0)
		{
			Debug.LogWarning((object)("EffectNetwork.Send - unpooled effect name: " + effect.pooledString));
			return;
		}
		NetWrite netWrite = Net.sv.StartWrite();
		netWrite.PacketID(Message.Type.Effect);
		ProtoStreamExtensions.WriteToStream((IProto)(object)effect, (Stream)netWrite, false, 2097152);
		netWrite.Send(new SendInfo(target));
		if (PacketProfiler.shouldCaptureDetailedProfiling)
		{
			BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(((EffectData)effect).entity) as BaseEntity;
			PacketProfiler.LogDetailedOutbound(Message.Type.Effect, ((EffectData)effect).entity, ((Object)(object)baseEntity != (Object)null) ? baseEntity.PrefabName : null, (int)netWrite.Length, null, Epoch.Current, server: true);
		}
	}
}

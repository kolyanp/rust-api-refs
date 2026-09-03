using System.IO;
using Network;
using Network.Visibility;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

public static class EffectNetwork
{
	public static void Send(Effect effect, EntityNetworkRange networkRange = EntityNetworkRange.Medium)
	{
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
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
				NetProfileCapture.Annotate(netWrite, ((EffectData)effect).entity.Value, 0u, ((EffectData)effect).pooledstringid, auxIsStringId: true);
				netWrite.Send(new SendInfo(BaseNetworkable.GlobalNetworkGroup.subscribers));
				return;
			}
			if (effect.targets != null)
			{
				NetWrite netWrite2 = Net.sv.StartWrite();
				netWrite2.PacketID(Message.Type.Effect);
				ProtoStreamExtensions.WriteToStream((IProto)(object)effect, (Stream)netWrite2, false, 2097152);
				NetProfileCapture.Annotate(netWrite2, ((EffectData)effect).entity.Value, 0u, ((EffectData)effect).pooledstringid, auxIsStringId: true);
				netWrite2.Send(new SendInfo(effect.targets));
				return;
			}
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
			if (obj != null)
			{
				NetWrite netWrite3 = Net.sv.StartWrite();
				netWrite3.PacketID(Message.Type.Effect);
				ProtoStreamExtensions.WriteToStream((IProto)(object)effect, (Stream)netWrite3, false, 2097152);
				NetProfileCapture.Annotate(netWrite3, ((EffectData)effect).entity.Value, 0u, ((EffectData)effect).pooledstringid, auxIsStringId: true);
				netWrite3.Send(new SendInfo(obj.subscribers));
			}
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

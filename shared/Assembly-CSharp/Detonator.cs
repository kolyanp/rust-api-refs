using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class Detonator : HeldEntity, IRFObject
{
	public int frequency = 55;

	private float timeSinceDeploy;

	public GameObjectRef frequencyPanelPrefab;

	public GameObjectRef attackEffect;

	public GameObjectRef unAttackEffect;

	private float nextChangeTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Detonator.OnRpcMessage"))
		{
			if (rpc == 2778616053u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ServerSetFrequency"));
				}
				using (TimeWarning.New("ServerSetFrequency"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ServerSetFrequency(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in ServerSetFrequency");
					}
				}
				return true;
			}
			if (rpc == 1106698135 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SetPressed"));
				}
				using (TimeWarning.New("SetPressed"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage pressed = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SetPressed(pressed);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SetPressed");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	public void SetPressed(RPCMessage msg)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)msg.player == (Object)null) && !((Object)(object)msg.player != (Object)(object)GetOwnerPlayer()))
		{
			bool num = HasFlag(Flags.On);
			bool flag = msg.read.Bit();
			InternalSetPressed(flag);
			if (num != flag)
			{
				Effect.server.Run(flag ? attackEffect.resourcePath : unAttackEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			}
		}
	}

	internal void InternalSetPressed(bool pressed)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, pressed);
		}
		if (pressed)
		{
			RFManager.AddBroadcaster(frequency, this);
		}
		else
		{
			RFManager.RemoveBroadcaster(frequency, this);
		}
	}

	public Vector3 GetPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position;
	}

	public float GetMaxRange()
	{
		return 100000f;
	}

	public void RFSignalUpdate(bool on)
	{
	}

	public override void SetHeld(bool bHeld)
	{
		if (!bHeld)
		{
			InternalSetPressed(pressed: false);
		}
		base.SetHeld(bHeld);
	}

	[RPC_Server]
	public void ServerSetFrequency(RPCMessage msg)
	{
		ServerSetFrequency(msg.player, msg.read.Int32());
	}

	public void ServerSetFrequency(BasePlayer player, int freq)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		if ((Object)(object)player == (Object)null || (Object)(object)GetOwnerPlayer() != (Object)(object)player || Time.time < nextChangeTime)
		{
			return;
		}
		nextChangeTime = Time.time + 2f;
		if (RFManager.IsReserved(freq))
		{
			RFManager.ReserveErrorPrint(player);
		}
		else
		{
			if (Interface.CallHook("OnRfFrequencyChange", this, freq, player) != null)
			{
				return;
			}
			Item ownerItem = GetOwnerItem();
			RFManager.ChangeFrequency(frequency, freq, this, isListener: false, IsOn());
			frequency = freq;
			SendNetworkUpdate();
			Item item = GetItem();
			if (item != null)
			{
				if (item.instanceData == null)
				{
					item.instanceData = new InstanceData();
					item.instanceData.ShouldPool = false;
				}
				item.instanceData.dataInt = frequency;
				item.MarkDirty();
			}
			ownerItem?.LoseCondition(ownerItem.maxCondition * 0.01f);
			Interface.CallHook("OnRfFrequencyChanged", this, freq, player);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.ioEntity == null)
		{
			info.msg.ioEntity = Pool.Get<IOEntity>();
		}
		info.msg.ioEntity.genericInt1 = frequency;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			frequency = info.msg.ioEntity.genericInt1;
		}
	}

	public int GetFrequency()
	{
		return frequency;
	}
}

using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class PagerEntity : BaseEntity, IRFObject
{
	public static Flags Flag_Silent = Flags.Reserved1;

	private int frequency = 55;

	public float beepRepeat = 2f;

	public GameObjectRef pagerEffect;

	public GameObjectRef silentEffect;

	private float nextChangeTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PagerEntity.OnRpcMessage"))
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
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2778616053u, "ServerSetFrequency", this, player, 3f))
						{
							return true;
						}
					}
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
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public int GetFrequency()
	{
		return frequency;
	}

	public override void SwitchParent(BaseEntity ent)
	{
		SetParent(ent, worldPositionStays: false, sendImmediate: true);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		RFManager.AddListener(frequency, this);
	}

	internal override void DoServerDestroy()
	{
		RFManager.RemoveListener(frequency, this);
		base.DoServerDestroy();
	}

	public Vector3 GetPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position;
	}

	public float GetMaxRange()
	{
		return float.PositiveInfinity;
	}

	public void RFSignalUpdate(bool on)
	{
		if (base.IsDestroyed)
		{
			return;
		}
		bool flag = IsOn();
		if (on != flag)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, on);
			}
			SendNetworkUpdate();
		}
	}

	public void SetSilentMode(bool wantsSilent)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flag_Silent, wantsSilent);
	}

	public void SetOff()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, b: false);
	}

	public void ChangeFrequency(int newFreq)
	{
		RFManager.ChangeFrequency(frequency, newFreq, this, isListener: true);
		frequency = newFreq;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void ServerSetFrequency(RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null) && msg.player.CanBuild() && !(Time.time < nextChangeTime))
		{
			nextChangeTime = Time.time + 2f;
			int num = msg.read.Int32();
			if (Interface.CallHook("OnRfFrequencyChange", this, num, msg.player) == null)
			{
				RFManager.ChangeFrequency(frequency, num, this, isListener: true);
				frequency = num;
				SendNetworkUpdateImmediate();
				Interface.CallHook("OnRfFrequencyChanged", this, num, msg.player);
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.ioEntity = Pool.Get<IOEntity>();
		info.msg.ioEntity.genericInt1 = frequency;
	}

	internal override void OnParentRemoved()
	{
		SetParent(null, worldPositionStays: false, sendImmediate: true);
	}

	public void OnParentDestroying()
	{
		if (base.isServer)
		{
			((Component)this).transform.parent = null;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			frequency = info.msg.ioEntity.genericInt1;
		}
		if (base.isServer && info.fromDisk)
		{
			ChangeFrequency(frequency);
		}
	}
}

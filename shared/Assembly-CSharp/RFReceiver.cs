using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class RFReceiver : IOEntity, IRFObject
{
	public int frequency;

	public GameObjectRef frequencyPanelPrefab;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RFReceiver.OnRpcMessage"))
		{
			if (rpc == 1052196345 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_RequestOpenPanel"));
				}
				using (TimeWarning.New("SERVER_RequestOpenPanel"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1052196345u, "SERVER_RequestOpenPanel", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1052196345u, "SERVER_RequestOpenPanel", this, player, 3f))
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
							SERVER_RequestOpenPanel(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SERVER_RequestOpenPanel");
					}
				}
				return true;
			}
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
						if (!RPC_Server.CallsPerSecond.Test(2778616053u, "ServerSetFrequency", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2778616053u, "ServerSetFrequency", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ServerSetFrequency(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in ServerSetFrequency");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private bool CanChangeFrequency(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null)
		{
			return player.CanBuild();
		}
		return false;
	}

	public int GetFrequency()
	{
		return frequency;
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
		if (!base.IsDestroyed && IsOn() != on && !(!IsPowered() && on))
		{
			SetFlagLocal(Flags.On, on);
			SendNetworkUpdate_Flags();
			MarkDirty();
		}
	}

	internal override void DoServerDestroy()
	{
		RFManager.RemoveListener(frequency, this);
		base.DoServerDestroy();
	}

	public override void ResetIOState()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, b: false);
	}

	public override bool WantsPower(int inputIndex)
	{
		return IsOn();
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (!IsOn())
		{
			return 0;
		}
		return GetCurrentEnergy();
	}

	public override void UpdateFromInput(int inputAmount, int inputSlot)
	{
		base.UpdateFromInput(inputAmount, inputSlot);
		if (inputAmount > 0 && !IsOn())
		{
			RFManager.AddListener(frequency, this);
		}
		else if (inputAmount == 0)
		{
			RFManager.RemoveListener(frequency, this);
			SetFlagLocal(Flags.On, b: false);
			SendNetworkUpdate_Flags();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(3uL)]
	public void ServerSetFrequency(RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null) && msg.player.CanBuild())
		{
			int num = RFManager.ClampFrequency(msg.read.Int32());
			if (Interface.CallHook("OnRfFrequencyChange", this, num, msg.player) == null)
			{
				SetFrequency(num);
				Interface.CallHook("OnRfFrequencyChanged", this, num, msg.player);
			}
		}
	}

	public void SetFrequency(int freq)
	{
		freq = RFManager.ClampFrequency(freq);
		RFManager.ChangeFrequency(frequency, freq, this, isListener: true, IsPowered());
		frequency = freq;
		MarkDirty();
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server.IsVisible(3f)]
	public void SERVER_RequestOpenPanel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && CanChangeFrequency(player))
		{
			ClientRPC(RpcTarget.Player("CLIENT_OpenPanel", player), frequency);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.ioEntity.genericInt1 = frequency;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			frequency = info.msg.ioEntity.genericInt1;
			if (info.fromDisk && base.isServer)
			{
				RFSignalUpdate(on: false);
			}
		}
	}
}

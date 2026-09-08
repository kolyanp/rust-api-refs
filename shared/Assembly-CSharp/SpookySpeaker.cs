using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class SpookySpeaker : IOEntity
{
	public SoundPlayer soundPlayer;

	public float soundSpacing = 12f;

	public float soundSpacingRand = 5f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SpookySpeaker.OnRpcMessage"))
		{
			if (rpc == 2523893445u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SetWantsOn"));
				}
				using (TimeWarning.New("SetWantsOn"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2523893445u, "SetWantsOn", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage wantsOn = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SetWantsOn(wantsOn);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SetWantsOn");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		UpdateInvokes();
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		base.UpdateHasPower(inputAmount, inputSlot);
		if (inputSlot == 1)
		{
			SetTargetState(state: false);
		}
		if (inputSlot == 0)
		{
			SetTargetState(state: true);
		}
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	private void SetTargetState(bool state)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, state);
		}
		UpdateInvokes();
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void SetWantsOn(RPCMessage msg)
	{
		bool targetState = msg.read.Bit();
		SetTargetState(targetState);
	}

	public void UpdateInvokes()
	{
		if (IsOn())
		{
			InvokeRandomized(SendPlaySound, soundSpacing, soundSpacing, soundSpacingRand);
			Invoke(DelayedOff, 7200f);
		}
		else
		{
			CancelInvoke(SendPlaySound);
			CancelInvoke(DelayedOff);
		}
	}

	public void SendPlaySound()
	{
		ClientRPC(RpcTarget.NetworkGroup("PlaySpookySound"));
	}

	public void DelayedOff()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, b: false);
	}
}

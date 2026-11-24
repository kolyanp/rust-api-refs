using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class ElectricSwitch : IOEntity
{
	[Header("Electric Switch")]
	public bool isToggleSwitch;

	public bool requiresPowergrid;

	private Action _actionUnBusy;

	private Action actionUnBusy => UnBusy;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ElectricSwitch.OnRpcMessage"))
		{
			if (rpc == 3043863856u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Switch"));
				}
				using (TimeWarning.New("RPC_Switch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3043863856u, "RPC_Switch", this, player, 3f))
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
							RPC_Switch(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_Switch");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override bool WantsPower(int inputIndex)
	{
		if (inputIndex == 0)
		{
			return IsOn();
		}
		return false;
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Busy, b: false);
	}

	public override void ResetIOState()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, b: false);
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (!IsOn())
		{
			return 0;
		}
		return GetCurrentEnergy();
	}

	public override int CalculateCurrentEnergy(int inputAmount, int inputSlot)
	{
		if (inputSlot != 0)
		{
			return currentEnergy;
		}
		return base.CalculateCurrentEnergy(inputAmount, inputSlot);
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		if (inputSlot == 1 && inputAmount > 0)
		{
			SetSwitch(state: true);
		}
		if (inputSlot == 2 && inputAmount > 0)
		{
			SetSwitch(state: false);
		}
		if (inputSlot == 0)
		{
			base.UpdateHasPower(inputAmount, inputSlot);
		}
	}

	public virtual void SetSwitch(bool state)
	{
		if (state != IsOn())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local))
			{
				flagsUpdateScope.Set(Flags.On, state);
				flagsUpdateScope.Set(Flags.Busy, b: true);
			}
			Invoke(actionUnBusy, 0.5f);
			SendNetworkUpdateImmediate();
			MarkDirty();
		}
	}

	public void Flip()
	{
		SetSwitch(!IsOn());
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_Switch(RPCMessage msg)
	{
		if (Interface.CallHook("OnSwitchToggle", this, msg.player) == null)
		{
			bool flag = msg.read.Bool();
			SetSwitch(flag);
			Interface.CallHook("OnSwitchToggled", this, msg.player);
		}
	}

	private void UnBusy()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Busy, b: false);
	}
}

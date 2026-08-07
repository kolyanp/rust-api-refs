using System;
using ConVar;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class SmartSwitch : AppIOEntity
{
	[Header("Smart Switch")]
	public Animator ReceiverAnimator;

	public override AppEntityType Type => (AppEntityType)1;

	public override bool Value
	{
		get
		{
			return IsOn();
		}
		set
		{
			SetSwitch(value);
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SmartSwitch.OnRpcMessage"))
		{
			if (rpc == 2810053005u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ToggleSwitch"));
				}
				using (TimeWarning.New("ToggleSwitch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2810053005u, "ToggleSwitch", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2810053005u, "ToggleSwitch", this, player, 3f))
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
							ToggleSwitch(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in ToggleSwitch");
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
			SetSwitch(wantsOn: true);
		}
		if (inputSlot == 2 && inputAmount > 0)
		{
			SetSwitch(wantsOn: false);
		}
		if (inputSlot == 0)
		{
			base.UpdateHasPower(inputAmount, inputSlot);
		}
	}

	public void SetSwitch(bool wantsOn)
	{
		if (wantsOn != IsOn())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local))
			{
				flagsUpdateScope.Set(Flags.On, wantsOn);
				flagsUpdateScope.Set(Flags.Busy, b: true);
			}
			Invoke(Unbusy, 0.5f);
			SendNetworkUpdateImmediate();
			MarkDirty();
			BroadcastValueChange();
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server.IsVisible(3f)]
	public void ToggleSwitch(RPCMessage msg)
	{
		if (PlayerCanToggle(msg.player))
		{
			SetSwitch(!IsOn());
		}
	}

	public void Unbusy()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Busy, b: false);
	}

	private static bool PlayerCanToggle(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null)
		{
			return player.CanBuild();
		}
		return false;
	}
}

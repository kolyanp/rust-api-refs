using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class PressButton : IOEntity
{
	public const Flags Flag_EmittingPower = Flags.Reserved3;

	[Header("PressButton")]
	public float pressDuration = 5f;

	public float pressPowerTime = 0.5f;

	public int pressPowerAmount = 2;

	public bool smallBurst;

	private Action _actionUnpress;

	private Action _actionUnpowerTime;

	private Action actionUnpress => Unpress;

	private Action actionUnpowerTime => UnpowerTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PressButton.OnRpcMessage"))
		{
			if (rpc == 4188121069u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Press"));
				}
				using (TimeWarning.New("RPC_Press"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(4188121069u, "RPC_Press", this, player, 3f))
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
							RPC_Press(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_Press");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	public override void ResetIOState()
	{
		base.ResetIOState();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: false);
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
		}
		CancelInvoke(actionUnpress);
		CancelInvoke(actionUnpowerTime);
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (IsOn())
		{
			if ((Object)(object)sourceItem != (Object)null || smallBurst)
			{
				if (HasFlag(Flags.Reserved3))
				{
					return Mathf.Max(pressPowerAmount, base.GetPassthroughAmount());
				}
				return 0;
			}
			return base.GetPassthroughAmount();
		}
		return 0;
	}

	public void UnpowerTime()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
		}
		MarkDirty();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, b: false);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_Press(RPCMessage msg)
	{
		if (Interface.CallHook("OnButtonPress", this, msg.player) == null)
		{
			Press();
		}
	}

	public void Press()
	{
		if (!IsOn())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local))
			{
				flagsUpdateScope.Set(Flags.On, b: true);
				flagsUpdateScope.Set(Flags.Reserved3, b: true);
			}
			Invoke(actionUnpowerTime, pressPowerTime);
			SendNetworkUpdateImmediate();
			MarkDirty();
			Invoke(actionUnpress, pressDuration);
		}
	}

	public void Unpress()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: false);
		}
		MarkDirty();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.ioEntity.genericFloat1 = pressDuration;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			pressDuration = info.msg.ioEntity.genericFloat1;
		}
	}
}

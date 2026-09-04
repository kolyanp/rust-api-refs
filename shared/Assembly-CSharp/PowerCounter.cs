using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class PowerCounter : IOEntity
{
	public const int MAX_VALUE = 999;

	[Space]
	public Canvas canvas;

	public CanvasGroup screenAlpha;

	public Text screenText;

	public GameObjectRef counterConfigPanel;

	[Space]
	public Color passthroughColor;

	public Color counterColor;

	public int counterNumber;

	public int targetCounterNumber = 10;

	private int resetTargetNumber;

	public const Flags Flag_ShowPassthrough = Flags.Reserved3;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PowerCounter.OnRpcMessage"))
		{
			if (rpc == 3554226761u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_SetTarget"));
				}
				using (TimeWarning.New("SERVER_SetTarget"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3554226761u, "SERVER_SetTarget", this, player, 3f))
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
							SERVER_SetTarget(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SERVER_SetTarget");
					}
				}
				return true;
			}
			if (rpc == 3222475159u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ToggleDisplayMode"));
				}
				using (TimeWarning.New("ToggleDisplayMode"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3222475159u, "ToggleDisplayMode", this, player, 3f))
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
							ToggleDisplayMode(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in ToggleDisplayMode");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	public bool DisplayPassthrough()
	{
		return HasFlag(Flags.Reserved3);
	}

	public bool DisplayCounter()
	{
		return !DisplayPassthrough();
	}

	public bool CanPlayerAdmin(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null)
		{
			return player.CanBuild();
		}
		return false;
	}

	public int GetTarget()
	{
		return targetCounterNumber;
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void SERVER_SetTarget(RPCMessage msg)
	{
		int num = msg.read.Int32();
		if (Interface.CallHook("OnCounterTargetChange", this, msg.player, num) == null && CanPlayerAdmin(msg.player))
		{
			targetCounterNumber = num;
			resetTargetNumber = msg.read.Int32();
			targetCounterNumber = Mathf.Clamp(targetCounterNumber, 1, 999);
			resetTargetNumber = Mathf.Clamp(resetTargetNumber, 0, 999);
			MarkDirty();
			SendNetworkUpdate();
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void ToggleDisplayMode(RPCMessage msg)
	{
		bool flag = msg.read.Bit();
		if (Interface.CallHook("OnCounterModeToggle", this, msg.player, flag) == null && msg.player.CanBuild())
		{
			SetFlagLocal(Flags.Reserved3, flag);
			MarkDirty();
			SendNetworkUpdate();
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (DisplayPassthrough())
		{
			return GetCurrentEnergy();
		}
		if (counterNumber >= targetCounterNumber)
		{
			return base.GetPassthroughAmount(outputSlot);
		}
		return 0;
	}

	public override bool WantsPower(int inputIndex)
	{
		if (inputIndex != 0)
		{
			return false;
		}
		if (DisplayPassthrough())
		{
			return true;
		}
		return counterNumber >= targetCounterNumber;
	}

	public void SetCounterNumber(int newNumber)
	{
		counterNumber = newNumber;
	}

	public override void SendIONetworkUpdate()
	{
		SendNetworkUpdate();
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		if (inputSlot == 0)
		{
			base.UpdateHasPower(inputAmount, inputSlot);
		}
	}

	public override void UpdateFromInput(int inputAmount, int inputSlot)
	{
		if (DisplayCounter() && inputAmount > 0 && inputSlot != 0)
		{
			int num = counterNumber;
			switch (inputSlot)
			{
			case 1:
				counterNumber++;
				break;
			case 2:
				counterNumber--;
				if (counterNumber < 0)
				{
					counterNumber = 0;
				}
				break;
			case 3:
				counterNumber = resetTargetNumber;
				break;
			}
			counterNumber = Mathf.Clamp(counterNumber, 0, 999);
			if (num != counterNumber)
			{
				MarkDirty();
				SendNetworkUpdate();
			}
		}
		if (inputSlot == 0)
		{
			base.UpdateFromInput(inputAmount, inputSlot);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.ioEntity == null)
		{
			info.msg.ioEntity = Pool.Get<IOEntity>();
		}
		info.msg.ioEntity.genericInt1 = counterNumber;
		info.msg.ioEntity.genericInt2 = GetPassthroughAmount();
		info.msg.ioEntity.genericInt3 = GetTarget();
		info.msg.ioEntity.genericFloat1 = resetTargetNumber;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			if (base.isServer)
			{
				counterNumber = info.msg.ioEntity.genericInt1;
			}
			targetCounterNumber = info.msg.ioEntity.genericInt3;
			resetTargetNumber = (int)info.msg.ioEntity.genericFloat1;
		}
	}
}

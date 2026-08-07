using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class WipeLaptopEntity : BaseEntity
{
	public static Phrase Phrase_Armed = new Phrase("laptop_armed", "Warhead Status: Armed");

	public static Phrase Phrase_Disarmed = new Phrase("laptop_disarmed", "Warhead Status: Disarmed");

	public static Flags ArmedFlag = Flags.Reserved1;

	public float ArmTime = 5f;

	public float DisarmTime = 5f;

	public TimeUntil TimeLeft;

	public Text MiddleText;

	private Phrase currentMiddlePhrase;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("WipeLaptopEntity.OnRpcMessage"))
		{
			if (rpc == 2017018603 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ArmLaptop"));
				}
				using (TimeWarning.New("ArmLaptop"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2017018603u, "ArmLaptop", this, player, 5f))
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
							ArmLaptop(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in ArmLaptop");
					}
				}
				return true;
			}
			if (rpc == 2423597272u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - DefuseLaptop"));
				}
				using (TimeWarning.New("DefuseLaptop"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2423597272u, "DefuseLaptop", this, player, 5f))
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
							DefuseLaptop(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in DefuseLaptop");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Save(SaveInfo info)
	{
		info.msg.wipeLaptop = Pool.Get<WipeLaptop>();
		info.msg.wipeLaptop.timeLeft = (int)((TimeUntil)(ref TimeLeft)).LeftFrom(info.cachedTime.Time);
		info.msg.wipeLaptop.armTime = ArmTime;
		info.msg.wipeLaptop.disarmTime = DisarmTime;
		base.Save(info);
	}

	public override void Load(LoadInfo info)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (info.msg.wipeLaptop != null)
		{
			TimeLeft = TimeUntil.op_Implicit((float)info.msg.wipeLaptop.timeLeft);
			ArmTime = info.msg.wipeLaptop.armTime;
			DisarmTime = info.msg.wipeLaptop.disarmTime;
		}
		base.Load(info);
	}

	[RPC_Server.IsVisible(5f)]
	[RPC_Server]
	public void ArmLaptop(RPCMessage msg)
	{
		if (msg.read.Int32() == 3 && !IsArmed())
		{
			SetArmed(state: true);
		}
	}

	private void OnArmLaptopStart()
	{
	}

	[RPC_Server.IsVisible(5f)]
	[RPC_Server]
	public void DefuseLaptop(RPCMessage msg)
	{
		if (msg.read.Int32() == 3 && IsArmed())
		{
			SetArmed(state: false);
		}
	}

	private void SetArmed(bool state)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(ArmedFlag, state);
	}

	private bool IsArmed()
	{
		return HasFlag(ArmedFlag);
	}

	public void SetTimeLeft(int seconds)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		TimeLeft = TimeUntil.op_Implicit((float)seconds);
		if (base.isServer)
		{
			SendNetworkUpdate();
		}
	}
}

using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class RustigeEgg : BaseCombatEntity
{
	public const Flags Flag_Spin = Flags.Reserved1;

	public Transform eggRotationTransform;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RustigeEgg.OnRpcMessage"))
		{
			if (rpc == 4254195175u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Open"));
				}
				using (TimeWarning.New("RPC_Open"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(4254195175u, "RPC_Open", this, player, 3f))
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
							RPC_Open(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_Open");
					}
				}
				return true;
			}
			if (rpc == 1455840454 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Spin"));
				}
				using (TimeWarning.New("RPC_Spin"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1455840454u, "RPC_Spin", this, player, 3f))
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
							RPC_Spin(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_Spin");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsSpinning()
	{
		return HasFlag(Flags.Reserved1);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_Spin(RPCMessage msg)
	{
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_Open(RPCMessage msg)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)msg.player == (Object)null)
		{
			return;
		}
		bool flag = msg.read.Bit();
		if (flag == IsOpen())
		{
			return;
		}
		if (flag)
		{
			ClientRPC(RpcTarget.NetworkGroup("FaceEggPosition"), msg.player.eyes.position);
			Invoke(CloseEgg, 60f);
		}
		else
		{
			CancelInvoke(CloseEgg);
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local))
		{
			flagsUpdateScope.Set(Flags.Open, flag);
			if (IsSpinning() & flag)
			{
				flagsUpdateScope.Set(Flags.Reserved1, b: false);
			}
		}
		SendNetworkUpdateImmediate();
	}

	public void CloseEgg()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Open, b: false);
	}
}

using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class CommandBlock : IOEntity
{
	public GameObjectRef commandPanelPrefab;

	[HideInInspector]
	public string currentCommand;

	private int currentPower;

	[HideInInspector]
	public ulong lastPlayerID;

	private static readonly Phrase disabledErrorPhrase;

	[ServerVar(Help = "Can command blocks execute commands")]
	public static bool commands_enabled;

	[ServerVar(Help = "If enabled, commands from command blocks will run using the last player who set them, allowing for a wider range of commands to be used")]
	public static bool use_player;

	[ServerVar(Help = "Print a log message when a command block is executed")]
	public static bool log_executions;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CommandBlock.OnRpcMessage"))
		{
			if (rpc == 1558722905 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_SetCommand"));
				}
				using (TimeWarning.New("RPC_SetCommand"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1558722905u, "RPC_SetCommand", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1558722905u, "RPC_SetCommand", this, player, 3f))
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
							RPC_SetCommand(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_SetCommand");
					}
				}
				return true;
			}
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
						if (!RPC_Server.MaxDistance.Test(1052196345u, "SERVER_RequestOpenPanel", this, player, 3f))
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
							SERVER_RequestOpenPanel(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SERVER_RequestOpenPanel");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.commandBlock != null)
		{
			currentCommand = info.msg.commandBlock.currentCommand;
			lastPlayerID = info.msg.commandBlock.lastPlayerID;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			if (info.msg.commandBlock == null)
			{
				info.msg.commandBlock = Pool.Get<CommandBlock>();
			}
			info.msg.commandBlock.currentCommand = currentCommand;
			info.msg.commandBlock.lastPlayerID = lastPlayerID;
		}
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		if (!commands_enabled)
		{
			currentPower = inputAmount;
			return;
		}
		if (inputAmount == 0 || currentPower > 0)
		{
			currentPower = inputAmount;
			return;
		}
		if (string.IsNullOrEmpty(currentCommand))
		{
			currentPower = inputAmount;
			return;
		}
		if (log_executions)
		{
			Debug.Log((object)("Executing \"" + currentCommand + "\" via CommandBlock"));
		}
		ConsoleSystem.Option options = ConsoleSystem.Option.Server;
		options.FromCommandBlock = true;
		if (use_player)
		{
			BasePlayer basePlayer = BasePlayer.FindByID(lastPlayerID);
			if ((Object)(object)basePlayer != (Object)null && basePlayer.Connection != null)
			{
				options = ConsoleSystem.Option.Server.FromConnection(basePlayer.Connection);
			}
		}
		ConsoleSystem.Run(options, currentCommand);
		currentPower = inputAmount;
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void SERVER_RequestOpenPanel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && (player.IsAdmin || player.IsDeveloper))
		{
			if (!commands_enabled)
			{
				player.ShowToast(GameTip.Styles.Error, disabledErrorPhrase, true);
			}
			ClientRPC(RpcTarget.Player("CLIENT_OpenPanel", player), currentCommand);
		}
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_SetCommand(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && (player.IsAdmin || player.IsDeveloper))
		{
			string text = msg.read.String();
			currentCommand = text;
			lastPlayerID = player.userID.Get();
		}
	}

	static CommandBlock()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		disabledErrorPhrase = new Phrase("commandblock.disabled.error", "Command blocks are currently disabled");
		commands_enabled = false;
		use_player = false;
		log_executions = true;
	}
}

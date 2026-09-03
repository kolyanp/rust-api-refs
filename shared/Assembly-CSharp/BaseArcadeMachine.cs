using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class BaseArcadeMachine : BaseVehicle
{
	public class ScoreEntry
	{
		public ulong playerID;

		public int score;

		public string displayName;
	}

	public BaseArcadeGame arcadeGamePrefab;

	public BaseArcadeGame activeGame;

	public ArcadeNetworkTrigger networkTrigger;

	public float broadcastRadius = 8f;

	public Transform gameScreen;

	public RawImage RTImage;

	public Transform leftJoystick;

	public Transform rightJoystick;

	public SoundPlayer musicPlayer;

	public const Flags Flag_P1 = Flags.Reserved6;

	public const Flags Flag_P2 = Flags.Reserved8;

	public List<ScoreEntry> scores = new List<ScoreEntry>(10);

	private const int inputFrameRate = 60;

	private const int snapshotFrameRate = 15;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BaseArcadeMachine.OnRpcMessage"))
		{
			if (rpc == 271542211 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - BroadcastEntityMessage"));
				}
				using (TimeWarning.New("BroadcastEntityMessage"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(271542211u, "BroadcastEntityMessage", this, player, 7uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(271542211u, "BroadcastEntityMessage", this, player, 3f))
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
							BroadcastEntityMessage(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in BroadcastEntityMessage");
					}
				}
				return true;
			}
			if (rpc == 1365277306 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - DestroyMessageFromHost"));
				}
				using (TimeWarning.New("DestroyMessageFromHost"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1365277306u, "DestroyMessageFromHost", this, player, 3f))
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
							DestroyMessageFromHost(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in DestroyMessageFromHost");
					}
				}
				return true;
			}
			if (rpc == 2467852388u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - GetSnapshotFromClient"));
				}
				using (TimeWarning.New("GetSnapshotFromClient"))
				{
					using (msg.read.UseRepeatedElementLimit(64))
					{
						using (TimeWarning.New("Conditions"))
						{
							if (!RPC_Server.CallsPerSecond.Test(2467852388u, "GetSnapshotFromClient", this, player, 30uL))
							{
								return true;
							}
							long position = msg.read.Position;
							ArcadeGame val = msg.read.Proto<ArcadeGame>((ArcadeGame)null);
							try
							{
								foreach (arcadeEnt arcadeEnt in val.arcadeEnts)
								{
									if (!RPC_Server.InputValidation.Test(arcadeEnt.position))
									{
										return true;
									}
									if (!RPC_Server.InputValidation.Test(arcadeEnt.heading))
									{
										return true;
									}
									if (!RPC_Server.InputValidation.Test(arcadeEnt.scale))
									{
										return true;
									}
									if (!RPC_Server.InputValidation.Test(arcadeEnt.colliderScale))
									{
										return true;
									}
									if (!RPC_Server.InputValidation.Test(arcadeEnt.alpha))
									{
										return true;
									}
								}
								msg.read.Position = position;
								if (!RPC_Server.IsVisible.Test(2467852388u, "GetSnapshotFromClient", this, player, 3f))
								{
									return true;
								}
							}
							finally
							{
								((IDisposable)val)?.Dispose();
							}
						}
						try
						{
							using (TimeWarning.New("Call"))
							{
								RPCMessage msg4 = new RPCMessage
								{
									connection = msg.connection,
									player = player,
									read = msg.read
								};
								GetSnapshotFromClient(msg4);
							}
						}
						catch (Exception ex3)
						{
							Debug.LogException(ex3);
							player.Kick("RPC Error in GetSnapshotFromClient");
						}
					}
				}
				return true;
			}
			if (rpc == 2990871635u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RequestAddScore"));
				}
				using (TimeWarning.New("RequestAddScore"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2990871635u, "RequestAddScore", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RequestAddScore(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in RequestAddScore");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void AddScore(BasePlayer player, int score)
	{
		ScoreEntry scoreEntry = new ScoreEntry();
		scoreEntry.displayName = player.displayName;
		scoreEntry.score = score;
		scoreEntry.playerID = player.userID;
		scores.Add(scoreEntry);
		scores.Sort((ScoreEntry a, ScoreEntry b) => b.score.CompareTo(a.score));
		scores.TrimExcess();
		SendNetworkUpdate();
		Interface.CallHook("OnArcadeScoreAdded", this, player, score);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RequestAddScore(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && PlayerIsMounted(player))
		{
			int score = msg.read.Int32();
			AddScore(player, score);
		}
	}

	public override void PlayerMounted(BasePlayer player, BaseMountable seat)
	{
		base.PlayerMounted(player, seat);
		ClientRPC(RpcTarget.Player("BeginHosting", player));
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved6, b: true, recursive: true);
	}

	public override void PlayerDismounted(BasePlayer player, BaseMountable seat)
	{
		base.PlayerDismounted(player, seat);
		ClientRPC(RpcTarget.Player("EndHosting", player));
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved6, b: false, recursive: true);
		}
		if (!AnyMounted())
		{
			NearbyClientMessage("NoHost");
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.arcadeMachine = Pool.Get<ArcadeMachine>();
		info.msg.arcadeMachine.scores = Pool.Get<List<ScoreEntry>>();
		for (int i = 0; i < scores.Count; i++)
		{
			ScoreEntry val = Pool.Get<ScoreEntry>();
			val.displayName = scores[i].displayName;
			val.playerID = scores[i].playerID;
			val.score = scores[i].score;
			info.msg.arcadeMachine.scores.Add(val);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.arcadeMachine != null && info.msg.arcadeMachine.scores != null)
		{
			scores.Clear();
			for (int i = 0; i < info.msg.arcadeMachine.scores.Count; i++)
			{
				ScoreEntry scoreEntry = new ScoreEntry();
				scoreEntry.displayName = info.msg.arcadeMachine.scores[i].displayName;
				scoreEntry.score = info.msg.arcadeMachine.scores[i].score;
				scoreEntry.playerID = info.msg.arcadeMachine.scores[i].playerID;
				scores.Add(scoreEntry);
			}
		}
	}

	protected override bool CanPushNow(BasePlayer pusher)
	{
		return false;
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
	}

	public void NearbyClientMessage(string msg)
	{
		if (networkTrigger.entityContents == null)
		{
			return;
		}
		foreach (BaseEntity entityContent in networkTrigger.entityContents)
		{
			if (entityContent is BasePlayer target)
			{
				ClientRPC(RpcTarget.Player(msg, target));
			}
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void DestroyMessageFromHost(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if ((Object)(object)player == (Object)null || (Object)(object)GetDriver() != (Object)(object)player || networkTrigger.entityContents == null)
		{
			return;
		}
		uint arg = msg.read.UInt32();
		foreach (BaseEntity entityContent in networkTrigger.entityContents)
		{
			if (entityContent is BasePlayer target)
			{
				ClientRPC(RpcTarget.Player("DestroyEntity", target), arg);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(7uL)]
	public void BroadcastEntityMessage(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if ((Object)(object)player == (Object)null || (Object)(object)GetDriver() != (Object)(object)player || networkTrigger.entityContents == null)
		{
			return;
		}
		uint arg = msg.read.UInt32();
		string arg2 = msg.read.String();
		foreach (BaseEntity entityContent in networkTrigger.entityContents)
		{
			if (entityContent is BasePlayer target)
			{
				ClientRPC(RpcTarget.Player("GetEntityMessage", target), arg, arg2);
			}
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server.InputValidation(new Type[] { typeof(ArcadeGame) })]
	[RPC_Server.MaxRepeatedElements(64)]
	[RPC_Server.CallsPerSecond(30uL)]
	[RPC_Server]
	public void GetSnapshotFromClient(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if ((Object)(object)player == (Object)null || (Object)(object)player != (Object)(object)GetDriver())
		{
			return;
		}
		ArcadeGame val = msg.read.Proto<ArcadeGame>((ArcadeGame)null);
		try
		{
			if (networkTrigger.entityContents == null)
			{
				return;
			}
			foreach (BaseEntity entityContent in networkTrigger.entityContents)
			{
				if (entityContent is BasePlayer target)
				{
					ClientRPC(RpcTarget.Player("GetSnapshotFromServer", target), val);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}

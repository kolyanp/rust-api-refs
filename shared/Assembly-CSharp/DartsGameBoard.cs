using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class DartsGameBoard : BaseCombatEntity
{
	public enum GameType
	{
		None,
		SinglePlayer,
		Multiplayer
	}

	[Header("Dependencies")]
	public GameObjectRef ReticlePrefab;

	public GameObjectRef DartPrefab;

	public GameObjectRef UIPrefab;

	[Header("References")]
	public List<DartsGameScoreDisplayUI> scoreDisplayUIs;

	public GameObjectRef winEffect;

	public ParticleSystemContainer bullseyeEffect;

	public SoundPlayer bullseyeEffectSound;

	[Header("Board Setup Parameters")]
	public Transform center;

	[Tooltip("Only used when determining rotation of darts loaded in for clients not playing the game")]
	public Transform simulatedThrowPoint;

	public float radius = 1f;

	public float bandSize = 0.1f;

	public float tripleBandRadiusOffset = 0.1f;

	public float bullseyeRadius = 0.1f;

	public float bullRadius = 0.1f;

	public float angleOffset = -1f;

	[Header("Gizmos")]
	public bool showGizmos;

	[Header("Throwing Reticle Options")]
	public AnimationCurve accuracyCurve = AnimationCurve.Constant(0f, 1f, 1f);

	public List<DartsGameLeaderboardEntry> Leaderboard;

	public static readonly int[] ScoreSlices;

	public static readonly int BullScore;

	private bool _disposed;

	public IDartsGameController GameController;

	private SinglePlayerDartsGameController _singlePlayerDartsGameController;

	private MultiplayerDartsGameController _multiplayerDartsGameController;

	private EntityRef<DartsGameMountable> mountableRef;

	private DartsGameMountable dgm;

	[HideInInspector]
	public DartsGameReticle Reticle;

	private static Vector3 DartSyncResetPosition;

	private int lastUsedDart;

	private Vector3 __sync_dartPosition0;

	private Vector3 __sync_dartPosition1;

	private Vector3 __sync_dartPosition2;

	public int scoreTarget => DartsGame.scoreTarget;

	public float cooldownBetweenThrows => DartsGame.cooldownBetweenThrows;

	public float holdFocusDuration => DartsGame.holdFocusDuration;

	public float reticleSpawnPointRadiusOffset => DartsGame.reticleSpawnPointRadiusOffset;

	public float reticleSpawnPointRadius => radius + reticleSpawnPointRadiusOffset;

	public float RadiusWithoutBands => radius - bullRadius - bandSize * 2f;

	public float EmptySpaceLength => RadiusWithoutBands / 2f;

	public GameType gameType { get; private set; }

	public bool isPlaying => gameType > GameType.None;

	public bool isSinglePlayer => gameType == GameType.SinglePlayer;

	public bool isMultiPlayer => gameType == GameType.Multiplayer;

	public DartsGameMountable mountable
	{
		get
		{
			if ((Object)(object)dgm == (Object)null)
			{
				dgm = mountableRef.Get(base.isServer);
			}
			return dgm;
		}
	}

	[Sync(Autosave = false, RequireChange = false)]
	public Vector3 dartPosition0
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return __sync_dartPosition0;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			__sync_dartPosition0 = value;
			byte nameID = __GetWeaverID("dartPosition0");
			QueueSyncVar(nameID);
		}
	}

	[Sync(Autosave = false, RequireChange = false)]
	public Vector3 dartPosition1
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return __sync_dartPosition1;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			__sync_dartPosition1 = value;
			byte nameID = __GetWeaverID("dartPosition1");
			QueueSyncVar(nameID);
		}
	}

	[Sync(Autosave = false, RequireChange = false)]
	public Vector3 dartPosition2
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return __sync_dartPosition2;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			__sync_dartPosition2 = value;
			byte nameID = __GetWeaverID("dartPosition2");
			QueueSyncVar(nameID);
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("DartsGameBoard.OnRpcMessage"))
		{
			if (rpc == 112085967 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_EndGame"));
				}
				using (TimeWarning.New("RPC_EndGame"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(112085967u, "RPC_EndGame", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(112085967u, "RPC_EndGame", this, player, 3f))
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
							RPC_EndGame(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_EndGame");
					}
				}
				return true;
			}
			if (rpc == 1898904248 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ReceiveDartHit"));
				}
				using (TimeWarning.New("RPC_ReceiveDartHit"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1898904248u, "RPC_ReceiveDartHit", this, player, 1uL))
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
							RPC_ReceiveDartHit(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_ReceiveDartHit");
					}
				}
				return true;
			}
			if (rpc == 3181726187u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ReceiveDartThrow"));
				}
				using (TimeWarning.New("RPC_ReceiveDartThrow"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3181726187u, "RPC_ReceiveDartThrow", this, player, 1uL))
						{
							return true;
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
							RPC_ReceiveDartThrow(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RPC_ReceiveDartThrow");
					}
				}
				return true;
			}
			if (rpc == 488834035 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_StartMultiplayerGame"));
				}
				using (TimeWarning.New("RPC_StartMultiplayerGame"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(488834035u, "RPC_StartMultiplayerGame", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(488834035u, "RPC_StartMultiplayerGame", this, player, 3f))
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
							RPC_StartMultiplayerGame(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in RPC_StartMultiplayerGame");
					}
				}
				return true;
			}
			if (rpc == 405074458 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_StartSinglePlayerGame"));
				}
				using (TimeWarning.New("RPC_StartSinglePlayerGame"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(405074458u, "RPC_StartSinglePlayerGame", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(405074458u, "RPC_StartSinglePlayerGame", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg6 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_StartSinglePlayerGame(msg6);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in RPC_StartSinglePlayerGame");
					}
				}
				return true;
			}
			if (rpc == 1532838903 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_UpdateThrowTimer"));
				}
				using (TimeWarning.New("RPC_UpdateThrowTimer"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1532838903u, "RPC_UpdateThrowTimer", this, player, 1uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg7 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_UpdateThrowTimer(msg7);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
						player.Kick("RPC Error in RPC_UpdateThrowTimer");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void DestroyShared()
	{
		if (!_disposed)
		{
			_disposed = true;
			base.DestroyShared();
			EndGame();
		}
	}

	public void EndGame()
	{
		if (GameController != null)
		{
			if ((Object)(object)mountable != (Object)null)
			{
				mountable.DismountAllPlayers();
			}
			gameType = GameType.None;
			GameController.ForceLeaveGame();
			NewTurn();
			GameController.Dispose();
			GameController = null;
			_singlePlayerDartsGameController = null;
			_multiplayerDartsGameController = null;
			SendNetworkUpdate();
		}
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		DartsGameMountable entity = default(DartsGameMountable);
		if (((Component)child).TryGetComponent<DartsGameMountable>(ref entity))
		{
			mountableRef.Set(entity);
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (info.msg.dartsGameLeaderboard == null)
		{
			info.msg.dartsGameLeaderboard = Pool.Get<DartsGameLeaderboard>();
		}
		info.msg.dartsGameLeaderboard.entries = Pool.Get<List<DartsGameLeaderboardEntry>>();
		foreach (DartsGameLeaderboardEntry item in Leaderboard)
		{
			DartsGameLeaderboardEntry val = Pool.Get<DartsGameLeaderboardEntry>();
			val.userid = item.userid;
			val.playerName = item.playerName;
			val.dartsThrown = item.dartsThrown;
			val.timeTaken = item.timeTaken;
			info.msg.dartsGameLeaderboard.entries.Add(val);
		}
		if (!info.forDisk)
		{
			if (info.msg.dartsGame == null)
			{
				info.msg.dartsGame = Pool.Get<DartsGame>();
			}
			info.msg.dartsGame.mountableId = mountableRef.uid;
			info.msg.dartsGame.players = Pool.Get<List<DartsPlayerData>>();
			info.msg.dartsGame.gameType = (int)gameType;
			if (GameController != null)
			{
				GameController.Save(info.msg.dartsGame);
			}
			else
			{
				info.msg.dartsGame.state = 0;
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.dartsGameLeaderboard != null)
		{
			Leaderboard = info.msg.dartsGameLeaderboard.entries.ToList();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		Leaderboard = new List<DartsGameLeaderboardEntry>();
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	public void RPC_StartSinglePlayerGame(RPCMessage msg)
	{
		if (GameController == null)
		{
			gameType = GameType.SinglePlayer;
			_singlePlayerDartsGameController = new SinglePlayerDartsGameController(this);
			GameController = _singlePlayerDartsGameController;
			GameController.JoinGame(msg.player);
			GameController.StartPreGame();
			SendNetworkUpdate();
		}
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_StartMultiplayerGame(RPCMessage msg)
	{
		if (GameController == null)
		{
			gameType = GameType.Multiplayer;
			_multiplayerDartsGameController = new MultiplayerDartsGameController(this);
			GameController = _multiplayerDartsGameController;
			GameController.StartPreGame();
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_EndGame(RPCMessage msg)
	{
		EndGame();
	}

	public void NewTurn(bool switchedPlayers = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		dartPosition0 = DartSyncResetPosition;
		dartPosition1 = DartSyncResetPosition;
		dartPosition2 = DartSyncResetPosition;
		lastUsedDart = 0;
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	public void RPC_ReceiveDartThrow(RPCMessage msg)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)msg.player == (Object)null) && GameController != null && GameController.IsGameOngoing && GameController.IsPlayersTurn(msg.player) && GameController.IsAtBoard(msg.player))
		{
			Vector3 dartThrowSyncVar = msg.read.Vector3();
			SetDartThrowSyncVar(dartThrowSyncVar);
		}
	}

	private void SetDartThrowSyncVar(Vector3 aimLocation)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		switch (lastUsedDart)
		{
		case 0:
			DartsDebug($"[DartsGameBoard] Server setting SyncVar dartPosition0 to {aimLocation}");
			dartPosition0 = aimLocation;
			lastUsedDart++;
			break;
		case 1:
			DartsDebug($"[DartsGameBoard] Server setting SyncVar dartPosition1 to {aimLocation}");
			dartPosition1 = aimLocation;
			lastUsedDart++;
			break;
		case 2:
			DartsDebug($"[DartsGameBoard] Server setting SyncVar dartPosition2 to {aimLocation}");
			dartPosition2 = aimLocation;
			lastUsedDart++;
			break;
		default:
			lastUsedDart = 0;
			SetDartThrowSyncVar(aimLocation);
			break;
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	public void RPC_UpdateThrowTimer(RPCMessage msg)
	{
		float timeTaken = msg.read.Float();
		if (GameController != null)
		{
			GameController.ServerReceivedUpdatedTimer(msg.player, timeTaken);
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	public void RPC_ReceiveDartHit(RPCMessage msg)
	{
		int points = msg.read.Int32();
		int pointsModifier = msg.read.Int32();
		GameController.ServerReceivedPlayerDartThrow(msg.player, points, pointsModifier);
	}

	public void SendBullseye()
	{
		ClientRPC(RpcTarget.NetworkGroup("PlayBullseyeEffect"));
	}

	public Vector3 WorldToLocalDartPosition(Vector3 worldPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return center.InverseTransformPoint(worldPosition);
	}

	public Vector3 LocalToWorldDartPosition(Vector3 localPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return center.TransformPoint(localPosition);
	}

	public (int pointSlice, int pointModifier) GetBoardScoreFromPosition(Vector3 worldPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = center.InverseTransformPoint(worldPosition);
		((Vector3)(ref val))._002Ector(0f - val.x, val.y, 0f);
		float num = Mathf.Atan2(val.y, val.x) * 57.29578f + angleOffset;
		if (num < 0f)
		{
			num += 360f;
		}
		int num2 = Mathf.RoundToInt(num / 18f) % 20;
		float magnitude = ((Vector3)(ref val)).magnitude;
		DartsDebug(string.Format("[DartsGameBoard] Scoring dart hit. Local Position: {0}, Angle: {1}, Distance: {2}, Slice Index: {3}", new object[4] { val, num, magnitude, num2 }));
		if (magnitude <= bullseyeRadius)
		{
			DartsDebug("[DartsGameBoard] Hit Bullseye!");
			return (pointSlice: BullScore, pointModifier: 2);
		}
		if (magnitude <= bullRadius)
		{
			DartsDebug("[DartsGameBoard] Hit Bull!");
			return (pointSlice: BullScore, pointModifier: 1);
		}
		if (magnitude <= bullRadius + EmptySpaceLength + tripleBandRadiusOffset)
		{
			DartsDebug($"[DartsGameBoard] Hit Single {ScoreSlices[num2]}");
			return (pointSlice: ScoreSlices[num2], pointModifier: 1);
		}
		if (magnitude <= bullRadius + bandSize + EmptySpaceLength + tripleBandRadiusOffset)
		{
			DartsDebug($"[DartsGameBoard] Hit Triple {ScoreSlices[num2]}");
			return (pointSlice: ScoreSlices[num2], pointModifier: 3);
		}
		if (magnitude <= bullRadius + bandSize + EmptySpaceLength * 2f)
		{
			DartsDebug($"[DartsGameBoard] Hit Single {ScoreSlices[num2]}");
			return (pointSlice: ScoreSlices[num2], pointModifier: 1);
		}
		if (magnitude <= bullRadius + bandSize * 2f + EmptySpaceLength * 2f)
		{
			DartsDebug($"[DartsGameBoard] Hit Double {ScoreSlices[num2]}");
			return (pointSlice: ScoreSlices[num2], pointModifier: 2);
		}
		return (pointSlice: 0, pointModifier: 0);
	}

	public int GetGameScore(DartsPlayerData playerData)
	{
		return scoreTarget - playerData.Score;
	}

	public int GetGameScore(int score)
	{
		return scoreTarget - score;
	}

	public int GetGameScoreWithTurn(DartsPlayerData playerData)
	{
		if (playerData.State != DartsPlayerData.DartsPlayerState.InGame)
		{
			return scoreTarget - playerData.Score;
		}
		return scoreTarget - playerData.Score - playerData.ScoreThisTurn;
	}

	[HideInCallstack]
	public void DartsDebug(string message)
	{
	}

	[HideInCallstack]
	public void DartsDebugLeaderboard()
	{
	}

	protected unsafe override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 0:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: dartPosition0 for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite<Vector3>(writer, __sync_dartPosition0);
			return true;
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: dartPosition1 for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite<Vector3>(writer, __sync_dartPosition1);
			return true;
		case 2:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: dartPosition2 for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite<Vector3>(writer, __sync_dartPosition2);
			return true;
		default:
			return base.WriteSyncVar(id, writer);
		}
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 0:
			try
			{
				_ = __sync_dartPosition0;
				Vector3 _sync_dartPosition2 = reader.Vector3();
				__sync_dartPosition0 = _sync_dartPosition2;
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_dartPosition1;
				Vector3 _sync_dartPosition3 = reader.Vector3();
				__sync_dartPosition1 = _sync_dartPosition3;
			}
			catch (Exception ex3)
			{
				Debug.LogException(ex3);
			}
			return true;
		case 2:
			try
			{
				_ = __sync_dartPosition2;
				Vector3 _sync_dartPosition = reader.Vector3();
				__sync_dartPosition2 = _sync_dartPosition;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		default:
			return base.OnSyncVar(id, reader, fromAutoSave);
		}
	}

	private byte __GetWeaverID(string propertyName)
	{
		return propertyName switch
		{
			"dartPosition0" => 0, 
			"dartPosition1" => 1, 
			"dartPosition2" => 2, 
			_ => byte.MaxValue, 
		};
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		return true;
	}

	protected override void ResetSyncVars()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		base.ResetSyncVars();
		__sync_dartPosition0 = default(Vector3);
		__sync_dartPosition1 = default(Vector3);
		__sync_dartPosition2 = default(Vector3);
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			0 => true, 
			1 => true, 
			2 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}

	static DartsGameBoard()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		ScoreSlices = new int[20]
		{
			6, 13, 4, 18, 1, 20, 5, 12, 9, 14,
			11, 8, 16, 7, 19, 3, 17, 2, 15, 10
		};
		BullScore = 25;
		DartSyncResetPosition = Vector3.one * -999f;
	}
}

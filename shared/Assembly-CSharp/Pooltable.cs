using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using PoolPhysics;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class Pooltable : BaseCombatEntity
{
	private readonly Dictionary<ulong, BaseEntity> playerMountables;

	private TimeSince timeSinceLastMove;

	[SerializeField]
	[Header("Shared")]
	private float ballRadius;

	[SerializeField]
	private float tableWidth;

	[SerializeField]
	private float tableHeight;

	[SerializeField]
	private float pocketRadius;

	[SerializeField]
	private float mouthWidth;

	[SerializeField]
	private float cueBallStartX;

	[SerializeField]
	private WorldSpline worldSpline;

	[Tooltip("Fraction of the gap between the walking spline and the table edge to close, so players stand the same bit closer everywhere on the loop.")]
	[Range(0f, 0.75f)]
	[SerializeField]
	private float splineTableCloseness;

	[Tooltip("Block walking the mountable into geometry (e.g. an adjacent boat's hull). Turn off to restore pre-check behaviour.")]
	[SerializeField]
	private bool runWalkClippingChecks;

	[Tooltip("Player body volume tested at each candidate walk pose, in MOUNTABLE space: origin is the pulled spline point, +z points at the cue ball, y=0 is 1m above the player's feet.")]
	[SerializeField]
	private Bounds walkAreaCheck;

	[SerializeField]
	[Header("Server")]
	private GameObjectRef mountableRef;

	[SerializeField]
	private GameObjectRef winEffect;

	[Header("Client")]
	[SerializeField]
	private List<GameObject> clientRenderingPoolBalls;

	[SerializeField]
	private Transform ballParent;

	[SerializeField]
	private GameObjectRef poolTableUIRef;

	[SerializeField]
	private SoundDefinition cueStrikeSound;

	[SerializeField]
	private SoundDefinition ballCollisionSound;

	[SerializeField]
	private SoundDefinition ballBumperCollisionSound;

	[SerializeField]
	private SoundDefinition ballPocketSound;

	[SerializeField]
	private GameObjectRef resetGameEffect;

	[SerializeField]
	private Vector2 ballCollisionSpeedRange;

	[SerializeField]
	private float ballCollisionSoundInterval;

	[SerializeField]
	[Tooltip("All pocketed balls spawn a fake visual at the start of this path and follow it into the basket.")]
	[Header("Ball Return")]
	private WorldSpline ballReturnPath;

	[SerializeField]
	[Tooltip("Preplaced basket balls enabled in order as balls arrive, independent of ball ID.")]
	private GameObject[] basketBalls;

	[SerializeField]
	private float ballReturnSpeed;

	[SerializeField]
	private float eyeOverrideBehindCueBallOffset;

	[SerializeField]
	private float eyeOverrideHeightOffset;

	protected const Flags Flag_IdleResettable = Flags.Reserved1;

	[ReplicatedVar]
	public static bool debug_pool;

	[ReplicatedVar]
	public static float physics_update_rate;

	[ServerVar(Saved = true, Help = "Show pool game tooltip notifications")]
	public static bool show_tooltips;

	[ServerVar(Help = "(Generated) Anyone can reset a pool game nobody has interacted with for this many seconds")]
	public static float idle_reset_seconds;

	[ServerVar(Help = "(Generated) Seconds the shooter stays seated watching their shot before being dismounted")]
	public static float watch_after_shot_seconds;

	private Engine physicsEngine;

	private PoolTableGameController gameController;

	private static readonly Phrase ProcessingTurnPhrase;

	private static readonly Phrase WaitingForPlayerPhrase;

	private const float MinShotForce = 1f;

	private const float MaxShotForce = 8f;

	private bool wasMovingLastTick;

	private double lastPhysicsTime;

	private const int MaxCatchUpTicks = 32;

	private const float TableBoundSoftness = 0.15f;

	private const int WalkCheckMask = 1235298561;

	private const float WalkCheckStep = 0.15f;

	private const int WalkCheckMaxSamples = 24;

	private const float WalkCheckScanStep = 0.25f;

	private static readonly int[][] RackRows;

	private float PhysicsRate => 1f / physics_update_rate;

	private Vector2 CueBallStartPosition
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return new Vector2(cueBallStartX, 0f);
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Pooltable.OnRpcMessage"))
		{
			if (rpc == 1237563035 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_JoinGame"));
				}
				using (TimeWarning.New("RPC_JoinGame"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1237563035u, "RPC_JoinGame", this, player, 3f))
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
							RPC_JoinGame(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_JoinGame");
					}
				}
				return true;
			}
			if (rpc == 985964862 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_RequestCancelGame"));
				}
				using (TimeWarning.New("RPC_RequestCancelGame"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(985964862u, "RPC_RequestCancelGame", this, player, 3f))
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
							RPC_RequestCancelGame(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_RequestCancelGame");
					}
				}
				return true;
			}
			if (rpc == 1316133824 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_RequestMount"));
				}
				using (TimeWarning.New("RPC_RequestMount"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1316133824u, "RPC_RequestMount", this, player, 3f))
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
							RPC_RequestMount(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RPC_RequestMount");
					}
				}
				return true;
			}
			if (rpc == 170667224 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_RequestResetGame"));
				}
				using (TimeWarning.New("RPC_RequestResetGame"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(170667224u, "RPC_RequestResetGame", this, player, 3f))
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
							RPC_RequestResetGame(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in RPC_RequestResetGame");
					}
				}
				return true;
			}
			if (rpc == 1741528195 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_RequestShoot"));
				}
				using (TimeWarning.New("RPC_RequestShoot"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1741528195u, "RPC_RequestShoot", this, player, 3f))
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
							RPC_RequestShoot(msg6);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in RPC_RequestShoot");
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
						if (!RPC_Server.IsVisible.Test(488834035u, "RPC_StartMultiplayerGame", this, player, 3f))
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
							RPC_StartMultiplayerGame(msg7);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
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
						if (!RPC_Server.IsVisible.Test(405074458u, "RPC_StartSinglePlayerGame", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg8 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_StartSinglePlayerGame(msg8);
						}
					}
					catch (Exception ex7)
					{
						Debug.LogException(ex7);
						player.Kick("RPC Error in RPC_StartSinglePlayerGame");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRepeating(UpdateIdleResettable, 1f, 1f);
	}

	private void UpdateIdleResettable()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		bool b = gameController != null && gameController.HasGame && TimeSince.op_Implicit(timeSinceLastMove) > idle_reset_seconds;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b);
	}

	public override void Save(SaveInfo info)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Pooltable.Save"))
		{
			base.Save(info);
			if (!base.isServer || info.forDisk)
			{
				return;
			}
			info.msg.Pooltable = Pool.Get<Pooltable>();
			info.msg.Pooltable.poolBalls = Pool.Get<List<PoolBallData>>();
			if (physicsEngine != null && physicsEngine.IsReady)
			{
				info.msg.Pooltable.poolBalls.Clear();
				Enumerator<PoolPhysics.Data.Ball> enumerator = physicsEngine.Balls.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						PoolPhysics.Data.Ball current = enumerator.Current;
						PoolBallData val = Pool.Get<PoolBallData>();
						val.position = Vector2.op_Implicit(current.Position);
						val.velocity = Vector2.op_Implicit(current.Velocity);
						val.pocketed = current.IsKinematic;
						info.msg.Pooltable.poolBalls.Add(val);
					}
				}
				finally
				{
					((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
				}
			}
			gameController?.Save(info.msg.Pooltable);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_StartSinglePlayerGame(RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null) && msg.player.CanInteract())
		{
			BeginGame(msg.player, solo: true);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_StartMultiplayerGame(RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null) && msg.player.CanInteract())
		{
			BeginGame(msg.player, solo: false);
		}
	}

	private void BeginGame(BasePlayer player, bool solo)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)player == (Object)null) && (gameController == null || !gameController.HasGame))
		{
			if (gameController == null)
			{
				gameController = new PoolTableGameController(this);
			}
			gameController.StartNewGame(player.userID, solo);
			timeSinceLastMove = TimeSince.op_Implicit(0f);
			if (solo)
			{
				MountPlayerAtTable(player);
			}
		}
	}

	public void PlayWinEffect()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (winEffect.isValid)
		{
			Effect.server.Run(winEffect.resourcePath, ((Component)this).transform.position, Vector3.up);
		}
	}

	public bool CanPlayerMove(ulong playerId)
	{
		if (gameController != null && gameController.State == PoolTableGameController.GameState.WaitingForShot)
		{
			return gameController.CurrentPlayerId == playerId;
		}
		return false;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_JoinGame(RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null) && msg.player.CanInteract() && gameController != null)
		{
			gameController.AddSecondPlayer(msg.player.userID);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_RequestMount(RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null) && msg.player.CanInteract() && gameController != null && gameController.HasGame && gameController.CanMount(msg.player.userID))
		{
			MountPlayerAtTable(msg.player);
		}
	}

	private void MountPlayerAtTable(BasePlayer player)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)worldSpline == (Object)null) && (physicsEngine == null || !physicsEngine.HasMovingBalls()) && playerMountables.Count <= 0)
		{
			CancelInvoke(DismountAllSeatedPlayers);
			worldSpline.GetClosestPointWorld(((Component)player).transform.position, out var distanceOnSpline);
			if (TryFindFreeSplineDistance(distanceOnSpline, out distanceOnSpline) && TryGetSplinePose(distanceOnSpline, out var pos, out var rot))
			{
				BaseEntity baseEntity = GameManager.server.CreateEntity(mountableRef.resourcePath, pos, rot);
				baseEntity.SetParent(this, worldPositionStays: true);
				baseEntity.Spawn();
				playerMountables[player.userID] = baseEntity;
				PooltableMountable component = ((Component)baseEntity).GetComponent<PooltableMountable>();
				component.SplineDistance = distanceOnSpline;
				component.MountPlayer(player);
				ClientRPC(RpcTarget.Player("RPC_OpenPoolUI", player));
				gameController?.OnPlayerJoined(player.userID);
				timeSinceLastMove = TimeSince.op_Implicit(0f);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_RequestShoot(RPCMessage msg)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)msg.player == (Object)null || IsInvoking(DismountAllSeatedPlayers) || gameController == null || !gameController.CanShoot(msg.player.userID))
		{
			return;
		}
		Vector3 val = msg.read.Vector3();
		float num = msg.read.Float();
		if (!Vector3Ex.IsNaNOrInfinity(val) && !FloatEx.IsNaNOrInfinity(num))
		{
			val.y = 0f;
			val = Vector3.ClampMagnitude(val, 1f);
			num = Mathf.Clamp(num, 1f, 8f);
			if (!(((Vector3)(ref val)).sqrMagnitude <= Mathf.Epsilon))
			{
				gameController.OnShotFired();
				timeSinceLastMove = TimeSince.op_Implicit(0f);
				ApplyShotShared(val, num);
				ClientRPC(RpcTarget.NetworkGroup("ClientOnShotFired"), val, num, gameController.ShotId);
				SendNetworkUpdateImmediate();
			}
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_RequestResetGame(RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null) && CanResetGame(msg.player))
		{
			ResetGameState();
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void RPC_RequestCancelGame(RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null) && CanCancelGame(msg.player))
		{
			ResetGameState();
		}
	}

	private void ResetGameState()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		CancelInvoke(DismountAllSeatedPlayers);
		DismountAllSeatedPlayers();
		ResetBallsToRack();
		if (gameController != null)
		{
			gameController.ResetToInitialState();
		}
		else
		{
			gameController = new PoolTableGameController(this);
			gameController.ResetToInitialState();
		}
		SendNetworkUpdateImmediate();
		if (resetGameEffect.isValid)
		{
			Effect.server.Run(resetGameEffect.resourcePath, ((Component)this).transform.position, Vector3.up);
		}
	}

	public void OnMountablePlayerLeft(ulong playerId)
	{
		if (playerMountables.TryGetValue(playerId, out var value))
		{
			playerMountables.Remove(playerId);
			if ((Object)(object)value != (Object)null && !value.IsDestroyed)
			{
				value.Kill();
			}
		}
	}

	private void DismountAllSeatedPlayers()
	{
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			((List<BaseEntity>)(object)val).AddRange((IEnumerable<BaseEntity>)playerMountables.Values);
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				if ((Object)(object)item != (Object)null && !item.IsDestroyed)
				{
					(item as PooltableMountable).DismountAllPlayers();
				}
			}
			playerMountables.Clear();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override void InitShared()
	{
		base.InitShared();
		if (physicsEngine == null)
		{
			physicsEngine = Pool.Get<Engine>();
		}
		gameController = new PoolTableGameController(this);
		SetupTable();
		StandardPoolBallSetup();
		if (physicsEngine != null)
		{
			Engine engine = physicsEngine;
			engine.OnBallPocketed = (Action<int>)Delegate.Combine(engine.OnBallPocketed, new Action<int>(OnBallPocketed));
		}
		lastPhysicsTime = Time.timeAsDouble;
		InvokeRepeating(PhysicsTick, 0f, PhysicsRate);
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		if (IsInvoking(PhysicsTick))
		{
			CancelInvoke(PhysicsTick);
		}
		if (physicsEngine != null)
		{
			Engine engine = physicsEngine;
			engine.OnBallPocketed = (Action<int>)Delegate.Remove(engine.OnBallPocketed, new Action<int>(OnBallPocketed));
		}
		if (physicsEngine != null)
		{
			Pool.Free<Engine>(ref physicsEngine);
		}
		gameController = null;
		if (IsInvoking(UpdateIdleResettable))
		{
			CancelInvoke(UpdateIdleResettable);
		}
		if (IsInvoking(DismountAllSeatedPlayers))
		{
			CancelInvoke(DismountAllSeatedPlayers);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		Pooltable pooltable = info.msg.Pooltable;
		if (pooltable != null && base.isServer)
		{
			gameController?.Load(pooltable);
		}
	}

	public bool IsBallPocketed(int ballId)
	{
		if (physicsEngine == null || !physicsEngine.IsReady)
		{
			return false;
		}
		return physicsEngine.Balls[ballId].IsKinematic;
	}

	public void RespotCueBallAfterFoul()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (physicsEngine != null && physicsEngine.IsReady && physicsEngine.Balls.Count > 0)
		{
			physicsEngine.SetBallPosition(0, CueBallStartPosition);
			physicsEngine.SetBallVelocity(0, Vector2.zero);
			physicsEngine.SetBallIsKinematic(0, isKinematic: false);
			DebugPool("RespotCueBallAfterFoul");
			if (base.isServer)
			{
				SendNetworkUpdateImmediate();
			}
		}
	}

	public bool TryGetCueBallWorldPosition(out Vector3 worldPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		worldPosition = Vector3.zero;
		if (physicsEngine == null || !physicsEngine.IsReady)
		{
			return false;
		}
		if (physicsEngine.Balls.Count <= 0)
		{
			return false;
		}
		PoolPhysics.Data.Ball ball = physicsEngine.Balls[0];
		if (ball.IsKinematic)
		{
			return false;
		}
		worldPosition = ((Component)this).transform.TransformPoint(new Vector3(ball.Position.x, 0f, ball.Position.y));
		return true;
	}

	public Vector3 GetShotLineEndPoint(Vector3 worldOrigin, Vector3 worldDirection, float maxDistance, int ignoreBallId = -1, bool includeWalls = true)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		if (maxDistance <= 0f)
		{
			return worldOrigin;
		}
		Vector3 val = worldDirection;
		val.y = 0f;
		if (((Vector3)(ref val)).sqrMagnitude <= Mathf.Epsilon)
		{
			return worldOrigin;
		}
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		if (physicsEngine == null || !physicsEngine.IsReady)
		{
			return worldOrigin + normalized * maxDistance;
		}
		Vector3 val2 = ((Component)this).transform.InverseTransformPoint(worldOrigin);
		Vector3 val3 = ((Component)this).transform.InverseTransformDirection(normalized);
		val3.y = 0f;
		Vector2 val4 = default(Vector2);
		((Vector2)(ref val4))._002Ector(val3.x, val3.z);
		if (((Vector2)(ref val4)).sqrMagnitude <= Mathf.Epsilon)
		{
			return worldOrigin;
		}
		((Vector2)(ref val4)).Normalize();
		Vector2 val5 = default(Vector2);
		((Vector2)(ref val5))._002Ector(val2.x, val2.z);
		if (physicsEngine.Raycast(val5, val4, maxDistance, out var hit, ignoreBallId, includeBalls: true, includeWalls))
		{
			return ((Component)this).transform.TransformPoint(new Vector3(hit.Point.x, 0f, hit.Point.y));
		}
		Vector2 val6 = val5 + val4 * maxDistance;
		return ((Component)this).transform.TransformPoint(new Vector3(val6.x, 0f, val6.y));
	}

	public Vector3 GetDirToCueBall(Vector3 pos)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		Vector2 position = physicsEngine.Balls[0].Position;
		Vector3 val = ((Component)this).transform.TransformPoint(new Vector3(position.x, 0f, position.y)) - pos;
		val.y = 0f;
		if (((Vector3)(ref val)).sqrMagnitude <= Mathf.Epsilon)
		{
			return ((Component)this).transform.forward;
		}
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		normalized.y = 0f;
		return normalized;
	}

	public float GetSplineDistanceForPosition(Vector3 worldPos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		worldSpline.GetClosestPointWorld(worldPos, out var distanceOnSpline);
		return distanceOnSpline;
	}

	public void MoveMountableAlongSpline(PooltableMountable mountable, float movement)
	{
		float length = worldSpline.GetData().Length;
		if (!(length <= 0f))
		{
			movement = ClampWalkDistance(mountable, movement, length);
			if (!(Mathf.Abs(movement) <= Mathf.Epsilon))
			{
				mountable.SplineDistance = Mathf.Repeat(mountable.SplineDistance + movement, length);
				PositionMountableOnSpline(mountable);
			}
		}
	}

	public void MoveMountableTowardSplineDistance(PooltableMountable mountable, float targetDistance, float maxWalkDistance)
	{
		float length = worldSpline.GetData().Length;
		if (!(length <= 0f))
		{
			float num = Mathf.Repeat(targetDistance - mountable.SplineDistance, length);
			if (num > length * 0.5f)
			{
				num -= length;
			}
			num = Mathf.Clamp(num, 0f - maxWalkDistance, maxWalkDistance);
			num = ClampWalkDistance(mountable, num, length);
			if (!(Mathf.Abs(num) <= Mathf.Epsilon))
			{
				mountable.SplineDistance = Mathf.Repeat(mountable.SplineDistance + num, length);
				PositionMountableOnSpline(mountable);
			}
		}
	}

	private static float SoftPositive(float t)
	{
		return (t + Mathf.Sqrt(t * t + 0.0225f)) * 0.5f;
	}

	private static float PullAxisTowardBound(float v, float halfExtent, float closeness)
	{
		return v - (SoftPositive(v - halfExtent) - SoftPositive(0f - halfExtent - v)) * closeness;
	}

	private Vector3 PullSplinePointTowardTable(Vector3 worldPos)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (splineTableCloseness <= 0f)
		{
			return worldPos;
		}
		Vector3 val = ((Component)this).transform.InverseTransformPoint(worldPos);
		val.x = PullAxisTowardBound(val.x, tableWidth * 0.5f, splineTableCloseness);
		val.z = PullAxisTowardBound(val.z, tableHeight * 0.5f, splineTableCloseness);
		return ((Component)this).transform.TransformPoint(val);
	}

	public bool TryGetSplinePose(float splineDistance, out Vector3 pos, out Quaternion rot)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		pos = default(Vector3);
		rot = default(Quaternion);
		if ((Object)(object)worldSpline == (Object)null)
		{
			return false;
		}
		if (physicsEngine == null || !physicsEngine.IsReady)
		{
			return false;
		}
		if (physicsEngine.Balls.Count <= 0)
		{
			return false;
		}
		pos = PullSplinePointTowardTable(worldSpline.GetPointCubicHermiteWorld(splineDistance));
		rot = Quaternion.LookRotation(GetDirToCueBall(pos));
		return true;
	}

	private void PositionMountableOnSpline(PooltableMountable mountable)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetSplinePose(mountable.SplineDistance, out var pos, out var rot))
		{
			((Component)mountable).transform.position = pos;
			((Component)mountable).transform.rotation = rot;
		}
	}

	public bool IsWalkPoseBlocked(Vector3 pos, Quaternion rot)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (!runWalkClippingChecks)
		{
			return false;
		}
		List<Collider> list = Pool.Get<List<Collider>>();
		GamePhysics.OverlapOBB(new OBB(pos, rot, walkAreaCheck), list, 1235298561, (QueryTriggerInteraction)1);
		BaseEntity rootParentEntity = GetRootParentEntity();
		bool result = false;
		foreach (Collider item in list)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(item);
			if ((Object)(object)baseEntity == (Object)null)
			{
				result = true;
				break;
			}
			if (baseEntity.isServer == base.isServer && !((Object)(object)baseEntity.GetRootParentEntity() == (Object)(object)rootParentEntity))
			{
				result = true;
				break;
			}
		}
		Pool.FreeUnmanaged<Collider>(ref list);
		return result;
	}

	private float ClampWalkDistance(PooltableMountable mountable, float delta, float splineLength)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (!runWalkClippingChecks)
		{
			return delta;
		}
		if (Mathf.Abs(delta) <= Mathf.Epsilon)
		{
			return delta;
		}
		float splineDistance = mountable.SplineDistance;
		if (TryGetSplinePose(splineDistance, out var pos, out var rot) && IsWalkPoseBlocked(pos, rot))
		{
			return delta;
		}
		float num = Mathf.Min(Mathf.Abs(delta), 3.6000001f);
		float num2 = Mathf.Sign(delta);
		float num3 = 0f;
		for (int i = 1; i <= 24; i++)
		{
			float num4 = Mathf.Min((float)i * 0.15f, num);
			float splineDistance2 = Mathf.Repeat(splineDistance + num2 * num4, splineLength);
			if (!TryGetSplinePose(splineDistance2, out var pos2, out var rot2) || IsWalkPoseBlocked(pos2, rot2))
			{
				break;
			}
			num3 = num4;
			if (num4 >= num)
			{
				break;
			}
		}
		return num2 * num3;
	}

	public bool TryFindFreeSplineDistance(float preferred, out float result)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		result = preferred;
		if (!runWalkClippingChecks)
		{
			return true;
		}
		float num = (((Object)(object)worldSpline != (Object)null) ? worldSpline.GetData().Length : 0f);
		if (num <= 0f)
		{
			return true;
		}
		if (TryGetSplinePose(preferred, out var pos, out var rot) && !IsWalkPoseBlocked(pos, rot))
		{
			return true;
		}
		int num2 = Mathf.CeilToInt(num * 0.5f / 0.25f);
		for (int i = 1; i <= num2; i++)
		{
			float num3 = (float)i * 0.25f;
			for (int j = 0; j < 2; j++)
			{
				float num4 = Mathf.Repeat(preferred + ((j == 0) ? num3 : (0f - num3)), num);
				if (TryGetSplinePose(num4, out var pos2, out var rot2) && !IsWalkPoseBlocked(pos2, rot2))
				{
					result = num4;
					return true;
				}
			}
		}
		return false;
	}

	private void ApplyShotShared(Vector3 dir, float force)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		force = Mathf.Clamp(force, 0f, 10f);
		Vector2 force2 = new Vector2(dir.x, dir.z) * force;
		physicsEngine.ApplyForce(0, force2);
		lastPhysicsTime = Time.timeAsDouble;
	}

	private void PhysicsTick()
	{
		using (TimeWarning.New("Pooltable.PhysicsTick"))
		{
			if (physicsEngine == null || !physicsEngine.IsReady)
			{
				return;
			}
			bool flag = physicsEngine.HasMovingBalls();
			if (flag)
			{
				int num = Mathf.Clamp((int)((Time.timeAsDouble - lastPhysicsTime) / (double)PhysicsRate), 0, 32);
				using (TimeWarning.New("Pooltable.PhysicsTick.Simulate"))
				{
					for (int i = 0; i < num; i++)
					{
						physicsEngine.Tick(PhysicsRate);
					}
				}
				lastPhysicsTime += (double)num * (double)PhysicsRate;
				if (Time.timeAsDouble - lastPhysicsTime > (double)(PhysicsRate * 32f))
				{
					lastPhysicsTime = Time.timeAsDouble;
				}
				if (base.isServer && num > 0)
				{
					using (TimeWarning.New("Pooltable.PhysicsTick.SendNetworkUpdate"))
					{
						SendNetworkUpdate();
					}
				}
			}
			else
			{
				lastPhysicsTime = Time.timeAsDouble;
				if (wasMovingLastTick)
				{
					DebugPool("PhysicsTick transition moving->stopped");
					using (TimeWarning.New("Pooltable.PhysicsTick.OnBallsStopped"))
					{
						gameController?.OnBallsStopped();
						if (base.isServer)
						{
							CancelInvoke(DismountAllSeatedPlayers);
							Invoke(DismountAllSeatedPlayers, watch_after_shot_seconds);
						}
					}
				}
			}
			wasMovingLastTick = flag;
		}
	}

	private List<(int id, Vector2 position)> BuildStartingLayout()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		List<(int, Vector2)> list = new List<(int, Vector2)>();
		list.Add((0, CueBallStartPosition));
		float num = ballRadius * 2f + 0.01f;
		float rowSpacingX = num * 0.866f;
		float rowSpacingY = num;
		Vector2 rackOrigin = new Vector2(0.4f, 0f);
		Vector2[] array = (Vector2[])(object)new Vector2[16];
		for (int i = 0; i < RackRows.Length; i++)
		{
			int[] array2 = RackRows[i];
			for (int j = 0; j < array2.Length; j++)
			{
				array[array2[j]] = slot(i, j, array2.Length);
			}
		}
		for (int k = 1; k <= 15; k++)
		{
			list.Add((k, array[k]));
		}
		return list;
		Vector2 slot(int row, int index, int rowCount)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			return rackOrigin + new Vector2(rowSpacingX * (float)row, ((float)index - (float)(rowCount - 1) * 0.5f) * rowSpacingY);
		}
	}

	private void StandardPoolBallSetup()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		foreach (var (id, position) in BuildStartingLayout())
		{
			physicsEngine.AddBall(new PoolPhysics.Data.Ball
			{
				Id = id,
				Position = position,
				Velocity = Vector2.zero,
				Radius = ballRadius,
				IsKinematic = false
			});
		}
		if (base.isServer)
		{
			Invoke(base.SendNetworkUpdateImmediate, 0.1f);
		}
	}

	private void ResetBallsToRack()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (physicsEngine == null || !physicsEngine.IsReady)
		{
			return;
		}
		foreach (var (id, pos) in BuildStartingLayout())
		{
			physicsEngine.SetBallIsKinematic(id, isKinematic: false);
			physicsEngine.SetBallPosition(id, pos);
		}
	}

	private void SetupTable()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		float num = tableWidth / 2f;
		float num2 = tableHeight / 2f;
		float num3 = mouthWidth;
		float num4 = mouthWidth * 0.8f;
		physicsEngine.AddPocket(new PoolPhysics.Data.Pocket
		{
			Position = new Vector2(0f - num, 0f - num2),
			Radius = pocketRadius
		});
		physicsEngine.AddPocket(new PoolPhysics.Data.Pocket
		{
			Position = new Vector2(num, 0f - num2),
			Radius = pocketRadius
		});
		physicsEngine.AddPocket(new PoolPhysics.Data.Pocket
		{
			Position = new Vector2(0f - num, num2),
			Radius = pocketRadius
		});
		physicsEngine.AddPocket(new PoolPhysics.Data.Pocket
		{
			Position = new Vector2(num, num2),
			Radius = pocketRadius
		});
		physicsEngine.AddPocket(new PoolPhysics.Data.Pocket
		{
			Position = new Vector2(0f, 0f - num2),
			Radius = pocketRadius
		});
		physicsEngine.AddPocket(new PoolPhysics.Data.Pocket
		{
			Position = new Vector2(0f, num2),
			Radius = pocketRadius
		});
		physicsEngine.AddWall(new PoolPhysics.Data.Wall
		{
			A = new Vector2(0f - num + num3, 0f - num2),
			B = new Vector2((0f - num4) / 2f, 0f - num2),
			Normal = Vector2.up
		});
		physicsEngine.AddWall(new PoolPhysics.Data.Wall
		{
			A = new Vector2(num4 / 2f, 0f - num2),
			B = new Vector2(num - num3, 0f - num2),
			Normal = Vector2.up
		});
		physicsEngine.AddWall(new PoolPhysics.Data.Wall
		{
			A = new Vector2(0f - num + num3, num2),
			B = new Vector2((0f - num4) / 2f, num2),
			Normal = Vector2.down
		});
		physicsEngine.AddWall(new PoolPhysics.Data.Wall
		{
			A = new Vector2(num4 / 2f, num2),
			B = new Vector2(num - num3, num2),
			Normal = Vector2.down
		});
		physicsEngine.AddWall(new PoolPhysics.Data.Wall
		{
			A = new Vector2(0f - num, 0f - num2 + num3),
			B = new Vector2(0f - num, num2 - num3),
			Normal = Vector2.right
		});
		physicsEngine.AddWall(new PoolPhysics.Data.Wall
		{
			A = new Vector2(num, 0f - num2 + num3),
			B = new Vector2(num, num2 - num3),
			Normal = Vector2.left
		});
	}

	private bool CanResetGame(BasePlayer player)
	{
		if (gameController == null || !gameController.HasGame)
		{
			return false;
		}
		if (gameController.State == PoolTableGameController.GameState.NotPlaying || gameController.State == PoolTableGameController.GameState.WaitingForPlayers || gameController.State == PoolTableGameController.GameState.BallsMoving)
		{
			return false;
		}
		if (gameController.State == PoolTableGameController.GameState.GameOver)
		{
			return true;
		}
		if (!HasFlag(Flags.Reserved1))
		{
			return gameController.IsParticipant(player.userID);
		}
		return true;
	}

	private bool CanCancelGame(BasePlayer player)
	{
		if (gameController != null && gameController.State == PoolTableGameController.GameState.WaitingForPlayers)
		{
			return gameController.IsParticipant(player.userID);
		}
		return false;
	}

	private void OnBallPocketed(int ballId)
	{
		physicsEngine.SetBallIsKinematic(ballId, isKinematic: true);
		gameController?.OnBallPocketed(ballId);
		DebugPool($"OnBallPocketed ballId={ballId}");
		if (base.isServer)
		{
			SendNetworkUpdateImmediate();
		}
	}

	private void DebugPool(string message)
	{
		if (debug_pool)
		{
			string text = ((net != null) ? net.ID.Value.ToString() : "n/a");
			string text2 = ((gameController != null) ? gameController.State.ToString() : "null");
			int num = ((gameController != null) ? gameController.CurrentIndex : (-1));
			uint num2 = ((gameController != null) ? gameController.ShotId : 0u);
			string text3 = "predictedShotId=n/a lastApplied=n/a";
			Debug.Log((object)string.Format("[PoolTable] table={0} gcState={1} currentIndex={2} serverShotId={3} {4} | {5}", new object[6] { text, text2, num, num2, text3, message }));
		}
	}

	public Pooltable()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		playerMountables = new Dictionary<ulong, BaseEntity>();
		tableWidth = 1.2f;
		tableHeight = 0.6f;
		pocketRadius = 0.045f;
		mouthWidth = 0.08f;
		cueBallStartX = -0.545f;
		splineTableCloseness = 0.25f;
		runWalkClippingChecks = true;
		walkAreaCheck = new Bounds(new Vector3(0f, 0f, 0.24f), new Vector3(0.55f, 1.3f, 0.44f));
		ballCollisionSpeedRange = new Vector2(0.1f, 3f);
		ballCollisionSoundInterval = 0.02f;
		ballReturnSpeed = 1.5f;
		eyeOverrideBehindCueBallOffset = 0.6f;
		eyeOverrideHeightOffset = 0.2f;
		base._002Ector();
	}

	static Pooltable()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		debug_pool = false;
		physics_update_rate = 64f;
		show_tooltips = false;
		idle_reset_seconds = 180f;
		watch_after_shot_seconds = 2f;
		ProcessingTurnPhrase = new Phrase("poolprocessing", "Processing turn");
		WaitingForPlayerPhrase = new Phrase("poolwaiting", "Waiting for player");
		RackRows = new int[5][]
		{
			new int[1] { 1 },
			new int[2] { 9, 2 },
			new int[3] { 10, 8, 3 },
			new int[4] { 11, 4, 12, 5 },
			new int[5] { 13, 6, 14, 15, 7 }
		};
	}
}

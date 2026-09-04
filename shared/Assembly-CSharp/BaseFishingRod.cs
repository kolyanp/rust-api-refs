using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseFishingRod : HeldEntity
{
	public class UpdateFishingRod : ObjectWorkQueue<BaseFishingRod>
	{
		protected override void RunJob(BaseFishingRod entity)
		{
			if (((ObjectWorkQueue<BaseFishingRod>)this).ShouldAdd(entity))
			{
				entity.CatchProcessBudgeted();
			}
		}

		protected override bool ShouldAdd(BaseFishingRod entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.IsValid();
			}
			return false;
		}
	}

	public enum CatchState
	{
		None,
		Aiming,
		Waiting,
		Catching,
		Caught
	}

	[Flags]
	public enum FishState
	{
		PullingLeft = 1,
		PullingRight = 2,
		PullingBack = 4
	}

	public enum FailReason
	{
		UserRequested,
		BadAngle,
		TensionBreak,
		Unequipped,
		TimeOut,
		Success,
		NoWaterFound,
		Obstructed,
		NoLure,
		TooShallow,
		TooClose,
		TooFarAway,
		PlayerMoved
	}

	public static UpdateFishingRod updateFishingRodQueue;

	public static Phrase overfishedWarningPhrase;

	private TimeUntil nextFishStateChange;

	private TimeSince fishCatchDuration;

	private float strainTimer;

	private const float strainMax = 6f;

	private TimeSince lastStrainUpdate;

	private TimeUntil catchTime;

	private TimeSince lastSightCheck;

	private Vector3 playerStartPosition;

	private WaterBody surfaceBody;

	private ItemDefinition lureUsed;

	private ItemDefinition currentFishTarget;

	private ItemModFishable fishableModifier;

	private ItemModFishable lastFish;

	public static Dictionary<Vector2, (int, TimeSince)> FishedSpots;

	private bool inQueue;

	[ServerVar(Help = "(Generated) When enabled, all fishing attempts succeed immediately regardless of bite probability; cheat for testing fishing catch logic")]
	public static bool ForceSuccess;

	[ServerVar(Help = "(Generated) When enabled, all fishing attempts fail immediately; cheat for testing failed-catch animations and UI feedback")]
	public static bool ForceFail;

	[ServerVar(Help = "(Generated) When enabled, fish bite the hook immediately after casting without any wait time; cheat for testing bite response")]
	public static bool ImmediateHook;

	public GameObjectRef FishingBobberRef;

	public float FishCatchDistance = 0.5f;

	public LineRenderer ReelLineRenderer;

	public Transform LineRendererWorldStartPos;

	private FishState currentFishState;

	private EntityRef<FishingBobber> currentBobber;

	public float ConditionLossOnSuccess = 0.02f;

	public float ConditionLossOnFail = 0.04f;

	public float GlobalStrainSpeedMultiplier = 1f;

	public float MaxCastDistance = 10f;

	public const Flags Straining = Flags.Reserved1;

	public ItemModFishable ForceFish;

	public static Flags PullingLeftFlag;

	public static Flags PullingRightFlag;

	public static Flags ReelingInFlag;

	public GameObjectRef BobberPreview;

	public SoundDefinition onLineSoundDef;

	public SoundDefinition strainSoundDef;

	public AnimationCurve strainGainCurve;

	public SoundDefinition tensionBreakSoundDef;

	public GameObjectRef OverfishedAreaPrefab;

	public CatchState CurrentState { get; private set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BaseFishingRod.OnRpcMessage"))
		{
			if (rpc == 4237324865u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_Cancel"));
				}
				using (TimeWarning.New("Server_Cancel"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(4237324865u, "Server_Cancel", this, player))
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
							Server_Cancel(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in Server_Cancel");
					}
				}
				return true;
			}
			if (rpc == 2268129697u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_OverfishingCheck"));
				}
				using (TimeWarning.New("Server_OverfishingCheck"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2268129697u, "Server_OverfishingCheck", this, player, 1uL))
						{
							return true;
						}
						long position = msg.read.Position;
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						msg.read.Position = position;
						if (!RPC_Server.IsActiveItem.Test(2268129697u, "Server_OverfishingCheck", this, player))
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
							Server_OverfishingCheck(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in Server_OverfishingCheck");
					}
				}
				return true;
			}
			if (rpc == 4238539495u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_RequestCast"));
				}
				using (TimeWarning.New("Server_RequestCast"))
				{
					using (TimeWarning.New("Conditions"))
					{
						long position2 = msg.read.Position;
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						msg.read.Position = position2;
						if (!RPC_Server.IsActiveItem.Test(4238539495u, "Server_RequestCast", this, player))
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
							Server_RequestCast(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in Server_RequestCast");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server.InputValidation(new Type[] { typeof(Vector3) })]
	[RPC_Server.IsActiveItem]
	[RPC_Server]
	private void Server_RequestCast(RPCMessage msg)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		Vector3 pos = msg.read.Vector3();
		BasePlayer ownerPlayer = GetOwnerPlayer();
		Item currentLure = GetCurrentLure();
		if (currentLure == null)
		{
			FailedCast(FailReason.NoLure);
			return;
		}
		if (!EvaluateFishingPosition(ref pos, ownerPlayer, out var reason, out surfaceBody))
		{
			FailedCast(reason);
			return;
		}
		object obj = Interface.CallHook("CanCastFishingRod", ownerPlayer, this, currentLure, pos);
		if (!(obj is bool) || (bool)obj)
		{
			FishingBobber component = ((Component)base.gameManager.CreateEntity(FishingBobberRef.resourcePath, ((Component)this).transform.position + Vector3.up * 2.8f + ownerPlayer.eyes.BodyForward() * 1.8f, GetOwnerPlayer().ServerRotation)).GetComponent<FishingBobber>();
			((Component)component).transform.forward = GetOwnerPlayer().eyes.BodyForward();
			component.Spawn();
			component.InitialiseBobber(ownerPlayer, surfaceBody, pos, 150f);
			int usedLureAmount = 0;
			bool isOverfished = false;
			if (FishLookup.Instance != null)
			{
				currentFishTarget = FishLookup.Instance.GetFish(((Component)component).transform.position, surfaceBody, currentLure, out fishableModifier, lastFish, out usedLureAmount, out isOverfished);
			}
			if (isOverfished)
			{
				ownerPlayer.ShowToast(GameTip.Styles.Red_Normal, overfishedWarningPhrase, false);
			}
			lureUsed = currentLure.info;
			currentLure.UseItem(usedLureAmount);
			lastFish = fishableModifier;
			currentBobber.Set(component);
			ClientRPC(RpcTarget.NetworkGroup("Client_ReceiveCastPoint"), component.net.ID);
			ownerPlayer.SignalBroadcast(Signal.Attack);
			catchTime = TimeUntil.op_Implicit(ImmediateHook ? 0f : Random.Range(10f, 20f));
			catchTime = TimeUntil.op_Implicit(TimeUntil.op_Implicit(catchTime) * fishableModifier.CatchWaitTimeMultiplier);
			ItemModCompostable itemModCompostable = default(ItemModCompostable);
			float num = (((Component)lureUsed).TryGetComponent<ItemModCompostable>(ref itemModCompostable) ? itemModCompostable.BaitValue : 0f);
			num = Mathx.RemapValClamped(num, 0f, 20f, 1f, 10f);
			catchTime = TimeUntil.op_Implicit(Mathf.Clamp(TimeUntil.op_Implicit(catchTime) - num, 3f, 20f));
			playerStartPosition = ((Component)ownerPlayer).transform.position;
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Busy, b: true);
			}
			CurrentState = CatchState.Waiting;
			InvokeRepeating(CatchProcess, 0f, 0f);
			inQueue = false;
			BasePlayer ownerPlayer2 = GetOwnerPlayer();
			if ((Object)(object)ownerPlayer2 != (Object)null)
			{
				Facepunch.Rust.Analytics.Azure.OnStartFish(ownerPlayer2, currentLure, pos);
			}
			Interface.CallHook("OnFishingRodCast", this, ownerPlayer, currentLure);
		}
	}

	private void FailedCast(FailReason reason)
	{
		CurrentState = CatchState.None;
		ClientRPC(RpcTarget.NetworkGroup("Client_ResetLine"), (int)reason);
	}

	[RPC_Server.InputValidation(new Type[] { typeof(Vector3) })]
	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.IsActiveItem]
	[RPC_Server]
	private void Server_OverfishingCheck(RPCMessage msg)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = msg.read.Vector3();
		if (!Fishing.disableOverfishing && (Object)(object)OverfishedArea.GetOverfishedAreaAtPosition(position) != (Object)null)
		{
			msg.player.ShowToast(GameTip.Styles.Red_Normal, overfishedWarningPhrase, false);
		}
	}

	private void CatchProcess()
	{
		if (!inQueue)
		{
			inQueue = true;
			((ObjectWorkQueue<BaseFishingRod>)updateFishingRodQueue).Add(this);
		}
	}

	private void CatchProcessBudgeted()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_055c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0595: Unknown result type (might be due to invalid IL or missing references)
		//IL_059a: Unknown result type (might be due to invalid IL or missing references)
		//IL_071e: Unknown result type (might be due to invalid IL or missing references)
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		inQueue = false;
		FishingBobber fishingBobber = currentBobber.Get(serverside: true);
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if ((Object)(object)ownerPlayer == (Object)null || ownerPlayer.IsSleeping() || ownerPlayer.IsWounded() || ownerPlayer.IsDead() || (Object)(object)fishingBobber == (Object)null)
		{
			Server_Cancel(FailReason.UserRequested);
			return;
		}
		Vector3 position = ((Component)ownerPlayer).transform.position;
		Vector3 val = Vector3Ex.WithY(((Component)fishingBobber).transform.position, 0f) - Vector3Ex.WithY(position, 0f);
		float num = Vector3.Angle(((Vector3)(ref val)).normalized, Vector3Ex.WithY(ownerPlayer.eyes.HeadForward(), 0f));
		float num2 = Vector3.Distance(position, Vector3Ex.WithY(((Component)fishingBobber).transform.position, position.y));
		if (num > ((num2 > 1.2f) ? 60f : 180f))
		{
			Server_Cancel(FailReason.BadAngle);
			return;
		}
		if (num2 > 1.2f && TimeSince.op_Implicit(lastSightCheck) > 0.4f)
		{
			if (!GamePhysics.LineOfSight(ownerPlayer.eyes.position, ((Component)fishingBobber).transform.position, 1084293377))
			{
				Server_Cancel(FailReason.Obstructed);
				return;
			}
			lastSightCheck = TimeSince.op_Implicit(0f);
		}
		if (Vector3.Distance(position, ((Component)fishingBobber).transform.position) > MaxCastDistance * 2f)
		{
			Server_Cancel(FailReason.TooFarAway);
			return;
		}
		if (Vector3.Distance(playerStartPosition, position) > 1f)
		{
			Server_Cancel(FailReason.PlayerMoved);
			return;
		}
		if (CurrentState == CatchState.Waiting)
		{
			if (TimeUntil.op_Implicit(catchTime) < 0f)
			{
				ClientRPC(RpcTarget.NetworkGroup("Client_HookedSomething"));
				CurrentState = CatchState.Catching;
				using (FlagsUpdateScope flagsUpdateScope2 = fishingBobber.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
				{
					flagsUpdateScope2.Set(Flags.Reserved1, b: true);
				}
				nextFishStateChange = TimeUntil.op_Implicit(0f);
				fishCatchDuration = TimeSince.op_Implicit(0f);
				strainTimer = 0f;
			}
			return;
		}
		FishState fishState = currentFishState;
		if (TimeUntil.op_Implicit(nextFishStateChange) < 0f)
		{
			float num3 = Mathx.RemapValClamped(fishingBobber.TireAmount, 0f, 20f, 0f, 1f);
			if (currentFishState != 0)
			{
				currentFishState = (FishState)0;
				nextFishStateChange = TimeUntil.op_Implicit(Random.Range(2f, 4f) * (num3 + 1f));
			}
			else
			{
				nextFishStateChange = TimeUntil.op_Implicit(Random.Range(3f, 7f) * (1f - num3));
				if (Random.Range(0, 100) < 50)
				{
					currentFishState = FishState.PullingLeft;
				}
				else
				{
					currentFishState = FishState.PullingRight;
				}
				if (Random.Range(0, 100) > 60 && Vector3.Distance(((Component)fishingBobber).transform.position, ((Component)ownerPlayer).transform.position) < MaxCastDistance - 2f)
				{
					currentFishState |= FishState.PullingBack;
				}
			}
		}
		if (TimeSince.op_Implicit(fishCatchDuration) > 120f)
		{
			Server_Cancel(FailReason.TimeOut);
			return;
		}
		bool flag = ownerPlayer.serverInput.IsDown(BUTTON.RIGHT);
		bool flag2 = ownerPlayer.serverInput.IsDown(BUTTON.LEFT);
		bool flag3 = HasReelInInput(ownerPlayer.serverInput);
		if (flag2 & flag)
		{
			flag2 = (flag = false);
		}
		UpdateFlags(flag2, flag, flag3);
		if (CurrentState == CatchState.Waiting)
		{
			flag = (flag2 = (flag3 = false));
		}
		if (flag2 && !AllowPullInDirection(-ownerPlayer.eyes.HeadRight(), ((Component)fishingBobber).transform.position))
		{
			flag2 = false;
		}
		if (flag && !AllowPullInDirection(ownerPlayer.eyes.HeadRight(), ((Component)fishingBobber).transform.position))
		{
			flag = false;
		}
		float value = ownerPlayer.modifiers.GetValue(Modifier.ModifierType.FishingBoost, 1f);
		fishingBobber.ServerMovementUpdate(flag2, flag, flag3, ref currentFishState, position, fishableModifier, value);
		bool flag4 = false;
		float num4 = 0f;
		if (flag3 | flag2 | flag)
		{
			flag4 = true;
			num4 = 0.5f;
		}
		if (currentFishState != 0 && flag4)
		{
			if (currentFishState.Contains(FishState.PullingBack) & flag3)
			{
				num4 = 1.5f;
			}
			else if ((currentFishState.Contains(FishState.PullingLeft) || currentFishState.Contains(FishState.PullingRight)) & flag3)
			{
				num4 = 1.2f;
			}
			else if (currentFishState.Contains(FishState.PullingLeft) & flag)
			{
				num4 = 0.8f;
			}
			else if (currentFishState.Contains(FishState.PullingRight) & flag2)
			{
				num4 = 0.8f;
			}
		}
		if (flag3 && currentFishState != 0)
		{
			num4++;
		}
		num4 *= fishableModifier.StrainModifier * GlobalStrainSpeedMultiplier;
		num4 -= ownerPlayer.modifiers.GetValue(Modifier.ModifierType.FishingBoost, 1f) - 1f;
		if (flag4)
		{
			strainTimer += Time.deltaTime * num4;
		}
		else
		{
			strainTimer = Mathf.MoveTowards(strainTimer, 0f, Time.deltaTime * 1.5f);
		}
		float num5 = strainTimer / 6f;
		flagsUpdateScope.Set(Flags.Reserved1, flag4 && num5 > 0.25f);
		if (TimeSince.op_Implicit(lastStrainUpdate) > 0.4f || fishState != currentFishState)
		{
			ClientRPC(RpcTarget.NetworkGroup("Client_UpdateFishState"), (int)currentFishState, num5);
			lastStrainUpdate = TimeSince.op_Implicit(0f);
		}
		if (strainTimer > 7f || ForceFail)
		{
			Server_Cancel(FailReason.TensionBreak);
		}
		else
		{
			if (!(num2 <= FishCatchDistance) && !ForceSuccess)
			{
				return;
			}
			CurrentState = CatchState.Caught;
			if ((Object)(object)currentFishTarget != (Object)null)
			{
				Item item = ItemManager.Create(currentFishTarget, 1, 0uL, isServerSide: true, 0uL);
				item.SetItemOwnership(ownerPlayer, ItemOwnershipPhrases.Fishing);
				object obj = Interface.CallHook("CanCatchFish", ownerPlayer, this, item);
				if (obj is bool && !(bool)obj)
				{
					return;
				}
				object obj2 = Interface.CallHook("OnFishCatch", item, this, ownerPlayer);
				if (obj2 is Item && obj2 as Item != item)
				{
					item.Remove();
					item = (Item)obj2;
				}
				ownerPlayer.GiveItem(item, GiveItemReason.Crafted);
				if (currentFishTarget.shortname == "skull.human")
				{
					item.name = RandomUsernames.Get(Random.Range(0, 1000));
				}
				if (Rust.GameInfo.HasAchievements && !string.IsNullOrEmpty(fishableModifier.SteamStatName))
				{
					ownerPlayer.stats.Add(fishableModifier.SteamStatName, 1);
					ownerPlayer.stats.Save(forceSteamSave: true);
					FishLookup.Instance.CheckCatchAllAchievement(ownerPlayer);
				}
				Facepunch.Rust.Analytics.Azure.OnCaughtFish(ownerPlayer, item);
				ItemModFishable itemModFishable = default(ItemModFishable);
				if (!Fishing.disableOverfishing && ((Component)currentFishTarget).TryGetComponent<ItemModFishable>(ref itemModFishable) && !itemModFishable.IsJunk)
				{
					FishArea(((Component)fishingBobber).transform.position);
				}
				ClientRPC(RpcTarget.NetworkGroup("Client_OnCaughtFish"), currentFishTarget.itemid);
			}
			ownerPlayer.SignalBroadcast(Signal.Alt_Attack);
			Invoke(ResetLine, 6f);
			fishingBobber.Kill();
			currentBobber.Set(null);
			Interface.CallHook("OnFishCaught", currentFishTarget, this, ownerPlayer);
			CancelInvoke(CatchProcess);
		}
	}

	private void ResetLine()
	{
		Server_Cancel(FailReason.Success);
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	private void Server_Cancel(RPCMessage msg)
	{
		if (CurrentState != CatchState.Caught)
		{
			Server_Cancel(FailReason.UserRequested);
		}
	}

	private void Server_Cancel(FailReason reason)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			if (GetItem() != null)
			{
				GetItem().LoseCondition((reason == FailReason.Success) ? ConditionLossOnSuccess : ConditionLossOnFail);
			}
			flagsUpdateScope.Set(Flags.Busy, b: false);
			UpdateFlags();
			CancelInvoke(CatchProcess);
			CurrentState = CatchState.None;
			flagsUpdateScope.Set(Flags.Reserved1, b: false);
			FishingBobber fishingBobber = currentBobber.Get(serverside: true);
			if ((Object)(object)fishingBobber != (Object)null)
			{
				fishingBobber.Kill();
				currentBobber.Set(null);
			}
			ClientRPC(RpcTarget.NetworkGroup("Client_ResetLine"), (int)reason);
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if ((Object)(object)ownerPlayer != (Object)null)
			{
				Facepunch.Rust.Analytics.Azure.OnFailedFish(ownerPlayer, reason);
			}
		}
		Interface.CallHook("OnFishingStopped", this, reason);
	}

	public override void OnHeldChanged()
	{
		base.OnHeldChanged();
		if (CurrentState != CatchState.None)
		{
			Server_Cancel(FailReason.Unequipped);
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (currentBobber.IsSet && info.msg.simpleUID == null)
		{
			info.msg.simpleUID = Pool.Get<SimpleUID>();
			info.msg.simpleUID.uid = currentBobber.uid;
		}
	}

	private void UpdateFlags(bool inputLeft = false, bool inputRight = false, bool back = false)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local);
		flagsUpdateScope.Set(PullingLeftFlag, (CurrentState == CatchState.Catching) & inputLeft);
		flagsUpdateScope.Set(PullingRightFlag, (CurrentState == CatchState.Catching) & inputRight);
		flagsUpdateScope.Set(ReelingInFlag, (CurrentState == CatchState.Catching) & back);
	}

	public void FishArea(Vector3 position)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		if (Fishing.disableOverfishing)
		{
			return;
		}
		if ((Object)(object)OverfishedArea.GetOverfishedAreaAtPosition(position) != (Object)null)
		{
			DebugOverfishing($"PROCESS FISH AREA | Position: ({position}) is already overfished.", (Object)(object)this);
			return;
		}
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector((float)Mathf.FloorToInt(position.x / Fishing.overfishedAreaRadius), (float)Mathf.FloorToInt(position.z / Fishing.overfishedAreaRadius));
		if (FishedSpots.ContainsKey(val))
		{
			float num = Fishing.overfishedAreaDurationMinutes * 60f;
			TimeSince val2;
			int num2;
			(num2, val2) = FishedSpots[val];
			if (TimeSince.op_Implicit(val2) >= num)
			{
				num2 = 1;
				DebugOverfishing("PROCESS FISH AREA | Area's timer has expired, resetting back to 1", (Object)(object)this);
			}
			else if (TimeSince.op_Implicit(val2) >= num / 2f)
			{
				num2 = Mathf.FloorToInt((float)num2 / 2f) + 1;
				DebugOverfishing("PROCESS FISH AREA | Area's timer has halfway expired, halving the counter and adding 1", (Object)(object)this);
			}
			else
			{
				num2++;
			}
			FishedSpots[val] = (num2, TimeSince.op_Implicit(0f));
			DebugOverfishing($"PROCESS FISH AREA | SubdividedPosition: ({val}) exists, incrementing counter to: {FishedSpots[val]}", (Object)(object)this);
		}
		else
		{
			FishedSpots.Add(val, (1, TimeSince.op_Implicit(0f)));
			DebugOverfishing($"PROCESS FISH AREA | SubdividedPosition: ({val}) does not exist yet, adding counter, set to: 1", (Object)(object)this);
		}
		if (FishedSpots[val].Item1 >= Fishing.successesUntilOverfished)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(OverfishedAreaPrefab.resourcePath, position, Quaternion.identity);
			baseEntity.Spawn();
			FishedSpots.Remove(val);
			DebugOverfishing($"PROCESS FISH AREA | SubdividedPosition: ({val}) is now overfished. Creating Overfished Area at position: {position}", (Object)(object)baseEntity);
		}
	}

	private void DebugOverfishing(string message, Object context = null)
	{
		if (Fishing.debugOverfishing)
		{
			Debug.Log((object)message, context);
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if ((!base.isServer || !info.fromDisk) && info.msg.simpleUID != null)
		{
			currentBobber.uid = info.msg.simpleUID.uid;
		}
	}

	public override bool BlocksGestures()
	{
		return CurrentState != CatchState.None;
	}

	private bool AllowPullInDirection(Vector3 worldDirection, Vector3 bobberPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		Vector3 val = Vector3Ex.WithY(bobberPosition, position.y);
		Vector3 val2 = val - position;
		return Vector3.Dot(worldDirection, ((Vector3)(ref val2)).normalized) < 0f;
	}

	private bool EvaluateFishingPosition(ref Vector3 pos, BasePlayer ply, out FailReason reason, out WaterBody waterBody)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		bool num = GamePhysics.Trace(new Ray(pos + Vector3.up, Vector3.down), 0f, out var hitInfo, 1.5f, 16, (QueryTriggerInteraction)0);
		if (num)
		{
			waterBody = RaycastHitEx.GetWaterBody(hitInfo);
			pos.y = ((RaycastHit)(ref hitInfo)).point.y;
		}
		else
		{
			waterBody = null;
		}
		if (!num)
		{
			reason = FailReason.NoWaterFound;
			return false;
		}
		if (Vector3.Distance(Vector3Ex.WithY(((Component)ply).transform.position, pos.y), pos) < 5f)
		{
			reason = FailReason.TooClose;
			return false;
		}
		if (!GamePhysics.LineOfSight(ply.eyes.position, pos, 1218652417))
		{
			reason = FailReason.Obstructed;
			return false;
		}
		Vector3 p = pos + Vector3.up * 2f;
		if (!GamePhysics.LineOfSight(ply.eyes.position, p, 1218652417))
		{
			reason = FailReason.Obstructed;
			return false;
		}
		Vector3 position = ((Component)ply).transform.position;
		position.y = pos.y;
		float num2 = Vector3.Distance(pos, position);
		Vector3 val = pos;
		Vector3 val2 = position - pos;
		Vector3 p2 = val + ((Vector3)(ref val2)).normalized * (num2 - FishCatchDistance);
		if (!GamePhysics.LineOfSight(pos, p2, 1218652417))
		{
			reason = FailReason.Obstructed;
			return false;
		}
		if (WaterLevel.GetOverallWaterDepth(Vector3.Lerp(pos, Vector3Ex.WithY(((Component)ply).transform.position, pos.y), 0.95f), waves: true, volumes: true) < 0.1f && ply.eyes.position.y > 0f)
		{
			reason = FailReason.TooShallow;
			return false;
		}
		if (WaterLevel.GetOverallWaterDepth(pos, waves: true, volumes: true) < 0.3f && ply.eyes.position.y > 0f)
		{
			reason = FailReason.TooShallow;
			return false;
		}
		Vector3 p3 = Vector3.MoveTowards(Vector3Ex.WithY(((Component)ply).transform.position, pos.y), pos, 1f);
		if (!GamePhysics.LineOfSight(ply.eyes.position, p3, 1218652417))
		{
			reason = FailReason.Obstructed;
			return false;
		}
		if (GamePhysics.CheckSphere(pos, 0.25f, 153092352, (QueryTriggerInteraction)0))
		{
			reason = FailReason.Obstructed;
			return false;
		}
		reason = FailReason.Success;
		return true;
	}

	private Item GetCurrentLure()
	{
		if (GetItem() == null)
		{
			return null;
		}
		if (GetItem().contents == null)
		{
			return null;
		}
		return GetItem().contents.GetSlot(0);
	}

	private bool HasReelInInput(InputState state)
	{
		if (!state.IsDown(BUTTON.BACKWARD))
		{
			return state.IsDown(BUTTON.FIRE_PRIMARY);
		}
		return true;
	}

	static BaseFishingRod()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		updateFishingRodQueue = new UpdateFishingRod();
		overfishedWarningPhrase = new Phrase("overfished_warning", "This area is overfished, only junk remains.");
		FishedSpots = new Dictionary<Vector2, (int, TimeSince)>();
		ForceSuccess = false;
		ForceFail = false;
		ImmediateHook = false;
		PullingLeftFlag = Flags.Reserved6;
		PullingRightFlag = Flags.Reserved7;
		ReelingInFlag = Flags.Reserved8;
	}
}

using System;
using ConVar;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class HackableLockedCrate : LootContainer
{
	public const Flags Flag_Hacking = Flags.Reserved1;

	public const Flags Flag_FullyHacked = Flags.Reserved2;

	public Text timerText;

	[ServerVar(Help = "How many seconds for the crate to unlock")]
	public static float requiredHackSeconds = 900f;

	[ServerVar(Help = "How many seconds until the crate is destroyed without any hack attempts")]
	public static float decaySeconds = 7200f;

	public SoundPlayer hackProgressBeep;

	public float hackSeconds;

	public GameObjectRef shockEffect;

	public GameObjectRef mapMarkerEntityPrefab;

	public GameObjectRef landEffect;

	public bool shouldDecay = true;

	public bool shouldParent = true;

	public string achievementStartHacking;

	public BasePlayer originalHackerPlayer;

	public ulong originalHackerPlayerId;

	public bool hasBeenOpened;

	public BaseEntity mapMarkerInstance;

	public bool hasLanded;

	public bool wasDropped;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("HackableLockedCrate.OnRpcMessage"))
		{
			if (rpc == 888500940 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Hack"));
				}
				using (TimeWarning.New("RPC_Hack"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(888500940u, "RPC_Hack", this, player, 3f))
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
							RPC_Hack(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_Hack");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsBeingHacked()
	{
		return HasFlag(Flags.Reserved1);
	}

	public bool IsFullyHacked()
	{
		return HasFlag(Flags.Reserved2);
	}

	public override void DestroyShared()
	{
		if (base.isServer && Object.op_Implicit((Object)(object)mapMarkerInstance))
		{
			mapMarkerInstance.Kill();
		}
		base.DestroyShared();
	}

	public void CreateMapMarker(float durationMinutes)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (mapMarkerEntityPrefab.isValid)
		{
			if (Object.op_Implicit((Object)(object)mapMarkerInstance))
			{
				mapMarkerInstance.Kill();
			}
			BaseEntity baseEntity = GameManager.server.CreateEntity(mapMarkerEntityPrefab.resourcePath, ((Component)this).transform.position, Quaternion.identity);
			if (shouldParent)
			{
				baseEntity.SetParent(this);
				((Component)baseEntity).transform.localPosition = Vector3.zero;
			}
			baseEntity.Spawn();
			baseEntity.SendNetworkUpdate();
			mapMarkerInstance = baseEntity;
		}
	}

	public void RefreshDecay()
	{
		CancelInvoke(DelayedDestroy);
		if (shouldDecay)
		{
			Invoke(DelayedDestroy, decaySeconds);
		}
	}

	public void DelayedDestroy()
	{
		Kill();
	}

	public override void OnAttacked(HitInfo info)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer)
		{
			if (StringPool.Get(info.HitBone) == "laptopcollision")
			{
				if (Interface.CallHook("OnCrateLaptopAttack", this, info) != null)
				{
					return;
				}
				Effect.server.Run(shockEffect.resourcePath, this, info.HitBone, info.HitPositionLocal, Vector3.up);
				hackSeconds -= 8f * (info.damageTypes.Total() / 50f);
				if (hackSeconds < 0f)
				{
					hackSeconds = 0f;
				}
			}
			RefreshDecay();
		}
		base.OnAttacked(info);
	}

	public void SetWasDropped()
	{
		wasDropped = true;
		Interface.CallHook("OnCrateDropped", this);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (base.isClient)
		{
			return;
		}
		if (!Application.isLoadingSave)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved1, b: false);
				flagsUpdateScope.Set(Flags.Reserved2, b: false);
			}
			if (wasDropped)
			{
				InvokeRepeating(LandCheck, 0f, 0.015f);
			}
		}
		RefreshDecay();
		isLootable = IsFullyHacked();
		CreateMapMarker(120f);
		base.inventory.onItemAddedRemoved = OnItemAddedOrRemoved;
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		if (!added && (Object)(object)mapMarkerInstance != (Object)null)
		{
			mapMarkerInstance.Kill();
		}
		base.OnItemAddedOrRemoved(item, added);
	}

	public void LandCheck()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (!hasLanded && Physics.Raycast(new Ray(((Component)this).transform.position + Vector3.up * 0.5f, Vector3.down), ref val, 1f, 1084293377))
		{
			Effect.server.Run(landEffect.resourcePath, ((RaycastHit)(ref val)).point, Vector3.up);
			hasLanded = true;
			Interface.CallHook("OnCrateLanded", this);
			CancelInvoke(LandCheck);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b: false);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_Hack(RPCMessage msg)
	{
		if (!IsBeingHacked() && Interface.CallHook("CanHackCrate", msg.player, this) == null)
		{
			Facepunch.Rust.Analytics.Azure.OnLockedCrateStarted(msg.player, this);
			originalHackerPlayerId = msg.player.userID;
			originalHackerPlayer = msg.player;
			if (!string.IsNullOrEmpty(achievementStartHacking))
			{
				msg.player?.GiveAchievement(achievementStartHacking);
			}
			StartHacking();
		}
	}

	public void StartHacking()
	{
		Interface.CallHook("OnCrateHack", this);
		BroadcastEntityMessage("HackingStarted", 20f, 257);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved1, b: true);
		}
		OnStartHacking();
		InvokeRepeating(HackProgress, 1f, 1f);
		ClientRPC(RpcTarget.NetworkGroup("UpdateHackProgress"), 0, (int)requiredHackSeconds);
		RefreshDecay();
	}

	protected virtual void OnStartHacking()
	{
	}

	public void HackProgress()
	{
		hackSeconds += 1f;
		if (hackSeconds > requiredHackSeconds)
		{
			Interface.CallHook("OnCrateHackEnd", this);
			Facepunch.Rust.Analytics.Azure.OnLockedCrateFinished(originalHackerPlayerId, this);
			if ((Object)(object)originalHackerPlayer != (Object)null && originalHackerPlayer.serverClan != null)
			{
				originalHackerPlayer.AddClanScore((ClanScoreEventType)5);
			}
			RefreshDecay();
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved2, b: true);
			}
			isLootable = true;
			CancelInvoke(HackProgress);
		}
		ClientRPC(RpcTarget.NetworkGroup("UpdateHackProgress"), (int)hackSeconds, (int)requiredHackSeconds);
	}

	public override bool OnStartBeingLooted(BasePlayer player)
	{
		bool num = base.OnStartBeingLooted(player);
		if (num && !hasBeenOpened)
		{
			hasBeenOpened = true;
			player.AddClanScore((ClanScoreEventType)6);
		}
		return num;
	}
}

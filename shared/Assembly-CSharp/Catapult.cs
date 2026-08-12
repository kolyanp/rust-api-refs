using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Catapult : BaseSiegeWeapon
{
	[Serializable]
	private struct Ammo
	{
		public ItemDefinition item;

		public GameObject go;
	}

	[Serializable]
	private struct FiringEffect
	{
		public ItemDefinition item;

		public GameObjectRef effectPrefab;
	}

	private static ItemDefinition _boulderItemDef;

	private Item loadedAmmoItem;

	private readonly float progressTickRate = 0.1f;

	[Header("Catapult")]
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Transform muzzle;

	[SerializeField]
	private float reloadTime = 6f;

	[SerializeField]
	private Ammo[] ammoPrefabs;

	[SerializeField]
	private GameObject ammoParent;

	[HideInInspector]
	public float reloadProgress;

	private const AmmoTypes ammoType = (AmmoTypes)8192;

	[SerializeField]
	private CatapultAmmoContainer ammoStoragePrefab;

	private EntityRef<CatapultAmmoContainer> ammoStorageRef;

	private const Flags Flag_Reloading = Flags.Reserved4;

	private const Flags Flag_Loaded = Flags.Unused23;

	private BasePlayer reloadingPlayer;

	private ItemDefinition loadedAmmoDef;

	private TimeSince timeSinceLastFire;

	[Header("Effects")]
	[SerializeField]
	public GameObjectRef dryFireEffectPrefab;

	[SerializeField]
	private FiringEffect[] fireEffects;

	public static ItemDefinition BoulderItemDef
	{
		get
		{
			if ((Object)(object)_boulderItemDef == (Object)null)
			{
				_boulderItemDef = ItemManager.FindItemDefinition("catapult.ammo.boulder");
			}
			return _boulderItemDef;
		}
	}

	public override float DriveWheelVelocity { get; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Catapult.OnRpcMessage"))
		{
			if (rpc == 1188838966 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_CancelReload"));
				}
				using (TimeWarning.New("SERVER_CancelReload"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1188838966u, "SERVER_CancelReload", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1188838966u, "SERVER_CancelReload", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1188838966u, "SERVER_CancelReload", this, player, 3f))
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
							SERVER_CancelReload(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SERVER_CancelReload");
					}
				}
				return true;
			}
			if (rpc == 3952894422u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_OpenAmmo"));
				}
				using (TimeWarning.New("SERVER_OpenAmmo"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3952894422u, "SERVER_OpenAmmo", this, player, 3f))
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
							SERVER_OpenAmmo(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SERVER_OpenAmmo");
					}
				}
				return true;
			}
			if (rpc == 2817383917u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_ReloadStart"));
				}
				using (TimeWarning.New("SERVER_ReloadStart"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2817383917u, "SERVER_ReloadStart", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2817383917u, "SERVER_ReloadStart", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2817383917u, "SERVER_ReloadStart", this, player, 3f))
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
							SERVER_ReloadStart(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in SERVER_ReloadStart");
					}
				}
				return true;
			}
			if (rpc == 4172853352u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_WantsFire"));
				}
				using (TimeWarning.New("SERVER_WantsFire"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4172853352u, "SERVER_WantsFire", this, player, 3f))
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
							SERVER_WantsFire(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in SERVER_WantsFire");
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
		ammoStorageRef.Set(ammoStoragePrefab);
	}

	public void UpdateLoadedAmmo(Item item, bool added)
	{
		loadedAmmoItem = (added ? item : null);
		loadedAmmoDef = (added ? item.info : null);
		SetFlagLocal(Flags.Unused23, (Object)(object)loadedAmmoDef != (Object)null);
		SendNetworkUpdateImmediate();
	}

	public bool Fire(BasePlayer shooter, float force)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		FireRecoil();
		float num = Mathf.Lerp(2f, 1f, Mathf.Clamp01(force));
		float num2 = Mathf.Lerp(0.5f, 1f, Mathf.Clamp01(force));
		bool flag = true;
		Vector3 firingPos = muzzle.position;
		BasePlayer passenger = GetPassenger();
		object obj = Interface.CallHook("OnCatapultFireForce", this, shooter, num2);
		if (obj is float)
		{
			num2 = (float)obj;
		}
		if ((Object)(object)passenger != (Object)null)
		{
			passenger.ServerPosition = ((Component)muzzle).transform.position;
			passenger.Ragdoll(((Component)muzzle).transform.forward * (20f * num2) + Vector3.up * (2.5f * num), matchPlayerGravity: false, flailInAir: true, dieOnImpact: true, this);
			return true;
		}
		if (GamePhysics.CheckSphere(muzzle.position, 1f, 1236994833, (QueryTriggerInteraction)1))
		{
			Vector3 val = ((Component)this).transform.position + Vector3.up * 2f;
			if (GamePhysics.Trace(new Ray(val, muzzle.position - val), 0f, out var hitInfo, 10f, 1236994833, (QueryTriggerInteraction)1))
			{
				flag = false;
				firingPos = ((RaycastHit)(ref hitInfo)).point - Vector3.up;
			}
		}
		ServerProjectile projectile2;
		if ((Object)(object)loadedAmmoDef == (Object)(object)BoulderItemDef)
		{
			ItemModCatapultBoulder component = ((Component)loadedAmmoDef).GetComponent<ItemModCatapultBoulder>();
			if ((Object)(object)component == (Object)null)
			{
				return false;
			}
			foreach (ItemModCatapultBoulder.ProjectileSettings projectileSetting in component.projectileSettings)
			{
				for (int i = 0; i < projectileSetting.count; i++)
				{
					Vector3 forward = muzzle.forward;
					forward = Quaternion.Euler(Random.Range(0f - component.spreadAngle, component.spreadAngle), Random.Range(0f - component.spreadAngle, component.spreadAngle), 0f) * forward;
					if (FireProjectile(projectileSetting.prefab, firingPos, forward, shooter, 0.25f, 30f * num2, out var projectile))
					{
						if (!flag)
						{
							((Component)projectile).GetComponent<TimedExplosive>()?.ForceExplode();
						}
						projectile.ignoreEntity = this;
						projectile.gravityModifier *= num * Random.Range(1f - projectileSetting.gravityModifier, 1f + projectileSetting.gravityModifier);
						shooter.MarkHostileFor();
					}
				}
			}
			loadedAmmoItem.UseItem();
		}
		else if (TryFireProjectile(ammoStorageRef.Get(base.isServer), (AmmoTypes)8192, firingPos, muzzle.forward, shooter, 0.25f, 30f * num2, out projectile2))
		{
			projectile2.ignoreEntity = this;
			if (!flag)
			{
				((Component)projectile2).GetComponent<TimedExplosive>()?.ForceExplode();
			}
			projectile2.gravityModifier *= num;
			shooter.MarkHostileFor();
			return true;
		}
		return false;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void SERVER_WantsFire(RPCMessage msg)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if (CanFire() && Interface.CallHook("OnSiegeWeaponFire", this, player) == null)
		{
			float force = reloadProgress;
			Fire(player, force);
			reloadProgress = 0f;
			timeSinceLastFire = TimeSince.op_Implicit(0f);
			FireAnimation();
			GameObjectRef loadedAmmoFiringEffect = GetLoadedAmmoFiringEffect();
			if (loadedAmmoFiringEffect != null && loadedAmmoFiringEffect.isValid)
			{
				Effect.server.Run(loadedAmmoFiringEffect.resourcePath, this, 0u, Vector3.zero, Vector3.up, null, broadcast: true);
			}
			RefreshLastUseTime();
			ClientRPC(RpcTarget.NetworkGroup("CLIENT_Fire"));
		}
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (!HasFlag(Flags.Unused23))
		{
			base.AttemptMount(player, doMountChecks);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void SERVER_OpenAmmo(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (CanBeLooted(player))
		{
			ammoStorageRef.Get(base.isServer).PlayerOpenLoot(player);
		}
	}

	public void FireRecoil()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)rigidBody == (Object)null))
		{
			if (rigidBody.IsSleeping())
			{
				rigidBody.WakeUp();
			}
			Vector3 val = Vector3.ProjectOnPlane(((Component)this).transform.forward, ((Component)this).transform.up);
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			rigidBody.AddForce(normalized * rigidBody.mass * (carPhysics.HasHandbrake() ? 5f : 1f), (ForceMode)1);
			rigidBody.AddForceAtPosition(Vector3.up * rigidBody.mass * 1.5f, centreOfMassTransform.position + ((Component)this).transform.forward * 1f, (ForceMode)1);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void SERVER_ReloadStart(RPCMessage msg)
	{
		if (msg.player.CanInteract() && CanReload())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved4, b: true);
			}
			RefreshLastUseTime();
			reloadingPlayer = msg.player;
			InvokeRepeating(ReloadProgress, 0f, progressTickRate);
		}
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void SERVER_CancelReload(RPCMessage msg)
	{
		if ((Object)(object)msg.player == (Object)(object)reloadingPlayer)
		{
			StopPlayerReload();
		}
	}

	private void ReloadProgress()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)reloadingPlayer == (Object)null || reloadingPlayer.IsDead() || reloadingPlayer.IsSleeping() || Vector3Ex.Distance2D(((Component)reloadingPlayer).transform.position, ((Component)this).transform.position) > 5f)
		{
			StopPlayerReload();
			return;
		}
		reloadProgress += progressTickRate / reloadTime;
		animator.SetFloat("Reload", reloadProgress);
		if (reloadProgress >= 1f)
		{
			reloadProgress = 1f;
			StopPlayerReload();
		}
		SendNetworkUpdate();
	}

	public void AdminReload(int ammo)
	{
		foreach (MountPointInfo allMountPoint in base.allMountPoints)
		{
			BasePlayer mounted = allMountPoint.mountable.GetMounted();
			if ((Object)(object)mounted != (Object)null && !mounted.IsBot)
			{
				return;
			}
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved4, b: true);
		}
		reloadProgress = 1f;
		animator.SetFloat("Reload", reloadProgress);
		switch (ammo)
		{
		case 5:
			ammoStorageRef.Get(base.isServer).inventory.Clear();
			SpawnAndMountBotPlayer();
			break;
		default:
		{
			ammoStorageRef.Get(base.isServer).inventory.Clear();
			foreach (MountPointInfo allMountPoint2 in base.allMountPoints)
			{
				BasePlayer mounted2 = allMountPoint2.mountable.GetMounted();
				if ((Object)(object)mounted2 != (Object)null && mounted2.IsBot)
				{
					mounted2.EnsureDismounted();
					mounted2.Kill();
				}
			}
			ItemDefinition itemToCreate = null;
			switch (ammo)
			{
			case 1:
				itemToCreate = ItemManager.FindItemDefinition("catapult.ammo.boulder");
				break;
			case 2:
				itemToCreate = ItemManager.FindItemDefinition("catapult.ammo.incendiary");
				break;
			case 3:
				itemToCreate = ItemManager.FindItemDefinition("catapult.ammo.explosive");
				break;
			case 4:
				itemToCreate = ItemManager.FindItemDefinition("catapult.ammo.bee");
				break;
			}
			ammoStorageRef.Get(base.isServer).inventory.AddItem(itemToCreate, 1, 0uL);
			break;
		}
		case 0:
			break;
		}
		SendNetworkUpdateImmediate();
		Invoke(StopPlayerReload, 0.5f);
	}

	private void SpawnAndMountBotPlayer()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		foreach (MountPointInfo allMountPoint in base.allMountPoints)
		{
			BasePlayer basePlayer = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab") as BasePlayer;
			basePlayer.Spawn();
			allMountPoint.mountable.AttemptMount(basePlayer, doMountChecks: false);
			if (!basePlayer.isMounted)
			{
				basePlayer.Kill();
			}
			else
			{
				basePlayer.UpdateNetworkGroup();
			}
		}
	}

	public void StopPlayerReload()
	{
		CancelInvoke(ReloadProgress);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved4, b: false);
		}
		reloadingPlayer = null;
	}

	public override void OnTowAttach()
	{
		base.OnTowAttach();
	}

	public override void OnTowDetach()
	{
		base.OnTowDetach();
	}

	public override void OnEngineStartFailed()
	{
	}

	public override bool MeetsEngineRequirements()
	{
		return false;
	}

	private GameObjectRef GetLoadedAmmoFiringEffect()
	{
		if ((Object)(object)loadedAmmoDef == (Object)null)
		{
			return dryFireEffectPrefab;
		}
		FiringEffect firingEffect = List.FindWith<FiringEffect, ItemDefinition>((IReadOnlyCollection<FiringEffect>)fireEffects, (Func<FiringEffect, ItemDefinition>)((FiringEffect x) => x.item), loadedAmmoDef, (IEqualityComparer<ItemDefinition>)null);
		if (firingEffect.effectPrefab != null && firingEffect.effectPrefab.isValid)
		{
			return firingEffect.effectPrefab;
		}
		return null;
	}

	[ServerVar(Help = "Reload all catapults. 0: empty, 1: stone boulder, 2: fire bomb, 3: propane explosive, 4: bee bomb, 5: bot player")]
	public static void reload(ConsoleSystem.Arg arg)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Null player.");
		}
		else
		{
			if (!basePlayer.IsAdmin)
			{
				return;
			}
			int num = arg.GetInt(0, 1);
			num = Mathf.Clamp(num, 0, 5);
			Catapult[] array = Util.FindAll<Catapult>();
			int num2 = 0;
			Catapult[] array2 = array;
			foreach (Catapult catapult in array2)
			{
				if (catapult.isServer && Vector3.Distance(((Component)catapult).transform.position, ((Component)basePlayer).transform.position) <= 100f)
				{
					catapult.AdminReload(num);
					num2++;
				}
			}
			arg.ReplyWith($"Reloaded {num2} catapults.");
		}
	}

	[ServerVar(Help = "Fire all catapults")]
	public static void fire(ConsoleSystem.Arg arg)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Null player.");
		}
		else
		{
			if (!basePlayer.IsAdmin)
			{
				return;
			}
			Catapult[] array = Util.FindAll<Catapult>();
			foreach (Catapult catapult in array)
			{
				if (catapult.isServer && Vector3.Distance(((Component)catapult).transform.position, ((Component)basePlayer).transform.position) <= 100f)
				{
					RPCMessage msg = new RPCMessage
					{
						player = basePlayer
					};
					catapult.SERVER_WantsFire(msg);
				}
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.catapult = Pool.Get<Catapult>();
		info.msg.catapult.ammoStorageID = ammoStorageRef.uid;
		info.msg.catapult.reloadProgress = reloadProgress;
		info.msg.catapult.ammoType = ((loadedAmmoItem != null) ? loadedAmmoDef.itemid : 0);
	}

	public override void Load(LoadInfo info)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.catapult != null)
		{
			ammoStorageRef.uid = info.msg.catapult.ammoStorageID;
			reloadProgress = info.msg.catapult.reloadProgress;
			_ = loadedAmmoDef;
			loadedAmmoDef = ItemManager.FindItemDefinition(info.msg.catapult.ammoType);
		}
	}

	public override void InitShared()
	{
		base.InitShared();
		Invoke(delegate
		{
			animator.SetFloat("Reload", reloadProgress);
		}, 0.25f);
	}

	protected override void CreateEngineController()
	{
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (child.prefabID == ammoStoragePrefab.GetEntity().prefabID)
		{
			CatapultAmmoContainer catapultAmmoContainer = (CatapultAmmoContainer)child;
			ammoStorageRef.Set(catapultAmmoContainer);
			catapultAmmoContainer.catapult = this;
		}
	}

	public bool IsArmed()
	{
		return reloadProgress >= 0.4f;
	}

	public bool IsReloading()
	{
		return HasFlag(Flags.Reserved4);
	}

	public bool CanReload()
	{
		if (!IsReloading() && !HasFlag(Flags.Reserved12) && reloadProgress != 1f)
		{
			return !IsWaterlogged();
		}
		return false;
	}

	public bool CanFire()
	{
		if (IsArmed() && !IsReloading())
		{
			return !IsWaterlogged();
		}
		return false;
	}

	protected override bool CanPushNow(BasePlayer pusher)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (base.CanPushNow(pusher))
		{
			return TimeSince.op_Implicit(timeSinceLastFire) > 2f;
		}
		return false;
	}

	private void FireAnimation()
	{
		animator.ResetTrigger("Fire");
		animator.SetTrigger("Fire");
		Invoke(delegate
		{
			reloadProgress = 0f;
			animator.SetFloat("Reload", reloadProgress);
		}, 0.5f);
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (!HasFlag(Flags.Reserved11))
		{
			return base.CanBeLooted(player);
		}
		return false;
	}
}

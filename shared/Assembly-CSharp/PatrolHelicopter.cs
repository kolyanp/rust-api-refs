using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;

public class PatrolHelicopter : BaseCombatEntity, SeekerTarget.ISeekerTargetOwner
{
	[Serializable]
	public class weakspot
	{
		[NonSerialized]
		public PatrolHelicopter body;

		public string[] bonenames;

		public float maxHealth;

		public float health;

		public float healthFractionOnDestroyed = 0.5f;

		public GameObjectRef destroyedParticles;

		public GameObjectRef damagedParticles;

		public GameObject damagedEffect;

		public GameObject destroyedEffect;

		public List<BasePlayer> attackers;

		private bool isDestroyed;

		public float HealthFraction()
		{
			return health / maxHealth;
		}

		public void Hurt(float amount, HitInfo info)
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			if (!isDestroyed)
			{
				health -= amount;
				Effect.server.Run(damagedParticles.resourcePath, body, StringPool.Get(bonenames[Random.Range(0, bonenames.Length)]), Vector3.zero, Vector3.up, null, broadcast: true);
				if (health <= 0f)
				{
					health = 0f;
					WeakspotDestroyed();
				}
			}
		}

		public void Heal(float amount)
		{
			health += amount;
		}

		public void WeakspotDestroyed()
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			isDestroyed = true;
			Effect.server.Run(destroyedParticles.resourcePath, body, StringPool.Get(bonenames[Random.Range(0, bonenames.Length)]), Vector3.zero, Vector3.up, null, broadcast: true);
			body.Hurt(body.MaxHealth() * healthFractionOnDestroyed, DamageType.Generic, null, useProtection: false);
		}
	}

	public weakspot[] weakspots;

	public GameObject rotorPivot;

	public GameObject mainRotor;

	public GameObject mainRotor_blades;

	public GameObject mainRotor_blur;

	public GameObject tailRotor;

	public GameObject tailRotor_blades;

	public GameObject tailRotor_blur;

	public GameObject rocket_tube_left;

	public GameObject rocket_tube_right;

	public GameObject left_gun_yaw;

	public GameObject left_gun_pitch;

	public GameObject left_gun_muzzle;

	public GameObject right_gun_yaw;

	public GameObject right_gun_pitch;

	public GameObject right_gun_muzzle;

	public GameObject spotlight_rotation;

	public GameObjectRef rocket_fire_effect;

	public GameObjectRef gun_fire_effect;

	public GameObjectRef bulletEffect;

	public GameObjectRef explosionEffect;

	public GameObjectRef fireBall;

	public GameObjectRef crateToDrop;

	public int maxCratesToSpawn = 4;

	public float bulletSpeed = 250f;

	public float bulletDamage = 20f;

	public GameObjectRef servergibs;

	public GameObjectRef debrisFieldMarker;

	public float flareDuration = 5f;

	public SoundDefinition rotorWashSoundDef;

	private Sound _rotorWashSound;

	public SoundDefinition flightEngineSoundDef;

	public SoundDefinition flightThwopsSoundDef;

	private Sound flightEngineSound;

	private Sound flightThwopsSound;

	public SoundModulation.Modulator flightEngineGainMod;

	public SoundModulation.Modulator flightThwopsGainMod;

	public float rotorGainModSmoothing = 0.25f;

	public float engineGainMin = 0.5f;

	public float engineGainMax = 1f;

	public float thwopGainMin = 0.5f;

	public float thwopGainMax = 1f;

	public float spotlightJitterAmount = 5f;

	public float spotlightJitterSpeed = 5f;

	public GameObject[] nightLights;

	public Vector3 spotlightTarget;

	public float engineSpeed = 1f;

	public float targetEngineSpeed = 1f;

	public float blur_rotationScale = 0.05f;

	public ParticleSystem[] _rotorWashParticles;

	public PatrolHelicopterAI myAI;

	public GameObjectRef mapMarkerEntityPrefab;

	public GameObjectRef fleeMapMarkerEntityPrefab;

	public static PatrolHelicopter Instance;

	public static BaseEntity ClientFleeMapMarker;

	public float lastNetworkUpdate = float.NegativeInfinity;

	private const float networkUpdateRate = 0.25f;

	private BaseEntity mapMarkerInstance;

	private BaseEntity fleeMapMarkerInstance;

	private NetworkableId __sync_FleeMarkerId;

	[Sync(Autosave = true)]
	public NetworkableId FleeMarkerId
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return __sync_FleeMarkerId;
		}
		[CompilerGenerated]
		set
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (!IsSyncVarEqual<NetworkableId>(__sync_FleeMarkerId, value))
			{
				__sync_FleeMarkerId = value;
				byte nameID = __GetWeaverID("FleeMarkerId");
				QueueSyncVar(nameID);
			}
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PatrolHelicopter.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void InitalizeWeakspots()
	{
		weakspot[] array = weakspots;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].body = this;
		}
	}

	public override void OnAttacked(HitInfo info)
	{
		base.OnAttacked(info);
		if (base.isServer)
		{
			myAI.WasAttacked(info);
		}
	}

	public override void Hurt(HitInfo info)
	{
		if (Interface.CallHook("OnPatrolHelicopterTakeDamage", this, info) != null)
		{
			return;
		}
		bool flag = false;
		if (info.damageTypes.Total() >= base.health)
		{
			if (Interface.CallHook("OnPatrolHelicopterKill", this, info) != null)
			{
				return;
			}
			base.health = 10000f;
			myAI.CriticalDamage();
			flag = true;
			if ((Object)(object)info.InitiatorPlayer != (Object)null && info.InitiatorPlayer.serverClan != null)
			{
				info.InitiatorPlayer.AddClanScore((ClanScoreEventType)11);
			}
		}
		base.Hurt(info);
		if (flag)
		{
			return;
		}
		myAI.OtherDamaged(info);
		weakspot[] array = weakspots;
		foreach (weakspot weakspot2 in array)
		{
			string[] bonenames = weakspot2.bonenames;
			foreach (string str in bonenames)
			{
				if (info.HitBone == StringPool.Get(str))
				{
					weakspot2.Hurt(info.damageTypes.Total(), info);
					myAI.WeakspotDamaged(weakspot2, info);
				}
			}
		}
	}

	public override float AntiHackVelocity()
	{
		return 100f;
	}

	public override void InitShared()
	{
		base.InitShared();
		InitalizeWeakspots();
		Instance = this;
	}

	public override void Load(LoadInfo info)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.helicopter != null)
		{
			spotlightTarget = info.msg.helicopter.spotlightVec;
		}
	}

	public void RadarLock(SeekingServerProjectile incoming)
	{
		if (!IsInvoking(CancelRadar))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved8, b: true);
			}
			Invoke(CancelRadar, 5f);
		}
	}

	public void CancelRadar()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved8, b: false);
	}

	public override void Save(SaveInfo info)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.helicopter = Pool.Get<Helicopter>();
		Helicopter helicopter = info.msg.helicopter;
		Quaternion localRotation = rotorPivot.transform.localRotation;
		helicopter.tiltRot = ((Quaternion)(ref localRotation)).eulerAngles;
		info.msg.helicopter.spotlightVec = spotlightTarget;
		info.msg.helicopter.weakspothealths = Pool.Get<List<float>>();
		for (int i = 0; i < weakspots.Length; i++)
		{
			info.msg.helicopter.weakspothealths.Add(weakspots[i].health);
		}
	}

	public override void ServerInit()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		myAI = ((Component)this).GetComponent<PatrolHelicopterAI>();
		if (!myAI.hasInterestZone)
		{
			myAI.SetInitialDestination(Vector3.zero, 1.25f);
			myAI.targetThrottleSpeed = 1f;
			myAI.ExitCurrentState();
			myAI.State_Patrol_Enter();
		}
		CreateMapMarker();
		SeekerTarget.SetSeekerTarget(this, SeekerTarget.SeekerStrength.MEDIUM);
	}

	public override void DestroyShared()
	{
		if (base.isServer)
		{
			SeekerTarget.SetSeekerTarget(this, SeekerTarget.SeekerStrength.OFF);
		}
		if ((Object)(object)fleeMapMarkerInstance != (Object)null)
		{
			DestroyFleeMarker();
		}
		Instance = null;
		base.DestroyShared();
	}

	public void CreateMapMarker()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)mapMarkerInstance))
		{
			mapMarkerInstance.Kill();
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(mapMarkerEntityPrefab.resourcePath, Vector3.zero, Quaternion.identity);
		baseEntity.SetParent(this);
		baseEntity.Spawn();
		mapMarkerInstance = baseEntity;
	}

	public bool HasFleeMarker()
	{
		if ((Object)(object)fleeMapMarkerInstance != (Object)null)
		{
			return fleeMapMarkerInstance.IsValid();
		}
		return false;
	}

	public void CreateFleeMarker(Vector3 fleePosition)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)fleeMapMarkerInstance))
		{
			fleeMapMarkerInstance.Kill();
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(fleeMapMarkerEntityPrefab.resourcePath, fleePosition, Quaternion.identity);
		baseEntity.Spawn();
		fleeMapMarkerInstance = baseEntity;
		FleeMarkerId = fleeMapMarkerInstance.net.ID;
	}

	public void DestroyFleeMarker()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (HasFleeMarker())
		{
			fleeMapMarkerInstance.Kill();
			fleeMapMarkerInstance = null;
			FleeMarkerId = default(NetworkableId);
		}
	}

	public override void OnPositionalNetworkUpdate()
	{
		SendNetworkUpdate();
		base.OnPositionalNetworkUpdate();
	}

	public void CreateExplosionMarker(float durationMinutes)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = GameManager.server.CreateEntity(debrisFieldMarker.resourcePath, ((Component)this).transform.position, Quaternion.identity);
		baseEntity.Spawn();
		((Component)baseEntity).SendMessage("SetDuration", (object)durationMinutes, (SendMessageOptions)1);
	}

	public override void OnDied(HitInfo info)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		if (base.isClient)
		{
			return;
		}
		CreateExplosionMarker(10f);
		Effect.server.Run(explosionEffect.resourcePath, ((Component)this).transform.position, Vector3.up, null, broadcast: true);
		Vector3 val = myAI.GetLastMoveDir() * myAI.GetMoveSpeed() * 0.75f;
		GameObject gibSource = servergibs.Get().GetComponent<ServerGib>()._gibSource;
		List<ServerGib> list = ServerGib.CreateGibs(servergibs.resourcePath, ((Component)this).gameObject, gibSource, val, 3f);
		if (info.damageTypes.GetMajorityDamageType() != DamageType.Decay)
		{
			for (int i = 0; i < 12 - maxCratesToSpawn; i++)
			{
				BaseEntity baseEntity = GameManager.server.CreateEntity(this.fireBall.resourcePath, ((Component)this).transform.position, ((Component)this).transform.rotation);
				if (!Object.op_Implicit((Object)(object)baseEntity))
				{
					continue;
				}
				float num = 3f;
				float num2 = 10f;
				Vector3 onUnitSphere = Random.onUnitSphere;
				((Component)baseEntity).transform.position = ((Component)this).transform.position + new Vector3(0f, 1.5f, 0f) + onUnitSphere * Random.Range(-4f, 4f);
				Collider component = ((Component)baseEntity).GetComponent<Collider>();
				baseEntity.Spawn();
				baseEntity.SetVelocity(val + onUnitSphere * Random.Range(num, num2));
				foreach (ServerGib item in list)
				{
					Physics.IgnoreCollision(component, (Collider)(object)item.GetCollider(), true);
				}
			}
		}
		for (int j = 0; j < maxCratesToSpawn; j++)
		{
			Vector3 onUnitSphere2 = Random.onUnitSphere;
			Vector3 pos = ((Component)this).transform.position + new Vector3(0f, 1.5f, 0f) + onUnitSphere2 * Random.Range(2f, 3f);
			BaseEntity baseEntity2 = GameManager.server.CreateEntity(crateToDrop.resourcePath, pos, Quaternion.LookRotation(onUnitSphere2));
			baseEntity2.Spawn();
			LootContainer lootContainer = baseEntity2 as LootContainer;
			if (Object.op_Implicit((Object)(object)lootContainer))
			{
				lootContainer.Invoke(lootContainer.RemoveMe, 1800f);
			}
			Collider component2 = ((Component)baseEntity2).GetComponent<Collider>();
			Rigidbody val2 = ((Component)baseEntity2).gameObject.AddComponent<Rigidbody>();
			val2.useGravity = true;
			val2.collisionDetectionMode = (CollisionDetectionMode)2;
			val2.mass = 2f;
			val2.interpolation = (RigidbodyInterpolation)1;
			val2.linearVelocity = val + onUnitSphere2 * Random.Range(1f, 3f);
			val2.angularVelocity = Vector3Ex.Range(-1.75f, 1.75f);
			val2.linearDamping = 0.5f * (val2.mass / 5f);
			val2.angularDamping = 0.2f * (val2.mass / 5f);
			FireBall fireBall = GameManager.server.CreateEntity(this.fireBall.resourcePath) as FireBall;
			if (Object.op_Implicit((Object)(object)fireBall))
			{
				fireBall.SetParent(baseEntity2);
				fireBall.Spawn();
				((Component)fireBall).GetComponent<Rigidbody>().isKinematic = true;
				((Component)fireBall).GetComponent<Collider>().enabled = false;
			}
			((Component)baseEntity2).SendMessage("SetLockingEnt", (object)((Component)fireBall).gameObject, (SendMessageOptions)1);
			foreach (ServerGib item2 in list)
			{
				Physics.IgnoreCollision(component2, (Collider)(object)item2.GetCollider(), true);
			}
			Interface.CallHook("OnCrateSpawned", this, baseEntity2);
		}
		base.OnDied(info);
	}

	public bool IsValidHomingTarget()
	{
		object obj = Interface.CallHook("CanBeHomingTargeted", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return true;
	}

	public override void OnEntityMessage(BaseEntity from, string msg)
	{
		base.OnEntityMessage(from, msg);
		if (msg == "RadarLock" && !IsInvoking(DoFlare))
		{
			Invoke(DoFlare, Random.Range(0.5f, 1f));
		}
	}

	public void DoFlare()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.OnFire, b: true);
		}
		SeekerTarget.SetSeekerTarget(this, SeekerTarget.SeekerStrength.OFF);
		Invoke(ClearFlares, flareDuration);
	}

	public void ClearFlares()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.OnFire, b: false);
		}
		SeekerTarget.SetSeekerTarget(this, SeekerTarget.SeekerStrength.MEDIUM);
	}

	public void Update()
	{
		if (base.isServer && Time.realtimeSinceStartup - lastNetworkUpdate >= 0.25f)
		{
			SendNetworkUpdate();
			lastNetworkUpdate = Time.realtimeSinceStartup;
		}
	}

	protected unsafe override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: FleeMarkerId for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite<NetworkableId>(writer, __sync_FleeMarkerId);
			return true;
		}
		return base.WriteSyncVar(id, writer);
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (id == 0)
		{
			try
			{
				_ = __sync_FleeMarkerId;
				NetworkableId _sync_FleeMarkerId = reader.EntityID();
				__sync_FleeMarkerId = _sync_FleeMarkerId;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		}
		return base.OnSyncVar(id, reader, fromAutoSave);
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (propertyName == "FleeMarkerId")
		{
			return 0;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		WriteAutoSaveSyncVars(netWrite);
		var (src, num) = netWrite.GetBuffer();
		if (_autosaveBuffer == null)
		{
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		if (_autosaveBuffer.Length < num)
		{
			BaseEntity._autosaveBufferPool.Return(_autosaveBuffer);
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		Buffer.BlockCopy(src, 0, _autosaveBuffer, 0, num);
		save.msg.baseEntity.syncVars = _autosaveBuffer;
		Pool.Free<NetWrite>(ref netWrite);
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		if (load.msg.baseEntity != null && load.msg.baseEntity.syncVars != null)
		{
			NetRead netRead = Pool.Get<NetRead>();
			netRead.Init(load.msg.baseEntity.syncVars.AsSpan());
			ReadAutoSaveSyncVars(netRead);
			Pool.Free<NetRead>(ref netRead);
		}
		return true;
	}

	protected override void ResetSyncVars()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		base.ResetSyncVars();
		__sync_FleeMarkerId = default(NetworkableId);
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 0)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}
}

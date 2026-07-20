using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;

public class BeeSwarmMaster : BaseCombatEntity, ISplashable
{
	[Header("References")]
	public ParticleSystem PSystem;

	public Light OnFireLight;

	public ParticleSystemForceField AngerForceField;

	public GameObject sound;

	public GameObjectRef beeSwarmPrefab;

	public const Flags IsDying = Flags.Reserved13;

	[ServerVar(Help = "How long a master swarm will stick around without a target")]
	public static float killWithoutATargetTime = 150f;

	[ServerVar(Help = "How many child swarms a master swarm will create")]
	public static int amountToSpawn = 3;

	[ServerVar(Help = "How long before a master swarm will create a child")]
	public static float secondsBetweenSpawns = 60f;

	private TimeSince timeSinceLastSpawnedSwarm;

	private int hasSpawnedCount;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BeeSwarmMaster.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk && base.isServer)
		{
			StartDie();
		}
	}

	public override void ServerInit()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		if (!base.isClient)
		{
			timeSinceLastSpawnedSwarm = TimeSince.op_Implicit(secondsBetweenSpawns);
			InvokeRepeating(ThinkAI, 0f, 0.25f);
		}
	}

	private void ThinkAI()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BeeSwarmMaster.ThinkAI"))
		{
			if (!AI.effectaiweapons || (!AI.ignoreplayers && AI.think))
			{
				if (IsSmoke())
				{
					StartDie();
				}
				if (IsFire())
				{
					SetOnFire();
				}
				if (TimeSince.op_Implicit(timeSinceLastSpawnedSwarm) > killWithoutATargetTime)
				{
					StartDie();
				}
				if (TimeSince.op_Implicit(timeSinceLastSpawnedSwarm) >= secondsBetweenSpawns && (Object)(object)BeeSwarmAI.FindTarget(((Component)this).transform) != (Object)null)
				{
					SpawnSwarm();
				}
			}
		}
	}

	private void StartDie()
	{
		if (!HasFlag(Flags.Reserved13))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved13, b: true);
			}
			Invoke(ActuallyDie, 3f, 0f);
		}
	}

	private void ActuallyDie()
	{
		Die();
	}

	private void SpawnSwarm()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		timeSinceLastSpawnedSwarm = TimeSince.op_Implicit(0f);
		hasSpawnedCount++;
		if (hasSpawnedCount >= amountToSpawn)
		{
			StartDie();
		}
		else
		{
			float arg = (float)hasSpawnedCount / (float)amountToSpawn;
			if (net.group != null)
			{
				ClientRPC(RpcTarget.NetworkGroup("RPC_PopulationChange"), arg);
			}
		}
		Vector3 position = ((Component)this).transform.position;
		BaseEntity baseEntity = GameManager.server.CreateEntity(beeSwarmPrefab.resourcePath, position, Quaternion.identity);
		baseEntity.OwnerID = base.OwnerID;
		baseEntity.creatorEntity = creatorEntity;
		baseEntity.Spawn();
	}

	private void SetOnFire()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.OnFire, b: true);
		}
		StartDie();
	}

	private bool IsSmoke()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			SingletonComponent<SmokeGrenadeManager>.Instance.GetSmokeAround(((Component)this).transform.position, 5f, (List<BaseEntity>)(object)val);
			return val != null && ((List<BaseEntity>)(object)val).Count > 0;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private bool IsFire()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			SingletonComponent<NpcFireManager>.Instance.GetFiresAround(((Component)this).transform.position, 10f, (List<BaseEntity>)(object)val);
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				if (!((Object)(object)item == (Object)null) && !item.IsDestroyed && item is FlameThrower && Vector3.Distance(((Component)this).transform.position, ((Component)item).transform.position) <= BeeSwarmAI.flameSettingDistance)
				{
					return true;
				}
			}
			return false;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		if ((Object)(object)splashType == (Object)null || splashType.shortname == null)
		{
			return false;
		}
		if (HasFlag(Flags.Reserved13))
		{
			return false;
		}
		if (amount > 0)
		{
			return true;
		}
		return false;
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		float num = base.health - 10f;
		if (num > 0f)
		{
			Hurt(num);
		}
		if (base.health <= 10f)
		{
			StartDie();
		}
		return amount;
	}
}

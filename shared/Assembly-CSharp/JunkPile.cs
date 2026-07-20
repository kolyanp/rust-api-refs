using System.Collections;
using System.Collections.Generic;
using Facepunch;
using Network;
using UnityEngine;

public class JunkPile : BaseEntity
{
	public GameObjectRef sinkEffect;

	public SpawnGroup[] spawngroups;

	public NPCSpawner NPCSpawn;

	private const float lifetimeMinutes = 30f;

	private const float lifetimeJitterSeconds = 30f;

	[ServerVar]
	public static bool DestroyIfSpawnOnSleepingBag = true;

	[ServerVar]
	public static float DestroyIfSpawnOnSleepingBagTime = 4f;

	[ServerVar]
	public static float DestroyIfSpawnOnSleepingBagDistance = 3f;

	protected bool isSinking;

	private float timeWantingDespawn;

	private float timeBeforeDespawn = 90f;

	private const float CheckEmptyDelay = 30f;

	private const float DelayRandomness = 5f;

	public virtual bool DespawnIfAnyLootTaken => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("JunkPile.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		StartTimeout();
		((MonoBehaviour)this).StartCoroutine(SpawnInitialCoroutine());
		isSinking = false;
	}

	public override void Spawn()
	{
		base.Spawn();
		if (DestroyIfSpawnOnSleepingBag)
		{
			Invoke(KillIfOnSleepingBag, DestroyIfSpawnOnSleepingBagTime, DestroyIfSpawnOnSleepingBagTime * 0.5f);
		}
	}

	private void KillIfOnSleepingBag()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		List<SleepingBag> list = Pool.Get<List<SleepingBag>>();
		Vis.Entities(((Component)this).transform.position, DestroyIfSpawnOnSleepingBagDistance, list, 153092352, (QueryTriggerInteraction)2);
		if (list.Count > 0)
		{
			SpawnGroup[] array = spawngroups;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Clear();
			}
			Kill();
		}
		Pool.FreeUnmanaged<SleepingBag>(ref list);
	}

	protected virtual void StartTimeout()
	{
		Invoke(TimeOut, 1800f + Random.Range(-30f, 30f));
		InvokeRandomized(CheckEmpty, 10f, 30f, 5f);
	}

	internal override void DoServerDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		base.DoServerDestroy();
		StabilityEntity.UpdateSurroundingsQueue updateSurroundingsQueue = StabilityEntity.updateSurroundingsQueue;
		OBB val = WorldSpaceBounds();
		((ObjectWorkQueue<Bounds>)updateSurroundingsQueue).Add(((OBB)(ref val)).ToBounds());
	}

	private IEnumerator SpawnInitialCoroutine()
	{
		yield return CoroutineEx.waitForSeconds(1f);
		SpawnGroup[] array = spawngroups;
		foreach (SpawnGroup s in array)
		{
			yield return CoroutineEx.waitForEndOfFrame;
			s.SpawnInitial();
		}
	}

	public bool SpawnGroupsEmpty()
	{
		SpawnGroup[] array = spawngroups;
		foreach (SpawnGroup spawnGroup in array)
		{
			if (spawnGroup.resetBehavior == SpawnGroupResetBehavior.Exclude || (spawnGroup.DoesGroupContainNPCs() && spawnGroup.resetBehavior != SpawnGroupResetBehavior.Include))
			{
				continue;
			}
			if (DespawnIfAnyLootTaken)
			{
				if (spawnGroup.ObjectsRemoved > 0)
				{
					return true;
				}
				foreach (SpawnPointInstance spawnInstance in spawnGroup.SpawnInstances)
				{
					if (spawnInstance.Entity is LootContainer { HasBeenLooted: not false })
					{
						return true;
					}
				}
			}
			else if (spawnGroup.currentPopulation > 0)
			{
				return false;
			}
		}
		if ((Object)(object)NPCSpawn != (Object)null && NPCSpawn.currentPopulation > 0)
		{
			return false;
		}
		if (DespawnIfAnyLootTaken)
		{
			return false;
		}
		return true;
	}

	public virtual void CheckEmpty()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (SpawnGroupsEmpty() && !BaseNetworkable.HasCloseConnections(((Component)this).transform.position, TimeoutPlayerCheckRadius()))
		{
			timeWantingDespawn += 30f;
			if (timeWantingDespawn >= timeBeforeDespawn)
			{
				CancelInvoke(CheckEmpty);
				SinkAndDestroy();
			}
		}
		else
		{
			timeWantingDespawn = 0f;
		}
	}

	public virtual float TimeoutPlayerCheckRadius()
	{
		return 15f;
	}

	public void TimeOut()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (BaseNetworkable.HasCloseConnections(((Component)this).transform.position, TimeoutPlayerCheckRadius()))
		{
			Invoke(TimeOut, 30f);
			return;
		}
		SpawnGroupsEmpty();
		SinkAndDestroy();
	}

	public void SinkAndDestroy()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer)
		{
			CancelInvoke(SinkAndDestroy);
			SpawnGroup[] array = spawngroups;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Clear();
			}
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved8, b: true, recursive: true);
			}
			if ((Object)(object)NPCSpawn != (Object)null)
			{
				NPCSpawn.Clear();
			}
			ClientRPC(RpcTarget.NetworkGroup("CLIENT_StartSink"));
			Transform transform = ((Component)this).transform;
			transform.position -= new Vector3(0f, 5f, 0f);
			isSinking = true;
			Invoke(KillMe, 22f);
		}
	}

	public void KillMe()
	{
		Kill();
	}

	public static void NotifyLootContainerLooted(BaseEntity entity)
	{
	}

	public override bool ShouldUseCastNoClipChecks()
	{
		return true;
	}
}

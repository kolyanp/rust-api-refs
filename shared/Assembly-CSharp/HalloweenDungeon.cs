using System.Collections.Generic;
using Facepunch;
using ProtoBuf;
using Rust;
using UnityEngine;

public class HalloweenDungeon : BasePortal
{
	public GameObjectRef dungeonPrefab;

	public EntityRef<ProceduralDynamicDungeon> dungeonInstance;

	[ServerVar(Help = "Population active on the server", ShowInAdminUI = true)]
	public static float population = 1f;

	[ServerVar(Help = "How long each active dungeon should last before dying", ShowInAdminUI = true)]
	public static float lifetime = 600f;

	private float secondsUsed;

	private float timeAlive;

	public AnimationCurve radiationCurve;

	public Phrase collapsePhrase;

	public Phrase mountPhrase;

	private bool anyplayers_cached;

	private float nextPlayerCheckTime = float.NegativeInfinity;

	public virtual float GetLifetime()
	{
		return lifetime;
	}

	public override void Load(LoadInfo info)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.fromDisk && info.msg.ioEntity != null)
		{
			dungeonInstance.uid = info.msg.ioEntity.genericEntRef3;
			secondsUsed = info.msg.ioEntity.genericFloat1;
			timeAlive = info.msg.ioEntity.genericFloat2;
		}
	}

	public float GetLifeFraction()
	{
		return Mathf.Clamp01(secondsUsed / GetLifetime());
	}

	public void Update()
	{
		if (!isClient)
		{
			if (secondsUsed > 0f)
			{
				secondsUsed += Time.deltaTime;
			}
			timeAlive += Time.deltaTime;
			float lifeFraction = GetLifeFraction();
			if (dungeonInstance.IsValid(serverside: true))
			{
				ProceduralDynamicDungeon proceduralDynamicDungeon = dungeonInstance.Get(serverside: true);
				float num = radiationCurve.Evaluate(lifeFraction) * 80f;
				proceduralDynamicDungeon.exitRadiation.RadiationAmountOverride = Mathf.Clamp(num, 0f, float.PositiveInfinity);
			}
			if (lifeFraction >= 1f)
			{
				KillIfNoPlayers();
			}
			else if (timeAlive > 3600f && secondsUsed == 0f)
			{
				ClearAllEntitiesInDungeon();
				Kill();
			}
		}
	}

	public void KillIfNoPlayers()
	{
		if (!AnyPlayersInside())
		{
			ClearAllEntitiesInDungeon();
			Kill();
		}
	}

	public bool AnyPlayersInside()
	{
		ProceduralDynamicDungeon proceduralDynamicDungeon = dungeonInstance.Get(serverside: true);
		if ((Object)(object)proceduralDynamicDungeon == (Object)null)
		{
			anyplayers_cached = false;
		}
		else if (Time.time > nextPlayerCheckTime)
		{
			nextPlayerCheckTime = Time.time + 10f;
			anyplayers_cached = proceduralDynamicDungeon.ContainsAnyPlayers(includeSleepers: false);
		}
		return anyplayers_cached;
	}

	private void ClearAllEntitiesInDungeon()
	{
		ProceduralDynamicDungeon proceduralDynamicDungeon = dungeonInstance.Get(serverside: true);
		if ((Object)(object)proceduralDynamicDungeon == (Object)null)
		{
			return;
		}
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		proceduralDynamicDungeon.GetEntitiesInside(list);
		foreach (BaseEntity item in list)
		{
			if (item.IsValid() && !item.IsDestroyed)
			{
				if (item is LootableCorpse lootableCorpse)
				{
					lootableCorpse.blockBagDrop = true;
				}
				item.Kill();
			}
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
	}

	public override void Save(SaveInfo info)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (info.msg.ioEntity == null)
		{
			info.msg.ioEntity = Pool.Get<IOEntity>();
		}
		info.msg.ioEntity.genericEntRef3 = dungeonInstance.uid;
		info.msg.ioEntity.genericFloat1 = secondsUsed;
		info.msg.ioEntity.genericFloat2 = timeAlive;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		timeAlive += Random.Range(0f, 60f);
	}

	public override void UsePortal(BasePlayer player)
	{
		if (GetLifeFraction() > 0.8f)
		{
			player.ShowToast(GameTip.Styles.Blue_Normal, collapsePhrase, false);
			return;
		}
		if (player.isMounted)
		{
			player.ShowToast(GameTip.Styles.Blue_Normal, mountPhrase, false);
			return;
		}
		if (secondsUsed == 0f)
		{
			secondsUsed = 1f;
		}
		base.UsePortal(player);
	}

	public override void Spawn()
	{
		base.Spawn();
	}

	public override void ServerInit()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		if (!Application.isLoadingSave)
		{
			timeAlive = Random.Range(0f, 60f);
			SpawnSubEntities();
		}
		TransformEx.DropToGround(localEntryExitPos, alignToNormal: false, 10f);
		Transform transform = ((Component)localEntryExitPos).transform;
		transform.position += Vector3.up * 0.05f;
		Invoke(CheckBlocked, 0.25f);
	}

	public void CheckBlocked()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)GetBuildingPrivilege(cached: true) != (Object)null)
		{
			Kill();
			return;
		}
		float num = 0.5f;
		float num2 = 1.8f;
		Vector3 position = localEntryExitPos.position;
		Vector3 val = position + new Vector3(0f, num, 0f);
		Vector3 val2 = position + new Vector3(0f, num2 - num, 0f);
		if (Physics.CheckCapsule(val, val2, num, 1537286401))
		{
			Kill();
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		if (dungeonInstance.IsValid(serverside: true))
		{
			dungeonInstance.Get(serverside: true).Kill();
		}
	}

	public void DelayedDestroy()
	{
		Kill();
	}

	public void SpawnSubEntities()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (!ProceduralDynamicDungeon.TryFindFreeNetworkGroup(out var spawnPoint))
		{
			Debug.LogError((object)"No dungeon spawn point");
			Invoke(DelayedDestroy, 5f);
			return;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(dungeonPrefab.resourcePath, spawnPoint, Quaternion.identity);
		ProceduralDynamicDungeon component = ((Component)baseEntity).GetComponent<ProceduralDynamicDungeon>();
		component.mapOffset = ((Component)this).transform.position - spawnPoint;
		baseEntity.Spawn();
		dungeonInstance.Set(component);
		BasePortal basePortal = (targetPortal = component.GetExitPortal());
		basePortal.targetPortal = this;
		LinkPortal();
		basePortal.LinkPortal();
	}
}

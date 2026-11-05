using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using UnityEngine;

[ExecuteInEditMode]
public class TreeEntity : ResourceEntity, IPrefabPreProcess
{
	[Serializable]
	public struct TreeAdditionSpawnData
	{
		public GameObjectRef prefab;

		public Vector3 localPosition;

		public Vector3 localRotation;
	}

	private struct HotspotMarkerSpawnValues
	{
		public Vector3 fromWorldPosition;

		public Vector3 hitPositionWorld;

		public Vector3 initiatorCenterPoint;

		public bool isBypassingBonusGame;

		public TreeEntity hitEntity;
	}

	[Header("Falling")]
	public bool fallOnDied = true;

	public float fallDuration = 1.5f;

	public GameObjectRef fallStartSound;

	public GameObjectRef fallImpactSound;

	public GameObjectRef fallImpactParticles;

	public SoundDefinition fallLeavesLoopDef;

	[NonSerialized]
	public bool[] usedHeights = new bool[20];

	public bool impactSoundPlayed;

	private float treeDistanceUponFalling;

	public GameObjectRef prefab;

	public bool hasBonusGame = true;

	public GameObjectRef bonusHitEffect;

	public GameObjectRef bonusHitSound;

	public Collider serverCollider;

	public Collider clientCollider;

	public SoundDefinition smallCrackSoundDef;

	public SoundDefinition medCrackSoundDef;

	private float lastAttackDamage;

	[Header("Tree Addition Settings")]
	public TreeAdditionSpawnData[] treeAdditions;

	private List<BaseEntity> treeAdditionRefs;

	public float treeAdditionSpawnChance = 0.1f;

	public float treeAdditionDestroyOnDeathChance;

	private HotspotMarkerSpawnValues nextHotspotMarkerValues;

	private Action _actionCreateNewHotspotMarker;

	public BaseEntity xMarker;

	private int currentBonusLevel;

	private float lastDirection = -1f;

	private float lastHitTime;

	private int lastHitMarkerIndex = -1;

	private float nextBirdTime;

	private uint birdCycleIndex;

	public virtual bool IncludeInNavmesh => false;

	private Action actionCreateNewHotspotMarker
	{
		get
		{
			if (_actionCreateNewHotspotMarker == null)
			{
				_actionCreateNewHotspotMarker = SpawnNewHotspotMarker;
			}
			return _actionCreateNewHotspotMarker;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("TreeEntity.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	public override float AntiHackPadding()
	{
		return 1f;
	}

	public override void OnAttacked(HitInfo info)
	{
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		bool canGather = info.CanGather;
		float time = Time.time;
		float num = time - lastHitTime;
		lastHitTime = time;
		DoBirds();
		bool flag = false;
		float num2 = 1f;
		if ((Object)(object)info.Weapon != (Object)null && info.Weapon.TryGetOwnerPlayer(out var ownerPlayer) && ResourceGatherItemConfig.instance.CanItemBypassHotspotGathering(resourceDispenser.gatherType, info.Weapon.GetOwnerItemDefinition(ownerPlayer), out var itemData))
		{
			num2 = itemData.hotspotGatherBonusScale;
			flag = true;
		}
		if (!hasBonusGame || !canGather || (Object)(object)info.Initiator == (Object)null || (BonusActive() && !flag && !DidHitMarker(info)))
		{
			base.OnAttacked(info);
			return;
		}
		bool flag2 = (Object)(object)xMarker != (Object)null;
		if ((flag | flag2) && !info.DidGather && info.gatherScale > 0f)
		{
			Vector3 arg;
			Vector3 arg2;
			if (flag2 && !flag)
			{
				arg = ((Component)xMarker).transform.position;
				arg2 = ((Component)xMarker).transform.up;
			}
			else
			{
				arg = info.HitPositionWorld;
				arg2 = info.HitNormalWorld;
			}
			ClientRPC(RpcTarget.NetworkGroup("HotspotHit"), arg, arg2, currentBonusLevel);
			currentBonusLevel++;
			info.gatherScale = 1f + Mathf.Clamp((float)currentBonusLevel * 0.125f, 0f, 1f * num2);
		}
		Vector3 fromWorldPosition = (flag2 ? ((Component)xMarker).transform.position : info.HitPositionWorld);
		CleanupMarker();
		nextHotspotMarkerValues = new HotspotMarkerSpawnValues
		{
			fromWorldPosition = fromWorldPosition,
			hitPositionWorld = info.HitPositionWorld,
			initiatorCenterPoint = info.Initiator.CenterPoint(),
			isBypassingBonusGame = flag,
			hitEntity = this
		};
		if (num > 5f)
		{
			StartBonusGame();
		}
		base.OnAttacked(info);
		if (health > 0f)
		{
			if (!flag)
			{
				SpawnNewHotspotMarker();
			}
			lastAttackDamage = info.damageTypes.Total();
			int num3 = Mathf.CeilToInt(health / lastAttackDamage);
			if (num3 < 2)
			{
				ClientRPC(RpcTarget.NetworkGroup("CrackSound"), 1);
			}
			else if (num3 < 5)
			{
				ClientRPC(RpcTarget.NetworkGroup("CrackSound"), 0);
			}
		}
	}

	public override void ServerInit()
	{
		if ((Object)(object)serverCollider == (Object)null)
		{
			serverCollider = clientCollider ?? ((Component)this).GetComponentInChildren<Collider>();
		}
		base.ServerInit();
		lastDirection = ((Random.Range(0, 2) != 0) ? 1 : (-1));
		TryAddTreeAddition();
	}

	public override void ServerInitPostNetworkGroupAssign()
	{
		base.ServerInitPostNetworkGroupAssign();
		TreeManager.OnTreeSpawned(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		CleanupMarker();
		TryKillTreeAddition();
		TreeManager.OnTreeDestroyed(this);
	}

	public bool DidHitMarker(HitInfo info)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)xMarker == (Object)null)
		{
			return false;
		}
		object obj = Interface.CallHook("OnTreeMarkerHit", this, info);
		if (obj is bool)
		{
			return (bool)obj;
		}
		TreeMarkerData treeMarkerData = PrefabAttribute.server.Find<TreeMarkerData>(prefabID);
		if (treeMarkerData != null)
		{
			Bounds val = new Bounds(((Component)xMarker).transform.position, Vector3.one * (0.2f + treeMarkerData.HotspotPadding));
			if (val.Contains(info.HitPositionWorld))
			{
				return true;
			}
		}
		else
		{
			Vector3 val2 = Vector3Ex.Direction2D(((Component)this).transform.position, ((Component)xMarker).transform.position);
			Vector3 attackNormal = info.attackNormal;
			float num = Vector3.Dot(val2, attackNormal);
			float num2 = Vector3.SqrMagnitude(((Component)xMarker).transform.position - info.HitPositionWorld);
			if (num >= 0.3f && num2 <= 0.040000003f)
			{
				return true;
			}
		}
		return false;
	}

	private void SpawnNewHotspotMarker()
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		if (health <= 0f)
		{
			return;
		}
		TreeMarkerData treeMarkerData = PrefabAttribute.server.Find<TreeMarkerData>(prefabID);
		if (treeMarkerData != null)
		{
			if (!(nextHotspotMarkerValues.isBypassingBonusGame ? treeMarkerData.TryGetClosestPoint(nextHotspotMarkerValues.hitPositionWorld, nextHotspotMarkerValues.hitEntity, out var closestPoint, out var normal) : treeMarkerData.TryGetNearbyPoint(nextHotspotMarkerValues.fromWorldPosition, nextHotspotMarkerValues.hitEntity, ref lastHitMarkerIndex, out closestPoint, out normal)))
			{
				return;
			}
			closestPoint = ((Component)this).transform.TransformPoint(closestPoint);
			Quaternion rot = QuaternionEx.LookRotationNormal(((Component)this).transform.TransformDirection(normal));
			xMarker = GameManager.server.CreateEntity("assets/content/nature/treesprefabs/trees/effects/tree_marking_nospherecast.prefab", closestPoint, rot);
		}
		else
		{
			Vector3 val = Vector3Ex.Direction2D(((Component)this).transform.position, nextHotspotMarkerValues.fromWorldPosition);
			Vector3 val2;
			if (nextHotspotMarkerValues.isBypassingBonusGame)
			{
				val2 = val;
			}
			else
			{
				Vector3 val3 = Vector3.Cross(val, Vector3.up);
				float num = lastDirection;
				float num2 = Random.Range(0.5f, 0.5f);
				val2 = Vector3.Lerp(-val, val3 * num, num2);
			}
			Vector3 val4 = ((Component)this).transform.InverseTransformDirection(val2.normalized) * 2.5f;
			val4 = ((Component)this).transform.InverseTransformPoint(serverCollider.ClosestPoint(((Component)this).transform.TransformPoint(val4)));
			Vector3 val5 = ((Component)this).transform.TransformPoint(val4);
			Vector3 val6 = ((Component)this).transform.InverseTransformPoint(nextHotspotMarkerValues.hitPositionWorld);
			val4.y = val6.y;
			Vector3 val7 = ((Component)this).transform.InverseTransformPoint(nextHotspotMarkerValues.initiatorCenterPoint);
			float num3 = Mathf.Max(0.75f, val7.y);
			float num4 = val7.y + 0.5f;
			val4.y = Mathf.Clamp(val4.y + Random.Range(0.1f, 0.2f) * ((Random.Range(0, 2) == 0) ? (-1f) : 1f), num3, num4);
			Vector3 val8 = Vector3Ex.Direction2D(((Component)this).transform.position, val5);
			val8 = ((Component)this).transform.InverseTransformDirection(val8);
			Quaternion val9 = QuaternionEx.LookRotationNormal(-val8, Vector3.zero);
			val4 = ((Component)this).transform.TransformPoint(val4);
			val4 = serverCollider.ClosestPoint(val4);
			if (!TreeMarkerData.IsAboveTerrain(val4))
			{
				return;
			}
			Line val10 = new Line(((Component)serverCollider).transform.TransformPoint(new Vector3(0f, 10f, 0f)), ((Component)serverCollider).transform.TransformPoint(new Vector3(0f, -10f, 0f)));
			val9 = QuaternionEx.LookRotationNormal(-Vector3Ex.Direction(val10.ClosestPoint(val4), val4));
			xMarker = GameManager.server.CreateEntity("assets/content/nature/treesprefabs/trees/effects/tree_marking.prefab", val4, val9);
		}
		xMarker.Spawn();
	}

	private void DelayedHotspotMarkerSpawn()
	{
		CancelInvoke(actionCreateNewHotspotMarker);
		Invoke(actionCreateNewHotspotMarker, 0.5f);
	}

	public float GetLastHitTime()
	{
		return lastHitTime;
	}

	public void StartBonusGame()
	{
		if (IsInvoking(StopBonusGame))
		{
			CancelInvoke(StopBonusGame);
		}
		Invoke(StopBonusGame, 60f);
	}

	public void StopBonusGame()
	{
		CleanupMarker();
		lastHitTime = 0f;
		currentBonusLevel = 0;
	}

	public bool BonusActive()
	{
		return (Object)(object)xMarker != (Object)null;
	}

	private void DoBirds()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (!isClient && !(Time.realtimeSinceStartup < nextBirdTime) && !(bounds.extents.y < 6f))
		{
			uint num = (uint)(int)net.ID.Value + birdCycleIndex;
			if (SeedRandom.Range(ref num, 0, 2) == 0)
			{
				Effect.server.Run("assets/prefabs/npc/birds/birdemission.prefab", ((Component)this).transform.position + Vector3.up * Random.Range(bounds.extents.y * 0.65f, bounds.extents.y * 0.9f), Vector3.up);
			}
			birdCycleIndex++;
			nextBirdTime = Time.realtimeSinceStartup + 90f;
		}
	}

	public void CleanupMarker()
	{
		if (Object.op_Implicit((Object)(object)xMarker))
		{
			xMarker.Kill();
		}
		xMarker = null;
	}

	public override void OnDied(HitInfo info)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		if (isKilled)
		{
			return;
		}
		isKilled = true;
		CleanupMarker();
		if (isServer)
		{
			StabilityEntity.UpdateSurroundingsQueue updateSurroundingsQueue = StabilityEntity.updateSurroundingsQueue;
			OBB val = WorldSpaceBounds();
			((ObjectWorkQueue<Bounds>)updateSurroundingsQueue).Add(val.ToBounds());
			TryKillTreeAddition();
		}
		if (fallOnDied)
		{
			Collider val2 = serverCollider;
			if (Object.op_Implicit((Object)(object)val2))
			{
				val2.enabled = false;
			}
			Vector3 val3 = info.attackNormal;
			if (val3 == Vector3.zero)
			{
				val3 = Vector3Ex.Direction2D(((Component)this).transform.position, info.PointStart);
			}
			PooledList<TimedExplosive> val4 = Pool.Get<PooledList<TimedExplosive>>();
			try
			{
				foreach (BaseEntity child in children)
				{
					if (child is TimedExplosive item)
					{
						((List<TimedExplosive>)(object)val4).Add(item);
					}
				}
				foreach (TimedExplosive item2 in (List<TimedExplosive>)(object)val4)
				{
					item2.UnStick();
				}
				OnFallServer();
				ClientRPC(RpcTarget.NetworkGroup("TreeFall"), val3);
				Invoke(DelayedKill, fallDuration + 1f);
				return;
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
		}
		DelayedKill();
	}

	protected virtual void OnFallServer()
	{
	}

	public void DelayedKill()
	{
		Kill();
	}

	private void TryAddTreeAddition()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (treeAdditions == null || treeAdditions.Length == 0 || Random.value > treeAdditionSpawnChance)
		{
			return;
		}
		treeAdditionRefs = Pool.Get<List<BaseEntity>>();
		for (int i = 0; i < treeAdditions.Length; i++)
		{
			if (treeAdditions[i].prefab.isValid)
			{
				BaseEntity baseEntity = GameManager.server.CreateEntity(treeAdditions[i].prefab.resourcePath, ((Component)this).transform.TransformPoint(treeAdditions[i].localPosition), ((Component)this).transform.rotation * Quaternion.Euler(treeAdditions[i].localRotation));
				if (!((Object)(object)baseEntity == (Object)null))
				{
					baseEntity.Spawn();
					treeAdditionRefs.Add(baseEntity);
				}
			}
		}
	}

	private void TryKillTreeAddition()
	{
		if (treeAdditionRefs == null)
		{
			return;
		}
		foreach (BaseEntity treeAdditionRef in treeAdditionRefs)
		{
			if (!((Object)(object)treeAdditionRef == (Object)null))
			{
				if (treeAdditionRef is NaturalBeehive naturalBeehive)
				{
					naturalBeehive.Die();
				}
				else if (treeAdditionRef is BaseCombatEntity baseCombatEntity && Random.value >= treeAdditionDestroyOnDeathChance)
				{
					baseCombatEntity.Die();
				}
				else
				{
					treeAdditionRef.Kill(DestroyMode.Gib);
				}
			}
		}
		Pool.FreeUnmanaged<BaseEntity>(ref treeAdditionRefs);
	}

	public BaseEntity GetBonusGame()
	{
		return xMarker;
	}

	public override void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.PreProcess(preProcess, rootObj, name, serverside, clientside, bundling);
		if (serverside)
		{
			globalBroadcast = Tree.global_broadcast;
		}
	}
}

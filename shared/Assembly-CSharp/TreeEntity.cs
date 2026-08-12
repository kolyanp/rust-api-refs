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
	public bool spawnTreeAddition;

	public GameObjectRef treeAdditionPrefab;

	public float treeAdditionSpawnChance = 0.1f;

	public Vector3 treeAdditionSpawnPosition;

	public Vector3 treeAdditionSpawnRotation;

	private BaseEntity treeAdditionRef;

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

	bool IPrefabPreProcess.CanRunDuringBundling => false;

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
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)xMarker == (Object)null)
		{
			return false;
		}
		object obj = Interface.CallHook("OnTreeMarkerHit", this, info);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (PrefabAttribute.server.Find<TreeMarkerData>(prefabID) != null)
		{
			Bounds val = default(Bounds);
			((Bounds)(ref val))._002Ector(((Component)xMarker).transform.position, Vector3.one * 0.2f);
			if (((Bounds)(ref val)).Contains(info.HitPositionWorld))
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
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		if (health <= 0f)
		{
			return;
		}
		TreeMarkerData treeMarkerData = PrefabAttribute.server.Find<TreeMarkerData>(prefabID);
		if (treeMarkerData != null)
		{
			Vector3 normal;
			Vector3 closestPoint;
			if (nextHotspotMarkerValues.isBypassingBonusGame)
			{
				if (!treeMarkerData.TryGetClosestPoint(nextHotspotMarkerValues.hitPositionWorld, nextHotspotMarkerValues.hitEntity, out closestPoint, out normal))
				{
					Debug.LogError((object)$"Failed to generate a closest point to spawn new marker from position {nextHotspotMarkerValues.fromWorldPosition}");
					return;
				}
			}
			else
			{
				closestPoint = treeMarkerData.GetNearbyPoint(nextHotspotMarkerValues.fromWorldPosition, nextHotspotMarkerValues.hitEntity, ref lastHitMarkerIndex, out normal);
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
			Vector3 val4 = ((Component)this).transform.InverseTransformDirection(((Vector3)(ref val2)).normalized) * 2.5f;
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
			Line val10 = default(Line);
			((Line)(ref val10))._002Ector(((Component)serverCollider).transform.TransformPoint(new Vector3(0f, 10f, 0f)), ((Component)serverCollider).transform.TransformPoint(new Vector3(0f, -10f, 0f)));
			val9 = QuaternionEx.LookRotationNormal(-Vector3Ex.Direction(((Line)(ref val10)).ClosestPoint(val4), val4));
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
		if (!base.isClient && !(Time.realtimeSinceStartup < nextBirdTime) && !(((Bounds)(ref bounds)).extents.y < 6f))
		{
			uint num = (uint)(int)net.ID.Value + birdCycleIndex;
			if (SeedRandom.Range(ref num, 0, 2) == 0)
			{
				Effect.server.Run("assets/prefabs/npc/birds/birdemission.prefab", ((Component)this).transform.position + Vector3.up * Random.Range(((Bounds)(ref bounds)).extents.y * 0.65f, ((Bounds)(ref bounds)).extents.y * 0.9f), Vector3.up);
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
		if (base.isServer)
		{
			StabilityEntity.UpdateSurroundingsQueue updateSurroundingsQueue = StabilityEntity.updateSurroundingsQueue;
			OBB val = WorldSpaceBounds();
			((ObjectWorkQueue<Bounds>)updateSurroundingsQueue).Add(((OBB)(ref val)).ToBounds());
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
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if (spawnTreeAddition && treeAdditionPrefab.isValid && Random.value <= treeAdditionSpawnChance && !((Object)(object)treeAdditionRef != (Object)null))
		{
			treeAdditionRef = GameManager.server.CreateEntity(treeAdditionPrefab.resourcePath, Vector3.zero, Quaternion.identity);
			((Component)treeAdditionRef).transform.position = ((Component)this).transform.TransformPoint(treeAdditionSpawnPosition);
			((Component)treeAdditionRef).transform.rotation = ((Component)this).transform.rotation * Quaternion.Euler(treeAdditionSpawnRotation);
			if ((Object)(object)((Component)treeAdditionRef).GetComponent<Poolable>() != (Object)null)
			{
				PoolableEx.AwakeFromInstantiate(((Component)treeAdditionRef).gameObject);
			}
			treeAdditionRef.Spawn();
			treeAdditionRef.SendNetworkUpdate();
		}
	}

	private void TryKillTreeAddition()
	{
		if (spawnTreeAddition && (Object)(object)treeAdditionRef != (Object)null)
		{
			if (treeAdditionRef is BaseCombatEntity baseCombatEntity)
			{
				baseCombatEntity.Die();
			}
			else
			{
				treeAdditionRef.Kill(DestroyMode.Gib);
			}
			treeAdditionRef = null;
		}
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

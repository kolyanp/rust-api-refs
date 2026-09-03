using System;
using ConVar;
using Network;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

public class OreResourceEntity : StagedResourceEntity
{
	[Header("Ore Resource Entity")]
	public GameObjectRef bonusPrefab;

	public GameObjectRef finishEffect;

	public GameObjectRef bonusFailEffect;

	public bool useHotspotMinigame;

	public SoundPlayer bonusSound;

	public float heightOffset;

	private int bonusesKilled;

	public int bonusesSpawned;

	public OreHotSpot _hotSpot;

	private Action _actionRespawnBonus;

	public Vector3 lastNodeDir;

	private Ray? spawnBonusHitRay;

	private Action actionRespawnBonus
	{
		get
		{
			if (_actionRespawnBonus == null)
			{
				_actionRespawnBonus = RespawnBonus;
			}
			return _actionRespawnBonus;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("OreResourceEntity.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnAttacked(HitInfo info)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		if (!info.DidGather && info.gatherScale > 0f)
		{
			if (useHotspotMinigame)
			{
				bool flag = (Object)(object)_hotSpot == (Object)null;
				if (flag)
				{
					_hotSpot = SpawnBonusSpotOnRay(new Ray(info.PointStart, info.attackNormal));
				}
				float num = 0f;
				bool flag2 = false;
				float num2 = 1f;
				if ((Object)(object)info.Weapon != (Object)null && info.Weapon.TryGetOwnerPlayer(out var ownerPlayer) && ResourceGatherItemConfig.instance.CanItemBypassHotspotGathering(resourceDispenser.gatherType, info.Weapon.GetOwnerItemDefinition(ownerPlayer), out var itemData))
				{
					num2 = itemData.hotspotGatherBonusScale;
					flag2 = true;
				}
				else
				{
					num = (((Object)(object)_hotSpot != (Object)null) ? Vector3.Distance(info.HitPositionWorld, ((Component)_hotSpot).transform.position) : float.MaxValue);
				}
				if ((flag | flag2) || num <= ((Component)_hotSpot).GetComponent<SphereCollider>().radius * 1.5f)
				{
					bonusesKilled++;
					info.gatherScale = 1f + Mathf.Clamp((float)bonusesKilled * 0.5f, 0f, 2f * num2);
					if ((Object)(object)_hotSpot != (Object)null)
					{
						_hotSpot.FireFinishEffect();
						ClientRPC(RpcTarget.NetworkGroup("PlayBonusLevelSound"), bonusesKilled, ((Component)_hotSpot).transform.position);
					}
				}
				else if (bonusesKilled > 0)
				{
					bonusesKilled = 0;
					Effect.server.Run(bonusFailEffect.resourcePath, ((Component)this).transform.position, ((Component)this).transform.up);
				}
				if (bonusesKilled > 0)
				{
					CleanupBonus();
				}
				if ((Object)(object)_hotSpot == (Object)null)
				{
					if (flag)
					{
						spawnBonusHitRay = new Ray(info.PointStart, info.attackNormal);
						Vector3 val = info.PointStart - (((Component)this).transform.position + new Vector3(0f, 0.5f, 0f));
						lastNodeDir = ((Vector3)(ref val)).normalized;
						float num3 = 0.5f;
						if (lastNodeDir.y > num3)
						{
							float num4 = lastNodeDir.y - num3;
							lastNodeDir.y = num4;
							lastNodeDir.x += num4;
							lastNodeDir.z += num4;
						}
						lastNodeDir = ((Component)this).transform.InverseTransformDirection(lastNodeDir);
					}
					DelayedBonusSpawn();
				}
			}
			else
			{
				info.gatherScale = 1f;
			}
		}
		base.OnAttacked(info);
	}

	public OreHotSpot GetHotSpot()
	{
		return _hotSpot;
	}

	public override void UpdateNetworkStage()
	{
		int num = stage;
		base.UpdateNetworkStage();
		if (stage != num && Object.op_Implicit((Object)(object)_hotSpot) && useHotspotMinigame)
		{
			DelayedBonusSpawn();
		}
	}

	public void CleanupBonus()
	{
		if (Object.op_Implicit((Object)(object)_hotSpot))
		{
			_hotSpot.Kill();
		}
		_hotSpot = null;
	}

	public override void ServerInit()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		if (!AI.useUnityNavmesh)
		{
			RustNavigation instance = RustNavigation.Instance;
			OBB val = WorldSpaceBounds();
			instance.RebuildTilesInBounds(((OBB)(ref val)).ToBounds());
		}
	}

	public override void DestroyShared()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		base.DestroyShared();
		CleanupBonus();
		if (base.isServer && !AI.useUnityNavmesh)
		{
			RustNavigation instance = RustNavigation.Instance;
			OBB val = WorldSpaceBounds();
			instance.RebuildTilesInBounds(((OBB)(ref val)).ToBounds());
		}
	}

	public override void OnDied(HitInfo info)
	{
		CleanupBonus();
		base.OnDied(info);
	}

	public void FinishBonusAssigned()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Effect.server.Run(finishEffect.resourcePath, ((Component)this).transform.position, ((Component)this).transform.up);
	}

	public void DelayedBonusSpawn()
	{
		CancelInvoke(actionRespawnBonus);
		Invoke(actionRespawnBonus, 0.25f);
	}

	public void RespawnBonus()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		CleanupBonus();
		if (spawnBonusHitRay.HasValue)
		{
			_hotSpot = SpawnBonusSpotOnRay(spawnBonusHitRay.Value);
			spawnBonusHitRay = null;
		}
		else
		{
			_hotSpot = SpawnBonusSpot(lastNodeDir);
		}
	}

	private OreHotSpot SpawnBonusSpotOnRay(Ray r)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (base.isClient)
		{
			return null;
		}
		if (!useHotspotMinigame)
		{
			return null;
		}
		if (!bonusPrefab.isValid)
		{
			return null;
		}
		RaycastHit val = default(RaycastHit);
		if (((Collider)ResourceMeshColliders[0]).Raycast(r, ref val, 15f))
		{
			OreHotSpot obj = GameManager.server.CreateEntity(bonusPrefab.resourcePath, ((RaycastHit)(ref val)).point - ((Ray)(ref r)).direction * 0.025f, Quaternion.LookRotation(((RaycastHit)(ref val)).normal, Vector3.up)) as OreHotSpot;
			obj.Spawn();
			obj.OreOwner(this);
			return obj;
		}
		return SpawnBonusSpot(lastNodeDir);
	}

	public OreHotSpot SpawnBonusSpot(Vector3 lastDirection)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		if (base.isClient)
		{
			return null;
		}
		if (!useHotspotMinigame)
		{
			return null;
		}
		if (!bonusPrefab.isValid)
		{
			return null;
		}
		Vector3 zero = Vector3.zero;
		MeshCollider val = ResourceMeshColliders[0];
		Transform transform = ((Component)this).transform;
		Bounds val2 = ((Collider)val).bounds;
		Vector3 val3 = transform.InverseTransformPoint(((Bounds)(ref val2)).center);
		Vector3 val7;
		if (lastDirection == Vector3.zero)
		{
			float num = 0.5f;
			if (heightOffset != 0f)
			{
				num = heightOffset;
			}
			Vector3 val4 = RandomCircle();
			Vector3 val5 = (lastNodeDir = ((Component)this).transform.TransformDirection(((Vector3)(ref val4)).normalized));
			val4 = ((Component)this).transform.position + ((Component)this).transform.up * (val3.y + num) + ((Vector3)(ref val5)).normalized * 5f;
			zero = val4;
		}
		else
		{
			Vector3 val6 = Vector3.Cross(lastNodeDir, Vector3.up);
			float num2 = Random.Range(0.25f, 0.5f) + (float)stage * 0.25f;
			float num3 = ((Random.Range(0, 2) == 0) ? (-1f) : 1f);
			val7 = lastNodeDir + val6 * (num2 * num3);
			Vector3 val8 = (lastNodeDir = ((Vector3)(ref val7)).normalized);
			zero = ((Component)this).transform.position + ((Component)this).transform.TransformDirection(val8) * 2f;
			float num4 = Random.Range(1f, 1.5f);
			zero += ((Component)this).transform.up * (val3.y + num4);
		}
		bonusesSpawned++;
		val2 = ((Collider)val).bounds;
		val7 = ((Bounds)(ref val2)).center - zero;
		Vector3 normalized = ((Vector3)(ref val7)).normalized;
		RaycastHit val9 = default(RaycastHit);
		if (((Collider)val).Raycast(new Ray(zero, normalized), ref val9, 15f))
		{
			OreHotSpot obj = GameManager.server.CreateEntity(bonusPrefab.resourcePath, ((RaycastHit)(ref val9)).point - normalized * 0.025f, Quaternion.LookRotation(((RaycastHit)(ref val9)).normal, Vector3.up)) as OreHotSpot;
			obj.Spawn();
			obj.OreOwner(this);
			return obj;
		}
		return null;
	}

	public Vector3 RandomCircle(float distance = 1f, bool allowInside = false)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val;
		if (!allowInside)
		{
			Vector2 insideUnitCircle = Random.insideUnitCircle;
			val = ((Vector2)(ref insideUnitCircle)).normalized;
		}
		else
		{
			val = Random.insideUnitCircle;
		}
		Vector2 val2 = val;
		return new Vector3(val2.x, 0f, val2.y);
	}

	public Vector3 RandomHemisphereDirection(Vector3 input, float degreesOffset, bool allowInside = true, bool changeHeight = true)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		degreesOffset = Mathf.Clamp(degreesOffset / 180f, -180f, 180f);
		Vector2 val;
		if (!allowInside)
		{
			Vector2 insideUnitCircle = Random.insideUnitCircle;
			val = ((Vector2)(ref insideUnitCircle)).normalized;
		}
		else
		{
			val = Random.insideUnitCircle;
		}
		Vector2 val2 = val;
		Vector3 val3 = default(Vector3);
		((Vector3)(ref val3))._002Ector(val2.x * degreesOffset, changeHeight ? (Random.Range(-1f, 1f) * degreesOffset) : 0f, val2.y * degreesOffset);
		Vector3 val4 = input + val3;
		return ((Vector3)(ref val4)).normalized;
	}

	public Vector3 ClampToHemisphere(Vector3 hemiInput, float degreesOffset, Vector3 inputVec)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		degreesOffset = Mathf.Clamp(degreesOffset / 180f, -180f, 180f);
		Vector3 val = hemiInput + Vector3.one * degreesOffset;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		val = hemiInput + Vector3.one * (0f - degreesOffset);
		Vector3 normalized2 = ((Vector3)(ref val)).normalized;
		for (int i = 0; i < 3; i++)
		{
			((Vector3)(ref inputVec))[i] = Mathf.Clamp(((Vector3)(ref inputVec))[i], ((Vector3)(ref normalized2))[i], ((Vector3)(ref normalized))[i]);
		}
		return inputVec;
	}

	public static Vector3 RandomCylinderPointAroundVector(Vector3 input, float distance, float minHeight = 0f, float maxHeight = 0f, bool allowInside = false)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val;
		if (!allowInside)
		{
			Vector2 insideUnitCircle = Random.insideUnitCircle;
			val = ((Vector2)(ref insideUnitCircle)).normalized;
		}
		else
		{
			val = Random.insideUnitCircle;
		}
		Vector2 val2 = val;
		Vector3 val3 = new Vector3(val2.x, 0f, val2.y);
		Vector3 result = ((Vector3)(ref val3)).normalized * distance;
		result.y = Random.Range(minHeight, maxHeight);
		return result;
	}

	public Vector3 ClampToCylinder(Vector3 localPos, Vector3 cylinderAxis, float cylinderDistance, float minHeight = 0f, float maxHeight = 0f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.zero;
	}

	public OreResourceEntity()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		useHotspotMinigame = true;
		lastNodeDir = Vector3.zero;
		base._002Ector();
	}
}

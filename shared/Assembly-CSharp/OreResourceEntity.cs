using System;
using ConVar;
using Network;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

public class OreResourceEntity : StagedResourceEntity
{
	public GameObjectRef bonusPrefab;

	public GameObjectRef finishEffect;

	public GameObjectRef bonusFailEffect;

	public bool useHotspotMinigame = true;

	public SoundPlayer bonusSound;

	public float heightOffset;

	private int bonusesKilled;

	public int bonusesSpawned;

	public OreHotSpot _hotSpot;

	private Action _actionRespawnBonus;

	public Vector3 lastNodeDir = Vector3.zero;

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
				if (flag || flag2 || num <= ((Component)_hotSpot).GetComponent<SphereCollider>().radius * 1.5f)
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
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
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
		if (((Collider)ResourceMeshCollider).Raycast(r, ref val, 15f))
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
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
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
		MeshCollider resourceMeshCollider = ResourceMeshCollider;
		Transform transform = ((Component)this).transform;
		Bounds val = ((Collider)resourceMeshCollider).bounds;
		Vector3 val2 = transform.InverseTransformPoint(((Bounds)(ref val)).center);
		Vector3 val6;
		if (lastDirection == Vector3.zero)
		{
			float num = 0.5f;
			if (heightOffset != 0f)
			{
				num = heightOffset;
			}
			Vector3 val3 = RandomCircle();
			Vector3 val4 = (lastNodeDir = ((Component)this).transform.TransformDirection(((Vector3)(ref val3)).normalized));
			val3 = ((Component)this).transform.position + ((Component)this).transform.up * (val2.y + num) + ((Vector3)(ref val4)).normalized * 5f;
			zero = val3;
		}
		else
		{
			Vector3 val5 = Vector3.Cross(lastNodeDir, Vector3.up);
			float num2 = Random.Range(0.25f, 0.5f) + (float)stage * 0.25f;
			float num3 = ((Random.Range(0, 2) == 0) ? (-1f) : 1f);
			val6 = lastNodeDir + val5 * (num2 * num3);
			Vector3 val7 = (lastNodeDir = ((Vector3)(ref val6)).normalized);
			zero = ((Component)this).transform.position + ((Component)this).transform.TransformDirection(val7) * 2f;
			float num4 = Random.Range(1f, 1.5f);
			zero += ((Component)this).transform.up * (val2.y + num4);
		}
		bonusesSpawned++;
		val = ((Collider)resourceMeshCollider).bounds;
		val6 = ((Bounds)(ref val)).center - zero;
		Vector3 normalized = ((Vector3)(ref val6)).normalized;
		RaycastHit val8 = default(RaycastHit);
		if (((Collider)resourceMeshCollider).Raycast(new Ray(zero, normalized), ref val8, 15f))
		{
			OreHotSpot obj = GameManager.server.CreateEntity(bonusPrefab.resourcePath, ((RaycastHit)(ref val8)).point - normalized * 0.025f, Quaternion.LookRotation(((RaycastHit)(ref val8)).normal, Vector3.up)) as OreHotSpot;
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
}

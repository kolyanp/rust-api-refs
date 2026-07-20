using System.Collections.Generic;
using Oxide.Core;
using Rust;
using Rust.Ai.Gen2;
using UnityEngine;

public class CH47HelicopterAIController : CH47Helicopter
{
	[Tooltip("Prefab for the NPCs that will be spawned in the helicopter IF it doesn't land and just drops a crate.")]
	public GameObjectRef scientistPrefab;

	[Tooltip("Prefab for the NPCs that will be spawned in the helicopter IF it lands.")]
	public GameObjectRef dismountablePrefab;

	public float maxTiltAngle = 0.3f;

	public float AiAltitudeForce = 10000f;

	public GameObjectRef lockedCratePrefab;

	public const Flags Flag_Damaged = Flags.Reserved9;

	public const Flags Flag_NearDeath = Flags.OnFire;

	public const Flags Flag_DropDoorOpen = Flags.Reserved8;

	public Vector3 landingTarget;

	private List<BaseNPC2> parentedNpcs = new List<BaseNPC2>();

	public int numCrates = 1;

	private bool shouldLand;

	public bool aimDirOverride;

	public Vector3 _aimDirection = Vector3.forward;

	public Vector3 _moveTarget = Vector3.zero;

	public int lastAltitudeCheckFrame;

	public float altOverride;

	public float currentDesiredAltitude;

	private bool altitudeProtection = true;

	public float hoverHeight = 30f;

	[ServerVar(Help = "(Generated) Commands all active CH47 Chinook helicopter AI controllers to drop their cargo crate immediately")]
	public static void dropCrate()
	{
		CH47HelicopterAIController[] array = Object.FindObjectsByType<CH47HelicopterAIController>((FindObjectsInactive)0, (FindObjectsSortMode)0);
		foreach (CH47HelicopterAIController cH47HelicopterAIController in array)
		{
			if (cH47HelicopterAIController.isServer)
			{
				cH47HelicopterAIController.DropCrate();
			}
		}
	}

	public void DropCrate()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (numCrates > 0)
		{
			Vector3 pos = ((Component)this).transform.position + Vector3.down * 5f;
			Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
			BaseEntity baseEntity = GameManager.server.CreateEntity(lockedCratePrefab.resourcePath, pos, rot);
			if (Object.op_Implicit((Object)(object)baseEntity))
			{
				Interface.CallHook("OnHelicopterDropCrate", this);
				((Component)baseEntity).SendMessage("SetWasDropped");
				baseEntity.Spawn();
			}
			numCrates--;
		}
	}

	public bool OutOfCrates()
	{
		object obj = Interface.CallHook("OnHelicopterOutOfCrates", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return numCrates <= 0;
	}

	public bool CanDropCrate()
	{
		object obj = Interface.CallHook("CanHelicopterDropCrate", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return numCrates > 0;
	}

	public bool IsDropDoorOpen()
	{
		return HasFlag(Flags.Reserved8);
	}

	public void SetDropDoorOpen(bool open)
	{
		if (Interface.CallHook("OnHelicopterDropDoorOpen", this) != null)
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved8, open);
	}

	public bool ShouldLand()
	{
		return shouldLand;
	}

	public void SetLandingTarget(Vector3 target)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		shouldLand = true;
		landingTarget = target;
		numCrates = 0;
	}

	public void ClearLandingTarget()
	{
		shouldLand = false;
	}

	public void TriggeredEventSpawn()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = TerrainMeta.RandomPointOffshore(avoidDeepSeaPortal: false, avoidDeepSea: true);
		val.y = WaterLevel.GetWaterSurface(val, waves: false, volumes: false) + 30f;
		((Component)this).transform.position = val;
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (Interface.CallHook("CanUseHelicopter", player, this) == null)
		{
			base.AttemptMount(player, doMountChecks);
		}
	}

	public override void ServerInit()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		Invoke(CheckSpawnScientists, 0.25f);
		SetMoveTarget(((Component)this).transform.position);
	}

	public void SpawnPassenger(Vector3 spawnPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		SpawnInternal(spawnPos, dismountablePrefab);
	}

	public void SpawnScientist(Vector3 spawnPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		SpawnInternal(spawnPos, scientistPrefab);
	}

	private void SpawnInternal(Vector3 spawnPos, GameObjectRef prefab)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		if (BaseNetworkableEx.Is<HumanNPC>((Object)(object)prefab.GetEntity(), out HumanNPC _))
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(prefab.resourcePath, spawnPos, Quaternion.identity);
			baseEntity.Spawn();
			HumanNPC humanNPC = baseEntity as HumanNPC;
			AttemptMount(humanNPC);
			if (prefab == scientistPrefab)
			{
				humanNPC.Brain.SetEnabled(flag: false);
			}
			OnSpawnedHuman(humanNPC);
		}
		else
		{
			if (!BaseNetworkableEx.Is<BaseNPC2>((Object)(object)prefab.GetEntity(), out BaseNPC2 _))
			{
				return;
			}
			BaseMountable baseMountable = null;
			float num = float.MaxValue;
			foreach (MountPointInfo allMountPoint in base.allMountPoints)
			{
				if (allMountPoint.mountable.AnyMounted())
				{
					continue;
				}
				bool flag = false;
				foreach (BaseNPC2 parentedNpc in parentedNpcs)
				{
					if (Vector3.Distance(((Component)parentedNpc).transform.position, allMountPoint.mountable.mountAnchor.position) < 0.5f)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					float num2 = Vector3.Distance(allMountPoint.mountable.mountAnchor.position, spawnPos);
					if (num2 < num)
					{
						baseMountable = allMountPoint.mountable;
						num = num2;
					}
				}
			}
			if ((Object)(object)baseMountable != (Object)null)
			{
				BaseEntity baseEntity2 = GameManager.server.CreateEntity(prefab.resourcePath, spawnPos, Quaternion.identity);
				baseEntity2.Spawn();
				parentedNpcs.Add(baseEntity2 as BaseNPC2);
				baseEntity2.SetParent(this);
				((Component)baseEntity2).transform.position = baseMountable.mountAnchor.position;
				((Component)baseEntity2).transform.rotation = baseMountable.mountAnchor.rotation;
				((Behaviour)((Component)baseEntity2).GetComponent<RustNavMeshAgent>()).enabled = false;
			}
		}
	}

	public bool TryDismountOne()
	{
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		foreach (MountPointInfo mountPoint in mountPoints)
		{
			if (Object.op_Implicit((Object)(object)mountPoint.mountable) && mountPoint.mountable.AnyMounted())
			{
				mountPoint.mountable.DismountAllPlayers();
				return true;
			}
		}
		foreach (BaseNPC2 parentedNpc in parentedNpcs)
		{
			Transform val = null;
			float num = float.MaxValue;
			Transform[] array = dismountPositions;
			foreach (Transform val2 in array)
			{
				bool flag = false;
				foreach (BaseNPC2 parentedNpc2 in parentedNpcs)
				{
					if (Vector3.Distance(((Component)parentedNpc2).transform.position, val2.position) < 0.5f)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					float num2 = Vector3.Distance(((Component)parentedNpc).transform.position, val2.position);
					if (num2 < num)
					{
						num = num2;
						val = val2;
					}
				}
			}
			if ((Object)(object)val != (Object)null)
			{
				parentedNpc.SetParent(null, worldPositionStays: true);
				((Behaviour)((Component)parentedNpc).GetComponent<RustNavMeshAgent>()).enabled = true;
				parentedNpcs.Remove(parentedNpc);
				((Component)parentedNpc).transform.position = val.position;
				((Component)parentedNpc).transform.rotation = val.rotation;
				return true;
			}
		}
		return false;
	}

	private void OnSpawnedHuman(HumanNPC human)
	{
		if (!((Object)(object)human == (Object)null) && (Object)(object)human.Brain != (Object)null && human.Brain.Senses != null)
		{
			human.Brain.Senses.ignoreTutorialPlayers = true;
		}
	}

	public void CheckSpawnScientists()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (ValidBounds.Test(this, ((Component)this).transform.position))
		{
			Invoke(SpawnScientists, 2f);
		}
		else
		{
			Invoke(CheckSpawnScientists, 2f);
		}
	}

	public void SpawnScientists()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (shouldLand)
		{
			float dropoffScale = CH47LandingZone.GetClosest(landingTarget).dropoffScale;
			int num = Mathf.FloorToInt((float)(mountPoints.Count - 2) * dropoffScale);
			for (int i = 0; i < num; i++)
			{
				Vector3 spawnPos = ((Component)this).transform.position + ((Component)this).transform.forward * 10f;
				SpawnPassenger(spawnPos);
			}
			Vector3 spawnPos2 = ((Component)this).transform.position - ((Component)this).transform.forward * 15f;
			SpawnPassenger(spawnPos2);
		}
		else
		{
			for (int j = 0; j < 4; j++)
			{
				Vector3 spawnPos3 = ((Component)this).transform.position + ((Component)this).transform.forward * 10f;
				SpawnScientist(spawnPos3);
			}
			Vector3 spawnPos4 = ((Component)this).transform.position - ((Component)this).transform.forward * 15f;
			SpawnScientist(spawnPos4);
		}
	}

	public void EnableFacingOverride(bool enabled)
	{
		aimDirOverride = enabled;
	}

	public void SetMoveTarget(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_moveTarget = position;
	}

	public Vector3 GetMoveTarget()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _moveTarget;
	}

	public void SetAimDirection(Vector3 dir)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_aimDirection = dir;
	}

	public Vector3 GetAimDirectionOverride()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _aimDirection;
	}

	public Vector3 GetPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position;
	}

	public override void MounteeTookDamage(BasePlayer mountee, HitInfo info)
	{
		InitiateAnger();
	}

	public void CancelAnger()
	{
		if (base.SecondsSinceAttacked > 120f)
		{
			UnHostile();
			CancelInvoke(UnHostile);
		}
	}

	public void InitiateAnger()
	{
		CancelInvoke(UnHostile);
		Invoke(UnHostile, 120f);
		foreach (MountPointInfo mountPoint in mountPoints)
		{
			if (!((Object)(object)mountPoint.mountable != (Object)null))
			{
				continue;
			}
			BasePlayer mounted = mountPoint.mountable.GetMounted();
			if (Object.op_Implicit((Object)(object)mounted))
			{
				ScientistNPC scientistNPC = mounted as ScientistNPC;
				if ((Object)(object)scientistNPC != (Object)null)
				{
					scientistNPC.Brain.SetEnabled(flag: true);
				}
			}
		}
	}

	public void UnHostile()
	{
		foreach (MountPointInfo mountPoint in mountPoints)
		{
			if (!((Object)(object)mountPoint.mountable != (Object)null))
			{
				continue;
			}
			BasePlayer mounted = mountPoint.mountable.GetMounted();
			if (Object.op_Implicit((Object)(object)mounted))
			{
				ScientistNPC scientistNPC = mounted as ScientistNPC;
				if ((Object)(object)scientistNPC != (Object)null)
				{
					scientistNPC.Brain.SetEnabled(flag: false);
				}
			}
		}
	}

	public override void OnDied(HitInfo info)
	{
		if (Interface.CallHook("OnEntityDestroy", this) == null)
		{
			if (!OutOfCrates())
			{
				DropCrate();
			}
			base.OnDied(info);
		}
	}

	public override void OnAttacked(HitInfo info)
	{
		if (Interface.CallHook("OnHelicopterAttack", this, info) != null)
		{
			return;
		}
		base.OnAttacked(info);
		InitiateAnger();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved9, base.healthFraction <= 0.8f);
		flagsUpdateScope.Set(Flags.OnFire, base.healthFraction <= 0.33f);
	}

	public void DelayedKill()
	{
		DismountAllPlayers();
		Kill();
	}

	public override void DismountAllPlayers()
	{
		foreach (MountPointInfo mountPoint in mountPoints)
		{
			if ((Object)(object)mountPoint.mountable != (Object)null)
			{
				BasePlayer mounted = mountPoint.mountable.GetMounted();
				if (Object.op_Implicit((Object)(object)mounted) && mounted.IsAlive())
				{
					mounted.Hurt(10000f, DamageType.Explosion, this, useProtection: false);
				}
			}
		}
		foreach (BaseNPC2 parentedNpc in parentedNpcs)
		{
			if (parentedNpc.IsAlive())
			{
				parentedNpc.Hurt(10000f, DamageType.Explosion, this, useProtection: false);
			}
		}
		parentedNpcs.Clear();
	}

	public override void AdminKill()
	{
		DismountAllPlayers();
		base.AdminKill();
	}

	public void SetAltitudeProtection(bool on)
	{
		altitudeProtection = on;
	}

	public void CalculateDesiredAltitude()
	{
		CalculateOverrideAltitude();
		if (altOverride > currentDesiredAltitude)
		{
			currentDesiredAltitude = altOverride;
		}
		else
		{
			currentDesiredAltitude = Mathf.MoveTowards(currentDesiredAltitude, altOverride, Time.fixedDeltaTime * 5f);
		}
	}

	public void SetMinHoverHeight(float newHeight)
	{
		hoverHeight = newHeight;
	}

	public float CalculateOverrideAltitude()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		if (Time.frameCount == lastAltitudeCheckFrame)
		{
			return altOverride;
		}
		lastAltitudeCheckFrame = Time.frameCount;
		float y = GetMoveTarget().y;
		float waterOrTerrainSurface = WaterLevel.GetWaterOrTerrainSurface(GetMoveTarget(), waves: false, volumes: false);
		float num = Mathf.Max(y, waterOrTerrainSurface + hoverHeight);
		if (altitudeProtection)
		{
			Vector3 val = rigidBody.linearVelocity;
			Vector3 val2;
			if (!(((Vector3)(ref val)).magnitude < 0.1f))
			{
				val = rigidBody.linearVelocity;
				val2 = ((Vector3)(ref val)).normalized;
			}
			else
			{
				val2 = ((Component)this).transform.forward;
			}
			Vector3 val3 = val2;
			val = Vector3.Cross(Vector3.Cross(((Component)this).transform.up, val3), Vector3.up) + Vector3.down * 0.3f;
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			RaycastHit val4 = default(RaycastHit);
			RaycastHit val5 = default(RaycastHit);
			if (Physics.SphereCast(((Component)this).transform.position - normalized * 20f, 20f, normalized, ref val4, 75f, 1218511105) && Physics.SphereCast(((RaycastHit)(ref val4)).point + Vector3.up * 200f, 20f, Vector3.down, ref val5, 200f, 1218511105))
			{
				num = ((RaycastHit)(ref val5)).point.y + hoverHeight;
			}
		}
		altOverride = num;
		return altOverride;
	}

	public override void SetDefaultInputState()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		currentInputState.Reset();
		Vector3 moveTarget = GetMoveTarget();
		Vector3 val = Vector3.Cross(((Component)this).transform.right, Vector3.up);
		Vector3 val2 = Vector3.Cross(Vector3.up, val);
		float num = 0f - Vector3.Dot(Vector3.up, ((Component)this).transform.right);
		float num2 = Vector3.Dot(Vector3.up, ((Component)this).transform.forward);
		float num3 = Vector3Ex.Distance2D(((Component)this).transform.position, moveTarget);
		float y = ((Component)this).transform.position.y;
		float num4 = currentDesiredAltitude;
		Vector3 val3 = ((Component)this).transform.position + ((Component)this).transform.forward * 10f;
		val3.y = num4;
		Vector3 val4 = Vector3Ex.Direction2D(moveTarget, ((Component)this).transform.position);
		float num5 = 0f - Vector3.Dot(val4, val2);
		float num6 = Vector3.Dot(val4, val);
		float num7 = Mathf.InverseLerp(0f, 25f, num3);
		if (num6 > 0f)
		{
			float num8 = Mathf.InverseLerp(0f - maxTiltAngle, 0f, num2);
			currentInputState.pitch = 1f * num6 * num8 * num7;
		}
		else
		{
			float num9 = 1f - Mathf.InverseLerp(0f, maxTiltAngle, num2);
			currentInputState.pitch = 1f * num6 * num9 * num7;
		}
		if (num5 > 0f)
		{
			float num10 = Mathf.InverseLerp(0f - maxTiltAngle, 0f, num);
			currentInputState.roll = 1f * num5 * num10 * num7;
		}
		else
		{
			float num11 = 1f - Mathf.InverseLerp(0f, maxTiltAngle, num);
			currentInputState.roll = 1f * num5 * num11 * num7;
		}
		float num12 = Mathf.Abs(num4 - y);
		float num13 = 1f - Mathf.InverseLerp(10f, 30f, num12);
		currentInputState.pitch *= num13;
		currentInputState.roll *= num13;
		float num14 = maxTiltAngle;
		float num15 = Mathf.InverseLerp(0f + Mathf.Abs(currentInputState.pitch) * num14, num14 + Mathf.Abs(currentInputState.pitch) * num14, Mathf.Abs(num2));
		currentInputState.pitch += num15 * ((num2 < 0f) ? (-1f) : 1f);
		float num16 = Mathf.InverseLerp(0f + Mathf.Abs(currentInputState.roll) * num14, num14 + Mathf.Abs(currentInputState.roll) * num14, Mathf.Abs(num));
		currentInputState.roll += num16 * ((num < 0f) ? (-1f) : 1f);
		if (aimDirOverride || num3 > 30f)
		{
			Vector3 val5 = (aimDirOverride ? GetAimDirectionOverride() : Vector3Ex.Direction2D(GetMoveTarget(), ((Component)this).transform.position));
			Vector3 val6 = (aimDirOverride ? GetAimDirectionOverride() : Vector3Ex.Direction2D(GetMoveTarget(), ((Component)this).transform.position));
			float num17 = Vector3.Dot(val2, val5);
			float num18 = Vector3.Angle(val, val6);
			float num19 = Mathf.InverseLerp(0f, 70f, Mathf.Abs(num18));
			currentInputState.yaw = ((num17 > 0f) ? 1f : 0f);
			currentInputState.yaw -= ((num17 < 0f) ? 1f : 0f);
			currentInputState.yaw *= num19;
		}
		float throttle = Mathf.InverseLerp(5f, 30f, num3);
		currentInputState.throttle = throttle;
	}

	public void MaintainAIAltutide()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.position + rigidBody.linearVelocity;
		float num = currentDesiredAltitude;
		float y = val.y;
		float num2 = Mathf.Abs(num - y);
		bool flag = num > y;
		float num3 = Mathf.InverseLerp(0f, 10f, num2) * AiAltitudeForce * (flag ? 1f : (-1f));
		rigidBody.AddForce(Vector3.up * num3, (ForceMode)0);
	}

	public override void VehicleFixedUpdate()
	{
		using (TimeWarning.New("CH47HeliAI.VehicleFixedUpdate"))
		{
			hoverForceScale = 1f;
			base.VehicleFixedUpdate();
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved5, TOD_Sky.Instance.IsNight);
			}
			CalculateDesiredAltitude();
			MaintainAIAltutide();
		}
	}

	public override void DestroyShared()
	{
		if (base.isServer)
		{
			foreach (MountPointInfo mountPoint in mountPoints)
			{
				if ((Object)(object)mountPoint.mountable != (Object)null)
				{
					BasePlayer mounted = mountPoint.mountable.GetMounted();
					if (Object.op_Implicit((Object)(object)mounted) && (Object)(object)((Component)mounted).transform != (Object)null && !mounted.IsDestroyed && !mounted.IsDead() && mounted.IsNpc)
					{
						mounted.Kill();
					}
				}
			}
		}
		base.DestroyShared();
	}
}

using System;
using ConVar;
using UnityEngine;

namespace Rust.Ai.Gen2;

[SoftRequireComponent(typeof(SenseComponent))]
public class NpcShootingComponent : EntityComponent<BaseEntity>
{
	[Header("Weapon Stats")]
	[SerializeField]
	public ItemDefinition weaponItemDefinition;

	[SerializeField]
	private Vector3 offset = new Vector3(0.25f, 1.4f, 0.71f);

	[SerializeField]
	private float damageModifier = 1f;

	private const float spreadWhenAccurate = 0.1f;

	private AttackEntity weapon;

	private SenseComponent _senses;

	private RustNavMeshAgent _agent;

	private double burstEndTime;

	private double nextBurstBeginTime;

	private bool toggleAttachmentLightAtNight;

	private SenseComponent Senses => _senses ?? (_senses = ((Component)this).GetComponent<SenseComponent>());

	private RustNavMeshAgent Agent => _agent ?? (_agent = ((Component)this).GetComponent<RustNavMeshAgent>());

	private Vector3 EyePosition => Senses.EyePosition;

	public bool AllowShooting { get; set; } = true;

	public bool AllowBeingAccurate { get; set; } = true;

	public bool OnlyShootIfTargetIsVisible { get; set; } = true;

	public override void ServerInitPostNetworkGroupAssign()
	{
		base.ServerInitPostNetworkGroupAssign();
		Item item = ItemManager.Create(weaponItemDefinition, 1, 0uL, isServerSide: true, 0uL);
		HeldEntity component = ((Component)item.GetHeldEntity()).GetComponent<HeldEntity>();
		weapon = component as AttackEntity;
		weapon.limitNetworking = false;
		weapon.SetHeld(bHeld: true);
		weapon.SetParent(base.baseEntity, StringPool.Get(weapon.handBone));
		weapon.TopUpAmmo();
		if (BaseNetworkableEx.Is<BaseProjectile>((Object)(object)weapon, out BaseProjectile _) && item.contents != null)
		{
			Item item2 = ItemManager.CreateByName((toggleAttachmentLightAtNight = Random.Range(0, 3) == 0) ? "weapon.mod.flashlight" : "weapon.mod.lasersight", 1, 0uL);
			if (!item2.MoveToContainer(item.contents))
			{
				item2.Remove();
			}
			else if (!toggleAttachmentLightAtNight)
			{
				weapon.SetLightsOn(isOn: true);
			}
		}
		weapon.EnableSaving(base.baseEntity.enableSaving);
		foreach (BaseEntity child in weapon.children)
		{
			child.EnableSaving(base.baseEntity.enableSaving);
		}
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		if ((Object)(object)weapon != (Object)null && !weapon.IsDestroyed)
		{
			if (AI.logIssues && (Object)(object)weapon.GetParentEntity() != (Object)(object)base.baseEntity)
			{
				Debug.LogError((object)$"Weapon {weapon} of {base.baseEntity} was not parented to the entity.", (Object)(object)weapon);
			}
			weapon.Kill();
			weapon = null;
		}
	}

	private void Reset()
	{
		burstEndTime = 0.0;
		nextBurstBeginTime = 0.0;
	}

	private void Update()
	{
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		if (!base.baseEntity.isServer)
		{
			return;
		}
		if ((Object)(object)weapon.GetParentEntity() != (Object)(object)base.baseEntity && AI.logIssues)
		{
			Debug.LogError((object)$"Weapon {weapon} of {base.baseEntity} was not parented to the entity.", (Object)(object)weapon);
		}
		if ((BaseNetworkableEx.Is<BaseCombatEntity>((Object)(object)base.baseEntity, out BaseCombatEntity castedUnityObject) && castedUnityObject.IsDead()) || IsReloading())
		{
			return;
		}
		if (toggleAttachmentLightAtNight)
		{
			if (TOD_Sky.Instance.IsNight && !weapon.LightsOn())
			{
				weapon.SetLightsOn(isOn: true);
			}
			else if (TOD_Sky.Instance.IsDay && weapon.LightsOn())
			{
				weapon.SetLightsOn(isOn: false);
			}
		}
		if (!Senses.FindTarget(out var target))
		{
			Reset();
			if (ShouldReload(weapon, 0.5f))
			{
				Reload();
			}
		}
		else
		{
			if (!Senses.GetVisibilityStatus(target, out var status))
			{
				return;
			}
			if (ShouldReload(weapon, 0.5f) && !status.IsAware && status.timeNotVisible > 6f)
			{
				Reload();
			}
			else
			{
				if (!AllowShooting || Agent.IsSprinting || weapon.HasAttackCooldown())
				{
					return;
				}
				double timeAsDouble = Time.timeAsDouble;
				if ((timeAsDouble > burstEndTime && timeAsDouble < nextBurstBeginTime) || !Senses.FindLKP(target, out var lkp))
				{
					return;
				}
				bool flag = status.IsVisible && status.IsAware;
				bool flag2 = !OnlyShootIfTargetIsVisible && status.timeNotVisible <= 5f;
				if ((!flag && !flag2) || Vector3.Angle(((Component)base.baseEntity).transform.forward, Vector3Ex.WithY(lkp - ((Component)base.baseEntity).transform.position, 0f)) > 5f)
				{
					return;
				}
				Vector3 entityPointToShootAt = GetEntityPointToShootAt(target, lkp);
				Vector3 val = entityPointToShootAt;
				float num = Mathx.RemapValClamped(Vector3.Distance(((Component)base.baseEntity).transform.position, lkp), 0f, weapon.effectiveRange, 0f, 1f);
				bool flag3 = false;
				if (flag)
				{
					float num2 = 0.1f;
					float num3 = 0.1f;
					flag3 = CheckIfShouldMiss(target, num, lkp);
					if (flag3)
					{
						Vector3 extents = ((Bounds)(ref target.bounds)).extents;
						num2 += extents.x;
						num3 += extents.y;
					}
					val += CalculateSpreadOffset(entityPointToShootAt, num2, num3);
				}
				else
				{
					val += CalculateSpreadOffset(entityPointToShootAt);
				}
				Vector3 muzzleEstimatedPositionOnServer = GetMuzzleEstimatedPositionOnServer(lkp);
				if (!flag3 && !CanShootFromAt(muzzleEstimatedPositionOnServer, val))
				{
					if (!Senses.FindLKP(target, out var lkp2, applyHeightOffset: true, predict: false, ignoreCrouch: false))
					{
						return;
					}
					val = lkp2;
				}
				Matrix4x4 value = Matrix4x4.TRS(muzzleEstimatedPositionOnServer, Quaternion.LookRotation(val - muzzleEstimatedPositionOnServer), Vector3.one);
				weapon.ServerUse(new HeldEntityServerUseParams(damageModifier, 1f, value, useBulletThickness: false, useProtectionForNPCs: true));
				base.baseEntity.ClientRPC(RpcTarget.NetworkGroup("CL_Attack"));
				if (status.IsAware && status.IsVisible)
				{
					SingletonComponent<NpcNoiseManager>.Instance.OnNpcWeaponShot(base.baseEntity, target, val);
				}
				if (ShouldReload(weapon))
				{
					Reset();
					Reload();
				}
				if (timeAsDouble >= nextBurstBeginTime)
				{
					if (num < 0.5f)
					{
						burstEndTime = timeAsDouble + (double)((float)Random.Range(3, 10) * weapon.repeatDelay);
						nextBurstBeginTime = burstEndTime + (double)Random.Range(0.3f, 0.4f);
					}
					else if (num < 1f)
					{
						burstEndTime = timeAsDouble + (double)((float)Random.Range(3, 6) * weapon.repeatDelay);
						nextBurstBeginTime = burstEndTime + (double)Random.Range(0.5f, 1.2f);
					}
					else
					{
						burstEndTime = timeAsDouble + (double)weapon.repeatDelay;
						nextBurstBeginTime = burstEndTime + (double)Random.Range(0.5f, 1.5f);
					}
				}
			}
		}
	}

	private bool CheckIfShouldMiss(BaseEntity target, float distanceRatio, Vector3 groundLkp)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!AllowBeingAccurate)
		{
			return false;
		}
		target.ToNonNpcPlayer(out var player);
		float num = (((Object)(object)player != (Object)null && ((player.IsRunning() && Vector3.Angle(player.estimatedVelocity, ((Component)base.baseEntity).transform.position - groundLkp) < 30f) || player.estimatedSpeed < 1f)) ? 1f : (((Object)(object)player != (Object)null && player.IsRunning()) ? 0.5f : ((distanceRatio < 0.5f) ? 1f : ((!(distanceRatio < 1f)) ? 0.5f : 0.75f))));
		return Random.value > num;
	}

	public bool CanShootFromAt(Vector3 potentialLocation, Vector3 targetLocation, string debugCategory = "shoot trace")
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return !Senses.IsLineOccluded(potentialLocation, targetLocation, 1218519297, debugCategory);
	}

	public bool IsReloading()
	{
		if ((Object)(object)weapon == (Object)null)
		{
			return false;
		}
		return weapon.ServerIsReloading();
	}

	private static bool ShouldReload(AttackEntity weapon, float ammoThresholdModifier = 0f)
	{
		if ((Object)(object)weapon == (Object)null)
		{
			return false;
		}
		int ammoCount = GetAmmoCount(weapon);
		if (Mathf.Approximately(ammoThresholdModifier, 0f))
		{
			return ammoCount <= 0;
		}
		int num = Mathf.FloorToInt((float)GetMagazineSize(weapon) * ammoThresholdModifier);
		return ammoCount <= num;
	}

	private static int GetAmmoCount(AttackEntity weapon)
	{
		if ((Object)(object)weapon == (Object)null)
		{
			return 0;
		}
		if (BaseNetworkableEx.Is<BaseProjectile>((Object)(object)weapon, out BaseProjectile castedUnityObject))
		{
			return castedUnityObject.primaryMagazine.contents;
		}
		if (BaseNetworkableEx.Is<FlameThrower>((Object)(object)weapon, out FlameThrower castedUnityObject2))
		{
			return castedUnityObject2.ammo;
		}
		return 0;
	}

	private static int GetMagazineSize(AttackEntity weapon)
	{
		if ((Object)(object)weapon == (Object)null)
		{
			return 0;
		}
		if (BaseNetworkableEx.Is<BaseProjectile>((Object)(object)weapon, out BaseProjectile castedUnityObject))
		{
			return castedUnityObject.primaryMagazine.definition.builtInSize;
		}
		if (BaseNetworkableEx.Is<FlameThrower>((Object)(object)weapon, out FlameThrower castedUnityObject2))
		{
			return castedUnityObject2.maxAmmo;
		}
		return 0;
	}

	private void Reload()
	{
		weapon.ServerReload();
		base.baseEntity.ClientRPC(RpcTarget.NetworkGroup("CL_Reload"));
	}

	private Vector3 CalculateSpreadOffset(Vector3 targetPos, float spreadX = 0.1f, float spreadY = 0.1f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = targetPos - EyePosition;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector3 val2 = Vector3.up;
		if (Mathf.Abs(Vector3.Dot(normalized, Vector3.up)) > 0.99f)
		{
			val2 = Vector3.right;
		}
		val = Vector3.Cross(normalized, val2);
		Vector3 normalized2 = ((Vector3)(ref val)).normalized;
		val = Vector3.Cross(normalized2, normalized);
		Vector3 normalized3 = ((Vector3)(ref val)).normalized;
		float num = Random.Range(0f, MathF.PI * 2f);
		return normalized2 * Mathf.Cos(num) * spreadX + normalized3 * Mathf.Sin(num) * spreadY;
	}

	public static Vector3 GetEntityPointToShootAt(BaseEntity entity, Vector3 entityGroundPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return entityGroundPos + ((Bounds)(ref entity.bounds)).extents.y * Vector3.up;
	}

	public Vector3 GetMuzzleEstimatedPositionOnServer(Vector3 targetGroundPos, bool noZ = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.LookRotation(targetGroundPos - ((Component)base.baseEntity).transform.position);
		if (noZ)
		{
			return ((Component)base.baseEntity).transform.TransformPoint(Vector3Ex.WithZ(offset, 0f));
		}
		Quaternion val2 = Quaternion.Inverse(((Component)base.baseEntity).transform.rotation) * val;
		return ((Component)base.baseEntity).transform.TransformPoint(val2 * Vector3Ex.WithXY(offset, 0f, 0f) + Vector3Ex.WithZ(offset, 0f));
	}
}

using System;
using Facepunch;
using Facepunch.Extend;
using Network;
using ProtoBuf;
using UnityEngine;

public class AttackHelicopterTurret : StorageContainer
{
	public enum GunStatus
	{
		NoWeapon,
		Ready,
		Reloading,
		NoAmmo
	}

	[SerializeField]
	public Transform turretSocket;

	[SerializeField]
	public Transform turretHorizontal;

	[SerializeField]
	public Transform turretVertical;

	[NonSerialized]
	public AttackHelicopter owner;

	public EntityRef<HeldEntity> attachedHeldEntity;

	[NonSerialized]
	public bool forceAcceptAmmo;

	public const float WEAPON_Z_OFFSET_SCALE = -0.5f;

	public float muzzleYOffset;

	public float lastSentX;

	public float lastSentY;

	private TransformHandle turretHorHandle;

	private TransformHandle turretVerHandle;

	private int cachedClipAmmo;

	private int cachedInventoryAmmo;

	public bool HasOwner => (Object)(object)owner != (Object)null;

	public GunStatus GunState { get; set; }

	public float GunXAngle => turretVertical.localEulerAngles.x;

	public float GunYAngle => turretHorizontal.localEulerAngles.y;

	public int ClipAmmo => cachedClipAmmo;

	public int InventoryAmmo => cachedInventoryAmmo;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("AttackHelicopterTurret.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Load(LoadInfo info)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.attackHeliTurret != null)
		{
			_ = GunState;
			GunState = (GunStatus)info.msg.attackHeliTurret.gunState;
			float xRot = info.msg.attackHeliTurret.xRot;
			float yRot = info.msg.attackHeliTurret.yRot;
			SetGunRotation(xRot, yRot);
			attachedHeldEntity.uid = info.msg.attackHeliTurret.heldEntityID;
		}
	}

	public void SetGunRotation(float xRot, float yRot)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)owner == (Object)null))
		{
			turretHorizontal.localEulerAngles = new Vector3(0f, yRot, 0f);
			turretVertical.localEulerAngles = new Vector3(0f - xRot, 0f, 0f);
		}
	}

	public HeldEntity GetAttachedHeldEntity()
	{
		HeldEntity heldEntity = attachedHeldEntity.Get(base.isServer);
		if (heldEntity.IsValid())
		{
			return heldEntity;
		}
		return null;
	}

	public void GetAmmoAmounts(out int clip, out int available)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		clip = 0;
		available = 0;
		if (base.isServer && GetAttachedHeldEntity() is BaseProjectile baseProjectile)
		{
			clip = baseProjectile.primaryMagazine.contents;
			if (baseProjectile.primaryMagazine.allowAmmoSwitching)
			{
				available = base.inventory.GetAmmoAmount(baseProjectile.primaryMagazine.definition.ammoTypes);
			}
			else
			{
				available = base.inventory.GetAmmoAmount(baseProjectile.primaryMagazine.ammoType);
			}
		}
	}

	public Vector3 GetProjectedHitPos()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		HeldEntity heldEntity = GetAttachedHeldEntity();
		if ((Object)(object)heldEntity == (Object)null || (Object)(object)heldEntity.MuzzleTransform == (Object)null)
		{
			return Ballistics.GetBulletHitPoint(turretSocket.position, turretSocket.forward);
		}
		return Ballistics.GetBulletHitPoint(heldEntity.MuzzleTransform.position, heldEntity.MuzzleTransform.forward);
	}

	public override void ServerInit()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		ItemContainer itemContainer = base.inventory;
		itemContainer.canAcceptItem = (Func<Item, int, bool>)Delegate.Combine(itemContainer.canAcceptItem, new Func<Item, int, bool>(CanAcceptItem));
		InvokeRandomized(RefreshGunState, 0f, 0.25f, 0.05f);
		turretHorHandle = ((Component)turretHorizontal).transformHandle;
		turretVerHandle = ((Component)turretVertical).transformHandle;
		UpdateAmmoAmounts();
	}

	public override void Save(SaveInfo info)
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (HasOwner)
		{
			info.msg.attackHeliTurret = Pool.Get<AttackHeliTurret>();
			info.msg.attackHeliTurret.clipAmmo = cachedClipAmmo;
			info.msg.attackHeliTurret.totalAmmo = cachedInventoryAmmo;
			info.msg.attackHeliTurret.gunState = (int)GunState;
			if (BaseNetworkable.UseParallelSaves)
			{
				Quaternion localRotMT = Facepunch.Extend.TransformEx.Unsafe.GetLocalRotMT(in turretVerHandle);
				Quaternion localRotMT2 = Facepunch.Extend.TransformEx.Unsafe.GetLocalRotMT(in turretHorHandle);
				info.msg.attackHeliTurret.xRot = ((Quaternion)(ref localRotMT)).eulerAngles.x;
				info.msg.attackHeliTurret.yRot = ((Quaternion)(ref localRotMT2)).eulerAngles.y;
			}
			else
			{
				info.msg.attackHeliTurret.xRot = turretVertical.localEulerAngles.x;
				info.msg.attackHeliTurret.yRot = turretHorizontal.localEulerAngles.y;
			}
			info.msg.attackHeliTurret.heldEntityID = attachedHeldEntity.uid;
		}
	}

	public override BasePlayer ToPlayer()
	{
		if (HasOwner)
		{
			return owner.GetPassenger();
		}
		return null;
	}

	public bool CanAcceptItem(Item item, int targetSlot)
	{
		Item slot = base.inventory.GetSlot(0);
		if (IsValidWeapon(item) && targetSlot == 0)
		{
			return true;
		}
		if (item.info.category == ItemCategory.Ammunition)
		{
			if (forceAcceptAmmo)
			{
				return true;
			}
			if (slot == null || (Object)(object)GetAttachedHeldEntity() == (Object)null)
			{
				return false;
			}
			if (targetSlot == 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool IsValidWeapon(Item item)
	{
		ItemDefinition info = item.info;
		if (item.isBroken)
		{
			return false;
		}
		ItemModEntity component = ((Component)info).GetComponent<ItemModEntity>();
		if ((Object)(object)component == (Object)null)
		{
			return false;
		}
		HeldEntity component2 = component.entityPrefab.Get().GetComponent<HeldEntity>();
		if ((Object)(object)component2 == (Object)null)
		{
			return false;
		}
		if (!component2.IsUsableByTurret)
		{
			return false;
		}
		return true;
	}

	public bool InputTick(AttackHelicopter.GunnerInputState input)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		if (!owner.GunnerIsInGunnerView)
		{
			return false;
		}
		bool result = false;
		if (input.reload)
		{
			BaseProjectile baseProjectile = GetAttachedHeldEntity() as BaseProjectile;
			if ((Object)(object)baseProjectile != (Object)null)
			{
				TryReload(baseProjectile);
			}
		}
		else if (input.fire1)
		{
			result = TryFireWeapon();
		}
		else if (GetAttachedHeldEntity() is ITurretNotify turretNotify)
		{
			turretNotify.WarmupTick(wantsShoot: false);
		}
		((Ray)(ref input.eyeRay)).direction = ClampEyeAngle(((Component)owner).transform, ((Ray)(ref input.eyeRay)).direction, owner.turretPitchClamp, owner.turretYawClamp);
		Vector3 bulletHitPoint = Ballistics.GetBulletHitPoint(input.eyeRay);
		bulletHitPoint.y -= muzzleYOffset;
		Vector3 val = bulletHitPoint - turretSocket.position;
		val = ((Component)this).transform.InverseTransformDirection(val);
		Quaternion val2 = Quaternion.LookRotation(val, Vector3.up);
		Vector3 eulerAngles = ((Quaternion)(ref val2)).eulerAngles;
		float num = 0f - eulerAngles.x;
		float y = eulerAngles.y;
		SetGunRotation(num, y);
		if (Mathf.Abs(num - lastSentX) > 1f || Mathf.Abs(y - lastSentY) > 1f)
		{
			ClientRPC(RpcTarget.NetworkGroup("RPCRotation"), GetNetworkTime(), num, y);
			lastSentX = num;
			lastSentY = y;
		}
		return result;
	}

	public Vector3 ClampEyeAngle(Transform heliTransform, Vector3 eyeDir, Vector2 pitchRange, Vector2 yawRange)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = heliTransform.InverseTransformDirection(eyeDir);
		float num = Mathf.Clamp(Mathf.Asin(0f - val.y) * 57.29578f, pitchRange.x, pitchRange.y);
		float num2 = Mathf.Atan2(val.x, val.z) * 57.29578f;
		num2 = Mathf.Clamp(num2, yawRange.x, yawRange.y);
		val = Quaternion.Euler(num, num2, 0f) * Vector3.forward;
		return heliTransform.TransformDirection(val);
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		if (Object.op_Implicit((Object)(object)((Component)item.info).GetComponent<ItemModEntity>()))
		{
			if (IsInvoking(UpdateAttachedWeapon))
			{
				UpdateAttachedWeapon();
			}
			Invoke(UpdateAttachedWeapon, 0.5f);
		}
		UpdateAmmoAmounts();
	}

	public void UpdateAmmoAmounts()
	{
		GetAmmoAmounts(out cachedClipAmmo, out cachedInventoryAmmo);
	}

	public void UpdateAttachedWeapon()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (!HasOwner)
		{
			Debug.LogError((object)(((object)this).GetType().Name + ": Turret socket not yet set."));
			return;
		}
		HeldEntity heldEntity = AutoTurret.TryAddWeaponToTurret(base.inventory.GetSlot(0), turretSocket, this, -0.5f);
		if ((Object)(object)heldEntity != (Object)null)
		{
			attachedHeldEntity.Set(heldEntity);
			muzzleYOffset = turretSocket.InverseTransformPoint(heldEntity.MuzzleTransform.position).y;
		}
		else
		{
			HeldEntity heldEntity2 = GetAttachedHeldEntity();
			if ((Object)(object)heldEntity2 != (Object)null)
			{
				heldEntity2.SetGenericVisible(wantsVis: false);
				heldEntity2.SetLightsOn(isOn: false);
				if (heldEntity2 is ITurretNotify turretNotify)
				{
					turretNotify.WarmupTick(wantsShoot: false);
				}
			}
			attachedHeldEntity.Set(null);
			muzzleYOffset = 0f;
		}
		SendNetworkUpdate();
	}

	private bool TryReload(BaseProjectile gun)
	{
		bool num = gun.ServerTryReload(base.inventory);
		if (num)
		{
			UpdateAmmoAmounts();
		}
		return num;
	}

	public bool TryFireWeapon()
	{
		HeldEntity heldEntity = GetAttachedHeldEntity();
		if ((Object)(object)heldEntity == (Object)null)
		{
			return false;
		}
		if (owner.InSafeZone())
		{
			return false;
		}
		if (heldEntity is BaseProjectile baseProjectile)
		{
			if (baseProjectile.primaryMagazine.contents <= 0)
			{
				TryReload(baseProjectile);
				return false;
			}
			if (baseProjectile.NextAttackTime > Time.time)
			{
				return false;
			}
			if (baseProjectile is ITurretNotify turretNotify)
			{
				turretNotify.WarmupTick(wantsShoot: true);
				if (!turretNotify.CanShoot())
				{
					return false;
				}
			}
		}
		heldEntity.ServerUse();
		UpdateAmmoAmounts();
		ClientRPC(RpcTarget.NetworkGroup("RPCAmmo"), (short)cachedClipAmmo, (short)cachedInventoryAmmo);
		return true;
	}

	public void RefreshGunState()
	{
		HeldEntity heldEntity = GetAttachedHeldEntity();
		GunStatus gunStatus;
		if (Object.op_Implicit((Object)(object)heldEntity))
		{
			gunStatus = GunStatus.Ready;
			BaseProjectile baseProjectile = heldEntity as BaseProjectile;
			if ((Object)(object)baseProjectile != (Object)null)
			{
				if (baseProjectile.ServerIsReloading())
				{
					gunStatus = GunStatus.Reloading;
				}
				else
				{
					UpdateAmmoAmounts();
					if (cachedClipAmmo == 0 && cachedInventoryAmmo == 0)
					{
						gunStatus = GunStatus.NoAmmo;
					}
				}
			}
		}
		else
		{
			gunStatus = GunStatus.NoWeapon;
		}
		if (gunStatus != GunState)
		{
			GunState = gunStatus;
			SendNetworkUpdate();
		}
	}
}

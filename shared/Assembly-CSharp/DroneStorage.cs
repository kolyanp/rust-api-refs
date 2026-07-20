using Facepunch;
using UnityEngine;

public class DroneStorage : StorageContainer
{
	[Header("Drone Storage")]
	public Transform AttachPoint;

	public Vector3 ReleaseVelocity;

	public float GrenadeWeaponDelayMod = 3f;

	public float ThrownWeaponDelayMod = 1f;

	private static readonly Phrase FailPhrase = new Phrase("drone_storage.fail", "Drone is stuck, can't access inventory");

	private const float DroneBoxOffset = 0.14f;

	public Drone Drone { get; set; }

	public void UpdateFlags()
	{
		using FlagsUpdateScope flagsUpdateScope = Drone.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (Drone.HasFlag(Flags.Reserved5))
		{
			if (!TryGetItem(out var item) || !TryGetHeldEntity(item, out var held) || !(held is ThrownWeapon thrownWeapon))
			{
				flagsUpdateScope.Set(Flags.Reserved5, b: false);
			}
			else
			{
				flagsUpdateScope.Set(Flags.Reserved5, thrownWeapon.HasAttackCooldown());
			}
		}
	}

	public override bool ItemFilter(Item item, int targetSlot)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!base.ItemFilter(item, targetSlot))
		{
			return false;
		}
		RaycastHit val = default(RaycastHit);
		if (!Drone.HasFlag(Flags.Reserved3) && Drone.body.SweepTest(((Component)Drone).transform.up, ref val, 0.14f))
		{
			return false;
		}
		return true;
	}

	public override PlayerInventory.CanMoveFromResponse CanMoveFrom(BasePlayer player, Item item)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Drone.body.SweepTest(((Component)Drone).transform.up, ref val, 0.14f))
		{
			return PlayerInventory.CanMoveFromResponse.Failure(FailPhrase);
		}
		return base.CanMoveFrom(player, item);
	}

	public override bool CanOpenLootPanel(BasePlayer player, string panelName)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!base.CanOpenLootPanel(player, panelName))
		{
			return false;
		}
		RaycastHit val = default(RaycastHit);
		if (Drone.body.SweepTest(((Component)Drone).transform.up, ref val, 0.14f))
		{
			player.ShowToast(GameTip.Styles.Error, FailPhrase, false);
			return false;
		}
		return true;
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		base.OnItemAddedOrRemoved(item, added);
		if (added && !Drone.HasFlag(Flags.Reserved2) && !Drone.HasFlag(Flags.Reserved3))
		{
			Rigidbody body = Drone.body;
			body.position += ((Component)Drone).transform.up * 0.14f;
		}
		Drone.body.WakeUp();
		Drone.body.isKinematic = false;
	}

	public bool TryServerDrop()
	{
		if (!TryGetItem(out var item))
		{
			return false;
		}
		bool flag = false;
		if (TryGetHeldEntity(item, out var held) && held is ThrownWeapon weapon)
		{
			return TryServerWeaponDrop(base.inventory.GetSlot(0), weapon);
		}
		return TryServerItemDrop(base.inventory.GetSlot(0));
	}

	private bool TryGetItem(out Item item)
	{
		item = null;
		if (base.inventory.IsEmpty())
		{
			return false;
		}
		item = base.inventory.GetSlot(0);
		if (item == null)
		{
			return false;
		}
		return true;
	}

	private bool TryGetHeldEntity(Item item, out BaseEntity held)
	{
		held = null;
		if (item == null)
		{
			return false;
		}
		held = item.GetHeldEntity();
		if ((Object)(object)held == (Object)null)
		{
			return false;
		}
		return true;
	}

	private bool TryServerWeaponDrop(Item item, ThrownWeapon weapon)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (item.amount <= 0 || weapon.HasAttackCooldown())
		{
			return false;
		}
		if ((Object)(object)Drone == (Object)null)
		{
			return false;
		}
		Vector3 eyePos = default(Vector3);
		Quaternion val = default(Quaternion);
		AttachPoint.GetPositionAndRotation(ref eyePos, ref val);
		Vector3 throwVelocityOverride = GetInheritedThrowVelocity(val * Vector3.down) + ReleaseVelocity;
		BasePlayer owningPlayer = Drone.ToPlayer();
		weapon.DoThrowImpl(eyePos, val * Vector3.down, owningPlayer, out var thrownEntity, 1f, throwVelocityOverride, item);
		if (weapon is GrenadeWeapon)
		{
			weapon.StartAttackCooldown(weapon.repeatDelay * GrenadeWeaponDelayMod);
		}
		else
		{
			weapon.StartAttackCooldown(weapon.repeatDelay * ThrownWeaponDelayMod);
		}
		item.UseItem();
		TempIgnoreParent(thrownEntity);
		Drone.MarkHostileFor();
		if (weapon.HasAttackCooldown())
		{
			using FlagsUpdateScope flagsUpdateScope = Drone.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved5, b: true);
		}
		SendNetworkUpdateImmediate();
		return true;
	}

	private bool TryServerItemDrop(Item item)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Vector3 vPos = default(Vector3);
		Quaternion val = default(Quaternion);
		AttachPoint.GetPositionAndRotation(ref vPos, ref val);
		BaseEntity ent = item.Drop(vPos, GetInheritedProjectileVelocity(val * Vector3.down) + ReleaseVelocity);
		TempIgnoreParent(ent);
		return true;
	}

	private void TempIgnoreParent(BaseEntity ent)
	{
		if ((Object)(object)ent == (Object)null || !parentEntity.IsValid(serverside: true))
		{
			return;
		}
		GameObjectExtensions.SetIgnoreCollisions(((Component)ent).gameObject, ((Component)parentEntity.Get(serverside: true)).gameObject, true);
		Invoke(delegate
		{
			BaseEntity baseEntity = ent;
			if (!((Object)(object)baseEntity == (Object)null))
			{
				BaseEntity baseEntity2 = parentEntity.Get(serverside: true);
				if (!((Object)(object)baseEntity2 == (Object)null))
				{
					GameObjectExtensions.SetIgnoreCollisions(((Component)baseEntity2).gameObject, ((Component)baseEntity).gameObject, false);
				}
			}
		}, 2f);
	}
}

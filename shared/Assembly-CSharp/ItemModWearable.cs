using Rust;
using UnityEngine;

public class ItemModWearable : ItemMod
{
	public enum WearableEventType
	{
		Easter,
		Halloween
	}

	public GameObjectRef entityPrefab = new GameObjectRef();

	public GameObjectRef entityPrefabFemale = new GameObjectRef();

	public ProtectionProperties protectionProperties;

	public ArmorProperties armorProperties;

	public ClothingMovementProperties movementProperties;

	public UIBlackoutOverlay.blackoutType occlusionType = UIBlackoutOverlay.blackoutType.NONE;

	[Range(0f, 1f)]
	[Tooltip("Muffles world audio for the local player while this is worn. Drives the head gear lowpass, layering over the active snapshot.")]
	public float audioMuffleAmount;

	public bool blocksAiming;

	public bool emissive;

	public float accuracyBonus;

	public bool blocksEquipping;

	public float eggVision;

	public WearableEventType eggVisionType;

	public float weight;

	public bool equipOnRightClick = true;

	public bool equipOnPickup;

	public bool npcOnly;

	public GameObjectRef breakEffect = new GameObjectRef();

	public bool preventsMounting;

	public bool preventsMap;

	public bool blockFromMannequin;

	public GameObjectRef viewmodelAddition;

	public Wearable targetWearable
	{
		get
		{
			if (entityPrefab.isValid)
			{
				return entityPrefab.Get().GetComponent<Wearable>();
			}
			return null;
		}
	}

	private void DoPrepare()
	{
		if (!entityPrefab.isValid)
		{
			Debug.LogWarning((object)("ItemModWearable: entityPrefab is null! " + (object)((Component)this).gameObject), (Object)(object)((Component)this).gameObject);
		}
		if (entityPrefab.isValid && (Object)(object)targetWearable == (Object)null)
		{
			Debug.LogWarning((object)("ItemModWearable: entityPrefab doesn't have a Wearable component! " + (object)((Component)this).gameObject), (Object)(object)entityPrefab.Get());
		}
		ValidateViewmodelRandomisation();
	}

	private void ValidateViewmodelRandomisation()
	{
		if (viewmodelAddition == null || !viewmodelAddition.isValid)
		{
			return;
		}
		GameObject val = viewmodelAddition.Get();
		ViewmodelClothingRandomiser viewmodelClothingRandomiser = default;
		if (!((Object)(object)val == (Object)null) && val.TryGetComponent<ViewmodelClothingRandomiser>(ref viewmodelClothingRandomiser))
		{
			if (viewmodelClothingRandomiser.randomSkins == null || viewmodelClothingRandomiser.randomSkins.Length == 0)
			{
				Debug.LogWarning((object)("ItemModWearable: viewmodelAddition has a ViewmodelClothingRandomiser with no sets! " + (object)((Component)this).gameObject), (Object)(object)val);
				return;
			}
			ValidateViewmodelRandomisation(viewmodelClothingRandomiser, entityPrefab);
			ValidateViewmodelRandomisation(viewmodelClothingRandomiser, entityPrefabFemale);
		}
	}

	private void ValidateViewmodelRandomisation(ViewmodelClothingRandomiser randomiser, GameObjectRef wearablePrefab)
	{
		if (wearablePrefab == null || !wearablePrefab.isValid)
		{
			return;
		}
		GameObject val = wearablePrefab.Get();
		Wearable wearable = default;
		if ((Object)(object)val == (Object)null || !val.TryGetComponent<Wearable>(ref wearable))
		{
			return;
		}
		Wearable.PartRandomizer[] randomParts = wearable.randomParts;
		int num = randomParts?.Length ?? 0;
		if (randomiser.randomSkins.Length > num)
		{
			Debug.LogWarning((object)($"ItemModWearable: ViewmodelClothingRandomiser has {randomiser.randomSkins.Length} set(s) but {((Object)val).name} only has {num} - the extra sets line up with nothing! " + (object)((Component)this).gameObject), (Object)(object)val);
			return;
		}
		for (int i = 0; i < randomiser.randomSkins.Length; i++)
		{
			ViewmodelClothingRandomiser.SkinCollection[] groups = randomiser.randomSkins[i].groups;
			if (groups != null && groups.Length != 0)
			{
				int num2 = randomParts[i].groups?.Length ?? 0;
				if (num2 != groups.Length)
				{
					Debug.LogWarning((object)(string.Format("ItemModWearable: ViewmodelClothingRandomiser set {0} has {1} group(s) but randomParts[{2}] on {3} has {4} - first person won't match third person! ", new object[5]
					{
						i,
						groups.Length,
						i,
						((Object)val).name,
						num2
					}) + (object)((Component)this).gameObject), (Object)(object)val);
				}
			}
		}
	}

	public override void ModInit(ItemDefinition parentItemDefinition)
	{
		base.parentItemDefinition = parentItemDefinition;
		if (string.IsNullOrEmpty(entityPrefab.resourcePath))
		{
			Debug.LogWarning((object)(((object)this)?.ToString() + " - entityPrefab is null or something.. - " + entityPrefab.guid));
		}
	}

	public bool ProtectsArea(HitArea area)
	{
		if ((Object)(object)armorProperties == (Object)null)
		{
			return false;
		}
		return armorProperties.Contains(area);
	}

	public bool ProtectsWholeBody()
	{
		bool flag = ProtectsArea(HitArea.Head);
		bool flag2 = ProtectsArea(HitArea.Chest);
		bool flag3 = ProtectsArea(HitArea.Leg);
		return flag & flag2 & flag3;
	}

	public bool HasProtections()
	{
		return protectionProperties != null;
	}

	public float GetProtection(Item item, DamageType damageType)
	{
		if (protectionProperties == null)
		{
			return 0f;
		}
		float num = protectionProperties.Get(damageType);
		float num2 = 0f;
		ItemModContainerArmorSlot itemModContainerArmorSlot = default;
		if (((Component)this).TryGetComponent<ItemModContainerArmorSlot>(ref itemModContainerArmorSlot))
		{
			num2 = itemModContainerArmorSlot.GetProtection(item, damageType);
		}
		return (num + num2) * ConditionProtectionScale(item);
	}

	public float ConditionProtectionScale(Item item)
	{
		if (!item.isBroken)
		{
			return 1f;
		}
		return 0.25f;
	}

	public void CollectProtection(Item item, ProtectionProperties protection, float multiplier)
	{
		if (protectionProperties != null)
		{
			protection.Add(protectionProperties, ConditionProtectionScale(item) * multiplier);
		}
		ItemModContainerArmorSlot itemModContainerArmorSlot = default;
		if (((Component)this).TryGetComponent<ItemModContainerArmorSlot>(ref itemModContainerArmorSlot))
		{
			itemModContainerArmorSlot.CollectProtection(item, protection, multiplier);
		}
	}

	private bool IsHeadgear()
	{
		Wearable component = entityPrefab.Get().GetComponent<Wearable>();
		if ((Object)(object)component != (Object)null && (component.occupationOver & (Wearable.OccupationSlots.HeadTop | Wearable.OccupationSlots.Face | Wearable.OccupationSlots.HeadBack)) != 0)
		{
			return true;
		}
		return false;
	}

	public bool IsFootwear()
	{
		Wearable component = entityPrefab.Get().GetComponent<Wearable>();
		if ((Object)(object)component != (Object)null && (component.occupationOver & (Wearable.OccupationSlots.LeftFoot | Wearable.OccupationSlots.RightFoot)) != 0)
		{
			return true;
		}
		return false;
	}

	public override void OnAttacked(Item item, HitInfo info)
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		if (!item.hasCondition)
		{
			return;
		}
		float num = 0f;
		for (int i = 0; i < 28; i++)
		{
			DamageType damageType = (DamageType)i;
			if (info.damageTypes.Has(damageType))
			{
				num += Mathf.Clamp(info.damageTypes.types[i] * GetProtection(item, damageType), 0f, item.condition);
				if (num >= item.condition)
				{
					break;
				}
			}
		}
		item.LoseCondition(num);
		if (item != null && item.isBroken && Object.op_Implicit((Object)(object)item.GetOwnerPlayer()) && IsHeadgear() && info.damageTypes.Total() >= item.GetOwnerPlayer().health)
		{
			Vector3 vPos = ((Component)item.GetOwnerPlayer()).transform.position + new Vector3(0f, 1.8f, 0f);
			Vector3 vVelocity = item.GetOwnerPlayer().GetInheritedDropVelocity() + Vector3.up * 3f;
			Quaternion rotation = default;
			BaseEntity baseEntity = item.Drop(vPos, vVelocity, rotation);
			rotation = Random.rotation;
			baseEntity.SetAngularVelocity(rotation.eulerAngles * 5f);
		}
	}

	public bool CanExistWith(ItemModWearable wearable)
	{
		if ((Object)(object)wearable == (Object)null)
		{
			return true;
		}
		Wearable wearable2 = targetWearable;
		Wearable wearable3 = wearable.targetWearable;
		if ((wearable2.occupationOver & wearable3.occupationOver) != 0)
		{
			return false;
		}
		if ((wearable2.occupationUnder & wearable3.occupationUnder) != 0)
		{
			return false;
		}
		return true;
	}
}

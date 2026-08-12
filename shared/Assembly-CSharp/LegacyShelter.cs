using System.Collections.Generic;
using ConVar;
using Facepunch;
using ProtoBuf;
using Rust;
using UnityEngine;

public class LegacyShelter : DecayEntity
{
	public static readonly int FpShelterDefault;

	[ReplicatedVar]
	public static int max_shelters;

	private static Dictionary<ulong, List<LegacyShelter>> sheltersPerPlayer;

	public static Phrase shelterLimitPhrase;

	public static Phrase shelterLimitReachedPhrase;

	[Header("Shelter References")]
	public GameObjectRef smallPrivilegePrefab;

	public GameObjectRef includedDoorPrefab;

	public GameObjectRef includedLockPrefab;

	public EntityRef<EntityPrivilege> entityPrivilege;

	private EntityRef<LegacyShelterDoor> childDoorInstance;

	private EntityRef<BaseLock> lockEntityInstance;

	private Decay decayReference;

	private float lastShelterDecayTick;

	public float lastInteractedWithDoor;

	private ulong shelterOwnerID;

	public static Dictionary<ulong, List<LegacyShelter>> SheltersPerPlayer => sheltersPerPlayer;

	public static Planner.CanBuildResult? CanBuildShelter(BasePlayer player, Construction construction)
	{
		GameObject obj = GameManager.server.FindPrefab(construction.prefabID);
		if (((obj != null) ? obj.GetComponent<BaseEntity>() : null) is LegacyShelter)
		{
			int num = 1;
			if (sheltersPerPlayer.TryGetValue(player.userID, out var value))
			{
				num = value.Count + 1;
				if (value.Count >= max_shelters)
				{
					return new Planner.CanBuildResult
					{
						Result = false,
						Phrase = shelterLimitReachedPhrase
					};
				}
			}
			return new Planner.CanBuildResult
			{
				Result = true,
				Phrase = shelterLimitPhrase,
				Arguments = new string[2]
				{
					num.ToString(),
					max_shelters.ToString()
				}
			};
		}
		return null;
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		if (sheltersPerPlayer.TryGetValue(shelterOwnerID, out var _))
		{
			sheltersPerPlayer[shelterOwnerID].Remove(this);
			BasePlayer basePlayer = BasePlayer.FindByID(shelterOwnerID);
			if ((Object)(object)basePlayer != (Object)null)
			{
				basePlayer.SendRespawnOptions();
			}
		}
	}

	public static int GetShelterCount(ulong userId)
	{
		if (userId == 0L)
		{
			return 0;
		}
		if (!sheltersPerPlayer.TryGetValue(userId, out var value))
		{
			return 0;
		}
		return value.Count;
	}

	private void AddToShelterList(ulong id)
	{
		if (!sheltersPerPlayer.ContainsKey(id))
		{
			sheltersPerPlayer.Add(id, new List<LegacyShelter>());
		}
		if (!IsShelterInList(sheltersPerPlayer[id], out var _))
		{
			sheltersPerPlayer[id].Add(this);
		}
	}

	private bool IsShelterInList(List<LegacyShelter> shelters, out LegacyShelter thisShelter)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		thisShelter = null;
		if (shelters.Count == 0)
		{
			return false;
		}
		if ((Object)(object)thisShelter == (Object)null)
		{
			return false;
		}
		foreach (LegacyShelter shelter in shelters)
		{
			if (shelter.net.ID == net.ID)
			{
				result = true;
				thisShelter = shelter;
				break;
			}
		}
		return result;
	}

	public override EntityPrivilege GetEntityBuildingPrivilege()
	{
		return GetEntityPrivilege();
	}

	public EntityPrivilege GetEntityPrivilege()
	{
		EntityPrivilege entityPrivilege = this.entityPrivilege.Get(base.isServer);
		if (entityPrivilege.IsValid())
		{
			return entityPrivilege;
		}
		return null;
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (base.isServer && child.prefabID == includedDoorPrefab.GetEntity().prefabID && !Application.isLoadingSave)
		{
			Setup(child);
		}
		if (child.prefabID == smallPrivilegePrefab.GetEntity().prefabID)
		{
			EntityPrivilege entity = (EntityPrivilege)child;
			entityPrivilege.Set(entity);
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.legacyShelter == null || !base.isServer)
		{
			return;
		}
		shelterOwnerID = info.msg.legacyShelter.ownerId;
		childDoorInstance = new EntityRef<LegacyShelterDoor>(info.msg.legacyShelter.doorID);
		lastInteractedWithDoor = info.msg.legacyShelter.timeSinceInteracted;
		AddToShelterList(shelterOwnerID);
		if (max_shelters == FpShelterDefault)
		{
			BasePlayer basePlayer = BasePlayer.FindByID(shelterOwnerID);
			if ((Object)(object)basePlayer != (Object)null)
			{
				basePlayer.SendRespawnOptions();
			}
		}
	}

	public override void DecayTick(bool force = false)
	{
		base.DecayTick(force);
		float num = Time.time - lastShelterDecayTick;
		lastShelterDecayTick = Time.time;
		float num2 = num * ConVar.Decay.scale;
		lastInteractedWithDoor += num2;
		UpdateDoorHp();
	}

	public void HasInteracted()
	{
		lastInteractedWithDoor = 0f;
	}

	public void SetupDecay()
	{
		decayReference = PrefabAttribute.server.Find<Decay>(prefabID);
	}

	public override float GetEntityDecayDuration()
	{
		if (lastInteractedWithDoor < 64800f)
		{
			return float.MaxValue;
		}
		if (decayReference == null)
		{
			SetupDecay();
		}
		if (decayReference != null)
		{
			return decayReference.GetDecayDuration(this);
		}
		return float.MaxValue;
	}

	public LegacyShelterDoor GetChildDoor()
	{
		LegacyShelterDoor legacyShelterDoor = childDoorInstance.Get(base.isServer);
		if (legacyShelterDoor.IsValid())
		{
			return legacyShelterDoor;
		}
		return null;
	}

	public override void Save(SaveInfo info)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.legacyShelter = Pool.Get<LegacyShelter>();
		info.msg.legacyShelter.doorID = childDoorInstance.uid;
		info.msg.legacyShelter.timeSinceInteracted = lastInteractedWithDoor;
		info.msg.legacyShelter.ownerId = shelterOwnerID;
	}

	public override void OnPlaced(BasePlayer player)
	{
		if (sheltersPerPlayer.TryGetValue(player.userID, out var value) && value.Count >= max_shelters)
		{
			value[0].Kill(DestroyMode.Gib);
		}
		shelterOwnerID = player.userID;
		AddToShelterList(shelterOwnerID);
		player.SendRespawnOptions();
	}

	public override void Hurt(HitInfo info)
	{
		base.Hurt(info);
		LegacyShelterDoor childDoor = GetChildDoor();
		if ((Object)(object)childDoor != (Object)null)
		{
			childDoor.ProtectedHurt(info);
		}
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
		LegacyShelterDoor childDoor = GetChildDoor();
		if ((Object)(object)childDoor != (Object)null && !childDoor.IsDead())
		{
			childDoor.Die();
		}
	}

	public override void OnRepair()
	{
		base.OnRepair();
		UpdateDoorHp();
	}

	public override void OnRepairFinished(BasePlayer player)
	{
		base.OnRepairFinished(player);
		UpdateDoorHp();
	}

	public void ProtectedHurt(HitInfo info)
	{
		info.HitEntity = this;
		base.Hurt(info);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		LegacyShelterDoor childDoor = GetChildDoor();
		if (Object.op_Implicit((Object)(object)childDoor))
		{
			childDoor.SetupDoor(this);
			childDoor.SetMaxHealth(MaxHealth());
			UpdateDoorHp();
		}
		SetupDecay();
	}

	private void Setup(BaseEntity child)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		LegacyShelterDoor legacyShelterDoor = (LegacyShelterDoor)child;
		childDoorInstance.Set(legacyShelterDoor);
		BasePlayer basePlayer = BasePlayer.FindByID(shelterOwnerID);
		if ((Object)(object)basePlayer != (Object)null)
		{
			((Component)this).GetComponentInChildren<EntityPrivilege>().AddPlayer(basePlayer);
		}
		legacyShelterDoor.SetupDoor(this);
		legacyShelterDoor.OwnerID = shelterOwnerID;
		legacyShelterDoor.SetMaxHealth(MaxHealth());
		UpdateDoorHp();
		BaseEntity baseEntity = GameManager.server.CreateEntity(includedLockPrefab.resourcePath);
		baseEntity.SetParent(legacyShelterDoor, legacyShelterDoor.GetSlotAnchorName(Slot.Lock));
		baseEntity.OwnerID = shelterOwnerID;
		baseEntity.OnDeployed(legacyShelterDoor, basePlayer, null);
		baseEntity.Spawn();
		BaseLock baseLock = (BaseLock)baseEntity;
		if ((Object)(object)baseLock != (Object)null)
		{
			baseLock.CanRemove = false;
		}
		legacyShelterDoor.SetSlot(Slot.Lock, baseEntity);
	}

	private void UpdateDoorHp()
	{
		LegacyShelterDoor childDoor = GetChildDoor();
		if ((Object)(object)childDoor != (Object)null)
		{
			childDoor.SetHealth(base.health);
		}
	}

	static LegacyShelter()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		FpShelterDefault = 1;
		max_shelters = 1;
		sheltersPerPlayer = new Dictionary<ulong, List<LegacyShelter>>();
		shelterLimitPhrase = new Phrase("shelter_limit_update", "You are now at {0}/{1} shelters");
		shelterLimitReachedPhrase = new Phrase("shelter_limit_reached", "You have reached your shelter limit!");
	}
}

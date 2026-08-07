using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class BoatBuildingBlock : BuildingBlock, IPlacementDirectionProvider
{
	public List<Transform> SnapCardinalDirections;

	public List<Vector3> CachedLocalSnapCardinalDirections;

	[ServerVar(Help = "(Generated) When enabled, damage dealt to a building block attached to a boat is forwarded up to the parent boat entity")]
	public static bool ForwardDamageToParentBoat = true;

	[ReplicatedVar]
	public static bool AlwaysDemolishable = true;

	[ReplicatedVar]
	public static bool AlwayRotatable = true;

	public bool ProvidesParentingTrigger;

	public bool Hull;

	public bool Floor;

	public float ContributingMass = 100f;

	public float ContributingHealth = 50f;

	public Transform[] DismountPoints;

	public float damageTaken;

	public ParticleSystem[] CardinalSplashFx = (ParticleSystem[])(object)new ParticleSystem[0];

	public GameObject WaterDisplacement;

	[HideInInspector]
	public bool SendNetworkUpdateOnHealthChanged = true;

	private List<TriggerParent> parentTriggers;

	private List<Transform> originalTriggerParents;

	public override bool AlsoVisCheckParent => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BoatBuildingBlock.OnRpcMessage"))
		{
			if (rpc == 2419844654u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_WantsPushParentBoat"));
				}
				using (TimeWarning.New("RPC_WantsPushParentBoat"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2419844654u, "RPC_WantsPushParentBoat", this, player, 5f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_WantsPushParentBoat(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_WantsPushParentBoat");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override bool HasAnySlot()
	{
		return false;
	}

	public Vector3 GetDismountCheckStart()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position + Vector3.up * 1f;
	}

	public List<Vector3> GetSnapForwardDirections()
	{
		return CachedLocalSnapCardinalDirections;
	}

	public bool IsFullyInsideOBB(OBB otherOBB)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		OBB val = WorldSpaceBounds();
		if (!((OBB)(ref otherOBB)).Contains(((OBB)(ref val)).GetPoint(-1f, 0f, -1f)))
		{
			return false;
		}
		if (!((OBB)(ref otherOBB)).Contains(((OBB)(ref val)).GetPoint(1f, 0f, -1f)))
		{
			return false;
		}
		if (!((OBB)(ref otherOBB)).Contains(((OBB)(ref val)).GetPoint(1f, 0f, 1f)))
		{
			return false;
		}
		if (!((OBB)(ref otherOBB)).Contains(((OBB)(ref val)).GetPoint(-1f, 0f, 1f)))
		{
			return false;
		}
		return true;
	}

	public override bool IsDemolishable()
	{
		if (AlwaysDemolishable)
		{
			return true;
		}
		return base.IsDemolishable();
	}

	public override bool HasRotateFlag()
	{
		if (AlwayRotatable)
		{
			return true;
		}
		return base.HasRotateFlag();
	}

	public override float MaxHealth()
	{
		if (maxHealthOverride > 0f)
		{
			return maxHealthOverride;
		}
		return ContributingHealth;
	}

	public override bool Interactable()
	{
		return (Object)(object)parentEntity.Get(base.isServer) == (Object)null;
	}

	public void SwitchToVehicle(bool loading)
	{
		if (!loading)
		{
			damageTaken = 0f;
		}
		if ((Object)(object)currentSkin == (Object)null)
		{
			UpdateSkin(force: true);
		}
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		BaseEntity baseEntity = parentEntity.Get(base.isServer);
		if ((Object)(object)baseEntity != (Object)null && baseEntity is PlayerBoat playerBoat)
		{
			playerBoat.OnSubChildAdded(child);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (base.isServer && info.fromDisk && !PlayerBoat.IsChildOfFinishedPlayerBoat(this))
		{
			DisableParentTrigger();
		}
		if (info.msg.boatBuildingBlock != null)
		{
			damageTaken = info.msg.boatBuildingBlock.damageTaken;
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!Application.isLoadingSave)
		{
			DisableParentTrigger();
		}
		SetMaxHealth(ContributingHealth);
		SetHealthToMax();
	}

	protected override bool CanRotate(BasePlayer player)
	{
		if ((Object)(object)PlayerBoat.GetParentPlayerBoat(this) != (Object)null)
		{
			return false;
		}
		return base.CanRotate(player);
	}

	protected override void OnSkinRefresh()
	{
		if (!IsFullySpawned() || (parentEntity.Get(serverside: true) is PlayerBoat playerBoat && Object.op_Implicit((Object)(object)playerBoat)))
		{
			return;
		}
		Invoke(delegate
		{
			if (!(parentEntity.Get(serverside: true) is PlayerBoat playerBoat2) || !Object.op_Implicit((Object)(object)playerBoat2))
			{
				DisableParentTrigger();
			}
		}, 0f);
	}

	private void DisableParentTrigger()
	{
		EnsurePopulatedTriggersList();
		foreach (TriggerParent parentTrigger in parentTriggers)
		{
			((Component)parentTrigger).gameObject.SetActive(false);
		}
	}

	private void EnsurePopulatedTriggersList()
	{
		using (TimeWarning.New("EnsurePopulatedTriggersList"))
		{
			List<TriggerParent> list = parentTriggers;
			if (list != null && list.Count > 0)
			{
				bool flag = false;
				foreach (TriggerParent parentTrigger in parentTriggers)
				{
					if ((Object)(object)parentTrigger == (Object)null)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return;
				}
			}
			bool flag2 = false;
			BoatConstructionSkin boatConstructionSkin = currentSkin as BoatConstructionSkin;
			Debug.Assert(Object.op_Implicit((Object)(object)boatConstructionSkin) || flag2);
			if (parentTriggers == null)
			{
				parentTriggers = new List<TriggerParent>();
			}
			parentTriggers.Clear();
			if (Object.op_Implicit((Object)(object)boatConstructionSkin.parentTrigger))
			{
				parentTriggers.Add(boatConstructionSkin.parentTrigger);
			}
			BoatConstructionSkin boatConstructionSkin2 = default(BoatConstructionSkin);
			foreach (GameObject conditional in boatConstructionSkin.conditionals)
			{
				if (conditional.TryGetComponent<BoatConstructionSkin>(ref boatConstructionSkin2) && Object.op_Implicit((Object)(object)boatConstructionSkin2.parentTrigger))
				{
					parentTriggers.Add(boatConstructionSkin2.parentTrigger);
				}
			}
		}
	}

	public List<TriggerParent> SetTriggerParent(PlayerBoat boat)
	{
		BoatConstructionSkin obj = currentSkin as BoatConstructionSkin;
		Debug.Assert(Object.op_Implicit((Object)(object)obj));
		TriggerHurtNotChild hurtTrigger = obj.hurtTrigger;
		if (Object.op_Implicit((Object)(object)hurtTrigger))
		{
			hurtTrigger.SetSourceEntity(boat);
		}
		EnsurePopulatedTriggersList();
		foreach (TriggerParent parentTrigger in parentTriggers)
		{
			((Component)parentTrigger).gameObject.SetActive(true);
		}
		originalTriggerParents = new List<Transform>(parentTriggers.Count);
		for (int i = 0; i < parentTriggers.Count; i++)
		{
			originalTriggerParents.Add(((Object)(object)parentTriggers[i] != (Object)null) ? ((Component)parentTriggers[i]).transform.parent : null);
		}
		foreach (TriggerParent parentTrigger2 in parentTriggers)
		{
			((Component)parentTrigger2).transform.SetParent(((Component)boat).transform);
			parentTrigger2.associatedMountable = boat;
		}
		return parentTriggers;
	}

	public void ResetTriggerParent()
	{
		Debug.Assert(parentTriggers.Count == originalTriggerParents.Count);
		for (int i = 0; i < parentTriggers.Count; i++)
		{
			((Component)parentTriggers[i]).transform.SetParent(originalTriggerParents[i]);
			((Component)parentTriggers[i]).gameObject.SetActive(false);
		}
		TriggerHurtNotChild triggerHurtNotChild = (currentSkin as BoatConstructionSkin)?.hurtTrigger;
		if (Object.op_Implicit((Object)(object)triggerHurtNotChild))
		{
			triggerHurtNotChild.ClearSourceEntity();
		}
	}

	public override void Hurt(HitInfo info)
	{
		PlayerBoat parentPlayerBoat = PlayerBoat.GetParentPlayerBoat(this);
		if (ForwardDamageToParentBoat && (Object)(object)parentPlayerBoat != (Object)null && !parentPlayerBoat.IsDestructibleWreck)
		{
			parentPlayerBoat.OnBuildingBlockHurt(this, info);
		}
		else
		{
			base.Hurt(info);
		}
	}

	public void RecordDamageTaken(float amount)
	{
		damageTaken += Mathf.Abs(amount);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(5f)]
	public void RPC_WantsPushParentBoat(RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null))
		{
			PlayerBoat parentPlayerBoat = PlayerBoat.GetParentPlayerBoat(this);
			if (!((Object)(object)parentPlayerBoat == (Object)null))
			{
				parentPlayerBoat.RPC_WantsPush(msg);
			}
		}
	}

	public override void AdminKill()
	{
		BaseEntity baseEntity = GetParentEntity();
		if ((Object)(object)baseEntity != (Object)null)
		{
			baseEntity.AdminKill();
		}
		else
		{
			base.AdminKill();
		}
	}

	public override void DoRepair(BasePlayer player)
	{
		if (!PlayerBoat.IsChildOfFinishedPlayerBoat(this) || PlayerBoat.HammerRepairEnabled)
		{
			base.DoRepair(player);
		}
	}

	public override bool ShouldRepairViaParent()
	{
		if (!PlayerBoat.HammerRepairEnabled)
		{
			return false;
		}
		return (Object)(object)PlayerBoat.GetParentPlayerBoat(this) != (Object)null;
	}

	public override BaseCombatEntity GetRepairableParent()
	{
		return PlayerBoat.GetParentPlayerBoat(this);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.boatBuildingBlock = Pool.Get<BoatBuildingBlock>();
			info.msg.boatBuildingBlock.damageTaken = damageTaken;
		}
	}

	public override void OnHealthChanged(float oldvalue, float newvalue)
	{
		if (base.isServer && Mathf.RoundToInt(oldvalue) != Mathf.RoundToInt(newvalue) && SendNetworkUpdateOnHealthChanged)
		{
			SendNetworkUpdate(BasePlayer.NetworkQueue.UpdateDistance);
		}
	}

	public override bool HasDemolishPrivilege(BasePlayer player)
	{
		if ((Object)(object)PlayerBoat.GetParentPlayerBoat(this) != (Object)null)
		{
			return false;
		}
		if (base.isServer)
		{
			if (base.OwnerID == (ulong)player.userID)
			{
				return true;
			}
			BoatBuildingStation forPlayer = BoatBuildingStation.GetForPlayer(player);
			if ((Object)(object)forPlayer != (Object)null)
			{
				return forPlayer.CanPlayerDemolish(player);
			}
			return false;
		}
		return false;
	}
}

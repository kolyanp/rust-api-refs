using System.Collections.Generic;
using ConVar;
using UnityEngine;

public class TriggerSafeZoneOverride : TriggerBase, IServerComponent
{
	public static List<TriggerSafeZoneOverride> allHostileZones = new List<TriggerSafeZoneOverride>();

	public Collider triggerCollider { get; private set; }

	public ApartmentRoom Apartment { get; set; }

	public bool IsCombatActive
	{
		get
		{
			if (!ApartmentCommands.allowcombatoutsideofbreakin && (Object)(object)Apartment != (Object)null)
			{
				return Apartment.IsBreakInActive();
			}
			return true;
		}
	}

	protected override void Awake()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		triggerCollider = ((Component)this).GetComponent<Collider>();
		base.InterestLayers = LayerMask.op_Implicit(LayerMask.op_Implicit(base.InterestLayers) | 0x200);
		Apartment = ((Component)this).GetComponentInParent<ApartmentRoom>();
	}

	protected void OnEnable()
	{
		allHostileZones.Add(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		allHostileZones.Remove(this);
	}

	internal override void OnEntityEnter(BaseEntity ent)
	{
		base.OnEntityEnter(ent);
		if ((Object)(object)Apartment != (Object)null && Apartment.isServer && ent is BasePlayer { IsBot: false, isServer: not false } basePlayer)
		{
			Apartment.OnPlayerEnterCombatZone(basePlayer);
		}
	}
}

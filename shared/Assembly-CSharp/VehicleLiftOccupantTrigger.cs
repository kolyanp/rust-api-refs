using Rust;
using UnityEngine;

public class VehicleLiftOccupantTrigger : TriggerBase
{
	public bool checkNonModularCarVehicles;

	public ModularCar carOccupant { get; private set; }

	public BaseVehicle vehicleOccupant { get; private set; }

	protected override void OnDisable()
	{
		if (!Application.isQuitting)
		{
			base.OnDisable();
			if ((Object)(object)carOccupant != (Object)null)
			{
				carOccupant = null;
			}
			if ((Object)(object)vehicleOccupant != (Object)null)
			{
				vehicleOccupant = null;
			}
		}
	}

	internal override GameObject InterestedInObject(GameObject obj)
	{
		if ((Object)(object)base.InterestedInObject(obj) == (Object)null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if ((Object)(object)baseEntity == (Object)null || baseEntity.isClient)
		{
			return null;
		}
		if (checkNonModularCarVehicles)
		{
			if (!(baseEntity is BaseVehicle))
			{
				return null;
			}
		}
		else if (!(baseEntity is ModularCar))
		{
			return null;
		}
		return obj;
	}

	internal override void OnEntityEnter(BaseEntity ent)
	{
		base.OnEntityEnter(ent);
		if (checkNonModularCarVehicles)
		{
			if ((Object)(object)vehicleOccupant == (Object)null && ent.isServer)
			{
				vehicleOccupant = (BaseVehicle)ent;
			}
			if ((Object)(object)carOccupant == (Object)null && ent.isServer && ent is ModularCar modularCar)
			{
				carOccupant = modularCar;
			}
		}
		else if ((Object)(object)carOccupant == (Object)null && ent.isServer)
		{
			carOccupant = (ModularCar)ent;
		}
	}

	internal override void OnEntityLeave(BaseEntity ent)
	{
		base.OnEntityLeave(ent);
		if (!((Object)(object)carOccupant == (Object)(object)ent) && (!checkNonModularCarVehicles || !((Object)(object)vehicleOccupant == (Object)(object)ent)))
		{
			return;
		}
		vehicleOccupant = null;
		carOccupant = null;
		if (entityContents == null || entityContents.Count <= 0)
		{
			return;
		}
		foreach (BaseEntity entityContent in entityContents)
		{
			if (!((Object)(object)entityContent != (Object)null))
			{
				continue;
			}
			if (checkNonModularCarVehicles)
			{
				vehicleOccupant = (BaseVehicle)entityContent;
				if (entityContent is ModularCar modularCar)
				{
					carOccupant = modularCar;
				}
			}
			else
			{
				carOccupant = (ModularCar)entityContent;
			}
			break;
		}
	}
}

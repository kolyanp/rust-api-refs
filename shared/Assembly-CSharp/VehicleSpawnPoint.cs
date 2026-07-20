using UnityEngine;

public class VehicleSpawnPoint : SpaceCheckingSpawnPoint
{
	public override void ObjectSpawned(SpawnPointInstance instance)
	{
		base.ObjectSpawned(instance);
		AddStartingFuel(GameObjectEx.ToBaseEntity(((Component)instance).gameObject) as VehicleSpawner.IVehicleSpawnUser);
	}

	public static void AddStartingFuel(VehicleSpawner.IVehicleSpawnUser vehicle)
	{
		vehicle?.GetFuelSystem()?.AddFuel(vehicle.StartingFuelUnits());
	}

	public static void AddStartingFlares(ICanFireHelicopterFlares vehicleWithFlares)
	{
		if (vehicleWithFlares != null)
		{
			HelicopterFlares flaresInstance = vehicleWithFlares.FlaresInstance;
			if ((Object)(object)flaresInstance != (Object)null)
			{
				flaresInstance.RefillFlares();
			}
		}
	}
}

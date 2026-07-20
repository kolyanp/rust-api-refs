using System;
using System.Collections.Generic;
using Facepunch;
using Facepunch.Rust;
using UnityEngine;

public class VehicleSpawner : BaseEntity
{
	public interface IVehicleSpawnUser
	{
		string ShortPrefabName { get; }

		bool IsClient { get; }

		bool IsDestroyed { get; }

		void SetupOwner(BasePlayer owner, Vector3 newSafeAreaOrigin, float newSafeAreaRadius);

		bool IsDespawnEligable();

		IFuelSystem GetFuelSystem();

		int StartingFuelUnits();

		void Kill(DestroyMode mode, bool runCallbacks);
	}

	[Serializable]
	public class SpawnPair
	{
		public string message;

		public GameObjectRef prefabToSpawn;
	}

	public float spawnNudgeRadius = 6f;

	public float cleanupRadius = 10f;

	public float occupyRadius = 5f;

	public TriggerBase additionalNudgeTrigger;

	public SpawnPair[] objectsToSpawn;

	public Transform spawnOffset;

	public float safeRadius = 10f;

	protected virtual bool LogAnalytics => true;

	public virtual int GetOccupyLayer()
	{
		return 32768;
	}

	public IVehicleSpawnUser GetVehicleOccupying()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		IVehicleSpawnUser result = null;
		List<IVehicleSpawnUser> list = Pool.Get<List<IVehicleSpawnUser>>();
		Vis.Entities(((Component)spawnOffset).transform.position, occupyRadius, list, GetOccupyLayer(), (QueryTriggerInteraction)1);
		if (list.Count > 0)
		{
			result = list[0];
		}
		Pool.FreeUnmanaged<IVehicleSpawnUser>(ref list);
		return result;
	}

	public bool IsPadOccupied()
	{
		IVehicleSpawnUser vehicleOccupying = GetVehicleOccupying();
		if (vehicleOccupying != null)
		{
			return !vehicleOccupying.IsDespawnEligable();
		}
		return false;
	}

	public override void OnEntityMessage(BaseEntity from, string msg)
	{
		BasePlayer newOwner = null;
		NPCTalking component = ((Component)from).GetComponent<NPCTalking>();
		if (Object.op_Implicit((Object)(object)component))
		{
			newOwner = component.GetActionPlayer();
		}
		SpawnPair[] array = objectsToSpawn;
		foreach (SpawnPair spawnPair in array)
		{
			if (msg == spawnPair.message)
			{
				SpawnVehicle(spawnPair.prefabToSpawn.resourcePath, newOwner);
				break;
			}
		}
	}

	public IVehicleSpawnUser SpawnVehicle(string prefabToSpawn, BasePlayer newOwner)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		CleanupArea(cleanupRadius);
		NudgePlayersInRadius(spawnNudgeRadius);
		BaseEntity baseEntity = GameManager.server.CreateEntity(prefabToSpawn, ((Component)spawnOffset).transform.position, ((Component)spawnOffset).transform.rotation);
		baseEntity.Spawn();
		IVehicleSpawnUser component = ((Component)baseEntity).GetComponent<IVehicleSpawnUser>();
		if ((Object)(object)newOwner != (Object)null)
		{
			component.SetupOwner(newOwner, ((Component)spawnOffset).transform.position, safeRadius);
		}
		VehicleSpawnPoint.AddStartingFuel(component);
		VehicleSpawnPoint.AddStartingFlares(((Component)baseEntity).GetComponent<ICanFireHelicopterFlares>());
		if ((Object)(object)newOwner != (Object)null)
		{
			Analytics.Azure.OnVehiclePurchased(newOwner, baseEntity);
		}
		return component;
	}

	public void CleanupArea(float radius)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		List<IVehicleSpawnUser> list = Pool.Get<List<IVehicleSpawnUser>>();
		Vis.Entities(((Component)spawnOffset).transform.position, radius, list, 32768, (QueryTriggerInteraction)2);
		foreach (IVehicleSpawnUser item in list)
		{
			if (!item.IsClient && !item.IsDestroyed)
			{
				item.Kill(DestroyMode.None, runCallbacks: true);
			}
		}
		List<ServerGib> list2 = Pool.Get<List<ServerGib>>();
		Vis.Entities(((Component)spawnOffset).transform.position, radius, list2, -2147483647, (QueryTriggerInteraction)2);
		foreach (ServerGib item2 in list2)
		{
			if (!item2.isClient)
			{
				item2.Kill();
			}
		}
		Pool.FreeUnmanaged<IVehicleSpawnUser>(ref list);
		Pool.FreeUnmanaged<ServerGib>(ref list2);
	}

	public void NudgePlayersInRadius(float radius)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
		Vis.Entities(((Component)spawnOffset).transform.position, radius, list, 131072, (QueryTriggerInteraction)2);
		foreach (BasePlayer item in list)
		{
			if ((!Object.op_Implicit((Object)(object)additionalNudgeTrigger) || (additionalNudgeTrigger.HasAnyEntityContents && additionalNudgeTrigger.entityContents.Contains(item))) && !item.IsNpc && !item.isMounted && item.IsConnected)
			{
				Vector3 position = ((Component)spawnOffset).transform.position;
				position += Vector3Ex.Direction2D(((Component)item).transform.position, ((Component)spawnOffset).transform.position) * radius;
				position += Vector3.up * 0.1f;
				item.MovePosition(position);
				item.ClientRPC(RpcTarget.Player("ForcePositionTo", item), position);
			}
		}
		Pool.FreeUnmanaged<BasePlayer>(ref list);
	}
}

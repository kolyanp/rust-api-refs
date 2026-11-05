using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Rust;
using UnityEngine;

public class RepairableVehiclePad : RepairableStructure
{
	public static ListHashSet<RepairableVehiclePad> server_RepairableVehiclePads = new ListHashSet<RepairableVehiclePad>();

	public override bool AcceptsRepairs => vehicle.padrepairsrequired;

	public static bool ShouldSpawnAsRepaired => !vehicle.padrepairsrequired;

	public static int ServerSetAllRepaired(bool repaired)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		Enumerator<RepairableVehiclePad> enumerator = server_RepairableVehiclePads.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				RepairableVehiclePad current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && !current.IsDestroyed)
				{
					current.ForceRepairedState(repaired);
					num++;
				}
			}
			return num;
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		server_RepairableVehiclePads.TryAdd(this);
		if (ShouldSpawnAsRepaired)
		{
			ForceRepairedState(repaired: true);
		}
		if (!Application.isLoadingSave)
		{
			LinkVehicleSpawners();
		}
	}

	private void LinkVehicleSpawners()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		List<VehicleSpawner> list = Pool.Get<List<VehicleSpawner>>();
		Vis.Entities(WorldSpaceBounds(), list, 1218652417, (QueryTriggerInteraction)2);
		foreach (VehicleSpawner item in list)
		{
			if (!item.isClient && item.IsValid())
			{
				item.FindRepairableVehiclePad();
			}
		}
		Pool.FreeUnmanaged<VehicleSpawner>(ref list);
	}

	internal override void DoServerDestroy()
	{
		server_RepairableVehiclePads.Remove(this);
		base.DoServerDestroy();
	}
}

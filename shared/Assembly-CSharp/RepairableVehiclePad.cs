using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Rust;
using UnityEngine;

public class RepairableVehiclePad : ConstructableEntity
{
	public const Flags Flag_Repaired = Flags.Reserved1;

	[Tooltip("Root holding the damaged build stages. Switched off once the pad is fully repaired.")]
	[Header("Repairable Vehicle Pad")]
	public GameObject damagedVisuals;

	[Tooltip("Root holding the intact pad. Only shown once the pad is fully repaired.")]
	public GameObject repairedVisuals;

	public GameObjectRef repairedEffect;

	public static ListHashSet<RepairableVehiclePad> server_RepairableVehiclePads = new ListHashSet<RepairableVehiclePad>();

	public bool IsRepaired => HasFlag(Flags.Reserved1);

	public override bool AcceptsRepairs => vehicle.padrepairsrequired;

	public static bool ShouldSpawnAsRepaired => !vehicle.padrepairsrequired;

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		bool flag = (old & Flags.Reserved1) == Flags.Reserved1;
		bool flag2 = (next & Flags.Reserved1) == Flags.Reserved1;
		if (flag != flag2)
		{
			UpdateRepairedVisuals();
		}
	}

	private void UpdateRepairedVisuals()
	{
		bool isRepaired = IsRepaired;
		if ((Object)(object)damagedVisuals != (Object)null)
		{
			damagedVisuals.SetActive(!isRepaired);
		}
		if ((Object)(object)repairedVisuals != (Object)null)
		{
			repairedVisuals.SetActive(isRepaired);
		}
	}

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
		UpdateRepairedVisuals();
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

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		SetRepaired(IsFullyBuilt());
	}

	internal override void DoServerDestroy()
	{
		server_RepairableVehiclePads.Remove(this);
		base.DoServerDestroy();
	}

	protected override void OnConstructionComplete(BasePlayer player)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		SetRepaired(repaired: true);
		if (repairedEffect.isValid)
		{
			Effect.server.Run(repairedEffect.resourcePath, ((Component)this).transform.position, Vector3.up);
		}
	}

	public void ForceRepairedState(bool repaired)
	{
		ForceBuildProgress(repaired);
		SetRepaired(repaired);
	}

	private void SetRepaired(bool repaired)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, repaired);
	}
}

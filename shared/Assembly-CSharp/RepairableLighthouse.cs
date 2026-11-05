using System;
using ConVar;
using UnityEngine;

public class RepairableLighthouse : RepairableStructure
{
	public static ListHashSet<RepairableLighthouse> server_RepairableLighthouses = new ListHashSet<RepairableLighthouse>();

	public override bool AcceptsRepairs => DeepSea.enabled;

	public static bool IsAnyRepaired()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<RepairableLighthouse> enumerator = server_RepairableLighthouses.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.IsRepaired)
				{
					return true;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return false;
	}

	public static void RepairAll()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<RepairableLighthouse> enumerator = server_RepairableLighthouses.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				RepairableLighthouse current = enumerator.Current;
				if (!current.IsRepaired)
				{
					current.ForceRepairedState(repaired: true);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		server_RepairableLighthouses.TryAdd(this);
		if (!DeepSea.enabled)
		{
			ForceRepairedState(repaired: true);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (!DeepSea.enabled)
		{
			ForceRepairedState(repaired: true);
		}
	}

	internal override void DoServerDestroy()
	{
		server_RepairableLighthouses.Remove(this);
		base.DoServerDestroy();
	}

	public override void OnRepair()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		base.OnRepair();
		Enumerator<RepairableLighthouse> enumerator = server_RepairableLighthouses.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				RepairableLighthouse current = enumerator.Current;
				if ((Object)(object)current != (Object)(object)this)
				{
					current.CopyBuildProgress(this);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	protected override void OnConstructionComplete(BasePlayer player)
	{
		base.OnConstructionComplete(player);
		RepairAll();
		if ((Object)(object)PointEntity<DeepSeaManager>.ServerInstance != (Object)null)
		{
			PointEntity<DeepSeaManager>.ServerInstance.OnLighthouseRepaired();
		}
	}
}

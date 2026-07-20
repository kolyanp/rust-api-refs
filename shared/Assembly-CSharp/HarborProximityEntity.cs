using System;
using UnityEngine;

public class HarborProximityEntity : BaseEntity
{
	public bool SupportChildDeployables;

	public const Flags IsMoving = Flags.Reserved1;

	private static ListHashSet<HarborProximityEntity> harborEntities = new ListHashSet<HarborProximityEntity>();

	public override void ServerInit()
	{
		base.ServerInit();
		harborEntities.Add(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		harborEntities.Remove(this);
	}

	public static HarborProximityEntity GetEntity(Vector3 worldPos)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<HarborProximityEntity> enumerator = harborEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				HarborProximityEntity current = enumerator.Current;
				if (Vector3.Distance(Vector3Ex.WithY(((Component)current).transform.position, worldPos.y), worldPos) < 3f)
				{
					return current;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return null;
	}

	public void NotifyStart()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b: true);
	}

	public void NotifyEnd()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b: false);
	}

	public override bool SupportsChildDeployables()
	{
		return SupportChildDeployables;
	}
}

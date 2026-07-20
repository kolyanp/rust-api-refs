using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Spatial;
using UnityEngine;

public class OverfishedArea : BaseEntity
{
	public static Grid<OverfishedArea> OverfishedGrid = new Grid<OverfishedArea>(32, 8096f);

	public override void ServerInit()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		OverfishedGrid.Add(this, ((Component)this).transform.position.x, ((Component)this).transform.position.z);
		Invoke(KillMe, 60f * Fishing.overfishedAreaDurationMinutes);
	}

	private void KillMe()
	{
		Kill();
	}

	internal override void DoServerDestroy()
	{
		OverfishedGrid.Remove(this);
		base.DoServerDestroy();
	}

	public static OverfishedArea GetOverfishedAreaAtPosition(Vector3 position)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("OverfishedArea.GetOverfishedAreaAtPosition()"))
		{
			PooledList<OverfishedArea> val = Pool.Get<PooledList<OverfishedArea>>();
			try
			{
				OverfishedGrid.Query(position.x, position.z, Fishing.overfishedAreaRadius, (List<OverfishedArea>)(object)val);
				foreach (OverfishedArea item in (List<OverfishedArea>)(object)val)
				{
					if (Fishing.debugOverfishing)
					{
						Debug.Log((object)$"OVERFISHED AREA QUERY | Found an area at position {position}, at distance: {Vector3.Distance(((Component)item).transform.position, position)}");
					}
					if (Vector3.Distance(((Component)item).transform.position, position) < Fishing.overfishedAreaRadius)
					{
						if (Fishing.debugOverfishing)
						{
							Debug.Log((object)$"OVERFISHED AREA QUERY | Accepting area at position {position} as overfished!", (Object)(object)item);
						}
						return item;
					}
				}
				return null;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}
}

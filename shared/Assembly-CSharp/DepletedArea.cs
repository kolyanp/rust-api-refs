using System;
using System.Collections.Generic;
using Facepunch;
using Spatial;
using UnityEngine;

public abstract class DepletedArea : BaseEntity
{
	public static Grid<DepletedArea> DepletedGrid = new Grid<DepletedArea>(32, 8096f);

	protected abstract float Radius { get; }

	protected abstract float DurationMinutes { get; }

	protected abstract bool DebugEnabled { get; }

	public override void ServerInit()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		DepletedGrid.Add(this, ((Component)this).transform.position.x, ((Component)this).transform.position.z);
		Invoke(KillMe, 60f * DurationMinutes);
	}

	private void KillMe()
	{
		Kill();
	}

	internal override void DoServerDestroy()
	{
		DepletedGrid.Remove(this);
		base.DoServerDestroy();
	}

	protected static T GetAtPosition<T>(Vector3 position, float searchRadius) where T : DepletedArea
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("DepletedArea.GetAtPosition()"))
		{
			PooledList<T> val = Pool.Get<PooledList<T>>();
			try
			{
				DepletedGrid.Query<T>(position.x, position.z, searchRadius, (List<T>)(object)val);
				foreach (T item in (List<T>)(object)val)
				{
					if (!(Vector3.Distance(((Component)item).transform.position, position) >= item.Radius))
					{
						if (item.DebugEnabled)
						{
							Debug.Log((object)$"DEPLETED AREA QUERY | {item.ShortPrefabName} covers position {position}", (Object)(object)item);
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

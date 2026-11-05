using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class ScientistBoatOilrigManager : MonoBehaviour
{
	private const float ABANDONED_REMOVE_DISTANCE = 500f;

	private BoatGroupSpawner _spawner;

	private HashSet<RHIB> _spawnedBoats = new HashSet<RHIB>();

	public void OnPuzzleReset()
	{
		if (_spawnedBoats == null)
		{
			_spawnedBoats = new HashSet<RHIB>();
		}
		if (_spawnedBoats.Count > 0)
		{
			PooledList<RHIB> val = Pool.Get<PooledList<RHIB>>();
			try
			{
				((List<RHIB>)(object)val).AddRange((IEnumerable<RHIB>)_spawnedBoats);
				foreach (RHIB item in (List<RHIB>)(object)val)
				{
					if ((Object)(object)item != (Object)null && !item.IsDestroyed && ShouldRemove(item))
					{
						item.AdminKillNoLoot();
					}
				}
				_spawnedBoats.Clear();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		if ((Object)(object)_spawner == (Object)null)
		{
			_spawner = ((Component)this).GetComponent<BoatGroupSpawner>();
		}
		_spawner.SpawnBoatGroup(_spawnedBoats, BoatAI.AILoadMode.KillBoat);
	}

	private bool ShouldRemove(RHIB boat)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)((Component)boat).GetComponentInChildren<BoatAI>() != (Object)null)
		{
			return true;
		}
		if (!boat.HasPlayerAboard())
		{
			return Vector3Ex.Distance2D(((Component)boat).transform.position, ((Component)this).transform.position) < 500f;
		}
		return false;
	}
}

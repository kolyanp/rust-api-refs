using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class ScientistBoatOilrigManager : MonoBehaviour
{
	private BoatGroupSpawner _spawner;

	private HashSet<RHIB> _spawnedBoats = new HashSet<RHIB>();

	public void AIDestroyed(RHIB rhib)
	{
		_spawnedBoats.Remove(rhib);
	}

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
					if ((Object)(object)item != (Object)null && !item.IsDestroyed)
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
		_spawner.SpawnBoatGroup(_spawnedBoats, BoatAI.AILoadMode.KillBoat, spawnsPT: false, this);
	}
}

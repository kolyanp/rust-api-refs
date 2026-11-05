using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class RoamingBoatGroups : TriggeredEvent
{
	public int maxGroups = 2;

	public float minLifetimeMinutes = 30f;

	public float maxLifetimeMinutes = 60f;

	private readonly List<RHIB> _spawnedBoats = new List<RHIB>();

	public override void RunEvent()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		if (AnyStillOut())
		{
			return;
		}
		int num = Random.Range(1, maxGroups + 1);
		for (int i = 0; i < num; i++)
		{
			if (!BoatAI.TryGetMapEdgeSpawn(out var position, out var destination))
			{
				Debug.Log((object)"[event] Failed to find a spawn position for a roaming boat group");
				break;
			}
			Debug.Log((object)$"[event] Spawning roaming boat group at {position}, heading to {destination}");
			PooledHashSet<RHIB> val = Pool.Get<PooledHashSet<RHIB>>();
			try
			{
				BoatAI.SpawnBoatGroup(new Vector2(position.x, position.z), Quaternion.LookRotation(Vector3Ex.WithY(TerrainMeta.Center - position, 0f)), (HashSet<RHIB>)(object)val);
				float num2 = Random.Range(minLifetimeMinutes, maxLifetimeMinutes) * 60f;
				foreach (RHIB boat in (HashSet<RHIB>)(object)val)
				{
					BoatAI componentInChildren = ((Component)boat).GetComponentInChildren<BoatAI>();
					componentInChildren.Roaming = true;
					componentInChildren.LoadMode = BoatAI.AILoadMode.KillBoat;
					componentInChildren.MoveTo(destination);
					componentInChildren.LeaveAfter(num2);
					boat.Invoke(() =>
					{
						RemoveBoat(boat);
					}, num2 + 60f * BoatAI.roaming_egress_minutes);
					_spawnedBoats.Add(boat);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	private bool AnyStillOut()
	{
		for (int num = _spawnedBoats.Count - 1; num >= 0; num--)
		{
			if (!_spawnedBoats[num].IsValid())
			{
				_spawnedBoats.RemoveAt(num);
			}
		}
		return _spawnedBoats.Count > 0;
	}

	private void RemoveBoat(RHIB boat)
	{
		_spawnedBoats.Remove(boat);
		if (!boat.HasPlayerAboard())
		{
			boat.AdminKillNoLoot();
		}
	}

	public override void Kill()
	{
		base.Kill();
		for (int i = 0; i < _spawnedBoats.Count; i++)
		{
			if (_spawnedBoats[i].IsValid() && !_spawnedBoats[i].HasPlayerAboard())
			{
				_spawnedBoats[i].AdminKillNoLoot();
			}
		}
		_spawnedBoats.Clear();
	}
}

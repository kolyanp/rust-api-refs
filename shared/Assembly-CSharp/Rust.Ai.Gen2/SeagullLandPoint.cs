using UnityEngine;

namespace Rust.Ai.Gen2;

public class SeagullLandPoint : GenericSpawnPoint
{
	private SeagullGroup claimant;

	private bool IsClaimed
	{
		get
		{
			if (claimant.IsValid())
			{
				return !claimant.IsDead();
			}
			return false;
		}
	}

	private bool IsClaimedByOther(SeagullGroup group)
	{
		if (IsClaimed)
		{
			return (Object)(object)claimant != (Object)(object)group;
		}
		return false;
	}

	public override bool IsAvailableTo(GameObject prefab)
	{
		if (base.IsAvailableTo(prefab))
		{
			return !IsClaimed;
		}
		return false;
	}

	public bool TryClaim(SeagullGroup group)
	{
		if (IsClaimedByOther(group))
		{
			return false;
		}
		claimant = group;
		return true;
	}

	public void Release(SeagullGroup group)
	{
		if ((Object)(object)claimant == (Object)(object)group)
		{
			claimant = null;
		}
	}

	public static SeagullLandPoint FindFree(SeagullGroup group, Vector3 position, float minDistance, float maxDistance)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (!BaseSpawnPoint.spawnPoints.TryGetValue(SpawnPointType.Seagull, out var value))
		{
			return null;
		}
		SeagullLandPoint result = null;
		int num = 0;
		float num2 = minDistance * minDistance;
		float num3 = maxDistance * maxDistance;
		foreach (BaseSpawnPoint item in value)
		{
			if (item is SeagullLandPoint seagullLandPoint && !((Object)(object)seagullLandPoint == (Object)(object)group.LandPoint) && !seagullLandPoint.IsClaimedByOther(group))
			{
				Vector3 val = ((Component)seagullLandPoint).transform.position - position;
				float sqrMagnitude = val.sqrMagnitude;
				if (!(sqrMagnitude < num2) && !(sqrMagnitude > num3) && Random.Range(0, ++num) == 0)
				{
					result = seagullLandPoint;
				}
			}
		}
		return result;
	}

	public static SeagullLandPoint FindNearestFree(SeagullGroup group, Vector3 position)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!BaseSpawnPoint.spawnPoints.TryGetValue(SpawnPointType.Seagull, out var value))
		{
			return null;
		}
		SeagullLandPoint result = null;
		float num = float.MaxValue;
		foreach (BaseSpawnPoint item in value)
		{
			if (item is SeagullLandPoint seagullLandPoint && !seagullLandPoint.IsClaimedByOther(group))
			{
				Vector3 val = ((Component)seagullLandPoint).transform.position - position;
				float sqrMagnitude = val.sqrMagnitude;
				if (!(sqrMagnitude >= num))
				{
					result = seagullLandPoint;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}
}

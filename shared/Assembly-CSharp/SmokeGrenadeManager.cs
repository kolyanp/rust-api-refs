using System.Collections.Generic;
using Spatial;
using UnityEngine;

public class SmokeGrenadeManager : SingletonComponent<SmokeGrenadeManager>
{
	private const float worldSize = 8096f;

	private const int cellSize = 32;

	private Grid<BaseEntity> smokeGrid = new Grid<BaseEntity>(32, 8096f);

	public void Add(BaseEntity smoke)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (IsSmokeGrenade(smoke))
		{
			smokeGrid.Add(smoke, ((Component)smoke).transform.position.x, ((Component)smoke).transform.position.z);
		}
	}

	public void Move(BaseEntity smoke)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SmokeGrenadeManager.Move"))
		{
			Vector3 position = ((Component)smoke).transform.position;
			smokeGrid.Move(smoke, position.x, position.z);
		}
	}

	public void Remove(BaseEntity smoke)
	{
		if (IsSmokeGrenade(smoke))
		{
			smokeGrid.Remove(smoke);
		}
	}

	public void GetSmokeAround(Vector3 position, float range, List<BaseEntity> results)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SmokeGrenadeManager.GetSmokeAround"))
		{
			if (smokeGrid != null)
			{
				smokeGrid.Query(position.x, position.z, range, results);
			}
		}
	}

	public static bool IsSmokeGrenade(BaseEntity entity)
	{
		if (entity is SmokeGrenade)
		{
			return true;
		}
		return false;
	}
}

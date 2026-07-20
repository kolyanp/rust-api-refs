using UnityEngine;

public class ValidBounds : SingletonComponent<ValidBounds>
{
	public Bounds worldBounds;

	public static bool Test(BaseEntity entity, Vector3 vPos)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)SingletonComponent<ValidBounds>.Instance))
		{
			return true;
		}
		if ((Object)(object)entity != (Object)null && entity.net != null && entity.net.group != null && TutorialIsland.IsTutorialNetworkGroup(entity.net.group.ID))
		{
			return SingletonComponent<ValidBounds>.Instance.IsInsideOuterBounds(vPos);
		}
		return SingletonComponent<ValidBounds>.Instance.IsInsideInnerBounds(vPos);
	}

	public static float TestDist(BaseEntity entity, Vector3 vPos)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)SingletonComponent<ValidBounds>.Instance))
		{
			return float.MaxValue;
		}
		if ((Object)(object)entity != (Object)null && entity.net != null && entity.net.group != null && TutorialIsland.IsTutorialNetworkGroup(entity.net.group.ID))
		{
			return float.MaxValue;
		}
		return SingletonComponent<ValidBounds>.Instance.DistToWorldEdge2D(vPos);
	}

	public static bool TestInnerBounds(Vector3 vPos)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)SingletonComponent<ValidBounds>.Instance))
		{
			return true;
		}
		return SingletonComponent<ValidBounds>.Instance.IsInsideInnerBounds(vPos);
	}

	public static bool TestOuterBounds(Vector3 vPos)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)SingletonComponent<ValidBounds>.Instance))
		{
			return true;
		}
		return SingletonComponent<ValidBounds>.Instance.IsInsideOuterBounds(vPos);
	}

	internal bool IsInsideInnerBounds(Vector3 vPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3Ex.IsNaNOrInfinity(vPos))
		{
			return false;
		}
		if (DeepSeaManager.IsInsideDeepSea(vPos))
		{
			return true;
		}
		if (!((Bounds)(ref worldBounds)).Contains(vPos))
		{
			return false;
		}
		if ((bool)TerrainMeta.TerrainRenderer)
		{
			if (World.Procedural && vPos.y < TerrainMeta.Position.y)
			{
				return false;
			}
			if (TerrainMeta.OutOfMargin(vPos))
			{
				return false;
			}
		}
		return true;
	}

	internal bool IsInsideOuterBounds(Vector3 vPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3Ex.IsNaNOrInfinity(vPos))
		{
			return false;
		}
		if (DeepSeaManager.IsInsideDeepSea(vPos))
		{
			return true;
		}
		Bounds val = default(Bounds);
		((Bounds)(ref val))._002Ector(((Bounds)(ref worldBounds)).center, new Vector3(((Bounds)(ref worldBounds)).size.x + TutorialIsland.TutorialBoundsSize * 2f, ((Bounds)(ref worldBounds)).size.y, ((Bounds)(ref worldBounds)).size.z + TutorialIsland.TutorialBoundsSize * 2f));
		if (!((Bounds)(ref val)).Contains(vPos))
		{
			return false;
		}
		if ((bool)TerrainMeta.TerrainRenderer)
		{
			if (World.Procedural && vPos.y < TerrainMeta.Position.y)
			{
				return false;
			}
			if (TerrainMeta.OutOfMarginPlusTutorialBounds(vPos))
			{
				return false;
			}
		}
		return true;
	}

	public static float GetMaximumPointTutorial()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)SingletonComponent<ValidBounds>.Instance == (Object)null)
		{
			return 0f;
		}
		return Mathf.Min(TerrainMeta.Position.x + TerrainMeta.Size.x * 2f + TutorialIsland.TutorialBoundsSize, ((Bounds)(ref SingletonComponent<ValidBounds>.Instance.worldBounds)).extents.x + TutorialIsland.TutorialBoundsSize);
	}

	public static float GetMaximumPoint()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)SingletonComponent<ValidBounds>.Instance == (Object)null)
		{
			return 0f;
		}
		float num = ((Bounds)(ref SingletonComponent<ValidBounds>.Instance.worldBounds)).max.x;
		if ((bool)TerrainMeta.TerrainRenderer)
		{
			num = Mathf.Min(TerrainMeta.Position.x + TerrainMeta.Size.x, num);
		}
		return num;
	}

	internal float DistToWorldEdge2D(Vector3 vPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (!IsInsideInnerBounds(vPos))
		{
			return -1f;
		}
		float num = BoundsEx.InnerDistToEdge2D(worldBounds, vPos);
		if ((bool)TerrainMeta.TerrainRenderer)
		{
			float num2 = TerrainMeta.InnerDistToEdge2D(vPos);
			return Mathf.Min(num, num2);
		}
		return num;
	}
}

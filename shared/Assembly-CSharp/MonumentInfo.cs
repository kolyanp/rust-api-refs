using UnityEngine;

public class MonumentInfo : LandmarkInfo, IPrefabPreProcess
{
	public enum DetatchChildrenMode
	{
		Default,
		True,
		False
	}

	[Header("MonumentInfo")]
	public MonumentType Type = MonumentType.Building;

	[InspectorFlags]
	public MonumentTier Tier = (MonumentTier)(-1);

	public int MinWorldSize;

	public Bounds Bounds = new Bounds(Vector3.zero, Vector3.zero);

	public bool HasNavmesh;

	public bool IsSafeZone;

	public bool AllowPatrolHeliCrash;

	public DetatchChildrenMode DetachChildren;

	[HideInInspector]
	public bool WantsDungeonLink;

	[HideInInspector]
	public bool HasDungeonLink;

	[HideInInspector]
	public DungeonGridInfo DungeonEntrance;

	private OBB obbBounds;

	bool IPrefabPreProcess.CanRunDuringBundling => false;

	public bool ShouldDetachChildren()
	{
		return false;
	}

	protected override void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		obbBounds = new OBB(((Component)this).transform.position, ((Component)this).transform.rotation, Bounds);
		if (Object.op_Implicit((Object)(object)TerrainMeta.Path) && !DeepSeaManager.IsInsideDeepSea(((Component)this).transform.position))
		{
			TerrainMeta.Path.Monuments.Add(this);
		}
	}

	private void Start()
	{
	}

	public bool CheckPlacement(Vector3 pos, Quaternion rot, Vector3 scale, int minCorners = 3, bool allowAdjacentTier = false)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		OBB val = default(OBB);
		((OBB)(ref val))._002Ector(pos, scale, rot, Bounds);
		Vector3 point = ((OBB)(ref val)).GetPoint(-1f, 0f, -1f);
		Vector3 point2 = ((OBB)(ref val)).GetPoint(-1f, 0f, 1f);
		Vector3 point3 = ((OBB)(ref val)).GetPoint(1f, 0f, -1f);
		Vector3 point4 = ((OBB)(ref val)).GetPoint(1f, 0f, 1f);
		int topology = TerrainMeta.TopologyMap.GetTopology(point);
		int topology2 = TerrainMeta.TopologyMap.GetTopology(point2);
		int topology3 = TerrainMeta.TopologyMap.GetTopology(point3);
		int topology4 = TerrainMeta.TopologyMap.GetTopology(point4);
		int num = (allowAdjacentTier ? TierToMaskWithNeighbours(Tier) : TierToMask(Tier));
		int num2 = 0;
		if ((num & topology) != 0)
		{
			num2++;
		}
		if ((num & topology2) != 0)
		{
			num2++;
		}
		if ((num & topology3) != 0)
		{
			num2++;
		}
		if ((num & topology4) != 0)
		{
			num2++;
		}
		if (num2 < minCorners)
		{
			return false;
		}
		WaterBody componentInChildren = ((Component)this).GetComponentInChildren<WaterBody>();
		if ((Object)(object)componentInChildren == (Object)null || componentInChildren.Type != WaterBodyType.Lake)
		{
			return true;
		}
		if (pos.y + componentInChildren.MinWaterLevel() < 1f)
		{
			return false;
		}
		return true;
	}

	public float Distance(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((OBB)(ref obbBounds)).Distance(position);
	}

	public float SqrDistance(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((OBB)(ref obbBounds)).SqrDistance(position);
	}

	public float Distance(OBB obb)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((OBB)(ref obbBounds)).Distance(obb);
	}

	public float SqrDistance(OBB obb)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((OBB)(ref obbBounds)).SqrDistance(obb);
	}

	public bool IsInBounds(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((OBB)(ref obbBounds)).Contains(position);
	}

	public Vector3 ClosestPointOnBounds(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((OBB)(ref obbBounds)).ClosestPoint(position);
	}

	public PathFinder.Point GetPathFinderPoint(int res)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		float num = TerrainMeta.NormalizeX(position.x);
		float num2 = TerrainMeta.NormalizeZ(position.z);
		return new PathFinder.Point
		{
			x = Mathf.Clamp((int)(num * (float)res), 0, res - 1),
			y = Mathf.Clamp((int)(num2 * (float)res), 0, res - 1)
		};
	}

	public int GetPathFinderRadius(int res)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		float num = ((Bounds)(ref Bounds)).extents.x * TerrainMeta.OneOverSize.x;
		float num2 = ((Bounds)(ref Bounds)).extents.z * TerrainMeta.OneOverSize.z;
		return Mathf.CeilToInt(Mathf.Max(num, num2) * (float)res);
	}

	public float GetWidest2DBound()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		float x = ((Bounds)(ref Bounds)).size.x;
		float z = ((Bounds)(ref Bounds)).size.z;
		return Mathf.Max(x, z);
	}

	protected void OnDrawGizmosSelected()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.matrix = ((Component)this).transform.localToWorldMatrix;
		Gizmos.color = new Color(0f, 0.7f, 1f, 0.1f);
		Gizmos.DrawCube(((Bounds)(ref Bounds)).center, ((Bounds)(ref Bounds)).size);
		Gizmos.color = new Color(0f, 0.7f, 1f, 1f);
		Gizmos.DrawWireCube(((Bounds)(ref Bounds)).center, ((Bounds)(ref Bounds)).size);
	}

	public MonumentNavMesh GetMonumentNavMesh()
	{
		return ((Component)this).GetComponent<MonumentNavMesh>();
	}

	public static int TierToMaskWithNeighbours(MonumentTier tier)
	{
		int num = (int)(tier & (MonumentTier)7);
		return TierToMask((MonumentTier)((num | (num << 1) | (num >> 1)) & 7));
	}

	public static int TierToMask(MonumentTier tier)
	{
		int num = 0;
		if ((tier & MonumentTier.Tier0) != 0)
		{
			num |= 0x4000000;
		}
		if ((tier & MonumentTier.Tier1) != 0)
		{
			num |= 0x8000000;
		}
		if ((tier & MonumentTier.Tier2) != 0)
		{
			num |= 0x10000000;
		}
		return num;
	}

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		HasDungeonLink = DetermineHasDungeonLink();
		WantsDungeonLink = DetermineWantsDungeonLink();
		DungeonEntrance = FindDungeonEntrance();
	}

	private DungeonGridInfo FindDungeonEntrance()
	{
		return ((Component)this).GetComponentInChildren<DungeonGridInfo>();
	}

	private bool DetermineHasDungeonLink()
	{
		return (Object)(object)((Component)this).GetComponentInChildren<DungeonGridLink>() != (Object)null;
	}

	private bool DetermineWantsDungeonLink()
	{
		if (Type == MonumentType.WaterWell)
		{
			return false;
		}
		if (Type == MonumentType.Building && displayPhrase.token.StartsWith("mining_quarry"))
		{
			return false;
		}
		if (Type == MonumentType.Radtown && displayPhrase.token.StartsWith("swamp"))
		{
			return false;
		}
		return true;
	}

	public bool IsOilRig()
	{
		if (displayPhrase.IsValid())
		{
			return displayPhrase.english.ToLower().Contains("oil rig");
		}
		return false;
	}

	public bool IsWaterTreatmentPlant()
	{
		if (displayPhrase.IsValid())
		{
			return displayPhrase.token.ToLower().Contains("water_treatment_plant");
		}
		return false;
	}

	public bool IsPowerPlant()
	{
		if (displayPhrase.IsValid())
		{
			return displayPhrase.token.ToLower().Contains("power_plant");
		}
		return false;
	}
}

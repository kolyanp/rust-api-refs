using Spatial;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

public class WaterVolume : TriggerBase
{
	public Bounds WaterBounds = new Bounds(Vector3.zero, Vector3.one);

	private OBB cachedBounds;

	private Transform cachedTransform;

	public Transform[] cutOffPlanes = (Transform[])(object)new Transform[0];

	[Tooltip("Allows filling bota bags, jugs, etc. Don't turn this on if the player is responsible for filling this water volume as that will allow water duplication")]
	public bool naturalSource;

	public bool waterEnabled = true;

	public static readonly SharedStatic<NativeGrid<WaterVolumeBurstData>> WaterVolumeBoundsGrid = SharedStatic<NativeGrid<WaterVolumeBurstData>>.GetOrCreateUnsafe(0u, 5868069335773950620L, 0L);

	private WaterVolumeBurstData? instanceBurstData;

	private WaterVolumeBurstData AllocateBurstData(Allocator allocator)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		WaterVolumeBurstData result = new WaterVolumeBurstData
		{
			bounds = cachedBounds,
			naturalSource = naturalSource,
			cutOffPlaneMatrices = new NativeArray<Matrix4x4>(cutOffPlanes.Length, allocator, (NativeArrayOptions)1),
			cutOffPlanePoses = new NativeArray<Pose>(cutOffPlanes.Length, allocator, (NativeArrayOptions)1)
		};
		Vector3 val = default(Vector3);
		Quaternion val2 = default(Quaternion);
		for (int i = 0; i < cutOffPlanes.Length; i++)
		{
			if ((Object)(object)cutOffPlanes[i] != (Object)null)
			{
				result.cutOffPlaneMatrices[i] = cutOffPlanes[i].localToWorldMatrix;
				cutOffPlanes[i].GetPositionAndRotation(ref val, ref val2);
				result.cutOffPlanePoses[i] = new Pose(val, val2);
			}
			else
			{
				result.cutOffPlaneMatrices[i] = Matrix4x4.identity;
				result.cutOffPlanePoses[i] = new Pose(Vector3.zero, Quaternion.identity);
			}
		}
		return result;
	}

	protected override void Awake()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		if (!WaterVolumeBoundsGrid.Data.IsCreated)
		{
			WaterVolumeBoundsGrid.Data = new NativeGrid<WaterVolumeBurstData>((Allocator)4, 32, 8096f, false);
		}
	}

	private void OnEnable()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		cachedTransform = ((Component)this).transform;
		cachedBounds = new OBB(cachedTransform, WaterBounds);
		instanceBurstData?.Dispose();
		instanceBurstData = AllocateBurstData((Allocator)4);
		WaterVolumeBoundsGrid.Data.AddUnique(instanceBurstData.Value, ((Component)this).transform.position.x, ((Component)this).transform.position.z);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (instanceBurstData.HasValue)
		{
			if (WaterVolumeBoundsGrid.Data.IsCreated)
			{
				WaterVolumeBoundsGrid.Data.Remove(instanceBurstData.Value);
			}
			instanceBurstData.Value.Dispose();
			instanceBurstData = null;
		}
	}

	private Plane GetWaterPlane()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return new Plane(cachedBounds.up, cachedBounds.position);
	}

	public bool Test(Vector3 pos, out WaterLevel.WaterInfo info)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		if (!waterEnabled)
		{
			info = default(WaterLevel.WaterInfo);
			return false;
		}
		UpdateCachedTransform();
		if (((OBB)(ref cachedBounds)).Contains(pos))
		{
			if (!CheckCutOffPlanes(pos, out var bottomCutY))
			{
				info = default(WaterLevel.WaterInfo);
				return false;
			}
			Plane waterPlane = GetWaterPlane();
			Vector3 val = ((Plane)(ref waterPlane)).ClosestPointOnPlane(pos);
			float y = (val + cachedBounds.up * cachedBounds.extents.y).y;
			float y2 = (val + -cachedBounds.up * cachedBounds.extents.y).y;
			y2 = Mathf.Max(y2, bottomCutY);
			info = default(WaterLevel.WaterInfo);
			info.isValid = true;
			info.artificalWater = !naturalSource;
			info.currentDepth = Mathf.Max(0f, y - pos.y);
			info.overallDepth = Mathf.Max(0f, y - y2);
			info.surfaceLevel = y;
			return true;
		}
		info = default(WaterLevel.WaterInfo);
		return false;
	}

	public bool Test(Bounds bounds, out WaterLevel.WaterInfo info)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		if (!waterEnabled)
		{
			info = default(WaterLevel.WaterInfo);
			return false;
		}
		UpdateCachedTransform();
		if (((OBB)(ref cachedBounds)).Contains(((Bounds)(ref bounds)).ClosestPoint(cachedBounds.position)))
		{
			if (!CheckCutOffPlanes(((Bounds)(ref bounds)).center, out var bottomCutY))
			{
				info = default(WaterLevel.WaterInfo);
				return false;
			}
			Plane waterPlane = GetWaterPlane();
			Vector3 val = ((Plane)(ref waterPlane)).ClosestPointOnPlane(((Bounds)(ref bounds)).center);
			float y = (val + cachedBounds.up * cachedBounds.extents.y).y;
			float y2 = (val + -cachedBounds.up * cachedBounds.extents.y).y;
			y2 = Mathf.Max(y2, bottomCutY);
			info = default(WaterLevel.WaterInfo);
			info.isValid = true;
			info.artificalWater = !naturalSource;
			info.currentDepth = Mathf.Max(0f, y - ((Bounds)(ref bounds)).min.y);
			info.overallDepth = Mathf.Max(0f, y - y2);
			info.surfaceLevel = y;
			return true;
		}
		info = default(WaterLevel.WaterInfo);
		return false;
	}

	public bool Test(Vector3 start, Vector3 end, float radius, out WaterLevel.WaterInfo info)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		if (!waterEnabled)
		{
			info = default(WaterLevel.WaterInfo);
			return false;
		}
		UpdateCachedTransform();
		Vector3 val = (start + end) * 0.5f;
		float num = Mathf.Min(start.y, end.y) - radius;
		if (((OBB)(ref cachedBounds)).Distance(start) < radius || ((OBB)(ref cachedBounds)).Distance(end) < radius)
		{
			if (!CheckCutOffPlanes(val, out var bottomCutY))
			{
				info = default(WaterLevel.WaterInfo);
				return false;
			}
			Plane waterPlane = GetWaterPlane();
			Vector3 val2 = ((Plane)(ref waterPlane)).ClosestPointOnPlane(val);
			float y = (val2 + cachedBounds.up * cachedBounds.extents.y).y;
			float y2 = (val2 + -cachedBounds.up * cachedBounds.extents.y).y;
			y2 = Mathf.Max(y2, bottomCutY);
			info = default(WaterLevel.WaterInfo);
			info.isValid = true;
			info.artificalWater = !naturalSource;
			info.currentDepth = Mathf.Max(0f, y - num);
			info.overallDepth = Mathf.Max(0f, y - y2);
			info.surfaceLevel = y;
			return true;
		}
		info = default(WaterLevel.WaterInfo);
		return false;
	}

	private bool CheckCutOffPlanes(Vector3 pos, out float bottomCutY)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		int num = cutOffPlanes.Length;
		bottomCutY = float.MaxValue;
		bool flag = true;
		for (int i = 0; i < num; i++)
		{
			if ((Object)(object)cutOffPlanes[i] != (Object)null)
			{
				Vector3 val = cutOffPlanes[i].InverseTransformPoint(pos);
				Vector3 position = cutOffPlanes[i].position;
				if (Vector3.Dot(cutOffPlanes[i].up, cachedBounds.up) < -0.1f)
				{
					bottomCutY = Mathf.Min(bottomCutY, position.y);
				}
				if (val.y > 0f)
				{
					flag = false;
					break;
				}
			}
		}
		if (!flag)
		{
			return false;
		}
		return true;
	}

	private void UpdateCachedTransform()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)cachedTransform != (Object)null) || !cachedTransform.hasChanged)
		{
			return;
		}
		cachedBounds = new OBB(cachedTransform, WaterBounds);
		if (WaterVolumeBoundsGrid.Data.IsCreated)
		{
			if (instanceBurstData.HasValue)
			{
				WaterVolumeBoundsGrid.Data.Remove(instanceBurstData.Value);
				instanceBurstData.Value.Dispose();
			}
			instanceBurstData = AllocateBurstData((Allocator)4);
			WaterVolumeBoundsGrid.Data.AddUnique(instanceBurstData.Value, ((Component)this).transform.position.x, ((Component)this).transform.position.z);
		}
		cachedTransform.hasChanged = false;
	}

	internal override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if ((Object)(object)obj == (Object)null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if ((Object)(object)baseEntity == (Object)null)
		{
			return null;
		}
		return ((Component)baseEntity).gameObject;
	}
}

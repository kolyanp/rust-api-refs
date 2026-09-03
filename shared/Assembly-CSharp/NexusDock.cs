using System;
using System.Collections.Generic;
using Facepunch.Extend;
using UnityEngine;

public class NexusDock : SingletonComponent<NexusDock>
{
	[Header("Targets")]
	public Transform FerryWaypoint;

	public Transform[] QueuePoints;

	public Transform Arrival;

	public Transform Docking;

	public Transform Docked;

	public Transform CastingOff;

	public Transform Departure;

	[Header("Ferry")]
	public float WaitTime;

	[Header("Ejection")]
	public BoxCollider EjectionZone;

	public float TraceHeight;

	public LayerMask TraceLayerMask;

	public int EjectionAttempts;

	[Range(0f, 1f)]
	public float MinGroundNormal;

	public float MaxGroundVariance;

	private const float SkinWidth = 0.05f;

	private const float MinFootprint = 0.25f;

	private const float CornerInset = 0.8f;

	[NonSerialized]
	public NexusFerry[] QueuedFerries;

	[NonSerialized]
	public NexusFerry CurrentFerry;

	public Transform GetEntryPoint(NexusFerry ferry, out bool entered)
	{
		if ((Object)(object)ferry == (Object)null)
		{
			throw new ArgumentNullException("ferry");
		}
		CleanupQueuedFerries();
		if ((Object)(object)ferry == (Object)(object)CurrentFerry)
		{
			entered = true;
			return Arrival;
		}
		int num = List.FindIndex<NexusFerry>((IReadOnlyList<NexusFerry>)QueuedFerries, ferry, (IEqualityComparer<NexusFerry>)null);
		if (num < 0)
		{
			if ((Object)(object)QueuedFerries[0] == (Object)null)
			{
				QueuedFerries[0] = ferry;
				entered = false;
				return QueuePoints[0];
			}
			entered = false;
			return FerryWaypoint;
		}
		int num2 = QueuedFerries.Length - 1;
		if (num == num2)
		{
			if ((Object)(object)CurrentFerry == (Object)null)
			{
				QueuedFerries[num] = null;
				CurrentFerry = ferry;
				entered = true;
				return Arrival;
			}
			entered = false;
			return QueuePoints[num];
		}
		if (num < num2)
		{
			if ((Object)(object)QueuedFerries[num + 1] == (Object)null)
			{
				QueuedFerries[num] = null;
				QueuedFerries[num + 1] = ferry;
				entered = false;
				return QueuePoints[num + 1];
			}
			entered = false;
			return QueuePoints[num];
		}
		entered = false;
		return QueuePoints[num];
	}

	public bool Depart(NexusFerry ferry)
	{
		if ((Object)(object)ferry != (Object)(object)CurrentFerry)
		{
			return false;
		}
		CurrentFerry = null;
		return true;
	}

	public bool TryFindEjectionPosition(BaseEntity entity, out Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		position = Vector3.zero;
		if ((Object)(object)entity == (Object)null)
		{
			Debug.LogError((object)"Cannot find an eject position without an entity to fit", (Object)(object)this);
			return false;
		}
		if ((Object)(object)EjectionZone == (Object)null)
		{
			Debug.LogError((object)"EjectionZone is null, cannot find an eject position", (Object)(object)this);
			return false;
		}
		Quaternion val = Quaternion.Euler(0f, ((Component)entity).transform.eulerAngles.y, 0f);
		Bounds bounds = entity.bounds;
		Vector3 lossyScale = ((Component)entity).transform.lossyScale;
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(Mathf.Max(Mathf.Abs(((Bounds)(ref bounds)).extents.x * lossyScale.x), 0.25f), Mathf.Max(Mathf.Abs(((Bounds)(ref bounds)).extents.y * lossyScale.y), 0.25f), Mathf.Max(Mathf.Abs(((Bounds)(ref bounds)).extents.z * lossyScale.z), 0.25f));
		Vector3 val3 = val * Vector3.Scale(((Bounds)(ref bounds)).center, lossyScale);
		Vector3 val4 = Vector3.Max(val2 - new Vector3(0.05f, 0.05f, 0.05f), new Vector3(0.05f, 0.05f, 0.05f));
		Transform transform = ((Component)EjectionZone).transform;
		Vector3 size = EjectionZone.size;
		float num = transform.position.y - size.y / 2f;
		bool flag = false;
		Vector3 val5 = Vector3.zero;
		Vector3 val8 = default(Vector3);
		RaycastHit val9 = default(RaycastHit);
		Vector3 val10 = default(Vector3);
		for (int i = 0; i < EjectionAttempts; i++)
		{
			Vector3 val6 = Vector3Ex.Scale(size, Random.value - 0.5f, 0f, Random.value - 0.5f);
			Vector3 val7 = transform.TransformPoint(val6);
			((Vector3)(ref val8))._002Ector(val7.x + val3.x, num + TraceHeight + val4.y, val7.z + val3.z);
			if (!Physics.BoxCast(val8, val4, Vector3.down, ref val9, val, TraceHeight + size.y, LayerMask.op_Implicit(TraceLayerMask), (QueryTriggerInteraction)1) || ((RaycastHit)(ref val9)).normal.y < MinGroundNormal)
			{
				continue;
			}
			float num2 = val8.y - ((RaycastHit)(ref val9)).distance - val4.y;
			if (num2 < val7.y - size.y || num2 > val7.y + size.y)
			{
				continue;
			}
			float waterSurface = WaterLevel.GetWaterSurface(val7, waves: false, volumes: false);
			if (!(num2 < waterSurface))
			{
				((Vector3)(ref val10))._002Ector(val8.x, num2 + val2.y, val8.z);
				Vector3 val11 = val10 - val3;
				if (!flag)
				{
					flag = true;
					val5 = val11;
				}
				if (IsRestingOnGround(val10, val2, val, num2, waterSurface) && !GamePhysics.CheckOBBAndEntity(new OBB(val10 + Vector3.up * MaxGroundVariance, val2 * 2f, val), LayerMask.op_Implicit(TraceLayerMask), (QueryTriggerInteraction)1, entity))
				{
					position = val11;
					return true;
				}
			}
		}
		if (flag)
		{
			position = val5;
			return true;
		}
		return false;
	}

	private bool IsRestingOnGround(Vector3 restingCenter, Vector3 extents, Quaternion rotation, float groundHeight, float waterHeight)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		float num = groundHeight + MaxGroundVariance;
		float num2 = MaxGroundVariance * 2f;
		RaycastHit val = default(RaycastHit);
		for (int i = 0; i < 4; i++)
		{
			float num3 = (((i & 1) == 0) ? (-1f) : 1f) * extents.x * 0.8f;
			float num4 = (((i & 2) == 0) ? (-1f) : 1f) * extents.z * 0.8f;
			if (!Physics.Raycast(Vector3Ex.WithY(restingCenter + rotation * new Vector3(num3, 0f, num4), num), Vector3.down, ref val, num2, LayerMask.op_Implicit(TraceLayerMask), (QueryTriggerInteraction)1))
			{
				return false;
			}
			if (Mathf.Abs(((RaycastHit)(ref val)).point.y - groundHeight) > MaxGroundVariance || ((RaycastHit)(ref val)).normal.y < MinGroundNormal || ((RaycastHit)(ref val)).point.y < waterHeight)
			{
				return false;
			}
		}
		return true;
	}

	public void CleanupQueuedFerries()
	{
		Array.Resize(ref QueuedFerries, QueuePoints.Length);
		for (int i = 0; i < QueuedFerries.Length; i++)
		{
			if (!Object.op_Implicit((Object)(object)QueuedFerries[i]))
			{
				QueuedFerries[i] = null;
			}
		}
		if (!Object.op_Implicit((Object)(object)CurrentFerry))
		{
			CurrentFerry = null;
		}
	}

	public NexusDock()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		WaitTime = 30f;
		TraceHeight = 100f;
		TraceLayerMask = LayerMask.op_Implicit(1503731969);
		EjectionAttempts = 25;
		MinGroundNormal = 0.7f;
		MaxGroundVariance = 0.35f;
		base._002Ector();
	}
}

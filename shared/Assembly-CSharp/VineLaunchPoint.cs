using System;
using System.Collections.Generic;
using Development.Attributes;
using Facepunch;
using Spatial;
using UnityEngine;

[ResetStaticFields]
public class VineLaunchPoint : MonoBehaviour
{
	[Header("References")]
	public GameObjectRef VineMountablePrefab;

	public float MaximumDestinationRange;

	public float MinimumDestinationRange;

	[Header("Arc Settings")]
	public float maxDistanceHeight = -10f;

	public float minDistanceHeight = -4f;

	public int resolution = 30;

	public bool drawArc = true;

	public float angle;

	public float VineSpawnOffset = 0.1f;

	public bool useLevelDirection = true;

	public Transform[] VineArrivalPoints;

	public VineSwingingTree ParentTree;

	public static Grid<VineLaunchPoint> pointGrid = new Grid<VineLaunchPoint>(32, 8096f);

	private bool hasDied;

	private VineMountable spawnedVine
	{
		get
		{
			return ParentTree.GetSpawnedVine(this);
		}
		set
		{
			ParentTree.SetSpawnedVine(this, value);
		}
	}

	public int Index()
	{
		if ((Object)(object)ParentTree != (Object)null)
		{
			for (int i = 0; i < ParentTree.LaunchPoints.Length; i++)
			{
				if ((Object)(object)ParentTree.LaunchPoints[i] == (Object)(object)this)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public Vector3 GetSwingPointAtTime(float time, VineLaunchPoint forPoint)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return GetSwingPointAtTime(time, ((Component)forPoint).transform.position);
	}

	public Vector3 GetSwingPointAtTime(float time, Vector3 forPoint)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		Vector3 val = forPoint;
		Vector3 val2 = val - position;
		Vector3 normalized = ((Vector3)(ref val2)).normalized;
		position += normalized * VineSpawnOffset;
		val += normalized * (0f - VineSpawnOffset);
		float num = Mathx.RemapValClamped(Vector3.Distance(position, val), MinimumDestinationRange, MaximumDestinationRange, 0f, 1f);
		Vector3 point = VineUtils.SampleParabola(position, val, Mathf.Lerp(minDistanceHeight, maxDistanceHeight, num), time, useLevelDirection);
		Vector3 pivot = (position + val) / 2f;
		val2 = position - val;
		return VineUtils.RotateAroundWorldAxis(point, pivot, ((Vector3)(ref val2)).normalized, angle);
	}

	public void ServerInit()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		pointGrid.Add(this, position.x, position.z);
		hasDied = false;
	}

	public void DoServerDestroy()
	{
		if (!hasDied)
		{
			hasDied = true;
			pointGrid.Remove(this);
			VineMountable.NotifyVinesLaunchSiteRemoved(this);
		}
	}

	public void SpawnVineIfPossible(VineSwingingTree fromTree)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		hasDied = false;
		VineMountable vineMountable = spawnedVine;
		PooledList<VineLaunchPoint> val = Pool.Get<PooledList<VineLaunchPoint>>();
		try
		{
			if ((Object)(object)vineMountable != (Object)null)
			{
				if (GetReceivePoints((List<VineLaunchPoint>)(object)val))
				{
					vineMountable.Initialise(this, (List<VineLaunchPoint>)(object)val, vineMountable.WorldSpaceAnchorPoint);
					vineMountable.SendNetworkUpdate();
				}
				return;
			}
			((List<VineLaunchPoint>)(object)val).Clear();
			GetReceivePoints((List<VineLaunchPoint>)(object)val);
			if (((List<VineLaunchPoint>)(object)val).Count <= 0)
			{
				return;
			}
			Vector3 val2 = ((Component)this).transform.TransformPoint(Vector3.forward * VineSpawnOffset);
			PooledList<VineMountable> val3 = Pool.Get<PooledList<VineMountable>>();
			try
			{
				GamePhysics.OverlapSphere<VineMountable>(val2, 5f, (List<VineMountable>)(object)val3, 134217728, (QueryTriggerInteraction)2);
				foreach (VineMountable item in (List<VineMountable>)(object)val3)
				{
					if (!item.HasFlag(BaseEntity.Flags.Reserved1))
					{
						return;
					}
				}
				VineMountable vineMountable2 = GameManager.server.CreateEntity(VineMountablePrefab.resourcePath, val2, Quaternion.identity) as VineMountable;
				if (FindVacantArrivalPoint(vineMountable2, out var worldPos))
				{
					((Component)vineMountable2).transform.position = worldPos;
				}
				spawnedVine = vineMountable2;
				Vector3 vineSpawnPos = fromTree.GetVineSpawnPos((List<VineLaunchPoint>)(object)val);
				vineMountable2.Initialise(this, (List<VineLaunchPoint>)(object)val, vineSpawnPos);
				vineMountable2.Spawn();
				vineMountable2.SendNetworkUpdate();
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void OnVineKilled()
	{
		spawnedVine = null;
	}

	private bool GetReceivePoints(List<VineLaunchPoint> points)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		Vector3 forward = ((Component)this).transform.forward;
		bool result = false;
		PooledList<VineLaunchPoint> val = Pool.Get<PooledList<VineLaunchPoint>>();
		try
		{
			if (!Application.isPlaying)
			{
				((List<VineLaunchPoint>)(object)val).AddRange((IEnumerable<VineLaunchPoint>)Object.FindObjectsByType<VineLaunchPoint>((FindObjectsInactive)0, (FindObjectsSortMode)0));
			}
			else
			{
				pointGrid.Query(position.x, position.z, MaximumDestinationRange, (List<VineLaunchPoint>)(object)val);
			}
			foreach (VineLaunchPoint item in (List<VineLaunchPoint>)(object)val)
			{
				if ((Object)(object)item == (Object)(object)this || points.Contains(item))
				{
					continue;
				}
				Vector3 position2 = ((Component)item).transform.position;
				float num = Vector3.Distance(position, Vector3Ex.WithY(position2, position.y));
				if (num > MaximumDestinationRange || num < MinimumDestinationRange)
				{
					continue;
				}
				Vector3 val2 = Vector3Ex.WithY(position2, position.y) - position;
				if (!(Vector3.Angle(forward, ((Vector3)(ref val2)).normalized) > 45f) && !(Vector3.Angle(forward, -((Component)item).transform.forward) > 90f))
				{
					if (!GamePhysics.LineOfSightRadius(position, position2, 1084293377, 0.25f, ParentTree))
					{
						return false;
					}
					Vector3 swingPointAtTime = GetSwingPointAtTime(0.5f, position2);
					if (!GamePhysics.LineOfSightRadius(position, swingPointAtTime, 1084293377, 0.25f, ParentTree))
					{
						return false;
					}
					if (!GamePhysics.LineOfSightRadius(position2, swingPointAtTime, 1084293377, 0.25f, ParentTree))
					{
						return false;
					}
					points.Add(item);
					result = true;
				}
			}
			return result;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool FindVacantArrivalPoint(VineMountable forMountable, out Vector3 worldPos)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		if (!forMountable.HasFlag(BaseEntity.Flags.Reserved1))
		{
			worldPos = ((Component)this).transform.TransformPoint(Vector3.forward * VineSpawnOffset);
			return true;
		}
		worldPos = Vector3.zero;
		PooledList<VineMountable> val = Pool.Get<PooledList<VineMountable>>();
		try
		{
			Vis.Entities(((Component)this).transform.position, 2f, (List<VineMountable>)(object)val, 134217728, (QueryTriggerInteraction)2);
			float num = float.MaxValue;
			Transform val2 = null;
			Transform[] vineArrivalPoints = VineArrivalPoints;
			foreach (Transform val3 in vineArrivalPoints)
			{
				Vector3 position = val3.position;
				bool flag = true;
				foreach (VineMountable item in (List<VineMountable>)(object)val)
				{
					if (!item.isClient && !((Object)(object)item == (Object)(object)forMountable) && Vector3.Distance(position, Vector3Ex.WithY(((Component)item).transform.position, position.y)) < 0.1f)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					float num2 = Vector3.Distance(Vector3Ex.WithY(((Component)forMountable).transform.position, position.y), position);
					if (num2 < num)
					{
						num = num2;
						val2 = val3;
					}
				}
			}
			bool num3 = (Object)(object)val2 != (Object)null;
			if (num3)
			{
				worldPos = val2.position;
			}
			return num3;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}

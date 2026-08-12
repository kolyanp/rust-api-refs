using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using UnityEngine;

public class CrashSpotSearch
{
	public enum Status
	{
		InProgress,
		Found,
		Exhausted
	}

	private enum Blocker
	{
		None,
		OutOfBounds,
		Topology,
		Water,
		Uneven,
		Obstruction,
		SafeZone,
		PhysicalUneven
	}

	private readonly Vector3 center;

	private readonly float radius;

	private readonly float clearanceRadius;

	private readonly Vector3 preferredSpot;

	private readonly bool hasPreferredSpot;

	private readonly bool logResult;

	private readonly int n;

	private readonly int[] order;

	private readonly List<Vector3> tcPositions;

	private bool initialized;

	private int nextSample;

	private int bounds;

	private int topo;

	private int water;

	private int uneven;

	private int obstruction;

	private int safezone;

	private int physicalUneven;

	[CompilerGenerated]
	private Vector3 _003CResult_003Ek__BackingField;

	private const float GoldenAngle = 2.3999631f;

	private const int FootprintSampleCount = 13;

	private const int PhysicalFootprintMask = 8454145;

	private const float FootprintRayHeight = 50f;

	private const int TopologyRejectMask = 6374530;

	private static readonly int ClearanceMask = 689963264;

	private const float SeaLevelClearance = 0.5f;

	public int TcsInArea { get; private set; }

	public int SamplesTested => nextSample;

	public Vector3 Result
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CResult_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CResult_003Ek__BackingField = value;
		}
	}

	private bool ShouldLog
	{
		get
		{
			if (logResult)
			{
				return Satellite.debug;
			}
			return false;
		}
	}

	public CrashSpotSearch(Vector3 center, float radius, float clearanceRadius, Vector3 preferredSpot = default(Vector3), bool hasPreferredSpot = false, bool logResult = true)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		tcPositions = new List<Vector3>();
		TcsInArea = -1;
		base._002Ector();
		this.center = center;
		this.radius = radius;
		this.clearanceRadius = clearanceRadius;
		this.preferredSpot = preferredSpot;
		this.hasPreferredSpot = hasPreferredSpot;
		this.logResult = logResult;
		n = ComputeSampleCount(radius, clearanceRadius);
		order = new int[n];
		Result = center;
	}

	private static int ComputeSampleCount(float radius, float clearanceRadius)
	{
		float num = ((clearanceRadius > 0f) ? (radius / clearanceRadius) : 1f);
		int num2 = Mathf.CeilToInt(Satellite.targeting_coverage_factor * num * num);
		int num3 = Mathf.Max(1, Satellite.targeting_min_samples);
		return Mathf.Clamp(num2, num3, 256);
	}

	public bool TryReusePreferred(out Vector3 result)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		result = preferredSpot;
		if (!Satellite.reuse_last_crash_spot || !hasPreferredSpot)
		{
			return false;
		}
		float num = preferredSpot.x - center.x;
		float num2 = preferredSpot.z - center.z;
		int num3;
		if (num * num + num2 * num2 <= radius * radius)
		{
			num3 = ((EvaluateCrashSpot(preferredSpot, clearanceRadius) == Blocker.None) ? 1 : 0);
			if (num3 != 0)
			{
				Result = preferredSpot;
			}
		}
		else
		{
			num3 = 0;
		}
		return (byte)num3 != 0;
	}

	public Status Step()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		if (!initialized)
		{
			Initialize();
		}
		if (nextSample >= n)
		{
			return Status.Exhausted;
		}
		int index = order[nextSample];
		nextSample++;
		Vector2 val = SampleDiscPointXZ(center, radius, index, n);
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(val.x, 0f, val.y);
		val2.y = TerrainMeta.HeightMap.GetHeight(val2);
		switch (EvaluateCrashSpot(val2, clearanceRadius))
		{
		case Blocker.None:
			Result = val2;
			if (ShouldLog)
			{
				Debug.Log((object)($"[Satellite] Crash target {val2} found after testing {nextSample}/{n} samples within " + string.Format("{0:F0}m of {1} (clearance {2:F0}m). Rejected so far: {3}", new object[4]
				{
					radius,
					center,
					clearanceRadius,
					BlockerTally()
				})));
			}
			return Status.Found;
		case Blocker.OutOfBounds:
			bounds++;
			break;
		case Blocker.Topology:
			topo++;
			break;
		case Blocker.Water:
			water++;
			break;
		case Blocker.Uneven:
			uneven++;
			break;
		case Blocker.Obstruction:
			obstruction++;
			break;
		case Blocker.SafeZone:
			safezone++;
			break;
		case Blocker.PhysicalUneven:
			physicalUneven++;
			break;
		}
		if (nextSample >= n)
		{
			Result = center;
			if (ShouldLog)
			{
				Debug.LogWarning((object)($"[Satellite] No clear crash target within {radius:F0}m of {center} after all {n} samples " + $"(clearance {clearanceRadius:F0}m). Blockers: {BlockerTally()}"));
			}
			return Status.Exhausted;
		}
		return Status.InProgress;
	}

	private string BlockerTally()
	{
		return string.Format("topology={0}, water={1}, uneven={2}, obstruction={3}, ", new object[4] { topo, water, uneven, obstruction }) + $"safezone={safezone}, bounds={bounds}, physicalUneven={physicalUneven}.";
	}

	private void Initialize()
	{
		initialized = true;
		for (int i = 0; i < n; i++)
		{
			order[i] = i;
		}
		for (int num = n - 1; num > 0; num--)
		{
			int num2 = Random.Range(0, num + 1);
			ref int reference = ref order[num];
			ref int reference2 = ref order[num2];
			int num3 = order[num2];
			int num4 = order[num];
			reference = num3;
			reference2 = num4;
		}
		if (Satellite.obstruction_tc_reorder)
		{
			PrefetchObstructionTcs();
			TcsInArea = tcPositions.Count;
			if (tcPositions.Count > 0)
			{
				ReorderSamplesByTc();
			}
		}
	}

	private void PrefetchObstructionTcs()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		tcPositions.Clear();
		PooledList<BuildingPrivlidge> val = Pool.Get<PooledList<BuildingPrivlidge>>();
		try
		{
			BaseEntity.Query.Server.GetInSphere(center, radius + clearanceRadius, (List<BuildingPrivlidge>)(object)val);
			for (int i = 0; i < ((List<BuildingPrivlidge>)(object)val).Count; i++)
			{
				BuildingPrivlidge buildingPrivlidge = ((List<BuildingPrivlidge>)(object)val)[i];
				if (IsRealToolCupboard(buildingPrivlidge))
				{
					tcPositions.Add(((Component)buildingPrivlidge).transform.position);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void ReorderSamplesByTc()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		Span<int> span = stackalloc int[n];
		int num = 0;
		int num2 = n;
		float sqrRadius = clearanceRadius * clearanceRadius;
		for (int i = 0; i < n; i++)
		{
			int num3 = order[i];
			Vector2 val = SampleDiscPointXZ(center, radius, num3, n);
			if (SampleNearTc(val.x, val.y, sqrRadius))
			{
				span[--num2] = num3;
			}
			else
			{
				span[num++] = num3;
			}
		}
		span.CopyTo(order);
	}

	private bool SampleNearTc(float x, float z, float sqrRadius)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < tcPositions.Count; i++)
		{
			float num = tcPositions[i].x - x;
			float num2 = tcPositions[i].z - z;
			if (num * num + num2 * num2 < sqrRadius)
			{
				return true;
			}
		}
		return false;
	}

	private static Vector2 SampleDiscPointXZ(Vector3 center, float radius, int index, int n)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		float num = radius * Mathf.Sqrt((float)index / (float)n);
		float num2 = (float)index * 2.3999631f;
		return new Vector2(center.x + Mathf.Cos(num2) * num, center.z + Mathf.Sin(num2) * num);
	}

	public static bool IsDryLandCandidate(Vector3 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!IsOutOfBounds(pos) && !IsBlockedByTopology(pos))
		{
			return !IsInWater(pos);
		}
		return false;
	}

	public static bool IsSpotOk(Vector3 pos, float clearanceRadius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return EvaluateCrashSpot(pos, clearanceRadius) == Blocker.None;
	}

	private static Blocker EvaluateCrashSpot(Vector3 pos, float clearanceRadius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (IsOutOfBounds(pos))
		{
			return Blocker.OutOfBounds;
		}
		if (IsBlockedByTopology(pos))
		{
			return Blocker.Topology;
		}
		if (IsInWater(pos))
		{
			return Blocker.Water;
		}
		if (IsTerrainTooUneven(pos))
		{
			return Blocker.Uneven;
		}
		if (IsInSafeZone(pos, clearanceRadius))
		{
			return Blocker.SafeZone;
		}
		if (IsObstructed(pos, clearanceRadius))
		{
			return Blocker.Obstruction;
		}
		if (Satellite.site_check_physical_geometry && IsPhysicalSurfaceTooUneven(pos))
		{
			return Blocker.PhysicalUneven;
		}
		return Blocker.None;
	}

	private static void GetFootprintOffsetsXZ(float radius, Span<Vector2> offsets)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		offsets[0] = Vector2.zero;
		int num = 1;
		for (int i = 0; i < 4; i++)
		{
			float num2 = ((float)i * 90f + 45f) * (MathF.PI / 180f);
			offsets[num++] = new Vector2(Mathf.Cos(num2), Mathf.Sin(num2)) * (radius * 0.5f);
		}
		for (int j = 0; j < 8; j++)
		{
			float num3 = (float)j * 45f * (MathF.PI / 180f);
			offsets[num++] = new Vector2(Mathf.Cos(num3), Mathf.Sin(num3)) * radius;
		}
	}

	private static void FitPlane(ReadOnlySpan<Vector3> samples, ReadOnlySpan<Vector3> sampleNormals, out Vector3 centroid, out Vector3 normal, out float maxDeviation)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		Vector3 val2 = Vector3.zero;
		for (int i = 0; i < samples.Length; i++)
		{
			val += samples[i];
			val2 += sampleNormals[i];
		}
		centroid = val / (float)samples.Length;
		normal = ((((Vector3)(ref val2)).sqrMagnitude > 0.0001f) ? ((Vector3)(ref val2)).normalized : Vector3.up);
		maxDeviation = 0f;
		for (int j = 0; j < samples.Length; j++)
		{
			maxDeviation = Mathf.Max(maxDeviation, Mathf.Abs(Vector3.Dot(samples[j] - centroid, normal)));
		}
	}

	public unsafe static bool SampleFootprintPlane(Vector3 center, float radius, out Vector3 centroid, out Vector3 normal, out float maxDeviation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		centroid = center;
		normal = Vector3.up;
		maxDeviation = 0f;
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		if ((Object)(object)heightMap == (Object)null)
		{
			return false;
		}
		Span<Vector2> offsets = new Span<Vector2>(stackalloc Vector2[13], 13);
		GetFootprintOffsetsXZ(radius, offsets);
		Span<Vector3> span = new Span<Vector3>(stackalloc Vector3[13], 13);
		Span<Vector3> span2 = new Span<Vector3>(stackalloc Vector3[13], 13);
		for (int i = 0; i < 13; i++)
		{
			Vector3 val = center + new Vector3(offsets[i].x, 0f, offsets[i].y);
			val.y = heightMap.GetHeight(val);
			span[i] = val;
			span2[i] = heightMap.GetNormal(val);
		}
		FitPlane(span, span2, out centroid, out normal, out maxDeviation);
		return true;
	}

	private static bool ExceedsSiteShapeLimits(Vector3 normal, float deviation)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.Angle(normal, Vector3.up) > Satellite.site_max_slope)
		{
			return true;
		}
		if (deviation > Satellite.site_max_unevenness)
		{
			return true;
		}
		return false;
	}

	private static bool IsTerrainTooUneven(Vector3 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!SampleFootprintPlane(pos, Satellite.site_footprint_radius, out var _, out var normal, out var maxDeviation))
		{
			return false;
		}
		return ExceedsSiteShapeLimits(normal, maxDeviation);
	}

	private unsafe static bool IsPhysicalSurfaceTooUneven(Vector3 pos)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		if ((Object)(object)heightMap == (Object)null)
		{
			return false;
		}
		float site_footprint_radius = Satellite.site_footprint_radius;
		Span<Vector2> offsets = new Span<Vector2>(stackalloc Vector2[13], 13);
		GetFootprintOffsetsXZ(site_footprint_radius, offsets);
		Span<Vector3> span = new Span<Vector3>(stackalloc Vector3[13], 13);
		Span<Vector3> span2 = new Span<Vector3>(stackalloc Vector3[13], 13);
		RaycastHit val2 = default(RaycastHit);
		for (int i = 0; i < 13; i++)
		{
			Vector3 val = pos + new Vector3(offsets[i].x, 0f, offsets[i].y);
			float height = heightMap.GetHeight(val);
			if (Physics.Raycast(new Vector3(val.x, height + 50f, val.z), Vector3.down, ref val2, 100f, 8454145, (QueryTriggerInteraction)1))
			{
				span[i] = ((RaycastHit)(ref val2)).point;
				span2[i] = ((RaycastHit)(ref val2)).normal;
			}
			else
			{
				val.y = height;
				span[i] = val;
				span2[i] = heightMap.GetNormal(val);
			}
		}
		FitPlane(span, span2, out var _, out var normal, out var maxDeviation);
		return ExceedsSiteShapeLimits(normal, maxDeviation);
	}

	public static bool IsOutOfBounds(Vector3 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		float num = TerrainMeta.Size.x * 0.5f;
		if (!(Mathf.Abs(pos.x) > num))
		{
			return Mathf.Abs(pos.z) > num;
		}
		return true;
	}

	private static bool IsBlockedByTopology(Vector3 pos)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TerrainMeta.TopologyMap != (Object)null)
		{
			return (TerrainMeta.TopologyMap.GetTopology(pos) & 0x614482) != 0;
		}
		return false;
	}

	public static bool IsInWater(Vector3 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		bool flag = WaterLevel.Test(pos, waves: false, volumes: true);
		if (!flag && (Object)(object)TerrainMeta.HeightMap != (Object)null)
		{
			flag = TerrainMeta.HeightMap.GetHeight(pos) < WaterSystem.OceanLevel - 0.5f;
		}
		return flag;
	}

	private static bool IsInSafeZone(Vector3 pos, float clearanceRadius)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		List<TriggerSafeZone> allSafeZones = TriggerSafeZone.allSafeZones;
		for (int i = 0; i < allSafeZones.Count; i++)
		{
			TriggerSafeZone triggerSafeZone = allSafeZones[i];
			if (!((Object)(object)triggerSafeZone == (Object)null) && !((Object)(object)triggerSafeZone.triggerCollider == (Object)null))
			{
				Vector3 val = triggerSafeZone.triggerCollider.ClosestPoint(pos) - pos;
				if (((Vector3)(ref val)).sqrMagnitude < clearanceRadius * clearanceRadius && triggerSafeZone.PassesHeightChecks(pos))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private static bool IsObstructed(Vector3 pos, float clearanceRadius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Physics.CheckSphere(pos, clearanceRadius, ClearanceMask, (QueryTriggerInteraction)2);
	}

	private static bool IsRealToolCupboard(BuildingPrivlidge tc)
	{
		if ((Object)(object)tc != (Object)null && tc.isServer)
		{
			return !tc.IsInvisibleAuth;
		}
		return false;
	}

	public static int CountToolCupboards(Vector3 center, float radius)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		PooledList<BuildingPrivlidge> val = Pool.Get<PooledList<BuildingPrivlidge>>();
		try
		{
			BaseEntity.Query.Server.GetInSphere(center, radius, (List<BuildingPrivlidge>)(object)val);
			int num = 0;
			for (int i = 0; i < ((List<BuildingPrivlidge>)(object)val).Count; i++)
			{
				if (IsRealToolCupboard(((List<BuildingPrivlidge>)(object)val)[i]))
				{
					num++;
				}
			}
			return num;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}

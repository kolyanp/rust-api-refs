using System;
using UnityEngine;

public class TerrainFootprint : PrefabAttribute
{
	private delegate void BandTexel(int x, int z, float lift, bool inside);

	[HideInInspector]
	public Vector3[] Ring = Array.Empty<Vector3>();

	[HideInInspector]
	public Vector3 Center = Vector3.zero;

	[HideInInspector]
	public int[] RunStarts = Array.Empty<int>();

	[HideInInspector]
	public bool[] RunClosed = Array.Empty<bool>();

	[Tooltip("Where the fill stops, relative to the ring: 0 lands the ground on the line, positive buries the base. Metres, never scaled by the prefab or by tilt.")]
	[Header("Seating")]
	public float FillOffset;

	[Tooltip("Safety ceiling on the raise, in metres. A backstop, not a tuning dial: set it below the deepest gutter you want bridged and the gutter stops short, leaving a step.")]
	[Header("Fill")]
	public float MaxFill = 4f;

	[Tooltip("Reject the placement when the ground has to rise further than this to meet the rock, in metres. 0 (the default) never rejects, so the footprint only ever fills.")]
	public float RejectAboveGap;

	[Tooltip("How far inward from the ring the fill reaches when FillInterior is off, easing to nothing rather than stopping dead.")]
	public float RimWidth = 2f;

	[Tooltip("Falloff distance outside the ring, so filled ground meets untouched terrain without a lip.")]
	public float Feather = 3f;

	[Range(0f, 1f)]
	[Tooltip("Strength of the raise. Below 1 the fill only partially closes the gap.")]
	public float Opacity = 1f;

	[Tooltip("Fill the whole interior of a closed ring, not just the band at its edge. Off leaves a gutter's banks raised with the trench between them, so turn it off only for arches and overhangs.")]
	public bool FillInterior = true;

	[Tooltip("Above this tilt the footprint does nothing: a tilted rock swings out past the captured ring, so the silhouette stops describing where it meets ground. Costs coverage, not correctness.")]
	[Header("Tilt")]
	public float MaxTiltDegrees = 35f;

	[NonSerialized]
	private Vector3[] rootRing;

	[NonSerialized]
	private Vector3 rootCenter;

	[NonSerialized]
	private bool rootRingValid;

	public bool HasRing
	{
		get
		{
			if (Ring != null)
			{
				return Ring.Length >= 2;
			}
			return false;
		}
	}

	public int RunCount
	{
		get
		{
			if (RunStarts == null || RunStarts.Length == 0)
			{
				return 1;
			}
			return RunStarts.Length;
		}
	}

	public float GetTilt(Quaternion rotation)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Angle(Vector3.up, rotation * Vector3.up);
	}

	public bool IsActive(Quaternion rotation)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (HasRing)
		{
			return GetTilt(rotation) <= MaxTiltDegrees;
		}
		return false;
	}

	public void InvalidateRootRing()
	{
		rootRingValid = false;
	}

	public void GetRun(int run, out int start, out int end, out bool closed)
	{
		int num = ((Ring != null) ? Ring.Length : 0);
		if (RunStarts == null || RunStarts.Length == 0)
		{
			start = 0;
			end = num;
			closed = true;
		}
		else
		{
			start = Mathf.Clamp(RunStarts[run], 0, num);
			end = ((run + 1 < RunStarts.Length) ? Mathf.Clamp(RunStarts[run + 1], start, num) : num);
			closed = RunClosed != null && run < RunClosed.Length && RunClosed[run];
		}
	}

	private void EnsureRootRing()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		if (!rootRingValid || rootRing == null || rootRing.Length != Ring.Length)
		{
			if (rootRing == null || rootRing.Length != Ring.Length)
			{
				rootRing = new Vector3[Ring.Length];
			}
			for (int i = 0; i < Ring.Length; i++)
			{
				rootRing[i] = worldPosition + worldRotation * Ring[i];
			}
			rootCenter = worldPosition + worldRotation * Center;
			rootRingValid = true;
		}
	}

	public float MeasureGap(Vector3 pos, Quaternion rot, Vector3 scale)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		float worst = 0f;
		ForEachBandTexel(pos, rot, scale, (int x, int z, float lift, bool inside) =>
		{
			if (inside && lift > worst)
			{
				worst = lift;
			}
		});
		return worst;
	}

	public void Fill(Vector3 pos, Quaternion rot, Vector3 scale)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (Opacity <= 0f)
		{
			return;
		}
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		if (!Object.op_Implicit((Object)(object)heightMap))
		{
			return;
		}
		int minX = int.MaxValue;
		int maxX = int.MinValue;
		int minZ = int.MaxValue;
		int maxZ = int.MinValue;
		ForEachBandTexel(pos, rot, scale, (int x, int z, float lift, bool inside) =>
		{
			if (!(lift <= 0f))
			{
				if (x < minX)
				{
					minX = x;
				}
				if (x > maxX)
				{
					maxX = x;
				}
				if (z < minZ)
				{
					minZ = z;
				}
				if (z > maxZ)
				{
					maxZ = z;
				}
			}
		});
		if (minX > maxX)
		{
			return;
		}
		int width = maxX - minX + 1;
		float[] lifts = new float[width * (maxZ - minZ + 1)];
		ForEachBandTexel(pos, rot, scale, (int x, int z, float lift, bool inside) =>
		{
			if (!(lift <= 0f))
			{
				int num4 = (z - minZ) * width + (x - minX);
				if (lift > lifts[num4])
				{
					lifts[num4] = lift;
				}
			}
		});
		for (int num = minZ; num <= maxZ; num++)
		{
			for (int num2 = minX; num2 <= maxX; num2++)
			{
				float num3 = lifts[(num - minZ) * width + (num2 - minX)];
				if (!(num3 <= 0f))
				{
					float y = heightMap.GetHeight(num2, num) + Mathf.Min(num3 * Opacity, MaxFill);
					heightMap.RaiseHeight(num2, num, TerrainMeta.NormalizeY(y), 1f);
				}
			}
		}
	}

	private void ForEachBandTexel(Vector3 pos, Quaternion rot, Vector3 scale, BandTexel action)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		if (!IsActive(rot))
		{
			return;
		}
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		if (!Object.op_Implicit((Object)(object)heightMap))
		{
			return;
		}
		EnsureRootRing();
		float fillOffset = FillOffset;
		float rim = Mathf.Max(RimWidth, 0.01f);
		float feather = Mathf.Max(Feather, 0.01f);
		Vector3 val = rot * Vector3.up;
		if (Mathf.Abs(val.y) < 0.0001f)
		{
			return;
		}
		Vector3 center = pos + rot * Vector3.Scale(rootCenter, scale);
		Vector3[] array = new Vector3[Ring.Length];
		for (int i = 0; i < Ring.Length; i++)
		{
			array[i] = pos + rot * Vector3.Scale(rootRing[i], scale);
		}
		for (int j = 0; j < RunCount; j++)
		{
			GetRun(j, out var start, out var end, out var closed);
			int num = end - start;
			if (num < 2)
			{
				continue;
			}
			bool flag = (FillInterior & closed) && num >= 3;
			int num2 = (closed ? num : (num - 1));
			for (int k = 0; k < num2; k++)
			{
				Vector3 a = array[start + k];
				Vector3 b = array[start + (k + 1) % num];
				SweepSegment(heightMap, a, b, center, val, fillOffset, rim, feather, flag, action);
			}
			if (flag)
			{
				for (int l = 0; l < num; l++)
				{
					Vector3 a2 = array[start + l];
					Vector3 b2 = array[start + (l + 1) % num];
					FanTriangle(heightMap, center, a2, b2, val, fillOffset, action);
				}
			}
		}
	}

	private static float PlaneHeightAt(Vector3 normal, Vector3 center, float x, float z)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		return center.y - (normal.x * (x - center.x) + normal.z * (z - center.z)) / normal.y;
	}

	private static void SweepSegment(TerrainHeightMap heightmap, Vector3 a, Vector3 b, Vector3 center, Vector3 normal, float offset, float rim, float feather, bool interiorFilled, BandTexel action)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		Vector3 inwardA = Inward(a, center);
		Vector3 inwardB = Inward(b, center);
		Vector3 v = a - inwardA * feather;
		Vector3 v2 = b - inwardB * feather;
		Vector3 v3 = a + inwardA * rim;
		Vector3 v4 = b + inwardB * rim;
		Vector2 ab = new Vector2(b.x - a.x, b.z - a.z);
		float abSqr = ab.sqrMagnitude;
		ForEachQuad(heightmap, v, v2, v3, v4, (int x, int z) =>
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			float num = TerrainMeta.DenormalizeX(heightmap.Coordinate(x));
			float num2 = TerrainMeta.DenormalizeZ(heightmap.Coordinate(z));
			float num3 = ((abSqr > 1E-06f) ? Mathf.Clamp01(Vector2.Dot(new Vector2(num - a.x, num2 - a.z), ab) / abSqr) : 0f);
			Vector3 val = Vector3.Lerp(a, b, num3);
			Vector2 val2 = new Vector2(num - val.x, num2 - val.z);
			float magnitude = val2.magnitude;
			Vector3 val3 = Vector3.Lerp(inwardA, inwardB, num3);
			bool flag = Vector2.Dot(val2, new Vector2(val3.x, val3.z)) >= 0f;
			float num4 = ((!flag) ? Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(1f - magnitude / feather)) : (interiorFilled ? 1f : Mathf.SmoothStep(1f, 0f, Mathf.Clamp01(magnitude / rim))));
			if (!(num4 <= 0f))
			{
				float num5 = (PlaneHeightAt(normal, center, num, num2) + offset - heightmap.GetHeight(x, z)) * num4;
				if (!(num5 <= 0f))
				{
					action(x, z, num5, flag);
				}
			}
		});
	}

	private static void FanTriangle(TerrainHeightMap heightmap, Vector3 center, Vector3 a, Vector3 b, Vector3 normal, float offset, BandTexel action)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		ForEachTriangle(heightmap, center, a, b, (int x, int z) =>
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			float x2 = TerrainMeta.DenormalizeX(heightmap.Coordinate(x));
			float z2 = TerrainMeta.DenormalizeZ(heightmap.Coordinate(z));
			float num = PlaneHeightAt(normal, center, x2, z2) + offset - heightmap.GetHeight(x, z);
			if (!(num <= 0f))
			{
				action(x, z, num, inside: true);
			}
		});
	}

	private static void ForEachQuad(TerrainHeightMap heightmap, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Action<int, int> action)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (SignedArea(v0, v1, v2) < 0f)
		{
			heightmap.ForEach(v0, v2, v1, v3, action);
		}
		else
		{
			heightmap.ForEach(v0, v1, v2, v3, action);
		}
	}

	private static void ForEachTriangle(TerrainHeightMap heightmap, Vector3 v0, Vector3 v1, Vector3 v2, Action<int, int> action)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (SignedArea(v0, v1, v2) < 0f)
		{
			heightmap.ForEach(v0, v2, v1, action);
		}
		else
		{
			heightmap.ForEach(v0, v1, v2, action);
		}
	}

	private static float SignedArea(Vector3 a, Vector3 b, Vector3 c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z);
	}

	private static Vector3 Inward(Vector3 point, Vector3 center)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = center - point;
		val.y = 0f;
		if (!(val.sqrMagnitude > 1E-06f))
		{
			return Vector3.zero;
		}
		return val.normalized;
	}

	protected override Type GetIndexedType()
	{
		return typeof(TerrainFootprint);
	}

	public TerrainFootprint()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
	}
}

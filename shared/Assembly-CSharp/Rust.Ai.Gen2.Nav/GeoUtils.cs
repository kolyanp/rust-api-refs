using System;
using System.Collections;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2.Nav;

public static class GeoUtils
{
	public static void MergeByDistance(List<Vector3> vertices, List<int> triangles, float mergeDistance)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		if (mergeDistance <= 0f)
		{
			return;
		}
		float num = mergeDistance * mergeDistance;
		int count = vertices.Count;
		PooledList<int> val = Pool.Get<PooledList<int>>();
		try
		{
			for (int i = 0; i < count; i++)
			{
				((List<int>)(object)val).Add(i);
			}
			for (int j = 0; j < count; j++)
			{
				if (((List<int>)(object)val)[j] != j)
				{
					continue;
				}
				Vector3 val2 = vertices[j];
				for (int k = j + 1; k < count; k++)
				{
					if (((List<int>)(object)val)[k] == k)
					{
						Vector3 val3 = vertices[k];
						Vector3 val4 = val2 - val3;
						if (((Vector3)(ref val4)).sqrMagnitude <= num)
						{
							((List<int>)(object)val)[k] = j;
						}
					}
				}
			}
			PooledList<Vector3> val5 = Pool.Get<PooledList<Vector3>>();
			try
			{
				PooledList<int> val6 = Pool.Get<PooledList<int>>();
				try
				{
					for (int l = 0; l < count; l++)
					{
						if (((List<int>)(object)val)[l] == l)
						{
							((List<int>)(object)val6).Add(((List<Vector3>)(object)val5).Count);
							((List<Vector3>)(object)val5).Add(vertices[l]);
						}
						else
						{
							((List<int>)(object)val6).Add(((List<int>)(object)val6)[((List<int>)(object)val)[l]]);
						}
					}
					for (int m = 0; m < triangles.Count; m++)
					{
						triangles[m] = ((List<int>)(object)val6)[triangles[m]];
					}
					vertices.Clear();
					vertices.AddRange((IEnumerable<Vector3>)val5);
				}
				finally
				{
					((IDisposable)val6)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val5)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void DeleteTrianglesOutsideWorldAABB(Bounds worldAabb, List<Vector3> vertices, List<int> triangles)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		PooledList<int> val = Pool.Get<PooledList<int>>();
		try
		{
			GetTriangleIndicesNotOverlappingBounds(worldAabb, vertices, triangles, (List<int>)(object)val);
			DeleteTrianglesAndCompact2(vertices, triangles, (List<int>)(object)val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static void GetTriangleIndicesNotOverlappingBounds(Bounds aaBB, List<Vector3> vertices, List<int> triangles, List<int> triangleIndices)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		triangleIndices.Clear();
		int num = triangles.Count / 3;
		for (int i = 0; i < num; i++)
		{
			int index = triangles[i * 3];
			int index2 = triangles[i * 3 + 1];
			int index3 = triangles[i * 3 + 2];
			Vector3 val = vertices[index];
			Vector3 val2 = vertices[index2];
			Vector3 val3 = vertices[index3];
			if (!((Bounds)(ref aaBB)).Contains(val) && !((Bounds)(ref aaBB)).Contains(val2) && !((Bounds)(ref aaBB)).Contains(val3))
			{
				triangleIndices.Add(i);
			}
		}
	}

	private static void DeleteTrianglesAndCompact(List<Vector3> vertices, List<int> triangles, List<int> triangleIndicesToDelete)
	{
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		if (triangles.Count == 0 || triangleIndicesToDelete == null || triangleIndicesToDelete.Count == 0)
		{
			return;
		}
		int num = triangles.Count / 3;
		BitArray bitArray = new BitArray(num);
		triangleIndicesToDelete.Sort();
		int num2 = -1;
		for (int i = 0; i < triangleIndicesToDelete.Count; i++)
		{
			int num3 = triangleIndicesToDelete[i];
			if ((uint)num3 >= (uint)num)
			{
				throw new ArgumentOutOfRangeException("triangleIndicesToDelete", $"Triangle {num3} is out of range 0..{num - 1}");
			}
			if (num3 != num2)
			{
				bitArray[num3] = true;
			}
			num2 = num3;
		}
		int num4 = 0;
		foreach (bool item in bitArray)
		{
			if (item)
			{
				num4++;
			}
		}
		if (num4 == num)
		{
			triangles.Clear();
			vertices.Clear();
			return;
		}
		PooledList<bool> val = Pool.Get<PooledList<bool>>();
		try
		{
			for (int j = 0; j < vertices.Count; j++)
			{
				((List<bool>)(object)val).Add(false);
			}
			int num5 = 0;
			for (int k = 0; k < num; k++)
			{
				if (!bitArray[k])
				{
					int num6 = k * 3;
					int num7 = triangles[num6];
					int num8 = triangles[num6 + 1];
					int num9 = triangles[num6 + 2];
					if ((uint)num7 >= (uint)vertices.Count || (uint)num8 >= (uint)vertices.Count || (uint)num9 >= (uint)vertices.Count)
					{
						throw new IndexOutOfRangeException($"Triangle {k} has an index out of range for vertices.Count={vertices.Count}");
					}
					((List<bool>)(object)val)[num7] = true;
					((List<bool>)(object)val)[num8] = true;
					((List<bool>)(object)val)[num9] = true;
					int num10 = num5 * 3;
					if (num10 != num6)
					{
						triangles[num10] = num7;
						triangles[num10 + 1] = num8;
						triangles[num10 + 2] = num9;
					}
					num5++;
				}
			}
			int num11 = num5 * 3;
			if (num11 < triangles.Count)
			{
				triangles.RemoveRange(num11, triangles.Count - num11);
			}
			if (triangles.Count == 0)
			{
				vertices.Clear();
				return;
			}
			PooledList<int> val2 = Pool.Get<PooledList<int>>();
			try
			{
				for (int l = ((List<int>)(object)val2).Count; l < vertices.Count; l++)
				{
					((List<int>)(object)val2).Add(-1);
				}
				PooledList<Vector3> val3 = Pool.Get<PooledList<Vector3>>();
				try
				{
					int num12 = 0;
					for (int m = 0; m < vertices.Count; m++)
					{
						if (((List<bool>)(object)val)[m])
						{
							((List<int>)(object)val2)[m] = num12++;
							((List<Vector3>)(object)val3).Add(vertices[m]);
						}
					}
					for (int n = 0; n < triangles.Count; n++)
					{
						int index = triangles[n];
						int num13 = ((List<int>)(object)val2)[index];
						if (num13 < 0)
						{
							throw new InvalidOperationException("Found a triangle referencing a removed vertex.");
						}
						triangles[n] = num13;
					}
					vertices.Clear();
					vertices.AddRange((IEnumerable<Vector3>)val3);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static void DeleteTrianglesAndCompact2(List<Vector3> vertices, List<int> triangles, List<int> triangleIndicesToDelete)
	{
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		triangleIndicesToDelete.Sort();
		PooledList<int> val = Pool.Get<PooledList<int>>();
		try
		{
			int num = 0;
			int num2 = triangles.Count / 3;
			for (int i = 0; i < num2; i++)
			{
				if (num < triangleIndicesToDelete.Count && triangleIndicesToDelete[num] == i)
				{
					num++;
					continue;
				}
				int num3 = i * 3;
				((List<int>)(object)val).Add(triangles[num3]);
				((List<int>)(object)val).Add(triangles[num3 + 1]);
				((List<int>)(object)val).Add(triangles[num3 + 2]);
			}
			triangles.Clear();
			triangles.AddRange((IEnumerable<int>)val);
			if (triangles.Count == 0)
			{
				vertices.Clear();
				return;
			}
			PooledList<bool> val2 = Pool.Get<PooledList<bool>>();
			try
			{
				for (int j = 0; j < vertices.Count; j++)
				{
					((List<bool>)(object)val2).Add(false);
				}
				for (int k = 0; k < triangles.Count; k++)
				{
					int num4 = triangles[k];
					if ((uint)num4 >= (uint)vertices.Count)
					{
						throw new IndexOutOfRangeException($"Triangle index {num4} out of range for vertices.Count={vertices.Count}");
					}
					((List<bool>)(object)val2)[num4] = true;
				}
				PooledList<int> val3 = Pool.Get<PooledList<int>>();
				try
				{
					for (int l = 0; l < vertices.Count; l++)
					{
						((List<int>)(object)val3).Add(-1);
					}
					PooledList<Vector3> val4 = Pool.Get<PooledList<Vector3>>();
					try
					{
						int m = 0;
						int num5 = 0;
						for (; m < vertices.Count; m++)
						{
							if (((List<bool>)(object)val2)[m])
							{
								((List<int>)(object)val3)[m] = num5++;
								((List<Vector3>)(object)val4).Add(vertices[m]);
							}
						}
						for (int n = 0; n < triangles.Count; n++)
						{
							int index = triangles[n];
							int num6 = ((List<int>)(object)val3)[index];
							if (num6 < 0)
							{
								throw new InvalidOperationException("Found a triangle referencing a removed vertex.");
							}
							triangles[n] = num6;
						}
						vertices.Clear();
						vertices.AddRange((IEnumerable<Vector3>)val4);
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static bool TriangleIntersectsAABB(Bounds box, Vector3 a, Vector3 b, Vector3 c)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		Vector3 center = ((Bounds)(ref box)).center;
		Vector3 extents = ((Bounds)(ref box)).extents;
		Vector3 val = a - center;
		Vector3 val2 = b - center;
		Vector3 val3 = c - center;
		if (!AabbOverlap(val, val2, val3, extents))
		{
			return false;
		}
		Vector3 val4 = val2 - val;
		Vector3 val5 = val3 - val2;
		Vector3 edge = val - val3;
		if (!OverlapOnAxis(val, val2, val3, new Vector3(1f, 0f, 0f), extents))
		{
			return false;
		}
		if (!OverlapOnAxis(val, val2, val3, new Vector3(0f, 1f, 0f), extents))
		{
			return false;
		}
		if (!OverlapOnAxis(val, val2, val3, new Vector3(0f, 0f, 1f), extents))
		{
			return false;
		}
		if (!PlaneBoxOverlap(Vector3.Cross(val4, val5), val, extents))
		{
			return false;
		}
		if (!TestEdgeAxes(val4, extents, val, val2, val3))
		{
			return false;
		}
		if (!TestEdgeAxes(val5, extents, val, val2, val3))
		{
			return false;
		}
		if (!TestEdgeAxes(edge, extents, val, val2, val3))
		{
			return false;
		}
		return true;
	}

	private static bool AabbOverlap(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 ext)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Min(v0.x, Mathf.Min(v1.x, v2.x));
		float num2 = Mathf.Max(v0.x, Mathf.Max(v1.x, v2.x));
		float num3 = Mathf.Min(v0.y, Mathf.Min(v1.y, v2.y));
		float num4 = Mathf.Max(v0.y, Mathf.Max(v1.y, v2.y));
		float num5 = Mathf.Min(v0.z, Mathf.Min(v1.z, v2.z));
		float num6 = Mathf.Max(v0.z, Mathf.Max(v1.z, v2.z));
		if (num2 < 0f - ext.x || num > ext.x)
		{
			return false;
		}
		if (num4 < 0f - ext.y || num3 > ext.y)
		{
			return false;
		}
		if (num6 < 0f - ext.z || num5 > ext.z)
		{
			return false;
		}
		return true;
	}

	private static bool TestEdgeAxes(Vector3 edge, Vector3 ext, Vector3 v0, Vector3 v1, Vector3 v2)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = (Vector3[])(object)new Vector3[3]
		{
			new Vector3(0f, 0f - edge.z, edge.y),
			new Vector3(edge.z, 0f, 0f - edge.x),
			new Vector3(0f - edge.y, edge.x, 0f)
		};
		for (int i = 0; i < 3; i++)
		{
			Vector3 axis = array[i];
			if (!(((Vector3)(ref axis)).sqrMagnitude < 1E-06f) && !OverlapOnAxis(v0, v1, v2, axis, ext))
			{
				return false;
			}
		}
		return true;
	}

	private static bool OverlapOnAxis(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 axis, Vector3 ext)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Dot(v0, axis);
		float num2 = Vector3.Dot(v1, axis);
		float num3 = Vector3.Dot(v2, axis);
		float num4 = Mathf.Min(num, Mathf.Min(num2, num3));
		float num5 = Mathf.Max(num, Mathf.Max(num2, num3));
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(Mathf.Abs(axis.x), Mathf.Abs(axis.y), Mathf.Abs(axis.z));
		float num6 = ext.x * val.x + ext.y * val.y + ext.z * val.z;
		if (!(num4 > num6))
		{
			return !(num5 < 0f - num6);
		}
		return false;
	}

	private static bool PlaneBoxOverlap(Vector3 n, Vector3 v0, Vector3 ext)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(Mathf.Abs(n.x), Mathf.Abs(n.y), Mathf.Abs(n.z));
		float num = ext.x * val.x + ext.y * val.y + ext.z * val.z;
		return Mathf.Abs(Vector3.Dot(n, v0)) <= num;
	}
}

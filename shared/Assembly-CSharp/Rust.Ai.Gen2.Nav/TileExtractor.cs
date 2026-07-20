using System;
using System.Collections.Generic;
using System.Diagnostics;
using ConVar;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2.Nav;

public class TileExtractor : MonoBehaviour
{
	public float CellSize = 50f;

	public float MaxBuildingHeight = 10f;

	private Mesh extractedMesh;

	private static int[] boxVertices = new int[36]
	{
		7, 4, 3, 7, 6, 4, 4, 6, 5, 4,
		5, 0, 4, 5, 1, 4, 1, 0, 5, 6,
		2, 5, 2, 1, 6, 7, 3, 6, 3, 2,
		0, 1, 3, 0, 3, 7
	};

	private void OnDrawGizmos()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (!Application.isPlaying)
		{
			extractedMesh = Extract();
		}
		Gizmos.color = Color.yellow;
		Bounds bounds = GetBounds();
		Gizmos.DrawWireCube(((Bounds)(ref bounds)).center, ((Bounds)(ref bounds)).size);
		if ((Object)(object)extractedMesh != (Object)null && extractedMesh.vertexCount > 0)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireMesh(extractedMesh);
		}
	}

	private Bounds GetBounds()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		float y = TerrainMeta.LowestPoint.y;
		float y2 = TerrainMeta.HighestPoint.y;
		return new Bounds(new Vector3(((Component)this).transform.position.x, (y2 + y) * 0.5f, ((Component)this).transform.position.z), new Vector3(CellSize, y2 - y + MaxBuildingHeight * 2f, CellSize));
	}

	public Mesh Extract()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		FPNativeList<Vector3> fPNativeList = Pool.Get<FPNativeList<Vector3>>();
		FPNativeList<int> fPNativeList2 = Pool.Get<FPNativeList<int>>();
		ExtractMeshDataWithinBounds(GetBounds(), fPNativeList, fPNativeList2);
		Mesh val = new Mesh();
		val.SetVertices<Vector3>(fPNativeList.Array);
		val.SetTriangles(fPNativeList2.Array.ToArray(), 0);
		val.RecalculateNormals();
		Pool.Free<FPNativeList<Vector3>>(ref fPNativeList);
		Pool.Free<FPNativeList<int>>(ref fPNativeList2);
		return val;
	}

	public static bool ExtractMeshDataWithinBounds(Bounds bounds, FPNativeList<Vector3> vertices, FPNativeList<int> triangles)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavigation.ExtractMeshDataWithinBounds"))
		{
			if (RustNav.enableVerboseLogs)
			{
				RustNavigation.Log($"Starting mesh extraction within bounds: {bounds}");
			}
			Stopwatch stopwatch = Stopwatch.StartNew();
			PooledList<Collider> val = Pool.Get<PooledList<Collider>>();
			try
			{
				using (TimeWarning.New("RustNavigation.ExtractMeshDataWithinBounds.OverlapBounds"))
				{
					GamePhysics.OverlapBounds(bounds, (List<Collider>)(object)val, 1090584833, (QueryTriggerInteraction)1);
				}
				stopwatch.Stop();
				if (RustNav.enableVerboseLogs)
				{
					RustNavigation.Log($"Physics query completed in {(double)stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0} ms");
				}
				stopwatch.Restart();
				if (RustNav.enableVerboseLogs)
				{
					RustNavigation.Log($"Found {((List<Collider>)(object)val).Count} colliders within the specified bounds");
				}
				foreach (Collider item in (List<Collider>)(object)val)
				{
					ExtractMesh(vertices, triangles, item, bounds);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			bool result = vertices.Count == 0;
			stopwatch.Stop();
			if (RustNav.enableVerboseLogs)
			{
				RustNavigation.Log($"Mesh extraction completed in {(double)stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0} ms");
			}
			stopwatch.Restart();
			Bounds val2 = new Bounds(TerrainMeta.Center, TerrainMeta.Size);
			if (((Bounds)(ref bounds)).Intersects(val2))
			{
				ExtractTerrainGeometry(Vector3Ex.WithY(((Bounds)(ref bounds)).center - ((Bounds)(ref bounds)).extents, 0f), Mathf.CeilToInt(((Bounds)(ref bounds)).size.x), vertices, triangles);
			}
			stopwatch.Stop();
			if (RustNav.enableVerboseLogs)
			{
				RustNavigation.Log($"Terrain mesh generated in {(double)stopwatch.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0} ms");
			}
			stopwatch.Restart();
			if (RustNav.enableVerboseLogs)
			{
				RustNavigation.Log($"Total extracted triangles: {triangles.Count / 3}");
				RustNavigation.Log($"Total vertices in result: {vertices.Count}");
			}
			return result;
		}
	}

	public static void ExtractMesh(FPNativeList<Vector3> vertices, FPNativeList<int> triangles, Collider collider, Bounds bounds)
	{
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Expected O, but got Unknown
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ExtractMesh"))
		{
			if ((Object)(object)collider == (Object)null)
			{
				if (RustNav.enableVerboseLogs)
				{
					RustNavigation.Log("Skipping object - no Collider component found");
				}
				return;
			}
			if (RustNav.enableVerboseLogs)
			{
				RustNavigation.Log("Processing object: " + ((Object)((Component)collider).gameObject).name);
			}
			Transform transform = ((Component)collider).transform;
			PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
			try
			{
				PooledList<int> val2 = Pool.Get<PooledList<int>>();
				try
				{
					MeshCollider val3 = (MeshCollider)(object)((collider is MeshCollider) ? collider : null);
					if (val3 != null)
					{
						if (!val3.sharedMesh.isReadable)
						{
							Debug.LogWarning((object)("Mesh is not readable: " + ((Object)val3.sharedMesh).name));
							return;
						}
						val3.sharedMesh.GetVertices((List<Vector3>)(object)val);
						for (int i = 0; i < val3.sharedMesh.subMeshCount; i++)
						{
							PooledList<int> val4 = Pool.Get<PooledList<int>>();
							try
							{
								val3.sharedMesh.GetTriangles((List<int>)(object)val4, i);
								((List<int>)(object)val2).AddRange((IEnumerable<int>)val4);
							}
							finally
							{
								((IDisposable)val4)?.Dispose();
							}
						}
					}
					else if (collider is BoxCollider)
					{
						BoxCollider val5 = (BoxCollider)collider;
						CreateBoxMesh((List<Vector3>)(object)val, (List<int>)(object)val2, val5.center, val5.size);
					}
					else if (collider is SphereCollider)
					{
						SphereCollider val6 = (SphereCollider)collider;
						CreateSphereMesh((List<Vector3>)(object)val, (List<int>)(object)val2, val6.center, val6.radius);
					}
					else
					{
						if (!(collider is CapsuleCollider))
						{
							if (RustNav.enableVerboseLogs)
							{
								RustNavigation.Log($"Unsupported collider type: {((object)collider).GetType()}");
							}
							return;
						}
						CapsuleCollider val7 = (CapsuleCollider)collider;
						CreateCapsuleMesh((List<Vector3>)(object)val, (List<int>)(object)val2, val7.center, val7.radius, val7.height, val7.direction);
					}
					int num = 0;
					Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
					for (int j = 0; j < ((List<Vector3>)(object)val).Count; j++)
					{
						((List<Vector3>)(object)val)[j] = ((Matrix4x4)(ref localToWorldMatrix)).MultiplyPoint3x4(((List<Vector3>)(object)val)[j]);
					}
					for (int k = 0; k < ((List<int>)(object)val2).Count / 3; k++)
					{
						int num2 = k * 3;
						triangles.Add(vertices.Count);
						triangles.Add(vertices.Count + 1);
						triangles.Add(vertices.Count + 2);
						vertices.Add(((List<Vector3>)(object)val)[((List<int>)(object)val2)[num2]]);
						vertices.Add(((List<Vector3>)(object)val)[((List<int>)(object)val2)[num2 + 1]]);
						vertices.Add(((List<Vector3>)(object)val)[((List<int>)(object)val2)[num2 + 2]]);
						num++;
					}
					if (RustNav.enableVerboseLogs)
					{
						RustNavigation.Log($"Extracted {num} triangles from mesh");
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
	}

	public static void CreateBoxMesh(List<Vector3> vertices, List<int> triangles, Vector3 center, Vector3 size)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		vertices.Clear();
		triangles.Clear();
		vertices.Add(center + new Vector3(0f - size.x, 0f - size.y, 0f - size.z) * 0.5f);
		vertices.Add(center + new Vector3(size.x, 0f - size.y, 0f - size.z) * 0.5f);
		vertices.Add(center + new Vector3(size.x, 0f - size.y, size.z) * 0.5f);
		vertices.Add(center + new Vector3(0f - size.x, 0f - size.y, size.z) * 0.5f);
		vertices.Add(center + new Vector3(0f - size.x, size.y, 0f - size.z) * 0.5f);
		vertices.Add(center + new Vector3(size.x, size.y, 0f - size.z) * 0.5f);
		vertices.Add(center + new Vector3(size.x, size.y, size.z) * 0.5f);
		vertices.Add(center + new Vector3(0f - size.x, size.y, size.z) * 0.5f);
		for (int i = 0; i < boxVertices.Length; i++)
		{
			triangles.Add(boxVertices[i]);
		}
	}

	private static void CreateSphereMesh(List<Vector3> vertices, List<int> triangles, Vector3 center, float radius, int resolution = 16)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		vertices.Clear();
		triangles.Clear();
		for (int i = 0; i <= resolution; i++)
		{
			float num = (float)i * MathF.PI / (float)resolution;
			float num2 = Mathf.Sin(num);
			float num3 = Mathf.Cos(num);
			for (int j = 0; j <= resolution; j++)
			{
				float num4 = (float)(j * 2) * MathF.PI / (float)resolution;
				float num5 = Mathf.Sin(num4);
				float num6 = Mathf.Cos(num4);
				float num7 = radius * num2 * num6;
				float num8 = radius * num3;
				float num9 = radius * num2 * num5;
				vertices.Add(center + new Vector3(num7, num8, num9));
				if (i < resolution && j < resolution)
				{
					int num10 = i * (resolution + 1) + j;
					int num11 = num10 + resolution + 1;
					triangles.Add(num10 + 1);
					triangles.Add(num11);
					triangles.Add(num10);
					triangles.Add(num11 + 1);
					triangles.Add(num11);
					triangles.Add(num10 + 1);
				}
			}
		}
	}

	private static void CreateCapsuleMesh(List<Vector3> vertices, List<int> triangles, Vector3 center, float radius, float height, int direction, int resolution = 16)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		vertices.Clear();
		triangles.Clear();
		if (height < 2f * radius)
		{
			CreateSphereMesh(vertices, triangles, center, radius);
		}
		int num = resolution / 2;
		float num2 = height - 2f * radius;
		for (int i = 0; i < 2; i++)
		{
			float num3 = (float)((i != 0) ? 1 : (-1)) * (num2 * 0.5f);
			for (int j = 0; j <= num; j++)
			{
				float num4 = MathF.PI / 2f * (float)((i != 0) ? 1 : (-1)) + (float)j * MathF.PI * 0.5f / (float)num * (float)((i == 0) ? 1 : (-1));
				float num5 = Mathf.Sin(num4);
				float num6 = Mathf.Cos(num4);
				for (int k = 0; k <= resolution; k++)
				{
					float num7 = (float)(k * 2) * MathF.PI / (float)resolution;
					float num8 = Mathf.Sin(num7);
					float num9 = Mathf.Cos(num7);
					float num10 = radius * num9 * num6;
					float num11 = radius * num5 + num3;
					float num12 = radius * num8 * num6;
					vertices.Add(center + (Vector3)(direction switch
					{
						1 => new Vector3(num10, num11, num12), 
						0 => new Vector3(num11, num10, num12), 
						_ => new Vector3(num10, num12, num11), 
					}));
					if (j < num && k < resolution)
					{
						int num13 = vertices.Count - 1;
						int num14 = num13 + resolution + 1;
						if (i == 0)
						{
							triangles.Add(num13);
							triangles.Add(num14);
							triangles.Add(num13 + 1);
							triangles.Add(num13 + 1);
							triangles.Add(num14);
							triangles.Add(num14 + 1);
						}
						else
						{
							triangles.Add(num13);
							triangles.Add(num13 + 1);
							triangles.Add(num14);
							triangles.Add(num13 + 1);
							triangles.Add(num14 + 1);
							triangles.Add(num14);
						}
					}
				}
			}
		}
		int count = vertices.Count;
		for (int l = 0; l <= resolution; l++)
		{
			float num15 = (float)(l * 2) * MathF.PI / (float)resolution;
			float num16 = Mathf.Sin(num15);
			float num17 = Mathf.Cos(num15);
			for (int m = 0; m <= 1; m++)
			{
				float num18 = ((float)m - 0.5f) * num2;
				float num19 = radius * num17;
				float num20 = radius * num16;
				vertices.Add(center + (Vector3)(direction switch
				{
					1 => new Vector3(num19, num18, num20), 
					0 => new Vector3(num18, num19, num20), 
					_ => new Vector3(num19, num20, num18), 
				}));
				if (l < resolution && m == 0)
				{
					int num21 = count + l * 2;
					int num22 = num21 + 2;
					triangles.Add(num21);
					triangles.Add(num21 + 1);
					triangles.Add(num22);
					triangles.Add(num21 + 1);
					triangles.Add(num22 + 1);
					triangles.Add(num22);
				}
			}
		}
	}

	public static void ExtractTerrainGeometry(Vector3 topLeftCorner, int tileSize, FPNativeList<Vector3> vertices, FPNativeList<int> triangles)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		PooledList<int> val = Pool.Get<PooledList<int>>();
		try
		{
			Vector3 val2 = new Vector3(topLeftCorner.x, 0f, topLeftCorner.z);
			TerrainHeightMap heightMap = TerrainMeta.HeightMap;
			TerrainAlphaMap alphaMap = TerrainMeta.AlphaMap;
			int num = 0;
			for (int i = 0; i <= tileSize; i++)
			{
				int num2 = 0;
				while (num2 <= tileSize)
				{
					Vector3 val3 = val2 + new Vector3((float)num2, 0f, (float)i);
					float height = heightMap.GetHeight(val3);
					if (height < -1f)
					{
						((List<int>)(object)val).Add(-1);
					}
					else if (alphaMap.GetAlpha(val3) < 0.1f)
					{
						((List<int>)(object)val).Add(-1);
					}
					else
					{
						val3.y = height;
						((List<int>)(object)val).Add(vertices.Count);
						vertices.Add(val3);
					}
					num2++;
					num++;
				}
			}
			int num3 = 0;
			int num4 = 0;
			while (num4 < tileSize)
			{
				int num5 = 0;
				while (num5 < tileSize)
				{
					int num6 = ((List<int>)(object)val)[num3];
					int num7 = ((List<int>)(object)val)[num3 + tileSize + 1];
					int num8 = ((List<int>)(object)val)[num3 + 1];
					int num9 = ((List<int>)(object)val)[num3 + 1];
					int num10 = ((List<int>)(object)val)[num3 + tileSize + 1];
					int num11 = ((List<int>)(object)val)[num3 + tileSize + 2];
					if (num6 != -1 && num7 != -1 && num8 != -1)
					{
						triangles.Add(num6);
						triangles.Add(num7);
						triangles.Add(num8);
					}
					if (num9 != -1 && num10 != -1 && num11 != -1)
					{
						triangles.Add(num9);
						triangles.Add(num10);
						triangles.Add(num11);
					}
					num5++;
					num3++;
				}
				num4++;
				num3++;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void ExtractTerrainGeometry2(Vector3 topLeftCorner, int tileSize, List<Vector3> vertices, List<int> triangles)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		PooledList<int> val = Pool.Get<PooledList<int>>();
		try
		{
			Vector3 val2 = new Vector3(topLeftCorner.x, 0f, topLeftCorner.z);
			TerrainHeightMap heightMap = TerrainMeta.HeightMap;
			TerrainAlphaMap alphaMap = TerrainMeta.AlphaMap;
			int num = 0;
			for (int i = 0; i <= tileSize; i++)
			{
				int num2 = 0;
				while (num2 <= tileSize)
				{
					Vector3 val3 = val2 + new Vector3((float)num2, 0f, (float)i);
					float height = heightMap.GetHeight(val3);
					if (height < -1f)
					{
						((List<int>)(object)val).Add(-1);
					}
					else if (alphaMap.GetAlpha(val3) < 0.1f)
					{
						((List<int>)(object)val).Add(-1);
					}
					else
					{
						val3.y = height;
						((List<int>)(object)val).Add(vertices.Count);
						vertices.Add(val3);
					}
					num2++;
					num++;
				}
			}
			int num3 = 0;
			int num4 = 0;
			while (num4 < tileSize)
			{
				int num5 = 0;
				while (num5 < tileSize)
				{
					int num6 = ((List<int>)(object)val)[num3];
					int num7 = ((List<int>)(object)val)[num3 + tileSize + 1];
					int num8 = ((List<int>)(object)val)[num3 + 1];
					int num9 = ((List<int>)(object)val)[num3 + 1];
					int num10 = ((List<int>)(object)val)[num3 + tileSize + 1];
					int num11 = ((List<int>)(object)val)[num3 + tileSize + 2];
					if (num6 != -1 && num7 != -1 && num8 != -1)
					{
						triangles.Add(num6);
						triangles.Add(num7);
						triangles.Add(num8);
					}
					if (num9 != -1 && num10 != -1 && num11 != -1)
					{
						triangles.Add(num9);
						triangles.Add(num10);
						triangles.Add(num11);
					}
					num5++;
					num3++;
				}
				num4++;
				num3++;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void ExtractTerrainGeometry3(FPNativeList<Vector3> vertices, FPNativeList<int> triangles, Terrain terrain, Bounds bounds, int gridResolution = 4)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ExtractTerrainGeometry3"))
		{
			if (RustNav.enableVerboseLogs)
			{
				RustNavigation.Log("Processing terrain: " + ((Object)terrain).name);
			}
			TerrainData terrainData = terrain.terrainData;
			Vector3 position = ((Component)terrain).transform.position;
			Bounds val = new Bounds(position + terrainData.size / 2f, terrainData.size);
			Bounds val2 = default(Bounds);
			((Bounds)(ref val2)).SetMinMax(Vector3.Max(((Bounds)(ref val)).min + new Vector3(0.0001f, 0f, 0.0001f), ((Bounds)(ref bounds)).min), Vector3.Min(((Bounds)(ref val)).max, ((Bounds)(ref bounds)).max));
			Vector3 val3 = ((Component)terrain).transform.InverseTransformPoint(((Bounds)(ref val2)).min);
			Vector3 val4 = ((Component)terrain).transform.InverseTransformPoint(((Bounds)(ref val2)).max);
			int num = Mathf.FloorToInt(val3.x / (float)gridResolution);
			int num2 = Mathf.FloorToInt(val3.z / (float)gridResolution);
			int num3 = Mathf.CeilToInt(val4.x / (float)gridResolution);
			int num4 = Mathf.CeilToInt(val4.z / (float)gridResolution);
			int num5 = 0;
			for (int i = num; i < num3; i++)
			{
				for (int j = num2; j < num4; j++)
				{
					Vector3 terrainVertex = GetTerrainVertex(i, j, terrain, gridResolution);
					Vector3 terrainVertex2 = GetTerrainVertex(i + 1, j, terrain, gridResolution);
					Vector3 terrainVertex3 = GetTerrainVertex(i, j + 1, terrain, gridResolution);
					Vector3 terrainVertex4 = GetTerrainVertex(i + 1, j + 1, terrain, gridResolution);
					if (((Bounds)(ref val2)).Contains(terrainVertex) || ((Bounds)(ref val2)).Contains(terrainVertex2) || ((Bounds)(ref val2)).Contains(terrainVertex3) || ((Bounds)(ref val2)).Contains(terrainVertex4))
					{
						AddTriangle(vertices, triangles, terrainVertex, terrainVertex2, terrainVertex3);
						num5++;
						AddTriangle(vertices, triangles, terrainVertex2, terrainVertex4, terrainVertex3);
						num5++;
					}
				}
			}
			if (RustNav.enableVerboseLogs)
			{
				RustNavigation.Log($"Extracted {num5} triangles from terrain");
			}
		}
	}

	private static Vector3 GetTerrainVertex(int x, int z, Terrain terrain, float gridResolution)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GetTerrainVertex"))
		{
			Vector3 val = ((Component)terrain).transform.TransformPoint(new Vector3((float)x * gridResolution, 0f, (float)z * gridResolution));
			float num = terrain.SampleHeight(val);
			num = Mathf.Clamp(num, 0.0001f, terrain.terrainData.size.y - 0.0001f);
			val.y = num + ((Component)terrain).transform.position.y;
			return val;
		}
	}

	private static void AddTriangle(FPNativeList<Vector3> vertices, FPNativeList<int> triangles, Vector3 v1, Vector3 v2, Vector3 v3)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		int count = vertices.Count;
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		triangles.Add(count + 2);
		triangles.Add(count + 1);
		triangles.Add(count);
	}
}

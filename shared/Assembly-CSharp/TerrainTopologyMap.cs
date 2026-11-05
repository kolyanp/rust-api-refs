using System;
using System.Threading.Tasks;
using ConVar;
using TerrainTopologyMapJobs;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class TerrainTopologyMap : TerrainMap<int>
{
	public struct TopologyQueryStructure
	{
		public ReadOnly<int> source;

		public int res;

		[BurstCompile]
		public readonly int GetTopologyFast(Vector2 uv)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			if (Hint.Unlikely(!source.IsCreated))
			{
				return 0;
			}
			int num = res - 1;
			int num2 = (int)(uv.x * (float)res);
			int num3 = (int)(uv.y * (float)res);
			num2 = ((num2 >= 0) ? num2 : 0);
			num3 = ((num3 >= 0) ? num3 : 0);
			num2 = ((num2 <= num) ? num2 : num);
			num3 = ((num3 <= num) ? num3 : num);
			return source[num3 * res + num2];
		}

		public readonly bool GetTopology(float normX, float normZ, int mask)
		{
			int x = Index(normX);
			int z = Index(normZ);
			return GetTopology(x, z, mask);
		}

		public readonly int Index(float normalized)
		{
			int num = (int)(normalized * (float)res);
			if (num >= 0)
			{
				if (num <= res - 1)
				{
					return num;
				}
				return res - 1;
			}
			return 0;
		}

		public readonly bool GetTopology(int x, int z, int mask)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return (source[z * res + x] & mask) != 0;
		}
	}

	public Texture2D TopologyTexture;

	private bool _generatedTopologyTexture;

	private TerrainTopologyHierarchy hierarchy;

	private TerrainTopologyHierarchy.Query hierarchyQuery;

	public override void Setup()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		res = terrainData.alphamapResolution;
		InitArrays(res * res);
		if ((Object)(object)TopologyTexture != (Object)null)
		{
			if (((Texture)TopologyTexture).width == ((Texture)TopologyTexture).height && ((Texture)TopologyTexture).width == res)
			{
				Color32[] pixels = TopologyTexture.GetPixels32();
				int i = 0;
				int num = 0;
				for (; i < res; i++)
				{
					int num2 = 0;
					while (num2 < res)
					{
						dst[i * res + num2] = BitUtility.DecodeInt(pixels[num]);
						num2++;
						num++;
					}
				}
			}
			else
			{
				Debug.LogError((object)("Invalid topology texture: " + ((Object)TopologyTexture).name));
			}
		}
		CreateHierarchy();
	}

	public void SetupEmpty(int newRes)
	{
		res = newRes;
		InitArrays(res * res);
		CreateHierarchy();
	}

	private void CreateHierarchy()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		hierarchy?.Dispose();
		hierarchy = new TerrainTopologyHierarchy(res);
		hierarchyQuery = hierarchy.GetQuery(src.AsReadOnly());
		RefreshHierarchy();
	}

	public void RefreshHierarchy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		hierarchy.Build(src.AsReadOnly());
	}

	public void RefreshHierarchy(int xMin, int zMin, int xMax, int zMax)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		hierarchy.RefreshRegion(src.AsReadOnly(), xMin, zMin, xMax, zMax);
	}

	public void RefreshHierarchy(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		int num = Index(TerrainMeta.NormalizeX(v0.x));
		int num2 = Index(TerrainMeta.NormalizeZ(v0.z));
		int num3 = Index(TerrainMeta.NormalizeX(v1.x));
		int num4 = Index(TerrainMeta.NormalizeZ(v1.z));
		int num5 = Index(TerrainMeta.NormalizeX(v2.x));
		int num6 = Index(TerrainMeta.NormalizeZ(v2.z));
		int num7 = Index(TerrainMeta.NormalizeX(v3.x));
		int num8 = Index(TerrainMeta.NormalizeZ(v3.z));
		RefreshHierarchy(Mathx.Min(num, num3, num5, num7), Mathx.Min(num2, num4, num6, num8), Mathx.Max(num, num3, num5, num7), Mathx.Max(num2, num4, num6, num8));
	}

	private void RefreshHierarchy(float normX, float normZ, float radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		float num = TerrainMeta.OneOverSize.x * radius;
		RefreshHierarchy(Index(normX - num), Index(normZ - num), Index(normX + num), Index(normZ + num));
	}

	public TerrainTopologyHierarchy.Query GetHierarchyQuery()
	{
		return hierarchyQuery;
	}

	protected override void OnDataReplaced()
	{
		RefreshHierarchy();
	}

	public void GenerateTextures()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected Obj, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		TopologyTexture = new Texture2D(res, res, (TextureFormat)4, false, true);
		((Object)TopologyTexture).name = "TopologyTexture";
		((Texture)TopologyTexture).wrapMode = (TextureWrapMode)1;
		NativeArray<Color32> col = TopologyTexture.GetPixelData<Color32>(0);
		Parallel.For(0, res, (int z) =>
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < res; i++)
			{
				col[z * res + i] = BitUtility.EncodeInt(src[z * res + i]);
			}
		});
		_generatedTopologyTexture = Application.isPlaying;
	}

	public void ApplyTextures()
	{
		TopologyTexture.Apply(false, true);
	}

	public bool GetTopology(Vector3 worldPos, int mask)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		return GetTopology(normX, normZ, mask);
	}

	public bool GetTopology(float normX, float normZ, int mask)
	{
		int x = Index(normX);
		int z = Index(normZ);
		return GetTopology(x, z, mask);
	}

	public bool GetTopology(int x, int z, int mask)
	{
		return (src[z * res + x] & mask) != 0;
	}

	public int GetTopology(Vector3 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		return GetTopology(normX, normZ);
	}

	public int GetTopology(float normX, float normZ)
	{
		int x = Index(normX);
		int z = Index(normZ);
		return GetTopology(x, z);
	}

	public int GetTopologyFast(Vector2 uv)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		int num = res - 1;
		int num2 = (int)(uv.x * (float)res);
		int num3 = (int)(uv.y * (float)res);
		num2 = ((num2 >= 0) ? num2 : 0);
		num3 = ((num3 >= 0) ? num3 : 0);
		num2 = ((num2 <= num) ? num2 : num);
		num3 = ((num3 <= num) ? num3 : num);
		return src[num3 * res + num2];
	}

	public TopologyQueryStructure GetQueryStructure()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return new TopologyQueryStructure
		{
			source = src.AsReadOnly(),
			res = res
		};
	}

	public int GetTopology(int x, int z)
	{
		return src[z * res + x];
	}

	public void GetTopologies(NativeArray<Vector3> worldPos, NativeArray<int> results)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		GetTopologyByPosJob getTopologyByPosJob = new GetTopologyByPosJob
		{
			Topologies = results,
			Pos = worldPos,
			Data = src,
			Res = res,
			DataOrigin = new Vector2(TerrainMeta.Position.x, TerrainMeta.Position.z),
			DataScale = new Vector2(TerrainMeta.OneOverSize.x, TerrainMeta.OneOverSize.z)
		};
		IJobExtensions.RunByRef<GetTopologyByPosJob>(ref getTopologyByPosJob);
	}

	public void GetTopologies(NativeArray<Vector2> uvs, NativeArray<int> results)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		GetTopologyByUVJob getTopologyByUVJob = new GetTopologyByUVJob
		{
			Topologies = results,
			UV = uvs,
			Data = src,
			Res = res
		};
		IJobExtensions.RunByRef<GetTopologyByUVJob>(ref getTopologyByUVJob);
	}

	public void GetTopologies(NativeArray<Vector2i> indices, NativeArray<int> results)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		GetTopologyByIndexJob getTopologyByIndexJob = new GetTopologyByIndexJob
		{
			Topologies = results,
			Indices = indices,
			Data = src,
			Res = res
		};
		IJobExtensions.RunByRef<GetTopologyByIndexJob>(ref getTopologyByIndexJob);
	}

	public void GetTopologiesIndirect(ReadOnly<Vector2> uvs, ReadOnly<int> indices, NativeArray<int> results)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		GetTopologyByUVJobIndirect getTopologyByUVJobIndirect = new GetTopologyByUVJobIndirect
		{
			Topologies = results,
			UV = uvs,
			Indices = indices,
			Data = src.AsReadOnly(),
			Res = res
		};
		IJobExtensions.RunByRef<GetTopologyByUVJobIndirect>(ref getTopologyByUVJobIndirect);
	}

	public JobHandle GetTopologiesIndirect(ReadOnly<Vector3> worldPositions, ReadOnly<float> radii, NativeArray<int> results, JobHandle inputDeps = default(JobHandle))
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		GetTopologyRadiusJobIndirect jobData = new GetTopologyRadiusJobIndirect
		{
			WorldX = TerrainMeta.Position.x,
			WorldZ = TerrainMeta.Position.z,
			OneOverSizeX = TerrainMeta.OneOverSize.x,
			OneOverSizeZ = TerrainMeta.OneOverSize.z,
			Topology = GetHierarchyQuery(),
			WorldPositions = worldPositions,
			Radii = radii,
			Topologies = results
		};
		return ParallelJobEx.ScheduleParallelByRef<GetTopologyRadiusJobIndirect>(ref jobData, worldPositions.Length, inputDeps);
	}

	public void GetTopologiesIndirect(ReadOnly<Vector2> normalizedCoords, ReadOnly<float> radii, NativeArray<int> results)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		GetTopologyRadiusNormalizedJobIndirect jobData = new GetTopologyRadiusNormalizedJobIndirect
		{
			OneOverSizeX = TerrainMeta.OneOverSize.x,
			Topology = GetHierarchyQuery(),
			WorldNXZ = normalizedCoords,
			Radii = radii,
			Topologies = results
		};
		int length = normalizedCoords.Length;
		JobHandle dependsOn = default;
		dependsOn = ParallelJobEx.ScheduleParallelByRef<GetTopologyRadiusNormalizedJobIndirect>(ref jobData, length, dependsOn);
		dependsOn.Complete();
	}

	public void SetTopology(Vector3 worldPos, int mask)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		SetTopology(normX, normZ, mask);
	}

	public void SetTopology(float normX, float normZ, int mask)
	{
		int x = Index(normX);
		int z = Index(normZ);
		SetTopology(x, z, mask);
	}

	public void SetTopology(int x, int z, int mask)
	{
		dst[z * res + x] = mask;
	}

	public void AddTopology(Vector3 worldPos, int mask)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		AddTopology(normX, normZ, mask);
	}

	public void AddTopology(float normX, float normZ, int mask)
	{
		int x = Index(normX);
		int z = Index(normZ);
		AddTopology(x, z, mask);
	}

	public void AddTopology(int x, int z, int mask)
	{
		ref NativeArray<int> reference = ref dst;
		int num = z * res + x;
		reference[num] |= mask;
	}

	public void RemoveTopology(Vector3 worldPos, int mask)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		RemoveTopology(normX, normZ, mask);
	}

	public void RemoveTopology(float normX, float normZ, int mask)
	{
		int x = Index(normX);
		int z = Index(normZ);
		RemoveTopology(x, z, mask);
	}

	public void RemoveTopology(int x, int z, int mask)
	{
		ref NativeArray<int> reference = ref dst;
		int num = z * res + x;
		reference[num] &= ~mask;
	}

	public int GetTopology(Vector3 worldPos, float radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		return GetTopology(normX, normZ, radius);
	}

	public int GetTopologyJob(Vector3 worldPos, float radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		return GetTopologyJob(normX, normZ, radius);
	}

	public int GetTopologyJob(float normX, float normZ, float radius)
	{
		return GetTopologyJob(normX, normZ, radius, -1, stopOnAny: false);
	}

	private int GetTopologyJob(float normX, float normZ, float radius, int mask, bool stopOnAny)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		float num = TerrainMeta.OneOverSize.x * radius;
		int x_mid = Index(normX);
		int z_mid = Index(normZ);
		int x_min = Index(normX - num);
		int x_max = Index(normX + num);
		int z_min = Index(normZ - num);
		int z_max = Index(normZ + num);
		TerrainTopologyHierarchy.Query topology = GetHierarchyQuery();
		int num2 = (stopOnAny ? TerrainTopologyMapJobUtil.AnyRadius(in topology, x_min, x_mid, x_max, z_min, z_mid, z_max, radius, mask) : TerrainTopologyMapJobUtil.OrRadius(in topology, x_min, x_mid, x_max, z_min, z_mid, z_max, radius, mask));
		if (ConVar.World.topology_hierarchy_validate)
		{
			ValidateTopologyRadius(num2, radius, mask, stopOnAny, x_min, x_mid, x_max, z_min, z_mid, z_max);
		}
		return num2;
	}

	private void ValidateTopologyRadius(int topology, float radius, int mask, bool stopOnAny, int x_min, int x_mid, int x_max, int z_min, int z_mid, int z_max)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		int num = TerrainTopologyMapJobUtil.GetTopologyRadius(src.AsReadOnly(), res, radius, x_min, x_mid, x_max, z_min, z_mid, z_max) & mask;
		if (!(stopOnAny ? (topology != 0 == (num != 0)) : (topology == num)))
		{
			Debug.LogError((object)string.Format("Topology hierarchy mismatch at ({0}, {1}) radius {2} mask {3} stopOnAny {4}: hierarchy {5}, flat {6}", new object[7] { x_mid, z_mid, radius, mask, stopOnAny, topology, num }));
		}
	}

	public int GetTopology(float normX, float normZ, float radius)
	{
		return GetTopologyJob(normX, normZ, radius);
	}

	public int GetTopology(Vector3 worldPos, float radius, int mask)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		return GetTopology(normX, normZ, radius, mask);
	}

	public int GetTopology(float normX, float normZ, float radius, int mask)
	{
		return GetTopologyJob(normX, normZ, radius, mask, stopOnAny: false);
	}

	public bool HasTopology(Vector3 worldPos, float radius, int mask)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		return HasTopology(normX, normZ, radius, mask);
	}

	public bool HasTopology(float normX, float normZ, float radius, int mask)
	{
		return GetTopologyJob(normX, normZ, radius, mask, stopOnAny: true) != 0;
	}

	public void SetTopology(Vector3 worldPos, int mask, float radius, float fade = 0f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		SetTopology(normX, normZ, mask, radius, fade);
	}

	public void SetTopology(float normX, float normZ, int mask, float radius, float fade = 0f)
	{
		Action<int, int, float> action = (int x, int z, float lerp) =>
		{
			if ((double)lerp > 0.5)
			{
				dst[z * res + x] = mask;
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
		RefreshHierarchy(normX, normZ, radius);
	}

	public void AddTopology(Vector3 worldPos, int mask, float radius, float fade = 0f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		AddTopology(normX, normZ, mask, radius, fade);
	}

	public void AddTopology(float normX, float normZ, int mask, float radius, float fade = 0f)
	{
		Action<int, int, float> action = (int x, int z, float lerp) =>
		{
			if ((double)lerp > 0.5)
			{
				ref NativeArray<int> reference = ref dst;
				int num = z * res + x;
				reference[num] |= mask;
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
		RefreshHierarchy(normX, normZ, radius);
	}

	public override void Dispose()
	{
		base.Dispose();
		hierarchy?.Dispose();
		hierarchy = null;
		hierarchyQuery = default;
		if (_generatedTopologyTexture && (Object)(object)TopologyTexture != (Object)null)
		{
			Object.Destroy((Object)(object)TopologyTexture);
			TopologyTexture = null;
		}
	}
}

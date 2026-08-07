using System;
using System.Threading;
using System.Threading.Tasks;
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

	private ThreadLocal<NativeReference<int>> topoNative = new ThreadLocal<NativeReference<int>>(() => new NativeReference<int>(0, AllocatorHandle.op_Implicit((Allocator)4)), trackAllValues: true);

	public override void Setup()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		res = terrainData.alphamapResolution;
		InitArrays(res * res);
		if (!((Object)(object)TopologyTexture != (Object)null))
		{
			return;
		}
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

	public void SetupEmpty(int newRes)
	{
		res = newRes;
		InitArrays(res * res);
	}

	public void GenerateTextures()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		TopologyTexture = new Texture2D(res, res, (TextureFormat)4, false, true);
		((Object)TopologyTexture).name = "TopologyTexture";
		((Texture)TopologyTexture).wrapMode = (TextureWrapMode)1;
		NativeArray<Color32> col = TopologyTexture.GetPixelData<Color32>(0);
		Parallel.For(0, res, delegate(int z)
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
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		TerrainTopologyMapJobs.GetTopologyRadiusJobIndirect jobData = new TerrainTopologyMapJobs.GetTopologyRadiusJobIndirect
		{
			WorldX = TerrainMeta.Position.x,
			WorldZ = TerrainMeta.Position.z,
			OneOverSizeX = TerrainMeta.OneOverSize.x,
			OneOverSizeZ = TerrainMeta.OneOverSize.z,
			Src = src.AsReadOnly(),
			Res = res,
			WorldPositions = worldPositions,
			Radii = radii,
			Topologies = results
		};
		return ParallelJobEx.ScheduleParallelByRef<TerrainTopologyMapJobs.GetTopologyRadiusJobIndirect>(ref jobData, worldPositions.Length, inputDeps);
	}

	public void GetTopologiesIndirect(ReadOnly<Vector2> normalizedCoords, ReadOnly<float> radii, NativeArray<int> results)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		TerrainTopologyMapJobs.GetTopologyRadiusNormalizedJobIndirect jobData = new TerrainTopologyMapJobs.GetTopologyRadiusNormalizedJobIndirect
		{
			OneOverSizeX = TerrainMeta.OneOverSize.x,
			Src = src.AsReadOnly(),
			Res = res,
			WorldNXZ = normalizedCoords,
			Radii = radii,
			Topologies = results
		};
		int length = normalizedCoords.Length;
		JobHandle dependsOn = default(JobHandle);
		dependsOn = ParallelJobEx.ScheduleParallelByRef<TerrainTopologyMapJobs.GetTopologyRadiusNormalizedJobIndirect>(ref jobData, length, dependsOn);
		((JobHandle)(ref dependsOn)).Complete();
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
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		float num = TerrainMeta.OneOverSize.x * radius;
		int x_mid = Index(normX);
		int z_mid = Index(normZ);
		int x_min = Index(normX - num);
		int x_max = Index(normX + num);
		int z_min = Index(normZ - num);
		int z_max = Index(normZ + num);
		NativeReference<int> value = topoNative.Value;
		TerrainTopologyMapJobs.GetTopologyRadiusJob getTopologyRadiusJob = new TerrainTopologyMapJobs.GetTopologyRadiusJob
		{
			Res = res,
			Src = src.AsReadOnly(),
			Topo = value,
			radius = radius,
			x_mid = x_mid,
			z_mid = z_mid,
			x_min = x_min,
			x_max = x_max,
			z_min = z_min,
			z_max = z_max
		};
		IJobExtensions.RunByRef<TerrainTopologyMapJobs.GetTopologyRadiusJob>(ref getTopologyRadiusJob);
		return value.Value;
	}

	public int GetTopology(float normX, float normZ, float radius)
	{
		return GetTopologyJob(normX, normZ, radius);
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
		Action<int, int, float> action = delegate(int x, int z, float lerp)
		{
			if ((double)lerp > 0.5)
			{
				dst[z * res + x] = mask;
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
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
		Action<int, int, float> action = delegate(int x, int z, float lerp)
		{
			if ((double)lerp > 0.5)
			{
				ref NativeArray<int> reference = ref dst;
				int num = z * res + x;
				reference[num] |= mask;
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
	}

	public override void Dispose()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		base.Dispose();
		if (_generatedTopologyTexture && (Object)(object)TopologyTexture != (Object)null)
		{
			Object.Destroy((Object)(object)TopologyTexture);
			TopologyTexture = null;
		}
		foreach (NativeReference<int> value in topoNative.Values)
		{
			if (value.IsCreated)
			{
				value.Dispose();
			}
		}
	}
}

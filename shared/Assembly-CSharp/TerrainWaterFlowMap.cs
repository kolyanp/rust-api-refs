using System;
using System.Threading.Tasks;
using Facepunch;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class TerrainWaterFlowMap : TerrainMap<byte>
{
	private const float TwoPi = MathF.PI * 2f;

	public override void Setup()
	{
		res = terrainData.heightmapResolution;
		InitArrays(res * res);
	}

	public override void PostSetup()
	{
		using (TimeWarning.New("TerrainWaterFlowMap.PostSetup"))
		{
			WriteWaterFlowFromShoreVectors();
			WriteWaterFlowFromRivers();
		}
	}

	private void WriteWaterFlowFromShoreVectors()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		NativeArray<Vector2> normalizedCoords = new NativeArray<Vector2>(res * res, (Allocator)3, (NativeArrayOptions)1);
		NativeArray<float> radii = new NativeArray<float>(res * res, (Allocator)3, (NativeArrayOptions)1);
		NativeArray<int> topologies = new NativeArray<int>(res * res, (Allocator)3, (NativeArrayOptions)1);
		Parallel.For(0, res, delegate(int z)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			float num = Coordinate(z);
			for (int i = 0; i < res; i++)
			{
				float num2 = Coordinate(i);
				normalizedCoords[z * res + i] = new Vector2(num2, num);
				radii[z * res + i] = 16f;
			}
		});
		TerrainMeta.TopologyMap.GetTopologiesIndirect(normalizedCoords.AsReadOnly(), radii.AsReadOnly(), topologies);
		TerrainTexturing.ShoreData shoreMap = TerrainTexturing.Instance.GetMap(isDeepSea: false);
		Parallel.For(0, res, delegate(int z)
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			float num = Coordinate(z);
			Vector3 flow = default(Vector3);
			for (int i = 0; i < res; i++)
			{
				float num2 = Coordinate(i);
				int num3 = topologies[z * res + i];
				Vector4 rawShoreVector = shoreMap.GetRawShoreVector(new Vector2(num2, num));
				((Vector3)(ref flow))._002Ector(rawShoreVector.x, 0f, rawShoreVector.y);
				if ((num3 & 0x14080) != 0)
				{
					SetFlowDirection(num2, num, flow);
				}
			}
		});
		normalizedCoords.Dispose(default(JobHandle));
		radii.Dispose(default(JobHandle));
		topologies.Dispose(default(JobHandle));
	}

	private void WriteWaterFlowFromRivers()
	{
		foreach (PathList river in TerrainMeta.Path.Rivers)
		{
			river.AdjustTerrainWaterFlow(scaleWidthWithLength: true);
		}
	}

	public Vector3 GetFlowDirection(Vector3 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		return GetFlowDirection(normX, normZ);
	}

	public Vector3 GetFlowDirection(Vector2 worldPos2D)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos2D.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos2D.y);
		return GetFlowDirection(normX, normZ);
	}

	public Vector3 GetFlowDirection(float normX, float normZ)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		int num = Index(normX);
		int num2 = Index(normZ);
		float num3 = ByteToAngle(src[num2 * res + num]);
		return new Vector3(Mathf.Sin(num3), 0f, Mathf.Cos(num3));
	}

	public void SetFlowDirection(Vector3 worldPos, Vector3 flow)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		SetFlowDirection(normX, normZ, flow);
	}

	public void SetFlowDirection(float normX, float normZ, Vector3 flow)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		int num = Index(normX);
		int num2 = Index(normZ);
		Vector3 val = Vector3Extensions.XZ(flow, 0f);
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		byte b = AngleToByte(Mathf.Atan2(normalized.x, normalized.z));
		src[num2 * res + num] = b;
	}

	public static float ByteToAngle(byte b)
	{
		return (float)(int)b / 255f * (MathF.PI * 2f) - MathF.PI;
	}

	private static byte AngleToByte(float a)
	{
		a = Mathf.Clamp(a, -MathF.PI, MathF.PI);
		return (byte)Mathf.RoundToInt((a + MathF.PI) / (MathF.PI * 2f) * 255f);
	}

	public NativeArray<float3> GetFlowDirections(NativeArray<Vector3> positions3D, Allocator allocator)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		NativeArray<float3> results = default(NativeArray<float3>);
		results._002Ector(positions3D.Length, allocator, (NativeArrayOptions)1);
		TerrainWaterFlowMapBurst.GetFlowDirections(in positions3D, ref results, in src, in res);
		return results;
	}
}

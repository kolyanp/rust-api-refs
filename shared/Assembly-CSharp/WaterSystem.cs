using System;
using System.Collections.Generic;
using ConVar;
using Rust;
using Rust.Water5;
using TerrainWaterMapJobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using WaterSystemJobs;

[ExecuteInEditMode]
public class WaterSystem : MonoBehaviour
{
	[Serializable]
	public class RenderingSettings
	{
		[Serializable]
		public class SkyProbe
		{
			public float ProbeUpdateInterval = 1f;

			public bool TimeSlicing = true;
		}

		[Serializable]
		public class SSR
		{
			public float FresnelCutoff = 0.02f;

			public float ThicknessMin = 1f;

			public float ThicknessMax = 20f;

			public float ThicknessStartDist = 40f;

			public float ThicknessEndDist = 100f;
		}

		public Vector4[] TessellationQuality;

		public SkyProbe SkyReflections;

		public SSR ScreenSpaceReflections;
	}

	private struct OceanSimulationWrapper : IDisposable
	{
		internal OceanSimulation mainlandSimulation;

		internal OceanSimulation deepSeaSimulation;

		internal Rust.Water5.NativeOceanDisplacementShort3 mainlandData;

		internal Rust.Water5.NativeOceanDisplacementShort3 deepSeaData;

		private bool sharedSimData;

		internal static OceanSimulationWrapper Init(OceanSettings mainland, OceanSettings deepsea)
		{
			OceanSimulationWrapper result = default(OceanSimulationWrapper);
			result.sharedSimData = (Object)(object)mainland == (Object)(object)deepsea;
			result.mainlandData = mainland.LoadNativeSimData();
			result.deepSeaData = (result.sharedSimData ? result.mainlandData : deepsea.LoadNativeSimData());
			result.mainlandSimulation = new OceanSimulation(mainland, result.mainlandData);
			result.deepSeaSimulation = new OceanSimulation(mainland, result.deepSeaData);
			return result;
		}

		public OceanSimulation GetOceanSimulation(Vector3 worldPosition)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			if (!DeepSeaManager.IsInsideDeepSea(worldPosition))
			{
				return mainlandSimulation;
			}
			return deepSeaSimulation;
		}

		public OceanSimulation GetOceanSimulation(bool isDeepSea)
		{
			if (!isDeepSea)
			{
				return mainlandSimulation;
			}
			return deepSeaSimulation;
		}

		public void Dispose()
		{
			mainlandSimulation?.Dispose();
			deepSeaSimulation?.Dispose();
			mainlandData.Dispose();
			if (!sharedSimData)
			{
				deepSeaData.Dispose();
			}
		}
	}

	private static float oceanLevel = 0f;

	[Header("Ocean Settings")]
	public OceanSettings oceanSettings;

	private OceanSimulationWrapper oceanSimulationWrapper;

	public WaterQuality Quality = WaterQuality.High;

	public Material oceanMaterial;

	public RenderingSettings Rendering = new RenderingSettings();

	public ComputeShader oceanVFaceShader;

	public OceanVariantMaterial TropicalMaterial;

	public int patchSize = 100;

	public int patchCount = 4;

	public float patchScale = 1f;

	public bool forceDeepSea;

	public static WaterSystem Instance { get; private set; }

	public static WaterCollision Collision { get; private set; }

	public static WaterBody Ocean { get; private set; }

	public static Material OceanMaterial => Instance?.oceanMaterial;

	public static ListHashSet<WaterCamera> WaterCameras { get; } = new ListHashSet<WaterCamera>();

	public static HashSet<WaterBody> WaterBodies { get; } = new HashSet<WaterBody>();

	public static HashSet<WaterDepthMask> DepthMasks { get; } = new HashSet<WaterDepthMask>();

	public static float WaveTime { get; private set; }

	public static ComputeShader OceanVFaceShader => Instance?.oceanVFaceShader;

	public static bool SuspendProcessing => false;

	public static float OceanLevel
	{
		get
		{
			return oceanLevel;
		}
		set
		{
			value = Mathf.Max(value, 0f);
			if (!Mathf.Approximately(oceanLevel, value))
			{
				oceanLevel = value;
				UpdateOceanLevel();
			}
		}
	}

	public bool IsInitialized { get; private set; }

	public int Layer => ((Component)this).gameObject.layer;

	public int Reflections => Water.reflections;

	public float WindDirection => oceanSettings.windDirection;

	public float[] OctaveScales => oceanSettings.octaveScales;

	private void EditorInitialize()
	{
	}

	private void EditorShutdown()
	{
	}

	public OceanSimulation GetOceanSimulation(Vector3 worldPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return oceanSimulationWrapper.GetOceanSimulation(worldPosition);
	}

	public OceanSimulation GetOceanSimulation(bool isDeep)
	{
		return oceanSimulationWrapper.GetOceanSimulation(isDeep);
	}

	private void CheckInstance()
	{
		Instance = (((Object)(object)Instance != (Object)null) ? Instance : this);
		Collision = (((Object)(object)Collision != (Object)null) ? Collision : ((Component)this).GetComponent<WaterCollision>());
	}

	private void Awake()
	{
		CheckInstance();
	}

	private void OnEnable()
	{
		CheckInstance();
		oceanSimulationWrapper = OceanSimulationWrapper.Init(oceanSettings, oceanSettings);
		IsInitialized = true;
	}

	private void OnDisable()
	{
		if (!Application.isPlaying || !Application.isQuitting)
		{
			oceanSimulationWrapper.Dispose();
			oceanSimulationWrapper = default(OceanSimulationWrapper);
			IsInitialized = false;
			Instance = null;
		}
	}

	private void Update()
	{
		if (SuspendProcessing)
		{
			return;
		}
		using (TimeWarning.New("UpdateWaves"))
		{
			UpdateOceanSimulation();
		}
	}

	public static bool Trace(Ray ray, out Vector3 position, float maxDist = 100f)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Instance == (Object)null)
		{
			position = Vector3.zero;
			return false;
		}
		if (Instance.GetOceanSimulation(((Ray)(ref ray)).origin).Trace(ray, maxDist, out position) && TerrainMeta.TopologyMap.GetTopology(position, 384))
		{
			return true;
		}
		return false;
	}

	public static bool Trace(Ray ray, out Vector3 position, out Vector3 normal, float maxDist = 100f)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Instance == (Object)null)
		{
			position = Vector3.zero;
			normal = Vector3.zero;
			return false;
		}
		normal = Vector3.up;
		if (Instance.GetOceanSimulation(((Ray)(ref ray)).origin).Trace(ray, maxDist, out position) && TerrainMeta.TopologyMap.GetTopology(position, 384))
		{
			return true;
		}
		return false;
	}

	public static JobHandle ScheduleTraceBatchDefer(NativeList<Ray> rays, NativeArray<float> maxDists, NativeArray<bool> hitResults, NativeArray<Vector3> hitPositions, NativeArray<Vector3> hitNormals, NativeList<int> deepIndices, NativeList<int> mainIndices, JobHandle inputDeps)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Instance == (Object)null)
		{
			inputDeps = IJobParallelForDeferExtensions.Schedule<WaterSystemJobs.FillFalseJobDefer, Ray>(new WaterSystemJobs.FillFalseJobDefer
			{
				rays = rays,
				HitResults = hitResults
			}, rays, 256, inputDeps);
			return inputDeps;
		}
		NativeArray<Ray> rays2 = rays.AsDeferredJobArray();
		inputDeps = Instance.oceanSimulationWrapper.mainlandSimulation.TraceBatch(mainIndices, rays2, maxDists, hitResults, hitPositions, inputDeps);
		inputDeps = Instance.oceanSimulationWrapper.deepSeaSimulation.TraceBatch(deepIndices, rays2, maxDists, hitResults, hitPositions, inputDeps);
		inputDeps = IJobParallelForDeferExtensions.Schedule<WaterSystemJobs.AdjustByTopologyJob, Ray>(new WaterSystemJobs.AdjustByTopologyJob
		{
			rays = rays,
			hitResults = hitResults,
			hitNormals = hitNormals,
			hitPositions = hitPositions.AsReadOnly(),
			TopologyData = TerrainMeta.TopologyMap.src,
			TopologyRes = TerrainMeta.TopologyMap.res,
			DataOrigin = new Vector2(TerrainMeta.Position.x, TerrainMeta.Position.z),
			DataScale = new Vector2(TerrainMeta.OneOverSize.x, TerrainMeta.OneOverSize.z)
		}, rays, 256, inputDeps);
		return inputDeps;
	}

	private static void GetHeightArray_Native(in NativeArray<Vector2> pos, in NativeArray<Vector2> posUV, ref NativeArray<float> shore, ref NativeArray<float> terrainHeight, ref NativeArray<float> waterHeight, in bool isDeepSea)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		TerrainTexturing.ShoreData shoreData = TerrainTexturing.Instance.GetMap(isDeepSea);
		TerrainHeightMap.HeightMapQueryStructure terrainHeightMapData = TerrainMeta.HeightMap.GetQueryStructure(isDeepSea);
		bool hasHeightMap = (Object)(object)TerrainTexturing.Instance != (Object)null;
		bool hasWaterAndTopology = (Object)(object)Instance != (Object)null && Object.op_Implicit((Object)(object)TerrainMeta.WaterMap) && Object.op_Implicit((Object)(object)TerrainMeta.TopologyMap);
		bool hasTerrainTexturing = (Object)(object)TerrainTexturing.Instance != (Object)null;
		GetHeightByUVJob getHeightByUVJob = TerrainMeta.WaterMap.FetchUVsHeightsJob(posUV, waterHeight);
		TerrainTopologyMap.TopologyQueryStructure topologyQueryStructure = TerrainMeta.TopologyMap.GetQueryStructure();
		bool hasWaterSystem = (Object)(object)Instance != (Object)null;
		OceanSimulation oceanSimulation = (hasWaterSystem ? Instance.GetOceanSimulation(isDeepSea) : null);
		float OceanLevel = WaterSystem.OceanLevel;
		float MaxOceanLevel = (hasWaterSystem ? oceanSimulation.MaxLevel() : OceanLevel);
		TerrainWaterMap.WaterMapQueryStructure waterMapQueryStruct = TerrainMeta.WaterMap.GetQueryStructure();
		WaterSystemBurst.GetHeightArray_Burst(posUV.AsReadOnly(), ref shore, ref terrainHeight, in shoreData, in terrainHeightMapData, in hasHeightMap, in hasTerrainTexturing);
		IJobExtensions.RunByRef<GetHeightByUVJob>(ref getHeightByUVJob);
		if (hasWaterSystem)
		{
			oceanSimulation.GetHeightBatch(NativeArray<Vector2>.op_Implicit(ref pos), NativeArray<float>.op_Implicit(ref waterHeight), NativeArray<float>.op_Implicit(ref shore), NativeArray<float>.op_Implicit(ref terrainHeight));
		}
		WaterSystemBurst.ComputeOceanSimHeight_Burst(in pos, in posUV, ref shore, ref waterHeight, in isDeepSea, in hasWaterAndTopology, in topologyQueryStructure, in OceanLevel, in MaxOceanLevel, in hasWaterSystem, in waterMapQueryStruct);
	}

	public static void GetHeightArray_Managed(Vector2[] pos, Vector2[] posUV, float[] shore, float[] terrainHeight, float[] waterHeight, bool isDeepSea)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TerrainTexturing.Instance != (Object)null)
		{
			TerrainTexturing.ShoreData map = TerrainTexturing.Instance.GetMap(isDeepSea);
			for (int i = 0; i < posUV.Length; i++)
			{
				shore[i] = map.GetCoarseDistanceToShore(posUV[i]);
			}
		}
		else
		{
			Array.Fill(shore, 0f, 0, posUV.Length);
		}
		if ((Object)(object)TerrainMeta.HeightMap != (Object)null)
		{
			if (isDeepSea)
			{
				Array.Fill(terrainHeight, DeepSeaManager.SeaFloorDepth);
			}
			else
			{
				for (int j = 0; j < posUV.Length; j++)
				{
					terrainHeight[j] = TerrainMeta.HeightMap.GetHeight(posUV[j]);
				}
			}
		}
		else
		{
			Array.Fill(terrainHeight, 0f, 0, posUV.Length);
		}
		if ((Object)(object)Instance != (Object)null && Object.op_Implicit((Object)(object)TerrainMeta.WaterMap) && Object.op_Implicit((Object)(object)TerrainMeta.TopologyMap))
		{
			bool flag = false;
			OceanSimulation oceanSimulation = Instance.GetOceanSimulation(isDeepSea);
			for (int k = 0; k < posUV.Length; k++)
			{
				Vector2 val = posUV[k];
				float num = TerrainMeta.WaterMap.GetHeightFast(val, isDeepSea);
				if (num < OceanLevel + oceanSimulation.MaxLevel() && (isDeepSea || TerrainMeta.TopologyMap.GetTopology(val.x, val.y, 384)))
				{
					if (!flag)
					{
						oceanSimulation.GetHeightBatch(pos, waterHeight, shore, terrainHeight);
						flag = true;
					}
					float num2 = waterHeight[k] + OceanLevel;
					num = Mathf.Max(num, num2);
				}
				waterHeight[k] = num;
			}
		}
		else if ((Object)(object)Instance != (Object)null)
		{
			Instance.GetOceanSimulation(isDeepSea).GetHeightBatch(pos, waterHeight, shore, terrainHeight);
			for (int l = 0; l < pos.Length; l++)
			{
				waterHeight[l] += OceanLevel;
			}
		}
		else
		{
			Array.Fill(waterHeight, OceanLevel, 0, pos.Length);
			Array.Fill(shore, 0f, 0, posUV.Length);
		}
	}

	public static void GetHeightArray(Vector2[] pos, Vector2[] posUV, float[] shore, float[] terrainHeight, float[] waterHeight, bool isDeepSea)
	{
		GetHeightArray_Managed(pos, posUV, shore, terrainHeight, waterHeight, isDeepSea);
	}

	public static void GetHeightArray(in NativeArray<Vector2> pos, in NativeArray<Vector2> posUV, ref NativeArray<float> shore, ref NativeArray<float> terrainHeight, ref NativeArray<float> waterHeight, bool isDeepSea)
	{
		GetHeightArray_Native(in pos, in posUV, ref shore, ref terrainHeight, ref waterHeight, in isDeepSea);
	}

	public static void RegisterBody(WaterBody body)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (body.Type == WaterBodyType.Ocean)
		{
			if ((Object)(object)Ocean == (Object)null)
			{
				Ocean = body;
				body.Transform.position = Vector3Ex.WithY(body.Transform.position, OceanLevel);
			}
			else if ((Object)(object)Ocean != (Object)(object)body)
			{
				Debug.LogWarning((object)"[Water] Ocean body is already registered. Ignoring call because only one is allowed.");
				return;
			}
		}
		WaterBodies.Add(body);
	}

	public static void UnregisterBody(WaterBody body)
	{
		if ((Object)(object)body == (Object)(object)Ocean)
		{
			Ocean = null;
		}
		WaterBodies.Remove(body);
	}

	private static void UpdateOceanLevel()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Ocean != (Object)null)
		{
			Ocean.Transform.position = Vector3Ex.WithY(Ocean.Transform.position, OceanLevel);
		}
		foreach (WaterBody waterBody in WaterBodies)
		{
			waterBody.OnOceanLevelChanged(OceanLevel);
		}
	}

	private void UpdateOceanSimulation()
	{
		if (Water.scaled_time)
		{
			WaveTime += Time.deltaTime;
		}
		else
		{
			WaveTime = Time.realtimeSinceStartup;
		}
		if (Weather.ocean_time >= 0f)
		{
			WaveTime = Weather.ocean_time;
		}
		float beaufort = (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance) ? SingletonComponent<Climate>.Instance.WeatherState.OceanScale : 4f);
		oceanSimulationWrapper.mainlandSimulation?.Update(WaveTime, Time.deltaTime, beaufort);
		float beaufort2 = (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance) ? SingletonComponent<Climate>.Instance.DeepSeaWeatherState.OceanScale : 4f);
		oceanSimulationWrapper.deepSeaSimulation?.Update(WaveTime, Time.deltaTime, beaufort2);
	}

	public void Refresh()
	{
		oceanSimulationWrapper.Dispose();
		oceanSimulationWrapper = OceanSimulationWrapper.Init(oceanSettings, oceanSettings);
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class LivestockFloor : IPooled
{
	[BurstCompile(CompileSynchronously = true)]
	private struct LampJob : IJob
	{
		[NativeDisableUnsafePtrRestriction]
		public unsafe Vector3* Points;

		public int PointCount;

		[NativeDisableUnsafePtrRestriction]
		public unsafe OBB* Lights;

		public int LightCount;

		public float Range;

		[NativeDisableUnsafePtrRestriction]
		public unsafe int* Lit;

		public unsafe void Execute()
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			int num = 0;
			for (int i = 0; i < PointCount; i++)
			{
				for (int j = 0; j < LightCount; j++)
				{
					if (GrowableEntity.IsInArtificialLightRange(Lights[j], Points[i], Range))
					{
						num++;
						break;
					}
				}
			}
			*Lit = num;
		}
	}

	private struct SeenLight
	{
		public CeilingLight Light;

		public bool On;
	}

	[BurstCompile(CompileSynchronously = true)]
	private struct GridJob : IJob
	{
		[NativeDisableUnsafePtrRestriction]
		public IntPtr Navmesh;

		public Matrix4x4 WorldToNav;

		public Matrix4x4 NavToWorld;

		public Vector3 Corner;

		public Vector3 Extents;

		public float Spacing;

		public float Half;

		[NativeDisableUnsafePtrRestriction]
		public unsafe ulong* Refs;

		public int RefCount;

		public LivestockGrass.Ground Ground;

		[NativeDisableUnsafePtrRestriction]
		public unsafe Vector3* Samples;

		[NativeDisableUnsafePtrRestriction]
		public unsafe bool* Grass;

		[NativeDisableUnsafePtrRestriction]
		public unsafe int* Sampled;

		public unsafe void Execute()
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			int num = 0;
			Vector3 extents = Extents;
			Vector3 val3 = default;
			ulong polyRef = default;
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					Vector3 val = Corner + new Vector3((float)i * Spacing, 0f, (float)j * Spacing);
					Vector3 val2 = WorldToNav.MultiplyPoint3x4(val);
					if (RecastWrapper.SamplePositionInJob(Navmesh, &val2, &extents, &val3, &polyRef, 0, 0) != 0 && !(val3 == Vector3.zero) && IsHeld(polyRef))
					{
						Vector3 val4 = NavToWorld.MultiplyPoint3x4(val3);
						if (!(Mathf.Abs(val4.x - val.x) > Half) && !(Mathf.Abs(val4.z - val.z) > Half))
						{
							System.Runtime.CompilerServices.Unsafe.Write(Samples + num, val4);
							Grass[num++] = Ground.IsGrass(val4);
						}
					}
				}
			}
			*Sampled = num;
		}

		private unsafe bool IsHeld(ulong polyRef)
		{
			if (polyRef != 0L)
			{
				return NativeSortExtension.BinarySearch<ulong>(Refs, RefCount, polyRef) >= 0;
			}
			return false;
		}
	}

	public struct SupplyEstimate
	{
		public float FoodMinutes;

		public float WaterMinutes;

		public int Heads;

		public int Troughs;

		public ItemDefinition FoodItem;

		public bool ShoreInReach;
	}

	private class RayBatch : IPooled
	{
		public JobHandle Job;

		private NativeArray<RaycastCommand> commands;

		private NativeArray<RaycastHit> hits;

		private readonly Vector3[] sunPoints = new Vector3[256];

		private readonly bool[] sunLit = new bool[256];

		private int hourCount;

		private int probeCount;

		private float areaScale;

		public float GrassArea { get; private set; }

		public int SunPointCount { get; private set; }

		public unsafe void Begin(Vector3[] samples, bool[] grass, int sampled, int probes, int sunPolys, int hours, float scale)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < sunPolys; i++)
			{
				sunPoints[i] = samples[i * sampled / sunPolys];
			}
			SunPointCount = sunPolys;
			hourCount = hours;
			probeCount = probes;
			areaScale = scale;
			int num = probes + sunPolys * hours;
			commands = new NativeArray<RaycastCommand>(num, (Allocator)4, (NativeArrayOptions)0);
			hits = new NativeArray<RaycastHit>(num, (Allocator)4, (NativeArrayOptions)1);
			fixed (Vector3* samples2 = samples)
			{
				fixed (bool* grass2 = grass)
				{
					fixed (Vector3* ptr = sunPoints)
					{
						fixed (Vector3* sunDirections = LivestockFloor.sunDirections)
						{
							IJobExtensions.Run<AimJob>(new AimJob
							{
								Commands = commands,
								Scene = Physics.defaultPhysicsScene,
								GrassQuery = GrassProbeQuery,
								SunQuery = SunRayQuery,
								Samples = samples2,
								Grass = grass2,
								SampleCount = sampled,
								SunPoints = ptr,
								SunPointCount = sunPolys,
								Directions = sunDirections,
								HourCount = hours
							});
						}
					}
				}
			}
		}

		public void Schedule()
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			Job = RaycastCommand.ScheduleBatch(commands, hits, 32, 1, default(JobHandle));
			JobHandle.ScheduleBatchedJobs();
		}

		public unsafe void Land()
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			int num = 0;
			fixed (bool* ptr = sunLit)
			{
				IJobExtensions.Run<FoldJob>(new FoldJob
				{
					Hits = hits,
					Probes = probeCount,
					SunPointCount = SunPointCount,
					HourCount = hourCount,
					SunLit = ptr,
					Open = &num
				});
			}
			GrassArea = (float)num * areaScale;
		}

		public int GatherShade(Vector3[] shaded)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			int result = 0;
			for (int i = 0; i < SunPointCount; i++)
			{
				if (!sunLit[i])
				{
					shaded[result++] = sunPoints[i];
				}
			}
			return result;
		}

		public void EnterPool()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			Job.Complete();
			Job = default;
			commands.SafeDispose<RaycastCommand>();
			hits.SafeDispose<RaycastHit>();
			SunPointCount = 0;
			hourCount = 0;
			probeCount = 0;
			areaScale = 0f;
			GrassArea = 0f;
		}

		public void LeavePool()
		{
		}
	}

	[BurstCompile(CompileSynchronously = true)]
	private struct AimJob : IJob
	{
		[WriteOnly]
		public NativeArray<RaycastCommand> Commands;

		public PhysicsScene Scene;

		public QueryParameters GrassQuery;

		public QueryParameters SunQuery;

		[NativeDisableUnsafePtrRestriction]
		public unsafe Vector3* Samples;

		[NativeDisableUnsafePtrRestriction]
		public unsafe bool* Grass;

		public int SampleCount;

		[NativeDisableUnsafePtrRestriction]
		public unsafe Vector3* SunPoints;

		public int SunPointCount;

		[NativeDisableUnsafePtrRestriction]
		public unsafe Vector3* Directions;

		public int HourCount;

		public unsafe void Execute()
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			int num = 0;
			for (int i = 0; i < SampleCount; i++)
			{
				if (Grass[i])
				{
					Commands[num++] = new RaycastCommand(Scene, Samples[i] + Vector3.up * 1f, Vector3.down, GrassQuery, 1.3f);
				}
			}
			for (int j = 0; j < SunPointCount; j++)
			{
				Vector3 val = SunPoints[j] + Vector3.up * 0.5f;
				for (int k = 0; k < HourCount; k++)
				{
					Commands[num++] = new RaycastCommand(Scene, val, Directions[k], SunQuery, 200f);
				}
			}
		}
	}

	[BurstCompile(CompileSynchronously = true)]
	private struct FoldJob : IJob
	{
		[ReadOnly]
		public NativeArray<RaycastHit> Hits;

		public int Probes;

		public int SunPointCount;

		public int HourCount;

		[NativeDisableUnsafePtrRestriction]
		public unsafe bool* SunLit;

		[NativeDisableUnsafePtrRestriction]
		public unsafe int* Open;

		public unsafe void Execute()
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			int num = 0;
			RaycastHit val;
			for (int i = 0; i < Probes; i++)
			{
				val = Hits[i];
				if (val.colliderInstanceID == 0)
				{
					num++;
				}
			}
			*Open = num;
			for (int j = 0; j < SunPointCount; j++)
			{
				int num2 = Probes + j * HourCount;
				bool flag = false;
				for (int k = 0; k < HourCount; k++)
				{
					if (flag)
					{
						break;
					}
					val = Hits[num2 + k];
					flag = val.colliderInstanceID == 0;
				}
				SunLit[j] = flag;
			}
		}
	}

	public const float FieldArea = 3000f;

	private const float ClaimMargin = 2f;

	private const int GridSide = 16;

	private const int MaxSamples = 256;

	private const float MinGridSpacing = 0.75f;

	private const float MinSampleHeight = 2f;

	private const int RaysPerJob = 32;

	private const float LightBoundsMargin = 2f;

	private const float LightCheckSeconds = 4f;

	private const float SupplyRefreshSeconds = 60f;

	private const float SupplyReachMargin = 25f;

	private const float ShoreRingMaxRadius = 60f;

	private const int ShoreRingSamples = 16;

	private const float ShoreMemorySeconds = 600f;

	private static readonly Vector3 HoldsPositionExtents = new Vector3(2f, 2f, 2f);

	private static float[] sunHours = new float[0];

	private static Vector3[] sunDirections = new Vector3[0];

	private static OBB[] litLights = new OBB[16];

	private static readonly QueryParameters GrassProbeQuery = new QueryParameters(2097408, false, (QueryTriggerInteraction)1, false);

	private static readonly QueryParameters SunRayQuery = new QueryParameters(144769025, false, (QueryTriggerInteraction)1, false);

	private readonly ulong[] refs = new ulong[2048];

	private readonly uint[] tiles = new uint[256];

	private readonly List<LivestockAnimal> members = new List<LivestockAnimal>();

	private readonly List<HitchTrough> troughs = new List<HitchTrough>();

	private readonly List<LiquidContainer> containers = new List<LiquidContainer>();

	private RustNavmesh navmesh;

	private int refCount;

	private int tileCount;

	private uint stamp;

	private int seenVersion;

	private bool stale;

	private Vector3 centre;

	private float radius;

	private RayBatch rays;

	private readonly Vector3[] floorSamples = new Vector3[256];

	private readonly bool[] floorGrass = new bool[256];

	private int floorSampleCount;

	private readonly Vector3[] shadedPoints = new Vector3[256];

	private int shadedCount;

	private Bounds shadedBounds;

	private int sunPointCount;

	private int lightChangesSeen;

	private TimeSince sinceLightsChecked;

	private readonly List<SeenLight> seenLights = new List<SeenLight>();

	private TimeSince sinceClassified;

	private SupplyEstimate supply;

	private bool supplyDirty = true;

	private RealTimeSince sinceSupplyMeasured;

	private RealTimeSince sinceShoreNoted;

	private bool shoreNoted;

	public float Area { get; private set; }

	public float GrassArea { get; private set; }

	public float SunShare { get; private set; } = 1f;

	public bool Classified { get; private set; }

	public bool SunPending => rays != null;

	public bool IsField { get; private set; }

	public Vector3 Centre
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return centre;
		}
	}

	public static float BreedingFloorPerHead => Mathf.Max(Livestock.lethalFloorPerHead + 0.01f, Livestock.breedingFloorPerHead);

	public float Heads
	{
		get
		{
			float num = 0f;
			foreach (LivestockAnimal member in members)
			{
				num += member.NeedScale;
			}
			return num;
		}
	}

	public float ProjectedHeads
	{
		get
		{
			float num = Heads;
			foreach (LivestockAnimal member in members)
			{
				if (member.IsPregnant())
				{
					num += member.LitterHeads;
				}
			}
			return num;
		}
	}

	public static float StandingSecondsPerRealSecond => Livestock.estimateStandingShare * LivestockSunPath.RealDaylightShare();

	public bool HasAdultOtherThan(LivestockAnimal self, LivestockAnimal.Personality personality, bool onlyLowerId)
	{
		foreach (LivestockAnimal member in members)
		{
			if (!((Object)(object)member == (Object)null) && member != self && !member.IsDead() && member.ActsAsAdult(personality) && member.IsSameSpeciesAs(self) && (!onlyLowerId || member.net.ID.Value < self.net.ID.Value))
			{
				return true;
			}
		}
		return false;
	}

	public static LivestockFloor Flood(RustNavMeshAgent agent, ulong startRef, LivestockFloor held)
	{
		LivestockFloor livestockFloor = FindFloor(agent, startRef);
		if (held != null && held.RenewFrom(livestockFloor))
		{
			livestockFloor = held;
		}
		livestockFloor?.Classify(agent);
		return livestockFloor;
	}

	private static LivestockFloor FindFloor(RustNavMeshAgent agent, ulong startRef)
	{
		using (LivestockProfiler.Sample(LivestockProfiler.Section.Floor))
		{
			LivestockFloor livestockFloor = Pool.Get<LivestockFloor>();
			if (!agent.FindFloor(startRef, 3000f, livestockFloor.refs, out livestockFloor.refCount, livestockFloor.tiles, out livestockFloor.tileCount, out var area, out livestockFloor.centre, out livestockFloor.radius, out var open, out livestockFloor.stamp))
			{
				Pool.Free<LivestockFloor>(ref livestockFloor);
				return null;
			}
			livestockFloor.Area = area;
			livestockFloor.GrassArea = area;
			livestockFloor.IsField = open;
			livestockFloor.seenVersion = agent.NavmeshChangeVersion;
			livestockFloor.navmesh = agent.BoundNavmesh;
			return livestockFloor;
		}
	}

	public void Refresh()
	{
		TryCompleteRays();
		RelightIfLightsChanged();
	}

	private void TryCompleteRays()
	{
		if (rays == null || !rays.Job.IsCompleted)
		{
			return;
		}
		using (LivestockProfiler.Sample(LivestockProfiler.Section.Classify))
		{
			rays.Job.Complete();
			rays.Land();
			GrassArea = Mathf.Min(rays.GrassArea, Area);
			sunPointCount = rays.SunPointCount;
			shadedCount = rays.GatherShade(shadedPoints);
			EncloseShadedPoints();
			CountLitSunPoints();
			Classified = true;
			Pool.Free<RayBatch>(ref rays);
		}
	}

	private void RelightIfLightsChanged()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (shadedCount != 0 && !(TimeSince.op_Implicit(sinceLightsChecked) < 4f))
		{
			sinceLightsChecked = TimeSince.op_Implicit(0f);
			if (LightChangesAround() != lightChangesSeen || AnySeenLightSwitched())
			{
				CountLitSunPoints();
			}
		}
	}

	private bool AnySeenLightSwitched()
	{
		foreach (SeenLight seenLight in seenLights)
		{
			if (seenLight.Light.IsOn() != seenLight.On)
			{
				return true;
			}
		}
		return false;
	}

	private int LightChangesAround()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Vector3 center = shadedBounds.center;
		return CeilingLight.FarmLightGrid.CountChanges(center.x, center.z, LightReach());
	}

	private float LightReach()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Max(shadedBounds.extents.x, shadedBounds.extents.z) + ConVar.Server.ceilingLightGrowableRange + 2f;
	}

	private void CountLitSunPoints()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		seenLights.Clear();
		sinceLightsChecked = TimeSince.op_Implicit(0f);
		int num = sunPointCount - shadedCount;
		if (shadedCount > 0)
		{
			lightChangesSeen = LightChangesAround();
			int num2 = GatherLights();
			if (num2 > 0)
			{
				num += CountLitShade(num2);
			}
		}
		SunShare = ((sunPointCount > 0) ? ((float)num / (float)sunPointCount) : 1f);
	}

	private unsafe int CountLitShade(int lightCount)
	{
		int result = 0;
		fixed (Vector3* points = shadedPoints)
		{
			fixed (OBB* lights = litLights)
			{
				IJobExtensions.Run<LampJob>(new LampJob
				{
					Points = points,
					PointCount = shadedCount,
					Lights = lights,
					LightCount = lightCount,
					Range = ConVar.Server.ceilingLightGrowableRange,
					Lit = &result
				});
			}
		}
		return result;
	}

	private int GatherLights()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		float ceilingLightGrowableRange = ConVar.Server.ceilingLightGrowableRange;
		Vector3 center = shadedBounds.center;
		PooledList<CeilingLight> val = Pool.Get<PooledList<CeilingLight>>();
		try
		{
			CeilingLight.FarmLightGrid.Query(center.x, center.z, LightReach(), (List<CeilingLight>)(object)val);
			Vector3 min = shadedBounds.min;
			Vector3 max = shadedBounds.max;
			int num = 0;
			foreach (CeilingLight item in (List<CeilingLight>)(object)val)
			{
				OBB lightBounds = item.LightBounds;
				if (!CouldLightShade(lightBounds, min, max, ceilingLightGrowableRange))
				{
					continue;
				}
				bool flag = item.IsOn();
				seenLights.Add(new SeenLight
				{
					Light = item,
					On = flag
				});
				if (flag)
				{
					if (num == litLights.Length)
					{
						Array.Resize(ref litLights, litLights.Length * 2);
					}
					litLights[num++] = lightBounds;
				}
			}
			return num;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static bool CouldLightShade(OBB light, Vector3 low, Vector3 high, float range)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		float num = range + light.extents.magnitude;
		Vector3 position = light.position;
		if (position.x >= low.x - num && position.x <= high.x + num && position.y >= low.y - num && position.y <= high.y + num && position.z >= low.z - num)
		{
			return position.z <= high.z + num;
		}
		return false;
	}

	private void EncloseShadedPoints()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		shadedBounds = ((shadedCount > 0) ? new Bounds(shadedPoints[0], Vector3.zero) : default(Bounds));
		for (int i = 1; i < shadedCount; i++)
		{
			shadedBounds.Encapsulate(shadedPoints[i]);
		}
	}

	private void ForgetSunPoints()
	{
		shadedCount = 0;
		seenLights.Clear();
		sunPointCount = 0;
		SunShare = 1f;
	}

	public void ResampleSunIfDue(RustNavMeshAgent agent)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)agent != (Object)null && rays == null && Livestock.sunResampleMinutes > 0f && TimeSince.op_Implicit(sinceClassified) >= Livestock.sunResampleMinutes * 60f)
		{
			Classify(agent);
		}
	}

	private void Classify(RustNavMeshAgent agent)
	{
		using (LivestockProfiler.Sample(LivestockProfiler.Section.Classify))
		{
			if (floorSampleCount == 0)
			{
				floorSampleCount = SampleGrid(agent);
			}
			Measure();
		}
	}

	private void Measure()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		CancelRays();
		sinceClassified = TimeSince.op_Implicit(0f);
		int num = floorSampleCount;
		if (num == 0)
		{
			Classified = false;
			GrassArea = Area;
			ForgetSunPoints();
			return;
		}
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (floorGrass[i])
			{
				num2++;
			}
		}
		int num3 = Mathf.Clamp(Livestock.sunSamplePolys, 0, num);
		int num4 = ((num3 > 0) ? Mathf.Max(1, Livestock.sunSampleHours) : 0);
		if (num2 + num3 * num4 == 0)
		{
			Classified = true;
			GrassArea = 0f;
			ForgetSunPoints();
		}
		else
		{
			SunDirectionsAcrossTheDay(num4);
			rays = Pool.Get<RayBatch>();
			rays.Begin(floorSamples, floorGrass, num, num2, num3, num4, Area / (float)num);
			rays.Schedule();
		}
	}

	private unsafe int SampleGrid(RustNavMeshAgent agent)
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		if (navmesh == null || !navmesh.IsValid() || radius <= 0f)
		{
			return 0;
		}
		float num = Mathf.Max(0.75f, radius * 2f / 16f);
		float num2 = num * 0.5f;
		bool hasValidIndependantNavmesh = agent.HasValidIndependantNavmesh;
		int result = 0;
		fixed (ulong* ptr = refs)
		{
			fixed (Vector3* samples = floorSamples)
			{
				fixed (bool* grass = floorGrass)
				{
					IJobExtensions.Run<GridJob>(new GridJob
					{
						Navmesh = navmesh.NavMeshHandle,
						WorldToNav = (hasValidIndependantNavmesh ? agent.independantNavmesh.WorldToNavMatrix : Matrix4x4.identity),
						NavToWorld = (hasValidIndependantNavmesh ? agent.independantNavmesh.NavToWorldMatrix : Matrix4x4.identity),
						Corner = centre - new Vector3(num * 15f * 0.5f, 0f, num * 15f * 0.5f),
						Extents = new Vector3(num2, Mathf.Max(2f, radius), num2),
						Spacing = num,
						Half = num2,
						Refs = ptr,
						RefCount = refCount,
						Ground = LivestockGrass.Ground.Read(),
						Samples = samples,
						Grass = grass,
						Sampled = &result
					});
				}
			}
		}
		return result;
	}

	private static void SunDirectionsAcrossTheDay(int hours)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (sunHours.Length < hours)
		{
			sunHours = new float[hours];
			sunDirections = new Vector3[hours];
		}
		LivestockSunPath.DaylightHours(hours, sunHours);
		for (int i = 0; i < hours; i++)
		{
			sunDirections[i] = LivestockSunPath.DirectionAt(sunHours[i]);
		}
	}

	private void CancelRays()
	{
		if (rays != null)
		{
			Pool.Free<RayBatch>(ref rays);
		}
	}

	public SupplyEstimate Supply()
	{
		MeasureSupplyIfDue();
		return supply;
	}

	public void TroughsWithin(Vector3 position, float range, List<HitchTrough> results)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		MeasureSupplyIfDue();
		Within(troughs, position, range, results);
	}

	public void ContainersWithin(Vector3 position, float range, List<LiquidContainer> results)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		MeasureSupplyIfDue();
		Within(containers, position, range, results);
	}

	private static void Within<T>(List<T> onFloor, Vector3 position, float range, List<T> results) where T : BaseCombatEntity
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		float num = range * range;
		foreach (T item in onFloor)
		{
			if (IsIntact(item))
			{
				Vector3 val = ((Component)item).transform.position - position;
				if (val.sqrMagnitude <= num)
				{
					results.Add(item);
				}
			}
		}
	}

	private void MeasureSupplyIfDue()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (supplyDirty || !(RealTimeSince.op_Implicit(sinceSupplyMeasured) < 60f))
		{
			GatherOnFloor(troughs, LivestockFeedingSlots.Serves.Food);
			GatherOnFloor(containers, LivestockFeedingSlots.Serves.Water);
			supply = MeasureSupply();
			supplyDirty = false;
			sinceSupplyMeasured = RealTimeSince.op_Implicit(0f);
		}
	}

	private void GatherOnFloor<T>(List<T> onFloor, LivestockFeedingSlots.Serves serves) where T : BaseCombatEntity
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		onFloor.Clear();
		PooledList<T> val = Pool.Get<PooledList<T>>();
		try
		{
			BaseEntity.Query.Server.GetInSphere(centre, radius + 25f, (List<T>)(object)val);
			LivestockAnimal member = MemberOnThisNavmesh();
			foreach (T item in (List<T>)(object)val)
			{
				if (IsIntact(item) && HoldsDeployable(item, serves, member))
				{
					onFloor.Add(item);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private bool HoldsDeployable(BaseCombatEntity deployable, LivestockFeedingSlots.Serves serves, LivestockAnimal member)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (HoldsPosition(((Component)deployable).transform.position))
		{
			return true;
		}
		if ((Object)(object)member == (Object)null)
		{
			return false;
		}
		BaseEntity owner = LivestockFeedingSlots.OwnerOf(deployable);
		PooledList<LivestockFeedingSlots.Place> val = Pool.Get<PooledList<LivestockFeedingSlots.Place>>();
		try
		{
			LivestockFeedingSlots.Gather(owner, serves, val);
			foreach (LivestockFeedingSlots.Place item in (List<LivestockFeedingSlots.Place>)(object)val)
			{
				LivestockFeedingSlots.Place place = item;
				if (LivestockFeedingSlots.TryStandAt(member.NavAgent, member, in place, out var standing) && HoldsPosition(standing) && LivestockFeedingSlots.CanReachInFrom(owner, standing, place.Nose))
				{
					return true;
				}
			}
			return false;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void MarkSupplyDirty()
	{
		supplyDirty = true;
	}

	public void NoteShore(Vector3 position)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (IsField || HoldsPosition(position))
		{
			shoreNoted = true;
			sinceShoreNoted = RealTimeSince.op_Implicit(0f);
		}
	}

	public bool HoldsPosition(Vector3 world)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (navmesh == null || !navmesh.IsValid())
		{
			return false;
		}
		if (navmesh.SamplePositionPoly(ToNavSpace(world), out var _, HoldsPositionExtents, out var nearestPolyRef))
		{
			return Holds(nearestPolyRef);
		}
		return false;
	}

	public bool Reaches(HitchTrough trough)
	{
		MeasureSupplyIfDue();
		return EatsAt(trough);
	}

	private static bool IsIntact(BaseCombatEntity deployable)
	{
		if ((Object)(object)deployable != (Object)null && !deployable.IsDestroyed)
		{
			return !deployable.IsDead();
		}
		return false;
	}

	private SupplyEstimate MeasureSupply()
	{
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		SupplyEstimate result = default;
		if (members.Count == 0)
		{
			return result;
		}
		float num = StandingSecondsPerRealSecond * 60f;
		float num2 = 0f;
		foreach (LivestockAnimal member in members)
		{
			num2 += member.DrainPerSecond(LivestockAnimal.Need.Hydration) * (float)member.BarrelWaterPerDrink * num;
		}
		PooledList<LivestockFloor> val = Pool.Get<PooledList<LivestockFloor>>();
		try
		{
			PooledList<HitchTrough> val2 = Pool.Get<PooledList<HitchTrough>>();
			try
			{
				GatherFeedingPool((List<LivestockFloor>)(object)val, (List<HitchTrough>)(object)val2);
				float num3 = 0f;
				float num4 = 0f;
				int num5 = 0;
				foreach (LivestockFloor item in (List<LivestockFloor>)(object)val)
				{
					result.Heads += item.members.Count;
					foreach (LivestockAnimal member2 in item.members)
					{
						num4 += member2.TroughCaloriesPerSecond * num;
					}
				}
				foreach (HitchTrough item2 in (List<HitchTrough>)(object)val2)
				{
					result.Troughs++;
					num3 += LivestockAnimal.TroughCaloriesIn(item2);
					if ((Object)(object)result.FoodItem == (Object)null)
					{
						result.FoodItem = item2.GetLivestockFoodItem()?.info;
					}
					num4 += (float)HitchedHorses(item2) * 60f / Mathf.Max(1f, item2.caloriesToDecaySeconds);
				}
				result.FoodMinutes = ((num4 > 0f) ? (num3 / num4) : 0f);
				result.ShoreInReach = FindsShore();
				if (result.ShoreInReach)
				{
					result.WaterMinutes = -1f;
					return result;
				}
				foreach (LiquidContainer container in containers)
				{
					if (AnyMemberWithinReachOf(((Component)container).transform.position))
					{
						num5 += LivestockAnimal.FreshWaterIn(container);
					}
				}
				result.WaterMinutes = ((num2 > 0f) ? ((float)num5 / num2) : 0f);
				return result;
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

	private void GatherFeedingPool(List<LivestockFloor> floors, List<HitchTrough> poolTroughs)
	{
		floors.Add(this);
		for (int i = 0; i < floors.Count; i++)
		{
			LivestockFloor livestockFloor = floors[i];
			foreach (HitchTrough trough in livestockFloor.troughs)
			{
				if (!poolTroughs.Contains(trough) && livestockFloor.EatsAt(trough))
				{
					poolTroughs.Add(trough);
					AddFloorsEatingAt(trough, floors);
				}
			}
		}
	}

	private static void AddFloorsEatingAt(HitchTrough trough, List<LivestockFloor> floors)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			BaseEntity.Query.Server.GetBrainsInSphere(((Component)trough).transform.position, 27f, (List<LivestockAnimal>)(object)val);
			foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
			{
				LivestockFloor floor = item.Floor;
				if (floor != null && !floors.Contains(floor) && floor.EatsAt(trough))
				{
					floors.Add(floor);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private bool EatsAt(HitchTrough trough)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (IsIntact(trough) && troughs.Contains(trough))
		{
			return AnyMemberWithinReachOf(((Component)trough).transform.position);
		}
		return false;
	}

	private bool AnyMemberWithinReachOf(Vector3 position)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		foreach (LivestockAnimal member in members)
		{
			float consumeSearchRadius = member.ConsumeSearchRadius;
			Vector3 val = ((Component)member).transform.position - position;
			if (val.sqrMagnitude <= consumeSearchRadius * consumeSearchRadius)
			{
				return true;
			}
		}
		return false;
	}

	private static int HitchedHorses(HitchTrough trough)
	{
		int num = 0;
		if (trough.hitchSpots == null)
		{
			return num;
		}
		HitchTrough.HitchSpot[] hitchSpots = trough.hitchSpots;
		foreach (HitchTrough.HitchSpot hitchSpot in hitchSpots)
		{
			if (hitchSpot != null && hitchSpot.IsOccupied())
			{
				num++;
			}
		}
		return num;
	}

	private bool FindsShore()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if (shoreNoted && RealTimeSince.op_Implicit(sinceShoreNoted) < 600f)
		{
			return true;
		}
		if (!IsField || (Object)(object)TerrainMeta.HeightMap == (Object)null)
		{
			return false;
		}
		float num = Mathf.Min(radius + 25f, 60f);
		for (int i = 0; i < 16; i++)
		{
			float num2 = (float)i * MathF.PI * 2f / 16f;
			Vector3 val = centre + new Vector3(Mathf.Cos(num2), 0f, Mathf.Sin(num2)) * num;
			val.y = TerrainMeta.HeightMap.GetHeight(val);
			if (members[0].IsDrinkableShore(val))
			{
				return true;
			}
		}
		return false;
	}

	private NavVector3 ToNavSpace(Vector3 world)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		LivestockAnimal livestockAnimal = MemberOnThisNavmesh();
		if (!((Object)(object)livestockAnimal != (Object)null))
		{
			return new NavVector3(world);
		}
		return livestockAnimal.NavAgent.WorldToNavSpace(world);
	}

	private LivestockAnimal MemberOnThisNavmesh()
	{
		foreach (LivestockAnimal member in members)
		{
			RustNavMeshAgent navAgent = member.NavAgent;
			if ((Object)(object)navAgent != (Object)null && navAgent.BoundNavmesh == navmesh)
			{
				return member;
			}
		}
		return null;
	}

	public bool Holds(ulong polyRef)
	{
		if (polyRef != 0L)
		{
			return Array.BinarySearch(refs, 0, refCount, polyRef) >= 0;
		}
		return false;
	}

	public bool IsCurrent(RustNavMeshAgent agent)
	{
		if (stale)
		{
			return false;
		}
		if (navmesh == null || !navmesh.IsValid())
		{
			stale = true;
			return false;
		}
		if (agent.BoundNavmesh != navmesh)
		{
			return false;
		}
		int navmeshChangeVersion = agent.NavmeshChangeVersion;
		if (navmeshChangeVersion == seenVersion)
		{
			return true;
		}
		if (tileCount < 0 || !agent.FloorUnchangedSince(tiles, tileCount, stamp))
		{
			stale = true;
			return false;
		}
		seenVersion = navmeshChangeVersion;
		return true;
	}

	public bool Serves(ulong polyRef, RustNavMeshAgent agent)
	{
		if (Holds(polyRef))
		{
			return IsCurrent(agent);
		}
		return false;
	}

	public void ClaimStandingAnimals()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			BaseEntity.Query.Server.GetBrainsInSphere(centre, radius + 2f, (List<LivestockAnimal>)(object)val);
			foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
			{
				if ((Object)(object)item == (Object)null || !item.CanStandOnAFloor)
				{
					continue;
				}
				RustNavMeshAgent navAgent = item.NavAgent;
				if ((Object)(object)navAgent == (Object)null || navAgent.BoundNavmesh != navmesh)
				{
					continue;
				}
				LivestockFloor floor = item.Floor;
				if (floor != this)
				{
					ulong currentPolyRef = navAgent.CurrentPolyRef;
					bool flag = Holds(currentPolyRef);
					if ((flag || IsAnotherFloodOfThisPen(floor, navAgent)) && (!flag || floor == null || (!IsField && !floor.IsField) || !floor.Serves(currentPolyRef, navAgent)))
					{
						item.StandOn(this);
					}
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private bool RenewFrom(LivestockFloor fresh)
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		if (fresh == null || IsField || fresh.IsField || fresh.navmesh != navmesh || fresh.refCount != refCount)
		{
			return false;
		}
		for (int i = 0; i < refCount; i++)
		{
			if (refs[i] != fresh.refs[i])
			{
				return false;
			}
		}
		if (fresh.tileCount > 0)
		{
			Array.Copy(fresh.tiles, tiles, fresh.tileCount);
		}
		tileCount = fresh.tileCount;
		stamp = fresh.stamp;
		seenVersion = fresh.seenVersion;
		Area = fresh.Area;
		centre = fresh.centre;
		radius = fresh.radius;
		stale = false;
		supplyDirty = true;
		Pool.Free<LivestockFloor>(ref fresh);
		return true;
	}

	public void GetBorder(RustNavMeshAgent agent, List<Vector3> segments)
	{
		if ((Object)(object)agent != (Object)null)
		{
			agent.FloorBorder(refs, refCount, segments);
		}
	}

	public void GetContents(List<LivestockAnimal> animals, List<BaseCombatEntity> deployables)
	{
		MeasureSupplyIfDue();
		animals.AddRange(members);
		foreach (HitchTrough trough in troughs)
		{
			if (IsIntact(trough))
			{
				deployables.Add(trough);
			}
		}
		foreach (LiquidContainer container in containers)
		{
			if (IsIntact(container))
			{
				deployables.Add(container);
			}
		}
	}

	private bool IsAnotherFloodOfThisPen(LivestockFloor other, RustNavMeshAgent agent)
	{
		if (other != null && !IsField && !other.IsField && other.Holds(refs[0]))
		{
			return other.IsCurrent(agent);
		}
		return false;
	}

	internal void Join(LivestockAnimal animal)
	{
		members.Add(animal);
		supplyDirty = true;
	}

	internal void Leave(LivestockAnimal animal)
	{
		supplyDirty = true;
		for (int i = 0; i < members.Count; i++)
		{
			if (members[i] == animal)
			{
				members[i] = members[members.Count - 1];
				members.RemoveAt(members.Count - 1);
				break;
			}
		}
		if (members.Count <= 0)
		{
			LivestockFloor livestockFloor = this;
			Pool.Free<LivestockFloor>(ref livestockFloor);
		}
	}

	public void EnterPool()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		members.Clear();
		navmesh = null;
		floorSampleCount = 0;
		refCount = 0;
		tileCount = 0;
		stamp = 0u;
		seenVersion = 0;
		stale = false;
		centre = default;
		radius = 0f;
		Area = 0f;
		IsField = false;
		CancelRays();
		GrassArea = 0f;
		ForgetSunPoints();
		Classified = false;
		supply = default;
		supplyDirty = true;
		shoreNoted = false;
		troughs.Clear();
		containers.Clear();
	}

	public void LeavePool()
	{
	}

	static LivestockFloor()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
	}
}

using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class CrabGrouping : IDisposable
{
	[Serializable]
	public struct CrabType
	{
		public Mesh mesh;

		public Material material;

		[Header("Population")]
		public int castsPerFrame;

		public int maxCount;

		[Tooltip("How far from the entity the crabs spread out. They turn back when they reach the edge rather than despawning.")]
		public float swarmRadius;

		[Header("Movement")]
		[Tooltip("Speed while wandering around.")]
		public float minSpeed;

		[Tooltip("Speed while startled, ie when the player gets close.")]
		public float maxSpeed;

		[Tooltip("Turn rate while wandering.")]
		public float minTurnSpeed;

		[Tooltip("Turn rate while startled.")]
		public float maxTurnSpeed;

		[Tooltip("How much a crab's size affects its speed. 0 means every crab moves alike, 1 means speed scales straight with size, so the biggest crab outpaces the smallest by the same ratio as their scales. Above 1 exaggerates it further. Keeps a catching up swarm staggered instead of forming a line.")]
		public float speedScaleInfluence;

		[Tooltip("Random speed spread per crab, on top of size. 0.25 means each crab is up to 25 percent faster or slower than its size alone suggests, so same sized crabs still differ.")]
		public float speedRandomness;

		[Header("Following")]
		[Tooltip("How close to the entity each crab wants to stay, as a fraction of the swarm radius. Rolled per crab, so some shadow the entity closely while others hang back near the edge.")]
		public Vector2 followTightnessRange;

		[Tooltip("Random variation in how long a crab dawdles before setting off after the entity, in seconds. Stops the swarm surging as one block.")]
		public Vector2 catchUpDelayRange;

		[Header("Wandering")]
		[Tooltip("How far a crab travels in one trip, as a fraction of the swarm radius. Picked randomly per trip.")]
		public Vector2 wanderDistanceRange;

		[Tooltip("How long a crab sits still after finishing a trip, in seconds. Higher means a calmer swarm.")]
		public Vector2 restDurationRange;

		[Tooltip("Give up on a destination after this long, so a stuck crab doesn't walk into a wall forever.")]
		public float wanderTimeout;

		[Tooltip("Tallest step a crab will walk up. Crabs look ahead at this height and steer around anything they'd hit, so rocks and walls block them.")]
		public float maxStepHeight;

		[Header("Startle")]
		[Tooltip("How long a crab stays spooked after a player gets close, in seconds.")]
		public float startleDuration;

		[Tooltip("A player has to get this close to startle a crab.")]
		public float startleRange;

		[Tooltip("How far a fleeing crab may stray outside the swarm radius, as a multiplier. It wanders back in once it calms down.")]
		public float startleBoundsMultiplier;

		[Header("Animation")]
		[Tooltip("Speed at which the walk animation reaches full blend. Below this the legs scale down, at 0 speed the crab is still.")]
		public float fullAnimationSpeed;

		[Tooltip("How quickly the walk animation blends in and out as the crab starts and stops.")]
		public float animationBlendRate;

		[Header("Scale")]
		public float minScale;

		public float maxScale;
	}

	public struct CrabData
	{
		public bool isAlive;

		public float updateTime;

		public float startleTime;

		public float destinationX;

		public float destinationZ;

		public float directionX;

		public float directionZ;

		public float speed;

		public float scale;

		public float speedScale;

		public float followTightness;

		public float catchUpDelay;

		public float restTime;

		public float groundY;

		public float groundNormalY;

		public float avoidTime;
	}

	public struct CrabRenderData
	{
		public float3 position;

		public float rotation;

		public float scale;

		public float distance;

		public float seed;

		public float moveToggle;

		public float legPhase;
	}

	public struct CrabGroundGatherJob : IJob
	{
		public int castCursor;

		public int castCount;

		public int crabCount;

		public float maxStepHeight;

		public NativeArray<RaycastCommand> castCommands;

		public NativeArray<RaycastCommand> obstacleCommands;

		public NativeArray<Vector3> castPositions;

		public NativeArray<int> crabCastIndices;

		[ReadOnly]
		public NativeArray<CrabData> crabDataArray;

		[ReadOnly]
		public NativeArray<CrabRenderData> crabRenderDataArray;

		public void Execute()
		{
		}
	}

	public struct CrabGroundProcessJob : IJob
	{
		public int castCount;

		public bool hasFallbackY;

		public NativeArray<CrabData> crabDataArray;

		[ReadOnly]
		public NativeArray<RaycastHit> castResults;

		[ReadOnly]
		public NativeArray<int> crabCastIndices;

		[ReadOnly]
		public NativeArray<float> crabCastFallbackY;

		public void Execute()
		{
		}
	}

	public struct CrabObstacleProcessJob : IJob
	{
		public int castCount;

		public NativeArray<CrabData> crabDataArray;

		[ReadOnly]
		public NativeArray<RaycastHit> obstacleResults;

		[ReadOnly]
		public NativeArray<int> crabCastIndices;

		[ReadOnly]
		public NativeArray<CrabRenderData> crabRenderDataArray;

		public void Execute()
		{
		}
	}

	public struct CrabUpdateJob : IJobParallelFor
	{
		[ReadOnly]
		public float3 swarmCenter;

		[ReadOnly]
		public int startleCount;

		[ReadOnly]
		public uint seed;

		[ReadOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<float3> startlePositions;

		[ReadOnly]
		public float dt;

		[ReadOnly]
		public float swarmRadius;

		[ReadOnly]
		public float minSpeed;

		[ReadOnly]
		public float maxSpeed;

		[ReadOnly]
		public float minTurnSpeed;

		[ReadOnly]
		public float maxTurnSpeed;

		[ReadOnly]
		public float2 wanderDistanceRange;

		[ReadOnly]
		public float2 restDurationRange;

		[ReadOnly]
		public float2 catchUpDelayRange;

		[ReadOnly]
		public float wanderTimeout;

		[ReadOnly]
		public float startleDuration;

		[ReadOnly]
		public float startleRangeSq;

		[ReadOnly]
		public float startleBoundsMultiplier;

		[ReadOnly]
		public float fullAnimationSpeed;

		[ReadOnly]
		public float animationBlendRate;

		[ReadOnly]
		public CritterLegSpeed legSpeed;

		[NativeDisableUnsafePtrRestriction]
		public unsafe CrabData* crabDataArray;

		[NativeDisableUnsafePtrRestriction]
		public unsafe CrabRenderData* crabRenderDataArray;

		public void Execute(int i)
		{
		}
	}

	public struct KillCrab : IJob
	{
		public NativeArray<CrabData> crabDataArray;

		public NativeArray<CrabRenderData> crabRenderDataArray;

		public NativeArray<int> crabCount;

		public void Execute()
		{
		}
	}

	private const float groundProbeHeight = 1f;

	private const float groundProbeDepth = 8f;

	private const float maxWalkableNormalY = 0.6f;

	private const float obstacleProbeDistance = 1f;

	private const float obstacleSlideDistance = 2f;

	private const float obstacleAvoidDuration = 1f;

	private const int maxStartlePlayers = 8;

	private CrabType crabType;

	private JobHandle jobHandle;

	private int castCursor;

	private NativeArray<RaycastCommand> castCommands;

	private NativeArray<RaycastHit> castResults;

	private NativeArray<RaycastCommand> obstacleCommands;

	private NativeArray<RaycastHit> obstacleResults;

	private NativeArray<Vector3> castPositions;

	private NativeArray<int> crabCastIndices;

	private NativeArray<float> crabCastFallbackY;

	private NativeArray<CrabData> crabData;

	private NativeArray<CrabRenderData> crabRenderData;

	private NativeArray<int> crabCount;

	private NativeArray<float3> startlePositions;

	private int startleCount;

	private MaterialPropertyBlock materialPropertyBlock;

	private ComputeBuffer crabBuffer;

	private bool renderDataDirty;

	public float SwarmRadius => crabType.swarmRadius;

	public int Count => crabCount[0];

	public CrabGrouping(CrabType crabType)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected Obj, but got Unknown
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected Obj, but got Unknown
		this.crabType = crabType;
		castCommands = new NativeArray<RaycastCommand>(crabType.castsPerFrame, (Allocator)4, (NativeArrayOptions)1);
		castResults = new NativeArray<RaycastHit>(crabType.castsPerFrame, (Allocator)4, (NativeArrayOptions)1);
		obstacleCommands = new NativeArray<RaycastCommand>(crabType.castsPerFrame, (Allocator)4, (NativeArrayOptions)1);
		obstacleResults = new NativeArray<RaycastHit>(crabType.castsPerFrame, (Allocator)4, (NativeArrayOptions)1);
		castPositions = new NativeArray<Vector3>(crabType.castsPerFrame, (Allocator)4, (NativeArrayOptions)1);
		crabCastIndices = new NativeArray<int>(crabType.castsPerFrame, (Allocator)4, (NativeArrayOptions)1);
		crabCastFallbackY = new NativeArray<float>(crabType.castsPerFrame, (Allocator)4, (NativeArrayOptions)1);
		crabData = new NativeArray<CrabData>(crabType.maxCount, (Allocator)4, (NativeArrayOptions)1);
		crabRenderData = new NativeArray<CrabRenderData>(crabType.maxCount, (Allocator)4, (NativeArrayOptions)1);
		crabCount = new NativeArray<int>(1, (Allocator)4, (NativeArrayOptions)1);
		startlePositions = new NativeArray<float3>(8, (Allocator)4, (NativeArrayOptions)1);
		crabBuffer = new ComputeBuffer(crabType.maxCount, UnsafeUtility.SizeOf<CrabRenderData>());
		materialPropertyBlock = new MaterialPropertyBlock();
		materialPropertyBlock.SetBuffer("_FishData", crabBuffer);
	}

	public void Dispose()
	{
		jobHandle.Complete();
		castCommands.Dispose();
		castResults.Dispose();
		obstacleCommands.Dispose();
		obstacleResults.Dispose();
		castPositions.Dispose();
		crabCastIndices.Dispose();
		crabCastFallbackY.Dispose();
		crabData.Dispose();
		crabRenderData.Dispose();
		crabCount.Dispose();
		startlePositions.Dispose();
		crabBuffer.Dispose();
	}
}

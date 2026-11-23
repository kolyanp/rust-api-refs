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

		[Tooltip("How long a crab stays spooked after a player gets close, in seconds.")]
		[Header("Startle")]
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

		[Header("Tint")]
		[Tooltip("Each crab picks a random point along this, applied where the material's colour mask is white.")]
		public Gradient tintRange;

		[Tooltip("Replaces the gradient for the odd crab, eg a rare blue one.")]
		public Color rareTint;

		[Range(0f, 1f)]
		public float rareTintChance;

		[Tooltip("Multiplies the colour mask for the rare crab, so its tint reaches further across the shell than the gradient's.")]
		public float rareTintStrength;
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

		public float3 tint;

		public float tintStrength;
	}

	public struct CrabSettings(in CrabType crabType)
	{
		public float swarmRadius = crabType.swarmRadius;

		public float minSpeed = crabType.minSpeed;

		public float maxSpeed = crabType.maxSpeed;

		public float minTurnSpeed = crabType.minTurnSpeed;

		public float maxTurnSpeed = crabType.maxTurnSpeed;

		public float2 wanderDistanceRange = float2.op_Implicit(crabType.wanderDistanceRange);

		public float2 restDurationRange = float2.op_Implicit(crabType.restDurationRange);

		public float2 catchUpDelayRange = float2.op_Implicit(crabType.catchUpDelayRange);

		public float wanderTimeout = crabType.wanderTimeout;

		public float startleDuration = crabType.startleDuration;

		public float startleRangeSq = crabType.startleRange * crabType.startleRange;

		public float startleBoundsMultiplier = crabType.startleBoundsMultiplier;

		public float fullAnimationSpeed = crabType.fullAnimationSpeed;

		public float animationBlendRate = crabType.animationBlendRate;

		public CritterLegSpeed legSpeed = CritterLegSpeed.FromMaterial(crabType.material);
	}

	internal const float groundProbeHeight = 1f;

	internal const float groundProbeDepth = 8f;

	internal const float maxWalkableNormalY = 0.6f;

	internal const float obstacleProbeDistance = 1f;

	internal const float obstacleSlideDistance = 2f;

	internal const float obstacleAvoidDuration = 1f;

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

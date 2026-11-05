using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class BirdGrouping : IDisposable
{
	[Serializable]
	public struct BirdType
	{
		public Mesh mesh;

		public Material material;

		[Header("Population")]
		public int maxCount;

		[Tooltip("How many birds probe ahead for obstacles and re-sample the height below them each update. Sampled as one batch, so raising this is cheap.")]
		public int castsPerFrame;

		[Tooltip("How far from the entity the birds spread out, in metres. Each bird is given its own spot within this to hang around.")]
		public float flockRadius;

		[Tooltip("Stretches the flock along each axis, as a multiple of flockRadius. Below 1 on Y gives a flatter flock, which reads more natural than a ball.")]
		[Header("Shape")]
		public Vector3 flockShape;

		[Tooltip("How far a bird can wander from its own spot before being pulled back, in metres. Higher is a looser flock.")]
		public float leashRadius;

		[Tooltip("How far the spots drift around over time, in metres. Neighbouring spots drift together, so the flock bulges and stretches rather than jittering.")]
		public float shapeNoiseAmount;

		[Tooltip("Size of the bulges the drift makes, in metres. Small breaks the flock into clumps, large sways it as a whole.")]
		public float shapeNoiseScale;

		[Tooltip("How quickly the shape changes. Low is a slow, lazy morph.")]
		public float shapeNoiseSpeed;

		[Header("Movement")]
		public float minSpeed;

		public float maxSpeed;

		[Tooltip("Hardest a bird can change its velocity, in metres per second per second. Low makes wide, graceful turns.")]
		public float maxAcceleration;

		[Tooltip("How far a bird rolls into a turn, in degrees, reached at a turn rate of one radian per second.")]
		public float maxBankAngle;

		[Tooltip("How quickly the roll follows a change in turn rate.")]
		public float bankEaseRate;

		[Tooltip("How much one bird's speed differs from another's, as a fraction either side. Stops the flock moving as one rigid sheet.")]
		public float speedVariance;

		[Tooltip("Seconds between one bird's darts, rolled each time. A dart is a quick swoop off to one side or down, like a scavenger checking something out.")]
		[Header("Darts")]
		public Vector2 dartInterval;

		[Tooltip("How far a dart goes from where the bird was, in metres.")]
		public Vector2 dartDistance;

		[Tooltip("How long a dart lasts, in seconds. The speed eases up and back down over it.")]
		public float dartDuration;

		[Tooltip("How much faster a bird can fly and turn at the height of a dart.")]
		public float dartSpeedMultiplier;

		[Header("Interested")]
		[Tooltip("How far the birds spread while the flock is over an interest point, as a multiple of their usual spots.")]
		public float interestedSpread;

		[Tooltip("Seconds between one bird's darts while the flock is over an interest point, rolled each time.")]
		public Vector2 interestedDartInterval;

		[Tooltip("How long the birds take to settle over the middle of the entity's circle, in seconds. Longer sits stiller over the point, shorter drifts round after the entity.")]
		public float interestedCenterLag;

		[Tooltip("How fast the birds' spots wheel round the point, in radians per second. Each bird is given its own share of this, so most drift round together at different rates while a few hang back or cut against them.")]
		public float interestedSwirlSpeed;

		[Tooltip("How far a bird looks for flockmates to fly with, in metres.")]
		[Header("Flocking")]
		public float neighbourRadius;

		[Tooltip("Closer than this to a flockmate and a bird pushes away from it, in metres.")]
		public float separationRadius;

		public float separationWeight;

		[Tooltip("How strongly a bird matches its flockmates' heading. High makes the flock wheel as one.")]
		public float alignmentWeight;

		[Tooltip("How strongly a bird closes on its flockmates. High makes a tight ball.")]
		public float cohesionWeight;

		[Tooltip("How hard a bird past leashRadius is pulled back towards its spot, per metre it is outside.")]
		public float anchorWeight;

		[Tooltip("How hard a bird matches the entity's velocity, per metre per second it is off. Keeps the flock with an entity that flies off somewhere, rather than trailing or circling it.")]
		public float anchorVelocityWeight;

		[Header("Fleeing")]
		[Tooltip("How much faster the birds can fly and turn while fleeing. Needs to keep up with the entity's flee speed.")]
		public float fleeSpeedMultiplier;

		[Tooltip("Gap between birds along each arm of the V the flock flees in, in metres.")]
		public float fleeSpacing;

		[Tooltip("How far each arm of the V sweeps back from straight behind, in degrees. Low is a narrow arrow, high a wide wedge.")]
		public float fleeArmAngle;

		[Tooltip("How far a fleeing bird can stray from its place in the V, in metres. Low holds a crisper shape.")]
		public float fleeLeashRadius;

		[Tooltip("Wing beat speed at the steepest climb, as a multiple of the material's level flight speed.")]
		[Header("Wing Beat")]
		public float flapRateClimbing;

		[Tooltip("Wing beat speed at the steepest dive, as a multiple of the material's level flight speed. Low reads as a glide.")]
		public float flapRateDiving;

		[Tooltip("How quickly the wing beat follows a change in climb angle.")]
		public float flapRateEaseRate;

		[Tooltip("How much one bird's wing beat speed differs from another's, as a fraction either side. Stops a flock drifting into flapping in step.")]
		public float flapRateVariance;

		[Tooltip("Where the wings are held in a glide, from -1 fully down to 1 fully up.")]
		[Range(-1f, 1f)]
		[Header("Gliding")]
		public float glideWingHeight;

		[Tooltip("How steep a descent has a bird glide down it instead of flapping, from 0 for any descent to 1 for only the steepest dive.")]
		[Range(0f, 1f)]
		public float diveGlideSteepness;

		[Tooltip("How long a bird glides between bouts of flapping in level flight, in seconds. Zero to only glide on dives.")]
		public Vector2 glideDuration;

		[Tooltip("How long a bird flaps between glides in level flight, in seconds.")]
		public Vector2 flapDuration;

		[Tooltip("How hard a gliding bird sinks. The flapping after climbs it back up, so level flight swoops.")]
		public float glideSink;

		[Header("Obstacles")]
		[Tooltip("How far ahead a bird looks for something to fly into, in metres.")]
		public float lookAheadDistance;

		public float obstacleWeight;

		[Tooltip("Lowest a bird is happy to fly above the ground or water, in metres.")]
		public float minClearance;

		[Tooltip("How hard a bird below minClearance climbs, per metre it is under.")]
		public float clearanceWeight;

		[Tooltip("How close a player gets before the birds nearest them scatter, in metres.")]
		[Header("Players")]
		public float playerAvoidRadius;

		public float playerAvoidWeight;

		[Header("Scale")]
		public float minScale;

		public float maxScale;

		[Header("LOD")]
		[Tooltip("Drawn instead of mesh once the flock is past farMeshDistance from the camera. Must use the same UV layout so the wing beat still works. Leave empty to always draw mesh.")]
		public Mesh farMesh;

		public float farMeshDistance;

		[Tooltip("Past this distance from the camera the birds stop casting shadows, in metres.")]
		public float shadowDistance;

		[Tooltip("Past this distance from the camera the flock is emptied and stops simulating, in metres. Birds grow back in once it is in range again.")]
		public float cullDistance;
	}

	public struct BirdData
	{
		public float3 velocity;

		public float3 obstacleAvoidance;

		public float3 homeOffset;

		public float surfaceY;

		public float scale;

		public float bank;

		public float flapRate;

		public quaternion poseOffset;

		public float flapRateScale;

		public float speedScale;

		public float3 dartTarget;

		public float dartTimeLeft;

		public float dartCooldown;

		public Random random;

		public float swirlScale;

		public bool glideBout;

		public float glideTimeLeft;
	}

	public struct BirdRenderData
	{
		public float3 position;

		public float scale;

		public quaternion rotation;

		public float flapPhase;
	}

	public struct BirdSettings(in BirdType birdType)
	{
		public float fleeSpeedMultiplier = birdType.fleeSpeedMultiplier;

		public float fleeSpacing = birdType.fleeSpacing;

		public float fleeArmAngle = math.radians(birdType.fleeArmAngle);

		public float fleeLeashRadius = birdType.fleeLeashRadius;

		public float leashRadius = birdType.leashRadius;

		public float shapeNoiseAmount = birdType.shapeNoiseAmount;

		public float shapeNoiseScale = birdType.shapeNoiseScale;

		public float shapeNoiseSpeed = birdType.shapeNoiseSpeed;

		public float flapSpeed = CritterLegSpeed.FromMaterial(birdType.material).moveSpeed;

		public float flapRateClimbing = birdType.flapRateClimbing;

		public float flapRateDiving = birdType.flapRateDiving;

		public float flapRateEaseRate = birdType.flapRateEaseRate;

		public float glidePhase = math.asin(birdType.glideWingHeight);

		public float diveGlideSteepness = birdType.diveGlideSteepness;

		public float2 glideDuration = float2.op_Implicit(birdType.glideDuration);

		public float2 flapDuration = float2.op_Implicit(birdType.flapDuration);

		public float glideSink = birdType.glideSink;

		public float minSpeed = birdType.minSpeed;

		public float maxSpeed = birdType.maxSpeed;

		public float maxAcceleration = birdType.maxAcceleration;

		public float maxBankAngle = math.radians(birdType.maxBankAngle);

		public float bankEaseRate = birdType.bankEaseRate;

		public float neighbourRadiusSq = birdType.neighbourRadius * birdType.neighbourRadius;

		public float separationRadiusSq = birdType.separationRadius * birdType.separationRadius;

		public float separationWeight = birdType.separationWeight;

		public float alignmentWeight = birdType.alignmentWeight;

		public float cohesionWeight = birdType.cohesionWeight;

		public float anchorWeight = birdType.anchorWeight;

		public float anchorVelocityWeight = birdType.anchorVelocityWeight;

		public float obstacleWeight = birdType.obstacleWeight;

		public float minClearance = birdType.minClearance;

		public float clearanceWeight = birdType.clearanceWeight;

		public float playerAvoidRadius = birdType.playerAvoidRadius;

		public float playerAvoidWeight = birdType.playerAvoidWeight;

		public float2 dartInterval = float2.op_Implicit(birdType.dartInterval);

		public float2 dartDistance = float2.op_Implicit(birdType.dartDistance);

		public float dartDuration = birdType.dartDuration;

		public float dartSpeedMultiplier = birdType.dartSpeedMultiplier;

		public float interestedSpread = birdType.interestedSpread;

		public float2 interestedDartInterval = float2.op_Implicit(birdType.interestedDartInterval);

		public float interestedSwirlSpeed = birdType.interestedSwirlSpeed;
	}

	private const int maxThreats = 8;

	internal const float maxClimbRatio = 0.5f;

	internal const float obstacleAvoidanceDecayRate = 1.5f;

	private const float anchorVelocityEaseRate = 2f;

	internal const float poseOffsetEaseRate = 2f;

	internal const float glideClimbLimit = 0.2f;

	private BirdType birdType;

	private JobHandle jobHandle;

	private int castCursor;

	private NativeArray<RaycastCommand> castCommands;

	private NativeArray<RaycastHit> castResults;

	private NativeArray<int> castIndices;

	private NativeArray<int> sampleSlots;

	private NativeArray<Vector3> samplePositions;

	private NativeArray<float> sampleTerrainY;

	private NativeArray<float> sampleWaterY;

	private NativeArray<BirdData> birdData;

	private NativeArray<BirdRenderData> birdRenderData;

	private int birdCount;

	private NativeArray<float3> flockPositions;

	private NativeArray<float3> flockVelocities;

	private NativeArray<float3> threatPositions;

	private int threatCount;

	private MaterialPropertyBlock materialPropertyBlock;

	private ComputeBuffer birdBuffer;

	private bool renderDataDirty;

	private float3 interestCenter;

	private bool wasInterested;

	private float3 anchorVelocity;

	private float3 lastFlockCenter;

	private bool hasLastFlockCenter;

	public float FlockRadius => birdType.flockRadius;

	public float FlockReach
	{
		get
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			return math.max(birdType.flockRadius * math.cmax(float3.op_Implicit(birdType.flockShape)) + birdType.leashRadius + birdType.shapeNoiseAmount, (float)(birdType.maxCount / 2 + 1) * birdType.fleeSpacing + birdType.fleeLeashRadius);
		}
	}

	public BirdGrouping(BirdType birdType)
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
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected Obj, but got Unknown
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected Obj, but got Unknown
		this.birdType = birdType;
		castCommands = new NativeArray<RaycastCommand>(birdType.castsPerFrame, (Allocator)4, (NativeArrayOptions)1);
		castResults = new NativeArray<RaycastHit>(birdType.castsPerFrame, (Allocator)4, (NativeArrayOptions)1);
		castIndices = new NativeArray<int>(birdType.castsPerFrame, (Allocator)4, (NativeArrayOptions)1);
		sampleSlots = new NativeArray<int>(birdType.castsPerFrame, (Allocator)4, (NativeArrayOptions)1);
		samplePositions = new NativeArray<Vector3>(birdType.castsPerFrame, (Allocator)4, (NativeArrayOptions)1);
		sampleTerrainY = new NativeArray<float>(birdType.castsPerFrame, (Allocator)4, (NativeArrayOptions)1);
		sampleWaterY = new NativeArray<float>(birdType.castsPerFrame, (Allocator)4, (NativeArrayOptions)1);
		birdData = new NativeArray<BirdData>(birdType.maxCount, (Allocator)4, (NativeArrayOptions)1);
		birdRenderData = new NativeArray<BirdRenderData>(birdType.maxCount, (Allocator)4, (NativeArrayOptions)1);
		flockPositions = new NativeArray<float3>(birdType.maxCount, (Allocator)4, (NativeArrayOptions)1);
		flockVelocities = new NativeArray<float3>(birdType.maxCount, (Allocator)4, (NativeArrayOptions)1);
		threatPositions = new NativeArray<float3>(8, (Allocator)4, (NativeArrayOptions)1);
		birdBuffer = new ComputeBuffer(birdType.maxCount, UnsafeUtility.SizeOf<BirdRenderData>());
		for (int i = 0; i < sampleSlots.Length; i++)
		{
			sampleSlots[i] = i;
		}
		materialPropertyBlock = new MaterialPropertyBlock();
		materialPropertyBlock.SetBuffer("_BirdData", birdBuffer);
	}

	public void Dispose()
	{
		jobHandle.Complete();
		castCommands.Dispose();
		castResults.Dispose();
		castIndices.Dispose();
		sampleSlots.Dispose();
		samplePositions.Dispose();
		sampleTerrainY.Dispose();
		sampleWaterY.Dispose();
		birdData.Dispose();
		birdRenderData.Dispose();
		flockPositions.Dispose();
		flockVelocities.Dispose();
		threatPositions.Dispose();
		birdBuffer.Dispose();
	}
}

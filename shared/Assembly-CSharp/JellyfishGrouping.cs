using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class JellyfishGrouping : IDisposable
{
	[Serializable]
	public struct JellyfishType
	{
		public Mesh mesh;

		public Material material;

		[Header("Population")]
		[Tooltip("How many jellyfish have their local water column re-sampled each update. Sampled as one batch, so raising this is cheap.")]
		public int samplesPerFrame;

		public int maxCount;

		[Tooltip("How far from the entity the jellyfish spread out. They turn back when they reach the edge rather than despawning.")]
		public float swarmRadius;

		[Header("Movement")]
		[Tooltip("Speed while drifting around. Jellyfish have no hurried state, this is the only speed.")]
		public float speed;

		[Tooltip("Turn rate while drifting.")]
		public float turnSpeed;

		[Tooltip("How much a jellyfish's size affects its speed. 0 means every jellyfish moves alike, 1 means speed scales straight with size. Keeps the swarm staggered instead of moving as one block.")]
		public float speedScaleInfluence;

		[Tooltip("Random speed spread per jellyfish, on top of size. 0.25 means each is up to 25 percent faster or slower than its size alone suggests.")]
		public float speedRandomness;

		[Tooltip("How far a jellyfish leans into a climb or dive, in degrees. This is the tilt at full tiltReferenceSpeed, reached only in a steep climb.")]
		[Header("Tilt")]
		public float maxTiltAngle;

		[Tooltip("Climb rate that earns the full maxTiltAngle, in metres per second. Lower tilts the swarm harder on gentle drifts.")]
		public float tiltReferenceSpeed;

		[Tooltip("How quickly the tilt follows a change in climb rate. Low is a languid lean, high snaps to it.")]
		public float tiltEaseRate;

		[Header("Pulse")]
		[Tooltip("Pulse rate of a jellyfish sat completely still, as a fraction of its rate at full speed. 0 stops the animation dead when resting, which looks lifeless. Around 0.3 keeps it ticking over.")]
		public float pulseRateAtRest;

		[Tooltip("Speed at which a jellyfish pulses at its full authored rate. Below this the pulse slows towards pulseRateAtRest, above it does not speed up further.")]
		public float pulseReferenceSpeed;

		[Tooltip("How close to the entity each jellyfish wants to stay, as a fraction of the swarm radius. Rolled per jellyfish, so some shadow the entity closely while others hang back near the edge.")]
		[Header("Following")]
		public Vector2 followTightnessRange;

		[Tooltip("Random variation in how long a jellyfish dawdles before setting off after the entity, in seconds. Stops the swarm surging as one block.")]
		public Vector2 catchUpDelayRange;

		[Header("Wandering")]
		[Tooltip("How far a jellyfish travels in one trip, as a fraction of the swarm radius. Picked randomly per trip.")]
		public Vector2 wanderDistanceRange;

		[Tooltip("How long a jellyfish hangs still after finishing a trip, in seconds. Higher means a calmer swarm.")]
		public Vector2 restDurationRange;

		[Tooltip("Give up on a destination after this long, so a stuck jellyfish doesn't push into a wall forever.")]
		public float wanderTimeout;

		[Header("Depth")]
		[Tooltip("Where in the water column each jellyfish sits, as a fraction of local depth. 0 hugs the surface and 1 hugs the floor. Rolled per jellyfish within this range so the swarm fills the column.")]
		public Vector2 depthFractionRange;

		[Tooltip("How quickly a jellyfish eases towards its preferred depth. Low is a slow, tidal rise and fall.")]
		public float verticalEaseRate;

		[Tooltip("How far a jellyfish wanders above and below its preferred depth over time, as a fraction of the local depth. Without this it converges on one height and stops moving vertically, which leaves it permanently level.")]
		public float depthDriftAmount;

		[Tooltip("How long one full rise and fall takes, in seconds. Long is a slow tidal swell.")]
		public float depthDriftPeriod;

		[Tooltip("Closest a jellyfish gets to the seabed, in metres.")]
		public float floorMargin;

		[Tooltip("Closest a jellyfish gets to the water surface, in metres.")]
		public float surfaceMargin;

		[Tooltip("Water this shallow is not worth drifting into, so a jellyfish that finds itself there picks a new destination. Keeps the swarm off sandbars.")]
		public float minSwimDepth;

		[Header("Displacement")]
		[Tooltip("How far from a moving body the water it pushes reaches, in metres. Generous reads better than tight - a body displaces water well past its own width.")]
		public float displacementRadius;

		[Tooltip("How hard a body at full reference speed shoves a jellyfish directly beside it.")]
		public float displacementStrength;

		[Tooltip("Speed at which a body displaces at full strength, in m/s. Roughly a swimming player, so a normal swim is near full and sprinting is not wildly more.")]
		public float referenceSpeed;

		[Tooltip("Something moving slower than this displaces nothing, so a player treading water leaves the swarm undisturbed.")]
		public float minDisplacerSpeed;

		[Tooltip("How quickly a displaced jellyfish settles back. Low means the swarm stays parted for longer after something swims through.")]
		public float pushDecayRate;

		[Header("Scale")]
		public float minScale;

		public float maxScale;
	}

	public struct JellyfishData
	{
		public bool isAlive;

		public float updateTime;

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

		public float depthFraction;

		public float floorY;

		public float surfaceY;

		public float3 pushVelocity;

		public float verticalSpeed;
	}

	public struct JellyfishRenderData
	{
		public float3 position;

		public float rotation;

		public float scale;

		public float seed;

		public float pitch;

		public float phase;
	}

	public struct JellyfishWaterProcessJob : IJob
	{
		public int sampleCount;

		public NativeArray<JellyfishData> jellyfishDataArray;

		[ReadOnly]
		public NativeArray<int> sampleIndices;

		[ReadOnly]
		public NativeArray<float> sampleFloorY;

		[ReadOnly]
		public NativeArray<float> sampleSurfaceY;

		public void Execute()
		{
		}
	}

	public struct JellyfishUpdateJob : IJobParallelFor
	{
		[ReadOnly]
		public float3 swarmCenter;

		[ReadOnly]
		public int displacerCount;

		[ReadOnly]
		public uint seed;

		[ReadOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<float3> displacerPositions;

		[ReadOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<float3> displacerVelocities;

		[ReadOnly]
		public float dt;

		[ReadOnly]
		public float swarmRadius;

		[ReadOnly]
		public float speed;

		[ReadOnly]
		public float turnSpeed;

		[ReadOnly]
		public float2 wanderDistanceRange;

		[ReadOnly]
		public float2 restDurationRange;

		[ReadOnly]
		public float2 catchUpDelayRange;

		[ReadOnly]
		public float wanderTimeout;

		[ReadOnly]
		public float verticalEaseRate;

		[ReadOnly]
		public float floorMargin;

		[ReadOnly]
		public float surfaceMargin;

		[ReadOnly]
		public float minSwimDepth;

		[ReadOnly]
		public float depthDriftAmount;

		[ReadOnly]
		public float depthDriftPeriod;

		[ReadOnly]
		public float elapsedTime;

		[ReadOnly]
		public float maxTiltAngle;

		[ReadOnly]
		public float tiltReferenceSpeed;

		[ReadOnly]
		public float tiltEaseRate;

		[ReadOnly]
		public float pulseRateAtRest;

		[ReadOnly]
		public float pulseReferenceSpeed;

		[ReadOnly]
		public float displacementRadius;

		[ReadOnly]
		public float displacementStrength;

		[ReadOnly]
		public float referenceSpeed;

		[ReadOnly]
		public float pushDecayRate;

		[NativeDisableUnsafePtrRestriction]
		public unsafe JellyfishData* jellyfishDataArray;

		[NativeDisableUnsafePtrRestriction]
		public unsafe JellyfishRenderData* jellyfishRenderDataArray;

		public void Execute(int i)
		{
		}
	}

	public struct KillJellyfish : IJob
	{
		public NativeArray<JellyfishData> jellyfishDataArray;

		public NativeArray<JellyfishRenderData> jellyfishRenderDataArray;

		public NativeArray<int> jellyfishCount;

		public void Execute()
		{
		}
	}

	private const int maxDisplacers = 8;

	private JellyfishType jellyfishType;

	private JobHandle jobHandle;

	private int sampleCursor;

	private NativeArray<int> sampleIndices;

	private NativeArray<int> sampleSlots;

	private NativeArray<Vector3> samplePositions;

	private NativeArray<float> sampleFloorY;

	private NativeArray<float> sampleSurfaceY;

	private NativeArray<JellyfishData> jellyfishData;

	private NativeArray<JellyfishRenderData> jellyfishRenderData;

	private NativeArray<int> jellyfishCount;

	private NativeArray<float3> displacerPositions;

	private NativeArray<float3> displacerVelocities;

	private int displacerCount;

	private MaterialPropertyBlock materialPropertyBlock;

	private ComputeBuffer jellyfishBuffer;

	private bool renderDataDirty;

	public float SwarmRadius => jellyfishType.swarmRadius;

	public JellyfishGrouping(JellyfishType jellyfishType)
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
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected Obj, but got Unknown
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Expected Obj, but got Unknown
		this.jellyfishType = jellyfishType;
		sampleIndices = new NativeArray<int>(jellyfishType.samplesPerFrame, (Allocator)4, (NativeArrayOptions)1);
		sampleSlots = new NativeArray<int>(jellyfishType.samplesPerFrame, (Allocator)4, (NativeArrayOptions)1);
		samplePositions = new NativeArray<Vector3>(jellyfishType.samplesPerFrame, (Allocator)4, (NativeArrayOptions)1);
		sampleFloorY = new NativeArray<float>(jellyfishType.samplesPerFrame, (Allocator)4, (NativeArrayOptions)1);
		sampleSurfaceY = new NativeArray<float>(jellyfishType.samplesPerFrame, (Allocator)4, (NativeArrayOptions)1);
		jellyfishData = new NativeArray<JellyfishData>(jellyfishType.maxCount, (Allocator)4, (NativeArrayOptions)1);
		jellyfishRenderData = new NativeArray<JellyfishRenderData>(jellyfishType.maxCount, (Allocator)4, (NativeArrayOptions)1);
		jellyfishCount = new NativeArray<int>(1, (Allocator)4, (NativeArrayOptions)1);
		displacerPositions = new NativeArray<float3>(8, (Allocator)4, (NativeArrayOptions)1);
		displacerVelocities = new NativeArray<float3>(8, (Allocator)4, (NativeArrayOptions)1);
		jellyfishBuffer = new ComputeBuffer(jellyfishType.maxCount, UnsafeUtility.SizeOf<JellyfishRenderData>());
		for (int i = 0; i < sampleSlots.Length; i++)
		{
			sampleSlots[i] = i;
		}
		materialPropertyBlock = new MaterialPropertyBlock();
		materialPropertyBlock.SetBuffer("_JellyfishData", jellyfishBuffer);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float GetTargetY(float floorY, float surfaceY, float depthFraction, float floorMargin = 0.2f, float surfaceMargin = 0.2f)
	{
		float num = floorY + floorMargin;
		float num2 = surfaceY - surfaceMargin;
		if (num > num2)
		{
			num = num2;
		}
		return math.lerp(num2, num, depthFraction);
	}

	public void Dispose()
	{
		jobHandle.Complete();
		sampleIndices.Dispose();
		sampleSlots.Dispose();
		samplePositions.Dispose();
		sampleFloorY.Dispose();
		sampleSurfaceY.Dispose();
		jellyfishData.Dispose();
		jellyfishRenderData.Dispose();
		jellyfishCount.Dispose();
		displacerPositions.Dispose();
		displacerVelocities.Dispose();
		jellyfishBuffer.Dispose();
	}
}

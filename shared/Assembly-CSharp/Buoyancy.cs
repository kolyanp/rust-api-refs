using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Development.Attributes;
using Facepunch;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UtilityJobs;
using WaterLevelJobs;

[ResetStaticFields]
public class Buoyancy : ListComponent<Buoyancy>, IServerComponent, IPrefabPreProcess
{
	public enum Priority
	{
		High,
		Low
	}

	[Serializable]
	public struct BuoyancyPointData
	{
		[ReadOnly]
		public Vector3 localPosition;

		[ReadOnly]
		public Vector3 rootToPoint;

		[NonSerialized]
		public Vector3 position;
	}

	public BuoyancyPoint[] points;

	public GameObjectRef[] waterImpacts;

	public Rigidbody rigidBody;

	public float buoyancyScale = 1f;

	public bool scaleForceWithMass;

	public bool doEffects = true;

	public float flowMovementScale = 1f;

	public float requiredSubmergedFraction = 0.5f;

	public bool useUnderwaterDrag;

	[Range(0f, 3f)]
	public float underwaterDrag = 2f;

	[FormerlySerializedAs("flatWaterLerp")]
	[Tooltip("How much this object will pay attention to the wave system, 0 = flat water, 1 = full waves (default 1)")]
	[Range(0f, 1f)]
	public float wavesEffect = 1f;

	public Action<bool> SubmergedChanged;

	public BaseEntity forEntity;

	[NonSerialized]
	public float submergedFraction;

	public bool FlowForceDisabled;

	[SerializeField]
	[ReadOnly]
	private BuoyancyPointData[] pointData;

	private bool initedPointArrays;

	private Vector2[] pointPositionArray;

	private Vector2[] pointPositionUVArray;

	private float[] pointShoreDistanceArray;

	private float[] pointTerrainHeightArray;

	private float[] pointWaterHeightArray;

	private float defaultDrag;

	private float defaultAngularDrag;

	private float timeInWater;

	[NonSerialized]
	public float? ArtificialHeight;

	private BaseVehicle forVehicle;

	private bool hasLocalPlayers;

	private bool hadLocalPlayers;

	private static NativeArray<Vector2> allPositions2D;

	private static NativeArray<Vector3> allPositions3D;

	private static NativeArray<Vector2> allUVPositions;

	private static NativeArray<float> pointWaterHeightNativeArray;

	private static NativeArray<float> pointShoreDistanceNativeArray;

	private static NativeArray<float> pointTerrainHeightNativeArray;

	private static NativeArray<WaterLevel.WaterInfo> pointWaterInfoNativeArray;

	private static NativeArray<float> radiiIgnores;

	private static NativeArray<bool> waterIgnoreResults;

	private static NativeArray<bool> instanceDoDeepWaterChecksStateArray;

	private static NativeArray<int> instancePointCountNativeArray;

	[ServerVar(Help = "(Generated) When enabled, buoyancy point physics updates are batched together each fixed update for better CPU efficiency")]
	public static bool use_batching = true;

	private Action _sleepCallback;

	private Action _wakeCallback;

	public float timeOutOfWater { get; private set; }

	public bool InWater => submergedFraction > requiredSubmergedFraction;

	bool IPrefabPreProcess.CanRunDuringBundling => false;

	public Priority BuoyancyPriority { get; set; }

	public static void Cleanup()
	{
	}

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (!Application.isPlaying || serverside)
		{
			SavePointData(forced: false);
		}
	}

	public void SavePointData(bool forced)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		if (points == null || points.Length == 0)
		{
			Rigidbody val = ((Component)this).GetComponent<Rigidbody>();
			if ((Object)(object)val == (Object)null)
			{
				val = ((Component)this).gameObject.AddComponent<Rigidbody>();
			}
			GameObject val2 = new GameObject("BuoyancyPoint");
			val2.transform.parent = ((Component)val).gameObject.transform;
			val2.transform.localPosition = val.centerOfMass;
			BuoyancyPoint buoyancyPoint = val2.AddComponent<BuoyancyPoint>();
			buoyancyPoint.buoyancyForce = val.mass * (0f - Physics.gravity.y);
			buoyancyPoint.buoyancyForce *= 1.32f;
			buoyancyPoint.size = 0.2f;
			points = new BuoyancyPoint[1];
			points[0] = buoyancyPoint;
		}
		if (pointData == null || pointData.Length != points.Length || forced)
		{
			pointData = new BuoyancyPointData[points.Length];
			for (int i = 0; i < points.Length; i++)
			{
				Transform transform = ((Component)points[i]).transform;
				pointData[i].localPosition = transform.localPosition;
				pointData[i].rootToPoint = ((Component)this).transform.InverseTransformPoint(transform.position);
			}
		}
	}

	public void SetBuoyancyPointLocations(List<Vector3> locations)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (points == null || points.Length != locations.Count)
		{
			Debug.LogWarning((object)"Trying to SetBuoyancyPointLocations with mismatching array lengths");
			return;
		}
		int num = 0;
		BuoyancyPoint[] array = points;
		for (int i = 0; i < array.Length; i++)
		{
			((Component)array[i]).transform.position = locations[num];
			num++;
		}
	}

	public static string DefaultWaterImpact()
	{
		return "assets/bundled/prefabs/fx/impacts/physics/water-enter-exit.prefab";
	}

	private void Awake()
	{
		forVehicle = forEntity as BaseVehicle;
		InvokeRandomized(CheckSleepState, 0.5f, 5f, 1f);
	}

	public void Sleep()
	{
		if (((Object)(object)forEntity == (Object)null || !forEntity.BuoyancySleep(InWater)) && (Object)(object)rigidBody != (Object)null)
		{
			rigidBody.Sleep();
		}
		((Behaviour)this).enabled = false;
	}

	private static void ClearArrays()
	{
		allPositions2D.MemClear<Vector2>();
		allPositions3D.MemClear<Vector3>();
		allUVPositions.MemClear<Vector2>();
		NativeArrayEx.MemClear(in pointShoreDistanceNativeArray);
		NativeArrayEx.MemClear(in pointTerrainHeightNativeArray);
		NativeArrayEx.MemClear(in pointWaterHeightNativeArray);
		NativeArrayEx.MemClear(in pointWaterInfoNativeArray);
		NativeArrayEx.MemClear(in radiiIgnores);
		NativeArrayEx.MemClear(in waterIgnoreResults);
		NativeArrayEx.MemClear(in instanceDoDeepWaterChecksStateArray);
		NativeArrayEx.MemClear(in instancePointCountNativeArray);
	}

	private static void EnsureNativeArrayLength(int maxPoints, int maxInstances)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		if (maxPoints > allPositions2D.Length || maxInstances > instancePointCountNativeArray.Length)
		{
			DisposeNativeArrays();
			allPositions2D = new NativeArray<Vector2>(maxPoints, (Allocator)4, (NativeArrayOptions)1);
			allPositions3D = new NativeArray<Vector3>(maxPoints, (Allocator)4, (NativeArrayOptions)1);
			allUVPositions = new NativeArray<Vector2>(maxPoints, (Allocator)4, (NativeArrayOptions)1);
			pointWaterHeightNativeArray = new NativeArray<float>(maxPoints, (Allocator)4, (NativeArrayOptions)1);
			pointShoreDistanceNativeArray = new NativeArray<float>(maxPoints, (Allocator)4, (NativeArrayOptions)1);
			pointTerrainHeightNativeArray = new NativeArray<float>(maxPoints, (Allocator)4, (NativeArrayOptions)1);
			pointWaterHeightNativeArray = new NativeArray<float>(maxPoints, (Allocator)4, (NativeArrayOptions)1);
			pointWaterInfoNativeArray = new NativeArray<WaterLevel.WaterInfo>(maxPoints, (Allocator)4, (NativeArrayOptions)1);
			radiiIgnores = new NativeArray<float>(maxPoints, (Allocator)4, (NativeArrayOptions)1);
			waterIgnoreResults = new NativeArray<bool>(maxPoints, (Allocator)4, (NativeArrayOptions)1);
			instanceDoDeepWaterChecksStateArray = new NativeArray<bool>(maxInstances, (Allocator)4, (NativeArrayOptions)1);
			instancePointCountNativeArray = new NativeArray<int>(maxInstances, (Allocator)4, (NativeArrayOptions)1);
		}
	}

	private static void DisposeNativeArrays()
	{
		allPositions2D.SafeDispose<Vector2>();
		allPositions3D.SafeDispose<Vector3>();
		allUVPositions.SafeDispose<Vector2>();
		NativeArrayEx.SafeDispose(ref pointShoreDistanceNativeArray);
		NativeArrayEx.SafeDispose(ref pointTerrainHeightNativeArray);
		NativeArrayEx.SafeDispose(ref pointWaterHeightNativeArray);
		NativeArrayEx.SafeDispose(ref pointWaterInfoNativeArray);
		NativeArrayEx.SafeDispose(ref radiiIgnores);
		NativeArrayEx.SafeDispose(ref waterIgnoreResults);
		NativeArrayEx.SafeDispose(ref instanceDoDeepWaterChecksStateArray);
		NativeArrayEx.SafeDispose(ref instancePointCountNativeArray);
	}

	public void Wake()
	{
		if (((Object)(object)forEntity == (Object)null || !forEntity.BuoyancyWake()) && (Object)(object)rigidBody != (Object)null)
		{
			rigidBody.WakeUp();
		}
		((Behaviour)this).enabled = true;
	}

	public void CheckSleepState()
	{
		if ((Object)(object)((Component)this).transform == (Object)null || (Object)(object)rigidBody == (Object)null)
		{
			return;
		}
		hasLocalPlayers = HasLocalPlayers();
		bool flag = rigidBody.IsSleeping() || rigidBody.isKinematic;
		bool flag2 = flag || (!hasLocalPlayers && timeInWater > 6f);
		if ((Object)(object)forVehicle != (Object)null && forVehicle.IsOn())
		{
			flag2 = false;
		}
		if (((Behaviour)this).enabled && flag2)
		{
			if (_sleepCallback == null)
			{
				_sleepCallback = Sleep;
			}
			Invoke(_sleepCallback, 0f);
			return;
		}
		if (!((Behaviour)this).enabled && hasLocalPlayers && !hadLocalPlayers)
		{
			DoCycle(forced: true);
		}
		bool flag3 = !flag || ShouldWake(hasLocalPlayers);
		if (!((Behaviour)this).enabled && flag3)
		{
			if (_wakeCallback == null)
			{
				_wakeCallback = Wake;
			}
			Invoke(_wakeCallback, 0f);
		}
		hadLocalPlayers = hasLocalPlayers;
	}

	public void LowPriorityCheck(bool forceHighPriority)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		Priority buoyancyPriority = BuoyancyPriority;
		Priority priority = buoyancyPriority;
		if (forceHighPriority)
		{
			priority = Priority.High;
		}
		else
		{
			Vector3 position = ((Component)this).transform.position;
			priority = ((!BaseNetworkable.HasCloseConnections(position, Server.lowPriorityBuoyancyRange)) ? Priority.Low : Priority.High);
			if (priority == Priority.Low && priority != buoyancyPriority)
			{
				Vector3 val = Vector3Ex.WithY(((Component)this).transform.TransformPoint(Vector3.forward * 2f), position.y);
				Rigidbody obj = rigidBody;
				Vector3 val2 = val - rigidBody.position;
				obj.rotation = Quaternion.LookRotation(((Vector3)(ref val2)).normalized, Vector3.up);
			}
		}
		if (priority != buoyancyPriority)
		{
			rigidBody.linearVelocity = Vector3.zero;
			rigidBody.angularVelocity = Vector3.zero;
			BuoyancyPriority = priority;
		}
	}

	public bool ShouldWake()
	{
		return ShouldWake(HasLocalPlayers());
	}

	public bool ShouldWake(bool hasLocalPlayers)
	{
		return hasLocalPlayers;
	}

	private bool HasLocalPlayers()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return BaseNetworkable.HasCloseConnections(((Component)this).transform.position, 100f);
	}

	private static void DoCycleBatched(ReadOnlySpan<Buoyancy> buoyancies, bool forced)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (buoyancies.Length == 0)
		{
			return;
		}
		PooledArray<bool> wasSubmergedStates = default(PooledArray<bool>);
		wasSubmergedStates._002Ector(buoyancies.Length);
		try
		{
			PooledArray<bool> wasSubmergedStates2 = new PooledArray<bool>(buoyancies.Length);
			try
			{
				PooledList<Buoyancy> val = Pool.Get<PooledList<Buoyancy>>();
				try
				{
					PooledList<Buoyancy> val2 = Pool.Get<PooledList<Buoyancy>>();
					try
					{
						int num = 0;
						int num2 = 0;
						for (int i = 0; i < buoyancies.Length; i++)
						{
							Buoyancy buoyancy = buoyancies[i];
							if (!((Object)(object)buoyancy == (Object)null) && !((Object)(object)buoyancy.rigidBody == (Object)null))
							{
								if (buoyancy.buoyancyScale == 0f)
								{
									buoyancy.Sleep();
								}
								else if (DeepSeaManager.IsInsideDeepSea(((Component)buoyancy).transform.position))
								{
									((List<Buoyancy>)(object)val2).Add(buoyancy);
									num2 += buoyancy.GetPointDataCount();
									wasSubmergedStates2[((List<Buoyancy>)(object)val2).Count - 1] = buoyancy.submergedFraction > 0f;
								}
								else
								{
									num += buoyancy.GetPointDataCount();
									((List<Buoyancy>)(object)val).Add(buoyancy);
									wasSubmergedStates[((List<Buoyancy>)(object)val).Count - 1] = buoyancy.submergedFraction > 0f;
								}
							}
						}
						int count = ((List<Buoyancy>)(object)val).Count;
						int count2 = ((List<Buoyancy>)(object)val2).Count;
						int maxPoints = math.max(num, num2);
						int maxInstances = math.max(count, count2);
						EnsureNativeArrayLength(maxPoints, maxInstances);
						if (count > 0)
						{
							ClearArrays();
							BuoyancyFixedUpdateBatched(UnsafeListAccess.ListAsReadOnlySpan<Buoyancy>((List<Buoyancy>)(object)val), isDeepSea: false);
						}
						if (count2 > 0)
						{
							ClearArrays();
							BuoyancyFixedUpdateBatched(UnsafeListAccess.ListAsReadOnlySpan<Buoyancy>((List<Buoyancy>)(object)val2), isDeepSea: true);
						}
						UpdateSubmergedState(UnsafeListAccess.ListAsReadOnlySpan<Buoyancy>((List<Buoyancy>)(object)val), wasSubmergedStates);
						UpdateSubmergedState(UnsafeListAccess.ListAsReadOnlySpan<Buoyancy>((List<Buoyancy>)(object)val2), wasSubmergedStates2);
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
			finally
			{
				((IDisposable)wasSubmergedStates2/*cast due to constrained. prefix*/).Dispose();
			}
		}
		finally
		{
			((IDisposable)wasSubmergedStates/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private static void UpdateSubmergedState(ReadOnlySpan<Buoyancy> buoyancies, PooledArray<bool> wasSubmergedStates)
	{
		for (int i = 0; i < buoyancies.Length; i++)
		{
			Buoyancy buoyancy = buoyancies[i];
			Rigidbody val = buoyancy.rigidBody;
			bool flag = buoyancy.submergedFraction > 0f;
			if (wasSubmergedStates[i] == flag)
			{
				continue;
			}
			if (buoyancy.useUnderwaterDrag && (Object)(object)val != (Object)null)
			{
				if (flag)
				{
					buoyancy.defaultDrag = val.linearDamping;
					buoyancy.defaultAngularDrag = val.angularDamping;
					val.linearDamping = buoyancy.underwaterDrag;
					val.angularDamping = buoyancy.underwaterDrag;
				}
				else
				{
					val.linearDamping = buoyancy.defaultDrag;
					val.angularDamping = buoyancy.defaultAngularDrag;
				}
			}
			buoyancy.SubmergedChanged?.Invoke(flag);
		}
	}

	protected void DoCycle(bool forced = false)
	{
		if (!((Behaviour)this).enabled && !forced)
		{
			return;
		}
		bool num = submergedFraction > 0f;
		BuoyancyFixedUpdate();
		bool flag = submergedFraction > 0f;
		if (num == flag)
		{
			return;
		}
		if (useUnderwaterDrag && (Object)(object)rigidBody != (Object)null)
		{
			if (flag)
			{
				defaultDrag = rigidBody.linearDamping;
				defaultAngularDrag = rigidBody.angularDamping;
				rigidBody.linearDamping = underwaterDrag;
				rigidBody.angularDamping = underwaterDrag;
			}
			else
			{
				rigidBody.linearDamping = defaultDrag;
				rigidBody.angularDamping = defaultAngularDrag;
			}
		}
		if (SubmergedChanged != null)
		{
			SubmergedChanged(flag);
		}
	}

	public static void Cycle()
	{
		bool autoSyncTransforms = Physics.autoSyncTransforms;
		try
		{
			Physics.autoSyncTransforms = false;
			Buoyancy[] buffer = ListComponent<Buoyancy>.InstanceList.Values.Buffer;
			int count = ListComponent<Buoyancy>.InstanceList.Count;
			if (use_batching)
			{
				DoCycleBatched(ListComponent<Buoyancy>.InstanceList.Values.ContentReadOnlySpan(), forced: false);
				return;
			}
			for (int i = 0; i < count; i++)
			{
				buffer[i].DoCycle();
			}
		}
		finally
		{
			if (autoSyncTransforms)
			{
				Physics.SyncTransforms();
			}
			Physics.autoSyncTransforms = autoSyncTransforms;
		}
	}

	private static Vector3 GetFlowDirection(Vector3 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return WaterLevel.GetWaterFlowDirection(worldPos);
	}

	private static void BuoyancyFixedUpdateBatched(ReadOnlySpan<Buoyancy> activeComponents, bool isDeepSea)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_060f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0612: Unknown result type (might be due to invalid IL or missing references)
		//IL_0603: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_0617: Unknown result type (might be due to invalid IL or missing references)
		//IL_061d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0622: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_047c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0704: Unknown result type (might be due to invalid IL or missing references)
		//IL_0709: Unknown result type (might be due to invalid IL or missing references)
		//IL_070b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0710: Unknown result type (might be due to invalid IL or missing references)
		//IL_0715: Unknown result type (might be due to invalid IL or missing references)
		//IL_066d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0672: Unknown result type (might be due to invalid IL or missing references)
		//IL_067f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0691: Unknown result type (might be due to invalid IL or missing references)
		//IL_069f: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_073d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0742: Unknown result type (might be due to invalid IL or missing references)
		//IL_0748: Unknown result type (might be due to invalid IL or missing references)
		//IL_074d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0763: Unknown result type (might be due to invalid IL or missing references)
		//IL_086f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0887: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0813: Unknown result type (might be due to invalid IL or missing references)
		//IL_0815: Unknown result type (might be due to invalid IL or missing references)
		//IL_0817: Unknown result type (might be due to invalid IL or missing references)
		//IL_081c: Unknown result type (might be due to invalid IL or missing references)
		Bounds deepSeaBounds = DeepSeaManager.DeepSeaBounds;
		Vector3 terrainPosition = TerrainMeta.Position;
		Vector3 terrainOneOverSize = TerrainMeta.OneOverSize;
		int pointIndexOffset = 0;
		for (int i = 0; i < activeComponents.Length; i++)
		{
			Buoyancy buoyancy = activeComponents[i];
			if (buoyancy.BuoyancyPriority == Priority.High)
			{
				NativeArray<BuoyancyPointData> val = NativeArrayUtility.CopyToNativeArray<BuoyancyPointData>(buoyancy.pointData, (Allocator)2);
				BuoyancyBurstUtility.FillPointData(in pointIndexOffset, ref allPositions2D, ref allUVPositions, ((Component)buoyancy).transform.localToWorldMatrix, ref val, in deepSeaBounds, in terrainPosition, in terrainOneOverSize, in isDeepSea, ref allPositions3D, out var pointCount);
				pointIndexOffset += pointCount;
				instancePointCountNativeArray[i] = pointCount;
				val.Dispose();
			}
			else
			{
				allPositions2D[pointIndexOffset] = Vector2.op_Implicit(((Component)buoyancy).transform.position);
				allPositions3D[pointIndexOffset] = ((Component)buoyancy).transform.position;
				pointIndexOffset++;
				instancePointCountNativeArray[i] = 1;
			}
			instanceDoDeepWaterChecksStateArray[i] = !buoyancy.ArtificialHeight.HasValue;
		}
		NativeArray<Vector2> pos = allPositions2D.GetSubArray(0, pointIndexOffset);
		NativeArray<Vector3> allPositions = allPositions3D.GetSubArray(0, pointIndexOffset);
		NativeArray<Vector2> posUV = allUVPositions.GetSubArray(0, pointIndexOffset);
		NativeArray<float> shore = pointShoreDistanceNativeArray.GetSubArray(0, pointIndexOffset);
		NativeArray<float> terrainHeight = pointTerrainHeightNativeArray.GetSubArray(0, pointIndexOffset);
		NativeArray<float> waterHeight = pointWaterHeightNativeArray.GetSubArray(0, pointIndexOffset);
		NativeArray<WaterLevel.WaterInfo> pointWaterInfo = pointWaterInfoNativeArray.GetSubArray(0, pointIndexOffset);
		NativeArray<float> subArray = radiiIgnores.GetSubArray(0, pointIndexOffset);
		NativeArray<bool> waterIgnoreStates = waterIgnoreResults.GetSubArray(0, pointIndexOffset);
		NativeArray<int> subArray2 = instancePointCountNativeArray.GetSubArray(0, activeComponents.Length);
		WaterSystem.GetHeightArray(in pos, in posUV, ref shore, ref terrainHeight, ref waterHeight, isDeepSea);
		TerrainTopologyMap.TopologyQueryStructure topologyMap = TerrainMeta.TopologyMap.GetQueryStructure();
		FillJob<float> fillJob = new FillJob<float>
		{
			Values = subArray,
			Value = 0.01f
		};
		IJobExtensions.RunByRef<FillJob<float>>(ref fillJob);
		WaterSystem.Collision?.GetIgnore(allPositions.AsReadOnly(), subArray.AsReadOnly(), waterIgnoreStates);
		NativeArray<bool> needsDeepWaterChecks = default(NativeArray<bool>);
		needsDeepWaterChecks._002Ector(pointWaterInfo.Length, (Allocator)2, (NativeArrayOptions)1);
		WaterLevelBurst.GetBuoyancyWaterInfoBatched(in allPositions, in posUV, in terrainHeight, in waterHeight, in instanceDoDeepWaterChecksStateArray, ref pointWaterInfo, in subArray2, activeComponents.Length, in topologyMap, in waterIgnoreStates, ref needsDeepWaterChecks, isDeepSea, out var hasAnyDeepWaterChecks);
		if (hasAnyDeepWaterChecks)
		{
			WaterLevelBurst.ConstructDeepWaterCommands(in allPositions, in pointWaterInfo, in needsDeepWaterChecks, out var deepWaterCasts, out var raycastPointIndices, (Allocator)3);
			if (deepWaterCasts.Length > 0)
			{
				NativeArray<RaycastHit> val2 = default(NativeArray<RaycastHit>);
				val2._002Ector(deepWaterCasts.Length, (Allocator)3, (NativeArrayOptions)1);
				NativeArray<RaycastCommand> val3 = NativeList<RaycastCommand>.op_Implicit(deepWaterCasts);
				NativeArray<RaycastHit> val4 = val2;
				JobHandle val5 = default(JobHandle);
				val5 = RaycastCommand.ScheduleBatch(val3, val4, 1, val5);
				((JobHandle)(ref val5)).Complete();
				for (int j = 0; j < deepWaterCasts.Length; j++)
				{
					RaycastHit val6 = val2[j];
					if (((RaycastHit)(ref val6)).colliderInstanceID != 0)
					{
						int num = raycastPointIndices[j];
						WaterLevel.WaterInfo waterInfo = pointWaterInfo[num];
						float surfaceLevel = waterInfo.surfaceLevel;
						val6 = val2[j];
						Bounds bounds = ((RaycastHit)(ref val6)).collider.bounds;
						float num2 = Mathf.Min(surfaceLevel, ((Bounds)(ref bounds)).max.y);
						waterInfo.currentDepth = Mathf.Max(0f, num2 - allPositions[num].y);
						waterInfo.overallDepth = Mathf.Max(0f, num2 - terrainHeight[num]);
						waterInfo.surfaceLevel = num2;
					}
				}
				val2.Dispose();
			}
			deepWaterCasts.Dispose();
		}
		float time = Time.time;
		float fixedDeltaTime = Time.fixedDeltaTime;
		NativeArray<BuoyancyForceAccumulationBurst.InstanceInput> instances = default(NativeArray<BuoyancyForceAccumulationBurst.InstanceInput>);
		instances._002Ector(activeComponents.Length, (Allocator)2, (NativeArrayOptions)1);
		NativeArray<BuoyancyForceAccumulationBurst.InstanceOutput> results = default(NativeArray<BuoyancyForceAccumulationBurst.InstanceOutput>);
		results._002Ector(activeComponents.Length, (Allocator)2, (NativeArrayOptions)1);
		NativeArray<float> pointSize = default(NativeArray<float>);
		pointSize._002Ector(pointIndexOffset, (Allocator)2, (NativeArrayOptions)1);
		NativeArray<float> pointBuoyancyForce = default(NativeArray<float>);
		pointBuoyancyForce._002Ector(pointIndexOffset, (Allocator)2, (NativeArrayOptions)1);
		NativeArray<float> pointRandomOffset = default(NativeArray<float>);
		pointRandomOffset._002Ector(pointIndexOffset, (Allocator)2, (NativeArrayOptions)1);
		NativeArray<float> pointWaveFrequency = default(NativeArray<float>);
		pointWaveFrequency._002Ector(pointIndexOffset, (Allocator)2, (NativeArrayOptions)1);
		NativeArray<float> pointWaveScale = default(NativeArray<float>);
		pointWaveScale._002Ector(pointIndexOffset, (Allocator)2, (NativeArrayOptions)1);
		int num3 = 0;
		for (int k = 0; k < activeComponents.Length; k++)
		{
			Buoyancy buoyancy2 = activeComponents[k];
			int num4 = subArray2[k];
			Rigidbody val7 = buoyancy2.rigidBody;
			instances[k] = new BuoyancyForceAccumulationBurst.InstanceInput
			{
				pointStartIndex = num3,
				pointCount = num4,
				buoyancyScale = buoyancy2.buoyancyScale,
				rigidBodyMass = (((Object)(object)val7 != (Object)null) ? val7.mass : 0f),
				wavesEffect = buoyancy2.wavesEffect,
				scaleForceWithMass = buoyancy2.scaleForceWithMass,
				flowForceDisabled = buoyancy2.FlowForceDisabled,
				flowMovementScale = buoyancy2.flowMovementScale,
				worldCom = (((Object)(object)val7 != (Object)null) ? float3.op_Implicit(val7.worldCenterOfMass) : float3.zero)
			};
			if (buoyancy2.BuoyancyPriority == Priority.High)
			{
				for (int l = 0; l < buoyancy2.points.Length; l++)
				{
					BuoyancyPoint buoyancyPoint = buoyancy2.points[l];
					int num5 = num3 + l;
					pointSize[num5] = buoyancyPoint.size;
					pointBuoyancyForce[num5] = buoyancyPoint.buoyancyForce;
					pointRandomOffset[num5] = buoyancyPoint.randomOffset;
					pointWaveFrequency[num5] = buoyancyPoint.waveFrequency;
					pointWaveScale[num5] = buoyancyPoint.waveScale;
				}
			}
			else
			{
				int num6 = num3;
				if (buoyancy2.points != null && buoyancy2.points.Length != 0)
				{
					BuoyancyPoint buoyancyPoint2 = buoyancy2.points[0];
					pointSize[num6] = buoyancyPoint2.size;
					pointBuoyancyForce[num6] = buoyancyPoint2.buoyancyForce;
					pointRandomOffset[num6] = buoyancyPoint2.randomOffset;
					pointWaveFrequency[num6] = buoyancyPoint2.waveFrequency;
					pointWaveScale[num6] = buoyancyPoint2.waveScale;
				}
				else
				{
					pointSize[num6] = 0f;
					pointBuoyancyForce[num6] = 0f;
					pointRandomOffset[num6] = 0f;
					pointWaveFrequency[num6] = 0f;
					pointWaveScale[num6] = 0f;
				}
			}
			num3 += num4;
		}
		NativeArray<float3> pointFlowDirection = (((Object)(object)TerrainMeta.WaterFlowMap != (Object)null) ? TerrainMeta.WaterFlowMap.GetFlowDirections(allPositions, (Allocator)2) : new NativeArray<float3>(pointIndexOffset, (Allocator)2, (NativeArrayOptions)1));
		BuoyancyForceAccumulationBurst.Compute(in instances, allPositions.Reinterpret<float3>(), in shore, in pointWaterInfo, in pointSize, in pointBuoyancyForce, in pointRandomOffset, in pointWaveFrequency, in pointWaveScale, in pointFlowDirection, time, ref results);
		num3 = 0;
		Vector3 val13 = default(Vector3);
		for (int m = 0; m < activeComponents.Length; m++)
		{
			Buoyancy buoyancy3 = activeComponents[m];
			Rigidbody val8 = buoyancy3.rigidBody;
			if (buoyancy3.BuoyancyPriority == Priority.Low)
			{
				Vector3 val9 = allPositions[num3];
				WaterLevel.WaterInfo waterInfo2 = pointWaterInfo[num3];
				if (val9.y < waterInfo2.surfaceLevel)
				{
					val8.position = new Vector3(val9.x, waterInfo2.surfaceLevel, val9.z);
				}
				num3++;
				continue;
			}
			int pointCount2 = instances[m].pointCount;
			int num7 = num3 + pointCount2;
			GameObjectRef[] array = buoyancy3.waterImpacts;
			bool flag = buoyancy3.doEffects;
			bool inWater = buoyancy3.InWater;
			BuoyancyForceAccumulationBurst.InstanceOutput instanceOutput = results[m];
			int numSubmerged = instanceOutput.numSubmerged;
			Vector3 val10 = float3.op_Implicit(instanceOutput.netForce);
			Vector3 val11 = float3.op_Implicit(instanceOutput.netTorque);
			int num8 = 0;
			for (int n = num3; n < num7; n++)
			{
				BuoyancyPoint buoyancyPoint3 = buoyancy3.points[num8];
				Vector3 localPosition = buoyancy3.pointData[num8].localPosition;
				Vector3 val12 = allPositions[n];
				WaterLevel.WaterInfo waterInfo3 = pointWaterInfo[n];
				bool flag2 = waterInfo3.isValid && val12.y < waterInfo3.surfaceLevel;
				if (buoyancyPoint3.doSplashEffects && ((!buoyancyPoint3.wasSubmergedLastFrame && flag2) || (!flag2 && buoyancyPoint3.wasSubmergedLastFrame)) && flag)
				{
					Vector3 relativePointVelocity = val8.GetRelativePointVelocity(localPosition);
					if (((Vector3)(ref relativePointVelocity)).magnitude > 1f)
					{
						string strName = ((array != null && array.Length != 0 && array[0].isValid) ? array[0].resourcePath : DefaultWaterImpact());
						((Vector3)(ref val13))._002Ector(Random.Range(-0.25f, 0.25f), 0f, Random.Range(-0.25f, 0.25f));
						Effect.server.Run(strName, val12 + val13, Vector3.up);
						buoyancyPoint3.nexSplashTime = time + 0.25f;
					}
				}
				buoyancyPoint3.wasSubmergedLastFrame = flag2;
				num8++;
			}
			num3 += pointCount2;
			if (((Vector3)(ref val10)).sqrMagnitude > 0f)
			{
				val8.AddForce(val10, (ForceMode)0);
			}
			if (((Vector3)(ref val11)).sqrMagnitude > 0f)
			{
				val8.AddTorque(val11, (ForceMode)0);
			}
			if (pointCount2 > 0)
			{
				buoyancy3.submergedFraction = (float)numSubmerged / (float)pointCount2;
			}
			if (inWater)
			{
				buoyancy3.timeInWater += fixedDeltaTime;
				buoyancy3.timeOutOfWater = 0f;
			}
			else
			{
				buoyancy3.timeOutOfWater += fixedDeltaTime;
				buoyancy3.timeInWater = 0f;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetPointDataCount()
	{
		if (BuoyancyPriority == Priority.High)
		{
			return pointData.Length;
		}
		return 1;
	}

	public void BuoyancyFixedUpdate()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rigidBody == (Object)null)
		{
			return;
		}
		if (buoyancyScale == 0f)
		{
			Invoke(Sleep, 0f);
			return;
		}
		if (BuoyancyPriority == Priority.Low)
		{
			WaterLevel.WaterInfo waterInfo = WaterLevel.GetWaterInfo(((Component)this).transform.position, waves: true, volumes: true, forEntity);
			Vector3 position = rigidBody.position;
			if (position.y < waterInfo.surfaceLevel)
			{
				rigidBody.position = new Vector3(position.x, waterInfo.surfaceLevel, position.z);
			}
			return;
		}
		if (!initedPointArrays)
		{
			InitPointArrays();
		}
		float time = Time.time;
		Matrix4x4 localToWorldMatrix = ((Component)this).transform.localToWorldMatrix;
		bool flag = DeepSeaManager.IsInsideDeepSea(((Matrix4x4)(ref localToWorldMatrix)).GetPosition());
		float x;
		float z;
		float x2;
		float z2;
		if (flag)
		{
			x = ((Bounds)(ref DeepSeaManager.DeepSeaBounds)).min.x;
			z = ((Bounds)(ref DeepSeaManager.DeepSeaBounds)).min.z;
			x2 = Vector3Ex.Inverse(((Bounds)(ref DeepSeaManager.DeepSeaBounds)).size).x;
			z2 = Vector3Ex.Inverse(((Bounds)(ref DeepSeaManager.DeepSeaBounds)).size).z;
		}
		else
		{
			x = TerrainMeta.Position.x;
			z = TerrainMeta.Position.z;
			x2 = TerrainMeta.OneOverSize.x;
			z2 = TerrainMeta.OneOverSize.z;
		}
		for (int i = 0; i < pointData.Length; i++)
		{
			Vector3 val = ((Matrix4x4)(ref localToWorldMatrix)).MultiplyPoint3x4(pointData[i].rootToPoint);
			pointData[i].position = val;
			float num = (val.x - x) * x2;
			float num2 = (val.z - z) * z2;
			pointPositionArray[i] = new Vector2(val.x, val.z);
			pointPositionUVArray[i] = new Vector2(num, num2);
		}
		WaterSystem.GetHeightArray(pointPositionArray, pointPositionUVArray, pointShoreDistanceArray, pointTerrainHeightArray, pointWaterHeightArray, flag);
		bool flag2 = wavesEffect < 1f;
		int num3 = 0;
		Vector3 accumForce = default(Vector3);
		Vector3 val2 = default(Vector3);
		for (int j = 0; j < points.Length; j++)
		{
			BuoyancyPoint buoyancyPoint = points[j];
			Vector3 pos = pointData[j].position;
			Vector3 localPosition = pointData[j].localPosition;
			Vector2 posUV = pointPositionUVArray[j];
			float terrainHeight = pointTerrainHeightArray[j];
			float num4 = pointWaterHeightArray[j];
			if (ArtificialHeight.HasValue)
			{
				num4 = ArtificialHeight.Value;
			}
			else if (flag2)
			{
				num4 = Mathf.Lerp(0f, num4, wavesEffect);
			}
			bool doDeepwaterChecks = !ArtificialHeight.HasValue;
			WaterLevel.WaterInfo waterInfo2 = WaterLevel.GetBuoyancyWaterInfo(pos, posUV, terrainHeight, num4, doDeepwaterChecks, forEntity);
			if (flag2 && waterInfo2.isValid)
			{
				waterInfo2.currentDepth = Mathf.Lerp(waterInfo2.currentDepth, waterInfo2.surfaceLevel - pos.y, wavesEffect);
			}
			bool flag3 = false;
			if (pos.y < waterInfo2.surfaceLevel && waterInfo2.isValid)
			{
				flag3 = true;
				num3++;
				float currentDepth = waterInfo2.currentDepth;
				float num5 = Mathf.InverseLerp(0f, buoyancyPoint.size, currentDepth);
				float num6 = 1f + Mathf.PerlinNoise(buoyancyPoint.randomOffset + time * buoyancyPoint.waveFrequency, 0f) * buoyancyPoint.waveScale;
				float scaledBuoyancyForce = buoyancyPoint.buoyancyForce * buoyancyScale;
				if (scaleForceWithMass)
				{
					scaledBuoyancyForce *= rigidBody.mass;
				}
				((Vector3)(ref accumForce))._002Ector(0f, num6 * num5 * scaledBuoyancyForce, 0f);
				AccumulateFlowForce(ref accumForce, in pos, in waterInfo2, Mathf.Abs(pointShoreDistanceArray[j]), ref scaledBuoyancyForce, in FlowForceDisabled, in flowMovementScale);
				rigidBody.AddForceAtPosition(accumForce, pos, (ForceMode)0);
			}
			if (buoyancyPoint.doSplashEffects && ((!buoyancyPoint.wasSubmergedLastFrame && flag3) || (!flag3 && buoyancyPoint.wasSubmergedLastFrame)) && doEffects)
			{
				Vector3 relativePointVelocity = rigidBody.GetRelativePointVelocity(localPosition);
				if (((Vector3)(ref relativePointVelocity)).magnitude > 1f)
				{
					string strName = ((waterImpacts != null && waterImpacts.Length != 0 && waterImpacts[0].isValid) ? waterImpacts[0].resourcePath : DefaultWaterImpact());
					((Vector3)(ref val2))._002Ector(Random.Range(-0.25f, 0.25f), 0f, Random.Range(-0.25f, 0.25f));
					Effect.server.Run(strName, pos + val2, Vector3.up);
					buoyancyPoint.nexSplashTime = Time.time + 0.25f;
				}
			}
			buoyancyPoint.wasSubmergedLastFrame = flag3;
		}
		if (points.Length != 0)
		{
			submergedFraction = (float)num3 / (float)points.Length;
		}
		if (InWater)
		{
			timeInWater += Time.fixedDeltaTime;
			timeOutOfWater = 0f;
		}
		else
		{
			timeOutOfWater += Time.fixedDeltaTime;
			timeInWater = 0f;
		}
	}

	public static void AccumulateFlowForce(ref Vector3 accumForce, in Vector3 pos, in WaterLevel.WaterInfo waterInfo, in float shoreDistance, ref float scaledBuoyancyForce, in bool flowForceDisabled, in float flowMovementScale)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (!waterInfo.artificalWater && !flowForceDisabled && (waterInfo.topology & 0x10000) == 0)
		{
			float num = Mathf.Clamp01(Mathf.InverseLerp(60f, 0f, shoreDistance));
			if (!(num <= Mathf.Epsilon))
			{
				num = Mathf.Pow(num, 0.5f);
				Vector3 flowDirection = GetFlowDirection(pos);
				scaledBuoyancyForce *= 0.025f * num;
				accumForce.x += flowDirection.x * scaledBuoyancyForce * flowMovementScale;
				accumForce.z += flowDirection.z * scaledBuoyancyForce * flowMovementScale;
			}
		}
	}

	private void InitPointArrays()
	{
		pointPositionArray = (Vector2[])(object)new Vector2[points.Length];
		pointPositionUVArray = (Vector2[])(object)new Vector2[points.Length];
		pointShoreDistanceArray = new float[points.Length];
		pointTerrainHeightArray = new float[points.Length];
		pointWaterHeightArray = new float[points.Length];
		initedPointArrays = true;
	}
}

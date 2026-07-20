using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

[ExecuteInEditMode]
public class EnvironmentVolume : MonoBehaviour, IPrefabPreProcess
{
	public enum VolumeShape
	{
		Cube,
		Sphere,
		Capsule
	}

	private static readonly Vector3[] volumeCorners = (Vector3[])(object)new Vector3[8]
	{
		new Vector3(-0.5f, -0.5f, -0.5f),
		new Vector3(0.5f, -0.5f, -0.5f),
		new Vector3(0.5f, 0.5f, -0.5f),
		new Vector3(-0.5f, 0.5f, -0.5f),
		new Vector3(-0.5f, -0.5f, 0.5f),
		new Vector3(0.5f, -0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, 0.5f),
		new Vector3(-0.5f, 0.5f, 0.5f)
	};

	[InspectorFlags]
	public EnvironmentType Type = EnvironmentType.Underground;

	[InspectorFlags]
	public NetworkGroupType NetworkType;

	public Vector3 Center = Vector3.zero;

	public Vector3 Size = Vector3.one;

	[NonSerialized]
	public float4x4 VolumeTransformation;

	[NonSerialized]
	public float4x4 VolumeTransformationInverse;

	[NonSerialized]
	public float3 VolumePosition;

	[NonSerialized]
	public Bounds VolumeBounds;

	[field: Tooltip("Controls the falloff amount of the positive axes of spatially aware volumes.")]
	[field: SerializeField]
	public Vector3 FalloffPositive { get; private set; } = Vector3.zero;

	[field: Tooltip("Controls the falloff amount of the negative axes of spatially aware volumes.")]
	[field: SerializeField]
	public Vector3 FalloffNegative { get; private set; } = Vector3.zero;

	[field: SerializeField]
	public VolumeShape SpatialVolumeShape { get; private set; }

	public float AmbientMultiplier { get; private set; }

	public float ReflectionMultiplier { get; private set; }

	public float CombinedMultiplier { get; private set; }

	public bool NoSunlight { get; private set; }

	public bool PropertiesCached { get; private set; }

	[field: SerializeField]
	public bool IsDynamic { get; private set; }

	public Collider trigger { get; private set; }

	public bool IsSpatialVolume => (Type & EnvironmentType.SpatiallyAware) != 0;

	bool IPrefabPreProcess.CanRunDuringBundling => false;

	private void OnValidate()
	{
		PropertiesCached = false;
		UpdateVolumeTransformationAndBounds();
	}

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (clientside && IsSpatialVolume && !((Object)(object)((Component)this).gameObject == (Object)null) && (Object)(object)((Component)this).GetComponent<EnvironmentVolumeLOD>() == (Object)null)
		{
			((Component)this).gameObject.AddComponent<EnvironmentVolumeLOD>();
		}
	}

	protected void Awake()
	{
		UpdateTrigger();
	}

	protected void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)trigger) && !trigger.enabled)
		{
			trigger.enabled = true;
		}
		UpdateVolumeTransformationAndBounds();
		if (IsDynamic && Object.op_Implicit((Object)(object)SingletonComponent<EnvironmentManager>.Instance))
		{
			SingletonComponent<EnvironmentManager>.Instance.RegisterDynamicVolume(this);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void UpdateVolumeTransformationAndBounds()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		EnvironmentVolumeMath.UpdateVolumeTransformationAndBoundsBurst(float3.op_Implicit(Size), float3.op_Implicit(Center), float4x4.op_Implicit(((Component)this).transform.localToWorldMatrix), IsSpatialVolume && SpatialVolumeShape == VolumeShape.Capsule, out VolumeTransformation, out VolumeTransformationInverse, out VolumePosition, out VolumeBounds);
	}

	protected void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)trigger) && trigger.enabled)
		{
			trigger.enabled = false;
		}
		if (IsDynamic && Object.op_Implicit((Object)(object)SingletonComponent<EnvironmentManager>.Instance))
		{
			SingletonComponent<EnvironmentManager>.Instance.UnregisterDynamicVolume(this);
		}
	}

	public void CacheVolumeProperties(EnvironmentVolumePropertiesCollection properties)
	{
		if (!PropertiesCached)
		{
			PropertiesCached = true;
			NoSunlight = (Type & EnvironmentType.NoSunlight) != 0 || (Type & EnvironmentType.TrainTunnels) != 0;
			CombinedMultiplier = AmbientMultiplier * ReflectionMultiplier;
		}
	}

	public void UpdateTrigger()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)trigger))
		{
			trigger = ((Component)this).gameObject.GetComponent<Collider>();
		}
		if (!Object.op_Implicit((Object)(object)trigger))
		{
			trigger = (Collider)(object)((Component)this).gameObject.AddComponent<BoxCollider>();
		}
		trigger.isTrigger = true;
		Collider obj = trigger;
		BoxCollider val = (BoxCollider)(object)((obj is BoxCollider) ? obj : null);
		if (Object.op_Implicit((Object)(object)val))
		{
			val.center = Center;
			val.size = Size;
		}
	}
}

using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Rust.Ai.Gen2;

public class SenseComponent : EntityComponent<BaseEntity>, IServerComponent
{
	[Serializable]
	public struct Cone(float halfAngle = 80f, float range = 10f)
	{
		public float halfAngle = halfAngle;

		public float range = range;
	}

	public class VisibilityStatus : IPooled
	{
		private const float maxPredictionTime = 1f;

		private bool isFirstAware;

		private BaseEntity baseEntity;

		private BaseEntity targetEntity;

		public Vector3 lastKnownPosition;

		public Vector3 predictedPosition;

		private const float maxClarity = 2f;

		private const float waterCheckInterval = 1f;

		private double? lastTimeInWaterUpdated;

		private double? lastTimeSurprised;

		public float clarity { get; private set; }

		public bool IsAware => clarity >= 1f;

		public float Accuracy
		{
			get
			{
				if (!IsAware)
				{
					return 0f;
				}
				return Mathx.RemapValClamped(clarity, 1f, 2f, 0f, 1f);
			}
		}

		public float timeVisible { get; private set; }

		public float timeNotVisible { get; private set; }

		public bool IsVisible => timeVisible > 0f;

		public float timeAwareAndVisible { get; private set; }

		public float timeNotAwareAndVisible { get; private set; }

		public float timeWatched { get; private set; }

		public float timeNotWatched { get; private set; }

		public float timeAimedAt { get; private set; }

		public float timeNotAimedAt { get; private set; }

		public WaterLevel.WaterInfo? lastWaterInfo { get; private set; }

		public bool isInWaterCached
		{
			get
			{
				//IL_0093: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Unknown result type (might be due to invalid IL or missing references)
				//IL_008c: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
				if (!targetEntity.ToNonNpcPlayer(out var player))
				{
					return false;
				}
				if (!lastWaterInfo.HasValue || !lastTimeInWaterUpdated.HasValue || Time.timeAsDouble - lastTimeInWaterUpdated > 1.0)
				{
					Vector3 val = (BaseNetworkableEx.Is<BaseMountable>((Object)(object)player.GetMounted(), out BaseMountable _) ? (Vector3.down * 0.5f) : Vector3.zero);
					lastWaterInfo = WaterLevel.GetWaterInfo(((Component)targetEntity).transform.position + val, waves: false, volumes: false);
					lastTimeInWaterUpdated = Time.timeAsDouble;
				}
				return lastWaterInfo.Value.currentDepth >= 0.3f;
			}
		}

		public bool IsCamping { get; private set; }

		public bool TryConsumeSurprise()
		{
			if (!lastTimeSurprised.HasValue)
			{
				return false;
			}
			lastTimeSurprised = null;
			return true;
		}

		private void Reset()
		{
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			isFirstAware = true;
			targetEntity = null;
			baseEntity = null;
			timeAwareAndVisible = 0f;
			timeNotAwareAndVisible = 100f;
			timeWatched = 0f;
			timeNotWatched = 100f;
			timeAimedAt = 0f;
			timeNotAimedAt = 100f;
			timeVisible = 0f;
			timeNotVisible = 100f;
			lastKnownPosition = Vector3.zero;
			predictedPosition = Vector3.zero;
			lastWaterInfo = null;
			lastTimeInWaterUpdated = null;
			lastTimeSurprised = null;
			IsCamping = false;
			clarity = 0f;
		}

		public void EnterPool()
		{
			Reset();
		}

		public void LeavePool()
		{
			Reset();
		}

		public static VisibilityStatus GetFromPool(BaseEntity baseEntity, BaseEntity targetEntity, bool isVisible, float deltaTime, float clarityGainSpeed, Vector3? lastKnownPositionOverride = null, float? minClarity = null)
		{
			VisibilityStatus visibilityStatus = Pool.Get<VisibilityStatus>();
			visibilityStatus.baseEntity = baseEntity;
			visibilityStatus.targetEntity = targetEntity;
			visibilityStatus.UpdateVisibility(isVisible, deltaTime, clarityGainSpeed, lastKnownPositionOverride, minClarity);
			return visibilityStatus;
		}

		private bool CheckValid()
		{
			if (!baseEntity.IsValid() || !targetEntity.IsValid())
			{
				if (AI.logIssues)
				{
					Debug.LogError((object)$"SenseComponent:UpdateVisibility NRE: {baseEntity} {targetEntity}");
				}
				return false;
			}
			return true;
		}

		public void UpdateVisibility(bool newVisibility, float deltaTime, float clarityGainSpeed, Vector3? lastKnownPositionOverride = null, float? minClarity = null)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0236: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_024d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0285: Unknown result type (might be due to invalid IL or missing references)
			//IL_028a: Unknown result type (might be due to invalid IL or missing references)
			//IL_028c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0291: Unknown result type (might be due to invalid IL or missing references)
			//IL_0293: Unknown result type (might be due to invalid IL or missing references)
			//IL_0297: Unknown result type (might be due to invalid IL or missing references)
			//IL_0265: Unknown result type (might be due to invalid IL or missing references)
			//IL_026a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0273: Unknown result type (might be due to invalid IL or missing references)
			//IL_0278: Unknown result type (might be due to invalid IL or missing references)
			if (!CheckValid())
			{
				return;
			}
			bool isAware = IsAware;
			Vector3 val = lastKnownPosition;
			if (minClarity.HasValue)
			{
				clarity = Mathf.Max(clarity, minClarity.Value);
			}
			else if (clarityGainSpeed > 0f)
			{
				clarity += clarityGainSpeed * (deltaTime / 1f);
			}
			else
			{
				clarity -= deltaTime / 3f;
			}
			clarity = Mathf.Clamp(clarity, 0f, 2f);
			bool flag = clarity >= 1f;
			if (lastKnownPositionOverride.HasValue)
			{
				lastKnownPosition = lastKnownPositionOverride.Value;
				predictedPosition = lastKnownPositionOverride.Value;
			}
			else if (newVisibility & flag)
			{
				lastKnownPosition = ((Component)targetEntity).transform.position;
				predictedPosition = ((Component)targetEntity).transform.position;
			}
			if (timeNotAwareAndVisible < 1f)
			{
				predictedPosition = ((Component)targetEntity).transform.position;
			}
			if (isFirstAware || ((!isAware & flag) && ShouldBeSurprised(val)))
			{
				lastTimeSurprised = Time.timeAsDouble;
			}
			if (lastTimeSurprised.HasValue && Time.timeAsDouble - lastTimeSurprised.Value > 3.0)
			{
				lastTimeSurprised = null;
			}
			if (((!isFirstAware && !isAware) & flag) && timeNotVisible >= 2f && timeNotVisible < 15f)
			{
				float num = Vector3.Distance(val, lastKnownPosition);
				IsCamping = num < 6f;
			}
			if (newVisibility)
			{
				timeNotVisible = 0f;
				timeVisible += deltaTime;
			}
			else
			{
				timeVisible = 0f;
				timeNotVisible += deltaTime;
				timeAwareAndVisible = 0f;
				timeNotAwareAndVisible += deltaTime;
			}
			if (flag)
			{
				if (newVisibility)
				{
					timeNotAwareAndVisible = 0f;
					timeAwareAndVisible += deltaTime;
				}
				Vector3 val2 = ((Component)targetEntity).transform.forward;
				Vector3 position = ((Component)targetEntity).transform.position;
				if (targetEntity.ToNonNpcPlayer(out var player))
				{
					val2 = player.eyes.HeadForward();
					position = player.eyes.position;
				}
				Vector3 val3 = ((Component)baseEntity).transform.position - position;
				float num2 = Mathf.Acos(Vector3.Dot(val2, ((Vector3)(ref val3)).normalized)) * 57.29578f * 2f;
				bool num3 = num2 < AI.watchedAngle;
				if (num3)
				{
					timeNotWatched = 0f;
					timeWatched += deltaTime;
				}
				else
				{
					timeWatched = 0f;
					timeNotWatched += deltaTime;
				}
				if (num3 && (Object)(object)player != (Object)null && player.modelState.aiming && num2 < AI.aimedAtAngle && !(player.GetHeldEntity() is BaseMelee { canScareAiWhenAimed: false }))
				{
					timeNotAimedAt = 0f;
					timeAimedAt += deltaTime;
				}
				else
				{
					timeAimedAt = 0f;
					timeNotAimedAt += deltaTime;
				}
				isFirstAware = false;
			}
			else
			{
				timeWatched = 0f;
				timeNotWatched += deltaTime;
				timeAimedAt = 0f;
				timeNotAimedAt += deltaTime;
			}
		}

		private bool ShouldBeSurprised(Vector3 previousLastKnownPosition)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			if (timeNotAwareAndVisible <= 4f)
			{
				return false;
			}
			if (Vector3.Angle(((Component)baseEntity).transform.forward, lastKnownPosition - ((Component)baseEntity).transform.position) > 45f)
			{
				return true;
			}
			if (Vector3.Distance(previousLastKnownPosition, lastKnownPosition) > 20f)
			{
				return true;
			}
			return false;
		}
	}

	[SerializeField]
	private Vector3 LongRangeVisionRectangle;

	[SerializeField]
	private Cone ShortRangeVisionCone;

	[SerializeField]
	private float touchDistance;

	[SerializeField]
	private float noiseRangeMultiplier;

	[SerializeField]
	private float hearingRange;

	[SerializeField]
	private NPCTeam team;

	public ResettableFloat timeToForgetSightings;

	private const float timeToForgetNoises = 5f;

	private static HashSet<BaseEntity> entitiesUpdatedThisFrame = new HashSet<BaseEntity>();

	[ServerVar]
	public static float minRefreshIntervalSeconds = 0.2f;

	[ServerVar]
	public static float maxRefreshIntervalSeconds = 1f;

	private double? _lastTickTime;

	private double nextRefreshTime;

	private double spawnTime;

	private Dictionary<BaseEntity, double> _alliesWeAreAwareOf;

	private Dictionary<BaseEntity, VisibilityStatus> entitiesWeAreAwareOf;

	private BaseEntity Target;

	private RustNavMeshAgent _agent;

	private static readonly float lookDistanceThresholdSq = Mathf.Pow(15f, 2f);

	private static readonly float lookBehindDotThreshold = Mathf.Cos(MathF.PI / 3f);

	public static readonly Dictionary<NpcNoiseIntensity, float> noiseRadii = new Dictionary<NpcNoiseIntensity, float>
	{
		{
			NpcNoiseIntensity.None,
			0f
		},
		{
			NpcNoiseIntensity.Low,
			10f
		},
		{
			NpcNoiseIntensity.Medium,
			20f
		},
		{
			NpcNoiseIntensity.High,
			50f
		}
	};

	private HashSet<NpcNoiseEvent> noises;

	[SerializeField]
	private float foodDetectionRange;

	private BaseEntity _nearestFood;

	[SerializeField]
	private float fireDetectionRange;

	[NonSerialized]
	public UnityEvent onFireMelee;

	private BaseEntity _nearestFire;

	private double? lastMeleeTime;

	[SerializeField]
	private float TargetingCooldown;

	[SerializeField]
	private float SwitchTargetToFocusAggressorCooldown;

	private LockState lockState;

	private double? lastTargetTime;

	private double? lastTimeSwitchedTargetToFocusAggressor;

	public float RefreshInterval
	{
		get
		{
			if (!ShouldRefreshFast)
			{
				return maxRefreshIntervalSeconds;
			}
			return minRefreshIntervalSeconds;
		}
	}

	private double LastTickTime
	{
		get
		{
			double valueOrDefault = _lastTickTime.GetValueOrDefault();
			if (!_lastTickTime.HasValue)
			{
				valueOrDefault = Time.timeAsDouble;
				_lastTickTime = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
		set
		{
			_lastTickTime = value;
		}
	}

	public bool HasPlayerInVicinity { get; private set; }

	public bool ShouldRefreshFast
	{
		get
		{
			if (!HasPlayerInVicinity)
			{
				if ((Object)(object)Target != (Object)null)
				{
					return Target.IsNonNpcPlayer();
				}
				return false;
			}
			return true;
		}
	}

	public Vector3 EyeOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			return EyePosition - ((Component)base.baseEntity).transform.position;
		}
	}

	private RustNavMeshAgent Agent => _agent ?? (_agent = ((Component)base.baseEntity).GetComponent<RustNavMeshAgent>());

	public Vector3 EyePosition
	{
		get
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("SenseComponent:ClientEyePosition"))
			{
				if (!BaseNetworkableEx.Is<ScientistNPC2>((Object)(object)base.baseEntity, out ScientistNPC2 _))
				{
					return base.baseEntity.CenterPoint();
				}
				Vector3 val = PlayerEyes.EyeOffset;
				if (base.baseEntity.HasFlag(BaseEntity.Flags.Reserved5))
				{
					val += PlayerEyes.DuckOffset;
				}
				return ((Component)base.baseEntity).transform.position + val;
			}
		}
	}

	private bool IsInCombat => base.baseEntity.HasFlag(BaseEntity.Flags.Reserved3);

	private bool ChangedTargetRecently
	{
		get
		{
			if ((Object)(object)Target != (Object)null && lastTargetTime.HasValue)
			{
				return Time.timeAsDouble - lastTargetTime.Value < (double)TargetingCooldown;
			}
			return false;
		}
	}

	private bool SwitchedTargetToFocusAggressorRecently
	{
		get
		{
			if ((Object)(object)Target != (Object)null && lastTimeSwitchedTargetToFocusAggressor.HasValue)
			{
				return Time.timeAsDouble - lastTimeSwitchedTargetToFocusAggressor.Value < (double)SwitchTargetToFocusAggressorCooldown;
			}
			return false;
		}
	}

	public void GetPerceivedAllies(List<BaseEntity> allies)
	{
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			foreach (var (baseEntity2, _) in _alliesWeAreAwareOf)
			{
				if (!baseEntity2.IsValid() || (BaseNetworkableEx.Is<BaseCombatEntity>((Object)(object)baseEntity2, out BaseCombatEntity castedUnityObject) && castedUnityObject.IsDead()))
				{
					((List<BaseEntity>)(object)val).Add(baseEntity2);
				}
				else
				{
					allies.Add(baseEntity2);
				}
			}
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				_alliesWeAreAwareOf.Remove(item);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void GetInitialAllies(List<BaseEntity> allies)
	{
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			foreach (var (baseEntity2, num2) in _alliesWeAreAwareOf)
			{
				if (!baseEntity2.IsValid() || (BaseNetworkableEx.Is<BaseCombatEntity>((Object)(object)baseEntity2, out BaseCombatEntity castedUnityObject) && castedUnityObject.IsDead()))
				{
					((List<BaseEntity>)(object)val).Add(baseEntity2);
				}
				else if (!(num2 - spawnTime > (double)(maxRefreshIntervalSeconds * 2f)))
				{
					allies.Add(baseEntity2);
				}
			}
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				_alliesWeAreAwareOf.Remove(item);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static Vector3 GetEntityLineOfSightTestPoint(BaseEntity entity, bool ignoreCrouch = true)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return GetEntityLineOfSightTestPoint(entity, ((Component)entity).transform.position, ignoreCrouch);
	}

	public static Vector3 GetEntityLineOfSightTestPoint(BaseEntity entity, Vector3 entityPosition, bool ignoreCrouch = true)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (entity.ToNonNpcPlayer(out var player))
		{
			if (ignoreCrouch)
			{
				return entityPosition + PlayerEyes.EyeOffset.y * Vector3.up;
			}
			return entityPosition + (player.eyes.position - ((Component)player).transform.position);
		}
		return entityPosition + ((Bounds)(ref entity.bounds)).size.y * Vector3.up;
	}

	public bool FindTargetLKP(out Vector3 lkp, bool applyHeightOffset = false, bool predict = false, bool ignoreCrouch = true)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!FindTarget(out var target))
		{
			lkp = Vector3.zero;
			return false;
		}
		return FindLKP(target, out lkp, applyHeightOffset, predict, ignoreCrouch);
	}

	public bool FindLKP(BaseEntity entity, out Vector3 lkp, bool applyHeightOffset = false, bool predict = false, bool ignoreCrouch = true)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (!GetVisibilityStatus(entity, out var status))
		{
			lkp = Vector3.zero;
			return false;
		}
		if (status.IsVisible && status.IsAware)
		{
			lkp = ((Component)entity).transform.position;
		}
		else
		{
			lkp = (predict ? status.predictedPosition : status.lastKnownPosition);
		}
		if (applyHeightOffset)
		{
			lkp = GetEntityLineOfSightTestPoint(entity, lkp, ignoreCrouch);
		}
		return true;
	}

	public bool GetVisibilityStatus(BaseEntity entity, out VisibilityStatus status)
	{
		status = null;
		if (!CanTarget(entity))
		{
			return false;
		}
		if (!entitiesWeAreAwareOf.TryGetValue(entity, out status))
		{
			return false;
		}
		return true;
	}

	public bool Forget(BaseEntity entity)
	{
		if (!entitiesWeAreAwareOf.TryGetValue(entity, out var value))
		{
			return false;
		}
		entitiesWeAreAwareOf.Remove(entity);
		Pool.Free<VisibilityStatus>(ref value);
		return true;
	}

	public bool IsVisible(BaseEntity entity)
	{
		if (!GetVisibilityStatus(entity, out var status))
		{
			return false;
		}
		return status.IsVisible;
	}

	public void GetSeenEntities(List<BaseEntity> perceivedEntities)
	{
		using (TimeWarning.New("SenseComponent:GetSeenEntities"))
		{
			foreach (BaseEntity key in entitiesWeAreAwareOf.Keys)
			{
				if (IsVisible(key))
				{
					perceivedEntities.Add(key);
				}
			}
		}
	}

	public void GetOncePerceivedEntities(List<BaseEntity> perceivedEntities)
	{
		foreach (BaseEntity key in entitiesWeAreAwareOf.Keys)
		{
			if (GetVisibilityStatus(key, out var _))
			{
				perceivedEntities.Add(key);
			}
		}
	}

	public bool Trace(Vector3 source, Vector3 direction, out RaycastHit hitInfo, int layerMask, string debugCategory = "sight")
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Trace"))
		{
			return GamePhysics.Trace(new Ray(source, direction), 0f, out hitInfo, ((Vector3)(ref direction)).magnitude, layerMask, (QueryTriggerInteraction)0);
		}
	}

	public bool IsLineOccluded(Vector3 a, Vector3 b, int layerMask, string debugCategory = "sight")
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("IsLineOccluded"))
		{
			RaycastHit hitInfo;
			return Trace(a, b - a, out hitInfo, layerMask, debugCategory);
		}
	}

	public bool CanSeeFromAt(Vector3 potentialLocation, Vector3 targetLocation, string debugCategory = "sight")
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.Cross(Vector3Ex.NormalizeXZ(targetLocation - potentialLocation), Vector3.up) * 0.5f * 2f;
		if (IsLineOccluded(potentialLocation, targetLocation + val, 1218519041, debugCategory))
		{
			return !IsLineOccluded(potentialLocation, targetLocation - val, 1218519041, debugCategory);
		}
		return true;
	}

	public bool CanBeSeenAtFrom(Vector3 potentialLocation, Vector3 targetLocation, string debugCategory = "sight")
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.Cross(Vector3Ex.NormalizeXZ(targetLocation - potentialLocation), Vector3.up) * 0.5f * 2f;
		if (IsLineOccluded(targetLocation, potentialLocation + val, 1218519041, debugCategory))
		{
			return !IsLineOccluded(targetLocation, potentialLocation - val, 1218519041, debugCategory);
		}
		return true;
	}

	public Matrix4x4 GetEyeTransform()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SenseComponent:GetEyeTransform"))
		{
			Vector3 eyePosition = EyePosition;
			Quaternion val = ((Component)base.baseEntity).transform.rotation;
			if (FindTargetLKP(out var lkp, applyHeightOffset: true, predict: false, ignoreCrouch: false) && !Agent.overrideDirectionWS.HasValue && (!BaseNetworkableEx.Is<ScientistNPC2>((Object)(object)base.baseEntity, out ScientistNPC2 _) || !Agent.IsSprinting))
			{
				val = Quaternion.LookRotation(lkp - eyePosition, Vector3.up);
				float maxAngle = 90f;
				if (Vector3.Dot(val * Vector3.forward, ((Component)base.baseEntity).transform.forward) < 0f - lookBehindDotThreshold)
				{
					Vector3 val2 = lkp - ((Component)base.baseEntity).transform.position;
					maxAngle = ((!(((Vector3)(ref val2)).sqrMagnitude > lookDistanceThresholdSq)) ? 70f : 0f);
				}
				val = Clamp(((Component)base.baseEntity).transform.rotation, val, maxAngle);
			}
			return Matrix4x4.TRS(eyePosition, val, Vector3.one);
		}
	}

	public override void InitShared()
	{
		base.InitShared();
		spawnTime = Time.timeAsDouble;
	}

	public override void Hurt(HitInfo hitInfo)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SenseComponent:Hurt"))
		{
			BaseEntity initiator = hitInfo.Initiator;
			if (CanTarget(initiator))
			{
				Vector3 entityPositionGuess = ((Component)initiator).transform.position + Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.forward * 5f;
				SimulateSighting(initiator, entityPositionGuess);
				if ((Object)(object)Target == (Object)null)
				{
					TrySetTarget(initiator, bypassCooldown: false);
				}
				else if ((Object)(object)Target != (Object)(object)initiator && !SwitchedTargetToFocusAggressorRecently && Vector3.Distance(((Component)base.baseEntity).transform.position, ((Component)initiator).transform.position) < 50f && TrySetTarget(initiator))
				{
					lastTimeSwitchedTargetToFocusAggressor = Time.timeAsDouble;
				}
			}
		}
	}

	public void SimulateSighting(BaseEntity entity, Vector3 entityPositionGuess)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (entitiesWeAreAwareOf.TryGetValue(entity, out var value))
		{
			if (!value.IsVisible || !value.IsAware)
			{
				value.UpdateVisibility(newVisibility: false, 0.01f, 0f, entityPositionGuess, 1f);
			}
		}
		else
		{
			VisibilityStatus fromPool = VisibilityStatus.GetFromPool(base.baseEntity, entity, isVisible: false, 0.01f, 0f, entityPositionGuess, 1f);
			entitiesWeAreAwareOf.Add(entity, fromPool);
		}
	}

	public void Tick()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SenseComponent:Tick"))
		{
			double timeAsDouble = Time.timeAsDouble;
			if (timeAsDouble < nextRefreshTime)
			{
				return;
			}
			float deltaTime = (float)(timeAsDouble - LastTickTime);
			LastTickTime = timeAsDouble;
			HasPlayerInVicinity = false;
			entitiesUpdatedThisFrame.Clear();
			using (TimeWarning.New("SenseComponent:Tick:ProcessEntities"))
			{
				PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
				try
				{
					GetModifiedSenses(null, out var _, out var _, out var _, out var modLongVisionRectangle);
					BaseEntity.Query.Server.GetPlayersAndBrainsInSphere(((Component)base.baseEntity).transform.position, modLongVisionRectangle.z, (List<BaseEntity>)(object)val, BaseEntity.Query.DistanceCheckType.None);
					foreach (BaseEntity item in (List<BaseEntity>)(object)val)
					{
						if (!((Object)(object)item == (Object)(object)base.baseEntity))
						{
							if (item.IsNonNpcPlayer())
							{
								HasPlayerInVicinity = true;
							}
							if (InSameTeam(item) && !_alliesWeAreAwareOf.ContainsKey(item))
							{
								_alliesWeAreAwareOf.Add(item, timeAsDouble);
							}
							if (CanTarget(item))
							{
								UpdateEntityVisibility(item, deltaTime);
							}
						}
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			using (TimeWarning.New("SenseComponent:Tick:RemoveEntities"))
			{
				PooledList<BaseEntity> val2 = Pool.Get<PooledList<BaseEntity>>();
				try
				{
					((List<BaseEntity>)(object)val2).AddRange((IEnumerable<BaseEntity>)entitiesWeAreAwareOf.Keys);
					foreach (BaseEntity item2 in (List<BaseEntity>)(object)val2)
					{
						if (!entitiesWeAreAwareOf.TryGetValue(item2, out var value))
						{
							continue;
						}
						if (!CanTarget(item2))
						{
							if (Target.IsValid() && (Object)(object)Target == (Object)(object)item2)
							{
								ClearTarget(forget: false);
							}
							Forget(item2);
						}
						else if (!value.IsVisible && value.timeNotVisible > timeToForgetSightings.Value)
						{
							if (Target.IsValid() && (Object)(object)Target == (Object)(object)item2)
							{
								ClearTarget(forget: false);
							}
							Forget(item2);
						}
						else if (!entitiesUpdatedThisFrame.Contains(item2))
						{
							if (IsInCombat)
							{
								UpdateEntityVisibility(item2, deltaTime);
							}
							else if (value.IsVisible)
							{
								entitiesWeAreAwareOf[item2].UpdateVisibility(newVisibility: false, deltaTime, 0f);
							}
						}
					}
					entitiesUpdatedThisFrame.Clear();
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			TickHearing(deltaTime);
			TickFoodDetection(deltaTime);
			TickFireDetection(deltaTime);
			TickTargeting(deltaTime);
			nextRefreshTime = Time.timeAsDouble + (double)RefreshInterval;
		}
	}

	private void GetModifiedSenses(BaseEntity entity, out float modTouchDistance, out float modHalfAngle, out float modShortVisionRange, out Vector3 modLongVisionRectangle)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		modTouchDistance = touchDistance;
		modHalfAngle = ShortRangeVisionCone.halfAngle;
		modShortVisionRange = ShortRangeVisionCone.range;
		modLongVisionRectangle = LongRangeVisionRectangle;
		if (!((Object)(object)entity != (Object)null) || !entity.ToNonNpcPlayer(out var player))
		{
			return;
		}
		if (BaseNetworkableEx.Is<BaseVehicle>((Object)(object)player.GetMountedVehicle(), out BaseVehicle castedUnityObject))
		{
			switch (castedUnityObject.npcVisibilityCategory)
			{
			case BaseVehicle.NpcVisibilityCategory.QuiteObious:
				modTouchDistance = touchDistance * 6f;
				modShortVisionRange = LongRangeVisionRectangle.z;
				return;
			case BaseVehicle.NpcVisibilityCategory.VeryObvious:
				modTouchDistance = LongRangeVisionRectangle.z;
				modShortVisionRange = LongRangeVisionRectangle.z;
				return;
			case BaseVehicle.NpcVisibilityCategory.LikeNormalPlayer:
				return;
			}
			if (AI.logIssues)
			{
				Debug.LogError((object)$"SenseComponent:GetModifiedSenses: Unknown npcVisibilityCategory {castedUnityObject.npcVisibilityCategory} for vehicle {castedUnityObject}");
			}
		}
		else if (player.IsDucked())
		{
			modTouchDistance = ((Bounds)(ref base.baseEntity.bounds)).extents.z * 1.5f;
			modHalfAngle = ShortRangeVisionCone.halfAngle * 0.85f;
			modShortVisionRange = ShortRangeVisionCone.range * 0.5f;
			modLongVisionRectangle = Vector3.Scale(LongRangeVisionRectangle, new Vector3(3f, 0.5f, 0.5f));
		}
		else if (player.IsRunning())
		{
			modTouchDistance = touchDistance * 3f;
			modShortVisionRange = ShortRangeVisionCone.range * 1.3f;
			modLongVisionRectangle = LongRangeVisionRectangle * 1.15f;
		}
	}

	private bool IsInAnyRange(BaseEntity entity, out float clarity)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("IsInAnyRange"))
		{
			Matrix4x4 eyeTransform = GetEyeTransform();
			Vector3 position = ((Matrix4x4)(ref eyeTransform)).GetPosition();
			eyeTransform = GetEyeTransform();
			Vector3 val = ((Matrix4x4)(ref eyeTransform)).rotation * Vector3.forward;
			Vector3 entityLineOfSightTestPoint = GetEntityLineOfSightTestPoint(entity, ignoreCrouch: false);
			Vector3 val2 = entityLineOfSightTestPoint - position;
			float magnitude = ((Vector3)(ref val2)).magnitude;
			GetModifiedSenses(entity, out var modTouchDistance, out var modHalfAngle, out var modShortVisionRange, out var modLongVisionRectangle);
			clarity = 0f;
			float num = Vector3.Angle(val, ((Vector3)(ref val2)).normalized);
			if (magnitude < 1.2f)
			{
				clarity = 999f;
			}
			else if (num < modHalfAngle)
			{
				if (magnitude < modShortVisionRange)
				{
					float num2 = Mathx.RemapValClamped(num, modHalfAngle, 0f, 0f, 1f);
					float num3 = Mathx.RemapValClamped(magnitude, modShortVisionRange, touchDistance, 0f, 1f);
					clarity = (num2 + num3) * 0.5f;
				}
				else
				{
					clarity = Mathx.RemapValClamped(magnitude, modLongVisionRectangle.z, modShortVisionRange, 0f, 0.5f);
				}
			}
			else if (magnitude < modTouchDistance)
			{
				clarity = 1f;
				if (entity.ToNonNpcPlayer(out var player))
				{
					if (player.IsRunning())
					{
						clarity = 2f;
					}
					else if (player.IsDucked())
					{
						clarity = 0.5f;
					}
				}
			}
			if (magnitude < modTouchDistance)
			{
				return true;
			}
			if (num < modHalfAngle)
			{
				if (magnitude < modShortVisionRange)
				{
					if (IsSightLineBrokenBySmoke(position, entityLineOfSightTestPoint))
					{
						clarity = 0f;
						return false;
					}
					return true;
				}
				if ((IsInCombat || (TOD_Sky.Instance.IsDay && magnitude < modLongVisionRectangle.z)) && DistToLineYZ(position, val, ((Component)entity).transform.position) < modLongVisionRectangle.y * 0.5f && DistToLineXZ(position, val, ((Component)entity).transform.position) < modLongVisionRectangle.x * 0.5f)
				{
					if (IsSightLineBrokenBySmoke(position, entityLineOfSightTestPoint))
					{
						clarity = 0f;
						return false;
					}
					return true;
				}
			}
			clarity = 0f;
			return false;
		}
	}

	private bool IsSightLineBrokenBySmoke(Vector3 eyePos, Vector3 entityTestPoint)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			SingletonComponent<SmokeGrenadeManager>.Instance.GetSmokeAround(eyePos, 50f, (List<BaseEntity>)(object)val);
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				if (BaseNetworkableEx.Is<SmokeGrenade>((Object)(object)item, out SmokeGrenade castedUnityObject) && NpcCoverManager.SegmentSphereIntersection(eyePos, entityTestPoint, ((Component)castedUnityObject).transform.position, AI.smokeGrenadeNpcRadius))
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

	private static float DistToLine(Vector3 lineStart, Vector3 lineDir, Vector3 point)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.Cross(((Vector3)(ref lineDir)).normalized, point - lineStart);
		return ((Vector3)(ref val)).magnitude;
	}

	private static float DistToLineXZ(Vector3 lineStart, Vector3 lineDir, Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return DistToLine(Vector3Ex.WithY(lineStart, 0f), Vector3Ex.WithY(lineDir, 0f), Vector3Ex.WithY(point, 0f));
	}

	private static float DistToLineYZ(Vector3 lineStart, Vector3 lineDir, Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return DistToLine(Vector3Ex.WithX(lineStart, 0f), Vector3Ex.WithX(lineDir, 0f), Vector3Ex.WithX(point, 0f));
	}

	private void UpdateEntityVisibility(BaseEntity entity, float deltaTime)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		bool flag = IsInAnyRange(entity, out var clarity);
		clarity *= 1f / AI.npcReactionTime;
		clarity = Mathf.Max(clarity, 0.001f);
		if (flag && entity.ToNonNpcPlayer(out var player))
		{
			Vector3 entityLineOfSightTestPoint = GetEntityLineOfSightTestPoint(player, ignoreCrouch: false);
			flag = !IsLineOccluded(EyePosition, entityLineOfSightTestPoint, 1218519041);
		}
		if (!flag)
		{
			clarity = 0f;
		}
		if (entitiesWeAreAwareOf.TryGetValue(entity, out var value))
		{
			value.UpdateVisibility(flag, deltaTime, clarity);
			entitiesUpdatedThisFrame.Add(entity);
		}
		else if (flag)
		{
			VisibilityStatus fromPool = VisibilityStatus.GetFromPool(base.baseEntity, entity, isVisible: true, deltaTime, clarity);
			entitiesWeAreAwareOf.Add(entity, fromPool);
			entitiesUpdatedThisFrame.Add(entity);
		}
	}

	public bool InSameTeam(BaseEntity other)
	{
		if (team != null && BaseNetworkableEx.Is<SenseComponent>((Object)(object)((Component)other).GetComponent<SenseComponent>(), out SenseComponent castedUnityObject) && team == castedUnityObject.team)
		{
			return true;
		}
		return base.baseEntity.InSameNpcTeam(other);
	}

	private static Quaternion Clamp(Quaternion originalForward, Quaternion targetRotation, float maxAngle)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		float num = Quaternion.Angle(originalForward, targetRotation);
		if (num > maxAngle)
		{
			float num2 = maxAngle / num;
			return Quaternion.Slerp(originalForward, targetRotation, num2);
		}
		return targetRotation;
	}

	private void TickHearing(float deltaTime)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SenseComponent:TickHearing"))
		{
			if (noiseRangeMultiplier > 0f)
			{
				PooledList<NpcNoiseEvent> val = Pool.Get<PooledList<NpcNoiseEvent>>();
				try
				{
					SingletonComponent<NpcNoiseManager>.Instance.GetNoisesAround(((Component)base.baseEntity).transform.position, hearingRange, (List<NpcNoiseEvent>)(object)val);
					foreach (NpcNoiseEvent item in (List<NpcNoiseEvent>)(object)val)
					{
						if (!noises.Contains(item) && !((Object)(object)item.Initiator == (Object)(object)base.baseEntity) && CanTarget(item.Initiator) && !(Time.timeAsDouble - item.EventTime > 5.0))
						{
							if (!noiseRadii.TryGetValue(item.Intensity, out var value))
							{
								Debug.LogError((object)$"Unknown noise intensity: {item.Intensity}");
							}
							else if (!(Vector3.Distance(item.NoisePosition, ((Component)base.baseEntity).transform.position) > Mathf.Min(value * noiseRangeMultiplier, hearingRange)))
							{
								noises.Add(item);
								SimulateSighting(item.Initiator, item.GuessedInitiatorPosition);
							}
						}
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			PooledList<NpcNoiseEvent> val2 = Pool.Get<PooledList<NpcNoiseEvent>>();
			try
			{
				foreach (NpcNoiseEvent noise in noises)
				{
					if (!CanTarget(noise.Initiator) || Time.timeAsDouble - noise.EventTime > 5.0)
					{
						((List<NpcNoiseEvent>)(object)val2).Add(noise);
					}
				}
				foreach (NpcNoiseEvent item2 in (List<NpcNoiseEvent>)(object)val2)
				{
					noises.Remove(item2);
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
	}

	public bool FindMostRelevantNoise(out NpcNoiseEvent mostRelevantNoise)
	{
		using (TimeWarning.New("SenseComponent:FindMostRelevantPooledNoise"))
		{
			NpcNoiseEvent? npcNoiseEvent = null;
			foreach (NpcNoiseEvent noise in noises)
			{
				if (CanTarget(noise.Initiator) && !(Time.timeAsDouble - noise.EventTime > 5.0) && (!npcNoiseEvent.HasValue || noise.Intensity > npcNoiseEvent.Value.Intensity))
				{
					npcNoiseEvent = noise;
				}
			}
			if (npcNoiseEvent.HasValue)
			{
				mostRelevantNoise = npcNoiseEvent.Value;
				return true;
			}
			mostRelevantNoise = default(NpcNoiseEvent);
			return false;
		}
	}

	public void ForgetAllNoises()
	{
		noises.Clear();
	}

	public bool FindFood(out BaseEntity food)
	{
		if (!_nearestFood.IsValid() || _nearestFood.IsDestroyed || !SingletonComponent<NpcFoodManager>.Instance.Contains(_nearestFood))
		{
			food = null;
			return false;
		}
		food = _nearestFood;
		return true;
	}

	private void TickFoodDetection(float deltaTime)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SenseComponent:TickFoodDetection"))
		{
			_nearestFood = null;
			if (foodDetectionRange <= 0f)
			{
				return;
			}
			float num = foodDetectionRange * foodDetectionRange;
			float num2 = float.MaxValue;
			PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
			try
			{
				SingletonComponent<NpcFoodManager>.Instance.GetFoodAround(((Component)base.baseEntity).transform.position, foodDetectionRange, (List<BaseEntity>)(object)val);
				RustNavMeshAgent component = ((Component)base.baseEntity).GetComponent<RustNavMeshAgent>();
				foreach (BaseEntity item in (List<BaseEntity>)(object)val)
				{
					if (!NpcFoodManager.IsFoodImmobile(item) || (item is BaseCorpse baseCorpse && BaseNetworkableEx.Is<HeadDispenser>((Object)(object)((Component)baseCorpse).GetComponent<HeadDispenser>(), out HeadDispenser castedUnityObject) && BaseNetworkableEx.Is<BaseEntity>((Object)(object)castedUnityObject.SourceEntity.GetEntity(), out BaseEntity castedUnityObject2) && castedUnityObject2.InSameNpcTeam(base.baseEntity)))
					{
						continue;
					}
					if (!component.IsPositionOnNavmesh(((Component)item).transform.position, out var _))
					{
						SingletonComponent<NpcFoodManager>.Instance.Remove(item);
						continue;
					}
					Vector3 val2 = ((Component)item).transform.position - ((Component)base.baseEntity).transform.position;
					float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
					if (sqrMagnitude < num2 && sqrMagnitude < num)
					{
						_nearestFood = item;
						num2 = sqrMagnitude;
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public bool FindFire(out BaseEntity fire)
	{
		if (!_nearestFire.IsValid() || _nearestFire.IsDestroyed || !NpcFireManager.IsOnFire(_nearestFire))
		{
			_nearestFire = null;
		}
		fire = _nearestFire;
		return (Object)(object)fire != (Object)null;
	}

	private void TickFireDetection(float deltaTime)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SenseComponent:TickFireDetection"))
		{
			if (fireDetectionRange <= 0f)
			{
				return;
			}
			if ((Object)(object)Target != (Object)null && SingletonComponent<NpcFireManager>.Instance.DidMeleeWithFireRecently(base.baseEntity, Target, out var meleeTime) && (!lastMeleeTime.HasValue || meleeTime != lastMeleeTime.Value))
			{
				lastMeleeTime = meleeTime;
				onFireMelee.Invoke();
			}
			PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
			try
			{
				SingletonComponent<NpcFireManager>.Instance.GetFiresAround(((Component)base.baseEntity).transform.position, fireDetectionRange, (List<BaseEntity>)(object)val);
				BaseEntity baseEntity = null;
				float num = fireDetectionRange * fireDetectionRange;
				float num2 = float.MaxValue;
				foreach (BaseEntity item in (List<BaseEntity>)(object)val)
				{
					Vector3 val2 = ((Component)item).transform.position - ((Component)base.baseEntity).transform.position;
					float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
					if (sqrMagnitude < num2 && sqrMagnitude < num)
					{
						baseEntity = item;
						num2 = sqrMagnitude;
					}
				}
				if ((Object)(object)baseEntity != (Object)null)
				{
					_nearestFire = baseEntity;
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public LockState.LockHandle LockCurrentTarget()
	{
		return lockState.AddLock();
	}

	public bool UnlockTarget(ref LockState.LockHandle handle)
	{
		return lockState.RemoveLock(ref handle);
	}

	public bool CanTarget(BaseEntity entity)
	{
		if (!entity.IsValid())
		{
			return false;
		}
		if (entity.IsTransferProtected())
		{
			return false;
		}
		if (entity.IsDestroyed)
		{
			return false;
		}
		if (!entity.IsNonNpcPlayer() && !entity.IsNpc)
		{
			return false;
		}
		if (entity.IsNpcPlayer())
		{
			return false;
		}
		if (entity is BaseCombatEntity baseCombatEntity && baseCombatEntity.IsDead())
		{
			return false;
		}
		if (InSameTeam(entity))
		{
			return false;
		}
		if (entity is BasePlayer item)
		{
			if (AI.ignoreplayers)
			{
				return false;
			}
			if (SimpleAIMemory.PlayerIgnoreList.Contains(item))
			{
				return false;
			}
		}
		object obj = Interface.CallHook("IOnNpcTarget", this, entity);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return true;
	}

	public bool FindTarget(out BaseEntity target)
	{
		if (!CanTarget(Target))
		{
			ClearTarget();
			target = null;
			return false;
		}
		target = Target;
		return (Object)(object)target != (Object)null;
	}

	public bool FindTargetPosition(out Vector3 targetPosition)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!FindTarget(out var target))
		{
			targetPosition = Vector3.zero;
			return false;
		}
		targetPosition = ((Component)target).transform.position;
		return true;
	}

	public bool FindTargetStatus(out VisibilityStatus status)
	{
		status = null;
		if (!FindTarget(out var target))
		{
			return false;
		}
		if (!GetVisibilityStatus(target, out status))
		{
			return false;
		}
		return true;
	}

	public bool TrySetTarget(BaseEntity newTarget, bool bypassCooldown = true)
	{
		if (lockState.IsLocked)
		{
			return false;
		}
		if ((Object)(object)newTarget == (Object)null)
		{
			ClearTarget();
			return true;
		}
		if ((Object)(object)newTarget == (Object)(object)Target)
		{
			return true;
		}
		if (!CanTarget(newTarget))
		{
			return false;
		}
		if ((Object)(object)Target != (Object)null && !bypassCooldown && ChangedTargetRecently)
		{
			return false;
		}
		lastTargetTime = Time.timeAsDouble;
		Target = newTarget;
		return true;
	}

	public void ClearTarget(bool forget = true)
	{
		if (Target.IsValid())
		{
			if (forget)
			{
				Forget(Target);
			}
			lastTargetTime = null;
			Target = null;
		}
	}

	private void TickTargeting(float deltaTime)
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SenseComponent:TickTargeting"))
		{
			if ((Object)(object)Target != (Object)null && !CanTarget(Target))
			{
				ClearTarget();
			}
			if (((Object)(object)Target != (Object)null && SwitchedTargetToFocusAggressorRecently) || ((Object)(object)Target != (Object)null && ChangedTargetRecently))
			{
				return;
			}
			PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
			try
			{
				GetOncePerceivedEntities((List<BaseEntity>)(object)val);
				if (((List<BaseEntity>)(object)val).Count == 0)
				{
					return;
				}
				BaseEntity baseEntity = null;
				float num = float.NegativeInfinity;
				foreach (BaseEntity item in (List<BaseEntity>)(object)val)
				{
					if (GetVisibilityStatus(item, out var status) && status.IsAware && FindLKP(item, out var lkp, applyHeightOffset: false, predict: true))
					{
						float num2 = 0f;
						float num3 = base.baseEntity.Distance(lkp);
						if (status.IsVisible && num3 < 3f)
						{
							num2 += 1000f;
						}
						num2 += Mathx.RemapValClamped(status.timeNotVisible, 0f, 10f, 1f, 0f) * 100f;
						num2 += Mathx.RemapValClamped(num3, 0f, 50f, 1f, 0f);
						if (num2 > num)
						{
							num = num2;
							baseEntity = item;
						}
					}
				}
				if ((Object)(object)baseEntity != (Object)null)
				{
					TrySetTarget(baseEntity, bypassCooldown: false);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public SenseComponent()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		LongRangeVisionRectangle = new Vector3(6f, 30f, 60f);
		ShortRangeVisionCone = new Cone(100f, 30f);
		touchDistance = 6f;
		noiseRangeMultiplier = 1f;
		hearingRange = 50f;
		timeToForgetSightings = new ResettableFloat(30f);
		_alliesWeAreAwareOf = new Dictionary<BaseEntity, double>(3);
		entitiesWeAreAwareOf = new Dictionary<BaseEntity, VisibilityStatus>(8);
		noises = new HashSet<NpcNoiseEvent>();
		foodDetectionRange = 30f;
		fireDetectionRange = 20f;
		onFireMelee = new UnityEvent();
		TargetingCooldown = 5f;
		SwitchTargetToFocusAggressorCooldown = 5f;
		lockState = new LockState();
		base._002Ector();
	}
}

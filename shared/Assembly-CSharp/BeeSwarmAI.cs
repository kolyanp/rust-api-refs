using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Rust.Ai;
using UnityEngine;

public class BeeSwarmAI : BaseCombatEntity, ISplashable
{
	public class UpdateBeeSwarmThink : ObjectWorkQueue<BeeSwarmAI>
	{
		protected override void RunJob(BeeSwarmAI entity)
		{
			if (((ObjectWorkQueue<BeeSwarmAI>)this).ShouldAdd(entity))
			{
				entity.ThinkAI();
			}
		}

		protected override bool ShouldAdd(BeeSwarmAI entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.IsValid();
			}
			return false;
		}
	}

	[Header("Settings")]
	public float moveSpeed = 2f;

	public float stopThreshold = 0.2f;

	[Header("Animation")]
	public float ReductionAmount;

	public float ReductionDuration;

	public float Frequency;

	public ParticleSystem PSystem;

	public Light OnFireLight;

	public ParticleSystemForceField AngerForceField;

	private Vector3 pastPosition = Vector3.one;

	private Vector3 velocity = Vector3.zero;

	public const Flags IsAngry = Flags.Reserved12;

	public const Flags IsDying = Flags.Reserved13;

	public const Flags HasTarget = Flags.Reserved14;

	[ServerVar(Help = "How long a swarm will stick around without a target")]
	public static float killWithoutTargetTime = 150f;

	[ServerVar(Help = "How far away fire has to be to set the swarm on fire")]
	public static float flameSettingDistance = 5.5f;

	[ServerVar(Help = "How much water a player needs to be in to be ignored")]
	public static float waterThreshold = 0.6f;

	[ServerVar(Help = "Range to find new targets")]
	public static float searchRange = 10f;

	[ServerVar(Help = "Range to leave current target alone (should be higher than search)")]
	public static float breakRange = 15f;

	[ServerVar(Help = "How many milliseconds to spend on thinking per frame")]
	public static float think_budget_ms = 0.1f;

	[ServerVar]
	public static bool disable = false;

	public static Phrase BeeHelpPhrase = (Phrase)(object)new TokenisedPhrase("bee.tip.help", "Throw water, light a fire, or put on a hazmat suit to deal with the bees.");

	private BasePlayer targetPlayer;

	private TimeSince timeSinceHadTarget;

	private float targetPlayerLastWaterLevel;

	private bool hasCameFromAHive;

	public static UpdateBeeSwarmThink updateBeeSwarmThink = new UpdateBeeSwarmThink();

	private TimeSince timeSinceEgress;

	private Vector3 egressDirection = Vector3.one;

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
	}

	private void Update()
	{
		DoAI();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk)
		{
			Die();
		}
	}

	public override void OnAttacked(HitInfo info)
	{
		base.OnAttacked(info);
		if (base.isServer)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved12, b: true);
			}
		}
	}

	public override void ServerInit()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved12, b: true);
		}
		timeSinceHadTarget = TimeSince.op_Implicit(0f);
		InvokeRandomized(delegate
		{
			((ObjectWorkQueue<BeeSwarmAI>)updateBeeSwarmThink).Add(this);
		}, 0f, 1f, 0.05f);
	}

	public void SetHasCameFromAHive(bool cameFromHive)
	{
		hasCameFromAHive = cameFromHive;
	}

	public void SetTarget(BasePlayer ply)
	{
		targetPlayer = ply;
	}

	private void ThinkAI()
	{
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BeeSwarmAI.ThinkAI"))
		{
			if (!HasFlag(Flags.Reserved12) || disable)
			{
				return;
			}
			if (hasCameFromAHive)
			{
				if (AI.ignoreplayers || !AI.think)
				{
					return;
				}
			}
			else if (AI.effectaiweapons && (AI.ignoreplayers || !AI.think))
			{
				return;
			}
			if ((Object)(object)targetPlayer == (Object)null)
			{
				targetPlayer = FindTarget(((Component)this).transform);
				if ((Object)(object)targetPlayer != (Object)null)
				{
					targetPlayerLastWaterLevel = targetPlayer.metabolism.wetness.value;
					timeSinceHadTarget = TimeSince.op_Implicit(0f);
					using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
					flagsUpdateScope.Set(Flags.Reserved14, b: true);
				}
			}
			ValidateTarget();
			if (IsSmoke())
			{
				StartDie();
			}
			if (TimeSince.op_Implicit(timeSinceHadTarget) > killWithoutTargetTime)
			{
				StartDie();
			}
		}
	}

	private bool IsSmoke()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			SingletonComponent<SmokeGrenadeManager>.Instance.GetSmokeAround(((Component)this).transform.position, 5f, (List<BaseEntity>)(object)val);
			return val != null && ((List<BaseEntity>)(object)val).Count > 0;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DoAI()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BeeSwarmAI.DoAI"))
		{
			if (HasFlag(Flags.Reserved12) && !disable && !((Object)(object)targetPlayer == (Object)null))
			{
				SteerToPlayer();
				Quaternion val = Quaternion.LookRotation(((Component)targetPlayer).transform.position - ((Component)this).transform.position);
				((Component)this).transform.rotation = Quaternion.Slerp(((Component)this).transform.rotation, val, Time.deltaTime * 2f);
				if ((Object)(object)targetPlayer != (Object)null)
				{
					targetPlayerLastWaterLevel = targetPlayer.metabolism.wetness.value;
				}
			}
		}
	}

	private bool IsFire(out Vector3 firePosition)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			SingletonComponent<NpcFireManager>.Instance.GetFiresAround(GetTargetEyesPosition(), 2f, (List<BaseEntity>)(object)val);
			firePosition = Vector3.zero;
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				if (!((Object)(object)item == (Object)null) && !item.IsDestroyed)
				{
					if (item is FlameThrower && Vector3.Distance(((Component)this).transform.position, ((Component)item).transform.position) < flameSettingDistance)
					{
						SetOnFire();
					}
					firePosition = ((Component)item).transform.position;
				}
			}
			return ((List<BaseEntity>)(object)val).Count > 0;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public Vector3 GetTargetEyesPosition()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		Vector3 zero = Vector3.zero;
		if ((Object)(object)targetPlayer.eyes == (Object)null)
		{
			return ((Component)targetPlayer).transform.position + new Vector3(0f, 1f, 0f);
		}
		return targetPlayer.eyes.position;
	}

	private void ValidateTarget()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if ((Object)(object)targetPlayer == (Object)null || targetPlayer.InSafeZone() || targetPlayer.IsDead())
		{
			targetPlayer = null;
			flagsUpdateScope.Set(Flags.Reserved14, b: false);
			return;
		}
		if (targetPlayer.metabolism.wetness.value > waterThreshold || TimeSince.op_Implicit(targetPlayer.TimeSinceLastWaterSplash) < 1f || targetPlayer.WaterFactor() > 0.5f)
		{
			targetPlayer = null;
			flagsUpdateScope.Set(Flags.Reserved14, b: false);
			return;
		}
		if (Vector3.Distance(((Component)this).transform.position, ((Component)targetPlayer).transform.position) > breakRange)
		{
			targetPlayer = null;
			flagsUpdateScope.Set(Flags.Reserved14, b: false);
			return;
		}
		Vector3 p = ((Component)targetPlayer).transform.position + new Vector3(0f, 1f, 0f);
		if (!GamePhysics.LineOfSight(((Component)this).transform.position, p, 1218519041))
		{
			targetPlayer = null;
			flagsUpdateScope.Set(Flags.Reserved14, b: false);
		}
	}

	private void SetOnFire()
	{
		if (!HasFlag(Flags.OnFire))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved14, b: false);
				flagsUpdateScope.Set(Flags.OnFire, b: true);
			}
			Invoke(StartDie, 1f);
		}
	}

	private void SteerToPlayer()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 targetEyesPosition = GetTargetEyesPosition();
		Vector3 localTarget = ((Component)this).transform.InverseTransformPoint(targetEyesPosition);
		if (IsFire(out var firePosition))
		{
			float num = 5f;
			Vector3 val = ((Component)this).transform.position - firePosition;
			if (((Vector3)(ref val)).magnitude < num)
			{
				((Vector3)(ref val)).Normalize();
				Transform transform = ((Component)this).transform;
				transform.position += val * moveSpeed * Time.deltaTime;
				return;
			}
		}
		if (!(Mathf.Abs(((Vector3)(ref localTarget)).magnitude) <= stopThreshold))
		{
			SteerToTarget(localTarget);
		}
	}

	private void SteerToTarget(Vector3 localTarget)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp(localTarget.y, -1f, 1f);
		float num2 = Mathf.Clamp(localTarget.x, -1f, 1f);
		Vector3 val = ((Component)this).transform.forward + ((Component)this).transform.right * num2 + ((Component)this).transform.up * num;
		((Vector3)(ref val)).Normalize();
		Transform transform = ((Component)this).transform;
		transform.position += val * moveSpeed * Time.deltaTime;
	}

	private void StartDie()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		if (!HasFlag(Flags.Reserved13))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved12, b: false);
				flagsUpdateScope.Set(Flags.Reserved13, b: true);
			}
			timeSinceEgress = TimeSince.op_Implicit(0f);
			int num = ((Random.Range(0, 2) != 0) ? 1 : (-1));
			int num2 = ((Random.Range(0, 2) != 0) ? 1 : (-1));
			egressDirection = Vector3.right * (float)num + Vector3.forward * (float)num2;
			InvokeRepeating(Egress, 0f, 0f);
		}
	}

	public static BasePlayer FindTarget(Transform transform)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BeeSwarmAI.FindTarget"))
		{
			PooledList<BasePlayer> val = Pool.Get<PooledList<BasePlayer>>();
			try
			{
				Query.Server.GetPlayersInSphere(transform.position, searchRange, (List<BasePlayer>)(object)val, Query.DistanceCheckType.OnlyCenter, includeHumanoidNpcs: true);
				BasePlayer result = null;
				float num = float.MaxValue;
				foreach (BasePlayer item in (List<BasePlayer>)(object)val)
				{
					if (SimpleAIMemory.PlayerIgnoreList.Contains(item) || item.InSafeZone() || item.IsInTutorial || item.IsDead() || item.metabolism.wetness.value > waterThreshold)
					{
						continue;
					}
					Vector3 p = ((Component)item).transform.position + new Vector3(0f, 1f, 0f);
					if (GamePhysics.LineOfSight(transform.position, p, 1218519041))
					{
						Vector3 val2 = ((Component)item).transform.position - transform.position;
						float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
						if (sqrMagnitude < num)
						{
							num = sqrMagnitude;
							result = item;
						}
					}
				}
				return result;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	private bool IsUnderWater(BasePlayer ply)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Abs(WaterSystem.OceanLevel - GetTargetEyesPosition().y) > 1f;
	}

	private void Egress()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.rotation = Quaternion.LookRotation(egressDirection);
		SteerToTarget(egressDirection);
		if (TimeSince.op_Implicit(timeSinceEgress) > 10f)
		{
			Die();
		}
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		if ((Object)(object)splashType == (Object)null || splashType.shortname == null)
		{
			return false;
		}
		if (HasFlag(Flags.Reserved13))
		{
			return false;
		}
		if (amount > 0)
		{
			return true;
		}
		return false;
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		float num = base.health - 10f;
		if (num > 0f)
		{
			Hurt(num);
		}
		if (base.health <= 10f)
		{
			StartDie();
		}
		return amount;
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using ConVar;
using Development.Attributes;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust.Ai.Gen2.Nav;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Rust.Ai.Gen2;

public class LivestockAnimal : BaseNPC2, ISimpleHearingReceiver, INaturalDeathPose, IFSMDebugInfo, ISenseObserver
{
	public enum DayActivity
	{
		Idling,
		Grazing,
		Resting,
		Other
	}

	public enum TrustTier
	{
		Stranger,
		Known,
		Tolerated,
		Bonded,
		Herd
	}

	private struct Acquaintance
	{
		public ulong userId;

		public float seconds;

		public TimeSince sinceCredited;
	}

	public enum FamiliarityReason
	{
		Handled,
		Purchased,
		Debug
	}

	private struct CullCandidate : IComparable<CullCandidate>
	{
		public readonly LivestockAnimal Animal;

		private readonly int group;

		private readonly float worth;

		public CullCandidate(LivestockAnimal animal)
		{
			Animal = animal;
			BuildingPrivlidge cupboard;
			if (!animal.IsFullyBonded)
			{
				group = 0;
			}
			else if (!animal.TryGetHomeCupboard(out cupboard))
			{
				group = 1;
			}
			else
			{
				group = 2;
			}
			worth = animal.SaleValueScale;
		}

		public int CompareTo(CullCandidate other)
		{
			if (group != other.group)
			{
				int num = group;
				return num.CompareTo(other.group);
			}
			float num2 = worth;
			return num2.CompareTo(other.worth);
		}
	}

	public enum AgeStage
	{
		Infant,
		Adult
	}

	public enum SexForm
	{
		Either,
		Female,
		Male
	}

	public enum LivestockSize
	{
		Large = 1,
		Small
	}

	[Flags]
	public enum LivestockSizeFlags
	{
		Large = 1,
		Small = 2
	}

	public enum Need
	{
		Fullness,
		PersonalSpace,
		Hydration
	}

	public struct TickableNeed(float startingValue)
	{
		public float NeedValue = startingValue;

		public float NextTick = 0f;

		[PoolAnalyzerGetWrapper]
		public LivestockNeed ToSavedData()
		{
			LivestockNeed val = Pool.Get<LivestockNeed>();
			val.needValue = NeedValue;
			val.needTick = NextTick;
			return val;
		}

		public void LoadFrom(LivestockNeed need)
		{
			NeedValue = need.needValue;
			NextTick = need.needTick;
		}
	}

	public class LivestockNeedsQueue : PersistentObjectWorkQueue<LivestockAnimal>
	{
		protected override void RunJob(LivestockAnimal entity)
		{
			if ((Object)(object)entity == (Object)null || entity.IsDestroyed)
			{
				return;
			}
			using (LivestockProfiler.Sample(LivestockProfiler.Section.Needs))
			{
				entity.TickNeeds();
				if (!entity.IsDead())
				{
					entity.TickFamiliarity();
					entity.TickFrailty();
					entity.TickLead();
				}
			}
		}
	}

	private enum DebugValue
	{
		Age,
		Fullness,
		Hydration,
		PersonalSpace,
		Mood,
		Familiarity,
		Count
	}

	private struct FlashedValue
	{
		private float lastShown;

		private TimeSince sinceMoved;

		private int direction;

		private bool sampled;

		public int Sample(float value, float precision)
		{
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			float showstateflashduration = AI.showstateflashduration;
			float num = Mathf.Round(value / Mathf.Max(precision, 0.0001f));
			if (!sampled || showstateflashduration <= 0f)
			{
				sampled = true;
				lastShown = num;
				direction = 0;
				return 0;
			}
			if (!Mathf.Approximately(num, lastShown))
			{
				direction = ((num > lastShown) ? 1 : (-1));
				lastShown = num;
				sinceMoved = TimeSince.op_Implicit(0f);
			}
			else if (TimeSince.op_Implicit(sinceMoved) > showstateflashduration)
			{
				direction = 0;
			}
			return direction;
		}
	}

	private PersistentTimer ageTimer;

	private LivestockPattern pattern;

	private LivestockSexParts sexParts;

	private bool breedingPaused;

	private float conditionSecondsSpent;

	private float conditionSecondsSkipped;

	public const float ConditionHealthStep = 0.05f;

	private float conditionUnsentHealth;

	[Header("Day activity")]
	[Tooltip("Fullness at or below this is starving rather than peckish. A starving animal gets up from a lie down and breaks off a curious walk to go and eat.")]
	[Range(0f, 1f)]
	public float StarvingThreshold = 0.2f;

	[Tooltip("How many seconds of waking time the day activity read out is measured over. It only reports how the day split up, nothing reads it to decide what to do.")]
	public float DayBudgetWindow = 300f;

	private float[] activityWeights;

	[Tooltip("The item dropped each time the animal dungs. Leave empty to disable it, which is how an age stage that shouldn't dung - a calf, say - opts out.")]
	[Header("Dung")]
	public ItemDefinition DungItem;

	[Tooltip("How long (in seconds) between dung drops. Zero disables it.")]
	public float DungInterval;

	[Tooltip("Random variance applied to DungInterval, as a fraction of it. 0.2 means +/-20%, so a herd doesn't drop in lockstep.")]
	[Range(0f, 1f)]
	public float DungIntervalVariance = 0.2f;

	[Tooltip("Where the dung drops from. Falls back to the animal's origin when unset.")]
	public Transform DungSpawnPoint;

	private PersistentTimer dungTimer;

	public const int MaxAcquaintances = 8;

	public const int TrustTiersAboveStranger = 4;

	private Acquaintance[] acquaintances;

	private Acquaintance probation;

	private TimeSince probationCredited;

	private const float ProbationTimeout = 60f;

	private TimeSince lastFamiliarityTick;

	private EntityRef<BasePlayer> trustedNearby;

	private float trustedNearbyDistanceSqr;

	private TimeSince trustedNearbySeen;

	private EntityRef<BasePlayer> curiousAbout;

	private const float CuriousMinFullness = 0.7f;

	private float lastDefendedAttack = float.NegativeInfinity;

	[Range(0f, 1f)]
	[Header("Old age")]
	[Tooltip("How far through its life an animal drops a whole gait, so it jogs where it used to run. A tier rather than a multiplier: the animations are authored at the gait speeds, so anything in between reads as sliding.")]
	public float OldAgeSlowFraction = 0.55f;

	[Range(0f, 1f)]
	[Tooltip("How far through its life it drops a second gait. Never goes below a walk.")]
	public float OldAgeVerySlowFraction = 0.8f;

	[Range(0.05f, 1f)]
	[Tooltip("How much of its roaming range an animal still has at the very end of its life, so an old one keeps close to home and mostly only moves to get back to it.")]
	public float OldAgeRangeFactor = 0.15f;

	[Range(1f, 10f)]
	[Tooltip("How much longer an old animal rests than one in its prime. 4 means a lie down at the end of its life lasts four times as long.")]
	public float OldAgeRestDurationScale = 4f;

	private RustNavMeshAgent frailtyAgent;

	private LivestockAnimalFSM frailtyFsm;

	private Vector3 homePosition;

	private const float AdoptionCheckInterval = 5f;

	private TimeSince lastAdoptionCheck;

	[Tooltip("How much hydration refills per second while the animal has its head in the water.")]
	[Header("Hydration")]
	public float HydrationFillRate = 0.25f;

	[Range(0f, 1f)]
	[Tooltip("Where a freshly spawned animal's hydration starts. Full, so a new animal has one thing less to go and sort out before it settles in.")]
	public float StartingHydration = 1f;

	[Tooltip("How much hydration drains per second while the animal is up and about in daylight. Tiny, because one mouthful refills the animal completely: this sets how often it goes for a drink rather than how long it stands there. At 0.00001 a full animal is down to the ConsumeThreshold of 0.95 after about 5000 seconds of daylight, which is one trip to the water in a shipped waking day.")]
	public float HydrationDecayRate = 1E-05f;

	[Tooltip("Shallowest water (in metres) that counts as somewhere to drink from. Above zero so a dry riverside tile with the water still 10m away doesn't qualify.")]
	public float MinStandingWaterDepth = 0.05f;

	[Tooltip("Deepest water (in metres) the animal will stand in to drink, so it drinks from the edge rather than wading out into the middle of the river.")]
	public float MaxStandingWaterDepth = 0.5f;

	[Tooltip("How much water a drink from completely dry takes out of a barrel. A drink takes its share of this for the hydration it puts back, so topping up often costs no more water than waiting until empty. 200 makes the daily top up from 0.95 cost 10.")]
	public int BarrelWaterPerDrink = 200;

	public const int MaxGeneration = 20;

	private const string EnglishSecond = "{0} Junior";

	private const string EnglishOrdinal = "{0} the {1}";

	private const string StemMark = "{0}";

	private static readonly HashSet<string> reportedLongNames = new HashSet<string>();

	private static readonly Dictionary<LivestockSpecies, int> populationBySpecies = new Dictionary<LivestockSpecies, int>();

	[Header("Sleep")]
	[Tooltip("How long (in seconds) after dusk this animal can take to settle, and after dawn to rise. Each animal rolls its own point in that window, so a herd beds down over a spread rather than dropping on one frame. Zero puts the whole herd on the same clock.")]
	public float SleepTimeVariance = 45f;

	[Tooltip("How far (in metres) from the middle of the herd is close enough to lie down. Further out than this and the animal walks in first, so the herd sleeps as a group.")]
	public float HerdGatherDistance = 6f;

	[Tooltip("How long (in seconds) the lie down animation runs before the animal is actually on the ground. Setting the resting flag only STARTS it, so for this long the animal is flagged lying down while still visibly on its feet. The cow's cow_lay_down is 5.2s.")]
	public float LieDownDuration = 5.2f;

	[Tooltip("The same for bedding down, which is the lie down plus the settle into sleep. The cow's cow_lay_down into cow_sleep_in is 8.2s.")]
	public float BedDownDuration = 8.2f;

	[Tooltip("How long (in seconds) the animator takes to stand up once the lying flag clears, after it has finished whatever was left of the lie down. The cow's cow_stand_up is 3s.")]
	public float StandUpDuration = 3f;

	[Tooltip("The same out of a sleep, which is the wake and then the stand up. The cow's cow_wake_up into cow_stand_up is 6.5s.")]
	public float WakeUpDuration = 6.5f;

	private float sleepFraction;

	[Tooltip("How far (in metres) the animal looks for herd mates. Herd identity rather than personal space: the regroup, the flee centroid and the distress broadcast all use it.")]
	[Header("PersonalSpace")]
	public float SocialSearchRadius = 20f;

	[Tooltip("How many herd mates of the same species within Social Search Radius the animal is content surrounded by. Past this the herd is too big to keep producing at full rate.")]
	public int ComfortableHerdSize = 8;

	[Tooltip("How many herd mates it takes for the herd load pressure to bottom out. Herd size alone never reaches the neglect band, so a big provisioned herd stops growing rather than dying.")]
	public int CrowdedHerdSize = 12;

	[Tooltip("How close (in metres) another animal has to stand to count as packed against this one. Measured flat, with a height filter, so a herd on the floor above is not in here.")]
	public float PackingRadius = 2.5f;

	[Tooltip("How far apart (in metres) two animals have to be vertically before neither is packed against the other. Under a storey, so a barn's two floors are separate spaces.")]
	public float PackingHeight = 2f;

	[Tooltip("How much room this animal takes up, in adult equivalents. A calf takes up less than a cow.")]
	public float AdultEquivalent = 1f;

	[Tooltip("How many adult equivalents can stand inside Packing Radius before it starts to bite.")]
	public float ComfortablePacking = 2f;

	[Tooltip("The packing at which the pressure has fallen as far as a merely oversized herd ever takes it.")]
	public float CrowdedPacking = 5f;

	[Tooltip("The packing at which the pressure bottoms out, which is deep enough to be neglect.")]
	public float SeverePacking = 9f;

	[FormerlySerializedAs("SocialChangeRate")]
	[Tooltip("How much personal space moves per second, towards whichever of the two pressures is worse. Zero disables the need.")]
	[FormerlySerializedAs("CrowdingChangeRate")]
	public float PersonalSpaceChangeRate = 0.01f;

	[Tooltip("How far a hurt animal's distress carries to the rest of its herd. Deliberately wider than SocialSearchRadius: that one is about who counts as company, this is about how far away a bull should still answer for one of his being shot. Only queried on a hit, so it can afford to be generous.")]
	public float DistressRadius = 40f;

	[Tooltip("How far a strayed animal looks for its herd when walking back to it. Must be wider than the FSM's strayDistance, or the search can never find anyone far enough away to count as strayed.")]
	public float RegroupSearchRadius = 40f;

	[Tooltip("How close (in metres) a player this animal already holds a grudge against has to stand to a calf of ours, and stay, before its mother treats it as a threat to the calf. Zero leaves only hurting it winding her up.")]
	public float CalfThreatDistance = 6f;

	[Tooltip("How close (in metres) a calf has to be to a mother who is standing her ground before it shelters behind her instead of bolting. Keep it under the FSM's strayDistance, or a calf can shelter from further away than it is willing to stand. Zero has calves always run.")]
	public float MotherShelterDistance = 12f;

	private bool playerLingeringOverCalf;

	private EntityRef<LivestockAnimal> lastShielded;

	private const int ShieldSlots = 3;

	private const float ShieldSlotLifetime = 5f;

	private EntityRef<LivestockAnimal>[] shieldSlotHolders;

	private TimeSince[] shieldSlotClaimed;

	private const float RestingSearchRadius = 30f;

	private const float RestingGap = 0.25f;

	private RustNavMeshAgent restingAgent;

	private const float AbreastSearchRadius = 10f;

	private const float InLineTolerance = 0.01f;

	private const float HerdFleeHeadingLifetime = 10f;

	private Vector3 herdFleeHeading;

	private TimeSince herdFleeHeadingSet;

	private bool hasHerdFleeHeading;

	public const float HerdLoadFloor = 0.3f;

	public const float PackingFloor = 0f;

	private float lastHerdLoadTarget = 1f;

	private float lastPackingTarget = 1f;

	private static readonly Dictionary<string, LivestockAnimal> livingSpecials = new Dictionary<string, LivestockAnimal>();

	private static readonly HashSet<string> discoveredSpecials = new HashSet<string>();

	private static LivestockSpecial queuedSpecial;

	private static bool[] eligibleSpecials;

	private float saleValueScale = 1f;

	public const Flags Leading = Flags.Reserved7;

	public const Flags Grazing = Flags.Reserved8;

	public const Flags Pregnant = Flags.Reserved10;

	public const Flags Sleeping = Flags.Reserved11;

	public const Flags Mating = Flags.Reserved12;

	public const Flags Drinking = Flags.Reserved13;

	public const Flags Resting = Flags.Reserved14;

	public const Flags Frisky = Flags.Reserved15;

	public const Flags Leadable = Flags.Reserved16;

	public const Flags LyingIn = Flags.Reserved17;

	public const int AgeTimerId = 1;

	public const int PregnancyTimerId = 2;

	public const int BreedCooldownTimerId = 3;

	public const int DungTimerId = 4;

	public AgeStage Age = AgeStage.Adult;

	public LivestockSize MountType = LivestockSize.Small;

	[Header("Aging")]
	[Tooltip("How long (in seconds) this animal stays at its current age stage. When it elapses an infant grows into the adult named by its Species asset and an adult dies of old age. Zero disables aging.")]
	public float TimeToGrow;

	[Tooltip("Random variance applied to TimeToGrow when the animal first spawns, as a fraction of it. 0.1 means +/-10%, so a herd doesn't all grow up in lockstep.")]
	[Range(0f, 1f)]
	public float GrowTimeVariance = 0.1f;

	[Tooltip("Optional effect played on the new animal when it grows into the next stage.")]
	public GameObjectRef GrowEffect;

	[Tooltip("How long (in seconds) this animal takes to give birth once pregnant")]
	public float PregnantDuration;

	[Tooltip("How long (in seconds) before the birth she stops what she is doing and lies down. The rest of the pregnancy she spends grazing like any other animal, so raising Pregnant Duration does not turn into a lie down of the same length.")]
	public float CalvingLieDownSeconds = 45f;

	[Tooltip("How far from the mother the newborn is placed, so it doesn't spawn inside her.")]
	public float ChildSpawnDistance = 1f;

	[Tooltip("Which sex this prefab represents. Infants and species that use one prefab for both sexes stay at Either and roll for it; a prefab that is specifically the male or female form of its species says so here, and must be the one its Species asset names.")]
	[Header("Sex")]
	public SexForm Form;

	[Tooltip("The species this animal belongs to. Shared by every prefab of the species - both sexes and every age stage - so it identifies mates and tells an infant which adult to grow into.")]
	public LivestockSpecies Species;

	[Header("Needs")]
	[Tooltip("How much fullness a second of grazing puts back. Deliberately slow: grazing is meant to be what the animal spends its day doing, not a top up it finishes in a few seconds.")]
	public float FullnessFillRate = 0.004f;

	[Tooltip("How much fullness drains per second while the animal is up and about in daylight. Against FullnessFillRate this sets how much of its standing time a well fed animal spends eating: 0.001 against 0.004 is a quarter of it. Slow, because the dip below ConsumeThreshold while the animal walks over is what a keeper sees. Lying down and sleeping drain at SleepingDecayScale.")]
	public float FullnessDecayRate = 0.001f;

	[Tooltip("Where a freshly spawned animal's fullness starts. At the trough ceiling, so a new animal in a well kept pen reads right straight away instead of spending minutes climbing to it.")]
	[Range(0f, 1f)]
	public float StartingFullness = 0.8f;

	[Tooltip("Scales the drain while the animal is asleep or the sun is down. Feeding is daytime work, so a full night at the waking rate would be a debt the day could not pay off.")]
	[Range(0f, 1f)]
	public float SleepingDecayScale = 0.25f;

	public float NeedTickFrequency = 5f;

	[Tooltip("How far (in metres) the animal looks for somewhere to feed itself - a trough, a water barrel, open grass or a river bank.")]
	public float ConsumeSearchRadius = 25f;

	[Tooltip("A need at or below this share of the most it can get sends the animal off to top it up. Fullness is measured against the trough ceiling while a trough is what feeds it, and against full otherwise. High on purpose: a keeper reads the bar as whether the pen is set up right, so a well kept animal has to sit near the top of it: about 0.75 to 0.8 on troughs and 0.95 to 1 on grass. The gap below the top sets how often it goes to eat, not how much of its day it spends eating.")]
	[Range(0f, 1f)]
	public float ConsumeThreshold = 0.95f;

	[Range(0f, 1f)]
	[Tooltip("The most fullness a trough will ever put back. Grass still fills an animal completely, so a herd living out of a trough sits permanently a little short and everything reading Condition pays for it.")]
	public float TroughFullnessCeiling = 0.8f;

	[Tooltip("How much fullness each calorie of trough food is worth. The animal grazes a trough item down before it takes the next, so richer food lasts longer, the way it does for a horse.")]
	public float TroughFullnessPerCalorie = 0.0021f;

	[Tooltip("How long (in seconds) the animal takes to raise its head once it stops grazing or drinking. Clearing the flag only STARTS the head coming up, so for this long it is still visibly head down. The cow's cow_eat_out is 1.7s and the sheep's sheep_eat_exit 1.5s.")]
	public float RaiseHeadDuration = 1.7f;

	public const float StartingPersonalSpaceNeed = 1f;

	private TimeSince poseHeldFor;

	private TimeUntil onItsFeetIn;

	private TimeSince headComingUpFor;

	private TickableNeed[] needs = Array.Empty<TickableNeed>();

	private TimeSince lastLoudNoise;

	private TimeSince lastNeedsTick;

	protected PersistentTimerSet timers;

	private PersistentTimer pregnancyTimer;

	private PersistentTimer breedCooldown;

	private EntityRef<LivestockAnimal> Father;

	private EntityRef<LivestockAnimal> Mother;

	private bool findingItsFeet;

	private EntityRef<LivestockAnimal> newbornCalf;

	public static LivestockNeedsQueue NeedsQueue = new LivestockNeedsQueue();

	private bool startled;

	private EntityRef<BasePlayer> leaderBeforeMount;

	private const float LeadTautDistance = 3.5f;

	private const float LeadFullSlowDistance = 7f;

	private const float LeadMaxSlow = 0.9f;

	private static readonly List<ModifierDefintion> leadModifier = new List<ModifierDefintion>
	{
		new ModifierDefintion
		{
			type = Modifier.ModifierType.MoveSpeed,
			source = Modifier.ModifierSource.Interaction,
			value = 0f,
			duration = 0f
		}
	};

	public const int MaxAnimalNameLength = 24;

	private Vector3 lastLoudNoisePosition;

	private EntityRef<BaseCombatEntity> aggressor;

	private TimeSince lastAggression;

	private float penaltyCharged;

	private TimeSince lastHerdDistress;

	private EntityRef<BaseCombatEntity> herdDistressAttacker;

	private TimeSince lastCalfThreat;

	private bool calfEverThreatened;

	private const float NeedFlashPrecision = 0.01f;

	private const float AgeFlashPrecision = 0.01f;

	private const float FamiliarityFlashPrecision = 1f;

	private FlashedValue[] flashedValues;

	private HitchTrough grazingTrough;

	private float troughFullnessStored;

	private bool hasCalved;

	private const int BirthCandidates = 8;

	private bool lastHelpingFromTrough;

	private bool __sync_IsMale;

	private EntityRef<LivestockAnimal> __sync_PregnantPartner;

	private string __sync_AnimalName;

	private EntityRef<BasePlayer> __sync_LeadingPlayer;

	private EntityRef<BuildingPrivlidge> __sync_HomeTc;

	private int __sync_Genes;

	private int __sync_PregnantPartnerGenes;

	public bool AgingEnabled
	{
		get
		{
			if (TimeToGrow > 0f)
			{
				return Livestock.ageScale > 0f;
			}
			return false;
		}
	}

	public bool IsAging
	{
		get
		{
			if (AgingEnabled)
			{
				return DecayStarted;
			}
			return false;
		}
	}

	private float AgeLifetime => TimeToGrow * LifespanScale * Livestock.ageScale;

	private float LifespanScale
	{
		get
		{
			if (!IsAdult())
			{
				return 1f;
			}
			return GeneScale(LivestockGene.Longevity);
		}
	}

	public float AgeFraction
	{
		get
		{
			if (!AgingEnabled || !IsAdult() || !ageTimer.IsRunning || !(AgeLifetime > 0f))
			{
				return 0f;
			}
			return Mathf.Clamp01(1f - ageTimer.Remaining / AgeLifetime);
		}
	}

	public float AgeSeconds
	{
		get
		{
			if (!AgingEnabled || !ageTimer.IsRunning)
			{
				return 0f;
			}
			return Mathf.Max(0f, AgeLifetime - ageTimer.Remaining);
		}
	}

	private GameObjectRef AdultForm
	{
		get
		{
			if (!((Object)(object)Species != (Object)null))
			{
				return null;
			}
			return Species.AdultFor(IsMale);
		}
	}

	private bool HasAdultForm => AdultForm?.isValid ?? false;

	public LivestockPattern PatternSet
	{
		get
		{
			if (!((Object)(object)pattern != (Object)null))
			{
				return pattern = ((Component)this).GetComponent<LivestockPattern>();
			}
			return pattern;
		}
	}

	public virtual int ShornFleece => 0;

	public LivestockSexParts SexPartSet
	{
		get
		{
			if (!((Object)(object)sexParts != (Object)null))
			{
				return sexParts = ((Component)this).GetComponent<LivestockSexParts>();
			}
			return sexParts;
		}
	}

	public string CoatName
	{
		get
		{
			if ((Object)(object)PatternSet == (Object)null)
			{
				return "none";
			}
			return (((Object)(object)PatternSet.Range != (Object)null) ? PatternSet.Range.ShadeNameOf(Genes) : null) ?? $"pattern {LivestockPattern.SeedFor(AnimalName):X8}";
		}
	}

	public float Condition
	{
		get
		{
			float num = MaxHealth();
			float num2 = Health() + conditionUnsentHealth;
			if (!(num > 0f))
			{
				return 0f;
			}
			return Mathf.Clamp01(num2 / num);
		}
	}

	public bool BreedingPaused => breedingPaused;

	public bool DungEnabled
	{
		get
		{
			if ((Object)(object)DungItem != (Object)null)
			{
				return DungInterval > 0f;
			}
			return false;
		}
	}

	public bool CanDung
	{
		get
		{
			if (IsLeadable && !IsStarving())
			{
				return Condition >= Livestock.dungConditionFloor;
			}
			return false;
		}
	}

	public bool IsCurious { get; private set; }

	public bool ActsCurious
	{
		get
		{
			if (IsCurious)
			{
				return IsDependent;
			}
			return false;
		}
	}

	public bool IsLeadable => AnyAcquaintanceAt(Livestock.trustToLead);

	public bool IsFullyBonded => AnyAcquaintanceAt(Livestock.maxTrust);

	public bool DecayStarted { get; private set; }

	private static float TrustedNearbyTimeout => SenseComponent.maxRefreshIntervalSeconds * 2f;

	public BasePlayer NearbyTrustedPlayer
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			if (!(TimeSince.op_Implicit(trustedNearbySeen) > TrustedNearbyTimeout))
			{
				return trustedNearby.Get(isServer);
			}
			return null;
		}
	}

	public BasePlayer CuriousAbout
	{
		get
		{
			BasePlayer basePlayer = curiousAbout.Get(isServer);
			if (!WouldFollowOutOfCuriosity(basePlayer))
			{
				return null;
			}
			return basePlayer;
		}
	}

	public int AgeGaitPenalty
	{
		get
		{
			if (IsInfant())
			{
				return -1;
			}
			float ageFraction = AgeFraction;
			if (ageFraction >= Mathf.Max(OldAgeSlowFraction, OldAgeVerySlowFraction))
			{
				return 2;
			}
			if (!(ageFraction >= OldAgeSlowFraction))
			{
				return 0;
			}
			return 1;
		}
	}

	public float AgeRestDurationScale => Mathf.Lerp(1f, Mathf.Max(1f, OldAgeRestDurationScale), AgeFraction);

	public bool IsInbred => LivestockGenome.IsInbred(Genes);

	public float GeneQuality => LivestockGenome.ExpressedQuality(Genes);

	public bool IsGodClone => LivestockGenome.IsGodGenome(Genes);

	public Vector3 HomePosition
	{
		get
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (!HomeTc.TryGet(isServer, out var entity) || !((Object)(object)entity != (Object)null))
			{
				return homePosition;
			}
			return ((Component)entity).transform.position;
		}
	}

	public int Generation { get; set; }

	public string NameLocale { get; set; }

	public string LineStem { get; set; }

	public bool HeirNamed { get; set; }

	public bool SpeciesAtCap
	{
		get
		{
			int maxPerSpecies = Livestock.maxPerSpecies;
			if (maxPerSpecies > 0)
			{
				return PopulationOf(Species) >= maxPerSpecies;
			}
			return false;
		}
	}

	public float SleepOffset => sleepFraction * Mathf.Max(0f, SleepTimeVariance);

	public bool IsDependent
	{
		get
		{
			if (IsMale)
			{
				return !IsAdult();
			}
			return true;
		}
	}

	private RustNavMeshAgent RestingAgent
	{
		get
		{
			if (!((Object)(object)restingAgent != (Object)null))
			{
				return restingAgent = ((Component)this).GetComponent<RustNavMeshAgent>();
			}
			return restingAgent;
		}
	}

	public float LastHerdLoadTarget => lastHerdLoadTarget;

	public float LastPackingTarget => lastPackingTarget;

	public float SaleValueScale
	{
		get
		{
			if (isServer)
			{
				return SpeciesSaleScale() * GeneSaleScale() * ConditionSaleScale() * AgeSaleScale() * OriginSaleScale();
			}
			return saleValueScale;
		}
	}

	public float SaleQuality => Mathf.Clamp01(SaleValueScale / Mathf.Max(0.01f, Livestock.priceFullQualityScale));

	public bool IsBred => Mother.IsSet;

	[Sync(Autosave = true)]
	public bool IsMale
	{
		[CompilerGenerated]
		get
		{
			return __sync_IsMale;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_IsMale, value))
			{
				__sync_IsMale = value;
				byte nameID = __GetWeaverID("IsMale");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public EntityRef<LivestockAnimal> PregnantPartner
	{
		[CompilerGenerated]
		get
		{
			return __sync_PregnantPartner;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_PregnantPartner, value))
			{
				__sync_PregnantPartner = value;
				byte nameID = __GetWeaverID("PregnantPartner");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public string AnimalName
	{
		[CompilerGenerated]
		get
		{
			return __sync_AnimalName;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_AnimalName, value))
			{
				__sync_AnimalName = value;
				byte nameID = __GetWeaverID("AnimalName");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public EntityRef<BasePlayer> LeadingPlayer
	{
		[CompilerGenerated]
		get
		{
			return __sync_LeadingPlayer;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_LeadingPlayer, value))
			{
				__sync_LeadingPlayer = value;
				byte nameID = __GetWeaverID("LeadingPlayer");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public EntityRef<BuildingPrivlidge> HomeTc
	{
		[CompilerGenerated]
		get
		{
			return __sync_HomeTc;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_HomeTc, value))
			{
				__sync_HomeTc = value;
				byte nameID = __GetWeaverID("HomeTc");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public int Genes
	{
		[CompilerGenerated]
		get
		{
			return __sync_Genes;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_Genes, value))
			{
				__sync_Genes = value;
				byte nameID = __GetWeaverID("Genes");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public int PregnantPartnerGenes
	{
		[CompilerGenerated]
		get
		{
			return __sync_PregnantPartnerGenes;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_PregnantPartnerGenes, value))
			{
				__sync_PregnantPartnerGenes = value;
				byte nameID = __GetWeaverID("PregnantPartnerGenes");
				QueueSyncVar(nameID);
			}
		}
	}

	public bool IsFemale => !IsMale;

	public TickableNeed Fullness => GetNeed(Need.Fullness);

	public TickableNeed PersonalSpace => GetNeed(Need.PersonalSpace);

	public TickableNeed Hydration => GetNeed(Need.Hydration);

	public bool IsHappy
	{
		get
		{
			if (Fullness.NeedValue > 0.5f && PersonalSpace.NeedValue > 0.5f)
			{
				return Hydration.NeedValue > 0.5f;
			}
			return false;
		}
	}

	public float Contentment => Mathf.Min(Fullness.NeedValue, Mathf.Min(PersonalSpace.NeedValue, Hydration.NeedValue));

	public bool RecentlyHeardLoudNoise
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return TimeSince.op_Implicit(lastLoudNoise) < 10f;
		}
	}

	public bool IsVeryHappy => Contentment >= Livestock.friskyThreshold;

	public bool IsFindingItsFeet => findingItsFeet;

	public LivestockAnimal MotherAnimal => Mother.Get(isServer);

	public bool IsBeingPurchased { get; set; }

	public bool StandsItsGround
	{
		get
		{
			if (IsAdult())
			{
				if (!IsMale)
				{
					return IsProtectiveOfCalf;
				}
				return true;
			}
			return false;
		}
	}

	public bool HerdMateRecentlyHurt
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return TimeSince.op_Implicit(lastHerdDistress) < 10f;
		}
	}

	public bool IsProtectiveOfCalf
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			float motherlyDuration = Livestock.motherlyDuration;
			if (motherlyDuration > 0f && calfEverThreatened && TimeSince.op_Implicit(lastCalfThreat) < motherlyDuration)
			{
				return HasDependentsNearby();
			}
			return false;
		}
	}

	public bool WantsToBeAlone
	{
		get
		{
			if (!IsPregnant())
			{
				LivestockAnimalFSM livestockAnimalFSM = default;
				if (((Component)this).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM))
				{
					return livestockAnimalFSM.WantsToBeAlone;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsHoldingPose => IsLyingDown();

	public bool IsGrazingFromTrough => (Object)(object)grazingTrough != (Object)null;

	public bool HasTroughFoodLeft
	{
		get
		{
			Item foundItem;
			if (!(troughFullnessStored > 0f))
			{
				return CanEatFrom(grazingTrough, out foundItem);
			}
			return true;
		}
	}

	public bool OnBreedingCooldown => breedCooldown.IsRunning;

	private bool MayBreed
	{
		get
		{
			if (IsFullyBonded && !BreedingPaused && !SpeciesAtCap)
			{
				BuildingPrivlidge cupboard;
				if (Livestock.breedOnlyAtHome)
				{
					return TryGetHomeCupboard(out cupboard);
				}
				return true;
			}
			return false;
		}
	}

	public bool HasCalved => hasCalved;

	public bool IsAboutToCalve
	{
		get
		{
			if (pregnancyTimer.IsRunning)
			{
				return pregnancyTimer.Remaining <= CalvingLieDownSeconds;
			}
			return false;
		}
	}

	public bool IsCarrying => pregnancyTimer.IsRunning;

	public bool IsCalving
	{
		get
		{
			if (HasFlag(Flags.Reserved10))
			{
				if (!IsAboutToCalve)
				{
					return !IsCarrying;
				}
				return true;
			}
			return false;
		}
	}

	public float ExpectedFullnessCeiling
	{
		get
		{
			if (!lastHelpingFromTrough)
			{
				return 1f;
			}
			return Mathf.Clamp01(TroughFullnessCeiling);
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("LivestockAnimal.OnRpcMessage"))
		{
			if (rpc == 3418655327u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RequestAnimalStats"));
				}
				using (TimeWarning.New("RequestAnimalStats"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3418655327u, "RequestAnimalStats", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3418655327u, "RequestAnimalStats", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RequestAnimalStats(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RequestAnimalStats");
					}
				}
				return true;
			}
			if (rpc == 3653170552u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Lead"));
				}
				using (TimeWarning.New("RPC_Lead"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3653170552u, "RPC_Lead", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3653170552u, "RPC_Lead", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_Lead(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_Lead");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private void InitAging()
	{
		ageTimer = new PersistentTimer(timers, 1)
		{
			onElapsed = OnAgeTimerElapsed,
			onStarted = OnAgeTimerStarted,
			carryOnReplace = false
		};
		if (IsAging)
		{
			StartAgeTimer();
		}
	}

	private void StartAgeTimerIfIdle()
	{
		if (IsAging && !ageTimer.IsRunning)
		{
			StartAgeTimer();
		}
	}

	private void StartAgeTimer()
	{
		ageTimer.Start(TimeToGrow * LifespanScale * Random.Range(1f - GrowTimeVariance, 1f + GrowTimeVariance) * Livestock.ageScale);
	}

	private void RestartLifespanForGenes()
	{
		if (IsAging && IsAdult())
		{
			StartAgeTimer();
		}
	}

	private void OnAgeTimerStarted()
	{
		DecayStarted = true;
	}

	private void OnAgeTimerElapsed()
	{
		DecayStarted = true;
		if (!IsDead() && AgingEnabled && !AdvanceAgeStage() && !HasAdultForm)
		{
			Debug.LogWarning((object)(Categorize() + " (" + PrefabName + ") is an Infant whose Species asset names no adult form; it will not age."));
		}
	}

	public bool ForceAgeUp()
	{
		if (IsDead())
		{
			return false;
		}
		ageTimer.Stop();
		return AdvanceAgeStage();
	}

	private bool AdvanceAgeStage()
	{
		if (IsInfant())
		{
			if (HasAdultForm)
			{
				return (Object)(object)GrowIntoAdult() != (Object)null;
			}
			return false;
		}
		DieOfOldAge();
		return true;
	}

	public LivestockAnimal GrowIntoAdult()
	{
		GameObjectRef adultForm = AdultForm;
		if (adultForm == null || !adultForm.isValid)
		{
			return null;
		}
		return ReplaceWith(adultForm.resourcePath, GrowEffect);
	}

	public LivestockAnimal ReplaceWith(string prefabPath, GameObjectRef effect = null)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		LivestockAnimal livestockAnimal = GameManager.server.CreateEntity(prefabPath, ((Component)this).transform.position, ((Component)this).transform.rotation) as LivestockAnimal;
		if ((Object)(object)livestockAnimal == (Object)null)
		{
			Debug.LogError((object)(Categorize() + " '" + prefabPath + "' is not a LivestockAnimal; aborting replacement."));
			return null;
		}
		if (HasParent())
		{
			livestockAnimal.SetParent(GetParentEntity(), worldPositionStays: true);
		}
		livestockAnimal.OwnerID = OwnerID;
		livestockAnimal.skinID = skinID;
		livestockAnimal.Spawn();
		TransferStateTo(livestockAnimal);
		livestockAnimal.SendNetworkUpdateImmediate();
		if (effect != null && effect.isValid)
		{
			Effect.server.Run(effect.resourcePath, ((Component)livestockAnimal).transform.position);
		}
		Kill();
		return livestockAnimal;
	}

	protected virtual void TransferStateTo(LivestockAnimal next)
	{
		next.health = next.MaxHealth() * healthFraction;
		if (next.Form == SexForm.Either)
		{
			next.IsMale = IsMale;
		}
		next.SetLeading(IsLeading());
		next.LeadingPlayer = LeadingPlayer;
		next.leaderBeforeMount = leaderBeforeMount;
		leaderBeforeMount = default;
		HandOverMountTo(next);
		timers.CopyTo(next.timers);
		next.PregnantPartner = PregnantPartner;
		next.sleepFraction = sleepFraction;
		next.AnimalName = AnimalName;
		next.TakeLineageFrom(this);
		next.HeirNamed = HeirNamed;
		next.Mother = Mother;
		next.Father = Father;
		next.DecayStarted = DecayStarted;
		TransferFamiliarityTo(next);
		next.Genes = Genes;
		next.RestartLifespanForGenes();
		next.RestartDungForGenes();
		for (int i = 0; i < needs.Length && i < next.needs.Length; i++)
		{
			next.needs[i] = needs[i];
		}
		TransferConditionTo(next);
		next.TakeHomeFrom(this);
	}

	private void DieOfOldAge()
	{
		LogEntry(RustLog.EntryType.Combat, 2, "died of old age");
		Die();
	}

	private void InitCondition()
	{
		if (!Application.isLoadingSave)
		{
			breedingPaused = false;
		}
	}

	private float ConditionTarget()
	{
		float num = Mathf.Max(0.01f, Livestock.conditionFullContentment / GeneScale(LivestockGene.Hardiness));
		float num2 = Mathf.InverseLerp(0f, num, Contentment);
		if (!DecayStarted)
		{
			num2 = Mathf.Max(num2, Mathf.Clamp01(Livestock.wildConditionFloor));
		}
		return num2;
	}

	private void TickCondition(float deltaTime)
	{
		if (deltaTime <= 0f || IsDead())
		{
			return;
		}
		UpdateBreedingPause();
		if (TOD_Sky.Instance.IsNight || IsSleeping())
		{
			conditionSecondsSkipped += deltaTime;
			return;
		}
		float num = MaxHealth();
		if (num <= 0f)
		{
			conditionSecondsSkipped += deltaTime;
			return;
		}
		float num2 = ConditionTarget() * num;
		float num3 = Health() + conditionUnsentHealth;
		if (Mathf.Approximately(num2, num3))
		{
			conditionSecondsSkipped += deltaTime;
			return;
		}
		float num4 = ((num2 < num3) ? Livestock.conditionFallSeconds : Livestock.conditionRiseSeconds);
		if (num4 <= 0f)
		{
			conditionSecondsSkipped += deltaTime;
			return;
		}
		conditionSecondsSpent += deltaTime;
		num3 = Mathf.MoveTowards(num3, num2, num / num4 * deltaTime);
		conditionUnsentHealth = num3 - Health();
		if (!(Mathf.Abs(conditionUnsentHealth) < num * 0.05f) || !(num3 > 0f) || !(num3 < num))
		{
			conditionUnsentHealth = 0f;
			SetHealth(num3);
			if (Health() <= 0f)
			{
				DieOfNeglect();
			}
		}
	}

	private void DieOfNeglect()
	{
		HitInfo hitInfo = new HitInfo();
		hitInfo.damageTypes.Set(DamageType.Hunger, MaxHealth());
		Die(hitInfo);
	}

	private void UpdateBreedingPause()
	{
		float condition = Condition;
		if (condition <= Livestock.breedPauseCondition)
		{
			breedingPaused = true;
		}
		else if (condition >= Livestock.breedResumeCondition)
		{
			breedingPaused = false;
		}
	}

	public void SetCondition(float value)
	{
		conditionUnsentHealth = 0f;
		SetHealth(Mathf.Clamp01(value) * MaxHealth());
		UpdateBreedingPause();
	}

	private void SaveCondition(LivestockAnimal msg)
	{
		msg.breedingPaused = breedingPaused;
	}

	private void LoadCondition(LivestockAnimal msg)
	{
		breedingPaused = msg.breedingPaused;
	}

	private void TransferConditionTo(LivestockAnimal next)
	{
		next.breedingPaused = breedingPaused;
	}

	public string GetConditionDebugInfo()
	{
		return string.Format("cond {0:0.00} -> {1:0.00}{2}", Condition, ConditionTarget(), breedingPaused ? " paused" : string.Empty) + $" ({conditionSecondsSpent:0}s spent, {conditionSecondsSkipped:0}s skipped)";
	}

	private void InitDayBudget()
	{
		activityWeights = new float[Enum.GetValues(typeof(DayActivity)).Length];
	}

	public void TickDayBudget(DayActivity activity, float deltaTime)
	{
		if (activityWeights != null && !(deltaTime <= 0f) && !TOD_Sky.Instance.IsNight && !IsSleeping())
		{
			float num = Mathf.Max(1f, DayBudgetWindow);
			float num2 = Mathf.Exp((0f - deltaTime) / num);
			for (int i = 0; i < activityWeights.Length; i++)
			{
				activityWeights[i] *= num2;
			}
			activityWeights[(int)activity] += deltaTime;
		}
	}

	public float ShareOfDay(DayActivity activity)
	{
		if (activityWeights == null)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < activityWeights.Length; i++)
		{
			num += activityWeights[i];
		}
		if (!(num > 0f))
		{
			return 0f;
		}
		return activityWeights[(int)activity] / num;
	}

	public bool IsStarving()
	{
		return Fullness.NeedValue <= StarvingThreshold;
	}

	public string GetDayBudgetDebugInfo()
	{
		return $"graze {ShareOfDay(DayActivity.Grazing):P0}" + $" rest {ShareOfDay(DayActivity.Resting):P0}" + $" idle {ShareOfDay(DayActivity.Idling):P0}";
	}

	private void InitDung()
	{
		dungTimer = new PersistentTimer(timers, 4)
		{
			onElapsed = DropDung
		};
		if (DungEnabled)
		{
			StartDungTimer();
		}
	}

	public void RestartDungForGenes()
	{
		if (DungEnabled)
		{
			StartDungTimer();
		}
	}

	private void StartDungTimer()
	{
		dungTimer.Start(DungInterval / GeneScale(LivestockGene.Dung) * DungIntervalConditionScale() * Random.Range(1f - DungIntervalVariance, 1f + DungIntervalVariance));
	}

	private float DungIntervalConditionScale()
	{
		float dungConditionFloor = Livestock.dungConditionFloor;
		float num = Mathf.Max(dungConditionFloor + 0.01f, Livestock.dungConditionFull);
		return Mathf.Lerp(Mathf.Max(1f, Livestock.dungSlowestScale), 1f, Mathf.InverseLerp(dungConditionFloor, num, Condition));
	}

	private void DropDung()
	{
		if (!IsDead())
		{
			if (CanDung)
			{
				SpawnDung();
			}
			StartDungTimer();
		}
	}

	public bool TryDropDung()
	{
		if (IsDead() || !DungEnabled || !CanDung)
		{
			return false;
		}
		SpawnDung();
		StartDungTimer();
		return true;
	}

	public float NextDungInterval()
	{
		return DungInterval / GeneScale(LivestockGene.Dung) * DungIntervalConditionScale();
	}

	public float DungBestOfBreed()
	{
		float num = DungInterval / Mathf.Max(0.01f, Livestock.geneDungGood);
		float num2 = NextDungInterval();
		if (!(num2 > 0f))
		{
			return 0f;
		}
		return Mathf.Clamp01(num / num2);
	}

	public void SpawnDung()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)DungItem == (Object)null))
		{
			Item item = ItemManager.Create(DungItem, 1, 0uL, isServerSide: true, 0uL);
			if (item != null)
			{
				item.SetItemOwnership(string.IsNullOrEmpty(AnimalName) ? Categorize() : AnimalName, ItemOwnershipPhrases.Pooped);
				Vector3 val = (((Object)(object)DungSpawnPoint != (Object)null) ? DungSpawnPoint.position : ((Component)this).transform.position);
				item.Drop(vVelocity: new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-3f, -1f), Random.Range(-0.5f, 0.5f)), vPos: val + Random.insideUnitSphere * 0.1f, rotation: Random.rotationUniform);
				LivestockCensus.RecordDung(val);
			}
		}
	}

	private void InitFamiliarity()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		probationCredited = TimeSince.op_Implicit(float.MaxValue);
		trustedNearbySeen = TimeSince.op_Implicit(float.MaxValue);
		lastFamiliarityTick = TimeSince.op_Implicit(0f);
		if (!Application.isLoadingSave)
		{
			IsCurious = Random.value < Livestock.curiousChance;
		}
		ValidateFamiliarityTuning();
	}

	public void SetCurious(bool curious)
	{
		IsCurious = curious;
	}

	private void ValidateFamiliarityTuning()
	{
	}

	private bool AnyAcquaintanceAt(float seconds)
	{
		if (acquaintances == null)
		{
			return false;
		}
		for (int i = 0; i < acquaintances.Length; i++)
		{
			if (acquaintances[i].userId != 0L && acquaintances[i].seconds >= seconds)
			{
				return true;
			}
		}
		return false;
	}

	public bool TrustsToHandle(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null)
		{
			return TrustOf(player) >= Livestock.trustToLead;
		}
		return false;
	}

	private void RefreshHusbandryClocks()
	{
		bool isLeadable = IsLeadable;
		SetNetworkedFlag(Flags.Reserved16, isLeadable);
		if (isLeadable)
		{
			DecayStarted = true;
			StartAgeTimerIfIdle();
		}
	}

	public float TrustOf(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return 0f;
		}
		if (IsSoreAt(player))
		{
			return 0f;
		}
		return Mathf.Max(StoredSecondsFor(player.userID), PartialCreditFor(player));
	}

	public bool IsSoreAt(BaseEntity entity)
	{
		if ((Object)(object)entity == (Object)null)
		{
			return false;
		}
		if (TryGetRememberedAggressor(out var attacker) && (Object)(object)attacker == (Object)(object)entity)
		{
			return true;
		}
		if (TryGetHerdDistressAttacker(out var attacker2))
		{
			return (Object)(object)attacker2 == (Object)(object)entity;
		}
		return false;
	}

	private float PartialCreditFor(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return 0f;
		}
		return Mathf.Max(HousemateCreditFor(player), HandlingCreditFor(player));
	}

	private float HandlingCreditFor(BasePlayer player)
	{
		float calmedTrustFloor = Livestock.calmedTrustFloor;
		if (calmedTrustFloor <= 0f || (Object)(object)player.modifiers == (Object)null)
		{
			return 0f;
		}
		if (!(player.modifiers.GetValue(Modifier.ModifierType.LivestockHandling) > 0f))
		{
			return 0f;
		}
		return calmedTrustFloor;
	}

	private float HousemateCreditFor(BasePlayer player)
	{
		float trustFloor = Livestock.trustFloor;
		if (trustFloor <= 0f || acquaintances == null || (Object)(object)player == (Object)null)
		{
			return 0f;
		}
		float trustToLead = Livestock.trustToLead;
		if (TryGetHomeCupboard(out var cupboard))
		{
			if (cupboard.IsAuthed(player.userID))
			{
				return trustFloor;
			}
			if (cupboard.recentGroupMembers.ContainsKey(player.userID))
			{
				return trustFloor;
			}
		}
		RelationshipManager serverInstance = RelationshipManager.ServerInstance;
		if ((Object)(object)serverInstance == (Object)null || player.currentTeam == 0L)
		{
			return 0f;
		}
		for (int i = 0; i < acquaintances.Length; i++)
		{
			if (acquaintances[i].userId != 0L && !(acquaintances[i].seconds < trustToLead))
			{
				RelationshipManager.PlayerTeam playerTeam = serverInstance.FindPlayersTeam(acquaintances[i].userId);
				if (playerTeam != null && playerTeam.teamID == player.currentTeam)
				{
					return trustFloor;
				}
			}
		}
		return 0f;
	}

	public float StoredSecondsFor(ulong userId)
	{
		if (acquaintances == null || userId == 0L)
		{
			return 0f;
		}
		for (int i = 0; i < acquaintances.Length; i++)
		{
			if (acquaintances[i].userId == userId)
			{
				return acquaintances[i].seconds;
			}
		}
		return 0f;
	}

	public void AddFamiliarity(ulong userId, float seconds, FamiliarityReason reason = FamiliarityReason.Handled)
	{
		if (Interface.CallHook("OnLivestockAnimalFamiliarityAdd", this, userId, seconds, reason) == null)
		{
			bool isLeadable = IsLeadable;
			CreditFamiliarity(userId, seconds);
			RefreshHusbandryClocks();
			Interface.CallHook("OnLivestockAnimalFamiliarityAdded", this, userId, seconds, reason);
			if (!isLeadable && IsLeadable && (reason == FamiliarityReason.Handled || reason == FamiliarityReason.Purchased))
			{
				Facepunch.Rust.Analytics.Azure.OnLivestockAcquired(userId, this, reason);
			}
		}
	}

	private void CreditFamiliarity(ulong userId, float seconds)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		if (userId == 0L || Mathf.Approximately(seconds, 0f))
		{
			return;
		}
		float maxTrust = Livestock.maxTrust;
		if (acquaintances == null)
		{
			acquaintances = new Acquaintance[8];
		}
		int num = -1;
		int num2 = 0;
		for (int i = 0; i < acquaintances.Length; i++)
		{
			if (acquaintances[i].userId == userId)
			{
				acquaintances[i].seconds = Mathf.Clamp(acquaintances[i].seconds + seconds, 0f, maxTrust);
				if (seconds > 0f)
				{
					acquaintances[i].sinceCredited = TimeSince.op_Implicit(0f);
				}
				return;
			}
			if (acquaintances[i].userId == 0L)
			{
				if (num < 0)
				{
					num = i;
				}
			}
			else if (acquaintances[i].seconds < acquaintances[num2].seconds)
			{
				num2 = i;
			}
		}
		if (seconds < 0f)
		{
			return;
		}
		if (num >= 0)
		{
			acquaintances[num] = new Acquaintance
			{
				userId = userId,
				seconds = Mathf.Min(seconds, maxTrust),
				sinceCredited = TimeSince.op_Implicit(0f)
			};
			return;
		}
		if (probation.userId != userId)
		{
			if (probation.userId != 0L && TimeSince.op_Implicit(probationCredited) < 60f)
			{
				return;
			}
			probation = new Acquaintance
			{
				userId = userId
			};
		}
		probationCredited = TimeSince.op_Implicit(0f);
		probation.seconds = Mathf.Min(probation.seconds + seconds, maxTrust);
		if (probation.seconds > acquaintances[num2].seconds)
		{
			acquaintances[num2] = probation;
			acquaintances[num2].sinceCredited = TimeSince.op_Implicit(0f);
			probation = default;
			probationCredited = TimeSince.op_Implicit(float.MaxValue);
		}
	}

	public void SetFamiliarity(ulong userId, float seconds, FamiliarityReason reason = FamiliarityReason.Handled)
	{
		if (userId != 0L)
		{
			AddFamiliarity(userId, seconds - StoredSecondsFor(userId), reason);
		}
	}

	public float BestFamiliarity(out ulong userId)
	{
		userId = 0uL;
		float num = 0f;
		if (acquaintances == null)
		{
			return 0f;
		}
		for (int i = 0; i < acquaintances.Length; i++)
		{
			if (acquaintances[i].userId != 0L && acquaintances[i].seconds > num)
			{
				num = acquaintances[i].seconds;
				userId = acquaintances[i].userId;
			}
		}
		return num;
	}

	private void TickFamiliarity()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		float num = TimeSince.op_Implicit(lastFamiliarityTick);
		lastFamiliarityTick = TimeSince.op_Implicit(0f);
		float familiarityForgetScale = Livestock.familiarityForgetScale;
		float maxTrust = Livestock.maxTrust;
		if (acquaintances == null || num <= 0f || familiarityForgetScale <= 0f)
		{
			return;
		}
		float num2 = num / familiarityForgetScale;
		float trustedNearbyTimeout = TrustedNearbyTimeout;
		bool flag = false;
		for (int i = 0; i < acquaintances.Length; i++)
		{
			ref Acquaintance reference = ref acquaintances[i];
			if (reference.userId != 0L && !(reference.seconds >= maxTrust) && !(TimeSince.op_Implicit(reference.sinceCredited) <= trustedNearbyTimeout))
			{
				reference.seconds -= num2;
				flag = true;
				if (reference.seconds <= 0f)
				{
					reference = default;
				}
			}
		}
		if (flag)
		{
			RefreshHusbandryClocks();
		}
		if (probation.userId != 0L && !(probation.seconds >= maxTrust) && !(TimeSince.op_Implicit(probationCredited) <= trustedNearbyTimeout))
		{
			probation.seconds -= num2;
			if (probation.seconds <= 0f)
			{
				probation = default;
				probationCredited = TimeSince.op_Implicit(float.MaxValue);
			}
		}
	}

	public void OnPlayerSensed(BasePlayer player, float deltaTime)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null || player.IsDead() || deltaTime <= 0f)
		{
			return;
		}
		TryDefendSensedPlayer(player);
		if (player.IsSleeping() || IsSoreAt(player))
		{
			return;
		}
		float familiarityRadius = Livestock.familiarityRadius;
		if (familiarityRadius <= 0f)
		{
			return;
		}
		float num = Vector3.SqrMagnitude(((Component)player).transform.position - ((Component)this).transform.position);
		if (!(num > familiarityRadius * familiarityRadius))
		{
			float num2 = Mathf.Max(0f, Livestock.familiarityRate);
			if ((Object)(object)player.modifiers != (Object)null)
			{
				num2 *= 1f + Mathf.Max(0f, player.modifiers.GetValue(Modifier.ModifierType.LivestockHandling));
			}
			AddFamiliarity(player.userID, deltaTime * num2);
			TrackTrustedNearby(player, num);
			TryAdoptHome(player);
		}
	}

	private void TrackTrustedNearby(BasePlayer player, float distanceSqr)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (!(TrustOf(player) < Livestock.trustToFollow) && (!(TimeSince.op_Implicit(trustedNearbySeen) <= TrustedNearbyTimeout) || !((Object)(object)trustedNearby.Get(isServer) != (Object)(object)player) || !(distanceSqr >= trustedNearbyDistanceSqr)))
		{
			trustedNearby.Set(player);
			trustedNearbyDistanceSqr = distanceSqr;
			trustedNearbySeen = TimeSince.op_Implicit(0f);
		}
	}

	public BasePlayer StartCuriousFollow()
	{
		BasePlayer nearbyTrustedPlayer = NearbyTrustedPlayer;
		curiousAbout.Set(nearbyTrustedPlayer);
		return nearbyTrustedPlayer;
	}

	public bool WouldFollowOutOfCuriosity(BasePlayer player)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null || player.IsDead() || player.IsSleeping() || player.IsDestroyed)
		{
			return false;
		}
		if (IsDead() || IsLeading() || IsCalving || IsSleeping() || TOD_Sky.Instance.IsNight)
		{
			return false;
		}
		if (Fullness.NeedValue < 0.7f)
		{
			return false;
		}
		if (TrustOf(player) < Livestock.trustToFollow)
		{
			return false;
		}
		float curiousBreakDistance = Livestock.curiousBreakDistance;
		if (!(curiousBreakDistance <= 0f))
		{
			return Vector3.SqrMagnitude(((Component)player).transform.position - ((Component)this).transform.position) <= curiousBreakDistance * curiousBreakDistance;
		}
		return true;
	}

	public void StopCuriousFollow()
	{
		curiousAbout = default;
	}

	private void TryDefendSensedPlayer(BasePlayer player)
	{
		if (!IsMale || !IsAdult() || IsDead())
		{
			return;
		}
		float maxTrust = Livestock.maxTrust;
		if (!(maxTrust <= 0f) && !(TrustOf(player) < maxTrust))
		{
			float defendReactionTime = Livestock.defendReactionTime;
			if (!(defendReactionTime <= 0f) && !(player.SecondsSinceAttacked > defendReactionTime) && !(player.lastAttackedTime <= lastDefendedAttack))
			{
				OnHerdMateHurt(player.lastAttacker, null);
				lastDefendedAttack = player.lastAttackedTime;
			}
		}
	}

	public bool RefusesToTarget(BaseEntity entity)
	{
		if (!(entity is BasePlayer basePlayer))
		{
			return !IsSoreAt(entity);
		}
		object obj = Interface.CallHook("CanLivestockAnimalRefuseTarget", this, basePlayer);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (!IsMale || !IsAdult())
		{
			return false;
		}
		float maxTrust = Livestock.maxTrust;
		if (maxTrust > 0f)
		{
			return TrustOf(basePlayer) >= maxTrust;
		}
		return false;
	}

	private void SaveFamiliarity(LivestockAnimal save)
	{
		save.curious = IsCurious;
		if (acquaintances == null)
		{
			return;
		}
		save.acquaintances = Pool.Get<List<LivestockAcquaintance>>();
		for (int i = 0; i < acquaintances.Length; i++)
		{
			if (acquaintances[i].userId != 0L)
			{
				LivestockAcquaintance val = Pool.Get<LivestockAcquaintance>();
				val.userId = acquaintances[i].userId;
				val.seconds = acquaintances[i].seconds;
				save.acquaintances.Add(val);
			}
		}
	}

	private void LoadFamiliarity(LivestockAnimal save)
	{
		IsCurious = save.curious;
		List<LivestockAcquaintance> list = save.acquaintances;
		if (list != null && list.Count > 0)
		{
			acquaintances = new Acquaintance[8];
			for (int i = 0; i < acquaintances.Length && i < list.Count; i++)
			{
				acquaintances[i].userId = list[i].userId;
				acquaintances[i].seconds = list[i].seconds;
			}
		}
		RefreshHusbandryClocks();
	}

	private void TransferFamiliarityTo(LivestockAnimal grown)
	{
		grown.IsCurious = IsCurious;
		CopyAcquaintancesTo(grown);
	}

	private void CopyAcquaintancesTo(LivestockAnimal other)
	{
		if (!((Object)(object)other == (Object)null) && acquaintances != null)
		{
			other.acquaintances = new Acquaintance[8];
			Array.Copy(acquaintances, other.acquaintances, 8);
			other.RefreshHusbandryClocks();
		}
	}

	private void PassBondsTo(LivestockAnimal calf)
	{
		if ((Object)(object)calf == (Object)null || acquaintances == null)
		{
			return;
		}
		float trustToLead = Livestock.trustToLead;
		int num = 0;
		for (int i = 0; i < acquaintances.Length; i++)
		{
			if (acquaintances[i].userId != 0L)
			{
				if (num == 0)
				{
					calf.acquaintances = new Acquaintance[8];
				}
				calf.acquaintances[num++] = new Acquaintance
				{
					userId = acquaintances[i].userId,
					seconds = Mathf.Min(acquaintances[i].seconds, trustToLead)
				};
			}
		}
		calf.RefreshHusbandryClocks();
	}

	public static float ThresholdFor(TrustTier tier)
	{
		return tier switch
		{
			TrustTier.Known => Livestock.trustToFollow, 
			TrustTier.Tolerated => Livestock.trustToTolerate, 
			TrustTier.Bonded => Livestock.trustToLead, 
			TrustTier.Herd => Livestock.maxTrust, 
			_ => 0f, 
		};
	}

	public static float ProgressInTier(float seconds)
	{
		TrustTier trustTier = TierFor(seconds);
		if (trustTier == TrustTier.Herd)
		{
			return 1f;
		}
		float num = ThresholdFor(trustTier);
		float num2 = ThresholdFor(trustTier + 1);
		if (!(num2 > num))
		{
			return 0f;
		}
		return Mathf.Clamp01((seconds - num) / (num2 - num));
	}

	public static TrustTier TierFor(float seconds)
	{
		if (seconds >= Livestock.maxTrust)
		{
			return TrustTier.Herd;
		}
		if (seconds >= Livestock.trustToLead)
		{
			return TrustTier.Bonded;
		}
		if (seconds >= Livestock.trustToTolerate)
		{
			return TrustTier.Tolerated;
		}
		if (seconds >= Livestock.trustToFollow)
		{
			return TrustTier.Known;
		}
		return TrustTier.Stranger;
	}

	public static string TrustTierName(float seconds)
	{
		if (seconds >= Livestock.maxTrust)
		{
			return "herd";
		}
		if (seconds >= Livestock.trustToLead)
		{
			return "bonded";
		}
		if (seconds >= Livestock.trustToTolerate)
		{
			return "tolerated";
		}
		if (seconds >= Livestock.trustToFollow)
		{
			return "known";
		}
		return "stranger";
	}

	public string GetFamiliarityDebugInfo()
	{
		float num = BestFamiliarity(out var userId);
		string text = (ActsCurious ? " curious" : string.Empty);
		if (userId != 0L)
		{
			return $"fam {num:0}s {TrustTierName(num)}{text}";
		}
		return "fam none stranger" + text;
	}

	private void InitFrailty()
	{
		((Component)this).TryGetComponent<LivestockAnimalFSM>(ref frailtyFsm);
		((Component)this).TryGetComponent<RustNavMeshAgent>(ref frailtyAgent);
		TickFrailty();
	}

	private void TickFrailty()
	{
		if ((Object)(object)frailtyAgent != (Object)null)
		{
			frailtyAgent.gaitPenalty = AgeGaitPenalty;
		}
		if ((Object)(object)frailtyFsm != (Object)null)
		{
			frailtyFsm.roam.SetHomeRadiusScale(Mathf.Lerp(1f, OldAgeRangeFactor, AgeFraction));
		}
	}

	public LivestockAllele Expressed(LivestockGene gene)
	{
		return LivestockGenome.Expressed(Genes, gene);
	}

	public LivestockAllele Carried(LivestockGene gene)
	{
		return LivestockGenome.Carried(Genes, gene);
	}

	public LivestockAllele AlleleFor(LivestockGene gene, int copy)
	{
		return LivestockGenome.GetAllele(Genes, gene, copy);
	}

	public bool IsPurebred(LivestockGene gene)
	{
		return LivestockGenome.IsPurebred(Genes, gene);
	}

	public int LineageMarker(int copy)
	{
		return LivestockGenome.GetMarker(Genes, copy);
	}

	public float GeneScale(LivestockGene gene)
	{
		if (!Livestock.genesEnabled)
		{
			return 1f;
		}
		return GeneScaleOf(Genes, gene);
	}

	public static float GeneScaleOf(int genome, LivestockGene gene)
	{
		float num = LivestockGeneRange.TraitFor(genome, gene);
		float num2 = ((num < 0.5f) ? Mathf.Lerp(BadScaleFor(gene), 1f, num * 2f) : Mathf.Lerp(1f, GoodScaleFor(gene), num * 2f - 1f));
		return Mathf.Max(0.01f, num2);
	}

	private static float BadScaleFor(LivestockGene gene)
	{
		return gene switch
		{
			LivestockGene.Dung => Livestock.geneDungBad, 
			LivestockGene.Longevity => Livestock.geneLongevityBad, 
			LivestockGene.Yield => Livestock.geneYieldBad, 
			LivestockGene.Fertility => Livestock.geneFertilityBad, 
			_ => Livestock.geneHardinessBad, 
		};
	}

	private static float GoodScaleFor(LivestockGene gene)
	{
		return gene switch
		{
			LivestockGene.Dung => Livestock.geneDungGood, 
			LivestockGene.Longevity => Livestock.geneLongevityGood, 
			LivestockGene.Yield => Livestock.geneYieldGood, 
			LivestockGene.Fertility => Livestock.geneFertilityGood, 
			_ => Livestock.geneHardinessGood, 
		};
	}

	private void InitGenes()
	{
		Genes = RollGenome();
	}

	public static int RollGenome()
	{
		return LivestockGenome.Roll();
	}

	private int InheritedGenome(int fatherGenes)
	{
		int father = ((fatherGenes != 0) ? fatherGenes : Genes);
		return LivestockGenome.Combine(Genes, father, Livestock.geneMutationChance);
	}

	public int LitterSize()
	{
		float num = Mathf.Max(0f, GeneScale(LivestockGene.Fertility) - 1f);
		int num2 = Mathf.FloorToInt(num);
		if (Random.value < num - (float)num2)
		{
			num2++;
		}
		return Mathf.Clamp(1 + num2, 1, Mathf.Max(1, Livestock.maxLitterSize));
	}

	public void SetGene(LivestockGene gene, LivestockAllele first, LivestockAllele second)
	{
		Genes = LivestockGenome.WithPair(Genes, gene, first, second);
	}

	public void SetLineageMarkers(int first, int second)
	{
		Genes = LivestockGenome.WithMarkers(Genes, first, second);
	}

	public string GetGenesDebugInfo()
	{
		StringBuilder stringBuilder = Pool.Get<StringBuilder>();
		stringBuilder.Clear();
		LivestockGene[] allGenes = LivestockGenome.AllGenes;
		foreach (LivestockGene livestockGene in allGenes)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(livestockGene).Append(' ').Append(AlleleFor(livestockGene, 0).ToString()[0])
				.Append(AlleleFor(livestockGene, 1).ToString()[0])
				.Append(" (")
				.Append(GeneScale(livestockGene).ToString("0.00"))
				.Append(')');
		}
		stringBuilder.Append(", lineage ").Append(LivestockGenome.GetMarker(Genes, 0)).Append('/')
			.Append(LivestockGenome.GetMarker(Genes, 1));
		if (IsInbred)
		{
			stringBuilder.Append(" INBRED");
		}
		string result = stringBuilder.ToString();
		Pool.FreeUnmanaged(ref stringBuilder);
		return result;
	}

	public void SetHomePosition(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		homePosition = position;
	}

	public bool TryGetHomeCupboard(out BuildingPrivlidge cupboard)
	{
		cupboard = HomeTc.Get(isServer);
		if ((Object)(object)cupboard != (Object)null)
		{
			return !cupboard.IsDestroyed;
		}
		return false;
	}

	private void InitHome()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		lastAdoptionCheck = TimeSince.op_Implicit(float.MaxValue);
		if (!Application.isLoadingSave)
		{
			homePosition = ((Component)this).transform.position;
		}
	}

	public void TakeHomeFrom(LivestockAnimal other)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)other == (Object)null))
		{
			homePosition = other.homePosition;
			HomeTc = other.HomeTc;
		}
	}

	private void TryAdoptHome(BasePlayer player)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (!(TrustOf(player) < Livestock.maxTrust) && !(TimeSince.op_Implicit(lastAdoptionCheck) < 5f))
		{
			lastAdoptionCheck = TimeSince.op_Implicit(0f);
			BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(cached: true);
			if (!((Object)(object)buildingPrivilege == (Object)null) && buildingPrivilege.IsAuthed(player))
			{
				HomeTc = new EntityRef<BuildingPrivlidge>(buildingPrivilege.net.ID);
			}
		}
	}

	private void SaveHome(LivestockAnimal save)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		save.homePosition = homePosition;
	}

	private void LoadHome(LivestockAnimal save)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (save.homePosition != Vector3.zero)
		{
			homePosition = save.homePosition;
		}
	}

	public bool TryGetBedtimeHome(out Vector3 home)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		home = HomePosition;
		float num = Mathf.Max(0f, HerdGatherDistance);
		Vector3 val = home - ((Component)this).transform.position;
		return val.sqrMagnitude > num * num;
	}

	public string GetHomeDebugInfo()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Distance(((Component)this).transform.position, HomePosition);
		if (!TryGetHomeCupboard(out var _))
		{
			return $"home wild {num:0}m";
		}
		return $"home tc {num:0}m";
	}

	public bool NeedsWater()
	{
		return Hydration.NeedValue <= ConsumeThreshold;
	}

	public void FindDrinkBarrels(List<BaseEntity> results)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (ConsumeSearchRadius <= 0f)
		{
			return;
		}
		PooledList<LiquidContainer> val = Pool.Get<PooledList<LiquidContainer>>();
		try
		{
			Query.Server.GetInSphere(((Component)this).transform.position, ConsumeSearchRadius, (List<LiquidContainer>)(object)val);
			foreach (LiquidContainer item in (List<LiquidContainer>)(object)val)
			{
				if (CanDrinkFrom(item))
				{
					results.Add(item);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public LiquidContainer FindDrinkBarrel()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			FindDrinkBarrels((List<BaseEntity>)(object)val);
			float distanceSqr;
			return Eqs.NearestBeyond((List<BaseEntity>)(object)val, ((Component)this).transform.position, -1f, out distanceSqr) as LiquidContainer;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool CanDrinkFrom(LiquidContainer container)
	{
		if ((Object)(object)container == (Object)null || container.IsDestroyed || container.IsDead())
		{
			return false;
		}
		Item liquidItem = container.GetLiquidItem();
		if (liquidItem != null && (Object)(object)liquidItem.info == (Object)(object)WaterTypes.WaterItemDef)
		{
			return liquidItem.amount >= WaterForADrink();
		}
		return false;
	}

	public int WaterForADrink()
	{
		float num = 1f - Hydration.NeedValue;
		return Mathf.Max(1, Mathf.RoundToInt((float)BarrelWaterPerDrink * num));
	}

	public void ConsumeFromBarrel(LiquidContainer container)
	{
		if (CanDrinkFrom(container))
		{
			container.GetLiquidItem().UseItem(WaterForADrink());
		}
	}

	public bool IsDrinkableShore(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (!WaterResource.IsFreshWater(position))
		{
			return false;
		}
		float waterDepth = WaterLevel.GetWaterDepth(position, waves: false, volumes: false);
		if (waterDepth >= MinStandingWaterDepth)
		{
			return waterDepth <= MaxStandingWaterDepth;
		}
		return false;
	}

	private void InitName()
	{
		if (!Application.isLoadingSave && string.IsNullOrEmpty(AnimalName))
		{
			AnimalName = RollAnimalName(IsMale, out var locale);
			FoundLine(locale);
		}
	}

	public static string RollAnimalName(bool male, out string locale)
	{
		if (Random.value < Livestock.devNameChance && LivestockNames.HasPack("devs"))
		{
			locale = "devs";
			return DrawFrom(male, locale);
		}
		locale = LivestockLocaleWeights.Roll();
		return DrawFrom(male, locale);
	}

	public static string RollAnimalName(bool male, string locale)
	{
		return DrawFrom(male, locale);
	}

	private static string DrawFrom(bool male, string locale)
	{
		string[] array = LivestockNames.PoolFor(male, locale);
		if (array.Length == 0)
		{
			return string.Empty;
		}
		string text = array[Random.Range(0, array.Length)];
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		text = LivestockNames.Capitalise(text, locale);
		if (text.Length <= 24)
		{
			return text;
		}
		return text.Substring(0, 24);
	}

	public bool TryInheritName(LivestockAnimal mother, LivestockAnimal father)
	{
		if (!Livestock.dynasticNames)
		{
			return false;
		}
		LivestockAnimal livestockAnimal = (IsMale ? father : mother);
		if ((Object)(object)livestockAnimal == (Object)null || string.IsNullOrEmpty(livestockAnimal.AnimalName))
		{
			return false;
		}
		if (livestockAnimal.HeirNamed)
		{
			return false;
		}
		if (livestockAnimal.NameLocale == "devs")
		{
			return false;
		}
		int num = Mathf.Max(1, livestockAnimal.Generation) + 1;
		if (num > 20)
		{
			return false;
		}
		string text = DynastyName(livestockAnimal.LineageStem(), num);
		if (text.Length > 24)
		{
			ReportTooLong(text);
			return false;
		}
		AnimalName = text;
		livestockAnimal.HeirNamed = true;
		TakeLineageFrom(livestockAnimal);
		Generation = num;
		return true;
	}

	public string LineageStem()
	{
		if (Generation < 2)
		{
			return AnimalName;
		}
		if (!string.IsNullOrEmpty(LineStem))
		{
			return LineStem;
		}
		return StripPattern(AnimalName, DynastyPattern(Generation)) ?? AnimalName;
	}

	public void FoundLine(string locale)
	{
		LineStem = AnimalName;
		NameLocale = locale;
		Generation = 0;
		HeirNamed = false;
	}

	public void TakeLineageFrom(LivestockAnimal other)
	{
		LineStem = other.LineageStem();
		NameLocale = other.NameLocale;
		Generation = other.Generation;
	}

	private static string StripPattern(string name, string pattern)
	{
		if (string.IsNullOrEmpty(pattern) || string.IsNullOrEmpty(name))
		{
			return null;
		}
		int num = pattern.IndexOf("{0}", StringComparison.Ordinal);
		if (num < 0)
		{
			return null;
		}
		string text = pattern.Substring(0, num);
		string text2 = pattern.Substring(num + "{0}".Length);
		if (name.Length <= text.Length + text2.Length || !name.StartsWith(text, StringComparison.Ordinal) || !name.EndsWith(text2, StringComparison.Ordinal))
		{
			return null;
		}
		return name.Substring(text.Length, name.Length - text.Length - text2.Length);
	}

	public static string DynastyName(string stem, int generation)
	{
		if (generation < 2 || string.IsNullOrEmpty(stem))
		{
			return stem;
		}
		if (generation == 2)
		{
			return $"{stem} Junior";
		}
		return $"{stem} the {Ordinal(generation)}";
	}

	private static string DynastyPattern(int generation)
	{
		if (generation < 2)
		{
			return null;
		}
		if (generation != 2)
		{
			return string.Format("{0} the {1}", "{0}", Ordinal(generation));
		}
		return "{0} Junior";
	}

	private static void ReportTooLong(string name)
	{
		if (reportedLongNames.Add(name))
		{
			Debug.LogWarning((object)$"The livestock dynasty name \"{name}\" is longer than the {24} characters a name may be, so the calf founds its own line instead.");
		}
	}

	public static string Ordinal(int number)
	{
		int num = number % 100;
		if (num >= 11 && num <= 13)
		{
			return number + "th";
		}
		return (number % 10) switch
		{
			1 => number + "st", 
			2 => number + "nd", 
			3 => number + "rd", 
			_ => number + "th", 
		};
	}

	private void SaveName(LivestockAnimal save)
	{
		save.generation = Generation;
		save.nameLocale = NameLocale;
		save.lineStem = LineStem;
		save.heirNamed = HeirNamed;
	}

	private void LoadName(LivestockAnimal save)
	{
		Generation = save.generation;
		NameLocale = save.nameLocale;
		LineStem = save.lineStem;
		HeirNamed = save.heirNamed;
	}

	public static int PopulationOf(LivestockSpecies species)
	{
		if ((Object)(object)species == (Object)null || !populationBySpecies.TryGetValue(species, out var value))
		{
			return 0;
		}
		return value;
	}

	private void JoinPopulation()
	{
		if (!((Object)(object)Species == (Object)null))
		{
			populationBySpecies.TryGetValue(Species, out var value);
			populationBySpecies[Species] = value + 1;
		}
	}

	private void LeavePopulation()
	{
		if (!((Object)(object)Species == (Object)null) && populationBySpecies.TryGetValue(Species, out var value))
		{
			populationBySpecies[Species] = Mathf.Max(0, value - 1);
		}
	}

	public static int CullPast(int max)
	{
		if (max < 0)
		{
			return 0;
		}
		PooledList<CullCandidate> val = Pool.Get<PooledList<CullCandidate>>();
		try
		{
			LivestockAnimal[] array = Util.FindAll<LivestockAnimal>();
			foreach (LivestockAnimal livestockAnimal in array)
			{
				if (!((Object)(object)livestockAnimal == (Object)null) && !livestockAnimal.IsDestroyed && !livestockAnimal.IsDead() && livestockAnimal.isServer && !((Object)(object)livestockAnimal.Species == (Object)null))
				{
					((List<CullCandidate>)(object)val).Add(new CullCandidate(livestockAnimal));
				}
			}
			((List<CullCandidate>)(object)val).Sort();
			int num = 0;
			for (int j = 0; j < ((List<CullCandidate>)(object)val).Count; j++)
			{
				LivestockAnimal animal = ((List<CullCandidate>)(object)val)[j].Animal;
				if (PopulationOf(animal.Species) > max)
				{
					animal.Kill();
					num++;
				}
			}
			return num;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void SetSleepFraction(float fraction)
	{
		sleepFraction = Mathf.Clamp01(fraction);
	}

	private void InitSleep()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		sleepFraction = Random.value;
		poseHeldFor = TimeSince.op_Implicit(float.MaxValue);
	}

	public LivestockAnimal FindSleepingCompanion()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		LivestockAnimal livestockAnimal = (IsInfant() ? MotherAnimal : null);
		LivestockAnimal livestockAnimal2 = (((Object)(object)livestockAnimal != (Object)null && !livestockAnimal.IsDead()) ? livestockAnimal : FindHerdToReturnTo());
		if ((Object)(object)livestockAnimal2 == (Object)null || livestockAnimal2.IsDead())
		{
			return null;
		}
		float num = Mathf.Max(0f, HerdGatherDistance);
		Vector3 val = ((Component)livestockAnimal2).transform.position - ((Component)this).transform.position;
		if (!(val.sqrMagnitude > num * num))
		{
			return null;
		}
		return livestockAnimal2;
	}

	public bool IsHerdMate(LivestockAnimal candidate)
	{
		if ((Object)(object)candidate == (Object)null || (Object)(object)candidate == (Object)(object)this || candidate.IsDestroyed || candidate.IsDead())
		{
			return false;
		}
		return candidate.IsSameSpeciesAs(this);
	}

	public bool IsMyDependent(LivestockAnimal candidate)
	{
		if (!IsHerdMate(candidate) || !candidate.IsDependent || IsInfant() || IsLeading())
		{
			return false;
		}
		if (IsMale)
		{
			return true;
		}
		if (candidate.IsInfant())
		{
			return (Object)(object)candidate.MotherAnimal == (Object)(object)this;
		}
		return false;
	}

	public int CountHerdMates()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (SocialSearchRadius <= 0f)
		{
			return 0;
		}
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			Query.Server.GetBrainsInSphere(((Component)this).transform.position, SocialSearchRadius, (List<LivestockAnimal>)(object)val);
			int num = 0;
			LivestockAnimal livestockAnimal = null;
			float num2 = float.MaxValue;
			foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
			{
				if (!IsHerdMate(item))
				{
					continue;
				}
				num++;
				if (IsMyDependent(item))
				{
					Vector3 val2 = ((Component)item).transform.position - ((Component)this).transform.position;
					float sqrMagnitude = val2.sqrMagnitude;
					if (!(sqrMagnitude >= num2))
					{
						livestockAnimal = item;
						num2 = sqrMagnitude;
					}
				}
			}
			if ((Object)(object)livestockAnimal != (Object)null)
			{
				RememberShielded(livestockAnimal);
			}
			TickCalfProximity(livestockAnimal);
			return num;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void TickCalfProximity(LivestockAnimal dependent)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (!IsMale)
		{
			bool flag = (Object)(object)dependent != (Object)null && CalfThreatDistance > 0f && TryGetRememberedAggressor(out var attacker) && Vector3.Distance(((Component)attacker).transform.position, ((Component)dependent).transform.position) <= CalfThreatDistance;
			if (flag && playerLingeringOverCalf)
			{
				OnCalfThreatened();
			}
			playerLingeringOverCalf = flag;
		}
	}

	public bool HasProtectiveMotherNearby()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (MotherShelterDistance <= 0f)
		{
			return false;
		}
		LivestockAnimal motherAnimal = MotherAnimal;
		if ((Object)(object)motherAnimal != (Object)null && !motherAnimal.IsDead() && motherAnimal.IsProtectiveOfCalf)
		{
			return Vector3.Distance(((Component)this).transform.position, ((Component)motherAnimal).transform.position) <= MotherShelterDistance;
		}
		return false;
	}

	public bool HasDependentsNearby()
	{
		return (Object)(object)FindNearestDependent(SocialSearchRadius) != (Object)null;
	}

	public LivestockAnimal FindNearestDependent(float radius)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return FindNearestDependent(((Component)this).transform.position, radius);
	}

	public LivestockAnimal FindNearestDependent(Vector3 centre, float radius)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (radius <= 0f)
		{
			return null;
		}
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			Query.Server.GetBrainsInSphere(centre, radius, (List<LivestockAnimal>)(object)val);
			LivestockAnimal result = null;
			float num = float.MaxValue;
			foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
			{
				if (IsMyDependent(item))
				{
					Vector3 val2 = ((Component)item).transform.position - centre;
					float sqrMagnitude = val2.sqrMagnitude;
					if (!(sqrMagnitude >= num))
					{
						result = item;
						num = sqrMagnitude;
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

	public void RememberShielded(LivestockAnimal animal)
	{
		if ((Object)(object)animal != (Object)null)
		{
			lastShielded.Set(animal);
		}
	}

	public int ClaimShieldSlot(LivestockAnimal defender)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)defender == (Object)null)
		{
			return 0;
		}
		if (shieldSlotHolders == null)
		{
			shieldSlotHolders = new EntityRef<LivestockAnimal>[3];
		}
		if (shieldSlotClaimed == null)
		{
			shieldSlotClaimed = new TimeSince[3];
		}
		int num = -1;
		for (int i = 0; i < 3; i++)
		{
			LivestockAnimal livestockAnimal = shieldSlotHolders[i].Get(serverside: true);
			if ((Object)(object)livestockAnimal == (Object)(object)defender)
			{
				shieldSlotClaimed[i] = TimeSince.op_Implicit(0f);
				return i;
			}
			if (num < 0 && ((Object)(object)livestockAnimal == (Object)null || livestockAnimal.IsDead() || TimeSince.op_Implicit(shieldSlotClaimed[i]) > 5f))
			{
				num = i;
			}
		}
		if (num < 0)
		{
			return 0;
		}
		shieldSlotHolders[num].Set(defender);
		shieldSlotClaimed[num] = TimeSince.op_Implicit(0f);
		return num;
	}

	public LivestockAnimal FindHerdToReturnTo()
	{
		if (IsInfant())
		{
			return FindNearestHerdMate(RegroupSearchRadius);
		}
		LivestockAnimal livestockAnimal = FindMyLedDependent();
		if ((Object)(object)livestockAnimal != (Object)null)
		{
			return livestockAnimal;
		}
		LivestockAnimal livestockAnimal2 = lastShielded.Get(serverside: true);
		if ((Object)(object)livestockAnimal2 != (Object)null && !livestockAnimal2.IsDead() && !livestockAnimal2.IsDestroyed && IsMyDependent(livestockAnimal2))
		{
			return livestockAnimal2;
		}
		LivestockAnimal livestockAnimal3 = FindRememberedDependent();
		if ((Object)(object)livestockAnimal3 != (Object)null)
		{
			return livestockAnimal3;
		}
		return FindNearestHerdMate(RegroupSearchRadius);
	}

	public LivestockAnimal FindMyLedDependent()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		if (RegroupSearchRadius <= 0f)
		{
			return null;
		}
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			Query.Server.GetBrainsInSphere(((Component)this).transform.position, RegroupSearchRadius, (List<LivestockAnimal>)(object)val);
			LivestockAnimal result = null;
			float num = float.MaxValue;
			foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
			{
				if (item.IsLeading() && IsMyDependent(item))
				{
					Vector3 val2 = ((Component)item).transform.position - ((Component)this).transform.position;
					float sqrMagnitude = val2.sqrMagnitude;
					if (!(sqrMagnitude >= num))
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

	public LivestockAnimal FindNearestHerdMate(float radius)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (radius <= 0f)
		{
			return null;
		}
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			Query.Server.GetBrainsInSphere(((Component)this).transform.position, radius, (List<LivestockAnimal>)(object)val);
			LivestockAnimal result = null;
			float num = float.MaxValue;
			foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
			{
				if (IsHerdMate(item) && !item.WantsToBeAlone)
				{
					Vector3 val2 = ((Component)item).transform.position - ((Component)this).transform.position;
					float sqrMagnitude = val2.sqrMagnitude;
					if (!(sqrMagnitude >= num))
					{
						result = item;
						num = sqrMagnitude;
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

	public void GatherRestingFootprints(Vector3 around, PooledList<LivestockFootprint> footprints)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			Query.Server.GetBrainsInSphere(around, 30f, (List<LivestockAnimal>)(object)val);
			foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
			{
				if (!((Object)(object)item == (Object)(object)this) && !item.IsDead())
				{
					((List<LivestockFootprint>)(object)footprints).Add(item.RestingFootprint());
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public LivestockFootprint RestingFootprint()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		RustNavMeshAgent rustNavMeshAgent = RestingAgent;
		if ((Object)(object)rustNavMeshAgent == (Object)null || !rustNavMeshAgent.hasPath)
		{
			return LivestockFootprint.Of(this);
		}
		return RestingFootprintFor(rustNavMeshAgent.destinationWS, rustNavMeshAgent.currentStoppingDistance);
	}

	public LivestockFootprint RestingFootprintFor(Vector3 destination, float stoppingDistance)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		RustNavMeshAgent rustNavMeshAgent = RestingAgent;
		Vector3 val = Vector3Ex.WithY(destination - ((Component)this).transform.position, 0f);
		float magnitude = val.magnitude;
		float num = (((Object)(object)rustNavMeshAgent != (Object)null) ? rustNavMeshAgent.RestingDistance(stoppingDistance) : stoppingDistance);
		if (magnitude <= num)
		{
			return LivestockFootprint.Of(this);
		}
		return LivestockFootprint.Of(this, destination - val * (num / magnitude), val);
	}

	public static float ClearanceOf(PooledList<LivestockFootprint> footprints, in LivestockFootprint footprint)
	{
		float num = float.MaxValue;
		foreach (LivestockFootprint item in (List<LivestockFootprint>)(object)footprints)
		{
			num = Mathf.Min(num, 0f - LivestockFootprint.Penetration(in footprint, item));
		}
		return num;
	}

	public static bool IsClear(float clearance)
	{
		return clearance >= 0.25f;
	}

	public bool IsWalkingAfter(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null)
		{
			if (!IsFollowing(player))
			{
				return (Object)(object)CuriousAbout == (Object)(object)player;
			}
			return true;
		}
		return false;
	}

	public void CountWalkingAbreast(BasePlayer player, out int onLeft, out int onRight)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		onLeft = 0;
		onRight = 0;
		if ((Object)(object)player == (Object)null)
		{
			return;
		}
		Vector3 position = ((Component)player).transform.position;
		Vector3 position2 = ((Component)this).transform.position;
		Vector3 val = Vector3Ex.WithY(position - position2, 0f);
		PooledList<LivestockAnimal> val2 = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			Query.Server.GetBrainsInSphere(position, 10f, (List<LivestockAnimal>)(object)val2);
			foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val2)
			{
				if ((Object)(object)item == (Object)(object)this || !item.IsWalkingAfter(player))
				{
					continue;
				}
				Vector3 position3 = ((Component)item).transform.position;
				if (!(Vector3.Dot(Vector3Ex.WithY(position3 - position, 0f), Vector3Ex.WithY(position2 - position, 0f)) <= 0f))
				{
					float num = Vector3.Cross(val, Vector3Ex.WithY(position3 - position2, 0f)).y;
					if (Mathf.Abs(num) < 0.01f)
					{
						num = ((item.net.ID.Value < net.ID.Value) ? (-1f) : 1f);
					}
					if (num < 0f)
					{
						onLeft++;
					}
					else
					{
						onRight++;
					}
				}
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	public bool TryFindSecludedSpot(RustNavMeshAgent agent, LivestockAnimal partner, float minDistance, float maxDistance, float clearRadius, out Vector3 spot)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		spot = default;
		if ((Object)(object)agent == (Object)null || maxDistance <= 0f)
		{
			return false;
		}
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			Query.Server.GetBrainsInSphere(((Component)this).transform.position, maxDistance + clearRadius, (List<LivestockAnimal>)(object)val);
			PooledList<Vector3> val2 = Pool.Get<PooledList<Vector3>>();
			try
			{
				foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
				{
					if (IsHerdMate(item) && !((Object)(object)item == (Object)(object)partner))
					{
						((List<Vector3>)(object)val2).Add(((Component)item).transform.position);
					}
				}
				PooledList<NavVector3> val3 = Pool.Get<PooledList<NavVector3>>();
				try
				{
					Eqs.SampleNavigablePositions(agent, agent.nextPosition, (List<NavVector3>)(object)val3, maxDistance, minDistance, 8);
					TryGetHerdCentroid(out var centroid);
					bool flag = false;
					float num = float.MinValue;
					NavVector3 locationNS = default;
					foreach (NavVector3 item2 in (List<NavVector3>)(object)val3)
					{
						if (agent.SamplePosition(item2, out var hitNS, 5f) && !IsCrowded(val2, hitNS.position.Value, clearRadius))
						{
							Vector3 val4 = hitNS.position.Value - centroid;
							float sqrMagnitude = val4.sqrMagnitude;
							if (!(sqrMagnitude <= num))
							{
								num = sqrMagnitude;
								locationNS = hitNS.position;
								flag = true;
							}
						}
					}
					if (!flag || !agent.CanReach(locationNS))
					{
						return false;
					}
					spot = locationNS.Value;
					return true;
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
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

	private static bool IsCrowded(PooledList<Vector3> crowd, Vector3 point, float clearRadius)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		float num = clearRadius * clearRadius;
		foreach (Vector3 item in (List<Vector3>)(object)crowd)
		{
			Vector3 val = item - point;
			if (val.sqrMagnitude < num)
			{
				return true;
			}
		}
		return false;
	}

	private LivestockAnimal FindRememberedDependent()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		SenseComponent senseComponent = default;
		if (!((Component)this).TryGetComponent<SenseComponent>(ref senseComponent))
		{
			return null;
		}
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			senseComponent.GetOncePerceivedEntities((List<BaseEntity>)(object)val);
			LivestockAnimal result = null;
			float num = float.MaxValue;
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				if (item is LivestockAnimal livestockAnimal && IsMyDependent(livestockAnimal) && senseComponent.FindLKP(livestockAnimal, out var lkp))
				{
					Vector3 val2 = lkp - ((Component)this).transform.position;
					float sqrMagnitude = val2.sqrMagnitude;
					if (!(sqrMagnitude >= num))
					{
						result = livestockAnimal;
						num = sqrMagnitude;
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

	public bool TryGetHerdCentroid(out Vector3 centroid)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		centroid = ((Component)this).transform.position;
		if (SocialSearchRadius <= 0f)
		{
			return false;
		}
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			Query.Server.GetBrainsInSphere(((Component)this).transform.position, SocialSearchRadius, (List<LivestockAnimal>)(object)val);
			Vector3 val2 = ((Component)this).transform.position;
			int num = 1;
			foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
			{
				if (IsHerdMate(item) && !item.WantsToBeAlone)
				{
					val2 += ((Component)item).transform.position;
					num++;
				}
			}
			if (num <= 1)
			{
				return false;
			}
			centroid = val2 / (float)num;
			return true;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool TryGetHerdFleeHeading(out Vector3 heading)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		heading = herdFleeHeading;
		if (hasHerdFleeHeading)
		{
			return TimeSince.op_Implicit(herdFleeHeadingSet) < 10f;
		}
		return false;
	}

	public void PublishHerdFleeHeading(Vector3 heading)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		heading = Vector3Ex.WithY(heading, 0f);
		if (heading.sqrMagnitude < 0.0001f)
		{
			return;
		}
		heading = heading.normalized;
		AdoptHerdFleeHeading(heading);
		if (SocialSearchRadius <= 0f)
		{
			return;
		}
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			Query.Server.GetBrainsInSphere(((Component)this).transform.position, SocialSearchRadius, (List<LivestockAnimal>)(object)val);
			foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
			{
				if (IsHerdMate(item))
				{
					item.AdoptHerdFleeHeading(heading);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void AdoptHerdFleeHeading(Vector3 heading)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		herdFleeHeading = heading;
		herdFleeHeadingSet = TimeSince.op_Implicit(0f);
		hasHerdFleeHeading = true;
	}

	public float HerdLoadTarget(int herdMates)
	{
		int num = Mathf.Max(0, ComfortableHerdSize);
		int num2 = Mathf.Max(num + 1, CrowdedHerdSize);
		if (herdMates <= num)
		{
			return 1f;
		}
		return Mathf.Lerp(1f, 0.3f, Mathf.InverseLerp((float)num, (float)num2, (float)herdMates));
	}

	public float PackingTarget(float neighbours)
	{
		float num = Mathf.Max(0f, ComfortablePacking);
		float num2 = Mathf.Max(num + 0.01f, CrowdedPacking);
		float num3 = Mathf.Max(num2 + 0.01f, SeverePacking);
		if (neighbours <= num)
		{
			return 1f;
		}
		if (neighbours <= num2)
		{
			return Mathf.Lerp(1f, 0.3f, Mathf.InverseLerp(num, num2, neighbours));
		}
		return Mathf.Lerp(0.3f, 0f, Mathf.InverseLerp(num2, num3, neighbours));
	}

	public float CountPackingNeighbours()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if (PackingRadius <= 0f)
		{
			return 0f;
		}
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			Query.Server.GetBrainsInSphere(((Component)this).transform.position, PackingRadius, (List<LivestockAnimal>)(object)val);
			float num = PackingRadius * PackingRadius;
			float num2 = 0f;
			foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
			{
				if ((Object)(object)item == (Object)null || (Object)(object)item == (Object)(object)this || item.IsDestroyed || item.IsDead())
				{
					continue;
				}
				Vector3 val2 = ((Component)item).transform.position - ((Component)this).transform.position;
				if (!(Mathf.Abs(val2.y) > PackingHeight))
				{
					Vector3 val3 = Vector3Ex.WithY(val2, 0f);
					if (!(val3.sqrMagnitude > num))
					{
						num2 += Mathf.Max(0f, item.AdultEquivalent);
					}
				}
			}
			return num2;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void TickPersonalSpace(ref TickableNeed need, float seconds)
	{
		int herdMates = CountHerdMates();
		if (!(PersonalSpaceChangeRate <= 0f) && !TOD_Sky.Instance.IsNight && !IsSleeping())
		{
			lastHerdLoadTarget = HerdLoadTarget(herdMates);
			lastPackingTarget = PackingTarget(CountPackingNeighbours());
			float num = Mathf.Min(lastHerdLoadTarget, lastPackingTarget);
			need.NeedValue = Mathf.MoveTowards(need.NeedValue, num, PersonalSpaceChangeRate * seconds);
		}
	}

	public LivestockSpecial SpecialIdentity()
	{
		LivestockSpecialTable instance = LivestockSpecialTable.Instance;
		if (!((Object)(object)instance != (Object)null))
		{
			return null;
		}
		return instance.Find(AnimalName);
	}

	public static LivestockAnimal FindLivingSpecial(string name)
	{
		if (string.IsNullOrEmpty(name) || !livingSpecials.TryGetValue(name, out var value))
		{
			return null;
		}
		if (!((Object)(object)value != (Object)null) || value.IsDestroyed || value.IsDead())
		{
			return null;
		}
		return value;
	}

	public static void QueueNext(LivestockSpecial special)
	{
		queuedSpecial = special;
	}

	public static bool IsSpecialDiscovered(string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			return discoveredSpecials.Contains(name);
		}
		return false;
	}

	public static string[] DiscoveredSpecialNames()
	{
		if (discoveredSpecials.Count == 0)
		{
			return Array.Empty<string>();
		}
		string[] array = new string[discoveredSpecials.Count];
		discoveredSpecials.CopyTo(array);
		return array;
	}

	public static void LoadDiscoveredSpecials(string[] names)
	{
		discoveredSpecials.Clear();
		if (names == null)
		{
			return;
		}
		for (int i = 0; i < names.Length; i++)
		{
			if (!string.IsNullOrEmpty(names[i]))
			{
				discoveredSpecials.Add(names[i]);
			}
		}
	}

	private void RememberSpecial(string name, bool discovered)
	{
		LivestockSpecialTable instance = LivestockSpecialTable.Instance;
		if (!string.IsNullOrEmpty(name) && !((Object)(object)instance == (Object)null) && instance.Find(name) != null)
		{
			livingSpecials[name] = this;
			if (discovered)
			{
				discoveredSpecials.Add(name);
			}
		}
	}

	private void ForgetSpecial()
	{
		ForgetSpecial(AnimalName);
	}

	private void ForgetSpecial(string name)
	{
		if (!string.IsNullOrEmpty(name) && livingSpecials.TryGetValue(name, out var value) && (Object)(object)value == (Object)(object)this)
		{
			livingSpecials.Remove(name);
		}
	}

	public bool IsPopulationSpawn()
	{
		Spawnable spawnable = default;
		if (((Component)this).TryGetComponent<Spawnable>(ref spawnable))
		{
			return spawnable.Population != null;
		}
		return false;
	}

	private void TryGrantSpecial()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		if (Application.isLoadingSave)
		{
			return;
		}
		LivestockSpecialTable instance = LivestockSpecialTable.Instance;
		if ((Object)(object)instance == (Object)null || instance.Entries == null || instance.Entries.Length == 0)
		{
			return;
		}
		LivestockSpecial livestockSpecial = TakeQueued();
		bool byCommand = livestockSpecial != null;
		if (livestockSpecial == null)
		{
			if (!IsPopulationSpawn() || Random.value >= Livestock.specialChance)
			{
				return;
			}
			livestockSpecial = PickSpecial(instance);
		}
		if (livestockSpecial != null)
		{
			Apply(livestockSpecial, byCommand);
			if (Livestock.debugSpecials)
			{
				Debug.Log((object)$"[Livestock] {ShortPrefabName} spawned as {livestockSpecial.Name} at {((Component)this).transform.position}.");
			}
		}
	}

	private LivestockSpecial TakeQueued()
	{
		LivestockSpecial livestockSpecial = queuedSpecial;
		if (livestockSpecial == null)
		{
			return null;
		}
		queuedSpecial = null;
		if (!Fits(livestockSpecial) || !((Object)(object)FindLivingSpecial(livestockSpecial.Name) == (Object)null))
		{
			return null;
		}
		return livestockSpecial;
	}

	public bool Fits(LivestockSpecial special)
	{
		if (special == null || string.IsNullOrEmpty(special.Name))
		{
			return false;
		}
		if (special.Sex != SexForm.Either && special.Sex == SexForm.Male != IsMale)
		{
			return false;
		}
		if (special.Species == null || special.Species.Length == 0)
		{
			return true;
		}
		LivestockSpecies[] species = special.Species;
		foreach (LivestockSpecies livestockSpecies in species)
		{
			if ((Object)(object)livestockSpecies != (Object)null && (Object)(object)livestockSpecies == (Object)(object)Species)
			{
				return true;
			}
		}
		return false;
	}

	private LivestockSpecial PickSpecial(LivestockSpecialTable table)
	{
		LivestockSpecial[] entries = table.Entries;
		if (eligibleSpecials == null || eligibleSpecials.Length < entries.Length)
		{
			eligibleSpecials = new bool[entries.Length];
		}
		float num = 0f;
		LivestockSpecial result = null;
		for (int i = 0; i < entries.Length; i++)
		{
			LivestockSpecial livestockSpecial = entries[i];
			eligibleSpecials[i] = livestockSpecial != null && livestockSpecial.Weight > 0f && Fits(livestockSpecial) && (Object)(object)FindLivingSpecial(livestockSpecial.Name) == (Object)null;
			if (eligibleSpecials[i])
			{
				num += livestockSpecial.Weight;
				result = livestockSpecial;
			}
		}
		if (num <= 0f)
		{
			return null;
		}
		float num2 = Random.value * num;
		for (int j = 0; j < entries.Length; j++)
		{
			if (eligibleSpecials[j])
			{
				num2 -= entries[j].Weight;
				if (num2 <= 0f)
				{
					return entries[j];
				}
			}
		}
		return result;
	}

	private void Apply(LivestockSpecial special, bool byCommand)
	{
		AnimalName = special.Name;
		FoundLine(LivestockLocaleWeights.HouseLocale());
		RememberSpecial(special.Name, !byCommand);
		ApplyGenes(special);
	}

	private void ApplyGenes(LivestockSpecial special)
	{
		if (special.Genes != null)
		{
			ForcedGene[] genes = special.Genes;
			foreach (ForcedGene forcedGene in genes)
			{
				if (forcedGene != null)
				{
					Genes = LivestockGenome.WithPair(Genes, forcedGene.Gene, forcedGene.First, forcedGene.Second);
				}
			}
		}
		int num = LivestockGenome.RollMarker();
		int num2 = LivestockGenome.RollMarker();
		if (num2 == num)
		{
			num2 = (num + 1 + Random.Range(0, 15)) % 16;
		}
		Genes = LivestockGenome.WithMarkers(Genes, num, num2);
	}

	private bool TryNameFromCross(LivestockAnimal mother, LivestockAnimal father)
	{
		if ((Object)(object)mother == (Object)null || (Object)(object)father == (Object)null)
		{
			return false;
		}
		LivestockSpecialTable instance = LivestockSpecialTable.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			return false;
		}
		LivestockSpecial livestockSpecial = instance.Find(mother.AnimalName);
		LivestockSpecial livestockSpecial2 = instance.Find(father.AnimalName);
		if (livestockSpecial == null || livestockSpecial2 == null)
		{
			return false;
		}
		LivestockCross livestockCross = instance.CrossFor(livestockSpecial.Name, livestockSpecial2.Name);
		if (livestockCross == null)
		{
			return false;
		}
		AnimalName = livestockCross.Name;
		FoundLine(LivestockLocaleWeights.HouseLocale());
		return true;
	}

	private float SpeciesSaleScale()
	{
		if (!((Object)(object)Species != (Object)null))
		{
			return 1f;
		}
		return Mathf.Max(0f, Species.SaleValueScale);
	}

	private float GeneSaleScale()
	{
		float num = 0f;
		LivestockGene[] allGenes = LivestockGenome.AllGenes;
		foreach (LivestockGene gene in allGenes)
		{
			num += GeneScale(gene);
		}
		float num2 = num / 5f;
		return Mathf.Max(0f, 1f + (num2 - 1f) * Livestock.priceGeneWeight);
	}

	private float ConditionSaleScale()
	{
		float priceConditionFloor = Livestock.priceConditionFloor;
		float num = Mathf.Max(priceConditionFloor + 0.01f, Livestock.priceConditionFull);
		return Mathf.Lerp(Mathf.Clamp01(Livestock.priceWorstScale), 1f, Mathf.InverseLerp(priceConditionFloor, num, Condition));
	}

	private float AgeSaleScale()
	{
		float num = Mathf.Clamp01(Livestock.priceAgePrime);
		return Mathf.Lerp(1f, Mathf.Clamp01(Livestock.priceOldestScale), Mathf.InverseLerp(num, 1f, AgeFraction));
	}

	private float OriginSaleScale()
	{
		if (!IsBred)
		{
			return Mathf.Clamp01(1f - Livestock.priceWildPenalty);
		}
		return 1f;
	}

	private void SaveSaleValue(LivestockAnimal msg)
	{
		msg.saleValueScale = SaleValueScale;
	}

	public float StartingValueFor(Need need)
	{
		return need switch
		{
			Need.Fullness => Mathf.Clamp01(StartingFullness), 
			Need.Hydration => Mathf.Clamp01(StartingHydration), 
			_ => 1f, 
		};
	}

	public float DecayRatePerSecond(Need need)
	{
		return need switch
		{
			Need.Fullness => Mathf.Max(0f, FullnessDecayRate), 
			Need.Hydration => Mathf.Max(0f, HydrationDecayRate), 
			_ => 0f, 
		};
	}

	public bool IsInfant()
	{
		return Age == AgeStage.Infant;
	}

	public bool IsAdult()
	{
		return Age == AgeStage.Adult;
	}

	public bool IsLeading()
	{
		return HasFlag(Flags.Reserved7);
	}

	public bool IsPregnant()
	{
		return HasFlag(Flags.Reserved10);
	}

	public bool IsMating()
	{
		return HasFlag(Flags.Reserved12);
	}

	public bool IsGrazing()
	{
		return HasFlag(Flags.Reserved8);
	}

	public bool IsDrinking()
	{
		return HasFlag(Flags.Reserved13);
	}

	public bool IsSleeping()
	{
		return HasFlag(Flags.Reserved11);
	}

	public bool IsResting()
	{
		return HasFlag(Flags.Reserved14);
	}

	public bool IsLyingIn()
	{
		return HasFlag(Flags.Reserved17);
	}

	public bool IsFrisky()
	{
		return HasFlag(Flags.Reserved15);
	}

	public bool IsLyingDown()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (IsSleeping())
		{
			return TimeSince.op_Implicit(poseHeldFor) >= Mathf.Max(0f, BedDownDuration);
		}
		if (IsResting() || IsLyingIn())
		{
			return TimeSince.op_Implicit(poseHeldFor) >= Mathf.Max(0f, LieDownDuration);
		}
		return false;
	}

	public bool IsSettlingDown()
	{
		if (IsHoldingAPoseFlag())
		{
			return !IsLyingDown();
		}
		return false;
	}

	private bool IsHoldingAPoseFlag()
	{
		if (!IsSleeping() && !IsResting())
		{
			return IsLyingIn();
		}
		return true;
	}

	public bool IsOffItsFeet()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (!IsHoldingAPoseFlag())
		{
			return TimeUntil.op_Implicit(onItsFeetIn) > 0f;
		}
		return true;
	}

	public void MarkOnItsFeet()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		onItsFeetIn = TimeUntil.op_Implicit(0f);
	}

	private void StartGettingUp(float settleDuration, float riseDuration)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Max(0f, Mathf.Max(0f, settleDuration) - TimeSince.op_Implicit(poseHeldFor));
		onItsFeetIn = TimeUntil.op_Implicit(num + Mathf.Max(0f, riseDuration));
	}

	public bool IsHeadDown()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!IsGrazing() && !IsDrinking())
		{
			return TimeSince.op_Implicit(headComingUpFor) < Mathf.Max(0f, RaiseHeadDuration);
		}
		return true;
	}

	public bool IsFollowing(BasePlayer player)
	{
		return (Object)(object)LeadingPlayer.Get(isServer) == (Object)(object)player;
	}

	public bool IsSameSpeciesAs(LivestockAnimal other)
	{
		if ((Object)(object)other == (Object)null)
		{
			return false;
		}
		if ((Object)(object)Species != (Object)null || (Object)(object)other.Species != (Object)null)
		{
			return (Object)(object)Species == (Object)(object)other.Species;
		}
		return prefabID == other.prefabID;
	}

	public GameObjectRef FormForSex(bool male)
	{
		if (!((Object)(object)Species != (Object)null))
		{
			return null;
		}
		return Species.For(Age, male);
	}

	public TickableNeed GetNeed(Need need)
	{
		if ((int)need >= needs.Length)
		{
			return default;
		}
		return needs[(int)need];
	}

	public override void InitShared()
	{
		base.InitShared();
		needs = new TickableNeed[3]
		{
			new TickableNeed(StartingValueFor(Need.Fullness)),
			new TickableNeed(StartingValueFor(Need.PersonalSpace)),
			new TickableNeed(StartingValueFor(Need.Hydration))
		};
	}

	public bool CanStopLead(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null && IsLeading())
		{
			return (Object)(object)LeadingPlayer.Get(isServer) == (Object)(object)player;
		}
		return false;
	}

	public void MarkNewborn()
	{
		findingItsFeet = true;
	}

	public void FinishFindingItsFeet()
	{
		findingItsFeet = false;
		LivestockAnimal motherAnimal = MotherAnimal;
		if ((Object)(object)motherAnimal != (Object)null)
		{
			motherAnimal.newbornCalf = default;
		}
	}

	public bool TryGetNewbornCalf(out LivestockAnimal calf)
	{
		calf = newbornCalf.Get(isServer);
		if ((Object)(object)calf == (Object)null || calf.IsDead() || !calf.IsFindingItsFeet)
		{
			calf = null;
			return false;
		}
		return true;
	}

	public override void ServerInit()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		timers = new PersistentTimerSet(this);
		pregnancyTimer = new PersistentTimer(timers, 2)
		{
			onElapsed = () =>
			{
				GiveBirth();
			},
			onStarted = () =>
			{
				SetNetworkedFlag(Flags.Reserved10, value: true);
			}
		};
		breedCooldown = new PersistentTimer(timers, 3);
		((PersistentObjectWorkQueue<LivestockAnimal>)NeedsQueue).Add(this);
		JoinPopulation();
		lastLoudNoise = TimeSince.op_Implicit(100f);
		lastHerdDistress = TimeSince.op_Implicit(100f);
		lastAggression = TimeSince.op_Implicit(float.MaxValue);
		if (Form != SexForm.Either)
		{
			IsMale = Form == SexForm.Male;
		}
		else if (!Application.isLoadingSave && !IsBeingPurchased)
		{
			IsMale = Random.Range(0f, 1f) > 0.5f;
		}
		ValidateSexForm();
		InitGenes();
		TryGrantSpecial();
		InitName();
		InitAging();
		InitDung();
		InitSleep();
		InitFrailty();
		InitDayBudget();
		InitFamiliarity();
		InitHome();
		InitCondition();
		lastNeedsTick = TimeSince.op_Implicit(0f);
		headComingUpFor = TimeSince.op_Implicit(float.MaxValue);
		IsBeingPurchased = false;
	}

	internal override void DoServerDestroy()
	{
		if (IsLeading())
		{
			ReleaseLeaderSpeed();
		}
		base.DoServerDestroy();
		((PersistentObjectWorkQueue<LivestockAnimal>)NeedsQueue).Remove(this);
		LeavePopulation();
		ForgetSpecial();
	}

	private void ValidateSexForm()
	{
	}

	public override void Save(SaveInfo info)
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.livestockAnimal = Pool.Get<LivestockAnimal>();
			info.msg.livestockAnimal.timers = Pool.Get<List<PersistentTimer>>();
			timers.Save(info.msg.livestockAnimal.timers, info.cachedTime.Time);
			info.msg.livestockAnimal.needs = Pool.Get<List<LivestockNeed>>();
			TickableNeed[] array = needs;
			foreach (TickableNeed tickableNeed in array)
			{
				info.msg.livestockAnimal.needs.Add(tickableNeed.ToSavedData());
			}
			info.msg.livestockAnimal.mother = Mother.uid;
			info.msg.livestockAnimal.father = Father.uid;
			SaveName(info.msg.livestockAnimal);
			SaveFamiliarity(info.msg.livestockAnimal);
			SaveHome(info.msg.livestockAnimal);
			SaveCondition(info.msg.livestockAnimal);
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.livestockAnimal != null && isServer && info.fromDisk)
		{
			timers.Load(info.msg.livestockAnimal.timers);
			List<LivestockNeed> list = info.msg.livestockAnimal.needs;
			if (list != null)
			{
				for (int i = 0; i < needs.Length && i < list.Count; i++)
				{
					needs[i].LoadFrom(list[i]);
				}
			}
			Mother.uid = info.msg.livestockAnimal.mother;
			Father.uid = info.msg.livestockAnimal.father;
			LoadName(info.msg.livestockAnimal);
			LoadFamiliarity(info.msg.livestockAnimal);
			LoadHome(info.msg.livestockAnimal);
			LoadCondition(info.msg.livestockAnimal);
		}
		if (info.fromDisk && IsLeading())
		{
			StopLeading();
		}
	}

	public bool CanLead(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null && !player.IsDead() && !IsDead() && !IsSoreAt(player) && !IsMounted)
		{
			return TrustsToHandle(player);
		}
		return false;
	}

	public bool TakeStartle()
	{
		bool result = startled;
		startled = false;
		return result;
	}

	public void OnGrabbedBy(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null || player.IsDead())
		{
			return;
		}
		if (IsInfant())
		{
			LivestockAnimal motherAnimal = MotherAnimal;
			if ((Object)(object)motherAnimal != (Object)null && !motherAnimal.IsDead())
			{
				motherAnimal.OnGrabbedAtDependent(player);
			}
		}
		SenseComponent senseComponent = default;
		if (((Component)this).TryGetComponent<SenseComponent>(ref senseComponent) && senseComponent.CanTarget(player))
		{
			senseComponent.TrySetTarget(player);
		}
		if (StandsItsGround)
		{
			Provoke(player);
			return;
		}
		startled = true;
		Spook();
	}

	private void OnGrabbedAtDependent(BasePlayer player)
	{
		OnCalfThreatened();
		SenseComponent senseComponent = default;
		if (((Component)this).TryGetComponent<SenseComponent>(ref senseComponent) && senseComponent.CanTarget(player))
		{
			senseComponent.TrySetTarget(player);
		}
		Provoke(player);
	}

	private void Provoke(BasePlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		lastHerdDistress = TimeSince.op_Implicit(0f);
		herdDistressAttacker.Set(player);
		ClearSpook();
	}

	public void StopLeading()
	{
		ReleaseLeaderSpeed();
		SetLeading(leading: false);
		LeadingPlayer = default;
	}

	private void StashLeadForMount()
	{
		if (IsLeading())
		{
			leaderBeforeMount = LeadingPlayer;
		}
		StopLeading();
	}

	private void RestoreLeadAfterMount()
	{
		BasePlayer basePlayer = leaderBeforeMount.Get(serverside: true);
		leaderBeforeMount = default;
		if (!((Object)(object)basePlayer == (Object)null) && CanLead(basePlayer))
		{
			SetLeading(leading: true);
			LeadingPlayer = RefTo(basePlayer);
		}
	}

	private void TickLead()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (!IsLeading())
		{
			return;
		}
		BasePlayer basePlayer = LeadingPlayer.Get(isServer);
		if ((Object)(object)basePlayer == (Object)null || basePlayer.IsDestroyed || basePlayer.IsDead() || basePlayer.IsSleeping())
		{
			StopLeading();
			return;
		}
		float num = Vector3.Distance(((Component)basePlayer).transform.position, ((Component)this).transform.position);
		if (Livestock.leadBreakDistance > 0f && num > Livestock.leadBreakDistance)
		{
			StopLeading();
		}
		else
		{
			SlowLeaderWhenTaut(basePlayer, num);
		}
	}

	private void SlowLeaderWhenTaut(BasePlayer leader, float distance)
	{
		if (!((Object)(object)leader.modifiers == (Object)null))
		{
			float num = 0f;
			if (distance > 3.5f && IsPulledAwayBy(leader))
			{
				num = 0f - Mathf.Lerp(0f, 0.9f, Mathf.InverseLerp(3.5f, 7f, distance));
			}
			if (!TryGetLeaderSpeed(leader, out var value))
			{
				PlayerModifiers.AddToPlayer(leader, leadModifier);
				value = 0f;
			}
			if (Mathf.Abs(value - num) > 0.01f)
			{
				leader.modifiers.SetValue(Modifier.ModifierSource.Interaction, Modifier.ModifierType.MoveSpeed, num);
			}
		}
	}

	public bool IsPulledAwayBy(BasePlayer leader)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Dot(Vector3Ex.NormalizeXZ(((Component)leader).transform.position - ((Component)this).transform.position), Vector3Ex.NormalizeXZ(leader.estimatedVelocity)) > 0.5f;
	}

	private static bool TryGetLeaderSpeed(BasePlayer leader, out float value)
	{
		foreach (Modifier item in leader.modifiers.All)
		{
			if (item != null && item.Source == Modifier.ModifierSource.Interaction && item.Type == Modifier.ModifierType.MoveSpeed)
			{
				value = item.Value;
				return true;
			}
		}
		value = 0f;
		return false;
	}

	private void ReleaseLeaderSpeed()
	{
		BasePlayer basePlayer = LeadingPlayer.Get(isServer);
		if ((Object)(object)basePlayer != (Object)null && !basePlayer.IsDestroyed && (Object)(object)basePlayer.modifiers != (Object)null)
		{
			basePlayer.modifiers.RemoveFromSource(Modifier.ModifierSource.Interaction);
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	private void RPC_Lead(RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null))
		{
			bool wantsLead = msg.read.Bit();
			TryLead(msg.player, wantsLead);
		}
	}

	public bool TryLead(BasePlayer player, bool wantsLead)
	{
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (wantsLead && !CanLead(player))
		{
			if (!IsLeading() && !IsMounted)
			{
				OnGrabbedBy(player);
			}
			return false;
		}
		if (!wantsLead && !CanStopLead(player))
		{
			return false;
		}
		if (!wantsLead)
		{
			StopLeading();
			return true;
		}
		if (IsLeading() && (Object)(object)LeadingPlayer.Get(isServer) != (Object)(object)player)
		{
			ReleaseLeaderSpeed();
		}
		SetLeading(leading: true);
		LeadingPlayer = RefTo(player);
		return true;
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (isServer && (old & Flags.Busy) != (next & Flags.Busy))
		{
			if ((next & Flags.Busy) != 0)
			{
				StashLeadForMount();
			}
			else
			{
				RestoreLeadAfterMount();
			}
		}
	}

	public virtual bool TryGetYield(out ItemDefinition item, out int amount, out float interval, out float bestOfBreed)
	{
		item = null;
		amount = 0;
		interval = 0f;
		bestOfBreed = 0f;
		return false;
	}

	public virtual void AddCorpseYield(ResourceDispenser dispenser)
	{
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server]
	private void RequestAnimalStats(RPCMessage msg)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)msg.player == (Object)null)
		{
			return;
		}
		LivestockAnimal val = Pool.Get<LivestockAnimal>();
		try
		{
			val.needs = Pool.Get<List<LivestockNeed>>();
			TickableNeed[] array = needs;
			foreach (TickableNeed tickableNeed in array)
			{
				val.needs.Add(tickableNeed.ToSavedData());
			}
			val.overgrazed = OvergrazedArea.IsOvergrazed(((Component)this).transform.position);
			float seconds = TrustOf(msg.player);
			val.trustTier = (int)TierFor(seconds);
			val.trustProgress = ProgressInTier(seconds);
			val.ageSeconds = AgeSeconds;
			val.ageFraction = AgeFraction;
			if (TryGetYield(out var item, out var amount, out var interval, out var bestOfBreed))
			{
				val.yieldItemId = item.itemid;
				val.yieldAmount = amount;
				val.yieldInterval = interval;
				val.yieldFraction = bestOfBreed;
			}
			val.dungInterval = (DungEnabled ? NextDungInterval() : 0f);
			val.dungFraction = (DungEnabled ? DungBestOfBreed() : 0f);
			val.dungItemId = (DungEnabled ? DungItem.itemid : 0);
			SaveSaleValue(val);
			ClientRPC(RpcTarget.Player("OnReceivedAnimalStats", msg.player), val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override void ScaleDamage(HitInfo info)
	{
		base.ScaleDamage(info);
		if ((Object)(object)info.Initiator != (Object)null && (Object)(object)info.Initiator != (Object)(object)this && InSafeZone())
		{
			info.damageTypes.ScaleAll(0f);
		}
	}

	public override void Hurt(HitInfo info)
	{
		if (Interface.CallHook("OnEntityTakeDamage", this, info) != null)
		{
			return;
		}
		base.Hurt(info);
		if (!InSafeZone())
		{
			if (IsLeading())
			{
				StopLeading();
			}
			StopCuriousFollow();
			RememberAggressor(info?.Initiator);
			ChargeFamiliarityPenalty(info?.InitiatorPlayer, Livestock.familiarityHurtPenalty);
			if ((Object)(object)info?.Initiator != (Object)null && !IsMale && HasDependentsNearby())
			{
				OnCalfThreatened();
			}
			AlertHerdOfDistress(info);
		}
	}

	public void OnHeardNoise(NpcNoiseEvent noise)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (noise.Intensity >= NpcNoiseIntensity.Medium)
		{
			lastLoudNoise = TimeSince.op_Implicit(0f);
			lastLoudNoisePosition = noise.NoisePosition;
			SenseComponent senseComponent = default;
			if ((Object)(object)noise.Initiator != (Object)null && noise.Initiator.ToNonNpcPlayer(out var player) && ((Component)this).TryGetComponent<SenseComponent>(ref senseComponent) && senseComponent.CanTarget(player))
			{
				senseComponent.TrySetTarget(player);
			}
		}
	}

	public bool HeardLoudNoiseWithin(float range)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (RecentlyHeardLoudNoise)
		{
			return Vector3.Distance(((Component)this).transform.position, lastLoudNoisePosition) <= range;
		}
		return false;
	}

	public void Spook()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		lastLoudNoise = TimeSince.op_Implicit(0f);
	}

	public void ClearSpook()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		lastLoudNoise = TimeSince.op_Implicit(100f);
	}

	private bool CanBeAnsweredFor(BaseEntity attacker, out BaseCombatEntity combatAttacker)
	{
		combatAttacker = attacker as BaseCombatEntity;
		if ((Object)(object)combatAttacker == (Object)null || combatAttacker.IsDead() || (Object)(object)combatAttacker == (Object)(object)this)
		{
			return false;
		}
		if (combatAttacker is LivestockAnimal candidate)
		{
			return !IsHerdMate(candidate);
		}
		return true;
	}

	public void RememberAggressor(BaseEntity attacker)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (CanBeAnsweredFor(attacker, out var combatAttacker))
		{
			if (!IsSoreAt(combatAttacker))
			{
				penaltyCharged = 0f;
			}
			aggressor.Set(combatAttacker);
			lastAggression = TimeSince.op_Implicit(0f);
		}
	}

	private void ChargeFamiliarityPenalty(BasePlayer player, float penalty)
	{
		if (!((Object)(object)player == (Object)null) && !(penalty <= penaltyCharged))
		{
			AddFamiliarity(player.userID, 0f - (penalty - penaltyCharged));
			penaltyCharged = penalty;
		}
	}

	public bool TryGetRememberedAggressor(out BaseCombatEntity attacker)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		attacker = null;
		float grudgeDuration = Livestock.grudgeDuration;
		if (grudgeDuration <= 0f || TimeSince.op_Implicit(lastAggression) > grudgeDuration)
		{
			return false;
		}
		BaseCombatEntity baseCombatEntity = aggressor.Get(isServer);
		if ((Object)(object)baseCombatEntity == (Object)null || baseCombatEntity.IsDead())
		{
			return false;
		}
		attacker = baseCombatEntity;
		return true;
	}

	public bool TryGetHerdDistressAttacker(out BaseCombatEntity attacker)
	{
		attacker = null;
		if (!HerdMateRecentlyHurt)
		{
			return false;
		}
		BaseCombatEntity baseCombatEntity = herdDistressAttacker.Get(isServer);
		if ((Object)(object)baseCombatEntity == (Object)null || baseCombatEntity.IsDead())
		{
			return false;
		}
		attacker = baseCombatEntity;
		return true;
	}

	public void OnCalfThreatened()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		lastCalfThreat = TimeSince.op_Implicit(0f);
		calfEverThreatened = true;
		ClearSpook();
	}

	public void ForgetCalfThreat()
	{
		calfEverThreatened = false;
	}

	public void OnHerdMateHurt(BaseEntity attacker, LivestockAnimal victim)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (CanBeAnsweredFor(attacker, out var combatAttacker))
		{
			lastHerdDistress = TimeSince.op_Implicit(0f);
			herdDistressAttacker.Set(combatAttacker);
			RememberAggressor(attacker);
			ChargeFamiliarityPenalty(attacker as BasePlayer, Livestock.familiarityDistressPenalty);
			if (IsMyDependent(victim))
			{
				OnCalfThreatened();
			}
			SenseComponent senseComponent = default;
			if (((Component)this).TryGetComponent<SenseComponent>(ref senseComponent) && senseComponent.CanTarget(attacker))
			{
				senseComponent.TrySetTarget(attacker);
			}
		}
	}

	public void ClearHerdDistress()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		lastHerdDistress = TimeSince.op_Implicit(100f);
	}

	private void AlertHerdOfDistress(HitInfo info)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (DistressRadius <= 0f || (IsMale && IsAdult()))
		{
			return;
		}
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			Query.Server.GetBrainsInSphere(((Component)this).transform.position, DistressRadius, (List<LivestockAnimal>)(object)val);
			foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
			{
				if (IsHerdMate(item))
				{
					item.OnHerdMateHurt(info?.Initiator, this);
					if (item.IsDependent && !item.IsMyDependent(this))
					{
						item.Spook();
					}
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void TickNeeds()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		TimeSince val = lastNeedsTick;
		lastNeedsTick = TimeSince.op_Implicit(0f);
		TickCondition(TimeSince.op_Implicit(val));
		float num = ((TOD_Sky.Instance.IsNight || IsSleeping() || IsResting()) ? SleepingDecayScale : 1f);
		for (int i = 0; i < needs.Length; i++)
		{
			ref TickableNeed reference = ref needs[i];
			reference.NextTick -= TimeSince.op_Implicit(val);
			if (!(reference.NextTick > 0f))
			{
				float num2 = NeedTickFrequency - reference.NextTick;
				reference.NextTick = NeedTickFrequency;
				if (i == 1)
				{
					TickPersonalSpace(ref reference, num2);
				}
				else
				{
					reference.NeedValue = Mathf.MoveTowards(reference.NeedValue, 0f, DecayRatePerSecond((Need)i) * num2 * num);
				}
			}
		}
		if (IsGrazing() && needs.Length != 0)
		{
			ref TickableNeed reference2 = ref needs[0];
			float num3 = (IsGrazingFromTrough ? Mathf.Clamp01(TroughFullnessCeiling) : 1f);
			if (reference2.NeedValue < num3)
			{
				float num4 = Mathf.Min(FullnessFillRate * TimeSince.op_Implicit(val), num3 - reference2.NeedValue);
				if (IsGrazingFromTrough)
				{
					num4 = TakeTroughFullness(num4);
				}
				reference2.NeedValue += num4;
			}
		}
		if (IsDrinking() && needs.Length > 2)
		{
			ref TickableNeed reference3 = ref needs[2];
			reference3.NeedValue = Mathf.MoveTowards(reference3.NeedValue, 1f, HydrationFillRate * TimeSince.op_Implicit(val));
		}
	}

	public void EnterNaturalDeathPose()
	{
		if (!IsLyingDown())
		{
			SetNetworkedFlag(Flags.Reserved11, value: true);
		}
	}

	private string DebugLine(DebugValue value, float number, float precision, string text)
	{
		return FSMDebugInfo.Mark(text, flashedValues[(int)value].Sample(number, precision));
	}

	public string GetDebugInfo()
	{
		if (flashedValues == null)
		{
			flashedValues = new FlashedValue[6];
		}
		float needValue = Fullness.NeedValue;
		float needValue2 = Hydration.NeedValue;
		float needValue3 = PersonalSpace.NeedValue;
		float contentment = Contentment;
		string text = DebugLine(DebugValue.Age, AgeFraction, 0.01f, $"age {AgeFraction:0.00}") + "\n" + DebugLine(DebugValue.Fullness, needValue, 0.01f, $"full {needValue:0.00}") + "\n" + DebugLine(DebugValue.Hydration, needValue2, 0.01f, $"hyd {needValue2:0.00}") + "\n" + DebugLine(DebugValue.PersonalSpace, needValue3, 0.01f, $"room {needValue3:0.00}") + "\n" + DebugLine(DebugValue.Mood, contentment, 0.01f, $"mood {contentment:0.00}") + "\n" + DebugLine(DebugValue.Familiarity, BestFamiliarity(out var _), 1f, GetFamiliarityDebugInfo()) + "\n" + GetHomeDebugInfo();
		if (Livestock.debugCondition)
		{
			text = text + "\n" + GetConditionDebugInfo();
		}
		if (Livestock.debugDayBudget)
		{
			text = text + "\n" + GetDayBudgetDebugInfo();
		}
		if (Livestock.debugGenes)
		{
			text = text + "\n" + GetGenesDebugInfo();
		}
		RustNavMeshAgent rustNavMeshAgent = default;
		if (Livestock.debugGait && ((Component)this).TryGetComponent<RustNavMeshAgent>(ref rustNavMeshAgent))
		{
			text += $"\nwalk {rustNavMeshAgent.walkSpeed:0.0} gait {rustNavMeshAgent.gaitPenalty:+0;-0;0}";
		}
		return text;
	}

	private void SetLeading(bool leading)
	{
		SetNetworkedFlag(Flags.Reserved7, leading);
	}

	public void SetGrazing(bool grazing)
	{
		SetHeadDownFlag(Flags.Reserved8, grazing);
	}

	public void SetGrazingFromTrough(HitchTrough trough)
	{
		grazingTrough = trough;
	}

	public void SetDrinking(bool drinking)
	{
		SetHeadDownFlag(Flags.Reserved13, drinking);
	}

	private void SetHeadDownFlag(Flags flag, bool down)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (HasFlag(flag) != down)
		{
			if (!down)
			{
				headComingUpFor = TimeSince.op_Implicit(0f);
			}
			SetNetworkedFlag(flag, down);
		}
	}

	public void SetMating(bool mating)
	{
		SetNetworkedFlag(Flags.Reserved12, mating);
	}

	public void SetResting(bool resting)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (IsResting() != resting)
		{
			if (resting)
			{
				poseHeldFor = TimeSince.op_Implicit(0f);
			}
			else
			{
				StartGettingUp(LieDownDuration, StandUpDuration);
			}
			SetNetworkedFlag(Flags.Reserved14, resting);
		}
	}

	public void SetLyingIn(bool lyingIn)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (IsLyingIn() != lyingIn)
		{
			if (lyingIn)
			{
				poseHeldFor = TimeSince.op_Implicit(0f);
			}
			else
			{
				StartGettingUp(LieDownDuration, StandUpDuration);
			}
			SetNetworkedFlag(Flags.Reserved17, lyingIn);
		}
	}

	public void SetFrisky(bool frisky)
	{
		if (IsFrisky() != frisky)
		{
			SetNetworkedFlag(Flags.Reserved15, frisky);
		}
	}

	public void SetSleeping(bool sleeping)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (IsSleeping() != sleeping)
		{
			if (sleeping)
			{
				poseHeldFor = TimeSince.op_Implicit(0f);
			}
			else
			{
				StartGettingUp(BedDownDuration, WakeUpDuration);
			}
			SetNetworkedFlag(Flags.Reserved11, sleeping);
		}
	}

	public void SetNeedValue(Need need, float value)
	{
		if ((int)need < needs.Length)
		{
			needs[(int)need].NeedValue = Mathf.Clamp01(value);
		}
	}

	private void SetNetworkedFlag(Flags flag, bool value)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(flag, value);
	}

	public void StartBreedingCooldown()
	{
		breedCooldown.Start(Livestock.breedCooldown / GeneScale(LivestockGene.Fertility));
	}

	public float CalvingCooldown()
	{
		float num = (((Object)(object)Species != (Object)null) ? Species.ChildTimeToGrow : 0f);
		if (num <= 0f)
		{
			return Livestock.breedCooldown;
		}
		return num * Mathf.Max(0f, Livestock.ageScale) * (1f + Mathf.Clamp01(GrowTimeVariance)) * Mathf.Max(0f, Livestock.calvingCooldownScale) / GeneScale(LivestockGene.Fertility);
	}

	public void StartCalvingCooldown()
	{
		breedCooldown.Start(CalvingCooldown());
	}

	public bool CanBePregnant()
	{
		if (IsAdult() && IsFemale && !IsDead() && !IsPregnant() && !OnBreedingCooldown)
		{
			return MayBreed;
		}
		return false;
	}

	public bool CanBreed()
	{
		if (IsAdult() && IsMale && !IsDead() && !IsLeading() && !OnBreedingCooldown)
		{
			return MayBreed;
		}
		return false;
	}

	private static EntityRef<T> RefTo<T>(T entity) where T : BaseEntity
	{
		EntityRef<T> result = default;
		result.Set(entity);
		return result;
	}

	public bool MakePregnant(LivestockAnimal partner)
	{
		if (!CanBePregnant())
		{
			return false;
		}
		PregnantPartner = RefTo(partner);
		PregnantPartnerGenes = (((Object)(object)partner != (Object)null) ? partner.Genes : 0);
		pregnancyTimer.Start(PregnantDuration);
		if ((Object)(object)partner != (Object)null)
		{
			partner.StartBreedingCooldown();
		}
		return true;
	}

	public bool ForceGiveBirth()
	{
		if (IsDead() || !IsPregnant())
		{
			return false;
		}
		pregnancyTimer.Stop();
		return GiveBirth();
	}

	public void FinishPregnancy()
	{
		hasCalved = false;
		SetLyingIn(lyingIn: false);
		SetNetworkedFlag(Flags.Reserved10, value: false);
	}

	private bool GiveBirth()
	{
		hasCalved = true;
		LivestockAnimal partner = PregnantPartner.Get(serverside: true);
		int pregnantPartnerGenes = PregnantPartnerGenes;
		PregnantPartner = default;
		PregnantPartnerGenes = 0;
		StartCalvingCooldown();
		int num = 0;
		for (int i = 0; i < LitterSize(); i++)
		{
			if (BirthOne(partner, pregnantPartnerGenes))
			{
				num++;
			}
		}
		return num > 0;
	}

	private bool BirthOne(LivestockAnimal partner, int partnerGenes)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		bool flag = Random.Range(0f, 1f) > 0.5f;
		GameObjectRef gameObjectRef = (((Object)(object)Species != (Object)null) ? Species.ChildFor(flag) : null);
		if (gameObjectRef == null || !gameObjectRef.isValid)
		{
			Debug.LogWarning((object)(Categorize() + " (" + PrefabName + ") gave birth but its Species asset names no child prefab."));
			return false;
		}
		BaseEntity baseEntity = gameManager.CreateEntity(gameObjectRef.resourcePath, GetBirthPosition());
		if ((Object)(object)baseEntity == (Object)null)
		{
			return false;
		}
		if (baseEntity is LivestockAnimal livestockAnimal)
		{
			livestockAnimal.MarkNewborn();
			livestockAnimal.SetSleeping(sleeping: true);
		}
		baseEntity.Spawn();
		if (baseEntity is LivestockAnimal livestockAnimal2)
		{
			if (livestockAnimal2.Form == SexForm.Either)
			{
				livestockAnimal2.IsMale = flag;
				livestockAnimal2.AnimalName = RollAnimalName(flag, out var locale);
				livestockAnimal2.FoundLine(locale);
			}
			livestockAnimal2.Genes = InheritedGenome(partnerGenes);
			livestockAnimal2.TakeHomeFrom(this);
			PassBondsTo(livestockAnimal2);
			livestockAnimal2.RegisterParentage(this, partner);
			if (!livestockAnimal2.TryNameFromCross(this, partner))
			{
				livestockAnimal2.TryInheritName(this, partner);
			}
			newbornCalf.Set(livestockAnimal2);
			livestockAnimal2.SendNetworkUpdateImmediate();
		}
		LivestockCensus.RecordBirth(((Component)this).transform.position);
		BuildingPrivlidge cupboard;
		if (BestFamiliarity(out var userId) > 0f && userId != 0)
		{
			Facepunch.Rust.Analytics.Azure.OnLivestockBirth("best_familiarity", baseEntity, userId);
		}
		else if (TryGetHomeCupboard(out cupboard))
		{
			Facepunch.Rust.Analytics.Azure.OnLivestockBirth("home_cupboard", baseEntity, cupboard.authorizedPlayers);
		}
		else
		{
			Facepunch.Rust.Analytics.Azure.OnLivestockBirth("none", baseEntity);
		}
		return true;
	}

	private void RegisterParentage(LivestockAnimal mother, LivestockAnimal father)
	{
		Father.Set(father);
		Mother.Set(mother);
	}

	private Vector3 GetBirthPosition()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		RustNavMeshAgent rustNavMeshAgent = default;
		if (!((Component)this).TryGetComponent<RustNavMeshAgent>(ref rustNavMeshAgent))
		{
			return ((Component)this).transform.position;
		}
		PooledList<NavVector3> val = Pool.Get<PooledList<NavVector3>>();
		try
		{
			bool flag = Eqs.SampleNavigablePositions(rustNavMeshAgent, rustNavMeshAgent.nextPosition, (List<NavVector3>)(object)val, ChildSpawnDistance, ChildSpawnDistance, 8);
			foreach (NavVector3 item in (List<NavVector3>)(object)val)
			{
				if (flag)
				{
					return rustNavMeshAgent.NavToWorldSpace(item);
				}
				if (rustNavMeshAgent.SamplePosition(item, out var hitNS, ChildSpawnDistance) && rustNavMeshAgent.CanReach(hitNS.position))
				{
					return rustNavMeshAgent.NavToWorldSpace(hitNS.position);
				}
			}
			return ((Component)this).transform.position;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool NeedsFood()
	{
		return Fullness.NeedValue <= ConsumeThreshold * ExpectedFullnessCeiling;
	}

	public void RecordHelpingSource(bool fromTrough)
	{
		lastHelpingFromTrough = fromTrough;
	}

	public void FindFoodTroughs(List<BaseEntity> results)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (ConsumeSearchRadius <= 0f)
		{
			return;
		}
		PooledList<HitchTrough> val = Pool.Get<PooledList<HitchTrough>>();
		try
		{
			Query.Server.GetInSphere(((Component)this).transform.position, ConsumeSearchRadius, (List<HitchTrough>)(object)val);
			foreach (HitchTrough item in (List<HitchTrough>)(object)val)
			{
				if (CanEatFrom(item, out var _))
				{
					results.Add(item);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool CanEatFrom(HitchTrough trough, out Item foundItem)
	{
		foundItem = null;
		if ((Object)(object)trough == (Object)null || trough.IsDestroyed || trough.IsDead())
		{
			return false;
		}
		ItemModConsumable itemModConsumable = default;
		foreach (Item item in trough.inventory.itemList)
		{
			if (((Component)item.info).TryGetComponent<ItemModConsumable>(ref itemModConsumable) && itemModConsumable.chickenCoopFood && TroughFullnessOf(itemModConsumable) > 0f)
			{
				foundItem = item;
				return true;
			}
		}
		return false;
	}

	private float TroughFullnessOf(ItemModConsumable food)
	{
		return food.GetIfType(MetabolismAttribute.Type.Calories) * TroughFullnessPerCalorie;
	}

	public bool ConsumeFromTrough(HitchTrough trough)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (!CanEatFrom(trough, out var foundItem))
		{
			return false;
		}
		troughFullnessStored += TroughFullnessOf(((Component)foundItem.info).GetComponent<ItemModConsumable>());
		foundItem.UseItem();
		LivestockCensus.RecordTroughItemEaten(((Component)this).transform.position);
		return true;
	}

	public float TakeTroughFullness(float wanted)
	{
		while (troughFullnessStored < wanted && ConsumeFromTrough(grazingTrough))
		{
		}
		float num = Mathf.Min(wanted, troughFullnessStored);
		troughFullnessStored -= num;
		return num;
	}

	private void OnSyncVar_IsMale(bool? oldValue, bool newValue)
	{
	}

	private void OnSyncVar_AnimalName(string? oldValue, string newValue)
	{
		if (isServer)
		{
			ForgetSpecial(oldValue);
			RememberSpecial(newValue, discovered: false);
		}
	}

	private void OnSyncVar_LeadingPlayer(EntityRef<BasePlayer>? oldValue, EntityRef<BasePlayer> newValue)
	{
	}

	private void OnSyncVar_Genes(int? oldValue, int newValue)
	{
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 0:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: IsMale for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_IsMale);
			return true;
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: PregnantPartner for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_PregnantPartner);
			return true;
		case 2:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: AnimalName for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_AnimalName);
			return true;
		case 3:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: LeadingPlayer for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_LeadingPlayer);
			return true;
		case 4:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: HomeTc for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_HomeTc);
			return true;
		case 5:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: Genes for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_Genes);
			return true;
		case 6:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: PregnantPartnerGenes for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_PregnantPartnerGenes);
			return true;
		default:
			return base.WriteSyncVar(id, writer);
		}
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		switch (id)
		{
		case 0:
			try
			{
				bool? oldValue3 = __sync_IsMale;
				bool newValue3 = (__sync_IsMale = reader.Bool());
				if (fromAutoSave)
				{
					oldValue3 = null;
				}
				OnSyncVar_IsMale(oldValue3, newValue3);
			}
			catch (Exception ex6)
			{
				Debug.LogException(ex6);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_PregnantPartner;
				EntityRef<LivestockAnimal> _sync_PregnantPartner = NetworkReadEx.EntityRef<LivestockAnimal>(reader);
				__sync_PregnantPartner = _sync_PregnantPartner;
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			return true;
		case 2:
			try
			{
				string oldValue2 = __sync_AnimalName;
				string newValue2 = (__sync_AnimalName = reader.String());
				if (fromAutoSave)
				{
					oldValue2 = null;
				}
				OnSyncVar_AnimalName(oldValue2, newValue2);
			}
			catch (Exception ex4)
			{
				Debug.LogException(ex4);
			}
			return true;
		case 3:
			try
			{
				EntityRef<BasePlayer>? oldValue4 = __sync_LeadingPlayer;
				EntityRef<BasePlayer> newValue4 = (__sync_LeadingPlayer = NetworkReadEx.EntityRef<BasePlayer>(reader));
				if (fromAutoSave)
				{
					oldValue4 = null;
				}
				OnSyncVar_LeadingPlayer(oldValue4, newValue4);
			}
			catch (Exception ex7)
			{
				Debug.LogException(ex7);
			}
			return true;
		case 4:
			try
			{
				_ = __sync_HomeTc;
				EntityRef<BuildingPrivlidge> _sync_HomeTc = NetworkReadEx.EntityRef<BuildingPrivlidge>(reader);
				__sync_HomeTc = _sync_HomeTc;
			}
			catch (Exception ex5)
			{
				Debug.LogException(ex5);
			}
			return true;
		case 5:
			try
			{
				int? oldValue = __sync_Genes;
				int newValue = (__sync_Genes = reader.Int32());
				if (fromAutoSave)
				{
					oldValue = null;
				}
				OnSyncVar_Genes(oldValue, newValue);
			}
			catch (Exception ex3)
			{
				Debug.LogException(ex3);
			}
			return true;
		case 6:
			try
			{
				_ = __sync_PregnantPartnerGenes;
				int _sync_PregnantPartnerGenes = reader.Int32();
				__sync_PregnantPartnerGenes = _sync_PregnantPartnerGenes;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		default:
			return base.OnSyncVar(id, reader, fromAutoSave);
		}
	}

	private byte __GetWeaverID(string propertyName)
	{
		return propertyName switch
		{
			"IsMale" => (byte)0, 
			"PregnantPartner" => (byte)1, 
			"AnimalName" => (byte)2, 
			"LeadingPlayer" => (byte)3, 
			"HomeTc" => (byte)4, 
			"Genes" => (byte)5, 
			"PregnantPartnerGenes" => (byte)6, 
			_ => byte.MaxValue, 
		};
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
		WriteSyncVar(1, writer);
		WriteSyncVar(2, writer);
		WriteSyncVar(3, writer);
		WriteSyncVar(4, writer);
		WriteSyncVar(5, writer);
		WriteSyncVar(6, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
		OnSyncVar(1, reader, fromAutoSave: true);
		OnSyncVar(2, reader, fromAutoSave: true);
		OnSyncVar(3, reader, fromAutoSave: true);
		OnSyncVar(4, reader, fromAutoSave: true);
		OnSyncVar(5, reader, fromAutoSave: true);
		OnSyncVar(6, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		WriteAutoSaveSyncVars(netWrite);
		(byte[] Buffer, int Length) buffer = netWrite.GetBuffer();
		byte[] item = buffer.Buffer;
		int item2 = buffer.Length;
		byte[] array = _autosaveBuffer;
		if (array == null || array.Length < item2)
		{
			byte[] array2 = BaseEntity._autosaveBufferPool.Rent(item2);
			while (array == null || array.Length < item2)
			{
				byte[] array3 = Interlocked.CompareExchange(ref _autosaveBuffer, array2, array);
				if (array3 == array)
				{
					if (array3 != null)
					{
						BaseEntity._autosaveBufferPool.Return(array3);
					}
					array = array2;
					break;
				}
				array = array3;
			}
			if (array != array2)
			{
				BaseEntity._autosaveBufferPool.Return(array2);
			}
		}
		Buffer.BlockCopy(item, 0, array, 0, item2);
		save.msg.baseEntity.syncVars = array;
		Pool.Free<NetWrite>(ref netWrite);
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		if (load.msg.baseEntity != null && load.msg.baseEntity.syncVars != null)
		{
			NetRead netRead = Pool.Get<NetRead>();
			netRead.Init(load.msg.baseEntity.syncVars.AsSpan());
			ReadAutoSaveSyncVars(netRead);
			Pool.Free<NetRead>(ref netRead);
		}
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_IsMale = false;
		__sync_PregnantPartner = default;
		__sync_AnimalName = null;
		__sync_LeadingPlayer = default;
		__sync_HomeTc = default;
		__sync_Genes = 0;
		__sync_PregnantPartnerGenes = 0;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			0 => true, 
			1 => true, 
			2 => true, 
			3 => true, 
			4 => true, 
			5 => true, 
			6 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}
}

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
		Hostile,
		Neutral,
		Trusted,
		Bonded
	}

	private struct Acquaintance
	{
		public ulong userId;

		public float seconds;
	}

	public enum Personality
	{
		Curious,
		Sleepy
	}

	public enum SlotState
	{
		Free,
		Held,
		BlockedByIdler,
		Blocked,
		HitchedOver,
		CannotBeCleared
	}

	private struct SlotQueue
	{
		public EntityRef<BaseEntity> Owner;

		public TimeSince Waited;

		public TimeSince Renewed;
	}

	public enum OwnerChangeReason
	{
		Purchased,
		Trough,
		Birth,
		Debug
	}

	private struct CullCandidate(LivestockAnimal animal) : IComparable<CullCandidate>
	{
		public readonly LivestockAnimal Animal = animal;

		private readonly int group = (animal.HasOwner ? 1 : 0);

		private readonly float worth = animal.SaleValueScale;

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

	public struct FormationSlot
	{
		public bool InFormation;

		public Vector3 Back;

		public Vector3 Right;

		public float Across;

		public int Row;

		public int GroupSize;

		public float Longest;
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
		Hydration,
		Grass,
		Sun
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
		public int CountLedBy(BasePlayer player)
		{
			int num = 0;
			LivestockAnimal[] buffer = base.workList.Values.Buffer;
			for (int i = 0; i < base.workList.Count; i++)
			{
				LivestockAnimal livestockAnimal = buffer[i];
				if ((Object)(object)livestockAnimal != (Object)null && !livestockAnimal.IsDestroyed && !livestockAnimal.IsDead() && livestockAnimal.IsFollowing(player))
				{
					num++;
				}
			}
			return num;
		}

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

	private float ageLived;

	private LivestockPattern pattern;

	private LivestockSexParts sexParts;

	private bool breedingPaused;

	public const float StarvationFloorHealth = 1f;

	private bool starvedDown;

	private float conditionSecondsSpent;

	private float conditionSecondsSkipped;

	public const float ConditionHealthStep = 0.05f;

	private float conditionUnsentHealth;

	[Range(0f, 1f)]
	[Tooltip("Fullness at or below this is starving rather than peckish. A starving animal gets up from a lie down and breaks off a curious walk to go and eat.")]
	[Header("Day activity")]
	public float StarvingThreshold = 0.2f;

	[Tooltip("How many seconds of waking time the day activity read out is measured over. It only reports how the day split up, nothing reads it to decide what to do.")]
	public float DayBudgetWindow = 300f;

	private float[] activityWeights;

	[Header("Dung")]
	[Tooltip("Leave empty for an age stage that shouldn't dung, like a calf.")]
	public ItemDefinition DungItem;

	[Tooltip("Seconds between drops. Zero disables it.")]
	public float DungInterval;

	[Range(0f, 1f)]
	[Tooltip("Fraction of DungInterval each wait is randomised by, so a herd doesn't drop in lockstep. 0.2 is +/-20%.")]
	public float DungIntervalVariance = 0.2f;

	[Tooltip("Falls back to the animal's origin when unset.")]
	public Transform DungSpawnPoint;

	private PersistentTimer dungTimer;

	private float dungBoostUntil;

	private float dungBoostRate = 1f;

	public const int MaxAcquaintances = 8;

	public const int TrustTiersAboveHostile = 3;

	private Acquaintance[] acquaintances;

	private Acquaintance probation;

	private TimeSince probationCredited;

	private const float ProbationTimeout = 60f;

	private EntityRef<BasePlayer> trustedNearby;

	private float trustedNearbyDistanceSqr;

	private TimeSince trustedNearbySeen;

	private EntityRef<BasePlayer> curiousAbout;

	private const float CuriousMinFullness = 0.7f;

	private float lastDefendedAttack = float.NegativeInfinity;

	[Header("Feeding slots")]
	[Tooltip("How far the nose reaches while eating from a trough, as a share of how far it reaches standing, so a slot stands the animal with its nose where the slot says. The trough eating clips measure 0.92 for a cow and 0.86 for a sheep, and 0.9 puts both within 4cm.")]
	public float TroughReachScale = 0.9f;

	public const float SlotSearchRadius = 30f;

	private const float QueueLifetime = 20f;

	private const float VacateLifetime = 30f;

	private const float FailedSlotCooldown = 15f;

	private const float NoWayOutCooldown = 30f;

	private EntityRef<BaseEntity> slotOwner;

	private EntityRef<BaseEntity> slotSource;

	private int slotIndex = -1;

	private Vector3 slotStandingPoint;

	private Vector3 slotFacing;

	private LivestockFootprint slotFootprint;

	private LivestockFeedingSlots.Serves slotNeed;

	private bool slotInUse;

	private bool askedToVacate;

	private TimeSince slotLeft;

	private SlotQueue foodQueue;

	private SlotQueue waterQueue;

	private EntityRef<BaseEntity> failedSlotOwner;

	private int failedSlotIndex = -1;

	private TimeSince failedSlotAt;

	private EntityRef<BaseEntity> noWayOutOwner;

	private TimeSince noWayOutAt;

	private bool slotSearchFailing;

	private TimeSince slotSearchFailedAt;

	private RustNavMeshAgent frailtyAgent;

	private LivestockAnimalFSM frailtyFsm;

	private Vector3 homePosition;

	private const float AdoptionCheckInterval = 5f;

	private TimeSince lastAdoptionCheck;

	[Tooltip("How much hydration refills per second while the animal has its head in the water.")]
	[Header("Hydration")]
	public float HydrationFillRate = 0.25f;

	[Tooltip("Where a freshly spawned animal's hydration starts. Full, so a new animal has one thing less to go and sort out before it settles in.")]
	[Range(0f, 1f)]
	public float StartingHydration = 1f;

	[Tooltip("How much hydration drains per second while the animal is up and about in daylight. Tiny, because one mouthful refills the animal completely: this sets how often it goes for a drink rather than how long it stands there. At 0.00001 a full animal is down to the ConsumeThreshold of 0.95 after about 5000 seconds of daylight, which is one trip to the water in a shipped waking day.")]
	public float HydrationDecayRate = 1E-05f;

	[Tooltip("Shallowest water (in metres) that counts as somewhere to drink from. Above zero so a dry riverside tile with the water still 10m away doesn't qualify.")]
	public float MinStandingWaterDepth = 0.05f;

	[Tooltip("Deepest water (in metres) the animal will stand in to drink, so it drinks from the edge rather than wading out into the middle of the river.")]
	public float MaxStandingWaterDepth = 0.5f;

	[Tooltip("How much water a drink from completely dry takes out of a barrel. A drink takes its share of this for the hydration it puts back, so topping up often costs no more water than waiting until empty. 200 makes the daily top up from 0.95 cost 10.")]
	public int BarrelWaterPerDrink = 200;

	private Phrase wildCategoryLabel;

	public const int MaxGeneration = 20;

	private const string EnglishSecond = "{0} Junior";

	private const string EnglishOrdinal = "{0} the {1}";

	private const string StemMark = "{0}";

	private static readonly HashSet<string> reportedLongNames = new HashSet<string>();

	private ulong offlineOwnerNameId;

	private string offlineOwnerName;

	private static readonly Phrase NotTrustedPhrase = new Phrase("livestock.handle.untrusted", "{0} doesn't trust you yet");

	private static readonly Phrase NotTrustedWildPhrase = new Phrase("livestock.handle.untrusted.wild", "This animal doesn't trust you yet");

	private static readonly Dictionary<LivestockSpecies, int> populationBySpecies = new Dictionary<LivestockSpecies, int>();

	private float potentialFromServer;

	private int potentialTrendFromServer;

	private const float PotentialTrendDeadband = 0.01f;

	private const float PotentialFullHappiness = 0.8f;

	private float potential;

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

	[Tooltip("The ground the body covers from lying down to standing back up, in the animal's own space. Zero size uses the standing bounds.")]
	public Bounds LyingBounds;

	private const float LyingSpotInnerRadius = 2.5f;

	private const float LyingSpotOuterRadius = 6f;

	private float sleepFraction;

	[Tooltip("How far (in metres) the animal looks for herd mates: the ones it answers for, the floor it can share with them and, on open ground, how big a free range herd it counts itself part of.")]
	[Header("PersonalSpace")]
	public float SocialSearchRadius = 20f;

	[Tooltip("Whether the animal has the personal space need at all. Without it the need stays full, while the room to breed rule still applies.")]
	public bool HasPersonalSpace = true;

	[Tooltip("How far a hurt animal's distress carries to the rest of its herd. Deliberately wider than SocialSearchRadius: that one is about who counts as company, this is about how far away a bull should still answer for one of his being shot. Only queried on a hit, so it can afford to be generous.")]
	public float DistressRadius = 40f;

	[Tooltip("How far a strayed animal looks for its herd when walking back to it. Must be wider than the FSM's strayDistance, or the search can never find anyone far enough away to count as strayed.")]
	public float RegroupSearchRadius = 40f;

	[Tooltip("How close (in metres) a player this animal already holds a grudge against has to stand to a calf of ours, and stay, before its mother treats it as a threat to the calf. Zero leaves only hurting it winding her up.")]
	public float CalfThreatDistance = 6f;

	[Tooltip("How close (in metres) a calf has to be to a mother who is standing her ground before it shelters behind her instead of bolting. Keep it under the FSM's strayDistance, or a calf can shelter from further away than it is willing to stand. Zero has calves always run.")]
	public float MotherShelterDistance = 12f;

	private static int lastFreeRangeHerdSearch;

	private int freeRangeHerdSearch;

	private float freeRangeHerdCountedAt = float.NegativeInfinity;

	private bool playerLingeringOverCalf;

	private EntityRef<LivestockAnimal> lastShielded;

	private const int ShieldSlots = 3;

	private const float ShieldSlotLifetime = 5f;

	private EntityRef<LivestockAnimal>[] shieldSlotHolders;

	private TimeSince[] shieldSlotClaimed;

	private const float RestingSearchRadius = 30f;

	private const float RestingGap = 0.25f;

	private const int RestingSpotSamples = 16;

	private const float LonelySpotInnerRadius = 4f;

	private const float LonelySpotOuterRadius = 12f;

	private const float LonelierBy = 2f;

	private const float FormationSearchRadius = 20f;

	private const float InLineTolerance = 0.1f;

	private const int FilesAbreast = 3;

	private const float HerdFleeHeadingLifetime = 10f;

	private Vector3 herdFleeHeading;

	private TimeSince herdFleeHeadingSet;

	private bool hasHerdFleeHeading;

	private LivestockFloor floor;

	private int freeRangeHerdMates;

	private int freeRangeHerdUnborn;

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

	public const Flags LyingIn = Flags.Reserved17;

	public const Flags AtTrough = Flags.Reserved18;

	public const Flags RecentlyPetted = Flags.Reserved19;

	public const int AgeTimerId = 1;

	public const int PregnancyTimerId = 2;

	public const int BreedCooldownTimerId = 3;

	public const int DungTimerId = 4;

	public AgeStage Age = AgeStage.Adult;

	public LivestockSize MountType = LivestockSize.Small;

	[Tooltip("How long (in seconds) an infant takes to grow into the adult named by its Species asset. Infants only: an adult never ages out of its stage, so its own is zero.")]
	[Header("Aging")]
	public float TimeToGrow;

	[Range(0f, 1f)]
	[Tooltip("Random variance applied to TimeToGrow when the animal first spawns, as a fraction of it. 0.1 means +/-10%, so a herd doesn't all grow up in lockstep.")]
	public float GrowTimeVariance = 0.1f;

	public GameObjectRef GrowEffect;

	[Tooltip("How long (in seconds) this animal takes to give birth once pregnant")]
	public float PregnantDuration;

	[Tooltip("How long (in seconds) before the birth she stops what she is doing and lies down. The rest of the pregnancy she spends grazing like any other animal, so raising Pregnant Duration does not turn into a lie down of the same length.")]
	public float CalvingLieDownSeconds = 45f;

	[Tooltip("How far from the mother the newborn is placed, so it doesn't spawn inside her.")]
	public float ChildSpawnDistance = 1f;

	[Header("Sex")]
	[Tooltip("Which sex this prefab represents. Infants and species that use one prefab for both sexes stay at Either and roll for it; a prefab that is specifically the male or female form of its species says so here, and must be the one its Species asset names.")]
	public SexForm Form;

	[Tooltip("The species this animal belongs to. Shared by every prefab of the species - both sexes and every age stage - so it identifies mates and tells an infant which adult to grow into.")]
	public LivestockSpecies Species;

	[Header("Needs")]
	[Tooltip("How much fullness a second of grazing puts back. Deliberately slow: grazing is meant to be what the animal spends its day doing, not a top up it finishes in a few seconds.")]
	public float FullnessFillRate = 0.004f;

	[Tooltip("How much fullness drains per second while the animal is up and about in daylight. Against FullnessFillRate this sets how much of its standing time a well fed animal spends eating: 0.001 against 0.004 is a quarter of it. Slow, because the dip below ConsumeThreshold while the animal walks over is what a keeper sees. Lying down and sleeping drain at SleepingDecayScale.")]
	public float FullnessDecayRate = 0.001f;

	[Range(0f, 1f)]
	[Tooltip("Where a freshly spawned animal's fullness starts. Full, so a new animal in a well kept pen reads right straight away instead of spending minutes climbing to the top of its bar.")]
	public float StartingFullness = 1f;

	[Tooltip("Scales the drain while the animal is asleep or the sun is down. Feeding is daytime work, so a full night at the waking rate would be a debt the day could not pay off.")]
	[Range(0f, 1f)]
	public float SleepingDecayScale = 0.25f;

	public float NeedTickFrequency = 5f;

	[Tooltip("How far (in metres) the animal looks for somewhere to feed itself - a trough, a water barrel, open grass or a river bank.")]
	public float ConsumeSearchRadius = 25f;

	[Tooltip("A need at or below this sends the animal off to top it up, and grass, a trough or water all fill it back to full. High on purpose: a keeper reads the bar as whether the pen is set up right, so a well kept animal has to sit near the top of it, about 0.93 to 1 whatever feeds it. The gap below the top sets how often it goes to eat, not how much of its day it spends eating.")]
	[Range(0f, 1f)]
	public float ConsumeThreshold = 0.95f;

	[Tooltip("How much fullness each calorie of trough food is worth. The animal grazes a trough item down before it takes the next, so richer food lasts longer, the way it does for a horse.")]
	public float TroughFullnessPerCalorie = 0.0021f;

	[Tooltip("The share of its food an animal whose grass need reads full takes off the grass. Its meals alternate between the grass and a stocked trough in reach to match, and the grass alone fills its bar no higher than this, so at 0.5 a herd on full grass with no trough sits half full.")]
	[Range(0f, 1f)]
	public float GrassFoodShare = 0.5f;

	[Tooltip("How long (in seconds) the animal takes to raise its head once it stops grazing or drinking. Clearing the flag only STARTS the head coming up, so for this long it is still visibly head down. The cow's cow_eat_out is 1.7s and the sheep's sheep_eat_exit 1.5s.")]
	public float RaiseHeadDuration = 1.7f;

	public GestureConfig PettingGesture;

	public const float StartingPersonalSpaceNeed = 1f;

	private TimeSince poseHeldFor;

	private TimeUntil onItsFeetIn;

	private TimeSince headComingUpFor;

	private TickableNeed[] needs = Array.Empty<TickableNeed>();

	public const float NoTroughFoodMinutes = -1f;

	private TimeSince lastLoudNoise;

	private TimeSince lastNeedsTick;

	protected PersistentTimerSet timers;

	private PersistentTimer pregnancyTimer;

	private PersistentTimer breedCooldown;

	private EntityRef<LivestockAnimal> Father;

	private EntityRef<LivestockAnimal> Mother;

	private string pregnantPartnerName;

	private FarmableAnimal.NameSource pregnantPartnerNameSource;

	private ulong pregnantPartnerNameAuthorId;

	private bool findingItsFeet;

	private EntityRef<LivestockAnimal> newbornCalf;

	public static LivestockNeedsQueue NeedsQueue = new LivestockNeedsQueue();

	private const int SaveVersion = 2;

	public const int MostLedByOnePlayer = 9;

	private static readonly Phrase LeadingTooManyPhrase = new Phrase("livestock.lead.toomany", "Leading too many animals");

	private static readonly Phrase LeadOwnedPhrase = new Phrase("livestock.lead.owned", "{0} is {1}'s");

	private float leadSlack;

	private bool leadTrusted;

	private TimeSince untrustedLeadHeld;

	public const float UntrustedLeadHold = 1.5f;

	private bool startled;

	private TimeSince lastProvoked;

	private EntityRef<BasePlayer> provokedBy;

	public const float ProvokedFor = 10f;

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

	private Action clearPettedFlagAction;

	public const int MaxAnimalNameLength = 24;

	private Vector3 lastLoudNoisePosition;

	private EntityRef<BaseCombatEntity> aggressor;

	private TimeSince lastAggression;

	private float penaltyCharged;

	private TimeSince lastHerdDistress;

	private EntityRef<BaseCombatEntity> herdDistressAttacker;

	private TimeSince lastCalfThreat;

	private bool calfEverThreatened;

	private TimeSince lastBondThreat;

	private bool bondEverThreatened;

	private const float NeedFlashPrecision = 0.01f;

	private const float AgeFlashPrecision = 0.1f;

	private const float FamiliarityFlashPrecision = 1f;

	private FlashedValue[] flashedValues;

	private HitchTrough grazingTrough;

	private float troughFullnessStored;

	private ItemModConsumable troughFullnessFood;

	private bool hasCalved;

	private const int BirthCandidates = 8;

	public const float SupplySearchRadius = 27f;

	private float? grassShareOfFullness;

	private bool __sync_IsMale;

	private int __sync_NameAuthorSourceSync;

	private string __sync_AnimalName;

	private EntityRef<BasePlayer> __sync_LeadingPlayer;

	private int __sync_Genes;

	public bool AgingEnabled => Livestock.ageScale > 0f;

	public bool IsAging
	{
		get
		{
			if (AgingEnabled)
			{
				return HasOwner;
			}
			return false;
		}
	}

	public bool CanGrowUp
	{
		get
		{
			if (IsInfant() && TimeToGrow > 0f)
			{
				return HasAdultForm;
			}
			return false;
		}
	}

	public float AgeSeconds => ageLived;

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

	public int CoatSeed { get; set; }

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
			return (((Object)(object)PatternSet.Range != (Object)null) ? PatternSet.Range.ShadeNameOf(Genes) : null) ?? $"pattern {CoatSeed:X8}";
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

	public bool IsAtConditionFloor => Health() <= 1.01f;

	public bool IsStarved
	{
		get
		{
			if (IsAtConditionFloor)
			{
				if (!(Fullness.NeedValue <= 0f))
				{
					return Hydration.NeedValue <= 0f;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsAsleep
	{
		get
		{
			if (IsSleeping())
			{
				return !starvedDown;
			}
			return false;
		}
	}

	private float ConditionFloorHealth
	{
		get
		{
			if (!(PersonalSpace.NeedValue <= 0f))
			{
				return 1f;
			}
			return 0f;
		}
	}

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
			if (HasOwner && !IsStarving())
			{
				return Condition >= Livestock.dungConditionFloor;
			}
			return false;
		}
	}

	private float DungBoostTimeLeft => Mathf.Max(0f, dungBoostUntil - Time.time);

	public bool WantsDungBoost
	{
		get
		{
			if (DungEnabled && CanDung)
			{
				return DungBoostTimeLeft <= 0f;
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

	public bool ActsCuriousAdult
	{
		get
		{
			if (ActsCurious)
			{
				return IsAdult();
			}
			return false;
		}
	}

	public bool IsSleepy { get; private set; }

	public bool ActsSleepy
	{
		get
		{
			if (IsSleepy)
			{
				return IsAdult();
			}
			return false;
		}
	}

	private static float NonOwnerTrustCap => Livestock.maxTrust * Mathf.Clamp01(Livestock.nonOwnerTrustShare);

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

	public LivestockFootprint HeldSlotFootprint => slotFootprint;

	public float SlotSearchFailedFor
	{
		get
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			if (!slotSearchFailing)
			{
				return 0f;
			}
			return TimeSince.op_Implicit(slotSearchFailedAt);
		}
	}

	public int AgeGaitPenalty
	{
		get
		{
			if (!IsInfant())
			{
				return 0;
			}
			return -1;
		}
	}

	public bool IsInbred => LivestockGenome.IsInbred(Genes);

	public float GeneQuality => LivestockGenome.ExpressedQuality(Genes);

	public bool IsGodClone => LivestockGenome.IsGodGenome(Genes);

	public bool HidesBadAllele => LivestockGenome.HidesBad(Genes);

	public float NeedScale => 1f / GeneScale(LivestockGene.Hardiness);

	public float ShelterNeedScale => Mathf.Min(1f, NeedScale * (Livestock.genesEnabled ? Mathf.Max(0.01f, Livestock.geneHardinessBad) : 1f));

	public Vector3 HomePosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return homePosition;
		}
	}

	public EntityRef<BuildingPrivlidge> HomeTc { get; set; }

	public FarmableAnimal.NameSource NameAuthorSource => (FarmableAnimal.NameSource)NameAuthorSourceSync;

	public bool HasSpecialAuthoredName => NameAuthorSource == FarmableAnimal.NameSource.Special;

	public bool HasPlayerAuthoredName => NameAuthorSource == FarmableAnimal.NameSource.Player;

	public int Generation { get; set; }

	public string NameLocale { get; set; }

	public string LineStem { get; set; }

	public bool HeirNamed { get; set; }

	public ulong NameAuthorId { get; private set; }

	public bool HasOwner => OwnerID != 0;

	public bool IsOwnersLead
	{
		get
		{
			if (!HasOwner || !IsLeading())
			{
				return false;
			}
			BasePlayer basePlayer = LeadingPlayer.Get(isServer);
			if ((Object)(object)basePlayer != (Object)null)
			{
				return (ulong)basePlayer.userID == OwnerID;
			}
			return false;
		}
	}

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

	public int PotentialTrend
	{
		get
		{
			if (isServer)
			{
				if (!IsEarningPotential)
				{
					return 0;
				}
				float num = PotentialTarget - Potential;
				if (!(Mathf.Abs(num) < 0.01f))
				{
					return (int)Mathf.Sign(num);
				}
				return 0;
			}
			return potentialTrendFromServer;
		}
	}

	public float Potential
	{
		get
		{
			if (isServer)
			{
				return Mathf.Min(potential, PotentialCeiling);
			}
			return potentialFromServer;
		}
	}

	private bool IsEarningPotential
	{
		get
		{
			if (IsInfant() && IsAging)
			{
				return !IsDead();
			}
			return false;
		}
	}

	private float PotentialCeiling
	{
		get
		{
			if (!Mother.IsSet)
			{
				return Mathf.Clamp01(Livestock.potentialUnbredCap);
			}
			return 1f;
		}
	}

	private float Happiness => Mathf.InverseLerp(0f, Mathf.Max(0.01f, Livestock.conditionFullContentment), Contentment);

	private float PotentialTarget => Mathf.Min(Mathf.InverseLerp(0f, 0.8f, Happiness), PotentialCeiling);

	private Bounds LyingBody
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if (!(LyingBounds.size == Vector3.zero))
			{
				return LyingBounds;
			}
			return bounds;
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

	internal bool CountsFreeRangeHerd
	{
		get
		{
			if (Livestock.maxFreeRangeHerd > 0 && HasOwner && !StandsInAPen)
			{
				return !IsDead();
			}
			return false;
		}
	}

	internal RustNavMeshAgent NavAgent => frailtyAgent;

	public float LitterHeads => (float)MaxLitterSize() * NeedScale;

	private int LitterOnTheWay
	{
		get
		{
			if (!IsPregnant())
			{
				return 0;
			}
			return MaxLitterSize();
		}
	}

	public float RoomTarget => PackingTarget(FloorPerHead);

	public float FloorPerHead
	{
		get
		{
			if (floor != null && !floor.IsField)
			{
				return floor.Area / Mathf.Max(floor.Heads, 0.01f);
			}
			return float.PositiveInfinity;
		}
	}

	public bool StandsInAField
	{
		get
		{
			if (floor != null)
			{
				return floor.IsField;
			}
			return false;
		}
	}

	public bool StandsInAPen
	{
		get
		{
			if (floor != null)
			{
				return !floor.IsField;
			}
			return false;
		}
	}

	public int FreeRangeHerd => freeRangeHerdMates + 1;

	public int ProjectedFreeRangeHerd => FreeRangeHerd + freeRangeHerdUnborn + LitterOnTheWay;

	internal LivestockFloor Floor => floor;

	internal LivestockFloor CurrentFloor
	{
		get
		{
			RustNavMeshAgent navAgent = NavAgent;
			if (floor == null || !((Object)(object)navAgent != (Object)null) || !floor.IsCurrent(navAgent))
			{
				return null;
			}
			return floor;
		}
	}

	internal bool CanStandOnAFloor
	{
		get
		{
			if (!IsDestroyed && !IsDead() && !IsMounted)
			{
				return !IsWild;
			}
			return false;
		}
	}

	internal bool IsWild
	{
		get
		{
			if (!HasOwner)
			{
				return IsPopulationSpawn();
			}
			return false;
		}
	}

	public bool HasRoomToBreed
	{
		get
		{
			RemeasureIfChanged();
			if (floor == null || floor.IsField)
			{
				if (Livestock.maxFreeRangeHerd <= 0)
				{
					return true;
				}
				return Livestock.maxFreeRangeHerd - ProjectedFreeRangeHerd >= ((!IsFemale) ? 1 : MaxLitterSize());
			}
			float projectedHeads = floor.ProjectedHeads;
			if (floor.Area / Mathf.Max(projectedHeads, 0.01f) < LivestockFloor.BreedingFloorPerHead)
			{
				return false;
			}
			if (IsFemale)
			{
				return floor.Area / (projectedHeads + LitterHeads) > Livestock.lethalFloorPerHead;
			}
			return true;
		}
	}

	public float SaleValueScale
	{
		get
		{
			if (isServer)
			{
				return SpeciesSaleScale() * GeneSaleScale() * ConditionSaleScale() * Potential;
			}
			return saleValueScale;
		}
	}

	public float SaleQuality => Mathf.Clamp01(SaleValueScale / Mathf.Max(0.01f, Livestock.priceFullQualityScale));

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
	private int NameAuthorSourceSync
	{
		[CompilerGenerated]
		get
		{
			return __sync_NameAuthorSourceSync;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_NameAuthorSourceSync, value))
			{
				__sync_NameAuthorSourceSync = value;
				byte nameID = __GetWeaverID("NameAuthorSourceSync");
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

	public bool IsFemale => !IsMale;

	public TickableNeed Fullness => GetNeed(Need.Fullness);

	public TickableNeed PersonalSpace => GetNeed(Need.PersonalSpace);

	public TickableNeed Hydration => GetNeed(Need.Hydration);

	public TickableNeed Grass => GetNeed(Need.Grass);

	public TickableNeed Sun => GetNeed(Need.Sun);

	public float GrazingShare => Mathf.Clamp01(GrassFoodShare) * Grass.NeedValue;

	public bool IsHappy => Contentment > 0.5f;

	public float Contentment
	{
		get
		{
			float shelterNeedFloor = Livestock.shelterNeedFloor;
			return Mathf.Min(Mathf.Min(Mathf.Min(Mathf.Min(Fullness.NeedValue, Hydration.NeedValue), PersonalSpace.NeedValue), Mathf.Max(shelterNeedFloor, Grass.NeedValue)), Mathf.Max(shelterNeedFloor, Sun.NeedValue));
		}
	}

	public int MilkCharges { get; protected set; }

	public bool RecentlyHeardLoudNoise
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return TimeSince.op_Implicit(lastLoudNoise) < 10f;
		}
	}

	public EntityRef<LivestockAnimal> PregnantPartner { get; set; }

	public int PregnantPartnerGenes { get; set; }

	public bool IsVeryHappy => Contentment >= Livestock.friskyThreshold;

	public bool IsFindingItsFeet => findingItsFeet;

	public LivestockAnimal MotherAnimal => Mother.Get(isServer);

	public bool IsBeingPurchased { get; set; }

	public bool LeadTrusted => leadTrusted;

	public bool StandsItsGround
	{
		get
		{
			if (IsAdult())
			{
				if (!IsMale)
				{
					return IsProtective;
				}
				return true;
			}
			return false;
		}
	}

	public virtual bool ProducesYield
	{
		get
		{
			if (HasOwner && !IsInfant())
			{
				return !IsDead();
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

	public bool IsProtectiveOfPlayer
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			float motherlyDuration = Livestock.motherlyDuration;
			if (motherlyDuration > 0f && bondEverThreatened)
			{
				return TimeSince.op_Implicit(lastBondThreat) < motherlyDuration;
			}
			return false;
		}
	}

	public bool IsProtective
	{
		get
		{
			if (!IsProtectiveOfCalf)
			{
				return IsProtectiveOfPlayer;
			}
			return true;
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
			if (HasOwner && !BreedingPaused && !SpeciesAtCap)
			{
				return HasRoomToBreed;
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

	public float TroughCaloriesPerSecond
	{
		get
		{
			if (!(TroughFullnessPerCalorie > 0f))
			{
				return 0f;
			}
			return DrainPerSecond(Need.Fullness) * (1f - GrazingShare) / TroughFullnessPerCalorie;
		}
	}

	private float GrassShareOfFullness => grassShareOfFullness ?? GrazingShare;

	public float GrassRoom => Mathf.Max(0f, GrazingShare - Fullness.NeedValue * GrassShareOfFullness);

	public bool GrassCanFeed => GrassRoom >= 1f - ConsumeThreshold;

	public bool HasGrazedItsShare
	{
		get
		{
			if (GrazingShare > 0f)
			{
				return !GrassCanFeed;
			}
			return false;
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
			if (rpc == 3115049114u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RequestNameChange"));
				}
				using (TimeWarning.New("RequestNameChange"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3115049114u, "RequestNameChange", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3115049114u, "RequestNameChange", this, player, 3f))
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
							RequestNameChange(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RequestNameChange");
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
							RPCMessage msg4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_Lead(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RPC_Lead");
					}
				}
				return true;
			}
			if (rpc == 3752731948u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ServerPet"));
				}
				using (TimeWarning.New("ServerPet"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3752731948u, "ServerPet", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3752731948u, "ServerPet", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ServerPet(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in ServerPet");
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
		if (CanGrowUp)
		{
			ageTimer.Start(TimeToGrow * Rng.Range(1f - GrowTimeVariance, 1f + GrowTimeVariance) * Livestock.ageScale);
		}
	}

	private void TakeAgeFrom(LivestockAnimal previous)
	{
		ageLived = previous.ageLived;
		if (Age == previous.Age)
		{
			previous.ageTimer.CopyTo(ageTimer);
		}
	}

	private void FoldSavedAdultStage()
	{
		if (IsAdult() && ageTimer.IsRunning)
		{
			ageLived += ageTimer.Elapsed;
			ageTimer.Stop();
		}
	}

	private void OnAgeTimerElapsed()
	{
		if (!IsDead() && IsInfant() && IsAging)
		{
			if (!HasAdultForm)
			{
				Debug.LogWarning((object)(Categorize() + " (" + PrefabName + ") is an Infant whose Species asset names no adult form; it will not age."));
			}
			else
			{
				GrowIntoAdult();
			}
		}
	}

	public bool ForceAgeUp()
	{
		if (IsDead() || !IsInfant() || !HasAdultForm)
		{
			return false;
		}
		ageTimer.Stop();
		return (Object)(object)GrowIntoAdult() != (Object)null;
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
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		next.health = next.MaxHealth() * healthFraction;
		if (next.Form == SexForm.Either)
		{
			next.IsMale = IsMale;
		}
		next.SetLeading(IsLeading());
		next.LeadingPlayer = LeadingPlayer;
		next.leadSlack = leadSlack;
		next.leadTrusted = leadTrusted;
		next.untrustedLeadHeld = untrustedLeadHeld;
		next.leaderBeforeMount = leaderBeforeMount;
		leaderBeforeMount = default;
		HandOverMountTo(next);
		timers.CopyTo(next.timers);
		next.PregnantPartner = PregnantPartner;
		next.pregnantPartnerName = pregnantPartnerName;
		next.pregnantPartnerNameSource = pregnantPartnerNameSource;
		next.pregnantPartnerNameAuthorId = pregnantPartnerNameAuthorId;
		next.sleepFraction = sleepFraction;
		next.GiveName(AnimalName, NameAuthorSource, NameAuthorId);
		next.TakeLineageFrom(this);
		next.HeirNamed = HeirNamed;
		next.Mother = Mother;
		next.Father = Father;
		TransferFamiliarityTo(next);
		next.Genes = Genes;
		next.CoatSeed = CoatSeed;
		next.TakeAgeFrom(this);
		next.RestartDungForGenes();
		next.RestartProductionForGenes();
		for (int i = 0; i < needs.Length && i < next.needs.Length; i++)
		{
			next.needs[i] = needs[i];
		}
		next.StandOn(floor);
		TransferConditionTo(next);
		TransferPotentialTo(next);
		next.TakeHomeFrom(this);
	}

	private void LoadCoatSeed(int seed)
	{
		if (seed != CoatSeed)
		{
			CoatSeed = seed;
		}
	}

	private int CoatSeedOfAnOlderSave()
	{
		int num = LivestockPattern.SeedFor(AnimalName);
		if (num == 0)
		{
			return (int)Rng.NextSeed();
		}
		return num;
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
		return Mathf.InverseLerp(0f, Mathf.Max(0.01f, Livestock.conditionFullContentment), Contentment);
	}

	public void SetStarvedDown(bool down)
	{
		starvedDown = down;
	}

	private void TickCondition(float deltaTime)
	{
		if (deltaTime <= 0f || IsDead())
		{
			return;
		}
		UpdateBreedingPause();
		if (TOD_Sky.Instance.IsNight || IsAsleep)
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
		float conditionFloorHealth = ConditionFloorHealth;
		num3 = Mathf.Max(Mathf.MoveTowards(num3, num2, num / num4 * deltaTime), conditionFloorHealth);
		conditionUnsentHealth = num3 - Health();
		if (!(Mathf.Abs(conditionUnsentHealth) < num * 0.05f) || !(num3 > conditionFloorHealth) || !(num3 < num))
		{
			conditionUnsentHealth = 0f;
			if (!Mathf.Approximately(num3, Health()))
			{
				SetHealth(num3);
			}
			if (Health() <= 0f)
			{
				DieOfCrowding();
			}
		}
	}

	private void DieOfCrowding()
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
		SetHealth(Mathf.Max(Mathf.Clamp01(value) * MaxHealth(), 1f));
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
		dungTimer.Start(BoostedWait(DungInterval / GeneScale(LivestockGene.Dung) * DungIntervalConditionScale() * Rng.Range(1f - DungIntervalVariance, 1f + DungIntervalVariance)));
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

	public static bool IsDungBoostFood(ItemModConsumable food)
	{
		foreach (ModifierDefintion modifier in food.modifiers)
		{
			if (modifier.type == Modifier.ModifierType.HorseDungProductionBoost)
			{
				return true;
			}
		}
		return false;
	}

	public void ApplyDungBoost(ItemModConsumable food)
	{
		if ((Object)(object)food == (Object)null || !DungEnabled || !dungTimer.IsRunning)
		{
			return;
		}
		foreach (ModifierDefintion modifier in food.modifiers)
		{
			if (modifier.type == Modifier.ModifierType.HorseDungProductionBoost)
			{
				float wait = UnboostedWait(dungTimer.Remaining);
				dungBoostRate = Mathf.Max((DungBoostTimeLeft > 0f) ? dungBoostRate : 1f, modifier.value);
				dungBoostUntil = Mathf.Max(dungBoostUntil, Time.time + modifier.duration);
				dungTimer.Reschedule(BoostedWait(wait));
			}
		}
	}

	public void RenewDungBoost(ItemModConsumable food)
	{
		if (DungBoostTimeLeft <= 0f)
		{
			ApplyDungBoost(food);
		}
	}

	private float BoostedWait(float wait)
	{
		float dungBoostTimeLeft = DungBoostTimeLeft;
		float num = dungBoostTimeLeft * dungBoostRate;
		if (!(wait <= num))
		{
			return dungBoostTimeLeft + (wait - num);
		}
		return wait / dungBoostRate;
	}

	private float UnboostedWait(float realWait)
	{
		float dungBoostTimeLeft = DungBoostTimeLeft;
		if (!(realWait <= dungBoostTimeLeft))
		{
			return dungBoostTimeLeft * dungBoostRate + (realWait - dungBoostTimeLeft);
		}
		return realWait * dungBoostRate;
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
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)DungItem == (Object)null))
		{
			Item item = ItemManager.Create(DungItem, 1, 0uL, isServerSide: true, 0uL);
			if (item != null)
			{
				item.SetItemOwnership(NameOrLabel(), ItemOwnershipPhrases.Pooped);
				Vector3 val = (((Object)(object)DungSpawnPoint != (Object)null) ? DungSpawnPoint.position : ((Component)this).transform.position);
				item.Drop(vVelocity: new Vector3(Rng.Range(-0.5f, 0.5f), Rng.Range(-3f, -1f), Rng.Range(-0.5f, 0.5f)), vPos: val + Rng.InsideUnitSphere * 0.1f, rotation: Rng.RotationUniform);
				LivestockCensus.RecordDung(val);
			}
		}
	}

	public static float ThresholdFor(TrustTier tier)
	{
		return tier switch
		{
			TrustTier.Neutral => Livestock.trustToTolerate, 
			TrustTier.Trusted => Livestock.trustToLead, 
			TrustTier.Bonded => Livestock.maxTrust, 
			_ => 0f, 
		};
	}

	public bool ActsAsAdult(Personality personality)
	{
		if (personality != Personality.Sleepy)
		{
			return ActsCuriousAdult;
		}
		return ActsSleepy;
	}

	private bool HerdHasAdult(Personality personality, PooledList<LivestockAnimal> neighbours, LivestockAnimal ignore, bool onlyLowerId)
	{
		if (floor != null && !floor.IsField)
		{
			return floor.HasAdultOtherThan(this, personality, onlyLowerId);
		}
		foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)neighbours)
		{
			if (!((Object)(object)item == (Object)(object)ignore) && IsHerdMate(item) && item.ActsAsAdult(personality) && (!onlyLowerId || item.net.ID.Value < net.ID.Value))
			{
				return true;
			}
		}
		return false;
	}

	private void KeepOnePerHerd(PooledList<LivestockAnimal> neighbours)
	{
		if (ActsCuriousAdult && HerdHasAdult(Personality.Curious, neighbours, null, onlyLowerId: true))
		{
			IsCurious = false;
		}
		if (ActsSleepy && HerdHasAdult(Personality.Sleepy, neighbours, null, onlyLowerId: true))
		{
			IsSleepy = false;
		}
	}

	private void InitFamiliarity()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		probationCredited = TimeSince.op_Implicit(float.MaxValue);
		trustedNearbySeen = TimeSince.op_Implicit(float.MaxValue);
		if (!Application.isLoadingSave)
		{
			IsCurious = Rng.Value < Livestock.curiousChance;
			IsSleepy = !IsCurious && Livestock.sleepyChance > 0f && Rng.Value < Livestock.sleepyChance;
		}
		ValidateFamiliarityTuning();
	}

	public void SetCurious(bool curious)
	{
		IsCurious = curious;
		if (curious)
		{
			IsSleepy = false;
		}
	}

	public void SetSleepy(bool sleepy)
	{
		IsSleepy = sleepy;
		if (sleepy)
		{
			IsCurious = false;
		}
	}

	private void ValidateFamiliarityTuning()
	{
	}

	public bool TrustsToHandle(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null)
		{
			return TrustOf(player) >= Livestock.trustToLead;
		}
		return false;
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
		float num = Mathf.Max(StoredSecondsFor(player.userID), PartialCreditFor(player));
		if ((ulong)player.userID != OwnerID)
		{
			return Mathf.Min(num, NonOwnerTrustCap);
		}
		return num;
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
		if (IsProvokedBy(entity))
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
		return Mathf.Max(OwnerCreditFor(player), Mathf.Max(HousemateCreditFor(player), HandlingCreditFor(player)));
	}

	private float OwnerCreditFor(BasePlayer player)
	{
		if (!HasOwner || (ulong)player.userID != OwnerID)
		{
			return 0f;
		}
		return Livestock.trustToLead;
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
		if (trustFloor <= 0f || (Object)(object)player == (Object)null)
		{
			return 0f;
		}
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
		if ((Object)(object)serverInstance == (Object)null || !HasOwner || player.currentTeam == 0L)
		{
			return 0f;
		}
		RelationshipManager.PlayerTeam playerTeam = serverInstance.FindPlayersTeam(OwnerID);
		if (playerTeam == null || playerTeam.teamID != player.currentTeam)
		{
			return 0f;
		}
		return trustFloor;
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

	public void AddFamiliarity(ulong userId, float seconds)
	{
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		if (userId == 0L || Mathf.Approximately(seconds, 0f) || Interface.CallHook("OnLivestockAnimalFamiliarityAdd", this, userId, seconds) != null)
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
				Interface.CallHook("OnLivestockAnimalFamiliarityAdded", this, userId, seconds);
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
				seconds = Mathf.Min(seconds, maxTrust)
			};
			Interface.CallHook("OnLivestockAnimalFamiliarityAdded", this, userId, seconds);
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
			probation = default;
			probationCredited = TimeSince.op_Implicit(float.MaxValue);
		}
		Interface.CallHook("OnLivestockAnimalFamiliarityAdded", this, userId, seconds);
	}

	public void SetFamiliarity(ulong userId, float seconds)
	{
		if (userId != 0L)
		{
			AddFamiliarity(userId, seconds - StoredSecondsFor(userId));
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
		if (!IsAdult() || IsDead())
		{
			return;
		}
		float maxTrust = Livestock.maxTrust;
		if (maxTrust <= 0f || TrustOf(player) < maxTrust)
		{
			return;
		}
		float defendReactionTime = Livestock.defendReactionTime;
		if (!(defendReactionTime <= 0f) && !(player.SecondsSinceAttacked > defendReactionTime) && !(player.lastAttackedTime <= lastDefendedAttack))
		{
			if (!InSafeZone() && CanBeAnsweredFor(player.lastAttacker, out var _))
			{
				OnBondThreatened();
			}
			OnHerdMateHurt(player.lastAttacker, null);
			lastDefendedAttack = player.lastAttackedTime;
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
		if (!IsAdult())
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
		save.sleepy = IsSleepy;
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
		IsSleepy = save.sleepy;
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
	}

	private void TransferFamiliarityTo(LivestockAnimal grown)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			if (SocialSearchRadius > 0f)
			{
				Query.Server.GetBrainsInSphere(((Component)this).transform.position, SocialSearchRadius, (List<LivestockAnimal>)(object)val);
			}
			grown.IsCurious = IsCurious && !HerdHasAdult(Personality.Curious, val, grown, onlyLowerId: false);
			grown.IsSleepy = IsSleepy && !HerdHasAdult(Personality.Sleepy, val, grown, onlyLowerId: false);
			CopyAcquaintancesTo(grown);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void CopyAcquaintancesTo(LivestockAnimal other)
	{
		if (!((Object)(object)other == (Object)null) && acquaintances != null)
		{
			other.acquaintances = new Acquaintance[8];
			Array.Copy(acquaintances, other.acquaintances, 8);
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
	}

	public static float LadderProgress(float seconds)
	{
		float maxTrust = Livestock.maxTrust;
		if (!(maxTrust > 0f))
		{
			return 1f;
		}
		return Mathf.Clamp01(seconds / maxTrust);
	}

	public static TrustTier TierFor(float seconds)
	{
		if (seconds >= Livestock.maxTrust)
		{
			return TrustTier.Bonded;
		}
		if (seconds >= Livestock.trustToLead)
		{
			return TrustTier.Trusted;
		}
		if (seconds >= Livestock.trustToTolerate)
		{
			return TrustTier.Neutral;
		}
		return TrustTier.Hostile;
	}

	public static string TrustTierName(float seconds)
	{
		return TierFor(seconds) switch
		{
			TrustTier.Bonded => "bonded", 
			TrustTier.Trusted => "trusts you", 
			TrustTier.Neutral => "neutral", 
			_ => "hostile", 
		};
	}

	public string GetFamiliarityDebugInfo()
	{
		float num = BestFamiliarity(out var userId);
		string text;
		if (ActsCurious)
		{
			text = " curious";
		}
		else
		{
			text = (ActsSleepy ? " sleepy" : string.Empty);
		}
		if (userId != 0L)
		{
			return $"fam {num:0}s {TrustTierName(num)}{text}";
		}
		return "fam none hostile" + text;
	}

	public void HoldSlot(BaseEntity owner, BaseEntity source, int index, Vector3 standingPoint, Vector3 facing, LivestockFeedingSlots.Serves need)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		slotOwner.Set(owner);
		slotSource.Set(source);
		slotIndex = index;
		slotStandingPoint = standingPoint;
		slotFacing = facing;
		slotFootprint = LivestockFootprint.Of((BaseEntity)this, standingPoint, facing);
		slotNeed = need;
		slotInUse = true;
		askedToVacate = false;
		LeaveQueue(need);
	}

	public void FinishWithSlot()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (slotInUse)
		{
			slotInUse = false;
			slotLeft = TimeSince.op_Implicit(0f);
		}
	}

	public void ForgetSlot()
	{
		slotIndex = -1;
		slotInUse = false;
		askedToVacate = false;
	}

	public bool HoldsAnySlotAt(BaseEntity owner)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (slotIndex < 0 || (Object)(object)owner == (Object)null || (Object)(object)slotOwner.Get(serverside: true) != (Object)(object)owner)
		{
			return false;
		}
		if (!slotInUse)
		{
			if (TimeSince.op_Implicit(slotLeft) < 30f)
			{
				return IsStillInSlot();
			}
			return false;
		}
		return true;
	}

	private bool IsStillInSlot()
	{
		return LivestockFootprint.Penetration(LivestockFootprint.Of(this), HeldSlotFootprint) > 0f;
	}

	public bool ShouldVacateSlot()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (slotIndex < 0 || slotInUse)
		{
			return false;
		}
		BaseEntity baseEntity = slotOwner.Get(serverside: true);
		if ((Object)(object)baseEntity == (Object)null || baseEntity.IsDestroyed || TimeSince.op_Implicit(slotLeft) >= 30f || !IsStillInSlot())
		{
			ForgetSlot();
			return false;
		}
		if (!askedToVacate && WantsMoreFrom(slotSource.Get(serverside: true), slotNeed))
		{
			return IsAnyoneQueuedAt(baseEntity, slotNeed);
		}
		return true;
	}

	public bool TryGetSlotToVacate(out BaseEntity owner, out Vector3 standingPoint, out Vector3 facing)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		owner = slotOwner.Get(serverside: true);
		standingPoint = slotStandingPoint;
		facing = slotFacing;
		if (slotIndex >= 0 && !slotInUse)
		{
			return (Object)(object)owner != (Object)null;
		}
		return false;
	}

	public void AskToVacate(BaseEntity owner, int index, Vector3 standingPoint, Vector3 facing, in LivestockFootprint needed)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (!slotInUse && !FoundNoWayOutAt(owner))
		{
			slotOwner.Set(owner);
			slotSource.Set(null);
			slotIndex = index;
			slotStandingPoint = standingPoint;
			slotFacing = facing;
			slotFootprint = needed;
			slotNeed = (LivestockFeedingSlots.Serves)0;
			askedToVacate = true;
			slotLeft = TimeSince.op_Implicit(0f);
		}
	}

	public bool WantsMoreFrom(BaseEntity source, LivestockFeedingSlots.Serves need)
	{
		if ((need & LivestockFeedingSlots.Serves.Food) != 0)
		{
			if (NeedsFood() && source is HitchTrough trough)
			{
				Item foundItem;
				return CanEatFrom(trough, out foundItem);
			}
			return false;
		}
		if ((need & LivestockFeedingSlots.Serves.Water) != 0)
		{
			if (NeedsWater() && source is LiquidContainer container)
			{
				return CanDrinkFrom(container);
			}
			return false;
		}
		return false;
	}

	public SlotState SlotStateFor(BaseEntity owner, int index, in LivestockFootprint footprint, PooledList<LivestockAnimal> neighbours, out LivestockAnimal blocker)
	{
		blocker = null;
		if (LivestockFeedingSlots.IsHitchedOver(owner, in footprint))
		{
			return SlotState.HitchedOver;
		}
		SlotState result = SlotState.Free;
		foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)neighbours)
		{
			if ((Object)(object)item == (Object)(object)this || item.IsDestroyed || item.IsDead())
			{
				continue;
			}
			if (item.HoldsAnySlotAt(owner) && (item.slotIndex == index || LivestockFootprint.Penetration(in footprint, item.HeldSlotFootprint) > 0f))
			{
				blocker = item;
				return SlotState.Held;
			}
			if (!(LivestockFootprint.Penetration(in footprint, item.RestingFootprint()) <= 0f))
			{
				if (item.FoundNoWayOutAt(owner))
				{
					blocker = item;
					return SlotState.CannotBeCleared;
				}
				if (!item.IsStandingIdle())
				{
					blocker = item;
					return SlotState.Blocked;
				}
				if ((Object)(object)blocker == (Object)null)
				{
					blocker = item;
					result = SlotState.BlockedByIdler;
				}
			}
		}
		return result;
	}

	public void SetAtTrough(bool atTrough)
	{
		SetNetworkedFlag(Flags.Reserved18, atTrough);
	}

	public bool IsStandingIdle()
	{
		LivestockAnimalFSM livestockAnimalFSM = frailtyFsm;
		RustNavMeshAgent navAgent = NavAgent;
		if ((Object)(object)livestockAnimalFSM != (Object)null && livestockAnimalFSM.CurrentState == livestockAnimalFSM.randomIdle && !IsOffItsFeet() && !IsHeadDown())
		{
			if (!((Object)(object)navAgent == (Object)null))
			{
				return !navAgent.hasPath;
			}
			return true;
		}
		return false;
	}

	private ref SlotQueue QueueFor(LivestockFeedingSlots.Serves need)
	{
		if ((need & LivestockFeedingSlots.Serves.Water) != 0)
		{
			return ref waterQueue;
		}
		return ref foodQueue;
	}

	public void JoinQueue(BaseEntity owner, LivestockFeedingSlots.Serves need)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		ref SlotQueue reference = ref QueueFor(need);
		if (!IsQueuedAt(owner, need))
		{
			reference.Waited = TimeSince.op_Implicit(0f);
		}
		reference.Owner.Set(owner);
		reference.Renewed = TimeSince.op_Implicit(0f);
	}

	public void LeaveQueue(LivestockFeedingSlots.Serves need)
	{
		QueueFor(need).Owner.Set(null);
	}

	public void LeaveQueues()
	{
		LeaveQueue(LivestockFeedingSlots.Serves.Food);
		LeaveQueue(LivestockFeedingSlots.Serves.Water);
	}

	public bool IsQueuedAt(BaseEntity owner, LivestockFeedingSlots.Serves need)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)owner == (Object)null || need == 0)
		{
			return false;
		}
		ref SlotQueue reference = ref QueueFor(need);
		if (TimeSince.op_Implicit(reference.Renewed) < 20f)
		{
			return (Object)(object)reference.Owner.Get(serverside: true) == (Object)(object)owner;
		}
		return false;
	}

	public float QueuedFor(BaseEntity owner, LivestockFeedingSlots.Serves need)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (!IsQueuedAt(owner, need))
		{
			return 0f;
		}
		return TimeSince.op_Implicit(QueueFor(need).Waited);
	}

	public int CountQueuedAhead(BaseEntity owner, LivestockFeedingSlots.Serves need, PooledList<LivestockAnimal> neighbours)
	{
		bool flag = IsQueuedAt(owner, need);
		float num = QueuedFor(owner, need);
		int num2 = 0;
		foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)neighbours)
		{
			if (!((Object)(object)item == (Object)(object)this) && !item.IsDestroyed && !item.IsDead() && item.IsQueuedAt(owner, need) && (!flag || item.QueuedFor(owner, need) > num))
			{
				num2++;
			}
		}
		return num2;
	}

	public bool IsAnyoneQueuedAt(BaseEntity owner, LivestockFeedingSlots.Serves need)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			Query.Server.GetBrainsInSphere(((Component)owner).transform.position, 30f, (List<LivestockAnimal>)(object)val);
			foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
			{
				if ((Object)(object)item != (Object)(object)this && !item.IsDestroyed && !item.IsDead() && item.IsQueuedAt(owner, need))
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

	public void MarkSlotFailed()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		failedSlotOwner = slotOwner;
		failedSlotIndex = slotIndex;
		failedSlotAt = TimeSince.op_Implicit(0f);
	}

	public bool RecentlyFailedAt(BaseEntity owner, int index)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (failedSlotIndex == index && TimeSince.op_Implicit(failedSlotAt) < 15f)
		{
			return (Object)(object)failedSlotOwner.Get(serverside: true) == (Object)(object)owner;
		}
		return false;
	}

	public void FoundNoWayOut(BaseEntity owner)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		noWayOutOwner.Set(owner);
		noWayOutAt = TimeSince.op_Implicit(0f);
		ForgetSlot();
	}

	public bool FoundNoWayOutAt(BaseEntity owner)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)owner != (Object)null && TimeSince.op_Implicit(noWayOutAt) < 30f)
		{
			return (Object)(object)noWayOutOwner.Get(serverside: true) == (Object)(object)owner;
		}
		return false;
	}

	public void NoteSlotSearchFailed()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (!slotSearchFailing)
		{
			slotSearchFailing = true;
			slotSearchFailedAt = TimeSince.op_Implicit(0f);
		}
	}

	public void NoteSlotSearchSettled()
	{
		slotSearchFailing = false;
	}

	private void InitFrailty()
	{
		((Component)this).TryGetComponent<LivestockAnimalFSM>(ref frailtyFsm);
		((Component)this).TryGetComponent<RustNavMeshAgent>(ref frailtyAgent);
		if ((Object)(object)frailtyAgent != (Object)null)
		{
			frailtyAgent.gaitPenalty = AgeGaitPenalty;
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

	public float DrainPerSecond(Need need)
	{
		return DecayRatePerSecond(need) * NeedScale;
	}

	public static float GeneScaleOf(int genome, LivestockGene gene)
	{
		return ScaleAt(gene, LivestockGeneRange.TraitFor(genome, gene));
	}

	public float CarriedGeneScale(LivestockGene gene)
	{
		if (!Livestock.genesEnabled)
		{
			return 1f;
		}
		return ScaleAt(gene, LivestockGeneRange.TraitOf(Genes, Carried(gene)));
	}

	private static float ScaleAt(LivestockGene gene, float trait)
	{
		float num = ((trait < 0.5f) ? Mathf.Lerp(BadScaleFor(gene), 1f, trait * 2f) : Mathf.Lerp(1f, GoodScaleFor(gene), trait * 2f - 1f));
		return Mathf.Max(0.01f, num);
	}

	private static float BadScaleFor(LivestockGene gene)
	{
		return gene switch
		{
			LivestockGene.Dung => Livestock.geneDungBad, 
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
			LivestockGene.Yield => Livestock.geneYieldGood, 
			LivestockGene.Fertility => Livestock.geneFertilityGood, 
			_ => Livestock.geneHardinessGood, 
		};
	}

	private void InitGenes()
	{
		Genes = RollGenome(Rng);
	}

	public static int RollGenome(NpcRandom random)
	{
		return LivestockGenome.Roll(random);
	}

	private int InheritedGenome(int fatherGenes)
	{
		int father = ((fatherGenes != 0) ? fatherGenes : Genes);
		return LivestockGenome.Combine(Rng, Genes, father, Livestock.geneMutationChance);
	}

	public int LitterSize()
	{
		float num = Mathf.Max(0f, GeneScale(LivestockGene.Fertility) - 1f);
		int num2 = Mathf.FloorToInt(num);
		if (Rng.Value < num - (float)num2)
		{
			num2++;
		}
		return Mathf.Clamp(1 + num2, 1, Mathf.Max(1, Livestock.maxLitterSize));
	}

	public int MaxLitterSize()
	{
		float num = Mathf.Max(0f, GeneScale(LivestockGene.Fertility) - 1f);
		return Mathf.Clamp(1 + Mathf.CeilToInt(num), 1, Mathf.Max(1, Livestock.maxLitterSize));
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
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (!HasOwner || (ulong)player.userID != OwnerID || TimeSince.op_Implicit(lastAdoptionCheck) < 5f)
		{
			return;
		}
		lastAdoptionCheck = TimeSince.op_Implicit(0f);
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(cached: true);
		if (!((Object)(object)buildingPrivilege == (Object)null) && buildingPrivilege.IsAuthed(player))
		{
			if ((Object)(object)HomeTc.Get(isServer) != (Object)(object)buildingPrivilege)
			{
				SetHomePosition(((Component)buildingPrivilege).transform.position);
			}
			HomeTc = new EntityRef<BuildingPrivlidge>(buildingPrivilege.net.ID);
		}
	}

	private void AdoptHomeOf(HitchTrough trough)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		BuildingPrivlidge buildingPrivilege = trough.GetBuildingPrivilege();
		HomeTc = (((Object)(object)buildingPrivilege != (Object)null) ? new EntityRef<BuildingPrivlidge>(buildingPrivilege.net.ID) : default(EntityRef<BuildingPrivlidge>));
		SetHomePosition(((Component)trough).transform.position);
	}

	private void SaveHome(LivestockAnimal save)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		save.homePosition = homePosition;
		save.homeTc = HomeTc.uid;
	}

	private void LoadHome(LivestockAnimal save)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (save.homePosition != Vector3.zero)
		{
			homePosition = save.homePosition;
		}
		HomeTc = new EntityRef<BuildingPrivlidge>(save.homeTc);
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
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (ConsumeSearchRadius <= 0f)
		{
			return;
		}
		PooledList<LiquidContainer> val = Pool.Get<PooledList<LiquidContainer>>();
		try
		{
			LivestockFloor currentFloor = CurrentFloor;
			if (currentFloor != null)
			{
				currentFloor.ContainersWithin(((Component)this).transform.position, ConsumeSearchRadius, (List<LiquidContainer>)(object)val);
			}
			else
			{
				Query.Server.GetInSphere(((Component)this).transform.position, ConsumeSearchRadius, (List<LiquidContainer>)(object)val);
			}
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
		return FreshWaterIn(container) >= WaterForADrink();
	}

	public static int FreshWaterIn(LiquidContainer container)
	{
		Item liquidItem = container.GetLiquidItem();
		if (liquidItem == null || !((Object)(object)liquidItem.info == (Object)(object)WaterTypes.WaterItemDef))
		{
			return 0;
		}
		return liquidItem.amount;
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

	public string NameOrLabel()
	{
		if (!string.IsNullOrEmpty(AnimalName))
		{
			return AnimalName;
		}
		return WildLabel().english;
	}

	protected virtual Phrase WildLabel()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected Obj, but got Unknown
		//IL_0022: Expected Obj, but got Unknown
		Phrase val = wildCategoryLabel;
		if (val == null)
		{
			Phrase val2 = new Phrase(string.Empty, Categorize());
			Phrase val3 = val2;
			wildCategoryLabel = val2;
			val = val3;
		}
		return val;
	}

	private void NameForOwner(ulong ownerId, string familyLocale)
	{
		string locale = (LivestockNames.IsDrawable(familyLocale) ? familyLocale : LocaleFor(ownerId));
		if (Rng.Value < Livestock.devNameChance && LivestockNames.HasPack("devs"))
		{
			locale = "devs";
		}
		GiveName(RollAnimalName(Rng, IsMale, locale), FarmableAnimal.NameSource.Game, 0uL);
		FoundLine(locale);
	}

	private void GiveName(string name, FarmableAnimal.NameSource source, ulong authorId = 0uL)
	{
		ForgetSpecial(AnimalName);
		NameAuthorSourceSync = (int)source;
		NameAuthorId = ((source == FarmableAnimal.NameSource.Player) ? authorId : 0);
		AnimalName = name;
		if (HasSpecialAuthoredName)
		{
			RememberSpecial(name, discovered: false);
		}
	}

	public void UpdateNameFromPlayer(string name, string locale, ulong authorId)
	{
		GiveName(name, FarmableAnimal.NameSource.Player, authorId);
		FoundLine(locale);
	}

	public bool CanBeRenamedBy(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null || IsDead() || HasSpecialAuthoredName || !TrustsToHandle(player))
		{
			return false;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(cached: true);
		if ((Object)(object)buildingPrivilege != (Object)null)
		{
			return buildingPrivilege.IsAuthed(player);
		}
		if (HasOwner && (ulong)player.userID == OwnerID)
		{
			return true;
		}
		if (TryGetHomeCupboard(out var cupboard))
		{
			return cupboard.IsAuthed(player);
		}
		return false;
	}

	public bool TryRename(BasePlayer player, string requestedName)
	{
		if (!CanBeRenamedBy(player))
		{
			return false;
		}
		string text = FarmableAnimal.CleanPlayerName(requestedName);
		if (text == null)
		{
			return false;
		}
		if (text == AnimalName)
		{
			return true;
		}
		UpdateNameFromPlayer(text, LocaleFor(player.userID), player.userID);
		return true;
	}

	public string GetNameOrLabelAsViewedBy(BasePlayer viewer)
	{
		if (string.IsNullOrEmpty(AnimalName) || !HidesNameFrom(viewer, NameAuthorSource, NameAuthorId))
		{
			return NameOrLabel();
		}
		return RandomUsernames.GetCapitalized(net.ID.Value);
	}

	public string GetFatherNameAsViewedBy(BasePlayer viewer)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(pregnantPartnerName))
		{
			return null;
		}
		if (!HidesNameFrom(viewer, pregnantPartnerNameSource, pregnantPartnerNameAuthorId))
		{
			return pregnantPartnerName;
		}
		return RandomUsernames.GetCapitalized(PregnantPartner.uid.Value);
	}

	private static bool HidesNameFrom(BasePlayer viewer, FarmableAnimal.NameSource source, ulong authorId)
	{
		if ((Object)(object)viewer == (Object)null || !viewer.IsConnected || !viewer.net.connection.info.GetBool("global.streamermode"))
		{
			return false;
		}
		if (FarmableAnimal.StreamerCanHideNameFromSource(source))
		{
			return (ulong)viewer.userID != authorId;
		}
		return false;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	private void RequestNameChange(RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null) && !TryRename(msg.player, msg.read.String(12)))
		{
			ClientRPC(RpcTarget.Player("Client_NameChangeRejected", msg.player));
		}
	}

	public static string LocaleFor(ulong userId)
	{
		BasePlayer basePlayer = BasePlayer.FindByID(userId);
		Connection connection = (((Object)(object)basePlayer != (Object)null && basePlayer.net != null) ? basePlayer.net.connection : null);
		if (connection == null)
		{
			return LivestockNames.HouseLocale();
		}
		string text = Translate.ValidateServerLanguage(connection.language);
		if (!LivestockNames.IsDrawable(text))
		{
			return LivestockNames.HouseLocale();
		}
		return text;
	}

	public static string RollAnimalName(NpcRandom random, bool male, string locale)
	{
		string[] array = LivestockNames.PoolFor(male, locale);
		if (array.Length == 0)
		{
			return string.Empty;
		}
		string text = array[random.Range(0, array.Length)];
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
		FarmableAnimal.NameSource source = ((livestockAnimal.NameAuthorSource != FarmableAnimal.NameSource.Player) ? FarmableAnimal.NameSource.Game : FarmableAnimal.NameSource.Player);
		GiveName(text, source, livestockAnimal.NameAuthorId);
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
		save.nameAuthorId = NameAuthorId;
	}

	private void LoadName(LivestockAnimal save)
	{
		Generation = save.generation;
		NameLocale = save.nameLocale;
		LineStem = save.lineStem;
		HeirNamed = save.heirNamed;
		NameAuthorId = save.nameAuthorId;
		if (HasSpecialAuthoredName)
		{
			RememberSpecial(AnimalName, discovered: false);
		}
	}

	public void ReactToUntrustedHandling(BasePlayer player)
	{
		OnGrabbedBy(player);
		if (!((Object)(object)player == (Object)null) && player.IsConnected)
		{
			if (string.IsNullOrEmpty(AnimalName))
			{
				player.ShowToast(GameTip.Styles.Error, NotTrustedWildPhrase, false);
				return;
			}
			player.ShowToast(GameTip.Styles.Error, NotTrustedPhrase, false, GetNameOrLabelAsViewedBy(player));
		}
	}

	public bool SetOwner(ulong userId, OwnerChangeReason reason, string familyLocale = null)
	{
		if (userId == 0L || userId == OwnerID)
		{
			return false;
		}
		OwnerID = userId;
		if (string.IsNullOrEmpty(AnimalName))
		{
			NameForOwner(userId, familyLocale);
		}
		StartAgeTimerIfIdle();
		RefreshProduction();
		if (reason != OwnerChangeReason.Debug)
		{
			Facepunch.Rust.Analytics.Azure.OnLivestockAcquired(userId, this, reason);
		}
		return true;
	}

	protected virtual void RefreshProduction()
	{
	}

	public virtual void RestartProductionForGenes()
	{
	}

	public void ClaimByTrough(HitchTrough trough)
	{
		if (!((Object)(object)trough == (Object)null) && trough.OwnerID != 0L && Interface.CallHook("OnLivestockAnimalClaim", this, trough) == null)
		{
			SetOwner(trough.OwnerID, OwnerChangeReason.Trough);
			AdoptHomeOf(trough);
		}
	}

	public string OwnerNameFor(BasePlayer viewer)
	{
		if (!HasOwner)
		{
			return string.Empty;
		}
		BasePlayer basePlayer = BasePlayer.FindByID(OwnerID);
		bool flag = (Object)(object)viewer != (Object)null && viewer.IsConnected;
		if ((Object)(object)basePlayer != (Object)null)
		{
			if (!flag)
			{
				return basePlayer.displayName;
			}
			return NameHelper.GetPlayerNameStreamSafe(viewer, basePlayer);
		}
		if (flag && viewer.net.connection.info.GetBool("global.streamermode"))
		{
			return RandomUsernames.Get(OwnerID);
		}
		return OfflineOwnerName();
	}

	private string OfflineOwnerName()
	{
		if (offlineOwnerNameId == OwnerID)
		{
			return offlineOwnerName;
		}
		ServerMgr instance = SingletonComponent<ServerMgr>.Instance;
		offlineOwnerNameId = OwnerID;
		offlineOwnerName = (((Object)(object)instance != (Object)null && instance.persistance != null) ? (instance.persistance.GetPlayerName(OwnerID) ?? string.Empty) : string.Empty);
		return offlineOwnerName;
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

	private void TickPotential(float deltaTime)
	{
		if (!(deltaTime <= 0f) && IsEarningPotential)
		{
			potential = Mathf.Min(potential, PotentialCeiling);
			float potentialTarget = PotentialTarget;
			float num = ((potentialTarget > potential) ? Livestock.potentialRiseSeconds : Livestock.potentialFallSeconds);
			if (num <= 0f)
			{
				potential = potentialTarget;
			}
			else
			{
				potential = Mathf.MoveTowards(potential, potentialTarget, deltaTime / num);
			}
		}
	}

	public void SetPotential(float value)
	{
		potential = Mathf.Clamp01(value);
	}

	private void SavePotential(LivestockAnimal msg)
	{
		msg.potential = potential;
	}

	private void LoadPotential(LivestockAnimal msg)
	{
		potential = Mathf.Clamp01(msg.potential);
	}

	private void TransferPotentialTo(LivestockAnimal next)
	{
		next.potential = potential;
	}

	public bool LiesInsideWallsAt(Vector3 position, Vector3 forward)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		RustNavMeshAgent navAgent = NavAgent;
		if (!((Object)(object)navAgent == (Object)null))
		{
			return navAgent.BoxInsideWalls(LyingBody, position, forward);
		}
		return true;
	}

	public bool TryFindLyingSpotNearby(out Vector3 spot)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		spot = default;
		if (LiesInsideWallsAt(((Component)this).transform.position, ((Component)this).transform.forward))
		{
			return false;
		}
		return TryFindLyingSpotNear(((Component)this).transform.position, 2.5f, 6f, out spot);
	}

	public void SetSleepFraction(float fraction)
	{
		sleepFraction = Mathf.Clamp01(fraction);
	}

	private void InitSleep()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		sleepFraction = Rng.Value;
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

	private void TrackHerdMates(PooledList<LivestockAnimal> candidates)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		LivestockAnimal livestockAnimal = null;
		float num = float.MaxValue;
		foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)candidates)
		{
			if (IsMyDependent(item))
			{
				Vector3 val = ((Component)item).transform.position - ((Component)this).transform.position;
				float sqrMagnitude = val.sqrMagnitude;
				if (!(sqrMagnitude >= num))
				{
					livestockAnimal = item;
					num = sqrMagnitude;
				}
			}
		}
		if ((Object)(object)livestockAnimal != (Object)null)
		{
			RememberShielded(livestockAnimal);
		}
		TickCalfProximity(livestockAnimal);
		if (Time.time - freeRangeHerdCountedAt >= NeedTickFrequency)
		{
			CountFreeRangeHerd();
		}
	}

	private void CountFreeRangeHerd()
	{
		if (!CountsFreeRangeHerd)
		{
			return;
		}
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			int num = GatherFreeRangeHerd(Livestock.maxFreeRangeHerd, (List<LivestockAnimal>)(object)val);
			float time = Time.time;
			foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
			{
				item.freeRangeHerdMates = ((List<LivestockAnimal>)(object)val).Count - 1;
				item.freeRangeHerdUnborn = num - item.LitterOnTheWay;
				item.freeRangeHerdCountedAt = time;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	internal int GatherFreeRangeHerd(int limit, List<LivestockAnimal> herd)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			float freeRangeLinkRadius = Livestock.freeRangeLinkRadius;
			int num = ++lastFreeRangeHerdSearch;
			int num2 = JoinFreeRangeSearch(this, num, herd);
			for (int i = 0; i < herd.Count; i++)
			{
				if (herd.Count + num2 >= limit)
				{
					break;
				}
				if (!(freeRangeLinkRadius > 0f))
				{
					break;
				}
				((List<LivestockAnimal>)(object)val).Clear();
				Query.Server.GetBrainsInSphere(((Component)herd[i]).transform.position, freeRangeLinkRadius, (List<LivestockAnimal>)(object)val);
				foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
				{
					if (item.freeRangeHerdSearch != num && IsHerdMate(item) && !item.StandsInAPen)
					{
						num2 += JoinFreeRangeSearch(item, num, herd);
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

	private static int JoinFreeRangeSearch(LivestockAnimal animal, int search, List<LivestockAnimal> herd)
	{
		animal.freeRangeHerdSearch = search;
		herd.Add(animal);
		return animal.LitterOnTheWay;
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
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		using (LivestockProfiler.Sample(LivestockProfiler.Section.Gather))
		{
			PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
			try
			{
				Query.Server.GetBrainsInSphere(around, 30f, (List<LivestockAnimal>)(object)val);
				foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
				{
					if (!((Object)(object)item == (Object)(object)this) && !item.IsDead())
					{
						((List<LivestockFootprint>)(object)footprints).Add(item.RestingFootprint());
						if (item.TryGetTurningRestingFootprint(out var footprint))
						{
							((List<LivestockFootprint>)(object)footprints).Add(footprint);
						}
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public LivestockFootprint RestingFootprint()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		RustNavMeshAgent navAgent = NavAgent;
		if ((Object)(object)navAgent != (Object)null && navAgent.TryGetArrival(out var standWS, out var facingWS))
		{
			return LivestockFootprint.Of((BaseEntity)this, standWS, facingWS);
		}
		if ((Object)(object)navAgent == (Object)null || !navAgent.hasPath)
		{
			return LivestockFootprint.Of(this);
		}
		if (navAgent.TryGetRestingPoint(navAgent.RestingDistance(navAgent.currentStoppingDistance), out var pointWS, out var headingWS))
		{
			return LivestockFootprint.Of((BaseEntity)this, pointWS, headingWS);
		}
		return RestingFootprintFor(navAgent.destinationWS, navAgent.currentStoppingDistance);
	}

	public bool TryGetTurningRestingFootprint(out LivestockFootprint footprint)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		footprint = default;
		RustNavMeshAgent navAgent = NavAgent;
		if ((Object)(object)navAgent == (Object)null || navAgent.TryGetArrival(out var _, out var _))
		{
			return false;
		}
		if (!navAgent.TryGetTurningRestingPoint(navAgent.RestingDistance(navAgent.currentStoppingDistance), out var pointWS, out var headingWS))
		{
			return false;
		}
		footprint = LivestockFootprint.Of((BaseEntity)this, pointWS, headingWS);
		return true;
	}

	public LivestockFootprint RestingFootprintFor(Vector3 destination, float stoppingDistance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		RestingPoseFor(destination, stoppingDistance, out var position, out var forward);
		return LivestockFootprint.Of((BaseEntity)this, position, forward);
	}

	private void RestingPoseFor(Vector3 destination, float stoppingDistance, out Vector3 position, out Vector3 forward)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		RustNavMeshAgent navAgent = NavAgent;
		Vector3 val = Vector3Ex.WithY(destination - ((Component)this).transform.position, 0f);
		float magnitude = val.magnitude;
		float num = (((Object)(object)navAgent != (Object)null) ? navAgent.RestingDistance(stoppingDistance) : stoppingDistance);
		if (magnitude <= num)
		{
			position = ((Component)this).transform.position;
			forward = ((Component)this).transform.forward;
		}
		else
		{
			position = destination - val * (num / magnitude);
			forward = val;
		}
	}

	public bool TryGetTurningFootprintFor(Vector3 destination, float stoppingDistance, bool braking, out LivestockFootprint footprint)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		footprint = default;
		if (!TryGetTurningPoseFor(destination, stoppingDistance, braking, out var rest, out var heading))
		{
			return false;
		}
		footprint = LivestockFootprint.Of((BaseEntity)this, rest, heading);
		return true;
	}

	private bool TryGetTurningPoseFor(Vector3 destination, float stoppingDistance, bool braking, out Vector3 rest, out Vector3 heading)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		rest = default;
		heading = default;
		RustNavMeshAgent navAgent = NavAgent;
		if ((Object)(object)navAgent != (Object)null)
		{
			return navAgent.TryPredictTurningRest(((Component)this).transform.position, ((Component)this).transform.forward, destination, stoppingDistance, braking, out rest, out heading);
		}
		return false;
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

	public float ClearanceWhereItStands()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		PooledList<LivestockFootprint> val = Pool.Get<PooledList<LivestockFootprint>>();
		try
		{
			GatherRestingFootprints(((Component)this).transform.position, val);
			return ClearanceOf(val, LivestockFootprint.Of(this));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public int PickRestingSpot(PooledList<LivestockFootprint> taken, PooledList<Vector3> spots, float stoppingDistance, bool braking = true, bool lyingDown = false)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		using (LivestockProfiler.Sample(LivestockProfiler.Section.Pick))
		{
			int result = -1;
			float num = float.MinValue;
			for (int i = 0; i < ((List<Vector3>)(object)spots).Count; i++)
			{
				RestingPoseFor(((List<Vector3>)(object)spots)[i], stoppingDistance, out var position, out var forward);
				float num2 = ClearanceOf(taken, LivestockFootprint.Of((BaseEntity)this, position, forward));
				if (num2 <= num || (lyingDown && !LiesInsideWallsAt(position, forward)))
				{
					continue;
				}
				if (TryGetTurningPoseFor(((List<Vector3>)(object)spots)[i], stoppingDistance, braking, out var rest, out var heading))
				{
					if (lyingDown && !LiesInsideWallsAt(rest, heading))
					{
						continue;
					}
					num2 = Mathf.Min(num2, ClearanceOf(taken, LivestockFootprint.Of((BaseEntity)this, rest, heading)));
				}
				if (IsClear(num2))
				{
					return i;
				}
				if (num2 > num)
				{
					num = num2;
					result = i;
				}
			}
			return result;
		}
	}

	public static int PickPlacement(PooledList<LivestockFootprint> taken, BaseEntity body, PooledList<Vector3> spots, Vector3 facing)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		int result = -1;
		float num = float.MinValue;
		for (int i = 0; i < ((List<Vector3>)(object)spots).Count; i++)
		{
			float num2 = ClearanceOf(taken, LivestockFootprint.Of(body, ((List<Vector3>)(object)spots)[i], facing));
			if (IsClear(num2))
			{
				return i;
			}
			if (num2 > num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public static void SortNearestFirst(PooledList<Vector3> spots, Vector3 from)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 1; i < ((List<Vector3>)(object)spots).Count; i++)
		{
			Vector3 val = ((List<Vector3>)(object)spots)[i];
			Vector3 val2 = val - from;
			float sqrMagnitude = val2.sqrMagnitude;
			int num;
			for (num = i - 1; num >= 0; num--)
			{
				val2 = ((List<Vector3>)(object)spots)[num] - from;
				if (!(val2.sqrMagnitude > sqrMagnitude))
				{
					break;
				}
				((List<Vector3>)(object)spots)[num + 1] = ((List<Vector3>)(object)spots)[num];
			}
			((List<Vector3>)(object)spots)[num + 1] = val;
		}
	}

	private static void SortLeastTurnFirst(PooledList<Vector3> spots, Vector3 from, Vector3 forward)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		PooledList<float> val = Pool.Get<PooledList<float>>();
		try
		{
			foreach (Vector3 item in (List<Vector3>)(object)spots)
			{
				((List<float>)(object)val).Add(Vector3.Angle(Vector3Ex.WithY(forward, 0f), Vector3Ex.WithY(item - from, 0f)));
			}
			for (int i = 1; i < ((List<Vector3>)(object)spots).Count; i++)
			{
				Vector3 value = ((List<Vector3>)(object)spots)[i];
				float num = ((List<float>)(object)val)[i];
				int num2 = i - 1;
				while (num2 >= 0 && ((List<float>)(object)val)[num2] > num)
				{
					((List<Vector3>)(object)spots)[num2 + 1] = ((List<Vector3>)(object)spots)[num2];
					((List<float>)(object)val)[num2 + 1] = ((List<float>)(object)val)[num2];
					num2--;
				}
				((List<Vector3>)(object)spots)[num2 + 1] = value;
				((List<float>)(object)val)[num2 + 1] = num;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool TryFindRestingSpotNear(Vector3 around, float innerRadius, float outerRadius, out Vector3 spot)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return TryFindRestingSpotNear(around, innerRadius, outerRadius, null, out spot);
	}

	public bool TryFindRestingSpotNear(Vector3 around, float innerRadius, float outerRadius, PooledList<LivestockFootprint> keepOff, out Vector3 spot)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return TryFindSpotNear(around, innerRadius, outerRadius, keepOff, lyingDown: false, out spot);
	}

	public bool TryFindLyingSpotNear(Vector3 around, float innerRadius, float outerRadius, out Vector3 spot)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return TryFindSpotNear(around, innerRadius, outerRadius, null, lyingDown: true, out spot);
	}

	private bool TryFindSpotNear(Vector3 around, float innerRadius, float outerRadius, PooledList<LivestockFootprint> keepOff, bool lyingDown, out Vector3 spot)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		spot = default;
		RustNavMeshAgent navAgent = NavAgent;
		if ((Object)(object)navAgent == (Object)null || outerRadius <= 0f)
		{
			return false;
		}
		PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
		try
		{
			SampleRestingSpots(navAgent, around, innerRadius, outerRadius, val);
			if (lyingDown)
			{
				SortLeastTurnFirst(val, ((Component)this).transform.position, ((Component)this).transform.forward);
			}
			else
			{
				SortNearestFirst(val, ((Component)this).transform.position);
			}
			PooledList<LivestockFootprint> val2 = Pool.Get<PooledList<LivestockFootprint>>();
			try
			{
				GatherRestingFootprints(around, val2);
				if (keepOff != null)
				{
					foreach (LivestockFootprint item in (List<LivestockFootprint>)(object)keepOff)
					{
						((List<LivestockFootprint>)(object)val2).Add(item);
					}
				}
				int num = PickRestingSpot(val2, val, navAgent.stoppingDistance, braking: true, lyingDown);
				if (num < 0)
				{
					return false;
				}
				if (keepOff != null && ClearanceOf(keepOff, RestingFootprintFor(((List<Vector3>)(object)val)[num], navAgent.stoppingDistance)) < 0f)
				{
					return false;
				}
				spot = ((List<Vector3>)(object)val)[num];
				return true;
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

	private static void SampleRestingSpots(RustNavMeshAgent agent, Vector3 around, float innerRadius, float outerRadius, PooledList<Vector3> spots)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		PooledList<NavVector3> val = Pool.Get<PooledList<NavVector3>>();
		try
		{
			bool flag = Eqs.SampleNavigablePositions(agent, agent.WorldToNavSpace(around), (List<NavVector3>)(object)val, outerRadius, innerRadius, 16);
			foreach (NavVector3 item in (List<NavVector3>)(object)val)
			{
				NavHit hitNS;
				if (flag)
				{
					((List<Vector3>)(object)spots).Add(agent.NavToWorldSpace(item));
				}
				else if (agent.SamplePosition(item, out hitNS, outerRadius))
				{
					((List<Vector3>)(object)spots).Add(agent.NavToWorldSpace(hitNS.position));
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool TryFindLonelyRestingSpot(float homeRadius, out Vector3 spot)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		spot = default;
		RustNavMeshAgent navAgent = NavAgent;
		if ((Object)(object)navAgent == (Object)null || SocialSearchRadius <= 0f)
		{
			return false;
		}
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			Query.Server.GetBrainsInSphere(((Component)this).transform.position, SocialSearchRadius, (List<LivestockAnimal>)(object)val);
			PooledList<Vector3> val2 = Pool.Get<PooledList<Vector3>>();
			try
			{
				foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
				{
					if (IsHerdMate(item))
					{
						((List<Vector3>)(object)val2).Add(item.RestingFootprint().Centre);
					}
				}
				if (((List<Vector3>)(object)val2).Count == 0)
				{
					return false;
				}
				PooledList<Vector3> val3 = Pool.Get<PooledList<Vector3>>();
				try
				{
					SampleRestingSpots(navAgent, ((Component)this).transform.position, 4f, 12f, val3);
					Vector3 val4 = HomePosition;
					float num = Mathf.Max(homeRadius, Vector3.Distance(((Component)this).transform.position, val4));
					LivestockFloor livestockFloor = (StandsInAPen ? floor : null);
					for (int num2 = ((List<Vector3>)(object)val3).Count - 1; num2 >= 0; num2--)
					{
						if (Vector3.Distance(((List<Vector3>)(object)val3)[num2], val4) > num || (livestockFloor != null && !livestockFloor.HoldsPosition(((List<Vector3>)(object)val3)[num2])))
						{
							((List<Vector3>)(object)val3).RemoveAt(num2);
						}
					}
					PooledList<float> val5 = Pool.Get<PooledList<float>>();
					try
					{
						foreach (Vector3 item2 in (List<Vector3>)(object)val3)
						{
							((List<float>)(object)val5).Add(DistanceToNearest(RestingFootprintFor(item2, navAgent.stoppingDistance).Centre, val2));
						}
						SortLoneliestFirst(val3, val5);
						PooledList<LivestockFootprint> val6 = Pool.Get<PooledList<LivestockFootprint>>();
						try
						{
							GatherRestingFootprints(((Component)this).transform.position, val6);
							int num3 = PickRestingSpot(val6, val3, navAgent.stoppingDistance, braking: true, lyingDown: true);
							if (num3 < 0 || ((List<float>)(object)val5)[num3] < DistanceToNearest(LivestockFootprint.Of(this).Centre, val2) + 2f)
							{
								return false;
							}
							spot = ((List<Vector3>)(object)val3)[num3];
							return true;
						}
						finally
						{
							((IDisposable)val6)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val5)?.Dispose();
					}
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

	private static void SortLoneliestFirst(PooledList<Vector3> spots, PooledList<float> loneliness)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 1; i < ((List<Vector3>)(object)spots).Count; i++)
		{
			Vector3 value = ((List<Vector3>)(object)spots)[i];
			float num = ((List<float>)(object)loneliness)[i];
			int num2 = i - 1;
			while (num2 >= 0 && ((List<float>)(object)loneliness)[num2] < num)
			{
				((List<Vector3>)(object)spots)[num2 + 1] = ((List<Vector3>)(object)spots)[num2];
				((List<float>)(object)loneliness)[num2 + 1] = ((List<float>)(object)loneliness)[num2];
				num2--;
			}
			((List<Vector3>)(object)spots)[num2 + 1] = value;
			((List<float>)(object)loneliness)[num2 + 1] = num;
		}
	}

	private static float DistanceToNearest(Vector3 point, PooledList<Vector3> others)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		foreach (Vector3 item in (List<Vector3>)(object)others)
		{
			num = Mathf.Min(num, Vector3Ex.MagnitudeXZ(item - point));
		}
		return num;
	}

	public bool IsWalkingAfter(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null && !IsDead())
		{
			if (!IsFollowing(player))
			{
				return (Object)(object)CuriousAbout == (Object)(object)player;
			}
			return true;
		}
		return false;
	}

	public static int CountLedBy(BasePlayer player)
	{
		if (!((Object)(object)player == (Object)null))
		{
			return NeedsQueue.CountLedBy(player);
		}
		return 0;
	}

	public FormationSlot FindFormationSlot(BasePlayer player, float fileWidth)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		FormationSlot result = default;
		if ((Object)(object)player == (Object)null)
		{
			return result;
		}
		Vector3 position = ((Component)player).transform.position;
		Vector3 val = Vector3Ex.WithY(((Component)this).transform.position - position, 0f);
		if (val.sqrMagnitude < 0.0001f)
		{
			return result;
		}
		PooledList<LivestockAnimal> val2 = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			Query.Server.GetBrainsInSphere(position, 20f, (List<LivestockAnimal>)(object)val2);
			PooledList<LivestockAnimal> val3 = Pool.Get<PooledList<LivestockAnimal>>();
			try
			{
				((List<LivestockAnimal>)(object)val3).Add(this);
				Vector3 val4 = val;
				result.Longest = bounds.size.z;
				foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val2)
				{
					if (!((Object)(object)item == (Object)(object)this) && item.IsWalkingAfter(player))
					{
						Vector3 val5 = Vector3Ex.WithY(((Component)item).transform.position - position, 0f);
						if (!(Vector3.Dot(val5, val) <= 0f))
						{
							((List<LivestockAnimal>)(object)val3).Add(item);
							val4 += val5;
							result.Longest = Mathf.Max(result.Longest, item.bounds.size.z);
						}
					}
				}
				if (((List<LivestockAnimal>)(object)val3).Count < 2)
				{
					return result;
				}
				val4 /= (float)((List<LivestockAnimal>)(object)val3).Count;
				result.InFormation = true;
				result.GroupSize = ((List<LivestockAnimal>)(object)val3).Count;
				result.Back = ((val4.sqrMagnitude > 0.01f) ? val4.normalized : val.normalized);
				result.Right = Vector3.Cross(Vector3.up, -result.Back);
				PooledList<int> val6 = Pool.Get<PooledList<int>>();
				try
				{
					PooledList<int> val7 = Pool.Get<PooledList<int>>();
					try
					{
						foreach (LivestockAnimal item2 in (List<LivestockAnimal>)(object)val3)
						{
							((List<int>)(object)val6).Add(BandAlong(item2, position, result.Back));
							((List<int>)(object)val7).Add(BandAlong(item2, position, result.Right));
						}
						int num = RankBehind(val3, val6, 0);
						result.Row = num / 3;
						int num2 = result.Row * 3;
						int num3 = Mathf.Min(3, ((List<LivestockAnimal>)(object)val3).Count - num2);
						int num4 = 0;
						for (int i = 1; i < ((List<LivestockAnimal>)(object)val3).Count; i++)
						{
							int num5 = RankBehind(val3, val6, i);
							if (num5 >= num2 && num5 < num2 + num3 && IsBefore(val3, val7, i, 0))
							{
								num4++;
							}
						}
						result.Across = FileOffset(num4, num3, ((List<LivestockAnimal>)(object)val3).Count > 3) * fileWidth;
						return result;
					}
					finally
					{
						((IDisposable)val7)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val6)?.Dispose();
				}
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

	private static int RankBehind(PooledList<LivestockAnimal> members, PooledList<int> backBands, int index)
	{
		int num = 0;
		for (int i = 0; i < ((List<LivestockAnimal>)(object)members).Count; i++)
		{
			if (i != index && IsBefore(members, backBands, i, index))
			{
				num++;
			}
		}
		return num;
	}

	private static bool IsBefore(PooledList<LivestockAnimal> members, PooledList<int> bands, int one, int other)
	{
		if (((List<int>)(object)bands)[one] == ((List<int>)(object)bands)[other])
		{
			return ((List<LivestockAnimal>)(object)members)[one].net.ID.Value < ((List<LivestockAnimal>)(object)members)[other].net.ID.Value;
		}
		return ((List<int>)(object)bands)[one] < ((List<int>)(object)bands)[other];
	}

	private static int BandAlong(LivestockAnimal animal, Vector3 leader, Vector3 axis)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.FloorToInt(Vector3.Dot(Vector3Ex.WithY(((Component)animal).transform.position - leader, 0f), axis) / 0.1f);
	}

	private static float FileOffset(int file, int inRow, bool behindAFullRow)
	{
		if ((inRow == 2) & behindAFullRow)
		{
			if (file != 0)
			{
				return 1f;
			}
			return -1f;
		}
		return (float)file - (float)(inRow - 1) * 0.5f;
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

	public float PackingTarget(float floorPerHead)
	{
		return Mathf.InverseLerp(Livestock.lethalFloorPerHead, LivestockFloor.BreedingFloorPerHead, floorPerHead);
	}

	private void RemeasureIfChanged()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		RustNavMeshAgent navAgent = NavAgent;
		if (floor == null || (Object)(object)navAgent == (Object)null || floor.IsCurrent(navAgent))
		{
			return;
		}
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			if (SocialSearchRadius > 0f)
			{
				Query.Server.GetBrainsInSphere(((Component)this).transform.position, SocialSearchRadius, (List<LivestockAnimal>)(object)val);
			}
			KeepFloor(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	internal void StandOn(LivestockFloor next)
	{
		if (floor != next)
		{
			LivestockFloor livestockFloor = floor;
			floor = next;
			next?.Join(this);
			livestockFloor?.Leave(this);
		}
	}

	internal void LeaveFloor()
	{
		StandOn(null);
	}

	private void KeepFloor(PooledList<LivestockAnimal> neighbours)
	{
		RustNavMeshAgent navAgent = NavAgent;
		ulong num = ((CanStandOnAFloor && (Object)(object)navAgent != (Object)null) ? navAgent.CurrentPolyRef : 0);
		if (num == 0L)
		{
			StandOn(null);
		}
		else
		{
			if (floor != null && floor.Serves(num, navAgent))
			{
				return;
			}
			ulong num2 = navAgent.WalkablePolyRef();
			if (num2 == 0L)
			{
				StandOn(null);
			}
			else
			{
				if (num2 != num && floor != null && floor.Serves(num2, navAgent))
				{
					return;
				}
				foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)neighbours)
				{
					LivestockFloor livestockFloor = item.floor;
					if (livestockFloor != null && livestockFloor != floor && livestockFloor.Serves(num2, navAgent))
					{
						StandOn(livestockFloor);
						return;
					}
				}
				LivestockFloor livestockFloor2 = LivestockFloor.Flood(navAgent, num2, floor);
				if (livestockFloor2 != floor)
				{
					StandOn(livestockFloor2);
					livestockFloor2?.ClaimStandingAnimals();
				}
			}
		}
	}

	private void TickFloorNeeds(ref TickableNeed need)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			if (SocialSearchRadius > 0f)
			{
				Query.Server.GetBrainsInSphere(((Component)this).transform.position, SocialSearchRadius, (List<LivestockAnimal>)(object)val);
				TrackHerdMates(val);
			}
			KeepFloor(val);
			KeepOnePerHerd(val);
			if (floor != null)
			{
				floor.Refresh();
				floor.ResampleSunIfDue(NavAgent);
			}
			TickGrassAndSun();
			if (!HasPersonalSpace)
			{
				need.NeedValue = 1f;
			}
			else if (!TOD_Sky.Instance.IsNight && !IsAsleep)
			{
				need.NeedValue = RoomTarget;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void TickGrassAndSun()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if (needs.Length <= 4 || IsWild)
		{
			return;
		}
		ref TickableNeed reference = ref needs[3];
		ref TickableNeed reference2 = ref needs[4];
		if (floor != null && floor.Classified)
		{
			reference.NeedValue = Mathf.Clamp01(floor.GrassArea / Mathf.Max(floor.Area * ShelterNeedScale, 0.01f));
			reference2.NeedValue = SunNeedFor(floor.SunShare);
		}
		else if (floor == null || !floor.SunPending)
		{
			Vector3 position = ((Component)this).transform.position;
			reference.NeedValue = ((LivestockGrass.IsGrassGround(position) && LivestockGrass.IsOnOpenGround(position)) ? 1f : 0f);
			if (!TOD_Sky.Instance.IsNight)
			{
				reference2.NeedValue = ((LivestockSunPath.SeesTheSunNow(position) || GrowableEntity.CalculateArtificialLightExposure(position) > 0f) ? 1f : 0f);
			}
		}
	}

	public float SunNeedFor(float share)
	{
		return Mathf.Clamp01(share / (Mathf.Max(0.01f, Livestock.sunFullShare) * ShelterNeedScale));
	}

	public LivestockSpecial SpecialIdentity()
	{
		if (!HasSpecialAuthoredName)
		{
			return null;
		}
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
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
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
			if (!IsPopulationSpawn() || Rng.Value >= Livestock.specialChance)
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
		float num2 = Rng.Value * num;
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
		GiveName(special.Name, FarmableAnimal.NameSource.Special, 0uL);
		FoundLine(LivestockNames.HouseLocale());
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
				if (forcedGene != null && LivestockGenome.IsLive(forcedGene.Gene))
				{
					Genes = LivestockGenome.WithPair(Genes, forcedGene.Gene, forcedGene.First, forcedGene.Second);
				}
			}
		}
		int num = LivestockGenome.RollMarker(Rng);
		int num2 = LivestockGenome.RollMarker(Rng);
		if (num2 == num)
		{
			num2 = (num + 1 + Rng.Range(0, 15)) % 16;
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
		LivestockSpecial livestockSpecial = mother.SpecialIdentity();
		LivestockSpecial livestockSpecial2 = father.SpecialIdentity();
		if (livestockSpecial == null || livestockSpecial2 == null)
		{
			return false;
		}
		LivestockCross livestockCross = instance.CrossFor(livestockSpecial.Name, livestockSpecial2.Name);
		if (livestockCross == null)
		{
			return false;
		}
		GiveName(livestockCross.Name, FarmableAnimal.NameSource.Game, 0uL);
		FoundLine(LivestockNames.HouseLocale());
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
			num += (GeneScale(gene) + CarriedGeneScale(gene)) * 0.5f;
		}
		float num2 = num / (float)LivestockGenome.AllGenes.Length;
		return Mathf.Max(0f, 1f + (num2 - 1f) * Livestock.priceGeneWeight);
	}

	private float ConditionSaleScale()
	{
		float priceConditionFloor = Livestock.priceConditionFloor;
		float num = Mathf.Max(priceConditionFloor + 0.01f, Livestock.priceConditionFull);
		return Mathf.Lerp(Mathf.Clamp01(Livestock.priceWorstScale), 1f, Mathf.InverseLerp(priceConditionFloor, num, Condition));
	}

	private void SaveSaleValue(LivestockAnimal msg)
	{
		msg.saleValueScale = SaleValueScale;
	}

	public float StartingValueFor(Need need)
	{
		switch (need)
		{
		case Need.Fullness:
			return Mathf.Clamp01(StartingFullness);
		case Need.Hydration:
			return Mathf.Clamp01(StartingHydration);
		case Need.Grass:
		case Need.Sun:
			return 1f;
		default:
			return 1f;
		}
	}

	public float DecayRatePerSecond(Need need)
	{
		switch (need)
		{
		case Need.Fullness:
			return Mathf.Max(0f, FullnessDecayRate);
		case Need.Hydration:
			return Mathf.Max(0f, HydrationDecayRate);
		case Need.Grass:
		case Need.Sun:
			return 0f;
		default:
			return 0f;
		}
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

	public bool IsAtTrough()
	{
		return HasFlag(Flags.Reserved18);
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

	public bool WasRecentlyPetted()
	{
		return HasFlag(Flags.Reserved19);
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
		needs = new TickableNeed[5]
		{
			new TickableNeed(StartingValueFor(Need.Fullness)),
			new TickableNeed(StartingValueFor(Need.PersonalSpace)),
			new TickableNeed(StartingValueFor(Need.Hydration)),
			new TickableNeed(StartingValueFor(Need.Grass)),
			new TickableNeed(StartingValueFor(Need.Sun))
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
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
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
			IsMale = Rng.Range(0f, 1f) > 0.5f;
		}
		ValidateSexForm();
		InitGenes();
		TryGrantSpecial();
		if (!Application.isLoadingSave)
		{
			CoatSeed = (int)Rng.NextSeed();
		}
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

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		ClearPettedFlag();
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
		LeaveFloor();
		ForgetSpecial();
	}

	private void ValidateSexForm()
	{
	}

	public override void Save(SaveInfo info)
	{
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.livestockAnimal = Pool.Get<LivestockAnimal>();
		info.msg.livestockAnimal.coatSeed = CoatSeed;
		if (info.forDisk)
		{
			info.msg.livestockAnimal.saveVersion = 2;
			info.msg.livestockAnimal.timers = Pool.Get<List<PersistentTimer>>();
			timers.Save(info.msg.livestockAnimal.timers, info.cachedTime.Time);
			info.msg.livestockAnimal.ageLived = ageLived;
			info.msg.livestockAnimal.needs = Pool.Get<List<LivestockNeed>>();
			TickableNeed[] array = needs;
			foreach (TickableNeed tickableNeed in array)
			{
				info.msg.livestockAnimal.needs.Add(tickableNeed.ToSavedData());
			}
			info.msg.livestockAnimal.mother = Mother.uid;
			info.msg.livestockAnimal.father = Father.uid;
			info.msg.livestockAnimal.fatherName = (string.IsNullOrEmpty(pregnantPartnerName) ? null : pregnantPartnerName);
			info.msg.livestockAnimal.fatherNameSource = (int)pregnantPartnerNameSource;
			info.msg.livestockAnimal.fatherNameAuthorId = pregnantPartnerNameAuthorId;
			info.msg.livestockAnimal.pregnantPartner = PregnantPartner.uid;
			info.msg.livestockAnimal.pregnantPartnerGenes = PregnantPartnerGenes;
			SaveName(info.msg.livestockAnimal);
			SaveFamiliarity(info.msg.livestockAnimal);
			SaveHome(info.msg.livestockAnimal);
			SaveCondition(info.msg.livestockAnimal);
			SavePotential(info.msg.livestockAnimal);
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		if (isServer && info.fromDisk)
		{
			MoveLegacySyncVarsIntoTheProto(info.msg);
			AddNameSourceToTheSyncVars(info.msg);
		}
		base.Load(info);
		if (info.msg.livestockAnimal != null)
		{
			LoadCoatSeed(info.msg.livestockAnimal.coatSeed);
		}
		if (isServer && info.fromDisk && CoatSeed == 0)
		{
			CoatSeed = CoatSeedOfAnOlderSave();
		}
		if (info.msg.livestockAnimal != null && isServer && info.fromDisk)
		{
			ageLived = info.msg.livestockAnimal.ageLived;
			timers.Load(info.msg.livestockAnimal.timers);
			FoldSavedAdultStage();
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
			pregnantPartnerName = info.msg.livestockAnimal.fatherName;
			pregnantPartnerNameSource = (FarmableAnimal.NameSource)info.msg.livestockAnimal.fatherNameSource;
			pregnantPartnerNameAuthorId = info.msg.livestockAnimal.fatherNameAuthorId;
			PregnantPartner = new EntityRef<LivestockAnimal>(info.msg.livestockAnimal.pregnantPartner);
			PregnantPartnerGenes = info.msg.livestockAnimal.pregnantPartnerGenes;
			LoadName(info.msg.livestockAnimal);
			LoadFamiliarity(info.msg.livestockAnimal);
			LoadHome(info.msg.livestockAnimal);
			LoadCondition(info.msg.livestockAnimal);
			LoadPotential(info.msg.livestockAnimal);
			StartAgeTimerIfIdle();
			RefreshProduction();
		}
		if (info.fromDisk && IsLeading())
		{
			StopLeading();
		}
	}

	private static void MoveLegacySyncVarsIntoTheProto(Entity msg)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		LivestockAnimal livestockAnimal = msg.livestockAnimal;
		byte[] array = msg.baseEntity?.syncVars;
		if (livestockAnimal != null && array != null && livestockAnimal.saveVersion < 1 && array.Length >= 13)
		{
			NetRead netRead = Pool.Get<NetRead>();
			netRead.Init(array);
			netRead.Bool();
			int num = (int)netRead.Position;
			livestockAnimal.pregnantPartner = netRead.EntityID();
			int num2 = (int)netRead.Position;
			netRead.String();
			TakeLegacyId(netRead);
			int num3 = (int)netRead.Position;
			livestockAnimal.homeTc = TakeLegacyId(netRead);
			int num4 = (int)netRead.Position;
			TakeLegacyInt(netRead);
			int num5 = (int)netRead.Position;
			livestockAnimal.pregnantPartnerGenes = TakeLegacyInt(netRead);
			int num6 = (int)netRead.Position;
			Pool.Free<NetRead>(ref netRead);
			byte[] array2 = new byte[array.Length - (num2 - num) - (num4 - num3) - (num6 - num5)];
			int written = AppendRange(array, 0, num, array2, 0);
			written = AppendRange(array, num2, num3, array2, written);
			written = AppendRange(array, num4, num5, array2, written);
			AppendRange(array, num6, array.Length, array2, written);
			msg.baseEntity.syncVars = array2;
		}
	}

	private static NetworkableId TakeLegacyId(NetRead reader)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (reader.Unread < 8)
		{
			return default;
		}
		return reader.EntityID();
	}

	private static int TakeLegacyInt(NetRead reader)
	{
		if (reader.Unread < 4)
		{
			return 0;
		}
		return reader.Int32();
	}

	private static int AppendRange(byte[] from, int start, int end, byte[] to, int written)
	{
		Buffer.BlockCopy(from, start, to, written, end - start);
		return written + end - start;
	}

	private static void AddNameSourceToTheSyncVars(Entity msg)
	{
		LivestockAnimal livestockAnimal = msg.livestockAnimal;
		byte[] array = msg.baseEntity?.syncVars;
		if (livestockAnimal != null && array != null && livestockAnimal.saveVersion < 2 && array.Length >= 5)
		{
			NetRead netRead = Pool.Get<NetRead>();
			netRead.Init(array);
			netRead.Bool();
			int num = (int)netRead.Position;
			string name = netRead.String();
			Pool.Free<NetRead>(ref netRead);
			byte[] array2 = new byte[array.Length + 4];
			int num2 = AppendRange(array, 0, num, array2, 0);
			BitConverter.TryWriteBytes(array2.AsSpan(num2), (int)GetLegacyNameSource(name));
			AppendRange(array, num, array.Length, array2, num2 + 4);
			msg.baseEntity.syncVars = array2;
		}
	}

	private static FarmableAnimal.NameSource GetLegacyNameSource(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return FarmableAnimal.NameSource.Unknown;
		}
		LivestockSpecialTable instance = LivestockSpecialTable.Instance;
		if (!((Object)(object)instance != (Object)null) || instance.Find(name) == null)
		{
			return FarmableAnimal.NameSource.Game;
		}
		return FarmableAnimal.NameSource.Special;
	}

	public void SetLeadSlack(float metres, float distanceToLeader)
	{
		leadSlack = Mathf.Max(Mathf.Max(0f, metres), Mathf.Min(leadSlack, distanceToLeader - 3.5f));
	}

	public static bool HasTooManyOnLeads(BasePlayer player)
	{
		if (CountLedBy(player) < 9)
		{
			return false;
		}
		if (player.IsConnected)
		{
			player.ShowToast(GameTip.Styles.Error, LeadingTooManyPhrase, false);
		}
		return true;
	}

	public bool CanLead(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null && !player.IsDead() && !IsDead() && !IsMounted)
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
		if ((Object)(object)player == (Object)null || player.IsDead() || InSafeZone())
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
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (!InSafeZone())
		{
			lastHerdDistress = TimeSince.op_Implicit(0f);
			herdDistressAttacker.Set(player);
			lastProvoked = TimeSince.op_Implicit(0f);
			provokedBy.Set(player);
			ClearSpook();
		}
	}

	public bool IsProvokedBy(BaseEntity entity)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)entity != (Object)null && TimeSince.op_Implicit(lastProvoked) < 10f)
		{
			return (Object)(object)provokedBy.Get(isServer) == (Object)(object)entity;
		}
		return false;
	}

	public void StopLeading()
	{
		ReleaseLeaderSpeed();
		SetLeading(leading: false);
		LeadingPlayer = default;
		leadSlack = 0f;
		leadTrusted = false;
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
		if (!((Object)(object)basePlayer == (Object)null) && CanLead(basePlayer) && !HasTooManyOnLeads(basePlayer))
		{
			SetLeading(leading: true);
			LeadingPlayer = RefTo(basePlayer);
			leadTrusted = true;
		}
	}

	private void TickLead()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
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
		if (!leadTrusted && TimeSince.op_Implicit(untrustedLeadHeld) >= 1.5f)
		{
			StopLeading();
			ReactToUntrustedHandling(basePlayer);
			return;
		}
		float num = Vector3.Distance(((Component)basePlayer).transform.position, ((Component)this).transform.position);
		if (Livestock.leadBreakDistance > 0f && num > Livestock.leadBreakDistance + leadSlack)
		{
			StopLeading();
		}
		else
		{
			SlowLeaderWhenTaut(basePlayer, num - leadSlack);
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

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
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
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (!wantsLead)
		{
			if (!CanStopLead(player))
			{
				return false;
			}
			StopLeading();
			return true;
		}
		if (IsFollowing(player))
		{
			return true;
		}
		if (IsDead() || player.IsDead() || IsMounted)
		{
			return false;
		}
		if (IsOwnersLead && (ulong)player.userID != OwnerID)
		{
			if (player.IsConnected)
			{
				player.ShowToast(GameTip.Styles.Error, LeadOwnedPhrase, false, GetNameOrLabelAsViewedBy(player), OwnerNameFor(player));
			}
			return false;
		}
		if (HasTooManyOnLeads(player))
		{
			return false;
		}
		leadTrusted = TrustsToHandle(player);
		untrustedLeadHeld = TimeSince.op_Implicit(0f);
		if (IsLeading() && (Object)(object)LeadingPlayer.Get(isServer) != (Object)(object)player)
		{
			ReleaseLeaderSpeed();
		}
		SetLeading(leading: true);
		LeadingPlayer = RefTo(player);
		return true;
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	private void ServerPet(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (CanPet(msg.player))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved19, b: true);
			}
			if (clearPettedFlagAction == null)
			{
				clearPettedFlagAction = ClearPettedFlag;
			}
			Invoke(clearPettedFlagAction, 5f);
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (((Component)this).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM))
			{
				livestockAnimalFSM.OnPetted();
			}
			if ((Object)(object)PettingGesture != (Object)null)
			{
				player.Server_StartGesture(PettingGesture, BasePlayer.GestureStartSource.ServerAction, bypassOwnershipCheck: true);
			}
		}
	}

	private void ClearPettedFlag()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved19, b: false);
	}

	private bool CanPet(BasePlayer player)
	{
		if (!WasRecentlyPetted() && !IsSleeping() && !IsLeading() && !IsMating() && (Object)(object)player != (Object)null && !player.isMounted && !player.modelState.blocking)
		{
			return !player.InGesture;
		}
		return false;
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

	public virtual bool TryGetYield(out ItemDefinition item, out int amount, out float interval)
	{
		item = null;
		amount = 0;
		interval = 0f;
		return false;
	}

	protected virtual void FillYieldProgress(LivestockAnimal update)
	{
	}

	protected static float ProgressOf(PersistentTimer timer)
	{
		if (!timer.IsRunning)
		{
			return 0f;
		}
		return Mathf.Clamp01(timer.Elapsed / Mathf.Max(0.01f, timer.Duration));
	}

	public virtual void AddCorpseYield(ResourceDispenser dispenser)
	{
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(2uL)]
	private void RequestAnimalStats(RPCMessage msg)
	{
		if ((Object)(object)msg.player == (Object)null)
		{
			return;
		}
		LivestockAnimal val = Pool.Get<LivestockAnimal>();
		try
		{
			FillAnimalStats(val, msg.player);
			ClientRPC(RpcTarget.Player("OnReceivedAnimalStats", msg.player), val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	internal void FillAnimalStats(LivestockAnimal update, BasePlayer viewer)
	{
		update.needs = Pool.Get<List<LivestockNeed>>();
		TickableNeed[] array = needs;
		foreach (TickableNeed tickableNeed in array)
		{
			update.needs.Add(tickableNeed.ToSavedData());
		}
		LivestockFloor.SupplyEstimate supplyEstimate = Floor?.Supply() ?? default(LivestockFloor.SupplyEstimate);
		update.foodMinutes = ((Floor != null && supplyEstimate.FoodMinutes <= 0f) ? (-1f) : supplyEstimate.FoodMinutes);
		update.waterMinutes = supplyEstimate.WaterMinutes;
		if (supplyEstimate.FoodMinutes > 0f && IsLivestockFood(supplyEstimate.FoodItem, out var food))
		{
			update.foodPerHour = TroughCaloriesPerSecond * LivestockFloor.StandingSecondsPerRealSecond * 3600f / food.GetIfType(MetabolismAttribute.Type.Calories);
			update.foodItemId = supplyEstimate.FoodItem.itemid;
		}
		float seconds = TrustOf(viewer);
		update.trustTier = (int)TierFor(seconds);
		update.trustProgress = LadderProgress(seconds);
		update.canRename = CanBeRenamedBy(viewer);
		update.viewerAuthoredName = (((Object)(object)viewer != (Object)null && (ulong)viewer.userID == NameAuthorId) ? AnimalName : null);
		ref ulong ownerId = ref update.ownerId;
		ulong num;
		if (HasOwner)
		{
			num = (((Object)(object)viewer != (Object)null && (ulong)viewer.userID == OwnerID) ? OwnerID : 1);
		}
		else
		{
			num = 0uL;
		}
		ownerId = num;
		update.ownerName = (HasOwner ? OwnerNameFor(viewer) : null);
		update.ageSeconds = AgeSeconds;
		update.fatherName = GetFatherNameAsViewedBy(viewer);
		ref float pregnancyFraction = ref update.pregnancyFraction;
		float num2;
		if (IsCarrying)
		{
			num2 = ProgressOf(pregnancyTimer);
		}
		else
		{
			num2 = (IsPregnant() ? 1f : 0f);
		}
		pregnancyFraction = num2;
		update.pregnancySeconds = pregnancyTimer.Remaining;
		if (ProducesYield && TryGetYield(out var item, out var amount, out var interval))
		{
			update.yieldItemId = item.itemid;
			update.yieldAmount = amount;
			update.yieldInterval = interval;
			FillYieldProgress(update);
		}
		update.milkCharges = MilkCharges;
		update.dungInterval = (DungEnabled ? NextDungInterval() : 0f);
		update.dungFraction = (DungEnabled ? DungBestOfBreed() : 0f);
		update.dungItemId = (DungEnabled ? DungItem.itemid : 0);
		SaveSaleValue(update);
		update.potential = Potential;
		update.potentialTrend = PotentialTrend;
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
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (noise.Intensity >= NpcNoiseIntensity.Medium && !InSafeZone())
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
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!InSafeZone())
		{
			lastLoudNoise = TimeSince.op_Implicit(0f);
		}
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

	private void OnBondThreatened()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		lastBondThreat = TimeSince.op_Implicit(0f);
		bondEverThreatened = true;
		ClearSpook();
	}

	public void OnHerdMateHurt(BaseEntity attacker, LivestockAnimal victim)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!InSafeZone() && CanBeAnsweredFor(attacker, out var combatAttacker))
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
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		TimeSince val = lastNeedsTick;
		lastNeedsTick = TimeSince.op_Implicit(0f);
		if (IsAging)
		{
			ageLived += TimeSince.op_Implicit(val);
		}
		TickCondition(TimeSince.op_Implicit(val));
		TickPotential(TimeSince.op_Implicit(val));
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
					TickFloorNeeds(ref reference);
				}
				else
				{
					reference.NeedValue = Mathf.MoveTowards(reference.NeedValue, 0f, DrainPerSecond((Need)i) * num2 * num);
				}
			}
		}
		if (IsGrazing() && needs.Length != 0)
		{
			ref TickableNeed reference2 = ref needs[0];
			if (reference2.NeedValue < 1f)
			{
				float num3 = Mathf.Min(FullnessFillRate * TimeSince.op_Implicit(val), 1f - reference2.NeedValue);
				float num4 = reference2.NeedValue * GrassShareOfFullness;
				if (IsGrazingFromTrough)
				{
					num3 = TakeTroughFullness(num3);
				}
				else
				{
					num3 = Mathf.Min(num3, GrassRoom);
					num4 += num3;
				}
				reference2.NeedValue += num3;
				if (reference2.NeedValue > 0f)
				{
					grassShareOfFullness = num4 / reference2.NeedValue;
				}
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
		float num = AgeSeconds / 3600f;
		string text = DebugLine(DebugValue.Age, num, 0.1f, $"age {num:0.0}h") + "\n" + DebugLine(DebugValue.Fullness, needValue, 0.01f, $"full {needValue:0.00}") + "\n" + DebugLine(DebugValue.Hydration, needValue2, 0.01f, $"hyd {needValue2:0.00}") + "\n" + DebugLine(DebugValue.PersonalSpace, needValue3, 0.01f, $"room {needValue3:0.00}") + "\n" + DebugLine(DebugValue.Mood, contentment, 0.01f, $"mood {contentment:0.00}") + "\n" + DebugLine(DebugValue.Familiarity, BestFamiliarity(out var _), 1f, GetFamiliarityDebugInfo()) + "\n" + GetHomeDebugInfo();
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
		CountFreeRangeHerd();
		if (!CanBePregnant())
		{
			return false;
		}
		PregnantPartner = RefTo(partner);
		PregnantPartnerGenes = (((Object)(object)partner != (Object)null) ? partner.Genes : 0);
		pregnantPartnerName = (((Object)(object)partner != (Object)null) ? partner.AnimalName : null);
		pregnantPartnerNameSource = (((Object)(object)partner != (Object)null) ? partner.NameAuthorSource : FarmableAnimal.NameSource.Unknown);
		pregnantPartnerNameAuthorId = (((Object)(object)partner != (Object)null) ? partner.NameAuthorId : 0);
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
		pregnantPartnerName = null;
		pregnantPartnerNameSource = FarmableAnimal.NameSource.Unknown;
		pregnantPartnerNameAuthorId = 0uL;
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
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		bool flag = Rng.Range(0f, 1f) > 0.5f;
		GameObjectRef gameObjectRef = (((Object)(object)Species != (Object)null) ? Species.ChildFor(flag) : null);
		if (gameObjectRef == null || !gameObjectRef.isValid)
		{
			Debug.LogWarning((object)(Categorize() + " (" + PrefabName + ") gave birth but its Species asset names no child prefab."));
			return false;
		}
		Quaternion val = Quaternion.Euler(0f, ((Component)this).transform.eulerAngles.y, 0f);
		BaseEntity baseEntity = gameManager.CreateEntity(gameObjectRef.resourcePath, GetBirthPosition(gameObjectRef.GetEntity(), val), val);
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
			}
			livestockAnimal2.Genes = InheritedGenome(partnerGenes);
			livestockAnimal2.TakeHomeFrom(this);
			PassBondsTo(livestockAnimal2);
			livestockAnimal2.RegisterParentage(this, partner);
			if (!livestockAnimal2.TryNameFromCross(this, partner))
			{
				livestockAnimal2.TryInheritName(this, partner);
			}
			if (HasOwner)
			{
				livestockAnimal2.SetOwner(OwnerID, OwnerChangeReason.Birth, NameLocale);
			}
			newbornCalf.Set(livestockAnimal2);
			livestockAnimal2.SendNetworkUpdateImmediate();
		}
		LivestockCensus.RecordBirth(((Component)this).transform.position);
		BuildingPrivlidge cupboard;
		if (HasOwner)
		{
			Facepunch.Rust.Analytics.Azure.OnLivestockBirth("owner", baseEntity, OwnerID);
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

	private Vector3 GetBirthPosition(BaseEntity newborn, Quaternion facing)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		RustNavMeshAgent rustNavMeshAgent = default;
		if (!((Component)this).TryGetComponent<RustNavMeshAgent>(ref rustNavMeshAgent))
		{
			return ((Component)this).transform.position;
		}
		PooledList<NavVector3> val = Pool.Get<PooledList<NavVector3>>();
		try
		{
			float outerRadius = (((Object)(object)newborn != (Object)null) ? (ChildSpawnDistance + newborn.bounds.size.z) : ChildSpawnDistance);
			bool flag = Eqs.SampleNavigablePositions(rustNavMeshAgent, rustNavMeshAgent.nextPosition, (List<NavVector3>)(object)val, outerRadius, ChildSpawnDistance, ((Object)(object)newborn != (Object)null) ? 16 : 8);
			PooledList<Vector3> val2 = Pool.Get<PooledList<Vector3>>();
			try
			{
				foreach (NavVector3 item in (List<NavVector3>)(object)val)
				{
					NavHit hitNS;
					if (flag)
					{
						((List<Vector3>)(object)val2).Add(rustNavMeshAgent.NavToWorldSpace(item));
					}
					else if (rustNavMeshAgent.SamplePosition(item, out hitNS, ChildSpawnDistance) && rustNavMeshAgent.CanReach(hitNS.position))
					{
						((List<Vector3>)(object)val2).Add(rustNavMeshAgent.NavToWorldSpace(hitNS.position));
					}
				}
				if (((List<Vector3>)(object)val2).Count == 0)
				{
					return ((Component)this).transform.position;
				}
				if ((Object)(object)newborn == (Object)null)
				{
					return ((List<Vector3>)(object)val2)[0];
				}
				SortNearestFirst(val2, ((Component)this).transform.position);
				PooledList<LivestockFootprint> val3 = Pool.Get<PooledList<LivestockFootprint>>();
				try
				{
					GatherRestingFootprints(((Component)this).transform.position, val3);
					((List<LivestockFootprint>)(object)val3).Add(LivestockFootprint.Of(this));
					return ((List<Vector3>)(object)val2)[PickPlacement(val3, newborn, val2, facing * Vector3.forward)];
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

	public bool NeedsFood()
	{
		return Fullness.NeedValue <= ConsumeThreshold;
	}

	public void FindFoodTroughs(List<BaseEntity> results)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (ConsumeSearchRadius <= 0f)
		{
			return;
		}
		PooledList<HitchTrough> val = Pool.Get<PooledList<HitchTrough>>();
		try
		{
			LivestockFloor currentFloor = CurrentFloor;
			if (currentFloor != null)
			{
				currentFloor.TroughsWithin(((Component)this).transform.position, ConsumeSearchRadius, (List<HitchTrough>)(object)val);
			}
			else
			{
				Query.Server.GetInSphere(((Component)this).transform.position, ConsumeSearchRadius, (List<HitchTrough>)(object)val);
			}
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
		bool wantsDungBoost = WantsDungBoost;
		foreach (Item item in trough.inventory.itemList)
		{
			if (IsTroughFood(item))
			{
				if (wantsDungBoost && IsDungBoostFood(item.info.ItemModConsumable))
				{
					foundItem = item;
					return true;
				}
				if (foundItem == null)
				{
					foundItem = item;
				}
				if (!wantsDungBoost)
				{
					return true;
				}
			}
		}
		return foundItem != null;
	}

	public static float TroughCaloriesIn(HitchTrough trough)
	{
		float num = 0f;
		if ((Object)(object)trough == (Object)null || trough.IsDestroyed || trough.IsDead())
		{
			return num;
		}
		foreach (Item item in trough.inventory.itemList)
		{
			if (IsLivestockFood(item.info, out var food))
			{
				num += (float)item.amount * food.GetIfType(MetabolismAttribute.Type.Calories);
			}
		}
		return num;
	}

	private bool IsTroughFood(Item item)
	{
		if (IsLivestockFood(item.info, out var food))
		{
			return TroughFullnessOf(food) > 0f;
		}
		return false;
	}

	public static bool IsLivestockFood(ItemDefinition item)
	{
		ItemModConsumable food;
		return IsLivestockFood(item, out food);
	}

	private static bool IsLivestockFood(ItemDefinition item, out ItemModConsumable food)
	{
		food = null;
		if ((Object)(object)item != (Object)null && item.category == ItemCategory.Food && ((Component)item).TryGetComponent<ItemModConsumable>(ref food))
		{
			return food.GetIfType(MetabolismAttribute.Type.Calories) > 0f;
		}
		return false;
	}

	public static void MarkSupplyDirtyNear(Vector3 position)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			Query.Server.GetBrainsInSphere(position, 27f, (List<LivestockAnimal>)(object)val);
			foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
			{
				item.Floor?.MarkSupplyDirty();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private float TroughFullnessOf(ItemModConsumable food)
	{
		return food.GetIfType(MetabolismAttribute.Type.Calories) * TroughFullnessPerCalorie;
	}

	public bool ConsumeFromTrough(HitchTrough trough)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (!CanEatFrom(trough, out var foundItem))
		{
			return false;
		}
		ItemModConsumable component = ((Component)foundItem.info).GetComponent<ItemModConsumable>();
		troughFullnessStored += TroughFullnessOf(component);
		troughFullnessFood = component;
		ApplyDungBoost(component);
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
		if (num > 0f)
		{
			RenewDungBoost(troughFullnessFood);
		}
		return num;
	}

	private void OnSyncVar_IsMale(bool? oldValue, bool newValue)
	{
	}

	private void OnSyncVar_AnimalName(string? oldValue, string newValue)
	{
	}

	private void OnSyncVar_LeadingPlayer(EntityRef<BasePlayer>? oldValue, EntityRef<BasePlayer> newValue)
	{
	}

	private void OnSyncVar_Genes(int? oldValue, int newValue)
	{
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
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
				Debug.Log((object)("SyncVar Writing: NameAuthorSourceSync for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_NameAuthorSourceSync);
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
				Debug.Log((object)("SyncVar Writing: Genes for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_Genes);
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
				bool? oldValue2 = __sync_IsMale;
				bool newValue2 = (__sync_IsMale = reader.Bool());
				if (fromAutoSave)
				{
					oldValue2 = null;
				}
				OnSyncVar_IsMale(oldValue2, newValue2);
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_NameAuthorSourceSync;
				int _sync_NameAuthorSourceSync = reader.Int32();
				__sync_NameAuthorSourceSync = _sync_NameAuthorSourceSync;
			}
			catch (Exception ex4)
			{
				Debug.LogException(ex4);
			}
			return true;
		case 2:
			try
			{
				string oldValue4 = __sync_AnimalName;
				string newValue4 = (__sync_AnimalName = reader.String());
				if (fromAutoSave)
				{
					oldValue4 = null;
				}
				OnSyncVar_AnimalName(oldValue4, newValue4);
			}
			catch (Exception ex5)
			{
				Debug.LogException(ex5);
			}
			return true;
		case 3:
			try
			{
				EntityRef<BasePlayer>? oldValue3 = __sync_LeadingPlayer;
				EntityRef<BasePlayer> newValue3 = (__sync_LeadingPlayer = NetworkReadEx.EntityRef<BasePlayer>(reader));
				if (fromAutoSave)
				{
					oldValue3 = null;
				}
				OnSyncVar_LeadingPlayer(oldValue3, newValue3);
			}
			catch (Exception ex3)
			{
				Debug.LogException(ex3);
			}
			return true;
		case 4:
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
			"NameAuthorSourceSync" => (byte)1, 
			"AnimalName" => (byte)2, 
			"LeadingPlayer" => (byte)3, 
			"Genes" => (byte)4, 
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
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
		OnSyncVar(1, reader, fromAutoSave: true);
		OnSyncVar(2, reader, fromAutoSave: true);
		OnSyncVar(3, reader, fromAutoSave: true);
		OnSyncVar(4, reader, fromAutoSave: true);
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
		__sync_NameAuthorSourceSync = 0;
		__sync_AnimalName = null;
		__sync_LeadingPlayer = default;
		__sync_Genes = 0;
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
			_ => base.ShouldInvalidateCache(id), 
		};
	}

	static LivestockAnimal()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected Obj, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected Obj, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected Obj, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected Obj, but got Unknown
	}
}

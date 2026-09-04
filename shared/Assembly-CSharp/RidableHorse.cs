using System;
using System.Collections.Generic;
using ConVar;
using FIMSpace.FProceduralAnimation;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class RidableHorse : BaseVehicle, IInventoryProvider, IDetector, HitchTrough.IHitchable, TriggerHurtNotChild.IHurtTriggerUser, IAnimalRagdollCollisionReceiver, ITowing, IMedicalToolTarget
{
	public enum HorseAvoidanceState
	{
		Normal,
		AvoidingObstacle
	}

	public enum GaitType : byte
	{
		Walk,
		Trot,
		Canter,
		Gallop
	}

	[Serializable]
	public struct Gait
	{
		public GaitType gaitType;

		public float minSpeed;

		public float maxSpeed;

		public float accelerationForce;

		public float brakingForce;

		public float turnSpeed;

		public float staminaReplenishRatio;

		public bool equipmentScalesMaxSpeed;

		public bool breedScalesMaxSpeed;
	}

	[Serializable]
	public struct PurchaseOption
	{
		public ItemDefinition tokenItem;

		public Phrase title;

		public Phrase description;

		public Sprite icon;

		public int order;
	}

	[Header("Breed")]
	public HorseBreed[] breeds;

	public SkinnedMeshRenderer[] bodyRenderers;

	public SkinnedMeshRenderer[] hairRenderers;

	private int currentBreedIndex;

	public HorseBreed currentBreed;

	public FakePhysicsRope leadingRope;

	public FakePhysicsRope leadingRope2;

	[Header("Container")]
	public ItemDefinition onlyAllowedItem;

	public ItemContainer.ContentsType allowedContents;

	[Space]
	public int maxStackSize;

	public int numStorageSlots;

	public int equipmentSlots;

	public string lootPanelName;

	public string storagePanelName;

	public bool needsBuildingPrivilegeToUse;

	public bool isLootable;

	public ItemContainer storageInventory;

	public ItemContainer equipmentInventory;

	public ProtectionProperties riderProtection;

	public ProtectionProperties baseHorseProtection;

	public float equipmentSpeedMod;

	[Header("Dung")]
	public TriggerBase foodTrigger;

	public ItemDefinition dungItem;

	public Transform dungSpawnPoint;

	public float caloriesToDigestPerHour;

	public float dungProducedPerCalorie;

	[NonSerialized]
	public HorseModifiers modifiers;

	[ServerVar]
	[Help("Scale all rideable animal dung production rates by this value. 0 will disable dung production.")]
	public static float dungTimeScale;

	private float nextEatTime;

	private float lastEatTime;

	private float pendingDungCalories;

	private float dungProduction;

	public HitchTrough currentHitch;

	private VehicleTerrainHandler terrainHandler;

	private readonly Dictionary<BaseEntity, float> damageSinceLastTick;

	private float nextCollisionDamageTime;

	private float steerInput;

	private float steerInputDownTime;

	private float throttleInput;

	private bool forwardInputDown;

	private bool backwardInputDown;

	private bool duckInputDown;

	private float doubleTapTime;

	private float lastDuckTapTime;

	private bool duckDoubleTapped;

	private float sprintInputHoldTime;

	private bool sprintInputJustPressed;

	private Vector3 targetUp;

	private Vector3 averagedUp;

	private float groundAngle;

	protected bool onIdealTerrain;

	protected bool onWaterTopology;

	private float nextTerrainCheckTime;

	private float nextAutoAvoidanceCheckTime;

	private float nextGroundNormalCheckTime;

	private TimeSince timeSinceWaterCheck;

	private TimeSince timeSinceDrowningDamage;

	private bool wasSleeping;

	private float lastMovingTime;

	private const float SLEEP_DELAY = 5f;

	private const float SLEEP_SPEED = 0.5f;

	private WaterLevel.WaterInfo lastWaterInfo;

	private float currentWaterFactor;

	private float airTime;

	private float slidingTime;

	private float lastCrashDamage;

	private Vector3 lastPullerPosition;

	private float lastYVelocity;

	public float kmDistance;

	public float tempDistanceTravelled;

	private float lastRoughTerrainTime;

	private bool wasGrounded;

	private bool isSubmerged;

	[HideInInspector]
	[SerializeField]
	private float baseDrag;

	[HideInInspector]
	[SerializeField]
	private float baseAngularDrag;

	private HorseAvoidanceState currentAvoidanceState;

	private int avoidanceSteeringInput;

	private Vector3 avoidanceScanDirection;

	private float nextStandTime;

	private IHorseInputProvider inputProvider;

	private TowingAttachment<RidableHorse> towingAttachment;

	private ITowing towableEntity;

	private float lastRiddenTime;

	private float nextDecayTime;

	[Header("Horse")]
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Transform centreOfMassTransform;

	[SerializeField]
	private RidableHorseAudio horseAudio;

	[SerializeField]
	private RidableHorseAnimation horseAnimation;

	[SerializeField]
	private LegsAnimator serverLegsAnimator;

	[SerializeField]
	private ScaleBySpeed scaleBySpeedWater;

	public WheelCollider wheelCollider;

	public GameObjectRef corpsePrefab;

	[Space]
	public Collider playerServerCollider;

	public Collider playerServerColliderRear;

	public CapsuleCollider clippingMountCheckCollider;

	public Gait[] gaits;

	public GaitType currentGait;

	public float gaitProgressionInterval;

	public float gravity;

	public float waterGravity;

	public float groundAlignmentSpeed;

	public float roadSpeedBonus;

	[Space]
	public float reverseSpeedFactor;

	public float reverseAccelerationForce;

	[Space]
	public float rotationResponsiveness;

	[Tooltip("The factor applied to rotationResponsiveness, based on the current speed ratio (0 = stopped, 1 = full speed)")]
	public AnimationCurve rotationResponsivenessCurve;

	public Transform[] groundSampleOffsets;

	public Vector2 minMaxSlopeAngle;

	public AnimationCurve slopeAngleSpeedFactor;

	[Header("Collision Damage")]
	[Space]
	[SerializeField]
	private GameObjectRef collisionEffect;

	[Tooltip("Ignore low magnitude so e.g. Players running into stationary vehicles doesn't trigger damage or FX")]
	[SerializeField]
	private float minCollisionDamageForce;

	[Tooltip("Cap max magnitude so unusual events can't cause mega damage")]
	[SerializeField]
	private float maxCollisionDamageForce;

	[Tooltip("Adjust this away from 1.0 if collision damage to this vehicle seems too high or low")]
	[SerializeField]
	private float collisionDamageMultiplier;

	[SerializeField]
	private float playerDamageThreshold;

	[SerializeField]
	private float playerRagdollThreshold;

	[SerializeField]
	private float maxAirTimeBeforeRagdoll;

	[Header("Towing")]
	public TriggerTowing towingTrigger;

	public Transform towingPoint;

	public TowingVisuals towingVisuals;

	private NetworkableId towingEntityId;

	public GameObjectRef towingAttachEffect;

	public GameObjectRef towingDetachEffect;

	[SerializeField]
	private float towingAccelerationBoost;

	[SerializeField]
	private float towingMaxSpeedBoost;

	[SerializeField]
	private GaitType maxTowingGait;

	[Header("Stamina")]
	public float currentStamina;

	public float currentMaxStamina;

	public float maxStamina;

	public float staminaCoreLossRatio;

	public float staminaCoreSpeedBonus;

	public float calorieToStaminaRatio;

	public float hydrationToStaminaRatio;

	public float maxStaminaCoreFromWater;

	[Header("Purchase")]
	public List<PurchaseOption> PurchaseOptions;

	[Header("Saddle")]
	public Phrase SwapToSingleTitle;

	public Phrase SwapToSingleDescription;

	public Sprite SwapToSingleIcon;

	public Phrase SwapToDoubleTitle;

	public Phrase SwapToDoubleDescription;

	public Sprite SwapToDoubleIcon;

	[HideInInspector]
	[SerializeField]
	protected bool[] hasItemTokenCache;

	[Space]
	public SoundPlayer standSound;

	public SoundPlayer slidingSound;

	private TimeSince timeSinceSlidingSoundPlayed;

	public ParticleSystemContainer skidDust;

	public GameObjectRef ragdollPrefab;

	[SerializeField]
	[Header("Pulling")]
	private List<ModifierDefintion> pullingPlayerModifiers;

	[Header("Avoidance")]
	public float avoidanceSphereRadius;

	public Vector2 avoidanceDetectionDistance;

	public LayerMask avoidanceObstacleMask;

	[Header("Sliding")]
	public float groundAngleSlideThresholdForced;

	public float groundAngleSlideThreshold;

	public float groundAngleToRecoverFromSlide;

	public float normalVariationSlideThreshold;

	[HideInInspector]
	public float normalVariation;

	[Min(0f)]
	[Header("Healing")]
	public float healingMultiplier;

	[Tooltip("How much stamina to replenish when healing. Value is not final - scaled further if we have a high stamina core.")]
	[Min(0f)]
	public float staminaReplenishAmount;

	public ItemDefinition[] prohibitedMedicalItems;

	public const Flags Flag_ForSale = Flags.Reserved2;

	public const Flags Flag_Hitched = Flags.Reserved3;

	public const Flags Flag_HideHair = Flags.Reserved4;

	public const Flags Flag_WoodArmor = Flags.Reserved8;

	public const Flags Flag_RoadsignArmor = Flags.Reserved6;

	public const Flags Flag_LNYArmor = Flags.Unused23;

	public const Flags Flag_Lead = Flags.Reserved16;

	public const Flags Flag_HasSingleSaddle = Flags.Reserved9;

	public const Flags Flag_HasDoubleSaddle = Flags.Reserved10;

	public const Flags Flag_IsRagdolling = Flags.Reserved12;

	public const Flags Flag_IsSwimming = Flags.Reserved13;

	public const Flags Flag_IsSliding = Flags.Reserved18;

	public const Flags Flag_IsInWater = Flags.Reserved19;

	private static readonly Phrase TowAngleErrorPhrase;

	private NetworkableId playerLeadingId;

	[ServerVar(Saved = true, ClientAdmin = true, Help = "(Generated) When enabled, draws debug visualisations for this system (seismic sensor range sphere, escape capture state, etc.); editor/admin-only")]
	public static bool debug;

	[ServerVar(Saved = true, ClientAdmin = true, Help = "(Generated) When enabled, horses use automatic avoidance steering to navigate around obstacles; saved between sessions; admin configurable")]
	public static bool autoAvoidance;

	[ServerVar(Saved = true, ClientAdmin = true, Default = "1", Help = "(Generated) When enabled, ground angle updates for horse body tilt are throttled to groundAngleUpdateRate seconds; improves performance")]
	public static bool throttledGroundAngleUpdate;

	[ServerVar(Saved = true, ClientAdmin = true, Default = "0.05", Help = "(Generated) Interval in seconds between ground angle recalculation updates for horse body tilting; default 0.05s")]
	public static float groundAngleUpdateRate;

	[ServerVar(Help = "How long before a horse dies unattended")]
	public static float decayMinutes;

	[ServerVar(Help = "Population active on the server, per square km", ShowInAdminUI = true)]
	public static float Population;

	[Header("Bones")]
	public Transform rootBone;

	public Transform[] allBones;

	private static Vector3[] bonesInitialLocalPos;

	public bool isGrounded { get; private set; }

	public bool isStanding { get; private set; }

	public bool isSkidding { get; private set; }

	private VehicleTerrainHandler.Surface OnSurface
	{
		get
		{
			if (terrainHandler == null)
			{
				return VehicleTerrainHandler.Surface.Default;
			}
			return terrainHandler.OnSurface;
		}
	}

	public override float PositionTickRate
	{
		protected get
		{
			return 0.05f;
		}
	}

	public BaseEntity TowEntity => this;

	public Transform TowAnchor => towingPoint;

	public Rigidbody TowBody => rigidBody;

	public bool IsTowingAllowed => !IsTowing;

	public bool HasSingleSaddle => HasFlag(Flags.Reserved9);

	public bool HasDoubleSaddle => HasFlag(Flags.Reserved10);

	public bool HasSaddle
	{
		get
		{
			if (!HasSingleSaddle)
			{
				return HasDoubleSaddle;
			}
			return true;
		}
	}

	public bool IsForSale => HasFlag(Flags.Reserved2);

	public bool IsTowing => HasFlag(Flags.Reserved14);

	public bool IsLeading => HasFlag(Flags.Reserved16);

	public bool IsSwimming => HasFlag(Flags.Reserved13);

	public bool IsSliding => HasFlag(Flags.Reserved18);

	public BasePlayer leadingPlayer { get; private set; }

	public override bool IsNpc => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RidableHorse.OnRpcMessage"))
		{
			if (rpc == 2663053610u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_Claim"));
				}
				using (TimeWarning.New("SERVER_Claim"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2663053610u, "SERVER_Claim", this, player, 3f))
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
							SERVER_Claim(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SERVER_Claim");
					}
				}
				return true;
			}
			if (rpc == 299778156 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_Lead"));
				}
				using (TimeWarning.New("SERVER_Lead"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(299778156u, "SERVER_Lead", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(299778156u, "SERVER_Lead", this, player, 3f))
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
							SERVER_Lead(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SERVER_Lead");
					}
				}
				return true;
			}
			if (rpc == 3442949235u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_OpenLoot"));
				}
				using (TimeWarning.New("SERVER_OpenLoot"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3442949235u, "SERVER_OpenLoot", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SERVER_OpenLoot(rpc2);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in SERVER_OpenLoot");
					}
				}
				return true;
			}
			if (rpc == 3395302925u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_RequestDetach"));
				}
				using (TimeWarning.New("SERVER_RequestDetach"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3395302925u, "SERVER_RequestDetach", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3395302925u, "SERVER_RequestDetach", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3395302925u, "SERVER_RequestDetach", this, player, 3f))
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
							SERVER_RequestDetach(msg4);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in SERVER_RequestDetach");
					}
				}
				return true;
			}
			if (rpc == 294213070 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_RequestSaddleSwap"));
				}
				using (TimeWarning.New("SERVER_RequestSaddleSwap"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(294213070u, "SERVER_RequestSaddleSwap", this, player, 3f))
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
							SERVER_RequestSaddleSwap(msg5);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in SERVER_RequestSaddleSwap");
					}
				}
				return true;
			}
			if (rpc == 3979037781u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_RequestTow"));
				}
				using (TimeWarning.New("SERVER_RequestTow"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3979037781u, "SERVER_RequestTow", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3979037781u, "SERVER_RequestTow", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3979037781u, "SERVER_RequestTow", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg6 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SERVER_RequestTow(msg6);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
						player.Kick("RPC Error in SERVER_RequestTow");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void ApplyBreed(int index)
	{
		if (currentBreedIndex != index)
		{
			if (index >= breeds.Length || index < 0)
			{
				Debug.LogError((object)("ApplyBreed issue! index is " + index + " breed length is : " + breeds.Length));
				return;
			}
			ApplyBreedInternal(breeds[index]);
			currentBreed = breeds[index];
			currentBreedIndex = index;
		}
	}

	protected void ApplyBreedInternal(HorseBreed breed)
	{
		if (base.isServer)
		{
			SetMaxHealth(StartHealth() * breed.maxHealth);
			base.health = MaxHealth();
		}
	}

	public HorseBreed GetBreed()
	{
		if (currentBreedIndex == -1 || currentBreedIndex >= breeds.Length)
		{
			return null;
		}
		return breeds[currentBreedIndex];
	}

	public void SetBreed(int index)
	{
		ApplyBreed(index);
		SendNetworkUpdate();
	}

	private bool ItemIsSaddle(Item item)
	{
		if (item == null)
		{
			return false;
		}
		ItemModAnimalEquipment component = ((Component)item.info).GetComponent<ItemModAnimalEquipment>();
		if ((Object)(object)component == (Object)null)
		{
			return false;
		}
		ItemModAnimalEquipment.SlotType slot = component.slot;
		return slot == ItemModAnimalEquipment.SlotType.Saddle || slot == ItemModAnimalEquipment.SlotType.SaddleDouble;
	}

	private bool CanOpenStorage(BasePlayer player)
	{
		if (!AnyMounted() || PlayerIsMounted(player))
		{
			return true;
		}
		return false;
	}

	public int GetStorageSlotCount()
	{
		return numStorageSlots;
	}

	public void InitContainers()
	{
		if (storageInventory == null)
		{
			CreateStorageInventory(giveUID: true);
		}
		if (equipmentInventory == null)
		{
			CreateEquipmentInventory(giveUID: true);
		}
	}

	private void CreateInventories(bool giveUID)
	{
		CreateStorageInventory(giveUID);
		CreateEquipmentInventory(giveUID);
	}

	private void CreateStorageInventory(bool giveUID)
	{
		Debug.Assert(storageInventory == null, "Double init of inventory!");
		storageInventory = CreateInventory(giveUID, 48);
		storageInventory.canAcceptItem = StorageItemFilter;
	}

	private void CreateEquipmentInventory(bool giveUID)
	{
		Debug.Assert(equipmentInventory == null, "Double init of inventory!");
		equipmentInventory = CreateInventory(giveUID, equipmentSlots);
		equipmentInventory.canAcceptItem = EquipmentItemFilter;
	}

	private ItemContainer CreateInventory(bool giveUID, int slots)
	{
		ItemContainer itemContainer = Pool.Get<ItemContainer>();
		itemContainer.entityOwner = this;
		itemContainer.allowedContents = ((allowedContents == (ItemContainer.ContentsType)0) ? ItemContainer.ContentsType.Generic : allowedContents);
		itemContainer.SetOnlyAllowedItem(onlyAllowedItem);
		itemContainer.maxStackSize = maxStackSize;
		itemContainer.ServerInitialize(null, slots);
		if (giveUID)
		{
			itemContainer.GiveUID();
		}
		itemContainer.onItemAddedRemoved = OnItemAddedOrRemoved;
		itemContainer.onDirty += OnInventoryDirty;
		return itemContainer;
	}

	public bool StorageItemFilter(BasePlayer player, Item item, int targetSlot)
	{
		return true;
	}

	public bool EquipmentItemFilter(BasePlayer player, Item item, int targetSlot)
	{
		if (IsForSale && ItemIsSaddle(item))
		{
			return false;
		}
		ItemModAnimalEquipment component = ((Component)item.info).GetComponent<ItemModAnimalEquipment>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return false;
		}
		if (ItemIsSaddle(item) && HasSaddle)
		{
			return false;
		}
		if (component.slot == ItemModAnimalEquipment.SlotType.Basic)
		{
			return true;
		}
		for (int i = 0; i < equipmentInventory.capacity; i++)
		{
			Item slot = equipmentInventory.GetSlot(i);
			if (slot != null)
			{
				ItemModAnimalEquipment component2 = ((Component)slot.info).GetComponent<ItemModAnimalEquipment>();
				if (!((Object)(object)component2 == (Object)null) && component2.slot == component.slot)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void OnInventoryDirty()
	{
		EquipmentUpdate();
	}

	private void OnItemAddedOrRemoved(Item arg1, bool arg2)
	{
	}

	private void ReleaseInventories()
	{
		Pool.Free<ItemContainer>(ref equipmentInventory);
		Pool.Free<ItemContainer>(ref storageInventory);
	}

	public void EquipmentUpdate()
	{
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local))
		{
			flagsUpdateScope.Set(Flags.Reserved4, b: false);
			flagsUpdateScope.Set(Flags.Reserved8, b: false);
			flagsUpdateScope.Set(Flags.Reserved6, b: false);
			flagsUpdateScope.Set(Flags.Unused23, b: false);
			riderProtection.Clear();
			baseProtection.Clear();
			equipmentSpeedMod = 0f;
			numStorageSlots = 0;
			for (int i = 0; i < equipmentInventory.capacity; i++)
			{
				Item slot = equipmentInventory.GetSlot(i);
				if (slot == null)
				{
					continue;
				}
				ItemModAnimalEquipment component = ((Component)slot.info).GetComponent<ItemModAnimalEquipment>();
				if ((Object)(object)component != (Object)null)
				{
					flagsUpdateScope.Set(component.WearableFlag, b: true);
					if (component.hideHair)
					{
						flagsUpdateScope.Set(Flags.Reserved4, b: true);
					}
					if (Object.op_Implicit((Object)(object)component.riderProtection))
					{
						riderProtection.Add(component.riderProtection, 1f);
					}
					if (Object.op_Implicit((Object)(object)component.animalProtection))
					{
						baseProtection.Add(component.animalProtection, 1f);
					}
					equipmentSpeedMod += component.speedModifier;
					numStorageSlots += component.additionalInventorySlots;
				}
			}
		}
		for (int j = 0; j < storageInventory.capacity; j++)
		{
			if (j >= numStorageSlots)
			{
				Item slot2 = storageInventory.GetSlot(j);
				if (slot2 != null)
				{
					slot2.RemoveFromContainer();
					slot2.Drop(((Component)this).transform.position + Vector3.up + Random.insideUnitSphere * 0.25f, Vector3.zero);
				}
			}
		}
		storageInventory.capacity = numStorageSlots;
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void SERVER_OpenLoot(RPCMessage rpc)
	{
		if (storageInventory == null)
		{
			return;
		}
		BasePlayer player = rpc.player;
		string text = rpc.read.String();
		if (!((Object)(object)player == (Object)null) && player.CanInteract() && CanOpenStorage(player) && (!needsBuildingPrivilegeToUse || player.CanBuild()) && Interface.CallHook("CanLootEntity", player, this) == null && player.inventory.loot.StartLootingEntity(this))
		{
			ItemContainer container = equipmentInventory;
			string arg = lootPanelName;
			if (text == "storage")
			{
				arg = storagePanelName;
				container = storageInventory;
			}
			player.inventory.loot.AddContainer(container);
			player.inventory.loot.SendImmediate();
			player.ClientRPC(RpcTarget.Player("RPC_OpenLootPanel", player), arg);
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void SERVER_RequestSaddleSwap(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && !IsForSale && HasSaddle && !AnyMounted())
		{
			int tokenItemID = msg.read.Int32();
			Item purchaseToken = GetPurchaseToken(player, tokenItemID);
			if (purchaseToken != null && ItemIsSaddle(purchaseToken))
			{
				ItemDefinition template = (HasSingleSaddle ? PurchaseOptions[0].tokenItem : PurchaseOptions[1].tokenItem);
				OnClaimedWithToken(purchaseToken);
				purchaseToken.UseItem();
				Item item = ItemManager.Create(template, 1, 0uL, isServerSide: true, 0uL);
				player.GiveItem(item);
				SendNetworkUpdateImmediate();
			}
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void SERVER_Claim(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if ((Object)(object)player == (Object)null || !IsForSale)
		{
			return;
		}
		int tokenItemID = msg.read.Int32();
		Item purchaseToken = GetPurchaseToken(player, tokenItemID);
		if (purchaseToken != null && Interface.CallHook("OnRidableAnimalClaim", this, player, purchaseToken) == null && ItemIsSaddle(purchaseToken))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved2, b: false);
			}
			OnClaimedWithToken(purchaseToken);
			purchaseToken.UseItem();
			Facepunch.Rust.Analytics.Azure.OnVehiclePurchased(msg.player, this);
			AttemptMount(player, doMountChecks: false);
			Interface.CallHook("OnRidableAnimalClaimed", this, player);
		}
	}

	public void OnClaimedWithToken(Item tokenItem)
	{
		int saddleItemSeatCount = GetSaddleItemSeatCount(tokenItem);
		SetSeatCount(saddleItemSeatCount);
	}

	public int GetSaddleItemSeatCount(Item item)
	{
		if (!ItemIsSaddle(item))
		{
			return 0;
		}
		ItemModAnimalEquipment component = ((Component)item.info).GetComponent<ItemModAnimalEquipment>();
		if ((Object)(object)component != (Object)null)
		{
			if (component.slot == ItemModAnimalEquipment.SlotType.Saddle)
			{
				return 1;
			}
			if (component.slot == ItemModAnimalEquipment.SlotType.SaddleDouble)
			{
				return 2;
			}
		}
		return 0;
	}

	public void GetAllInventories(List<ItemContainer> list)
	{
		list.Add(storageInventory);
		list.Add(equipmentInventory);
	}

	public Item GetPurchaseToken(BasePlayer player, int tokenItemID)
	{
		return player.inventory.FindItemByItemID(tokenItemID);
	}

	public bool PlayerHasToken(BasePlayer player, int tokenItemID)
	{
		return GetPurchaseToken(player, tokenItemID) != null;
	}

	public void SaveContainer(SaveInfo info, Horse msgHorse)
	{
		if (info.forDisk)
		{
			if (storageInventory != null)
			{
				msgHorse.storageContainer = storageInventory.Save();
			}
			if (equipmentInventory != null)
			{
				msgHorse.equipmentContainer = equipmentInventory.Save();
			}
		}
	}

	public void LoadContainer(LoadInfo info)
	{
		if (info.fromDisk && info.msg.horse != null)
		{
			if (equipmentInventory != null && info.msg.horse.equipmentContainer != null)
			{
				equipmentInventory.Load(info.msg.horse.equipmentContainer);
				equipmentInventory.capacity = equipmentSlots;
			}
			else
			{
				Debug.LogWarning((object)("Horse didn't have saved equipment inventory: " + ((object)this).ToString()));
			}
			if (storageInventory != null && info.msg.horse.storageContainer != null)
			{
				storageInventory.Load(info.msg.horse.storageContainer);
				storageInventory.capacity = numStorageSlots;
			}
			else
			{
				Debug.LogWarning((object)("Horse didn't have savevd storage inventorry: " + ((object)this).ToString()));
			}
		}
	}

	public bool ShouldTrigger()
	{
		if (IsStopped() && isGrounded && !IsSwimming)
		{
			return !HasFlag(Flags.Reserved19);
		}
		return false;
	}

	public void OnObjects()
	{
	}

	public void OnObjectAdded(GameObject obj, Collider col)
	{
		if (ShouldTrigger() && !base.isClient)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
			if (!((Object)(object)baseEntity == (Object)null) && !baseEntity.isClient && baseEntity is DroppedItem { item: not null } droppedItem && droppedItem.item.info.category == ItemCategory.Food)
			{
				OnFoodDetected(droppedItem);
			}
		}
	}

	public void OnEmpty()
	{
	}

	private void OnFoodDetected(DroppedItem droppedItem)
	{
		Invoke(delegate
		{
			EatDroppedFood(droppedItem);
		}, Random.Range(1f, 2f));
	}

	private void EatDroppedFood(DroppedItem droppedItem)
	{
		if ((Object)(object)droppedItem == (Object)null || !foodTrigger.HasAnyEntityContents || (GetMaxStaminaFraction() >= 1f && base.healthFraction >= 1f))
		{
			return;
		}
		if (Time.time < nextEatTime)
		{
			Invoke(delegate
			{
				OnFoodDetected(droppedItem);
			}, nextEatTime - Time.time);
		}
		else
		{
			if (!foodTrigger.entityContents.Contains(droppedItem))
			{
				return;
			}
			ItemModConsumable component = ((Component)droppedItem.item.info).GetComponent<ItemModConsumable>();
			if ((Object)(object)component == (Object)null)
			{
				return;
			}
			droppedItem.item.UseItem();
			if (droppedItem.item.amount <= 0)
			{
				droppedItem.Kill();
			}
			else
			{
				Invoke(delegate
				{
					EatDroppedFood(droppedItem);
				}, nextEatTime - Time.time);
			}
			nextEatTime = Time.time + Random.Range(2f, 3f) + Mathf.InverseLerp(0.5f, 1f, GetMaxStaminaFraction()) * 4f;
			ReplenishFromFood(component);
		}
	}

	public void ReplenishFromFood(ItemModConsumable consumable)
	{
		if (!((Object)(object)consumable == (Object)null))
		{
			HorseModifiers.AddToHorse(this, consumable.modifiers);
			lastEatTime = Time.time;
			float ifType = consumable.GetIfType(MetabolismAttribute.Type.Calories);
			float ifType2 = consumable.GetIfType(MetabolismAttribute.Type.Hydration);
			float num = consumable.GetIfType(MetabolismAttribute.Type.Health) + consumable.GetIfType(MetabolismAttribute.Type.HealthOverTime);
			ApplyDungCalories(ifType);
			ReplenishStaminaCore(ifType, ifType2);
			Heal(num * 4f);
			ClientRPC(RpcTarget.NetworkGroup("CLIENT_HorseEat"));
		}
	}

	private void UpdateDung(float deltaTime)
	{
		if (pendingDungCalories < 0.01f || (Object)(object)dungItem == (Object)null)
		{
			return;
		}
		deltaTime *= dungTimeScale;
		float num = (((Object)(object)modifiers != (Object)null) ? modifiers.GetValue(Modifier.ModifierType.HorseDungProductionBoost, 1f) : 1f);
		deltaTime *= num;
		if (!(deltaTime < 0.01f))
		{
			float num2 = Mathf.Min(pendingDungCalories * deltaTime, caloriesToDigestPerHour / 3600f * deltaTime) * dungProducedPerCalorie;
			dungProduction += num2;
			pendingDungCalories -= num2;
			if (dungProduction >= 1f)
			{
				DoDung();
			}
		}
	}

	public void ApplyDungCalories(float calories)
	{
		pendingDungCalories += calories;
	}

	private void DoDung()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		dungProduction--;
		if (Interface.CallHook("OnAnimalDungProduce", this) == null)
		{
			Quaternion rotation = Quaternion.Euler(Random.Range(-180f, 180f), Random.Range(-180f, 180f), Random.Range(-180f, 180f));
			Vector3 vVelocity = default(Vector3);
			((Vector3)(ref vVelocity))._002Ector(Random.Range(-0.5f, 0.5f), Random.Range(-1f, -3f), Random.Range(-0.5f, 0.5f));
			Item item = ItemManager.Create(dungItem, 1, 0uL, isServerSide: true, 0uL);
			item.SetItemOwnership(currentBreed.breedName.english, ItemOwnershipPhrases.Pooped);
			item.Drop(dungSpawnPoint.position + Random.insideUnitSphere * 0.1f, vVelocity, rotation);
			Interface.CallHook("OnAnimalDungProduced", this, item);
		}
	}

	public bool IsHitched()
	{
		return (Object)(object)currentHitch != (Object)null;
	}

	public void TryToHitch()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		List<HitchTrough> list = Pool.Get<List<HitchTrough>>();
		Vis.Entities(((Component)this).transform.position, 2.5f, list, 256, (QueryTriggerInteraction)1);
		foreach (HitchTrough item in list)
		{
			if (!item.isClient && !(Vector3.Dot(Vector3Ex.Direction2D(((Component)item).transform.position, ((Component)this).transform.position), ((Component)this).transform.forward) < 0.4f) && item.HasSpace() && item.IsValidHitchPosition(((Component)this).transform.position) && item.AttemptToHitch(this))
			{
				break;
			}
		}
		Pool.FreeUnmanaged<HitchTrough>(ref list);
	}

	public void SetHitch(HitchTrough hitch, HitchTrough.HitchSpot spot)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		currentHitch = hitch;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved3, (Object)(object)currentHitch != (Object)null);
		}
		if ((Object)(object)hitch != (Object)null)
		{
			((Component)this).transform.SetPositionAndRotation(spot.tr.position, spot.tr.rotation);
			DismountAllPlayers();
		}
	}

	private void EatFromHitch()
	{
		if (!IsHitched())
		{
			CancelInvoke(EatFromHitch);
		}
		else
		{
			if (Time.time < nextEatTime || (GetMaxStaminaFraction() >= 1f && base.healthFraction >= 1f))
			{
				return;
			}
			Item foodItem = currentHitch.GetFoodItem();
			if (foodItem != null && foodItem.amount > 0)
			{
				ItemModConsumable component = ((Component)foodItem.info).GetComponent<ItemModConsumable>();
				if (Object.op_Implicit((Object)(object)component))
				{
					float time = component.GetIfType(MetabolismAttribute.Type.Calories) * currentHitch.caloriesToDecaySeconds;
					AddDecayDelay(time);
					ReplenishFromFood(component);
					foodItem.UseItem();
					nextEatTime = Time.time + Random.Range(2f, 3f) + Mathf.InverseLerp(0.5f, 1f, GetMaxStaminaFraction()) * 4f;
				}
			}
		}
	}

	public void TryLeaveHitch()
	{
		if ((Object)(object)currentHitch != (Object)null)
		{
			currentHitch.UnHitch(this);
		}
	}

	public bool IsReversing()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		if (base.isServer)
		{
			val = rigidBody.linearVelocity;
		}
		return Vector3.Dot(val, ((Component)this).transform.forward) < -0.1f;
	}

	private float GetCurrentSpeed()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer)
		{
			Vector3 linearVelocity = rigidBody.linearVelocity;
			return ((Vector3)(ref linearVelocity)).magnitude;
		}
		return 0f;
	}

	public float GetCurrentGaitSpeedFraction()
	{
		return Mathf.Clamp01(GetCurrentSpeed() / GetCurrentMaxSpeed());
	}

	public float GetSpeedFraction()
	{
		return Mathf.Clamp01(GetCurrentSpeed() / GetTopSpeed());
	}

	private float GetTurnSpeed()
	{
		float turnSpeed = GetCurrentGait().turnSpeed;
		if (IsSwimming)
		{
			return turnSpeed * 0.8f;
		}
		if (IsReversing())
		{
			return turnSpeed * 0.7f;
		}
		return turnSpeed;
	}

	private float GetCurrentAcceleration()
	{
		float num = GetCurrentGait().accelerationForce;
		if (IsTowing)
		{
			num *= towingAccelerationBoost;
		}
		return num;
	}

	private float GetCurrentMaxSpeed()
	{
		Gait gait = GetCurrentGait();
		float num = gait.maxSpeed;
		if (onIdealTerrain && gait.gaitType == GaitType.Gallop)
		{
			num += roadSpeedBonus;
		}
		if (gait.equipmentScalesMaxSpeed)
		{
			num += equipmentSpeedMod;
			float num2 = (((Object)(object)modifiers != (Object)null) ? modifiers.GetValue(Modifier.ModifierType.HorseGallopSpeed) : 0f);
			num += num2;
		}
		if (gait.breedScalesMaxSpeed)
		{
			num *= currentBreed.maxSpeed;
		}
		if (IsTowing)
		{
			num *= towingMaxSpeedBoost;
		}
		return num;
	}

	public float GetTopSpeed()
	{
		float num = (gaits[gaits.Length - 1].maxSpeed + equipmentSpeedMod) * currentBreed.maxSpeed;
		float num2 = (((Object)(object)modifiers != (Object)null) ? modifiers.GetValue(Modifier.ModifierType.HorseGallopSpeed) : 0f);
		return num + num2;
	}

	private Gait GetCurrentGait()
	{
		return gaits[(uint)currentGait];
	}

	private bool IsStopped()
	{
		if (!IsSliding)
		{
			return GetSpeedFraction() < 0.05f;
		}
		return false;
	}

	public float GetMaxStaminaFraction()
	{
		return Mathf.InverseLerp(0f, maxStamina, currentMaxStamina);
	}

	public float GetStaminaFraction()
	{
		return Mathf.InverseLerp(0f, maxStamina, currentStamina);
	}

	public bool IsDrowning()
	{
		if (IsSwimming)
		{
			return GetStaminaFraction() < 0.02f;
		}
		return false;
	}

	public bool CanLead(BasePlayer player)
	{
		if (!AnyMounted() && NearMountPoint(player) && DirectlyMountable() && !HasFlag(Flags.Reserved12) && !player.isMounted && !IsLeading && !IsTowing)
		{
			return !IsForSale;
		}
		return false;
	}

	public bool CanStopLead(BasePlayer player)
	{
		if (IsLeading && (Object)(object)leadingPlayer == (Object)(object)player)
		{
			return NearMountPoint(player);
		}
		return false;
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		if (!IsDriver(player))
		{
			return;
		}
		if (inputProvider != null)
		{
			throttleInput = inputProvider.GetMoveInput();
			steerInput = inputProvider.GetSteerInput();
		}
		float num = steerInput;
		if (num == 1f || num == -1f)
		{
			if (steerInputDownTime == 0f)
			{
				steerInputDownTime = Time.time;
			}
		}
		else
		{
			steerInputDownTime = 0f;
		}
		forwardInputDown = throttleInput == 1f;
		backwardInputDown = throttleInput == -1f;
		sprintInputJustPressed = inputState.WasJustPressed(BUTTON.SPRINT);
		bool flag = inputState.IsDown(BUTTON.SPRINT);
		if (sprintInputJustPressed)
		{
			IncrementGait(flag);
		}
		if (inputState.WasJustReleased(BUTTON.SPRINT) && currentGait == GaitType.Gallop)
		{
			RetrogradeGait();
		}
		if (flag)
		{
			if (sprintInputHoldTime == 0f)
			{
				sprintInputHoldTime = Time.time;
			}
			if (Time.time - sprintInputHoldTime >= gaitProgressionInterval && (int)currentGait < 3)
			{
				sprintInputHoldTime = Time.time;
				IncrementGait(sprintHeld: true);
			}
		}
		else
		{
			sprintInputHoldTime = 0f;
		}
		duckInputDown = inputState.IsDown(BUTTON.DUCK);
		if (inputState.WasJustReleased(BUTTON.DUCK))
		{
			float time = Time.time;
			if (time - lastDuckTapTime <= doubleTapTime)
			{
				duckDoubleTapped = true;
			}
			else
			{
				duckDoubleTapped = false;
			}
			lastDuckTapTime = time;
		}
		else if (duckDoubleTapped)
		{
			duckDoubleTapped = false;
		}
	}

	public override void VehicleFixedUpdate()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		if (HasFlag(Flags.Reserved12))
		{
			return;
		}
		base.VehicleFixedUpdate();
		using (TimeWarning.New("RidableHorse.VehicleFixedUpdate"))
		{
			if ((Object)(object)modifiers != (Object)null)
			{
				modifiers.ServerUpdate(this);
				modifiers.SendNetworkUpdateIfDirty(this);
			}
			float fixedDeltaTime = Time.fixedDeltaTime;
			UpdateStamina(fixedDeltaTime);
			UpdateDung(fixedDeltaTime);
			if (IsDrowning() && TimeSince.op_Implicit(timeSinceDrowningDamage) > 2f)
			{
				timeSinceDrowningDamage = TimeSince.op_Implicit(0f);
				Hurt(75f, DamageType.Drowned, null, useProtection: false);
			}
			if (Time.time >= nextCollisionDamageTime)
			{
				nextCollisionDamageTime = Time.time + 0.33f;
				foreach (KeyValuePair<BaseEntity, float> item in damageSinceLastTick)
				{
					DoCollisionDamage(item.Key, item.Value);
				}
				damageSinceLastTick.Clear();
			}
			bool flag = AnyMounted();
			if ((!IsLeading && !HasDoubleSaddle && !flag) || (HasDoubleSaddle && !HasDriver()))
			{
				throttleInput = 0f;
				steerInput = 0f;
			}
			if (IsLeading)
			{
				if ((Object)(object)leadingPlayer == (Object)null || leadingPlayer.IsDead() || leadingPlayer.IsSleeping() || leadingPlayer.IsDestroyed)
				{
					SetLeading(null);
				}
				else
				{
					throttleInput = inputProvider.GetMoveInput();
					steerInput = inputProvider.GetSteerInput();
				}
			}
			if ((flag || IsLeading) && rigidBody.IsSleeping())
			{
				((Collider)wheelCollider).enabled = true;
				rigidBody.WakeUp();
			}
			if (rigidBody.IsSleeping())
			{
				wasSleeping = true;
				((Behaviour)serverLegsAnimator).enabled = false;
				((Collider)wheelCollider).enabled = false;
				return;
			}
			((Collider)wheelCollider).enabled = true;
			((Behaviour)serverLegsAnimator).enabled = true;
			serverLegsAnimator.HipsHeightStepSpeed = Mathf.Lerp(0.7f, 0.05f, Mathf.InverseLerp(0f, 10f, normalVariation));
			if (IsTowing)
			{
				towingAttachment.FixedUpdate();
			}
			if (!wasSleeping && !(GetCurrentSpeed() > 0.5f))
			{
				Vector3 angularVelocity = rigidBody.angularVelocity;
				if (!(Mathf.Abs(((Vector3)(ref angularVelocity)).magnitude) > 0.5f))
				{
					goto IL_02a2;
				}
			}
			lastMovingTime = Time.time;
			goto IL_02a2;
			IL_02a2:
			float num = GetCurrentSpeed() * Time.fixedDeltaTime;
			if (!flag && !IsLeading && !IsTowing && Time.time > lastMovingTime + 5f)
			{
				airTime = 0f;
				wheelCollider.motorTorque = 0f;
				((Behaviour)serverLegsAnimator).enabled = false;
				((Collider)wheelCollider).enabled = false;
				rigidBody.Sleep();
			}
			else
			{
				if (autoAvoidance)
				{
					AutoAvoidObstacles();
				}
				MovementsUpdate();
			}
			wasSleeping = false;
			tempDistanceTravelled += num;
		}
	}

	public override float AntiHackVelocity()
	{
		return GetTopSpeed() * 1.3f;
	}

	private void MovementsUpdate()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ae1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ae7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b11: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b24: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b29: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b31: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b5a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b5f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b3d: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b9c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ba7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b77: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b7c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b80: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b86: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bf0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bf7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bfe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0433: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c5a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c2e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c33: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c49: Unknown result type (might be due to invalid IL or missing references)
		//IL_0870: Unknown result type (might be due to invalid IL or missing references)
		//IL_0875: Unknown result type (might be due to invalid IL or missing references)
		//IL_081e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0763: Unknown result type (might be due to invalid IL or missing references)
		//IL_0768: Unknown result type (might be due to invalid IL or missing references)
		//IL_08dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0908: Unknown result type (might be due to invalid IL or missing references)
		//IL_090d: Unknown result type (might be due to invalid IL or missing references)
		//IL_078c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0791: Unknown result type (might be due to invalid IL or missing references)
		//IL_077e: Unknown result type (might be due to invalid IL or missing references)
		//IL_092d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0932: Unknown result type (might be due to invalid IL or missing references)
		//IL_093f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0945: Unknown result type (might be due to invalid IL or missing references)
		//IL_094a: Unknown result type (might be due to invalid IL or missing references)
		//IL_094f: Unknown result type (might be due to invalid IL or missing references)
		//IL_095d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0962: Unknown result type (might be due to invalid IL or missing references)
		//IL_0967: Unknown result type (might be due to invalid IL or missing references)
		//IL_096f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0974: Unknown result type (might be due to invalid IL or missing references)
		//IL_0979: Unknown result type (might be due to invalid IL or missing references)
		//IL_097d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0984: Unknown result type (might be due to invalid IL or missing references)
		//IL_0989: Unknown result type (might be due to invalid IL or missing references)
		//IL_098b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0608: Unknown result type (might be due to invalid IL or missing references)
		//IL_060a: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_063d: Unknown result type (might be due to invalid IL or missing references)
		//IL_063f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0650: Unknown result type (might be due to invalid IL or missing references)
		//IL_0655: Unknown result type (might be due to invalid IL or missing references)
		//IL_065d: Unknown result type (might be due to invalid IL or missing references)
		//IL_09dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_09df: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a03: Unknown result type (might be due to invalid IL or missing references)
		//IL_071e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0722: Unknown result type (might be due to invalid IL or missing references)
		//IL_0727: Unknown result type (might be due to invalid IL or missing references)
		//IL_0729: Unknown result type (might be due to invalid IL or missing references)
		//IL_072e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0732: Unknown result type (might be due to invalid IL or missing references)
		//IL_073f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0744: Unknown result type (might be due to invalid IL or missing references)
		//IL_074c: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0700: Unknown result type (might be due to invalid IL or missing references)
		//IL_0704: Unknown result type (might be due to invalid IL or missing references)
		//IL_0709: Unknown result type (might be due to invalid IL or missing references)
		//IL_0711: Unknown result type (might be due to invalid IL or missing references)
		if (currentGait == GaitType.Canter && !CanCanter())
		{
			RetrogradeGait();
		}
		else if (currentGait == GaitType.Gallop && !CanGallop())
		{
			RetrogradeGait();
		}
		CheckSpeedForRetrograde();
		UpdateOnTerrain();
		bool isSwimming = IsSwimming;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (onWaterTopology)
		{
			if (TimeSince.op_Implicit(timeSinceWaterCheck) > (isSwimming ? 0.05f : 0.25f))
			{
				OBB val = WorldSpaceBounds();
				Bounds val2 = ((OBB)(ref val)).ToBounds();
				lastWaterInfo = WaterLevel.GetWaterInfo(val2, waves: true, volumes: true, this);
				currentWaterFactor = (lastWaterInfo.isValid ? Mathf.InverseLerp(((Bounds)(ref val2)).min.y, ((Bounds)(ref val2)).max.y, lastWaterInfo.surfaceLevel) : 0f);
				isSubmerged = currentWaterFactor > 0.65f;
				bool flag = (currentWaterFactor > 0.5f && !isGrounded) || isSubmerged;
				if (isSwimming != flag)
				{
					flagsUpdateScope.Set(Flags.Reserved13, flag);
				}
				bool flag2 = currentWaterFactor > 0.32f;
				if (HasFlag(Flags.Reserved19) != flag2)
				{
					flagsUpdateScope.Set(Flags.Reserved19, flag2);
				}
				rigidBody.linearDamping = Mathf.Max(baseDrag, currentWaterFactor * 3f);
				rigidBody.angularDamping = Mathf.Max(baseAngularDrag, currentWaterFactor * 2f);
				timeSinceWaterCheck = TimeSince.op_Implicit(0f);
			}
		}
		else
		{
			if (HasFlag(Flags.Reserved19))
			{
				flagsUpdateScope.Set(Flags.Reserved19, b: false);
			}
			currentWaterFactor = 0f;
		}
		wasGrounded = isGrounded;
		if (throttledGroundAngleUpdate)
		{
			if (Time.time > nextGroundNormalCheckTime)
			{
				UpdateGroundNormal();
				nextGroundNormalCheckTime = Time.time + Random.Range(groundAngleUpdateRate, groundAngleUpdateRate + 0.1f);
			}
		}
		else
		{
			UpdateGroundNormal();
		}
		bool flag3;
		Vector3 force;
		if (isGrounded | isSwimming)
		{
			if (!wasGrounded)
			{
				OnLanded(Mathf.Abs(lastYVelocity));
			}
			airTime = 0f;
			flag3 = IsStopped();
			force = rigidBody.linearVelocity;
			if (((Vector3)(ref force)).magnitude > 0.5f && !isSwimming)
			{
				Vector3 val3 = -((Component)this).transform.up;
				force = rigidBody.linearVelocity;
				Vector3 val4 = val3 * ((Vector3)(ref force)).magnitude * 20f;
				rigidBody.AddForce(val4, (ForceMode)0);
			}
			AlignWithNormal(averagedUp);
			Vector3 forward = ((Component)this).transform.forward;
			Vector3 linearVelocity = rigidBody.linearVelocity;
			if (ShouldSlide())
			{
				SetWheelStiffness(0.1f, 0f);
				flagsUpdateScope.Set(Flags.Reserved18, b: true);
			}
			float currentSpeed = GetCurrentSpeed();
			if (IsSliding)
			{
				if ((groundAngle < groundAngleToRecoverFromSlide && currentSpeed < 7f) || (currentSpeed < 1f && slidingTime > Time.fixedDeltaTime * 10f))
				{
					SetWheelStiffness(1f, 1f);
					flagsUpdateScope.Set(Flags.Reserved18, b: false);
					slidingTime = 0f;
				}
				slidingTime += Time.fixedDeltaTime;
				float num = Vector3.Dot(rigidBody.linearVelocity, -((Component)this).transform.forward);
				if (slidingTime > Time.fixedDeltaTime * 5f && num > 7f)
				{
					if (groundAngle > groundAngleToRecoverFromSlide + 5f)
					{
						force = default(Vector3);
						RagdollAllRiders(force);
					}
					RagdollHorse();
				}
			}
			Vector3 val5 = Vector3.Project(linearVelocity, forward);
			Vector3 val6 = linearVelocity - val5;
			if (((Vector3)(ref val6)).magnitude > 1f)
			{
				Vector3 val7 = -val6 * (isSkidding ? 1f : 3f);
				rigidBody.AddForce(val7 * rigidBody.mass, (ForceMode)0);
			}
			float num2 = Mathf.Clamp01(currentSpeed / GetTopSpeed());
			if (num2 > 0.9f && duckInputDown && !isSkidding && currentWaterFactor < 0.1f && groundAngle < 10f)
			{
				isSkidding = true;
				SetWheelStiffness(0.1f, 0.3f);
				ClientRPC(RpcTarget.NetworkGroup("CLIENT_Skid"));
			}
			if (isSkidding)
			{
				SetWheelStiffness(0f, 0f);
				Brake(1.25f);
				if (num2 <= 0.01f || Vector3.Dot(forward, ((Vector3)(ref linearVelocity)).normalized) < 0.2f)
				{
					isSkidding = false;
				}
			}
			else if (!IsSliding)
			{
				SetWheelStiffness(1f, 1f);
			}
			if (Mathf.Abs(steerInput) > 0f && num2 < 0.3f && !IsReversing() && !backwardInputDown && !forwardInputDown && !isSkidding && !duckInputDown && Time.time - steerInputDownTime > 1f)
			{
				throttleInput = 1f;
			}
			if (!isStanding && !IsSliding && !isSkidding)
			{
				if (duckInputDown && !flag3)
				{
					float multiplier = (((int)currentGait <= 1) ? 0.15f : 1f);
					Brake(multiplier);
				}
				if (throttleInput != 0f && !duckInputDown)
				{
					wheelCollider.motorTorque = 1E-05f * Mathf.Sign(throttleInput);
					if (throttleInput < 0f)
					{
						if ((Vector3.Dot(linearVelocity, forward) < -0.1f) | flag3)
						{
							float num3 = GetCurrentMaxSpeed() * reverseSpeedFactor;
							float num4 = Mathf.Max(num3 - currentSpeed, 0f);
							Vector3 val8 = -forward * (reverseAccelerationForce * (num4 / num3));
							rigidBody.AddForce(val8, (ForceMode)0);
						}
						else
						{
							Brake();
						}
					}
					else if (throttleInput > 0f)
					{
						float currentMaxSpeed = GetCurrentMaxSpeed();
						float currentAcceleration = GetCurrentAcceleration();
						if (((Vector3)(ref linearVelocity)).magnitude < currentMaxSpeed)
						{
							Vector3 val9 = forward * currentAcceleration;
							if (currentGait != GaitType.Walk || IsTowing)
							{
								float num5 = slopeAngleSpeedFactor.Evaluate(Mathf.InverseLerp(minMaxSlopeAngle.x, minMaxSlopeAngle.y, groundAngle));
								if (IsTowing)
								{
									num5++;
								}
								val9 *= num5;
							}
							rigidBody.AddForce(val9, (ForceMode)0);
						}
						else
						{
							Vector3 val10 = forward * currentMaxSpeed - linearVelocity;
							Vector3 val11 = ((Vector3)(ref val10)).normalized * (currentAcceleration * 0.2f);
							rigidBody.AddForce(val11, (ForceMode)0);
						}
					}
				}
				else
				{
					float num6 = 0.1f;
					force = rigidBody.linearVelocity;
					if (((Vector3)(ref force)).sqrMagnitude < num6 * num6)
					{
						rigidBody.linearVelocity = Vector3.zero;
					}
					else
					{
						Vector3 val12 = -((Vector3)(ref linearVelocity)).normalized * (rigidBody.mass * (AnyMounted() ? 1.1f : 3f));
						rigidBody.AddForce(val12, (ForceMode)0);
					}
				}
			}
			if (!isStanding && !IsSliding)
			{
				float num7 = GetTurnSpeed();
				if (duckInputDown && (int)currentGait < 2)
				{
					num7 *= 1.5f;
				}
				float num8 = ((steerInput + (float)avoidanceSteeringInput) * num7 * (MathF.PI / 180f) - rigidBody.angularVelocity.y) * rotationResponsiveness * rotationResponsivenessCurve.Evaluate(num2);
				rigidBody.AddRelativeTorque(0f, num8, 0f, (ForceMode)5);
			}
			if (Mathf.Abs(throttleInput) <= 0f)
			{
				force = rigidBody.linearVelocity;
				if (((Vector3)(ref force)).magnitude < 2.5f && groundAngle < minMaxSlopeAngle.y && !isSwimming && !IsSliding)
				{
					goto IL_08b4;
				}
			}
			if (IsLeading && currentSpeed > 6f)
			{
				goto IL_08b4;
			}
			wheelCollider.brakeTorque = 0f;
			goto IL_08cc;
		}
		airTime += Time.fixedDeltaTime;
		if (airTime > maxAirTimeBeforeRagdoll)
		{
			force = default(Vector3);
			RagdollAllRiders(force);
			RagdollHorse();
		}
		float num9 = ((currentWaterFactor != 0f) ? waterGravity : gravity);
		Vector3 val13 = Vector3.down * (rigidBody.mass * num9);
		rigidBody.AddForce(val13, (ForceMode)0);
		goto IL_0b39;
		IL_08cc:
		if (IsLeading)
		{
			Vector3 val14 = ((Component)this).transform.TransformPoint(Vector3.up * 1.8f + Vector3.forward);
			float num10 = Vector3.Distance(((Component)leadingPlayer).transform.position, val14);
			if (num10 > 3.5f)
			{
				Vector3 position = ((Component)leadingPlayer).transform.position;
				Vector3 val15 = ((Component)leadingPlayer).transform.position - lastPullerPosition;
				lastPullerPosition = ((Component)leadingPlayer).transform.position;
				force = position - ((Component)this).transform.position;
				Vector3 normalized = ((Vector3)(ref force)).normalized;
				Vector3 normalized2 = ((Vector3)(ref val15)).normalized;
				if (Vector3.Dot(normalized, normalized2) > 0.5f)
				{
					float value = Mathf.Lerp(0f, -0.9f, Mathf.Clamp01(Mathf.InverseLerp(3.5f, 7f, num10)));
					leadingPlayer.modifiers.SetValue(Modifier.ModifierSource.Interaction, Modifier.ModifierType.MoveSpeed, value);
					if (num10 > 5f)
					{
						force = position - val14;
						Vector3 normalized3 = ((Vector3)(ref force)).normalized;
						rigidBody.AddForceAtPosition(normalized3 * 5000f, val14, (ForceMode)0);
					}
				}
				else
				{
					leadingPlayer.modifiers.SetValue(Modifier.ModifierSource.Interaction, Modifier.ModifierType.MoveSpeed, 0f);
				}
			}
			else
			{
				leadingPlayer.modifiers.SetValue(Modifier.ModifierSource.Interaction, Modifier.ModifierType.MoveSpeed, 0f);
			}
			if (num10 > 7f)
			{
				leadingPlayer.modifiers.RemoveFromSource(Modifier.ModifierSource.Interaction);
				SetLeading(null);
			}
		}
		if (flag3 && duckDoubleTapped && CanStand())
		{
			ClientRPC(RpcTarget.NetworkGroup("CLIENT_Stand"));
			nextStandTime = Time.time + 4f;
			isStanding = true;
		}
		if (isStanding && nextStandTime < Time.time)
		{
			isStanding = false;
		}
		goto IL_0b39;
		IL_08b4:
		ApplyHandBrake();
		goto IL_08cc;
		IL_0b39:
		if (isSwimming)
		{
			AlignWithNormal(Vector3.up);
			ApplyBuoyancy();
		}
		float num11 = 10000f;
		force = rigidBody.linearVelocity;
		if (((Vector3)(ref force)).magnitude > num11)
		{
			Rigidbody obj = rigidBody;
			force = rigidBody.linearVelocity;
			obj.linearVelocity = ((Vector3)(ref force)).normalized * num11;
		}
		float num12 = -1.3f;
		float num13 = Vector3.Dot(rigidBody.linearVelocity, ((Component)this).transform.forward);
		if (num13 <= num12)
		{
			float num14 = num12 - num13;
			float num15 = Mathf.Lerp(1f, 0.2f, Mathf.InverseLerp(10f, 40f, groundAngle));
			rigidBody.AddForce(((Component)this).transform.forward * num14 * num15, (ForceMode)5);
		}
		if (Mathf.Abs(steerInput) == 0f && !rigidBody.isKinematic)
		{
			force = rigidBody.angularVelocity;
			if (((Vector3)(ref force)).magnitude < 0.2f)
			{
				rigidBody.angularVelocity = Vector3.zero;
			}
		}
		lastYVelocity = rigidBody.linearVelocity.y;
	}

	private void Brake(float multiplier = 1f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero - rigidBody.linearVelocity;
		Vector3 val2 = ((Vector3)(ref val)).normalized * GetCurrentGait().brakingForce * multiplier;
		rigidBody.AddForce(val2, (ForceMode)0);
	}

	private bool ShouldSlide()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if ((IsSliding || !(groundAngle > groundAngleSlideThreshold) || !(normalVariation < normalVariationSlideThreshold)) && !(groundAngle > groundAngleSlideThresholdForced))
		{
			return Vector3.Dot(rigidBody.linearVelocity, -((Component)this).transform.forward) > 4f;
		}
		return true;
	}

	private void ApplyBuoyancy()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		float num = lastWaterInfo.surfaceLevel - (IsDrowning() ? (centreOfMassTransform.position.y + 3f) : (centreOfMassTransform.position.y - 0.5f));
		num = Mathf.Clamp(num, 0f, 5f);
		if (num > 0f)
		{
			float num2 = Mathf.Sin(Time.time * 2f) * 0.3f;
			float num3 = 1f + num2;
			Vector3 val = Vector3.up * (10f * num * num3);
			rigidBody.AddForce(val, (ForceMode)5);
			Vector3 val2 = Vector3.ProjectOnPlane(rigidBody.linearVelocity, Vector3.up);
			Vector3 val3 = Vector3.Project(rigidBody.linearVelocity, Vector3.up);
			val3 *= 1f - Time.fixedDeltaTime * 6f;
			rigidBody.linearVelocity = val2 + val3;
		}
	}

	private void CheckSpeedForRetrograde()
	{
		float currentSpeed = GetCurrentSpeed();
		Gait gait = GetCurrentGait();
		if ((int)currentGait > 0 && currentSpeed < gait.minSpeed)
		{
			RetrogradeGait();
		}
		if (IsSwimming && currentGait != GaitType.Walk)
		{
			RetrogradeGait();
		}
	}

	public void IncrementGait(bool sprintHeld)
	{
		if (!IsTowing || currentGait != maxTowingGait)
		{
			GaitType gaitType = currentGait + 1;
			if ((gaitType != GaitType.Gallop || (sprintHeld && CanStartGalloping())) && (gaitType != GaitType.Trot || !IsSwimming) && (int)currentGait < 3)
			{
				currentGait++;
			}
		}
	}

	private bool CanGallop()
	{
		if (GetStaminaFraction() > 0f)
		{
			return Time.time - lastRoughTerrainTime > 2.5f;
		}
		return false;
	}

	private bool CanCanter()
	{
		return Time.time - lastRoughTerrainTime > 1.5f;
	}

	private bool CanStartGalloping()
	{
		return GetStaminaFraction() > 0.04f;
	}

	public void RetrogradeGait()
	{
		if ((int)currentGait > 0)
		{
			currentGait--;
		}
	}

	private void ApplyHandBrake()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.ProjectOnPlane(Vector3.down, averagedUp);
		rigidBody.AddForce(-val * rigidBody.mass * 10f, (ForceMode)0);
		wheelCollider.brakeTorque = 10000f;
		wheelCollider.motorTorque = 0f;
	}

	private void AlignWithNormal(Vector3 normal, bool force = false)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		Vector3 normalized = ((Vector3)(ref normal)).normalized;
		Vector3 val = Vector3.ProjectOnPlane(((Component)this).transform.forward, normalized);
		Vector3 val2 = ((Vector3)(ref val)).normalized;
		if (val2 == Vector3.zero)
		{
			val2 = ((Component)this).transform.forward;
		}
		Quaternion val3 = Quaternion.LookRotation(val2, Vector3.up);
		Quaternion val4 = (force ? val3 : Quaternion.Slerp(rigidBody.rotation, val3, 5f * Time.fixedDeltaTime));
		rigidBody.MoveRotation(val4);
	}

	public void UpdateGroundNormal()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RidableHorse.UpdateGroundNormal"))
		{
			int num = 0;
			bool flag = false;
			List<Vector3> list = Pool.Get<List<Vector3>>();
			Vector3 val = Vector3.zero;
			Vector3 val2 = averagedUp;
			for (int i = 0; i < groundSampleOffsets.Length; i++)
			{
				Vector3 val3 = groundSampleOffsets[i].position + Vector3.up;
				if (GamePhysics.Trace(new Ray(val3, Vector3.down), 0f, out var hitInfo, 1.2f, 1503731969, (QueryTriggerInteraction)1, this))
				{
					Vector3 normal = ((RaycastHit)(ref hitInfo)).normal;
					num++;
					if (i == groundSampleOffsets.Length - 1)
					{
						flag = true;
					}
					val2 += normal;
					list.Add(normal);
					val += normal;
				}
				else
				{
					val2 += Vector3.up;
					list.Add(Vector3.up);
					val += Vector3.up;
				}
			}
			isGrounded = num >= 2;
			if (!IsSwimming && !flag && throttleInput == 0f)
			{
				throttleInput = 1f;
			}
			Vector3 val4 = val / (float)list.Count;
			Vector3 normalized = ((Vector3)(ref val4)).normalized;
			float num2 = 0f;
			for (int j = 0; j < list.Count; j++)
			{
				float num3 = Vector3.Angle(list[j], normalized);
				num2 += num3;
			}
			normalVariation = num2 / (float)list.Count;
			if (normalVariation > 25f && !onIdealTerrain)
			{
				lastRoughTerrainTime = Time.time;
			}
			val2 += Vector3.up;
			val2 /= (float)(groundSampleOffsets.Length + 1);
			((Vector3)(ref val2)).Normalize();
			if (normalVariation < 10f || Vector3.Dot(targetUp, val2) < 0.99f)
			{
				targetUp = val2;
			}
			averagedUp = Vector3.Lerp(averagedUp, targetUp, Time.fixedDeltaTime * groundAlignmentSpeed);
			groundAngle = Vector3.Angle(normalized, Vector3.up);
			Pool.FreeUnmanaged<Vector3>(ref list);
		}
	}

	private void SetWheelStiffness(float forward, float sideways)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		WheelFrictionCurve forwardFriction = wheelCollider.forwardFriction;
		WheelFrictionCurve sidewaysFriction = wheelCollider.sidewaysFriction;
		((WheelFrictionCurve)(ref forwardFriction)).stiffness = forward;
		((WheelFrictionCurve)(ref sidewaysFriction)).stiffness = sideways;
		wheelCollider.forwardFriction = forwardFriction;
		wheelCollider.sidewaysFriction = sidewaysFriction;
	}

	private void AutoAvoidObstacles()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		if (currentAvoidanceState == HorseAvoidanceState.Normal && GetSpeedFraction() < 0.1f)
		{
			avoidanceSteeringInput = 0;
			return;
		}
		float num = Mathf.Lerp(avoidanceDetectionDistance.x, avoidanceDetectionDistance.y, GetSpeedFraction());
		Vector3 val = rigidBody.linearVelocity;
		Vector3 val2;
		if (!(((Vector3)(ref val)).sqrMagnitude > 0.01f))
		{
			val2 = ((Component)this).transform.forward;
		}
		else
		{
			val = rigidBody.linearVelocity;
			val2 = ((Vector3)(ref val)).normalized;
		}
		Vector3 val3 = ((Component)this).transform.right * steerInput * 0.4f;
		val = val2 + val3 * 0.4f;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		val = Vector3.Lerp(avoidanceScanDirection, normalized, Time.deltaTime * 5f);
		avoidanceScanDirection = ((Vector3)(ref val)).normalized;
		if (currentAvoidanceState == HorseAvoidanceState.Normal && Time.time < nextAutoAvoidanceCheckTime)
		{
			return;
		}
		nextAutoAvoidanceCheckTime = Time.time + Random.Range(0.1f, 0.2f);
		switch (currentAvoidanceState)
		{
		case HorseAvoidanceState.Normal:
		{
			if (DetectObstacleAhead(num, avoidanceScanDirection, out var _))
			{
				currentAvoidanceState = HorseAvoidanceState.AvoidingObstacle;
			}
			avoidanceSteeringInput = 0;
			break;
		}
		case HorseAvoidanceState.AvoidingObstacle:
		{
			int num2 = DetermineAvoidanceDirection(num);
			avoidanceSteeringInput = num2;
			if (!DetectObstacleAhead(num, ((Component)this).transform.forward, out var _))
			{
				if (steerInput != 0f && steerInput != (float)avoidanceSteeringInput)
				{
					currentAvoidanceState = HorseAvoidanceState.Normal;
					avoidanceSteeringInput = 0;
				}
				else
				{
					currentAvoidanceState = HorseAvoidanceState.Normal;
					avoidanceSteeringInput = 0;
				}
			}
			break;
		}
		}
	}

	private bool DetectObstacleAhead(float distance, Vector3 direction, out BaseEntity avoidedEnt)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		avoidedEnt = null;
		RaycastHit val = default(RaycastHit);
		if (Physics.SphereCast(((Component)this).transform.position + ((Component)this).transform.forward + Vector3.up * 1f, avoidanceSphereRadius, direction, ref val, distance, LayerMask.op_Implicit(avoidanceObstacleMask)))
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(((RaycastHit)(ref val)).collider);
			if (baseEntity is TreeEntity)
			{
				avoidedEnt = baseEntity;
				return true;
			}
			if (baseEntity is ResourceEntity)
			{
				Physics.IgnoreCollision((Collider)(object)wheelCollider, ((RaycastHit)(ref val)).collider);
			}
		}
		return false;
	}

	private int DetermineAvoidanceDirection(float detectDistance)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.forward + ((Component)this).transform.right * 0.5f;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		val = ((Component)this).transform.forward - ((Component)this).transform.right * 0.5f;
		Vector3 normalized2 = ((Vector3)(ref val)).normalized;
		float num = CheckSideClearance(normalized, detectDistance);
		float num2 = CheckSideClearance(normalized2, detectDistance);
		if (!(num > num2))
		{
			return -1;
		}
		return 1;
	}

	private float CheckSideClearance(Vector3 direction, float distance)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.SphereCast(((Component)this).transform.position + Vector3.up * 1f, avoidanceSphereRadius, direction, ref val, distance, LayerMask.op_Implicit(avoidanceObstacleMask)))
		{
			if (GameObjectEx.ToBaseEntity(((RaycastHit)(ref val)).collider) is TreeEntity)
			{
				return ((RaycastHit)(ref val)).distance;
			}
			return distance;
		}
		return distance;
	}

	public void OnRagdollCollisionEnter(Collision collision)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		OnLanded(Mathf.Abs(collision.relativeVelocity.y));
	}

	protected void OnCollisionEnter(Collision collision)
	{
		if (base.isServer)
		{
			ProcessCollision(collision, rigidBody);
		}
	}

	protected void ProcessCollision(Collision collision, Rigidbody ourRigidbody)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (!base.isClient && collision != null && !((Object)(object)collision.gameObject == (Object)null) && !((Object)(object)collision.gameObject == (Object)null))
		{
			Vector3 relativeVelocity = collision.relativeVelocity;
			float num = ((Vector3)(ref relativeVelocity)).magnitude * rigidBody.mass;
			float num2 = Vector3.Dot(-((ContactPoint)(ref collision.contacts[0])).normal, ((Component)this).transform.forward);
			float num3 = Mathf.Lerp(0.2f, 1f, Mathf.Clamp01(num2));
			num *= num3;
			if (QueueCollisionDamage(this, num) > 0f)
			{
				TryShowCollisionFX(collision);
			}
		}
	}

	private float QueueCollisionDamage(BaseEntity hitEntity, float forceMagnitude)
	{
		float num = Mathf.InverseLerp(minCollisionDamageForce, maxCollisionDamageForce, forceMagnitude);
		if (num > 0f)
		{
			float num2 = Mathf.Lerp(1f, 200f, num) * collisionDamageMultiplier;
			if (damageSinceLastTick.TryGetValue(hitEntity, out var value))
			{
				if (value < num2)
				{
					damageSinceLastTick[hitEntity] = num2;
				}
			}
			else
			{
				damageSinceLastTick[hitEntity] = num2;
			}
		}
		return num;
	}

	protected virtual void DoCollisionDamage(BaseEntity hitEntity, float damage)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		lastCrashDamage = damage;
		if (damage > playerDamageThreshold)
		{
			float damage2 = (damage - playerDamageThreshold) / 4f;
			DamageAllRiders(damage2);
		}
		if (damage > playerRagdollThreshold)
		{
			Vector3 mountRagdollVelocity = GetMountRagdollVelocity(GetDriver());
			RagdollAllRiders(mountRagdollVelocity);
		}
		Hurt(damage, DamageType.Collision, this, useProtection: false);
		if (damage > playerRagdollThreshold && !IsDead() && !HasFlag(Flags.Reserved12))
		{
			RagdollHorse();
		}
	}

	private void OnLanded(float impactSpeed)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		float num = DamageFromFalling(impactSpeed, checkAirtime: true);
		if (num != 0f)
		{
			Hurt(num, DamageType.Fall, this, useProtection: false);
			TryShowCollisionFX(((Component)this).transform.position);
			if (num > playerDamageThreshold)
			{
				float damage = (num - playerDamageThreshold) / 4f;
				DamageAllRiders(damage);
			}
			if (num > 100f)
			{
				RagdollAllRiders();
			}
		}
	}

	private float DamageFromFalling(float impactSpeed, bool checkAirtime)
	{
		float result = 0f;
		if (impactSpeed > 5f && (!checkAirtime || airTime > 0.4f))
		{
			result = (impactSpeed - 5f) * 10f;
		}
		return result;
	}

	private void DamageAllRiders(float damage)
	{
		BasePlayer driver = GetDriver();
		if ((Object)(object)driver != (Object)null && !driver.IsDead())
		{
			driver.Hurt(damage, DamageType.Collision, this, useProtection: false);
		}
		BasePlayer passenger = GetPassenger();
		if ((Object)(object)passenger != (Object)null && !passenger.IsDead())
		{
			passenger.Hurt(damage, DamageType.Collision, this, useProtection: false);
		}
	}

	private void RagdollAllRiders(Vector3 force = default(Vector3))
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer driver = GetDriver();
		if ((Object)(object)driver != (Object)null && !driver.IsDead())
		{
			driver.Ragdoll(force);
		}
		BasePlayer passenger = GetPassenger();
		if ((Object)(object)passenger != (Object)null && !passenger.IsDead())
		{
			passenger.Ragdoll(force);
		}
	}

	public override GameObjectRef GetCollisionFX()
	{
		return collisionEffect;
	}

	public override Vector3 GetMountRagdollVelocity(BasePlayer player)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp(lastCrashDamage, 0f, 75f);
		return ((Component)this).transform.forward * num * 0.25f;
	}

	public float GetDamageMultiplier(BaseEntity ent)
	{
		return Mathf.Abs(GetSpeed()) * 1f;
	}

	public void OnHurtTriggerOccupant(BaseEntity hurtEntity, DamageType damageType, float damageTotal)
	{
	}

	[ServerVar(Help = "(Generated) Ragdolls the ridable horse entity directly in front of the calling admin player; useful for testing horse physics and death states")]
	public static void Ragdoll(ConsoleSystem.Arg arg)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		List<RidableHorse> list = Pool.Get<List<RidableHorse>>();
		Vis.Entities(basePlayer.eyes.position, basePlayer.eyes.position + basePlayer.eyes.HeadForward() * 5f, 0f, list, -1, (QueryTriggerInteraction)2);
		foreach (RidableHorse item in list)
		{
			item.RagdollHorse();
		}
		Pool.FreeUnmanaged<RidableHorse>(ref list);
	}

	public void RagdollHorse()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!Physics.allowhorsetempragdoll)
		{
			DismountAllPlayers();
		}
		else if (!HasFlag(Flags.Reserved12))
		{
			DismountAllPlayers();
			CreateRagdoll(((Component)this).transform.position, ((Component)this).transform.rotation);
			SetFlagLocal(Flags.Reserved12, b: true);
			SendNetworkUpdateImmediate();
		}
	}

	private void CreateRagdoll(Vector3 position, Quaternion rotation)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		BaseAnimalRagdoll baseAnimalRagdoll = GameManager.server.CreateEntity(ragdollPrefab.resourcePath) as BaseAnimalRagdoll;
		if (baseAnimalRagdoll != null)
		{
			((Component)baseAnimalRagdoll).transform.SetPositionAndRotation(position, rotation);
		}
		Ragdoll ragdoll = ((baseAnimalRagdoll != null) ? ((Component)baseAnimalRagdoll).GetComponent<Ragdoll>() : null);
		if ((Object)(object)ragdoll != (Object)null)
		{
			ragdoll.simOnServer = true;
		}
		baseAnimalRagdoll?.InitFromEnt(this);
		baseAnimalRagdoll?.Spawn();
		GameObjectExtensions.SetIgnoreCollisions(((Component)this).gameObject, ((Component)baseAnimalRagdoll).gameObject, true);
	}

	private void OnRagdollStartServer()
	{
		TowDetach();
		SetLeading(null);
		rigidBody.isKinematic = true;
		((Collider)wheelCollider).enabled = false;
		SetWorldColliders(enabled: false);
		((Behaviour)serverLegsAnimator).enabled = false;
	}

	private void OnRagdollEndServer()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		rigidBody.isKinematic = false;
		((Collider)wheelCollider).enabled = true;
		currentGait = GaitType.Walk;
		UpdateGroundNormal();
		averagedUp = targetUp;
		AlignWithNormal(targetUp, force: true);
		isGrounded = true;
		airTime = 0f;
		lastMovingTime = Time.time;
		SetWorldColliders(enabled: true);
		ResetBonesPositions();
		((Behaviour)serverLegsAnimator).enabled = true;
		damageSinceLastTick.Clear();
		rigidBody.WakeUp();
	}

	private void SetWorldColliders(bool enabled)
	{
		List<Collider> list = Pool.Get<List<Collider>>();
		((Component)this).GetComponentsInChildren<Collider>(list);
		foreach (Collider item in list)
		{
			if (ColliderEx.IsOnLayer(item, (Layer)15))
			{
				item.enabled = enabled;
			}
		}
		Pool.FreeUnmanaged<Collider>(ref list);
	}

	private void UpdateOnTerrain()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (Time.time < nextTerrainCheckTime)
		{
			return;
		}
		nextTerrainCheckTime = Time.time + Random.Range(0.5f, 1f);
		onIdealTerrain = false;
		onWaterTopology = false;
		if ((Object)(object)TerrainMeta.TopologyMap != (Object)null)
		{
			int topology = TerrainMeta.TopologyMap.GetTopology(((Component)this).transform.position);
			if ((topology & 0x80800) != 0)
			{
				onIdealTerrain = true;
			}
			if ((topology & 0x14080) != 0)
			{
				onWaterTopology = true;
			}
		}
	}

	public void UpdateStamina(float delta)
	{
		if (currentGait == GaitType.Gallop)
		{
			UseStamina(delta);
		}
		else if (IsSwimming)
		{
			UseStamina(delta * 0.5f);
		}
		else if (currentStamina != currentMaxStamina)
		{
			ReplenishStamina(GetStaminaReplenishRatio() * delta);
		}
	}

	public void UseStamina(float amount)
	{
		if (onIdealTerrain)
		{
			amount *= 0.5f;
		}
		currentStamina -= amount;
		if (currentStamina <= 0f)
		{
			currentStamina = 0f;
		}
	}

	private float GetStaminaReplenishRatio()
	{
		return GetCurrentGait().staminaReplenishRatio;
	}

	public void ReplenishStamina(float amount)
	{
		float num = 1f + Mathf.InverseLerp(maxStamina * 0.5f, maxStamina, currentMaxStamina);
		amount *= num;
		amount = Mathf.Min(currentMaxStamina - currentStamina, amount);
		float num2 = Mathf.Min(currentMaxStamina - staminaCoreLossRatio * amount, amount * staminaCoreLossRatio);
		currentMaxStamina = Mathf.Clamp(currentMaxStamina - num2, 0f, maxStamina);
		currentStamina = Mathf.Clamp(currentStamina + num2 / staminaCoreLossRatio, 0f, currentMaxStamina);
		if (currentStamina == currentMaxStamina)
		{
			OnStaminaReplenished();
		}
	}

	private void OnStaminaReplenished()
	{
		UpdateClients(force: true);
	}

	public void ReplenishStaminaCore(float calories, float hydration)
	{
		float num = calories * calorieToStaminaRatio;
		float num2 = hydration * hydrationToStaminaRatio;
		num2 = Mathf.Min(maxStaminaCoreFromWater - currentMaxStamina, num2);
		if (num2 < 0f)
		{
			num2 = 0f;
		}
		float num3 = num + num2;
		currentMaxStamina = Mathf.Clamp(currentMaxStamina + num3, 0f, maxStamina);
		currentStamina = Mathf.Clamp(currentStamina + num3, 0f, currentMaxStamina);
	}

	public void SetLeading(BasePlayer target)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		Flags flags = base.flags;
		SetFlagLocal(Flags.Reserved16, (Object)(object)target != (Object)null);
		if ((Object)(object)target == (Object)(object)leadingPlayer)
		{
			if (flags != base.flags)
			{
				SendNetworkUpdate();
			}
			return;
		}
		if ((Object)(object)target != (Object)null)
		{
			playerLeadingId = target.net.ID;
			inputProvider = new AIHorseInputProvider(this, ((Component)target).transform, 3f);
			PlayerModifiers.AddToPlayer(target, pullingPlayerModifiers);
		}
		else
		{
			leadingPlayer.modifiers.RemoveFromSource(Modifier.ModifierSource.Interaction);
			playerLeadingId = default(NetworkableId);
		}
		leadingPlayer = target;
		SendNetworkUpdateImmediate();
		LeadingChanged();
		if ((Object)(object)leadingPlayer == (Object)null)
		{
			ClientRPC(RpcTarget.NetworkGroup("CLIENT_StopLeading"));
		}
		else
		{
			ClientRPC(RpcTarget.NetworkGroup("CLIENT_StartLeading"));
		}
	}

	public void LeadingChanged()
	{
		if (!IsLeading)
		{
			TryToHitch();
		}
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void SERVER_Lead(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if ((Object)(object)player == (Object)null)
		{
			return;
		}
		bool flag = msg.read.Bool();
		if (flag)
		{
			if (!CanLead(player))
			{
				return;
			}
		}
		else if (!CanStopLead(player))
		{
			return;
		}
		if (Interface.CallHook("OnHorseLead", this, player) == null)
		{
			SetLeading(flag ? player : null);
		}
	}

	public bool CanStand()
	{
		if (nextStandTime > Time.time)
		{
			return false;
		}
		if ((Object)(object)mountPoints[0].mountable == (Object)null)
		{
			return false;
		}
		return IsStandCollisionClear();
	}

	public virtual bool IsStandCollisionClear()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		List<Collider> list = Pool.Get<List<Collider>>();
		Vis.Colliders<Collider>(((Component)mountPoints[0].mountable.eyePositionOverride).transform.position - ((Component)this).transform.forward * 1f, 2f, list, 2162689, (QueryTriggerInteraction)2);
		bool num = list.Count > 0;
		Pool.FreeUnmanaged<Collider>(ref list);
		return !num;
	}

	public override float GetNetworkTime()
	{
		return Time.fixedTime;
	}

	public override void ServerInit()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		InitContainers();
		SetBreed(Random.Range(0, breeds.Length));
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved16, b: false);
		}
		baseHorseProtection = baseProtection;
		riderProtection = ScriptableObject.CreateInstance<ProtectionProperties>();
		baseProtection = ScriptableObject.CreateInstance<ProtectionProperties>();
		baseProtection.Add(baseHorseProtection, 1f);
		rigidBody.centerOfMass = centreOfMassTransform.localPosition;
		terrainHandler = new VehicleTerrainHandler(this);
		towingTrigger.OnEntityEnterTrigger = HandleTowTrigger;
		towingTrigger.OnEntityLeaveTrigger = HandleTowTriggerLeave;
		towingAttachment = new TowingAttachment<RidableHorse>(this);
		base.ServerInit();
		InvokeRandomized(UpdateClients, 0f, 0.333f, 0.1f);
		InvokeRandomized(HorseDecay, Random.Range(30f, 60f), 60f, 6f);
		SpawnWildSaddle();
		EquipmentUpdate();
		if ((Object)(object)modifiers != (Object)null)
		{
			modifiers.ServerInit(this);
		}
	}

	public override void PreServerLoad()
	{
		base.PreServerLoad();
		CreateInventories(giveUID: false);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		ReleaseInventories();
		Object.Destroy((Object)(object)riderProtection);
		Object.Destroy((Object)(object)baseProtection);
	}

	private void ServerFlagsChanged(Flags old, Flags next)
	{
		if ((old & Flags.Reserved3) != 0 && (next & Flags.Reserved3) != 0)
		{
			InvokeRepeating(EatFromHitch, Random.Range(1f, 2f), 2f);
		}
		if ((next & Flags.Reserved13) != 0 && (next & Flags.Reserved14) != 0)
		{
			TowDetach();
		}
		bool flag = (old & Flags.Reserved12) != 0;
		bool flag2 = (next & Flags.Reserved12) != 0;
		if (!flag & flag2)
		{
			OnRagdollStartServer();
		}
		else if (flag && !flag2)
		{
			OnRagdollEndServer();
		}
	}

	private void SpawnWildSaddle()
	{
		SetSeatCount(1);
	}

	public void SetForSale()
	{
		SetFlagLocal(Flags.Reserved2, b: true);
		Flags num = flags;
		SetSeatCount(0);
		if (num == flags)
		{
			SendNetworkUpdate();
		}
	}

	protected override bool IsSeatClipping(BaseMountable mountable, Vector3 startPos, float radius, int mask, Vector3 seatPos, Vector3 direction)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		radius *= 0.5f;
		return base.IsSeatClipping(mountable, startPos, radius, mask, seatPos, direction);
	}

	protected override int GetClipCheckMask()
	{
		return base.GetClipCheckMask() & -1073741825;
	}

	private bool IsHorseClipping()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 localToWorldMatrix = ((Component)clippingMountCheckCollider).transform.localToWorldMatrix;
		Vector3 val = ((Matrix4x4)(ref localToWorldMatrix)).MultiplyPoint3x4(clippingMountCheckCollider.center);
		Vector3 val2 = Vector3.zero;
		((Vector3)(ref val2))[clippingMountCheckCollider.direction] = 1f;
		val2 = ((Matrix4x4)(ref localToWorldMatrix)).MultiplyVector(val2);
		float num = clippingMountCheckCollider.radius * 0.9f;
		float num2 = 0.5f * clippingMountCheckCollider.height * 0.9f - num;
		Vector3 point = val + val2 * num2;
		Vector3 point2 = val - val2 * num2;
		List<Collider> list = Pool.Get<List<Collider>>();
		GamePhysics.OverlapCapsule(point, point2, num, list, 1235583233, (QueryTriggerInteraction)1);
		for (int i = 0; i < list.Count; i++)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(list[i]);
			if ((Object)(object)baseEntity != (Object)null && (baseEntity.isClient != base.isClient || (Object)(object)baseEntity == (Object)(object)this))
			{
				ListEx.RemoveUnordered<Collider>(list, i);
				i--;
			}
		}
		bool result = list.Count > 0;
		Pool.FreeUnmanaged<Collider>(ref list);
		return result;
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (IsForSale || !MountEligable(player) || HasFlag(Flags.Reserved12) || IsPlayerTooHeavy(player) || !CanPlayerSeeSaddlePoint(player) || IsHorseClipping())
		{
			return;
		}
		BaseMountable baseMountable = null;
		if (HasSingleSaddle && !player.IsRestrained)
		{
			baseMountable = mountPoints[0].mountable;
		}
		else
		{
			if (!HasDoubleSaddle)
			{
				return;
			}
			baseMountable = ((HasDriver() || player.IsRestrained) ? mountPoints[2].mountable : mountPoints[1].mountable);
		}
		if ((Object)(object)baseMountable != (Object)null)
		{
			baseMountable.AttemptMount(player, doMountChecks);
		}
	}

	public override void PlayerMounted(BasePlayer player, BaseMountable seat)
	{
		base.PlayerMounted(player, seat);
		UpdateClients(force: true);
		TryLeaveHitch();
		if (IsLeading)
		{
			SetLeading(null);
		}
		if (IsDriver(player))
		{
			((Component)playerServerCollider).gameObject.SetActive(true);
			inputProvider = new PlayerHorseInputProvider(player);
		}
		if (IsPassenger(player))
		{
			((Component)playerServerColliderRear).gameObject.SetActive(true);
		}
		InvokeRepeating(SaveTraveledDistance, 10f, 10f);
		InvokeRepeating(PostPlayerLateUpdate, 0f, 0f);
	}

	public override void PlayerDismounted(BasePlayer player, BaseMountable seat)
	{
		base.PlayerDismounted(player, seat);
		UpdateClients(force: true);
		if (NumMounted() == 0)
		{
			TryToHitch();
		}
		if ((Object)(object)GetDriver() == (Object)null)
		{
			((Component)playerServerCollider).gameObject.SetActive(false);
			inputProvider = null;
		}
		if ((Object)(object)GetPassenger() == (Object)null)
		{
			((Component)playerServerColliderRear).gameObject.SetActive(false);
		}
		CancelInvoke(SaveTraveledDistance);
		CancelInvoke(PostPlayerLateUpdate);
		lastRiddenTime = Time.time;
	}

	private void PostPlayerLateUpdate()
	{
		if (!AnyMounted())
		{
			return;
		}
		foreach (MountPointInfo allMountPoint in base.allMountPoints)
		{
			if (!((Object)(object)allMountPoint.mountable == (Object)null) && !((Object)(object)allMountPoint.mountable.GetMounted() == (Object)null))
			{
				allMountPoint.mountable.MountedPlayerSync();
			}
		}
	}

	private void SetSeatCount(int count)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local);
		Flags flags = base.flags;
		flagsUpdateScope.Set(Flags.Reserved9, b: false);
		flagsUpdateScope.Set(Flags.Reserved10, b: false);
		switch (count)
		{
		case 1:
			flagsUpdateScope.Set(Flags.Reserved9, b: true);
			break;
		case 2:
			flagsUpdateScope.Set(Flags.Reserved10, b: true);
			break;
		}
		Flags num = base.flags;
		UpdateMountFlags();
		if (num == base.flags && flags != base.flags)
		{
			SendNetworkUpdate();
		}
	}

	public override bool IsPlayerSeatSwapValid(BasePlayer player, int fromIndex, int toIndex, bool ignoreRestraint)
	{
		if (!base.IsPlayerSeatSwapValid(player, fromIndex, toIndex, ignoreRestraint))
		{
			return false;
		}
		if (!HasSaddle)
		{
			return false;
		}
		if (HasSingleSaddle)
		{
			return false;
		}
		if (HasDoubleSaddle && toIndex == 0)
		{
			return false;
		}
		return true;
	}

	public override int MaxMounted()
	{
		return GetSeatCapacity();
	}

	public int GetSeatCapacity()
	{
		if (HasDoubleSaddle)
		{
			return 2;
		}
		if (HasSingleSaddle)
		{
			return 1;
		}
		return 0;
	}

	public override int NumSwappableSeats()
	{
		return mountPoints.Count;
	}

	public override void OnDied(HitInfo hitInfo)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		BaseCorpse baseCorpse = DropCorpse(corpsePrefab.resourcePath);
		if (Object.op_Implicit((Object)(object)baseCorpse))
		{
			SetupCorpse(baseCorpse);
			baseCorpse.Spawn();
			baseCorpse.TakeChildren(this);
		}
		SaveTraveledDistance();
		TryLeaveHitch();
		TowDetach();
		RagdollAllRiders();
		Invoke(base.KillMessage, 0.5f);
		base.OnDied(hitInfo);
	}

	public override void AdminKill()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		SaveTraveledDistance();
		TryLeaveHitch();
		TowDetach();
		RagdollAllRiders();
		Invoke(base.KillMessage, 0.5f);
		base.AdminKill();
	}

	public virtual void SetupCorpse(BaseCorpse corpse)
	{
		corpse.flags = flags;
		LootableCorpse component = ((Component)corpse).GetComponent<LootableCorpse>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.TakeFrom(this, storageInventory);
		}
		HorseCorpse component2 = ((Component)corpse).GetComponent<HorseCorpse>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			component2.breedIndex = currentBreedIndex;
		}
	}

	private void UpdateClients()
	{
		UpdateClients(force: false);
	}

	private void UpdateClients(bool force)
	{
		if (force || AnyMounted() || IsLeading)
		{
			byte num = (byte)((duckInputDown ? (-1f) : throttleInput) + 1f);
			byte b = (byte)(steerInput + 1f);
			byte b2 = (byte)(avoidanceSteeringInput + 1);
			byte arg = (byte)(num | (b << 2) | (b2 << 4));
			byte arg2 = (byte)Mathf.Clamp(normalVariation / 100f * 255f, 0f, 255f);
			ClientRPC(RpcTarget.NetworkGroup("CLIENT_HorseUpdate"), currentStamina, currentMaxStamina, (byte)currentGait, arg, arg2);
		}
	}

	public override void OnMountedPlayerWeightChanged(BasePlayer player)
	{
		base.OnMountedPlayerWeightChanged(player);
		if (IsPlayerTooHeavy(player))
		{
			player.EnsureDismounted();
		}
	}

	private void SaveTraveledDistance()
	{
		BasePlayer driver = GetDriver();
		if ((Object)(object)driver == (Object)null)
		{
			tempDistanceTravelled = 0f;
			return;
		}
		kmDistance += tempDistanceTravelled / 1000f;
		if (kmDistance >= 1f)
		{
			driver.stats.Add("horse_distance_ridden_km", 1, (Stats)5);
			kmDistance--;
		}
		driver.stats.Add("horse_distance_ridden", Mathf.FloorToInt(tempDistanceTravelled));
		driver.stats.Save();
		tempDistanceTravelled = 0f;
	}

	public override void ScaleDamageForPlayer(BasePlayer player, HitInfo info)
	{
		base.ScaleDamageForPlayer(player, info);
		riderProtection.Scale(info.damageTypes);
	}

	public void OnTowAttach()
	{
		Invoke(delegate
		{
			ComponentExtensions.SetActive<TriggerTowing>(towingTrigger, false);
		}, 0f);
	}

	public void OnTowDetach()
	{
		Invoke(delegate
		{
			ComponentExtensions.SetActive<TriggerTowing>(towingTrigger, true);
		}, 1f);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server]
	public void SERVER_RequestTow(RPCMessage msg)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (towableEntity != null && !IsTowing && !HasFlag(Flags.Reserved12))
		{
			TowAttach(msg.player);
			Effect.server.Run(towingAttachEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.MaxDistance(3f)]
	public void SERVER_RequestDetach(RPCMessage msg)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (IsTowing)
		{
			BasePlayer player = msg.player;
			if (!((Object)(object)player == (Object)null) && (!AnyMounted() || !((Object)(object)player.GetMounted().VehicleParent() != (Object)(object)this)))
			{
				TowDetach();
				Effect.server.Run(towingDetachEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			}
		}
	}

	private void TowAttach(BasePlayer requester = null)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (towableEntity == null || !towableEntity.IsTowingAllowed)
		{
			return;
		}
		if (Vector3.Dot(TowAnchor.forward, ((Component)towableEntity.TowAnchor).transform.forward) <= 0.5f)
		{
			if ((Object)(object)requester != (Object)null)
			{
				requester.ShowToast(GameTip.Styles.Error, TowAngleErrorPhrase, false);
			}
			return;
		}
		towingEntityId = towableEntity.TowEntity.net.ID;
		ClientRPC(RpcTarget.NetworkGroup("CLIENT_SetTowId"), towableEntity.TowEntity.net.ID);
		towingAttachment.AttachTo(towableEntity);
		Invoke(delegate
		{
			ComponentExtensions.SetActive<TriggerTowing>(towingTrigger, false);
		}, 0f);
	}

	private void TowDetach()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (IsTowing)
		{
			towingEntityId = default(NetworkableId);
			((BaseEntity)this).ClientRPC(RpcTarget.NetworkGroup("CLIENT_SetTowId"), default(NetworkableId));
			ClientRPC(RpcTarget.NetworkGroup("CLIENT_CanTow"), arg1: false);
			towableEntity = null;
			towingAttachment.Detach();
		}
	}

	private void ValidateTowableEntity()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (towableEntity != null)
		{
			towingEntityId = towableEntity.TowEntity.net.ID;
		}
		else
		{
			towableEntity = BaseNetworkable.serverEntities.Find(towingEntityId) as ITowing;
		}
	}

	private void HandleTowTrigger(BaseNetworkable networkable)
	{
		if (networkable is ITowing { IsTowing: false } towing)
		{
			towableEntity = towing;
			ClientRPC(RpcTarget.NetworkGroup("CLIENT_CanTow"), arg1: true);
		}
		else
		{
			towableEntity = null;
			ClientRPC(RpcTarget.NetworkGroup("CLIENT_CanTow"), arg1: false);
		}
	}

	private void HandleTowTriggerLeave(BaseNetworkable networkable)
	{
		ClientRPC(RpcTarget.NetworkGroup("CLIENT_CanTow"), arg1: false);
		towableEntity = null;
	}

	public void OnJointBreak(float breakForce)
	{
		TowDetach();
	}

	private void HorseDecay()
	{
		if (base.healthFraction != 0f && !base.IsDestroyed && !(Time.time < lastRiddenTime + 600f) && !(Time.time < lastEatTime + 600f) && !IsForSale && !(Time.time < nextDecayTime))
		{
			float num = 1f / decayMinutes;
			float num2 = ((!IsOutside()) ? 1f : 0.5f);
			Hurt(MaxHealth() * num * num2, DamageType.Decay, this, useProtection: false);
		}
	}

	public override void Hurt(HitInfo info)
	{
		if (!IsForSale)
		{
			base.Hurt(info);
		}
	}

	private void AddDecayDelay(float time)
	{
		if (nextDecayTime < Time.time)
		{
			nextDecayTime = Time.time + 5f;
		}
		nextDecayTime += time;
	}

	[ServerVar(Help = "(Generated) Sets the breed index of the horse directly in front of the calling admin player to the given integer value")]
	public static void SetHorseBreed(ConsoleSystem.Arg arg)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		int breed = arg.GetInt(0);
		List<RidableHorse> list = Pool.Get<List<RidableHorse>>();
		Vis.Entities(basePlayer.eyes.position, basePlayer.eyes.position + basePlayer.eyes.HeadForward() * 5f, 0f, list, -1, (QueryTriggerInteraction)2);
		foreach (RidableHorse item in list)
		{
			item.SetBreed(breed);
		}
		Pool.FreeUnmanaged<RidableHorse>(ref list);
	}

	[ServerVar(Help = "(Generated) Marks the horse directly in front of the calling admin player as for-sale, enabling the purchase interaction")]
	public static void SetForSale(ConsoleSystem.Arg arg)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		List<RidableHorse> list = Pool.Get<List<RidableHorse>>();
		Vis.Entities(basePlayer.eyes.position, basePlayer.eyes.position + basePlayer.eyes.HeadForward() * 5f, 0f, list, -1, (QueryTriggerInteraction)2);
		foreach (RidableHorse item in list)
		{
			item.SetForSale();
		}
		Pool.FreeUnmanaged<RidableHorse>(ref list);
	}

	public override bool AdminFixUp(int tier)
	{
		if (IsDead())
		{
			return false;
		}
		ReplenishStamina(1000f);
		ReplenishStaminaCore(10000f, 10000f);
		return base.AdminFixUp(tier);
	}

	void IMedicalToolTarget.OnMedicalToolApplied(BasePlayer fromPlayer, ItemDefinition itemDef, ItemModConsumable consumable, MedicalTool medicalToolEntity, bool canRevive)
	{
		foreach (ItemModConsumable.ConsumableEffect effect in consumable.effects)
		{
			if (effect.type == MetabolismAttribute.Type.Health)
			{
				Heal(effect.amount * healingMultiplier);
				ReplenishStamina(staminaReplenishAmount);
			}
		}
		modifiers.Add(consumable.modifiers);
	}

	public override void PreInitShared()
	{
		base.PreInitShared();
		modifiers = ((Component)this).GetComponent<HorseModifiers>();
	}

	public bool HasSeatAvailable()
	{
		if (HasSaddle)
		{
			return !HasFlag(Flags.Reserved11);
		}
		return false;
	}

	public bool IsPlayerTooHeavy(BasePlayer player)
	{
		return player.Weight >= 10f;
	}

	public static float UnitsToKPH(float unitsPerSecond)
	{
		return unitsPerSecond * 60f * 60f / 1000f;
	}

	public override void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		base.PreProcess(preProcess, rootObj, name, serverside, clientside, bundling);
		hasItemTokenCache = new bool[PurchaseOptions.Count];
		if (serverside | bundling)
		{
			baseDrag = rigidBody.linearDamping;
			baseAngularDrag = rigidBody.angularDamping;
		}
		bonesInitialLocalPos = (Vector3[])(object)new Vector3[allBones.Length];
		for (int i = 0; i < allBones.Length; i++)
		{
			bonesInitialLocalPos[i] = allBones[i].localPosition;
		}
	}

	public void ResetBonesPositions()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < allBones.Length; i++)
		{
			allBones[i].localPosition = bonesInitialLocalPos[i];
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (old != next && base.isServer)
		{
			ServerFlagsChanged(old, next);
		}
	}

	public override bool AnyMounted()
	{
		return base.AnyMounted();
	}

	private bool CanPlayerSeeSaddlePoint(BasePlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (!GamePhysics.CheckCapsule(player.eyes.position, mountAnchor.position, 0.25f, 2162688, (QueryTriggerInteraction)0))
		{
			return !GamePhysics.CheckCapsule(player.eyes.position, mountAnchor.position + Vector3.up * 0.5f, 0.25f, 2162688, (QueryTriggerInteraction)0);
		}
		return false;
	}

	bool IMedicalToolTarget.IsValidMedicalToolItem(ItemDefinition itemDef)
	{
		for (int i = 0; i < prohibitedMedicalItems.Length; i++)
		{
			if ((Object)(object)prohibitedMedicalItems[i] == (Object)(object)itemDef)
			{
				return false;
			}
		}
		return true;
	}

	public override void Save(SaveInfo info)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.horse = Pool.Get<Horse>();
		SaveContainer(info, info.msg.horse);
		info.msg.horse.stamina = currentStamina;
		info.msg.horse.maxStamina = currentMaxStamina;
		info.msg.horse.towEntityId = towingEntityId;
		info.msg.horse.breedIndex = currentBreedIndex;
		info.msg.horse.numStorageSlots = numStorageSlots;
		if (!info.forDisk)
		{
			info.msg.horse.gait = (int)currentGait;
			info.msg.horse.equipmentSpeedMod = equipmentSpeedMod;
			info.msg.horse.playerLeadingId = playerLeadingId;
		}
		info.msg.horse.modifiers = null;
		if ((Object)(object)modifiers != (Object)null)
		{
			info.msg.horse.modifiers = modifiers.Save(info.forDisk);
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.horse != null)
		{
			LoadContainer(info);
			currentStamina = info.msg.horse.stamina;
			currentMaxStamina = info.msg.horse.maxStamina;
			if (info.fromDisk)
			{
				towingEntityId = info.msg.horse.towEntityId;
				ValidateTowableEntity();
				TowAttach();
			}
			if ((Object)(object)modifiers != (Object)null)
			{
				modifiers.Load(info.msg.horse.modifiers, info.fromDisk);
			}
			ApplyBreed(info.msg.horse.breedIndex);
		}
	}

	public RidableHorse()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		currentBreedIndex = -1;
		allowedContents = ItemContainer.ContentsType.Generic;
		equipmentSlots = 4;
		lootPanelName = "animal";
		storagePanelName = "animal-storage";
		isLootable = true;
		caloriesToDigestPerHour = 100f;
		dungProducedPerCalorie = 0.1f;
		lastEatTime = float.NegativeInfinity;
		damageSinceLastTick = new Dictionary<BaseEntity, float>();
		doubleTapTime = 0.25f;
		lastDuckTapTime = -1f;
		targetUp = Vector3.up;
		averagedUp = Vector3.up;
		gaitProgressionInterval = 1f;
		gravity = 10f;
		waterGravity = 1f;
		groundAlignmentSpeed = 50f;
		roadSpeedBonus = 1f;
		reverseSpeedFactor = 0.5f;
		reverseAccelerationForce = 4000f;
		rotationResponsiveness = 1f;
		minMaxSlopeAngle = new Vector2(10f, 60f);
		minCollisionDamageForce = 20000f;
		maxCollisionDamageForce = 2500000f;
		collisionDamageMultiplier = 1f;
		playerDamageThreshold = 40f;
		playerRagdollThreshold = 75f;
		maxAirTimeBeforeRagdoll = 1.5f;
		towingAccelerationBoost = 2f;
		towingMaxSpeedBoost = 1f;
		maxTowingGait = GaitType.Trot;
		currentStamina = 10f;
		currentMaxStamina = 10f;
		maxStamina = 20f;
		staminaCoreLossRatio = 0.1f;
		staminaCoreSpeedBonus = 3f;
		calorieToStaminaRatio = 0.1f;
		hydrationToStaminaRatio = 0.5f;
		maxStaminaCoreFromWater = 0.5f;
		avoidanceSphereRadius = 0.5f;
		avoidanceDetectionDistance = new Vector2(3f, 8f);
		groundAngleSlideThresholdForced = 50f;
		groundAngleSlideThreshold = 37f;
		groundAngleToRecoverFromSlide = 24f;
		normalVariationSlideThreshold = 2.5f;
		healingMultiplier = 4f;
		staminaReplenishAmount = 1f;
		prohibitedMedicalItems = Array.Empty<ItemDefinition>();
		base._002Ector();
	}

	static RidableHorse()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		dungTimeScale = 1f;
		TowAngleErrorPhrase = new Phrase("horse_tow_error", "Straighten up to tow");
		debug = false;
		autoAvoidance = true;
		throttledGroundAngleUpdate = true;
		groundAngleUpdateRate = 0.05f;
		decayMinutes = 180f;
		Population = 2f;
	}
}

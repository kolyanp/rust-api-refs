using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class FarmableAnimal : BaseCombatEntity
{
	public class FarmableAnimalNeedsWorkQueue : ObjectWorkQueue<FarmableAnimal>
	{
		protected override void RunJob(FarmableAnimal entity)
		{
			entity.TickHappiness();
		}
	}

	[Header("Client")]
	public Animator AnimalAnimator;

	public Renderer[] AllRenderers;

	public Gradient RendererColourGradient;

	public GameObjectRef CorpsePrefab;

	public GestureConfig PettingGesture;

	[Header("Movements")]
	public float RepositionMinTime = 10f;

	public float RepositionMaxTime = 30f;

	public float AnimalMoveSpeed = 0.75f;

	[Header("Needs")]
	public float HungerDecayRate = 0.5f;

	public float ThirstDecayRate = 0.1f;

	public float LoveDecayRate = 0.1f;

	public float SunlightDecayRate = 0.05f;

	public float SunlightIncreaseRate = 0.05f;

	[Header("Eating")]
	public float MinimumTimeBetweenEating = 30f;

	[Range(0f, 1f)]
	public float AmountPerFood = 0.4f;

	[Header("Drinking")]
	public float MinimumTimeBetweenDrinking = 10f;

	[Range(0f, 1f)]
	public float AmountPerDrink = 0.1f;

	[Header("Health")]
	public float HealthChangeRate = 10f;

	public float HealthChangeScale = 1f;

	[Header("Egg Production")]
	public float MinimumMinutesBetweenProduction = 10f;

	public float MaximumMinutesBetweenProduction = 50f;

	public ItemDefinition ItemToCreate;

	public const float StatCount = 4f;

	public const Flags RecentlyPetted = Flags.Reserved1;

	public const Flags Moving = Flags.Reserved2;

	public const Flags Sleeping = Flags.Reserved3;

	public const float MaxHappinessStat = 100f;

	public const int MaxNameLength = 12;

	private Vector3 currentMoveTarget;

	private TimeSince lastHappinessTick;

	private Action moveChickenAction;

	private Action moveToNewLocationAction;

	private TimeSince lastEat;

	private TimeSince lastDrink;

	private TimeUntil nextEggProduction;

	private TimeUntil nextHealthCheck;

	private TimeSince wokenUp;

	public static FarmableAnimalNeedsWorkQueue NeedsWorkQueue = new FarmableAnimalNeedsWorkQueue();

	public float AnimalHunger { get; private set; } = 100f;

	public float AnimalThirst { get; private set; } = 100f;

	public float AnimalLove { get; private set; } = 100f;

	public float AnimalSunlight { get; private set; } = 100f;

	public string AnimalName { get; private set; } = string.Empty;

	public float HappinessNormalised => (AnimalHunger + AnimalThirst + AnimalLove + AnimalSunlight) / 4f / 100f;

	private ChickenCoop ParentCoop => parentEntity.Get(base.isServer) as ChickenCoop;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("FarmableAnimal.OnRpcMessage"))
		{
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
						if (!RPC_Server.MaxDistance.Test(3115049114u, "RequestNameChange", this, player, 3f, checkParent: true))
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
							RequestNameChange(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RequestNameChange");
					}
				}
				return true;
			}
			if (rpc == 2457655601u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ServerPetChicken"));
				}
				using (TimeWarning.New("ServerPetChicken"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2457655601u, "ServerPetChicken", this, player, 3f))
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
							ServerPetChicken(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in ServerPetChicken");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		moveToNewLocationAction = MoveToNewLocation;
		Invoke(moveToNewLocationAction, Random.Range(RepositionMinTime, RepositionMaxTime));
		((Component)this).transform.localPosition = SnapMovementPos(((Component)this).transform.localPosition);
		lastHappinessTick = TimeSince.op_Implicit(0f);
		nextEggProduction = TimeUntil.op_Implicit(Random.Range(MinimumMinutesBetweenProduction, MaximumMinutesBetweenProduction) * 60f);
		InvokeRepeating(QueueHappinessTick, 10f, 1f);
	}

	private void QueueHappinessTick()
	{
		((ObjectWorkQueue<FarmableAnimal>)NeedsWorkQueue).Add(this);
	}

	private float ConvertDecayRateToSecondMultiplier(float rate)
	{
		return rate / 60f / 60f;
	}

	private void TickHappiness()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		float num = TimeSince.op_Implicit(lastHappinessTick);
		lastHappinessTick = TimeSince.op_Implicit(0f);
		AnimalHunger = Mathf.Clamp(AnimalHunger - num * ConvertDecayRateToSecondMultiplier(HungerDecayRate), 0f, 100f);
		AnimalThirst = Mathf.Clamp(AnimalThirst - num * ConvertDecayRateToSecondMultiplier(ThirstDecayRate), 0f, 100f);
		AnimalLove = Mathf.Clamp(AnimalLove - num * ConvertDecayRateToSecondMultiplier(LoveDecayRate), 0f, 100f);
		if ((Object)(object)ParentCoop != (Object)null && ParentCoop.IsInSun)
		{
			AnimalSunlight = Mathf.Clamp(AnimalSunlight + num * ConvertDecayRateToSecondMultiplier(SunlightIncreaseRate), 0f, 100f);
		}
		else
		{
			AnimalSunlight = Mathf.Clamp(AnimalSunlight - num * ConvertDecayRateToSecondMultiplier(SunlightDecayRate), 0f, 100f);
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			if (TOD_Sky.Instance.IsNight && !HasFlag(Flags.Reserved2))
			{
				flagsUpdateScope.Set(Flags.Reserved3, b: true);
			}
			else if (!TOD_Sky.Instance.IsNight)
			{
				if (HasFlag(Flags.Reserved3))
				{
					wokenUp = TimeSince.op_Implicit(0f);
				}
				flagsUpdateScope.Set(Flags.Reserved3, b: false);
			}
		}
		if (TimeSince.op_Implicit(lastEat) > MinimumTimeBetweenEating && AnimalHunger / 100f <= 1f - AmountPerFood * 0.2f)
		{
			lastEat = TimeSince.op_Implicit(0f);
			if ((Object)(object)ParentCoop != (Object)null)
			{
				Item currentFoodItem = ParentCoop.CurrentFoodItem;
				if (currentFoodItem != null)
				{
					currentFoodItem.UseItem();
					AnimalHunger = Mathf.Clamp(AnimalHunger + AmountPerFood * 100f, 0f, 100f);
				}
			}
		}
		if (TimeSince.op_Implicit(lastDrink) > MinimumTimeBetweenDrinking && AnimalThirst / 100f <= 1f - AmountPerDrink * 0.5f)
		{
			lastDrink = TimeSince.op_Implicit(0f);
			if ((Object)(object)ParentCoop != (Object)null)
			{
				Item currentWaterItem = ParentCoop.CurrentWaterItem;
				if (currentWaterItem != null)
				{
					currentWaterItem.UseItem();
					AnimalThirst = Mathf.Clamp(AnimalThirst + AmountPerDrink * 100f, 0f, 100f);
				}
			}
		}
		if (TimeUntil.op_Implicit(nextEggProduction) < 0f && (Object)(object)ItemToCreate != (Object)null)
		{
			nextEggProduction = TimeUntil.op_Implicit(Random.Range(MinimumMinutesBetweenProduction, MaximumMinutesBetweenProduction) * 60f);
			if (base.healthFraction > 0.75f)
			{
				float num2 = Mathx.RemapValClamped(HappinessNormalised, 0.75f, 1f, 0f, 1f);
				if (Random.Range(0f, 1f) < num2 && (Object)(object)ParentCoop != (Object)null)
				{
					ParentCoop.SpawnFoodProduction(ItemToCreate, 1);
				}
			}
		}
		if (TimeUntil.op_Implicit(nextHealthCheck) <= 0f)
		{
			nextHealthCheck = TimeUntil.op_Implicit(HealthChangeRate);
			if (HappinessNormalised < 0.5f)
			{
				float num3 = 1f - Mathx.RemapValClamped(HappinessNormalised, 0f, 0.5f, 0f, 1f);
				float amount = HealthChangeScale * num3;
				Hurt(amount);
			}
			else
			{
				float num4 = Mathx.RemapValClamped(HappinessNormalised, 0.5f, 1f, 0f, 1f);
				float amount2 = HealthChangeScale * num4;
				Heal(amount2);
			}
		}
	}

	private void MoveToNewLocation()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (HasFlag(Flags.Reserved3) || TimeSince.op_Implicit(wokenUp) <= 2f)
		{
			Invoke(moveToNewLocationAction, Random.Range(RepositionMinTime, RepositionMaxTime));
			return;
		}
		ChickenCoop parentCoop = ParentCoop;
		if ((Object)(object)parentCoop != (Object)null)
		{
			Vector3 randomMovePoint = parentCoop.GetRandomMovePoint();
			currentMoveTarget = randomMovePoint;
			if (moveChickenAction == null)
			{
				moveChickenAction = MoveChicken;
			}
			InvokeRepeating(moveChickenAction, 0f, 0f);
		}
	}

	private void MoveChicken()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)this).transform.localPosition;
		Quaternion val = ((Component)this).transform.localRotation;
		localPosition = SnapMovementPos(Vector3.MoveTowards(localPosition, currentMoveTarget, AnimalMoveSpeed * Time.deltaTime));
		if ((Object)(object)ParentCoop != (Object)null && !ParentCoop.IsLocationClear(localPosition, 0.25f, this))
		{
			StopMoving();
			return;
		}
		if (Vector3.Distance(currentMoveTarget, localPosition) > 0.3f)
		{
			Vector3 val2 = Vector3Ex.WithY(currentMoveTarget, localPosition.y) - localPosition;
			val = Quaternion.LookRotation(((Vector3)(ref val2)).normalized);
		}
		((Component)this).transform.SetLocalPositionAndRotation(localPosition, val);
		if (Vector3.Distance(localPosition, Vector3Ex.WithY(currentMoveTarget, localPosition.y)) < 0.1f)
		{
			StopMoving();
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved2, b: true);
	}

	private void StopMoving()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved2, b: false);
		}
		CancelInvoke(moveChickenAction);
		CancelInvoke(moveToNewLocationAction);
		Invoke(moveToNewLocationAction, Random.Range(RepositionMinTime, RepositionMaxTime));
	}

	private Vector3 SnapMovementPos(Vector3 desiredPos)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ParentCoop == (Object)null)
		{
			return desiredPos;
		}
		desiredPos = ((Component)ParentCoop).transform.TransformPoint(desiredPos);
		if ((Object)(object)ParentCoop != (Object)null)
		{
			if (ParentCoop.IsOnTerrain)
			{
				desiredPos = Vector3Ex.WithY(desiredPos, TerrainMeta.HeightMap.GetHeight(desiredPos));
			}
			else
			{
				ParentCoop.UpdateMovementPlane();
				desiredPos.y = ((Plane)(ref ParentCoop.MovementPlane)).ClosestPointOnPlane(desiredPos).y;
			}
		}
		return ((Component)ParentCoop).transform.InverseTransformPoint(desiredPos);
	}

	public void ApplyStartingStats(string defaultName)
	{
		AnimalName = defaultName;
		AnimalHunger = Random.Range(20f, 40f);
		AnimalThirst = Random.Range(20f, 40f);
		AnimalLove = Random.Range(20f, 40f);
		AnimalSunlight = Random.Range(20f, 40f);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void ServerPetChicken(RPCMessage msg)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		if (CanPetChicken(msg.player))
		{
			AnimalLove = Mathf.Clamp(AnimalLove + 15f, 0f, 100f);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved1, b: true);
			}
			Invoke(ClearPettedFlag, 5f);
			StopMoving();
			ClientRPC(RpcTarget.NetworkGroup("OnPetted"));
			Transform transform = ((Component)this).transform;
			Vector3 val = Vector3Ex.WithY(((Component)msg.player).transform.position, ((Component)this).transform.position.y) - ((Component)this).transform.position;
			transform.rotation = Quaternion.LookRotation(((Vector3)(ref val)).normalized, ((Component)this).transform.up);
			if ((Object)(object)PettingGesture != (Object)null && (Object)(object)msg.player != (Object)null)
			{
				msg.player.Server_StartGesture(PettingGesture, BasePlayer.GestureStartSource.ServerAction, bypassOwnershipCheck: true);
			}
		}
	}

	private void ClearPettedFlag()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b: false);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.farmableAnimal = Pool.Get<FarmableAnimal>();
		SaveToData(info.msg.farmableAnimal);
	}

	public void SaveToData(FarmableAnimal data)
	{
		data.hunger = AnimalHunger;
		data.thirst = AnimalThirst;
		data.love = AnimalLove;
		data.sunlight = AnimalSunlight;
		data.animalName = AnimalName;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f, CheckParent = true)]
	private void RequestNameChange(RPCMessage msg)
	{
		if (msg.player.CanBuild(cached: true))
		{
			string text = msg.read.String(12);
			if (!string.IsNullOrWhiteSpace(text))
			{
				AnimalName = text;
				SendNetworkUpdate();
			}
		}
	}

	public override void OnDied(HitInfo info)
	{
		if ((Object)(object)ParentCoop != (Object)null)
		{
			ParentCoop.OnAnimalDied(this);
		}
		BaseCorpse baseCorpse = DropCorpse(CorpsePrefab.resourcePath);
		if (Object.op_Implicit((Object)(object)baseCorpse))
		{
			baseCorpse.Spawn();
			baseCorpse.TakeChildren(this);
		}
		base.OnDied(info);
	}

	public override void AdminKill()
	{
		if ((Object)(object)ParentCoop != (Object)null)
		{
			ParentCoop.OnAnimalDied(this);
		}
		base.AdminKill();
	}

	[ServerVar(Help = "Simulates the provided number of hours on all farm animals within 10m")]
	public static void SimHours(ConsoleSystem.Arg arg)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		int num = arg.GetInt(0);
		PooledList<FarmableAnimal> val = Pool.Get<PooledList<FarmableAnimal>>();
		try
		{
			Vis.Entities(((Component)basePlayer).transform.position, 10f, (List<FarmableAnimal>)(object)val, 2048, (QueryTriggerInteraction)2);
			foreach (FarmableAnimal item in (List<FarmableAnimal>)(object)val)
			{
				if (item.isServer)
				{
					item.lastHappinessTick = TimeSince.op_Implicit(TimeSince.op_Implicit(item.lastHappinessTick) + (float)num * 60f * 60f);
					item.TickHappiness();
					item.SendNetworkUpdate();
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.farmableAnimal != null)
		{
			LoadFromData(info.msg.farmableAnimal);
		}
		if (base.isServer)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved1, b: false);
			}
		}
	}

	public void LoadFromData(FarmableAnimal data)
	{
		AnimalHunger = data.hunger;
		AnimalThirst = data.thirst;
		AnimalLove = data.love;
		AnimalSunlight = data.sunlight;
		AnimalName = data.animalName;
	}

	private bool CanPetChicken(BasePlayer bp)
	{
		if (!HasFlag(Flags.Reserved1) && !HasFlag(Flags.Reserved3) && (Object)(object)bp != (Object)null && !bp.isMounted && !bp.modelState.blocking)
		{
			return !bp.InGesture;
		}
		return false;
	}

	public ChickenCoop.AnimalStatus GetStatus()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ParentCoop != (Object)null)
		{
			foreach (ChickenCoop.AnimalStatus animal in ParentCoop.Animals)
			{
				EntityRef<FarmableAnimal> spawnedAnimal = animal.SpawnedAnimal;
				if (spawnedAnimal.uid == net.ID)
				{
					return animal;
				}
			}
		}
		return default(ChickenCoop.AnimalStatus);
	}
}

using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class GrowableEntity : BaseCombatEntity, IInstanceDataReceiver, IHeatSourceListener
{
	public class GrowableEntityUpdateQueue : ObjectWorkQueue<GrowableEntity>
	{
		protected override void RunJob(GrowableEntity entity)
		{
			if (((ObjectWorkQueue<GrowableEntity>)this).ShouldAdd(entity))
			{
				entity.CalculateQualities_Water();
			}
		}

		protected override bool ShouldAdd(GrowableEntity entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.IsValid();
			}
			return false;
		}
	}

	public const float artificalLightQuality = 1f;

	public const float planterGroundModifierBase = 0.6f;

	public const float fertilizerGroundModifierBonus = 0.4f;

	public const float growthGeneSpeedMultiplier = 0.25f;

	public const float waterGeneRequirementMultiplier = 0.1f;

	public const float hardinessGeneModifierBonus = 0.2f;

	public const float hardinessGeneTemperatureModifierBonus = 0.05f;

	public const float baseYieldIncreaseMultiplier = 1f;

	public const float yieldGeneBonusMultiplier = 0.25f;

	public const float maxNonPlanterGroundQuality = 0.6f;

	public const float deathRatePerQuality = 0.1f;

	public TimeCachedValue<float> sunExposure;

	public TimeCachedValue<float> artificialLightExposure;

	public TimeCachedValue<float> artificialTemperatureExposure;

	[Help("How many miliseconds to budget for processing growable quality updates per frame")]
	[ServerVar]
	public static float framebudgetms = 0.25f;

	public const Flags GodSpawn = Flags.Reserved1;

	public static GrowableEntityUpdateQueue growableEntityUpdateQueue = new GrowableEntityUpdateQueue();

	public bool underWater;

	public int seasons;

	public int harvests;

	public float terrainTypeValue;

	public float yieldPool;

	public PlanterBox planter;

	public PlantProperties Properties;

	public ItemDefinition SourceItemDef;

	public float stageAge;

	public GrowableGenes Genes = new GrowableGenes();

	public const float startingHealth = 10f;

	private bool god => HasFlag(Flags.Reserved1);

	public float CurrentTemperature
	{
		get
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)GetPlanter() != (Object)null)
			{
				return GetPlanter().GetPlantTemperature();
			}
			return Climate.GetTemperature(((Component)this).transform.position) + (artificialTemperatureExposure?.Get(force: false) ?? 0f);
		}
	}

	public PlantProperties.State State { get; set; }

	public float Age { get; set; }

	public float LightQuality { get; set; }

	public float GroundQuality { get; set; } = 1f;

	public float WaterQuality { get; set; }

	public float WaterConsumption { get; set; }

	public bool Fertilized { get; set; }

	public float TemperatureQuality { get; set; }

	public float OverallQuality { get; set; }

	public float Yield { get; set; }

	public float StageProgressFraction => stageAge / currentStage.lifeLengthSeconds;

	public PlantProperties.Stage currentStage => Properties.stages[(int)State];

	public static float ThinkDeltaTime => ConVar.Server.planttick;

	public float growDeltaTime => ConVar.Server.planttick * ConVar.Server.planttickscale;

	public override bool ValidateMeleeColliderAntihack => false;

	public int CurrentPickAmount => Mathf.RoundToInt(CurrentPickAmountFloat);

	public float CurrentPickAmountFloat => (currentStage.resources + Yield) * (float)Properties.pickupMultiplier;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("GrowableEntity.OnRpcMessage"))
		{
			if (rpc == 759768385 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_EatFruit"));
				}
				using (TimeWarning.New("RPC_EatFruit"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(759768385u, "RPC_EatFruit", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(759768385u, "RPC_EatFruit", this, player, 3f))
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
							RPC_EatFruit(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_EatFruit");
					}
				}
				return true;
			}
			if (rpc == 598660365 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_PickFruit"));
				}
				using (TimeWarning.New("RPC_PickFruit"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(598660365u, "RPC_PickFruit", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(598660365u, "RPC_PickFruit", this, player, 3f))
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
							RPC_PickFruit(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_PickFruit");
					}
				}
				return true;
			}
			if (rpc == 3465633431u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_PickFruitAll"));
				}
				using (TimeWarning.New("RPC_PickFruitAll"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3465633431u, "RPC_PickFruitAll", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3465633431u, "RPC_PickFruitAll", this, player, 3f))
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
							RPC_PickFruitAll(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RPC_PickFruitAll");
					}
				}
				return true;
			}
			if (rpc == 1959480148 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_RemoveDying"));
				}
				using (TimeWarning.New("RPC_RemoveDying"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1959480148u, "RPC_RemoveDying", this, player, 3f))
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
							RPC_RemoveDying(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in RPC_RemoveDying");
					}
				}
				return true;
			}
			if (rpc == 1771718099 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_RemoveDyingAll"));
				}
				using (TimeWarning.New("RPC_RemoveDyingAll"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1771718099u, "RPC_RemoveDyingAll", this, player, 3f))
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
							RPC_RemoveDyingAll(msg6);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in RPC_RemoveDyingAll");
					}
				}
				return true;
			}
			if (rpc == 232075937 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_RequestQualityUpdate"));
				}
				using (TimeWarning.New("RPC_RequestQualityUpdate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(232075937u, "RPC_RequestQualityUpdate", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg7 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_RequestQualityUpdate(msg7);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
						player.Kick("RPC Error in RPC_RequestQualityUpdate");
					}
				}
				return true;
			}
			if (rpc == 2222960834u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_TakeClone"));
				}
				using (TimeWarning.New("RPC_TakeClone"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2222960834u, "RPC_TakeClone", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2222960834u, "RPC_TakeClone", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg8 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_TakeClone(msg8);
						}
					}
					catch (Exception ex7)
					{
						Debug.LogException(ex7);
						player.Kick("RPC Error in RPC_TakeClone");
					}
				}
				return true;
			}
			if (rpc == 95639240 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_TakeCloneAll"));
				}
				using (TimeWarning.New("RPC_TakeCloneAll"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(95639240u, "RPC_TakeCloneAll", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(95639240u, "RPC_TakeCloneAll", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg9 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_TakeCloneAll(msg9);
						}
					}
					catch (Exception ex8)
					{
						Debug.LogException(ex8);
						player.Kick("RPC Error in RPC_TakeCloneAll");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void QueueForQualityUpdate()
	{
		((ObjectWorkQueue<GrowableEntity>)growableEntityUpdateQueue).Add(this);
	}

	public void SetGodQuality(bool qual)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, qual);
	}

	public void CalculateQualities(bool firstTime, bool forceArtificialLightUpdates = false, bool forceArtificialTemperatureUpdates = false)
	{
		if (IsDead())
		{
			return;
		}
		if (god)
		{
			SetMaxGrowingConditions();
			return;
		}
		if (sunExposure == null)
		{
			sunExposure = new TimeCachedValue<float>
			{
				refreshCooldown = 30f,
				refreshRandomRange = 5f,
				updateValue = SunRaycast
			};
		}
		if (artificialLightExposure == null)
		{
			artificialLightExposure = new TimeCachedValue<float>
			{
				refreshCooldown = 60f,
				refreshRandomRange = 5f,
				updateValue = CalculateArtificialLightExposure
			};
		}
		if (artificialTemperatureExposure == null)
		{
			artificialTemperatureExposure = new TimeCachedValue<float>
			{
				refreshCooldown = 60f,
				refreshRandomRange = 5f,
				updateValue = CalculateArtificialTemperature
			};
		}
		if (forceArtificialTemperatureUpdates)
		{
			artificialTemperatureExposure.ForceNextRun();
		}
		CalculateLightQuality(forceArtificialLightUpdates || firstTime);
		CalculateWaterQuality();
		CalculateWaterConsumption();
		CalculateGroundQuality(firstTime);
		CalculateTemperatureQuality();
		CalculateOverallQuality();
	}

	private void CalculateQualities_Water()
	{
		CalculateWaterQuality();
		CalculateWaterConsumption();
		CalculateOverallQuality();
	}

	public void CalculateLightQuality(bool forceArtificalUpdate)
	{
		float num = Mathf.Clamp01(Properties.timeOfDayHappiness.Evaluate(TOD_Sky.Instance.Cycle.Hour));
		if (!ConVar.Server.plantlightdetection)
		{
			LightQuality = num;
			return;
		}
		LightQuality = CalculateSunExposure(forceArtificalUpdate) * num;
		if (LightQuality <= 0f)
		{
			LightQuality = GetArtificialLightExposure(forceArtificalUpdate);
		}
		LightQuality = RemapValue(LightQuality, 0f, Properties.OptimalLightQuality, 0f, 1f);
	}

	public float CalculateSunExposure(bool force)
	{
		if (TOD_Sky.Instance.IsNight)
		{
			return 0f;
		}
		if ((Object)(object)GetPlanter() != (Object)null)
		{
			return GetPlanter().GetSunExposure();
		}
		return sunExposure?.Get(force) ?? 0f;
	}

	public float SunRaycast()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return SunRaycast(((Component)this).transform.position + new Vector3(0f, 1f, 0f));
	}

	public float GetArtificialLightExposure(bool force)
	{
		if ((Object)(object)GetPlanter() != (Object)null)
		{
			return GetPlanter().GetArtificialLightExposure();
		}
		return artificialLightExposure?.Get(force) ?? 0f;
	}

	public float CalculateArtificialLightExposure()
	{
		return CalculateArtificialLightExposure(((Component)this).transform);
	}

	public static float CalculateArtificialLightExposure(Transform forTransform)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		float result = 0f;
		List<CeilingLight> list = Pool.Get<List<CeilingLight>>();
		Vector3 position = forTransform.position;
		float ceilingLightGrowableRange = ConVar.Server.ceilingLightGrowableRange;
		CeilingLight.FarmLightGrid.Query(position.x, position.z, ceilingLightGrowableRange, list);
		foreach (CeilingLight item in list)
		{
			if (!(item.SqrDistance(position) > ceilingLightGrowableRange * ceilingLightGrowableRange) && item.IsOn())
			{
				result = 1f;
				break;
			}
		}
		Pool.FreeUnmanaged<CeilingLight>(ref list);
		return result;
	}

	public static float SunRaycast(Vector3 checkPosition)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = TOD_Sky.Instance.Components.Sun.transform.position - checkPosition;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		RaycastHit val2 = default(RaycastHit);
		if (!Physics.Raycast(checkPosition, normalized, ref val2, 100f, 144769025))
		{
			return 1f;
		}
		return 0f;
	}

	public void CalculateWaterQuality()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Invalid comparison between Unknown and I4
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Invalid comparison between Unknown and I4
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Invalid comparison between Unknown and I4
		if ((Object)(object)GetPlanter() != (Object)null)
		{
			float soilSaturationFraction = planter.soilSaturationFraction;
			if (soilSaturationFraction > ConVar.Server.optimalPlanterQualitySaturation)
			{
				WaterQuality = RemapValue(soilSaturationFraction, ConVar.Server.optimalPlanterQualitySaturation, 1f, 1f, 0.6f);
			}
			else
			{
				WaterQuality = RemapValue(soilSaturationFraction, 0f, ConVar.Server.optimalPlanterQualitySaturation, 0f, 1f);
			}
		}
		else
		{
			Enum val = (Enum)TerrainMeta.BiomeMap.GetBiomeMaxType(((Component)this).transform.position);
			if (val - 1 > 1 && (int)val != 4)
			{
				if ((int)val == 8)
				{
					WaterQuality = 0.1f;
				}
				else
				{
					WaterQuality = 0f;
				}
			}
			else
			{
				WaterQuality = 0.3f;
			}
		}
		WaterQuality = Mathf.Clamp01(WaterQuality);
		WaterQuality = RemapValue(WaterQuality, 0f, Properties.OptimalWaterQuality, 0f, 1f);
	}

	public void CalculateGroundQuality(bool firstCheck)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (underWater && !firstCheck)
		{
			GroundQuality = 0f;
			return;
		}
		if (firstCheck)
		{
			Vector3 position = ((Component)this).transform.position;
			if (WaterLevel.Test(position, waves: true, volumes: true, this))
			{
				underWater = true;
				GroundQuality = 0f;
				return;
			}
			underWater = false;
			terrainTypeValue = GetGroundTypeValue(position);
		}
		if ((Object)(object)GetPlanter() != (Object)null)
		{
			GroundQuality = 0.6f;
			GroundQuality += (Fertilized ? 0.4f : 0f);
		}
		else
		{
			GroundQuality = terrainTypeValue;
			float num = (float)Genes.GetGeneTypeCount(GrowableGenetics.GeneType.Hardiness) * 0.2f;
			float num2 = GroundQuality + num;
			GroundQuality = Mathf.Min(0.6f, num2);
		}
		GroundQuality = RemapValue(GroundQuality, 0f, Properties.OptimalGroundQuality, 0f, 1f);
	}

	public float GetGroundTypeValue(Vector3 pos)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected I4, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Invalid comparison between Unknown and I4
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		Enum val = (Enum)TerrainMeta.SplatMap.GetSplatMaxType(pos);
		if ((int)val <= 16)
		{
			switch (val - 1)
			{
			default:
				if ((int)val != 8)
				{
					if ((int)val != 16)
					{
						break;
					}
					return 0.3f;
				}
				return 0f;
			case 1:
				return 0f;
			case 0:
				return 0.3f;
			case 3:
				return 0f;
			case 2:
				break;
			}
		}
		else
		{
			if ((int)val == 32)
			{
				return 0.2f;
			}
			if ((int)val == 64)
			{
				return 0f;
			}
			if ((int)val == 128)
			{
				return 0f;
			}
		}
		return 0.5f;
	}

	public void CalculateTemperatureQuality()
	{
		TemperatureQuality = Mathf.Clamp01(Properties.temperatureHappiness.Evaluate(CurrentTemperature));
		float num = (float)Genes.GetGeneTypeCount(GrowableGenetics.GeneType.Hardiness) * 0.05f;
		TemperatureQuality = Mathf.Clamp01(TemperatureQuality + num);
		TemperatureQuality = RemapValue(TemperatureQuality, 0f, Properties.OptimalTemperatureQuality, 0f, 1f);
	}

	public void SetMaxGrowingConditions()
	{
		LightQuality = 1f;
		WaterQuality = 1f;
		GroundQuality = 1f;
		TemperatureQuality = 1f;
		OverallQuality = 1f;
	}

	public float CalculateOverallQuality()
	{
		float num = 1f;
		if (ConVar.Server.useMinimumPlantCondition)
		{
			num = Mathf.Min(num, LightQuality);
			num = Mathf.Min(num, WaterQuality);
			num = Mathf.Min(num, GroundQuality);
			num = Mathf.Min(num, TemperatureQuality);
		}
		else
		{
			num = LightQuality * WaterQuality * GroundQuality * TemperatureQuality;
		}
		OverallQuality = num;
		return OverallQuality;
	}

	public void CalculateWaterConsumption()
	{
		float num = Properties.temperatureWaterRequirementMultiplier.Evaluate(CurrentTemperature);
		float num2 = 1f + (float)Genes.GetGeneTypeCount(GrowableGenetics.GeneType.WaterRequirement) * 0.1f;
		WaterConsumption = Properties.WaterIntake * num * num2;
	}

	public float CalculateArtificialTemperature()
	{
		return CalculateArtificialTemperature(((Component)this).transform);
	}

	public static float CalculateArtificialTemperature(Transform forTransform)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = forTransform.position;
		List<GrowableHeatSource> list = Pool.Get<List<GrowableHeatSource>>();
		Vector3 position2 = forTransform.position;
		float artificialTemperatureGrowableRange = ConVar.Server.artificialTemperatureGrowableRange;
		GrowableHeatSource.FarmHeatSourceGrid.UpdateMobileEntities();
		GrowableHeatSource.FarmHeatSourceGrid.Grid.Query(position2.x, position2.z, artificialTemperatureGrowableRange, list);
		float num = 0f;
		foreach (GrowableHeatSource item in list)
		{
			if (!((Object)(object)item.GetBaseEntity() == (Object)null) && !((Object)(object)((Component)item.GetBaseEntity()).transform == (Object)null) && !(Vector3.Distance(((Component)item.GetBaseEntity()).transform.position, position2) > artificialTemperatureGrowableRange))
			{
				num = Mathf.Max(item.ApplyHeat(position), num);
			}
		}
		Pool.FreeUnmanaged<GrowableHeatSource>(ref list);
		return num;
	}

	public int CalculateMarketValue()
	{
		int baseMarketValue = Properties.BaseMarketValue;
		int num = Genes.GetPositiveGeneCount() * 10;
		int num2 = Genes.GetNegativeGeneCount() * -10;
		baseMarketValue += num;
		baseMarketValue += num2;
		return Mathf.Max(0, baseMarketValue);
	}

	private static float RemapValue(float inValue, float minA, float maxA, float minB, float maxB)
	{
		if (inValue >= maxA)
		{
			return maxB;
		}
		float num = Mathf.InverseLerp(minA, maxA, inValue);
		return Mathf.Lerp(minB, maxB, num);
	}

	public bool IsFood()
	{
		if (Properties.pickupItem.category == ItemCategory.Food)
		{
			return (Object)(object)((Component)Properties.pickupItem).GetComponent<ItemModConsume>() != (Object)null;
		}
		return false;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		SetGrowing(state: true);
		base.health = 10f;
		ResetSeason();
		Genes.GenerateRandom(this);
		if (!Application.isLoadingSave)
		{
			CalculateQualities(firstTime: true);
		}
	}

	public PlanterBox GetPlanter()
	{
		if ((Object)(object)planter == (Object)null)
		{
			BaseEntity baseEntity = GetParentEntity();
			if ((Object)(object)baseEntity != (Object)null)
			{
				planter = baseEntity as PlanterBox;
			}
		}
		return planter;
	}

	public void SetGrowing(bool state)
	{
		if (state)
		{
			if (!IsInvoking(RunUpdate))
			{
				InvokeRandomized(RunUpdate, ThinkDeltaTime, ThinkDeltaTime, ThinkDeltaTime * 0.1f);
			}
		}
		else
		{
			CancelInvoke(RunUpdate);
		}
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		planter = newParent as PlanterBox;
		if (!Application.isLoadingSave && (Object)(object)planter != (Object)null)
		{
			planter.FertilizeGrowables();
		}
		CalculateQualities(firstTime: true);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		CalculateQualities(firstTime: true);
	}

	public void ResetSeason()
	{
		Yield = 0f;
		yieldPool = 0f;
	}

	public void RunUpdate()
	{
		if (!IsDead())
		{
			CalculateQualities(firstTime: false);
			float overallQuality = CalculateOverallQuality();
			float actualStageAgeIncrease = UpdateAge(overallQuality);
			UpdateHealthAndYield(overallQuality, actualStageAgeIncrease);
			if (base.health <= 0f)
			{
				TellPlanter();
				Die();
			}
			else
			{
				UpdateState();
				ConsumeWater();
				SendNetworkUpdate();
			}
		}
	}

	public float UpdateAge(float overallQuality)
	{
		Age += growDeltaTime;
		float num = (currentStage.IgnoreConditions ? 1f : (Mathf.Max(overallQuality, 0f) * GetGrowthBonus(overallQuality)));
		float num2 = growDeltaTime * num;
		stageAge += num2;
		return num2;
	}

	public void UpdateHealthAndYield(float overallQuality, float actualStageAgeIncrease)
	{
		if ((Object)(object)GetPlanter() == (Object)null && Random.Range(0f, 1f) <= ConVar.Server.nonPlanterDeathChancePerTick)
		{
			base.health = 0f;
			return;
		}
		if (overallQuality <= 0f)
		{
			ApplyDeathRate();
		}
		base.health += overallQuality * currentStage.health * growDeltaTime;
		if (yieldPool > 0f)
		{
			float num = currentStage.yield / (currentStage.lifeLengthSeconds / growDeltaTime);
			float num2 = Mathf.Min(yieldPool, num * (actualStageAgeIncrease / growDeltaTime));
			yieldPool -= num;
			float num3 = 1f + (float)Genes.GetGeneTypeCount(GrowableGenetics.GeneType.Yield) * 0.25f;
			Yield += num2 * 1f * num3;
		}
	}

	public void ApplyDeathRate()
	{
		float num = 0f;
		if (WaterQuality <= 0f)
		{
			num += 0.1f;
		}
		if (LightQuality <= 0f)
		{
			num += 0.1f;
		}
		if (GroundQuality <= 0f)
		{
			num += 0.1f;
		}
		if (TemperatureQuality <= 0f)
		{
			num += 0.1f;
		}
		base.health -= num;
	}

	public float GetGrowthBonus(float overallQuality)
	{
		float result = 1f + (float)Genes.GetGeneTypeCount(GrowableGenetics.GeneType.GrowthSpeed) * 0.25f;
		if (overallQuality <= 0f)
		{
			result = 1f;
		}
		return result;
	}

	public PlantProperties.State UpdateState()
	{
		if (stageAge <= currentStage.lifeLengthSeconds)
		{
			return State;
		}
		if (State == PlantProperties.State.Dying)
		{
			TellPlanter();
			Die();
			return PlantProperties.State.Dying;
		}
		if (currentStage.nextState <= State)
		{
			seasons++;
		}
		if (seasons >= Properties.MaxSeasons)
		{
			ChangeState(PlantProperties.State.Dying, resetAge: true);
		}
		else
		{
			ChangeState(currentStage.nextState, resetAge: true);
		}
		return State;
	}

	public void ConsumeWater()
	{
		if (State != PlantProperties.State.Dying && !((Object)(object)GetPlanter() == (Object)null))
		{
			int num = Mathf.CeilToInt(Mathf.Min((float)planter.soilSaturation, WaterConsumption));
			if ((float)num > 0f)
			{
				planter.ConsumeWater(num, this);
			}
		}
	}

	public void Fertilize()
	{
		if (!Fertilized)
		{
			Fertilized = true;
			CalculateQualities(firstTime: false);
			SendNetworkUpdate();
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_TakeClone(RPCMessage msg)
	{
		TakeClones(msg.player);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	public void RPC_TakeCloneAll(RPCMessage msg)
	{
		if ((Object)(object)GetParentEntity() != (Object)null)
		{
			List<GrowableEntity> list = Pool.Get<List<GrowableEntity>>();
			foreach (BaseEntity child in GetParentEntity().children)
			{
				if ((Object)(object)child != (Object)(object)this && child is GrowableEntity item)
				{
					list.Add(item);
				}
			}
			foreach (GrowableEntity item2 in list)
			{
				item2.TakeClones(msg.player);
			}
			Pool.FreeUnmanaged<GrowableEntity>(ref list);
		}
		TakeClones(msg.player);
	}

	public void TakeClones(BasePlayer player)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null || !CanClone(player) || Interface.CallHook("CanTakeCutting", player, this) != null)
		{
			return;
		}
		int num = Properties.BaseCloneCount + Genes.GetGeneTypeCount(GrowableGenetics.GeneType.Yield) / 2;
		if (num > 0)
		{
			Item item = ItemManager.Create(Properties.CloneItem, num, 0uL, isServerSide: true, 0uL);
			item.SetItemOwnership(player, ItemOwnershipPhrases.Cloned);
			GrowableGeneEncoding.EncodeGenesToItem(this, item);
			Facepunch.Rust.Analytics.Azure.OnGatherItem(item.info.shortname, item.amount, this, player);
			player.GiveItem(item, GiveItemReason.ResourceHarvested, GiveItemOptions.BackpackOverflow);
			if (Properties.pickEffect.isValid)
			{
				Effect.server.Run(Properties.pickEffect.resourcePath, ((Component)this).transform.position, Vector3.up);
			}
			TellPlanter();
			Die();
		}
	}

	public void PickFruit(BasePlayer player, bool eat = false)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if (!CanPick(player) || Interface.CallHook("OnGrowableGather", this, player, eat) != null)
		{
			return;
		}
		harvests++;
		GiveFruit(player, CurrentPickAmount, eat);
		RandomItemDispenser randomItemDispenser = PrefabAttribute.server.Find<RandomItemDispenser>(prefabID);
		if (randomItemDispenser != null)
		{
			randomItemDispenser.DistributeItems(player, ((Component)this).transform.position);
		}
		ResetSeason();
		if (Properties.pickEffect.isValid)
		{
			Effect.server.Run(Properties.pickEffect.resourcePath, ((Component)this).transform.position, Vector3.up);
		}
		if (harvests >= Properties.maxHarvests)
		{
			if (Properties.disappearAfterHarvest)
			{
				TellPlanter();
				Die();
			}
			else
			{
				ChangeState(PlantProperties.State.Dying, resetAge: true);
			}
		}
		else
		{
			ChangeState(PlantProperties.State.Mature, resetAge: true);
		}
	}

	public void GiveFruit(BasePlayer player, int amount, bool eat)
	{
		if (amount <= 0)
		{
			return;
		}
		bool enabled = Properties.pickupItem.condition.enabled;
		if (enabled)
		{
			for (int i = 0; i < amount; i++)
			{
				GiveFruit(player, 1, enabled, eat);
			}
		}
		else
		{
			GiveFruit(player, amount, enabled, eat);
		}
	}

	public void GiveFruit(BasePlayer player, int amount, bool applyCondition, bool eat)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		Item item = ItemManager.Create(Properties.pickupItem, amount, 0uL, isServerSide: true, 0uL);
		item.SetItemOwnership(player, ItemOwnershipPhrases.Harvested);
		if (applyCondition)
		{
			item.conditionNormalized = Properties.fruitVisualScaleCurve.Evaluate(StageProgressFraction);
		}
		if (eat && (Object)(object)player != (Object)null && IsFood())
		{
			ItemModConsume component = ((Component)item.info).GetComponent<ItemModConsume>();
			if ((Object)(object)component != (Object)null)
			{
				component.DoAction(item, player);
				return;
			}
		}
		if ((Object)(object)player != (Object)null)
		{
			Interface.CallHook("OnGrowableGathered", this, item, player);
			Facepunch.Rust.Analytics.Azure.OnGatherItem(item.info.shortname, item.amount, this, player);
			player.GiveItem(item, GiveItemReason.ResourceHarvested, GiveItemOptions.BackpackOverflow);
		}
		else
		{
			item.Drop(((Component)this).transform.position + Vector3.up * 0.5f, Vector3.up * 1f);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void RPC_PickFruit(RPCMessage msg)
	{
		PickFruit(msg.player);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	public void RPC_EatFruit(RPCMessage msg)
	{
		PickFruit(msg.player, eat: true);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void RPC_PickFruitAll(RPCMessage msg)
	{
		if ((Object)(object)GetParentEntity() != (Object)null)
		{
			List<GrowableEntity> list = Pool.Get<List<GrowableEntity>>();
			foreach (BaseEntity child in GetParentEntity().children)
			{
				if ((Object)(object)child != (Object)(object)this && child is GrowableEntity item)
				{
					list.Add(item);
				}
			}
			foreach (GrowableEntity item2 in list)
			{
				item2.PickFruit(msg.player);
			}
			Pool.FreeUnmanaged<GrowableEntity>(ref list);
		}
		PickFruit(msg.player);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_RemoveDying(RPCMessage msg)
	{
		RemoveDying(msg.player);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void RPC_RemoveDyingAll(RPCMessage msg)
	{
		if ((Object)(object)GetParentEntity() != (Object)null)
		{
			List<GrowableEntity> list = Pool.Get<List<GrowableEntity>>();
			foreach (BaseEntity child in GetParentEntity().children)
			{
				if ((Object)(object)child != (Object)(object)this && child is GrowableEntity item)
				{
					list.Add(item);
				}
			}
			foreach (GrowableEntity item2 in list)
			{
				item2.RemoveDying(msg.player);
			}
			Pool.FreeUnmanaged<GrowableEntity>(ref list);
		}
		RemoveDying(msg.player);
	}

	public void RemoveAllForce()
	{
		if ((Object)(object)GetParentEntity() != (Object)null)
		{
			List<GrowableEntity> list = Pool.Get<List<GrowableEntity>>();
			foreach (BaseEntity child in GetParentEntity().children)
			{
				if ((Object)(object)child != (Object)(object)this && child is GrowableEntity item)
				{
					list.Add(item);
				}
			}
			foreach (GrowableEntity item2 in list)
			{
				item2.RemoveDying(null);
			}
			Pool.FreeUnmanaged<GrowableEntity>(ref list);
		}
		ForceRemove();
	}

	public void ForceRemove()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Properties.removeDyingItem == (Object)null))
		{
			if (Properties.removeDyingEffect.isValid)
			{
				Effect.server.Run(Properties.removeDyingEffect.resourcePath, ((Component)this).transform.position, Vector3.up);
			}
			TellPlanter();
			Die();
		}
	}

	public void RemoveDying(BasePlayer receiver)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		if (State == PlantProperties.State.Dying && !((Object)(object)Properties.removeDyingItem == (Object)null) && Interface.CallHook("OnRemoveDying", this, receiver) == null)
		{
			if (Properties.removeDyingEffect.isValid)
			{
				Effect.server.Run(Properties.removeDyingEffect.resourcePath, ((Component)this).transform.position, Vector3.up);
			}
			Item item = ItemManager.Create(Properties.removeDyingItem, 1, 0uL, isServerSide: true, 0uL);
			if ((Object)(object)receiver != (Object)null)
			{
				receiver.GiveItem(item, GiveItemReason.PickedUp, GiveItemOptions.BackpackOverflow);
			}
			else
			{
				item.Drop(((Component)this).transform.position + Vector3.up * 0.5f, Vector3.up * 1f);
			}
			TellPlanter();
			Die();
		}
	}

	private void TellPlanter()
	{
		BaseEntity baseEntity = GetParentEntity();
		if ((Object)(object)baseEntity != (Object)null && baseEntity is PlanterBox planterBox)
		{
			planterBox.OnPlantRemoved(this, null);
		}
	}

	[ServerVar(ServerAdmin = true, Help = "(Generated) Server admin: forces all growable entities within 6m of the calling player to advance to their next growth stage instantly")]
	public static void GrowAll(ConsoleSystem.Arg arg)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer.IsAdmin)
		{
			return;
		}
		PooledList<GrowableEntity> val = Pool.Get<PooledList<GrowableEntity>>();
		try
		{
			Query.Server.GetInSphere(basePlayer.ServerPosition, 6f, (List<GrowableEntity>)(object)val, Query.DistanceCheckType.Bounds);
			foreach (GrowableEntity item in (List<GrowableEntity>)(object)val)
			{
				if (item.isServer)
				{
					item.ChangeState(item.currentStage.nextState, resetAge: false);
				}
			}
			PooledList<PlanterBox> val2 = Pool.Get<PooledList<PlanterBox>>();
			try
			{
				Query.Server.GetInSphere(basePlayer.ServerPosition, 6f, (List<PlanterBox>)(object)val2, Query.DistanceCheckType.Bounds);
				foreach (PlanterBox item2 in (List<PlanterBox>)(object)val2)
				{
					if (item2.children == null)
					{
						continue;
					}
					foreach (BaseEntity child in item2.children)
					{
						if (child is GrowableEntity growableEntity)
						{
							growableEntity.ChangeState(growableEntity.currentStage.nextState, resetAge: false);
						}
					}
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

	[ServerVar(ServerAdmin = true, Help = "(Generated) Server admin: kills all growable entities within 6m of the calling player, removing them from the planter")]
	public static void KillAll(ConsoleSystem.Arg arg)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer.IsAdmin)
		{
			return;
		}
		PooledList<GrowableEntity> val = Pool.Get<PooledList<GrowableEntity>>();
		try
		{
			Query.Server.GetInSphere(basePlayer.ServerPosition, 6f, (List<GrowableEntity>)(object)val, Query.DistanceCheckType.Bounds);
			foreach (GrowableEntity item in (List<GrowableEntity>)(object)val)
			{
				if (item.isServer)
				{
					item.ChangeState(item.currentStage.nextState, resetAge: false);
				}
			}
			PooledList<PlanterBox> val2 = Pool.Get<PooledList<PlanterBox>>();
			try
			{
				Query.Server.GetInSphere(basePlayer.ServerPosition, 6f, (List<PlanterBox>)(object)val2, Query.DistanceCheckType.Bounds);
				foreach (PlanterBox item2 in (List<PlanterBox>)(object)val2)
				{
					if (item2.children == null)
					{
						continue;
					}
					foreach (BaseEntity child in item2.children)
					{
						if (child is GrowableEntity growableEntity)
						{
							growableEntity.ChangeState(PlantProperties.State.Dying, resetAge: false);
						}
					}
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

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void RPC_RequestQualityUpdate(RPCMessage msg)
	{
		if ((Object)(object)msg.player != (Object)null)
		{
			GrowableEntity val = Pool.Get<GrowableEntity>();
			val.lightModifier = LightQuality;
			val.groundModifier = GroundQuality;
			val.waterModifier = WaterQuality;
			val.happiness = OverallQuality;
			val.temperatureModifier = TemperatureQuality;
			val.waterConsumption = WaterConsumption;
			ClientRPC(RpcTarget.Player("RPC_ReceiveQualityUpdate", msg.player), val);
			val.Dispose();
		}
	}

	public void ReceiveInstanceData(InstanceData data)
	{
		GrowableGeneEncoding.DecodeIntToGenes(data.dataInt, Genes);
		GrowableGeneEncoding.DecodeIntToPreviousGenes(data.dataInt, Genes);
	}

	public override void ResetState()
	{
		base.ResetState();
		State = PlantProperties.State.Seed;
	}

	private bool BlockedInDeepSea(BasePlayer player)
	{
		if (DeepSeaManager.IsInsideDeepSea((BaseNetworkable)this) && HasParent() && parentEntity.Get(base.isServer) is PlanterBoxStatic)
		{
			if ((Object)(object)DeepSeaManager.Get(base.isServer) != (Object)null)
			{
				return !DeepSeaManager.Get(base.isServer).HasPaidFoodToll(player);
			}
			return false;
		}
		return false;
	}

	public bool CanPick(BasePlayer player)
	{
		if (BlockedInDeepSea(player))
		{
			return false;
		}
		return currentStage.resources > 0f;
	}

	public bool CanTakeSeeds()
	{
		if (currentStage.resources > 0f)
		{
			return (Object)(object)Properties.SeedItem != (Object)null;
		}
		return false;
	}

	public bool CanClone(BasePlayer player)
	{
		if (BlockedInDeepSea(player))
		{
			return false;
		}
		if (currentStage.resources > 0f)
		{
			return (Object)(object)Properties.CloneItem != (Object)null;
		}
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.growableEntity = Pool.Get<GrowableEntity>();
		info.msg.growableEntity.state = (int)State;
		info.msg.growableEntity.totalAge = Age;
		info.msg.growableEntity.stageAge = stageAge;
		info.msg.growableEntity.yieldFraction = Yield;
		info.msg.growableEntity.yieldPool = yieldPool;
		info.msg.growableEntity.fertilized = Fertilized;
		if (Genes != null)
		{
			Genes.Save(info);
		}
		if (!info.forDisk)
		{
			info.msg.growableEntity.lightModifier = LightQuality;
			info.msg.growableEntity.groundModifier = GroundQuality;
			info.msg.growableEntity.waterModifier = WaterQuality;
			info.msg.growableEntity.happiness = OverallQuality;
			info.msg.growableEntity.temperatureModifier = TemperatureQuality;
			info.msg.growableEntity.waterConsumption = WaterConsumption;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.growableEntity != null)
		{
			Age = info.msg.growableEntity.totalAge;
			stageAge = info.msg.growableEntity.stageAge;
			Yield = info.msg.growableEntity.yieldFraction;
			Fertilized = info.msg.growableEntity.fertilized;
			yieldPool = info.msg.growableEntity.yieldPool;
			Genes.Load(info);
			ChangeState((PlantProperties.State)info.msg.growableEntity.state, resetAge: false, loading: true);
		}
		else
		{
			Genes.GenerateRandom(this);
		}
	}

	public void ChangeState(PlantProperties.State state, bool resetAge, bool loading = false)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnGrowableStateChange", this, state) != null || (base.isServer && State == state))
		{
			return;
		}
		State = state;
		if (!base.isServer)
		{
			return;
		}
		if (!loading)
		{
			if (currentStage.resources > 0f)
			{
				yieldPool = currentStage.yield;
			}
			if (state == PlantProperties.State.Crossbreed)
			{
				if (Properties.CrossBreedEffect.isValid)
				{
					Effect.server.Run(Properties.CrossBreedEffect.resourcePath, ((Component)this).transform.position, Vector3.up);
				}
				GrowableGenetics.CrossBreed(this);
			}
			SendNetworkUpdate();
		}
		if (resetAge)
		{
			stageAge = 0f;
		}
	}

	public void OnHeatSourceChanged()
	{
		CalculateQualities(firstTime: false, forceArtificialLightUpdates: false, forceArtificialTemperatureUpdates: true);
		SendNetworkUpdate();
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		float num = (((Object)(object)deployedBy.modifiers != (Object)null) ? deployedBy.modifiers.GetValue(Modifier.ModifierType.Farming_BetterGenes) : 0f);
		if (num != 0f && fromItem.instanceData == null)
		{
			Genes.GenerateFavourableGenes(this, num);
			SendNetworkUpdate();
		}
		if ((Object)(object)parent != (Object)null && parent is PlanterBox planterBox)
		{
			planterBox.OnPlantInserted(this, deployedBy);
		}
	}
}

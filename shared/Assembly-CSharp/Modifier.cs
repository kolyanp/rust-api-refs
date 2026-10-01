using System;
using System.Text;
using Facepunch;
using ProtoBuf;

public class Modifier
{
	public enum ModifierType
	{
		Wood_Yield,
		Ore_Yield,
		Radiation_Resistance,
		Radiation_Exposure_Resistance,
		Max_Health,
		Scrap_Yield,
		MoveSpeed,
		ObscureVision,
		Warming,
		Cooling,
		CoreTemperatureMinAdjustment,
		CoreTemperatureMaxAdjustment,
		Crafting_Quality,
		VisionCare,
		MetabolismBooster,
		Harvesting,
		DigestionBoost,
		FishingBoost,
		Collectible_DoubleYield,
		Farming_BetterGenes,
		HorseGallopSpeed,
		HorseDungProductionBoost,
		Comfort,
		Clotting,
		HunterVision,
		Radiation,
		DigestionBoostTimeMod,
		Hunger_Capacity,
		LivestockHandling
	}

	public enum ModifierSource
	{
		Tea,
		Dart,
		Interaction,
		NegativeEffect,
		MedicalSyringe
	}

	public static Phrase WoodYieldPhrase = new Phrase("mod.woodyield", "Wood Yield");

	public static Phrase OreYieldPhrase = new Phrase("mod.oreyield", "Ore Yield");

	public static Phrase RadiationResistancePhrase = new Phrase("mod.radiationresistance", "Radiation Resistance");

	public static Phrase RadiationExposureResistancePhrase = new Phrase("mod.radiationexposureresistance", "Radiation Exposure Resistance");

	public static Phrase MaxHealthPhrase = new Phrase("mod.maxhealth", "Max Health");

	public static Phrase ScrapYieldPhrase = new Phrase("mod.scrapyield", "Scrap Yield");

	public static Phrase MoveSpeedPhrase = new Phrase("mod.movespeed", "Movement Speed");

	public static Phrase ObscureVisionPhrase = new Phrase("mod.ObscureVision", "Obscure Vision");

	public static Phrase RadiationPhrase = new Phrase("mod.radiation", "Radiation");

	public static Phrase CraftingQualityPhrase = new Phrase("mod.craftingquality", "Crafting Quality");

	public static Phrase WarmingPhrase = new Phrase("mod.warming", "Warming");

	public static Phrase CoolingPhrase = new Phrase("mod.cooling", "Cooling");

	public static Phrase CoreTempMinPhrase = new Phrase("mod.coretempmin", "Min Temp");

	public static Phrase CoreTempMaxPhrase = new Phrase("mod.coretempmax", "Max Temp");

	public static Phrase VisionCarePhrase = new Phrase("mod.VisionCare", "Vision Care");

	public static Phrase MetabolismBoosterPhrase = new Phrase("mod.MetabolismBooster", "Metabolism Booster");

	public static Phrase HarvestingPhrase = new Phrase("mod.Harvesting", "Harvesting");

	public static Phrase DigestionBoostPhrase = new Phrase("mod.DigestionBoost", "Digestion Boost");

	public static Phrase FishingBoostPhrase = new Phrase("mod.FishingBoost", "Fishing Boost");

	public static Phrase CollectibleYieldPhrase = new Phrase("mod.CollectibleDoubleYield", "Double Yield Chance");

	public static Phrase Farming_BetterGenesPhrase = new Phrase("mod.Farming_BetterGenes", "Better Genes Chance");

	public static Phrase HorseGallopSpeedPhrase = new Phrase("mod.HorseGallopSpeed", "Horse Gallop Speed");

	public static Phrase ComfortPhrase = new Phrase("mod.Comfort", "Comfort");

	public static Phrase ClottingPhrase = new Phrase("mod.Clotting", "Clotting");

	public static Phrase Temperature = new Phrase("mod.temperature", "Temperature: ");

	public static Phrase MinTemp = new Phrase("mod.mintemp", "Min temperature: ");

	public static Phrase MaxTemp = new Phrase("mod.maxtemp", "Max temperature: ");

	public static Phrase HunterVisionPhrase = new Phrase("mod.huntervision", "Hunter Vision");

	public static Phrase HungerCapacityPhrase = new Phrase("mod.hungercapacity", "Hunger Capacity");

	public static Phrase LivestockHandlingPhrase = new Phrase("mod.livestockhandling", "Livestock Handling");

	public static Phrase Farming_BetterGenesPanelPhrase = new Phrase("mod.Farming_BetterGenes.panel", "Increase");

	public ModifierType Type { get; private set; }

	public ModifierSource Source { get; private set; }

	public float Value { get; private set; } = 1f;

	public float Duration { get; private set; } = 10f;

	public double TimeRemaining { get; private set; }

	public bool Expired { get; private set; }

	public void Init(ModifierType type, ModifierSource source, float value, float duration, double remaining)
	{
		Type = type;
		Source = source;
		Value = value;
		Duration = duration;
		Expired = false;
		TimeRemaining = remaining;
	}

	public void Tick(BaseCombatEntity ownerEntity, double delta)
	{
		TimeRemaining -= delta;
		Expired = Duration > 0f && TimeRemaining <= 0.0;
	}

	public Modifier Save()
	{
		Modifier val = Pool.Get<Modifier>();
		val.type = (int)Type;
		val.source = (int)Source;
		val.value = Value;
		val.timeRemaining = TimeRemaining;
		val.duration = Duration;
		return val;
	}

	public void Load(Modifier m)
	{
		Type = (ModifierType)m.type;
		Source = (ModifierSource)m.source;
		Value = m.value;
		TimeRemaining = m.timeRemaining;
		Duration = m.duration;
	}

	public static Phrase GetPhraseForModType(ModifierType type)
	{
		switch (type)
		{
		case ModifierType.Wood_Yield:
			return WoodYieldPhrase;
		case ModifierType.Ore_Yield:
			return OreYieldPhrase;
		case ModifierType.Radiation_Resistance:
			return RadiationResistancePhrase;
		case ModifierType.Radiation_Exposure_Resistance:
			return RadiationExposureResistancePhrase;
		case ModifierType.Max_Health:
			return MaxHealthPhrase;
		case ModifierType.Scrap_Yield:
			return ScrapYieldPhrase;
		case ModifierType.MoveSpeed:
			return MoveSpeedPhrase;
		case ModifierType.ObscureVision:
			return ObscureVisionPhrase;
		case ModifierType.Crafting_Quality:
			return CraftingQualityPhrase;
		case ModifierType.Warming:
			return WarmingPhrase;
		case ModifierType.Cooling:
			return CoolingPhrase;
		case ModifierType.CoreTemperatureMinAdjustment:
			return CoreTempMinPhrase;
		case ModifierType.CoreTemperatureMaxAdjustment:
			return CoreTempMaxPhrase;
		case ModifierType.VisionCare:
			return VisionCarePhrase;
		case ModifierType.MetabolismBooster:
			return MetabolismBoosterPhrase;
		case ModifierType.Harvesting:
			return HarvestingPhrase;
		case ModifierType.DigestionBoost:
		case ModifierType.HorseDungProductionBoost:
			return DigestionBoostPhrase;
		case ModifierType.FishingBoost:
			return FishingBoostPhrase;
		case ModifierType.Collectible_DoubleYield:
			return CollectibleYieldPhrase;
		case ModifierType.Farming_BetterGenes:
			return Farming_BetterGenesPhrase;
		case ModifierType.HorseGallopSpeed:
			return HorseGallopSpeedPhrase;
		case ModifierType.Comfort:
			return ComfortPhrase;
		case ModifierType.Clotting:
			return ClottingPhrase;
		case ModifierType.HunterVision:
			return HunterVisionPhrase;
		case ModifierType.Radiation:
			return RadiationPhrase;
		case ModifierType.Hunger_Capacity:
			return HungerCapacityPhrase;
		case ModifierType.LivestockHandling:
			return LivestockHandlingPhrase;
		default:
			throw new ArgumentOutOfRangeException("type", type, $"Couldn't find a phrase for this modifier! {type}");
		}
	}

	public static Phrase GetPanelPhraseForModType(ModifierType type)
	{
		if (type == ModifierType.Farming_BetterGenes)
		{
			return Farming_BetterGenesPanelPhrase;
		}
		throw new ArgumentOutOfRangeException("type", type, $"Couldn't find a phrase for this modifier! {type}");
	}

	public static bool TryAppendModifierDescription(Modifier modifier, StringBuilder stringBuilder)
	{
		return TryAppendModifierDescription(modifier.Type, modifier.Value, stringBuilder);
	}

	public static bool TryAppendModifierDescription(ModifierType type, float value, StringBuilder stringBuilder)
	{
		switch (type)
		{
		case ModifierType.Warming:
			stringBuilder.Append(Temperature.translated);
			stringBuilder.Append("+");
			stringBuilder.Append(value);
			return true;
		case ModifierType.Cooling:
			stringBuilder.Append(Temperature.translated);
			stringBuilder.Append(value);
			return true;
		case ModifierType.CoreTemperatureMinAdjustment:
			stringBuilder.Append(MinTemp.translated);
			stringBuilder.Append(value);
			return true;
		case ModifierType.CoreTemperatureMaxAdjustment:
			stringBuilder.Append(MaxTemp.translated);
			stringBuilder.Append(value);
			return true;
		case ModifierType.Farming_BetterGenes:
			stringBuilder.Append(GetPhraseForModType(type).translated);
			return true;
		case ModifierType.HorseGallopSpeed:
		{
			if (value > 0f)
			{
				stringBuilder.Append("+");
			}
			float value2 = value * 60f * 60f / 1000f;
			stringBuilder.Append(value2);
			stringBuilder.Append("km/h ");
			return true;
		}
		default:
			return false;
		}
	}

	public bool IsHiddenModifier()
	{
		return Type == ModifierType.DigestionBoostTimeMod;
	}

	public bool HasNegativeSource()
	{
		if (Source != ModifierSource.Dart)
		{
			return Source == ModifierSource.NegativeEffect;
		}
		return true;
	}

	static Modifier()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected Obj, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected Obj, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected Obj, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected Obj, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected Obj, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected Obj, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected Obj, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected Obj, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected Obj, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected Obj, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected Obj, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected Obj, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Expected Obj, but got Unknown
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected Obj, but got Unknown
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Expected Obj, but got Unknown
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Expected Obj, but got Unknown
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected Obj, but got Unknown
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected Obj, but got Unknown
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Expected Obj, but got Unknown
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Expected Obj, but got Unknown
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Expected Obj, but got Unknown
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Expected Obj, but got Unknown
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Expected Obj, but got Unknown
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Expected Obj, but got Unknown
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Expected Obj, but got Unknown
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Expected Obj, but got Unknown
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Expected Obj, but got Unknown
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Expected Obj, but got Unknown
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Expected Obj, but got Unknown
	}
}

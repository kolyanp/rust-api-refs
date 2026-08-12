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
		DigestionBoostTimeMod
	}

	public enum ModifierSource
	{
		Tea,
		Dart,
		Interaction,
		NegativeEffect,
		MedicalSyringe
	}

	public static Phrase WoodYieldPhrase;

	public static Phrase OreYieldPhrase;

	public static Phrase RadiationResistancePhrase;

	public static Phrase RadiationExposureResistancePhrase;

	public static Phrase MaxHealthPhrase;

	public static Phrase ScrapYieldPhrase;

	public static Phrase MoveSpeedPhrase;

	public static Phrase ObscureVisionPhrase;

	public static Phrase RadiationPhrase;

	public static Phrase CraftingQualityPhrase;

	public static Phrase WarmingPhrase;

	public static Phrase CoolingPhrase;

	public static Phrase CoreTempMinPhrase;

	public static Phrase CoreTempMaxPhrase;

	public static Phrase VisionCarePhrase;

	public static Phrase MetabolismBoosterPhrase;

	public static Phrase HarvestingPhrase;

	public static Phrase DigestionBoostPhrase;

	public static Phrase FishingBoostPhrase;

	public static Phrase CollectibleYieldPhrase;

	public static Phrase Farming_BetterGenesPhrase;

	public static Phrase HorseGallopSpeedPhrase;

	public static Phrase ComfortPhrase;

	public static Phrase ClottingPhrase;

	public static Phrase Temperature;

	public static Phrase MinTemp;

	public static Phrase MaxTemp;

	public static Phrase HunterVisionPhrase;

	public static Phrase Farming_BetterGenesPanelPhrase;

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
		Modifier obj = Pool.Get<Modifier>();
		obj.type = (int)Type;
		obj.source = (int)Source;
		obj.value = Value;
		obj.timeRemaining = TimeRemaining;
		obj.duration = Duration;
		return obj;
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
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Expected O, but got Unknown
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected O, but got Unknown
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Expected O, but got Unknown
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Expected O, but got Unknown
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected O, but got Unknown
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Expected O, but got Unknown
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Expected O, but got Unknown
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Expected O, but got Unknown
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Expected O, but got Unknown
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Expected O, but got Unknown
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Expected O, but got Unknown
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Expected O, but got Unknown
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Expected O, but got Unknown
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Expected O, but got Unknown
		WoodYieldPhrase = new Phrase("mod.woodyield", "Wood Yield");
		OreYieldPhrase = new Phrase("mod.oreyield", "Ore Yield");
		RadiationResistancePhrase = new Phrase("mod.radiationresistance", "Radiation Resistance");
		RadiationExposureResistancePhrase = new Phrase("mod.radiationexposureresistance", "Radiation Exposure Resistance");
		MaxHealthPhrase = new Phrase("mod.maxhealth", "Max Health");
		ScrapYieldPhrase = new Phrase("mod.scrapyield", "Scrap Yield");
		MoveSpeedPhrase = new Phrase("mod.movespeed", "Movement Speed");
		ObscureVisionPhrase = new Phrase("mod.ObscureVision", "Obscure Vision");
		RadiationPhrase = new Phrase("mod.radiation", "Radiation");
		CraftingQualityPhrase = new Phrase("mod.craftingquality", "Crafting Quality");
		WarmingPhrase = new Phrase("mod.warming", "Warming");
		CoolingPhrase = new Phrase("mod.cooling", "Cooling");
		CoreTempMinPhrase = new Phrase("mod.coretempmin", "Min Temp");
		CoreTempMaxPhrase = new Phrase("mod.coretempmax", "Max Temp");
		VisionCarePhrase = new Phrase("mod.VisionCare", "Vision Care");
		MetabolismBoosterPhrase = new Phrase("mod.MetabolismBooster", "Metabolism Booster");
		HarvestingPhrase = new Phrase("mod.Harvesting", "Harvesting");
		DigestionBoostPhrase = new Phrase("mod.DigestionBoost", "Digestion Boost");
		FishingBoostPhrase = new Phrase("mod.FishingBoost", "Fishing Boost");
		CollectibleYieldPhrase = new Phrase("mod.CollectibleDoubleYield", "Double Yield Chance");
		Farming_BetterGenesPhrase = new Phrase("mod.Farming_BetterGenes", "Better Genes Chance");
		HorseGallopSpeedPhrase = new Phrase("mod.HorseGallopSpeed", "Horse Gallop Speed");
		ComfortPhrase = new Phrase("mod.Comfort", "Comfort");
		ClottingPhrase = new Phrase("mod.Clotting", "Clotting");
		Temperature = new Phrase("mod.temperature", "Temperature: ");
		MinTemp = new Phrase("mod.mintemp", "Min temperature: ");
		MaxTemp = new Phrase("mod.maxtemp", "Max temperature: ");
		HunterVisionPhrase = new Phrase("mod.huntervision", "Hunter Vision");
		Farming_BetterGenesPanelPhrase = new Phrase("mod.Farming_BetterGenes.panel", "Increase");
	}
}

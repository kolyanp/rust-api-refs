using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Plant Properties")]
public class PlantProperties : ScriptableObject
{
	public enum State
	{
		Seed,
		Seedling,
		Sapling,
		Crossbreed,
		Mature,
		Fruiting,
		Ripe,
		Dying
	}

	[Serializable]
	public struct Stage
	{
		public State nextState;

		public float lifeLength;

		public float health;

		public float resources;

		public float yield;

		public GameObjectRef skinObject;

		public bool IgnoreConditions;

		public float lifeLengthSeconds => lifeLength * 60f;
	}

	public Phrase Description;

	public GrowableGeneProperties Genes;

	[ArrayIndexIsEnum(enumType = typeof(State))]
	public Stage[] stages;

	[Header("Metabolism")]
	public AnimationCurve timeOfDayHappiness;

	public AnimationCurve temperatureHappiness;

	public AnimationCurve temperatureWaterRequirementMultiplier;

	public AnimationCurve fruitVisualScaleCurve;

	public int MaxSeasons;

	public float WaterIntake;

	public float OptimalLightQuality;

	public float OptimalWaterQuality;

	public float OptimalGroundQuality;

	public float OptimalTemperatureQuality;

	[Header("Harvesting")]
	public BaseEntity.Menu.Option pickOption;

	public BaseEntity.Menu.Option pickAllOption;

	public BaseEntity.Menu.Option eatOption;

	public ItemDefinition pickupItem;

	public BaseEntity.Menu.Option cloneOption;

	public BaseEntity.Menu.Option cloneAllOption;

	public BaseEntity.Menu.Option removeDyingOption;

	public BaseEntity.Menu.Option removeDyingAllOption;

	public ItemDefinition removeDyingItem;

	public GameObjectRef removeDyingEffect;

	public int pickupMultiplier;

	public GameObjectRef pickEffect;

	public int maxHarvests;

	public bool disappearAfterHarvest;

	[Header("Seeds")]
	public GameObjectRef CrossBreedEffect;

	public ItemDefinition SeedItem;

	public ItemDefinition CloneItem;

	public int BaseCloneCount;

	[Header("Market")]
	public int BaseMarketValue;

	public PlantProperties()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Expected O, but got Unknown
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Expected O, but got Unknown
		stages = new Stage[8];
		timeOfDayHappiness = new AnimationCurve((Keyframe[])(object)new Keyframe[3]
		{
			new Keyframe(0f, 0f),
			new Keyframe(12f, 1f),
			new Keyframe(24f, 0f)
		});
		temperatureHappiness = new AnimationCurve((Keyframe[])(object)new Keyframe[5]
		{
			new Keyframe(-10f, -1f),
			new Keyframe(1f, 0f),
			new Keyframe(30f, 1f),
			new Keyframe(50f, 0f),
			new Keyframe(80f, -1f)
		});
		temperatureWaterRequirementMultiplier = new AnimationCurve((Keyframe[])(object)new Keyframe[5]
		{
			new Keyframe(-10f, 1f),
			new Keyframe(0f, 1f),
			new Keyframe(30f, 1f),
			new Keyframe(50f, 1f),
			new Keyframe(80f, 1f)
		});
		fruitVisualScaleCurve = new AnimationCurve((Keyframe[])(object)new Keyframe[3]
		{
			new Keyframe(0f, 0f),
			new Keyframe(0.75f, 1f),
			new Keyframe(1f, 0f)
		});
		MaxSeasons = 1;
		WaterIntake = 20f;
		OptimalLightQuality = 1f;
		OptimalWaterQuality = 1f;
		OptimalGroundQuality = 1f;
		OptimalTemperatureQuality = 1f;
		pickupMultiplier = 1;
		maxHarvests = 1;
		BaseCloneCount = 1;
		BaseMarketValue = 10;
		((ScriptableObject)this)._002Ector();
	}
}

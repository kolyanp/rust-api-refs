using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Ambience Definition")]
public class AmbienceDefinition : ScriptableObject
{
	[Serializable]
	public class ValueRange
	{
		public float min;

		public float max;

		public ValueRange(float min, float max)
		{
			this.min = min;
			this.max = max;
		}
	}

	[Header("Sound")]
	public List<SoundDefinition> sounds;

	[Horizontal(2, -1)]
	public ValueRange stingFrequency;

	[Header("Environment")]
	[InspectorFlags]
	public Enum biomes;

	[InspectorFlags]
	public Enum topologies;

	public EnvironmentType environmentType;

	public bool useEnvironmentType;

	public AnimationCurve time;

	[Horizontal(2, -1)]
	public ValueRange rain;

	[Horizontal(2, -1)]
	public ValueRange wind;

	[Horizontal(2, -1)]
	public ValueRange snow;

	[Horizontal(2, -1)]
	public ValueRange waves;

	public AmbienceDefinition()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		stingFrequency = new ValueRange(15f, 30f);
		biomes = (Enum)(-1);
		topologies = (Enum)(-1);
		environmentType = EnvironmentType.Underground;
		time = AnimationCurve.Linear(0f, 0f, 24f, 0f);
		rain = new ValueRange(0f, 1f);
		wind = new ValueRange(0f, 1f);
		snow = new ValueRange(0f, 1f);
		waves = new ValueRange(0f, 10f);
		((ScriptableObject)this)._002Ector();
	}
}

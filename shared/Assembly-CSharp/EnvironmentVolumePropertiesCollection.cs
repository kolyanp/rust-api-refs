using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Environment Volume Properties Collection")]
public class EnvironmentVolumePropertiesCollection : ScriptableObject
{
	[Serializable]
	public class EnvironmentMultiplier
	{
		public EnvironmentType Type;

		public float Multiplier;
	}

	[Serializable]
	public class OceanParameters
	{
		[Header("Lighting")]
		public AnimationCurve TransitionCurveLighting;

		[Range(0f, 1f)]
		public float DirectionalLightMultiplier;

		[Range(0f, 1f)]
		public float AmbientLightMultiplier;

		[Range(0f, 1f)]
		public float ReflectionMultiplier;

		[Header("Sun/Moon")]
		public AnimationCurve TransitionCurveSunMoon;

		[Range(0f, 1f)]
		public float SunMeshBrightnessMultiplier;

		[Range(0f, 1f)]
		public float MoonMeshBrightnessMultiplier;

		[Header("Atmosphere")]
		public AnimationCurve TransitionCurveAtmosphere;

		[Range(0f, 1f)]
		public float AtmosphereBrightnessMultiplier;

		[Header("Colors")]
		public AnimationCurve TransitionCurve;

		[Range(0f, 1f)]
		public float LightColorMultiplier;

		public Color LightColor;

		[Range(0f, 1f)]
		public float SunRayColorMultiplier;

		public Color SunRayColor;

		[Range(0f, 1f)]
		public float MoonRayColorMultiplier;

		public Color MoonRayColor;

		public OceanParameters()
		{
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			TransitionCurveLighting = AnimationCurve.Linear(0f, 0f, 40f, 1f);
			DirectionalLightMultiplier = 0.25f;
			ReflectionMultiplier = 1f;
			TransitionCurveSunMoon = AnimationCurve.Linear(0f, 0f, 40f, 1f);
			SunMeshBrightnessMultiplier = 1f;
			MoonMeshBrightnessMultiplier = 1f;
			TransitionCurveAtmosphere = AnimationCurve.Linear(0f, 0f, 40f, 1f);
			AtmosphereBrightnessMultiplier = 1f;
			TransitionCurve = AnimationCurve.Linear(0f, 0f, 40f, 1f);
			LightColorMultiplier = 1f;
			LightColor = Color.black;
			SunRayColorMultiplier = 1f;
			SunRayColor = Color.black;
			MoonRayColorMultiplier = 1f;
			MoonRayColor = Color.black;
			base._002Ector();
		}
	}

	public float TransitionSpeed;

	public LayerMask ReflectionMask;

	[Horizontal(1, 0)]
	public EnvironmentMultiplier[] ReflectionMultipliers;

	public float DefaultReflectionMultiplier;

	[Horizontal(1, 0)]
	public EnvironmentMultiplier[] AmbientMultipliers;

	public float DefaultAmbientMultiplier;

	public OceanParameters OceanOverrides;

	public OceanParameters OceanOverridesDeepSea;

	public EnvironmentVolumePropertiesCollection()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		TransitionSpeed = 1f;
		ReflectionMask = LayerMask.op_Implicit(1084293120);
		DefaultReflectionMultiplier = 1f;
		DefaultAmbientMultiplier = 1f;
		((ScriptableObject)this)._002Ector();
	}
}

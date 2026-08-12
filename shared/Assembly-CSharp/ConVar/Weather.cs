using System;
using System.Globalization;
using System.Text;
using Facepunch;
using UnityEngine;

namespace ConVar;

[Factory("weather")]
public class Weather : ConsoleSystem
{
	private static readonly string[] SnapshotConVars = new string[4] { "global.admintime", "env.skyrotation", "graphics.biomefogdensityscale", "global.adminambientmultiplier" };

	[ServerVar(Help = "(Generated) Controls how wet surfaces become during rain; higher values cause characters and world objects to appear more soaked when it is raining")]
	public static float wetness_rain = 0.4f;

	[ServerVar(Help = "(Generated) Controls how wet surfaces become during snow; affects surface wetness shaders independently from rain wetness")]
	public static float wetness_snow = 0.2f;

	[ServerVar(Saved = true, Help = "Number of in-game hours after a wipe during which rain and storms are suppressed (0 to disable)")]
	public static float rain_grace_period = 18f;

	[ReplicatedVar(Help = "Whether the post-wipe rain grace period is currently active (set automatically by the server)", Default = "false")]
	public static bool rain_grace_active = false;

	[ReplicatedVar(Default = "-1")]
	public static float ocean_time = -1f;

	[ReplicatedVar(Default = "1")]
	public static float clear_chance
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return 1f;
			}
			return SingletonComponent<Climate>.Instance.Weather.ClearChance;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.Weather.ClearChance = Mathf.Clamp01(value);
			}
		}
	}

	[ReplicatedVar(Default = "0")]
	public static float dust_chance
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return 0f;
			}
			return SingletonComponent<Climate>.Instance.Weather.DustChance;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.Weather.DustChance = Mathf.Clamp01(value);
			}
		}
	}

	[ReplicatedVar(Default = "0")]
	public static float fog_chance
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return 0f;
			}
			return SingletonComponent<Climate>.Instance.Weather.FogChance;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.Weather.FogChance = Mathf.Clamp01(value);
			}
		}
	}

	[ReplicatedVar(Default = "0")]
	public static float overcast_chance
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return 0f;
			}
			return SingletonComponent<Climate>.Instance.Weather.OvercastChance;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.Weather.OvercastChance = Mathf.Clamp01(value);
			}
		}
	}

	[ReplicatedVar(Default = "0")]
	public static float storm_chance
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return 0f;
			}
			return SingletonComponent<Climate>.Instance.Weather.StormChance;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.Weather.StormChance = Mathf.Clamp01(value);
			}
		}
	}

	[ReplicatedVar(Default = "0")]
	public static float rain_chance
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return 0f;
			}
			return SingletonComponent<Climate>.Instance.Weather.RainChance;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.Weather.RainChance = Mathf.Clamp01(value);
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float rain
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Rain;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Rain = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float wind
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Wind;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Wind = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float thunder
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Thunder;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Thunder = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float rainbow
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Rainbow;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Rainbow = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float fog
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.Fogginess;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.Fogginess = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float atmosphere_rayleigh
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.RayleighMultiplier;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.RayleighMultiplier = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float atmosphere_mie
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.MieMultiplier;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.MieMultiplier = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float atmosphere_brightness
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.Brightness;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.Brightness = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float atmosphere_contrast
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.Contrast;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.Contrast = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float atmosphere_directionality
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.Directionality;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Atmosphere.Directionality = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float cloud_size
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Size;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Size = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float cloud_opacity
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Opacity;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Opacity = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float cloud_coverage
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Coverage;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Coverage = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float cloud_sharpness
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Sharpness;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Sharpness = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float cloud_coloring
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Coloring;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Coloring = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float cloud_attenuation
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Attenuation;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Attenuation = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float cloud_saturation
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Saturation;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Saturation = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float cloud_scattering
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Scattering;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Scattering = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float cloud_brightness
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Brightness;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.Clouds.Brightness = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float ocean_scale
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.OceanScale;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.OceanScale = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float ambient_light_multiplier
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.AmbientLightMultiplier;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.AmbientLightMultiplier = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float directional_light_multiplier
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.DirectionalLightMultiplier;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.DirectionalLightMultiplier = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float reflection_multiplier
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.ReflectionMultiplier;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.ReflectionMultiplier = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float sun_mesh_brightness_multiplier
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.SunMeshBrightnessMultiplier;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.SunMeshBrightnessMultiplier = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float moon_mesh_brightness_multiplier
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.MoonMeshBrightnessMultiplier;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.MoonMeshBrightnessMultiplier = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float fog_multiplier
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.FogMultiplier;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.FogMultiplier = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float biome_fog_distance_curve
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.BiomeFogDistanceCurve;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.BiomeFogDistanceCurve = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float biome_fog_ambient_saturation_mult
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.BiomeFogAmbientSaturationMult;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.BiomeFogAmbientSaturationMult = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float atmosphere_fog_height_falloff
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.AtmosphereFogHeightFalloff;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.AtmosphereFogHeightFalloff = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float vclouds_sun_color_scale
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.VolumeCloudsSunColorScale;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.VolumeCloudsSunColorScale = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float vclouds_moon_color_scale
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.VolumeCloudsMoonColorScale;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.VolumeCloudsMoonColorScale = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float arid_fog_ambient_intensity_mult
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.AridFogMults.AmbientIntensityMult;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.AridFogMults.AmbientIntensityMult = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float arid_fog_light_boost_mult
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.AridFogMults.LightBoostMult;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.AridFogMults.LightBoostMult = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float arid_fog_ramp_start
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.AridFogMults.FogRampStartDist;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.AridFogMults.FogRampStartDist = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float arid_fog_ramp_end
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.AridFogMults.FogRampEndDist;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.AridFogMults.FogRampEndDist = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float temperate_fog_ambient_intensity_mult
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.TemperateFogMults.AmbientIntensityMult;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.TemperateFogMults.AmbientIntensityMult = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float temperate_fog_light_boost_mult
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.TemperateFogMults.LightBoostMult;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.TemperateFogMults.LightBoostMult = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float temperate_fog_ramp_start
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.TemperateFogMults.FogRampStartDist;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.TemperateFogMults.FogRampStartDist = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float temperate_fog_ramp_end
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.TemperateFogMults.FogRampEndDist;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.TemperateFogMults.FogRampEndDist = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float tundra_fog_ambient_intensity_mult
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.TundraFogMults.AmbientIntensityMult;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.TundraFogMults.AmbientIntensityMult = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float tundra_fog_light_boost_mult
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.TundraFogMults.LightBoostMult;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.TundraFogMults.LightBoostMult = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float tundra_fog_ramp_start
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.TundraFogMults.FogRampStartDist;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.TundraFogMults.FogRampStartDist = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float tundra_fog_ramp_end
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.TundraFogMults.FogRampEndDist;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.TundraFogMults.FogRampEndDist = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float arctic_fog_ambient_intensity_mult
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.ArcticFogMults.AmbientIntensityMult;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.ArcticFogMults.AmbientIntensityMult = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float arctic_fog_light_boost_mult
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.ArcticFogMults.LightBoostMult;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.ArcticFogMults.LightBoostMult = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float arctic_fog_ramp_start
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.ArcticFogMults.FogRampStartDist;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.ArcticFogMults.FogRampStartDist = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float arctic_fog_ramp_end
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.ArcticFogMults.FogRampEndDist;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.ArcticFogMults.FogRampEndDist = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float jungle_fog_ambient_intensity_mult
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.JungleFogMults.AmbientIntensityMult;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.JungleFogMults.AmbientIntensityMult = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float jungle_fog_light_boost_mult
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.JungleFogMults.LightBoostMult;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.JungleFogMults.LightBoostMult = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float jungle_fog_ramp_start
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.JungleFogMults.FogRampStartDist;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.JungleFogMults.FogRampStartDist = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float jungle_fog_ramp_end
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.JungleFogMults.FogRampEndDist;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.JungleFogMults.FogRampEndDist = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static int cloud_config
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.OverrideCloudConfig;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.OverrideCloudConfig = value;
				if (value < 0)
				{
					SingletonComponent<Climate>.Instance.WeatherOverrides.VolumeCloudsConfigs = null;
				}
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float atmosphere_fog_ramp_start_distance
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.AtmosphereFogRampStartDistance;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.AtmosphereFogRampStartDistance = value;
			}
		}
	}

	[ReplicatedVar(Default = "-1")]
	public static float atmosphere_fog_ramp_end_distance
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				return -1f;
			}
			return SingletonComponent<Climate>.Instance.WeatherOverrides.AtmosphereFogRampEndDistance;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
			{
				SingletonComponent<Climate>.Instance.WeatherOverrides.AtmosphereFogRampEndDistance = value;
			}
		}
	}

	[ClientVar(Help = "(Generated) Loads and applies a named weather preset to the climate system; admin/developer only; server replicates the change to all clients")]
	[ServerVar(Help = "(Generated) Loads and applies a named weather preset to the climate system; admin/developer only; server replicates the change to all clients")]
	public static void load(Arg args)
	{
		if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
		{
			return;
		}
		string name = args.GetString(0);
		if (string.IsNullOrEmpty(name))
		{
			args.ReplyWith("Weather preset name invalid.");
			return;
		}
		WeatherPreset weatherPreset = Array.Find(SingletonComponent<Climate>.Instance.WeatherPresets, (WeatherPreset x) => StringEx.Contains(((Object)x).name, name, CompareOptions.IgnoreCase));
		if ((Object)(object)weatherPreset == (Object)null)
		{
			args.ReplyWith("Weather preset not found: " + name);
			return;
		}
		SingletonComponent<Climate>.Instance.WeatherOverrides.Set(weatherPreset);
		SingletonComponent<Climate>.Instance.WeatherOverrides.SetVolumeCloudsConfigs(weatherPreset);
		if (args.IsServerside)
		{
			ServerMgr.SendReplicatedVars("weather.");
		}
	}

	public static WeatherPreset GetWeatherPreset(string withName)
	{
		return Array.Find(SingletonComponent<Climate>.Instance.WeatherPresets, (WeatherPreset x) => StringEx.Contains(((Object)x).name, withName, CompareOptions.IgnoreCase));
	}

	[ClientVar(Help = "(Generated) Prints the current cloud system world position and layer offsets to the console; useful for debugging cloud layer alignment on the map")]
	public static void debug_cloud_position(Arg args)
	{
	}

	[ServerVar(Help = "(Generated) Loads a named volumetric cloud configuration and applies it to the climate override; admin/developer only; server replicates to clients")]
	[ClientVar(Help = "(Generated) Loads a named volumetric cloud configuration and applies it to the climate override; admin/developer only; server replicates to clients")]
	public static void load_cloud_config(Arg args)
	{
		if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
		{
			return;
		}
		string name = args.GetString(0);
		if (string.IsNullOrEmpty(name))
		{
			return;
		}
		VolumeCloudsConfig volumeCloudsConfig = Array.Find(SingletonComponent<Climate>.Instance.AllCloudConfigs, (VolumeCloudsConfig x) => StringEx.Contains(((Object)x).name, name, CompareOptions.IgnoreCase));
		if ((Object)(object)volumeCloudsConfig == (Object)null)
		{
			args.ReplyWith("Cloud config not found: " + name);
			return;
		}
		WeatherPreset weatherOverrides = SingletonComponent<Climate>.Instance.WeatherOverrides;
		weatherOverrides.VolumeCloudsConfigs = new VolumeCloudsConfig[1] { volumeCloudsConfig };
		int overrideCloudConfig = Array.IndexOf(SingletonComponent<Climate>.Instance.AllCloudConfigs, volumeCloudsConfig);
		weatherOverrides.OverrideCloudConfig = overrideCloudConfig;
		if (args.IsServerside)
		{
			ServerMgr.SendReplicatedVars("weather.");
		}
	}

	[ServerVar(Help = "(Generated) Lists all available volumetric cloud configuration asset names registered in the Climate instance")]
	[ClientVar(Help = "(Generated) Lists all available volumetric cloud configuration asset names registered in the Climate instance")]
	public static void list_cloud_configs(Arg args)
	{
		if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
		{
			VolumeCloudsConfig[] allCloudConfigs = SingletonComponent<Climate>.Instance.AllCloudConfigs;
			StringBuilder stringBuilder = new StringBuilder();
			VolumeCloudsConfig[] array = allCloudConfigs;
			foreach (VolumeCloudsConfig volumeCloudsConfig in array)
			{
				stringBuilder.AppendLine(((Object)volumeCloudsConfig).name);
			}
			args.ReplyWith(stringBuilder.ToString());
		}
	}

	[ClientVar(Help = "(Generated) Clears the volumetric cloud configuration override and resets cloud settings to the dynamic weather system; server replicates to clients")]
	[ServerVar(Help = "(Generated) Clears the volumetric cloud configuration override and resets cloud settings to the dynamic weather system; server replicates to clients")]
	public static void reset_cloud_config(Arg args)
	{
		if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
		{
			WeatherPreset weatherOverrides = SingletonComponent<Climate>.Instance.WeatherOverrides;
			weatherOverrides.VolumeCloudsConfigs = null;
			weatherOverrides.OverrideCloudConfig = -1;
			if (args.IsServerside)
			{
				ServerMgr.SendReplicatedVars("weather.");
			}
		}
	}

	[ServerVar(Help = "(Generated) Resets all weather overrides and cloud configurations, restoring the dynamic weather system; admin/developer only; server replicates to clients")]
	[ClientVar(Help = "(Generated) Resets all weather overrides and cloud configurations, restoring the dynamic weather system; admin/developer only; server replicates to clients")]
	public static void reset(Arg args)
	{
		if (Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
		{
			SingletonComponent<Climate>.Instance.WeatherOverrides.Reset();
			SingletonComponent<Climate>.Instance.WeatherOverrides.VolumeCloudsConfigs = null;
			if (args.IsServerside)
			{
				ServerMgr.SendReplicatedVars("weather.");
			}
		}
	}

	[ClientVar(Help = "(Generated) Prints a detailed report of the current weather state including fog, rain, wind, cloud, and all climate parameter values; admin/developer only")]
	[ServerVar(Help = "(Generated) Prints a detailed report of the current weather state including fog, rain, wind, cloud, and all climate parameter values; admin/developer only")]
	public static void report(Arg args)
	{
		if (!Object.op_Implicit((Object)(object)SingletonComponent<Climate>.Instance))
		{
			return;
		}
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.AddColumn(((Object)SingletonComponent<Climate>.Instance.WeatherStatePrevious).name);
			val.AddColumn("|");
			val.AddColumn(((Object)SingletonComponent<Climate>.Instance.WeatherStateTarget).name);
			val.AddColumn("|");
			val.AddColumn(((Object)SingletonComponent<Climate>.Instance.WeatherStateNext).name);
			int num = Mathf.RoundToInt(SingletonComponent<Climate>.Instance.WeatherStateBlend * 100f);
			if (num < 100)
			{
				val.AddRow(new string[5]
				{
					"fading out (" + (100 - num) + "%)",
					"|",
					"fading in (" + num + "%)",
					"|",
					"up next"
				});
			}
			else
			{
				val.AddRow(new string[5] { "previous", "|", "current", "|", "up next" });
			}
			args.ReplyWith(((object)val).ToString() + Environment.NewLine + ((object)SingletonComponent<Climate>.Instance.WeatherState).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ClientVar(ClientAdmin = true, Help = "(Generated) Saves the current environment and weather snapshot to cfg/<name>.cfg so it can later be restored with exec")]
	public static void savetofile(Arg args)
	{
	}
}

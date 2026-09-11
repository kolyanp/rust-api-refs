using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Volume Clouds/Cloud Config")]
public class VolumeCloudsConfig : ScriptableObject
{
	public VolumeCloudsWeatherLayerConfig WeatherLayer;

	[Header("Detail")]
	[Range(0f, 1f)]
	public float Billows;

	[Range(0f, 3f)]
	public float BillowsGamma;

	[Range(0f, 2f)]
	public float BillowsFrequencyCurve;

	[Range(0f, 1f)]
	public float Wisps;

	[Range(0f, 3f)]
	public float WispsGamma;

	[Range(0f, 2f)]
	public float WispsFrequencyCurve;

	[Range(0f, 2f)]
	public float DetailTypeDensityCurve;

	[Range(0.001f, 1f)]
	public float DetailTypeHeightTransition;

	[Range(0f, 2f)]
	public float CurlNoiseScale;

	public float CurlNoiseStrength;

	[Header("Density")]
	[Range(0f, 1f)]
	public float DensityScale;

	[Range(0f, 1f)]
	public float DensityCurve;

	[Range(0f, 1f)]
	public float WispsDensitySoftening;

	[Header("Multiple Scattering")]
	public float MSIntensity;

	[Range(0f, 1f)]
	public float MSAbsorption;

	[Range(0f, 2f)]
	public float MSDepthFalloff;

	[Range(0f, 1f)]
	public float MSMinDepth;

	[Header("Direct Scattering")]
	public Gradient SunColorScale;

	[Range(-1f, 1f)]
	public float SunEccentricity1;

	[Range(-1f, 1f)]
	public float SunEccentricity2;

	public Gradient MoonColorScale;

	[Range(-1f, 1f)]
	public float MoonEccentricity1;

	[Range(-1f, 1f)]
	public float MoonEccentricity2;

	[Range(0f, 2f)]
	public float MoonSaturation;

	[Range(0f, 3f)]
	public float DirectScatterBrightness;

	[Range(0f, 3f)]
	public float DirectScatterContrast;

	[Range(0f, 2f)]
	public float Absorption;

	[Header("Ambient Scattering")]
	public Gradient AmbientColorScale;

	[Range(0f, 1f)]
	public float AmbientSaturation;

	[Range(0f, 2f)]
	public float AmbientScatteringFalloff;

	[Range(0f, 3f)]
	public float AmbientScatterBrightness;

	[Range(0f, 3f)]
	public float AmbientScatterContrast;

	[Range(0f, 1f)]
	[Header("Other")]
	public float CoverageScale;

	[Range(0f, 1f)]
	public float CloudTypeTop;

	[Range(0f, 1f)]
	public float CloudTypeBottom;

	[Range(0f, 1f)]
	public float VerticalProfileTopEnd;

	[Range(0f, 1f)]
	public float VerticalProfileTopStart;

	[Range(0f, 1f)]
	public float VerticalProfileBottomStart;

	[Range(0f, 1f)]
	public float VerticalProfileBottomEnd;

	public float WindShear;

	public Vector2 WindVector;

	[Range(0f, 1f)]
	public float ShadowDensityScale;

	[Range(0f, 1f)]
	public float AtmosphereShadowDensityScale;

	[Min(0f)]
	public float HazeDensity;

	[Min(0f)]
	public float HazeHeightFalloff;

	[Range(0f, 1f)]
	public float HorizonBuffer;

	public float EvalSunColorScale { get; set; }

	public float EvalMoonColorScale { get; set; }

	public float EvalAmbientColorScale { get; set; }

	public void CopyWeatherGen(VolumeCloudsConfig copy)
	{
		WeatherLayer.CopyFrom(copy.WeatherLayer);
	}

	public void CopyFrom(VolumeCloudsConfig copy)
	{
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		CopyWeatherGen(copy);
		Billows = copy.Billows;
		BillowsGamma = copy.BillowsGamma;
		BillowsFrequencyCurve = copy.BillowsFrequencyCurve;
		Wisps = copy.Wisps;
		WispsGamma = copy.WispsGamma;
		WispsFrequencyCurve = copy.WispsFrequencyCurve;
		DetailTypeDensityCurve = copy.DetailTypeDensityCurve;
		DetailTypeHeightTransition = copy.DetailTypeHeightTransition;
		CurlNoiseScale = copy.CurlNoiseScale;
		CurlNoiseStrength = copy.CurlNoiseStrength;
		DensityScale = copy.DensityScale;
		DensityCurve = copy.DensityCurve;
		WispsDensitySoftening = copy.WispsDensitySoftening;
		SunEccentricity1 = copy.SunEccentricity1;
		SunEccentricity2 = copy.SunEccentricity2;
		AmbientScatteringFalloff = copy.AmbientScatteringFalloff;
		Absorption = copy.Absorption;
		SunColorScale.SetKeys(copy.SunColorScale.colorKeys, copy.SunColorScale.alphaKeys);
		AmbientColorScale.SetKeys(copy.AmbientColorScale.colorKeys, copy.AmbientColorScale.alphaKeys);
		AmbientSaturation = copy.AmbientSaturation;
		CoverageScale = copy.CoverageScale;
		MSIntensity = copy.MSIntensity;
		MSAbsorption = copy.MSAbsorption;
		MSDepthFalloff = copy.MSDepthFalloff;
		MSMinDepth = copy.MSMinDepth;
		CloudTypeTop = copy.CloudTypeTop;
		CloudTypeBottom = copy.CloudTypeBottom;
		MoonColorScale.SetKeys(copy.MoonColorScale.colorKeys, copy.MoonColorScale.alphaKeys);
		MoonEccentricity1 = copy.MoonEccentricity1;
		MoonEccentricity2 = copy.MoonEccentricity2;
		MoonSaturation = copy.MoonSaturation;
		VerticalProfileTopEnd = copy.VerticalProfileTopEnd;
		VerticalProfileTopStart = copy.VerticalProfileTopStart;
		VerticalProfileBottomStart = copy.VerticalProfileBottomStart;
		VerticalProfileBottomEnd = copy.VerticalProfileBottomEnd;
		DirectScatterBrightness = copy.DirectScatterBrightness;
		DirectScatterContrast = copy.DirectScatterContrast;
		AmbientScatterBrightness = copy.AmbientScatterBrightness;
		AmbientScatterContrast = copy.AmbientScatterContrast;
		WindShear = copy.WindShear;
		WindVector = copy.WindVector;
		ShadowDensityScale = copy.ShadowDensityScale;
		AtmosphereShadowDensityScale = copy.AtmosphereShadowDensityScale;
		HazeDensity = copy.HazeDensity;
		HazeHeightFalloff = copy.HazeHeightFalloff;
		HorizonBuffer = copy.HorizonBuffer;
		EvalSunColorScale = copy.EvalSunColorScale;
		EvalMoonColorScale = copy.EvalMoonColorScale;
		EvalAmbientColorScale = copy.EvalAmbientColorScale;
	}

	public void Lerp(VolumeCloudsConfig a, VolumeCloudsConfig b, float t)
	{
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		Billows = Mathf.Lerp(a.Billows, b.Billows, t);
		BillowsGamma = Mathf.Lerp(a.BillowsGamma, b.BillowsGamma, t);
		BillowsFrequencyCurve = Mathf.Lerp(a.BillowsFrequencyCurve, b.BillowsFrequencyCurve, t);
		Wisps = Mathf.Lerp(a.Wisps, b.Wisps, t);
		WispsGamma = Mathf.Lerp(a.WispsGamma, b.WispsGamma, t);
		WispsFrequencyCurve = Mathf.Lerp(a.WispsFrequencyCurve, b.WispsFrequencyCurve, t);
		DetailTypeDensityCurve = Mathf.Lerp(a.DetailTypeDensityCurve, b.DetailTypeDensityCurve, t);
		DetailTypeHeightTransition = Mathf.Lerp(a.DetailTypeHeightTransition, b.DetailTypeHeightTransition, t);
		CurlNoiseScale = Mathf.Lerp(a.CurlNoiseScale, b.CurlNoiseScale, t);
		CurlNoiseStrength = Mathf.Lerp(a.CurlNoiseStrength, b.CurlNoiseStrength, t);
		DensityScale = Mathf.Lerp(a.DensityScale, b.DensityScale, t);
		DensityCurve = Mathf.Lerp(a.DensityCurve, b.DensityCurve, t);
		WispsDensitySoftening = Mathf.Lerp(a.WispsDensitySoftening, b.WispsDensitySoftening, t);
		MSIntensity = Mathf.Lerp(a.MSIntensity, b.MSIntensity, t);
		MSAbsorption = Mathf.Lerp(a.MSAbsorption, b.MSAbsorption, t);
		MSDepthFalloff = Mathf.Lerp(a.MSDepthFalloff, b.MSDepthFalloff, t);
		MSMinDepth = Mathf.Lerp(a.MSMinDepth, b.MSMinDepth, t);
		SunEccentricity1 = Mathf.Lerp(a.SunEccentricity1, b.SunEccentricity1, t);
		SunEccentricity2 = Mathf.Lerp(a.SunEccentricity2, b.SunEccentricity2, t);
		EvalAmbientColorScale = Mathf.Lerp(a.EvalAmbientColorScale, b.EvalAmbientColorScale, t);
		EvalSunColorScale = Mathf.Lerp(a.EvalSunColorScale, b.EvalSunColorScale, t);
		EvalMoonColorScale = Mathf.Lerp(a.EvalMoonColorScale, b.EvalMoonColorScale, t);
		AmbientSaturation = Mathf.Lerp(a.AmbientSaturation, b.AmbientSaturation, t);
		AmbientScatteringFalloff = Mathf.Lerp(a.AmbientScatteringFalloff, b.AmbientScatteringFalloff, t);
		Absorption = Mathf.Lerp(a.Absorption, b.Absorption, t);
		CoverageScale = Mathf.Lerp(a.CoverageScale, b.CoverageScale, t);
		CloudTypeTop = Mathf.Lerp(a.CloudTypeTop, b.CloudTypeTop, t);
		CloudTypeBottom = Mathf.Lerp(a.CloudTypeBottom, b.CloudTypeBottom, t);
		MoonEccentricity1 = Mathf.Lerp(a.MoonEccentricity1, b.MoonEccentricity1, t);
		MoonEccentricity2 = Mathf.Lerp(a.MoonEccentricity2, b.MoonEccentricity2, t);
		MoonSaturation = Mathf.Lerp(a.MoonSaturation, b.MoonSaturation, t);
		DirectScatterBrightness = Mathf.Lerp(a.DirectScatterBrightness, b.DirectScatterBrightness, t);
		DirectScatterContrast = Mathf.Lerp(a.DirectScatterContrast, b.DirectScatterContrast, t);
		AmbientScatterBrightness = Mathf.Lerp(a.AmbientScatterBrightness, b.AmbientScatterBrightness, t);
		AmbientScatterContrast = Mathf.Lerp(a.AmbientScatterContrast, b.AmbientScatterContrast, t);
		VerticalProfileTopStart = Mathf.Lerp(a.VerticalProfileTopStart, b.VerticalProfileTopStart, t);
		VerticalProfileTopEnd = Mathf.Lerp(a.VerticalProfileTopEnd, b.VerticalProfileTopEnd, t);
		VerticalProfileBottomStart = Mathf.Lerp(a.VerticalProfileBottomStart, b.VerticalProfileBottomStart, t);
		VerticalProfileBottomEnd = Mathf.Lerp(a.VerticalProfileBottomEnd, b.VerticalProfileBottomEnd, t);
		WindShear = Mathf.Lerp(a.WindShear, b.WindShear, t);
		WindVector = Vector2.Lerp(a.WindVector, b.WindVector, t);
		ShadowDensityScale = Mathf.Lerp(a.ShadowDensityScale, b.ShadowDensityScale, t);
		AtmosphereShadowDensityScale = Mathf.Lerp(a.AtmosphereShadowDensityScale, b.AtmosphereShadowDensityScale, t);
		HazeDensity = Mathf.Lerp(a.HazeDensity, b.HazeDensity, t);
		HazeHeightFalloff = Mathf.Lerp(a.HazeHeightFalloff, b.HazeHeightFalloff, t);
		HorizonBuffer = Mathf.Lerp(a.HorizonBuffer, b.HorizonBuffer, t);
	}

	public VolumeCloudsConfig()
	{
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		WeatherLayer = new VolumeCloudsWeatherLayerConfig();
		Billows = 1f;
		BillowsGamma = 2f;
		BillowsFrequencyCurve = 0.45f;
		Wisps = 1f;
		WispsGamma = 2f;
		WispsFrequencyCurve = 1f;
		DetailTypeDensityCurve = 0.25f;
		DetailTypeHeightTransition = 0.3f;
		CurlNoiseScale = 0.2f;
		CurlNoiseStrength = 50f;
		DensityScale = 0.05f;
		DensityCurve = 0.4f;
		WispsDensitySoftening = 0.3f;
		MSIntensity = 1f;
		MSAbsorption = 0.5f;
		MSDepthFalloff = 1f;
		MSMinDepth = 0.01f;
		SunEccentricity1 = 0.6f;
		SunEccentricity2 = 0.2f;
		MoonEccentricity1 = 0.9f;
		MoonEccentricity2 = 0.2f;
		MoonSaturation = 1f;
		DirectScatterBrightness = 1f;
		DirectScatterContrast = 1f;
		Absorption = 1f;
		AmbientSaturation = 0.6f;
		AmbientScatteringFalloff = 1f;
		AmbientScatterBrightness = 1f;
		AmbientScatterContrast = 1f;
		CoverageScale = 1f;
		CloudTypeTop = 1f;
		VerticalProfileTopEnd = 1f;
		VerticalProfileBottomStart = 1f;
		WindVector = new Vector2(0f, 1f);
		ShadowDensityScale = 1f;
		AtmosphereShadowDensityScale = 1f;
		HazeDensity = 1f;
		HazeHeightFalloff = 0.035f;
		((ScriptableObject)this)._002Ector();
	}
}

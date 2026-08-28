using AmplifyOcclusion;
using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/AmbientOcclusion")]
public class SSAORendererFeature : RustRendererFeature
{
	[Header("Ambient Occlusion")]
	[Tooltip("How the occlusion result is composited into the frame.")]
	public ApplicationMethod ApplyMethod;

	[Tooltip("Number of samples per occlusion pass.")]
	public SampleCountLevel SampleCount;

	[Tooltip("Source used for per-pixel normals.")]
	public PerPixelNormalSource PerPixelNormals;

	[Range(0f, 1f)]
	[Tooltip("Final applied intensity of the occlusion effect.")]
	public float Intensity;

	[Tooltip("Tint colour blended with the occlusion shadow.")]
	public Color Tint;

	[Range(0f, 32f)]
	[Tooltip("World-space radius of the occlusion kernel.")]
	public float Radius;

	[Tooltip("Power exponent attenuation of the occlusion.")]
	[Range(0f, 16f)]
	public float PowerExponent;

	[Range(0f, 0.99f)]
	[Tooltip("Initial occlusion contribution offset (reduces self-occlusion / acne).")]
	public float Bias;

	[Range(0f, 1f)]
	[Tooltip("Controls thickness-based occlusion contribution.")]
	public float Thickness;

	[Tooltip("Compute occlusion and blur at half resolution.")]
	public bool Downsample;

	[Header("Distance Fade")]
	[Tooltip("Fade the effect out at a distance.")]
	public bool FadeEnabled;

	[Tooltip("Distance (Unity units) where fading begins.")]
	public float FadeStart;

	[Tooltip("Length of the fade transition zone.")]
	public float FadeLength;

	[Range(0f, 1f)]
	public float FadeToIntensity;

	public Color FadeToTint;

	[Range(0f, 32f)]
	public float FadeToRadius;

	[Range(0f, 16f)]
	public float FadeToPowerExponent;

	[Range(0f, 1f)]
	public float FadeToThickness;

	[Header("Bilateral Blur")]
	public bool BlurEnabled;

	[Range(1f, 4f)]
	[Tooltip("Blur kernel radius in screen pixels (1–4).")]
	public int BlurRadius;

	[Tooltip("Number of blur passes.")]
	[Range(1f, 4f)]
	public int BlurPasses;

	[Tooltip("0 = blurred / 1 = sharpened.")]
	[Range(0f, 20f)]
	public float BlurSharpness;

	[Header("Temporal Filter")]
	[Tooltip("Accumulate occlusion over multiple frames to reduce noise.")]
	public bool FilterEnabled;

	[Tooltip("Accumulation decay. 0 = fast update (more flicker). 1 = slow update (ghosting).")]
	[Range(0f, 1f)]
	public float FilterBlending;

	[Tooltip("Motion-discard sensitivity. 0 = reuse more. 1 = discard more.")]
	[Range(0f, 1f)]
	public float FilterResponse;

	[Header("Shaders")]
	public Shader occlusionShader;

	public Shader blurShader;

	public Shader occlusionApplyShader;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}

	public SSAORendererFeature()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		SampleCount = SampleCountLevel.Medium;
		PerPixelNormals = PerPixelNormalSource.Camera;
		Intensity = 1f;
		Tint = Color.black;
		Radius = 2f;
		PowerExponent = 1.8f;
		Bias = 0.05f;
		Thickness = 1f;
		Downsample = true;
		FadeStart = 100f;
		FadeLength = 50f;
		FadeToTint = Color.black;
		FadeToRadius = 2f;
		FadeToPowerExponent = 1.8f;
		FadeToThickness = 1f;
		BlurEnabled = true;
		BlurRadius = 3;
		BlurPasses = 1;
		BlurSharpness = 10f;
		FilterEnabled = true;
		FilterBlending = 0.5f;
		FilterResponse = 0.5f;
		((RustRendererFeature)this)._002Ector();
	}
}

using AmplifyOcclusion;
using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/AmbientOcclusion")]
public class SSAORendererFeature : RustRendererFeature
{
	[Tooltip("How the occlusion result is composited into the frame.")]
	[Header("Ambient Occlusion")]
	public ApplicationMethod ApplyMethod;

	[Tooltip("Number of samples per occlusion pass.")]
	public SampleCountLevel SampleCount;

	[Tooltip("Source used for per-pixel normals.")]
	public PerPixelNormalSource PerPixelNormals;

	[Tooltip("Final applied intensity of the occlusion effect.")]
	[Range(0f, 1f)]
	public float Intensity;

	[Tooltip("Tint colour blended with the occlusion shadow.")]
	public Color Tint;

	[Tooltip("World-space radius of the occlusion kernel.")]
	[Range(0f, 32f)]
	public float Radius;

	[Range(0f, 16f)]
	[Tooltip("Power exponent attenuation of the occlusion.")]
	public float PowerExponent;

	[Range(0f, 0.99f)]
	[Tooltip("Initial occlusion contribution offset (reduces self-occlusion / acne).")]
	public float Bias;

	[Tooltip("Controls thickness-based occlusion contribution.")]
	[Range(0f, 1f)]
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

	[Tooltip("Blur kernel radius in screen pixels (1–4).")]
	[Range(1f, 4f)]
	public int BlurRadius;

	[Range(1f, 4f)]
	[Tooltip("Number of blur passes.")]
	public int BlurPasses;

	[Range(0f, 20f)]
	[Tooltip("0 = blurred / 1 = sharpened.")]
	public float BlurSharpness;

	[Tooltip("Accumulate occlusion over multiple frames to reduce noise.")]
	[Header("Temporal Filter")]
	public bool FilterEnabled;

	[Range(0f, 1f)]
	[Tooltip("Accumulation decay. 0 = fast update (more flicker). 1 = slow update (ghosting).")]
	public float FilterBlending;

	[Range(0f, 1f)]
	[Tooltip("Motion-discard sensitivity. 0 = reuse more. 1 = discard more.")]
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

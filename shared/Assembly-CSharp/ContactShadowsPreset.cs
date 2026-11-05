using UnityEngine;

[CreateAssetMenu(fileName = "ContactShadowsPreset", menuName = "Rendering/ContactShadowsPreset")]
public class ContactShadowsPreset : ScriptableObject
{
	[Range(0f, 1f)]
	public float BlendStrength = 1f;

	[Range(0f, 1f)]
	public float Accumulation = 0.9f;

	[Range(0.1f, 5f)]
	public float LengthFade = 0.7f;

	[Range(0.01f, 5f)]
	public float Range = 0.7f;

	[Range(0f, 1f)]
	public float ZThickness = 0.1f;

	[Range(2f, 92f)]
	public int Samples = 32;

	[Range(0.5f, 4f)]
	public float NearSampleQuality = 1.5f;

	[Range(0f, 1f)]
	public float TraceBias = 0.03f;

	public bool StochasticSampling = true;

	public bool LeverageTemporalAA;

	[Header("Bilateral Blur")]
	public bool BilateralBlur = true;

	[Range(1f, 2f)]
	public int BlurPasses = 1;

	[Range(0.01f, 0.5f)]
	public float BlurDepthTolerance = 0.1f;
}

using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Deploy Guide Material")]
public class DeployGuideMaterial : ScriptableObject
{
	public Color Albedo = Color.white;

	public Color Emission = Color.white;

	public float EmissionStrength = 1f;

	public float FresnelPower = 1f;

	public float FresnelStrength = 1f;

	public float RimPower = 1f;

	public float RimStrength = 1f;

	public float Alpha = 0.5f;

	public float AlphaFresnelPower = 1f;

	public float BackfaceBrightness = 1f;

	public float BackfaceAmount = 0.1f;

	public DeployGuideMaterial()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
	}
}

using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Deploy Guide Material")]
public class DeployGuideMaterial : ScriptableObject
{
	public Color Albedo;

	public Color Emission;

	public float EmissionStrength;

	public float FresnelPower;

	public float FresnelStrength;

	public float RimPower;

	public float RimStrength;

	public float Alpha;

	public float AlphaFresnelPower;

	public float BackfaceBrightness;

	public float BackfaceAmount;

	public DeployGuideMaterial()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Albedo = Color.white;
		Emission = Color.white;
		EmissionStrength = 1f;
		FresnelPower = 1f;
		FresnelStrength = 1f;
		RimPower = 1f;
		RimStrength = 1f;
		Alpha = 0.5f;
		AlphaFresnelPower = 1f;
		BackfaceBrightness = 1f;
		BackfaceAmount = 0.1f;
		((ScriptableObject)this)._002Ector();
	}
}

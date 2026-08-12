using UnityEngine;

public class ExplosionsShaderColorGradient : MonoBehaviour, IClientComponent
{
	public string ShaderProperty;

	public int MaterialID;

	public Gradient Color;

	public float TimeMultiplier;

	public ExplosionsShaderColorGradient()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		ShaderProperty = "_TintColor";
		Color = new Gradient();
		TimeMultiplier = 1f;
		((MonoBehaviour)this)._002Ector();
	}
}

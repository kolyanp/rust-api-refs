using UnityEngine;

public class ExplosionsShaderColorGradient : MonoBehaviour, IClientComponent
{
	public string ShaderProperty = "_TintColor";

	public int MaterialID;

	public Gradient Color = new Gradient();

	public float TimeMultiplier = 1f;

	public ExplosionsShaderColorGradient()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
	}
}

using Development.Attributes;
using UnityEngine;

[ResetStaticFields]
public class EmissionScaledByLight : MonoBehaviour, IClientComponent
{
	private Color emissionColor;

	public Renderer[] targetRenderers;

	public int materialIndex = -1;

	public Light lightToFollow;

	public float maxEmissionValue = 3f;
}

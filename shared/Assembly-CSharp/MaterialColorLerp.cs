using UnityEngine;

public class MaterialColorLerp : MonoBehaviour, IClientComponent, IFlattenKeepsChildren
{
	public Color startColor;

	public Color endColor;

	public Color currentColor;

	public float delta;
}

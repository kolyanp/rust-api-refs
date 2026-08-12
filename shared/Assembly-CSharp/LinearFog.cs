using UnityEngine;

[ExecuteInEditMode]
public class LinearFog : MonoBehaviour
{
	public Material fogMaterial;

	public Color fogColor;

	public float fogStart;

	public float fogRange;

	public float fogDensity;

	public bool fogSky;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)fogMaterial))
		{
			Graphics.Blit((Texture)(object)source, destination);
			return;
		}
		fogMaterial.SetColor("_FogColor", fogColor);
		fogMaterial.SetFloat("_Start", fogStart);
		fogMaterial.SetFloat("_Range", fogRange);
		fogMaterial.SetFloat("_Density", fogDensity);
		if (fogSky)
		{
			fogMaterial.SetFloat("_CutOff", 2f);
		}
		else
		{
			fogMaterial.SetFloat("_CutOff", 1f);
		}
		for (int i = 0; i < fogMaterial.passCount; i++)
		{
			Graphics.Blit((Texture)(object)source, destination, fogMaterial, i);
		}
	}

	public LinearFog()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		fogColor = Color.white;
		fogRange = 1f;
		fogDensity = 1f;
		((MonoBehaviour)this)._002Ector();
	}
}

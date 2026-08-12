using UnityEngine;

namespace FIMSpace.Basics;

public class FBasic_MaterialTiler : FBasic_MaterialScriptBase
{
	[Tooltip("Texture identificator in shader")]
	[Header("When you scale object change")]
	[Space(10f)]
	[Header("something in script to apply")]
	public string TextureProperty;

	[Tooltip("How much tiles should be multiplied according to gameObject's scale")]
	public Vector2 ScaleValues;

	[Tooltip("When scale on Y should be same as X")]
	public bool EqualDimensions;

	private void OnValidate()
	{
		GetRendererMaterial();
		if (EqualDimensions)
		{
			ScaleValues.y = ScaleValues.x;
		}
		TileMaterialToScale();
	}

	private void TileMaterialToScale()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)RendererMaterial == (Object)null) && !((Object)(object)ObjectRenderer == (Object)null))
		{
			Vector2 scaleValues = ScaleValues;
			scaleValues.x *= ((Component)this).transform.localScale.x;
			scaleValues.y *= ((Component)this).transform.localScale.z;
			RendererMaterial.SetTextureScale("_MainTex", scaleValues);
			ObjectRenderer.material = RendererMaterial;
		}
	}

	public FBasic_MaterialTiler()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		TextureProperty = "_MainTex";
		ScaleValues = new Vector2(1f, 1f);
		base._002Ector();
	}
}

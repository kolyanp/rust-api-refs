using UnityEngine;

public class DigitController : MonoBehaviour
{
	public MeshRenderer[] AllDigits;

	public void SetDigitActive(int digit)
	{
		for (int i = 0; i < AllDigits.Length; i++)
		{
			((Component)AllDigits[i]).gameObject.SetActive(i == digit);
		}
	}

	public void SetAllDigitsTransparency(float normalizedAlpha, MaterialPropertyBlock materialPropertyBlock, int colorProperty)
	{
		for (int i = 0; i < AllDigits.Length; i++)
		{
			SetDigitTransparency(i, normalizedAlpha, materialPropertyBlock, colorProperty);
		}
	}

	public void SetDigitTransparency(int digit, float normalizedAlpha, MaterialPropertyBlock materialPropertyBlock, int colorProperty)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		MeshRenderer val = AllDigits[digit];
		((Renderer)val).GetPropertyBlock(materialPropertyBlock);
		Color color = materialPropertyBlock.GetColor(colorProperty);
		color.a = Mathf.Lerp(0f, ((Renderer)val).sharedMaterial.color.a, normalizedAlpha);
		materialPropertyBlock.SetColor(colorProperty, color);
		((Renderer)val).SetPropertyBlock(materialPropertyBlock);
	}
}

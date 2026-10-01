using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing;

[Preserve]
internal sealed class VignetteRenderer : PostProcessEffectRenderer<Vignette>
{
	public override void Render(PostProcessRenderContext context)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		PropertySheet uberSheet = context.uberSheet;
		uberSheet.EnableKeyword("VIGNETTE");
		uberSheet.properties.SetColor(ShaderIDs.Vignette_Color, settings.color.value);
		if ((VignetteMode)settings.mode == VignetteMode.Classic)
		{
			uberSheet.properties.SetFloat(ShaderIDs.Vignette_Mode, 0f);
			uberSheet.properties.SetVector(ShaderIDs.Vignette_Center, Vector4.op_Implicit(settings.center.value));
			float num = (1f - settings.roundness.value) * 6f + settings.roundness.value;
			uberSheet.properties.SetVector(ShaderIDs.Vignette_Settings, new Vector4(settings.intensity.value * 3f, settings.smoothness.value * 5f, num, settings.rounded.value ? 1f : 0f));
		}
		else
		{
			uberSheet.properties.SetFloat(ShaderIDs.Vignette_Mode, 1f);
			uberSheet.properties.SetTexture(ShaderIDs.Vignette_Mask, settings.mask.value);
			uberSheet.properties.SetFloat(ShaderIDs.Vignette_Opacity, Mathf.Clamp01(settings.opacity.value));
		}
	}
}

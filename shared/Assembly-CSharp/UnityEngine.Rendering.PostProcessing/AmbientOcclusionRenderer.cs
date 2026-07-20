using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing;

[Preserve]
internal sealed class AmbientOcclusionRenderer : PostProcessEffectRenderer<AmbientOcclusion>
{
	private UnityEngine.Rendering.PostProcessing.IAmbientOcclusionMethod[] m_Methods;

	public override void Init()
	{
		if (m_Methods == null)
		{
			m_Methods = new UnityEngine.Rendering.PostProcessing.IAmbientOcclusionMethod[2]
			{
				new UnityEngine.Rendering.PostProcessing.ScalableAO(base.settings),
				new UnityEngine.Rendering.PostProcessing.MultiScaleVO(base.settings)
			};
		}
	}

	public bool IsAmbientOnly(PostProcessRenderContext context)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		Camera camera = context.camera;
		if (base.settings.ambientOnly.value && (int)camera.actualRenderingPath == 3)
		{
			return camera.allowHDR;
		}
		return false;
	}

	public UnityEngine.Rendering.PostProcessing.IAmbientOcclusionMethod Get()
	{
		return m_Methods[(int)base.settings.mode.value];
	}

	public override DepthTextureMode GetCameraFlags()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Get().GetCameraFlags();
	}

	[UnityEvent]
	public override void Release()
	{
		UnityEngine.Rendering.PostProcessing.IAmbientOcclusionMethod[] methods = m_Methods;
		for (int i = 0; i < methods.Length; i++)
		{
			methods[i].Release();
		}
	}

	public UnityEngine.Rendering.PostProcessing.ScalableAO GetScalableAO()
	{
		return (UnityEngine.Rendering.PostProcessing.ScalableAO)m_Methods[0];
	}

	public UnityEngine.Rendering.PostProcessing.MultiScaleVO GetMultiScaleVO()
	{
		return (UnityEngine.Rendering.PostProcessing.MultiScaleVO)m_Methods[1];
	}

	public override void Render(PostProcessRenderContext context)
	{
	}
}

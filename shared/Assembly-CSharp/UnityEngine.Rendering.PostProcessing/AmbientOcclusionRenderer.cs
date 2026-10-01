using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing;

[Preserve]
internal sealed class AmbientOcclusionRenderer : PostProcessEffectRenderer<AmbientOcclusion>
{
	private IAmbientOcclusionMethod[] m_Methods;

	public override void Init()
	{
		if (m_Methods == null)
		{
			m_Methods = new IAmbientOcclusionMethod[2]
			{
				new ScalableAO(settings),
				new MultiScaleVO(settings)
			};
		}
	}

	public bool IsAmbientOnly(PostProcessRenderContext context)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		Camera camera = context.camera;
		if (settings.ambientOnly.value && (int)camera.actualRenderingPath == 3)
		{
			return camera.allowHDR;
		}
		return false;
	}

	public IAmbientOcclusionMethod Get()
	{
		return m_Methods[(int)settings.mode.value];
	}

	public override DepthTextureMode GetCameraFlags()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Get().GetCameraFlags();
	}

	[UnityEvent]
	public override void Release()
	{
		IAmbientOcclusionMethod[] methods = m_Methods;
		for (int i = 0; i < methods.Length; i++)
		{
			methods[i].Release();
		}
	}

	public ScalableAO GetScalableAO()
	{
		return (ScalableAO)m_Methods[0];
	}

	public MultiScaleVO GetMultiScaleVO()
	{
		return (MultiScaleVO)m_Methods[1];
	}

	public override void Render(PostProcessRenderContext context)
	{
	}
}

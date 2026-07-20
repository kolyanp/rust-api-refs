using System;
using Rust.RenderPipeline.Runtime;
using Rust.RenderPipeline.Runtime.RenderingContext;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class WaterRendererPreFogPass : RustRenderPass
{
	public class PassData
	{
		public TextureHandle srcHandle;

		public TextureHandle dstHandle;

		public RenderTargetIdentifier src;

		public RenderTargetIdentifier dst;

		public Material multiCopyMat;
	}

	private Material _multiCopyMat;

	public static TextureDesc BackgroundTextureDesc;

	public WaterRendererPreFogPass(Material multiCopyMat)
	{
		_multiCopyMat = multiCopyMat;
	}

	public static void Draw(CommandBuffer cmd, PassData data)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		cmd.Blit(data.src, data.dst, data.multiCopyMat, 1);
		cmd.SetGlobalTexture(WaterRendererShaderProps.WaterPreFogBackgroundTexture, data.dst);
	}

	public static void ExecutePass(PassData data, RenderGraphContext ctx)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		data.src = TextureHandle.op_Implicit(data.srcHandle);
		data.dst = TextureHandle.op_Implicit(data.dstHandle);
		Draw(((RenderGraphContext)(ref ctx)).cmd, data);
	}

	public unsafe override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData, CameraFeatureContexts featureContexts)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		RustResourceDataContext val = frameData.Get<RustResourceDataContext>();
		RustCameraContext val2 = frameData.Get<RustCameraContext>();
		WaterRendererFrameData waterRendererFrameData = frameData.Get<WaterRendererFrameData>();
		PassData passData = default(PassData);
		RenderGraphBuilder val3 = renderGraph.AddRenderPass<PassData>("Water Pre Fog", ref passData, "D:\\ws\\workspace\\Rust-Server-release\\Assets\\Scripts\\Rendering\\RendererFeatures\\Water\\WaterRendererPreFogPass.cs", 68);
		try
		{
			passData.multiCopyMat = _multiCopyMat;
			PassData passData2 = passData;
			TextureHandle activeColorTexture = val.ActiveColorTexture;
			passData2.srcHandle = ((RenderGraphBuilder)(ref val3)).ReadTexture(ref activeColorTexture);
			PassData passData3 = passData;
			TextureDesc backgroundTextureDesc = BackgroundTextureDesc;
			Vector2Int cameraBufferSize = val2.CameraBufferSize;
			int num = ((Vector2Int)(ref cameraBufferSize)).x / 2;
			cameraBufferSize = val2.CameraBufferSize;
			TextureDesc val4 = RustRenderPipelineUtils.TextureDescSetSize(backgroundTextureDesc, num, ((Vector2Int)(ref cameraBufferSize)).y / 2, -1);
			activeColorTexture = renderGraph.CreateTexture(ref val4);
			passData3.dstHandle = (waterRendererFrameData.preFogBackgroundTex = ((RenderGraphBuilder)(ref val3)).WriteTexture(ref activeColorTexture));
			((RenderGraphBuilder)(ref val3)).SetRenderFunc<PassData>((BaseRenderFunc<PassData, RenderGraphContext>)ExecutePass);
		}
		finally
		{
			((IDisposable)(*(RenderGraphBuilder*)(&val3))/*cast due to constrained. prefix*/).Dispose();
		}
	}

	static WaterRendererPreFogPass()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		TextureDesc backgroundTextureDesc = default(TextureDesc);
		((TextureDesc)(ref backgroundTextureDesc))._002Ector(0, 0, false, false);
		backgroundTextureDesc.name = "Water Pre Fog Background Texture";
		((TextureDesc)(ref backgroundTextureDesc)).colorFormat = (GraphicsFormat)4;
		backgroundTextureDesc.filterMode = (FilterMode)1;
		backgroundTextureDesc.wrapMode = (TextureWrapMode)1;
		BackgroundTextureDesc = backgroundTextureDesc;
	}
}

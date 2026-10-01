using System;
using Rust.RenderPipeline.Runtime;
using Rust.RenderPipeline.Runtime.RenderingContext;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class WaterRendererPostFogPass : RustRenderPass
{
	public class PassData
	{
		public TextureHandle srcHandle;

		public TextureHandle dstHandle;

		public TextureHandle ssrReflectionHandle;

		public RenderTargetIdentifier src;

		public RenderTargetIdentifier dst;

		public RenderTargetIdentifier ssrReflection;

		public Material multiCopyMat;

		public Material reflectionMat;

		public Matrix4x4 ssrProj;
	}

	private readonly Material _multiCopyMat;

	private readonly Material _reflectionMat;

	public static TextureDesc BackgroundTextureDesc;

	public static TextureDesc SSRTextureDesc;

	public WaterRendererPostFogPass(Material multiCopyMat, Material reflectionMat)
	{
		_multiCopyMat = multiCopyMat;
		_reflectionMat = reflectionMat;
	}

	public static void Draw(CommandBuffer cmd, PassData data)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		cmd.Blit(data.src, data.dst, data.multiCopyMat, 1);
		cmd.SetGlobalTexture(WaterRendererShaderProps.BackgroundColorTexture, data.dst);
		cmd.SetGlobalMatrix(WaterRendererShaderProps.WaterSSR_CameraProj, data.ssrProj);
		cmd.Blit((Texture)null, data.ssrReflection, data.reflectionMat, WaterSystem.Instance.Reflections);
		cmd.SetGlobalTexture(WaterRendererShaderProps.WaterSSR_ReflectionTexture, data.ssrReflection);
	}

	public static void ExecutePass(PassData data, RenderGraphContext ctx)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		data.src = TextureHandle.op_Implicit(data.srcHandle);
		data.dst = TextureHandle.op_Implicit(data.dstHandle);
		data.ssrReflection = TextureHandle.op_Implicit(data.ssrReflectionHandle);
		Draw(ctx.cmd, data);
	}

	public static Matrix4x4 ComputeCameraSSRProj(int width, int height, Matrix4x4 projMat)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		return Matrix4x4.Scale(new Vector3((float)width, (float)height, 1f)) * Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0f), Quaternion.identity, new Vector3(0.5f, 0.5f, 1f)) * GL.GetGPUProjectionMatrix(projMat, false);
	}

	public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData, CameraFeatureContexts featureContexts)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		if (WaterSystem.Instance.Reflections <= 0)
		{
			return;
		}
		RustResourceDataContext val = frameData.Get<RustResourceDataContext>();
		RustCameraContext val2 = frameData.Get<RustCameraContext>();
		WaterRendererFrameData waterRendererFrameData = frameData.Get<WaterRendererFrameData>();
		PostOpaqueDepthResourceData postOpaqueDepthResourceData = frameData.Get<PostOpaqueDepthResourceData>();
		Vector2Int cameraBufferSize = val2.CameraBufferSize;
		int x = cameraBufferSize.x;
		cameraBufferSize = val2.CameraBufferSize;
		int y = cameraBufferSize.y;
		PassData passData = default;
		RenderGraphBuilder val3 = renderGraph.AddRenderPass<PassData>("Water Post Fog", ref passData, "D:\\ws\\workspace\\Rust-Server-release\\Assets\\Scripts\\Rendering\\RendererFeatures\\Water\\WaterRendererPostFogPass.cs", 99);
		try
		{
			cameraBufferSize = val2.CameraBufferSize;
			int x2 = cameraBufferSize.x;
			cameraBufferSize = val2.CameraBufferSize;
			Matrix4x4 ssrProj = ComputeCameraSSRProj(x2, cameraBufferSize.y, val2.Camera.nonJitteredProjectionMatrix);
			passData.ssrProj = ssrProj;
			passData.multiCopyMat = _multiCopyMat;
			passData.reflectionMat = _reflectionMat;
			PassData passData2 = passData;
			TextureHandle activeColorTexture = val.ActiveColorTexture;
			passData2.srcHandle = val3.ReadTexture(ref activeColorTexture);
			val3.ReadTexture(ref postOpaqueDepthResourceData.postOpaqueDepthHandle);
			activeColorTexture = val.CameraDepthTexture;
			val3.ReadTexture(ref activeColorTexture);
			PassData passData3 = passData;
			TextureDesc val4 = RustRenderPipelineUtils.TextureDescSetSize(BackgroundTextureDesc, x / 2, y / 2, -1);
			activeColorTexture = renderGraph.CreateTexture(ref val4);
			passData3.dstHandle = (waterRendererFrameData.backgroundColorTex = val3.WriteTexture(ref activeColorTexture));
			PassData passData4 = passData;
			val4 = RustRenderPipelineUtils.TextureDescSetSize(SSRTextureDesc, x, y, -1);
			activeColorTexture = renderGraph.CreateTexture(ref val4);
			passData4.ssrReflectionHandle = (waterRendererFrameData.ssrReflectionTex = val3.WriteTexture(ref activeColorTexture));
			val3.SetRenderFunc<PassData>((BaseRenderFunc<PassData, RenderGraphContext>)((PassData pd, RenderGraphContext ctx) =>
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				ExecutePass(pd, ctx);
			}));
		}
		finally
		{
			((IDisposable)val3/*cast due to constrained. prefix*/).Dispose();
		}
	}

	static WaterRendererPostFogPass()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		TextureDesc val = new TextureDesc(0, 0, false, false)
		{
			name = "Water Post Fog Background Texture"
		};
		val.colorFormat = (GraphicsFormat)4;
		val.wrapMode = (TextureWrapMode)1;
		val.filterMode = (FilterMode)1;
		BackgroundTextureDesc = val;
		val = new TextureDesc(0, 0, false, false)
		{
			name = "Water SSR Texture"
		};
		val.colorFormat = (GraphicsFormat)4;
		val.filterMode = (FilterMode)1;
		val.wrapMode = (TextureWrapMode)1;
		SSRTextureDesc = val;
	}
}

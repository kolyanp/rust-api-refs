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

		public Material multiCopyMat;

		public Material reflectionMat;

		public int reflectionPass;

		public Matrix4x4 ssrProj;
	}

	private readonly Material _multiCopyMat;

	private readonly Material _reflectionMat;

	public static TextureDesc BackgroundTextureDesc;

	public static TextureDesc SSRTextureDesc;

	private const int MultiCopyRGBMProceduralPass = 3;

	private const int ReflectionProceduralPassOffset = 2;

	private static readonly int PID_MainTex;

	private static readonly MaterialPropertyBlock s_Properties;

	public WaterRendererPostFogPass(Material multiCopyMat, Material reflectionMat)
	{
		_multiCopyMat = multiCopyMat;
		_reflectionMat = reflectionMat;
	}

	public static void ExecutePass(PassData data, UnsafeGraphContext ctx)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		UnsafeCommandBuffer cmd = ctx.cmd;
		s_Properties.Clear();
		s_Properties.SetTexture(PID_MainTex, TextureHandle.op_Implicit(data.srcHandle));
		cmd.SetRenderTarget(TextureHandle.op_Implicit(data.dstHandle));
		cmd.DrawProcedural(Matrix4x4.identity, data.multiCopyMat, 3, (MeshTopology)0, 3, 1, s_Properties);
		cmd.SetGlobalMatrix(WaterRendererShaderProps.WaterSSR_CameraProj, data.ssrProj);
		s_Properties.Clear();
		s_Properties.SetTexture(WaterRendererShaderProps.BackgroundColorTexture, TextureHandle.op_Implicit(data.dstHandle));
		cmd.SetRenderTarget(TextureHandle.op_Implicit(data.ssrReflectionHandle));
		cmd.DrawProcedural(Matrix4x4.identity, data.reflectionMat, data.reflectionPass, (MeshTopology)0, 3, 1, s_Properties);
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
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
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
		IUnsafeRenderGraphBuilder val3 = renderGraph.AddUnsafePass<PassData>("Water Post Fog", ref passData, "D:\\ws\\workspace\\Rust-Server-release\\Assets\\Scripts\\Rendering\\RendererFeatures\\Water\\WaterRendererPostFogPass.cs", 104);
		try
		{
			cameraBufferSize = val2.CameraBufferSize;
			int x2 = cameraBufferSize.x;
			cameraBufferSize = val2.CameraBufferSize;
			Matrix4x4 ssrProj = ComputeCameraSSRProj(x2, cameraBufferSize.y, val2.Camera.nonJitteredProjectionMatrix);
			passData.ssrProj = ssrProj;
			passData.multiCopyMat = _multiCopyMat;
			passData.reflectionMat = _reflectionMat;
			passData.srcHandle = val.ActiveColorTexture;
			((IBaseRenderGraphBuilder)val3).UseTexture(ref passData.srcHandle, (AccessFlags)1);
			((IBaseRenderGraphBuilder)val3).UseTexture(ref postOpaqueDepthResourceData.postOpaqueDepthHandle, (AccessFlags)1);
			TextureHandle cameraDepthTexture = val.CameraDepthTexture;
			((IBaseRenderGraphBuilder)val3).UseTexture(ref cameraDepthTexture, (AccessFlags)1);
			PassData passData2 = passData;
			TextureDesc val4 = RustRenderPipelineUtils.TextureDescSetSize(BackgroundTextureDesc, x / 2, y / 2, -1);
			passData2.dstHandle = (waterRendererFrameData.backgroundColorTex = renderGraph.CreateTexture(ref val4));
			((IBaseRenderGraphBuilder)val3).UseTexture(ref passData.dstHandle, (AccessFlags)6);
			((IBaseRenderGraphBuilder)val3).SetGlobalTextureAfterPass(ref passData.dstHandle, WaterRendererShaderProps.BackgroundColorTexture);
			PassData passData3 = passData;
			val4 = RustRenderPipelineUtils.TextureDescSetSize(SSRTextureDesc, x, y, -1);
			passData3.ssrReflectionHandle = (waterRendererFrameData.ssrReflectionTex = renderGraph.CreateTexture(ref val4));
			((IBaseRenderGraphBuilder)val3).UseTexture(ref passData.ssrReflectionHandle, (AccessFlags)6);
			((IBaseRenderGraphBuilder)val3).SetGlobalTextureAfterPass(ref passData.ssrReflectionHandle, WaterRendererShaderProps.WaterSSR_ReflectionTexture);
			passData.reflectionPass = WaterSystem.Instance.Reflections + 2;
			((IBaseRenderGraphBuilder)val3).UseTexture(ref waterRendererFrameData.surfaceTex, (AccessFlags)1);
			val3.SetRenderFunc<PassData>((BaseRenderFunc<PassData, UnsafeGraphContext>)((PassData pd, UnsafeGraphContext ctx) =>
			{
				ExecutePass(pd, ctx);
			}));
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
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
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected Obj, but got Unknown
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
		PID_MainTex = Shader.PropertyToID("_MainTex");
		s_Properties = new MaterialPropertyBlock();
	}
}

using System;
using System.Collections;
using ConVar;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

[Serializable]
public class ShadowCacher
{
	private const int CACHE_TEXTURE_RESOLUTION = 256;

	private const float CACHE_TEXTURE_FRAGMENT_SIZE = 0.00390625f;

	private const GraphicsFormat CACHE_TEXTURE_FORMAT = (GraphicsFormat)52;

	private static readonly int tempShadowMapCacheId = Shader.PropertyToID("_TempShadowMapCache");

	private static readonly int tempGaussianBlurId = Shader.PropertyToID("_TempGaussianBlur");

	private static readonly int blurDirectionId = Shader.PropertyToID("_BlurDirection");

	private static readonly int shadowMapTextureId = Shader.PropertyToID("_ShadowMapTexture");

	private static readonly int cubemapFaceId = Shader.PropertyToID("_CubemapFace");

	private static readonly int originalLightCookieId = Shader.PropertyToID("_OriginalLightCookie");

	private static readonly int shadowNearPlaneId = Shader.PropertyToID("_ShadowNearPlane");

	private static bool staticResourcesLoaded;

	private static Material copyShadowMapMat;

	private static Material gaussianBlurMat;

	private static Cubemap defaultWhiteCubemap;

	private static Texture defaultSpotLightCookie;

	private static RenderTexturePool spotLightRtPool;

	private static RenderTexturePool pointLightRtPool;

	[SerializeField]
	private float refreshDistanceDelta = 0.01f;

	private Light light;

	private LightLOD lightLod;

	private CommandBuffer shadowCopyCommandBuffer;

	private RenderTexture cachedShadowMap;

	private bool pendingCapture;

	private Texture originalCookieTexture;

	private Coroutine captureRoutine;

	private Vector3 lastRefreshPosition;

	private float shadowCacheRefreshTimer;

	private bool isInitialized;

	private float currentShadowFrameRate;

	private RenderTexturePool renderTexturePool;

	public void Initialize(Light light, LightLOD lightLod)
	{
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Invalid comparison between Unknown and I4
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Invalid comparison between Unknown and I4
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		if (isInitialized)
		{
			return;
		}
		this.light = light;
		this.lightLod = lightLod;
		if (!staticResourcesLoaded)
		{
			Shader val = Shader.Find("Hidden/ShadowCacheCopy");
			if ((Object)(object)val == (Object)null)
			{
				Debug.LogError((object)"Failed to find the copy shader for shadow caching!", (Object)(object)lightLod);
			}
			else
			{
				copyShadowMapMat = new Material(val)
				{
					hideFlags = (HideFlags)61
				};
			}
			Shader val2 = Shader.Find("Hidden/ShadowCacheGaussianBlur");
			if ((Object)(object)val2 == (Object)null)
			{
				Debug.LogError((object)"Failed to find the Gaussian Blur shader for shadow caching!", (Object)(object)lightLod);
			}
			else
			{
				gaussianBlurMat = new Material(val2)
				{
					hideFlags = (HideFlags)61
				};
			}
			defaultWhiteCubemap = Resources.Load<Cubemap>("ShadowCaching/WhiteCubemap");
			if ((Object)(object)defaultWhiteCubemap == (Object)null)
			{
				Debug.LogError((object)"Failed to load default white cubemap texture for shadow caching!", (Object)(object)lightLod);
			}
			defaultSpotLightCookie = Resources.Load<Texture>("ShadowCaching/DefaultUnityPointLightCookie");
			if ((Object)(object)defaultSpotLightCookie == (Object)null)
			{
				Debug.LogError((object)"Failed to load default spot light cookie texture for shadow caching!", (Object)(object)lightLod);
			}
			RebuildShadowMapPools(Graphics.shadowlights);
			staticResourcesLoaded = true;
		}
		originalCookieTexture = light.cookie;
		LightType type;
		if ((Object)(object)originalCookieTexture == (Object)null)
		{
			type = light.type;
			Texture val3 = (Texture)(((int)type == 0) ? defaultSpotLightCookie : (((int)type != 2) ? ((object)Texture2D.whiteTexture) : ((object)defaultWhiteCubemap)));
			originalCookieTexture = val3;
		}
		type = light.type;
		RenderTexturePool renderTexturePool = (((int)type == 0) ? spotLightRtPool : (((int)type != 2) ? null : pointLightRtPool));
		this.renderTexturePool = renderTexturePool;
		cachedShadowMap = this.renderTexturePool?.GetInstance();
		if ((Object)(object)cachedShadowMap == (Object)null)
		{
			Debug.LogError((object)"Failed to get cached shadow map render texture from pool! Initialization cancelled.", (Object)(object)lightLod);
			return;
		}
		InitializeCommandBuffers();
		Refresh();
		SetEnabledFlag(enabled: true, light);
		isInitialized = true;
	}

	private void InitializeCommandBuffers()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Invalid comparison between Unknown and I4
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		shadowCopyCommandBuffer = new CommandBuffer();
		shadowCopyCommandBuffer.name = "Shadow Cache Copy";
		shadowCopyCommandBuffer.SetGlobalTexture(shadowMapTextureId, RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)1));
		shadowCopyCommandBuffer.SetGlobalTexture(originalLightCookieId, RenderTargetIdentifier.op_Implicit(originalCookieTexture));
		shadowCopyCommandBuffer.SetGlobalFloat(shadowNearPlaneId, light.shadowNearPlane);
		if ((int)light.type == 2)
		{
			shadowCopyCommandBuffer.GetTemporaryRTArray(tempShadowMapCacheId, 256, 256, 6, 0, (FilterMode)0, (GraphicsFormat)52);
			shadowCopyCommandBuffer.GetTemporaryRTArray(tempGaussianBlurId, 256, 256, 6, 0, (FilterMode)1, (GraphicsFormat)52);
			for (int i = 0; i < 6; i++)
			{
				shadowCopyCommandBuffer.SetGlobalInt(cubemapFaceId, i);
				shadowCopyCommandBuffer.Blit(RenderTargetIdentifier.op_Implicit((Texture)null), RenderTargetIdentifier.op_Implicit(tempShadowMapCacheId), copyShadowMapMat, 0, i);
				shadowCopyCommandBuffer.SetGlobalVector(blurDirectionId, Vector4.op_Implicit(new Vector2(0.00390625f, 0f)));
				shadowCopyCommandBuffer.Blit(RenderTargetIdentifier.op_Implicit(tempShadowMapCacheId), RenderTargetIdentifier.op_Implicit(tempGaussianBlurId), gaussianBlurMat, 0, i);
				shadowCopyCommandBuffer.SetGlobalVector(blurDirectionId, Vector4.op_Implicit(new Vector2(0f, 0.00390625f)));
				shadowCopyCommandBuffer.Blit(RenderTargetIdentifier.op_Implicit(tempGaussianBlurId), RenderTargetIdentifier.op_Implicit(tempShadowMapCacheId), gaussianBlurMat, 0, i);
				shadowCopyCommandBuffer.CopyTexture(RenderTargetIdentifier.op_Implicit(tempShadowMapCacheId), i, 0, RenderTargetIdentifier.op_Implicit((Texture)(object)cachedShadowMap), i, 0);
			}
			shadowCopyCommandBuffer.ReleaseTemporaryRT(tempShadowMapCacheId);
			shadowCopyCommandBuffer.ReleaseTemporaryRT(tempGaussianBlurId);
		}
		if ((int)light.type == 0)
		{
			shadowCopyCommandBuffer.GetTemporaryRT(tempShadowMapCacheId, 256, 256, 0, (FilterMode)0, (GraphicsFormat)52);
			shadowCopyCommandBuffer.GetTemporaryRT(tempGaussianBlurId, 256, 256, 0, (FilterMode)0, (GraphicsFormat)52);
			shadowCopyCommandBuffer.Blit((Texture)null, RenderTargetIdentifier.op_Implicit(tempShadowMapCacheId), copyShadowMapMat, 1);
			shadowCopyCommandBuffer.SetGlobalVector(blurDirectionId, Vector4.op_Implicit(new Vector2(0.00390625f, 0f)));
			shadowCopyCommandBuffer.Blit(RenderTargetIdentifier.op_Implicit(tempShadowMapCacheId), RenderTargetIdentifier.op_Implicit(tempGaussianBlurId), gaussianBlurMat, 1);
			shadowCopyCommandBuffer.SetGlobalVector(blurDirectionId, Vector4.op_Implicit(new Vector2(0f, 0.00390625f)));
			shadowCopyCommandBuffer.Blit(RenderTargetIdentifier.op_Implicit(tempGaussianBlurId), RenderTargetIdentifier.op_Implicit(tempShadowMapCacheId), gaussianBlurMat, 1);
			shadowCopyCommandBuffer.CopyTexture(RenderTargetIdentifier.op_Implicit(tempShadowMapCacheId), RenderTargetIdentifier.op_Implicit((Texture)(object)cachedShadowMap));
			shadowCopyCommandBuffer.ReleaseTemporaryRT(tempShadowMapCacheId);
			shadowCopyCommandBuffer.ReleaseTemporaryRT(tempGaussianBlurId);
		}
	}

	public void SetEnabledFlag(bool enabled, Light lightComponent)
	{
		if (!((Object)(object)lightComponent == (Object)null))
		{
			lightComponent.shadowStrength = (enabled ? 1f : 0f);
		}
	}

	public static void RebuildShadowMapPools(int maxShadowLights)
	{
		int capacity = Mathf.Max(0, maxShadowLights) * 3;
		int capacity2 = Mathf.Max(0, maxShadowLights);
		spotLightRtPool = new RenderTexturePool(256, 256, (GraphicsFormat)52, (TextureDimension)2, (FilterMode)1, capacity);
		pointLightRtPool = new RenderTexturePool(256, 256, (GraphicsFormat)52, (TextureDimension)4, (FilterMode)1, capacity2);
	}

	private bool HasLightMoved()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (refreshDistanceDelta > 0f)
		{
			Vector3 val = ((Component)lightLod).transform.position - lastRefreshPosition;
			return ((Vector3)(ref val)).sqrMagnitude >= refreshDistanceDelta * refreshDistanceDelta;
		}
		return false;
	}

	private void Refresh()
	{
		if (!pendingCapture)
		{
			light.shadows = (LightShadows)2;
			light.AddCommandBuffer((LightEvent)1, shadowCopyCommandBuffer);
			pendingCapture = true;
			captureRoutine = ((MonoBehaviour)lightLod).StartCoroutine(FinishCaptureAtEndOfFrame());
		}
	}

	private IEnumerator FinishCaptureAtEndOfFrame()
	{
		yield return (object)new WaitForEndOfFrame();
		FinishCapture();
	}

	private void FinishCapture()
	{
		light.shadows = (LightShadows)0;
		light.cookie = (Texture)(object)cachedShadowMap;
		if (shadowCopyCommandBuffer != null)
		{
			light.RemoveCommandBuffer((LightEvent)1, shadowCopyCommandBuffer);
		}
		pendingCapture = false;
	}

	public void Release()
	{
		if (!isInitialized)
		{
			return;
		}
		isInitialized = false;
		pendingCapture = false;
		if ((Object)(object)lightLod != (Object)null && captureRoutine != null)
		{
			((MonoBehaviour)lightLod).StopCoroutine(captureRoutine);
		}
		renderTexturePool.ReleaseInstance(cachedShadowMap);
		cachedShadowMap = null;
		if ((Object)(object)light != (Object)null)
		{
			light.shadows = (LightShadows)0;
			if ((Object)(object)originalCookieTexture == (Object)(object)defaultSpotLightCookie || (Object)(object)originalCookieTexture == (Object)(object)defaultWhiteCubemap)
			{
				originalCookieTexture = null;
			}
			light.cookie = originalCookieTexture;
			if (shadowCopyCommandBuffer != null)
			{
				light.RemoveCommandBuffer((LightEvent)1, shadowCopyCommandBuffer);
			}
		}
		CommandBuffer obj = shadowCopyCommandBuffer;
		if (obj != null)
		{
			obj.Release();
		}
		shadowCopyCommandBuffer = null;
		SetEnabledFlag(enabled: false, light);
	}
}

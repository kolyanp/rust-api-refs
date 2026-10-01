using System;
using System.IO;
using UnityEngine;

namespace Facepunch.Extend;

public static class CameraEx
{
	public static bool IsLowerLod(GameObject obj)
	{
		string name = ((Object)obj).name;
		for (int i = 1; i <= 4; i++)
		{
			if (name.EndsWith("lod0" + i, StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
			if (name.EndsWith("lod" + i, StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	public static Bounds CalculateRendererBounds(GameObject obj, int layerMask = -1)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = obj.transform.position;
		Quaternion rotation = obj.transform.rotation;
		obj.transform.SetPositionAndRotation(Vector3.one, Quaternion.identity);
		obj.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
		Bounds result = new Bounds(Vector3.zero, Vector3.one * 0.01f);
		bool flag = true;
		Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>();
		foreach (Renderer val in componentsInChildren)
		{
			if (val.enabled && ((Component)val).gameObject.activeInHierarchy && !(val is ParticleSystemRenderer) && !IsLowerLod(((Component)val).gameObject) && (layerMask & (1 << ((Component)val).gameObject.layer)) != 0)
			{
				if (flag)
				{
					result = val.bounds;
					flag = false;
				}
				else
				{
					result.Encapsulate(val.bounds);
				}
			}
		}
		obj.transform.SetPositionAndRotation(position, rotation);
		return result;
	}

	public static float DistanceToFrame(Bounds bounds, float fieldOfView)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Vector3 size = bounds.size;
		return size.magnitude * 0.33f / Mathf.Tan(fieldOfView * 0.5f * ((float)Math.PI / 180f));
	}

	public static void FocusOnRenderer(this Camera cam, GameObject obj, Vector3 lookDirection, Vector3 Up, int layerMask = -1, float distanceModifier = 0f)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = CalculateRendererBounds(obj, layerMask);
		float num = DistanceToFrame(bounds, cam.fieldOfView);
		Matrix4x4 localToWorldMatrix = obj.transform.localToWorldMatrix;
		Vector3 val = localToWorldMatrix.MultiplyPoint(bounds.center);
		((Component)cam).transform.position = val + obj.transform.TransformDirection(lookDirection.normalized) * (num + distanceModifier);
		((Component)cam).transform.LookAt(val, obj.transform.TransformDirection(Up.normalized));
	}

	public static void SavePNG(string path, Texture2D texture)
	{
		byte[] bytes = ImageConversion.EncodeToPNG(texture);
		string directoryName = Path.GetDirectoryName(path);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		File.WriteAllBytes(path, bytes);
	}

	public static void ScreenshotToDisk(this Camera cam, string name, int width, int height, bool transparent, int SuperSampleSize, Color? background = null)
	{
		Texture2D val = cam.ScreenshotToTexture(width, height, transparent, SuperSampleSize, background);
		SavePNG(name, val);
		Object.DestroyImmediate((Object)(object)val, true);
	}

	public static Texture2D ScreenshotToTexture(this Camera cam, int width, int height, bool transparent, int superSampleSize, Color? background = null)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected Obj, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Expected Obj, but got Unknown
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		RenderTexture temporary = RenderTexture.GetTemporary(width * superSampleSize, height * superSampleSize, 24, (RenderTextureFormat)0, (RenderTextureReadWrite)2);
		Color backgroundColor = cam.backgroundColor;
		CameraClearFlags clearFlags = cam.clearFlags;
		RenderTexture targetTexture = cam.targetTexture;
		int antiAliasing = QualitySettings.antiAliasing;
		AnisotropicFiltering anisotropicFiltering = QualitySettings.anisotropicFiltering;
		bool sRGBWrite = GL.sRGBWrite;
		GameObject val = new GameObject();
		cam.forceIntoRenderTexture = true;
		cam.targetTexture = temporary;
		cam.aspect = 1f;
		cam.renderingPath = (RenderingPath)(-1);
		cam.rect = new Rect(0f, 0f, 1f, 1f);
		cam.allowHDR = true;
		Texture.SetGlobalAnisotropicFilteringLimits(16, 16);
		QualitySettings.anisotropicFiltering = (AnisotropicFiltering)2;
		QualitySettings.antiAliasing = 8;
		if (transparent)
		{
			cam.clearFlags = (CameraClearFlags)3;
			cam.backgroundColor = background ?? new Color(0f, 0f, 0f, 0f);
		}
		RenderTexture.active = temporary;
		GL.Clear(true, true, background ?? new Color(0f, 0f, 0f, 0f));
		GL.sRGBWrite = true;
		cam.Render();
		RenderTexture.active = null;
		RenderTexture.active = temporary;
		Texture2D val2 = new Texture2D(((Texture)temporary).width, ((Texture)temporary).height, (TextureFormat)5, true);
		val2.ReadPixels(new Rect(0f, 0f, (float)((Texture)temporary).width, (float)((Texture)temporary).height), 0, 0, true);
		((Texture)val2).filterMode = (FilterMode)2;
		((Texture)val2).anisoLevel = 32;
		RenderTexture.active = null;
		cam.targetTexture = targetTexture;
		QualitySettings.antiAliasing = antiAliasing;
		QualitySettings.anisotropicFiltering = anisotropicFiltering;
		Texture.SetGlobalAnisotropicFilteringLimits(1, 16);
		if (superSampleSize != 1)
		{
			val2.Apply();
			RenderTexture val3 = (RenderTexture.active = RenderTexture.GetTemporary(width, height, 24, (RenderTextureFormat)0, (RenderTextureReadWrite)2));
			GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));
			GL.sRGBWrite = true;
			Graphics.Blit((Texture)(object)val2, val3);
			val2.Resize(width, height);
			val2.ReadPixels(new Rect(0f, 0f, (float)width, (float)height), 0, 0);
			RenderTexture.active = null;
			val2.Apply();
			RenderTexture.ReleaseTemporary(val3);
		}
		RenderTexture.ReleaseTemporary(temporary);
		Object.DestroyImmediate((Object)(object)val, true);
		if (transparent)
		{
			cam.clearFlags = clearFlags;
			cam.backgroundColor = backgroundColor;
		}
		GL.sRGBWrite = sRGBWrite;
		return val2;
	}
}

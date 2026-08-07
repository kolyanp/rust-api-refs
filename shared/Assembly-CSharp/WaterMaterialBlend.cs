using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Rust/Water/Material Blend")]
public class WaterMaterialBlend : ScriptableObject
{
	[Serializable]
	public struct BlendState
	{
		public Color Albedo;

		public Color Specular;

		public float Smoothness;

		public float NormalStrength;

		public Color WaterColor;

		public Vector4 ColorExtinction;

		public float ScatterCoefficient;

		public Color SubSurfaceColour;

		public static BlendState Default = new BlendState
		{
			Albedo = Color.white,
			Specular = Color.white,
			Smoothness = 0.5f,
			NormalStrength = 0.5f,
			WaterColor = Color.white,
			ColorExtinction = Vector4.zero,
			ScatterCoefficient = 0.5f,
			SubSurfaceColour = Color.white
		};

		public static BlendState Blend(BlendState a, BlendState b, float t)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			if (t <= 0.001f)
			{
				return a;
			}
			if (t >= 0.999f)
			{
				return b;
			}
			return new BlendState
			{
				Albedo = Color.Lerp(a.Albedo, b.Albedo, t),
				Specular = Color.Lerp(a.Specular, b.Specular, t),
				Smoothness = Mathf.Lerp(a.Smoothness, b.Smoothness, t),
				NormalStrength = Mathf.Lerp(a.NormalStrength, b.NormalStrength, t),
				WaterColor = Color.Lerp(a.WaterColor, b.WaterColor, t),
				ColorExtinction = Vector4.Lerp(Color.op_Implicit(a.WaterColor), Color.op_Implicit(b.WaterColor), t),
				ScatterCoefficient = Mathf.Lerp(a.ScatterCoefficient, b.ScatterCoefficient, t),
				SubSurfaceColour = Color.Lerp(a.SubSurfaceColour, b.SubSurfaceColour, t)
			};
		}
	}

	public BlendState MaterialA = BlendState.Default;

	public BlendState MaterialB = BlendState.Default;
}

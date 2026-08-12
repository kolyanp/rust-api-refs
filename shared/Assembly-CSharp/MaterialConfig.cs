using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Rust/Material Config")]
public class MaterialConfig : ScriptableObject
{
	[Serializable]
	public class EnvironmentVolumeOverride
	{
		public EnvironmentType Environment;

		public Enum Biome;
	}

	public class ShaderParameters<T>
	{
		public string Name;

		public T Arid;

		public T Temperate;

		public T Tundra;

		public T Arctic;

		[FormerlySerializedAs("Temperate")]
		public T Jungle;

		[FormerlySerializedAs("Temperate")]
		public T DeepSea;

		private T[] climates;

		public float FindBlendParameters(Vector3 pos, int biomeOverride, out T src, out T dst)
		{
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)TerrainMeta.BiomeMap == (Object)null)
			{
				src = Temperate;
				dst = Tundra;
				return 0f;
			}
			if (climates == null || climates.Length == 0)
			{
				climates = new T[6] { Arid, Temperate, Tundra, Arctic, Jungle, DeepSea };
			}
			int num = ((biomeOverride != 0) ? biomeOverride : TerrainMeta.BiomeMap.GetBiomeMaxType(pos));
			int num2 = ((biomeOverride != 0) ? biomeOverride : TerrainMeta.BiomeMap.GetBiomeMaxType(pos, ~num));
			src = climates[TerrainBiome.TypeToIndex(num)];
			dst = climates[TerrainBiome.TypeToIndex(num2)];
			return TerrainMeta.BiomeMap.GetBiome(pos, num2);
		}

		public T FindBlendParameters(Vector3 pos, int biomeOverride)
		{
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)TerrainMeta.BiomeMap == (Object)null)
			{
				return Temperate;
			}
			if (climates == null || climates.Length == 0)
			{
				climates = new T[5] { Arid, Temperate, Tundra, Arctic, Jungle };
			}
			int num = ((biomeOverride != 0) ? biomeOverride : TerrainMeta.BiomeMap.GetBiomeMaxType(pos));
			return climates[TerrainBiome.TypeToIndex(num)];
		}
	}

	[Serializable]
	public class ShaderParametersFloat : ShaderParameters<float>
	{
	}

	[Serializable]
	public class ShaderParametersColor : ShaderParameters<Color>
	{
	}

	[Serializable]
	public class ShaderParametersTexture : ShaderParameters<Texture>
	{
	}

	[Serializable]
	public class ShaderParametersToggle : ShaderParameters<bool>
	{
	}

	[Horizontal(5, 0)]
	public ShaderParametersFloat[] Floats;

	[Horizontal(5, 0)]
	public ShaderParametersColor[] Colors;

	[Horizontal(5, 0)]
	public ShaderParametersTexture[] Textures;

	[Horizontal(5, 0)]
	public ShaderParametersToggle[] Toggles;

	public string[] ScaleUV;

	[Horizontal(2, -1)]
	public EnvironmentVolumeOverride[] EnvironmentVolumeOverrides;

	private MaterialPropertyBlock properties;

	public MaterialPropertyBlock GetMaterialPropertyBlock(Material mat, Vector3 pos, Vector3 scale)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected I4, but got Unknown
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		if (properties == null)
		{
			properties = new MaterialPropertyBlock();
		}
		properties.Clear();
		int biomeOverride = 0;
		if (EnvironmentVolumeOverrides.Length != 0)
		{
			EnvironmentType environmentType = EnvironmentManager.Get(pos);
			if (TerrainMeta.IsPointWithinTutorialBounds(pos))
			{
				biomeOverride = 2;
			}
			else
			{
				EnvironmentVolumeOverride[] environmentVolumeOverrides = EnvironmentVolumeOverrides;
				foreach (EnvironmentVolumeOverride environmentVolumeOverride in environmentVolumeOverrides)
				{
					if ((environmentType & environmentVolumeOverride.Environment) != 0)
					{
						biomeOverride = (int)environmentVolumeOverride.Biome;
						break;
					}
				}
			}
		}
		for (int j = 0; j < Floats.Length; j++)
		{
			ShaderParametersFloat shaderParametersFloat = Floats[j];
			float num = shaderParametersFloat.FindBlendParameters(pos, biomeOverride, out var src, out var dst);
			properties.SetFloat(shaderParametersFloat.Name, Mathf.Lerp(src, dst, num));
		}
		for (int k = 0; k < Colors.Length; k++)
		{
			ShaderParametersColor shaderParametersColor = Colors[k];
			float num2 = shaderParametersColor.FindBlendParameters(pos, biomeOverride, out var src2, out var dst2);
			properties.SetColor(shaderParametersColor.Name, Color.Lerp(src2, dst2, num2));
		}
		for (int l = 0; l < Textures.Length; l++)
		{
			ShaderParametersTexture shaderParametersTexture = Textures[l];
			Texture val = shaderParametersTexture.FindBlendParameters(pos, biomeOverride);
			if (Object.op_Implicit((Object)(object)val))
			{
				properties.SetTexture(shaderParametersTexture.Name, val);
			}
		}
		for (int m = 0; m < Toggles.Length; m++)
		{
			ShaderParametersToggle shaderParametersToggle = Toggles[m];
			bool flag = shaderParametersToggle.FindBlendParameters(pos, biomeOverride);
			properties.SetFloat(shaderParametersToggle.Name, flag ? 1f : 0f);
		}
		for (int n = 0; n < ScaleUV.Length; n++)
		{
			Vector4 vector = mat.GetVector(ScaleUV[n]);
			((Vector4)(ref vector))._002Ector(vector.x * scale.y, vector.y * scale.y, vector.z, vector.w);
			properties.SetVector(ScaleUV[n], vector);
		}
		return properties;
	}
}

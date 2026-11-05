using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Rust/Terrain Config")]
public class TerrainConfig : BaseScriptableObject
{
	[Serializable]
	public class SplatOverlay
	{
		public Color Color = new Color(1f, 1f, 1f, 0f);

		[Range(0f, 1f)]
		public float Smoothness;

		[Range(0f, 1f)]
		public float NormalIntensity = 1f;

		[Range(0f, 8f)]
		public float BlendFactor = 0.5f;

		[Range(0.01f, 32f)]
		public float BlendFalloff = 0.5f;

		public SplatOverlay()
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	public class SplatType
	{
		public string Name = "";

		[FormerlySerializedAs("WarmColor")]
		public Color AridColor = Color.white;

		public SplatOverlay AridOverlay = new SplatOverlay();

		[FormerlySerializedAs("Color")]
		public Color TemperateColor = Color.white;

		public SplatOverlay TemperateOverlay = new SplatOverlay();

		[FormerlySerializedAs("ColdColor")]
		public Color TundraColor = Color.white;

		public SplatOverlay TundraOverlay = new SplatOverlay();

		[FormerlySerializedAs("ColdColor")]
		public Color ArcticColor = Color.white;

		public SplatOverlay ArcticOverlay = new SplatOverlay();

		public Color JungleColor = Color.white;

		public SplatOverlay JungleOverlay = new SplatOverlay();

		public PhysicsMaterial Material;

		public float SplatTiling = 5f;

		[Range(0f, 1f)]
		public float UVMIXMult = 0.15f;

		public float UVMIXStart;

		public float UVMIXDist = 100f;

		public SplatType()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	public enum GroundType
	{
		None,
		HardSurface,
		Grass,
		Sand,
		Snow,
		Dirt,
		Gravel
	}

	public bool CastShadows = true;

	public LayerMask GroundMask = LayerMask.op_Implicit(0);

	public LayerMask WaterMask = LayerMask.op_Implicit(0);

	public PhysicsMaterial GenericMaterial;

	public PhysicsMaterial WaterMaterial;

	public Material Material;

	public Material MarginMaterial;

	public Material GeoClipmapMaterial;

	public Material GeoClipmapPixelNormalMaterial;

	public ComputeShader GeoClipCompute;

	public HoleShapeAsset[] HoleShapeAssets;

	public const string TemperateTextureArrayBundle = "terrain/temperate.bundle";

	public const string TropicalTextureArrayBundle = "terrain/tropical.bundle";

	public DynamicResourceRef<Texture>[] AlbedoArrays = new DynamicResourceRef<Texture>[3];

	public DynamicResourceRef<Texture>[] NormalArrays = new DynamicResourceRef<Texture>[3];

	[NonSerialized]
	private Texture[] loadedAlbedoArrays = new Texture[3];

	[NonSerialized]
	private Texture[] loadedNormalArrays = new Texture[3];

	public float HeightMapErrorMin = 5f;

	public float HeightMapErrorMax = 100f;

	public float BaseMapDistanceMin = 100f;

	public float BaseMapDistanceMax = 500f;

	public float ShaderLodMin = 100f;

	public float ShaderLodMax = 600f;

	public SplatType[] Splats = new SplatType[8];

	private string snowMatName;

	private string grassMatName;

	private string sandMatName;

	private List<string> dirtMatNames;

	private List<string> stoneyMatNames;

	private int snowMatID;

	private int grassMatID;

	private int sandMatID;

	private List<int> dirtMatIds;

	private List<int> stoneyMatIds;

	public Texture AlbedoArray => LoadTextureArray(AlbedoArrays, loadedAlbedoArrays);

	public Texture NormalArray => LoadTextureArray(NormalArrays, loadedNormalArrays);

	public string TextureArrayBundle => AlbedoArrays[0].BundleName;

	private static Texture LoadTextureArray(DynamicResourceRef<Texture>[] tiers, Texture[] loadedTiers)
	{
		int num = Mathf.Clamp(QualitySettings.globalTextureMipmapLimit, 0, 2);
		_ = tiers[num];
		return null;
	}

	public PhysicsMaterial[] GetPhysicMaterials()
	{
		PhysicsMaterial[] array = new PhysicsMaterial[Splats.Length];
		for (int i = 0; i < Splats.Length; i++)
		{
			array[i] = Splats[i].Material;
		}
		return array;
	}

	public float GetMaxSplatTiling()
	{
		float num = float.MinValue;
		for (int i = 0; i < Splats.Length; i++)
		{
			if (Splats[i].SplatTiling > num)
			{
				num = Splats[i].SplatTiling;
			}
		}
		return num;
	}

	public float GetMinSplatTiling()
	{
		float num = float.MaxValue;
		for (int i = 0; i < Splats.Length; i++)
		{
			if (Splats[i].SplatTiling < num)
			{
				num = Splats[i].SplatTiling;
			}
		}
		return num;
	}

	[Obsolete("Use GetCurrentGroundTypeNoAlloc instead")]
	public GroundType GetCurrentGroundType(bool isGrounded, RaycastHit hit)
	{
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("TerrainConfig.GetCurrentGroundType"))
		{
			if (string.IsNullOrEmpty(grassMatName))
			{
				dirtMatNames = new List<string>();
				stoneyMatNames = new List<string>();
				SplatType[] splats = Splats;
				foreach (SplatType obj in splats)
				{
					string text = obj.Name.ToLower();
					string name = ((Object)obj.Material).name;
					switch (text)
					{
					case "grass":
						grassMatName = name;
						break;
					case "snow":
						snowMatName = name;
						break;
					case "sand":
						sandMatName = name;
						break;
					case "dirt":
					case "forest":
					case "tundra":
						dirtMatNames.Add(name);
						break;
					case "stones":
					case "gravel":
						stoneyMatNames.Add(name);
						break;
					}
				}
			}
			if (!isGrounded)
			{
				return GroundType.None;
			}
			if ((Object)(object)hit.collider == (Object)null)
			{
				return GroundType.HardSurface;
			}
			PhysicsMaterial materialAt = ColliderEx.GetMaterialAt(hit.collider, hit.point);
			if ((Object)(object)materialAt == (Object)null)
			{
				return GroundType.HardSurface;
			}
			string name2 = ((Object)materialAt).name;
			if (name2 == grassMatName)
			{
				return GroundType.Grass;
			}
			if (name2 == sandMatName)
			{
				return GroundType.Sand;
			}
			if (name2 == snowMatName)
			{
				return GroundType.Snow;
			}
			for (int j = 0; j < dirtMatNames.Count; j++)
			{
				if (dirtMatNames[j] == name2)
				{
					return GroundType.Dirt;
				}
			}
			for (int k = 0; k < stoneyMatNames.Count; k++)
			{
				if (stoneyMatNames[k] == name2)
				{
					return GroundType.Gravel;
				}
			}
			return GroundType.HardSurface;
		}
	}

	public GroundType GetCurrentGroundTypeNoAlloc(bool isGrounded, RaycastHit hit)
	{
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("TerrainConfig.GetCurrentGroundTypeNoAlloc"))
		{
			if (grassMatID == 0)
			{
				dirtMatIds = new List<int>();
				stoneyMatIds = new List<int>();
				SplatType[] splats = Splats;
				foreach (SplatType obj in splats)
				{
					int instanceID = ((Object)obj.Material).GetInstanceID();
					switch (obj.Name.ToLower())
					{
					case "grass":
						grassMatID = instanceID;
						break;
					case "snow":
						snowMatID = instanceID;
						break;
					case "sand":
						sandMatID = instanceID;
						break;
					case "dirt":
					case "forest":
					case "tundra":
						dirtMatIds.Add(instanceID);
						break;
					case "stones":
					case "gravel":
						stoneyMatIds.Add(instanceID);
						break;
					}
				}
			}
			if (!isGrounded)
			{
				return GroundType.None;
			}
			if ((Object)(object)hit.collider == (Object)null)
			{
				return GroundType.HardSurface;
			}
			PhysicsMaterial materialAt = ColliderEx.GetMaterialAt(hit.collider, hit.point);
			if ((Object)(object)materialAt == (Object)null)
			{
				return GroundType.HardSurface;
			}
			int instanceID2 = ((Object)materialAt).GetInstanceID();
			if (instanceID2 == grassMatID)
			{
				return GroundType.Grass;
			}
			if (instanceID2 == sandMatID)
			{
				return GroundType.Sand;
			}
			if (instanceID2 == snowMatID)
			{
				return GroundType.Snow;
			}
			if (dirtMatIds.Contains(instanceID2))
			{
				return GroundType.Dirt;
			}
			if (stoneyMatIds.Contains(instanceID2))
			{
				return GroundType.Gravel;
			}
			return GroundType.HardSurface;
		}
	}

	public TerrainConfig()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
	}
}

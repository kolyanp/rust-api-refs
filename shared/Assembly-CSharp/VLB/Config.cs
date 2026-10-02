using Rust.RenderPipeline.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace VLB;

[HelpURL("http://saladgamer.com/vlb-doc/config/")]
public class Config : ScriptableObject
{
	public int geometryLayerID = 1;

	public string geometryTag = "Untagged";

	public int geometryRenderQueue = 3000;

	public bool forceSinglePass = RustRenderPipeline.IsActive();

	[HighlightNull]
	[SerializeField]
	private Shader beamShader1Pass;

	[HighlightNull]
	[SerializeField]
	[FormerlySerializedAs("beamShader")]
	[FormerlySerializedAs("BeamShader")]
	private Shader beamShader2Pass;

	public int sharedMeshSides = 24;

	public int sharedMeshSegments = 5;

	[Range(0.01f, 2f)]
	public float globalNoiseScale = 0.5f;

	public Vector3 globalNoiseVelocity = Consts.NoiseVelocityDefault;

	[HighlightNull]
	public TextAsset noise3DData;

	public int noise3DSize = 64;

	[HighlightNull]
	public ParticleSystem dustParticlesPrefab;

	private static Config m_Instance;

	public Shader beamShader
	{
		get
		{
			if (!forceSinglePass)
			{
				return beamShader2Pass;
			}
			return beamShader1Pass;
		}
	}

	public Vector4 globalNoiseParam
	{
		get
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			return new Vector4(globalNoiseVelocity.x, globalNoiseVelocity.y, globalNoiseVelocity.z, globalNoiseScale);
		}
	}

	public static Config Instance
	{
		get
		{
			if ((Object)(object)m_Instance == (Object)null)
			{
				Config[] array = Resources.LoadAll<Config>("Config");
				Debug.Assert(array.Length != 0, $"Can't find any resource of type '{typeof(Config)}'. Make sure you have a ScriptableObject of this type in a 'Resources' folder.");
				m_Instance = array[0];
			}
			return m_Instance;
		}
	}

	public void Reset()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		geometryLayerID = 1;
		geometryTag = "Untagged";
		geometryRenderQueue = 3000;
		beamShader1Pass = Shader.Find("Hidden/VolumetricLightBeam1Pass");
		beamShader2Pass = Shader.Find("Hidden/VolumetricLightBeam2Pass");
		sharedMeshSides = 24;
		sharedMeshSegments = 5;
		globalNoiseScale = 0.5f;
		globalNoiseVelocity = Consts.NoiseVelocityDefault;
		Object val = Resources.Load("Noise3D_64x64x64");
		noise3DData = (TextAsset)(object)((val is TextAsset) ? val : null);
		noise3DSize = 64;
		Object val2 = Resources.Load("DustParticles", typeof(ParticleSystem));
		dustParticlesPrefab = (ParticleSystem)(object)((val2 is ParticleSystem) ? val2 : null);
	}

	public ParticleSystem NewVolumetricDustParticles()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)dustParticlesPrefab))
		{
			if (Application.isPlaying)
			{
				Debug.LogError((object)"Failed to instantiate VolumetricDustParticles prefab.");
			}
			return null;
		}
		ParticleSystem val = Object.Instantiate<ParticleSystem>(dustParticlesPrefab);
		val.useAutoRandomSeed = false;
		((Object)val).name = "Dust Particles";
		((Object)((Component)val).gameObject).hideFlags = Consts.ProceduralObjectsHideFlags;
		((Component)val).gameObject.SetActive(true);
		return val;
	}

	public Config()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
	}
}

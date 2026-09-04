using Rust.RenderPipeline.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace VLB;

[HelpURL("http://saladgamer.com/vlb-doc/config/")]
public class Config : ScriptableObject
{
	public int geometryLayerID;

	public string geometryTag;

	public int geometryRenderQueue;

	public bool forceSinglePass;

	[HighlightNull]
	[SerializeField]
	private Shader beamShader1Pass;

	[FormerlySerializedAs("BeamShader")]
	[SerializeField]
	[FormerlySerializedAs("beamShader")]
	[HighlightNull]
	private Shader beamShader2Pass;

	public int sharedMeshSides;

	public int sharedMeshSegments;

	[Range(0.01f, 2f)]
	public float globalNoiseScale;

	public Vector3 globalNoiseVelocity;

	[HighlightNull]
	public TextAsset noise3DData;

	public int noise3DSize;

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
		Object obj = Resources.Load("Noise3D_64x64x64");
		noise3DData = (TextAsset)(object)((obj is TextAsset) ? obj : null);
		noise3DSize = 64;
		Object obj2 = Resources.Load("DustParticles", typeof(ParticleSystem));
		dustParticlesPrefab = (ParticleSystem)(object)((obj2 is ParticleSystem) ? obj2 : null);
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
		ParticleSystem obj = Object.Instantiate<ParticleSystem>(dustParticlesPrefab);
		obj.useAutoRandomSeed = false;
		((Object)obj).name = "Dust Particles";
		((Object)((Component)obj).gameObject).hideFlags = Consts.ProceduralObjectsHideFlags;
		((Component)obj).gameObject.SetActive(true);
		return obj;
	}

	public Config()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		geometryLayerID = 1;
		geometryTag = "Untagged";
		geometryRenderQueue = 3000;
		forceSinglePass = RustRenderPipeline.IsActive();
		sharedMeshSides = 24;
		sharedMeshSegments = 5;
		globalNoiseScale = 0.5f;
		globalNoiseVelocity = Consts.NoiseVelocityDefault;
		noise3DSize = 64;
		((ScriptableObject)this)._002Ector();
	}
}

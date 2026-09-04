using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace VLB;

[ExecuteInEditMode]
[AddComponentMenu("")]
[HelpURL("http://saladgamer.com/vlb-doc/comp-lightbeam/")]
public class BeamGeometry : MonoBehaviour
{
	private VolumetricLightBeam m_Master;

	private Matrix4x4 m_ColorGradientMatrix;

	private MeshType m_CurrentMeshType;

	public MeshRenderer meshRenderer { get; private set; }

	public MeshFilter meshFilter { get; private set; }

	public Material material { get; private set; }

	public Mesh coneMesh { get; private set; }

	public bool visible
	{
		get
		{
			if (((Renderer)meshRenderer).enabled)
			{
				return ((Behaviour)this).enabled;
			}
			return false;
		}
		set
		{
			((Behaviour)this).enabled = value;
			((Renderer)meshRenderer).enabled = value;
		}
	}

	public int sortingLayerID
	{
		get
		{
			return ((Renderer)meshRenderer).sortingLayerID;
		}
		set
		{
			((Renderer)meshRenderer).sortingLayerID = value;
		}
	}

	public int sortingOrder
	{
		get
		{
			return ((Renderer)meshRenderer).sortingOrder;
		}
		set
		{
			((Renderer)meshRenderer).sortingOrder = value;
		}
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)material))
		{
			Object.DestroyImmediate((Object)(object)material);
			material = null;
		}
	}

	public void Initialize(VolumetricLightBeam master, Shader shader)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		HideFlags proceduralObjectsHideFlags = Consts.ProceduralObjectsHideFlags;
		m_Master = master;
		((Component)this).transform.SetParent(((Component)master).transform, false);
		material = new Material(shader);
		((Object)material).hideFlags = proceduralObjectsHideFlags;
		meshRenderer = Utils.GetOrAddComponent<MeshRenderer>(((Component)this).gameObject);
		((Object)meshRenderer).hideFlags = proceduralObjectsHideFlags;
		((Renderer)meshRenderer).material = material;
		((Renderer)meshRenderer).shadowCastingMode = (ShadowCastingMode)0;
		((Renderer)meshRenderer).receiveShadows = false;
		((Renderer)meshRenderer).lightProbeUsage = (LightProbeUsage)0;
		if (SortingLayer.IsValid(m_Master.sortingLayerID))
		{
			sortingLayerID = m_Master.sortingLayerID;
		}
		else
		{
			Debug.LogError((object)$"Beam '{Utils.GetPath(((Component)m_Master).transform)}' has an invalid sortingLayerID ({m_Master.sortingLayerID}). Please fix it by setting a valid layer.");
		}
		sortingOrder = m_Master.sortingOrder;
		meshFilter = Utils.GetOrAddComponent<MeshFilter>(((Component)this).gameObject);
		((Object)meshFilter).hideFlags = proceduralObjectsHideFlags;
		((Object)((Component)this).gameObject).hideFlags = proceduralObjectsHideFlags;
	}

	public void RegenerateMesh()
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		Debug.Assert(Object.op_Implicit((Object)(object)m_Master));
		((Component)this).gameObject.layer = Config.Instance.geometryLayerID;
		((Component)this).gameObject.tag = Config.Instance.geometryTag;
		if (Object.op_Implicit((Object)(object)coneMesh) && m_CurrentMeshType == MeshType.Custom)
		{
			Object.DestroyImmediate((Object)(object)coneMesh);
		}
		m_CurrentMeshType = m_Master.geomMeshType;
		switch (m_Master.geomMeshType)
		{
		case MeshType.Custom:
			coneMesh = MeshGenerator.GenerateConeZ_Radius(1f, 1f, 1f, m_Master.geomCustomSides, m_Master.geomCustomSegments, m_Master.geomCap);
			((Object)coneMesh).hideFlags = Consts.ProceduralObjectsHideFlags;
			meshFilter.mesh = coneMesh;
			break;
		case MeshType.Shared:
			coneMesh = GlobalMesh.mesh;
			meshFilter.sharedMesh = coneMesh;
			break;
		default:
			Debug.LogError((object)"Unsupported MeshType");
			break;
		}
		UpdateMaterialAndBounds();
	}

	private void ComputeLocalMatrix()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Max(m_Master.coneRadiusStart, m_Master.coneRadiusEnd);
		((Component)this).transform.localScale = new Vector3(num, num, m_Master.fadeEnd);
	}

	public void UpdateMaterialAndBounds()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		Debug.Assert(Object.op_Implicit((Object)(object)m_Master));
		material.renderQueue = Config.Instance.geometryRenderQueue;
		float num = m_Master.coneAngle * (MathF.PI / 180f) / 2f;
		material.SetVector("_ConeSlopeCosSin", Vector4.op_Implicit(new Vector2(Mathf.Cos(num), Mathf.Sin(num))));
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(Mathf.Max(m_Master.coneRadiusStart, 0.0001f), Mathf.Max(m_Master.coneRadiusEnd, 0.0001f));
		material.SetVector("_ConeRadius", Vector4.op_Implicit(val));
		float num2 = Mathf.Sign(m_Master.coneApexOffsetZ) * Mathf.Max(Mathf.Abs(m_Master.coneApexOffsetZ), 0.0001f);
		material.SetFloat("_ConeApexOffsetZ", num2);
		if (m_Master.colorMode == ColorMode.Gradient)
		{
			Utils.FloatPackingPrecision floatPackingPrecision = Utils.GetFloatPackingPrecision();
			material.EnableKeyword((floatPackingPrecision == Utils.FloatPackingPrecision.High) ? "VLB_COLOR_GRADIENT_MATRIX_HIGH" : "VLB_COLOR_GRADIENT_MATRIX_LOW");
			m_ColorGradientMatrix = Utils.SampleInMatrix(m_Master.colorGradient, (int)floatPackingPrecision);
			material.SetMatrix("_ColorGradientMatrix", m_ColorGradientMatrix);
		}
		else
		{
			material.DisableKeyword("VLB_COLOR_GRADIENT_MATRIX_HIGH");
			material.DisableKeyword("VLB_COLOR_GRADIENT_MATRIX_LOW");
			material.SetColor("_ColorFlat", m_Master.color);
		}
		if (Consts.BlendingMode_AlphaAsBlack[m_Master.blendingModeAsInt])
		{
			material.EnableKeyword("ALPHA_AS_BLACK");
		}
		else
		{
			material.DisableKeyword("ALPHA_AS_BLACK");
		}
		material.SetInt("_BlendSrcFactor", (int)Consts.BlendingMode_SrcFactor[m_Master.blendingModeAsInt]);
		material.SetInt("_BlendDstFactor", (int)Consts.BlendingMode_DstFactor[m_Master.blendingModeAsInt]);
		material.SetFloat("_AlphaInside", m_Master.alphaInside);
		material.SetFloat("_AlphaOutside", m_Master.alphaOutside);
		material.SetFloat("_AttenuationLerpLinearQuad", m_Master.attenuationLerpLinearQuad);
		material.SetFloat("_DistanceFadeStart", m_Master.fadeStart);
		material.SetFloat("_DistanceFadeEnd", m_Master.fadeEnd);
		material.SetFloat("_DistanceCamClipping", m_Master.cameraClippingDistance);
		material.SetFloat("_FresnelPow", Mathf.Max(0.001f, m_Master.fresnelPow));
		material.SetFloat("_GlareBehind", m_Master.glareBehind);
		material.SetFloat("_GlareFrontal", m_Master.glareFrontal);
		material.SetFloat("_DrawCap", (float)(m_Master.geomCap ? 1 : 0));
		if (m_Master.depthBlendDistance > 0f)
		{
			material.EnableKeyword("VLB_DEPTH_BLEND");
			material.SetFloat("_DepthBlendDistance", m_Master.depthBlendDistance);
		}
		else
		{
			material.DisableKeyword("VLB_DEPTH_BLEND");
		}
		if (m_Master.noiseEnabled && m_Master.noiseIntensity > 0f && Noise3D.isSupported)
		{
			Noise3D.LoadIfNeeded();
			material.EnableKeyword("VLB_NOISE_3D");
			material.SetVector("_NoiseLocal", new Vector4(m_Master.noiseVelocityLocal.x, m_Master.noiseVelocityLocal.y, m_Master.noiseVelocityLocal.z, m_Master.noiseScaleLocal));
			material.SetVector("_NoiseParam", Vector4.op_Implicit(new Vector3(m_Master.noiseIntensity, m_Master.noiseVelocityUseGlobal ? 1f : 0f, m_Master.noiseScaleUseGlobal ? 1f : 0f)));
		}
		else
		{
			material.DisableKeyword("VLB_NOISE_3D");
		}
		ComputeLocalMatrix();
	}

	public void SetClippingPlane(Plane planeWS)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 normal = ((Plane)(ref planeWS)).normal;
		material.EnableKeyword("VLB_CLIPPING_PLANE");
		material.SetVector("_ClippingPlaneWS", new Vector4(normal.x, normal.y, normal.z, ((Plane)(ref planeWS)).distance));
	}

	public void SetClippingPlaneOff()
	{
		material.DisableKeyword("VLB_CLIPPING_PLANE");
	}
}

using UnityEngine;

namespace VLB;

[RequireComponent(typeof(VolumetricLightBeam))]
[DisallowMultipleComponent]
[HelpURL("http://saladgamer.com/vlb-doc/comp-triggerzone/")]
public class TriggerZone : MonoBehaviour
{
	public bool setIsTrigger = true;

	public float rangeMultiplier = 1f;

	private const int kMeshColliderNumSides = 8;

	private Mesh m_Mesh;

	private void Update()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		VolumetricLightBeam component = ((Component)this).GetComponent<VolumetricLightBeam>();
		if (Object.op_Implicit((Object)(object)component))
		{
			MeshCollider orAddComponent = Utils.GetOrAddComponent<MeshCollider>(((Component)this).gameObject);
			Debug.Assert(Object.op_Implicit((Object)(object)orAddComponent));
			float lengthZ = component.fadeEnd * rangeMultiplier;
			float radiusEnd = Mathf.LerpUnclamped(component.coneRadiusStart, component.coneRadiusEnd, rangeMultiplier);
			m_Mesh = MeshGenerator.GenerateConeZ_Radius(lengthZ, component.coneRadiusStart, radiusEnd, 8, 0, cap: false);
			((Object)m_Mesh).hideFlags = Consts.ProceduralObjectsHideFlags;
			orAddComponent.sharedMesh = m_Mesh;
			if (setIsTrigger)
			{
				orAddComponent.convex = true;
				((Collider)orAddComponent).isTrigger = true;
			}
			Object.Destroy((Object)(object)this);
		}
	}
}

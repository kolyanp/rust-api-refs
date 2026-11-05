using UnityEngine;

public class PredictedThrowGhost : MonoBehaviour, IClientComponent, IPrefabPostProcess
{
	[HideInInspector]
	public Poolable poolable;

	[HideInInspector]
	public Rigidbody body;

	[HideInInspector]
	public Light[] lights;

	[HideInInspector]
	public ParticleSystem[] particleSystems;

	[HideInInspector]
	public float sweepRadius;

	[HideInInspector]
	public Vector3 meshCenter;

	public void PostProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		poolable = ((Component)this).GetComponent<Poolable>();
		body = ((Component)this).GetComponent<Rigidbody>();
		lights = ((Component)this).GetComponentsInChildren<Light>(true);
		particleSystems = ((Component)this).GetComponentsInChildren<ParticleSystem>(true);
		Collider component = ((Component)this).GetComponent<Collider>();
		SphereCollider val = (SphereCollider)(object)((component is SphereCollider) ? component : null);
		float num;
		if (val == null)
		{
			CapsuleCollider val2 = (CapsuleCollider)(object)((component is CapsuleCollider) ? component : null);
			if (val2 == null)
			{
				BoxCollider val3 = (BoxCollider)(object)((component is BoxCollider) ? component : null);
				num = ((val3 == null) ? 0.05f : (Mathf.Min(new float[3]
				{
					val3.size.x,
					val3.size.y,
					val3.size.z
				}) * 0.5f));
			}
			else
			{
				num = val2.radius;
			}
		}
		else
		{
			num = val.radius;
		}
		sweepRadius = num;
		MeshFilter componentInChildren = ((Component)this).GetComponentInChildren<MeshFilter>(true);
		Vector3 val4;
		if (!((Object)(object)componentInChildren != (Object)null) || !((Object)(object)componentInChildren.sharedMesh != (Object)null))
		{
			val4 = Vector3.zero;
		}
		else
		{
			Transform transform = ((Component)this).transform;
			Transform transform2 = ((Component)componentInChildren).transform;
			Bounds bounds = componentInChildren.sharedMesh.bounds;
			val4 = transform.InverseTransformPoint(transform2.TransformPoint(bounds.center));
		}
		meshCenter = val4;
	}
}

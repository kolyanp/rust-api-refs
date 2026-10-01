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
	public float sweepRadius;

	public void PostProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		poolable = ((Component)this).GetComponent<Poolable>();
		body = ((Component)this).GetComponent<Rigidbody>();
		lights = ((Component)this).GetComponentsInChildren<Light>(true);
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
	}
}

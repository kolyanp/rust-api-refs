using UnityEngine;

public class ParticleDisableOnParentDestroy : MonoBehaviour, IOnParentDestroying
{
	public float destroyAfterSeconds;

	public void OnParentDestroying()
	{
		ParticleSystem component = ((Component)this).GetComponent<ParticleSystem>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.enableEmission = false;
		}
		if (!PoolableEx.IsPooledPrefabChild(((Component)this).gameObject))
		{
			((Component)this).transform.parent = null;
			if (destroyAfterSeconds > 0f)
			{
				GameManager.Destroy(((Component)this).gameObject, destroyAfterSeconds);
			}
		}
	}
}

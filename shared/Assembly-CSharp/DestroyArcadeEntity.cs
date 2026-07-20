using UnityEngine;

public class DestroyArcadeEntity : BaseMonoBehaviour
{
	public ArcadeEntity ent;

	public float TimeToDie = 1f;

	public float TimeToDieVariance;

	private void Start()
	{
		Invoke(DestroyAction, TimeToDie, TimeToDieVariance * 0.5f);
	}

	private void DestroyAction()
	{
		if (((Object)(object)ent != (Object)null) & ent.host)
		{
			Object.Destroy((Object)(object)((Component)ent).gameObject);
		}
	}
}

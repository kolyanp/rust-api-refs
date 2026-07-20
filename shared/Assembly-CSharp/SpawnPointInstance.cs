using Rust;
using UnityEngine;

public class SpawnPointInstance : MonoBehaviour
{
	internal BaseEntity Entity;

	public ISpawnPointUser parentSpawnPointUser;

	public BaseSpawnPoint parentSpawnPoint;

	public void Notify()
	{
		if (!ObjectEx.IsUnityNull(parentSpawnPointUser))
		{
			parentSpawnPointUser.ObjectSpawned(this);
		}
		if (Object.op_Implicit((Object)(object)parentSpawnPoint))
		{
			parentSpawnPoint.ObjectSpawned(this);
		}
	}

	public void Retire()
	{
		if (!ObjectEx.IsUnityNull(parentSpawnPointUser))
		{
			parentSpawnPointUser.ObjectRetired(this);
		}
		if (Object.op_Implicit((Object)(object)parentSpawnPoint))
		{
			parentSpawnPoint.ObjectRetired(this);
		}
	}

	protected void OnDestroy()
	{
		if (!Application.isQuitting)
		{
			Retire();
		}
	}
}

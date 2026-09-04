using Rust;
using UnityEngine;

public class SpawnPointInstance : MonoBehaviour
{
	internal BaseEntity Entity;

	public ISpawnPointUser parentSpawnPointUser;

	public BaseSpawnPoint parentSpawnPoint;

	private bool notified;

	public bool blockSpawnHandlerRespawns { get; set; }

	public void Notify()
	{
		if (!notified)
		{
			if (!ObjectEx.IsUnityNull(parentSpawnPointUser))
			{
				parentSpawnPointUser.ObjectSpawned(this);
			}
			if (Object.op_Implicit((Object)(object)parentSpawnPoint))
			{
				parentSpawnPoint.ObjectSpawned(this);
			}
			notified = true;
		}
	}

	public void Retire()
	{
		if (notified)
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
	}

	protected void OnDestroy()
	{
		if (!Application.isQuitting)
		{
			Retire();
		}
	}
}

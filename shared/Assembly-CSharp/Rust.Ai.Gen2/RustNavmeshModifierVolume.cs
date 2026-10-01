using UnityEngine;

namespace Rust.Ai.Gen2;

public class RustNavmeshModifierVolume : MonoBehaviour, IServerComponent
{
	public static SparseGrid<RustNavmeshModifierVolume> AllModifierVolumes = new SparseGrid<RustNavmeshModifierVolume>();

	private static int liveCount;

	public static bool HasAny => liveCount > 0;

	private void Awake()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		AllModifierVolumes.Add(((Component)this).transform.position, this);
		liveCount++;
	}

	private void OnDestroy()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		AllModifierVolumes.Remove(((Component)this).transform.position, this);
		liveCount--;
	}
}

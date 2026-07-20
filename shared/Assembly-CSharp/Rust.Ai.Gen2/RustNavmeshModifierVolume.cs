using UnityEngine;

namespace Rust.Ai.Gen2;

public class RustNavmeshModifierVolume : MonoBehaviour, IServerComponent
{
	public static SparseGrid<RustNavmeshModifierVolume> AllModifierVolumes = new SparseGrid<RustNavmeshModifierVolume>();

	private void Awake()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		AllModifierVolumes.Add(((Component)this).transform.position, this);
	}

	private void OnDestroy()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		AllModifierVolumes.Remove(((Component)this).transform.position, this);
	}
}

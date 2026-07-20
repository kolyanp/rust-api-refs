using UnityEngine;

namespace Rust.Safety;

public static class CheckExtensions
{
	public static bool IsValidAttackTarget(this BasePlayer ply)
	{
		return Check.IsValidAttackTarget(ply);
	}

	public static bool IsInsideDeepSea(this BaseNetworkable entity)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return DeepSeaManager.IsInsideDeepSea(((Component)entity).transform.position);
	}
}

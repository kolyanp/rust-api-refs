using UnityEngine;

namespace Rust.Ai.Gen2;

public class SeagullCorpse : BaseCorpse
{
	public override void ServerInitCorpse(BaseEntity pr, Vector3 posOnDeath, Quaternion rotOnDeath, BasePlayer.PlayerFlags playerFlagsOnDeath, ModelState modelState)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.SetPositionAndRotation(BaseCorpse.LiftAboveTerrain(posOnDeath), rotOnDeath);
	}
}

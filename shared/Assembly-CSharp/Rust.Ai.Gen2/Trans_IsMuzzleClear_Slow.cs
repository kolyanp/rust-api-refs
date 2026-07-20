using UnityEngine;

namespace Rust.Ai.Gen2;

public class Trans_IsMuzzleClear_Slow : FSMSlowTransitionBase
{
	private NpcShootingComponent _shootingComponent;

	private NpcShootingComponent ShootingComponent => _shootingComponent ?? (_shootingComponent = ((Component)Owner).GetComponent<NpcShootingComponent>());

	protected override bool EvaluateAtInterval(ref FSMPayload payload)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Trans_IsMuzzleClear_Slow"))
		{
			if (!base.Senses.FindTarget(out var target))
			{
				return false;
			}
			if (!base.Senses.FindLKP(target, out var lkp))
			{
				return false;
			}
			Vector3 muzzleEstimatedPositionOnServer = ShootingComponent.GetMuzzleEstimatedPositionOnServer(lkp);
			Vector3 entityPointToShootAt = NpcShootingComponent.GetEntityPointToShootAt(target, lkp);
			return ShootingComponent.CanShootFromAt(muzzleEstimatedPositionOnServer, entityPointToShootAt, "muzzle clear");
		}
	}
}

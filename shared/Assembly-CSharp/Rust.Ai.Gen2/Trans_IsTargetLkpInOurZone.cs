using UnityEngine;

namespace Rust.Ai.Gen2;

public class Trans_IsTargetLkpInOurZone : FSMTransitionBase
{
	private NpcZoneComponent _npcZoneComponent;

	private NpcBarkComponent _barkComponent;

	private NpcZoneComponent NpcZoneComponent => _npcZoneComponent ?? (_npcZoneComponent = ((Component)Owner).GetComponent<NpcZoneComponent>());

	private NpcBarkComponent BarkComponent => _barkComponent ?? (_barkComponent = ((Component)Owner).GetComponent<NpcBarkComponent>());

	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Trans_IsTargetLkpInOurZone"))
		{
			if (!base.Senses.FindTargetLKP(out var lkp))
			{
				return false;
			}
			if (NpcZoneComponent.IsPointInsideZone(lkp))
			{
				return true;
			}
			BarkComponent.PlayVoicelineFromCategory(ENPCVoicelineCategory.Hold);
			return false;
		}
	}
}

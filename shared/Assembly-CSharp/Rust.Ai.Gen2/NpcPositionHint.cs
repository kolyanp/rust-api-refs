using UnityEngine;

namespace Rust.Ai.Gen2;

public class NpcPositionHint : MonoBehaviour, IServerComponent
{
	private void OnDrawGizmosSelected()
	{
		NpcLevelScript npcLevelScript = default(NpcLevelScript);
		if (!((Object)(object)((Component)this).transform.parent == (Object)null) && ((Component)((Component)this).transform.parent).TryGetComponent<NpcLevelScript>(ref npcLevelScript))
		{
			npcLevelScript.OnDrawGizmosSelected();
		}
	}

	private void OnValidate()
	{
		NpcLevelScript npcLevelScript = default(NpcLevelScript);
		if (!((Object)(object)((Component)this).transform.parent == (Object)null) && ((Component)((Component)this).transform.parent).TryGetComponent<NpcLevelScript>(ref npcLevelScript) && !npcLevelScript.positionHints.Contains(this))
		{
			npcLevelScript.positionHints.Add(this);
		}
	}
}

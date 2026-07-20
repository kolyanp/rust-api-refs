using System.Collections.Generic;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class NpcLevelTrigger : TriggerBase, IServerComponent
{
	private HashSet<BasePlayer> playersInside = new HashSet<BasePlayer>();

	internal override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if ((Object)(object)obj == (Object)null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if ((Object)(object)baseEntity == (Object)null)
		{
			return null;
		}
		if (!baseEntity.IsNonNpcPlayer())
		{
			return null;
		}
		if (baseEntity.isClient)
		{
			return null;
		}
		return ((Component)baseEntity).gameObject;
	}

	internal override void OnEntityEnter(BaseEntity ent)
	{
		base.OnEntityEnter(ent);
		if (ent.ToNonNpcPlayer(out var player))
		{
			playersInside.Add(player);
		}
	}

	internal override void OnEntityLeave(BaseEntity ent)
	{
		base.OnEntityLeave(ent);
		if (ent.ToNonNpcPlayer(out var player))
		{
			playersInside.Remove(player);
		}
	}

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
		if (!((Object)(object)((Component)this).transform.parent == (Object)null) && ((Component)((Component)this).transform.parent).TryGetComponent<NpcLevelScript>(ref npcLevelScript) && !npcLevelScript.linkedTriggers.Contains(this))
		{
			npcLevelScript.linkedTriggers.Add(this);
		}
	}
}

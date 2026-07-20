using ConVar;
using Rust.Ai;
using UnityEngine;

public class TriggerQTE : TriggerBase, IServerComponent
{
	public WildlifeHazard Entity;

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
		if (baseEntity.isClient)
		{
			return null;
		}
		if ((Object)(object)(baseEntity as BasePlayer) == (Object)null)
		{
			return null;
		}
		if (baseEntity.IsNpc)
		{
			return null;
		}
		if (AI.ignoreplayers)
		{
			return null;
		}
		if (SimpleAIMemory.PlayerIgnoreList.Contains(baseEntity as BasePlayer))
		{
			return null;
		}
		return ((Component)baseEntity).gameObject;
	}

	internal override void OnEntityEnter(BaseEntity ent)
	{
		base.OnEntityEnter(ent);
		if ((Object)(object)Entity == (Object)null)
		{
			Debug.LogWarning((object)"TriggerQTE with no Entity linked", (Object)(object)((Component)this).gameObject);
		}
		else
		{
			Entity.TriggeredByPlayer(ent as BasePlayer);
		}
	}
}

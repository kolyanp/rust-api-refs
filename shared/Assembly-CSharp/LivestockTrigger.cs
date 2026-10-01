using Rust.Ai.Gen2;
using UnityEngine;

public class LivestockTrigger : TriggerBase
{
	public LivestockVendor OwnerVendor;

	internal override GameObject InterestedInObject(GameObject obj)
	{
		if ((Object)(object)base.InterestedInObject(obj) == (Object)null)
		{
			return null;
		}
		if ((Object)(object)OwnerVendor == (Object)null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if ((Object)(object)baseEntity != (Object)null && baseEntity is LivestockAnimal && baseEntity.isServer == OwnerVendor.isServer)
		{
			return obj;
		}
		return null;
	}
}

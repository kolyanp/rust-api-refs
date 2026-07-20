using UnityEngine;

public class BaseLadder : BaseCombatEntity
{
	[SerializeField]
	private TriggerParent triggerParent;

	public override bool ShouldBlockProjectiles()
	{
		return false;
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		if (base.isServer && Object.op_Implicit((Object)(object)triggerParent))
		{
			BaseVehicle baseVehicle = parent as BaseVehicle;
			bool flag = (Object)(object)baseVehicle != (Object)null;
			((Behaviour)triggerParent).enabled = flag;
			triggerParent.associatedMountable = (flag ? baseVehicle : null);
		}
	}
}

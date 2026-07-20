using UnityEngine;

public class BoxStorage : StorageContainer
{
	public Hopper.MountType HopperMountType;

	public override bool PreserveChildrenWhenReskinning => true;

	public override Vector3 GetDropPosition()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return ClosestPoint(base.GetDropPosition() + base.LastAttackedDir * 10f);
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if (children.Count != 0)
		{
			pickupErrorToFormat = (format: PickupErrors.ItemHasAttachment, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}
}

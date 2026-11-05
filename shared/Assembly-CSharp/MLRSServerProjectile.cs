public class MLRSServerProjectile : ServerProjectile
{
	public override bool HasRangeLimit => false;

	protected override int mask => base.mask & -1048577;

	protected override bool IsAValidHit(BaseEntity hitEnt)
	{
		if (!base.IsAValidHit(hitEnt))
		{
			return false;
		}
		if (hitEnt.IsValid())
		{
			return !(hitEnt is MLRS);
		}
		return true;
	}
}

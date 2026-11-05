using System;

public readonly struct HeldEntityServerUseParams
{
	public readonly float damageModifier;

	public readonly float speedModifier;

	public readonly Nullable originOverride;

	public readonly bool useBulletThickness;

	public readonly bool useProtectionForNPCs;

	public static HeldEntityServerUseParams Default => new HeldEntityServerUseParams(1f, 1f, null, true, false);

	public HeldEntityServerUseParams(float damageModifier = 1f, float speedModifier = 1f, Nullable originOverride = null, bool useBulletThickness = true, bool useProtectionForNPCs = false)
	{
		this.damageModifier = damageModifier;
		this.speedModifier = speedModifier;
		this.originOverride = originOverride;
		this.useBulletThickness = useBulletThickness;
		this.useProtectionForNPCs = useProtectionForNPCs;
	}
}

using UnityEngine;

public class ComputerStationIO : IOEntity
{
	public bool visibilityPassesThroughParent;

	public int powerConsumption = 1;

	private ComputerStation parentComputer;

	public override bool VisibilityPassesThroughParent => visibilityPassesThroughParent;

	public override int ConsumptionAmount()
	{
		return powerConsumption;
	}

	public override bool ShouldDrainBattery(IOEntity battery)
	{
		if ((Object)(object)parentComputer == (Object)null && parentEntity.IsValid(isServer))
		{
			parentComputer = ((Component)parentEntity.Get(isServer)).GetComponent<ComputerStation>();
		}
		if ((Object)(object)parentComputer != (Object)null && parentComputer.IsOn())
		{
			return base.ShouldDrainBattery(battery);
		}
		return false;
	}

	public override void ResetIOState()
	{
		base.ResetIOState();
		if (IsOn())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags))
			{
				flagsUpdateScope.Set(Flags.On, b: false);
			}
		}
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		bool flag = IsOn();
		bool flag2 = IsPowered();
		if (flag != flag2)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags))
			{
				flagsUpdateScope.Set(Flags.On, flag2);
			}
		}
	}
}

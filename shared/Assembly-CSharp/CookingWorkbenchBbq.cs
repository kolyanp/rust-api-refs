using UnityEngine;

public class CookingWorkbenchBbq : BaseOven
{
	private CookingWorkbench ParentBench
	{
		get
		{
			if (!parentEntity.IsValid(base.isServer))
			{
				return null;
			}
			return parentEntity.Get(base.isServer) as CookingWorkbench;
		}
	}

	public override void OnAttacked(HitInfo info)
	{
		if ((Object)(object)ParentBench != (Object)null && base.isServer)
		{
			ParentBench.Hurt(info);
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (!base.isServer || (next & Flags.On) == Flags.On == ((old & Flags.On) == Flags.On))
		{
			return;
		}
		CookingWorkbench parentBench = ParentBench;
		if ((Object)(object)parentBench != (Object)null)
		{
			bool b = (parentBench.IsOn() ? parentBench.IsOn() : ((next & Flags.On) == Flags.On));
			using FlagsUpdateScope flagsUpdateScope = parentBench.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved9, b);
		}
	}

	public override void AdminKill()
	{
		BaseEntity baseEntity = GetParentEntity();
		if ((Object)(object)baseEntity != (Object)null)
		{
			baseEntity.AdminKill();
		}
	}
}

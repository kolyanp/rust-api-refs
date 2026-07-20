using UnityEngine;

public class LockedByEntCrate : LootContainer
{
	public GameObject lockingEnt;

	public void SetLockingEnt(GameObject ent)
	{
		CancelInvoke(Think);
		SetLocked(isLocked: false);
		lockingEnt = ent;
		if ((Object)(object)lockingEnt != (Object)null)
		{
			InvokeRepeating(Think, Random.Range(0f, 1f), 1f);
			SetLocked(isLocked: true);
		}
	}

	public void SetLocked(bool isLocked)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.OnFire, isLocked);
		flagsUpdateScope.Set(Flags.Locked, isLocked);
	}

	public void Think()
	{
		if ((Object)(object)lockingEnt == (Object)null && IsLocked())
		{
			SetLockingEnt(null);
		}
	}
}

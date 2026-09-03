using System;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class LockedByEntCrate : LootContainer
{
	[NonSerialized]
	public BaseEntity lockingEnt;

	public override void Save(SaveInfo info)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (info.forDisk && lockingEnt.IsValid())
		{
			info.msg.lockedByEntCrate = Pool.Get<LockedByEntCrate>();
			info.msg.lockedByEntCrate.lockingEntId = lockingEnt.net.ID;
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.fromDisk && info.msg.lockedByEntCrate != null && info.msg.lockedByEntCrate.lockingEntId != default(NetworkableId))
		{
			lockingEnt = BaseNetworkable.serverEntities.Find(info.msg.lockedByEntCrate.lockingEntId) as BaseEntity;
			SetLockingEnt(lockingEnt);
		}
	}

	public void SetLockingEnt(BaseEntity ent)
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

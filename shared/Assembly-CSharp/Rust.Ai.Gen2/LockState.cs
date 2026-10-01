using System.Collections.Generic;
using Facepunch;
using UnityEngine.Assertions;

namespace Rust.Ai.Gen2;

public class LockState
{
	public class LockHandle
	{
	}

	private HashSet<LockHandle> locks = new HashSet<LockHandle>();

	public bool IsLocked => locks.Count > 0;

	public LockHandle AddLock()
	{
		LockHandle lockHandle = Pool.Get<LockHandle>();
		locks.Add(lockHandle);
		return lockHandle;
	}

	public bool RemoveLock(ref LockHandle handle)
	{
		if (handle == null)
		{
			return false;
		}
		bool flag = locks.Remove(handle);
		Assert.IsTrue(flag, "Trying to remove a lock that doesn't exist");
		if (flag)
		{
			Pool.FreeUnsafe<LockHandle>(ref handle);
		}
		return flag;
	}
}

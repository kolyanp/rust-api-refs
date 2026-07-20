using System.Collections.Generic;
using Facepunch;
using UnityEngine;

namespace Rust.Rendering.IndirectInstancing;

public static class MotionToggleEx
{
	public static void BroadcastMotionStartSlow(this GameObject go)
	{
		List<IMotionToggle> list = Pool.Get<List<IMotionToggle>>();
		go.GetComponentsInChildren<IMotionToggle>(list);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].MotionStart();
		}
		Pool.FreeUnmanaged<IMotionToggle>(ref list);
	}

	public static void BroadcastBeforeMaterialChange(this GameObject go)
	{
		List<IMotionToggle> list = Pool.Get<List<IMotionToggle>>();
		go.GetComponentsInChildren<IMotionToggle>(list);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].OnBeforeMaterialChange();
		}
		Pool.FreeUnmanaged<IMotionToggle>(ref list);
	}

	public static void BroadcastAfterMaterialChange(this GameObject go)
	{
		List<IMotionToggle> list = Pool.Get<List<IMotionToggle>>();
		go.GetComponentsInChildren<IMotionToggle>(list);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].OnAfterMaterialChange();
		}
		Pool.FreeUnmanaged<IMotionToggle>(ref list);
	}
}

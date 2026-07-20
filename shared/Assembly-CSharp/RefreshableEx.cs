using System.Collections.Generic;
using Facepunch;
using UnityEngine;

internal static class RefreshableEx
{
	public static void BroadcastRefresh(this GameObject go)
	{
		List<IRefreshable> list = Pool.Get<List<IRefreshable>>();
		go.GetComponentsInChildren<IRefreshable>(list);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].Refresh();
		}
		Pool.FreeUnmanaged<IRefreshable>(ref list);
	}
}

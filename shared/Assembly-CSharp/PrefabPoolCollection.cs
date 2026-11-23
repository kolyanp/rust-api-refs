using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PrefabPoolCollection
{
	public Dictionary<ulong, PrefabPool> storage = new Dictionary<ulong, PrefabPool>();

	private PrefabKey prefabKey = new PrefabKey();

	private GameManager gameManager;

	private bool isClient;

	public PrefabPoolCollection(GameManager manager, bool client)
	{
		gameManager = manager;
		isClient = client;
	}

	public void Push(GameObject instance)
	{
		Poolable component = instance.GetComponent<Poolable>();
		ulong key = prefabKey.Get(component.prefabID);
		if (!storage.TryGetValue(key, out var value))
		{
			int targetCapacity = (isClient ? component.ClientCount : component.ServerCount);
			value = new PrefabPool(component.prefabID, targetCapacity);
			storage.Add(key, value);
		}
		value.Push(component, gameManager.FindPrefab(component.prefabID));
	}

	public bool TryGetPool(uint id, out PrefabPool pool)
	{
		return storage.TryGetValue(prefabKey.Get(id), out pool);
	}

	public GameObject Pop(uint id, Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion))
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetPool(id, out var pool))
		{
			return pool.Pop(pos, rot);
		}
		return null;
	}

	public void Clear(string filter = null)
	{
		if (string.IsNullOrEmpty(filter))
		{
			foreach (KeyValuePair<ulong, PrefabPool> item in storage)
			{
				item.Value.Clear();
			}
			return;
		}
		foreach (KeyValuePair<ulong, PrefabPool> item2 in storage)
		{
			if (StringEx.Contains(item2.Value.PrefabName, filter, CompareOptions.IgnoreCase))
			{
				item2.Value.Clear();
			}
		}
	}
}

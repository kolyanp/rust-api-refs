using System.Collections.Generic;
using UnityEngine;

public class PrefabPool
{
	private EncryptedValue<uint> prefabID = 0u;

	public Stack<Poolable> stack = new Stack<Poolable>();

	public uint PrefabID => prefabID.Get();

	public string PrefabName => StringPool.Get(PrefabID);

	public int Missed { get; private set; }

	public int Pushed { get; private set; }

	public int Popped { get; private set; }

	public int Count => stack.Count;

	public int TargetCapacity { get; private set; }

	public PrefabPool(uint prefabId, int targetCapacity)
	{
		prefabID = prefabId;
		TargetCapacity = targetCapacity;
	}

	public void Push(Poolable info, GameObject prefab)
	{
		Pushed++;
		stack.Push(info);
		info.EnterPool(prefab);
	}

	public GameObject Pop(Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion))
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		while (stack.Count > 0)
		{
			Poolable poolable = stack.Pop();
			Popped++;
			if (Object.op_Implicit((Object)(object)poolable))
			{
				((Component)poolable).transform.SetPositionAndRotation(pos, rot);
				poolable.LeavePool();
				return ((Component)poolable).gameObject;
			}
		}
		Missed++;
		return null;
	}

	public void Clear()
	{
		foreach (Poolable item in stack)
		{
			if (Object.op_Implicit((Object)(object)item))
			{
				Object.Destroy((Object)(object)((Component)item).gameObject);
			}
		}
		stack.Clear();
	}
}

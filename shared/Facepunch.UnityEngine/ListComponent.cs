using System;
using UnityEngine;

public abstract class ListComponent<T> : ListComponent where T : MonoBehaviour
{
	public static ListHashSet<T> InstanceList = new ListHashSet<T>();

	public override void Setup()
	{
		if (!InstanceList.Contains((T)(object)((this is T) ? this : null)))
		{
			InstanceList.Add((T)(object)((this is T) ? this : null));
		}
	}

	public override void Clear()
	{
		InstanceList.Remove((T)(object)((this is T) ? this : null));
	}

	public static void RunOnAll(Action<T> toRun)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<T> enumerator = InstanceList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				toRun?.Invoke(current);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}
}
public abstract class ListComponent : FacepunchBehaviour
{
	public abstract void Setup();

	public abstract void Clear();

	protected virtual void OnEnable()
	{
		Setup();
	}

	protected virtual void OnDisable()
	{
		Clear();
	}
}

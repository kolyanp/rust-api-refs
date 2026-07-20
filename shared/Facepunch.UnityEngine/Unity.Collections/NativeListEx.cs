namespace Unity.Collections;

public static class NativeListEx
{
	public static void Expand<T>(this ref NativeList<T> list, int newCapacity, bool copyContents = true) where T : unmanaged
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (!list.IsCreated || newCapacity > list.Capacity)
		{
			if (list.IsCreated)
			{
				if (copyContents)
				{
					list.Capacity = newCapacity;
				}
				else
				{
					list.Dispose();
					list = new NativeList<T>(newCapacity, AllocatorHandle.op_Implicit((Allocator)4));
				}
			}
			else
			{
				list = new NativeList<T>(newCapacity, AllocatorHandle.op_Implicit((Allocator)4));
			}
		}
		if (!copyContents)
		{
			list.Clear();
		}
	}

	public static void SafeDispose<T>(this ref NativeList<T> list) where T : unmanaged
	{
		if (list.IsCreated)
		{
			list.Dispose();
		}
	}

	public static void CopyFrom<T>(this ref NativeList<T> list, in ReadOnly<T> from) where T : unmanaged
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		list.Resize(from.Length, (NativeArrayOptions)0);
		from.CopyTo(list.AsArray());
	}
}

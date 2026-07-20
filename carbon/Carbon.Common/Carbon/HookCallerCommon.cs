using System;
using System.Collections.Generic;
using System.Reflection;
using Carbon.Base;

namespace Carbon;

public abstract class HookCallerCommon
{
	public class HookArgPool
	{
		public static readonly int BufferSize = 256;

		private readonly int length;

		private readonly Stack<object[]> pool;

		private readonly object syncRoot = new object();

		private int rentedExtra;

		private int rented;

		private int returned;

		public int RentedExtra => rentedExtra;

		public int Rented => rented;

		public int Returned => returned;

		public int Length => length;

		public int Count => pool.Count;

		public HookArgPool(int length)
		{
			this.length = length;
			rented = 0;
			returned = 0;
			rentedExtra = 0;
			pool = new Stack<object[]>(BufferSize);
			for (int i = 0; i < BufferSize; i++)
			{
				pool.Push(new object[length]);
			}
		}

		public object[] Rent()
		{
			lock (syncRoot)
			{
				if (pool.Count > 0)
				{
					rented++;
					return pool.Pop();
				}
				rentedExtra++;
				return new object[length];
			}
		}

		public void Return(object[] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = null;
			}
			lock (syncRoot)
			{
				returned++;
				pool.Push(array);
			}
		}
	}

	public struct Conflict
	{
		public BaseHookable Hookable;

		public uint Hook;

		public object Result;

		public static Conflict Make(BaseHookable hookable, uint hook, object result)
		{
			return new Conflict
			{
				Hookable = hookable,
				Hook = hook,
				Result = result
			};
		}
	}

	public readonly Dictionary<int, HookArgPool> _argumentBuffer = new Dictionary<int, HookArgPool>();

	public readonly Dictionary<uint, DateTime> _lastDeprecatedWarningAt = new Dictionary<uint, DateTime>();

	public abstract object[] AllocateBuffer(int count);

	public abstract object[] RescaleBuffer(object[] oldBuffer, int newScale, BaseHookable.CachedHook hook);

	public abstract void ProcessDefaults(object[] buffer, BaseHookable.CachedHook hook);

	public abstract void ReturnBuffer(object[] buffer);

	public abstract object CallHook<T>(T hookable, uint hookId, BindingFlags flags, object[] args) where T : BaseHookable;

	public abstract object CallDeprecatedHook<T>(T plugin, uint oldHookId, uint newHookId, DateTime expireDate, BindingFlags flags, object[] args) where T : BaseHookable;
}

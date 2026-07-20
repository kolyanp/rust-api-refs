using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Facepunch;
using Facepunch.Extend;
using Network.Visibility;
using Oxide.Core;
using Unity.Collections;
using UnityEngine;

namespace Network;

public class Networkable : IPooled
{
	private class UpdateSubs_AsyncState
	{
		public BufferList<Networkable> Networkables;

		public BufferList<List<Group>> Added = new BufferList<List<Group>>();

		public BufferList<List<Group>> Removed = new BufferList<List<Group>>();

		public BufferList<bool> IsDone = new BufferList<bool>();
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003CUpdateSubscriptions_003Eg__ProcessBatchAsync_007C36_0_003Ed : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public UpdateSubs_AsyncState state;

		public int batchIndex;

		public int batchSize;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					SwitchToThreadPoolAwaitable val = UniTask.SwitchToThreadPool();
					val2 = ((SwitchToThreadPoolAwaitable)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003C_003CUpdateSubscriptions_003Eg__ProcessBatchAsync_007C36_0_003Ed>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
				}
				((Awaiter)(ref val2)).GetResult();
				_003CUpdateSubscriptions_003Eg__ProcessBatch_007C36_1(state, batchIndex, batchSize);
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	public NetworkableId ID;

	public Group group;

	public Group secondaryGroup;

	public Subscriber subscriber;

	public NetworkHandler handler;

	public bool updateSubscriptions;

	public Server sv;

	internal Client cl;

	private static UpdateSubs_AsyncState updateSubsAsyncState = new UpdateSubs_AsyncState();

	public Connection connection { get; private set; }

	public ISubscriberStrategy SubStrategy { get; set; }

	public bool ShouldUpdateSubscriptions
	{
		get
		{
			if (updateSubscriptions)
			{
				return subscriber != null;
			}
			return false;
		}
	}

	public void Destroy()
	{
		CloseSubscriber();
		if (((NetworkableId)(ref ID)).IsValid)
		{
			SwitchGroup(null);
			if (sv != null)
			{
				sv.ReturnUID(ID.Value);
			}
		}
	}

	public void EnterPool()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		ID = default(NetworkableId);
		connection = null;
		group = null;
		secondaryGroup = null;
		sv = null;
		cl = null;
		handler = null;
		updateSubscriptions = false;
		SubStrategy = null;
	}

	public void LeavePool()
	{
	}

	public void StartSubscriber()
	{
		if (subscriber != null)
		{
			Debug.Log((object)"BecomeSubscriber called twice!");
			return;
		}
		subscriber = sv.visibility.CreateSubscriber(connection);
		OnSubscriptionChange();
	}

	public void OnConnected(Connection c)
	{
		connection = c;
	}

	public void OnDisconnected()
	{
		connection = null;
		CloseSubscriber();
	}

	public void CloseSubscriber()
	{
		if (subscriber != null)
		{
			sv.visibility.DestroySubscriber(ref subscriber);
		}
	}

	public bool UpdateGroups(Vector3 position, EntityNetworkRange range)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Debug.Assert(sv != null, "SV IS NULL");
		Debug.Assert(sv.visibility != null, "sv.visibility IS NULL");
		Group newGroup = sv.visibility.GetGroup(position, range);
		return SwitchGroup(newGroup);
	}

	public bool SwitchGroup(Group newGroup)
	{
		if (newGroup == group)
		{
			return false;
		}
		using (TimeWarning.New("SwitchGroup"))
		{
			if (group != null)
			{
				using (TimeWarning.New("group.Leave"))
				{
					group.Leave(this);
				}
			}
			Group oldGroup = group;
			group = newGroup;
			if (group != null)
			{
				using (TimeWarning.New("group.Join"))
				{
					group.Join(this);
				}
			}
			using (TimeWarning.New("OnSubscriptionChange"))
			{
				OnSubscriptionChange();
			}
			if (handler != null)
			{
				using (TimeWarning.New("OnNetworkGroupChange"))
				{
					handler.OnNetworkGroupChange(oldGroup);
				}
			}
			using (TimeWarning.New("OnGroupTransition"))
			{
				OnGroupTransition(oldGroup);
			}
		}
		return true;
	}

	public void OnGroupTransition(Group oldGroup)
	{
		if (oldGroup == null)
		{
			if (group != null && group.HasSubscribers() && handler != null)
			{
				handler.OnNetworkSubscribersEnter(group.subscribers);
			}
			return;
		}
		if (group == null)
		{
			if (oldGroup != null && handler != null)
			{
				handler.OnNetworkSubscribersLeave(oldGroup.subscribers);
			}
			return;
		}
		List<Connection> list = Pool.Get<List<Connection>>();
		List<Connection> list2 = Pool.Get<List<Connection>>();
		List.Compare<Connection>(oldGroup.subscribers, group.subscribers, list, list2, (List<Connection>)null);
		if (handler != null)
		{
			handler.OnNetworkSubscribersEnter(list);
		}
		if (handler != null)
		{
			handler.OnNetworkSubscribersLeave(list2);
		}
		Pool.FreeUnmanaged<Connection>(ref list);
		Pool.FreeUnmanaged<Connection>(ref list2);
	}

	public void OnSubscriptionChange()
	{
		if (subscriber == null)
		{
			return;
		}
		if (group != null && !subscriber.IsSubscribed(group))
		{
			subscriber.Subscribe(group);
			if (handler != null)
			{
				handler.OnNetworkGroupEnter(group);
			}
		}
		updateSubscriptions = true;
		UpdateHighPrioritySubscriptions();
	}

	public bool SwitchSecondaryGroup(Group newGroup)
	{
		if (newGroup == secondaryGroup)
		{
			return false;
		}
		using (TimeWarning.New("SwitchSecondaryGroup"))
		{
			secondaryGroup = newGroup;
			using (TimeWarning.New("OnSubscriptionChange"))
			{
				OnSubscriptionChange();
			}
		}
		return true;
	}

	public void SetUpdateSubscriptions(bool shouldUpdate)
	{
		updateSubscriptions = shouldUpdate;
	}

	public bool UpdateSubscriptions(int removeLimit, int addLimit)
	{
		if (!ShouldUpdateSubscriptions)
		{
			return false;
		}
		using (TimeWarning.New("UpdateSubscriptions"))
		{
			updateSubscriptions = false;
			List<Group> list = Pool.Get<List<Group>>();
			List<Group> list2 = Pool.Get<List<Group>>();
			ListHashSet<Group> val = Pool.Get<ListHashSet<Group>>();
			SubStrategy.GatherSubscriptions(this, val);
			ListHashSet<Group>.Compare(subscriber.subscribed, val, list, list2, (List<Group>)null);
			if (Interface.CallHook("OnNetworkSubscriptionsUpdate", this, list, list2) == null)
			{
				for (int i = 0; i < list2.Count; i++)
				{
					Group obj = list2[i];
					if (removeLimit > 0)
					{
						subscriber.Unsubscribe(obj);
						if (handler != null)
						{
							handler.OnNetworkGroupLeave(obj);
						}
						removeLimit -= obj.networkables?.Count ?? 0;
					}
					else
					{
						updateSubscriptions = true;
					}
				}
				for (int j = 0; j < list.Count; j++)
				{
					Group obj2 = list[j];
					if (addLimit > 0)
					{
						subscriber.Subscribe(obj2);
						if (handler != null)
						{
							handler.OnNetworkGroupEnter(obj2);
						}
						addLimit -= obj2.networkables?.Count ?? 0;
					}
					else
					{
						updateSubscriptions = true;
					}
				}
			}
			Pool.FreeUnmanaged<Group>(ref list);
			Pool.FreeUnmanaged<Group>(ref list2);
			Pool.FreeUnmanaged<Group>(ref val);
		}
		return true;
	}

	public bool UpdateHighPrioritySubscriptions()
	{
		if (subscriber == null)
		{
			return false;
		}
		using (TimeWarning.New("UpdateHighPrioritySubscriptions"))
		{
			List<Group> list = Pool.Get<List<Group>>();
			ListHashSet<Group> val = Pool.Get<ListHashSet<Group>>();
			SubStrategy.GatherHighPrioSubscriptions(this, val);
			ListHashSet<Group>.Compare(subscriber.subscribed, val, list, (List<Group>)null, (List<Group>)null);
			if (Interface.CallHook("OnNetworkSubscriptionsUpdate", this, list, null) == null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					Group obj = list[i];
					subscriber.Subscribe(obj);
					if (handler != null && obj.HasSubscribers())
					{
						handler.OnNetworkGroupEnter(obj);
					}
				}
			}
			Pool.FreeUnmanaged<Group>(ref list);
			Pool.FreeUnmanaged<Group>(ref val);
		}
		return true;
	}

	public void InvalidateSubscriptions(int maxNewCells)
	{
		if (subscriber == null)
		{
			return;
		}
		bool flag = false;
		using (TimeWarning.New("InvalidateSubscriptions"))
		{
			List<Group> list = Pool.Get<List<Group>>();
			List<Group> list2 = Pool.Get<List<Group>>();
			ListHashSet<Group> val = Pool.Get<ListHashSet<Group>>();
			SubStrategy.GatherHighPrioSubscriptions(this, val);
			using (TimeWarning.New("Compare"))
			{
				ListHashSet<Group>.Compare(subscriber.subscribed, val, list, list2, (List<Group>)null);
			}
			using (TimeWarning.New("Unsubscribe"))
			{
				for (int i = 0; i < list2.Count; i++)
				{
					Group obj = list2[i];
					subscriber.Unsubscribe(obj);
					if (handler != null)
					{
						handler.OnNetworkGroupLeave(obj);
					}
				}
			}
			using (TimeWarning.New("Subscribe"))
			{
				int num = Mathf.Min(list.Count, maxNewCells);
				for (int j = 0; j < num; j++)
				{
					Group obj2 = list[j];
					subscriber.Subscribe(obj2);
					if (handler != null)
					{
						handler.OnNetworkGroupEnter(obj2);
					}
				}
				flag = num == list.Count;
			}
			Pool.FreeUnmanaged<Group>(ref list);
			Pool.FreeUnmanaged<Group>(ref list2);
			Pool.FreeUnmanaged<Group>(ref val);
		}
		updateSubscriptions = !flag;
	}

	public static void UpdateSubscriptions(BufferList<Networkable> nets, NativeArray<int> removeLimits, NativeArray<int> addLimits)
	{
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("UpdateSubscriptions"))
		{
			updateSubsAsyncState.Networkables = nets;
			if (updateSubsAsyncState.Added.Capacity < nets.Count)
			{
				int capacity = updateSubsAsyncState.Added.Capacity;
				updateSubsAsyncState.Added.Resize(nets.Count);
				updateSubsAsyncState.Removed.Resize(nets.Count);
				updateSubsAsyncState.IsDone.Resize(nets.Count);
				for (int i = capacity; i < nets.Count; i++)
				{
					updateSubsAsyncState.Added[i] = new List<Group>();
					updateSubsAsyncState.Removed[i] = new List<Group>();
					updateSubsAsyncState.IsDone[i] = false;
				}
			}
			int processorCount = Environment.ProcessorCount;
			int num = nets.Count / processorCount / 2;
			num = Mathf.Max(num, 1);
			int num2 = (nets.Count + num - 1) / num;
			List<UniTask> list = Pool.Get<List<UniTask>>();
			for (int j = 1; j < num2; j++)
			{
				list.Add(ProcessBatchAsync(updateSubsAsyncState, j, num));
			}
			int num3 = Mathf.Min(num2, 1);
			for (int k = 0; k < num3; k++)
			{
				ProcessBatch(updateSubsAsyncState, 0, num);
			}
			WaitAndIntegrate(updateSubsAsyncState, list, removeLimits, addLimits);
			Pool.FreeUnmanaged<UniTask>(ref list);
		}
		static void IntegrateResults(UpdateSubs_AsyncState state, int index, NativeArray<int> val, NativeArray<int> val2)
		{
			Networkable networkable = state.Networkables[index];
			using (TimeWarning.New("Unsubscribe"))
			{
				List<Group> list2 = state.Removed[index];
				int num4 = val[index];
				foreach (Group item in list2)
				{
					if (num4 <= 0)
					{
						num4 = int.MinValue;
						break;
					}
					networkable.subscriber.Unsubscribe(item);
					if (networkable.handler != null)
					{
						networkable.handler.OnNetworkGroupLeave(item);
					}
					num4 -= item.networkables?.Count ?? 0;
				}
				list2.Clear();
				val[index] = num4;
			}
			using (TimeWarning.New("Subscribe"))
			{
				List<Group> list3 = state.Added[index];
				int num5 = val2[index];
				foreach (Group item2 in list3)
				{
					if (num5 <= 0)
					{
						num5 = int.MinValue;
						break;
					}
					networkable.subscriber.Subscribe(item2);
					if (networkable.handler != null)
					{
						networkable.handler.OnNetworkGroupEnter(item2);
					}
					num5 -= item2.networkables?.Count ?? 0;
				}
				list3.Clear();
				val2[index] = num5;
			}
			state.IsDone[index] = false;
		}
		static void ProcessBatch(UpdateSubs_AsyncState state, int batchIndex, int batchSize)
		{
			int num4 = batchIndex * batchSize;
			int num5 = Mathf.Min(batchSize, state.Networkables.Count - num4);
			ListHashSet<Group> val = Pool.Get<ListHashSet<Group>>();
			for (int l = 0; l < num5; l++)
			{
				int num6 = num4 + l;
				Networkable networkable = state.Networkables[num6];
				val.Clear();
				networkable.SubStrategy.GatherSubscriptions(networkable, val);
				using (TimeWarning.New("Compare"))
				{
					List<Group> list2 = state.Added[num6];
					List<Group> list3 = state.Removed[num6];
					ListHashSet<Group>.Compare(networkable.subscriber.subscribed, val, list2, list3, (List<Group>)null);
				}
				state.IsDone[num6] = true;
			}
			Pool.FreeUnmanaged<Group>(ref val);
		}
		[AsyncStateMachine(typeof(_003C_003CUpdateSubscriptions_003Eg__ProcessBatchAsync_007C36_0_003Ed))]
		static UniTask ProcessBatchAsync(UpdateSubs_AsyncState state, int batchIndex, int batchSize)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			_003C_003CUpdateSubscriptions_003Eg__ProcessBatchAsync_007C36_0_003Ed _003C_003CUpdateSubscriptions_003Eg__ProcessBatchAsync_007C36_0_003Ed2 = default(_003C_003CUpdateSubscriptions_003Eg__ProcessBatchAsync_007C36_0_003Ed);
			_003C_003CUpdateSubscriptions_003Eg__ProcessBatchAsync_007C36_0_003Ed2._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
			_003C_003CUpdateSubscriptions_003Eg__ProcessBatchAsync_007C36_0_003Ed2.state = state;
			_003C_003CUpdateSubscriptions_003Eg__ProcessBatchAsync_007C36_0_003Ed2.batchIndex = batchIndex;
			_003C_003CUpdateSubscriptions_003Eg__ProcessBatchAsync_007C36_0_003Ed2.batchSize = batchSize;
			_003C_003CUpdateSubscriptions_003Eg__ProcessBatchAsync_007C36_0_003Ed2._003C_003E1__state = -1;
			((AsyncUniTaskMethodBuilder)(ref _003C_003CUpdateSubscriptions_003Eg__ProcessBatchAsync_007C36_0_003Ed2._003C_003Et__builder)).Start<_003C_003CUpdateSubscriptions_003Eg__ProcessBatchAsync_007C36_0_003Ed>(ref _003C_003CUpdateSubscriptions_003Eg__ProcessBatchAsync_007C36_0_003Ed2);
			return ((AsyncUniTaskMethodBuilder)(ref _003C_003CUpdateSubscriptions_003Eg__ProcessBatchAsync_007C36_0_003Ed2._003C_003Et__builder)).Task;
		}
		static void WaitAndIntegrate(UpdateSubs_AsyncState state, List<UniTask> tasks, NativeArray<int> removeLimits2, NativeArray<int> addLimits2)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("WaitAndIntegrate"))
			{
				bool flag;
				do
				{
					flag = false;
					foreach (UniTask task in tasks)
					{
						UniTask current = task;
						flag |= !UniTaskStatusExtensions.IsCompleted(((UniTask)(ref current)).Status);
					}
					for (int l = 0; l < state.Networkables.Count; l++)
					{
						if (state.IsDone.Buffer[l])
						{
							IntegrateResults(state, l, removeLimits2, addLimits2);
						}
					}
				}
				while (flag);
				foreach (UniTask task2 in tasks)
				{
					UniTask current2 = task2;
					Awaiter awaiter = ((UniTask)(ref current2)).GetAwaiter();
					((Awaiter)(ref awaiter)).GetResult();
				}
			}
		}
	}

	[CompilerGenerated]
	internal static void _003CUpdateSubscriptions_003Eg__ProcessBatch_007C36_1(UpdateSubs_AsyncState state, int batchIndex, int batchSize)
	{
		int num = batchIndex * batchSize;
		int num2 = Mathf.Min(batchSize, state.Networkables.Count - num);
		ListHashSet<Group> val = Pool.Get<ListHashSet<Group>>();
		for (int i = 0; i < num2; i++)
		{
			int num3 = num + i;
			Networkable networkable = state.Networkables[num3];
			val.Clear();
			networkable.SubStrategy.GatherSubscriptions(networkable, val);
			using (TimeWarning.New("Compare"))
			{
				List<Group> list = state.Added[num3];
				List<Group> list2 = state.Removed[num3];
				ListHashSet<Group>.Compare(networkable.subscriber.subscribed, val, list, list2, (List<Group>)null);
			}
			state.IsDone[num3] = true;
		}
		Pool.FreeUnmanaged<Group>(ref val);
	}
}

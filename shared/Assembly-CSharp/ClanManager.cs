using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ConVar;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Facepunch;
using JetBrains.Annotations;
using Network;
using ProtoBuf;
using Rust;
using Rust.Assertions;
using UnityEngine;
using UnityEngine.Assertions;

public class ClanManager : BaseEntity
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_AcceptInvitation_003Ed__14 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private ValueTaskAwaiter<ClanValueResult<IClan>> _003C_003Eu__1;

		private ValueTaskAwaiter<ClanResult> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<IClan>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<IClan>>);
					num = (_003C_003E1__state = -1);
					goto IL_00f6;
				}
				ValueTaskAwaiter<ClanResult> valueTaskAwaiter2;
				if (num == 1)
				{
					valueTaskAwaiter2 = _003C_003Eu__2;
					_003C_003Eu__2 = default(ValueTaskAwaiter<ClanResult>);
					num = (_003C_003E1__state = -1);
					goto IL_018d;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					long num2 = msg.read.Int64();
					if (msg.player.CanModifyClan())
					{
						valueTaskAwaiter = clanManager.Backend.Get(num2).GetAwaiter();
						if (!valueTaskAwaiter.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = valueTaskAwaiter;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<IClan>>, _003CServer_AcceptInvitation_003Ed__14>(ref valueTaskAwaiter, ref this);
							return;
						}
						goto IL_00f6;
					}
					clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, (ClanResult)20));
				}
				goto end_IL_000e;
				IL_018d:
				ClanResult result = valueTaskAwaiter2.GetResult();
				clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, result));
				goto end_IL_000e;
				IL_00f6:
				ClanValueResult<IClan> result2 = valueTaskAwaiter.GetResult();
				if (clanManager.CheckClanResult(_003CrequestId_003E5__2, msg.player, result2, out var clan))
				{
					valueTaskAwaiter2 = clan.AcceptInvite((ulong)msg.player.userID).GetAwaiter();
					if (!valueTaskAwaiter2.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = valueTaskAwaiter2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanResult>, _003CServer_AcceptInvitation_003Ed__14>(ref valueTaskAwaiter2, ref this);
						return;
					}
					goto IL_018d;
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_CancelInvitation_003Ed__15 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private ValueTaskAwaiter<ClanValueResult<IClan>> _003C_003Eu__1;

		private ValueTaskAwaiter<ClanResult> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<IClan>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<IClan>>);
					num = (_003C_003E1__state = -1);
					goto IL_00f6;
				}
				ValueTaskAwaiter<ClanResult> valueTaskAwaiter2;
				if (num == 1)
				{
					valueTaskAwaiter2 = _003C_003Eu__2;
					_003C_003Eu__2 = default(ValueTaskAwaiter<ClanResult>);
					num = (_003C_003E1__state = -1);
					goto IL_01a2;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					long num2 = msg.read.Int64();
					if (msg.player.CanModifyClan())
					{
						valueTaskAwaiter = clanManager.Backend.Get(num2).GetAwaiter();
						if (!valueTaskAwaiter.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = valueTaskAwaiter;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<IClan>>, _003CServer_CancelInvitation_003Ed__15>(ref valueTaskAwaiter, ref this);
							return;
						}
						goto IL_00f6;
					}
					clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, (ClanResult)20));
				}
				goto end_IL_000e;
				IL_01a2:
				ClanResult result = valueTaskAwaiter2.GetResult();
				clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, result));
				goto end_IL_000e;
				IL_00f6:
				ClanValueResult<IClan> result2 = valueTaskAwaiter.GetResult();
				if (clanManager.CheckClanResult(_003CrequestId_003E5__2, msg.player, result2, out var clan))
				{
					valueTaskAwaiter2 = clan.CancelInvite((ulong)msg.player.userID, (ulong)msg.player.userID).GetAwaiter();
					if (!valueTaskAwaiter2.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = valueTaskAwaiter2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanResult>, _003CServer_CancelInvitation_003Ed__15>(ref valueTaskAwaiter2, ref this);
						return;
					}
					goto IL_01a2;
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_CancelInvite_003Ed__13 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private ulong _003CsteamId_003E5__3;

		private IClan _003Cclan_003E5__4;

		private ValueTaskAwaiter<ClanValueResult<IClan>> _003C_003Eu__1;

		private ValueTaskAwaiter<ClanResult> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<IClan>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<IClan>>);
					num = (_003C_003E1__state = -1);
					goto IL_010a;
				}
				ValueTaskAwaiter<ClanResult> valueTaskAwaiter2;
				if (num == 1)
				{
					valueTaskAwaiter2 = _003C_003Eu__2;
					_003C_003Eu__2 = default(ValueTaskAwaiter<ClanResult>);
					num = (_003C_003E1__state = -1);
					goto IL_01b2;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					_003CsteamId_003E5__3 = msg.read.UInt64();
					if (msg.player.CanModifyClan())
					{
						valueTaskAwaiter = clanManager.Backend.Get(msg.player.clanId).GetAwaiter();
						if (!valueTaskAwaiter.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = valueTaskAwaiter;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<IClan>>, _003CServer_CancelInvite_003Ed__13>(ref valueTaskAwaiter, ref this);
							return;
						}
						goto IL_010a;
					}
					clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, (ClanResult)20));
				}
				goto end_IL_000e;
				IL_01b2:
				ClanResult result = valueTaskAwaiter2.GetResult();
				clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, result, _003Cclan_003E5__4));
				goto end_IL_000e;
				IL_010a:
				ClanValueResult<IClan> result2 = valueTaskAwaiter.GetResult();
				if (clanManager.CheckClanResult(_003CrequestId_003E5__2, msg.player, result2, out _003Cclan_003E5__4))
				{
					valueTaskAwaiter2 = _003Cclan_003E5__4.CancelInvite(_003CsteamId_003E5__3, (ulong)msg.player.userID).GetAwaiter();
					if (!valueTaskAwaiter2.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = valueTaskAwaiter2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanResult>, _003CServer_CancelInvite_003Ed__13>(ref valueTaskAwaiter2, ref this);
						return;
					}
					goto IL_01b2;
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cclan_003E5__4 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cclan_003E5__4 = null;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_CreateClan_003Ed__1 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private ValueTaskAwaiter<ClanValueResult<IClan>> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<IClan>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<IClan>>);
					num = (_003C_003E1__state = -1);
					goto IL_0116;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					ClanValidatorResult val = ClanValidator.ValidateClanName(msg.read.String());
					if (((ClanValidatorResult)(ref val)).Success)
					{
						valueTaskAwaiter = clanManager.Backend.Create((ulong)msg.player.userID, ((ClanValidatorResult)(ref val)).Value).GetAwaiter();
						if (!valueTaskAwaiter.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = valueTaskAwaiter;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<IClan>>, _003CServer_CreateClan_003Ed__1>(ref valueTaskAwaiter, ref this);
							return;
						}
						goto IL_0116;
					}
					clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, ClanValidator.ToClanResult(((ClanValidatorResult)(ref val)).Error)));
				}
				goto end_IL_000e;
				IL_0116:
				ClanValueResult<IClan> result = valueTaskAwaiter.GetResult();
				if (clanManager.CheckClanResult(_003CrequestId_003E5__2, msg.player, result, out var clan))
				{
					clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, (ClanResult)1, clan));
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_CreateRole_003Ed__19 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private ClanRole _003Crole_003E5__3;

		private IClan _003Cclan_003E5__4;

		private ValueTaskAwaiter<ClanValueResult<IClan>> _003C_003Eu__1;

		private ValueTaskAwaiter<ClanResult> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0264: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_021c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0221: Unknown result type (might be due to invalid IL or missing references)
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_028e: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<IClan>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<IClan>>);
					num = (_003C_003E1__state = -1);
					goto IL_0170;
				}
				ValueTaskAwaiter<ClanResult> valueTaskAwaiter2;
				if (num == 1)
				{
					valueTaskAwaiter2 = _003C_003Eu__2;
					_003C_003Eu__2 = default(ValueTaskAwaiter<ClanResult>);
					num = (_003C_003E1__state = -1);
					goto IL_021a;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					string text = msg.read.String(128);
					if (!msg.player.CanModifyClan())
					{
						clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, (ClanResult)20));
					}
					else
					{
						ClanValidatorResult val = ClanValidator.ValidateRoleName(text);
						if (((ClanValidatorResult)(ref val)).Success)
						{
							_003Crole_003E5__3 = new ClanRole
							{
								Name = ((ClanValidatorResult)(ref val)).Value
							};
							valueTaskAwaiter = clanManager.Backend.Get(msg.player.clanId).GetAwaiter();
							if (!valueTaskAwaiter.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = valueTaskAwaiter;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<IClan>>, _003CServer_CreateRole_003Ed__19>(ref valueTaskAwaiter, ref this);
								return;
							}
							goto IL_0170;
						}
						clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, ClanValidator.ToClanResult(((ClanValidatorResult)(ref val)).Error)));
					}
				}
				goto end_IL_000e;
				IL_021a:
				ClanResult result = valueTaskAwaiter2.GetResult();
				clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, result, _003Cclan_003E5__4));
				goto end_IL_000e;
				IL_0170:
				ClanValueResult<IClan> result2 = valueTaskAwaiter.GetResult();
				if (clanManager.CheckClanResult(_003CrequestId_003E5__2, msg.player, result2, out _003Cclan_003E5__4))
				{
					valueTaskAwaiter2 = _003Cclan_003E5__4.CreateRole(_003Crole_003E5__3, (ulong)msg.player.userID).GetAwaiter();
					if (!valueTaskAwaiter2.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = valueTaskAwaiter2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanResult>, _003CServer_CreateRole_003Ed__19>(ref valueTaskAwaiter2, ref this);
						return;
					}
					goto IL_021a;
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Crole_003E5__3 = default(ClanRole);
				_003Cclan_003E5__4 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Crole_003E5__3 = default(ClanRole);
			_003Cclan_003E5__4 = null;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_DeleteRole_003Ed__21 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private int _003CroleId_003E5__3;

		private IClan _003Cclan_003E5__4;

		private ValueTaskAwaiter<ClanValueResult<IClan>> _003C_003Eu__1;

		private ValueTaskAwaiter<ClanResult> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<IClan>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<IClan>>);
					num = (_003C_003E1__state = -1);
					goto IL_010a;
				}
				ValueTaskAwaiter<ClanResult> valueTaskAwaiter2;
				if (num == 1)
				{
					valueTaskAwaiter2 = _003C_003Eu__2;
					_003C_003Eu__2 = default(ValueTaskAwaiter<ClanResult>);
					num = (_003C_003E1__state = -1);
					goto IL_01b2;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					_003CroleId_003E5__3 = msg.read.Int32();
					if (msg.player.CanModifyClan())
					{
						valueTaskAwaiter = clanManager.Backend.Get(msg.player.clanId).GetAwaiter();
						if (!valueTaskAwaiter.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = valueTaskAwaiter;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<IClan>>, _003CServer_DeleteRole_003Ed__21>(ref valueTaskAwaiter, ref this);
							return;
						}
						goto IL_010a;
					}
					clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, (ClanResult)20));
				}
				goto end_IL_000e;
				IL_01b2:
				ClanResult result = valueTaskAwaiter2.GetResult();
				clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, result, _003Cclan_003E5__4));
				goto end_IL_000e;
				IL_010a:
				ClanValueResult<IClan> result2 = valueTaskAwaiter.GetResult();
				if (clanManager.CheckClanResult(_003CrequestId_003E5__2, msg.player, result2, out _003Cclan_003E5__4))
				{
					valueTaskAwaiter2 = _003Cclan_003E5__4.DeleteRole(_003CroleId_003E5__3, (ulong)msg.player.userID).GetAwaiter();
					if (!valueTaskAwaiter2.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = valueTaskAwaiter2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanResult>, _003CServer_DeleteRole_003Ed__21>(ref valueTaskAwaiter2, ref this);
						return;
					}
					goto IL_01b2;
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cclan_003E5__4 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cclan_003E5__4 = null;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_GetClan_003Ed__2 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private IClan _003Cclan_003E5__3;

		private ValueTaskAwaiter<ClanValueResult<IClan>> _003C_003Eu__1;

		private ValueTaskAwaiter _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<IClan>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<IClan>>);
					num = (_003C_003E1__state = -1);
					goto IL_00b2;
				}
				ValueTaskAwaiter valueTaskAwaiter2;
				if (num == 1)
				{
					valueTaskAwaiter2 = _003C_003Eu__2;
					_003C_003Eu__2 = default(ValueTaskAwaiter);
					num = (_003C_003E1__state = -1);
					goto IL_013f;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					valueTaskAwaiter = clanManager.Backend.Get(msg.player.clanId).GetAwaiter();
					if (!valueTaskAwaiter.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = valueTaskAwaiter;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<IClan>>, _003CServer_GetClan_003Ed__2>(ref valueTaskAwaiter, ref this);
						return;
					}
					goto IL_00b2;
				}
				goto end_IL_000e;
				IL_00b2:
				ClanValueResult<IClan> result = valueTaskAwaiter.GetResult();
				if (clanManager.CheckClanResult(_003CrequestId_003E5__2, msg.player, result, out _003Cclan_003E5__3))
				{
					valueTaskAwaiter2 = _003Cclan_003E5__3.RefreshIfStale().GetAwaiter();
					if (!valueTaskAwaiter2.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = valueTaskAwaiter2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter, _003CServer_GetClan_003Ed__2>(ref valueTaskAwaiter2, ref this);
						return;
					}
					goto IL_013f;
				}
				goto end_IL_000e;
				IL_013f:
				valueTaskAwaiter2.GetResult();
				clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, (ClanResult)1, _003Cclan_003E5__3, includeLogo: true));
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cclan_003E5__3 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cclan_003E5__3 = null;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_GetInvitations_003Ed__5 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private ValueTaskAwaiter<ClanValueResult<List<ClanInvitation>>> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<List<ClanInvitation>>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<List<ClanInvitation>>>);
					num = (_003C_003E1__state = -1);
					goto IL_00b0;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					valueTaskAwaiter = clanManager.Backend.ListInvitations((ulong)msg.player.userID).GetAwaiter();
					if (!valueTaskAwaiter.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = valueTaskAwaiter;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<List<ClanInvitation>>>, _003CServer_GetInvitations_003Ed__5>(ref valueTaskAwaiter, ref this);
						return;
					}
					goto IL_00b0;
				}
				goto end_IL_000e;
				IL_00b0:
				ClanValueResult<List<ClanInvitation>> result = valueTaskAwaiter.GetResult();
				if (result.IsSuccess)
				{
					clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveClanInvitations", msg.player), ClanInvitationExtensions.ToProto(result.Value));
				}
				clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, result.Result));
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_GetLeaderboard_003Ed__8 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private ValueTaskAwaiter<ClanValueResult<List<ClanLeaderboardEntry>>> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<List<ClanLeaderboardEntry>>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<List<ClanLeaderboardEntry>>>);
					num = (_003C_003E1__state = -1);
					goto IL_00bd;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					if (clanManager._leaderboardCache == null || RealTimeSince.op_Implicit(clanManager._sinceLastLeaderboardUpdate) > 30f)
					{
						valueTaskAwaiter = clanManager.Backend.GetLeaderboard(100).GetAwaiter();
						if (!valueTaskAwaiter.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = valueTaskAwaiter;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<List<ClanLeaderboardEntry>>>, _003CServer_GetLeaderboard_003Ed__8>(ref valueTaskAwaiter, ref this);
							return;
						}
						goto IL_00bd;
					}
					goto IL_00f4;
				}
				goto end_IL_000e;
				IL_00bd:
				ClanValueResult<List<ClanLeaderboardEntry>> result = valueTaskAwaiter.GetResult();
				if (result.IsSuccess)
				{
					clanManager._leaderboardCache = result.Value;
					clanManager._sinceLastLeaderboardUpdate = RealTimeSince.op_Implicit(0f);
				}
				else
				{
					clanManager._leaderboardCache = null;
				}
				goto IL_00f4;
				IL_00f4:
				if (clanManager._leaderboardCache != null)
				{
					clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveClanLeaderboard", msg.player), ClanLeaderboardExtensions.ToProto(clanManager._leaderboardCache));
				}
				ClanResult result2 = (ClanResult)(clanManager._leaderboardCache != null);
				clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, result2));
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_GetLogs_003Ed__3 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private IClan _003Cclan_003E5__3;

		private ValueTaskAwaiter<ClanValueResult<IClan>> _003C_003Eu__1;

		private ValueTaskAwaiter<ClanValueResult<ClanLogs>> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<IClan>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<IClan>>);
					num = (_003C_003E1__state = -1);
					goto IL_00b5;
				}
				ValueTaskAwaiter<ClanValueResult<ClanLogs>> valueTaskAwaiter2;
				if (num == 1)
				{
					valueTaskAwaiter2 = _003C_003Eu__2;
					_003C_003Eu__2 = default(ValueTaskAwaiter<ClanValueResult<ClanLogs>>);
					num = (_003C_003E1__state = -1);
					goto IL_0159;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					valueTaskAwaiter = clanManager.Backend.Get(msg.player.clanId).GetAwaiter();
					if (!valueTaskAwaiter.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = valueTaskAwaiter;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<IClan>>, _003CServer_GetLogs_003Ed__3>(ref valueTaskAwaiter, ref this);
						return;
					}
					goto IL_00b5;
				}
				goto end_IL_000e;
				IL_00b5:
				ClanValueResult<IClan> result = valueTaskAwaiter.GetResult();
				if (clanManager.CheckClanResult(_003CrequestId_003E5__2, msg.player, result, out _003Cclan_003E5__3))
				{
					valueTaskAwaiter2 = _003Cclan_003E5__3.GetLogs(100, (ulong)msg.player.userID).GetAwaiter();
					if (!valueTaskAwaiter2.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = valueTaskAwaiter2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<ClanLogs>>, _003CServer_GetLogs_003Ed__3>(ref valueTaskAwaiter2, ref this);
						return;
					}
					goto IL_0159;
				}
				goto end_IL_000e;
				IL_0159:
				ClanValueResult<ClanLogs> result2 = valueTaskAwaiter2.GetResult();
				if (result2.IsSuccess)
				{
					clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveClanLogs", msg.player), ClanLogExtensions.ToProto(result2.Value));
				}
				clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, result2.Result, _003Cclan_003E5__3));
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cclan_003E5__3 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cclan_003E5__3 = null;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_GetScoreEvents_003Ed__4 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private IClan _003Cclan_003E5__3;

		private ValueTaskAwaiter<ClanValueResult<IClan>> _003C_003Eu__1;

		private ValueTaskAwaiter<ClanValueResult<ClanScoreEvents>> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<IClan>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<IClan>>);
					num = (_003C_003E1__state = -1);
					goto IL_00b5;
				}
				ValueTaskAwaiter<ClanValueResult<ClanScoreEvents>> valueTaskAwaiter2;
				if (num == 1)
				{
					valueTaskAwaiter2 = _003C_003Eu__2;
					_003C_003Eu__2 = default(ValueTaskAwaiter<ClanValueResult<ClanScoreEvents>>);
					num = (_003C_003E1__state = -1);
					goto IL_0159;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					valueTaskAwaiter = clanManager.Backend.Get(msg.player.clanId).GetAwaiter();
					if (!valueTaskAwaiter.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = valueTaskAwaiter;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<IClan>>, _003CServer_GetScoreEvents_003Ed__4>(ref valueTaskAwaiter, ref this);
						return;
					}
					goto IL_00b5;
				}
				goto end_IL_000e;
				IL_00b5:
				ClanValueResult<IClan> result = valueTaskAwaiter.GetResult();
				if (clanManager.CheckClanResult(_003CrequestId_003E5__2, msg.player, result, out _003Cclan_003E5__3))
				{
					valueTaskAwaiter2 = _003Cclan_003E5__3.GetScoreEvents(100, (ulong)msg.player.userID).GetAwaiter();
					if (!valueTaskAwaiter2.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = valueTaskAwaiter2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<ClanScoreEvents>>, _003CServer_GetScoreEvents_003Ed__4>(ref valueTaskAwaiter2, ref this);
						return;
					}
					goto IL_0159;
				}
				goto end_IL_000e;
				IL_0159:
				ClanValueResult<ClanScoreEvents> result2 = valueTaskAwaiter2.GetResult();
				if (result2.IsSuccess)
				{
					clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveClanScoreEvents", msg.player), ClanLogExtensions.ToProto(result2.Value));
				}
				clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, result2.Result, _003Cclan_003E5__3));
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cclan_003E5__3 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cclan_003E5__3 = null;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_Invite_003Ed__12 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private ulong _003CsteamId_003E5__3;

		private IClan _003Cclan_003E5__4;

		private ValueTaskAwaiter<ClanValueResult<IClan>> _003C_003Eu__1;

		private ValueTaskAwaiter<ClanResult> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<IClan>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<IClan>>);
					num = (_003C_003E1__state = -1);
					goto IL_010a;
				}
				ValueTaskAwaiter<ClanResult> valueTaskAwaiter2;
				if (num == 1)
				{
					valueTaskAwaiter2 = _003C_003Eu__2;
					_003C_003Eu__2 = default(ValueTaskAwaiter<ClanResult>);
					num = (_003C_003E1__state = -1);
					goto IL_01b2;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					_003CsteamId_003E5__3 = msg.read.UInt64();
					if (msg.player.CanModifyClan())
					{
						valueTaskAwaiter = clanManager.Backend.Get(msg.player.clanId).GetAwaiter();
						if (!valueTaskAwaiter.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = valueTaskAwaiter;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<IClan>>, _003CServer_Invite_003Ed__12>(ref valueTaskAwaiter, ref this);
							return;
						}
						goto IL_010a;
					}
					clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, (ClanResult)20));
				}
				goto end_IL_000e;
				IL_01b2:
				ClanResult result = valueTaskAwaiter2.GetResult();
				clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, result, _003Cclan_003E5__4));
				goto end_IL_000e;
				IL_010a:
				ClanValueResult<IClan> result2 = valueTaskAwaiter.GetResult();
				if (clanManager.CheckClanResult(_003CrequestId_003E5__2, msg.player, result2, out _003Cclan_003E5__4))
				{
					valueTaskAwaiter2 = _003Cclan_003E5__4.Invite(_003CsteamId_003E5__3, (ulong)msg.player.userID).GetAwaiter();
					if (!valueTaskAwaiter2.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = valueTaskAwaiter2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanResult>, _003CServer_Invite_003Ed__12>(ref valueTaskAwaiter2, ref this);
						return;
					}
					goto IL_01b2;
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cclan_003E5__4 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cclan_003E5__4 = null;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_Kick_003Ed__16 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private ulong _003CsteamId_003E5__3;

		private IClan _003Cclan_003E5__4;

		private ValueTaskAwaiter<ClanValueResult<IClan>> _003C_003Eu__1;

		private ValueTaskAwaiter<ClanResult> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<IClan>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<IClan>>);
					num = (_003C_003E1__state = -1);
					goto IL_0127;
				}
				ValueTaskAwaiter<ClanResult> valueTaskAwaiter2;
				if (num == 1)
				{
					valueTaskAwaiter2 = _003C_003Eu__2;
					_003C_003Eu__2 = default(ValueTaskAwaiter<ClanResult>);
					num = (_003C_003E1__state = -1);
					goto IL_01cf;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					_003CsteamId_003E5__3 = msg.read.UInt64();
					if ((ulong)msg.player.userID == _003CsteamId_003E5__3 || msg.player.CanModifyClan())
					{
						valueTaskAwaiter = clanManager.Backend.Get(msg.player.clanId).GetAwaiter();
						if (!valueTaskAwaiter.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = valueTaskAwaiter;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<IClan>>, _003CServer_Kick_003Ed__16>(ref valueTaskAwaiter, ref this);
							return;
						}
						goto IL_0127;
					}
					clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, (ClanResult)20));
				}
				goto end_IL_000e;
				IL_01cf:
				ClanResult result = valueTaskAwaiter2.GetResult();
				clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, result, _003Cclan_003E5__4));
				goto end_IL_000e;
				IL_0127:
				ClanValueResult<IClan> result2 = valueTaskAwaiter.GetResult();
				if (clanManager.CheckClanResult(_003CrequestId_003E5__2, msg.player, result2, out _003Cclan_003E5__4))
				{
					valueTaskAwaiter2 = _003Cclan_003E5__4.Kick(_003CsteamId_003E5__3, (ulong)msg.player.userID).GetAwaiter();
					if (!valueTaskAwaiter2.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = valueTaskAwaiter2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanResult>, _003CServer_Kick_003Ed__16>(ref valueTaskAwaiter2, ref this);
						return;
					}
					goto IL_01cf;
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cclan_003E5__4 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cclan_003E5__4 = null;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_SetColor_003Ed__10 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private Color32 _003CnewColor_003E5__3;

		private IClan _003Cclan_003E5__4;

		private ValueTaskAwaiter<ClanValueResult<IClan>> _003C_003Eu__1;

		private ValueTaskAwaiter<ClanResult> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<IClan>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<IClan>>);
					num = (_003C_003E1__state = -1);
					goto IL_0149;
				}
				ValueTaskAwaiter<ClanResult> valueTaskAwaiter2;
				if (num == 1)
				{
					valueTaskAwaiter2 = _003C_003Eu__2;
					_003C_003Eu__2 = default(ValueTaskAwaiter<ClanResult>);
					num = (_003C_003E1__state = -1);
					goto IL_01f1;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					_003CnewColor_003E5__3 = msg.read.Color32();
					if (!msg.player.CanModifyClan())
					{
						clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, (ClanResult)20));
					}
					else
					{
						if (_003CnewColor_003E5__3.a == byte.MaxValue)
						{
							valueTaskAwaiter = clanManager.Backend.Get(msg.player.clanId).GetAwaiter();
							if (!valueTaskAwaiter.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = valueTaskAwaiter;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<IClan>>, _003CServer_SetColor_003Ed__10>(ref valueTaskAwaiter, ref this);
								return;
							}
							goto IL_0149;
						}
						clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, (ClanResult)12));
					}
				}
				goto end_IL_000e;
				IL_01f1:
				ClanResult result = valueTaskAwaiter2.GetResult();
				clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, result, _003Cclan_003E5__4));
				goto end_IL_000e;
				IL_0149:
				ClanValueResult<IClan> result2 = valueTaskAwaiter.GetResult();
				if (clanManager.CheckClanResult(_003CrequestId_003E5__2, msg.player, result2, out _003Cclan_003E5__4))
				{
					valueTaskAwaiter2 = _003Cclan_003E5__4.SetColor(_003CnewColor_003E5__3, (ulong)msg.player.userID).GetAwaiter();
					if (!valueTaskAwaiter2.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = valueTaskAwaiter2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanResult>, _003CServer_SetColor_003Ed__10>(ref valueTaskAwaiter2, ref this);
						return;
					}
					goto IL_01f1;
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cclan_003E5__4 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cclan_003E5__4 = null;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_SetLogo_003Ed__9 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private byte[] _003CnewLogo_003E5__3;

		private IClan _003Cclan_003E5__4;

		private ValueTaskAwaiter<ClanValueResult<IClan>> _003C_003Eu__1;

		private ValueTaskAwaiter<ClanResult> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0203: Unknown result type (might be due to invalid IL or missing references)
			//IL_0220: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<IClan>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<IClan>>);
					num = (_003C_003E1__state = -1);
					goto IL_0154;
				}
				ValueTaskAwaiter<ClanResult> valueTaskAwaiter2;
				if (num == 1)
				{
					valueTaskAwaiter2 = _003C_003Eu__2;
					_003C_003Eu__2 = default(ValueTaskAwaiter<ClanResult>);
					num = (_003C_003E1__state = -1);
					goto IL_01fc;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					_003CnewLogo_003E5__3 = msg.read.BytesWithSize();
					if (!msg.player.CanModifyClan())
					{
						clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, (ClanResult)20));
					}
					else
					{
						if (ImageProcessing.IsValidPNG(_003CnewLogo_003E5__3, 512, 512))
						{
							valueTaskAwaiter = clanManager.Backend.Get(msg.player.clanId).GetAwaiter();
							if (!valueTaskAwaiter.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = valueTaskAwaiter;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<IClan>>, _003CServer_SetLogo_003Ed__9>(ref valueTaskAwaiter, ref this);
								return;
							}
							goto IL_0154;
						}
						clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, (ClanResult)11));
					}
				}
				goto end_IL_000e;
				IL_01fc:
				ClanResult result = valueTaskAwaiter2.GetResult();
				clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, result, _003Cclan_003E5__4, includeLogo: true));
				goto end_IL_000e;
				IL_0154:
				ClanValueResult<IClan> result2 = valueTaskAwaiter.GetResult();
				if (clanManager.CheckClanResult(_003CrequestId_003E5__2, msg.player, result2, out _003Cclan_003E5__4))
				{
					valueTaskAwaiter2 = _003Cclan_003E5__4.SetLogo(_003CnewLogo_003E5__3, (ulong)msg.player.userID).GetAwaiter();
					if (!valueTaskAwaiter2.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = valueTaskAwaiter2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanResult>, _003CServer_SetLogo_003Ed__9>(ref valueTaskAwaiter2, ref this);
						return;
					}
					goto IL_01fc;
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CnewLogo_003E5__3 = null;
				_003Cclan_003E5__4 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CnewLogo_003E5__3 = null;
			_003Cclan_003E5__4 = null;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_SetMotd_003Ed__11 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private ClanValidatorResult _003CvalidatedMotd_003E5__3;

		private IClan _003Cclan_003E5__4;

		private long _003CpreviousTimestamp_003E5__5;

		private ValueTaskAwaiter<ClanValueResult<IClan>> _003C_003Eu__1;

		private ValueTaskAwaiter<ClanResult> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0292: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_021f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0224: Unknown result type (might be due to invalid IL or missing references)
			//IL_0242: Unknown result type (might be due to invalid IL or missing references)
			//IL_0255: Unknown result type (might be due to invalid IL or missing references)
			//IL_0258: Invalid comparison between Unknown and I4
			//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<IClan>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<IClan>>);
					num = (_003C_003E1__state = -1);
					goto IL_015f;
				}
				ValueTaskAwaiter<ClanResult> valueTaskAwaiter2;
				if (num == 1)
				{
					valueTaskAwaiter2 = _003C_003Eu__2;
					_003C_003Eu__2 = default(ValueTaskAwaiter<ClanResult>);
					num = (_003C_003E1__state = -1);
					goto IL_021d;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					string text = msg.read.StringMultiLine(4096);
					if (!msg.player.CanModifyClan())
					{
						clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, (ClanResult)20));
					}
					else
					{
						_003CvalidatedMotd_003E5__3 = ClanValidator.ValidateMotd(text);
						if (((ClanValidatorResult)(ref _003CvalidatedMotd_003E5__3)).Success)
						{
							valueTaskAwaiter = clanManager.Backend.Get(msg.player.clanId).GetAwaiter();
							if (!valueTaskAwaiter.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = valueTaskAwaiter;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<IClan>>, _003CServer_SetMotd_003Ed__11>(ref valueTaskAwaiter, ref this);
								return;
							}
							goto IL_015f;
						}
						clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, ClanValidator.ToClanResult(((ClanValidatorResult)(ref _003CvalidatedMotd_003E5__3)).Error)));
					}
				}
				goto end_IL_000e;
				IL_021d:
				ClanResult result = valueTaskAwaiter2.GetResult();
				clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, result, _003Cclan_003E5__4));
				if ((int)result == 1)
				{
					ClanPushNotifications.SendClanAnnouncement(_003Cclan_003E5__4, _003CpreviousTimestamp_003E5__5, msg.player.userID);
				}
				goto end_IL_000e;
				IL_015f:
				ClanValueResult<IClan> result2 = valueTaskAwaiter.GetResult();
				if (clanManager.CheckClanResult(_003CrequestId_003E5__2, msg.player, result2, out _003Cclan_003E5__4))
				{
					_003CpreviousTimestamp_003E5__5 = _003Cclan_003E5__4.MotdTimestamp;
					valueTaskAwaiter2 = _003Cclan_003E5__4.SetMotd(((ClanValidatorResult)(ref _003CvalidatedMotd_003E5__3)).Value, (ulong)msg.player.userID).GetAwaiter();
					if (!valueTaskAwaiter2.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = valueTaskAwaiter2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanResult>, _003CServer_SetMotd_003Ed__11>(ref valueTaskAwaiter2, ref this);
						return;
					}
					goto IL_021d;
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CvalidatedMotd_003E5__3 = default(ClanValidatorResult);
				_003Cclan_003E5__4 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CvalidatedMotd_003E5__3 = default(ClanValidatorResult);
			_003Cclan_003E5__4 = null;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_SetPlayerNotes_003Ed__18 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private ulong _003CsteamId_003E5__3;

		private ClanValidatorResult _003CvalidatedNotes_003E5__4;

		private IClan _003Cclan_003E5__5;

		private ValueTaskAwaiter<ClanValueResult<IClan>> _003C_003Eu__1;

		private ValueTaskAwaiter<ClanResult> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0272: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_022a: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Unknown result type (might be due to invalid IL or missing references)
			//IL_024d: Unknown result type (might be due to invalid IL or missing references)
			//IL_029c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<IClan>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<IClan>>);
					num = (_003C_003E1__state = -1);
					goto IL_0175;
				}
				ValueTaskAwaiter<ClanResult> valueTaskAwaiter2;
				if (num == 1)
				{
					valueTaskAwaiter2 = _003C_003Eu__2;
					_003C_003Eu__2 = default(ValueTaskAwaiter<ClanResult>);
					num = (_003C_003E1__state = -1);
					goto IL_0228;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					_003CsteamId_003E5__3 = msg.read.UInt64();
					string text = msg.read.StringMultiLine(1024);
					if (!msg.player.CanModifyClan())
					{
						clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, (ClanResult)20));
					}
					else
					{
						_003CvalidatedNotes_003E5__4 = ClanValidator.ValidatePlayerNote(text);
						if (((ClanValidatorResult)(ref _003CvalidatedNotes_003E5__4)).Success)
						{
							valueTaskAwaiter = clanManager.Backend.Get(msg.player.clanId).GetAwaiter();
							if (!valueTaskAwaiter.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = valueTaskAwaiter;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<IClan>>, _003CServer_SetPlayerNotes_003Ed__18>(ref valueTaskAwaiter, ref this);
								return;
							}
							goto IL_0175;
						}
						clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, ClanValidator.ToClanResult(((ClanValidatorResult)(ref _003CvalidatedNotes_003E5__4)).Error)));
					}
				}
				goto end_IL_000e;
				IL_0228:
				ClanResult result = valueTaskAwaiter2.GetResult();
				clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, result, _003Cclan_003E5__5));
				goto end_IL_000e;
				IL_0175:
				ClanValueResult<IClan> result2 = valueTaskAwaiter.GetResult();
				if (clanManager.CheckClanResult(_003CrequestId_003E5__2, msg.player, result2, out _003Cclan_003E5__5))
				{
					valueTaskAwaiter2 = _003Cclan_003E5__5.SetPlayerNotes(_003CsteamId_003E5__3, ((ClanValidatorResult)(ref _003CvalidatedNotes_003E5__4)).Value, (ulong)msg.player.userID).GetAwaiter();
					if (!valueTaskAwaiter2.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = valueTaskAwaiter2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanResult>, _003CServer_SetPlayerNotes_003Ed__18>(ref valueTaskAwaiter2, ref this);
						return;
					}
					goto IL_0228;
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CvalidatedNotes_003E5__4 = default(ClanValidatorResult);
				_003Cclan_003E5__5 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CvalidatedNotes_003E5__4 = default(ClanValidatorResult);
			_003Cclan_003E5__5 = null;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_SetPlayerRole_003Ed__17 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private ulong _003CsteamId_003E5__3;

		private int _003CnewRoleId_003E5__4;

		private IClan _003Cclan_003E5__5;

		private ValueTaskAwaiter<ClanValueResult<IClan>> _003C_003Eu__1;

		private ValueTaskAwaiter<ClanResult> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<IClan>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<IClan>>);
					num = (_003C_003E1__state = -1);
					goto IL_0120;
				}
				ValueTaskAwaiter<ClanResult> valueTaskAwaiter2;
				if (num == 1)
				{
					valueTaskAwaiter2 = _003C_003Eu__2;
					_003C_003Eu__2 = default(ValueTaskAwaiter<ClanResult>);
					num = (_003C_003E1__state = -1);
					goto IL_01ce;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					_003CsteamId_003E5__3 = msg.read.UInt64();
					_003CnewRoleId_003E5__4 = msg.read.Int32();
					if (msg.player.CanModifyClan())
					{
						valueTaskAwaiter = clanManager.Backend.Get(msg.player.clanId).GetAwaiter();
						if (!valueTaskAwaiter.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = valueTaskAwaiter;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<IClan>>, _003CServer_SetPlayerRole_003Ed__17>(ref valueTaskAwaiter, ref this);
							return;
						}
						goto IL_0120;
					}
					clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, (ClanResult)20));
				}
				goto end_IL_000e;
				IL_01ce:
				ClanResult result = valueTaskAwaiter2.GetResult();
				clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, result, _003Cclan_003E5__5));
				goto end_IL_000e;
				IL_0120:
				ClanValueResult<IClan> result2 = valueTaskAwaiter.GetResult();
				if (clanManager.CheckClanResult(_003CrequestId_003E5__2, msg.player, result2, out _003Cclan_003E5__5))
				{
					valueTaskAwaiter2 = _003Cclan_003E5__5.SetPlayerRole(_003CsteamId_003E5__3, _003CnewRoleId_003E5__4, (ulong)msg.player.userID).GetAwaiter();
					if (!valueTaskAwaiter2.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = valueTaskAwaiter2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanResult>, _003CServer_SetPlayerRole_003Ed__17>(ref valueTaskAwaiter2, ref this);
						return;
					}
					goto IL_01ce;
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cclan_003E5__5 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cclan_003E5__5 = null;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_SwapRoles_003Ed__22 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private int _003CroleIdA_003E5__3;

		private int _003CroleIdB_003E5__4;

		private IClan _003Cclan_003E5__5;

		private ValueTaskAwaiter<ClanValueResult<IClan>> _003C_003Eu__1;

		private ValueTaskAwaiter<ClanResult> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				ValueTaskAwaiter<ClanValueResult<IClan>> valueTaskAwaiter;
				if (num == 0)
				{
					valueTaskAwaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<IClan>>);
					num = (_003C_003E1__state = -1);
					goto IL_0120;
				}
				ValueTaskAwaiter<ClanResult> valueTaskAwaiter2;
				if (num == 1)
				{
					valueTaskAwaiter2 = _003C_003Eu__2;
					_003C_003Eu__2 = default(ValueTaskAwaiter<ClanResult>);
					num = (_003C_003E1__state = -1);
					goto IL_01ce;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					_003CroleIdA_003E5__3 = msg.read.Int32();
					_003CroleIdB_003E5__4 = msg.read.Int32();
					if (msg.player.CanModifyClan())
					{
						valueTaskAwaiter = clanManager.Backend.Get(msg.player.clanId).GetAwaiter();
						if (!valueTaskAwaiter.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = valueTaskAwaiter;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<IClan>>, _003CServer_SwapRoles_003Ed__22>(ref valueTaskAwaiter, ref this);
							return;
						}
						goto IL_0120;
					}
					clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, (ClanResult)20));
				}
				goto end_IL_000e;
				IL_01ce:
				ClanResult result = valueTaskAwaiter2.GetResult();
				clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, result, _003Cclan_003E5__5));
				goto end_IL_000e;
				IL_0120:
				ClanValueResult<IClan> result2 = valueTaskAwaiter.GetResult();
				if (clanManager.CheckClanResult(_003CrequestId_003E5__2, msg.player, result2, out _003Cclan_003E5__5))
				{
					valueTaskAwaiter2 = _003Cclan_003E5__5.SwapRoleRanks(_003CroleIdA_003E5__3, _003CroleIdB_003E5__4, (ulong)msg.player.userID).GetAwaiter();
					if (!valueTaskAwaiter2.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = valueTaskAwaiter2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanResult>, _003CServer_SwapRoles_003Ed__22>(ref valueTaskAwaiter2, ref this);
						return;
					}
					goto IL_01ce;
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cclan_003E5__5 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cclan_003E5__5 = null;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CServer_UpdateRole_003Ed__20 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ClanManager _003C_003E4__this;

		public RPCMessage msg;

		private int _003CrequestId_003E5__2;

		private Role _003Crole_003E5__3;

		private IClan _003Cclan_003E5__4;

		private ValueTaskAwaiter<ClanValueResult<IClan>> _003C_003Eu__1;

		private ValueTaskAwaiter<ClanResult> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_0225: Unknown result type (might be due to invalid IL or missing references)
			//IL_022a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			ClanManager clanManager = _003C_003E4__this;
			try
			{
				if ((uint)num <= 1u)
				{
					goto IL_0096;
				}
				if (Clan.enabled && clanManager.Backend != null)
				{
					_003CrequestId_003E5__2 = msg.read.Int32();
					if (msg.player.CanModifyClan())
					{
						_003Crole_003E5__3 = msg.read.Proto<Role>((Role)null);
						goto IL_0096;
					}
					clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, (ClanResult)20));
				}
				goto end_IL_000e;
				IL_0096:
				try
				{
					ValueTaskAwaiter<ClanValueResult<IClan>> valueTaskAwaiter;
					if (num == 0)
					{
						valueTaskAwaiter = _003C_003Eu__1;
						_003C_003Eu__1 = default(ValueTaskAwaiter<ClanValueResult<IClan>>);
						num = (_003C_003E1__state = -1);
						goto IL_0176;
					}
					ValueTaskAwaiter<ClanResult> valueTaskAwaiter2;
					if (num == 1)
					{
						valueTaskAwaiter2 = _003C_003Eu__2;
						_003C_003Eu__2 = default(ValueTaskAwaiter<ClanResult>);
						num = (_003C_003E1__state = -1);
						goto IL_0223;
					}
					ClanValidatorResult val = ClanValidator.ValidateRoleName(_003Crole_003E5__3.name);
					if (((ClanValidatorResult)(ref val)).Success)
					{
						_003Crole_003E5__3.name = ((ClanValidatorResult)(ref val)).Value;
						valueTaskAwaiter = clanManager.Backend.Get(msg.player.clanId).GetAwaiter();
						if (!valueTaskAwaiter.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = valueTaskAwaiter;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanValueResult<IClan>>, _003CServer_UpdateRole_003Ed__20>(ref valueTaskAwaiter, ref this);
							return;
						}
						goto IL_0176;
					}
					clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, ClanValidator.ToClanResult(((ClanValidatorResult)(ref val)).Error)));
					goto end_IL_0096;
					IL_0223:
					ClanResult result = valueTaskAwaiter2.GetResult();
					clanManager.ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", msg.player), BuildActionResult(_003CrequestId_003E5__2, result, _003Cclan_003E5__4));
					goto end_IL_0096;
					IL_0176:
					ClanValueResult<IClan> result2 = valueTaskAwaiter.GetResult();
					if (clanManager.CheckClanResult(_003CrequestId_003E5__2, msg.player, result2, out _003Cclan_003E5__4))
					{
						valueTaskAwaiter2 = _003Cclan_003E5__4.UpdateRole(ClanInfoExtensions.FromProto(_003Crole_003E5__3), (ulong)msg.player.userID).GetAwaiter();
						if (!valueTaskAwaiter2.IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__2 = valueTaskAwaiter2;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<ValueTaskAwaiter<ClanResult>, _003CServer_UpdateRole_003Ed__20>(ref valueTaskAwaiter2, ref this);
							return;
						}
						goto IL_0223;
					}
					end_IL_0096:;
				}
				finally
				{
					if (num < 0 && _003Crole_003E5__3 != null)
					{
						((IDisposable)_003Crole_003E5__3).Dispose();
					}
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Crole_003E5__3 = null;
				_003Cclan_003E5__4 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Crole_003E5__3 = null;
			_003Cclan_003E5__4 = null;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private RealTimeSince _sinceLastLeaderboardUpdate;

	private List<ClanLeaderboardEntry> _leaderboardCache;

	public static readonly TokenisedPhrase InvitationToast = new TokenisedPhrase("clan.invitation.toast", "You were invited to {clanName}! Press [clan.toggleclan] to manage your clan invitations.");

	private const int MaxMetadataRequestsPerSecond = 3;

	private const float MaxMetadataRequestInterval = 0.5f;

	private const float MetadataExpiry = 300f;

	private readonly Dictionary<long, List<Connection>> _clanMemberConnections = new Dictionary<long, List<Connection>>();

	public const int LogoSize = 512;

	private string _backendType;

	private ClanChangeTracker _changeTracker;

	public static ClanManager ServerInstance { get; private set; }

	public IClanBackend Backend { get; private set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0909: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a0f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b15: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c1b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d21: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e27: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f2d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1033: Unknown result type (might be due to invalid IL or missing references)
		//IL_1139: Unknown result type (might be due to invalid IL or missing references)
		//IL_123f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1345: Unknown result type (might be due to invalid IL or missing references)
		//IL_144b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1551: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ClanManager.OnRpcMessage"))
		{
			if (rpc == 3593616087u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_AcceptInvitation"));
				}
				using (TimeWarning.New("Server_AcceptInvitation"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3593616087u, "Server_AcceptInvitation", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_AcceptInvitation(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in Server_AcceptInvitation");
					}
				}
				return true;
			}
			if (rpc == 73135447 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_CancelInvitation"));
				}
				using (TimeWarning.New("Server_CancelInvitation"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(73135447u, "Server_CancelInvitation", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_CancelInvitation(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in Server_CancelInvitation");
					}
				}
				return true;
			}
			if (rpc == 785874715 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_CancelInvite"));
				}
				using (TimeWarning.New("Server_CancelInvite"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(785874715u, "Server_CancelInvite", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_CancelInvite(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in Server_CancelInvite");
					}
				}
				return true;
			}
			if (rpc == 4017901233u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_CreateClan"));
				}
				using (TimeWarning.New("Server_CreateClan"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4017901233u, "Server_CreateClan", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_CreateClan(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in Server_CreateClan");
					}
				}
				return true;
			}
			if (rpc == 835697933 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_CreateRole"));
				}
				using (TimeWarning.New("Server_CreateRole"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(835697933u, "Server_CreateRole", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg6 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_CreateRole(msg6);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in Server_CreateRole");
					}
				}
				return true;
			}
			if (rpc == 3966624879u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_DeleteRole"));
				}
				using (TimeWarning.New("Server_DeleteRole"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3966624879u, "Server_DeleteRole", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg7 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_DeleteRole(msg7);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
						player.Kick("RPC Error in Server_DeleteRole");
					}
				}
				return true;
			}
			if (rpc == 4071826018u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_GetClan"));
				}
				using (TimeWarning.New("Server_GetClan"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4071826018u, "Server_GetClan", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg8 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_GetClan(msg8);
						}
					}
					catch (Exception ex7)
					{
						Debug.LogException(ex7);
						player.Kick("RPC Error in Server_GetClan");
					}
				}
				return true;
			}
			if (rpc == 2338234158u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_GetClanMetadata"));
				}
				using (TimeWarning.New("Server_GetClanMetadata"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2338234158u, "Server_GetClanMetadata", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg9 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_GetClanMetadata(msg9);
						}
					}
					catch (Exception ex8)
					{
						Debug.LogException(ex8);
						player.Kick("RPC Error in Server_GetClanMetadata");
					}
				}
				return true;
			}
			if (rpc == 507204008 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_GetInvitations"));
				}
				using (TimeWarning.New("Server_GetInvitations"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(507204008u, "Server_GetInvitations", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg10 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_GetInvitations(msg10);
						}
					}
					catch (Exception ex9)
					{
						Debug.LogException(ex9);
						player.Kick("RPC Error in Server_GetInvitations");
					}
				}
				return true;
			}
			if (rpc == 1953068009 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_GetLeaderboard"));
				}
				using (TimeWarning.New("Server_GetLeaderboard"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1953068009u, "Server_GetLeaderboard", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg11 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_GetLeaderboard(msg11);
						}
					}
					catch (Exception ex10)
					{
						Debug.LogException(ex10);
						player.Kick("RPC Error in Server_GetLeaderboard");
					}
				}
				return true;
			}
			if (rpc == 3858074978u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_GetLogs"));
				}
				using (TimeWarning.New("Server_GetLogs"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3858074978u, "Server_GetLogs", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg12 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_GetLogs(msg12);
						}
					}
					catch (Exception ex11)
					{
						Debug.LogException(ex11);
						player.Kick("RPC Error in Server_GetLogs");
					}
				}
				return true;
			}
			if (rpc == 558876504 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_GetScoreEvents"));
				}
				using (TimeWarning.New("Server_GetScoreEvents"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(558876504u, "Server_GetScoreEvents", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg13 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_GetScoreEvents(msg13);
						}
					}
					catch (Exception ex12)
					{
						Debug.LogException(ex12);
						player.Kick("RPC Error in Server_GetScoreEvents");
					}
				}
				return true;
			}
			if (rpc == 1782867876 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_Invite"));
				}
				using (TimeWarning.New("Server_Invite"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1782867876u, "Server_Invite", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg14 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_Invite(msg14);
						}
					}
					catch (Exception ex13)
					{
						Debug.LogException(ex13);
						player.Kick("RPC Error in Server_Invite");
					}
				}
				return true;
			}
			if (rpc == 3093528332u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_Kick"));
				}
				using (TimeWarning.New("Server_Kick"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3093528332u, "Server_Kick", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg15 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_Kick(msg15);
						}
					}
					catch (Exception ex14)
					{
						Debug.LogException(ex14);
						player.Kick("RPC Error in Server_Kick");
					}
				}
				return true;
			}
			if (rpc == 2235419116u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_SetColor"));
				}
				using (TimeWarning.New("Server_SetColor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2235419116u, "Server_SetColor", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg16 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_SetColor(msg16);
						}
					}
					catch (Exception ex15)
					{
						Debug.LogException(ex15);
						player.Kick("RPC Error in Server_SetColor");
					}
				}
				return true;
			}
			if (rpc == 1189444132 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_SetLogo"));
				}
				using (TimeWarning.New("Server_SetLogo"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1189444132u, "Server_SetLogo", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg17 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_SetLogo(msg17);
						}
					}
					catch (Exception ex16)
					{
						Debug.LogException(ex16);
						player.Kick("RPC Error in Server_SetLogo");
					}
				}
				return true;
			}
			if (rpc == 4088477037u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_SetMotd"));
				}
				using (TimeWarning.New("Server_SetMotd"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4088477037u, "Server_SetMotd", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg18 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_SetMotd(msg18);
						}
					}
					catch (Exception ex17)
					{
						Debug.LogException(ex17);
						player.Kick("RPC Error in Server_SetMotd");
					}
				}
				return true;
			}
			if (rpc == 285489852 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_SetPlayerNotes"));
				}
				using (TimeWarning.New("Server_SetPlayerNotes"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(285489852u, "Server_SetPlayerNotes", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg19 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_SetPlayerNotes(msg19);
						}
					}
					catch (Exception ex18)
					{
						Debug.LogException(ex18);
						player.Kick("RPC Error in Server_SetPlayerNotes");
					}
				}
				return true;
			}
			if (rpc == 3232449870u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_SetPlayerRole"));
				}
				using (TimeWarning.New("Server_SetPlayerRole"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3232449870u, "Server_SetPlayerRole", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg20 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_SetPlayerRole(msg20);
						}
					}
					catch (Exception ex19)
					{
						Debug.LogException(ex19);
						player.Kick("RPC Error in Server_SetPlayerRole");
					}
				}
				return true;
			}
			if (rpc == 738181899 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_SwapRoles"));
				}
				using (TimeWarning.New("Server_SwapRoles"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(738181899u, "Server_SwapRoles", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg21 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_SwapRoles(msg21);
						}
					}
					catch (Exception ex20)
					{
						Debug.LogException(ex20);
						player.Kick("RPC Error in Server_SwapRoles");
					}
				}
				return true;
			}
			if (rpc == 1548667516 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_UpdateRole"));
				}
				using (TimeWarning.New("Server_UpdateRole"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1548667516u, "Server_UpdateRole", this, player, 3uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg22 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_UpdateRole(msg22);
						}
					}
					catch (Exception ex21)
					{
						Debug.LogException(ex21);
						player.Kick("RPC Error in Server_UpdateRole");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[AsyncStateMachine(typeof(_003CServer_CreateClan_003Ed__1))]
	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	public UniTaskVoid Server_CreateClan(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_CreateClan_003Ed__1 _003CServer_CreateClan_003Ed__2 = default(_003CServer_CreateClan_003Ed__1);
		_003CServer_CreateClan_003Ed__2._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_CreateClan_003Ed__2._003C_003E4__this = this;
		_003CServer_CreateClan_003Ed__2.msg = msg;
		_003CServer_CreateClan_003Ed__2._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_CreateClan_003Ed__2._003C_003Et__builder)).Start<_003CServer_CreateClan_003Ed__1>(ref _003CServer_CreateClan_003Ed__2);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_CreateClan_003Ed__2._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CServer_GetClan_003Ed__2))]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	public UniTaskVoid Server_GetClan(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_GetClan_003Ed__2 _003CServer_GetClan_003Ed__3 = default(_003CServer_GetClan_003Ed__2);
		_003CServer_GetClan_003Ed__3._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_GetClan_003Ed__3._003C_003E4__this = this;
		_003CServer_GetClan_003Ed__3.msg = msg;
		_003CServer_GetClan_003Ed__3._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_GetClan_003Ed__3._003C_003Et__builder)).Start<_003CServer_GetClan_003Ed__2>(ref _003CServer_GetClan_003Ed__3);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_GetClan_003Ed__3._003C_003Et__builder)).Task;
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	[AsyncStateMachine(typeof(_003CServer_GetLogs_003Ed__3))]
	public UniTaskVoid Server_GetLogs(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_GetLogs_003Ed__3 _003CServer_GetLogs_003Ed__4 = default(_003CServer_GetLogs_003Ed__3);
		_003CServer_GetLogs_003Ed__4._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_GetLogs_003Ed__4._003C_003E4__this = this;
		_003CServer_GetLogs_003Ed__4.msg = msg;
		_003CServer_GetLogs_003Ed__4._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_GetLogs_003Ed__4._003C_003Et__builder)).Start<_003CServer_GetLogs_003Ed__3>(ref _003CServer_GetLogs_003Ed__4);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_GetLogs_003Ed__4._003C_003Et__builder)).Task;
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	[AsyncStateMachine(typeof(_003CServer_GetScoreEvents_003Ed__4))]
	public UniTaskVoid Server_GetScoreEvents(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_GetScoreEvents_003Ed__4 _003CServer_GetScoreEvents_003Ed__5 = default(_003CServer_GetScoreEvents_003Ed__4);
		_003CServer_GetScoreEvents_003Ed__5._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_GetScoreEvents_003Ed__5._003C_003E4__this = this;
		_003CServer_GetScoreEvents_003Ed__5.msg = msg;
		_003CServer_GetScoreEvents_003Ed__5._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_GetScoreEvents_003Ed__5._003C_003Et__builder)).Start<_003CServer_GetScoreEvents_003Ed__4>(ref _003CServer_GetScoreEvents_003Ed__5);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_GetScoreEvents_003Ed__5._003C_003Et__builder)).Task;
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	[AsyncStateMachine(typeof(_003CServer_GetInvitations_003Ed__5))]
	public UniTaskVoid Server_GetInvitations(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_GetInvitations_003Ed__5 _003CServer_GetInvitations_003Ed__6 = default(_003CServer_GetInvitations_003Ed__5);
		_003CServer_GetInvitations_003Ed__6._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_GetInvitations_003Ed__6._003C_003E4__this = this;
		_003CServer_GetInvitations_003Ed__6.msg = msg;
		_003CServer_GetInvitations_003Ed__6._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_GetInvitations_003Ed__6._003C_003Et__builder)).Start<_003CServer_GetInvitations_003Ed__5>(ref _003CServer_GetInvitations_003Ed__6);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_GetInvitations_003Ed__6._003C_003Et__builder)).Task;
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	[AsyncStateMachine(typeof(_003CServer_GetLeaderboard_003Ed__8))]
	public UniTaskVoid Server_GetLeaderboard(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_GetLeaderboard_003Ed__8 _003CServer_GetLeaderboard_003Ed__9 = default(_003CServer_GetLeaderboard_003Ed__8);
		_003CServer_GetLeaderboard_003Ed__9._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_GetLeaderboard_003Ed__9._003C_003E4__this = this;
		_003CServer_GetLeaderboard_003Ed__9.msg = msg;
		_003CServer_GetLeaderboard_003Ed__9._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_GetLeaderboard_003Ed__9._003C_003Et__builder)).Start<_003CServer_GetLeaderboard_003Ed__8>(ref _003CServer_GetLeaderboard_003Ed__9);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_GetLeaderboard_003Ed__9._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CServer_SetLogo_003Ed__9))]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	public UniTaskVoid Server_SetLogo(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_SetLogo_003Ed__9 _003CServer_SetLogo_003Ed__10 = default(_003CServer_SetLogo_003Ed__9);
		_003CServer_SetLogo_003Ed__10._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_SetLogo_003Ed__10._003C_003E4__this = this;
		_003CServer_SetLogo_003Ed__10.msg = msg;
		_003CServer_SetLogo_003Ed__10._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_SetLogo_003Ed__10._003C_003Et__builder)).Start<_003CServer_SetLogo_003Ed__9>(ref _003CServer_SetLogo_003Ed__10);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_SetLogo_003Ed__10._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CServer_SetColor_003Ed__10))]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	public UniTaskVoid Server_SetColor(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_SetColor_003Ed__10 _003CServer_SetColor_003Ed__11 = default(_003CServer_SetColor_003Ed__10);
		_003CServer_SetColor_003Ed__11._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_SetColor_003Ed__11._003C_003E4__this = this;
		_003CServer_SetColor_003Ed__11.msg = msg;
		_003CServer_SetColor_003Ed__11._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_SetColor_003Ed__11._003C_003Et__builder)).Start<_003CServer_SetColor_003Ed__10>(ref _003CServer_SetColor_003Ed__11);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_SetColor_003Ed__11._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CServer_SetMotd_003Ed__11))]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	public UniTaskVoid Server_SetMotd(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_SetMotd_003Ed__11 _003CServer_SetMotd_003Ed__12 = default(_003CServer_SetMotd_003Ed__11);
		_003CServer_SetMotd_003Ed__12._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_SetMotd_003Ed__12._003C_003E4__this = this;
		_003CServer_SetMotd_003Ed__12.msg = msg;
		_003CServer_SetMotd_003Ed__12._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_SetMotd_003Ed__12._003C_003Et__builder)).Start<_003CServer_SetMotd_003Ed__11>(ref _003CServer_SetMotd_003Ed__12);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_SetMotd_003Ed__12._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CServer_Invite_003Ed__12))]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	public UniTaskVoid Server_Invite(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_Invite_003Ed__12 _003CServer_Invite_003Ed__13 = default(_003CServer_Invite_003Ed__12);
		_003CServer_Invite_003Ed__13._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_Invite_003Ed__13._003C_003E4__this = this;
		_003CServer_Invite_003Ed__13.msg = msg;
		_003CServer_Invite_003Ed__13._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_Invite_003Ed__13._003C_003Et__builder)).Start<_003CServer_Invite_003Ed__12>(ref _003CServer_Invite_003Ed__13);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_Invite_003Ed__13._003C_003Et__builder)).Task;
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[AsyncStateMachine(typeof(_003CServer_CancelInvite_003Ed__13))]
	[RPC_Server]
	public UniTaskVoid Server_CancelInvite(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_CancelInvite_003Ed__13 _003CServer_CancelInvite_003Ed__14 = default(_003CServer_CancelInvite_003Ed__13);
		_003CServer_CancelInvite_003Ed__14._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_CancelInvite_003Ed__14._003C_003E4__this = this;
		_003CServer_CancelInvite_003Ed__14.msg = msg;
		_003CServer_CancelInvite_003Ed__14._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_CancelInvite_003Ed__14._003C_003Et__builder)).Start<_003CServer_CancelInvite_003Ed__13>(ref _003CServer_CancelInvite_003Ed__14);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_CancelInvite_003Ed__14._003C_003Et__builder)).Task;
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	[AsyncStateMachine(typeof(_003CServer_AcceptInvitation_003Ed__14))]
	public UniTaskVoid Server_AcceptInvitation(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_AcceptInvitation_003Ed__14 _003CServer_AcceptInvitation_003Ed__15 = default(_003CServer_AcceptInvitation_003Ed__14);
		_003CServer_AcceptInvitation_003Ed__15._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_AcceptInvitation_003Ed__15._003C_003E4__this = this;
		_003CServer_AcceptInvitation_003Ed__15.msg = msg;
		_003CServer_AcceptInvitation_003Ed__15._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_AcceptInvitation_003Ed__15._003C_003Et__builder)).Start<_003CServer_AcceptInvitation_003Ed__14>(ref _003CServer_AcceptInvitation_003Ed__15);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_AcceptInvitation_003Ed__15._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CServer_CancelInvitation_003Ed__15))]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	public UniTaskVoid Server_CancelInvitation(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_CancelInvitation_003Ed__15 _003CServer_CancelInvitation_003Ed__16 = default(_003CServer_CancelInvitation_003Ed__15);
		_003CServer_CancelInvitation_003Ed__16._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_CancelInvitation_003Ed__16._003C_003E4__this = this;
		_003CServer_CancelInvitation_003Ed__16.msg = msg;
		_003CServer_CancelInvitation_003Ed__16._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_CancelInvitation_003Ed__16._003C_003Et__builder)).Start<_003CServer_CancelInvitation_003Ed__15>(ref _003CServer_CancelInvitation_003Ed__16);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_CancelInvitation_003Ed__16._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CServer_Kick_003Ed__16))]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	public UniTaskVoid Server_Kick(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_Kick_003Ed__16 _003CServer_Kick_003Ed__17 = default(_003CServer_Kick_003Ed__16);
		_003CServer_Kick_003Ed__17._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_Kick_003Ed__17._003C_003E4__this = this;
		_003CServer_Kick_003Ed__17.msg = msg;
		_003CServer_Kick_003Ed__17._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_Kick_003Ed__17._003C_003Et__builder)).Start<_003CServer_Kick_003Ed__16>(ref _003CServer_Kick_003Ed__17);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_Kick_003Ed__17._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CServer_SetPlayerRole_003Ed__17))]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	public UniTaskVoid Server_SetPlayerRole(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_SetPlayerRole_003Ed__17 _003CServer_SetPlayerRole_003Ed__18 = default(_003CServer_SetPlayerRole_003Ed__17);
		_003CServer_SetPlayerRole_003Ed__18._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_SetPlayerRole_003Ed__18._003C_003E4__this = this;
		_003CServer_SetPlayerRole_003Ed__18.msg = msg;
		_003CServer_SetPlayerRole_003Ed__18._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_SetPlayerRole_003Ed__18._003C_003Et__builder)).Start<_003CServer_SetPlayerRole_003Ed__17>(ref _003CServer_SetPlayerRole_003Ed__18);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_SetPlayerRole_003Ed__18._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CServer_SetPlayerNotes_003Ed__18))]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	public UniTaskVoid Server_SetPlayerNotes(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_SetPlayerNotes_003Ed__18 _003CServer_SetPlayerNotes_003Ed__19 = default(_003CServer_SetPlayerNotes_003Ed__18);
		_003CServer_SetPlayerNotes_003Ed__19._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_SetPlayerNotes_003Ed__19._003C_003E4__this = this;
		_003CServer_SetPlayerNotes_003Ed__19.msg = msg;
		_003CServer_SetPlayerNotes_003Ed__19._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_SetPlayerNotes_003Ed__19._003C_003Et__builder)).Start<_003CServer_SetPlayerNotes_003Ed__18>(ref _003CServer_SetPlayerNotes_003Ed__19);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_SetPlayerNotes_003Ed__19._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CServer_CreateRole_003Ed__19))]
	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	public UniTaskVoid Server_CreateRole(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_CreateRole_003Ed__19 _003CServer_CreateRole_003Ed__20 = default(_003CServer_CreateRole_003Ed__19);
		_003CServer_CreateRole_003Ed__20._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_CreateRole_003Ed__20._003C_003E4__this = this;
		_003CServer_CreateRole_003Ed__20.msg = msg;
		_003CServer_CreateRole_003Ed__20._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_CreateRole_003Ed__20._003C_003Et__builder)).Start<_003CServer_CreateRole_003Ed__19>(ref _003CServer_CreateRole_003Ed__20);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_CreateRole_003Ed__20._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CServer_UpdateRole_003Ed__20))]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	public UniTaskVoid Server_UpdateRole(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_UpdateRole_003Ed__20 _003CServer_UpdateRole_003Ed__21 = default(_003CServer_UpdateRole_003Ed__20);
		_003CServer_UpdateRole_003Ed__21._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_UpdateRole_003Ed__21._003C_003E4__this = this;
		_003CServer_UpdateRole_003Ed__21.msg = msg;
		_003CServer_UpdateRole_003Ed__21._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_UpdateRole_003Ed__21._003C_003Et__builder)).Start<_003CServer_UpdateRole_003Ed__20>(ref _003CServer_UpdateRole_003Ed__21);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_UpdateRole_003Ed__21._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CServer_DeleteRole_003Ed__21))]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	public UniTaskVoid Server_DeleteRole(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_DeleteRole_003Ed__21 _003CServer_DeleteRole_003Ed__22 = default(_003CServer_DeleteRole_003Ed__21);
		_003CServer_DeleteRole_003Ed__22._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_DeleteRole_003Ed__22._003C_003E4__this = this;
		_003CServer_DeleteRole_003Ed__22.msg = msg;
		_003CServer_DeleteRole_003Ed__22._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_DeleteRole_003Ed__22._003C_003Et__builder)).Start<_003CServer_DeleteRole_003Ed__21>(ref _003CServer_DeleteRole_003Ed__22);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_DeleteRole_003Ed__22._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CServer_SwapRoles_003Ed__22))]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	public UniTaskVoid Server_SwapRoles(RPCMessage msg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CServer_SwapRoles_003Ed__22 _003CServer_SwapRoles_003Ed__23 = default(_003CServer_SwapRoles_003Ed__22);
		_003CServer_SwapRoles_003Ed__23._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CServer_SwapRoles_003Ed__23._003C_003E4__this = this;
		_003CServer_SwapRoles_003Ed__23.msg = msg;
		_003CServer_SwapRoles_003Ed__23._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_SwapRoles_003Ed__23._003C_003Et__builder)).Start<_003CServer_SwapRoles_003Ed__22>(ref _003CServer_SwapRoles_003Ed__23);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CServer_SwapRoles_003Ed__23._003C_003Et__builder)).Task;
	}

	private bool CheckClanResult(int requestId, BasePlayer player, ClanValueResult<IClan> result, out IClan clan)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (result.IsSuccess)
		{
			clan = result.Value;
			return true;
		}
		ClientRPC(RpcTarget.Player("Client_ReceiveActionResult", player), BuildActionResult(requestId, result.Result));
		clan = null;
		return false;
	}

	private static ClanActionResult BuildActionResult(int requestId, ClanResult result)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected I4, but got Unknown
		ClanActionResult obj = Pool.Get<ClanActionResult>();
		obj.requestId = requestId;
		obj.result = (int)result;
		obj.hasClanInfo = false;
		obj.clanInfo = null;
		return obj;
	}

	private static ClanActionResult BuildActionResult(int requestId, ClanResult result, [NotNull] IClan clan, bool includeLogo = false)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected I4, but got Unknown
		Assert.NotNull(clan, "clan is null");
		ClanActionResult val = Pool.Get<ClanActionResult>();
		val.requestId = requestId;
		val.result = (int)result;
		val.hasClanInfo = true;
		val.clanInfo = ClanInfoExtensions.ToProto(clan);
		if (val.clanInfo != null && !includeLogo)
		{
			val.clanInfo.logo = null;
		}
		return val;
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	public async void Server_GetClanMetadata(RPCMessage msg)
	{
		long clanId = msg.read.Int64();
		ClanValueResult<IClan> val = await Backend.Get(clanId);
		if (val.IsSuccess)
		{
			IClan value = val.Value;
			ClientRPC(RpcTarget.Player("Client_GetClanMetadataResponse", msg.player), clanId, value.Name ?? "", value.Members?.Count ?? 0, value.Color);
		}
		else
		{
			ClientRPC(RpcTarget.Player("Client_GetClanMetadataResponse", msg.player), clanId, "[unknown]", 0, Color32.op_Implicit(Color.white));
		}
	}

	public void AddScore(IClan clan, ClanScoreEvent entry)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Assert.IsNotNull<IClan>(clan, "clan != null");
		ValueTask<ClanResult> task = clan.AddScoreEvent(entry);
		if (task.IsCompletedSuccessfully)
		{
			CheckResult(task.Result);
		}
		else
		{
			AwaitResult(task);
		}
		async void AwaitResult(ValueTask<ClanResult> valueTask)
		{
			try
			{
				CheckResult(await valueTask);
			}
			catch (Exception ex)
			{
				Debug.LogError((object)$"Exception while adding score event to clan {clan.ClanId}:");
				Debug.LogException(ex);
			}
		}
		void CheckResult(ClanResult result)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Invalid comparison between Unknown and I4
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if ((int)result != 1)
			{
				Debug.LogWarning((object)$"Failed to add score event to clan {clan.ClanId}: {result}");
			}
		}
	}

	public void SendClanChanged(IClan clan)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		List<Connection> list = Pool.Get<List<Connection>>();
		foreach (ClanMember member in clan.Members)
		{
			BasePlayer basePlayer = BasePlayer.FindByID(member.SteamId);
			if ((Object)(object)basePlayer != (Object)null && basePlayer.IsConnected)
			{
				list.Add(basePlayer.net.connection);
			}
		}
		ClientRPC(RpcTarget.Players("Client_CurrentClanChanged", list));
		Pool.FreeUnmanaged<Connection>(ref list);
	}

	public void SendClanInvitation(ulong steamId, long clanId)
	{
		BasePlayer basePlayer = BasePlayer.FindByID(steamId);
		if (!((Object)(object)basePlayer == (Object)null) && basePlayer.IsConnected)
		{
			ClientRPC(RpcTarget.Player("Client_ReceiveClanInvitation", basePlayer), clanId);
		}
	}

	public bool TryGetClanMemberConnections(long clanId, out List<Connection> connections)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (_clanMemberConnections.TryGetValue(clanId, out connections))
		{
			return true;
		}
		IClan val = default(IClan);
		if (!Backend.TryGet(clanId, ref val))
		{
			return false;
		}
		connections = Pool.Get<List<Connection>>();
		foreach (ClanMember member in val.Members)
		{
			BasePlayer basePlayer = BasePlayer.FindByID(member.SteamId);
			if ((Object)(object)basePlayer == (Object)null)
			{
				basePlayer = BasePlayer.FindSleeping(member.SteamId);
			}
			if (!((Object)(object)basePlayer == (Object)null) && basePlayer.IsConnected)
			{
				connections.Add(basePlayer.Connection);
			}
		}
		_clanMemberConnections.Add(clanId, connections);
		return true;
	}

	public void ClanMemberConnectionsChanged(long clanId)
	{
		if (_clanMemberConnections.TryGetValue(clanId, out var value))
		{
			_clanMemberConnections.Remove(clanId);
			Pool.FreeUnmanaged<Connection>(ref value);
		}
	}

	public async void LoadClanInfoForSleepers()
	{
		Dictionary<ulong, BasePlayer> sleepers = Pool.Get<Dictionary<ulong, BasePlayer>>();
		sleepers.Clear();
		Enumerator<BasePlayer> enumerator = BasePlayer.sleepingPlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (current.IsValid() && !current.IsNpc && !current.IsBot)
				{
					sleepers.Add(current.userID, current);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		HashSet<ulong> found = Pool.Get<HashSet<ulong>>();
		found.Clear();
		foreach (BasePlayer player in sleepers.Values)
		{
			if (!player.IsValid() || player.IsConnected || found.Contains(player.userID))
			{
				continue;
			}
			try
			{
				ClanValueResult<IClan> val = await Backend.GetByMember((ulong)player.userID);
				if (val.IsSuccess)
				{
					IClan value = val.Value;
					player.serverClan = value;
					player.clanId = value.ClanId;
					SendNetworkUpdate();
					found.Add(player.userID);
					foreach (ClanMember member in value.Members)
					{
						if (sleepers.TryGetValue(member.SteamId, out var value2) && found.Add(member.SteamId))
						{
							value2.serverClan = value;
							value2.clanId = value.ClanId;
							value2.SendNetworkUpdate();
						}
					}
				}
				else if ((int)val.Result == 3)
				{
					player.serverClan = null;
					player.clanId = 0L;
					SendNetworkUpdate();
					found.Add(player.userID);
				}
				else
				{
					Debug.LogError((object)$"Failed to find clan for {player.userID.Get()}: {val.Result}");
					Invoke(delegate
					{
						player.LoadClanInfo();
					}, 45 + Random.Range(0, 30));
				}
			}
			catch (Exception ex)
			{
				DebugEx.Log($"Exception was thrown while loading clan info for {player.userID.Get()}:", (StackTraceLogType)0);
				Debug.LogException(ex);
			}
		}
		found.Clear();
		Pool.FreeUnmanaged<ulong>(ref found);
		sleepers.Clear();
		Pool.FreeUnmanaged<ulong, BasePlayer>(ref sleepers);
	}

	public async Task Initialize()
	{
		if (string.IsNullOrWhiteSpace(_backendType))
		{
			throw new InvalidOperationException("Clan backend type has not been assigned!");
		}
		IClanBackend backend = CreateBackendInstance(_backendType);
		if (backend == null)
		{
			throw new InvalidOperationException("Clan backend failed to create (returned null)");
		}
		try
		{
			_changeTracker = new ClanChangeTracker(this);
			await backend.Initialize((IClanChangeSink)(object)_changeTracker);
			Backend = backend;
			InvokeRandomized(delegate
			{
				_changeTracker.HandleEvents();
			}, 1f, 0.25f, 0.1f);
		}
		catch (Exception innerException)
		{
			throw new InvalidOperationException("Clan backend failed to initialize (threw exception)", innerException);
		}
	}

	public void Shutdown()
	{
		if (Backend == null)
		{
			return;
		}
		try
		{
			((IDisposable)Backend).Dispose();
			Backend = null;
		}
		catch (Exception innerException)
		{
			throw new InvalidOperationException("Clan backend failed to shutdown (threw exception)", innerException);
		}
	}

	public override void Spawn()
	{
		base.Spawn();
		if (!base.isServer)
		{
			return;
		}
		if (Application.isLoadingSave)
		{
			if (!Clan.enabled)
			{
				Debug.LogWarning((object)"Clan manager was loaded from a save, but the server has the clan system disabled - destroying clan manager!");
				Invoke(delegate
				{
					Kill();
				}, 0.1f);
			}
		}
		else if (!Application.isLoadingSave)
		{
			_backendType = ChooseBackendType();
			if (string.IsNullOrWhiteSpace(_backendType))
			{
				Debug.LogError((object)"Clan manager did not choose a backend type!");
			}
			else
			{
				Debug.Log((object)("Clan manager will use backend type: " + _backendType));
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.clanManager = Pool.Get<ClanManager>();
			info.msg.clanManager.backendType = _backendType;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.clanManager != null)
		{
			_backendType = info.msg.clanManager.backendType;
		}
	}

	private static string ChooseBackendType()
	{
		if (NexusServer.Started)
		{
			return "nexus";
		}
		return "local";
	}

	private static IClanBackend CreateBackendInstance(string type)
	{
		if (!(type == "local"))
		{
			if (type == "nexus")
			{
				return (IClanBackend)(object)new NexusClanBackend();
			}
			throw new NotSupportedException("Clan backend '" + type + "' is not supported");
		}
		return (IClanBackend)(object)new LocalClanBackend(ConVar.Server.rootFolder, 286, Clan.maxMemberCount);
	}

	public override void InitShared()
	{
		base.InitShared();
		if (base.isServer)
		{
			if ((Object)(object)ServerInstance != (Object)null)
			{
				Debug.LogError((object)"Major fuckup! Server ClanManager spawned twice, contact Developers!");
				Object.Destroy((Object)(object)((Component)this).gameObject);
			}
			else
			{
				ServerInstance = this;
			}
		}
	}

	public void OnDestroy()
	{
		if (base.isServer)
		{
			if ((Object)(object)ServerInstance == (Object)(object)this)
			{
				ServerInstance = null;
			}
			Shutdown();
		}
	}
}

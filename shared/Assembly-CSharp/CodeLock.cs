using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class CodeLock : BaseLock, IReskinCallback
{
	public struct CodeLockPreserveInfo
	{
		public string code;

		public string guestCode;

		public bool isLocked;

		public List<ulong> whitelistPlayers;

		public List<ulong> guestPlayers;
	}

	public GameObjectRef keyEnterDialog;

	public GameObjectRef effectUnlocked;

	public GameObjectRef effectLocked;

	public GameObjectRef effectDenied;

	public GameObjectRef effectCodeChanged;

	public GameObjectRef effectShock;

	public bool hasCode;

	public const Flags Flag_CodeEntryBlocked = Flags.Reserved11;

	public static readonly Phrase blockwarning;

	[ServerVar(Help = "(Generated) Maximum number of failed code entry attempts on a code lock before the player is locked out; default 8")]
	public static float maxFailedAttempts;

	[ServerVar(Help = "(Generated) Duration in seconds a player is locked out from attempting the code lock after exceeding maxFailedAttempts; default 900s (15 minutes)")]
	public static float lockoutCooldown;

	public bool hasGuestCode;

	[NonSerialized]
	public string code = string.Empty;

	[NonSerialized]
	public string guestCode = string.Empty;

	[NonSerialized]
	public List<ulong> whitelistPlayers = new List<ulong>();

	[NonSerialized]
	public List<ulong> guestPlayers = new List<ulong>();

	public int wrongCodes;

	public float lastWrongTime = float.NegativeInfinity;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CodeLock.OnRpcMessage"))
		{
			if (rpc == 4013784361u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ChangeCode"));
				}
				using (TimeWarning.New("RPC_ChangeCode"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4013784361u, "RPC_ChangeCode", this, player, 3f, checkParent: true))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_ChangeCode(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_ChangeCode");
					}
				}
				return true;
			}
			if (rpc == 2626067433u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - TryLock"));
				}
				using (TimeWarning.New("TryLock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2626067433u, "TryLock", this, player, 3f, checkParent: true))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							TryLock(rpc3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in TryLock");
					}
				}
				return true;
			}
			if (rpc == 1718262 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - TryUnlock"));
				}
				using (TimeWarning.New("TryUnlock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1718262u, "TryUnlock", this, player, 3f, checkParent: true))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							TryUnlock(rpc4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in TryUnlock");
					}
				}
				return true;
			}
			if (rpc == 418605506 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - UnlockWithCode"));
				}
				using (TimeWarning.New("UnlockWithCode"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(418605506u, "UnlockWithCode", this, player, 3f, checkParent: true))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							UnlockWithCode(rpc5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in UnlockWithCode");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsCodeEntryBlocked()
	{
		return HasFlag(Flags.Reserved11);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.codeLock != null)
		{
			hasCode = info.msg.codeLock.hasCode;
			hasGuestCode = info.msg.codeLock.hasGuestCode;
			LoadCodelockPrivateData(info.msg.codeLock);
		}
	}

	public void LoadCodelockPrivateData(CodeLock proto)
	{
		if (proto?.pv != null)
		{
			code = proto.pv.code;
			whitelistPlayers = List.ShallowClonePooled<ulong>(proto.pv.users);
			guestCode = proto.pv.guestCode;
			guestPlayers = List.ShallowClonePooled<ulong>(proto.pv.guestUsers);
		}
	}

	internal void DoEffect(string effect)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Effect.server.Run(effect, this, 0u, Vector3.zero, Vector3.forward);
	}

	public override bool GetPlayerLockPermission(BasePlayer player)
	{
		if (!IsLocked())
		{
			return true;
		}
		if (whitelistPlayers.Contains(player.userID) || guestPlayers.Contains(player.userID))
		{
			return true;
		}
		return false;
	}

	public override bool OnTryToOpen(BasePlayer player)
	{
		object obj = Interface.CallHook("CanUseLockedEntity", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (!IsLocked())
		{
			return true;
		}
		if (whitelistPlayers.Contains(player.userID) || guestPlayers.Contains(player.userID))
		{
			DoEffect(effectUnlocked.resourcePath);
			return true;
		}
		DoEffect(effectDenied.resourcePath);
		return false;
	}

	public override bool OnTryToClose(BasePlayer player)
	{
		object obj = Interface.CallHook("CanUseLockedEntity", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (!IsLocked())
		{
			return true;
		}
		if (whitelistPlayers.Contains(player.userID) || guestPlayers.Contains(player.userID))
		{
			DoEffect(effectUnlocked.resourcePath);
			return true;
		}
		DoEffect(effectDenied.resourcePath);
		return false;
	}

	public override bool CanUseNetworkCache(Connection connection)
	{
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.codeLock = Pool.Get<CodeLock>();
		info.msg.codeLock.hasGuestCode = guestCode.Length > 0;
		info.msg.codeLock.hasCode = code.Length > 0;
		if (!info.forDisk && info.forConnection != null)
		{
			info.msg.codeLock.hasAuth = whitelistPlayers.Contains(info.forConnection.userid);
			info.msg.codeLock.hasGuestAuth = guestPlayers.Contains(info.forConnection.userid);
		}
		if (info.forDisk)
		{
			info.msg.codeLock.pv = Pool.Get<Private>();
			info.msg.codeLock.pv.code = code;
			info.msg.codeLock.pv.users = List.ShallowClonePooled<ulong>(whitelistPlayers);
			info.msg.codeLock.pv.guestCode = guestCode;
			info.msg.codeLock.pv.guestUsers = List.ShallowClonePooled<ulong>(guestPlayers);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f, CheckParent = true)]
	private void RPC_ChangeCode(RPCMessage rpc)
	{
		if (!rpc.player.CanInteract())
		{
			return;
		}
		string text = rpc.read.String();
		bool flag = rpc.read.Bit();
		if (IsLocked() || text.Length != 4 || !StringEx.IsNumeric(text) || (!hasCode & flag) || Interface.CallHook("CanChangeCode", rpc.player, this, text, flag) != null)
		{
			return;
		}
		if (!hasCode && !flag)
		{
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Locked, b: true);
		}
		Facepunch.Rust.Analytics.Azure.OnCodelockChanged(rpc.player, this, flag ? guestCode : code, text, flag);
		if (!flag)
		{
			code = text;
			hasCode = code.Length > 0;
			whitelistPlayers.Clear();
			whitelistPlayers.Add(rpc.player.userID);
		}
		else
		{
			guestCode = text;
			hasGuestCode = guestCode.Length > 0;
			guestPlayers.Clear();
			guestPlayers.Add(rpc.player.userID);
		}
		Interface.CallHook("OnCodeChanged", rpc.player, this, text, flag);
		DoEffect(effectCodeChanged.resourcePath);
		SendNetworkUpdate();
		UpdateParentBox();
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f, CheckParent = true)]
	private void TryUnlock(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && IsLocked() && Interface.CallHook("CanUnlock", rpc.player, this) == null && !IsCodeEntryBlocked() && whitelistPlayers.Contains(rpc.player.userID))
		{
			DoEffect(effectUnlocked.resourcePath);
			SetFlagLocal(Flags.Locked, b: false);
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f, CheckParent = true)]
	private void TryLock(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && !IsLocked() && code.Length == 4 && Interface.CallHook("CanLock", rpc.player, this) == null && whitelistPlayers.Contains(rpc.player.userID))
		{
			DoEffect(effectLocked.resourcePath);
			SetFlagLocal(Flags.Locked, b: true);
			SendNetworkUpdate();
		}
	}

	public void ClearCodeEntryBlocked()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved11, b: false);
		}
		wrongCodes = 0;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f, CheckParent = true)]
	private void UnlockWithCode(RPCMessage rpc)
	{
		if (!rpc.player.CanInteract() || !IsLocked() || IsCodeEntryBlocked())
		{
			return;
		}
		string text = rpc.read.String();
		if (Interface.CallHook("OnCodeEntered", this, rpc.player, text) != null)
		{
			return;
		}
		bool flag = text == guestCode;
		bool flag2 = text == code;
		if (!(text == code) && (!hasGuestCode || !(text == guestCode)))
		{
			if (Time.realtimeSinceStartup > lastWrongTime + 60f)
			{
				wrongCodes = 0;
			}
			DoEffect(effectDenied.resourcePath);
			DoEffect(effectShock.resourcePath);
			rpc.player.Hurt((float)(wrongCodes + 1) * 5f, DamageType.ElectricShock, this, useProtection: false);
			wrongCodes++;
			if (wrongCodes > 5)
			{
				rpc.player.ShowToast(GameTip.Styles.Red_Normal, blockwarning, false);
			}
			if ((float)wrongCodes >= maxFailedAttempts)
			{
				using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
				{
					flagsUpdateScope.Set(Flags.Reserved11, b: true);
				}
				Invoke(ClearCodeEntryBlocked, lockoutCooldown);
			}
			lastWrongTime = Time.realtimeSinceStartup;
			return;
		}
		SendNetworkUpdate();
		if (flag2)
		{
			if (!whitelistPlayers.Contains(rpc.player.userID))
			{
				DoEffect(effectCodeChanged.resourcePath);
				whitelistPlayers.Add(rpc.player.userID);
				wrongCodes = 0;
				UpdateParentBox();
			}
			Facepunch.Rust.Analytics.Azure.OnCodeLockEntered(rpc.player, this, isGuest: false);
		}
		else if (flag && !guestPlayers.Contains(rpc.player.userID))
		{
			DoEffect(effectCodeChanged.resourcePath);
			guestPlayers.Add(rpc.player.userID);
			UpdateParentBox();
			Facepunch.Rust.Analytics.Azure.OnCodeLockEntered(rpc.player, this, isGuest: true);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved11, b: false);
	}

	private void UpdateParentBox()
	{
		BaseEntity baseEntity = GetParentEntity();
		if ((Object)(object)baseEntity != (Object)null && baseEntity is DisplayingBoxStorage)
		{
			baseEntity.SendNetworkUpdate();
		}
	}

	void IReskinCallback.OnReskinned(BasePlayer byPlayer)
	{
		hasCode = code.Length > 0;
		hasGuestCode = guestCode.Length > 0;
	}

	public override void Reskin_Preserve(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		base.Reskin_Preserve(ref preserveInfo);
		ref CodeLockPreserveInfo codeLockPreserve = ref preserveInfo.codeLockPreserve;
		codeLockPreserve.code = code;
		codeLockPreserve.guestCode = guestCode;
		codeLockPreserve.isLocked = IsLocked();
		codeLockPreserve.whitelistPlayers = Pool.Get<List<ulong>>();
		codeLockPreserve.guestPlayers = Pool.Get<List<ulong>>();
		codeLockPreserve.whitelistPlayers.AddRange(whitelistPlayers);
		codeLockPreserve.guestPlayers.AddRange(guestPlayers);
	}

	public override void Reskin_Restore(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		base.Reskin_Restore(ref preserveInfo);
		ref CodeLockPreserveInfo codeLockPreserve = ref preserveInfo.codeLockPreserve;
		BaseEntity parent = preserveInfo.baseEntityPreserve.parent;
		parent.SetSlot(Slot.Lock, this);
		SetParent(parent, parent.GetSlotAnchorName(Slot.Lock));
		code = codeLockPreserve.code;
		guestCode = codeLockPreserve.guestCode;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Locked, codeLockPreserve.isLocked);
		}
		whitelistPlayers.AddRange(codeLockPreserve.whitelistPlayers);
		guestPlayers.AddRange(codeLockPreserve.guestPlayers);
		Pool.FreeUnmanaged<ulong>(ref codeLockPreserve.whitelistPlayers);
		Pool.FreeUnmanaged<ulong>(ref codeLockPreserve.guestPlayers);
	}

	static CodeLock()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		blockwarning = new Phrase("codelock.blockwarning", "Further failed attempts will block code entry for some time");
		maxFailedAttempts = 8f;
		lockoutCooldown = 900f;
	}
}

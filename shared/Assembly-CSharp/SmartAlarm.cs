using System;
using System.Collections.Generic;
using CompanionServer;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class SmartAlarm : AppIOEntity, ISubscribable
{
	public const Flags Flag_HasCustomMessage = Flags.Reserved6;

	public static readonly Phrase DefaultNotificationTitle = new Phrase("app.alarm.title", "Alarm");

	public static readonly Phrase DefaultNotificationBody = new Phrase("app.alarm.body", "Your base is under attack!");

	[Header("Smart Alarm")]
	public GameObjectRef SetupNotificationDialog;

	public Animator Animator;

	public string _notificationTitle = "";

	public string _notificationBody = "";

	public float _lastSentTime;

	private readonly HashSet<ulong> _mutedPlayers = new HashSet<ulong>();

	public override AppEntityType Type => (AppEntityType)2;

	public override bool Value { get; set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SmartAlarm.OnRpcMessage"))
		{
			if (rpc == 3292290572u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SetNotificationTextImpl"));
				}
				using (TimeWarning.New("SetNotificationTextImpl"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3292290572u, "SetNotificationTextImpl", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3292290572u, "SetNotificationTextImpl", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage notificationTextImpl = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SetNotificationTextImpl(notificationTextImpl);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SetNotificationTextImpl");
					}
				}
				return true;
			}
			if (rpc == 4207149767u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - StartSetupNotification"));
				}
				using (TimeWarning.New("StartSetupNotification"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4207149767u, "StartSetupNotification", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(4207149767u, "StartSetupNotification", this, player, 3f))
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
							StartSetupNotification(rpc2);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in StartSetupNotification");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool AddSubscription(ulong steamId)
	{
		if (!IsPairedWith(steamId))
		{
			return false;
		}
		_mutedPlayers.Remove(steamId);
		return true;
	}

	public bool RemoveSubscription(ulong steamId)
	{
		if (IsPairedWith(steamId))
		{
			return _mutedPlayers.Add(steamId);
		}
		return false;
	}

	public bool HasSubscription(ulong steamId)
	{
		if (IsPairedWith(steamId))
		{
			return !_mutedPlayers.Contains(steamId);
		}
		return false;
	}

	public override void InitShared()
	{
		base.InitShared();
		_notificationTitle = DefaultNotificationTitle.translated;
		_notificationBody = DefaultNotificationBody.translated;
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		Value = inputAmount > 0;
		if (Value == IsOn())
		{
			return;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, Value);
		}
		BroadcastValueChange();
		float num = Mathf.Max(App.alarmcooldown, 15f);
		if (Value && Time.realtimeSinceStartup - _lastSentTime >= num)
		{
			BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege();
			if ((Object)(object)buildingPrivilege != (Object)null)
			{
				_pairedPlayers.IntersectWith(buildingPrivilege.authorizedPlayers);
				_mutedPlayers.IntersectWith(buildingPrivilege.authorizedPlayers);
			}
			_pairedPlayers.SendNotification(NotificationChannel.SmartAlarm, _notificationTitle, _notificationBody, "alarm", _mutedPlayers);
			_lastSentTime = Time.realtimeSinceStartup;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.smartAlarm = Pool.Get<SmartAlarm>();
			info.msg.smartAlarm.notificationTitle = _notificationTitle;
			info.msg.smartAlarm.notificationBody = _notificationBody;
			info.msg.smartAlarm.mutedPlayers = Pool.Get<List<ulong>>();
			info.msg.smartAlarm.mutedPlayers.AddRange(_mutedPlayers);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk && info.msg.smartAlarm != null)
		{
			_notificationTitle = info.msg.smartAlarm.notificationTitle;
			_notificationBody = info.msg.smartAlarm.notificationBody;
			_mutedPlayers.Clear();
			if (info.msg.smartAlarm.mutedPlayers != null)
			{
				_mutedPlayers.UnionWith(info.msg.smartAlarm.mutedPlayers);
			}
		}
	}

	protected override void OnPairedWithPlayer(BasePlayer player)
	{
		base.OnPairedWithPlayer(player);
		if ((Object)(object)player != (Object)null)
		{
			_mutedPlayers.Remove(player.userID);
		}
	}

	protected override void OnPairedPlayersCleared()
	{
		_mutedPlayers.Clear();
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	private void StartSetupNotification(RPCMessage rpc)
	{
		if (rpc.player.CanInteract())
		{
			BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege();
			if (!((Object)(object)buildingPrivilege != (Object)null) || buildingPrivilege.CanAdministrate(rpc.player))
			{
				ClientRPC(RpcTarget.Player("SetupNotification", rpc.player), _notificationTitle, _notificationBody);
			}
		}
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void SetNotificationTextImpl(RPCMessage rpc)
	{
		if (!rpc.player.CanInteract())
		{
			return;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege();
		if ((Object)(object)buildingPrivilege != (Object)null && !buildingPrivilege.CanAdministrate(rpc.player))
		{
			return;
		}
		string text = rpc.read.String(128);
		string text2 = rpc.read.String(512);
		if (!string.IsNullOrWhiteSpace(text))
		{
			_notificationTitle = text;
		}
		if (!string.IsNullOrWhiteSpace(text2))
		{
			_notificationBody = text2;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved6, b: true);
	}

	static SmartAlarm()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
	}
}

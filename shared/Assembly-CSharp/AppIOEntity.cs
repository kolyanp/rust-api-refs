using System;
using System.Collections.Generic;
using System.Globalization;
using CompanionServer;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class AppIOEntity : IOEntity
{
	protected readonly NotificationList _pairedPlayers = new NotificationList();

	private float _cacheTime;

	private BuildingPrivlidge _cache;

	public abstract AppEntityType Type { get; }

	public virtual bool Value
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("AppIOEntity.OnRpcMessage"))
		{
			if (rpc == 3704248322u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ClearPairedPlayers"));
				}
				using (TimeWarning.New("ClearPairedPlayers"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3704248322u, "ClearPairedPlayers", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3704248322u, "ClearPairedPlayers", this, player, 3f))
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
							ClearPairedPlayers(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in ClearPairedPlayers");
					}
				}
				return true;
			}
			if (rpc == 3018927126u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - PairWithApp"));
				}
				using (TimeWarning.New("PairWithApp"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3018927126u, "PairWithApp", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3018927126u, "PairWithApp", this, player, 3f))
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
							PairWithApp(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in PairWithApp");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsPairedWith(ulong steamId)
	{
		return _pairedPlayers.HasSubscription(steamId);
	}

	public bool CanAppAccess(ulong steamId)
	{
		if (!IsPairedWith(steamId))
		{
			return false;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege();
		if (!((Object)(object)buildingPrivilege == (Object)null))
		{
			return buildingPrivilege.IsAuthed(steamId);
		}
		return true;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.appIOEntity = Pool.Get<AppIOEntity>();
			info.msg.appIOEntity.pairedPlayers = _pairedPlayers.ToList();
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk && info.msg.appIOEntity != null)
		{
			_pairedPlayers.LoadFrom(info.msg.appIOEntity.pairedPlayers);
		}
	}

	protected void BroadcastValueChange()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!this.IsValid())
		{
			return;
		}
		AppBroadcast val = Pool.Get<AppBroadcast>();
		try
		{
			val.entityChanged = Pool.Get<AppEntityChanged>();
			val.entityChanged.entityId = net.ID;
			val.entityChanged.payload = Pool.Get<AppEntityPayload>();
			FillEntityPayload(val.entityChanged.payload);
			CompanionServer.Server.Broadcast(this, val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	internal virtual void FillEntityPayload(AppEntityPayload payload)
	{
		payload.value = Value;
	}

	public override BuildingPrivlidge GetBuildingPrivilege()
	{
		if (Time.realtimeSinceStartup - _cacheTime > 5f)
		{
			_cache = base.GetBuildingPrivilege();
			_cacheTime = Time.realtimeSinceStartup;
		}
		return _cache;
	}

	public EntityTarget GetTarget()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return new EntityTarget(net.ID);
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.IsVisible(3f)]
	public async void PairWithApp(RPCMessage msg)
	{
		try
		{
			BasePlayer player = msg.player;
			Dictionary<string, string> dictionary = CompanionServer.Util.TryGetPlayerPairingData(player);
			if (dictionary == null)
			{
				player.ClientRPC(RpcTarget.Player("HandleCompanionPairingResult", player), 3);
				return;
			}
			dictionary.Add("entityId", net.ID.Value.ToString("G", CultureInfo.InvariantCulture));
			dictionary.Add("entityType", ((int)Type).ToString("G", CultureInfo.InvariantCulture));
			dictionary.Add("entityName", GetDisplayName().translated);
			NotificationSendResult notificationSendResult = await CompanionServer.Util.SendPairNotification("entity", player, GetDisplayName().translated, "Tap to pair with this device.", dictionary);
			if (notificationSendResult == NotificationSendResult.Sent)
			{
				OnPairedWithPlayer(msg.player);
			}
			else
			{
				player.ClientRPC(RpcTarget.Player("HandleCompanionPairingResult", player), (int)notificationSendResult);
			}
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
	}

	protected virtual void OnPairedWithPlayer(BasePlayer player)
	{
		if (!((Object)(object)player == (Object)null) && !IsPairedWith(player.userID) && !_pairedPlayers.AddSubscription(player.userID))
		{
			player.ClientRPC(RpcTarget.Player("HandleCompanionPairingResult", player), 7);
		}
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void ClearPairedPlayers(RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null) && msg.player.CanBuild())
		{
			_pairedPlayers.Clear();
			OnPairedPlayersCleared();
		}
	}

	protected virtual void OnPairedPlayersCleared()
	{
	}
}

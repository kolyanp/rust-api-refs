using System;
using System.Collections.Generic;
using Facepunch;
using Fleck;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer;

public class Connection : IConnection
{
	private readonly Listener _listener;

	private readonly BackhaulConnection _backhaul;

	private readonly uint _channelId;

	private PlayerTarget? _subscribedPlayer;

	private readonly HashSet<EntityTarget> _subscribedEntities;

	private readonly HashSet<ClanTarget> _subscribedClans;

	private IRemoteControllable _currentCamera;

	private ulong _cameraViewerSteamId;

	private bool _isControllingCamera;

	public long ConnectionId { get; private set; }

	public bool IsClosed { get; private set; }

	public ulong SubscribedSteamId => _subscribedPlayer?.SteamId ?? 0;

	public ulong SteamId { get; }

	public IRemoteControllable CurrentCamera
	{
		get
		{
			if (!IsCameraAlive)
			{
				return null;
			}
			return _currentCamera;
		}
	}

	public bool IsControllingCamera
	{
		get
		{
			if (IsCameraAlive)
			{
				return _isControllingCamera;
			}
			return false;
		}
	}

	public ulong ControllingSteamId
	{
		get
		{
			if (!IsCameraAlive)
			{
				return 0uL;
			}
			return _cameraViewerSteamId;
		}
	}

	private bool IsCameraAlive
	{
		get
		{
			CameraTarget target;
			return TryGetCameraTarget(_currentCamera, out target);
		}
	}

	public InputState InputState { get; set; }

	public Connection(long connectionId, Listener listener, BackhaulConnection backhaul, uint channelId, ulong steamId)
	{
		ConnectionId = connectionId;
		_listener = listener;
		_backhaul = backhaul;
		_channelId = channelId;
		SteamId = steamId;
		_subscribedEntities = new HashSet<EntityTarget>();
		_subscribedClans = new HashSet<ClanTarget>();
	}

	public void OnClose()
	{
		IsClosed = true;
		if (_subscribedPlayer.HasValue)
		{
			_listener.PlayerSubscribers.Remove(_subscribedPlayer.Value, this);
			_subscribedPlayer = null;
		}
		foreach (EntityTarget subscribedEntity in _subscribedEntities)
		{
			_listener.EntitySubscribers.Remove(subscribedEntity, this);
		}
		_subscribedEntities.Clear();
		foreach (ClanTarget subscribedClan in _subscribedClans)
		{
			_listener.ClanSubscribers.Remove(subscribedClan, this);
		}
		_subscribedClans.Clear();
		EndViewing();
	}

	public void Close()
	{
		_backhaul.CloseChannelLocal(_channelId);
	}

	public void Send(AppResponse response)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		if (IsClosed || !_backhaul.IsAvailable)
		{
			response.ResetToPool();
			return;
		}
		BufferStream val = Pool.Get<BufferStream>().Initialize();
		try
		{
			AppMessage val2 = Pool.Get<AppMessage>();
			try
			{
				val2.response = response;
				val2.WriteToStream(val);
				if (val.Length > 2097147)
				{
					Debug.LogError((object)$"Rust+ response is too large to send ({val.Length} bytes), replying with an error instead");
					Listener.SendError(this, response.seq, "server_error");
				}
				else
				{
					MemoryBuffer val3 = new MemoryBuffer(val.Length);
					val.GetBuffer().CopyTo(val3.Data, 0);
					SendRaw(val3.Slice(val.Length));
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void Subscribe(PlayerTarget target)
	{
		if (!IsClosed && !(_subscribedPlayer == target))
		{
			EndViewing();
			if (_subscribedPlayer.HasValue)
			{
				_listener.PlayerSubscribers.Remove(_subscribedPlayer.Value, this);
				_subscribedPlayer = null;
			}
			_listener.PlayerSubscribers.Add(target, this);
			_subscribedPlayer = target;
		}
	}

	public void Subscribe(EntityTarget target)
	{
		if (!IsClosed && _subscribedEntities.Add(target))
		{
			_listener.EntitySubscribers.Add(target, this);
		}
	}

	public bool BeginViewing(IRemoteControllable camera)
	{
		if (IsClosed || !_subscribedPlayer.HasValue)
		{
			return false;
		}
		if (!TryGetCameraTarget(camera, out var target))
		{
			return false;
		}
		if (_currentCamera == camera)
		{
			_listener.CameraSubscribers.Add(target, this);
			return true;
		}
		EndViewing();
		ulong steamId = _subscribedPlayer.Value.SteamId;
		if (!camera.CanControl(steamId))
		{
			return false;
		}
		_listener.CameraSubscribers.Add(target, this);
		_currentCamera = camera;
		_isControllingCamera = _currentCamera.InitializeControl(new CameraViewerId(steamId, ConnectionId));
		_cameraViewerSteamId = steamId;
		InputState?.Clear();
		return true;
	}

	public void EndViewing()
	{
		if (TryGetCameraTarget(_currentCamera, out var target))
		{
			_listener.CameraSubscribers.Remove(target, this);
			_currentCamera.StopControl(new CameraViewerId(_cameraViewerSteamId, ConnectionId));
		}
		_currentCamera = null;
		_isControllingCamera = false;
		_cameraViewerSteamId = 0uL;
	}

	public void Subscribe(ClanTarget target)
	{
		if (IsClosed || _subscribedClans.Contains(target))
		{
			return;
		}
		foreach (ClanTarget subscribedClan in _subscribedClans)
		{
			_listener.ClanSubscribers.Remove(subscribedClan, this);
		}
		_subscribedClans.Clear();
		if (_subscribedClans.Add(target))
		{
			_listener.ClanSubscribers.Add(target, this);
		}
	}

	public void Unsubscribe(ClanTarget target)
	{
		if (_subscribedClans.Remove(target))
		{
			_listener.ClanSubscribers.Remove(target, this);
		}
	}

	public void SendRaw(MemoryBuffer data)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (data.Length == 0)
		{
			return;
		}
		if (IsClosed || !_backhaul.IsAvailable)
		{
			data.Dispose();
			return;
		}
		try
		{
			_backhaul.SendData(_channelId, data);
		}
		catch (Exception arg)
		{
			Debug.LogError((object)$"Failed to send message to app client (connectionID={ConnectionId} steamID={SteamId}): {arg}");
		}
	}

	private static bool TryGetCameraTarget(IRemoteControllable camera, out CameraTarget target)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = (ObjectEx.IsUnityNull(camera) ? null : camera.GetEnt());
		if (!baseEntity.IsValid())
		{
			target = default;
			return false;
		}
		target = new CameraTarget(baseEntity.net.ID);
		return true;
	}
}

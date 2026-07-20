using System;
using Facepunch;
using Network;
using ProtoBuf;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

public class PlayerStateManager
{
	private readonly MruDictionary<ulong, PlayerState> _cache;

	private readonly UserPersistance _persistence;

	public int CacheSize
	{
		get
		{
			return _cache.Capacity;
		}
		set
		{
			_cache.SetCapacity(value);
		}
	}

	public int CacheCount => _cache.Count;

	public long CacheEvictions => _cache.EvictionCount;

	public PlayerStateManager(UserPersistance persistence)
	{
		_cache = new MruDictionary<ulong, PlayerState>(10000, (Action<ulong, PlayerState>)FreeOldState);
		_persistence = persistence;
	}

	public PlayerState Get(ulong playerId)
	{
		using (TimeWarning.New("PlayerStateManager.Get"))
		{
			PlayerState result = default(PlayerState);
			if (_cache.TryGetValue(playerId, ref result))
			{
				return result;
			}
			byte[] playerState = _persistence.GetPlayerState(playerId);
			PlayerState val;
			if (playerState != null && playerState.Length != 0)
			{
				try
				{
					val = PlayerState.Deserialize(playerState);
					OnPlayerStateLoaded(val);
					_cache.Add(playerId, val);
					return val;
				}
				catch (Exception arg)
				{
					Debug.LogError((object)$"Failed to load player state for {playerId}: {arg}");
				}
			}
			val = Pool.Get<PlayerState>();
			_cache.Add(playerId, val);
			return val;
		}
	}

	public void Save(ulong playerId)
	{
		PlayerState state = default(PlayerState);
		if (_cache.TryGetValue(playerId, ref state))
		{
			SaveState(playerId, state);
		}
	}

	public void SaveState(ulong playerId, PlayerState state)
	{
		using (TimeWarning.New("PlayerStateManager.SaveState"))
		{
			try
			{
				byte[] state2 = ProtoStreamExtensions.ToProtoBytes((IProto)(object)state);
				_persistence.SetPlayerState(playerId, state2);
			}
			catch (Exception arg)
			{
				Debug.LogError((object)$"Failed to save player state for {playerId}: {arg}");
			}
		}
	}

	private void FreeOldState(ulong playerId, PlayerState state)
	{
		SaveState(playerId, state);
		state.Dispose();
	}

	public void Reset(ulong playerId)
	{
		_cache.Remove(playerId);
		_persistence.ResetPlayerState(playerId);
	}

	private void OnPlayerStateLoaded(PlayerState state)
	{
		state.unHostileTimestamp = Math.Min(state.unHostileTimestamp, TimeEx.currentTimestamp + 1800.0);
	}
}

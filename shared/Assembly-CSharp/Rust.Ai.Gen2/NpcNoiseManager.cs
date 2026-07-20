using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Facepunch;
using Spatial;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class NpcNoiseManager : SingletonComponent<NpcNoiseManager>, IServerComponent
{
	private const float voiceChatEventMaxAge = 1f;

	private const float noiseMaxAge = 10f;

	private const float worldSize = 8096f;

	private const int cellSize = 32;

	private ConcurrentDictionary<BasePlayer, double> recentVoiceChatEvents = new ConcurrentDictionary<BasePlayer, double>();

	private Grid<NpcNoiseEvent> noiseGrid = new Grid<NpcNoiseEvent>(32, 8096f);

	private Queue<NpcNoiseEvent> noises = new Queue<NpcNoiseEvent>();

	private double nextTickTime;

	private int nextNoiseId = 1;

	private Action _removeOldNoisesCallback;

	public void AddNoise(BaseEntity initiator, Vector3 position, NpcNoiseIntensity intensity, bool guessInitiatorPosition = false)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (_removeOldNoisesCallback == null)
		{
			_removeOldNoisesCallback = RemoveOldNoises;
		}
		if (!IsInvoking(_removeOldNoisesCallback))
		{
			InvokeRepeating(_removeOldNoisesCallback, 0f, 0f);
		}
		NpcNoiseEvent npcNoiseEvent = new NpcNoiseEvent(nextNoiseId++, initiator, position, guessInitiatorPosition ? ((Component)initiator).transform.position : position, intensity, Time.timeAsDouble);
		noiseGrid.Add(npcNoiseEvent, npcNoiseEvent.NoisePosition.x, npcNoiseEvent.NoisePosition.z);
		noises.Enqueue(npcNoiseEvent);
	}

	private void RemoveOldNoises()
	{
		using (TimeWarning.New("RemoveOldNoises"))
		{
			while (noises.Count > 0)
			{
				NpcNoiseEvent npcNoiseEvent = noises.Peek();
				if (Time.timeAsDouble - npcNoiseEvent.EventTime <= 10.0)
				{
					break;
				}
				noises.Dequeue();
				noiseGrid.Remove(npcNoiseEvent);
			}
		}
	}

	public void GetNoisesAround(Vector3 position, float range, List<NpcNoiseEvent> results)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (noiseGrid != null)
		{
			noiseGrid.Query(position.x, position.z, range, results);
		}
	}

	public void OnServerProjectileHit(BaseEntity entity, ServerProjectile projectile, RaycastHit hit)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		AddNoise(entity, ((Component)projectile).transform.position, NpcNoiseIntensity.High);
	}

	public void OnProjectileHit(BaseEntity entity, HitInfo hit)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("NpcNoiseManager.OnProjectileHit"))
		{
			if (BaseNetworkableEx.Is<BaseProjectile>((Object)(object)hit.Weapon, out BaseProjectile _) || BaseNetworkableEx.Is<BaseMelee>((Object)(object)hit.Weapon, out BaseMelee _))
			{
				if (BaseNetworkableEx.Is<BaseProjectile>((Object)(object)hit.Weapon, out BaseProjectile castedUnityObject3) && !BaseNetworkableEx.Is<BowWeapon>((Object)(object)hit.Weapon, out BowWeapon _))
				{
					AddNoise(entity, hit.HitPositionWorld, castedUnityObject3.IsSilenced() ? NpcNoiseIntensity.Medium : NpcNoiseIntensity.High, guessInitiatorPosition: true);
				}
				else
				{
					AddNoise(entity, hit.HitPositionWorld, NpcNoiseIntensity.Low);
				}
			}
		}
	}

	public void OnWeaponShot(BasePlayer player, BaseProjectile weapon)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("NpcNoiseManager.OnWeaponShot"))
		{
			NpcNoiseIntensity intensity = NpcNoiseIntensity.High;
			if (BaseNetworkableEx.Is<BowWeapon>((Object)(object)weapon, out BowWeapon _))
			{
				intensity = NpcNoiseIntensity.Low;
			}
			else if ((Object)(object)weapon != (Object)null && weapon.IsSilenced())
			{
				intensity = NpcNoiseIntensity.Medium;
			}
			AddNoise(player, ((Component)player).transform.position, intensity);
		}
	}

	public void OnNpcWeaponShot(BaseEntity npc, BaseEntity target, Vector3 impactLocation)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("NpcNoiseManager.OnNpcWeaponShot"))
		{
			AddNoise(target, impactLocation, NpcNoiseIntensity.High, guessInitiatorPosition: true);
		}
	}

	public void OnWeaponThrown(BasePlayer player, BaseMelee weapon, bool canAiHearIt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (canAiHearIt)
		{
			AddNoise(player, ((Component)player).transform.position, NpcNoiseIntensity.Low);
		}
	}

	public void OnExplosion(BaseEntity creatorEntity, TimedExplosive explosive)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (creatorEntity.IsValid())
		{
			AddNoise(creatorEntity, ((Component)explosive).transform.position, NpcNoiseIntensity.High, guessInitiatorPosition: true);
		}
	}

	public void OnVoiceChat(BasePlayer player)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		recentVoiceChatEvents[player] = Time.timeAsDouble;
		AddNoise(player, ((Component)player).transform.position, NpcNoiseIntensity.Low);
	}

	public void OnMeleeHit(BaseMelee weapon, HitInfo info)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer ownerPlayer = weapon.GetOwnerPlayer();
		if ((Object)(object)ownerPlayer != (Object)null)
		{
			AddNoise(ownerPlayer, ((Component)ownerPlayer).transform.position, NpcNoiseIntensity.Low);
		}
	}

	public bool HasPlayerSpokenNear(BaseEntity querier, BasePlayer targetPlayer, float maxDistance = 16f)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("NpcNoiseManager.HasPlayerSpokenNear"))
		{
			double value;
			return recentVoiceChatEvents.TryGetValue(targetPlayer, out value) && Vector3.Distance(((Component)querier).transform.position, ((Component)targetPlayer).transform.position) <= maxDistance;
		}
	}

	public void Tick()
	{
		if (Time.timeAsDouble < nextTickTime)
		{
			return;
		}
		nextTickTime = Time.timeAsDouble + (double)Random.Range(4f, 6f);
		using (TimeWarning.New("NpcNoiseManager.RemoveStaleEntries"))
		{
			PooledList<BasePlayer> val = Pool.Get<PooledList<BasePlayer>>();
			try
			{
				double value;
				foreach (KeyValuePair<BasePlayer, double> recentVoiceChatEvent in recentVoiceChatEvents)
				{
					recentVoiceChatEvent.Deconstruct(out var key, out value);
					BasePlayer basePlayer = key;
					double num = value;
					if (!basePlayer.IsValid() || Time.timeAsDouble - num > 1.0)
					{
						((List<BasePlayer>)(object)val).Add(basePlayer);
					}
				}
				foreach (BasePlayer item in (List<BasePlayer>)(object)val)
				{
					recentVoiceChatEvents.Remove(item, out value);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}
}

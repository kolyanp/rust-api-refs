using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Rust;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;

public class PuzzleReset : FacepunchBehaviour
{
	public SpawnGroup[] respawnGroups;

	public IOEntity[] resetEnts;

	public GameObject[] resetObjects;

	public bool playersBlockReset;

	public bool CheckSleepingAIZForPlayers;

	public float playerDetectionRadius;

	public Transform playerDetectionOrigin;

	public bool ignoreAboveGroundPlayers;

	public float timeBetweenResets = 30f;

	public bool scaleWithServerPopulation;

	public bool pauseUntilLooted;

	[Tooltip("Ignore players below this height")]
	public float minimumHeightOffset;

	[HideInInspector]
	public Vector3[] resetPositions;

	public bool broadcastResetMessage;

	public Phrase resetPhrase;

	public bool radiationReset;

	public static ListHashSet<PuzzleReset> AllResets;

	private List<SpawnGroup> _cachedSpawnGroups;

	private List<GameObject> _cachedResetObjects;

	public static Phrase BlockedWarningPhrase;

	private AIInformationZone zone;

	private TwoTierRadiationZone radiationZone;

	private float currentResetTotalTime;

	private float timePausedUnlooted;

	public float resetTimeElapsed;

	private float timeSpentEmptyWithRads;

	private float timeSpentBlockedWithRads;

	private bool hasBeenLooted;

	private float resetTickTime = 10f;

	private bool hasPlayerEnteredRange;

	private float timeSpentBlocked;

	private string lootedSpawnGroupName;

	private static string TwoTierRadSpherePath;

	private static string TwoTierRadBoxPath;

	private List<NetworkableId> danglingSpawnedInstances;

	private bool canUseRadiationReset
	{
		get
		{
			if (radiationReset)
			{
				return ConVar.Server.monumentPuzzleResetRadiation;
			}
			return false;
		}
	}

	public float lastNormalizedRadiation { get; private set; }

	public void Save(BaseNetworkable.SaveInfo info)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		info.msg.puzzleReset = Pool.Get<PuzzleReset>();
		info.msg.puzzleReset.playerBlocksReset = playersBlockReset;
		if ((Object)(object)playerDetectionOrigin != (Object)null)
		{
			info.msg.puzzleReset.playerDetectionOrigin = playerDetectionOrigin.position;
		}
		info.msg.puzzleReset.playerDetectionRadius = playerDetectionRadius;
		info.msg.puzzleReset.scaleWithServerPopulation = scaleWithServerPopulation;
		info.msg.puzzleReset.timeBetweenResets = timeBetweenResets;
		info.msg.puzzleReset.checkSleepingAIZForPlayers = CheckSleepingAIZForPlayers;
		info.msg.puzzleReset.ignoreAboveGroundPlayers = ignoreAboveGroundPlayers;
		info.msg.puzzleReset.broadcastResetMessage = broadcastResetMessage;
		info.msg.puzzleReset.resetPhrase = resetPhrase?.token ?? "";
		info.msg.puzzleReset.radiationReset = radiationReset;
		info.msg.puzzleReset.pauseUntilLooted = pauseUntilLooted;
		foreach (SpawnGroup spawnGroup in GetSpawnGroups())
		{
			if (spawnGroup.shouldBlockSpawnedEntitySaving)
			{
				continue;
			}
			PuzzleReset puzzleReset = info.msg.puzzleReset;
			if (puzzleReset.danglingSpawnedInstances == null)
			{
				puzzleReset.danglingSpawnedInstances = Pool.Get<List<NetworkableId>>();
			}
			foreach (SpawnPointInstance spawnInstance in spawnGroup.SpawnInstances)
			{
				if ((Object)(object)spawnInstance != (Object)null && spawnInstance.Entity.IsValid() && spawnInstance.Entity.enableSaving)
				{
					info.msg.puzzleReset.danglingSpawnedInstances.Add(spawnInstance.Entity.net.ID);
				}
			}
		}
		if (danglingSpawnedInstances != null && danglingSpawnedInstances.Count > 0)
		{
			PuzzleReset puzzleReset = info.msg.puzzleReset;
			if (puzzleReset.danglingSpawnedInstances == null)
			{
				puzzleReset.danglingSpawnedInstances = Pool.Get<List<NetworkableId>>();
			}
			foreach (NetworkableId danglingSpawnedInstance in danglingSpawnedInstances)
			{
				if (!info.msg.puzzleReset.danglingSpawnedInstances.Contains(danglingSpawnedInstance))
				{
					info.msg.puzzleReset.danglingSpawnedInstances.Add(danglingSpawnedInstance);
				}
			}
		}
		if (resetPositions != null && resetPositions.Length != 0)
		{
			info.msg.puzzleReset.resetPositions = Pool.Get<List<Vector3>>();
			for (int i = 0; i < resetPositions.Length; i++)
			{
				info.msg.puzzleReset.resetPositions.Add(resetPositions[i]);
			}
		}
	}

	public void Load(BaseNetworkable.LoadInfo info)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		playersBlockReset = info.msg.puzzleReset.playerBlocksReset;
		if ((Object)(object)playerDetectionOrigin != (Object)null)
		{
			playerDetectionOrigin.position = info.msg.puzzleReset.playerDetectionOrigin;
		}
		playerDetectionRadius = info.msg.puzzleReset.playerDetectionRadius;
		scaleWithServerPopulation = info.msg.puzzleReset.scaleWithServerPopulation;
		timeBetweenResets = info.msg.puzzleReset.timeBetweenResets;
		CheckSleepingAIZForPlayers = info.msg.puzzleReset.checkSleepingAIZForPlayers;
		ignoreAboveGroundPlayers = info.msg.puzzleReset.ignoreAboveGroundPlayers;
		broadcastResetMessage = info.msg.puzzleReset.broadcastResetMessage;
		if (!string.IsNullOrEmpty(info.msg.puzzleReset.resetPhrase))
		{
			Phrase phrase = Translate.GetPhrase(info.msg.puzzleReset.resetPhrase);
			if (phrase != null)
			{
				resetPhrase = phrase;
			}
		}
		if (info.msg.puzzleReset.danglingSpawnedInstances != null)
		{
			danglingSpawnedInstances = List.ShallowClonePooled<NetworkableId>(info.msg.puzzleReset.danglingSpawnedInstances);
			foreach (NetworkableId danglingSpawnedInstance in danglingSpawnedInstances)
			{
				Net.sv.RegisterUID(danglingSpawnedInstance.Value);
			}
		}
		if (info.msg.puzzleReset.resetPositions != null && info.msg.puzzleReset.resetPositions.Count > 0)
		{
			resetPositions = (Vector3[])(object)new Vector3[info.msg.puzzleReset.resetPositions.Count];
			for (int i = 0; i < info.msg.puzzleReset.resetPositions.Count; i++)
			{
				resetPositions[i] = info.msg.puzzleReset.resetPositions[i];
			}
		}
		radiationReset = info.msg.puzzleReset.radiationReset;
		pauseUntilLooted = info.msg.puzzleReset.pauseUntilLooted;
		ResetTimer();
	}

	public float GetResetSpacing()
	{
		return timeBetweenResets * (scaleWithServerPopulation ? (1f - SpawnHandler.PlayerLerp(Spawn.min_rate, Spawn.max_rate)) : 1f);
	}

	private void OnEnable()
	{
		AllResets.Add(this);
	}

	private void OnDisable()
	{
		AllResets.Remove(this);
	}

	public void Start()
	{
		if (timeBetweenResets != float.PositiveInfinity)
		{
			ResetTimer();
		}
	}

	public void ResetTimer()
	{
		ResetTimeCounters();
		CancelInvoke(ResetTick);
		InvokeRandomized(ResetTick, Random.Range(0f, 1f), resetTickTime, 0.5f);
	}

	private void ResetTimeCounters()
	{
		resetTimeElapsed = 0f;
		timePausedUnlooted = 0f;
		timeSpentBlocked = 0f;
		timeSpentEmptyWithRads = 0f;
		timeSpentBlockedWithRads = 0f;
		currentResetTotalTime = 0f;
		hasPlayerEnteredRange = false;
	}

	public bool PassesResetCheck()
	{
		if (playersBlockReset)
		{
			if (CheckSleepingAIZForPlayers)
			{
				if (radiationReset && (Object)(object)radiationZone != (Object)null && lastNormalizedRadiation > 0f)
				{
					return !radiationZone.HasPlayersInRange();
				}
				bool num = AIZSleeping();
				if (!num)
				{
					TryDDrawAIZone();
				}
				return num;
			}
			return !PlayersWithinDistance();
		}
		return true;
	}

	private void TryDDrawAIZone()
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		AIInformationZone aIZone = GetAIZone();
		if (!ConVar.Server.drawpuzzleresets || !((Object)(object)aIZone != (Object)null))
		{
			return;
		}
		if (aIZone.wakeZones.Count == 0)
		{
			Debug.LogWarning((object)"Trying to draw AIZone for PuzzleReset but it has no TriggerWakeAIZ!");
			return;
		}
		_ = aIZone.wakeZones[0];
		if (aIZone.wakeZones.Count > 1)
		{
			Debug.LogWarning((object)"Trying to draw AIZone for PuzzleReset but it has multiple TriggerWakeAIZs! Defaulting to first one found");
		}
		PooledList<BasePlayer> pooledListOfPlayers = aIZone.wakeZones[0].GetPooledListOfPlayers();
		try
		{
			foreach (BasePlayer item in (List<BasePlayer>)(object)pooledListOfPlayers)
			{
				if (item.IsAdmin)
				{
					OBB areaBox = aIZone.areaBox;
					if (((OBB)(ref areaBox)).Contains(((Component)item).transform.position))
					{
						item.SendConsoleCommand(DDrawCommand.Box(aIZone.areaBox.position, 10f, Color.green, aIZone.areaBox.extents * 2f, aIZone.areaBox.rotation, distanceFade: false));
						item.SendConsoleCommand(DDrawCommand.Box(aIZone.areaBox.position, 10f, Color.yellow, ScaleSizeByConVar(aIZone.areaBox.extents * 2f), aIZone.areaBox.rotation, distanceFade: false));
					}
				}
			}
		}
		finally
		{
			((IDisposable)pooledListOfPlayers)?.Dispose();
		}
	}

	private void DDrawLootedStatus()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)playerDetectionOrigin == (Object)null)
		{
			return;
		}
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (current.IsAdmin && IsPlayerInRange(current))
				{
					string text = $"Looted: {hasBeenLooted}";
					if (hasBeenLooted)
					{
						text = text + "\nGroup: " + (lootedSpawnGroupName ?? "Unknown");
					}
					current.SendConsoleCommand(DDrawCommand.Text(((Component)playerDetectionOrigin).transform.position, 10f, Color.green, text, 2f, distanceFade: false));
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public AIInformationZone GetAIZone()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)zone != (Object)null)
		{
			if (!zone.PointInside(((Component)this).transform.position))
			{
				zone = AIInformationZone.GetForPoint(((Component)this).transform.position);
			}
		}
		else
		{
			zone = AIInformationZone.GetForPoint(((Component)this).transform.position);
		}
		return zone;
	}

	public List<SpawnGroup> GetSpawnGroups()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (_cachedSpawnGroups != null)
		{
			return _cachedSpawnGroups;
		}
		_cachedSpawnGroups = new List<SpawnGroup>();
		Vis.Components<SpawnGroup>(((Component)this).transform.position, 1f, _cachedSpawnGroups, 262144, (QueryTriggerInteraction)2);
		return _cachedSpawnGroups;
	}

	public List<GameObject> GetResetObjects()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (_cachedResetObjects != null)
		{
			return _cachedResetObjects;
		}
		_cachedResetObjects = new List<GameObject>();
		List<PuzzleResetObject> list = Pool.Get<List<PuzzleResetObject>>();
		Vis.Components<PuzzleResetObject>(((Component)this).transform.position, 1f, list, 262144, (QueryTriggerInteraction)2);
		foreach (PuzzleResetObject item in list)
		{
			_cachedResetObjects.Add(((Component)((Component)item).gameObject.transform.parent).gameObject);
		}
		Pool.FreeUnmanaged<PuzzleResetObject>(ref list);
		return _cachedResetObjects;
	}

	private bool AIZSleeping()
	{
		AIInformationZone aIZone = GetAIZone();
		if ((Object)(object)aIZone == (Object)null)
		{
			return false;
		}
		return aIZone.Sleeping;
	}

	private bool PlayersWithinDistance(bool includeSleepers = false)
	{
		return AnyPlayersWithinDistance(playerDetectionOrigin, playerDetectionRadius, minimumHeightOffset, ignoreAboveGroundPlayers, includeSleepers);
	}

	public static bool AnyPlayersWithinDistance(Transform origin, float radius, float heightOffset, bool ignoreAboveGroundPlayers = false, bool includeSleepers = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		float num = origin.position.y - radius + heightOffset;
		IEnumerable<BasePlayer> enumerable;
		if (!includeSleepers)
		{
			IEnumerable<BasePlayer> activePlayerList = (IEnumerable<BasePlayer>)BasePlayer.activePlayerList;
			enumerable = activePlayerList;
		}
		else
		{
			enumerable = BasePlayer.allPlayerList;
		}
		foreach (BasePlayer item in enumerable)
		{
			if (!item.IsAlive() || item.isInvisible || (!includeSleepers && item.IsSleeping()) || ((Component)item).transform.position.y < num || Vector3.Distance(((Component)item).transform.position, origin.position) >= radius || (ignoreAboveGroundPlayers && item.IsUnderground()))
			{
				continue;
			}
			if (ConVar.Server.drawpuzzleresets && item.IsConnected && item.IsAdmin)
			{
				item.SendConsoleCommand(DDrawCommand.Sphere(origin.position, 10f, Color.green, radius, distanceFade: false));
				item.SendConsoleCommand(DDrawCommand.Sphere(origin.position, 10f, Color.yellow, ScaleRadiusByConVar(radius), distanceFade: false));
				if (heightOffset > 0f)
				{
					Vector3 val = origin.position + new Vector3(0f, 0f - radius + heightOffset, 0f);
					item.SendConsoleCommand(DDrawCommand.Line(val + new Vector3(radius, 0f, 0f), val - new Vector3(radius, 0f, 0f), 10f, Color.red, distanceFade: false, zTest: true));
					item.SendConsoleCommand(DDrawCommand.Line(val + new Vector3(0f, 0f, radius), val - new Vector3(0f, 0f, radius), 10f, Color.red, distanceFade: false, zTest: true));
				}
			}
			return true;
		}
		return false;
	}

	public void ResetTick()
	{
		float num = resetTickTime * Debugging.puzzleResetTimeMultiplier;
		currentResetTotalTime += num;
		if (pauseUntilLooted && ConVar.Server.pauseunlootedpuzzles)
		{
			hasBeenLooted = HasPuzzleBeenPartialLooted(out var lootedSpawnGroup);
			lootedSpawnGroupName = (((Object)(object)lootedSpawnGroup == (Object)null) ? null : ((Object)lootedSpawnGroup).name);
			if (ConVar.Server.drawpuzzleresets)
			{
				DDrawLootedStatus();
			}
			if (!hasBeenLooted)
			{
				timePausedUnlooted += num;
				return;
			}
		}
		if (canUseRadiationReset)
		{
			bool num2 = !PassesResetCheck();
			if (num2)
			{
				hasPlayerEnteredRange = true;
			}
			if (!hasPlayerEnteredRange)
			{
				num = 0f;
			}
			resetTimeElapsed += num;
			if (num2)
			{
				timeSpentBlocked += num;
				timeSpentBlockedWithRads += num;
				hasPlayerEnteredRange = true;
			}
			else if (lastNormalizedRadiation > 0f)
			{
				timeSpentEmptyWithRads += num;
			}
		}
		else if (PassesResetCheck())
		{
			resetTimeElapsed += num;
		}
		else
		{
			timeSpentBlocked += num;
		}
		float resetSpacing = GetResetSpacing();
		if (resetTimeElapsed > resetSpacing && (!canUseRadiationReset || timeSpentEmptyWithRads > ConVar.Server.monumentPuzzleResetRadiationPlayerEmptyTime))
		{
			DoReset();
			ResetTimeCounters();
		}
		float num3 = resetSpacing - resetTimeElapsed;
		if (!canUseRadiationReset || (!(num3 < ConVar.Server.monumentPuzzleResetRadiationPreResetTime) && !ConVar.Server.monumentpuzzleresetradiationoverride))
		{
			return;
		}
		SetRadiusRadiationAmount(ConVar.Server.monumentpuzzleresetradiationoverride ? 0.95f : (1f - Mathf.Clamp01(num3 / ConVar.Server.monumentPuzzleResetRadiationPreResetTime)));
		if (ConVar.Server.monumentPuzzleResetWarnings && (Object)(object)radiationZone != (Object)null)
		{
			PooledList<BasePlayer> val = Pool.Get<PooledList<BasePlayer>>();
			try
			{
				NotifyRadiationZone(radiationZone.InnerRadiation, (List<BasePlayer>)(object)val);
				NotifyRadiationZone(radiationZone.OuterRadiation, (List<BasePlayer>)(object)val);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		void NotifyRadiationZone(TriggerRadiation r, List<BasePlayer> sentPlayers)
		{
			if (r.entityContents == null)
			{
				return;
			}
			foreach (BaseEntity entityContent in r.entityContents)
			{
				if (entityContent is BasePlayer { IsNpc: false } basePlayer && !sentPlayers.Contains(basePlayer) && (!ignoreAboveGroundPlayers || basePlayer.IsUnderground()))
				{
					sentPlayers.Add(basePlayer);
					basePlayer.ShowToast(GameTip.Styles.Error, BlockedWarningPhrase, false);
				}
			}
		}
	}

	private bool HasPuzzleBeenPartialLooted(out SpawnGroup lootedSpawnGroup)
	{
		lootedSpawnGroup = null;
		if (GetSpawnGroups().Count == 0)
		{
			return true;
		}
		foreach (SpawnGroup spawnGroup in GetSpawnGroups())
		{
			if ((Object)(object)spawnGroup == (Object)null || spawnGroup.WantsTimedSpawn() || spawnGroup.resetBehavior == SpawnGroupResetBehavior.Exclude || (spawnGroup.DoesGroupContainNPCs() && spawnGroup.resetBehavior != SpawnGroupResetBehavior.Include))
			{
				continue;
			}
			if (spawnGroup.ObjectsActive < Mathf.Min(spawnGroup.maxPopulation, spawnGroup.SpawnPointCount))
			{
				lootedSpawnGroup = spawnGroup;
				return true;
			}
			foreach (SpawnPointInstance spawnInstance in spawnGroup.SpawnInstances)
			{
				if (spawnInstance.Entity is LootContainer { HasBeenLooted: not false })
				{
					lootedSpawnGroup = spawnGroup;
					return true;
				}
			}
		}
		return false;
	}

	public void CleanupSleepers()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)playerDetectionOrigin == (Object)null || BasePlayer.sleepingPlayerList == null)
		{
			return;
		}
		for (int num = BasePlayer.sleepingPlayerList.Count - 1; num >= 0; num--)
		{
			BasePlayer basePlayer = BasePlayer.sleepingPlayerList[num];
			if (!((Object)(object)basePlayer == (Object)null) && basePlayer.IsSleeping() && Vector3.Distance(((Component)basePlayer).transform.position, playerDetectionOrigin.position) <= playerDetectionRadius && (!ignoreAboveGroundPlayers || basePlayer.IsUnderground()))
			{
				basePlayer.Hurt(1000f, DamageType.Suicide, basePlayer, useProtection: false);
			}
		}
	}

	public void TryForceReset()
	{
		if (PlayersWithinDistance(includeSleepers: true))
		{
			RadiationSphere radiationSphere = default(RadiationSphere);
			foreach (GameObject resetObject in GetResetObjects())
			{
				if (resetObject.TryGetComponent<RadiationSphere>(ref radiationSphere))
				{
					radiationSphere.RestartRadiation();
				}
			}
			return;
		}
		DoReset(skipAlerts: true);
		ResetTimer();
	}

	public void DoReset(bool skipAlerts = false)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		SetRadiusRadiationAmount(0f);
		CleanupSleepers();
		IOEntity component = ((Component)this).GetComponent<IOEntity>();
		if ((Object)(object)component != (Object)null)
		{
			ResetIOEntRecursive(component, Time.frameCount);
			component.MarkDirty();
		}
		if (resetPositions != null)
		{
			Vector3[] array = resetPositions;
			foreach (Vector3 val in array)
			{
				Vector3 position = ((Component)this).transform.TransformPoint(val);
				List<IOEntity> list = Pool.Get<List<IOEntity>>();
				Vis.Entities(position, 0.5f, list, 1235288065, (QueryTriggerInteraction)1);
				foreach (IOEntity item in list)
				{
					if (item.IsRootEntity() && item.isServer)
					{
						ResetIOEntRecursive(item, Time.frameCount);
						item.MarkDirty();
					}
				}
				Pool.FreeUnmanaged<IOEntity>(ref list);
			}
		}
		if (danglingSpawnedInstances != null)
		{
			foreach (NetworkableId danglingSpawnedInstance in danglingSpawnedInstances)
			{
				BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(danglingSpawnedInstance);
				if (baseNetworkable.IsValid())
				{
					baseNetworkable.Kill();
				}
			}
			Pool.FreeUnmanaged<NetworkableId>(ref danglingSpawnedInstances);
		}
		foreach (SpawnGroup spawnGroup in GetSpawnGroups())
		{
			if (!((Object)(object)spawnGroup == (Object)null))
			{
				spawnGroup.Clear();
				spawnGroup.DelayedSpawn();
			}
		}
		OilRigResetNotification oilRigResetNotification = default(OilRigResetNotification);
		foreach (GameObject resetObject in GetResetObjects())
		{
			if ((!skipAlerts || !resetObject.TryGetComponent<OilRigResetNotification>(ref oilRigResetNotification)) && (Object)(object)resetObject != (Object)null)
			{
				resetObject.SendMessage("OnPuzzleReset", (SendMessageOptions)1);
			}
		}
		if (broadcastResetMessage && !skipAlerts)
		{
			Enumerator<BasePlayer> enumerator5 = BasePlayer.activePlayerList.GetEnumerator();
			try
			{
				while (enumerator5.MoveNext())
				{
					BasePlayer current5 = enumerator5.Current;
					if (!current5.IsNpc && current5.IsConnected && !current5.IsInTutorial)
					{
						current5.ShowToast(GameTip.Styles.Server_Event, resetPhrase, false);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator5/*cast due to constrained. prefix*/).Dispose();
			}
		}
		Analytics.Azure.OnPuzzleReset(this, currentResetTotalTime, timeSpentBlocked, timeSpentBlockedWithRads, timePausedUnlooted);
	}

	public void DebugApplyPuzzleResetTime(float time)
	{
		float num = resetTickTime;
		resetTickTime = time;
		ResetTick();
		resetTickTime = num;
	}

	public static void ResetIOEntRecursive(IOEntity target, int resetIndex)
	{
		if (target.lastResetIndex == resetIndex)
		{
			return;
		}
		target.lastResetIndex = resetIndex;
		target.ResetIOState();
		IOEntity.IOSlot[] outputs = target.outputs;
		foreach (IOEntity.IOSlot iOSlot in outputs)
		{
			if ((Object)(object)iOSlot.connectedTo.Get() != (Object)null && (Object)(object)iOSlot.connectedTo.Get() != (Object)(object)target)
			{
				ResetIOEntRecursive(iOSlot.connectedTo.Get(), resetIndex);
			}
		}
	}

	private void SetRadiusRadiationAmount(float normalisedAmount)
	{
		if (!canUseRadiationReset)
		{
			normalisedAmount = 0f;
		}
		InitialiseRadiationTriggers();
		if ((Object)(object)radiationZone == (Object)null)
		{
			return;
		}
		((Component)radiationZone).gameObject.SetActive(normalisedAmount > 0f);
		radiationZone.SetRadiationLevel(normalisedAmount * ConVar.Server.monumentPuzzleResetRadiationAmount, normalisedAmount * 10f);
		radiationZone.SetBypassArmor(state: true);
		radiationZone.SetIgnoreAboveGroundPlayers(ignoreAboveGroundPlayers);
		lastNormalizedRadiation = normalisedAmount;
		if (!(normalisedAmount >= 1f) || radiationZone.InnerRadiation.entityContents == null)
		{
			return;
		}
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			((List<BaseEntity>)(object)val).AddRange((IEnumerable<BaseEntity>)radiationZone.InnerRadiation.entityContents);
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				if (item is BasePlayer basePlayer && (!ignoreAboveGroundPlayers || basePlayer.IsUnderground()))
				{
					basePlayer.Hurt(25f, DamageType.Radiation, null, useProtection: false);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void InitialiseRadiationTriggers()
	{
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		if (!canUseRadiationReset)
		{
			return;
		}
		if ((Object)(object)radiationZone == (Object)null)
		{
			if (CheckSleepingAIZForPlayers)
			{
				if ((Object)(object)GetAIZone() != (Object)null)
				{
					GameObject val = GameManager.server.CreatePrefab(TwoTierRadBoxPath, ((Component)this).transform);
					radiationZone = val.GetComponent<TwoTierRadiationZone>();
				}
			}
			else
			{
				GameObject val2 = GameManager.server.CreatePrefab(TwoTierRadSpherePath, ((Component)this).transform);
				radiationZone = val2.GetComponent<TwoTierRadiationZone>();
				if ((Object)(object)playerDetectionOrigin != (Object)null)
				{
					val2.transform.localPosition = playerDetectionOrigin.localPosition;
					val2.transform.localRotation = playerDetectionOrigin.localRotation;
					val2.transform.localScale = Vector3.one;
				}
			}
		}
		if (CheckSleepingAIZForPlayers)
		{
			Vector3 val3 = ScaleSizeByConVar(zone.areaBox.extents * 2f);
			Vector3 val4 = ((Component)this).transform.InverseTransformPoint(zone.areaBox.position);
			radiationZone.Apply(new Bounds(val4, zone.areaBox.extents * 2f), new Bounds(val4, val3));
			((Component)radiationZone).transform.rotation = zone.areaBox.rotation;
			return;
		}
		float num = playerDetectionRadius;
		float num2 = ScaleRadiusByConVar(playerDetectionRadius);
		float num3 = num2 - num;
		((Component)radiationZone).transform.localPosition = playerDetectionOrigin.localPosition;
		radiationZone.Apply(num, num2);
		radiationZone.InnerRadiation.MinLocalHeight = 0f - num + minimumHeightOffset;
		_ = playerDetectionRadius;
		radiationZone.OuterRadiation.MinLocalHeight = 0f - num2 + num3 + minimumHeightOffset;
		radiationZone.InnerRadiation.ApplyLocalHeightCheck = true;
		radiationZone.OuterRadiation.ApplyLocalHeightCheck = true;
	}

	public bool IsPlayerInRange(BasePlayer bp)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (CheckSleepingAIZForPlayers)
		{
			OBB areaBox = GetAIZone().areaBox;
			if (((OBB)(ref areaBox)).Contains(((Component)bp).transform.position))
			{
				return true;
			}
		}
		else
		{
			float num = playerDetectionOrigin.position.y - playerDetectionRadius + minimumHeightOffset;
			if (((Component)bp).transform.position.y < num)
			{
				return false;
			}
			if (bp.Distance(playerDetectionOrigin.position) <= playerDetectionRadius)
			{
				return true;
			}
		}
		return false;
	}

	public void GetDebugInfo(List<string> readout)
	{
		float resetSpacing = GetResetSpacing();
		readout.Add($"Reset time: {resetTimeElapsed}/{resetSpacing}");
		if (canUseRadiationReset)
		{
			float num = resetSpacing - ConVar.Server.monumentPuzzleResetRadiationPreResetTime - resetTimeElapsed;
			if (num > 0f)
			{
				readout.Add($"Rads begin in {num}");
			}
			else
			{
				readout.Add($"Rads active: {lastNormalizedRadiation}");
				readout.Add($"Time spent empty with rads:{timeSpentEmptyWithRads}/{ConVar.Server.monumentPuzzleResetRadiationPlayerEmptyTime}");
			}
			if (!hasPlayerEnteredRange)
			{
				readout.Add("No player has entered range, will not tick");
			}
			else
			{
				readout.Add("Player has entered range, ticking...");
			}
		}
	}

	public MonumentInfo GetClosestMonumentInfo()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TerrainMeta.Path != (Object)null && TerrainMeta.Path.Monuments != null)
		{
			float num = float.MaxValue;
			MonumentInfo result = null;
			{
				foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
				{
					float num2 = Vector3.Distance(((Component)this).transform.position, ((Component)monument).transform.position);
					if (num2 < num)
					{
						num = num2;
						result = monument;
					}
				}
				return result;
			}
		}
		return null;
	}

	private static float ScaleRadiusByConVar(float radius)
	{
		return Mathf.Min(radius * ConVar.Server.monumentPuzzleResetRadiationRadiusMultiplier, radius + ConVar.Server.monumentPuzzleResetRadiationMaxRadiusIncrease);
	}

	private static Vector3 ScaleSizeByConVar(Vector3 size)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		size.x = Mathf.Min(size.x * ConVar.Server.monumentPuzzleResetRadiationRadiusMultiplier, size.x + ConVar.Server.monumentPuzzleResetRadiationMaxRadiusIncrease * 2f);
		size.y = Mathf.Min(size.y * ConVar.Server.monumentPuzzleResetRadiationRadiusMultiplier, size.y + ConVar.Server.monumentPuzzleResetRadiationMaxRadiusIncrease * 2f);
		size.z = Mathf.Min(size.z * ConVar.Server.monumentPuzzleResetRadiationRadiusMultiplier, size.z + ConVar.Server.monumentPuzzleResetRadiationMaxRadiusIncrease * 2f);
		return size;
	}

	static PuzzleReset()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		AllResets = new ListHashSet<PuzzleReset>();
		BlockedWarningPhrase = new Phrase("monument.blocked.warning", "This monument is resetting, please leave the area!");
		TwoTierRadSpherePath = "assets/prefabs/io/electric/generators/twotierradiationsphere.prefab";
		TwoTierRadBoxPath = "assets/prefabs/io/electric/generators/twotierradiationbox.prefab";
	}
}

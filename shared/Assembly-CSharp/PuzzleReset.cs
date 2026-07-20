using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
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

	private float timeSpentBlocked;

	[HideInInspector]
	public Vector3[] resetPositions;

	public bool broadcastResetMessage;

	public Phrase resetPhrase;

	private List<SpawnGroup> _cachedSpawnGroups;

	public bool radiationReset;

	public static Phrase BlockedWarningPhrase = new Phrase("monument.blocked.warning", "This monument is resetting, please leave the area!");

	private AIInformationZone zone;

	private TwoTierRadiationZone radiationZone;

	public static ListHashSet<PuzzleReset> AllResets = new ListHashSet<PuzzleReset>();

	private float currentResetTotalTime;

	private float timePausedUnlooted;

	public float resetTimeElapsed;

	private float timeSpentEmptyWithRads;

	private float timeSpentBlockedWithRads;

	private bool hasBeenLooted;

	private float resetTickTime = 10f;

	private bool hasPlayerEnteredRange;

	private string lootedSpawnGroupName;

	private static string TwoTierRadSpherePath = "assets/prefabs/io/electric/generators/twotierradiationsphere.prefab";

	private static string TwoTierRadBoxPath = "assets/prefabs/io/electric/generators/twotierradiationbox.prefab";

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
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
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

	private bool AIZSleeping()
	{
		AIInformationZone aIZone = GetAIZone();
		if ((Object)(object)aIZone == (Object)null)
		{
			return false;
		}
		return aIZone.Sleeping;
	}

	private bool PlayersWithinDistance()
	{
		return AnyPlayersWithinDistance(playerDetectionOrigin, playerDetectionRadius, minimumHeightOffset, ignoreAboveGroundPlayers);
	}

	public static bool AnyPlayersWithinDistance(Transform origin, float radius, float heightOffset, bool ignoreAboveGroundPlayers = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		float num = origin.position.y - radius + heightOffset;
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (current.IsSleeping() || !current.IsAlive() || current.isInvisible || ((Component)current).transform.position.y < num || !(Vector3.Distance(((Component)current).transform.position, origin.position) < radius) || (ignoreAboveGroundPlayers && !current.IsUnderground()))
				{
					continue;
				}
				if (ConVar.Server.drawpuzzleresets && current.IsAdmin)
				{
					float duration = 10f;
					current.SendConsoleCommand(DDrawCommand.Sphere(origin.position, duration, Color.green, radius, distanceFade: false));
					current.SendConsoleCommand(DDrawCommand.Sphere(origin.position, duration, Color.yellow, ScaleRadiusByConVar(radius), distanceFade: false));
					if (heightOffset > 0f)
					{
						Vector3 val = origin.position + new Vector3(0f, 0f - radius + heightOffset, 0f);
						current.SendConsoleCommand(DDrawCommand.Line(val + new Vector3(radius, 0f, 0f), val - new Vector3(radius, 0f, 0f), duration, Color.red, distanceFade: false, zTest: true));
						current.SendConsoleCommand(DDrawCommand.Line(val + new Vector3(0f, 0f, radius), val - new Vector3(0f, 0f, radius), duration, Color.red, distanceFade: false, zTest: true));
					}
				}
				return true;
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
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

	public void DoReset()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		SetRadiusRadiationAmount(0f);
		CleanupSleepers();
		IOEntity component = ((Component)this).GetComponent<IOEntity>();
		if ((Object)(object)component != (Object)null)
		{
			ResetIOEntRecursive(component, Time.frameCount);
			component.MarkDirty();
		}
		else if (resetPositions != null)
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
		foreach (SpawnGroup spawnGroup in GetSpawnGroups())
		{
			if (!((Object)(object)spawnGroup == (Object)null))
			{
				spawnGroup.Clear();
				spawnGroup.DelayedSpawn();
			}
		}
		GameObject[] array2 = resetObjects;
		foreach (GameObject val2 in array2)
		{
			if ((Object)(object)val2 != (Object)null)
			{
				val2.SendMessage("OnPuzzleReset", (SendMessageOptions)1);
			}
		}
		if (broadcastResetMessage)
		{
			Enumerator<BasePlayer> enumerator3 = BasePlayer.activePlayerList.GetEnumerator();
			try
			{
				while (enumerator3.MoveNext())
				{
					BasePlayer current3 = enumerator3.Current;
					if (!current3.IsNpc && current3.IsConnected && !current3.IsInTutorial)
					{
						current3.ShowToast(GameTip.Styles.Server_Event, resetPhrase, false);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator3/*cast due to constrained. prefix*/).Dispose();
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
}

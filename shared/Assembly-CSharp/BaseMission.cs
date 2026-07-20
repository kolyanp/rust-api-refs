using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Rust;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Rust/Missions/Base Mission")]
public class BaseMission : BaseScriptableObject
{
	public class MissionInstance : IPooled
	{
		[Serializable]
		public class ObjectiveStatus : IPooled
		{
			public bool started;

			public bool softCompleted;

			public bool blockReset;

			public bool completed;

			public bool failed;

			public float progressTarget;

			public float progressCurrent;

			public Vector3 worldLocation;

			public RealTimeSince sinceLastThink;

			void IPooled.EnterPool()
			{
				Reset();
			}

			void IPooled.LeavePool()
			{
			}

			public void Reset()
			{
				//IL_003f: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Unknown result type (might be due to invalid IL or missing references)
				started = false;
				softCompleted = false;
				blockReset = false;
				completed = false;
				failed = false;
				progressTarget = 0f;
				progressCurrent = 0f;
				worldLocation = default(Vector3);
				sinceLastThink = default(RealTimeSince);
			}

			public bool IsObjectiveActive()
			{
				if (started && !completed)
				{
					return !failed;
				}
				return false;
			}
		}

		private IMissionProvider _cachedProvider;

		private BaseMission _cachedMission;

		public NetworkableId providerID;

		public uint missionID;

		public MissionStatus status;

		public long startTimeUtcSeconds;

		public long endTimeUtcSeconds;

		public float timePassed;

		public Dictionary<string, Vector3> missionPoints = new Dictionary<string, Vector3>();

		public Dictionary<string, MissionEntity> spawnedMissionEntities = new Dictionary<string, MissionEntity>();

		public ListHashSet<BaseEntity> persistentMissionEntities = new ListHashSet<BaseEntity>();

		public bool hasDispensedRewards;

		private int playerInputCounter;

		public BufferList<ObjectiveStatus> objectiveStatuses = new BufferList<ObjectiveStatus>();

		public IMissionProvider GetMissionProvider()
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			if (_cachedProvider == null)
			{
				_cachedProvider = BaseNetworkable.serverEntities.Find(providerID) as IMissionProvider;
			}
			return _cachedProvider;
		}

		public BaseMission GetMission()
		{
			using (TimeWarning.New("MissionInstance.GetMission"))
			{
				if (_cachedMission == null)
				{
					_cachedMission = MissionManifest.GetFromID(missionID);
				}
				return _cachedMission;
			}
		}

		public bool NeedsPlayerInput()
		{
			return playerInputCounter > 0;
		}

		public void EnablePlayerInput()
		{
			playerInputCounter++;
		}

		public void DisablePlayerInput()
		{
			playerInputCounter--;
			if (playerInputCounter < 0)
			{
				playerInputCounter = 0;
			}
		}

		public virtual void ProcessMissionEvent(BasePlayer playerFor, MissionEventType type, MissionEventPayload payload, float amount)
		{
			if (status == MissionStatus.Active)
			{
				BaseMission mission = GetMission();
				for (int i = 0; i < mission.objectives.Length; i++)
				{
					mission.objectives[i].objective.ProcessMissionEvent(playerFor, this, i, type, payload, amount);
				}
			}
		}

		public void ServerThink(BasePlayer assignee, float timeSinceLastThink)
		{
			if (status == MissionStatus.Accomplished || status == MissionStatus.Active)
			{
				BaseMission mission = GetMission();
				timePassed = (float)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - startTimeUtcSeconds) * Time.missiontimerscale;
				mission.ServerThink(this, assignee, timeSinceLastThink);
				if (mission.timeLimitSeconds > 0 && timePassed >= (float)mission.timeLimitSeconds)
				{
					mission.MissionFailed(this, assignee, MissionFailReason.TimeOut);
				}
			}
		}

		public bool TryGetMissionPoint(string identifier, out Vector3 point, int depth = 0)
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("MissionInstance.TryGetMissionPoint"))
			{
				if (identifier == null)
				{
					identifier = string.Empty;
				}
				if (missionPoints.TryGetValue(identifier, out point))
				{
					return true;
				}
				BaseMission mission = GetMission();
				point = Vector3.zero;
				if (!mission.TryGetPositionGenerator(identifier, out var positionGenerator))
				{
					Debug.LogError((object)("Failure in TryGetMissionPoint on mission instance for " + ((Object)GetMission()).name + ", cannot find position generator for '" + identifier + "'"), (Object)(object)GetMission());
					return false;
				}
				if (!positionGenerator.TryGetPosition(this, out point, depth))
				{
					return false;
				}
				missionPoints.Add(identifier, point);
				if (positionGenerator.positionsAreExclusive)
				{
					AddPositionBlocker(this, point);
				}
				return true;
			}
		}

		public MissionEntity GetSpawnedMissionEntity(string identifier, BasePlayer playerFor)
		{
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			if (identifier == null)
			{
				identifier = "";
			}
			MissionEntity value = null;
			if (spawnedMissionEntities.TryGetValue(identifier, out value))
			{
				return value;
			}
			MissionEntityEntry missionEntityEntry = List.FindWith<MissionEntityEntry, string>((IReadOnlyCollection<MissionEntityEntry>)GetMission().spawnMissionEntityDefinitions, (Func<MissionEntityEntry, string>)((MissionEntityEntry e) => e.identifier), identifier, (IEqualityComparer<string>)null);
			Vector3 point;
			if (missionEntityEntry == null)
			{
				Debug.LogError((object)$"Cannot spawn mission entity, identifier '{identifier}' not found in mission ID {missionID}");
				value = null;
			}
			else if (!missionEntityEntry.entityRef.isValid)
			{
				Debug.LogError((object)$"Cannot spawn mission entity, identifier '{identifier}' has no entity set in mission ID {missionID}");
				value = null;
			}
			else if (TryGetMissionPoint(missionEntityEntry.spawnPositionToUse, out point))
			{
				BaseEntity baseEntity = GameManager.server.CreateEntity(missionEntityEntry.entityRef.resourcePath, point, Quaternion.identity);
				MissionEntity missionEntity = default(MissionEntity);
				value = (((Component)baseEntity).gameObject.TryGetComponent<MissionEntity>(ref missionEntity) ? missionEntity : ((Component)baseEntity).gameObject.AddComponent<MissionEntity>());
				value.Setup(playerFor, this, identifier, missionEntityEntry.cleanupOnMissionSuccess, missionEntityEntry.cleanupOnMissionFailed);
				baseEntity.Spawn();
				if (baseEntity is LootContainer lootContainer && missionEntityEntry.overrideLootOnItem != null && missionEntityEntry.overrideLootOnItem.Length != 0)
				{
					lootContainer.inventory.Clear();
					ItemAmount[] overrideLootOnItem = missionEntityEntry.overrideLootOnItem;
					foreach (ItemAmount itemAmount in overrideLootOnItem)
					{
						lootContainer.inventory.AddItem(itemAmount.itemDef, (int)itemAmount.amount, 0uL);
					}
				}
			}
			if ((Object)(object)value != (Object)null)
			{
				spawnedMissionEntities.Add(identifier, value);
				value.MissionStarted(playerFor, this);
			}
			return value;
		}

		public void PostServerLoad(BasePlayer player)
		{
			BaseMission mission = GetMission();
			for (int i = 0; i < mission.objectives.Length; i++)
			{
				if (i >= 0 && i < objectiveStatuses.Count)
				{
					mission.objectives[i].objective.PostServerLoad(i, this, player);
				}
			}
		}

		void IPooled.EnterPool()
		{
			Reset();
		}

		void IPooled.LeavePool()
		{
		}

		public void Reset()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			RemovePositionBlockers(this);
			providerID = default(NetworkableId);
			missionID = 0u;
			status = MissionStatus.Undefined;
			startTimeUtcSeconds = long.MinValue;
			endTimeUtcSeconds = long.MinValue;
			_cachedMission = null;
			_cachedProvider = null;
			timePassed = 0f;
			missionPoints.Clear();
			spawnedMissionEntities.Clear();
			persistentMissionEntities.Clear();
			for (int i = 0; i < objectiveStatuses.Count; i++)
			{
				ObjectiveStatus objectiveStatus = objectiveStatuses[i];
				Pool.Free<ObjectiveStatus>(ref objectiveStatus);
			}
			objectiveStatuses.Clear();
			hasDispensedRewards = false;
		}

		public bool IsActive()
		{
			MissionStatus missionStatus = status;
			return missionStatus == MissionStatus.Accomplished || missionStatus == MissionStatus.Active;
		}

		public bool TryGetTotalRequiredRewardItemSlots(out int requiredSlots)
		{
			requiredSlots = 0;
			BaseMission mission = GetMission();
			int count = objectiveStatuses.Count;
			int num = mission.objectives.Length;
			if (count != num)
			{
				Debug.LogError((object)$"Mission instance for mission {((Object)mission).name} contains data for {count} objectives but mission has {num} objectives", (Object)(object)mission);
				return false;
			}
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				MissionObjectiveEntry missionObjectiveEntry = mission.objectives[i];
				if (missionObjectiveEntry.bonusRewards == null)
				{
					continue;
				}
				ObjectiveStatus objectiveStatus = objectiveStatuses[i];
				if (missionObjectiveEntry.isRequired || !objectiveStatus.completed)
				{
					continue;
				}
				foreach (MissionBonusReward bonusReward in missionObjectiveEntry.bonusRewards)
				{
					if (bonusReward.RewardType == RewardType.Item)
					{
						num2 += bonusReward.GetRequiredInventorySlots();
					}
				}
			}
			int num3 = 0;
			mission.TryGetRewardsForChoice(0, out var rewards);
			if (rewards != null)
			{
				foreach (MissionReward item in rewards)
				{
					if (item.RewardType == RewardType.Item)
					{
						num3 += item.GetRequiredInventorySlots();
					}
				}
			}
			requiredSlots = num3 + num2;
			return true;
		}
	}

	[Serializable]
	public class PositionGenerator
	{
		public enum RelativeType
		{
			Player,
			Provider,
			Position
		}

		public enum PositionType
		{
			MissionPoint,
			WorldPositionGenerator,
			DungeonPoint,
			Radius,
			UnderwaterLab,
			UnderwaterLabWithBoomboxes
		}

		public class PositionPointAttribute : PropertyAttribute
		{
		}

		public string identifier;

		public float minDistForMovePoint;

		public float maxDistForMovePoint = 25f;

		public bool allowDoubleDistanceIfNoOptionsAreFound;

		public bool positionsAreExclusive = true;

		public RelativeType relativeTo;

		public PositionType positionType;

		[PositionPoint]
		public string centerOnPositionIdentifier = "";

		[InspectorFlags]
		public MissionPoint.MissionPointEnum Flags = (MissionPoint.MissionPointEnum)(-1);

		[InspectorFlags]
		public MissionPoint.MissionPointEnum ExclusionFlags;

		public WorldPositionGenerator worldPositionGenerator;

		private float minDistForMovePoint_2x;

		private float maxDistForMovePoint_2x;

		private float minDistForMovePoint_sqr;

		private float maxDistForMovePoint_sqr;

		private float maxDistForMovePoint_2x_sqr;

		private float maxDistForMovePoint_4x;

		public void CacheDistanceValues()
		{
			minDistForMovePoint_2x = minDistForMovePoint * 2f;
			maxDistForMovePoint_2x = maxDistForMovePoint * 2f;
			minDistForMovePoint_sqr = minDistForMovePoint * minDistForMovePoint;
			maxDistForMovePoint_sqr = maxDistForMovePoint * maxDistForMovePoint;
			maxDistForMovePoint_2x_sqr = maxDistForMovePoint_2x * maxDistForMovePoint_2x;
			maxDistForMovePoint_4x = maxDistForMovePoint * 4f;
		}

		public bool TryGetPosition(MissionInstance instance, out Vector3 outPosition, int depth = 0)
		{
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_028e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0293: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_021e: Unknown result type (might be due to invalid IL or missing references)
			//IL_022d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0239: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0439: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0471: Unknown result type (might be due to invalid IL or missing references)
			//IL_0476: Unknown result type (might be due to invalid IL or missing references)
			//IL_03da: Unknown result type (might be due to invalid IL or missing references)
			//IL_03df: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_04be: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("PositionGenerator.TryGetPosition"))
			{
				if (depth > 10)
				{
					Debug.Log((object)$"Exceeded max depth while calculating position, mission: {((Object)instance.GetMission()).name} missionID: {instance.missionID}, identifier: {identifier}");
					outPosition = Vector3.zero;
					return false;
				}
				Vector3 point;
				bool num = TryGetRelativeToPosition(instance, depth, out point);
				outPosition = point;
				if (!num)
				{
					Debug.LogError((object)("Failed to get relative to position for mission " + ((Object)instance.GetMission()).name), (Object)(object)instance.GetMission());
					return false;
				}
				switch (positionType)
				{
				case PositionType.MissionPoint:
				{
					List<Vector3> points = Pool.Get<List<Vector3>>();
					bool missionPoints = MissionPoint.GetMissionPoints(ref points, point, minDistForMovePoint_sqr, maxDistForMovePoint_sqr, (int)Flags, (int)ExclusionFlags);
					if (!missionPoints && allowDoubleDistanceIfNoOptionsAreFound)
					{
						points.Clear();
						missionPoints = MissionPoint.GetMissionPoints(ref points, point, minDistForMovePoint_sqr, maxDistForMovePoint_2x_sqr, (int)Flags, (int)ExclusionFlags);
					}
					if (missionPoints)
					{
						outPosition = points[Random.Range(0, points.Count)];
					}
					Pool.FreeUnmanaged<Vector3>(ref points);
					return missionPoints;
				}
				case PositionType.WorldPositionGenerator:
					if ((Object)(object)worldPositionGenerator != (Object)null)
					{
						bool flag = false;
						if (worldPositionGenerator.TrySample(point, minDistForMovePoint, maxDistForMovePoint, minDistForMovePoint_2x, maxDistForMovePoint_2x, out var position3))
						{
							outPosition = position3;
							flag = true;
						}
						if (!flag && allowDoubleDistanceIfNoOptionsAreFound && worldPositionGenerator.TrySample(point, minDistForMovePoint, maxDistForMovePoint_2x, minDistForMovePoint_2x, maxDistForMovePoint_4x, out position3))
						{
							outPosition = position3;
							flag = true;
						}
						return flag;
					}
					goto default;
				case PositionType.DungeonPoint:
					outPosition = DynamicDungeon.GetNextDungeonPoint();
					return true;
				case PositionType.Radius:
				{
					for (int m = 0; m < 10; m++)
					{
						Vector3 onUnitSphere = Random.onUnitSphere;
						onUnitSphere.y = 0f;
						((Vector3)(ref onUnitSphere)).Normalize();
						Vector3 val2 = point + onUnitSphere * Random.Range(minDistForMovePoint, maxDistForMovePoint);
						val2.y = WaterLevel.GetWaterOrTerrainSurface(val2, waves: false, volumes: false);
						if (TryAlignToGround(val2, out var correctedPosition))
						{
							outPosition = correctedPosition;
							return true;
						}
					}
					return false;
				}
				case PositionType.UnderwaterLab:
				{
					List<DungeonBaseInfo> dungeonBaseEntrances2 = TerrainMeta.Path.DungeonBaseEntrances;
					if (dungeonBaseEntrances2.Count > 0)
					{
						int index2 = Random.Range(0, dungeonBaseEntrances2.Count);
						outPosition = ((Component)dungeonBaseEntrances2[index2]).transform.position;
						return true;
					}
					return false;
				}
				case PositionType.UnderwaterLabWithBoomboxes:
				{
					List<DungeonBaseInfo> dungeonBaseEntrances = TerrainMeta.Path.DungeonBaseEntrances;
					int count = dungeonBaseEntrances.Count;
					if (count <= 0)
					{
						return false;
					}
					BaseMission mission = instance.GetMission();
					if (mission == null)
					{
						Debug.LogError((object)$"Failed to retrieve mission from mission instance with ID {instance.missionID}");
						return false;
					}
					MissionObjective_UnderwaterLabsBoomboxBonus missionObjective_UnderwaterLabsBoomboxBonus = null;
					for (int i = 0; i < mission.objectives.Length; i++)
					{
						if (mission.objectives[i].Get() is MissionObjective_UnderwaterLabsBoomboxBonus missionObjective_UnderwaterLabsBoomboxBonus2)
						{
							missionObjective_UnderwaterLabsBoomboxBonus = missionObjective_UnderwaterLabsBoomboxBonus2;
							break;
						}
					}
					if ((Object)(object)missionObjective_UnderwaterLabsBoomboxBonus == (Object)null)
					{
						Debug.LogError((object)("Failed to find lab boombox objective in mission " + ((Object)mission).name), (Object)(object)mission);
						return false;
					}
					int num2 = 0;
					if (string.IsNullOrWhiteSpace(missionObjective_UnderwaterLabsBoomboxBonus.requireProximityToPosition))
					{
						using (TimeWarning.New("PositionGenerator.TryGetPosition.UnderwaterLabWithBoomboxes.GetValidLabDistanceIrrespective"))
						{
							for (int j = 0; j < DeployableBoomBox.ServerStaticInstances.Count; j++)
							{
								DeployableBoomBox deployableBoomBox = DeployableBoomBox.ServerStaticInstances[j];
								if (!((Object)(object)deployableBoomBox == (Object)null))
								{
									if (EnvironmentManager.Check(((Component)deployableBoomBox).transform.position, EnvironmentType.UnderwaterLab))
									{
										num2++;
									}
									if (num2 >= 2)
									{
										int index = Random.Range(0, count);
										outPosition = ((Component)dungeonBaseEntrances[index]).transform.position;
										return true;
									}
								}
							}
						}
					}
					else
					{
						using (TimeWarning.New("PositionGenerator.TryGetPosition.UnderwaterLabWithBoomboxes.GetValidLab"))
						{
							ListHashSet<Vector3> val = Pool.Get<ListHashSet<Vector3>>();
							for (int k = 0; k < count; k++)
							{
								val.Add(((Component)dungeonBaseEntrances[k]).transform.position);
							}
							while (val.Count > 0)
							{
								int num3 = Random.Range(0, val.Count);
								Vector3 position = ((Component)dungeonBaseEntrances[num3]).transform.position;
								for (int l = 0; l < DeployableBoomBox.ServerStaticInstances.Count; l++)
								{
									using (TimeWarning.New("PositionGenerator.TryGetPosition.UnderwaterLabWithBoomboxes.CheckBoombox"))
									{
										DeployableBoomBox deployableBoomBox2 = DeployableBoomBox.ServerStaticInstances[l];
										if ((Object)(object)deployableBoomBox2 == (Object)null)
										{
											continue;
										}
										Vector3 position2 = ((Component)deployableBoomBox2).transform.position;
										float sqrMinimumDistanceToMissionPoint = missionObjective_UnderwaterLabsBoomboxBonus.sqrMinimumDistanceToMissionPoint;
										if (!(Vector3.SqrMagnitude(position - position2) > sqrMinimumDistanceToMissionPoint))
										{
											if (EnvironmentManager.Check(((Component)deployableBoomBox2).transform.position, EnvironmentType.UnderwaterLab))
											{
												num2++;
											}
											if (num2 >= 2)
											{
												outPosition = position;
												Pool.FreeUnmanaged<Vector3>(ref val);
												return true;
											}
										}
									}
								}
								num2 = 0;
								val.RemoveAt(num3);
							}
							Pool.FreeUnmanaged<Vector3>(ref val);
						}
					}
					return false;
				}
				default:
					Debug.LogError((object)$"Unhandled position generator type ({positionType}), defaulting to use {PositionType.Radius}");
					goto case PositionType.Radius;
				}
			}
		}

		private bool TryGetRelativeToPosition(MissionInstance instance, int depth, out Vector3 point)
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("PositionGenerator.TryGetRelativeToPosition"))
			{
				switch (relativeTo)
				{
				case RelativeType.Position:
					if (instance.TryGetMissionPoint(centerOnPositionIdentifier, out point, depth + 1))
					{
						return true;
					}
					break;
				case RelativeType.Provider:
				{
					IMissionProvider missionProvider = instance.GetMissionProvider();
					if (missionProvider != null)
					{
						point = missionProvider.ProviderPosition();
						return true;
					}
					break;
				}
				}
				point = Vector3.zero;
				Debug.LogError((object)string.Format("Failed to get point for {0} {1}, outputting {2} as a fallback", "RelativeType", relativeTo, point));
				return false;
			}
		}

		public static bool TryAlignToGround(Vector3 wishPosition, out Vector3 correctedPosition)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("BaseMission.PositionGenerator.TryAlignToGround"))
			{
				Vector3 val = Vector3Ex.WithY(wishPosition, wishPosition.y + 50f);
				RaycastHit hit = default(RaycastHit);
				if (!Physics.Raycast(new Ray(val, Vector3.down), ref hit, 50f, 1218652417, (QueryTriggerInteraction)1))
				{
					correctedPosition = wishPosition;
					return true;
				}
				if ((Object)(object)RaycastHitEx.GetEntity(hit) != (Object)null)
				{
					correctedPosition = wishPosition;
					return false;
				}
				correctedPosition = ((RaycastHit)(ref hit)).point;
				return true;
			}
		}
	}

	[Serializable]
	public class MissionDependancy
	{
		[FormerlySerializedAs("targetMission")]
		public BaseMission mission;

		[FormerlySerializedAs("targetMissionDesiredStatus")]
		[FilteredEnum(0, 4)]
		public MissionStatus desiredStatus;

		public uint missionID
		{
			get
			{
				if (!(mission == null))
				{
					return mission.id;
				}
				return 0u;
			}
		}
	}

	public enum MissionStatus
	{
		Undefined,
		Active,
		Accomplished,
		Failed,
		Completed,
		Pending
	}

	public enum MissionEventType
	{
		CUSTOM,
		HARVEST,
		CONVERSATION,
		KILL_ENTITY,
		ACQUIRE_ITEM,
		FREE_CRATE,
		MOUNT_ENTITY,
		HURT_ENTITY,
		PLAYER_TICK,
		CRAFT_ITEM,
		DEPLOY,
		HEAL,
		CLOTHINGCHANGED,
		STARTOVEN,
		CONSUME,
		ACQUITE_ITEM_STACK,
		OPEN_STORAGE,
		COOK,
		ENTER_TRIGGER,
		UPGRADE_BUILDING_GRADE,
		RESPAWN,
		METAL_DETECTOR_FIND,
		LONG_USE_OBJECT,
		PLAY_BOOMBOX
	}

	[Serializable]
	public class MissionObjectiveEntry
	{
		public Phrase description;

		public bool startAfterPriorObjectives;

		public int[] startAfterCompletedObjectives;

		public int[] autoCompleteOtherObjectives;

		public bool onlyProgressIfStarted = true;

		public bool isRequired = true;

		public MissionObjective objective;

		public string[] requiredEntities;

		public List<MissionBonusReward> bonusRewards;

		public MissionObjective Get()
		{
			return objective;
		}
	}

	public struct MissionEventPayload
	{
		public NetworkableId NetworkIdentifier;

		public uint UintIdentifier;

		public int IntIdentifier;

		public Vector3 WorldPosition;

		public string StringIdentifier;
	}

	public struct MissionIdentifierData(BaseMission mission, NetworkableId missionProviderNetId) : IEquatable<MissionIdentifierData>
	{
		public BaseMission mission = mission;

		public NetworkableId missionProviderNetId = missionProviderNetId;

		public bool Equals(MissionIdentifierData other)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			if (mission.id == other.mission.id)
			{
				return missionProviderNetId == other.missionProviderNetId;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is MissionIdentifierData other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(mission.id, missionProviderNetId.Value);
		}
	}

	public struct MissionValidStateData(MissionInstance missionInstance, bool isValid, int lastUpdateFrame, float lastUpdateTime)
	{
		public MissionInstance missionInstance = missionInstance;

		public bool isValid = isValid;

		public int lastUpdateFrame = lastUpdateFrame;

		public float lastUpdateTime = lastUpdateTime;
	}

	[Serializable]
	public class MissionEntityEntry
	{
		[FormerlySerializedAs("entityIdentifier")]
		public string identifier;

		public GameObjectRef entityRef;

		[PositionGenerator.PositionPoint]
		public string spawnPositionToUse;

		public bool spawnOnMissionStart = true;

		public bool cleanupOnMissionFailed;

		public bool cleanupOnMissionSuccess;

		public ItemAmount[] overrideLootOnItem;
	}

	[Serializable]
	public class MissionReward
	{
		public RewardType RewardType;

		public ItemAmount Item;

		public NonItemReward NonItem;

		public int GetRequiredInventorySlots()
		{
			return Mathf.CeilToInt(Item.GetAmount() / (float)Item.itemDef.stackable);
		}
	}

	[Serializable]
	public class MissionBonusReward : MissionReward
	{
		[Tooltip("If enabled, this reward will be dispensed for each amount the player got towards the target. Otherwise, this reward will only be dispensed once if the objective is fully complete.\nExample: if player achieved 2/3 for this bonus objective, then this reward will be dispensed 2 times.")]
		public bool isIncremental;
	}

	[Serializable]
	public class RewardsList
	{
		public List<MissionReward> rewards = new List<MissionReward>();
	}

	[Serializable]
	public class EraSpecificRewards
	{
		public Era[] eras;

		public List<RewardsList> rewardChoices = new List<RewardsList>();
	}

	[Serializable]
	public class NonItemReward
	{
		public Phrase DisplayPhrase;

		public Sprite DisplaySprite;

		public NonItemRewardType RewardType;
	}

	public enum NonItemRewardType
	{
		None,
		SafeZoneRespawnUnlock
	}

	public enum RewardType
	{
		Item,
		Other
	}

	public enum MissionFailReason
	{
		TimeOut,
		Disconnect,
		ResetPlayerState,
		Abandon,
		ObjectiveFailed,
		DeepSeaClosed
	}

	public class UpdateMissionValidStateWorkQueue : ObjectWorkQueue<MissionIdentifierData>
	{
		protected override void RunJob(MissionIdentifierData missionIdentifierData)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			queueProfilerRecorder = ServerProfiler.RecordScope("updateMissionValidStateWorkQueue", shouldProfileNextWorkQueueRun);
			shouldProfileNextWorkQueueRun = false;
			missionIdentifierData.mission.Server_UpdateMissionValidState(missionIdentifierData.missionProviderNetId, out var _);
			Server_SendValidMissionStatesIfWorkQueueComplete();
		}

		protected override bool IsValidToRun(MissionIdentifierData entity)
		{
			return true;
		}
	}

	public static readonly Phrase missionFailedPhrase = new Phrase("missionfailed.message", "You have failed the mission: {0}. Reason: {1}");

	public static readonly Phrase missionFailedReason_Timeout = new Phrase("missionfailed.reason.timeout", "Mission timeout");

	public static readonly Phrase missionFailedReason_Disconnect = new Phrase("missionfailed.reason.disconnect", "Disconnected");

	public static readonly Phrase missionFailedReason_PlayerStateReset = new Phrase("missionfailed.reason.playerstatereset", "Player state reset");

	public static readonly Phrase missionFailedReason_Abandon = new Phrase("missionfailed.reason.abandon", "Mission abandoned");

	public static readonly Phrase missionFailedReason_ObjectiveFailed = new Phrase("missionfailed.reason.objectivefailed", "Objective failed");

	public static readonly Phrase missionFailedReason_DeepSeaClosed = new Phrase("missionfailed.reason.deepseaclosed", "Deep sea closed");

	[ServerVar(Help = "(Generated) When enabled, missions are available and can be assigned to players; disable to globally suppress mission generation and assignment on the server")]
	public static bool missionsenabled = true;

	public static Dictionary<MissionIdentifierData, MissionValidStateData> server_missionInstanceValidStates = new Dictionary<MissionIdentifierData, MissionValidStateData>();

	public string shortname;

	private uint _id;

	private string previousShortname;

	public Phrase missionName;

	public Phrase missionDesc;

	public bool canBeAbandoned = true;

	public bool completeSilently;

	public string[] requiredGameModeTags = Array.Empty<string>();

	public MissionObjectiveEntry[] objectives;

	public static Dictionary<MissionInstance, ListHashSet<Vector3>> blockedPoints = new Dictionary<MissionInstance, ListHashSet<Vector3>>();

	public GameObjectRef acceptEffect;

	public GameObjectRef failedEffect;

	public GameObjectRef victoryEffect;

	public BaseMission followupMission;

	public int repeatDelaySecondsSuccess = -1;

	public int repeatDelaySecondsFailed = -1;

	public int timeLimitSeconds;

	[FormerlySerializedAs("hideStagesNotStarted")]
	public bool hideObjectivesNotStarted;

	[FormerlySerializedAs("acceptDependancies")]
	public MissionDependancy[] prerequisiteMissions;

	[FormerlySerializedAs("missionEntities")]
	public MissionEntityEntry[] spawnMissionEntityDefinitions;

	public PositionGenerator[] positionGenerators;

	private Dictionary<string, PositionGenerator> positionGeneratorMap;

	public List<RewardsList> defaultRewardChoices = new List<RewardsList>();

	public List<EraSpecificRewards> eraSpecificRewardChoices = new List<EraSpecificRewards>();

	[ServerVar(Help = "How long per frame (ms) to spend processing updateMissionValidStateWorkQueue", Saved = true, ShowInAdminUI = true)]
	public static float missionValidStateWorkQueueBudget = 0.1f;

	[ServerVar(Help = "Minimum time (s) between starting runs of updateMissionValidStateWorkQueue", Saved = true, ShowInAdminUI = true)]
	public static float missionValidStateWorkQueueCooldown = 3f;

	[ServerVar(Help = "Minimum time (s) between revalidating individual missions via updateMissionValidStateWorkQueue", Saved = true, ShowInAdminUI = true)]
	public static float missionPerValidStateCooldown = 3f;

	public static UpdateMissionValidStateWorkQueue updateMissionValidStateWorkQueue = new UpdateMissionValidStateWorkQueue();

	private static ListHashSet<MissionIdentifierData> validStatesToProcess = new ListHashSet<MissionIdentifierData>();

	private static ListHashSet<BasePlayer> playersRequestingValidStatesUpdate = new ListHashSet<BasePlayer>();

	private static float lastServerValidMissionsUpdateTime;

	private static ServerProfiler.ScopeRecorder queueProfilerRecorder;

	private static bool shouldProfileNextWorkQueueRun = false;

	public uint id
	{
		get
		{
			if (previousShortname != shortname)
			{
				previousShortname = shortname;
				_id = StringEx.ManifestHash(shortname);
			}
			return _id;
		}
	}

	public bool isRepeatable
	{
		get
		{
			if (repeatDelaySecondsSuccess < 0)
			{
				return repeatDelaySecondsFailed >= 0;
			}
			return true;
		}
	}

	public static Phrase GetPhraseForFailureReason(MissionFailReason reason)
	{
		return (Phrase)(reason switch
		{
			MissionFailReason.TimeOut => missionFailedReason_Timeout, 
			MissionFailReason.Disconnect => missionFailedReason_Disconnect, 
			MissionFailReason.ResetPlayerState => missionFailedReason_PlayerStateReset, 
			MissionFailReason.Abandon => missionFailedReason_Abandon, 
			MissionFailReason.ObjectiveFailed => missionFailedReason_ObjectiveFailed, 
			MissionFailReason.DeepSeaClosed => missionFailedReason_DeepSeaClosed, 
			_ => Phrase.op_Implicit($"Unhandled reason: {reason}"), 
		});
	}

	public static void PlayerDisconnected(BasePlayer player)
	{
		if (!player.IsNpc && player.TryGetActiveMissionInstance(out var instance))
		{
			BaseMission mission = instance.GetMission();
			if (mission.spawnMissionEntityDefinitions.Length != 0)
			{
				mission.MissionFailed(instance, player, MissionFailReason.Disconnect);
			}
		}
	}

	private void OnEnable()
	{
		PositionGenerator[] array = positionGenerators;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].CacheDistanceValues();
		}
	}

	public bool TryGetPositionGenerator(string identifier, out PositionGenerator positionGenerator)
	{
		if (positionGeneratorMap == null)
		{
			positionGeneratorMap = new Dictionary<string, PositionGenerator>();
			PositionGenerator[] array = positionGenerators;
			foreach (PositionGenerator positionGenerator2 in array)
			{
				positionGeneratorMap.Add(positionGenerator2.identifier, positionGenerator2);
			}
		}
		positionGenerator = null;
		return positionGeneratorMap.TryGetValue(identifier, out positionGenerator);
	}

	public List<RewardsList> GetRewardChoices()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		if ((int)ConVar.Server.Era == 0)
		{
			return defaultRewardChoices;
		}
		if (eraSpecificRewardChoices != null)
		{
			foreach (EraSpecificRewards eraSpecificRewardChoice in eraSpecificRewardChoices)
			{
				if (eraSpecificRewardChoice.eras == null)
				{
					continue;
				}
				Era[] eras = eraSpecificRewardChoice.eras;
				foreach (Era val in eras)
				{
					if ((val == ConVar.Server.Era || (int)val == 1) && eraSpecificRewardChoice.rewardChoices != null)
					{
						return eraSpecificRewardChoice.rewardChoices;
					}
				}
			}
		}
		return defaultRewardChoices;
	}

	public bool HasRewards()
	{
		List<RewardsList> rewardChoices = GetRewardChoices();
		for (int i = 0; i < rewardChoices.Count; i++)
		{
			if (rewardChoices[i].rewards.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetRewardsForChoice(int choice, out List<MissionReward> rewards)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Invalid comparison between Unknown and I4
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		rewards = null;
		if ((int)ConVar.Server.Era == 0)
		{
			return TryGetDefaultRewardsForChoice(choice, out rewards);
		}
		if (eraSpecificRewardChoices != null)
		{
			foreach (EraSpecificRewards eraSpecificRewardChoice in eraSpecificRewardChoices)
			{
				if (eraSpecificRewardChoice.eras == null)
				{
					continue;
				}
				Era[] eras = eraSpecificRewardChoice.eras;
				foreach (Era val in eras)
				{
					if ((val == ConVar.Server.Era || (int)val == 1) && eraSpecificRewardChoice.rewardChoices != null)
					{
						if (choice < 0 || choice >= eraSpecificRewardChoice.rewardChoices.Count)
						{
							Debug.LogError((object)string.Format("Cannot retrieve {0} era mission rewards from mission {1} for choice ({2}) as this is out of bounds of choices count ({3})", new object[4]
							{
								val,
								((Object)this).name,
								choice,
								eraSpecificRewardChoice.rewardChoices.Count
							}), (Object)(object)this);
							return false;
						}
						rewards = eraSpecificRewardChoice.rewardChoices[choice].rewards;
						return rewards != null && rewards.Count > 0;
					}
				}
			}
		}
		return TryGetDefaultRewardsForChoice(choice, out rewards);
	}

	private bool TryGetDefaultRewardsForChoice(int choice, out List<MissionReward> rewards)
	{
		rewards = null;
		if (defaultRewardChoices == null || defaultRewardChoices.Count == 0)
		{
			return false;
		}
		if (choice < 0 || choice >= defaultRewardChoices.Count)
		{
			Debug.LogError((object)$"Cannot retrieve mission rewards from mission {((Object)this).name} for choice ({choice}) as this is out of bounds of choices count ({defaultRewardChoices.Count})", (Object)(object)this);
			return false;
		}
		rewards = defaultRewardChoices[choice].rewards;
		if (rewards != null)
		{
			return rewards.Count > 0;
		}
		return false;
	}

	[ServerVar(Help = "Generate a performance capture containing the next run of updateMissionValidStateWorkQueue")]
	public static void profileNextMissionsValidStateWorkQueue(ConsoleSystem.Arg arg)
	{
		shouldProfileNextWorkQueueRun = true;
	}

	public static void PlayerRequestedValidStatesUpdate(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return;
		}
		if (validStatesToProcess.Count > 0)
		{
			playersRequestingValidStatesUpdate.TryAdd(player);
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup - lastServerValidMissionsUpdateTime > missionValidStateWorkQueueCooldown)
		{
			lastServerValidMissionsUpdateTime = realtimeSinceStartup;
			playersRequestingValidStatesUpdate.TryAdd(player);
			Server_StartMissionValidStateWorkQueue();
		}
		else
		{
			player.Server_SendValidMissionStates();
		}
	}

	private static void Server_StartMissionValidStateWorkQueue()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < NPCTalking.serverMissionProviders.Count; i++)
		{
			IMissionProvider missionProvider = NPCTalking.serverMissionProviders[i];
			if (ObjectEx.IsUnityNull(missionProvider) || missionProvider is TutorialNPC)
			{
				continue;
			}
			BufferList<BaseMission> allMissions = missionProvider.GetAllMissions();
			int count = allMissions.Count;
			if (count <= 0)
			{
				continue;
			}
			for (int j = 0; j < count; j++)
			{
				BaseMission mission = allMissions[j];
				MissionIdentifierData missionIdentifierData = new MissionIdentifierData(mission, missionProvider.ProviderID());
				bool flag = true;
				if (server_missionInstanceValidStates.TryGetValue(missionIdentifierData, out var value))
				{
					flag = Time.realtimeSinceStartup - value.lastUpdateTime > missionPerValidStateCooldown;
				}
				if (flag && validStatesToProcess.TryAdd(missionIdentifierData))
				{
					((ObjectWorkQueue<MissionIdentifierData>)updateMissionValidStateWorkQueue).Add(missionIdentifierData);
				}
			}
		}
	}

	private static void Server_SendValidMissionStatesIfWorkQueueComplete()
	{
		if (validStatesToProcess.Count > 0)
		{
			return;
		}
		queueProfilerRecorder.Dispose();
		for (int i = 0; i < playersRequestingValidStatesUpdate.Count; i++)
		{
			BasePlayer basePlayer = playersRequestingValidStatesUpdate[i];
			if ((Object)(object)basePlayer != (Object)null && basePlayer.IsConnected && !basePlayer.IsSleeping())
			{
				basePlayer.Server_SendValidMissionStates();
			}
		}
		playersRequestingValidStatesUpdate.Clear();
	}

	public void Server_UpdateMissionValidState(NetworkableId providerNetId, out bool isValid)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BaseMission.Server_IsMissionValid"))
		{
			MissionIdentifierData missionIdentifierData = new MissionIdentifierData(this, providerNetId);
			validStatesToProcess.Remove(missionIdentifierData);
			if (server_missionInstanceValidStates.TryGetValue(missionIdentifierData, out var value))
			{
				if (value.lastUpdateFrame == Time.frameCount)
				{
					isValid = value.isValid;
					return;
				}
				Pool.Free<MissionInstance>(ref value.missionInstance);
			}
			MissionInstance missionInstance = Pool.Get<MissionInstance>();
			missionInstance.providerID = new NetworkableId(providerNetId.Value);
			missionInstance.missionID = id;
			missionInstance.status = MissionStatus.Pending;
			server_missionInstanceValidStates[missionIdentifierData] = new MissionValidStateData(missionInstance, isValid: false, Time.frameCount, Time.realtimeSinceStartup);
			isValid = false;
			if (!missionsenabled)
			{
				return;
			}
			using (TimeWarning.New("BaseMission.Server_IsMissionValid - gamemode"))
			{
				BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
				if ((Object)(object)activeGameMode != (Object)null && (activeGameMode.HasBlockedMission(this) || (requiredGameModeTags.Length != 0 && !activeGameMode.HasAnyGameModeTag(requiredGameModeTags))))
				{
					return;
				}
			}
			using (TimeWarning.New("BaseMission.Server_IsMissionValid - positionGenerators"))
			{
				PositionGenerator[] array = positionGenerators;
				foreach (PositionGenerator positionGenerator in array)
				{
					if (!positionGenerator.TryGetPosition(missionInstance, out var outPosition))
					{
						blockedPoints.Remove(missionInstance);
						return;
					}
					missionInstance.missionPoints.Add(positionGenerator.identifier, outPosition);
					if (positionGenerator.positionsAreExclusive)
					{
						AddPositionBlocker(missionInstance, outPosition);
					}
				}
			}
			using (TimeWarning.New("BaseMission.Server_IsMissionValid - objectives"))
			{
				for (int j = 0; j < objectives.Length; j++)
				{
					if (!objectives[j].Get().IsObjectiveValid(j, missionInstance))
					{
						blockedPoints.Remove(missionInstance);
						return;
					}
				}
			}
			server_missionInstanceValidStates[missionIdentifierData] = new MissionValidStateData(missionInstance, isValid: true, Time.frameCount, Time.realtimeSinceStartup);
			isValid = true;
		}
	}

	public static void AddPositionBlocker(MissionInstance missionInstance, Vector3 point)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (missionInstance.status != MissionStatus.Pending && !missionInstance.IsActive())
		{
			Debug.LogError((object)$"Cannot add point {point} for mission {((Object)missionInstance.GetMission()).name} to blocked points due to status being: {missionInstance.status}", (Object)(object)missionInstance.GetMission());
			return;
		}
		if (!blockedPoints.TryGetValue(missionInstance, out var value) || value == null)
		{
			blockedPoints[missionInstance] = Pool.Get<ListHashSet<Vector3>>();
		}
		blockedPoints[missionInstance].TryAdd(point);
	}

	public static void RemovePositionBlockers(MissionInstance missionInstance)
	{
		if (blockedPoints.TryGetValue(missionInstance, out var value))
		{
			Pool.FreeUnmanaged<Vector3>(ref value);
			blockedPoints.Remove(missionInstance);
		}
	}

	public static void DoMissionEffect(string effectString, BasePlayer assignee)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Effect effect = new Effect();
		effect.Init(Effect.Type.Generic, assignee, StringPool.Get("head"), Vector3.zero, Vector3.forward);
		effect.pooledString = effectString;
		EffectNetwork.Send(effect, assignee.net.connection);
	}

	public virtual void MissionStart(MissionInstance instance, BasePlayer assignee)
	{
		if (Interface.CallHook("OnMissionStart", this, instance, assignee) != null)
		{
			return;
		}
		for (int i = 0; i < objectives.Length; i++)
		{
			objectives[i].Get().MissionStarted(i, instance, assignee);
		}
		if (acceptEffect.isValid)
		{
			DoMissionEffect(acceptEffect.resourcePath, assignee);
		}
		MissionEntityEntry[] array = spawnMissionEntityDefinitions;
		foreach (MissionEntityEntry missionEntityEntry in array)
		{
			if (missionEntityEntry.spawnOnMissionStart)
			{
				instance.GetSpawnedMissionEntity(missionEntityEntry.identifier, assignee);
			}
		}
		Interface.CallHook("OnMissionStarted", this, instance, assignee);
	}

	public void CheckObjectives(MissionInstance instance, BasePlayer assignee)
	{
		if (instance.status != MissionStatus.Active)
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < objectives.Length; i++)
		{
			if (instance.objectiveStatuses[i].failed && objectives[i].isRequired)
			{
				MissionFailed(instance, assignee, MissionFailReason.ObjectiveFailed);
				break;
			}
			if (flag && objectives[i].isRequired && (!instance.objectiveStatuses[i].completed || instance.objectiveStatuses[i].failed))
			{
				flag = false;
			}
		}
		if (flag)
		{
			MissionSuccess(instance, assignee);
		}
	}

	public virtual void ServerThink(MissionInstance instance, BasePlayer assignee, float timeSinceLastThink)
	{
		for (int i = 0; i < objectives.Length; i++)
		{
			objectives[i].Get().ServerThink(i, instance, assignee, timeSinceLastThink);
		}
		CheckObjectives(instance, assignee);
		assignee.SaveMissionsIfDirty();
	}

	public virtual void MissionSuccess(MissionInstance instance, BasePlayer assignee)
	{
		instance.status = MissionStatus.Accomplished;
		MissionEnded(instance, assignee);
		MissionComplete(instance, assignee);
		Interface.CallHook("OnMissionSucceeded", this, instance, assignee);
	}

	public virtual void MissionComplete(MissionInstance instance, BasePlayer assignee)
	{
		DoMissionEffect(victoryEffect.resourcePath, assignee);
		if (!instance.GetMission().completeSilently)
		{
			assignee.ChatMessage("You have completed the mission : " + missionName.english);
		}
		BaseMission mission = instance.GetMission();
		if (mission != null)
		{
			if (mission.GetRewardChoices().Count == 1)
			{
				DispenseRewards(instance, assignee, 0);
			}
			for (int i = 0; i < objectives.Length; i++)
			{
				MissionObjectiveEntry missionObjectiveEntry = objectives[i];
				MissionInstance.ObjectiveStatus objectiveStatus = instance.objectiveStatuses[i];
				if (missionObjectiveEntry.isRequired || missionObjectiveEntry.bonusRewards == null || objectiveStatus.failed)
				{
					continue;
				}
				foreach (MissionBonusReward bonusReward in missionObjectiveEntry.bonusRewards)
				{
					if (bonusReward.isIncremental && objectiveStatus.progressCurrent > 0f && objectiveStatus.progressTarget > 0f)
					{
						int num = Mathf.FloorToInt(objectiveStatus.progressCurrent);
						for (int j = 0; j < num; j++)
						{
							DispenseReward(assignee, bonusReward);
						}
					}
					else if (instance.objectiveStatuses[i].completed)
					{
						DispenseReward(assignee, bonusReward);
					}
				}
			}
		}
		Facepunch.Rust.Analytics.Azure.OnMissionComplete(assignee, this);
		instance.status = MissionStatus.Completed;
		assignee.SetActiveMissionIndex(-1);
		assignee.MissionsDirty(saveImmediately: true);
		if (followupMission != null)
		{
			IMissionProvider missionProvider = instance.GetMissionProvider();
			if (missionProvider == null)
			{
				Debug.LogError((object)("Failed to retrieve mission provider on instance for mission " + ((Object)instance.GetMission()).name));
			}
			else
			{
				assignee.RegisterFollowupMission(followupMission, missionProvider);
			}
		}
		if (Rust.GameInfo.HasAchievements && mission != null && !(mission is TutorialMission))
		{
			assignee.stats.Add("missions_completed", 1, Stats.All);
			assignee.stats.Save(forceSteamSave: true);
		}
		if (assignee.IsInTutorial)
		{
			TutorialIsland currentTutorialIsland = assignee.GetCurrentTutorialIsland();
			if ((Object)(object)currentTutorialIsland != (Object)null && currentTutorialIsland.FinalMission == this)
			{
				currentTutorialIsland.StartEndingCinematic(assignee);
			}
		}
		if (!completeSilently)
		{
			assignee.ClientRPC(RpcTarget.Player("Client_MissionComplete", assignee), id);
		}
	}

	public virtual void DispenseRewards(MissionInstance instance, BasePlayer assignee, int choice)
	{
		MissionStatus status = instance.status;
		bool flag = status == MissionStatus.Accomplished || status == MissionStatus.Completed;
		if (instance.hasDispensedRewards || !flag)
		{
			return;
		}
		instance.hasDispensedRewards = true;
		assignee.MissionsDirty();
		if (TryGetRewardsForChoice(choice, out var rewards))
		{
			for (int i = 0; i < rewards.Count; i++)
			{
				MissionReward reward = rewards[i];
				DispenseReward(assignee, reward);
			}
		}
	}

	private void DispenseReward(BasePlayer assignee, MissionReward reward)
	{
		switch (reward.RewardType)
		{
		case RewardType.Item:
			GiveItemReward(assignee, reward.Item);
			break;
		case RewardType.Other:
		{
			if (reward.NonItem.RewardType != NonItemRewardType.SafeZoneRespawnUnlock)
			{
				break;
			}
			using HashSet<StaticRespawnArea>.Enumerator enumerator = StaticRespawnArea.staticRespawnAreas.GetEnumerator();
			if (enumerator.MoveNext())
			{
				enumerator.Current.Authorize(assignee.userID);
				assignee.SendRespawnOptions();
			}
			break;
		}
		}
	}

	private void GiveItemReward(BasePlayer player, ItemAmount reward)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)reward.itemDef == (Object)null || reward.amount == 0f)
		{
			Debug.LogError((object)"BIG REWARD SCREWUP, NULL ITEM DEF");
			return;
		}
		if (!reward.itemDef.IsAllowed((EraRestriction)8))
		{
			Debug.LogError((object)$"Blocking mission reward '{reward.itemDef.shortname}' not allowed in era '{ConVar.Server.Era}'");
			return;
		}
		Item item = (reward.isBP ? ItemManager.Create(ItemManager.blueprintBaseDef, Mathf.CeilToInt(reward.amount), 0uL, isServerSide: true, 0uL) : ItemManager.Create(reward.itemDef, Mathf.CeilToInt(reward.amount), 0uL, isServerSide: true, 0uL));
		if (item == null)
		{
			return;
		}
		if (reward.isBP)
		{
			item.blueprintTarget = reward.itemDef.itemid;
		}
		int num = item.MaxStackable();
		if (num > 0)
		{
			while (item.amount > num)
			{
				Item item2 = item.SplitItem(item.MaxStackable());
				if (item2 == null)
				{
					break;
				}
				item2.SetItemOwnership(player, ItemOwnershipPhrases.MissionRewardPhrase);
				player.GiveItem(item2, BaseEntity.GiveItemReason.PickedUp);
			}
		}
		item.SetItemOwnership(player, ItemOwnershipPhrases.MissionRewardPhrase);
		player.GiveItem(item, BaseEntity.GiveItemReason.PickedUp);
	}

	public virtual void MissionFailed(MissionInstance instance, BasePlayer assignee, MissionFailReason failReason, bool saveImmediately = true)
	{
		if (!instance.GetMission().completeSilently)
		{
			assignee.Server_SendMissionFailed(instance.missionID, failReason);
			if (failReason != MissionFailReason.ResetPlayerState)
			{
				DoMissionEffect(failedEffect.resourcePath, assignee);
			}
		}
		Facepunch.Rust.Analytics.Azure.OnMissionComplete(assignee, this, failReason);
		instance.status = MissionStatus.Failed;
		MissionEnded(instance, assignee);
		Interface.CallHook("OnMissionFailed", this, instance, assignee, failReason);
		assignee.MissionsDirty(saveImmediately);
	}

	public virtual void MissionEnded(MissionInstance instance, BasePlayer assignee)
	{
		instance.endTimeUtcSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		if (instance.spawnedMissionEntities != null)
		{
			List<MissionEntity> list = Pool.Get<List<MissionEntity>>();
			foreach (MissionEntity value in instance.spawnedMissionEntities.Values)
			{
				if (!((Object)(object)value == (Object)null))
				{
					list.Add(value);
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				list[i].MissionEnded(assignee, instance);
			}
			Pool.FreeUnmanaged<MissionEntity>(ref list);
		}
		RemovePositionBlockers(instance);
		assignee.SetActiveMissionIndex(-1);
	}

	public void OnObjectiveCompleted(int objectiveIndex, MissionInstance instance, BasePlayer playerFor)
	{
		if (objectiveIndex < 0 || objectiveIndex >= objectives.Length)
		{
			Debug.LogError((object)$"Objective index {objectiveIndex} is invalid, mission {((Object)this).name} has {objectives.Length} objectives");
			return;
		}
		MissionObjectiveEntry missionObjectiveEntry = objectives[objectiveIndex];
		if (missionObjectiveEntry.autoCompleteOtherObjectives.Length != 0)
		{
			for (int i = 0; i < missionObjectiveEntry.autoCompleteOtherObjectives.Length; i++)
			{
				int num = missionObjectiveEntry.autoCompleteOtherObjectives[i];
				MissionObjectiveEntry missionObjectiveEntry2 = objectives[num];
				MissionInstance.ObjectiveStatus objectiveStatus = instance.objectiveStatuses[num];
				objectiveStatus.blockReset = true;
				if (!objectiveStatus.completed)
				{
					missionObjectiveEntry2.objective.CompleteObjective(num, instance, playerFor);
				}
			}
		}
		CheckObjectives(instance, playerFor);
	}

	public void OnObjectiveFailed(int objectiveIndex, MissionInstance instance, BasePlayer playerFor)
	{
		CheckObjectives(instance, playerFor);
	}

	public static bool AssignMission(BasePlayer assignee, IMissionProvider provider, BaseMission mission)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		if (!missionsenabled)
		{
			return false;
		}
		if (!assignee.Server_CanAcceptMission(provider.ProviderID(), mission))
		{
			return false;
		}
		object obj = Interface.CallHook("CanAssignMission", assignee, mission, provider);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (!server_missionInstanceValidStates.TryGetValue(new MissionIdentifierData(mission, provider.ProviderID()), out var value))
		{
			return false;
		}
		MissionInstance missionInstance = null;
		int num = -1;
		for (int i = 0; i < assignee.acceptedMissions.Count; i++)
		{
			MissionInstance missionInstance2 = assignee.acceptedMissions[i];
			if (mission.id == missionInstance2.missionID)
			{
				num = i;
				break;
			}
		}
		int activeMissionIndex;
		if (num >= 0)
		{
			missionInstance = assignee.acceptedMissions[num];
			activeMissionIndex = num;
			missionInstance.Reset();
		}
		else
		{
			missionInstance = Pool.Get<MissionInstance>();
			activeMissionIndex = assignee.acceptedMissions.Count;
			assignee.acceptedMissions.Add(missionInstance);
		}
		missionInstance.missionID = mission.id;
		missionInstance.startTimeUtcSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		missionInstance.providerID = provider.ProviderID();
		missionInstance.status = MissionStatus.Active;
		foreach (KeyValuePair<string, Vector3> missionPoint in value.missionInstance.missionPoints)
		{
			string key = missionPoint.Key;
			Vector3 value2 = missionPoint.Value;
			missionInstance.missionPoints.Add(key, value2);
			if (mission.TryGetPositionGenerator(key, out var positionGenerator) && positionGenerator.positionsAreExclusive)
			{
				AddPositionBlocker(missionInstance, value2);
			}
		}
		mission.Server_UpdateMissionValidState(provider.ProviderID(), out var _);
		for (int j = 0; j < mission.objectives.Length; j++)
		{
			MissionInstance.ObjectiveStatus objectiveStatus = Pool.Get<MissionInstance.ObjectiveStatus>();
			missionInstance.objectiveStatuses.Add(objectiveStatus);
		}
		if (missionInstance.objectiveStatuses.Count != mission.objectives.Length)
		{
			Debug.LogError((object)$"New mission instance for {((Object)mission).name} has {missionInstance.objectiveStatuses.Count} objective statuses but mission has {mission.objectives.Length} objectives", (Object)(object)mission);
		}
		mission.MissionStart(missionInstance, assignee);
		assignee.SetActiveMissionIndex(activeMissionIndex);
		assignee.MissionsDirty(saveImmediately: true);
		Interface.CallHook("OnMissionAssigned", mission, provider, assignee);
		return true;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using ConVar;
using Epic.OnlineServices.Version;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;

namespace Facepunch.Rust;

public static class Analytics
{
	public static class Azure
	{
		[JsonModel]
		private struct EntitySumItem
		{
			public uint PrefabId;

			public int Count;

			public int Grade;
		}

		private struct EntityKey : IEquatable<EntityKey>
		{
			public uint PrefabId;

			public int Grade;

			public bool Equals(EntityKey other)
			{
				if (PrefabId == other.PrefabId)
				{
					return Grade == other.Grade;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (17 * 23 + PrefabId.GetHashCode()) * 31 + Grade.GetHashCode();
			}
		}

		private class PendingItemsData : IPooled
		{
			public PendingItemsKey Key;

			public int amount;

			public string category;

			public void EnterPool()
			{
				Key = default(PendingItemsKey);
				amount = 0;
				category = null;
			}

			public void LeavePool()
			{
			}
		}

		private struct PendingItemsKey : IEquatable<PendingItemsKey>
		{
			public string Item;

			public bool Consumed;

			public string Entity;

			public string Category;

			public NetworkableId EntityId;

			public bool Equals(PendingItemsKey other)
			{
				//IL_0027: Unknown result type (might be due to invalid IL or missing references)
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				if (Item == other.Item && Entity == other.Entity && EntityId == other.EntityId && Consumed == other.Consumed)
				{
					return Category == other.Category;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return ((((17 * 23 + Item.GetHashCode()) * 31 + Consumed.GetHashCode()) * 37 + Entity.GetHashCode()) * 47 + Category.GetHashCode()) * 53 + ((object)Unsafe.As<NetworkableId, NetworkableId>(ref EntityId)/*cast due to constrained. prefix*/).GetHashCode();
			}
		}

		[JsonModel]
		private class PlayerAggregate : IPooled
		{
			public string UserId;

			public Vector3 Position;

			public Vector3 Direction;

			public List<string> Hotbar = new List<string>();

			public List<string> Worn = new List<string>();

			public string ActiveItem;

			public string Biome;

			public void EnterPool()
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				UserId = null;
				Position = default(Vector3);
				Direction = default(Vector3);
				Hotbar.Clear();
				Worn.Clear();
				ActiveItem = null;
				Biome = null;
			}

			public void LeavePool()
			{
			}
		}

		[JsonModel]
		private class TeamInfo : IPooled
		{
			public List<string> online = new List<string>();

			public List<string> offline = new List<string>();

			public int member_count;

			public void EnterPool()
			{
				online.Clear();
				offline.Clear();
				member_count = 0;
			}

			public void LeavePool()
			{
			}
		}

		public enum ResourceMode
		{
			Produced,
			Consumed
		}

		private static class EventIds
		{
			public const string EntityBuilt = "entity_built";

			public const string EntityPickup = "entity_pickup";

			public const string EntityDamage = "entity_damage";

			public const string PlayerRespawn = "player_respawn";

			public const string ExplosiveLaunched = "explosive_launch";

			public const string Explosion = "explosion";

			public const string ItemEvent = "item_event";

			public const string EntitySum = "entity_sum";

			public const string ItemSum = "item_sum";

			public const string ItemDespawn = "item_despawn";

			public const string ItemDropped = "item_drop";

			public const string ItemPickup = "item_pickup";

			public const string AntihackViolation = "antihack_violation";

			public const string AntihackViolationDetailed = "antihack_violation_detailed";

			public const string PlayerConnect = "player_connect";

			public const string PlayerDisconnect = "player_disconnect";

			public const string ConsumableUsed = "consumeable_used";

			public const string MedUsed = "med_used";

			public const string ResearchStarted = "research_start";

			public const string BlueprintLearned = "blueprint_learned";

			public const string TeamChanged = "team_change";

			public const string EntityAuthChange = "auth_change";

			public const string VendingOrderChanged = "vending_changed";

			public const string VendingSale = "vending_sale";

			public const string ChatMessage = "chat";

			public const string BlockUpgrade = "block_upgrade";

			public const string BlockDemolish = "block_demolish";

			public const string ItemRepair = "item_repair";

			public const string EntityRepair = "entity_repair";

			public const string ItemSkinned = "item_skinned";

			public const string EntitySkinned = "entity_skinned";

			public const string ItemAggregate = "item_aggregate";

			public const string CodelockChanged = "code_change";

			public const string CodelockEntered = "code_enter";

			public const string SleepingBagAssign = "sleeping_bag_assign";

			public const string FallDamage = "fall_damage";

			public const string PlayerWipeIdSet = "player_wipe_id_set";

			public const string ServerInfo = "server_info";

			public const string UnderwaterCrateUntied = "crate_untied";

			public const string VehiclePurchased = "vehicle_purchase";

			public const string NPCVendor = "npc_vendor";

			public const string BlueprintsOnline = "blueprint_aggregate_online";

			public const string PlayerPositions = "player_positions";

			public const string ProjectileInvalid = "projectile_invalid";

			public const string ItemDefinitions = "item_definitions";

			public const string KeycardSwiped = "keycard_swiped";

			public const string EntitySpawned = "entity_spawned";

			public const string EntityKilled = "entity_killed";

			public const string HackableCrateStarted = "hackable_crate_started";

			public const string HackableCrateEnded = "hackable_crate_ended";

			public const string StashHidden = "stash_hidden";

			public const string StashRevealed = "stash_reveal";

			public const string EntityManifest = "entity_manifest";

			public const string LootEntity = "loot_entity";

			public const string OnlineTeams = "online_teams";

			public const string Gambling = "gambing";

			public const string BuildingBlockColor = "building_block_color";

			public const string MissionComplete = "mission_complete";

			public const string PlayerPinged = "player_pinged";

			public const string BagUnclaim = "bag_unclaim";

			public const string SteamAuth = "steam_auth";

			public const string ParachuteUsed = "parachute_used";

			public const string MountEntity = "mount";

			public const string DismountEntity = "dismount";

			public const string BurstToggle = "burst_toggle";

			public const string TutorialStarted = "tutorial_started";

			public const string TutorialCompleted = "tutorial_completed";

			public const string TutorialQuit = "tutorial_quit";

			public const string BaseInteraction = "base_interaction";

			public const string PlayerDeath = "player_death";

			public const string CarShredded = "car_shredded";

			public const string PlayerTick = "player_tick";

			public const string WallpaperPlaced = "wallpaper_placed";

			public const string StartFish = "fishing_start";

			public const string FailedFish = "fishing_failed";

			public const string CaughtFish = "fishing_caught";

			public const string InjureStateChange = "injure_state";

			public const string LifeStoryEnd = "life_story_end";

			public const string ServerRPC = "server_rpc";

			public const string AdminCommand = "admin_command";

			public const string PuzzleReset = "puzzle_reset";

			public const string DeepSeaTraverse = "deep_sea_traverse";

			public const string DeepSeaToggle = "deep_sea_toggle";

			public const string PlayerBoatFinish = "player_boat_finish";
		}

		private struct SimpleItemAmount(Item item)
		{
			public string ItemName = item.info.shortname;

			public int Amount = item.amount;

			public ulong Skin = item.skin;

			public float Condition = item.conditionNormalized;
		}

		private struct FiredProjectileKey(ulong userId, int projectileId) : IEquatable<FiredProjectileKey>
		{
			public ulong UserId = userId;

			public int ProjectileId = projectileId;

			public bool Equals(FiredProjectileKey other)
			{
				if (other.UserId == UserId)
				{
					return other.ProjectileId == ProjectileId;
				}
				return false;
			}
		}

		private class PendingFiredProjectile : IPooled
		{
			public EventRecord Record;

			public BasePlayer.FiredProjectile FiredProjectile;

			public bool Hit;

			public void EnterPool()
			{
				Hit = false;
				Record = null;
				FiredProjectile = null;
			}

			public void LeavePool()
			{
			}
		}

		private static Dictionary<int, string> geneCache = new Dictionary<int, string>();

		public static int MaxMSPerFrame = 5;

		private static Dictionary<PendingItemsKey, PendingItemsData> pendingItems = new Dictionary<PendingItemsKey, PendingItemsData>();

		private static Dictionary<FiredProjectileKey, PendingFiredProjectile> trackedProjectiles = new Dictionary<FiredProjectileKey, PendingFiredProjectile>();

		public static bool GameplayAnalytics => GameplayAnalyticsConVar;

		private static string GetGenesAsString(GrowableEntity plant)
		{
			int key = GrowableGeneEncoding.EncodeGenesToInt(plant.Genes);
			if (!geneCache.TryGetValue(key, out var value))
			{
				return string.Join("", from x in plant.Genes.Genes
					group x by x.GetDisplayCharacter() into x
					orderby x.Key
					select x.Count() + x.Key);
			}
			return value;
		}

		private static string GetMonument(BaseEntity entity)
		{
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)entity == (Object)null)
			{
				return null;
			}
			SpawnGroup spawnGroup = null;
			if (entity is BaseCorpse baseCorpse)
			{
				spawnGroup = baseCorpse.spawnGroup;
			}
			if ((Object)(object)spawnGroup == (Object)null)
			{
				SpawnPointInstance component = ((Component)entity).GetComponent<SpawnPointInstance>();
				if ((Object)(object)component != (Object)null)
				{
					spawnGroup = component.parentSpawnPointUser as SpawnGroup;
				}
			}
			if ((Object)(object)spawnGroup != (Object)null)
			{
				if (!string.IsNullOrEmpty(spawnGroup.category))
				{
					return spawnGroup.category;
				}
				if ((Object)(object)spawnGroup.Monument != (Object)null)
				{
					return ((Object)spawnGroup.Monument).name;
				}
			}
			MonumentInfo monumentInfo = TerrainMeta.Path.FindMonumentWithBoundsOverlap(((Component)entity).transform.position);
			if ((Object)(object)monumentInfo != (Object)null)
			{
				return ((Object)monumentInfo).name;
			}
			return null;
		}

		private static string GetBiome(Vector3 position)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Invalid comparison between Unknown and I4
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Invalid comparison between Unknown and I4
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected I4, but got Unknown
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Invalid comparison between Unknown and I4
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Invalid comparison between Unknown and I4
			string result = null;
			Enum val = (Enum)TerrainMeta.BiomeMap.GetBiomeMaxType(position);
			if ((int)val <= 8)
			{
				switch (val - 1)
				{
				default:
					if ((int)val == 8)
					{
						result = "arctic";
					}
					break;
				case 0:
					result = "arid";
					break;
				case 1:
					result = "grass";
					break;
				case 3:
					result = "tundra";
					break;
				case 2:
					break;
				}
			}
			else if ((int)val != 16)
			{
				if ((int)val == 32)
				{
					result = "deepsea";
				}
			}
			else
			{
				result = "jungle";
			}
			return result;
		}

		private static bool IsOcean(Vector3 position)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return TerrainMeta.TopologyMap.GetTopology(position) == 128;
		}

		public static Dictionary<string, string> GetHardwareData()
		{
			string value;
			using (SHA256 sHA = SHA256.Create())
			{
				value = Convert.ToBase64String(sHA.ComputeHash(Encoding.UTF8.GetBytes(SystemInfo.deviceUniqueIdentifier)));
			}
			return new Dictionary<string, string>
			{
				["device_name"] = SystemInfo.deviceName,
				["device_hash"] = value,
				["gpu_name"] = SystemInfo.graphicsDeviceName,
				["gpu_ram"] = SystemInfo.graphicsMemorySize.ToString(),
				["gpu_vendor"] = SystemInfo.graphicsDeviceVendor,
				["gpu_version"] = SystemInfo.graphicsDeviceVersion,
				["gpu_shader_level"] = SystemInfo.graphicsShaderLevel.ToString(),
				["gpu_max_buffer_size"] = SystemInfo.maxGraphicsBufferSize.ToString(),
				["gpu_device_version"] = SystemInfo.graphicsDeviceVersion.ToString(),
				["cpu_cores"] = SystemInfo.processorCount.ToString(),
				["max_compute_work_size"] = SystemInfo.maxComputeWorkGroupSize.ToString(),
				["max_compute_work_size_x"] = SystemInfo.maxComputeWorkGroupSizeX.ToString(),
				["max_compute_work_size_y"] = SystemInfo.maxComputeWorkGroupSizeY.ToString(),
				["max_compute_work_size_z"] = SystemInfo.maxComputeWorkGroupSizeZ.ToString(),
				["cpu_frequency"] = SystemInfo.processorFrequency.ToString(),
				["gpu_max_texture_size"] = SystemInfo.maxTextureSize.ToString(),
				["cpu_name"] = SystemInfo.processorType.Trim(),
				["system_memory"] = SystemInfo.systemMemorySize.ToString(),
				["os"] = SystemInfo.operatingSystem,
				["supports_compute_shaders"] = SystemInfo.supportsComputeShaders.ToString(),
				["supports_async_compute"] = SystemInfo.supportsAsyncCompute.ToString(),
				["supports_async_gpu_readback"] = SystemInfo.supportsAsyncGPUReadback.ToString(),
				["supports_3d_textures"] = SystemInfo.supports3DTextures.ToString(),
				["supports_instancing"] = SystemInfo.supportsInstancing.ToString()
			};
		}

		public static Dictionary<string, string> GetApplicationData()
		{
			Dictionary<string, string> obj = new Dictionary<string, string> { ["unity"] = Application.unityVersion ?? "editor" };
			BuildInfo current = BuildInfo.Current;
			obj["changeset"] = ((current != null) ? current.Scm.ChangeId : null) ?? "editor";
			BuildInfo current2 = BuildInfo.Current;
			obj["branch"] = ((current2 != null) ? current2.Scm.Branch : null) ?? "editor";
			obj["network_version"] = 2631.ToString();
			obj["eos_sdk"] = ((object)VersionInterface.GetVersion())?.ToString() ?? "disabled";
			return obj;
		}

		private static IEnumerator AggregateLoop()
		{
			int loop = 0;
			while (!Application.isQuitting)
			{
				yield return CoroutineEx.waitForSecondsRealtime(60f);
				if (GameplayAnalytics)
				{
					yield return TryCatch(AggregatePlayers(blueprints: false, positions: true));
					if (loop % 60 == 0)
					{
						PushServerInfo();
						yield return TryCatch(AggregateEntitiesAndItems());
						yield return TryCatch(AggregatePlayers(blueprints: true));
						yield return TryCatch(AggregateTeams());
						Dictionary<PendingItemsKey, PendingItemsData> dict = pendingItems;
						pendingItems = new Dictionary<PendingItemsKey, PendingItemsData>();
						yield return PushPendingItemsLoopAsync(dict);
					}
					loop++;
				}
			}
		}

		private static IEnumerator TryCatch(IEnumerator coroutine)
		{
			while (true)
			{
				try
				{
					if (!coroutine.MoveNext())
					{
						break;
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					break;
				}
				yield return coroutine.Current;
			}
		}

		private static IEnumerator AggregateEntitiesAndItems()
		{
			List<BaseNetworkable> entityQueue = new List<BaseNetworkable>();
			entityQueue.Clear();
			int totalCount = BaseNetworkable.serverEntities.Count;
			entityQueue.AddRange(BaseNetworkable.serverEntities);
			Dictionary<string, int> itemDict = new Dictionary<string, int>();
			Dictionary<EntityKey, int> entityDict = new Dictionary<EntityKey, int>();
			yield return null;
			Debug.Log((object)"Starting to aggregate entities & items...");
			DateTime startTime = DateTime.UtcNow;
			Stopwatch watch = Stopwatch.StartNew();
			foreach (BaseNetworkable entity in entityQueue)
			{
				if (watch.ElapsedMilliseconds > MaxMSPerFrame)
				{
					yield return null;
					watch.Restart();
				}
				if ((Object)(object)entity == (Object)null || entity.IsDestroyed)
				{
					continue;
				}
				EntityKey key = new EntityKey
				{
					PrefabId = entity.prefabID
				};
				if (entity is BuildingBlock buildingBlock)
				{
					key.Grade = (int)(buildingBlock.grade + 1);
				}
				entityDict.TryGetValue(key, out var value);
				entityDict[key] = value + 1;
				if (!(entity is LootContainer) && !(entity is BasePlayer { IsNpc: not false }) && !(entity is NPCPlayer))
				{
					if (entity is BasePlayer basePlayer2)
					{
						AddItemsToDict(basePlayer2.inventory.containerMain, itemDict);
						AddItemsToDict(basePlayer2.inventory.containerBelt, itemDict);
						AddItemsToDict(basePlayer2.inventory.containerWear, itemDict);
					}
					else if (entity is IItemContainerEntity itemContainerEntity)
					{
						AddItemsToDict(itemContainerEntity.inventory, itemDict);
					}
					else if (entity is DroppedItemContainer { inventory: not null } droppedItemContainer)
					{
						AddItemsToDict(droppedItemContainer.inventory, itemDict);
					}
				}
			}
			Debug.Log((object)$"Took {Math.Round(DateTime.UtcNow.Subtract(startTime).TotalSeconds, 1)}s to aggregate {totalCount} entities & items...");
			_ = DateTime.UtcNow;
			SubmitPoint(EventRecord.New("entity_sum").AddObject("counts", entityDict.Select((KeyValuePair<EntityKey, int> x) => new EntitySumItem
			{
				PrefabId = x.Key.PrefabId,
				Grade = x.Key.Grade,
				Count = x.Value
			})));
			yield return null;
			SubmitPoint(EventRecord.New("item_sum").AddObject("counts", itemDict));
			yield return null;
		}

		private static void AddItemsToDict(ItemContainer container, Dictionary<string, int> dict)
		{
			if (container == null || container.itemList == null)
			{
				return;
			}
			foreach (Item item in container.itemList)
			{
				string shortname = item.info.shortname;
				dict.TryGetValue(shortname, out var value);
				dict[shortname] = value + item.amount;
				if (item.contents != null)
				{
					AddItemsToDict(item.contents, dict);
				}
			}
		}

		private static IEnumerator PushPendingItemsLoopAsync(Dictionary<PendingItemsKey, PendingItemsData> dict)
		{
			Stopwatch watch = Stopwatch.StartNew();
			foreach (PendingItemsData value in dict.Values)
			{
				try
				{
					LogResource(value.Key.Consumed ? ResourceMode.Consumed : ResourceMode.Produced, value.category, value.Key.Item, value.amount, null, null, safezone: false, null, 0uL, value.Key.Entity, null, null, 0uL);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
				PendingItemsData pendingItemsData = value;
				Pool.Free<PendingItemsData>(ref pendingItemsData);
				if (watch.ElapsedMilliseconds > MaxMSPerFrame)
				{
					yield return null;
					watch.Restart();
				}
			}
			dict.Clear();
		}

		public static void AddPendingItems(BaseEntity entity, string itemName, int amount, string category, bool consumed = true, bool perEntity = false)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			PendingItemsKey key = new PendingItemsKey
			{
				Entity = entity.ShortPrefabName,
				Category = category,
				Item = itemName,
				Consumed = consumed,
				EntityId = (NetworkableId)(perEntity ? entity.net.ID : default(NetworkableId))
			};
			if (!pendingItems.TryGetValue(key, out var value))
			{
				value = Pool.Get<PendingItemsData>();
				value.Key = key;
				value.category = category;
				pendingItems[key] = value;
			}
			value.amount += amount;
		}

		private static IEnumerator AggregatePlayers(bool blueprints = false, bool positions = false)
		{
			Stopwatch watch = Stopwatch.StartNew();
			Dictionary<int, int> playerBps = (blueprints ? new Dictionary<int, int>() : null);
			List<PlayerAggregate> playerPositions = (positions ? Pool.Get<List<PlayerAggregate>>() : null);
			Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					BasePlayer current = enumerator.Current;
					if ((Object)(object)current == (Object)null || current.IsDestroyed)
					{
						continue;
					}
					if (blueprints)
					{
						foreach (int unlockedItem in current.PersistantPlayerInfo.unlockedItems)
						{
							playerBps.TryGetValue(unlockedItem, out var value);
							playerBps[unlockedItem] = value + 1;
						}
					}
					if (positions)
					{
						PlayerAggregate playerAggregate = Pool.Get<PlayerAggregate>();
						playerAggregate.UserId = current.UserIDString;
						playerAggregate.Position = ((Component)current).transform.position;
						Quaternion bodyRotation = current.eyes.bodyRotation;
						playerAggregate.Direction = ((Quaternion)(ref bodyRotation)).eulerAngles;
						foreach (Item item in current.inventory.containerBelt.itemList)
						{
							playerAggregate.Hotbar.Add(item.info.shortname);
						}
						foreach (Item item2 in current.inventory.containerWear.itemList)
						{
							playerAggregate.Worn.Add(item2.info.shortname);
						}
						playerAggregate.ActiveItem = current.GetActiveItem()?.info.shortname;
						playerAggregate.Biome = GetBiome(((Component)current).transform.position);
						playerPositions.Add(playerAggregate);
					}
					if (watch.ElapsedMilliseconds > MaxMSPerFrame)
					{
						yield return null;
						watch.Restart();
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			if (blueprints)
			{
				SubmitPoint(EventRecord.New("blueprint_aggregate_online").AddObject("blueprints", playerBps.Select((KeyValuePair<int, int> x) => new
				{
					Key = ItemManager.FindItemDefinition(x.Key).shortname,
					value = x.Value
				})));
			}
			if (positions)
			{
				SubmitPoint(EventRecord.New("player_positions").AddObject("positions", playerPositions).AddObject("player_count", playerPositions.Count));
				Pool.Free<PlayerAggregate>(ref playerPositions, true);
			}
		}

		private static IEnumerator AggregateTeams()
		{
			yield return null;
			HashSet<ulong> teamIds = new HashSet<ulong>();
			int inTeam = 0;
			int notInTeam = 0;
			Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					BasePlayer current = enumerator.Current;
					if ((Object)(object)current != (Object)null && !current.IsDestroyed && current.currentTeam != 0L)
					{
						teamIds.Add(current.currentTeam);
						inTeam++;
					}
					else
					{
						notInTeam++;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			yield return null;
			Stopwatch watch = Stopwatch.StartNew();
			List<TeamInfo> teams = Pool.Get<List<TeamInfo>>();
			foreach (ulong item in teamIds)
			{
				RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindTeam(item);
				if (playerTeam == null || !((playerTeam.members != null) & (playerTeam.members.Count > 0)))
				{
					continue;
				}
				TeamInfo teamInfo = Pool.Get<TeamInfo>();
				teams.Add(teamInfo);
				foreach (ulong member in playerTeam.members)
				{
					BasePlayer basePlayer = RelationshipManager.FindByID(member);
					if ((Object)(object)basePlayer != (Object)null && !basePlayer.IsDestroyed && basePlayer.IsConnected && !basePlayer.IsSleeping())
					{
						teamInfo.online.Add(SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(member));
					}
					else
					{
						teamInfo.offline.Add(SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(member));
					}
				}
				teamInfo.member_count = teamInfo.online.Count + teamInfo.offline.Count;
				if (watch.ElapsedMilliseconds > MaxMSPerFrame)
				{
					yield return null;
					watch.Restart();
				}
			}
			SubmitPoint(EventRecord.New("online_teams").AddObject("teams", teams).AddField("users_in_team", inTeam)
				.AddField("users_not_in_team", notInTeam));
			foreach (TeamInfo item2 in teams)
			{
				TeamInfo current4 = item2;
				Pool.Free<TeamInfo>(ref current4);
			}
			Pool.Free<TeamInfo>(ref teams, false);
		}

		public static void Initialize()
		{
			TickLogging.RegisterForAnalytics(Manager);
			Manager.AddTable(AnalyticsManager.GameplayEventsTableServer, Manager.ServerUploader);
			PushItemDefinitions();
			PushEntityManifest();
			((MonoBehaviour)SingletonComponent<ServerMgr>.Instance).StartCoroutine(AggregateLoop());
		}

		private static void PushServerInfo()
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("server_info").AddField("seed", World.Seed).AddField("size", World.Size)
					.AddField("url", World.Url)
					.AddField("ip_convar", Net.sv.ip)
					.AddField("port_convar", Net.sv.port)
					.AddField("net_protocol", Net.sv.ProtocolId)
					.AddField("protocol_network", 2631)
					.AddField("protocol_save", 286);
				BuildInfo current = BuildInfo.Current;
				EventRecord eventRecord2 = eventRecord.AddField("changeset", ((current != null) ? current.Scm.ChangeId : null) ?? "0").AddField("unity_version", Application.unityVersion);
				BuildInfo current2 = BuildInfo.Current;
				SubmitPoint(eventRecord2.AddField("branch", ((current2 != null) ? current2.Scm.Branch : null) ?? "empty").AddField("server_tags", ConVar.Server.tags).AddField("device_id", SystemInfo.deviceUniqueIdentifier)
					.AddField("network_id", Net.sv.GetLastUIDGiven()));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		private static void PushItemDefinitions()
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				if ((Object)(object)GameManifest.Current == (Object)null)
				{
					return;
				}
				BuildInfo current = BuildInfo.Current;
				object obj;
				if (current == null)
				{
					obj = null;
				}
				else
				{
					ScmInfo scm = current.Scm;
					obj = ((scm != null) ? scm.ChangeId : null);
				}
				if (obj == null)
				{
					return;
				}
				SubmitPoint(EventRecord.New("item_definitions").AddObject("items", from x in ItemManager.itemDictionary
					select x.Value into x
					select new
					{
						item_id = x.itemid,
						shortname = x.shortname,
						craft_time = (x.Blueprint?.GetCraftTime() ?? 0f),
						workbench = (x.Blueprint?.GetWorkbenchLevel() ?? 0),
						category = x.category.ToString(),
						display_name = x.displayName.english,
						despawn_rarity = x.despawnRarity,
						ingredients = (from y in x.Blueprint?.GetIngredients()
							select new
							{
								shortname = y.itemDef.shortname,
								amount = (int)y.amount
							})
					}).AddField("changeset", BuildInfo.Current.Scm.ChangeId));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		private static void PushEntityManifest()
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				if ((Object)(object)GameManifest.Current == (Object)null)
				{
					return;
				}
				BuildInfo current = BuildInfo.Current;
				object obj;
				if (current == null)
				{
					obj = null;
				}
				else
				{
					ScmInfo scm = current.Scm;
					obj = ((scm != null) ? scm.ChangeId : null);
				}
				if (obj != null)
				{
					EventRecord eventRecord = EventRecord.New("entity_manifest").AddObject("entities", GameManifest.Current.entities.Select((string x) => new
					{
						shortname = Path.GetFileNameWithoutExtension(x),
						prefab_id = StringPool.Get(x.ToLower())
					}));
					BuildInfo current2 = BuildInfo.Current;
					SubmitPoint(eventRecord.AddField("changeset", ((current2 != null) ? current2.Scm.ChangeId : null) ?? "editor"));
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		private static void SubmitPoint(EventRecord point)
		{
			point.Submit();
		}

		public static void OnFiredProjectile(BasePlayer player, BasePlayer.FiredProjectile projectile, Guid projectileGroupId)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord record = EventRecord.New("entity_damage").AddField("start_pos", projectile.position).AddField("start_vel", projectile.initialVelocity)
					.AddField("velocity_inherit", projectile.inheritedVelocity)
					.AddField("ammo_item", projectile.itemDef?.shortname)
					.AddField("weapon", (BaseNetworkable)projectile.weaponSource)
					.AddField("projectile_group", projectileGroupId)
					.AddField("projectile_id", projectile.id)
					.AddField("attacker", (BaseNetworkable)player)
					.AddField("look_dir", player.tickViewAngles)
					.AddField("model_state", (player.modelStateTick ?? player.modelState).flags)
					.AddField("burst_mode", projectile.weaponSource?.HasFlag(BaseEntity.Flags.Reserved6) ?? false);
				PendingFiredProjectile pendingFiredProjectile = Pool.Get<PendingFiredProjectile>();
				pendingFiredProjectile.Record = record;
				pendingFiredProjectile.FiredProjectile = projectile;
				trackedProjectiles[new FiredProjectileKey(player.userID, projectile.id)] = pendingFiredProjectile;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnFiredProjectileRemoved(BasePlayer player, BasePlayer.FiredProjectile projectile)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				FiredProjectileKey key = new FiredProjectileKey(player.userID, projectile.id);
				if (!trackedProjectiles.TryGetValue(key, out var value))
				{
					return;
				}
				if (!value.Hit)
				{
					EventRecord record = value.Record;
					if (projectile.updates.Count > 0)
					{
						record.AddObject("projectile_updates", projectile.updates);
					}
					SubmitPoint(record);
				}
				Pool.Free<PendingFiredProjectile>(ref value);
				trackedProjectiles.Remove(key);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnQuarryItem(ResourceMode mode, string item, int amount, MiningQuarry sourceEntity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				AddPendingItems(sourceEntity, item, amount, "quarry", mode == ResourceMode.Consumed);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnExcavatorProduceItem(Item item, BaseEntity sourceEntity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				AddPendingItems(sourceEntity, item.info.shortname, item.amount, "excavator", consumed: false);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnExcavatorConsumeFuel(Item item, int amount, BaseEntity dieselEngine)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				LogResource(ResourceMode.Consumed, "excavator", item.info.shortname, amount, dieselEngine, null, safezone: false, null, 0uL, null, null, null, 0uL);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnCraftItem(string item, int amount, BasePlayer player, BaseEntity workbench, bool inSafezone, ulong skin = 0uL)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				LogResource(ResourceMode.Produced, "craft", item, amount, null, null, inSafezone, workbench, player?.userID ?? ((EncryptedValue<ulong>)0uL), null, null, null, skin);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnCraftMaterialConsumed(string item, int amount, BasePlayer player, BaseEntity workbench, bool inSafezone, string targetItem)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				LogResource(safezone: inSafezone, workbench: workbench, targetItem: targetItem, mode: ResourceMode.Consumed, category: "craft", itemName: item, amount: amount, sourceEntity: null, tool: null, steamId: player?.userID ?? ((EncryptedValue<ulong>)0uL), sourceEntityPrefab: null, sourceItem: null, skinId: 0uL);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnConsumableUsed(BasePlayer player, Item item)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("consumeable_used").AddField("player", (BaseNetworkable)player).AddField("item", item));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnEntitySpawned(BaseEntity entity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("entity_spawned").AddShortEntityField("entity", entity, includePos: true));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnEntityDestroyed(BaseEntity entity)
		{
			if (!GameplayAnalytics || !entity.IsValid())
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("entity_killed").AddShortEntityField("entity", entity, entity.syncPosition));
				if (!(entity is LootContainer { FirstLooterId: 0uL } lootContainer))
				{
					return;
				}
				foreach (Item item in lootContainer.inventory.itemList)
				{
					OnItemDespawn(lootContainer, item, 3, lootContainer.LastLootedBy);
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnMedUsed(string itemName, BasePlayer player, BaseEntity target)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("med_used").AddField("player", (BaseNetworkable)player).AddField("target", (BaseNetworkable)target)
					.AddField("item_name", itemName));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnCodelockChanged(BasePlayer player, CodeLock codeLock, string oldCode, string newCode, bool isGuest)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("code_change").AddField("player", (BaseNetworkable)player).AddField("codelock", (BaseNetworkable)codeLock)
					.AddField("old_code", oldCode)
					.AddField("new_code", newCode)
					.AddField("is_guest", isGuest));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnCodeLockEntered(BasePlayer player, CodeLock codeLock, bool isGuest)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("code_enter").AddField("player", (BaseNetworkable)player).AddField("codelock", (BaseNetworkable)codeLock)
					.AddField("is_guest", isGuest));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnTeamChanged(string change, ulong teamId, ulong teamLeader, ulong user, List<ulong> members)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			List<ulong> list = Pool.Get<List<ulong>>();
			try
			{
				if (members != null)
				{
					foreach (ulong member in members)
					{
						list.Add(member);
					}
				}
				SubmitPoint(EventRecord.New("team_change").AddField("team_leader", teamLeader).AddField("team", teamId)
					.AddField("target_user", user)
					.AddField("change", change)
					.AddObject("users", list)
					.AddField("member_count", members.Count));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			Pool.FreeUnmanaged<ulong>(ref list);
		}

		public static void OnEntityAuthChanged(BaseEntity entity, BasePlayer player, IEnumerable<ulong> authedList, string change, ulong targetUser)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("auth_change").AddField("entity", (BaseNetworkable)entity).AddField("player", (BaseNetworkable)player)
					.AddField("target", targetUser)
					.AddObject("auth_list", authedList)
					.AddField("change", change));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnSleepingBagAssigned(BasePlayer player, SleepingBag bag, ulong targetUser)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("sleeping_bag_assign").AddField("entity", (BaseNetworkable)bag).AddField("player", (BaseNetworkable)player)
					.AddField("target", targetUser));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnFallDamage(BasePlayer player, float velocity, float damage)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("fall_damage").AddField("player", (BaseNetworkable)player).AddField("velocity", velocity)
					.AddField("damage", damage));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnResearchStarted(BasePlayer player, BaseEntity entity, Item item, int scrapCost)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("research_start").AddField("player", (BaseNetworkable)player).AddField("item", item.info.shortname)
					.AddField("scrap", scrapCost)
					.AddField("entity", (BaseNetworkable)entity));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnBlueprintLearned(BasePlayer player, ItemDefinition item, string reason, int scrapCost, BaseEntity entity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("blueprint_learned").AddField("player", (BaseNetworkable)player).AddField("item", item.shortname)
					.AddField("reason", reason)
					.AddField("entity", (BaseNetworkable)entity)
					.AddField("scrap_cost", scrapCost));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnItemRecycled(string item, int amount, Recycler recycler)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				LogResource(ResourceMode.Consumed, "recycler", item, amount, recycler, null, safezone: false, null, recycler.LastLootedBy, null, null, null, 0uL);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnRecyclerItemProduced(string item, int amount, Recycler recycler, Item sourceItem)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				LogResource(ResourceMode.Produced, "recycler", item, amount, recycler, null, safezone: false, null, recycler.LastLootedBy, null, sourceItem, null, 0uL);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnGatherItem(string item, int amount, BaseEntity sourceEntity, BasePlayer player, AttackEntity weapon = null)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				LogResource(ResourceMode.Produced, "gather", item, amount, sourceEntity, weapon, safezone: false, null, player.userID, null, null, null, 0uL);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnFirstLooted(BaseEntity entity, BasePlayer player)
		{
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				if (entity is LootContainer lootContainer)
				{
					LogItemsLooted(player, entity, lootContainer.inventory);
					SubmitPoint(EventRecord.New("loot_entity").AddField("entity", (BaseNetworkable)entity).AddField("player", (BaseNetworkable)player)
						.AddField("monument", GetMonument(entity))
						.AddField("biome", GetBiome(((Component)entity).transform.position)));
				}
				else if (entity is LootableCorpse { containers: var containers })
				{
					foreach (ItemContainer container in containers)
					{
						LogItemsLooted(player, entity, container);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnLootContainerDestroyed(LootContainer entity, BasePlayer player, AttackEntity weapon)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				if (entity.DropsLoot && (Object)(object)player != (Object)null && Vector3.Distance(((Component)entity).transform.position, ((Component)player).transform.position) < 50f && entity.inventory?.itemList != null && entity.inventory.itemList.Count > 0)
				{
					LogItemsLooted(player, entity, entity.inventory, weapon);
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnEntityBuilt(BaseEntity entity, BasePlayer player)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("entity_built").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)entity);
				if (entity is SleepingBag)
				{
					int sleepingBagCount = SleepingBag.GetSleepingBagCount(player.userID);
					eventRecord.AddField("bags_active", sleepingBagCount);
					eventRecord.AddField("max_sleeping_bags", ConVar.Server.max_sleeping_bags);
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnMountEntity(BasePlayer player, BaseEntity seat, BaseEntity vehicle)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("mount").AddField("player", (BaseNetworkable)player).AddField("vehicle", (BaseNetworkable)vehicle)
					.AddField("seat", (BaseNetworkable)seat));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnDismountEntity(BasePlayer player, BaseEntity seat, BaseEntity vehicle)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("dismount").AddField("player", (BaseNetworkable)player).AddField("vehicle", (BaseNetworkable)vehicle)
					.AddField("seat", (BaseNetworkable)seat));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnKeycardSwiped(BasePlayer player, CardReader cardReader)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("keycard_swiped").AddField("player", (BaseNetworkable)player).AddField("card_level", cardReader.accessLevel)
					.AddField("entity", (BaseNetworkable)cardReader));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnLockedCrateStarted(BasePlayer player, HackableLockedCrate crate)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("hackable_crate_started").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)crate));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnLockedCrateFinished(ulong player, HackableLockedCrate crate)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("hackable_crate_ended").AddField("player_steamid", player).AddField("entity", (BaseNetworkable)crate));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnStashHidden(BasePlayer player, StashContainer entity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("stash_hidden").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)entity)
					.AddField("owner", entity.OwnerID));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnStashRevealed(BasePlayer player, StashContainer entity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("stash_reveal").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)entity)
					.AddField("owner", entity.OwnerID));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnAntihackViolation(BasePlayer player, AntiHackType type, string message)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("antihack_violation").AddField("player", (BaseNetworkable)player).AddField("violation_type", (int)type)
					.AddField("violation", type.ToString())
					.AddField("message", message);
				if (BuildInfo.Current != null)
				{
					eventRecord.AddField("changeset", BuildInfo.Current.Scm.ChangeId).AddField("network", 2631);
				}
				switch (type)
				{
				case AntiHackType.SpeedHack:
					eventRecord.AddField("speedhack_protection", ConVar.AntiHack.speedhack_protection).AddField("speedhack_forgiveness", ConVar.AntiHack.speedhack_forgiveness).AddField("speedhack_forgiveness_inertia", ConVar.AntiHack.speedhack_forgiveness_inertia)
						.AddField("speedhack_penalty", ConVar.AntiHack.speedhack_penalty)
						.AddField("speedhack_penalty", ConVar.AntiHack.speedhack_reject)
						.AddField("speedhack_slopespeed", ConVar.AntiHack.speedhack_slopespeed);
					break;
				case AntiHackType.NoClip:
					eventRecord.AddField("noclip_protection", ConVar.AntiHack.noclip_protection).AddField("noclip_penalty", ConVar.AntiHack.noclip_penalty).AddField("noclip_maxsteps", ConVar.AntiHack.noclip_maxsteps)
						.AddField("noclip_margin_dismount", ConVar.AntiHack.noclip_margin_dismount)
						.AddField("noclip_margin", ConVar.AntiHack.noclip_margin)
						.AddField("noclip_backtracking", ConVar.AntiHack.noclip_backtracking)
						.AddField("noclip_reject", ConVar.AntiHack.noclip_reject)
						.AddField("noclip_stepsize", ConVar.AntiHack.noclip_stepsize);
					break;
				case AntiHackType.ProjectileHack:
					eventRecord.AddField("projectile_anglechange", ConVar.AntiHack.projectile_anglechange).AddField("projectile_backtracking", ConVar.AntiHack.projectile_backtracking).AddField("projectile_clientframes", ConVar.AntiHack.projectile_clientframes)
						.AddField("projectile_damagedepth", ConVar.AntiHack.projectile_damagedepth)
						.AddField("projectile_desync", ConVar.AntiHack.projectile_desync)
						.AddField("projectile_forgiveness", ConVar.AntiHack.projectile_forgiveness)
						.AddField("projectile_impactspawndepth", ConVar.AntiHack.projectile_impactspawndepth)
						.AddField("projectile_losforgiveness", ConVar.AntiHack.projectile_losforgiveness)
						.AddField("projectile_penalty", ConVar.AntiHack.projectile_penalty)
						.AddField("projectile_positionoffset", ConVar.AntiHack.projectile_positionoffset)
						.AddField("projectile_protection", ConVar.AntiHack.projectile_protection)
						.AddField("projectile_serverframes", ConVar.AntiHack.projectile_serverframes)
						.AddField("projectile_terraincheck", ConVar.AntiHack.projectile_terraincheck)
						.AddField("projectile_trajectory", ConVar.AntiHack.projectile_trajectory)
						.AddField("projectile_vehiclecheck", ConVar.AntiHack.projectile_vehiclecheck)
						.AddField("projectile_velocitychange", ConVar.AntiHack.projectile_velocitychange);
					break;
				case AntiHackType.InsideTerrain:
					eventRecord.AddField("terrain_check_geometry", ConVar.AntiHack.terrain_check_geometry).AddField("terrain_kill", ConVar.AntiHack.terrain_kill).AddField("terrain_padding", ConVar.AntiHack.terrain_padding)
						.AddField("terrain_penalty", ConVar.AntiHack.terrain_penalty)
						.AddField("terrain_protection", ConVar.AntiHack.terrain_protection)
						.AddField("terrain_timeslice", ConVar.AntiHack.terrain_timeslice);
					break;
				case AntiHackType.MeleeHack:
					eventRecord.AddField("melee_backtracking", ConVar.AntiHack.melee_backtracking).AddField("melee_clientframes", ConVar.AntiHack.melee_clientframes).AddField("melee_forgiveness", ConVar.AntiHack.melee_forgiveness)
						.AddField("melee_losforgiveness", ConVar.AntiHack.melee_losforgiveness)
						.AddField("melee_penalty", ConVar.AntiHack.melee_penalty)
						.AddField("melee_protection", ConVar.AntiHack.melee_protection)
						.AddField("melee_serverframes", ConVar.AntiHack.melee_serverframes)
						.AddField("melee_terraincheck", ConVar.AntiHack.melee_terraincheck)
						.AddField("melee_vehiclecheck", ConVar.AntiHack.melee_vehiclecheck);
					break;
				case AntiHackType.FlyHack:
					eventRecord.AddField("flyhack_extrusion", ConVar.AntiHack.flyhack_extrusion).AddField("flyhack_forgiveness_horizontal", ConVar.AntiHack.flyhack_forgiveness_horizontal).AddField("flyhack_forgiveness_horizontal_inertia", ConVar.AntiHack.flyhack_forgiveness_horizontal_inertia)
						.AddField("flyhack_forgiveness_vertical", ConVar.AntiHack.flyhack_forgiveness_vertical)
						.AddField("flyhack_forgiveness_vertical_inertia", ConVar.AntiHack.flyhack_forgiveness_vertical_inertia)
						.AddField("flyhack_margin", ConVar.AntiHack.flyhack_margin)
						.AddField("flyhack_maxsteps", ConVar.AntiHack.flyhack_maxsteps)
						.AddField("flyhack_penalty", ConVar.AntiHack.flyhack_penalty)
						.AddField("flyhack_protection", ConVar.AntiHack.flyhack_protection)
						.AddField("flyhack_reject", ConVar.AntiHack.flyhack_reject);
					break;
				case AntiHackType.EyeHack:
					eventRecord.AddField("eye_clientframes", ConVar.AntiHack.eye_clientframes).AddField("eye_forgiveness", ConVar.AntiHack.eye_forgiveness).AddField("eye_history_forgiveness", ConVar.AntiHack.eye_history_forgiveness)
						.AddField("eye_history_penalty", ConVar.AntiHack.eye_history_penalty)
						.AddField("eye_losradius", ConVar.AntiHack.eye_losradius)
						.AddField("eye_noclip_backtracking", ConVar.AntiHack.eye_noclip_backtracking)
						.AddField("eye_noclip_cutoff", ConVar.AntiHack.eye_noclip_cutoff)
						.AddField("eye_penalty", ConVar.AntiHack.eye_penalty)
						.AddField("eye_protection", ConVar.AntiHack.eye_protection)
						.AddField("eye_serverframes", ConVar.AntiHack.eye_serverframes)
						.AddField("eye_terraincheck", ConVar.AntiHack.eye_terraincheck)
						.AddField("eye_vehiclecheck", ConVar.AntiHack.eye_vehiclecheck);
					break;
				case AntiHackType.AttackHack:
					eventRecord.AddField("maxdesync", ConVar.AntiHack.maxdesync);
					break;
				case AntiHackType.Ticks:
					eventRecord.AddField("max_distance", ConVar.AntiHack.tick_max_distance).AddField("max_distance_falling", ConVar.AntiHack.tick_max_distance_falling).AddField("max_distance_parented", ConVar.AntiHack.tick_max_distance_parented)
						.AddField("tick_buffer_noclip_threshold", ConVar.AntiHack.tick_buffer_noclip_threshold)
						.AddField("tick_buffer_reject_threshold", ConVar.AntiHack.tick_buffer_reject_threshold);
					break;
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnEyehackViolation(BasePlayer player, Vector3 eyePos)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("antihack_violation_detailed").AddField("player", (BaseNetworkable)player).AddField("violation_type", 6)
					.AddField("eye_pos", eyePos));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnNoclipViolation(BasePlayer player, Vector3 startPos, Vector3 endPos, int tickCount, Collider collider)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("antihack_violation_detailed").AddField("player", (BaseNetworkable)player).AddField("violation_type", 1)
					.AddField("start_pos", startPos)
					.AddField("end_pos", endPos)
					.AddField("tick_count", tickCount)
					.AddField("collider_name", ((Object)collider).name));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnFlyhackViolation(BasePlayer player, Vector3 startPos, Vector3 endPos, int tickCount)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("antihack_violation_detailed").AddField("player", (BaseNetworkable)player).AddField("violation_type", 3)
					.AddField("start_pos", startPos)
					.AddField("end_pos", endPos)
					.AddField("tick_count", tickCount));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnProjectileHackViolation(BasePlayer.FiredProjectile projectile)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				if (!projectile.invalid)
				{
					FiredProjectileKey key = new FiredProjectileKey(projectile.attacker.userID, projectile.id);
					if (trackedProjectiles.TryGetValue(key, out var value))
					{
						projectile.invalid = true;
						value.Record.AddField("projectile_invalid", value: true).AddObject("updates", projectile.updates);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnSpeedhackViolation(BasePlayer player, Vector3 startPos, Vector3 endPos, int tickCount)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("antihack_violation_detailed").AddField("player", (BaseNetworkable)player).AddField("violation_type", 2)
					.AddField("start_pos", startPos)
					.AddField("end_pos", endPos)
					.AddField("tick_count", tickCount)
					.AddField("distance", Vector3.Distance(startPos, endPos))
					.AddField("distance_2d", Vector3Ex.Distance2D(startPos, endPos)));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnTickViolation(BasePlayer player, Vector3 startPos, Vector3 endPos, int tickCount)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("antihack_violation_detailed").AddField("player", (BaseNetworkable)player).AddField("violation_type", 13)
					.AddField("start_pos", startPos)
					.AddField("end_pos", endPos)
					.AddField("tick_count", tickCount)
					.AddField("distance", Vector3.Distance(startPos, endPos))
					.AddField("distance_2d", Vector3Ex.Distance2D(startPos, endPos)));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnTerrainHackViolation(BasePlayer player)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("antihack_violation_detailed").AddField("player", (BaseNetworkable)player).AddField("violation_type", 10)
					.AddField("seed", World.Seed)
					.AddField("size", World.Size)
					.AddField("map_url", World.Url)
					.AddField("map_checksum", World.Checksum));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnEntityTakeDamage(HitInfo info, bool isDeath)
		{
			//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_0221: Unknown result type (might be due to invalid IL or missing references)
			//IL_022c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0361: Unknown result type (might be due to invalid IL or missing references)
			//IL_0366: Unknown result type (might be due to invalid IL or missing references)
			//IL_0426: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0463: Unknown result type (might be due to invalid IL or missing references)
			//IL_047a: Unknown result type (might be due to invalid IL or missing references)
			//IL_05e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_05fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0615: Unknown result type (might be due to invalid IL or missing references)
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				BasePlayer initiatorPlayer = info.InitiatorPlayer;
				BasePlayer basePlayer = info.HitEntity as BasePlayer;
				if (((Object)(object)info.Initiator == (Object)null && !isDeath) || (((Object)(object)initiatorPlayer == (Object)null || initiatorPlayer.IsNpc || initiatorPlayer.IsBot) && ((Object)(object)basePlayer == (Object)null || basePlayer.IsNpc || basePlayer.IsBot)))
				{
					return;
				}
				EventRecord eventRecord = null;
				float value = -1f;
				float value2 = -1f;
				if ((Object)(object)initiatorPlayer != (Object)null)
				{
					if (info.IsProjectile())
					{
						FiredProjectileKey key = new FiredProjectileKey(initiatorPlayer.userID, info.ProjectileID);
						if (trackedProjectiles.TryGetValue(key, out var value3))
						{
							eventRecord = value3.Record;
							value = Vector3.Distance(info.HitPositionWorld, value3.FiredProjectile.initialPosition);
							value = Vector3Ex.Distance2D(info.HitPositionWorld, value3.FiredProjectile.initialPosition);
							value3.Hit = info.DidHit;
							if (eventRecord != null && value3.FiredProjectile.updates.Count > 0)
							{
								eventRecord.AddObject("projectile_updates", value3.FiredProjectile.updates);
							}
							if (eventRecord != null && value3.FiredProjectile.simulatedPositions.Count > 0)
							{
								eventRecord.AddObject("simulated_position", value3.FiredProjectile.simulatedPositions);
							}
							if (eventRecord != null)
							{
								eventRecord.AddField("partial_time", value3.FiredProjectile.partialTime);
								eventRecord.AddField("desync_lifetime", value3.FiredProjectile.desyncLifeTime);
								eventRecord.AddField("startpoint_mismatch", value3.FiredProjectile.startPointMismatch);
								eventRecord.AddField("endpoint_mismatch", value3.FiredProjectile.endPointMismatch);
								eventRecord.AddField("entity_distance", value3.FiredProjectile.entityDistance);
								eventRecord.AddField("position_offset", value3.FiredProjectile.initialPositionOffset);
							}
							trackedProjectiles.Remove(key);
							Pool.Free<PendingFiredProjectile>(ref value3);
						}
					}
					else
					{
						value = Vector3.Distance(info.HitNormalWorld, initiatorPlayer.eyes.position);
						value2 = Vector3Ex.Distance2D(info.HitNormalWorld, initiatorPlayer.eyes.position);
					}
				}
				if (eventRecord == null)
				{
					eventRecord = EventRecord.New("entity_damage");
				}
				eventRecord.AddField("is_headshot", info.isHeadshot).AddField("victim", (BaseNetworkable)info.HitEntity).AddField("damage", info.damageTypes.Total())
					.AddField("damage_type", info.damageTypes.GetMajorityDamageType().ToString())
					.AddField("pos_world", info.HitPositionWorld)
					.AddField("pos_local", info.HitPositionLocal)
					.AddField("point_start", info.PointStart)
					.AddField("point_end", info.PointEnd)
					.AddField("normal_world", info.HitNormalWorld)
					.AddField("normal_local", info.HitNormalLocal)
					.AddField("distance_cl", info.ProjectileDistance)
					.AddField("distance", value)
					.AddField("distance_2d", value2);
				if ((Object)(object)info.HitEntity != (Object)null && (Object)(object)info.HitEntity.model != (Object)null)
				{
					eventRecord.AddField("pos_local_model", ((Component)info.HitEntity.model).transform.InverseTransformPoint(info.HitPositionWorld));
				}
				if (!info.IsProjectile())
				{
					eventRecord.AddField("weapon", (BaseNetworkable)info.Weapon);
					eventRecord.AddField("attacker", (BaseNetworkable)info.Initiator);
				}
				if (info.HitBone != 0)
				{
					eventRecord.AddField("bone", info.HitBone).AddField("bone_name", info.boneName).AddField("hit_area", (int)info.boneArea);
				}
				if (info.ProjectileID != 0)
				{
					eventRecord.AddField("projectile_integrity", info.ProjectileIntegrity).AddField("projectile_hits", info.ProjectileHits).AddField("trajectory_mismatch", info.ProjectileTrajectoryMismatch)
						.AddField("travel_time", info.ProjectileTravelTime)
						.AddField("projectile_velocity", info.ProjectileVelocity)
						.AddField("projectile_prefab", ((Object)info.ProjectilePrefab).name);
				}
				if ((Object)(object)initiatorPlayer != (Object)null && !info.IsProjectile())
				{
					eventRecord.AddField("attacker_eye_pos", initiatorPlayer.eyes.position);
					eventRecord.AddField("attacker_eye_dir", initiatorPlayer.eyes.BodyForward());
					if (((object)initiatorPlayer).GetType() == typeof(BasePlayer))
					{
						eventRecord.AddField("attacker_life", initiatorPlayer.respawnId);
					}
				}
				else if ((Object)(object)initiatorPlayer != (Object)null)
				{
					eventRecord.AddObject("attacker_worn", initiatorPlayer.inventory.containerWear.itemList.Select((Item x) => new SimpleItemAmount(x)));
					eventRecord.AddObject("attacker_hotbar", initiatorPlayer.inventory.containerBelt.itemList.Select((Item x) => new SimpleItemAmount(x)));
				}
				if ((Object)(object)basePlayer != (Object)null)
				{
					eventRecord.AddField("victim_life", basePlayer.respawnId);
					eventRecord.AddObject("victim_worn", basePlayer.inventory.containerWear.itemList.Select((Item x) => new SimpleItemAmount(x)));
					eventRecord.AddObject("victim_hotbar", basePlayer.inventory.containerBelt.itemList.Select((Item x) => new SimpleItemAmount(x)));
					eventRecord.AddField("victim_view_dir", basePlayer.tickViewAngles);
					eventRecord.AddField("victim_eye_pos", basePlayer.eyes.position);
					eventRecord.AddField("victim_eye_dir", basePlayer.eyes.BodyForward());
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnPlayerRespawned(BasePlayer player, BaseEntity targetEntity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("player_respawn").AddField("player", (BaseNetworkable)player).AddField("bag", (BaseNetworkable)targetEntity)
					.AddField("life_id", player.respawnId));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnExplosiveLaunched(BasePlayer player, BaseEntity explosive, BaseEntity launcher = null)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("explosive_launch").AddField("player", (BaseNetworkable)player).AddField("explosive", (BaseNetworkable)explosive)
					.AddField("explosive_velocity", explosive.GetWorldVelocity());
				Vector3 worldVelocity = explosive.GetWorldVelocity();
				EventRecord eventRecord2 = eventRecord.AddField("explosive_direction", ((Vector3)(ref worldVelocity)).normalized);
				if ((Object)(object)launcher != (Object)null)
				{
					eventRecord2.AddField("launcher", (BaseNetworkable)launcher);
				}
				SubmitPoint(eventRecord2);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnExplosion(TimedExplosive explosive)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("explosion").AddField("entity", (BaseNetworkable)explosive));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnItemDespawn(BaseEntity itemContainer, Item item, int dropReason, ulong userId)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("item_despawn").AddField("entity", (BaseNetworkable)itemContainer).AddField("item", item)
					.AddField("drop_reason", dropReason);
				if (userId != 0L)
				{
					eventRecord.AddField("player_userid", userId);
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnItemDropped(BasePlayer player, WorldItem entity, DroppedItem.DropReasonEnum dropReason)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("item_drop").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)entity)
					.AddField("item", entity.GetItem())
					.AddField("drop_reason", (int)dropReason));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnItemPickup(BasePlayer player, WorldItem entity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("item_pickup").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)entity)
					.AddField("item", entity.GetItem()));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnPlayerConnected(Connection connection)
		{
			try
			{
				string userWipeId = SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(connection.userid);
				SubmitPoint(EventRecord.New("player_connect").AddField("player_userid", userWipeId).AddField("steam_id", connection.userid)
					.AddField("username", connection.username)
					.AddField("ip", connection.ipaddress));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnPlayerDisconnected(Connection connection, string reason)
		{
			try
			{
				string userWipeId = SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(connection.userid);
				SubmitPoint(EventRecord.New("player_disconnect").AddField("player_userid", userWipeId).AddField("steam_id", connection.userid)
					.AddField("username", connection.username)
					.AddField("reason", reason));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnEntityPickedUp(BasePlayer player, BaseEntity entity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("entity_pickup").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)entity));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnChatMessage(BasePlayer player, string message, int channel)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("chat").AddField("player", (BaseNetworkable)player).AddField("message", message)
					.AddField("channel", channel));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnVendingMachineOrderChanged(BasePlayer player, VendingMachine vendingMachine, int sellItemId, int sellAmount, bool sellingBp, int buyItemId, int buyAmount, bool buyingBp, bool added)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				ItemDefinition itemDefinition = ItemManager.FindItemDefinition(sellItemId);
				ItemDefinition itemDefinition2 = ItemManager.FindItemDefinition(buyItemId);
				SubmitPoint(EventRecord.New("vending_changed").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)vendingMachine)
					.AddField("sell_item", itemDefinition.shortname)
					.AddField("sell_amount", sellAmount)
					.AddField("buy_item", itemDefinition2.shortname)
					.AddField("buy_amount", buyAmount)
					.AddField("is_selling_bp", sellingBp)
					.AddField("is_buying_bp", buyingBp)
					.AddField("change", added ? "added" : "removed"));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnBuyFromVendingMachine(BasePlayer player, VendingMachine vendingMachine, int sellItemId, int sellAmount, bool sellingBp, int buyItemId, int buyAmount, bool buyingBp, int numberOfTransactions, float discount, BaseEntity drone = null)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				ItemDefinition itemDefinition = ItemManager.FindItemDefinition(sellItemId);
				ItemDefinition itemDefinition2 = ItemManager.FindItemDefinition(buyItemId);
				SubmitPoint(EventRecord.New("vending_sale").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)vendingMachine)
					.AddField("sell_item", itemDefinition.shortname)
					.AddField("sell_amount", sellAmount)
					.AddField("buy_item", itemDefinition2.shortname)
					.AddField("buy_amount", buyAmount)
					.AddField("transactions", numberOfTransactions)
					.AddField("is_selling_bp", sellingBp)
					.AddField("is_buying_bp", buyingBp)
					.AddField("drone_terminal", (BaseNetworkable)drone)
					.AddField("discount", discount));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnNPCVendor(BasePlayer player, NPCTalking vendor, int scrapCost, string action)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("npc_vendor").AddField("player", (BaseNetworkable)player).AddField("vendor", (BaseNetworkable)vendor)
					.AddField("scrap_amount", scrapCost)
					.AddField("action", action));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		private static void LogItemsLooted(BasePlayer looter, BaseEntity entity, ItemContainer container, AttackEntity tool = null)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				if ((Object)(object)entity == (Object)null || container == null)
				{
					return;
				}
				foreach (Item item in container.itemList)
				{
					if (item != null)
					{
						string shortname = item.info.shortname;
						int amount = item.amount;
						ulong steamId = looter?.userID ?? ((EncryptedValue<ulong>)0uL);
						LogResource(ResourceMode.Produced, "loot", shortname, amount, entity, tool, safezone: false, null, steamId, null, null, null, 0uL);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void LogResource(ResourceMode mode, string category, string itemName, int amount, BaseEntity sourceEntity = null, AttackEntity tool = null, bool safezone = false, BaseEntity workbench = null, ulong steamId = 0uL, string sourceEntityPrefab = null, Item sourceItem = null, string targetItem = null, ulong skinId = 0uL)
		{
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				string value = ((mode == ResourceMode.Produced) ? "Produced" : "Consumed");
				EventRecord eventRecord = EventRecord.New("item_event").AddField("item_mode", value).AddField("category", category)
					.AddField("item_name", itemName)
					.AddField("amount", amount);
				if ((Object)(object)sourceEntity != (Object)null)
				{
					eventRecord.AddField("entity", (BaseNetworkable)sourceEntity);
					string biome = GetBiome(((Component)sourceEntity).transform.position);
					if (biome != null)
					{
						eventRecord.AddField("biome", biome);
					}
					if (IsOcean(((Component)sourceEntity).transform.position))
					{
						eventRecord.AddField("ocean", value: true);
					}
					string monument = GetMonument(sourceEntity);
					if (monument != null)
					{
						eventRecord.AddField("monument", monument);
					}
				}
				if (sourceEntityPrefab != null)
				{
					eventRecord.AddField("entity_prefab", sourceEntityPrefab);
				}
				if ((Object)(object)tool != (Object)null)
				{
					eventRecord.AddField("tool", (BaseNetworkable)tool);
				}
				if (safezone)
				{
					eventRecord.AddField("safezone", value: true);
				}
				if ((Object)(object)workbench != (Object)null)
				{
					eventRecord.AddField("workbench", (BaseNetworkable)workbench);
				}
				if (sourceEntity is GrowableEntity plant)
				{
					eventRecord.AddField("genes", GetGenesAsString(plant));
				}
				if (sourceItem != null)
				{
					eventRecord.AddField("source_item", sourceItem.info.shortname);
				}
				if (targetItem != null)
				{
					eventRecord.AddField("target_item", targetItem);
				}
				if (steamId != 0L)
				{
					string userWipeId = SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(steamId);
					eventRecord.AddField("player_userid", userWipeId);
					eventRecord.AddField("player_steamid", steamId);
				}
				if (skinId != 0L)
				{
					eventRecord.AddField("item_skin_id", skinId);
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnSkinChanged(BasePlayer player, RepairBench repairBench, Item item, ulong workshopId)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("item_skinned").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)repairBench)
					.AddField("item", item)
					.AddField("new_skin", workshopId));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnEntitySkinChanged(BasePlayer player, BaseNetworkable entity, int newSkin)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("entity_skinned").AddField("player", (BaseNetworkable)player).AddField("entity", entity)
					.AddField("new_skin", newSkin));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnItemRepaired(BasePlayer player, BaseEntity repairBench, Item itemToRepair, float conditionBefore, float maxConditionBefore)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("item_repair").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)repairBench)
					.AddField("item", itemToRepair)
					.AddField("old_condition", conditionBefore)
					.AddField("old_max_condition", maxConditionBefore)
					.AddField("max_condition", itemToRepair.maxConditionNormalized));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnEntityRepaired(BasePlayer player, BaseEntity entity, float healthBefore, float healthAfter)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("entity_repair").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)entity)
					.AddField("healing", healthAfter - healthBefore)
					.AddField("health_before", healthBefore)
					.AddField("health_after", healthAfter));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnBuildingBlockUpgraded(BasePlayer player, BuildingBlock buildingBlock, BuildingGrade.Enum targetGrade, uint targetColor, ulong targetSkin, bool includeColour)
		{
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("block_upgrade").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)buildingBlock)
					.AddField("old_grade", (int)buildingBlock.grade)
					.AddField("new_grade", (int)targetGrade)
					.AddField("biome", GetBiome(((Component)buildingBlock).transform.position))
					.AddField("skin_old", buildingBlock.skinID)
					.AddField("skin", targetSkin);
				if (includeColour)
				{
					eventRecord.AddField("color", targetColor);
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnBuildingBlockDemolished(BasePlayer player, StabilityEntity buildingBlock)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("block_demolish").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)buildingBlock));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnPlayerInitializedWipeId(ulong userId, string wipeId)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("player_wipe_id_set").AddField("user_id", userId).AddField("player_wipe_id", wipeId));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnFreeUnderwaterCrate(BasePlayer player, FreeableLootContainer freeableLootContainer)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("crate_untied").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)freeableLootContainer));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnVehiclePurchased(BasePlayer player, BaseEntity vehicle)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("vehicle_purchase").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)vehicle)
					.AddField("price", (BaseNetworkable)vehicle));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnMissionComplete(BasePlayer player, BaseMission mission, BaseMission.MissionFailReason? failReason = null)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("mission_complete").AddField("player", (BaseNetworkable)player).AddField("mission", mission.shortname);
				if (failReason.HasValue)
				{
					eventRecord.AddField("mission_succeed", value: false).AddField("fail_reason", failReason.Value.ToString());
				}
				else
				{
					eventRecord.AddField("mission_succeed", value: true);
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnGamblingResult(BasePlayer player, BaseEntity entity, int scrapPaid, int scrapRecieved, Guid? gambleGroupId = null)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("gambing").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)entity)
					.AddField("scrap_input", scrapPaid)
					.AddField("scrap_output", scrapRecieved);
				if (gambleGroupId.HasValue)
				{
					eventRecord.AddField("gamble_grouping", gambleGroupId.Value);
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnPlayerPinged(BasePlayer player, BasePlayer.PingType type, bool wasViaWheel)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("player_pinged").AddField("player", (BaseNetworkable)player).AddField("pingType", (int)type)
					.AddField("viaWheel", wasViaWheel));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnBagUnclaimed(BasePlayer player, SleepingBag bag)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("bag_unclaim").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)bag));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnSteamAuth(ulong userId, ulong ownerUserId, string authResponse)
		{
			try
			{
				SubmitPoint(EventRecord.New("steam_auth").AddField("user", userId).AddField("owner", ownerUserId)
					.AddField("response", authResponse)
					.AddField("server_port", Net.sv.port)
					.AddField("network_mode", Net.sv.ProtocolId)
					.AddField("player_count", BasePlayer.activePlayerList.Count)
					.AddField("max_players", ConVar.Server.maxplayers)
					.AddField("hostname", ConVar.Server.hostname)
					.AddField("secure", Net.sv.secure));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnEntityColorChanged(BasePlayer player, BaseEntity entity, uint oldColor, uint newColor)
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("player_pinged").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)entity)
					.AddField("color_old", oldColor)
					.AddField("color_new", newColor)
					.AddField("biome", GetBiome(((Component)entity).transform.position)));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnBurstModeToggled(BasePlayer player, BaseProjectile gun, bool state)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("burst_toggle").AddField("player", (BaseNetworkable)player).AddField("weapon", (BaseNetworkable)gun)
					.AddField("enabled", state));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnParachuteUsed(BasePlayer player, float distanceTravelled, float deployHeight, float timeInAir)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("parachute_used").AddField("player", (BaseNetworkable)player).AddField("distanceTravelled", distanceTravelled)
					.AddField("deployHeight", deployHeight)
					.AddField("timeInAir", timeInAir));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnTutorialStarted(BasePlayer player)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("tutorial_started").AddField("player", (BaseNetworkable)player));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnTutorialCompleted(BasePlayer player, float timeElapsed)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("tutorial_completed").AddField("player", (BaseNetworkable)player).AddLegacyTimespan("duration", TimeSpan.FromSeconds(timeElapsed)));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnTutorialQuit(BasePlayer player, string activeMissionName)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("tutorial_quit").AddField("player", (BaseNetworkable)player).AddField("activeMissionName", activeMissionName));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnBaseInteract(BasePlayer player, BaseEntity entity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("base_interaction").AddField("player", (BaseNetworkable)player).AddField("entity", (BaseNetworkable)entity));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnPlayerDeath(BasePlayer player, BasePlayer killer)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("player_death").AddField("player", (BaseNetworkable)player).AddField("killer", (BaseNetworkable)killer));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnCarShredded(MagnetLiftable car, List<Item> produced)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("car_shredded").AddField("player", (BaseNetworkable)car.associatedPlayer).AddField("car", (BaseNetworkable)car.GetBaseEntity());
				foreach (Item item in produced)
				{
					eventRecord.AddField("item_" + item.info.shortname, item);
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnPlayerTick(BasePlayer player, Vector3 pos, in BasePlayer.CachedState tickState, bool isMounted)
		{
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			if (GameplayTickAnalyticsConVar)
			{
				EventRecord record = EventRecord.New("player_tick").AddField("player_steamid", player.UserIDString).AddField("modelstate", (player.modelStateTick ?? player.modelState).flags)
					.AddField("heldentity", ((Object)(object)player.GetHeldEntity() != (Object)null) ? player.GetHeldEntity().ShortPrefabName : "")
					.AddField("pitch", player.tickViewAngles.x)
					.AddField("yaw", player.tickViewAngles.y)
					.AddField("pos_x", pos.x)
					.AddField("pos_y", pos.y)
					.AddField("pos_z", pos.z)
					.AddField("eye_pos_x", tickState.EyePos.x)
					.AddField("eye_pos_y", tickState.EyePos.y)
					.AddField("eye_pos_z", tickState.EyePos.z)
					.AddField("mouse_delta_x", player.tickMouseDelta.x)
					.AddField("mouse_delta_y", player.tickMouseDelta.y)
					.AddField("mouse_delta_z", player.tickMouseDelta.z)
					.AddField("parented", player.HasParent())
					.AddField("mounted", isMounted)
					.AddField("admin", player.IsAdmin || player.IsDeveloper)
					.AddField("water_factor", tickState.WaterFactor)
					.AddField("Timestamp", DateTime.UtcNow);
				TickLogging.TickTable.Append(record);
			}
		}

		public static void OnWallpaperPlaced(BasePlayer player, BuildingBlock buildingBlock, ulong skinID, int side, bool reskin)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("wallpaper_placed").AddField("player", (BaseNetworkable)player).AddField("buildingBlock", (BaseNetworkable)buildingBlock)
					.AddField("skin", skinID)
					.AddField("side", side)
					.AddField("reskin", reskin));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnStartFish(BasePlayer player, Item lure, Vector3 targetPos)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("fishing_start").AddField("player", (BaseNetworkable)player).AddField("lure", lure)
					.AddField("target_pos", targetPos));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnFailedFish(BasePlayer player, BaseFishingRod.FailReason reason)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("fishing_failed").AddField("player", (BaseNetworkable)player).AddField("fail_reason", (int)reason));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnCaughtFish(BasePlayer player, Item item)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("fishing_caught").AddField("player", (BaseNetworkable)player).AddField("item", item));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnPlayerChangeInjureState(BasePlayer player, BasePlayer.InjureState oldState, BasePlayer.InjureState newState)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("injure_state").AddField("player", (BaseNetworkable)player).AddField("old_state", (int)oldState)
					.AddField("new_state", (int)newState));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnPlayerLifeStoryEnd(BasePlayer player, PlayerLifeStory lifeStory)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("life_story_end").AddField("player", (BaseNetworkable)player).AddField("time_born", lifeStory.timeBorn)
					.AddField("time_died", lifeStory.timeDied)
					.AddField("meters_walked", lifeStory.metersWalked)
					.AddField("meters_ran", lifeStory.metersRun)
					.AddField("seconds_alive", lifeStory.secondsAlive)
					.AddField("seconds_in_base", lifeStory.secondsInBase)
					.AddField("seconds_in_wilderness", lifeStory.secondsWilderness)
					.AddField("seconds_in_monument", lifeStory.secondsInMonument)
					.AddField("seconds_driving", lifeStory.secondsDriving)
					.AddField("seconds_flying", lifeStory.secondsFlying)
					.AddField("seconds_boating", lifeStory.secondsBoating)
					.AddField("seconds_sleeping", lifeStory.secondsSleeping)
					.AddField("seconds_swimming", lifeStory.secondsSwimming)
					.AddField("total_damage_taken", lifeStory.totalDamageTaken)
					.AddField("total_healed", lifeStory.totalHealing)
					.AddField("killed_players", lifeStory.killedPlayers)
					.AddField("killed_animals", lifeStory.killedAnimals)
					.AddField("killed_scientists", lifeStory.killedScientists)
					.AddObject("death_info", lifeStory.deathInfo)
					.AddObject("weapon_stats", lifeStory.weaponStats));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnServerRPC(BasePlayer player, uint nameID, byte[] data, int length)
		{
			if (!GameplayAnalytics || !GameplayRpcAnalyticsConVar)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("server_rpc").AddField("player", (BaseNetworkable)player).AddField("rpc", StringPool.Get(nameID))
					.AddField("data", data)
					.AddField("length", length));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnClientRanCommand(Connection connection, string command)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("admin_command").AddField("steam_id", connection.userid).AddField("owner_id", connection.ownerid)
					.AddField("ip", connection.ipaddress)
					.AddField("auth_level", connection.authLevel)
					.AddField("connected_time", connection.connectionTime)
					.AddField("username", connection.username)
					.AddField("command", command));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnPuzzleReset(PuzzleReset reset, float timeTaken, float timeSpentBlocked, float timeBlockedRadiation, float timeBeforeLooted)
		{
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				if (timeSpentBlocked == 0f || !reset.playersBlockReset || reset.timeBetweenResets <= 0f || reset.timeBetweenResets >= 100000f)
				{
					return;
				}
				List<SpawnGroup> spawnGroups = reset.GetSpawnGroups();
				if (spawnGroups.Count != 0)
				{
					string value = string.Join("|", from x in spawnGroups
						where (Object)(object)x != (Object)null
						select ((Object)x).name into x
						orderby x
						select x);
					EventRecord eventRecord = EventRecord.New("puzzle_reset").AddField("time_blocked_total", timeSpentBlocked).AddField("time_blocked_radiation", timeBlockedRadiation)
						.AddField("time_intended", reset.timeBetweenResets)
						.AddField("time_taken", timeTaken)
						.AddField("time_until_looted", timeBeforeLooted)
						.AddField("ai_zone_enabled", reset.CheckSleepingAIZForPlayers)
						.AddField("player_radius", reset.playerDetectionRadius)
						.AddField("ignore_above_ground", reset.ignoreAboveGroundPlayers)
						.AddField("scales_with_population", reset.scaleWithServerPopulation)
						.AddField("spawn_groups", value);
					AIInformationZone aIZone = reset.GetAIZone();
					if ((Object)(object)aIZone != (Object)null)
					{
						eventRecord.AddField("zone_size", ((Bounds)(ref aIZone.bounds)).size);
						Vector3 size = ((Bounds)(ref aIZone.bounds)).size;
						eventRecord.AddField("zone_size_magnitude", ((Vector3)(ref size)).magnitude);
					}
					MonumentInfo monumentInfo = (from x in spawnGroups
						where (Object)(object)x != (Object)null
						select ((Component)x).GetComponentInParent<MonumentInfo>()).FirstOrDefault((MonumentInfo x) => (Object)(object)x != (Object)null);
					if ((Object)(object)monumentInfo != (Object)null)
					{
						eventRecord.AddField("monument", ((Object)monumentInfo).name);
					}
					SubmitPoint(eventRecord);
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnDeepSeaTraverse(BasePlayer entity, bool entering, float timeToWipe)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("deep_sea_traverse").AddField("player", (BaseNetworkable)entity).AddField("entering", entering)
					.AddField("time_to_wipe", timeToWipe));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnDeepSeaToggled(bool toggle)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("deep_sea_toggle").AddField("open", toggle));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void OnPlayerBoatFinish(BasePlayer player, int blockCount, int deployableCount)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("player_boat_finish").AddField("player", (BaseNetworkable)player).AddField("block_count", blockCount)
					.AddField("deployable_count", deployableCount));
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}
	}

	private const int ShutdownTimeoutMs = 10000;

	public static AnalyticsManager Manager = new AnalyticsManager();

	private static bool started;

	private const string DryRunDocs = "Executes entire flow without actually sending out anything";

	[ConsoleVar(Clientside = true, Serverside = true, ServerAdmin = true, Help = "Executes entire flow without actually sending out anything")]
	public static bool DryRun;

	[ConsoleVar(Clientside = true, Serverside = true, ServerAdmin = true)]
	public static bool Log;

	public static string ClientAnalyticsUrl { get; set; } = "https://rust-api.facepunch.com/api/public/analytics/rust/client";

	[RconVar(Name = "server_analytics_url")]
	public static string ServerAnalyticsUrl { get; set; } = "https://rust-api.facepunch.com/api/public/analytics/rust/server";

	[RconVar(Name = "analytics_header", Saved = true, Help = "Header key of secret when uploading analytics")]
	public static string AnalyticsHeader { get; set; } = "X-API-KEY";

	[RconVar(Name = "analytics_secret", Saved = true, Help = "Header secret value when uploading analytics")]
	public static string AnalyticsSecret { get; set; } = "";

	public static string AnalyticsPublicKey { get; set; } = "pub878ABLezSB6onshSwBCRGYDCpEI";

	[RconVar(Name = "analytics_bulk_upload_url", Saved = true, Help = "Azure blob container url + SAS token, enables a more efficient upload method")]
	public static string BulkUploadConnectionString { get; set; }

	[RconVar(Name = "analytics_bulk_container_url", Saved = true, Help = "Azure blob container url for use with client secret authentication")]
	public static string BulkContainerUrl { get; set; }

	[RconVar(Name = "azure_tenant_id", Saved = true, Help = "Azure tenant id for authentication")]
	public static string AzureTenantId { get; set; }

	[RconVar(Name = "azure_client_id", Saved = true, Help = "Azure client id for authentication")]
	public static string AzureClientId { get; set; }

	[RconVar(Name = "azure_client_secret", Saved = true, Help = "Azure client secret for authentication")]
	public static string AzureClientSecret { get; set; }

	[RconVar(Name = "performance_analytics", Saved = true, Help = "Toggle to turn off server performance collection")]
	public static bool ServerPerformanceConVar { get; set; } = true;

	[RconVar(Name = "gameplay_analytics", Saved = true, Help = "Toggle whether gameplay analytics is collected")]
	public static bool GameplayAnalyticsConVar { get; set; }

	[RconVar(Name = "gameplay_tick_analytics", Saved = true, Help = "Toggle whether gameplay tick analytics is collected")]
	public static bool GameplayTickAnalyticsConVar { get; set; }

	[RconVar(Name = "gameplay_rpc_analytics", Saved = true, Help = "Toggle whether gameplay rpc logging is collected")]
	public static bool GameplayRpcAnalyticsConVar { get; set; } = false;

	public static void StartForServer()
	{
		if (!started)
		{
			Manager.StartThead();
			started = true;
		}
		Azure.Initialize();
	}

	public static void ShutdownForServer()
	{
		if (started)
		{
			Manager.Shutdown(10000);
		}
		started = false;
	}

	[ConsoleVar(Clientside = true, Serverside = true, ServerAdmin = true)]
	public static void Stats(ConsoleSystem.Arg arg)
	{
		AnalyticsManager.TelemStats telemStats = Manager.GatherStats();
		string empty = string.Empty;
		empty += $"Total Events: {telemStats.SerializedCount}\n";
		empty += $"Total Bytes: {telemStats.SerializedSize}\n";
		empty += $"Uploaded Bytes: {telemStats.UploadedSize}\n";
		empty += $"Queue Count: {telemStats.QueueCount}\n";
		empty += $"Max Queue Count: {telemStats.MaxQueueCount}";
		arg.ReplyWith(empty);
	}

	[ConsoleVar(Clientside = true, Serverside = true, ServerAdmin = true)]
	public static void TableStats(ConsoleSystem.Arg arg)
	{
		TextTable val = Pool.Get<TextTable>();
		val.ResizeColumns(8);
		val.AddColumn("Table Name");
		val.AddColumn("Uploader Name");
		val.AddColumn("Compressed");
		val.AddColumn("Upload interval");
		val.AddColumn("Total Events");
		val.AddColumn("Total Bytes");
		val.AddColumn("Avg Event/s");
		val.AddColumn("Avg Bytes/s");
		double totalSeconds = (DateTime.Now - Manager.StatsStartTime).TotalSeconds;
		ReadOnlySpan<AnalyticsManager.UploadingTable> tables = Manager.Tables;
		val.ResizeRows(tables.Length);
		ReadOnlySpan<AnalyticsManager.UploadingTable> readOnlySpan = tables;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			AnalyticsManager.UploadingTable uploadingTable = readOnlySpan[i];
			AnalyticsTable table = uploadingTable.Table;
			AnalyticsManager.IUploader uploader = uploadingTable.Uploader.Resolve();
			AnalyticsManager.IAccumulator obj = uploader?.GetAccumulatorFor(table) ?? null;
			val.AddValue(table.Name);
			val.AddValue(uploader?.Name ?? "None");
			val.AddValue(uploader?.IsCompressed ?? false);
			val.AddValue(table.UploadInterval.ToString());
			int num = obj?.ItemsWritten ?? 0;
			val.AddValue(num);
			long num2 = obj?.BytesWritten ?? 0;
			val.AddValue(num2);
			float num3 = (float)((double)num / totalSeconds);
			val.AddValue($"{num3:F2}");
			float num4 = (float)((double)num2 / totalSeconds);
			val.AddValue($"{num4:F2}");
		}
		arg.ReplyWith(((object)val).ToString());
		Pool.Free<TextTable>(ref val);
	}

	[ConsoleVar(Clientside = true, Serverside = true, ServerAdmin = true)]
	public static void UploaderStats(ConsoleSystem.Arg arg)
	{
		TextTable val = Pool.Get<TextTable>();
		val.ResizeColumns(7);
		val.AddColumn("Uploader Name");
		val.AddColumn("Is Enabled");
		val.AddColumn("Compressed");
		val.AddColumn("Total Events");
		val.AddColumn("Total Bytes");
		val.AddColumn("Avg Event/s");
		val.AddColumn("Avg Bytes/s");
		double totalSeconds = (DateTime.Now - Manager.StatsStartTime).TotalSeconds;
		ReadOnlySpan<AnalyticsManager.IUploader> uploaders = Manager.Uploaders;
		for (int i = 0; i < uploaders.Length; i++)
		{
			AnalyticsManager.IUploader uploader = uploaders[i];
			val.AddValue(uploader.Name);
			val.AddValue(uploader.Enabled);
			val.AddValue(uploader.IsCompressed);
			int itemsSerialized = uploader.ItemsSerialized;
			val.AddValue(itemsSerialized);
			long bytesSerialized = uploader.BytesSerialized;
			val.AddValue(bytesSerialized);
			float num = (float)((double)itemsSerialized / totalSeconds);
			val.AddValue($"{num:F2}");
			float num2 = (float)((double)bytesSerialized / totalSeconds);
			val.AddValue($"{num2:F2}");
		}
		arg.ReplyWith(((object)val).ToString());
		Pool.Free<TextTable>(ref val);
	}

	[ConsoleVar(Clientside = true, Serverside = true, ServerAdmin = true)]
	public static void ResetStats(ConsoleSystem.Arg arg)
	{
		Manager.ResetStats();
	}
}

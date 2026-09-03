using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using API.Analytics;
using Carbon.Base;
using Carbon.Components;
using Carbon.Contracts;
using Carbon.Core;
using Carbon.Extensions;
using ConVar;
using Facepunch;
using Facepunch.Math;
using Fleck;
using Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oxide.Core.Libraries;
using Oxide.Game.Rust.Cui;
using Oxide.Plugins;
using ProtoBuf;
using Rust;
using UnityEngine;

namespace Carbon;

public static class WebControlPanel
{
	public class Account
	{
		public string Name;

		public string Password;

		public Permissions Permissions = new Permissions(enabled: false);

		public static bool HasPermission(BridgeConnection connection, PermissionTypes permission)
		{
			if (!(connection.Reference is Account account))
			{
				return false;
			}
			return permission switch
			{
				PermissionTypes.ConsoleView => account.Permissions.console_view, 
				PermissionTypes.ConsoleInput => account.Permissions.console_input, 
				PermissionTypes.ChatView => account.Permissions.chat_view, 
				PermissionTypes.ChatInput => account.Permissions.chat_input, 
				PermissionTypes.PlayersView => account.Permissions.players_view, 
				PermissionTypes.PlayersIp => account.Permissions.players_ip, 
				PermissionTypes.PlayersInventory => account.Permissions.players_inventory, 
				PermissionTypes.EntitiesView => account.Permissions.entities_view, 
				PermissionTypes.EntitiesEdit => account.Permissions.entities_edit, 
				PermissionTypes.PermissionsView => account.Permissions.permissions_view, 
				PermissionTypes.PermissionsEdit => account.Permissions.permissions_edit, 
				PermissionTypes.ProfilerView => account.Permissions.profiler_view, 
				PermissionTypes.ProfilerLoad => account.Permissions.profiler_load, 
				PermissionTypes.ProfilerEdit => account.Permissions.profiler_edit, 
				PermissionTypes.PluginsView => account.Permissions.plugins_view, 
				PermissionTypes.PluginsEdit => account.Permissions.plugins_edit, 
				PermissionTypes.MapView => account.Permissions.map_view, 
				PermissionTypes.MapEntities => account.Permissions.map_entities, 
				PermissionTypes.MapTerrain => account.Permissions.map_terrain, 
				PermissionTypes.MapData => account.Permissions.map_data, 
				PermissionTypes.DrawUi => account.Permissions.draw_ui, 
				_ => false, 
			};
		}
	}

	public class Permissions(bool enabled)
	{
		public bool console_view = enabled;

		public bool console_input = enabled;

		public bool chat_view = enabled;

		public bool chat_input = enabled;

		public bool players_view = enabled;

		public bool players_ip = enabled;

		public bool players_inventory = enabled;

		public bool entities_view = enabled;

		public bool entities_edit = enabled;

		public bool permissions_view = enabled;

		public bool permissions_edit = enabled;

		public bool profiler_view = enabled;

		public bool profiler_load = enabled;

		public bool profiler_edit = enabled;

		public bool plugins_view = enabled;

		public bool plugins_edit = enabled;

		public bool map_view = enabled;

		public bool map_entities = enabled;

		public bool map_terrain = enabled;

		public bool map_data = enabled;

		public bool draw_ui = enabled;

		public void Serialize(BridgeWrite write)
		{
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, console_view);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, console_input);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, chat_view);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, chat_input);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, players_view);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, players_ip);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, players_inventory);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, entities_view);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, entities_edit);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, permissions_view);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, permissions_edit);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, profiler_view);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, profiler_load);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, profiler_edit);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, plugins_view);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, plugins_edit);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, map_view);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, map_entities);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, map_terrain);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, map_data);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, draw_ui);
		}
	}

	public enum PermissionTypes
	{
		None,
		ConsoleView,
		ConsoleInput,
		ChatView,
		ChatInput,
		PlayersView,
		PlayersIp,
		PlayersInventory,
		EntitiesView,
		EntitiesEdit,
		PermissionsView,
		PermissionsEdit,
		ProfilerView,
		ProfilerLoad,
		ProfilerEdit,
		PluginsView,
		PluginsEdit,
		MapView,
		MapEntities,
		MapTerrain,
		MapData,
		DrawUi
	}

	public class Config
	{
		public bool Enabled;

		public PanelConfig Panel = new PanelConfig();

		public ServerConfig BridgeServer = new ServerConfig();

		public Account[] WebAccounts = new Account[1]
		{
			new Account
			{
				Name = "owner",
				Password = RandomEx.GetRandomString(7),
				Permissions = new Permissions(enabled: true)
			}
		};

		public bool ShouldStartServer(out string reason)
		{
			if (!Enabled)
			{
				reason = "The server is disabled in the config";
				return false;
			}
			if (string.IsNullOrEmpty(BridgeServer.Ip))
			{
				reason = "The server IP isn't set in the config. Can just be set to 'localhost'";
				return false;
			}
			if (BridgeServer.Port == 0)
			{
				reason = "The server port isn't set in the config";
				return false;
			}
			reason = null;
			return true;
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class WebCall : Attribute
	{
		[AttributeUsage(AttributeTargets.Method)]
		public class Condition : Attribute
		{
			[AttributeUsage(AttributeTargets.Method)]
			public class Permission(PermissionTypes permission) : Condition
			{
				public PermissionTypes PermissionType = permission;

				public override bool Test(BridgeConnection connection)
				{
					return Account.HasPermission(connection, PermissionType);
				}
			}

			public virtual bool Test(BridgeConnection connection)
			{
				return true;
			}
		}

		public MethodInfo Method;

		public uint MethodId;

		public Condition[] Conditions;

		public void Setup(MethodInfo method)
		{
			Method = method;
			MethodId = Vault.Pool.Get(method.Name);
		}
	}

	public class WebBehaviour : FacepunchBehaviour
	{
		public void Update()
		{
			if (server != null)
			{
				Frame();
			}
		}
	}

	private struct ResponseError(ResponseErrorCodes code, string error)
	{
		[JsonProperty]
		public ResponseErrorCodes Code = code;

		[JsonProperty]
		public string Error = error;
	}

	private enum ResponseErrorCodes
	{
		InvalidArgs = 1,
		NoSuchFile
	}

	public struct EntityInfo
	{
		private ulong netId;

		private string name;

		private string shortName;

		private uint id;

		private int flags;

		private float posX;

		private float posY;

		private float posZ;

		private float rotX;

		private float rotY;

		private float rotZ;

		public EntityInfo(BaseEntity entity)
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Expected I4, but got Unknown
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			netId = ((BaseNetworkable)entity).net.ID.Value;
			name = ((Object)entity).name;
			shortName = ((BaseNetworkable)entity).ShortPrefabName;
			id = ((BaseNetworkable)entity).prefabID;
			flags = (int)entity.flags;
			posX = entity.ServerPosition.x;
			posY = entity.ServerPosition.y;
			posZ = entity.ServerPosition.z;
			Quaternion serverRotation = entity.ServerRotation;
			rotX = ((Quaternion)(ref serverRotation)).eulerAngles.x;
			serverRotation = entity.ServerRotation;
			rotY = ((Quaternion)(ref serverRotation)).eulerAngles.y;
			serverRotation = entity.ServerRotation;
			rotZ = ((Quaternion)(ref serverRotation)).eulerAngles.z;
		}

		public void Serialize(BridgeWrite write)
		{
			NetworkWriteEx.WriteObject<ulong>((NetWrite)(object)write, netId);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, name);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, shortName);
			NetworkWriteEx.WriteObject<uint>((NetWrite)(object)write, id);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, flags);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, posX);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, posY);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, posZ);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, rotX);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, rotY);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, rotZ);
		}
	}

	public struct DetailedEntityInfo
	{
		private ulong netId;

		private string name;

		private string shortName;

		private uint id;

		private string[] flags;

		private string type;

		private float posX;

		private float posY;

		private float posZ;

		private float rotX;

		private float rotY;

		private float rotZ;

		private ulong owner;

		private ulong skin;

		private BaseEntity parent;

		private List<BaseEntity> children;

		private BaseCombatEntity combatEntity;

		private BasePlayer playerEntity;

		public DetailedEntityInfo(BaseEntity entity)
		{
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			netId = ((BaseNetworkable)entity).net.ID.Value;
			name = ((Object)entity).name;
			shortName = ((BaseNetworkable)entity).ShortPrefabName;
			id = ((BaseNetworkable)entity).prefabID;
			flags = ((object)Unsafe.As<Flags, Flags>(ref entity.flags)/*cast due to constrained. prefix*/).ToString().Split(',');
			type = ((object)entity).GetType().Name;
			posX = entity.ServerPosition.x;
			posY = entity.ServerPosition.y;
			posZ = entity.ServerPosition.z;
			Quaternion serverRotation = entity.ServerRotation;
			rotX = ((Quaternion)(ref serverRotation)).eulerAngles.x;
			serverRotation = entity.ServerRotation;
			rotY = ((Quaternion)(ref serverRotation)).eulerAngles.y;
			serverRotation = entity.ServerRotation;
			rotZ = ((Quaternion)(ref serverRotation)).eulerAngles.z;
			owner = entity.OwnerID;
			skin = entity.skinID;
			parent = ((BaseNetworkable)entity).GetParentEntity();
			children = ((BaseNetworkable)entity).children;
			combatEntity = (BaseCombatEntity)(object)((entity is BaseCombatEntity) ? entity : null);
			playerEntity = (BasePlayer)(object)((entity is BasePlayer) ? entity : null);
		}

		public void Serialize(BridgeWrite write, bool ignoreParent = false)
		{
			NetworkWriteEx.WriteObject<ulong>((NetWrite)(object)write, netId);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, name);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, shortName);
			NetworkWriteEx.WriteObject<uint>((NetWrite)(object)write, id);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, flags.Length);
			for (int i = 0; i < flags.Length; i++)
			{
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, flags[i]);
			}
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, type);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, posX);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, posY);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, posZ);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, rotX);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, rotY);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, rotZ);
			NetworkWriteEx.WriteObject<ulong>((NetWrite)(object)write, owner);
			NetworkWriteEx.WriteObject<ulong>((NetWrite)(object)write, skin);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, BaseNetworkableEx.IsValid((BaseNetworkable)(object)parent) && !ignoreParent);
			if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)parent) && !ignoreParent)
			{
				new DetailedEntityInfo(parent).Serialize(write);
			}
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, children.Count);
			for (int j = 0; j < children.Count; j++)
			{
				new DetailedEntityInfo(children[j]).Serialize(write, ignoreParent: true);
			}
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, (Object)(object)combatEntity != (Object)null);
			if ((Object)(object)combatEntity != (Object)null)
			{
				NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, (float)Math.Round(((BaseEntity)combatEntity).Health(), 2));
				NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, (float)Math.Round(((BaseEntity)combatEntity).MaxHealth(), 2));
			}
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, (Object)(object)playerEntity != (Object)null);
			if ((Object)(object)playerEntity != (Object)null)
			{
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, playerEntity.displayName);
				NetworkWriteEx.WriteObject<ulong>((NetWrite)(object)write, playerEntity.userID.Get());
				NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, (float)Math.Round(((BaseMetabolism<BasePlayer>)(object)playerEntity.metabolism).hydration.value, 2));
				NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, (float)Math.Round(((BaseMetabolism<BasePlayer>)(object)playerEntity.metabolism).hydration.max, 2));
				NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, (float)Math.Round(((BaseMetabolism<BasePlayer>)(object)playerEntity.metabolism).calories.value, 2));
				NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, (float)Math.Round(((BaseMetabolism<BasePlayer>)(object)playerEntity.metabolism).calories.max, 2));
				NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, (float)Math.Round(playerEntity.metabolism.radiation_poison.value, 2));
				NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, (float)Math.Round(playerEntity.metabolism.radiation_poison.max, 2));
				NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, (float)Math.Round(playerEntity.metabolism.bleeding.value, 2));
				NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, (float)Math.Round(playerEntity.metabolism.bleeding.max, 2));
			}
		}
	}

	public struct EntitySearchRange
	{
		public Vector3 position;

		public float range;

		public string filter;

		public readonly bool isValid
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				if (position != Vector3.zero)
				{
					return range > 0f;
				}
				return false;
			}
		}

		public static EntitySearchRange Parse(string value)
		{
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			if (!value.Contains(":"))
			{
				return default(EntitySearchRange);
			}
			string[] array = value.Split(':');
			string[] array2 = array[0].Split(' ');
			return new EntitySearchRange
			{
				position = new Vector3(float.Parse(array2[0]), float.Parse(array2[1]), float.Parse(array2[2])),
				range = float.Parse(array[1]),
				filter = ((array.Length >= 3) ? array[2] : null)
			};
		}
	}

	public struct ItemInfo(Item item)
	{
		private int itemId = item.info?.itemid ?? 0;

		private string shortName = item.info?.shortname ?? "";

		private int position = item.position;

		private int amount = item.amount;

		private float maxCondition = item.maxCondition;

		private float condition = item.condition;

		private float conditionNormalized = item.conditionNormalized;

		private bool hasCondition = item.hasCondition;

		public void Serialize(BridgeWrite write)
		{
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, itemId);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, shortName);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, amount);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, position);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, maxCondition);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, condition);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, conditionNormalized);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, hasCondition);
		}
	}

	public struct MapInfo
	{
		private int imageWidth;

		private int imageHeight;

		private byte[] imageData;

		private uint worldSize;

		public bool IsValid()
		{
			return imageData != null;
		}

		public static MapInfo Get(float scale)
		{
			scale = scale.Clamp(0.1f, 1f);
			int num = default(int);
			int num2 = default(int);
			Color val = default(Color);
			MapInfo result = new MapInfo
			{
				imageData = MapImageRenderer.Render(ref num, ref num2, ref val, scale, false, true, 0),
				imageWidth = num,
				imageHeight = num2,
				worldSize = World.Size
			};
			Logger.Warn("Processed WebControlPanel map");
			return result;
		}

		public void Serialize(BridgeWrite write)
		{
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, imageWidth);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, imageHeight);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, imageData.Length);
			NetworkWriteEx.WriteObject<byte[]>((NetWrite)(object)write, imageData);
			NetworkWriteEx.WriteObject<uint>((NetWrite)(object)write, worldSize);
			PooledList<MapMonument> val = Pool.Get<PooledList<MapMonument>>();
			try
			{
				for (int i = 0; i < TerrainMeta.Path.Monuments.Count; i++)
				{
					MapMonument item = new MapMonument(TerrainMeta.Path.Monuments[i]);
					if (item.HasValidLabel())
					{
						((List<MapMonument>)(object)val).Add(item);
					}
				}
				NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, ((List<MapMonument>)(object)val).Count);
				for (int j = 0; j < ((List<MapMonument>)(object)val).Count; j++)
				{
					((List<MapMonument>)(object)val)[j].Serialize(write);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public struct MapData : IDisposable
	{
		public struct PrefabInfo
		{
			public string category;

			public uint id;

			public string path;

			public Vector3 position;

			public Vector3 rotation;

			public Vector3 scale;

			public void Serialize(BridgeWrite write)
			{
				//IL_0026: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Unknown result type (might be due to invalid IL or missing references)
				//IL_003e: Unknown result type (might be due to invalid IL or missing references)
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, category);
				NetworkWriteEx.WriteObject<uint>((NetWrite)(object)write, id);
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, path);
				NetworkWriteEx.WriteObject<Vector3>((NetWrite)(object)write, position);
				NetworkWriteEx.WriteObject<Vector3>((NetWrite)(object)write, rotation);
				NetworkWriteEx.WriteObject<Vector3>((NetWrite)(object)write, scale);
			}
		}

		public PooledList<PrefabInfo> prefabs;

		public static MapData Parse(WorldData data)
		{
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			MapData result = new MapData
			{
				prefabs = Pool.Get<PooledList<PrefabInfo>>()
			};
			for (int i = 0; i < data.prefabs.Count; i++)
			{
				PrefabData val = data.prefabs[i];
				PrefabInfo item = new PrefabInfo
				{
					category = val.category,
					id = val.id,
					path = StringPool.Get(val.id),
					position = VectorData.op_Implicit(val.position),
					rotation = VectorData.op_Implicit(val.rotation),
					scale = VectorData.op_Implicit(val.scale)
				};
				((List<PrefabInfo>)(object)result.prefabs).Add(item);
			}
			return result;
		}

		public void Dispose()
		{
			if (prefabs != null)
			{
				Pool.Free<PooledList<PrefabInfo>>(ref prefabs);
			}
		}

		public void Serialize(BridgeWrite write)
		{
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, ((List<PrefabInfo>)(object)prefabs).Count);
			for (int i = 0; i < ((List<PrefabInfo>)(object)prefabs).Count; i++)
			{
				((List<PrefabInfo>)(object)prefabs)[i].Serialize(write);
			}
		}
	}

	public struct MapMonument(MonumentInfo monument)
	{
		private string label = ((LandmarkInfo)monument).displayPhrase.english ?? GetCustomMarkerName(((Component)monument).gameObject);

		private Vector3 GetPosition()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			return TerrainMeta.Normalize(((Component)monument).transform.position);
		}

		public bool HasValidLabel()
		{
			return label.Length > 2;
		}

		public void Serialize(BridgeWrite write)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			Vector3 position = GetPosition();
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, label);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, position.x);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, position.z);
		}

		private static string GetCustomMarkerName(GameObject go)
		{
			foreach (KeyValuePair<string, HashSet<GameObject>> spawnedPrefab in World.SpawnedPrefabs)
			{
				if (spawnedPrefab.Value != null && spawnedPrefab.Value.Contains(go))
				{
					return spawnedPrefab.Key;
				}
			}
			return ((Object)go).name;
		}
	}

	public struct MapEntity(BaseEntity entity, MapEntity.Types type)
	{
		public enum Types
		{
			ActivePlayers,
			SleepingPlayers
		}

		private Vector3 GetPosition()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			return TerrainMeta.Normalize(((Component)entity).transform.position);
		}

		private string GetLabel()
		{
			BaseEntity val = entity;
			BasePlayer val2 = (BasePlayer)(object)((val is BasePlayer) ? val : null);
			if (val2 != null)
			{
				return val2.displayName;
			}
			return null;
		}

		public void Serialize(BridgeWrite write)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			Vector3 position = GetPosition();
			string label = GetLabel();
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, (int)type);
			NetworkWriteEx.WriteObject<NetworkableId>((NetWrite)(object)write, ((BaseNetworkable)entity).net.ID);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, !string.IsNullOrEmpty(label));
			if (!string.IsNullOrEmpty(label))
			{
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, label);
			}
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, position.x);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, position.z);
		}
	}

	public struct HookableInfo
	{
		private string name;

		private string author;

		private string[] permissions;

		public static HookableInfo Get(BaseHookable hookable, string[] permissions)
		{
			HookableInfo result = new HookableInfo
			{
				name = hookable.Name
			};
			if (hookable is RustPlugin rustPlugin)
			{
				result.author = rustPlugin.Author;
			}
			result.permissions = permissions;
			return result;
		}

		public void Serialize(BridgeWrite write)
		{
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, name);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, author);
			if (permissions == null)
			{
				NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, 0);
				return;
			}
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, permissions.Length);
			for (int i = 0; i < permissions.Length; i++)
			{
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, permissions[i]);
			}
		}
	}

	public struct UserInfo
	{
		private string displayName;

		private string steamId;

		private UserData data;

		private bool permissionsOnly;

		public static UserInfo Get(string steamId, UserData user, bool permissionsOnly = false)
		{
			return new UserInfo
			{
				displayName = user.LastSeenNickname,
				steamId = steamId,
				data = user,
				permissionsOnly = permissionsOnly
			};
		}

		public void Serialize(BridgeWrite write)
		{
			if (permissionsOnly)
			{
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, steamId);
				NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, data.Perms.Count);
				foreach (string perm in data.Perms)
				{
					NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, perm);
				}
				NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, data.Groups.Count);
				{
					foreach (string group in data.Groups)
					{
						NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, group);
					}
					return;
				}
			}
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, displayName);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, steamId);
		}
	}

	public struct PlayerInfo
	{
		private ulong steamId;

		private ulong ownerSteamId;

		private string displayName;

		private int ping;

		private string address;

		private ulong entityId;

		private int connectedSeconds;

		private float violationLevel;

		private int currentLevel;

		private int unspentXp;

		private float health;

		private TeamInfo team;

		public PlayerInfo(BasePlayer player)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			steamId = EncryptedValue<ulong>.op_Implicit(player.userID);
			ownerSteamId = ((BaseEntity)player).OwnerID;
			displayName = player.displayName;
			ping = (player.IsConnected ? Net.sv.GetAveragePing(player.Connection) : (-1));
			address = (player.IsConnected ? player.Connection.ipaddress : string.Empty);
			entityId = ((BaseNetworkable)player).net.ID.Value;
			connectedSeconds = player.secondsConnected;
			violationLevel = player.ViolationLevel;
			currentLevel = 0;
			unspentXp = 0;
			health = ((BaseCombatEntity)player).health;
			team = new TeamInfo(player.Team);
		}

		public void Serialize(BridgeWrite write, bool excludeIps)
		{
			NetworkWriteEx.WriteObject<ulong>((NetWrite)(object)write, steamId);
			NetworkWriteEx.WriteObject<ulong>((NetWrite)(object)write, ownerSteamId);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, displayName);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, ping);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, excludeIps ? "hidden" : address);
			NetworkWriteEx.WriteObject<ulong>((NetWrite)(object)write, entityId);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, connectedSeconds);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, violationLevel);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, currentLevel);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, unspentXp);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, health);
			team.Serialize(write);
		}
	}

	public struct TeamInfo(PlayerTeam team)
	{
		private bool hasTeam = team != null;

		private List<ulong> members = team?.members;

		private ulong leader = team?.teamLeader ?? 0;

		public void Serialize(BridgeWrite write)
		{
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, hasTeam);
			if (hasTeam)
			{
				NetworkWriteEx.WriteObject<ulong>((NetWrite)(object)write, leader);
				NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, members.Count);
				for (int i = 0; i < members.Count; i++)
				{
					NetworkWriteEx.WriteObject<ulong>((NetWrite)(object)write, members[i]);
				}
			}
		}
	}

	public struct PluginInfo(RustPlugin plugin)
	{
		private string name = plugin.Name;

		private string fileName = plugin.FileName;

		private string version = plugin.Version.ToString();

		private string author = plugin.Author;

		private string description = plugin.Description;

		public void Serialize(BridgeWrite write)
		{
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, name);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, fileName);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, version);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, author);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, description);
		}
	}

	public struct PluginDetails(RustPlugin plugin)
	{
		private int compileTime = (int)plugin.CompileTime.TotalMilliseconds;

		private int intCallHookGenTime = (int)plugin.InternalCallHookGenTime.TotalMilliseconds;

		private int uptime = (int)plugin.Uptime;

		private int memoryUsed = (int)plugin.TotalMemoryUsed;

		private bool hasInternalHookOverride = plugin.InternalCallHookOverriden;

		private bool hasConditionals = plugin.HasConditionals;

		private string[] permissions = plugin.permission.GetPermissions(plugin);

		private BaseHookable.HookCachePool hookPool = plugin.HookPool;

		public bool IsHookValid(BaseHookable.CachedHookInstance hook)
		{
			return plugin.Hooks.Contains(hook.PrimaryHook.Id);
		}

		public void Serialize(BridgeWrite write)
		{
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, compileTime);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, intCallHookGenTime);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, uptime);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, memoryUsed);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, hasInternalHookOverride);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)write, hasConditionals);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, permissions.Length);
			for (int i = 0; i < permissions.Length; i++)
			{
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, permissions[i]);
			}
			PooledList<BaseHookable.CachedHookInstance> val = Pool.Get<PooledList<BaseHookable.CachedHookInstance>>();
			try
			{
				foreach (KeyValuePair<uint, BaseHookable.CachedHookInstance> item in hookPool)
				{
					if (IsHookValid(item.Value))
					{
						((List<BaseHookable.CachedHookInstance>)(object)val).Add(item.Value);
					}
				}
				NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, ((List<BaseHookable.CachedHookInstance>)(object)val).Count);
				for (int j = 0; j < ((List<BaseHookable.CachedHookInstance>)(object)val).Count; j++)
				{
					new HookDetails(((List<BaseHookable.CachedHookInstance>)(object)val)[j]).Serialize(write);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public struct HookDetails(BaseHookable.CachedHookInstance instance)
	{
		private string name = instance.PrimaryHook.Name;

		private uint id = instance.PrimaryHook.Id;

		private float time = (float)instance.Hooks.Sum((BaseHookable.CachedHook x) => x.HookTime.TotalMilliseconds);

		private int fires = instance.Hooks.Sum((BaseHookable.CachedHook x) => x.TimesFired);

		private int memoryUsage = (int)instance.Hooks.Sum((BaseHookable.CachedHook x) => x.MemoryUsage);

		private int lagSpikes = instance.Hooks.Sum((BaseHookable.CachedHook x) => x.LagSpikes);

		private int asyncOverloads = instance.Hooks.Count((BaseHookable.CachedHook x) => x.IsAsync);

		private int debuggedOverloads = instance.Hooks.Count((BaseHookable.CachedHook x) => x.IsDebugged);

		public void Serialize(BridgeWrite write)
		{
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)write, name);
			NetworkWriteEx.WriteObject<uint>((NetWrite)(object)write, id);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)write, time);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, fires);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, memoryUsage);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, lagSpikes);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, asyncOverloads);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)write, debuggedOverloads);
		}
	}

	public struct ServerInfoOutput
	{
		private string Hostname;

		private int MaxPlayers;

		private int Players;

		private int Queued;

		private int Joining;

		private int ReservedSlots;

		private int EntityCount;

		private string GameTime;

		private int Uptime;

		private string Map;

		private float Framerate;

		private int Memory;

		private int MemoryUsageSystem;

		private int Collections;

		private int NetworkIn;

		private int NetworkOut;

		private bool Restarting;

		private string SaveCreatedTime;

		private int Version;

		private string Protocol;

		public static ServerInfoOutput Get()
		{
			if (!Community.IsServerInitialized)
			{
				return default(ServerInfoOutput);
			}
			return new ServerInfoOutput
			{
				Hostname = Server.hostname,
				MaxPlayers = Server.maxplayers,
				Players = BasePlayer.activePlayerList.Count,
				Queued = SingletonComponent<ServerMgr>.Instance.connectionQueue.Queued,
				Joining = SingletonComponent<ServerMgr>.Instance.connectionQueue.Joining,
				ReservedSlots = SingletonComponent<ServerMgr>.Instance.connectionQueue.ReservedCount,
				EntityCount = BaseNetworkable.serverEntities.Count,
				GameTime = (((Object)(object)TOD_Sky.Instance != (Object)null) ? TOD_Sky.Instance.Cycle.DateTime.ToString(CultureInfo.CurrentCulture) : DateTime.UtcNow.ToString(CultureInfo.CurrentCulture)),
				Uptime = (int)Time.realtimeSinceStartup,
				Map = Server.level,
				Framerate = Performance.report.frameRate,
				Memory = (int)Performance.report.memoryAllocations,
				MemoryUsageSystem = (int)Performance.report.memoryUsageSystem,
				Collections = (int)Performance.report.memoryCollections,
				NetworkIn = (int)((Net.sv != null) ? ((BaseNetwork)Net.sv).GetStat((Connection)null, (StatTypeLong)3) : 0),
				NetworkOut = (int)((Net.sv != null) ? ((BaseNetwork)Net.sv).GetStat((Connection)null, (StatTypeLong)1) : 0),
				Restarting = SingletonComponent<ServerMgr>.Instance.Restarting,
				SaveCreatedTime = SaveRestore.SaveCreatedTime.ToString(CultureInfo.CurrentCulture),
				Version = 2633,
				Protocol = Protocol.printable
			};
		}

		public void Serialize(BridgeWrite obj)
		{
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)obj, Hostname);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)obj, MaxPlayers);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)obj, Players);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)obj, Queued);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)obj, Joining);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)obj, ReservedSlots);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)obj, EntityCount);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)obj, GameTime);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)obj, Uptime);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)obj, Map);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)obj, Framerate);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)obj, Memory);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)obj, MemoryUsageSystem);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)obj, Collections);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)obj, NetworkIn);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)obj, NetworkOut);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)obj, Restarting);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)obj, SaveCreatedTime);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)obj, Version);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)obj, Protocol);
		}
	}

	public class ServerConfig
	{
		public string Ip = "localhost";

		public int Port;

		public int MaxConnections = 500;

		public int MaxConnectionsPerIp = 5;
	}

	public class PanelConfig
	{
		public float MapImageScale = 1f;
	}

	public class Server : BridgeServer
	{
		public override void OnServerConnected()
		{
			base.OnServerConnected();
			Analytics.webcontrolpanel_serverconnect();
		}

		public override bool OnPasswordValidate(string password)
		{
			return true;
		}

		public override bool OnSocketValidate(IWebSocketConnection socket)
		{
			Account account;
			return TryFindAccount(socket.ConnectionInfo.Path.TrimStart('/'), out account);
		}

		public override void OnBridgeConnection(BridgeConnection connection)
		{
			if (TryFindAccount(connection.Socket.ConnectionInfo.Path.TrimStart('/'), out var account))
			{
				Analytics.webcontrolpanel_clientconnect();
				connection.Reference = account;
			}
		}

		public override void OnBridgeDisconnection(BridgeConnection connection)
		{
		}
	}

	public class ServerMessages : BridgeMessages
	{
		public override bool ShouldPool => false;

		protected override void OnCommand(BridgeRead read)
		{
		}

		protected override void OnCustom(BridgeRead read)
		{
		}

		protected override void OnRpc(BridgeRead read)
		{
			EnqueueRpc(read);
		}

		protected override void OnUnhandled(BridgeRead read)
		{
		}
	}

	public static Dictionary<uint, WebCall> rpcs = new Dictionary<uint, WebCall>();

	public static Server server;

	public static ServerMessages serverMessages = new ServerMessages();

	public static Config config;

	private static uint currentRpcId;

	private static object[] args = new object[1] { 1 };

	private static Queue<BridgeRead> reads = new Queue<BridgeRead>();

	private const string Ok = "ok";

	public static readonly uint CHAT_LOG = Vault.Pool.Get("RPC_ChatLog");

	public static readonly uint CONSOLE_LOG = Vault.Pool.Get("RPC_ConsoleLog");

	public static MapInfo MAPINFO_CACHE;

	public static readonly uint PLUGINS = Vault.Pool.Get("RPC_Plugins");

	[WebCall]
	private static void RPC_AccountPermissions(BridgeRead read)
	{
		if (read.Connection.Reference is Account { Permissions: { } permissions })
		{
			BridgeWrite write = StartRpcResponse();
			permissions.Serialize(write);
			SendRpcResponse(read.Connection, write);
		}
	}

	public static bool TryFindAccount(string password, out Account account)
	{
		if (string.IsNullOrEmpty(password))
		{
			account = null;
			return false;
		}
		return (account = FindAccount(password)) != null;
	}

	public static Account FindAccount(string password)
	{
		for (int i = 0; i < config.WebAccounts.Length; i++)
		{
			Account account = config.WebAccounts[i];
			if (account.Password == password)
			{
				return account;
			}
		}
		return null;
	}

	public static void LoadConfig()
	{
		string webPanelConfigFile = Defines.GetWebPanelConfigFile();
		if (!File.Exists(webPanelConfigFile))
		{
			SaveConfig();
			return;
		}
		config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(webPanelConfigFile));
		SaveConfig();
		RestartServer();
	}

	public static void SaveConfig()
	{
		File.WriteAllText(Defines.GetWebPanelConfigFile(), JsonConvert.SerializeObject((object)(config ?? (config = new Config())), (Formatting)1));
	}

	public static void RestartServer()
	{
		server?.Shutdown();
		if (config.ShouldStartServer(out var reason))
		{
			BridgeServerInfo serverInfo = new BridgeServerInfo
			{
				port = config.BridgeServer.Port,
				ip = config.BridgeServer.Ip,
				messages = serverMessages,
				context = "WebControlPanel",
				maxConnections = config.BridgeServer.MaxConnections,
				maxConnectionsPerIp = config.BridgeServer.MaxConnectionsPerIp
			};
			(server ?? (server = new Server())).Start(serverInfo);
			if (Community.IsServerInitialized && !MAPINFO_CACHE.IsValid())
			{
				MAPINFO_CACHE = MapInfo.Get(config.Panel.MapImageScale);
			}
		}
		if (config.Enabled && !string.IsNullOrEmpty(reason))
		{
			Logger.Warn("WebControlPanel couldn't start: " + reason);
		}
	}

	public static void Init()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		LoadConfig();
		rpcs.Clear();
		MethodInfo[] methods = typeof(WebControlPanel).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			WebCall customAttribute = methodInfo.GetCustomAttribute<WebCall>();
			if (customAttribute != null)
			{
				customAttribute.Setup(methodInfo);
				customAttribute.Conditions = methodInfo.GetCustomAttributes<WebCall.Condition>().ToArray();
				rpcs[customAttribute.MethodId] = customAttribute;
			}
		}
		Output.OnPostMessage += OnLog;
		WebBehaviour webBehaviour = new GameObject("WebControlPanel").AddComponent<WebBehaviour>();
		Object.DontDestroyOnLoad((Object)(object)((Component)webBehaviour).gameObject);
	}

	public static void ServerInit()
	{
		if (config.ShouldStartServer(out var reason))
		{
			MAPINFO_CACHE = MapInfo.Get(config.Panel.MapImageScale);
		}
		if (config.Enabled && !string.IsNullOrEmpty(reason))
		{
			Logger.Warn("WebControlPanel couldn't start: " + reason);
		}
	}

	public static void Shutdown()
	{
		Output.OnPostMessage -= OnLog;
		if (server != null)
		{
			server.Shutdown();
			server = null;
		}
	}

	public static void Frame()
	{
		while (reads.Count > 0)
		{
			BridgeRead read = reads.Dequeue();
			if (read != null)
			{
				RunRpc(read);
				BridgeRead.Return(ref read);
			}
		}
	}

	private static void RunRpc(BridgeRead read)
	{
		currentRpcId = ((NetRead)read).UInt32();
		if (!rpcs.TryGetValue(currentRpcId, out var value))
		{
			return;
		}
		if (value.Conditions != null)
		{
			for (int i = 0; i < value.Conditions.Length; i++)
			{
				WebCall.Condition condition = value.Conditions[i];
				if (!condition.Test(read.Connection))
				{
					return;
				}
			}
		}
		try
		{
			args[0] = read;
			value.Method.Invoke(null, args);
		}
		catch (Exception ex)
		{
			Logger.Error("Failed WebControlPanel.RunRpc", ex.InnerException);
		}
	}

	private static void EnqueueRpc(BridgeRead read)
	{
		reads.Enqueue(read);
	}

	private static ItemContainer FindContainer(int id, BasePlayer player)
	{
		return (ItemContainer)(id switch
		{
			0 => player.inventory.containerMain, 
			1 => player.inventory.containerBelt, 
			2 => player.inventory.containerWear, 
			_ => null, 
		});
	}

	private static string CompressStringToBase64(string data)
	{
		if (string.IsNullOrEmpty(data))
		{
			return string.Empty;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(data);
		using MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
		{
			gZipStream.Write(bytes, 0, bytes.Length);
		}
		return Convert.ToBase64String(memoryStream.ToArray());
	}

	private static string DecompressBase64ToString(string base64)
	{
		if (string.IsNullOrEmpty(base64))
		{
			return string.Empty;
		}
		byte[] buffer = Convert.FromBase64String(base64);
		using MemoryStream stream = new MemoryStream(buffer);
		using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
		using MemoryStream memoryStream = new MemoryStream();
		gZipStream.CopyTo(memoryStream);
		return Encoding.UTF8.GetString(memoryStream.ToArray());
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.ChatView)]
	private static void RPC_ChatTail(BridgeRead read)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected I4, but got Unknown
		int count = Chat.History.Size - ((NetRead)read).Int32();
		PooledList<ChatEntry> val = Pool.Get<PooledList<ChatEntry>>();
		try
		{
			((List<ChatEntry>)(object)val).AddRange(((IEnumerable<ChatEntry>)Chat.History).Skip(count));
			BridgeWrite bridgeWrite = StartRpcResponse();
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, ((List<ChatEntry>)(object)val).Count);
			foreach (ChatEntry item in (List<ChatEntry>)(object)val)
			{
				ChatEntry current = item;
				NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, (int)((ChatEntry)(ref current)).Channel);
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, ((ChatEntry)(ref current)).Message);
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, ((ChatEntry)(ref current)).UserId);
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, ((ChatEntry)(ref current)).Username);
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, ((ChatEntry)(ref current)).Color);
				NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, ((ChatEntry)(ref current)).Time);
			}
			SendRpcResponse(read.Connection, bridgeWrite);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.ChatInput)]
	private static void RPC_ChatInput(BridgeRead read)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		string text = StringEx.EscapeRichText(((NetRead)read).String(256, false), false);
		string text2 = ((NetRead)read).String(256, false);
		string text3 = ((NetRead)read).String(256, false);
		string text4 = ((NetRead)read).String(256, false);
		ConsoleNetwork.BroadcastToAllClients("chat.add", new object[3]
		{
			2,
			text4,
			"<color=" + text3 + ">" + text + "</color>: " + text2
		});
		ChatEntry val = default(ChatEntry);
		((ChatEntry)(ref val)).Channel = (ChatChannel)0;
		((ChatEntry)(ref val)).Message = text2;
		((ChatEntry)(ref val)).UserId = text4;
		((ChatEntry)(ref val)).Username = text;
		((ChatEntry)(ref val)).Color = text3;
		((ChatEntry)(ref val)).Time = Epoch.Current;
		Chat.Record(val);
	}

	public static void OnPlayerChat(BasePlayer player, string message, ChatChannel channel)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected I4, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		if (server == null)
		{
			return;
		}
		PooledList<BridgeConnection> val = Pool.Get<PooledList<BridgeConnection>>();
		try
		{
			for (int i = 0; i < server.ConnectionsList.Count; i++)
			{
				BridgeConnection bridgeConnection = server.ConnectionsList[i];
				if (bridgeConnection.Reference is Account account && account.Permissions.chat_view)
				{
					((List<BridgeConnection>)(object)val).Add(bridgeConnection);
				}
			}
			if (((List<BridgeConnection>)(object)val).Count != 0)
			{
				BridgeWrite bridgeWrite = StartRpcResponse(CHAT_LOG);
				NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, (int)channel);
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, message);
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, player.UserIDString);
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, player.displayName);
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, Chat.GetNameColor(EncryptedValue<ulong>.op_Implicit(player.userID), player));
				NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, Epoch.Current);
				SendRpcResponse((List<BridgeConnection>)(object)val, bridgeWrite);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.ConsoleView)]
	private static void RPC_ConsoleTail(BridgeRead read)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		int count = Math.Min(0, Output.HistoryOutput.Count - ((NetRead)read).Int32());
		PooledList<Entry> val = Pool.Get<PooledList<Entry>>();
		try
		{
			((List<Entry>)(object)val).AddRange(Output.HistoryOutput.Skip(count));
			BridgeWrite bridgeWrite = StartRpcResponse();
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, ((List<Entry>)(object)val).Count);
			foreach (Entry item in (List<Entry>)(object)val)
			{
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, item.Message);
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, item.Type);
				NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, item.Time);
			}
			SendRpcResponse(read.Connection, bridgeWrite);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.ConsoleInput)]
	private static void RPC_ConsoleInput(BridgeRead read)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		string text = ((NetRead)read).String(256, false);
		BridgeConnection connection = read.Connection;
		Option val = Option.Server;
		string text2 = ConsoleSystem.Run(((Option)(ref val)).Quiet(), text, Array.Empty<object>());
		if (!string.IsNullOrEmpty(text2))
		{
			connection.Reply(text2);
		}
	}

	private static void OnLog(string message, string stacktrace, LogType type)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (server == null)
		{
			return;
		}
		PooledList<BridgeConnection> val = Pool.Get<PooledList<BridgeConnection>>();
		try
		{
			for (int i = 0; i < server.ConnectionsList.Count; i++)
			{
				BridgeConnection bridgeConnection = server.ConnectionsList[i];
				if (bridgeConnection.Reference is Account account && account.Permissions.console_view)
				{
					((List<BridgeConnection>)(object)val).Add(bridgeConnection);
				}
			}
			if (((List<BridgeConnection>)(object)val).Count != 0)
			{
				BridgeWrite bridgeWrite = StartRpcResponse(CONSOLE_LOG);
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, message);
				NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, Output.LogTypeToString.Get(type));
				NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, Epoch.Current);
				SendRpcResponse((List<BridgeConnection>)(object)val, bridgeWrite);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.DrawUi)]
	private static void RPC_AddUi(BridgeRead read)
	{
		BasePlayer val = BasePlayer.FindAwakeOrSleepingByID(((NetRead)read).UInt64());
		if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)val))
		{
			string text = ((NetRead)read).StringRaw(4195328, false);
			if (!string.IsNullOrWhiteSpace(text))
			{
				CuiHelper.AddUi(val, text);
			}
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.DrawUi)]
	private static void RPC_DestroyUi(BridgeRead read)
	{
		BasePlayer val = BasePlayer.FindAwakeOrSleepingByID(((NetRead)read).UInt64());
		if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)val))
		{
			string text = ((NetRead)read).String(256, false);
			if (!string.IsNullOrWhiteSpace(text))
			{
				CuiHelper.DestroyUi(val, text);
			}
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.EntitiesView)]
	private static void RPC_SearchEntities(BridgeRead read)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		int num = ((NetRead)read).Int32().Clamp(1, 200);
		string text = ((NetRead)read).String(256, false);
		EntitySearchRange entitySearchRange = EntitySearchRange.Parse(text);
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			if (entitySearchRange.isValid)
			{
				Vis.Entities<BaseEntity>(entitySearchRange.position, entitySearchRange.range, (List<BaseEntity>)(object)val, -1, (QueryTriggerInteraction)1);
				int num2 = ((List<BaseEntity>)(object)val).Count - 1;
				while (num2 >= 0 && !string.IsNullOrEmpty(entitySearchRange.filter))
				{
					BaseEntity val2 = ((List<BaseEntity>)(object)val)[num2];
					if (((List<BaseEntity>)(object)val).Count > num || !StringEx.Contains(((object)val2).GetType().Name, entitySearchRange.filter, CompareOptions.OrdinalIgnoreCase) || StringEx.Contains(((BaseNetworkable)val2).PrefabName, entitySearchRange.filter, CompareOptions.OrdinalIgnoreCase) || StringEx.Contains(((BaseNetworkable)val2).net.ID.Value.ToString(), entitySearchRange.filter, CompareOptions.OrdinalIgnoreCase))
					{
						((List<BaseEntity>)(object)val).RemoveAt(num2);
					}
					num2--;
				}
			}
			else
			{
				foreach (BaseEntity item in ((IEnumerable)BaseNetworkable.serverEntities).OfType<BaseEntity>())
				{
					if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)item) && !((BaseNetworkable)item).IsDestroyed)
					{
						if (StringEx.Contains(((object)item).GetType().Name, text, CompareOptions.OrdinalIgnoreCase) || StringEx.Contains(((BaseNetworkable)item).PrefabName, text, CompareOptions.OrdinalIgnoreCase) || StringEx.Contains(((BaseNetworkable)item).net.ID.Value.ToString(), text, CompareOptions.OrdinalIgnoreCase))
						{
							((List<BaseEntity>)(object)val).Add(item);
						}
						if (((List<BaseEntity>)(object)val).Count >= num)
						{
							break;
						}
					}
				}
			}
			BridgeWrite bridgeWrite = StartRpcResponse();
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, ((List<BaseEntity>)(object)val).Count);
			for (int i = 0; i < ((List<BaseEntity>)(object)val).Count; i++)
			{
				new EntityInfo(((List<BaseEntity>)(object)val)[i]).Serialize(bridgeWrite);
			}
			SendRpcResponse(read.Connection, bridgeWrite);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.EntitiesView)]
	private static void RPC_EntityDetails(BridgeRead read)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		BaseNetworkable obj = BaseNetworkable.serverEntities.Find(new NetworkableId(((NetRead)read).UInt64()));
		BaseEntity val = (BaseEntity)(object)((obj is BaseEntity) ? obj : null);
		if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)val))
		{
			BridgeWrite write = StartRpcResponse();
			new DetailedEntityInfo(val).Serialize(write);
			SendRpcResponse(read.Connection, write);
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.EntitiesEdit)]
	private static void RPC_EntitySave(BridgeRead read)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		JObject val = JObject.Parse(((NetRead)read).String(((NetRead)read).Int32(), false));
		BaseNetworkable obj = BaseNetworkable.serverEntities.Find(new NetworkableId(val["NetId"].ToObject<ulong>()));
		BaseEntity val2 = (BaseEntity)(object)((obj is BaseEntity) ? obj : null);
		if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)val2))
		{
			val2.OwnerID = val["Owner"].ToObject<ulong>();
			val2.skinID = val["Skin"].ToObject<ulong>();
			val2.ServerPosition = new Vector3(val["PosX"].ToObject<float>(), val["PosY"].ToObject<float>(), val["PosZ"].ToObject<float>());
			val2.ServerRotation = Quaternion.Euler(new Vector3(val["RotX"].ToObject<float>(), val["RotY"].ToObject<float>(), val["RotZ"].ToObject<float>()));
			BasePlayer val3 = (BasePlayer)(object)((val2 is BasePlayer) ? val2 : null);
			if (val3 != null)
			{
				JToken val4 = val["PlayerEntity"];
				((BaseMetabolism<BasePlayer>)(object)val3.metabolism).hydration.max = val4[(object)"MaxThirst"].ToObject<float>();
				((BaseMetabolism<BasePlayer>)(object)val3.metabolism).hydration.value = val4[(object)"Thirst"].ToObject<float>();
				((BaseMetabolism<BasePlayer>)(object)val3.metabolism).calories.max = val4[(object)"MaxHunger"].ToObject<float>();
				((BaseMetabolism<BasePlayer>)(object)val3.metabolism).calories.value = val4[(object)"Hunger"].ToObject<float>();
				val3.metabolism.radiation_poison.max = val4[(object)"MaxRads"].ToObject<float>();
				val3.metabolism.radiation_poison.value = val4[(object)"Rads"].ToObject<float>();
				val3.metabolism.bleeding.max = val4[(object)"MaxBleed"].ToObject<float>();
				val3.metabolism.bleeding.value = val4[(object)"Bleed"].ToObject<float>();
			}
			BaseCombatEntity val5 = (BaseCombatEntity)(object)((val2 is BaseCombatEntity) ? val2 : null);
			if (val5 != null)
			{
				JToken val6 = val["CombatEntity"];
				val5.SetMaxHealth(val6[(object)"MaxHealth"].ToObject<float>());
				val5.SetHealth(val6[(object)"Health"].ToObject<float>());
			}
			((BaseNetworkable)val2).SendNetworkUpdate((NetworkQueue)0);
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.EntitiesEdit)]
	private static void RPC_EntityKill(BridgeRead read)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		BaseNetworkable obj = BaseNetworkable.serverEntities.Find(new NetworkableId(((NetRead)read).UInt64()));
		BaseEntity val = (BaseEntity)(object)((obj is BaseEntity) ? obj : null);
		if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)val))
		{
			BasePlayer val2 = (BasePlayer)(object)((val is BasePlayer) ? val : null);
			if (val2 != null)
			{
				((BaseCombatEntity)val2).Hurt(((BaseEntity)val2).MaxHealth() + 1f);
			}
			else
			{
				((BaseNetworkable)val).AdminKill();
			}
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.PlayersInventory)]
	private static void RPC_SendPlayerInventory(BridgeRead read)
	{
		BasePlayer val = BasePlayer.FindAwakeOrSleepingByID(((NetRead)read).UInt64());
		if (!BaseNetworkableEx.IsValid((BaseNetworkable)(object)val))
		{
			return;
		}
		PlayerInventory inventory = val.inventory;
		BaseEntity entitySource = inventory.loot.entitySource;
		StorageContainer val2 = (StorageContainer)(object)((entitySource is StorageContainer) ? entitySource : null);
		BridgeWrite bridgeWrite = StartRpcResponse();
		NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, val.GetActiveItem()?.position ?? (-1));
		NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, inventory.containerMain.itemList.Count);
		for (int i = 0; i < inventory.containerMain.itemList.Count; i++)
		{
			new ItemInfo(inventory.containerMain.itemList[i]).Serialize(bridgeWrite);
		}
		NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, inventory.containerBelt.itemList.Count);
		for (int j = 0; j < inventory.containerBelt.itemList.Count; j++)
		{
			new ItemInfo(inventory.containerBelt.itemList[j]).Serialize(bridgeWrite);
		}
		NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, inventory.containerWear.itemList.Count);
		for (int k = 0; k < inventory.containerWear.itemList.Count; k++)
		{
			new ItemInfo(inventory.containerWear.itemList[k]).Serialize(bridgeWrite);
		}
		NetworkWriteEx.WriteObject<bool>((NetWrite)(object)bridgeWrite, (Object)(object)val2 != (Object)null);
		if ((Object)(object)val2 != (Object)null)
		{
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, val2.panelName);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, val2.inventory.itemList.Count);
			for (int l = 0; l < val2.inventory.itemList.Count; l++)
			{
				new ItemInfo(val2.inventory.itemList[l]).Serialize(bridgeWrite);
			}
		}
		SendRpcResponse(read.Connection, bridgeWrite);
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.PlayersInventory)]
	private static void RPC_MoveInventoryItem(BridgeRead read)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer val = BasePlayer.FindAwakeOrSleepingByID(((NetRead)read).UInt64());
		if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)val))
		{
			ItemContainer val2 = FindContainer(((NetRead)read).Int32(), val);
			int fromPosition = ((NetRead)read).Int32();
			Item val3 = val2.itemList.FirstOrDefault((Item x) => x.position.Equals(fromPosition));
			int num = ((NetRead)read).Int32();
			switch (num)
			{
			case 10:
				val3.Drop(((BaseEntity)val).GetDropPosition(), ((BaseEntity)val).GetDropVelocity(), default(Quaternion));
				break;
			case 11:
				val3.Remove(0f);
				break;
			default:
				val3.MoveToContainer(FindContainer(num, val), ((NetRead)read).Int32(), true, false, (BasePlayer)null, true);
				break;
			}
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.MapView)]
	private static void RPC_LoadMapInfo(BridgeRead read)
	{
		if (MAPINFO_CACHE.IsValid())
		{
			BridgeWrite write = StartRpcResponse();
			MAPINFO_CACHE.Serialize(write);
			SendRpcResponse(read.Connection, write);
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.MapEntities)]
	private static void RPC_RequestMapEntities(BridgeRead read)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if (!MAPINFO_CACHE.IsValid())
		{
			return;
		}
		int num = ((NetRead)read).Int32();
		PooledList<MapEntity> val = Pool.Get<PooledList<MapEntity>>();
		try
		{
			EntityRealm serverEntities = BaseNetworkable.serverEntities;
			BufferList<BaseNetworkable> values = serverEntities.entityList.Get().Values;
			bool flag = Account.HasPermission(read.Connection, PermissionTypes.PlayersView);
			for (int i = 0; i < num; i++)
			{
				MapEntity.Types types = (MapEntity.Types)((NetRead)read).Int32();
				for (int j = 0; j < values.Count; j++)
				{
					switch (types)
					{
					case MapEntity.Types.ActivePlayers:
						if (flag)
						{
							BaseNetworkable obj2 = values[j];
							BasePlayer val3 = (BasePlayer)(object)((obj2 is BasePlayer) ? obj2 : null);
							if (val3 != null && val3.IsConnected && val3.userID.IsSteamId())
							{
								((List<MapEntity>)(object)val).Add(new MapEntity((BaseEntity)(object)val3, types));
							}
						}
						break;
					case MapEntity.Types.SleepingPlayers:
						if (flag)
						{
							BaseNetworkable obj = values[j];
							BasePlayer val2 = (BasePlayer)(object)((obj is BasePlayer) ? obj : null);
							if (val2 != null && !val2.IsConnected && val2.userID.IsSteamId())
							{
								((List<MapEntity>)(object)val).Add(new MapEntity((BaseEntity)(object)val2, types));
							}
						}
						break;
					}
				}
			}
			BridgeWrite bridgeWrite = StartRpcResponse();
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, ((List<MapEntity>)(object)val).Count);
			for (int k = 0; k < ((List<MapEntity>)(object)val).Count; k++)
			{
				((List<MapEntity>)(object)val)[k].Serialize(bridgeWrite);
			}
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)bridgeWrite, TOD_Sky.Instance.Cycle.Hour);
			SendRpcResponse(read.Connection, bridgeWrite);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.MapTerrain)]
	private static void RPC_LoadTerrain(BridgeRead read)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		byte[] array = ((TerrainMap<short>)(object)TerrainMeta.HeightMap).ToByteArray();
		BridgeWrite bridgeWrite = StartRpcResponse();
		NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, ((TerrainMap)TerrainMeta.HeightMap).res);
		NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, (int)World.Size);
		NetworkWriteEx.WriteObject<float>((NetWrite)(object)bridgeWrite, TerrainMeta.Position.y);
		NetworkWriteEx.WriteObject<float>((NetWrite)(object)bridgeWrite, TerrainMeta.Size.y);
		NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, array.Length);
		NetworkWriteEx.WriteObject<byte[]>((NetWrite)(object)bridgeWrite, array);
		SendRpcResponse(read.Connection, bridgeWrite);
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.MapData)]
	private static void RPC_LoadMap(BridgeRead read)
	{
		byte[] array = ((TerrainMap<short>)(object)TerrainMeta.HeightMap).ToByteArray();
		BridgeWrite write = StartRpcResponse();
		MapData mapData = MapData.Parse(World.Serialization.world);
		try
		{
			mapData.Serialize(write);
			Debug.Log((object)$"Sending {((List<MapData.PrefabInfo>)(object)mapData.prefabs).Count} prefabs");
			SendRpcResponse(read.Connection, write);
		}
		finally
		{
			((IDisposable)mapData/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.PermissionsView)]
	private static void RPC_GetPermissionsMetadata(BridgeRead read)
	{
		if (Community.Runtime == null || Community.Runtime.Core == null || Community.Runtime.Core.permission == null)
		{
			return;
		}
		Permission permission = Community.Runtime.Core.permission;
		PooledList<HookableInfo> val = Pool.Get<PooledList<HookableInfo>>();
		try
		{
			PooledList<HookableInfo> val2 = Pool.Get<PooledList<HookableInfo>>();
			try
			{
				foreach (ModLoader.Package package in ModLoader.Packages)
				{
					if (package.IsCoreMod)
					{
						continue;
					}
					foreach (RustPlugin plugin in package.Plugins)
					{
						string[] permissions = permission.GetPermissions(plugin);
						if (permissions != null && permissions.Length != 0)
						{
							((List<HookableInfo>)(object)val).Add(HookableInfo.Get(plugin, permissions));
						}
					}
				}
				foreach (BaseHookable module in Community.Runtime.ModuleProcessor.Modules)
				{
					string[] permissions2 = permission.GetPermissions(module);
					if (permissions2 != null && permissions2.Length != 0)
					{
						((List<HookableInfo>)(object)val2).Add(HookableInfo.Get(module, permissions2));
					}
				}
				BridgeWrite bridgeWrite = StartRpcResponse();
				string[] groups = permission.GetGroups();
				string[] permissions3 = permission.GetPermissions();
				NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, groups.Length);
				for (int i = 0; i < groups.Length; i++)
				{
					NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, groups[i]);
				}
				NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, permissions3.Length);
				for (int j = 0; j < permissions3.Length; j++)
				{
					NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, permissions3[j]);
				}
				NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, ((List<HookableInfo>)(object)val).Count);
				for (int k = 0; k < ((List<HookableInfo>)(object)val).Count; k++)
				{
					((List<HookableInfo>)(object)val)[k].Serialize(bridgeWrite);
				}
				NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, ((List<HookableInfo>)(object)val2).Count);
				for (int l = 0; l < ((List<HookableInfo>)(object)val2).Count; l++)
				{
					((List<HookableInfo>)(object)val2)[l].Serialize(bridgeWrite);
				}
				SendRpcResponse(read.Connection, bridgeWrite);
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

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.PermissionsView)]
	private static void RPC_GetGroupPermissions(BridgeRead read)
	{
		string name = ((NetRead)read).String(256, false);
		string[] groupPermissions = Community.Runtime.Core.permission.GetGroupPermissions(name, parents: true);
		BridgeWrite bridgeWrite = StartRpcResponse();
		NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, groupPermissions.Length);
		for (int i = 0; i < groupPermissions.Length; i++)
		{
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, groupPermissions[i]);
		}
		SendRpcResponse(read.Connection, bridgeWrite);
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.PermissionsEdit)]
	private static void RPC_TogglePermission(BridgeRead read)
	{
		string name = ((NetRead)read).String(256, false);
		string text = ((NetRead)read).String(256, false);
		string plugin = ((NetRead)read).String(256, false);
		BaseHookable hookable = ModLoader.FindPlugin(plugin) ?? Community.Runtime.ModuleProcessor.Modules.FirstOrDefault((BaseHookable x) => x.Name.Equals(plugin));
		if (!(text == "grantall"))
		{
			if (text == "revokeall")
			{
				string[] permissions = Community.Runtime.Core.permission.GetPermissions(hookable);
				foreach (string perm in permissions)
				{
					Community.Runtime.Core.permission.RevokeGroupPermission(name, perm);
				}
			}
			else if (Community.Runtime.Core.permission.GroupHasPermission(name, text))
			{
				Community.Runtime.Core.permission.RevokeGroupPermission(name, text);
			}
			else
			{
				Community.Runtime.Core.permission.GrantGroupPermission(name, text, null);
			}
		}
		else
		{
			string[] permissions2 = Community.Runtime.Core.permission.GetPermissions(hookable);
			foreach (string perm2 in permissions2)
			{
				Community.Runtime.Core.permission.GrantGroupPermission(name, perm2, null);
			}
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.PermissionsEdit)]
	private static void RPC_GetUserMetadata(BridgeRead read)
	{
		string id = ((NetRead)read).String(256, false);
		Permission permission = Community.Runtime.Core.permission;
		KeyValuePair<string, UserData> keyValuePair = permission.FindUser(id);
		if (keyValuePair.Value != null)
		{
			BridgeWrite write = StartRpcResponse();
			UserInfo.Get(keyValuePair.Key, keyValuePair.Value, permissionsOnly: true).Serialize(write);
			SendRpcResponse(read.Connection, write);
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.PlayersView)]
	private static void RPC_SearchUsers(BridgeRead read)
	{
		string text = ((NetRead)read).String(256, false);
		Permission permission = Community.Runtime.Core.permission;
		PooledList<UserInfo> val = Pool.Get<PooledList<UserInfo>>();
		try
		{
			foreach (KeyValuePair<string, UserData> userdatum in permission.userdata)
			{
				if (text == userdatum.Key || text == userdatum.Value.LastSeenNickname || userdatum.Value.LastSeenNickname.Contains(text, StringComparison.CurrentCultureIgnoreCase))
				{
					((List<UserInfo>)(object)val).Add(UserInfo.Get(userdatum.Key, userdatum.Value));
				}
			}
			BridgeWrite bridgeWrite = StartRpcResponse();
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, ((List<UserInfo>)(object)val).Count);
			for (int i = 0; i < ((List<UserInfo>)(object)val).Count; i++)
			{
				((List<UserInfo>)(object)val)[i].Serialize(bridgeWrite);
			}
			SendRpcResponse(read.Connection, bridgeWrite);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.PermissionsEdit)]
	private static void RPC_ToggleUserGroup(BridgeRead read)
	{
		string id = ((NetRead)read).String(256, false);
		string item = ((NetRead)read).String(256, false);
		Permission permission = Community.Runtime.Core.permission;
		KeyValuePair<string, UserData> keyValuePair = permission.FindUser(id);
		if (keyValuePair.Value != null)
		{
			if (keyValuePair.Value.Groups.Contains(item))
			{
				keyValuePair.Value.Groups.Remove(item);
			}
			else
			{
				keyValuePair.Value.Groups.Add(item);
			}
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.PermissionsEdit)]
	private static void RPC_ToggleUserPermission(BridgeRead read)
	{
		string id = ((NetRead)read).String(256, false);
		string item = ((NetRead)read).String(256, false);
		Permission permission = Community.Runtime.Core.permission;
		KeyValuePair<string, UserData> keyValuePair = permission.FindUser(id);
		if (keyValuePair.Value != null)
		{
			if (keyValuePair.Value.Perms.Contains(item))
			{
				keyValuePair.Value.Perms.Remove(item);
			}
			else
			{
				keyValuePair.Value.Perms.Add(item);
			}
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.PlayersView)]
	private static void RPC_Players(BridgeRead read)
	{
		bool flag = Account.HasPermission(read.Connection, PermissionTypes.PlayersIp);
		BridgeWrite bridgeWrite = StartRpcResponse();
		NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, BasePlayer.activePlayerList.Count);
		for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
		{
			new PlayerInfo(BasePlayer.activePlayerList[i]).Serialize(bridgeWrite, !flag);
		}
		PooledList<BasePlayer> val = Pool.Get<PooledList<BasePlayer>>();
		try
		{
			for (int j = 0; j < BasePlayer.sleepingPlayerList.Count; j++)
			{
				BasePlayer val2 = BasePlayer.sleepingPlayerList[j];
				if (!val2.IsConnected)
				{
					((List<BasePlayer>)(object)val).Add(val2);
				}
			}
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, ((List<BasePlayer>)(object)val).Count);
			for (int k = 0; k < ((List<BasePlayer>)(object)val).Count; k++)
			{
				new PlayerInfo(((List<BasePlayer>)(object)val)[k]).Serialize(bridgeWrite, !flag);
			}
			SendRpcResponse(read.Connection, bridgeWrite);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static BridgeWrite CollectPlugins()
	{
		BridgeWrite bridgeWrite = StartRpcResponse(PLUGINS);
		PooledList<RustPlugin> val = Pool.Get<PooledList<RustPlugin>>();
		try
		{
			ModLoader.Packages.GetAllHookables((List<RustPlugin>)(object)val, ignoreCore: true);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, ((List<RustPlugin>)(object)val).Count);
			for (int i = 0; i < ((List<RustPlugin>)(object)val).Count; i++)
			{
				new PluginInfo(((List<RustPlugin>)(object)val)[i]).Serialize(bridgeWrite);
			}
			PooledList<string> val2 = Pool.Get<PooledList<string>>();
			try
			{
				((List<string>)(object)val2).AddRange((IEnumerable<string>)Community.Runtime.ScriptProcessor.IgnoreList);
				((List<string>)(object)val2).AddRange((IEnumerable<string>)Community.Runtime.ZipScriptProcessor.IgnoreList);
				NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, ((List<string>)(object)val2).Count);
				for (int j = 0; j < ((List<string>)(object)val2).Count; j++)
				{
					NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, Path.GetFileNameWithoutExtension(((List<string>)(object)val2)[j]));
				}
				PooledList<ModLoader.CompilationResult> val3 = Pool.Get<PooledList<ModLoader.CompilationResult>>();
				try
				{
					foreach (ModLoader.CompilationResult value in ModLoader.FailedCompilations.Values)
					{
						if (value.HasFailed())
						{
							((List<ModLoader.CompilationResult>)(object)val3).Add(value);
						}
					}
					NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, ((List<ModLoader.CompilationResult>)(object)val3).Count);
					for (int k = 0; k < ((List<ModLoader.CompilationResult>)(object)val3).Count; k++)
					{
						ModLoader.CompilationResult compilationResult = ((List<ModLoader.CompilationResult>)(object)val3)[k];
						NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, Path.GetFileNameWithoutExtension(compilationResult.File));
						NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, compilationResult.Errors.Count);
						for (int l = 0; l < compilationResult.Errors.Count; l++)
						{
							ModLoader.Trace trace = compilationResult.Errors[l];
							NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, trace.Message);
							NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, trace.Number);
							NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, trace.Column);
							NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, trace.Line);
						}
					}
					return bridgeWrite;
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
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

	public static void SendPluginsToAllConnections()
	{
		if (server == null)
		{
			return;
		}
		PooledList<BridgeConnection> val = Pool.Get<PooledList<BridgeConnection>>();
		try
		{
			for (int i = 0; i < server.ConnectionsList.Count; i++)
			{
				BridgeConnection bridgeConnection = server.ConnectionsList[i];
				if (bridgeConnection.Reference is Account account && account.Permissions.plugins_view)
				{
					((List<BridgeConnection>)(object)val).Add(bridgeConnection);
				}
			}
			if (((List<BridgeConnection>)(object)val).Count != 0)
			{
				SendRpcResponse((List<BridgeConnection>)(object)val, CollectPlugins());
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.PluginsView)]
	private static void RPC_Plugins(BridgeRead read)
	{
		SendRpcResponse(read.Connection, CollectPlugins());
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.PluginsEdit)]
	private static void RPC_PluginsUnload(BridgeRead read)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(((NetRead)read).String(256, false));
		RustPlugin rustPlugin = ModLoader.FindPlugin(fileNameWithoutExtension);
		if (rustPlugin != null)
		{
			CorePlugin.ProcessableFilesLookup();
			CorePlugin.ProcessableFile pluginFile = CorePlugin.GetPluginFile(fileNameWithoutExtension);
			if (!string.IsNullOrEmpty(pluginFile.Path))
			{
				pluginFile.GetProcessor().Ignore(pluginFile.Path);
			}
			ModLoader.UninitializePlugin(rustPlugin);
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.PluginsEdit)]
	private static void RPC_PluginsLoad(BridgeRead read)
	{
		string shortName = ((NetRead)read).String(256, false);
		CorePlugin.ProcessableFilesLookup();
		CorePlugin.ProcessableFile pluginFile = CorePlugin.GetPluginFile(shortName);
		if (!string.IsNullOrEmpty(pluginFile.Path))
		{
			IBaseProcessor processor = pluginFile.GetProcessor();
			if (processor != null)
			{
				processor.ClearIgnore(pluginFile.Path);
				processor.Prepare(pluginFile.Id, pluginFile.Path);
			}
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.PluginsView)]
	private static void RPC_PluginDetails(BridgeRead read)
	{
		RustPlugin rustPlugin = ModLoader.FindPlugin(((NetRead)read).String(256, false));
		if (rustPlugin != null)
		{
			BridgeWrite write = StartRpcResponse();
			new PluginDetails(rustPlugin).Serialize(write);
			SendRpcResponse(read.Connection, write);
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.ProfilerView)]
	private static void RPC_ProfilesList(BridgeRead read)
	{
		BridgeWrite bridgeWrite = StartRpcResponse();
		IOrderedEnumerable<string> orderedEnumerable = from x in Directory.GetFiles(Defines.GetProfilesFolder(), "*.cprf")
			orderby new FileInfo(x).LastWriteTime descending
			select x;
		NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, orderedEnumerable.Count());
		foreach (string item in orderedEnumerable)
		{
			FileInfo fileInfo = new FileInfo(item);
			bool flag = MonoProfiler.ValidateFile(item, out var protocol, out var duration, out var isCompared);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, item);
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, Path.GetFileNameWithoutExtension(item));
			NetworkWriteEx.WriteObject<long>((NetWrite)(object)bridgeWrite, fileInfo.Length);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, Epoch.FromDateTime(fileInfo.LastWriteTimeUtc));
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)bridgeWrite, flag);
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, protocol);
			NetworkWriteEx.WriteObject<float>((NetWrite)(object)bridgeWrite, (float)duration);
			NetworkWriteEx.WriteObject<bool>((NetWrite)(object)bridgeWrite, isCompared);
		}
		SendRpcResponse(read.Connection, bridgeWrite);
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.ProfilerLoad)]
	private static void RPC_ProfilesLoad(BridgeRead read)
	{
		string fullPath = Path.GetFullPath(((NetRead)read).String(256, false));
		if (File.Exists(fullPath) && !(Path.GetDirectoryName(fullPath) != Defines.GetProfilesFolder()))
		{
			byte[] array = File.ReadAllBytes(fullPath);
			BridgeWrite bridgeWrite = StartRpcResponse();
			NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, Path.GetFileName(fullPath));
			NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, array.Length);
			NetworkWriteEx.WriteObject<byte[]>((NetWrite)(object)bridgeWrite, array);
			SendRpcResponse(read.Connection, bridgeWrite);
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.ProfilerEdit)]
	private static void RPC_ProfilesDelete(BridgeRead read)
	{
		string fullPath = Path.GetFullPath(((NetRead)read).String(256, false));
		if (File.Exists(fullPath) && !(Path.GetDirectoryName(fullPath) != Defines.GetProfilesFolder()))
		{
			File.Delete(fullPath);
		}
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.ProfilerView)]
	private static void RPC_ProfilesState(BridgeRead read)
	{
		BridgeWrite bridgeWrite = StartRpcResponse();
		NetworkWriteEx.WriteObject<bool>((NetWrite)(object)bridgeWrite, MonoProfiler.IsRecording);
		NetworkWriteEx.WriteObject<bool>((NetWrite)(object)bridgeWrite, MonoProfiler.Enabled);
		NetworkWriteEx.WriteObject<bool>((NetWrite)(object)bridgeWrite, MonoProfiler.Crashed);
		NetworkWriteEx.WriteObject<float>((NetWrite)(object)bridgeWrite, (float)MonoProfiler.CurrentDurationTime.TotalSeconds);
		SendRpcResponse(read.Connection, bridgeWrite);
	}

	[WebCall]
	[WebCall.Condition.Permission(PermissionTypes.ProfilerEdit)]
	private static void RPC_ProfilesToggle(BridgeRead read)
	{
		bool wantsCancel = ((NetRead)read).Bool();
		MonoProfiler.ProfilerArgs flags = MonoProfiler.ProfilerArgs.None;
		bool flag = ((NetRead)read).Bool();
		bool flag2 = ((NetRead)read).Bool();
		bool flag3 = ((NetRead)read).Bool();
		bool flag4 = ((NetRead)read).Bool();
		bool flag5 = ((NetRead)read).Bool();
		bool flag6 = ((NetRead)read).Bool();
		if (flag)
		{
			flags |= MonoProfiler.ProfilerArgs.CallMemory;
		}
		if (flag2)
		{
			flags |= MonoProfiler.ProfilerArgs.AdvancedMemory;
		}
		if (flag3)
		{
			flags |= MonoProfiler.ProfilerArgs.Timings;
		}
		if (flag4)
		{
			flags |= MonoProfiler.ProfilerArgs.Calls;
		}
		if (flag5)
		{
			flags |= MonoProfiler.ProfilerArgs.GCEvents;
		}
		if (flag6)
		{
			flags |= MonoProfiler.ProfilerArgs.StackWalkAllocations;
		}
		if (flags == MonoProfiler.ProfilerArgs.None)
		{
			flags = MonoProfiler.ProfilerArgs.CallMemory | MonoProfiler.ProfilerArgs.AdvancedMemory | MonoProfiler.ProfilerArgs.Timings | MonoProfiler.ProfilerArgs.Calls | MonoProfiler.ProfilerArgs.GCEvents;
		}
		Community.Runtime.Core.NextFrame(delegate
		{
			if (MonoProfiler.IsRecording)
			{
				if (wantsCancel)
				{
					MonoProfiler.ToggleProfiling(MonoProfiler.ProfilerArgs.Abort, logging: false);
				}
				else
				{
					MonoProfiler.Sample sample = MonoProfiler.Sample.Create();
					MonoProfiler.ToggleProfiling(MonoProfiler.ProfilerArgs.CallMemory | MonoProfiler.ProfilerArgs.AdvancedMemory | MonoProfiler.ProfilerArgs.Timings | MonoProfiler.ProfilerArgs.Calls | MonoProfiler.ProfilerArgs.GCEvents, logging: false);
					sample.Resample();
					DateTime now = DateTime.Now;
					string path = Path.Combine(Defines.GetProfilesFolder(), string.Format("profile-{0}_{1}_{2}_{3}{4}{5}.{6}", new object[7] { now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, "cprf" }));
					File.WriteAllBytes(path, sample.ToProto());
					MonoProfiler.Clear();
				}
			}
			else
			{
				MonoProfiler.ToggleProfiling(flags, logging: false);
			}
		});
	}

	[WebCall]
	private static void RPC_ServerInfo(BridgeRead read)
	{
		BridgeWrite bridgeWrite = StartRpcResponse();
		ServerInfoOutput.Get().Serialize(bridgeWrite);
		SendRpcResponse(read.Connection, bridgeWrite);
	}

	[WebCall]
	private static void RPC_CarbonInfo(BridgeRead read)
	{
		IAnalyticsManager analytics = Community.Runtime.Analytics;
		RpcResponse(read, "Carbon" + string.Format(" {0}/{1}/{2} [{3}] [{4}] on Rust {5}/{6} ({7}) {8}", new object[9]
		{
			analytics.Version,
			analytics.Platform,
			analytics.Protocol,
			Build.Git.Branch,
			Build.Git.Tag,
			BuildInfo.Current.Build.Number,
			Protocol.printable,
			BuildInfo.Current.BuildDate,
			BuildInfo.Current.Scm.ChangeId
		}));
	}

	[WebCall]
	private static void RPC_ServerDescription(BridgeRead read)
	{
		RpcResponse(read, Server.description);
	}

	[WebCall]
	private static void RPC_ServerHeaderImage(BridgeRead read)
	{
		RpcResponse(read, Server.headerimage);
	}

	public static BridgeWrite StartRpcResponse()
	{
		BridgeWrite bridgeWrite = BridgeWrite.Rent();
		bridgeWrite.BridgeMessage(BridgeMessages.Channels.Rpc);
		NetworkWriteEx.WriteObject<uint>((NetWrite)(object)bridgeWrite, currentRpcId);
		return bridgeWrite;
	}

	public static BridgeWrite StartRpcResponse(string rpc)
	{
		BridgeWrite bridgeWrite = BridgeWrite.Rent();
		bridgeWrite.BridgeMessage(BridgeMessages.Channels.Rpc);
		NetworkWriteEx.WriteObject<uint>((NetWrite)(object)bridgeWrite, Vault.Pool.Get(rpc));
		return bridgeWrite;
	}

	public static BridgeWrite StartRpcResponse(uint rpc)
	{
		BridgeWrite bridgeWrite = BridgeWrite.Rent();
		bridgeWrite.BridgeMessage(BridgeMessages.Channels.Rpc);
		NetworkWriteEx.WriteObject<uint>((NetWrite)(object)bridgeWrite, rpc);
		return bridgeWrite;
	}

	public static void SendRpcResponse(BridgeConnection connection, BridgeWrite write)
	{
		connection.Send(write);
		BridgeWrite.Return(ref write);
	}

	public static void SendRpcResponse(List<BridgeConnection> connections, BridgeWrite write)
	{
		for (int i = 0; i < connections.Count; i++)
		{
			connections[i].Send(write);
		}
		BridgeWrite.Return(ref write);
	}

	public static void RpcResponse(BridgeRead read)
	{
		BridgeWrite write = StartRpcResponse();
		SendRpcResponse(read.Connection, write);
	}

	public static void RpcResponse<T1>(BridgeRead read, T1 arg1)
	{
		BridgeWrite bridgeWrite = StartRpcResponse();
		NetworkWriteEx.WriteObject<T1>((NetWrite)(object)bridgeWrite, arg1);
		SendRpcResponse(read.Connection, bridgeWrite);
	}

	public static void RpcResponse<T1, T2>(BridgeRead read, T1 arg1, T2 arg2)
	{
		BridgeWrite bridgeWrite = StartRpcResponse();
		NetworkWriteEx.WriteObject<T1>((NetWrite)(object)bridgeWrite, arg1);
		NetworkWriteEx.WriteObject<T2>((NetWrite)(object)bridgeWrite, arg2);
		SendRpcResponse(read.Connection, bridgeWrite);
	}

	public static void RpcResponse<T1, T2, T3>(BridgeRead read, T1 arg1, T2 arg2, T3 arg3)
	{
		BridgeWrite bridgeWrite = StartRpcResponse();
		NetworkWriteEx.WriteObject<T1>((NetWrite)(object)bridgeWrite, arg1);
		NetworkWriteEx.WriteObject<T2>((NetWrite)(object)bridgeWrite, arg2);
		NetworkWriteEx.WriteObject<T3>((NetWrite)(object)bridgeWrite, arg3);
		SendRpcResponse(read.Connection, bridgeWrite);
	}

	public static void RpcResponse<T1, T2, T3, T4>(BridgeRead read, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		BridgeWrite bridgeWrite = StartRpcResponse();
		NetworkWriteEx.WriteObject<T1>((NetWrite)(object)bridgeWrite, arg1);
		NetworkWriteEx.WriteObject<T2>((NetWrite)(object)bridgeWrite, arg2);
		NetworkWriteEx.WriteObject<T3>((NetWrite)(object)bridgeWrite, arg3);
		NetworkWriteEx.WriteObject<T4>((NetWrite)(object)bridgeWrite, arg4);
		SendRpcResponse(read.Connection, bridgeWrite);
	}

	public static void RpcResponse<T1, T2, T3, T4, T5>(BridgeRead read, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		BridgeWrite bridgeWrite = StartRpcResponse();
		NetworkWriteEx.WriteObject<T1>((NetWrite)(object)bridgeWrite, arg1);
		NetworkWriteEx.WriteObject<T2>((NetWrite)(object)bridgeWrite, arg2);
		NetworkWriteEx.WriteObject<T3>((NetWrite)(object)bridgeWrite, arg3);
		NetworkWriteEx.WriteObject<T4>((NetWrite)(object)bridgeWrite, arg4);
		NetworkWriteEx.WriteObject<T5>((NetWrite)(object)bridgeWrite, arg5);
		SendRpcResponse(read.Connection, bridgeWrite);
	}

	public static void Reply(this BridgeConnection connection, string message)
	{
		BridgeWrite bridgeWrite = StartRpcResponse(CONSOLE_LOG);
		NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, message);
		NetworkWriteEx.WriteObject<string>((NetWrite)(object)bridgeWrite, Output.LogTypeToString.Get((LogType)3));
		NetworkWriteEx.WriteObject<int>((NetWrite)(object)bridgeWrite, Epoch.Current);
		SendRpcResponse(connection, bridgeWrite);
	}
}

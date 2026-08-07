using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using Rust.Ai.Gen2;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ConVar;

[Factory("copypaste")]
public class CopyPaste : ConsoleSystem
{
	private class EntityWrapper
	{
		public BaseEntity Entity;

		public Entity Protobuf;

		public Vector3 Position;

		public Quaternion Rotation;

		public bool HasParent;
	}

	public class PasteOptions
	{
		public const string Argument_NPCs = "--npcs";

		public const string Argument_Resources = "--resources";

		public const string Argument_Vehicles = "--vehicles";

		public const string Argument_Deployables = "--deployables";

		public const string Argument_FoundationsOnly = "--foundations-only";

		public const string Argument_BuildingBlocksOnly = "--building-only";

		public const string Argument_SnapToTerrain = "--autosnap-terrain";

		public const string Argument_SnapToZeroHeight = "--autosnap-zero";

		public const string Argument_PastePlayers = "--players";

		public const string Argument_AutoAuth = "--auto-auth";

		public bool Resources;

		public bool NPCs;

		public bool Vehicles;

		public bool Deployables;

		public bool FoundationsOnly;

		public bool BuildingBlocksOnly;

		public bool SnapToTerrain;

		public bool SnapToZero;

		public bool Players;

		public bool AutoAuth;

		public Vector3 Origin;

		public Quaternion PlayerRotation;

		public Vector3 HeightOffset;

		public PasteOptions(Arg arg)
		{
			Resources = arg.HasArg("--resources", remove: true);
			NPCs = arg.HasArg("--npcs", remove: true);
			Vehicles = arg.HasArg("--vehicles", remove: true);
			Deployables = arg.HasArg("--deployables", remove: true);
			FoundationsOnly = arg.HasArg("--foundations-only", remove: true);
			BuildingBlocksOnly = arg.HasArg("--building-only", remove: true);
			SnapToTerrain = arg.HasArg("--autosnap-terrain", remove: true);
			SnapToZero = arg.HasArg("--autosnap-zero", remove: true);
			Players = arg.HasArg("--players", remove: true);
			AutoAuth = arg.HasArg("--auto-auth", remove: true);
		}

		public PasteOptions(PasteRequest request)
		{
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			Resources = request.resources;
			NPCs = request.npcs;
			Vehicles = request.vehicles;
			Deployables = request.deployables;
			FoundationsOnly = request.foundationsOnly;
			BuildingBlocksOnly = request.buildingBlocksOnly;
			SnapToTerrain = request.snapToTerrain;
			SnapToZero = request.snapToZero;
			Players = request.players;
			AutoAuth = request.autoAuth;
			Origin = request.origin;
			PlayerRotation = Quaternion.Euler(request.playerRotation);
			HeightOffset = request.heightOffset;
		}

		public PasteOptions()
		{
		}
	}

	private const string ClipboardFileName = "clipboard";

	private const string OverwriteFlag = "--overwrite";

	public static CopyPasteHistoryManager playerHistory = new CopyPasteHistoryManager();

	private static void PrintPasteNames(StringBuilder builder, string directory)
	{
		if (!Directory.Exists(directory))
		{
			builder.AppendLine("No pastes found");
			return;
		}
		string[] files = Directory.GetFiles(directory, "*.data");
		builder.AppendLine($"Found {files.Length} pastes");
		foreach (string item in files.OrderBy((string x) => x))
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item);
			builder.AppendLine(fileNameWithoutExtension);
		}
	}

	private static void CopyEntities(BasePlayer player, List<BaseEntity> entities, string name, Vector3 originPos, Quaternion originRot)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		CopyPasteEntityInfo val = BuildCopyPaste(entities, originPos, originRot);
		try
		{
			CopyPasteEntity.ServerInstance?.ClientRPC(RpcTarget.Player("CLIENT_ReceivePaste", player), name, val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static byte[] BuildCopyPasteBytes(List<BaseEntity> entities, Vector3 originPos, Quaternion originRot, List<BaseEntity> savedOrdered)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		CopyPasteEntityInfo val = BuildCopyPaste(entities, originPos, originRot, savedOrdered);
		try
		{
			return ProtoStreamExtensions.ToProtoBytes((IProto)(object)val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static CopyPasteEntityInfo BuildCopyPaste(List<BaseEntity> entities, Vector3 originPos, Quaternion originRot, List<BaseEntity> savedOrdered = null)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		OrderEntitiesForSave(entities);
		CopyPasteEntityInfo val = Pool.Get<CopyPasteEntityInfo>();
		val.entities = Pool.Get<List<Entity>>();
		Transform transform = new GameObject("Align").transform;
		transform.position = originPos;
		transform.rotation = originRot;
		foreach (BaseEntity entity in entities)
		{
			if (!entity.isClient && entity.enableSaving)
			{
				BaseEntity baseEntity = entity.parentEntity.Get(serverside: true);
				if ((Object)(object)baseEntity != (Object)null && (!entities.Contains(baseEntity) || !baseEntity.enableSaving))
				{
					Debug.LogWarning((object)("Skipping " + entity.ShortPrefabName + " as it is parented to an entity not included in the copy (it would become orphaned)"));
					continue;
				}
				SaveEntity(entity, val, baseEntity, transform);
				savedOrdered?.Add(entity);
			}
		}
		val.entityCount = val.entities.Count;
		Object.Destroy((Object)(object)((Component)transform).gameObject);
		return val;
	}

	private static List<EntityWrapper> PrepareEntityProtos(CopyPasteEntityInfo toLoad, PasteOptions options, bool assignNewUids)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		toLoad = toLoad.Copy();
		HashSet<NetworkableId> hashSet = new HashSet<NetworkableId>();
		for (int i = 0; i < toLoad.entities.Count; i++)
		{
			Entity val = toLoad.entities[i];
			if (!hashSet.Add(val.baseNetworkable.uid))
			{
				GameObject obj = GameManager.server.FindPrefab(val.baseNetworkable.prefabID);
				BaseEntity baseEntity = ((obj != null) ? obj.GetComponent<BaseEntity>() : null);
				Debug.LogWarning((object)string.Format("Skipping entity [{0}/{1}]: duplicate entity in paste, please re-save", val.baseNetworkable.uid, ((Object)(object)baseEntity == (Object)null) ? "unknown" : baseEntity.ShortPrefabName));
				toLoad.entities.RemoveAt(i);
				i--;
			}
		}
		Transform transform = new GameObject("Align").transform;
		transform.position = options.Origin;
		transform.rotation = options.PlayerRotation;
		List<EntityWrapper> list = new List<EntityWrapper>();
		Dictionary<ulong, ulong> remapping = new Dictionary<ulong, ulong>();
		Dictionary<uint, uint> dictionary = new Dictionary<uint, uint>();
		if (assignNewUids)
		{
			remapping = new Dictionary<ulong, ulong>();
		}
		foreach (Entity entity in toLoad.entities)
		{
			if (assignNewUids)
			{
				entity.InspectUids((UidInspector<ulong>)UpdateWithNewUid);
			}
			EntityWrapper item = new EntityWrapper
			{
				Protobuf = entity,
				HasParent = (entity.parent != null && entity.parent.uid != default(NetworkableId))
			};
			list.Add(item);
			if (entity.decayEntity != null)
			{
				if (!dictionary.TryGetValue(entity.decayEntity.buildingID, out var value))
				{
					value = BuildingManager.server.NewBuildingID();
					dictionary.Add(entity.decayEntity.buildingID, value);
				}
				entity.decayEntity.buildingID = value;
			}
		}
		foreach (EntityWrapper item2 in list)
		{
			item2.Position = item2.Protobuf.baseEntity.pos;
			item2.Rotation = Quaternion.Euler(item2.Protobuf.baseEntity.rot);
			if (!item2.HasParent)
			{
				item2.Protobuf.baseEntity.pos = transform.TransformPoint(item2.Protobuf.baseEntity.pos);
				BaseEntity baseEntity2 = item2.Protobuf.baseEntity;
				Quaternion val2 = transform.rotation * Quaternion.Euler(item2.Protobuf.baseEntity.rot);
				baseEntity2.rot = ((Quaternion)(ref val2)).eulerAngles;
			}
		}
		if (Application.isPlaying)
		{
			Object.Destroy((Object)(object)((Component)transform).gameObject);
		}
		else
		{
			Object.DestroyImmediate((Object)(object)((Component)transform).gameObject);
		}
		return list;
		void UpdateWithNewUid(UidType type, ref ulong prevUid)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Invalid comparison between Unknown and I4
			if ((int)type == 3)
			{
				prevUid = 0uL;
			}
			else if (prevUid != 0L && remapping != null)
			{
				if (!remapping.TryGetValue(prevUid, out var value2))
				{
					value2 = Net.sv.TakeUID();
					remapping.Add(prevUid, value2);
				}
				prevUid = value2;
			}
		}
	}

	public static float ComputeAutoSnapOffsetY(IList<BaseEntity> entities, PasteOptions options, Vector3 pasteOrigin)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		if (!options.SnapToTerrain && !options.SnapToZero)
		{
			return 0f;
		}
		float num = float.MaxValue;
		float num2 = float.MinValue;
		foreach (BaseEntity entity in entities)
		{
			if ((!((Object)(object)entity.parentEntity.Get(serverside: true) == (Object)null) || !(entity.ShortPrefabName == "foundation")) && !(entity.ShortPrefabName == "foundation.triangle") && !(entity is PlayerBoat))
			{
				continue;
			}
			Vector3 position = ((Component)entity).transform.position;
			float num3 = position.y - pasteOrigin.y;
			float num4;
			if (options.SnapToZero)
			{
				num4 = 0f;
			}
			else
			{
				num4 = ((!Application.isPlaying) ? 0f : TerrainMeta.HeightMap.GetHeight(position));
				if (GamePhysics.Trace(new Ray(new Vector3(position.x, num4, position.z) + new Vector3(0f, 100f, 0f), Vector3.down), 0f, out var hitInfo, 100f, 8454160, (QueryTriggerInteraction)0))
				{
					num4 = ((RaycastHit)(ref hitInfo)).point.y;
				}
			}
			if (num3 > num4)
			{
				num = Mathf.Min(num, num3 - num4);
			}
			if (num4 > num3)
			{
				num2 = Mathf.Max(num2, num4 - num3);
			}
		}
		if (num == float.MaxValue && num2 == float.MinValue)
		{
			return 0f;
		}
		if (!(num < num2) && num2 != float.MinValue)
		{
			return num2;
		}
		return 0f - num;
	}

	private static void ApplyAutoSnap(List<BaseEntity> entities, PasteOptions options)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(0f, ComputeAutoSnapOffsetY(entities, options, Vector3.zero), 0f);
		val += options.HeightOffset;
		if (!(val != Vector3.zero))
		{
			return;
		}
		foreach (BaseEntity entity in entities)
		{
			if ((Object)(object)entity.parentEntity.Get(serverside: true) == (Object)null)
			{
				Transform transform = ((Component)entity).transform;
				transform.position += val;
			}
			if (!(entity is IOEntity iOEntity))
			{
				continue;
			}
			if (iOEntity.inputs != null)
			{
				IOEntity.IOSlot[] inputs = iOEntity.inputs;
				foreach (IOEntity.IOSlot obj in inputs)
				{
					obj.originPosition += val;
				}
			}
			if (iOEntity.outputs != null)
			{
				IOEntity.IOSlot[] inputs = iOEntity.outputs;
				foreach (IOEntity.IOSlot obj2 in inputs)
				{
					obj2.originPosition += val;
				}
			}
		}
	}

	public static List<BaseEntity> PasteEntitiesInternal(CopyPasteEntityInfo toLoad, PasteOptions options, ulong admin)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		List<EntityWrapper> list = PrepareEntityProtos(toLoad, options, assignNewUids: true);
		List<BaseEntity> list2 = new List<BaseEntity>();
		foreach (EntityWrapper item in list)
		{
			if (CanPrefabBePasted(item.Protobuf.baseNetworkable.prefabID, options))
			{
				item.Entity = GameManager.server.CreateEntity(StringPool.Get(item.Protobuf.baseNetworkable.prefabID), item.Protobuf.baseEntity.pos, Quaternion.Euler(item.Protobuf.baseEntity.rot));
				if (item.Protobuf.basePlayer != null && item.Protobuf.basePlayer.userid > 10000000)
				{
					ulong userid = 10000000uL + (ulong)Random.Range(1, int.MaxValue);
					item.Protobuf.basePlayer.userid = userid;
				}
				item.Entity.InitLoad(item.Protobuf.baseNetworkable.uid);
				item.Entity.PreServerLoad();
				list2.Add(item.Entity);
			}
		}
		list.RemoveAll((EntityWrapper x) => (Object)(object)x.Entity == (Object)null);
		for (int num = 0; num < list.Count; num++)
		{
			EntityWrapper entityWrapper = list[num];
			BaseNetworkable.LoadInfo info = new BaseNetworkable.LoadInfo
			{
				fromDisk = true,
				fromCopy = true,
				msg = entityWrapper.Protobuf
			};
			try
			{
				entityWrapper.Entity.Spawn();
				bool flag = false;
				if (!flag && entityWrapper.Protobuf.parent != null && entityWrapper.Protobuf.parent.uid != default(NetworkableId))
				{
					BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(entityWrapper.Protobuf.parent.uid) as BaseEntity;
					if ((Object)(object)baseEntity == (Object)null || baseEntity.net == null)
					{
						flag = true;
					}
				}
				if (flag)
				{
					entityWrapper.Entity.Kill();
					list.RemoveAt(num);
					num--;
				}
				else
				{
					entityWrapper.Entity.Load(info);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("Failed to spawn entity '" + entityWrapper.Entity?.PrefabName + "' while pasting"));
				Debug.LogException(ex);
				try
				{
					entityWrapper.Entity.Kill();
				}
				catch
				{
				}
			}
		}
		ApplyAutoSnap(list2, options);
		foreach (EntityWrapper item2 in list)
		{
			item2.Entity.PostServerLoad();
			item2.Entity.UpdateNetworkGroup();
		}
		foreach (EntityWrapper item3 in list)
		{
			item3.Entity.RefreshEntityLinks();
		}
		foreach (EntityWrapper item4 in list)
		{
			if (item4.Entity is BuildingBlock buildingBlock)
			{
				buildingBlock.UpdateSkin(force: true);
			}
		}
		foreach (EntityWrapper item5 in list)
		{
			if (item5.Entity is BaseMountable baseMountable)
			{
				baseMountable.UpdateMountFlags();
			}
			if (options.AutoAuth)
			{
				Admin.SetUserAuthorized(item5.Entity, admin, state: true);
			}
		}
		return (from x in list
			select x.Entity into x
			where (Object)(object)x != (Object)null
			select x).ToList();
	}

	public static CopyPasteEntityInfo LoadFileFromBundles(string fullPath)
	{
		CopyPasteDataAsset copyPasteDataAsset = FileSystem.Load<CopyPasteDataAsset>(fullPath, true);
		if (copyPasteDataAsset == null)
		{
			Debug.LogWarning((object)("Missing file: " + fullPath));
			return null;
		}
		return LoadFromAsset(copyPasteDataAsset);
	}

	public static CopyPasteEntityInfo LoadFromAsset(CopyPasteDataAsset copyPasteAsset)
	{
		if (copyPasteAsset == null)
		{
			return null;
		}
		byte[] data = copyPasteAsset.GetData();
		if (data == null || data.Length == 0)
		{
			return null;
		}
		return CopyPasteEntityInfo.Deserialize(data);
	}

	private static void SaveEntity(BaseEntity baseEntity, CopyPasteEntityInfo toSave, BaseEntity parent, Transform alignObject)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		BaseNetworkable.SaveInfo info = new BaseNetworkable.SaveInfo
		{
			forDisk = true,
			msg = Pool.Get<Entity>(),
			cachedTime = BaseNetworkable.ThreadSafeTime.TakeSnapshot()
		};
		baseEntity.Save(info);
		if ((Object)(object)parent == (Object)null)
		{
			info.msg.baseEntity.pos = alignObject.InverseTransformPoint(info.msg.baseEntity.pos);
			_ = alignObject.rotation * ((Component)baseEntity).transform.rotation;
			BaseEntity baseEntity2 = info.msg.baseEntity;
			Quaternion val = Quaternion.Inverse(((Component)alignObject).transform.rotation) * ((Component)baseEntity).transform.rotation;
			baseEntity2.rot = ((Quaternion)(ref val)).eulerAngles;
		}
		toSave.entities.Add(info.msg);
	}

	private static void GetEntitiesLookingAt(Vector3 originPoint, Vector3 direction, List<BaseEntity> entityList)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		entityList.Clear();
		BuildingBlock buildingBlock = GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, new Ray(originPoint, direction), 0f, 100f, 136315136, (QueryTriggerInteraction)0) as BuildingBlock;
		if ((Object)(object)buildingBlock == (Object)null)
		{
			return;
		}
		ListHashSet<DecayEntity> val = buildingBlock.GetBuilding()?.decayEntities;
		if (val != null)
		{
			BaseEntity rootParentEntity = buildingBlock.GetRootParentEntity();
			if (rootParentEntity is PlayerBoat)
			{
				entityList.Add(rootParentEntity);
			}
			else
			{
				entityList.AddRange((IEnumerable<BaseEntity>)val);
			}
		}
	}

	private static void GetEntitiesInRadius(Vector3 originPoint, float radius, List<BaseEntity> entityList)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (radius <= 0f)
		{
			return;
		}
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		global::Vis.Entities(originPoint, radius, list, -1, (QueryTriggerInteraction)2);
		foreach (BaseEntity item in list)
		{
			if (!item.isClient)
			{
				entityList.Add(item);
			}
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
	}

	public static void GetEntitiesInBounds(Bounds bounds, List<BaseEntity> entityList)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		OBB bounds2 = new OBB(bounds);
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		global::Vis.Entities(bounds2, list, -1, (QueryTriggerInteraction)2);
		foreach (BaseEntity item in list)
		{
			if (!item.isClient)
			{
				entityList.Add(item);
			}
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
	}

	public static bool CanPrefabBePasted(uint prefabId, PasteOptions options)
	{
		GameObject val = GameManager.server.FindPrefab(prefabId);
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		BaseEntity component = val.GetComponent<BaseEntity>();
		if ((Object)(object)component == (Object)null)
		{
			return false;
		}
		if (options.FoundationsOnly && component.ShortPrefabName != "foundation" && component.ShortPrefabName != "foundation.triangle")
		{
			return false;
		}
		if (options.BuildingBlocksOnly && !(component is BuildingBlock))
		{
			return false;
		}
		if (component is DecayEntity && !(component is BuildingBlock) && !options.Deployables)
		{
			return false;
		}
		if (component is BasePlayer { IsNpc: false } && !options.Players)
		{
			return false;
		}
		if (component is PointEntity || component is RelationshipManager)
		{
			return false;
		}
		if ((component is ResourceEntity || component is BushEntity) && !options.Resources)
		{
			return false;
		}
		if ((component is BaseNpc || component is RidableHorse) && !options.NPCs)
		{
			return false;
		}
		if (component is BaseVehicle && !(component is RidableHorse) && !options.Vehicles)
		{
			return false;
		}
		return true;
	}

	private static void OrderEntitiesForSave(List<BaseEntity> entities)
	{
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		list.AddRange(entities);
		entities.Clear();
		HashSet<BaseEntity> hash = new HashSet<BaseEntity>();
		foreach (BaseEntity item in list.OrderBy((BaseEntity x) => x.net.ID.Value))
		{
			AddRecursive(item);
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
		void AddRecursive(BaseEntity current)
		{
			if (hash.Add(current))
			{
				entities.Add(current);
				if (current.children != null)
				{
					foreach (BaseEntity child in current.children)
					{
						AddRecursive(child);
					}
				}
			}
		}
	}

	[ServerVar(Name = "copybox_sv", Help = "(Generated) Server-side handler that copies all entities within the specified bounding box (center + size) into a named paste file; called from copybox client command")]
	public static void copybox_sv(Arg args)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (!args.HasArgs(3))
		{
			args.ReplyWith("Missing args: copybox_sv <name> <center> <size> <rotation>");
			return;
		}
		string name = args.GetString(0);
		Vector3 vector = args.GetVector3(1);
		Vector3 vector2 = args.GetVector3(2);
		Quaternion originRot = Quaternion.Euler(args.GetVector3(3));
		Bounds bounds = new Bounds(vector, vector2);
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		GetEntitiesInBounds(bounds, list);
		CopyEntities(ArgEx.Player(args), list, name, vector, originRot);
		Pool.FreeUnmanaged<BaseEntity>(ref list);
	}

	public static void copyboat_sv(Arg args)
	{
	}

	public static List<BaseEntity> PasteEntities(CopyPasteEntityInfo data, PasteOptions options, ulong steamId)
	{
		List<BaseEntity> list;
		try
		{
			Application.isLoadingSave = true;
			Application.isLoading = true;
			list = PasteEntitiesInternal(data, options, steamId);
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			return new List<BaseEntity>();
		}
		finally
		{
			Application.isLoadingSave = false;
			Application.isLoading = false;
		}
		foreach (BaseEntity item in list)
		{
			if (!((Object)(object)item == (Object)null) && item is StabilityEntity stabilityEntity)
			{
				stabilityEntity.UpdateStability();
			}
		}
		return list;
	}

	[ServerVar(Help = "(Generated) Undoes the most recent paste operation for the calling player by destroying all entities that were spawned in that paste; replies with 'History empty' if nothing to undo")]
	public static void undopaste_sv(Arg args)
	{
		ulong steamId = ArgEx.Player(args)?.userID ?? ((EncryptedValue<ulong>)0uL);
		PasteResult pasteResult = playerHistory.Undo(steamId);
		if (pasteResult == null)
		{
			args.ReplyWith("History empty");
			return;
		}
		foreach (BaseEntity entity in pasteResult.Entities)
		{
			entity.Kill();
		}
	}

	[ServerVar(Help = "(Generated) Server-side handler that copies all entities within the specified radius around a position into a named paste file; called from the copyradius client command")]
	public static void copyradius_sv(Arg args)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		string name = args.GetString(0);
		Vector3 vector = args.GetVector3(1);
		float num = args.GetFloat(2);
		Quaternion originRot = Quaternion.Euler(args.GetVector3(3));
		if (num <= 0f)
		{
			args.ReplyWith("Invalid radius: must be greater than zero");
			return;
		}
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		GetEntitiesInRadius(vector, num, list);
		CopyEntities(ArgEx.Player(args), list, name, vector, originRot);
		Pool.FreeUnmanaged<BaseEntity>(ref list);
	}

	[ServerVar(Help = "(Generated) Server-side handler that copies all entities belonging to the building the player is looking at into a named paste file; called from the copybuilding client command")]
	public static void copybuilding_sv(Arg args)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		string name = args.GetString(0);
		Vector3 vector = args.GetVector3(1);
		Vector3 vector2 = args.GetVector3(2);
		Quaternion originRot = Quaternion.Euler(args.GetVector3(3));
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		GetEntitiesLookingAt(vector, vector2, list);
		CopyEntities(ArgEx.Player(args), list, name, vector, originRot);
		Pool.FreeUnmanaged<BaseEntity>(ref list);
	}

	[ServerVar(Help = "(Generated) Server-side handler that prints the names of all entities within the current selection bounds; used to preview what would be included in a copy operation")]
	public static void printselection_sv(Arg args)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		Vector3 vector = args.GetVector3(0);
		Vector3 vector2 = args.GetVector3(1);
		args.GetVector3(2);
		GetEntitiesInBounds(new Bounds(vector, vector2), list);
		StringBuilder stringBuilder = new StringBuilder();
		if (list.Count == 0)
		{
			stringBuilder.AppendLine("Empty");
		}
		else
		{
			foreach (BaseEntity item in list)
			{
				if (!item.isClient)
				{
					stringBuilder.AppendLine(((Object)item).name);
				}
			}
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
		args.ReplyWith(stringBuilder.ToString());
	}

	private static string GetLegacyServerDirectory()
	{
		return Server.GetServerFolder("copypaste");
	}

	private static string GetLegacyServerPath(string name)
	{
		return GetLegacyServerDirectory() + "/" + name + ".data";
	}

	[ServerVar(Help = "Downloads a paste file stored on the server (legacy server-side storage) by name and sends its entity data to the requesting client for local storage")]
	public static void download_paste_sv(Arg arg)
	{
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Missing args: download_paste_sv <name>");
			return;
		}
		string text = arg.GetString(0);
		string legacyServerPath = GetLegacyServerPath(arg.GetString(0));
		if (!File.Exists(legacyServerPath))
		{
			arg.ReplyWith("Paste '" + text + "' not found");
			return;
		}
		CopyPasteEntityInfo val = CopyPasteEntityInfo.Deserialize(File.ReadAllBytes(legacyServerPath));
		try
		{
			CopyPasteEntity.ServerInstance.ClientRPC(RpcTarget.Player("CLIENT_ReceivePaste", arg.Connection), text, val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "(Generated) Lists all paste files stored in the legacy server-side copypaste directory and prints their names to the console")]
	public static void list_pastes_sv(Arg arg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		PrintPasteNames(stringBuilder, GetLegacyServerDirectory());
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "(Generated) Server-side handler that destroys all entities within the current selection bounds that match the active paste filter options (NPCs, vehicles, deployables etc.)")]
	public static void killbox_sv(Arg args)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 vector = args.GetVector3(0);
		Vector3 vector2 = args.GetVector3(1);
		PasteOptions options = new PasteOptions(args);
		Bounds bounds = new Bounds(vector, vector2);
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		GetEntitiesInBounds(bounds, list);
		foreach (BaseEntity item in list)
		{
			if (!item.isClient && CanPrefabBePasted(item.prefabID, options) && (!(item is BasePlayer entity) || entity.IsNpcPlayer()))
			{
				item.Kill();
			}
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
	}

	public static Quaternion GetPlayerRotation(BasePlayer ply)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ply.eyes.BodyForward();
		val.y = 0f;
		return Quaternion.LookRotation(val, Vector3.up);
	}
}

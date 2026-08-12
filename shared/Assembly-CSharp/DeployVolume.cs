using System;
using System.Collections.Generic;
using Development.Attributes;
using Facepunch;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class DeployVolume : PrefabAttribute
{
	public enum EntityMode
	{
		ExcludeList,
		IncludeList
	}

	public enum TypeFilterMode
	{
		Include,
		Ignore
	}

	public LayerMask layers;

	[InspectorFlags]
	public ColliderInfo.Flags ignore;

	public EntityMode entityMode;

	[FormerlySerializedAs("entities")]
	public BaseEntity[] entityList;

	[SerializeField]
	public EntityListScriptableObject[] entityGroups;

	public bool IsBuildingBlock { get; set; }

	public static Collider LastDeployHit { get; set; }

	protected override Type GetIndexedType()
	{
		return typeof(DeployVolume);
	}

	public override void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.PreProcess(preProcess, rootObj, name, serverside, clientside, bundling);
		IsBuildingBlock = (Object)(object)rootObj.GetComponent<BuildingBlock>() != (Object)null;
	}

	protected abstract bool Check(Vector3 position, Quaternion rotation, int mask = -1);

	protected abstract bool Check(Vector3 position, Quaternion rotation, List<Type> types, TypeFilterMode filterMode, BaseEntity ignoredEntity = null, int mask = -1, bool ignoreChildrenOfEntity = false);

	protected abstract bool Check(Vector3 position, Quaternion rotation, OBB test, int mask = -1);

	public static bool Check(Vector3 position, Quaternion rotation, DeployVolume[] volumes, int mask = -1)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < volumes.Length; i++)
		{
			if (volumes[i].Check(position, rotation, mask))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Check(Vector3 position, Quaternion rotation, DeployVolume[] volumes, List<Type> types, TypeFilterMode filterMode, BaseEntity ignoredEntity = null, int mask = -1, bool ignoreChildrenOfEntity = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < volumes.Length; i++)
		{
			if (volumes[i].Check(position, rotation, types, filterMode, ignoredEntity, mask, ignoreChildrenOfEntity))
			{
				return true;
			}
		}
		return false;
	}

	[PoolAnalyzerNonCaching]
	public static bool Check(Vector3 position, Quaternion rotation, List<DeployVolume> volumes, OBB test, int mask = -1)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < volumes.Count; i++)
		{
			if (volumes[i].Check(position, rotation, test, mask))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CheckSphere(Vector3 pos, float radius, int layerMask, DeployVolume volume)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		List<Collider> list = Pool.Get<List<Collider>>();
		GamePhysics.OverlapSphere(pos, radius, list, layerMask, (QueryTriggerInteraction)2);
		bool result = CheckFlags(list, volume);
		Pool.FreeUnmanaged<Collider>(ref list);
		return result;
	}

	public static bool CheckCapsule(Vector3 start, Vector3 end, float radius, int layerMask, DeployVolume volume)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return CheckCapsule(start, end, radius, layerMask, volume, null, TypeFilterMode.Include);
	}

	[PoolAnalyzerNonCaching]
	public static bool CheckCapsule(Vector3 start, Vector3 end, float radius, int layerMask, DeployVolume volume, List<Type> types, TypeFilterMode filterMode, BaseEntity ignoredEntity = null)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		List<Collider> list = Pool.Get<List<Collider>>();
		GamePhysics.OverlapCapsule(start, end, radius, list, layerMask, (QueryTriggerInteraction)2);
		bool result = CheckFlags(list, volume, types, filterMode, ignoredEntity);
		Pool.FreeUnmanaged<Collider>(ref list);
		return result;
	}

	public static bool CheckOBB(OBB obb, int layerMask, DeployVolume volume)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return CheckOBB(obb, layerMask, volume, null, TypeFilterMode.Include);
	}

	[PoolAnalyzerNonCaching]
	public static bool CheckOBB(OBB obb, int layerMask, DeployVolume volume, List<Type> types, TypeFilterMode filterMode, BaseEntity ignoredEntity = null, bool ignoreChildrenOfEntity = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		List<Collider> list = Pool.Get<List<Collider>>();
		GamePhysics.OverlapOBB(obb, list, layerMask, (QueryTriggerInteraction)2);
		bool result = CheckFlags(list, volume, types, filterMode, ignoredEntity, ignoreChildrenOfEntity);
		Pool.FreeUnmanaged<Collider>(ref list);
		return result;
	}

	public static bool CheckBounds(Bounds bounds, int layerMask, DeployVolume volume)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		List<Collider> list = Pool.Get<List<Collider>>();
		GamePhysics.OverlapBounds(bounds, list, layerMask, (QueryTriggerInteraction)2);
		bool result = CheckFlags(list, volume);
		Pool.FreeUnmanaged<Collider>(ref list);
		return result;
	}

	[PoolAnalyzerNonCaching]
	private static bool CheckFlags(List<Collider> list, DeployVolume volume, List<Type> types = null, TypeFilterMode filterMode = TypeFilterMode.Include, BaseEntity ignoredEntity = null, bool ignoreChildrenOfEntity = false)
	{
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		if (volume == null)
		{
			return true;
		}
		LastDeployHit = null;
		for (int i = 0; i < list.Count; i++)
		{
			LastDeployHit = list[i];
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(LastDeployHit);
			if ((Object)(object)ignoredEntity != (Object)null && (Object)(object)baseEntity != (Object)null && ((Object)(object)baseEntity == (Object)(object)ignoredEntity || (ignoreChildrenOfEntity && (Object)(object)baseEntity.GetRootParentEntity() == (Object)(object)ignoredEntity)))
			{
				continue;
			}
			if ((Object)(object)baseEntity != (Object)null && types != null)
			{
				Type type = ((object)baseEntity).GetType();
				bool flag = types.Contains(type);
				if ((filterMode == TypeFilterMode.Include && !flag) || ((filterMode == TypeFilterMode.Ignore) & flag))
				{
					continue;
				}
			}
			GameObject gameObject = ((Component)list[i]).gameObject;
			if (gameObject.CompareTag("DeployVolumeIgnore"))
			{
				continue;
			}
			ColliderInfo component = gameObject.GetComponent<ColliderInfo>();
			if (((Object)(object)component != (Object)null && component.HasFlag(ColliderInfo.Flags.OnlyBlockBuildingBlock) && !volume.IsBuildingBlock) || ((Object)(object)component != (Object)null && component.HasFlag(ColliderInfo.Flags.OnlyBlockDeployables) && volume.IsBuildingBlock))
			{
				continue;
			}
			if (gameObject.HasCustomTag(GameObjectTag.BlockPlacement))
			{
				return true;
			}
			MonumentInfo monument = ColliderEx.GetMonument(list[i]);
			if ((!((Object)(object)monument != (Object)null) || monument.IsSafeZone || (volume.ignore & ColliderInfo.Flags.Monument) != ColliderInfo.Flags.Monument) && (!((Object)(object)monument == (Object)null) || (LayerMask.op_Implicit(volume.layers) & 0x20000000) == 0 || (volume.ignore & ColliderInfo.Flags.OnlyEvaluatePreventBuildingInMonuments) != ColliderInfo.Flags.OnlyEvaluatePreventBuildingInMonuments) && (!((Object)(object)component != (Object)null) || (volume.ignore & component.flags) == 0))
			{
				if ((Object)(object)component != (Object)null && volume.ignore != 0 && component.HasFlag(volume.ignore))
				{
					return false;
				}
				if (ShouldApplyVolumeForEntity(volume, baseEntity))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool ShouldApplyVolumeForEntity(DeployVolume volume, BaseEntity entity)
	{
		if (volume.entityList == null || volume.entityGroups == null || (volume.entityList.Length == 0 && volume.entityGroups.Length == 0))
		{
			return true;
		}
		if (volume.entityGroups.Length != 0)
		{
			EntityListScriptableObject[] array = volume.entityGroups;
			foreach (EntityListScriptableObject entityListScriptableObject in array)
			{
				if (CollectionEx.IsNullOrEmpty(entityListScriptableObject.entities))
				{
					Debug.LogWarning((object)("Skipping entity group '" + ((Object)entityListScriptableObject).name + "' when checking volume: there are no entities"));
				}
				else if (CheckEntityList(entity, entityListScriptableObject.entities, volume.entityMode == EntityMode.IncludeList))
				{
					return true;
				}
			}
		}
		if (volume.entityList.Length != 0 && CheckEntityList(entity, volume.entityList, volume.entityMode == EntityMode.IncludeList))
		{
			return true;
		}
		return false;
	}

	public static bool CheckEntityList(BaseEntity entity, BaseEntity[] entities, bool trueIfAnyFound)
	{
		if (entities == null || entities.Length == 0)
		{
			return true;
		}
		bool flag = false;
		if ((Object)(object)entity != (Object)null)
		{
			foreach (BaseEntity baseEntity in entities)
			{
				if (entity.prefabID == baseEntity.prefabID)
				{
					flag = true;
					break;
				}
				if (entity is ModularCar && baseEntity is ModularCar)
				{
					flag = true;
					break;
				}
			}
		}
		if (trueIfAnyFound)
		{
			return flag;
		}
		return !flag;
	}

	protected DeployVolume()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		layers = LayerMask.op_Implicit(537001984);
		base._002Ector();
	}
}

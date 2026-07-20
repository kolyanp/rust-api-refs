using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

public class GroundWatch : EntityComponent<BaseEntity>, IServerComponent
{
	public Vector3 groundPosition = Vector3.zero;

	public LayerMask layers = LayerMask.op_Implicit(161546240);

	public float radius = 0.1f;

	public bool needBuildingBlock;

	[Tooltip("By default, we consider a deployable as not grounded when at least one AreaCheck fails. This allows you to consider it grounded as long as one AreaCheck passes.")]
	public bool needOnlyOneAreaCheckValid;

	[Header("Whitelist")]
	public BaseEntity[] whitelist;

	public int fails;

	public BaseCombatEntity cachedGround { get; private set; }

	public override void InitShared()
	{
		base.InitShared();
		CacheGround();
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.matrix = ((Component)this).transform.localToWorldMatrix;
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(groundPosition, radius);
	}

	public static void PhysicsChanged(GameObject obj)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)obj == (Object)null)
		{
			return;
		}
		Collider component = obj.GetComponent<Collider>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		Bounds bounds = component.bounds;
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		Vector3 center = ((Bounds)(ref bounds)).center;
		Vector3 extents = ((Bounds)(ref bounds)).extents;
		Vis.Entities(center, ((Vector3)(ref extents)).magnitude + 1f, list, 136481024, (QueryTriggerInteraction)2);
		foreach (BaseEntity item in list)
		{
			if (!item.IsDestroyed && !item.isClient && !(item is BuildingBlock))
			{
				((Component)item).BroadcastMessage("OnPhysicsNeighbourChanged", (SendMessageOptions)1);
			}
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
	}

	public static void PhysicsChanged(Vector3 origin, float radius, int layerMask)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		Vis.Entities(origin, radius, list, layerMask, (QueryTriggerInteraction)2);
		foreach (BaseEntity item in list)
		{
			if (!item.IsDestroyed && !item.isClient && !(item is BuildingBlock))
			{
				((Component)item).BroadcastMessage("OnPhysicsNeighbourChanged", (SendMessageOptions)1);
			}
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
	}

	public void DirectCallOnPhysicsNeighbourChanged()
	{
		OnPhysicsNeighbourChanged();
	}

	public void OnPhysicsNeighbourChanged()
	{
		bool flag = OnGround();
		if (flag && needBuildingBlock)
		{
			flag = HasBuildingBlock();
		}
		if (!flag)
		{
			fails++;
			if (fails >= Physics.groundwatchfails)
			{
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(((Component)this).gameObject);
				if (Object.op_Implicit((Object)(object)baseEntity))
				{
					((Component)((Component)baseEntity).transform).BroadcastMessage("OnGroundMissing", (object)cachedGround, (SendMessageOptions)1);
				}
			}
			else
			{
				if (Physics.groundwatchdebug)
				{
					Debug.Log((object)("GroundWatch retry: " + fails));
				}
				Invoke(OnPhysicsNeighbourChanged, Physics.groundwatchdelay);
			}
		}
		else
		{
			fails = 0;
		}
	}

	private bool HasBuildingBlock()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity component = ((Component)this).GetComponent<BaseEntity>();
		List<Collider> list = Pool.Get<List<Collider>>();
		if ((Object)(object)component != (Object)null && !PlayerBoat.IsChildOfFinishedPlayerBoat(component))
		{
			Vis.Colliders<Collider>(((Component)this).transform.TransformPoint(groundPosition), radius, list, 2097152, (QueryTriggerInteraction)2);
		}
		else
		{
			Vis.Colliders<Collider>(((Component)this).transform.TransformPoint(groundPosition), radius, list, 136314880, (QueryTriggerInteraction)2);
		}
		bool result = false;
		foreach (Collider item in list)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(((Component)item).gameObject);
			if (!((Object)(object)baseEntity == (Object)null) && !baseEntity.IsDestroyed && !baseEntity.isClient && baseEntity is BuildingBlock)
			{
				result = true;
				break;
			}
		}
		Pool.FreeUnmanaged<Collider>(ref list);
		return result;
	}

	public bool OnGround()
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity component = ((Component)this).GetComponent<BaseEntity>();
		if (Object.op_Implicit((Object)(object)component) && component.isServer)
		{
			if (component.HasParent() && !PlayerBoat.IsChildOfFinishedPlayerBoat(component))
			{
				return true;
			}
			Construction construction = PrefabAttribute.server.Find<Construction>(component.prefabID);
			if ((bool)construction)
			{
				Socket_Base[] allSockets = construction.allSockets;
				for (int i = 0; i < allSockets.Length; i++)
				{
					SocketMod[] socketMods = allSockets[i].socketMods;
					for (int j = 0; j < socketMods.Length; j++)
					{
						SocketMod_AreaCheck socketMod_AreaCheck = socketMods[j] as SocketMod_AreaCheck;
						if (!socketMod_AreaCheck || !socketMod_AreaCheck.wantsInside)
						{
							continue;
						}
						if (needOnlyOneAreaCheckValid)
						{
							if (socketMod_AreaCheck.DoCheck(((Component)component).transform.position, ((Component)component).transform.rotation, component))
							{
								return true;
							}
						}
						else if (!socketMod_AreaCheck.DoCheck(((Component)component).transform.position, ((Component)component).transform.rotation, component))
						{
							if (Physics.groundwatchdebug)
							{
								Debug.Log((object)("GroundWatch failed: " + socketMod_AreaCheck.hierachyName));
							}
							return false;
						}
					}
				}
			}
		}
		if (Physics.groundwatchdebug)
		{
			Debug.Log((object)"GroundWatch failed: Legacy radius check");
		}
		if (LegacyRadiusCheck(component))
		{
			return true;
		}
		return false;
	}

	private void CacheGround()
	{
		BaseEntity baseEntity = GetBaseEntity();
		if ((Object)(object)baseEntity != (Object)null && baseEntity.isServer)
		{
			LegacyRadiusCheck(baseEntity);
		}
	}

	private bool LegacyRadiusCheck(BaseEntity entity)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)entity == (Object)null)
		{
			return false;
		}
		List<Collider> list = Pool.Get<List<Collider>>();
		Vis.Colliders<Collider>(((Component)this).transform.TransformPoint(groundPosition), radius, list, LayerMask.op_Implicit(layers), (QueryTriggerInteraction)2);
		foreach (Collider item in list)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(((Component)item).gameObject);
			if ((Object)(object)baseEntity == (Object)null)
			{
				Pool.FreeUnmanaged<Collider>(ref list);
				return true;
			}
			if ((Object)(object)baseEntity != (Object)null && ((Object)(object)baseEntity == (Object)(object)entity || baseEntity.IsDestroyed || baseEntity.isClient))
			{
				continue;
			}
			if (whitelist != null && whitelist.Length != 0)
			{
				bool flag = false;
				BaseEntity[] array = whitelist;
				foreach (BaseEntity baseEntity2 in array)
				{
					if (baseEntity.prefabID == baseEntity2.prefabID)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
			}
			DecayEntity decayEntity = entity as DecayEntity;
			DecayEntity decayEntity2 = baseEntity as DecayEntity;
			if (!((Object)(object)decayEntity != (Object)null) || decayEntity.buildingID == 0 || !((Object)(object)decayEntity2 != (Object)null) || decayEntity2.buildingID == 0 || decayEntity.buildingID == decayEntity2.buildingID)
			{
				cachedGround = baseEntity as BaseCombatEntity;
				Pool.FreeUnmanaged<Collider>(ref list);
				return true;
			}
		}
		Pool.FreeUnmanaged<Collider>(ref list);
		return false;
	}
}

using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;

public class StabilityEntity : DecayEntity
{
	public class StabilityCheckWorkQueue : ObjectWorkQueue<StabilityEntity>
	{
		protected override void RunJob(StabilityEntity entity)
		{
			if (((ObjectWorkQueue<StabilityEntity>)this).ShouldAdd(entity))
			{
				entity.StabilityCheck();
			}
		}

		protected override bool ShouldAdd(StabilityEntity entity)
		{
			if (!ConVar.Server.stability)
			{
				return false;
			}
			if (!entity.IsValid())
			{
				return false;
			}
			if (!entity.isServer)
			{
				return false;
			}
			return true;
		}
	}

	public class UpdateSurroundingsQueue : ObjectWorkQueue<Bounds>
	{
		protected override void RunJob(Bounds bounds)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			NotifyNeighbours(bounds);
		}

		public static void NotifyNeighbours(Bounds bounds)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			if (!ConVar.Server.stability)
			{
				return;
			}
			List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
			Vector3 center = bounds.center;
			Vector3 extents = bounds.extents;
			Vis.Entities(center, extents.magnitude + 1f, list, -2010478334, (QueryTriggerInteraction)2);
			foreach (BaseEntity item in list)
			{
				if (!item.IsDestroyed && !item.isClient)
				{
					if (item is StabilityEntity stabilityEntity)
					{
						stabilityEntity.OnPhysicsNeighbourChanged();
					}
					else
					{
						((Component)item).BroadcastMessage("OnPhysicsNeighbourChanged", (SendMessageOptions)1);
					}
				}
			}
			Pool.FreeUnmanaged<BaseEntity>(ref list);
		}

		protected override bool IsValidToRun(Bounds entity)
		{
			return true;
		}
	}

	public class Support
	{
		public StabilityEntity parent;

		public EntityLink link;

		public float factor = 1f;

		public Support(StabilityEntity parent, EntityLink link, float factor)
		{
			this.parent = parent;
			this.link = link;
			this.factor = factor;
		}

		public StabilityEntity SupportEntity(StabilityEntity ignoreEntity = null)
		{
			StabilityEntity stabilityEntity = null;
			for (int i = 0; i < link.connections.Count; i++)
			{
				StabilityEntity stabilityEntity2 = link.connections[i].owner as StabilityEntity;
				Socket_Base socket = link.connections[i].socket;
				if ((Object)(object)stabilityEntity2 == (Object)null || (Object)(object)stabilityEntity2 == (Object)(object)parent || (Object)(object)stabilityEntity2 == (Object)(object)ignoreEntity || stabilityEntity2.isClient || stabilityEntity2.IsDestroyed || socket is ConstructionSocket { femaleNoStability: not false })
				{
					continue;
				}
				if ((Object)(object)stabilityEntity == (Object)null)
				{
					stabilityEntity = stabilityEntity2;
				}
				else if (Stability.support_highest_stability)
				{
					if (stabilityEntity2.cachedStability > stabilityEntity.cachedStability)
					{
						stabilityEntity = stabilityEntity2;
					}
				}
				else if (stabilityEntity2.cachedDistanceFromGround < stabilityEntity.cachedDistanceFromGround)
				{
					stabilityEntity = stabilityEntity2;
				}
			}
			return stabilityEntity;
		}
	}

	public enum TriangularPrismDirection
	{
		PositiveZ,
		NegativeZ,
		PositiveX,
		NegativeX
	}

	public static StabilityCheckWorkQueue stabilityCheckQueue = new StabilityCheckWorkQueue();

	public static UpdateSurroundingsQueue updateSurroundingsQueue = new UpdateSurroundingsQueue();

	public bool grounded;

	public bool useTriangularPrismRadialDamageVisibility;

	public TriangularPrismDirection triangularPrismDirection;

	[NonSerialized]
	public float cachedStability;

	[NonSerialized]
	public int cachedDistanceFromGround = int.MaxValue;

	private int stabilityUpdateDepth;

	private List<Support> supports;

	private int stabilityStrikes;

	private bool dirty;

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.stabilityEntity = Pool.Get<StabilityEntity>();
		info.msg.stabilityEntity.stability = cachedStability;
		info.msg.stabilityEntity.distanceFromGround = cachedDistanceFromGround;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.stabilityEntity != null)
		{
			cachedStability = info.msg.stabilityEntity.stability;
			cachedDistanceFromGround = info.msg.stabilityEntity.distanceFromGround;
			if (cachedStability <= 0f)
			{
				cachedStability = 0f;
			}
			if (cachedDistanceFromGround <= 0)
			{
				cachedDistanceFromGround = int.MaxValue;
			}
		}
	}

	public override void ResetState()
	{
		base.ResetState();
		cachedStability = 0f;
		cachedDistanceFromGround = int.MaxValue;
		if (isServer)
		{
			supports = null;
			stabilityStrikes = 0;
			dirty = false;
		}
	}

	public void InitializeSupports()
	{
		supports = new List<Support>();
		if (grounded || (HasParent() && !GetParentEntity().AllowInitChildSupports()))
		{
			return;
		}
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			EntityLink entityLink = entityLinks[i];
			if (entityLink.IsMale())
			{
				if (entityLink.socket is StabilitySocket)
				{
					supports.Add(new Support(this, entityLink, (entityLink.socket as StabilitySocket).support));
				}
				if (entityLink.socket is ConstructionSocket)
				{
					supports.Add(new Support(this, entityLink, (entityLink.socket as ConstructionSocket).support));
				}
			}
		}
	}

	private bool ParentForcesFullStability()
	{
		BaseEntity baseEntity = GetParentEntity();
		if ((Object)(object)baseEntity != (Object)null)
		{
			return baseEntity.ForceChildFullStability();
		}
		return false;
	}

	public int DistanceFromGround(StabilityEntity ignoreEntity = null)
	{
		if (grounded || ParentForcesFullStability())
		{
			return 1;
		}
		if (supports == null)
		{
			return 1;
		}
		if ((Object)(object)ignoreEntity == (Object)null)
		{
			ignoreEntity = this;
		}
		int num = int.MaxValue;
		for (int i = 0; i < supports.Count; i++)
		{
			StabilityEntity stabilityEntity = supports[i].SupportEntity(ignoreEntity);
			if (!((Object)(object)stabilityEntity == (Object)null))
			{
				int num2 = stabilityEntity.CachedDistanceFromGround(ignoreEntity);
				if (num2 != int.MaxValue)
				{
					num = Mathf.Min(num, num2 + 1);
				}
			}
		}
		return num;
	}

	public float SupportValue(out StabilityEntity supportEntity, StabilityEntity ignoreEntity = null)
	{
		supportEntity = null;
		if (grounded || ParentForcesFullStability())
		{
			return 1f;
		}
		if (supports == null)
		{
			return 1f;
		}
		if ((Object)(object)ignoreEntity == (Object)null)
		{
			ignoreEntity = this;
		}
		float num = 0f;
		for (int i = 0; i < supports.Count; i++)
		{
			Support support = supports[i];
			supportEntity = support.SupportEntity(ignoreEntity);
			if (!((Object)(object)supportEntity == (Object)null))
			{
				float num2 = supportEntity.CachedSupportValue(ignoreEntity);
				if (num2 != 0f)
				{
					num += num2 * support.factor;
				}
			}
		}
		return Mathf.Clamp01(num);
	}

	public int CachedDistanceFromGround(StabilityEntity ignoreEntity = null)
	{
		if (grounded || ParentForcesFullStability())
		{
			return 1;
		}
		if (supports == null)
		{
			return 1;
		}
		if ((Object)(object)ignoreEntity == (Object)null)
		{
			ignoreEntity = this;
		}
		int num = int.MaxValue;
		for (int i = 0; i < supports.Count; i++)
		{
			StabilityEntity stabilityEntity = supports[i].SupportEntity(ignoreEntity);
			if (!((Object)(object)stabilityEntity == (Object)null))
			{
				int num2 = stabilityEntity.cachedDistanceFromGround;
				if (num2 != int.MaxValue)
				{
					num = Mathf.Min(num, num2 + 1);
				}
			}
		}
		return num;
	}

	public float CachedSupportValue(StabilityEntity ignoreEntity = null)
	{
		if (grounded || ParentForcesFullStability())
		{
			return 1f;
		}
		if (supports == null)
		{
			return 1f;
		}
		if ((Object)(object)ignoreEntity == (Object)null)
		{
			ignoreEntity = this;
		}
		float num = 0f;
		for (int i = 0; i < supports.Count; i++)
		{
			Support support = supports[i];
			StabilityEntity stabilityEntity = support.SupportEntity(ignoreEntity);
			if (!((Object)(object)stabilityEntity == (Object)null))
			{
				float num2 = stabilityEntity.cachedStability;
				if (num2 != 0f)
				{
					num += num2 * support.factor;
				}
			}
		}
		return Mathf.Clamp01(num);
	}

	public void LogStabilityUpdate(string reason)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		DebugEx.Log(string.Format("[Stability] [Depth:{0}] Updating {1} at position {2} with distance {3} and building ID {4} due to ({5})", new object[6]
		{
			stabilityUpdateDepth,
			this,
			((Component)this).transform.position,
			cachedDistanceFromGround,
			buildingID,
			reason
		}), (StackTraceLogType)0);
	}

	public virtual void StabilityCheck()
	{
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		if (IsDestroyed || Interface.CallHook("OnEntityStabilityCheck", this) != null)
		{
			return;
		}
		if (supports == null)
		{
			InitializeSupports();
		}
		bool flag = false;
		int num = DistanceFromGround();
		if (num != cachedDistanceFromGround)
		{
			float num2 = cachedDistanceFromGround;
			cachedDistanceFromGround = num;
			if (!Stability.support_highest_stability)
			{
				if (Stability.log_stability_updates)
				{
					LogStabilityUpdate($"old distance : {num2} != new distance {num}");
				}
				flag = true;
			}
		}
		float num3 = SupportValue(out var supportEntity);
		if (Mathf.Abs(cachedStability - num3) > Stability.accuracy)
		{
			if (Stability.log_stability_updates)
			{
				LogStabilityUpdate(string.Format("old stability: {0} new stability: {1} support entity: {2}", cachedStability, num3, ((Object)(object)supportEntity == (Object)null) ? "null" : ((object)supportEntity).ToString()));
			}
			cachedStability = num3;
			flag = true;
		}
		if (flag)
		{
			dirty = true;
			UpdateConnectedEntities();
			UpdateStability(stabilityUpdateDepth + 1);
		}
		else if (dirty)
		{
			dirty = false;
			SendNetworkUpdate();
		}
		if (num3 < Stability.collapse)
		{
			if (stabilityStrikes < Stability.strikes)
			{
				if (Stability.log_stability_updates)
				{
					LogStabilityUpdate($"stability strikes {stabilityStrikes} / {Stability.strikes}");
				}
				UpdateStability(stabilityUpdateDepth + 1);
				stabilityStrikes++;
				return;
			}
			if (Stability.log_stability_death)
			{
				Debug.Log((object)string.Format("Killing '{0}' at position {1} due to low stability: {2} < {3}%", new object[4]
				{
					((object)this).ToString(),
					((Component)this).transform.position,
					Math.Round(num3 * 100f, 2),
					Math.Round(Stability.collapse * 100f, 1)
				}));
			}
			Kill(DestroyMode.Gib);
		}
		else
		{
			stabilityStrikes = 0;
		}
	}

	public void UpdateStability(int depth = 0)
	{
		stabilityUpdateDepth = depth;
		((ObjectWorkQueue<StabilityEntity>)stabilityCheckQueue).Add(this);
	}

	public void UpdateSurroundingEntities()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		UpdateSurroundingsQueue updateSurroundingsQueue = StabilityEntity.updateSurroundingsQueue;
		OBB val = WorldSpaceBounds();
		((ObjectWorkQueue<Bounds>)updateSurroundingsQueue).Add(val.ToBounds());
	}

	public void UpdateConnectedEntities()
	{
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			EntityLink entityLink = entityLinks[i];
			if (!entityLink.IsFemale())
			{
				continue;
			}
			for (int j = 0; j < entityLink.connections.Count; j++)
			{
				StabilityEntity stabilityEntity = entityLink.connections[j].owner as StabilityEntity;
				if (!((Object)(object)stabilityEntity == (Object)null) && !stabilityEntity.isClient && !stabilityEntity.IsDestroyed)
				{
					stabilityEntity.UpdateStability(stabilityUpdateDepth + 1);
				}
			}
		}
	}

	protected void OnPhysicsNeighbourChanged()
	{
		if (!IsDestroyed)
		{
			StabilityCheck();
		}
	}

	protected void DebugNudge()
	{
		StabilityCheck();
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!Application.isLoadingSave)
		{
			UpdateStability();
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		UpdateSurroundingEntities();
	}

	public override bool IsVisibleRadialDamage(Vector3 position, float maxDistance = float.PositiveInfinity)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		if (!useTriangularPrismRadialDamageVisibility)
		{
			return base.IsVisibleRadialDamage(position, maxDistance);
		}
		float m2 = maxDistance * maxDistance;
		OBB val = WorldSpaceBounds();
		Vector3 extents = val.extents;
		Triangle val2 = triangularPrismDirection switch
		{
			TriangularPrismDirection.NegativeZ => new Triangle(new Vector3(0f - extents.x, 0f, extents.z), new Vector3(extents.x, 0f, extents.z), new Vector3(0f, 0f, 0f - extents.z)), 
			TriangularPrismDirection.PositiveX => new Triangle(new Vector3(0f - extents.x, 0f, 0f - extents.z), new Vector3(0f - extents.x, 0f, extents.z), new Vector3(extents.x, 0f, 0f)), 
			TriangularPrismDirection.NegativeX => new Triangle(new Vector3(extents.x, 0f, extents.z), new Vector3(extents.x, 0f, 0f - extents.z), new Vector3(0f - extents.x, 0f, 0f)), 
			_ => new Triangle(new Vector3(extents.x, 0f, 0f - extents.z), new Vector3(0f - extents.x, 0f, 0f - extents.z), new Vector3(0f, 0f, extents.z)), 
		};
		Vector3 val3 = val.position + val.rotation * val2.Center;
		if (InRange(val3) && CanSee(position, val3))
		{
			return true;
		}
		Vector3 val4 = Quaternion.Inverse(val.rotation) * (position - val.position);
		Vector3 val5 = val2.ClosestPoint(Vector3Ex.WithY(val4, 0f));
		val5.y = Mathf.Clamp(val4.y, 0f - extents.y, extents.y);
		Vector3 val6 = val.position + val.rotation * val5;
		if (InRange(val6) && CanSee(position, val6))
		{
			return true;
		}
		return false;
		bool InRange(Vector3 p)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return Vector3.SqrMagnitude(p - position) <= m2;
		}
	}
}

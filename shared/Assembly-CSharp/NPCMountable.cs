using System;
using Rust;
using Rust.Ai.Gen2;
using UnityEngine;

public class NPCMountable : EntityComponent<BaseEntity>
{
	[Serializable]
	public struct NPCMountAnchor
	{
		[Tooltip("Where the NPC sits. It is parented to the vehicle and zeroed against this transform.")]
		public Transform anchor;

		[Tooltip("Optional ground point on the world navmesh the NPC walks to before mounting. Falls back to the anchor itself.")]
		public Transform approach;

		[Tooltip("How close a parented NPC has to be to this anchor to count as sitting in it. Zero uses the default.")]
		public float radius;
	}

	[Header("NPC Mountable")]
	[Tooltip("The seats on this vehicle. An NPC takes the nearest free one.")]
	public NPCMountAnchor[] anchors;

	[Tooltip("Where NPCs are put down when they get off. Same name and meaning as BaseMountable's.")]
	public Transform[] dismountPositions;

	[Tooltip("Whether a timed mount refuses to start while the host is moving. Root motion accumulates in world space, so a moving deck slides out from under the montage.")]
	public bool requireStationary = true;

	[Tooltip("How far water can rise above an anchor before its rider is put off, so a sunk vehicle does not drown what it carries.")]
	public float maxAnchorWaterDepth = 0.5f;

	private const float DefaultAnchorRadius = 0.75f;

	private const float DismountPointRadius = 0.5f;

	private const float DismountNavmeshSampleRadius = 2f;

	private EntityRef<BaseNPC2>[] occupants;

	private const float RideableCheckInterval = 1f;

	private Action _checkRideable;

	public int AnchorCount
	{
		get
		{
			if (anchors == null)
			{
				return 0;
			}
			return anchors.Length;
		}
	}

	public bool IsHostStationary
	{
		get
		{
			if (((baseEntity is BaseVehicleModule baseVehicleModule) ? baseVehicleModule.Vehicle : baseEntity) is BaseVehicle baseVehicle)
			{
				return baseVehicle.IsStationary();
			}
			return true;
		}
	}

	public bool IsHostRideable
	{
		get
		{
			if (!IsHostFlipped)
			{
				return !IsAnyAnchorSubmerged;
			}
			return false;
		}
	}

	private bool IsHostFlipped
	{
		get
		{
			if (baseEntity is BaseVehicle baseVehicle)
			{
				return baseVehicle.IsFlipped();
			}
			return false;
		}
	}

	private bool IsAnyAnchorSubmerged
	{
		get
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < AnchorCount; i++)
			{
				if (IsValidAnchor(i) && WaterLevel.GetWaterDepth(anchors[i].anchor.position, waves: true, volumes: true, baseEntity) > maxAnchorWaterDepth)
				{
					return true;
				}
			}
			return false;
		}
	}

	public int MountedCount
	{
		get
		{
			int num = 0;
			foreach (BaseEntity child in baseEntity.children)
			{
				if (child is BaseNPC2 { IsMounted: not false })
				{
					num++;
				}
			}
			return num;
		}
	}

	public bool AnyMounted => MountedCount > 0;

	public bool HasFreeAnchor => MountedCount < AnchorCount;

	private Action CheckRideableAction => CheckRideable;

	private bool HasAnyOccupant
	{
		get
		{
			for (int i = 0; i < occupants.Length; i++)
			{
				if ((Object)(object)occupants[i].Get(serverside: true) != (Object)null)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool IsValidAnchor(int index)
	{
		if (index >= 0 && index < AnchorCount)
		{
			return (Object)(object)anchors[index].anchor != (Object)null;
		}
		return false;
	}

	public Transform GetAnchorTransform(int index)
	{
		if (!IsValidAnchor(index))
		{
			return null;
		}
		return anchors[index].anchor;
	}

	public Vector3 GetApproachPosition(int index)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (!IsValidAnchor(index))
		{
			return ((Component)baseEntity).transform.position;
		}
		Transform approach = anchors[index].approach;
		if (!((Object)(object)approach != (Object)null))
		{
			return anchors[index].anchor.position;
		}
		return approach.position;
	}

	private float AnchorRadius(int index)
	{
		if (!(anchors[index].radius > 0f))
		{
			return 0.75f;
		}
		return anchors[index].radius;
	}

	public override void InitShared()
	{
		base.InitShared();
		if (baseEntity.isServer)
		{
			occupants = new EntityRef<BaseNPC2>[AnchorCount];
		}
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		if (!baseEntity.isServer || Application.isQuitting || occupants == null)
		{
			return;
		}
		for (int i = 0; i < occupants.Length; i++)
		{
			BaseNPC2 baseNPC = occupants[i].Get(serverside: true);
			if ((Object)(object)baseNPC != (Object)null)
			{
				DismountImmediate(baseNPC);
			}
		}
	}

	public bool IsAnchorFree(int index)
	{
		if (IsValidAnchor(index))
		{
			return (Object)(object)occupants[index].Get(serverside: true) == (Object)null;
		}
		return false;
	}

	private void Occupy(int index, BaseNPC2 npc)
	{
		occupants[index].Set(npc);
		if (!IsInvoking(CheckRideableAction))
		{
			InvokeRandomized(CheckRideableAction, 1f, 1f, 0.1f);
		}
	}

	private void CheckRideable()
	{
		if (!HasAnyOccupant)
		{
			CancelInvoke(CheckRideableAction);
		}
		else
		{
			if (IsHostRideable)
			{
				return;
			}
			for (int i = 0; i < occupants.Length; i++)
			{
				BaseNPC2 baseNPC = occupants[i].Get(serverside: true);
				if ((Object)(object)baseNPC != (Object)null && !baseNPC.IsDismounting)
				{
					DismountImmediate(baseNPC);
				}
			}
		}
	}

	public bool TryGetAnchorIndex(BaseNPC2 npc, out int index)
	{
		index = -1;
		if ((Object)(object)npc == (Object)null || occupants == null)
		{
			return false;
		}
		for (int i = 0; i < occupants.Length; i++)
		{
			if (!((Object)(object)occupants[i].Get(serverside: true) != (Object)(object)npc))
			{
				index = i;
				return true;
			}
		}
		return false;
	}

	public bool TryReserveAnchor(BaseNPC2 npc, out int index)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		index = -1;
		if ((Object)(object)npc == (Object)null || occupants == null)
		{
			return false;
		}
		float num = float.MaxValue;
		for (int i = 0; i < occupants.Length; i++)
		{
			if (IsAnchorFree(i))
			{
				float num2 = Vector3.Distance(anchors[i].anchor.position, ((Component)npc).transform.position);
				if (!(num2 >= num))
				{
					num = num2;
					index = i;
				}
			}
		}
		if (index < 0)
		{
			return false;
		}
		Occupy(index, npc);
		return true;
	}

	public void ReleaseAnchor(BaseNPC2 npc)
	{
		if (TryGetAnchorIndex(npc, out var index))
		{
			occupants[index] = default;
		}
	}

	public void MountImmediate(BaseNPC2 npc, int index)
	{
		if (!((Object)(object)npc == (Object)null) && IsValidAnchor(index))
		{
			Occupy(index, npc);
			npc.OnMountFinalized(baseEntity, index);
			npc.ResumeMountedState();
		}
	}

	public bool RequestMount(BaseNPC2 npc)
	{
		if ((Object)(object)npc == (Object)null || npc.IsDead() || (Object)(object)npc.Mount != (Object)null)
		{
			return false;
		}
		if (requireStationary && !IsHostStationary)
		{
			return false;
		}
		if (!IsHostRideable)
		{
			return false;
		}
		if (!TryReserveAnchor(npc, out var index))
		{
			return false;
		}
		if (npc.StartMountTo(this, index))
		{
			return true;
		}
		occupants[index] = default;
		return false;
	}

	public bool RequestDismount(BaseNPC2 npc)
	{
		if ((Object)(object)npc == (Object)null || !TryGetAnchorIndex(npc, out var _))
		{
			return false;
		}
		if (npc.IsMounted)
		{
			if (!TryGetDismountPosition(npc, out var _, out var _))
			{
				return false;
			}
			if (npc.StartDismount())
			{
				return true;
			}
		}
		DismountImmediate(npc);
		return true;
	}

	public bool TryDismountOne()
	{
		if (occupants == null)
		{
			return false;
		}
		for (int i = 0; i < occupants.Length; i++)
		{
			BaseNPC2 baseNPC = occupants[i].Get(serverside: true);
			if ((Object)(object)baseNPC == (Object)null)
			{
				occupants[i] = default;
			}
			else if (!baseNPC.IsDismounting && RequestDismount(baseNPC))
			{
				return true;
			}
		}
		return false;
	}

	public void DismountImmediate(BaseNPC2 npc)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)npc == (Object)null))
		{
			if (!npc.IsMounted)
			{
				npc.CancelMount();
				return;
			}
			TryGetDismountPosition(npc, out var position, out var rotation);
			npc.OnDismountFinalized(position, rotation);
		}
	}

	public bool TryGetDismountPosition(BaseNPC2 npc, out Vector3 position, out Quaternion rotation)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		bool flag = false;
		position = default;
		rotation = default;
		if (dismountPositions != null)
		{
			Transform[] array = dismountPositions;
			foreach (Transform val in array)
			{
				if (!((Object)(object)val == (Object)null) && !IsDismountPointTaken(val, npc))
				{
					float num2 = Vector3.Distance(val.position, ((Component)npc).transform.position);
					if (!(num2 >= num) && npc.TrySampleNavmesh(val.position, 2f, out var onNavmesh))
					{
						num = num2;
						position = onNavmesh;
						rotation = Upright(val.rotation);
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			return true;
		}
		if (TryGetAnchorIndex(npc, out var index) && IsValidAnchor(index) && (Object)(object)anchors[index].approach != (Object)null && npc.TrySampleNavmesh(anchors[index].approach.position, 2f, out position))
		{
			rotation = Upright(anchors[index].approach.rotation);
			return true;
		}
		Transform transform = ((Component)baseEntity).transform;
		rotation = Upright(Quaternion.LookRotation(-transform.forward));
		position = transform.position - transform.forward * (baseEntity.bounds.extents.z + 1f);
		if (npc.TrySampleNavmesh(position, 2f, out var onNavmesh2))
		{
			position = onNavmesh2;
			return true;
		}
		return false;
	}

	private static Quaternion Upright(Quaternion rotation)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.ProjectOnPlane(rotation * Vector3.forward, Vector3.up);
		if (val.sqrMagnitude < 0.001f)
		{
			val = Vector3.ProjectOnPlane(rotation * Vector3.up, Vector3.up);
		}
		if (!(val.sqrMagnitude < 0.001f))
		{
			return Quaternion.LookRotation(val);
		}
		return Quaternion.identity;
	}

	private bool IsDismountPointTaken(Transform point, BaseNPC2 asking)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		EntityRef<BaseNPC2>[] array = occupants;
		foreach (EntityRef<BaseNPC2> entityRef in array)
		{
			BaseNPC2 baseNPC = entityRef.Get(serverside: true);
			if (!((Object)(object)baseNPC == (Object)null) && !((Object)(object)baseNPC == (Object)(object)asking) && Vector3.Distance(((Component)baseNPC).transform.position, point.position) < 0.5f)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryReclaimAnchor(BaseNPC2 npc, out int index)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		index = -1;
		if ((Object)(object)npc == (Object)null || occupants == null)
		{
			return false;
		}
		float num = float.MaxValue;
		for (int i = 0; i < occupants.Length; i++)
		{
			if (IsAnchorFree(i))
			{
				float num2 = Vector3.Distance(anchors[i].anchor.position, ((Component)npc).transform.position);
				if (!(num2 > AnchorRadius(i)) && !(num2 >= num))
				{
					num = num2;
					index = i;
				}
			}
		}
		if (index < 0)
		{
			return false;
		}
		Occupy(index, npc);
		return true;
	}

	public bool HandOverAnchor(BaseNPC2 from, BaseNPC2 to)
	{
		if ((Object)(object)to == (Object)null || !TryGetAnchorIndex(from, out var index))
		{
			return false;
		}
		occupants[index].Set(to);
		return true;
	}
}

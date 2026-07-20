using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using UnityEngine;
using UnityEngine.Serialization;

public class Construction : PrefabAttribute
{
	public struct Target
	{
		public bool valid;

		public Ray ray;

		public BaseEntity entity;

		public Socket_Base socket;

		public bool onTerrain;

		public Vector3 position;

		public Vector3 normal;

		public Vector3 rotation;

		public BasePlayer player;

		public bool buildingBlocked;

		public bool isHoldingShift;

		public int snappingMode;

		public bool isSnapped;

		public Vector3 snappedPosition;

		public Vector3 snappedRotation;

		public bool shouldParent;

		public Quaternion GetWorldRotation(bool female)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			Quaternion val = socket.rotation;
			if (socket.male && socket.female && female)
			{
				val = socket.rotation * Quaternion.Euler(180f, 0f, 180f);
			}
			return ((Component)entity).transform.rotation * val;
		}

		public Vector3 GetWorldPosition()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			Matrix4x4 localToWorldMatrix = ((Component)entity).transform.localToWorldMatrix;
			return ((Matrix4x4)(ref localToWorldMatrix)).MultiplyPoint3x4(socket.position);
		}
	}

	public struct Placement
	{
		public Vector3 position;

		public Quaternion rotation;

		public bool isPopulated;

		public bool shouldParent;

		public bool parentPassed;

		public readonly bool isHoldingShift;

		public Transform transform;

		public BaseEntity ignoredEntity;

		public Placement(Target target)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			isHoldingShift = target.isHoldingShift;
			position = Vector3.zero;
			rotation = Quaternion.identity;
			isPopulated = true;
			transform = null;
			ignoredEntity = null;
			shouldParent = target.shouldParent;
			parentPassed = false;
			if ((Object)(object)target.entity != (Object)null)
			{
				transform = ((Component)target.entity).transform;
			}
		}

		public bool ShouldIgnoreCollider(Collider col)
		{
			if ((Object)(object)ignoredEntity == (Object)null || (Object)(object)col == (Object)null)
			{
				return false;
			}
			return ShouldIgnoreEntity(GameObjectEx.ToBaseEntity(col));
		}

		public bool ShouldIgnoreEntity(BaseEntity ent)
		{
			if ((Object)(object)ignoredEntity == (Object)null || (Object)(object)ent == (Object)null)
			{
				return false;
			}
			if ((Object)(object)ent == (Object)(object)ignoredEntity)
			{
				return true;
			}
			return false;
		}
	}

	public class Grade
	{
		public BuildingGrade grade;

		public float maxHealth;

		public List<ItemAmount> costToBuild;

		public PhysicsMaterial physicMaterial => grade.physicMaterial;

		public ProtectionProperties damageProtecton => grade.damageProtecton;
	}

	public static Phrase lastPlacementError = Phrase.op_Implicit(string.Empty);

	public static bool lastPlacementErrorIsDetailed;

	public static string lastPlacementErrorDebug;

	public static BuildingBlock lastBuildingBlockError;

	public BaseEntity.Menu.Option info;

	public bool canBypassBuildingPermission;

	public bool showBuildingBlockedPreview = true;

	[InspectorName("Can Bypass Road Checks")]
	public bool canPlaceOnRoads;

	[FormerlySerializedAs("canRotate")]
	public bool canRotateBeforePlacement;

	[FormerlySerializedAs("canRotate")]
	public bool canRotateAfterPlacement;

	public bool canBeDeployedInDeepSeaWater;

	public bool checkVolumeOnRotate;

	public bool checkVolumeOnUpgrade;

	public bool canPlaceAtMaxDistance;

	public bool placeOnWater;

	public bool overridePlacementLayer;

	public LayerMask overridedPlacementLayer;

	public LayerMask additionalPlacementLayer;

	public Vector3 rotationAmount = new Vector3(0f, 90f, 0f);

	public Vector3 applyStartingRotation = Vector3.zero;

	public Transform deployOffset;

	public bool enforceLineOfSightCheckAgainstParentEntity;

	[Tooltip("Axis Snapping for IO Entities.")]
	public bool canSnap;

	public bool forceY;

	public float forceYValue;

	public float holdToPlaceDuration;

	public bool canFloodFillSockets;

	[Space]
	public bool alternativeLOSChecks;

	public Vector3[] alternativeLOSPositions;

	public bool canUseLastValidPosition = true;

	[Range(0f, 10f)]
	public float healthMultiplier = 1f;

	[Range(0f, 10f)]
	public float costMultiplier = 1f;

	[Range(1f, 50f)]
	public float maxplaceDistance = 4f;

	[Range(0f, 10f)]
	public float minPlaceDistance = 1f;

	public Mesh guideMesh;

	public Material[] guideMeshMaterial;

	public GameObjectRef placeEffect;

	[Space]
	public bool hideBuildingPlannerViewmodel;

	[NonSerialized]
	public Socket_Base[] allSockets;

	[NonSerialized]
	public BuildingProximity[] allProximities;

	[NonSerialized]
	public ConstructionGrade defaultGrade;

	[NonSerialized]
	public SocketHandle socketHandle;

	[NonSerialized]
	public Bounds bounds;

	[NonSerialized]
	public bool isBuildingPrivilege;

	[NonSerialized]
	public bool isSleepingBag;

	[NonSerialized]
	public ConstructionGrade[] grades;

	[NonSerialized]
	public Deployable deployable;

	[NonSerialized]
	public ConstructionPlaceholder placeholder;

	[ReplicatedVar]
	public static bool alternativeLOSChecks_enabled = true;

	public bool UpdatePlacement(Transform transform, Construction common, ref Target target)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_0524: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_0589: Unknown result type (might be due to invalid IL or missing references)
		//IL_0590: Unknown result type (might be due to invalid IL or missing references)
		//IL_054f: Unknown result type (might be due to invalid IL or missing references)
		//IL_055c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_061f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0626: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0635: Unknown result type (might be due to invalid IL or missing references)
		//IL_0642: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0661: Unknown result type (might be due to invalid IL or missing references)
		//IL_0668: Unknown result type (might be due to invalid IL or missing references)
		//IL_066e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0706: Unknown result type (might be due to invalid IL or missing references)
		//IL_070d: Unknown result type (might be due to invalid IL or missing references)
		//IL_067e: Unknown result type (might be due to invalid IL or missing references)
		//IL_068b: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0783: Unknown result type (might be due to invalid IL or missing references)
		//IL_078a: Unknown result type (might be due to invalid IL or missing references)
		//IL_079d: Unknown result type (might be due to invalid IL or missing references)
		if (common.deployable == null && !TargetIsInteractable(target))
		{
			return false;
		}
		if (common.forceY)
		{
			transform.position = Vector3Ex.WithY(transform.position, forceYValue);
		}
		if (!target.valid)
		{
			if (common.placeOnWater)
			{
				lastPlacementError = ConstructionErrors.WantsWater;
			}
			return false;
		}
		if (!common.canBypassBuildingPermission && !target.player.CanBuild(isServer))
		{
			lastPlacementError = ConstructionErrors.NoPermission;
			return false;
		}
		if (!canBeDeployedInDeepSeaWater && DeepSea.block_building && DeepSeaManager.IsInsideDeepSea(transform.position))
		{
			if ((Object)(object)target.entity == (Object)null && !canPlaceAtMaxDistance)
			{
				lastPlacementError = ConstructionErrors.CannotBuildInTheDeepSea;
				return false;
			}
			BoatBuildingStation forPlayer = BoatBuildingStation.GetForPlayer(target.player);
			if ((Object)(object)forPlayer == (Object)null || !forPlayer.IsInsideBuildArea(((Component)target.player).transform.position))
			{
				VehiclePrivilege vehiclePrivilege = target.player.GetVehicleBuildingPrivilege(cached: true) as VehiclePrivilege;
				if ((Object)(object)vehiclePrivilege == (Object)null)
				{
					lastPlacementError = ConstructionErrors.CannotBuildInTheDeepSea;
					return false;
				}
				if (!common.canBypassBuildingPermission)
				{
					if (!vehiclePrivilege.IsAuthed(target.player))
					{
						lastPlacementError = ConstructionErrors.CannotBuildInTheDeepSea;
						return false;
					}
					BaseVehicle parentVehicle = vehiclePrivilege.ParentVehicle;
					if ((Object)(object)parentVehicle == (Object)null)
					{
						lastPlacementError = ConstructionErrors.CannotBuildInTheDeepSea;
						return false;
					}
					BaseEntity entity = target.entity;
					BaseVehicle baseVehicle = ((entity != null) ? ((Component)entity).GetComponentInParent<BaseVehicle>() : null);
					if ((Object)(object)baseVehicle == (Object)null || (Object)(object)baseVehicle != (Object)(object)parentVehicle || !target.player.IsStandingOnEntity(parentVehicle, 134217728))
					{
						lastPlacementError = ConstructionErrors.CannotBuildInTheDeepSea;
						return false;
					}
				}
			}
		}
		List<Socket_Base> list = Pool.Get<List<Socket_Base>>();
		common.FindMaleSockets(target, list);
		foreach (Socket_Base item in list)
		{
			Placement placement = default(Placement);
			if ((Object)(object)target.entity != (Object)null && target.socket != null && target.entity.IsOccupied(target.socket))
			{
				continue;
			}
			if (!placement.isPopulated)
			{
				placement = item.DoPlacement(target);
			}
			if ((Object)(object)target.player != (Object)null && target.player.IsInTutorial)
			{
				TutorialIsland currentTutorialIsland = target.player.GetCurrentTutorialIsland();
				if ((Object)(object)currentTutorialIsland != (Object)null && !currentTutorialIsland.CheckPlacement(common, target, ref placement))
				{
					placement = default(Placement);
				}
			}
			if (!placement.isPopulated)
			{
				continue;
			}
			if (target.player.IsInCreativeMode && Creative.freePlacement)
			{
				transform.SetPositionAndRotation(placement.position, placement.rotation);
				return true;
			}
			if (!item.CheckSocketMods(ref placement))
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				continue;
			}
			if (!IOEntity.allow_on_boats && (Object)(object)target.entity != (Object)null)
			{
				GameObject obj = GameManager.server.FindPrefab(common.prefabID);
				BaseEntity baseEntity = ((obj != null) ? obj.GetComponent<BaseEntity>() : null);
				if ((baseEntity is IOEntity iOEntity && !(iOEntity is Signage)) || baseEntity is ElectricOven)
				{
					if (!(target.entity is BoatBuildingBlock))
					{
						BaseEntity entity2 = target.entity;
						if (!(entity2 is PlayerBoat) && !(entity2 is Tugboat) && !target.entity.HasParentBoat(out var _))
						{
							goto IL_0391;
						}
					}
					transform.position = placement.position;
					transform.rotation = placement.rotation;
					lastPlacementError = ConstructionErrors.CannotBuildOnBoat;
					continue;
				}
			}
			goto IL_0391;
			IL_0391:
			if (!TestPlacingThroughRock(ref placement, target))
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				lastPlacementError = ConstructionErrors.ThroughRock;
				continue;
			}
			if (common.HasAlternativeLOSChecks())
			{
				if (target.socket == null && !TestPlacingThroughWall(ref placement, transform, common, target))
				{
					transform.position = placement.position;
					transform.rotation = placement.rotation;
					lastPlacementError = ConstructionErrors.ThroughWalls;
					lastPlacementErrorDebug = "Placing through walls";
					continue;
				}
				if (!Planner.HasLineOfSight(ref placement, common, target))
				{
					transform.position = placement.position;
					transform.rotation = placement.rotation;
					lastPlacementError = ConstructionErrors.LineOfSightBlocked;
					continue;
				}
			}
			else if (!TestPlacingThroughWall(ref placement, transform, common, target))
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				lastPlacementError = ConstructionErrors.ThroughWalls;
				lastPlacementErrorDebug = "Placing through walls";
				continue;
			}
			if (!TestPlacingCloseToRoad(ref placement, target, common))
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				lastPlacementError = ConstructionErrors.TooCloseToRoad;
				continue;
			}
			if (target.entity is Door && target.socket == null)
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				lastPlacementError = ConstructionErrors.CantDeployOnDoor;
				continue;
			}
			if (Vector3.Distance(placement.position, target.player.eyes.position) > common.maxplaceDistance + 1f)
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				lastPlacementError = ConstructionErrors.TooFarAway;
				continue;
			}
			DeployVolume[] volumes = PrefabAttribute.server.FindAll<DeployVolume>(prefabID);
			if (DeployVolume.Check(placement.position, placement.rotation, volumes))
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				if ((Object)(object)DeployVolume.LastDeployHit != (Object)null)
				{
					lastPlacementErrorDebug = ((Object)DeployVolume.LastDeployHit).name;
					string blockedByErrorFromCollider = ConstructionErrors.GetBlockedByErrorFromCollider(DeployVolume.LastDeployHit, target.player);
					if (!string.IsNullOrEmpty(blockedByErrorFromCollider))
					{
						lastPlacementError = Phrase.op_Implicit(blockedByErrorFromCollider);
						lastPlacementErrorIsDetailed = true;
						continue;
					}
				}
				lastPlacementError = ConstructionErrors.NotEnoughSpace;
				continue;
			}
			if (BuildingProximity.Check(target.player, this, placement.position, placement.rotation))
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				continue;
			}
			if (common.isBuildingPrivilege && !target.player.CanPlaceBuildingPrivilege(placement.position, placement.rotation, common.bounds))
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				lastPlacementError = ConstructionErrors.StackPrivilege;
				continue;
			}
			bool flag = target.player.IsBuildingBlocked(placement.position, placement.rotation, common.bounds, cached: false);
			if (!common.canBypassBuildingPermission && flag)
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				lastPlacementError = ConstructionErrors.NoPermission;
				continue;
			}
			target.buildingBlocked = flag;
			transform.SetPositionAndRotation(placement.position, placement.rotation);
			if ((!((Object)(object)target.player != (Object)null) || !target.player.IsInCreativeMode || !Creative.bypassHoldToPlaceDuration) && common.holdToPlaceDuration > 0f && (Object)(object)target.player != (Object)null && isServer && target.player.GetHeldEntity() is Planner planner && (Vector3.Distance(((Component)target.player).transform.position, planner.serverStartDurationPlacementPosition) > 1f || Mathf.Abs(TimeSince.op_Implicit(planner.serverStartDurationPlacementTime) - common.holdToPlaceDuration) > 0.5f))
			{
				break;
			}
			Pool.FreeUnmanaged<Socket_Base>(ref list);
			return true;
		}
		Pool.FreeUnmanaged<Socket_Base>(ref list);
		return false;
	}

	private bool TestPlacingThroughRock(ref Placement placement, Target target)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		OBB val = default(OBB);
		((OBB)(ref val))._002Ector(placement.position, Vector3.one, placement.rotation, bounds);
		Vector3 center = target.player.GetCenter(ducked: true);
		Vector3 origin = ((Ray)(ref target.ray)).origin;
		if (Physics.Linecast(center, origin, 65536, (QueryTriggerInteraction)1))
		{
			return false;
		}
		RaycastHit val3 = default(RaycastHit);
		Vector3 val2 = (((OBB)(ref val)).Trace(target.ray, ref val3, float.PositiveInfinity) ? ((RaycastHit)(ref val3)).point : ((OBB)(ref val)).ClosestPoint(origin));
		if (Physics.Linecast(origin, val2, 65536, (QueryTriggerInteraction)1))
		{
			return false;
		}
		return true;
	}

	private static bool TestPlacingThroughWall(ref Placement placement, Transform transform, Construction common, Target target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = placement.position;
		if ((Object)(object)common.deployOffset != (Object)null)
		{
			val += placement.rotation * common.deployOffset.localPosition;
		}
		Vector3 val2 = val - ((Ray)(ref target.ray)).origin;
		RaycastHit hit = default(RaycastHit);
		if (!Physics.Raycast(((Ray)(ref target.ray)).origin, ((Vector3)(ref val2)).normalized, ref hit, ((Vector3)(ref val2)).magnitude, 2097152))
		{
			return true;
		}
		StabilityEntity stabilityEntity = RaycastHitEx.GetEntity(hit) as StabilityEntity;
		if (!common.enforceLineOfSightCheckAgainstParentEntity && (Object)(object)stabilityEntity != (Object)null && (Object)(object)target.entity == (Object)(object)stabilityEntity)
		{
			return true;
		}
		if (((Vector3)(ref val2)).magnitude - ((RaycastHit)(ref hit)).distance < 0.2f)
		{
			return true;
		}
		transform.SetPositionAndRotation(((RaycastHit)(ref hit)).point, placement.rotation);
		return false;
	}

	private bool TestPlacingCloseToRoad(ref Placement placement, Target target, Construction construction)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		if (construction.canPlaceOnRoads)
		{
			return true;
		}
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		TerrainTopologyMap topologyMap = TerrainMeta.TopologyMap;
		if ((Object)(object)heightMap == (Object)null)
		{
			return true;
		}
		if ((Object)(object)topologyMap == (Object)null)
		{
			return true;
		}
		OBB val = default(OBB);
		((OBB)(ref val))._002Ector(placement.position, Vector3.one, placement.rotation, bounds);
		float num = Mathf.Abs(heightMap.GetHeight(val.position) - val.position.y);
		if (num > 9f)
		{
			return true;
		}
		float radius = Mathf.Lerp(3f, 0f, num / 9f);
		Vector3 position = val.position;
		Vector3 point = ((OBB)(ref val)).GetPoint(-1f, 0f, -1f);
		Vector3 point2 = ((OBB)(ref val)).GetPoint(-1f, 0f, 1f);
		Vector3 point3 = ((OBB)(ref val)).GetPoint(1f, 0f, -1f);
		Vector3 point4 = ((OBB)(ref val)).GetPoint(1f, 0f, 1f);
		int topology = topologyMap.GetTopology(position, radius);
		int topology2 = topologyMap.GetTopology(point, radius);
		int topology3 = topologyMap.GetTopology(point2, radius);
		int topology4 = topologyMap.GetTopology(point3, radius);
		int topology5 = topologyMap.GetTopology(point4, radius);
		if (((topology | topology2 | topology3 | topology4 | topology5) & 0x80800) == 0)
		{
			return true;
		}
		return false;
	}

	public virtual bool ShowAsNeutral(Target target)
	{
		return target.buildingBlocked;
	}

	public bool TargetIsInteractable(Target target)
	{
		if ((Object)(object)target.entity == (Object)null)
		{
			return true;
		}
		if (target.entity is BuildingBlock buildingBlock)
		{
			return buildingBlock.Interactable();
		}
		return true;
	}

	public BaseEntity CreateConstruction(Target target, bool bNeedsValidPlacement = false)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = GameManager.server.CreatePrefab(fullName, Vector3.zero, Quaternion.identity, active: false);
		bool flag = UpdatePlacement(val.transform, this, ref target);
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(val);
		if (bNeedsValidPlacement && !flag)
		{
			if (baseEntity.IsValid())
			{
				baseEntity.Kill();
			}
			else
			{
				GameManager.Destroy(val);
			}
			return null;
		}
		DecayEntity decayEntity = baseEntity as DecayEntity;
		if (Object.op_Implicit((Object)(object)decayEntity))
		{
			decayEntity.AttachToBuilding(target.entity as DecayEntity);
		}
		return baseEntity;
	}

	public bool HasMaleSockets(Target target)
	{
		Socket_Base[] array = allSockets;
		foreach (Socket_Base socket_Base in array)
		{
			if (socket_Base.male && !socket_Base.maleDummy && socket_Base.TestTarget(target))
			{
				return true;
			}
		}
		return false;
	}

	public void FindMaleSockets(Target target, List<Socket_Base> sockets)
	{
		Socket_Base[] array = allSockets;
		foreach (Socket_Base socket_Base in array)
		{
			if (socket_Base.male && !socket_Base.maleDummy && socket_Base.TestTarget(target))
			{
				sockets.Add(socket_Base);
			}
		}
	}

	public ConstructionGrade GetGrade(BuildingGrade.Enum iGrade, ulong iSkin)
	{
		ConstructionGrade[] array = grades;
		foreach (ConstructionGrade constructionGrade in array)
		{
			if (constructionGrade.gradeBase.type == iGrade && constructionGrade.gradeBase.skin == iSkin)
			{
				return constructionGrade;
			}
		}
		return defaultGrade;
	}

	protected override void AttributeSetup(GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		base.AttributeSetup(rootObj, name, serverside, clientside, bundling);
		isBuildingPrivilege = Object.op_Implicit((Object)(object)rootObj.GetComponent<BuildingPrivlidge>());
		isSleepingBag = Object.op_Implicit((Object)(object)rootObj.GetComponent<SleepingBag>());
		bounds = rootObj.GetComponent<BaseEntity>().bounds;
		deployable = ((Component)this).GetComponent<Deployable>();
		placeholder = ((Component)this).GetComponentInChildren<ConstructionPlaceholder>();
		allSockets = ((Component)this).GetComponentsInChildren<Socket_Base>(true);
		allProximities = ((Component)this).GetComponentsInChildren<BuildingProximity>(true);
		socketHandle = ((Component)this).GetComponentsInChildren<SocketHandle>(true).FirstOrDefault();
		grades = rootObj.GetComponents<ConstructionGrade>();
		ConstructionGrade[] array = grades;
		foreach (ConstructionGrade constructionGrade in array)
		{
			if (!(constructionGrade == null))
			{
				constructionGrade.construction = this;
				if (!(defaultGrade != null))
				{
					defaultGrade = constructionGrade;
				}
			}
		}
	}

	protected override Type GetIndexedType()
	{
		return typeof(Construction);
	}

	public bool HasAlternativeLOSChecks()
	{
		if (alternativeLOSChecks_enabled && alternativeLOSChecks && alternativeLOSPositions != null)
		{
			return alternativeLOSPositions.Length != 0;
		}
		return false;
	}

	private bool CanDeployOnTugboat(Socket_Base baseSocket)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		SocketMod[] socketMods = baseSocket.socketMods;
		foreach (SocketMod socketMod in socketMods)
		{
			if (socketMod is SocketMod_AreaCheck socketMod_AreaCheck)
			{
				if (socketMod_AreaCheck.wantsInside && (LayerMask.op_Implicit(socketMod_AreaCheck.layerMask) & 0x8000000) > 0)
				{
					return true;
				}
			}
			else if (socketMod is SocketMod_SphereCheck { wantsCollide: not false } socketMod_SphereCheck && (LayerMask.op_Implicit(socketMod_SphereCheck.layerMask) & 0x8000000) > 0)
			{
				return true;
			}
		}
		return false;
	}
}

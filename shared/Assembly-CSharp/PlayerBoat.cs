using System;
using System.Collections.Generic;
using System.Text;
using Development.Attributes;
using Facepunch;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class PlayerBoat : BaseBoat, Anchor.IAnchorable, TriggerHurtNotChild.IHurtTriggerUser, IReceiveDeepSeaNotifications, ILargeVehicleForProjectiles, IPlannerReparentChildrenToMe
{
	public struct DragByAngle
	{
		public float[] dragByDirectionOfTravel;

		public int directionIncrements;

		public PlayerBoat boat;

		private const int substeps = 16;

		private const int hitsPerRay = 24;

		public void Init(int angleIncrements)
		{
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0213: Unknown result type (might be due to invalid IL or missing references)
			//IL_0241: Unknown result type (might be due to invalid IL or missing references)
			//IL_0250: Unknown result type (might be due to invalid IL or missing references)
			//IL_0259: Unknown result type (might be due to invalid IL or missing references)
			//IL_025e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0262: Unknown result type (might be due to invalid IL or missing references)
			//IL_0278: Unknown result type (might be due to invalid IL or missing references)
			//IL_027e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0287: Unknown result type (might be due to invalid IL or missing references)
			//IL_028c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0299: Unknown result type (might be due to invalid IL or missing references)
			//IL_029f: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_030a: Unknown result type (might be due to invalid IL or missing references)
			//IL_030f: Unknown result type (might be due to invalid IL or missing references)
			//IL_031c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0321: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("DragByAngle.Init"))
			{
				directionIncrements = angleIncrements;
				dragByDirectionOfTravel = new float[360 / directionIncrements];
				List<BoatBuildingBlock> list = Pool.Get<List<BoatBuildingBlock>>();
				foreach (BoatBuildingBlock item in boat.BoatBuildingBlocks.Cached)
				{
					if (Object.op_Implicit((Object)(object)item) && item.Hull)
					{
						list.Add(item);
					}
				}
				if (list.Count == 0)
				{
					Pool.FreeUnmanaged<BoatBuildingBlock>(ref list);
					return;
				}
				Bounds val = new Bounds(((Component)list[0]).transform.localPosition, ((Bounds)(ref list[0].bounds)).size);
				Vector3 val2 = default(Vector3);
				Quaternion val3 = default(Quaternion);
				OBB val4 = default(OBB);
				for (int i = 1; i < list.Count; i++)
				{
					((Component)list[i]).transform.GetLocalPositionAndRotation(ref val2, ref val3);
					((OBB)(ref val4))._002Ector(val2, val3, list[i].bounds);
					((OBB)(ref val4)).BoundsEncapsulate(ref val);
				}
				OBB bounds = new OBB(((Component)boat).transform, val);
				Vector3 forward = ((Component)boat).transform.forward;
				JobHandle val5 = default(JobHandle);
				List<NativeList<SpherecastCommand>> list2 = Pool.Get<List<NativeList<SpherecastCommand>>>();
				List<NativeArray<RaycastHit>> list3 = Pool.Get<List<NativeArray<RaycastHit>>>();
				List<Plane> list4 = Pool.Get<List<Plane>>();
				List<float> list5 = Pool.Get<List<float>>();
				Plane val8 = default(Plane);
				for (int j = 0; j < dragByDirectionOfTravel.Length; j++)
				{
					Vector3 val6 = -(Quaternion.Euler(0f, (float)(j * directionIncrements), 0f) * forward);
					float num = ExtentInDirXZ(-val6);
					Vector3 val7 = bounds.position + Vector3Ex.WithY(-val6 * (num + 5f), bounds.position.y);
					((Plane)(ref val8))._002Ector(val6, val7);
					val5 = JobHandle.CombineDependencies(val5, SchedulePlaneProjection(val8, val7, bounds, out var commands, out var hits, out var step));
					list2.Add(commands);
					list3.Add(hits);
					list4.Add(val8);
					list5.Add(step);
				}
				((JobHandle)(ref val5)).Complete();
				val5 = default(JobHandle);
				for (int k = 0; k < list2.Count; k++)
				{
					GamePhysics.VerifySpheres(list3[k], list2[k].AsArray(), 24);
					val5 = JobHandle.CombineDependencies(val5, GamePhysics.SortDeferred(list3[k], list2[k].Length, 24));
				}
				((JobHandle)(ref val5)).Complete();
				for (int l = 0; l < list2.Count; l++)
				{
					dragByDirectionOfTravel[l] = GetDragFromHits(list2[l].AsArray(), list3[l], list4[l], list5[l]);
					list2[l].Dispose();
					list3[l].Dispose();
				}
				Pool.FreeUnmanaged<BoatBuildingBlock>(ref list);
				Pool.FreeUnmanaged<NativeList<SpherecastCommand>>(ref list2);
				Pool.FreeUnmanaged<NativeArray<RaycastHit>>(ref list3);
				Pool.FreeUnmanaged<Plane>(ref list4);
				Pool.FreeUnmanaged<float>(ref list5);
				float ExtentInDirXZ(Vector3 dir)
				{
					//IL_0000: Unknown result type (might be due to invalid IL or missing references)
					//IL_0007: Unknown result type (might be due to invalid IL or missing references)
					//IL_0011: Unknown result type (might be due to invalid IL or missing references)
					//IL_0016: Unknown result type (might be due to invalid IL or missing references)
					//IL_0019: Unknown result type (might be due to invalid IL or missing references)
					//IL_0039: Unknown result type (might be due to invalid IL or missing references)
					//IL_0040: Unknown result type (might be due to invalid IL or missing references)
					//IL_004a: Unknown result type (might be due to invalid IL or missing references)
					//IL_004f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0052: Unknown result type (might be due to invalid IL or missing references)
					Vector3 val9 = Vector3Extensions.XZ(bounds.right, 0f);
					float num2 = Mathf.Abs(Vector3.Dot(dir, ((Vector3)(ref val9)).normalized)) * bounds.extents.x;
					val9 = Vector3Extensions.XZ(bounds.forward, 0f);
					return num2 + Mathf.Abs(Vector3.Dot(dir, ((Vector3)(ref val9)).normalized)) * bounds.extents.z;
				}
			}
		}

		private JobHandle SchedulePlaneProjection(Plane plane, Vector3 planePoint, OBB worldBounds, out NativeList<SpherecastCommand> commands, out NativeArray<RaycastHit> hits, out float step)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("SchedulePlaneProjection"))
			{
				Vector3 normal = ((Plane)(ref plane)).normal;
				Vector3 val = Vector3.Cross(Vector3.up, normal);
				Vector3 normalized = ((Vector3)(ref val)).normalized;
				float num = float.MaxValue;
				float num2 = float.MinValue;
				for (int i = -1; i <= 1; i += 2)
				{
					for (int j = -1; j <= 1; j += 2)
					{
						for (int k = -1; k <= 1; k += 2)
						{
							Vector3 val2 = Vector3.Scale(new Vector3((float)i, (float)j, (float)k), worldBounds.extents);
							float num3 = Vector3.Dot(worldBounds.position + worldBounds.rotation * val2 - worldBounds.position, normalized);
							if (num3 < num)
							{
								num = num3;
							}
							if (num3 > num2)
							{
								num2 = num3;
							}
						}
					}
				}
				commands = new NativeList<SpherecastCommand>(16, AllocatorHandle.op_Implicit((Allocator)3));
				QueryParameters val3 = new QueryParameters(134217728, false, (QueryTriggerInteraction)1, false);
				float num4 = num2 - num;
				step = num4 / 16f;
				int num5 = 0;
				SpherecastCommand val4 = default(SpherecastCommand);
				for (float num6 = num; num6 <= num2; num6 += step)
				{
					if (num5 >= 16)
					{
						break;
					}
					((SpherecastCommand)(ref val4))._002Ector(planePoint + normalized * num6, step * 0.5f, normal, val3, Vector3Ex.Max(worldBounds.extents) * 3f);
					num5++;
					commands.Add(ref val4);
				}
				hits = new NativeArray<RaycastHit>(commands.Length * 24, (Allocator)3, (NativeArrayOptions)1);
				return SpherecastCommand.ScheduleBatch(NativeList<SpherecastCommand>.op_Implicit(commands), hits, 4, 24, default(JobHandle));
			}
		}

		private float GetDragFromHits(NativeArray<SpherecastCommand> commands, NativeArray<RaycastHit> hits, Plane plane, float step)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("GetDragFromHits"))
			{
				float num = 0f;
				for (int i = 0; i < commands.Length; i++)
				{
					int num2 = i * 24;
					for (int j = num2; j < num2 + 24; j++)
					{
						RaycastHit hit = hits[j];
						if (RaycastHitEx.GetEntity(hit) is BoatBuildingBlock { Hull: not false } boatBuildingBlock && (Object)(object)boatBuildingBlock.GetParentEntity() == (Object)(object)boat)
						{
							float num3 = Vector3.Dot(((RaycastHit)(ref hit)).normal, -((Plane)(ref plane)).normal) * step;
							num += num3;
							break;
						}
					}
				}
				return Mathf.Lerp(DragByAngle_MinDrag, DragByAngle_MaxDrag, Mathf.Pow(Mathf.InverseLerp(DragByAngle_MinContrib, DragByAngle_MaxContrib, num), DragByAngle_Exponent));
			}
		}

		public float GetDrag(Transform t, Rigidbody r)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			if (dragByDirectionOfTravel == null)
			{
				return 1f;
			}
			Matrix4x4 localToWorldMatrix = t.localToWorldMatrix;
			Vector3 val = ((Matrix4x4)(ref localToWorldMatrix)).MultiplyVector(Vector3.forward);
			Vector3 val2 = ((Matrix4x4)(ref localToWorldMatrix)).MultiplyVector(Vector3.up);
			Vector3 linearVelocity = r.linearVelocity;
			float num = Vector3.SignedAngle(val, linearVelocity, val2);
			if (num < 0f)
			{
				num += 360f;
			}
			int num2 = Mathf.FloorToInt(num / (float)directionIncrements) % dragByDirectionOfTravel.Length;
			int num3 = Mathf.CeilToInt(num / (float)directionIncrements) % dragByDirectionOfTravel.Length;
			bool flag = num2 > num3;
			float num4 = Mathf.InverseLerp((float)(num2 * directionIncrements), (float)(num3 * directionIncrements) + (flag ? 360f : 0f), num);
			return Mathf.Lerp(dragByDirectionOfTravel[num2], dragByDirectionOfTravel[num3], num4);
		}

		public void BuildDragDebugTextTable(TextTable table)
		{
			table.AddColumn("Angle");
			table.AddColumn("Drag");
			for (int i = 0; i < dragByDirectionOfTravel.Length; i++)
			{
				int num = i * directionIncrements;
				float num2 = dragByDirectionOfTravel[i];
				table.AddRow(new string[2]
				{
					num.ToString(),
					num2.ToString()
				});
			}
		}

		public void DDrawForPlayer(BasePlayer p)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)boat))
			{
				Vector3 val = ((Component)boat).transform.position + Vector3.up * 2f;
				Vector3 forward = ((Component)boat).transform.forward;
				for (int i = 0; i < dragByDirectionOfTravel.Length; i++)
				{
					int num = i * directionIncrements;
					float num2 = dragByDirectionOfTravel[i];
					Vector3 val2 = Quaternion.Euler(0f, (float)num, 0f) * forward;
					Vector3 val3 = val + val2 * 5f;
					DDraw.Arrow(p, val, val3, Color.yellow, 30f, 0.5f, distanceFade: true, zTest: false);
					DDraw.Text(p, val3 + Vector3.up * 1f, $"{num}°: {num2:F4}", Color.yellow, 30f);
				}
			}
		}
	}

	[Header("Effects")]
	public Transform boatRear;

	public ParticleSystemContainer wakeEffect;

	[ServerVar(Help = "(Generated) Duration in seconds after entering the deep sea zone that a player boat has before its engine is powered down")]
	public static float DeepSeaTransitionPowerDownGraceDuration;

	[ServerVar(Help = "(Generated) When enabled, player boat engines are powered down when no players are aboard; prevents runaway unmanned boats")]
	public static bool PowerdownOnNoPlayers;

	[ServerVar(Help = "When enabled, deployables on boats send immediate network updates when orphaned during edit mode to prevent looping sounds from being killed")]
	public static bool OrphanSendImmediate;

	[ServerVar(Help = "(Generated) Interval in seconds between checks to determine whether any players are still aboard the boat")]
	public static float AboardPlayerCheckInterval;

	[ServerVar(Help = "(Generated) Time in seconds after a boat is anchored before it becomes eligible for shore drift; default 21600s (6 hours)")]
	public static float AnchoredDriftDelaySeconds;

	[ServerVar(Help = "0 - 1")]
	public static float SailPositionInfluence;

	[ServerVar(Help = "0 - 1")]
	public static float EnginePositionInfluences;

	[ServerVar(Help = "(Generated) Maximum angle in degrees from vertical at which building blocks can be placed on a player boat; default 30 degrees")]
	public static float PlacementUpThreshold;

	[ServerVar]
	[Help("How long until player boat corpses despawn")]
	public static float corpseseconds;

	[ServerVar(Help = "How long before a boat loses all its health while outside")]
	public static float decayminutes;

	[ServerVar(Help = "How long until decay begins after the boat was last used")]
	public static float decaystartdelayminutes;

	public static float CannonHitSlowdownMultiplier;

	private bool cachedPlayersAboard;

	private float approxTimestampNoPlayersAboard = float.NegativeInfinity;

	private List<TriggerParent> parentingTriggers = new List<TriggerParent>();

	private TimeUntil nextBeachedBoatCheck;

	private TimeSince timeBeached;

	private bool isBeached;

	private TimeCachedValue<float> desiredDrag;

	private TimeCachedValue<float> adjustedVelocityMax;

	private float calculatedMass;

	private TimeSince timeSinceLastUsed;

	private TimeSince timeSinceLastCannonAttack;

	private const float DECAY_TICK_TIME = 60f;

	private List<BaseEntity> entitiesToDestroyOnDeath = new List<BaseEntity>();

	private List<IOEntity> ioEntitiesToDisconnectOnDeath = new List<IOEntity>();

	private float powerDownGraceExpireTimestamp;

	public const string ACHIEVEMENT_HIT_BY_CANNON_NAME = "BOAT_CANNON_HIT";

	private DragByAngle dragByAngle;

	[ServerVar]
	public static float DragByAngle_MinDrag;

	[ServerVar]
	public static float DragByAngle_MaxDrag;

	[ServerVar]
	public static float DragByAngle_MinContrib;

	[ServerVar]
	public static float DragByAngle_MaxContrib;

	[ServerVar]
	public static float DragByAngle_Exponent;

	public const Flags Flag_DestructibleWreck = Flags.Reserved18;

	public static Phrase tooFastToEditErrorPhrase;

	public static Phrase recentlyDamagedCantEditPhrase;

	public static Phrase onDeployEditCoolDownPhrase;

	public static Phrase invalidDeployLocationPhrase;

	public ItemDefinition BoatBuildingStationItem;

	public GameObjectRef BoatBuildingStationPrefab;

	public List<BaseEntity> ChildrenToDestroyOnDeath;

	public GameObjectRef sinkEffect;

	[HideInInspector]
	public List<ItemAmount> DynamicBuildCost = new List<ItemAmount>();

	[ReplicatedVar]
	public static bool EditEnabled;

	[ReplicatedVar]
	public static bool FinishEditingEnabled;

	[ReplicatedVar]
	public static bool HammerRepairEnabled;

	[ReplicatedVar]
	public static int MaxBlockCount;

	[ReplicatedVar]
	public static int MaxDeployableCount;

	[ReplicatedVar]
	public static bool DestructibleWrecksEnabled;

	[ReplicatedVar]
	public static bool UseDestructibleWreckStability;

	[Header("Player Boat")]
	public float MaxEditVelocity = 2f;

	public float DeathSinkRate = 0.05f;

	public AnimationCurve AimSwayVelocityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve AimSwayClarityClampVelocityCurve = AnimationCurve.Linear(0f, 0f, 15f, 1f);

	public AnimationCurve CannonAttackSlowdownCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public const Flags Flag_Dying = Flags.Broken;

	[ReplicatedVar]
	public static float VelocityMax;

	public CachedBoatParts<BoatBuildingBlock> BoatBuildingBlocks = new CachedBoatParts<BoatBuildingBlock>();

	public CachedBoatParts<BaseEntity> Deployables = new CachedBoatParts<BaseEntity>();

	private CachedBoatParts<Sail> Sails = new CachedBoatParts<Sail>();

	private CachedBoatParts<SmallEngine> Engines = new CachedBoatParts<SmallEngine>();

	private CachedBoatParts<Anchor> Anchors = new CachedBoatParts<Anchor>();

	private CachedBoatParts<SteeringWheel> SteeringWheels = new CachedBoatParts<SteeringWheel>();

	public ProtectionProperties ParentedBoatDeployableProtection;

	public PlayerBoatSounds playerBoatSounds;

	public Vector3 lastEditLocalPos;

	public Vector3 lastEditLocalRot;

	[ServerVar(Help = "(Generated) When enabled, draws debug visualisations for player boat state including drift target, shore direction, and power zones")]
	public static bool DebugVis { get; set; }

	protected override bool AllowKinematicDrift => true;

	public float boatSpawnTime { get; set; }

	public float TotalThrust => SailThrust + EngineThrust;

	public float SailThrust
	{
		get
		{
			float num = 0f;
			foreach (Sail item in Sails.Cached)
			{
				if (!((Object)(object)item == (Object)null))
				{
					num += item.CurrentThrust;
				}
			}
			return num;
		}
	}

	public float EngineThrust
	{
		get
		{
			float num = 0f;
			foreach (SmallEngine item in Engines.Cached)
			{
				if (!((Object)(object)item == (Object)null))
				{
					num += item.CurrentThrust;
				}
			}
			return num;
		}
	}

	public DragByAngle Tests_DragByAngle => dragByAngle;

	public bool KilledForEditMode { get; set; }

	public bool Anchored { get; private set; }

	public bool IsDying => HasFlag(Flags.Broken);

	public bool IsDestructibleWreck
	{
		get
		{
			if (DestructibleWrecksEnabled)
			{
				return HasFlag(Flags.Reserved18);
			}
			return false;
		}
	}

	public override VehiclePrivilege GetChildPrivilege()
	{
		foreach (SteeringWheel item in SteeringWheels.Cached)
		{
			if (!((Object)(object)item == (Object)null))
			{
				return item.Privilege;
			}
		}
		return null;
	}

	public override bool IsAuthedForBuilding(BasePlayer player)
	{
		return IsPlayerAuthed(player, authedIfNoPriveOrLock: true);
	}

	public static bool IsPlayerAuthedOnChildEntity(BaseEntity entity, BasePlayer player, bool authedIfNoPrivOrLock)
	{
		if ((Object)(object)entity == (Object)null)
		{
			return false;
		}
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		PlayerBoat parentPlayerBoat = GetParentPlayerBoat(entity);
		if ((Object)(object)parentPlayerBoat != (Object)null)
		{
			return parentPlayerBoat.IsPlayerAuthed(player, authedIfNoPrivOrLock);
		}
		return false;
	}

	public bool IsPlayerAuthed(BasePlayer player, bool authedIfNoPriveOrLock)
	{
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		PlayerBoatPrivilege playerBoatPrivilege = null;
		bool flag = false;
		SteeringWheel steeringWheel = GetSteeringWheel();
		if ((Object)(object)steeringWheel != (Object)null)
		{
			playerBoatPrivilege = steeringWheel.Privilege;
			flag = steeringWheel.BoatLock.HasALock;
		}
		if ((Object)(object)playerBoatPrivilege == (Object)null || !flag)
		{
			return authedIfNoPriveOrLock;
		}
		return playerBoatPrivilege.IsAuthed(player);
	}

	public void Init(List<BoatBuildingBlock> blocks, List<BaseEntity> ents, Vector3 halfExtents, bool loading)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		rigidBody.isKinematic = true;
		bool autoSyncTransforms = Physics.autoSyncTransforms;
		try
		{
			Physics.autoSyncTransforms = false;
			if (!loading)
			{
				ParentChildBlocks(blocks);
				ParentChildEntities(ents);
			}
			ParentBlockTriggers(blocks);
		}
		finally
		{
			if (autoSyncTransforms)
			{
				Physics.SyncTransforms();
			}
			Physics.autoSyncTransforms = autoSyncTransforms;
		}
		rigidBody.isKinematic = false;
		Vector3 dimensions = halfExtents * 2f;
		dimensions.y += 6f;
		SetDimensions(dimensions);
		CalculateHealth(loading);
		CalculateRepairCost();
		CalculateMass();
		CalculateBuoyancy();
		SwitchToVehicle(loading);
		SetShouldTriggersParentSwimmers(shouldParentSwimmers: true);
		SetTriggersMovementMask(Axis.XZ);
		if (loading)
		{
			OnEntitiesAddedToBoat();
			rigidBody.isKinematic = false;
		}
		OnAnchoredChanged();
		boatSpawnTime = Time.time;
	}

	public override void ServerInit()
	{
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		desiredDrag = new TimeCachedValue<float>
		{
			refreshCooldown = 1f,
			refreshRandomRange = 0.5f,
			updateValue = CalculateDesiredDrag
		};
		adjustedVelocityMax = new TimeCachedValue<float>
		{
			refreshCooldown = 1.2f,
			refreshRandomRange = 0.2f,
			updateValue = CalculateAdjustedMaxVelocity
		};
		InvokeRandomized(CheckForPlayersAboard, 0f, AboardPlayerCheckInterval, AboardPlayerCheckInterval * 0.1f);
		dragByAngle.boat = this;
		Invoke(BakeDragAngles, 0f);
		ResetTimeSinceUsed();
		InvokeRandomized(BoatDecay, Random.Range(30f, 60f), 60f, 6f);
		Invoke(StartDeployAndEditCoolDown, 0f);
		timeSinceLastCannonAttack = TimeSince.op_Implicit(100f);
	}

	private void StartDeployAndEditCoolDown()
	{
		BoatBuildingStation.StartGlobalEditFinishCoolDown();
		if (IsInvoking(ClearDeployAndEditCoolDown))
		{
			CancelInvoke(ClearDeployAndEditCoolDown);
		}
		SetFlagLocal(Flags.Busy, b: true);
		SendNetworkUpdateImmediate();
		Invoke(ClearDeployAndEditCoolDown, BoatBuildingStation.EditFinishUseInterval);
	}

	private void ClearDeployAndEditCoolDown()
	{
		SetFlagLocal(Flags.Busy, b: false);
		SendNetworkUpdateImmediate();
	}

	private void BakeDragAngles()
	{
		dragByAngle.Init(45);
	}

	public override void Save(SaveInfo info)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.playerBoat = Pool.Get<PlayerBoat>();
		info.msg.playerBoat.size = ((Bounds)(ref bounds)).size;
		info.msg.playerBoat.lastEditLocalPos = lastEditLocalPos;
		info.msg.playerBoat.lastEditLocalRot = lastEditLocalRot;
	}

	public override void PostServerLoad()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		base.PostServerLoad();
		if (!base.isServer)
		{
			return;
		}
		if (base.health <= 0f || HasFlag(Flags.Broken))
		{
			Kill();
			return;
		}
		BoatBuildingStation.GetBoatBlocksOBBExtents(BoatBuildingBlocks.Cached, ((Component)this).transform.forward, out var _, out var halfExtents, out var _);
		if (!IsDying)
		{
			((Component)this).transform.position = Vector3Ex.WithY(((Component)this).transform.position, 0f);
			((Component)this).transform.localEulerAngles = new Vector3(0f, ((Component)this).transform.localEulerAngles.y, 0f);
		}
		Init(BoatBuildingBlocks.Cached, null, halfExtents, loading: true);
	}

	public void ResetTimeSinceUsed()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		timeSinceLastUsed = TimeSince.op_Implicit(0f);
	}

	public bool VerifyDeployablePlacement(BasePlayer player)
	{
		if (MaxDeployableCount > 0 && Deployables.Cached.Count >= MaxDeployableCount)
		{
			player.ShowToast(GameTip.Styles.Error, BoatBuildingStation.invalidTooManyDeployablesPhrase, false);
			return false;
		}
		if (!HasFlag(Flags.Broken) && !IsWithinDegreesOfUp(PlacementUpThreshold))
		{
			player.ShowToast(GameTip.Styles.Error, BoatBuildingStation.invalidIllegalPlacement, false);
			return false;
		}
		return true;
	}

	private void CheckForPlayersAboard()
	{
		bool flag = cachedPlayersAboard;
		cachedPlayersAboard = AnyPlayersOnBoat();
		if (Time.time < powerDownGraceExpireTimestamp)
		{
			cachedPlayersAboard = flag;
		}
		if (flag != cachedPlayersAboard)
		{
			if (cachedPlayersAboard)
			{
				OnPlayersAboard();
			}
			else
			{
				OnNoPlayersAboard();
			}
		}
	}

	private void OnPlayersAboard()
	{
		approxTimestampNoPlayersAboard = 0f;
	}

	private void OnNoPlayersAboard()
	{
		PowerDown();
		approxTimestampNoPlayersAboard = Time.realtimeSinceStartup;
	}

	public void PowerDown(bool force = false)
	{
		if (PowerdownOnNoPlayers || force)
		{
			SetAllSailsOpen(flag: false);
			SetAllEnginesOn(flag: false);
			desiredDrag.ForceNextRun();
			ResetSteering();
		}
	}

	public void OnCreatedAtBBS(BoatBuildingStation bbs)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		lastEditLocalPos = Quaternion.Inverse(((Component)this).transform.rotation) * (((Component)bbs).transform.position - ((Component)this).transform.position);
		Quaternion val = Quaternion.Inverse(((Component)this).transform.rotation) * ((Component)bbs).transform.rotation;
		lastEditLocalRot = ((Quaternion)(ref val)).eulerAngles;
	}

	public bool DeployAndEdit(BasePlayer player)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (!CanDeployAndEdit(player, out var outPosition, out var outRotation, checkForCoolDown: true, checkForItem: true, checkDeploy: true, sendErrorToasts: true))
		{
			return false;
		}
		BoatBuildingStation obj = GameManager.server.CreateEntity(BoatBuildingStationPrefab.resourcePath, outPosition, outRotation) as BoatBuildingStation;
		obj.Spawn();
		obj.SendNetworkUpdate();
		obj.OnPlaced(player);
		Deployable component = BoatBuildingStationPrefab.Get().GetComponent<Deployable>();
		if (component != null && component.placeEffect.isValid)
		{
			Effect.server.Run(component.placeEffect.resourcePath, outPosition, Vector3.up);
		}
		player.inventory.Take(null, BoatBuildingStationItem.itemid, 1);
		player.Command("note.inv", BoatBuildingStationItem.itemid, -1);
		return true;
	}

	public void ParentChildBlocks(List<BoatBuildingBlock> blocks)
	{
		foreach (BoatBuildingBlock block in blocks)
		{
			block.SetParent(this, worldPositionStays: true);
		}
	}

	public void ParentBlockTriggers(List<BoatBuildingBlock> blocks)
	{
		parentingTriggers.Clear();
		List<Transform> list = Pool.Get<List<Transform>>();
		foreach (BoatBuildingBlock block in blocks)
		{
			if (!block.ProvidesParentingTrigger)
			{
				continue;
			}
			List<TriggerParent> collection = block.SetTriggerParent(this);
			parentingTriggers.AddRange(collection);
			Transform[] dismountPoints = block.DismountPoints;
			if (dismountPoints == null || dismountPoints.Length == 0)
			{
				continue;
			}
			for (int i = 0; i < block.DismountPoints.Length; i++)
			{
				if (Object.op_Implicit((Object)(object)block.DismountPoints[i]))
				{
					list.Add(block.DismountPoints[i]);
				}
			}
		}
		dismountPositions = list.ToArray();
		Pool.FreeUnmanaged<Transform>(ref list);
	}

	private void SetShouldTriggersParentSwimmers(bool shouldParentSwimmers)
	{
		foreach (TriggerParent parentingTrigger in parentingTriggers)
		{
			parentingTrigger.parentSwimmers = shouldParentSwimmers;
		}
	}

	private void SetTriggersMovementMask(Axis movementMask)
	{
		foreach (TriggerParent parentingTrigger in parentingTriggers)
		{
			parentingTrigger.TriggerMovementMask = movementMask;
		}
	}

	public void ParentChildEntities(List<BaseEntity> ents)
	{
		foreach (BaseEntity ent in ents)
		{
			ent.SetParent(this, worldPositionStays: true);
		}
		OnEntitiesAddedToBoat();
	}

	private void OnEntitiesAddedToBoat()
	{
		foreach (BaseEntity child in children)
		{
			if (child is global::IBoatBuildingPiece boatBuildingPiece)
			{
				boatBuildingPiece.OnAddedToBoat(this);
			}
		}
		ListenServerColliderFix();
	}

	public void DistributeHealthAcrossBlocks()
	{
		List<BoatBuildingBlock> cached = BoatBuildingBlocks.Cached;
		float num = 0f;
		float num2 = 0f;
		float num3 = base.health;
		foreach (BoatBuildingBlock item in cached)
		{
			item.SendNetworkUpdateOnHealthChanged = false;
			item.DecayTouch();
			if (!(item.damageTaken <= 0f))
			{
				num += item.damageTaken;
				float num4 = Mathf.Max(item.health - item.damageTaken, 1f);
				float num5 = Mathf.Abs(item.health - num4);
				item.health = num4;
				num2 += num5;
			}
		}
		float num6 = 0f;
		foreach (BoatBuildingBlock item2 in cached)
		{
			if (num2 >= num)
			{
				num6 += item2.health;
				continue;
			}
			float num7 = Mathf.Min(item2.health - 1f, num - num2);
			item2.health -= num7;
			num6 += item2.health;
			num2 += num7;
		}
		float num8 = Mathf.Clamp(num3 - num6, 0f, num3);
		foreach (BoatBuildingBlock item3 in cached)
		{
			float num9 = Mathf.Min(num8, item3.MaxHealth() - item3.health);
			if (num9 > 0f)
			{
				item3.health += num9;
				num8 -= num9;
			}
			item3.SendNetworkUpdateOnHealthChanged = true;
		}
	}

	public void OrphanChildEntities(bool boatBlocksOnly = false)
	{
		for (int num = children.Count - 1; num >= 0; num--)
		{
			BaseEntity baseEntity = children[num];
			if (!boatBlocksOnly || !((Object)(object)(baseEntity as BoatBuildingBlock) == (Object)null))
			{
				bool sendImmediate = baseEntity is BasePlayer || baseEntity is PartyBalloon || (OrphanSendImmediate && !(baseEntity is BoatBuildingBlock));
				baseEntity.SetParent(null, worldPositionStays: true, sendImmediate);
			}
		}
	}

	[ServerVar(ClientAdmin = true, Help = "(Generated) Prints drag force debug data based on the angle between the boat heading and player look direction; admin-only")]
	public static void LookAtDragByAngle(ConsoleSystem.Arg arg)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			BaseNetworkable baseNetworkable = GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, basePlayer.eyes.HeadRay(), 0f, 5f, -5, (QueryTriggerInteraction)0);
			if ((Object)(object)baseNetworkable == (Object)null || !(baseNetworkable.GetRootParentEntity() is PlayerBoat playerBoat))
			{
				arg.ReplyWith("Not looking at boat");
				return;
			}
			TextTable val = Pool.Get<TextTable>();
			playerBoat.dragByAngle.BuildDragDebugTextTable(val);
			playerBoat.dragByAngle.DDrawForPlayer(basePlayer);
			arg.ReplyWith(((object)val).ToString());
			Pool.Free<TextTable>(ref val);
		}
	}

	private float CalculateAdjustedMaxVelocity()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("CalculateAdjustedMaxVelocity"))
		{
			float num = float.MaxValue;
			NativeArray<Vector3> val = new NativeArray<Vector3>(3, (Allocator)3, (NativeArrayOptions)1);
			NativeArray<int> val2 = new NativeArray<int>(3, (Allocator)3, (NativeArrayOptions)1);
			NativeArray<float> results = new NativeArray<float>(3, (Allocator)3, (NativeArrayOptions)1);
			for (int i = 0; i < 3; i++)
			{
				val2[i] = i;
				Vector3 pos = (val[i] = planeFitPoints[i].position);
				num = Mathf.Min(num, WaterLevel.GetOverallWaterDepth(pos, waves: false, volumes: false));
			}
			TerrainMeta.Texturing.GetCoarseDistancesToShoreIndirect(val.AsReadOnly(), val2.AsReadOnly(), results);
			float num2 = float.MaxValue;
			for (int j = 0; j < results.Length; j++)
			{
				num2 = Mathf.Min(num2, results[j]);
			}
			val.Dispose(default(JobHandle));
			val2.Dispose(default(JobHandle));
			results.Dispose(default(JobHandle));
			float num3 = Mathf.Lerp(0.3f, 1f, Mathf.InverseLerp(20f, 50f, num2));
			float num4 = Mathf.Lerp(0.3f, 1f, Mathf.InverseLerp(3f, 7f, num));
			return VelocityMax * Mathf.Min(num3, num4);
		}
	}

	private float CalculateDesiredDrag()
	{
		using (TimeWarning.New("CalculateDesiredDrag"))
		{
			if (!(Mathf.Abs(TotalThrust) > Mathf.Epsilon))
			{
				return 1.5f;
			}
			return dragByAngle.GetDrag(((Component)this).transform, rigidBody);
		}
	}

	public float CurrentThrust()
	{
		if (Anchored)
		{
			return 0f;
		}
		return TotalThrust;
	}

	public void CalculateHealth(bool loading)
	{
		float num = 0f;
		float num2 = 0f;
		foreach (BoatBuildingBlock item in BoatBuildingBlocks.Cached)
		{
			if (!((Object)(object)item == (Object)null))
			{
				num2 += item.MaxHealth();
				num += item.health;
			}
		}
		OverrideMaxHealth(num2);
		if (!loading)
		{
			SetHealth(num);
		}
	}

	public void CalculateRepairCost()
	{
		if (BoatBuildingBlocks.Cached == null || BoatBuildingBlocks.Cached.Count == 0)
		{
			return;
		}
		DynamicBuildCost.Clear();
		int num = 0;
		int num2 = 0;
		List<ItemAmount> list = null;
		List<ItemAmount> list2 = null;
		foreach (BoatBuildingBlock item in BoatBuildingBlocks.Cached)
		{
			if ((Object)(object)item == (Object)null)
			{
				continue;
			}
			if (item.Hull)
			{
				if (list == null)
				{
					list = item.BuildCost().Items;
				}
				num++;
			}
			else
			{
				if (list2 == null)
				{
					list2 = item.BuildCost().Items;
				}
				num2++;
			}
		}
		AddBlockCosts(list, num, ref DynamicBuildCost);
		AddBlockCosts(list2, num2, ref DynamicBuildCost);
	}

	private void AddBlockCosts(List<ItemAmount> costs, int blockCount, ref List<ItemAmount> dynamicBuildCost)
	{
		if (costs == null || blockCount <= 0)
		{
			return;
		}
		foreach (ItemAmount cost in costs)
		{
			if (cost == null || (Object)(object)cost.itemDef == (Object)null)
			{
				continue;
			}
			float num = cost.amount * (float)blockCount;
			bool flag = false;
			for (int i = 0; i < dynamicBuildCost.Count; i++)
			{
				ItemAmount itemAmount = dynamicBuildCost[i];
				if ((Object)(object)itemAmount.itemDef == (Object)(object)cost.itemDef)
				{
					itemAmount.amount += num;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				dynamicBuildCost.Add(new ItemAmount(cost.itemDef, num));
			}
		}
	}

	public override float RepairCostFraction()
	{
		return 1f;
	}

	public void CalculateMass()
	{
		float num = 0f;
		int num2 = 0;
		foreach (BoatBuildingBlock item in BoatBuildingBlocks.Cached)
		{
			if (!((Object)(object)item == (Object)null))
			{
				num += item.ContributingMass;
				num2++;
			}
		}
		num = Mathf.Max(num, 1000f);
		rigidBody.mass = num;
		calculatedMass = num;
	}

	public void CalculateBuoyancy()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		buoyancy.scaleForceWithMass = true;
		Vector3 forward = ((Component)this).transform.forward;
		Vector3 right = ((Component)this).transform.right;
		Vector3 position = ((Component)this).transform.position;
		float num = ((Bounds)(ref bounds)).size.x / 2f;
		float num2 = ((Bounds)(ref bounds)).size.z / 2f;
		List<Vector3> list = Pool.Get<List<Vector3>>();
		list.Add(position + forward * num2);
		list.Add(position + forward * num2 + right * num);
		list.Add(position + forward * num2 + -right * num);
		list.Add(position);
		list.Add(position + right * num);
		list.Add(position + -right * num);
		list.Add(position + -forward * num2);
		list.Add(position + -forward * num2 + right * num);
		list.Add(position + -forward * num2 + -right * num);
		buoyancy.SetBuoyancyPointLocations(list);
		buoyancy.SavePointData(forced: true);
		Pool.FreeUnmanaged<Vector3>(ref list);
		Transform[] array = planeFitPoints;
		if (array != null && array.Length >= 3)
		{
			planeFitPoints[0].position = position + forward * num2;
			planeFitPoints[1].position = position + -forward * num2 + right * num;
			planeFitPoints[2].position = position + -forward * num2 + -right * num;
		}
	}

	public override bool EngineOn()
	{
		if (!(CurrentThrust() > 0f))
		{
			return !RudderInDeadzone();
		}
		return true;
	}

	private bool RudderInDeadzone()
	{
		return Mathf.Abs(steering) < 0.02f;
	}

	public override bool EngineOnEligible()
	{
		if (EngineOn() && !IsFlipped())
		{
			return base.healthFraction > 0f;
		}
		return false;
	}

	public override void VehicleFixedUpdate()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("PlayerBoat.VehicleFixedUpdate"))
		{
			UpdateBuoyancy();
			Vector3 linearVelocity = rigidBody.linearVelocity;
			bool flag = ((Vector3)(ref linearVelocity)).magnitude > AntiHackVelocity();
			if (EngineOn() && !Anchored)
			{
				bool reversing = false;
				if (!flag)
				{
					CalcSailsForces(out var accumForce, out var accumTorque);
					CalcEnginesForces(out var accumForce2, out var accumTorque2);
					Vector3 accumulatedForce = accumForce + accumForce2;
					Vector3 val = accumTorque + accumTorque2;
					float num = 13f;
					float num2 = 1f;
					if (((Vector3)(ref accumulatedForce)).magnitude > 0.1f)
					{
						float mass = rigidBody.mass;
						Vector3 val2 = accumulatedForce / mass;
						Vector3 val3 = accumulatedForce;
						if (((Vector3)(ref val2)).sqrMagnitude > num * num)
						{
							accumulatedForce = ((Vector3)(ref accumulatedForce)).normalized * (mass * num);
						}
						num2 = ((Vector3)(ref accumulatedForce)).sqrMagnitude / ((Vector3)(ref val3)).sqrMagnitude;
						HandleCannonAttackSlowdown(ref accumulatedForce);
						rigidBody.AddForce(accumulatedForce, (ForceMode)0);
						if (Vector3.Dot(accumulatedForce, ((Component)this).transform.forward) <= 0f)
						{
							reversing = true;
						}
					}
					if (val != Vector3.zero)
					{
						rigidBody.AddTorque(val * num2, (ForceMode)0);
					}
				}
				RudderTorque(reversing);
				gasPedal = 0f;
			}
			base.VehicleFixedUpdate();
			HandleBeachedPushing();
			rigidBody.linearDamping = desiredDrag.Get(force: false);
			if (flag)
			{
				Rigidbody obj = rigidBody;
				linearVelocity = rigidBody.linearVelocity;
				obj.linearVelocity = ((Vector3)(ref linearVelocity)).normalized * AntiHackVelocity();
			}
		}
	}

	public override bool AnyMounted()
	{
		return HasDriver();
	}

	private void HandleCannonAttackSlowdown(ref Vector3 accumulatedForce)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		float num = CannonAttackSlowdownCurve.Evaluate(TimeSince.op_Implicit(timeSinceLastCannonAttack));
		num = Mathf.Lerp(1f, num, CannonHitSlowdownMultiplier);
		accumulatedForce *= num;
	}

	public override void DismountAllPlayers()
	{
		base.DismountAllPlayers();
		List<SteeringWheel> cached = SteeringWheels.Cached;
		for (int i = 0; i < cached.Count; i++)
		{
			if (Object.op_Implicit((Object)(object)cached[i]))
			{
				cached[i].DismountAllPlayers();
			}
		}
	}

	private void UpdateBuoyancy()
	{
		if (IsDying)
		{
			buoyancy.buoyancyScale = Mathf.Lerp(buoyancy.buoyancyScale, 0f, Time.fixedDeltaTime * DeathSinkRate);
		}
	}

	private void HandleBeachedPushing()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("PlayerBoat.HandleBeachedPushing"))
		{
			if (Anchored)
			{
				isBeached = false;
				return;
			}
			if (TimeUntil.op_Implicit(nextBeachedBoatCheck) <= 0f)
			{
				bool flag = isBeached;
				isBeached = false;
				Transform val = null;
				float num = float.MaxValue;
				Transform[] array = planeFitPoints;
				foreach (Transform val2 in array)
				{
					Vector3 position = val2.position;
					float coarseDistanceToShore = TerrainTexturing.Instance.GetCoarseDistanceToShore(position);
					if (coarseDistanceToShore < num)
					{
						num = coarseDistanceToShore;
						val = val2;
					}
				}
				if ((Object)(object)val != (Object)null)
				{
					float overallWaterDepth = WaterLevel.GetOverallWaterDepth(val.position, waves: false, volumes: false, this);
					if (num < 10f || overallWaterDepth < 3.5f)
					{
						isBeached = true;
					}
				}
				if (!isBeached)
				{
					nextBeachedBoatCheck = TimeUntil.op_Implicit(4f);
					return;
				}
				if (isBeached && !flag)
				{
					timeBeached = TimeSince.op_Implicit(0f);
				}
			}
			if (isBeached && TimeSince.op_Implicit(timeBeached) > 10f)
			{
				float num2 = Mathf.InverseLerp(10f, 13f, TimeSince.op_Implicit(timeBeached));
				float num3 = Mathf.Lerp(0f, 3f, num2);
				Transform[] array = planeFitPoints;
				for (int i = 0; i < array.Length; i++)
				{
					Vector3 position2 = array[i].position;
					Vector3 val3 = -TerrainTexturing.Instance.GetCoarseVectorToShore(position2).shoreDir * (num3 * (1f / (float)planeFitPoints.Length)) + Vector3.up * (num3 * 0.75f);
					rigidBody.AddForceAtPosition(val3 * rigidBody.mass, position2, (ForceMode)0);
				}
			}
		}
	}

	public override bool BuoyancySleep(bool inWater)
	{
		if (isBeached)
		{
			return false;
		}
		SetToKinematic();
		return true;
	}

	public override bool BuoyancyWake()
	{
		SetToNonKinematic();
		return true;
	}

	private void CalcSailsForces(out Vector3 accumForce, out Vector3 accumTorque)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ApplySailsForces"))
		{
			if (!buoyancy.InWater)
			{
				accumForce = Vector3.zero;
				accumTorque = Vector3.zero;
				return;
			}
			List<Sail> cached = Sails.Cached;
			Vector3 val = rigidBody.centerOfMass + rigidBody.position;
			accumForce = Vector3.zero;
			accumTorque = Vector3.zero;
			foreach (Sail item in cached)
			{
				if (!(item.CurrentThrust < Mathf.Epsilon))
				{
					Vector3 val2 = Vector3.Lerp(val, item.ThrustPosition, SailPositionInfluence);
					Vector3 val3 = item.Direction * item.CurrentThrust;
					accumForce += val3;
					accumTorque += Vector3.Cross(val2 - val, val3);
				}
			}
		}
	}

	private void RudderTorque(bool reversing)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (!RudderInDeadzone() && !Anchored)
		{
			float num = (reversing ? 1f : (-1f)) * steering * steeringScale;
			float num2 = Mathf.Clamp(Vector3.Dot(rigidBody.linearVelocity, ((Component)this).transform.forward) / AntiHackVelocity(), 0.6f, 1f);
			Vector3 linearVelocity = rigidBody.linearVelocity;
			if (((Vector3)(ref linearVelocity)).sqrMagnitude < 0.1f)
			{
				num2 = 0.3f;
			}
			float num3 = num * num2 * 3f;
			rigidBody.AddRelativeTorque(Vector3.up * num3, (ForceMode)5);
		}
	}

	private void CalcEnginesForces(out Vector3 accumForce, out Vector3 accumTorque)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ApplyEngineForces"))
		{
			accumForce = Vector3.zero;
			accumTorque = Vector3.zero;
			if (!buoyancy.InWater)
			{
				return;
			}
			List<SmallEngine> cached = Engines.Cached;
			Vector3 val = rigidBody.centerOfMass + rigidBody.position;
			Vector3 forward = ((Component)this).transform.forward;
			Plane val2 = new Plane(forward, val);
			Vector3 val3 = Quaternion.AngleAxis(steering * 30f, Vector3.up) * forward;
			Vector3 val4 = Quaternion.AngleAxis((0f - steering) * 30f, Vector3.up) * forward;
			foreach (SmallEngine item in cached)
			{
				float num = item.CurrentThrust;
				if (!(num < Mathf.Epsilon) && item.TryUseFuel())
				{
					Vector3 val5 = Vector3.Lerp(val, item.ThrustPosition, EnginePositionInfluences);
					if (item.InReverse)
					{
						num *= item.ReverseMod;
					}
					Vector3 val6 = (((Plane)(ref val2)).GetSide(val5) ? val4 : val3) * num;
					accumForce += val6;
					accumTorque += Vector3.Cross(val5 - val, val6);
				}
			}
		}
	}

	public void SwitchToVehicle(bool loading)
	{
		foreach (BoatBuildingBlock item in BoatBuildingBlocks.Cached)
		{
			if (!((Object)(object)item == (Object)null))
			{
				item.SwitchToVehicle(loading);
			}
		}
	}

	public void SwitchToConstruction()
	{
		foreach (BoatBuildingBlock item in BoatBuildingBlocks.Cached)
		{
			if (!((Object)(object)item == (Object)null) && item.ProvidesParentingTrigger)
			{
				item.ResetTriggerParent();
			}
		}
	}

	public override bool AnyPlayersOnBoat()
	{
		if (base.AnyPlayersOnBoat())
		{
			return true;
		}
		List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
		GetPlayersOnBoat(list);
		bool num = list.Count > 0;
		Pool.FreeUnmanaged<BasePlayer>(ref list);
		if (!num)
		{
			return base.AnyPlayersOnBoat();
		}
		return true;
	}

	[PoolAnalyzerNonCaching]
	public override void GetPlayersOnBoat(List<BasePlayer> players)
	{
		if (players == null)
		{
			return;
		}
		players.Clear();
		base.GetPlayersOnBoat(players);
		foreach (TriggerParent parentingTrigger in parentingTriggers)
		{
			if ((Object)(object)parentingTrigger == (Object)null || !parentingTrigger.HasAnyEntityContents)
			{
				continue;
			}
			foreach (BaseEntity entityContent in parentingTrigger.entityContents)
			{
				BasePlayer basePlayer = entityContent.ToPlayer();
				if ((Object)(object)basePlayer != (Object)null && !players.Contains(basePlayer))
				{
					players.Add(basePlayer);
				}
			}
		}
		foreach (BaseEntity child in children)
		{
			if (!(child is SmallRamp) && !(child is Plank) && !(child is BaseLadder))
			{
				continue;
			}
			foreach (BaseEntity child2 in child.children)
			{
				BasePlayer basePlayer2 = child2.ToPlayer();
				if ((Object)(object)basePlayer2 != (Object)null && !players.Contains(basePlayer2))
				{
					players.Add(basePlayer2);
				}
			}
		}
	}

	public void SetAllSailsOpen(bool flag)
	{
		foreach (Sail item in Sails.Cached)
		{
			if (flag)
			{
				item.Lower(null);
			}
			else
			{
				item.Raise(null);
			}
		}
	}

	public void SetAllEnginesOn(bool flag)
	{
		foreach (SmallEngine item in Engines.Cached)
		{
			if (flag)
			{
				item.TurnOn();
			}
			else
			{
				item.TurnOff();
			}
		}
	}

	public void ResetSteering()
	{
		foreach (SteeringWheel item in SteeringWheels.Cached)
		{
			if (!((Object)(object)item == (Object)null))
			{
				item.ResetSteering();
			}
		}
	}

	public void OnAnchoredChanged()
	{
		bool anchored = Anchored;
		Anchored = false;
		foreach (Anchor item in Anchors.Cached)
		{
			if (item.Lowered)
			{
				Anchored = true;
				break;
			}
		}
		buoyancy.FlowForceDisabled = Anchored;
		if (Anchored != anchored)
		{
			SetDriftDelayAmount(Anchored ? AnchoredDriftDelaySeconds : 0f);
		}
		if (Anchored)
		{
			rigidBody.mass = 100000f;
		}
		else
		{
			rigidBody.mass = calculatedMass;
		}
	}

	private void DebugDrawDimensions()
	{
	}

	[ServerVar(Help = "(Generated) Opens all sails on the player boat directly in front of the calling admin player; admin-only dev command")]
	public static void SetSailsOpen(ConsoleSystem.Arg arg)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer.IsAdmin)
		{
			return;
		}
		List<Sail> list = Pool.Get<List<Sail>>();
		Vis.Entities(((Component)basePlayer).transform.position, arg.GetFloat(1), list, -1, (QueryTriggerInteraction)2);
		bool flag = arg.GetBool(0);
		foreach (Sail item in list)
		{
			if (item.isServer)
			{
				if (flag)
				{
					item.Lower(basePlayer);
				}
				else
				{
					item.Raise(basePlayer);
				}
			}
		}
		Pool.FreeUnmanaged<Sail>(ref list);
	}

	[ServerVar(Help = "(Generated) Instantly kills the player boat directly in front of the calling admin player; admin-only dev command")]
	public static void Sink(ConsoleSystem.Arg arg)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer.IsAdmin)
		{
			return;
		}
		PooledList<ConstructionSkin> val = Pool.Get<PooledList<ConstructionSkin>>();
		try
		{
			Vis.Components<ConstructionSkin>(((Component)basePlayer).transform.position, arg.GetFloat(1, 5f), (List<ConstructionSkin>)(object)val, 134217728, (QueryTriggerInteraction)2);
			PooledHashSet<PlayerBoat> val2 = Pool.Get<PooledHashSet<PlayerBoat>>();
			try
			{
				foreach (ConstructionSkin item in (List<ConstructionSkin>)(object)val)
				{
					PlayerBoat componentInParent = ((Component)item).GetComponentInParent<PlayerBoat>();
					if (Object.op_Implicit((Object)(object)componentInParent) && !componentInParent.IsClient && ((HashSet<PlayerBoat>)(object)val2).Add(componentInParent))
					{
						componentInParent.SetHealth(0f);
						componentInParent.Die();
					}
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

	public override void OnDied(HitInfo info)
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		if (!IsDying)
		{
			PowerDown(force: true);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Broken, b: true);
			}
			buoyancy.buoyancyScale *= 0.6f;
			SetShouldTriggersParentSwimmers(shouldParentSwimmers: false);
			SetTriggersMovementMask(Axis.XYZ);
			Invoke(DestroyFlaggedItemsOnDeath, 20f);
			Invoke(EnableDestructibleWreck, 20f);
			repair.enabled = false;
			Invoke(DismountAllPlayers, 10f);
			EnterCorpseState();
			if (sinkEffect.isValid)
			{
				Effect.server.Run(sinkEffect.resourcePath, this, 0u, default(Vector3), default(Vector3), null, false, null, 0, Effect.Type.Generic);
			}
		}
	}

	private void EnableDestructibleWreck()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved18, b: true);
	}

	private void DestroyFlaggedItemsOnDeath()
	{
		if (entitiesToDestroyOnDeath != null)
		{
			for (int num = entitiesToDestroyOnDeath.Count - 1; num >= 0; num--)
			{
				BaseEntity baseEntity = entitiesToDestroyOnDeath[num];
				if (!((Object)(object)baseEntity == (Object)null))
				{
					baseEntity.Kill(DestroyMode.Gib);
				}
			}
		}
		if (ioEntitiesToDisconnectOnDeath == null)
		{
			return;
		}
		for (int num2 = ioEntitiesToDisconnectOnDeath.Count - 1; num2 >= 0; num2--)
		{
			IOEntity iOEntity = ioEntitiesToDisconnectOnDeath[num2];
			if (!((Object)(object)iOEntity == (Object)null))
			{
				iOEntity.DisconnectAll();
			}
		}
	}

	protected void EnterCorpseState()
	{
		Invoke(ActualDeath, corpseseconds);
	}

	public void ActualDeath()
	{
		Kill(DestroyMode.Gib);
	}

	public void OnBoatDeployableHurt(BaseEntity deployable, HitInfo info)
	{
		TakeDamage(deployable, ParentedBoatDeployableProtection, info);
	}

	public void OnBuildingBlockHurt(BoatBuildingBlock block, HitInfo info)
	{
		TakeDamage(block, block.baseProtection, info);
	}

	private void TakeDamage(BaseEntity damagedEntity, ProtectionProperties protectionProperties, HitInfo hitInfo)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (DeepSeaManager.IsInsideDeepSea((BaseNetworkable)this) && InSafeZone())
		{
			return;
		}
		float num = base.health;
		ProtectionProperties protectionProperties2 = baseProtection;
		baseProtection = protectionProperties;
		Hurt(hitInfo);
		baseProtection = protectionProperties2;
		if (damagedEntity is BoatBuildingBlock boatBuildingBlock)
		{
			float amount = base.health - num;
			boatBuildingBlock.RecordDamageTaken(amount);
		}
		if (hitInfo.damageTypes.Has(DamageType.Cannon))
		{
			timeSinceLastCannonAttack = TimeSince.op_Implicit(0f);
			if (Rust.GameInfo.HasAchievements && Object.op_Implicit((Object)(object)hitInfo.InitiatorPlayer))
			{
				hitInfo.InitiatorPlayer.GiveAchievement("BOAT_CANNON_HIT");
			}
		}
	}

	public override BasePlayer GetPlayerDamageInitiator()
	{
		if (HasDriver())
		{
			foreach (SteeringWheel item in SteeringWheels.Cached)
			{
				BasePlayer mounted = item.GetMounted();
				if (Object.op_Implicit((Object)(object)mounted))
				{
					return mounted;
				}
			}
		}
		else if (cachedPlayersAboard)
		{
			foreach (TriggerParent parentingTrigger in parentingTriggers)
			{
				if (!Object.op_Implicit((Object)(object)parentingTrigger) || !parentingTrigger.HasAnyEntityContents)
				{
					continue;
				}
				foreach (BaseEntity entityContent in parentingTrigger.entityContents)
				{
					if ((Object)(object)entityContent.ToPlayer() != (Object)null)
					{
						return entityContent as BasePlayer;
					}
				}
			}
		}
		return BasePlayer.FindByID(base.OwnerID);
	}

	public float GetDamageMultiplier(BaseEntity ent)
	{
		return Mathf.Max(1f, Mathf.Abs(GetSpeed()) * 4f);
	}

	public void OnHurtTriggerOccupant(BaseEntity hurtEntity, DamageType damageType, float damageTotal)
	{
	}

	protected override float GetPushActionForce()
	{
		return Mathf.Min(rigidBody.mass, 1000f) * 5f;
	}

	public override bool AllowInitChildSupports()
	{
		return true;
	}

	private void BoatDecay()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		BaseBoat.WaterVehicleDecay(this, 60f, TimeSince.op_Implicit(timeSinceLastUsed), decayminutes, decayminutes, decaystartdelayminutes, preventDecayIndoors);
	}

	void IReceiveDeepSeaNotifications.OnEnterDeepSea()
	{
		TriggerPowerDownGracePeriod();
	}

	void IReceiveDeepSeaNotifications.OnExitDeepSea()
	{
		TriggerPowerDownGracePeriod();
	}

	private void TriggerPowerDownGracePeriod()
	{
		powerDownGraceExpireTimestamp = Time.time + DeepSeaTransitionPowerDownGraceDuration;
	}

	[ServerVar(Help = "(Generated) Kills all player boats that have more building blocks than the given threshold; used for server cleanup")]
	public static void kill_all_above_block_count(ConsoleSystem.Arg arg)
	{
		if (arg.Args == null || arg.Args.Length == 0)
		{
			arg.ReplyWith("Must provide a minimum block count.");
			return;
		}
		int num = arg.GetInt(0);
		int num2 = 0;
		PlayerBoat[] array = Util.FindAll<PlayerBoat>();
		foreach (PlayerBoat playerBoat in array)
		{
			if (!((Object)(object)playerBoat == (Object)null) && playerBoat.BoatBuildingBlocks != null && playerBoat.BoatBuildingBlocks.Cached.Count >= num)
			{
				playerBoat.Kill();
				num2++;
			}
		}
		arg.ReplyWith($"Killed {num2} player boats");
	}

	[ServerVar(Help = "(Generated) Kills all player boats that have more deployed entities than the given threshold; used for server cleanup")]
	public static void kill_all_above_deployable_count(ConsoleSystem.Arg arg)
	{
		if (arg.Args == null || arg.Args.Length == 0)
		{
			arg.ReplyWith("Must provide a minimum deployable count.");
			return;
		}
		int num = arg.GetInt(0);
		int num2 = 0;
		PlayerBoat[] array = Util.FindAll<PlayerBoat>();
		foreach (PlayerBoat playerBoat in array)
		{
			if (!((Object)(object)playerBoat == (Object)null) && playerBoat.Deployables != null && playerBoat.Deployables.Cached.Count >= num)
			{
				playerBoat.Kill();
				num2++;
			}
		}
		arg.ReplyWith($"Killed {num2} player boats");
	}

	[ServerVar(Help = "(Generated) Prints statistics about all player boats on the server including block counts, deployable counts, and resource totals")]
	public static void print_stats(ConsoleSystem.Arg arg)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("BOATS:");
		PlayerBoat[] array = Util.FindAll<PlayerBoat>();
		foreach (PlayerBoat playerBoat in array)
		{
			if (!((Object)(object)playerBoat == (Object)null) && playerBoat.BoatBuildingBlocks != null)
			{
				stringBuilder.AppendLine($"{playerBoat.BoatBuildingBlocks.Cached.Count} blocks / " + $"{playerBoat.Deployables.Cached.Count} deployables. Alive: " + $"{Time.time - playerBoat.boatSpawnTime}s. Pos: " + $"{((Component)playerBoat).transform.position}");
			}
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "Prints a list of boats with non-convex collider deployables. Not a fast command. Use sparingly when needed.")]
	public static void print_nonconvex(ConsoleSystem.Arg arg)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("BOATS WITH NON CONVEX MESH COLLIDERS:");
		PlayerBoat[] array = Util.FindAll<PlayerBoat>();
		foreach (PlayerBoat playerBoat in array)
		{
			if (!((Object)(object)playerBoat == (Object)null) && playerBoat.Deployables != null && playerBoat.HasNonConvexColliders())
			{
				stringBuilder.AppendLine($"{playerBoat.BoatBuildingBlocks.Cached.Count} blocks / " + $"{playerBoat.Deployables.Cached.Count} deployables. Alive: " + $"{Time.time - playerBoat.boatSpawnTime}s. Pos: " + $"{((Component)playerBoat).transform.position}");
			}
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "Kills any entities deployed on boats with non-convex colliders. Not a fast command. Use sparingly when needed.")]
	public static void kill_nonconvex_deployables(ConsoleSystem.Arg arg)
	{
		new StringBuilder();
		int num = 0;
		PlayerBoat[] array = Util.FindAll<PlayerBoat>();
		foreach (PlayerBoat playerBoat in array)
		{
			if (!((Object)(object)playerBoat == (Object)null) && playerBoat.Deployables != null && playerBoat.DestroyNonConvexColliderDeployables())
			{
				num++;
			}
		}
		arg.ReplyWith("Killed convex entities on " + num + " boats.");
	}

	[ServerVar(Help = "Kills any IO entities deployed on boats. Not a fast command. Use sparingly when needed.")]
	public static void kill_io_deployables(ConsoleSystem.Arg arg)
	{
		new StringBuilder();
		int num = 0;
		PlayerBoat[] array = Util.FindAll<PlayerBoat>();
		PlayerBoat[] array2 = array;
		foreach (PlayerBoat playerBoat in array2)
		{
			if ((Object)(object)playerBoat == (Object)null || playerBoat.Deployables == null)
			{
				continue;
			}
			for (int num2 = playerBoat.Deployables.Cached.Count - 1; num2 >= 0; num2--)
			{
				BaseEntity baseEntity = playerBoat.Deployables.Cached[num2];
				if (!((Object)(object)baseEntity == (Object)null) && !((Object)(object)((Component)baseEntity).gameObject == (Object)null) && baseEntity is IOEntity iOEntity && !(iOEntity is Signage))
				{
					baseEntity.Kill();
					num++;
				}
			}
		}
		arg.ReplyWith($"Killed {num} IO entities on {array.Length} boats.");
	}

	public bool HasNonConvexColliders()
	{
		foreach (BaseEntity item in Deployables.Cached)
		{
			if ((Object)(object)item == (Object)null || (Object)(object)((Component)item).gameObject == (Object)null)
			{
				continue;
			}
			MeshCollider[] componentsInChildren = ((Component)item).gameObject.GetComponentsInChildren<MeshCollider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (!componentsInChildren[i].convex)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool DestroyNonConvexColliderDeployables()
	{
		int num = 0;
		for (int num2 = Deployables.Cached.Count - 1; num2 >= 0; num2--)
		{
			BaseEntity baseEntity = Deployables.Cached[num2];
			if (!((Object)(object)baseEntity == (Object)null) && !((Object)(object)((Component)baseEntity).gameObject == (Object)null) && !(baseEntity is global::IBoatBuildingPiece))
			{
				MeshCollider[] componentsInChildren = ((Component)baseEntity).gameObject.GetComponentsInChildren<MeshCollider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (!componentsInChildren[i].convex)
					{
						num++;
						baseEntity.Kill();
						break;
					}
				}
			}
		}
		return num > 0;
	}

	public bool IsWithinDegreesOfUp(float maxDegrees)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Dot(((Component)this).transform.up, Vector3.up);
		float num2 = Mathf.Cos(maxDegrees * (MathF.PI / 180f));
		return num >= num2;
	}

	public void Teleport(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = position;
		Rigidbody component = ((Component)this).GetComponent<Rigidbody>();
		if ((Object)(object)component != (Object)null)
		{
			component.velocity = Vector3.zero;
			component.angularVelocity = Vector3.zero;
		}
		SendNetworkUpdateImmediate();
		UpdateNetworkGroup();
		List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
		GetMountedPlayers(list);
		foreach (BasePlayer item in list)
		{
			item.Teleport(((Component)item).transform.position);
		}
		Pool.FreeUnmanaged<BasePlayer>(ref list);
	}

	public SteeringWheel GetSteeringWheel()
	{
		foreach (SteeringWheel item in SteeringWheels.Cached)
		{
			if (!((Object)(object)item == (Object)null))
			{
				return item;
			}
		}
		return null;
	}

	public override bool ForceChildFullStability()
	{
		if (IsDestructibleWreck)
		{
			return !UseDestructibleWreckStability;
		}
		return true;
	}

	public override float AntiHackVelocity()
	{
		if (base.isServer)
		{
			return adjustedVelocityMax.Get(force: false);
		}
		return VelocityMax;
	}

	public static bool IsChildOfInteractablePlayerBoat(BaseEntity entity)
	{
		PlayerBoat parentPlayerBoat = GetParentPlayerBoat(entity);
		if ((Object)(object)parentPlayerBoat == (Object)null)
		{
			return false;
		}
		return !parentPlayerBoat.IsDying;
	}

	public static bool IsChildOfFinishedPlayerBoat(BaseEntity entity)
	{
		if ((Object)(object)GetParentPlayerBoat(entity) == (Object)null)
		{
			return false;
		}
		return true;
	}

	public static bool HasPermissionToPickup(BasePlayer player, BaseEntity entity, out bool parentIsBoat)
	{
		parentIsBoat = false;
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		PlayerBoat parentPlayerBoat = GetParentPlayerBoat(entity);
		if ((Object)(object)parentPlayerBoat == (Object)null)
		{
			return false;
		}
		parentIsBoat = true;
		return parentPlayerBoat.IsAuthedForBuilding(player);
	}

	public static PlayerBoat GetParentPlayerBoat(BaseEntity entity, bool includeEntityItself = false)
	{
		if ((Object)(object)entity == (Object)null)
		{
			return null;
		}
		if (includeEntityItself && entity is PlayerBoat)
		{
			return entity as PlayerBoat;
		}
		BaseEntity baseEntity = entity.GetParentEntity();
		while ((Object)(object)baseEntity != (Object)null)
		{
			if (baseEntity is PlayerBoat result)
			{
				return result;
			}
			entity = baseEntity;
			baseEntity = entity.GetParentEntity();
		}
		return null;
	}

	public override void Load(LoadInfo info)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		lastEditLocalPos = info.msg.playerBoat.lastEditLocalPos;
		lastEditLocalRot = info.msg.playerBoat.lastEditLocalRot;
		if (base.isServer)
		{
			rigidBody.isKinematic = true;
		}
	}

	public void SetDimensions(Vector3 size)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		bounds = new Bounds(new Vector3(0f, size.y / 8f, 0f), size);
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}

	public override bool ForceDeployableSetParent()
	{
		return true;
	}

	public void OnSubChildAdded(BaseEntity subChild)
	{
		ListenServerColliderFix();
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		CacheChild(child);
		if (base.isServer)
		{
			if (ShouldDestroyOnDeath(child))
			{
				entitiesToDestroyOnDeath.Add(child);
			}
			if (child is IOEntity item)
			{
				ioEntitiesToDisconnectOnDeath.Add(item);
			}
		}
		if (!(child is BasePlayer))
		{
			ListenServerColliderFix();
		}
	}

	protected override void OnChildRemoved(BaseEntity child)
	{
		base.OnChildRemoved(child);
		if (base.isServer && KilledForEditMode)
		{
			return;
		}
		UnCacheChild(child);
		if (base.isServer)
		{
			if (ShouldDestroyOnDeath(child))
			{
				entitiesToDestroyOnDeath.Remove(child);
			}
			if (child is IOEntity item)
			{
				ioEntitiesToDisconnectOnDeath.Remove(item);
			}
		}
	}

	private bool ShouldDestroyOnDeath(BaseEntity entity)
	{
		if ((Object)(object)entity == (Object)null)
		{
			return false;
		}
		if (entity is BoatBuildingBlock)
		{
			return false;
		}
		if (entity is Door)
		{
			return true;
		}
		if (entity is SimpleBuildingBlock)
		{
			return true;
		}
		if (ChildrenToDestroyOnDeath != null)
		{
			foreach (BaseEntity item in ChildrenToDestroyOnDeath)
			{
				if (!((Object)(object)item == (Object)null) && item.prefabID == entity.prefabID)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ListenServerColliderFix()
	{
	}

	private void CacheChild(BaseEntity child)
	{
		AddIf<Sail>(child as Sail, Sails.Cached);
		AddIf<SmallEngine>(child as SmallEngine, Engines.Cached);
		AddIf<BoatBuildingBlock>(child as BoatBuildingBlock, BoatBuildingBlocks.Cached);
		AddIf<Anchor>(child as Anchor, Anchors.Cached);
		AddIf<SteeringWheel>(child as SteeringWheel, SteeringWheels.Cached);
		if (!(child is BoatBuildingBlock) && !(child is BasePlayer) && !(child is DroppedItem))
		{
			AddIf<BaseEntity>(child, Deployables.Cached);
		}
		static bool AddIf<T>(T value, List<T> list) where T : BaseEntity
		{
			if ((Object)(object)value != (Object)null && !list.Contains(value))
			{
				list.Add(value);
				return true;
			}
			return false;
		}
	}

	private void UnCacheChild(BaseEntity child)
	{
		RemoveIf<Sail>(child as Sail, Sails.Cached);
		RemoveIf<SmallEngine>(child as SmallEngine, Engines.Cached);
		RemoveIf<BoatBuildingBlock>(child as BoatBuildingBlock, BoatBuildingBlocks.Cached);
		RemoveIf<Anchor>(child as Anchor, Anchors.Cached);
		RemoveIf<SteeringWheel>(child as SteeringWheel, SteeringWheels.Cached);
		if (!(child is BoatBuildingBlock) && !(child is BasePlayer) && !(child is DroppedItem))
		{
			RemoveIf<BaseEntity>(child, Deployables.Cached);
		}
		static void RemoveIf<T>(T value, List<T> list) where T : BaseEntity
		{
			if ((Object)(object)value != (Object)null)
			{
				list.Remove(value);
			}
		}
	}

	protected override bool IgnoreChildEntitiesForDismountClipChecks()
	{
		return true;
	}

	protected override bool DismountCheckSkipVehicles()
	{
		return false;
	}

	public bool CanStartEditing(BasePlayer player, bool sendErrorToasts)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		object obj = Interface.CallHook("CanEditPlayerBoat", this, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (IsDying)
		{
			return false;
		}
		if (base.isServer)
		{
			Vector3 linearVelocity = rigidBody.linearVelocity;
			if (((Vector3)(ref linearVelocity)).magnitude >= MaxEditVelocity)
			{
				if (sendErrorToasts)
				{
					player.ShowToast(GameTip.Styles.Error, tooFastToEditErrorPhrase, false);
				}
				return false;
			}
		}
		if (IsOnDamagedCoolDown())
		{
			if (base.isServer & sendErrorToasts)
			{
				player.ShowToast(GameTip.Styles.Error, recentlyDamagedCantEditPhrase, false);
			}
			return false;
		}
		if (!IsPlayerAuthed(player, authedIfNoPriveOrLock: true))
		{
			return false;
		}
		return true;
	}

	public bool IsOnDamagedCoolDown()
	{
		if (base.isServer)
		{
			if (base.SecondsSinceAttacked <= GetDamageRepairCooldown())
			{
				return Time.time - boatSpawnTime > GetDamageRepairCooldown();
			}
			return false;
		}
		return false;
	}

	public override Vector3 GetDismountCheckStart(BasePlayer player)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		List<BoatBuildingBlock> cached = BoatBuildingBlocks.Cached;
		if (cached == null || cached.Count == 0)
		{
			return base.GetDismountCheckStart(player);
		}
		TriggerLadder triggerLadder = player.FindTrigger<TriggerLadder>();
		if (Object.op_Implicit((Object)(object)triggerLadder))
		{
			return GameObjectEx.ToBaseEntity(((Component)triggerLadder).gameObject).CenterPoint();
		}
		Vector3 val = player.TriggerPoint();
		int num = -1;
		float num2 = float.MaxValue;
		for (int i = 0; i < cached.Count; i++)
		{
			BoatBuildingBlock boatBuildingBlock = cached[i];
			if ((Object)(object)boatBuildingBlock == (Object)null)
			{
				continue;
			}
			Transform[] dismountPoints = boatBuildingBlock.DismountPoints;
			if (dismountPoints == null || dismountPoints.Length != 0)
			{
				float num3 = Vector3.SqrMagnitude(val - boatBuildingBlock.GetDismountCheckStart());
				if (num3 < num2)
				{
					num = i;
					num2 = num3;
				}
			}
		}
		Debug.Assert(num != -1);
		return cached[num].GetDismountCheckStart();
	}

	protected override bool CanPushNow(BasePlayer pusher)
	{
		if (IsOn())
		{
			return false;
		}
		if (Anchored)
		{
			return false;
		}
		if (pusher.OnLadder())
		{
			return false;
		}
		if ((Object)(object)pusher.GetParentEntity() == (Object)(object)this)
		{
			return false;
		}
		if (!IsFlipped() && pusher.IsStandingOnEntity(this, 1218652417))
		{
			return false;
		}
		if (pusher.IsBuildingBlockedByVehicle())
		{
			return false;
		}
		if (IsDying)
		{
			return false;
		}
		if (!pusher.isMounted)
		{
			return base.healthFraction > 0f;
		}
		return false;
	}

	public bool IsOnDeployAndEditCoolDown()
	{
		if (!IsBusy())
		{
			return BoatBuildingStation.IsOnGlobalEditFinishCoolDown();
		}
		return true;
	}

	public bool CanDeployAndEdit(BasePlayer player, out Vector3 outPosition, out Quaternion outRotation, bool checkForCoolDown, bool checkForItem, bool checkDeploy, bool sendErrorToasts)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		outPosition = Vector3.zero;
		outRotation = Quaternion.identity;
		if (!EditEnabled)
		{
			return false;
		}
		if (checkForCoolDown && IsOnDeployAndEditCoolDown())
		{
			if (sendErrorToasts && (Object)(object)player != (Object)null)
			{
				player.ShowToast(GameTip.Styles.Error, onDeployEditCoolDownPhrase, false);
			}
			return false;
		}
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if ((Object)(object)player.GetParentEntity() != (Object)(object)this)
		{
			return false;
		}
		if (!player.CanInteract())
		{
			return false;
		}
		if (DeepSeaManager.IsInsideDeepSea(((Component)this).transform.position))
		{
			return false;
		}
		if (checkForItem && !PlayerHasBBSItem(player))
		{
			return false;
		}
		if (!CanStartEditing(player, sendErrorToasts))
		{
			return false;
		}
		if (checkDeploy)
		{
			GetDeployAndEditPositionRotation(out outPosition, out outRotation);
			if (!ContainerCorpse.IsValidPointForEntity(BoatBuildingStationPrefab.resourceID, outPosition, outRotation, this, -1, ignoreChildrenOfEntity: true))
			{
				if (sendErrorToasts && (Object)(object)player != (Object)null)
				{
					player.ShowToast(GameTip.Styles.Error, invalidDeployLocationPhrase, false);
				}
				return false;
			}
		}
		return true;
	}

	public bool PlayerHasBBSItem(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		return player.inventory.GetAmount(BoatBuildingStationItem) > 0;
	}

	public void GetDeployAndEditPositionRotation(out Vector3 outPosition, out Quaternion outRotation)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3Ex.WithY(((Component)this).transform.position, 0f);
		Quaternion val2 = Quaternion.Euler(Vector3Ex.WithXZ(((Component)this).transform.eulerAngles, 0f, 0f));
		outPosition = val + val2 * lastEditLocalPos;
		outRotation = val2 * Quaternion.Euler(lastEditLocalRot);
	}

	protected void OnCollisionEnter(Collision collision)
	{
		if (!base.isClient)
		{
			ProcessCollision(collision);
		}
	}

	private void ProcessCollision(Collision collision)
	{
		if (!base.isClient && collision != null && !((Object)(object)collision.gameObject == (Object)null) && !((Object)(object)collision.gameObject == (Object)null))
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(collision.gameObject);
			if (Interface.CallHook("OnPlayerBoatCollide", this, baseEntity, collision) == null && (Object)(object)baseEntity != (Object)null && !baseEntity.isClient && baseEntity is IDestroyableOnPlayerBoatCollision destroyableOnPlayerBoatCollision && destroyableOnPlayerBoatCollision.ShouldBeDestroyedBy(this))
			{
				baseEntity.Kill(DestroyMode.Gib);
			}
		}
	}

	public override EntityBuildCost BuildCost()
	{
		return new EntityBuildCost(DynamicBuildCost);
	}

	static PlayerBoat()
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		DeepSeaTransitionPowerDownGraceDuration = 10f;
		PowerdownOnNoPlayers = true;
		OrphanSendImmediate = true;
		AboardPlayerCheckInterval = 2f;
		AnchoredDriftDelaySeconds = 21600f;
		SailPositionInfluence = 0.1f;
		EnginePositionInfluences = 0.05f;
		PlacementUpThreshold = 30f;
		corpseseconds = 1800f;
		decayminutes = 720f;
		decaystartdelayminutes = 1440f;
		CannonHitSlowdownMultiplier = 0f;
		DragByAngle_MinDrag = 0.65f;
		DragByAngle_MaxDrag = 2.2f;
		DragByAngle_MinContrib = 5f;
		DragByAngle_MaxContrib = 50f;
		DragByAngle_Exponent = 0.4f;
		tooFastToEditErrorPhrase = new Phrase("playerboat_too_fast_to_edit", "The boat is moving too fast to edit.");
		recentlyDamagedCantEditPhrase = new Phrase("playerboat_recently_damaged_cant_edit", "The boat has recently been damaged and can't be edited.");
		onDeployEditCoolDownPhrase = new Phrase("playerboat_cooldown_phrase", "Deploy & Edit is on cooldown.");
		invalidDeployLocationPhrase = new Phrase("playerboat_invalid_deploy_location_phrase", "Unable to deploy & edit in this location.");
		EditEnabled = true;
		FinishEditingEnabled = true;
		HammerRepairEnabled = true;
		MaxBlockCount = 180;
		MaxDeployableCount = 120;
		DestructibleWrecksEnabled = true;
		UseDestructibleWreckStability = true;
		VelocityMax = 15f;
	}
}

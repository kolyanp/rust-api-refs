using System;
using System.Collections.Generic;
using AntiHackJobs;
using BasePlayerJobs;
using ConVar;
using Epic.OnlineServices.Reports;
using Facepunch;
using Facepunch.Rust;
using Oxide.Core;
using Rust;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UtilityJobs;

public static class AntiHack
{
	public struct Batch
	{
		public int PlayerIndex;

		public int Count;

		public bool Force;

		public bool CastVehicleLayer;

		public bool SkipVehicleLayer;
	}

	public struct FlyingBatch
	{
		public int PlayerIndex;

		public int Count;
	}

	private class GroupedLog : IPooled
	{
		public float firstLogTime;

		public string playerName;

		public AntiHackType antiHackType;

		public string message;

		public Vector3 averagePos;

		public int num;

		public GroupedLog()
		{
		}

		public GroupedLog(string playerName, AntiHackType antiHackType, string message, Vector3 pos)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			SetInitial(playerName, antiHackType, message, pos);
		}

		public void EnterPool()
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			firstLogTime = 0f;
			playerName = string.Empty;
			antiHackType = AntiHackType.None;
			averagePos = Vector3.zero;
			num = 0;
		}

		public void LeavePool()
		{
		}

		public void SetInitial(string playerName, AntiHackType antiHackType, string message, Vector3 pos)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			firstLogTime = Time.unscaledTime;
			this.playerName = playerName;
			this.antiHackType = antiHackType;
			this.message = message;
			averagePos = pos;
			num = 1;
		}

		public bool TryGroup(string playerName, AntiHackType antiHackType, string message, Vector3 pos, float maxDistance)
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			if (antiHackType != this.antiHackType || playerName != this.playerName || message != this.message)
			{
				return false;
			}
			if (Vector3.SqrMagnitude(averagePos - pos) > maxDistance * maxDistance)
			{
				return false;
			}
			Vector3 val = averagePos * (float)num;
			averagePos = (val + pos) / (float)(num + 1);
			num++;
			return true;
		}
	}

	public struct PlayerState
	{
		public float ViolationLevel;

		public float LastViolationTime;

		public float LastMovementViolationTime;

		public float LastAdminCheatTime;

		public float TickDistancePausetime;

		public float UnparentTime;

		public AntiHackType LastViolationType;
	}

	public struct PlayerNoclipState
	{
		public float VehiclePauseTime;

		public float ForceCastTime;
	}

	public struct PlayerSpeedhackState
	{
		public float PauseTime;

		public float ExtraSpeedTime;

		public float Distance;

		public float ExtraSpeed;
	}

	public struct PlayerFlyhackState
	{
		public Vector3 LastGroundedPosition;

		public float PauseTime;

		public float VerticalDistance;

		public float HorizontalDistance;

		public float LastInAirTime;

		public bool IsInAir;

		public bool IsOnPlayer;
	}

	private const int movement_mask = 1503731969;

	private const int vehicle_mask = 134225920;

	private const int grounded_mask = 1503764737;

	private const int player_mask = 131072;

	private static Collider[] buffer = (Collider[])(object)new Collider[4];

	private static Dictionary<ulong, int> kicks = new Dictionary<ulong, int>();

	private static Dictionary<ulong, int> bans = new Dictionary<ulong, int>();

	private const float LOG_GROUP_SECONDS = 60f;

	private static Queue<GroupedLog> groupedLogs = new Queue<GroupedLog>();

	private static NativeArray<bool> FindIndexWorkBuffer;

	private static NativeList<int> ValidIndexAccum1;

	private static NativeList<int> ValidIndexAccum2;

	private static NativeList<int> InvalidIndices;

	private static NativeList<Vector3> From;

	private static NativeList<Vector3> To;

	private static NativeList<Batch> Batches;

	private static NativeList<int> LayerMasks;

	private static NativeArray<float> PlayerRadii;

	private static NativeList<int> ToOverlapIndices;

	private static NativeList<Matrix4x4> Matrices;

	private static NativeList<Vector3> ToOverlapFrom;

	private static NativeList<Vector3> ToOverlapTo;

	private static NativeList<int> ToOverlapLayerMasks;

	private static NativeList<int> RaycastIndices;

	private static NativeList<RaycastCommand> RaycastRays;

	private static NativeList<SpherecastCommand> RaycastSpheres;

	private static NativeList<int> TraceIndices;

	private static NativeList<RaycastCommand> TraceRays;

	private static NativeList<SpherecastCommand> TraceSpheres;

	private static NativeArray<RaycastHit> RaycastHits;

	private static NativeArray<ColliderHit> ColliderHits;

	private static NativeArray<bool> TerrainIgnoreVolumeHits;

	private static NativeArray<int> QueryToBatchMap;

	private static BufferList<Collider> Colliders;

	public static NativeArray<PlayerState> PlayerStates;

	public static NativeArray<PlayerNoclipState> PlayerNoclipStates;

	public static NativeArray<PlayerSpeedhackState> PlayerSpeedhackStates;

	public static NativeArray<PlayerFlyhackState> PlayerFlyhackStates;

	public static RaycastHit isInsideRayHit;

	private static RaycastHit[] isInsideMeshRaycastHits = (RaycastHit[])(object)new RaycastHit[64];

	public static bool TestNoClipping(BasePlayer ply, Vector3 oldPos, Vector3 newPos, float radius, float backtracking, out Collider col, bool overlapVehicleLayer = false, BaseEntity ignoreEntity = null, bool forceCast = false, bool ignoreChildrenOfIgnoreEntity = false, bool skipVehicles = false)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		int num = 1503731969;
		if (skipVehicles)
		{
			num &= -134225921;
		}
		Vector3 val = newPos - oldPos;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector3 val2 = oldPos - normalized * backtracking;
		val = newPos - val2;
		float magnitude = ((Vector3)(ref val)).magnitude;
		Ray val3 = default(Ray);
		((Ray)(ref val3))._002Ector(val2, normalized);
		if (GamePhysics.CheckCapsule(oldPos, newPos, radius, num, (QueryTriggerInteraction)1))
		{
			List<Collider> list = Pool.Get<List<Collider>>();
			GamePhysics.OverlapCapsule(oldPos, newPos, radius, list, num, (QueryTriggerInteraction)1);
			bool recheck = false;
			bool recheckTerrain = false;
			for (int i = 0; i < list.Count; i++)
			{
				Collider val4 = list[i];
				if (IsColliderBlocking(val4, ply, forceCast, !overlapVehicleLayer, ignoreEntity, ignoreChildrenOfIgnoreEntity, ref recheck, ref recheckTerrain))
				{
					col = val4;
					Pool.FreeUnmanaged<Collider>(ref list);
					return true;
				}
			}
			Pool.FreeUnmanaged<Collider>(ref list);
			if (recheck || recheckTerrain)
			{
				if (!recheckTerrain && (Object)(object)ignoreEntity == (Object)null)
				{
					RaycastHit val5 = default(RaycastHit);
					bool result = Physics.Raycast(val3, ref val5, magnitude + radius, num, (QueryTriggerInteraction)1) || Physics.SphereCast(val3, radius, ref val5, magnitude, num, (QueryTriggerInteraction)1);
					col = ((RaycastHit)(ref val5)).collider;
					return result;
				}
				RaycastHit hitInfo;
				bool result2 = GamePhysics.Trace(val3, 0f, out hitInfo, magnitude + radius, num, (QueryTriggerInteraction)1, ignoreEntity) || GamePhysics.Trace(val3, radius, out hitInfo, magnitude, num, (QueryTriggerInteraction)1, ignoreEntity);
				col = ((RaycastHit)(ref hitInfo)).collider;
				return result2;
			}
		}
		col = null;
		return false;
	}

	private static bool IsColliderBlocking(Collider collider, BasePlayer ply, bool forceCast, bool forceCastVehicles, BaseEntity ignoreEntity, bool ignoreChildrenOfIgnoreEntity, ref bool recheck, ref bool recheckTerrain)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (collider is TerrainCollider)
		{
			recheckTerrain = true;
			return false;
		}
		if ((LayerMask.op_Implicit(collider.excludeLayers) & 0x1000) == 4096)
		{
			return false;
		}
		if (forceCastVehicles && ((1 << ((Component)collider).gameObject.layer) & 0x8002000) > 0)
		{
			recheck = true;
			return false;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(collider);
		if (((1 << ((Component)collider).gameObject.layer) & 0x2000) > 0)
		{
			if (baseEntity is HotAirBalloon && ply.RecentlyUnparented(5f))
			{
				return false;
			}
			recheck = true;
			return false;
		}
		if (GamePhysics.CompareEntity(baseEntity, ignoreEntity))
		{
			return false;
		}
		if (ignoreChildrenOfIgnoreEntity && Object.op_Implicit((Object)(object)baseEntity) && !baseEntity.ShouldAlwaysBlockNoClipChecks() && GamePhysics.CompareEntity(baseEntity.GetRootParentEntity(), ignoreEntity))
		{
			return false;
		}
		if (forceCast)
		{
			recheck = true;
			return false;
		}
		if ((Object)(object)baseEntity != (Object)null && baseEntity.ShouldUseCastNoClipChecks())
		{
			recheck = true;
			return false;
		}
		if (ply.GetParentEntity() is ElevatorLift)
		{
			recheck = true;
			return false;
		}
		return true;
	}

	public static void TestAreNoClipping(in BasePlayer.PlayerServerStates.ReadOnly playerStates, ReadOnly<Vector3> fromPos, ReadOnly<Vector3> toPos, ReadOnly<Batch> batches, NativeList<int> foundIndices, Span<Collider> foundColls)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_057a: Unknown result type (might be due to invalid IL or missing references)
		//IL_057f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Unknown result type (might be due to invalid IL or missing references)
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_0497: Unknown result type (might be due to invalid IL or missing references)
		//IL_0499: Unknown result type (might be due to invalid IL or missing references)
		//IL_049b: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05de: Unknown result type (might be due to invalid IL or missing references)
		//IL_05df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0520: Unknown result type (might be due to invalid IL or missing references)
		//IL_0506: Unknown result type (might be due to invalid IL or missing references)
		//IL_0812: Unknown result type (might be due to invalid IL or missing references)
		//IL_0817: Unknown result type (might be due to invalid IL or missing references)
		//IL_081f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0825: Unknown result type (might be due to invalid IL or missing references)
		//IL_0827: Unknown result type (might be due to invalid IL or missing references)
		//IL_082c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0612: Unknown result type (might be due to invalid IL or missing references)
		//IL_0617: Unknown result type (might be due to invalid IL or missing references)
		//IL_061d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0625: Unknown result type (might be due to invalid IL or missing references)
		//IL_062a: Unknown result type (might be due to invalid IL or missing references)
		//IL_062c: Unknown result type (might be due to invalid IL or missing references)
		//IL_092a: Unknown result type (might be due to invalid IL or missing references)
		//IL_092f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0937: Unknown result type (might be due to invalid IL or missing references)
		//IL_093d: Unknown result type (might be due to invalid IL or missing references)
		//IL_093f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0944: Unknown result type (might be due to invalid IL or missing references)
		//IL_068e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0693: Unknown result type (might be due to invalid IL or missing references)
		//IL_085c: Unknown result type (might be due to invalid IL or missing references)
		//IL_086d: Unknown result type (might be due to invalid IL or missing references)
		//IL_086e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0794: Unknown result type (might be due to invalid IL or missing references)
		//IL_0959: Unknown result type (might be due to invalid IL or missing references)
		//IL_096a: Unknown result type (might be due to invalid IL or missing references)
		//IL_096b: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d8: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("TestAreNoClipping"))
		{
			float noclip_backtracking = ConVar.AntiHack.noclip_backtracking;
			float num = BasePlayer.NoClipRadius(ConVar.AntiHack.noclip_margin);
			NativeListEx.Expand(ref LayerMasks, fromPos.Length, copyContents: false);
			BuildLayerMasksJob buildLayerMasksJob = new BuildLayerMasksJob
			{
				LayerMasks = LayerMasks,
				Batches = batches,
				DefaultMask = 1503731969,
				NoVehicleMask = 1369506049
			};
			IJobExtensions.RunByRef<BuildLayerMasksJob>(ref buildLayerMasksJob);
			NativeArrayEx.Expand(ref PlayerRadii, fromPos.Length, (NativeArrayOptions)0, copyContents: false);
			FillJob<float> fillJob = new FillJob<float>
			{
				Values = PlayerRadii,
				Value = num
			};
			IJobExtensions.RunByRef<FillJob<float>>(ref fillJob);
			NativeArrayEx.Expand(ref TerrainIgnoreVolumeHits, fromPos.Length, (NativeArrayOptions)0, copyContents: false);
			JobHandle val = GamePhysics.CheckCapsules(fromPos, toPos, PlayerRadii.AsReadOnly(), LayerMasks.AsReadOnly(), TerrainIgnoreVolumeHits, (QueryTriggerInteraction)1, GamePhysics.MasksToValidate.Terrain);
			((JobHandle)(ref val)).Complete();
			NativeListEx.Expand(ref ToOverlapIndices, fromPos.Length, copyContents: false);
			GatherHitIndicesJob gatherHitIndicesJob = new GatherHitIndicesJob
			{
				Results = ToOverlapIndices,
				Hits = TerrainIgnoreVolumeHits.GetSubArray(0, fromPos.Length).AsReadOnly()
			};
			IJobExtensions.RunByRef<GatherHitIndicesJob>(ref gatherHitIndicesJob);
			if (ToOverlapIndices.IsEmpty)
			{
				return;
			}
			NativeArrayEx.Expand(ref QueryToBatchMap, fromPos.Length, (NativeArrayOptions)0, copyContents: false);
			BuildBatchLookupMapJob buildBatchLookupMapJob = new BuildBatchLookupMapJob
			{
				Lookup = QueryToBatchMap,
				Batches = batches
			};
			IJobExtensions.RunByRef<BuildBatchLookupMapJob>(ref buildBatchLookupMapJob);
			ToOverlapFrom.Resize(ToOverlapIndices.Length, (NativeArrayOptions)0);
			GatherJob<Vector3> gatherJob = new GatherJob<Vector3>
			{
				Results = ToOverlapFrom.AsArray(),
				Source = fromPos,
				Indices = ToOverlapIndices.AsReadOnly()
			};
			IJobExtensions.RunByRef<GatherJob<Vector3>>(ref gatherJob);
			ToOverlapTo.Resize(ToOverlapIndices.Length, (NativeArrayOptions)0);
			GatherJob<Vector3> gatherJob2 = new GatherJob<Vector3>
			{
				Results = ToOverlapTo.AsArray(),
				Source = toPos,
				Indices = ToOverlapIndices.AsReadOnly()
			};
			IJobExtensions.RunByRef<GatherJob<Vector3>>(ref gatherJob2);
			ToOverlapLayerMasks.Resize(ToOverlapIndices.Length, (NativeArrayOptions)0);
			GatherJob<int> gatherJob3 = new GatherJob<int>
			{
				Results = ToOverlapLayerMasks.AsArray(),
				Source = LayerMasks.AsReadOnly(),
				Indices = ToOverlapIndices.AsReadOnly()
			};
			IJobExtensions.RunByRef<GatherJob<int>>(ref gatherJob3);
			int defaultMaxResultsPerQuery = GamePhysics.DefaultMaxResultsPerQuery;
			ColliderHits.Expand<ColliderHit>(ToOverlapLayerMasks.Length * defaultMaxResultsPerQuery, (NativeArrayOptions)0, false, false);
			val = GamePhysics.OverlapCapsules(ToOverlapFrom.AsReadOnly(), ToOverlapTo.AsReadOnly(), PlayerRadii.GetSubArray(0, ToOverlapLayerMasks.Length).AsReadOnly(), ToOverlapLayerMasks.AsReadOnly(), ColliderHits, defaultMaxResultsPerQuery, (QueryTriggerInteraction)1, GamePhysics.MasksToValidate.Terrain);
			((JobHandle)(ref val)).Complete();
			NativeListEx.Expand(ref TraceIndices, ToOverlapIndices.Length, copyContents: false);
			TraceRays.Expand<RaycastCommand>(ToOverlapIndices.Length, false);
			NativeListEx.Expand(ref RaycastIndices, ToOverlapIndices.Length, copyContents: false);
			RaycastRays.Expand<RaycastCommand>(ToOverlapIndices.Length, false);
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			using (TimeWarning.New("FilterOverlapResults"))
			{
				bool flag = false;
				QueryParameters val8 = default(QueryParameters);
				RaycastCommand val9 = default(RaycastCommand);
				for (int i = 0; i < ToOverlapIndices.Length; i++)
				{
					int num2 = ToOverlapIndices[i];
					int num3 = QueryToBatchMap[num2];
					Batch batch = batches[num3];
					if (flag)
					{
						if (foundColls[batch.PlayerIndex] != null)
						{
							continue;
						}
						flag = false;
					}
					BasePlayer ply = objects[batch.PlayerIndex];
					bool force = batch.Force;
					bool castVehicleLayer = batch.CastVehicleLayer;
					bool recheck = false;
					bool recheckTerrain = false;
					Collider val2 = null;
					for (int j = 0; j < defaultMaxResultsPerQuery; j++)
					{
						int num4 = i * defaultMaxResultsPerQuery + j;
						ColliderHit val3 = ColliderHits[num4];
						if (((ColliderHit)(ref val3)).instanceID == 0)
						{
							break;
						}
						Collider collider = ((ColliderHit)(ref val3)).collider;
						if (IsColliderBlocking(collider, ply, force, castVehicleLayer, null, ignoreChildrenOfIgnoreEntity: false, ref recheck, ref recheckTerrain))
						{
							flag = true;
							val2 = collider;
							break;
						}
					}
					int playerIndex = batch.PlayerIndex;
					if (flag)
					{
						foundIndices.Add(ref playerIndex);
						foundColls[playerIndex] = val2;
					}
					else if (recheck || recheckTerrain)
					{
						Vector3 val4 = ToOverlapFrom[i];
						Vector3 val5 = ToOverlapTo[i];
						Vector3 val6 = val5 - val4;
						Vector3 normalized = ((Vector3)(ref val6)).normalized;
						Vector3 val7 = val4 - normalized * noclip_backtracking;
						val6 = val5 - val7;
						float magnitude = ((Vector3)(ref val6)).magnitude;
						new Ray(val7, normalized);
						int num5 = ToOverlapLayerMasks[i];
						((QueryParameters)(ref val8))._002Ector(num5, false, (QueryTriggerInteraction)1, false);
						((RaycastCommand)(ref val9))._002Ector(val7, normalized, val8, magnitude + num);
						if (recheckTerrain)
						{
							TraceIndices.AddNoResize(num2);
							TraceRays.AddNoResize(val9);
						}
						else
						{
							RaycastIndices.AddNoResize(num2);
							RaycastRays.AddNoResize(val9);
						}
					}
				}
			}
			if (!TraceIndices.IsEmpty)
			{
				RaycastHits.Expand<RaycastHit>(TraceIndices.Length * defaultMaxResultsPerQuery, (NativeArrayOptions)0, false, false);
				GamePhysics.TraceRays(TraceRays.AsArray(), RaycastHits, defaultMaxResultsPerQuery, traceWater: false);
				using (TimeWarning.New("GatherRays"))
				{
					int num6 = 0;
					TraceSpheres.Expand<SpherecastCommand>(TraceIndices.Length, false);
					SpherecastCommand val11 = default(SpherecastCommand);
					for (int k = 0; k < TraceIndices.Length; k++)
					{
						if (!RecordNoclip(RaycastHits[k * defaultMaxResultsPerQuery], TraceIndices[k], batches, foundIndices, foundColls))
						{
							TraceIndices[num6++] = TraceIndices[k];
							RaycastCommand val10 = TraceRays[k];
							((SpherecastCommand)(ref val11))._002Ector(((RaycastCommand)(ref val10)).from, num, ((RaycastCommand)(ref val10)).direction, val10.queryParameters, ((RaycastCommand)(ref val10)).distance - num);
							TraceSpheres.Add(ref val11);
						}
					}
					TraceIndices.Resize(num6, (NativeArrayOptions)0);
				}
				if (!TraceIndices.IsEmpty)
				{
					GamePhysics.TraceSpheres(TraceSpheres.AsArray(), RaycastHits, defaultMaxResultsPerQuery, traceWater: false);
					using (TimeWarning.New("GatherSpheres"))
					{
						for (int l = 0; l < TraceIndices.Length; l++)
						{
							RecordNoclip(RaycastHits[l * defaultMaxResultsPerQuery], TraceIndices[l], batches, foundIndices, foundColls);
						}
					}
				}
			}
			if (RaycastIndices.IsEmpty)
			{
				return;
			}
			if (!TraceIndices.IsEmpty)
			{
				using (TimeWarning.New("SkipDupeRaycasts"))
				{
					int num7 = 0;
					for (int m = 0; m < RaycastIndices.Length; m++)
					{
						int num8 = RaycastIndices[m];
						int num9 = QueryToBatchMap[num8];
						if (foundColls[batches[num9].PlayerIndex] == null)
						{
							int num10 = num7++;
							RaycastIndices[num10] = num8;
							RaycastRays[num10] = RaycastRays[m];
						}
					}
					RaycastIndices.Resize(num7, (NativeArrayOptions)0);
					RaycastRays.Resize(num7, (NativeArrayOptions)0);
				}
			}
			if (!RaycastIndices.IsEmpty)
			{
				using (TimeWarning.New("RayCasts"))
				{
					RaycastHits.Expand<RaycastHit>(RaycastIndices.Length, (NativeArrayOptions)0, false, false);
					NativeArray<RaycastCommand> val12 = RaycastRays.AsArray();
					NativeArray<RaycastHit> raycastHits = RaycastHits;
					val = default(JobHandle);
					JobHandle val13 = RaycastCommand.ScheduleBatch(val12, raycastHits, 1, val);
					((JobHandle)(ref val13)).Complete();
					int num11 = 0;
					RaycastSpheres.Expand<SpherecastCommand>(RaycastIndices.Length, false);
					SpherecastCommand val15 = default(SpherecastCommand);
					for (int n = 0; n < RaycastIndices.Length; n++)
					{
						if (!RecordNoclip(RaycastHits[n], RaycastIndices[n], batches, foundIndices, foundColls))
						{
							RaycastIndices[num11++] = RaycastIndices[n];
							RaycastCommand val14 = RaycastRays[n];
							((SpherecastCommand)(ref val15))._002Ector(((RaycastCommand)(ref val14)).from, num, ((RaycastCommand)(ref val14)).direction, val14.queryParameters, ((RaycastCommand)(ref val14)).distance - num);
							RaycastSpheres.AddNoResize(val15);
						}
					}
					RaycastIndices.Resize(num11, (NativeArrayOptions)0);
				}
			}
			if (RaycastIndices.IsEmpty)
			{
				return;
			}
			using (TimeWarning.New("SphereCasts"))
			{
				NativeArray<SpherecastCommand> val16 = RaycastSpheres.AsArray();
				NativeArray<RaycastHit> raycastHits2 = RaycastHits;
				val = default(JobHandle);
				JobHandle val17 = SpherecastCommand.ScheduleBatch(val16, raycastHits2, 1, val);
				((JobHandle)(ref val17)).Complete();
				for (int num12 = 0; num12 < RaycastIndices.Length; num12++)
				{
					RecordNoclip(RaycastHits[num12], RaycastIndices[num12], batches, foundIndices, foundColls);
				}
			}
		}
		static bool RecordNoclip(RaycastHit hit, int queryIndex, ReadOnly<Batch> val18, NativeList<int> val19, Span<Collider> span)
		{
			bool num13 = ((RaycastHit)(ref hit)).colliderInstanceID != 0;
			if (num13)
			{
				int num14 = QueryToBatchMap[queryIndex];
				int playerIndex2 = val18[num14].PlayerIndex;
				if (span[playerIndex2] == null)
				{
					val19.Add(ref playerIndex2);
					span[playerIndex2] = ((RaycastHit)(ref hit)).collider;
				}
			}
			return num13;
		}
	}

	public static void Cycle()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.unscaledTime - 60f;
		if (groupedLogs.Count <= 0)
		{
			return;
		}
		GroupedLog groupedLog = groupedLogs.Peek();
		while (groupedLog.firstLogTime <= num)
		{
			GroupedLog groupedLog2 = groupedLogs.Dequeue();
			LogToConsole(groupedLog2.playerName, groupedLog2.antiHackType, $"{groupedLog2.message} (x{groupedLog2.num})", groupedLog2.averagePos);
			Pool.Free<GroupedLog>(ref groupedLog2);
			if (groupedLogs.Count != 0)
			{
				groupedLog = groupedLogs.Peek();
				continue;
			}
			break;
		}
	}

	public static void ResetTimer(BasePlayer ply)
	{
		ref PlayerState reference = ref NativeArray<PlayerState>.op_Implicit(ref PlayerStates)[ply.ActivePlayerInd];
		reference.LastViolationTime = Time.realtimeSinceStartup;
		reference.LastMovementViolationTime = Time.realtimeSinceStartup;
	}

	public static bool ShouldIgnore(BasePlayer ply)
	{
		using (TimeWarning.New("AntiHack.ShouldIgnore"))
		{
			ref PlayerState reference = ref NativeArray<PlayerState>.op_Implicit(ref PlayerStates)[ply.ActivePlayerInd];
			if (ply.IsFlying)
			{
				reference.LastAdminCheatTime = Time.realtimeSinceStartup;
			}
			else if ((ply.IsAdmin || ply.IsDeveloper) && reference.LastAdminCheatTime == 0f)
			{
				reference.LastAdminCheatTime = Time.realtimeSinceStartup;
			}
			if (ply.IsAdmin)
			{
				if (ConVar.AntiHack.userlevel < 1)
				{
					return true;
				}
				if (ConVar.AntiHack.admincheat && ply.UsedAdminCheat())
				{
					return true;
				}
			}
			if (ply.IsDeveloper)
			{
				if (ConVar.AntiHack.userlevel < 2)
				{
					return true;
				}
				if (ConVar.AntiHack.admincheat && ply.UsedAdminCheat())
				{
					return true;
				}
			}
			if (ply.IsSpectating())
			{
				return true;
			}
			if (ply.isInvisible)
			{
				return true;
			}
			return false;
		}
	}

	public static void InitInternalState(int initCap)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		DisposeInternalState();
		FindIndexWorkBuffer = new NativeArray<bool>(initCap, (Allocator)4, (NativeArrayOptions)0);
		ValidIndexAccum1 = new NativeList<int>(initCap, AllocatorHandle.op_Implicit((Allocator)4));
		ValidIndexAccum2 = new NativeList<int>(initCap, AllocatorHandle.op_Implicit((Allocator)4));
		InvalidIndices = new NativeList<int>(initCap, AllocatorHandle.op_Implicit((Allocator)4));
		From = new NativeList<Vector3>(initCap, AllocatorHandle.op_Implicit((Allocator)4));
		To = new NativeList<Vector3>(initCap, AllocatorHandle.op_Implicit((Allocator)4));
		Batches = new NativeList<Batch>(initCap, AllocatorHandle.op_Implicit((Allocator)4));
		LayerMasks = new NativeList<int>(initCap, AllocatorHandle.op_Implicit((Allocator)4));
		PlayerRadii = new NativeArray<float>(initCap, (Allocator)4, (NativeArrayOptions)0);
		ToOverlapIndices = new NativeList<int>(initCap, AllocatorHandle.op_Implicit((Allocator)4));
		Matrices = new NativeList<Matrix4x4>(initCap, AllocatorHandle.op_Implicit((Allocator)4));
		ToOverlapFrom = new NativeList<Vector3>(initCap, AllocatorHandle.op_Implicit((Allocator)4));
		ToOverlapTo = new NativeList<Vector3>(initCap, AllocatorHandle.op_Implicit((Allocator)4));
		ToOverlapLayerMasks = new NativeList<int>(initCap, AllocatorHandle.op_Implicit((Allocator)4));
		RaycastIndices = new NativeList<int>(initCap, AllocatorHandle.op_Implicit((Allocator)4));
		RaycastRays = new NativeList<RaycastCommand>(initCap, AllocatorHandle.op_Implicit((Allocator)4));
		RaycastSpheres = new NativeList<SpherecastCommand>(initCap, AllocatorHandle.op_Implicit((Allocator)4));
		TraceIndices = new NativeList<int>(initCap, AllocatorHandle.op_Implicit((Allocator)4));
		TraceRays = new NativeList<RaycastCommand>(initCap, AllocatorHandle.op_Implicit((Allocator)4));
		TraceSpheres = new NativeList<SpherecastCommand>(initCap, AllocatorHandle.op_Implicit((Allocator)4));
		RaycastHits = new NativeArray<RaycastHit>(initCap, (Allocator)4, (NativeArrayOptions)0);
		ColliderHits = new NativeArray<ColliderHit>(initCap, (Allocator)4, (NativeArrayOptions)0);
		TerrainIgnoreVolumeHits = new NativeArray<bool>(initCap, (Allocator)4, (NativeArrayOptions)0);
		QueryToBatchMap = new NativeArray<int>(initCap, (Allocator)4, (NativeArrayOptions)0);
		Colliders = new BufferList<Collider>(initCap);
		PlayerStates = new NativeArray<PlayerState>(initCap, (Allocator)4, (NativeArrayOptions)1);
		PlayerNoclipStates = new NativeArray<PlayerNoclipState>(initCap, (Allocator)4, (NativeArrayOptions)1);
		PlayerSpeedhackStates = new NativeArray<PlayerSpeedhackState>(initCap, (Allocator)4, (NativeArrayOptions)1);
		PlayerFlyhackStates = new NativeArray<PlayerFlyhackState>(initCap, (Allocator)4, (NativeArrayOptions)1);
	}

	public static void DisposeInternalState()
	{
		NativeArrayEx.SafeDispose(ref FindIndexWorkBuffer);
		NativeListEx.SafeDispose(ref ValidIndexAccum1);
		NativeListEx.SafeDispose(ref ValidIndexAccum2);
		NativeListEx.SafeDispose(ref InvalidIndices);
		From.SafeDispose<Vector3>();
		To.SafeDispose<Vector3>();
		NativeListEx.SafeDispose(ref Batches);
		NativeListEx.SafeDispose(ref LayerMasks);
		NativeArrayEx.SafeDispose(ref PlayerRadii);
		NativeListEx.SafeDispose(ref ToOverlapIndices);
		Matrices.SafeDispose<Matrix4x4>();
		ToOverlapFrom.SafeDispose<Vector3>();
		ToOverlapTo.SafeDispose<Vector3>();
		NativeListEx.SafeDispose(ref ToOverlapLayerMasks);
		NativeListEx.SafeDispose(ref RaycastIndices);
		RaycastRays.SafeDispose<RaycastCommand>();
		RaycastSpheres.SafeDispose<SpherecastCommand>();
		NativeListEx.SafeDispose(ref TraceIndices);
		TraceRays.SafeDispose<RaycastCommand>();
		TraceSpheres.SafeDispose<SpherecastCommand>();
		RaycastHits.SafeDispose<RaycastHit>();
		ColliderHits.SafeDispose<ColliderHit>();
		NativeArrayEx.SafeDispose(ref TerrainIgnoreVolumeHits);
		NativeArrayEx.SafeDispose(ref QueryToBatchMap);
		Colliders = null;
		NativeArrayEx.SafeDispose(ref PlayerStates);
		NativeArrayEx.SafeDispose(ref PlayerNoclipStates);
		NativeArrayEx.SafeDispose(ref PlayerSpeedhackStates);
		NativeArrayEx.SafeDispose(ref PlayerFlyhackStates);
	}

	public static void OnPlayerAddedToCache(BasePlayer player, StableObjectArray<BasePlayer> cache, int index)
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		NativeArrayEx.Expand(ref PlayerStates, cache.Capacity, (NativeArrayOptions)1);
		PlayerStates[index] = default(PlayerState);
		NativeArrayEx.Expand(ref PlayerNoclipStates, cache.Capacity, (NativeArrayOptions)1);
		PlayerNoclipStates[index] = default(PlayerNoclipState);
		NativeArrayEx.Expand(ref PlayerSpeedhackStates, cache.Capacity, (NativeArrayOptions)1);
		PlayerSpeedhackStates[index] = default(PlayerSpeedhackState);
		NativeArrayEx.Expand(ref PlayerFlyhackStates, cache.Capacity, (NativeArrayOptions)1);
		ref PlayerFlyhackState reference = ref NativeArray<PlayerFlyhackState>.op_Implicit(ref PlayerFlyhackStates)[index];
		reference = default(PlayerFlyhackState);
		reference.LastGroundedPosition = ((Component)player).transform.position;
	}

	public static void OnPlayerRemovedFromCache(BasePlayer player, int movedFrom, int movedTo)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (movedTo != movedFrom)
		{
			Debug.Assert(movedTo < movedFrom, "Unexpected swap indices, expecting to swap from end to earlier in range!");
			PlayerStates[movedTo] = PlayerStates[movedFrom];
			PlayerNoclipStates[movedTo] = PlayerNoclipStates[movedFrom];
			PlayerSpeedhackStates[movedTo] = PlayerSpeedhackStates[movedFrom];
			PlayerFlyhackStates[movedTo] = PlayerFlyhackStates[movedFrom];
		}
		BasePlayer.ResetAntiHack(player, PlayerStates, PlayerNoclipStates, PlayerSpeedhackStates, PlayerFlyhackStates);
	}

	internal static void ValidateMoves(in BasePlayer.PlayerServerStates.ReadOnly playerStates, ReadOnly<int> indices, NativeArray<BasePlayer.PositionChange> results)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0445: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		//IL_044f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0451: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_0528: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_053e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0570: Unknown result type (might be due to invalid IL or missing references)
		//IL_0575: Unknown result type (might be due to invalid IL or missing references)
		//IL_057a: Unknown result type (might be due to invalid IL or missing references)
		//IL_057f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0582: Unknown result type (might be due to invalid IL or missing references)
		//IL_0587: Unknown result type (might be due to invalid IL or missing references)
		//IL_0596: Unknown result type (might be due to invalid IL or missing references)
		//IL_0598: Unknown result type (might be due to invalid IL or missing references)
		//IL_0726: Unknown result type (might be due to invalid IL or missing references)
		//IL_072b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0730: Unknown result type (might be due to invalid IL or missing references)
		//IL_0732: Unknown result type (might be due to invalid IL or missing references)
		//IL_0737: Unknown result type (might be due to invalid IL or missing references)
		//IL_0739: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0764: Unknown result type (might be due to invalid IL or missing references)
		//IL_0769: Unknown result type (might be due to invalid IL or missing references)
		//IL_0610: Unknown result type (might be due to invalid IL or missing references)
		//IL_0619: Unknown result type (might be due to invalid IL or missing references)
		//IL_0636: Unknown result type (might be due to invalid IL or missing references)
		//IL_063b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0658: Unknown result type (might be due to invalid IL or missing references)
		//IL_0669: Unknown result type (might be due to invalid IL or missing references)
		//IL_066e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0673: Unknown result type (might be due to invalid IL or missing references)
		//IL_0675: Unknown result type (might be due to invalid IL or missing references)
		//IL_068e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0693: Unknown result type (might be due to invalid IL or missing references)
		//IL_0698: Unknown result type (might be due to invalid IL or missing references)
		//IL_069c: Unknown result type (might be due to invalid IL or missing references)
		//IL_069e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0786: Unknown result type (might be due to invalid IL or missing references)
		//IL_078b: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_085f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0864: Unknown result type (might be due to invalid IL or missing references)
		//IL_0869: Unknown result type (might be due to invalid IL or missing references)
		//IL_086b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0870: Unknown result type (might be due to invalid IL or missing references)
		//IL_0872: Unknown result type (might be due to invalid IL or missing references)
		//IL_0893: Unknown result type (might be due to invalid IL or missing references)
		//IL_0898: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0800: Unknown result type (might be due to invalid IL or missing references)
		//IL_080f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0811: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("AntiHack.ValidateMoves"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			NativeListEx.Expand(ref ValidIndexAccum1, indices.Length, copyContents: false);
			NativeListEx.Expand(ref ValidIndexAccum2, indices.Length, copyContents: false);
			ValidIndexAccum1.Clear();
			ValidIndexAccum2.Clear();
			using (TimeWarning.New("ShouldIgnore"))
			{
				Enumerator<int> enumerator = indices.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						int current = enumerator.Current;
						if (ShouldIgnore(objects[current]))
						{
							results[current] = BasePlayer.PositionChange.Valid;
						}
						else
						{
							ValidIndexAccum1.Add(ref current);
						}
					}
				}
				finally
				{
					((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
				}
			}
			NativeListEx.Expand(ref InvalidIndices, ValidIndexAccum1.Length, copyContents: false);
			InvalidIndices.Clear();
			if (Colliders.Capacity < objects.Length)
			{
				Colliders.Resize(objects.Length);
			}
			AreNoClipping(in playerStates, PlayerNoclipStates, ValidIndexAccum1.AsReadOnly(), InvalidIndices, Colliders.Buffer);
			NativeArrayEx.Expand(ref FindIndexWorkBuffer, objects.Length, (NativeArrayOptions)0, copyContents: false);
			FindValidIndicesJob findValidIndicesJob = new FindValidIndicesJob
			{
				ValidIndices = ValidIndexAccum2,
				WorkBuffer = FindIndexWorkBuffer,
				InvalidIndices = InvalidIndices.AsReadOnly(),
				AllIndices = ValidIndexAccum1.AsReadOnly()
			};
			IJobExtensions.RunByRef<FindValidIndicesJob>(ref findValidIndicesJob);
			TickInterpolatorCache.ReadOnlyState tickCache = playerStates.TickCache;
			using (TimeWarning.New("NoClipRejections"))
			{
				Enumerator<int> enumerator2 = InvalidIndices.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						int current2 = enumerator2.Current;
						if (playerStates.TickDeltaTime[current2] > ConVar.AntiHack.maxdeltatime)
						{
							results[current2] = BasePlayer.PositionChange.Invalid;
							continue;
						}
						BasePlayer obj = objects[current2];
						Vector3 startPoint = TickInterpolatorCache.GetStartPoint(tickCache, current2);
						Vector3 endPoint = TickInterpolatorCache.GetEndPoint(tickCache, current2);
						TickInterpolatorCache.PlayerInfo playerInfo = tickCache.Infos[current2];
						Facepunch.Rust.Analytics.Azure.OnNoclipViolation(obj, startPoint, endPoint, playerInfo.Count, Colliders[current2]);
						AddViolation(obj, AntiHackType.NoClip, ConVar.AntiHack.noclip_penalty * playerInfo.Length, ((Component)Colliders[current2]).gameObject);
						if (ConVar.AntiHack.noclip_reject)
						{
							results[current2] = BasePlayer.PositionChange.Invalid;
						}
					}
				}
				finally
				{
					((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
				}
			}
			Array.Clear(Colliders.Buffer, 0, Colliders.Capacity);
			NativeList<int> validIndexAccum = ValidIndexAccum2;
			NativeList<int> validIndexAccum2 = ValidIndexAccum1;
			ValidIndexAccum1 = validIndexAccum;
			ValidIndexAccum2 = validIndexAccum2;
			ValidIndexAccum2.Clear();
			InvalidIndices.Clear();
			if (ValidIndexAccum1.Length > 0)
			{
				NativeArray<bool> results2 = default(NativeArray<bool>);
				results2._002Ector(ValidIndexAccum1.Length, (Allocator)3, (NativeArrayOptions)0);
				try
				{
					AreSpeeding(in playerStates, PlayerSpeedhackStates, ValidIndexAccum1.AsReadOnly(), results2);
					for (int i = 0; i < ValidIndexAccum1.Length; i++)
					{
						int num = ValidIndexAccum1[i];
						if (results2[i])
						{
							InvalidIndices.Add(ref num);
						}
						else
						{
							ValidIndexAccum2.Add(ref num);
						}
					}
				}
				finally
				{
					((IDisposable)results2/*cast due to constrained. prefix*/).Dispose();
				}
			}
			using (TimeWarning.New("IsSpeedingRejections"))
			{
				Enumerator<int> enumerator2 = InvalidIndices.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						int current3 = enumerator2.Current;
						if (playerStates.TickDeltaTime[current3] > ConVar.AntiHack.maxdeltatime)
						{
							results[current3] = BasePlayer.PositionChange.Invalid;
							continue;
						}
						BasePlayer obj2 = objects[current3];
						Vector3 startPoint2 = TickInterpolatorCache.GetStartPoint(tickCache, current3);
						Vector3 endPoint2 = TickInterpolatorCache.GetEndPoint(tickCache, current3);
						TickInterpolatorCache.PlayerInfo playerInfo2 = tickCache.Infos[current3];
						Facepunch.Rust.Analytics.Azure.OnSpeedhackViolation(obj2, startPoint2, endPoint2, playerInfo2.Count);
						AddViolation(obj2, AntiHackType.SpeedHack, ConVar.AntiHack.speedhack_penalty * playerInfo2.Length);
						if (ConVar.AntiHack.speedhack_reject)
						{
							results[current3] = BasePlayer.PositionChange.Invalid;
						}
					}
				}
				finally
				{
					((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
				}
			}
			NativeList<int> validIndexAccum3 = ValidIndexAccum2;
			validIndexAccum2 = ValidIndexAccum1;
			ValidIndexAccum1 = validIndexAccum3;
			ValidIndexAccum2 = validIndexAccum2;
			ValidIndexAccum2.Clear();
			InvalidIndices.Clear();
			if (ValidIndexAccum1.Length > 0)
			{
				NativeArray<bool> results3 = default(NativeArray<bool>);
				results3._002Ector(ValidIndexAccum1.Length, (Allocator)3, (NativeArrayOptions)0);
				try
				{
					AreFlying(in playerStates, PlayerStates.AsReadOnly(), PlayerFlyhackStates, ValidIndexAccum1.AsReadOnly(), results3);
					for (int j = 0; j < ValidIndexAccum1.Length; j++)
					{
						int num2 = ValidIndexAccum1[j];
						if (results3[j])
						{
							InvalidIndices.Add(ref num2);
						}
						else
						{
							ValidIndexAccum2.Add(ref num2);
						}
					}
				}
				finally
				{
					((IDisposable)results3/*cast due to constrained. prefix*/).Dispose();
				}
			}
			using (TimeWarning.New("IsFlyingRejections"))
			{
				Enumerator<int> enumerator2 = InvalidIndices.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						int current4 = enumerator2.Current;
						if (playerStates.TickDeltaTime[current4] > ConVar.AntiHack.maxdeltatime)
						{
							results[current4] = BasePlayer.PositionChange.Invalid;
							continue;
						}
						BasePlayer basePlayer = objects[current4];
						Vector3 startPoint3 = TickInterpolatorCache.GetStartPoint(tickCache, current4);
						Vector3 endPoint3 = TickInterpolatorCache.GetEndPoint(tickCache, current4);
						TickInterpolatorCache.PlayerInfo playerInfo3 = tickCache.Infos[current4];
						Facepunch.Rust.Analytics.Azure.OnFlyhackViolation(basePlayer, startPoint3, endPoint3, playerInfo3.Count);
						AddViolation(basePlayer, AntiHackType.FlyHack, ConVar.AntiHack.flyhack_penalty * playerInfo3.Length);
						if (!ConVar.AntiHack.flyhack_reject)
						{
							continue;
						}
						results[current4] = BasePlayer.PositionChange.Invalid;
						Vector3 lastGroundedPosition = PlayerFlyhackStates[current4].LastGroundedPosition;
						if (lastGroundedPosition == default(Vector3) && basePlayer.IsConnected)
						{
							ValidIndexAccum2.Add(ref current4);
						}
						else if (Vector3.Distance(lastGroundedPosition, ((Component)basePlayer).transform.position) <= 10f)
						{
							Collider col;
							bool num3 = TestNoClipping(basePlayer, ((Component)basePlayer).transform.position, lastGroundedPosition, BasePlayer.NoClipRadius(ConVar.AntiHack.noclip_margin), ConVar.AntiHack.noclip_backtracking, out col);
							Vector3 val = lastGroundedPosition + new Vector3(0f, BasePlayer.GetRadius(), 0f);
							Vector3 val2 = lastGroundedPosition + new Vector3(0f, basePlayer.GetHeight() - BasePlayer.GetRadius(), 0f);
							if (!num3 && !Physics.CheckCapsule(val, val2, BasePlayer.GetRadius(), 1537286401))
							{
								basePlayer.MovePosition(lastGroundedPosition);
								basePlayer.ClientRPC(RpcTarget.Player("ForcePositionTo", basePlayer), ((Component)basePlayer).transform.position);
								NativeArray<PlayerState>.op_Implicit(ref PlayerStates)[basePlayer.ActivePlayerInd].ViolationLevel = 0f;
							}
						}
					}
				}
				finally
				{
					((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
				}
			}
			NativeList<int> validIndexAccum4 = ValidIndexAccum2;
			validIndexAccum2 = ValidIndexAccum1;
			ValidIndexAccum1 = validIndexAccum4;
			ValidIndexAccum2 = validIndexAccum2;
			ValidIndexAccum2.Clear();
			InvalidIndices.Clear();
			using (TimeWarning.New("TickOverflowValidation"))
			{
				Enumerator<int> enumerator2 = ValidIndexAccum1.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						int current5 = enumerator2.Current;
						BasePlayer basePlayer2 = objects[current5];
						if (playerStates.TickDeltaTime[current5] < ConVar.AntiHack.tick_buffer_server_lag_threshold && ConVar.AntiHack.tick_buffer_preventions && (float)basePlayer2.rawTickCount >= ConVar.AntiHack.tick_buffer_reject_threshold * (float)Player.tickrate_cl)
						{
							Log(basePlayer2, AntiHackType.Ticks, $"Player had too many ticks buffered ({basePlayer2.rawTickCount})", logToAnalytics: false);
							Vector3 startPoint4 = TickInterpolatorCache.GetStartPoint(tickCache, current5);
							Vector3 endPoint4 = TickInterpolatorCache.GetEndPoint(tickCache, current5);
							Facepunch.Rust.Analytics.Azure.OnTickViolation(basePlayer2, startPoint4, endPoint4, tickCache.Infos[current5].Count);
							results[current5] = BasePlayer.PositionChange.Invalid;
						}
						else
						{
							ValidIndexAccum2.Add(ref current5);
						}
					}
				}
				finally
				{
					((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
				}
			}
			NativeList<int> validIndexAccum5 = ValidIndexAccum2;
			validIndexAccum2 = ValidIndexAccum1;
			ValidIndexAccum1 = validIndexAccum5;
			ValidIndexAccum2 = validIndexAccum2;
			ValidIndexAccum2.Clear();
			using (TimeWarning.New("MarkPositionsValid"))
			{
				Enumerator<int> enumerator2 = ValidIndexAccum1.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						int current6 = enumerator2.Current;
						results[current6] = BasePlayer.PositionChange.Valid;
					}
				}
				finally
				{
					((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
				}
			}
		}
	}

	public static void ValidateAgainstTerrain(in BasePlayer.PlayerServerStates.ReadOnly playerStates)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ValidateAgainstTerrain"))
		{
			int num = Time.frameCount % ConVar.AntiHack.terrain_timeslice;
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			for (int i = 0; i < objects.Length; i++)
			{
				BasePlayer basePlayer = objects[i];
				int num2 = (int)(basePlayer.net.ID.Value % (ulong)ConVar.AntiHack.terrain_timeslice);
				if (num == num2 && !ShouldIgnore(basePlayer))
				{
					bool flag = false;
					if (IsInsideTerrain(basePlayer))
					{
						flag = true;
						AddViolation(basePlayer, AntiHackType.InsideTerrain, ConVar.AntiHack.terrain_penalty);
					}
					else if (ConVar.AntiHack.terrain_check_geometry && IsInsideMesh(basePlayer.eyes.position))
					{
						flag = true;
						AddViolation(basePlayer, AntiHackType.InsideGeometry, ConVar.AntiHack.terrain_penalty);
						Log(basePlayer, AntiHackType.InsideGeometry, "Seems to be clipped inside " + ((Object)((RaycastHit)(ref isInsideRayHit)).collider).name);
					}
					if (flag && ConVar.AntiHack.terrain_kill)
					{
						Facepunch.Rust.Analytics.Azure.OnTerrainHackViolation(basePlayer);
						basePlayer.Hurt(1000f, DamageType.Suicide, basePlayer, useProtection: false);
					}
				}
			}
		}
	}

	public static void ValidateEyeHistory(BasePlayer ply)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("AntiHack.ValidateEyeHistory"))
		{
			for (int i = 0; i < ply.eyeHistory.Count; i++)
			{
				Vector3 val = ply.eyeHistory[i];
				if (ply.tickHistory.Distance(ply, val) > ConVar.AntiHack.eye_history_forgiveness)
				{
					AddViolation(ply, AntiHackType.EyeHack, ConVar.AntiHack.eye_history_penalty);
					Facepunch.Rust.Analytics.Azure.OnEyehackViolation(ply, val);
				}
			}
			ply.eyeHistory.Clear();
		}
	}

	public static bool IsInsideTerrain(BasePlayer ply)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (ply.IsInTutorial || DeepSeaManager.IsInsideDeepSea((BaseNetworkable)ply))
		{
			return false;
		}
		return TestInsideTerrain(((Component)ply).transform.position);
	}

	public static bool TestInsideTerrain(Vector3 pos)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("AntiHack.TestInsideTerrain"))
		{
			if (!TerrainMeta.TerrainRenderer)
			{
				return false;
			}
			if (!Object.op_Implicit((Object)(object)TerrainMeta.HeightMap))
			{
				return false;
			}
			if (!Object.op_Implicit((Object)(object)TerrainMeta.Collision))
			{
				return false;
			}
			float terrain_padding = ConVar.AntiHack.terrain_padding;
			float height = TerrainMeta.HeightMap.GetHeight(pos);
			if (pos.y > height - terrain_padding)
			{
				return false;
			}
			float num = TerrainMeta.SampleTerrainMeshHeight(pos);
			if (pos.y > num - terrain_padding)
			{
				return false;
			}
			if (TerrainMeta.Collision.GetIgnore(pos))
			{
				return false;
			}
			return true;
		}
	}

	public static void TestInsideTerrain(ReadOnly<Vector3> posi, NativeArray<bool> results)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("AntiHack.TestInsideTerrain"))
		{
			if (!TerrainMeta.TerrainRenderer || !Object.op_Implicit((Object)(object)TerrainMeta.HeightMap) || !Object.op_Implicit((Object)(object)TerrainMeta.Collision))
			{
				for (int i = 0; i < results.Length; i++)
				{
					results[i] = false;
				}
				return;
			}
			NativeArray<float> results2 = new NativeArray<float>(posi.Length, (Allocator)3, (NativeArrayOptions)0);
			JobHandle heights = TerrainMeta.HeightMap.GetHeights(posi, results2);
			JobHandle.ScheduleBatchedJobs();
			NativeArray<float> val = new NativeArray<float>(posi.Length, (Allocator)3, (NativeArrayOptions)0);
			for (int j = 0; j < posi.Length; j++)
			{
				val[j] = TerrainMeta.SampleTerrainMeshHeight(posi[j]);
			}
			((JobHandle)(ref heights)).Complete();
			NativeList<int> indicesToCheck = new NativeList<int>(posi.Length, AllocatorHandle.op_Implicit((Allocator)3));
			NativeArray<Vector3> posiToCheck = new NativeArray<Vector3>(posi.Length, (Allocator)3, (NativeArrayOptions)0);
			NativeArray<float> radiiToCheck = new NativeArray<float>(posi.Length, (Allocator)3, (NativeArrayOptions)0);
			InsideTerrainHeightsChecksJob insideTerrainHeightsChecksJob = new InsideTerrainHeightsChecksJob
			{
				Results = results,
				IndicesToCheck = indicesToCheck,
				PosiToCheck = posiToCheck,
				RadiiToCheck = radiiToCheck,
				Posi = posi,
				HeightMapHeights = results2.AsReadOnly(),
				TerrainHeights = val.AsReadOnly(),
				TerrainPadding = ConVar.AntiHack.terrain_padding,
				RadiusToCheck = 0.01f
			};
			IJobExtensions.RunByRef<InsideTerrainHeightsChecksJob>(ref insideTerrainHeightsChecksJob);
			results2.Dispose();
			val.Dispose();
			if (!indicesToCheck.IsEmpty)
			{
				NativeArray<Vector3> subArray = posiToCheck.GetSubArray(0, indicesToCheck.Length);
				NativeArray<float> subArray2 = radiiToCheck.GetSubArray(0, indicesToCheck.Length);
				NativeArray<bool> results3 = default(NativeArray<bool>);
				results3._002Ector(indicesToCheck.Length, (Allocator)3, (NativeArrayOptions)0);
				TerrainMeta.Collision.GetIgnore(subArray.AsReadOnly(), subArray2.AsReadOnly(), results3);
				ScatterInvertedBool scatterInvertedBool = new ScatterInvertedBool
				{
					To = results,
					From = results3.AsReadOnly(),
					Indices = indicesToCheck.AsReadOnly()
				};
				IJobExtensions.RunByRef<ScatterInvertedBool>(ref scatterInvertedBool);
				results3.Dispose();
			}
			radiiToCheck.Dispose();
			posiToCheck.Dispose();
			indicesToCheck.Dispose();
		}
	}

	public static bool IsInsideMesh(Vector3 pos)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		if (ConVar.AntiHack.mesh_inside_check_distance <= 0f)
		{
			return false;
		}
		bool queriesHitBackfaces = Physics.queriesHitBackfaces;
		if (ConVar.AntiHack.use_legacy_mesh_inside_check)
		{
			Physics.queriesHitBackfaces = true;
			if (Physics.Raycast(pos, Vector3.up, ref isInsideRayHit, ConVar.AntiHack.mesh_inside_check_distance, 65536))
			{
				Physics.queriesHitBackfaces = queriesHitBackfaces;
				return Vector3.Dot(Vector3.up, ((RaycastHit)(ref isInsideRayHit)).normal) > 0f;
			}
			Physics.queriesHitBackfaces = queriesHitBackfaces;
			return false;
		}
		Physics.queriesHitBackfaces = true;
		int num = Physics.RaycastNonAlloc(pos, Vector3.up, isInsideMeshRaycastHits, ConVar.AntiHack.mesh_inside_check_distance, 65536);
		Physics.queriesHitBackfaces = queriesHitBackfaces;
		SortHitsByDistance(isInsideMeshRaycastHits, num);
		Collider val = null;
		ColliderInfo colliderInfo = default(ColliderInfo);
		for (int i = 0; i < num; i++)
		{
			RaycastHit val2 = isInsideMeshRaycastHits[i];
			if (((Component)((RaycastHit)(ref val2)).collider).TryGetComponent<ColliderInfo>(ref colliderInfo) && colliderInfo.HasFlag(ColliderInfo.Flags.AllowBuildInsideMesh))
			{
				continue;
			}
			if (Vector3.Dot(Vector3.up, ((RaycastHit)(ref val2)).normal) > 0f)
			{
				if ((Object)(object)val != (Object)(object)((RaycastHit)(ref val2)).collider)
				{
					isInsideRayHit = val2;
					return true;
				}
			}
			else
			{
				val = ((RaycastHit)(ref val2)).collider;
			}
		}
		return false;
	}

	public static void AreInsideMesh(ReadOnly<Vector3> posi, NativeArray<bool> results)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		if (ConVar.AntiHack.mesh_inside_check_distance <= 0f)
		{
			for (int i = 0; i < results.Length; i++)
			{
				results[i] = false;
			}
			return;
		}
		NativeArray<RaycastCommand> val = default(NativeArray<RaycastCommand>);
		val._002Ector(posi.Length, (Allocator)3, (NativeArrayOptions)0);
		GenerateInsideMeshCommandsJob obj = new GenerateInsideMeshCommandsJob
		{
			Commands = val,
			Posi = posi,
			Distance = ConVar.AntiHack.mesh_inside_check_distance
		};
		int batchSize = GamePhysics.GetBatchSize(posi.Length);
		int length = posi.Length;
		JobHandle val2 = default(JobHandle);
		val2 = IJobForExtensions.ScheduleParallel<GenerateInsideMeshCommandsJob>(obj, length, batchSize, val2);
		((JobHandle)(ref val2)).Complete();
		NativeArray<RaycastHit> val3 = default(NativeArray<RaycastHit>);
		val3._002Ector(posi.Length, (Allocator)3, (NativeArrayOptions)0);
		int batchSize2 = GamePhysics.GetBatchSize(val.Length);
		NativeArray<RaycastCommand> val4 = val;
		NativeArray<RaycastHit> val5 = val3;
		val2 = default(JobHandle);
		JobHandle val6 = RaycastCommand.ScheduleBatch(val4, val5, batchSize2, val2);
		((JobHandle)(ref val6)).Complete();
		val.Dispose();
		CheckInsideMeshHitsJob obj2 = new CheckInsideMeshHitsJob
		{
			Results = results,
			Hits = val3.AsReadOnly()
		};
		int length2 = posi.Length;
		val2 = default(JobHandle);
		val2 = IJobForExtensions.ScheduleParallel<CheckInsideMeshHitsJob>(obj2, length2, batchSize, val2);
		((JobHandle)(ref val2)).Complete();
		val3.Dispose();
	}

	public static void AreInsideMesh(ReadOnly<Vector3> posi, NativeArray<RaycastHit> hits)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (ConVar.AntiHack.mesh_inside_check_distance <= 0f)
		{
			for (int i = 0; i < hits.Length; i++)
			{
				hits[i] = default(RaycastHit);
			}
			return;
		}
		NativeArray<RaycastCommand> val = default(NativeArray<RaycastCommand>);
		val._002Ector(posi.Length, (Allocator)3, (NativeArrayOptions)0);
		GenerateInsideMeshCommandsJob obj = new GenerateInsideMeshCommandsJob
		{
			Commands = val,
			Posi = posi,
			Distance = ConVar.AntiHack.mesh_inside_check_distance
		};
		int batchSize = GamePhysics.GetBatchSize(posi.Length);
		int length = posi.Length;
		JobHandle val2 = default(JobHandle);
		val2 = IJobForExtensions.ScheduleParallel<GenerateInsideMeshCommandsJob>(obj, length, batchSize, val2);
		((JobHandle)(ref val2)).Complete();
		int batchSize2 = GamePhysics.GetBatchSize(val.Length);
		NativeArray<RaycastCommand> val3 = val;
		NativeArray<RaycastHit> val4 = hits;
		val2 = default(JobHandle);
		JobHandle val5 = RaycastCommand.ScheduleBatch(val3, val4, batchSize2, val2);
		((JobHandle)(ref val5)).Complete();
		val.Dispose();
		FilterInsideMeshHitsJob obj2 = new FilterInsideMeshHitsJob
		{
			Hits = hits
		};
		int length2 = posi.Length;
		val2 = default(JobHandle);
		val2 = IJobForExtensions.ScheduleParallel<FilterInsideMeshHitsJob>(obj2, length2, batchSize, val2);
		((JobHandle)(ref val2)).Complete();
	}

	private static void SortHitsByDistance(RaycastHit[] hits, int maxLength)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < maxLength - 1; i++)
		{
			int num = i;
			for (int j = i + 1; j < maxLength; j++)
			{
				if (((RaycastHit)(ref hits[j])).distance < ((RaycastHit)(ref hits[num])).distance)
				{
					num = j;
				}
			}
			if (num != i)
			{
				RaycastHit val = hits[i];
				hits[i] = hits[num];
				hits[num] = val;
			}
		}
	}

	public static void AreNoClipping(in BasePlayer.PlayerServerStates.ReadOnly playerStates, NativeArray<PlayerNoclipState> noclipStates, ReadOnly<int> indices, NativeList<int> foundIndices, Span<Collider> colliders)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("AntiHack.AreNoClipping"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			Span<PlayerNoclipState> span = NativeArray<PlayerNoclipState>.op_Implicit(ref noclipStates);
			Enumerator<int> enumerator = indices.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					float num = playerStates.TickDeltaTime[current];
					ref PlayerNoclipState reference = ref span[current];
					reference.VehiclePauseTime = Mathf.Max(0f, reference.VehiclePauseTime - num);
					reference.ForceCastTime = Mathf.Max(0f, reference.ForceCastTime - num);
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			if (ConVar.AntiHack.noclip_protection <= 0)
			{
				return;
			}
			int num2 = Mathf.Max(ConVar.AntiHack.noclip_maxsteps, 1);
			using (TimeWarning.New("GatherBatches"))
			{
				NativeListEx.Expand(ref ToOverlapIndices, indices.Length, copyContents: false);
				GatherPlayersWithTicksJob gatherPlayersWithTicksJob = new GatherPlayersWithTicksJob
				{
					ValidIndices = ToOverlapIndices,
					TickCache = playerStates.TickCache,
					Indices = indices
				};
				IJobExtensions.RunByRef<GatherPlayersWithTicksJob>(ref gatherPlayersWithTicksJob);
				using (TimeWarning.New("GatherPlayerInfo"))
				{
					NativeListEx.Expand(ref Batches, ToOverlapIndices.Length, copyContents: false);
					Matrices.Expand<Matrix4x4>(ToOverlapIndices.Length, false);
					Enumerator<int> enumerator2 = ToOverlapIndices.GetEnumerator();
					try
					{
						while (enumerator2.MoveNext())
						{
							int current2 = enumerator2.Current;
							BasePlayer basePlayer = objects[current2];
							Transform parent = ((Component)basePlayer).transform.parent;
							Matrix4x4 val = (((Object)(object)parent == (Object)null) ? Matrix4x4.zero : parent.localToWorldMatrix);
							Matrices.AddNoResize(val);
							PlayerNoclipState playerNoclipState = noclipStates[current2];
							bool flag = playerNoclipState.VehiclePauseTime <= 0f && !basePlayer.isMounted;
							bool force = playerNoclipState.ForceCastTime > 0f;
							bool skipVehicleLayer = false;
							Batches.AddNoResize(new Batch
							{
								PlayerIndex = current2,
								Count = (int)basePlayer.rawTickCount,
								Force = force,
								CastVehicleLayer = !flag,
								SkipVehicleLayer = skipVehicleLayer
							});
						}
					}
					finally
					{
						((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
					}
				}
				From.Expand<Vector3>(ToOverlapIndices.Length * num2, false);
				To.Expand<Vector3>(ToOverlapIndices.Length * num2, false);
				GatherNoClipBatchesJob gatherNoClipBatchesJob = new GatherNoClipBatchesJob
				{
					From = From,
					To = To,
					Batches = Batches.AsArray(),
					TickCache = playerStates.TickCache,
					Indices = ToOverlapIndices.AsReadOnly(),
					Matrices = Matrices.AsReadOnly(),
					DeltaTimes = playerStates.TickDeltaTime,
					MaxSteps = num2,
					DefaultStepSize = Mathf.Max(ConVar.AntiHack.noclip_stepsize, 0.1f),
					DefaultProtection = ConVar.AntiHack.noclip_protection,
					MaxTickCount = ConVar.AntiHack.tick_buffer_noclip_threshold * (float)Player.tickrate_cl,
					LagThreshold = ConVar.AntiHack.tick_buffer_server_lag_threshold,
					TickBufferPrevention = ConVar.AntiHack.tick_buffer_preventions
				};
				IJobExtensions.RunByRef<GatherNoClipBatchesJob>(ref gatherNoClipBatchesJob);
			}
			foundIndices.Clear();
			if (!Batches.IsEmpty)
			{
				TestAreNoClipping(in playerStates, From.AsReadOnly(), To.AsReadOnly(), Batches.AsReadOnly(), foundIndices, colliders);
			}
		}
	}

	public static void AreSpeeding(in BasePlayer.PlayerServerStates.ReadOnly playerStates, NativeArray<PlayerSpeedhackState> speedStateCache, ReadOnly<int> indices, NativeArray<bool> results)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0433: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Unknown result type (might be due to invalid IL or missing references)
		//IL_0446: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		//IL_045c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0461: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_046a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0496: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_050c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0511: Unknown result type (might be due to invalid IL or missing references)
		//IL_051c: Unknown result type (might be due to invalid IL or missing references)
		//IL_051e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_0527: Unknown result type (might be due to invalid IL or missing references)
		//IL_0529: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("AntiHack.AreSpeeding"))
		{
			ProgressSpeedingStatesJob progressSpeedingStatesJob = new ProgressSpeedingStatesJob
			{
				SpeedStates = speedStateCache,
				DeltaTime = playerStates.TickDeltaTime,
				Indices = indices
			};
			int length = indices.Length;
			JobHandle val = default(JobHandle);
			JobHandle val2 = IJobForExtensions.ScheduleByRef<ProgressSpeedingStatesJob>(ref progressSpeedingStatesJob, length, val);
			if (ConVar.AntiHack.speedhack_protection == 0)
			{
				FillJob<bool> fillJob = new FillJob<bool>
				{
					Value = false,
					Values = results
				};
				val = IJobExtensions.ScheduleByRef<FillJob<bool>>(ref fillJob, val2);
				((JobHandle)(ref val)).Complete();
				return;
			}
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			NativeArray<Matrix4x4> val3 = new NativeArray<Matrix4x4>(indices.Length, (Allocator)3, (NativeArrayOptions)0);
			for (int i = 0; i < indices.Length; i++)
			{
				int index = indices[i];
				BasePlayer basePlayer = objects[index];
				bool flag = (Object)(object)((Component)basePlayer).transform.parent == (Object)null;
				val3[i] = (flag ? Matrix4x4.zero : ((Component)basePlayer).transform.parent.localToWorldMatrix);
			}
			NativeArray<Vector3> starts = new NativeArray<Vector3>(objects.Length, (Allocator)3, (NativeArrayOptions)0);
			NativeArray<Vector3> ends = new NativeArray<Vector3>(objects.Length, (Allocator)3, (NativeArrayOptions)0);
			TransformStartEndTicksJob transformStartEndTicksJob = new TransformStartEndTicksJob
			{
				Starts = starts,
				Ends = ends,
				Matrices = val3.AsReadOnly(),
				TickCache = playerStates.TickCache,
				Indices = indices
			};
			int length2 = indices.Length;
			val = default(JobHandle);
			JobHandle val4 = IJobForExtensions.ScheduleByRef<TransformStartEndTicksJob>(ref transformStartEndTicksJob, length2, val);
			val3.Dispose(val4);
			NativeArray<RDC> val5 = new NativeArray<RDC>(objects.Length, (Allocator)3, (NativeArrayOptions)0);
			JobHandle val6;
			if (ConVar.AntiHack.speedhack_protection >= 2)
			{
				CalculateRDCsJob calculateRDCsJob = new CalculateRDCsJob
				{
					RDCs = val5,
					States = playerStates.CachedStates,
					MsFlags = playerStates.PlayerModelStateFlags,
					Ducking = playerStates.PlayerModelStateDucking,
					Indices = indices
				};
				int length3 = indices.Length;
				val = default(JobHandle);
				val6 = IJobForExtensions.ScheduleByRef<CalculateRDCsJob>(ref calculateRDCsJob, length3, val);
			}
			else
			{
				FillJob<RDC> fillJob2 = new FillJob<RDC>
				{
					Values = val5,
					Value = new RDC
					{
						Running = 1f,
						Ducking = 0f,
						Crawling = 0f
					}
				};
				val = default(JobHandle);
				val6 = IJobExtensions.ScheduleByRef<FillJob<RDC>>(ref fillJob2, val);
			}
			NativeArray<float> speed = new NativeArray<float>(indices.Length, (Allocator)3, (NativeArrayOptions)0);
			CalcSpeedHackSpeedJob calcSpeedHackSpeedJob = new CalcSpeedHackSpeedJob
			{
				Speed = speed,
				States = playerStates.CachedStates,
				RDCs = val5.AsReadOnly(),
				Indices = indices,
				WaterThreshold = ConVar.AntiHack.speedhack_water_threshold
			};
			JobHandle val7 = IJobForExtensions.ScheduleByRef<CalcSpeedHackSpeedJob>(ref calcSpeedHackSpeedJob, indices.Length, val6);
			val5.Dispose(val7);
			NativeArray<(float, float)> distAndBudget = new NativeArray<(float, float)>(objects.Length, (Allocator)3, (NativeArrayOptions)0);
			NativeList<int> indicesForNormalSample = new NativeList<int>(indices.Length, AllocatorHandle.op_Implicit((Allocator)3));
			CalcDistAndBudgetJob calcDistAndBudgetJob = new CalcDistAndBudgetJob
			{
				DistAndBudget = distAndBudget,
				IndicesForNormalSample = indicesForNormalSample,
				Start = starts.AsReadOnly(),
				End = ends.AsReadOnly(),
				Speed = speed.AsReadOnly(),
				DeltaTime = playerStates.TickDeltaTime,
				States = playerStates.CachedStates,
				Indices = indices,
				Use3DMagnitude = (ConVar.AntiHack.speedhack_protection >= 3)
			};
			JobHandle val8 = JobHandle.CombineDependencies(val4, val7);
			JobHandle val9 = IJobForExtensions.ScheduleByRef<CalcDistAndBudgetJob>(ref calcDistAndBudgetJob, indices.Length, val8);
			speed.Dispose(val9);
			NativeArray<Vector3> results2 = new NativeArray<Vector3>(objects.Length, (Allocator)3, (NativeArrayOptions)0);
			JobHandle val10;
			if ((Object)(object)TerrainMeta.HeightMap != (Object)null)
			{
				val10 = TerrainMeta.HeightMap.GetNormalsIndirect(starts.AsReadOnly(), results2, indicesForNormalSample.AsDeferredJobArray(), val9);
			}
			else
			{
				ScatterValueToJobDeferred<Vector3> scatterValueToJobDeferred = new ScatterValueToJobDeferred<Vector3>
				{
					Results = results2,
					Value = Vector3.up,
					Indices = indicesForNormalSample.AsDeferredJobArray()
				};
				val10 = IJobExtensions.ScheduleByRef<ScatterValueToJobDeferred<Vector3>>(ref scatterValueToJobDeferred, val9);
			}
			AdjustDistBasedOnNormalsJob adjustDistBasedOnNormalsJob = new AdjustDistBasedOnNormalsJob
			{
				DistAndBudget = distAndBudget,
				Start = starts.AsReadOnly(),
				End = ends.AsReadOnly(),
				Normals = results2.AsReadOnly(),
				DeltaTime = playerStates.TickDeltaTime,
				Indices = indicesForNormalSample.AsDeferredJobArray().AsReadOnly(),
				SlopeSpeed = ConVar.AntiHack.speedhack_slopespeed
			};
			JobHandle val11 = IJobExtensions.ScheduleByRef<AdjustDistBasedOnNormalsJob>(ref adjustDistBasedOnNormalsJob, val10);
			results2.Dispose(val11);
			indicesForNormalSample.Dispose(val11);
			starts.Dispose(val11);
			ends.Dispose(val11);
			TestAreSpeedingJob testAreSpeedingJob = new TestAreSpeedingJob
			{
				Results = results,
				PlayerStates = speedStateCache,
				DistAndBudget = distAndBudget.AsReadOnly(),
				DeltaTime = playerStates.TickDeltaTime,
				Indices = indices,
				ForgivenessInertia = ConVar.AntiHack.speedhack_forgiveness_inertia,
				Forgiveness = ConVar.AntiHack.speedhack_forgiveness
			};
			JobHandle val12 = JobHandle.CombineDependencies(val2, val11);
			JobHandle val13 = IJobForExtensions.ScheduleByRef<TestAreSpeedingJob>(ref testAreSpeedingJob, indices.Length, val12);
			distAndBudget.Dispose(val13);
			((JobHandle)(ref val13)).Complete();
		}
	}

	public static void AreFlying(in BasePlayer.PlayerServerStates.ReadOnly playerStates, ReadOnly<PlayerState> ahStates, NativeArray<PlayerFlyhackState> playerFlyStates, ReadOnly<int> indices, NativeArray<bool> results)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		FillJob<bool> fillJob = new FillJob<bool>
		{
			Value = false,
			Values = results
		};
		IJobExtensions.RunByRef<FillJob<bool>>(ref fillJob);
		ProcessFlyhackPauseTimeJob processFlyhackPauseTimeJob = new ProcessFlyhackPauseTimeJob
		{
			PlayerStates = playerFlyStates,
			Indices = indices,
			DeltaTimes = playerStates.TickDeltaTime
		};
		IJobExtensions.RunByRef<ProcessFlyhackPauseTimeJob>(ref processFlyhackPauseTimeJob);
		if (ConVar.AntiHack.flyhack_protection <= 0)
		{
			return;
		}
		ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
		NativeArray<Matrix4x4> val = default(NativeArray<Matrix4x4>);
		val._002Ector(indices.Length, (Allocator)3, (NativeArrayOptions)0);
		for (int i = 0; i < indices.Length; i++)
		{
			int index = indices[i];
			BasePlayer basePlayer = objects[index];
			bool flag = (Object)(object)((Component)basePlayer).transform.parent == (Object)null;
			val[i] = (flag ? Matrix4x4.zero : ((Component)basePlayer).transform.parent.localToWorldMatrix);
		}
		NativeArray<FlyingBatch> batches = default(NativeArray<FlyingBatch>);
		batches._002Ector(indices.Length, (Allocator)3, (NativeArrayOptions)1);
		try
		{
			NativeList<Vector3> val2 = new NativeList<Vector3>(indices.Length * ConVar.AntiHack.flyhack_maxsteps, AllocatorHandle.op_Implicit((Allocator)3));
			try
			{
				NativeList<Vector3> to = new NativeList<Vector3>(indices.Length * ConVar.AntiHack.flyhack_maxsteps, AllocatorHandle.op_Implicit((Allocator)3));
				try
				{
					NativeList<Vector3> checkPoses = new NativeList<Vector3>(indices.Length * ConVar.AntiHack.flyhack_maxsteps, AllocatorHandle.op_Implicit((Allocator)3));
					try
					{
						GatherFlyingBatchesJob gatherFlyingBatchesJob = new GatherFlyingBatchesJob
						{
							From = val2,
							To = to,
							CheckPoses = checkPoses,
							Batches = batches,
							TickCache = playerStates.TickCache,
							Indices = indices,
							Matrices = val.AsReadOnly(),
							MaxSteps = ConVar.AntiHack.flyhack_maxsteps,
							DefaultStepSize = Mathf.Max(ConVar.AntiHack.flyhack_stepsize, 0.1f),
							Protection = ConVar.AntiHack.flyhack_protection
						};
						IJobExtensions.RunByRef<GatherFlyingBatchesJob>(ref gatherFlyingBatchesJob);
						val.Dispose();
						if (batches.Length > 0)
						{
							TestAreFlying(in playerStates, val2.AsReadOnly(), to.AsReadOnly(), checkPoses.AsReadOnly(), ahStates, playerFlyStates, batches.AsReadOnly(), ConVar.AntiHack.flyhack_protection >= 2, indices, results);
						}
					}
					finally
					{
						((IDisposable)checkPoses/*cast due to constrained. prefix*/).Dispose();
					}
				}
				finally
				{
					((IDisposable)to/*cast due to constrained. prefix*/).Dispose();
				}
			}
			finally
			{
				((IDisposable)val2/*cast due to constrained. prefix*/).Dispose();
			}
		}
		finally
		{
			((IDisposable)batches/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static void TestAreFlying(in BasePlayer.PlayerServerStates.ReadOnly playerStates, ReadOnly<Vector3> oldPoses, ReadOnly<Vector3> newPoses, ReadOnly<Vector3> checkPoses, ReadOnly<PlayerState> ahStates, NativeArray<PlayerFlyhackState> flyStates, ReadOnly<FlyingBatch> flyingBatches, bool verifyGrounded, ReadOnly<int> indices, NativeArray<bool> results)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0522: Unknown result type (might be due to invalid IL or missing references)
		//IL_0524: Unknown result type (might be due to invalid IL or missing references)
		//IL_052b: Unknown result type (might be due to invalid IL or missing references)
		//IL_052d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0535: Unknown result type (might be due to invalid IL or missing references)
		//IL_053a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0542: Unknown result type (might be due to invalid IL or missing references)
		//IL_0547: Unknown result type (might be due to invalid IL or missing references)
		//IL_054e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0550: Unknown result type (might be due to invalid IL or missing references)
		//IL_0557: Unknown result type (might be due to invalid IL or missing references)
		//IL_0558: Unknown result type (might be due to invalid IL or missing references)
		//IL_0561: Unknown result type (might be due to invalid IL or missing references)
		//IL_0566: Unknown result type (might be due to invalid IL or missing references)
		//IL_056f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0574: Unknown result type (might be due to invalid IL or missing references)
		//IL_057b: Unknown result type (might be due to invalid IL or missing references)
		//IL_057d: Unknown result type (might be due to invalid IL or missing references)
		//IL_059f: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0605: Unknown result type (might be due to invalid IL or missing references)
		//IL_060a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0611: Unknown result type (might be due to invalid IL or missing references)
		//IL_0612: Unknown result type (might be due to invalid IL or missing references)
		//IL_062c: Unknown result type (might be due to invalid IL or missing references)
		//IL_062e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0635: Unknown result type (might be due to invalid IL or missing references)
		//IL_0637: Unknown result type (might be due to invalid IL or missing references)
		//IL_063e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0640: Unknown result type (might be due to invalid IL or missing references)
		//IL_0649: Unknown result type (might be due to invalid IL or missing references)
		//IL_064e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0655: Unknown result type (might be due to invalid IL or missing references)
		//IL_0656: Unknown result type (might be due to invalid IL or missing references)
		//IL_065d: Unknown result type (might be due to invalid IL or missing references)
		//IL_065e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0667: Unknown result type (might be due to invalid IL or missing references)
		//IL_066c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0675: Unknown result type (might be due to invalid IL or missing references)
		//IL_067a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		int defaultMaxResultsPerQuery = GamePhysics.DefaultMaxResultsPerQuery;
		NativeArray<Vector3> results2 = default(NativeArray<Vector3>);
		results2._002Ector(checkPoses.Length, (Allocator)3, (NativeArrayOptions)0);
		NativeArray<Vector3> results3 = default(NativeArray<Vector3>);
		results3._002Ector(checkPoses.Length, (Allocator)3, (NativeArrayOptions)0);
		NativeArray<float> values = default(NativeArray<float>);
		values._002Ector(checkPoses.Length, (Allocator)3, (NativeArrayOptions)0);
		NativeArray<int> values2 = default(NativeArray<int>);
		values2._002Ector(checkPoses.Length, (Allocator)3, (NativeArrayOptions)0);
		NativeArray<ColliderHit> hits = default(NativeArray<ColliderHit>);
		hits._002Ector(checkPoses.Length * defaultMaxResultsPerQuery, (Allocator)3, (NativeArrayOptions)0);
		AddVectorJob addVectorJob = new AddVectorJob
		{
			Inputs = checkPoses,
			Results = results2,
			Modification = new Vector3(0f, 0.5f - ConVar.AntiHack.flyhack_extrusion, 0f)
		};
		IJobExtensions.RunByRef<AddVectorJob>(ref addVectorJob);
		AddVectorJob addVectorJob2 = new AddVectorJob
		{
			Inputs = checkPoses,
			Results = results3,
			Modification = new Vector3(0f, 1.3f, 0f)
		};
		IJobExtensions.RunByRef<AddVectorJob>(ref addVectorJob2);
		FillJob<float> fillJob = new FillJob<float>
		{
			Values = values,
			Value = 0.5f - ConVar.AntiHack.flyhack_margin
		};
		IJobExtensions.RunByRef<FillJob<float>>(ref fillJob);
		FillJob<int> fillJob2 = new FillJob<int>
		{
			Values = values2,
			Value = 1503895809
		};
		IJobExtensions.RunByRef<FillJob<int>>(ref fillJob2);
		JobHandle val = GamePhysics.OverlapCapsules(results2.AsReadOnly(), results3.AsReadOnly(), values.AsReadOnly(), values2.AsReadOnly(), hits, defaultMaxResultsPerQuery, (QueryTriggerInteraction)1, GamePhysics.MasksToValidate.None);
		NativeArray<Vector3> results4 = default(NativeArray<Vector3>);
		results4._002Ector(checkPoses.Length, (Allocator)3, (NativeArrayOptions)0);
		try
		{
			AddVectorJob addVectorJob3 = new AddVectorJob
			{
				Inputs = checkPoses,
				Results = results4,
				Modification = new Vector3(0f, 0f - ConVar.AntiHack.flyhack_extrusion, 0f)
			};
			IJobExtensions.RunByRef<AddVectorJob>(ref addVectorJob3);
			NativeArray<WaterLevel.WaterInfo> results5 = new NativeArray<WaterLevel.WaterInfo>(checkPoses.Length, (Allocator)3, (NativeArrayOptions)0);
			try
			{
				WaterLevel.GetWaterInfos(results4.AsReadOnly(), waves: true, volumes: false, null, results5);
				NativeArray<bool> results6 = new NativeArray<bool>(checkPoses.Length, (Allocator)3, (NativeArrayOptions)0);
				try
				{
					GatherValidWaterIndicesJob gatherValidWaterIndicesJob = new GatherValidWaterIndicesJob
					{
						WaterInfos = results5.AsReadOnly(),
						Results = results6
					};
					IJobExtensions.RunByRef<GatherValidWaterIndicesJob>(ref gatherValidWaterIndicesJob);
					results2.Dispose(val);
					results3.Dispose(val);
					values.Dispose(val);
					values2.Dispose(val);
					NativeArray<float> values3 = new NativeArray<float>(checkPoses.Length, (Allocator)3, (NativeArrayOptions)0);
					try
					{
						FillJob<float> fillJob3 = new FillJob<float>
						{
							Values = values3,
							Value = 0.01f
						};
						IJobExtensions.RunByRef<FillJob<float>>(ref fillJob3);
						NativeArray<int> values4 = new NativeArray<int>(checkPoses.Length, (Allocator)3, (NativeArrayOptions)0);
						try
						{
							FillJob<int> fillJob4 = new FillJob<int>
							{
								Values = values4,
								Value = 262144
							};
							IJobExtensions.RunByRef<FillJob<int>>(ref fillJob4);
							NativeArray<EnvironmentType> results7 = new NativeArray<EnvironmentType>(checkPoses.Length, (Allocator)3, (NativeArrayOptions)0);
							try
							{
								EnvironmentManager.Get(checkPoses, values3.AsReadOnly(), values4.AsReadOnly(), results7, GamePhysics.DefaultMaxResultsPerQuery, (QueryTriggerInteraction)2, GamePhysics.MasksToValidate.None);
								NativeArray<bool> results8 = new NativeArray<bool>(checkPoses.Length, (Allocator)3, (NativeArrayOptions)0);
								try
								{
									CheckAnyEnvironmentTypeInGroupJob checkAnyEnvironmentTypeInGroupJob = new CheckAnyEnvironmentTypeInGroupJob
									{
										Hits = results7.AsReadOnly(),
										Results = results8,
										GroupSize = defaultMaxResultsPerQuery,
										TypeToTest = EnvironmentType.Elevator
									};
									IJobExtensions.RunByRef<CheckAnyEnvironmentTypeInGroupJob>(ref checkAnyEnvironmentTypeInGroupJob);
									((JobHandle)(ref val)).Complete();
									NativeList<int> results9 = new NativeList<int>(results2.Length * defaultMaxResultsPerQuery, AllocatorHandle.op_Implicit((Allocator)3));
									try
									{
										GatherHitColliderIndicesJob gatherHitColliderIndicesJob = new GatherHitColliderIndicesJob
										{
											Hits = hits.AsReadOnly(),
											Results = results9,
											ResultsPerQuery = defaultMaxResultsPerQuery
										};
										IJobExtensions.RunByRef<GatherHitColliderIndicesJob>(ref gatherHitColliderIndicesJob);
										NativeArray<int> lookup = new NativeArray<int>(checkPoses.Length, (Allocator)3, (NativeArrayOptions)0);
										try
										{
											BuildFlyingBatchLookupMapJob buildFlyingBatchLookupMapJob = new BuildFlyingBatchLookupMapJob
											{
												Lookup = lookup,
												Batches = flyingBatches
											};
											IJobExtensions.RunByRef<BuildFlyingBatchLookupMapJob>(ref buildFlyingBatchLookupMapJob);
											NativeArray<bool> val2 = new NativeArray<bool>(checkPoses.Length, (Allocator)3, (NativeArrayOptions)0);
											FillJob<bool> fillJob5 = new FillJob<bool>
											{
												Values = val2,
												Value = true
											};
											IJobExtensions.RunByRef<FillJob<bool>>(ref fillJob5);
											ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
											Span<PlayerFlyhackState> span = NativeArray<PlayerFlyhackState>.op_Implicit(ref flyStates);
											for (int i = 0; i < results9.Length; i++)
											{
												int num = results9[i] / defaultMaxResultsPerQuery;
												int num2 = lookup[num];
												FlyingBatch flyingBatch = flyingBatches[num2];
												if (!val2[num])
												{
													continue;
												}
												BasePlayer basePlayer = objects[flyingBatch.PlayerIndex];
												ref PlayerFlyhackState reference = ref span[flyingBatch.PlayerIndex];
												reference.IsOnPlayer = false;
												ColliderHit val3 = hits[results9[i]];
												Collider collider = ((ColliderHit)(ref val3)).collider;
												if ((0x20000 & (1 << ((Component)collider).gameObject.layer)) != 0)
												{
													BasePlayer basePlayer2 = GameObjectEx.ToBaseEntity(collider) as BasePlayer;
													if ((Object)(object)basePlayer2 == (Object)(object)basePlayer)
													{
														continue;
													}
													if (basePlayer2.ActivePlayerInd != -1)
													{
														PlayerFlyhackState playerFlyhackState = flyStates[basePlayer2.ActivePlayerInd];
														if (playerFlyhackState.IsInAir || playerFlyhackState.IsOnPlayer || basePlayer2.TriggeredAntiHack(ahStates))
														{
															continue;
														}
													}
													if (!basePlayer2.IsSleeping())
													{
														reference.IsOnPlayer = true;
														val2[num] = false;
													}
												}
												else
												{
													val2[num] = false;
												}
											}
											hits.Dispose();
											DetermineInAirJob determineInAirJob = new DetermineInAirJob
											{
												Results = val2,
												PlayerFlyStates = flyStates,
												PlayerMSFlags = playerStates.PlayerModelStateFlags,
												PlayerStates = playerStates.CachedStates,
												FlyingBatches = flyingBatches,
												OldPoses = oldPoses,
												WaterValidStates = results6.AsReadOnly(),
												ElevatorValidStates = results8.AsReadOnly(),
												Indices = indices,
												verifyGrounded = verifyGrounded
											};
											IJobExtensions.RunByRef<DetermineInAirJob>(ref determineInAirJob);
											NativeArray<bool> results10 = new NativeArray<bool>(indices.Length, (Allocator)3, (NativeArrayOptions)1);
											try
											{
												GatherWasInAirStatesJob gatherWasInAirStatesJob = new GatherWasInAirStatesJob
												{
													Results = results10,
													PlayerStates = flyStates.AsReadOnly(),
													Indices = indices
												};
												IJobExtensions.RunByRef<GatherWasInAirStatesJob>(ref gatherWasInAirStatesJob);
												CacheInAirStateJob cacheInAirStateJob = new CacheInAirStateJob
												{
													PlayerStates = flyStates,
													Indices = indices,
													BatchMap = lookup.AsReadOnly(),
													PlayersInAir = val2.AsReadOnly(),
													OldPoses = oldPoses
												};
												IJobExtensions.RunByRef<CacheInAirStateJob>(ref cacheInAirStateJob);
												TestAreFlyingJob testAreFlyingJob = new TestAreFlyingJob
												{
													Results = results,
													PlayerStates = flyStates,
													Indices = indices,
													BatchMap = lookup.AsReadOnly(),
													OldPoses = oldPoses,
													NewPoses = newPoses,
													PlayersInAir = val2.AsReadOnly(),
													WasInAirStates = results10.AsReadOnly(),
													ForgivenessVerticalInertia = ConVar.AntiHack.flyhack_forgiveness_vertical_inertia,
													ForgivenessVertical = ConVar.AntiHack.flyhack_forgiveness_vertical,
													ForgivenessHorizontalInertia = ConVar.AntiHack.flyhack_forgiveness_horizontal_inertia,
													ForgivenessHorizontal = ConVar.AntiHack.flyhack_forgiveness_horizontal,
													TimeSinceStartup = Time.realtimeSinceStartup
												};
												IJobExtensions.RunByRef<TestAreFlyingJob>(ref testAreFlyingJob);
												val2.Dispose();
											}
											finally
											{
												((IDisposable)results10/*cast due to constrained. prefix*/).Dispose();
											}
										}
										finally
										{
											((IDisposable)lookup/*cast due to constrained. prefix*/).Dispose();
										}
									}
									finally
									{
										((IDisposable)results9/*cast due to constrained. prefix*/).Dispose();
									}
								}
								finally
								{
									((IDisposable)results8/*cast due to constrained. prefix*/).Dispose();
								}
							}
							finally
							{
								((IDisposable)results7/*cast due to constrained. prefix*/).Dispose();
							}
						}
						finally
						{
							((IDisposable)values4/*cast due to constrained. prefix*/).Dispose();
						}
					}
					finally
					{
						((IDisposable)values3/*cast due to constrained. prefix*/).Dispose();
					}
				}
				finally
				{
					((IDisposable)results6/*cast due to constrained. prefix*/).Dispose();
				}
			}
			finally
			{
				((IDisposable)results5/*cast due to constrained. prefix*/).Dispose();
			}
		}
		finally
		{
			((IDisposable)results4/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static bool TestIsBuildingInsideSomething(Construction.Target target, Vector3 deployPos)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (ConVar.AntiHack.build_inside_check <= 0)
		{
			return false;
		}
		foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
		{
			if (monument.IsInBounds(deployPos))
			{
				return false;
			}
		}
		if (IsInsideMesh(deployPos) && IsInsideMesh(((Ray)(ref target.ray)).origin))
		{
			LogToConsoleBatched(target.player, AntiHackType.InsideGeometry, "Tried to build while clipped inside " + ((Object)((RaycastHit)(ref isInsideRayHit)).collider).name, 25f);
			if (ConVar.AntiHack.build_inside_check > 1)
			{
				return true;
			}
		}
		return false;
	}

	public static void FadeViolations(BasePlayer ply, float deltaTime)
	{
		ref PlayerState reference = ref NativeArray<PlayerState>.op_Implicit(ref PlayerStates)[ply.ActivePlayerInd];
		if (Time.realtimeSinceStartup - reference.LastViolationTime > ConVar.AntiHack.relaxationpause)
		{
			reference.ViolationLevel = Mathf.Max(0f, reference.ViolationLevel - ConVar.AntiHack.relaxationrate * deltaTime);
		}
	}

	public static bool EnforceViolations(BasePlayer ply)
	{
		PlayerState playerState = PlayerStates[ply.ActivePlayerInd];
		if (playerState.ViolationLevel > ConVar.AntiHack.maxviolation)
		{
			if (ConVar.AntiHack.debuglevel >= 1)
			{
				LogToConsole(ply, playerState.LastViolationType, $"Enforcing (violation of {playerState.ViolationLevel})");
			}
			string reason = $"{playerState.LastViolationType} Violation Level {playerState.ViolationLevel}";
			if (ConVar.AntiHack.enforcementlevel > 1)
			{
				Kick(ply, reason);
			}
			else
			{
				Kick(ply, reason);
			}
			return true;
		}
		return false;
	}

	public static void Log(BasePlayer ply, AntiHackType type, string message, bool logToAnalytics = true)
	{
		if (ConVar.AntiHack.debuglevel > 1)
		{
			LogToConsole(ply, type, message);
		}
		if (logToAnalytics)
		{
			Facepunch.Rust.Analytics.Azure.OnAntihackViolation(ply, type, message);
		}
		LogToEAC(ply, type, message);
	}

	public static void LogToConsoleBatched(BasePlayer ply, AntiHackType type, string message, float maxDistance)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		string playerName = ((object)ply).ToString();
		Vector3 position = ((Component)ply).transform.position;
		foreach (GroupedLog groupedLog2 in groupedLogs)
		{
			if (groupedLog2.TryGroup(playerName, type, message, position, maxDistance))
			{
				return;
			}
		}
		GroupedLog groupedLog = Pool.Get<GroupedLog>();
		groupedLog.SetInitial(playerName, type, message, position);
		groupedLogs.Enqueue(groupedLog);
	}

	private static void LogToConsole(BasePlayer ply, AntiHackType type, string message)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		Debug.LogWarning((object)(((object)ply)?.ToString() + " " + type.ToString() + ": " + message + " at " + ((object)((Component)ply).transform.position/*cast due to constrained. prefix*/).ToString()));
	}

	private unsafe static void LogToConsole(string plyName, AntiHackType type, string message, Vector3 pos)
	{
		Debug.LogWarning((object)(plyName + " " + type.ToString() + ": " + message + " at " + ((object)(*(Vector3*)(&pos))/*cast due to constrained. prefix*/).ToString()));
	}

	private static void LogToEAC(BasePlayer ply, AntiHackType type, string message)
	{
		if (ConVar.AntiHack.reporting)
		{
			EACServer.SendPlayerBehaviorReport((PlayerReportsCategory)2, ply.UserIDString, type.ToString() + ": " + message);
		}
	}

	public static void AddViolation(BasePlayer ply, AntiHackType type, float amount, GameObject gameObject = null)
	{
		if (Interface.CallHook("OnPlayerViolation", ply, type, amount, gameObject) != null || ply.ActivePlayerInd == -1)
		{
			return;
		}
		using (TimeWarning.New("AntiHack.AddViolation"))
		{
			ref PlayerState reference = ref NativeArray<PlayerState>.op_Implicit(ref PlayerStates)[ply.ActivePlayerInd];
			reference.LastViolationType = type;
			reference.LastViolationTime = Time.realtimeSinceStartup;
			reference.ViolationLevel += amount;
			if (type == AntiHackType.NoClip || type == AntiHackType.FlyHack || type == AntiHackType.SpeedHack || type == AntiHackType.InsideGeometry || type == AntiHackType.InsideTerrain || type == AntiHackType.Ticks)
			{
				reference.LastMovementViolationTime = Time.realtimeSinceStartup;
			}
			if ((ConVar.AntiHack.debuglevel < 2 || !(amount > 0f)) && (ConVar.AntiHack.debuglevel < 3 || type == AntiHackType.NoClip) && ConVar.AntiHack.debuglevel < 4)
			{
				return;
			}
			string text = "Added violation of " + amount + " in frame " + Time.frameCount + " (now has " + reference.ViolationLevel + ")";
			if ((Object)(object)gameObject != (Object)null)
			{
				text = text + " " + ((Object)gameObject).name;
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(gameObject);
				if ((Object)(object)baseEntity != (Object)null)
				{
					text = text + " (entity: " + baseEntity.ShortPrefabName + ")";
				}
			}
			LogToConsole(ply, type, text);
		}
	}

	public static void Kick(BasePlayer ply, string reason)
	{
		AddRecord(ply, kicks);
		ply.Kick(reason);
	}

	public static void Ban(BasePlayer ply, string reason)
	{
		AddRecord(ply, bans);
		ConsoleSystem.Run(ConsoleSystem.Option.Server, "ban", ply.userID.Get(), reason);
	}

	private static void AddRecord(BasePlayer ply, Dictionary<ulong, int> records)
	{
		if (records.ContainsKey(ply.userID))
		{
			records[ply.userID]++;
		}
		else
		{
			records.Add(ply.userID, 1);
		}
	}

	public static int GetKickRecord(BasePlayer ply)
	{
		return GetRecord(ply, kicks);
	}

	public static int GetBanRecord(BasePlayer ply)
	{
		return GetRecord(ply, bans);
	}

	private static int GetRecord(BasePlayer ply, Dictionary<ulong, int> records)
	{
		if (!records.ContainsKey(ply.userID))
		{
			return 0;
		}
		return records[ply.userID];
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ConVar;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Development.Attributes;
using Facepunch;
using GamePhysicsJobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UtilityJobs;

public static class GamePhysics
{
	public enum Realm
	{
		Client,
		Server
	}

	[Flags]
	public enum MasksToValidate : byte
	{
		None = 0,
		Terrain = 1,
		Water = 2,
		All = Terrain | Water
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003CFindComponent_003Eg__FindCompAsync_007C36_0_003Ed<T> : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public ReadOnly<ColliderHit> hits;

		public int start;

		public int end;

		public int maxResPerCast;

		public NativeArray<bool> results;

		private UnsafeScriptingAccess.MaybeSwitchToThreadPool.Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				UnsafeScriptingAccess.MaybeSwitchToThreadPool.Awaiter awaiter;
				if (num != 0)
				{
					awaiter = UnsafeScriptingAccess.SwitchToMultithreading().GetAwaiter();
					if (!awaiter.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = awaiter;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<UnsafeScriptingAccess.MaybeSwitchToThreadPool.Awaiter, _003C_003CFindComponent_003Eg__FindCompAsync_007C36_0_003Ed<T>>(ref awaiter, ref this);
						return;
					}
				}
				else
				{
					awaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(UnsafeScriptingAccess.MaybeSwitchToThreadPool.Awaiter);
					num = (_003C_003E1__state = -1);
				}
				awaiter.GetResult();
				TimeWarning timeWarning = TimeWarning.New("FindComponentAsync<T>");
				try
				{
					UnsafeScriptingAccess unsafeScriptingAccess = UnsafeScriptingAccess.Start();
					try
					{
						_003CFindComponent_003Eg__FindComp_007C36_1<T>(hits, start, end, maxResPerCast, results);
					}
					finally
					{
						if (num < 0)
						{
							((IDisposable)unsafeScriptingAccess/*cast due to constrained. prefix*/).Dispose();
						}
					}
				}
				finally
				{
					if (num < 0)
					{
						((IDisposable)timeWarning)?.Dispose();
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	public const int BufferLength = 32768;

	private static RaycastHit[] hitBuffer = (RaycastHit[])(object)new RaycastHit[32768];

	private static RaycastHit[] hitBufferB = (RaycastHit[])(object)new RaycastHit[32768];

	private static Collider[] colBuffer = (Collider[])(object)new Collider[32768];

	[ServerVar(Help = "How many results to collect per command - DONT set this too low or you'll risk missing results", Default = "48")]
	public static int DefaultMaxResultsPerQuery = 48;

	public static bool CheckSphere(Realm realm, Vector3 position, float radius, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)0)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return CheckSphere(position, radius, layerMask, triggerInteraction);
	}

	public static bool CheckSphere(Vector3 position, float radius, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)0)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		layerMask = HandleIgnoreCollision(position, layerMask);
		return Physics.CheckSphere(position, radius, layerMask, triggerInteraction);
	}

	public static JobHandle CheckSpheres(ReadOnly<Vector3> pos, ReadOnly<float> radii, ReadOnly<int> layerMasks, NativeArray<bool> results, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1, MasksToValidate validate = MasksToValidate.All)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GamePhysics.CheckSpheres"))
		{
			NativeArray<ColliderHit> hits = new NativeArray<ColliderHit>(pos.Length, (Allocator)3, (NativeArrayOptions)0);
			JobHandle val = OverlapSpheres(pos, radii, layerMasks, hits, 1, triggerInteraction, validate);
			CheckHitsJob checkHitsJob = new CheckHitsJob
			{
				Results = results,
				Hits = hits.AsReadOnly()
			};
			JobHandle val2 = IJobExtensions.ScheduleByRef<CheckHitsJob>(ref checkHitsJob, val);
			hits.Dispose(val2);
			return val2;
		}
	}

	public static bool CheckCapsule(Realm realm, Vector3 start, Vector3 end, float radius, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)0)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return CheckCapsule(start, end, radius, layerMask, triggerInteraction);
	}

	public static bool CheckCapsule(Vector3 start, Vector3 end, float radius, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)0, BaseEntity ignoreEntity = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		layerMask = HandleIgnoreCollision((start + end) * 0.5f, layerMask);
		if ((Object)(object)ignoreEntity == (Object)null)
		{
			return Physics.CheckCapsule(start, end, radius, layerMask, triggerInteraction);
		}
		int num = Physics.OverlapCapsuleNonAlloc(start, end, radius, colBuffer, layerMask, triggerInteraction);
		for (int i = 0; i < num; i++)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(colBuffer[i]);
			if ((Object)(object)baseEntity == (Object)null)
			{
				return true;
			}
			if (!((Object)(object)baseEntity == (Object)(object)ignoreEntity) && baseEntity.isServer == ignoreEntity.isServer)
			{
				return true;
			}
		}
		return false;
	}

	public static JobHandle CheckCapsules(ReadOnly<Vector3> starts, ReadOnly<Vector3> ends, ReadOnly<float> radii, ReadOnly<int> layerMasks, NativeArray<bool> results, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1, MasksToValidate validate = MasksToValidate.All, bool mitigateSpheres = true)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GamePhysics.CheckCapsules"))
		{
			ReadOnly<int> layerMasks2 = layerMasks;
			NativeArray<int> array = default(NativeArray<int>);
			if (validate != MasksToValidate.None)
			{
				array = new NativeArray<int>(layerMasks.Length, (Allocator)3, (NativeArrayOptions)0);
				layerMasks.CopyTo(array);
				NativeArray<Vector3> results2 = default(NativeArray<Vector3>);
				results2._002Ector(starts.Length, (Allocator)3, (NativeArrayOptions)0);
				CalcMidpoingJob calcMidpoingJob = new CalcMidpoingJob
				{
					Results = results2,
					From = starts,
					To = ends
				};
				IJobExtensions.RunByRef<CalcMidpoingJob>(ref calcMidpoingJob);
				HandleIgnoreCollision(results2.AsReadOnly(), array, validate);
				results2.Dispose();
				layerMasks2 = array.AsReadOnly();
			}
			NativeArray<OverlapCapsuleCommand> val = new NativeArray<OverlapCapsuleCommand>(starts.Length, (Allocator)3, (NativeArrayOptions)0);
			GenerateOverlapCapsuleCommandsJob generateOverlapCapsuleCommandsJob = new GenerateOverlapCapsuleCommandsJob
			{
				CapsuleCommands = val,
				From = starts,
				To = ends,
				Radiii = radii,
				LayerMasks = layerMasks2,
				TriggerInteraction = triggerInteraction,
				HitBackfaces = false,
				HitMultipleFaces = false
			};
			IJobExtensions.RunByRef<GenerateOverlapCapsuleCommandsJob>(ref generateOverlapCapsuleCommandsJob);
			NativeArrayEx.SafeDispose(ref array);
			NativeArray<ColliderHit> hits = new NativeArray<ColliderHit>(starts.Length, (Allocator)3, (NativeArrayOptions)0);
			JobHandle val2 = default(JobHandle);
			val2 = ((!mitigateSpheres) ? ExecuteOverlapCapsuleCommands(val, hits, 1) : MitigateSphereCapsuleCommands(val, hits, 1));
			val.Dispose(val2);
			CheckHitsJob checkHitsJob = new CheckHitsJob
			{
				Results = results,
				Hits = hits.AsReadOnly()
			};
			JobHandle val3 = IJobExtensions.ScheduleByRef<CheckHitsJob>(ref checkHitsJob, val2);
			hits.Dispose(val3);
			return val3;
		}
	}

	public static bool CheckOBB(OBB obb, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)0)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		layerMask = HandleIgnoreCollision(obb.position, layerMask);
		return Physics.CheckBox(obb.position, obb.extents, obb.rotation, layerMask, triggerInteraction);
	}

	public static bool CheckOBBAndEntity(OBB obb, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)0, BaseEntity ignoreEntity = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		layerMask = HandleIgnoreCollision(obb.position, layerMask);
		int num = Physics.OverlapBoxNonAlloc(obb.position, obb.extents, colBuffer, obb.rotation, layerMask, triggerInteraction);
		for (int i = 0; i < num; i++)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(colBuffer[i]);
			if (!((Object)(object)baseEntity != (Object)null) || !((Object)(object)ignoreEntity != (Object)null) || (baseEntity.isServer == ignoreEntity.isServer && !((Object)(object)baseEntity == (Object)(object)ignoreEntity)))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CheckBounds(Bounds bounds, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)0)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		layerMask = HandleIgnoreCollision(((Bounds)(ref bounds)).center, layerMask);
		return Physics.CheckBox(((Bounds)(ref bounds)).center, ((Bounds)(ref bounds)).extents, Quaternion.identity, layerMask, triggerInteraction);
	}

	public static void CheckBounds(ReadOnly<Vector3> centers, ReadOnly<Vector3> halfExtents, ReadOnly<int> layerMasks, NativeArray<bool> results, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1, MasksToValidate validate = MasksToValidate.All)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		ReadOnly<int> layerMasks2 = layerMasks;
		NativeArray<int> array = default(NativeArray<int>);
		if (validate != MasksToValidate.None)
		{
			array._002Ector(layerMasks.Length, (Allocator)3, (NativeArrayOptions)0);
			layerMasks.CopyTo(array);
			HandleIgnoreCollision(centers, array, validate);
			layerMasks2 = array.AsReadOnly();
		}
		NativeArray<OverlapBoxCommand> val = default(NativeArray<OverlapBoxCommand>);
		val._002Ector(centers.Length, (Allocator)3, (NativeArrayOptions)0);
		IJobExtensions.Run<GenerateOverlapBoxCommandsJob>(new GenerateOverlapBoxCommandsJob
		{
			BoxCommands = val,
			Centers = centers,
			Extents = halfExtents,
			LayerMasks = layerMasks2,
			TriggerInteraction = triggerInteraction,
			HitBackfaces = false,
			HitMultipleFaces = false
		});
		NativeArrayEx.SafeDispose(ref array);
		NativeArray<ColliderHit> hits = default(NativeArray<ColliderHit>);
		hits._002Ector(centers.Length, (Allocator)3, (NativeArrayOptions)0);
		JobHandle val2 = ExecuteOverlapBoxCommands(val, hits, 1);
		((JobHandle)(ref val2)).Complete();
		val.Dispose();
		CheckHitsJob checkHitsJob = new CheckHitsJob
		{
			Results = results,
			Hits = hits.AsReadOnly()
		};
		IJobExtensions.RunByRef<CheckHitsJob>(ref checkHitsJob);
		hits.Dispose();
	}

	private static JobHandle ExecuteOverlapBoxCommands(NativeArray<OverlapBoxCommand> commands, NativeArray<ColliderHit> hits, int maxResPerCast)
	{
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		if (Debug.isDebugBuild)
		{
			NativeList<int> invalidIndices = default(NativeList<int>);
			invalidIndices._002Ector(commands.Length, AllocatorHandle.op_Implicit((Allocator)3));
			ValidateOverlapBoxCommandsJob validateOverlapBoxCommandsJob = new ValidateOverlapBoxCommandsJob
			{
				InvalidIndices = invalidIndices,
				Commands = commands.AsReadOnly()
			};
			IJobExtensions.RunByRef<ValidateOverlapBoxCommandsJob>(ref validateOverlapBoxCommandsJob);
			if (!invalidIndices.IsEmpty)
			{
				int num = invalidIndices[0];
				OverlapBoxCommand val = commands[num];
				Debug.LogError((object)string.Concat(string.Concat(string.Concat($"OverlapBox has {invalidIndices.Length} invalid box commands!" + $"\nFirst one was at index {num}:", $"\n\tCenter: {((OverlapBoxCommand)(ref val)).center}"), $"\n\tExtents: {((OverlapBoxCommand)(ref val)).halfExtents}"), "\nThese queries will be skipped!"));
			}
			invalidIndices.Dispose();
		}
		int batchSize = ThreadUtils.GetBatchSize(commands.Length);
		return OverlapBoxCommand.ScheduleBatch(commands, hits, batchSize, maxResPerCast, default(JobHandle));
	}

	public static bool CheckInsideNonConvexMesh(Vector3 point, int layerMask = -5)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		bool queriesHitBackfaces = Physics.queriesHitBackfaces;
		Physics.queriesHitBackfaces = true;
		int num = Physics.RaycastNonAlloc(point, Vector3.up, hitBuffer, 100f, layerMask);
		int num2 = Physics.RaycastNonAlloc(point, -Vector3.up, hitBufferB, 100f, layerMask);
		if (num >= hitBuffer.Length)
		{
			Debug.LogWarning((object)"CheckInsideNonConvexMesh query is exceeding hitBuffer length.");
			return false;
		}
		if (num2 > hitBufferB.Length)
		{
			Debug.LogWarning((object)"CheckInsideNonConvexMesh query is exceeding hitBufferB length.");
			return false;
		}
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if ((Object)(object)((RaycastHit)(ref hitBuffer[i])).collider == (Object)(object)((RaycastHit)(ref hitBufferB[j])).collider)
				{
					Physics.queriesHitBackfaces = queriesHitBackfaces;
					return true;
				}
			}
		}
		Physics.queriesHitBackfaces = queriesHitBackfaces;
		return false;
	}

	public static bool CheckInsideAnyCollider(Vector3 point, int layerMask = -5)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (Physics.CheckSphere(point, 0f, layerMask))
		{
			return true;
		}
		if (CheckInsideNonConvexMesh(point, layerMask))
		{
			return true;
		}
		if ((Object)(object)TerrainMeta.HeightMap != (Object)null && TerrainMeta.HeightMap.GetHeight(point) > point.y)
		{
			return true;
		}
		return false;
	}

	[PoolAnalyzerNonCaching]
	public static void OverlapSphere(Vector3 position, float radius, List<Collider> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		layerMask = HandleIgnoreCollision(position, layerMask);
		int count = Physics.OverlapSphereNonAlloc(position, radius, colBuffer, layerMask, triggerInteraction);
		BufferToList(colBuffer, count, list);
	}

	public static bool ContainsEntity(List<Collider> colliders, BaseEntity entity)
	{
		for (int i = 0; i < colliders.Count; i++)
		{
			if (CompareEntity(GameObjectEx.ToBaseEntity(colliders[i]), entity))
			{
				return true;
			}
		}
		return false;
	}

	public static bool OverlapSphereHasEntity(Vector3 position, float radius, BaseEntity entity, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		List<Collider> list = Pool.Get<List<Collider>>();
		OverlapSphere(position, radius, list, layerMask, triggerInteraction);
		bool result = ContainsEntity(list, entity);
		Pool.FreeUnmanaged<Collider>(ref list);
		return result;
	}

	public static JobHandle OverlapSpheres(ReadOnly<Vector3> positions, ReadOnly<float> radii, ReadOnly<int> layerMasks, NativeArray<ColliderHit> hits, int maxResPerCast, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1, MasksToValidate validate = MasksToValidate.All)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GamePhysics.OverlapSpheres"))
		{
			ReadOnly<int> layerMasks2 = layerMasks;
			NativeArray<int> array = default(NativeArray<int>);
			if (validate != MasksToValidate.None)
			{
				array = new NativeArray<int>(layerMasks.Length, (Allocator)3, (NativeArrayOptions)0);
				layerMasks.CopyTo(array);
				HandleIgnoreCollision(positions, array, validate);
				layerMasks2 = array.AsReadOnly();
			}
			NativeArray<OverlapSphereCommand> val = new NativeArray<OverlapSphereCommand>(positions.Length, (Allocator)3, (NativeArrayOptions)0);
			GenerateOverlapSphereCommandsJob generateOverlapSphereCommandsJob = new GenerateOverlapSphereCommandsJob
			{
				SphereCommands = val,
				Pos = positions,
				Radiii = radii,
				LayerMasks = layerMasks2,
				TriggerInteraction = triggerInteraction,
				HitBackfaces = false,
				HitMultipleFaces = false
			};
			IJobExtensions.RunByRef<GenerateOverlapSphereCommandsJob>(ref generateOverlapSphereCommandsJob);
			NativeArrayEx.SafeDispose(ref array);
			JobHandle val2 = ExecuteOverlapSphereCommands(val, hits, maxResPerCast);
			val.Dispose(val2);
			return val2;
		}
	}

	private static JobHandle ExecuteOverlapSphereCommands(NativeArray<OverlapSphereCommand> commands, NativeArray<ColliderHit> hits, int maxResPerCast, JobHandle dependsOn = default(JobHandle))
	{
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (Debug.isDebugBuild)
		{
			NativeList<int> invalidIndices = default(NativeList<int>);
			invalidIndices._002Ector(commands.Length, AllocatorHandle.op_Implicit((Allocator)3));
			ValidateOverlapSphereCommandsJob validateOverlapSphereCommandsJob = new ValidateOverlapSphereCommandsJob
			{
				InvalidIndices = invalidIndices,
				Commands = commands.AsReadOnly()
			};
			JobHandle val = IJobExtensions.ScheduleByRef<ValidateOverlapSphereCommandsJob>(ref validateOverlapSphereCommandsJob, dependsOn);
			((JobHandle)(ref val)).Complete();
			if (!invalidIndices.IsEmpty)
			{
				int num = invalidIndices[0];
				OverlapSphereCommand val2 = commands[num];
				Debug.LogError((object)string.Concat(string.Concat(string.Concat($"OverlapSpheres has {invalidIndices.Length} invalid sphere commands!" + $"\nFirst one was at index {num}:", $"\n\tPos: {((OverlapSphereCommand)(ref val2)).point}"), $"\n\tRadius: {((OverlapSphereCommand)(ref val2)).radius}"), "\nThese queries will be skipped!"));
			}
			invalidIndices.Dispose();
		}
		int batchSize = ThreadUtils.GetBatchSize(commands.Length);
		return OverlapSphereCommand.ScheduleBatch(commands, hits, batchSize, maxResPerCast, dependsOn);
	}

	[PoolAnalyzerNonCaching]
	public static void OBBSweep(OBB obb, Vector3 direction, float distance, List<RaycastHit> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		layerMask = HandleIgnoreCollision(obb.position, layerMask);
		HitBufferToList(Physics.BoxCastNonAlloc(obb.position, obb.extents, direction, hitBuffer, obb.rotation, distance, layerMask, triggerInteraction), list);
	}

	[PoolAnalyzerNonCaching]
	public static void CapsuleSweep(Vector3 position0, Vector3 position1, float radius, Vector3 direction, float distance, List<RaycastHit> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		layerMask = HandleIgnoreCollision(position0, layerMask);
		layerMask = HandleIgnoreCollision(position1, layerMask);
		HitBufferToList(Physics.CapsuleCastNonAlloc(position0, position1, radius, direction, hitBuffer, distance, layerMask, triggerInteraction), list);
	}

	[PoolAnalyzerNonCaching]
	public static void OverlapCapsule(Vector3 point0, Vector3 point1, float radius, List<Collider> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		layerMask = HandleIgnoreCollision(point0, layerMask);
		layerMask = HandleIgnoreCollision(point1, layerMask);
		int count = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, colBuffer, layerMask, triggerInteraction);
		BufferToList(colBuffer, count, list);
	}

	public static JobHandle OverlapCapsules(ReadOnly<Vector3> starts, ReadOnly<Vector3> ends, ReadOnly<float> radii, ReadOnly<int> layerMasks, NativeArray<ColliderHit> hits, int maxResPerCast, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1, MasksToValidate validate = MasksToValidate.All, bool mitigateSpheres = true)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GamePhysics.OverlapCapsules"))
		{
			ReadOnly<int> layerMasks2 = layerMasks;
			NativeArray<int> array = default(NativeArray<int>);
			if (validate != MasksToValidate.None)
			{
				array = new NativeArray<int>(layerMasks.Length, (Allocator)3, (NativeArrayOptions)0);
				layerMasks.CopyTo(array);
				HandleIgnoreCollision(starts, array, validate);
				HandleIgnoreCollision(ends, array, validate);
				layerMasks2 = array.AsReadOnly();
			}
			NativeArray<OverlapCapsuleCommand> val = new NativeArray<OverlapCapsuleCommand>(starts.Length, (Allocator)3, (NativeArrayOptions)0);
			GenerateOverlapCapsuleCommandsJob generateOverlapCapsuleCommandsJob = new GenerateOverlapCapsuleCommandsJob
			{
				CapsuleCommands = val,
				From = starts,
				To = ends,
				Radiii = radii,
				LayerMasks = layerMasks2,
				TriggerInteraction = triggerInteraction,
				HitBackfaces = false,
				HitMultipleFaces = false
			};
			IJobExtensions.RunByRef<GenerateOverlapCapsuleCommandsJob>(ref generateOverlapCapsuleCommandsJob);
			NativeArrayEx.SafeDispose(ref array);
			JobHandle val2 = default(JobHandle);
			val2 = ((!mitigateSpheres) ? ExecuteOverlapCapsuleCommands(val, hits, maxResPerCast) : MitigateSphereCapsuleCommands(val, hits, maxResPerCast));
			val.Dispose(val2);
			return val2;
		}
	}

	private static JobHandle MitigateSphereCapsuleCommands(NativeArray<OverlapCapsuleCommand> commands, NativeArray<ColliderHit> hits, int maxResPerCast)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		NativeList<int> val = default(NativeList<int>);
		val._002Ector(commands.Length, AllocatorHandle.op_Implicit((Allocator)3));
		FindSphereCmdsInCapsuleCmdsJob findSphereCmdsInCapsuleCmdsJob = new FindSphereCmdsInCapsuleCmdsJob
		{
			SphereIndices = val,
			Commands = commands.AsReadOnly()
		};
		IJobExtensions.RunByRef<FindSphereCmdsInCapsuleCmdsJob>(ref findSphereCmdsInCapsuleCmdsJob);
		if (val.IsEmpty)
		{
			val.Dispose();
			return ExecuteOverlapCapsuleCommands(commands, hits, maxResPerCast);
		}
		int num = Math.Max(val.Length, commands.Length - val.Length);
		NativeArray<ColliderHit> hits2 = default(NativeArray<ColliderHit>);
		hits2._002Ector(num * maxResPerCast, (Allocator)3, (NativeArrayOptions)0);
		bool num2 = val.Length != commands.Length;
		NativeArray<OverlapSphereCommand> val2 = default(NativeArray<OverlapSphereCommand>);
		val2._002Ector(val.Length, (Allocator)3, (NativeArrayOptions)1);
		GenerateSphereCmdsFromCapsuleCmdsJob generateSphereCmdsFromCapsuleCmdsJob = new GenerateSphereCmdsFromCapsuleCmdsJob
		{
			SphereCommands = val2,
			Commands = commands.AsReadOnly(),
			Indices = val.AsReadOnly()
		};
		JobHandle val3 = ExecuteOverlapSphereCommands(dependsOn: IJobExtensions.ScheduleByRef<GenerateSphereCmdsFromCapsuleCmdsJob>(ref generateSphereCmdsFromCapsuleCmdsJob, default(JobHandle)), commands: val2, hits: hits2, maxResPerCast: maxResPerCast);
		val2.Dispose(val3);
		ScatterColliderHitsJob scatterColliderHitsJob = new ScatterColliderHitsJob
		{
			To = hits,
			From = hits2.AsReadOnly(),
			Indices = val.AsReadOnly(),
			MaxHitsPerRay = maxResPerCast
		};
		JobHandle val4 = IJobExtensions.ScheduleByRef<ScatterColliderHitsJob>(ref scatterColliderHitsJob, val3);
		if (!num2)
		{
			val.Dispose(val4);
			hits2.Dispose(val4);
			return val4;
		}
		NativeArray<bool> workBuffer = default(NativeArray<bool>);
		workBuffer._002Ector(commands.Length, (Allocator)3, (NativeArrayOptions)0);
		InvertIndexListJob invertIndexListJob = new InvertIndexListJob
		{
			Indices = val,
			WorkBuffer = workBuffer
		};
		JobHandle val5 = IJobExtensions.ScheduleByRef<InvertIndexListJob>(ref invertIndexListJob, val4);
		((JobHandle)(ref val5)).Complete();
		workBuffer.Dispose();
		NativeArray<OverlapCapsuleCommand> val6 = default(NativeArray<OverlapCapsuleCommand>);
		val6._002Ector(val.Length, (Allocator)3, (NativeArrayOptions)0);
		GatherJob<OverlapCapsuleCommand> gatherJob = new GatherJob<OverlapCapsuleCommand>
		{
			Results = val6,
			Source = commands.AsReadOnly(),
			Indices = val.AsReadOnly()
		};
		JobHandle dependsOn = IJobExtensions.ScheduleByRef<GatherJob<OverlapCapsuleCommand>>(ref gatherJob, val5);
		JobHandle val7 = ExecuteOverlapCapsuleCommands(val6, hits2, maxResPerCast, dependsOn);
		ScatterColliderHitsJob scatterColliderHitsJob2 = new ScatterColliderHitsJob
		{
			To = hits,
			From = hits2.AsReadOnly(),
			Indices = val.AsReadOnly(),
			MaxHitsPerRay = maxResPerCast
		};
		val6.Dispose(val7);
		JobHandle val8 = IJobExtensions.ScheduleByRef<ScatterColliderHitsJob>(ref scatterColliderHitsJob2, val7);
		val.Dispose(val8);
		hits2.Dispose(val8);
		return val8;
	}

	private static JobHandle ExecuteOverlapCapsuleCommands(NativeArray<OverlapCapsuleCommand> commands, NativeArray<ColliderHit> hits, int maxResPerCast, JobHandle dependsOn = default(JobHandle))
	{
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		if (Debug.isDebugBuild)
		{
			NativeList<int> invalidIndices = default(NativeList<int>);
			invalidIndices._002Ector(commands.Length, AllocatorHandle.op_Implicit((Allocator)3));
			ValidateOverlapCapsuleCommandsJob validateOverlapCapsuleCommandsJob = new ValidateOverlapCapsuleCommandsJob
			{
				InvalidIndices = invalidIndices,
				Commands = commands.AsReadOnly()
			};
			JobHandle val = IJobExtensions.ScheduleByRef<ValidateOverlapCapsuleCommandsJob>(ref validateOverlapCapsuleCommandsJob, dependsOn);
			((JobHandle)(ref val)).Complete();
			if (!invalidIndices.IsEmpty)
			{
				int num = invalidIndices[0];
				OverlapCapsuleCommand val2 = commands[num];
				Debug.LogError((object)string.Concat(string.Concat(string.Concat(string.Concat($"OverlapCapsules has {invalidIndices.Length} invalid sphere commands!" + $"\nFirst one was at index {num}:", $"\n\tPoint0: {((OverlapCapsuleCommand)(ref val2)).point0}"), $"\n\tPoint1: {((OverlapCapsuleCommand)(ref val2)).point1}"), $"\n\tRadius: {((OverlapCapsuleCommand)(ref val2)).radius}"), "\nThese queries will be skipped!"));
			}
			invalidIndices.Dispose();
		}
		int batchSize = ThreadUtils.GetBatchSize(commands.Length);
		return OverlapCapsuleCommand.ScheduleBatch(commands, hits, batchSize, maxResPerCast, dependsOn);
	}

	[PoolAnalyzerNonCaching]
	public static void OverlapOBB(OBB obb, List<Collider> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		layerMask = HandleIgnoreCollision(obb.position, layerMask);
		int count = Physics.OverlapBoxNonAlloc(obb.position, obb.extents, colBuffer, obb.rotation, layerMask, triggerInteraction);
		BufferToList(colBuffer, count, list);
	}

	public static JobHandle OverlapOBBs(ReadOnly<OBB> obbs, ReadOnly<int> layerMasks, NativeArray<ColliderHit> results, int maxResPerCast, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1, MasksToValidate validate = MasksToValidate.All)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		ReadOnly<int> layerMasks2 = layerMasks;
		NativeArray<int> array = default(NativeArray<int>);
		if (validate != MasksToValidate.None)
		{
			array._002Ector(layerMasks.Length, (Allocator)3, (NativeArrayOptions)0);
			layerMasks.CopyTo(array);
			NativeArray<Vector3> posi = default(NativeArray<Vector3>);
			posi._002Ector(obbs.Length, (Allocator)3, (NativeArrayOptions)0);
			try
			{
				GatherPosFromOBBsJob gatherPosFromOBBsJob = new GatherPosFromOBBsJob
				{
					Posi = posi,
					OBBs = obbs
				};
				IJobExtensions.RunByRef<GatherPosFromOBBsJob>(ref gatherPosFromOBBsJob);
				HandleIgnoreCollision(posi.AsReadOnly(), array, validate);
				layerMasks2 = array.AsReadOnly();
			}
			finally
			{
				((IDisposable)posi/*cast due to constrained. prefix*/).Dispose();
			}
		}
		NativeArray<OverlapBoxCommand> val = default(NativeArray<OverlapBoxCommand>);
		val._002Ector(obbs.Length, (Allocator)3, (NativeArrayOptions)0);
		IJobExtensions.Run<GenerateOverlapBoxCommandsFromOBBsJob>(new GenerateOverlapBoxCommandsFromOBBsJob
		{
			BoxCommands = val,
			OBBs = obbs,
			LayerMasks = layerMasks2,
			TriggerInteraction = triggerInteraction,
			HitBackfaces = false,
			HitMultipleFaces = false
		});
		NativeArrayEx.SafeDispose(ref array);
		JobHandle val2 = ExecuteOverlapBoxCommands(val, results, maxResPerCast);
		val.Dispose(val2);
		return val2;
	}

	[PoolAnalyzerNonCaching]
	public static void OverlapBounds(Bounds bounds, List<Collider> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		layerMask = HandleIgnoreCollision(((Bounds)(ref bounds)).center, layerMask);
		int count = Physics.OverlapBoxNonAlloc(((Bounds)(ref bounds)).center, ((Bounds)(ref bounds)).extents, colBuffer, Quaternion.identity, layerMask, triggerInteraction);
		BufferToList(colBuffer, count, list);
	}

	[PoolAnalyzerNonCaching]
	private static void BufferToList(Collider[] buffer, int count, List<Collider> list)
	{
		for (int i = 0; i < count; i++)
		{
			list.Add(buffer[i]);
			buffer[i] = null;
		}
	}

	public static bool CheckSphere<T>(Vector3 pos, float radius, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1) where T : Component
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		List<Collider> list = Pool.Get<List<Collider>>();
		OverlapSphere(pos, radius, list, layerMask, triggerInteraction);
		bool result = CheckComponent<T>(list);
		Pool.FreeUnmanaged<Collider>(ref list);
		return result;
	}

	public static void CheckSpheres<T>(ReadOnly<Vector3> positions, ReadOnly<float> radii, ReadOnly<int> layerMasks, NativeArray<bool> results, int maxResPerCast, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1, MasksToValidate validate = MasksToValidate.All) where T : Component
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GamePhysics.CheckSpheres<T>"))
		{
			NativeArray<ColliderHit> hits = new NativeArray<ColliderHit>(positions.Length * maxResPerCast, (Allocator)3, (NativeArrayOptions)0);
			try
			{
				JobHandle val = OverlapSpheres(positions, radii, layerMasks, hits, maxResPerCast, triggerInteraction, validate);
				((JobHandle)(ref val)).Complete();
				FindComponent<T>(hits.AsReadOnly(), maxResPerCast, results);
			}
			finally
			{
				((IDisposable)hits/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private static void FindComponent<T>(ReadOnly<ColliderHit> hits, int maxResPerCast, NativeArray<bool> results)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("FindComponent<T>"))
		{
			int num = hits.Length / maxResPerCast;
			int batchSize = ThreadUtils.GetBatchSize(num);
			int num2 = (num + batchSize - 1) / batchSize;
			if (num2 >= 4)
			{
				List<UniTask> list = new List<UniTask>();
				for (int i = 0; i < num2; i++)
				{
					int num3 = i * batchSize;
					int end = Math.Min(num3 + batchSize, num);
					list.Add(FindCompAsync(hits, num3, end, maxResPerCast, results));
				}
				ThreadUtils.WaitForTasks(list);
			}
			else
			{
				FindComp(hits, 0, num, maxResPerCast, results);
			}
		}
		static void FindComp(ReadOnly<ColliderHit> val2, int start, int num4, int num6, NativeArray<bool> val4)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			T val3 = default(T);
			for (int j = start; j < num4; j++)
			{
				bool flag = false;
				int num5 = j * num6;
				for (int k = 0; k < num6; k++)
				{
					ColliderHit val = val2[num5 + k];
					if (((ColliderHit)(ref val)).instanceID == 0)
					{
						break;
					}
					if (((Component)((ColliderHit)(ref val)).collider).TryGetComponent<T>(ref val3))
					{
						flag = true;
						break;
					}
				}
				val4[j] = flag;
			}
		}
		[AsyncStateMachine(typeof(_003C_003CFindComponent_003Eg__FindCompAsync_007C36_0_003Ed<>))]
		static UniTask FindCompAsync(ReadOnly<ColliderHit> hits2, int start, int end2, int maxResPerCast2, NativeArray<bool> results2)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			_003C_003CFindComponent_003Eg__FindCompAsync_007C36_0_003Ed<T> obj = default(_003C_003CFindComponent_003Eg__FindCompAsync_007C36_0_003Ed<T>);
			obj._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
			obj.hits = hits2;
			obj.start = start;
			obj.end = end2;
			obj.maxResPerCast = maxResPerCast2;
			obj.results = results2;
			obj._003C_003E1__state = -1;
			((AsyncUniTaskMethodBuilder)(ref obj._003C_003Et__builder)).Start<_003C_003CFindComponent_003Eg__FindCompAsync_007C36_0_003Ed<T>>(ref obj);
			return ((AsyncUniTaskMethodBuilder)(ref obj._003C_003Et__builder)).Task;
		}
	}

	public static bool CheckCapsule<T>(Vector3 start, Vector3 end, float radius, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1) where T : Component
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		List<Collider> list = Pool.Get<List<Collider>>();
		OverlapCapsule(start, end, radius, list, layerMask, triggerInteraction);
		bool result = CheckComponent<T>(list);
		Pool.FreeUnmanaged<Collider>(ref list);
		return result;
	}

	public static bool CheckOBB<T>(OBB obb, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1) where T : Component
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		List<Collider> list = Pool.Get<List<Collider>>();
		OverlapOBB(obb, list, layerMask, triggerInteraction);
		bool result = CheckComponent<T>(list);
		Pool.FreeUnmanaged<Collider>(ref list);
		return result;
	}

	public static bool CheckBounds<T>(Bounds bounds, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1) where T : Component
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		List<Collider> list = Pool.Get<List<Collider>>();
		OverlapBounds(bounds, list, layerMask, triggerInteraction);
		bool result = CheckComponent<T>(list);
		Pool.FreeUnmanaged<Collider>(ref list);
		return result;
	}

	private static bool CheckComponent<T>(List<Collider> list)
	{
		T val = default(T);
		for (int i = 0; i < list.Count; i++)
		{
			if (((Component)list[i]).gameObject.TryGetComponent<T>(ref val))
			{
				return true;
			}
		}
		return false;
	}

	[PoolAnalyzerNonCaching]
	public static void OverlapSphere<T>(Vector3 position, float radius, List<T> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1) where T : Component
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		layerMask = HandleIgnoreCollision(position, layerMask);
		int count = Physics.OverlapSphereNonAlloc(position, radius, colBuffer, layerMask, triggerInteraction);
		BufferToList(colBuffer, count, list);
	}

	public static void CheckCapsules<T>(ReadOnly<Vector3> starts, ReadOnly<Vector3> ends, ReadOnly<float> radii, ReadOnly<int> layerMasks, Span<bool> results, int maxResPerCast, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1, MasksToValidate validate = MasksToValidate.All, bool mitigateSpheres = true) where T : Component
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GamePhysics.CheckCapsules<T>"))
		{
			NativeArray<ColliderHit> hits = new NativeArray<ColliderHit>(starts.Length * maxResPerCast, (Allocator)3, (NativeArrayOptions)0);
			JobHandle val = OverlapCapsules(starts, ends, radii, layerMasks, hits, maxResPerCast, triggerInteraction, validate, mitigateSpheres);
			((JobHandle)(ref val)).Complete();
			using (TimeWarning.New("FindComponent"))
			{
				T val3 = default(T);
				for (int i = 0; i < starts.Length; i++)
				{
					bool flag = false;
					int num = i * maxResPerCast;
					for (int j = 0; j < maxResPerCast; j++)
					{
						ColliderHit val2 = hits[num + j];
						if (((ColliderHit)(ref val2)).instanceID == 0)
						{
							break;
						}
						if (((Component)((ColliderHit)(ref val2)).collider).TryGetComponent<T>(ref val3))
						{
							flag = true;
							break;
						}
					}
					results[i] = flag;
				}
				hits.Dispose();
			}
		}
	}

	[PoolAnalyzerNonCaching]
	public static void OverlapCapsule<T>(Vector3 point0, Vector3 point1, float radius, List<T> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1) where T : Component
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		layerMask = HandleIgnoreCollision(point0, layerMask);
		layerMask = HandleIgnoreCollision(point1, layerMask);
		int count = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, colBuffer, layerMask, triggerInteraction);
		BufferToList(colBuffer, count, list);
	}

	[PoolAnalyzerNonCaching]
	public static void OverlapOBB<T>(OBB obb, List<T> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1) where T : Component
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		layerMask = HandleIgnoreCollision(obb.position, layerMask);
		int count = Physics.OverlapBoxNonAlloc(obb.position, obb.extents, colBuffer, obb.rotation, layerMask, triggerInteraction);
		BufferToList(colBuffer, count, list);
	}

	[PoolAnalyzerNonCaching]
	public static void OverlapBounds<T>(Bounds bounds, List<T> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)1) where T : Component
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		layerMask = HandleIgnoreCollision(((Bounds)(ref bounds)).center, layerMask);
		int count = Physics.OverlapBoxNonAlloc(((Bounds)(ref bounds)).center, ((Bounds)(ref bounds)).extents, colBuffer, Quaternion.identity, layerMask, triggerInteraction);
		BufferToList(colBuffer, count, list);
	}

	[PoolAnalyzerNonCaching]
	private static void BufferToList<T>(Collider[] buffer, int count, List<T> list) where T : Component
	{
		T item = default(T);
		for (int i = 0; i < count; i++)
		{
			if (((Component)buffer[i]).TryGetComponent<T>(ref item))
			{
				list.Add(item);
			}
			buffer[i] = null;
		}
	}

	[PoolAnalyzerNonCaching]
	private static void HitBufferToList(int count, List<RaycastHit> list)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (count >= hitBuffer.Length)
		{
			Debug.LogWarning((object)"Physics query is exceeding collider buffer length.");
		}
		for (int i = 0; i < count; i++)
		{
			list.Add(hitBuffer[i]);
		}
	}

	public static bool TraceRealm(Realm realm, Ray ray, float radius, out RaycastHit hitInfo, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)0, BaseEntity ignoreEntity = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (Trace(ray, radius, out var hitInfo2, maxDistance, layerMask, triggerInteraction, ignoreEntity))
		{
			hitInfo = hitInfo2;
			return true;
		}
		hitInfo = default(RaycastHit);
		return false;
	}

	public static void TraceRealmRays(Realm realm, NativeArray<RaycastCommand> cmds, NativeArray<RaycastHit> hits, bool traceWater = true, ReadOnlySpan<BaseEntity> ignoreEntities = default(ReadOnlySpan<BaseEntity>))
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		TraceRays(cmds, hits, 1, traceWater, ignoreEntities);
	}

	public static BaseNetworkable TraceRealmEntity(Realm realm, Ray ray, float radius = 0f, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)0, BaseEntity ignoreEntity = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!Trace(ray, radius, out var hitInfo, maxDistance, layerMask, triggerInteraction, ignoreEntity))
		{
			return null;
		}
		return RaycastHitEx.GetEntity(hitInfo);
	}

	public static bool Trace(Ray ray, float radius, out RaycastHit hitInfo, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)0, BaseEntity ignoreEntity = null)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		List<RaycastHit> list = Pool.Get<List<RaycastHit>>();
		TraceAllUnordered(ray, radius, list, maxDistance, layerMask, triggerInteraction, ignoreEntity);
		if (list.Count == 0)
		{
			hitInfo = default(RaycastHit);
			Pool.FreeUnmanaged<RaycastHit>(ref list);
			return false;
		}
		RaycastHit val = list[0];
		float num = ((RaycastHit)(ref val)).distance;
		int index = 0;
		for (int i = 1; i < list.Count; i++)
		{
			val = list[i];
			float distance = ((RaycastHit)(ref val)).distance;
			if (distance < num)
			{
				num = distance;
				index = i;
			}
		}
		hitInfo = list[index];
		Pool.FreeUnmanaged<RaycastHit>(ref list);
		return true;
	}

	[PoolAnalyzerNonCaching]
	public static void TraceAll(Ray ray, float radius, List<RaycastHit> hits, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)0, BaseEntity ignoreEntity = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		TraceAllUnordered(ray, radius, hits, maxDistance, layerMask, triggerInteraction, ignoreEntity);
		Sort(hits);
	}

	[PoolAnalyzerNonCaching]
	public static void TraceAllUnordered(Ray ray, float radius, List<RaycastHit> hits, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction triggerInteraction = (QueryTriggerInteraction)0, BaseEntity ignoreEntity = null)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		int num = ((radius != 0f) ? Physics.SphereCastNonAlloc(ray, radius, hitBuffer, maxDistance, layerMask, triggerInteraction) : Physics.RaycastNonAlloc(ray, hitBuffer, maxDistance, layerMask, triggerInteraction));
		if (num < hitBuffer.Length && (layerMask & 0x10) != 0 && WaterSystem.Trace(ray, out var position, out var normal, maxDistance))
		{
			RaycastHit val = default(RaycastHit);
			((RaycastHit)(ref val)).point = position;
			((RaycastHit)(ref val)).normal = normal;
			Vector3 val2 = position - ((Ray)(ref ray)).origin;
			((RaycastHit)(ref val)).distance = ((Vector3)(ref val2)).magnitude;
			RaycastHit val3 = val;
			hitBuffer[num++] = val3;
		}
		if (num == 0)
		{
			return;
		}
		if (num >= hitBuffer.Length)
		{
			Debug.LogWarning((object)"Physics query is exceeding hit buffer length.");
		}
		for (int i = 0; i < num; i++)
		{
			RaycastHit val4 = hitBuffer[i];
			if (Verify(val4, ((Ray)(ref ray)).origin, ignoreEntity))
			{
				hits.Add(val4);
			}
		}
	}

	public static void TraceRaysUnordered(NativeArray<RaycastCommand> rays, NativeArray<RaycastHit> hits, int maxHitsPerTrace, bool traceWater = true, ReadOnlySpan<BaseEntity> ignoreEntities = default(ReadOnlySpan<BaseEntity>))
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GamePhysics.TraceRaysUnordered"))
		{
			int batchSize = ThreadUtils.GetBatchSize(rays.Length);
			JobHandle val = RaycastCommand.ScheduleBatch(rays, hits, batchSize, maxHitsPerTrace, default(JobHandle));
			if (traceWater)
			{
				if (false)
				{
					JobHandle.ScheduleBatchedJobs();
					NativeArray<RaycastHit> hits2;
					hits2._002Ector(rays.Length, (Allocator)3, (NativeArrayOptions)1);
					JobHandle val2 = TraceWaterRaysDeferred(hits2, rays, 1, default(JobHandle));
					JobHandle val3 = JobHandle.CombineDependencies(val, val2);
					GamePhysicsJobs.AppendRaycastHitsJob appendRaycastHitsJob = new GamePhysicsJobs.AppendRaycastHitsJob
					{
						Dst = hits,
						Src = hits2.AsReadOnly(),
						DstMaxHitsPerBatch = maxHitsPerTrace,
						SrcMaxHitsPerBatch = 1
					};
					val = IJobExtensions.ScheduleByRef<GamePhysicsJobs.AppendRaycastHitsJob>(ref appendRaycastHitsJob, val3);
					hits2.Dispose(val);
				}
				else
				{
					val = TraceWaterRaysDeferred(hits, rays, maxHitsPerTrace, val);
				}
			}
			((JobHandle)(ref val)).Complete();
			VerifyRays(hits, rays, maxHitsPerTrace, ignoreEntities);
		}
	}

	public static JobHandle TraceWaterRaysDeferred(NativeArray<RaycastHit> hits, NativeArray<RaycastCommand> rays, int maxHitsPerTrace, JobHandle inputDeps)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ScheduleTraceWaterRaysDeferred"))
		{
			if (!rays.IsCreated || rays.Length == 0)
			{
				return inputDeps;
			}
			NativeList<Vector2i> waterIndices = new NativeList<Vector2i>(rays.Length, AllocatorHandle.op_Implicit((Allocator)3));
			NativeList<Ray> val = new NativeList<Ray>(rays.Length, AllocatorHandle.op_Implicit((Allocator)3));
			NativeList<int> deepIndices = new NativeList<int>(rays.Length, AllocatorHandle.op_Implicit((Allocator)3));
			NativeList<int> mainIndices = new NativeList<int>(rays.Length, AllocatorHandle.op_Implicit((Allocator)3));
			NativeArray<bool> val2 = new NativeArray<bool>(rays.Length, (Allocator)3, (NativeArrayOptions)0);
			NativeArray<float> val3 = new NativeArray<float>(rays.Length, (Allocator)3, (NativeArrayOptions)0);
			NativeArray<Vector3> val4 = new NativeArray<Vector3>(rays.Length, (Allocator)3, (NativeArrayOptions)0);
			NativeArray<Vector3> val5 = new NativeArray<Vector3>(rays.Length, (Allocator)3, (NativeArrayOptions)0);
			GamePhysicsJobs.PreProcessWaterRaysJob preProcessWaterRaysJob = new GamePhysicsJobs.PreProcessWaterRaysJob
			{
				hits = hits.AsReadOnly(),
				rays = rays.AsReadOnly(),
				maxHitsPerTrace = maxHitsPerTrace,
				WaterIndices = waterIndices,
				WaterRays = val,
				WaterMaxDists = val3,
				DeepIndices = deepIndices,
				MainIndices = mainIndices,
				DeepSeaBounds = DeepSeaManager.DeepSeaBounds
			};
			inputDeps = IJobExtensions.ScheduleByRef<GamePhysicsJobs.PreProcessWaterRaysJob>(ref preProcessWaterRaysJob, inputDeps);
			inputDeps = WaterSystem.ScheduleTraceBatchDefer(val, val3, val2, val4, val5, deepIndices, mainIndices, inputDeps);
			GamePhysicsJobs.PostProcessWaterRaysJob postProcessWaterRaysJob = new GamePhysicsJobs.PostProcessWaterRaysJob
			{
				hits = hits,
				rays = val.AsDeferredJobArray(),
				WaterIndices = waterIndices,
				hitsSub = val2,
				positionsSub = val4,
				normalsSub = val5
			};
			inputDeps = IJobExtensions.ScheduleByRef<GamePhysicsJobs.PostProcessWaterRaysJob>(ref postProcessWaterRaysJob, inputDeps);
			waterIndices.Dispose(inputDeps);
			val.Dispose(inputDeps);
			deepIndices.Dispose(inputDeps);
			mainIndices.Dispose(inputDeps);
			val2.Dispose(inputDeps);
			val3.Dispose(inputDeps);
			val4.Dispose(inputDeps);
			val5.Dispose(inputDeps);
			return inputDeps;
		}
	}

	public static void VerifyRays(NativeArray<RaycastHit> hits, NativeArray<RaycastCommand> rays, int maxHitsPerCast, ReadOnlySpan<BaseEntity> ignoreEntities = default(ReadOnlySpan<BaseEntity>))
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		if (rays.Length == 0)
		{
			return;
		}
		using (TimeWarning.New("VerifyRays"))
		{
			Debug.Assert(ignoreEntities.Length == 0 || rays.Length == ignoreEntities.Length);
			hits = hits.GetSubArray(0, rays.Length * maxHitsPerCast);
			NativeList<RaycastHit> colliderHits = new NativeList<RaycastHit>(hits.Length, AllocatorHandle.op_Implicit((Allocator)3));
			try
			{
				NativeList<int> colliderIndices = new NativeList<int>(hits.Length, AllocatorHandle.op_Implicit((Allocator)3));
				try
				{
					NativeList<Vector3> waterHits = new NativeList<Vector3>(hits.Length, AllocatorHandle.op_Implicit((Allocator)3));
					try
					{
						NativeList<int> waterIndices = new NativeList<int>(hits.Length, AllocatorHandle.op_Implicit((Allocator)3));
						try
						{
							FilterRaycastHitsJob filterRaycastHitsJob = new FilterRaycastHitsJob
							{
								ColliderHits = colliderHits,
								ColliderIndices = colliderIndices,
								WaterHits = waterHits,
								WaterIndices = waterIndices,
								Hits = hits.AsReadOnly(),
								HitsPerBatch = maxHitsPerCast
							};
							IJobExtensions.RunByRef<FilterRaycastHitsJob>(ref filterRaycastHitsJob);
							NativeArray<bool> results = new NativeArray<bool>(hits.Length, (Allocator)3, (NativeArrayOptions)0);
							NativeArray<bool> res = new NativeArray<bool>(waterHits.Length, (Allocator)3, (NativeArrayOptions)0);
							try
							{
								VerifyWaterHits(waterHits.AsReadOnly(), res);
								ScatterToJob<bool> scatterToJob = new ScatterToJob<bool>
								{
									Results = results,
									Source = res.AsReadOnly(),
									Indices = waterIndices.AsReadOnly()
								};
								IJobExtensions.RunByRef<ScatterToJob<bool>>(ref scatterToJob);
								using (TimeWarning.New("VerifyColliderHits"))
								{
									waterHits.Clear();
									if (waterHits.Capacity < colliderHits.Length)
									{
										waterHits.SetCapacity(colliderHits.Length);
									}
									waterIndices.Clear();
									if (waterIndices.Capacity < colliderHits.Length)
									{
										waterIndices.SetCapacity(colliderHits.Length);
									}
									for (int i = 0; i < colliderIndices.Length; i++)
									{
										RaycastHit val = colliderHits[i];
										int num = colliderIndices[i];
										Collider collider = ((RaycastHit)(ref val)).collider;
										if (collider is TerrainCollider)
										{
											Vector3 val2 = ((RaycastHit)(ref val)).point;
											if (val2 == Vector3.zero && ((RaycastHit)(ref val)).distance == 0f)
											{
												int num2 = num / maxHitsPerCast;
												RaycastCommand val3 = rays[num2];
												val2 = ((RaycastCommand)(ref val3)).from;
											}
											waterHits.AddNoResize(val2);
											waterIndices.AddNoResize(num);
											continue;
										}
										bool flag = true;
										if (ignoreEntities != default(ReadOnlySpan<BaseEntity>))
										{
											int index = num / maxHitsPerCast;
											BaseEntity b = ignoreEntities[index];
											if (CompareEntity(GameObjectEx.ToBaseEntity(collider), b))
											{
												flag = false;
											}
										}
										if (flag)
										{
											flag = collider.enabled;
										}
										results[num] = flag;
									}
								}
								NativeArray<bool> res2 = new NativeArray<bool>(hits.Length, (Allocator)3, (NativeArrayOptions)0);
								try
								{
									VerifyTerrainColliderHits(waterHits.AsReadOnly(), res2);
									ScatterToJob<bool> scatterToJob2 = new ScatterToJob<bool>
									{
										Results = results,
										Source = res2.AsReadOnly(),
										Indices = waterIndices.AsReadOnly()
									};
									IJobExtensions.RunByRef<ScatterToJob<bool>>(ref scatterToJob2);
									RemoveInvalidRaycastHitsJob removeInvalidRaycastHitsJob = new RemoveInvalidRaycastHitsJob
									{
										Hits = hits,
										AreValid = results.AsReadOnly(),
										HitsPerBatch = maxHitsPerCast
									};
									IJobExtensions.RunByRef<RemoveInvalidRaycastHitsJob>(ref removeInvalidRaycastHitsJob);
									results.Dispose();
								}
								finally
								{
									((IDisposable)res2/*cast due to constrained. prefix*/).Dispose();
								}
							}
							finally
							{
								((IDisposable)res/*cast due to constrained. prefix*/).Dispose();
							}
						}
						finally
						{
							((IDisposable)waterIndices/*cast due to constrained. prefix*/).Dispose();
						}
					}
					finally
					{
						((IDisposable)waterHits/*cast due to constrained. prefix*/).Dispose();
					}
				}
				finally
				{
					((IDisposable)colliderIndices/*cast due to constrained. prefix*/).Dispose();
				}
			}
			finally
			{
				((IDisposable)colliderHits/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private static void VerifyWaterHits(ReadOnly<Vector3> hits, NativeArray<bool> res)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if (hits.Length == 0)
		{
			return;
		}
		using (TimeWarning.New("VerifyWaterHits"))
		{
			if ((Object)(object)WaterSystem.Collision == (Object)null)
			{
				FillJob<bool> fillJob = new FillJob<bool>
				{
					Values = res,
					Value = true
				};
				IJobExtensions.RunByRef<FillJob<bool>>(ref fillJob);
				return;
			}
			NativeArray<float> values = new NativeArray<float>(hits.Length, (Allocator)3, (NativeArrayOptions)0);
			try
			{
				FillJob<float> fillJob2 = new FillJob<float>
				{
					Values = values,
					Value = 0.01f
				};
				IJobExtensions.RunByRef<FillJob<float>>(ref fillJob2);
				WaterSystem.Collision.GetIgnore(hits, values.AsReadOnly(), res);
				FlipBoolJob flipBoolJob = new FlipBoolJob
				{
					Values = res
				};
				IJobExtensions.RunByRef<FlipBoolJob>(ref flipBoolJob);
			}
			finally
			{
				((IDisposable)values/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private static void VerifyTerrainColliderHits(ReadOnly<Vector3> hits, NativeArray<bool> res)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if (hits.Length == 0)
		{
			return;
		}
		using (TimeWarning.New("VerifyTerrainColliderHits"))
		{
			if ((Object)(object)TerrainMeta.Collision == (Object)null)
			{
				FillJob<bool> fillJob = new FillJob<bool>
				{
					Values = res,
					Value = true
				};
				IJobExtensions.RunByRef<FillJob<bool>>(ref fillJob);
				return;
			}
			NativeArray<float> values = new NativeArray<float>(hits.Length, (Allocator)3, (NativeArrayOptions)0);
			try
			{
				FillJob<float> fillJob2 = new FillJob<float>
				{
					Values = values,
					Value = 0.01f
				};
				IJobExtensions.RunByRef<FillJob<float>>(ref fillJob2);
				TerrainMeta.Collision.GetIgnore(hits, values.AsReadOnly(), res);
				FlipBoolJob flipBoolJob = new FlipBoolJob
				{
					Values = res
				};
				IJobExtensions.RunByRef<FlipBoolJob>(ref flipBoolJob);
			}
			finally
			{
				((IDisposable)values/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	public static void TraceRays(NativeArray<RaycastCommand> rays, NativeArray<RaycastHit> hits, int maxHitsPerTrace, bool traceWater = true, ReadOnlySpan<BaseEntity> ignoreEntities = default(ReadOnlySpan<BaseEntity>))
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (rays.Length != 0)
		{
			int num = Mathf.Max(32, maxHitsPerTrace * 2);
			NativeArray<RaycastHit> val = default(NativeArray<RaycastHit>);
			val._002Ector(rays.Length * num, (Allocator)3, (NativeArrayOptions)0);
			TraceRaysUnordered(rays, val, num, traceWater, ignoreEntities);
			NativeArray<RaycastHit> val2 = val;
			int length = rays.Length;
			JobHandle dependsOn = default(JobHandle);
			dependsOn = SelectNearest(maxHitsPerTrace, val2, hits, length, num, dependsOn);
			((JobHandle)(ref dependsOn)).Complete();
			val.Dispose();
		}
	}

	public static void TraceSpheresUnordered(NativeArray<SpherecastCommand> spheres, NativeArray<RaycastHit> hits, int maxHitsPerTrace, bool traceWater = true, ReadOnlySpan<BaseEntity> ignoreEntities = default(ReadOnlySpan<BaseEntity>))
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GamePhysics.TraceSpheresUnordered"))
		{
			int batchSize = ThreadUtils.GetBatchSize(spheres.Length);
			JobHandle inputDeps = SpherecastCommand.ScheduleBatch(spheres, hits, batchSize, maxHitsPerTrace, default(JobHandle));
			if (traceWater)
			{
				inputDeps = TraceWaterSpheresDeferred(hits, spheres, maxHitsPerTrace, inputDeps);
			}
			((JobHandle)(ref inputDeps)).Complete();
			VerifySpheres(hits, spheres, maxHitsPerTrace, ignoreEntities);
		}
	}

	public static JobHandle TraceWaterSpheresDeferred(NativeArray<RaycastHit> hits, NativeArray<SpherecastCommand> spheres, int maxHitsPerTrace, JobHandle inputDeps)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ScheduleTraceWaterSpheresDeferred"))
		{
			if (!spheres.IsCreated || spheres.Length == 0)
			{
				return inputDeps;
			}
			NativeList<Vector2i> waterIndices = new NativeList<Vector2i>(spheres.Length, AllocatorHandle.op_Implicit((Allocator)3));
			NativeList<Ray> val = new NativeList<Ray>(spheres.Length, AllocatorHandle.op_Implicit((Allocator)3));
			NativeList<int> deepIndices = new NativeList<int>(spheres.Length, AllocatorHandle.op_Implicit((Allocator)3));
			NativeList<int> mainIndices = new NativeList<int>(spheres.Length, AllocatorHandle.op_Implicit((Allocator)3));
			NativeArray<bool> val2 = new NativeArray<bool>(spheres.Length, (Allocator)3, (NativeArrayOptions)0);
			NativeArray<float> val3 = new NativeArray<float>(spheres.Length, (Allocator)3, (NativeArrayOptions)0);
			NativeArray<Vector3> val4 = new NativeArray<Vector3>(spheres.Length, (Allocator)3, (NativeArrayOptions)0);
			NativeArray<Vector3> val5 = new NativeArray<Vector3>(spheres.Length, (Allocator)3, (NativeArrayOptions)0);
			GamePhysicsJobs.PreProcessWaterSpheresJob preProcessWaterSpheresJob = new GamePhysicsJobs.PreProcessWaterSpheresJob
			{
				hits = hits.AsReadOnly(),
				rays = spheres.AsReadOnly(),
				maxHitsPerTrace = maxHitsPerTrace,
				WaterIndices = waterIndices,
				WaterRays = val,
				WaterMaxDists = val3,
				DeepIndices = deepIndices,
				MainIndices = mainIndices,
				DeepSeaBounds = DeepSeaManager.DeepSeaBounds
			};
			inputDeps = IJobExtensions.ScheduleByRef<GamePhysicsJobs.PreProcessWaterSpheresJob>(ref preProcessWaterSpheresJob, inputDeps);
			inputDeps = WaterSystem.ScheduleTraceBatchDefer(val, val3, val2, val4, val5, deepIndices, mainIndices, inputDeps);
			GamePhysicsJobs.PostProcessWaterRaysJob postProcessWaterRaysJob = new GamePhysicsJobs.PostProcessWaterRaysJob
			{
				hits = hits,
				rays = val.AsDeferredJobArray(),
				WaterIndices = waterIndices,
				hitsSub = val2,
				positionsSub = val4,
				normalsSub = val5
			};
			inputDeps = IJobExtensions.ScheduleByRef<GamePhysicsJobs.PostProcessWaterRaysJob>(ref postProcessWaterRaysJob, inputDeps);
			waterIndices.Dispose(inputDeps);
			val.Dispose(inputDeps);
			deepIndices.Dispose(inputDeps);
			mainIndices.Dispose(inputDeps);
			val2.Dispose(inputDeps);
			val3.Dispose(inputDeps);
			val4.Dispose(inputDeps);
			val5.Dispose(inputDeps);
			return inputDeps;
		}
	}

	public static void VerifySpheres(NativeArray<RaycastHit> hits, NativeArray<SpherecastCommand> spheres, int maxHitsPerCast, ReadOnlySpan<BaseEntity> ignoreEntities = default(ReadOnlySpan<BaseEntity>))
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		if (spheres.Length == 0)
		{
			return;
		}
		using (TimeWarning.New("VerifySpheres"))
		{
			Debug.Assert(ignoreEntities.IsEmpty || spheres.Length == ignoreEntities.Length);
			hits = hits.GetSubArray(0, spheres.Length * maxHitsPerCast);
			NativeList<RaycastHit> colliderHits = new NativeList<RaycastHit>(hits.Length, AllocatorHandle.op_Implicit((Allocator)3));
			try
			{
				NativeList<int> colliderIndices = new NativeList<int>(hits.Length, AllocatorHandle.op_Implicit((Allocator)3));
				try
				{
					NativeList<Vector3> waterHits = new NativeList<Vector3>(hits.Length, AllocatorHandle.op_Implicit((Allocator)3));
					try
					{
						NativeList<int> waterIndices = new NativeList<int>(hits.Length, AllocatorHandle.op_Implicit((Allocator)3));
						try
						{
							FilterRaycastHitsJob filterRaycastHitsJob = new FilterRaycastHitsJob
							{
								ColliderHits = colliderHits,
								ColliderIndices = colliderIndices,
								WaterHits = waterHits,
								WaterIndices = waterIndices,
								Hits = hits.AsReadOnly(),
								HitsPerBatch = maxHitsPerCast
							};
							IJobExtensions.RunByRef<FilterRaycastHitsJob>(ref filterRaycastHitsJob);
							NativeArray<bool> results = new NativeArray<bool>(hits.Length, (Allocator)3, (NativeArrayOptions)0);
							NativeArray<bool> res = new NativeArray<bool>(waterHits.Length, (Allocator)3, (NativeArrayOptions)0);
							try
							{
								VerifyWaterHits(waterHits.AsReadOnly(), res);
								ScatterToJob<bool> scatterToJob = new ScatterToJob<bool>
								{
									Results = results,
									Source = res.AsReadOnly(),
									Indices = waterIndices.AsReadOnly()
								};
								IJobExtensions.RunByRef<ScatterToJob<bool>>(ref scatterToJob);
								using (TimeWarning.New("VerifyColliderHits"))
								{
									waterHits.Clear();
									if (waterHits.Capacity < colliderHits.Length)
									{
										waterHits.SetCapacity(colliderHits.Length);
									}
									waterIndices.Clear();
									if (waterIndices.Capacity < colliderHits.Length)
									{
										waterIndices.SetCapacity(colliderHits.Length);
									}
									for (int i = 0; i < colliderIndices.Length; i++)
									{
										RaycastHit val = colliderHits[i];
										int num = colliderIndices[i];
										Collider collider = ((RaycastHit)(ref val)).collider;
										if (collider is TerrainCollider)
										{
											Vector3 val2 = ((RaycastHit)(ref val)).point;
											if (val2 == Vector3.zero && ((RaycastHit)(ref val)).distance == 0f)
											{
												int num2 = num / maxHitsPerCast;
												SpherecastCommand val3 = spheres[num2];
												val2 = ((SpherecastCommand)(ref val3)).origin;
											}
											waterHits.AddNoResize(val2);
											waterIndices.AddNoResize(num);
											continue;
										}
										bool flag = true;
										if (ignoreEntities != default(ReadOnlySpan<BaseEntity>))
										{
											int index = num / maxHitsPerCast;
											BaseEntity b = ignoreEntities[index];
											if (CompareEntity(GameObjectEx.ToBaseEntity(collider), b))
											{
												flag = false;
											}
										}
										if (flag)
										{
											flag = collider.enabled;
										}
										results[num] = flag;
									}
								}
								NativeArray<bool> res2 = new NativeArray<bool>(hits.Length, (Allocator)3, (NativeArrayOptions)0);
								try
								{
									VerifyTerrainColliderHits(waterHits.AsReadOnly(), res2);
									ScatterToJob<bool> scatterToJob2 = new ScatterToJob<bool>
									{
										Results = results,
										Source = res2.AsReadOnly(),
										Indices = waterIndices.AsReadOnly()
									};
									IJobExtensions.RunByRef<ScatterToJob<bool>>(ref scatterToJob2);
									RemoveInvalidRaycastHitsJob removeInvalidRaycastHitsJob = new RemoveInvalidRaycastHitsJob
									{
										Hits = hits,
										AreValid = results.AsReadOnly(),
										HitsPerBatch = maxHitsPerCast
									};
									IJobExtensions.RunByRef<RemoveInvalidRaycastHitsJob>(ref removeInvalidRaycastHitsJob);
									results.Dispose();
								}
								finally
								{
									((IDisposable)res2/*cast due to constrained. prefix*/).Dispose();
								}
							}
							finally
							{
								((IDisposable)res/*cast due to constrained. prefix*/).Dispose();
							}
						}
						finally
						{
							((IDisposable)waterIndices/*cast due to constrained. prefix*/).Dispose();
						}
					}
					finally
					{
						((IDisposable)waterHits/*cast due to constrained. prefix*/).Dispose();
					}
				}
				finally
				{
					((IDisposable)colliderIndices/*cast due to constrained. prefix*/).Dispose();
				}
			}
			finally
			{
				((IDisposable)colliderHits/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	public static void TraceSpheres(NativeArray<SpherecastCommand> spheres, NativeArray<RaycastHit> hits, int maxHitsPerTrace, bool traceWater = true, ReadOnlySpan<BaseEntity> ignoreEntities = default(ReadOnlySpan<BaseEntity>))
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (spheres.Length != 0)
		{
			int num = Mathf.Max(32, maxHitsPerTrace * 2);
			NativeArray<RaycastHit> val = default(NativeArray<RaycastHit>);
			val._002Ector(spheres.Length * num, (Allocator)3, (NativeArrayOptions)0);
			TraceSpheresUnordered(spheres, val, num, traceWater, ignoreEntities);
			NativeArray<RaycastHit> val2 = val;
			int length = spheres.Length;
			JobHandle dependsOn = default(JobHandle);
			dependsOn = SelectNearest(maxHitsPerTrace, val2, hits, length, num, dependsOn);
			((JobHandle)(ref dependsOn)).Complete();
			val.Dispose();
		}
	}

	public static bool LineOfSightRadius(Vector3 p0, Vector3 p1, int layerMask, float radius, float padding0, float padding1, BaseEntity ignoreEntity = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return LineOfSightInternal(p0, p1, layerMask, radius, padding0, padding1, ignoreEntity);
	}

	public static bool LineOfSightRadius(Vector3 p0, Vector3 p1, int layerMask, float radius, float padding, BaseEntity ignoreEntity = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return LineOfSightInternal(p0, p1, layerMask, radius, padding, padding, ignoreEntity);
	}

	public static bool LineOfSightRadius(Vector3 p0, Vector3 p1, int layerMask, float radius, BaseEntity ignoreEntity = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return LineOfSightInternal(p0, p1, layerMask, radius, 0f, 0f, ignoreEntity);
	}

	public static bool LineOfSight(Vector3 p0, Vector3 p1, int layerMask, float padding0, float padding1, BaseEntity ignoreEntity = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return LineOfSightRadius(p0, p1, layerMask, 0f, padding0, padding1, ignoreEntity);
	}

	public static bool LineOfSight(Vector3 p0, Vector3 p1, int layerMask, float padding, BaseEntity ignoreEntity = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return LineOfSightRadius(p0, p1, layerMask, 0f, padding, padding, ignoreEntity);
	}

	public static bool LineOfSight(Vector3 p0, Vector3 p1, int layerMask, BaseEntity ignoreEntity = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return LineOfSightRadius(p0, p1, layerMask, 0f, 0f, 0f, ignoreEntity);
	}

	private static bool LineOfSightInternal(Vector3 p0, Vector3 p1, int layerMask, float radius, float padding0, float padding1, BaseEntity ignoreEntity = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		if (!ValidBounds.TestOuterBounds(p0))
		{
			return false;
		}
		if (!ValidBounds.TestOuterBounds(p1))
		{
			return false;
		}
		Vector3 val = p1 - p0;
		float magnitude = ((Vector3)(ref val)).magnitude;
		if (magnitude <= padding0 + padding1)
		{
			return true;
		}
		Vector3 val2 = val / magnitude;
		Ray val3 = default(Ray);
		((Ray)(ref val3))._002Ector(p0 + val2 * padding0, val2);
		float num = magnitude - padding0 - padding1;
		bool flag;
		RaycastHit hitInfo = default(RaycastHit);
		if (!ignoreEntity.IsRealNull() || (layerMask & 0x800000) != 0)
		{
			flag = Trace(val3, 0f, out hitInfo, num, layerMask, (QueryTriggerInteraction)1, ignoreEntity);
			if (radius > 0f && !flag)
			{
				flag = Trace(val3, radius, out hitInfo, num, layerMask, (QueryTriggerInteraction)1, ignoreEntity);
			}
		}
		else
		{
			flag = Physics.Raycast(val3, ref hitInfo, num, layerMask, (QueryTriggerInteraction)1);
			if (radius > 0f && !flag)
			{
				flag = Physics.SphereCast(val3, radius, ref hitInfo, num, layerMask, (QueryTriggerInteraction)1);
			}
		}
		if (!flag)
		{
			if (ConVar.Vis.lineofsight)
			{
				ConsoleNetwork.BroadcastToAllClients("ddraw.line", 60f, Color.green, p0, p1);
			}
			return true;
		}
		if (ConVar.Vis.lineofsight)
		{
			ConsoleNetwork.BroadcastToAllClients("ddraw.line", 60f, Color.red, p0, p1);
			ConsoleNetwork.BroadcastToAllClients("ddraw.text", 60f, Color.white, ((RaycastHit)(ref hitInfo)).point, ((Object)((RaycastHit)(ref hitInfo)).collider).name);
		}
		return false;
	}

	public static bool Verify(RaycastHit hitInfo, Vector3 rayOrigin, BaseEntity ignoreEntity = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((RaycastHit)(ref hitInfo)).point;
		if (((RaycastHit)(ref hitInfo)).collider is TerrainCollider && val == Vector3.zero && ((RaycastHit)(ref hitInfo)).distance == 0f)
		{
			val = rayOrigin;
		}
		return Verify(((RaycastHit)(ref hitInfo)).collider, val, ignoreEntity);
	}

	public static bool Verify(Collider collider, Vector3 point, BaseEntity ignoreEntity = null)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)collider == (Object)null)
		{
			if (Object.op_Implicit((Object)(object)WaterSystem.Collision) && WaterSystem.Collision.GetIgnore(point))
			{
				return false;
			}
			return true;
		}
		if (collider is TerrainCollider)
		{
			if (Object.op_Implicit((Object)(object)TerrainMeta.Collision) && TerrainMeta.Collision.GetIgnore(point))
			{
				return false;
			}
			return true;
		}
		if (!ignoreEntity.IsRealNull() && CompareEntity(GameObjectEx.ToBaseEntity(collider), ignoreEntity))
		{
			return false;
		}
		return collider.enabled;
	}

	public static bool CompareEntity(BaseEntity a, BaseEntity b)
	{
		if (a.IsRealNull() || b.IsRealNull())
		{
			return false;
		}
		if ((Object)(object)a == (Object)(object)b)
		{
			return true;
		}
		return false;
	}

	public static int HandleIgnoreCollision(Vector3 position, int layerMask)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		int num = 8388608;
		if ((layerMask & num) != 0 && Object.op_Implicit((Object)(object)TerrainMeta.Collision) && TerrainMeta.Collision.GetIgnore(position))
		{
			layerMask &= ~num;
		}
		int num2 = 16;
		if ((layerMask & num2) != 0 && Object.op_Implicit((Object)(object)WaterSystem.Collision) && WaterSystem.Collision.GetIgnore(position))
		{
			layerMask &= ~num2;
		}
		return layerMask;
	}

	public static void HandleIgnoreTerrain(ReadOnly<Vector3> positions, NativeArray<bool> hitIgnoreVolumes)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		NativeArray<float> values = default(NativeArray<float>);
		values._002Ector(positions.Length, (Allocator)3, (NativeArrayOptions)0);
		FillJob<float> fillJob = new FillJob<float>
		{
			Values = values,
			Value = 0.01f
		};
		IJobExtensions.RunByRef<FillJob<float>>(ref fillJob);
		TerrainMeta.Collision.GetIgnore(positions, values.AsReadOnly(), hitIgnoreVolumes);
		values.Dispose();
	}

	public static void HandleIgnoreWater(ReadOnly<Vector3> positions, NativeArray<bool> hitIgnoreVolumes)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		NativeArray<float> values = default(NativeArray<float>);
		values._002Ector(positions.Length, (Allocator)3, (NativeArrayOptions)0);
		FillJob<float> fillJob = new FillJob<float>
		{
			Values = values,
			Value = 0.01f
		};
		IJobExtensions.RunByRef<FillJob<float>>(ref fillJob);
		WaterSystem.Collision.GetIgnore(positions, values.AsReadOnly(), hitIgnoreVolumes);
		values.Dispose();
	}

	public static void HandleIgnoreCollision(ReadOnly<Vector3> positions, NativeArray<int> layerMasks, MasksToValidate validate = MasksToValidate.All)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if ((validate & MasksToValidate.Terrain) == MasksToValidate.Terrain)
		{
			NativeArray<bool> hitIgnoreVolumes = default(NativeArray<bool>);
			hitIgnoreVolumes._002Ector(positions.Length, (Allocator)3, (NativeArrayOptions)0);
			HandleIgnoreTerrain(positions, hitIgnoreVolumes);
			RemoveLayerMaskJob removeLayerMaskJob = new RemoveLayerMaskJob
			{
				LayerMasks = layerMasks,
				ShouldIgnore = hitIgnoreVolumes.AsReadOnly(),
				MaskToRemove = 8388608
			};
			IJobExtensions.RunByRef<RemoveLayerMaskJob>(ref removeLayerMaskJob);
			hitIgnoreVolumes.Dispose();
		}
		if ((validate & MasksToValidate.Water) == MasksToValidate.Water)
		{
			NativeArray<bool> hitIgnoreVolumes2 = default(NativeArray<bool>);
			hitIgnoreVolumes2._002Ector(positions.Length, (Allocator)3, (NativeArrayOptions)0);
			HandleIgnoreWater(positions, hitIgnoreVolumes2);
			RemoveLayerMaskJob removeLayerMaskJob2 = new RemoveLayerMaskJob
			{
				LayerMasks = layerMasks,
				ShouldIgnore = hitIgnoreVolumes2.AsReadOnly(),
				MaskToRemove = 16
			};
			IJobExtensions.RunByRef<RemoveLayerMaskJob>(ref removeLayerMaskJob2);
			hitIgnoreVolumes2.Dispose();
		}
	}

	[PoolAnalyzerNonCaching]
	public static void Sort(List<RaycastHit> hits)
	{
		hits.Sort((RaycastHit a, RaycastHit b) => ((RaycastHit)(ref a)).distance.CompareTo(((RaycastHit)(ref b)).distance));
	}

	public static void Sort(RaycastHit[] hits)
	{
		Array.Sort(hits, (RaycastHit a, RaycastHit b) => ((RaycastHit)(ref a)).distance.CompareTo(((RaycastHit)(ref b)).distance));
	}

	public static void Sort(NativeArray<RaycastHit> hits, int queryCount, int maxHitsPerQuery)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GamePhysics.Sort"))
		{
			JobHandle dependsOn = default(JobHandle);
			dependsOn = SortDeferred(hits, queryCount, maxHitsPerQuery, dependsOn);
			((JobHandle)(ref dependsOn)).Complete();
		}
	}

	public static JobHandle SortDeferred(NativeArray<RaycastHit> hits, int queryCount, int maxHitsPerQuery, JobHandle dependsOn = default(JobHandle))
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GamePhysics.SortDeferred"))
		{
			SortHitsJob<RaycastHitComparer> sortHitsJob = new SortHitsJob<RaycastHitComparer>
			{
				Hits = hits,
				MaxHitsPerRay = maxHitsPerQuery,
				Comp = default(RaycastHitComparer)
			};
			int batchSize = ThreadUtils.GetBatchSize(queryCount);
			return IJobForExtensions.ScheduleParallelByRef<SortHitsJob<RaycastHitComparer>>(ref sortHitsJob, queryCount, batchSize, dependsOn);
		}
	}

	public static JobHandle SelectNearest(int nearestCount, NativeArray<RaycastHit> from, NativeArray<RaycastHit> to, int queryCount, int maxHitsPerQuery, JobHandle dependsOn = default(JobHandle))
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		Debug.Assert(maxHitsPerQuery > 0, "Invalid maxHitsPerQuery param!");
		Debug.Assert(nearestCount < maxHitsPerQuery, "Invalid nearestCount param!");
		if (nearestCount == 1)
		{
			SelectNearestHitsJob selectNearestHitsJob = new SelectNearestHitsJob
			{
				Results = to,
				Hits = from.AsReadOnly(),
				HitsPerBatch = maxHitsPerQuery
			};
			return IJobExtensions.ScheduleByRef<SelectNearestHitsJob>(ref selectNearestHitsJob, dependsOn);
		}
		JobHandle val = SortDeferred(from, queryCount, maxHitsPerQuery, dependsOn);
		SelectNearestNHitsJob selectNearestNHitsJob = new SelectNearestNHitsJob
		{
			Results = to,
			Hits = from.AsReadOnly(),
			HitsPerBatch = maxHitsPerQuery,
			SelectCount = nearestCount
		};
		return IJobExtensions.ScheduleByRef<SelectNearestNHitsJob>(ref selectNearestNHitsJob, val);
	}
}

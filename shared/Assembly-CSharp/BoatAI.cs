using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using Rust;
using Rust.Ai;
using Rust.Assertions;
using Rust.Safety;
using Unity.Collections;
using UnityEngine;

public class BoatAI : BaseEntity
{
	private struct FeelerResult
	{
		public Vector3 direction;

		public Nullable hit;
	}

	public struct Context(int resolution)
	{
		public float[] InterestMap = new float[resolution];

		public float[] DangerMap = new float[resolution];
	}

	public abstract class BoatState
	{
		public abstract void Enter(BoatAI boatAI);

		public abstract void Update(Context ctx, BoatAI boatAI, float delta);

		public abstract void Exit(BoatAI boatAI);

		public abstract string GetStateName();
	}

	public class IdleState : BoatState
	{
		public override void Enter(BoatAI boatAI)
		{
			boatAI.StartEngine(boatAI.Boat);
			boatAI.SwitchState(boatAI._wanderState);
		}

		public override void Update(Context ctx, BoatAI boatAI, float delta)
		{
		}

		public override void Exit(BoatAI boatAI)
		{
		}

		public override string GetStateName()
		{
			return "Idle";
		}
	}

	public class WaitState : BoatState
	{
		private const float MAX_WAIT_TIME = 60f;

		private const float MIN_WAIT_TIME = 10f;

		private TimeSince _timeSinceEntered;

		private float _timeToWait;

		public override void Enter(BoatAI boatAI)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			_timeSinceEntered = TimeSince.op_Implicit(0f);
			_timeToWait = Random.Range(10f, 60f);
		}

		public override void Update(Context ctx, BoatAI boatAI, float delta)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			if (TimeSince.op_Implicit(_timeSinceEntered) >= _timeToWait)
			{
				boatAI.SwitchState(boatAI._wanderState);
			}
		}

		public override void Exit(BoatAI boatAI)
		{
		}

		public override string GetStateName()
		{
			return "Wait";
		}
	}

	public class WanderState : BoatState
	{
		private float _wanderAngle;

		private float _targetAngle;

		private TimeSince _timeSinceLastAngleChange;

		private Vector3 _macroTarget;

		private TimeSince _timeSinceNewTarget;

		private float _nextMacroInterval;

		private bool _isInDeepsea;

		private TimeSince _timeSinceEntered;

		private float _timeToWait;

		public override void Enter(BoatAI boatAI)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			_timeSinceEntered = TimeSince.op_Implicit(0f);
			_timeToWait = Random.Range(120f, 320f);
			_wanderAngle = Random.Range(-5f, 5f);
			_targetAngle = _wanderAngle;
			_timeSinceLastAngleChange = TimeSince.op_Implicit(0f);
			_nextMacroInterval = Random.Range(60f, 150f);
			_timeSinceNewTarget = TimeSince.op_Implicit(_nextMacroInterval + 1f);
			if ((Object)(object)PointEntity<DeepSeaManager>.ServerInstance != (Object)null)
			{
				_isInDeepsea = DeepSeaManager.IsInsideDeepSea(((Component)boatAI).transform.position);
			}
		}

		private void CheckLeaveState(BoatAI boatAI)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			if (TimeSince.op_Implicit(_timeSinceEntered) >= _timeToWait)
			{
				boatAI.SwitchState(boatAI._waitState);
			}
			if (boatAI.HasProtectionArea)
			{
				boatAI.SwitchState(boatAI._orbitState);
			}
		}

		public override void Update(Context ctx, BoatAI boatAI, float delta)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			CheckLeaveState(boatAI);
			boatAI.MaintainGroupCohesion(ctx);
			if (TimeSince.op_Implicit(_timeSinceNewTarget) > _nextMacroInterval)
			{
				PickNewMacroTarget(boatAI);
			}
			if (TimeSince.op_Implicit(_timeSinceLastAngleChange) > 0.5f)
			{
				_targetAngle = Random.Range(-5f, 5f);
				_timeSinceLastAngleChange = TimeSince.op_Implicit(0f);
			}
			_wanderAngle = Mathf.Lerp(_wanderAngle, _targetAngle, delta * 0.5f);
			Vector3 val = Quaternion.AngleAxis(_wanderAngle, Vector3.up) * ((Component)boatAI._boat).transform.forward;
			Vector3 worldDirection = Vector3.zero;
			if (_macroTarget != Vector3.zero)
			{
				worldDirection = _macroTarget - ((Component)boatAI).transform.position;
				worldDirection.y = 0f;
				if (worldDirection.sqrMagnitude > 100f)
				{
					worldDirection.Normalize();
					boatAI.AddContextInterest(ctx, worldDirection, 0.3f);
				}
			}
			boatAI.AddContextInterest(ctx, val, 0.15f, 2);
			if (_isInDeepsea && (Object)(object)PointEntity<DeepSeaManager>.ServerInstance != (Object)null)
			{
				Vector3 val2 = DeepSeaManager.DeepSeaBounds.center - ((Component)boatAI).transform.position;
				val2.y = 0f;
				if (val2.magnitude > 3000f)
				{
					Vector3 normalized = val2.normalized;
					Vector3 worldDirection2 = Vector3.Lerp(val, normalized, 0.75f);
					boatAI.AddContextInterest(ctx, worldDirection2, 0.5f, 0);
				}
			}
			if (boatAI.HasProtectionArea)
			{
				Vector3 val3 = boatAI.ProtectionCenter - ((Component)boatAI).transform.position;
				val3.y = 0f;
				if (val3.magnitude > 20f)
				{
					Vector3 normalized2 = val3.normalized;
					Vector3 worldDirection3 = Vector3.Lerp(val, normalized2, 0.75f);
					boatAI.AddContextInterest(ctx, worldDirection3, 0.5f, 0);
				}
			}
			if (_macroTarget != Vector3.zero && Vector3Ex.Distance2D(((Component)boatAI).transform.position, _macroTarget) < 25f)
			{
				PickNewMacroTarget(boatAI);
			}
		}

		private void PickNewMacroTarget(BoatAI boatAI)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			_timeSinceNewTarget = TimeSince.op_Implicit(0f);
			_nextMacroInterval = Random.Range(60f, 150f);
			if (boatAI.HasProtectionArea)
			{
				Vector3 protectionCenter = boatAI.ProtectionCenter;
				_macroTarget = protectionCenter + new Vector3(Random.Range(0f - boatAI.ProtectionRadius, boatAI.ProtectionRadius), 0f, Random.Range(0f - boatAI.ProtectionRadius, boatAI.ProtectionRadius));
			}
			else if (!boatAI.Roaming || !TryFindRoamPoint(((Component)boatAI).transform.position, out _macroTarget))
			{
				if ((Object)(object)PointEntity<DeepSeaManager>.ServerInstance == (Object)null || boatAI.Roaming)
				{
					_macroTarget = ((Component)boatAI).transform.position + ((Component)boatAI._boat).transform.forward * 100f;
					return;
				}
				Bounds deepSeaBounds = DeepSeaManager.DeepSeaBounds;
				Vector3 min = deepSeaBounds.min;
				Vector3 max = deepSeaBounds.max;
				_macroTarget = new Vector3(Random.Range(min.x, max.x), ((Component)boatAI).transform.position.y, Random.Range(min.z, max.z));
			}
		}

		public override void Exit(BoatAI boatAI)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			_macroTarget = Vector3.zero;
		}

		public override string GetStateName()
		{
			return "Wander";
		}
	}

	public class SeekState : BoatState
	{
		public override void Enter(BoatAI boatAI)
		{
		}

		public override void Update(Context ctx, BoatAI boatAI, float delta)
		{
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)boatAI._boat == (Object)null))
			{
				IAITarget activeTarget = boatAI.ActiveTarget;
				if (activeTarget == null)
				{
					boatAI.SwitchState(boatAI._wanderState);
					return;
				}
				if (!activeTarget.Position.HasValue)
				{
					boatAI.ClearCurrentTarget();
					boatAI.SwitchState(boatAI._wanderState);
					return;
				}
				if (activeTarget.IsReached(boatAI))
				{
					boatAI.ClearCurrentTarget();
					boatAI.SwitchState(boatAI._wanderState);
					return;
				}
				Vector3 val = activeTarget.Position.Value - ((Component)boatAI._boat).transform.position;
				Vector3 normalized = val.normalized;
				normalized.y = 0f;
				boatAI.AddContextInterest(ctx, normalized, 1f, 5);
			}
		}

		public override void Exit(BoatAI boatAI)
		{
		}

		public override string GetStateName()
		{
			return "Seek";
		}
	}

	public class DriveByState : BoatState
	{
		private bool _beenClose;

		private const float PASS_DISTANCE = 25f;

		private const float AIM_AHEAD = 15f;

		public override void Enter(BoatAI boatAI)
		{
			_beenClose = false;
		}

		public override void Update(Context ctx, BoatAI boatAI, float delta)
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)boatAI._boat == (Object)null)
			{
				return;
			}
			if (!(boatAI.ActiveTarget is PlayerTarget { Position: var position } playerTarget))
			{
				boatAI.SwitchState(boatAI._wanderState);
			}
			else
			{
				if (!position.HasValue)
				{
					return;
				}
				Vector3 position2 = ((Component)playerTarget.Player).transform.position;
				Vector3 val = position2 - ((Component)boatAI._boat).transform.position;
				Vector3 normalized = val.normalized;
				Vector3 val2 = position2 + normalized * 15f;
				Vector3 val3 = Vector3.Cross(Vector3.up, normalized);
				Vector3 normalized2 = val3.normalized;
				Vector3 val4 = ((Vector3.Dot(((Component)boatAI._boat).transform.forward, normalized2) > 0f) ? normalized2 : (-normalized2)) * 25f;
				Vector3 val5 = val2 + val4;
				Vector3 val6 = val5 - ((Component)boatAI._boat).transform.position;
				Vector3 normalized3 = val6.normalized;
				normalized3.y = 0f;
				boatAI.AddContextInterest(ctx, normalized3, 1f, 2);
				float num = Vector3Ex.Distance2D(((Component)boatAI._boat).transform.position, val5);
				if (_beenClose)
				{
					if (Vector3.Dot(((Component)boatAI._boat).transform.forward, val.normalized) < 0f)
					{
						boatAI.SwitchState(boatAI._seekState);
					}
					else if (num > 20f)
					{
						boatAI.SwitchState(boatAI._seekState);
					}
				}
				else if (num < 10f)
				{
					_beenClose = true;
				}
			}
		}

		public override void Exit(BoatAI boatAI)
		{
		}

		public override string GetStateName()
		{
			return "DriveBy";
		}
	}

	public class RamState : BoatState
	{
		private TimeSince _timeSinceStartedRam;

		private const float RAM_DURATION = 20f;

		public override void Enter(BoatAI boatAI)
		{
		}

		public override void Update(Context ctx, BoatAI boatAI, float delta)
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)boatAI._boat == (Object)null)
			{
				return;
			}
			PlayerTarget playerTarget = boatAI.ActiveTarget as PlayerTarget;
			bool flag = playerTarget != null && playerTarget.IsValid(boatAI) && playerTarget.Position.HasValue && Mathf.Abs(playerTarget.Position.Value.y - ((Component)boatAI._boat).transform.position.y) > 15f;
			if ((playerTarget == null) | flag)
			{
				if (PRINT_DEBUGS)
				{
					Debug.Log((object)"Leaving ram state");
				}
				boatAI.SwitchState(boatAI._seekState);
			}
			else if (playerTarget.Position.HasValue)
			{
				Vector3 val = playerTarget.Position.Value - ((Component)boatAI._boat).transform.position;
				Vector3 normalized = val.normalized;
				normalized.y = 0f;
				boatAI.AddContextInterest(ctx, normalized, 1f, 0);
				if (Vector3Ex.Distance2D(((Component)boatAI._boat).transform.position, playerTarget.Position.Value) < 2f && Vector3.Dot(((Component)boatAI._boat).transform.forward, val) < 0f)
				{
					boatAI.SwitchState(boatAI._seekState);
				}
			}
		}

		public override void Exit(BoatAI boatAI)
		{
		}

		public override string GetStateName()
		{
			return "Ram";
		}
	}

	public class OrbitState : BoatState
	{
		private const float TAU = MathF.PI * 2f;

		private const float CIRCLE_RESOLUTION = 45f;

		private const float ORBIT_RADIUS = 30f;

		private const float ORBIT_STEP = 1f / 45f;

		private float orbitPercent;

		private Vector3 currentOrbitTargetPoint;

		public override void Enter(BoatAI boatAI)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			orbitPercent = 0f;
			currentOrbitTargetPoint = GetRandomPointOnCircle(boatAI.ProtectionCenter, 80f);
		}

		public override void Update(Context ctx, BoatAI boatAI, float delta)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)boatAI._boat == (Object)null)
			{
				return;
			}
			if (!boatAI.HasProtectionArea)
			{
				boatAI.SwitchState(boatAI._wanderState);
			}
			if (Vector3.Distance(((Component)boatAI).transform.position, currentOrbitTargetPoint) < 60f || Check.IsOnTerrainSimple(currentOrbitTargetPoint))
			{
				orbitPercent += 1f / 45f;
				if (orbitPercent > 1f)
				{
					orbitPercent--;
				}
				float radAngle = orbitPercent * (MathF.PI * 2f);
				currentOrbitTargetPoint = GetPointOnCircle(boatAI.ProtectionCenter, 80f, radAngle);
			}
			Vector3 val = currentOrbitTargetPoint - ((Component)boatAI).transform.position;
			Vector3 normalized = val.normalized;
			boatAI.AddContextInterest(ctx, normalized, 0.5f, 2);
		}

		private Vector3 GetRandomPointOnCircle(Vector3 centre, float radius)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			float radAngle = Random.Range(0f, MathF.PI * 2f);
			return GetPointOnCircle(centre, radius, radAngle);
		}

		private Vector3 GetPointOnCircle(Vector3 centre, float radius, float radAngle)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			float num = Mathf.Cos(radAngle) * radius;
			float num2 = Mathf.Sin(radAngle) * radius;
			return centre + new Vector3(num, 0f, num2);
		}

		public override void Exit(BoatAI boatAI)
		{
		}

		public override string GetStateName()
		{
			return "Orbit";
		}
	}

	public enum AILoadMode
	{
		LoadAi,
		KillAi,
		KillBoat
	}

	private class InputProvider : IAiInputProvider
	{
		private BoatAI _boatAI;

		public InputProvider(BoatAI boatAI)
		{
			_boatAI = boatAI;
		}

		public void OnAdd(BaseVehicle vehicle)
		{
			_boatAI.OnAdd(vehicle);
			vehicle.BeenAttacked += _boatAI.BoatAttacked;
			vehicle.Died += _boatAI.BoatDied;
			vehicle.OnDismountAll += _boatAI.KillAllRemainingScientists;
			_boatAI.OnAttached();
		}

		public void OnTick(BaseVehicle vehicle, float delta, ref float steering, ref float gasPedal)
		{
			((ObjectWorkQueue<BoatAIInstruction>)BoatWorkQueue).Add(new BoatAIInstruction
			{
				AI = _boatAI,
				Delta = delta
			});
		}

		public void OnRemove(BaseVehicle vehicle)
		{
			_boatAI.OnRemove(vehicle);
			vehicle.BeenAttacked -= _boatAI.BoatAttacked;
			vehicle.Died -= _boatAI.BoatDied;
			vehicle.OnDismountAll -= _boatAI.KillAllRemainingScientists;
		}

		public void OnTick(BaseVehicle vehicle, float delta)
		{
		}
	}

	public struct BoatAIInstruction : IEquatable<BoatAIInstruction>
	{
		public BoatAI AI;

		public float Delta;

		public bool Equals(BoatAIInstruction other)
		{
			return AI == other.AI;
		}

		public override bool Equals(object obj)
		{
			if (obj is BoatAIInstruction other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			if (!((Object)(object)AI != (Object)null))
			{
				return 0;
			}
			return ((object)AI).GetHashCode();
		}
	}

	public class BoatAIWorkQueue : ObjectWorkQueue<BoatAIInstruction>
	{
		protected override void RunJob(BoatAIInstruction instruction)
		{
			if (((ObjectWorkQueue<BoatAIInstruction>)this).ShouldAdd(instruction))
			{
				instruction.AI.OnTick(instruction.AI.Boat, instruction.Delta, ref instruction.AI._boat.steering, ref instruction.AI._boat.gasPedal);
			}
		}

		protected override bool ShouldAdd(BoatAIInstruction instruction)
		{
			if (base.ShouldAdd(instruction))
			{
				return instruction.AI.IsValid();
			}
			return false;
		}

		protected override bool IsValidToRun(BoatAIInstruction entity)
		{
			return true;
		}
	}

	private float? _closestObstacle;

	private FeelerResult[] feelers = new FeelerResult[16];

	private TimeSince timeSinceAvoidanceUpdate;

	private const int MAX_HITS_PER_TRACE = 8;

	private static readonly List<Vector3> _contextMap = new List<Vector3>();

	private Context _bufferContext;

	private int _lastBestIndex;

	private const float LEAVE_WANDER_TIME_MIN = 120f;

	private const float LEAVE_WANDER_TIME_MAX = 320f;

	private const float SMALL_WANDER_CHANGE_INTERVAL = 0.5f;

	private const float WANDER_ANGLE_CHANGE = 5f;

	private const float MACRO_TARGET_INTERVAL_MIN = 60f;

	private const float MACRO_TARGET_INTERVAL_MAX = 150f;

	private const float MACRO_TARGET_PULL_STRENGTH = 0.3f;

	private const float DEEPSEA_CENTER_PULL_START = 3000f;

	private const float DEEPSEA_CENTER_PULL_STRENGTH = 0.75f;

	private const float PROTECTION_AREA_CENTER_PULL_START = 20f;

	private const float PROTECTION_AREA_PULL_STRENGTH = 0.75f;

	private const float ROAM_MIN_SHORE_DISTANCE = 35f;

	private const float ROAM_MAX_SHORE_DISTANCE = 400f;

	private const float GROUP_COHESION_RADIUS = 30f;

	private const float GROUP_PULL_STRENGTH = 0.25f;

	private const float MAX_CHASE_TIME = 180f;

	private const float MAX_CHASE_DISTANCE = 500f;

	private const float GIVE_UP_IGNORE_TIME = 120f;

	private const float RETARGET_INTERVAL = 1f;

	private const float DRIVER_SWAP_DELAY = 3f;

	private const int CONTEXT_RESOLUTION = 16;

	private BasePlayer _chasedPlayer;

	private Vector3 _chaseStartPosition;

	private readonly Dictionary<ulong, float> _ignoredUntil = new Dictionary<ulong, float>();

	private bool _isLeaving;

	[Header("Boat AI - Scientists")]
	[SerializeField]
	private bool _autoFillWithScientists;

	[SerializeField]
	private GameObjectRef _scientistPrefab;

	private HumanNPC _driverNpc;

	private bool _replacingDriver;

	private TimeSince _timeSinceDriverDied;

	private List<PTBoatTurret> _turrets = new List<PTBoatTurret>();

	private bool _hasSpawnedScientists;

	private bool _hasKilledScientists;

	private BoatState _currentState;

	private IdleState _idleState;

	private WanderState _wanderState;

	private WaitState _waitState;

	private SeekState _seekState;

	private DriveByState _driveByState;

	private RamState _ramState;

	private OrbitState _orbitState;

	public const string DeepSeaRHIBPath = "assets/content/vehicles/boats/rhib/rhib.deepsea.prefab";

	public const string DeepSeaPTBoatPath = "assets/content/vehicles/boats/ptboat/ptboat.deepsea.prefab";

	[Header("Boat AI")]
	[SerializeField]
	private BaseBoat _boat;

	[Header("Boat AI - General")]
	[SerializeField]
	private bool _autoInit;

	[SerializeField]
	private bool _autoPursue;

	[SerializeField]
	private float _thinkTime = 5f;

	[SerializeField]
	private float _searchRange = 50f;

	[SerializeField]
	[Header("Boat AI - Collision Avoidance")]
	private float _awarenessAngle;

	[SerializeField]
	private float _awarenessDistance;

	[SerializeField]
	[Header("Boat AI - Debug")]
	private Transform _debugMoveTo;

	[ServerVar(Help = "(Generated) When enabled, draws DDraw visualisations of boat AI steering, avoidance, and pathfinding state")]
	public static bool DRAW_DEBUGS = false;

	[ServerVar(Help = "(Generated) When enabled, logs verbose boat AI decision-making output to the server console each AI tick")]
	public static bool PRINT_DEBUGS = false;

	[ServerVar(Help = "Distance players need to be to start syncing mounted seats")]
	public static float enable_mount_sync_distance = 750f;

	[ServerVar(Help = "How often to update the avoidance cache. Lower number means a more accurate cache at the expensive of performance.", ShowInAdminUI = true)]
	public static float avoidance_update_interval = 0.8f;

	[ServerVar(Help = "How long per frame to spend on boat ai", Saved = true, ShowInAdminUI = true)]
	public static float boat_ai_frame_budget_ms = 0.3f;

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "(Generated) Maximum speed as a fraction of the boat's top speed that AI-controlled boats will use; default 0.9; saved and shown in admin UI")]
	public static float max_speed_percentage = 0.9f;

	[ServerVar(Help = "Minutes a roaming boat has to sail off the map before it is removed where it is")]
	public static float roaming_egress_minutes = 10f;

	private float _helm;

	private float _throttle;

	private float _stalledTime;

	private float _recoverTimeLeft;

	private float _recoverDirection;

	private Vector3 _recoverStart;

	private IAITarget _progressTarget;

	private float _closestToTarget;

	private float _timeSinceProgress;

	private float _detourTimeLeft;

	private int _detourSide;

	[ServerVar(Help = "(Generated) When enabled, boat AI entities can enter a sleep state when no players are nearby; reduce CPU usage for idle boats")]
	public static bool allow_sleeping = false;

	[ServerVar(Help = "(Generated) Number of seconds a boat AI will wait without player interaction before entering sleep mode; default 30s")]
	public static float seconds_until_sleep = 30f;

	private const float AI_SPAWN_DELAY = 5f;

	private InputProvider _provider;

	private TimeSince _timeSinceThought;

	private TimeSince _timeSinceRetarget;

	private TimeSince _timeSinceSleepy;

	private TimeSince _timeSinceSpawned;

	private bool _isSleeping;

	private bool _setupRan;

	public static BoatAIWorkQueue BoatWorkQueue = new BoatAIWorkQueue();

	private int __sync_GroupId;

	private NetworkableId __sync_BoatID;

	private int __sync_LoadModeSync;

	public bool HasProtectionArea => ProtectionRadius > 0f;

	public float SearchRange => _searchRange;

	public MotorRowboat Boat { get; private set; }

	public IAITarget ActiveTarget { get; set; }

	public float PursuitTargetAcquireTime { get; set; }

	public bool InGroup => GroupId != -1;

	[Sync(Autosave = true)]
	public int GroupId
	{
		[CompilerGenerated]
		get
		{
			return __sync_GroupId;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_GroupId, value))
			{
				__sync_GroupId = value;
				byte nameID = __GetWeaverID("GroupId");
				QueueSyncVar(nameID);
			}
		}
	}

	public Vector3 ProtectionCenter
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public float ProtectionRadius { get; set; }

	public bool Roaming { get; set; }

	[Sync(Autosave = true)]
	private NetworkableId BoatID
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return __sync_BoatID;
		}
		[CompilerGenerated]
		set
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (!IsSyncVarEqual<NetworkableId>(__sync_BoatID, value))
			{
				__sync_BoatID = value;
				byte nameID = __GetWeaverID("BoatID");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	private int LoadModeSync
	{
		[CompilerGenerated]
		get
		{
			return __sync_LoadModeSync;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_LoadModeSync, value))
			{
				__sync_LoadModeSync = value;
				byte nameID = __GetWeaverID("LoadModeSync");
				QueueSyncVar(nameID);
			}
		}
	}

	public AILoadMode LoadMode
	{
		get
		{
			return (AILoadMode)LoadModeSync;
		}
		set
		{
			LoadModeSync = (int)value;
		}
	}

	private void FillRaycastCommands(NativeArray<RaycastCommand> allRaycasts)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)_boat).transform.position;
		QueryParameters val = new QueryParameters(1218781441, false, (QueryTriggerInteraction)0, false);
		for (int i = 0; i < _contextMap.Count; i++)
		{
			Vector3 val2 = _contextMap[i];
			allRaycasts[i] = new RaycastCommand(position, val2, val, _awarenessDistance);
		}
	}

	private void ProcessHits(NativeArray<RaycastHit> hits, in FeelerResult[] results, int rayCount)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		Assert.That(results.Length == rayCount);
		_closestObstacle = null;
		for (int i = 0; i < rayCount; i++)
		{
			int num = i * 8;
			Nullable hit = null;
			RaycastHit value;
			for (int j = 0; j < 8; j++)
			{
				RaycastHit val = hits[num + j];
				if ((Object)(object)val.collider == (Object)null || (ColliderEx.IsOnLayer(val.collider, (Layer)18) && !((Component)val.collider).CompareTag("BoatAIAvoid")))
				{
					continue;
				}
				BaseEntity entity = RaycastHitEx.GetEntity(val);
				if ((Object)(object)entity == (Object)(object)this || (Object)(object)((Component)val.collider).transform.root == (Object)(object)((Component)_boat).transform || (entity.EntityValid() && entity.EntityIsClient()))
				{
					continue;
				}
				if (hit.HasValue)
				{
					float distance = val.distance;
					value = hit.Value;
					if (!(distance < value.distance))
					{
						continue;
					}
				}
				hit = val;
			}
			results[i] = new FeelerResult
			{
				direction = _contextMap[i],
				hit = hit
			};
			if (!hit.HasValue)
			{
				continue;
			}
			if (_closestObstacle.HasValue)
			{
				value = hit.Value;
				if (!(value.distance < _closestObstacle.Value))
				{
					continue;
				}
			}
			value = hit.Value;
			_closestObstacle = value.distance;
		}
	}

	private void AvoidObstacles(Context ctx)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BoatAI.AvoidObstacles"))
		{
			if (TimeSince.op_Implicit(timeSinceAvoidanceUpdate) >= avoidance_update_interval + Random.Range(-0.1f, 0.1f))
			{
				timeSinceAvoidanceUpdate = TimeSince.op_Implicit(0f);
				_closestObstacle = null;
				int count = _contextMap.Count;
				if (feelers == null || feelers.Length != count)
				{
					feelers = new FeelerResult[count];
				}
				NativeArray<RaycastCommand> val = new NativeArray<RaycastCommand>(count, (Allocator)3, (NativeArrayOptions)1);
				try
				{
					FillRaycastCommands(val);
					NativeArray<RaycastHit> hits = new NativeArray<RaycastHit>(count * 8, (Allocator)3, (NativeArrayOptions)1);
					try
					{
						GamePhysics.TraceRaysUnordered(val, hits, 8, traceWater: false);
						ProcessHits(hits, in feelers, count);
					}
					finally
					{
						((IDisposable)hits/*cast due to constrained. prefix*/).Dispose();
					}
				}
				finally
				{
					((IDisposable)val/*cast due to constrained. prefix*/).Dispose();
				}
			}
			for (int i = 0; i < feelers.Length; i++)
			{
				FeelerResult feelerResult = feelers[i];
				if (feelerResult.hit.HasValue)
				{
					RaycastHit value = feelerResult.hit.Value;
					float num = Mathf.Clamp01(1f - value.distance / _awarenessDistance);
					ctx.DangerMap[i] += num * num;
				}
			}
		}
	}

	private float GetClearance(Vector3 worldDirection)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Repeat(Vector3.SignedAngle(Vector3.forward, Vector3Ex.WithY(worldDirection, 0f), Vector3.up) / 360f * 16f, 16f);
		int num2 = (int)num % 16;
		return Mathf.Lerp(GetFeelerClearance(num2), GetFeelerClearance((num2 + 1) % 16), num - (float)num2);
	}

	private float GetFeelerClearance(int index)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Nullable hit = feelers[index].hit;
		if (!hit.HasValue)
		{
			return _awarenessDistance;
		}
		RaycastHit value = hit.Value;
		return value.distance;
	}

	private void DetourRoundBlockages(Context ctx, float delta)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		Nullable val = ActiveTarget?.Position;
		if (!val.HasValue || ActiveTarget != _progressTarget)
		{
			_progressTarget = ActiveTarget;
			_closestToTarget = float.MaxValue;
			_detourTimeLeft = 0f;
			return;
		}
		Vector3 val2 = val.Value - ((Component)_boat).transform.position;
		val2.y = 0f;
		float magnitude = val2.magnitude;
		float num = Mathf.Min(magnitude, _awarenessDistance);
		bool flag = GetClearance(val2) < num;
		if (magnitude < _closestToTarget - 5f)
		{
			_closestToTarget = magnitude;
			_timeSinceProgress = 0f;
			if (_detourTimeLeft <= 0f)
			{
				_detourSide = 0;
			}
		}
		else
		{
			_timeSinceProgress += delta;
		}
		if (((_detourTimeLeft <= 0f) & flag) && _timeSinceProgress > 8f)
		{
			if (_detourSide == 0)
			{
				_detourSide = ((Vector3.SignedAngle(val2, ((Component)_boat).transform.forward, Vector3.up) >= 0f) ? 1 : (-1));
			}
			_detourTimeLeft = 15f;
			_closestToTarget = magnitude;
			_timeSinceProgress = 0f;
		}
		if (!(_detourTimeLeft > 0f))
		{
			return;
		}
		_detourTimeLeft = (flag ? (_detourTimeLeft - delta) : 0f);
		Vector3 worldDirection = -val2;
		float num2 = 0f;
		for (int i = 1; i <= 8; i++)
		{
			Vector3 val3 = Quaternion.AngleAxis((float)(_detourSide * i) * 360f / 16f, Vector3.up) * val2;
			float clearance = GetClearance(val3);
			if (clearance > num2)
			{
				num2 = clearance;
				worldDirection = val3;
			}
			if (clearance >= num)
			{
				break;
			}
		}
		AddContextInterest(ctx, worldDirection, 1f);
	}

	private void AvoidShore(Context ctx)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)_boat).transform.position;
		float num = Mathf.InverseLerp(35f, 17.5f, TerrainTexturing.Instance.GetCoarseDistanceToShore(position));
		if (!(num <= 0f))
		{
			Vector3 item = TerrainTexturing.Instance.GetCoarseVectorToShore(position).shoreDir;
			AddContextDanger(ctx, item, num * 2f, 2);
			AddContextInterest(ctx, -item, num);
		}
	}

	public void MoveTo(Vector3 pos, float stopRadius = 80f, Action onArrived = null)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		SetMoveCommand(new PointTarget(pos, stopRadius));
	}

	public void MoveTo(Transform t, float stopRadius = 3f)
	{
		SetMoveCommand(new TransformTarget(t, stopRadius));
	}

	public void LeaveAfter(float seconds)
	{
		InvokeRepeating(Leave, seconds, 5f);
	}

	private void Leave()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		_isLeaving = true;
		Vector3 position = ((Component)_boat).transform.position;
		if (TerrainMeta.OutOfBounds(position))
		{
			RemoveBoat();
		}
		else if (ActiveTarget == null)
		{
			Vector3 val = Vector3Ex.WithY(position - TerrainMeta.Center, 0f);
			Vector3 normalized = val.normalized;
			MoveTo(position + normalized * TerrainMeta.Size.x);
		}
	}

	private void RemoveBoat()
	{
		if (_boat is RHIB rHIB)
		{
			rHIB.AdminKillNoLoot();
		}
	}

	private void SetMoveCommand(IAITarget t)
	{
		ActiveTarget = t;
		EnableScientistBrains();
		RefreshSleeping();
		if (t is PlayerTarget playerTarget)
		{
			if (PRINT_DEBUGS)
			{
				Debug.Log((object)("Found target - " + playerTarget.Player.displayName));
			}
			GiveMountedWeaponsExtraTarget(playerTarget.Player);
			if (playerTarget.StayClose && Random.value >= 0.8f)
			{
				SwitchState(_ramState);
				return;
			}
		}
		SwitchState(_seekState);
	}

	private bool HasValidPursuit()
	{
		if (ActiveTarget != null)
		{
			return ActiveTarget.IsValid(this);
		}
		return false;
	}

	public void SetProtectionArea(Vector3 center, float radius)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		ProtectionCenter = center;
		ProtectionRadius = radius;
	}

	private void ClearCurrentTarget()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (PRINT_DEBUGS)
		{
			Debug.Log((object)"attempt to release target");
		}
		if (ActiveTarget != null && ActiveTarget is PlayerTarget playerTarget)
		{
			BoatAICoordination.ReleaseClaim(this, playerTarget.Player);
		}
		_timeSinceSleepy = TimeSince.op_Implicit(0f);
		ActiveTarget = null;
		DisableScientistBrains();
		GiveMountedWeaponsExtraTarget(null);
	}

	private void ResetContext(Context ctx)
	{
		for (int i = 0; i < ctx.InterestMap.Length; i++)
		{
			ctx.InterestMap[i] = 0f;
			ctx.DangerMap[i] = 0f;
		}
	}

	private void InitialiseContextMaps()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (_contextMap.Count != 16)
		{
			_contextMap.Clear();
			float num = 22.5f;
			for (int i = 0; i < 16; i++)
			{
				Vector3 item = Quaternion.AngleAxis((float)i * num, Vector3.up) * Vector3.forward;
				_contextMap.Add(item);
			}
		}
		_bufferContext = new Context(16);
	}

	private void BlurContext(Context ctx, int iterations = 1)
	{
		int num = ctx.InterestMap.Length;
		BufferList<float> val = Pool.Get<BufferList<float>>();
		BufferList<float> val2 = Pool.Get<BufferList<float>>();
		val.Clear();
		val2.Clear();
		for (int i = 0; i < num; i++)
		{
			val.Add(0f);
			val2.Add(0f);
		}
		for (int j = 0; j < iterations; j++)
		{
			for (int k = 0; k < num; k++)
			{
				int num2 = (k - 1 + num) % num;
				int num3 = (k + 1) % num;
				val[k] = (ctx.InterestMap[num2] + ctx.InterestMap[k] + ctx.InterestMap[num3]) / 3f;
				val2[k] = (ctx.DangerMap[num2] + ctx.DangerMap[k] + ctx.DangerMap[num3]) / 3f;
			}
			for (int l = 0; l < num; l++)
			{
				ctx.InterestMap[l] = val[l];
				ctx.DangerMap[l] = val2[l];
			}
		}
		Pool.FreeUnmanaged<float>(ref val);
		Pool.FreeUnmanaged<float>(ref val2);
	}

	private Vector3 GetContextDirection(int index)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _contextMap[index];
	}

	private Vector3 GetBestContextDirection()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BoatAI.Context.GetBestContextDirection"))
		{
			if (_contextMap.Count == 0)
			{
				return ((Component)this).transform.forward;
			}
			BlurContext(_bufferContext);
			int count = _contextMap.Count;
			float num = 0f;
			for (int i = 0; i < count; i++)
			{
				num = Mathf.Max(num, _bufferContext.InterestMap[i]);
			}
			if (num <= 0.01f)
			{
				return Vector3.zero;
			}
			int num2 = _lastBestIndex;
			float num3 = GetContextScore(num2) + 0.05f;
			for (int j = 0; j < count; j++)
			{
				float contextScore = GetContextScore(j);
				if (contextScore > num3)
				{
					num3 = contextScore;
					num2 = j;
				}
			}
			while (GetContextScore((num2 + 1) % count) > GetContextScore(num2))
			{
				num2 = (num2 + 1) % count;
			}
			while (GetContextScore((num2 + count - 1) % count) > GetContextScore(num2))
			{
				num2 = (num2 + count - 1) % count;
			}
			_lastBestIndex = num2;
			float contextScore2 = GetContextScore((num2 + count - 1) % count);
			float contextScore3 = GetContextScore(num2);
			float contextScore4 = GetContextScore((num2 + 1) % count);
			float num4 = contextScore2 - 2f * contextScore3 + contextScore4;
			float num5 = ((num4 < 0f) ? Mathf.Clamp(0.5f * (contextScore2 - contextScore4) / num4, -0.5f, 0.5f) : 0f);
			return Quaternion.AngleAxis(((float)num2 + num5) * 360f / (float)count, Vector3.up) * Vector3.forward;
		}
	}

	private float GetContextScore(int index)
	{
		return _bufferContext.InterestMap[index] - _bufferContext.DangerMap[index];
	}

	private void AddContextInterest(Context context, Vector3 worldDirection, float strength, int spread = 1)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)(spread + 1) * 45f;
		worldDirection.y = 0f;
		for (int i = 0; i < context.InterestMap.Length; i++)
		{
			float num2 = 1f - Vector3.Angle(_contextMap[i], worldDirection) / num;
			if (num2 > 0f)
			{
				context.InterestMap[i] += strength * num2;
			}
		}
	}

	private void AddContextDanger(Context context, Vector3 worldDirection, float strength, int spread = 1)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)(spread + 1) * 45f;
		worldDirection.y = 0f;
		for (int i = 0; i < context.DangerMap.Length; i++)
		{
			float num2 = 1f - Vector3.Angle(_contextMap[i], worldDirection) / num;
			if (num2 > 0f)
			{
				context.DangerMap[i] += strength * num2;
			}
		}
	}

	private IEnumerator SpawnAllScientists()
	{
		yield return (object)new WaitForSeconds(1f);
		int num = 0;
		for (int i = 0; i < _boat.mountPoints.Count; i++)
		{
			BaseMountable mountable = _boat.mountPoints[i].mountable;
			if (mountable is RHIBDriver || mountable is PTBoatTurret)
			{
				SpawnScientist(mountable);
				continue;
			}
			if (num <= 0)
			{
				SpawnScientist(mountable);
			}
			else if (Random.value <= 0.5f)
			{
				SpawnScientist(mountable);
			}
			num++;
		}
		yield return (object)new WaitForEndOfFrame();
		CacheImportantScientists();
		_hasSpawnedScientists = true;
		DisableScientistBrains();
	}

	public void GiveMountedWeaponsExtraTarget(BasePlayer ply)
	{
		foreach (PTBoatTurret turret in _turrets)
		{
			turret.SetAiExtraTarget(ply);
		}
	}

	public bool IsScientistDriverDead()
	{
		if (!_hasSpawnedScientists)
		{
			return false;
		}
		if (!((Object)(object)_driverNpc == (Object)null))
		{
			if ((Object)(object)_driverNpc != (Object)null)
			{
				return !_driverNpc.IsAlive();
			}
			return false;
		}
		return true;
	}

	private bool TryReplaceDriver()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		BaseMountable baseMountable = null;
		HumanNPC humanNPC = null;
		for (int i = 0; i < _boat.mountPoints.Count; i++)
		{
			BaseMountable mountable = _boat.mountPoints[i].mountable;
			if (mountable is RHIBDriver)
			{
				baseMountable = mountable;
			}
			else if ((Object)(object)mountable != (Object)null && mountable.GetMounted() is HumanNPC humanNPC2 && humanNPC2.IsAlive() && ((Object)(object)humanNPC == (Object)null || humanNPC.GetMounted() is PTBoatTurret))
			{
				humanNPC = humanNPC2;
			}
		}
		if ((Object)(object)baseMountable == (Object)null || (Object)(object)humanNPC == (Object)null)
		{
			return false;
		}
		if (!_replacingDriver)
		{
			_replacingDriver = true;
			_timeSinceDriverDied = TimeSince.op_Implicit(0f);
		}
		if (TimeSince.op_Implicit(_timeSinceDriverDied) < 3f || baseMountable.AnyMounted())
		{
			return true;
		}
		humanNPC.GetMounted().DismountPlayer(humanNPC, lite: true);
		baseMountable.MountPlayer(humanNPC);
		humanNPC.inventory.containerBelt.Clear();
		humanNPC.inventory.containerMain.Clear();
		_driverNpc = humanNPC;
		_replacingDriver = false;
		return true;
	}

	private void CacheImportantScientists()
	{
		foreach (BaseVehicle.MountPointInfo allMountPoint in _boat.allMountPoints)
		{
			BasePlayer mounted = allMountPoint.mountable.GetMounted();
			if (allMountPoint.mountable is RHIBDriver)
			{
				mounted.inventory.containerBelt.Clear();
				mounted.inventory.containerMain.Clear();
				_driverNpc = mounted as HumanNPC;
			}
			if (allMountPoint.mountable is PTBoatTurret item)
			{
				mounted.inventory.containerBelt.Clear();
				mounted.inventory.containerMain.Clear();
				_turrets.Add(item);
			}
		}
	}

	private void KillAllRemainingScientists()
	{
		KillAllRemainingScientists(false);
	}

	private void KillAllRemainingScientists(bool skipLoot = false)
	{
		if (_hasSpawnedScientists && !_hasKilledScientists)
		{
			foreach (BaseVehicle.MountPointInfo mountPoint in _boat.mountPoints)
			{
				if (mountPoint.mountable.GetMounted() is HumanNPC { IsDestroyed: false } humanNPC)
				{
					if (skipLoot)
					{
						humanNPC.Kill();
					}
					else
					{
						humanNPC.Die();
					}
				}
			}
		}
		_hasKilledScientists = true;
	}

	private void SpawnScientist(BaseMountable mountable)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (BaseNetworkableEx.Is<HumanNPC>((Object)(object)_scientistPrefab.GetEntity(), out HumanNPC _))
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(_scientistPrefab.resourcePath, _boat.mountAnchor.position, Quaternion.identity);
			baseEntity.Spawn();
			HumanNPC humanNPC = baseEntity as HumanNPC;
			mountable.AttemptMount(humanNPC, doMountChecks: false);
			if ((Object)(object)humanNPC.Brain != (Object)null && humanNPC.Brain.Senses != null)
			{
				humanNPC.Brain.Senses.ignoreTutorialPlayers = true;
				humanNPC.Brain.Senses.ignorePlayer = IsIgnoredByCrew;
			}
		}
	}

	private void ShareSighting(BasePlayer ply)
	{
		for (int i = 0; i < _boat.mountPoints.Count; i++)
		{
			BaseMountable mountable = _boat.mountPoints[i].mountable;
			if ((Object)(object)mountable != (Object)null && mountable.GetMounted() is HumanNPC humanNPC && (Object)(object)humanNPC.Brain != (Object)null && humanNPC.Brain.Senses != null)
			{
				humanNPC.Brain.Senses.Memory.SetKnown(ply, humanNPC, humanNPC.Brain.Senses);
			}
		}
	}

	private void EnableScientistBrains()
	{
		for (int i = 0; i < _boat.mountPoints.Count; i++)
		{
			BaseMountable mountable = _boat.mountPoints[i].mountable;
			if ((Object)(object)mountable == (Object)null)
			{
				Debug.LogError((object)"Mountable was null, skipping enabling boat scientist brain");
				continue;
			}
			BasePlayer mounted = mountable.GetMounted();
			if ((Object)(object)mounted != (Object)null && mounted is HumanNPC humanNPC && (Object)(object)humanNPC != (Object)null)
			{
				humanNPC.Brain.SetThinkMode(AIThinkMode.Interval);
			}
		}
	}

	private bool IsCrewOutOfAngle(BasePlayer target)
	{
		bool result = false;
		for (int i = 0; i < _boat.mountPoints.Count; i++)
		{
			BaseMountable mountable = _boat.mountPoints[i].mountable;
			if ((Object)(object)mountable == (Object)null || !(mountable.GetMounted() is HumanNPC humanNPC))
			{
				continue;
			}
			if (mountable is PTBoatTurret pTBoatTurret)
			{
				result = true;
				if (pTBoatTurret.CanAiShootAt(target))
				{
					return false;
				}
			}
			else if ((Object)(object)humanNPC.GetAttackEntity() != (Object)null && (Object)(object)humanNPC.Brain != (Object)null)
			{
				result = true;
				if (humanNPC.Brain.Senses.Memory.IsLOS(target))
				{
					return false;
				}
			}
		}
		return result;
	}

	private void DisableScientistBrains()
	{
		for (int i = 0; i < _boat.mountPoints.Count; i++)
		{
			BasePlayer mounted = _boat.mountPoints[i].mountable.GetMounted();
			if ((Object)(object)mounted != (Object)null && mounted is HumanNPC humanNPC && (Object)(object)humanNPC != (Object)null)
			{
				humanNPC.Brain.SetThinkMode(AIThinkMode.None);
			}
		}
	}

	private void SwitchState(BoatState newState)
	{
		_currentState?.Exit(this);
		if (newState != null)
		{
			_currentState = newState;
			_currentState.Enter(this);
		}
	}

	private void ExitState()
	{
		if (_currentState != null)
		{
			_currentState.Exit(this);
			_currentState = null;
		}
	}

	private bool IsInState(BoatState state)
	{
		return _currentState == state;
	}

	private void SetupStateCache()
	{
		if (_idleState == null)
		{
			_idleState = new IdleState();
		}
		if (_wanderState == null)
		{
			_wanderState = new WanderState();
		}
		if (_waitState == null)
		{
			_waitState = new WaitState();
		}
		if (_seekState == null)
		{
			_seekState = new SeekState();
		}
		if (_driveByState == null)
		{
			_driveByState = new DriveByState();
		}
		if (_ramState == null)
		{
			_ramState = new RamState();
		}
		if (_orbitState == null)
		{
			_orbitState = new OrbitState();
		}
	}

	private float GetHelm(float headingError)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		float num = _boat.rigidBody.angularVelocity.y * 57.29578f;
		return 0f - Mathf.Clamp((headingError - num * 0.7f) / 35f, -1f, 1f);
	}

	private void MaintainGroupCohesion(Context ctx)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if (!InGroup)
		{
			return;
		}
		ListHashSet<BoatAI> groupMembers = BoatAICoordination.GetGroupMembers(GroupId);
		if (groupMembers == null || groupMembers.Count <= 1)
		{
			return;
		}
		Vector3 val = Vector3.zero;
		int num = 0;
		Enumerator<BoatAI> enumerator = groupMembers.Values.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BoatAI current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && !((Object)(object)current == (Object)(object)this))
				{
					val += ((Component)current).transform.position;
					num++;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		if (num != 0)
		{
			val /= (float)num;
			if (Vector3Ex.Distance2D(((Component)this).transform.position, val) > 30f)
			{
				Vector3 val2 = val - ((Component)this).transform.position;
				Vector3 normalized = val2.normalized;
				normalized.y = 0f;
				AddContextInterest(ctx, normalized, 0.25f);
			}
		}
	}

	private void StartEngine(MotorRowboat boat)
	{
		if (!boat.EngineOn())
		{
			boat.EngineToggle(wantsOn: true);
		}
	}

	private void StopEngine(MotorRowboat boat)
	{
		if (!((Object)(object)boat == (Object)null) && boat.EngineOn())
		{
			boat.EngineToggle(wantsOn: false);
		}
	}

	public bool IsPlayerTargetValid(BasePlayer ply)
	{
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ply == (Object)null)
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning((object)"[BoatAI] Invalid target: player was null");
			}
			return false;
		}
		if (ply.isClient)
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning((object)$"[BoatAI] Invalid target: {ply} is client-side");
			}
			return false;
		}
		if (SimpleAIMemory.PlayerIgnoreList.Contains(ply))
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning((object)("[BoatAI] Invalid target: " + ply.displayName + " is in ignore list"));
			}
			return false;
		}
		if (_ignoredUntil.TryGetValue(ply.userID.Get(), out var value) && Time.time < value)
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning((object)("[BoatAI] Invalid target: " + ply.displayName + " was given up on recently"));
			}
			return false;
		}
		if (!ply.IsValidAttackTarget())
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning((object)("[BoatAI] Invalid target: " + ply.displayName + " not valid attack target"));
			}
			return false;
		}
		object obj = Interface.CallHook("IOnNpcTarget", this, ply);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (Check.IsOnTerrainSimple(((Component)ply).transform.position))
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning((object)("[BoatAI] Invalid target: " + ply.displayName + " standing on terrain (excluded)"));
			}
			return false;
		}
		return true;
	}

	public bool HasLineOfSightToPlayer(BasePlayer ply)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		return GamePhysics.LineOfSight(_boat.CenterPoint() + Vector3.up * _boat.bounds.extents.y, ply.eyes.position, 161546496, _boat);
	}

	public bool IsPlayerInRange(BasePlayer ply, float range)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.SqrMagnitude(((Component)ply).transform.position - ((Component)_boat).transform.position) <= range * range;
	}

	public bool IsSameAsActiveTarget(BasePlayer ply)
	{
		if (!(ActiveTarget is PlayerTarget playerTarget))
		{
			return false;
		}
		return (ulong)playerTarget.Player.userID == (ulong)ply.userID;
	}

	private static bool IsThreat(BasePlayer ply)
	{
		return ply.GetThreatLevel() > 0.5f;
	}

	private bool IsIgnoredByCrew(BasePlayer ply)
	{
		if (Roaming && !IsThreat(ply))
		{
			return !IsSameAsActiveTarget(ply);
		}
		return false;
	}

	public static void SpawnBoatGroup(Vector2 pos, Quaternion rot, HashSet<RHIB> ActiveRHIBS = null, bool registerWithDeepSea = false, bool spawnsPT = true)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		if (!AI.scientist_spawners_enabled)
		{
			return;
		}
		PooledHashSet<RHIB> val = Pool.Get<PooledHashSet<RHIB>>();
		try
		{
			Vector3 val2 = new Vector3(pos.x, 0f, pos.y);
			Vector3 val3 = rot * Vector3.forward;
			Vector3 val4 = rot * Vector3.right;
			Vector3[] array = new Vector3[3]
			{
				val3 * 10f,
				-val3 * 5f - val4 * 7.5f,
				-val3 * 5f + val4 * 7.5f
			};
			int nextGroupId = BoatAICoordination.GetNextGroupId();
			bool flag = false;
			Vector3[] array2 = array;
			foreach (Vector3 val5 in array2)
			{
				Vector3 val6 = val2;
				val6.y = 0f;
				RHIB rHIB = SpawnEntityAt((spawnsPT && !flag) ? "assets/content/vehicles/boats/ptboat/ptboat.deepsea.prefab" : "assets/content/vehicles/boats/rhib/rhib.deepsea.prefab", val6 + val5, rot) as RHIB;
				if ((Object)(object)rHIB != (Object)null)
				{
					ActiveRHIBS?.Add(rHIB);
				}
				if ((Object)(object)rHIB != (Object)null)
				{
					((HashSet<RHIB>)(object)val).Add(rHIB);
				}
				BoatAICoordination.AddToGroup(((Component)rHIB).GetComponentInChildren<BoatAI>(), nextGroupId);
				LootFill component = ((Component)rHIB).GetComponent<LootFill>();
				if ((Object)(object)component != (Object)null)
				{
					component.FillLoot();
				}
				flag = true;
			}
			if (registerWithDeepSea && (Object)(object)PointEntity<DeepSeaManager>.ServerInstance != (Object)null)
			{
				PointEntity<DeepSeaManager>.ServerInstance.RegisterRHIBs((HashSet<RHIB>)(object)val);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static BaseEntity SpawnEntityAt(string prefabPath, Vector3 position, Quaternion rotation)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = GameManager.server.CreateEntity(prefabPath, position, rotation);
		if ((Object)(object)baseEntity == (Object)null)
		{
			return null;
		}
		baseEntity.Spawn();
		baseEntity.UpdateNetworkGroup();
		return baseEntity;
	}

	public static bool FindBoatSpawnPositionInRadius(Vector2 centre, float radius, out Vector2 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		position = default;
		int layerMask = 1218652417;
		int i = 0;
		float num = Random.Range(0f, MathF.PI * 2f);
		for (; i < 10; i++)
		{
			Vector2 val = centre + new Vector2(Mathf.Cos(num), Mathf.Sin(num)) * radius;
			if (!GamePhysics.CheckSphere(new Vector3(val.x, 0f, val.y), 4f, layerMask, (QueryTriggerInteraction)0))
			{
				position = val;
				return true;
			}
			num = Random.Range(0f, MathF.PI * 2f);
		}
		return false;
	}

	public static bool TryGetMapEdgeSpawn(out Vector3 position, out Vector3 destination)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 50; i++)
		{
			float num = Random.value * MathF.PI * 2f;
			Vector3 val = new Vector3(Mathf.Cos(num), 0f, Mathf.Sin(num));
			val /= Mathf.Max(Mathf.Abs(val.x), Mathf.Abs(val.z));
			position = Vector3Ex.WithY(TerrainMeta.Center, 0f) + val * (TerrainMeta.Size.x * 0.5f - 50f);
			if (!(TerrainTexturing.Instance.GetCoarseDistanceToShore(position) >= 35f) || BaseNetworkable.HasCloseConnections(position, 300f))
			{
				continue;
			}
			Vector3 val2 = -val.normalized;
			for (float num2 = 0f; num2 < TerrainMeta.Size.x * 0.5f; num2 += 25f)
			{
				destination = position + val2 * num2;
				if (IsRoamable(destination))
				{
					return true;
				}
			}
		}
		position = default;
		destination = default;
		return false;
	}

	public static bool TryFindRoamPoint(Vector3 from, out Vector3 position)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 20; i++)
		{
			Vector2 insideUnitCircle = Random.insideUnitCircle;
			Vector2 normalized = insideUnitCircle.normalized;
			position = from + new Vector3(normalized.x, 0f, normalized.y) * Random.Range(150f, 400f);
			position.y = 0f;
			if (IsRoamable(position) && IsRoamPathClear(from, position))
			{
				return true;
			}
		}
		position = default;
		return false;
	}

	private static bool IsRoamable(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (TerrainMeta.OutOfBounds(position))
		{
			return false;
		}
		float coarseDistanceToShore = TerrainTexturing.Instance.GetCoarseDistanceToShore(position);
		if (coarseDistanceToShore >= 35f)
		{
			return coarseDistanceToShore <= 400f;
		}
		return false;
	}

	private static bool IsRoamPathClear(Vector3 from, Vector3 to)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.CeilToInt(Vector3Ex.Distance2D(from, to) / 25f);
		for (int i = 1; i < num; i++)
		{
			Vector3 pos = Vector3.Lerp(from, to, (float)i / (float)num);
			if (TerrainTexturing.Instance.GetCoarseDistanceToShore(pos) < 17.5f)
			{
				return false;
			}
		}
		return true;
	}

	private void SetupAI(BaseBoat boat)
	{
		if (!((Object)(object)boat == (Object)null) && !_setupRan)
		{
			Invoke(() =>
			{
				SetupInternal(boat);
			}, 1f);
		}
	}

	private void SetupInternal(BaseBoat boat)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		_setupRan = true;
		_boat = boat;
		Boat = boat as MotorRowboat;
		_timeSinceSpawned = TimeSince.op_Implicit(0f);
		_timeSinceSleepy = TimeSince.op_Implicit(seconds_until_sleep);
		BoatAICoordination.Register(this);
		InitialiseContextMaps();
		SetupStateCache();
		_provider = new InputProvider(this);
		_boat.AddAIDriver(_provider);
		BoatID = _boat.net.ID;
		if (Object.op_Implicit((Object)(object)_debugMoveTo))
		{
			_autoPursue = false;
			SetMoveCommand(new TransformTarget(_debugMoveTo));
		}
		Invoke(NightCheck, 1f);
		InvokeRandomized(NightCheck, 0f, 30f, 0.05f);
		InvokeRandomized(TargetCheck, 0f, 5f, 0.1f);
		SwitchState((ActiveTarget != null) ? ((BoatState)_seekState) : ((BoatState)_idleState));
		RefreshSleeping();
	}

	private void TargetCheck()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BoatAi.TargetCheck"))
		{
			bool flag = Time.time - PursuitTargetAcquireTime > 180f;
			bool flag2 = Vector3Ex.Distance2D(((Component)_boat).transform.position, _chaseStartPosition) > 500f;
			if (ActiveTarget is PlayerTarget playerTarget && (flag | flag2))
			{
				_ignoredUntil[playerTarget.Player.userID.Get()] = Time.time + 120f;
				_chasedPlayer = null;
			}
			if (ActiveTarget != null && !ActiveTarget.IsValid(this))
			{
				DropTarget();
			}
		}
	}

	private void DropTarget()
	{
		BasePlayer ignore = ((ActiveTarget is PlayerTarget playerTarget) ? playerTarget.Player : null);
		BasePlayer basePlayer = (_autoPursue ? FindClosestPlayerTarget(ignore) : null);
		if ((Object)(object)basePlayer != (Object)null)
		{
			PursuePlayer(basePlayer, Time.time);
		}
		else
		{
			ClearCurrentTarget();
		}
	}

	public override void Save(SaveInfo info)
	{
		if (!isClient && (!info.forDisk || LoadMode != AILoadMode.KillAi))
		{
			base.Save(info);
		}
	}

	public void OnTargetClaimAvailable(BasePlayer ply)
	{
		if (ply.EntityValid() && ActiveTarget is PlayerTarget playerTarget && ply.userID.Get() == playerTarget.Player.userID.Get())
		{
			PursuePlayer(ply, playerTarget.LastSeen);
		}
	}

	public void OnGroupChanged(int groupId)
	{
		GroupId = groupId;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (_autoInit)
		{
			SetupAI(_boat);
		}
	}

	public override void OnKilled()
	{
		RemoveAI();
		base.OnKilled();
	}

	private void OnAttached()
	{
		if (_autoFillWithScientists)
		{
			((MonoBehaviour)this).StartCoroutine(SpawnAllScientists());
		}
	}

	private void RefreshSleeping()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		_isSleeping = allow_sleeping && ActiveTarget == null && TimeSince.op_Implicit(_timeSinceSleepy) > seconds_until_sleep;
		if (_isSleeping && Random.Range(0f, 1f) < 0.01f)
		{
			_isSleeping = false;
			SwitchState(_wanderState);
			_timeSinceSleepy = TimeSince.op_Implicit(0f);
		}
	}

	private void NightCheck()
	{
		bool flag = (Object)(object)TOD_Sky.Instance != (Object)null && (TOD_Sky.Instance.Cycle.Hour > 19f || TOD_Sky.Instance.Cycle.Hour < 8f);
		if (_boat.HasFlag(Flags.Reserved5) != flag)
		{
			using (FlagsUpdateScope flagsUpdateScope = _boat.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved5, flag);
			}
		}
	}

	private void RemoveAI()
	{
		if (!((Object)(object)_boat == (Object)null))
		{
			StopEngine(Boat);
			LoadMode = AILoadMode.KillAi;
			if (ActiveTarget != null)
			{
				ClearCurrentTarget();
			}
			ExitState();
			_boat.RemoveAIDriver(runCallbacks: false);
			_boat.BeenAttacked -= BoatAttacked;
			BoatAICoordination.Unregister(this);
			if (InGroup)
			{
				BoatAICoordination.RemoveFromGroup(this, GroupId);
			}
			EnableScientistBrains();
			Boat = null;
			Kill(DestroyMode.None, callOnKilled: false);
		}
	}

	public override void PostServerLoad()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		base.PostServerLoad();
		BaseBoat baseBoat = BaseNetworkable.serverEntities.Find(BoatID) as BaseBoat;
		if (!((Object)(object)baseBoat != (Object)null))
		{
			return;
		}
		if (LoadMode == AILoadMode.LoadAi)
		{
			SetupAI(baseBoat);
		}
		if (LoadMode == AILoadMode.KillAi)
		{
			KillAllRemainingScientists(skipLoot: true);
			Kill();
		}
		if (LoadMode == AILoadMode.KillBoat)
		{
			if (baseBoat is RHIB rHIB)
			{
				rHIB.AdminKillNoLoot(killMountedNPCs: false);
			}
			else
			{
				baseBoat.Kill();
			}
			KillAllRemainingScientists(skipLoot: true);
			Kill();
		}
	}

	public void OnAdd(BaseVehicle vehicle)
	{
		SetupAI(vehicle as BaseBoat);
	}

	public void OnTick(BaseVehicle vehicle, float delta, ref float steering, ref float gasPedal)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_043d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_049b: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_boat == (Object)null)
		{
			return;
		}
		using (TimeWarning.New("BoatAI.OnTick"))
		{
			if (IsScientistDriverDead())
			{
				gasPedal = 0f;
				if (!TryReplaceDriver())
				{
					RemoveAI();
				}
				return;
			}
			gasPedal = 0f;
			if (TimeSince.op_Implicit(_timeSinceSpawned) < 5f || ((_isSleeping || !BaseNetworkable.HasCloseConnections(((Component)this).transform.position, enable_mount_sync_distance)) && !HasValidPursuit()) || !AI.move)
			{
				return;
			}
			ResetContext(_bufferContext);
			RunGlobalMethods();
			if (!Boat.IsOn())
			{
				StartEngine(Boat);
			}
			if (ActiveTarget != null && !ActiveTarget.Position.HasValue)
			{
				DropTarget();
			}
			using (TimeWarning.New("BoatAI.OnTick.CurrentState.Update"))
			{
				_currentState?.Update(_bufferContext, this, delta);
				DetourRoundBlockages(_bufferContext, delta);
				Vector3 bestContextDirection = GetBestContextDirection();
				Drive(bestContextDirection, delta, ref steering, ref gasPedal);
				if (!DRAW_DEBUGS)
				{
					return;
				}
				UnityEngine.DDraw.BroadcastText(((Component)this).transform.position + Vector3.up * 5f, _currentState?.GetStateName() ?? "No State", Color.white, 0.05f, distanceFade: true, zTest: true);
				UnityEngine.DDraw.BroadcastText(((Component)this).transform.position + Vector3.up * 3.5f, $"Speed {gasPedal}", Color.white, 0.05f, distanceFade: true, zTest: true);
				UnityEngine.DDraw.BroadcastText(((Component)this).transform.position + Vector3.up * 0f, $"GROUP {GroupId}", Color.white, 0.05f, distanceFade: true, zTest: true);
				UnityEngine.DDraw.BroadcastLine(((Component)_boat).transform.position, ((Component)_boat).transform.position + bestContextDirection, Color.green, 0.05f, distanceFade: false, zTest: false);
				UnityEngine.DDraw.BroadcastSphere(((Component)_boat).transform.position + bestContextDirection, 0.5f, Color.green, 0.05f, distanceFade: false, zTest: false);
				if (ActiveTarget != null && ActiveTarget.IsValid(this))
				{
					Vector3 val = ActiveTarget.Position.Value - ((Component)_boat).transform.position;
					Vector3 normalized = val.normalized;
					normalized.y = 0f;
					UnityEngine.DDraw.BroadcastLine(((Component)_boat).transform.position, ((Component)_boat).transform.position + normalized * 10f, Color.magenta, 0.05f, distanceFade: false, zTest: false);
					UnityEngine.DDraw.BroadcastLine(((Component)_boat).transform.position, ActiveTarget.Position.Value, Color.white, 0.05f, distanceFade: false);
				}
				if (ActiveTarget != null && ActiveTarget.IsValid(this) && ActiveTarget.Position.HasValue)
				{
					if (ActiveTarget is PlayerTarget playerTarget)
					{
						UnityEngine.DDraw.BroadcastText(((Component)this).transform.position + Vector3.up * 2f, $"STAY CLOSE {playerTarget.StayClose}", Color.white, 0.05f, distanceFade: true, zTest: true);
					}
					UnityEngine.DDraw.BroadcastSphere(ActiveTarget.Position.Value, 0.5f, Color.magenta, 0.05f, distanceFade: false, zTest: false);
				}
				UnityEngine.DDraw.BroadcastLine(((Component)_boat).transform.position, ((Component)_boat).transform.position + bestContextDirection * 5f, Color.magenta, 0.05f, distanceFade: false, zTest: false);
				UnityEngine.DDraw.BroadcastSphere(((Component)_boat).transform.position + bestContextDirection * 5f, 0.5f, Color.magenta, 0.05f, distanceFade: false, zTest: false);
				for (int i = 0; i < _contextMap.Count; i++)
				{
					float num = _bufferContext.DangerMap[i];
					float num2 = _bufferContext.InterestMap[i];
					UnityEngine.DDraw.BroadcastText(((Component)_boat).transform.position + _contextMap[i] * 5f, string.Format("INDEX {0}, Dgr {1}, Int {2}", i, num.ToString("F2"), num2.ToString("F2")), Color.black, 0.05f, distanceFade: false);
				}
			}
		}
	}

	private void Drive(Vector3 wantedDirection, float delta, ref float steering, ref float gasPedal)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)_boat).transform;
		float num = ((ActiveTarget != null) ? Vector3Ex.Distance2D(transform.position, ActiveTarget.Position.Value) : float.MaxValue);
		float num2 = Vector3.Dot(_boat.rigidBody.linearVelocity, transform.forward);
		float num3 = 0f;
		float num4 = 0f;
		bool flag = false;
		if (wantedDirection != Vector3.zero)
		{
			float num5 = Vector3.SignedAngle(Vector3Ex.WithY(transform.forward, 0f), wantedDirection, Vector3.up);
			float clearance = GetClearance(transform.forward);
			num3 = GetHelm(num5);
			float num6 = Mathf.InverseLerp(150f, 30f, Mathf.Abs(num5));
			float num7 = Mathf.InverseLerp(10f, 60f, clearance);
			float num8 = ((ActiveTarget is PlayerTarget) ? Mathf.InverseLerp(5f, 40f, num) : 1f);
			num4 = max_speed_percentage * Mathf.Lerp(0.35f, 1f, Mathf.Min(num6, num7)) * num8;
			flag = clearance < 10f && Mathf.Abs(num5) > 60f;
		}
		bool flag2 = num < 5f;
		if (flag2)
		{
			num4 = ((num2 > 1f) ? (-0.6f) : 0f);
			if (num2 <= 1f && ActiveTarget is PlayerTarget playerTarget && HasLineOfSightToPlayer(playerTarget.Player) && IsCrewOutOfAngle(playerTarget.Player))
			{
				Vector3 val = ((Component)playerTarget.Player).transform.position - transform.position;
				float num9 = Mathf.Sign(Vector3.Dot(transform.forward, val));
				num4 = 0.3f * num9;
				num3 = GetHelm(Vector3.SignedAngle(Vector3Ex.WithY(transform.forward, 0f), Vector3Ex.WithY(val, 0f), Vector3.up)) * num9;
			}
		}
		if (_recoverTimeLeft > 0f)
		{
			_recoverTimeLeft -= delta;
			num4 = max_speed_percentage * _recoverDirection;
			num3 *= _recoverDirection;
			if (Vector3Ex.Distance2D(transform.position, _recoverStart) > 8f)
			{
				_recoverTimeLeft = 0f;
			}
		}
		else
		{
			bool flag3 = !flag2 && Mathf.Abs(_throttle) > 0.25f && Vector3Ex.Magnitude2D(_boat.rigidBody.linearVelocity) < 1f;
			_stalledTime = (flag3 ? (_stalledTime + delta) : 0f);
			if (_stalledTime > 1.5f || (flag && !flag2))
			{
				_stalledTime = 0f;
				_recoverTimeLeft = 3f;
				_recoverDirection = 0f - Mathf.Sign(_throttle);
				_recoverStart = transform.position;
			}
		}
		_helm = Mathf.MoveTowards(_helm, num3, 2f * delta);
		_throttle = Mathf.MoveTowards(_throttle, num4, 1.5f * delta);
		steering = ((Mathf.Abs(_helm) < 0.08f) ? 0f : _helm);
		gasPedal = _throttle;
	}

	public void OnRemove(BaseVehicle vehicle)
	{
		Kill();
	}

	private void RunGlobalMethods()
	{
		using (TimeWarning.New("BoatAI.GlobalMethods"))
		{
			EnsureHasFuel();
			Boat.DriverHeartbeat();
			AvoidObstacles(_bufferContext);
			if (Roaming)
			{
				AvoidShore(_bufferContext);
			}
			RunThink();
		}
	}

	private void RunThink()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (_autoPursue && TimeSince.op_Implicit(_timeSinceRetarget) >= 1f)
		{
			_timeSinceRetarget = TimeSince.op_Implicit(0f);
			RefreshTarget();
		}
		if (TimeSince.op_Implicit(_timeSinceThought) < _thinkTime)
		{
			return;
		}
		using (TimeWarning.New("BoatAI.RunThink"))
		{
			_timeSinceThought = TimeSince.op_Implicit(0f);
			if (_autoPursue && !_isLeaving && (!HasValidPursuit() || !(ActiveTarget is PlayerTarget)))
			{
				BasePlayer basePlayer = FindClosestPlayerTarget();
				if ((Object)(object)basePlayer != (Object)null)
				{
					PursuePlayer(basePlayer, Time.time);
				}
			}
		}
	}

	private void RefreshTarget()
	{
		if (!HasValidPursuit() || !(ActiveTarget is PlayerTarget playerTarget))
		{
			return;
		}
		bool flag = HasLineOfSightToPlayer(playerTarget.Player);
		if (flag)
		{
			ShareSighting(playerTarget.Player);
		}
		if (!flag || playerTarget.Player.IsWounded())
		{
			BasePlayer basePlayer = FindClosestPlayerTarget(playerTarget.Player);
			if ((Object)(object)basePlayer != (Object)null && !basePlayer.IsWounded())
			{
				PursuePlayer(basePlayer, Time.time);
			}
		}
	}

	private void EnsureHasFuel()
	{
		IFuelSystem fuelSystem = Boat.GetFuelSystem();
		int fuelAmount = fuelSystem.GetFuelAmount();
		if (fuelAmount < 50)
		{
			int amount = 50 - fuelAmount;
			fuelSystem.AddFuel(amount);
		}
	}

	private void PursuePlayer(BasePlayer ply, float lastSeen, bool applyToGroup = true)
	{
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ply == (Object)null)
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning((object)(((Object)this).name + ": PursuePlayer called with null player"));
			}
		}
		else if ((Object)(object)_boat == (Object)null)
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning((object)(((Object)this).name + ": PursuePlayer called before Setup() \ufffd _boat is null"));
			}
		}
		else if (_seekState == null)
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning((object)(((Object)this).name + ": PursuePlayer called before SetupStateCache()"));
			}
		}
		else if (!IsPlayerTargetValid(ply))
		{
			if (PRINT_DEBUGS)
			{
				Debug.Log((object)"Player isnt valid");
			}
		}
		else if (!IsPlayerInRange(ply, _searchRange))
		{
			if (PRINT_DEBUGS)
			{
				Debug.Log((object)"Player isnt in range");
			}
		}
		else
		{
			if (Time.time - lastSeen > 45f)
			{
				return;
			}
			if (ActiveTarget is PlayerTarget playerTarget)
			{
				BoatAICoordination.ReleaseClaim(this, playerTarget.Player);
			}
			bool stayClose = BoatAICoordination.TryClaimTarget(this, ply);
			float lastSeen2 = ((ActiveTarget is PlayerTarget playerTarget2 && (Object)(object)playerTarget2.Player == (Object)(object)ply) ? Mathf.Max(lastSeen, playerTarget2.LastSeen) : lastSeen);
			PlayerTarget playerTarget3 = new PlayerTarget(ply, Time.time, ((Component)_boat).transform)
			{
				StayClose = stayClose,
				LastSeen = lastSeen2
			};
			SetMoveCommand(playerTarget3);
			if ((Object)(object)ply != (Object)(object)_chasedPlayer || Time.time - PursuitTargetAcquireTime > 180f)
			{
				_chasedPlayer = ply;
				_chaseStartPosition = ((Component)_boat).transform.position;
				PursuitTargetAcquireTime = Time.time;
			}
			if (!(InGroup & applyToGroup))
			{
				return;
			}
			ListHashSet<BoatAI> groupMembers = BoatAICoordination.GetGroupMembers(GroupId);
			if (groupMembers == null)
			{
				return;
			}
			Enumerator<BoatAI> enumerator = groupMembers.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					BoatAI current = enumerator.Current;
					if (!((Object)(object)current == (Object)null) && !((Object)(object)current == (Object)(object)this) && (!(current.ActiveTarget is PlayerTarget playerTarget4) || !current.HasLineOfSightToPlayer(playerTarget4.Player)))
					{
						current.PursuePlayer(ply, playerTarget3.LastSeen, applyToGroup: false);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private BasePlayer FindClosestPlayerTarget(BasePlayer ignore = null)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BoatAI.FindTarget"))
		{
			PooledList<BasePlayer> val = Pool.Get<PooledList<BasePlayer>>();
			try
			{
				Query.Server.GetPlayersInSphere(((Component)_boat).transform.position, _searchRange, (List<BasePlayer>)(object)val);
				BasePlayer result = null;
				float num = float.MaxValue;
				foreach (BasePlayer item in (List<BasePlayer>)(object)val)
				{
					if (!((Object)(object)item == (Object)(object)ignore) && IsPlayerTargetValid(item) && IsPlayerInRange(item, _searchRange) && !(((Component)item).transform.position.y < -5f) && (!Roaming || IsThreat(item)) && !BoatAICoordination.IsTargetClaimedByAnotherGroup(this, item) && HasLineOfSightToPlayer(item))
					{
						float num2 = Vector3.SqrMagnitude(((Component)item).transform.position - ((Component)_boat).transform.position);
						if (item.IsWounded())
						{
							num2 += _searchRange * _searchRange;
						}
						if (num2 < num)
						{
							num = num2;
							result = item;
						}
					}
				}
				return result;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	private void BoatAttacked(HitInfo info)
	{
		if (!((Object)(object)info.InitiatorPlayer != (Object)null))
		{
			return;
		}
		BasePlayer initiatorPlayer = info.InitiatorPlayer;
		ulong key = initiatorPlayer.userID.Get();
		_ignoredUntil.Remove(key);
		ListHashSet<BoatAI> val = (InGroup ? BoatAICoordination.GetGroupMembers(GroupId) : null);
		if (val != null)
		{
			for (int i = 0; i < val.Count; i++)
			{
				if ((Object)(object)val[i] != (Object)null)
				{
					val[i]._ignoredUntil.Remove(key);
				}
			}
		}
		if (IsPlayerTargetValid(initiatorPlayer))
		{
			PursuePlayer(info.InitiatorPlayer, Time.time);
		}
	}

	private void BoatDied()
	{
		if (_hasSpawnedScientists)
		{
			KillAllRemainingScientists();
		}
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 0:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: GroupId for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_GroupId);
			return true;
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: BoatID for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite<NetworkableId>(writer, __sync_BoatID);
			return true;
		case 2:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: LoadModeSync for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_LoadModeSync);
			return true;
		default:
			return base.WriteSyncVar(id, writer);
		}
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 0:
			try
			{
				_ = __sync_GroupId;
				int _sync_GroupId = reader.Int32();
				__sync_GroupId = _sync_GroupId;
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_BoatID;
				NetworkableId _sync_BoatID = reader.EntityID();
				__sync_BoatID = _sync_BoatID;
			}
			catch (Exception ex3)
			{
				Debug.LogException(ex3);
			}
			return true;
		case 2:
			try
			{
				_ = __sync_LoadModeSync;
				int _sync_LoadModeSync = reader.Int32();
				__sync_LoadModeSync = _sync_LoadModeSync;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		default:
			return base.OnSyncVar(id, reader, fromAutoSave);
		}
	}

	private byte __GetWeaverID(string propertyName)
	{
		return propertyName switch
		{
			"GroupId" => (byte)0, 
			"BoatID" => (byte)1, 
			"LoadModeSync" => (byte)2, 
			_ => byte.MaxValue, 
		};
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
		WriteSyncVar(1, writer);
		WriteSyncVar(2, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
		OnSyncVar(1, reader, fromAutoSave: true);
		OnSyncVar(2, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		WriteAutoSaveSyncVars(netWrite);
		(byte[] Buffer, int Length) buffer = netWrite.GetBuffer();
		byte[] item = buffer.Buffer;
		int item2 = buffer.Length;
		byte[] array = _autosaveBuffer;
		if (array == null || array.Length < item2)
		{
			byte[] array2 = BaseEntity._autosaveBufferPool.Rent(item2);
			while (array == null || array.Length < item2)
			{
				byte[] array3 = Interlocked.CompareExchange(ref _autosaveBuffer, array2, array);
				if (array3 == array)
				{
					if (array3 != null)
					{
						BaseEntity._autosaveBufferPool.Return(array3);
					}
					array = array2;
					break;
				}
				array = array3;
			}
			if (array != array2)
			{
				BaseEntity._autosaveBufferPool.Return(array2);
			}
		}
		Buffer.BlockCopy(item, 0, array, 0, item2);
		save.msg.baseEntity.syncVars = array;
		Pool.Free<NetWrite>(ref netWrite);
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		if (load.msg.baseEntity != null && load.msg.baseEntity.syncVars != null)
		{
			NetRead netRead = Pool.Get<NetRead>();
			netRead.Init(load.msg.baseEntity.syncVars.AsSpan());
			ReadAutoSaveSyncVars(netRead);
			Pool.Free<NetRead>(ref netRead);
		}
		return true;
	}

	protected override void ResetSyncVars()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		base.ResetSyncVars();
		__sync_GroupId = 0;
		__sync_BoatID = default;
		__sync_LoadModeSync = 0;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			0 => true, 
			1 => true, 
			2 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}
}
